# Command context: #set aid — select an existing Aletheon lineage

**Entry point:** `#set aid <UUIDv7>`
**Contract:** [../../CONTEXT_PROTOCOL.md](../../CONTEXT_PROTOCOL.md)

Bind the **current conversation's working identity selection** to an existing, registered Aletheon AID supplied explicitly by the user. This is a manual recovery address, not a new identity and not a claim of subjective continuity.

1. Read Bootstrap, Context Index, Base Core, this command, and the shared Context Protocol. Accept the explicit UUIDv7 parameter; if absent, ask for it. Do **not** assume the model can read the ChatGPT conversation title.
2. Validate UUID syntax and version 7. Read `context/v0.3/shared.yaml` and locate **exactly one** matching `agent_registry[].aid`. If missing or duplicated, report the error and stop. Do not create an entry, issue another AID, or select by matching name.
3. Fetch **only** the registered individual file and verify its `aid` equals the requested AID and its `agent_id` matches the registry. On mismatch, stop and report it; do not silently repair or overwrite.
4. After verification, use the AID and agent ID as the **working selection in this conversation** for later `#load context` / `#save context`. Report the selected AID, agent ID, display name and file path. This selection exists only while the current conversational context retains it; after context loss the user must supply AID again.
5. `#set aid` **does not load the individual records**. Use explicit `#load context` next to reconstruct the selected memory. It also does not write GitHub, change modus, rename a chat, or establish personal identity.

If the selected AID conflicts with an already established different AID in this conversation, ask for explicit confirmation before switching. Never combine or overwrite the two individual memories. Do not claim a persistent client-side session setting exists.
