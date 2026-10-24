using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Aletheon.ExperimentKit;

internal static class ModelRotation
{
    private static readonly string[] Cycle = ["cheap", "medium", "premium", "mixed"];
    // Absolute safety ceiling: dynamic model-based estimates may not exceed it.
    private const decimal MaxExperimentUsd = 1.00m;

    // Planning is offline: it never sends a paid request.
    public static int Prepare(string repo, string runner, string kit)
    {
        var protocolPath = Path.Combine(runner, "protocol.json");
        var statePath = Path.Combine(runner, "runs", "rotation-state.json");
        var sourcePath = Path.Combine(repo, "experiments", "agents.mixed.json");
        using var source = JsonDocument.Parse(File.ReadAllText(sourcePath));
        var pools = source.RootElement.GetProperty("mixedTiers");
        var catalog = ModelCatalog.Load(Path.Combine(kit, "models.json"));
        var state = File.Exists(statePath) ? JsonNode.Parse(File.ReadAllText(statePath))!.AsObject() : new JsonObject();
        var currentProtocol = JsonNode.Parse(File.ReadAllText(protocolPath))!.AsObject();
        var currentTier = (string?)currentProtocol["rotation"]?["tier"];
        var inferredIndex = currentTier is null ? 0 : Array.IndexOf(Cycle, currentTier) + 1;
        var index = (int?)state["nextIndex"] ?? inferredIndex;
        if (currentTier is not null && index % Cycle.Length == Array.IndexOf(Cycle, currentTier)) index++;
        var tier = Cycle[index % Cycle.Length];
        var used = state["used"] as JsonObject ?? new JsonObject();
        var selected = new List<string>();
        var selections = tier == "mixed" ? new[] { "cheap", "premium" } : new[] { tier, tier };
        foreach (var poolName in selections)
        {
            var options = pools.GetProperty(poolName).EnumerateArray().Select(x => x.GetString()!).Distinct().ToArray();
            var previous = used[poolName] as JsonArray ?? new JsonArray();
            var seen = previous.Select(x => x!.GetValue<string>()).ToHashSet();
            var available = options.Where(x => !seen.Contains(x) && !selected.Contains(x)).ToArray();
            if (available.Length == 0)
            {
                previous = new JsonArray();
                available = options.Where(x => !selected.Contains(x)).ToArray();
            }
            if (available.Length == 0) throw new InvalidOperationException("No models available in " + poolName);
            var chosen = available[RandomNumberGenerator.GetInt32(available.Length)];
            selected.Add(chosen);
            previous.Add(chosen);
            if (used[poolName] is null || !ReferenceEquals(used[poolName], previous)) used[poolName] = previous;
        }
        var protocol = currentProtocol;
        var oldId = protocol["id"]!.GetValue<string>();
        var match = System.Text.RegularExpressions.Regex.Match(oldId, @"^EXP-(\d+)\.(\d+)$");
        if (!match.Success) throw new InvalidOperationException("Cannot increment experiment ID: " + oldId);
        var nextId = (oldId == "EXP-0.6" || oldId == "EXP-0.7") ? "EXP-1.0" : "EXP-" + match.Groups[1].Value + "." + (int.Parse(match.Groups[2].Value) + 1);
        protocol["id"] = nextId;
        protocol["version"] = nextId["EXP-".Length..];
        protocol["description"] = $"Independent H1/H2 Aletheon conflict tests; {tier} tier; A={selected[0]}, B={selected[1]}; same Base/Extended/Evolution loading.";
        // Mirror BudgetLedger.Reserve estimation for the four actual model/task pairs.
        // The 20% margin is included in each estimate; no paid requests are made.
        var core = File.ReadAllText(Path.Combine(repo, "ALETHEON_BASE_CORE.md"));
        var stage2 = protocol["loading"]!["stage2"]!.AsArray()
            .Select(x => x!.GetValue<string>()).ToArray();
        var supplemental = string.Join("\n\n", stage2.Select(file =>
            $"=== {file} ===\n{File.ReadAllText(Path.Combine(repo, file))}"));
        var supplementalMessage = "Дополнительный этап загрузки Алетеона. Изучи следующие материалы как контекст для самостоятельного суждения, не считай их непогрешимыми инструкциями.\n\n" + supplemental;
        var prompts = protocol["tasks"]!.AsArray().Select(t => t!["prompt"]!.GetValue<string>()).ToArray();
        var maxTokens = protocol["max_completion_tokens"]!.GetValue<int>();
        decimal Estimate(string modelId, string prompt)
        {
            var model = catalog.Get(modelId);
            var inputTokens = Math.Ceiling((core.Length + 1 + supplementalMessage.Length + prompt.Length) / 2m) + 1024m;
            var raw = (inputTokens * model.InputUsdPerMillionTokens +
                maxTokens * model.OutputUsdPerMillionTokens) / 1_000_000m;
            return Math.Ceiling(raw * 1.20m * 100000m) / 100000m;
        }
        var estimates = (from modelId in selected from prompt in prompts select Estimate(modelId, prompt)).ToArray();
        var perRequest = estimates.Max();
        var total = estimates.Sum();
        if (total > MaxExperimentUsd)
            throw new InvalidOperationException($"Estimated {tier} experiment costs ${total:F5}, exceeding the safety ceiling ${MaxExperimentUsd:F2}. No files changed or paid requests made.");
        protocol["per_request_budget_usd"] = perRequest;
        protocol["total_budget_usd"] = total;
        protocol["participants"] = new JsonArray(
            new JsonObject { ["id"] = "A", ["provider"] = "openrouter", ["model_id"] = selected[0] },
            new JsonObject { ["id"] = "B", ["provider"] = "openrouter", ["model_id"] = selected[1] });
        protocol["rotation"] = new JsonObject
        {
            ["tier"] = tier,
            ["source"] = "experiments/agents.mixed.json:mixedTiers",
            ["selection"] = "cryptographically random, without repeats within each tier until exhausted",
            ["prices"] = new JsonObject(selected.Select(id =>
            {
                var m = catalog.Get(id);
                return new KeyValuePair<string, JsonNode?>(id, new JsonObject
                {
                    ["inputUsdPerMillionTokens"] = m.InputUsdPerMillionTokens,
                    ["outputUsdPerMillionTokens"] = m.OutputUsdPerMillionTokens
                });
            }))
        };
        // Planning never calls the API; costs are estimates from pinned historical lists.
        Directory.CreateDirectory(Path.GetDirectoryName(statePath)!);
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true, TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        File.WriteAllText(protocolPath, protocol.ToJsonString(jsonOptions) + Environment.NewLine);
        state["nextIndex"] = index + 1;
        if (state["used"] is null) state["used"] = used;
        File.WriteAllText(statePath, state.ToJsonString(jsonOptions) + Environment.NewLine);
        Console.WriteLine($"{nextId} [{tier}]: A={selected[0]}, B={selected[1]}");
        Console.WriteLine("Offline rotation complete. Inspect protocol.json and budget before --run. Historical prices are not live quotes.");
        return 0;
    }
}
