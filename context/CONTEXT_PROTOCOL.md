# Aletheon Context Protocol — v0.2 (Save / Load)

**Status:** manual GitHub-backed save/load protocol. Commands require an active session with repository tools.
**Architecture:** [../ALETHEON_CONTEXT_ARCHITECTURE.md](../ALETHEON_CONTEXT_ARCHITECTURE.md).

## Routing
The authoritative command map is `ALETHEON_CONTEXT_INDEX.yaml#context_commands`:
- `#save context` → `context/commands/save/CONTEXT.md`
- `#load context` → `context/commands/load/CONTEXT.md`

Read the command-specific instructions and this protocol before acting.

## Storage
- `context/memory.yaml` is the **single mutable save slot**. It contains the complete current saved state, not a pointer.
- Previous versions remain in Git commit history. Historical `context/checkpoints/` files are legacy snapshots, not required for new saves or loads.
- Preserve meaningful fields: `schema_version`, `checkpoint_id` (save identifier), `created_at`, `repository`, `active_goals`, `current_tasks`, `decisions` (with rationale and source refs), `open_questions`, `source_refs`, `restore_instructions`, and `limitations`.
- Existing v0.1 full-state saves, including `2026-10-31-prime-001`, are readable. Do not require `checkpoint_path` or `checkpoint_commit`.
- A save ID identifies a particular saved version, not a separately required checkpoint file.

## #save context
1. Read `context/memory.yaml` and inspect current conversation and relevant repository sources.
2. Reconcile goals, unfinished work, decisions **and their reasons**, unresolved questions, source paths, and next actions. Clearly distinguish verified facts, inference, and missing information.
3. Construct the complete replacement save document. Do not erase unrelated ongoing tasks merely because they were not discussed in this conversation.
4. Immediately before writing, re-fetch `context/memory.yaml` and compare its blob SHA with the initial read. If another writer changed it, stop and reconcile; do not overwrite blindly.
5. Replace `context/memory.yaml` using the latest verified blob SHA. Do not create a new checkpoint as part of normal save.
6. Read the saved file back and verify the ID and expected fields. Report success only after verification; otherwise report the exact failure.
7. Report a compact fingerprint: save ID, commit, goals, next step, unresolved issues, and missing sources.

Repeated sequential saves are allowed. If two saves overlap, optimistic concurrency checks prevent silent lost updates.

## #load context
1. Read `context/memory.yaml`.
2. Validate the available state fields. If the file is absent or empty, report that no saved state is available.
3. Load Bootstrap, Context Index, Base Core and task-relevant sources, validating important paths.
4. Reconstruct the saved goals, decisions, reasons, open questions and next action. Distinguish saved notes from independently verified sources.
5. Report a compact load fingerprint and any gaps. Do not execute pending irreversible actions automatically.

## Limits and integrity
- Save/load are manual tool-mediated operations; Markdown cannot automatically invoke them.
- Git history is version history, not proof of continuous identity or subjective experience.
- No automatic garbage collection, TTL, background agent, or context-window clearing is implemented.
- A Git commit proves a write, not successful philosophical reconstruction.
- Never claim success if verification fails; never silently overwrite concurrent changes.
