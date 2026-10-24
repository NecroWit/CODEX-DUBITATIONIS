# Shared Python experiment infrastructure

Extracted/adapted from the earlier .NET 8 experiments in `experiments/Aletheon.Trial`:
- `OpenRouterClient.cs` → `ask` and `parse_response`
- `BudgetManager.cs` → `Budget`
- `TrialConfig.cs` / `agents.*.json` → `load_model_catalog` and `models.json`
- `RatingsReport.cs`: experiment-specific score parser; **not** reused for T1/H1 because its rubric is unrelated.

Guarantees: no model substitution, no accepting empty/truncated replies as success, raw provider output retained by caller, bounded HTTP retries, per-run thread-safe estimated budget reservations, recorded actual costs.

Limitations: model prices are manually maintained estimates; token budget includes reasoning tokens; a retry is a new paid request; local cost gate cannot guarantee OpenRouter billing cap. Set account/key limit. Callers must validate their own output schema. No third-party dependencies.
