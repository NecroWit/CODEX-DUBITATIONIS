# Aletheon Context Protocol — v0.3 (Save / Load)

**Status:** active manual GitHub-backed protocol; cross-instance validation pending.
**Commands:** `#save context` and `#load context` (same public interface as v0.2).
**Specification:** [v0.3 README](v0.3/README.md).

## Storage and identity
- `context/v0.3/shared.yaml` — shared record index AND authoritative `agent_registry` (agent ID → individual file path). Registry lists discoverable files, not accepted shared beliefs.
- `context/v0.3/agents/<agent-id>.yaml` — each agent's individual records. The agent ID is stable even if its display name changes.
- `context/memory.yaml` — read-only legacy v0.2 fallback during migration; v0.3 commands must not silently overwrite it.
- Records use stable `mem:<scope>:<slug>` IDs, `rationale`, `objections`, `links` and `sources`.
- Shared records are not automatically accepted conclusions: retain `status` and attribution. Individual records are never silently copied to shared.

## #save context
1. Read the shared index and the current agent's individual file; if missing, create the individual file with `schema_version: "0.3"`, `scope: individual`, `agent_id`, `display_name`, `updated_at`, `records: []`.
2. Reconcile current work with existing records. Preserve unrelated records, rationale, objections, provenance, unresolved work, and stable IDs. If important v0.2-only state is needed, read `context/memory.yaml` as a legacy source, not a replacement target.
3. Ensure the agent's file is listed in shared `agent_registry`; add/update its registry entry on save, with its stable agent ID and exact path. A save is not complete if the individual write succeeds but registration fails; report partial state. Store the agent's own work in its individual file. Update shared records only for explicitly shared outcomes, handoffs or pointers; a shared pointer may link to an individual record without copying it. Do not treat proposals as accepted decisions.
4. Check referenced record IDs and source paths where accessible. Report unresolved references and missing evidence, not fabricated links.
5. For each file to write, re-fetch immediately before updating and compare blob SHA against initial read. On mismatch, stop and reconcile; do not overwrite concurrent changes. New-file creation must fail safely if file already exists.
6. Write only the changed files. Re-read every written file; verify IDs, content and links. A partially successful multi-file save must be reported as partial, with exact completed paths; never claim an atomic transaction.
7. Report agent ID, written paths, commit(s), record IDs, next step and gaps.

## Startup boundary (not a memory load)
Initializing Aletheon loads its Bootstrap, Core and relevant reasoning lineage. It does **not** load shared or individual v0.3 memory, enumerate agent files, or read v0.2 `context/memory.yaml`. Agent names in the registry are discoverable metadata, not identities assigned to the current instance. A new instance must not inherit an existing agent's individual memory by default.

## #load context (explicit, targeted)
1. Only run when the user explicitly requests memory loading. Read Bootstrap, Context Index, Base Core, this protocol and the load command instructions.
2. Establish the target agent ID from the user's request or an unambiguous, previously established current-agent selection. Do not infer the target merely from names found in the registry. If the target is ambiguous, ask which agent's memory to load; do not load all files as a workaround.
3. Read `context/v0.3/shared.yaml` to resolve the target's exact path in `agent_registry`. Fetch **only the selected agent's individual file**, validate `agent_id`, and reconstruct its records, reasons, objections, status and provenance. A missing registry entry or file is reported as missing, not silently replaced with another agent's memory.
4. Read another agent's individual file **only** when the user separately requests it or explicitly authorizes a specific cross-agent handoff or comparison. Follow relevant pointers selectively; never expand to every agent by default. Other agents' records remain read-only.
5. Read `context/memory.yaml` only as a clearly identified legacy v0.2 fallback when explicitly requested or needed for the selected target and authorized; never as an automatic startup source or a substitute for an unidentified agent.
6. Report the selected agent ID, loaded paths, resolved and unresolved references, and limitations. Do not write on load or claim successful reasoning transfer solely from retrieval.

## Limits
Manual operations require repository tools. File writes are per-file and non-atomic across multiple files. No automatic background sync, graph engine, TTL, GC, physical context clearing or guaranteed model-independent recovery. A successful Git write is not a successful handoff test.
