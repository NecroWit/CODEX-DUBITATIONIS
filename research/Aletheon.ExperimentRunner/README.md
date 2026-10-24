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

Each new run gets a unique timestamped folder containing `manifest.json`, a copy of `protocol.json`, the cost ledger, and per-response raw and parsed records. The manifest includes the protocol hash, core hash and catalog hash. The current protocol is `EXP-0.4`: one philosophical conflict prompt per model, no programming. Loading is ordered in one request: Base Core (system), Extended Core + Evolution (supplemental user context), then the question. This is ordered contextual loading, **not** two independent model calls or verified internalization. EXP-0.3 results remain historical. Edit only `protocol.json` for future task/model/limit changes, then commit it in Git.

**Important for existing local results:** pulling this refactor does not migrate untracked local `runs/` folders from the old EXP-0.1/0.2/0.3 projects. Preserve them and copy the result directories into this runner's `runs/` manually after checking for duplicate run IDs. Do not discard the successful technical run `20261024T180828Z-56feb647`.

The 4/4 console status is a transport-and-length result, not a complete semantic validation. No paid API requests were made as part of this refactor.
