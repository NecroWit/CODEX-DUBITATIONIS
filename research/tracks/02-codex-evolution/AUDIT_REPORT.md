# Private archive extraction audit — 2026-10-18

The user-supplied private `results.zip` contains 108 run folders. 35 generations G1–G35 have selected results; G5 has three separate completed snapshots, of which the latest repository result was selected. Other/debug runs are not included in the generation-level summary.

A full local research dataset was generated: 35 JSON files, 165 proposal records, 1,335 vote records, 496 review records, 432 preliminary-to-final vote transitions, 180 article-history records, 60 XI interest choices, and other derived tables. **The full dataset is not yet committed to GitHub; only the compact summary and documentation here are public.**

No raw API request/response dumps, cost ledgers or config secrets were included in the generated dataset. Pattern scanning found no obvious API tokens or email addresses, but this is not a guarantee of complete privacy review. Source archives remain private.

Crucial caveat: observed `REJECT->ACCEPT` transitions are numerous; these are within-proposal voting transitions and do not establish causal effects of Shadow Codex. Numeric liking/willingness scales were not found in these structured generation outputs. The full detailed dataset should be independently reviewed for privacy before publishing.
