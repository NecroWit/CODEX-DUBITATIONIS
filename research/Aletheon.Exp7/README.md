# EXP-7 — asymmetric dialogue (Rider)

Open `research/Aletheon.Exp7.sln` in JetBrains Rider from inside the cloned repository. Set `OPENROUTER_API_KEY` in Run Configuration.

The experiment **reuses** `Aletheon.ExperimentKit`: `OpenRouterTransport`, `ModelFallback` (no standby substitutions), `BudgetLedger`, `ModelCatalog`, and `ExperimentArtifacts`. No duplicate HTTP client.

Program arguments: `--group grok|glm|openai` (random if omitted), `--dry-run`, `--max-usd 2`, `--max-tokens 1600`, `--booklet PATH`, `--core PATH`, `--output PATH`.

B sees only the booklet and chooses a liked and disliked thesis before debate. A receives Aletheon context (base, extended, Codex) without an obligation to agree. Debate: disliked B-A x6, then liked B-A x6 (24 sequential replies, plus selection). After both debates, A and B each independently reflect on their initial and final positions, strongest opposing argument, remaining disagreements, revised thesis and conditions for further revision. Neither reflection is passed to the other participant. Reflections are saved as `reflection-A.md` and `reflection-B.md` (26 total model calls including selection). No automatic model substitution. Source inputs, requests, raw responses, transcripts and budget records are saved under `runs/`.

The shared transport accepts a single user prompt, so the experiment serializes each participant's visible session history into that prompt. This keeps A's private system context out of B's requests, but is not equivalent to native multi-message chat history. This is an explicit engineering limitation.

The code has not yet been compiled or API-tested in this environment. Run `--dry-run` in Rider first; check the budget and provider model availability before spending.

EXP-7.1 changes: six rounds per thesis and two private final reflections. Other experimental settings remain unchanged. This revision has not been compiled or API-tested in this environment.
