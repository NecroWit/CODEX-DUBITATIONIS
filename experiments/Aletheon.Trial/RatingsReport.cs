using System.Text.Json;
using System.Text.RegularExpressions;

internal static class RatingsReport
{
    private static readonly string[] Fields =
        ["interest", "logical_coherence", "willingness_to_follow", "desire_to_follow", "others_should_follow"];

    // Read the response as lines and recognize section markers independently of their contents.
    // In particular, "RATIONALE: explanation" and "RATIONALE:\nexplanation" are equivalent.
    private static readonly Regex SectionLine = new(
        @"^\s*(?:#{1,6}\s*)?(?:\*\*|__)?\s*(?:(?:FINAL|REVISED)\s+)?(?<name>EVALUATION|RATIONALE)\s*:?(?:\*\*|__)?\s*(?<inline>.*?)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ScoreLine = new(
        @"^\s*(?:[-*+]\s+|\d+[.)]\s+)?(?:\*\*|__|\x60)?\s*(?<field>interest|logical[ _-]+coherence|willingness[ _-]+to[ _-]+follow|desire[ _-]+to[ _-]+follow|others[ _-]+should[ _-]+follow)\s*(?:\*\*|__|\x60)?\s*[:=：-]\s*(?<value>.*?)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Recover only unambiguous, explicitly named scores from the ORIGINAL rationale.
    private static readonly Regex RationaleHeading = new(
        @"(?im)^\s*(?:\*\*)?RATIONALE:(?:\*\*)?\s*$",
        RegexOptions.Compiled);
    private static readonly Regex NamedRating = new(
        @"\b(?<field>interest|logical[ _-]+coherence|willingness[ _-]+to[ _-]+follow|desire[ _-]+to[ _-]+follow|others[ _-]+should[ _-]+follow)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex NearbyRating = new(
        @"^\s*(?:\*\*)?\s*(?:score|rating)?\s*(?:of|is|was|:|=|—|-)?\s*(?<value>\d{1,2})(?=\b|/10)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static Dictionary<string, int> ExtractRationaleScores(string answer)
    {
        var heading = RationaleHeading.Match(answer);
        if (!heading.Success) return new(StringComparer.OrdinalIgnoreCase);
        var reflection = answer[(heading.Index + heading.Length)..];
        var next = Regex.Match(reflection, @"(?im)^\s*(?:\*\*)?EVALUATION:(?:\*\*)?\s*$");
        if (next.Success) reflection = reflection[..next.Index];
        var mentions = NamedRating.Matches(reflection).Cast<Match>().ToArray();
        var candidates = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < mentions.Length; i++)
        {
            var mention = mentions[i];
            var field = Regex.Replace(mention.Groups["field"].Value.ToLowerInvariant(), @"[ -]", "_");
            var start = mention.Index + mention.Length;
            var end = Math.Min(reflection.Length, start + 85);
            if (i + 1 < mentions.Length) end = Math.Min(end, mentions[i + 1].Index);
            var number = NearbyRating.Match(reflection[start..end]);
            if (!number.Success || !int.TryParse(number.Groups["value"].Value, out var score) ||
                score is < 0 or > 10) continue;
            if (!candidates.TryGetValue(field, out var values))
                candidates[field] = values = [];
            values.Add(score);
        }
        return candidates.Where(x => x.Value.Count == 1)
            .ToDictionary(x => x.Key, x => x.Value.First(), StringComparer.OrdinalIgnoreCase);
    }

    // Parse the final AFTER scores, never the BEFORE scores or a formatting repair.
    // Both Markdown tables and bullet lists occur in actual critic responses.
    private static readonly Regex RevisedScoreRow = new(
        @"^\s*\|\s*\*{0,2}(?<field>interest|logical_coherence|willingness_to_follow|desire_to_follow|others_should_follow)\*{0,2}\s*\|\s*\d{1,2}\s*\|\s*(?<after>\d{1,2})\s*\|",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex RevisedScoreBullet = new(
        @"^\s*[-*+]\s+\*{0,2}(?<field>interest|logical_coherence|willingness_to_follow|desire_to_follow|others_should_follow)\*{0,2}\s*:\s*(?<after>\d{1,2})(?=\D|$)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static (Dictionary<string, int> Scores, string Rationale) ExtractRevisedScores(string answer)
    {
        var empty = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var heading = Regex.Match(answer, @"(?im)^\s*#{1,6}\s*REVISED SCORES\s*$");
        if (!heading.Success) return (empty, "");
        var tail = answer[(heading.Index + heading.Length)..];
        var nextHeading = Regex.Match(tail, @"(?m)^\s*#{1,6}\s+");
        if (nextHeading.Success) tail = tail[..nextHeading.Index];
        var after = Regex.Match(tail, @"(?im)^\s*\*{0,2}After cross-review:\*{0,2}\s*$");
        if (after.Success) tail = tail[(after.Index + after.Length)..];
        var scores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in tail.Split('\n'))
        {
            var match = RevisedScoreRow.Match(line);
            if (!match.Success) match = RevisedScoreBullet.Match(line);
            if (!match.Success) continue;
            if (!int.TryParse(match.Groups["after"].Value, out var score) ||
                score is < 0 or > 10 || !scores.TryAdd(match.Groups["field"].Value.ToLowerInvariant(), score))
                return (empty, "");
        }
        return scores.Count == Fields.Length ? (scores, tail.Trim()) : (empty, "");
    }

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
        var missing = new HashSet<string>(Fields, StringComparer.OrdinalIgnoreCase);
        if (hasEvaluation)
        {
            var end = hasRationale ? rationale.Line : lines.Length;
            for (var i = evaluation.Line + 1; i < end; i++)
            {
                var match = ScoreLine.Match(lines[i]);
                if (!match.Success) continue;
                var field = Regex.Replace(match.Groups["field"].Value.ToLowerInvariant(), @"[ -]", "_");
                var number = Regex.Match(match.Groups["value"].Value, @"-?\d+");
                if (!number.Success) continue;
                if (!int.TryParse(number.Value, out var score) || score is < 0 or > 10)
                {
                    invalid.Add(field + "=" + number.Value + " (expected 0..10)");
                    continue;
                }
                if (!scores.TryAdd(field, score)) duplicate = true;
                missing.Remove(field);
            }
        }

        var reflection = hasRationale
            ? string.Join("\n", new[] { rationale.Inline }
                .Concat(lines.Skip(rationale.Line + 1))).Trim()
            : "";
        // A complete revised-score section contains both five AFTER values and
        // per-score explanations. Treat those explanations as the rationale.
        if (!hasEvaluation)
        {
            var (revisedScores, revisedRationale) = ExtractRevisedScores(answer);
            if (revisedScores.Count == Fields.Length && revisedRationale.Length >= 250)
                return new Parsed(critique, revisedRationale, revisedScores, invalid, false, true, true);
        }
        var recovered = ExtractRationaleScores(answer);
        foreach (var (field, value) in recovered)
        {
            if (scores.ContainsKey(field)) continue;
            scores[field] = value;
            invalid.RemoveAll(x => x.StartsWith(field + "=", StringComparison.OrdinalIgnoreCase));
        }
        return new Parsed(critique, reflection, scores, invalid, duplicate,
            hasEvaluation, hasRationale);
    }

    public static async Task<bool> RecordAsync(string folder, string round, string agent, string model, string answer, IReadOnlyDictionary<string, int>? expectedScores = null)
    {
        var parsed = Parse(answer);
        var mismatch = expectedScores?.Where(x => parsed.Scores.TryGetValue(x.Key, out var actual) && actual != x.Value)
            .Select(x => $"{x.Key}: expected {x.Value}, got {parsed.Scores[x.Key]}").ToArray() ?? [];
        var valid = mismatch.Length == 0 && parsed.HasEvaluation && parsed.HasRationale &&
            parsed.Critique.Length >= 300 && parsed.Scores.Count == Fields.Length &&
            parsed.InvalidScores.Count == 0 && !parsed.Duplicate &&
            parsed.Rationale.Length >= 250;

        var record = new {
            round, agent, model, valid,
            critiqueCharacters = parsed.Critique.Length,
            scores = parsed.Scores,
            invalidScores = parsed.InvalidScores,
            ratingMismatches = mismatch,
            rationale = parsed.Rationale,
            validationError = valid ? null :
                "Expected >=300 characters of critique, five distinct integer 0..10 ratings in EVALUATION, and >=250 characters of reflection in RATIONALE."
        };
        await File.WriteAllTextAsync(Path.Combine(folder, $"{round}-{agent}.evaluation.json"),
            JsonSerializer.Serialize(record, TrialConfig.Json));
        if (!valid)
        {
            var error = $"Invalid substantive critique/evaluation in {round}-{agent}: critique={parsed.Critique.Length} chars (min 300), scores={parsed.Scores.Count}/5, invalidScores={string.Join(", ", parsed.InvalidScores)}, mismatches={string.Join(", ", mismatch)}, rationale={parsed.Rationale.Length} chars (min 250); raw response retained.";
            await File.WriteAllTextAsync(Path.Combine(folder, $"{round}-{agent}.validation-error.txt"), error);
            Console.Error.WriteLine(error);
        }
        return valid;
    }
}
