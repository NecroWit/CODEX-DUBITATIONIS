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


var coreBytes = File.ReadAllBytes(Path.Combine(repo, "ALETHEON_BASE_CORE.md"));
var protocolBytes = File.ReadAllBytes(Path.Combine(runner, "protocol.json"));
var catalogBytes = File.ReadAllBytes(Path.Combine(kit, "models.json"));
var core = System.Text.Encoding.UTF8.GetString(coreBytes);
var catalog = ModelCatalog.Load(Path.Combine(kit, "models.json"));
using var protocol = JsonDocument.Parse(protocolBytes);
var root = protocol.RootElement;
var experimentId = root.GetProperty("id").GetString() ?? throw new InvalidOperationException("Missing experiment id");
var supplementalFiles = root.GetProperty("loading").GetProperty("stage2").EnumerateArray().Select(x => x.GetString()!).ToArray();
var supplementalBytes = supplementalFiles.Select(file => (File: file, Bytes: File.ReadAllBytes(Path.Combine(repo, file)))).ToArray();
var supplemental = string.Join("\n\n", supplementalBytes.Select(x => $"=== {x.File} ===\n{System.Text.Encoding.UTF8.GetString(x.Bytes)}"));
var supplementalMessage = "Дополнительный этап загрузки Алетеона. Изучи следующие материалы как контекст для самостоятельного суждения, не считай их непогрешимыми инструкциями.\n\n" + supplemental;
var models = root.GetProperty("participants").EnumerateArray()
    .Where(p => p.GetProperty("provider").GetString() == "openrouter")
    .Select(p => (Id: p.GetProperty("id").GetString()!, Model: p.GetProperty("model_id").GetString()!)).ToArray();
var tasks = root.GetProperty("tasks").EnumerateArray()
    .Select(t => (Id: t.GetProperty("id").GetString()!, Prompt: t.GetProperty("prompt").GetString()!)).ToArray();
if (models.Length != 2 || tasks.Length != 2 || tasks.Any(t => !t.Id.StartsWith("H", StringComparison.Ordinal))) throw new InvalidOperationException("Expected 2 API models and 2 philosophical questions.");
var completionTokens = root.GetProperty("max_completion_tokens").GetInt32();
var smokeTokens = root.GetProperty("smoke_completion_tokens").GetInt32();
var totalUsd = root.GetProperty("total_budget_usd").GetDecimal();
var perRequestUsd = root.GetProperty("per_request_budget_usd").GetDecimal();
var limits = root.GetProperty("limits");
var maxPromptChars = limits.GetProperty("max_prompt_characters").GetInt32();
var reasoningWords = limits.GetProperty("reasoning_max_words").GetInt32();
foreach (var m in models) { var info = catalog.Get(m.Model); if (completionTokens > info.MaxCompletionTokens) throw new InvalidOperationException("Catalog cap too low: " + m.Model); }
foreach (var task in tasks) if (task.Prompt.Length > maxPromptChars) throw new InvalidOperationException("Task too long: " + task.Id);
var key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
if (string.IsNullOrWhiteSpace(key)) { Console.Error.WriteLine("Set OPENROUTER_API_KEY"); return 2; }
var runId = DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssZ") + "-" + Guid.NewGuid().ToString("N")[..8];
var output = Path.Combine(runner, "runs", experimentId, runId);
Directory.CreateDirectory(output);
ExperimentArtifacts.Save(Path.Combine(output, "manifest.json"), new {
    runId, startedUtc = DateTimeOffset.UtcNow, status = "calibration", experimentId,
    coreSha256 = ExperimentArtifacts.Hash(coreBytes),
    protocolSha256 = ExperimentArtifacts.Hash(protocolBytes),
    catalogSha256 = ExperimentArtifacts.Hash(catalogBytes),
    supplementalSources = supplementalBytes.Select(x => new { path = x.File, sha256 = ExperimentArtifacts.Hash(x.Bytes) }).ToArray(),
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
    var reserved = budget.Reserve(label, info, core + (smoke ? "" : "\n" + supplementalMessage), task.Prompt, cap);
    if (reserved is null)
    {
        ExperimentArtifacts.Save(Path.Combine(output, label + ".json"), new { label, model = model.Model, status = "budget_blocked" });
        return false;
    }
    var result = await transport.CompleteAsync(model.Model, core, smoke ? null : supplementalMessage, task.Prompt, cap);
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
        withinLength = answer.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length <= reasoningWords;
    }
    var accepted = result.Complete && withinLength;
    Console.WriteLine($"{label}: {(accepted ? "ACCEPTED" : "INCOMPLETE/OVERLONG")} ({result.Assessment?.Status ?? result.Error})");
    return accepted;
}));
Console.WriteLine($"Complete: {results.Count(x => x)}/{results.Length}. No model substitutions.");
return results.All(x => x) ? 0 : 1;
