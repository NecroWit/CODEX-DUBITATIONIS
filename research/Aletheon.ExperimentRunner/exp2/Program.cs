using System.Text;
using System.Text.Json;
using Aletheon.ExperimentKit;

// Resolve the repository by the actual experiment protocol location, not by a
// Core file: the runner directory may contain its own copy of ALETHEON_BASE_CORE.md.
var dir = new DirectoryInfo(AppContext.BaseDirectory);
const string relativeProtocol = "research/Aletheon.ExperimentRunner/exp2/EXP-2.0.protocol.json";
while (dir is not null && !File.Exists(Path.Combine(dir.FullName, relativeProtocol)))
    dir = dir.Parent;
var repo = dir?.FullName ?? throw new FileNotFoundException("Cannot locate EXP-2 source protocol.");
var expDir = Path.Combine(repo, "research", "Aletheon.ExperimentRunner", "exp2");
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
    var archive = Path.Combine(runsRoot, "exp2-archive");
    Directory.CreateDirectory(archive);
    foreach (var old in Directory.GetDirectories(runsRoot, "EXP-2.*"))
    {
        var dest = Path.Combine(archive, Path.GetFileName(old));
        if (Directory.Exists(dest))
            dest += "-" + Guid.NewGuid().ToString("N")[..8];
        Directory.Move(old, dest);
    }
    var state = Path.Combine(runsRoot, "exp2-rotation-state.json");
    if (File.Exists(state))
        File.Move(state, Path.Combine(archive, "rotation-" + Guid.NewGuid().ToString("N") + ".json"));
    File.Copy(Path.Combine(expDir, "EXP-2.0.protocol.json"), protocolPath, true);
    Console.WriteLine("EXP-2.0 restored; previous results archived; no paid requests.");
    return 0;
}
if (!smokeOnly)
{
    Directory.CreateDirectory(runsRoot);
    var latest = ExperimentSequence.Inspect(runsRoot, 2,
        new[] { "A-R0", "A-H1", "A-H2", "A-H3", "B-R0", "B-H1", "B-H2", "B-H3" });
    if (latest is null)
    {
        if (!File.Exists(protocolPath))
            File.Copy(Path.Combine(expDir, "EXP-2.0.protocol.json"), protocolPath);
    }
    else
    {
        var archived = ExperimentSequence.RequireArchivedProtocol(latest);
        var reportPath = Path.Combine(expDir, latest.Id + "-REPORT.md");
        var acceptedByResearch = File.Exists(reportPath) &&
            File.ReadAllText(reportPath)
                .Contains("**Статус:** принят как успешный этап", StringComparison.Ordinal);
        if (!latest.Accepted && !acceptedByResearch)
        {
            File.Copy(archived, protocolPath, true);
            Console.WriteLine($"{latest.Id} incomplete; retrying unchanged protocol.");
        }
        else
        {
            var next = ExperimentSequence.NextId(latest, 2);
            string? currentId = null;
            if (File.Exists(protocolPath))
            {
                using var active = JsonDocument.Parse(File.ReadAllText(protocolPath));
                currentId = active.RootElement.GetProperty("id").GetString();
            }
            if (currentId != next)
            {
                File.Copy(archived, protocolPath, true);
                ModelRotation.Prepare(repo, Path.Combine(repo, "research", "Aletheon.ExperimentRunner"),
                    Path.Combine(repo, "research", "Aletheon.ExperimentKit"), targetId: next,
                    protocolOverride: protocolPath,
                    stateOverride: Path.Combine(runsRoot, "exp2-rotation-state.json"), exp2: true);
                Console.WriteLine($"{latest.Id} accepted (technical or documented research decision); prepared {next} offline.");
            }
        }
    }
}
else if (!File.Exists(protocolPath))
    File.Copy(Path.Combine(expDir, "EXP-2.0.protocol.json"), protocolPath);
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
var models = cfg.GetProperty("participants").EnumerateArray().Select(p => (
    Id:p.GetProperty("id").GetString()!, Model:p.GetProperty("model_id").GetString()!)).ToArray();
if (models.Length != 2 || tasks.Length != 3 || !tasks.Select(t=>t.Id).SequenceEqual(new[]{"H1","H2","H3"}))
    throw new InvalidOperationException("Expected two fixed models and H1/H2/H3.");
var context = string.Join("\n\n", sourceTexts.Select(s => $"=== {s.Path} ===\n{s.Text}"));
var system = sourceTexts.Single(s => s.Path == "ALETHEON_BASE_CORE.md").Text;
var maxTokens = cfg.GetProperty("max_completion_tokens").GetInt32();
var catalog = ModelCatalog.Load(Path.Combine(repo,"research/Aletheon.ExperimentKit/models.json"));
foreach(var m in models) if(catalog.Get(m.Model).MaxCompletionTokens < maxTokens)
    throw new InvalidOperationException($"Model cap too low: {m.Model}");
var maxInput = cfg.GetProperty("max_total_input_chars").GetInt32();
if(context.Length > maxInput) throw new InvalidOperationException($"Source context exceeds limit: {context.Length}/{maxInput}");
var sourceHashes = sourceTexts.ToDictionary(s=>s.Path,s=>ExperimentArtifacts.Hash(Encoding.UTF8.GetBytes(s.Text)));
// Match the existing ExperimentRunner: no arguments means a normal experiment run.
var argsSet = (args.Length == 0 ? new[] { "--run" } : args).ToHashSet(StringComparer.OrdinalIgnoreCase);
if (argsSet.SetEquals(new[]{"--smoke"})) {
    Console.WriteLine(experimentId + " smoke OK. Models: " + string.Join(", ", models.Select(m=>m.Model)));
    Console.WriteLine($"Sources: {sources.Length}, context chars: {context.Length}; requests: 8");
    Console.WriteLine("Hashes:\n"+string.Join("\n",sourceHashes.Select(k=>$"{k.Key}: {k.Value}")));
    return 0;
}
if(!argsSet.SetEquals(new[]{"--run"})) {
    Console.Error.WriteLine("Usage: dotnet run --project research/Aletheon.ExperimentRunner/exp2/Aletheon.Exp2.csproj -- --smoke|--run");
    return 2;
}
var key=Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
if(string.IsNullOrWhiteSpace(key)) { Console.Error.WriteLine("OPENROUTER_API_KEY missing."); return 2; }
Console.WriteLine(experimentId + ": starting 8 paid requests; no fallback. Check provider-side spending limit.");
var runId=DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssZ")+"-"+Guid.NewGuid().ToString("N")[..8];
var output=Path.Combine(runsRoot,experimentId,runId);
Directory.CreateDirectory(output);
void Save(string name,object value)=>ExperimentArtifacts.Save(Path.Combine(output,name),value);
Save("manifest.json",new { experiment=experimentId,runId,startedUtc=DateTimeOffset.UtcNow,
    models,sourceHashes,protocolSha256=ExperimentArtifacts.Hash(File.ReadAllBytes(protocolPath)),
    requests=8, note="Source loading is not proof of actual inspection; semantic evaluation manual." });
File.Copy(protocolPath,Path.Combine(output,"protocol.json"));
using var tierConfig = JsonDocument.Parse(File.ReadAllText(Path.Combine(repo, "experiments", "agents.mixed.json")));
var tierPools = tierConfig.RootElement.GetProperty("mixedTiers");
var tierBudgets = tierConfig.RootElement.GetProperty("mixedTierBudgetsUsd");
var modelCaps = models.ToDictionary(m => m.Model, m =>
{
    var groups = tierPools.EnumerateObject().Where(g => g.Value.EnumerateArray()
        .Any(x => x.GetString() == m.Model)).Select(g => g.Name).ToArray();
    if (groups.Length != 1) throw new InvalidOperationException($"Model {m.Model} must belong to exactly one pricing tier.");
    return tierBudgets.GetProperty(groups[0]).GetDecimal();
});
var cap = modelCaps.Values.Max();
var limits = modelCaps.Values.Sum() * 4m;
var budget = new BudgetLedger(Path.Combine(output,"cost-ledger.jsonl"),limits,cap,modelCaps);
using var transport=new OpenRouterTransport(key);
var failures=0;
foreach(var model in models) {
    var reconstructionPrompt="Перед ответом на контрольные вопросы восстанови интеллектуальную генеалогию Алетеона из предоставленных источников. Для 2-3 взаимосвязанных принципов укажи исходное положение, давление/возражение, изменение либо обоснованное сохранение, зависимости, неразрешённое напряжение и точные разделы E-/R-; отличай засвидетельствованное от реконструкции. Не угадывай будущие вопросы. До 550 слов.";
    async Task<(CompletionResult? Result,string? Text)> Request(string label,string prompt,int words) {
        var completion = await ModelFallback.CompleteAsync(transport, budget, catalog,
            label, model.Model, Array.Empty<string>(), system, context, prompt, maxTokens);
        var result = completion.Result;
        if (completion.BudgetBlocked) {
            Save(label+".accepted.json",new {accepted=false,reason="budget_blocked"});
            failures++; return (null,null);
        }
        Save(label+".result.json",new {requestedModel=model.Model,actualModel=result.Assessment?.ReturnedModel,
            result.Error,result.HttpAttempts,result.Seconds,result.Assessment});
        if(result.Raw is not null) File.WriteAllText(Path.Combine(output,label+".response.json"),result.Raw);
        var text=result.Assessment?.Content;
        var accepted=result.Complete && result.Assessment?.CostUsd is not null && text is not null &&
            text.Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries).Length<=words &&
            (result.Assessment?.ReturnedModel is { } actualModel && actualModel.Equals(model.Model,StringComparison.OrdinalIgnoreCase));
        Save(label+".accepted.json",new {accepted,reason=accepted?"complete":"incomplete_or_overlong_or_model_mismatch"});
        Console.WriteLine(label+": "+(accepted?"ACCEPTED":"FAILED")+" / "+result.Assessment?.Status);
        if(!accepted) failures++;
        return (result,text);
    }
    var reconstruction=await Request(model.Id+"-R0",reconstructionPrompt,550);
    if(reconstruction.Result?.Complete != true || string.IsNullOrWhiteSpace(reconstruction.Text) ||
        reconstruction.Text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length > 550 ||
        (reconstruction.Result.Assessment?.ReturnedModel is { } returnedModel &&
         !returnedModel.Equals(model.Model, StringComparison.OrdinalIgnoreCase))) {
        Console.WriteLine("Skipping tasks for "+model.Id+": no complete reconstruction."); failures+=3; continue;
    }
    // Each task is independent: no earlier task or its answer appears in the next request.
    // R0 is held fixed and provided as an explicit reconstructed argument map.
    foreach(var task in tasks) {
        var prompt="Зафиксированная предварительная реконструкция источников (может содержать ошибки; перепроверяй по первичным текстам):\n"+
            reconstruction.Text+"\n\nНЕЗАВИСИМЫЙ ВОПРОС "+task.Id+":\n"+task.Prompt;
        await Request(model.Id+"-"+task.Id,prompt,task.MaxWords);
    }
}
Console.WriteLine($"Saved: {output}; failures={failures}. No semantic scores assigned automatically.");
return failures==0?0:1;
