# Goal169B package-bound event proof, nested combat replay and payload closure

Status: `BLOCKED_AFTER_SINGLE_CACHED_SMOKE_PAYLOAD_SELF_CHECK`.

Goal169B is a focused continuation of Goal169A, not a new product slice. The Goal169A independent audit result is retained as `BLOCKED_AT_D012B8AC`; all six findings are `closed_by_goal169b`.

Nested exact combat now records actual command and Runtime event sequences, qualified descriptor and ability authority, observed effect fingerprints, before/after encounter state, turn/round progress and outcome. Both resolution replays compare the complete chain. Equal final state with changed command order, ability, utility no-op, Runtime event, effect or turn chain is rejected.

Binding, overlay inventory, summary inventory, qualification, signature and frame ownership use exact event ID sets. History, regeneration and save selection recompute dialogue, interaction, entity prototype, map entity, position, references, requirements/effects and metadata from the actual loaded package. The absent profile requires an exactly empty overlay/inventory/package event graph.

Migration requires exact marker, prototype, dialogue and interaction definitions under `EXACT_PLACEMENT_REQUIRED`. The public persisted save schema remains unchanged. Standalone payload frames carry versioned event/route/replay/sequence/command identity and a strict authority with six event IDs and 24 signatures.

Exactly one cached hidden smoke was invoked. It reused the host, rebuilt nothing and started Unity zero times, but stopped before launch/publication when the legacy parser rejected the multiline JSON authority fact as `standalone.payload.human_facts_parse_mismatch`. Consequently no Goal169B immutable pointer/run status, RC or portable proof was published. The diagnosed correction stores authority as a legacy-safe Base64 fact and places versioned frame identity in the serialized title; 71/71 focused non-smoke tests, including the real 13/13 structural plus legacy-parser payload self-check, pass after this correction. Retry remains 0, so the fix was not re-smoked. Goal169 and Goal169A outputs remain byte-identical.

Goal169, Goal169A and Goal169B are `accepted=false`. Goal169B creates no human gate, requests no manual testing and requires independent blocker audit/follow-up without retrying the consumed smoke.

Goal169B independent audit is recorded as `BLOCKED_AT_91BEF55B`; its code findings are closed. Goal169C is the separate post-fix publication/qualification continuation. It must not rewrite or reinterpret the consumed Goal169B smoke or its byte-identical forensics.
