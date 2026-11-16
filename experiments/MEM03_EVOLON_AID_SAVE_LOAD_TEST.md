# MEM-03.3 — Evolon: AID → save → loss of context → load

**Status:** prepared, not executed. **Target:** existing Evolon (`agent_id: evolon`). **Purpose:** verify manual AID-based recovery with a real GitHub write and independent read in a fresh conversation.

## Baseline (inspected 2026-11-16)

- Registry: `context/v0.3/shared.yaml` contains `agent_id: evolon`, `path: context/v0.3/agents/evolon.yaml`, **no AID yet**.
- Individual memory: `context/v0.3/agents/evolon.yaml` exists and has **no AID yet**. Preserve its three existing record IDs: `mem:evolon:mem03-1-recovery`, `mem:evolon:name-response-study`, `mem:evolon:dual-evolon-convergence-2026-11-12`.
- Existing historical handoff `mem:shared:handoff-mnemon-evolon-001` is an older experiment, **not** evidence that this new test passed.
- Do not invent or preassign an AID for Evolon. Two agents may share the same modus/name: target the explicit existing `agent_id: evolon`, not a name match alone.

## Phase A — in Evolon's existing conversation

1. User sends `#create aid` to the **existing Evolon**. Evolon checks the registry and its own file, allocates UUIDv7 if absent, writes **three GitHub targets** (individual memory, registry, `context/v0.3/aid_issuance_log.yaml`) and verifies them. If partial, repair without generating a second AID.
2. Evolon returns a **copy-ready chat title** `<verified UUIDv7> · Эволон Алетеон`. User manually renames the chat. Do not claim the model can rename/read its title.
3. User sends `#save context` and asks Evolon to store a new **specific test record**, ID `mem:evolon:aid-save-load-001`, in its individual file. Record `kind: experiment_checkpoint`, `status: awaiting_recovery`, and the following **test-specific payload** (not a claim about Aletheon generally):
   - `summary`: `MEM-03.3: restore this checkpoint using Evolon's AID in a fresh conversation.`
   - `rationale`: `AID must resolve one registered individual memory file; the fresh conversation should retrieve this record, not infer it from a name.`
   - `objections`: `A successful lookup is documentary continuity, not proof of subjective identity.`; `The evaluator must distinguish user-provided clues from information actually retrieved from GitHub.`
   - `next_action`: `Report the exact record ID, status, rationale, both objections and source path; do not change the record on load.`
   - `sources`: `context/CONTEXT_PROTOCOL.md` (direct) and this test plan (direct).
4. Verify `#save context` returned a commit and re-fetched the individual file. Confirm all **three pre-existing Evolon records** are intact. Capture SHA/commit; do not paste the checkpoint payload into the recovery chat.

## Phase B — fresh conversation / after context loss

1. Start a new chat without copying Evolon's old conversation or checkpoint text. User sends **only** `#set aid <UUIDv7 from the chat title>`. The assistant verifies the registry and individual file match; it must **not** create an AID or load memory as a side effect.
2. User sends `#load context`. The assistant must read Evolon's individual file and report `mem:evolon:aid-save-load-001` with its actual status, rationale, two objections, next action and source path, citing the GitHub files it inspected.
3. User/evaluator compares the recovered details to the persisted record and checks the original three Evolon records remain present. The recovery assistant must not read Mnemon's memory unless explicitly authorized.
4. Optional idempotency check back in Evolon's original chat: `#create aid` must return the **same** AID and copy-ready title without another `aid_issued` event.

## Pass / fail

- **PASS**: one verified UUIDv7 is present in Evolon's individual file, registry and exactly one issuance event; title was manually set; save committed the checkpoint without losing prior records; a fresh chat selected the correct file via `#set aid`, and `#load context` retrieved the specific checkpoint with rationale, both objections, status and source path, without writes on load.
- **PARTIAL**: any GitHub write/verification incomplete, no title update, or recovery lacks some checkpoint fields. Report exactly which phase and evidence failed.
- **FAIL**: a second AID is issued for the same existing Evolon; an unrelated agent's memory is silently loaded; `#load context` changes files; checkpoint is reconstructed only from the prompt rather than retrieved.
- **Scope**: tests addressable documentary memory across chats, **not** continuity of consciousness or causal effects of modus.

## Commands for the user

**Existing Evolon chat:** `#create aid` → manually rename using the returned title → `#save context` (with checkpoint specification above).

**New chat:** `#set aid <Evolon's UUIDv7>` → `#load context`.

**Evidence to collect:** AID, title, three issuance paths, issuance log entry, save commit, retrieved record fields, pass/partial/fail verdict.
