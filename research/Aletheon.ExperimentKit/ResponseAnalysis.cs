using System.Text.Json;

namespace Aletheon.ExperimentKit;

public sealed record ResponseAssessment(bool Complete, string Status, string? FinishReason,
    string? NativeFinishReason, string Content, string? ReturnedModel, decimal? CostUsd,
    int? PromptTokens, int? CompletionTokens);

public static class ResponseAnalysis
{
    public static ResponseAssessment Parse(string raw)
    {
        using var doc = JsonDocument.Parse(raw);
        var root = doc.RootElement;
        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array ||
            choices.GetArrayLength() == 0)
            return new(false, "missing_choices", null, null, "", null, null, null, null);
        var choice = choices[0];
        var finish = String(choice, "finish_reason");
        var native = String(choice, "native_finish_reason");
        var message = choice.GetProperty("message");
        var content = String(message, "content") ?? "";
        var usage = root.TryGetProperty("usage", out var u) && u.ValueKind == JsonValueKind.Object ? u : default;
        decimal? cost = usage.ValueKind == JsonValueKind.Object && usage.TryGetProperty("cost", out var c) &&
            c.ValueKind == JsonValueKind.Number && c.TryGetDecimal(out var amount) ? amount : null;
        int? prompt = Number(usage, "prompt_tokens"), completion = Number(usage, "completion_tokens");
        var status = finish == "length" ? "truncated" : finish != "stop" ? "finish_" + (finish ?? "missing") :
            string.IsNullOrWhiteSpace(content) ? "empty_content" : "complete";
        return new(status == "complete", status, finish, native, content, String(root, "model"), cost, prompt, completion);
    }
    private static string? String(JsonElement element, string key) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(key, out var value) &&
        value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    private static int? Number(JsonElement element, string key) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(key, out var value) &&
        value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) ? number : null;
}
