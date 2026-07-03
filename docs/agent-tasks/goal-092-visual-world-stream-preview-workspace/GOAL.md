# Goal 092 — Visual World Stream Preview Workspace & Chunk Inspector

## Repo

https://github.com/kpCat/LLMGameCreator

## Working copy

`C:\Users\endim\LLMGameCreator\`

## Branch

`main`

## Codex reasoning

very high

## Primary objective

Add a bounded WinForms review workspace for the new procedural visual world stack.

Goal 086-091 generated deterministic text-SVG/JSON evidence for microtiles, map patches, region profiles and chunk stream windows. Goal 092 must make those artifacts inspectable through a real Application seam and WinForms UserControl surface, without adding external rendering dependencies, without Runtime/Unity changes, and without provider/media generation.

This is a usability and proof-spine goal: the editor should be able to load the visual world artifacts, show their catalogs/proofs/hashes, select SVG previews, and surface validation status in a dedicated workspace.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes:
   - `40cd1db08 GREEN Goal 090 parameterized visual world profiles`
   - `871741346 GREEN Goal 091 deterministic visual chunk stream window`
4. Confirm Goal 091 artifacts exist, are GREEN, and remain `accepted=false`.
5. Confirm Goal 091 report proves finite/huge/infinite stream windows, seam proof, cache reuse proof, layer transition proof and negative proof.
6. Confirm Goal 089 tiered validation scripts exist and will be used.
7. Inspect current dirty state before edits. Do not stage/revert unrelated user work.

## Read first

- `AGENTS.md`
- `docs/VALIDATION_PIPELINE.md`
- `.devflow/validation-profiles/validation-tiers.json`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-materializer-report.md`
- `.llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-composer-report.md`
- `.llmgc/procedural/goal-088-deterministic-visual-region-composer/visual-region-composer-report.md`
- `.llmgc/procedural/goal-090-parameterized-visual-world-profiles/visual-world-profile-report.md`
- `.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-window-report.md`
- `.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-window-catalog.json`
- `.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-materialization-manifest.json`
- `.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-file-ledger.json`
- `.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-quality-gate-scan.json`
- existing WinForms page patterns:
  - `src/LLMGameCreator.WinForms/CompositionRoot.cs`
  - a recent bounded workspace page/control with Designer split.

## Allowed files / areas

- New Application namespace:
  - `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- New WinForms page/control namespace:
  - `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
- WinForms registration:
  - `src/LLMGameCreator.WinForms/CompositionRoot.cs`
- Tests:
  - `tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/`
  - `tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs`
- Evidence:
  - `.llmgc/procedural/goal-092-visual-world-stream-preview-workspace/`
- Docs/state:
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/CONTEXT_INDEX.md`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`
  - `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- Artifact scope:
  - `.devflow/artifact-scope/artifact-scope-policy.json`
- Task pack:
  - `docs/agent-tasks/goal-092-visual-world-stream-preview-workspace/`

## Forbidden files / areas

Do not change:

- public GamePackage schema;
- Runtime / Runtime.Abstractions;
- Unity files, including `AlphaRuntimeBootstrap.cs`;
- Infrastructure provider / LLM / RAG / media provider code;
- Lua / Scripting;
- generator-library;
- `.sln`;
- `.csproj`;
- package lock files;
- binary media assets;
- generated raster assets;
- real NSFW assets;
- explicit prompt dumps or provider-output fixtures;
- external dependencies.

Do not branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Create Application seam

Create BCL-only Application service/models for visual world preview workspace.

Recommended types:

- `VisualWorldStreamPreviewWorkspaceService`
- `VisualWorldStreamPreviewWorkspaceResult`
- `VisualWorldPreviewArtifactGroup`
- `VisualWorldPreviewArtifactEntry`
- `VisualWorldPreviewSvgEntry`
- `VisualWorldPreviewProofStatus`
- `VisualWorldPreviewSelection`
- `VisualWorldPreviewWorkspaceQualityGate`

The service must load real repository artifacts from Goal 086-091 and build a consolidated preview model.

Minimum groups:
- Microtiles from Goal 086.
- Map patches from Goal 087.
- Region composer from Goal 088.
- World profiles from Goal 090.
- Chunk stream windows from Goal 091.

Minimum data per entry:
- id;
- relative path;
- artifact kind;
- source goal id;
- sha256 or declared hash where available;
- status/pass/fail/unknown;
- diagnostic summary;
- text SVG preview path when available;
- safe/rating metadata summary when available;
- no absolute local path.

### 2. Create WinForms workspace

Add a separate WinForms UserControl/page, not a god-form.

The UI should expose:
- artifact group list;
- entry list;
- selected entry details;
- selected SVG text preview or summary;
- proof status summary;
- diagnostics panel;
- refresh/reload button.

No external SVG renderer dependency. It is acceptable to display SVG as text/metadata and file path summary. Do not add WebView2 or browser dependencies.

Keep parent registration bounded and consistent with existing WinForms patterns.

### 3. Generate deterministic Goal 092 evidence

Create `.llmgc/procedural/goal-092-visual-world-stream-preview-workspace/`.

Recommended artifacts:
- `visual-world-stream-preview-workspace-report.md`
- `visual-world-stream-preview-catalog.json`
- `visual-world-stream-preview-proof-status.json`
- `visual-world-stream-preview-winforms-binding-inventory.json`
- `visual-world-stream-preview-quality-gate-scan.json`

Evidence must prove:
- Goal 086-091 artifacts were discovered by relative paths;
- at least 5 artifact groups are present;
- at least 4 SVG/text preview entries are available;
- Goal 091 stream windows are visible in the catalog;
- no absolute paths;
- no binary/raster media added;
- no Runtime/Unity/provider/schema/project/dependency changes;
- WinForms parent/page/control binding is real, not report-only.

### 4. Tests

Focused tests:
- service loads real Goal086-091 artifacts from repo root;
- grouped catalog contains microtiles, map patches, region, profiles and stream windows;
- SVG entries are relative text paths and safe to display;
- proof statuses for Goal091 seam/cache/layer/negative are surfaced;
- missing artifact scenarios produce diagnostics, not fake GREEN.

WinForms binding test:
- instantiate the workspace control/page;
- bind the Application result;
- verify the control stores/displays at least one selected group/entry/proof summary.
- Use reflection only if needed to avoid public API solely for tests.

Product smoke:
- build evidence from repo root;
- read back catalog/proof/status/binding inventory;
- verify no forbidden areas and no binary media;
- verify at least one Goal091 stream window SVG is represented.

### 5. Docs/state

Update docs quartet and debt register.

Goal 092 manual gate:
`visual_world_stream_preview_workspace_verification required`

Goal 092 `accepted=false`.

Record that this is editor/review usability only; it does not implement runtime streaming or Unity consumption.

## Validation policy

Use Goal 089 tiered validation.

Required:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspace
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspaceProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-092-visual-world-stream-preview-workspace" -FocusedFilter "VisualWorldStreamPreviewWorkspace" -ProductSmokeFilter "VisualWorldStreamPreviewWorkspaceProductSmokeTests"
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-092-visual-world-stream-preview-workspace"
git diff --check
git diff --cached --check
```

Do not require full `check-all.ps1` for this ordinary feature goal unless current-goal/spine-fast indicates shared/core risk.

## Quality gate

GREEN only if:

- no forbidden files changed;
- no Runtime/Unity/provider/schema/project/dependency changes;
- no binary/raster media added;
- no prompt dumps;
- Application service loads real artifacts, not hardcoded success;
- WinForms workspace is a separate UserControl/page and binds real Application result;
- at least 5 artifact groups are represented;
- Goal091 stream windows are represented;
- selected SVG/text preview is safe and relative;
- evidence is deterministic;
- current-goal and spine-fast validation pass;
- artifact scope passes;
- source formatting guard remains clean.

## Stop / block conditions

Return BLOCKED if:
- workspace requires external rendering dependency;
- workspace requires Unity/Runtime/provider/schema changes;
- real artifact loading cannot be done without absolute paths;
- WinForms registration requires project file changes;
- artifact scope cannot be satisfied.

Return FAILED if:
- build/tests regress due to this goal and cannot be fixed inside allowed files.

## Final report format

Report:
- Final status.
- Latest commit before/after.
- Push status.
- Files changed.
- Application seam summary.
- WinForms workspace binding summary.
- Artifact groups/entries summary.
- Goal091 stream-window visibility proof.
- Validation tier commands and results.
- Artifact scope result.
- Evidence hygiene.
- Remaining P2/P3 debt.
- Final git status.
- Git commands used and why.

## Mandatory commit/push policy

Always commit and push to `origin/main`, even for GREEN/BLOCKED/FAILED.

Commit message must honestly reflect status:
- `GREEN Goal 092 visual world stream preview workspace`
- `BLOCKED Goal 092 visual world stream preview workspace`
- `FAILED Goal 092 visual world stream preview workspace`
