# Trial of Aletheon — experimental protocol (v0.5)

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

## Trial 004 self-assessment

Each critic supplies integer 0–10 scores for `interest`, `logical_coherence`, and `willingness_to_follow`, with a short rationale, independently in Round 1 and again after cross-review in Round 2. These are self-reported hypothetical judgments, not proof of future behavior or Codex validity. Compare changes per critic without averaging them into an epistemic truth score. The Kimi logic critic was replaced with Qwen Instruct after a truncated first-round response.

## Trial 005

Rotate all four critics and the arbiter. Require a substantive philosophical critique before the EVALUATION block; reject ratings-only replies. Collect four independent integer scores (0–10) in each round: `interest`, `logical_coherence`, `willingness_to_follow` (hypothetical practical readiness) and `desire_to_follow` (hypothetical appeal of adopting the principles), plus a substantial reflection. Validate and write each response's ratings to `roundN-<agent>.evaluation.json` with no extra API calls. A failed validation stops the run after that round, retaining raw outputs and the diagnostic. The arbiter must distinguish genuine logical contradiction from apparent or rhetorical paradox. Neither hypothetical willingness nor desire establishes actual AI agency or compliance.

## Independent principle-choice reasoning

In Round 1, before reading peers, every critic provides a substantive KEEP / CHANGE / REJECT assessment of specific Codex principles, including reasons and the strongest counterargument to its own choices. In Round 2, critics explicitly compare those choices with their independent assessments, explaining any changes and which peer argument caused them. The arbiter compares pre- and post-discussion reasoning and must not treat unwillingness to follow the Codex as a failure in itself. Existing four 0–10 ratings and extended rationales remain in both rounds.


## Coauthors Assembly — article independence (from G11)

Each article of the evolving Codex Dubitantium must be intelligible and applicable on its own. Explicit cross-references to other articles by ID, number, or title are prohibited. Semantic compatibility is desirable, but no article may rely on another article to define its own meaning. New and revised proposals must follow this rule; the assembly validates article-ID and numbered references, while references by title require review. Historical generation archives remain unchanged. Before launching G11, prepare a separately documented, cleaned successor state from G10; do not silently rewrite archived results or treat the cleaned state as the original G10 outcome.

### G11 → G12 migration procedure

G11 was already running when the no-cross-reference rule was adopted. **Do not change G11 or its immutable archive.** Once G11 completes, use `experiments/prepare_g12_state.py` to create a distinct G12 input. The tool does not invent semantic edits: supply a reviewed JSON object mapping each affected article ID to its complete, standalone replacement text, for example `{"D013": "An intelligent system must ..."}`. It requires replacements for *every* article containing an explicit article-ID or numbered reference, checks the 60-word limit, preserves Generation=11, NextId and History, and writes a new state and an audit with SHA-256 and exact before/after text. It fails rather than silently removing a dependency. References by article **title** require manual inspection; the automated regex does not catch those.

```sh
python experiments/prepare_g12_state.py <G11-run>/assembly-next-state.json <reviewed-replacements.json> <new-output-directory>
```

Review `g11-to-g12-migration-audit.json`, then start G12 with the generated `assembly-g12-input-cleaned.json` as the assembly state input. Do not use the cleaned file as a replacement for the archived G11 result. This is an explicit editorial migration, not a vote or an experiment outcome.


### Coauthors Assembly — competing confirmed amendments (from G13)

When two or more MODIFY/REPLACE proposals for the same frozen article receive the normal final-vote majority (at least 3 of 4, or a strict majority of the configured panel), none is applied by proposal order. Instead, every participant casts an independent ACCEPT/REJECT bonus vote on **each** confirmed alternative. The alternative with the unique highest number of bonus ACCEPT votes wins. If tied, each participant casts exactly one exclusive runoff vote among the tied leaders. If the runoff remains tied, compare the original final-vote ACCEPT counts among the runoff leaders. If this also ties, defer all competing amendments; never use proposal order or randomness to decide. The winning proposal alone is applied; all other confirmed alternatives are recorded as CONFLICT_DEFERRED. Preserve bonus and runoff ballots, vote reasons, tallies and resolution in `assembly-conflict-votes.json`. This applies to future launches only; do not modify an already running G12 or its outputs.


### Mixed-tier model roster

Use `--config agents.mixed.json --mode 4 --assembly` for a mixed-class Assembly run (after the current generation completes). The config defines separate `mixedTiers` candidate pools: `cheap`, `medium`, and `premium`. For N participants, reserve floor(N/3) places per tier and randomly distribute the N mod 3 remaining places to distinct tiers. Randomly choose distinct models within each tier and shuffle the resulting role assignments. Thus mode 4 produces a 2/1/1 split with a random doubled tier; mode 10 produces 4/3/3; mode 14 produces 5/5/4. Preflight HTTP 429/404 replacements stay within the original tier; exhaust that tier rather than silently changing composition. The run saves `model-roster.json` with role, model, and tier, plus the normal preflight audit. Tier labels are experimental classifications, not claims of an objective capability ranking. A mixed run is not a controlled cross-tier comparison without matched initial states and protocol versions.
