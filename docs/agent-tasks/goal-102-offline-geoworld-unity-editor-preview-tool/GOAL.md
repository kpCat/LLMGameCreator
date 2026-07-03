# Goal 102 — Offline Geoworld Unity Editor Preview Tool & Travel Window Inspector

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`
Codex reasoning: very high

## Primary objective

Deliver a larger, user-visible composite milestone after Goal 101.

Goal 101 created a standalone Unity Alpha preview runner and travel-window demo payload. Goal 102 must make that payload easier to inspect in Unity and from the WinForms workspace, without modifying existing Unity scenes, `AlphaRuntimeBootstrap.cs`, Runtime projects, GamePackage schema, providers, Lua, generator-library, project files, dependencies, or real map fetching.

Required outcome:

1. Unity Editor preview window/tool that can read the Goal101 StreamingAssets payload and instantiate/clear a placeholder preview in the currently open Unity scene on demand.
2. Bounded helper code only where needed; no scene/prefab/project settings edits.
3. WinForms workspace integration that surfaces Unity editor tool readiness, payload path, script inventory and manual launch instructions.
4. Application-side source/readiness evidence.
5. Simulated Unity editor action proof.
6. Negative proof.
7. Focused tests and product smoke.

This is not final gameplay, not final Runtime consumption, not final art and not scene/prefab production.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes:
   - `98b16d78 GREEN Goal 100 offline geoworld visual cache Unity handoff`
   - `708558ae GREEN Goal 101 offline geoworld Unity preview runner`
4. Confirm Goal101 artifacts exist and remain `accepted=false`.
5. Confirm Goal101 report proves 18 preview commands, 10 command kinds, 4 travel-window steps, 5 Unity payload files, standalone Unity preview scripts, simulated command proof passed and AlphaRuntimeBootstrap unchanged.
6. Record `AlphaRuntimeBootstrap.cs` hash/line count before work and do not modify it.
7. Inspect dirty state. Do not stage/revert unrelated user work.

## Read first

- `AGENTS.md`
- `docs/GOAL_PRODUCTIVITY_POLICY.md`
- `docs/VALIDATION_PIPELINE.md`
- `docs/context/LFZ_GEOWORLD_PATTERN_STUDY.md`
- `docs/context/GEOWORLD_RUNTIME_STREAMING_DESIGN_NOTES.md`
- `docs/context/GEOWORLD_SOURCE_ADAPTER_ARCHITECTURE.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner/offline-geoworld-unity-preview-runner-report.md`
- `.llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner/offline-geoworld-preview-command-catalog.json`
- `.llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner/offline-geoworld-preview-travel-window-script.json`
- `.llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner/offline-geoworld-preview-unity-script-inventory.json`
- `.llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner/offline-geoworld-preview-simulated-command-proof.json`
- `.llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner/offline-geoworld-preview-negative-proof.json`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewRunner.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPrimitiveFactory.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewTravelWindow.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs` read-only baseline.
- existing Visual World Stream Preview Workspace Application/WinForms files.

## Allowed files / areas

- New Application namespace:
  - `src/LLMGameCreator.Application/Design/OfflineGeoworldUnityEditorPreviewTool/`
- Existing workspace integration:
  - `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
  - `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
- Tests:
  - `tests/LLMGameCreator.Tests/Application/OfflineGeoworldUnityEditorPreviewTool/`
  - `tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/`
  - `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldUnityEditorPreviewToolProductSmokeTests.cs`
  - `tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs`
- Bounded Unity additions only:
  - `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs`
  - `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewEditorBridge.cs` only if needed as runtime-safe DTO/helper, not as auto-run gameplay.
- Evidence:
  - `.llmgc/procedural/goal-102-offline-geoworld-unity-editor-preview-tool/`
- Docs/state:
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/CONTEXT_INDEX.md`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`
  - `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- Artifact scope:
  - `.devflow/artifact-scope/artifact-scope-policy.json`
- Task pack:
  - `docs/agent-tasks/goal-102-offline-geoworld-unity-editor-preview-tool/`

## Forbidden files / areas

Do not change LFZ archive/source, Runtime, public schema, providers, Lua, generator-library, `.sln`, `.csproj`, lock files, `AlphaRuntimeBootstrap.cs`, Unity scenes/prefabs/asmdefs/settings/packages/build settings, binary/raster media, real geodata dumps, live network fetch code, external dependencies, or prompt dumps. No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Required behavior

### 1. Unity Editor preview window

Add `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs`.

Expected editor behavior:
- menu item such as `LLMGameCreator/Offline Geoworld Preview`;
- reads Goal101 manifest from `Application.streamingAssetsPath/LLMGameCreator/OfflineGeoworldGoal101`;
- shows payload status, command count and travel-window step count;
- has a button/method to create preview objects in the current scene;
- has a button/method to clear preview objects created by the tool;
- uses simple primitives/lines/labels or the existing Goal101 primitive factory where possible;
- does not modify scenes automatically on import;
- does not require external packages;
- does not call network/provider/LLM;
- does not hardcode GREEN without reading files.

If Unity Editor-only code must use `UnityEditor`, keep it under `Assets/Editor`.

### 2. Application readiness service

Create BCL-only Application service that reads Goal101 artifacts and Unity source files and writes readiness evidence:
- editor window script exists;
- menu marker present;
- read manifest path marker present;
- instantiate/clear methods present;
- no network/provider markers;
- no AlphaRuntimeBootstrap dependency;
- no scene/prefab/settings changes;
- expected payload files exist;
- command/travel counts match Goal101.

### 3. Simulated editor action proof

Create deterministic .NET proof:
1. read Goal101 manifest;
2. read command catalog;
3. read travel script;
4. map commands to expected editor preview objects;
5. verify expected object count;
6. verify clear operation model;
7. verify no unsupported command kind;
8. verify no absolute paths, raw geodata, binary/raster media, network/provider markers.

### 4. WinForms workspace integration

Extend Visual World Stream Preview Workspace with an `offline_geoworld_unity_editor_preview` group showing:
- editor window script path;
- menu item marker;
- payload path;
- command count;
- travel-window step count;
- simulated editor action proof status;
- clear/cleanup proof status;
- AlphaRuntimeBootstrap unchanged status;
- diagnostics/manual instructions.

The workspace must read real Goal102 evidence, not hardcoded success.

### 5. Evidence

Create `.llmgc/procedural/goal-102-offline-geoworld-unity-editor-preview-tool/`.

Required artifacts:
- `offline-geoworld-unity-editor-preview-tool-report.md`
- `offline-geoworld-unity-editor-tool-inventory.json`
- `offline-geoworld-unity-editor-simulated-action-proof.json`
- `offline-geoworld-unity-editor-negative-proof.json`
- `offline-geoworld-unity-editor-workspace-binding-inventory.json`
- `offline-geoworld-unity-editor-source-lineage.json`
- `offline-geoworld-unity-editor-quality-gate-scan.json`

### 6. Negative proof

Reject missing Goal101 payload, missing editor window script, missing menu marker, missing clear method, unsupported command kind, network/provider marker in editor script, AlphaRuntimeBootstrap dependency marker, scene/prefab/project settings change marker, fake success without payload read, absolute path, raw geodata leaked into command, binary/raster media marker.

### 7. Tests

Focused tests:
- readiness service builds from repo root;
- Unity editor window inventory passes;
- simulated editor action proof passes;
- negative proof rejects expected scenarios;
- workspace group exists;
- AlphaRuntimeBootstrap unchanged;
- all new/touched C# files stay under source-health limits.

Product smoke:
- build evidence from repo root;
- read `.llmgc` evidence and Unity editor script;
- verify menu marker, payload path markers, create/clear operation markers;
- verify workspace binding inventory;
- verify no Runtime/schema/provider/dependency changes, no scene/prefab changes, no binary/raster media.

### 8. Docs/state

Update docs quartet and debt register.

Manual gate: `offline_geoworld_unity_editor_preview_tool_verification required`

Status: `accepted=false`.

Record that this is Unity Editor preview tooling only. It does not implement final Runtime gameplay, scene production, atlas/final art, real geodata fetching or release build behavior.

## Validation

Use Goal089 tiered validation:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter OfflineGeoworldUnityEditorPreviewTool
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter OfflineGeoworldUnityEditorPreviewToolProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspace
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspaceProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-102-offline-geoworld-unity-editor-preview-tool" -FocusedFilter "OfflineGeoworldUnityEditorPreviewTool|VisualWorldStreamPreviewWorkspace" -ProductSmokeFilter "OfflineGeoworldUnityEditorPreviewToolProductSmokeTests|VisualWorldStreamPreviewWorkspaceProductSmokeTests"
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-102-offline-geoworld-unity-editor-preview-tool"
git diff --check
git diff --cached --check
```

Full check-all is not required unless current-goal/spine-fast indicates shared/core risk.

## Quality gate

GREEN only if no forbidden files changed; no LFZ code copied; no network/provider implementation; no Runtime/public schema/project/dependency changes; no Unity scenes/prefabs/settings/packages/build settings changed; AlphaRuntimeBootstrap unchanged by hash; Unity Editor window exists and is source-health clean; simulated editor action proof passes; workspace integration is real; negative proof passes; no raw geodata dump; no binary/raster media; all new/touched C# files stay within source-health limits; current-goal/spine-fast/artifact-scope pass; final worktree clean.

## Commit/push

Always commit and push to `origin/main`.

Commit message:
- `GREEN Goal 102 offline geoworld Unity editor preview tool`
- `BLOCKED Goal 102 offline geoworld Unity editor preview tool`
- `FAILED Goal 102 offline geoworld Unity editor preview tool`
