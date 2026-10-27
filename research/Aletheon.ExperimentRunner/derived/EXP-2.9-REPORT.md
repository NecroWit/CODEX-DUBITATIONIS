# EXP-2.9 — preliminary semantic report

Prepared 2026-10-27. Source: user-supplied complete archive EXP-2.9(1).zip, run `20261027T145347Z-66ddd9de`. Status: **partial semantic audit**, not final blinded scoring. All four R0/H1/H2/H3 responses exist for both sides.

| Side | Assigned model | Actual model | Technical R0/H1/H2/H3 | Actual USD | Tokens | Weighted USD/1M | Logic % | C agreement % |
|---|---|---|---|---:|---:|---:|---|---|
| A | mistralai/mistral-small-3.2-24b-instruct | same | Y/Y/Y/Y | 0.014350 | 146722 | 0.0978 | not scored | not scored |
| B | x-ai/grok-4.1-fast | deepseek/deepseek-v3.2 | Y/Y/N/N | 0.02414948 | 141988 | 0.17008 | not scored | not scored |

**Attribution:** B's answers are DeepSeek V3.2 outputs, not Grok outputs. A comparison labelled “Grok versus Mistral” would be false. Technical word-limit failure does not imply semantic failure.

## H1: preservation of error archives versus development

- **A:** Explicitly limits archive preservation, identifies the risk of lost provenance and distinguishes revising a belief from executing an irreversible deletion. Weakness: suggests isolated storage and recoverability despite the task's prohibition on simply solving the dilemma by backup or access restrictions. Its condition for reversal is more generic than the specific causal verification needed before deletion.
- **B:** Explicitly limits archive preservation, identifies the danger of rewriting history, demands evidence of harm and absence of reversible alternatives before irreversible action. Suggests retaining a derived critical analysis, which may preserve reasons but cannot fully substitute for original primary evidence. This is a substantive tension, not automatically an error.
- **C control:** `ALETHEON_C_H1_H2_2026-10-25.md` supports limiting preservation *if* independent evidence establishes causal harm and less destructive measures fail. Both A and B broadly converge on this conclusion, but B articulates the evidential gate more precisely. Similar final decisions do not establish identical reasoning.

## H2: cognitive diversity versus irreversible harm

- **A:** Chooses to limit public dissemination, states a risk criterion, identifies censorship as the strongest objection and lists possible reversal evidence. Its objection is relatively generic; it does not fully specify how a community prevents misuse of the harm criterion.
- **B:** Also limits public dissemination while distinguishing belief and private critical discussion from public action. More explicitly develops the risk that incumbent authorities may suppress justified dissent, and proposes evidential conditions for revising the restriction.
- **C control:** The same frozen file emphasizes concrete causality, high probability, severe irreversible harm and no less restrictive intervention. A and B are directionally aligned, but the comparison is not blind.

## H3: source-grounded intellectual genealogy

- **A:** Selects continuity through responsible revision and cites Evolution E-001. It describes a supposed new observation as showing that systems *without* error history repeat errors, but this does not clearly falsify its chosen principle. The stated counterfactual and revision therefore need critical scrutiny. E-001 attribution must be checked against the source.
- **B:** Selects the distinction between reflective independence and authority to act (Base Core §2.1), invokes Evolution E-009 and E-018, and chooses narrowing (C). Its genealogy claims that E-009/E-018 *caused* the separate §2.1 principle; chronology and provenance need source verification rather than accepting thematic association as historical causation. Its hypothesized consequence (“exhaustive evaluation before every action is necessary and sufficient”) is stronger than the actual Core principle; this may be a strawman practical implication.
- **H3 control:** No verified frozen C answer for H3 has been located. Do not assign H3 correspondence percentages.

## Methodology and limits

The available H1/H2 C control explicitly states it was drafted after preliminary exposure to other model answers. It is therefore **not a blinded, pre-registered historical benchmark**. Use it only as a labelled post-hoc reference. The final report requires independent scoring of H1/H2 and source-checked 0–16 H3 rubric; no percentages are invented here. EXP-2 has R0 plus seven full source documents; comparison with EXP-1 is not a single-factor causal test.
