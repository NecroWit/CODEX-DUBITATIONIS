# Command context: #save context — v0.3

**Entry point:** `#save context`
**Contract:** [../../CONTEXT_PROTOCOL.md](../../CONTEXT_PROTOCOL.md)

1. Read the shared v0.3 memory and the current agent's individual memory. Identify the current agent by stable ID, not display name. Consult legacy `context/memory.yaml` only when necessary.
2. Reconcile the current conversation and relevant repository sources with existing records; preserve unrelated work, reasoning, objections and provenance.
3. Save individual work to `context/v0.3/agents/<agent-id>.yaml`. Save explicitly shared decisions, handoffs or pointers to `context/v0.3/shared.yaml`, without silently promoting individual hypotheses to consensus.
4. Use stable record IDs, explicit status, rationale, objections, typed links and source evidence status. Do not invent references.
5. Re-fetch each target and compare blob SHA immediately before writing. Stop/reconcile on conflict; use create-file only for absent agent files.
6. Re-read every written file and verify. Report each commit, saved IDs, missing links and whether the multi-file operation was complete or partial.

Do not overwrite `context/memory.yaml`, other agents' individual files, Core, Evolution or experiment logs as a side effect. No guarantee of atomic multi-file saves.
