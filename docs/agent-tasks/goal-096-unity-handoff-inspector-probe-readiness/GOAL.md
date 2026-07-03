# Goal 096 — Unity Handoff Inspector & Probe Readiness Workspace Integration

## Repo

https://github.com/kpCat/LLMGameCreator

## Working copy

`C:\Users\endim\LLMGameCreator\`

## Branch

`main`

## Codex reasoning

very high

## Primary objective

Integrate the Goal 095 Unity StreamingAssets handoff into the existing Visual World Stream Preview Workspace so the editor can inspect Unity-facing payload readiness without launching Unity and without changing Runtime/Unity behavior.

Goal 095 created:
- `unity/LLMGameCreatorAlpha/Assets/Scripts/VisualChunkCacheHandoffProbe.cs`
- mirrored payload under `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/VisualChunkCacheGoal095/`
- Goal095 `.llmgc` evidence and simulated Unity read proof.

Goal 096 must make these artifacts visible in the existing workspace Application seam and WinForms UI:
- Unity handoff manifest;
- StreamingAssets payload file ledger;
- probe source inventory;
- simulated Unity read proof;
- negative proof;
- AlphaRuntimeBootstrap unchanged status;
- source-health/forbidden-area readiness status.

This is not runtime implementation. Do not change AlphaRuntimeBootstrap, Unity project settings, Runtime, GamePackage schema, providers, Lua, generator-library, project files, dependencies or binary/raster media.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes:
   - `627356b3 GREEN Goal 094 visual chunk cache export inspector`
   - `b8ac7242 GREEN Goal 095 visual chunk cache Unity StreamingAssets handoff`
4. Confirm Goal095 artifacts exist and remain `accepted=false`.
5. Confirm Goal095 report proves:
   - 5 StreamingAssets payload files;
   - 4 packages;
   - 93 records;
   - 5 stream windows;
   - 93 unique chunk keys;
   - simulated Unity read proof passed;
   - negative proof passed;
   - AlphaRuntimeBootstrap hash unchanged.
6. Confirm `VisualWorldStreamPreviewWorkspace` source-health remains below limits from Goal092A/094.
7. Inspect dirty state. Do not stage/revert unrelated user work.

## Read first

- `AGENTS.md`
- `docs/VALIDATION_PIPELINE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.llmgc/procedural/goal-092a-visual-world-preview-service-split-source-health/source-health-before-after.json`
- `.llmgc/procedural/goal-094-visual-chunk-cache-export-inspector/visual-chunk-cache-export-inspector-report.md`
- `.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/visual-chunk-cache-unity-handoff-report.md`
- `.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/visual-chunk-cache-unity-handoff-manifest.json`
- `.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/visual-chunk-cache-unity-streamingassets-ledger.json`
- `.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/visual-chunk-cache-unity-probe-source-inventory.json`
- `.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/visual-chunk-cache-unity-simulated-read-proof.json`
- `.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/visual-chunk-cache-unity-negative-proof.json`
- `.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/visual-chunk-cache-unity-quality-gate-scan.json`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/VisualChunkCacheHandoffProbe.cs`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/VisualChunkCacheGoal095/`
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs`

## Allowed files / areas

- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs`
- `.llmgc/procedural/goal-096-unity-handoff-inspector-probe-readiness/`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.devflow/artifact-scope/artifact-scope-policy.json`
- `docs/agent-tasks/goal-096-unity-handoff-inspector-probe-readiness/`

Optional only if needed to refresh derived Goal092 workspace evidence:
- `.llmgc/procedural/goal-092-visual-world-stream-preview-workspace/`

## Forbidden files / areas

Do not change:

- `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/VisualChunkCacheHandoffProbe.cs`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/VisualChunkCacheGoal095/`
- Unity scenes, prefabs, asmdefs, project settings, packages, build settings.
- Runtime / Runtime.Abstractions.
- Public GamePackage schema.
- Infrastructure provider / LLM / RAG / media provider code.
- Lua / Scripting.
- generator-library.
- `.sln`, `.csproj`, lock files.
- binary/raster media assets.
- generated real NSFW assets.
- prompt dumps or provider-output fixtures.
- external dependencies.

No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Extend workspace Application seam

Extend the existing Visual World Stream Preview Workspace Application seam so it discovers Goal095 Unity handoff artifacts.

Add cohesive helper files if needed. Do not bloat existing files.

Required catalog concepts:
- `unity_handoff` artifact group;
- StreamingAssets payload root;
- manifest entry;
- package index entry;
- stream window index entry;
- chunk key ledger entry;
- runtime readme entry;
- Unity probe source inventory entry;
- simulated Unity read proof entry;
- negative proof entry;
- AlphaRuntimeBootstrap unchanged status;
- forbidden Unity areas unchanged status;
- metadata-only status.

The service must read real Goal095 files by relative paths and must not hardcode GREEN.

### 2. Update WinForms workspace

Update existing Visual World Stream Preview Workspace UI so selected Unity handoff entries show:
- payload file count;
- package count;
- record count;
- stream window count;
- unique chunk key count;
- simulated read proof status;
- negative proof status;
- probe source status;
- AlphaRuntimeBootstrap unchanged status;
- diagnostics.

No WebView2/browser/SVG dependency.

### 3. Generate Goal096 evidence

Create `.llmgc/procedural/goal-096-unity-handoff-inspector-probe-readiness/`.

Recommended artifacts:
- `unity-handoff-inspector-report.md`
- `unity-handoff-inspector-catalog.json`
- `unity-handoff-inspector-proof-status.json`
- `unity-handoff-inspector-winforms-binding-inventory.json`
- `unity-handoff-inspector-source-health-scan.json`
- `unity-handoff-inspector-quality-gate-scan.json`

Evidence must prove:
- Goal095 files discovered by relative paths;
- all 5 StreamingAssets payload files are represented;
- Unity probe source inventory is represented;
- simulated read proof and negative proof are surfaced;
- AlphaRuntimeBootstrap unchanged status is surfaced;
- workspace binding is real;
- no Unity files are changed by this goal;
- no absolute paths, binary media, provider calls, Runtime/schema/project changes.

### 4. Tests

Focused tests:
- workspace service loads Goal095 artifacts from repo root;
- `unity_handoff` group exists;
- all 5 payload files represented;
- Unity probe source inventory visible;
- simulated read proof status visible;
- negative proof status visible;
- missing Goal095 artifact yields diagnostics, not fake GREEN;
- no file in touched workspace namespace exceeds 700 logical lines and none exceeds 1000.

Product smoke:
- build Goal096 evidence from repo root;
- read catalog/proof/binding/quality artifacts;
- verify Unity handoff group and payload entries;
- verify no Unity source/payload files changed by this goal;
- verify no binary/raster media or forbidden areas.

### 5. Docs/state

Update docs quartet and debt register.

Goal096 manual gate:
`unity_handoff_inspector_probe_readiness_verification required`

Goal096 `accepted=false`.

Record that this is editor/readiness inspection only; it does not implement live Unity rendering or Runtime consumption.

## Validation policy

Use Goal089 tiered validation.

Required:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspace
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspaceProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-096-unity-handoff-inspector-probe-readiness" -FocusedFilter "VisualWorldStreamPreviewWorkspace" -ProductSmokeFilter "VisualWorldStreamPreviewWorkspaceProductSmokeTests"
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-096-unity-handoff-inspector-probe-readiness"
git diff --check
git diff --cached --check
```

Full `check-all.ps1` is not required unless current-goal/spine-fast indicates shared/core risk.

## Quality gate

GREEN only if:
- no forbidden files changed;
- no Unity files changed by Goal096;
- Goal095 Unity handoff artifacts are loaded through real Application seam;
- all 5 StreamingAssets payload entries and probe inventory are surfaced;
- simulated read proof and negative proof are surfaced;
- AlphaRuntimeBootstrap unchanged status is surfaced;
- WinForms binding is real;
- no file in touched workspace namespace exceeds 700 logical lines; no file in repo changes exceeds 1000;
- no Runtime/provider/schema/project/dependency changes;
- no binary/raster media or prompt dumps;
- current-goal and spine-fast validation pass;
- artifact scope passes;
- final worktree is clean.

## Stop / block conditions

Return BLOCKED if:
- Goal095 artifacts cannot be loaded without changing Unity payload/probe or forbidden zones;
- UI integration requires external rendering/browser dependency;
- artifact scope cannot be satisfied;
- source-health limits cannot be kept.

Return FAILED if:
- build/tests regress due to this goal and cannot be fixed inside allowed files.

## Final report format

Report:
- Final status.
- Latest commit before/after.
- Push status.
- Files changed.
- Application seam changes.
- WinForms UI changes.
- Unity handoff entries surfaced.
- Probe readiness proof.
- AlphaRuntimeBootstrap status.
- Source-health summary.
- Validation tier results.
- Artifact scope result.
- Evidence hygiene.
- Remaining P2/P3 debt.
- Final git status.
- Git commands used and why.

## Mandatory commit/push policy

Always commit and push to `origin/main`, even for GREEN/BLOCKED/FAILED.

Commit message must honestly reflect status:
- `GREEN Goal 096 Unity handoff inspector probe readiness`
- `BLOCKED Goal 096 Unity handoff inspector probe readiness`
- `FAILED Goal 096 Unity handoff inspector probe readiness`
