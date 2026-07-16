# Goal 161Q — Standalone Self-Check Diagnosis & Qualification Closure

## Identity

- Task ID: `goal-161q-standalone-self-check-diagnosis-and-qualification-closure`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `7935eaba5039c908b9807a09b3651f63b95ab912`
- Required base message: `BLOCKED Goal 161 profile neutral world change and generated gameplay save migration`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Sol
Reasoning effort: High
```

Reason: Goal161 product implementation and bounded regressions are GREEN, but the only allowed cached
standalone invocation failed with an opaque Unity-player self-check. The exact failing host check is
unknown. This task must diagnose the already-produced payload without consuming a launch, fix only
the proven cause, add durable diagnostics and perform exactly one new qualification smoke.

## Pre-approval

- The owner approved execution by launching this task.
- Produce a concise internal plan; do not ask for confirmation.
- Do not request manual testing.
- Do not start a player or Unity before offline failed-payload diagnosis is complete.
- Do not make speculative fixes.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No intermediate commits.
- Codex performs commit and standard push itself.

## Initial worktree

After unpacking, only these untracked files are allowed:

```text
docs/agent-tasks/goal-161q-standalone-self-check-diagnosis-and-qualification-closure/GOAL.md
docs/agent-tasks/goal-161q-standalone-self-check-diagnosis-and-qualification-closure/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-161q-standalone-self-check-diagnosis-and-qualification-closure/README.md
```

Require:

```text
HEAD == origin/main == 7935eaba5039c908b9807a09b3651f63b95ab912
branch=main
no other tracked/staged/untracked changes
```

Never use reset, stash, merge, rebase or destructive cleanup.

## Execution budgets

```text
offline payload diagnosis: required before launch
new hidden standalone smoke budget: exactly 1
corrective smoke retry budget: 0
Unity Editor invocation budget:
  0 when no Unity bootstrap change is required
Unity host build budget:
  0 when Application/payload fix is sufficient
  exactly 1 only when proven host-bootstrap defect requires an authorized source change
visible standalone launch: 0
```

If the one new smoke fails, publish BLOCKED with an exact named check/exception/log. Do not retry.

## Goal161 intake

Goal161 commit:

```text
7935eaba5039c908b9807a09b3651f63b95ab912
```

Published status:

```text
BLOCKED_HIDDEN_STANDALONE_SMOKE_FAILED
```

Verified useful implementation:

```text
Goal160 profile-neutral P1 fixed in code
all-selectable regeneration/rollback commit GREEN
core-only regeneration/rollback commit GREEN
core-only AcceptedMechanics remains incomplete without false RC readiness
76/76 Goal161 behavioral tests GREEN
generated_gameplay_save_v1 immutable revisions
generated_gameplay_save_slot_v1 atomic manifest
same-world exact UnifiedRuntimeSession load
controlled cross-world migration
19 observed compatible references preserved
10 observed incompatible references dropped with reasons
map/transient reset
post-load movement/travel/destination interaction/replay GREEN
historical original revision becomes CURRENT again
Runtime Simulator and Projects save UI implemented
operation races covered
```

Failed standalone facts:

```text
Host cache key: 6af4d5eb5b42f956110555b58fb4e276
HostReused=true
HostRebuilt=false
Unity Editor starts=0
player exit code=2
smoke file contains only LLMGC_PROJECT_STANDALONE_SMOKE_FAIL
assembled payload contains migration/travel/accepted-mechanics facts
RC CURRENT and portable post-smoke assertions not reached
```

## Independent audit result

Goal160's profile-neutral P1 is correctly implemented:

```text
history AcceptedMechanics requires Present=true, not Passed=true
AcceptedMechanics and Compatibility canonical hashes equal the sealed candidate
generic RC record and overall statuses equal the seal
```

No new product P0/P1 was found in the save-store/migration code during independent audit.

The standalone failure itself remains unresolved. A generic exit code and marker are not enough to
classify it as payload defect, host defect or transient environment failure.

## Product truth to preserve

Do not redesign:

```text
generated save schemas
definition fingerprint migration policy
operation coordinator
regeneration/history rollback
Runtime session/state schemas
FeatureModules
GamePackage
generated source/history
```

This is a qualification closure, not Goal162 product development.

## Mandatory read-first files

Read at most 12 primary files:

```text
ProjectStandaloneBuildService.cs
ProjectStandaloneBuildModels.cs
ProjectStandalonePlayerAdapterBootstrap.cs
UnifiedGameProjectWorkspaceController.cs
GeneratedGameplaySavesSummaryService.cs
Goal161StandaloneAndPortabilityTests.cs
run-goal161-generated-save-migration.ps1
Goal161 standalone-portability-proof.json
Goal161 goal161-report.md
Goal157/158/159/160 standalone proof tests
docs/CURRENT_GENERATOR_STATE.md
docs/UNITY_EXECUTION_POLICY.md
```

Before production changes write:

```text
.llmgc/procedural/goal-161q-standalone-self-check-diagnosis-and-qualification-closure/diagnosis-plan.json
```

Required sections:

```text
failedPayloadLocation
failedOutputHashes
hostCacheIdentity
twelveHostChecks
offlineReproduction
namedRootCause
applicationFixOption
hostFixOption
chosenMinimalFix
singleSmokePlan
postSmokeRcAndPortability
nonGoals
```

`namedRootCause` must be resolved before any player or Unity invocation.


## A. Preserve and inspect the exact failed output

Locate the actual Goal161 failed project/output using bounded local roots:

```text
.devflow/runs/goal161-validation/
%TEMP%/LLMGameCreator/Goal161/
%LOCALAPPDATA%/LLMGameCreator/
```

Do not scan unrelated user folders.

Capture SHA-256 and parse actual:

```text
project-manifest.json
player-adapter-model.json
player-adapter-frames.json
standalone-launch.json
game-package.json
build-manifest.json
smoke marker file
associated Unity Player.log when causally matched
```

Do not commit payload, executable, Player.log or absolute paths.

Evidence may contain relative names, hashes, counts, sanitized exception/check names and bounded
relevant log lines with machine paths removed.

If the original failed output is unavailable, reconstruct the exact Goal161 migrated project and
payload through a no-launch capture seam. This reconstruction does not consume the smoke budget.

## B. Application-side payload self-check

Create:

```text
ProjectStandalonePayloadSelfCheckService
ProjectStandalonePayloadSelfCheckResult
ProjectStandalonePayloadCheckResult
```

Use `System.Text.Json` for structural parsing.

Run the same logical checks as the cached host:

```text
01 payload files and supported v2 schemas
02 project package ID/title/version nonempty
03 package/final hashes nonempty
04 Runtime authority true, Unity gameplay false, projectionOnly false
05 Runtime frames nonempty
06 frame indices contiguous from zero and title/category/state hash nonempty
07 selected optional count equals selected module ID count
08 active count equals required + selected optional
09 configured parameter count equals actual effective parameter entries
10 human review facts nonempty and parseable
11 deterministic frame cursor contract
12 equipment/total damage invariant
13 actual game-package.json SHA equals manifest package SHA
```

The Unity host treats package SHA as load validation outside its 12-counter; expose it as check 13.

Also provide:

```text
LegacyHostParserCompatibility
```

This mirrors the exact current cached-host regex extraction for frames/facts so parser limitations are
diagnosed before launch.

Each failure has a stable code, for example:

```text
standalone.payload.frames_parse_mismatch
standalone.payload.selected_optional_count_mismatch
standalone.payload.active_count_mismatch
standalone.payload.parameter_count_mismatch
standalone.payload.human_facts_parse_mismatch
standalone.payload.package_hash_mismatch
```

Run the service against the exact failed Goal161 payload and publish named failing checks.

## C. Mandatory preflight

Before `RunSmoke()`:

```text
run ProjectStandalonePayloadSelfCheckService on assembled output
```

If any check fails:

```text
do not start executable
return Stage=payload_self_check
include every named failed check
preserve prior successful output according to existing policy
```

A future standalone failure may never again be represented only as exit code 2.

## D. Detailed smoke diagnostics

Update `RunSmoke()` to pass:

```text
-logFile <short confined player-log path>
```

Return a typed result:

```text
ExitCode
SmokeMarkerText
PlayerLogPresent
PlayerLogRelevantLines
NamedFailure
```

Keep evidence bounded and sanitized.

On failure, `ProjectStandaloneBuildResult.Diagnostics` includes:

```text
exit code
smoke marker
Application preflight result
bounded Player.log exception/self-check line
```

No absolute user paths in docs/evidence.

## E. Root-cause routing

### E1. Application/payload defect

When offline checks identify request/payload inconsistency:

```text
fix only ProjectStandaloneBuildService, request construction or generated save fact formatting
do not change Unity source
HostReused=true
HostRebuilt=false
```

### E2. Cached-host parser/bootstrap defect

Only when Application structural checks pass but `LegacyHostParserCompatibility` or existing Player.log
proves host parser/bootstrap is the cause.

Conditionally authorized Unity path:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/ProjectStandalonePlayerAdapterBootstrap.cs
```

Required fix:

```text
replace fragile regex extraction with robust JsonUtility-compatible DTO parsing or equivalent built-in
Unity JSON parsing
emit named check failures and exception text to smoke log
preserve visible UI and payload v2 compatibility
```

Then:

```text
host cache key changes naturally
exactly one normal ProjectStandaloneBuildService host build
exactly one new hidden smoke
```

Do not edit scenes, prefabs, ProjectSettings, Packages or other Unity files.

### E3. External/transient failure

Allowed only when:

```text
offline structural checks pass
legacy parser compatibility passes
existing Player.log identifies an external environment/process failure
no repository fix is warranted
```

Still add durable diagnostics and perform one authorized smoke. If it fails, publish BLOCKED with
exact evidence.

## F. Required tests

Create at least 20 Goal161Q tests; at least 16 behavioral:

1. exact failed Goal161 payload receives a named offline result;
2. valid Goal160 historical payload passes all checks;
3. current migrated payload passes after fix;
4. missing payload file;
5. unsupported schema;
6. package hash mismatch;
7. noncontiguous frame index;
8. blank frame title/category/hash;
9. selected optional/module count mismatch;
10. active count mismatch;
11. configured parameter count mismatch;
12. no human facts;
13. malformed/escaped human fact compatibility;
14. damage invariant failure;
15. self-check failure prevents process invocation;
16. detailed RunSmoke captures exit code/marker/player log;
17. all-selectable migrated request facts retained;
18. travel facts retained;
19. accepted-mechanics facts retained;
20. save migration facts retained;
21. Goal161 all-selectable/core-only regressions GREEN;
22. Goal157–160 standalone regressions GREEN;
23. RC write occurs only after GREEN smoke;
24. portable assertions run only after GREEN smoke.

When Unity source changes, add bootstrap JSON/named-log tests, but source-string tests do not replace
the behavioral matrix.

## G. One qualification run

After offline tests and regressions:

1. confirm no Unity process;
2. confirm current cache state;
3. run exactly one standalone build on Goal161 all-selectable migrated project;
4. no second attempt.

Expected:

```text
payload preflight all checks GREEN
hidden smoke GREEN
all five required smoke markers
HostReused=true / HostRebuilt=false for Application fix
or HostRebuilt=true exactly once for proven bootstrap fix
Unity Editor starts 0 for reuse or exactly 1 for authorized host build
RC CURRENT
portable all-selectable CURRENT
portable core-only save truth without false RC readiness
```


## H. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal161Q"
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
unchanged smoke retry
more than one new player invocation
more than one host build
```

A zero-match filter is failure.

## I. Evidence

Create exactly 9 files in each mirrored root:

```text
goal161q-dashboard.json
diagnosis-plan.json
goal161-failed-output-forensics.json
payload-self-check-proof.json
named-root-cause-proof.json
single-smoke-proof.json
rc-portability-closure-proof.json
artifact-scope-proof.json
goal161q-report.md
```

Roots:

```text
.llmgc/procedural/goal-161q-standalone-self-check-diagnosis-and-qualification-closure/
.llmgc/exports/goal-161q-standalone-self-check-diagnosis-and-qualification-closure/
```

Twins byte-identical.

Dashboard fields:

```text
status
goal161qTestsDiscovered
goal161qBehavioralTestsPassed
goal161ProductRegressionsPassed
failedPayloadRecovered
failedPayloadSha256
offlineFailedCheckCodes
namedRootCause
applicationPayloadFixApplied
unityBootstrapFixApplied
payloadPreflightPassed
legacyHostParserCompatibilityPassed
newHostCacheKey
hostReused
hostRebuilt
unityEditorProcessStartCount
newHiddenSmokeInvocationCount
newHiddenSmokePassed
smokeExitCode
smokeMarkersPassed
playerLogDiagnosticCaptured
actualPayloadSaveMigrationFactsPassed
actualPayloadTravelFactsPassed
actualPayloadAcceptedFactsPassed
releaseCandidateRecordCurrent
portableAllSelectablePassed
portableCoreOnlyPassed
coreOnlyNoFalseRcReady
artifactScopeViolationCount
goal160AuditBlockerClosed
goal161ImplementationStatus
goal161QualificationStatus
goal161Accepted=false
goal161qAccepted=false
```

No GREEN-required value may be null/PARTIAL/NOT_EXECUTED.

## J. State/docs

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
```

Create:

```text
docs/manual-acceptance/goal161q-standalone-qualification-closure.md
```

On GREEN:

```text
goal160AuditBlocker=closed_by_goal161q
goal160IndependentAuditRequired=false

goal161ImplementationStatus=GREEN
goal161CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal161QualificationStatus=GREEN
goal161Accepted=false
goal161ManualReviewRequired=false
goal161ManualGateReady=false
goal161IndependentAuditRequired=true

goal161qImplementationStatus=GREEN
goal161qAccepted=false
goal161qManualReviewRequired=false
goal161qIndependentAuditRequired=true

goal161StandaloneNamedRootCause=<exact code>
goal161PayloadSelfCheckPassed=true
goal161HiddenSmokeInvocationCount=2_historical_total
goal161qNewHiddenSmokeInvocationCount=1
goal161ReleaseCandidateCurrent=true
goal161PortableAllSelectablePassed=true
goal161PortableCoreOnlyPassed=true

nextAction=independent_goal161q_audit_and_plan_next_major_product_vertical_slice
```

Historical failed Goal161 smoke remains immutable.

No human gate.

## K. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal161q-standalone-closure.ps1
.devflow/scripts/run-goal161q-standalone-closure.cmd

src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildModels.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildService.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandalonePayloadSelfCheckService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs

tests/LLMGameCreator.Tests/Application/Goal161Q/Goal161QPayloadForensicsTests.cs
tests/LLMGameCreator.Tests/Application/Goal161Q/Goal161QPayloadSelfCheckTests.cs
tests/LLMGameCreator.Tests/Application/Goal161Q/Goal161QSmokeDiagnosticsTests.cs
tests/LLMGameCreator.Tests/Application/Goal161Q/Goal161QQualificationTests.cs
tests/LLMGameCreator.Tests/Application/Goal161/Goal161StandaloneAndPortabilityTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/goal161-generated-gameplay-save-migration.md
docs/manual-acceptance/goal161q-standalone-qualification-closure.md

docs/agent-tasks/goal-161q-standalone-self-check-diagnosis-and-qualification-closure/
.llmgc/procedural/goal-161q-standalone-self-check-diagnosis-and-qualification-closure/
.llmgc/exports/goal-161q-standalone-self-check-diagnosis-and-qualification-closure/
```

Conditionally allowed only after proven host defect:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/ProjectStandalonePlayerAdapterBootstrap.cs
tests for that exact bootstrap contract
```

Forbidden:

```text
other Unity paths
Runtime/Runtime.Abstractions
GamePackage schema
FeatureModule catalog
generation/save migration semantics
scenes/prefabs/ProjectSettings/Packages
```

## L. Command budget

```text
read-first and recover failed output: 6 minutes
offline self-check implementation/reproduction: 10 minutes
exact root-cause fix: 10 minutes
tests and focused regressions: 12 minutes
one qualification smoke: 8 minutes
evidence/docs/artifact scope: 8 minutes
target wall clock: 45 minutes
maximum two testhost processes
```

No unchanged command repetition or timeout escalation.

## M. Publication

Create exactly one final commit:

```text
GREEN Goal 161Q standalone self-check diagnosis and qualification closure
```

or honest BLOCKED/FAILED.

Required final:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three task files tracked
new smoke count exactly 1
no corrective retry
Unity budget obeyed
Goal142 and goal148-manual unchanged
Goal161 accepted=false
no human gate
```

## N. GREEN criteria

```text
exact old failed payload diagnosed offline
one named root cause
no speculative fixes
payload self-check preflight installed
future smoke failures expose exact details
all Goal161Q tests pass
Goal161/160/159/158/157 regressions pass
one new hidden smoke passes
cache reused or exactly one justified host rebuild
RC CURRENT
portable all-selectable passes
portable core-only passes without false RC readiness
Goal160 blocker formally closed
Goal161 becomes GREEN_ACCEPTABLE_CANDIDATE
9+9 evidence mirrored
artifact scope 0
one final commit pushed
```
