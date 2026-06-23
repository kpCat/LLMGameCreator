You are Kilo Code working in the LLMGameCreator repository.

Implement Product Slice 024.1: Unity Archive Review History Ordering Cleanup.

Follow this task exactly:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/024_1_UNITY_ARCHIVE_REVIEW_HISTORY_ORDERING_CLEANUP.md
```

Hard constraints:

- Work in the current working tree only.
- Do not run git commands.
- Do not create/switch branches.
- Do not merge/rebase/cherry-pick/push.
- Use repo-relative paths and normal Windows/PowerShell paths only.
- Do not use `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt`, or container paths.
- Do not change Runtime, WinForms, GamePackage schema, generator-library, `.sln`, or `*.csproj`.
- Do not implement Unity.
- Do not execute providers, LLMs, generators, Lua, ComfyUI, Suno, or Runtime gameplay.
- Preserve M5/M6 Locked semantics in current state docs.

Read first:

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/PRODUCT_SLICE_024_UNITY_ARCHIVE_REVIEW_RETENTION_COMPARISON.md
.devflow/scripts/run-product-smoke.ps1
src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonService.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveReviewHistoryTests.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveReviewComparisonTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveReviewHistorySmokeTests.cs
```

Primary repair:

- Add deterministic `Sequence`/ordinal to history snapshot entries.
- Store snapshots in insertion sequence order, not hash order.
- Make comparison choose previous snapshot by sequence, not lexicographic hash ordering.
- Add diagnostics for missing/invalid review/index/snapshot/current-not-indexed cases.
- Keep existing output paths and product smoke scenario id.

Required checks:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveReviewHistory|FullyQualifiedName~UnityArchiveReviewComparison"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-review-history
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Final report in Russian.
