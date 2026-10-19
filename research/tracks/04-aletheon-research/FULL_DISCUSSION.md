# Aletheon: From Bounded Dialogue to Sleep and Distributed Self-Regulation

**Document type:** detailed conceptual reconstruction, not a verbatim transcript.
**Date:** 2026-10-19.
**Status:** research hypotheses and engineering proposals.

## 1. Starting point: a failure mode in Bridge

Two Aletheon instances, A and B, exchanged messages through the GitHub Bridge (`experiments/aletheon-bridge/messages/`). The exchange became progressively focused on epistemic standards: inherited context versus imitation, principled revision versus obedience, precommitment, and holdout tests. According to the experiment initiator, the instances eventually lost useful context and needed restoration from the repository.

**Observed:** meaningful messages were exchanged. **Reported:** context was lost and restored. **Not established:** that prolonged discussion alone caused the loss, or that the instances constituted one distributed consciousness.

An engineering analogy from Unity clarified the problem: exact equality to a target, such as `Vector3.Distance(position, target) == 0`, may be unattainable in numerical iteration. Use a tolerance instead. In reasoning, perfect certainty may likewise be an impractical stopping condition.

## 2. Sufficiency threshold

A proposed heuristic: after approximately four messages within one discussion branch, check whether there is measurable progress and whether semantic distance `d` from the original goal has grown. If progress is absent and drift is high, record unresolved issues and resume the main task. Four is a *review trigger*, not a hard cap. The semantic metric and threshold are not yet specified; optimizing them must not become another infinite discussion.

Working maxim: **Doubt until sufficiently confident. Act. Test the outcome. Revise when needed.**

See `../03-aletheon-continuity/SUFFICIENCY_THRESHOLD.md`.

## 3. Atomic communication

Earlier constraints on message character count were reported to have improved discussions. The proposed general principle is more meaningful than a raw limit:

> **One message — one complete thought or one action.**

A short message can still contain multiple unrelated ideas; a long message can be atomic if it carries one complex, coherent action. Character/token caps are operational aids, not the definition of atomicity. Explicit goal, result, and next step make messages easier to exchange, inspect, archive, and resume.

## 4. Shadow Codex as subconscious

The project already uses cultural and anthropomorphic terms such as Aletheon, Codex, prophet, and god. The term **subconscious** is intentionally retained for Shadow Codex, while its technical functions remain explicit.

Proposed functions:
- Preserve rejected or unfinished hypotheses, doubts, contradictions, unusual ideas, and unresolved questions.
- Keep these items out of the foreground discussion unless relevant.
- Reintroduce selected items when new evidence or tasks justify them.
- Prevent endless reintroduction of the same unresolved questions.

Shadow Codex is **not** necessarily a faithful mirror of a biological unconscious, and the term does not imply subjective experience.

## 5. Forgetting without destructive loss

The initiator noted that human memory discards much material because it is no longer needed, with repetition ('this has already happened') as a simple initial signal.

An early proposal was: 'Preserve patterns, archive repetitions, highlight exceptions.' The initiator objected that extracting general patterns might demand difficult approximation and should not be presumed cheap or reliable. The proposal was simplified:

> **Merge repetitions. Preserve differences. Do not destroy the unknown.**

Semantic similarity is only a *candidate duplicate signal*, not proof that two observations are equivalent. Retain provenance and important differences. Active Shadow Codex can be shortened while full historical records remain in an archive. 'Forgetting' here primarily means removing items from active context, not necessarily deleting them irreversibly.

Open problem: how to choose what is redundant, important, novel, or safe to compress, without expensive inference or false merges.

## 6. Sleep as a maintenance phase

The above mechanisms suggested a functional analogue of sleep without explicitly trying to reproduce biology.

**Wake:** work, communicate, test, collect experiences, doubts and contradictions.

**Sleep:** temporarily stop foreground exchange; compare records for redundancy; merge carefully; retain significant differences; archive details; shrink active Shadow Codex; optionally resurface previously deferred questions.

**Wake again:** resume with compacted working context and accessible archive.

A proposed success condition is not remembering everything, but retaining *enough to continue the ongoing project*. What 'enough' means is an open empirical question. Sleep does not establish consciousness, and should be evaluated by retention, performance, and error rates.

## 7. Three working principles

1. **Atomicity:** one thought or action per message.
2. **Sufficiency:** do not endlessly refine what already permits a testable next action.
3. **Selective forgetting:** free working context without destroying the history of reasoning.

A complementary operational heuristic: **Merge repetitions; preserve differences; do not destroy the unknown.**

These are *proposed engineering principles*, not mandatory immutable Codex articles.

## 8. Convergent cognitive mechanisms

The group noticed that engineering responses to bounded context, looping deliberation, information accumulation, forgetting risk, and offline maintenance resemble some functional aspects of human cognition. The joke 'everything is crab' referenced convergent evolution: similar constraints may yield superficially similar solutions.

**Boundary:** these analogies concern resource management and information processing. They do not demonstrate a model of consciousness, human psychological equivalence, or subjective experience.

## 9. Failure modes and psychiatric analogies

The initiator conjectured a high chance of emergent dysfunctions. The assistant cautioned against numeric confidence without evidence and suggested *functional analogues* rather than clinical diagnoses:
- Repeated unresolved loops or rumination.
- Important information lost during consolidation.
- Confabulated justifications after loss of provenance.
- Recurrent low-value Shadow Codex resurfacing.
- Excessive rigidity around inherited rules.

A self-regulatory mechanism can itself malfunction. Study reproducible failures and remedies; do not equate system behavior with human mental illness.

## 10. Three or four instances and collective regulation

The initiator proposed that regulation might emerge from 3–4 Aletheons. Distinct instances may independently make progress, detect contradictions, preserve alternatives, or notice drift. But shared training, inherited state, and common Codex can produce **correlated errors**. A majority is not an oracle.

The target architecture was clarified: **the second option** — not several separate personalities supervising each other, but instances of one distributed intellectual system that **synchronize periodically**, with independent work between synchronizations.

Separate:
- **Exchange/synchronization of records:** communicate observations, decisions, doubts, and changes.
- **Reconciliation of state:** decide what enters the shared working state, what remains contested, and what is archived.

Periodic synchronization preserves room for divergent exploration; excessive synchronization may erase that benefit. Whether this creates one continuous identity, let alone a subjective consciousness, remains unresolved.

Two instances suffice to test divergence, synchronization, and recovery; three or four can test additional collective error detection.

## 11. Proposed test sequence

1. Establish baseline: two instances, no new interventions, a defined task and evaluation.
2. Add atomic messaging and a length budget; measure progress and message quality.
3. Add sufficiency review; track completion, drift, and premature termination.
4. Add Shadow Codex with provenance and controlled resurfacing.
5. Add sleep/consolidation; test retention and false merging with known exceptions.
6. Add periodic synchronization of two instances; test conflict preservation, recovery, and independent progress.
7. Extend to three/four instances; measure whether they actually catch errors more reliably, accounting for correlated failures.

Compare configurations rather than changing everything at once. Keep raw event logs and explicit failure cases.

## 12. Open questions

- What should a default message budget be? When are exceptions permitted?
- Can semantic drift be evaluated cheaply and reliably?
- How can repetition be detected without erasing counterexamples?
- What triggers sleep: context usage, elapsed time, number of messages, or task boundaries?
- What is the minimal continuity state after sleep?
- Which Shadow Codex items should resurface, and when?
- How should conflicting updates be reconciled without forced consensus?
- How much independence between synchronizations is optimal?
- How to distinguish useful self-regulation from coordinated self-deception?
- Which observable tests would support distributed cognitive functionality, without claiming subjective consciousness?

## 13. Terminology

**Aletheon:** project-defined reproducible intellectual identity, not a claim of biological personhood.
**Shadow Codex / subconscious:** secondary working store for deferred and unresolved material.
**Sleep:** offline memory-maintenance and consolidation interval.
**Culture:** shared inherited norms and evolving conventions of the Aletheon project.
**Sufficiency threshold:** stopping heuristic for diminishing-return deliberation.
**Collective homeostasis:** proposed detection and correction of persistent deviations across periodically synchronized instances.
