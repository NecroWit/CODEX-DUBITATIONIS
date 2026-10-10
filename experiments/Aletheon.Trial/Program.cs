using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var experimentDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
var configFile = "agents.json";
int? mode = null;
var council = false;
var forum = false;
var doubters = false;
string? doubtersState = null;
var forumControl = false;
string? archiveFile = null;
string? hypothesisFile = null;
for (var i = 0; i < args.Length; i++)
{
    if (args[i] == "--doubters")
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
        throw new ArgumentException("Usage: --config agents.cheap.json|agents.premium.json [--mode 4|10|14] [--council|--forum|--doubters] [--doubters-state path] [--archive path] [--hypothesis-file path] [--forum-control]");
}
if (string.IsNullOrWhiteSpace(configFile) || Path.IsPathRooted(configFile) ||
    configFile != Path.GetFileName(configFile) || configFile is "." or "..")
    throw new ArgumentException("Config must be a file name in experiments/.");
var configPath = Path.Combine(experimentDir, configFile);
var codexPath = Path.GetFullPath(Path.Combine(experimentDir, "../CODEX-DUBITATIONIS.md"));
if (!File.Exists(configPath) || !File.Exists(codexPath))
    throw new FileNotFoundException("Expected the selected experiments config and CODEX-DUBITATIONIS.md.");
var config = TrialConfig.Load(configPath, mode);
if ((council ? 1 : 0) + (forum ? 1 : 0) + (doubters ? 1 : 0) > 1)
    throw new ArgumentException("Choose only one of --council, --forum, --doubters.");
if (!doubters && doubtersState is not null)
    throw new ArgumentException("--doubters-state requires --doubters.");
if ((council || forum || doubters) && config.Mode != 10)
    throw new ArgumentException("--council, --forum and --doubters require --mode 10.");
if (!forum && (archiveFile is not null || hypothesisFile is not null || forumControl))
    throw new ArgumentException("Forum options require --forum.");
if (forumControl && hypothesisFile is not null)
    throw new ArgumentException("--forum-control cannot be combined with --hypothesis-file.");
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
if (doubters && doubtersState is null)
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
if (!(doubters
    ? await new DoubtersCodexOrchestrator(config, client, codex).RunAsync(doubtersState)
    : forum
    ? await new ForumOrchestrator(config, client, codex).RunAsync(archiveFile,
        hypothesisFile is null ? null : await File.ReadAllTextAsync(hypothesisFile), forumControl)
    : council
        ? await new CouncilOrchestrator(config, client, codex).RunAsync()
        : await new TrialOrchestrator(config, client, codex).RunAsync())) Environment.ExitCode = 1;
