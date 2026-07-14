# Goal 154C2 — Authoring Fingerprint, Evidence & Standalone Payload Final Closure

## Identity

- Task ID: `goal-154c2-authoring-fingerprint-evidence-and-standalone-payload-final-closure`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `d791a1196234e1ea527bfaabe329163806e5d24d`
- Required base message: `FAILED Goal 154C1 persisted social result real project and cached standalone closure`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Reason: the social Runtime core, typed projection, persisted history reader, WinForms card, disposable
project lifecycle and one cache-only standalone smoke already exist. This is the final bounded
truth/publication closure: distinguish current saved authoring from the last qualified authoring,
prove custom standalone facts without a second smoke, publish evidence/current-state and run
artifact scope.

## Pre-approval

- The owner approved execution by launching this task.
- Produce a concise internal plan; do not ask for confirmation.
- Do not request intermediate human testing.
- Preserve the useful Goal154C/154C1 foundation; do not restart or redesign the feature.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No candidate/intermediate commits.
- Codex performs commit and push itself.

## Mandatory orientation

Read:

```text
AGENTS.md
docs/GOAL_DESIGN_QUALITY_POLICY.md
docs/UNITY_EXECUTION_POLICY.md
docs/CURRENT_GENERATOR_STATE.md
Goal154B/B1/C/C1 task files and reports
```

Before edits write a compact final-closure audit:

```text
inherited product code retained
current saved-vs-qualified truth gap
authoring fingerprint contract
history migration behavior
custom standalone payload proof
evidence/publication gaps
manual gate
```

Any unanswered item blocks GREEN.

## Expected initial worktree

After the owner unpacks this ZIP, the only permitted untracked files are:

```text
docs/agent-tasks/goal-154c2-authoring-fingerprint-evidence-and-standalone-payload-final-closure/GOAL.md
docs/agent-tasks/goal-154c2-authoring-fingerprint-evidence-and-standalone-payload-final-closure/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-154c2-authoring-fingerprint-evidence-and-standalone-payload-final-closure/README.md
```

Required:

```text
HEAD == origin/main == d791a1196234e1ea527bfaabe329163806e5d24d
tracked diff count=0
staged diff count=0
unknown dirty/untracked path count=0
```

The three task files are explicitly authorized and must be committed.
Any other dirty path blocks the task. Do not use destructive cleanup.

## Unity/standalone budget

```text
Unity Editor invocation budget: 0
Unity host build budget: 0
hidden standalone smoke budget: exactly 1
visible automated standalone launch budget: 0
custom-payload proof smoke budget: 0
```

Do not modify `unity/**`.
The existing generic host cache must be reused. If unavailable/invalid, publish BLOCKED; do not start
Unity.

## Current state

Preserve:

```text
Goals153/153A/153B/153C accepted by human
Goal154 historical FAILED foundation
Goal154A historical FAILED partial closure
Goal154B GREEN Runtime core
Goal154B1 GREEN semantics hotfix
Goal154C historical FAILED product-surface foundation
Goal154C1 historical FAILED persisted-history/real-project foundation
Goal154 family human-unaccepted
Goal154 manualGateReady=false
```

Goal154C2 remains human-unaccepted until explicit owner acceptance.

## Independent audit findings

### Useful inherited foundation

Goal154C/C1 already provide:

```text
typed GameProjectSocialSummary
data-driven SocialRuntimeReviewProjectionService
build/workspace/build-history propagation
WinForms social card
standalone HumanReviewFacts
hash-validated GREEN history reader
fresh-controller card recovery
ChoiceText from package data
causal missing/ambiguous quest-resource diagnostics
locked facts without repeat-reward row
18/18 Goal154C1 behavioral tests
disposable goal148-manual lifecycle
default 0→10 and gold 0→10→17
custom reward 9 -> 19
locked final gold 10
invalid 101 rollback
one cache-only hidden smoke, 5/5, Unity processes 0
six earlier task-pack files tracked
```

Retain these changes.

### P1 — saved unbuilt authoring can masquerade as the current qualified result

Current behavior:

1. A GREEN build stores package/composition/final hashes and Social in build history.
2. `SetParameterValue()` or `SetModuleSelected()` changes authoring and marks it dirty.
3. `Save()` persists the changed authoring and clears `Dirty`.
4. The last successful hashes remain unchanged.
5. On fresh reopen, the history reader matches those hashes and restores the old Social.
6. WinForms adds “последняя успешная проверка” only when `snapshot.Dirty=true`.

Therefore:

```text
build reward=7 -> Social gold 17
change reward=9
save without build
close/reopen
Dirty=false
old Social gold 17 is shown without a last-success warning
```

The card is valid as historical last-success evidence, but it is falsely presented as current.

### Missing publication

Goal154C1 did not commit:

```text
nine mirrored evidence files
current-state/docs updates
artifact-scope policy/run
new evidence text-integrity scan
Goal153C regression result
custom reward standalone request/payload proof
```

These are required before manual gate.

## Product result

After GREEN:

```text
last qualified authoring matches current saved authoring
  -> normal “Социальные последствия” card

current saved authoring differs from last qualified authoring
  -> “Социальные последствия — последняя успешная проверка”
  -> old result remains visible but cannot masquerade as current

change values back to the qualified semantic configuration
  -> match becomes true again, regardless of revision/timestamps
```

Default manual result remains:

```text
Reputation 0→10
Gold 0→10→17
Trusted reward +7
Claimed
```


## A. Semantic authoring fingerprint

Create a reusable Application-layer service, naming flexible:

```text
FeatureModuleAuthoringFingerprintService
```

Inputs:

```text
FeatureModuleCompositionDocument
FeatureModuleLibrarySnapshot
```

Output:

```text
Passed
Sha256
Diagnostics[]
CanonicalAuthoringJson optional evidence-only
```

### A1. Included semantic inputs

Include only inputs that determine the selected composition:

```text
BaseCandidateId
explicit SelectedModuleIds sorted
required + selected current module fingerprints sorted
effective parameter values after default resolution and validation
parameter identity:
  moduleId
  parameterId
  valueType
  normalized semantic value
```

Do not include:

```text
Revision
CreatedAtUtc
UpdatedAtUtc
last/previous package hashes
qualification status
display name/description
project folder
build attempt IDs
```

### A2. Effective parameters

Use the existing generic parameter validator/binding vocabulary.

Required semantic normalization:

```text
integer: invariant canonical integer
number: invariant canonical decimal without culture-dependent formatting
boolean: true|false lowercase
enum/string: exact ordinal string
```

An omitted explicit value and an explicitly stored default value must produce the same fingerprint.

Reordering selected modules, parameter records or module-fingerprint dictionary entries must not
change the fingerprint.

### A3. Library changes

Use current fingerprints for:

```text
all required modules
all explicitly selected modules
```

An unrelated unselected optional module change must not change the authoring fingerprint.

A selected module or required-core fingerprint change must change it.

### A4. Failure

Return causal diagnostics for:

```text
unknown selected module
invalid parameter value
duplicate parameter identity
unselected-module parameter
missing current module fingerprint
unsupported parameter value type
```

Do not throw for ordinary invalid authoring.

## B. Persist qualified-authoring identity

Add backward-compatible fields:

```text
GameProjectBuildResult.QualifiedAuthoringFingerprint
GameProjectBuildHistoryEntry.QualifiedAuthoringFingerprint
UnifiedGameProjectWorkspaceSnapshot.SocialMatchesCurrentConfiguration
UnifiedGameProjectWorkspaceSnapshot.SocialConfigurationStatus
```

Recommended statuses:

```text
CURRENT
LAST_SUCCESS
UNKNOWN
ABSENT
```

Do not bump the public GamePackage schema.

A build-history schema bump is not required for an additive internal field. Older entries with an
empty fingerprint remain readable but can never claim `CURRENT`; they restore only as
`LAST_SUCCESS/UNKNOWN`.

### B1. Successful build

In `GameProjectBuildAndQualificationService`:

1. compute the semantic fingerprint from the exact saved authoring used for the successful build;
2. require `Passed=true`;
3. write the SHA into the build result;
4. write the same SHA into the GREEN build-history entry;
5. never write a qualified fingerprint for a failed attempt.

### B2. History reader

`GameProjectBuildHistoryReader` still selects the newest hash-matching valid GREEN Social result.

It also:

```text
computes current saved-authoring fingerprint using the current library
reads entry QualifiedAuthoringFingerprint
returns MatchesCurrentConfiguration
returns CurrentAuthoringFingerprint
returns QualifiedAuthoringFingerprint
```

Rules:

```text
equal nonempty fingerprints -> CURRENT
different nonempty fingerprints -> LAST_SUCCESS
missing old entry fingerprint -> UNKNOWN, never CURRENT
current fingerprint failure -> UNKNOWN plus diagnostics
```

Do not reject a valid historical card merely because current authoring differs. Restore it truthfully
as last success.

Require `entry.Social.CheckpointReplayPassed=true` and
`entry.Social.FullReplayEquivalent=true` in addition to entry-level replay/binding guards.

### B3. Controller

For an in-memory successful build:

```text
compare current semantic fingerprint with build.QualifiedAuthoringFingerprint
```

For persisted recovery use the history-reader result.

After:

```text
parameter change
module selection change
save
fresh reopen
```

the social card remains visible but status is `LAST_SUCCESS`.

Changing authoring back to the same semantic values restores `CURRENT` without rebuilding, even when
revision/timestamps differ.

### B4. WinForms

Use `SocialConfigurationStatus`, not `Dirty`, for the card heading.

```text
CURRENT:
  Социальные последствия

LAST_SUCCESS or UNKNOWN:
  Социальные последствия — последняя успешная проверка
```

`ABSENT` hides the card.

Dirty state may remain separately visible in existing authoring status.

## C. Behavioral truth matrix

Create at least 14 Goal154C2 tests; at least 11 must be behavioral.

Required:

1. fingerprint deterministic under reordered modules/parameters/dictionaries;
2. omitted default equals explicitly stored default;
3. revision/timestamp/hash/status changes do not affect fingerprint;
4. integer/number/boolean/enum values normalize invariantly;
5. parameter change changes fingerprint;
6. selected module change changes fingerprint;
7. required/selected module fingerprint change changes fingerprint;
8. unrelated unselected optional module change does not change fingerprint;
9. invalid/duplicate/unselected parameters fail causally;
10. successful build result/history carry identical nonempty fingerprint;
11. history with matching fingerprint returns `CURRENT`;
12. saved reward 7→9 without build then fresh reopen returns old gold 17 with `LAST_SUCCESS`;
13. WinForms heading says last successful after saved-unbuilt reopen;
14. change 9→7 and save/reopen returns `CURRENT` without rebuilding;
15. revision-only save with no semantic change remains `CURRENT`;
16. old history without fingerprint returns `UNKNOWN`, never current;
17. mismatched current library selected-module fingerprint returns last-success/unknown;
18. failed invalid attempt preserves the qualified fingerprint and current social result;
19. Social replay flags false cause history entry rejection;
20. default real project build/reopen remains `CURRENT`;
21. custom reward 9 after successful build becomes `CURRENT` with gold 19;
22. Goal154C1 persisted/history/projection regressions remain GREEN.

Tests must invoke real fingerprint/history/controller/build/WinForms services. Source-string tests do
not count.

## D. Finish disposable project proof

Reuse the existing Goal154C1 disposable infrastructure.

### D1. Default

Require:

```text
0/10/5/10/7 persists
build/repeat deterministic
fresh reopen SocialConfigurationStatus=CURRENT
gold 0→10→17
```

### D2. Saved-unbuilt custom state

After default GREEN:

```text
change trustedGoldReward 7→9
save
dispose/recreate controller
reopen without build
```

Require:

```text
saved parameter=9
card still shows last-success gold 17
SocialConfigurationStatus=LAST_SUCCESS
heading explicitly last successful
```

Then build:

```text
gold 0→10→19
package/final hashes change
QualifiedAuthoringFingerprint changes
fresh reopen status=CURRENT
```

### D3. Return to prior semantic configuration

Change 9→7, save/reopen without building.

Require:

```text
current fingerprint equals the earlier default qualified fingerprint
status=CURRENT
```

This proves semantic comparison rather than revision comparison.

### D4. Locked and invalid attempt

Preserve:

```text
threshold 20 -> GREEN, gold 10
threshold 101 -> rejected, last success/fingerprint/card preserved across reopen
```

### D5. Source project

The read-only `goal148-manual` manifest remains byte-identical.

## E. Standalone final proof

### E1. One real hidden smoke

Using a default valid `CURRENT` disposable project:

```text
BuildWindowsStandalone
```

Require:

```text
HostReused=true
HostRebuilt=false
Unity process start count=0
exactly one hidden smoke
all five markers GREEN
self-check passed == total
host cache key=6af4d5eb5b42f956110555b58fb4e276 unless actual existing cache truth differs
host executable SHA-256 unchanged before/after
package hash equals normal GREEN build
```

Parse actual:

```text
<output>/<slug>_Data/StreamingAssets/LLMGameCreatorProject/player-adapter-model.json
```

Require actual `humanReviewFacts` include:

```text
Репутация = 0 → 10
Золото = 0 → 10 → 17
Награда за доверие = +7
Повторная награда = недоступна
Социальный итог = награда получена
```

No raw IDs/hashes in those facts.

### E2. Custom payload without second smoke

Inject a capturing `IProjectStandaloneBuildService` into the real workspace controller for the
successfully built reward-9 project.

Call `BuildWindowsStandalone()`; the capturing service:

```text
records ProjectStandaloneBuildRequest
does not invoke Unity
does not assemble/launch a player
returns a clearly test-only captured result
```

Require captured request HumanReviewFacts:

```text
Золото = 0 → 10 → 19
Награда за доверие = +9
```

Also prove:

```text
host-source/settings inputs are unchanged from the default build
therefore the same host cache key applies
no second smoke ran
Unity process count remains 0
```

Do not claim a second real standalone assembly.

## F. Evidence, docs and publication

Update source-of-truth documents:

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
```

Historical evidence roots remain immutable.

Record the sanitized intended meaning of the malformed Goal154B1 report without claiming its bytes
changed.

Run Goal153C focused regression and record the actual result.


## G. Evidence contract

Create exactly 10 files in each mirrored root:

```text
goal154c2-dashboard.json
final-closure-audit.json
authoring-fingerprint-proof.json
saved-unbuilt-last-success-proof.json
real-project-lifecycle-proof.json
winforms-social-truth-proof.json
cached-standalone-default-proof.json
custom-standalone-request-proof.json
artifact-scope-proof.json
goal154c2-report.md
```

Procedural/export twins must be byte-identical.

Dashboard fields:

```text
status
goal154c2TestsDiscovered
goal154c2BehavioralTestsPassed
defaultAuthoringFingerprint
customAuthoringFingerprint
fingerprintsDiffer
savedUnbuiltStatus
returnedToDefaultStatus
defaultReputationBefore
defaultReputationAfter
defaultGoldAfterQuest
defaultGoldAfterClaim
customGoldAfterClaim
lockedFinalGold
persistedCardRecovered
invalidAttemptPreservedLastSuccess
sourceProjectByteIdentical
winFormsCurrentHeadingPassed
winFormsLastSuccessHeadingPassed
hostCacheKey
hostReused
hostRebuilt
hostExecutableHashUnchanged
unityProcessStartCount
hiddenSmokeInvocationCount
hiddenSmokePassed
standaloneSelfChecksPassed
defaultStandaloneFactsPassed
customCapturedRequestFactsPassed
customSecondSmokeInvocationCount
goal153cRegressionPassed
artifactScopeViolationCount
goal154Accepted=false
goal154c2Accepted=false
manualGateReady=true
```

No GREEN field may be:

```text
null
PARTIAL
NOT_EXECUTED
source-string-only assertion
unverified constant
```

### Text integrity

All new task/docs/evidence text must pass:

```text
valid UTF-8
no NUL
no forbidden C0 control characters except CR/LF/TAB
no mojibake markers
no escaped Cyrillic where repository policy forbids it
```

The validation scans actual committed candidate files, not just test literals.

## H. Current-state publication

On GREEN:

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

goal154c2ImplementationStatus=GREEN
goal154c2Accepted=false
goal154c2AcceptedByHuman=false
goal154c2AcceptedByCodex=false
goal154c2ManualReviewPerformed=false
goal154c2ManualGateReady=true

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
goal154HiddenSmokePassed=true
goal154CustomStandaloneRequestFactsPassed=true
nextAction=perform_goal154_combined_human_gate
```

Preserve Goal153-family acceptance.
Do not claim human acceptance for Goal154 family.

## I. Manual gate after independent audit

Exactly four steps:

```text
1. Enable the three social mechanics, set 0/10/5/10/7 and build.
2. Confirm the social card shows reputation 0→10 and gold 0→10→17.
3. Save, close/reopen and confirm the five values and social card remain.
4. Build/launch cached standalone and confirm the same social facts.
```

No manual fingerprint/hash/locked/rollback/event inspection.

## J. Command budget

```text
read-first: maximum 9 primary files
fingerprint/model/history integration: maximum 10 minutes
behavioral tests: maximum 12 minutes
disposable lifecycle and one smoke: maximum 8 minutes
docs/evidence/artifact scope: maximum 10 minutes
total target wall clock: 38 minutes
maximum two testhost processes
Unity process count: 0
```

Rules:

```text
write/discover Goal154C2 tests before long commands
no unchanged command repetition
no timeout escalation loop
no full suite
no 85-case historical closure
no all-ProductSmoke
no Unity host build
no historical evidence rewrite
exactly one hidden smoke
raw logs remain ignored
```

## K. Required validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal154C2"
# require total >=14 and behavioral >=11

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

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Then run:

```text
disposable real-project fingerprint lifecycle
exactly one default hidden standalone smoke
capturing custom standalone request proof with zero smoke
new-text integrity scan
artifact-scope validation last
```

## L. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal154c2-final-social-closure.ps1
.devflow/scripts/run-goal154c2-final-social-closure.cmd

src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleCompositionDocumentModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleAuthoringFingerprintService.cs

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildHistoryReader.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/SocialRuntimeReviewProjectionService.cs

src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs

tests/LLMGameCreator.Tests/Application/Goal154C1/Goal154C1RealProjectLifecycleTests.cs
tests/LLMGameCreator.Tests/Application/Goal154C2/Goal154C2AuthoringFingerprintTests.cs
tests/LLMGameCreator.Tests/Application/Goal154C2/Goal154C2PersistedTruthTests.cs
tests/LLMGameCreator.Tests/Application/Goal154C2/Goal154C2WinFormsTests.cs
tests/LLMGameCreator.Tests/Application/Goal154C2/Goal154C2StandaloneAndEvidenceTests.cs
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal154SocialConsequenceWorkspaceTests.cs
tests/LLMGameCreator.Tests/Application/ProjectStandaloneBuild/Goal154C2SocialPayloadTests.cs

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

docs/agent-tasks/goal-154c2-authoring-fingerprint-evidence-and-standalone-payload-final-closure/
.llmgc/procedural/goal-154c2-authoring-fingerprint-evidence-and-standalone-payload-final-closure/
.llmgc/exports/goal-154c2-authoring-fingerprint-evidence-and-standalone-payload-final-closure/
```

If an exact compile/test failure proves one additional existing Application/WinForms/test path is
required:

```text
record exact reason
add exact path only
do not broaden a subtree
```

Forbidden without a newly reproduced P0/P1:

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


## M. Publication

Create exactly one final commit:

```text
GREEN Goal 154C2 authoring fingerprint evidence and standalone payload final closure
```

or honest:

```text
BLOCKED Goal 154C2 authoring fingerprint evidence and standalone payload final closure
FAILED Goal 154C2 authoring fingerprint evidence and standalone payload final closure
```

Codex performs commit and push itself.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three Goal154C2 task files tracked
Unity process start count=0
HostRebuilt=false
hidden smoke invocation count=1
custom second smoke invocation count=0
Goal154 family human acceptance=false
Goal154 manualGateReady=true only on GREEN
```

A coherent BLOCKED/FAILED result with task-owned changes is still committed and pushed.

## GREEN criteria

```text
initial worktree has only three authorized Goal154C2 task files
Goal154C2 tests discovered >=14 and behavioral >=11
semantic authoring fingerprint is deterministic and value-type aware
omitted default equals explicit default
revision/timestamps/last hashes do not affect fingerprint
selected/required module changes affect fingerprint
unselected optional module changes do not
successful result/history persist nonempty matching fingerprint
history reader restores valid old card but distinguishes CURRENT/LAST_SUCCESS/UNKNOWN
saved reward 7→9 without build reopens as LAST_SUCCESS with old gold 17
WinForms heading truthfully says last successful
successful custom build becomes CURRENT with gold 19
returning to semantic default becomes CURRENT without rebuild
invalid attempt preserves last success and fingerprint
history Social replay guards are validated
default real project lifecycle remains GREEN and source byte-identical
locked outcome remains GREEN at gold 10
one real hidden standalone smoke uses cached host, 5/5, Unity 0
actual default payload contains 0→10→17 social facts
custom captured standalone request contains 0→10→19 and +9
custom second smoke count=0
Goal154B/B1/C/C1 and Goal153C regressions GREEN
new evidence passes control-character/mojibake/UTF-8 scan
ten procedural/export evidence files mirrored byte-identically
current-state/docs/manual files published
artifact scope 0 violations
Goal154 implementation GREEN but human-unaccepted
Goal154C2 manualGateReady=true
one final commit pushed
```

## Final report

Return GREEN, BLOCKED or FAILED and include:

- model/reasoning used;
- initial worktree inventory;
- authoring fingerprint schema and normalization;
- persisted build/history fingerprint values;
- CURRENT/LAST_SUCCESS/UNKNOWN cases;
- saved-unbuilt reward-9 reopen result;
- return-to-default semantic match result;
- WinForms headings;
- Goal154C2 discovered/behavioral test counts;
- default/custom/locked/invalid disposable lifecycle;
- source project immutability;
- host cache key/hash/reuse and Unity process count;
- exactly-one hidden smoke markers/self-checks/default facts;
- custom captured request facts and zero second smoke;
- Goal154B/B1/C/C1 and Goal153C regressions;
- evidence integrity/mirror;
- artifact scope;
- Goal154/154A/154B/154B1/154C/154C1/154C2 flags;
- exact four-step manual gate;
- final SHA/push/HEAD/worktree;
- confirmation no human acceptance was claimed.
