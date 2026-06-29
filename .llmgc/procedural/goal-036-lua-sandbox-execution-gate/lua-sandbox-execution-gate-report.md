# Lua Sandbox Execution Gate Report

- accepted: false
- accepted=false
- finalStatus: lua_sandbox_execution_gate_verification
- manualGate: lua_sandbox_execution_gate_verification
- required marker: lua_sandbox_execution_gate_verification required
- luaExecuted=false
- productSmokeRoute: goal-036-lua-sandbox-execution-gate
- contractProofPassed: true
- requestCount: 4
- decisionCount: 4
- traceCount: 4
- repairPlanCount: 25
- invalidScenarioCount: 24
- metamoduleSpeciesArchetypeSlotManifestCount: 112
- policySummaryHash: 38a3d618b8c8cc60aa56801be46b6ac41fd09ac52351d8f3568837abf8c94eb8
- hostBindingMatrixHash: e03d4b5ad385e7f2bbeb66e68158547f1b078dcc4e0bbfc75a6eca9a6ccf42ac
- requestMatrixHash: d2976c9353065851eed2038df49e4934f8d9a00302cdd45e813d2b24f11baebb
- dryRunTraceMatrixHash: 4ddb837997b63bab9c56245b5e6434a3bfc2bdf595b06401c0b42aa9b4d9f777
- repairPlanMatrixHash: d8d4bb1f5999c6ea6a0a05b6c82108fa039479d8ce88f838a938067d0b2f4ec8
- invalidMatrixHash: b4a113f100244708b9b78ce596910c6e4cc3c3e642264edcc69ef137c778d2f9
- reportHash: fefa30dc32ae4e63c8bf0e7107d13e5cb30f3864e887f44ac0eee48c673d5650

## What became more real

Goal 035 manifest selections now pass through an Application-layer deny-first sandbox execution gate with budget, determinism, host binding, dry-run trace and repair evidence before any future executor adapter can be considered.

## Scenario decisions

- caravan_trade: status=dry_run_only, selected=4, metamoduleSlots=0, luaExecuted=false, summary=caravan_trade|status=dry_run_only|selected=4|bindings=5|diagnostics=0|luaExecuted=false
- frontier_survival: status=dry_run_only, selected=6, metamoduleSlots=0, luaExecuted=false, summary=frontier_survival|status=dry_run_only|selected=6|bindings=8|diagnostics=0|luaExecuted=false
- gothic_intrigue: status=ready_for_future_executor, selected=5, metamoduleSlots=0, luaExecuted=false, summary=gothic_intrigue|status=ready_for_future_executor|selected=5|bindings=6|diagnostics=0|luaExecuted=false
- metamodule_kingdoms: status=blocked_no_executor, selected=116, metamoduleSlots=112, luaExecuted=false, summary=metamodule_kingdoms|status=blocked_no_executor|selected=116|bindings=6|diagnostics=1|luaExecuted=false

## Invalid/fake/leak matrix

- denied_host_api_group: expectedStatus=needs_repair, actualStatus=needs_repair, codes=lua_sandbox.host_api.denied
- fake_manifest_id: expectedStatus=rejected, actualStatus=rejected, codes=lua_sandbox.dependency_order.unstable,lua_sandbox.manifest_id.fake
- filesystem_leak: expectedStatus=needs_repair, actualStatus=needs_repair, codes=lua_sandbox.host_api.denied
- final_prose_included: expectedStatus=rejected, actualStatus=rejected, codes=lua_sandbox.final_prose.forbidden
- immutable_repair_mutation: expectedStatus=rejected, actualStatus=rejected, codes=lua_sandbox.repair.immutable_manifest_mutation
- lua_execution_claim_included: expectedStatus=rejected, actualStatus=rejected, codes=lua_sandbox.lua_execution_claim.forbidden
- missing_budget: expectedStatus=needs_repair, actualStatus=needs_repair, codes=lua_sandbox.budget.missing
- missing_goal034_promotion_trace: expectedStatus=rejected, actualStatus=rejected, codes=lua_sandbox.promotion_trace.missing
- native_interop_leak: expectedStatus=needs_repair, actualStatus=needs_repair, codes=lua_sandbox.host_api.denied
- network_leak: expectedStatus=needs_repair, actualStatus=needs_repair, codes=lua_sandbox.host_api.denied
- nondeterministic_ordering: expectedStatus=rejected, actualStatus=rejected, codes=lua_sandbox.manifest_order.nondeterministic
- over_budget: expectedStatus=needs_repair, actualStatus=needs_repair, codes=lua_sandbox.budget.over_limit
- parser_claim_included: expectedStatus=rejected, actualStatus=rejected, codes=lua_sandbox.parser_claim.forbidden
- process_leak: expectedStatus=needs_repair, actualStatus=needs_repair, codes=lua_sandbox.host_api.denied
- provider_llm_rag_leak: expectedStatus=rejected, actualStatus=rejected, codes=lua_sandbox.host_api.boundary_blocked
- random_leak: expectedStatus=needs_repair, actualStatus=needs_repair, codes=lua_sandbox.host_api.denied
- reflection_leak: expectedStatus=needs_repair, actualStatus=needs_repair, codes=lua_sandbox.host_api.denied
- runtime_ui_unity_gamepackage_schema_mutation_leak: expectedStatus=rejected, actualStatus=rejected, codes=lua_sandbox.host_api.boundary_blocked,lua_sandbox.host_api.denied
- self_promotion: expectedStatus=rejected, actualStatus=rejected, codes=lua_sandbox.promotion.self_forbidden
- source_text_included: expectedStatus=rejected, actualStatus=rejected, codes=lua_sandbox.source_text.forbidden
- threading_leak: expectedStatus=needs_repair, actualStatus=needs_repair, codes=lua_sandbox.host_api.denied
- time_leak: expectedStatus=needs_repair, actualStatus=needs_repair, codes=lua_sandbox.host_api.denied
- unknown_host_api_group: expectedStatus=rejected, actualStatus=rejected, codes=lua_sandbox.host_api.unknown
- unstable_dependency_order: expectedStatus=rejected, actualStatus=rejected, codes=lua_sandbox.dependency_order.unstable

## Repair plans

- lua-sandbox-repair-plan/invalid-denied_host_api_group: status=planned, actions=remove-denied-host-api-group, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-fake_manifest_id: status=planned, actions=replace-fake-manifest-id,restore-deterministic-ordering, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-filesystem_leak: status=planned, actions=remove-denied-host-api-group, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-final_prose_included: status=not_required, actions=, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-immutable_repair_mutation: status=not_required, actions=, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-lua_execution_claim_included: status=not_required, actions=, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-missing_budget: status=planned, actions=add-missing-budget, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-missing_goal034_promotion_trace: status=planned, actions=add-goal034-promotion-trace, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-native_interop_leak: status=planned, actions=remove-denied-host-api-group, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-network_leak: status=planned, actions=remove-denied-host-api-group, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-nondeterministic_ordering: status=planned, actions=restore-deterministic-ordering, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-over_budget: status=planned, actions=reduce-budget,split-overlarge-request, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-parser_claim_included: status=not_required, actions=, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-process_leak: status=planned, actions=remove-denied-host-api-group, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-provider_llm_rag_leak: status=not_required, actions=, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-random_leak: status=planned, actions=remove-denied-host-api-group, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-reflection_leak: status=planned, actions=remove-denied-host-api-group, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-runtime_ui_unity_gamepackage_schema_mutation_leak: status=blocked, actions=remove-denied-host-api-group, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-self_promotion: status=not_required, actions=, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-source_text_included: status=not_required, actions=, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-threading_leak: status=planned, actions=remove-denied-host-api-group, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-time_leak: status=planned, actions=remove-denied-host-api-group, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-unknown_host_api_group: status=planned, actions=remove-unknown-host-api-group, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/invalid-unstable_dependency_order: status=planned, actions=restore-deterministic-ordering, mutatesAcceptedManifests=false
- lua-sandbox-repair-plan/metamodule-kingdoms: status=planned, actions=mark-future-executor-adapter-required, mutatesAcceptedManifests=false

## Boundaries

- luaExecuted: false
- luaParserUsed: false
- luaSourceGenerated: false
- externalDependencyAdded: false
- runtimeUiUnityGamePackageProviderLlmRagTouched: false

No real Lua execution happened. No Lua parser was used. No Lua source was generated. No external dependency was added. No Runtime/UI/Unity/GamePackage/provider/LLM/RAG path was touched.

lua_sandbox_execution_gate_verification required
luaExecuted=false
