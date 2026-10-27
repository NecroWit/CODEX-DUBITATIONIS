# EXP-2 — complete run inventory

Recomputed from user ZIPs on 2026-10-27: **12 unique runs, 23 model-side records**. Each line: EXP ID, run UTC HHMMSS, side, actual returned model, technical acceptance R0/H1/H2/H3, weighted actual USD/1M tokens (rounded to 3 decimals). Do not mistake technical acceptance for semantic quality. Assigned-vs-returned model mismatches are detailed in `EXP-2.0-2.9-AUDIT.md` and `EXP-2.9-CORRECTION.md`.

```
2.0 221834 A llama-4-maverick Y/Y/Y/Y 0.098
2.0 221834 B gemini-3-flash-preview Y/Y/Y/Y 0.562
2.0 233118 A llama-4-maverick Y/Y/N/Y 0.247
2.0 233118 B gemini-3-flash-preview Y/Y/Y/Y 0.558
2.1 234042 A deepseek-v3.2 Y/Y/Y/Y 0.197
2.1 234042 B mistral-small-3.2-24b Y/Y/Y/Y 0.092
2.2 234607 A kimi-k2.5 Y/Y/Y/Y 0.528
2.2 234607 B gpt-5-mini Y/Y/Y/N 0.184
2.3 000441 A grok-4.6 Y/Y/Y/Y 2.144
2.3 000441 B claude-haiku-4.5 Y/N/Y/N 1.152
2.4 001658 A command-a-plus Y/N/Y/Y 0.379
2.4 142427 A mistral-small-3.2-24b Y/N/Y/N 0.098
2.4 142427 B grok-4.3 Y/Y/Y/Y 0.646
2.5 143641 A mistral-small-3.2-24b Y/Y/Y/Y 0.097
2.5 143641 B nemotron-3-nano-30b-a3b N/Y/N/N 0.027
2.6 143925 A gpt-5-mini Y/Y/Y/Y 0.182
2.6 143925 B gemini-3-flash-preview Y/Y/Y/Y 0.560
2.7 144659 A claude-haiku-4.5 Y/Y/Y/N 1.145
2.7 144659 B mistral-medium-3.1 Y/Y/Y/Y 0.260
2.8 145048 A command-a-plus Y/Y/Y/Y 0.398
2.8 145048 B claude-haiku-4.5 Y/N/Y/N 1.134
2.9 145347 A mistral-small-3.2-24b Y/Y/Y/Y 0.098
2.9 145347 B deepseek-v3.2 Y/Y/N/N 0.170
```

EXP-2.0 has two runs, EXP-2.4 has two runs (one A-only). EXP-2.9 figures use complete replacement archive. Full-precision totals and exact run IDs are in the original ZIP files. No frozen blind C control has been established; H1/H2 C file is explicitly post-hoc, H3 C missing. Scores remain pending manual assessment.
