# Aletheon — model comparison and effective token cost

> Convergence and logical-consistency scores are **subjective preliminary judgments**, not validated measurements. Missing values are unknown, not zero. Effective token prices are observed from archived provider usage, not public tariffs.

## Unified comparison — EXP > 1 (EXP-1.1 and EXP-1.2)

![EXP > 1: three bars per model, scores on left Y-axis and actual USD per million tokens on right Y-axis](EXP_GT1_DUAL_AXIS.svg)

**One chart, two Y-axes, no normalization:** blue = convergence with C (%), green = logical consistency (%), orange = observed effective cost (USD per million input + output tokens). Left axis: 0–100%; right axis: $0–$0.50 per million tokens. Higher scores are better; lower cost is better.

| Experiment | Model | Convergence (%) | Logical consistency (%) | Effective USD / 1M total tokens |
|---|---|---:|---:|---:|
| EXP-1.1 | DeepSeek V3.2 | 88 | 82 | $0.28448 |
| EXP-1.1 | Mistral Small 3.2 | 73 | 63 | $0.09961 |
| EXP-1.2 | GPT-5 Mini | 87 | 81 | $0.29799 |
| EXP-1.2 | Qwen3.5 Plus | 85 | 84 | $0.43921 |

Scores are subjective preliminary assessments. Effective price is total actual USD cost / total billed input and output tokens × 1,000,000. The embedded SVG is the canonical combined summary visualization for EXP > 1; extend it and this table as new experiments are evaluated.

## Convergence with frozen C (%)

```mermaid
xychart-beta
    title "Convergence with C — evaluated model/experiment pairs"
    x-axis ["0.4 Nano", "0.5 GLM", "0.5 Qwen", "0.7 GPT4", "0.7 Nemo", "1.1 DeepSeek", "1.1 Mistral", "1.2 GPT5", "1.2 Qwen"]
    y-axis "Score (%)" 0 --> 100
    bar [75, 88, 73, 84, 62, 88, 73, 87, 85]
```

## Logical consistency (%): only experiments actually assessed

```mermaid
xychart-beta
    title "Logical consistency — EXP-1.1 and EXP-1.2"
    x-axis ["1.1 DeepSeek", "1.1 Mistral", "1.2 GPT5", "1.2 Qwen"]
    y-axis "Score (%)" 0 --> 100
    bar [82, 63, 81, 84]
```

## Average effective token price (USD per 1 million tokens)

```mermaid
xychart-beta
    title "Observed blended cost per 1M tokens"
    x-axis ["1.1 DeepSeek", "1.1 Mistral", "1.2 GPT5", "1.2 Qwen"]
    y-axis "USD / 1M tokens" 0 --> 0.5
    bar [0.28448, 0.09961, 0.29799, 0.43921]
```

Calculated as **sum of actual USD cost / sum of (prompt_tokens + completion_tokens) × 1,000,000**, pooling H1 and H2 for each model. This is a **token-weighted blended effective price**, not the arithmetic mean of input and output tariffs. Includes billed reasoning tokens where reported. Source: raw `*.response.json` usage in uploaded EXP-1.1 and EXP-1.2 archives.

## Summary table

| Experiment | Model | Convergence C | Logical consistency | Actual USD / 1M total tokens | Average actual USD / answer |
|---|---|---:|---:|---:|---:|
| EXP-0.4 | GPT-4.1 Nano | ~75% | — | — | $0.00104 |
| EXP-0.4 | DeepSeek V3.2 (blocked) | — | — | — | — |
| EXP-0.5 | GLM-4.5 Air | ~88% | — | — | $0.00270 |
| EXP-0.5 | Qwen3 32B | ~73% | — | — | $0.00145 |
| EXP-0.6 | GPT-4.1 Mini (blocked) | — | — | — | — |
| EXP-0.6 | Ministral 8B (overlong) | — | — | — | $0.00139 |
| EXP-0.7 | GPT-4.1 Mini | ~84% | — | — | $0.00611 |
| EXP-0.7 | Nemotron 3 Nano | ~62% | — | — | $0.00110 |
| EXP-1.1 | DeepSeek V3.2 | ~88% | ~82% | $0.28448 | $0.00418 |
| EXP-1.1 | Mistral Small 3.2 | ~73% | ~63% | $0.09961 | $0.00152 |
| EXP-1.2 | GPT-5 Mini | ~87% | ~81% | $0.29799 | $0.00424 |
| EXP-1.2 | Qwen3.5 Plus | ~85% | ~84% | $0.43921 | $0.00687 |

The earlier experiment records available in this review contain answer-level costs but not a verified matching count of billed tokens. Their effective prices remain blank pending raw usage recovery.

## Super-short experiment conclusions

- **EXP-0.4:** Budget blocked DeepSeek; GPT Nano gave partial comparison.
- **EXP-0.5:** GLM was the closer of two tested models to frozen C.
- **EXP-0.6:** Budget and overlength responses prevented acceptance.
- **EXP-0.7:** All four accepted; Nemotron revealed contradictions despite similar decisions.
- **EXP-1.1:** DeepSeek argued more coherently than Mistral; both broke some H1 constraints.
- **EXP-1.2:** Both accepted; GPT-5 Mini was more procedural, Qwen3.5 Plus more concise.

Technical acceptance is not semantic correctness. The model/experiment pair, not the model alone, is the unit of comparison.
