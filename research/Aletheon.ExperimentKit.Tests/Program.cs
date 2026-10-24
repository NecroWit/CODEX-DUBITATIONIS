using Aletheon.ExperimentKit;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception("FAILED: " + message);
    Console.WriteLine("PASS: " + message);
}
var good = ResponseAnalysis.Parse("""{"model":"test/model","choices":[{"finish_reason":"stop","message":{"content":"answer"}}],"usage":{"cost":0.01,"prompt_tokens":10,"completion_tokens":20}}""");
Check(good.Complete && good.Content == "answer" && good.CostUsd == 0.01m, "finished content and usage");
var truncated = ResponseAnalysis.Parse("""{"choices":[{"finish_reason":"length","message":{"content":"partial"}}]}""");
Check(!truncated.Complete && truncated.Status == "truncated", "length is not success");
var empty = ResponseAnalysis.Parse("""{"choices":[{"finish_reason":"stop","message":{"content":"","reasoning":"internal"}}]}""");
Check(!empty.Complete && empty.Status == "empty_content", "reasoning-only is not final");
var missing = ResponseAnalysis.Parse("""{"choices":[]}""");
Check(!missing.Complete && missing.Status == "missing_choices", "missing choices");
var dir = Path.Combine(Path.GetTempPath(), "aletheon-kit-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(dir);
try
{
    var budget = new BudgetLedger(Path.Combine(dir, "ledger.jsonl"), 0.001m, 0.001m);
    var model = new ModelInfo("test", 1m, 1m);
    var reservations = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => Task.Run(
        () => budget.Reserve("parallel", model, new string('x', 200), "hello", 100))));
    Check(reservations.Count(x => x.HasValue) < 20, "parallel budget gate");
    Check(File.Exists(Path.Combine(dir, "ledger.jsonl")), "audit ledger");
}
finally { Directory.Delete(dir, true); }
Console.WriteLine("All offline checks passed.");
