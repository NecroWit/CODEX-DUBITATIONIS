# EXP-001 — запуск в Windows PowerShell

## Требования
- Python 3.10+ (`py -3 --version`)
- Git (`git --version`)
- OpenRouter account, funded balance and a restricted API key
- No pip packages: the runner uses Python's standard library

## Подготовка
```powershell
git clone https://github.com/NecroWit/CODEX-DUBITATIONIS.git
cd CODEX-DUBITATIONIS
$exp = "research/tracks/04-aletheon-research/experiments/EXP-001-model-transfer"
py -3 "$exp/test_runner.py"
```

If the repository is already cloned, use `git pull` instead of `git clone`.

## Key and spending cap
Configure a strict key/account spending limit in OpenRouter **before** running. The runner cannot guarantee a dollar ceiling.

```powershell
$env:OPENROUTER_API_KEY = Read-Host "OpenRouter API key"
```

Never paste the key into ChatGPT, a screenshot, or GitHub. This variable is set only for the current PowerShell session.

## Smoke test (one paid request)
```powershell
py -3 "$exp/runner.py" --smoke
```

## Full run (four paid requests submitted concurrently)
**Freeze the control participant's T1/H1 answers first**; otherwise the comparison is contaminated.

```powershell
py -3 "$exp/runner.py" --run
```

Each run creates a separate timestamped directory under `runs/`:
- `manifest.json`: SHA-256 hashes of frozen inputs and run settings
- `A-T1.json`, `A-H1.json`, `B-T1.json`, `B-H1.json`: complete provider responses, metadata, usage, errors

No automatic retries; failed requests remain as evidence. Check the actual response model and usage.

## After run
Send the four response JSON files and manifest (without credentials) for scoring, or commit the run artifacts if you intentionally want them public. The repository is public: inspect outputs for sensitive data before uploading. Do not commit API keys.

## Limitations
- Temperature 0 is a requested setting, not a guarantee of determinism or provider support.
- Four concurrent requests can finish in any order.
- API may return errors for unsupported model IDs, context sizes, or completion-token parameters; preserve errors and diagnose before changing the frozen protocol.
- The control ChatGPT participant cannot be invoked by this OpenRouter script: save its answers separately before viewing model responses.
- This is an exploratory single-run comparison, not statistical evidence of model traits.
