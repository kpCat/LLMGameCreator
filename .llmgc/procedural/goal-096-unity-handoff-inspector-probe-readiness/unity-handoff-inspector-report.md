# Goal 096 Unity Handoff Inspector Probe Readiness Report

- implementationStatus: GREEN
- accepted: false
- manualGate: unity_handoff_inspector_probe_readiness_verification required
- deterministicReportHash: 53a69f054f71fae3e98114509b2f3603786a868fe5c2e5b80b09b86ba1ae520d

## Summary

Goal 096 extends the existing BCL-only Visual World Stream Preview Workspace so editor review can inspect Goal 095 Unity StreamingAssets handoff readiness without launching Unity. It loads real Goal 095 evidence and mirrored payload files by repository-relative path, compares hashes against the Goal 095 ledgers, and does not change Runtime, Unity behavior, providers, schema, project files, dependencies, binary media or raster media.

## Catalog

- groupCount: 9
- entryCount: 112
- svgTextPreviewCount: 39
- goal091StreamWindowEntryCount: 4

- microtiles: entries=27, svgEntries=24, sourceGoal=goal_086_deterministic_visual_microtile_materializer, status=Passed
- map_patches: entries=6, svgEntries=3, sourceGoal=goal_087_deterministic_visual_map_patch_composer, status=Passed
- region_composer: entries=6, svgEntries=3, sourceGoal=goal_088_deterministic_visual_region_composer, status=Passed
- world_profiles: entries=7, svgEntries=4, sourceGoal=goal_090_parameterized_visual_world_profiles, status=Passed
- chunk_stream_windows: entries=8, svgEntries=4, sourceGoal=goal_091_deterministic_visual_chunk_stream_window, status=Passed
- cache_exports: entries=13, svgEntries=0, sourceGoal=goal_093_visual_chunk_cache_export_contract, status=Passed
- unity_handoff: entries=14, svgEntries=0, sourceGoal=goal_095_visual_chunk_cache_unity_streamingassets_handoff, status=Passed
- geoworld: entries=13, svgEntries=1, sourceGoal=goal_099_offline_geoworld_worldsourcegraph_streaming, status=Passed
- offline_geoworld_handoff: entries=18, svgEntries=0, sourceGoal=goal_100_offline_geoworld_visual_cache_unity_handoff, status=Passed

## Cache Export Inspector

- cacheExportPackageCount: 4
- cacheExportRecordCount: 93
- cacheExportSourceChunkCount: 117
- cacheExportStreamWindowCount: 5
- runtimeHandoffSidecarVisible: true
- runtimeHandoffSidecarMetadataOnly: true
- cacheReadbackProofPassed: true
- cacheOverlapReuseProofPassed: true
- cacheNegativeProofPassed: true
- cacheInvalidationMatrixPassed: true
- cacheNoRawFullWorldDump: true

## Unity Handoff Inspector

- unityPayloadFileCount: 5
- unityPackageCount: 4
- unityExportRecordCount: 93
- unityStreamWindowCount: 5
- unityUniqueChunkKeyCount: 93
- unityProbeSourceInventoryVisible: true
- unityProbeSourceInventoryPassed: true
- unitySimulatedReadProofPassed: true
- unityNegativeProofPassed: true
- unityAlphaRuntimeBootstrapUnchanged: true
- unityForbiddenAreasUnchanged: true
- unityHandoffMetadataOnly: true
- unityPayloadHashesMatchGoal095Ledger: true
- goal095FilesDiscoveredByRelativePaths: true
- noUnityFilesChangedByGoal096: true

## Geoworld Inspector

- geoworldOfflineBundleId: synthetic_city_radius_offline_bundle
- geoworldNormalizedFeatureCount: 10
- geoworldWorldSourceGraphChunkCount: 5
- geoworldStreamWindowChunkCount: 9
- geoworldBoundaryPrefetchPassed: true
- geoworldNegativeProofPassed: true
- geoworldQualityGatePassed: true
- goal099FilesDiscoveredByRelativePaths: true

## Offline Geoworld Handoff

- offlineGeoworldHandoffPackageCount: 3
- offlineGeoworldHandoffFeatureCount: 10
- offlineGeoworldHandoffVisualCacheRecordCount: 18
- offlineGeoworldHandoffSourceChunkCount: 5
- offlineGeoworldHandoffStreamWindowChunkCount: 9
- offlineGeoworldHandoffUnityPayloadFileCount: 5
- offlineGeoworldHandoffFeatureKindCounts: administrativeHint=1; barrier=1; bridge=1; buildingFootprint=1; landUse=1; poi=1; roadSegment=1; terrainHint=1; vegetation=1; waterBody=1
- offlineGeoworldHandoffSimulatedReadProofPassed: true
- offlineGeoworldHandoffNegativeProofPassed: true
- offlineGeoworldHandoffAlphaRuntimeBootstrapUnchanged: true
- offlineGeoworldHandoffQualityGatePassed: true
- goal100FilesDiscoveredByRelativePaths: true

## Proof Status

- proofStatusPassed: true
- proofCount: 32
- goal091.cache_reuse: passed=true, path=.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-cache-reuse-proof.json
- goal091.finite_boundary_clipping: passed=true, path=.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-quality-gate-scan.json
- goal091.huge_sparse_no_raw_dump: passed=true, path=.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-quality-gate-scan.json
- goal091.infinite_overlap_reuse: passed=true, path=.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-quality-gate-scan.json
- goal091.layer_transition: passed=true, path=.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-layer-transition-proof.json
- goal091.negative: passed=true, path=.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-negative-proof.json
- goal091.seam: passed=true, path=.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-seam-proof.json
- goal093.invalidation_matrix: passed=true, path=.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/visual-chunk-cache-invalidation-matrix.json
- goal093.negative: passed=true, path=.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/visual-chunk-cache-negative-proof.json
- goal093.overlap_reuse: passed=true, path=.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/visual-chunk-cache-overlap-reuse-proof.json
- goal093.readback: passed=true, path=.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/visual-chunk-cache-readback-proof.json
- goal093.runtime_handoff_metadata_only: passed=true, path=.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/visual-chunk-cache-runtime-handoff-sidecar.json
- goal095.alpha_runtime_bootstrap_unchanged: passed=true, path=.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/visual-chunk-cache-unity-quality-gate-scan.json
- goal095.forbidden_unity_areas_unchanged: passed=true, path=.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/visual-chunk-cache-unity-quality-gate-scan.json
- goal095.metadata_only: passed=true, path=.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/visual-chunk-cache-unity-handoff-manifest.json
- goal095.negative: passed=true, path=.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/visual-chunk-cache-unity-negative-proof.json
- goal095.probe_source_inventory: passed=true, path=.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/visual-chunk-cache-unity-probe-source-inventory.json
- goal095.simulated_read: passed=true, path=.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/visual-chunk-cache-unity-simulated-read-proof.json
- goal095.streamingassets_ledger: passed=true, path=.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/visual-chunk-cache-unity-streamingassets-ledger.json
- goal099.boundary_prefetch: passed=true, path=.llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming/offline-geoworld-boundary-prefetch-proof.json
- goal099.negative: passed=true, path=.llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming/offline-geoworld-negative-proof.json
- goal099.quality_gate: passed=true, path=.llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming/offline-geoworld-quality-gate-scan.json
- goal099.visual_projection: passed=true, path=.llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming/offline-geoworld-visual-projection-summary.json
- goal100.all_feature_kinds_mapped: passed=true, path=.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/offline-geoworld-quality-gate-scan.json
- goal100.alpha_runtime_bootstrap_unchanged: passed=true, path=.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/offline-geoworld-quality-gate-scan.json
- goal100.negative: passed=true, path=.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/offline-geoworld-negative-proof.json
- goal100.probe_source_inventory: passed=true, path=.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/offline-geoworld-unity-probe-source-inventory.json
- goal100.quality_gate: passed=true, path=.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/offline-geoworld-quality-gate-scan.json
- goal100.simulated_read: passed=true, path=.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/offline-geoworld-unity-simulated-read-proof.json
- goal100.streamingassets_ledger: passed=true, path=.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/offline-geoworld-unity-streamingassets-ledger.json
- goal100.visual_cache_records: passed=true, path=.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/offline-geoworld-quality-gate-scan.json
- goal100.workspace_binding: passed=true, path=.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/offline-geoworld-workspace-binding-inventory.json

## WinForms Binding

- bindingPassed: true
- pageControlExists: true
- designerExists: true
- compositionRootRegistersService: true
- compositionRootRegistersPage: true
- editorRegistryIncludesPage: true
- pageActivationLoadsApplicationResult: true
- pageBindDisplaysGroupsEntriesProofs: true
- pageBindDisplaysCacheExports: true
- pageBindDisplaysUnityHandoff: true
- pageBindDisplaysGeoworld: true
- pageBindDisplaysOfflineGeoworldHandoff: true

## Source Health

- sourceHealthPassed: true
- scannedCSharpFileCount: 15
- workspaceServiceLogicalLineCount: 155
- maxLogicalLineCount: 631
- maxPhysicalLineLength: 451
- filesOver1000LogicalLinesCount: 0
- filesOver700LogicalLinesInGoal092NamespaceCount: 0
- zeroLfSourceCount: 0
- crOnlySourceCount: 0
- rawPhysicalOneLineSourceCount: 0
- minifiedSourceCount: 0

## Quality Gate

- qualityGatePassed: true
- requiredArtifactGroupsPresent: true
- goal091StreamWindowsVisible: true
- cacheExportGroupPresent: true
- goal093FilesDiscoveredByRelativePaths: true
- unityHandoffGroupPresent: true
- unityProbeSourceInventoryPassed: true
- unitySimulatedReadProofPassed: true
- unityNegativeProofPassed: true
- unityAlphaRuntimeBootstrapUnchanged: true
- unityForbiddenAreasUnchanged: true
- unityHandoffMetadataOnly: true
- unityPayloadHashesMatchGoal095Ledger: true
- goal095FilesDiscoveredByRelativePaths: true
- noUnityFilesChangedByGoal096: true
- geoworldGroupPresent: true
- geoworldBoundaryPrefetchPassed: true
- geoworldTaxonomyCoveragePassed: true
- geoworldNegativeProofPassed: true
- geoworldQualityGatePassed: true
- geoworldOverviewVisible: true
- goal099FilesDiscoveredByRelativePaths: true
- offlineGeoworldHandoffGroupPresent: true
- offlineGeoworldHandoffPackageCount: 3
- offlineGeoworldHandoffVisualCacheRecordCount: 18
- offlineGeoworldHandoffSimulatedReadProofPassed: true
- offlineGeoworldHandoffNegativeProofPassed: true
- offlineGeoworldHandoffAlphaRuntimeBootstrapUnchanged: true
- offlineGeoworldHandoffQualityGatePassed: true
- goal100FilesDiscoveredByRelativePaths: true
- noAbsolutePaths: true
- noBinaryOrRasterMediaAdded: true
- noRuntimeUnityProviderSchemaProjectDependencyChanges: true
- noPromptDumps: true

## Artifact Hashes

- catalogHash: 1cb879f7e2b7a3317bc771e0a63fdf28e7ed8f08461d50c5a0a9b8c8fd8f4caf
- proofStatusHash: 7f23750a3ebbd7e2da20e7e537d141a0764f88bd7882f534c503af2fc9288a42
- winFormsBindingInventoryHash: c8f9937e2d02dd4769901a96b03cc886db053e40b534965037bb0a23fc51f429
- qualityGateHash: b15057888a59787dc225aba4700dfa2a52e0151e5d6bea90249b6172179fa497
