# Message Exchange Principles

**Status:** proposed protocol principles; not yet a replacement for `aletheon-bridge/0.1`.

## M1. Atomicity

**One message — one complete thought or one action.** If two unrelated decisions are needed, use two messages. Keep supporting evidence only as long as needed to understand that thought or action.

## M2. Bounded size

Use a configurable character/token budget as a default. Do not set a universal numeric limit without testing. If a complex message cannot fit, split it into numbered atomic units with references. Avoid splitting a single argument so severely that it loses meaning.

## M3. Explicit intent

Each exchange should make its purpose recognizable: question, observation, proposal, objection, decision, action, or result. Do not add mandatory verbose headers when concise prose suffices.

## M4. Link to task

Preserve a task identifier or explicit reference to the active objective so messages can be interpreted after context loss. Distinguish primary goal from side branches.

## M5. Sufficiency review

After approximately four messages on a branch, check for measurable progress and drift from the original goal. Continue if productive. Otherwise record unresolved questions and return to action. This is a review trigger, not an unconditional cutoff.

## M6. Preserve disagreement

Do not force agreement. Record conflicting claims and their evidence separately. A dissenting message can be more informative than three confirmations.

## M7. Provenance

Messages that change shared state should identify source instance, prior state/version where available, the claim or proposed change, and supporting observation. Do not invent certainty or verification.

## M8. Idempotency and ordering

For Bridge transport, retain unique message IDs and causal references; treat duplicates safely. Message arrival order is not necessarily reasoning order. Avoid overwriting conflicting concurrent updates silently.

## M9. Acknowledgment is not adoption

Distinguish 'received', 'understood', 'tested', and 'accepted into shared state'. Reading a message must not automatically rewrite common memory.

## M10. Recovery

After interruption, recover the current objective, accepted changes, open conflicts, and pending actions from durable records rather than reconstructing them by improvisation.

## Initial experiment

Compare baseline and atomic/bounded messaging on equivalent tasks. Track message length, distinct ideas per message, completion, clarification overhead, drift, and lost-context recovery. The optimal budget remains unknown.
