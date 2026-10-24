#!/usr/bin/env python3
"""Offline shared-library regression tests. No network or paid calls."""
import importlib.util
import json
from pathlib import Path
import sys
import tempfile
import unittest

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[4]
sys.path.insert(0, str(ROOT))
from research.common.openrouter import Budget, load_model_catalog, parse_response


class SharedTests(unittest.TestCase):
    def test_finished_content(self):
        r = parse_response({"choices": [{"finish_reason": "stop", "message": {"content": "Done"}}]})
        self.assertTrue(r["valid"])

    def test_length_not_success(self):
        r = parse_response({"choices": [{"finish_reason": "length", "message": {"content": ""}}]})
        self.assertFalse(r["valid"])
        self.assertEqual(r["reason"], "truncated")

    def test_reasoning_only_not_success(self):
        r = parse_response({"choices": [{"finish_reason": "stop",
                            "message": {"content": "", "reasoning": "thinking"}}]})
        self.assertFalse(r["valid"])

    def test_missing_choices(self):
        self.assertFalse(parse_response({})["valid"])

    def test_catalog(self):
        catalog = load_model_catalog(ROOT / "research/common/models.json")
        self.assertIn("z-ai/glm-5.3", catalog)
        self.assertIn("deepseek/deepseek-v3.2", catalog)

    def test_parallel_budget_reservation(self):
        from concurrent.futures import ThreadPoolExecutor
        catalog = load_model_catalog(ROOT / "research/common/models.json")
        with tempfile.TemporaryDirectory() as tmp:
            budget = Budget(catalog, "0.00010", "0.00010", Path(tmp) / "ledger.jsonl")
            messages = [{"role": "user", "content": "x" * 200}]
            with ThreadPoolExecutor(max_workers=12) as pool:
                results = list(pool.map(lambda _: budget.reserve("test", "z-ai/glm-5.3", messages, 100), range(12)))
            self.assertLess(sum(r is not None for r in results), 12)
            self.assertLessEqual(budget.committed, budget.total)

    def test_experiment_configuration(self):
        tasks = json.loads((HERE / "questions.json").read_text(encoding="utf-8"))["tasks"]
        people = json.loads((HERE / "participants.json").read_text(encoding="utf-8"))["participants"]
        self.assertEqual({t["id"] for t in tasks}, {"T1", "H1"})
        self.assertEqual(len([p for p in people if p["provider"] == "openrouter"]), 2)


if __name__ == "__main__":
    unittest.main()
