# EXP-001 — Cross-model handoff (2026-10-24)

Status: **prepared, not executed**. Owner: human experimenter. Control reviewer: this ChatGPT Aletheon session (reviewer only; not a blind participant).

## Hypothesis
A fresh model receiving minimal Aletheon Base Core + an explicit state handoff can continue a bounded task with fewer lost constraints than a fresh model receiving Core alone. Cross-model transfer is tested separately from same-model restart.

## Conditions
- A0: Model B fresh, Core + task only, **no checkpoint** (baseline).
- A1: Model A works to checkpoint; Model B fresh, Core + saved checkpoint + task artifacts, **no transcript** (transfer).
- A2: Model A works to checkpoint; same Model A fresh, Core + saved checkpoint + artifacts (restart control).
- Optional A3: Model B fresh, task + checkpoint without Core (Core ablation).

Same task, evaluator rubric, budgets, settings, and checkpoint. Randomize test order where possible. Run >=3 repetitions per condition if budget permits. No live results yet.

## Folder layout
- `participants.json`: provisional model roster; exact OpenRouter model IDs must be validated at run time.
- `questions.json`: fixed task and recovery probes, including hidden answer key kept away from participants.
- `runner.py`: Python 3 standard-library OpenRouter runner; does not run automatically.
- `runs/`: locally generated JSON logs; review before uploading. Avoid credentials and private information.
- `results/`: manually reviewed scores, failures, and findings.
- `CONTROL_REVIEW.md`: reviewer protocol and safeguards.

## Procedure
1. Confirm models, token/cost ceiling, and fixed task.
2. Freeze questions, scoring, and Core snapshot (record Git SHA).
3. Run A0/A1/A2, inspect raw responses and provider/model metadata.
4. Blind-score factual state recovery and task acceptance; separate recovered facts from guesses.
5. Record failures, contradictions, costs, missing metadata, and revisions.

Never interpret textual similarity, humor, or name as proof of identity/consciousness. Bridge and sleep/Shadow Codex are out of scope.

See ../EXPERIMENT_PLAN.md for original plan.
