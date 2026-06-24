Task id: PRODUCT_SLICE_025_READ_ONLY_ARCHIVE_REVIEW_HISTORY_UI
Goal: Add a bounded read-only WinForms page for existing Unity archive review/history/comparison reports

Source docs/code read:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/PRODUCT_SMOKE_SCENARIOS.md
- docs/WINFORMS_DESIGNER_RULES.md
- .devflow/scripts/run-product-smoke.ps1
- src/LLMGameCreator.WinForms/CompositionRoot.cs
- src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/*
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewSnapshotModels.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryModels.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonModels.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryService.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonService.cs
- tests/LLMGameCreator.Tests/WinForms/CompositionWorkbenchPresenterTests.cs
- tests/LLMGameCreator.Tests/ProductSmoke/CompositionWorkbenchReadonlySmokeTests.cs
- tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveReviewHistorySmokeTests.cs

Implemented files:
- src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/* (added: presenter, view state, page and Designer layout)
- src/LLMGameCreator.WinForms/CompositionRoot.cs (modified: presenter/page/registry registration and explicit existing prompt-builder constructor selection)
- tests/LLMGameCreator.Tests/WinForms/UnityArchiveReviewPresenterTests.cs (added)
- tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveReviewReadonlySmokeTests.cs (added)
- .devflow/scripts/run-product-smoke.ps1 (modified: unity-archive-review-ui-readonly scenario)
- docs/PRODUCT_SLICE_025_READ_ONLY_ARCHIVE_REVIEW_HISTORY_UI.md (added)
- docs/PRODUCT_SMOKE_SCENARIOS.md (modified)
- docs/CURRENT_GENERATOR_STATE.md and .json (modified: S025 handoff, M5/M6 remain Locked)

Expected checks:
- ArchiveReview/UnityArchiveReview filtered tests: 37/37 passed
- WinForms filtered tests: 42/42 passed
- ProductSmoke filtered tests: 24/24 passed
- unity-archive-review-ui-readonly product smoke: 1/1 passed
- check-devflow-state.ps1: passed in STOP_REVIEW mode
- check-all.ps1: passed 619/619 tests, build 0 warnings / 0 errors

Forbidden scope preserved:
- no Unity project or implementation
- no Runtime, Application archive service/model, GamePackage schema, generator-library, solution or project changes
- no provider, generator, LLM or Lua execution
- no git commands
