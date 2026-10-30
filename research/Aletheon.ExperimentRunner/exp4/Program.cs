using System.Text.Json;

// Rider: open research/Aletheon.Exp4.sln, choose Aletheon.Exp4 and press Run.
// Resolve paths from the compiled executable, not Rider's working directory.
var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
var runner = Path.GetFullPath(Path.Combine(projectDir, ".."));
var repo = Path.GetFullPath(Path.Combine(runner, "../.."));
var protocolPath = Path.Combine(projectDir, "EXP-4.protocol.json");
if (!File.Exists(protocolPath))
{
    Console.Error.WriteLine($"EXP-4 protocol not found: {protocolPath}");
    return 1;
}

using var protocol = JsonDocument.Parse(File.ReadAllText(protocolPath));
var settings = protocol.RootElement.GetProperty("execution");
var generations = settings.GetProperty("generations").GetInt32();
var seed = settings.GetProperty("seed").GetInt32();
var arguments = args.Length == 0
    ? new[] { "--exp4-run", generations.ToString(), seed.ToString() }
    : args;
return await Exp4Runner.Execute(repo, runner, arguments);
