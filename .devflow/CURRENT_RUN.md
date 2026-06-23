Task id: PRODUCT_SLICE_022_UNITY_ARCHIVE_FULFILLMENT_STATE_V1
Goal: add fulfillment state scanner that checks expected output file existence and writes fulfillment state metadata

Source docs/code read:
- AGENTS.md, docs/CONTEXT_INDEX.md, docs/CURRENT_GENERATOR_STATE.md/json
- docs/agent-tasks/NEXT_PRODUCT_SLICE/022_PROVIDER_OUTPUT_INTAKE.md
- UnityArchiveProviderJobPlanModels.cs, UnityArchiveMaterializationModels.cs, UnityArchiveMaterializationService.cs
- existing fulfillment slot and request models/services

Patterns applied:
- Application-only metadata scanning without provider execution
- schemaVersion JSON envelopes, camelCase enum serialization and stable ordinal ordering
- path safety via UnityArchiveProviderJobPlanService.IsSafeExpectedOutputRelativePath
- status: missing/available/invalid based on file existence and validity
- materialization writes metadata files even for empty slot arrays

Implemented:
- UnityArchiveFulfillmentState models: FulfillmentStateRequest, FulfillmentStateResult, FulfillmentStateReport, FulfilledAssetEntry, FulfilledAudioEntry, FulfilledLuaEntry, InvalidOutputEntry, FulfillmentStatus enum
- UnityArchiveFulfillmentStateService scans expected output paths and checks physical file existence
- Status logic: missing for absent files, available for valid non-empty files with correct extension, invalid for unsafe paths, wrong extensions, empty files or directory paths
- UnityArchiveMaterializationService integration writes five fulfillment state files under production/
- UnityArchiveFulfillmentStateTests for empty manifests, missing available invalid status, wrong extension, unsafe path diagnostics

Checks run before state handoff:
- UnityArchiveFulfillmentState tests: passed, 7 tests
- UnityArchiveProviderJobPlan tests: passed, 5 tests
- UnityArchiveMaterialization tests: passed, 7 tests
- unity-archive-fulfillment-state product smoke: passed
- unity-archive-provider-job-plan product smoke: passed
- ProductSmoke filtered tests: passed (multiple scenarios)

Forbidden scope preserved:
- no Unity project or implementation
- no Runtime, WinForms, GamePackage schema, generator-library, solution or project changes
- no provider, generator, LLM or Lua execution
- no git commands
