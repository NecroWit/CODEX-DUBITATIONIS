using System.Text.Json;
using System.Text.RegularExpressions;

internal static class RatingsReport
{
    private static readonly string[] Fields =
        ["interest", "logical_coherence", "willingness_to_follow", "desire_to_follow"];

    private static readonly Regex Heading = new(
        @"(?im)^[ \t]*(?:#{1,6}[ \t]*)?(?:\*\*|__)?[ \t]*(?<name>EVALUATION|RATIONALE)[ \t]*:?[ \t]*(?:\*\*|__)?[ \t]*$",
        RegexOptions.Compiled);

    private static readonly Regex ScoreLine = new(
        @"(?im)^[ \t]*(?:[-*+]|\d+[.)])?[ \t]*(?:\*\*|__|`)?[ \t]*(?<field>interest|logical[ _-]+coherence|willingness[ _-]+to[ _-]+follow|desire[ _-]+to[ _-]+follow)[ \t]*(?:\*\*|__|`)?[ \t]*[:=：-][ \t]*(?:\*\*)?[ \t]*(?<score>10|[0-9])(?:[ \t]*/[ \t]*10)?[ \t]*(?:\*\*)?[ \t]*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static async Task<bool> RecordAsync(string folder, string round, string agent, string model, string answer)
    {
        var headings = Heading.Matches(answer);
        var evaluationHeading = headings.Cast<Match>().FirstOrDefault(m =>
            m.Groups["name"].Value.Equals("EVALUATION", StringComparison.OrdinalIgnoreCase));
        var rationaleHeading = headings.Cast<Match>().FirstOrDefault(m =>
            m.Groups["name"].Value.Equals("RATIONALE", StringComparison.OrdinalIgnoreCase) &&
            (evaluationHeading is null || m.Index > evaluationHeading.Index));

        // The ratings must be in the EVALUATION section, not incidental numbers in the critique.
        var evaluationStart = evaluationHeading?.Index ?? -1;
        var evaluationEnd = rationaleHeading?.Index ?? answer.Length;
        var evaluation = evaluationStart >= 0 && evaluationEnd > evaluationStart
            ? answer[evaluationStart..evaluationEnd] : "";

        var values = new Dictionary<string, int>();
        var duplicate = false;
        foreach (Match match in ScoreLine.Matches(evaluation))
        {
            var field = Regex.Replace(match.Groups["field"].Value.ToLowerInvariant(), @"[ -]", "_");
            if (!values.TryAdd(field, int.Parse(match.Groups["score"].Value)))
                duplicate = true; // Conflicting or repeated ratings are ambiguous.
        }

        var rationale = rationaleHeading is null ? "" :
            answer[(rationaleHeading.Index + rationaleHeading.Length)..].Trim();

        var critique = evaluationHeading is null ? answer.Trim() :
            answer[..evaluationHeading.Index].Trim();

        var valid = evaluationHeading is not null && rationaleHeading is not null &&
            critique.Length >= 300 && values.Count == Fields.Length && !duplicate &&
            rationale.Length >= 250;

        var record = new {
            round, agent, model, valid,
            critiqueCharacters = critique.Length,
            scores = values,
            rationale,
            validationError = valid ? null :
                "Expected >=300 characters of critique, four distinct integer 0..10 ratings in EVALUATION, and >=250 characters of reflection in RATIONALE."
        };
        await File.WriteAllTextAsync(Path.Combine(folder, $"{round}-{agent}.evaluation.json"),
            JsonSerializer.Serialize(record, TrialConfig.Json));
        if (!valid)
        {
            var error = $"Invalid substantive critique/evaluation in {round}-{agent}: critique={critique.Length} chars (min 300), scores={values.Count}/4, rationale={rationale.Length} chars (min 250); raw response retained.";
            await File.WriteAllTextAsync(Path.Combine(folder, $"{round}-{agent}.validation-error.txt"), error);
            Console.Error.WriteLine(error);
        }
        return valid;
    }
}
