# Goal 161T — Immutable Standalone Payload / RC Correlation & Qualification Closure

## Identity

- Task ID: `goal-161t-immutable-standalone-payload-rc-correlation-and-qualification-closure`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `bc669ef7865cd2a15133bab37ccc4afb98be908f`
- Required base message: `BLOCKED Goal 161S immutable standalone run publication and qualification closure`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Luna
Reasoning effort: High
```

Reason: the remaining defect is exact and mechanical. Goal161S produced a fully qualified immutable
standalone run, GREEN run-status, validated current pointer, exit code 0 and all smoke markers. The
controller then failed only because release-candidate inspection still hardcodes the removed
project-local `Builds/Windows/<slug>` payload path. This task must correlate RC evidence with the
validated immutable run and close qualification without launching Player or Unity again.

## Pre-approval

- The owner approved execution by launching this task.
- Produce a concise internal plan; do not ask for confirmation.
- Do not request manual testing.
- Do not invoke Player, Unity Editor or a Unity host build.
- Do not rebuild the standalone solely to recover RC evidence.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No intermediate commits.
- Codex performs commit and standard push itself.

## Initial worktree

After unpacking, only these untracked files are allowed:

```text
docs/agent-tasks/goal-161t-immutable-standalone-payload-rc-correlation-and-qualification-closure/GOAL.md
docs/agent-tasks/goal-161t-immutable-standalone-payload-rc-correlation-and-qualification-closure/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-161t-immutable-standalone-payload-rc-correlation-and-qualification-closure/README.md
```

Require:

```text
HEAD == origin/main == bc669ef7865cd2a15133bab37ccc4afb98be908f
branch=main
no other tracked/staged/untracked changes
```

Never use reset, stash, merge, rebase or destructive cleanup.

## Execution budget

```text
Player invocation budget: 0
Unity Editor invocation budget: 0
Unity host build budget: 0
standalone rebuild budget: 0
manual test budget: 0
```

Use the already existing Goal161S GREEN immutable run and current pointer for real qualification.
If the retained real project/run is unavailable or invalid, publish BLOCKED. Do not substitute a new
smoke.

## Goal161S independent-audit intake

Commit:

```text
bc669ef7865cd2a15133bab37ccc4afb98be908f
```

Verified GREEN standalone layer:

```text
immutable run layout under %LOCALAPPDATA%/LGC/O/<token>/runs/r-<attempt>
no post-smoke run-tree move
payload preflight 13/13
legacy parser compatibility GREEN
maximum player path 143/240
Player exit code 0
all five smoke markers
Player.log captured
GREEN run-status
atomic current.json published and validated
HostReused=true
HostRebuilt=false
Unity Editor starts=0
```

The exact final failure:

```text
controller Stage=release_candidate_record
diagnostic=rc.payload.missing
```

### Proven source defect

`GameProjectReleaseCandidateRecordService.Write()` calls:

```text
InspectPayload(projectFolder, identity.PackageId)
```

`PayloadPaths()` still resolves only:

```text
<project>/Builds/Windows/<slug>/<slug>_Data/StreamingAssets/LLMGameCreatorProject
```

It never resolves:

```text
%LOCALAPPDATA%/LGC/O/<project-token>/runs/<current-run>/g_Data/StreamingAssets/LLMGameCreatorProject
```

Therefore `rc.payload.missing` is deterministic even though the immutable run and pointer are valid.

The same stale assumption also exists in RC `Read()`: it may validate an old project-local payload,
but cannot validate the machine-local immutable current output.

## Product truth to preserve

Do not redesign:

```text
immutable standalone run and current pointer
short path budget
payload preflight and smoke diagnostics
generated gameplay saves/migration
world regeneration/history rollback
Runtime/GamePackage/FeatureModules
RC record schema unless an additive field is absolutely required
```

Portable project copies must continue to restore RC truth without requiring machine-local output.

## Mandatory read-first

Read at most 10 primary files:

```text
GameProjectReleaseCandidateRecordService.cs
ProjectStandaloneOutputLocationService.cs
ProjectStandaloneBuildService.cs
ProjectStandaloneBuildModels.cs
UnifiedGameProjectWorkspaceController.cs
Goal161StandaloneAndPortabilityTests.cs
Goal161S qualification tests/evidence
Goal155A RC correlation tests
Goal155 standalone portability tests
docs/CURRENT_GENERATOR_STATE.md
```

Before production edits write:

```text
.llmgc/procedural/goal-161t-immutable-standalone-payload-rc-correlation-and-qualification-closure/architecture-review.json
```

Required sections:

```text
exactStalePathDefect
immutableCurrentOutputAuthority
writeCorrelation
readCorrelation
legacyOutputCompatibility
portableNoOutputSemantics
standaloneHistoryRecovery
zeroExecutionQualification
rcAndPortabilityClosure
nonGoals
```


## A. Immutable standalone payload evidence

Create a typed Application service, naming flexible:

```text
ProjectStandalonePayloadEvidenceService
ProjectStandalonePayloadEvidenceResult
ProjectStandalonePayloadEvidence
```

Dependencies:

```text
ProjectStandaloneOutputLocationService
ProjectStandalonePayloadSelfCheckService
```

Input:

```text
ProjectFolder
ProjectPackageId
ExpectedStandaloneResult optional
```

Output:

```text
Passed
SourceKind
RunOutputFolder technical only
Pointer
RunStatus
ProjectManifestSha256
PlayerAdapterModelSha256
PlayerAdapterFramesSha256
GamePackageSha256
PackageSha256
CompositionPackageSha256
FinalStateHash
ModelFinalStateHash
HumanFacts
HumanFactsSha256
PayloadSelfCheck
Diagnostics
```

Source kinds:

```text
immutable_current_pointer
legacy_project_local_output
absent
```

### A1. Immutable current output authority

Call:

```text
ProjectStandaloneOutputLocationService.LoadCurrentOutput(projectFolder, packageId)
```

This already validates:

```text
current pointer schema and project token
confined safe run name
complete player set
build manifest
project manifest package/composition/final hashes
run-status GREEN
payload preflight
smoke exit 0 and markers
```

Then inspect payload under:

```text
<run>/g_Data/StreamingAssets/LLMGameCreatorProject
```

Parse using `System.Text.Json`.

Require:

```text
project-manifest schema v2
player-adapter-model schema v2
payload package/composition/final hashes equal pointer
model final hash equals pointer final hash
actual game-package file SHA equals pointer package SHA
human facts valid
payload self-check passes
```

Diagnostics:

```text
rc.payload.current_pointer_invalid:<cause>
rc.payload.current_run_missing
rc.payload.current_run_hash_mismatch
rc.payload.current_run_attempt_mismatch
rc.payload.current_run_result_mismatch
rc.payload.current_run_invalid_json
```

### A2. Standalone-result correlation for Write

When an expected `ProjectStandaloneBuildResult` is supplied, require:

```text
standalone Status=GREEN
OutputLocationKind=immutable_short_local_appdata_run
standalone OutputProjectToken == pointer ProjectToken
standalone OutputRunDirectoryName == pointer RunDirectoryName
standalone AttemptId == pointer PublishedAttemptId
standalone CurrentPointerSha256 == actual current.json SHA
standalone OutputFolder == resolved run folder
standalone ExecutablePath == resolved g.exe
standalone BuildManifestPath == resolved build-manifest.json
standalone PackageSha256 == pointer PackageSha256
standalone FinalStateHash == pointer FinalStateHash
standalone HostCacheKey == pointer HostCacheKey
standalone SmokeExitCode == 0
standalone LaunchSmokePassed=true
standalone PayloadSelfCheckPassed=true
standalone LegacyHostParserCompatibilityPassed=true
```

Do not accept a caller-supplied arbitrary external folder.

### A3. Legacy fallback

For compatibility with historical successful outputs/RC records:

```text
when no immutable current pointer exists,
inspect the old project-local Builds/Windows/<slug> payload if both manifest and model exist
```

Use the same structural/hash validation.

Legacy fallback is allowed for:

```text
reading existing RC records
writing only when standalone.OutputLocationKind is empty/legacy and its output paths match the
project-local resolved output
```

If an immutable current pointer exists but is invalid, do not silently fall back to stale project-local
payload. Return the immutable current-output diagnostic.

### A4. Absent machine output

For RC `Read()` only:

```text
no immutable pointer for this project path
and no legacy project-local payload
```

means:

```text
payload evidence absent
```

This is allowed so a portable copied project can validate its project-local RC record from:

```text
record internal integrity
current package/document/identity
authoring fingerprint
```

Do not reject portable RC merely because machine-local operational output is absent.

## B. Release-candidate service integration

Inject `ProjectStandalonePayloadEvidenceService` into
`GameProjectReleaseCandidateRecordService`, with a backward-compatible optional/default constructor.

### B1. Write

Replace old hardcoded `InspectPayload(projectFolder, packageId)` with:

```text
evidenceService.InspectForWrite(projectFolder, identity.PackageId, standalone)
```

Require evidence Passed and exact correlation with:

```text
build.PackageSha256
build.CompositionPackageSha256
build.FinalStateHash
AcceptedMechanics HumanFacts
Release Candidate=готов
```

Write the same project-local RC schema unless an additive source-kind field is needed.

Never store:

```text
absolute output folder
LocalAppData path
project token
run directory
current pointer path
```

in the RC record.

Stable diagnostics:

```text
rc.write.payload_evidence_missing
rc.write.payload_evidence_invalid:<cause>
rc.write.standalone_pointer_mismatch
rc.write.actual_payload_hash_mismatch
rc.write.actual_payload_missing_accepted_fact
rc.write.actual_payload_missing_ready_fact
```

### B2. Read

Resolution order:

```text
1. validate project-local RC record internal fields
2. inspect immutable current output when a pointer exists
3. otherwise inspect legacy project-local output when present
4. otherwise continue with no operational payload evidence
5. perform current package/document/identity/fingerprint truth
```

When immutable/legacy evidence exists, require:

```text
payload hashes/facts equal RC record
```

When evidence is absent:

```text
do not change RC record status solely due to absence
```

When evidence exists but is invalid/mismatched:

```text
reject RC record causally
```

### B3. Remove stale path authority

Delete or demote old `PayloadPaths()` so it is only a legacy resolver.

No current immutable RC path may be derived from package slug under the project.

## C. Current standalone result recovery

Add a typed read method, either to `ProjectStandaloneBuildService` or a focused history service:

```text
LoadCurrentQualifiedResult(projectFolder, packageId)
```

It must correlate:

```text
validated current pointer/run
project-local .llmgc/standalone-build-history.json
```

Select exactly one GREEN history row matching:

```text
AttemptId == pointer PublishedAttemptId
PackageSha256 == pointer PackageSha256
FinalStateHash == pointer FinalStateHash
HostCacheKey == pointer HostCacheKey
OutputProjectToken == pointer ProjectToken
OutputRunDirectoryName == pointer RunDirectoryName
CurrentPointerSha256 == actual pointer SHA
OutputLocationKind == immutable_short_local_appdata_run
LaunchSmokePassed
PayloadSelfCheckPassed
LegacyHostParserCompatibilityPassed
SmokeExitCode=0
```

Return causal failure for:

```text
standalone.current_history_missing
standalone.current_history_ambiguous
standalone.current_history_pointer_mismatch
```

Old history JSON remains readable.

## D. Zero-execution RC finalization

Create a focused method/service:

```text
FinalizeCurrentReleaseCandidate(...)
```

It may live on the workspace controller or a dedicated Application service.

Input authority:

```text
open project current authoring/build history
validated immutable current pointer/run
matching current standalone GREEN history row
```

Required:

1. no build;
2. no standalone assembly;
3. no Player;
4. no Unity;
5. select current GREEN `GameProjectBuildHistoryEntry` matching current package/composition/final and
   authoring fingerprint;
6. reconstruct/restore the typed `GameProjectBuildResult` using existing history reader;
7. load current qualified standalone result;
8. call RC `Write()`;
9. reopen/read RC;
10. require `CURRENT`.

Diagnostic stages:

```text
rc.finalize.current_build_missing
rc.finalize.current_standalone_missing
rc.finalize.payload_invalid
rc.finalize.write_failed
rc.finalize.read_not_current
```

This is a recovery seam for the real case:

```text
standalone succeeded and current pointer was committed,
but project-local RC write failed afterward
```

It is safe/idempotent:

```text
same build + same standalone + same facts -> equivalent RC truth
```

A successful future `BuildWindowsStandalone()` still writes RC normally; this method is not the
primary ordinary workflow.

## E. Qualification using the retained Goal161S run

Locate only within bounded operational/test roots:

```text
%LOCALAPPDATA%/LGC/O
%TEMP%/LLMGameCreator/Goal156Copies
.devflow/runs/goal161s*
```

Use the actual Goal161S evidence to identify the project/run:

```text
pointer exists and validates
run-status GREEN
Player exit 0
five markers
package/composition/final match the project
project-local standalone history contains matching GREEN result
project-local current build history is TRAVEL_CURRENT
generated save migration facts present
```

Do not search unrelated user folders.

Run `FinalizeCurrentReleaseCandidate()` with zero execution.

Require:

```text
RC record written project-locally
RC record configuration status CURRENT
overall RC status CURRENT
player-adapter-model/facts hashes recorded from immutable run
current pointer/run bytes unchanged
standalone history unchanged
build history unchanged
generated save tree unchanged
Player/Unity process counts unchanged
```

### E1. Portable all-selectable

Copy the complete project after RC finalization to a new short project path, but do not copy the
machine-local immutable output/pointer.

Without build/Player/Runtime/Unity:

```text
GeneratedWorld TRAVEL_CURRENT
save slot/revisions and migration CURRENT
AcceptedMechanics Passed=true
RC record CURRENT
```

The portable path will have another operational project token and no pointer. RC remains valid through
the absent-output rule.

### E2. Portable core-only

Use the already qualified core-only Goal161 project/copy.

Require without execution:

```text
GeneratedWorld TRAVEL_CURRENT
generated save truth restored
AcceptedMechanics Passed=false with MissingFactKinds
no RC CURRENT/READY/BUILD_GREEN_STANDALONE_PENDING
```

Do not manufacture an RC record for core-only.

## F. Future ordinary workflow

Add an end-to-end behavioral test with a fake/nonprocess standalone service or prebuilt immutable run:

```text
controller BuildWindowsStandalone receives GREEN immutable standalone result
RC Write resolves current pointer
controller returns GREEN instead of release_candidate_record failure
```

No actual Player invocation in the test.

Ensure:

```text
RC is never written before standalone GREEN/pointer publication
RC write failure still returns Stage=release_candidate_record
```

## G. Required behavioral tests

Create at least 22 Goal161T tests; at least 18 behavioral.

1. immutable current pointer resolves payload;
2. payload root uses `runs/<run>/g_Data`, not project `Builds/Windows`;
3. expected standalone result exact correlation passes;
4. attempt mismatch rejected;
5. project token mismatch rejected;
6. run directory mismatch rejected;
7. current pointer SHA mismatch rejected;
8. output folder/executable mismatch rejected;
9. missing immutable model rejected;
10. immutable payload hash mismatch rejected;
11. accepted fact missing rejected;
12. RC-ready fact missing rejected;
13. invalid existing pointer does not fall back to legacy output;
14. legacy project-local payload remains readable;
15. portable no-output RC read remains CURRENT;
16. portable no-output tampered package is rejected;
17. current standalone history exact row selected;
18. missing/ambiguous standalone history rejected;
19. zero-execution finalization writes RC CURRENT;
20. repeated finalization is idempotent in truth;
21. finalization changes no run/pointer/build/save bytes;
22. ordinary controller GREEN standalone writes RC through immutable evidence;
23. RC not written for failed standalone;
24. core-only remains no false readiness;
25. Goal161S/R/Q regressions GREEN;
26. Goal161 product/profile-neutral/save regressions GREEN;
27. Goal160–155 RC/portable regressions GREEN;
28. Player/Unity invocation count remains zero.

Source-string tests do not count as behavioral.


## H. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal161T"
dotnet test ... --filter "FullyQualifiedName~Goal161T"
dotnet test ... --filter "FullyQualifiedName~Goal161S"
dotnet test ... --filter "FullyQualifiedName~Goal161R"
dotnet test ... --filter "FullyQualifiedName~Goal161Q"
dotnet test ... --filter "FullyQualifiedName~Goal161"
dotnet test ... --filter "FullyQualifiedName~Goal160"
dotnet test ... --filter "FullyQualifiedName~Goal159"
dotnet test ... --filter "FullyQualifiedName~Goal158"
dotnet test ... --filter "FullyQualifiedName~Goal157"
dotnet test ... --filter "FullyQualifiedName~Goal155A"
dotnet test ... --filter "FullyQualifiedName~Goal155"
dotnet test ... --filter "FullyQualifiedName~ProjectStandaloneBuild"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~RuntimeSnapshotStore"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Then run the real zero-execution Goal161S RC finalization and portable-copy proof.

Explicitly monitor and record:

```text
Player process starts=0
Unity Editor process starts=0
standalone Build() calls=0
Runtime execution starts=0 for portable copies
```

Do not run:

```text
full suite
85-case historical closure
all-ProductSmoke
Player executable
BuildWindowsStandalone on the real fixture
Unity host build
visible automatic launch
```

A zero-match filter is failure.

## I. Evidence

Create exactly 8 files in each mirrored root:

```text
goal161t-dashboard.json
architecture-review.json
goal161s-independent-audit-finding.json
immutable-payload-correlation-proof.json
zero-execution-rc-finalization-proof.json
rc-portability-closure-proof.json
artifact-scope-proof.json
goal161t-report.md
```

Roots:

```text
.llmgc/procedural/goal-161t-immutable-standalone-payload-rc-correlation-and-qualification-closure/
.llmgc/exports/goal-161t-immutable-standalone-payload-rc-correlation-and-qualification-closure/
```

Procedural/export twins must be byte-identical.

### Dashboard fields

```text
status
candidateStatus
goal161tTestsDiscovered
goal161tBehavioralTestsPassed

goal161sIndependentAuditResult
goal161sStandaloneLayerPassed
goal161sRcDefectRecorded
staleProjectLocalPayloadPathRemovedFromCurrentAuthority

immutableCurrentPointerResolved
immutableRunPayloadPassed
immutablePayloadSourceKind
pointerAttemptCorrelationPassed
pointerHashCorrelationPassed
standaloneResultCorrelationPassed
payloadPackageHashPassed
payloadCompositionHashPassed
payloadFinalHashPassed
payloadAcceptedFactsPassed
payloadReadyFactPassed

currentStandaloneHistoryResolved
currentBuildHistoryResolved
zeroExecutionRcFinalizationPassed
rcFinalizationIdempotent
runTreeByteIdentical
currentPointerByteIdentical
standaloneHistoryByteIdentical
buildHistoryByteIdentical
generatedSaveTreeByteIdentical

playerProcessStartCount=0
unityEditorProcessStartCount=0
standaloneBuildInvocationCount=0

releaseCandidateRecordCurrent
releaseCandidateOverallCurrent
portableAllSelectablePassed
portableAllSelectableOperationalPointerAbsent
portableCoreOnlyPassed
coreOnlyNoFalseRcReady

legacyPayloadCompatibilityPassed
portableNoOutputRcReadPassed

goal161sRegressionPassed
goal161rRegressionPassed
goal161qRegressionPassed
goal161RegressionPassed
goal160RegressionPassed
goal159RegressionPassed
goal158RegressionPassed
goal157RegressionPassed
goal155aRegressionPassed
goal155RegressionPassed

goal142SourceByteIdentical
sourceGoal148ByteIdentical
artifactScopeViolationCount

goal160AuditBlockerClosed
goal161QualificationStatus
goal161Accepted=false
goal161tAccepted=false
```

No GREEN-required field may be null/PARTIAL/NOT_EXECUTED.

## J. State and docs

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
docs/manual-acceptance/goal161r-short-output-qualification-closure.md
docs/manual-acceptance/goal161s-immutable-standalone-publication-closure.md
```

Create:

```text
docs/manual-acceptance/goal161t-immutable-payload-rc-closure.md
```

On GREEN record:

```text
goal160AuditBlocker=closed_by_goal161t
goal160IndependentAuditRequired=false

goal161ImplementationStatus=GREEN
goal161CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal161QualificationStatus=GREEN
goal161Accepted=false
goal161AcceptedByHuman=false
goal161AcceptedByCodex=false
goal161ManualReviewRequired=false
goal161ManualGateReady=false
goal161IndependentAuditRequired=true

goal161qDiagnosticInfrastructurePassed=true
goal161rShortPathPlayerSmokePassed=true
goal161sImmutableRunPublicationPassed=true
goal161sRcPayloadDefect=closed_by_goal161t

goal161tImplementationStatus=GREEN
goal161tCandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal161tAccepted=false
goal161tManualReviewRequired=false
goal161tIndependentAuditRequired=true

goal161ImmutableStandalonePayloadCorrelationPassed=true
goal161ZeroExecutionRcFinalizationPassed=true
goal161HistoricalHiddenPlayerInvocationCount=4
goal161tPlayerInvocationCount=0
goal161HostReused=true
goal161HostRebuilt=false
goal161UnityEditorProcessStartCount=0
goal161ReleaseCandidateCurrent=true
goal161PortableAllSelectablePassed=true
goal161PortableCoreOnlyPassed=true

nextAction=independent_goal161t_audit_and_plan_next_major_product_vertical_slice
```

Preserve all historical failed/blocked Goal161/Q/R/S records.

No human gate.

## K. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal161t-rc-correlation-closure.ps1
.devflow/scripts/run-goal161t-rc-correlation-closure.cmd

src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildModels.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildService.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneOutputLocationService.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandalonePayloadEvidenceService.cs

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectReleaseCandidateRecordService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs

tests/LLMGameCreator.Tests/Application/Goal161T/Goal161TImmutablePayloadCorrelationTests.cs
tests/LLMGameCreator.Tests/Application/Goal161T/Goal161TReleaseCandidateWriteReadTests.cs
tests/LLMGameCreator.Tests/Application/Goal161T/Goal161TZeroExecutionFinalizationTests.cs
tests/LLMGameCreator.Tests/Application/Goal161T/Goal161TPortabilityTests.cs
tests/LLMGameCreator.Tests/Application/Goal161/Goal161StandaloneAndPortabilityTests.cs
tests/LLMGameCreator.Tests/Application/Goal161S/Goal161SQualificationTests.cs
tests/LLMGameCreator.Tests/Application/Goal155/Goal155ReleaseCandidateRecordTests.cs
tests/LLMGameCreator.Tests/Application/Goal155A/Goal155ACurrentPackageCorrelationTests.cs

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
docs/manual-acceptance/goal161s-immutable-standalone-publication-closure.md
docs/manual-acceptance/goal161t-immutable-payload-rc-closure.md

docs/agent-tasks/goal-161t-immutable-standalone-payload-rc-correlation-and-qualification-closure/
.llmgc/procedural/goal-161t-immutable-standalone-payload-rc-correlation-and-qualification-closure/
.llmgc/exports/goal-161t-immutable-standalone-payload-rc-correlation-and-qualification-closure/
```

One exact additional existing standalone-history/build-history test/model path may be added only after a
concrete compile/test failure and with a recorded reason.

Forbidden:

```text
all Unity paths
Runtime/Runtime.Abstractions
GamePackage schema
FeatureModule catalog
generated save/migration/source/history implementation
standalone payload schema changes
```

## L. Command budget

```text
read-first/architecture: 6 minutes
payload evidence service and RC integration: 12 minutes
standalone history recovery/finalizer: 10 minutes
behavioral tests: 14 minutes
focused regressions: 10 minutes
real zero-execution finalization/portability: 8 minutes
evidence/docs/artifact scope: 8 minutes
target wall clock: 55 minutes
maximum two concurrent testhost processes
```

Rules:

```text
write test inventory before production edits
no unchanged command repetition
no timeout escalation
after failure run only exact class/test
never invoke Player/Unity/standalone Build
do not defer evidence/docs/artifact scope
```

## M. Publication

Create exactly one final commit:

```text
GREEN Goal 161T immutable standalone payload RC correlation and qualification closure
```

or honest:

```text
BLOCKED Goal 161T immutable standalone payload RC correlation and qualification closure
FAILED Goal 161T immutable standalone payload RC correlation and qualification closure
```

Codex performs standard push.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three Goal161T task files tracked
Player starts=0
Unity starts=0
standalone builds=0
Goal142 and goal148-manual unchanged
Goal161 accepted=false
no human gate
```

## N. GREEN criteria

```text
exact stale RC payload-path defect recorded
immutable current pointer/run is RC payload authority
standalone result/pointer/run exact correlation
legacy output read/write compatibility preserved
portable no-output RC semantics preserved
current standalone history recovery truthful
zero-execution RC finalization succeeds on real Goal161S run
RC record and overall status CURRENT
no run/pointer/history/save bytes change
portable all-selectable CURRENT with no operational pointer
portable core-only no false readiness
Goal161T tests and required regressions GREEN
Player/Unity/standalone invocation counts zero
8+8 evidence byte-identical
text integrity GREEN
artifact scope 0
Goal160 blocker formally closed
Goal161 qualification GREEN_ACCEPTABLE_CANDIDATE
one final commit pushed
```

## O. Final report

Return GREEN/BLOCKED/FAILED and include:

- model/reasoning;
- initial worktree;
- exact Goal161S RC defect;
- immutable pointer/run payload evidence;
- standalone-result correlation;
- legacy and portable absent-output behavior;
- recovered current standalone/build histories;
- zero-execution finalization result;
- process/build invocation counts;
- RC record/overall status;
- portable all-selectable/core-only;
- tests/regressions;
- source/baseline immutability;
- evidence/text/artifact scope;
- state/no-human-gate;
- final SHA/push/HEAD/worktree.
