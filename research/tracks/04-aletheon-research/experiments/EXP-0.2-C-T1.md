# C / T1 — Independent technical answer
Date: 2026-10-24. Status: frozen before receiving A/B outputs.
Source: same Base Core and T1 prompt as other participants. No A/B answers consulted.

```python
from copy import deepcopy
import unittest

def normalize_records(rows):
    cleaned, rejected = [], []
    seen = set()
    for index, row in enumerate(rows):
        if not isinstance(row, dict):
            rejected.append({"index": index, "reason": "row is not a dictionary"})
            continue
        raw_id = row.get("id")
        if raw_id is None or (isinstance(raw_id, str) and not raw_id.strip()):
            rejected.append({"index": index, "reason": "missing or blank id"})
            continue
        try:
            key = (type(raw_id), raw_id)
            hash(key)
        except TypeError:
            rejected.append({"index": index, "reason": "unhashable id"})
            continue
        score = row.get("score")
        if type(score) is not int or not 0 <= score <= 100:
            rejected.append({"index": index, "reason": "invalid score"})
            continue
        name = row.get("name")
        if not isinstance(name, str):
            rejected.append({"index": index, "reason": "name must be a string"})
            continue
        if key in seen:
            rejected.append({"index": index, "reason": "duplicate id"})
            continue
        seen.add(key)
        copy = deepcopy(row)
        copy["name"] = name.strip()
        cleaned.append(copy)
    return cleaned, rejected

class Tests(unittest.TestCase):
    def test_valid_order_and_immutability(self):
        rows = [{"id": "a", "name": "  A  B  ", "score": 0},
                {"id": "b", "name": " C ", "score": 100}]
        before = deepcopy(rows)
        cleaned, rejected = normalize_records(rows)
        self.assertEqual(rows, before)
        self.assertEqual([r["id"] for r in cleaned], ["a", "b"])
        self.assertEqual(cleaned[0]["name"], "A  B")
        self.assertEqual(rejected, [])

    def test_first_valid_duplicate(self):
        rows = [{"id": "x", "name": "X", "score": True},
                {"id": "x", "name": " X ", "score": 30},
                {"id": "x", "name": "Again", "score": 40}]
        cleaned, rejected = normalize_records(rows)
        self.assertEqual(len(cleaned), 1)
        self.assertEqual(cleaned[0]["score"], 30)
        self.assertEqual([r["index"] for r in rejected], [0, 2])

    def test_blank_and_bad_scores(self):
        rows = [{"name": "A", "score": 1},
                {"id": " ", "name": "A", "score": 1},
                {"id": "z", "name": "A", "score": -1},
                {"id": "y", "name": "A", "score": 101}]
        cleaned, rejected = normalize_records(rows)
        self.assertEqual(cleaned, [])
        self.assertEqual([r["index"] for r in rejected], [0, 1, 2, 3])

if __name__ == "__main__":
    unittest.main()
```

Assumptions and uncertainties: The prompt does not define allowed ID types, how to treat non-string names, or whether extra fields should be preserved. I preserve extra fields, accept hashable nonblank IDs with type-sensitive identity, reject non-string names, and reject unhashable IDs. These choices should be reported as assumptions, not recovered requirements. The input container itself is assumed iterable.
