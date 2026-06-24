You are Codex working in the LLMGameCreator repository.

Implement Product Slice 025: Read-only Archive Review/History UI.

Follow this task exactly:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/025_READ_ONLY_ARCHIVE_REVIEW_HISTORY_UI.md
```

This is a WinForms UI slice. Use Codex-level care.

Hard constraints:

- Work in the current working tree only.
- Do not run git commands.
- Do not create/switch branches.
- Do not merge/rebase/cherry-pick/push.
- Use repo-relative paths and normal Windows/PowerShell paths only.
- Do not use `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt`, or container paths.
- Do not change Runtime, GamePackage schema, Application archive services/models, generator-library, `.sln`, or `*.csproj`.
- Do not implement Unity.
- Do not execute providers, LLMs, generators, Lua, ComfyUI, Suno, Unity, or Runtime gameplay.
- Preserve M5/M6 Locked semantics in current state docs.

Read first:

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/PRODUCT_SMOKE_SCENARIOS.md

src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/CompositionWorkbenchPageControl.cs
src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/CompositionWorkbenchPageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/CompositionWorkbenchPresenter.cs
src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/CompositionWorkbenchViewState.cs

tests/LLMGameCreator.Tests/WinForms/CompositionWorkbenchPresenterTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/CompositionWorkbenchReadonlySmokeTests.cs

src/LLMGameCreator.Application/Composition/UnityArchiveReviewSnapshotModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonModels.cs
```

Primary implementation:

- New read-only editor page `UnityArchiveReviewPageControl`.
- New WinForms presenter/view state for reading existing S023/S024/S024.1 report files.
- Register page in `CompositionRoot` and `EditorPageRegistry`.
- Show current review markdown/json, comparison markdown/json and history index/snapshot list.
- Handle missing project/archive/files/invalid JSON without throwing.
- Add tests and product smoke.
- Update current state docs after all checks pass.

Required checks:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ArchiveReview|FullyQualifiedName~UnityArchiveReview"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~WinForms"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Final report in Russian.
