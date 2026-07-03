# Goal 092A — Visual World Preview Service Split & Source-Health Backstop

## Repo

https://github.com/kpCat/LLMGameCreator

## Working copy

`C:\Users\endim\LLMGameCreator\`

## Branch

`main`

## Codex reasoning

very high

## Primary objective

Repair the Goal 092 source-health regression: `VisualWorldStreamPreviewWorkspaceService.cs` was added as an oversized 1294-line / 1203-loc service. Goal 092 behavior is useful, but this violates the project’s source-health policy and must be fixed before the next feature goal.

This is a bounded hotfix/audit task:
- split the oversized Goal 092 Application service into smaller cohesive BCL-only helper files;
- preserve behavior and existing public seam as much as practical;
- strengthen Goal 092/092A source-health evidence so files over 1000 logical lines cannot pass silently;
- do not add features, dependencies, Runtime, Unity, provider, schema, Lua or project-file changes.

## Current context

Goal 092 added:
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceService.cs`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs`
- Goal 092 artifacts under `.llmgc/procedural/goal-092-visual-world-stream-preview-workspace/`

GitHub audit found:
- Goal 092 commit: `18d98f381 GREEN Goal 092 visual world stream preview workspace`
- `VisualWorldStreamPreviewWorkspaceService.cs`: 1294 lines / 1203 loc
- Goal 092 quality gate reports behavior status but does not enforce or record `filesOver1000LinesCount` / source-health metrics for this new workspace.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes `18d98f381 GREEN Goal 092 visual world stream preview workspace`.
4. Confirm Goal 092 artifacts exist and remain `accepted=false`.
5. Confirm `VisualWorldStreamPreviewWorkspaceService.cs` currently exceeds 1000 logical lines before repair.
6. Confirm no unrelated user changes are present. If unrelated dirty files exist, do not stage/revert them.
7. Confirm no forbidden areas are needed for this repair.

## Read first

- `AGENTS.md`
- `docs/VALIDATION_PIPELINE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.llmgc/procedural/goal-092-visual-world-stream-preview-workspace/visual-world-stream-preview-workspace-report.md`
- `.llmgc/procedural/goal-092-visual-world-stream-preview-workspace/visual-world-stream-preview-quality-gate-scan.json`
- `.llmgc/procedural/goal-092-visual-world-stream-preview-workspace/visual-world-stream-preview-catalog.json`
- `.llmgc/procedural/goal-092-visual-world-stream-preview-workspace/visual-world-stream-preview-proof-status.json`
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceService.cs`
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceModels.cs`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs`
- `src/LLMGameCreator.WinForms/CompositionRoot.cs`
- `tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceServiceTests.cs`
- `tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs`

## Allowed files / areas

You may change only:

- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs`
- `.llmgc/procedural/goal-092-visual-world-stream-preview-workspace/`
- `.llmgc/procedural/goal-092a-visual-world-preview-service-split-source-health/`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.devflow/artifact-scope/artifact-scope-policy.json`
- `docs/agent-tasks/goal-092a-visual-world-preview-service-split-source-health/`

Optional only if absolutely required by compile/test:
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs`
- `src/LLMGameCreator.WinForms/CompositionRoot.cs`

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

### 1. Split the oversized Application service

Refactor `VisualWorldStreamPreviewWorkspaceService.cs` into smaller cohesive files while preserving the main public service seam.

Recommended helper split:

- `VisualWorldStreamPreviewWorkspaceService.cs`
  - orchestration only;
  - target under 500 logical lines, hard maximum 700.

- `VisualWorldStreamPreviewArtifactDiscovery.cs`
  - loads/discovers Goal086-091 artifact paths and SVG entries.

- `VisualWorldStreamPreviewCatalogBuilder.cs`
  - builds grouped catalog entries.

- `VisualWorldStreamPreviewProofStatusLoader.cs`
  - reads proof/status artifacts.

- `VisualWorldStreamPreviewEvidenceWriter.cs`
  - writes Goal092 report/catalog/proof/binding/quality evidence.

- `VisualWorldStreamPreviewSourceHealthScanner.cs`
  - raw/logical source-health scan for this namespace.

You may use different names if the responsibilities remain clear.

No new feature scope. This is a readability/source-health repair.

### 2. Preserve Goal 092 behavior

The refactor must preserve:

- 5 artifact groups minimum;
- Goal091 stream window entries visible;
- SVG/text preview entries visible and relative;
- no absolute paths;
- proof status passed;
- WinForms binding inventory remains real;
- no Runtime/Unity/provider/schema/project/dependency changes.

Tests should prove behavior through the real Application service, not a report-only marker.

### 3. Strengthen quality evidence

Update Goal092 and add Goal092A evidence so source-health metrics are recorded and enforced.

Required source-health metrics:

- scanned C# file count;
- max logical line count;
- max physical line length;
- files over 1000 logical lines;
- files over 700 logical lines in Goal092 namespace;
- zero-LF source count;
- CR-only source count;
- raw physical one-line source count;
- minified source count;
- oversized service before/after;
- `VisualWorldStreamPreviewWorkspaceService.cs` line count before/after.

Goal092A quality gate must fail if any file in the Goal092 namespace exceeds 1000 logical lines. Prefer stricter target: no file in the namespace over 700 logical lines.

### 4. Generate Goal 092A evidence

Create `.llmgc/procedural/goal-092a-visual-world-preview-service-split-source-health/`.

Recommended artifacts:

- `visual-world-preview-service-split-report.md`
- `source-health-before-after.json`
- `refactor-file-inventory.json`
- `behavior-equivalence-proof.json`
- `quality-gate-scan.json`

Evidence must prove:

- oversized service was detected before repair;
- no C# file in Goal092 namespace remains over 1000 lines;
- preferred max 700 lines achieved or residual debt recorded honestly;
- behavior equivalence with Goal092 catalog/proof expectations;
- no forbidden areas changed;
- no binary/media/prompt/provider artifacts.

### 5. Tests

Update focused tests to assert:

- service still loads real Goal086-091 artifacts;
- group/entry counts remain at or above Goal092 baseline;
- Goal091 stream windows remain visible;
- source-health scanner rejects a synthetic over-1000-line C# sample;
- source-health scanner rejects zero-LF/CR-only/minified samples;
- no file in current Goal092 namespace exceeds 1000 logical lines.

Product smoke should assert:

- Goal092A evidence exists;
- source-health before/after is read and passed;
- behavior equivalence proof passed;
- no forbidden areas and no binary media.

### 6. Docs/state

Update docs quartet and debt register.

Manual gate:
`visual_world_preview_service_split_source_health_verification required`

Status:
- Goal092 remains produced-for-review / accepted=false.
- Goal092A is produced-for-review / accepted=false.
- Do not mark future renderer/runtime/Unity work as complete.

## Validation policy

Use Goal 089 tiered validation.

Required:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspace
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspaceProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-092a-visual-world-preview-service-split-source-health" -FocusedFilter "VisualWorldStreamPreviewWorkspace" -ProductSmokeFilter "VisualWorldStreamPreviewWorkspaceProductSmokeTests"
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-092a-visual-world-preview-service-split-source-health"
git diff --check
git diff --cached --check
```

Full `check-all.ps1` is not required for this bounded source-health repair unless current-goal/spine-fast indicates shared/core risk.

## Quality gate

GREEN only if:

- no forbidden files changed;
- no C# file in Goal092 namespace over 1000 logical lines;
- target no file over 700 logical lines is met or residual is recorded as P2 with strong justification;
- `VisualWorldStreamPreviewWorkspaceService.cs` is split and below 700 lines;
- source-health scanner has regression tests for over-1000, zero-LF, CR-only and one-physical-line samples;
- behavior equivalence proof passes;
- current-goal and spine-fast validation pass;
- artifact scope passes;
- final worktree is clean.

## Stop / block conditions

Return BLOCKED if:

- service split requires project file changes;
- service split requires Runtime/Unity/provider/schema changes;
- behavior cannot be preserved within allowed files;
- source-health evidence cannot be generated deterministically.

Return FAILED if:

- build/tests regress due to this hotfix and cannot be repaired inside allowed files.

## Final report format

Report:

- Final status.
- Latest commit before/after.
- Push status.
- Files changed.
- Service line count before/after.
- Max file line count after repair.
- Files split/created.
- Source-health guard improvements.
- Behavior equivalence proof.
- Validation tier commands and results.
- Artifact scope result.
- Evidence hygiene.
- Remaining P2/P3 debt.
- Final git status.
- Git commands used and why.

## Mandatory commit/push policy

Always commit and push to `origin/main`, even for GREEN/BLOCKED/FAILED.

Commit message must honestly reflect status:

- `GREEN Goal 092A visual world preview service split source health`
- `BLOCKED Goal 092A visual world preview service split source health`
- `FAILED Goal 092A visual world preview service split source health`
