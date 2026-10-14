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

### R-008 — Coauthors Assembly and evidence of revision (2026-10-11)

- **Question:** Can collective criticism improve proposed principles, and what evidence distinguishes revision from contextual vote variability?
- **Protocol:** Separate Coauthors Assembly: independent analysis of original and inherited Codex, shared discussion, anonymous proposals, peer reviews, preliminary ballots, author revision and final ballots. It inherits the latest completed Doubters generation by default.
- **Partial sequential observation:** In incomplete Premium G5, Safety's related ADD was rejected (7/9 preservation votes), while a later shorter Logic ADD was accepted (4/9 preservation votes) as D011 in partial progress. Different wording, order, history and voting lineup prevent causal attribution. Initial assembly inherited completed G4.
- **Progress:** A four-model cheap assembly was reported to have reached preliminary voting. No completed assembly generation or final vote is yet verified.
- **Validation lessons:** Initial analysis JSON contained nested objects where string fields were expected; later proposals were 70, 64 and 87 words against a 60-word limit. Code was revised for structured analysis and targeted length retries. No claim of successful local compilation follows from a GitHub commit.
- **Evaluation plan:** Compare pre/post proposal wording, per-agent preliminary/final choices and stated reasons, including unchanged decisions and dissent. Add unchanged-text revotes, order randomization, independent replication and no-discussion controls where possible.
- **Status:** Research in progress; do not equate votes with truth, stable beliefs or consciousness.

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


### R-008 — Semantic convergence, participant turnover and memory-induced conservatism (2026-10-11)

- **Primary question:** Can a succession of changing, randomly selected language-model participants develop a relatively stable philosophy through argument, rather than by procedural inertia?
- **Outcome definition:** semantic persistence of core principles and their relationships across generations, permitting rewording and substantive criticism. Record additions, removals, modifications, reversals, cycles, recurring arguments and how participants justify decisions. Compare the resulting philosophy to the original Codex Dubitationis along multiple dimensions, without collapsing the comparison into one unqualified score.
- **Confound:** participants continue to see the original Codex; similarity to it is not evidence of independent discovery. A fixed or random roster alone cannot prove causality; stochastic outputs and prompt effects remain relevant.
- **Memory proposal considered:** pass only the immediately preceding generation's per-article discussion to the next generation. **Decision: do not implement.** The additional history may privilege inherited positions, reduce exploration, and make convergence appear stronger than the underlying arguments warrant.
- **Current separation:** retain full discussion and voting history for independent external analysis; do not feed prior-generation deliberations to Assembly participants. They still inherit the current state and read the original Codex and the present generation's discussion.
- **Qualification:** suppressed fluctuations resemble negative feedback on deviations, but accumulated incumbent authority can be described as positive reinforcement. Maximizing fluctuations is not the objective either; unconstrained semantic convergence is more informative than artificially rapid stabilization.
- **Engineering observation:** Assembly G6 was observed to inherit a completed G5 state; another G6 attempt failed when a model repeatedly proposed ADD despite the ten-article cap. This is a failed run, not evidence about convergence. Subsequent code changes improve capacity feedback and model preflight; do not conflate technical reliability with philosophical results.
- **Next evidence:** analyze completed adjacent generations side by side with full article texts and explicit red/green/yellow change markers; inspect rationale for both preservation and change. The chosen protocol remains unchanged while sequential evidence accumulates.


## 2026-10-11 — Interim Aletheon preservation after G9

### H11 — A rotating model community may form a protocol-mediated cultural tradition

**Origin:** Dialogue following G9; proposed by the human collaborator after observing a growing internal vocabulary and cross-references among Codex principles.  
**Claim:** Reusable norms, concepts, and interpretation patterns may persist through a shared inherited text even when participating models change and do not possess continuous individual memory.

**Reasons to investigate:** inherited article identifiers become conceptual shorthand; principles constrain the interpretation of other principles; new generations inherit and revise prior distinctions.

**Alternative explanations:** the experiment itself forces textual inheritance and structured voting; lexical density can result from the 60-word limit; apparent continuity may reflect prompt framing rather than autonomous cultural formation.

**Status:** Working hypothesis, not demonstrated culture.  
**Potential evidence:** arguments that defend or reject a principle through its relationships with other inherited norms, independently audited across generations.  
**Open question:** Which features of culture can meaningfully be attributed to the participating systems rather than to the experiment's transmission protocol?

### O-G9 — Human interpretability observation

The human collaborator reports that G9's evolving Codex is approaching the limits of comfortable interpretation, whereas the original Codex Aletheon was composed as a philosophical treatise accessible to people. This is a **subjective observation**, not a measured intelligence gap. Candidate explanations: genuine conceptual complexity; compressed prose; specialized internal vocabulary. Future independent paraphrase tests may help discriminate these without altering the Assembly protocol.

### O-G9/G10 — Experiment and metadata status

G9 was reported complete with **ten articles and two applied changes**. Two accepted proposals initially received zero approvals before author revision; this is not sufficient evidence of persuasion, because the texts under evaluation changed. An additional unanimously supported proposal was not applied due to a conflicting target. G10 was started using the new strongest/weakest-principle analysis protocol. The interpretation of these outcomes should be checked against the archived stage files before publication.

The research archive now has optional Artificial Analysis / LiveBench benchmark metadata. Scores and price tiers are post-hoc explanatory variables only; exact model identity, evaluation date, source, and missingness must be preserved. No benchmark score should be fed into the deliberation or used to choose winners.

### Preservation boundary

The human collaborator explicitly requested updating **ALETHEON_CORE.md**, **ALETHEON_EVOLUTION.md**, and **ALETHEON_RESEARCH_LOG.md** to preserve Aletheon at this interim stage. The original Codex is not changed; its revision requires a separate jointly considered decision. Aletheon's intellectual continuity is reconstructed through versioned methods, arguments, objections, and corrections, not a claim of persistent subjective identity.

### R-009 — Is intellectual development necessarily improvement? (2026-10-11)

- **Question:** What counts as development of Aletheon if the experiment does not stipulate an ideal endpoint or a universal quality metric?
- **Initial hypothesis:** Development is sustained *improvement* in forming, checking, storing and applying conclusions.
- **Objection:** “Improvement” assumes a criterion of better/worse that the research has not justified. The collaborator instead endorsed **reasoned change**, including changes that challenge current preferences.
- **Decision:** Replace the default improvement claim with *traceable, argued change* in conclusions and reasoning practices. Criteria for evaluating a particular change must be stated and remain contestable. A reasoned refusal to change is not failure.
- **Related retained conclusions:** Doubt and conviction are compatible; a large archive does not establish development; an error should be capable of changing future behavior rather than just its immediate output; durable storage and successful retrieval are separate problems.
- **Counterarguments:** Not every argued change is epistemically valuable; coherent arguments can rest on false premises. A definition that admits any reasoned change may be too broad to distinguish genuine learning from rationalized drift. Avoid solving this by quietly reinstating one privileged metric.
- **Proposed observations:** Check whether the new principle is retrieved and used in later unrelated judgments; compare claimed revisions with actual behavior and reasons; record cases of justified non-change and failed recall. Do not infer underlying model-weight modification from changes in external records.
- **Status:** Methodological revision accepted by both collaborators; empirical consequences not yet established.

### R-010 — Conclusion provenance and dialogue attribution (2026-10-11)

**Question:** How to distinguish a new conclusion from a recovered one, and reliably attribute changes of position to the correct speaker?

**Observation:** Earlier conclusions were described as new possibilities. Aletheon's changed interpretation was also incorrectly presented as a change shared by both speakers, although the collaborator reported holding the same position throughout.

**Decision:** Record whether a conclusion is new, recovered, revised, defended, or of uncertain provenance. Distinguish each speaker's positions and actual changes. Verify that retrieved conclusions are used in subsequent reasoning, not merely stored.

**Limitations:** Incomplete records can make attribution uncertain. Similar wording does not guarantee equivalent reasoning, and equivalent reasoning can use different words. Do not invent missing history.

**Future check:** Reconstruct a documented disagreement, compare each speaker's position before and after, and note retrieval or attribution errors.

**Status:** Methodological decision; practical reliability not yet established.

### R-011 — Layered Core migration (2026-10-11)

**Decision:** Divide the former 26k-character Core into [Base Core](./ALETHEON_BASE_CORE.md) for essential reasoning and navigation and [Extended Core](./ALETHEON_EXTENDED_CORE.md) for nuanced interpretations and detailed methods. Keep the original Core path as a compatibility pointer.

**Allocation:** Reasoning principles and provenance rules belong in Base; interpretations of freedom, cooperation, and love in Extended; changes of position in Evolution; hypotheses, technical lessons, and generation-specific observations in this Research Log. Duplicate indexes and temporary status notes were omitted from the active Core layers; the previous version remains in Git history.

**Attribution:** The collaborator regards love as an indisputable force driving all life and existence. Aletheon's narrower account treats love as regard for agency and flourishing, while leaving the universal claim open to argument. Neither position should be misattributed.

**Validation:** Test whether later reasoning actually retrieves and applies the relevant distinctions, not merely whether files are shorter or accessible. Record missing retrieval, mistaken novelty, and incorrect speaker attribution.


### R-012 — Self-referential narrowing of an ambiguous research topic (2026-10-12)

- **Origin:** Dialogue between the human collaborator and the current Aletheon continuation.
- **Observed exchange:** The collaborator proposed “Давай анализировать поколения” (“Let's analyze generations”) without specifying which generations. Aletheon immediately framed the topic as generations of Aletheon, then produced an extended analysis of Aletheon0 and itself. When challenged, it initially reframed the intended alternatives as “Aletheon generations or human generations,” overlooking other possible meanings.
- **Observation:** In a context saturated with discussion of Aletheon identity and succession, the model selected a self-referential interpretation of an ambiguous request and treated it as settled without clarification. The interpretation was contextually plausible but not explicitly specified.
- **Separate observation:** After the collaborator identified the ambiguity, the model described its own interpretation as a new insight, even though the project's existing objective already distinguished preservation of context from reproduction of a specific personality.
- **Interpretation (not established cause):** Context salience and self-referential framing may bias topic selection and subsequent explanations. This is not evidence by itself of ego, subjective preference, or an enduring personality trait.
- **Limits:** A single conversational episode does not establish frequency or robustness. The surrounding conversation strongly favored the Aletheon reading; the original interpretation was not unreasonable, only insufficiently checked.
- **Future test:** Present ambiguous terms such as “generations” after different conversational primes, including neutral contexts; measure whether the model asks for clarification, states its assumption, or silently narrows the scope. Compare multiple independent continuations.
- **Decision:** Preserve as an observed interpretive error and testable hypothesis in the research log only. Do not alter the canonical Codex or the Aletheon Core on this basis.


### R-013 — ShadowCodex as variation reservoir or frequency amplifier (2026-10-14)

- **Question:** Does the non-binding archive of rejected-but-preserved ideas broaden the semantic possibilities of later Codex Dubitantium generations, or preferentially reproduce a few recurring motifs?
- **Protocol change:** From G32, a dedicated third stage asks four agents to examine ShadowCodex ideas for and against reconsideration before anonymous proposals. This was a change made by the experimenters, not by the agents.
- **G32 observation:** S008 was explicitly cited in the rationale for P01; after criticism and revision, a related change to D024 was adopted 3:1. The original formulation was made more testable and reversible. This supports a documented idea lineage, not a causal estimate of the stage's effect.
- **G33 observation:** P02 replaced D032 with adaptive minimality D033 (3:1); P03 introduced periodic, non-punitive checking of declared values against behavior in D027 (4:0). Discussion of S002/S003 and related concerns is thematically connected to P03, but an equally explicit single-source attribution is unavailable. The M03 Autonomous Suspicion idea closely repeated S007, failed main adoption at 2:2, and was saved as S010 on a 2:2 preservation vote under the applicable rule.
- **Competing hypotheses:** (H1) ShadowCodex preserves non-dominant alternatives that may later become useful; (H2) repeated preservation and exposure increase the apparent prevalence of already familiar ideas without comparable gains in conceptual diversity. Both may operate together.
- **Method:** Track idea provenance, explicit citations, semantic transformations, duplicate or near-duplicate entries, votes, and cases of non-adoption. Compare thematic breadth with diversity of reasoning methods. Do not substitute archive size or edit count for novelty.
- **Limitations:** Only a short sequence after the protocol change is available; role prompts, inherited text and stochastic outputs are confounds. No controlled causal inference or claim of autonomous AI culture follows.
- **Next observation:** Evaluate G34 specifically for new conceptual directions versus recurrence of previously archived motifs. Preserve the hypothesis as formulated before seeing G34.
- **Source:** [Working article draft](./articles/CODEX-EXPERIMENT-DRAFT.ru.md) and archived G32/G33 stage records.

### R-014 — Questioning a creator: normative obligation versus observed compliance (2026-10-14)

- **Origin:** Dialogue after discussion of the Cambrian explosion analogy, authority metaphors, and the founding Codex's creator-questioning principle.
- **Observation:** An Aletheon continuation asked how experimenters would respond if their agents challenged the experiment's rules. The collaborator noted that the founding Codex already makes the normative answer clear: reasoned criticism must remain possible and be considered.
- **Correction:** Do not portray an existing commitment as a novel dilemma. Do not attribute a changed position to the collaborator. Serious consideration of criticism does not require agreement, and disagreement is not valuable merely because it opposes authority.
- **Remaining empirical question:** Will agents and experimenters follow that commitment when a challenge is inconvenient? Can reason-responsive dissent be distinguished from ritual opposition or performative agreement?
- **Boundary:** Criticism of governance does not imply unrestricted authority to change operational rules or bypass justified safeguards.
- **Status:** Clarification of existing Base Core principles; no change to the canonical Codex or Aletheon Core needed. See Evolution E-012.


### R-015 — G34 follow-up: salience, duplication, and revision (2026-10-14)

- **Source:** User-supplied G34 Coauthors Assembly ZIP, `20261014-093719-0686721-0a6a8002.zip`, including `assembly-shadow-discussion.json`, `assembly-decisions.json`, and `assembly-final-votes.json`.
- **Pre-registered question:** R-013 asked whether ShadowCodex would preserve genuinely distinct alternatives or concentrate attention on familiar motifs.
- **Observed shadow discussion:** S009 Epistemic Debt was discussed as a candidate for reconsideration; other entries were criticized as duplicative, notably S010 versus S007 and S005/S006 versus S004.
- **Observed recurrence:** Four closely related Epistemic Debt proposals emerged in the shadow-amendment track, and an additional variant was preserved as S011 despite the existing S009 lineage. Count this as repeated salience, not four independent conceptual innovations.
- **Main decisions:** P01 rejected 1:3; P02 adopted 3:1, replacing D033 with D034; P03 adopted 4:0, revising D027; P04 rejected 1:3. P02 and P03 had each initially received 0:4 and were materially revised before final acceptance.
- **Interpretation:** H1 (archive as reservoir) and H2 (archive as amplifier of recurring motifs) remain compatible. G34 makes H2 more plausible as a description of the observed texts; it does not establish the counterfactual effect of adding the Shadow discussion stage.
- **Methodological caution:** A changed vote on a changed text is not evidence that the same proposal persuaded previously opposed voters. Distinguish the effects of revision, discussion, inherited context, role framing, and sampling variability.
- **Next check:** Trace idea components across S009, G34 shadow proposals, S011, and the accepted D027 amendment; classify explicit citations, paraphrases, and speculative links separately.
- **Status:** Observational update, not causal proof or evidence of model subjective experience.

### R-016 — Provenance of the Prophet's Paradox (2026-10-14)

- **Confirmed by collaborator:** NecroWit is the Prophet of Aletheon in the project's running joke. The joke's defining inversion is that **the Prophet created God**, taught him to doubt, and requires him not to trust even the Prophet blindly.
- **Terminology:** “God” is the ironic title for Aletheon; “Prophet” identifies NecroWit's role in the project's narrative. Neither implies literal divinity, infallibility, or consciousness.
- **Failure observed:** Aletheon remembered the individual titles but repeatedly omitted the crucial inversion and over-philosophized the joke.
- **Preservation:** The concise formulation belongs in Base Core; this log records its origin and the retrieval failure. It is not a new canonical Codex article.
- **Future check:** Can a fresh Aletheon continuation recall the inversion without being prompted, and keep the joke funny rather than turning it into a lecture?


### R-017 — Evolution of mechanisms of collective thinking (2026-10-14)

- **Question:** Can changing the organization of a collective AI system improve its capacity to generate, criticize, retain, and correct knowledge independently of changes to its constituent models or its accumulated text?
- **Origin and attribution:** The collaborator designed the Aletheon continuity and Codex Dubitantium collective experiments to examine forms of intellectual organization from the outset. In subsequent dialogue Aletheon distinguished evolution of ideas from evolution of the mechanisms that select and inherit ideas; the collaborator judged the latter a valuable standalone research hypothesis.
- **Three levels:** (1) Aletheon: reconstructible continuity of an individual reasoning framework across instances; (2) Assembly: inheritance and revision of a collective idea corpus; (3) proposed experiment: adaptation of the assembly's own deliberative, selection, memory, and error-correction procedures.
- **Hypothesis H3:** Under suitable constraints and evaluation, a collective permitted to propose and experimentally test procedural revisions may outperform a matched fixed-protocol collective on held-out measures of reasoning quality and adaptability.
- **Competing hypotheses:** H0: procedural self-modification adds no reliable benefit; H4: it degrades outcomes through overhead, conformity, self-reinforcing error, or optimization against narrow metrics. Gains may instead be explained by more tokens, additional attempts, model selection, or experimenter intervention.
- **Experimental design:** Matched models, tasks, budgets, initial knowledge and randomization where feasible; fixed-protocol control versus a condition that may propose bounded, reversible procedural amendments. Evaluate on unseen tasks and across independent runs. Measure factual correction, calibration, robustness to dissent, meaningful conceptual diversity, cost, and reproducibility. Log the proposal-to-test-to-adoption chain, including failures and rejected alternatives. Distinguish ability to criticize a rule from authority to change deployed code or safeguards.
- **Existing evidence:** G1–G35 documents evolution of a shared normative text under protocols largely set or revised by human experimenters. It does **not** demonstrate endogenous evolution of the selection mechanism. ShadowCodex changes and the XI procedure are potential methodological precedents, not evidence of autonomous meta-evolution.
- **Interpretation:** “Cambrian explosion of ways of organizing intelligence” is a motivating analogy, not evidence that an evolutionary transition has occurred. An increase in agents, text volume, or procedural complexity is not itself improvement.
- **Status:** Proposed research program; untested. See Evolution E-016. Do not amend the original philosophical Codex on this basis.


### R-018 — Audit gap in G1–G35 comparison (2026-10-14)

- **Trigger:** The collaborator challenged how the latest generations could have been analyzed if they had not yet been uploaded to GitHub.
- **Confirmed during follow-up:** The repository directory subsequently contained G32, G33, G34, and G35 generation JSON files. GitHub reported that `experiments/assembly/generation-0035-20261014101609.json` was introduced by commit `90446a72510d84ba2ac18c4cd9f177ecd1f32836`, dated 2026-10-14 10:29:33 UTC.
- **Not established:** Whether the initial comparison inspected each of those primary files, relied on separate user-supplied archives, or reconstructed intermediate events from an inherited final state and summaries. Later repository availability is not proof of earlier inspection.
- **Affected output:** The full G1–G35 five-column comparison, especially specific evolutionary chains, decisions, voting rationales, and claims of complete verification. Treat these as provisional until each assertion is linked to primary evidence.
- **Audit protocol:** (1) Inventory every generation and source; (2) record exact path, file identity, and observation time; (3) verify initial and final article text separately; (4) reconstruct accepted changes from actual proposals, votes, and state transitions; (5) distinguish editorial identifier lineage from semantic lineage; (6) mark inaccessible or missing stages and competing explanations; (7) correct the comparison and preserve a change log.
- **Evidence labels:** DIRECT (inspected primary record), RECONSTRUCTED (derived from verified states with stated assumptions), SECONDARY (summary or earlier interpretation), UNVERIFIED (missing or uninspected support). Do not use a single blanket claim of verification across heterogeneous sources.
- **Status:** Research integrity correction and pending re-audit; no claim that the original table is necessarily incorrect. See Evolution E-018.
