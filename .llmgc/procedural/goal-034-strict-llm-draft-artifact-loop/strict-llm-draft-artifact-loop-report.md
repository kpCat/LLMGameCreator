# Strict LLM Draft Artifact Loop Report

- accepted: false
- accepted=false
- finalStatus: strict_llm_draft_artifact_loop_verification
- manualGate: strict_llm_draft_artifact_loop_verification
- required marker: strict_llm_draft_artifact_loop_verification required
- productSmokeRoute: goal-034-strict-llm-draft-artifact-loop
- contractProofPassed: true
- familyCount: 9
- requestCount: 143
- candidateCount: 143
- repairRequestCount: 1
- promotionDecisionCount: 14
- promotedDecisions: 12
- repairRequiredDecisions: 1
- rejectedDecisions: 1
- metamoduleSpeciesArchetypeRequestCount: 112
- invalidMatrixPassed: true
- contractSummaryHash: 4fe0a24fc4040a4f75e940e7aef14a817a4a2ab1b3d22554787d61e32c911133
- requestMatrixHash: f0de5fef95910d8ff3136c3a8e4b0aed80dcd8641c36f6fc9463650a32863eb7
- candidateMatrixHash: 0b1a63c331f1a3cd61fab19d08630e9958ac5afff3051f45648c5986615a0607
- repairMatrixHash: be0d0f673252c601df212a3db546e9ef6f532b9d22260ea365f8234edc17f7df
- promotionMatrixHash: 0003e3263c4dc3490d7ed944db47fa482f3b1c317693f9ab30ef31c3100e90cc
- invalidMatrixHash: af61cfcff06a555f7480b741fbaa5ba735667643613d7db4864a583f9377e42f
- reportHash: 338a0a31563ef55fa76ed0c25610596f6300ddc6c3b84a55113089dfbbbc3ba7

## What became more real

Future LLM/manual/import output can only enter the generator as quarantined contract-bound draft candidates, and the program deterministically validates, repairs or rejects them before any promotion.

## Scenarios

- caravan_trade: requests=7, speciesArchetypeSlotRequests=0, summary=caravan_trade|requests=7|families=7|speciesSlots=0
- frontier_survival: requests=9, speciesArchetypeSlotRequests=0, summary=frontier_survival|requests=9|families=9|speciesSlots=0
- gothic_intrigue: requests=7, speciesArchetypeSlotRequests=0, summary=gothic_intrigue|requests=7|families=7|speciesSlots=0
- metamodule_kingdoms: requests=120, speciesArchetypeSlotRequests=112, summary=metamodule_kingdoms|requests=120|families=8|speciesSlots=112

## Invalid/fake/leak matrix

- candidate_self_marked_promoted: expectedValid=false, actualValid=false, codes=strict_draft.candidate.self_promoted
- duplicate_candidate_id: expectedValid=false, actualValid=false, codes=strict_draft.candidate_id.duplicate
- duplicate_request_id: expectedValid=false, actualValid=false, codes=strict_draft.request_id.duplicate
- fake_semantic_scope: expectedValid=false, actualValid=false, codes=strict_draft.semantic_scope.fake
- fake_target_contract: expectedValid=false, actualValid=false, codes=strict_draft.contract.fake
- forbidden_final_prose_field: expectedValid=false, actualValid=false, codes=strict_draft.final_prose.forbidden
- incompatible_scenario_profile: expectedValid=false, actualValid=false, codes=strict_draft.scenario.incompatible
- invalid_repair_target: expectedValid=false, actualValid=false, codes=strict_draft.repair_target.invalid
- missing_contract_trace: expectedValid=false, actualValid=false, codes=strict_draft.contract_trace.missing
- missing_intent_trace: expectedValid=false, actualValid=false, codes=strict_draft.intent_trace.missing
- missing_required_field: expectedValid=false, actualValid=false, codes=strict_draft.required_field.missing
- nondeterministic_ordering_mutation: expectedValid=false, actualValid=false, codes=strict_draft.order.nondeterministic
- over_budget_candidate_count: expectedValid=false, actualValid=false, codes=strict_draft.candidate_count.over_budget
- provider_runtime_ui_unity_lua_gamepackage_code_leakage: expectedValid=false, actualValid=false, codes=strict_draft.boundary.leakage
- repair_attempts_immutable_mutation: expectedValid=false, actualValid=false, codes=strict_draft.repair.immutable_field_mutation
- source_provenance_mismatch: expectedValid=false, actualValid=false, codes=strict_draft.source_kind.mismatch
- unknown_request: expectedValid=false, actualValid=false, codes=strict_draft.request.unknown
- wrong_family: expectedValid=false, actualValid=false, codes=strict_draft.family.wrong

## Boundaries

- providerLlmRagCalled: false
- finalProseGeneratedOrPromoted: false
- gamePackageMaterialized: false
- runtimeUiUnityLuaGeneratorLibraryTouched: false

No provider/LLM/RAG call happened. No final prose was generated or promoted. No GamePackage materialization happened.

strict_llm_draft_artifact_loop_verification required
