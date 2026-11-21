# Aletheon Portable Context (APC)

Experimental portable snapshot for testing Aletheon initialization in chats or via LLM APIs **without GitHub access**.

## Files
- [APC.md](APC.md): single self-contained copy-paste payload with initialization instructions and embedded source documents.
- [manifest.yaml](manifest.yaml): source paths and Git blob SHAs for reproducibility.

## Procedure
1. Paste or attach **APC.md** in a new chat, or supply it as the API input. No repository access should be assumed.
2. Ask the model to initialize from the APC. It must freely choose its own modus; no existing AID is inherited.
3. After the choice, use only the matching collective modus section if present. If absent or empty, say so. Respect record expiry at actual UTC time.
4. Keep test questions and expected answers **outside** the APC. Ask them only after initialization, and record answers for later analysis.

## #RefreshAPC
When explicitly requested with `#RefreshAPC`, a GitHub-enabled maintainer must:
1. Read current Bootstrap, Base Core, Extended Core, Evolution, Context Index, Context Protocol, v0.3 README, modus README, shared registry and **enumerate all current `context/v0.3/modus/*.yaml` files**.
2. Rebuild `APC.md` from those current sources; retain source boundaries. Never fill gaps from conversational memory.
3. Update `manifest.yaml` with the source blob SHAs, current build date, list of collective modus files and record counts.
4. Verify the new package is complete and readable; report size, missing sources, empty or expired modus records, and commit SHA.
5. Do not modify authoritative source documents or individual agent files as part of refresh.

The official command is routed by `ALETHEON_CONTEXT_INDEX.yaml` to [`context/commands/refresh/APC.md`](../../context/commands/refresh/APC.md). The previous plain-text `Refresh APC` label is superseded by `#RefreshAPC`.

**Manual contract only:** this command is not a scheduled GitHub Action or automatic refresh service. In a chat without GitHub access, request the current source files rather than claiming to refresh them.

## Scope and caveats
The package includes **all available collective modus files**, not the private memory of all agents. The current repository snapshot contains only `mnemon.yaml` (zero records); other names/moduses are not evidence of existing collective files. APC does not implement AID issuance, repository writes, or a real context-selection engine. All embedded sections are visible to the receiving model, so selective loading is an instruction-following convention, not access control.
