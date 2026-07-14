# Goal 154C1 — Persisted Social Result, Real Project & Cached Standalone Closure

## Identity

- Task ID: `goal-154c1-persisted-social-result-real-project-and-cached-standalone-closure`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `237a48b87f744ef917536244b3306c02154ad367`
- Required base message: `FAILED Goal 154C saved project WinForms and cached standalone social closure`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Reason: Goal154C already published a useful typed social projection, build/workspace propagation,
WinForms card and standalone HumanReviewFacts. This task must preserve that foundation and close
persisted last-success recovery, real saved-project lifecycle, cache-only standalone, evidence and
publication. It must not redesign Runtime.

## Pre-approval

- The owner approved execution by launching this task.
- Produce a concise internal plan; do not ask for confirmation.
- Do not request intermediate human testing.
- Continue from the published FAILED Goal154C foundation; do not revert it or restart the feature.
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
Goal154B, Goal154B1 and Goal154C reports/task files
```

Before edits write a compact closure audit:

```text
useful inherited Goal154C source files
remaining P0/P1 issues
untracked task-pack inventory
persistent last-success source
real-project lifecycle stages
standalone cache contract
manual-review contract
```

Any unanswered item blocks GREEN.

## Expected initial worktree and exact authorization

The previous Codex commit intentionally left the owner-provided Goal154C task pack untracked.

After the owner unpacks this Goal154C1 ZIP, the only permitted pre-existing untracked paths are:

```text
docs/agent-tasks/goal-154c-saved-project-winforms-and-cached-standalone-social-closure/GOAL.md
docs/agent-tasks/goal-154c-saved-project-winforms-and-cached-standalone-social-closure/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-154c-saved-project-winforms-and-cached-standalone-social-closure/README.md

docs/agent-tasks/goal-154c1-persisted-social-result-real-project-and-cached-standalone-closure/GOAL.md
docs/agent-tasks/goal-154c1-persisted-social-result-real-project-and-cached-standalone-closure/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-154c1-persisted-social-result-real-project-and-cached-standalone-closure/README.md
```

These six exact files are task-owned and explicitly authorized for staging and commit.

Required start:

```text
HEAD == origin/main == 237a48b87f744ef917536244b3306c02154ad367
tracked diff count=0
staged diff count=0
unknown dirty/untracked path count=0
```

If any other dirty/untracked path exists:

```text
do not delete/restore it
publish BLOCKED with exact paths
```

Do not use `git clean`, `git reset --hard` or broad cleanup.

## Unity budget

```text
Unity Editor invocation budget: 0
Unity host build budget: 0
cached standalone hidden-smoke budget: exactly 1
visible automated standalone launch budget: 0
```

Do not modify `unity/**`.
Use the existing host cache. If unavailable or invalid, publish BLOCKED; do not start Unity.

## Current state

Preserve:

```text
Goals153/153A/153B/153C accepted by human
Goal154 historical FAILED foundation
Goal154A historical FAILED partial closure
Goal154B GREEN Runtime core
Goal154B1 GREEN product-semantics hotfix
Goal154C historical FAILED product-surface foundation
Goal154 family human acceptance=false
Goal154 manualGateReady=false
```

Goal154C1 remains human-unaccepted.

## Independent audit findings

### Inherited useful foundation

Goal154C already implements:

```text
SocialRuntimeReviewProjectionService
typed GameProjectSocialSummary
GameProjectBuildResult/workspace propagation
build-history Social field
WinForms social card
standalone HumanReviewFacts
14 focused tests, 12 behavioral projection tests
default 0→10 and gold 0→10→17
custom reward 9 -> 19
locked final gold 10
```

Retain and harden these changes.

### P1-A — last successful social card is not restored after reopen

`GameProjectBuildAndQualificationService` writes a GREEN build-history entry including `Social`.

But `UnifiedGameProjectWorkspaceController.OpenProject()` currently executes:

```text
_lastBuild = null
_lastSuccessfulBuild = null
```

and never reads the latest valid GREEN build history.

Therefore:

```text
build GREEN
save
close/reopen
→ social card disappears until another build
```

This violates persisted last-success truth and the accepted workspace pattern separating:

```text
last successful build
current attempt
current saved configuration
```

### P1-B — failed attempt preservation is only in-memory

Within one controller instance, `_lastSuccessfulBuild` survives a failed attempt. Across reopen, it
does not. Invalid-attempt rollback must preserve the persisted last successful social result.

### P2-A — locked human facts are semantically noisy

The locked outcome should not say:

```text
Повторная награда: недоступна
```

because no first reward was claimed.

Locked primary facts should contain:

```text
Репутация
Квест
Доверенная реплика
Золото
Награда за доверие: пока недоступна
Социальный итог
```

The repeat-reward row is shown only for a claimed result.

### P2-B — ChoiceText is empty

Populate `ChoiceText` from the package's data-defined dialogue choice. Do not expose the raw choice
ID in the primary UI.

### P2-C — ambiguous quest-gold projection must be causal

The social projection currently uses `SingleOrDefault()` for the quest-action gold event. More than
one matching event can throw a generic exception.

Require:

```text
exactly one quest-action resource event -> accept
zero -> causal missing diagnostic
more than one -> causal ambiguous diagnostic
```

### P2-D — old Goal154B1 report control character

Do not rewrite historical evidence. Record a sanitized correction in Goal154C1 evidence/current
state and ensure every new text artifact rejects NUL/forbidden controls.

## Product result

The final ordinary workflow:

```text
Игры
→ enable the three social mechanics
→ set 0/10/5/10/7
→ save
→ close/reopen
→ values and last-success social card persist
→ build/repeat deterministic
→ invalid attempt preserves last success
→ Windows standalone uses cached host
→ same social facts appear in standalone
```

Default primary facts:

```text
Репутация: 0 → 10
Квест: завершён
Доверенная реплика: недоступна → доступна → получена → недоступна
Золото: 0 → 10 → 17
Награда за доверие: +7
Повторная награда: недоступна
Социальный итог: награда получена
```

Locked automated facts:

```text
Репутация: 0 → 10
Квест: завершён
Доверенная реплика: недоступна
Золото: 0 → 10
Награда за доверие: пока недоступна
Социальный итог: порог репутации ещё не достигнут
```


## A. Persisted last-success social recovery

Create a confined build-history reader/service, naming flexible:

```text
GameProjectBuildHistoryReader
```

Input:

```text
project folder
current FeatureModuleCompositionDocument
```

Read only:

```text
<project>/.llmgc/build-history/*.json
```

### Selection contract

Select the newest entry satisfying all:

```text
schemaVersion supported
Status=GREEN
AttemptStatus=GREEN
Social.Present=true
Social.Passed=true
PackageSha256 equals current document LastActivatedProjectPackageSha256
CompositionPackageSha256 equals current document LastCompositionPackageSha256
FinalStateHash equals current document LastQualifiedFinalStateHash
CheckpointReloadPassed=true
FullReplayEquivalent=true
ActionBindingPassed=true
```

If more than one entry has the same timestamp/name ordering ambiguity, use deterministic filename
ordinal order after `CompletedAtUtc`.

Never trust a social summary whose hashes do not match the current persisted last-success identity.

### Safety

```text
all paths confined under project
invalid JSON -> diagnostic, skip entry
unsupported schema -> diagnostic, skip entry
FAILED history entry -> never used as last success
Social Passed=false -> never used
missing history -> no card, not a crash
stale hash -> no card and causal technical diagnostic
```

Do not alter history files while reading.

### Controller integration

On `OpenProject()`:

```text
_lastBuild=null
_lastSuccessfulBuild=<validated persisted GREEN history projection or null>
```

A minimal `GameProjectBuildResult` may be reconstructed from the matching history, or the controller
may keep a separately typed persisted-last-success record. Do not fake fields that are unavailable.

`Snapshot().Social` priority:

```text
current in-memory successful build Social
then persisted matching last-success Social
else null
```

After a successful build, update in-memory and persisted truth.
After a failed attempt, preserve both.

### Dirty configuration semantics

When the user changes parameters after a successful build:

```text
social card remains explicitly the last successful result
current configuration remains separately visible/dirty
```

The card must not silently represent unbuilt current values. Add a label or header suffix:

```text
Социальные последствия — последняя успешная проверка
```

when `snapshot.Dirty=true`.

## B. Harden typed social projection

Retain the inherited data-driven implementation.

Required corrections:

1. Populate `ChoiceText` from the exact package dialogue/node/choice resolved through the contract.
2. Quest-action resource-event count:
   ```text
   exactly 1 -> use
   0 -> social.projection.quest_resource_transition_missing
   >1 -> social.projection.quest_resource_transition_ambiguous
   ```
3. Claim-action resource-event count remains action-scoped and causal.
4. Claimed human facts include:
   ```text
   Награда за доверие: +N
   Повторная награда: недоступна
   ```
5. Locked human facts include:
   ```text
   Награда за доверие: пока недоступна
   ```
   and omit the repeat-reward row.
6. `CheckpointReplayPassed` and `FullReplayEquivalent` must be true for a `Passed=true` complete social
   projection. A build has global replay guards, but the typed record must remain independently
   truthful.
7. Partial social dependency closures remain valid with `Present=false`, not failed.
8. Ambiguous/missing complete social contracts fail causally.

Do not add starter-content IDs to generic branches.

## C. Real saved-project lifecycle

Use read-only source:

```text
%LOCALAPPDATA%\LLMGameCreator\Games\goal148-manual
```

### C1. Source immutability

Before any Application service opens a copy:

```text
capture exact source manifest:
relative path
size
SHA-256
```

After all proofs require byte-identical equality and mutation count 0.

Never open the source project itself for write.

### C2. Disposable default project

Create a short LocalAppData copy:

```text
%LOCALAPPDATA%\LLMGameCreator\Goal154C1\default\
```

On the copy:

```text
open project
enable three social modules
set 0/10/5/10/7
save authoring
dispose/close controller and current package services
recreate fresh services/controller
reopen project
verify all five values exactly
build GREEN
repeat build without changes
```

Require:

```text
project identity preserved
package ID/title/version unchanged
source copy's activated package has no proof/dummy content
baseline healer quest gold remains 10
social result 0→10 and gold 0→10→17
checkpoint/full replay/action binding GREEN
repeat package/composition/final hashes identical
```

### C3. Persisted card recovery

After the first GREEN build:

```text
dispose controller
recreate controller
reopen copy
do not build
```

Require:

```text
Snapshot().Social present/passed
facts equal previous GREEN build
card source is matching GREEN build-history entry
no Runtime qualification rerun needed
```

Then create a FAILED invalid attempt and repeat reopen. The same last-success social result must still
load.

### C4. Custom reward

On a separate disposable copy or a restored default copy:

```text
trustedGoldReward 7→9
save
close/reopen
build
```

Require:

```text
value persists
package hash changes
final state hash changes
gold 0→10→19
trusted delta 9
other social facts unchanged
```

### C5. Locked outcome

On a separate copy:

```text
threshold=20
```

Require GREEN:

```text
reputation 0→10
gold 0→10
rewardClaimed=false
socialOutcome=still_locked
locked human facts contain no repeat-reward row
```

### C6. Invalid attempt rollback

From a valid claimed copy attempt:

```text
trustedReputationThreshold=101
```

Require:

```text
rejected at parameter/authoring stage
last successful package/composition/final hashes unchanged
persisted last-success social result unchanged
activated package not partially replaced
diagnostic contains parameter ID and allowed range
```

Restore valid persisted configuration before final default standalone proof.

## D. WinForms social card closure

Retain the inherited social panel and make it a true last-success card.

Required claimed display:

```text
Социальные последствия
Репутация                  0 → 10
Квест                      завершён
Доверенная реплика         недоступна → доступна → получена → недоступна
Золото                     0 → 10 → 17
Награда за доверие         +7
Повторная награда          недоступна
Социальный итог            награда получена
```

Required locked display omits the repeat-reward row.

Requirements:

```text
visible only for Present+Passed social result
readable at normal 1100x720 project page
word wrap/no clipping for the dialogue row
no module/choice/faction IDs
no hashes
no duplicate technical block
dirty configuration clearly marks card as last successful
failed attempt does not hide or replace card
fresh controller reopen restores the card from history
```

Add behavioral WinForms tests using actual `ProjectsPageControl` binding or an internal typed formatter
used by the real control. Source-string-only tests do not count.

## E. Standalone facts and cached proof

The inherited controller already appends typed social HumanReviewFacts.

Harden and prove:

```text
claimed facts exactly reflect 0→10 and 0→10→17
locked facts omit repeat reward
custom reward facts reflect 0→10→19
no raw IDs/hashes
```

### E1. Default standalone build

Using the valid default disposable project:

```text
BuildWindowsStandalone
```

Require:

```text
normal project qualification GREEN
HostReused=true
HostRebuilt=false
Unity process start count=0
host executable hash unchanged
one hidden smoke only
all five LLMGC_PROJECT_STANDALONE_* markers GREEN
self-check passed count == total count
payload game-package hash equals normal build
payload humanReviewFacts include social default facts
payload contains no proof fixture
```

Do not press the visible Launch button.

### E2. Custom payload without second smoke

For custom reward 9, prove the generated `ProjectStandaloneBuildRequest.HumanReviewFacts` or
assembled payload contains `0→10→19` while using the same host key/hash.

Do not run a second standalone smoke. Hidden smoke budget is exactly one.

## F. Evidence and source-of-truth

Commit the six exact task-pack files authorized at task start.

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
```

Historical Goal154B1 evidence remains untouched.

Record sanitized correction:

```text
historical Goal154B1 report intended:
Gold: baseline 10; default 0 -> 10 -> 17
resource_transition_truthful reads only declaring-action events
```

Do not claim the historical bytes were corrected.


## G. Required tests

Create at least 16 Goal154C1 tests; at least 12 must be behavioral.

A behavioral test invokes real history/controller/build/WinForms/standalone services and asserts
resulting data/files/process plans.

Required:

1. history reader loads newest matching GREEN social entry;
2. history reader skips FAILED, malformed, stale-hash and Social.Passed=false entries;
3. history reader handles missing history without crash;
4. OpenProject restores last-success social result before rebuild;
5. dirty parameter change preserves card but marks it last-success;
6. failed invalid attempt preserves social result in-memory;
7. failed invalid attempt preserves social result after fresh-controller reopen;
8. projection populates actual ChoiceText;
9. projection rejects zero/multiple quest gold events causally;
10. claimed replay flags required for Passed=true;
11. locked facts omit repeat-reward row;
12. WinForms claimed card visible/readable/no IDs/hashes;
13. WinForms locked card omits repeat reward;
14. WinForms failed-attempt/reopen preserves card;
15. disposable default project save/reopen/build/repeat;
16. custom reward 9 changes hashes and facts;
17. locked project remains GREEN;
18. invalid 101 rollback preserves last success;
19. source project manifest byte-identical;
20. default standalone host reuse with zero Unity starts;
21. hidden smoke has five markers and social facts;
22. custom standalone request/payload facts use same host and 19 gold;
23. new evidence text rejects NUL/forbidden controls;
24. Goal154B/B1 core regressions remain GREEN;
25. no-social/default-off historical results unchanged.

Historical Goal154C projection tests may remain, but source-contract-only tests do not count toward the
12 behavioral minimum.

## H. Evidence

Create exactly 9 files in each mirrored root:

```text
goal154c1-dashboard.json
closure-audit.json
persisted-last-success-proof.json
real-project-lifecycle-proof.json
winforms-social-card-proof.json
cached-standalone-social-proof.json
rollback-and-projection-hardening-proof.json
artifact-scope-proof.json
goal154c1-report.md
```

Procedural/export twins must be byte-identical.

Dashboard fields:

```text
status
goal154c1TestsDiscovered
behavioralTestsPassed
defaultValuesPersisted
defaultReputationBefore
defaultReputationAfter
defaultGoldBefore
defaultGoldAfterQuest
defaultGoldAfterClaim
customGoldAfterClaim
lockedFinalGold
persistedCardRecoveredBeforeRebuild
invalidAttemptPreservedLastSuccess
sourceProjectByteIdentical
winFormsClaimedCardPassed
winFormsLockedCardPassed
hostCacheKey
hostReused
hostRebuilt
hostExecutableHashUnchanged
unityProcessStartCount
hiddenSmokeInvocationCount
hiddenSmokePassed
standaloneSelfChecksPassed
standaloneSocialFactsPassed
artifactScopeViolationCount
goal154Accepted=false
goal154c1Accepted=false
manualGateReady=true
```

No GREEN field may be null, `PARTIAL`, `NOT_EXECUTED` or copied from a constant without behavioral
source.

All new text artifacts:

```text
valid UTF-8
no NUL
no forbidden C0 control characters except CR/LF/TAB
no mojibake markers
no escaped Cyrillic where repository policy forbids it
```

## I. Current-state publication

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

goal154c1ImplementationStatus=GREEN
goal154c1Accepted=false
goal154c1AcceptedByHuman=false
goal154c1AcceptedByCodex=false
goal154c1ManualReviewPerformed=false
goal154c1ManualGateReady=true

goal154DefaultValues=0/10/5/10/7
goal154DefaultReputation=0->10
goal154DefaultGold=0->10->17
goal154DefaultSocialOutcome=claimed
goal154LockedFinalGold=10
goal154PersistedCardRecoveryPassed=true
goal154SavedProjectLifecyclePassed=true
goal154WinFormsSocialCardPassed=true
goal154HostReused=true
goal154HostRebuilt=false
goal154UnityProcessStartCount=0
goal154HiddenSmokePassed=true
nextAction=perform_goal154_combined_human_gate
```

Preserve Goal153-family acceptance.
Do not claim human acceptance for Goal154 family.

## J. Manual gate after independent audit

Exactly:

```text
1. Enable the three social mechanics, set 0/10/5/10/7 and build.
2. Confirm the social card shows reputation 0→10 and gold 0→10→17.
3. Save, close/reopen and confirm the five values and last-success card remain.
4. Build/launch cached standalone and confirm the same social facts.
```

No manual hashes, IDs, replay events, locked case or rollback inspection.

## K. Command budget

```text
read-first: maximum 10 primary files
history/projection hardening: maximum 8 minutes
WinForms tests/fixes: maximum 5 minutes
real-project lifecycle: maximum 9 minutes
cached standalone smoke: maximum 4 minutes
focused regressions/evidence/artifact scope: maximum 9 minutes
total target wall clock: 35 minutes
maximum two testhost processes
Unity process count: 0
```

Rules:

```text
write/discover Goal154C1 tests before long commands
no unchanged command repetition
no timeout escalation loop
no full suite
no 85-case historical closure
no all-ProductSmoke
no Unity host build
no historical evidence rewrite
raw logs remain ignored
```

## L. Required validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal154C1"
# require total >=16 and behavioral >=12

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

Then run the disposable project lifecycle and exactly one hidden standalone smoke.

Run artifact-scope validation last.


## M. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal154c1-persisted-social-closure.ps1
.devflow/scripts/run-goal154c1-persisted-social-closure.cmd

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/SocialRuntimeReviewProjectionService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildHistoryReader.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs

src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/Goal154C/Goal154CSocialProjectionTests.cs
tests/LLMGameCreator.Tests/Application/Goal154C1/Goal154C1PersistedLastSuccessTests.cs
tests/LLMGameCreator.Tests/Application/Goal154C1/Goal154C1RealProjectLifecycleTests.cs
tests/LLMGameCreator.Tests/Application/Goal154C1/Goal154C1WinFormsSocialCardTests.cs
tests/LLMGameCreator.Tests/Application/Goal154C1/Goal154C1StandaloneSocialTests.cs
tests/LLMGameCreator.Tests/Application/Goal154C1/Goal154C1EvidenceIntegrityTests.cs
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal154SocialConsequenceWorkspaceTests.cs
tests/LLMGameCreator.Tests/Application/ProjectStandaloneBuild/Goal154C1SocialStandaloneTests.cs

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

docs/agent-tasks/goal-154c-saved-project-winforms-and-cached-standalone-social-closure/
docs/agent-tasks/goal-154c1-persisted-social-result-real-project-and-cached-standalone-closure/
.llmgc/procedural/goal-154c1-persisted-social-result-real-project-and-cached-standalone-closure/
.llmgc/exports/goal-154c1-persisted-social-result-real-project-and-cached-standalone-closure/
```

If exact compilation/test failure proves one additional existing Application/WinForms/test path is
required:

```text
record exact reason
add exact path only
do not broaden a subtree
```

Forbidden without a newly reproduced P0/P1 and explicit evidence:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
catalogs/feature-modules/**
src/LLMGameCreator.Application/Design/FeatureModuleComposition/**
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/**
unity/**
samples/**
generator-library/**
provider/**
LLM/**
RAG/**
public GamePackage schema changes
```

The existing host cache and user project source are external runtime inputs, never committed.

## N. Publication

Create exactly one final commit:

```text
GREEN Goal 154C1 persisted social result real project and cached standalone closure
```

or honest:

```text
BLOCKED Goal 154C1 persisted social result real project and cached standalone closure
FAILED Goal 154C1 persisted social result real project and cached standalone closure
```

Codex performs commit and push itself.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
the six task-pack files are tracked
Unity process start count=0
HostRebuilt=false
Goal154 family human acceptance=false
Goal154 manualGateReady=true only on GREEN
```

A coherent BLOCKED/FAILED result with task-owned changes is still committed and pushed.

## GREEN criteria

```text
initial worktree contains only six authorized task files
Goal154C1 tests discovered >=16 and behavioral >=12
persisted last-success reader is confined and hash-validated
fresh controller reopen restores social card before rebuild
failed invalid attempt preserves persisted social result across reopen
dirty config marks card as last successful
ChoiceText comes from package data
quest resource event ambiguity is causal
locked facts omit repeat-reward row
claimed facts remain 0→10 and 0→10→17
WinForms card readable/no IDs/hashes
disposable default project saves/reopens five values
repeat build deterministic
custom reward 9 changes hashes and facts to 19
locked outcome remains GREEN at gold 10
invalid 101 attempt rolls back without replacing last success
source goal148-manual byte-identical
existing host cache reused
Unity process start count=0
exactly one hidden standalone smoke passes five markers/self-checks
standalone social facts match default
custom standalone payload facts use 19 without second smoke
Goal154B/B1 and Goal153 regressions GREEN
new evidence has no forbidden control characters
procedural/export evidence byte-identical
artifact scope 0 violations
Goal154 implementation GREEN but human-unaccepted
Goal154C1 manualGateReady=true
six task-pack files committed
one final commit pushed
```

## Final report

Return GREEN, BLOCKED or FAILED and include:

- model/reasoning used;
- initial dirty/untracked inventory;
- committed old/new task-pack files;
- closure audit;
- Goal154C1 discovered/behavioral test counts;
- persisted history selection and reopen result;
- WinForms claimed/locked/dirty card facts;
- default save/reopen/build/repeat result;
- custom reward 9 result;
- locked outcome;
- invalid-attempt rollback across reopen;
- source project immutability;
- host cache key/hash/reuse and Unity process count;
- hidden standalone markers/self-checks/social facts;
- Goal154B/B1 and Goal153 regressions;
- evidence integrity/mirror;
- artifact scope;
- Goal154/154A/154B/154B1/154C/154C1 flags;
- exact four-step manual gate;
- final SHA/push/HEAD/worktree;
- confirmation no human acceptance was claimed.
