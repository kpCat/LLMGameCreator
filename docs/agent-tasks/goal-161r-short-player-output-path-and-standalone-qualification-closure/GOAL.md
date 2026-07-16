# Goal 161R — Short Player Output Path & Standalone Qualification Closure

## Identity

- Task ID: `goal-161r-short-player-output-path-and-standalone-qualification-closure`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `8219cd1bd2a99c7ff7232f502be143c56eb95e7f`
- Required base message: `BLOCKED Goal 161Q standalone self-check diagnosis and qualification closure`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Reason: the blocker is now exact and bounded. Goal161Q proved that the payload is structurally correct
and the cached host parser is compatible. The Unity player cannot read payload files whose absolute
paths reach the legacy Windows 260-character boundary. This task replaces the project-nested
standalone output with a short operational output root, installs a hard path-budget preflight and
performs exactly one final qualification smoke. No Unity or gameplay architecture change is needed.

## Pre-approval

- The owner approved execution by launching this task.
- Produce a concise internal plan; do not ask for confirmation.
- Do not request manual testing.
- Do not invoke the player until all path-layout tests and focused regressions pass.
- Do not modify Unity source.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No intermediate commits.
- Codex performs commit and standard push itself.

## Initial worktree

After unpacking, only these untracked files are permitted:

```text
docs/agent-tasks/goal-161r-short-player-output-path-and-standalone-qualification-closure/GOAL.md
docs/agent-tasks/goal-161r-short-player-output-path-and-standalone-qualification-closure/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-161r-short-player-output-path-and-standalone-qualification-closure/README.md
```

Require:

```text
HEAD == origin/main == 8219cd1bd2a99c7ff7232f502be143c56eb95e7f
branch=main
tracked diff count=0
staged diff count=0
unknown dirty/untracked count=0
```

Any other dirt blocks execution. Never use reset, stash, merge, rebase or destructive cleanup.

## Execution budget

```text
Unity Editor invocation budget: 0
Unity host build budget: 0
cached host key: 6af4d5eb5b42f956110555b58fb4e276
new hidden player invocation budget: exactly 1
corrective player retry budget: 0
visible launch budget: 0
```

If the one smoke fails, publish BLOCKED with the exact named preflight/smoke/Player.log diagnostic.
Do not retry.

## Goal161Q audited intake

Commit:

```text
8219cd1bd2a99c7ff7232f502be143c56eb95e7f
```

Goal161Q correctly added:

```text
Application-side 13-check payload preflight
legacy cached-host parser compatibility check
process suppression on preflight failure
separate confined Player.log
typed exit code / marker / named failure diagnostics
preservation of prior successful output when staging preflight fails
24/24 Goal161Q tests
```

The recovered failed Goal161 payload passed:

```text
13/13 structural checks
5/5 frames through exact legacy parser compatibility
62/62 human facts through exact legacy parser compatibility
game-package SHA
```

No payload check failed.

### Proven root cause

```text
standalone.player.payload_path_unreadable
```

Player.log:

```text
Could not find a part of the path <payload>/player-adapter-model.json
```

Measured paths:

```text
project-manifest.json: 256 characters and readable
player-adapter-model.json: 260 characters and unreadable
player-adapter-frames.json: 261 characters
```

Classification:

```text
application_launch_path
```

Required fix from Goal161Q evidence:

```text
short_confined_application_smoke_path
```

Unity parser/bootstrap defect was explicitly not proven, and Unity source remained unchanged.

## Why the existing layout is defective

Current `ProjectStandaloneBuildService.AssembleProjectOutput()` uses:

```text
<project folder>/Builds/Windows/<package slug>/
```

and the player reads:

```text
<slug>_Data/StreamingAssets/LLMGameCreatorProject/player-adapter-model.json
```

A project created under a long temporary, copied or user-selected folder can therefore produce a
valid standalone payload that the cached Unity/Mono player cannot read.

A smoke-only short copy would not be sufficient: the user-launched published executable would still
fail from the long project-local path.

## Goal161R product outcome

The authoritative Windows standalone output becomes a short operational artifact:

```text
%LOCALAPPDATA%/LGC/O/<project-token>/current/
  game.exe
  game_Data/
  UnityPlayer.dll
  MonoBleedingEdge/
  build-manifest.json
```

The project remains the source of truth; the output is a rebuildable machine-local artifact.

Required:

```text
normal BuildWindowsStandalone returns the short output path
LaunchLastBuild launches that exact executable
OpenLastBuildFolder opens that exact folder
standalone history records the exact result
RC record remains project-local and portable
project copy does not depend on the old machine output
```

Old project-local `Builds/Windows` outputs are historical operational files:

```text
do not migrate
do not delete
do not treat as current after a new successful build
```

## Non-goals

Do not change:

```text
Unity bootstrap or other Unity files
Runtime or Runtime.Abstractions
GamePackage schema
FeatureModules
generated saves or migration policy
generated source/world history
project identity
host cache key
installer/clean-machine packaging
```


## Mandatory architecture review

Read at most 10 primary files:

```text
ProjectStandaloneBuildService.cs
ProjectStandaloneBuildModels.cs
ProjectStandalonePayloadSelfCheckService.cs
UnifiedGameProjectWorkspaceController.cs
Goal161StandaloneAndPortabilityTests.cs
Goal161Q tests
Goal161Q named-root-cause-proof.json
Goal161Q single-smoke-proof.json
docs/UNITY_EXECUTION_POLICY.md
docs/CURRENT_GENERATOR_STATE.md
```

Before production edits write:

```text
.llmgc/procedural/goal-161r-short-player-output-path-and-standalone-qualification-closure/architecture-review.json
```

Required resolved sections:

```text
provenPathFailure
shortOutputRoot
projectToken
stagingAndFinalPaths
pathBudget
smokeBeforePublish
priorOutputPreservation
standaloneHistory
rcAndPortability
singleSmokePlan
nonGoals
```

Every section must name exact types, paths, ordering and behavioral tests.

## A. Short output location service

Create:

```text
ProjectStandaloneOutputLocationService
ProjectStandaloneOutputLocation
```

Production default root:

```text
%LOCALAPPDATA%/LGC/O
```

The service may accept an optional root override through constructor injection for tests. Do not use a
global environment variable as production truth.

### A1. Project token

Derive a lowercase hexadecimal token from:

```text
normalized full project folder path
project package ID
```

Use SHA-256 and at least 16 hex characters.

Properties:

```text
same project/package -> same token
same package ID in different project folders -> different token
portable project copy -> a different operational output token is allowed
token contains only lowercase hex
```

The output token is operational and does not participate in package, RC or project identity hashes.

### A2. File names

Use short fixed operational names:

```text
executable: g.exe
data directory: g_Data
final directory: current
staging directory: s-<short attempt token>
backup directory: b-<short attempt token>
```

Project title/package ID remain in payload metadata, not filesystem names.

Do not hardcode product content counts.

### A3. Paths

```text
root/<project-token>/current
root/<project-token>/s-<attempt-token>
root/<project-token>/b-<attempt-token>
```

All paths must remain confined under the configured output root.

Reject escape/symlink-style traversal inputs causally:

```text
standalone.output.path_escape
```

## B. Player path budget

Add typed fields:

```text
ProjectStandaloneOutputPathBudgetResult
MaximumAbsolutePathLength
LongestRelativePath
BudgetLimit
Passed
Diagnostics[]
```

Safety limit:

```text
240 characters
```

Reason: the verified player failed at the legacy boundary of 260; 240 preserves operational margin
for system/launcher behavior.

Check at minimum:

```text
g.exe
g_Data
UnityPlayer.dll
MonoBleedingEdge
build-manifest.json
all files under g_Data/StreamingAssets/LLMGameCreatorProject
Player.log and smoke-marker paths
```

The required payload files must each be below or equal to the budget.

Diagnostic:

```text
standalone.output.player_path_budget_exceeded:<relative-path>:<length>
```

### B1. Preflight ordering

Path-budget validation runs:

```text
after staging assembly
before payload structural self-check
before process start
```

A path-budget failure:

```text
does not start the player
does not publish staging
does not replace prior final output
returns Stage=output_path_budget
```

### B2. Result fields

Extend `ProjectStandaloneBuildResult` additively:

```text
OutputLocationKind=short_local_appdata
OutputProjectToken
MaximumPlayerPathLength
PlayerPathBudgetLimit
PlayerPathBudgetPassed
PriorSuccessfulOutputPreserved
```

Old standalone history JSON remains readable.

## C. Smoke before publish

Change the build order to:

```text
qualify project
resolve/reuse host
create payload
assemble short staging output
validate output completeness
validate path budget
run Application payload self-check
run hidden smoke against staging executable
only after GREEN smoke:
  atomically publish staging -> current
  write standalone history
  return GREEN
```

Do not publish before smoke.

### C1. Atomic publication

On GREEN:

```text
move current -> backup when present
move staging -> current
delete backup only after final output validation
```

On publication failure:

```text
remove incomplete current
restore backup
remove staging
```

After final move, re-run lightweight:

```text
output completeness
path budget
payload self-check
```

No second player invocation.

### C2. Smoke/preflight failure

On path, payload or smoke failure:

```text
delete staging
leave current byte-identical
return PriorSuccessfulOutputPreserved=true when current existed
do not write standalone GREEN history
do not write RC record
```

The failed result may reference the diagnostic logs, but must not claim its deleted staging output as
launchable.

## D. Short smoke logs

Keep marker and Player.log under the existing short root:

```text
%LOCALAPPDATA%/LLMGameCreator/S
```

Validate those paths under the same 240-character budget.

Preserve Goal161Q typed diagnostics.

## E. No project-local output dependency

After a successful build:

```text
new output is outside the project folder
project-local Builds/Windows is untouched by Goal161R
BuildManifestPath points into the short final output
LaunchLastBuild/OpenLastBuildFolder use the short final output
```

Project-local standalone history may contain the machine-local output path as operational metadata.

RC and generated project portability must not require that path or output folder to exist.

### E1. Reopen behavior

Do not broaden scope into a new output registry UI.

At minimum:

```text
the current controller instance launches/opens the new output
standalone history records the output
a portable copied project restores RC/save truth without accessing the machine-local output
```

## F. Exact long-path regression

Create a disposable generated project whose folder/path reproduces or exceeds the Goal161 failing
project prefix.

Require:

```text
old project-local payload path would be >=260
new short staging payload maximum <=240
new final payload maximum <=240
payload bytes/hashes equal the request
host cache unchanged
```

Do not launch during this regression. It is an offline proof.

## G. Historical compatibility

Required:

```text
existing project-local outputs remain untouched
existing standalone history deserializes
prior successful short output is atomically replaced on GREEN
prior successful short output survives path/preflight/smoke failure
same project repeat build uses same final directory
different project copy uses a different token
```


## H. Required behavioral tests

Create at least 22 Goal161R tests; at least 18 behavioral.

1. default output root is `%LOCALAPPDATA%/LGC/O`;
2. output token is deterministic for same project/package;
3. different project paths produce different tokens;
4. token/path segments are safe and confined;
5. staging/final executable uses short fixed names;
6. reproduced old project-local model path is >=260;
7. new staging maximum path is <=240;
8. new final maximum path is <=240;
9. path-budget failure prevents process invocation;
10. path-budget failure preserves prior output;
11. payload preflight failure preserves prior output;
12. smoke failure preserves prior output;
13. GREEN smoke publishes only afterward;
14. atomic publication restores backup on injected publish failure;
15. final output revalidation passes without a second smoke;
16. repeat GREEN build replaces the same final directory deterministically;
17. old project-local output remains untouched;
18. old standalone history is readable;
19. result contains path-budget/location fields;
20. LaunchLastBuild targets short final executable;
21. OpenLastBuildFolder targets short final folder;
22. RC record is written only after GREEN smoke/publication;
23. portable all-selectable project restores save/travel/RC truth without output folder;
24. portable core-only restores save truth without false RC readiness;
25. Goal161Q payload diagnostics regressions remain GREEN;
26. Goal161 product/profile-neutral/save regressions remain GREEN;
27. Goal160/159/158/157 standalone regressions remain GREEN;
28. host cache key/file set unchanged and Unity starts zero.

Use real filesystem assemblies/services. Source-string tests do not count as behavioral.

## I. One qualification smoke

After offline tests and focused regressions:

1. ensure Goal161Q player ledger does not authorize retries;
2. create a new Goal161R ledger;
3. confirm no Unity process;
4. confirm cache `6af4d5eb5b42f956110555b58fb4e276` complete;
5. build the real Goal161 all-selectable migrated project;
6. invoke exactly one hidden player smoke from the short staging path;
7. no retry.

Require on GREEN:

```text
payload preflight 13/13
legacy parser compatibility true
staging/final max player path <=240
player exit code 0
all five smoke markers
Player.log captured
HostReused=true
HostRebuilt=false
Unity Editor starts=0
final output location kind=short_local_appdata
RC record CURRENT
portable all-selectable passed
portable core-only passed
core-only no false RC READY
prior project-local failed outputs untouched
```

Historical total:

```text
Goal161 failed smoke: 1
Goal161Q failed diagnostic smoke: 1
Goal161R new qualification smoke: 1
total historical player invocations: 3
```

## J. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal161R"
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

Do not run:

```text
full suite
85-case historical closure
all-ProductSmoke
Unity host build
more than one new player invocation
corrective smoke retry
visible automatic launch
```

A zero-match filter is failure.

## K. Evidence

Create exactly 9 files in each mirrored root:

```text
goal161r-dashboard.json
architecture-review.json
goal161q-independent-audit-intake.json
short-output-layout-proof.json
path-budget-proof.json
smoke-before-publish-proof.json
single-smoke-rc-portability-proof.json
artifact-scope-proof.json
goal161r-report.md
```

Roots:

```text
.llmgc/procedural/goal-161r-short-player-output-path-and-standalone-qualification-closure/
.llmgc/exports/goal-161r-short-player-output-path-and-standalone-qualification-closure/
```

Twins byte-identical.

### Dashboard fields

```text
status
candidateStatus
goal161rTestsDiscovered
goal161rBehavioralTestsPassed

goal161qRootCauseAccepted
namedRootCause=standalone.player.payload_path_unreadable
historicalModelPathLength=260
historicalFramesPathLength=261

outputLocationKind
outputProjectToken
shortOutputRootKind
oldProjectLocalWouldExceedLegacyBoundary
stagingMaximumPlayerPathLength
finalMaximumPlayerPathLength
playerPathBudgetLimit
playerPathBudgetPassed

smokeBeforePublishPassed
priorOutputPreservedOnPreflightFailure
priorOutputPreservedOnSmokeFailure
atomicPublishPassed
oldProjectLocalOutputUntouched

hostCacheKey
hostReused
hostRebuilt
hostFileSetHashUnchanged
unityEditorProcessStartCount
newHiddenSmokeInvocationCount
newHiddenSmokePassed
smokeExitCode
smokeMarkersPassed
playerLogPresent
payloadSelfCheckPassed
legacyHostParserCompatibilityPassed

releaseCandidateRecordCurrent
portableAllSelectablePassed
portableCoreOnlyPassed
coreOnlyNoFalseRcReady

goal161qRegressionPassed
goal161RegressionPassed
goal160RegressionPassed
goal159RegressionPassed
goal158RegressionPassed
goal157RegressionPassed

goal142SourceByteIdentical
sourceGoal148ByteIdentical
artifactScopeViolationCount
goal160AuditBlockerClosed
goal161QualificationStatus
goal161Accepted=false
goal161rAccepted=false
```

No GREEN-required field may be null/PARTIAL/NOT_EXECUTED.

## L. State and docs

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/goal161-generated-gameplay-save-migration.md
docs/manual-acceptance/goal161q-standalone-qualification-closure.md
```

Create:

```text
docs/manual-acceptance/goal161r-short-output-qualification-closure.md
```

On GREEN:

```text
goal160AuditBlocker=closed_by_goal161r
goal160IndependentAuditRequired=false

goal161ImplementationStatus=GREEN
goal161CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal161QualificationStatus=GREEN
goal161Accepted=false
goal161ManualReviewRequired=false
goal161ManualGateReady=false
goal161IndependentAuditRequired=true

goal161qImplementationStatus=BLOCKED
goal161qNamedRootCause=standalone.player.payload_path_unreadable
goal161qDiagnosticInfrastructurePassed=true
goal161qIndependentAuditRequired=false

goal161rImplementationStatus=GREEN
goal161rCandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal161rAccepted=false
goal161rManualReviewRequired=false
goal161rIndependentAuditRequired=true

goal161StandaloneOutputLocationKind=short_local_appdata
goal161StandalonePlayerPathBudget=240
goal161StandalonePlayerPathBudgetPassed=true
goal161HistoricalHiddenSmokeInvocationCount=3
goal161rNewHiddenSmokeInvocationCount=1
goal161HostReused=true
goal161HostRebuilt=false
goal161UnityEditorProcessStartCount=0
goal161ReleaseCandidateCurrent=true
goal161PortableAllSelectablePassed=true
goal161PortableCoreOnlyPassed=true

nextAction=independent_goal161r_audit_and_plan_next_major_product_vertical_slice
```

Preserve both historical failed smoke records.

No human gate.

## M. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal161r-short-output-closure.ps1
.devflow/scripts/run-goal161r-short-output-closure.cmd

src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildModels.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildService.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneOutputLocationService.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandalonePayloadSelfCheckService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs

tests/LLMGameCreator.Tests/Application/Goal161R/Goal161ROutputLocationTests.cs
tests/LLMGameCreator.Tests/Application/Goal161R/Goal161RPathBudgetTests.cs
tests/LLMGameCreator.Tests/Application/Goal161R/Goal161RPublicationRollbackTests.cs
tests/LLMGameCreator.Tests/Application/Goal161R/Goal161RQualificationTests.cs
tests/LLMGameCreator.Tests/Application/Goal161/Goal161StandaloneAndPortabilityTests.cs
tests/LLMGameCreator.Tests/Application/Goal161Q/Goal161QQualificationTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/goal161-generated-gameplay-save-migration.md
docs/manual-acceptance/goal161q-standalone-qualification-closure.md
docs/manual-acceptance/goal161r-short-output-qualification-closure.md

docs/agent-tasks/goal-161r-short-player-output-path-and-standalone-qualification-closure/
.llmgc/procedural/goal-161r-short-player-output-path-and-standalone-qualification-closure/
.llmgc/exports/goal-161r-short-player-output-path-and-standalone-qualification-closure/
```

Forbidden:

```text
all Unity paths
Runtime/Runtime.Abstractions
GamePackage schema
FeatureModule catalog
generated save/migration implementation
generated source/history
```


## N. Command budget

```text
read-first and architecture review: 6 minutes
short output resolver and path budget: 10 minutes
smoke-before-publish/rollback: 10 minutes
behavioral tests: 12 minutes
focused regressions: 8 minutes
single qualification smoke: 8 minutes
evidence/docs/artifact scope: 8 minutes
target wall clock: 50 minutes
maximum two concurrent testhost processes
Unity Editor process count: 0
```

Rules:

```text
write test inventory before production edits
no unchanged command repetition
no timeout escalation
after failure run only the exact class/test
do not invoke player before all offline checks pass
do not consume more than one new player invocation
do not defer evidence/docs/artifact scope
```

## O. Publication

Create exactly one final commit:

```text
GREEN Goal 161R short player output path and standalone qualification closure
```

or honest:

```text
BLOCKED Goal 161R short player output path and standalone qualification closure
FAILED Goal 161R short player output path and standalone qualification closure
```

Codex performs standard push.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three Goal161R task files tracked
new player invocation count exactly 1
corrective retries 0
Unity Editor starts 0
Unity host builds 0
host cache key unchanged
Goal142 and goal148-manual unchanged
Goal161 accepted=false
no human gate
```

## P. GREEN criteria

```text
Goal161Q root cause recorded and accepted
short output resolver implemented
long project path cannot create long player payload path
staging and final maximum player paths <=240
path budget blocks launch before process start
smoke runs before publication
prior successful output survives preflight/smoke/publication failures
old project-local outputs untouched
one new cached hidden smoke GREEN
HostReused=true
HostRebuilt=false
Unity Editor starts=0
all five smoke markers
Player.log captured
RC CURRENT
portable all-selectable passed
portable core-only passed without false RC readiness
Goal160 audit blocker formally closed
Goal161 qualification GREEN_ACCEPTABLE_CANDIDATE
Goal161R tests and required regressions GREEN
9+9 evidence byte-identical
text integrity GREEN
artifact scope 0
one final commit pushed
```

## Q. Final report

Return GREEN/BLOCKED/FAILED and include:

- model/reasoning;
- initial worktree inventory;
- Goal161Q root-cause intake;
- output root/token/final/staging layout;
- reproduced old and new maximum path lengths;
- path-budget/preflight matrix;
- smoke-before-publish and prior-output preservation;
- Goal161R discovered/behavioral test counts;
- focused regression counts;
- host key/hash/reuse and Unity count;
- exact new smoke count, exit code, markers and Player.log;
- RC and portable all-selectable/core-only results;
- Goal160/161/161Q state;
- evidence/text/artifact scope;
- final SHA/push/HEAD/worktree;
- confirmation no human gate was created.
