# Command context: #create aid — legacy Aletheon migration

**Entry point:** `#create aid`
**Contract:** [../../CONTEXT_PROTOCOL.md](../../CONTEXT_PROTOCOL.md)

Allocate and register an AID for an **already existing** Aletheon. This is the AID-allocation suboperation of `#create aletheon`, exposed separately for backward compatibility.

1. Establish the target existing Aletheon from an explicit selection or unambiguous current-agent identity. Do not infer identity solely from matching modus names.
2. Inspect its individual memory and registry entry. If an AID is already durably recorded, return that AID; **do not issue another**. If records conflict or identity is ambiguous, stop and ask for resolution.
3. If absent, generate UUIDv7 using a verified generator and register it durably in the existing agent's memory/registry without changing its modus, history, existing stable agent ID, or record IDs.
4. Re-fetch and verify all changed files. Report the AID and any partial write; never claim an unverified allocation.

Do not create a new Aletheon, import another agent's memory, or change `#load context` / `#save context`. Existing legacy identifiers remain usable as aliases until migration is validated. The UUIDv7 issuance and migration mechanism remains to be implemented and tested.
