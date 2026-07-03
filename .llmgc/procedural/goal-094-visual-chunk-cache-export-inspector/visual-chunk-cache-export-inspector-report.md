# Goal 094 Visual Chunk Cache Export Inspector Report

- implementationStatus: GREEN
- accepted: false
- manualGate: visual_chunk_cache_export_inspector_verification required
- deterministicReportHash: dbfaea499227e900ae2caf7da7eb4c1ea95d8b467fcad499de25f5d36f10e2c9

## Summary

Goal 094 extends the existing BCL-only Visual World Stream Preview Workspace so editor review can inspect Goal 093 cache/export artifacts beside the earlier visual stack. It loads Goal 093 JSON evidence by repository-relative path and does not add Runtime, Unity, provider, schema, project-file, dependency, binary media or raster media changes.

## Catalog

- groupCount: 6
- entryCount: 67
- svgTextPreviewCount: 38
- goal091StreamWindowEntryCount: 4

- microtiles: entries=27, svgEntries=24, sourceGoal=goal_086_deterministic_visual_microtile_materializer, status=Passed
- map_patches: entries=6, svgEntries=3, sourceGoal=goal_087_deterministic_visual_map_patch_composer, status=Passed
- region_composer: entries=6, svgEntries=3, sourceGoal=goal_088_deterministic_visual_region_composer, status=Passed
- world_profiles: entries=7, svgEntries=4, sourceGoal=goal_090_parameterized_visual_world_profiles, status=Passed
- chunk_stream_windows: entries=8, svgEntries=4, sourceGoal=goal_091_deterministic_visual_chunk_stream_window, status=Passed
- cache_exports: entries=13, svgEntries=0, sourceGoal=goal_093_visual_chunk_cache_export_contract, status=Passed

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

## Proof Status

- proofStatusPassed: true
- proofCount: 12
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

## Source Health

- sourceHealthPassed: true
- scannedCSharpFileCount: 11
- workspaceServiceLogicalLineCount: 151
- maxLogicalLineCount: 489
- maxPhysicalLineLength: 360
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
- noAbsolutePaths: true
- noBinaryOrRasterMediaAdded: true
- noRuntimeUnityProviderSchemaProjectDependencyChanges: true
- noPromptDumps: true

## Artifact Hashes

- catalogHash: 1e5e227f9f4efa41e2620be70ed66731328d9f171db6589fc53bc3dd6120cd76
- proofStatusHash: 6f6d896053d92540e5822169d9bb5337779f8640b653e8af59cc23f3fbfb792d
- winFormsBindingInventoryHash: 84958b3aef89b81f09fbad43b526cb6f00dcd14adb9c3084508921f14417ac4b
- qualityGateHash: 4c960b89e451284c22944cced486d91d4284c1cca618776c130bf37189a13bcc
