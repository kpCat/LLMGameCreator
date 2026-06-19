# CURRENT_RUN.md

Task id: PRODUCT_SLICE_003_ARTIFACT_REVIEW_APPLY_PACKAGE_ASSEMBLY
Goal: turn persisted Artifact Review approved baseline artifacts into inspectable draft GamePackage assembly without provider, Lua, runtime or generator-library execution
Task source: docs/agent-tasks/NEXT_PRODUCT_SLICE/003_ARTIFACT_REVIEW_APPLY_PACKAGE_ASSEMBLY.md

Source docs read:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/ROADMAP_TO_FULL_GENERATOR.md
- docs/M4_1_REAL_EVALUATION_GATE_REPORT.md
- docs/PRODUCT_SLICE_003_ARTIFACT_REVIEW_APPLY_PACKAGE_ASSEMBLY.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/003_CODEX_PROMPT.md
- docs/agent-tasks/NEXT_PRODUCT_SLICE/003_ARTIFACT_REVIEW_APPLY_PACKAGE_ASSEMBLY.md
- docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
- docs/GENERATOR_PLAN_ARTIFACT_REVIEW_UI.md
- docs/GENERATOR_PLAN_ONE_CLICK_PACKAGE_EXPORT_UI.md
- src/LLMGameCreator.Application/LLMGameCreator.Application.csproj
- src/LLMGameCreator.WinForms/LLMGameCreator.WinForms.csproj
- tests/LLMGameCreator.Tests/LLMGameCreator.Tests.csproj
- src/LLMGameCreator.GamePackage/LLMGameCreator.GamePackage.csproj

Source files read:
- src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
- src/LLMGameCreator.Domain/Definitions/GameDefinitions.cs
- src/LLMGameCreator.Application/Validation/GamePackageValidator.cs
- src/LLMGameCreator.Application/Validation/GameDefinitionValidator.cs
- src/LLMGameCreator.Application/Projects/CurrentGamePackageService.cs
- src/LLMGameCreator.Infrastructure/Storage/JsonGamePackageRepository.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanDraftArtifactReviewService.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanDraftArtifactApprovalModels.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanDraftArtifactApprovalArtifactModels.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanDraftArtifactApprovalArtifactService.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanDraftArtifactApprovalArtifactReader.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanApprovedArtifactSetReader.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactContractCatalog.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactValidator.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyService.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyModels.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyDiagnostics.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyValidator.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyMarkdownRenderer.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyArtifactModels.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyArtifactService.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyArtifactReader.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanPackageExportRunService.cs
- src/LLMGameCreator.WinForms/Pages/ArtifactReview/ArtifactReviewPageControl.cs
- src/LLMGameCreator.WinForms/Pages/ArtifactReview/ArtifactReviewPresenter.cs
- src/LLMGameCreator.WinForms/Pages/ArtifactReview/ArtifactReviewViewModels.cs
- src/LLMGameCreator.WinForms/Pages/PackageExport/PackageExportPageControl.cs
- src/LLMGameCreator.WinForms/Pages/PackageExport/PackageExportRunPresenter.cs
- src/LLMGameCreator.WinForms/CompositionRoot.cs
- tests/LLMGameCreator.Tests/Design/GeneratorPlanGamePackageAssemblyPipelineTests.cs
- tests/LLMGameCreator.Tests/Design/GeneratorPlanPackageExportRunTests.cs
- tests/LLMGameCreator.Tests/WinForms/ArtifactReviewPresenterTests.cs
- tests/LLMGameCreator.Tests/Docs/CurrentGeneratorStateDocsTests.cs

Existing patterns inspected:
- Package Export already runs approval -> assembly -> assembly artifact save -> final run artifact save and exports `package.json` through `JsonGamePackageRepository`.
- Artifact Review already persists human decisions into `artifact/generator_plan_approved_artifact_set/latest`; package assembly should read that approved set instead of recapturing or auto-approving.
- GamePackage assembly already maps legacy draft artifacts through `GeneratorPlanGamePackageAssembler`, validates through `GamePackageValidator`, and saves assembly/package draft/markdown artifacts through existing generated artifact storage.
- WinForms pages in this area use runtime-safe constructors plus in-file TableLayoutPanel layout and event wiring; DI remains in `CompositionRoot`.

Files changed:
- src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyModels.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyDiagnostics.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyService.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyValidator.cs
- src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyMarkdownRenderer.cs
- src/LLMGameCreator.WinForms/Pages/ArtifactReview/ArtifactReviewPageControl.cs
- src/LLMGameCreator.WinForms/CompositionRoot.cs
- tests/LLMGameCreator.Tests/Design/GeneratorPlanGamePackageAssemblyPipelineTests.cs
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- .devflow/CURRENT_RUN.md

Implemented:
- Added minimal non-breaking `generatedContent` section to draft GamePackage for generated profile summaries, scene summaries, quest summaries, mechanic summaries, applied artifact provenance and preserved unknown artifact JSON.
- Extended GamePackage assembly so baseline strict artifacts preserve core loop, pillars, source context, scene descriptions/purposes, quest steps/objectives, mechanic descriptions/tags and per-artifact provenance.
- Added content hash, applied timestamp, contract id, artifact id, capability selection id and mapping result provenance for applied approved artifacts.
- Preserved unknown/semantic approved artifacts as generated-content preserved artifacts with raw JSON when valid, while reporting warnings instead of crashing.
- Added assembly diagnostics for artifact kind mismatch, duplicate generated scene/quest/mechanic ids, generated package title presence, provenance presence and preserved raw JSON validity.
- Added `AppliedArtifactCount` and `SkippedArtifactCount` to assembly summary and markdown report.
- Added Artifact Review action `Apply approved to package` that reads the persisted latest approved artifact set, assembles/validates/exports a draft package to `.llmgc/package-assembly/package.json` by default, saves assembly/package draft/markdown artifacts, and replaces current in-memory package state with the draft package.
- Kept approval persistence as the gate: unsaved yellow UI decisions are not assembled until `Apply selected decisions` writes the approved artifact set.
- Updated current generator state handoff to Product Slice 003 while keeping `current_phase = m4_1_real_model_evaluation_gate` and `last_completed_milestone = M4.1`.

Non-goals preserved:
- No provider/LLM/LM Studio calls in apply or assembly.
- No Lua, scripting, runtime, runtime preview, Unity or generator-library implementation changes.
- No solution, project, NuGet, devflow script, NEXT_TASK or task queue changes.
- No broad future artifact contract expansion.
- No destructive package schema rewrite; new GamePackage section is default-empty and non-breaking.

Checks run:
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Artifact": passed. 138 passed, 0 failed.
- dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Package": passed. 88 passed, 0 failed.
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1: passed. Output: "Devflow state check passed. Current mode: stop (STOP_REVIEW). Tasks: 9. Known warnings: 2."
- powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1: passed. Build: 0 warnings, 0 errors. Tests: 460 passed, 0 failed. Run directory: .devflow\runs\20260619_150606-check-all.
- Mojibake marker scan over changed files with rg: passed, no markers found.

Manual verification:
- Not run interactively in this note. Required manual UI workflow remains: start WinForms, open project, produce/stage strict baseline artifacts, Artifact Review -> Load latest -> Approve all valid -> Apply selected decisions -> Apply approved to package -> inspect `.llmgc/package-assembly/package.json` and report/status text.
