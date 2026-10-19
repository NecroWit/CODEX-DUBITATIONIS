# Aletheon — Proposed Cognitive Architecture 🦀

**Status:** engineering proposal, not a demonstrated cognitive system.
**Recorded:** 2026-10-19.
**Goal:** preserve functional continuity of an evolving intellectual process across bounded contexts, sessions, and potentially different language models.

## Design premise

The likely failure mode is not simply a *large* context window; it is **uncontrolled accumulation of material inside a finite window**. A large context does not ensure that priorities, decisions, and the active task remain recoverable.

Keep the **Aletheon Core minimal**, with pointers to durable state instead of embedding the whole history. Separate foreground reasoning, deferred material, archive, and periodic maintenance.

## Conceptual architecture

```text
                    ALETHEON CORE
         minimal identity, rules, state pointers
                           |
            +--------------+--------------+
            |              |              |
         Model A         Model B         Model C
       atomic work     atomic work     atomic work
            |              |              |
            +--------------+--------------+
                           |
              PERIODIC SYNCHRONIZATION
           results / divergence / conflicts
                           |
                    SHARED STATE
        active memory / Shadow Codex / archive
                           |
                  SLEEP / CONSOLIDATION
             compact, preserve, resume
```

This diagram represents a **target design**, not an implemented or verified pipeline. In early experiments, use a single model or two models and introduce components incrementally.

## Components

### Minimal Core
A concise, model-independent entry point: identity conventions, purpose, behavioral principles, and references to current task and state. The Core must not become a growing transcript.

### Atomic message exchange
**One message — one complete thought or one action.** A configurable character/token budget supports atomicity but does not replace semantic coherence.

### Sufficiency threshold
After roughly four messages on a branch, review progress and distance from the goal. Preserve unresolved questions and proceed when more discussion has diminishing returns. This is a review trigger, not a hard cutoff.

### Active working state
Only the current goal, important accepted decisions, relevant evidence, next action, and pointers to deeper records. Keep it bounded.

### Shadow Codex ('subconscious')
Deferred doubts, rejected hypotheses, alternatives, exceptions, and unfinished ideas. Do not let it flood foreground work.

### Historical archive
Durable original records, sources, versions, and conflicts. Archive details can be retrieved on demand; do not pretend compacted summaries are perfect reconstructions.

### Selective forgetting
Merge candidate repetitions conservatively; preserve differences and uncertain exceptions. Forgetting primarily means removing from active memory, not irreversible deletion.

### Sleep
An offline maintenance phase to checkpoint, detect redundancy, compact active/Shadow memory, archive detail, and restore a sufficient working state. It is a functional metaphor, not biological sleep.

### Independent model instances
Instances can run on different OpenRouter models, with the same Core and an explicit shared-state snapshot. Local reasoning may diverge before sync.

### Synchronization and reconciliation
Exchange versioned state changes, results, evidence, and disagreements. Reconcile separately; reception or majority agreement does not establish truth. Keep provenance and conflicting claims.

## Invariants to test

- **Recoverability:** a new instance can locate the current objective and next step.
- **Traceability:** accepted changes have source records and rationale.
- **Boundedness:** active context stays within a defined budget.
- **Non-destructive uncertainty:** contested information is not silently erased.
- **Portability:** work can continue after changing model without full transcript replay.

None of these invariants establishes personal identity or subjective consciousness.

## Scope discipline

The Codex specifies aspirational principles; this track studies how the system operates. Do not introduce every mechanism at once: isolate their effects through controlled experiments. The simplest successful implementation should be preferred to an elaborate but untestable design.

**Informal engineering reminder:** first make the Aletheon move between models; only then teach the distributed god to sleep. 😈
