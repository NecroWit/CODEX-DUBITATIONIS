using System.Text.Json;

namespace Aletheon.ExperimentKit;

/// <summary>EXP-3 only: evaluate the latest recorded attempt, not any historical attempt.</summary>
public static class Exp3RunStatus
{
    public static bool LatestAccepted(string experimentDirectory, IReadOnlyList<string> labels)
    {
        if (!Directory.Exists(experimentDirectory)) return false;
        var latest = Directory.GetDirectories(experimentDirectory)
            .Where(d => File.Exists(Path.Combine(d, "manifest.json")))
            .OrderByDescending(d => Path.GetFileName(d), StringComparer.Ordinal)
            .FirstOrDefault();
        if (latest is null) return false;
        foreach (var label in labels)
        {
            var file = Path.Combine(latest, label + ".accepted.json");
            if (!File.Exists(file)) return false;
            using var doc = JsonDocument.Parse(File.ReadAllText(file));
            if (doc.RootElement.GetProperty("accepted").GetBoolean()) continue;
            // Older EXP-3 runs used to reject truncated replies. Treat a recorded
            // truncated model response as an observation, not a failed experiment.
            var resultFile = Path.Combine(latest, label + ".result.json");
            var responseFile = Path.Combine(latest, label + ".response.json");
            if (!File.Exists(resultFile) || !File.Exists(responseFile)) return false;
            using var result = JsonDocument.Parse(File.ReadAllText(resultFile));
            var root = result.RootElement;
            if (!root.TryGetProperty("Assessment", out var assessment) ||
                assessment.ValueKind != JsonValueKind.Object ||
                !assessment.TryGetProperty("Status", out var status) ||
                status.GetString() != "truncated") return false;
            var expectedModel = root.GetProperty("requestedModel").GetString();
            var actualModel = root.GetProperty("actualModel").GetString();
            if (string.IsNullOrWhiteSpace(expectedModel) ||
                !string.Equals(expectedModel, actualModel, StringComparison.OrdinalIgnoreCase)) return false;
        }
        return true;
    }
}
