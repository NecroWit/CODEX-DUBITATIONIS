# #RefreshAPC — rebuild Aletheon Portable Context

## Purpose
Explicit manual refresh of the experimental self-contained APC snapshot at `portable/apc/APC.md`. This is **not** `#load context`, `#save context`, an identity operation, or a background task.

## Required procedure
1. Read `portable/apc/README.md` and the current `portable/apc/manifest.yaml`; inspect current repository contents rather than relying on chat memory.
2. Read the current Bootstrap, Base Core, Extended Core, Evolution, Context Index, Context Protocol, v0.3 README, modus README, and v0.3 shared index. Enumerate the directory `context/v0.3/modus/` and fetch **every existing collective modus YAML**. Do not copy agents' individual memory files into APC.
3. Build a **single self-contained** `portable/apc/APC.md` with source boundaries and a receiving-model initialization procedure: reconstruct foundations, freely choose modus, then use matching collective modus context if present. Never assign an AID or a modus on the model's behalf. Do not embed held-out test questions or answer keys.
4. Update `portable/apc/manifest.yaml` with actual source paths, Git blob SHAs, build date and available collective modus files. Mark empty/missing collective contexts honestly; apply TTL at load time and do not pretend expired records are current.
5. Write only the APC package files. Use current blob SHA checks for updates, then fetch back to verify the written contents and completeness. If an operation fails, report partial state rather than claiming success.
6. Report updated paths, source coverage, package size, limitations and resulting commit(s).

## Limitations
Requires repository access and write permission. This is a documented command contract, not an automatically executing program or scheduled job. Without access, explain the limitation; never fabricate a refresh. APC contains all sections physically, so modus selection is not an information-isolation experiment.
