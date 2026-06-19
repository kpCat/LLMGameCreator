# CURRENT_RUN.md

Task id: PRODUCT_SLICE_005_GENERATED_PACKAGE_RUNTIME_PREVIEW
Goal: make assembled generated package content visible in Runtime Preview and cover it with headless runtime-preview smoke
Task source: docs/agent-tasks/NEXT_PRODUCT_SLICE/005_GENERATED_PACKAGE_RUNTIME_PREVIEW.md

Source docs read:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/ROADMAP_TO_FULL_GENERATOR.md
- docs/RUNTIME_MODEL.md
- docs/WINFORMS_DESIGNER_RULES.md
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/PRODUCT_SLICE_005_GENERATED_PACKAGE_RUNTIME_PREVIEW.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/005_CODEX_PROMPT.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/005_GENERATED_PACKAGE_RUNTIME_PREVIEW.md
- src/LLMGameCreator.WinForms/LLMGameCreator.WinForms.csproj
- src/LLMGameCreator.Runtime/LLMGameCreator.Runtime.csproj
- tests/LLMGameCreator.Tests/LLMGameCreator.Tests.csproj

Source files read:
- src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.cs
- src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.Designer.cs
- src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimeMapCanvas.cs
- src/LLMGameCreator.Runtime/DefaultGameRuntime.cs
- src/LLMGameCreator.Runtime.Abstractions/RuntimeContracts.cs
- src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
- src/LLMGameCreator.WinForms/CompositionRoot.cs
- src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPageControl.cs
- src/LLMGameCreator.WinForms/Pages/ArtifactReview/ArtifactReviewPageControl.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyService.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyModels.cs
- tests/LLMGameCreator.Tests/ProductSmoke/BaselineStrictArtifactsPackageAssemblySmokeTests.cs
- tests/LLMGameCreator.Tests/Design/GeneratorPlanGamePackageAssemblyPipelineTests.cs
- tests/LLMGameCreator.Tests/RuntimeUnifiedBridgeTests.cs
- tests/LLMGameCreator.Tests/Docs/CurrentGeneratorStateDocsTests.cs
- .devflow/scripts/run-product-smoke.ps1

Existing patterns inspected:
- `DefaultGameRuntime` remains the existing map runtime path and returns `CommandResult` events without generated-content responsibility.
- `RuntimePreviewPageControl` is the existing WinForms page seam for start/command/log/canvas behavior.
- `GamePackageDefinition.GeneratedContent` already stores profile, scenes, quests, mechanics and applied artifact provenance.
- `BaselineStrictArtifactsPackageAssemblySmokeTests` provides deterministic approved-artifact fixtures for the four sampled strict contracts.
- `CapabilityPickerPageControl` and `ArtifactReviewPageControl` provide the safe `SplitContainer.SizeChanged` initialization pattern.
- `CompositionRoot` uses singleton service registration and `RegisterDelegate` page construction.

Files changed:
- src/LLMGameCreator.Application/RuntimePreview/GeneratedPackageRuntimePreviewService.cs
- src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.cs
- src/LLMGameCreator.WinForms/Pages/RuntimePreview/RuntimePreviewPageControl.Designer.cs
- src/LLMGameCreator.WinForms/CompositionRoot.cs
- tests/LLMGameCreator.Tests/ProductSmoke/GeneratedPackageRuntimePreviewSmokeTests.cs
- .devflow/scripts/run-product-smoke.ps1
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- .devflow/CURRENT_RUN.md

Implemented:
- Added `GeneratedPackageRuntimePreviewService`, a read-only projection from `GamePackageDefinition` plus optional `GameState`.
- Projection exposes package title/description, current map, current generated scene, generated profile, quests, mechanics, applied provenance and warnings.
- Runtime Preview now has right-side tabs for `Log` and `Generated Content`.
- Runtime Preview refreshes generated content after Start and player commands.
- Runtime Preview appends start-summary messages for scene, scene description, quest count and mechanic count.
- Runtime Preview split initialization now follows the safe SizeChanged pattern and avoids hard startup `SplitterDistance`.
- Added headless `GeneratedPackageRuntimePreviewSmoke` over fixture approved artifacts, package assembly/export, `DefaultGameRuntime.Start`, projection assertions and movement.
- Added product-smoke scenario `generated-package-runtime-preview`.
- Updated product smoke docs and current generator state handoff to Product Slice 005.

Non-goals preserved:
- No Unity changes.
- No Lua/script execution.
- No generator-library changes.
- No LLM/provider/LM Studio calls.
- No solution or csproj edits.
- No `DefaultGameRuntime` rewrite.
- No runtime mutation of `GamePackageDefinition`.

Checks planned:
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~RuntimePreview"
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~GeneratedPackageRuntimePreviewSmoke"
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-runtime-preview
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
- mojibake marker scan over changed files

Checks run:
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~RuntimePreview": passed. 1 passed, 0 failed.
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~GeneratedPackageRuntimePreviewSmoke": passed. 1 passed, 0 failed.
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke": passed. 3 passed, 0 failed.
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly: passed. 2 passed, 0 failed. Run directory: .devflow\runs\20260619_174754-product-smoke.
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-runtime-preview: passed. 1 passed, 0 failed. Run directory: .devflow\runs\20260619_174803-product-smoke.
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1: passed. Output: "Devflow state check passed. Current mode: stop (STOP_REVIEW). Tasks: 9. Known warnings: 2."
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1: passed. Build: 0 warnings, 0 errors. Tests: 465 passed, 0 failed. Run directory: .devflow\runs\20260619_174818-check-all.
- Mojibake marker scan over changed files with rg: passed, no markers found.

Manual verification:
- Manual UI verification not run yet. Expected: open Runtime Preview, press Start, confirm map/log/generated-content tab, move once and confirm generated content remains populated.
