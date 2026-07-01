# Codex task — GOAL 066 Settlement Construction Destruction Production Matrix

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
goal-066-settlement-construction-destruction-production-matrix
Goal 066: Settlement Construction Destruction Production Matrix
```

Codex reasoning level:

```text
very high
```

Expected manual gate:

```text
settlement_construction_destruction_production_matrix_verification required
```

## Git policy — always commit/push final state

You must commit and push the final state to `origin/main` regardless of GREEN/BLOCKED/FAILED result.

Commit messages:

- GREEN: `GREEN Goal 066 settlement construction destruction production matrix`
- BLOCKED: `BLOCKED Goal 066 settlement construction destruction production matrix`
- FAILED: `FAILED Goal 066 settlement construction destruction production matrix`

Do not pretend non-green work is accepted. The final report must clearly state GREEN/BLOCKED/FAILED and why.

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
git commit -m "<message>"
git rev-parse HEAD
git push origin main
```

Forbidden git commands:

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

## Read-first list

Read first:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_066_SETTLEMENT_CONSTRUCTION_DESTRUCTION_PRODUCTION_MATRIX_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_066_SETTLEMENT_CONSTRUCTION_DESTRUCTION_PRODUCTION_MATRIX.md`
8. Goal 060 artifacts under `.llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix/`
9. Goal 061 artifacts under `.llmgc/procedural/goal-061-full-campaign-playable-review-package-rc/`
10. Goal 062 artifacts under `.llmgc/procedural/goal-062-constrained-spatial-detail-generation/`
11. Goal 063 artifacts under `.llmgc/procedural/goal-063-gameplay-consequence-depth-matrix/`
12. Goal 064 artifacts under `.llmgc/procedural/goal-064-living-world-npc-faction-simulation-matrix/`
13. Goal 065 artifacts under `.llmgc/procedural/goal-065-interlocked-gameplay-systems-depth-matrix/`
14. Existing local source/test analogs:
    - `src/LLMGameCreator.Application/Design/GameplayConsequenceDepthMatrix/**`
    - `src/LLMGameCreator.Application/Design/LivingWorldNpcFactionSimulationMatrix/**`
    - `src/LLMGameCreator.Application/Design/InterlockedGameplaySystemsDepthMatrix/**`
    - matching tests and product smoke classes.

Do not read the whole repository unless a local search proves the relevant files live elsewhere.

## Allowed files / areas

You may create/edit:

```text
docs/GOAL_066_SETTLEMENT_CONSTRUCTION_DESTRUCTION_PRODUCTION_MATRIX_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_066_SETTLEMENT_CONSTRUCTION_DESTRUCTION_PRODUCTION_MATRIX.md
docs/agent-tasks/GOAL_066_SETTLEMENT_CONSTRUCTION_DESTRUCTION_PRODUCTION_MATRIX.md
docs/agent-tasks/GOAL_066_LAUNCHER.txt
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/SettlementConstructionDestructionProductionMatrix/**
tests/LLMGameCreator.Tests/Application/SettlementConstructionDestructionProductionMatrix/**
tests/LLMGameCreator.Tests/ProductSmoke/SettlementConstructionDestructionProductionMatrixProductSmokeTests.cs
.llmgc/procedural/goal-066-settlement-construction-destruction-production-matrix/**
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

Unity change must be narrow marker-plan loading/proof only.

## Forbidden files / areas

Do not change:

```text
src/LLMGameCreator.GamePackage/** public schema/model shape
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
Unity project settings/scenes/prefabs except the single allowed AlphaRuntimeBootstrap.cs file
```

Also forbidden:

- external dependencies;
- network/provider/LLM/RAG calls;
- media generation;
- arbitrary Lua execution;
- broad Unity UI/building/physics/destruction implementation;
- public GamePackage schema changes.

## Exact behavior

### 1. Preflight

- Confirm branch is `main`.
- Confirm worktree status.
- Record Goal 065 handoff acceptance in docs/state:

```text
interlocked_gameplay_systems_depth_matrix_verification passed before Goal 066
```

Do not mark Goal 066 passed.

### 2. Implement Application seam

Create a BCL-only Application seam under:

```text
src/LLMGameCreator.Application/Design/SettlementConstructionDestructionProductionMatrix/
```

Suggested components, adapt names to repository conventions:

- `SettlementConstructionDestructionProductionModels.cs`
- `SettlementConstructionDestructionProductionSourceLoader.cs`
- `SettlementConstructionDestructionProductionBuilder.cs`
- `SettlementConstructionDestructionProductionValidator.cs`
- `SettlementConstructionDestructionProductionEvidenceService.cs`
- `SettlementConstructionDestructionProductionHash.cs`
- `SettlementConstructionUnityProofRunner.cs`

The seam must consume Goal 060/061/062/063/064/065 evidence and produce 9 family/seed rows.

### 3. Row requirements

For every family/seed row, produce deterministic records for:

- settlement id/name/family/seed;
- site/spatial detail reference;
- building slot/footprint;
- construction action;
- construction cost/resource ledger;
- production/service output ledger;
- damage/destruction/threat event;
- repair/upgrade/defense response;
- NPC/faction/living-world consequence;
- interlocked gameplay dependency;
- before/after runtime-like state hashes;
- save/load/replay proof;
- Unity marker plan.

Rows must be meaningfully different by family and seed.

### 4. Unity proof

Extend `AlphaRuntimeBootstrap.cs` narrowly to load a Goal 066 staged command/marker plan and emit deterministic markers.

No real building UI/physics/destructible terrain is required. This is a proof route.

### 5. Evidence artifacts

Write deterministic compact artifacts under:

```text
.llmgc/procedural/goal-066-settlement-construction-destruction-production-matrix/
```

Required files from the spec must be present. Add artifact-scope policy entry.

### 6. Docs update

Update the docs quartet:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

State must show:

- Goal 065 accepted by user handoff before Goal 066;
- Goal 066 produced for review;
- `settlement_construction_destruction_production_matrix_verification required`;
- accepted=false;
- next work recommended but not started.

## Tests

Add focused tests under:

```text
tests/LLMGameCreator.Tests/Application/SettlementConstructionDestructionProductionMatrix/
```

Add exact product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/SettlementConstructionDestructionProductionMatrixProductSmokeTests.cs
```

Tests must prove:

- source loading;
- 9-row matrix creation;
- construction/production/destruction/repair/defense ledgers;
- living-world and interlocked-gameplay linkage;
- save/load/replay;
- meaningful variance;
- invalid/fake/leak matrix;
- evidence files;
- Unity proof markers.

## Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~SettlementConstructionDestruction|FullyQualifiedName~Goal066"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~SettlementConstructionDestructionProductionMatrixProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal066|FullyQualifiedName~SettlementConstruction"

.\.devflow\scripts\check-all.ps1

# If existing scope guard supports scenarios:
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-066-settlement-construction-destruction-production-matrix"
```

Also run a mojibake marker scan over changed text/artifact files using the local pattern already used in prior goals.

## Bounded repairs pre-authorized

Allowed if needed:

- update stale current-state/handoff guard tests only when they incorrectly hardcode a previous latest gate;
- restore exact check-all-mutated historical artifacts from HEAD using `git restore --source=HEAD -- <exact paths>` only for accidental non-Goal066 artifacts;
- keep all such repairs in final report and commit/push final state.

Do not use broad reset/clean/stash/checkout.

## Final report format

Report in Russian:

```text
Status: GREEN / BLOCKED / FAILED
Gate: settlement_construction_destruction_production_matrix_verification required
Commit: <hash>
Push: <result>

Что стало реальнее:
...

Изменённые файлы:
...

Proof:
- rows
- state-changing rows
- construction/production/destruction/repair/defense ledgers
- save/load/replay
- Unity/player proof
- invalid matrix

Проверки:
...

Git:
...

Ограничения:
...
```
