You are Codex working in the LLMGameCreator repository.

Implement Product Slice 026: Controlled Manual Provider Output Import v1 + Archive Review UX Snapshot Detail.

Follow this task exactly:

```text
docs/agent-tasks/NEXT_PRODUCT_SLICE/026_CONTROLLED_MANUAL_PROVIDER_OUTPUT_IMPORT.md
```

Hard constraints:

- Work in the current working tree only.
- Do not run git commands.
- Do not create/switch branches.
- Do not merge/rebase/cherry-pick/push.
- Use repo-relative paths and normal Windows/PowerShell paths only.
- Do not use `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt`, or container paths.
- Do not change Runtime, GamePackage schema, generator-library, `.sln`, or `*.csproj`.
- Do not implement Unity.
- Do not execute providers, LLMs, generators, Lua, ComfyUI, Suno, Unity, or Runtime gameplay.
- Preserve M5/M6 Locked semantics in current state docs.

Primary implementation:

1. New Application-layer `UnityArchiveManualProviderImportService` and models.
2. Controlled manifest-based import from `.llmgc/unity-archive/manual-import/` into existing expected output slots.
3. Deterministic import JSON/Markdown reports.
4. Refresh fulfillment/review/history/comparison after import if possible without provider/generator execution.
5. UX polish: existing `Unity Archive Review` page must show selected history snapshot JSON and manual import report tabs.

Read first:

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/PRODUCT_SMOKE_SCENARIOS.md
src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveProviderJobPlanModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveFulfillmentStateService.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPresenter.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPageControl.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPageControl.Designer.cs
```

Required checks:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ManualProviderImport|FullyQualifiedName~UnityArchiveReview"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-manual-provider-import
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Final report in Russian.
