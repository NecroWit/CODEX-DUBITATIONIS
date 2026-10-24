# EXP-001 — Rider / .NET 8

**Open the repository-root `Aletheon.Research.sln` in Rider, not the individual `.csproj`.** The solution already includes the experiment, the shared library and its offline tests. No manual Add Reference or Attach Project steps are needed. After Git Pull, use File → Open → `Aletheon.Research.sln` once; Rider restores and indexes the projects.

Run from the repository root, or choose the experiment project in Rider:

```powershell
dotnet run --project research/tracks/04-aletheon-research/experiments/EXP-001-model-transfer/Aletheon.ModelTransferResearch.Experiment01.csproj -- --smoke
```

After a successful smoke test, run four concurrent independent requests:

```powershell
dotnet run --project research/tracks/04-aletheon-research/experiments/EXP-001-model-transfer/Aletheon.ModelTransferResearch.Experiment01.csproj -- --run
```

Set `OPENROUTER_API_KEY` in IDE environment variables or PowerShell before launch. Do not commit it. Install .NET 8 SDK. No Python or NuGet dependencies.

Shared infrastructure: `research/Aletheon.ExperimentKit`. All model IDs and conservative **manually pinned** price estimates live in `research/Aletheon.ExperimentKit/models.json`. Check actual OpenRouter pricing and configure an OpenRouter key spending limit before a paid run. Reasoning tokens count toward the 12,000-token output cap; complete means `finish_reason=stop` and nonempty `message.content`. If incomplete, the runner archives the raw response and fails rather than silently substituting models or hiding failures.

The old local run `20261024T164028Z-a86f4b17` is **excluded from the analytical sample**, not physically deleted from your computer or Git history. Preserve it as a failed pilot, or manually move it to an archive folder. The runner always creates a new timestamped output directory.

The ChatGPT control answers are frozen in `control/`. Do not regenerate or change them after viewing API outputs.

The project reference from this directory uses `../../../../` to reach `research/Aletheon.ExperimentKit`.
