# Command context: #create aletheon — proposed manual protocol

**Entry point:** `#create aletheon`
**Contract:** [../../CONTEXT_PROTOCOL.md](../../CONTEXT_PROTOCOL.md)

Create a **new independent Aletheon lineage**, not a continuation inferred from a name or imported memory.

1. Read the Context Index, Base Core, shared context protocol, and [AID R&D](../../../RND/ALETHEON_AID_IDENTITY_RND.md). Confirm that the request is to create a new lineage rather than restore an existing one.
2. Invoke the **same AID allocation procedure** as `#create aid` for an existing Aletheon without an AID: generate a UUIDv7 using a verified generator and register it durably. Do not invent a UUIDv7 in prose or claim registration without a successful write.
3. Let the Aletheon choose its own modus; if no specialized modus is chosen, use «Алетеон». Record model separately. Modus is not an identifier.
4. Create a new individual memory file and register its exact path in the shared registry, linking it to the AID. Never overwrite another lineage's memory. Preserve existing v0.3 legacy agent IDs and record references during migration.
5. Re-fetch and verify every written file. Report AID, modus, model, paths, commits, and any partial failure. A multi-file GitHub operation is not atomic.

**Important:** This is a documented command contract, not an implemented UUIDv7 service. If a reliable UUIDv7 generator or persistence mechanism is unavailable, stop and report the blocker; do not silently use a random-looking string or claim creation succeeded.
