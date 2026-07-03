# Goal 101 — Offline Geoworld Unity Preview Runner & Travel Window Demo

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`
Codex reasoning: very high

## Primary objective

Deliver a larger composite, user-visible milestone after Goal 100.

Goal 100 connected synthetic offline geoworld data to visual cache export and Unity StreamingAssets handoff. Goal 101 must add a bounded Unity Alpha preview runner that reads the Goal100 payload and constructs a simple runtime-visible placeholder world preview in Unity from metadata commands.

This remains a Unity Alpha proof, not final Runtime and not final art. Do not change `AlphaRuntimeBootstrap.cs`, Runtime projects, GamePackage schema, providers, Lua, generator-library, project files, dependencies, or real map fetching.

## Composite outcome required

This goal must produce all of the following in one slice:

1. Application-side preview runner payload.
2. Unity-side standalone preview runner scripts.
3. Travel-window demo metadata.
4. Simulated Unity command execution proof.
5. Visual World Stream Preview Workspace integration.
6. Deterministic evidence.
7. Focused tests and product smoke.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes:
   - `c6c2093a GREEN Goal 098 geoworld source adapter streaming contract`
   - `48322dae GREEN Goal 099 offline geoworld WorldSourceGraph streaming`
   - `98b16d78 GREEN Goal 100 offline geoworld visual cache Unity handoff`
4. Confirm Goal100 artifacts exist and remain `accepted=false`.
5. Confirm Goal100 report proves:
   - 3 metadata-only packages;
   - 18 visual cache records;
   - 5 Unity StreamingAssets payload files;
   - standalone `OfflineGeoworldHandoffProbe.cs`;
   - simulated Unity read proof passed;
   - AlphaRuntimeBootstrap unchanged.
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
- `.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/offline-geoworld-visual-cache-unity-handoff-report.md`
- `.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/offline-geoworld-visual-cache-catalog.json`
- `.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/offline-geoworld-feature-chunk-ledger.json`
- `.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/offline-geoworld-unity-handoff-manifest.json`
- `.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/offline-geoworld-unity-simulated-read-proof.json`
- `.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/offline-geoworld-negative-proof.json`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldHandoffProbe.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/VisualChunkCacheHandoffProbe.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs` read-only baseline.
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`

## Allowed files / areas

- `src/LLMGameCreator.Application/Design/OfflineGeoworldUnityPreviewRunner/`
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldUnityPreviewRunner/`
- `tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldUnityPreviewRunnerProductSmokeTests.cs`
- `tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewRunner.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPrimitiveFactory.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewTravelWindow.cs`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal101/`
- `.llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner/`
- docs quartet, debt register, artifact-scope policy
- this task pack

## Forbidden files / areas

Do not change LFZ archive/source, Runtime, public schema, providers, Lua, generator-library, `.sln`, `.csproj`, lock files, `AlphaRuntimeBootstrap.cs`, Unity scenes/prefabs/asmdefs/settings/packages/build settings, binary/raster media, real geodata dumps, live network fetch code, external dependencies, or prompt dumps. No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Required behavior

### 1. Build Unity preview runner payload

Create a BCL-only Application service that consumes Goal100 artifacts and writes:

- `.llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner/`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal101/`

Required payload files:

- `offline-geoworld-preview-runner-manifest.json`
- `offline-geoworld-preview-feature-commands.json`
- `offline-geoworld-preview-travel-window-script.json`
- `offline-geoworld-preview-style-legend.json`
- `offline-geoworld-preview-readme.json`

Minimum command kinds:

- `building_footprint_marker`
- `road_segment_line`
- `water_body_plane`
- `land_use_area_plane`
- `poi_marker`
- `bridge_marker`
- `barrier_line`
- `vegetation_area_marker`
- `terrain_hint_marker`
- `administrative_hint_marker`

Commands are metadata instructions only, not meshes/assets.

### 2. Add standalone Unity preview runner scripts

Add:

- `OfflineGeoworldPreviewRunner.cs`
- `OfflineGeoworldPreviewPrimitiveFactory.cs`
- `OfflineGeoworldPreviewTravelWindow.cs`

Expected behavior:

- read Goal101 manifest from `Application.streamingAssetsPath`;
- read feature command file and style legend;
- create simple GameObjects/primitives/LineRenderer-like placeholders from command metadata;
- expose status fields in Inspector;
- support a travel-window demo sequence from payload metadata;
- no provider/LLM/network calls;
- no external packages;
- no `AlphaRuntimeBootstrap.cs` dependency;
- no scene auto-modification requirement;
- no final art claim.

### 3. Simulated Unity command execution proof

Create .NET proof that simulates the Unity runner:

- manifest read;
- command file read;
- style legend read;
- command count by kind;
- travel window steps;
- expected object counts;
- no unsupported command kind;
- no absolute paths;
- no raw geodata;
- no binary/raster media;
- no network/provider markers.

### 4. Workspace integration

Extend Visual World Stream Preview Workspace with an `offline_geoworld_unity_preview` group showing:

- preview command count;
- feature kind command coverage;
- travel window step count;
- Unity scripts readiness status;
- simulated command execution proof;
- AlphaRuntimeBootstrap unchanged status;
- negative proof status.

The workspace must read real Goal101 evidence.

### 5. Evidence

Create `.llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner/`.

Recommended artifacts:

- `offline-geoworld-unity-preview-runner-report.md`
- `offline-geoworld-preview-command-catalog.json`
- `offline-geoworld-preview-style-legend.json`
- `offline-geoworld-preview-travel-window-script.json`
- `offline-geoworld-preview-streamingassets-ledger.json`
- `offline-geoworld-preview-unity-script-inventory.json`
- `offline-geoworld-preview-simulated-command-proof.json`
- `offline-geoworld-preview-negative-proof.json`
- `offline-geoworld-preview-workspace-binding-inventory.json`
- `offline-geoworld-preview-source-lineage.json`
- `offline-geoworld-preview-quality-gate-scan.json`

Evidence must prove Goal100 consumed, all feature kinds mapped to preview commands, Unity payload exists, Unity scripts exist and are source-health clean, simulated command proof passed, workspace group real, AlphaRuntimeBootstrap unchanged, no forbidden areas.

### 6. Negative proof

Reject at least:

- missing Goal100 payload;
- unsupported feature command kind;
- raw geodata leaked into command;
- missing style legend;
- missing travel window script;
- absolute path in payload;
- network/provider marker in Unity script;
- fake success without file read;
- AlphaRuntimeBootstrap changed marker;
- binary/raster media marker;
- missing safe fallback for rating metadata if present.

### 7. Tests

Focused tests:

- service builds payload from real Goal100 artifacts;
- all feature command kinds represented;
- simulated command execution proof passes;
- travel window demo steps represented;
- Unity script inventory passes no-network/no-provider scan;
- workspace group exists;
- negative proof rejects expected cases;
- evidence deterministic;
- new/touched C# files stay under source-health limits.

Product smoke:

- build evidence from repo root;
- read `.llmgc` evidence and Unity StreamingAssets payload;
- verify feature command counts and travel-window steps;
- verify Unity scripts exist;
- verify no Runtime/schema/provider/dependency changes, no raw geodata dump and no binary/raster media.

### 8. Docs/state

Update docs quartet and debt register.

Manual gate: `offline_geoworld_unity_preview_runner_verification required`

Status: `accepted=false`.

Record that this is Unity Alpha preview runner only. It does not implement final Runtime consumption, full gameplay, real geodata fetching, final art, atlas, or scene/prefab production.

## Validation

Use Goal089 tiered validation:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter OfflineGeoworldUnityPreviewRunner
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter OfflineGeoworldUnityPreviewRunnerProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspace
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspaceProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-101-offline-geoworld-unity-preview-runner" -FocusedFilter "OfflineGeoworldUnityPreviewRunner|VisualWorldStreamPreviewWorkspace" -ProductSmokeFilter "OfflineGeoworldUnityPreviewRunnerProductSmokeTests|VisualWorldStreamPreviewWorkspaceProductSmokeTests"
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-101-offline-geoworld-unity-preview-runner"
git diff --check
git diff --cached --check
```

Full check-all is not required unless current-goal/spine-fast indicates shared/core risk.

## Quality gate

GREEN only if no forbidden files changed; no LFZ code copied; no network/provider implementation; no Runtime/public schema/project/dependency changes; AlphaRuntimeBootstrap unchanged by hash; Unity preview runner scripts and StreamingAssets payload exist; all feature kinds map into preview commands; simulated command proof passes; workspace integration is real; negative proof passes; no raw geodata dump; no binary/raster media; all new/touched C# files stay within source-health limits; current-goal/spine-fast/artifact-scope pass; final worktree clean.

## Commit/push

Always commit and push to `origin/main`.

Commit message:

- `GREEN Goal 101 offline geoworld Unity preview runner`
- `BLOCKED Goal 101 offline geoworld Unity preview runner`
- `FAILED Goal 101 offline geoworld Unity preview runner`
