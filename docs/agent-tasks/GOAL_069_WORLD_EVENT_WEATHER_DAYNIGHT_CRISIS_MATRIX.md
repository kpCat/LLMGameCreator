# Codex Task — Goal 069 World Event / Weather / Day-Night / Crisis Matrix

## Metadata

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

Goal id/name:

```text
goal_069_world_event_weather_daynight_crisis_matrix
Goal 069 — World Event / Weather / Day-Night / Crisis Matrix
```

Codex reasoning:

```text
very high
```

Manual gate to produce for review:

```text
world_event_weather_daynight_crisis_matrix_verification required
```

## Required launcher behavior

Treat this file as the authoritative task contract. If another instruction conflicts with this file, stop and report unless the user explicitly overrides it.

This task must commit/push final state to `origin/main` whether the result is GREEN, BLOCKED, or FAILED.

## Preflight

1. Work in `C:\Users\endim\LLMGameCreator\`.
2. Confirm branch is `main`.
3. Inspect worktree state.
4. If untracked Goal 069 task/spec/scouting/launcher files are present under `docs/**`, treat them as part of this task.
5. Read the read-first files before implementation.
6. Record Goal 068 acceptance by user handoff before Goal 069:
   - `combat_magic_ability_boss_encounter_matrix_verification passed before Goal 069`
7. Do not mark Goal 069 passed.

## Read-first list

Read in this order:

1. `AGENTS.md`
2. `README.md` if present
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CURRENT_GENERATOR_STATE.json`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/ROADMAP_TO_FULL_GENERATOR.md` if present
8. `docs/GOAL_069_WORLD_EVENT_WEATHER_DAYNIGHT_CRISIS_MATRIX_SPEC.md`
9. `docs/EXTERNAL_SCOUTING_GOAL_069_WORLD_EVENT_WEATHER_DAYNIGHT_CRISIS_MATRIX.md`
10. Existing Goal 060–068 compact artifacts under `.llmgc/procedural/**`, especially:
    - `goal-060-full-campaign-gamepackage-materialization-matrix`
    - `goal-061-full-campaign-playable-review-package-rc`
    - `goal-062-constrained-spatial-detail-generation`
    - `goal-063-gameplay-consequence-depth-matrix`
    - `goal-064-living-world-npc-faction-simulation-matrix`
    - `goal-065-interlocked-gameplay-systems-depth-matrix`
    - `goal-066-settlement-construction-destruction-production-matrix`
    - `goal-067-programmatic-narrative-quest-dialogue-event-matrix`
    - `goal-068-combat-magic-ability-boss-encounter-matrix`
11. Local implementation patterns:
    - `src/LLMGameCreator.Application/Design/GameplayConsequenceDepthMatrix/**`
    - `src/LLMGameCreator.Application/Design/LivingWorldNpcFactionSimulationMatrix/**`
    - `src/LLMGameCreator.Application/Design/InterlockedGameplaySystemsDepthMatrix/**`
    - `src/LLMGameCreator.Application/Design/SettlementConstructionDestructionProductionMatrix/**`
    - `src/LLMGameCreator.Application/Design/ProgrammaticNarrativeQuestDialogueEventMatrix/**`
    - `src/LLMGameCreator.Application/Design/CombatMagicAbilityBossEncounterMatrix/**`
12. Existing product smoke tests for Goals 063–068.
13. `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs` only for the narrow marker loader pattern.

Do not read the entire repository unless required by a specific failing reference.

## Allowed files / areas

You may create/edit only:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/GOAL_069_WORLD_EVENT_WEATHER_DAYNIGHT_CRISIS_MATRIX_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_069_WORLD_EVENT_WEATHER_DAYNIGHT_CRISIS_MATRIX.md
docs/agent-tasks/GOAL_069_WORLD_EVENT_WEATHER_DAYNIGHT_CRISIS_MATRIX.md
docs/agent-tasks/GOAL_069_LAUNCHER.txt
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/WorldEventWeatherDayNightCrisisMatrix/**
tests/LLMGameCreator.Tests/Application/WorldEventWeatherDayNightCrisisMatrix/**
tests/LLMGameCreator.Tests/ProductSmoke/WorldEventWeatherDayNightCrisisMatrixProductSmokeTests.cs
.llmgc/procedural/goal-069-world-event-weather-daynight-crisis-matrix/**
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

Unity change must be narrow: read a staged Goal 069 command plan and emit deterministic markers. Do not implement broad Unity gameplay/weather rendering.

## Forbidden files / areas

Do not change:

```text
public GamePackage schema/model definitions
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/** provider/LLM/RAG paths
src/LLMGameCreator.Scripting/**
generator-library/**
samples/**
templates/**
unity/** except AlphaRuntimeBootstrap.cs narrow marker loader
*.sln
*.csproj
external dependencies / NuGet references
```

No real weather API, no provider call, no LLM call, no arbitrary Lua, no generated Lua source.

## Exact behavior

Implement an Application-layer BCL-only seam:

```text
src/LLMGameCreator.Application/Design/WorldEventWeatherDayNightCrisisMatrix/
```

Suggested components, adapt names to local style:

- `WorldEventWeatherDayNightCrisisModels`
- `WorldEventWeatherDayNightCrisisSourceLoader`
- `WorldEventWeatherDayNightCrisisCatalog`
- `WorldEventWeatherDayNightCrisisProjector`
- `WorldEventWeatherDayNightCrisisValidator`
- `WorldEventWeatherDayNightCrisisEvidenceService`
- `WorldEventWeatherDayNightCrisisHash`
- `WorldEventWeatherDayNightCrisisUnityProofRunner`

The seam must consume existing compact evidence from Goals 060–068 and produce 9 deterministic rows.

### Row requirements

Each row must include:

- family id
- seed id
- upstream refs/hashes
- before/after world clock
- day/night phase effect
- weather or environmental condition
- hazard/crisis event
- cross-system deltas touching at least two of:
  - NPC/faction
  - settlement/production
  - combat/magic/status
  - narrative/quest/dialogue
  - economy/resource/inventory
- save/load snapshot proof
- replay proof
- deterministic row hash
- Unity marker expectations

### Required row count

```text
3 families x 3 seeds = 9 rows
```

### Family row behavior

`map_panel_rpg` rows must include travel/route or faction/NPC/event pressure.

`survival_sandbox` rows must include hazard/need/resource/shelter/craft/consume/recover pressure.

`first_person_grid_dungeon` rows must include dungeon darkness/fog/torch/magic light/encounter/loot/progression pressure.

### Unity Alpha proof

Add only a narrow marker-loader extension to `AlphaRuntimeBootstrap.cs`.

It should read a staged Goal 069 command plan and emit markers such as:

```text
world_event_matrix_loaded=true
world_event_row=<row id>
world_event_family=<family id>
world_event_seed=<seed id>
world_event_clock_phase=<phase>
world_event_weather=<weather id>
world_event_crisis=<crisis id>
world_event_state_changed=true
world_event_save_load_replay=true
world_event_row_completed=<row id>
world_event_matrix_completed=true
```

Use the existing Goal 063–068 marker proof style. Do not render weather, particles, sky, or audio.

## Invalid/fake/leak matrix

Must cover at least:

- missing Goal 068 source
- fake family
- fake seed
- duplicate row id
- non-state-changing row
- no day/night effect
- no weather/hazard effect
- crisis with no consequence
- missing cross-system delta
- save/load mismatch
- replay mismatch
- nondeterministic ordering
- unsafe path
- provider/LLM/RAG claim
- real weather/network claim
- Runtime/UI/GamePackage mutation claim
- broad Unity gameplay/weather rendering claim
- arbitrary Lua/generated Lua claim

Each invalid case must have a stable diagnostic code and matched expectation.

## Evidence artifacts

Write deterministic compact artifacts under:

```text
.llmgc/procedural/goal-069-world-event-weather-daynight-crisis-matrix/
```

Required artifacts:

- `source-manifest.json`
- `world-clock-calendar-policy.json`
- `weather-hazard-catalog.json`
- `crisis-event-catalog.json`
- `world-event-weather-daynight-row-matrix.json`
- `save-load-replay-proof.json`
- `variance-metrics.json`
- `unity-command-plan.json`
- `unity-proof-summary.json`
- `invalid-diagnostics-matrix.json`
- `preview-export-payload.json`
- `world-event-weather-daynight-crisis-matrix-report.md`
- artifact scope output if produced by the existing guard

Evidence must not contain absolute local paths, nondeterministic timestamps, heavy build logs, or generated Unity build outputs.

## Tests

Add focused tests, adapting local naming style:

```text
tests/LLMGameCreator.Tests/Application/WorldEventWeatherDayNightCrisisMatrix/WorldEventWeatherDayNightCrisisMatrixTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/WorldEventWeatherDayNightCrisisMatrixProductSmokeTests.cs
```

Focused tests must prove:

- 9/9 rows exist.
- 9/9 rows state-changing.
- day/night effects exist and are not labels only.
- weather/hazard/crisis effects exist and cause deltas.
- all three families have distinct behavior.
- save/load/replay proof passes.
- meaningful variance passes.
- invalid/fake/leak matrix passes.
- Unity command plan is complete.
- report contains `world_event_weather_daynight_crisis_matrix_verification required` and `accepted=false`.

## Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~WorldEventWeatherDayNight|FullyQualifiedName~Goal069"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~WorldEventWeatherDayNightCrisisMatrixProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal069|FullyQualifiedName~WorldEventWeather"

.\.devflow\scripts\check-all.ps1

.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-069-world-event-weather-daynight-crisis-matrix"
```

If local filters need exact class names, adapt narrowly and report.

Also run a mojibake marker scan over changed text files/artifacts using the existing local pattern.

## Bounded repairs

Pre-authorized bounded repairs:

1. Update stale current-state/handoff guard tests only if they hardcode the previous latest gate and block check-all.
2. Restore exact accidental historical `.llmgc/procedural/**` artifacts from HEAD if check-all mutates unrelated tracked artifacts.
3. Update `.devflow/artifact-scope/artifact-scope-policy.json` for Goal 069.
4. Narrowly adapt `AlphaRuntimeBootstrap.cs` marker loader only for Goal 069.

If repairs touch files outside the allowed list, commit/push as BLOCKED and report exact reason unless the user explicitly authorizes more.

## Git policy

At the end, commit and push final state to `origin/main` regardless of result.

Allowed git commands:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <explicit changed docs/policy/source/test/unity paths>
git add -- <explicit allowed paths>
git diff --cached --name-status
git diff --cached --stat
git diff --cached --check
git commit -m "<message>"
git rev-parse HEAD
git push origin main
```

Forbidden:

```text
git checkout
git switch
git merge
git rebase
git cherry-pick
git reset
git stash
git clean
git push --force
```

Commit message:

- GREEN: `GREEN Goal 069 world event weather daynight crisis matrix`
- BLOCKED: `BLOCKED Goal 069 world event weather daynight crisis matrix`
- FAILED: `FAILED Goal 069 world event weather daynight crisis matrix`

## Final report

Report in Russian:

```text
Goal 069 выполнен / заблокирован / упал

Status:
GREEN / BLOCKED / FAILED

Gate:
world_event_weather_daynight_crisis_matrix_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые области:
<список>

Доказательства:
rowCount, stateChangingRowCount, families, seeds, save/load/replay, variance, Unity proof

Проверки:
<commands/results>

Invalid/fake/leak matrix:
<summary>

Git:
<commit hash and push result>

Ограничения:
<what was not touched>

Следующий разумный шаг:
<one paragraph>
```
