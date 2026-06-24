Task id: PRODUCT_SLICE_033_VISIBLE_GENERATED_PLAYABLE_PREVIEW
Goal: Expose the S032 generated package MVP through the smallest existing preview/runtime projection path and hand off to manual user preview verification

Read-first sources:
- AGENTS.md, README.md, docs/CONTEXT_INDEX.md and current-state pair
- docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md and docs/ROADMAP_TO_FULL_GENERATOR.md
- Product Slice 029, 030, 031, 032 and 033 task docs
- generation policy, full generator master plan, game system taxonomy
- Application/Runtime/Test csproj files
- S029/S030/S031/S032 procedural services and tests
- GeneratedPackageRuntimePreviewService and GeneratedPackageRuntimePreviewSmokeTests
- DefaultGameRuntime and Runtime.Abstractions contracts
- product smoke runner and product smoke analogs

Implemented:
- S032 provenance/hash cleanup: GeneratedPackageMvp report now distinguishes final package hash from pre-provenance content hash
- deterministic VisibleGeneratedPlayablePreview service in Application/RuntimePreview
- runtime adapter contract so Application can avoid a direct Runtime implementation dependency
- reuse of GeneratedPackageRuntimePreviewService projection
- focused VisibleGeneratedPlayablePreview tests
- visible-generated-playable-preview product smoke route
- deterministic `.llmgc/procedural/visible-generated-playable-preview/visible-generated-playable-preview-snapshot.json`
- deterministic `.llmgc/procedural/visible-generated-playable-preview/visible-generated-playable-preview-report.json`
- deterministic `.llmgc/procedural/visible-generated-playable-preview/visible-generated-playable-preview-report.md`
- deterministic `.llmgc/procedural/visible-generated-playable-preview/manual-verification.md`
- docs/MANUAL_VISIBLE_GENERATED_PLAYABLE_PREVIEW_CHECK.md
- README, CONTEXT_INDEX and current-state pair updated to recommend manual_user_preview_verification next

Verification:
- GeneratedPackageMvp filtered tests: 5/5 passed
- VisibleGeneratedPlayablePreview filtered tests: 4/4 passed
- visible-generated-playable-preview product smoke: 1/1 passed
- check-all.ps1: 678/678 tests passed, build 0 warnings / 0 errors

Forbidden scope preserved:
- M5/M6 remain Locked
- no WinForms UI, provider execution, LLM calls, Lua execution, Unity work or media generation
- no broad GamePackage schema redesign
- no public runtime command/state contract redesign
- no git commands
