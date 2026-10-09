using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var experimentDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
var configPath = Path.Combine(experimentDir, "agents.json");
var codexPath = Path.GetFullPath(Path.Combine(experimentDir, "../CODEX-DUBITATIONIS.md"));
if (!File.Exists(configPath) || !File.Exists(codexPath))
    throw new FileNotFoundException("Expected experiments/agents.json and CODEX-DUBITATIONIS.md.");
var config = TrialConfig.Load(configPath);
var key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
if (string.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("Set OPENROUTER_API_KEY.");
var codex = await File.ReadAllTextAsync(codexPath);
var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(codex))).ToLowerInvariant();
var output = Path.Combine(experimentDir, "results",
    DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff") + "-" + hash[..8]);
Directory.CreateDirectory(output);
await File.WriteAllTextAsync(Path.Combine(output, "codex.md"), codex);
await File.WriteAllTextAsync(Path.Combine(output, "config.json"), JsonSerializer.Serialize(config, TrialConfig.Json));
await File.WriteAllTextAsync(Path.Combine(output, "manifest.json"),
    JsonSerializer.Serialize(new { utc = DateTimeOffset.UtcNow, codexSha256 = hash, protocol = "v0.4" }, TrialConfig.Json));
Console.WriteLine($"Experiment output: {output}");
var budget = new BudgetManager(config, output);
using var client = new OpenRouterClient(config, budget, output, key);
if (!await new TrialOrchestrator(config, client, codex).RunAsync()) Environment.ExitCode = 1;
