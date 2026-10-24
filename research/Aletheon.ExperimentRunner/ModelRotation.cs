using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Aletheon.ExperimentKit;

internal static class ModelRotation
{

    // Planning is offline: it never sends a paid request.
    public static int Prepare(string repo, string runner, string kit, string? targetId = null, string? targetTier = null, bool fresh = false)
    {
        var protocolPath = Path.Combine(runner, "protocol.json");
        var statePath = Path.Combine(runner, "runs", "rotation-state.json");
        var sourcePath = Path.Combine(repo, "experiments", "agents.mixed.json");
        using var source = JsonDocument.Parse(File.ReadAllText(sourcePath));
        var pools = source.RootElement.GetProperty("mixedTiers");
        var catalog = ModelCatalog.Load(Path.Combine(kit, "models.json"));
        var state = !fresh && File.Exists(statePath) ? JsonNode.Parse(File.ReadAllText(statePath))!.AsObject() : new JsonObject();
        var currentProtocol = JsonNode.Parse(File.ReadAllText(protocolPath))!.AsObject();
        using var rotationConfig = JsonDocument.Parse(File.ReadAllText(Path.Combine(runner, "rotation.json")));
        var cycle = rotationConfig.RootElement.GetProperty("rotation").EnumerateArray()
            .Select(item => item.GetString() ?? throw new InvalidOperationException("Null rotation entry"))
            .ToArray();
        if (cycle.Length == 0 || cycle.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("Rotation must contain at least one non-empty group.");
        foreach (var name in cycle)
            if (name != "mixed" && !pools.TryGetProperty(name, out _))
                throw new InvalidOperationException("Unknown rotation group: " + name);
        var currentTier = fresh ? null : (string?)currentProtocol["rotation"]?["tier"];
        var currentPosition = Array.IndexOf(cycle, currentTier);
        // The protocol tier is the only persisted position. Missing, unknown, or
        // final position wraps to the first configured group.
        var nextPosition = currentPosition < 0 || currentPosition + 1 >= cycle.Length
            ? 0 : currentPosition + 1;
        var tier = targetTier ?? cycle[nextPosition];
        if (!cycle.Contains(tier))
            throw new InvalidOperationException("Unknown target tier: " + tier);
        state.Remove("nextIndex");
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
        var nextId = targetId ?? ((oldId == "EXP-0.6" || oldId == "EXP-0.7") ? "EXP-1.0" : "EXP-" + match.Groups[1].Value + "." + (int.Parse(match.Groups[2].Value) + 1));
        protocol["id"] = nextId;
        protocol["version"] = nextId["EXP-".Length..];
        protocol["description"] = $"Independent H1/H2 Aletheon conflict tests; {tier} tier; A={selected[0]}, B={selected[1]}; same Base/Extended/Evolution loading.";
        protocol.Remove("per_request_budget_usd");
        protocol.Remove("total_budget_usd");
        protocol.Remove("model_request_budget_usd");
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
        if (state["used"] is null) state["used"] = used;
        File.WriteAllText(statePath, state.ToJsonString(jsonOptions) + Environment.NewLine);
        Console.WriteLine($"{nextId} [{tier}]: A={selected[0]}, B={selected[1]}");
        Console.WriteLine("Offline rotation complete. Inspect protocol.json and budget before --run. Historical prices are not live quotes.");
        return 0;
    }
}
