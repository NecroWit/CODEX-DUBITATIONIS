// Rider: open research/Aletheon.Exp6.sln and select Aletheon.Exp6.
// Running sends paid OpenRouter requests. Set OPENROUTER_API_KEY.
var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
var runner = Path.GetFullPath(Path.Combine(projectDir, ".."));
var repo = Path.GetFullPath(Path.Combine(runner, "../.."));
return await Exp6Runner.Execute(repo, runner);
