# Goal 094 — Visual Chunk Cache Export Inspector & Workspace Integration

## Repo

https://github.com/kpCat/LLMGameCreator

## Working copy

`C:\Users\endim\LLMGameCreator\`

## Branch

`main`

## Codex reasoning

very high

## Primary objective

Integrate Goal 093 visual chunk cache/export artifacts into the existing visual world review workflow.

Goal 093 produced metadata-only cache export packages and runtime handoff sidecar evidence over real Goal091 stream-window artifacts. Goal 094 must make those cache/export artifacts inspectable through the existing Visual World Stream Preview Workspace Application seam and WinForms UI, without changing Runtime, Unity, public GamePackage schema, providers, Lua, generator-library, project files, dependencies, or binary media.

This is not a new renderer. It is an editor/review integration goal:
- load real Goal093 cache/export artifacts;
- surface package summaries, record counts, chunk keys, handoff sidecar metadata, invalidation rules and negative proof status;
- add compact evidence proving the workspace reads Goal093 artifacts rather than reporting hardcoded success;
- avoid reintroducing oversized source files.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes:
   - `bf286b608 GREEN Goal 092A visual world preview service split source health`
   - `7d77687ca GREEN Goal 093 visual chunk cache export contract`
4. Confirm Goal093 artifacts exist and remain `accepted=false`.
5. Confirm Goal093 report proves:
   - 4 export packages;
   - 93 records;
   - readback proof passed;
   - overlap reuse proof passed;
   - negative proof passed;
   - runtime handoff sidecar is metadata-only.
6. Confirm current `VisualWorldStreamPreviewWorkspace` files remain below source-health limits after Goal092A split.
7. Inspect dirty state. Do not stage/revert unrelated user work.

## Read first

- `AGENTS.md`
- `docs/VALIDATION_PIPELINE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.llmgc/procedural/goal-092a-visual-world-preview-service-split-source-health/quality-gate-scan.json`
- `.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/visual-chunk-cache-export-report.md`
- `.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/visual-chunk-cache-export-manifest.json`
- `.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/visual-chunk-cache-runtime-handoff-sidecar.json`
- `.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/visual-chunk-cache-invalidation-matrix.json`
- `.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/visual-chunk-cache-readback-proof.json`
- `.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/visual-chunk-cache-overlap-reuse-proof.json`
- `.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/visual-chunk-cache-negative-proof.json`
- `.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/visual-chunk-cache-quality-gate-scan.json`
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs`

## Allowed files / areas

- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs`
- `.llmgc/procedural/goal-094-visual-chunk-cache-export-inspector/`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.devflow/artifact-scope/artifact-scope-policy.json`
- `docs/agent-tasks/goal-094-visual-chunk-cache-export-inspector/`

Optional only if absolutely necessary:
- `.llmgc/procedural/goal-092-visual-world-stream-preview-workspace/`

## Forbidden files / areas

Do not change:
- `src/LLMGameCreator.Application/Design/VisualChunkCacheExportContract/` unless a narrow read DTO bug is unavoidable; prefer reading Goal093 JSON from the workspace seam without changing Goal093 code.
- Runtime / Runtime.Abstractions.
- Unity files, including `AlphaRuntimeBootstrap.cs`.
- Public GamePackage schema.
- Infrastructure provider / LLM / RAG / media provider code.
- Lua / Scripting.
- generator-library.
- `.sln`, `.csproj`, lock files.
- binary/raster media assets.
- prompt dumps, generated provider outputs or real NSFW assets.
- external dependencies.

No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Extend workspace Application seam

Extend the existing Visual World Stream Preview Workspace Application seam so it also discovers Goal093 cache/export artifacts.

Add cohesive helper files if needed. Do not bloat existing files.

Required new catalog concepts:
- cache export group;
- export package entry;
- cache record count;
- source chunk count;
- stream window count;
- target kind: editor review / runtime handoff;
- runtime handoff sidecar metadata-only flag;
- invalidation matrix status;
- readback proof status;
- overlap reuse proof status;
- negative proof status;
- no raw full-world dump flag.

The workspace must read real files under `.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/`, not hardcoded success.

### 2. Update WinForms UI

Update the existing Visual World Stream Preview Workspace UI to surface cache/export entries.

Acceptable UI:
- a new group in existing artifact group list;
- selected entry details showing package id, record count, target, handoff metadata and proof status;
- diagnostics panel.

Do not add WebView2/SVG rendering/browser dependencies.

Keep the UI bounded and UserControl-based.

### 3. Generate Goal094 evidence

Create `.llmgc/procedural/goal-094-visual-chunk-cache-export-inspector/`.

Recommended artifacts:
- `visual-chunk-cache-export-inspector-report.md`
- `visual-chunk-cache-export-inspector-catalog.json`
- `visual-chunk-cache-export-inspector-proof-status.json`
- `visual-chunk-cache-export-inspector-winforms-binding-inventory.json`
- `visual-chunk-cache-export-inspector-quality-gate-scan.json`
- `source-health-scan.json`

Evidence must prove:
- Goal093 files discovered by relative paths;
- at least 4 export packages are represented;
- 93 cache records are represented or summarized consistently with Goal093 report;
- runtime handoff sidecar is represented and metadata-only;
- readback/overlap/negative proofs are surfaced;
- workspace binding is real;
- no absolute paths, binary media, provider calls, Runtime/Unity/schema/project changes;
- source-health limits pass.

### 4. Tests

Focused tests must prove:
- workspace service loads Goal093 artifacts from repo root;
- cache export group exists;
- all four Goal093 packages are represented;
- runtime handoff sidecar entry is visible;
- readback/overlap/negative proof statuses are surfaced;
- missing Goal093 artifact yields diagnostics rather than fake GREEN;
- no workspace file exceeds 700 logical lines and no file exceeds 1000 logical lines.

Product smoke must:
- build Goal094 evidence from repo root;
- read catalog/proof/binding/quality artifacts;
- verify the cache export inspector group and runtime handoff sidecar are present;
- verify no binary/raster media or forbidden areas.

### 5. Docs/state

Update docs quartet and debt register.

Goal094 manual gate:
`visual_chunk_cache_export_inspector_verification required`

Goal094 `accepted=false`.

Record that this is editor/review integration only; it does not implement runtime consumption or Unity consumption.

## Validation policy

Use Goal089 tiered validation.

Required:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspace
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspaceProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-094-visual-chunk-cache-export-inspector" -FocusedFilter "VisualWorldStreamPreviewWorkspace" -ProductSmokeFilter "VisualWorldStreamPreviewWorkspaceProductSmokeTests"
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-094-visual-chunk-cache-export-inspector"
git diff --check
git diff --cached --check
```

Full `check-all.ps1` is not required for this ordinary workspace integration goal unless current-goal/spine-fast indicates shared/core risk.

## Quality gate

GREEN only if:
- no forbidden files changed;
- Goal093 cache/export artifacts are loaded through real Application seam;
- at least 4 export packages and runtime handoff sidecar are surfaced;
- readback/overlap/negative proof status is surfaced;
- WinForms binding is real;
- no file in touched workspace namespace exceeds 700 logical lines; no file in repo changes exceeds 1000 logical lines;
- no Runtime/Unity/provider/schema/project/dependency changes;
- no binary/raster media or prompt dumps;
- current-goal and spine-fast validation pass;
- artifact scope passes;
- final worktree is clean.

## Stop / block conditions

Return BLOCKED if:
- Goal093 artifacts cannot be loaded without changing Goal093 code or forbidden zones;
- UI integration requires external rendering/browser dependency;
- artifact scope cannot be satisfied;
- source-health limits cannot be kept without broad refactor.

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
- Cache export packages surfaced.
- Runtime handoff sidecar visibility proof.
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
- `GREEN Goal 094 visual chunk cache export inspector`
- `BLOCKED Goal 094 visual chunk cache export inspector`
- `FAILED Goal 094 visual chunk cache export inspector`
