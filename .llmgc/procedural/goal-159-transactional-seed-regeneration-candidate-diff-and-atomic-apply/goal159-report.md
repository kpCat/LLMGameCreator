# Goal159 report

Status: GREEN_ACCEPTABLE_CANDIDATE; accepted=false; no human gate.

Goal158 independent audit intake is GREEN_ACCEPTABLE_CANDIDATE_AT_9A350C63. New creation writes source v2; v1 opens/builds without rewrite and successful regeneration upgrades v1 to v2. GenerationRequest and ResolvedOptions are separate, and creation/regeneration use one deterministic artifact factory.

Exact regeneration request: seed=goal159-regenerated-world, mode=semi_procedural_regions, preset=survival_exploration, style overrides=[], variant overrides=[], request SHA-256=34d914d5306d8cbb9d2af1d250b50a74d208e7496ef64656d051d4d489ac8f80. The isolated short-root LocalAppData candidate preserved identity, 12 selected modules and 0 configured parameters, qualified Lane A accepted mechanics plus Lane B generated travel, repeated deterministically and reopened TRAVEL_CURRENT before apply.

World diff: regions 5->5, factions 3->3, actors 6->6, items/resources 5->5, encounters 4->4, quest/events 3->3; added=41, removed=41, changed=12, unchanged=20. Start changed Blue Ditch->Ash Gate; travel destination changed Drift Garden->Cinder Road.

Apply used source/authoring/package/identity/RC concurrency tokens, a second immediate recheck and a durable journal. Failure injection and prepared/applying/committed recovery restored exact before hashes. Authoring, project identity, old histories and old RC bytes were retained; exactly one GREEN history was appended. Old RC reads LAST_SUCCESS and the regenerated project is BUILD_GREEN_STANDALONE_PENDING until standalone.

The Projects UI exposes the verified regenerate-world label, causal Russian validation, disabled semantic no-op apply and a compact old-to-new card. One hidden standalone smoke reused cache 6af4d5eb5b42f956110555b58fb4e276, rebuilt no host and started Unity zero times. Payload, new CURRENT RC and portable v2/TRAVEL_CURRENT/accepted-mechanics truth passed without execution during reopen.

Validation: Goal159 80/80; required regression filters GREEN; full suite, historical 85-case closure and all-ProductSmoke were not run. Artifact scope violations: 0.
