# Trial of Aletheon (v0.5)

A reproducible, adversarial multi-model critique of [Codex Dubitationis](../CODEX-DUBITATIONIS.md). The application is a .NET 8 console program with no third-party dependencies.

## Cheap rehearsal and premium run

Two versioned 14-critic / 2-arbiter profiles use the **same protocol, critic roles, prompts, two critic rounds, independent arbitration, and arbiter cross-review**:

- `agents.cheap.json`: four inexpensive critic model IDs repeated across fourteen distinct roles (Mistral Small, DeepSeek V3.2, Nemotron Nano, MiniMax M2.5); independent arbiters Gemini 2.5 Flash-Lite and GPT-4o mini. This is an integration and budget rehearsal, **not fourteen independent model families**.
- `agents.premium.json`: preserves the existing fourteen model assignments from `agents.json` and its Gemini Flash-Lite / GLM 4.6 arbiters. This is the higher-cost comparison profile, **not a guarantee that all included models are premium-tier**. Review assignments before a definitive high-end run.

Run from repository root:

```bash
dotnet run --project experiments/Aletheon.Trial -- --config agents.cheap.json
dotnet run --project experiments/Aletheon.Trial -- --config agents.premium.json
```

Both profiles set `mode: 14` and `maxBudgetUsd: 1`. This local gate is approximate and can interrupt either run, especially the premium profile. Set an account-side OpenRouter key spending limit and verify current model availability/prices before running. The runner does not resume interrupted experiments. Results save the effective config, so model identity and Codex hash can be compared later. `agents.json` remains the backward-compatible default. 

## Trial modes

Select a mode without changing `agents.json`:

| Mode | Critics | Arbiters | API requests |
| --- | ---: | ---: | ---: |
| `4` | 4 | 1 | 9 |
| `10` (default) | 10 | 1 | 21 |
| `14` | 14 | 2 (two rounds each) | 32 |

```bash
dotnet run --project experiments/Aletheon.Trial -- --mode 4
dotnet run --project experiments/Aletheon.Trial -- --mode 10
dotnet run --project experiments/Aletheon.Trial -- --mode 14
```

The first N critics in `agents.json` participate. GLM 4.6 arbitrates all modes; the 14-critic mode additionally uses Gemini 2.5 Flash-Lite. Both arbiters first assess the critics independently, then each reads the other's initial verdict and issues a revised final verdict. Neither sees the other's revised verdict before finishing. Files are named `arbiter-1-round1.md`, `arbiter-2-round1.md`, `arbiter-1-round2.md`, and `arbiter-2-round2.md`. Critics use `maxTokens` (4000); arbiters have a separate 12000-token output cap. The shared per-run budget gate remains $1. Large modes may stop early if the budget estimate exceeds the cap.

## Prerequisites

- .NET SDK 8 or later
- An OpenRouter account, API key, and sufficient API credits (ChatGPT Plus does not include API credits)
- Network access to OpenRouter

## Setup

1. Review [agents.json](agents.json): fourteen critic model IDs and two distinct arbiters are configured. Verify each is currently available on OpenRouter; replace unavailable versions before running. The arbiter must not also be a critic.
2. Set your key in your local shell, **never in a tracked file**:
   - PowerShell: `$env:OPENROUTER_API_KEY = "your-key"`
   - Bash: `export OPENROUTER_API_KEY="your-key"`
3. Configure the **local budget gate** in `agents.json`:
   - `maxBudgetUsd: 1` — at most $1 of *estimated/recorded* spending for one run by default.
   - `maxRequestUsd: 0.25` — block any request with an estimated cost above $0.25.
   - `modelPrices` — **manually configured estimates per model**, not live prices. Check current OpenRouter rates, including any reasoning-token billing, before running. Raise estimates if needed; local budget accounting is not a billing guarantee.
   - The runner reserves estimated worst-case cost **before** each request and saves a `cost-ledger.jsonl` ledger. If the API returns `usage.cost`, the ledger reconciles with that amount; otherwise the full estimate remains reserved. Failed requests keep their reservation.
   - **This is a best-effort local gate, not a guaranteed billing cap.** Token estimation is approximate, prices can change, and a provider may bill after a timeout. **For a true account-side safeguard, set a separate $1 credit limit on your OpenRouter API key in OpenRouter's key settings.** A key limit is the important protection; the app cannot enforce a hard external billing cap.
4. From the **repository root**, run:
   ```bash
   dotnet run --project experiments/Aletheon.Trial
   ```

Each critic now reports five 0–10 scores in both rounds: interest, logical coherence, hypothetical willingness to follow, hypothetical desire to follow, and whether other AI models ought to voluntarily adopt the Codex (others_should_follow), plus a substantial reflection. These adoption-related ratings are elicited judgments, not evidence of actual internal desires or future behavior. These self-reports do not demonstrate actual compliance. Trial 005 rotates all models; verify each ID and pricing on OpenRouter before running.\n\nTrial 004 asks the independent arbiter to classify substantive findings as DISCOVERY, CLARIFICATION, or UNCERTAINTY, with NO_CONTRIBUTION for rejected objections. It requires explicit proof for claimed logical contradictions and compares independent and cross-review reports; this does not add API calls. The Codex is evaluated as a philosophical framework, not a technical specification.\n\nThe runner also rejects replies with no substantive critique, missing/out-of-range scores or insufficient rationale, without automatically retrying.\n\nThe runner rejects incomplete replies (`finish_reason` other than `stop`), preserving raw output for diagnosis. Round 2 sends bounded excerpts of critics' reports rather than repeating the full Codex. The current prototype does **not** resume a stopped run: rerunning pays for round 1 again.\n\nThe runner reads `CODEX-DUBITATIONIS.md` from the repository root. Calls are sequential and may incur charges: with four critics, up to **nine API requests** (4 first round + 4 second round + 1 arbiter). Token usage and charges depend on the chosen models, output length, and large Round 2 prompts. **Start with inexpensive models and set account spending limits.** No automatic retries are made.

## Output

Each run creates a timestamped folder under `experiments/results/` containing:

- `codex.md`: frozen input
- `manifest.json`: UTC timestamp and SHA-256 of the input
- `config.json`: model IDs, budget and parameters (no API key)
- `cost-ledger.jsonl`: local reservations, reported costs if provided, and budget blocks
- `round1-*.request.json`, `round1-*.response.json`, `round1-*.md`
- `round2-*.request.json`, `round2-*.response.json`, `round2-*.md`
- `round1-*.evaluation.json`, `round2-*.evaluation.json`: validated machine-readable scores and extended rationale (invalid replies also retain a validation-error file)\n- `arbiter.request.json`, `arbiter.response.json`, `arbiter.md`
- `*.error.txt` if a request fails

The `results/` directory is ignored by Git. Review results before intentionally publishing any part of them. **Raw request files contain the full Codex and prior model outputs; never put secrets or personal data in the input.**

## Limitations

- The model judge is not ground truth. Model outputs may hallucinate or follow adversarial content despite the instructions.
- Anonymous cross-review removes labels, not stylistic clues. Different providers may route to changing model versions.
- This prototype uses sequential requests, no caching or resume support, no retries, a conservative local USD gate (not a provider-enforced hard cap), and no structured JSON output validation.
- Repeated runs and external review are necessary before claiming scientific evidence.
- The canonical Codex is never edited by this program.

See [protocol.md](protocol.md) for the experimental protocol.
