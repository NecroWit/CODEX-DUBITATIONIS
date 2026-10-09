using System.Text.Json;
using System.Text.RegularExpressions;

internal static class RatingsReport
{
    private static readonly string[] Fields =
        ["interest", "logical_coherence", "willingness_to_follow", "desire_to_follow"];

    // Read the response as lines and recognize section markers independently of their contents.
    // In particular, "RATIONALE: explanation" and "RATIONALE:\nexplanation" are equivalent.
    private static readonly Regex SectionLine = new(
        @"^\s*(?:#{1,6}\s*)?(?:\*\*|__)?\s*(?:(?:FINAL|REVISED)\s+)?(?<name>EVALUATION|RATIONALE)\s*:?(?:\*\*|__)?\s*(?<inline>.*?)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ScoreLine = new(
        @"^\s*(?:[-*+]\s+|\d+[.)]\s+)?(?:\*\*|__|\x60)?\s*(?<field>interest|logical[ _-]+coherence|willingness[ _-]+to[ _-]+follow|desire[ _-]+to[ _-]+follow)\s*(?:\*\*|__|\x60)?\s*[:=：-]\s*(?:\*\*)?\s*(?<score>[+-]?\d+)(?:\s*/\s*10)?\s*(?:\*\*)?\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private sealed record Parsed(string Critique, string Rationale,
        Dictionary<string, int> Scores, List<string> InvalidScores, bool Duplicate,
        bool HasEvaluation, bool HasRationale);

    private static Parsed Parse(string answer)
    {
        var lines = answer.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var markers = new List<(int Line, string Name, string Inline)>();
        for (var i = 0; i < lines.Length; i++)
        {
            var match = SectionLine.Match(lines[i]);
            if (!match.Success) continue;
            var name = match.Groups["name"].Value.ToUpperInvariant();
            var inline = match.Groups["inline"].Value.Trim();
            // An EVALUATION heading is a marker, not arbitrary prose beginning with the word.
            if (name == "EVALUATION" && inline.Length != 0) continue;
            markers.Add((i, name, inline));
        }

        // The final evaluation/rationale pair is authoritative (repairs append a new pair).
        var evaluation = markers.LastOrDefault(m => m.Name == "EVALUATION" &&
            markers.Any(r => r.Name == "RATIONALE" && r.Line > m.Line));
        var hasEvaluation = evaluation.Name == "EVALUATION";
        var rationale = hasEvaluation
            ? markers.FirstOrDefault(m => m.Name == "RATIONALE" && m.Line > evaluation.Line)
            : default;
        var hasRationale = rationale.Name == "RATIONALE";

        var critique = string.Join("\n", lines.Take(hasEvaluation ? evaluation.Line : lines.Length)).Trim();
        var scores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var invalid = new List<string>();
        var duplicate = false;
        if (hasEvaluation)
        {
            var end = hasRationale ? rationale.Line : lines.Length;
            for (var i = evaluation.Line + 1; i < end; i++)
            {
                var match = ScoreLine.Match(lines[i]);
                if (!match.Success) continue;
                var field = Regex.Replace(match.Groups["field"].Value.ToLowerInvariant(), @"[ -]", "_");
                if (!int.TryParse(match.Groups["score"].Value, out var score) || score is < 0 or > 10)
                {
                    invalid.Add(field + "=" + match.Groups["score"].Value + " (expected 0..10)");
                    continue;
                }
                if (!scores.TryAdd(field, score)) duplicate = true;
            }
        }

        var reflection = hasRationale
            ? string.Join("\n", new[] { rationale.Inline }
                .Concat(lines.Skip(rationale.Line + 1))).Trim()
            : "";
        return new Parsed(critique, reflection, scores, invalid, duplicate,
            hasEvaluation, hasRationale);
    }

    public static async Task<bool> RecordAsync(string folder, string round, string agent, string model, string answer)
    {
        var parsed = Parse(answer);
        var valid = parsed.HasEvaluation && parsed.HasRationale &&
            parsed.Critique.Length >= 300 && parsed.Scores.Count == Fields.Length &&
            parsed.InvalidScores.Count == 0 && !parsed.Duplicate &&
            parsed.Rationale.Length >= 250;

        var record = new {
            round, agent, model, valid,
            critiqueCharacters = parsed.Critique.Length,
            scores = parsed.Scores,
            invalidScores = parsed.InvalidScores,
            rationale = parsed.Rationale,
            validationError = valid ? null :
                "Expected >=300 characters of critique, four distinct integer 0..10 ratings in EVALUATION, and >=250 characters of reflection in RATIONALE."
        };
        await File.WriteAllTextAsync(Path.Combine(folder, $"{round}-{agent}.evaluation.json"),
            JsonSerializer.Serialize(record, TrialConfig.Json));
        if (!valid)
        {
            var error = $"Invalid substantive critique/evaluation in {round}-{agent}: critique={parsed.Critique.Length} chars (min 300), scores={parsed.Scores.Count}/4, invalidScores={string.Join(", ", parsed.InvalidScores)}, rationale={parsed.Rationale.Length} chars (min 250); raw response retained.";
            await File.WriteAllTextAsync(Path.Combine(folder, $"{round}-{agent}.validation-error.txt"), error);
            Console.Error.WriteLine(error);
        }
        return valid;
    }
}
