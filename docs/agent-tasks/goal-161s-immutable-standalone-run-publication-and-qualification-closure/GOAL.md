# Goal 161S — Immutable Standalone Run Publication & Qualification Closure

## Identity

- Task ID: `goal-161s-immutable-standalone-run-publication-and-qualification-closure`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `63158d213693df7898122c8d9e56517a7efc6349`
- Required base message: `BLOCKED Goal 161R short player output path and standalone qualification closure`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Reason: Goal161R proved the payload, path and player execution are GREEN. The sole remaining failure is
post-smoke publication. The current design moves the directory tree that was just executed. The exact
exception was not persisted. This task replaces that fragile operation with immutable versioned run
directories plus an atomic current-pointer commit, preserves exact publication diagnostics and
performs exactly one final qualification smoke. Unity, host, Runtime and product mechanics are not
involved.

## Pre-approval

- The owner approved execution by launching this task.
- Produce a concise internal plan; do not ask for confirmation.
- Do not request manual testing.
- Do not invoke the player until all offline publication tests and regressions pass.
- Do not make a speculative Unity or payload change.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No intermediate commits.
- Codex performs commit and standard push itself.

## Initial worktree

After unpacking, only these untracked files are allowed:

```text
docs/agent-tasks/goal-161s-immutable-standalone-run-publication-and-qualification-closure/GOAL.md
docs/agent-tasks/goal-161s-immutable-standalone-run-publication-and-qualification-closure/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-161s-immutable-standalone-run-publication-and-qualification-closure/README.md
```

Require:

```text
HEAD == origin/main == 63158d213693df7898122c8d9e56517a7efc6349
branch=main
no other tracked/staged/untracked changes
```

Never use reset, stash, merge, rebase or destructive cleanup.

## Budgets

```text
Unity Editor invocation budget: 0
Unity host build budget: 0
cached host key: 6af4d5eb5b42f956110555b58fb4e276
new hidden player invocation budget: exactly 1
corrective player retry budget: 0
visible player launch budget: 0
```

If the one smoke or pointer commit fails, publish BLOCKED with the exact persisted stage/exception.
Do not retry.

## Goal161R independent-audit intake

Commit:

```text
63158d213693df7898122c8d9e56517a7efc6349
```

Verified GREEN facts:

```text
short output root under %LOCALAPPDATA%/LGC/O
project token 876c262253cec7d3 for the qualification fixture
staging folder s-eb3b986fb4f5
fixed g.exe and g_Data names
historical model path 260
historical frames path 261
short staging maximum path 138 / budget 240
payload preflight 13/13
legacy host parser compatibility GREEN
hidden Player invocation exactly 1
Player exit code 0
all five smoke markers
Player.log captured
HostReused=true
HostRebuilt=false
Unity Editor starts=0
22/22 Goal161R tests and focused regressions GREEN
```

The short-path fix is therefore correct.

### Remaining blocker

After the GREEN smoke, `ProjectStandaloneBuildService` calls:

```text
ProjectStandaloneOutputLocationService.Publish()
```

which performs:

```text
current -> backup when present
staging -> current
final validation
delete backup
```

The first real publication returned FAILED and removed staging. The Goal161R fixture asserted GREEN
before writing its capture, so the exact returned publication diagnostic was not persisted.

Independent audit cannot honestly state whether the exception was:

```text
Directory.Move on a recently executed Windows player tree
post-move file availability/locking
post-move validation
backup cleanup
```

No root cause within that set is proven.

The architecture issue is proven: correctness currently depends on relocating a tree after executing
binaries from it. That relocation is unnecessary and creates an avoidable Windows file-lock boundary.

## Goal161S outcome

Successful standalone output becomes an immutable run:

```text
%LOCALAPPDATA%/LGC/O/<project-token>/
  runs/
    r-<attempt-token>/
      g.exe
      g_Data/
      UnityPlayer.dll
      MonoBleedingEdge/
      build-manifest.json
      run-status.json
  current.json
```

Build sequence:

```text
assemble directly into r-<attempt>
validate completeness
validate path budget
payload self-check
hidden smoke in that exact run directory
post-smoke lightweight validation in the same directory
write run-status GREEN
atomically replace current.json pointer
write standalone history
return GREEN
```

The executed directory is never renamed, moved or rewritten after smoke.

## Non-goals

Do not change:

```text
Unity source
host cache
Runtime or Runtime.Abstractions
GamePackage
FeatureModules
generated saves or migration
generated world/history
project identity
payload v2 schemas
```

Do not add the next product Goal.

## Mandatory read-first

Read at most 9 primary files:

```text
ProjectStandaloneBuildService.cs
ProjectStandaloneBuildModels.cs
ProjectStandaloneOutputLocationService.cs
ProjectStandalonePayloadSelfCheckService.cs
Goal161RPublicationRollbackTests.cs
Goal161RQualificationTests.cs
Goal161StandaloneAndPortabilityTests.cs
Goal161R report/dashboard
docs/CURRENT_GENERATOR_STATE.md
```

Before production edits write:

```text
.llmgc/procedural/goal-161s-immutable-standalone-run-publication-and-qualification-closure/architecture-review.json
```

Required resolved sections:

```text
goal161rVerifiedFacts
lostPublicationDiagnostic
immutableRunLayout
currentPointerSchema
pointerAtomicity
postSmokeValidation
failureForensics
priorCurrentPreservation
historyAndLaunchResolution
singleSmokePlan
nonGoals
```

## A. Immutable run location

Refactor `ProjectStandaloneOutputLocationService`.

Create/add typed models:

```text
ProjectStandaloneRunLocation
ProjectStandaloneCurrentPointer
ProjectStandalonePublicationResult
ProjectStandaloneCurrentOutputReadResult
```

### A1. Paths

Production root remains:

```text
%LOCALAPPDATA%/LGC/O
```

Layout:

```text
<root>/<project-token>/runs/r-<attempt-token>
<root>/<project-token>/current.json
```

Safe fixed run names:

```text
r- + first 12 lowercase hex/alphanumeric attempt characters
```

No `current` directory and no post-smoke directory move.

Paths remain confined; reject reparse/path escape.

### A2. Pointer schema

```text
standalone_current_output_v1
ProjectToken
RunDirectoryName
ExecutableRelativePath=g.exe
BuildManifestRelativePath=build-manifest.json
PackageSha256
CompositionPackageSha256
FinalStateHash
HostCacheKey
PayloadSelfCheckSha256
SmokeMarkerSha256
PlayerLogSha256
SmokeExitCode
PublishedAttemptId
```

No absolute paths or timestamps required for identity.

Pointer validation requires:

```text
safe relative run name
run directory under project operational root
g.exe / g_Data / UnityPlayer.dll / MonoBleedingEdge present
build manifest valid
payload preflight GREEN
package/final hashes match pointer
run-status GREEN
```

### A3. Run status

Write only after GREEN smoke and post-smoke validation:

```text
standalone_run_status_v1
Status=GREEN
AttemptId
PackageSha256
FinalStateHash
PayloadSelfCheckPassed
LegacyParserCompatibilityPassed
MaximumPlayerPathLength
PlayerPathBudgetLimit
SmokeExitCode
SmokeMarkersPassed
PlayerLogPresent
HostCacheKey
HostReused
HostRebuilt
```

Write atomically. Do not modify payload/player files.

## B. Publication transaction

Replace directory publication with:

```text
PublishCurrentPointer(run, pointer)
```

Required order:

1. validate immutable run;
2. read and preserve existing `current.json` bytes when present;
3. write pointer to sibling temporary file with `WriteThrough`;
4. reread/validate temporary pointer;
5. atomically replace/move temporary -> `current.json`;
6. reread current pointer and resolve exact run;
7. return typed GREEN publication result.

On any failure:

```text
existing current.json byte-identical
new run remains for forensics with run-status or failed-publication status
temporary pointer removed
exact stage/code/exception returned
```

Do not delete the smoked run on publication failure.

A failed run is not current because the pointer was not changed.

## C. No post-smoke tree mutation

After process exit, prohibited operations on the run tree:

```text
Directory.Move
File.Move of g.exe/g_Data
renaming the run directory
rewriting payload files
rewriting build-manifest
deleting the run before result/evidence capture
```

Allowed:

```text
read-only validation
atomic creation of run-status.json
atomic pointer write outside the run directory
```

Add a behavioral test that opens `g.exe` with restrictive sharing and proves pointer publication still
succeeds because no run-tree relocation is attempted.

## D. Build service integration

`ProjectStandaloneBuildService.Build()`:

```text
ResolveRun(...)
AssembleProjectOutput directly into RunOutputFolder
preflight
smoke
post-smoke validation
write run status
publish pointer
create GREEN result referencing immutable run
write project-local standalone history
return GREEN
```

On preflight/smoke failure:

```text
return exact diagnostic
retain or mark failed run for forensics
current pointer unchanged
no GREEN history
no RC write
```

On pointer failure:

```text
Stage=publish_current_pointer
Diagnostics include exact publication stage/code/exception
run retained
current pointer unchanged
no GREEN history
no RC write
```

### D1. Result fields

Add/adjust:

```text
OutputLocationKind=immutable_short_local_appdata_run
OutputProjectToken
OutputRunDirectoryName
CurrentPointerPath
CurrentPointerSha256
RunStatusPath
PublicationStage
PublicationDiagnostic
PriorSuccessfulOutputPreserved
```

Old history remains readable.

### D2. Launch/open

`LaunchLastBuild()` and `OpenLastBuildFolder()` use the GREEN result's immutable run.

Add:

```text
LoadCurrentOutput(projectFolder, packageId)
```

so a fresh service/controller may resolve validated `current.json` without relying only on in-memory
`LastResult`.

Do not launch during tests.

## E. Retention

Do not delete old GREEN runs in Goal161S.

Orphan/failed runs remain noncurrent and may be recorded as P2 operational cleanup debt.

No correctness depends on directory timestamps.

## F. Exact diagnostics

Update the Goal161 standalone fixture so the returned standalone result is persisted even when not
GREEN.

The one real run must capture:

```text
Status
Stage
Diagnostics
PublicationStage
PublicationDiagnostic
run folder
pointer presence/hash
```

No future post-smoke failure may lose its returned diagnostic.

## G. Path budget

Keep limit 240.

Validate:

```text
run path
payload paths
smoke marker and Player.log
current pointer path
run-status path
```

The real Goal161 qualification must remain <=240.

## H. Required tests

Create at least 20 Goal161S tests; at least 17 behavioral.

1. immutable run path deterministic/safe;
2. different attempts create different runs;
3. current pointer path deterministic per project;
4. pointer has no absolute paths;
5. pointer rejects unsafe run name;
6. pointer resolves exact confined run;
7. GREEN smoke run is never moved during publish;
8. publication succeeds while g.exe is held with restrictive sharing;
9. atomic pointer write creates current;
10. atomic pointer replacement changes current only after validation;
11. injected temp-pointer write failure preserves prior pointer;
12. injected pointer validation failure preserves prior pointer;
13. injected atomic replace failure preserves prior pointer;
14. failed publication retains run for forensics;
15. failed run without pointer is never current;
16. fresh service resolves current output;
17. LaunchLastBuild target is immutable run executable;
18. OpenLastBuildFolder target is immutable run;
19. old Goal161R output/history remains readable;
20. long project path still yields max <=240;
21. preflight failure preserves pointer;
22. smoke failure preserves pointer;
23. RC writes only after pointer publication GREEN;
24. fixture captures failed publication diagnostics;
25. portable project restore does not require operational pointer/run;
26. Goal161R/Q/161 product regressions GREEN;
27. Goal160–157 standalone regressions GREEN;
28. host cache unchanged and Unity starts zero.

## I. One qualification smoke

After offline tests/regressions:

```text
new hidden player invocation: exactly 1
corrective retry: 0
Unity Editor starts: 0
host builds: 0
```

Run the real Goal161 migrated all-selectable fixture.

Require:

```text
payload preflight 13/13
legacy parser compatible
max path <=240
exit 0
all five markers
Player.log
run-status GREEN
current.json atomically published
fresh pointer read resolves exact run
standalone GREEN
RC CURRENT
portable all-selectable passed
portable core-only passed
core-only no false RC readiness
```

Historical player invocation total becomes 4:

```text
Goal161 failed
Goal161Q diagnostic failed
Goal161R smoke GREEN but publication failed
Goal161S qualification
```

## J. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal161S"
dotnet test ... --filter "FullyQualifiedName~Goal161S"
dotnet test ... --filter "FullyQualifiedName~Goal161R"
dotnet test ... --filter "FullyQualifiedName~Goal161Q"
dotnet test ... --filter "FullyQualifiedName~Goal161"
dotnet test ... --filter "FullyQualifiedName~Goal160"
dotnet test ... --filter "FullyQualifiedName~Goal159"
dotnet test ... --filter "FullyQualifiedName~Goal158"
dotnet test ... --filter "FullyQualifiedName~Goal157"
dotnet test ... --filter "FullyQualifiedName~ProjectStandaloneBuild"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~RuntimeSnapshotStore"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Do not run full suite, 85-case closure, all-ProductSmoke, Unity host build or more than one player.

## K. Evidence

Create exactly 9 files in each mirrored root:

```text
goal161s-dashboard.json
architecture-review.json
goal161r-independent-audit-intake.json
immutable-run-layout-proof.json
atomic-pointer-publication-proof.json
publication-failure-forensics-proof.json
single-smoke-rc-portability-proof.json
artifact-scope-proof.json
goal161s-report.md
```

Roots:

```text
.llmgc/procedural/goal-161s-immutable-standalone-run-publication-and-qualification-closure/
.llmgc/exports/goal-161s-immutable-standalone-run-publication-and-qualification-closure/
```

Twins byte-identical.

Dashboard fields:

```text
status
goal161sTestsDiscovered
goal161sBehavioralTestsPassed
goal161rSmokeAccepted
goal161rPublicationFailureRecorded
immutableRunLayoutPassed
runTreeMovedAfterSmoke=false
restrictiveExeHandlePublicationPassed
currentPointerAtomicityPassed
priorPointerPreservationPassed
failedRunForensicsPassed
publicationDiagnosticPersistencePassed
outputProjectToken
outputRunDirectoryName
maximumPlayerPathLength
playerPathBudgetLimit
hostCacheKey
hostReused
hostRebuilt
unityEditorProcessStartCount
newHiddenSmokeInvocationCount
newHiddenSmokePassed
smokeExitCode
smokeMarkersPassed
playerLogPresent
runStatusGreen
currentPointerPublished
currentPointerValidated
standaloneGreen
releaseCandidateRecordCurrent
portableAllSelectablePassed
portableCoreOnlyPassed
coreOnlyNoFalseRcReady
goal161rRegressionPassed
goal161qRegressionPassed
goal161RegressionPassed
goal160RegressionPassed
goal159RegressionPassed
goal158RegressionPassed
goal157RegressionPassed
artifactScopeViolationCount
goal160AuditBlockerClosed
goal161QualificationStatus
goal161Accepted=false
goal161sAccepted=false
```

## L. State/docs

Update current state/queue/milestone/risk/debt and Goal161/R acceptance docs.

Create:

```text
docs/manual-acceptance/goal161s-immutable-standalone-publication-closure.md
```

On GREEN:

```text
goal160AuditBlocker=closed_by_goal161s
goal160IndependentAuditRequired=false

goal161ImplementationStatus=GREEN
goal161CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal161QualificationStatus=GREEN
goal161Accepted=false
goal161IndependentAuditRequired=true

goal161qDiagnosticInfrastructurePassed=true
goal161rShortPathSmokePassed=true
goal161rPublicationFailure=closed_by_goal161s

goal161sImplementationStatus=GREEN
goal161sCandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal161sAccepted=false
goal161sManualReviewRequired=false
goal161sIndependentAuditRequired=true

goal161StandaloneOutputLocationKind=immutable_short_local_appdata_run
goal161StandaloneCurrentPointerPassed=true
goal161HistoricalHiddenSmokeInvocationCount=4
goal161sNewHiddenSmokeInvocationCount=1
goal161HostReused=true
goal161HostRebuilt=false
goal161UnityEditorProcessStartCount=0
goal161ReleaseCandidateCurrent=true
goal161PortableAllSelectablePassed=true
goal161PortableCoreOnlyPassed=true

nextAction=independent_goal161s_audit_and_plan_next_major_product_vertical_slice
```

No human gate.

## M. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal161s-immutable-publication-closure.ps1
.devflow/scripts/run-goal161s-immutable-publication-closure.cmd

src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildModels.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildService.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneOutputLocationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs

tests/LLMGameCreator.Tests/Application/Goal161S/Goal161SRunLocationTests.cs
tests/LLMGameCreator.Tests/Application/Goal161S/Goal161SPointerPublicationTests.cs
tests/LLMGameCreator.Tests/Application/Goal161S/Goal161SFailureForensicsTests.cs
tests/LLMGameCreator.Tests/Application/Goal161S/Goal161SQualificationTests.cs
tests/LLMGameCreator.Tests/Application/Goal161/Goal161StandaloneAndPortabilityTests.cs
tests/LLMGameCreator.Tests/Application/Goal161R/Goal161RPublicationRollbackTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/goal161-generated-gameplay-save-migration.md
docs/manual-acceptance/goal161r-short-output-qualification-closure.md
docs/manual-acceptance/goal161s-immutable-standalone-publication-closure.md

docs/agent-tasks/goal-161s-immutable-standalone-run-publication-and-qualification-closure/
.llmgc/procedural/goal-161s-immutable-standalone-run-publication-and-qualification-closure/
.llmgc/exports/goal-161s-immutable-standalone-run-publication-and-qualification-closure/
```

Forbidden:

```text
all Unity paths
Runtime/Runtime.Abstractions
GamePackage
FeatureModule catalog
generated save/migration/source/history implementation
```

## N. Command budget

```text
read-first/architecture: 6 minutes
immutable run/pointer: 12 minutes
build integration/diagnostics: 10 minutes
tests: 12 minutes
regressions: 8 minutes
one smoke: 8 minutes
evidence/docs/scope: 8 minutes
target: 55 minutes
maximum two testhost
```

## O. Publication

Exactly one final GREEN/BLOCKED/FAILED commit and standard push.

GREEN requires:

```text
no post-smoke run-tree move
atomic validated current pointer
one new smoke GREEN
standalone GREEN
RC CURRENT
portable all/core passed
Goal160 blocker closed
9+9 evidence
artifact scope 0
HEAD==origin/main and clean
```
