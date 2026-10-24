# EXP-0.3 — cheap calibration

Rider: open root `Aletheon.Research.sln`; select EXP-0.3 project.

**No arguments / --run:** four paid requests (two models × two short tasks). **--smoke:** one paid connectivity request. Both modes validate local configuration before sending requests. Never automatically retries an entire experiment.

```powershell
dotnet run --project research/tracks/04-aletheon-research/experiments/EXP-0.3-calibration/Aletheon.ModelTransferResearch.Experiment01.csproj
dotnet run --project research/tracks/04-aletheon-research/experiments/EXP-0.3-calibration/Aletheon.ModelTransferResearch.Experiment01.csproj -- --smoke
dotnet run --project research/tracks/04-aletheon-research/experiments/EXP-0.3-calibration/Aletheon.ModelTransferResearch.Experiment01.csproj -- --run
```

Set `OPENROUTER_API_KEY` for paid modes. Pin OpenRouter key spending limit separately. Model prices in shared `models.json` are manually pinned estimates, not verified current rates; the $0.05 in-process cap is an **estimate, not a guaranteed provider spending limit**. Output cap 1600 tokens includes reasoning. Raw responses are saved to unique run folders. A response with `finish_reason=length` remains incomplete. The length and semantic criteria are evaluated separately from transport completion. The new tasks require new independently frozen GPT-6 control answers before making cross-model scientific claims. EXP-0.1/0.2 remain immutable historical attempts.
