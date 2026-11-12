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

## #load context
1. Read Bootstrap, Context Index, Base Core, this protocol and relevant command instructions.
2. Read `context/v0.3/shared.yaml` and enumerate **every** `agent_registry` entry. Fetch every registered individual file, including files belonging to other agents, before declaring load complete. Check `agent_id` matches the registry entry; missing files or mismatches are explicit load errors. Other agents' files are **read-only** for the current agent. Resolve relevant handoff pointers and record IDs across all fetched files. Do not rely only on links pointing *to* the current agent; detect new records in other agents' files. If registry is absent, report discovery incomplete and inspect `context/v0.3/agents/` directory where tooling permits.
3. Verify target IDs and sources; recover decisions with reasons, objections, attribution, status and next actions. Report missing references.
4. If v0.3 files are missing or contain no relevant legacy state, read `context/memory.yaml` as an explicitly labeled v0.2 fallback. Do not silently interpret old records as new shared consensus.
5. Report registry entries, loaded agent IDs/paths, newly discovered records, unresolved links and concrete next step. Never report full cross-agent recovery without checking the registry. Do not write on load or automatically execute irreversible actions.

## Limits
Manual operations require repository tools. File writes are per-file and non-atomic across multiple files. No automatic background sync, graph engine, TTL, GC, physical context clearing or guaranteed model-independent recovery. A successful Git write is not a successful handoff test.
