using System.Text.Json;
using System.Text.RegularExpressions;

internal static class RatingsReport
{
    private static readonly string[] Fields =
        ["interest", "logical_coherence", "willingness_to_follow", "desire_to_follow"];

    public static async Task<bool> RecordAsync(string folder, string round, string agent, string model, string answer)
    {
        var marker = Regex.Match(answer, @"(?im)^\s*(?:#{1,6}\s*)?(?:\*\*)?EVALUATION\s*:?\s*(?:\*\*)?\s*$");
        var critique = marker.Success ? answer[..marker.Index].Trim() : "";
        var values = new Dictionary<string, int>();
        var evaluation = marker.Success ? answer[marker.Index..] : "";
        foreach (var field in Fields)
        {
            var match = Regex.Match(evaluation, @"(?im)^" + field + @"\s*:\s*(\d{1,2})\s*$");
            if (!match.Success || !int.TryParse(match.Groups[1].Value, out var score) || score > 10)
                break;
            values[field] = score;
        }

        var rationaleMatch = Regex.Match(evaluation, @"(?ims)^\s*(?:#{1,6}\s*)?(?:\*\*)?RATIONALE\s*:?\s*(?:\*\*)?\s*\n(?<text>[\s\S]+?)(?=^\s*#{1,6}\s+[A-Z][A-Z\s-]*\s*$|\z)");
        var rationale = rationaleMatch.Success ? rationaleMatch.Groups["text"].Value.Trim() : "";
        if (marker.Success && rationaleMatch.Success)
        {
            var tailStart = marker.Index + rationaleMatch.Index + rationaleMatch.Length;
            critique = (critique + "\n" + answer[tailStart..]).Trim();
        }
        // Do not accept ratings-only answers as complete philosophical critiques.
        var valid = critique.Length >= 300 && values.Count == Fields.Length && rationale.Length >= 250;
        var record = new {
            round, agent, model, valid,
            critiqueCharacters = critique.Length,
            scores = values,
            rationale,
            validationError = valid ? null : "Expected >=300 characters of critique, four integer 0..10 ratings, and >=250 characters of reflection."
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
