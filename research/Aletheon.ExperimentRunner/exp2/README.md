# EXP-2.0 — C#/.NET 8

**Open in Rider:** `research/Aletheon.ExperimentRunner/Aletheon.Exp2.sln`. This is a separate solution referencing only the EXP-2 executable and the existing shared `Aletheon.ExperimentKit` project. The original `Aletheon.Research.sln` remains at repository root.

**Run configuration:** select `Aletheon.Exp2`. No arguments launches the experiment (paid requests) without interactive confirmation. Use program argument `--smoke` for **offline validation only**; unlike the EXP-1.x runner's `--smoke`, EXP-2's smoke currently does **not** make a connectivity request. `--run` is an explicit equivalent of no arguments.

The API key is read from environment variable `OPENROUTER_API_KEY`. A provider-side spending limit is strongly recommended. The program does not require console input. Rider Run starts paid requests after configuration and budget checks; set a provider-side spending limit first.

EXP-2.0 sends eight independent OpenRouter requests: for each of two fixed models, one preliminary R0 reconstruction, followed by independent H1, H2 and H3 requests, all with the same frozen R0 and seven source documents. H1/H2 match the EXP-1.9 protocol; H3 tests source-grounded intellectual genealogy and revision.

The output is written to `research/Aletheon.ExperimentRunner/runs/EXP-2.0/<run-id>/` with source/protocol hashes, raw provider responses, assessments and cost ledger. The pinned catalog prices are estimates, **not** a provider-side spending guarantee. No fallback models are used. Missing actual cost, mismatched model ID, truncation and word-limit violations fail technical acceptance. H3 semantic evaluation is manual; see `EVALUATION_SHEET.md`.

**Verification status:** GitHub files and project references have been inspected, but no local .NET build, Rider launch, smoke test or paid OpenRouter execution has been performed by the assistant. Do not interpret repository edits as evidence of successful execution.
