# Goal 155A — Current-Package-Correlated Release Candidate Record Truth Hotfix

## Identity
- Task ID: `goal-155a-current-package-correlated-release-candidate-record-truth-hotfix`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `7084244a67bd863f128a6bfb67d5fa5031bf0832`
- Required base message: `GREEN Goal 155 accepted mechanics release candidate integration and operator readiness`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration
```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Goal155 is largely GREEN. Independent audit found one bounded P1 truth hole in the project-local
release-candidate reader. Fix current package/document/identity correlation without touching Runtime,
FeatureModules, Unity or accepted mechanics.

## Pre-approval
- The owner approved execution by launching this task.
- Produce a concise internal plan; do not ask for confirmation.
- Do not request manual testing.
- Preserve Goal155 product code/evidence except explicit state updates.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No candidate/intermediate commits.

## Initial worktree
After unpack, only these untracked files are allowed:
```text
docs/agent-tasks/goal-155a-current-package-correlated-release-candidate-record-truth-hotfix/GOAL.md
docs/agent-tasks/goal-155a-current-package-correlated-release-candidate-record-truth-hotfix/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-155a-current-package-correlated-release-candidate-record-truth-hotfix/README.md
```
Require HEAD==origin/main==`7084244a67bd863f128a6bfb67d5fa5031bf0832`, branch main, no other dirt. No reset/stash/merge/rebase.

## Budgets
```text
Unity Editor: 0
Unity host build: 0
standalone smoke: 0
visible standalone launch: 0
```

## Accepted state to preserve
Goal154-family human acceptance is correct and immutable. Goal155 remains implementation GREEN,
candidate GREEN_ACCEPTABLE_CANDIDATE, accepted=false, no human gate.

## P1 independent-audit finding
`GameProjectReleaseCandidateRecordService.Read()` validates record schema/status/internal hashes,
standalone/payload hashes and semantic authoring fingerprint, but it does not correlate the record
with:
```text
current <project>/package.json SHA-256
document.LastActivatedProjectPackageSha256
document.LastCompositionPackageSha256
document.LastQualifiedFinalStateHash
current project identity
```
The semantic authoring fingerprint deliberately excludes build/package/final hashes. Therefore an
unchanged authoring document plus a modified/corrupt `package.json` can still yield `CURRENT` and
“RC готов”.

## State model
Separate:
```text
record integrity
current applicability
```
A valid historical record must remain available after a newer build, but it cannot be CURRENT unless
it matches the current activated project.

### CURRENT
All must hold:
```text
record internally valid
record package ID equals current package ID
package.json exists
SHA-256(package.json) == document.LastActivatedProjectPackageSha256
record.PackageSha256 == document.LastActivatedProjectPackageSha256
record.CompositionPackageSha256 == document.LastCompositionPackageSha256
record.FinalStateHash == document.LastQualifiedFinalStateHash
record title/version equal current identity
current authoring fingerprint passed and equals record fingerprint
```

### LAST_SUCCESS
Keep the record and return LAST_SUCCESS when it is valid historical evidence and the current package
is internally consistent with the current document, but:
```text
record build hashes differ from current document
or authoring fingerprint differs
or title/version differs while package ID remains equal
```

### UNKNOWN
Keep record, never CURRENT, when:
```text
current fingerprint cannot be calculated
or current document lacks enough qualified-build identity
```

### Rejected/ABSENT
Reject record without deleting it when:
```text
record package ID belongs to another project
current package.json missing
actual package SHA != document activated-package SHA
record/payload malformed under existing rules
```


## A. Correlated read request
Introduce a typed request or extend `Read()` cleanly:
```text
ProjectFolder
Document
Library
Identity
```
Controller passes its already validated typed identity. Keep paths confined.

## B. Current package verification
For every existing RC record:
```text
packagePath = confined <project>/package.json
```
Require file existence and actual SHA-256.

Rules:
```text
actual package hash != document LastActivatedProjectPackageSha256
  -> Record=null, ABSENT, rc.read.current_package_hash_mismatch

package missing
  -> Record=null, ABSENT, rc.read.current_package_missing
```
Do not modify the record.

## C. Current build-identity correlation
Compare record to document:
```text
PackageSha256            vs LastActivatedProjectPackageSha256
CompositionPackageSha256 vs LastCompositionPackageSha256
FinalStateHash           vs LastQualifiedFinalStateHash
```
Rules:
```text
all equal -> build identity matches
any differ -> valid record remains, status LAST_SUCCESS,
              diagnostic rc.read.record_build_identity_differs_from_current
missing current hashes -> UNKNOWN
```
This preserves:
```text
new GREEN build + old standalone record
=> record LAST_SUCCESS
=> overall BUILD_GREEN_STANDALONE_PENDING
```

## D. Identity correlation
Compare record with current typed identity:
```text
package ID differs -> reject rc.read.project_package_id_mismatch
same package ID but title/version differs -> LAST_SUCCESS
diagnostic rc.read.project_identity_metadata_differs
```
Portable directory copies with unchanged identity remain CURRENT. Paths do not participate.

## E. Controller truth
`Snapshot()` must use the correlated result.

Required:
```text
valid exact record -> RC CURRENT / ready
package tamper -> record rejected; card cannot say ready; causal diagnostic
new successful build with different hashes -> record LAST_SUCCESS; overall pending
saved authoring change -> LAST_SUCCESS
return to authoring values alone cannot restore CURRENT while build hashes differ
portable complete copy -> CURRENT without execution
missing build history + correlated package/document/record -> CURRENT remains supported
```

## F. Behavioral tests
Create >=16 Goal155A tests, >=14 behavioral:

1. valid exact record/document/package/identity -> CURRENT;
2. package byte tamper rejected;
3. missing package rejected;
4. document activated hash differs from actual package rejected;
5. record package hash differs from current document -> LAST_SUCCESS;
6. composition hash differs -> LAST_SUCCESS;
7. final hash differs -> LAST_SUCCESS;
8. missing current document hashes -> UNKNOWN;
9. fingerprint failure -> UNKNOWN;
10. saved authoring change -> LAST_SUCCESS;
11. returning authoring alone cannot produce CURRENT while build hashes differ;
12. title change -> LAST_SUCCESS;
13. version change -> LAST_SUCCESS;
14. package ID mismatch rejected;
15. portable complete copy -> CURRENT without execution;
16. missing build history still permits CURRENT when all other truth correlates;
17. new GREEN build/no standalone -> old record LAST_SUCCESS and overall pending;
18. controller package tamper cannot display RC ready;
19. failed read leaves record bytes unchanged;
20. malformed/payload mismatch regressions remain causal;
21. Goal155 Profile A/B/incomplete regressions remain GREEN;
22. Goal154D/153C/150/149 focused regressions remain GREEN.

Tests invoke actual record/controller/files. Source-string tests do not count.

## G. No smoke
Do not rerun the Goal155 hidden smoke. Keep all smoke env vars unset. Use synthetic/captured fixtures
and immutable existing Goal155 evidence. Do not claim a new smoke result.

## H. Evidence
Create exactly 7 files in each mirrored root:
```text
goal155a-dashboard.json
independent-audit-finding.json
current-package-correlation-proof.json
build-identity-status-matrix.json
controller-rc-truth-proof.json
artifact-scope-proof.json
goal155a-report.md
```
Procedural/export twins byte-identical.

Dashboard:
```text
status
goal155aTestsDiscovered
goal155aBehavioralTestsPassed
validRecordCurrentPassed
packageTamperRejected
missingPackageRejected
documentPackageMismatchRejected
olderBuildRecordLastSuccessPassed
identityMismatchRulesPassed
portableCopyCurrentPassed
historyIndependentCurrentPassed
controllerCannotShowReadyForTamperedPackage
recordBytesPreservedOnFailure
goal155RegressionPassed
goal154dRegressionPassed
goal153cRegressionPassed
goal150RegressionPassed
goal149RegressionPassed
unityProcessStartCount=0
standaloneSmokeInvocationCount=0
artifactScopeViolationCount
goal155AuditBlockerClosed=true
goal155CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal155Accepted=false
```


## I. State/docs
Update:
```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/goal155-accepted-mechanics-release-candidate.md
docs/manual-acceptance/goal155a-current-package-correlated-rc-record.md
```

Record:
```text
goal155IndependentAuditResult=BLOCKED_AT_7084244a
goal155IndependentAuditBlocker=rc_record_not_correlated_with_current_package_and_document
goal155AuditBlocker=closed_by_goal155a

goal155ImplementationStatus=GREEN
goal155CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal155Accepted=false
goal155ManualReviewRequired=false
goal155ManualGateReady=false

goal155aImplementationStatus=GREEN
goal155aAccepted=false
goal155aAcceptedByHuman=false
goal155aAcceptedByCodex=false
goal155aManualReviewRequired=false
goal155aManualGateReady=false
goal155aIndependentAuditRequired=true

goal155RcCurrentPackageCorrelationPassed=true
goal155RcCurrentDocumentCorrelationPassed=true
goal155RcIdentityCorrelationPassed=true
goal155RcTamperRejectionPassed=true
goal155aUnityProcessStartCount=0
goal155aStandaloneSmokeInvocationCount=0
nextAction=independent_goal155a_audit_and_select_next_major_product_vertical_slice
```
Preserve Goal154 acceptance exactly.

## J. Artifact scope
Initially allowed:
```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal155a-rc-record-truth.ps1
.devflow/scripts/run-goal155a-rc-record-truth.cmd

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectReleaseCandidateRecordService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs

tests/LLMGameCreator.Tests/Application/Goal155A/Goal155ACurrentPackageCorrelationTests.cs
tests/LLMGameCreator.Tests/Application/Goal155A/Goal155ABuildIdentityStatusTests.cs
tests/LLMGameCreator.Tests/Application/Goal155A/Goal155AControllerTruthTests.cs
tests/LLMGameCreator.Tests/Application/Goal155/Goal155ReleaseCandidateRecordTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/goal155-accepted-mechanics-release-candidate.md
docs/manual-acceptance/goal155a-current-package-correlated-rc-record.md

docs/agent-tasks/goal-155a-current-package-correlated-release-candidate-record-truth-hotfix/
.llmgc/procedural/goal-155a-current-package-correlated-release-candidate-record-truth-hotfix/
.llmgc/exports/goal-155a-current-package-correlated-release-candidate-record-truth-hotfix/
```
One exact extra existing Goal155 test/model path may be added only after a concrete compile/test
failure and with recorded reason.

Forbidden:
```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.Application/Design/FeatureModuleComposition/**
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/**
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/**
src/LLMGameCreator.WinForms/**
catalogs/feature-modules/**
unity/**
samples/**
public GamePackage schema
```
No UI change should be necessary; controller truth drives the existing card.

## K. Validation
```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal155A"
# require >=16 total and >=14 behavioral

dotnet test ... --filter "FullyQualifiedName~Goal155A"
dotnet test ... --filter "FullyQualifiedName~Goal155"
dotnet test ... --filter "FullyQualifiedName~Goal154D"
dotnet test ... --filter "FullyQualifiedName~Goal153C"
dotnet test ... --filter "FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronization"
dotnet test ... --filter "FullyQualifiedName~Goal149"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleLibrary"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleCertification"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```
Ensure all smoke flags unset. Run artifact scope last.

Do not run full suite, 85-case closure, all-ProductSmoke, standalone smoke or Unity.

## L. Budget
```text
read-first/reproduction: max 7 files / 5 minutes
record correlation implementation: 9 minutes
behavioral tests: 12 minutes
focused regressions: 8 minutes
evidence/docs/artifact scope: 8 minutes
target wall clock: 32 minutes
maximum two testhost
Unity process count: 0
```
No unchanged command repetition or timeout escalation.

## M. Publication
Create exactly one final commit:
```text
GREEN Goal 155A current-package-correlated release candidate record truth hotfix
```
or honest BLOCKED/FAILED. Standard push by Codex.

Required:
```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three task files tracked
Unity count=0
standalone smoke count=0
Goal154 acceptance unchanged
Goal155 accepted=false
no human gate
```

## N. GREEN criteria
```text
valid exact record CURRENT
package tamper/missing package cannot produce CURRENT/RC ready
actual package bytes correlate with document activated hash
older valid record remains LAST_SUCCESS after new build
composition/final/document differences prevent CURRENT
identity correlation truthful
portable copy CURRENT
history-independent valid record supported
failed reads preserve record bytes
Goal155/Profile A/Profile B/incomplete regressions GREEN
Goal154D/153C/150/149 regressions GREEN
no Unity/smoke
7+7 evidence mirrored
artifact scope 0
Goal155 audit blocker closed
one final commit pushed
```

## O. Final report
Return GREEN/BLOCKED/FAILED with model/reasoning, exact P1 reproduction, status matrix, test counts,
package/document/identity correlation, controller behavior, portable/history-independent result,
regressions, zero smoke/Unity, evidence/artifact scope, state flags, final SHA/push/HEAD/worktree and
confirmation no human gate was created.
