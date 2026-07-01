# Codex task — Goal 072 Generator Spine Quality Consolidation And Risk Audit

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
goal-072-generator-spine-quality-consolidation
Goal 072: Generator Spine Quality Consolidation And Risk Audit
```

Required gate:

```text
generator_spine_quality_consolidation_verification required
```

Codex reasoning level:

```text
very high
```

## Why this goal exists

The repository has grown quickly through aggressive composite goals. The current risk is no longer lack of generated playable proof. The risk is hidden technical debt: too-large files, shallow tests, repeated generated-seam patterns, Unity Alpha bootstrap bloat, fragile artifact evidence, and proof paths that might become marker-only.

This task must audit the actual repository, not just read previous reports.

## Git policy — mandatory final commit/push

You must commit and push final state to `origin/main` regardless of GREEN/BLOCKED/FAILED result.

Use honest commit messages:

```text
GREEN Goal 072 generator spine quality consolidation
BLOCKED Goal 072 generator spine quality consolidation
FAILED Goal 072 generator spine quality consolidation
```

Never mark the gate passed. Goal 072 must remain produced-for-review.

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

Allowed bounded git operations:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <explicit paths>
git diff --cached --name-status
git diff --cached --stat
git diff --cached --check
git add -- <explicit allowed paths>
git commit -m "<honest message>"
git rev-parse HEAD
git push origin main
```

## Read-first list

Read in this order:

1. `AGENTS.md`
2. `README.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CURRENT_GENERATOR_STATE.json`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/GOAL_072_GENERATOR_SPINE_QUALITY_CONSOLIDATION_SPEC.md`
8. `docs/EXTERNAL_SCOUTING_GOAL_072_GENERATOR_SPINE_QUALITY_CONSOLIDATION.md`
9. Recent Goal 071 quality audit commit context:
   - `tests/LLMGameCreator.Tests/Application/UnityAlphaInteractiveCampaignPlayer/UnityAlphaInteractiveCampaignPlayerTests.cs`
   - `tests/LLMGameCreator.Tests/ProductSmoke/UnityAlphaInteractiveCampaignPlayerProductSmokeTests.cs`
   - `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
10. Recent high-risk source areas:
   - `src/LLMGameCreator.Application/Design/*`
   - `tests/LLMGameCreator.Tests/Application/*`
   - `tests/LLMGameCreator.Tests/ProductSmoke/*`
   - `.llmgc/procedural/goal-060-*` through `.llmgc/procedural/goal-071-*`
11. Existing artifact-scope policy:
   - `.devflow/artifact-scope/artifact-scope-policy.json`

Do not read the entire repository blindly. Use targeted scans/scripts.

## Allowed files / areas

You may create/edit:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/GOAL_072_GENERATOR_SPINE_QUALITY_CONSOLIDATION_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_072_GENERATOR_SPINE_QUALITY_CONSOLIDATION.md
docs/agent-tasks/GOAL_072_GENERATOR_SPINE_QUALITY_CONSOLIDATION.md
docs/agent-tasks/GOAL_072_LAUNCHER.txt
docs/technical-debt/**
.devflow/artifact-scope/artifact-scope-policy.json
.llmgc/procedural/goal-072-generator-spine-quality-consolidation/**
src/LLMGameCreator.Application/Design/GeneratorSpineQualityConsolidation/**
tests/LLMGameCreator.Tests/Application/GeneratorSpineQualityConsolidation/**
tests/LLMGameCreator.Tests/Devflow/**
tests/LLMGameCreator.Tests/ProductSmoke/**
```

You may also edit these only for bounded P0/P1 fixes discovered by the audit:

```text
src/LLMGameCreator.Application/Design/UnityAlphaInteractiveCampaignPlayer/**
src/LLMGameCreator.Application/Design/IntegratedCampaignTimelineSimulationMatrix/**
src/LLMGameCreator.Application/Design/GameplayConsequenceDepthMatrix/**
src/LLMGameCreator.Application/Design/LivingWorldNpcFactionSimulationMatrix/**
src/LLMGameCreator.Application/Design/InterlockedGameplaySystemsDepthMatrix/**
src/LLMGameCreator.Application/Design/SettlementConstructionDestructionProductionMatrix/**
src/LLMGameCreator.Application/Design/ProgrammaticNarrativeQuestDialogueEventMatrix/**
src/LLMGameCreator.Application/Design/CombatMagicAbilityBossEncounterMatrix/**
src/LLMGameCreator.Application/Design/WorldEventWeatherDayNightCrisisMatrix/**
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

If you need to edit any other source file, stop with BLOCKED and commit/push the audit evidence.

## Forbidden files / areas

Do not edit:

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

No external dependencies.

## Exact behavior

### 1. Preflight

- Confirm branch is `main`.
- Confirm no unexpected tracked changes before work.
- Treat untracked Goal 072 task/spec/scouting/launcher files as part of the task package if present.
- Record Goal 071 acceptance by user handoff in docs quartet:

```text
unity_alpha_interactive_campaign_player_verification passed before Goal 072
```

- Set Goal 072 as active/produced-for-review:

```text
generator_spine_quality_consolidation_verification required
accepted=false
```

### 2. Build a deterministic quality scanner/evidence seam

Implement a BCL-only scanner under:

```text
src/LLMGameCreator.Application/Design/GeneratorSpineQualityConsolidation/
```

Suggested components:

```text
GeneratorSpineQualityModels.cs
GeneratorSpineQualityScanner.cs
GeneratorSpineQualityRiskClassifier.cs
GeneratorSpineQualityEvidenceService.cs
GeneratorSpineQualityHash.cs
```

Keep files reasonably sized. Do not create a God class.

The scanner must collect at least:

- source file line counts;
- max line length;
- one-line/minified candidate files;
- large file candidates;
- repeated seam-role names by folder (`SourceLoader`, `EvidenceService`, `Hash`, `Validator`, `UnityProofRunner`, `Projector`, `Builder`);
- Unity Alpha bootstrap line count and marker-route count;
- product smoke tests with shallow assertion heuristics;
- artifacts with absolute-path-like strings;
- artifacts with suspicious timestamp-like volatile values;
- current-state/gate consistency;
- Goal 071 proof quality indicators.

### 3. Risk classification

Classify findings as P0/P1/P2/P3 using the spec.

P0 examples:

- minified/one-line C# with many declarations;
- absolute local paths in compact evidence;
- staged proof input missing but tests still pass;
- current-state docs inconsistent;
- check-all failure.

P1 examples:

- very large files in recent seams;
- shallow product-smoke checks;
- AlphaRuntimeBootstrap growth risk;
- repeated helpers needing future shared extraction.

P2 examples:

- broad framework extraction candidates;
- generic proof runner consolidation;
- common source loader/evidence writer framework.

### 4. Safe bounded fixes

Apply safe fixes only:

- If Goal 071 or recent files are minified/one-line: reformat without logic changes.
- If product smoke tests in allowed recent areas are shallow: add assertions on counts, hashes, before/after deltas, matched/missing markers, and staged input presence.
- If AlphaRuntimeBootstrap has a small local extraction available: extract private helper methods without broad architecture refactor.
- If current artifacts have absolute paths/timestamps: fix evidence generation and regenerate only current Goal 072 evidence or bounded current-goal artifacts when safe.

Do not attempt large cross-goal refactoring.

### 5. Technical debt register

Write or update:

```text
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
```

Include:

- finding id;
- severity P0/P1/P2/P3;
- area;
- evidence;
- recommended future goal;
- whether fixed in Goal 072;
- why not fixed if not fixed.

This register must be concrete, not vague.

### 6. Evidence artifacts

Write all required evidence under:

```text
.llmgc/procedural/goal-072-generator-spine-quality-consolidation/
```

Required files:

```text
quality-inventory-summary.json
source-format-risk-report.json
large-file-and-method-risk-report.json
unity-alpha-bootstrap-risk-report.json
proof-quality-risk-report.json
artifact-reproducibility-risk-report.json
safe-fix-summary.json
technical-debt-register.json
quality-dashboard.json
generator-spine-quality-consolidation-report.md
```

The final markdown report must contain:

```text
generator_spine_quality_consolidation_verification required
accepted=false
```

### 7. Tests

Add focused tests under:

```text
tests/LLMGameCreator.Tests/Application/GeneratorSpineQualityConsolidation/
```

Test at least:

- scanner identifies a deliberately minified file fixture;
- scanner does not flag normal files as minified;
- large-file risk classification is deterministic;
- absolute-path detection catches Windows and Unix-style paths;
- proof-quality heuristic catches report-only shallow smoke fixture;
- Goal 071 proof indicators are recognized;
- evidence writer produces required files;
- quality dashboard contains P0/P1/P2 counts and recommended next actions.

If you modify existing product smoke tests, keep their original intent and add stronger assertions, not weaker ones.

### 8. Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~GeneratorSpineQuality|FullyQualifiedName~Goal072"
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~UnityAlphaInteractiveCampaignPlayer|FullyQualifiedName~Goal071"
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal072|FullyQualifiedName~GeneratorSpineQuality"
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-072-generator-spine-quality-consolidation"
```

If a filter matches too broadly and times out, rerun using exact class filters and document it.

### 9. Artifact scope guard

Add or update artifact-scope policy for Goal 072 so the new evidence folder and allowed docs/test/source changes are accepted.

### 10. Stop / BLOCKED conditions

Commit/push as BLOCKED if:

- P0 is found but cannot be fixed safely in allowed scope;
- check-all fails and cannot be fixed safely;
- broad Runtime/GamePackage/Unity architecture changes are required;
- a real proof route appears fake/hardcoded and needs broad rewrite;
- the quality scan identifies too many risky files for safe bounded repair.

Do not hide the problem. Commit/push the audit evidence.

## Final report format

Report in Russian:

```text
Goal 072 выполнен / заблокирован / провален

Status:
GREEN / BLOCKED / FAILED

Gate:
generator_spine_quality_consolidation_verification required

Что реально проверено:
<summary>

Safe fixes:
<files and reasons>

P0/P1/P2/P3 findings:
<count and top items>

Debt register:
<path and summary>

Evidence artifacts:
<required files>

Проверки:
<commands/results>

Git:
<commit hash and push result>

Ограничения:
<what was not touched>

Следующий разумный шаг:
<feature goal or follow-up audit/hotfix>
```
