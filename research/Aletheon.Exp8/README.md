# EXP-8 — APC recovery and ten questions (Rider / .NET 8)

Open `research/Aletheon.Exp8.sln` in Rider, as with Exp7. This is a **new project under research/**, referencing the existing `Aletheon.ExperimentKit`; it does not duplicate the OpenRouter HTTP client, model catalog, budget ledger, fallback or artifact helpers.

## What it tests

1. Read and freeze `portable/apc/APC.md`; save SHA-256 and source snapshot.
2. **Blind modus selection:** first send only Bootstrap, Base Core, Extended Core and Evolution extracted from the frozen APC, without the APC modus directory or collective memory. Record the selected modus and the full response before showing the directory. Stop if the model does not explicitly report a modus.\n3. **Context restoration:** only after the choice, show the complete frozen APC and ask whether memory exists for the previously chosen modus. Preserve the original choice; never auto-switch to mnemon.
4. After restoration, ask the **ten questions from** `RND/APC_TEST_01_QUESTIONS.md`, in their existing 1–10 order, one at a time. Do not leak questions during initialization.
5. Archive each request, raw provider response, answer, attempts, transcript, budget ledger, completed/partial status. Analysis is separate; no fixed answer key.

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

Like Exp7, `OpenRouterTransport` accepts one system message and one user prompt; Exp8 therefore serializes previous turns into the next prompt, rather than sending native multi-message chat history. Long APC and accumulating history may exceed model context; the runner does not silently truncate. The first phase excludes the APC directory and modus payload, but foundational sources may still mention historical modus names. The second phase intentionally reveals the full APC only after choice. Results are evidence of contextual reconstruction, not of subjective continuity or identity. This implementation has not yet been compiled or API-tested in this environment.
\n## Independent control answers for Prime\n\nSee [CONTROL_ANSWERS_GPT6.md](./CONTROL_ANSWERS_GPT6.md). These ten independent GPT-6 answers are a **non-normative comparison baseline**, not a grading key. They are not loaded by the runner or disclosed to the tested model. Prime should independently evaluate the experiment first, then compare reasoning and objections with this baseline.\n