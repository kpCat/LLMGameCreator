# Goal 102B — Actual Unity Editor Source Reformat & Evidence Trust Repair

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`
Codex reasoning: very high

## Objective

Repair the real Goal102/102A source-format failure.

External audit after Goal102A found that `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs` is still physically one line in repository raw source at commit `62f883b`, despite Goal102A evidence claiming it is readable and repaired.

This is a P0/P1 evidence-trust and source-health repair. Do not add features.

Goal102B must:
1. actually reformat `OfflineGeoworldPreviewWindow.cs` into readable multi-line C#;
2. prove the actual committed file was one-line before this goal using `git show HEAD:<path>` / byte-level scan, not a synthetic sample;
3. prove the actual working tree file is multi-line after repair;
4. repair/replace misleading Goal102A evidence status so future audits do not trust false GREEN claims;
5. add a focused trust check preventing synthetic-before-only source-format evidence from passing when the actual file remains malformed.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes `62f883b GREEN Goal 102A Unity editor source format guard`.
4. Confirm actual HEAD file is malformed before editing:
   - `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs`;
   - use raw byte / physical-line scan on `git show HEAD:unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs`;
   - expected before: one physical line / minified multi-statement C#.
5. Confirm Goal102A evidence conflicts with the actual file and record this as root cause.
6. Record `AlphaRuntimeBootstrap.cs` hash/line count before work and do not modify it.
7. Inspect dirty state. Do not stage/revert unrelated user work.

## Read first

- `AGENTS.md`
- `docs/GOAL_PRODUCTIVITY_POLICY.md`
- `docs/VALIDATION_PIPELINE.md`
- `.llmgc/procedural/goal-102a-unity-editor-source-format-guard/unity-editor-source-format-guard-report.md`
- `.llmgc/procedural/goal-102a-unity-editor-source-format-guard/unity-editor-source-format-quality-gate.json`
- `.llmgc/procedural/goal-102a-unity-editor-source-format-guard/unity-editor-source-format-scan-before-after.json`
- `src/LLMGameCreator.Application/Design/OfflineGeoworldUnityEditorPreviewTool/OfflineGeoworldUnityEditorSourceFormatGuardScanner.cs`
- `src/LLMGameCreator.Application/Design/OfflineGeoworldUnityEditorPreviewTool/OfflineGeoworldUnityEditorSourceFormatGuardEvidenceService.cs`
- `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`

## Allowed files / areas

- `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs`
- `src/LLMGameCreator.Application/Design/OfflineGeoworldUnityEditorPreviewTool/`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldUnityEditorPreviewTool/`
- `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldUnityEditorPreviewToolProductSmokeTests.cs`
- `.llmgc/procedural/goal-102a-unity-editor-source-format-guard/` only for narrow superseded/corrected marker if needed
- `.llmgc/procedural/goal-102b-actual-unity-editor-source-reformat/`
- docs quartet, debt register, artifact-scope policy
- `docs/agent-tasks/goal-102b-actual-unity-editor-source-reformat/`

## Forbidden files / areas

Do not change Runtime, public GamePackage schema, providers, Lua, generator-library, `.sln`, `.csproj`, lock files, `AlphaRuntimeBootstrap.cs`, Unity scenes/prefabs/asmdefs/settings/packages/build settings, StreamingAssets payloads, binary/raster media, real geodata, network code, LFZ archive/source, or external dependencies. No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Actually reformat the Unity editor source

Rewrite `OfflineGeoworldPreviewWindow.cs` as readable multi-line C#.

Required:
- `git diff --name-only` must include `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs`;
- keep behavior equivalent;
- keep menu path `LLMGameCreator/Offline Geoworld Preview`;
- keep payload root `LLMGameCreator/OfflineGeoworldGoal101`;
- keep create/clear/refresh behavior;
- normal using directives on separate lines;
- namespace/type/method blocks with indentation;
- no line > 180 characters unless unavoidable;
- file physical line count should be at least 80;
- no one-line multi-statement body.

### 2. Repair source-format guard to inspect actual file

Update guard/evidence service so it scans actual HEAD-before file bytes using `git show HEAD:<path>` or equivalent repository object extraction before evidence generation, and actual current working tree bytes after repair. Synthetic negative samples may remain, but cannot substitute for actual before/after scan.

### 3. Supersede misleading Goal102A evidence

Goal102B evidence must explicitly say Goal102A evidence was insufficient/misleading because it passed a synthetic before sample while the actual repo source remained one-line. Record this in debt/state. If updating Goal102A artifacts, add a superseded/corrected marker only; do not pretend Goal102A was clean.

### 4. Generate Goal102B evidence

Create `.llmgc/procedural/goal-102b-actual-unity-editor-source-reformat/` with:
- `actual-unity-editor-source-reformat-report.md`
- `actual-unity-editor-source-before-after.json`
- `actual-unity-editor-source-quality-gate.json`
- `actual-unity-editor-source-negative-proof.json`
- `actual-unity-editor-source-trust-audit.json`

Evidence must prove actual HEAD-before scan detected `OfflineGeoworldPreviewWindow.cs` as one-line/minified; actual after scan passes; target file changed; Goal102A evidence-trust defect recorded; `AlphaRuntimeBootstrap.cs` unchanged; no forbidden areas changed.

### 5. Negative proof

Reject at least: actual file remains one-line; target file not included in diff; before scan uses only synthetic sample; evidence claims repaired but raw file still has one physical line; fake pass without reading bytes; attempt to modify AlphaRuntimeBootstrap; Unity scene/project setting changed marker; StreamingAssets payload changed marker.

### 6. Tests

Focused tests:
- scanner rejects actual one-line sample;
- scanner rejects synthetic one-line / zero-LF / CR-only / extreme-line samples;
- scanner accepts repaired actual `OfflineGeoworldPreviewWindow.cs`;
- evidence fails if target file did not change;
- evidence fails if actual file physical line count is 1;
- AlphaRuntimeBootstrap unchanged.

Product smoke:
- build Goal102B evidence from repo root;
- verify actual before/after scan;
- verify target Unity editor source is in changed files;
- verify no forbidden areas.

### 7. Docs/state

Update docs quartet and debt register.

Manual gate: `actual_unity_editor_source_reformat_verification required`

Status: `accepted=false`.

Record Goal102A as superseded by Goal102B for source-format trust. Root cause: guard trusted synthetic before evidence and did not verify actual target file after repair.

## Validation

Use Goal089 tiered validation:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter OfflineGeoworldUnityEditorPreviewTool
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter OfflineGeoworldUnityEditorPreviewToolProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-102b-actual-unity-editor-source-reformat" -FocusedFilter "OfflineGeoworldUnityEditorPreviewTool" -ProductSmokeFilter "OfflineGeoworldUnityEditorPreviewToolProductSmokeTests"
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-102b-actual-unity-editor-source-reformat"
git diff --check
git diff --cached --check
```

Additionally run and report:

```powershell
$path = "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs"
$content = Get-Content -Raw -LiteralPath $path
$lines = ($content -split "`n").Count
$maxLen = (($content -split "`n") | ForEach-Object { $_.Length } | Measure-Object -Maximum).Maximum
"physicalLines=$lines maxPhysicalLineLength=$maxLen"
```

## Quality gate

GREEN only if target Unity editor source is actually changed and readable; actual HEAD-before scan detects the real prior one-line file; actual after scan passes; Goal102A trust defect is recorded; scanner rejects synthetic and actual failure classes; no forbidden files changed; AlphaRuntimeBootstrap unchanged; focused/product smoke pass; artifact scope passes; final worktree clean.

## Commit/push

Always commit and push to `origin/main`.

Commit message:
- `GREEN Goal 102B actual Unity editor source reformat`
- `BLOCKED Goal 102B actual Unity editor source reformat`
- `FAILED Goal 102B actual Unity editor source reformat`
