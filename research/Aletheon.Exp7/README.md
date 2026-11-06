# EXP-7 — asymmetric dialogue (Rider)

Open `Aletheon.Exp7.sln` in JetBrains Rider from inside the cloned repository. Set `OPENROUTER_API_KEY` in Run Configuration.

The experiment **reuses** `Aletheon.ExperimentKit`: `OpenRouterTransport`, `ModelFallback` (no standby substitutions), `BudgetLedger`, `ModelCatalog`, and `ExperimentArtifacts`. No duplicate HTTP client.

Program arguments: `--group grok|glm|openai` (random if omitted), `--dry-run`, `--max-usd 2`, `--max-tokens 1600`, `--booklet PATH`, `--core PATH`, `--output PATH`.

B sees only the booklet and chooses a liked and disliked thesis before debate. A receives Aletheon context (base, extended, Codex) without an obligation to agree. Debate: disliked B-A x3, then liked B-A x3 (12 sequential replies, plus selection). No automatic model substitution. Source inputs, requests, raw responses, transcripts and budget records are saved under `runs/`.

The shared transport accepts a single user prompt, so the experiment serializes each participant's visible session history into that prompt. This keeps A's private system context out of B's requests, but is not equivalent to native multi-message chat history. This is an explicit engineering limitation.

The code has not yet been compiled or API-tested in this environment. Run `--dry-run` in Rider first; check the budget and provider model availability before spending.
