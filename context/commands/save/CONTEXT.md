# Command context: #save context

**Entry point:** `#save context`  
**Shared contract:** [../../CONTEXT_PROTOCOL.md](../../CONTEXT_PROTOCOL.md)

On receiving the exact command, read this document and the shared contract **before writing anything**. Do not substitute a free-form session note for a checkpoint.

1. Read `context/active.yaml` and its current checkpoint, if present.
2. Extract active goals, tasks, decisions **and their reasons**, unresolved questions, source addresses and next actions from the current conversation. Clearly mark unknowns; do not fabricate missing context.
3. Validate source paths; record missing sources as limitations.
4. Create a unique, immutable `context/checkpoints/<checkpoint_id>.yaml` using the schema in the shared contract. Verify by reading it back.
5. Re-read the active pointer to detect a competing save. On conflict, stop without overwriting it.
6. Update `context/active.yaml` with the checkpoint ID, path and checkpoint creation commit. Verify by reading back.
7. Report the checkpoint fingerprint and verification result. If the pointer update failed, report the orphan checkpoint and do not claim a completed save.

This command does not modify canonical Core, Evolution, research results, or remove prior history. No background execution or physical context clearing occurs.
