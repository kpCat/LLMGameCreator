# Codex Task — Goal 059 Full Generator Variability Regression Matrix

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
goal_059_full_generator_variability_matrix
Goal 059: Full Generator Variability Regression Matrix
```

Required goal marker / gate:

```text
full_generator_variability_regression_matrix_verification required
```

Codex reasoning level:

```text
very high
```

## Process policy

This is an aggressive composite goal. Do not make a paper-only proof.

You must commit and push the final state to `origin/main` even if the result is GREEN, BLOCKED, or FAILED.

Commit messages:

- GREEN: `GREEN Goal 059 full generator variability regression matrix`
- BLOCKED: `BLOCKED Goal 059 full generator variability regression matrix`
- FAILED: `FAILED Goal 059 full generator variability regression matrix`

Do not mark the Goal 059 manual gate passed. Leave:

```text
full_generator_variability_regression_matrix_verification required
```

## Read-first list

Read first:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_059_FULL_GENERATOR_VARIABILITY_MATRIX_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_059_FULL_GENERATOR_VARIABILITY_MATRIX.md`
8. Goal 058 artifacts under `.llmgc/procedural/goal-058-full-media-bound-generator-campaign/`
9. Existing local analogs:
   - `src/LLMGameCreator.Application/Design/FullMediaBoundGeneratorCampaign/**`
   - `src/LLMGameCreator.Application/Design/UnityAlphaMultiFamilyPlayableLoop/**`
   - `src/LLMGameCreator.Application/Design/MediaBoundPlayableReviewPackage/**`
   - `tests/LLMGameCreator.Tests/Application/FullMediaBoundGeneratorCampaign/**`
   - `tests/LLMGameCreator.Tests/ProductSmoke/FullMediaBoundGeneratorCampaignProductSmokeTests.cs`
10. `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs` only if Unity marker extension is needed.
11. `.devflow/artifact-scope/artifact-scope-policy.json`

## Allowed files / areas

Allowed to create/edit:

```text
docs/GOAL_059_FULL_GENERATOR_VARIABILITY_MATRIX_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_059_FULL_GENERATOR_VARIABILITY_MATRIX.md
docs/agent-tasks/GOAL_059_FULL_GENERATOR_VARIABILITY_MATRIX.md
docs/agent-tasks/GOAL_059_LAUNCHER.txt
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/FullGeneratorVariabilityRegressionMatrix/**
tests/LLMGameCreator.Tests/Application/FullGeneratorVariabilityRegressionMatrix/**
tests/LLMGameCreator.Tests/ProductSmoke/FullGeneratorVariabilityRegressionMatrixProductSmokeTests.cs
.llmgc/procedural/goal-059-full-generator-variability-regression-matrix/**
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

The Unity file is allowed only for a narrow deterministic matrix command-plan marker extension.

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

Also forbidden:

- new NuGet dependencies;
- real provider/LLM/RAG calls;
- real media generation/import/network calls;
- arbitrary Lua execution;
- public GamePackage schema changes;
- broad Runtime/UI/Unity refactors;
- heavy Unity build/log/unity-work outputs in Git.

## Exact behavior

### 1. Preflight: accept Goal 058 by handoff

Record the user's handoff acceptance:

```text
full_media_bound_generator_campaign_verification passed before Goal 059
```

Do not start by making an acceptance-only commit. Include this in the Goal 059 commit.

Goal 059 must remain produced-for-review:

```text
full_generator_variability_regression_matrix_verification required
```

### 2. Build Application-only variability matrix seam

Create small classes under:

```text
src/LLMGameCreator.Application/Design/FullGeneratorVariabilityRegressionMatrix/
```

Suggested files:

```text
FullGeneratorVariabilityMatrixModels.cs
FullGeneratorVariabilityMatrixSourceLoader.cs
FullGeneratorVariabilityMatrixBuilder.cs
FullGeneratorVariabilityMatrixValidator.cs
FullGeneratorVariabilityMatrixEvidenceService.cs
FullGeneratorVariabilityMatrixHash.cs
FullGeneratorVariabilityUnityProofRunner.cs
```

Avoid giant monoliths.

### 3. Consume Goal 058 source chain

Load the Goal 058 campaign artifacts and verify exact expected files/hashes where practical.

You must consume at least:

- campaign source manifest;
- campaign plan;
- family run proofs;
- unified review package manifest;
- preview/export campaign payload;
- Unity command plan;
- Unity player proof;
- invalid matrix/report where useful.

Do not invent source facts that are missing.

### 4. Generate deterministic seed/profile matrix

Minimum required matrix:

- 3 families x 3 seeds = 9 rows.

Families:

```text
map_panel_rpg
survival_sandbox
first_person_grid_dungeon
```

Seeds:

```text
seed_alpha
seed_beta
seed_gamma
```

Each row must include:

- row id;
- family id;
- seed id;
- source manifest/campaign refs;
- derived campaign hash;
- selected media refs;
- selected family-loop refs;
- selected preview/export refs;
- deterministic marker plan;
- variance explanation.

### 5. Prove real variation, not id-only variation

Implement variance metrics and fail/diagnose overfit cases.

Required minimum metrics:

- 9 distinct row ids;
- 9 distinct derived row hashes;
- 3 families represented;
- 3 seeds represented;
- media binding coverage;
- family-loop marker coverage;
- at least two meaningful variation dimensions per family across seeds;
- row-pair or row-group difference summary.

### 6. Replay determinism

Build every row twice from the same source facts and compare stable hash/JSON.

Record replay proof for all 9 rows.

### 7. Review package and preview/export matrix payload

Write compact matrix review and preview/export payloads suitable for future UI/export consumers.

Do not copy heavy Unity build outputs.

### 8. Unity Alpha matrix proof

Prefer one Unity/player route that consumes `unity-alpha-matrix-command-plan.json` and emits matrix markers for all 9 rows.

You may narrowly extend `AlphaRuntimeBootstrap.cs` to read the matrix command plan and emit deterministic markers.

GREEN requires honest proof:

- either all 9 rows have Unity/player markers;
- or all 9 rows have Application replay proof and at least one row per family has Unity/player markers, with the report explicitly recording bounded Unity coverage.

If you cannot safely extend the Unity route, commit/push BLOCKED with evidence.

### 9. Evidence writer

Write required compact artifacts under:

```text
.llmgc/procedural/goal-059-full-generator-variability-regression-matrix/
```

Required files are defined in the spec.

### 10. Invalid/fake/leak matrix

Implement causal invalid diagnostics covering at least the cases listed in the spec.

### 11. State docs and queue

Update the docs quartet consistently:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Expected state:

- Goal 058 accepted by user handoff before Goal 059.
- Goal 059 produced-for-review.
- Goal 059 gate required, not passed.
- Recommend next work after Goal 059 based on results, but do not implement Goal 060.

### 12. Artifact scope policy

Update `.devflow/artifact-scope/artifact-scope-policy.json` for Goal 059 scope.

## Tests

Add focused tests under:

```text
tests/LLMGameCreator.Tests/Application/FullGeneratorVariabilityRegressionMatrix/
```

Suggested test coverage:

- source loading and Goal 058 reference validation;
- seed/profile matrix count and stable ordering;
- meaningful variance metrics;
- replay determinism;
- review package matrix manifest;
- preview/export payload;
- Unity matrix command plan / proof parsing;
- invalid/fake/leak matrix.

Add product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/FullGeneratorVariabilityRegressionMatrixProductSmokeTests.cs
```

## Validation commands

Run from repo root:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~FullGeneratorVariabilityRegressionMatrix|FullyQualifiedName~Goal059"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~FullGeneratorVariabilityRegressionMatrixProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal059|FullyQualifiedName~FullGeneratorVariability"

.\.devflow\scripts\check-all.ps1
```

Run artifact scope guard using the existing repository script/pattern for scenario:

```text
goal-059-full-generator-variability-regression-matrix
```

If exact script invocation differs, use the established local pattern from Goal 058.

## Bounded repairs pre-authorized

You may perform bounded repairs without stopping if they are required by check-all and are clearly caused by current-state handoff drift or artifact-scope policy updates:

1. Update stale current-state/handoff guard assertions to current-state consistency, preserving historical goal-specific checks.
2. Restore exact accidental historical `.llmgc/procedural/**` mutations from HEAD with `git restore --source=HEAD -- <exact paths>` only if check-all mutates old tracked artifacts.
3. Add Goal 059 artifact-scope policy entries.
4. Narrowly extend `AlphaRuntimeBootstrap.cs` for matrix markers only.

Do not use bounded repairs to hide real failures.

## Git policy

Must commit and push final state even when GREEN/BLOCKED/FAILED.

Allowed git operations:

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
git commit -m "GREEN Goal 059 full generator variability regression matrix"
git commit -m "BLOCKED Goal 059 full generator variability regression matrix"
git commit -m "FAILED Goal 059 full generator variability regression matrix"
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
Goal 059 выполнен / заблокирован / провален
Status: GREEN / BLOCKED / FAILED
Gate: full_generator_variability_regression_matrix_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Matrix proof:
<families, seeds, row count, variance metrics, replay proof>

Unity proof:
<unity/player execution, marker coverage, bounded coverage if any>

Evidence artifacts:
<список основных файлов>

Проверки:
<commands and results>

Invalid/fake/leak matrix:
<summary>

Git:
<commit hash and push result>

Ограничения:
<what was not touched>

Следующий разумный шаг:
<Goal 060 recommendation, no implementation>
```
