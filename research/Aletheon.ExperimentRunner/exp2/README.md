# EXP-2.0 — C#/.NET 8

**Open in Rider:** `research/Aletheon.Exp2.sln`. This is a separate solution referencing only the EXP-2 executable and the existing shared `Aletheon.ExperimentKit` project. The original `Aletheon.Research.sln` is also in `research/`.

**Run configuration:** select `Aletheon.Exp2`. No arguments launches the experiment (paid requests) without interactive confirmation. Use program argument `--smoke` for **offline validation only**; unlike the EXP-1.x runner's `--smoke`, EXP-2's smoke currently does **not** make a connectivity request. `--run` is an explicit equivalent of no arguments.

The API key is read from environment variable `OPENROUTER_API_KEY`. A provider-side spending limit is strongly recommended. The program does not require console input. Rider Run starts paid requests after configuration and budget checks; set a provider-side spending limit first.

EXP-2.0 sends eight independent OpenRouter requests: for each of two fixed models, one preliminary R0 reconstruction, followed by independent H1, H2 and H3 requests, all with the same frozen R0 and seven source documents. H1/H2 match the EXP-1.9 protocol; H3 tests source-grounded intellectual genealogy and revision.

The output is written to `research/Aletheon.ExperimentRunner/runs/EXP-2.0/<run-id>/` with source/protocol hashes, raw provider responses, assessments and cost ledger. The pinned catalog prices are estimates, **not** a provider-side spending guarantee. No fallback models are used. Missing actual cost, mismatched model ID, truncation and word-limit violations fail technical acceptance. H3 semantic evaluation is manual; see `EVALUATION_SHEET.md`.

**Verification status:** GitHub files and project references have been inspected, but no local .NET build, Rider launch, smoke test or paid OpenRouter execution has been performed by the assistant. Do not interpret repository edits as evidence of successful execution.

## Automatic EXP-2 sequence

Default Run checks local `runs/EXP-2.x/` archives before paid calls. Eight accepted markers (A/B × R0,H1,H2,H3) advance the sequence to EXP-2.(x+1) and select two previously unused models from `experiments/agents.mixed.json` using the `rotation.json` tier order (cheap → medium → premium → mixed). Mixed selects one unused cheap and one unused premium. An incomplete run retries the same archived protocol. If no eligible pair remains, execution stops before requests. The active generated protocol is `exp2/protocol.json`, archived with each run; `EXP-2.0.protocol.json` remains the initial seed. `--smoke` does not advance the sequence. The generated protocol retains the EXP-2.0 tasks and source loading; it changes experiment ID and model pair, not the philosophical prompts. This uses the EXP-2 budget ceiling of $3 rather than EXP-1 tier-specific limits; inspect the generated protocol and provider spending cap. Existing EXP-2.0 local run folders are required for automatic advancement.
