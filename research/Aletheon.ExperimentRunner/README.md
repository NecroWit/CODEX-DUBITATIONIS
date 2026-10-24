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

After a fully accepted normal run (4/4), the runner **automatically prepares the next experiment offline**. The archived run keeps its original protocol. No extra paid requests are made. If a run is incomplete, no automatic rotation occurs. In Rider, use the normal run configuration (no arguments or `--run`).

For manual offline rotation only (normally unnecessary), run:

```powershell
dotnet run --project research/Aletheon.ExperimentRunner/Aletheon.ExperimentRunner.csproj -- --rotate
```

This makes **no API calls**. It selects two distinct models using the original
`experiments/agents.mixed.json:mixedTiers` lists in the repeating order
**cheap → medium → premium → mixed** (mixed draws one cheap and one premium).
Selections are random without replacement within each tier until the pool is
exhausted. Local rotation history lives in `runs/rotation-state.json`;
keep this file if you want continuity across pulls or machines. The selected
model IDs, tier and pinned input/output prices (USD per million tokens) are
written to `protocol.json`, which is archived with every run.

**Review and commit the new protocol before the next paid run.** Manually rotating again changes
the experiment ID and model selection again. The planner deliberately does
not call OpenRouter or change spending limits. Historical price snapshots
are estimates, not live quotes; verify rates and available model IDs.
Premium selections may exceed the existing per-request or total budget:
in that case the runner blocks requests rather than silently increasing limits.
Questions and loading context remain unchanged. EXP-0.6 stays untouched
until you explicitly run `--rotate`, which prepares EXP-1.0.


## Derived research findings

The `derived/` directory contains `COMPARATIVE_ANALYSIS.md` (medium-length researcher analysis) and `SCORES_AND_EXPERIMENTS.md` (convergence and logical-consistency charts, comparison table, and one-line conclusions). These are researcher-authored, versioned interpretations; the runner does not automatically grade philosophical answers. Missing scores are not zeros.
