using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Aletheon.ExperimentKit;

/// <summary>Offline model rotation dedicated to EXP-3 paired N/C trials.</summary>
public static class Exp3ModelRotation
{
    public static void Prepare(string repo, string protocolPath, string runsRoot, string nextId)
    {
        using var poolsDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(repo, "experiments", "agents.mixed.json")));
        var tiers = poolsDocument.RootElement.GetProperty("mixedTiers");
        var catalog = ModelCatalog.Load(Path.Combine(repo, "research", "Aletheon.ExperimentKit", "models.json"));
        using var current = JsonDocument.Parse(File.ReadAllText(protocolPath));
        var maxTokens = current.RootElement.GetProperty("max_completion_tokens").GetInt32();
        var eligible = tiers.EnumerateObject().SelectMany(t => t.Value.EnumerateArray().Select(x => x.GetString()!))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(id => catalog.All.Any(m => m.Id.Equals(id, StringComparison.OrdinalIgnoreCase) && m.MaxCompletionTokens >= maxTokens))
            .ToArray();
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var directory in Directory.GetDirectories(runsRoot, "EXP-3.*"))
            foreach (var run in Directory.GetDirectories(directory))
            {
                var file = Path.Combine(run, "protocol.json");
                if (!File.Exists(file)) continue;
                using var prior = JsonDocument.Parse(File.ReadAllText(file));
                var participants = prior.RootElement.GetProperty("participants").EnumerateArray().ToArray();
                if (participants.Length == 1) used.Add(participants[0].GetProperty("model_id").GetString()!);
            }
        var currentModel = current.RootElement.GetProperty("participants")[0].GetProperty("model_id").GetString();
        var alternatives = eligible.Where(id => !string.Equals(id, currentModel, StringComparison.OrdinalIgnoreCase)).ToArray();
        var available = alternatives.Where(id => !used.Contains(id)).ToArray();
        if (available.Length == 0) available = alternatives;
        if (available.Length == 0) throw new InvalidOperationException("No alternative eligible EXP-3 model; refusing to retry the same model.");
        var chosen = available[RandomNumberGenerator.GetInt32(available.Length)];
        var tier = tiers.EnumerateObject().Single(t => t.Value.EnumerateArray()
            .Any(x => string.Equals(x.GetString(), chosen, StringComparison.OrdinalIgnoreCase))).Name;
        var cap = poolsDocument.RootElement.GetProperty("mixedTierBudgetsUsd").GetProperty(tier).GetDecimal();
        var protocol = JsonNode.Parse(File.ReadAllText(protocolPath))!.AsObject();
        protocol["id"] = nextId;
        protocol["participants"]![0]!["model_id"] = chosen;
        protocol["budget_usd_max"] = cap * 8m;
        protocol["rotation"] = new JsonObject {
            ["source"] = "experiments/agents.mixed.json:mixedTiers",
            ["selection"] = "single model, no repeat until eligible models exhausted"
        };
        File.WriteAllText(protocolPath, protocol.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
        Console.WriteLine($"{nextId}: selected {chosen} offline; inspect protocol and budget before --run.");
    }
}
