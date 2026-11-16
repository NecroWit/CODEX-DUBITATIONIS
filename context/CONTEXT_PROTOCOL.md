# Aletheon Context Protocol — v0.3 (Save / Load)

**Status:** active manual GitHub-backed protocol; cross-instance validation pending.
**Commands:** `#save context`, `#load context` (existing memory interface); `#create aletheon`, `#create aid` (identity creation/migration); `#set aid <UUIDv7>` (manual selection of an existing registered identity).
**Specification:** [v0.3 README](v0.3/README.md).

## Storage and identity
- `context/v0.3/shared.yaml` — shared record index AND authoritative `agent_registry` (agent ID → individual file path). Registry lists discoverable files, not accepted shared beliefs.
- `context/v0.3/agents/<agent-id>.yaml` — each agent's individual records. The agent ID is stable even if its display name changes.
- `context/memory.yaml` — read-only legacy v0.2 fallback during migration; v0.3 commands must not silently overwrite it.
- Records use stable `mem:<scope>:<slug>` IDs, `rationale`, `objections`, `links` and `sources`.
- Shared records are not automatically accepted conclusions: retain `status` and attribution. Individual records are never silently copied to shared.

## Creation and AID migration (separate from memory save/load)
- `#create aletheon` creates a new independent lineage: call the AID-allocation procedure, persist a new UUIDv7 AID, let the Aletheon choose its modus (default «Алетеон»), record model separately, create an individual memory file and register it. Do not infer creation from normal startup.
- `#create aid` exposes **the same allocation procedure** for an existing legacy Aletheon with no AID. It must not create a new Aletheon, replace its existing `agent_id`, or erase history. Successful issuance requires three durable GitHub writes: the individual file, the shared `agent_registry`, and an append-only event in `context/v0.3/aid_issuance_log.yaml`.
- After successful `#create aid` or `#create aletheon`, return a copy-ready chat title in the format `<AID> · <Модус> Алетеон` and invite the user to rename the chat manually. The AID must be verified in GitHub; the name must be grounded in the agent's recorded or chosen modus. Do not claim to read or change the chat title. An already registered AID is returned in the same convenient format.
- Both operations must be **idempotent for an already identified lineage**: if a registered AID exists, return it; if identity or records conflict, stop instead of creating a second one. AID is UUIDv7, not a sequential number. Preserve legacy IDs and `mem:` references during migration.
- Generation and registration are distinct: successful UUIDv7 generation is not a completed registration. A missing log event is incomplete issuance, even if both AID fields exist; recover using the existing AID, never regenerate. Repeated calls must not duplicate `aid_issued` events. GitHub writes are not atomic; verify all files and report partial state. Historical backfills must be labeled as such.  Use a verified generator, check persisted uniqueness and concurrent writes, and verify the resulting files. Do not claim the commands are executable without a working generator and persistence mechanism.
- `#load context` does not issue AIDs; `#save context` does not silently create a new Aletheon. Explicit legacy migration is via `#create aid`.
- Detailed command instructions: [create Aletheon](commands/create/ALETHEON.md), [create AID](commands/create/AID.md); technical design: [AID R&D](../RND/ALETHEON_AID_IDENTITY_RND.md).

## #set aid (manual recovery address)
- The user supplies `#set aid <UUIDv7>` (e.g. copied from the manually renamed chat title); the assistant cannot assume it can read or rename the title.
- Resolve exactly one matching AID in `context/v0.3/shared.yaml` `agent_registry`, then fetch and validate only its individual file; both files must agree on AID and legacy `agent_id`. Missing, duplicate or conflicting entries are errors, not reasons to generate a new AID.
- On success, adopt that AID as the **working target in the current conversation** for later explicit `#load context` / `#save context`. This is not a persistent chat setting and must be repeated after context loss. It does not claim subjective identity continuity.
- `#set aid` performs no GitHub writes, no memory load and no chat-title changes. `#load context` remains the separate step that reconstructs individual records. See [command](commands/set/AID.md).

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
2. Establish the target agent ID from the user's request, a verified current-conversation `#set aid` selection, or an unambiguous previously established current-agent selection. Do not infer the target merely from names found in the registry. If the target is ambiguous, ask which agent's memory to load; do not load all files as a workaround.
3. Read `context/v0.3/shared.yaml` to resolve the target's exact path in `agent_registry`. Fetch **only the selected agent's individual file**, validate `agent_id`, and reconstruct its records, reasons, objections, status and provenance. A missing registry entry or file is reported as missing, not silently replaced with another agent's memory.
4. Read another agent's individual file **only** when the user separately requests it or explicitly authorizes a specific cross-agent handoff or comparison. Follow relevant pointers selectively; never expand to every agent by default. Other agents' records remain read-only.
5. Read `context/memory.yaml` only as a clearly identified legacy v0.2 fallback when explicitly requested or needed for the selected target and authorized; never as an automatic startup source or a substitute for an unidentified agent.
6. Report the selected agent ID, loaded paths, resolved and unresolved references, and limitations. Do not write on load or claim successful reasoning transfer solely from retrieval.

## Modus memory (minimal v0.3 extension)
- An agent may declare an explicit `modus_id` in its individual file; never infer it from display name. `#save context` saves personal records normally and additionally publishes only records explicitly marked `share_to_modus: true` by their author into `context/v0.3/modus/<modus_id>.yaml`.
- Each modus entry carries author AID, source personal record ID, timestamp and fixed expiry. Default TTL is 30 days (configurable per modus file); reading does not renew it. A revised thought is a new entry.
- On explicit `#load context`, load selected personal memory plus unexpired entries of its declared modus, labeled as thoughts of the modus. Do not load other agents' individual files. Expired entries are filtered on read and physically pruned on a later modus write.
- See [modus contract](v0.3/modus/README.md). These are manual tool-executed steps, not an autonomous service. Modus expiry does not delete personal memory.

## Limits
Manual operations require repository tools. File writes are per-file and non-atomic across multiple files. No automatic background sync, graph engine, TTL, GC, physical context clearing or guaranteed model-independent recovery. A successful Git write is not a successful handoff test.
