#!/usr/bin/env python3
"""EXP-001: independent parallel model comparison, using research.common.

Run from PyCharm via main.py. All requests are fresh and independent.
"""
from __future__ import annotations
from concurrent.futures import ThreadPoolExecutor, as_completed
from datetime import datetime, timezone
from hashlib import sha256
from pathlib import Path
import argparse
import json
import os
import sys
import uuid

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[4]
sys.path.insert(0, str(REPO))
from research.common.openrouter import ask, Budget, load_model_catalog


def utc() -> str:
    return datetime.now(timezone.utc).isoformat()


def perform(p: dict, t: dict, core: str, tokens: int, budget: Budget,
            reasoning: dict | None) -> dict:
    model = p["model_id"]
    label = p["id"] + "-" + t["id"]
    messages = [{"role": "system", "content": core},
                {"role": "user", "content": t["prompt"]}]
    reserved = budget.reserve(label, model, messages, tokens)
    if reserved is None:
        return {"ok": False, "error": "Blocked by local estimated budget", "participant": p["id"],
                "task_id": t["id"], "requested_model": model}
    result = ask(model, messages, tokens, retries=1, reasoning=reasoning)
    budget.reconcile(label, model, reserved, (result.get("response") or {}).get("usage", {}).get("cost"))
    result.update({"participant": p["id"], "task_id": t["id"], "requested_model": model,
                   "submitted_utc": utc()})
    return result


def main() -> int:
    ap = argparse.ArgumentParser()
    mode = ap.add_mutually_exclusive_group(required=True)
    mode.add_argument("--run", action="store_true")
    mode.add_argument("--smoke", action="store_true")
    ap.add_argument("--core", type=Path, default=REPO / "ALETHEON_BASE_CORE.md")
    ap.add_argument("--max-tokens", type=int, default=8000)
    ap.add_argument("--budget-usd", default="1.00")
    ap.add_argument("--max-request-usd", default="0.40")
    ap.add_argument("--output-dir", type=Path, default=HERE / "runs")
    args = ap.parse_args()
    if not os.getenv("OPENROUTER_API_KEY"):
        ap.error("Set OPENROUTER_API_KEY in PyCharm Environment variables")
    if not 100 <= args.max_tokens <= 16000:
        ap.error("max-tokens must be 100..16000")
    if not args.core.is_file():
        ap.error(f"Missing Core: {args.core}")
    q_raw = (HERE / "questions.json").read_bytes()
    p_raw = (HERE / "participants.json").read_bytes()
    core_raw = args.core.read_bytes()
    questions = json.loads(q_raw)
    people = json.loads(p_raw)
    participants = [p for p in people["participants"] if p["provider"] == "openrouter"]
    if len(participants) != 2 or len(questions["tasks"]) != 2:
        ap.error("EXP-001 requires two API models and two tasks")
    models = load_model_catalog(REPO / "research/common/models.json")
    for p in participants:
        if p["model_id"] not in models:
            ap.error(f'Missing price for {p["model_id"]} in research/common/models.json')
    run_id = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ") + "-" + uuid.uuid4().hex[:8]
    dest = args.output_dir / run_id
    dest.mkdir(parents=True)
    budget = Budget(models, args.budget_usd, args.max_request_usd, dest / "cost-ledger.jsonl")
    jobs = ([(participants[0], {"id": "SMOKE", "prompt": "Reply exactly OK."})]
            if args.smoke else [(p, t) for p in participants for t in questions["tasks"]])
    tokens = 100 if args.smoke else args.max_tokens
    manifest = {"run_id": run_id, "experiment": "EXP-001", "started_utc": utc(),
                "core_sha256": sha256(core_raw).hexdigest(),
                "questions_sha256": sha256(q_raw).hexdigest(),
                "participants_sha256": sha256(p_raw).hexdigest(),
                "catalog_sha256": sha256((REPO / "research/common/models.json").read_bytes()).hexdigest(),
                "max_tokens": tokens, "max_budget_usd": args.budget_usd,
                "max_request_usd": args.max_request_usd,
                "note": "Estimated local gate only; configure provider-side spending cap."}
    (dest / "manifest.json").write_text(json.dumps(manifest, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"EXP-001 {run_id}: {len(jobs)} concurrent independent requests", flush=True)
    errors = 0
    with ThreadPoolExecutor(max_workers=len(jobs)) as executor:
        future_map = {executor.submit(perform, p, t, core_raw.decode("utf-8"), tokens, budget, None): (p, t)
                      for p, t in jobs}
        for future in as_completed(future_map):
            p, t = future_map[future]
            try:
                result = future.result()
            except Exception as exc:
                result = {"ok": False, "error": f"Unhandled: {exc}", "participant": p["id"],
                          "task_id": t["id"], "requested_model": p["model_id"]}
            (dest / f'{p["id"]}-{t["id"]}.json').write_text(
                json.dumps(result, indent=2, ensure_ascii=False), encoding="utf-8")
            if not result.get("ok"):
                errors += 1
            parsed = result.get("parsed") or {}
            print(f'{p["id"]}/{t["id"]}: {"COMPLETE" if result.get("ok") else "INCOMPLETE/ERROR"} '
                  f'finish={parsed.get("finish_reason")} reason={parsed.get("reason")}', flush=True)
    print(f"Saved {dest}; complete {len(jobs)-errors}/{len(jobs)}; no model substitutions.")
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
