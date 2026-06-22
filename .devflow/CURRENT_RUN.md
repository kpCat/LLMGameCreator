Task id: PRODUCT_SLICE_021_UNITY_ARCHIVE_PROVIDER_JOB_PLAN_V1
Goal: convert Slice 020 request metadata into deterministic fulfillment slots and provider-specific non-executable job plans

Source docs/code read:
- AGENTS.md, docs/CONTEXT_INDEX.md, docs/CURRENT_GENERATOR_STATE.md/json
- docs/agent-tasks/NEXT_PRODUCT_SLICE/021_PROVIDER_JOB_PLAN.md
- docs/PRODUCT_SLICE_021_PROVIDER_JOB_PLAN.md and docs/UNITY_ARCHIVE_PROVIDER_JOB_PLAN_SPEC.md
- target Application/test csproj files
- existing Unity archive request models/service/builders/diagnostics
- existing Unity archive materialization models/service and focused tests
- request-pipeline product smoke, smoke runner and smoke scenario docs

Patterns applied:
- Application-only metadata planning over the existing request-pipeline result
- schemaVersion JSON envelopes, camelCase enum serialization and stable ordinal ordering
- normalized archive-relative expected output paths with lexical safety validation
- future providers remain warning-only and every provider job remains planned_not_executed
- materialization writes metadata files even for empty slot/job arrays

Implemented:
- UnityArchiveProviderJobPlan models for fulfillment plan, typed slots, provider batches/jobs, diagnostics and readiness
- UnityArchiveProviderJobPlanService creates one slot per request and no job for provider none
- provider jobs grouped into manual-import, comfyui, suno, local-audio and procedural batches
- UnityArchiveMaterializationService writes ten new provider-plan metadata files
- focused contract/materialization tests and unity-archive-provider-job-plan product smoke
- existing unity-archive-request-pipeline smoke remains green

Path safety:
- expected outputs are relative, slash-normalized metadata paths only
- rooted paths, backslashes, colon and traversal markers are rejected
- asset/audio/Lua slots use .png/.wav/.lua extensions respectively
- no expected output file is physically created

Checks run before state handoff:
- UnityArchiveProviderJob filtered tests: passed, 5 tests
- UnityArchiveRequestPipeline filtered tests: passed, 11 tests
- UnityArchiveMaterialization filtered tests: passed, 5 tests
- unity-archive-request-pipeline product smoke: passed
- unity-archive-provider-job-plan product smoke: passed
- ProductSmoke filtered tests: passed, 19 tests
- check-devflow-state.ps1: passed, STOP_REVIEW preserved
- check-all.ps1: passed, build 0 warnings/0 errors, 567 tests passed

Forbidden scope preserved:
- no Unity project or implementation
- no Runtime, WinForms, GamePackage schema, generator-library, solution or project changes
- no provider, generator, LLM or Lua execution
- no git commands
