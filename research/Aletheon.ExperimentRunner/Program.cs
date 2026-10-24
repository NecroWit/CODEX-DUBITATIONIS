using System.Text.Json;
using Aletheon.ExperimentKit;

var runner = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
var repo = Path.GetFullPath(Path.Combine(runner, "../.."));
var kit = Path.Combine(repo, "research", "Aletheon.ExperimentKit");
var smoke = args.Contains("--smoke", StringComparer.OrdinalIgnoreCase);
var run = args.Contains("--run", StringComparer.OrdinalIgnoreCase);
if (smoke && run) { Console.Error.WriteLine("Choose either --smoke or --run."); return 2; }
if (args.Any(arg => arg != "--smoke" && arg != "--run"))
{ Console.Error.WriteLine("Unknown argument. Supported: --smoke, --run; no argument defaults to --run."); return 2; }
// No arguments run the full experiment; --smoke checks connectivity.
run = !smoke;

var coreBytes = File.ReadAllBytes(Path.Combine(repo, "ALETHEON_BASE_CORE.md"));
var protocolBytes = File.ReadAllBytes(Path.Combine(runner, "protocol.json"));
var catalogBytes = File.ReadAllBytes(Path.Combine(kit, "models.json"));
var core = System.Text.Encoding.UTF8.GetString(coreBytes);
var catalog = ModelCatalog.Load(Path.Combine(kit, "models.json"));
using var protocol = JsonDocument.Parse(protocolBytes);
var root = protocol.RootElement;
var experimentId = root.GetProperty("id").GetString() ?? throw new InvalidOperationException("Missing experiment id");
var models = root.GetProperty("participants").EnumerateArray()
    .Where(p => p.GetProperty("provider").GetString() == "openrouter")
    .Select(p => (Id: p.GetProperty("id").GetString()!, Model: p.GetProperty("model_id").GetString()!)).ToArray();
var tasks = root.GetProperty("tasks").EnumerateArray()
    .Select(t => (Id: t.GetProperty("id").GetString()!, Prompt: t.GetProperty("prompt").GetString()!)).ToArray();
if (models.Length != 2 || tasks.Length != 2) throw new InvalidOperationException("Expected 2 API models and 2 tasks.");
var completionTokens = root.GetProperty("max_completion_tokens").GetInt32();
var smokeTokens = root.GetProperty("smoke_completion_tokens").GetInt32();
var totalUsd = root.GetProperty("total_budget_usd").GetDecimal();
var perRequestUsd = root.GetProperty("per_request_budget_usd").GetDecimal();
var limits = root.GetProperty("limits");
var maxPromptChars = limits.GetProperty("max_prompt_characters").GetInt32();
var technicalLines = limits.GetProperty("technical_max_nonempty_lines").GetInt32();
var reasoningWords = limits.GetProperty("reasoning_max_words").GetInt32();
foreach (var m in models) { var info = catalog.Get(m.Model); if (completionTokens > info.MaxCompletionTokens) throw new InvalidOperationException("Catalog cap too low: " + m.Model); }
foreach (var task in tasks) if (task.Prompt.Length > maxPromptChars) throw new InvalidOperationException("Task too long: " + task.Id);
var key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
if (string.IsNullOrWhiteSpace(key)) { Console.Error.WriteLine("Set OPENROUTER_API_KEY"); return 2; }
var runId = DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssZ") + "-" + Guid.NewGuid().ToString("N")[..8];
var output = Path.Combine(runner, "runs", runId);
Directory.CreateDirectory(output);
ExperimentArtifacts.Save(Path.Combine(output, "manifest.json"), new {
    runId, startedUtc = DateTimeOffset.UtcNow, status = "calibration", experimentId,
    coreSha256 = ExperimentArtifacts.Hash(coreBytes),
    protocolSha256 = ExperimentArtifacts.Hash(protocolBytes),
    catalogSha256 = ExperimentArtifacts.Hash(catalogBytes),
    maxCompletionTokens = smoke ? smokeTokens : completionTokens,
    totalBudgetUsd = totalUsd, perRequestBudgetUsd = perRequestUsd,
    note = "Budget is approximate. Set OpenRouter key spending limit."
});
File.WriteAllBytes(Path.Combine(output, "protocol.json"), protocolBytes);
using var transport = new OpenRouterTransport(key);
var budget = new BudgetLedger(Path.Combine(output, "cost-ledger.jsonl"), totalUsd, perRequestUsd);
var jobs = smoke ? new[] { (models[0], (Id: "SMOKE", Prompt: "Reply exactly OK.")) } :
    (from model in models from task in tasks select (model, task)).ToArray();
Console.WriteLine($"{experimentId}: {jobs.Length} independent concurrent requests. Output: {output}");
var results = await Task.WhenAll(jobs.Select(async job =>
{
    var (model, task) = job;
    var label = model.Id + "-" + task.Id;
    var cap = smoke ? smokeTokens : completionTokens;
    var info = catalog.Get(model.Model);
    if (cap > info.MaxCompletionTokens)
        throw new InvalidOperationException($"Token limit exceeds catalog limit for {model.Model}");
    var reserved = budget.Reserve(label, info, core, task.Prompt, cap);
    if (reserved is null)
    {
        ExperimentArtifacts.Save(Path.Combine(output, label + ".json"), new { label, model = model.Model, status = "budget_blocked" });
        return false;
    }
    var result = await transport.CompleteAsync(model.Model, core, task.Prompt, cap);
    budget.Reconcile(label, info, reserved.Value, result.Assessment?.CostUsd);
    ExperimentArtifacts.Save(Path.Combine(output, label + ".json"), new {
        label, requestedModel = model.Model, result.Complete, result.Error,
        result.HttpAttempts, result.Seconds, result.Assessment,
        result.Raw // unmodified provider JSON as a string; never parsed as a final answer
    });
    if (result.Raw is not null)
        File.WriteAllText(Path.Combine(output, label + ".response.json"), result.Raw);
    var withinLength = false;
    if (result.Raw is not null && result.Complete)
    {
        using var responseJson = JsonDocument.Parse(result.Raw);
        var message = responseJson.RootElement.GetProperty("choices")[0].GetProperty("message");
        var answer = message.GetProperty("content").GetString() ?? "";
        withinLength = task.Id == "H1"
            ? answer.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length <= reasoningWords
            : answer.Replace("```python", "").Replace("```", "").Split('\n').Count(line => !string.IsNullOrWhiteSpace(line)) <= technicalLines;
    }
    var accepted = result.Complete && withinLength;
    Console.WriteLine($"{label}: {(accepted ? "ACCEPTED" : "INCOMPLETE/OVERLONG")} ({result.Assessment?.Status ?? result.Error})");
    return accepted;
}));
Console.WriteLine($"Complete: {results.Count(x => x)}/{results.Length}. No model substitutions.");
return results.All(x => x) ? 0 : 1;
