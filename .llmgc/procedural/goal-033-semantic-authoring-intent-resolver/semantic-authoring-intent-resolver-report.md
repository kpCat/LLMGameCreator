# Semantic Authoring Intent Resolver Report

- accepted: false
- finalStatus: semantic_authoring_intent_resolver_verification
- manualGate: semantic_authoring_intent_resolver_verification
- required marker: semantic_authoring_intent_resolver_verification required
- previousProducedGate: dynamic_semantic_feature_system_verification required
- productSmokeRoute: goal-033-semantic-authoring-intent-resolver
- contractProofPassed: true
- workspaceFieldCount: 122
- intentCount: 25
- metamoduleSpeciesArchetypeSlotCount: 112
- invalidMatrixPassed: true
- workspaceSchemaSummaryHash: 3aa4501de1bd564e65f7cbc6e4319f0671ee06743167e6fced60899e1f92056f
- loreSkeletonHash: 04389dc40c5924e3a4119270373e5375041291865201fc580f92dffa402c03dd
- manualMatrixHash: 0d3e2c80b8dce2065e093fa94b3a101b65ecd94d5cd932f12201a3c59cd71429
- invalidMatrixHash: e35332422ce1126679afa1d6f4bfe9a6a683aaa262fe279095e3c0e2fe9a5e4f
- reportHash: 781910d43b9215e720d4deb82a3f16c3e8693352845838e68ed50c8215c22555

## What became more real

Goal 033 adds a deterministic semantic authoring workspace, lore intake skeleton, provenance matrix and feature-driven content-intent resolver over the existing Goal 030-032 semantic stack.

## Scenarios

- caravan_trade: intents=5, summary=caravan_trade|intents=5|families=5|features=5
- frontier_survival: intents=7, summary=frontier_survival|intents=7|families=7|features=10
- gothic_intrigue: intents=5, summary=gothic_intrigue|intents=5|families=5|features=8
- metamodule_kingdoms: intents=8, summary=metamodule_kingdoms|intents=8|families=6|features=13

## Invalid/fake/leak matrix

- conflicting_provenance_for_same_field: expectedValid=false, actualValid=false, codes=semantic_authoring.provenance.unknown
- duplicate_workspace_field_id: expectedValid=false, actualValid=false, codes=semantic_authoring.field_id.duplicate
- fake_intent_target_accepted: expectedValid=false, actualValid=false, codes=semantic_authoring.intent_target.unknown
- final_dialogue_prose_leakage: expectedValid=false, actualValid=false, codes=semantic_authoring.final_prose.leakage
- final_gamepackage_materialization_leakage: expectedValid=false, actualValid=false, codes=semantic_authoring.boundary.leakage
- illegal_feature_domain_applicability: expectedValid=false, actualValid=false, codes=semantic_authoring.feature_domain.illegal
- imported_candidate_treated_as_accepted: expectedValid=false, actualValid=false, codes=semantic_authoring.candidate.not_accepted
- llm_candidate_treated_as_accepted: expectedValid=false, actualValid=false, codes=semantic_authoring.candidate.not_accepted
- missing_source_feature_trace: expectedValid=false, actualValid=false, codes=semantic_authoring.intent_trace.missing
- nondeterministic_ordering_mutation: expectedValid=false, actualValid=false, codes=semantic_authoring.order.nondeterministic
- optional_absent_field_valid: expectedValid=true, actualValid=true, codes=
- required_manual_field_missing: expectedValid=false, actualValid=false, codes=semantic_authoring.required_field.missing
- runtime_ui_unity_provider_llm_rag_lua_media_boundary_leakage: expectedValid=false, actualValid=false, codes=semantic_authoring.boundary.leakage
- unknown_feature_reference: expectedValid=false, actualValid=false, codes=semantic_authoring.feature_ref.unknown
- unknown_target_domain: expectedValid=false, actualValid=false, codes=semantic_authoring.domain.unknown

## Boundaries

- publicGamePackageSchemaChanged: false
- uiChanged: false
- runtimeBehaviorChanged: false
- unityBuildExecuted: false
- llmRagProviderMediaLuaExecuted: false
- finalDialogueProseGenerated: false
- finalGamePackageMaterialized: false

Final dialogue/prose/GamePackage/runtime/UI/Unity/provider/LLM/RAG/Lua/media generation was not performed.
