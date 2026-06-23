You are Kilo Code working in the LLMGameCreator repository.

Implement Product Slice 024: Unity Archive Review Retention & Comparison v1.

Follow this task exactly:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/024_UNITY_ARCHIVE_REVIEW_RETENTION_COMPARISON.md
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
.devflow/scripts/run-product-smoke.ps1
src/LLMGameCreator.Application/Composition/UnityArchiveReviewSnapshotModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewSnapshotService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewSnapshotMarkdownRenderer.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveReviewSnapshotTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveReviewSnapshotSmokeTests.cs
```

Implement deterministic content-hash snapshot retention and current-vs-previous comparison over `production/archive-review.json`.

Required checks:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveReviewHistory|FullyQualifiedName~UnityArchiveReviewComparison"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-review-history
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Final report in Russian.
