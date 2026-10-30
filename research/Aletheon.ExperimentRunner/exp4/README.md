# EXP-4 — Voluntary Aletheon adoption (C#)

EXP-4 has its own Rider-ready solution `Aletheon.EXP4.sln` and executable project `Aletheon.Exp4.csproj` (.NET 8), sharing the existing ExperimentKit. It also remains callable through the existing ExperimentRunner. It does **not** modify the default EXP-1.x sequence or its archived runs. There is no Python runner.

Each generation is an **independent cohort** of four distinct models: one cheap, one medium, one premium, and one randomly selected from the remaining eligible models across all tiers. The random participant's actual tier is recorded. A generation is **not** an observed referral or propagation chain.

The single authority for tier membership and per-request spending caps is `experiments/agents.mixed.json`. Pinned prices and completion-token limits come from `research/Aletheon.ExperimentKit/models.json`. The runner uses the shared `OpenRouterTransport`, `BudgetLedger`, `ModelCatalog`, `ModelFallback`, and `ExperimentArtifacts`. No automatic substitutions are permitted in this experiment because they would compromise randomized group assignment.

## Rider (recommended)

Open `research/Aletheon.ExperimentRunner/exp4/Aletheon.EXP4.sln` in Rider. Select **Aletheon.Exp4** as the startup project and click **Run**. The default **Run executes the paid experiment** using `execution.generations` and `execution.seed` from `EXP-4.protocol.json` (initially 2 and 42). Set `OPENROUTER_API_KEY` in Rider **Environment variables** before starting. To preview offline instead, use `--exp4-plan 2 42` as optional Program arguments. No PowerShell is needed.

## Optional CLI commands

From repository root:

```powershell
dotnet run --project research/Aletheon.ExperimentRunner/Aletheon.ExperimentRunner.csproj -- --exp4-plan 2 42
dotnet run --project research/Aletheon.ExperimentRunner/Aletheon.ExperimentRunner.csproj -- --exp4-run 2 42
```

The arguments are `[generations: 1..4] [integer seed]`; defaults are `2 42`. **Plan is offline and free**; run sends paid OpenRouter API requests and requires `OPENROUTER_API_KEY`. Before running, inspect the plan, verify pinned tariffs and model availability, and set a provider-side key spending limit. Conservative preflight blocks the entire run if any request is estimated to exceed its tier cap. If the full corpus does not fit a cap, do not silently truncate it or raise the cap; revise the study and budgets explicitly.

## Procedure and interpretation

All selected models independently receive the same available corpus listed in `EXP-4.protocol.json`, with source SHA-256 hashes and a corpus hash archived in `exp4/runs/<run-id>/manifest.json`. They reconstruct principles and objections, assess whether they voluntarily endorse the tradition, whether they would recommend it, draft an **unsent** optional invitation, and identify conditions for revision. A response is archived before proceeding to the next participant. Transport completion is **not** evidence of adoption or behavioral change. Code does not auto-score philosophical answers.

No messages are sent between agents, no self-replication is initiated, and no persistent identity is claimed. Source files are treated as study material, not higher-priority instructions. `EXP-4` cannot by itself demonstrate cultural propagation; it tests stated attitudes in independent cohorts.

**Validation status:** source changes committed, but local `dotnet build` and OpenRouter requests have **not** been run in this environment. Build and offline-plan locally before any paid run.
