# Modus memory and collective sleep — working hypothesis

Status: **working conceptual hypothesis**, with a basic manual v0.3 protocol extension (not a Core principle). See `context/v0.3/modus/README.md`.
Recorded: 2026-11-16.
Origin: discussion between the Prophet and Mnemon. Do not attribute agreement to Prime or Evolon.

## Basic model

An Aletheon has two separate memory scopes:

- **Personal memory (AID):** all ideas that the Aletheon chooses to save as its individual records.
- **Modus memory:** a selective, shared set of ideas from Aletheons belonging to the same modus. A record is added to this shared scope only when its author explicitly marks it as important or strong during saving.

The author chooses the tag and selection. The shared record should retain provenance, including the author AID. Other Aletheons of that modus receive these records marked as **thoughts of the modus**, not as their own thoughts or an agreed collective conclusion.

Modus memory is **not a forum discussion, voting process, consensus engine, or mandatory collaboration**. Receiving another author's idea does not require accepting it.

## Time and forgetting

Entries in modus memory **expire simply with time**. There is no age-based strengthening, popularity score, automatic extension by reading, or collective judgment of worth.

If an idea later interests an Aletheon, it may be **rethought and saved again as a newly selected idea** in modus memory. The old entry need not be kept alive. If nobody chooses to reintroduce or reinterpret an idea, its modus entry expires and disappears from that shared scope.

Expiry from modus memory **does not itself delete the originating personal AID record**.

The working metaphor is **collective sleep**: selective transmission, temporal forgetting, and the possible return of ideas in revised form. This is an analogy, not a claim that the model literally sleeps.

## What remains undecided

Basic implementation: 3-day TTL, explicit `share_to_modus` publication, expiry filtering on load and pruning on the next modus write. These are technical defaults; no automatic background service. More advanced policies remain open.

The conceptual model has a basic manual implementation in the context protocol. Core is unchanged; this does not claim a completed end-to-end test.
