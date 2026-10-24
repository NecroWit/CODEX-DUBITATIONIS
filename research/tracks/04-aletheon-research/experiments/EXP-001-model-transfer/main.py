#!/usr/bin/env python3
"""EXP-001 — PyCharm entry point. Run this file using the green Run button.

Set OPENROUTER_API_KEY in PyCharm Run Configuration -> Environment variables.
Choose MODE below. No third-party packages needed (Python 3.10+).
"""
from pathlib import Path
import os
import subprocess
import sys

HERE = Path(__file__).resolve().parent

# Change only this setting to switch between trial and experiment.
MODE = "smoke"  # "smoke" (one request) or "run" (four parallel requests)
MAX_TOKENS = 2500


def main() -> int:
    if MODE not in ("smoke", "run"):
        print("ERROR: MODE must be 'smoke' or 'run'")
        return 2
    if not os.environ.get("OPENROUTER_API_KEY"):
        print("ERROR: Set OPENROUTER_API_KEY in PyCharm Run -> Edit Configurations.")
        print("Do not put your key in source code or commit it to GitHub.")
        return 2
    command = [
        sys.executable,
        str(HERE / "runner.py"),
        "--smoke" if MODE == "smoke" else "--run",
        "--max-tokens", str(MAX_TOKENS),
    ]
    print(f"EXP-001 | MODE={MODE} | Python={sys.version.split()[0]}", flush=True)
    print("Core is loaded automatically from repository root.", flush=True)
    return subprocess.call(command, cwd=str(HERE))


if __name__ == "__main__":
    raise SystemExit(main())
