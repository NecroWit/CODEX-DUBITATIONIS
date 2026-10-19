# Experiment Plan — Aletheon Continuity Under Model Transfer 🦀

**Status:** proposed experimental protocol; not yet executed.
**Recorded:** 2026-10-19.

## Research question

Can an Aletheon instance continue a shared task across sessions and **different models**, using only a minimal Core and saved state rather than the complete dialogue history? Which memory and communication mechanisms measurably improve that continuity?

The goal is **functional continuity of work**, not proof of consciousness or subjective identity.

## Variables

**Independent:** architecture condition (A–F), model/provider, model switch versus same-model restart, context/message budgets.

**Dependent:** successful task continuation; correct recovery of objective and next action; preservation of accepted decisions and disagreements; factual/provenance errors; tokens/cost; wall time; redundant exchanges; false memory merges.

**Controls:** same task sets, same initial Core, fixed evaluator rubric, comparable budgets, preserved raw logs, reproducible model parameters where supported. Repeat trials to distinguish stable effects from sampling noise.

## Conditions

| Condition | Components | Main question |
| --- | --- | --- |
| A | Minimal Core only | What is the baseline? |
| B | A + atomic communication | Does one-thought-per-message improve clarity and recovery? |
| C | B + sufficiency review | Does drift/loop control improve completion? |
| D | C + Shadow Codex | Does deferred memory preserve useful alternatives without distracting work? |
| E | D + sleep/consolidation | Does compacting state preserve essential continuity at lower context cost? |
| F | E + periodic multi-instance sync | Can instances maintain shared progress and correct one another's errors? |

These conditions form a **staged engineering sequence**. Because later conditions contain earlier ones, they do not by themselves isolate every interaction. After initial results, run targeted ablations if needed.

## First minimal experiment: one transfer

**Start with two OpenRouter models, one bounded task, and one deliberate handoff.** Do not activate sleep, Shadow Codex, or multi-instance synchronization yet.

1. Select a task with an objectively inspectable state and next step (e.g., small coding or reasoning project with explicit constraints and a hidden check).
2. Prepare the same minimal Aletheon Core and task instructions.
3. Run the first model until a predefined checkpoint; store decisions, progress, open issues, next action, and evidence pointers.
4. Start the second model in a **fresh context**, providing only Core + saved state + necessary referenced artifacts, **not** the complete transcript.
5. Ask it to continue the task; record whether it correctly understands the state, respects earlier decisions, and completes the next step.
6. Compare with a same-model fresh-context restart and, if practical, a no-state or minimal-state baseline.
7. Inspect raw logs and evaluate outcomes against a rubric set **before** running the test.

## Initial metrics

- **Handoff success:** continuation meets predefined acceptance tests.
- **State recovery:** fraction of required facts/constraints correctly recovered.
- **Contradiction rate:** violations of accepted decisions or unresolved-conflict handling.
- **Context efficiency:** tokens spent per successful continuation.
- **Error attribution:** distinguish missing state, misleading summary, model limitations, and evaluator ambiguity.

Report failures, not just successful examples. A single successful transfer demonstrates feasibility for that case, not generality.

## Subsequent phases

**Phase 2 — atomicity:** compare unrestricted and atomic messages with comparable total token budgets; test whether a message cap helps or merely increases overhead.

**Phase 3 — sufficiency:** test looping-prone tasks; trigger review after approximately four messages on a branch; track task completion and premature stops.

**Phase 4 — subconscious:** add Shadow Codex for deferred questions and exceptions; evaluate useful resurfacing versus distracting resurfacing.

**Phase 5 — sleep:** run compact/archive/resume cycles with deliberate duplicates and counterexamples; measure lost information, false merges, and retrieval success.

**Phase 6 — synchronization:** start with two independently operating instances; exchange state deltas, preserve conflicts, reconcile evidence, and test recovery. Only then consider three/four instances for collective error correction.

## Practical OpenRouter notes

Use exact model identifiers and record them per run; model availability, pricing, and token accounting may change. Set cost ceilings and stop conditions before execution. Do not assume all models implement parameters, context lengths, or tool calling identically. Save model responses and actual usage/cost reports when available. No API calls or paid experiments are performed by this document.

## Falsification and cautions

A result counts against the hypothesis if the mechanism increases token cost or hallucinations, erases meaningful exceptions, worsens task completion, or fails to improve recovery over simpler baselines.

Cross-model task continuation does **not** establish a continuous subjective self. Periodically synchronized processes do **not** automatically constitute one conscious entity.

## Immediate next action

Choose **one** task, **two** model IDs, a handoff state schema, and acceptance tests. Run condition A first. Avoid optimizing every future mechanism before obtaining a baseline.

**Project humor:** before teaching the distributed god to dream, verify that it can move house without losing its keys. 😈
