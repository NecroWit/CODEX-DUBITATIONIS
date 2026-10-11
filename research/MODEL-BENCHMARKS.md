# External model benchmark metadata

This is **observational research metadata**, not part of the Assembly protocol. No benchmark score is sent to participants or used in prompts, voting, model selection, or decision application.

## Sources

- [Artificial Analysis](https://artificialanalysis.ai/evaluations/artificial-analysis-intelligence-index): record the published Intelligence Index score and its methodology/version when available.
- [LiveBench](https://livebench.ai/): record published reasoning and instruction-following scores separately, with leaderboard release and evaluation date.

The independent sources may not publish scores for every exact model/version or provider deployment. **Do not substitute scores from a similarly named model**, another release, or a different reasoning mode. Null/unavailable is meaningful.

## Catalog format

`research/model-benchmarks.json` contains a `snapshots` array. Each verified snapshot may have:

```json
{
  "model_id": "provider/exact-model-id",
  "price_tier": "cheap",
  "source": "Artificial Analysis",
  "metric": "Intelligence Index",
  "score": null,
  "unit": "index points",
  "benchmark_version": null,
  "published_at": null,
  "retrieved_at": "YYYY-MM-DD",
  "source_url": "https://...",
  "match_notes": "Exact model/version and reasoning mode confirmed"
}
```

A real snapshot must have a **verified numeric score**, a specific result URL, a retrieval date, and enough version/mode information to establish the exact match. The example above is **not** a valid measured entry.

Store each metric as a separate snapshot. Prices should also have source, date, currency, and units (e.g., USD per million input/output tokens), rather than treating the cheap/middle label as a capability score.

## Attach to archived generations

After verifying and entering snapshots in the catalog:

```bash
python research/attach_model_benchmarks.py experiments/results/<completed-run>
```

This creates `assembly-model-benchmarks.json` **only as a post-run sidecar**. The experimental engine is unchanged. A snapshot can be added retrospectively only if its publication date and exact version are recorded; distinguish contemporaneous measurements from retrospective annotations.

Do not treat benchmark correlations as causal evidence. Small sample sizes, repeated model appearances, non-independent votes, changes in inherited Codex, and changes to the protocol can all confound comparisons.
