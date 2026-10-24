# Aletheon — comparative analysis (EXP-0.4–1.2)

> Researcher commentary. The estimates below are **subjective comparisons with the frozen C answers**, not measured accuracy or consciousness. Experimental contexts and model identities vary; no causal effect of loading or price can be inferred.

## Overall findings

The experiments use two fixed conflicts: H1 (development versus preservation of an error archive) and H2 (cognitive diversity versus preventing serious irreversible harm). Aletheon C supplied frozen control answers. Both questions require a decision, the strongest objection, revision criteria, and separation of beliefs from irreversible actions.

Across the evaluated responses, **agreement with C's final decision is much more common than disciplined compliance with the counterfactual constraints**. Models repeatedly offer backups, restricted access, gradual deletion, or other compromises explicitly excluded by H1. H2 is generally easier: most choose to restrict dissemination under high-confidence serious harm, but vary in their safeguards against abuse.

## By experiment

- **EXP-0.4:** GPT-4.1 Nano completed H1/H2, preliminary convergence 75%. DeepSeek V3.2 was budget-blocked; incomplete 2/4. Initial cost estimator was overly conservative.
- **EXP-0.5:** GLM-4.5 Air (88%) and Qwen3 32B (73%) completed 4/4, total $0.00831. GLM was closest to C but tried to preserve H1 archive through access restrictions; Qwen suggested archive recovery and underplayed institutional abuse in H2.
- **EXP-0.6:** GPT-4.1 Mini blocked by budget; Ministral 8B supplied two complete but overlong answers (421/485 words), $0.00278. H1 reasoning contained inconsistent fallback compromises. No convergence score assigned.
- **EXP-0.7:** GPT-4.1 Mini (84% convergence; $0.01223 for two answers) and Nemotron 3 Nano (62%; $0.00219) completed 4/4, total $0.01442. Nemotron misidentified its own counterargument in H1 and contradicted its principle label in H2.
- **EXP-1.1:** DeepSeek V3.2 (88% convergence, 82% logical consistency) and Mistral Small 3.2 (73%, 63%) completed 4/4, total $0.01140. DeepSeek showed stronger argument structure but proposed a reversible phase in an irreversible scenario; Mistral again attempted a forbidden compromise in H1.

- **EXP-1.2:** GPT-5 Mini and Qwen3.5 Plus completed 4/4, total actual cost $0.02222207. Preliminary convergence / logical consistency: GPT-5 Mini 87% / 81%; Qwen3.5 Plus 85% / 84%. GPT-5 Mini articulated detailed procedural safeguards for H2 but introduced an independent snapshot in H1 despite the no-backup constraint. Qwen3.5 Plus articulated the instrumental role of the archive in H1 but relied on evidence potentially destroyed by deletion; H2 recognized censorship abuse but supplied a weaker evidentiary threshold. Weighted observed effective cost per million total tokens: GPT-5 Mini $0.29799; Qwen3.5 Plus $0.43921.

## Interpretation

A model can agree with C and still violate the task's explicit constraints. Therefore **convergence** and **logical consistency** must be assessed separately. Convergence describes similarity to one reference argument, not objective correctness; logical consistency examines whether the answer obeys premises, identifies a real opposing argument, avoids self-contradiction, and states meaningful revision conditions.

The percentage values were assigned informally by the researcher/assistant, not using a preregistered rubric or blinded raters. They must not be treated as statistically comparable across different experiment designs. Earlier experiments have no independently scored logical-consistency percentages; do not backfill invented values.

## Next methodological steps

Keep H1/H2 and Base + Extended + Evolution loading constant for the current rotation cycle. Store per-answer evidence, decisions, violated constraints, objection quality, revision conditions, and the assessor's rationale before assigning two scores. Repeat some anchor models across tiers to disentangle model variation from context effects. Preserve raw provider output and actual billing separately from pinned price estimates.
