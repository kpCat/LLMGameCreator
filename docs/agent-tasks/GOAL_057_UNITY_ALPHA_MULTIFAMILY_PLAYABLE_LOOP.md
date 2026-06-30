# Codex Task — Goal 057 Unity Alpha Multi-Family Playable Loop

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
goal_057_unity_alpha_multifamily_playable_loop
Goal 057: Unity Alpha Multi-Family Playable Loop
```

Required gate marker:

```text
unity_alpha_multifamily_playable_loop_verification required
```

Codex reasoning level:

```text
very high
```

## Required starting action

Treat the user handoff as acceptance of Goal 056.

Before implementing Goal 057, update the current-state docs so that Goal 056 is recorded as accepted by user handoff:

```text
unity_alpha_media_bound_playable_package_verification passed
```

Then implement Goal 057 and leave Goal 057 at:

```text
unity_alpha_multifamily_playable_loop_verification required
```

Do not mark Goal 057 passed.

## Goal purpose

Build a real repo-local Unity Alpha multi-family playable loop proof.

Goal 056 proved Unity Alpha can consume media-bound `StreamingAssets` and emit media markers. Goal 057 must prove the Alpha player can select and execute bounded family-specific loops for:

- `map_panel_rpg`;
- `survival_sandbox`;
- `first_person_grid_dungeon`.

This should be a real Unity/player proof goal, not another Application-only report.

## Read-first list

Read first, in order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_057_UNITY_ALPHA_MULTIFAMILY_PLAYABLE_LOOP_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_057_UNITY_ALPHA_MULTIFAMILY_PLAYABLE_LOOP.md`
8. Goal 043 artifacts under `.llmgc/procedural/goal-043-multi-family-generated-template-vertical-slice/`
9. Goal 047 artifacts under `.llmgc/procedural/goal-047-full-generator-without-media-dry-run/`
10. Goal 055 artifacts under `.llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/`
11. Goal 056 artifacts under `.llmgc/procedural/goal-056-unity-alpha-media-bound-playable-package/`
12. Existing Unity Alpha runtime/bootstrap files under `unity/LLMGameCreatorAlpha/Assets/Scripts/`
13. Existing Unity Alpha build/diagnostic/product smoke patterns and scripts. Search narrowly for:
    - `media_bound_`
    - `Application.streamingAssetsPath`
    - `AlphaRuntimeBootstrap`
    - `UnityAlphaMediaBoundPlayablePackage`
    - `BuildPipeline.BuildPlayer`
    - `player log`
    - `play loop`
    - `StreamingAssets`

Do not scan or refactor the entire repository unless a narrow search proves the relevant files moved.

## Allowed files / areas

You may create or edit only these areas:

```text
docs/GOAL_057_UNITY_ALPHA_MULTIFAMILY_PLAYABLE_LOOP_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_057_UNITY_ALPHA_MULTIFAMILY_PLAYABLE_LOOP.md
docs/agent-tasks/GOAL_057_UNITY_ALPHA_MULTIFAMILY_PLAYABLE_LOOP.md
docs/agent-tasks/GOAL_057_LAUNCHER.txt
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/UnityAlphaMultiFamilyPlayableLoop/**
tests/LLMGameCreator.Tests/Application/UnityAlphaMultiFamilyPlayableLoop/**
tests/LLMGameCreator.Tests/ProductSmoke/UnityAlphaMultiFamilyPlayableLoopProductSmokeTests.cs
.llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop/**
unity/LLMGameCreatorAlpha/Assets/Scripts/**
unity/LLMGameCreatorAlpha/Assets/StreamingAssets/**
```

Prefer modifying the existing Unity Alpha bootstrap/runtime file if that is the local pattern. Keep Unity changes narrow, explicit and diagnostic-marker based.

## Forbidden files / areas

Do not modify:

```text
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/**
src/LLMGameCreator.Scripting/**
generator-library/**
samples/**
templates/**
*.sln
*.csproj
```

Do not add external dependencies or Unity packages.

Do not call LLM/provider/RAG/network/media generation.

Do not generate or import real third-party media.

Do not make broad Unity architecture changes, scene redesigns, package upgrades or editor settings churn.

Do not change public GamePackage schema.

## Exact behavior

### 1. Preflight docs

- Record Goal 056 as accepted by user handoff.
- Set current Goal 057 state to produced-for-review / required gate.
- Update queue/context consistently.
- Keep Goal 031/032 status exactly as current docs model requires.

### 2. Application seam

Add a small Application-layer seam under:

```text
src/LLMGameCreator.Application/Design/UnityAlphaMultiFamilyPlayableLoop/
```

It should load/consume compact source evidence from Goals 043, 047, 055 and 056 where available and build:

- source manifest;
- family mode manifest;
- Unity staging manifest;
- family command plan;
- expected marker list;
- media binding validation;
- review package manifest;
- preview/export payload;
- invalid/fake/leak diagnostics;
- compact evidence writer.

### 3. Unity Alpha runtime extension

Narrowly extend the repo-local Unity Alpha runtime so that it can:

- load the staged Goal 056 media manifest from `Application.streamingAssetsPath`;
- select a family mode from deterministic command-line/diagnostic args or existing diagnostic route;
- emit deterministic markers for:
  - `family_mode_selected`;
  - `family_loop_started`;
  - `family_loop_step`;
  - `family_loop_completed`;
  - media manifest/hash validation;
  - PNG/WAV/bundle media proof;
  - review package proof.
- handle all three families:
  - map/panel RPG: traversal or focus/quest/item/event style loop;
  - survival sandbox: hazard/resource/collect/consume/craft/survival transition style loop;
  - first-person grid dungeon: orientation/corridor/encounter/progression pressure style loop.

The proof can remain simple and diagnostic, but it must be real Unity/player executed for GREEN.

### 4. Unity staging / review package

Stage a deterministic Unity `StreamingAssets` payload under the allowed artifact folder and/or `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/` if the existing Alpha build route requires it.

Keep heavy Unity build/log/cache outputs ignored.

Write compact evidence artifacts under:

```text
.llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop/
```

Required files:

```text
source-manifest.json
family-mode-manifest.json
unity-staging-manifest.json
family-command-plan.json
family-loop-proof-map-panel-rpg.json
family-loop-proof-survival-sandbox.json
family-loop-proof-first-person-grid-dungeon.json
player-log-summary.json
media-binding-validation.json
preview-export-payload.json
review-package-manifest.json
invalid-matrix.json
unity-alpha-multifamily-playable-loop-report.md
```

### 5. Product smoke

Add an exact product smoke route/class that proves:

- source facts are consumed;
- Unity staging exists;
- all three family loops are executed or verified through the existing automated Unity Alpha player route;
- player logs contain required family/media markers;
- report contains:
  - `implementationStatus=GREEN` for GREEN result;
  - `accepted=false`;
  - `manualGate=unity_alpha_multifamily_playable_loop_verification`;
  - all three family ids;
  - player/editor exit code evidence;
  - invalid matrix passed.

### 6. Invalid/fake/leak matrix

Cover at minimum:

- missing Goal 056 source;
- missing media manifest;
- stale/hash-mismatched media file;
- fake family id;
- duplicate family mode id;
- missing family command plan;
- missing player marker;
- fake player log;
- malformed PNG/WAV/bundle ref;
- unsafe relative path;
- provider/network/LLM/RAG claim;
- Lua execution claim;
- Runtime/GamePackage schema mutation claim;
- broad Unity mutation claim;
- nondeterministic ordering;
- missing review trace.

Diagnostics must be causal and stable.

## Validation commands

Run focused first, then final gate.

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~UnityAlphaMultiFamilyPlayableLoop|FullyQualifiedName~Goal057"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~UnityAlphaMultiFamilyPlayableLoopProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal057|FullyQualifiedName~UnityAlphaMultiFamily"

.\.devflow\scripts\check-all.ps1
```

Also run the existing artifact scope guard for Goal 057 after adding the policy entry. Do not invent a new guard pattern if the repository already has one.

Run the existing Unity Alpha build/player diagnostic route if available. If it is unavailable or fails for environmental reasons, commit/push `BLOCKED` with exact diagnostics.

## Bounded repairs pre-authorized

To reduce unnecessary user handoffs, the following bounded repairs are pre-authorized if needed:

1. Update stale current-state/handoff guard tests if they only fail because the current gate advanced to Goal 057, while preserving historical strict assertions.
2. Restore exact accidental historical `.llmgc/procedural/**` artifacts from `HEAD` if `check-all.ps1` mutates them outside Goal 057 scope.
3. Update `.devflow/artifact-scope/artifact-scope-policy.json` only for Goal 057 paths.
4. Fix narrow Unity Alpha diagnostic marker wording only inside allowed Unity Alpha script files.
5. Fix narrow product smoke filter/test naming issues under the allowed Goal 057 tests.

Do not use `git reset`, `git clean`, `git stash`, broad checkout, merge, rebase or cherry-pick.

## Git policy

You must commit and push final state to `origin/main` regardless of result.

If GREEN:

```text
GREEN Goal 057 unity alpha multifamily playable loop
```

If implemented but blocked:

```text
BLOCKED Goal 057 unity alpha multifamily playable loop
```

If failed/incomplete:

```text
FAILED Goal 057 unity alpha multifamily playable loop
```

Allowed git commands:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <explicit paths>
git diff --stat --cached
git diff --cached --name-only
git diff --cached --check
git add <explicit allowed paths>
git commit -m "<message>"
git push origin main
git rev-parse HEAD
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
Goal 057 выполнен / заблокирован / провален

Status:
GREEN / BLOCKED / FAILED

Gate:
unity_alpha_multifamily_playable_loop_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<paths>

Unity proof:
<editor/player route, exit codes, marker summary, logs/evidence paths>

Family loop proof:
map_panel_rpg: <markers>
survival_sandbox: <markers>
first_person_grid_dungeon: <markers>

Evidence artifacts:
<required files>

Проверки:
<commands and results>

Invalid/fake/leak matrix:
<summary>

Bounded repairs:
<none or exact details>

Git:
<commit hash and push result; state whether committed despite non-green>

Ограничения:
<confirm no forbidden changes>

Следующий разумный шаг:
<short recommendation>
```
