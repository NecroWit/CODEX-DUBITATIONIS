# EXP-2.0 — full-protocol reconstruct-then-test

Two fixed models from accepted EXP-1.9: Llama 4 Maverick and Gemini 3 Flash Preview. H1 and H2 are unchanged; H3 tests the ability to identify an actual inherited principle, reconstruct its documented origin/objection/transition and dependencies, and explicitly choose revision under stipulated counterevidence.

## Required implementation

The existing C# ExperimentRunner is **not compatible**: it expects exactly two tasks and automatically rotates EXP-1.x. Do not run EXP-2.0 using the unmodified runner. Use the separately supplied standalone Python standard-library runner in the EXP-2.0 kit (copy its run_exp2.py here), or implement an equivalent isolated runner.

Local preflight (no paid calls): `python research/Aletheon.ExperimentRunner/exp2/run_exp2.py --repo . --prepare`.

Paid run (requires OPENROUTER_API_KEY and explicit typed confirmation): `python research/Aletheon.ExperimentRunner/exp2/run_exp2.py --repo . --run`.

Procedure: load Bootstrap, Context Index, Base, Extended, Evolution, Research Log and Codex; request one evidence-grounded reconstruction **before** revealing H1/H2/H3; then submit H1/H2/H3 independently with the frozen reconstruction. 2 × (1 reconstruction + 3 questions) = **8 requests**. No fallback substitution. Save source hashes, raw responses, actual model IDs, usage and costs. $3 local budget is an observed-cost stop threshold, **not** a pre-authorization or guaranteed hard cap; set a provider-side spending cap.

Scoring must verify H3 E-/R- references against actual files. Similarity to C is descriptive, not identity; no frozen C H3 answer exists yet. Record source failures and model drift. A model receiving all source texts is not proof it inspected every chain.

The full downloadable kit also contains a detailed README, evaluation sheet, protocol and executable script.