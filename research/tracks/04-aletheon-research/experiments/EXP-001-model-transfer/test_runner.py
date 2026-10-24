#!/usr/bin/env python3
"""Offline validation of EXP-001 runner configuration. No API calls."""
import json
import pathlib
import unittest

ROOT = pathlib.Path(__file__).resolve().parent

class ConfigTests(unittest.TestCase):
    def test_two_tasks(self):
        tasks = json.loads((ROOT / "questions.json").read_text(encoding="utf-8"))["tasks"]
        self.assertEqual({t["id"] for t in tasks}, {"T1", "H1"})
        self.assertEqual(len(tasks), 2)
        self.assertTrue(all(t["prompt"].strip() for t in tasks))

    def test_two_api_models(self):
        participants = json.loads((ROOT / "participants.json").read_text(encoding="utf-8"))["participants"]
        api = [p for p in participants if p["provider"] == "openrouter"]
        self.assertEqual(len(api), 2)
        self.assertNotEqual(api[0]["model_id"], api[1]["model_id"])
        self.assertTrue(all("/" in p["model_id"] for p in api))

    def test_runner_is_importable(self):
        import importlib.util
        spec = importlib.util.spec_from_file_location("exp001_runner", ROOT / "runner.py")
        module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)
        self.assertEqual(len(module.sha256(b"abc")), 64)

if __name__ == "__main__":
    unittest.main()
