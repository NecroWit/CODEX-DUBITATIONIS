"""Reusable OpenRouter infrastructure for Aletheon Python experiments.

Adapted from experiments/Aletheon.Trial/{OpenRouterClient,BudgetManager,TrialConfig}.cs.
Only Python standard library. Never silently substitute a model.
"""
from __future__ import annotations
from dataclasses import dataclass
from decimal import Decimal, ROUND_UP
from pathlib import Path
from threading import Lock
from typing import Any
import json
import os
import time
import urllib.error
import urllib.request


def parse_response(raw: dict[str, Any]) -> dict[str, Any]:
    """HTTP 200 does NOT mean the generation completed."""
    choices = raw.get("choices") or []
    if not choices:
        return {"valid": False, "reason": "missing choices", "content": "", "finish_reason": None}
    choice = choices[0]
    message = choice.get("message") or {}
    content = message.get("content")
    if isinstance(content, list):
        content = "\n".join(part.get("text", "") for part in content if isinstance(part, dict) and part.get("type") == "text")
    content = content if isinstance(content, str) else ""
    finish = choice.get("finish_reason")
    valid = finish == "stop" and bool(content.strip()) and not raw.get("error")
    return {
        "valid": valid, "reason": "complete" if valid else
            ("truncated" if finish == "length" else "empty content" if not content.strip() else f"finish_reason={finish}"),
        "content": content, "finish_reason": finish,
        "native_finish_reason": choice.get("native_finish_reason"),
        "reasoning_present": bool(message.get("reasoning")),
        "returned_model": raw.get("model"),
        "usage": raw.get("usage") or {},
    }


@dataclass(frozen=True)
class Price:
    input_per_million: Decimal
    output_per_million: Decimal


def load_model_catalog(path: Path) -> dict[str, Price]:
    data = json.loads(path.read_text(encoding="utf-8"))
    prices = {}
    for model, price in data["models"].items():
        prices[model] = Price(Decimal(str(price["inputUsdPerMillionTokens"])),
                              Decimal(str(price["outputUsdPerMillionTokens"])))
        if prices[model].input_per_million <= 0 or prices[model].output_per_million <= 0:
            raise ValueError(f"Invalid model price: {model}")
    return prices


class Budget:
    """Thread-safe conservative reservation. NOT a guaranteed provider billing cap."""
    def __init__(self, prices: dict[str, Price], total_usd: str, per_request_usd: str, ledger: Path):
        self.prices = prices
        self.total = Decimal(total_usd)
        self.per_request = Decimal(per_request_usd)
        self.ledger = ledger
        self.lock = Lock()
        self.committed = Decimal("0")
        if self.total <= 0 or self.per_request <= 0 or self.per_request > self.total:
            raise ValueError("Invalid budget limits")

    def _log(self, item: dict) -> None:
        self.ledger.parent.mkdir(parents=True, exist_ok=True)
        with self.ledger.open("a", encoding="utf-8") as f:
            f.write(json.dumps(item, ensure_ascii=False) + "\n")

    def reserve(self, label: str, model: str, messages: list[dict], max_tokens: int) -> Decimal | None:
        price = self.prices[model]
        # Same deliberately conservative char-count estimate as legacy C#.
        input_estimate = sum(len(str(m.get("content", ""))) for m in messages) + 1024
        estimated = ((Decimal(input_estimate) * price.input_per_million +
                      Decimal(max_tokens) * price.output_per_million) / Decimal(1_000_000))
        estimated = estimated.quantize(Decimal("0.00001"), rounding=ROUND_UP)
        with self.lock:
            if estimated > self.per_request or self.committed + estimated > self.total:
                self._log({"label": label, "model": model, "reservedUsd": str(estimated), "status": "blocked"})
                return None
            self.committed += estimated
            self._log({"label": label, "model": model, "reservedUsd": str(estimated), "status": "reserved"})
            return estimated

    def reconcile(self, label: str, model: str, reserved: Decimal, actual: Any) -> None:
        with self.lock:
            charged = None
            try:
                if actual is not None:
                    charged = Decimal(str(actual))
                    if not charged.is_finite() or charged < 0:
                        charged = None
            except (ValueError, ArithmeticError):
                charged = None
            if charged is not None:
                self.committed = max(Decimal("0"), self.committed - reserved + charged)
            self._log({"label": label, "model": model, "reservedUsd": str(reserved),
                       "reportedUsd": str(charged) if charged is not None else None,
                       "status": "reconciled" if charged is not None else "reservation_retained",
                       "cumulativeUsd": str(self.committed)})


def ask(model: str, messages: list[dict], max_tokens: int, *,
        timeout: int = 240, temperature: float = 0, retries: int = 1,
        reasoning: dict | None = None) -> dict:
    """Fresh stateless request; bounded transient HTTP retries, raw response preserved."""
    key = os.getenv("OPENROUTER_API_KEY")
    if not key:
        raise RuntimeError("OPENROUTER_API_KEY is not set")
    payload = {"model": model, "messages": messages, "max_completion_tokens": max_tokens,
               "temperature": temperature, "stream": False}
    if reasoning is not None:
        payload["reasoning"] = reasoning
    started = time.monotonic()
    for attempt in range(1, retries + 2):
        req = urllib.request.Request("https://openrouter.ai/api/v1/chat/completions",
            data=json.dumps(payload, ensure_ascii=False).encode("utf-8"),
            headers={"Authorization": "Bearer " + key, "Content-Type": "application/json",
                     "HTTP-Referer": "https://github.com/NecroWit/CODEX-DUBITATIONIS",
                     "X-Title": "Aletheon Experiments"}, method="POST")
        try:
            with urllib.request.urlopen(req, timeout=timeout) as response:
                raw = json.loads(response.read().decode("utf-8"))
            parsed = parse_response(raw)
            return {"ok": parsed["valid"], "parsed": parsed, "response": raw,
                    "attempts": attempt, "elapsed_seconds": round(time.monotonic() - started, 3)}
        except urllib.error.HTTPError as exc:
            body = exc.read(4000).decode("utf-8", errors="replace")
            if exc.code in (429, 500, 502, 503, 504) and attempt <= retries:
                time.sleep(min(15, 5 * attempt))
                continue
            return {"ok": False, "http_status": exc.code, "error": body,
                    "attempts": attempt, "elapsed_seconds": round(time.monotonic() - started, 3)}
        except Exception as exc:
            return {"ok": False, "error": f"{type(exc).__name__}: {exc}",
                    "attempts": attempt, "elapsed_seconds": round(time.monotonic() - started, 3)}
    raise AssertionError("unreachable")
