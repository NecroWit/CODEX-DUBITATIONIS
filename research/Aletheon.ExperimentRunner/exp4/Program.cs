using System.Text.Json;

// Rider: open Aletheon.EXP4.sln, choose Aletheon.Exp4 and press Run.
// By default the experiment runs; --exp4-plan remains an offline preview.
var runner = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
var repo = Path.GetFullPath(Path.Combine(runner, "../.."));
var protocolPath = Path.Combine(runner, "exp4", "EXP-4.protocol.json");
using var protocol = JsonDocument.Parse(File.ReadAllText(protocolPath));
var settings = protocol.RootElement.GetProperty("execution");
var generations = settings.GetProperty("generations").GetInt32();
var seed = settings.GetProperty("seed").GetInt32();
var arguments = args.Length == 0
    ? new[] { "--exp4-run", generations.ToString(), seed.ToString() }
    : args;
return await Exp4Runner.Execute(repo, runner, arguments);
