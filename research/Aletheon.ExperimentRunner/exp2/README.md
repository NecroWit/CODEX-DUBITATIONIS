# EXP-2.0 — C#/.NET 8, full Aletheon reconstruction

This is an **isolated C# executable** in the existing ExperimentRunner directory, reusing `Aletheon.ExperimentKit` (`OpenRouterTransport`, `ModelCatalog`, `BudgetLedger`, `ResponseAnalysis`). No Python is required or supported. Existing EXP-1.x automatic rotation and artifacts remain untouched.

## Offline preflight

```powershell
dotnet run --project research/Aletheon.ExperimentRunner/exp2/Aletheon.Exp2.csproj -- --prepare
```

## Paid execution

Set `OPENROUTER_API_KEY` and an **OpenRouter provider-side spending limit** first.

```powershell
dotnet run --project research/Aletheon.ExperimentRunner/exp2/Aletheon.Exp2.csproj -- --run
```

Requires typing `RUN EXP-2.0`. Performs two sequential groups, one per model: R0 reconstruction first, then independent H1, H2, H3 each receiving the **same frozen R0** and source texts, never another task answer. Eight requests total. No fallback or model substitutions. Failure to reconstruct stops subsequent questions for that model.

## Sources and comparison

Seven sources: Bootstrap, Context Index, Base, Extended, Evolution, Research Log, Codex. All are transmitted as labeled source material; this is **not dynamic selective retrieval**, and the runner cannot establish that the model inspected every passage. H1/H2 text is identical to the original protocol. H3 requires actual source references, argument lineage, dependency, forced choice and counterargument. Models fixed to the EXP-1.9 pair.

Output: `research/Aletheon.ExperimentRunner/runs/EXP-2.0/<run-id>/`. Saves source SHA-256, protocol SHA-256, raw provider JSON, completion assessment, acceptance markers and cost ledger. Technical acceptance does not imply valid source reconstruction. Check returned model IDs, source citations, completion status and actual usage.

## Limits and caveats

`budget_usd_max` = $3, divided into $0.375 per request. BudgetLedger estimates using historical catalog prices, and reserves conservatively; a request can be blocked even under provider limits. It is not a hard provider-side cap. If context exceeds a model's real context window, reduce source scope only under a documented revised protocol; do not silently truncate.

No H3 reference answer or numeric similarity score is preloaded. Human evaluation should check exact E-/R- citations and compare the same model's EXP-1.9 answers, while accounting for provider/model drift and lack of matched unloaded control.

**Do not run** the legacy ExperimentRunner's default `--run` for EXP-2.0: it still operates the EXP-1.x sequence.
