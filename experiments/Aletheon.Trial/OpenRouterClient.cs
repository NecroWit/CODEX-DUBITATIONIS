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

    public async Task<string?> AskAsync(string label, string model, string system, string user, int? maxTokensOverride = null, bool retryOnLength = true)
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
            // Retry only transient throttling/server failures. One budget reservation covers all attempts.
            string body = "";
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, config.Endpoint) {
                    Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
                };
                using var response = await http.SendAsync(request);
                body = await response.Content.ReadAsStringAsync();
                await File.WriteAllTextAsync(Path.Combine(output, label + ".response.json"), body);
                await File.WriteAllTextAsync(Path.Combine(output, label + $".attempt-{attempt}.response.json"), body);
                if (response.IsSuccessStatusCode) break;

                var status = (int)response.StatusCode;
                var retryable = status == 429 || status is 500 or 502 or 503 or 504;
                if (!retryable || attempt == 3)
                    throw new HttpRequestException($"HTTP {status} after {attempt} attempt(s); inspect saved response.");

                var delay = TimeSpan.FromSeconds(attempt == 1 ? 5 : 15);
                var retryAfter = response.Headers.RetryAfter;
                if (retryAfter?.Delta is { } delta && delta > TimeSpan.Zero)
                    delay = delta;
                else if (retryAfter?.Date is { } date && date > DateTimeOffset.UtcNow)
                    delay = date - DateTimeOffset.UtcNow;
                if (delay > TimeSpan.FromSeconds(60)) delay = TimeSpan.FromSeconds(60);
                Console.Error.WriteLine($"RETRY: {label}, HTTP {status}, attempt {attempt}/3, waiting {delay.TotalSeconds:F0}s.");
                await Task.Delay(delay);
            }
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
            if (finish == "length" && retryOnLength)
            {
                var increasedTokens = Math.Min(maxTokens * 2, 16000);
                if (increasedTokens > maxTokens)
                {
                    Console.Error.WriteLine($"RETRY: {label}, finish_reason=length; retrying once with {increasedTokens} tokens.");
                    return await AskAsync(label + "-length-retry", model, system, user,
                        maxTokensOverride: increasedTokens, retryOnLength: false);
                }
            }
            if (finish != "stop")
                throw new InvalidOperationException($"Incomplete response: finish_reason={finish ?? "missing"}; saved output is NOT accepted.");
            if (string.IsNullOrWhiteSpace(answer))
                throw new InvalidOperationException("Empty model response.");
            Console.WriteLine($"RECEIVED: {label}");
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
