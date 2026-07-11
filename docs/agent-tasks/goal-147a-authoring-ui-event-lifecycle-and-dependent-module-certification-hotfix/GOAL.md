# Goal 147A — Authoring UI Event Lifecycle + Dependent-Module Certification Hotfix

## Identity

- Task ID: `goal-147a-authoring-ui-event-lifecycle-and-dependent-module-certification-hotfix`
- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: commit `43e0d4bc6cb9b189bbe96c3108bdaa448e7d3996` or a direct descendant

## Fresh-dialog rule

This task runs in a new Codex dialog. This GOAL.md is the complete instruction source. Do not rely on memory from other chats.

## Status

```text
Goal145 accepted=true
Goal146 GREEN, accepted=false, manualReviewDeferred=true
Goal146A GREEN
Goal147 GREEN, accepted=false
Goal141 accepted=false
```

Do not start Goal148. Do not mark Goal146 or Goal147 accepted.

## P1 A — Goal147 CheckedListBox event lifecycle is not programmatically silent

Current production code always queues a `BeginInvoke` callback from `CheckedListBox.ItemCheck`. `BindGoal147All()` sets `_goal147Binding=true`, clears/re-adds checked items, and then sets the flag false. The queued callbacks run later, when the guard is already false.

This can cause:

```text
programmatic binding → delayed user-selection callback
Refresh Library while Document=null → delayed SetSelectedModules throws
Delete → Document=null → rebind → delayed callback throws
stale queued callback can act on a later document
multiple callbacks for one rebind
```

Existing tests exercise the controller directly, not the real page/control message lifecycle.

### Required correction

Eliminate delayed binding callbacks.

Preferred production event model:

```text
ItemCheck handler
→ synchronously return when _goal147Binding=true
→ synchronously return when controller.Document=null
→ compute post-event checked IDs from ItemCheckEventArgs.Index/NewValue
→ SetSelectedModules exactly once
→ rebuild parameters/status exactly once
```

Do not use `BeginInvoke` merely to wait for `CheckedItems`.

Required behavior:

```text
programmatic Items.Add(..., checked) applied callbacks=0
programmatic clear/rebind applied callbacks=0
programmatic rebind dirty transitions=0
programmatic rebind materialization invocations=0
Refresh Library with Document=null=no exception
Delete then rebind with Document=null=no exception
operator check change applied exactly once
operator check change uses post-event state
no queued callback can mutate a later document
```

No module-ID-specific UI branches.

## P1 B — heavy primary actions run on the UI thread

`Goal147RunAsync` currently yields once and then executes a synchronous `Func<string>` on the WinForms UI thread. `Materialize & Qualify` and `Save, Materialize & Qualify` perform package materialization, Runtime qualification, save/replay and certification and may freeze the window.

Required:

- capture UI values on the UI thread;
- run heavy Application services through `Task.Run` or an injected in-process async executor;
- marshal results back to UI thread;
- keep controls disabled while running;
- show running status before work begins;
- prevent a second concurrent operation;
- exceptions appear in diagnostics;
- no compiler/test/PowerShell child process.

At minimum the heavy body of these actions must run off the UI thread:

```text
Materialize & Qualify
Save, Materialize & Qualify
```

## P1 C — optional dependency closure is ignored during module certification

The persistent module contract allows dependencies on other optional modules. The current certification service certifies each optional module with only `[item.ModuleId]`. A future optional module that depends on another optional module will fail certification even though the library accepts the dependency.

The cache dependency fingerprint is also flat/direct and does not safely represent transitive dependency closure.

### Required dependency-aware certification

For each optional module ledger entry compute a deterministic transitive optional dependency closure.

Required plan fields:

```text
moduleId
certificationSelectedModuleIds[]
optionalDependencyClosureIds[]
dependencyClosureFingerprint
moduleFingerprint
basePackageSha256
runtimeQualifierContractVersion
actionPlanSignature
parameterDefaultsFingerprint
cacheKey
```

Rules:

1. One ledger entry remains per optional module.
2. `certificationSelectedModuleIds` contains target + all transitive optional dependencies, sorted.
3. Required core modules remain implicit through Composer.
4. `ComposeAndQualify` receives the dependency closure, not only the target ID.
5. Target certification requires the closure composition to pass and the target module's own required effect contracts to pass.
6. Cache key includes closure IDs and fingerprints of all transitive dependencies.
7. Dependency change invalidates itself and all transitive dependents.
8. Unrelated entries remain reusable.
9. Unknown dependency and dependency cycles are rejected deterministically.
10. No current module IDs may be hardcoded.

## Required executable dependency test

Create a synthetic test catalog:

```text
feature.synthetic.base_optional
feature.synthetic.dependent_optional
  depends on feature.synthetic.base_optional
feature.synthetic.unrelated_optional
```

Use supported non-conflicting real mutation operations and runtime-effect contracts.

Prove:

```text
ledger entries=3
initial executed=3 and GREEN
second run reused=3
dependent certificationSelectedModuleIds contains base+dependent
change base module
→ base executed
→ dependent executed
→ unrelated reused
executed=2, reused=1
corrupt dependent cache regenerates dependent
dependency cycle rejected before Runtime execution
```

Boolean constants are insufficient.

## Required real WinForms STA tests

Instantiate the production `VisualWorldStreamPreviewWorkspacePageControl` on an STA thread and pump messages.

### Refresh before document

```text
invoke Refresh Library
Document remains null
optional list populated
pump messages
no exception
no dirty transition
no materialization
```

### New and programmatic rebind

```text
New Composition
dirtyTransitionCount=1
BindGoal147All
pump messages
dirtyTransitionCount remains 1
materializationInvocationCount remains 0
```

### User check change

```text
uncheck one optional module
SetSelectedModules applied exactly once
document IDs match post-event UI state
dirty increments once
no materialization
```

### Delete and rebind

```text
save composition
delete
Document=null
BindGoal147All
pump messages
no exception or missing-document callback
```

### Background work

Use an injected bounded executor/synchronization probe to prove:

```text
heavy body runs on non-UI thread
UI processes a posted callback while work is pending
controls disabled during work
controls restored on success/failure
no child process
```

Do not run an expensive full proof inside the STA unit test.

## Preserve existing GREEN behavior

Preserve exactly:

```text
10 core + 3 optional file-based modules
8 typed parameter definitions
all Goal146 package/final hashes
custom package SHA=2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991
custom final hash=80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e
saved create/save/load/list/clone/delete/save-as
atomic write and staleness detection
100-module certification plan
interaction rows<=24, no powerset
multi-effect accounting
shared Runtime qualifier
checkpoint/full replay/action binding
Unity read-only smoke
```

Do not rewrite historical Goal146/146A/147 evidence roots.

## Required Goal147A artifacts

Write under both:

```text
.llmgc/procedural/goal-147a-authoring-ui-event-lifecycle-and-dependent-module-certification-hotfix/
.llmgc/exports/goal-147a-authoring-ui-event-lifecycle-and-dependent-module-certification-hotfix/
```

Required:

```text
goal147a-hotfix-dashboard.json
goal147-authoring-ui-event-lifecycle-proof.json
dependent-module-certification-proof.json
goal147-regression-compatibility-proof.json
goal147a-negative-proof.json
goal147a-file-index.json
goal147a-report.md
```

Dashboard:

```text
status=GREEN
programmaticItemCheckAppliedCount=0
refreshWithoutDocumentPassed=true
deleteRebindWithoutDocumentPassed=true
operatorItemCheckAppliedCount=1
operatorItemCheckUsesPostEventState=true
programmaticRebindDirtyTransitionCount=0
programmaticRebindMaterializationCount=0
heavyWorkRunsOffUiThread=true
uiRemainsPumpResponsiveDuringHeavyWork=true
controlsDisabledWhileHeavyWorkRuns=true
dependentModuleCertificationPassed=true
transitiveDependencyClosurePassed=true
dependencyChangeExecutedCount=2
dependencyChangeReusedCount=1
dependencyCycleRejected=true
goal147RegressionGreen=true
goal146RegressionGreen=true
unitySmokeStillGreen=true
goal146Accepted=false
goal147Accepted=false
accepted=false
```

File index includes SHA-256.

## State updates

```text
current_phase_title=Goal 147A authoring UI event lifecycle and dependent-module certification hotfix
goal146Accepted=false
goal147Accepted=false
goal147AuthoringItemCheckLifecycleFixed=true
goal147ProgrammaticItemCheckAppliedCount=0
goal147RefreshWithoutDocumentPassed=true
goal147DeleteRebindWithoutDocumentPassed=true
goal147HeavyWorkRunsOffUiThread=true
goal147DependentModuleCertificationPassed=true
goal147TransitiveDependencyInvalidationPassed=true
nextProductGoal=review_goals_146_147_featuremodule_composer_authoring_workflow
```

Do not mark Goal141 accepted.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/NARROW_ALPHA_EXPANSION_POLICY.md
docs/AUTOMATED_VALIDATION_TIERS.md

docs/agent-tasks/goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix/GOAL.md
docs/agent-tasks/goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix/GOAL.md
docs/agent-tasks/goal-147-persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification/GOAL.md

src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal147.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleAuthoringWorkbenchController.cs
src/LLMGameCreator.Application/Design/FeatureModuleCertification/**
tests/LLMGameCreator.Tests/WinForms/Goal147FeatureModuleAuthoringBindingTests.cs
tests/LLMGameCreator.Tests/Application/FeatureModuleCertification/FeatureModuleCertificationAndCoverageTests.cs
```

## Allowed paths

```text
docs/agent-tasks/goal-147a-authoring-ui-event-lifecycle-and-dependent-module-certification-hotfix/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal147a-authoring-and-certification-hotfix.ps1
.devflow/scripts/run-goal147a-authoring-and-certification-hotfix.cmd

.llmgc/procedural/goal-147a-authoring-ui-event-lifecycle-and-dependent-module-certification-hotfix/**
.llmgc/exports/goal-147a-authoring-ui-event-lifecycle-and-dependent-module-certification-hotfix/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix.md
docs/manual-acceptance/persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification.md

src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal147.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleAuthoringWorkbenchController.cs
src/LLMGameCreator.Application/Design/FeatureModuleCertification/**

tests/LLMGameCreator.Tests/WinForms/Goal147FeatureModuleAuthoringBindingTests.cs
tests/LLMGameCreator.Tests/Application/FeatureModuleCertification/FeatureModuleCertificationAndCoverageTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGoal147AAuthoringAndCertificationHotfixScriptTests.cs
```

A small new WinForms Goal147 coordination helper file is allowed.

## Forbidden paths

```text
.llmgc/manual/**
.llmgc/workspace/**
catalogs/feature-modules/**
samples/minimal-map-game/**

.llmgc/procedural/goal-142*/**
.llmgc/exports/goal-142*/**
.llmgc/procedural/goal-143*/**
.llmgc/exports/goal-143*/**
.llmgc/procedural/goal-144*/**
.llmgc/exports/goal-144*/**
.llmgc/procedural/goal-145*/**
.llmgc/exports/goal-145*/**
.llmgc/procedural/goal-146*/**
.llmgc/exports/goal-146*/**
.llmgc/procedural/goal-147-persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification/**
.llmgc/exports/goal-147-persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification/**

src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Generation/**
src/LLMGameCreator.AssetPipeline/**
src/LLMGameCreator.Scripting/**
generator-library/**
provider/**
LLM/**
RAG/**
unity/**

*.sln
*.csproj
Directory.Build.*
dependency/package files
```

No public schema, Runtime gameplay, Unity or module-catalog changes. No new dependency.

## Validation

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore

dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~Goal147A|FullyQualifiedName~Goal147FeatureModuleAuthoringBinding|FullyQualifiedName~FeatureModuleCertification|FullyQualifiedName~FeatureModuleAuthoring|FullyQualifiedName~FeatureModuleComposition"

.\.devflow\scripts\run-featuremodule-authoring-persistence-and-certification.ps1 -DryRun
.\.devflow\scripts\run-featuremodule-composer-scalability-hotfix.ps1 -DryRun
.\.devflow\scripts\run-featuremodule-composition-runtime-matrix.ps1 -DryRun
.\.devflow\scripts\run-product-line-interactive-session-matrix.ps1 -DryRun

.\.devflow\scripts\run-goal147a-authoring-and-certification-hotfix.ps1 -DryRun
.\.devflow\scripts\run-goal147a-authoring-and-certification-hotfix.ps1 -ApplyCleanup

.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-147a-authoring-ui-event-lifecycle-and-dependent-module-certification-hotfix
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun

git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual .llmgc/workspace
```

Required build: 0 warnings/errors.
Changed-text mojibake and escaped Cyrillic: zero.
Forbidden diff: empty.
Restore validation churn only by exact policy-derived paths.

Never use `git reset --hard`, `git clean`, broad restore, branch switching, merge, rebase or cherry-pick.

## Publication

Before staging:

```text
real STA UI lifecycle tests GREEN
Refresh/Delete no-document paths GREEN
heavy work off UI thread GREEN
dependency-closure certification GREEN
transitive invalidation GREEN
Goal146/147 regressions GREEN
historical artifacts unchanged
Goal146/147 accepted=false
scope clean
forbidden diff empty
```

Commit:

```text
GREEN Goal 147A authoring UI event lifecycle and dependent-module certification hotfix
```

Push `origin/main`.

Final report must include commit SHA, UI event model/callback counts, Refresh/Delete results, heavy-work thread proof, dependency closure, invalidation executed/reused counts, cycle rejection, regression/test/scope results and clean `HEAD == origin/main`.
