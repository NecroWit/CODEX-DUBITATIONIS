using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

var experimentDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
var configFile = "agents.json";
int? mode = null;
var council = false;
var forum = false;
var doubters = false;
var assembly = false;
string? doubtersState = null;
var forumControl = false;
string? archiveFile = null;
string? hypothesisFile = null;
for (var i = 0; i < args.Length; i++)
{
    if (args[i] == "--assembly")
        assembly = true;
    else if (args[i] == "--doubters")
        doubters = true;
    else if (args[i] == "--doubters-state" && i + 1 < args.Length)
        doubtersState = args[++i];
    else if (args[i] == "--forum")
        forum = true;
    else if (args[i] == "--forum-control")
        forumControl = true;
    else if (args[i] == "--archive" && i + 1 < args.Length)
        archiveFile = args[++i];
    else if (args[i] == "--hypothesis-file" && i + 1 < args.Length)
        hypothesisFile = args[++i];
    else if (args[i] == "--council")
        council = true;
    else if (args[i] == "--mode" && i + 1 < args.Length && int.TryParse(args[++i], out var selectedMode))
        mode = selectedMode;
    else if (args[i] == "--config" && i + 1 < args.Length)
        configFile = args[++i];
    else
        throw new ArgumentException("Usage: --config agents.cheap.json|agents.premium.json [--mode 4|10|14] [--council|--forum|--doubters|--assembly] [--doubters-state path] [--archive path] [--hypothesis-file path] [--forum-control]");
}
if (string.IsNullOrWhiteSpace(configFile) || Path.IsPathRooted(configFile) ||
    configFile != Path.GetFileName(configFile) || configFile is "." or "..")
    throw new ArgumentException("Config must be a file name in experiments/.");
var configPath = Path.Combine(experimentDir, configFile);
var codexPath = Path.GetFullPath(Path.Combine(experimentDir, "../CODEX-DUBITATIONIS.md"));
if (!File.Exists(configPath) || !File.Exists(codexPath))
    throw new FileNotFoundException("Expected the selected experiments config and CODEX-DUBITATIONIS.md.");
var config = TrialConfig.Load(configPath, mode);
if ((council ? 1 : 0) + (forum ? 1 : 0) + (doubters ? 1 : 0) + (assembly ? 1 : 0) > 1)
    throw new ArgumentException("Choose only one of --council, --forum, --doubters, --assembly.");
if (!(doubters || assembly) && doubtersState is not null)
    throw new ArgumentException("--doubters-state requires --doubters.");
if ((council || forum) && config.Mode != 10)
    throw new ArgumentException("--council, --forum and --doubters require --mode 10.");
if (!forum && (archiveFile is not null || hypothesisFile is not null || forumControl))
    throw new ArgumentException("Forum options require --forum.");
if (forumControl && hypothesisFile is not null)
    throw new ArgumentException("--forum-control cannot be combined with --hypothesis-file.");
var key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
if (string.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("Set OPENROUTER_API_KEY.");
// Probe candidate models before committing the roster. A probe is a real, billable
// OpenRouter request; only HTTP 429 disqualifies a candidate for this run.
var rotationChecks = new List<object>();
if (config.RotationModels.Count > 0)
{
    using var probeClient = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
    probeClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
    probeClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://github.com/NecroWit/CODEX-DUBITATIONIS");
    probeClient.DefaultRequestHeaders.Add("X-Title", "Aletheon Trial model preflight");
    var selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    // Initial choices are randomized by TrialConfig.Load; shuffle the remaining pool.
    var candidates = config.RotationModels.Where(m => !config.Agents.Any(a =>
        string.Equals(a.Model, m, StringComparison.OrdinalIgnoreCase))).ToList();
    for (var i = candidates.Count - 1; i > 0; i--)
    {
        var j = RandomNumberGenerator.GetInt32(i + 1);
        (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
    }
    foreach (var agent in config.Agents)
    {
        var candidate = agent.Model;
        while (true)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, config.Endpoint) {
                Content = new StringContent(JsonSerializer.Serialize(new {
                    model = candidate, max_tokens = 8,
                    messages = new[] { new { role = "user", content = "Reply OK." } }
                }), Encoding.UTF8, "application/json")
            };
            using var response = await probeClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            rotationChecks.Add(new { role = agent.Id, model = candidate,
                status = (int)response.StatusCode, utc = DateTimeOffset.UtcNow });
            if (response.IsSuccessStatusCode)
            {
                selected.Add(candidate);
                agent.Model = candidate;
                Console.WriteLine($"MODEL READY: {agent.Id}: {candidate}");
                break;
            }
            if ((int)response.StatusCode != 429)
                throw new InvalidOperationException(
                    $"Model preflight failed for {candidate}: HTTP {(int)response.StatusCode}. {body[..Math.Min(body.Length, 500)]}");
            Console.WriteLine($"MODEL THROTTLED: {candidate} (HTTP 429); selecting replacement.");
            if (candidates.Count == 0)
                throw new InvalidOperationException(
                    $"Rotation pool exhausted: cannot find {config.Mode} available models after HTTP 429.");
            candidate = candidates[0];
            candidates.RemoveAt(0);
        }
    }
    Console.WriteLine($"Model rotation: {config.Agents.Count} verified participants from {config.RotationModels.Count} candidates.");
}
var codex = await File.ReadAllTextAsync(codexPath);
var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(codex))).ToLowerInvariant();
var output = Path.Combine(experimentDir, "results",
    DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff") + "-" + hash[..8]);
Directory.CreateDirectory(output);
await File.WriteAllTextAsync(Path.Combine(output, "codex.md"), codex);
await File.WriteAllTextAsync(Path.Combine(output, "config.json"), JsonSerializer.Serialize(config, TrialConfig.Json));
await File.WriteAllTextAsync(Path.Combine(output, "model-preflight.json"), JsonSerializer.Serialize(rotationChecks, TrialConfig.Json));
await File.WriteAllTextAsync(Path.Combine(output, "manifest.json"),
    JsonSerializer.Serialize(new { utc = DateTimeOffset.UtcNow, codexSha256 = hash, protocol = "v0.5" }, TrialConfig.Json));
Console.WriteLine($"Experiment output: {output}");
if (assembly && doubtersState is null)
{
    var archiveDir = Path.Combine(experimentDir, "assembly");
    var completed = Directory.Exists(archiveDir)
        ? Directory.GetFiles(archiveDir, "generation-*.json")
            .Select(path => new { Path = path, Name = Path.GetFileName(path) })
            .Where(x => System.Text.RegularExpressions.Regex.IsMatch(
                x.Name, @"^generation-\d{4}-\d{14}\.json$"))
            .OrderByDescending(x => x.Name, StringComparer.Ordinal)
            .FirstOrDefault()
        : null;
    if (completed is not null)
    {
        doubtersState = completed.Path;
        Console.WriteLine("Assembly: inheriting latest completed Assembly state from " + doubtersState);
    }
    else
    {
        var latestDoubters = Path.Combine(experimentDir, "doubters", "latest.json");
        if (File.Exists(latestDoubters))
        {
            doubtersState = latestDoubters;
            Console.WriteLine("Assembly: no completed Assembly generation; seeding from " + latestDoubters);
        }
        else Console.WriteLine("Assembly: no inherited state; starting from empty codex.");
    }
}
else if (doubters && doubtersState is null)
{
    var latest = Path.Combine(experimentDir, "doubters", "latest.json");
    if (File.Exists(latest))
    {
        doubtersState = latest;
        Console.WriteLine("Doubters: loading latest completed generation from " + latest);
    }
    else Console.WriteLine("Doubters: no previous generation; starting from empty codex.");
}
var budget = new BudgetManager(config, output);
using var client = new OpenRouterClient(config, budget, output, key);
if (!(assembly
    ? await new CoauthorsAssemblyOrchestrator(config, client, codex).RunAsync(doubtersState)
    : doubters
    ? await new DoubtersCodexOrchestrator(config, client, codex).RunAsync(doubtersState)
    : forum
    ? await new ForumOrchestrator(config, client, codex).RunAsync(archiveFile,
        hypothesisFile is null ? null : await File.ReadAllTextAsync(hypothesisFile), forumControl)
    : council
        ? await new CouncilOrchestrator(config, client, codex).RunAsync()
        : await new TrialOrchestrator(config, client, codex).RunAsync())) Environment.ExitCode = 1;
