#!/usr/bin/env python3
"""Build a reproducible, non-selective index of Assembly vote transitions."""
import argparse
import json
from pathlib import Path


def read(path):
    return json.loads(path.read_text(encoding="utf-8"))


def collect(run):
    generated = run / "assembly-vote-transitions.json"
    if generated.exists():
        return read(generated)
    required = ["assembly-input.json", "assembly-anonymous-proposals.json",
                "assembly-authorship.json", "assembly-preliminary-votes.json",
                "assembly-revisions.json", "assembly-final-votes.json",
                "assembly-next-state.json"]
    if not all((run / name).exists() for name in required):
        return []
    initial = read(run / "assembly-anonymous-proposals.json")
    authors = {p["Key"]: p["Author"] for p in read(run / "assembly-authorship.json")}
    prelim = {p["Key"]: p["PreliminaryVotes"] for p in read(run / "assembly-preliminary-votes.json")}
    final = {p["Key"]: p["FinalVotes"] for p in read(run / "assembly-final-votes.json")}
    revised = {p["Key"]: p["Revised"] for p in read(run / "assembly-revisions.json")}
    manifest = read(run / "assembly-input.json")
    config = read(run / "config.json") if (run / "config.json").exists() else {}
    models = {a["Id"]: a.get("Model") for a in config.get("Agents", [])}
    rows = []
    for item in initial:
        key, proposal = item["Key"], item["Proposal"]
        if proposal["Action"] == "PASS":
            continue
        final_by_voter = {v["Voter"]: v for v in final[key]}
        for before in prelim[key]:
            role = before["Voter"]
            after = final_by_voter[role]
            rows.append({
                "Generation": manifest["generation"], "Proposal": key,
                "VoterRole": role, "Model": models.get(role),
                "AuthorRole": authors.get(key), "Action": proposal["Action"],
                "TargetId": proposal.get("TargetId", ""),
                "InitialText": proposal["Text"], "RevisedText": revised[key]["Text"],
                "TextChanged": proposal["Text"] != revised[key]["Text"],
                "PreliminaryChoice": before["Choice"], "PreliminaryReason": before["Reason"],
                "FinalChoice": after["Choice"], "FinalReason": after["Reason"],
                "Transition": before["Choice"] + "->" + after["Choice"],
                "RunDirectory": run.name,
                "Evidence": required,
            })
    return rows


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("results", type=Path, help="experiments/results directory")
    parser.add_argument("--output", type=Path, default=Path("research/vote-transitions.jsonl"))
    args = parser.parse_args()
    rows = []
    for run in sorted(args.results.iterdir()):
        if run.is_dir():
            rows.extend(collect(run))
    rows.sort(key=lambda row: (row["Generation"], row["RunDirectory"],
                                row["Proposal"], row["VoterRole"]))
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text("".join(json.dumps(row, ensure_ascii=False) + "\n"
                                   for row in rows), encoding="utf-8")
    print(f"Indexed {len(rows)} vote transitions from completed Assembly runs into {args.output}")


if __name__ == "__main__":
    main()
