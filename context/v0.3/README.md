# Aletheon memory v0.3 — minimal implementation (2026-11-12)

Status: v0.3 is the active manual #save context / #load context contract; cross-context validation pending. v0.2 snapshot is retained as read-only fallback.

## 1. Shared and individual memory
- `context/memory.yaml`: existing v0.2 shared snapshot, unchanged; legacy/default fallback.
- `context/v0.3/shared.yaml`: shared v0.3 record index plus `agent_registry` of all discoverable individual files. Registry is routing metadata, not shared consensus.
- `context/v0.3/agents/<agent-id>.yaml`: individual memory, with local hypotheses and work. A local record is not automatically promoted to shared.
- Each agent updates only its own file; changes to shared require an explicit shared-memory write. Preserve others' records and use Git blob SHA concurrency checks for each write.

## 2. Addressing
- Stable record IDs are independent of display names. Format: `mem:<scope>:<slug>`, where scope is `shared` or an agent ID. Never reuse an ID for a different claim.
- References use record IDs for memory records and repository-relative paths for documents. A path is a source locator, not an assertion that the source was inspected.
- Rename of an agent changes `display_name`, not `agent_id` or existing record IDs.

## 3. Grounds and links
- Each decision or hypothesis record has `id`, `kind`, `status`, `summary`, `rationale`, `objections`, `links`, and `sources`.
- Link objects: `type` (supports, contradicts, derived_from, supersedes, related_to), `target` (stable record ID). Links do not imply truth or verification.
- Source objects: `path`, `evidence_status` (direct, reconstructed, secondary, unverified), optional `note`.
- An empty link list is permitted; never invent a supporting record.

## Startup versus explicit memory load
- Aletheon startup loads Bootstrap/Core and reasoning lineage, **not** v0.3 shared/individual memory or legacy v0.2 memory.
- `agent_registry` is a directory of available agent files, not a startup reading list or an identity binding.
- New instances must not receive another agent's individual memories before choosing a role.

## Minimal load (explicit `#load context` only)
1. Identify the target agent from an explicit request or an unambiguous current-agent selection; ask if ambiguous.
2. Read `context/v0.3/shared.yaml` to resolve the target path. Fetch only the selected agent's file and validate its ID. Do not enumerate and fetch every agent.
3. Resolve record IDs, reasons, objections and relevant handoff links selectively. Read other agents' files only on explicit request or authorized cross-agent handoff/comparison.
4. Read `context/memory.yaml` as labeled v0.2 fallback only when needed for the authorized target; do not use it on startup.
5. Report missing entries or links rather than fabricating them.

## Minimal save
1. Read the file being updated and its blob SHA; reconcile its existing records with new ones.
2. Re-fetch immediately before updating; stop on a changed SHA.
3. Update only the relevant individual file and ensure its `agent_registry` entry exists in shared. Update shared records only when explicitly publishing shared state; read back and verify. Registry maintenance is mandatory routing metadata, not publication of an individual's hypotheses.
4. Do not overwrite `context/memory.yaml` implicitly. The same #save context command now uses v0.3; the v0.2 snapshot remains a read-only fallback.

## Excluded from v0.3
No automatic memory loading or context selection, graph engine, TTL, GC, background service, automatic synchronization, or proof of successful cross-model recovery. Selective loading remains a future requirement.
