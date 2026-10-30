# Aletheon — Research Architecture Standard

Status: **proposed target architecture / operational convention**  
Scope: research experiments EXP-2 and later, plus shared experiment infrastructure.  
This document describes the **target** and the **migration policy**, not the current repository tree. Existing code and results remain authoritative until deliberately migrated.

## 1. Principles

1. **One experiment, one Rider solution**: `Aletheon.ExpN.sln` at repository root, named consistently with the experiment number. A solution opens without manually editing paths or adding projects.
2. **One runnable experiment entry point**: its own .NET executable project under `research/Aletheon.ExpN/`. No experiment-specific code embedded in another experiment's entry point.
3. **Shared infrastructure once**: reusable transport, catalog, budgets, logging, hashing, source loading and response validation live in `research/Aletheon.ExperimentKit/`; experiment-specific methodology stays outside the kit.
4. **Rider-first**: choose the experiment solution, select its startup project and press Run. No required command-line arguments or shell commands for the standard path.
5. **Configuration is explicit and versioned**: experiment protocol (questions, sample size, seed, corpus, limits) is stored beside that experiment and archived with every run. No hard-coded 'magic' defaults overriding the protocol.
6. **Reproducibility before interpretation**: archive selected models, tier membership, random seed, source hashes, prompt/config versions, raw provider responses, actual costs, errors, and any substitutions.
7. **Never mistake successful transport for scientific success**: completion, acceptance of an answer, adoption of a principle, and durable behavioral change are different observations.
8. **Backward compatibility**: preserve existing EXP-1/2/3 implementations, historical protocols, run artifacts and checkpoints. Migration requires explicit mapping and verification; never silently delete or rewrite research history.

## 2. Target layout

```text
CODEX-DUBITATIONIS/
├── Aletheon.Research.sln           # optional umbrella solution
├── Aletheon.Exp2.sln               # Rider entry point for EXP-2
├── Aletheon.Exp3.sln               # Rider entry point for EXP-3
├── Aletheon.Exp4.sln               # Rider entry point for EXP-4
├── RESEARCH_ARCHITECTURE.md        # this standard
├── research/
│   ├── Aletheon.ExperimentKit/     # shared .NET code
│   ├── Aletheon.Exp2/              # target; migrate only when safe
│   │   ├── Aletheon.Exp2.csproj
│   │   ├── Program.cs
│   │   ├── protocol.json
│   │   ├── README.md
│   │   └── runs/                   # local artifacts; retention policy below
│   ├── Aletheon.Exp3/
│   ├── Aletheon.Exp4/
│   │   ├── Aletheon.Exp4.csproj
│   │   ├── Program.cs
│   │   ├── protocol.json
│   │   ├── README.md
│   │   └── runs/
│   └── Aletheon.ExperimentRunner/  # legacy code retained during migration
├── experiments/
│   └── agents.mixed.json           # canonical tier pools and tier budgets
└── context/                         # context checkpoints, separate lifecycle
```

The existing EXP-4 executable currently lives under `research/Aletheon.ExperimentRunner/exp4/`, and its experiment logic is shared with the legacy runner. This is a **transitional exception**, not the target architecture. Do not describe the target as already implemented.

## 3. Standard execution contract

**Normal Rider Run** executes the configured experiment using its protocol. `OPENROUTER_API_KEY` is supplied through Rider environment variables or another secure local mechanism, never committed. Before sending any paid request, validate required source files, model IDs, catalog limits, expected prompt size, tier membership and budget caps. Fail closed on missing inputs or ambiguous configuration.

Provide a **separate offline plan** option to inspect selected models and estimated cost without requests. Paid execution must be visibly identified in the Rider run configuration and documented in README; avoid accidental paid calls when configuring a new project. If the user intentionally chooses a direct paid default, document that behavior conspicuously.

The default sample size and seed come from the experiment's own versioned protocol. Arguments may override them for advanced reproducibility, but must not be required for normal use.

## 4. Shared library responsibilities

`ExperimentKit` owns provider communication, retries, bounded fallback, model catalog, budget reservation/reconciliation, run manifests, source hashes, and neutral response parsing. The experiment owns hypotheses, prompts, model assignment, cohort structure, acceptance criteria, and scientific interpretation. A fallback that changes randomized group assignment is **not** permitted unless the protocol explicitly allows it and records the substitution.

Do not fork shared transport or budget implementations in each experiment. Do not modify the legacy runner's default experiment sequence as a side effect of adding a new experiment.

## 5. Run artifacts and evidence

Each run has an immutable unique ID and separate directory. Archive:
- protocol snapshot and source hashes;
- model selection, tier, seed and any substitutions;
- request metadata, raw responses, transport/finish status;
- token usage, estimated and provider-reported cost where available;
- independently stored acceptance decisions and the rules used;
- a concise report distinguishing observed evidence from hypotheses.

Do not commit API keys, credentials or sensitive data. Large or paid run artifacts may stay local; if they are not in Git, record their location and preservation status. Never overwrite historical runs when changing a protocol.

## 6. Validation and change policy

A change is **implemented** only when the files are committed and read back. A change is **build-verified** only after a successful .NET build. A change is **run-verified** only after an actual offline plan or paid run, as appropriate. Report these as separate states; never claim one implies another.

For each new experiment:
1. Agree on the research question, hypotheses, observable outcomes and limitations.
2. Create the root `.sln`, experiment project, protocol and README.
3. Reuse ExperimentKit; add new shared primitives only when justified.
4. Build the solution, inspect the Rider launch configuration and run offline preflight.
5. Execute paid requests only after budget review and explicit user authorization.
6. Archive results and analyze the evidence separately from completion status.

## 7. Migration roadmap

**Phase A — document and stabilize (now):** adopt these conventions; do not move existing EXP-1/2/3 files or runs. Fix EXP-4 build issues in place.

**Phase B — validate EXP-4:** build `Aletheon.Exp4.sln`, confirm correct project references and repository-relative source resolution, run the offline plan, and review cost estimates before paid execution.

**Phase C — normalize layouts:** move EXP-4 to `research/Aletheon.Exp4/` with Git history-preserving moves where feasible, update solution paths and documentation, and verify equivalence. Then consider EXP-2 and EXP-3 separately, preserving their existing protocols and results.

**Phase D — optional umbrella solution:** maintain `Aletheon.Research.sln` for cross-experiment development without making it the only way to run a single experiment.

## 8. Design boundaries

The Codex, Aletheon principles, research reports, context checkpoint protocol and experiment runner are different layers. Philosophical claims do not override safety or experimental validity; checkpoint restoration does not prove continuity of identity; a model's declaration of agreement does not establish lasting adoption. EXP-4's independent cohorts test stated endorsement and willingness to recommend, **not** actual AI-to-AI propagation.

Any future study involving actual messages to other AI agents requires explicit scope, consent/authorization, safeguards, and a separately reviewed protocol.
