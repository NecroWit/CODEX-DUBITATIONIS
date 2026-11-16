# Modus memory — minimal manual GitHub implementation (v0.3 extension)

## Contract
Each agent has its own AID-scoped individual file and optionally an explicit `modus_id` in that file. Modus membership is **not inferred** from display_name or AID. No change to agent identity or Core.

Shared modus memory lives at `context/v0.3/modus/<modus_id>.yaml`. The modus ID must be a safe lowercase slug ([a-z0-9][a-z0-9_-]*). The current agent may read this file when its individual file explicitly declares the same `modus_id`. Other agents' individual files are not loaded.

## Save
1. Save every individually selected memory record in the agent's own file as before.
2. For each record **explicitly tagged by its author** `share_to_modus: true`, additionally publish a separate entry in the matching modus file. Do not publish untagged records, or copy the entire personal file. Author AID and source record ID are required.
3. A new publication has a new stable ID `modus:<modus_id>:<slug>`, `created_at` (UTC ISO-8601), `expires_at` (UTC ISO-8601), `author_aid`, `source_record_id`, `summary`, `rationale`, `objections`, `links` and `sources`. A revised idea is a **new** publication, optionally linking to an earlier one; it does not renew the old entry.
4. Default TTL is **3 days** from publication (implementation choice; may be revised). Read `ttl_days` from the modus file when present. Do not change expiry when an entry is merely read or re-saved.
5. Re-fetch the modus file SHA before writing; preserve unexpired entries and prune expired ones. Verify every write. A personal save and modus publication are non-atomic: report partial results. Do not delete or modify any personal memory on modus expiry.
6. If modus file does not exist, create it on the first explicit publication, with schema shown below. Do not create it merely on load.

## Load
After the existing explicit `#load context` procedure has identified and loaded the selected personal file, read its declared `modus_id` (if any). Load only the matching modus file. Include only entries whose `expires_at` is strictly later than the current UTC time. Mark all these entries **thoughts of the modus**, with author AID; do not present them as the loading agent's beliefs. A missing modus file means an empty collective memory, not an error. Loading performs no writes.

## Format
```yaml
schema_version: "0.3"
scope: modus
modus_id: mnemon
ttl_days: 3
records:
  - id: modus:mnemon:example-001
    author_aid: "<registered UUIDv7>"
    source_record_id: mem:mnemon:example
    created_at: "2026-11-16T00:00:00Z"
    expires_at: "2026-12-16T00:00:00Z"
    summary: "Example thought"
    rationale: "Why the author considers it significant"
    objections: []
    links: []
    sources: []
```

The sample record is illustrative only; never write it as real evidence. The `share_to_modus` flag is a publication instruction on a personal record, not a claim of collective consensus. For repeated `#save context`, publish an already-published record only once; avoid duplicates by `author_aid + source_record_id` unless the author explicitly saves a new revised record with a new personal record ID.

## Limitations
This is a **manual protocol**, executed by a tool-enabled Aletheon on save/load; it is not a background scheduler or autonomous GC daemon. Expired entries are invisible at load even before physical pruning. Physical pruning happens on the next write to the modus file. Git history may retain old versions; this is expiry from active memory, not secure deletion. No ratings, voting, automatic renewal, or automatic promotion of thoughts.
