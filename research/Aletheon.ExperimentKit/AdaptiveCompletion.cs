namespace Aletheon.ExperimentKit;

/// <summary>
/// Shared budget-aware completion retry policy. A truncated response is preserved as an
/// independent paid attempt; retrying starts a fresh completion with a larger token cap.
/// </summary>
public static class AdaptiveCompletion
{
    public sealed record Attempt(int MaxTokens, FallbackCompletion Completion);
    public sealed record Outcome(FallbackCompletion Final, IReadOnlyList<Attempt> History);

    public static async Task<Outcome> CompleteAsync(
        OpenRouterTransport transport, BudgetLedger budget, ModelCatalog catalog,
        string label, string model, string system, string? context, string prompt,
        int initialMaxTokens, int maxRetries = 2, int maxTokensCeiling = 12000,
        Action<Attempt>? onAttempt = null, CancellationToken cancellation = default)
    {
        if (initialMaxTokens < 1 || maxRetries < 0 || maxTokensCeiling < initialMaxTokens)
            throw new ArgumentOutOfRangeException(nameof(initialMaxTokens));
        var history = new List<Attempt>();
        var limit = initialMaxTokens;
        for (var i = 0; ; i++)
        {
            var result = await ModelFallback.CompleteAsync(
                transport, budget, catalog, label + "-retry" + i, model,
                Array.Empty<string>(), system, context, prompt, limit,
                cancellation: cancellation);
            var attempt = new Attempt(limit, result);
            history.Add(attempt);
            onAttempt?.Invoke(attempt);
            if (result.Result.Assessment?.Status != "truncated" ||
                i >= maxRetries || limit >= maxTokensCeiling)
                return new(result, history);
            var next = Math.Min(maxTokensCeiling, Math.Min(catalog.Get(model).MaxCompletionTokens, limit * 2));
            if (next <= limit) return new(result, history);
            limit = next;
        }
    }
}
