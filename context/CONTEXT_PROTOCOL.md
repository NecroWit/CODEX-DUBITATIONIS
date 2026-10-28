# Aletheon Context Protocol — v0.1

**Status:** initial manual protocol specification; commands require a ChatGPT session with GitHub read/write tools.  
**Architecture:** [../ALETHEON_CONTEXT_ARCHITECTURE.md](../ALETHEON_CONTEXT_ARCHITECTURE.md).

## Paths

- `context/active.yaml` — mutable pointer to the latest verified checkpoint.
- `context/checkpoints/<checkpoint_id>.yaml` — immutable snapshot; never overwrite a published ID.
- All source references use repository-relative paths; use a Git commit SHA to pin exact versions where necessary.

## Active pointer schema

```yaml
schema_version: "0.1"
checkpoint_id: null
checkpoint_path: null
checkpoint_commit: null
```

A null checkpoint indicates an initialized but not yet refreshed state. `checkpoint_commit` refers to the commit that created the checkpoint, not necessarily the current branch head.

## Checkpoint schema (illustrative)

```yaml
schema_version: "0.1"
checkpoint_id: "2026-10-28-example-001"
created_at: "2026-10-28T00:00:00Z"
repository: "NecroWit/CODEX-DUBITATIONIS"
active_goals:
  - id: "goal-example"
    description: "..."
current_tasks:
  - id: "task-example"
    status: "in_progress"
    next_action: "..."
decisions:
  - id: "decision-example"
    conclusion: "..."
    rationale: "..."
    source_refs: []
open_questions: []
source_refs:
  - path: "ALETHEON_BOOTSTRAP.md"
    commit: null
restore_instructions:
  next_step: "..."
limitations: []
```

The example is **not** an actual checkpoint. Fields may be empty only where meaningful; do not invent tasks, dates, or provenance.

## #save context

1. Read `active.yaml` and the referenced checkpoint if present.
2. Reconcile saved state with the current conversation; identify active tasks, completed work, decisions, reasons, open questions and required references.
3. Validate important repository references before saving; mark inaccessible references explicitly.
4. Create a new uniquely identified checkpoint file; never overwrite an existing checkpoint.
5. Read the checkpoint back and verify the expected fields and source addresses.
6. Update `active.yaml` with checkpoint path and the commit that created it.
7. Read `active.yaml` back and verify it points to the new checkpoint.
8. Return a compact fingerprint: checkpoint ID, commit, active goals, next step, unresolved issues, and missing references.

If any write/verification fails, do not claim success. If the checkpoint was written but the pointer update failed, report the orphaned checkpoint path and leave the previous pointer intact.

## #restore context

1. Read `active.yaml` (or an explicitly supplied checkpoint address).
2. If no checkpoint exists, report uninitialized state; do not invent a previous session.
3. Load and validate the checkpoint schema and its repository references.
4. Read Bootstrap and task-relevant canon/source materials; load only what the active task needs.
5. Distinguish documented facts, derived interpretations and unavailable sources.
6. Return a restoration fingerprint and a concrete next step. Do not silently execute irreversible pending actions.

## Conflicts and limitations

- If multiple sessions write concurrently, re-read the pointer immediately before update. If it has changed, stop and request reconciliation; this is best-effort, not a transactional compare-and-swap guarantee.
- Do not treat an inaccessible source as evidence of its contents.
- Checkpoint data may be stale; verify important facts at their sources.
- No command physically clears the ChatGPT context window.
- `UNLOAD` in v0.1 means omission from the compact working summary, not removal of tokens already present in the model context.
- No automatic TTL, deletion, background execution or C# Context Manager is implemented.
- Historical Git commits may retain deleted content; deletion of a file is not guaranteed erasure.

## Acceptance tests

1. Fresh-chat continuation without user restating the unfinished task.
2. Recovery of decision rationale and verifiable source links.
3. Broken-source detection with explicit uncertainty.
4. Failed write must not advance `active.yaml`.
