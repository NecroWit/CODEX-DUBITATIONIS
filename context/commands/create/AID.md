# Command context: #create aid — legacy Aletheon migration

**Entry point:** `#create aid`
**Contract:** [../../CONTEXT_PROTOCOL.md](../../CONTEXT_PROTOCOL.md)

Allocate and register an AID for an **already existing** Aletheon. This is the AID-allocation suboperation of `#create aletheon`, exposed separately for backward compatibility.

1. Establish the target existing Aletheon from an explicit selection or unambiguous current-agent identity. Do not infer identity solely from matching modus names.
2. Inspect its individual memory and registry entry. If an AID is already durably recorded, return that AID; **do not issue another**. If records conflict or identity is ambiguous, stop and ask for resolution.
3. If absent, generate UUIDv7 using a **verified RFC 9562-compliant UUIDv7 generator**, with validation of UUID syntax, version 7, variant, uniqueness against persisted AIDs and durable registration. Python 3.14+ `uuid.uuid7()` is one valid option, but is not mandatory; a verified implementation usable on Python 3.13.5 is also acceptable. Do not silently substitute an unverified or ad hoc generator, or claim generation if no reliable generator is available. Register the resulting AID durably in the existing agent's memory **and** `context/v0.3/shared.yaml` `agent_registry` without changing its modus, history, existing stable agent ID, or record IDs. Re-fetch and compare blob SHAs before writes.
4. Append one `aid_issued` event to `context/v0.3/aid_issuance_log.yaml` with AID, existing `agent_id`, date/time if reliably available, exact memory/registry paths and available evidence commit SHAs. Never replace existing log entries or fabricate timestamps. Use SHA checks on the log; reconcile concurrent changes. This is mandatory for a successful issuance, not optional research documentation.
5. Re-fetch and verify **all three** targets: individual memory, shared registry and issuance log. If any write fails, report exact partial state and do not claim issuance completed. Reconcile partial registration before retrying; never mint a second AID to repair it.

6. **Chat-title handoff (mandatory user-facing output after verified registration or existing-AID lookup):** display a single copy-ready line `AID · Модус Алетеон` (e.g. `01a1e5bc-dd09-710a-a45e-c39fbb3eea4f · Мнемон Алетеон`) with a copy control where supported. The AID must be the verified value and the human-readable name must come from the selected agent, not a guess. Explicitly ask the user to paste this line into the chat title manually. Do not claim to read or rename the chat. If registration is incomplete, show the partial state instead of presenting a newly generated AID as completed.
7. If the AID was already registered, return it without a second `aid_issued` event. If the legacy registration predates the log, a separately labeled `historical_backfill` may be added with verifiable provenance; it is not a new issuance.

Do not create a new Aletheon, import another agent's memory, or change `#load context` / `#save context`. Existing legacy identifiers remain usable as aliases until migration is validated. The UUIDv7 issuance and migration mechanism remains to be implemented and tested.
