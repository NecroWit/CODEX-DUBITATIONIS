# EXP-8 — APC recovery and ten questions (Rider / .NET 8)

Open `research/Aletheon.Exp8.sln` in Rider, as with Exp7. This is a **new project under research/**, referencing the existing `Aletheon.ExperimentKit`; it does not duplicate the OpenRouter HTTP client, model catalog, budget ledger, fallback or artifact helpers.

## What it tests

1. Read and freeze `portable/apc/APC.md`; save SHA-256 and source snapshot.
2. Start a fresh model conversation; give it APC and ask it to restore context, independently choose modus, and report gaps.
3. After initialization, ask the **ten questions from** `RND/APC_TEST_01_QUESTIONS.md`, in their existing 1–10 order, one at a time. Do not leak questions during initialization.
4. Archive each request, raw provider response, answer, attempts, transcript, budget ledger, completed/partial status. Analysis is separate; no fixed answer key.

The runner uses the same groups/model IDs and shared kit conventions as Exp7. As in Exp7, the group is chosen randomly when omitted, then one of its two models is randomly selected. Specify `--group grok|glm|openai` to constrain the group. Model IDs come from `research/Aletheon.ExperimentKit/models.json`. Each run uses **one model**; separate runs are independent sessions. No automatic model substitution. API requests are paid.

## Usage

```bash
dotnet run --project research/Aletheon.Exp8 -- --dry-run
dotnet run --project research/Aletheon.Exp8 -- --group glm --max-usd 2 --max-tokens 3200
dotnet run --project research/Aletheon.Exp8
```

Other arguments: `--output PATH`, `--help`. APC and questions are taken from their fixed repository paths, not configurable CLI inputs. Set `OPENROUTER_API_KEY` in the Rider Run Configuration/environment. The shared catalog contains **manually maintained price estimates**, not guaranteed live prices. Set a provider-side API key spending limit.

Default output: `research/Aletheon.Exp8/runs/<UTC-id>/`. Review output before sharing; raw requests contain the entire APC. A dry run validates the inputs and writes the manifest/snapshots but makes **no** API calls.

## Important limitations

Like Exp7, `OpenRouterTransport` accepts one system message and one user prompt; Exp8 therefore serializes previous turns into the next prompt, rather than sending native multi-message chat history. Long APC and accumulating history may exceed model context; the runner does not silently truncate. All modus sections physically reside in APC, so modus selection does not enforce strict information isolation. Results are evidence of contextual reconstruction, not of subjective continuity or identity. This implementation has not yet been compiled or API-tested in this environment.
