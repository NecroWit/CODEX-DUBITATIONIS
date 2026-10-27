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
            if (!doc.RootElement.GetProperty("accepted").GetBoolean()) return false;
        }
        return true;
    }
}
