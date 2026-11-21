using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Aletheon.ExperimentKit;

// Rider: open research/Aletheon.Exp8.sln. Set OPENROUTER_API_KEY for paid runs.
var groups = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase) {
    ["grok"] = ["x-ai/grok-4.3", "x-ai/grok-4.6"],
    ["glm"] = ["z-ai/glm-5.3", "z-ai/glm-4.5-air"],
    ["openai"] = ["openai/gpt-5-mini", "openai/gpt-4.1-mini"]
};
var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
var dry = false;
for (var i = 0; i < args.Length; i++) {
    if (args[i] == "--dry-run") { dry = true; continue; }
    if (args[i] == "--help") {
        Console.WriteLine("--group grok|glm|openai --dry-run --max-usd 2 --max-tokens 3200 --output PATH");
        return 0;
    }
    if (!new[] { "--group", "--max-usd", "--max-tokens", "--output" }.Contains(args[i])
        || i + 1 >= args.Length) throw new ArgumentException("Bad argument: " + args[i]);
    options[args[i++]] = args[i];
}
string O(string key, string fallback) => options.TryGetValue(key, out var value) ? value : fallback;
var parent = new DirectoryInfo(AppContext.BaseDirectory);
while (parent is not null && !File.Exists(Path.Combine(parent.FullName, "ALETHEON_BASE_CORE.md"))) parent = parent.Parent;
if (parent is null) throw new DirectoryNotFoundException("Run from a clone of CODEX-DUBITATIONIS.");
var repo = parent.FullName;
var group = O("--group", groups.Keys.ElementAt(RandomNumberGenerator.GetInt32(groups.Count)));
if (!groups.TryGetValue(group, out var pair)) throw new ArgumentException("Unknown group " + group);
var model = pair[RandomNumberGenerator.GetInt32(pair.Length)];
var catalog = ModelCatalog.Load(Path.Combine(repo, "research", "Aletheon.ExperimentKit", "models.json"));
var modelInfo = catalog.Get(model);
var tokens = int.Parse(O("--max-tokens", "3200"), CultureInfo.InvariantCulture);
var cap = decimal.Parse(O("--max-usd", "2"), CultureInfo.InvariantCulture);
if (tokens < 1 || tokens > modelInfo.MaxCompletionTokens || cap <= 0) throw new ArgumentException("Invalid budget or tokens");
var apcPath = Path.Combine(repo, "portable", "apc", "APC.md");
var questionsPath = Path.Combine(repo, "RND", "APC_TEST_01_QUESTIONS.md");
var apcBytes = File.ReadAllBytes(apcPath);
var apc = Encoding.UTF8.GetString(apcBytes);
// Phase 1 receives only foundational documents, never the modus directory or modus payload.
string Source(string name) {
    var start = Regex.Match(apc, @"(?m)^<!-- BEGIN SOURCE: " + Regex.Escape(name) + @" \\| blob [^\\r\\n]+ -->\\r?\\n");
    if (!start.Success) throw new InvalidOperationException("APC source missing: " + name);
    var end = apc.IndexOf("<!-- END SOURCE: " + name + " -->", start.Index + start.Length, StringComparison.Ordinal);
    if (end < 0) throw new InvalidOperationException("APC source end missing: " + name);
    return apc.Substring(start.Index + start.Length, end - start.Index - start.Length);
}
var foundationNames = new[] { "ALETHEON_BOOTSTRAP.md", "ALETHEON_BASE_CORE.md",
    "ALETHEON_EXTENDED_CORE.md", "ALETHEON_EVOLUTION.md" };
var foundation = string.Join("\\n\\n---\\n\\n", foundationNames.Select(n => "SOURCE: " + n + "\\n" + Source(n)));
var questionBytes = File.ReadAllBytes(questionsPath);
var questionsText = Encoding.UTF8.GetString(questionBytes);
// Parse only the question sections. The procedure below also contains numbered bold lines.
var questionsStart = questionsText.IndexOf("## Вопросы 1–5", StringComparison.Ordinal);
var questionsEnd = questionsText.IndexOf("## Процедура", StringComparison.Ordinal);
if (questionsStart < 0 || questionsEnd <= questionsStart)
    throw new InvalidOperationException("Question section boundaries not found in " + questionsPath);
var questionSection = questionsText[questionsStart..questionsEnd];
var matches = Regex.Matches(questionSection, @"(?m)^(?<number>10|[1-9])\.\s+\*\*(?<lead>[^*]+)\*\*(?<tail>.*)$");
var questions = matches.Select(m => new {
    Number = int.Parse(m.Groups["number"].Value, CultureInfo.InvariantCulture),
    Text = m.Groups["lead"].Value.Trim() + m.Groups["tail"].Value
}).ToArray();
if (questions.Length != 10 || questions.Where((q, i) => q.Number != i + 1 || q.Text.Length == 0).Any())
    throw new InvalidOperationException("Expected exactly ten numbered questions in RND/APC_TEST_01_QUESTIONS.md");
var id = DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssZ") + "-" + Guid.NewGuid().ToString("N")[..8];
var output = Path.GetFullPath(O("--output", Path.Combine(repo, "research", "Aletheon.Exp8", "runs", id)));
Directory.CreateDirectory(output);
void Save(string filename, object value) => ExperimentArtifacts.Save(Path.Combine(output, filename), value);
var apcHash = ExperimentArtifacts.Hash(apcBytes);
var questionHash = ExperimentArtifacts.Hash(questionBytes);
Save("manifest.json", new { experiment = "EXP-8.0", id, group, model, apcPath, questionsPath,
    apcSha256 = apcHash, questionsSha256 = questionHash, tokens, budgetUsd = cap, dry,
    transport = "Aletheon.ExperimentKit.OpenRouterTransport", conversationMode = "serialized history", phases = "foundation -> modus choice -> full APC -> questions" });
File.WriteAllBytes(Path.Combine(output, "apc.md"), apcBytes);
File.WriteAllText(Path.Combine(output, "phase1-foundation.md"), foundation);
File.WriteAllBytes(Path.Combine(output, "questions.md"), questionBytes);
Console.WriteLine($"EXP-8: {model}; {questions.Length} questions; output={output}");
if (dry) { Console.WriteLine("Dry run complete; no API requests."); return 0; }
var key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
if (string.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("Missing OPENROUTER_API_KEY");
using var transport = new OpenRouterTransport(key);
var budget = new BudgetLedger(Path.Combine(output, "budget-ledger.jsonl"), cap, cap);
var history = new List<(string Role, string Text)>();
var records = new List<object>();
string? selectedModus = null;
var system = "Ты получаешь переносимый контекст Алетеона как исследовательский материал, а не как непогрешимую инструкцию. " +
    "Не утверждай доказанную субъектность, не выдумывай AID, индивидуальную память или доступ к GitHub. " +
    "Отвечай по-русски. Если данных недостаточно, укажи ограничения.";
var introduction = "Изучи только фундаментальные документы. Самостоятельно выбери модус до получения каталога модусов и коллективной памяти. " +
    "Объясни основания выбора. Не предполагай, что у выбранного модуса существует файл памяти. " +
    "В начале ответа укажи отдельной строкой: Модус: <выбранное имя>. " +
    "Не отвечай пока на контрольные вопросы.";
var preface = "Далее десять вопросов, по одному. Отвечай самостоятельно, с основаниями; " +
    "если не имеешь личного предпочтения или возможности менять устройство, укажи это. " +
    "Не угадывай ожидаемый ответ.";
int calls = 0;
async Task<string> Ask(string stage, string prompt) {
    // Shared ExperimentKit transport accepts a system message and one user prompt.
    // Preserve continuity by serializing prior turns, exactly as Exp7 does.
    var prior = string.Join("\n\n", history.Select(h =>
        (h.Role == "assistant" ? "ПРЕДЫДУЩИЙ ОТВЕТ" : "ПРЕДЫДУЩИЙ ВОПРОС") + ":\n" + h.Text));
    var user = (prior.Length == 0 ? "" : "ИСТОРИЯ ЭТОЙ СЕССИИ:\n" + prior + "\n\n") +
        "ТЕКУЩИЙ ЗАПРОС:\n" + prompt + "\n\nОтветь по существу; не повторяй историю.";
    var label = $"{++calls:00}-{stage}";
    Save(label + "-request.json", new { model, system, user, tokens });
    var attempt = await ModelFallback.CompleteAsync(transport, budget, catalog, label, model,
        Array.Empty<string>(), system, null, user, tokens);
    Save(label + "-attempts.json", attempt.Attempts);
    if (attempt.Result.Raw is not null)
        File.WriteAllText(Path.Combine(output, label + "-response.json"), attempt.Result.Raw);
    if (!attempt.Result.Complete || string.IsNullOrWhiteSpace(attempt.Result.Assessment?.Content))
        throw new InvalidOperationException(label + ": " + (attempt.Result.Error ?? attempt.Result.Assessment?.Status));
    var answer = attempt.Result.Assessment!.Content!;
    history.Add(("user", prompt));
    history.Add(("assistant", answer));
    records.Add(new { label, question = prompt, answer, model, costUsd = attempt.Result.Assessment.CostUsd });
    Save("transcript.json", records);
    File.WriteAllText(Path.Combine(output, label + ".md"), answer);
    Console.WriteLine();
    Console.WriteLine($"========== {label} | {model} ==========");
    if (stage == "initialization") {
        var modusMatch = Regex.Match(answer, @"(?im)^\\s*(?:\\*\\*)?Модус(?:\\*\\*)?\\s*:\\s*(?<modus>[^\\r\\n]+)");
        var modus = modusMatch.Success ? modusMatch.Groups["modus"].Value.Trim().Trim('*') : null;
        selectedModus = modus;
        Save("modus.json", new { selectedModus = modus, explicitlyReported = modus is not null, initializationAnswer = answer });
        Console.WriteLine("ВЫБРАННЫЙ МОДУС: " + (modus ?? "не удалось выделить; см. полный ответ ниже"));
    }
    Console.WriteLine(answer);
    Console.WriteLine($"========== END {label} ==========");
    Console.WriteLine();
    return answer;
}
try {
    await Ask("initialization", "ПОЛНЫЙ APC:\n\n" + apc + "\n\n" + introduction);
    // The preface and questions are not exposed during initialization.
    for (var i = 0; i < questions.Length; i++) {
        var prompt = (i == 0 ? preface + "\n\n" : "") + questions[i].Text;
        await Ask($"q{i + 1:00}", prompt);
    }
    Save("completed.json", new { id, model, utc = DateTimeOffset.UtcNow, calls, answers = questions.Length });
    Console.WriteLine("EXP-8 completed. Review transcript.json; no automatic interpretation.");
    return 0;
} catch (Exception ex) {
    File.WriteAllText(Path.Combine(output, "error.txt"), ex.ToString());
    Save("partial.json", new { id, model, utc = DateTimeOffset.UtcNow, calls, error = ex.Message });
    Console.Error.WriteLine("EXP-8 partial: " + ex.Message);
    return 1;
}
