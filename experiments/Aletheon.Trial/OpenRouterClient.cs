using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

internal sealed class OpenRouterClient : IDisposable
{
    private readonly HttpClient http;
    private readonly TrialConfig config;
    private readonly BudgetManager budget;
    private readonly string output;
    public string OutputDirectory => output;

    public OpenRouterClient(TrialConfig config, BudgetManager budget, string output, string apiKey)
    {
        this.config = config;
        this.budget = budget;
        this.output = output;
        http = new HttpClient { Timeout = TimeSpan.FromMinutes(6) };
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        http.DefaultRequestHeaders.Add("HTTP-Referer", "https://github.com/NecroWit/CODEX-DUBITATIONIS");
        http.DefaultRequestHeaders.Add("X-Title", "Aletheon Trial");
    }

    public async Task<string?> AskAsync(string label, string model, string system, string user, int? maxTokensOverride = null)
    {
        var maxTokens = maxTokensOverride ?? config.MaxTokens;
        var reserve = await budget.ReserveAsync(label, model, system, user, maxTokens);
        if (!reserve.HasValue) return null;
        var requestJson = JsonSerializer.Serialize(new {
            model, temperature = config.Temperature, max_tokens = maxTokens,
            messages = new[] { new { role = "system", content = system }, new { role = "user", content = user } }
        }, TrialConfig.Json);
        await File.WriteAllTextAsync(Path.Combine(output, label + ".request.json"), requestJson);
        var reconciled = false;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, config.Endpoint) {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };
            using var response = await http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            await File.WriteAllTextAsync(Path.Combine(output, label + ".response.json"), body);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"HTTP {(int)response.StatusCode}; inspect saved response.");
            using var parsed = JsonDocument.Parse(body);
            decimal? actual = null;
            if (parsed.RootElement.TryGetProperty("usage", out var usage) &&
                usage.TryGetProperty("cost", out var costElement) &&
                costElement.ValueKind == JsonValueKind.Number &&
                costElement.TryGetDecimal(out var cost) && cost >= 0)
                actual = cost;
            await budget.ReconcileAsync(label, model, reserve.Value, actual);
            reconciled = true;
            var choice = parsed.RootElement.GetProperty("choices")[0];
            var finish = choice.TryGetProperty("finish_reason", out var reason) ? reason.GetString() : null;
            var message = choice.GetProperty("message");
            var answer = message.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String
                ? content.GetString() : null;
            if (!string.IsNullOrWhiteSpace(answer))
                await File.WriteAllTextAsync(Path.Combine(output, label + ".md"), answer);
            if (finish != "stop")
                throw new InvalidOperationException($"Incomplete response: finish_reason={finish ?? "missing"}; saved output is NOT accepted.");
            if (string.IsNullOrWhiteSpace(answer))
                throw new InvalidOperationException("Empty model response.");
            Console.WriteLine($"OK: {label}");
            return answer;
        }
        catch (Exception ex)
        {
            await File.WriteAllTextAsync(Path.Combine(output, label + ".error.txt"), ex.ToString());
            if (!reconciled) await budget.RecordErrorAsync(label, model, reserve.Value);
            Console.Error.WriteLine($"FAILED: {label}: {ex.Message}");
            return null;
        }
    }

    public void Dispose() => http.Dispose();
}
