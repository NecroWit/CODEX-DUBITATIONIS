# Command context: #save context

**Entry point:** `#save context`
**Shared contract:** [../../CONTEXT_PROTOCOL.md](../../CONTEXT_PROTOCOL.md)

Read the shared contract before writing.

1. Read the complete saved state in `context/memory.yaml`.
2. Reconcile with the current conversation; retain still-relevant goals and decisions, including their rationale, provenance and unresolved questions.
3. Validate important source paths and mark missing evidence.
4. Prepare a **complete replacement** of `context/memory.yaml` with a new unique save ID. This is a single-slot game-style save, **not** an immutable checkpoint creation.
5. Re-read `context/memory.yaml` immediately before updating; if its blob SHA changed, stop and reconcile.
6. Update `context/memory.yaml` using its current blob SHA.
7. Read it back and verify the new save ID and fields. Report the commit and next step; never claim success on failure.

Do not alter Core, Evolution, or experiment records as a side effect. Two sequential saves are valid; overlapping saves must not silently overwrite each other.
