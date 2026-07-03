# Goal 095 — Visual Chunk Cache Unity StreamingAssets Handoff & Probe

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`
Codex reasoning: very high

## Objective

Create the first explicit Unity-facing handoff for the procedural visual-world chunk cache stack.

Goal 093 produced metadata-only visual chunk cache export packages. Goal 094 made them inspectable in the WinForms visual workspace. Goal 095 must mirror a compact, runtime-readable subset of those cache/export artifacts into Unity Alpha `StreamingAssets`, add a small standalone Unity probe script that can validate/read the handoff payload, and add Application-side simulation evidence proving the payload is deterministic and safe.

This goal may touch Unity only in the explicitly allowed areas. Do not modify `AlphaRuntimeBootstrap.cs`, Unity scenes, prefabs, asmdefs, project settings, build settings, packages, Runtime, public GamePackage schema, providers, Lua, generator-library, `.sln`, `.csproj`, dependencies, or binary/raster media.

## Preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes:
   - `bf286b608 GREEN Goal 092A visual world preview service split source health`
   - `7d77687ca GREEN Goal 093 visual chunk cache export contract`
   - `627356b3 GREEN Goal 094 visual chunk cache export inspector`
4. Confirm Goal093 and Goal094 artifacts exist and remain `accepted=false`.
5. Confirm Goal094 report proves cache export group, 4 packages, 93 records, metadata-only runtime sidecar, readback/overlap/negative proof.
6. Record `AlphaRuntimeBootstrap.cs` hash/line count before work and do not modify it.
7. Inspect dirty state; do not stage or revert unrelated user work.

## Read first

- `AGENTS.md`
- `docs/VALIDATION_PIPELINE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/visual-chunk-cache-export-report.md`
- `.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/visual-chunk-cache-export-manifest.json`
- `.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/visual-chunk-cache-runtime-handoff-sidecar.json`
- `.llmgc/procedural/goal-094-visual-chunk-cache-export-inspector/visual-chunk-cache-export-inspector-report.md`
- `.llmgc/procedural/goal-094-visual-chunk-cache-export-inspector/visual-chunk-cache-export-inspector-quality-gate-scan.json`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/EditDrivenGamePackageHandoffProbe.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs` read-only baseline only.

## Allowed

- `src/LLMGameCreator.Application/Design/VisualChunkCacheUnityStreamingAssetsHandoff/`
- `tests/LLMGameCreator.Tests/Application/VisualChunkCacheUnityStreamingAssetsHandoff/`
- `tests/LLMGameCreator.Tests/ProductSmoke/VisualChunkCacheUnityStreamingAssetsHandoffProductSmokeTests.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/VisualChunkCacheHandoffProbe.cs`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/VisualChunkCacheGoal095/`
- `.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/`
- docs quartet, debt register, artifact-scope policy
- this task pack

## Forbidden

No `AlphaRuntimeBootstrap.cs` changes, Unity scenes/settings/packages, Runtime, public GamePackage schema, providers, Lua, generator-library, `.sln`, `.csproj`, dependencies, binary/raster media, prompt dumps, external dependencies, branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Required behavior

### 1. Build compact StreamingAssets payload

Create BCL-only Application service that reads Goal093/094 artifacts and writes both:
- `.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/VisualChunkCacheGoal095/`

Required payload files:
- `visual-chunk-cache-unity-handoff-manifest.json`
- `visual-chunk-cache-package-index.json`
- `visual-chunk-cache-stream-window-index.json`
- `visual-chunk-cache-chunk-key-ledger.json`
- `visual-chunk-cache-runtime-readme.json`

Use repository-relative/source-relative paths only. Keep compact summary/ledger data. No raw full-world dump and no copied heavy logs.

### 2. Add standalone Unity probe

Add `unity/LLMGameCreatorAlpha/Assets/Scripts/VisualChunkCacheHandoffProbe.cs`.

It must:
- read manifest from `Application.streamingAssetsPath`;
- validate expected files exist;
- validate schema/version/package counts;
- expose simple result fields useful in Unity Inspector;
- perform no provider/LLM/network calls;
- do no runtime generation beyond reading already-exported metadata;
- avoid external packages;
- not hardcode GREEN.

### 3. Simulated Unity read proof

.NET tests must simulate Unity read path by reading mirrored StreamingAssets payload and validating:
- manifest exists and is read;
- package count matches Goal093/094;
- stream windows/chunk keys represented;
- runtime handoff sidecar metadata-only status;
- hashes/counts match;
- no raw full-world dump;
- no absolute paths;
- no binary/raster media.

### 4. Negative proof

Reject missing manifest, tampered manifest hash, missing package index, stream-window count mismatch, chunk key ledger mismatch, absolute path in payload, raw full-world dump marker, provider call marker in Unity probe/payload, and fake success without file read.

### 5. Evidence

Create:
- `visual-chunk-cache-unity-handoff-report.md`
- `visual-chunk-cache-unity-handoff-manifest.json`
- `visual-chunk-cache-unity-streamingassets-ledger.json`
- `visual-chunk-cache-unity-probe-source-inventory.json`
- `visual-chunk-cache-unity-simulated-read-proof.json`
- `visual-chunk-cache-unity-negative-proof.json`
- `visual-chunk-cache-unity-source-lineage.json`
- `visual-chunk-cache-unity-quality-gate-scan.json`

Evidence must prove Goal093/094 lineage, StreamingAssets mirror, Unity probe source, unchanged AlphaRuntimeBootstrap hash, no forbidden Unity areas, no Runtime/provider/schema/project/dependency changes.

### 6. Tests

Focused tests: build payload, simulated read proof, negative proof, AlphaRuntimeBootstrap hash unchanged, Unity probe has no provider/LLM/network markers, payload paths relative/compact.

Product smoke: build evidence from repo root, read `.llmgc` evidence and Unity StreamingAssets payload, verify package/record/window/chunk summaries, negative proof, no binary/raster media, no forbidden areas.

### 7. Docs/state

Update docs quartet and debt register.

Manual gate: `visual_chunk_cache_unity_streamingassets_handoff_verification required`
Status: `accepted=false`.

Record this is Unity Alpha handoff/probe only; it does not implement Runtime consumption, live Unity gameplay rendering, final atlas, or runtime streaming.

## Validation

Use Goal089 tiered validation:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualChunkCacheUnityStreamingAssetsHandoff
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualChunkCacheUnityStreamingAssetsHandoffProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-095-visual-chunk-cache-unity-streamingassets-handoff" -FocusedFilter "VisualChunkCacheUnityStreamingAssetsHandoff" -ProductSmokeFilter "VisualChunkCacheUnityStreamingAssetsHandoffProductSmokeTests"
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-095-visual-chunk-cache-unity-streamingassets-handoff"
git diff --check
git diff --cached --check
```

Full `check-all.ps1` is not required unless current-goal/spine-fast indicates shared/core risk.

## Quality gate

GREEN only if:
- no forbidden files changed;
- `AlphaRuntimeBootstrap.cs` unchanged by hash;
- Unity probe and StreamingAssets payload exist;
- payload reads real Goal093/094 data, not hardcoded success;
- simulated Unity read proof passes;
- negative proof passes;
- no Runtime/provider/schema/project/dependency changes;
- no binary/raster media or prompt dumps;
- all new C# files below 700 logical lines and no file above 1000;
- current-goal and spine-fast validation pass;
- artifact scope passes;
- final worktree clean.

## Stop conditions

BLOCKED if Unity handoff requires changing AlphaRuntimeBootstrap, Unity project settings, scenes, asmdefs, packages, external dependencies, or raw heavy dumps. FAILED if build/tests regress and cannot be fixed inside allowed files.

## Final report

Report final status, before/after commits, push status, files changed, StreamingAssets payload, Unity probe behavior, AlphaRuntimeBootstrap hash before/after, simulated read proof, negative proof, source health, validation results, artifact scope, hygiene, debt, final git status, git commands.

## Commit/push

Always commit and push to `origin/main`.

Commit message:
- `GREEN Goal 095 visual chunk cache Unity StreamingAssets handoff`
- `BLOCKED Goal 095 visual chunk cache Unity StreamingAssets handoff`
- `FAILED Goal 095 visual chunk cache Unity StreamingAssets handoff`
