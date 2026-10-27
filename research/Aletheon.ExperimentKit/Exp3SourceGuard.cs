using System.Text.Json;

namespace Aletheon.ExperimentKit;

/// <summary>Allow model rotation between EXP-3 attempts; fail closed on source changes.</summary>
public static class Exp3SourceGuard
{
    public static void Verify(string runsRoot, string experimentId, string model,
        IReadOnlyDictionary<string, string> sourceHashes)
    {
        var directory = Path.Combine(runsRoot, experimentId);
        if (!Directory.Exists(directory)) return;
        foreach (var run in Directory.GetDirectories(directory))
        {
            var manifest = Path.Combine(run, "manifest.json");
            if (!File.Exists(manifest)) continue;
            using var doc = JsonDocument.Parse(File.ReadAllText(manifest));
            var root = doc.RootElement;
            var hashes = root.GetProperty("sourceHashes");
            foreach (var pair in sourceHashes)
                if (!hashes.TryGetProperty(pair.Key, out var previous) || previous.GetString() != pair.Value)
                    throw new InvalidOperationException("EXP-3 source changed since previous attempt: " + pair.Key);
        }
    }
}
