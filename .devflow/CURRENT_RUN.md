# CURRENT_RUN.md

Task id: PRODUCT_SLICE_004_HEADLESS_PRODUCT_SMOKE_RUNNER
Goal: automate the baseline strict approved-artifact package assembly/export flow through a headless product smoke runner
Task source: docs/agent-tasks/NEXT_PRODUCT_SLICE/004_HEADLESS_PRODUCT_SMOKE_RUNNER.md

Source docs read:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/ROADMAP_TO_FULL_GENERATOR.md
- docs/M4_1_REAL_EVALUATION_GATE_REPORT.md
- docs/PRODUCT_SLICE_003_ARTIFACT_REVIEW_APPLY_PACKAGE_ASSEMBLY.md
- docs/PRODUCT_SLICE_004_HEADLESS_PRODUCT_SMOKE_RUNNER.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/004_CODEX_PROMPT.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/004_HEADLESS_PRODUCT_SMOKE_RUNNER.md
- docs/PRODUCT_SMOKE_SCENARIOS.md
- tests/LLMGameCreator.Tests/LLMGameCreator.Tests.csproj

Source files read:
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyService.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyValidator.cs
- src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
- src/LLMGameCreator.Infrastructure/Storage/JsonGamePackageRepository.cs
- tests/LLMGameCreator.Tests/Design/GeneratorPlanGamePackageAssemblyPipelineTests.cs
- tests/LLMGameCreator.Tests/Design/GeneratorPlanPackageExportRunTests.cs
- tests/LLMGameCreator.Tests/Docs/CurrentGeneratorStateDocsTests.cs
- .devflow/scripts/_common.ps1
- .devflow/scripts/check-all.ps1
- .devflow/scripts/check-devflow-state.ps1
- .devflow/scripts/README.md

Existing patterns inspected:
- `GeneratorPlanGamePackageAssemblyService` is the application seam for approved artifact set -> draft package assembly -> optional package export.
- `GeneratorPlanGamePackageAssembler` already maps `game_profile_v1`, `scene_pack_v1`, `quest_pack_v1` and `mechanics_pack_v1` into baseline package/generatedContent/provenance.
- `JsonGamePackageRepository.SaveAsync` is the existing package JSON export convention.
- `.devflow/scripts/check-all.ps1` and `_common.ps1` provide the run-directory, UTF-8 environment and logged-command script pattern.
- Existing package assembly/export tests use temporary folders and focused xUnit assertions rather than UI automation or provider calls.

Files changed:
- tests/LLMGameCreator.Tests/ProductSmoke/BaselineStrictArtifactsPackageAssemblySmokeTests.cs
- .devflow/scripts/run-product-smoke.ps1
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- .devflow/CURRENT_RUN.md

Implemented:
- Added deterministic ProductSmoke approved artifact fixture for all four M4.1 baseline contracts: `game_profile_v1`, `scene_pack_v1`, `quest_pack_v1`, `mechanics_pack_v1`.
- Added focused headless smoke tests that run real package assembly/export through `GeneratorPlanGamePackageAssemblyService` and `JsonGamePackageRepository`.
- The smoke asserts exported `package.json`, populated manifest/profile/scenes/quests/mechanics, all four applied artifact provenance records, content hashes and no package-blocking diagnostics.
- Added `.devflow/scripts/run-product-smoke.ps1` for the scenario `baseline-strict-package-assembly`.
- The script writes `.devflow/runs/<timestamp>-product-smoke/product-smoke-summary.md`, `product-smoke-summary.json`, `test-results/` and `package-output/package.json`.
- Added `docs/PRODUCT_SMOKE_SCENARIOS.md`.
- Updated current generator state handoff to Product Slice 004 while keeping `current_phase = m4_1_real_model_evaluation_gate` and `last_completed_milestone = M4.1`.

Non-goals preserved:
- No Runtime, Scripting/Lua, WinForms, generator-library, solution or csproj changes.
- No provider, OpenAI-compatible API, LM Studio, repair prompt, UI automation or runtime preview calls.
- No broad schema rewrite and no M5/M6/M6-lite unlock.

Checks planned:
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Package"
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
- mojibake marker scan over changed files

Checks run:
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke": passed. 2 passed, 0 failed.
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Package": passed. 90 passed, 0 failed.
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly: passed. Run directory: .devflow\runs\20260619_164636-product-smoke. Package output: .devflow\runs\20260619_164636-product-smoke\package-output\package.json.
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1: passed. Output: "Devflow state check passed. Current mode: stop (STOP_REVIEW). Tasks: 9. Known warnings: 2."
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1: passed. Build: 0 warnings, 0 errors. Tests: 464 passed, 0 failed. Run directory: .devflow\runs\20260619_164652-check-all.
- Mojibake marker scan over changed files with rg: passed, no markers found.

Manual verification:
- Manual UI verification not required for this headless smoke slice.
