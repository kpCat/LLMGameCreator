# Goal 102A — Unity Editor Source Format Repair & Guard Backstop

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`
Codex reasoning: very high

## Objective

Repair the Goal102 source-format regression and strengthen the relevant quality guard.

Audit finding after Goal102:
`unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs` is functionally present but physically minified / one physical line in raw source view. This is not acceptable for repository source health even if tests are GREEN.

This is a bounded hotfix. Do not add new features. Do not change behavior except formatting/source-health evidence.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes `c6f16eb GREEN Goal 102 offline geoworld Unity editor preview tool`.
4. Confirm Goal102 artifacts exist and remain `accepted=false`.
5. Inspect current raw source formatting for:
   - `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs`
   - `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewRunner.cs`
   - `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPrimitiveFactory.cs`
   - `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewTravelWindow.cs`
   - Goal102 Application namespace
   - Goal102 VisualWorldStreamPreviewWorkspace files
6. Record `AlphaRuntimeBootstrap.cs` hash/line count before work and do not modify it.
7. Inspect dirty state. Do not stage/revert unrelated user work.

## Read first

- `AGENTS.md`
- `docs/GOAL_PRODUCTIVITY_POLICY.md`
- `docs/VALIDATION_PIPELINE.md`
- `.llmgc/procedural/goal-102-offline-geoworld-unity-editor-preview-tool/offline-geoworld-unity-editor-preview-tool-report.md`
- `.llmgc/procedural/goal-102-offline-geoworld-unity-editor-preview-tool/offline-geoworld-unity-editor-quality-gate-scan.json`
- `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
- docs quartet and debt register.

## Allowed files / areas

- `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs`
- `src/LLMGameCreator.Application/Design/OfflineGeoworldUnityEditorPreviewTool/`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldUnityEditorPreviewTool/`
- `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldUnityEditorPreviewToolProductSmokeTests.cs`
- `.llmgc/procedural/goal-102a-unity-editor-source-format-guard/`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.devflow/artifact-scope/artifact-scope-policy.json`
- `docs/agent-tasks/goal-102a-unity-editor-source-format-guard/`

## Forbidden files / areas

Do not change Runtime, public GamePackage schema, providers, Lua, generator-library, `.sln`, `.csproj`, lock files, `AlphaRuntimeBootstrap.cs`, Unity scenes/prefabs/asmdefs/settings/packages/build settings, StreamingAssets payloads, binary/raster media, real geodata, network code, LFZ archive/source, or external dependencies.

No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Reformat Unity editor source

Rewrite `OfflineGeoworldPreviewWindow.cs` into normal readable C# formatting:
- normal using directives on separate lines;
- namespace/type/method blocks with indentation;
- one statement per line where practical;
- preserve behavior, menu path, payload path and public methods;
- keep `#if UNITY_EDITOR` wrapper if desired;
- no Unity scene/project changes.

### 2. Add/strengthen source-format scanner

In the Goal102 Application evidence namespace, add or extend a scanner that checks physical/raw formatting for the relevant Goal102 Unity and Application files.

It must detect:
- zero-LF files;
- CR-only files;
- one-physical-line C# files with multiple statements;
- extreme physical line length;
- minified source markers such as many `{`, `;` or `using` tokens on one physical line;
- files over 700 logical lines or 1000 logical lines.

The scanner must be reusable by tests and evidence.

### 3. Generate Goal102A evidence

Create `.llmgc/procedural/goal-102a-unity-editor-source-format-guard/` with:

- `unity-editor-source-format-guard-report.md`
- `unity-editor-source-format-scan-before-after.json`
- `unity-editor-source-format-quality-gate.json`
- `unity-editor-source-format-negative-proof.json`

Evidence must show:
- before finding for `OfflineGeoworldPreviewWindow.cs` as malformed/minified/one-line;
- after finding as repaired;
- all scanned Goal102 relevant C# files pass;
- `AlphaRuntimeBootstrap.cs` unchanged;
- no forbidden areas changed.

### 4. Negative proof

Reject at least:
- one-line multi-statement C# file;
- zero-LF C# file;
- CR-only C# file;
- extreme physical line length;
- fake pass without reading file bytes;
- attempt to modify AlphaRuntimeBootstrap;
- Unity scene/project setting changed marker.

### 5. Tests

Focused tests:
- scanner rejects synthetic one-line C#;
- scanner rejects zero-LF/CR-only/extreme line;
- scanner accepts repaired `OfflineGeoworldPreviewWindow.cs`;
- evidence is deterministic;
- `AlphaRuntimeBootstrap.cs` unchanged.

Product smoke:
- build Goal102A evidence from repo root;
- verify before/after scan exists;
- verify repaired file has multiple physical lines and normal max physical line length;
- verify no forbidden areas.

### 6. Docs/state

Update docs quartet and debt register.

Manual gate:
`unity_editor_source_format_guard_verification required`

Status:
`accepted=false`.

Record this as a P0/P1 source-health repair after Goal102.

## Validation

Use Goal089 tiered validation:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter OfflineGeoworldUnityEditorPreviewTool
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter OfflineGeoworldUnityEditorPreviewToolProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-102a-unity-editor-source-format-guard" -FocusedFilter "OfflineGeoworldUnityEditorPreviewTool" -ProductSmokeFilter "OfflineGeoworldUnityEditorPreviewToolProductSmokeTests"
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-102a-unity-editor-source-format-guard"
git diff --check
git diff --cached --check
```

Full check-all is not required unless current-goal/spine-fast indicates shared/core risk.

## Quality gate

GREEN only if:
- `OfflineGeoworldPreviewWindow.cs` is physically readable, multi-line, and not minified;
- scanner detects the original failure class;
- all relevant scanned C# files pass;
- no forbidden files changed;
- AlphaRuntimeBootstrap unchanged;
- no behavior regression in Goal102 focused/product smoke;
- artifact scope passes;
- final worktree clean.

## Commit/push

Always commit and push to `origin/main`.

Commit message:
- `GREEN Goal 102A Unity editor source format guard`
- `BLOCKED Goal 102A Unity editor source format guard`
- `FAILED Goal 102A Unity editor source format guard`
