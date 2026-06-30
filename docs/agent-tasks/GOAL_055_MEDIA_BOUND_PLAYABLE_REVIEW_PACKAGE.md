# Codex task — Goal 055 Media-Bound Playable Review Package Smoke

## Assignment metadata

Repository:

```text
https://github.com/kpCat/LLMGameCreator
```

Working copy:

```text
C:\Users\endim\LLMGameCreator\
```

Branch:

```text
main
```

Composite goal id/name:

```text
goal_055_media_bound_playable_review_package_smoke
Goal 055: Media-Bound Playable Review Package Smoke
```

Required gate marker:

```text
media_bound_playable_review_package_verification required
```

Codex reasoning level:

```text
very high
```

## Status policy

This task must end with a commit and push to `origin/main` regardless of final status.

Use one of these statuses:

- `GREEN`: implementation and final checks passed.
- `BLOCKED`: useful bounded work was completed, but a required proof could not be produced without forbidden scope, unavailable Unity tooling, or unsafe assumptions.
- `FAILED`: implementation could not be completed, but diagnostics/evidence were produced.

Commit message policy:

```text
GREEN Goal 055 media-bound playable review package smoke
BLOCKED Goal 055 media-bound playable review package smoke
FAILED Goal 055 media-bound playable review package smoke
```

Do not pretend a non-green result is accepted. Do not mark the Goal 055 gate passed.

## Preflight: accept Goal 054 by user handoff

The user reported Goal 054 as GREEN with:

- commit `ab0d1bf5`
- `media_materialization_review_package_verification required`
- physical media proof: 9 PNG, 3 WAV, 3 bundle JSON
- `check-all.ps1`: 1033/1033 passed
- artifact scope guard: 13/13 allowed, 0 violations

Before implementing Goal 055, update the state docs to record:

```text
media_materialization_review_package_verification passed
```

as accepted by user handoff before Goal 055.

Do not mark Goal 031 or Goal 032 passed unless current docs already do so. Preserve them as produced-for-review/not passed if that is the current model.

## Read-first list

Read first, in order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_055_MEDIA_BOUND_PLAYABLE_REVIEW_PACKAGE_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_055_MEDIA_BOUND_PLAYABLE_REVIEW_PACKAGE.md`
8. Goal 047 artifacts under `.llmgc/procedural/goal-047-full-generator-without-media-dry-run/`
9. Goal 053 artifacts under `.llmgc/procedural/goal-053-media-asset-campaign-orchestration/`
10. Goal 054 artifacts under `.llmgc/procedural/goal-054-media-materialization-review-package/`
11. Existing Application seams with similar source-loader/evidence-service patterns:
    - `src/LLMGameCreator.Application/Design/MediaAssetCampaignOrchestration/**`
    - `src/LLMGameCreator.Application/Design/MediaMaterializationReviewPackage/**`
    - `src/LLMGameCreator.Application/Design/FullGeneratorWithoutMediaDryRun/**`
    - `src/LLMGameCreator.Application/Design/ChunkedRuntimePreviewExportSmoke/**`
12. Existing Unity Alpha project paths only if needed:
    - `unity/LLMGameCreatorAlpha/Assets/**`
    - existing Unity product-smoke/staging docs/tests if discoverable by local search
13. Existing artifact scope policy and check scripts.

Do not read the whole repository unless a local search shows the exact relevant files are elsewhere.

## Allowed files / areas

You may create or edit only:

```text
docs/GOAL_055_MEDIA_BOUND_PLAYABLE_REVIEW_PACKAGE_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_055_MEDIA_BOUND_PLAYABLE_REVIEW_PACKAGE.md
docs/agent-tasks/GOAL_055_MEDIA_BOUND_PLAYABLE_REVIEW_PACKAGE.md
docs/agent-tasks/GOAL_055_LAUNCHER.txt
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/MediaBoundPlayableReviewPackage/**
tests/LLMGameCreator.Tests/Application/MediaBoundPlayableReviewPackage/**
tests/LLMGameCreator.Tests/ProductSmoke/MediaBoundPlayableReviewPackageProductSmokeTests.cs
.llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/**
unity/LLMGameCreatorAlpha/Assets/**
```

The Unity path is allowed only for a narrow media manifest/load/logging integration. If no Unity change is needed, prefer no Unity source change.

## Forbidden files / areas

Do not modify:

```text
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/** provider/LLM/RAG paths
src/LLMGameCreator.Scripting/**
generator-library/**
samples/**
templates/**
*.sln
*.csproj
```

Do not add dependencies. Do not call providers/network/LLM/RAG. Do not execute Lua. Do not generate real media with external tools.

## Exact behavior

### 1. Source loading

Create an Application-only seam under:

```text
src/LLMGameCreator.Application/Design/MediaBoundPlayableReviewPackage/
```

It must load compact source facts from Goal 047, Goal 053 and Goal 054 artifacts. It must not rely on timestamps or absolute paths.

### 2. Media-bound review package model

Create deterministic records for:

- source manifest
- family review package
- staged media file
- streaming-assets media manifest
- media binding record
- media-bound preview payload
- Unity media load contract
- media load proof record
- family smoke result
- invalid/fake/leak diagnostics

Required families:

```text
map_panel_rpg
survival_sandbox
first_person_grid_dungeon
```

### 3. Physical package staging

For each family, stage selected Goal 054 physical media into a Goal 055 review package folder with stable compact names.

Each staged file must record:

- relative path
- media kind
- slot id
- family id
- source Goal 054 id/path/hash
- SHA-256 of staged bytes
- size in bytes
- license/provenance decision

The staged package must include at least:

- one image fixture per family
- one audio fixture per family if available from Goal 054
- one bundle JSON per family if available from Goal 054
- `README.md` or checklist file
- `media-bound-playable-manifest.json`

### 4. Unity-compatible media load proof

Build a bounded Unity-compatible media load contract.

If an existing Unity Alpha runtime manifest loader/staging pattern can be reused safely, update it narrowly so the Alpha runtime can read the Goal 055 media manifest from StreamingAssets or a staged package path and log deterministic proof lines.

Required proof lines or equivalent structured proof records:

```text
MEDIA_BOUND_MANIFEST_LOADED family=<family>
MEDIA_BOUND_IMAGE_LOADED family=<family> slot=<slot> width=<w> height=<h> sha256=<hash>
MEDIA_BOUND_WAV_VALIDATED family=<family> slot=<slot> sampleRate=<rate> channels=<channels> sampleCount=<count> sha256=<hash>
MEDIA_BOUND_FAMILY_PANEL_READY family=<family>
```

For PNG, use a BCL validator in Application tests and, if Unity code is touched, Unity `ImageConversion.LoadImage` or existing image-loading pattern.

For WAV, use a narrow PCM WAV header/data validator. If Unity code is touched and an AudioClip proof is practical, use a tiny internal PCM WAV parser and `AudioClip.Create`/`SetData`; otherwise log WAV structural validation rather than pretending playback occurred.

If Unity CLI/build/player execution is available through existing repo scripts/product smoke, run the narrow media-bound smoke. If not available, do not fabricate logs. Either:

- produce GREEN only if Application-level package, Unity-compatible contract, physical media proof and product smoke are sufficient per existing repo pattern; or
- produce BLOCKED with exact missing Unity executable/tooling reason.

### 5. Preview/export payload proof

Produce media-bound preview/export payloads that connect:

```text
family dry-run -> media-bound package -> staged media -> manifest -> proof record
```

No public GamePackage schema changes.

### 6. Invalid/fake/leak matrix

Cover at least:

- missing Goal 054 source
- missing staged file
- stale hash
- malformed PNG
- malformed WAV
- unsafe relative path / path traversal
- duplicate binding id
- fake family id
- fake slot id
- license/provenance blocked candidate promoted
- provider/network/LLM/RAG claim
- Lua execution claim
- Runtime/UI/GamePackage schema mutation claim
- Unity broad mutation claim
- nondeterministic ordering
- missing review trace
- fake Unity proof line

Each invalid case must produce a causal diagnostic code.

### 7. Evidence artifacts

Write compact deterministic evidence under:

```text
.llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/
```

Required files:

```text
source-manifest.json
media-bound-review-package-manifest.json
streaming-assets-media-manifest.json
media-bound-preview-payloads.json
unity-media-load-contract.json
unity-media-load-proof-map-panel-rpg.json
unity-media-load-proof-survival-sandbox.json
unity-media-load-proof-first-person-grid-dungeon.json
media-bound-family-smoke-matrix.json
invalid-media-bound-package-diagnostics-matrix.json
artifact-scope-report.json
media-bound-playable-review-package-report.md
```

The markdown report must include:

```text
media_bound_playable_review_package_verification required
accepted=false
providerCalls=false
networkImports=false
llmCalls=false
luaExecuted=false
publicGamePackageSchemaChanged=false
```

### 8. Docs/state update

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
```

Expected final state:

- Goal 054 accepted by user handoff.
- Goal 055 produced for review.
- `media_bound_playable_review_package_verification required`.
- Goal 056 recommended, not started.
- Goal 031 and Goal 032 remain produced-for-review/not passed if that is current state.

### 9. Artifact scope policy

Update `.devflow/artifact-scope/artifact-scope-policy.json` with a bounded Goal 055 entry so the final scope guard can pass.

## Tests

Add focused tests for:

- source loading from Goal 047/053/054 artifacts
- staged file hash/provenance validation
- PNG validator
- WAV validator
- media-bound manifest stability
- media-bound preview/export payload stability
- Unity media load contract/proof records
- family smoke matrix for all three families
- invalid/fake/leak diagnostics matrix
- deterministic artifact generation

Add a product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/MediaBoundPlayableReviewPackageProductSmokeTests.cs
```

## Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~MediaBoundPlayableReviewPackage|FullyQualifiedName~Goal055"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~MediaBoundPlayableReviewPackageProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal055|FullyQualifiedName~MediaBound"

.\.devflow\scripts\check-all.ps1
```

Run the existing artifact scope guard after `check-all.ps1` if the repository has the standard script.

If there is an existing Unity Alpha product-smoke route suitable for this task, run the narrowest exact media-bound route. Do not run broad Unity rebuild loops repeatedly.

## Bounded repairs pre-authorized

You may perform these bounded repairs if needed:

1. Update stale current-state/handoff guard tests if they only hardcode the previous latest gate and can be made current-state-consistent without weakening historical assertions.
2. Restore exact check-all-mutated historical `.llmgc/procedural/**` artifacts from HEAD with `git restore --source=HEAD -- <exact paths>`, but not Goal 055 artifacts/code/docs.
3. Copy existing real historical logs only from a repo-local generated cache to the exact expected path if check-all requires them and report source -> target. Do not fabricate logs.
4. Update artifact-scope policy for Goal 055 only.

## Stop / status conditions

Use `GREEN` only if the evidence proves physical media staged, hashed, bound, validated and consumed by the media-bound review/playable package proof.

Use `BLOCKED` if:

- a required Unity CLI/build/player proof is unavailable and the repo pattern requires it;
- the task would need public GamePackage schema, Runtime, WinForms/UI, provider/LLM/RAG, Lua, generator-library, `.sln` or `.csproj` changes;
- real media provider/importer calls would be needed;
- physical media proof cannot be produced from Goal 054.

Use `FAILED` if implementation cannot be made coherent.

## Git policy

Must commit and push final state to `origin/main` even for BLOCKED/FAILED.

Allowed git commands:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <explicit changed paths>
git diff --stat --cached
git add <explicit allowed paths>
git commit -m "GREEN Goal 055 media-bound playable review package smoke"
git commit -m "BLOCKED Goal 055 media-bound playable review package smoke"
git commit -m "FAILED Goal 055 media-bound playable review package smoke"
git push origin main
```

Forbidden:

```text
git checkout
git reset
git clean
git stash
git merge
git rebase
git cherry-pick
git push --force
```

## Final report format

Report in Russian:

```text
Goal 055 выполнен / заблокирован / провален
Status: GREEN / BLOCKED / FAILED
Gate: media_bound_playable_review_package_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Physical media package proof:
<количество файлов, PNG/WAV/bundle, hashes/provenance>

Unity/media-bound proof:
<что реально доказано; были ли Unity source/build/player/logs>

Evidence artifacts:
<список>

Проверки:
<commands/results>

Invalid/fake/leak matrix:
<covered scenarios>

Git:
<commit hash/push result/status>

Ограничения:
<что не трогалось>

Следующий разумный шаг:
<Goal 056 proposal>
```
