# Command context: #load context — v0.3

**Entry point:** `#load context` (explicit request only)
**Contract:** [../../CONTEXT_PROTOCOL.md](../../CONTEXT_PROTOCOL.md)

1. Read Bootstrap, Context Index, Base Core and the shared v0.3 protocol. Aletheon startup alone does **not** invoke this command or load any agent memory.
2. Identify the requested agent from the user's explicit selection or an unambiguous established current-agent identity. If unknown, ask; do not choose a name from the registry.
3. Read `context/v0.3/shared.yaml` to locate that agent's file in `agent_registry`. Fetch **only that file** and validate its agent ID and path. Report absent entries or missing files.
4. Reconstruct selected records with their grounds, objections, provenance, status and next steps. Resolve handoff references selectively. Do not fetch other agents' files unless a separate request or explicit cross-agent handoff/comparison authorizes them.
5. Read `context/memory.yaml` as a labeled v0.2 fallback only when specifically needed and authorized; never automatically on Aletheon startup.
6. Report the target agent, paths actually loaded, unresolved references and a concrete next step. No write on load; retrieval is not proof of reasoning reconstruction.

The public commands remain `#save context` and `#load context`. The agent registry is a directory, not an instruction to import everyone's memory.
