using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Aletheon.ExperimentKit;

public sealed record CompletionResult(string Model, string? Raw, ResponseAssessment? Assessment,
    string? Error, int HttpAttempts, double Seconds)
{
    public bool Complete => Error is null && Assessment?.Complete == true;
}

public sealed class OpenRouterTransport : IDisposable
{
    private readonly HttpClient http;
    public OpenRouterTransport(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) throw new ArgumentException("Missing OPENROUTER_API_KEY");
        http = new HttpClient { Timeout = TimeSpan.FromMinutes(8) };
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        http.DefaultRequestHeaders.Add("HTTP-Referer", "https://github.com/NecroWit/CODEX-DUBITATIONIS");
        http.DefaultRequestHeaders.Add("X-Title", "Aletheon Experiments");
    }
    public async Task<CompletionResult> CompleteAsync(string model, string system, string user,
        int maxTokens, CancellationToken cancellation = default)
    {
        var request = JsonSerializer.Serialize(new {
            model, temperature = 0, max_completion_tokens = maxTokens,
            messages = new[] { new { role = "system", content = system },
                               new { role = "user", content = user } }
        });
        var clock = Stopwatch.StartNew();
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post,
                    "https://openrouter.ai/api/v1/chat/completions") {
                    Content = new StringContent(request, Encoding.UTF8, "application/json") };
                using var response = await http.SendAsync(req, cancellation);
                var raw = await response.Content.ReadAsStringAsync(cancellation);
                if (!response.IsSuccessStatusCode)
                {
                    if (attempt == 1 && (response.StatusCode == HttpStatusCode.TooManyRequests ||
                        (int)response.StatusCode >= 500))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), cancellation);
                        continue;
                    }
                    return new(model, raw, null, $"HTTP {(int)response.StatusCode}", attempt, clock.Elapsed.TotalSeconds);
                }
                try
                {
                    var assessment = ResponseAnalysis.Parse(raw);
                    return new(model, raw, assessment, null, attempt, clock.Elapsed.TotalSeconds);
                }
                catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
                {
                    return new(model, raw, null, "Invalid provider JSON: " + ex.Message, attempt, clock.Elapsed.TotalSeconds);
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                if (attempt == 1 && !cancellation.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellation);
                    continue;
                }
                return new(model, null, null, ex.Message, attempt, clock.Elapsed.TotalSeconds);
            }
        }
        throw new InvalidOperationException("Unreachable");
    }
    public void Dispose() => http.Dispose();
}
