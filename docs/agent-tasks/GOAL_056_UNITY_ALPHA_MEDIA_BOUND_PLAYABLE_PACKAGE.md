# Codex task — Goal 056 Unity Alpha Media-Bound Playable Package

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
goal_056_unity_alpha_media_bound_playable_package
Goal 056: Unity Alpha Media-Bound Playable Package
```

Manual gate:

```text
unity_alpha_media_bound_playable_package_verification required
```

Codex reasoning:

```text
very high
```

## Required preflight

1. Work from `C:\Users\endim\LLMGameCreator\`.
2. Confirm current branch is `main`.
3. Read the required files below.
4. Accept Goal 055 by user handoff before starting Goal 056 implementation:
   - Record `media_bound_playable_review_package_verification passed` in the state docs.
   - Do not mark Goal 056 passed.
5. Inspect current worktree. If there are unrelated user changes, do not overwrite them. Continue only if you can keep your changes separated.

## Read-first list

Read in order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_056_UNITY_ALPHA_MEDIA_BOUND_PLAYABLE_PACKAGE_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_056_UNITY_ALPHA_MEDIA_BOUND_PLAYABLE_PACKAGE.md`
8. Goal 055 artifacts:
   - `.llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/`
9. Goal 054 artifacts:
   - `.llmgc/procedural/goal-054-media-materialization-review-package/`
10. Goal 047 artifacts:
    - `.llmgc/procedural/goal-047-full-generator-without-media-dry-run/`
11. Existing Application seams:
    - `src/LLMGameCreator.Application/Design/MediaBoundPlayableReviewPackage/**`
    - `src/LLMGameCreator.Application/Design/MediaMaterializationReviewPackage/**`
    - `src/LLMGameCreator.Application/Design/FullGeneratorWithoutMediaDryRun/**`
12. Existing Unity Alpha / Unity export / playable routes. Search narrowly for:
    - `UnityPlayableAlpha`
    - `UnityQuestLoop`
    - `UnityMultiVariant`
    - `UnityReadablePresentation`
    - `minimum-playable-generated-game`
    - `LLMGameCreatorAlpha`
    - `StreamingAssets`
    - `Application.streamingAssetsPath`
    - automated player diagnostic/play-loop logs
13. `.devflow/artifact-scope/artifact-scope-policy.json`
14. Existing tests/product smoke naming conventions around Unity Alpha and media-bound artifacts.

Do not read the whole repository unless these searches show exact relevant files elsewhere.

## Allowed files / areas

You may create or edit:

```text
docs/GOAL_056_UNITY_ALPHA_MEDIA_BOUND_PLAYABLE_PACKAGE_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_056_UNITY_ALPHA_MEDIA_BOUND_PLAYABLE_PACKAGE.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/agent-tasks/GOAL_056_UNITY_ALPHA_MEDIA_BOUND_PLAYABLE_PACKAGE.md
docs/agent-tasks/GOAL_056_LAUNCHER.txt
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/UnityAlphaMediaBoundPlayablePackage/**
tests/LLMGameCreator.Tests/Application/UnityAlphaMediaBoundPlayablePackage/**
tests/LLMGameCreator.Tests/ProductSmoke/UnityAlphaMediaBoundPlayablePackageProductSmokeTests.cs
.llmgc/procedural/goal-056-unity-alpha-media-bound-playable-package/**
```

Narrow Unity allowance:

```text
unity/LLMGameCreatorAlpha/**
```

but only for a bounded media manifest/PNG/WAV loader, diagnostic log lines, and simple visible media panel/presentation additions needed to prove Goal 056.

If the actual Unity project path differs, use the existing repo-local Alpha project path discovered from accepted Goal 013-020/055 evidence. Do not create a new Unity project.

## Forbidden files / areas

Do not modify unless this task explicitly requires a BLOCKED final state explaining why:

```text
src/LLMGameCreator.Domain/**
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

Also forbidden:

- new NuGet packages;
- new Unity packages;
- provider/network/media generation;
- live LLM/RAG calls;
- Lua execution or Lua source generation;
- broad Unity refactor;
- GamePackage schema changes;
- WinForms UI changes;
- arbitrary filesystem/network/process/reflection/thread/time/random/native interop beyond existing build/smoke scripts and deterministic file/hash operations required for this proof.

## Exact behavior

### 1. Record Goal 055 acceptance by handoff

Update current state docs so Goal 055 is accepted by user handoff before Goal 056:

```text
media_bound_playable_review_package_verification passed
```

Goal 056 must remain:

```text
unity_alpha_media_bound_playable_package_verification required
```

Do not mark Goal 056 accepted/passed.

### 2. Add Application seam

Create a narrow Application-only seam under:

```text
src/LLMGameCreator.Application/Design/UnityAlphaMediaBoundPlayablePackage/
```

Expected responsibilities:

- load Goal 055 compact evidence and staged package facts;
- validate Goal 055/054/047 source references;
- stage/copy the media-bound review package into Goal 056 evidence;
- build a Unity `StreamingAssets` payload model:
  - manifest;
  - family bindings;
  - relative paths;
  - hashes;
  - PNG/WAV/bundle classification;
  - provenance/license references;
- create a media-bound panel model for:
  - `map_panel_rpg`;
  - `survival_sandbox`;
  - `first_person_grid_dungeon`;
- validate staged files:
  - PNG signature/dimensions/CRC or equivalent existing proof path;
  - PCM WAV RIFF/header/data sample metadata;
  - SHA-256 hashes;
  - safe relative paths;
  - duplicate binding rejection;
  - required file presence;
- write compact evidence artifacts.

Keep classes small. Do not put everything in one giant service.

### 3. Extend Unity Alpha narrowly

Use the existing repo-local Unity Alpha project and its existing automated diagnostic/player smoke pattern.

Add only the smallest needed Unity-side code to:

- locate a staged media manifest through `Application.streamingAssetsPath`;
- parse a simple DTO-friendly manifest shape;
- load at least one PNG fixture per family into a texture or equivalent proof path;
- parse/load at least one WAV fixture or validate/create an `AudioClip` proof path for the deterministic PCM WAV fixtures;
- expose/log per-family media panel proof;
- log deterministic markers that Application tests/product smoke can validate.

Required log markers or semantically equivalent stable markers:

```text
media_bound_manifest_loaded=true
media_bound_family_count=3
media_bound_png_loaded=true
media_bound_wav_loaded=true
media_bound_bundle_loaded=true
media_bound_family_panel_proof=map_panel_rpg
media_bound_family_panel_proof=survival_sandbox
media_bound_family_panel_proof=first_person_grid_dungeon
media_bound_hash_validation=true
media_bound_playable_review_package_verification=required
```

Do not implement a generic Unity asset browser. Do not add UI tabs. A small IMGUI panel extension is acceptable if it follows existing Alpha runtime style.

### 4. Stage StreamingAssets-compatible payload

Stage the selected media-bound package into a deterministic review path under:

```text
.llmgc/procedural/goal-056-unity-alpha-media-bound-playable-package/
```

The staged package should include:

- compact manifest JSON;
- media files copied from Goal 055/054 proof, not regenerated;
- hash inventory;
- family panel proof records;
- compact Unity load proof;
- invalid/fake/leak matrix;
- final report.

If actual Unity build/player smoke requires copying into `unity/LLMGameCreatorAlpha/Assets/StreamingAssets`, keep the source files deterministic and compact, avoid committing heavy generated Unity caches/build outputs, and follow existing `.gitignore` patterns.

### 5. Run real Unity proof if available

A GREEN result requires a real Unity-side consumption proof if the environment has the existing Unity Alpha build/play-loop route available.

Use the existing repo pattern for Unity Alpha product smoke / build / player diagnostic execution. Do not invent a new build pipeline if one already exists.

If Unity is not available or the existing Unity route cannot be run, do not fake the proof. Commit/push `BLOCKED Goal 056 unity alpha media-bound playable package` with:

- completed Application staging/proof artifacts;
- exact Unity/environment blocker;
- evidence that no fake Unity proof was claimed.

### 6. Invalid/fake/leak matrix

Cover at least these cases:

- missing Goal 055 source;
- stale Goal 055 hash;
- missing staged PNG;
- missing staged WAV;
- malformed PNG;
- malformed WAV;
- unsafe relative path;
- duplicate media binding id;
- fake family id;
- fake slot id;
- missing Unity load trace;
- stale Unity load hash;
- provider/network/LLM/RAG claim;
- Lua execution claim;
- GamePackage schema mutation claim;
- Runtime/UI broad mutation claim;
- Unity broad refactor claim;
- nondeterministic ordering;
- missing review/provenance trace.

Each invalid scenario must produce causal diagnostics; no generic failure bucket.

### 7. Evidence report

Write:

```text
.llmgc/procedural/goal-056-unity-alpha-media-bound-playable-package/unity-alpha-media-bound-playable-package-report.md
```

It must include:

```text
implementationStatus=GREEN|BLOCKED|FAILED
accepted=false
manualGate=unity_alpha_media_bound_playable_package_verification
goal055AcceptedByUserHandoff=true
streamingAssetsPayloadStaged=true
physicalMediaFileCount=<number>
pngLoadProofPassed=<true/false>
wavLoadProofPassed=<true/false>
bundleProofPassed=<true/false>
unityEditorOrPlayerExecuted=<true/false>
unityMediaLoadContractPassed=<true/false>
familyMediaPanelProofPassed=<true/false>
invalidMatrixPassed=<true/false>
```

### 8. Docs/current state

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
```

Expected final state if GREEN:

- Goal 055 accepted by user handoff;
- Goal 056 produced for review;
- `unity_alpha_media_bound_playable_package_verification required`;
- recommend next work but do not start it;
- preserve Goal 031 and Goal 032 as produced-for-review/not passed unless current docs already say otherwise.

### 9. Artifact scope policy

Update `.devflow/artifact-scope/artifact-scope-policy.json` with the bounded Goal 056 artifact allowance if needed.

Do not broaden policy globally.

## Tests

Add focused tests under:

```text
tests/LLMGameCreator.Tests/Application/UnityAlphaMediaBoundPlayablePackage/
```

Suggested tests:

- `UnityAlphaMediaBoundSourceLoadingTests`
- `UnityStreamingAssetsPayloadTests`
- `UnityMediaLoadContractTests`
- `UnityMediaBoundFamilyPanelTests`
- `UnityAlphaMediaBoundInvalidMatrixTests`
- `UnityAlphaMediaBoundEvidenceTests`

Add product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/UnityAlphaMediaBoundPlayablePackageProductSmokeTests.cs
```

The tests must prove:

- Goal 055 source facts are consumed, not invented;
- staged files exist and hashes match;
- manifest uses safe relative paths;
- PNG/WAV/bundle facts are validated;
- family panel proof covers all three families;
- Unity load proof is real or status is BLOCKED, never faked;
- invalid/fake/leak matrix is causal;
- evidence is deterministic and JSON parseable.

## Validation commands

Run from repository root:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~UnityAlphaMediaBoundPlayablePackage|FullyQualifiedName~Goal056"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~UnityAlphaMediaBoundPlayablePackageProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal056|FullyQualifiedName~UnityAlphaMediaBound"

.\.devflow\scripts\check-all.ps1
```

Run artifact scope guard using the existing repo script/pattern after `check-all.ps1` is green. Do not invent a new guard.

Also inspect:

```powershell
Get-ChildItem .\.llmgc\procedural\goal-056-unity-alpha-media-bound-playable-package -Recurse -File | Sort-Object FullName | Select-Object FullName,Length
```

Check changed text files for mojibake markers.

If Unity CLI/player route is required by existing product smoke or proof, run the existing route and include exact command/results in the final report.

## Bounded repairs pre-authorized

You may perform these bounded repairs without stopping:

1. Update stale current-state/handoff guard tests if they hardcode the previous latest gate instead of reading current state consistency.
2. Restore exact accidental historical `.llmgc/procedural/**` artifacts from HEAD if `check-all.ps1` mutates them outside Goal 056 scope.
3. Add a narrow Goal 056 artifact-scope policy entry.
4. Fix deterministic ordering/hashing issues inside Goal 056 scope.
5. Adjust Goal 056 docs/state wording if current-state tests require exact consistency.

Do not use `git reset`, `git clean`, `git stash`, `git checkout`, `git merge`, `git rebase`, `git cherry-pick`, or force push.

## Git policy

Codex must commit and push the final state to `origin/main` regardless of GREEN/BLOCKED/FAILED result.

Allowed final commit messages:

```text
GREEN Goal 056 unity alpha media-bound playable package
BLOCKED Goal 056 unity alpha media-bound playable package
FAILED Goal 056 unity alpha media-bound playable package
```

Use explicit path staging. Do not stage unrelated files.

## Final report format

Report in Russian:

```text
Goal 056 выполнен / заблокирован / провален

Status:
GREEN / BLOCKED / FAILED

Gate:
unity_alpha_media_bound_playable_package_verification required

Что стало реальнее:
<1-3 предложения>

Goal 055 acceptance:
<exact wording>

Изменённые файлы:
<paths grouped by docs/src/tests/unity/artifacts/policy>

Unity proof:
<Unity source changed? Unity editor/player executed? markers? logs?>

Physical media proof:
<count PNG/WAV/bundle, hashes/provenance, staged package>

Проверки:
<commands and results>

Invalid/fake/leak matrix:
<summary>

Bounded repairs:
<summary or none>

Git:
<commit hash, push result, final worktree>

Ограничения:
<explicit no-go areas not touched>

Следующий разумный шаг:
<next goal recommendation>
```
