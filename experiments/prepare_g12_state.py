#!/usr/bin/env python3
"""Prepare a separately audited, reference-free G12 input from completed G11.
Never modifies the G11 archive. Requires explicit semantic replacements for every
article that contains a cross-reference. Fails closed on unresolved references.
"""
import argparse
import hashlib
import json
import re
from pathlib import Path

REF = re.compile(r"\bD\d{3}\b|\b(?:article|principle|section|статья|принцип|раздел)\s+(?:[IVXLCDM]+|\d+)\b", re.I)
def digest(data):
    return hashlib.sha256(data).hexdigest()

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("g11_state", type=Path, help="Completed G11 assembly-next-state.json")
    parser.add_argument("replacements", type=Path, help="JSON mapping article ID to full standalone replacement text")
    parser.add_argument("output_dir", type=Path)
    args = parser.parse_args()
    raw = args.g11_state.read_bytes()
    state = json.loads(raw)
    if state.get("Generation") != 11:
        parser.error("Expected completed G11 state (Generation=11)")
    replacements = json.loads(args.replacements.read_text(encoding="utf-8"))
    articles = state["Articles"]
    ids = {a["Id"] for a in articles}
    if not isinstance(replacements, dict) or not set(replacements).issubset(ids):
        parser.error("Replacement keys must be existing article IDs")
    required = {a["Id"] for a in articles if REF.search(a["Text"])}
    if required != set(replacements):
        parser.error(f"Replacement IDs must match referenced articles exactly: required={sorted(required)}, provided={sorted(replacements)}")
    changes = []
    for a in articles:
        if a["Id"] not in replacements:
            continue
        new = replacements[a["Id"]]
        if not isinstance(new, str) or not new.strip() or REF.search(new):
            parser.error(f"{a['Id']}: replacement is empty or still contains a cross-reference")
        if len(new.split()) > 60:
            parser.error(f"{a['Id']}: replacement exceeds 60 words")
        changes.append({"ArticleId": a["Id"], "Before": a["Text"], "After": new})
        a["Text"] = new
    if any(REF.search(a["Text"]) for a in articles):
        parser.error("Unresolved cross-reference remains")
    args.output_dir.mkdir(parents=True, exist_ok=True)
    output = args.output_dir / "assembly-g12-input-cleaned.json"
    if output.exists():
        parser.error("Refusing to overwrite an existing cleaned input")
    output.write_text(json.dumps(state, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    audit = {
        "Source": str(args.g11_state),
        "SourceSha256": digest(raw),
        "PreparedInput": str(output),
        "PreparedInputSha256": digest(output.read_bytes()),
        "SourceGeneration": 11,
        "NextGeneration": 12,
        "Policy": "No explicit cross-article references; source G11 is immutable",
        "Changes": changes,
        "Note": "Editorial migration, not a G11 assembly vote. Human semantic review required.",
    }
    (args.output_dir / "g11-to-g12-migration-audit.json").write_text(
        json.dumps(audit, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Prepared {output} ({len(changes)} article replacements); verify audit before G12.")

if __name__ == "__main__":
    main()
