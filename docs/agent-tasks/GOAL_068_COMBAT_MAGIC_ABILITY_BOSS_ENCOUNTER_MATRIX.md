# Codex task — Goal 068 Combat/Magic/Ability/Boss Encounter Matrix

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
goal_068_combat_magic_ability_boss_encounter_matrix
Goal 068: Combat/Magic/Ability/Boss Encounter Matrix
```

Required gate marker:

```text
combat_magic_ability_boss_encounter_matrix_verification required
```

Codex reasoning level:

```text
very high
```

## Operating rule

This is an aggressive composite goal. Work for a real implementation, not a paper-only artifact.

Commit and push the final state to `origin/main` even if the result is `GREEN`, `BLOCKED`, or `FAILED`.

Use honest commit messages:

- `GREEN Goal 068 combat magic ability boss encounter matrix`
- `BLOCKED Goal 068 combat magic ability boss encounter matrix`
- `FAILED Goal 068 combat magic ability boss encounter matrix`

Never mark the manual gate passed inside this goal.

## Required preflight

1. Confirm current branch is `main`.
2. Read the read-first list.
3. Confirm the working tree state.
4. Treat untracked Goal 068 task/spec/scouting/launcher files as part of this task if present.
5. Record Goal 067 user-handoff acceptance in the docs quartet before implementation:

```text
programmatic_narrative_quest_dialogue_event_matrix_verification passed before Goal 068
```

6. Start Goal 068 as produced-for-review:

```text
combat_magic_ability_boss_encounter_matrix_verification required
```

Goal 068 must remain:

```text
accepted=false
```

## Read-first list

Read, in this order:

1. `AGENTS.md`
2. `README.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CURRENT_GENERATOR_STATE.json`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/GOAL_068_COMBAT_MAGIC_ABILITY_BOSS_ENCOUNTER_MATRIX_SPEC.md`
8. `docs/EXTERNAL_SCOUTING_GOAL_068_COMBAT_MAGIC_ABILITY_BOSS_ENCOUNTER_MATRIX.md`
9. `docs/agent-tasks/GOAL_068_COMBAT_MAGIC_ABILITY_BOSS_ENCOUNTER_MATRIX.md`
10. `docs/agent-tasks/GOAL_068_LAUNCHER.txt`
11. `.devflow/artifact-scope/artifact-scope-policy.json`
12. Existing Goal 060–067 artifacts under `.llmgc/procedural/`
13. Existing analogous source/tests:
    - `src/LLMGameCreator.Application/Design/GameplayConsequenceDepthMatrix/**`
    - `src/LLMGameCreator.Application/Design/LivingWorldNpcFactionSimulationMatrix/**`
    - `src/LLMGameCreator.Application/Design/InterlockedGameplaySystemsDepthMatrix/**`
    - `src/LLMGameCreator.Application/Design/SettlementConstructionDestructionProductionMatrix/**`
    - `src/LLMGameCreator.Application/Design/ProgrammaticNarrativeQuestDialogueEventMatrix/**`
    - corresponding tests/product smokes.
14. `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`

## Allowed files / areas

You may create or edit only:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/GOAL_068_COMBAT_MAGIC_ABILITY_BOSS_ENCOUNTER_MATRIX_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_068_COMBAT_MAGIC_ABILITY_BOSS_ENCOUNTER_MATRIX.md
docs/agent-tasks/GOAL_068_COMBAT_MAGIC_ABILITY_BOSS_ENCOUNTER_MATRIX.md
docs/agent-tasks/GOAL_068_LAUNCHER.txt
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/CombatMagicAbilityBossEncounterMatrix/**
tests/LLMGameCreator.Tests/Application/CombatMagicAbilityBossEncounterMatrix/**
tests/LLMGameCreator.Tests/ProductSmoke/CombatMagicAbilityBossEncounterMatrixProductSmokeTests.cs
.llmgc/procedural/goal-068-combat-magic-ability-boss-encounter-matrix/**
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

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
unity/** except unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
*.sln
*.csproj
```

Also forbidden:

- external NuGet dependencies;
- public GamePackage schema changes;
- broad Unity gameplay implementation;
- Unity assets/scenes/prefabs/settings/package changes;
- LLM/provider/RAG calls;
- final prose generation;
- arbitrary Lua execution;
- generated Lua source;
- weakening existing tests;
- deleting historical evidence.

## Exact behavior

Implement a BCL-only Application-layer seam:

```text
src/LLMGameCreator.Application/Design/CombatMagicAbilityBossEncounterMatrix/
```

Suggested files, adjust if local style strongly suggests otherwise:

```text
CombatMagicAbilityBossEncounterModels.cs
CombatMagicAbilityBossEncounterSourceLoader.cs
CombatMagicAbilityBossEncounterCatalog.cs
CombatMagicAbilityBossEncounterProjector.cs
CombatMagicAbilityBossEncounterValidator.cs
CombatMagicAbilityBossEncounterEvidenceService.cs
CombatMagicAbilityBossEncounterUnityProofRunner.cs
CombatMagicAbilityBossEncounterHash.cs
```

The seam must consume Goal 060–067 compact evidence and produce 9 combat/magic/ability/boss rows.

### Required concepts

Represent at least:

- combatant snapshot;
- attribute/resource snapshot;
- active ability;
- passive trait;
- status effect;
- damage/effect packet;
- cooldown/cost;
- resistance/weakness;
- boss/elite phase;
- round/phase result;
- counterplay record;
- loot/progression record;
- non-combat consequence record;
- save/load/replay record;
- Unity marker command record.

### Family-specific rows

Create 9 rows:

- `map_panel_rpg` × `seed_alpha|seed_beta|seed_gamma`
- `survival_sandbox` × `seed_alpha|seed_beta|seed_gamma`
- `first_person_grid_dungeon` × `seed_alpha|seed_beta|seed_gamma`

Each row must have real before/after state changes.

### State-changing requirements

Each row must change at least three different categories, for example:

- health/armor/energy/mana/stamina;
- cooldown or resource cost;
- status/effect stack;
- loot/progression;
- faction/narrative/living-world/settlement/survival state.

Across the matrix:

- 9/9 rows state-changing;
- save/load/replay passed for 9/9;
- same-family seed variance true;
- family combat flavor variance true;
- at least 3 boss/elite phase rows;
- at least 3 magic/status-heavy rows;
- at least 3 resource/gear/crafting-linked rows.

### Unity Alpha proof

Update `AlphaRuntimeBootstrap.cs` narrowly to read a staged command plan and print deterministic markers. Follow existing Goal 063–067 marker-loader patterns. Do not add scenes/assets/settings.

Required markers include:

```text
combat_magic_row_loaded
combat_magic_round_step
combat_magic_ability_resolved
combat_magic_status_delta
combat_magic_progression_delta
combat_magic_row_completed
combat_magic_matrix_completed
review_package_proof=goal068
```

The product smoke must prove Unity/player markers if the existing Unity proof route is available. If it is not available for environmental reasons, commit as `BLOCKED` with exact reason.

## Required artifacts

Write under:

```text
.llmgc/procedural/goal-068-combat-magic-ability-boss-encounter-matrix/
```

Required files:

```text
combat-magic-source-manifest.json
ability-trait-catalog.json
status-effect-catalog.json
boss-encounter-phase-catalog.json
combat-magic-row-matrix.json
combat-magic-save-load-replay-proof.json
combat-magic-progression-loot-ledger.json
combat-magic-counterplay-ledger.json
combat-magic-preview-export-payload.json
combat-magic-unity-command-plan.json
combat-magic-unity-player-proof-summary.json
combat-magic-invalid-diagnostics-matrix.json
artifact-scope-report.json
combat-magic-ability-boss-encounter-matrix-report.md
rows/map-panel-rpg-seed-alpha-combat-magic-row.json
rows/map-panel-rpg-seed-beta-combat-magic-row.json
rows/map-panel-rpg-seed-gamma-combat-magic-row.json
rows/survival-sandbox-seed-alpha-combat-magic-row.json
rows/survival-sandbox-seed-beta-combat-magic-row.json
rows/survival-sandbox-seed-gamma-combat-magic-row.json
rows/first-person-grid-dungeon-seed-alpha-combat-magic-row.json
rows/first-person-grid-dungeon-seed-beta-combat-magic-row.json
rows/first-person-grid-dungeon-seed-gamma-combat-magic-row.json
```

No timestamps, no absolute paths, stable ordering.

## Tests

Add focused tests under:

```text
tests/LLMGameCreator.Tests/Application/CombatMagicAbilityBossEncounterMatrix/
```

Suggested coverage:

- source loading and prior-goal chain;
- ability/status/boss catalog validation;
- row matrix has 9 rows and 3 families × 3 seeds;
- every row state-changing;
- save/load/replay deterministic;
- boss/elite rows present;
- magic/status rows present;
- resource/gear/crafting-linked rows present;
- invalid/fake/leak matrix;
- evidence artifact shape and hash determinism.

Add product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/CombatMagicAbilityBossEncounterMatrixProductSmokeTests.cs
```

## Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~CombatMagicAbilityBoss|FullyQualifiedName~Goal068"
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~CombatMagicAbilityBossEncounterMatrixProductSmokeTests"
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal068|FullyQualifiedName~CombatMagic"
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-068-combat-magic-ability-boss-encounter-matrix"
```

Also run a mojibake scan over changed/new text files and Goal 068 artifacts.

## Pre-authorized bounded repairs

Allowed if needed:

1. Update stale current-state/handoff guard tests only if they hardcode the previous latest gate and the change is necessary for Goal 068 state consistency.
2. Restore exact check-all-mutated historical `.llmgc/procedural/**` artifacts from HEAD using:

```powershell
git restore --source=HEAD -- <exact accidental historical artifact paths>
```

Do not restore Goal 068 code/docs/evidence.
3. If Unity logs are required and a prior real generated cache contains exact required logs, copy only those exact logs and report source/target. Do not fabricate logs.

## Stop / BLOCKED conditions

Commit/push `BLOCKED` if:

- Unity proof route is unavailable and cannot be reasonably rerun;
- prior Goal 060–067 source evidence is missing;
- any row cannot prove state changes without fake data;
- public GamePackage schema needs modification;
- Runtime/Runtime.Abstractions source must be changed;
- external dependencies become necessary;
- only narrative/descriptive rows can be produced;
- check-all fails and cannot be fixed inside bounded scope.

Commit/push `FAILED` only if implementation is not coherent enough for review.

## Git policy

Always commit and push final state to `origin/main`.

Allowed git commands:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <explicit files>
git add -- <explicit allowed paths>
git diff --cached --name-status
git diff --cached --stat
git diff --cached --check
git commit -m "<honest message>"
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

## Final report format

Report in Russian:

```text
Goal 068 выполнен / заблокирован / провален

Status:
GREEN / BLOCKED / FAILED

Gate:
combat_magic_ability_boss_encounter_matrix_verification required

Commit:
<hash and push result>

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<list>

Proof:
- rows
- state-changing count
- boss/elite rows
- magic/status rows
- resource/gear/crafting rows
- save/load/replay
- Unity/player proof
- invalid matrix

Проверки:
<commands and results>

Ограничения:
<forbidden areas not touched>

Следующий разумный шаг:
<one paragraph>
```
