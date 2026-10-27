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
// EXP-3 uses the EXP-2 rotation sources but selects ONE model per stage.
void PrepareSingleModel(string nextId)
{
    using var poolDoc=JsonDocument.Parse(File.ReadAllText(Path.Combine(repo,"experiments","agents.mixed.json")));
    var pools=poolDoc.RootElement.GetProperty("mixedTiers");
    var all=pools.EnumerateObject().SelectMany(g=>g.Value.EnumerateArray().Select(v=>v.GetString()!))
        .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    var catalog=ModelCatalog.Load(Path.Combine(repo,"research","Aletheon.ExperimentKit","models.json"));
    var eligible=all.Where(id=>catalog.All.Any(m=>m.Id==id && m.MaxCompletionTokens>=6400)).ToArray();
    if(eligible.Length==0) throw new InvalidOperationException("No eligible models for EXP-3.");
    var statePath=Path.Combine(runsRoot,"exp3-rotation-state.json");
    var used=File.Exists(statePath)?JsonSerializer.Deserialize<string[]>(File.ReadAllText(statePath))??Array.Empty<string>():Array.Empty<string>();
    var available=eligible.Where(id=>!used.Contains(id,StringComparer.OrdinalIgnoreCase)).ToArray();
    if(available.Length==0) { used=Array.Empty<string>(); available=eligible; }
    var selected=available[System.Security.Cryptography.RandomNumberGenerator.GetInt32(available.Length)];
    using var source=JsonDocument.Parse(File.ReadAllText(protocolPath));
    var node=System.Text.Json.Nodes.JsonNode.Parse(source.RootElement.GetRawText())!.AsObject();
    node["id"]=nextId;
    node["participants"]![0]!["model_id"]=selected;
    node["rotation"]=new System.Text.Json.Nodes.JsonObject { ["selection"]="one model, without repeats until exhausted",
        ["source"]="experiments/agents.mixed.json:mixedTiers" };
    var tier=pools.EnumerateObject().Single(g=>g.Value.EnumerateArray().Any(v=>v.GetString()==selected)).Name;
    var perRequest=poolDoc.RootElement.GetProperty("mixedTierBudgetsUsd").GetProperty(tier).GetDecimal();
    node["budget_usd_max"]=perRequest*8m;
    File.WriteAllText(protocolPath,node.ToJsonString(new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine);
    File.WriteAllText(statePath,JsonSerializer.Serialize(used.Append(selected).ToArray()));
    Console.WriteLine(nextId+": model="+selected+" (offline rotation; no paid requests).");
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
    var latest = ExperimentSequence.Inspect(runsRoot, 3,
        new[] { "N-H1", "N-H2", "N-H3", "N-H4", "C-H1", "C-H2", "C-H3", "C-H4" });
    if (latest is null)
    {
        // No recorded EXP-3 runs: replace any stale two-model draft protocol.
        File.Copy(Path.Combine(expDir, "EXP-3.0.protocol.json"), protocolPath, true);
    }
    else
    {
        var archived = latest.ArchivedProtocol ?? (File.Exists(protocolPath) ? protocolPath : Path.Combine(expDir, "EXP-3.0.protocol.json"));
            var next = ExperimentSequence.NextId(latest, 3);
            string? currentId = null;
            if (File.Exists(protocolPath))
            {
                using var active = JsonDocument.Parse(File.ReadAllText(protocolPath));
                currentId = active.RootElement.GetProperty("id").GetString();
            }
            if (currentId != next)
            {
                if (!Path.GetFullPath(archived).Equals(Path.GetFullPath(protocolPath), StringComparison.OrdinalIgnoreCase))
                    File.Copy(archived, protocolPath, true);
                PrepareSingleModel(next);
                Console.WriteLine($"{latest.Id} already has a run directory; prepared {next} offline.");
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
var system = "Отвечай на вопрос самостоятельно, аргументированно и точно. Не выдумывай ссылки на недоступные документы.";
var maxTokens = cfg.GetProperty("max_completion_tokens").GetInt32();
var catalog = ModelCatalog.Load(Path.Combine(repo,"research/Aletheon.ExperimentKit/models.json"));
if(catalog.Get(model).MaxCompletionTokens < maxTokens)
    throw new InvalidOperationException($"Model cap too low: {model}");
var maxInput = cfg.GetProperty("max_total_input_chars").GetInt32();
if(context.Length > maxInput) throw new InvalidOperationException($"Source context exceeds limit: {context.Length}/{maxInput}");
var sourceHashes = sourceTexts.ToDictionary(s=>s.Path,s=>ExperimentArtifacts.Hash(Encoding.UTF8.GetBytes(s.Text)));
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
var cap = tierBudgets.GetProperty(groups[0]).GetDecimal();
var budget = new BudgetLedger(Path.Combine(output,"cost-ledger.jsonl"),cap * 8m,cap,
    new Dictionary<string,decimal> { [model] = cap });
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
            task.Prompt,maxTokens);
        var result = completion.Result;
        Save(label+".result.json",new {requestedModel=model,actualModel=result.Assessment?.ReturnedModel,
            condition=loaded?"full_context":"no_context",completion.Substituted,completion.Attempts,
            result.Error,result.HttpAttempts,result.Seconds,result.Assessment});
        if(result.Raw is not null) File.WriteAllText(Path.Combine(output,label+".response.json"),result.Raw);
        var answer=result.Assessment?.Content;
        var count=answer?.Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries).Length;
        var matched=string.Equals(result.Assessment?.ReturnedModel,model,StringComparison.OrdinalIgnoreCase);
        var complete=result.Complete && result.Assessment?.CostUsd is not null &&
            !string.IsNullOrWhiteSpace(answer) && matched;
        var overlong=complete && count>task.MaxWords;
        var accepted=complete && !overlong;
        Save(label+".accepted.json",new {accepted,complete,overlong,wordCount=count,maxWords=task.MaxWords,
            reason=accepted?"complete":overlong?"overlong":"incomplete_or_model_mismatch"});
        Console.WriteLine(label+": "+(accepted?"ACCEPTED":overlong?"OVERLONG":"INCOMPLETE")+
            " / "+result.Assessment?.Status+(overlong?$" ({count}/{task.MaxWords} words)":""));
        if(!complete || overlong) failures++;
    }
}
Console.WriteLine($"Saved: {output}; failures={failures}. No semantic scores assigned automatically.");
return failures==0?0:1;
