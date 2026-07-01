# Codex task — Goal 064 Living World NPC/Faction Simulation Matrix

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
goal_064_living_world_npc_faction_simulation_matrix
Goal 064: Living World NPC/Faction Simulation Matrix
```

Required goal marker / gate:

```text
living_world_npc_faction_simulation_matrix_verification required
```

Codex reasoning level:

```text
very high
```

## Global process policy

This is an aggressive composite goal. Do not stop for a separate user acceptance prompt. First record Goal 063 acceptance by user handoff, then implement Goal 064.

Final commit/push is mandatory even if status is GREEN, BLOCKED or FAILED. The commit message must honestly reflect the result:

```text
GREEN Goal 064 living world npc faction simulation matrix
BLOCKED Goal 064 living world npc faction simulation matrix
FAILED Goal 064 living world npc faction simulation matrix
```

Do not mark the Goal 064 gate passed. Leave it as produced-for-review:

```text
living_world_npc_faction_simulation_matrix_verification required
accepted=false
```

## Read-first list

Read first, in order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_064_LIVING_WORLD_NPC_FACTION_SIMULATION_MATRIX_SPEC.md` if present, otherwise this task file is authoritative.
7. `docs/EXTERNAL_SCOUTING_GOAL_064_LIVING_WORLD_NPC_FACTION_SIMULATION_MATRIX.md` if present.
8. Goal 060 artifacts under `.llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix/`
9. Goal 061 artifacts under `.llmgc/procedural/goal-061-full-campaign-playable-review-package-rc/`
10. Goal 062 artifacts under `.llmgc/procedural/goal-062-constrained-spatial-detail-generation/`
11. Goal 063 artifacts under `.llmgc/procedural/goal-063-gameplay-consequence-depth-matrix/`
12. Existing local analogs:
    - `src/LLMGameCreator.Application/Design/GameplayConsequenceDepthMatrix/**`
    - `src/LLMGameCreator.Application/Design/ConstrainedSpatialDetailGeneration/**`
    - `src/LLMGameCreator.Application/Design/FullCampaignPlayableReviewPackageRc/**`
    - recent Unity proof runner classes
    - recent product smoke tests.
13. `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
14. `.devflow/artifact-scope/artifact-scope-policy.json`

Avoid broad repository reads. Search narrowly for local patterns.

## Allowed files / areas

You may create/edit only:

```text
docs/GOAL_064_LIVING_WORLD_NPC_FACTION_SIMULATION_MATRIX_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_064_LIVING_WORLD_NPC_FACTION_SIMULATION_MATRIX.md
docs/agent-tasks/GOAL_064_LIVING_WORLD_NPC_FACTION_SIMULATION_MATRIX.md
docs/agent-tasks/GOAL_064_LAUNCHER.txt
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/LivingWorldNpcFactionSimulationMatrix/**
tests/LLMGameCreator.Tests/Application/LivingWorldNpcFactionSimulationMatrix/**
tests/LLMGameCreator.Tests/ProductSmoke/LivingWorldNpcFactionSimulationMatrixProductSmokeTests.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
.llmgc/procedural/goal-064-living-world-npc-faction-simulation-matrix/**
```

If local naming convention strongly prefers a shorter folder name, use it consistently and document it in the final report.

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

Do not add external dependencies. Do not use network calls. Do not run LLM/provider/RAG. Do not generate/import media. Do not execute arbitrary Lua.

## Required behavior

### 1. Handoff acceptance preflight

Record in the docs quartet that Goal 063 is accepted by user handoff before Goal 064:

```text
gameplay_consequence_depth_matrix_verification passed before Goal 064
```

Do not mark Goal 064 passed.

### 2. BCL-only living-world simulation seam

Add an Application-layer seam, expected folder:

```text
src/LLMGameCreator.Application/Design/LivingWorldNpcFactionSimulationMatrix/
```

Suggested small components:

- `LivingWorldNpcFactionSimulationModels.cs`
- `LivingWorldNpcFactionSimulationSourceLoader.cs`
- `LivingWorldNpcFactionSimulationBuilder.cs`
- `LivingWorldNpcFactionSimulationValidator.cs`
- `LivingWorldNpcFactionSimulationEvidenceService.cs`
- `LivingWorldNpcFactionSimulationHash.cs`
- `LivingWorldNpcFactionUnityProofRunner.cs`

Avoid one huge class.

### 3. Consume Goal 060/061/062/063 proof chain

The source loader must read/validate enough compact evidence to prove the chain exists:

- Goal 060 materialized packages;
- Goal 061 review package rows;
- Goal 062 spatial detail rows;
- Goal 063 gameplay consequence rows.

Do not invent source facts if evidence is missing. Missing/fake source evidence must be a causal diagnostic.

### 4. Generate 9 living-world simulation rows

For each family/seed row, produce:

- row id;
- family id;
- seed id;
- source package row id;
- actor records;
- faction/group records;
- relationship/reputation records;
- schedule/availability records;
- world event records;
- memory/rumor/consequence trace records;
- ordered tick plan;
- before state;
- after state;
- state delta summary;
- save/load/replay proof;
- row hash.

Minimum families:

```text
map_panel_rpg
survival_sandbox
first_person_grid_dungeon
```

Minimum seeds:

```text
seed_alpha
seed_beta
seed_gamma
```

### 5. Family-specific depth requirements

#### map_panel_rpg

Prove at least:

- one NPC availability/route/status change;
- one faction/social relation or reputation change;
- one quest/event memory or rumor pressure record;
- one inventory/reward/event consequence link from Goal 063.

#### survival_sandbox

Prove at least:

- camp/group/NPC support or availability change;
- scarcity/resource/hazard pressure consequence;
- group trust/reputation/resource state change;
- one survival event memory such as weather, hunger, shelter, danger or recovery.

#### first_person_grid_dungeon

Prove at least:

- dungeon actor/encounter pressure or alertness change;
- monster-group/faction aggression or relation change;
- one loot/progression consequence;
- one spatial relation to Goal 062 detail row such as room/corridor/blocked/valid movement context.

### 6. Replay, save/load and variance

For all 9 rows:

- before and after state hashes must differ;
- save/load roundtrip must preserve after-state hash;
- replay from the same input must reproduce tick/state hashes;
- variance metrics must prove differences are not only ID/hash noise;
- same-family seed variation must produce at least one meaningful difference per seed;
- cross-family variation must prove different rule families.

### 7. Unity Alpha proof

A narrow extension to `AlphaRuntimeBootstrap.cs` is allowed to load a staged Goal 064 command/marker plan and emit deterministic living-world markers.

Do not create broad Unity gameplay systems. Do not add packages. Do not track heavy Unity build/log/cache output.

### 8. Evidence artifacts

Write compact deterministic evidence under:

```text
.llmgc/procedural/goal-064-living-world-npc-faction-simulation-matrix/
```

Required evidence intent:

- source manifest;
- actor/faction catalog summary;
- simulation matrix plan;
- 9 row traces or row files;
- save-load-replay proof;
- variance metrics;
- Unity command plan;
- Unity/player proof summary;
- preview/export payload;
- invalid diagnostics matrix;
- artifact scope reports if standard;
- compact markdown report.

Evidence must be deterministic, no timestamps unless project convention requires them, no absolute paths, no heavy logs.

### 9. Invalid/fake/leak matrix

Cover at minimum:

- missing Goal063 source;
- missing Goal062 spatial detail source;
- fake family id;
- fake seed id;
- duplicate actor id;
- duplicate faction id;
- invalid relation target;
- impossible schedule/availability state;
- non-state-changing row;
- save/load mismatch;
- replay mismatch;
- hash-only variance;
- missing Unity marker;
- unsafe path;
- provider/LLM/RAG claim;
- Runtime/UI/GamePackage schema mutation claim;
- Unity broad mutation claim;
- media generation/import claim;
- arbitrary Lua execution claim;
- nondeterministic ordering.

### 10. State docs and queue

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
```

State should say:

- Goal 063 accepted by user handoff before Goal 064;
- Goal 064 produced for review;
- gate `living_world_npc_faction_simulation_matrix_verification required`;
- Goal 064 accepted=false;
- recommended next work should move toward deeper gameplay systems, settlements/construction, or authoring UI, but do not start it.

## Bounded repairs pre-authorized

If stale current-state/handoff guard tests hardcode the previous latest gate, you may update only the specific stale tests to use current-state consistency while preserving strict historical assertions.

If check-all mutates historical tracked `.llmgc/procedural/**` artifacts outside Goal 064, you may restore exact accidental historical artifact paths from HEAD using:

```powershell
git restore --source=HEAD -- <exact accidental historical artifact paths>
```

Do not restore Goal 064 code/docs/evidence. Do not use reset/clean/stash.

## Validation commands

Run focused first, then full gate:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~LivingWorldNpcFaction|FullyQualifiedName~Goal064"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~LivingWorldNpcFactionSimulationMatrixProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal064|FullyQualifiedName~LivingWorld"

.\.devflow\scripts\check-all.ps1
```

Run the existing artifact scope guard for Goal 064 if available or after adding the policy entry. Do not invent a new guard architecture.

## Git policy

At the end, commit and push final state to `origin/main` regardless of status.

Allowed git operations:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <explicit files>
git diff --cached --name-status
git diff --cached --stat
git diff --cached --check
git add -- <explicit allowed paths>
git commit -m "GREEN Goal 064 living world npc faction simulation matrix"
git commit -m "BLOCKED Goal 064 living world npc faction simulation matrix"
git commit -m "FAILED Goal 064 living world npc faction simulation matrix"
git rev-parse HEAD
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
Status: GREEN / BLOCKED / FAILED
Gate: living_world_npc_faction_simulation_matrix_verification required
Commit: <hash>
Push: <result>

Что стало реальнее:
...

Изменённые области:
...

Living-world proof:
- rows: 9/9
- state-changing rows: ...
- save/load/replay: ...
- Unity/player proof: ...

Проверки:
...

Invalid/fake/leak matrix:
...

Ограничения:
...

Следующий разумный шаг:
...
```
