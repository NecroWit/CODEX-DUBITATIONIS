# Command context: #restore context

Read `context/CONTEXT_PROTOCOL.md` before restoring.

1. Read `context/active.yaml` or the explicitly supplied checkpoint path.
2. If the pointer is null, report that there is no active checkpoint; do not invent one.
3. Fetch the referenced checkpoint and verify its ID, schema, and source paths.
4. Load `ALETHEON_BOOTSTRAP.md`, `ALETHEON_CONTEXT_INDEX.yaml`, `ALETHEON_BASE_CORE.md`, and task-relevant sources.
5. Distinguish documented claims, saved notes, inferences, and missing evidence.
6. Report restored goals, rationale, next step, and verification status.

Do not mistake unrelated session notes for the active checkpoint. No automatic context-window clearing occurs.
