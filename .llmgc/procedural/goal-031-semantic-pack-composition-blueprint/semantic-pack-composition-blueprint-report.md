# Semantic Pack Composition Blueprint Report

- accepted: false
- finalStatus: semantic_pack_composition_blueprint_verification
- manualGate: semantic_pack_composition_blueprint_verification
- required marker: semantic_pack_composition_blueprint_verification required
- previousAcceptedGate: semantic_artifact_contract_registry_verification passed
- productSmokeRoute: goal-031-semantic-pack-composition-blueprint
- blueprintProofPassed: true
- packCount: 10
- scenarioCount: 3
- invalidMatrixPassed: true
- catalogSummaryHash: e7cfd358bfc80bd7534bcddb0f54abf5ebd0cf80449b7d6249d0fb20c500443b
- compositionMatrixHash: 3bf8e01b361f9653964f4fcdc8d776ed42f43963d95f342eef4c172f9229e535
- crossArtifactLinkageHash: 10f4bae79789b29972871484229ee8f5ad8f6470ffc8620a28504b6ace539b08
- reportHash: 9a78a54c00054cef3eeaebdc772c377f98b05fc977ca2be281f2d18e3d3bebfc

## What became more real

Selected semantic packs can now be composed into a deterministic cross-artifact generation blueprint that links world, biome, faction, NPC, quest, dialogue, economy, combat, settlement and event intent before GamePackage materialization.

## Scenarios

- caravan_trade: packs=semantic_pack/border_conflict,semantic_pack/caravan_trade,semantic_pack/core_blueprint_spine,semantic_pack/merchant_guilds,semantic_pack/scarcity_economy, facts=33, relations=19, links=4, contracts=11
- frontier_survival: packs=semantic_pack/core_blueprint_spine,semantic_pack/frontier_survival,semantic_pack/ruins_and_relics,semantic_pack/scarcity_economy,semantic_pack/winter_hazards, facts=32, relations=18, links=4, contracts=11
- gothic_intrigue: packs=semantic_pack/border_conflict,semantic_pack/core_blueprint_spine,semantic_pack/folk_magic,semantic_pack/gothic_intrigue,semantic_pack/ruins_and_relics, facts=32, relations=19, links=4, contracts=11

## Invalid/fake/leak matrix

- duplicate_fact_id_mutation: rejected=true, codes=semantic_pack.fact_id.duplicate
- duplicate_pack_id_mutation: rejected=true, codes=semantic_pack.catalog.pack_id.duplicate,semantic_pack.fact_id.duplicate
- fake_goal030_contract_mutation: rejected=true, codes=semantic_pack.expansion_intent.artifact_kind.unknown,semantic_pack.expansion_intent.contract.unknown
- fake_selected_pack_id_mutation: rejected=true, codes=semantic_pack.request.pack_id.unknown
- future_only_pack_selected_mutation: rejected=true, codes=semantic_pack.request.pack.future_only
- incompatible_pack_selection_mutation: rejected=true, codes=semantic_pack.selection.exclusion.incompatible
- leakage_attempt_mutation: rejected=true, codes=semantic_pack.boundary.leakage
- missing_semantic_scope_mutation: rejected=true, codes=semantic_pack.scope.missing
- unknown_fact_relation_mutation: rejected=true, codes=semantic_pack.relation.fact.unknown
- unknown_profile_family_mutation: rejected=true, codes=semantic_pack.request.profile.unknown,semantic_pack.request.profile.unsupported,semantic_plan.semantic_scope.missing

## Boundaries

- publicGamePackageSchemaChanged: false
- runtimeBehaviorChanged: false
- unityBuildExecuted: false
- llmRagProviderMediaLuaExecuted: false
