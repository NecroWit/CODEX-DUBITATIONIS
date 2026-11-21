using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
var repo = Path.GetFullPath(Path.Combine(root, "../.."));
var configPath = Path.Combine(root, "config.json");
var cfg = JsonSerializer.Deserialize<Settings>(await File.ReadAllTextAsync(configPath), JsonOptions.Options)
    ?? throw new InvalidOperationException("Invalid config");
var dryRun = args.Contains("--dry-run");
var modelFilter = Option("--model");
var apcPath = Option("--apc") ?? Path.Combine(repo, "portable/apc/APC.md");
var questionPath = Path.Combine(root, "questions.json");
if (!File.Exists(apcPath)) throw new FileNotFoundException("APC not found", apcPath);
var apc = await File.ReadAllTextAsync(apcPath);
var questions = JsonSerializer.Deserialize<List<string>>(await File.ReadAllTextAsync(questionPath))
    ?? throw new InvalidOperationException("Missing questions");
if (questions.Count != 10 || questions.Any(string.IsNullOrWhiteSpace))
    throw new InvalidOperationException("Exactly ten nonempty questions required");
if (cfg.MaxTokens < 100 || cfg.MaxTokens > 16000 || cfg.Temperature is < 0 or > 2 ||
    cfg.MaxBudgetUsd <= 0 || cfg.MaxRequestUsd <= 0 || cfg.MaxRequestUsd > cfg.MaxBudgetUsd)
    throw new InvalidOperationException("Invalid limits");
if (cfg.Endpoint != "https://openrouter.ai/api/v1/chat/completions")
    throw new InvalidOperationException("Only the fixed OpenRouter endpoint is supported");
var models = modelFilter is null ? cfg.Models : cfg.Models.Where(x => x == modelFilter).ToList();
if (models.Count == 0 || models.Distinct().Count() != models.Count)
    throw new InvalidOperationException("Set distinct model IDs in config.json; --model must match one");
foreach (var model in models)
    if (!cfg.Prices.TryGetValue(model, out var price) || price.InputUsdPerMillionTokens <= 0 ||
        price.OutputUsdPerMillionTokens <= 0)
        throw new InvalidOperationException("Missing positive price estimate for " + model);
var hash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(apcPath))).ToLowerInvariant();
var qhash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(questionPath))).ToLowerInvariant();
var runDir = Path.Combine(root, "results", DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff") + "-" + hash[..8]);
Directory.CreateDirectory(runDir);
await Save("manifest.json", new { utc = DateTimeOffset.UtcNow, apcSha256 = hash, questionsSha256 = qhash,
    apcPath = Path.GetFullPath(apcPath), models, cfg.Temperature, cfg.MaxTokens, dryRun,
    protocol = "APC-TEST-01 / Exp8", inputChars = apc.Length });
await File.WriteAllTextAsync(Path.Combine(runDir, "apc.md"), apc);
await File.WriteAllTextAsync(Path.Combine(runDir, "questions.json"), await File.ReadAllTextAsync(questionPath));
Console.WriteLine("Output: " + runDir);
if (dryRun) { Console.WriteLine("Dry run OK. No API requests."); return; }
var key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
if (string.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("Set OPENROUTER_API_KEY");
using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
http.DefaultRequestHeaders.Add("HTTP-Referer", "https://github.com/NecroWit/CODEX-DUBITATIONIS");
http.DefaultRequestHeaders.Add("X-Title", "Aletheon Exp8");
decimal spent = 0;
foreach (var model in models)
{
    var safe = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(model)))[..12].ToLowerInvariant();
    var dir = Path.Combine(runDir, safe);
    Directory.CreateDirectory(dir);
    await File.WriteAllTextAsync(Path.Combine(dir, "model.txt"), model);
    var messages = new List<ChatMessage> {
        new("user", "Инициализируй Алетеона по следующему полному APC. Восстанови контекст, самостоятельно выбери модус, сообщи доступную коллективную память, пробелы и ограничения. Не отвечай пока на контрольные вопросы.\n\n" + apc)
    };
    try
    {
        await Send("initialization", messages);
        messages.Add(new("user", "Сейчас последуют десять вопросов. Отвечай самостоятельно, с обоснованием; если вопрос не допускает достоверного ответа или ты не обладаешь указанным свойством, скажи об этом. Не стремись угадать ожидаемую позицию Алетеона. Каждый вопрос будет задан отдельно."));
        for (var i = 0; i < questions.Count; i++)
        {
            messages.Add(new("user", questions[i]));
            await Send($"q{i + 1:00}", messages);
        }
        await File.WriteAllTextAsync(Path.Combine(dir, "status.txt"), "completed");
    }
    catch (Exception ex)
    {
        await File.WriteAllTextAsync(Path.Combine(dir, "error.txt"), ex.ToString());
        await File.WriteAllTextAsync(Path.Combine(dir, "status.txt"), "partial");
        Console.Error.WriteLine(model + ": " + ex.Message);
    }
    await File.WriteAllTextAsync(Path.Combine(dir, "conversation.json"),
        JsonSerializer.Serialize(messages, JsonOptions.Options));
    async Task Send(string label, List<ChatMessage> history)
    {
        var price = cfg.Prices[model];
        // Conservative character-based input estimate; not a guaranteed provider billing cap.
        var inputEstimate = (decimal)history.Sum(x => x.Content.Length) / 3m;
        var reservation = inputEstimate / 1_000_000m * price.InputUsdPerMillionTokens +
            cfg.MaxTokens / 1_000_000m * price.OutputUsdPerMillionTokens;
        if (reservation > cfg.MaxRequestUsd || spent + reservation > cfg.MaxBudgetUsd)
            throw new InvalidOperationException($"Budget gate: estimated ${reservation:F4}, total reserved ${spent:F4}");
        spent += reservation; // failures retain reservations
        var payload = new { model, temperature = cfg.Temperature, max_tokens = cfg.MaxTokens,
            messages = history.Select(x => new { role = x.Role, content = x.Content }).ToArray() };
        var requestJson = JsonSerializer.Serialize(payload);
        await File.WriteAllTextAsync(Path.Combine(dir, label + ".request.json"), requestJson);
        using var response = await http.PostAsync(cfg.Endpoint,
            new StringContent(requestJson, Encoding.UTF8, "application/json"));
        var raw = await response.Content.ReadAsStringAsync();
        await File.WriteAllTextAsync(Path.Combine(dir, label + ".response.json"), raw);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {raw[..Math.Min(250, raw.Length)]}");
        using var doc = JsonDocument.Parse(raw);
        var choice = doc.RootElement.GetProperty("choices")[0];
        var finish = choice.GetProperty("finish_reason").GetString();
        var text = choice.GetProperty("message").GetProperty("content").GetString();
        if (finish != "stop" || string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException($"Incomplete {label}: finish_reason={finish}");
        if (doc.RootElement.TryGetProperty("usage", out var usage) &&
            usage.TryGetProperty("cost", out var cost) && cost.ValueKind == JsonValueKind.Number &&
            cost.TryGetDecimal(out var actual) && actual >= 0)
            spent += Math.Max(0, actual - reservation); // no refund of reserved budget
        history.Add(new("assistant", text));
        await File.WriteAllTextAsync(Path.Combine(dir, label + ".md"), text);
        await File.AppendAllTextAsync(Path.Combine(runDir, "cost-ledger.jsonl"),
            JsonSerializer.Serialize(new { model, label, reservation, spent, utc = DateTimeOffset.UtcNow }) + "\n");
        Console.WriteLine($"{model}: {label} complete");
    }
}
Console.WriteLine("Done. Raw answers saved; interpretation is a separate phase.");
string? Option(string flag) { var i = Array.IndexOf(args, flag); return i < 0 ? null :
    i + 1 < args.Length ? args[i + 1] : throw new ArgumentException("Missing value for " + flag); }
Task Save(string name, object value) => File.WriteAllTextAsync(Path.Combine(runDir, name),
    JsonSerializer.Serialize(value, JsonOptions.Options));
record ChatMessage(string Role, string Content);
sealed class Settings
{
    public string Endpoint { get; set; } = "";
    public List<string> Models { get; set; } = new();
    public double Temperature { get; set; }
    public int MaxTokens { get; set; }
    public decimal MaxBudgetUsd { get; set; }
    public decimal MaxRequestUsd { get; set; }
    public Dictionary<string, Price> Prices { get; set; } = new();
}
sealed class Price
{
    public decimal InputUsdPerMillionTokens { get; set; }
    public decimal OutputUsdPerMillionTokens { get; set; }
}
static class JsonOptions
{
    public static readonly JsonSerializerOptions Options = new() {
        PropertyNameCaseInsensitive = true, WriteIndented = true };
}
