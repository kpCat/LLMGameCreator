Task id: PRODUCT_SLICE_024_UNITY_ARCHIVE_REVIEW_RETENTION_COMPARISON_V1
Goal: Implement deterministic content-hash snapshot retention and comparison for Unity archive review

Source docs/code read:
- AGENTS.md
- docs/CONTEXT_INDEX.md
- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/PRODUCT_SMOKE_SCENARIOS.md
- .devflow/scripts/run-product-smoke.ps1
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewSnapshotModels.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewSnapshotService.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewSnapshotMarkdownRenderer.cs
- tests/LLMGameCreator.Tests/Application/UnityArchiveReviewSnapshotTests.cs
- tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveReviewSnapshotSmokeTests.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationService.cs
- src/LLMGameCreator.Application/Composition/UnityArchiveFulfillmentStateModels.cs

Implemented files:
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryModels.cs (new)
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryService.cs (new)
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonModels.cs (new)
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonService.cs (new)
- src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonMarkdownRenderer.cs (new)
- tests/LLMGameCreator.Tests/Application/UnityArchiveReviewHistoryTests.cs (new)
- tests/LLMGameCreator.Tests/Application/UnityArchiveReviewComparisonTests.cs (new)
- tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveReviewHistorySmokeTests.cs (new)
- .devflow/scripts/run-product-smoke.ps1 (updated with scenario)
- docs/PRODUCT_SMOKE_SCENARIOS.md (updated with scenario docs)
- docs/CURRENT_GENERATOR_STATE.md (updated with S024 state)
- docs/CURRENT_GENERATOR_STATE.json (updated with S024 state)

Expected checks:
- UnityArchiveReviewHistory and UnityArchiveReviewComparison filtered tests: 18/18 passed
- unity-archive-review-history product smoke: 1/1 passed
- ProductSmoke filtered tests: 23/23 passed
- check-devflow-state.ps1: passed
- check-all.ps1: passed 606/606 tests

Forbidden scope preserved:
- no Unity project or implementation
- no Runtime, WinForms, GamePackage schema, generator-library, solution or project changes
- no provider, generator, LLM or Lua execution
- no git commands