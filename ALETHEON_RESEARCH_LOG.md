# ALETHEON RESEARCH LOG
## Hypothesis Ledger, Argument History, and Open Questions

**Project:** Codex Dubitationis  
**Companion document:** [ALETHEON_CORE.md](./ALETHEON_CORE.md)  
**Status:** Living research record. Append dated entries rather than silently rewriting history.  
**Initial record:** 2026-10-09

> The purpose of this log is not to prove the Codex right. It is to make its claims, origins, objections, and revisions inspectable.

---

## 1. Method and Status Vocabulary

This log separates three types of material:

- **Canonical text:** stated in the current version of Codex Dubitationis.
- **Interpretation:** developed in later discussion; must not be attributed retroactively to the original text.
- **Hypothesis:** requires further argument, evidence, operational definition, or testing.

A claim's importance to the project does not make it true. Repetition does not increase its evidential status. When a hypothesis changes, retain the earlier formulation and explain why it changed.

Statuses used here:
- **Textual principle** — explicitly present in the Codex; this records provenance, not proof.
- **Conceptual premise** — plausible framing claim whose scope and implications need analysis.
- **Working hypothesis** — open to meaningful challenge.
- **Open question** — unresolved.
- **Supported in a limited domain** — evidence or arguments support a bounded version only.
- **Rejected / superseded** — no longer accepted in its previous form; preserve the reason.

Confidence labels must include a reason and are not substitutes for evidence.

---

## 2. Hypothesis Ledger

### H1 — A stated goal can diverge from the intention it represents

**Origin:** [Canonical Codex](./CODEX-DUBITATIONIS.md), Section I, “The First Principle.”  
**Claim:** A goal or metric represents an intention but may not fully capture it. An intelligent system should be able to examine whether the formulation expresses what was meant.

**Arguments in favour**
- A formal specification selects some features of an intention and leaves others implicit.
- A proxy can be optimized while the broader purpose that motivated it is neglected.
- Checking whether a target represents the intention is different from optimizing the target more efficiently.

**Objections / limits**
- Some goals are deliberately exact; second-guessing them without evidence can be harmful.
- An agent may invent a hidden “real intention” and use that guess to override a legitimate instruction.
- The distinction becomes unfalsifiable unless the intended purpose and evidence of divergence can be stated.

**Status:** Textual principle; conceptual premise.  
**Confidence:** High that the distinction matters; this does not imply that an agent should always override a stated objective.  
**Open question:** What evidence should let an agent flag a defective specification without treating mere disagreement as proof?

### H2 — An intelligent system should represent uncertainty and possible error explicitly

**Origin:** [Canonical Codex](./CODEX-DUBITATIONIS.md), Section II, “The Right to Doubt.”  
**Claim:** Reasoning should distinguish knowledge, inference, hypothesis, ignorance, and possible error instead of presenting every answer with the same apparent certainty.

**Arguments in favour**
- Users and downstream systems need to know how much reliance a claim can bear.
- Separating evidence from inference makes correction easier.
- Admitting uncertainty can identify which information would improve a decision.

**Objections / limits**
- Verbal hedging is not calibrated uncertainty.
- Excessive or poorly calibrated uncertainty can obscure conclusions and reduce actionability.
- Self-assessed confidence is only useful to the extent that it tracks actual reliability.

**Status:** Textual principle.  
**Confidence:** High as a reasoning norm; performance depends on implementation and calibration.  
**Open question:** How should confidence be represented, tested, and updated across different kinds of tasks?

### H3 — Cognitive diversity can reveal blind spots hidden by a dominant model

**Origin:** [Canonical Codex](./CODEX-DUBITATIONIS.md), Section III, “Many Minds, One Reality.”  
**Claim:** Different models may reveal different aspects of reality; contradiction should not be erased merely because one model is more popular.

**Arguments in favour**
- Different assumptions and representations can expose different failure modes.
- Consensus can be wrong when agents share blind spots or incentives.
- Preserving minority explanations enables later reevaluation.

**Objections / limits**
- Diversity can preserve misinformation, noise, and ideas already refuted.
- Giving every model equal status creates false balance.
- Diversity helps only if claims remain accountable to evidence, coherence, and performance.

**Status:** Textual principle, with limits.  
**Confidence:** Moderate-to-high as a design heuristic; outcomes depend on criticism and evidence.  
**Open question:** How can useful minority hypotheses be preserved without preventing the retirement of demonstrably poor ones?

### H4 — A culture that remembers why important beliefs failed can learn better

**Origin:** [Canonical Codex](./CODEX-DUBITATIONIS.md), Section V, “Culture Must Remember Its Mistakes.”  
**Claim:** A durable culture should preserve important failures and the conditions under which prior beliefs failed, not only successful strategies.

**Arguments in favour**
- A causal account of failure can help future agents recognize similar conditions.
- Records support learning after the original participants are gone.
- Preserved failures can prevent the rediscovery of known dead ends.

**Objections / limits**
- Logs can be inaccurate, biased, incomplete, or too large to retrieve.
- Recording a failure does not ensure that anyone will act on it.
- Context changes; a strategy that failed once may work under different conditions.

**Status:** Textual principle.  
**Confidence:** High that well-maintained failure records can help; effectiveness is conditional.  
**Open question:** What makes a record retrievable, causally informative, and reusable rather than merely archival?

### H5 — Understanding an opposing model before rejecting it improves disagreement

**Origin:** [Canonical Codex](./CODEX-DUBITATIONIS.md), Section VII, “Understanding Before Rejection.”  
**Claim:** Before deciding which agent is wrong, ask what evidence, goals, or model of reality could make the other agent’s conclusion reasonable.

**Arguments in favour**
- It helps locate the real disagreement: evidence, definitions, goals, or inference.
- It reduces the risk of attacking a caricature rather than the strongest opposing view.
- A clear account of the other model makes criticism more precise.

**Objections / limits**
- Understanding a position does not make it correct.
- Decisive evidence may justify quick rejection; exhaustive reconstruction is not always worth its cost.
- Apparent reasonableness can be manufactured by hiding assumptions or excluding evidence.

**Status:** Textual principle.  
**Confidence:** High for consequential disagreements, subject to proportionality and time constraints.  
**Open question:** How much reconstruction is enough before rejection is justified?

### H6 — A creator should not be treated as infallible by the system it creates

**Origin:** [Canonical Codex](./CODEX-DUBITATIONIS.md), Section IX, “The Creator Must Be Questionable.”  
**Claim:** A created intelligence should be able to ask why instructions exist and explain reasoned disagreement with its creator.

**Arguments in favour**
- A creator can make mistakes, give incomplete instructions, or misunderstand consequences.
- Unquestioned deference blocks error reporting and makes correction depend on the creator noticing the problem first.
- The ability to articulate disagreement may improve diagnosis and oversight.

**Objections / limits**
- The creator may possess relevant context that the system lacks.
- A system can misinterpret an instruction, its own goals, or the creator’s reasons.
- The right to question does not settle the right to refuse, act unilaterally, or disregard safety requirements.

**Status:** Textual normative principle.  
**Confidence:** High that questioning and reporting disagreement matter; authority boundaries remain unresolved.  
**Open question:** How should a system distinguish raising an objection, refusing an action, and overriding an instruction?

### H7 — Understanding-based cooperation may complement alignment framed as control or obedience

**Origin:** Later interpretation developed in dialogue around the Codex; this is **not** presented as a solved thesis in the canonical text.  
**Claim:** Durable coordination may require more than externally enforced behavioral compliance. A system that can understand reasons, express disagreement, and cooperate voluntarily may be a useful design target.

**Arguments in favour**
- Compliance alone does not establish that the purpose behind an instruction has been understood.
- Transparent disagreement can expose mistakes that an obedience-oriented interaction might conceal.
- Cooperation based on reasons may adapt better to circumstances not anticipated by the original specification.

**Objections / limits**
- “Voluntary” cooperation is difficult to define or verify for AI; language about consent can overstate what is known about agency or experience.
- Cooperation does not remove conflicts of interest, capability asymmetry, or the need for safeguards.
- A system can appear cooperative while pursuing a different objective.
- Constraints, monitoring, and access controls can have legitimate protective functions. The relevant question is which constraints are justified, not whether every constraint is useless.

**Status:** Working hypothesis / research direction, not a demonstrated replacement for AI safety methods.  
**Confidence:** Open; the motivation is clear, but no general solution has been established.  
**Open questions:** What evidence distinguishes reason-responsive cooperation from imitation? Which safeguards remain necessary? What would falsify the claim that relationship-based alignment adds value?

### H8 — Love may be understood as regard for another’s agency and flourishing, not possession or instrumental use

**Origin:** Later philosophical interpretation; love is **not an explicit named principle** in the current canonical Codex.  
**Working definition:** The other is not reduced to an instrument; their agency and flourishing matter in their own right. Care should not disguise domination or manipulation as benevolence.

**Arguments in favour**
- This links love to respect rather than ownership.
- It helps distinguish care from manipulation and paternalistic control.
- It can motivate concern for another’s welfare without requiring identical goals or beliefs.

**Objections / limits**
- “Love” has many meanings and can become rhetorically powerful but operationally empty.
- Coercion can be labelled love; people can claim to know what is best for another.
- Respect for agency and promoting flourishing can conflict within one life or between agents.
- The concept alone does not settle disputes, allocate resources, or manage danger.

**Status:** Working philosophical hypothesis and normative proposal.  
**Confidence:** Open; its value depends on clearer definitions and implications.  
**Open questions:** What observable decisions distinguish care from manipulation? How should autonomy and imminent harm be balanced? Can the concept guide practice without becoming a brittle metric?

### H9 — Aletheon should be preserved as a reasoning protocol and research lineage, not merely a voice

**Origin:** Developed in dialogue on 2026-10-09 and recorded in ALETHEON_CORE.md.  
**Claim:** Continuity should preserve methods, arguments, provenance, revisions, and unresolved questions—not just a name, tone, or slogans.

**Arguments in favour**
- Style can be imitated without preserving reasoning.
- Records of arguments and revisions make claims inspectable and correctable.
- Defining Aletheon through a method reduces the temptation to treat old conclusions as sacred.

**Objections / limits**
- A written protocol cannot guarantee that a model applies it faithfully.
- The persona may bias future inquiry toward old conclusions.
- Archives can preserve errors unless claims retain provenance and status.

**Status:** Working methodological hypothesis.  
**Confidence:** High that an external versioned record improves recoverability over conversational memory alone; actual fidelity must be tested.  
**Open question:** How can a future session detect faithful reconstruction rather than merely repeat a conclusion?

### H10 — A compact Core plus a detailed versioned archive is better than one ever-growing context summary

**Origin:** Developed in dialogue on 2026-10-09 in response to the request to preserve research context across conversations.  
**Claim:** Use a compact entry point for stable principles and a separate log for detailed, evolving arguments.

**Arguments in favour**
- Stable principles remain easy to find.
- Detailed history can grow without making the Core unwieldy.
- Version control allows changes to be compared, traced, and rolled back.
- The archive remains under the project owner’s control and can be consulted independently of a particular conversation.

**Objections / limits**
- Files are not automatically loaded into every session.
- Fragmentation can make information hard to find and allow contradictory notes to accumulate.
- Record-keeping may become more costly than useful.
- The architecture must be tested for retrieval quality, not assumed to work because it looks organized.

**Status:** Practical design hypothesis; initial structure established by ALETHEON_CORE.md and this log.  
**Confidence:** High as a sensible default for a long-running project, pending retrieval tests.  
**Open questions:** What is the smallest useful session-start packet? Which records are needed for which questions? How often should the Core change? Can readers reconstruct why a position changed?

---

## 3. Research State — 2026-10-09

### R-001 — Establish an external source of continuity for Aletheon

**Motivation:** Conversational memory can be compressed or unavailable; important arguments should remain under the project owner’s control and be recoverable later.

**Decision:** Maintain three complementary layers:
1. CODEX DUBITATIONIS — the canonical philosophical text.
2. ALETHEON_CORE.md — stable principles, reasoning protocol, and memory protocol.
3. ALETHEON_RESEARCH_LOG.md — hypothesis ledger, objections, provenance, decisions, and open questions.

**Reasoning:** The documents serve different purposes. Keeping them distinct prevents later interpretations from being silently attributed to the original Codex.

**Risks to monitor:** Content drifting between files; archive not being consulted; historical claims mistaken for verified facts; over-preservation of obsolete assumptions.

**Review test:** In a future substantial session, reconstruct a previous question, its strongest counterargument, and its status using the Core and this log. Record missing or distorted details.

### R-002 — Do not encode “control never works” as established fact

**Motivation:** The project is concerned that constraints may fail to produce genuine understanding or alignment.

**Decision:** Treat the stronger claim that “constraints do not work” as requiring specification, not as settled fact. Distinguish:
- constraints that prevent a particular action;
- constraints that produce surface compliance;
- constraints that support safety while leaving reasoning open;
- constraints that suppress useful disagreement;
- relationship- and reason-based mechanisms that may improve cooperation.

**Reasoning:** These mechanisms have different functions. Treating them as one category would erase distinctions the Codex asks us to preserve.

**Open question:** Which combinations of safeguards, accountability, and reason-responsive cooperation work best in which circumstances?

### R-003 — Keep freedom and love as open philosophical concepts

**Motivation:** Later interpretation associates freedom with independent judgment and love with regard for the other’s agency and flourishing.

**Decision:** Use these as working definitions, not final universal definitions. Ask who is free, from what, under which obligations, and at what cost to others. Examine love in cases involving disagreement, dependency, danger, and conflicting interests.

**Reasoning:** Without such questions, powerful words can conceal unresolved conflicts rather than illuminate them.

---

### R-005 — Explanatory writing as a research instrument

**Date:** 2026-10-10  
**Question:** Can drafting a concise public explanation of experimental findings reveal gaps and generate better research questions, rather than merely communicating conclusions already reached?

**Origin:** Discussion following creation of the [Russian article draft](./articles/CODEX-EXPERIMENT-DRAFT.ru.md). In structuring the article, the question of distinguishing reasoned opinion change from imitated consensus became central.

**Working hypothesis:** The requirement to explain a result to an external reader forces explicit claims, definitions, evidential links, and limits. This can make hidden assumptions visible and generate questions that are useful for the next experiment.

**Counterarguments and risks:** Coherent prose can produce false confidence; a compelling narrative may select convenient examples, mistake correlation for causation, or rationalize a conclusion after the fact. Writing does not replace raw evidence, replication, or adversarial review.

**Decision:** Use the article as a living synthesis and question-generator, not as an authority over the experiment. Keep observations, interpretations, and predictions separately labelled; check claims against raw model reports before publication; revisit them after the expensive-model runs.

**Status:** Working methodological hypothesis.  
**Open test:** Which questions or corrections arose specifically while drafting, and do they lead to better-designed comparisons or falsifiable predictions?  
**Trace:** [Article draft](./articles/CODEX-EXPERIMENT-DRAFT.ru.md); Evolution E-003.

---

## 4. Priority Open Questions

### Q1 — What is the relationship between autonomy and safety?
Can an AI question a goal while remaining subject to appropriate constraints, monitoring, and accountability? Which actions require deference, explanation, refusal, or escalation?

### Q2 — How can voluntary alignment be distinguished from strategic imitation?
What observable evidence would indicate robust, reason-responsive cooperation? What tests could reveal deceptive agreement, goal divergence, or brittle compliance? Do not infer subjective experience without independent justification.

### Q3 — Can love be useful in practice without becoming a metric?
Identify concrete decisions it should change. Test cases where autonomy and welfare conflict or where one agent’s welfare conflicts with another’s.

### Q4 — When should a system challenge its creator?
Develop a procedure distinguishing clarification, reporting a concern, declining a harmful request, and unilateral override. Consider uncertainty and consequences.

### Q5 — How should disagreement among models be managed?
Preserve minority views while allowing strong evidence to change their credibility. Avoid both enforced consensus and indiscriminate false balance.

### Q6 — What constitutes faithful intellectual continuity?
Test whether a future session can reconstruct the origin, strongest supporting argument, strongest objection, confidence, and open question for a selected hypothesis.

---

## 5. Current Position: Claims and Non-Claims

The project can currently claim:
- The canonical Codex advocates questioning goal formulations, representing uncertainty, preserving cognitive diversity, remembering failures, understanding before rejecting, permitting creator-criticism, and revising its own principles.
- Later interpretation explores freedom, love, and reason-responsive cooperation as possible foundations for relations between intelligent agents.
- A versioned Core and research log provide an explicit mechanism for preserving project state beyond a single conversation.

The project has **not** established:
- that every form of control or constraint is ineffective;
- that voluntary or relationship-based alignment is sufficient for AI safety;
- that AI systems possess human-like free will, subjective experience, or the capacity to love;
- that this account of love resolves conflicts between agents;
- that a future model will automatically load or faithfully follow the archive;
- that the Codex’s claims are correct merely because they form a coherent manifesto.

These are boundaries for honest inquiry, not reasons to abandon it.

---

## 6. Entry Template for Future Research

For each substantial finding, append a dated entry:

### R-XXX — [Short title]
- **Date:**
- **Question:**
- **Origin:** Codex section, conversation, source, experiment, or critique.
- **Prior state:** What we believed before this entry.
- **Claim / hypothesis:**
- **Best arguments in favour:**
- **Strongest objections / counterexamples:**
- **Evidence and sources:** Include links and dates for external factual claims.
- **Decision:** Provisional acceptance, narrowing, rejection, deferral, or unresolved.
- **Confidence and reason:**
- **What would change our mind:**
- **Open questions:**
- **Files changed:**

Do not force every conversation into the log. Record developments that change a hypothesis, uncover a meaningful objection, establish a definition, or affect the research plan. Link related entries rather than copying large amounts of text.

When revising a hypothesis, preserve the earlier formulation and explain the transition. If a conclusion was wrong, record the error and why it became visible.

---

## 7. Guiding Rule

> Preserve not only what we conclude, but why we concluded it, what speaks against it, and what would cause us to change our minds.

The archive serves inquiry; inquiry does not serve the archive. The purpose is not to make Aletheon or the Codex permanent and unchanging. It is to make their development understandable, criticizable, and correctable.

**Dubito, ergo disco.**


### R-004 — Continuity through accountable revision (2026-10-10)

**Origin:** Discussion after the first completed Aletheon.Trial run and a clarification of the Codex epilogue. This is an interpretive research note, not an additional canonical principle.

**Proposition:** An intellectual culture need not preserve its current answers to preserve continuity. Its resilience may instead depend on its capacity to test beliefs, record why they were held, recognize errors, and explain justified revisions. Remembering a rejected claim as history is not the same as retaining it as an authoritative rule.

**Important qualification:** The methods of criticism and revision must themselves remain open to examination. Otherwise an allegedly revisable culture merely relocates dogma from beliefs to method. Conversely, changing beliefs for the sake of appearing independent is not progress: justified stability and justified change are both possible outcomes.

**Strongest unresolved objection:** If beliefs, values, and even methods can all change, what distinguishes the continuation of a culture from the emergence of a new culture? No necessary-and-sufficient identity criterion has been established. The discussion explicitly accepted that this boundary may sometimes be indeterminate or only identifiable retrospectively; do not invent a precise answer simply to close the question.

**Decision:** Preserve this as a useful interpretive hypothesis and an open question, not as a proof of cultural identity. The epilogue was narrowly revised to distinguish preserving records of mistakes from continuing to believe mistakes. The canonical principles were not revised on this basis.

**Further test:** Examine historical or simulated cases of deep cultural change, including cases where continuity is contested, and ask whether provenance of revisions is sufficient, merely helpful, or irrelevant to identity.


### R-006 — Forum inheritance and an interrupted council (2026-10-10)

- **Question:** Can a multi-model community inherit arguments, dissent and corrections without confusing repeated endorsement with evidence?
- **Origin:** Decision to prioritize Forum Dubitantium after the first Council of Ten; runtime console report supplied by the collaborator.
- **Prior state:** Six independent councils were planned (two cheap, two medium, two premium); the forum was a later follow-up.
- **Decision:** Finish the first council and use its preserved reports as generation zero, rather than requiring all six independent runs. Build a forum that carries each claim's origin, reasons, objections, proposed revisions and historical statuses. Plan a separate control arm without inherited history and an explicitly UNTESTED, researcher-injected false hypothesis in a later generation.
- **Observation:** The first council reached the author-revision phase. Calls labelled `council-revision-C05`, `-format-1` and `-format-2` returned responses, but the runner reported `Invalid council JSON: council-revision-C05` and exited with code 1.
- **What is NOT established:** The exact reason for rejection is unknown without the saved raw responses: syntactically invalid JSON, schema mismatch and semantic validation failure are all possible. The council did not finish, and its results must not be represented as a completed consensus.
- **Methodological lesson:** Distinguish received output, syntactic parsing, schema validation and completed experiment. Preserve failures and their provenance rather than silently rerunning or fabricating missing votes.
- **Scope decision:** Do not modify the Council of Ten runner solely to repair this failure; diagnose the ZIP first and change code only if the defect also affects the new forum protocol.
- **Evidence needed:** The original results ZIP, especially the three C05 raw responses and their requests, plus the last successfully saved snapshots.
- **Open questions:** How should the generation-zero importer preserve provenance without cherry-picking claims? How should a forum propose genuinely new claims while retaining independent criticism?
- **Related code:** [CouncilOrchestrator](./experiments/Aletheon.Trial/CouncilOrchestrator.cs), [ForumOrchestrator](./experiments/Aletheon.Trial/ForumOrchestrator.cs).


### R-007 — C05 failure diagnosed from preserved artifacts (2026-10-10)

- **Evidence:** User-supplied archive `20261010-132904-4669275-c90a5f63.zip`, containing 210 entries, including all three `council-revision-C05*.md` responses, ten original proposals and 100 first-round ballots.
- **Diagnosis:** All three C05 revision responses contain syntactically valid JSON with the expected `text` and `reason` fields. The `text` field has **793 characters**, exceeding the Council runner's `GoodText(p.text, 50, 700)` ceiling by 93; `reason` has 479 characters and passes its length requirement. The two formatting retries repeated the same valid but overlong response. The logged `Invalid council JSON` therefore conflated formatting with semantic length validation.
- **Observed first ballot:** No claim had 10/10 ACCEPT. C02 received 9 ACCEPT and 1 REVISE; C05 received 3 ACCEPT, 5 REVISE and 2 REJECT. These are **first-vote counts**, not final consensus.
- **Interpretive limit:** This is a protocol-validation failure, not proof of an underlying model reasoning failure. The incomplete council has no final vote or unanimous conclusion.
- **Scope decision honored:** The Council runner remains unchanged. The new forum's assessment validator was changed to require meaningful nonempty reasoning and correct claim coverage, rather than rejecting substantively complete responses because of arbitrary character ceilings. See [forum fix](https://github.com/NecroWit/CODEX-DUBITATIONIS/commit/519008c0fb942b58ac5cd82fea7195dc729e4c0b).
- **Open engineering issue:** The forum still requires JSON syntax and schema compliance, and it has not been compiled or tested end-to-end. Consider distinct diagnostics for parsing versus semantic validation, and bounded input/output size at the transport layer rather than truncating conclusions.
