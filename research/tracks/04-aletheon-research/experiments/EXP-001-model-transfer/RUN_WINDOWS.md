# EXP-001: PyCharm run guide (Python 3.10+)

1. Open the repository project in PyCharm, pull the latest GitHub changes.
2. Open `research/tracks/04-aletheon-research/experiments/EXP-001-model-transfer/main.py`.
3. Run `test_runner.py` first (right-click -> Run). Tests are offline and do not cost money.
4. In Run -> Edit Configurations for `main`, add environment variable `OPENROUTER_API_KEY`. Never commit the key.
5. In `main.py`, start with `MODE = "smoke"`; click Run.
6. Once smoke succeeds, change to `MODE = "run"`; click Run to send four independent concurrent requests.

Default `MAX_TOKENS=8000` includes reasoning tokens. The shared parser marks `finish_reason=length` and empty final content as INCOMPLETE, not success. It **does not automatically rerun or replace a model**: this would contaminate the comparison. A repeated run costs money and is a new trial.

Shared library: `research/common/openrouter.py`, model price estimates: `research/common/models.json`. Budget values `BUDGET_USD` and `MAX_REQUEST_USD` in main.py are local *estimates* only. Set a provider-side key spending cap. Review/update model price estimates before running. The GLM-5.3 catalog price is a conservative placeholder, not a verified current tariff.

Each run saves `manifest.json`, `cost-ledger.jsonl`, and `A-T1.json`, `A-H1.json`, `B-T1.json`, `B-H1.json` in its own timestamped folder under `runs/`. Failed and incomplete replies remain in the archive for diagnosis. The control C answers were frozen in `control/` before viewing A/B outputs.

No third-party Python dependencies. Earlier .NET experiments remain unchanged.
