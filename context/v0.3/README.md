# Aletheon memory v0.3 — minimal implementation (2026-11-12)

Status: v0.3 is the active manual #save context / #load context contract; cross-context validation pending. v0.2 snapshot is retained as read-only fallback.

## 1. Shared and individual memory
- `context/memory.yaml`: existing v0.2 shared snapshot, unchanged; legacy/default fallback.
- `context/v0.3/shared.yaml`: shared v0.3 record index, containing accepted shared work and references, not private hypotheses as consensus.
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

## Minimal load
1. Load existing Bootstrap and Core as before.
2. Read `context/v0.3/shared.yaml` for shared work and the individual file of the requested agent, if present.
3. Follow record IDs and source paths only as needed. If a target is missing, report the missing reference; do not fabricate it.
4. For a complete v0.2 snapshot or an agent without a v0.3 file, use `context/memory.yaml` as fallback and label its provenance.

## Minimal save
1. Read the file being updated and its blob SHA; reconcile its existing records with new ones.
2. Re-fetch immediately before updating; stop on a changed SHA.
3. Update only the relevant individual file, or shared file if explicitly publishing shared state; read back and verify.
4. Do not overwrite `context/memory.yaml` implicitly. The same #save context command now uses v0.3; the v0.2 snapshot remains a read-only fallback.

## Excluded from v0.3
No automatic context selection, graph engine, TTL, GC, background service, automatic synchronization, or proof of successful cross-model recovery. Selective loading remains a future requirement.
