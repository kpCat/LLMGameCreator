# Goal159 transactional seed regeneration candidate diff and atomic apply

Status: `GREEN_ACCEPTABLE_CANDIDATE`

Goal158 independent audit intake is `GREEN_ACCEPTABLE_CANDIDATE_AT_9A350C63`. Goal159 adds backward-compatible generated source v2 with separate `GenerationRequest` and `ResolvedOptions`, truthful preset-definition hash and explicit style/variant overrides. Existing v1 projects open and build without rewrite; successful regeneration upgrades them to v2. New creation and regeneration use the same deterministic artifact factory.

Regeneration rejects a semantic no-op without changing any file. A real candidate is created outside the authoritative project under a short LocalAppData root, retains identity, selected modules and parameter JSON, and qualifies Lane A AcceptedMechanics plus Lane B generated start/travel twice before a fresh `TRAVEL_CURRENT` reopen. The result exposes a typed old/new world diff with data-derived counts.

Apply requires unchanged source, authoring, package, identity and RC tokens at preview and immediately before promotion. A durable journal stages every mutation, rolls failures back to exact before hashes, and recovers nonterminal journals before workspace open. Authoring and identity remain exact; old histories and old RC bytes remain; exactly one GREEN history and one regeneration record are added. The old RC reads `LAST_SUCCESS`, while overall state is `BUILD_GREEN_STANDALONE_PENDING` until the single cached hidden standalone smoke writes a new CURRENT RC. Portable copy restores v2, regeneration, travel, accepted mechanics and RC truth without execution.

The Projects page provides «Перегенерировать мир», seed/mode/preset/advanced override inputs, causal Russian validation, disabled no-op apply and a compact old→new result card. No user action or manual Unity review is requested. `goal159Accepted=false`, `goal159ManualReviewRequired=false`.

Independent audit result: `BLOCKED_AT_C7788E1E`. Blocker: `regeneration_commit_not_sealed_inside_shared_operation_and_semantic_rollback_boundary`. Goal160 closes this commit-boundary P1 with a shared operation lock, immutable cached candidate seal, in-transaction truth/inventory recheck, journal `validating` and semantic rollback/recovery. The Goal159 product foundation—source v1/v2, shared deterministic factory, typed diff, Projects UI and regeneration behavior—remains in use. `goal159IndependentAuditRequired=false`; this does not accept Goal159 and creates no human gate.
