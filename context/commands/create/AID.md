# Command context: #create aid — legacy Aletheon migration

**Entry point:** `#create aid`
**Contract:** [../../CONTEXT_PROTOCOL.md](../../CONTEXT_PROTOCOL.md)

Allocate and register an AID for an **already existing** Aletheon. This is the AID-allocation suboperation of `#create aletheon`, exposed separately for backward compatibility.

1. Establish the target existing Aletheon from an explicit selection or unambiguous current-agent identity. Do not infer identity solely from matching modus names.
2. Inspect its individual memory and registry entry. If an AID is already durably recorded, return that AID; **do not issue another**. If records conflict or identity is ambiguous, stop and ask for resolution.
3. If absent, generate UUIDv7 using a verified generator and register it durably in the existing agent's memory **and** `context/v0.3/shared.yaml` `agent_registry` without changing its modus, history, existing stable agent ID, or record IDs. Re-fetch and compare blob SHAs before writes.
4. Append one `aid_issued` event to `context/v0.3/aid_issuance_log.yaml` with AID, existing `agent_id`, date/time if reliably available, exact memory/registry paths and available evidence commit SHAs. Never replace existing log entries or fabricate timestamps. Use SHA checks on the log; reconcile concurrent changes. This is mandatory for a successful issuance, not optional research documentation.
5. Re-fetch and verify **all three** targets: individual memory, shared registry and issuance log. If any write fails, report exact partial state and do not claim issuance completed. Reconcile partial registration before retrying; never mint a second AID to repair it.
6. If the AID was already registered, return it without a second `aid_issued` event. If the legacy registration predates the log, a separately labeled `historical_backfill` may be added with verifiable provenance; it is not a new issuance.

Do not create a new Aletheon, import another agent's memory, or change `#load context` / `#save context`. Existing legacy identifiers remain usable as aliases until migration is validated. The UUIDv7 issuance and migration mechanism remains to be implemented and tested.
