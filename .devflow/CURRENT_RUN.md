Task id: CLEANUP_S021_S022_PROVIDER_PLAN_FULFILLMENT_STATE
Goal: make S021 provider planning and S022 fulfillment state deterministic, diagnostic-complete and merge-ready

Source docs/code read:
- AGENTS.md, docs/CONTEXT_INDEX.md, docs/CURRENT_GENERATOR_STATE.md/json
- S021 provider plan, S021 hardening and S022 provider output intake task/spec docs
- provider plan, fulfillment state and materialization models/services
- provider plan, fulfillment state, materialization and product-smoke tests
- run-product-smoke.ps1, PRODUCT_SMOKE_SCENARIOS.md and target project files
- request pipeline service and asset request builder for a real duplicate-path regression fixture

Patterns applied:
- existing provider-plan readiness aggregation remains the materialization gate
- stable application-layer diagnostics retain their original machine-readable codes
- fulfillment scanning uses safe relative paths plus output-root containment
- deterministic System.Text.Json camelCase envelopes with stable ordinal ordering

Implemented:
- removed LastWriteTimeUtc from all fulfillment JSON-facing entries without replacement
- preserved available-file detection and fileSizeBytes for valid .png, .wav and .lua outputs
- unsafe rooted/traversal/backslash/colon paths are rejected without leaking those paths into fulfillment JSON
- empty files and directory outputs now emit fulfillment_state.invalid_existing_output errors
- fulfillment diagnostics are written to production/fulfillment-state.json and export-validation.json
- duplicate expected output path regression proves provider-plan blocking and diagnostic materialization
- fixed local indentation regressions in UnityArchiveMaterializationService.cs and run-product-smoke.ps1
- synchronized CURRENT_GENERATOR_STATE.md/json on Product Slice 022 completion

Checks run before final guards:
- UnityArchiveFulfillment filtered tests: passed, 13 tests
- UnityArchiveProviderJob filtered tests: passed, 5 tests
- UnityArchiveMaterialization filtered tests: passed, 7 tests
- unity-archive-provider-job-plan product smoke: passed
- unity-archive-fulfillment-state product smoke: passed
- ProductSmoke filtered tests: passed, 21 tests
- check-devflow-state.ps1: passed, STOP_REVIEW preserved
- check-all.ps1: passed, build 0 warnings/0 errors and 582 tests passed

Forbidden scope preserved:
- no Unity project or implementation
- no Runtime, WinForms, GamePackage schema, generator-library, solution or project changes
- no provider, generator, LLM or Lua execution
- no git commands
