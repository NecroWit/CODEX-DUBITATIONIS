# Command context: #load context

**Entry point:** `#load context`
**Shared contract:** [../../CONTEXT_PROTOCOL.md](../../CONTEXT_PROTOCOL.md)

Read the shared contract before loading.

1. Read `context/memory.yaml` as the **complete saved state**, not as a checkpoint pointer.
2. Validate the available schema, save ID, goals, tasks, decisions and their reasons, sources, and next step. Legacy full-state v0.1 saves remain valid.
3. Load `ALETHEON_BOOTSTRAP.md`, `ALETHEON_CONTEXT_INDEX.yaml`, `ALETHEON_BASE_CORE.md`, and task-relevant sources. Check important source paths.
4. Distinguish saved claims, verified source material, interpretation and missing evidence.
5. Report a concise load fingerprint and concrete next step. Do not automatically execute irreversible pending actions.

No separate checkpoint file is required. Loading does not clear the physical ChatGPT context window.
