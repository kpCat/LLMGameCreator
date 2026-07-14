# Goal 154C3 — Real Project, Standalone Evidence & Publication Closure

## Identity

- Task ID: `goal-154c3-real-project-standalone-evidence-publication-closure`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `9e3bde92b44c6bf1c1d5dbf08a5f886f8829813e`
- Required base message: `BLOCKED Goal 154C2 authoring fingerprint evidence and standalone payload final closure`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Luna
Reasoning effort: High
```

Reason: the product code is already implemented. This task is a bounded mechanical closure:
behavioral integration proof, one cached smoke, actual payload inspection, evidence, current-state and
artifact-scope publication. Do not redesign the social Runtime or authoring architecture.

## Pre-approval

- The owner approved execution by launching this task.
- Produce a concise internal execution checklist; do not ask for confirmation.
- Do not request intermediate human testing.
- Preserve Goal154B/B1/C/C1/C2 code unless a focused behavioral test reproduces a P0/P1.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No candidate/intermediate commits.
- Codex performs commit and push itself.

## Expected initial worktree

After this ZIP is unpacked, the only permitted untracked files are:

```text
docs/agent-tasks/goal-154c3-real-project-standalone-evidence-publication-closure/GOAL.md
docs/agent-tasks/goal-154c3-real-project-standalone-evidence-publication-closure/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-154c3-real-project-standalone-evidence-publication-closure/README.md
```

Required:

```text
HEAD == origin/main == 9e3bde92b44c6bf1c1d5dbf08a5f886f8829813e
tracked diff count=0
staged diff count=0
unknown dirty/untracked count=0
```

The three task files are authorized and must be committed.
Any other dirty path blocks execution. Never use destructive cleanup.

## Mandatory orientation

Read only:

```text
AGENTS.md
docs/UNITY_EXECUTION_POLICY.md
docs/CURRENT_GENERATOR_STATE.md
Goal154C1 and Goal154C2 GOAL.md
FeatureModuleAuthoringFingerprintService.cs
GameProjectBuildHistoryReader.cs
UnifiedGameProjectWorkspaceController.cs
```

Then write `.llmgc/procedural/<goal>/closure-audit.json` containing:

```text
inheritedCodeRetained=true
remainingProductCodeGap
remainingProofGap
remainingPublicationGap
hostCacheContract
manualGate
```

If the audit finds a real P0/P1, fix only that defect and record it. Otherwise production-source
changes are limited to tiny truth/diagnostic corrections.

## Unity and smoke budget

```text
Unity Editor invocation budget: 0
Unity host build budget: 0
real hidden standalone smoke budget: exactly 1
visible automated standalone launch budget: 0
custom standalone smoke budget: 0
```

Use the existing host cache. If it is incomplete or would rebuild, publish BLOCKED without starting
Unity.

## Current status

Preserve:

```text
Goals153/153A/153B/153C accepted by human
Goal154 and Goal154A historical FAILED
Goal154B and Goal154B1 GREEN
Goal154C and Goal154C1 historical FAILED foundations
Goal154C2 historical BLOCKED fingerprint foundation
Goal154 family human-unaccepted
Goal154ManualGateReady=false
```

## Independently audited inherited code

The required base already has:

```text
semantic authoring SHA-256 fingerprint
effective default normalization
required/selected module fingerprints
GREEN build/history fingerprint persistence
CURRENT/LAST_SUCCESS/UNKNOWN/ABSENT status
WinForms heading driven by configuration status
persisted last-success Social recovery
typed social projection
default 0→10 reputation and gold 0→10→17
custom reward 9 -> gold 19
locked final gold 10
one prior cache-only smoke implementation path
```

No new social primitives, module mutations or Runtime effects are authorized.

## Important semantic scenario correction

Test this sequence on one default disposable copy:

```text
build reward=7 -> CURRENT
change 7→9, save/reopen without build -> LAST_SUCCESS, card still gold 17
change 9→7, save/reopen without any intervening build -> CURRENT
```

This is the correct semantic-return proof.

Use a **separate custom copy** for:

```text
build reward=9 -> CURRENT, gold 19
```

Do not require returning from a successfully built 9 configuration to 7 to become CURRENT without
another build; the active package would still be the 9 build.

## Primary objectives

1. Run and publish the real disposable project lifecycle.
2. Prove semantic fingerprint status through fresh controller reopen.
3. Run exactly one cached hidden standalone smoke.
4. Inspect the actual default standalone payload.
5. Prove custom reward standalone facts through a capturing service with zero second smoke.
6. Run Goal153C and required focused regressions.
7. Publish 10+10 byte-identical evidence files.
8. Update current-state, queue, milestone, risk, debt and manual-acceptance docs.
9. Run actual artifact-scope validation.
10. Finish with Goal154 manual gate ready but no human acceptance claimed.


## A. Behavioral proof additions

Create at least 12 Goal154C3 tests; at least 10 must be behavioral.

Required tests:

1. default disposable build stores a nonempty qualified authoring fingerprint;
2. fresh reopen after default build reports `CURRENT`;
3. reward `7→9`, save/reopen without build reports `LAST_SUCCESS`;
4. that last-success card still contains gold `0→10→17`;
5. reward `9→7`, save/reopen without an intervening build reports `CURRENT`;
6. revision/timestamp-only save remains `CURRENT`;
7. separate custom reward-9 build reports `CURRENT` and gold 19;
8. locked threshold-20 build reports `CURRENT`, gold 10 and no repeat row;
9. invalid threshold 101 preserves last success and qualified fingerprint across fresh reopen;
10. old history without fingerprint reports `UNKNOWN`;
11. history Social false checkpoint/full-replay flags are rejected;
12. current fingerprint failure reports `UNKNOWN`, never `CURRENT`;
13. integer, number, boolean and enum canonicalization is invariant under a comma-decimal culture;
14. custom captured standalone request contains actual social facts 19/+9;
15. capturing custom proof invokes no real smoke/Unity;
16. WinForms heading is normal for `CURRENT`;
17. WinForms heading says last successful for `LAST_SUCCESS` and `UNKNOWN`;
18. Goal154C1 real project and history regressions remain GREEN.

A behavioral test invokes real fingerprint/history/controller/build/WinForms/standalone-interface
services and asserts resulting files/data. Source-string tests do not count.

### A1. Tiny inherited-code hygiene

If focused tests expose no P0/P1, the only optional production corrections are:

```text
clear stale persisted-history diagnostics after a successful build
ensure current social configuration status updates immediately after successful build
```

Do not make unrelated refactors.

## B. Disposable real-project lifecycle

Use read-only source:

```text
%LOCALAPPDATA%\LLMGameCreator\Games\goal148-manual
```

Capture a source manifest before any Application service touches a copy:

```text
relative path
size
SHA-256
```

Use short disposable roots under:

```text
%LOCALAPPDATA%\LLMGameCreator\Goal154C3\
```

### B1. Default copy

```text
enable three social modules
set 0/10/5/10/7
save
dispose/recreate services
reopen
verify five values
build GREEN
repeat build
```

Require:

```text
identity preserved
quest gold reward remains 10
reputation 0→10
gold 0→10→17
SocialOutcome=claimed
QualifiedAuthoringFingerprint nonempty
fresh reopen status=CURRENT
repeat package/composition/final hashes identical
```

### B2. Saved-unbuilt semantic truth

On the default copy after default GREEN:

```text
change trustedGoldReward 7→9
save
dispose/recreate
reopen without build
```

Require:

```text
saved value=9
status=LAST_SUCCESS
card still shows last-success gold 17
heading says last successful
active package/final hashes unchanged
```

Then, without building 9:

```text
change 9→7
save
dispose/recreate
reopen
```

Require:

```text
status=CURRENT
card gold 17
current fingerprint equals default qualified fingerprint
revision/timestamps may differ
```

### B3. Custom copy

On a separate copy:

```text
set 0/10/5/10/9
save/reopen/build
```

Require:

```text
status=CURRENT
gold 0→10→19
trusted delta=9
package and final hashes differ from default
qualified fingerprint differs from default
```

### B4. Locked copy

On a separate copy:

```text
threshold=20
reward=7
```

Require GREEN:

```text
status=CURRENT
reputation 0→10
gold 0→10
SocialOutcome=still_locked
RewardClaimed=false
no repeat-reward human fact
```

### B5. Invalid attempt

From a valid default copy:

```text
threshold=101
```

Require:

```text
build rejected at parameter/authoring stage
last successful package/composition/final hashes unchanged
qualified fingerprint unchanged
Social status/facts preserved after fresh reopen
no partial activation
causal diagnostic names parameter/range
```

### B6. Source immutability

After all tests:

```text
source manifest byte-identical
source mutation count=0
no disposable proof path committed
```

## C. Exactly one real cached standalone smoke

Use the valid default copy after returning to `CURRENT`.

Before calling standalone:

```text
assert zero Unity processes
resolve expected cache key
verify host cache is complete
hash host executable and complete host file set
```

If host is incomplete or `HostRebuilt` would be true:

```text
publish BLOCKED
do not start Unity
```

Call `BuildWindowsStandalone()` exactly once.

Require:

```text
Status=GREEN
HostReused=true
HostRebuilt=false
HostCacheKey recorded
Unity process count before/after=0
LaunchSmokePassed=true
SelfCheckPassedCount=SelfCheckTotalCount=5
host executable/file-set hashes unchanged
package hash equals default normal build
```

Inspect actual:

```text
<output>/<slug>_Data/StreamingAssets/LLMGameCreatorProject/player-adapter-model.json
```

Require actual `humanReviewFacts`:

```text
Репутация = 0 → 10
Квест = завершён
Доверенная реплика = недоступна → доступна → недоступна
Золото = 0 → 10 → 17
Награда за доверие = +7
Повторная награда = недоступна
Социальный итог = награда получена
```

The rendered phrase may include “получена” in the UI projection, but the typed visibility sequence
must remain the actual three Runtime observations already proven by Goal154B.

No raw IDs or hashes in social human facts.

Record exact smoke invocation count `1`.

## D. Custom standalone request proof with zero second smoke

Inject a capturing `IProjectStandaloneBuildService` into a fresh controller for the successfully
built custom reward-9 copy.

The capturing service:

```text
records the ProjectStandaloneBuildRequest
does not invoke ProjectStandaloneBuildService
does not assemble a player
does not launch a process
returns a clearly test-only captured result
```

Call `BuildWindowsStandalone()` through the real controller.

Require captured request:

```text
package/final hashes equal custom build
HumanReviewFacts include:
  Золото = 0 → 10 → 19
  Награда за доверие = +9
  Социальный итог = награда получена
selected modules/parameters reflect reward 9
RuntimeFrames nonempty
```

Prove:

```text
custom real smoke count=0
total task real smoke count=1
Unity process count remains 0
host-source/settings inputs are unchanged, so the same cache key applies
```


## E. Focused regressions

Run and record actual counts:

```text
Goal154C3
Goal154C2
Goal154C1
Goal154C
Goal154B1
Goal154B
Goal153C
UnifiedGameProjectWorkspace
ProjectsPage
ProjectStandaloneBuild
FeatureModuleLibrary
FeatureModuleCertification
Goal149 focused slice
Goal150 focused slice
current-state guard
```

Do not run:

```text
full suite
85-case historical closure
all-ProductSmoke
Unity host build
```

A zero-match test filter is a failure.

## F. Evidence and text integrity

Create exactly 10 files in each mirrored root:

```text
goal154c3-dashboard.json
closure-audit.json
authoring-status-lifecycle-proof.json
real-project-lifecycle-proof.json
cached-standalone-default-proof.json
actual-standalone-payload-proof.json
custom-captured-request-proof.json
focused-regression-proof.json
artifact-scope-proof.json
goal154c3-report.md
```

Procedural/export files must be byte-identical by relative name.

### Dashboard

Required fields:

```text
status
goal154c3TestsDiscovered
goal154c3BehavioralTestsPassed
defaultAuthoringFingerprint
customAuthoringFingerprint
fingerprintsDiffer
defaultStatusAfterBuild
savedUnbuiltStatus
returnedToDefaultStatus
customStatusAfterBuild
lockedStatusAfterBuild
defaultReputationBefore
defaultReputationAfter
defaultGoldAfterQuest
defaultGoldAfterClaim
customGoldAfterClaim
lockedFinalGold
invalidAttemptPreservedLastSuccess
sourceProjectByteIdentical
hostCacheKey
hostReused
hostRebuilt
hostFileSetHashUnchanged
unityProcessStartCount
hiddenSmokeInvocationCount
hiddenSmokePassed
standaloneSelfChecksPassed
actualDefaultPayloadFactsPassed
customCapturedRequestFactsPassed
customSecondSmokeInvocationCount
goal153cRegressionPassed
artifactScopeViolationCount
goal154Accepted=false
goal154c3Accepted=false
manualGateReady=true
```

No required GREEN value may be:

```text
null
PARTIAL
NOT_EXECUTED
unverified constant
source-string-only assertion
```

### New text integrity

Scan the actual candidate commit file set:

```text
all Goal154C3 task files
all Goal154C3 docs
all Goal154C3 procedural/export files
all changed C#/PS1/CMD/JSON/Markdown
```

Require:

```text
valid UTF-8
no NUL
no forbidden C0 controls except CR/LF/TAB
no mojibake markers
no escaped Cyrillic in user-facing JSON/Markdown where policy forbids it
```

Historical malformed Goal154B1 evidence remains immutable. Record only its sanitized intended
meaning:

```text
default gold 0 -> 10 -> 17
resource_transition_truthful is declaring-action scoped
```

## G. Documentation and state publication

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

docs/manual-acceptance/goal154-faction-reputation-social-consequences.md
docs/manual-acceptance/goal154a-social-lifecycle-runtime-proof.md
docs/manual-acceptance/goal154b-executable-social-runtime-core.md
docs/manual-acceptance/goal154b1-quest-reward-preservation.md
docs/manual-acceptance/goal154c-saved-project-winforms-standalone-social-closure.md
docs/manual-acceptance/goal154c1-persisted-social-result-real-project-standalone.md
docs/manual-acceptance/goal154c2-authoring-fingerprint-final-closure.md
docs/manual-acceptance/goal154c3-final-publication-closure.md
```

On GREEN publish:

```text
goal154ImplementationStatus=GREEN
goal154Accepted=false
goal154AcceptedByHuman=false
goal154AcceptedByCodex=false
goal154ManualReviewPerformed=false
goal154ManualGateReady=true

goal154aImplementationStatus=FAILED historical partial closure
goal154bImplementationStatus=GREEN
goal154b1ImplementationStatus=GREEN
goal154cImplementationStatus=FAILED historical product-surface foundation
goal154c1ImplementationStatus=FAILED historical persisted-history foundation
goal154c2ImplementationStatus=BLOCKED historical authoring-fingerprint foundation

goal154c3ImplementationStatus=GREEN
goal154c3Accepted=false
goal154c3AcceptedByHuman=false
goal154c3AcceptedByCodex=false
goal154c3ManualReviewPerformed=false
goal154c3ManualGateReady=true

goal154DefaultValues=0/10/5/10/7
goal154DefaultReputation=0->10
goal154DefaultGold=0->10->17
goal154DefaultSocialOutcome=claimed
goal154LockedFinalGold=10
goal154AuthoringFingerprintPassed=true
goal154SavedUnbuiltLastSuccessTruthPassed=true
goal154PersistedCardRecoveryPassed=true
goal154SavedProjectLifecyclePassed=true
goal154WinFormsSocialCardPassed=true
goal154HostReused=true
goal154HostRebuilt=false
goal154UnityProcessStartCount=0
goal154HiddenSmokeInvocationCount=1
goal154HiddenSmokePassed=true
goal154ActualStandaloneFactsPassed=true
goal154CustomCapturedRequestFactsPassed=true
goal154CustomSecondSmokeInvocationCount=0
nextAction=perform_goal154_combined_human_gate
```

Preserve all Goal153 acceptance flags exactly.
Do not claim Goal154 human acceptance.

## H. Artifact scope

Add a narrow policy entry for this Goal.

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal154c3-final-publication.ps1
.devflow/scripts/run-goal154c3-final-publication.cmd

src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleAuthoringFingerprintService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildHistoryReader.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs

tests/LLMGameCreator.Tests/Application/Goal154C2/Goal154C2AuthoringFingerprintTests.cs
tests/LLMGameCreator.Tests/Application/Goal154C3/Goal154C3AuthoringLifecycleTests.cs
tests/LLMGameCreator.Tests/Application/Goal154C3/Goal154C3RealProjectAndStandaloneTests.cs
tests/LLMGameCreator.Tests/Application/Goal154C3/Goal154C3WinFormsAndEvidenceTests.cs
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal154SocialConsequenceWorkspaceTests.cs
tests/LLMGameCreator.Tests/Application/ProjectStandaloneBuild/Goal154C3SocialPayloadTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

docs/manual-acceptance/goal154-faction-reputation-social-consequences.md
docs/manual-acceptance/goal154a-social-lifecycle-runtime-proof.md
docs/manual-acceptance/goal154b-executable-social-runtime-core.md
docs/manual-acceptance/goal154b1-quest-reward-preservation.md
docs/manual-acceptance/goal154c-saved-project-winforms-standalone-social-closure.md
docs/manual-acceptance/goal154c1-persisted-social-result-real-project-standalone.md
docs/manual-acceptance/goal154c2-authoring-fingerprint-final-closure.md
docs/manual-acceptance/goal154c3-final-publication-closure.md

docs/agent-tasks/goal-154c3-real-project-standalone-evidence-publication-closure/
.llmgc/procedural/goal-154c3-real-project-standalone-evidence-publication-closure/
.llmgc/exports/goal-154c3-real-project-standalone-evidence-publication-closure/
```

If an exact focused failure proves one additional existing test/Application path is required, record
the reason and add only that exact path.

Forbidden:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
catalogs/feature-modules/**
src/LLMGameCreator.Application/Design/FeatureModuleComposition/**
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/**
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs
unity/**
samples/**
generator-library/**
provider/**
LLM/**
RAG/**
public GamePackage schema changes
```

Run artifact-scope validation after all task-owned files and evidence exist.
Require violation count 0.


## I. Required validation sequence

### Phase 1 — build and discovery

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal154C3"
```

Require:

```text
Goal154C3 total discovered >=12
Goal154C3 behavioral discovered >=10
```

### Phase 2 — focused tests

```powershell
dotnet test ... --filter "FullyQualifiedName~Goal154C3"
dotnet test ... --filter "FullyQualifiedName~Goal154C2"
dotnet test ... --filter "FullyQualifiedName~Goal154C1"
dotnet test ... --filter "FullyQualifiedName~Goal154C"
dotnet test ... --filter "FullyQualifiedName~Goal154B1"
dotnet test ... --filter "FullyQualifiedName~Goal154B"
dotnet test ... --filter "FullyQualifiedName~Goal153C"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"
dotnet test ... --filter "FullyQualifiedName~ProjectStandaloneBuild"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleLibrary"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleCertification"
```

### Phase 3 — focused legacy slices

```powershell
.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

### Phase 4 — external proofs

```text
run disposable default/custom/locked/invalid lifecycle
run exactly one default cached hidden standalone smoke
inspect actual default player-adapter-model.json
run custom capturing-service proof with zero smoke
verify source manifest
```

### Phase 5 — publication

```text
write 10 procedural files
copy to export
verify exact names/counts and SHA-256 equality
scan new text integrity
update docs/current state
run artifact scope
re-run current-state guard if docs changed
```

No phase may be marked GREEN from a constant or previous Goal artifact.

## J. Command budget

```text
read-first: maximum 7 primary files
test additions and focused execution: maximum 9 minutes
disposable lifecycle: maximum 7 minutes
one smoke and payload inspection: maximum 4 minutes
evidence/docs/artifact scope: maximum 9 minutes
total target wall clock: 30 minutes
maximum two testhost processes
Unity process count: 0
```

Rules:

```text
do not repeat an unchanged command
do not escalate timeouts
after a concrete failure, run only the exact failing class/test
do not run full suite
do not run 85-case closure
do not run all-ProductSmoke
do not build Unity host
do not rewrite historical evidence
```

## K. Manual gate after independent GREEN audit

Exactly four steps:

```text
1. Enable “Фракции и репутация”, “Последствия квестов для репутации” and
   “Репутационные ветки диалога”; set 0/10/5/10/7 and build.
2. Confirm the social card shows reputation 0→10 and gold 0→10→17.
3. Save, close/reopen and confirm all five values and the social card remain.
4. Build/launch the cached standalone and confirm the same social facts.
```

No manual hashes, IDs, fingerprints, locked case, rollback or replay inspection.

## L. Publication

Create exactly one final commit:

```text
GREEN Goal 154C3 real project standalone evidence and publication closure
```

or honest:

```text
BLOCKED Goal 154C3 real project standalone evidence and publication closure
FAILED Goal 154C3 real project standalone evidence and publication closure
```

Codex performs commit and push.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three Goal154C3 task files tracked
Unity process start count=0
HostRebuilt=false
hidden smoke invocation count=1
custom second smoke count=0
Goal154 family accepted=false
Goal154ManualGateReady=true only on GREEN
```

## GREEN criteria

```text
no new P0/P1 product defect
Goal154C3 tests >=12, behavioral >=10, all pass
semantic CURRENT/LAST_SUCCESS return sequence passes
default/custom/locked/invalid disposable projects pass
source project byte-identical
one cached hidden smoke passes 5/5 with Unity 0
actual default standalone payload social facts pass
custom captured request facts pass with zero second smoke
Goal153C and all required focused regressions pass
10+10 evidence files complete and byte-identical
new text integrity passes
current-state/docs/manual publication complete
artifact-scope violation count=0
Goal154 implementation GREEN but human-unaccepted
manual gate ready
one final commit pushed
```

## Final report

Return GREEN, BLOCKED or FAILED and include:

- model/reasoning;
- initial worktree inventory;
- closure audit result;
- Goal154C3 discovered/behavioral test counts;
- default fingerprint and `CURRENT`;
- saved-unbuilt reward-9 `LAST_SUCCESS`;
- return-to-default `CURRENT`;
- custom fingerprint/status/gold 19;
- locked status/gold 10;
- invalid attempt preservation;
- source manifest result;
- host key/hash/reuse, Unity count and exact smoke count;
- five smoke markers/self-check count;
- actual default payload social facts;
- custom captured request facts and zero second smoke;
- focused regression counts including Goal153C;
- evidence text/mirror;
- artifact scope;
- state/manual-gate flags;
- exact four-step human gate;
- final SHA/push/HEAD/worktree;
- confirmation no human acceptance claimed.
