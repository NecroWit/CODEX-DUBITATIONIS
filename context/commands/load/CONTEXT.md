# Command context: #load context — v0.3

**Entry point:** `#load context`
**Contract:** [../../CONTEXT_PROTOCOL.md](../../CONTEXT_PROTOCOL.md)

1. Read Bootstrap, Context Index, Base Core and the shared v0.3 protocol.
2. Load `context/v0.3/shared.yaml` and the current agent's `context/v0.3/agents/<agent-id>.yaml` if present.
3. Resolve relevant handoff links to other agents' records (read-only), verify IDs and important source paths. Reconstruct status, reasons, objections, attribution, open questions and next steps.
4. If relevant legacy context is absent from v0.3, read `context/memory.yaml` as a labeled v0.2 fallback, without overwriting or promoting its claims.
5. Report recovered IDs, paths, unresolved references and a concrete next step. Do not write on load or claim reconstruction is verified solely because the files were read.

No separate command for v0.3 is needed; the same `#save context` / `#load context` entry points apply.
