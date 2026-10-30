using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Aletheon.ExperimentKit;

internal static class Exp4Runner
{
    private sealed record Participant(int Generation, string Slot, string Tier, string Model);
    private static readonly string[] Slots = ["cheap", "medium", "premium", "random"];

    public static async Task<int> Execute(string repo, string runner, string[] args)
    {
        bool paid = args[0] == "--exp4-run";
        if (args.Length > 3 || (args.Length > 1 && (!int.TryParse(args[1], out var n) || n < 1 || n > 4)))
        {
            Console.Error.WriteLine("Usage: --exp4-plan [generations:1..4] [seed] | --exp4-run [generations:1..4] [seed]");
            return 2;
        }
        int generations = args.Length > 1 ? int.Parse(args[1]) : 2;
        if (args.Length > 2 && !int.TryParse(args[2], out _))
        {
            Console.Error.WriteLine("Seed must be a 32-bit integer."); return 2;
        }
        int seed = args.Length > 2 ? int.Parse(args[2]) : 42;
        var expDir = Path.Combine(runner, "exp4");
        var protocolBytes = File.ReadAllBytes(Path.Combine(expDir, "EXP-4.protocol.json"));
        using var protocol = JsonDocument.Parse(protocolBytes);
        var cfg = protocol.RootElement;
        var id = cfg.GetProperty("id").GetString()!;
        if (generations > cfg.GetProperty("max_generations").GetInt32())
            throw new InvalidOperationException("Generations exceed protocol limit.");
        var tierBytes = File.ReadAllBytes(Path.Combine(repo, "experiments", "agents.mixed.json"));
        using var tierDoc = JsonDocument.Parse(tierBytes);
        var tiers = tierDoc.RootElement.GetProperty("mixedTiers");
        var caps = tierDoc.RootElement.GetProperty("mixedTierBudgetsUsd");
        var catalogPath = Path.Combine(repo, "research", "Aletheon.ExperimentKit", "models.json");
        var catalogBytes = File.ReadAllBytes(catalogPath);
        var catalog = ModelCatalog.Load(catalogPath);
        var maxTokens = cfg.GetProperty("max_completion_tokens").GetInt32();
        var random = new Random(seed);
        var participants = new List<Participant>();
        var tierNames = new[] { "cheap", "medium", "premium" };
        for (int g = 0; g < generations; g++)
        {
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var tier in tierNames)
            {
                var candidates = tiers.GetProperty(tier).EnumerateArray().Select(x => x.GetString()!)
                    .Where(m => catalog.All.Any(i => i.Id == m && i.MaxCompletionTokens >= maxTokens))
                    .Where(m => !used.Contains(m)).ToArray();
                if (candidates.Length == 0) throw new InvalidOperationException("No eligible models for " + tier);
                var chosen = candidates[random.Next(candidates.Length)];
                used.Add(chosen);
                participants.Add(new(g, tier, tier, chosen));
            }
            var remainder = tierNames.SelectMany(t => tiers.GetProperty(t).EnumerateArray()
                .Select(x => (Tier: t, Model: x.GetString()!)))
                .Where(x => !used.Contains(x.Model))
                .Where(x => catalog.All.Any(i => i.Id == x.Model && i.MaxCompletionTokens >= maxTokens))
                .ToArray();
            if (remainder.Length == 0) throw new InvalidOperationException("No eligible random model.");
            var draw = remainder[random.Next(remainder.Length)];
            participants.Add(new(g, "random", draw.Tier, draw.Model));
        }
        Console.WriteLine(JsonSerializer.Serialize(new { id, generations, seed, paid, participants },
            new JsonSerializerOptions { WriteIndented = true }));
        if (!paid) { Console.WriteLine("Offline plan only: no requests or charges."); return 0; }
        var key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
        if (string.IsNullOrWhiteSpace(key)) { Console.Error.WriteLine("OPENROUTER_API_KEY is required."); return 2; }

        // All sources are immutable within a run, and missing source files fail before any API call.
        var sources = cfg.GetProperty("source_files").EnumerateArray().Select(x => x.GetString()!).ToArray();
        var sourceRecords = sources.Select(relative =>
        {
            var path = Path.GetFullPath(Path.Combine(repo, relative));
            if (!path.StartsWith(Path.GetFullPath(repo) + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                || !File.Exists(path)) throw new InvalidOperationException("Invalid or missing source: " + relative);
            var bytes = File.ReadAllBytes(path);
            return new { path = relative, bytes, sha256 = ExperimentArtifacts.Hash(bytes) };
        }).ToArray();
        var corpus = string.Join("\n\n", sourceRecords.Select(s =>
            "=== " + s.path + " ===\n" + Encoding.UTF8.GetString(s.bytes)));
        var system = cfg.GetProperty("system_prompt").GetString()!;
        var context = cfg.GetProperty("loading_prompt").GetString()! + "\n\n" + corpus;
        var prompt = cfg.GetProperty("questions_prompt").GetString()!;
        var capByModel = participants.Select(p => new { p.Model, Cap = caps.GetProperty(p.Tier).GetDecimal() })
            .GroupBy(x => x.Model, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Min(x => x.Cap), StringComparer.OrdinalIgnoreCase);
        var totalBudget = participants.Sum(p => caps.GetProperty(p.Tier).GetDecimal());
        var perRequest = participants.Max(p => caps.GetProperty(p.Tier).GetDecimal());
        // Preflight each estimated reservation; do not create a partial run if any model cannot fit.
        foreach (var p in participants)
        {
            var info = catalog.Get(p.Model);
            var estimatedInput = Math.Ceiling((system.Length + context.Length + prompt.Length) / 2m) + 1024m;
            var estimate = Math.Ceiling((estimatedInput * info.InputUsdPerMillionTokens +
                maxTokens * info.OutputUsdPerMillionTokens) / 1_000_000m * 1.2m * 100000m) / 100000m;
            if (estimate > caps.GetProperty(p.Tier).GetDecimal())
            {
                Console.Error.WriteLine($"Budget preflight blocked {p.Slot}/{p.Model}: estimated ${estimate}, tier cap ${caps.GetProperty(p.Tier).GetDecimal()}. No requests sent.");
                return 2;
            }
        }
        var runId = DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssZ") + "-" + Guid.NewGuid().ToString("N")[..8];
        var output = Path.Combine(expDir, "runs", runId);
        Directory.CreateDirectory(output);
        ExperimentArtifacts.Save(Path.Combine(output, "manifest.json"), new {
            id, runId, seed, generations, participants, startedUtc = DateTimeOffset.UtcNow,
            protocolSha256 = ExperimentArtifacts.Hash(protocolBytes),
            tierConfigSha256 = ExperimentArtifacts.Hash(tierBytes),
            catalogSha256 = ExperimentArtifacts.Hash(catalogBytes),
            sources = sourceRecords.Select(s => new { s.path, s.sha256 }),
            corpusSha256 = ExperimentArtifacts.Hash(Encoding.UTF8.GetBytes(corpus)),
            totalBudgetUsd = totalBudget, perRequestBudgetUsd = perRequest,
            note = "Independent simulated cohorts; no actual AI-to-AI messaging, no persistence of model identity."
        });
        File.WriteAllBytes(Path.Combine(output, "EXP-4.protocol.json"), protocolBytes);
        File.WriteAllBytes(Path.Combine(output, "agents.mixed.json"), tierBytes);
        using var transport = new OpenRouterTransport(key);
        var budget = new BudgetLedger(Path.Combine(output, "cost-ledger.jsonl"), totalBudget, perRequest, capByModel);
        int accepted = 0;
        // Sequential execution prevents uncontrolled concurrency and records each independent response.
        foreach (var p in participants)
        {
            var label = $"g{p.Generation}-{p.Slot}";
            var result = await ModelFallback.CompleteAsync(transport, budget, catalog,
                label, p.Model, Array.Empty<string>(), system, context, prompt, maxTokens);
            var response = result.Result;
            ExperimentArtifacts.Save(Path.Combine(output, label + ".json"), new {
                participant = p, requestedModel = p.Model, actualModel = result.ActualModel,
                response.Complete, response.Error, response.Assessment, response.HttpAttempts,
                response.Seconds, response.Raw, result.Attempts
            });
            if (response.Raw is not null)
                File.WriteAllText(Path.Combine(output, label + ".response.json"), response.Raw);
            ExperimentArtifacts.Save(Path.Combine(output, label + ".accepted.json"), new {
                label, accepted = response.Complete, reason = response.Error ?? response.Assessment?.Status
            });
            Console.WriteLine($"{label}: {(response.Complete ? "COMPLETE" : "INCOMPLETE")} ({response.Error ?? response.Assessment?.Status})");
            if (!response.Complete)
            {
                Console.Error.WriteLine("Stopping after incomplete response. Inspect artifacts before retrying.");
                return 1;
            }
            accepted++;
        }
        Console.WriteLine($"EXP-4 transport completion: {accepted}/{participants.Count}. Semantic assessment is manual.");
        return 0;
    }
}
