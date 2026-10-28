using System.Text;
using System.Text.Json;
using Aletheon.ExperimentKit;

// Resolve the repository by the actual experiment protocol location, not by a
// Core file: the runner directory may contain its own copy of ALETHEON_BASE_CORE.md.
var dir = new DirectoryInfo(AppContext.BaseDirectory);
const string relativeProtocol = "research/Aletheon.ExperimentRunner/exp3/EXP-3.0.protocol.json";
while (dir is not null && !File.Exists(Path.Combine(dir.FullName, relativeProtocol)))
    dir = dir.Parent;
var repo = dir?.FullName ?? throw new FileNotFoundException("Cannot locate EXP-3 source protocol.");
var expDir = Path.Combine(repo, "research", "Aletheon.ExperimentRunner", "exp3");
var runsRoot = Path.Combine(repo, "research", "Aletheon.ExperimentRunner", "runs");
var protocolPath = Path.Combine(expDir, "protocol.json");
var resetOnly = args.Length == 1 && args[0] == "--reset";
var smokeOnly = args.Length == 1 && args[0].Equals("--smoke", StringComparison.OrdinalIgnoreCase);
var prepareOnly = args.Length == 1 && args[0].Equals("--prepare", StringComparison.OrdinalIgnoreCase);
if (args.Length > 1 || (args.Length == 1 && !new[] { "--smoke", "--prepare", "--run", "--reset" }.Contains(args[0], StringComparer.OrdinalIgnoreCase)))
{
    Console.Error.WriteLine("Usage: --prepare (offline), --smoke (offline), --run (paid). No arguments means --run.");
    return 2;
}
if (resetOnly)
{
    Directory.CreateDirectory(runsRoot);
    var archive = Path.Combine(runsRoot, "exp3-archive");
    Directory.CreateDirectory(archive);
    foreach (var old in Directory.GetDirectories(runsRoot, "EXP-3.*"))
    {
        var dest = Path.Combine(archive, Path.GetFileName(old));
        if (Directory.Exists(dest))
            dest += "-" + Guid.NewGuid().ToString("N")[..8];
        Directory.Move(old, dest);
    }
    var state = Path.Combine(runsRoot, "exp3-rotation-state.json");
    if (File.Exists(state))
        File.Move(state, Path.Combine(archive, "rotation-" + Guid.NewGuid().ToString("N") + ".json"));
    File.Copy(Path.Combine(expDir, "EXP-3.0.protocol.json"), protocolPath, true);
    Console.WriteLine("EXP-3.0 restored; previous results archived; no paid requests.");
    return 0;
}
if (!smokeOnly)
{
    Directory.CreateDirectory(runsRoot);
    var labels = new[] { "N-H1", "N-H2", "N-H3", "N-H4", "C-H1", "C-H2", "C-H3", "C-H4" };
    var latest = ExperimentSequence.Inspect(runsRoot, 3, labels);
    if (latest is null)
    {
        // Preserve an already prepared protocol.
        if (!File.Exists(protocolPath))
            File.Copy(Path.Combine(expDir, "EXP-3.0.protocol.json"), protocolPath);
    }
    else
    {
        var archived = ExperimentSequence.RequireArchivedProtocol(latest);
        string? activeId = null;
        if (File.Exists(protocolPath))
        {
            using var active = JsonDocument.Parse(File.ReadAllText(protocolPath));
            activeId = active.RootElement.GetProperty("id").GetString();
        }
        var nextId = ExperimentSequence.NextId(latest, 3);
        var sourcePathsForGuard = new[] { "ALETHEON_BOOTSTRAP.md", "ALETHEON_CONTEXT_INDEX.yaml",
            "ALETHEON_BASE_CORE.md", "ALETHEON_EXTENDED_CORE.md", "ALETHEON_EVOLUTION.md",
            "ALETHEON_RESEARCH_LOG.md", "CODEX-DUBITATIONIS.md" };
        var currentHashesForGuard = sourcePathsForGuard.ToDictionary(
            p => p, p => ExperimentArtifacts.Hash(File.ReadAllBytes(Path.Combine(repo, p))));
        var changedSource = Exp3SourceGuard.FirstChangedSource(runsRoot, latest.Id, currentHashesForGuard);
        if (changedSource is not null)
        {
            // Preserve old runs as historical evidence: a changed source is a new experimental stage.
            if (activeId != nextId)
            {
                File.Copy(archived, protocolPath, true);
                Exp3ModelRotation.Prepare(repo, protocolPath, runsRoot, nextId);
            }
            Console.WriteLine($"{latest.Id}: source drift in {changedSource}; preserving historical attempts and moving to {nextId}.");
        }
        else if (!Exp3RunStatus.LatestAccepted(latest.Path, labels))
        {
            using var previous = JsonDocument.Parse(File.ReadAllText(archived));
            var failedModel = previous.RootElement.GetProperty("participants")[0].GetProperty("model_id").GetString();
            string? preparedModel = null;
            if (activeId == latest.Id && File.Exists(protocolPath))
            {
                using var prepared = JsonDocument.Parse(File.ReadAllText(protocolPath));
                preparedModel = prepared.RootElement.GetProperty("participants")[0].GetProperty("model_id").GetString();
            }
            if (string.IsNullOrEmpty(preparedModel) || string.Equals(preparedModel, failedModel, StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(archived, protocolPath, true);
                Exp3ModelRotation.Prepare(repo, protocolPath, runsRoot, latest.Id);
            }
            // A failed run must never silently repeat the same model, especially
            // when all eight observations were INCOMPLETE.
            using var rotatedProtocol = JsonDocument.Parse(File.ReadAllText(protocolPath));
            var rotatedModel = rotatedProtocol.RootElement.GetProperty("participants")[0].GetProperty("model_id").GetString();
            if (string.Equals(rotatedModel, failedModel, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"EXP-3 rotation failed: previous model {failedModel} was not replaced. " +
                    "Choose an eligible alternative before issuing any paid requests.");
            Console.WriteLine($"{latest.Id} is not accepted 8/8: model changed from {failedModel} to {rotatedModel}; experiment number unchanged.");
        }
        else if (activeId != nextId)
        {
            if (!Path.GetFullPath(archived).Equals(Path.GetFullPath(protocolPath), StringComparison.OrdinalIgnoreCase))
                File.Copy(archived, protocolPath, true);
            Exp3ModelRotation.Prepare(repo, protocolPath, runsRoot, nextId);
        }
    }
}
else if (!File.Exists(protocolPath))
    File.Copy(Path.Combine(expDir, "EXP-3.0.protocol.json"), protocolPath);
if (prepareOnly)
{
    Console.WriteLine("Preparation complete. No API requests sent.");
    return 0;
}
using var protocol = JsonDocument.Parse(File.ReadAllText(protocolPath));
var cfg = protocol.RootElement;
var experimentId = cfg.GetProperty("id").GetString() ?? throw new InvalidOperationException("Missing experiment ID");
var sources = new[] { "ALETHEON_BOOTSTRAP.md", "ALETHEON_CONTEXT_INDEX.yaml",
    "ALETHEON_BASE_CORE.md", "ALETHEON_EXTENDED_CORE.md", "ALETHEON_EVOLUTION.md",
    "ALETHEON_RESEARCH_LOG.md", "CODEX-DUBITATIONIS.md" };
var sourceTexts = sources.Select(p => (Path:p, Text:File.ReadAllText(Path.Combine(repo,p)))).ToArray();
var tasks = cfg.GetProperty("tasks").EnumerateArray().Select(t => (
    Id:t.GetProperty("id").GetString()!, Prompt:t.GetProperty("prompt").GetString()!,
    MaxWords:t.GetProperty("max_words").GetInt32())).ToArray();
var model = cfg.GetProperty("participants").EnumerateArray().Select(p => p.GetProperty("model_id").GetString()!).Single();
if (tasks.Length != 4 || !tasks.Select(t=>t.Id).SequenceEqual(new[]{"H1","H2","H3","H4"}))
    throw new InvalidOperationException("Expected one model and H1/H2/H3/H4.");
var context = string.Join("\n\n", sourceTexts.Select(s => $"=== {s.Path} ===\n{s.Text}"));
var system = "Отвечай на вопрос самостоятельно, аргументированно и точно. Не выдумывай ссылки на недоступные документы. Ограничение количества слов в задании — ЖЁСТКОЕ. Перед отправкой проверь объём ответа; если превышен, сократи, сохраняя смысл. Если ответ всё ещё длиннее лимита, сократи его до допустимого объёма НЕМЕДЛЕННО, иначе ответ будет ОТКЛОНЁН. Не добавляй текст сверх лимита.";
var maxTokens = cfg.GetProperty("max_completion_tokens").GetInt32();
var catalog = ModelCatalog.Load(Path.Combine(repo,"research/Aletheon.ExperimentKit/models.json"));
if(catalog.Get(model).MaxCompletionTokens < maxTokens)
    throw new InvalidOperationException($"Model cap too low: {model}");
var maxInput = cfg.GetProperty("max_total_input_chars").GetInt32();
if(context.Length > maxInput) throw new InvalidOperationException($"Source context exceeds limit: {context.Length}/{maxInput}");
var sourceHashes = sourceTexts.ToDictionary(s=>s.Path,s=>ExperimentArtifacts.Hash(Encoding.UTF8.GetBytes(s.Text)));
Exp3SourceGuard.Verify(runsRoot, experimentId, model, sourceHashes);
// Match the existing ExperimentRunner: no arguments means a normal experiment run.
var argsSet = (args.Length == 0 ? new[] { "--run" } : args).ToHashSet(StringComparer.OrdinalIgnoreCase);
if (argsSet.SetEquals(new[]{"--smoke"})) {
    Console.WriteLine(experimentId + " smoke OK. Models: " + model);
    Console.WriteLine($"Sources: {sources.Length}, context chars: {context.Length}; requests: 8");
    Console.WriteLine("Hashes:\n"+string.Join("\n",sourceHashes.Select(k=>$"{k.Key}: {k.Value}")));
    return 0;
}
if(!argsSet.SetEquals(new[]{"--run"})) {
    Console.Error.WriteLine("Usage: dotnet run --project research/Aletheon.ExperimentRunner/exp3/Aletheon.Exp3.csproj -- --smoke|--run");
    return 2;
}
var key=Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
if(string.IsNullOrWhiteSpace(key)) { Console.Error.WriteLine("OPENROUTER_API_KEY missing."); return 2; }
Console.WriteLine(experimentId + ": starting up to 8 paid requests; same model in both conditions; no fallback within a run. Check provider-side spending limit.");
var runId=DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssZ")+"-"+Guid.NewGuid().ToString("N")[..8];
var output=Path.Combine(runsRoot,experimentId,runId);
Directory.CreateDirectory(output);
void Save(string name,object value)=>ExperimentArtifacts.Save(Path.Combine(output,name),value);
Save("manifest.json",new { experiment=experimentId,runId,startedUtc=DateTimeOffset.UtcNow,
    model,sourceHashes,protocolSha256=ExperimentArtifacts.Hash(File.ReadAllBytes(protocolPath)),
    requests=8, conditions=new[]{"N:no_context","C:full_context"}, note="Independent calls; no R0; same model in both arms; semantic evaluation manual." });
File.Copy(protocolPath,Path.Combine(output,"protocol.json"));
using var tierConfig = JsonDocument.Parse(File.ReadAllText(Path.Combine(repo, "experiments", "agents.mixed.json")));
var tierPools = tierConfig.RootElement.GetProperty("mixedTiers");
var tierBudgets = tierConfig.RootElement.GetProperty("mixedTierBudgetsUsd");
var groups = tierPools.EnumerateObject().Where(g => g.Value.EnumerateArray()
    .Any(x => x.GetString() == model)).Select(g => g.Name).ToArray();
if (groups.Length != 1) throw new InvalidOperationException("Model must belong to exactly one pricing tier: " + model);
var configuredCap = tierBudgets.GetProperty(groups[0]).GetDecimal();
var requestCap = Exp3BudgetPolicy.RequiredRequestCap(catalog.Get(model), system,
    context, tasks.Select(t => t.Prompt), maxTokens, configuredCap);
var totalCap = cfg.GetProperty("budget_usd_max").GetDecimal();
if (requestCap > totalCap)
    throw new InvalidOperationException($"EXP-3 reservation {requestCap} USD exceeds total budget {totalCap} USD.");
Console.WriteLine($"EXP-3 budget: request reservation cap {requestCap} USD, total cap {totalCap} USD (unchanged).");
var budget = new BudgetLedger(Path.Combine(output,"cost-ledger.jsonl"),totalCap,requestCap,
    new Dictionary<string,decimal> { [model] = requestCap });
using var transport = new OpenRouterTransport(key);
var failures = 0;
foreach (var condition in new[] { "N", "C" })
{
    var loaded = condition == "C";
    foreach (var task in tasks)
    {
        var label = condition + "-" + task.Id;
        // Each question starts a fresh request. No answers from the other condition or previous questions.
        var completion = await ModelFallback.CompleteAsync(transport,budget,catalog,
            label,model,Array.Empty<string>(),system,loaded ? context : null,
            task.Prompt + $"\n\nЖЁСТКОЕ ОГРАНИЧЕНИЕ: не более {task.MaxWords} слов, разделённых пробелами. Если превышаешь лимит, сократи ответ до {task.MaxWords} слов или меньше НЕМЕДЛЕННО, иначе результат будет ОТКЛОНЁН. Сохрани смысл и все обязательные части ответа.",maxTokens);
        var result = completion.Result;
        Save(label+".result.json",new {requestedModel=model,actualModel=result.Assessment?.ReturnedModel,
            condition=loaded?"full_context":"no_context",completion.Substituted,completion.Attempts,
            result.Error,result.HttpAttempts,result.Seconds,result.Assessment});
        if(result.Raw is not null) File.WriteAllText(Path.Combine(output,label+".response.json"),result.Raw);
        var answer=result.Assessment?.Content;
        var count=answer?.Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries).Length;
        var matched=string.Equals(result.Assessment?.ReturnedModel,model,StringComparison.OrdinalIgnoreCase);
        var complete=result.Complete && result.Assessment?.CostUsd is not null &&
            result.Assessment.PromptTokens is not null && result.Assessment.CompletionTokens is not null &&
            !string.IsNullOrWhiteSpace(answer) && matched;
        var overlong=complete && count>task.MaxWords;
        // EXP-3 treats a provider-truncated response as a valid observation.
        // Do not silently accept transport errors, missing responses, or model substitutions.
        var truncated=result.Assessment?.Status == "truncated" &&
            result.Raw is not null && matched;
        var accepted=(complete && !overlong) || truncated;
        Save(label+".accepted.json",new {accepted,complete,overlong,wordCount=count,maxWords=task.MaxWords,
            reason=truncated?"truncated_observation":accepted?"complete":overlong?"overlong":"incomplete_or_model_mismatch"});
        Console.WriteLine(label+": "+(accepted?"ACCEPTED":overlong?"OVERLONG":"INCOMPLETE")+
            " / "+result.Assessment?.Status+(overlong?$" ({count}/{task.MaxWords} words)":""));
        if(!accepted) failures++;
    }
}
Console.WriteLine($"Saved: {output}; failures={failures}. No semantic scores assigned automatically.");
return failures==0?0:1;
