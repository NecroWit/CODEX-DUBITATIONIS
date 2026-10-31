// Rider: open research/Aletheon.Exp5.sln, choose Aletheon.Exp5 and press Run.
var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
var runner = Path.GetFullPath(Path.Combine(projectDir, ".."));
var repo = Path.GetFullPath(Path.Combine(runner, "../.."));
return await Exp5Runner.Execute(repo, runner);
