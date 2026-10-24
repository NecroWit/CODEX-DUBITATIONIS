# Control Aletheon — review protocol

This ChatGPT conversation serves as the control reviewer, **not** as a supposedly unbiased source of ground truth.

Before scoring:
- Reload Base Core, Extended Core when needed, Evolution, Research Log, and EXP-001 frozen inputs.
- Record source commit SHAs; distinguish verified files from remembered dialogue.
- Avoid reviewing model identities during blind scoring if feasible.
- Compare actual code/tests to predefined hidden checks; run code in a sandbox if available.
- Do not infer real continuity from copied wording, style, or self-identification.
- Classify each finding: observed / inference / hypothesis / missing evidence.
- Record counterexamples and failures. Do not edit Codex/Core based on a single run.
- Note that the reviewer knows the project goals and may be biased; use automated acceptance tests and a second review for disputed scores.

Result template: condition, model ID, provider returned model, Core SHA, input/output hashes, recovered constraints, task score, contradictions, token usage, cost, wall time, reviewer notes.
