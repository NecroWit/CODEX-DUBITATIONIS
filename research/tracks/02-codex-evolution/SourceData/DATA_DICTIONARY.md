# Data dictionary

Source: private results.zip. Each generation JSON contains the selected result and only derived research data; no raw HTTP requests, responses, configuration secrets or cost ledgers.

- `generation`: generation number.
- `run_id`: source archive directory ID.
- `source_repo_snapshot`: reference to corresponding public result.
- `proposals`: anonymized proposal IDs (P01–P04), action, target, text, reason.
- `votes`: binding/preliminary/final decisions; `choice` and `reason` are direct source values.
- `reviews`: arguments for/against proposals, not quantitative liking scores.
- `vote_transitions`: individual preliminary to final vote changes.
- `shadow_codex`: snapshots of accumulated shadow ideas; do not sum counts across generations.
- `xi_interest`: ranked-choice interest votes; **not a numeric preference scale**.
- `protocols`: run-level non-secret methodological settings.
- `generation_summary`: counts of history records and voting stages, not independent experimental trials.

Null or missing source fields indicate absent evidence; never infer individual votes from `Applied`.
