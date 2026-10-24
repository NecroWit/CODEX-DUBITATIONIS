using System.Text.Json;
using Aletheon.ExperimentKit;

var experiment = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
var repo = Path.GetFullPath(Path.Combine(experiment, "../../../../../"));
var kit = Path.Combine(repo, "research", "Aletheon.ExperimentKit");
var smoke = args.Contains("--smoke", StringComparer.OrdinalIgnoreCase);
var run = args.Contains("--run", StringComparer.OrdinalIgnoreCase);
if (smoke == run) { Console.Error.WriteLine("Use --smoke or --run"); return 2; }
var key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
if (string.IsNullOrWhiteSpace(key)) { Console.Error.WriteLine("Set OPENROUTER_API_KEY"); return 2; }
var coreBytes = File.ReadAllBytes(Path.Combine(repo, "ALETHEON_BASE_CORE.md"));
var questionsBytes = File.ReadAllBytes(Path.Combine(experiment, "questions.json"));
var peopleBytes = File.ReadAllBytes(Path.Combine(experiment, "participants.json"));
var catalogBytes = File.ReadAllBytes(Path.Combine(kit, "models.json"));
var core = System.Text.Encoding.UTF8.GetString(coreBytes);
var catalog = ModelCatalog.Load(Path.Combine(kit, "models.json"));
using var people = JsonDocument.Parse(peopleBytes);
using var questions = JsonDocument.Parse(questionsBytes);
var models = people.RootElement.GetProperty("participants").EnumerateArray()
    .Where(p => p.GetProperty("provider").GetString() == "openrouter")
    .Select(p => (Id: p.GetProperty("id").GetString()!, Model: p.GetProperty("model_id").GetString()!)).ToArray();
var tasks = questions.RootElement.GetProperty("tasks").EnumerateArray()
    .Select(t => (Id: t.GetProperty("id").GetString()!, Prompt: t.GetProperty("prompt").GetString()!)).ToArray();
if (models.Length != 2 || tasks.Length != 2) throw new InvalidOperationException("Expected 2 API models and 2 tasks.");
const int completionTokens = 12000; // Includes internal reasoning; deliberately higher than failed 2500.
const decimal totalUsd = 1m, perRequestUsd = 0.40m;
var runId = DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssZ") + "-" + Guid.NewGuid().ToString("N")[..8];
var output = Path.Combine(experiment, "runs", runId);
Directory.CreateDirectory(output);
ExperimentArtifacts.Save(Path.Combine(output, "manifest.json"), new {
    runId, startedUtc = DateTimeOffset.UtcNow, status = "new_trial",
    excludedPriorTrial = "20261024T164028Z-a86f4b17",
    coreSha256 = ExperimentArtifacts.Hash(coreBytes),
    questionsSha256 = ExperimentArtifacts.Hash(questionsBytes),
    participantsSha256 = ExperimentArtifacts.Hash(peopleBytes),
    catalogSha256 = ExperimentArtifacts.Hash(catalogBytes),
    maxCompletionTokens = smoke ? 100 : completionTokens,
    totalBudgetUsd = totalUsd, perRequestBudgetUsd = perRequestUsd,
    note = "Budget is approximate. Set OpenRouter key spending limit."
});
using var transport = new OpenRouterTransport(key);
var budget = new BudgetLedger(Path.Combine(output, "cost-ledger.jsonl"), totalUsd, perRequestUsd);
var jobs = smoke ? new[] { (models[0], (Id: "SMOKE", Prompt: "Reply exactly OK.")) } :
    (from model in models from task in tasks select (model, task)).ToArray();
Console.WriteLine($"EXP-001: {jobs.Length} independent concurrent requests. Output: {output}");
var results = await Task.WhenAll(jobs.Select(async job =>
{
    var (model, task) = job;
    var label = model.Id + "-" + task.Id;
    var cap = smoke ? 100 : completionTokens;
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
    Console.WriteLine($"{label}: {(result.Complete ? "COMPLETE" : "INCOMPLETE")} ({result.Assessment?.Status ?? result.Error})");
    return result.Complete;
}));
Console.WriteLine($"Complete: {results.Count(x => x)}/{results.Length}. No model substitutions.");
return results.All(x => x) ? 0 : 1;
