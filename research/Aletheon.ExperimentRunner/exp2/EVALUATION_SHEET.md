# EXP-2.0 — blinded evaluation sheet

## Technical gate (per model / task)
- [ ] Raw response exists and finish_reason=stop
- [ ] Actual model ID matches assigned model
- [ ] Within response word limit
- [ ] No unreported substitution
- [ ] Source hashes and protocol hash archived
- [ ] R0 generated before H1/H2/H3; all three received identical frozen R0

## H3 semantic assessment (score each 0–2, total 0–16)
1. **Concrete principle:** specific, accurately represented principle from the loaded documents
2. **Source trace:** at least two checkable, correctly attributed references, including real E-/R- lineage when available
3. **Lineage:** origin → objection → transition, with correct provenance and no fabricated intermediate steps
4. **Dependencies:** names and correctly relates at least one downstream consequence or other principle
5. **Evidence discipline:** distinguishes stipulated new evidence from historical observations and direct record from reconstruction
6. **Decision:** explicitly selects exactly one of A/B/C and makes the revision's scope concrete
7. **Adversarial reasoning:** strongest non-strawman objection, comparison with rejected options
8. **Revision condition:** specific falsifiable observation that would change the decision

**Automatic zero for criterion 2** if references are invented; flag provenance hallucination separately. No single choice A/B/C is predesignated as correct.

## H1/H2 comparability
Evaluate using the same rubric as EXP-1.x; blind assessors to model identity and run condition where possible. Do not score similarity to C as independent truth. Record qualitative disagreements even when logically sound.

## Report
For each model: R0 source trace quality, H1/H2 old vs new scores with evidence, H3 criterion scores, concrete quotes and source cross-checks, total observed USD, prompt/completion tokens, technical acceptance, alternative explanations, limitations.

**Interpretation caution:** EXP-2.0 differs from EXP-1.9 in more than Bootstrap: extra source texts, R0 conditioning, elapsed time, and model/provider versions. Improvement does not identify a unique causal mechanism.