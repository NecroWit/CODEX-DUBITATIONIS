#!/usr/bin/env python3
"""EXP-001 PyCharm entry point (Python 3.10+, no pip dependencies).

Configure OPENROUTER_API_KEY in PyCharm Run Configuration.
Change MODE to 'run' after a successful smoke test.
"""
import os
from pathlib import Path
import subprocess
import sys

HERE = Path(__file__).resolve().parent
MODE = "smoke"  # "smoke" or "run"
MAX_TOKENS = 8000
BUDGET_USD = "1.00"
MAX_REQUEST_USD = "0.40"


def main():
    if MODE not in ("smoke", "run"):
        print("Invalid MODE")
        return 2
    if not os.getenv("OPENROUTER_API_KEY"):
        print("Set OPENROUTER_API_KEY in PyCharm Run -> Edit Configurations")
        return 2
    args = [sys.executable, str(HERE / "runner.py"),
            "--smoke" if MODE == "smoke" else "--run",
            "--max-tokens", str(MAX_TOKENS),
            "--budget-usd", BUDGET_USD,
            "--max-request-usd", MAX_REQUEST_USD]
    print(f"EXP-001 | MODE={MODE} | Python={sys.version.split()[0]}", flush=True)
    return subprocess.call(args, cwd=str(HERE))


if __name__ == "__main__":
    raise SystemExit(main())
