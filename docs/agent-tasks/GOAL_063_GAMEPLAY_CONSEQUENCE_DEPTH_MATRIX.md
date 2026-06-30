# Codex task — Goal 063 Gameplay Consequence Depth Matrix

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
goal-063-gameplay-consequence-depth-matrix
Goal 063: Gameplay Consequence Depth Matrix
```

Required gate marker:

```text
gameplay_consequence_depth_matrix_verification
```

Codex reasoning level:

```text
very high
```

## Important process policy

This task must end with a commit and push to `origin/main` regardless of GREEN/BLOCKED/FAILED result.

Use honest commit messages:

```text
GREEN Goal 063 gameplay consequence depth matrix
BLOCKED Goal 063 gameplay consequence depth matrix
FAILED Goal 063 gameplay consequence depth matrix
```

Do not pretend non-green work is accepted.

Do not mark the Goal 063 manual gate passed.

## Preflight: accept Goal 062 by user handoff

The user handed off Goal 062 as GREEN:

- commit `e82718bc`
- `constrained_spatial_detail_generation_verification required`
- 9/9 spatial-detail rows
- Unity proof passed
- check-all passed 1077/1077

Record the user handoff acceptance before Goal 063:

```text
constrained_spatial_detail_generation_verification passed before Goal 063
```

Update the docs quartet accordingly:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
```

Goal 031 and Goal 032 must remain produced-for-review/not passed if current docs still model them that way.

## Read-first list

Read first:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_063_GAMEPLAY_CONSEQUENCE_DEPTH_MATRIX_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_063_GAMEPLAY_CONSEQUENCE_DEPTH_MATRIX.md`
8. Goal 060 artifacts under `.llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix/`
9. Goal 061 artifacts under `.llmgc/procedural/goal-061-full-campaign-playable-review-package-rc/`
10. Goal 062 artifacts under `.llmgc/procedural/goal-062-constrained-spatial-detail-generation/`
11. Relevant local source/tests:
    - `src/LLMGameCreator.Application/Design/FullCampaignGamePackageMaterialization/**`
    - `src/LLMGameCreator.Application/Design/FullCampaignPlayableReviewPackageRc/**`
    - `src/LLMGameCreator.Application/Design/ConstrainedSpatialDetailGeneration/**`
    - `src/LLMGameCreator.Application/Design/MultiFamilyGeneratedTemplateVerticalSlice/**`
    - `src/LLMGameCreator.Application/Design/RuntimeChunkDeltaTraversal/**`
    - existing `Unity Alpha` bootstrap script if markers are needed.
12. Existing artifact-scope policy and closest previous Goal 060/061/062 product smoke tests.

Do not read the entire repository unless a local search shows exact relevant files elsewhere.

## Allowed files / areas

You may create/edit:

```text
docs/EXTERNAL_SCOUTING_GOAL_063_GAMEPLAY_CONSEQUENCE_DEPTH_MATRIX.md
docs/GOAL_063_GAMEPLAY_CONSEQUENCE_DEPTH_MATRIX_SPEC.md
docs/agent-tasks/GOAL_063_GAMEPLAY_CONSEQUENCE_DEPTH_MATRIX.md
docs/agent-tasks/GOAL_063_LAUNCHER.txt
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/GameplayConsequenceDepthMatrix/**
tests/LLMGameCreator.Tests/Application/GameplayConsequenceDepthMatrix/**
tests/LLMGameCreator.Tests/ProductSmoke/GameplayConsequenceDepthMatrixProductSmokeTests.cs
.llmgc/procedural/goal-063-gameplay-consequence-depth-matrix/**
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

Unity source permission is narrow: only append/extend deterministic diagnostic marker loading for Goal 063. Do not redesign Unity UI.

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

Also forbidden:

- public GamePackage schema changes;
- provider/LLM/RAG calls;
- real media generation/import/network access;
- arbitrary Lua execution or generated Lua source;
- new NuGet dependencies;
- broad Runtime or Unity architecture changes;
- branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

Implement an Application-only gameplay consequence depth matrix.

The matrix must consume the accepted proof chain:

```text
Goal 060 package rows
Goal 061 playable review package RC
Goal 062 constrained spatial detail rows
```

Produce 9 family/seed gameplay consequence rows:

```text
3 families x 3 seeds = 9 rows
```

Each row must have:

- stable row id;
- family id;
- seed id;
- source package row ref;
- source review package row ref;
- source spatial-detail row ref;
- command plan;
- state transition proof;
- save/load or serializer roundtrip proof;
- replay determinism proof;
- variance contribution;
- Unity marker plan/proof.

Do not fake state changes by only changing ids/hashes. At least three meaningful state-changing steps per row are required.

## Runtime/state proof

Prefer consuming existing runtime-owned patterns already proven by earlier goals. If direct real runtime service usage is not available inside allowed scope, build an explicit Application-layer state projection with causal before/after records and state roundtrip proof.

Every row must prove:

- before state;
- command;
- after state;
- delta;
- expected vs actual;
- replay same-seed same-output;
- different seed/family meaningful variance.

## Unity proof

If current Unity Alpha bootstrap already supports the needed diagnostic route, use it. Otherwise narrowly extend:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

Required proof:

- Unity Editor route exits 0, if local route exists and previous goals use it.
- Player route exits 0, if previous proof path supports it.
- Required Goal 063 markers are present for all 9 rows.
- Heavy Unity build/log/work outputs stay ignored and are not committed.

## Evidence

Write all compact evidence under:

```text
.llmgc/procedural/goal-063-gameplay-consequence-depth-matrix/
```

Include all required artifacts from the spec.

The final markdown report must contain:

```text
implementationStatus=GREEN|BLOCKED|FAILED
accepted=false
manualGate=gameplay_consequence_depth_matrix_verification
```

For GREEN, the report must also record:

```text
rowCount=9
familyCount=3
seedCount=3
stateChangingRowCount=9
saveLoadReplayPassed=true
meaningfulVariancePassed=true
unityExitCode=0
playerExitCode=0
allGameplayMarkersMatched=true
```

If any of these cannot be proven, commit/push as BLOCKED.

## Bounded repairs pre-authorized

You may do bounded repairs if needed:

1. Update stale current-state/handoff guard tests only if they hardcode a previous latest gate and fail after docs are correctly updated.
2. Update `.devflow/artifact-scope/artifact-scope-policy.json` for Goal 063 artifacts.
3. Restore exact accidental historical `.llmgc/procedural/**` artifacts from HEAD if `check-all.ps1` mutates old tracked artifacts outside Goal 063.
4. Keep Goal 063 code/docs/evidence intact during cleanup.
5. Include any bounded repair in final report.

Do not use `git reset`, `git clean`, `git stash`, `git checkout`, `git switch`, `git merge`, `git rebase`, `git cherry-pick` or force push.

## Tests

Add focused tests proving:

- source loading from Goal 060/061/062;
- 9 row matrix construction;
- per-family required consequence shapes;
- state delta proof;
- save/load or serializer roundtrip;
- replay determinism;
- meaningful variance;
- Unity command plan/proof parsing;
- invalid/fake/leak matrix;
- evidence files are present, deterministic and JSON-parseable.

Suggested test classes:

```text
GameplayConsequenceDepthMatrixSourceLoadingTests
GameplayConsequenceDepthMatrixPlanTests
GameplayConsequenceDepthMatrixStateDeltaTests
GameplayConsequenceDepthMatrixReplayTests
GameplayConsequenceDepthMatrixUnityProofTests
GameplayConsequenceDepthMatrixInvalidMatrixTests
GameplayConsequenceDepthMatrixEvidenceTests
```

Names may follow local style.

## Validation commands

Run from:

```text
C:\Users\endim\LLMGameCreator\
```

Commands:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .	ests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~GameplayConsequenceDepthMatrix|FullyQualifiedName~Goal063"

dotnet test .	ests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~GameplayConsequenceDepthMatrixProductSmokeTests"

dotnet test .	ests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal063|FullyQualifiedName~GameplayConsequence"

.\.devflow\scripts\check-all.ps1
```

Run the existing artifact-scope guard for the Goal 063 scenario if the repo pattern supports it.

Run mojibake marker scan for changed text files/artifacts.

## Git policy

Required final push for every result.

Allowed git commands:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <explicit changed files>
git diff --cached --name-status
git diff --cached --stat
git diff --cached --check
git add -- <explicit allowed paths>
git commit -m "<GREEN/BLOCKED/FAILED message>"
git rev-parse HEAD
git push origin main
```

Forbidden:

```text
git checkout
git switch
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
Goal 063 выполнен / остановлен

Status:
GREEN / BLOCKED / FAILED

Gate:
gameplay_consequence_depth_matrix_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Gameplay consequence proof:
<row count, families, seeds, state-changing rows, save/load/replay, variance>

Runtime/state proof:
<что доказано>

Unity proof:
<unityExitCode/playerExitCode/markers>

Evidence artifacts:
<список>

Checks:
<commands/results>

Invalid/fake/leak matrix:
<covered cases>

Bounded repairs:
<none or exact list>

Git:
<commit hash and push result>

Ограничения:
<schema/runtime/UI/provider/media/Lua/deps not touched>

Следующий разумный шаг:
<one concise paragraph>
```
