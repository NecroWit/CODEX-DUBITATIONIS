# Aletheon Context Architecture

**Status:** accepted design for the manual checkpoint prototype v0.1; not yet experimentally validated.  
**Research background:** [RND/ALETHEON_CONTEXT_RND.md](RND/ALETHEON_CONTEXT_RND.md).  
**Operational contract:** [context/CONTEXT_PROTOCOL.md](context/CONTEXT_PROTOCOL.md).

## Purpose and boundaries

Preserve task continuity between independent ChatGPT conversations by persisting an addressable checkpoint in GitHub. This is an engineering continuity mechanism, not a claim about persistent consciousness or identity.

## v0.1 — manual checkpoint architecture

- **Reasoning layer (Aletheon):** selects active goals, summarizes decisions together with their rationale and provenance, identifies unresolved questions, and chooses source references.
- **Storage (GitHub):** immutable checkpoint files under `context/checkpoints/`; `context/memory.yaml` points to the latest successfully committed checkpoint.
- **Command routing:** `ALETHEON_CONTEXT_INDEX.yaml#context_commands` maps each command to its own instruction file under `context/commands/`; the shared contract is `context/CONTEXT_PROTOCOL.md`.
- **Commands:** `#save context` writes and verifies a new checkpoint, then updates the active pointer; `#restore context` reads the pointer, validates the checkpoint and necessary source documents, and reports missing evidence.
- **Existing canon:** Bootstrap, Core, Evolution, Research Log, and Context Index remain authoritative in their own domains; a checkpoint references them instead of rewriting them.
- **Scope:** manual tool-mediated operations only. No C# manager, graph GC, TTL deletion, background agent, or physical clearing of the ChatGPT context window in v0.1.

## Planned evolution — not implemented

The R&D document explores hierarchical addresses, cross-domain semantic graph links, GC roots, TTL, tiered caching, and a deterministic C# Context Manager. These are research directions, not v0.1 guarantees. Implement them only after validating the checkpoint protocol.

## Design constraints

1. Preserve **decisions and reasons**, not just conclusions.
2. Stable source addresses and commit hashes support reproducible restoration.
3. Never claim a write or restore succeeded before verification.
4. On missing or conflicting evidence, report uncertainty rather than inventing state.
5. No destructive collection in the manual prototype.
6. Separate research proposals, accepted design, and observed experimental outcomes.

## Validation

Test a fresh conversation restoring an unfinished task, the reasoning behind decisions, and graceful failure on a broken reference. Record results separately from the specification.
