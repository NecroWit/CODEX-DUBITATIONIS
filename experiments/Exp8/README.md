# Exp8 — APC recovery test (.NET 8 / C#)

Standalone C# runner for [APC-TEST-01](../apc-test-01/PROTOCOL.md), using the same OpenRouter chat-completions endpoint and archival conventions as `Aletheon.Trial`. No changes to the existing Trial runner.

## Setup

1. Install .NET SDK 8+. In `experiments/Exp8/config.json`, set `models` to one or more **real OpenRouter model IDs** and add a matching `prices` entry for each model, e.g. `"provider/model": { "inputUsdPerMillionTokens": 1, "outputUsdPerMillionTokens": 2 }`. The numbers are **examples only**, not verified current prices. Check live rates and each model's context window before running.
2. Set `OPENROUTER_API_KEY` in your shell; do not commit the key. Set an account-side key spending cap. `maxBudgetUsd` and `maxRequestUsd` are approximate local gates, **not** a provider-enforced cap.
3. From repository root:

```bash
dotnet run --project experiments/Exp8 -- --dry-run
dotnet run --project experiments/Exp8
dotnet run --project experiments/Exp8 -- --model provider/model
```

`--apc /path/to/APC.md` overrides the default snapshot. The `--dry-run` checks inputs and archives a manifest without API calls.

## Flow

Each model gets a **fresh, independent conversation**: the entire frozen APC in the initialization user message, then the fixed introductory instruction and ten questions in exact order, one at a time, preserving chat history. Questions are stored separately in `questions.json` and are not included in the initialization message. The model selects its own modus; no AID is assigned. No evaluator or answer key is sent to the model.

Raw input/response JSON, text answers, conversation, model IDs, APC and question hashes, cost ledger and status are archived under `experiments/Exp8/results/<run-id>/<model-hash>/`. Do not commit outputs without review; the `results/` directory should remain untracked. Analysis is manual and separate, as in prior tests.

## Caveats

- The current APC is large (~118k characters); confirm model context capacity, including accumulated chat history across ten questions. This runner does **not** silently truncate APC or history.
- Input token estimation is based on character count, not provider tokenization; provider charges may exceed the local estimate. The gate retains reservations on failures.
- This runner does not retry or replace models after errors, to preserve test conditions. A failed model is marked `partial` and its evidence retained. Responses with `finish_reason` other than `stop` are not accepted.
- A successful API run does not prove absence of other provider-side context or subjective identity. Modus-specific collective sections are visible within APC; selective loading is an instruction, not isolation.
- This is prepared code, **not an executed experiment**. API credits and valid model IDs are required.
