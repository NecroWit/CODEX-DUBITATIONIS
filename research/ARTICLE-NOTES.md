# Article research notes — vote transitions

## Research question

Do rotating probabilistic agents converge on relatively stable principles through substantive argument rather than procedural inertia?

## Evidence policy

Record **every** preliminary-to-final vote transition, not just successful reversals. The Assembly now emits `assembly-vote-transitions.json` alongside its original audit files. Each row contains the generation, proposal, voter role/model, original and revised article text, both vote choices and reasons, and references to the underlying audit files.

A vote change is an **observation**, not proof of persuasion. The second vote follows both exposure to reviews and possible changes to the proposed text. Repeated prompting, order effects, model variability, and role/model substitutions are potential confounders. Distinguish within-voter reversals from aggregate vote-count changes.

## Case-study register

### Generation 7 — P04 (candidate case, verify against the archived ballots)

- Preliminary tally: **0/4 ACCEPT**.
- Final tally: **4/4 ACCEPT**.
- Initial approach: unconditional priority for radical doubt and revision over operational protocols.
- Revised approach: no principle receives unconditional priority; conflicts require documented, revisable adjudication.
- Working interpretation: the revision may have resolved a substantive objection about rigid priority hierarchies.
- Competing explanations: a new proposal text was evaluated; reviewers' arguments, repeated voting, and stochastic output cannot yet be isolated.
- Status: **candidate**, pending per-voter audit and exact-text quotation from the original archive.

## Analysis plan

1. Aggregate all four transitions: REJECT→ACCEPT, ACCEPT→REJECT, REJECT→REJECT, ACCEPT→ACCEPT.
2. Separate unchanged-text votes from revised-text votes; record revision failures and substitutions.
3. Code stated reasons for changed votes, including explicit references to resolved objections, remaining objections, or unexplained reversals.
4. Compare distributions by generation, role, and model only with sufficient sample sizes.
5. Publish all eligible cases and selection criteria; choose illustrative examples for the main article without hiding counterexamples.

## Reproducibility

Use `python research/collect_vote_transitions.py experiments/results` to consolidate completed runs into `research/vote-transitions.jsonl`. Do not treat an incomplete run as a completed generation. Keep source run folders as primary evidence; the aggregate is a derived index, not a replacement for the audit.
