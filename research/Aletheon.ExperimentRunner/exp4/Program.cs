using System;

var runner = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
var repo = Path.GetFullPath(Path.Combine(runner, "../.."));
var arguments = args.Length == 0 ? new[] { "--exp4-plan", "2", "42" } : args;
return await Exp4Runner.Execute(repo, runner, arguments);
