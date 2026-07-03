# Goal 096 Unity Handoff Inspector Probe Readiness Report

- implementationStatus: GREEN
- accepted: false
- manualGate: unity_handoff_inspector_probe_readiness_verification required
- deterministicReportHash: ab02bef416aa9fa12e71592e8eef5b4e5830b179be2774b11f7ee1e33463345f

## Summary

Goal 096 extends the existing BCL-only Visual World Stream Preview Workspace so editor review can inspect Goal 095 Unity StreamingAssets handoff readiness without launching Unity. It loads real Goal 095 evidence and mirrored payload files by repository-relative path, compares hashes against the Goal 095 ledgers, and does not change Runtime, Unity behavior, providers, schema, project files, dependencies, binary media or raster media.

## Catalog

- groupCount: 7
- entryCount: 81
- svgTextPreviewCount: 38
- goal091StreamWindowEntryCount: 4

- microtiles: entries=27, svgEntries=24, sourceGoal=goal_086_deterministic_visual_microtile_materializer, status=Passed
- map_patches: entries=6, svgEntries=3, sourceGoal=goal_087_deterministic_visual_map_patch_composer, status=Passed
- region_composer: entries=6, svgEntries=3, sourceGoal=goal_088_deterministic_visual_region_composer, status=Passed
- world_profiles: entries=7, svgEntries=4, sourceGoal=goal_090_parameterized_visual_world_profiles, status=Passed
- chunk_stream_windows: entries=8, svgEntries=4, sourceGoal=goal_091_deterministic_visual_chunk_stream_window, status=Passed
- cache_exports: entries=13, svgEntries=0, sourceGoal=goal_093_visual_chunk_cache_export_contract, status=Passed
- unity_handoff: entries=14, svgEntries=0, sourceGoal=goal_095_visual_chunk_cache_unity_streamingassets_handoff, status=Passed

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

## Proof Status

- proofStatusPassed: true
- proofCount: 19
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

## Source Health

- sourceHealthPassed: true
- scannedCSharpFileCount: 12
- workspaceServiceLogicalLineCount: 153
- maxLogicalLineCount: 533
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
- noAbsolutePaths: true
- noBinaryOrRasterMediaAdded: true
- noRuntimeUnityProviderSchemaProjectDependencyChanges: true
- noPromptDumps: true

## Artifact Hashes

- catalogHash: f7332f4455dd509c509683946dbd4d806d07457862c326adfebbd5f15af00c7e
- proofStatusHash: f89c76f3538bca12d5d5309af12a57eb8f821735a517b1486f6360714b3c8385
- winFormsBindingInventoryHash: fa9887f7ebf262d1e3cd69f359d56c6772ef735bbe88efe7d7ac6745f0265cf0
- qualityGateHash: 25676b7dce2753d75030530b951297651a4a9a6758c801abd9404928240970b5
