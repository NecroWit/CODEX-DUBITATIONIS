# Aletheon.ExperimentKit (.NET 8)

Reusable C# experiment infrastructure extracted from patterns in `experiments/Aletheon.Trial` without changing its historical behavior.

Modules:
- `OpenRouterTransport` — HTTP transport and bounded retry for transient 429/5xx failures
- `ResponseAnalysis` — structured completion status, `finish_reason`, empty final answer, cost and usage
- `ModelCatalog` — pinned model roster and manually estimated costs
- `BudgetLedger` — thread-safe reservations, per-request and per-run local budget gate, cost reconciliation
- `ExperimentArtifacts` — SHA-256 and JSON persistence

**Important:** The transport never silently substitutes models. A complete answer must have `finish_reason=stop` and nonempty `content`. Reasoning-only output is not treated as final. Provider-side key spending limits remain necessary; local estimates are not billing guarantees. Experiment-specific output validation and scientific scoring belong in the experiment, not the transport.

Existing `experiments/Aletheon.Trial` C# code remains intact. The new EXP-001 runner consumes this library. Later experiments should reference the project instead of copying HTTP code.
