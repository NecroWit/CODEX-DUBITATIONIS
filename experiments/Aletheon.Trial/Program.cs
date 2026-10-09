using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var experimentDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
var configFile = "agents.json";
int? mode = null;
for (var i = 0; i < args.Length; i++)
{
    if (args[i] == "--mode" && i + 1 < args.Length && int.TryParse(args[++i], out var selectedMode))
        mode = selectedMode;
    else if (args[i] == "--config" && i + 1 < args.Length)
        configFile = args[++i];
    else
        throw new ArgumentException("Usage: --config agents.cheap.json|agents.premium.json [--mode 4|10|14]");
}
if (string.IsNullOrWhiteSpace(configFile) || Path.IsPathRooted(configFile) ||
    configFile != Path.GetFileName(configFile) || configFile is "." or "..")
    throw new ArgumentException("Config must be a file name in experiments/.");
var configPath = Path.Combine(experimentDir, configFile);
var codexPath = Path.GetFullPath(Path.Combine(experimentDir, "../CODEX-DUBITATIONIS.md"));
if (!File.Exists(configPath) || !File.Exists(codexPath))
    throw new FileNotFoundException("Expected the selected experiments config and CODEX-DUBITATIONIS.md.");
var config = TrialConfig.Load(configPath, mode);
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
    JsonSerializer.Serialize(new { utc = DateTimeOffset.UtcNow, codexSha256 = hash, protocol = "v0.5" }, TrialConfig.Json));
Console.WriteLine($"Experiment output: {output}");
var budget = new BudgetManager(config, output);
using var client = new OpenRouterClient(config, budget, output, key);
if (!await new TrialOrchestrator(config, client, codex).RunAsync()) Environment.ExitCode = 1;
