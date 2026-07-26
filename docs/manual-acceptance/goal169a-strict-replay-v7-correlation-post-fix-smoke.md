# Goal169A strict replay v7 correlation and post-fix smoke closure

Status: `GREEN_ACCEPTABLE_CANDIDATE`.

Goal169A is a continuation of the honestly published Goal169 result `BLOCKED_AT_F861229C`, not a new product slice. The original Goal169 run, pointer/history/payload/RC evidence remains immutable and retains standalone GREEN, 5/5 self-checks, 0 explicit `Move.*` frames and 84 direction-only frames.

Every regional event now has two independent fresh-start `LOCKED_PROBE` routes and two independent fresh-start `RESOLUTION` routes. Replay signatures compare exact commands, Runtime events, status transitions, before/after hashes, available choices, reputation, resolution and relationship flags, quest truth, encounter truth and final state. Equal final hashes with divergent frames, events or choices are rejected.

History v7 recomputes summary, overlay and inventory hashes and enforces exact one-to-one correlation from available relationship branches through event kind, binding, inventory, qualification and runtime frames. It also requires `ArcQuestCount == QualifiedArcQuestCount`. Challenge region derives from exact generated encounter provenance when available, uses relationship-home fallback only when provenance is absent, and rejects ambiguity or mismatch.

Event inventory includes actor/faction/prototype/map entity/interaction/source quest/challenge encounter identity plus prerequisite, reward, target-region and semantic fingerprints. Save migration consumes exact source and target selected v7 inventories and definition fingerprints, exposes typed compatible/preserve/reset/drop facts at application preview/result level, leaves the public persisted save schema unchanged and creates no ghost event state.

Exactly one separate Goal169A cached hidden smoke proves explicit `Move.*` output and zero bare direction frames. It reuses the cached host, rebuilds nothing, starts Unity zero times and receives zero retry. RC and portable all-selectable are current; portable core-only does not claim false RC readiness.

Goal169 and Goal169A are `accepted=false`. Goal169A creates no human gate and requires independent audit. No manual testing is requested.

Independent audit result at `d012b8ac40a9c6ded421ec4bbcbddd9cc3b8d385`: `BLOCKED_AT_D012B8AC`. The six remaining proof-boundary findings—nested synthetic combat, non-exact ID ownership, SHA-only package trust, incomplete prototype/map-entity migration, incomplete payload identity and non-sealed absent truth—are closed by Goal169B. This Goal169A record and its run/pointer/history/payload/RC remain historical and byte-identical.
