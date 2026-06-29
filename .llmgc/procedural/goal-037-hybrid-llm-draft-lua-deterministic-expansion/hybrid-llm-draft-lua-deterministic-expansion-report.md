# Hybrid LLM Draft Plus Lua Deterministic Expansion Report

- accepted: false
- accepted=false
- implementationStatus: GREEN
- finalStatus: hybrid_llm_draft_lua_deterministic_expansion_verification
- manualGate: hybrid_llm_draft_lua_deterministic_expansion_verification
- required marker: hybrid_llm_draft_lua_deterministic_expansion_verification required
- productSmokeRoute: goal-037-hybrid-llm-draft-lua-deterministic-expansion
- contractProofPassed: true
- realBoundedExecutorPathProven: true
- adapter: hybrid-lua-executor/luacsharp-0.5.5
- package: LuaCSharp 0.5.5 (MIT)
- scenarioCount: 4
- expansionRequestCount: 8
- executedRequestCount: 8
- outputCount: 8
- metamoduleSpeciesArchetypeSlotCount: 112
- invalidScenarioCount: 15
- invalidMatrixPassed: true
- adapterSelectionHash: bcaba6fc00ea3c25e31443997e297e473c17df591c2b9ad2687fa0ad85a42710
- pipelineSummaryHash: c8187f27350964f57f6a5bc3b47b68099b03b17b6784afd5ce7b5b2e812fa668
- draftRequestMapHash: 6aeb2759576baab4a36b779c7ede636c213cac5b2f7f89a4e01bc3250adb864c
- sandboxMatrixHash: 1c70dc55da0380ff7e41f80ba3add3341f00119a5315140a85e93b5e5fb82635
- promotionMatrixHash: a798f6fe0f194a469d3e2a524a75fa40226de0b36581c8f253910d4a30b51ab2
- invalidMatrixHash: 417f018ce3b8fdd882f922e76e610e71e3a75a14ce74634d3ac315fc12e185da
- reportHash: 8c7662d6181f0e268569c1d3e5c8e4681f8ced51263d40c4c60b6ecde388c39c

## What became more real

Goal 034 strict draft ids, Goal 035 manifest selections and Goal 036 sandbox decisions now flow through a real bounded LuaCSharp executor adapter for repo-owned deterministic expansion fixtures, then through C# validation and promotion decisions.

## Pipeline

- 1. goal034_draft_request_candidate: Goal034; Use strict draft request/candidate identifiers only; no live LLM call.
- 2. goal035_lua_manifest_selection: Goal035; Select reviewed manifest ids and dependency order from the manifest registry.
- 3. goal036_sandbox_gate_decision: Goal036; Require deny-first sandbox decision evidence before any bounded executor attempt.
- 4. bounded_lua_expansion_request: Goal037; Map draft/manifest/sandbox evidence to repo-owned deterministic fixture requests.
- 5. executor_adapter_result: Goal037; Run LuaCSharp without standard libraries or host APIs only for declarative fixtures.
- 6. csharp_output_validator: Goal037; Validate structured IR shape, budgets, traces and forbidden boundary claims.
- 7. promotion_decision: Goal037; Accept only validated IR for future review; never self-promote the manual gate.

## Adapter decision

- packageId: LuaCSharp
- packageVersion: 0.5.5
- license: MIT
- status: selected_real_bounded_executor
- standardLibrariesOpened: false
- arbitraryUserLuaAllowed: false
- instructionCountHookSupported: false
- declarativeFixtureRestrictionRequired: true
- riskNote: LuaCSharp 0.5.5 restored in a disposable net8.0 probe and declares MIT license in the package nuspec.
- riskNote: The package declares LuaCSharp.SourceGenerator transitively, but package metadata excludes Build and Analyzers assets for that dependency; no explicit source-generator package is added by this repo.
- riskNote: LuaCSharp does not expose a Goal037-proven instruction-count hook, so accepted scripts are restricted to repo-owned declarative fixtures and loop/import/global boundary tokens are rejected before execution.
- riskNote: The adapter never calls OpenStandardLibraries and never exposes host functions, .NET objects, filesystem, network, process, reflection, threading, wall-clock time, random, native interop, Runtime, UI, Unity, provider, LLM or RAG surfaces.

## Sandbox approvals

- caravan_trade: goal036=dry_run_only, approvedForGoal037=true, reason=Goal036 decision is not rejected/repair_required and Goal037 provides a repo-owned fixture-only adapter.
- frontier_survival: goal036=dry_run_only, approvedForGoal037=true, reason=Goal036 decision is not rejected/repair_required and Goal037 provides a repo-owned fixture-only adapter.
- gothic_intrigue: goal036=ready_for_future_executor, approvedForGoal037=true, reason=Goal036 decision is not rejected/repair_required and Goal037 provides a repo-owned fixture-only adapter.
- metamodule_kingdoms: goal036=blocked_no_executor, approvedForGoal037=true, reason=Goal036 decision is not rejected/repair_required and Goal037 provides a repo-owned fixture-only adapter.

## Promotion decisions

- caravan_trade: status=accepted, promoted=true, output=hybrid-expansion/caravan_trade/economy_combat_settlement_expansion_hints
- caravan_trade: status=accepted, promoted=true, output=hybrid-expansion/caravan_trade/quest_event_intent_expansion_hints
- frontier_survival: status=accepted, promoted=true, output=hybrid-expansion/frontier_survival/npc_species_archetype_expansion_hints
- frontier_survival: status=accepted, promoted=true, output=hybrid-expansion/frontier_survival/quest_event_intent_expansion_hints
- gothic_intrigue: status=accepted, promoted=true, output=hybrid-expansion/gothic_intrigue/quest_event_intent_expansion_hints
- gothic_intrigue: status=accepted, promoted=true, output=hybrid-expansion/gothic_intrigue/region_faction_kingdom_expansion_hints
- metamodule_kingdoms: status=accepted, promoted=true, output=hybrid-expansion/metamodule_kingdoms/metamodule_species_archetype_slot_expansion
- metamodule_kingdoms: status=accepted, promoted=true, output=hybrid-expansion/metamodule_kingdoms/region_faction_kingdom_expansion_hints

## Invalid/fake/leak matrix

- dependency_unavailable_unsafe_adapter_blocker_path: expectedStatus=blocked, actualStatus=blocked, codes=hybrid.adapter.blocked
- fake_goal034_draft_id: expectedStatus=rejected, actualStatus=rejected, codes=hybrid.goal034_draft.fake,hybrid.output.goal034_draft_mismatch
- fake_goal035_manifest_id: expectedStatus=rejected, actualStatus=rejected, codes=hybrid.goal035_manifest.fake,hybrid.output.goal035_manifest_mismatch
- fake_goal036_sandbox_decision_id: expectedStatus=rejected, actualStatus=rejected, codes=hybrid.goal036_sandbox_decision.fake,hybrid.output.goal036_decision_mismatch
- filesystem_network_process_reflection_thread_time_random_native_interop_request: expectedStatus=rejected, actualStatus=rejected, codes=hybrid.boundary.file_system.forbidden,hybrid.boundary.native_interop.forbidden,hybrid.boundary.network.forbidden,hybrid.boundary.process.forbidden,hybrid.boundary.random.forbidden,hybrid.boundary.reflection.forbidden,hybrid.boundary.threading.forbidden,hybrid.boundary.time.forbidden
- final_prose_payload: expectedStatus=rejected, actualStatus=rejected, codes=hybrid.final_prose.forbidden
- gamepackage_mutation_claim: expectedStatus=rejected, actualStatus=rejected, codes=hybrid.gamepackage_mutation.forbidden
- malformed_executor_output: expectedStatus=rejected, actualStatus=rejected, codes=hybrid.output.malformed
- missing_trace: expectedStatus=rejected, actualStatus=rejected, codes=hybrid.output.trace.missing
- nondeterministic_output_order: expectedStatus=rejected, actualStatus=rejected, codes=hybrid.output.order.nondeterministic
- over_budget_output: expectedStatus=rejected, actualStatus=rejected, codes=hybrid.output.budget.exceeded
- runtime_ui_unity_provider_llm_rag_lua_source_generation_leak: expectedStatus=rejected, actualStatus=rejected, codes=hybrid.boundary.lua_source_generation.forbidden,hybrid.boundary.provider_llm.forbidden,hybrid.boundary.rag.forbidden,hybrid.boundary.runtime_mutation.forbidden,hybrid.boundary.ui.forbidden,hybrid.boundary.unity.forbidden
- sandbox_denied_executor_attempted: expectedStatus=rejected, actualStatus=rejected, codes=hybrid.sandbox_denied.executor_attempted
- self_promotion: expectedStatus=rejected, actualStatus=rejected, codes=hybrid.self_promotion.forbidden
- wrong_scenario_profile: expectedStatus=rejected, actualStatus=rejected, codes=hybrid.profile.wrong_scenario

## Boundaries

- noLiveLlmProviderRagCall: true
- noFinalProse: true
- noRuntimeUiUnityGamePackageMutation: true
- noFilesystemNetworkProcessReflectionThreadTimeRandomNativeInterop: true

No live LLM/provider/RAG call happened. No final prose was generated. No Runtime/UI/Unity/GamePackage/provider/LLM/RAG path was touched. No filesystem/network/process/reflection/thread/time/random/native interop surface was exposed.

hybrid_llm_draft_lua_deterministic_expansion_verification required
