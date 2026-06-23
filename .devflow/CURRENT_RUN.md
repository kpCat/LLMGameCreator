Task id: PRODUCT_SLICE_024_1_UNITY_ARCHIVE_REVIEW_HISTORY_ORDERING_CLEANUP
Goal: Fix S024 archive review history ordering semantics and diagnostics completeness

Source docs/code read:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/PRODUCT_SMOKE_SCENARIOS.md
- .devflow/scripts/run-product-smoke.ps1
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryModels.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryService.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonModels.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonService.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonMarkdownRenderer.cs
- tests/LLMGameCreator.Tests/Application/UnityArchiveReviewHistoryTests.cs
- tests/LLMGameCreator.Tests/Application/UnityArchiveReviewComparisonTests.cs
- tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveReviewHistorySmokeTests.cs

Implemented files:
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryModels.cs (modified: added Sequence property)
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryService.cs (modified: sequence-based ordering, migration support)
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonService.cs (modified: sequence-based previous selection, diagnostics)

Expected checks:
- UnityArchiveReviewHistory and UnityArchiveReviewComparison filtered tests: 23/23 passed
- unity-archive-review-history product smoke: 1/1 passed
- ProductSmoke filtered tests: 23/23 passed
- check-devflow-state.ps1: passed
- check-all.ps1: passed 611/611 tests, build 0 warnings

Forbidden scope preserved:
- no Unity project or implementation
- no Runtime, WinForms, GamePackage schema, generator-library, solution or project changes
- no provider, generator, LLM or Lua execution
- no git commands