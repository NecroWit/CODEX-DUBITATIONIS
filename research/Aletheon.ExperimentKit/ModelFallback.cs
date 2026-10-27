using System.Text.Json;

namespace Aletheon.ExperimentKit;

public sealed record ModelAttempt(string Model, string? Error, int HttpAttempts, decimal? ActualCostUsd);
public sealed record FallbackCompletion(string RequestedModel, CompletionResult Result,
    IReadOnlyList<ModelAttempt> Attempts, bool BudgetBlocked)
{
    public string ActualModel => Result.Model;
    public bool Substituted => !string.Equals(RequestedModel, ActualModel, StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Shared, opt-in model fallback for independent experimental requests.
/// Candidate selection is supplied by the experiment; transport and accounting live here.
/// </summary>
public static class ModelFallback
{
    private static readonly object LogGate = new();
    public static Dictionary<string, string[]> Standbys(JsonElement pools, ModelCatalog catalog,
        IReadOnlyList<(string Id, string Model)> participants, int maxTokens)
    {
        return participants.ToDictionary(p => p.Id, p =>
        {
            var groups = pools.EnumerateObject()
                .Where(g => g.Value.EnumerateArray().Any(x => x.GetString() == p.Model))
                .Select(g => g.Name).ToArray();
            if (groups.Length != 1)
                throw new InvalidOperationException("Model must belong to one pricing tier: " + p.Model);
            return pools.GetProperty(groups[0]).EnumerateArray().Select(x => x.GetString()!)
                .Where(id => id != p.Model && !participants.Any(other => other.Model == id))
                .Where(id => catalog.All.Any(info => info.Id == id && info.MaxCompletionTokens >= maxTokens))
                .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        });
    }

    public static async Task<FallbackCompletion> CompleteAsync(
        OpenRouterTransport transport, BudgetLedger budget, ModelCatalog catalog,
        string label, string requestedModel, IEnumerable<string> standbyModels,
        string system, string? additionalContext, string prompt, int maxTokens,
        string? substitutionLogPath = null, CancellationToken cancellation = default)
    {
        var attempts = new List<ModelAttempt>();
        var candidates = new[] { requestedModel }.Concat(standbyModels)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        CompletionResult? last = null;
        bool blocked = false;
        for (var i = 0; i < candidates.Length; i++)
        {
            var model = candidates[i];
            var info = catalog.Get(model);
            if (maxTokens > info.MaxCompletionTokens) continue;
            var reservation = budget.Reserve(label + "-attempt-" + (i + 1), info,
                system + (additionalContext is null ? "" : "\n" + additionalContext), prompt, maxTokens);
            if (reservation is null)
            {
                blocked = true;
                continue;
            }
            var result = await transport.CompleteAsync(model, system, additionalContext, prompt, maxTokens, cancellation);
            // HTTP 429/404 responses do not generate completions; release their reservation.
            // Unknown failures retain their reservation conservatively.
            var fallbackError = result.Error is "HTTP 429" or "HTTP 404";
            budget.Reconcile(label + "-attempt-" + (i + 1), info, reservation.Value,
                fallbackError ? 0m : result.Assessment?.CostUsd);
            attempts.Add(new ModelAttempt(model, result.Error, result.HttpAttempts, result.Assessment?.CostUsd));
            last = result;
            if (i > 0 && substitutionLogPath is not null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(substitutionLogPath)!);
                lock (LogGate) File.AppendAllText(substitutionLogPath, JsonSerializer.Serialize(new {
                    label, requestedModel, replacementModel = model,
                    reason = attempts[i - 1].Error, result.Error, utc = DateTimeOffset.UtcNow
                }) + Environment.NewLine);
            }
            if (!fallbackError || i == candidates.Length - 1) break;
        }
        if (last is not null && last.Error is not ("HTTP 429" or "HTTP 404")) blocked = false;
        if (last is null)
            return new(requestedModel, new CompletionResult(requestedModel, null, null,
                "Budget blocked or no eligible model", 0, 0), attempts, blocked);
        return new(requestedModel, last, attempts, blocked);
    }
}
