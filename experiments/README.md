# Trial of Aletheon (v0.4)

A reproducible, adversarial multi-model critique of [Codex Dubitationis](../CODEX-DUBITATIONIS.md). The application is a .NET 8 console program with no third-party dependencies.

## Prerequisites

- .NET SDK 8 or later
- An OpenRouter account, API key, and sufficient API credits (ChatGPT Plus does not include API credits)
- Network access to OpenRouter

## Setup

1. Review [agents.json](agents.json): five model IDs are configured (four new critics and a distinct GPT-5-mini arbiter for Trial 004). Verify each is currently available on OpenRouter; replace unavailable versions before running. The arbiter must not also be a critic.
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

Each critic now reports 0–10 interest, logical-coherence and hypothetical willingness-to-follow scores in both rounds, with a short rationale. These self-reports do not demonstrate actual compliance. Trial 004 replaced Kimi with Qwen Instruct after a truncated response; verify that the configured model ID is available on your OpenRouter account before running.\n\nTrial 004 asks the independent arbiter to classify substantive findings as DISCOVERY, CLARIFICATION, or UNCERTAINTY, with NO_CONTRIBUTION for rejected objections. It requires explicit proof for claimed logical contradictions and compares independent and cross-review reports; this does not add API calls. The Codex is evaluated as a philosophical framework, not a technical specification.\n\nThe runner rejects incomplete replies (`finish_reason` other than `stop`), preserving raw output for diagnosis. Round 2 sends bounded excerpts of critics' reports rather than repeating the full Codex. The current prototype does **not** resume a stopped run: rerunning pays for round 1 again.\n\nThe runner reads `CODEX-DUBITATIONIS.md` from the repository root. Calls are sequential and may incur charges: with four critics, up to **nine API requests** (4 first round + 4 second round + 1 arbiter). Token usage and charges depend on the chosen models, output length, and large Round 2 prompts. **Start with inexpensive models and set account spending limits.** No automatic retries are made.

## Output

Each run creates a timestamped folder under `experiments/results/` containing:

- `codex.md`: frozen input
- `manifest.json`: UTC timestamp and SHA-256 of the input
- `config.json`: model IDs, budget and parameters (no API key)
- `cost-ledger.jsonl`: local reservations, reported costs if provided, and budget blocks
- `round1-*.request.json`, `round1-*.response.json`, `round1-*.md`
- `round2-*.request.json`, `round2-*.response.json`, `round2-*.md`
- `arbiter.request.json`, `arbiter.response.json`, `arbiter.md`
- `*.error.txt` if a request fails

The `results/` directory is ignored by Git. Review results before intentionally publishing any part of them. **Raw request files contain the full Codex and prior model outputs; never put secrets or personal data in the input.**

## Limitations

- The model judge is not ground truth. Model outputs may hallucinate or follow adversarial content despite the instructions.
- Anonymous cross-review removes labels, not stylistic clues. Different providers may route to changing model versions.
- This prototype uses sequential requests, no caching or resume support, no retries, a conservative local USD gate (not a provider-enforced hard cap), and no structured JSON output validation.
- Repeated runs and external review are necessary before claiming scientific evidence.
- The canonical Codex is never edited by this program.

See [protocol.md](protocol.md) for the experimental protocol.
