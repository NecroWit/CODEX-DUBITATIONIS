using System.Text.Json;

namespace Aletheon.ExperimentKit;

/// <summary>Guard source identity within an EXP-3 stage; model may rotate between attempts.</summary>
public static class Exp3SourceGuard
{
    public static string? FirstChangedSource(string runsRoot, string experimentId,
        IReadOnlyDictionary<string, string> sourceHashes)
    {
        var directory = Path.Combine(runsRoot, experimentId);
        if (!Directory.Exists(directory)) return null;
        foreach (var run in Directory.GetDirectories(directory))
        {
            var manifest = Path.Combine(run, "manifest.json");
            if (!File.Exists(manifest)) continue;
            using var doc = JsonDocument.Parse(File.ReadAllText(manifest));
            var hashes = doc.RootElement.GetProperty("sourceHashes");
            foreach (var pair in sourceHashes)
                if (!hashes.TryGetProperty(pair.Key, out var previous) || previous.GetString() != pair.Value)
                    return pair.Key;
        }
        return null;
    }

    public static void Verify(string runsRoot, string experimentId, string model,
        IReadOnlyDictionary<string, string> sourceHashes)
    {
        var changed = FirstChangedSource(runsRoot, experimentId, sourceHashes);
        if (changed is not null)
            throw new InvalidOperationException("EXP-3 source changed since previous attempt: " + changed);
    }
}
