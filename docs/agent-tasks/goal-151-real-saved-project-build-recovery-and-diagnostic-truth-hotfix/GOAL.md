# Goal 151 — Real Saved Project Build Recovery + Diagnostic Truth Hotfix

## Identity

- Task ID: `goal-151-real-saved-project-build-recovery-and-diagnostic-truth-hotfix`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `a6c6892c636a4eadbf858978aec608a713f152da`
- Required base message: `BLOCKED Goal 150F PowerShell parser gate and acceptance closure execution hotfix`
- Real affected project, read-only source: `C:\Users\endim\AppData\Local\LLMGameCreator\Games\goal148-manual`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Sol
Reasoning effort: High
```

Reason: this is a real P1 saved-project lifecycle failure spanning persisted authoring state, identity recovery, parameterized composition, Runtime qualification, transaction rollback and WinForms diagnostic presentation. The task is tightly bounded, so Extra High is not required.

## Pre-approval

The owner approved execution by launching this task.

- Give a concise internal plan.
- Do not ask for plan confirmation.
- Begin after base/worktree/source-project preflight.
- Do not ask the owner to manually reproduce or inspect hidden files.
- Do not create validation-candidate commits.
- Produce at most one final status commit and push it yourself.

## User-observed failure

The normal product path was used:

```text
Игры
→ goal148-manual / Проверка конструктора
→ Механики
→ Настройки
→ Собрать и проверить игру
```

The UI displayed:

```text
Есть ошибки
Игра не прошла проверку Runtime. Текущий пакет не изменён.
```

The project technical state showed:

```text
projectFolder=C:\Users\endim\AppData\Local\LLMGameCreator\Games\goal148-manual
Project package ID=game/goal148-manual
Project title=Проверка конструктора
Project-scoped composition ID=project-game-goal148-manual-b64404fafc75
Identity source=recovered_after_template_overwrite
authoringRevision=3
catalogFingerprint=62b82e28b4cd1edfc50da9a4d46832d7c1e024b36f994b593b35bdd1d1a6dd29
```

All required modules and all six current optional modules appeared selected:

```text
feature.profile.exploration_resource_focus
feature.profile.alchemy_focus
feature.profile.combat_focus
feature.equipment.weapon_loadout
feature.character.attributes
feature.character.level_progression
```

The technical panel also displayed old baseline hashes and zero current-attempt capability/action fields:

```text
composition=e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221
activated=c46826d8231951ab941f6ee1608d30273b1e186f920ea8cad58c58c25317eeeb
final=95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8
capability/action/checkpoint/replay=0/0/0/0
```

These fields currently mix:

- hashes from the last successful persisted build;
- zero-valued fields from the failed current attempt.

They do not prove that the selected modules were ignored.

## Independent audit findings

### 1. The failure occurs inside parameterized composition qualification

`GameProjectBuildAndQualificationService.Build` reaches:

```text
materializer.MaterializeAndQualify(...)
materialized.Passed == false
```

and returns:

```text
Игра не прошла проверку Runtime. Текущий пакет не изменён.
```

### 2. Failure diagnostics are discarded

`FeatureModuleParameterizedCompositionResult.Passed` may be false because of:

- parameter binding failure;
- package validation;
- mutation audit;
- composition validation;
- order-independence failure;
- invalid-action mutation;
- checkpoint replay;
- full replay;
- action binding;
- semantic Runtime-effect observations;
- satisfied selected-module count.

But `GameProjectBuildAndQualificationService` currently forwards only:

```text
materialized.Qualification.Result.Diagnostics
```

`FeatureModuleCompositionService.BuildComposition` does not populate comprehensive `FeatureModuleCompositionResult.Diagnostics`.

Therefore a real semantic/effect failure can produce an empty diagnostic list and only the generic UI message.

### 3. Technical details mix last success and current failure

`UnifiedGameProjectWorkspaceController.Snapshot()` reads persisted hashes from the authoring document while reading action/capability summaries from `_lastBuild`.

After rollback, this produces a misleading hybrid view.

### 4. Existing tests do not close the real lifecycle gap

The repository has synthetic/temp tests that claim GREEN for:

- recovered `goal148-manual`;
- all six optional modules;
- custom `3/8/2/12`;
- reopen/rebuild.

The real saved project still fails.

The missing coverage may be:

- an already-existing project-scoped authoring document from an earlier catalog generation;
- revision/history/fingerprint transitions over multiple real app versions;
- persisted parameter values not represented by the fixtures;
- certification-cache state;
- package/support-file state;
- a stale running executable relative to the repository/catalog;
- another property only present in the real project.

Do not assume which one. Prove it from the read-only real-project copy.

## Product objectives

1. Reproduce the user failure from an isolated byte-for-byte copy of the real project.
2. Determine the exact first failing stage, module, action or Runtime effect.
3. Fix the generic product cause without hardcoding `goal148-manual`, Goal numbers or exact hashes.
4. Make every future build failure diagnostically complete and actionable.
5. Separate last-success evidence from current-attempt evidence in the UI.
6. Prove the original project folder was not mutated during diagnosis.
7. Prove a copied real project succeeds after the fix with the intended module/parameter state.
8. Preserve transaction rollback and project identity.
9. Move the unrelated 64 historical snapshot failures to explicit validation debt; they are not the Goal151 acceptance gate.

## Safety boundary: original project is read-only

The source project is:

```text
C:\Users\endim\AppData\Local\LLMGameCreator\Games\goal148-manual
```

Codex must never call the Application build/activation path directly against that folder.

### Required original-state manifest

Before any diagnostic work, hash at least:

```text
package.json
.llmgc/project-identity.json
.llmgc/authoring/**
.llmgc/certification-cache/**
.llmgc/build-history/**
```

Also inventory all regular files in the project except transient `.llmgc/build-staging/**`.

After all task work, recompute the same manifest.

Required:

```text
originalProjectMutationCount=0
originalTrackedStateByteIdentical=true
```

Do not commit raw project files, private absolute-path payloads, save data or personal content.

### Diagnostic copy

Create a disposable copy under:

```text
.devflow/runs/goal151-*/real-project-copy/
```

Exclude only transient build-staging contents.

All opening, saving, building and activation tests run against the copy.

## Phase A — provenance and reproduction

### A1. Build current HEAD

Run a clean normal repository build before reproduction.

Record:

```text
repositoryCommit
built WinForms assembly path
assembly SHA-256
assembly last-write UTC
assembly informational/file version
```

### A2. Running executable provenance

If an `LLMGameCreator` process is running, inspect it read-only and record when accessible:

```text
process executable path
process executable SHA-256
file version
last-write UTC
matches freshly built binary=true/false
```

Do not terminate the process.

Absence of a running process is not a blocker.

### A3. Reproduce with current HEAD code

Use the real Application composition root/services, not a mocked build.

On the disposable copy:

1. load `package.json`;
2. open the unified workspace;
3. capture identity, scoped/legacy authoring documents, selected modules, explicit/effective parameter values and fingerprints;
4. save/reopen once;
5. run `BuildAndQualify`;
6. capture all stage data before rollback cleanup removes staging.

Required pre-fix evidence:

```text
failureReproduced=true|false
failureStage
firstCausalDiagnostic
selectedOptionalModuleIds
explicitParameterValues
effectiveParameterValues
parameterBindingPassed
materializedPackageSha256
packageValidationPassed
mutationAuditPassed
compositionValidationPassed
orderIndependencePassed
capabilityPlanId/count/actions
checkpointPassed
fullReplayPassed
actionBindingPassed
semanticObservationCount
failedSemanticObservations
satisfiedSelectedModuleCount
```

### A4. Stale binary decision

If current HEAD succeeds on the copy but the running binary differs:

```text
rootCause=stale_executable_or_launch_target
```

Do not invent a composition defect.

Still implement truthful executable/build provenance in Technical Details and create a deterministic current-binary launcher/build proof appropriate to the repository.

If current HEAD also fails, fix the reproduced generic product defect.

## Phase B — diagnostic truth

### B1. Aggregated composition diagnostics

Add a generic diagnostic builder for parameterized composition failures.

It must include only failed conditions and provide stable codes, for example:

```text
composition.parameter_binding.failed
composition.package_validation.failed
composition.mutation_audit.failed
composition.validation.failed
composition.order_independence.failed
runtime.invalid_action_state_changed
runtime.checkpoint_replay.failed
runtime.full_replay.failed
runtime.action_binding.failed
runtime.semantic_effect.failed
runtime.selected_module_unsatisfied
```

For each failed semantic observation include:

```text
moduleId
effectId
metricKind
targetId
expectedValue
actualValue
diagnostics
```

No failure may return an empty diagnostic list.

### B2. Stage-aware build result

Extend `GameProjectBuildResult` with stable, generic attempt fields such as:

```text
AttemptId
AttemptStatus
FailureStage
AttemptedSelectedModuleIds
AttemptedConfiguredParameterCount
AttemptedCapabilityCount
AttemptedPlannedActionCount
AttemptedCheckpointActionCount
AttemptedFinalReplayActionCount
AttemptedCompositionPackageSha256
AttemptedFinalStateHash
```

Populate as far as the pipeline reached.

Keep last-success fields separate.

### B3. Preserve failure evidence before staging cleanup

Write a compact failed-attempt history record under the copied/project build-history only after sensitive path confinement.

For real application behavior, a failed attempt should write a small diagnostic record without activating the failed package.

It must not contain raw private file content.

### B4. UI truth

In `Игры → Сборка и проверка`:

- show `Этап сбоя`;
- show the first causal diagnostic prominently;
- show all diagnostics below;
- do not display a generic Runtime failure with no cause.

In `Технические детали`, label separate sections:

```text
Последняя успешная сборка
Последняя попытка сборки
Текущая сохранённая конфигурация
```

Do not present old hashes beside current failed-attempt zero counts as one result.

Show executable provenance:

```text
Executable path
Executable SHA-256
File/informational version
```

Do not expose Goal numbers in normal UI.

## Phase C — generic root fix

After exact reproduction, fix only the proven cause.

Possible areas may include, but are not assumptions:

```text
persisted scoped-authoring migration
additive catalog evolution
parameter default/effective-value binding
certification cache invalidation
capability plan construction
semantic-effect expected/actual synchronization
reopen/save lifecycle
stale executable launch path
```

Forbidden shortcuts:

- project-folder-name special case;
- package-ID special case;
- exact-hash branch;
- disabling semantic checks;
- declaring failed effects non-required;
- weakening checkpoint/replay/action binding;
- resetting all user selections;
- deleting user authoring/certification data;
- replacing the current project with a template;
- mutating the original source folder.

## Required regression fixtures

### C1. Existing scoped project lifecycle fixture

Create a deterministic repository-local test fixture by code, not by committing the user's project.

Required lifecycle:

1. create the old composed-template project package;
2. create legacy authoring with the accepted three profile modules/values;
3. open under identity recovery and create the project-scoped document;
4. successfully build baseline;
5. close/reopen;
6. simulate catalog evolution to equipment/attributes/progression;
7. select all six optional modules;
8. apply `3/8/2/12`;
9. save;
10. close/reopen;
11. build and qualify;
12. repeat build.

This must model an already-existing scoped document and revision progression, not only first-open migration.

### C2. Actual copied project

The actual disposable copy must pass after the fix.

Expected user-facing result:

```text
Игра успешно собрана и проверена.
Бонус урона: +3
Сила: 8
Бонус урона от силы: +6
Уровень: 2
Опыт: 12
```

If the real project contains different explicit values, first record them. Set `3/8/2/12` only on the disposable copy through the normal controller path, save/reopen, then build.

Expected:

```text
equipment/stat/total=3/6/9
level/XP=2/12
checkpoint reload passed
full replay equivalent
action binding passed
project identity preserved
transactional activation passed
```

### C3. Failure diagnostic regression

Inject one semantic mismatch and one checkpoint/replay failure.

Assert:

- `Passed=false`;
- exact `FailureStage`;
- non-empty causal diagnostics;
- current package byte-identical;
- selected modules/parameter edits retained;
- last-success section unchanged;
- current-attempt section reports the failure honestly.

### C4. UI binding regression

Use the real WinForms page/controller binding.

Assert that failure details survive the post-build `BindWorkspace` call and are visible without opening raw artifacts.

## Validation policy reset

The 64 Goal150F historical snapshot failures are not part of Goal151 acceptance.

Record them in:

```text
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
```

as a bounded historical-fixture/validation-debt cluster.

Do not mark them fixed.
Do not run the 85-case closure.
Do not run all ProductSmoke tests.

Goal151 acceptance is based on:

- actual copied saved project;
- deterministic lifecycle fixture;
- affected product tests;
- current-goal validation;
- transactional/hash regressions.

## Required validation commands

Run once unless a relevant code change requires rerunning the exact affected command:

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal151"
dotnet test ... --filter "FullyQualifiedName~Goal148CProjectIdentityTests"
dotnet test ... --filter "FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronizationTests"
dotnet test ... --filter "FullyQualifiedName~Goal150BZeroValueRuntimeEvidenceTests"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Run the Goal151 real-project-copy runner once before the fix for reproduction and once after the fix for proof.

No full suite.
No 85-case closure.
No all-ProductSmoke sweep.

## Command and investigation budget

```text
read-first: maximum 10 primary files
initial real-copy reproduction: maximum 8 minutes
root-cause investigation before first production edit: maximum 12 minutes
focused build/tests: maximum 15 minutes
post-fix real-copy proof: maximum 10 minutes
total wall clock target: 45 minutes
maximum two testhost processes
```

Rules:

- Do not repeat an unchanged failing command without a new hypothesis or code change.
- Do not spend time repairing unrelated historical snapshots.
- Raw local-project diagnostics remain under ignored `.devflow/runs`.
- No OCR, network or provider calls.
- No manual user action.

## Likely allowed production paths

Add an exact Goal151 artifact-scope scenario.

Initially allowed:

```text
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterizedCompositionService.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionService.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs

.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal151-real-saved-project-build-recovery-and-diagnostic-truth-hotfix.ps1
.devflow/scripts/run-goal151-real-saved-project-build-recovery-and-diagnostic-truth-hotfix.cmd

tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal151RealSavedProjectBuildRecoveryTests.cs
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal151BuildDiagnosticTruthTests.cs
tests/LLMGameCreator.Tests/WinForms/Goal151ProjectsPageDiagnosticTruthTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice.md
docs/manual-acceptance/character-attributes-and-level-progression-featuremodules-vertical-slice.md

docs/agent-tasks/goal-151-real-saved-project-build-recovery-and-diagnostic-truth-hotfix/
.llmgc/procedural/goal-151-real-saved-project-build-recovery-and-diagnostic-truth-hotfix/
.llmgc/exports/goal-151-real-saved-project-build-recovery-and-diagnostic-truth-hotfix/
```

If exact reproduction proves another path is required:

1. record the causal failure;
2. add only the exact path;
3. explain why;
4. do not add broad source prefixes.

Runtime/GamePackage public schema changes are not pre-authorized. If the proven root requires them, publish BLOCKED with exact evidence rather than widening silently.

## Compact evidence

Maximum 10 files per root:

```text
goal151-dashboard.json
original-project-immutability-proof.json
binary-provenance-proof.json
pre-fix-real-copy-reproduction.json
root-cause-proof.json
post-fix-real-copy-build-proof.json
diagnostic-truth-proof.json
focused-regression-proof.json
artifact-scope-proof.json
goal151-report.md
```

Never commit:

- copied real project;
- absolute private path lists beyond the already specified source path in the task;
- raw package/authoring contents;
- saves;
- raw logs/TRX.

## Publication

Create exactly one final commit:

```text
GREEN Goal 151 real saved project build recovery and diagnostic truth hotfix
```

or honest `BLOCKED` / `FAILED`.

Codex must push it.

Required final state:

```text
HEAD == origin/main
worktree clean
original project byte-identical
```

## GREEN criteria

```text
real source project found
original project byte-identical
pre-fix failure reproduced OR stale running binary conclusively proven
exact root cause recorded
generic fix implemented
real copied project 3/8/2/12 build GREEN
deterministic existing-scoped lifecycle fixture GREEN
repeat/reopen build GREEN
checkpoint/replay/action binding GREEN
identity preserved
transactional activation GREEN
failure diagnostics non-empty and stage-aware
UI separates last success/current attempt/current config
focused validation GREEN
artifact scope 0 violations
manualGateReady=true
all acceptance flags=false
manualReviewPerformed=false
one commit pushed
```

## Final report

Return exactly `GREEN`, `BLOCKED` or `FAILED`, then include:

- model/reasoning used;
- source project presence;
- original before/after manifest hash;
- running/built executable provenance;
- pre-fix reproduction outcome;
- exact failure stage/root cause;
- exact generic code fix;
- post-fix real-copy result and `3/8/2/12` values;
- lifecycle/reopen/repeat result;
- diagnostic/UI result;
- focused commands;
- historical hashes;
- artifact scope;
- manualGateReady;
- acceptance flags;
- final commit/push/HEAD/worktree;
- confirmation no human acceptance was claimed.
