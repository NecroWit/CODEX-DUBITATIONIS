# Experiment numbering policy (2026-10-24)

**Rule:** Every experimental attempt before the first fully successful experiment has a number strictly below 1. **EXP-001** is reserved for the first run meeting all preregistered success criteria; it has **not yet occurred**.

- **EXP-0.1** — first pilot, 2500-token completion cap; incomplete. Historical legacy label: EXP-001 pilot.
- **EXP-0.2** — 2026-10-24 C# run `20261024T173608Z-246d8e7f`, 12000-token cap; 2/4 complete. Historical legacy label: EXP-001 model-transfer rerun.
- **EXP-0.3, EXP-0.4, ...** — subsequent diagnostic/calibration attempts, in chronological order, until a fully successful run.
- **EXP-001** — first fully successful comparative run, after checking preregistered success criteria. Later successful experiments increment as EXP-002, EXP-003, etc.

## Traceability

Do not rename historical folders, run IDs, raw outputs or prior commit history merely to retrofit numbering. Existing `EXP-001-model-transfer` path is a **legacy path**, not evidence that EXP-001 succeeded. Cite both canonical and legacy IDs when necessary. Retain unsuccessful runs as scientific evidence. Any new run should record canonical experiment ID, legacy/source ID if any, protocol version, participant/model identifiers, prompts, limits, timestamps, completion statuses and costs.

## Success criteria

Before each comparative attempt, freeze a protocol with its task prompts, participants, completion/length requirements, budget, analysis rules and acceptance criteria. API success alone does not imply experiment success. Mark as successful only after all required answers are complete, usable and comparable under the frozen protocol; otherwise retain it under the next EXP-0.x number. The first successful experiment receives EXP-001, not a retroactive reinterpretation of the earlier incomplete runs.
