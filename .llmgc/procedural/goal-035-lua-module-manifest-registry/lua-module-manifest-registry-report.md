# Lua Module Manifest Registry Report

- accepted: false
- accepted=false
- finalStatus: lua_module_manifest_registry_verification
- manualGate: lua_module_manifest_registry_verification
- required marker: lua_module_manifest_registry_verification required
- productSmokeRoute: goal-035-lua-module-manifest-registry
- contractProofPassed: true
- familyCount: 11
- hostApiGroupCount: 21
- manifestCount: 133
- selectedScenarioCount: 4
- metamoduleSpeciesArchetypeSlotManifestCount: 112
- invalidMatrixPassed: true
- registrySummaryHash: 16ed017207819973f236c02b28eaf97985f9a6023669baae9d846834f9b3a52d
- hostApiPolicyHash: bcb846d02bb47b4b3047982681be032c4998974dae3183c6491c26a180b64748
- dependencyPlanHash: 524ee9c884c91feaaf33798f7db7dc68dcd8e2d7fcc853e1b6f61d933f7e1860
- invalidMatrixHash: 0334cc30d59605bc7236373319617d22e1bab122b1c48f936530ba35632028b4
- reportHash: 6c1ceba4a029daa415c39a105c308d6f633fd23d391a20d17af52b86c6d36880

## What became more real

Future Lua/manual/import/LLM module output can only become selectable through deterministic manifest records, host API surface policy, dependency planning, provenance checks and invalid/fake/leak diagnostics before any executor is allowed.

## Scenarios

- caravan_trade: selected=4, blocked=0, futureRequired=1, missingDependencies=0, summary=caravan_trade|selected=4|blocked=0|future=1|missing=0|order=4
- frontier_survival: selected=6, blocked=0, futureRequired=0, missingDependencies=0, summary=frontier_survival|selected=6|blocked=0|future=0|missing=0|order=6
- gothic_intrigue: selected=5, blocked=0, futureRequired=0, missingDependencies=0, summary=gothic_intrigue|selected=5|blocked=0|future=0|missing=0|order=5
- metamodule_kingdoms: selected=116, blocked=1, futureRequired=0, missingDependencies=0, summary=metamodule_kingdoms|selected=116|blocked=1|future=0|missing=0|order=116

## Host API denied groups

- arbitrary_code_generation
- filesystem
- gamepackage_schema_mutation
- implicit_lua_execution
- network
- os_process
- provider_llm_rag
- reflection
- runtime_direct_mutation
- ui_winforms
- unity_direct_call

## Invalid/fake/leak matrix

- denied_host_api_group_allowed: expectedValid=false, actualValid=false, codes=lua_manifest.host_api.denied_allowed,lua_manifest.side_effect.mismatch
- dependency_cycle: expectedValid=false, actualValid=false, codes=lua_manifest.dependency.cycle
- duplicate_family_id_conflict: expectedValid=false, actualValid=false, codes=lua_manifest.family_id.duplicate_conflict
- duplicate_module_id: expectedValid=false, actualValid=false, codes=lua_manifest.module_id.duplicate
- fake_profile_scenario: expectedValid=false, actualValid=false, codes=lua_manifest.profile.fake,lua_manifest.scenario.fake
- final_prose_content: expectedValid=false, actualValid=false, codes=lua_manifest.final_prose.forbidden
- future_required_treated_ready: expectedValid=false, actualValid=false, codes=lua_manifest.future_required.treated_ready
- invalid_module_id: expectedValid=false, actualValid=false, codes=lua_manifest.module_id.invalid
- lua_source_execution_claim: expectedValid=false, actualValid=false, codes=lua_manifest.lua_source_or_execution.forbidden
- missing_required_semantic_scope: expectedValid=false, actualValid=false, codes=lua_manifest.semantic_scope.required_missing
- nondeterministic_ordering_mutation: expectedValid=false, actualValid=false, codes=lua_manifest.dependency.unknown,lua_manifest.order.nondeterministic
- over_budget_module: expectedValid=false, actualValid=false, codes=lua_manifest.resource_budget.over_limit
- provenance_mismatch: expectedValid=false, actualValid=false, codes=lua_manifest.provenance.mismatch
- provider_llm_rag_leak: expectedValid=false, actualValid=false, codes=lua_manifest.provider_llm_rag.leakage
- quarantined_candidate_marked_ready: expectedValid=false, actualValid=false, codes=lua_manifest.candidate.ready_without_review
- runtime_ui_unity_gamepackage_leak: expectedValid=false, actualValid=false, codes=lua_manifest.host_api.denied_allowed,lua_manifest.runtime_ui_unity_gamepackage.leakage,lua_manifest.side_effect.mismatch
- side_effect_class_mismatch: expectedValid=false, actualValid=false, codes=lua_manifest.side_effect.mismatch
- unknown_artifact_contract_reference: expectedValid=false, actualValid=false, codes=lua_manifest.artifact_contract.unknown
- unknown_dependency: expectedValid=false, actualValid=false, codes=lua_manifest.dependency.unknown
- unknown_host_api_group: expectedValid=false, actualValid=false, codes=lua_manifest.host_api.unknown
- unknown_intent_family_reference: expectedValid=false, actualValid=false, codes=lua_manifest.intent_family.unknown

## Boundaries

- noLuaExecutionOrParsing: true
- noLuaSourceGenerated: true
- noProviderLlmRagCallHappened: true
- noRuntimeUiUnityGamePackageMutation: true

No Lua execution or parsing happened. No Lua source was generated. No provider/LLM/RAG call happened. No Runtime/UI/Unity/GamePackage schema mutation happened.

lua_module_manifest_registry_verification required
