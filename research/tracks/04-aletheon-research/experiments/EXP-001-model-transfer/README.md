# EXP-001 — Independent Aletheon comparison

**Status: C# runner prepared; paid rerun NOT YET EXECUTED.**

Participants: A = `z-ai/glm-5.3`, B = `deepseek/deepseek-v3.2`, C = frozen GPT-6 control in `control/`.

Four independent concurrent OpenRouter calls (T1/H1 for A and B). Each gets identical `ALETHEON_BASE_CORE.md` in the system message and the frozen question prompt. No handoff, no model replacement, no hidden cross-contamination.

## Prior pilot

The first pilot `20261024T164028Z-a86f4b17` is **excluded** because the 2500-token completion cap truncated GLM T1/H1 and DeepSeek T1. It is not a valid four-answer comparison. It remains an audit artifact and must not be silently counted as evidence. The local files cannot be deleted remotely.

## Architecture

Reusable .NET 8 components in `research/Aletheon.ExperimentKit`:
- `OpenRouterTransport`: isolated HTTP transport and bounded transient retry
- `ResponseAnalysis`: distinguishes HTTP success from complete final response
- `ModelCatalog`: pinned model IDs, estimated prices, output caps
- `BudgetLedger`: thread-safe approximate reservation and actual cost reconciliation
- `ExperimentArtifacts`: hashes and JSON outputs

EXP-001 owns only the prompts, participant roster, protocol, and concurrent orchestration (`Program.cs`). Legacy `experiments/Aletheon.Trial` remains unchanged pending a separate safe migration; this is a reusable C# library, not yet the single engine of all historical experiments.

See [RUN_WINDOWS.md](RUN_WINDOWS.md) for running and safety limits. No new paid API requests have been made.
