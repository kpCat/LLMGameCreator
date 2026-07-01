# Codex Task — Goal 073 Source Format P0 Readability Repair

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
goal-073-source-format-p0-readability-repair
Goal 073: Source Format P0 Readability Repair
```

Codex reasoning level:

```text
very high
```

Required gate marker:

```text
source_format_p0_readability_repair_verification required
```

## Background

Goal 072 produced a useful audit but ended honestly BLOCKED:

```text
generator_spine_quality_consolidation_verification required
implementationStatus=BLOCKED
p0Count=1
```

The P0 is concrete source-format debt:

```text
GQ-P0-SOURCE-EXTREME-LINE-LENGTH
```

Goal 073 must repair this P0 in a narrow, behavior-preserving way.

## Read-first list

Read first, in this order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_073_SOURCE_FORMAT_P0_READABILITY_REPAIR_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_073_SOURCE_FORMAT_P0_READABILITY_REPAIR.md`
8. `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
9. `.llmgc/procedural/goal-072-generator-spine-quality-consolidation/source-format-risk-report.json`
10. `.llmgc/procedural/goal-072-generator-spine-quality-consolidation/quality-dashboard.json`
11. `.llmgc/procedural/goal-072-generator-spine-quality-consolidation/generator-spine-quality-consolidation-report.md`
12. Existing Goal 072 tests:
    - `tests/LLMGameCreator.Tests/Application/GeneratorSpineQualityConsolidation/**`
    - `tests/LLMGameCreator.Tests/ProductSmoke/GeneratorSpineQualityConsolidationProductSmokeTests.cs`

Then inspect only the P0 candidate files listed below.

## Allowed files

You may edit only:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilityHelpCatalog.cs
src/LLMGameCreator.Application/Design/PackageAssemblyCombatProgression/PackageAssemblyCombatProgressionAcceptanceService.cs
src/LLMGameCreator.Application/Design/CombatMagicAbilityBossEncounterMatrix/CombatMagicAbilityBossEncounterProjector.cs
tests/LLMGameCreator.Tests/Application/PackageAssemblyCombatProgression/PackageAssemblyCombatProgressionAcceptanceTests.cs
src/LLMGameCreator.Application/Design/LuaModuleManifestRegistry/LuaModuleManifestRegistryCatalog.cs
src/LLMGameCreator.Application/Design/PackageAssemblyDialogueQuests/PackageAssemblyDialogueQuestsAcceptanceService.cs
src/LLMGameCreator.Application/Design/PackageAssemblyItemsEconomyCrafting/PackageAssemblyItemsEconomyCraftingAcceptanceService.cs
src/LLMGameCreator.Application/Design/CandidateModules/WorldBiomeNoise/WorldBiomeNoiseCandidateService.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

docs/GOAL_073_SOURCE_FORMAT_P0_READABILITY_REPAIR_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_073_SOURCE_FORMAT_P0_READABILITY_REPAIR.md
docs/agent-tasks/GOAL_073_SOURCE_FORMAT_P0_READABILITY_REPAIR.md
docs/agent-tasks/GOAL_073_LAUNCHER.txt

.llmgc/procedural/goal-073-source-format-p0-readability-repair/**
.devflow/artifact-scope/artifact-scope-policy.json
```

You may read but must not modify Goal 072 code/tests/artifacts unless explicitly necessary to run existing tests. Prefer creating Goal 073 evidence rather than rewriting Goal 072 historical evidence.

## Forbidden files / areas

Do not edit:

```text
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/**
src/LLMGameCreator.Scripting/**
unity/**
generator-library/**
samples/**
templates/**
*.sln
*.csproj
```

Do not add external dependencies.

Forbidden git operations:

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

## Exact behavior

### 1. Preflight

Confirm:

- branch is `main`;
- worktree is clean except the Goal 073 task package files if they were just unpacked;
- Goal 072 is currently BLOCKED due to `GQ-P0-SOURCE-EXTREME-LINE-LENGTH`.

### 2. Measure before editing

Create a before table for the eight P0 candidate files:

- relative path;
- line count;
- max line length;
- line number of max line;
- short reason/category if obvious.

Do not include absolute paths.

### 3. Repair P0 source format

For each of the eight files, repair only extreme line lengths:

- break long initializers/literals/assertions into readable multi-line form;
- use local variables for long repeated expressions;
- preserve deterministic ordering;
- preserve ids/string values exactly unless line wrapping requires string concatenation and tests prove behavior unchanged;
- avoid large algorithm changes.

Target:

```text
maxLineLength <= 300 preferred
maxLineLength <= 500 required
```

If a file cannot be safely repaired without behavior risk, do not hide it. Commit/push BLOCKED and explain.

### 4. Evidence artifacts

Create:

```text
.llmgc/procedural/goal-073-source-format-p0-readability-repair/source-format-p0-before.json
.llmgc/procedural/goal-073-source-format-p0-readability-repair/source-format-p0-after.json
.llmgc/procedural/goal-073-source-format-p0-readability-repair/source-format-p0-repair-summary.json
.llmgc/procedural/goal-073-source-format-p0-readability-repair/source-format-p0-readability-repair-report.md
.llmgc/procedural/goal-073-source-format-p0-readability-repair/artifact-scope-report.json
```

The summary/report must include:

```text
source_format_p0_readability_repair_verification required
implementationStatus=GREEN/BLOCKED/FAILED
p0BeforeCount
p0AfterCount
repairedFileCount
behaviorPreservationStrategy
```

No timestamps, no absolute paths, no heavy logs.

### 5. Debt register and state docs

Update `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`:

- mark `GQ-P0-SOURCE-EXTREME-LINE-LENGTH` as repaired by Goal 073 if `p0AfterCount=0`;
- keep P1/P2 debt registered;
- do not delete historical Goal 072 BLOCKED evidence.

Update state docs quartet consistently:

- record Goal 073 produced-for-review;
- if P0 repaired, state that Goal 072 P0 blocker was repaired by Goal 073;
- keep Goal 031/032 produced-for-review/not passed unless already recorded otherwise;
- do not start any feature goal.

### 6. Artifact scope

Add/update artifact-scope policy for:

```text
goal-073-source-format-p0-readability-repair
```

Only allow the explicit paths and prefixes in this task.

### 7. Validation

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~GeneratorSpineQuality|FullyQualifiedName~Goal072|FullyQualifiedName~Goal073"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~PackageAssemblyCombatProgression|FullyQualifiedName~PackageAssemblyDialogueQuests|FullyQualifiedName~PackageAssemblyItemsEconomyCrafting|FullyQualifiedName~LuaModuleManifestRegistry|FullyQualifiedName~CombatMagicAbilityBossEncounter|FullyQualifiedName~WorldBiomeNoise|FullyQualifiedName~GeneratorPlan"

.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-073-source-format-p0-readability-repair"
```

Also run a simple max-line-length scan over the eight repaired files and include results in the final report/evidence.

### 8. Commit/push policy

Commit/push final state even if BLOCKED/FAILED.

Commit messages:

```text
GREEN Goal 073 source format P0 readability repair
BLOCKED Goal 073 source format P0 readability repair
FAILED Goal 073 source format P0 readability repair
```

## Final report format

```text
Goal 073 выполнен / остановлен

Status:
GREEN / BLOCKED / FAILED

Gate:
source_format_p0_readability_repair_verification required

P0 repair:
- before count:
- after count:
- repaired files:
- remaining P0, if any:

Evidence:
<files>

Checks:
<commands/results>

Debt register:
<what changed>

State docs:
<what changed>

Git:
<commit hash / push result>

Limitations:
<what was not touched>
```
