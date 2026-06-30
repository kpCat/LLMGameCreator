# Codex task - Goal 058 Full Media-Bound Generator Campaign

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
goal-058-full-media-bound-generator-campaign
Goal 058: Full Media-Bound Generator Campaign
```

Required goal marker / gate:

```text
full_media_bound_generator_campaign_verification required
```

Codex reasoning level:

```text
very high
```

## Mandatory outcome policy

You must commit and push final state to `origin/main` regardless of GREEN/BLOCKED/FAILED status.

Use honest commit messages:

```text
GREEN Goal 058 full media-bound generator campaign
BLOCKED Goal 058 full media-bound generator campaign
FAILED Goal 058 full media-bound generator campaign
```

Never pretend a non-green result is accepted. Never mark the manual gate passed unless explicitly instructed by the user.

## Starting context

Goal 057 was completed GREEN and pushed by the user/Codex handoff. The user's current instruction is to continue aggressively without separate acceptance-only tasks.

Preflight requirement: record Goal 057 as accepted by user handoff in the docs/state before starting Goal 058 implementation:

```text
unity_alpha_multifamily_playable_loop_verification passed
```

Then implement Goal 058 and leave its gate as:

```text
full_media_bound_generator_campaign_verification required
```

Goal 031 and Goal 032 must remain produced-for-review/not passed unless current docs already model them differently. Do not invent acceptance for them.

## Read-first list

Read these first, in this order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_058_FULL_MEDIA_BOUND_GENERATOR_CAMPAIGN_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_058_FULL_MEDIA_BOUND_GENERATOR_CAMPAIGN.md`
8. Goal 057 artifacts under `.llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop/`
9. Goal 056 artifacts under `.llmgc/procedural/goal-056-unity-alpha-media-bound-playable-package/`
10. Goal 055 artifacts under `.llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/`
11. Goal 054 artifacts under `.llmgc/procedural/goal-054-media-materialization-review-package/`
12. Goal 053 artifacts under `.llmgc/procedural/goal-053-media-asset-campaign-orchestration/`
13. Goal 047 artifacts under `.llmgc/procedural/goal-047-full-generator-without-media-dry-run/`
14. Goal 043 artifacts under `.llmgc/procedural/goal-043-multi-family-generated-template-vertical-slice/`
15. Goal 040 artifacts under `.llmgc/procedural/goal-040-chunked-runtime-preview-export-multifamily-smoke/`
16. Goal 039 artifacts under `.llmgc/procedural/goal-039-runtime-chunk-delta-traversal-smoke/`
17. Goal 038 artifacts under `.llmgc/procedural/goal-038-world-scale-region-map-foundation/`
18. Existing code/tests under:
    - `src/LLMGameCreator.Application/Design/UnityAlphaMultiFamilyPlayableLoop/`
    - `src/LLMGameCreator.Application/Design/UnityAlphaMediaBoundPlayablePackage/`
    - `src/LLMGameCreator.Application/Design/MediaBoundPlayableReviewPackage/`
    - `src/LLMGameCreator.Application/Design/FullGeneratorWithoutMediaDryRun/`
    - `tests/LLMGameCreator.Tests/ProductSmoke/*UnityAlpha*`
    - `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`

Do not scan the whole repository unless a focused search shows a referenced type/path moved.

## Allowed files / areas

You may create or edit:

```text
docs/GOAL_058_FULL_MEDIA_BOUND_GENERATOR_CAMPAIGN_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_058_FULL_MEDIA_BOUND_GENERATOR_CAMPAIGN.md
docs/agent-tasks/GOAL_058_FULL_MEDIA_BOUND_GENERATOR_CAMPAIGN.md
docs/agent-tasks/GOAL_058_LAUNCHER.txt
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/FullMediaBoundGeneratorCampaign/**
tests/LLMGameCreator.Tests/Application/FullMediaBoundGeneratorCampaign/**
tests/LLMGameCreator.Tests/ProductSmoke/FullMediaBoundGeneratorCampaignProductSmokeTests.cs
.llmgc/procedural/goal-058-full-media-bound-generator-campaign/**
```

Narrow Unity allowance, only if needed to emit campaign-run markers through the existing Alpha route:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

If you can satisfy the goal without changing Unity source, prefer not to change it.

## Forbidden files / areas

Do not touch:

```text
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/** provider/LLM/RAG/media paths
src/LLMGameCreator.Scripting/**
generator-library/**
samples/**
templates/**
*.sln
*.csproj
```

Do not add external dependencies.

Do not create provider calls, network calls, LLM calls, RAG calls, ComfyUI/Fooocus calls, media import calls or arbitrary Lua execution.

Do not change public GamePackage schema.

## Exact behavior

### 1. Preflight acceptance and source audit

- Confirm branch `main`.
- Record Goal 057 acceptance by user handoff in the docs/state:

```text
unity_alpha_multifamily_playable_loop_verification passed before Goal 058
```

- Do not mark Goal 058 accepted.
- Load/validate source facts from Goals 034-057 needed for the campaign. At minimum, consume Goal 043, 047, 053, 054, 055, 056 and 057 artifacts.
- Produce a source manifest that records source artifact paths, source hashes, source goal ids, required fields and causal diagnostics.

### 2. Campaign runner

Create an Application-only seam under:

```text
src/LLMGameCreator.Application/Design/FullMediaBoundGeneratorCampaign/
```

Suggested component split, adapt to local style:

```text
FullMediaBoundGeneratorCampaignModels
FullMediaBoundGeneratorCampaignSourceLoader
FullMediaBoundGeneratorCampaignBuilder
FullMediaBoundGeneratorCampaignValidator
FullMediaBoundGeneratorCampaignEvidenceService
FullMediaBoundGeneratorCampaignHash
FullMediaBoundUnityProofRunner
```

The campaign runner must expose a single deterministic entrypoint equivalent to:

```text
RunFullMediaBoundCampaign(seed/profile/family set)
```

It must build one unified campaign result for the three families:

```text
map_panel_rpg
survival_sandbox
first_person_grid_dungeon
```

### 3. Campaign stages

The campaign result must include stage records for:

- strict draft / quarantined candidate source facts;
- Lua manifest/sandbox/expansion source facts when available;
- world region/chunk/runtime delta source facts;
- family simulatable loop source facts;
- full generator without media dry-run source facts;
- media materialization/review package source facts;
- Unity Alpha media-bound package source facts;
- Unity Alpha multi-family playable loop source facts;
- campaign review package plan;
- campaign Unity/player command plan;
- campaign preview/export payload.

Do not duplicate all prior artifacts. Reference them by path/hash/id and build new campaign-level summaries.

### 4. Review package / StreamingAssets staging

Create a Goal 058 review package under:

```text
.llmgc/procedural/goal-058-full-media-bound-generator-campaign/review-package/
```

It should include compact staged payloads needed by the existing Unity Alpha route, such as:

```text
StreamingAssets/full-media-bound-campaign-manifest.json
StreamingAssets/family-command-plan.json
StreamingAssets/media-bound-manifest.json
```

Reuse/copy compact physical fixture media from accepted prior evidence only when needed. Keep hashes/provenance.

No heavy Unity build/log/cache output should be tracked.

### 5. Unity/player proof

If the existing Unity Alpha automation route is available, run it and collect real proof.

Required proof markers should include family-specific and campaign-specific lines equivalent to:

```text
campaign_loaded=goal058
campaign_family=map_panel_rpg
campaign_family=survival_sandbox
campaign_family=first_person_grid_dungeon
campaign_family_completed=map_panel_rpg
campaign_family_completed=survival_sandbox
campaign_family_completed=first_person_grid_dungeon
campaign_media_bound=true
campaign_review_package_proof=goal058
```

If AlphaRuntimeBootstrap already emits enough markers from a command plan, do not change Unity source. If it cannot emit campaign-specific proof, use the narrow allowed Unity file to add only deterministic marker support.

GREEN requires real Unity/player proof with matching markers. If Unity proof cannot run honestly, commit/push BLOCKED.

### 6. Required evidence artifacts

Write compact deterministic artifacts under:

```text
.llmgc/procedural/goal-058-full-media-bound-generator-campaign/
```

Required files:

```text
campaign-source-manifest.json
campaign-plan.json
family-run-map-panel-rpg.json
family-run-survival-sandbox.json
family-run-first-person-grid-dungeon.json
unified-review-package-manifest.json
unity-alpha-campaign-command-plan.json
unity-alpha-campaign-player-proof.json
preview-export-campaign-payload.json
campaign-package-compatibility-proof.json
invalid-campaign-diagnostics-matrix.json
artifact-scope-report.md
full-media-bound-generator-campaign-report.md
```

JSON must be deterministic:

- stable ordering;
- no wall-clock timestamps;
- no absolute machine paths;
- no heavy logs;
- no nondeterministic ids.

### 7. Invalid/fake/leak matrix

Cover at minimum:

- missing Goal 057 source;
- stale source hash;
- fake family id;
- missing family command plan;
- missing media file;
- media hash mismatch;
- missing Unity marker;
- duplicate campaign id;
- unsafe relative path;
- provider/network/LLM/RAG claim;
- real media generation claim;
- Lua arbitrary execution claim;
- Runtime/UI/GamePackage schema mutation claim;
- Unity broad mutation claim;
- nondeterministic order;
- missing review trace;
- self-promotion without validation.

Each invalid scenario must produce causal diagnostics.

### 8. Docs/state update

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
```

Expected final state:

```text
unity_alpha_multifamily_playable_loop_verification passed by user handoff before Goal 058
full_media_bound_generator_campaign_verification required
```

Do not start Goal 059.

## Pre-authorized bounded repairs

To reduce handoff stalls, you may perform these bounded repairs if needed:

1. Update stale current-state/handoff guard tests only if they hardcode the previous latest gate and fail after legitimate Goal 058 state update. Keep historical assertions strict.
2. Restore exact accidental historical `.llmgc/procedural/**` artifacts from HEAD if `check-all.ps1` mutates them outside Goal 058 scope.
3. Copy existing real generated Unity/player logs only if a historical test requires them and the source log is already present in repo-local generated cache. Report source -> target exactly.
4. Add Goal 058 artifact-scope policy entry.
5. If a tiny Unity marker addition is required, restrict it to `AlphaRuntimeBootstrap.cs` and campaign marker emission only.

Still forbidden:

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

## Tests and validation commands

Run focused checks first, then full gate.

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~FullMediaBoundGeneratorCampaign|FullyQualifiedName~Goal058"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~FullMediaBoundGeneratorCampaignProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal058|FullyQualifiedName~FullMediaBound"

.\.devflow\scripts\check-all.ps1
```

Run the existing artifact scope guard for the Goal 058 scenario. Do not invent a new guard.

Also inspect:

```powershell
Get-ChildItem .\.llmgc\procedural\goal-058-full-media-bound-generator-campaign -Recurse -File | Sort-Object FullName | Select-Object FullName,Length
```

Run mojibake marker scan against changed text files/artifacts.

## Git policy

Allowed git inspection/staging commands:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <explicit changed files>
git diff --stat --cached
git diff --cached --check
git add <explicit allowed paths>
git commit -m "GREEN Goal 058 full media-bound generator campaign"
git commit -m "BLOCKED Goal 058 full media-bound generator campaign"
git commit -m "FAILED Goal 058 full media-bound generator campaign"
git push origin main
```

You must commit/push final state even if BLOCKED/FAILED.

## Final report format

Report in Russian:

```text
Goal 058 completed / blocked / failed

Status:
GREEN / BLOCKED / FAILED

Gate:
full_media_bound_generator_campaign_verification required

What became more real:
<1-3 Russian sentences>

Changed files:
<list>

Implemented:
<campaign runner / source manifest / review package / Unity proof / evidence>

Unity/player proof:
<unityExitCode/playerExitCode/markers or blocker>

Evidence artifacts:
<list>

Checks:
<commands and results>

Invalid/fake/leak matrix:
<coverage>

Bounded repairs:
<what was repaired or none>

Git:
<commit hash and push result>

Constraints:
<what was not touched>

Next reasonable step:
<Goal 059 direction>
```
