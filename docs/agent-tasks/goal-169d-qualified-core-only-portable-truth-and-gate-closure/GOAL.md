# Goal 169D — Qualified Core-Only Portable Truth & Gate Closure

## Identity

- Task ID: `goal-169d-qualified-core-only-portable-truth-and-gate-closure`
- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `72f69be12b898583d902237ae99e5bc1fe890d2c`
- Required base message: `BLOCKED Goal 169C post fix immutable standalone rc and portable closure`
- Model: GPT-5.6 Terra
- Reasoning: High

Goal169C successfully launched and published an all-selectable standalone, proved immutable
payload/history/package correlation, RC CURRENT and all-selectable portability. Its only failure was
a core-only assertion that copied `Goal156TestKit.CoreOnly` directly.

Independent review found that `Goal156TestKit.CreateGenerated()` creates package/source/authoring but
does not call `BuildAndQualify()`. Therefore the raw CoreOnly fixture has no current v7 build history.
Expecting campaign/event CURRENT from it is an invalid test fixture, not a product defect.

The valid authority is `Goal164BuildFixture.Create(coreOnly:true)`, which performs a real current-code
`BuildAndQualify()`. Goal169D replaces the invalid proof with a qualified core-only build followed by
a physical portable copy. No Player smoke and no new product feature.

## 1. Publication contract

- Launching this task approves the complete plan.
- No extra confirmation or manual testing.
- No intermediate commits.
- Publish exactly one honest GREEN/BLOCKED/FAILED commit.
- Always push `origin/main`; never leave publication to the user.

## 2. Initial worktree

After unpacking only these untracked files are allowed:

```text
docs/agent-tasks/goal-169d-qualified-core-only-portable-truth-and-gate-closure/GOAL.md
docs/agent-tasks/goal-169d-qualified-core-only-portable-truth-and-gate-closure/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-169d-qualified-core-only-portable-truth-and-gate-closure/README.md
```

Require `HEAD == origin/main == 72f69be12b898583d902237ae99e5bc1fe890d2c`, branch `main`, and no other changes.
Never reset, revert, stash, merge or rebase.

## 3. Budgets

```text
Goal169/169A/169B/169C smoke reruns=0
Goal169D real Player smoke invocations=0
Unity Editor starts=0
Unity host builds=0
cached host mutations=0
manual/visible launches=0
max testhost=2
```

A capturing/fake standalone service is allowed for focused request/result tests but must not launch a
Player or publish an external operational pointer.

## 4. Classification and audit intake

Before edits create mirrored `scaffold-classification.json`.
Classify touched files KEEP_AND_COMPLETE / REFACTOR / REPLACE / REMOVE_AS_UNUSED.

Record:

```text
goal169cImplementationCommit=72f69be12b898583d902237ae99e5bc1fe890d2c
goal169cIndependentAuditResult=BLOCKED_AT_72F69BE1
goal169cStandaloneProof=GREEN
goal169cImmutablePublication=GREEN
goal169cAllSelectablePortable=GREEN
goal169cRcCurrent=true
goal169cBlocker=invalid_creation_only_core_fixture
goal169cRawFixture=Goal156TestKit.CoreOnly
goal169cRawFixtureBuildInvocationCount=0
goal169dAuthority=qualified_current_code_core_only_build_then_physical_portable_copy
```

All Goal169 variants remain accepted=false; no human gate.

## 5. Root-cause review

Read at most 16 primary files:

```text
Goal169CStandaloneSmokeTests.cs
Goal156ProjectCreationTests.cs / Goal156TestKit
Goal164CombatContractResolutionTests.cs / Goal164BuildFixture
Goal164StandaloneAndPortabilityTests.cs / Goal164PortableState
Goal168StandalonePortabilityTests.cs
GameProjectBuildHistoryReader.cs
UnifiedGameProjectWorkspaceController.cs
GameProjectBuildAndQualificationService.cs
GeneratedCampaignRegionalEventCorrelationService.cs
ProjectStandaloneOutputLocationService.cs (read-only)
Goal169C dashboard/smoke/RC/immutable-correlation evidence
CURRENT_GENERATOR_STATE.json
check-current-goal.ps1 and the exact stale CurrentState test
```

Create mirrored:

```text
architecture-review.json
core-only-fixture-root-cause-proof.json
```

Prove:

```text
Goal156TestKit.CoreOnly comes from CreateGenerated
CreateGenerated does not invoke BuildAndQualify
raw fixture has no selected current v7 build
raw fixture must not claim CAMPAIGN_CURRENT
Goal164BuildFixture.Create(coreOnly:true) invokes BuildAndQualify exactly once
only the qualified fixture is valid portable campaign authority
```

Do not change production to make an unbuilt project look CURRENT.

## 6. Raw creation-only truth

Behaviorally prove:

```text
valid created package/source/authoring
CoreOnly profile, no optional modules
zero build invocation
no selected successful v7 history
no false current campaign/event/RC projection
Projects action = «Собрать и играть»
```

Record status `CREATION_ONLY_NOT_QUALIFIED`.

## 7. Qualified core-only build

Use `Goal164BuildFixture.Create(coreOnly:true)` or an equivalent explicit current-code fixture.

Require:

```text
exactly one BuildAndQualify
build Passed=true / GREEN
selected history schema=v7
package SHA exact and package validates
generation source/sidecars unchanged
GeneratedWorld.Status=CAMPAIGN_CURRENT
```

Measure:

```text
availableBranchCount =
  relationships.BranchQualifications.Count(Available=true)
```

If zero:

```text
relationships exact ABSENT or valid zero-branch CURRENT
events Present=false, Passed=true, Status=ABSENT, EventCount=0
strict empty policy/package record absence exact
```

If greater than zero:

```text
relationships RELATIONSHIPS_CURRENT
events REGIONAL_EVENTS_CURRENT
EventCount=QualifiedEventCount=availableBranchCount
exact package-backed branch/event/replay correlation
```

Always require:

```text
AcceptedMechanics incomplete for core-only
ReleaseCandidateConfigurationStatus != CURRENT
ReleaseCandidateRecordConfigurationStatus != CURRENT
```

Only if this correctly built fixture fails may production code be changed. Any production change
requires a reproduced defect, minimal path and new behavioral regression.

## 8. Physical portable core-only copy

Copy the fully built project, never raw `Goal156TestKit.CoreOnly`.

Remove:

```text
Builds/**
project-local transient standalone output
```

Do not copy/synthesize the external LocalAppData pointer.

Reopen using a new workspace/controller and require:

```text
package SHA equals qualified source
selected v7 history exact
GeneratedWorld CAMPAIGN_CURRENT
relationship/event truth equals source
package-backed regional event correlation passes
no operational pointer resolves for copied project
RC configuration/record remain not CURRENT
no source/authoring/generation rewrite
```

Record source/copy hashes for package, selected history, authoring, generation sidecars, event
inventory and final state.

## 9. No false RC readiness

Prove campaign-current and RC-current are distinct:

```text
core-only campaign current
AcceptedMechanics incomplete
RC configuration not CURRENT
RC record not CURRENT or absent by exact policy
no operational pointer
no false standalone readiness
```

The retained all-selectable Goal169C RC remains CURRENT and immutable.

## 10. Retained publication

Capture before/after hashes for successful Goal169C:

```text
current pointer
immutable run
run-status
payload
standalone history
selected v7 history
RC record
package/final hashes
```

Also preserve Goal169/Goal169A outputs, failed Goal169B run/forensics, cached host, Goal142, Goal148
and generation sidecars. Goal169D must not create a Player run or mutate the Goal169C pointer.

## 11. Current-state gate cleanliness

Reproduce the report's nongating stale Goal169B CurrentState assertion before changing it.

If reproduced:

- identify the exact stale expected token;
- update only the narrow test/document authority;
- do not weaken `check-current-goal.ps1`.

Final require:

```text
check-current-goal exit=0
PASSED present
FAILED absent
assertion mismatch count=0
stale Goal169B/Goal169C gate expectation absent
CURRENT_GENERATOR_STATE.json and markdown agree
```

If not reproducible, record `NOT_REPRODUCED_AT_72F69BE1` and make no speculative edit.

## 12. Tests

Create >=30 Goal169D tests, >=26 behavioral.

Mandatory groups:

```text
raw creation-only truth and invalid old assertion rejection
qualified core-only one-build v7/package/history truth
branch-dependent relationship/event truth
strict empty/event-bearing package correlation
physical portable copy and source/hash preservation
operational pointer absence
campaign current without false RC readiness
retained Goal169C pointer/run/payload/history/RC immutability
clean check-current-goal output
```

Regressions:

```text
Goal169C=34 with smoke disabled
Goal169B=72 with smoke disabled
Goal169A=60
Goal169=108
Goal168 focused
Goal167=94
Goal166=59
Goal165=55
Goal164=61
Goal163–157 and GeneratedCampaign/Save/Runtime/workspace/coordinator/standalone filters
```

No source-string-only assertion counts as behavioral proof.

## 13. Commands

Run build, Goal169D discovery/tests, all focused filters above, then:

```powershell
.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

All old smoke environment flags must be false.

Do not run full suite, Goal168 85-case closure, all-ProductSmoke, Unity host build, Player or any old
smoke. Zero-match filter is failure.

## 14. Evidence

Create exactly 15 byte-identical files in each root:

```text
.llmgc/procedural/goal-169d-qualified-core-only-portable-truth-and-gate-closure/
.llmgc/exports/goal-169d-qualified-core-only-portable-truth-and-gate-closure/
```

Files:

```text
goal169d-dashboard.json
architecture-review.json
scaffold-classification.json
goal169c-independent-audit-finding.json
core-only-fixture-root-cause-proof.json
core-only-qualified-build-proof.json
core-only-event-truth-proof.json
core-only-portable-copy-proof.json
no-false-rc-proof.json
retained-goal169c-publication-proof.json
current-goal-gate-proof.json
source-immutability-proof.json
regression-immutability-proof.json
artifact-scope-proof.json
goal169d-report.md
```

Dashboard must measure raw/qualified build counts, v7/package/world/branch/event truth, portable hashes,
pointer absence, RC false readiness, retained Goal169C exactness, Player/Unity/host counts, gate
cleanliness, regressions, protected bytes, scope and accepted=false.

No required GREEN field may be null/PARTIAL/NOT_EVALUATED.

## 15. Docs/state

Update generator current state/index/queue/gates/risks/debt/strategy/roadmap and Goal169C acceptance.
Create:

```text
docs/manual-acceptance/goal169d-qualified-core-only-portable-truth-gate-closure.md
```

GREEN state:

```text
goal169cCandidateStatus=BLOCKED_AT_72F69BE1
goal169cAccepted=false
goal169cIndependentAuditRequired=false
goal169cStandaloneProof=GREEN
goal169cImmutablePublication=GREEN
goal169cAllSelectablePortable=GREEN
goal169cCoreOnlyBlocker=invalid_creation_only_fixture
goal169cCoreOnlyBlockerClosure=closed_by_goal169d

goal169dImplementationStatus=GREEN
goal169dCandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal169dAccepted=false
goal169dManualGateReady=false
goal169dIndependentAuditRequired=true
goal169dQualifiedCoreOnlyBuildPassed=true
goal169dCoreOnlyCampaignCurrent=true
goal169dCoreOnlyPortablePassed=true
goal169dCoreOnlyNoFalseRcReady=true
goal169dRetainedGoal169CPublicationExact=true
goal169dCurrentGoalGateClean=true
goal169dRealPlayerSmokeInvocationCount=0
goal169dUnityEditorProcessStartCount=0
goal169dUnityHostBuildCount=0
goal169dArtifactScopeViolationCount=0

nextAction=independent_goal169d_audit_then_plan_next_visible_campaign_slice
```

No human gate.

## 16. Scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal169d-core-only-portable-closure.ps1
.devflow/scripts/run-goal169d-core-only-portable-closure.cmd
tests/LLMGameCreator.Tests/Application/Goal169D/**
tests/LLMGameCreator.Tests/Application/Goal169C/Goal169CStandaloneSmokeTests.cs
tests/LLMGameCreator.Tests/Application/Goal168/Goal168StandalonePortabilityTests.cs
one exact CurrentState test file if stale mismatch reproduces
listed generator docs/manual acceptance
task/evidence roots
```

Production application code is not initially allowed.
One exact production path may be added only after a correctly qualified core-only fixture reproduces
a real defect.

Forbidden: Runtime, Runtime.Abstractions, GamePackage, Domain, feature catalogs, ProceduralGameKernel,
GeneratedPackageMvp, source sidecars, Unity, standalone/RC implementation, cached host and retained
Goal169/169A/169B/169C outputs.

Scope violations=0.

## 17. Publication

GREEN message:

```text
GREEN Goal 169D qualified core only portable truth and gate closure
```

or honest BLOCKED/FAILED.

Final:

```text
one commit from 72f69be12b898583d902237ae99e5bc1fe890d2c
push origin/main
HEAD==origin/main
clean worktree
three task files tracked
Goal169C immutable publication unchanged
Goal169D Player smoke=0
Unity starts/builds=0
host/Goal142/Goal148/source sidecars unchanged
accepted=false
no human gate
```

GREEN requires the root cause, a real qualified core-only build, exact portable campaign truth,
no false RC readiness, retained Goal169C publication, clean current-state gate, >=30/>=26 tests,
focused regressions, 15+15 evidence, text integrity, scope0 and one pushed commit.

Final report must include model/base, classifications, root cause, qualified build/history/package,
branch/event truth, portable hashes/pointer, RC, retained publication, clean gate, Player/Unity counts,
tests/evidence/scope, SHA/message/push, HEAD==origin/main and clean worktree.
