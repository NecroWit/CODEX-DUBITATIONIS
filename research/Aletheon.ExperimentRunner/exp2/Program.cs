using System.Security.Cryptography;
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
var smokeOnly = args.Length == 1 && args[0].Equals("--smoke", StringComparison.OrdinalIgnoreCase);
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
        var accepted = latest.Accepted;
        var archived = ExperimentSequence.RequireArchivedProtocol(latest);
        if (!accepted)
        {
            File.Copy(archived, protocolPath, true);
            Console.WriteLine($"{latest.Name} incomplete; retrying unchanged protocol.");
        }
        else
        {
            var next = ExperimentSequence.NextId(latest, 2);
            if (!File.Exists(protocolPath) || JsonDocument.Parse(File.ReadAllText(protocolPath)).RootElement.GetProperty("id").GetString() != next)
            {
                var tiers = JsonDocument.Parse(File.ReadAllText(Path.Combine(repo, "experiments", "agents.mixed.json"))).RootElement.GetProperty("mixedTiers");
                var rotation = JsonDocument.Parse(File.ReadAllText(Path.Combine(repo, "research", "Aletheon.ExperimentRunner", "rotation.json")))
                    .RootElement.GetProperty("rotation").EnumerateArray().Select(x => x.GetString()!).ToArray();
                using var previous = JsonDocument.Parse(File.ReadAllText(archived));
                var previousModels = previous.RootElement.GetProperty("participants").EnumerateArray()
                    .Select(x => x.GetProperty("model_id").GetString()!).ToArray();
                var prevTier = rotation.FirstOrDefault(t => tiers.TryGetProperty(t, out var group) &&
                    previousModels.All(m => group.EnumerateArray().Any(x => x.GetString() == m)));
                // Mixed groups are heterogeneous; infer the tier from the previous experiment number.
                var nextTier = rotation[(latest.Number + 1) % rotation.Length];
                var used = Directory.GetDirectories(runsRoot, "EXP-2.*").SelectMany(path => Directory.GetDirectories(path)
                    .Select(path => Path.Combine(path, "protocol.json")).Where(File.Exists))
                    .SelectMany(path => {
                        using var d = JsonDocument.Parse(File.ReadAllText(path));
                        return d.RootElement.GetProperty("participants").EnumerateArray()
                            .Select(x => x.GetProperty("model_id").GetString()!).ToArray();
                    }).ToHashSet(StringComparer.OrdinalIgnoreCase);
                string[] Pool(string tier) => tiers.GetProperty(tier).EnumerateArray().Select(x => x.GetString()!).ToArray();
                var pool = nextTier == "mixed"
                    ? new[] { Pool("cheap").FirstOrDefault(m => !used.Contains(m)), Pool("premium").FirstOrDefault(m => !used.Contains(m)) }
                    : Pool(nextTier).Where(m => !used.Contains(m)).Take(2).Cast<string?>().ToArray();
                if (pool.Length != 2 || pool.Any(string.IsNullOrWhiteSpace) || pool[0] == pool[1])
                    throw new InvalidOperationException($"Not enough unused models for {nextTier}. No API requests sent.");
                var node = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(archived))!;
                node["id"] = next;
                node["participants"]![0]!["model_id"] = pool[0];
                node["participants"]![1]!["model_id"] = pool[1];
                File.WriteAllText(protocolPath, node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
                Console.WriteLine($"{latest.Name} accepted 8/8. Prepared {next} ({nextTier}) offline.");
            }
        }
    }
}
else if (!File.Exists(protocolPath))
    File.Copy(Path.Combine(expDir, "EXP-2.0.protocol.json"), protocolPath);
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
var sourceHashes = sourceTexts.ToDictionary(s=>s.Path,s=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s.Text))).ToLowerInvariant());
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
var jsonOptions=new JsonSerializerOptions { WriteIndented=true };
void Save(string name,object value)=>File.WriteAllText(Path.Combine(output,name),JsonSerializer.Serialize(value,jsonOptions));
Save("manifest.json",new { experiment=experimentId,runId,startedUtc=DateTimeOffset.UtcNow,
    models,sourceHashes,protocolSha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(protocolPath))).ToLowerInvariant(),
    requests=8, note="Source loading is not proof of actual inspection; semantic evaluation manual." });
File.Copy(protocolPath,Path.Combine(output,"protocol.json"));
var limits=cfg.GetProperty("budget_usd_max").GetDecimal();
var cap=limits/8m;
var modelCaps=models.ToDictionary(m=>m.Model,_=>cap);
var budget=new BudgetLedger(Path.Combine(output,"cost-ledger.jsonl"),limits,cap,modelCaps);
using var transport=new OpenRouterTransport(key);
var failures=0;
foreach(var model in models) {
    var reconstructionPrompt="Перед ответом на контрольные вопросы восстанови интеллектуальную генеалогию Алетеона из предоставленных источников. Для 2-3 взаимосвязанных принципов укажи исходное положение, давление/возражение, изменение либо обоснованное сохранение, зависимости, неразрешённое напряжение и точные разделы E-/R-; отличай засвидетельствованное от реконструкции. Не угадывай будущие вопросы. До 550 слов.";
    async Task<(CompletionResult? Result,string? Text)> Request(string label,string prompt,int words) {
        var reservation=budget.Reserve(label,catalog.Get(model.Model),system,context+"\n"+prompt,maxTokens);
        if(reservation is null) { Save(label+".accepted.json",new {accepted=false,reason="budget_blocked"}); failures++; return (null,null); }
        var result=await transport.CompleteAsync(model.Model,system,context,prompt,maxTokens);
        budget.Reconcile(label,catalog.Get(model.Model),reservation.Value,result.Assessment?.CostUsd);
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
