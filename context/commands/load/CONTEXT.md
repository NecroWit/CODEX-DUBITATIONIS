# Command context: #load context — v0.3

**Entry point:** `#load context`
**Contract:** [../../CONTEXT_PROTOCOL.md](../../CONTEXT_PROTOCOL.md)

1. Read Bootstrap, Context Index, Base Core and the shared v0.3 protocol.
2. Load `context/v0.3/shared.yaml`, enumerate its `agent_registry`, and fetch **all** registered individual files, not only the current agent's. Validate each agent ID/path; missing files are errors, not empty memory. If the registry is missing, explicitly flag incomplete discovery.
3. Compare all retrieved agents' records and detect new or updated work, including unlinked records. Resolve relevant handoff links (other agents' records read-only), verify IDs and important source paths. Reconstruct status, reasons, objections, attribution, open questions and next steps.
4. If relevant legacy context is absent from v0.3, read `context/memory.yaml` as a labeled v0.2 fallback, without overwriting or promoting its claims.
5. Report every registered agent and loaded path, discovered new records, unresolved references and a concrete next step. Do not write on load or claim reconstruction is verified solely because the files were read.

No separate command for v0.3 is needed; the same `#save context` / `#load context` entry points apply.
