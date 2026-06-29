# Runtime Chunk Delta Traversal Smoke Report

- accepted: false
- accepted=false
- implementationStatus: GREEN
- finalStatus: runtime_chunk_delta_traversal_smoke_verification
- manualGate: runtime_chunk_delta_traversal_smoke_verification
- required marker: runtime_chunk_delta_traversal_smoke_verification required
- productSmokeRoute: runtime-chunk-delta-traversal-smoke
- goal038AcceptedByUserHandoff: true
- scenarioCount: 4
- traversalPlanCount: 4
- runtimeStateProofCount: 4
- runtimeMutationScenarioCount: 4
- totalCommandCount: 152
- runtimeStateChangedAfterTraversal: true
- saveLoadRoundtripPassed: true
- replayDeterminismPassed: true
- invalidMatrixPassed: true
- gamePackageDefinitionsMutated: false
- metamoduleKingdomGroupCount: 7
- metamoduleSpeciesArchetypeSlotRefCount: 112
- noRuntimeUiUnityProviderLlmRagLuaGeneratorLibraryLeakage: true
- frontierStateHash: 3234fe183a86e416f6bbe2292f5f27086151bc9eee79c6cde3475dd4c5077458
- metamoduleStateHash: 56a9d74c2f95694b47e115c7e61988a0ca72f16650c7bb1f2c3524f8d4671ccb
- saveLoadProofHash: 9f5958d51eac5835988ff947ecbf841e0ec0708758ff82754fdaa2f6a1b97551
- replayProofHash: 81721d73dd6da7c5c6802d39817bf61f743e7444f8464376d2371d8ed4631db1
- invalidMatrixHash: 74c043826a8a3f68bff5e96bf720557d87799eec9ef5798e582985b8a482e18a
- reportHash: a6819ab26302d58fdb83a51764c3a62144d45ad606669bc9fcbf1098e904e982

## What became more real

Goal 038 region graph, finite-map and chunk-config facts now drive runtime-facing traversal commands that mutate runtime-owned chunk delta state and survive serializer/snapshot save-load proof.

## Traversal plans

- caravan_trade: steps=8, commands=39, requiredTargets=4, chunks=5, mutations=7
- frontier_survival: steps=7, commands=34, requiredTargets=3, chunks=4, mutations=6
- gothic_intrigue: steps=9, commands=44, requiredTargets=4, chunks=5, mutations=8
- metamodule_kingdoms: steps=7, commands=35, requiredTargets=7, chunks=7, mutations=7

## Runtime state

- caravan_trade: changed=true, visitedRegions=5, chunks=5, mutations=4, deltas=39, hash=62410d81fe30708c9f0bf83d535d7c75718f02220b634b115fd06c5459f29e7d
- frontier_survival: changed=true, visitedRegions=4, chunks=4, mutations=3, deltas=34, hash=3234fe183a86e416f6bbe2292f5f27086151bc9eee79c6cde3475dd4c5077458
- gothic_intrigue: changed=true, visitedRegions=5, chunks=5, mutations=4, deltas=44, hash=5d85343a2bbb8310a4f97ac2bcfdd14f92b5db6ec6b9f260a81b2b30ac146570
- metamodule_kingdoms: changed=true, visitedRegions=7, chunks=7, mutations=7, deltas=35, hash=56a9d74c2f95694b47e115c7e61988a0ca72f16650c7bb1f2c3524f8d4671ccb

## Save/load

- caravan_trade: serializer=true, snapshotStore=true, serializerRoundtrip=true, snapshotRoundtrip=true, slot=goal039_caravan-trade
- frontier_survival: serializer=true, snapshotStore=true, serializerRoundtrip=true, snapshotRoundtrip=true, slot=goal039_frontier-survival
- gothic_intrigue: serializer=true, snapshotStore=true, serializerRoundtrip=true, snapshotRoundtrip=true, slot=goal039_gothic-intrigue
- metamodule_kingdoms: serializer=true, snapshotStore=true, serializerRoundtrip=true, snapshotRoundtrip=true, slot=goal039_metamodule-kingdoms

## Replay determinism

- caravan_trade: sameSeed=true, commands=39, hash=62410d81fe30708c9f0bf83d535d7c75718f02220b634b115fd06c5459f29e7d
- frontier_survival: sameSeed=true, commands=34, hash=3234fe183a86e416f6bbe2292f5f27086151bc9eee79c6cde3475dd4c5077458
- gothic_intrigue: sameSeed=true, commands=44, hash=5d85343a2bbb8310a4f97ac2bcfdd14f92b5db6ec6b9f260a81b2b30ac146570
- metamodule_kingdoms: sameSeed=true, commands=35, hash=56a9d74c2f95694b47e115c7e61988a0ca72f16650c7bb1f2c3524f8d4671ccb

## Invalid/fake/leak matrix

- chunk_coordinate_outside_bounds: expectedStatus=rejected, actualStatus=rejected, codes=runtime_chunk.coordinate.out_of_bounds
- conflicting_delta_mutation: expectedStatus=rejected, actualStatus=rejected, codes=runtime_chunk.delta.conflict,runtime_chunk.delta.duplicate,runtime_chunk.order.nondeterministic
- duplicate_delta_id: expectedStatus=rejected, actualStatus=rejected, codes=runtime_chunk.delta.duplicate,runtime_chunk.order.nondeterministic
- fake_chunk_id: expectedStatus=rejected, actualStatus=rejected, codes=runtime_chunk.chunk.unknown
- fake_goal038_scenario_id: expectedStatus=rejected, actualStatus=rejected, codes=runtime_chunk.goal038_scenario.fake
- fake_region_id: expectedStatus=rejected, actualStatus=rejected, codes=runtime_chunk.region.unknown
- filesystem_network_process_reflection_thread_time_random_native_interop_leakage: expectedStatus=blocked, actualStatus=blocked, codes=runtime_chunk.boundary.filesystem.forbidden,runtime_chunk.boundary.native_interop.forbidden,runtime_chunk.boundary.network.forbidden,runtime_chunk.boundary.process.forbidden,runtime_chunk.boundary.random.forbidden,runtime_chunk.boundary.reflection.forbidden,runtime_chunk.boundary.thread.forbidden,runtime_chunk.boundary.time.forbidden
- missing_save_load_proof: expectedStatus=rejected, actualStatus=rejected, codes=runtime_chunk.persistence.missing
- mutation_tries_to_edit_gamepackage_definitions: expectedStatus=blocked, actualStatus=blocked, codes=runtime_chunk.boundary.gamepackage.forbidden
- nondeterministic_ordering: expectedStatus=rejected, actualStatus=rejected, codes=runtime_chunk.order.nondeterministic
- replay_seed_mismatch: expectedStatus=rejected, actualStatus=rejected, codes=runtime_chunk.replay.seed_mismatch
- route_edge_not_in_reachability_plan: expectedStatus=rejected, actualStatus=rejected, codes=runtime_chunk.route.edge_unreachable
- runtime_ui_unity_provider_llm_rag_lua_generator_library_leakage: expectedStatus=blocked, actualStatus=blocked, codes=runtime_chunk.boundary.generator_library.forbidden,runtime_chunk.boundary.lua.forbidden,runtime_chunk.boundary.provider_llm_rag.forbidden,runtime_chunk.boundary.runtime_source.forbidden,runtime_chunk.boundary.ui.forbidden,runtime_chunk.boundary.unity.forbidden

## Boundaries

No GamePackage schema/source definition, WinForms/UI, Unity, provider, LLM/RAG, Lua source/execution, generator-library or external dependency change is required by this evidence.

runtime_chunk_delta_traversal_smoke_verification required
