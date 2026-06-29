# World-scale Region Map Foundation Report

- accepted: false
- accepted=false
- implementationStatus: GREEN
- finalStatus: world_scale_region_map_foundation_verification
- manualGate: world_scale_region_map_foundation_verification
- required marker: world_scale_region_map_foundation_verification required
- productSmokeRoute: goal-038-world-scale-region-map-foundation
- goal037AcceptedByUserHandoff: true
- contractProofPassed: true
- scenarioCount: 4
- regionGraphCount: 4
- totalRegionCount: 32
- totalTravelEdgeCount: 32
- requiredReachabilityCount: 18
- reachableRequiredTargetCount: 18
- finiteMapPackCount: 4
- chunkConfigScenarioCount: 4
- metamoduleKingdomGroupCount: 7
- metamoduleSpeciesArchetypeSlotRefCount: 112
- invalidScenarioCount: 17
- invalidMatrixPassed: true
- noRuntimeUiUnityGamePackageProviderLlmRagLuaGeneratorLibraryChanges: true
- regionGraphSummaryHash: 2ce5833e180a703428a083df0f898994599a109e274179663b7bf707d7ad003d
- reachabilityMatrixHash: 3fabe1b8eea38d2b6a39ff76f6a9fd00f44d0fe3752896ca4c65bd55ad84213c
- chunkConfigPreludeHash: 7b7671c52309eadfa7832c466d94d1baaa74f32b13c68ac42621b795c9f3effe
- traversalItineraryMatrixHash: 62a95521aba93a831c65f0cbda742828d93e4ed6fd7dbd06bef59ea1450ec06b
- invalidMatrixHash: 2d196f3f8f2f9db9a7e921f93eb14dc7ceffb1d946a8c7a98339a4d663c1be71
- reportHash: 9a60fcbb0220ece87e117ff263b94fac29838503fb4d32ed1b97fc29392583e6

## What became more real

Goal 037 hybrid expansion outputs now feed deterministic world-scale region graphs, reachability proof, finite map pack summaries and chunk-config prelude records that later runtime/export goals can consume.

## Route kinds

- caravan_route, dungeon_descent, magical_gate, mountain_pass, river, road, sea_lane, trail

## Reachability

- caravan_trade: start=region/caravan/oasis-camp, required=4, reachableRequired=4, totalCostTargets=region/caravan/glass-dunes=18,region/caravan/harbor=12,region/caravan/salt-pass=7,region/caravan/spice-market=3
- frontier_survival: start=region/frontier/homestead, required=3, reachableRequired=3, totalCostTargets=region/frontier/mountain-pass=10,region/frontier/pine-barrens=2,region/frontier/river-ford=5
- gothic_intrigue: start=region/gothic/manor, required=4, reachableRequired=4, totalCostTargets=region/gothic/abbey=5,region/gothic/crypt=9,region/gothic/market-square=2,region/gothic/observatory=16
- metamodule_kingdoms: start=region/metamodule/aurelian-capital, required=7, reachableRequired=7, totalCostTargets=region/metamodule/aurelian-capital=0,region/metamodule/brindle-capital=3,region/metamodule/cindervale-capital=7,region/metamodule/duskmire-capital=12,region/metamodule/elderglass-capital=19,region/metamodule/frostmere-capital=11,region/metamodule/goldwake-capital=5

## Finite map packs

- caravan_trade: map=finite-map/caravan_trade/world-scale-preview, coordinateKind=square, regionBindings=6, landmarks=6, routeSummaries=6, previewCells=6
- frontier_survival: map=finite-map/frontier_survival/world-scale-preview, coordinateKind=square, regionBindings=6, landmarks=6, routeSummaries=6, previewCells=6
- gothic_intrigue: map=finite-map/gothic_intrigue/world-scale-preview, coordinateKind=axial_hex, regionBindings=6, landmarks=6, routeSummaries=5, previewCells=6
- metamodule_kingdoms: map=finite-map/metamodule_kingdoms/world-scale-preview, coordinateKind=axial_hex, regionBindings=14, landmarks=14, routeSummaries=15, previewCells=12

## Chunk config prelude

- caravan_trade: chunkSize=16, coverageRegions=6, chunkIds=12, futureRules=3
- frontier_survival: chunkSize=16, coverageRegions=6, chunkIds=12, futureRules=3
- gothic_intrigue: chunkSize=16, coverageRegions=6, chunkIds=12, futureRules=3
- metamodule_kingdoms: chunkSize=24, coverageRegions=14, chunkIds=28, futureRules=3

## Invalid/fake/leak matrix

- all_routes_blocked: expectedStatus=rejected, actualStatus=rejected, codes=world_scale.routes.all_blocked
- chunk_config_missing_required_map_region: expectedStatus=rejected, actualStatus=rejected, codes=world_scale.chunk.coverage_missing
- contradictory_bidirectional_edge_declaration: expectedStatus=rejected, actualStatus=rejected, codes=world_scale.edge.bidirectional_contradiction
- duplicate_edge_id: expectedStatus=rejected, actualStatus=rejected, codes=world_scale.edge.duplicate,world_scale.order.nondeterministic
- duplicate_region_id: expectedStatus=rejected, actualStatus=rejected, codes=world_scale.order.nondeterministic,world_scale.region.duplicate
- fake_goal037_expansion_output_id: expectedStatus=rejected, actualStatus=rejected, codes=world_scale.goal037_output.fake
- forbidden_runtime_ui_unity_gamepackage_provider_llm_rag_lua_generator_library_leakage: expectedStatus=blocked, actualStatus=blocked, codes=world_scale.boundary.gamepackage.forbidden,world_scale.boundary.generator_library.forbidden,world_scale.boundary.lua.forbidden,world_scale.boundary.provider_llm_rag.forbidden,world_scale.boundary.runtime.forbidden,world_scale.boundary.ui.forbidden,world_scale.boundary.unity.forbidden
- huge_tile_array_dump_attempt: expectedStatus=rejected, actualStatus=rejected, codes=world_scale.map.tile_dump.forbidden
- invalid_map_coordinate_size: expectedStatus=rejected, actualStatus=rejected, codes=world_scale.map.coordinate_invalid,world_scale.map.size_invalid
- missing_start_region: expectedStatus=rejected, actualStatus=rejected, codes=world_scale.start_region.missing
- negative_zero_invalid_travel_cost: expectedStatus=rejected, actualStatus=rejected, codes=world_scale.edge.travel_cost.invalid
- nondeterministic_ordering_mutation: expectedStatus=rejected, actualStatus=rejected, codes=world_scale.order.nondeterministic
- required_target_unreachable: expectedStatus=rejected, actualStatus=rejected, codes=world_scale.edge.blocked_critical,world_scale.required_target.unreachable
- route_polyline_missing_required_region_binding: expectedStatus=rejected, actualStatus=rejected, codes=world_scale.route_polyline.region_binding_missing
- scenario_profile_mismatch: expectedStatus=rejected, actualStatus=rejected, codes=world_scale.scenario_profile.mismatch
- unknown_edge_endpoint: expectedStatus=rejected, actualStatus=rejected, codes=world_scale.edge.endpoint_unknown
- unknown_landmark_region: expectedStatus=rejected, actualStatus=rejected, codes=world_scale.landmark.region_unknown

## Boundaries

No Runtime, UI, Unity, GamePackage schema, provider, LLM/RAG, Lua source/execution, generator-library or external dependency change is required by this evidence.

world_scale_region_map_foundation_verification required
