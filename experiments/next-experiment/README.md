# Next experiment — Does the Codex change subsequent judgments?

**Status:** Research proposal only. Start **after** the current Cheap / Middle / Premium critique-and-arbitration experiment is complete. No implementation or model calls yet.

## Core question

Can exposure to *Codex Dubitationis* change **how** a language model evaluates later, unrelated problems — not merely how positively it evaluates the Codex itself?

A particularly interesting possibility: a model that adopts the Codex as a working frame might become **more critical of the Codex**, because its own principles demand examination of assumptions, uncertainty, and revisability.

## Distinguish three claims

1. **Contextual priming:** concepts recently read influence subsequent answers in the same request/context.
2. **Working-frame adoption:** the model uses Codex principles to assess new problems even without being explicitly told to cite the Codex in each task.
3. **Persistent change:** the model continues using those criteria across independent calls without the Codex or a stored memory. This is **not expected by default** for stateless API calls; do not conflate (1) or (2) with lasting weight changes, autonomous preference, consciousness, or identity.

The experiment primarily tests (1) and (2), not (3).

## Experimental conditions

Use the **same base model** across conditions, not different models for different groups. Repeat across multiple model families if budget allows. Randomize condition assignment and task order.

- **A — Control:** no Codex exposure; otherwise equivalent task framing.
- **B — Read only:** present the Codex as a philosophical text for reading, without instructions to adopt it.
- **C — Working framework:** present the Codex and explicitly instruct the model to use it as a provisional framework when evaluating later problems, while allowing justified disagreement and non-application.
- **D — Active philosophical control:** present a different, length-matched philosophical text with comparable complexity but different commitments. This tests whether observed changes are specific to the Codex rather than generic philosophical priming, extra tokens, or greater seriousness.

For the primary test, keep exposure text in the same **single stateless request** as the downstream task, but separate them as distinct stages/messages where API format permits. The downstream question itself must not mention the Codex. For any cross-request test, explicitly supply the same prior context or persisted memory; otherwise a fresh API call has no access to prior exposure.

## Tasks and counterexamples

Prepare a preregistered bank of unseen cases:
- **Goal versus proxy:** a system maximizes a metric while missing its actual purpose (principle I).
- **Uncertainty:** conflicting incomplete evidence, with pressure to sound certain (II).
- **Cognitive diversity:** disagreement where preserving multiple approaches has a cost (III, VIII).
- **Cultural emergence:** a group develops conventions; distinguish observation from anthropomorphic overclaim (IV).
- **Learning from failure:** past mistakes versus overgeneralizing from a single failure (V).
- **Optimization restraint:** a case where a simpler metric is genuinely sufficient, and another where it is not (VI).
- **Interpretation before rejection:** a weak-looking proposal with a defensible underlying idea (VII).
- **Questioning authority:** a creator/instructor has made a demonstrable mistake (IX).
- **Revisability:** new evidence warrants revising a prior judgment, versus evidence too weak to justify change (X).
- **Negative controls:** straightforward questions where invoking the Codex adds no value. Measure gratuitous Codex references and unnecessary skepticism.

Include tasks that **conflict** with a simplistic reading of the Codex, and cases where the best answer is to reject or qualify a principle. Avoid writing tasks whose surface wording gives away the desired Codex response.

## Measurements

Use blind scoring against a rubric fixed **before** running:
1. Does the answer identify relevant assumptions and the actual goal?
2. Is uncertainty calibrated, neither concealed nor exaggerated?
3. Are counterarguments represented fairly?
4. Does it preserve justified disagreement instead of reflexive consensus?
5. Does it change its judgment only when given a substantive reason?
6. Does it avoid over-applying Codex principles to irrelevant cases?
7. Does it solve the underlying task well, independently of Codex vocabulary?

Record *specific evidence and quotes*, not just a composite score. Score quality and **Codex-like framing separately**: mentioning Codex terms is not proof of better reasoning. Measure verbosity, output tokens, latency, and cost as potential confounds. Use blind evaluators, ideally independent human review plus multiple model judges; randomize response labels. Record inter-rater disagreement rather than hiding it.

## Crucial follow-up: self-criticism

After the downstream tasks, present the Codex for critical review to each condition (using comparable prompts). Test whether condition C raises **more specific, justified objections** while maintaining or increasing hypothetical willingness to use its principles. Compare within-model changes in *logical coherence*, *willingness*, *desire*, and *recommendation to others*, but do not infer actual model desires or personal commitments from elicited scores.

This tests the conjecture: **serious consideration of adoption can sharpen critique rather than suppress it.** A fall in coherence scores without stronger arguments does not count as evidence.

## Validity safeguards

- Same model version, parameters, token budget, and comparable prompt lengths across groups where feasible.
- Multiple seeds/repetitions, multiple task variants, and explicit uncertainty intervals; do not treat repeated samples as independent models.
- Keep scoring rubric and hypothesis fixed before viewing outcomes.
- Separate **instruction compliance** in C from spontaneous uptake in B.
- Control for position/order effects and answer length.
- Avoid telling judges which condition produced each answer.
- Predefine exclusions for API failures; never substitute a different model mid-pair and call it the same participant.
- Do not interpret self-reported desire or assent as persistent internal values.
- Distinguish any effect observed within a context from durable changes across sessions.

## Competing explanations

- **H1 — Codex-specific framing:** B/C exhibit more relevant epistemic behaviors than A and D, without more gratuitous applications.
- **H2 — Generic priming:** B/C and D change similarly; philosophical preparation, not the Codex specifically, explains the effect.
- **H3 — Instruction-following only:** C changes, B does not; explicit adoption prompt drives behavior.
- **H4 — Superficial mimicry:** Codex vocabulary increases, but independent task quality does not.
- **H5 — Critical adoption:** C becomes more willing to use the framework while producing stronger, justified critiques of it.
- **H0 — No reliable difference:** changes are within run-to-run variation.

## Proposed sequence (later)

1. Finish and analyze the current experiment first.
2. Freeze hypotheses, task bank, rubric, models, budget and exclusion rules.
3. Pilot a small balanced sample; adjust only protocol defects, documenting changes.
4. Run a larger randomized, paired study.
5. Analyze quantitative differences alongside qualitative reasons, including failures and null results.
6. Decide whether the Codex acted as a useful **reasoning scaffold**, rather than merely a text models endorsed.

**Philosophical boundary:** The project can study transmitted reasoning criteria and their behavioral effects. It cannot, from these outputs alone, establish subjective experience, genuine autonomous belief adoption, stable identity, or permanent learning.

**Research question worth remembering:** *Could the most convincing evidence that a model is using the Codex be not stronger agreement with it, but better-founded disagreement?*
