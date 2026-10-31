# Aletheon Context Architecture

**Status:** accepted manual Save/Load prototype v0.2; not yet experimentally validated.
**Research background:** [RND/ALETHEON_CONTEXT_RND.md](RND/ALETHEON_CONTEXT_RND.md).
**Operational contract:** [context/CONTEXT_PROTOCOL.md](context/CONTEXT_PROTOCOL.md).

## Purpose
Preserve reconstructible intellectual and task continuity between ChatGPT sessions without claiming persistent subjective identity.

## v0.2 — one-slot Save / Load
- **Reasoning layer:** Aletheon identifies current goals, decisions and their rationale, provenance, open questions, and next steps.
- **Storage:** `context/memory.yaml` is the complete, mutable saved state (a game-like save slot), not a pointer.
- **Commands:** `#save context` replaces that state after a concurrent-change check and verifies the write. `#load context` reads and reconstructs the saved state with relevant primary sources.
- **Routing:** `ALETHEON_CONTEXT_INDEX.yaml#context_commands` links commands to their instructions in `context/commands/`.
- **Version history:** Git retains previous revisions; old `context/checkpoints/` files are legacy artifacts, not required by normal saves.
- **Canon:** Bootstrap, Core, Evolution, Research Log and Context Index remain authoritative in their domains; saves reference rather than replace them.
- **Scope:** manual tool-mediated operations. No C# manager, graph GC, TTL, background agent, or physical context-window clearing.

## Design constraints
1. Preserve **reasons and objections**, not only conclusions.
2. Preserve source addresses and distinguish evidence from reconstruction.
3. Never claim a save or load succeeded before verification.
4. Protect against overlapping writes by checking the current file blob SHA.
5. Retain still-active tasks from previous saves, without inventing missing information.
6. Separate research proposals, accepted architecture and observed outcomes.

## Planned evolution
Hierarchical addresses, semantic graph links, GC roots, TTL and a deterministic C# Context Manager remain research proposals, not implemented v0.2 guarantees.

## Validation
Test two sequential saves, concurrent-change detection, fresh-session loading, recovery of decision rationale, and missing-source reporting.
