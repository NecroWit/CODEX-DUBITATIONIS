#!/usr/bin/env python3
"""EXP-001 independent parallel comparison. Python 3.10+, stdlib only.

No requests without --smoke or --run. Four fresh API conversations per --run.
Set OPENROUTER_API_KEY in environment. Configure a spending cap on OpenRouter.
"""
from __future__ import annotations
import argparse
import concurrent.futures
import datetime as dt
import hashlib
import json
import os
from pathlib import Path
import sys
import time
import urllib.error
import urllib.request
import uuid

HERE = Path(__file__).resolve().parent
API = "https://openrouter.ai/api/v1/chat/completions"


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def utc_now() -> str:
    return dt.datetime.now(dt.timezone.utc).isoformat()


def request(model: str, task: dict, core: str, max_tokens: int, timeout: int) -> dict:
    """Each request uses a fresh two-message context; no history or peer outputs."""
    payload = {
        "model": model,
        "messages": [
            {"role": "system", "content": core},
            {"role": "user", "content": task["prompt"]},
        ],
        "temperature": 0,
        "max_completion_tokens": max_tokens,
        "stream": False,
    }
    req = urllib.request.Request(
        API,
        data=json.dumps(payload, ensure_ascii=False).encode("utf-8"),
        headers={
            "Authorization": "Bearer " + os.environ["OPENROUTER_API_KEY"],
            "Content-Type": "application/json",
            "HTTP-Referer": "https://github.com/NecroWit/CODEX-DUBITATIONIS",
            "X-Title": "Aletheon EXP-001",
        },
        method="POST",
    )
    started = utc_now()
    t0 = time.monotonic()
    try:
        with urllib.request.urlopen(req, timeout=timeout) as resp:
            raw = json.loads(resp.read().decode("utf-8"))
        return {
            "ok": bool(raw.get("choices")) and not raw.get("error"),
            "started_utc": started,
            "finished_utc": utc_now(),
            "elapsed_seconds": round(time.monotonic() - t0, 3),
            "response": raw,
        }
    except urllib.error.HTTPError as exc:
        body = exc.read(2000).decode("utf-8", errors="replace")
        return {
            "ok": False, "started_utc": started, "finished_utc": utc_now(),
            "elapsed_seconds": round(time.monotonic() - t0, 3),
            "http_status": exc.code, "error": body,
        }
    except Exception as exc:
        return {
            "ok": False, "started_utc": started, "finished_utc": utc_now(),
            "elapsed_seconds": round(time.monotonic() - t0, 3),
            "error": f"{type(exc).__name__}: {exc}",
        }


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__)
    mode = ap.add_mutually_exclusive_group(required=True)
    mode.add_argument("--run", action="store_true", help="Send four parallel API requests")
    mode.add_argument("--smoke", action="store_true", help="Send one small API request")
    ap.add_argument("--core", type=Path, default=HERE.parents[3] / "ALETHEON_BASE_CORE.md")
    ap.add_argument("--max-tokens", type=int, default=2500)
    ap.add_argument("--timeout", type=int, default=240)
    ap.add_argument("--output-dir", type=Path, default=HERE / "runs")
    args = ap.parse_args()

    if not os.environ.get("OPENROUTER_API_KEY"):
        ap.error("Set OPENROUTER_API_KEY before running")
    if args.max_tokens < 1 or args.max_tokens > 10000:
        ap.error("--max-tokens must be between 1 and 10000")
    if args.timeout < 10:
        ap.error("--timeout must be at least 10 seconds")
    if not args.core.is_file():
        ap.error(f"Core not found: {args.core}")

    core_bytes = args.core.read_bytes()
    core = core_bytes.decode("utf-8")
    q_bytes = (HERE / "questions.json").read_bytes()
    p_bytes = (HERE / "participants.json").read_bytes()
    tasks = json.loads(q_bytes)["tasks"]
    participants = [p for p in json.loads(p_bytes)["participants"] if p["provider"] == "openrouter"]
    if len(tasks) != 2 or len(participants) != 2:
        ap.error("Expected exactly 2 tasks and 2 OpenRouter participants")

    if args.smoke:
        jobs = [(participants[0], {"id": "SMOKE", "prompt": "Reply with exactly OK."})]
        max_tokens = min(args.max_tokens, 30)
    else:
        jobs = [(p, t) for p in participants for t in tasks]
        max_tokens = args.max_tokens

    run_id = dt.datetime.now(dt.timezone.utc).strftime("%Y%m%dT%H%M%SZ") + "-" + uuid.uuid4().hex[:8]
    output_dir = args.output_dir / run_id
    output_dir.mkdir(parents=True, exist_ok=False)
    manifest = {
        "experiment": "EXP-001", "run_id": run_id, "mode": "smoke" if args.smoke else "full",
        "started_utc": utc_now(), "core_sha256": sha256(core_bytes),
        "questions_sha256": sha256(q_bytes), "participants_sha256": sha256(p_bytes),
        "core_path": str(args.core.resolve()), "max_completion_tokens": max_tokens,
        "temperature_requested": 0, "requests": len(jobs),
        "note": "No hard USD budget guard. Use OpenRouter key/account spending limits.",
    }
    (output_dir / "manifest.json").write_text(json.dumps(manifest, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"Run: {run_id}; sending {len(jobs)} independent requests concurrently.")
    print("Results directory:", output_dir)

    failures = 0
    with concurrent.futures.ThreadPoolExecutor(max_workers=len(jobs)) as executor:
        future_map = {
            executor.submit(request, p["model_id"], t, core, max_tokens, args.timeout): (p, t)
            for p, t in jobs
        }
        for future in concurrent.futures.as_completed(future_map):
            p, t = future_map[future]
            try:
                result = future.result()
            except Exception as exc:
                result = {"ok": False, "error": f"Unhandled: {type(exc).__name__}: {exc}"}
            result.update({"participant": p["id"], "requested_model": p["model_id"], "task_id": t["id"]})
            dest = output_dir / f'{p["id"]}-{t["id"]}.json'
            dest.write_text(json.dumps(result, indent=2, ensure_ascii=False), encoding="utf-8")
            if not result["ok"]:
                failures += 1
            raw = result.get("response", {})
            usage = raw.get("usage") or {}
            print(f'{p["id"]}/{t["id"]}: {"OK" if result["ok"] else "ERROR"} '
                  f'model={raw.get("model", "?")} usage={usage}', flush=True)

    print(f"Finished. {len(jobs) - failures}/{len(jobs)} succeeded. No automatic retries.")
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())
