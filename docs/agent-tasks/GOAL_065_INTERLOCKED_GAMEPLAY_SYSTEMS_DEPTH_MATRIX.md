# Codex task — Goal 065 Interlocked Gameplay Systems Depth Matrix

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
goal-065-interlocked-gameplay-systems-depth-matrix
Goal 065: Interlocked Gameplay Systems Depth Matrix
```

Codex reasoning level:

```text
very high
```

Gate:

```text
interlocked_gameplay_systems_depth_matrix_verification required
```

## Required preflight

1. Work on `main` only.
2. Read current source-of-truth docs before code changes.
3. Record acceptance of Goal 064 by user handoff before Goal 065:

```text
living_world_npc_faction_simulation_matrix_verification passed before Goal 065
```

4. Do not mark Goal 065 passed. Goal 065 must end as produced-for-review with:

```text
interlocked_gameplay_systems_depth_matrix_verification required
accepted=false
```

## Read-first list

Read in this order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_065_INTERLOCKED_GAMEPLAY_SYSTEMS_DEPTH_MATRIX_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_065_INTERLOCKED_GAMEPLAY_SYSTEMS_DEPTH_MATRIX.md`
8. Goal 060 artifacts under `.llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix/`
9. Goal 061 artifacts under `.llmgc/procedural/goal-061-full-campaign-playable-review-package-rc/`
10. Goal 062 artifacts under `.llmgc/procedural/goal-062-constrained-spatial-detail-generation/`
11. Goal 063 artifacts under `.llmgc/procedural/goal-063-gameplay-consequence-depth-matrix/`
12. Goal 064 artifacts under `.llmgc/procedural/goal-064-living-world-npc-faction-simulation-matrix/`
13. Local analogs in `src/LLMGameCreator.Application/Design/GameplayConsequenceDepthMatrix/`
14. Local analogs in `src/LLMGameCreator.Application/Design/LivingWorldNpcFactionSimulationMatrix/`
15. Local analogs in `tests/LLMGameCreator.Tests/Application/GameplayConsequenceDepthMatrix/`
16. Local analogs in `tests/LLMGameCreator.Tests/Application/LivingWorldNpcFactionSimulationMatrix/`
17. `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
18. `.devflow/artifact-scope/artifact-scope-policy.json`

Do not read the whole repo blindly. Use local search for exact referenced classes and artifact names.

## Allowed files / areas

You may create or edit only:

```text
docs/GOAL_065_INTERLOCKED_GAMEPLAY_SYSTEMS_DEPTH_MATRIX_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_065_INTERLOCKED_GAMEPLAY_SYSTEMS_DEPTH_MATRIX.md
docs/agent-tasks/GOAL_065_INTERLOCKED_GAMEPLAY_SYSTEMS_DEPTH_MATRIX.md
docs/agent-tasks/GOAL_065_LAUNCHER.txt
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/InterlockedGameplaySystemsDepthMatrix/**
tests/LLMGameCreator.Tests/Application/InterlockedGameplaySystemsDepthMatrix/**
tests/LLMGameCreator.Tests/ProductSmoke/InterlockedGameplaySystemsDepthMatrixProductSmokeTests.cs
.llmgc/procedural/goal-065-interlocked-gameplay-systems-depth-matrix/**
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

Unity edit is allowed only for a narrow deterministic marker loader/marker output extension for Goal 065 command plan. Do not perform broad Unity refactors.

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
public GamePackage schema/model files
```

Do not add external dependencies. Do not call LLM/provider/RAG. Do not generate media. Do not execute arbitrary Lua. Do not stage heavy Unity build/cache/log outputs.

## Exact behavior

### 1. Acceptance handoff

Update the docs quartet to record that Goal 064 was accepted by user handoff before Goal 065:

```text
living_world_npc_faction_simulation_matrix_verification passed before Goal 065
```

Keep Goal 031 and Goal 032 produced-for-review/not passed if that is the current model.

### 2. Application seam

Create an Application-layer BCL-only seam under:

```text
src/LLMGameCreator.Application/Design/InterlockedGameplaySystemsDepthMatrix/
```

Suggested small components, adjust to local style:

```text
InterlockedGameplaySystemsDepthMatrixModels
InterlockedGameplaySystemsDepthMatrixSourceLoader
InterlockedGameplaySystemsRuleCatalog
InterlockedGameplaySystemsProjector
InterlockedGameplaySystemsValidator
InterlockedGameplaySystemsEvidenceService
InterlockedGameplaySystemsHash
InterlockedGameplaySystemsUnityProofRunner
```

Avoid one giant class. Keep deterministic ordering.

### 3. Source loading

Consume Goal 060/061/062/063/064 evidence. The row matrix must be tied to actual prior artifacts and not invented from scratch.

Each row must know:

- family id;
- seed id;
- source package row;
- spatial/detail row;
- gameplay consequence row;
- living-world row;
- derived interlocked system rule set;
- expected Unity marker set.

### 4. Interlocked systems

Produce 9 state-changing rows. Each row must include domain-appropriate deltas in at least these categories:

- economy/resource/currency/trade/work ledger;
- crafting/recipe/resource conversion or upgrade ledger;
- combat/encounter/conflict pressure;
- progression/skill/status/effect/reward delta;
- inventory/equipment/loot delta;
- living-world interaction/cause trace from Goal 064;
- save/load/replay proof.

The implementation does not need to mutate public GamePackage schema. Use Application evidence records and existing package/runtime-compatible state where already available.

### 5. Family-specific expectations

`map_panel_rpg` rows must include NPC/faction work/trade/social consequence plus conflict/combat/progression delta.

`survival_sandbox` rows must include hazard/need/resource pressure, collect/consume/craft/recover deltas, plus condition/status pressure.

`first_person_grid_dungeon` rows must include orientation/traversal context, encounter/combat pressure, loot/key/progression/status deltas and valid/blocked movement consequence.

### 6. Unity Alpha marker proof

Narrowly extend `AlphaRuntimeBootstrap.cs` to load a Goal 065 staged command plan and emit deterministic markers. Required marker families should include:

```text
interlocked_gameplay_loaded
interlocked_gameplay_row
interlocked_economy_delta
interlocked_crafting_delta
interlocked_combat_delta
interlocked_progression_delta
interlocked_status_delta
interlocked_replay_verified
interlocked_gameplay_completed
review_package_proof=goal065
```

Do not claim real Unity gameplay visuals beyond the existing Alpha marker route unless physically implemented.

### 7. Evidence artifacts

Write compact deterministic evidence under:

```text
.llmgc/procedural/goal-065-interlocked-gameplay-systems-depth-matrix/
```

Required files are listed in the Goal 065 spec. JSON must be stable and parseable. Avoid absolute paths, timestamps, heavy logs, Unity build outputs and nondeterministic ordering.

### 8. Invalid/fake/leak matrix

Cover at least:

- missing Goal060 source;
- missing Goal061 source;
- missing Goal062 source;
- missing Goal063 source;
- missing Goal064 source;
- fake family id;
- fake seed id;
- duplicate row id;
- non-state-changing row;
- economy delta without source trace;
- crafting delta without resource input/output;
- combat delta without outcome;
- progression delta without causal trace;
- replay mismatch;
- save/load mismatch;
- nondeterministic ordering;
- unsafe path;
- provider/LLM/RAG/media generation claim;
- Runtime/UI/GamePackage schema mutation claim;
- Unity broad mutation claim;
- arbitrary Lua execution claim.

Each must produce causal diagnostics.

## Tests

Add focused tests under:

```text
tests/LLMGameCreator.Tests/Application/InterlockedGameplaySystemsDepthMatrix/
```

Suggested tests:

- source loading and matrix row construction;
- family-specific interlocked deltas;
- save/load/replay proof;
- variance proof;
- invalid/fake/leak matrix;
- evidence artifact parse and report checks.

Add product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/InterlockedGameplaySystemsDepthMatrixProductSmokeTests.cs
```

## Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~InterlockedGameplaySystems|FullyQualifiedName~Goal065"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~InterlockedGameplaySystemsDepthMatrixProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal065|FullyQualifiedName~InterlockedGameplay"

.\.devflow\scripts\check-all.ps1
```

Then run the existing artifact scope guard for scenario:

```text
goal-065-interlocked-gameplay-systems-depth-matrix
```

Use the existing script/pattern; do not invent a new scope tool.

Scan changed text files/artifacts for mojibake markers as in recent goals.

## Bounded repairs

Pre-authorized bounded repairs:

1. Update stale current-state/handoff tests only if they still hardcode a previous latest gate and only to check current-state consistency, not to weaken historical assertions.
2. Restore exact accidental historical generated artifacts from HEAD if `check-all.ps1` mutates unrelated tracked artifacts. Do not restore Goal 065 code/docs/evidence.
3. If Unity heavy output is generated, leave it ignored and do not commit it.
4. If a bounded repair is needed, report exact files and reason.

## Git policy

You must commit and push final state to `origin/main` regardless of GREEN/BLOCKED/FAILED.

Commit message:

- GREEN: `GREEN Goal 065 interlocked gameplay systems depth matrix`
- BLOCKED: `BLOCKED Goal 065 interlocked gameplay systems depth matrix`
- FAILED: `FAILED Goal 065 interlocked gameplay systems depth matrix`

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
git commit -m "..."
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

## Final report

Report in Russian:

```text
Status: GREEN / BLOCKED / FAILED
Gate: interlocked_gameplay_systems_depth_matrix_verification required
Commit: <hash>
Push: <result>

Что стало реальнее:
<brief>

Changed files:
<list>

Proof:
- rows 9/9
- state-changing 9/9
- save/load/replay
- variance
- Unity/player proof

Checks:
<commands/results>

Invalid/fake/leak matrix:
<summary>

Bounded repairs:
<none or exact>

Ограничения:
<forbidden areas not touched>
```
