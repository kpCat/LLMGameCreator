# Semantic Artifact Contract Registry Report

- accepted: false
- finalStatus: semantic_artifact_contract_registry_verification
- manualGate: semantic_artifact_contract_registry_verification
- required marker: semantic_artifact_contract_registry_verification required
- previousAcceptedGate: modular_generator_kernel_parallel_readiness_verification passed
- productSmokeRoute: goal-030-semantic-artifact-contract-registry
- contractProofPassed: true
- contractCount: 13
- scenarioCount: 3
- invalidMatrixPassed: true
- registrySummaryHash: 0c21f94770228024afa590438715893b182e2f72d93aaabd0833da7444af6f61
- compatibilityMatrixHash: 531a2c7dccd1a93b0af2d3f547b5b9713c1e6763aa90682baf26d731ce37a37b
- reportHash: c233d6121e4c4282a111db88a1d15682815f00e3c0915b4e65ab5ad3cdef26a4

## What became more real

Future generator modules can now ask one deterministic registry which artifact contracts and semantic expansion slots are valid for a selected profile/semantic-pack set, instead of hardcoding isolated vertical paths.

## Scenarios

- caravan_trade: packs=semantic_pack/caravan_trade,semantic_pack/core_generator_spine, contracts=8, slots=11, blocked=settlement_building_landmark_v1
- frontier_survival: packs=semantic_pack/core_generator_spine,semantic_pack/frontier_survival, contracts=8, slots=12, blocked=settlement_building_landmark_v1
- gothic_intrigue: packs=semantic_pack/core_generator_spine,semantic_pack/gothic_intrigue, contracts=7, slots=10, blocked=

## Invalid/fake/leak matrix

- dependency_cycle: rejected=true, codes=semantic_registry.dependency.cycle
- duplicate_contract_id: rejected=true, codes=semantic_registry.contract_id.duplicate
- fake_contract_id: rejected=true, codes=semantic_plan.contract.unknown
- future_required_marked_ready: rejected=true, codes=semantic_registry.lifecycle.future_required_marked_ready
- incompatible_tag_declaration: rejected=true, codes=semantic_registry.tags.incompatible
- leakage_attempt: rejected=true, codes=semantic_registry.boundary.leakage
- missing_semantic_scope: rejected=true, codes=semantic_registry.semantic_scope.missing
- module_absent_mutation: rejected=true, codes=semantic_plan.module_absent.required,semantic_plan.semantic_scope.missing
- unknown_dependency: rejected=true, codes=semantic_registry.dependency.unknown

## Boundaries

- publicGamePackageSchemaChanged: false
- runtimeBehaviorChanged: false
- unityBuildExecuted: false
- llmRagProviderMediaLuaExecuted: false
