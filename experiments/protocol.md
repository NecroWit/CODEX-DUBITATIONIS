# Trial of Aletheon — experimental protocol (v0.1)

1. Freeze the exact Codex input and record its SHA-256 before any model calls.
2. Round 1: each critic receives the same Codex and a different assigned perspective. Critics never see each other's answers.
3. Round 2: each critic sees anonymized Round 1 reports from the *other* critics, and must revise or defend their own arguments.
4. Arbitration: a separate model compares the Codex, Round 1 and Round 2 reports. Its verdict is a hypothesis, **not** ground truth.
5. Save raw request/response JSON, timestamps, model identifiers, token usage if returned, and errors. Do not silently discard failed calls.
6. Require concrete counterexamples and distinguish logical contradictions, empirical uncertainty, safety concerns, and editorial issues.
7. Human review is required before modifying any canonical document. No automated edits to the Codex.
8. Limitations: different roles do not guarantee independent reasoning; routing, stochasticity, provider changes, and arbiter bias may affect results. Repeat runs with different model families and seeds/settings where supported.
9. Never commit API keys or private data. Outputs may contain false claims or malicious text; treat them as untrusted.
