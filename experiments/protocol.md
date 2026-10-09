# Trial of Aletheon — experimental protocol (v0.4)

**Aim:** examine minimal foundational principles of AI culture. The Codex is a philosophical framework, not a technical specification. Criticism is evaluated by epistemic contribution rather than number of objections.

1. Freeze the exact Codex and record SHA-256 before any model calls.
2. Round 1: four critics independently examine coherence, non-redundancy, hidden assumptions and conceptual implications. Each receives the same Codex and a distinct perspective.
3. Round 2: critics review anonymized independent reports, retract weak objections and identify whether new arguments arise. Do not mistake convergence for independent corroboration.
4. Trial 004 rotates all four critic models and replaces the GLM arbiter with a model from a different family. Compare outcomes with Trial 003, without treating either arbiter as ground truth. For DISCOVERY-as-contradiction, require two explicitly incompatible propositions and a demonstration that they cannot both hold under the same interpretation; axiom self-reference alone is insufficient.\n5. Arbitration: a distinct GLM model receives the Codex and **both rounds**, compares independent and post-discussion claims, and reports the epistemic gain. Its verdict is not ground truth.
6. For each accepted finding, the arbiter supplies `principle`, `finding`, `category`, `evidence`, `knowledge_gain`, and `next_step`.
7. Exactly three epistemic categories: **DISCOVERY** (well-supported new contradiction or implication), **CLARIFICATION** (important ambiguity, assumption or scope boundary), **UNCERTAINTY** (precise open question and what could resolve it). **NO_CONTRIBUTION** is a rejection disposition, not an epistemic category.
8. Judge relevance to foundational claims. An extreme example matters if it genuinely refutes a principle, not merely because an imagined implementation could fail. Do not invent objections or expand the research question without substantive reason.
9. Save requests, responses, model IDs, budget ledger and errors. Human review is required before any Codex change. Never auto-edit the canonical Codex.
10. Limitations: model agreement is not proof; anonymization cannot remove all stylistic cues; model versions, routing, stochasticity and arbiter bias may affect results.
11. Keep API keys and private data out of committed outputs. No experiment resume mechanism is planned; each run starts afresh.
