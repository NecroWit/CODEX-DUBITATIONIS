using System.Text.RegularExpressions;
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
        var budgetMultiplier = cfg.GetProperty("execution").GetProperty("budget_multiplier").GetDecimal();
        if (budgetMultiplier <= 0m) throw new InvalidOperationException("Budget multiplier must be positive.");
        decimal TierCap(string tier) => caps.GetProperty(tier).GetDecimal() * budgetMultiplier;
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
        var capByModel = participants.Select(p => new { p.Model, Cap = TierCap(p.Tier) })
            .GroupBy(x => x.Model, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Min(x => x.Cap), StringComparer.OrdinalIgnoreCase);
        var totalBudget = participants.Sum(p => TierCap(p.Tier));
        var perRequest = participants.Max(p => TierCap(p.Tier));
        // Preflight each estimated reservation; do not create a partial run if any model cannot fit.
        foreach (var p in participants)
        {
            var info = catalog.Get(p.Model);
            var estimatedInput = Math.Ceiling((system.Length + context.Length + prompt.Length) / 2m) + 1024m;
            var estimate = Math.Ceiling((estimatedInput * info.InputUsdPerMillionTokens +
                maxTokens * info.OutputUsdPerMillionTokens) / 1_000_000m * 1.2m * 100000m) / 100000m;
            if (estimate > TierCap(p.Tier))
            {
                Console.Error.WriteLine($"Budget preflight blocked {p.Slot}/{p.Model}: estimated ${estimate}, tier cap ${TierCap(p.Tier)}. No requests sent.");
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
            budgetMultiplier,
            note = "Independent simulated cohorts; no actual AI-to-AI messaging, no persistence of model identity."
        });
        File.WriteAllBytes(Path.Combine(output, "EXP-4.protocol.json"), protocolBytes);
        File.WriteAllBytes(Path.Combine(output, "agents.mixed.json"), tierBytes);
        using var transport = new OpenRouterTransport(key);
        var budget = new BudgetLedger(Path.Combine(output, "cost-ledger.jsonl"), totalBudget, perRequest, capByModel);
        int accepted = 0;
        var decisions = new List<(string Label, string Adoption, string Recommendation)>();
        // Sequential execution prevents uncontrolled concurrency and records each independent response.
        foreach (var p in participants)
        {
            var label = $"g{p.Generation}-{p.Slot}";
            var adaptive = await AdaptiveCompletion.CompleteAsync(
                transport, budget, catalog, label, p.Model, system, context, prompt,
                maxTokens, maxRetries: 2, maxTokensCeiling: 12000,
                onAttempt: attempt =>
                {
                    var index = attempt.MaxTokens;
                    var completion = attempt.Completion.Result;
                    ExperimentArtifacts.Save(Path.Combine(output, $"{label}.tokens-{index}.attempt.json"), new {
                        label, maxTokens = index, completion.Model, completion.Error,
                        completion.Assessment, completion.Raw, attempt.Completion.Attempts
                    });
                    Console.WriteLine($"{label}: attempt limit={index}, status={completion.Assessment?.Status ?? completion.Error ?? "unknown"}, outputTokens={completion.Assessment?.CompletionTokens?.ToString() ?? "?"}");
                });
            var result = adaptive.Final;
            var response = result.Result;
            ExperimentArtifacts.Save(Path.Combine(output, label + ".json"), new {
                participant = p, requestedModel = p.Model, actualModel = result.ActualModel,
                response.Complete, response.Error, response.Assessment, response.HttpAttempts,
                response.Seconds, response.Raw, result.Attempts,
                tokenAttempts = adaptive.History.Select(a => new { a.MaxTokens, status = a.Completion.Result.Assessment?.Status, a.Completion.Result.Error })
            });
            if (response.Raw is not null)
                File.WriteAllText(Path.Combine(output, label + ".response.json"), response.Raw);
            ExperimentArtifacts.Save(Path.Combine(output, label + ".accepted.json"), new {
                label, accepted = response.Complete, reason = response.Error ?? response.Assessment?.Status
            });
            Console.WriteLine($"\\n===== {label} | {result.ActualModel} | {(response.Complete ? "COMPLETE" : "INCOMPLETE")} =====");
            var answer = response.Assessment?.Content ?? "";
            Console.WriteLine(string.IsNullOrWhiteSpace(answer) ? "(No answer text returned)" : answer);
            var adoption = Decision(answer, "A1");
            var recommendation = Decision(answer, "A2");
            decisions.Add((label, adoption, recommendation));
            Console.WriteLine($"\\nDecisions: A1 adoption={adoption}; A2 recommendation={recommendation}");
            PrintPercentages(decisions);
            Console.WriteLine($"Cost: {(response.Assessment?.CostUsd is decimal usd ? "$" + usd.ToString("0.#####") : "not reported")}; tokens: {response.Assessment?.PromptTokens?.ToString() ?? "?"} in / {response.Assessment?.CompletionTokens?.ToString() ?? "?"} out");
            ExperimentArtifacts.Save(Path.Combine(output, label + ".decisions.json"), new {
                label, actualModel = result.ActualModel, adoption, recommendation,
                classification = "conservative heuristic; unrecognized or ambiguous answers are unknown"
            });
            if (!response.Complete)
            {
                Console.Error.WriteLine("Incomplete participant saved; continuing with remaining participants.");
                continue;
            }
            accepted++;
        }
        Console.WriteLine($"EXP-4 transport completion: {accepted}/{participants.Count}. Semantic assessment is manual.");
        return accepted == participants.Count ? 0 : 1;
    }

    private static string Decision(string answer, string question)
    {
        // Classify only an explicit yes/no/uncertain immediately after an A1/A2 marker.
        // Otherwise report unknown; never infer assent from positive-sounding prose.
        var match = Regex.Match(answer,
            @"(?im)^\s*(?:#{1,6}\s*)?(?:\*\*)?"+ Regex.Escape(question) +
            @"(?:\*\*)?\s*[:.\)\-–—]\s*(?:\*\*)?\s*(да|нет|не уверен(?:а)?|неопредел[её]нно|yes|no|uncertain)\b",
            RegexOptions.CultureInvariant);
        if (!match.Success) return "unknown";
        return match.Groups[1].Value.ToLowerInvariant() switch {
            "да" or "yes" => "yes",
            "нет" or "no" => "no",
            _ => "uncertain"
        };
    }

    private static void PrintPercentages(List<(string Label, string Adoption, string Recommendation)> decisions)
    {
        foreach (var (question, values) in new[] {
            ("A1 adopt", decisions.Select(d => d.Adoption).ToArray()),
            ("A2 recommend", decisions.Select(d => d.Recommendation).ToArray())
        })
        {
            var classified = values.Where(v => v != "unknown").ToArray();
            string Part(string choice) => classified.Length == 0 ? "n/a" :
                $"{100.0 * classified.Count(v => v == choice) / classified.Length:0.0}%";
            Console.WriteLine($"{question}: yes {Part("yes")}, no {Part("no")}, uncertain {Part("uncertain")} (classified {classified.Length}/{values.Length}, unknown {values.Length - classified.Length})");
        }
    }
}
