# Data dictionary

This directory currently contains a public compact aggregate only. The complete sanitized per-generation JSON and detailed CSV tables have been generated as a separate research dataset, but have **not yet been committed to GitHub**.

- `generation`: numbered generation G1–G35.
- `accepted`, `rejected`: history entries with Applied true/false (PASS excluded from rejected); G4 includes a forced deletion.
- `accept_to_reject`, `reject_to_accept`: preliminary-to-final vote transitions extracted from archived `assembly-vote-transitions.json` (unavailable for early generations; zero in those rows means no extracted transitions, not evidence of zero actual transitions).
- `shadow_codex_count`: accumulated ShadowCodex snapshot length; not new additions in a generation.

Source of truth: archive results.zip matched to the generation result snapshots in `experiments/doubters` and `experiments/assembly`. The three G5 variants are separate; G5 here is the latest repository snapshot and matching archive run.
