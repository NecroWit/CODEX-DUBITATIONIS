using System.Diagnostics;
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

    private readonly Dictionary<string, string> assignedModels = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> round2RateLimited = new(StringComparer.OrdinalIgnoreCase);
    public bool WasRound2RateLimited(string id) => round2RateLimited.Contains(id);
    private readonly HashSet<string> usedStandbys = new(StringComparer.OrdinalIgnoreCase);

    private static string Participant(string label)
    {
        var parts = label.Split('-');
        if (parts.Length >= 2 && (parts[0] == "round1" || parts[0] == "round2"))
            return "critic-" + parts[1];
        if (parts.Length >= 2 && parts[0] == "arbiter")
            return "arbiter-" + parts[1];
        return label;
    }

    public string ResolveModel(string label, string configuredModel) =>
        assignedModels.TryGetValue(Participant(label), out var actual) ? actual : configuredModel;

    private async Task<string?> TryStandbyAsync(string label, string failedModel, string system, string user,
        int? maxTokensOverride, bool retryOnLength, int statusCode, string errorDetail)
    {
        var participant = Participant(label);
        var candidate = config.StandbyModels.FirstOrDefault(m => !usedStandbys.Contains(m));
        if (candidate is null) return null;
        usedStandbys.Add(candidate);
        assignedModels[participant] = candidate;
        Console.Error.WriteLine($"STANDBY: {participant}, {failedModel} -> {candidate} after HTTP {statusCode}: {errorDetail}");
        await File.AppendAllTextAsync(Path.Combine(output, "model-substitutions.jsonl"),
            JsonSerializer.Serialize(new {
                participant, failedModel, replacementModel = candidate,
                reason = $"HTTP {statusCode}", errorDetail, utc = DateTimeOffset.UtcNow
            }) + Environment.NewLine);
        return await AskAsync(label + "-standby-" + usedStandbys.Count, candidate, system, user,
            maxTokensOverride, retryOnLength);
    }


    private static string ErrorDetail(string body)
    {
        try
        {
            using var json = JsonDocument.Parse(body);
            if (json.RootElement.TryGetProperty("error", out var error))
            {
                if (error.ValueKind == JsonValueKind.Object &&
                    error.TryGetProperty("message", out var message) &&
                    message.ValueKind == JsonValueKind.String)
                    return message.GetString() ?? "No error message";
                return error.ToString();
            }
        }
        catch (JsonException) { }
        return "No error detail supplied; inspect saved response";
    }

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

    public async Task<string?> AskAsync(string label, string model, string system, string user, int? maxTokensOverride = null, bool retryOnLength = true, bool retryOnEmpty = true)
    {
        model = ResolveModel(label, model);
        var maxTokens = maxTokensOverride ?? config.MaxTokens;
        var reserve = await budget.ReserveAsync(label, model, system, user, maxTokens);
        if (!reserve.HasValue) return null;
        var requestJson = JsonSerializer.Serialize(new {
            model, temperature = config.Temperature, max_tokens = maxTokens,
            messages = new[] { new { role = "system", content = system }, new { role = "user", content = user } }
        }, TrialConfig.Json);
        await File.WriteAllTextAsync(Path.Combine(output, label + ".request.json"), requestJson);
        var reconciled = false;
        var elapsed = Stopwatch.StartNew();
        var attempts = 0;
        var completed = false;
        try
        {
            // Retry only transient throttling/server failures. One budget reservation covers all attempts.
            string body = "";
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                attempts = attempt;
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
                    throw new HttpRequestException($"HTTP {status} after {attempt} attempt(s): {ErrorDetail(body)}; inspect saved response.", null, response.StatusCode);

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
                throw new InvalidOperationException($"Incomplete response: finish_reason={finish ?? "missing"}, native_finish_reason={(choice.TryGetProperty("native_finish_reason", out var native) ? native.ToString() : "missing")}, provider={(parsed.RootElement.TryGetProperty("provider", out var provider) ? provider.ToString() : "missing")}, request_id={(parsed.RootElement.TryGetProperty("id", out var id) ? id.ToString() : "missing")}; inspect saved response for any error details; saved output is NOT accepted.");
            if (string.IsNullOrWhiteSpace(answer))
            {
                if (retryOnEmpty)
                {
                    Console.Error.WriteLine($"RETRY: {label}, empty content; requesting final answer once from same model.");
                    await File.AppendAllTextAsync(Path.Combine(output, "empty-content-retries.jsonl"),
                        JsonSerializer.Serialize(new {
                            label, model, reason = "Empty message.content", utc = DateTimeOffset.UtcNow
                        }) + Environment.NewLine);
                    return await AskAsync(label + "-content-retry", model,
                        system + "\nReturn your complete final report in message.content, not only in reasoning. " +
                        "Do not output internal reasoning. This is a fresh stateless request.",
                        user, maxTokensOverride, retryOnLength: false, retryOnEmpty: false);
                }
                throw new InvalidOperationException("Empty model response after content retry.");
            }
            completed = true;
            Console.WriteLine($"RECEIVED: {label} ({elapsed.Elapsed.TotalSeconds:F1}s, {attempts} HTTP attempt(s))");
            return answer;
        }
        catch (Exception ex)
        {
            await File.WriteAllTextAsync(Path.Combine(output, label + ".error.txt"), ex.ToString());
            if (!reconciled) await budget.RecordErrorAsync(label, model, reserve.Value);
            Console.Error.WriteLine($"FAILED: {label}: {ex.Message}");
            if (ex is HttpRequestException httpError && httpError.StatusCode == System.Net.HttpStatusCode.TooManyRequests && label.StartsWith("round2-", StringComparison.Ordinal))
            {
                round2RateLimited.Add(Participant(label)["critic-".Length..]);
                Console.Error.WriteLine($"EXCLUDED: {Participant(label)}, HTTP 429 in round 2; no model substitution.");
                return null;
            }
            if (ex is HttpRequestException standbyError &&
                (standbyError.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                 standbyError.StatusCode == System.Net.HttpStatusCode.NotFound))
            {
                var statusCode = (int)standbyError.StatusCode.Value;
                var responsePath = Path.Combine(output, label + ".response.json");
                var detail = File.Exists(responsePath) ? ErrorDetail(await File.ReadAllTextAsync(responsePath)) : "No response body";
                return await TryStandbyAsync(label, model, system, user, maxTokensOverride, retryOnLength, statusCode, detail);
            }
            return null;
        }
        finally
        {
            elapsed.Stop();
            // A length retry has its own label and timing record.
            // The parent call's elapsed time includes its child retry, so use
            // the per-attempt records for additive latency comparisons.
            await File.AppendAllTextAsync(Path.Combine(output, "request-timings.jsonl"),
                JsonSerializer.Serialize(new {
                    label, model, elapsedSeconds = Math.Round(elapsed.Elapsed.TotalSeconds, 3),
                    httpAttempts = attempts, completed,
                    includesLengthRetry = !completed && retryOnLength &&
                        File.Exists(Path.Combine(output, label + "-length-retry.request.json"))
                }) + Environment.NewLine);
        }
    }

    public void Dispose() => http.Dispose();
}
