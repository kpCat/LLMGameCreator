# Goal 103 — Offline Geoworld Play Mode Travel Preview & Boundary Prefetch Controller

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`
Codex reasoning: very high

## Primary objective

Deliver the next large, user-visible composite milestone after Goal 102/102B.

Goal 101 produced Unity preview commands and a travel-window demo payload.
Goal 102 added a Unity Editor preview window for manual create/clear inspection.
Goal 102B found that the alleged source-format defect was a false premise in actual HEAD and recorded the evidence-trust issue.

Goal 103 must move from static manual preview toward an interactive Unity Alpha play-mode travel preview:

1. close the Goal102B false-positive/proceed decision in docs/debt/state without pretending Goal102B was GREEN;
2. consume real Goal101/102 payload/evidence;
3. generate a deterministic play-mode travel preview plan over the synthetic geoworld window;
4. add standalone Unity scripts that can run the travel-step demo from StreamingAssets metadata;
5. add a Unity Editor launch/helper window for creating the play-mode preview controller object on demand;
6. integrate the play-mode travel preview into Visual World Stream Preview Workspace;
7. produce deterministic evidence, negative proof, focused tests and product smoke.

This is still Unity Alpha tooling, not final Runtime, not final gameplay, not final art, not real geodata fetching.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes:
   - `708558ae GREEN Goal 101 offline geoworld Unity preview runner`
   - `c6f16eb GREEN Goal 102 offline geoworld Unity editor preview tool`
   - `62f883b GREEN Goal 102A Unity editor source format guard`
   - `9e9aa89 BLOCKED Goal 102B actual Unity editor source reformat`
4. Confirm Goal102B evidence says actual target source is already readable:
   - actualHeadBeforeMalformedDetected=false;
   - workingTreeSourceReadable=true;
   - rawPhysicalLineCount around 154;
   - max physical line length around 115;
   - AlphaRuntimeBootstrap unchanged.
5. Record in Goal103 docs/evidence that this is a false-positive closure/proceed decision, not an acceptance of Goal102B as GREEN.
6. Record `AlphaRuntimeBootstrap.cs` hash/line count before work and do not modify it.
7. Inspect dirty state. Do not stage/revert unrelated user work.

## Read first

- `AGENTS.md`
- `docs/GOAL_PRODUCTIVITY_POLICY.md`
- `docs/VALIDATION_PIPELINE.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner/offline-geoworld-preview-command-catalog.json`
- `.llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner/offline-geoworld-preview-travel-window-script.json`
- `.llmgc/procedural/goal-102-offline-geoworld-unity-editor-preview-tool/offline-geoworld-unity-editor-preview-tool-report.md`
- `.llmgc/procedural/goal-102b-actual-unity-editor-source-reformat/actual-unity-editor-source-before-after.json`
- `.llmgc/procedural/goal-102b-actual-unity-editor-source-reformat/actual-unity-editor-source-trust-audit.json`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewRunner.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPrimitiveFactory.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewTravelWindow.cs`
- `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs` read-only baseline.
- existing Visual World Stream Preview Workspace files.

## Allowed files / areas

- `src/LLMGameCreator.Application/Design/OfflineGeoworldUnityPlayModeTravelPreview/`
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldUnityPlayModeTravelPreview/`
- `tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldUnityPlayModeTravelPreviewProductSmokeTests.cs`
- `tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeTravelController.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeTravelState.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeChunkVisibility.cs`
- `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPlayModeTravelWindow.cs`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal103/`
- `.llmgc/procedural/goal-103-offline-geoworld-playmode-travel-preview/`
- docs quartet, debt register, artifact-scope policy
- this task pack

## Forbidden files / areas

Do not change LFZ archive/source, Runtime, public GamePackage schema, providers, Lua, generator-library, `.sln`, `.csproj`, lock files, `AlphaRuntimeBootstrap.cs`, Unity scenes/prefabs/asmdefs/project settings/packages/build settings, existing Goal101/102 StreamingAssets payloads, binary/raster media, real geodata dumps, live network fetch code, external dependencies, prompt dumps. No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Required behavior

### 1. Goal102B false-positive closure/proceed record

Update docs/state/debt to say:
- Goal102B remains BLOCKED as an honest investigation;
- the product/source blocker is closed because actual target source is already readable;
- Goal102A synthetic-before trust issue remains recorded as a lesson;
- future source-format gates must use actual target bytes for actual claims.

Do not mark Goal102B GREEN.

### 2. Application-side play-mode travel preview payload

Create a BCL-only service that reads real Goal101 command/travel payloads and writes:

- `.llmgc/procedural/goal-103-offline-geoworld-playmode-travel-preview/`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal103/`

Required payload files:
- `offline-geoworld-playmode-travel-manifest.json`
- `offline-geoworld-playmode-steps.json`
- `offline-geoworld-playmode-chunk-visibility.json`
- `offline-geoworld-playmode-object-state-index.json`
- `offline-geoworld-playmode-readme.json`

The plan must include:
- at least 4 travel steps from Goal101 lineage;
- active chunk set per step;
- boundary-prefetch chunks per step;
- object visibility diff per step;
- expected visible object count per step;
- deterministic state hash per step;
- no raw geodata.

### 3. Unity play-mode scripts

Add standalone scripts:

- `OfflineGeoworldPlayModeTravelController.cs`
- `OfflineGeoworldPlayModeTravelState.cs`
- `OfflineGeoworldPlayModeChunkVisibility.cs`

Expected behavior:
- read Goal103 manifest from `Application.streamingAssetsPath`;
- step through travel states manually or on timer;
- expose current step/status/visible object count in Inspector;
- activate/deactivate preview objects by metadata ids/names if present;
- never call network/provider/LLM;
- do not depend on `AlphaRuntimeBootstrap.cs`;
- do not require scene/prefab/project settings changes;
- tolerate missing preview objects with diagnostics.

### 4. Unity Editor launch/helper window

Add `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPlayModeTravelWindow.cs`.

Expected behavior:
- menu item such as `LLMGameCreator/Offline Geoworld Play Mode Travel`;
- read Goal103 payload readiness;
- create a controller GameObject on demand;
- clear controller GameObject on demand;
- no automatic scene mutation on import;
- no external packages;
- no network/provider calls.

### 5. Simulated play-mode execution proof

.NET proof must simulate:
- manifest read;
- step file read;
- chunk visibility read;
- object state index read;
- step-by-step expected visible counts;
- boundary-prefetch progression;
- deterministic state hash chain;
- no unsupported step;
- no absolute paths;
- no raw geodata;
- no binary/raster media;
- no network/provider markers.

### 6. Workspace integration

Extend Visual World Stream Preview Workspace with `offline_geoworld_playmode_travel` group showing:
- travel step count;
- active chunk count by step;
- boundary prefetch count by step;
- expected visible object count by step;
- Unity script readiness;
- Unity editor launch helper readiness;
- simulated play-mode proof status;
- Goal102B false-positive closure status;
- AlphaRuntimeBootstrap unchanged status.

The workspace must read real Goal103 evidence.

### 7. Evidence

Create `.llmgc/procedural/goal-103-offline-geoworld-playmode-travel-preview/`.

Required artifacts:
- `offline-geoworld-playmode-travel-report.md`
- `offline-geoworld-playmode-travel-manifest.json`
- `offline-geoworld-playmode-steps.json`
- `offline-geoworld-playmode-chunk-visibility.json`
- `offline-geoworld-playmode-object-state-index.json`
- `offline-geoworld-playmode-unity-script-inventory.json`
- `offline-geoworld-playmode-editor-window-inventory.json`
- `offline-geoworld-playmode-simulated-execution-proof.json`
- `offline-geoworld-playmode-negative-proof.json`
- `offline-geoworld-playmode-workspace-binding-inventory.json`
- `offline-geoworld-playmode-source-lineage.json`
- `offline-geoworld-playmode-quality-gate-scan.json`
- `goal102b-false-positive-closure.json`

### 8. Negative proof

Reject at least:
- missing Goal101 travel payload;
- missing Goal103 manifest;
- unsupported travel step;
- active chunk missing from chunk visibility;
- object state references unknown object;
- fake success without reading files;
- absolute path in payload;
- raw geodata leaked into play-mode plan;
- network/provider marker in Unity scripts;
- AlphaRuntimeBootstrap dependency marker;
- scene/prefab/project settings mutation marker;
- binary/raster media marker;
- Goal102B closure claimed without actual before/after evidence.

### 9. Tests

Focused tests:
- service builds payload from repo root;
- play-mode travel plan has at least 4 steps;
- boundary-prefetch progression is represented;
- state hash chain is deterministic;
- Unity script inventory passes no-network/no-provider/no-AlphaRuntimeBootstrap scan;
- editor window inventory passes menu/create/clear markers;
- workspace group exists;
- negative proof rejects expected cases;
- source-health limits pass.

Product smoke:
- build evidence from repo root;
- read `.llmgc` evidence and Unity StreamingAssets payload;
- verify travel steps, chunk visibility and object states;
- verify Unity scripts/editor helper exist;
- verify no Runtime/schema/provider/dependency changes, no scene/prefab/settings changes, no raw geodata dump and no binary/raster media.

### 10. Docs/state

Update docs quartet and debt register.

Manual gate:
`offline_geoworld_playmode_travel_preview_verification required`

Status:
`accepted=false`.

Record that this is Unity Alpha play-mode preview tooling only. It does not implement final Runtime gameplay, final art, real geodata fetching or release build behavior.

## Validation

Use Goal089 tiered validation:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter OfflineGeoworldUnityPlayModeTravelPreview
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter OfflineGeoworldUnityPlayModeTravelPreviewProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspace
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspaceProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-103-offline-geoworld-playmode-travel-preview" -FocusedFilter "OfflineGeoworldUnityPlayModeTravelPreview|VisualWorldStreamPreviewWorkspace" -ProductSmokeFilter "OfflineGeoworldUnityPlayModeTravelPreviewProductSmokeTests|VisualWorldStreamPreviewWorkspaceProductSmokeTests"
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-103-offline-geoworld-playmode-travel-preview"
git diff --check
git diff --cached --check
```

Full check-all is not required unless current-goal/spine-fast indicates shared/core risk.

## Quality gate

GREEN only if no forbidden files changed; no LFZ code copied; no network/provider implementation; no Runtime/public schema/project/dependency changes; no Unity scenes/prefabs/settings/packages/build settings changed; AlphaRuntimeBootstrap unchanged; play-mode payload/scripts/editor helper exist; simulated play-mode proof passes; workspace integration is real; Goal102B false-positive closure recorded; negative proof passes; no raw geodata dump; no binary/raster media; source-health limits pass; current-goal/spine-fast/artifact-scope pass; final worktree clean.

## Commit/push

Always commit and push to `origin/main`.

Commit message:
- `GREEN Goal 103 offline geoworld playmode travel preview`
- `BLOCKED Goal 103 offline geoworld playmode travel preview`
- `FAILED Goal 103 offline geoworld playmode travel preview`
