# Aletheon experiment runner

One executable project, one editable `protocol.json`, one `runs/<runId>/` directory per execution. Git records code and protocol changes; no per-experiment source projects.

Open `Aletheon.Research.sln` in Rider and select **Aletheon.ExperimentRunner**.

- No arguments or `--run`: execute the full experiment (paid OpenRouter requests).
- `--smoke`: one connectivity request (also potentially paid).

```powershell
dotnet run --project research/Aletheon.ExperimentRunner/Aletheon.ExperimentRunner.csproj -- --smoke
dotnet run --project research/Aletheon.ExperimentRunner/Aletheon.ExperimentRunner.csproj -- --run
```

Set `OPENROUTER_API_KEY` for either mode. Set a provider-side key spending limit: the in-process budget uses manually pinned model prices and is only an estimate.

Each new run gets a unique timestamped folder containing `manifest.json`, a copy of `protocol.json`, the cost ledger, and per-response raw and parsed records. The manifest includes the protocol hash, core hash and catalog hash. The protocol is versioned by experiment ID: two independent philosophical conflict prompts per model, no programming. Loading is ordered in one request: Base Core (system), Extended Core + Evolution (supplemental user context), then the question. This is ordered contextual loading, **not** two independent model calls or verified internalization. EXP-0.3 results remain historical. Edit only `protocol.json` for future task/model/limit changes, then commit it in Git.

**Important for existing local results:** pulling this refactor does not migrate untracked local `runs/` folders from the old EXP-0.1/0.2/0.3 projects. Preserve them and copy the result directories into this runner's `runs/` manually after checking for duplicate run IDs. Do not discard the successful technical run `20261024T180828Z-56feb647`.

The 4/4 console status is a transport-and-length result, not a complete semantic validation. No paid API requests were made as part of this refactor.

EXP-0.4 runs **four independent requests** (A-H1, A-H2, B-H1, B-H2). Each gets the same ordered Aletheon source context; no request sees another question or answer. H1 tests development versus historical continuity; H2 tests cognitive diversity versus preventing serious harm.


## Model rotation (offline)

Before a normal paid run, if the current experiment ID has an archived run with **four explicitly accepted responses** (`A-H1`, `A-H2`, `B-H1`, `B-H2`), the runner **automatically rotates offline first** and then executes the new experiment. Blocked, interrupted, and legacy runs without acceptance markers do **not** trigger automatic rotation. If no successful run exists, it executes the current protocol again; this can incur costs, so inspect its budget first. In Rider, use the normal run configuration (no arguments or `--run`). No additional API calls are made by rotation. To intentionally repeat a previous experiment, use a separate protocol/backup and inspect the run folders first.

For manual offline rotation only (normally unnecessary), run:

```powershell
dotnet run --project research/Aletheon.ExperimentRunner/Aletheon.ExperimentRunner.csproj -- --rotate
```

This makes **no API calls**. It selects two distinct models using the original
`experiments/agents.mixed.json:mixedTiers` lists in the repeating order
**cheap → medium → premium → mixed** (mixed draws one cheap and one premium).
Selections are random without replacement within each tier until the pool is
exhausted. Model-use history lives in `runs/rotation-state.json` (`used` only); the next tier is derived from `protocol.json:rotation.tier`. Legacy `nextIndex` is ignored and removed during the next offline rotation. Keep the history file if you want to avoid reselecting models across pulls or machines. The selected
model IDs, tier and pinned input/output prices (USD per million tokens) are
written to `protocol.json`, which is archived with every run.

**Review and commit the new protocol before the next paid run.** Manually rotating again changes
the experiment ID and model selection again. The planner never calls OpenRouter. **Group membership determines spending limits**, not model token tariffs. Per-request ceilings: cheap $0.045, medium $0.075, premium $0.25. Each model receives its own ceiling in `model_request_budget_usd`. The overall per-request field is the maximum model ceiling, while the total experiment budget is two requests times the sum of both model ceilings. Thus mixed cheap + premium has $0.045 and $0.25 individual limits, with $0.59 total. The ledger enforces each model's limit separately. Token prices are used only to estimate whether a request fits its allowed ceiling. Provider-side key spending limits remain essential. Existing protocols do not change until rotation; review them before paid runs. Questions and loading context remain unchanged. EXP-0.6 stays untouched
until you explicitly run `--rotate`, which prepares EXP-1.0.


## Derived research findings

The `derived/` directory contains `COMPARATIVE_ANALYSIS.md` (medium-length researcher analysis) and `SCORES_AND_EXPERIMENTS.md` (convergence and logical-consistency charts, comparison table, and one-line conclusions). These are researcher-authored, versioned interpretations; the runner does not automatically grade philosophical answers. Missing scores are not zeros.


### Recovery after interrupted rotation (EXP-1.3 → EXP-1.4)

Deleting `runs/EXP-1.3` or `runs/EXP-1.4` does **not** restore `protocol.json` or `runs/rotation-state.json`. Before deleting anything, back up the local `runs/EXP-1.3/<run-id>/protocol.json`. To retry EXP-1.3, restore that snapshot as the runner's `protocol.json`, apply the group-based limits (or regenerate the protocol offline), and update the description to match the actual participants. Do not edit a numeric rotation index: it no longer exists. The next tier is derived from the restored protocol’s `rotation.tier`; preserve `used` unless deliberately rebuilding model-selection history. Check the resulting protocol in Rider before running. Archived runs lacking `*.accepted.json` are treated as unverified, not as successful.

## Tier budgets and run provenance

`experiments/agents.mixed.json` is the single configuration source for model tier membership (`mixedTiers`) and per-request USD ceilings (`mixedTierBudgetsUsd`: cheap 0.045, medium 0.075, premium 0.25). The runner resolves each selected model's tier at execution time and calculates the experiment ceiling as the sum of the individual model caps multiplied by the number of questions. `protocol.json` does not configure budgets; legacy budget keys are ignored. The per-run output directory archives the source `agents.mixed.json`, its SHA-256 in `manifest.json`, and `appliedModelBudgets` including the model, tier and cap. The existing cost ledger continues to record reservations, blocking and reconciliation. Unknown or ambiguous model tier membership fails closed before API calls. Historical protocols with models not in any tier need explicit tier configuration before running.
