#!/usr/bin/env python3
"""Attach independent benchmark metadata to a completed Assembly run.

Never queries external services, alters experiment inputs, or guesses benchmark scores.
Manually verify and enter snapshots in research/model-benchmarks.json first.
"""
import argparse
import json
from pathlib import Path


def load(path):
    return json.loads(path.read_text(encoding="utf-8"))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("run", type=Path, help="Completed Assembly run directory")
    parser.add_argument("--catalog", type=Path, default=Path(__file__).with_name("model-benchmarks.json"))
    args = parser.parse_args()
    run = args.run
    if not (run / "assembly-next-state.json").exists():
        parser.error("Only completed Assembly runs may be annotated")
    config_path = run / "config.json"
    if not config_path.exists():
        parser.error("Run config.json is required for exact model identification")
    config = load(config_path)
    catalog = load(args.catalog)
    snapshots = catalog.get("snapshots", [])
    output = []
    for agent in config.get("Agents", []):
        model = agent["Model"]
        matches = [s for s in snapshots if s.get("model_id") == model]
        # Explicitly preserve missing coverage; never infer scores from related models.
        output.append({
            "role": agent["Id"], "model_id": model,
            "price_tier": next((s.get("price_tier") for s in matches if s.get("price_tier")), None),
            "benchmark_snapshots": matches,
            "coverage": "exact_match" if matches else "unavailable",
        })
    result = {
        "schema_version": 1,
        "source_catalog": str(args.catalog),
        "method": "exact model ID matching; no interpolation or imputation",
        "agents": output,
    }
    destination = run / "assembly-model-benchmarks.json"
    destination.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Wrote {destination} ({sum(a['coverage'] == 'exact_match' for a in output)} matched agents)")


if __name__ == "__main__":
    main()
