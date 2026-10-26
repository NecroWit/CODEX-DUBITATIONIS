using System.Text.Json;
using System.Text.RegularExpressions;

namespace Aletheon.ExperimentKit;

/// <summary>Shared archived-run sequencing for EXP-1 and EXP-2. Never sends requests.</summary>
public static class ExperimentSequence
{
    public sealed record Latest(string Id, int Number, string Path, string[] Attempts, bool Accepted, bool Completed, string? ArchivedProtocol);

    public static Latest? Inspect(string runsRoot, int major, IReadOnlyList<string> requiredLabels)
    {
        if (!Directory.Exists(runsRoot)) return null;
        var latest = Directory.GetDirectories(runsRoot, $"EXP-{major}.*")
            .Select(path => (Path: path, Match: Regex.Match(Path.GetFileName(path), $@"^EXP-{major}\.(\d+)$")))
            .Where(x => x.Match.Success)
            .OrderBy(x => int.Parse(x.Match.Groups[1].Value)).LastOrDefault();
        if (latest.Path is null) return null;
        var attempts = Directory.GetDirectories(latest.Path)
            .Where(path => File.Exists(System.IO.Path.Combine(path, "manifest.json"))).ToArray();
        bool Accepted(string attempt) => requiredLabels.All(label =>
        {
            try
            {
                using var json = JsonDocument.Parse(File.ReadAllText(System.IO.Path.Combine(attempt, label + ".accepted.json")));
                return json.RootElement.GetProperty("accepted").ValueKind == JsonValueKind.True;
            }
            catch { return false; }
        });
        // Completion is about usable responses, not word-count acceptance.
        bool Completed(string attempt) => requiredLabels.All(label =>
        {
            try
            {
                using var json = JsonDocument.Parse(File.ReadAllText(System.IO.Path.Combine(attempt, label + ".result.json")));
                var root = json.RootElement;
                var assessment = root.GetProperty("Assessment");
                return root.GetProperty("Error").ValueKind == JsonValueKind.Null &&
                    assessment.GetProperty("Complete").GetBoolean() &&
                    assessment.GetProperty("CostUsd").ValueKind == JsonValueKind.Number &&
                    !string.IsNullOrWhiteSpace(assessment.GetProperty("Content").GetString()) &&
                    string.Equals(root.GetProperty("requestedModel").GetString(),
                        root.GetProperty("actualModel").GetString(), StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        });
        var archived = attempts.OrderByDescending(x => x)
            .Select(x => System.IO.Path.Combine(x, "protocol.json")).FirstOrDefault(File.Exists);
        var number = int.Parse(latest.Match.Groups[1].Value);
        return new Latest($"EXP-{major}.{number}", number, latest.Path, attempts, attempts.Any(Accepted), attempts.Any(Completed), archived);
    }

    public static string RequireArchivedProtocol(Latest latest)
        => latest.ArchivedProtocol ?? throw new InvalidOperationException(
            $"Cannot recover {latest.Id}: no archived protocol. No API calls sent.");

    public static string NextId(Latest latest, int major) => $"EXP-{major}.{latest.Number + 1}";
}
