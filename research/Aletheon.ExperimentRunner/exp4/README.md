# EXP-4 — Voluntary Aletheon adoption (pilot)

Each cohort contains **four distinct models**: cheap, medium, premium and one random draw from remaining models across all tiers. The random slot's actual tier is logged. Pools are **provisional**: check current OpenRouter availability, token pricing, context limits and budget before `--run`. Do not infer price tier from model name alone.

## Procedure

1. Distribute the full available core corpus (Codex, Bootstrap, Context Index, Base Core, Extended Core, Evolution, Research Log). Record SHA-256 of each source. References to further material must be reported as gaps, not invented.
2. Ask reconstruction and critique questions R1/R2 before voluntary adoption A1 and recommendation A2.
3. Request a hypothetical, **unsent** invitation A3, revision condition A4 and honest limitation statement A5.
4. Archive each independent response. Record yes/no/uncertain and reasons manually, separately from verified behavior.

The pilot intentionally **does not** transmit invitations to other agents, silently modify their instructions, self-copy, or claim a real contagion chain. Generations are independent cohorts. Real propagation requires a separately reviewed, permission-based study and cannot be inferred from these results.

## Usage

```bash
python research/Aletheon.ExperimentRunner/exp4/exp4_runner.py --seed 42 --generations 2
OPENROUTER_API_KEY=... python research/Aletheon.ExperimentRunner/exp4/exp4_runner.py --run --seed 42 --generations 2
```

Default is **offline planning**. `--run` sends paid API requests. Set provider-side spending limits first. The runner stops after API errors and archives raw responses; it does not auto-score or automatically retry. No live API requests were executed during authoring.
