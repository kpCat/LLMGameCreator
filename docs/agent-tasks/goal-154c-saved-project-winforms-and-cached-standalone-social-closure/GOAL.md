# Goal 154C — Saved Project, WinForms & Cached Standalone Social Closure

## Identity
- Task ID: `goal-154c-saved-project-winforms-and-cached-standalone-social-closure`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `d12370117a462ded2f8d414eab854562be1c7726`
- Required base message: `GREEN Goal 154B1 quest reward preservation and action-scoped social effect truth hotfix`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration
```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

The social Runtime core, rollback, replay, module independence and action-scoped effect truth are
already GREEN. This Goal closes saved-project, WinForms and cached standalone integration. Do not
redesign Runtime.

## Pre-approval
- The owner approved execution by launching this task.
- Produce a concise internal plan; do not ask for confirmation.
- Do not request intermediate manual testing.
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
Goal154B and Goal154B1 reports/evidence
```

Before edits write a compact product-closure review:
```text
Runtime facts already proven
facts missing from GameProjectBuildResult
facts missing from WinForms
facts missing from standalone payload
real-project mutation risks
host-cache reuse contract
manual-review actions
```
Unanswered items block GREEN.

## Unity budget
```text
Unity Editor invocation budget: 0
Unity host build budget: 0
cached standalone hidden-smoke budget: 1
visible automated standalone launch budget: 0
```

Do not modify `unity/**`. Use the existing generic host cache. If unavailable/invalid, publish
BLOCKED; do not start Unity.

## Current state
Preserve:
```text
Goals153/153A/153B/153C accepted by human
Goal154 historical FAILED foundation
Goal154A historical FAILED partial closure
Goal154B GREEN Runtime core, human-unaccepted
Goal154B1 GREEN semantics hotfix, human-unaccepted
Goal154 manualGateReady=false
```

Goal154C remains accepted=false until explicit owner acceptance.

## Independent audit result
Runtime core truthfully proves:
```text
default 0/10/5/10/7
reputation 0→10
quest completed
choice unavailable→available→unavailable
quest gold 0→10
trusted reward gold 10→17
claim flag true
still_locked final gold 10
second claim atomic
clamps, rollback, checkpoint/full replay
```

Goal154B1 preserves quest gold 10, proves module independence and scopes trusted-resource evidence to
the claim action.

### P2 historical report integrity
Historical `goal154b1-report.md` contains a NUL/truncated letter. Do not rewrite historical evidence.
Record a sanitized correction in Goal154C evidence/current state and add a generic test rejecting NUL
or forbidden control characters in new reports.

## User-visible result
```text
Игры → Механики
enable:
  Фракции и репутация
  Последствия квестов для репутации
  Репутационные ветки диалога
set 0/10/5/10/7
save → close/reopen → build → concise GREEN social card
build Windows game → cached host reused → same facts in standalone
```

Default facts:
```text
Репутация: 0 → 10
Квест: завершён
Доверенная реплика: недоступна → доступна → получена → недоступна
Золото: 0 → 10 → 17
Награда за доверие: +7
Повторная награда: недоступна
Социальный итог: награда получена
Сохранение/повтор: пройдено
```

Threshold 20 automated facts:
```text
Репутация: 0 → 10
Квест: завершён
Доверенная реплика: недоступна
Золото: 0 → 10
Социальный итог: порог репутации ещё не достигнут
```

No hashes, raw IDs or event dumps in the primary card.

## Non-goals
```text
new Runtime commands/effect kinds
new modules
faction simulation
merchant pricing
Unity UI changes
public GamePackage schema changes
dummy content
```
A Runtime change is allowed only for a newly reproduced P0/P1 regression with exact evidence.


## A. Generic social review projection

Create a reusable Application-layer projection service, for example:

```text
SocialRuntimeReviewProjectionService
```

Inputs:

```text
selected effective FeatureModules
qualified GamePackage
capability plan
qualified interactive Runtime session
Runtime effect observations
```

Output record, naming flexible:

```text
present
factionId/factionTitle
reputationBefore
reputationAfter
questId/questTitle
questState
choiceId/choiceText
choiceVisibilitySequence[]
goldBefore
goldAfterQuest
goldAfterClaim
trustedRewardDelta
claimFlagId
rewardClaimed
repeatRewardAvailable
socialOutcome
checkpointReplayPassed
fullReplayEquivalent
humanFacts[]
diagnostics[]
```

### Data-driven correlation

Do not hardcode starter IDs in production C#.

Resolve facts through module contracts and plan data:

```text
faction_reputation_initialized metric
faction_reputation_transition_truthful metric
quest_state_equals metric
dialogue_choice_visibility_sequence metric
trusted_reward_social_outcome metric
resource_transition_truthful metric
flag_equals metric
actions declaring those expected effects
package definitions named by contract target IDs
snapshots named capability.<data-driven actionId>
```

The projection must fail causally when selected social modules declare incompatible or ambiguous
facts. It may return `present=false` when no social module is selected.

### Truth requirements

Claimed:

```text
reputationBefore=0
reputationAfter=10
questState=completed
visibility=[unavailable,available,unavailable]
goldBefore=0
goldAfterQuest=10
goldAfterClaim=17
trustedRewardDelta=7
rewardClaimed=true
repeatRewardAvailable=false
socialOutcome=claimed
```

Locked:

```text
visibility=[unavailable,unavailable,unavailable]
goldAfterQuest=10
goldAfterClaim=10
trustedRewardDelta=0
rewardClaimed=false
socialOutcome=still_locked
```

Do not infer `claimed` from the final flag alone. Correlate the claim action, its events and effect
observations.

## B. Build-result and workspace models

Extend `GameProjectBuildResult` and `UnifiedGameProjectWorkspaceSnapshot` with one structured social
summary record or equivalent typed fields.

Preferred:

```text
GameProjectSocialSummary Social
```

The fields must survive:

```text
normal build result
workspace Snapshot()
build history where semantically appropriate
standalone request construction
failed-attempt separation
```

A failed build must not overwrite the last successful social summary.

Attempt state may expose only attempted module/parameter counts and diagnostics unless a complete
qualified social result exists.

Do not parse localized UI strings back into data.

## C. Build and qualification integration

In `GameProjectBuildAndQualificationService`:

1. after identity-overlaid package qualification succeeds;
2. obtain selected effective modules and semantic effect observations;
3. run the generic social projection;
4. if social modules are selected, require projection `Passed=true`;
5. populate typed build result;
6. append concise Russian social lines to `HumanSummary`.

Required default summary:

```text
Репутация: 0 → 10
Квест: завершён
Доверенная реплика: недоступна → доступна → получена → недоступна
Золото: 0 → 10 → 17
Награда за доверие: +7
Повторная награда: недоступна
Социальный итог: награда получена
```

For no social modules, existing build summaries/hashes remain unchanged.

For `still_locked`, build remains GREEN and summary says the threshold is not reached.

## D. Concise WinForms social card

In ordinary `Игры → Сборка и проверка`, add a compact section shown only when the last successful
build contains social facts.

Header:

```text
Социальные последствия
```

Default rows:

```text
Репутация                  0 → 10
Квест                      завершён
Доверенная реплика         недоступна → доступна → получена → недоступна
Золото                     0 → 10 → 17
Награда за доверие         +7
Повторная награда          недоступна
Социальный итог            награда получена
```

Requirements:

```text
Russian human labels
large readable text
word wrap
no raw IDs/hashes
no duplicate long technical block
card updates after build
card remains after save/reopen and rebuild
failed attempt does not replace last-success card
technical details remain separately available
```

Do not add another manual verification checklist to the main UI.

## E. Standalone human facts

Extend `UnifiedGameProjectWorkspaceController.BuildHumanReviewFacts()` generically.

When social summary is present add:

```text
Репутация
Квест
Доверенная реплика
Золото
Награда за доверие
Повторная награда
Социальный итог
```

Values come from the typed build result, not from hardcoded default values.

The existing Unity host already renders arbitrary `HumanReviewFacts`. Do not change Unity code or
host key.

Hidden standalone smoke must validate that payload facts contain the actual default social values.

## F. Real saved-project lifecycle

Use read-only source:

```text
%LOCALAPPDATA%\LLMGameCreator\Games\goal148-manual
```

Create a short disposable LocalAppData copy. The source is never opened for write.

### F1. Default claimed lifecycle

On the disposable copy:

```text
open existing project
select three social modules
set 0/10/5/10/7
save
close/reopen
verify values exactly
build GREEN
repeat build without changes
```

Require:

```text
project identity preserved
source manifest before/after byte-identical
activated package has no dummy/proof content
baseline quest gold remains 10
social summary 0→10 and 0→10→17
checkpoint/full replay GREEN
second build package/final hashes byte-identical
last successful social card populated
```

### F2. Parameter change

Change only:

```text
trustedGoldReward 7 → 9
```

Save/reopen/build.

Require:

```text
package hash changes
final state hash changes
social gold becomes 0→10→19
trusted delta becomes 9
unrelated mechanics facts unchanged
source project byte-identical
```

### F3. Locked outcome

On a separate disposable copy or transactional temporary attempt:

```text
threshold 20
```

Require GREEN:

```text
reputation 0→10
gold 0→10
outcome still_locked
rewardClaimed=false
```

Do not leave the default claimed project's persisted values changed to 20.

### F4. Invalid attempt rollback

Attempt an invalid value, for example:

```text
trustedReputationThreshold=101
```

Require:

```text
build rejected at authoring/parameter stage
last successful package/final hash unchanged
last successful social card unchanged
attempt diagnostics name parameter and range
no partial package activation
```

Restore the disposable project's valid persisted value before final evidence.

## G. Cached standalone proof

Using the valid default claimed disposable project:

```text
BuildWindowsStandalone
```

Required:

```text
normal project qualification GREEN
HostReused=true
HostRebuilt=false
Unity process start count=0
one hidden smoke
all five standalone markers GREEN
self-checks all GREEN
payload package hash matches build
payload contains social human facts
payload contains no proof fixture
```

Second assembly after reward `7→9`:

```text
same host executable hash
HostReused=true
HostRebuilt=false
social facts 0→10→19
no Unity process
```

Do not invoke the visible Launch button during automation.

## H. Manual-review contract

After GREEN the human gate is exactly:

```text
1. Enable the three social mechanics, set 0/10/5/10/7 and build.
2. Confirm one GREEN social card shows reputation 0→10 and gold 0→10→17.
3. Save, close/reopen and confirm the five values remain.
4. Build/launch cached standalone and confirm the same social facts.
```

No manual hash, raw ID, replay-event or clamp inspection.


## I. Required behavioral tests

Create at least 14 Goal154C tests, of which at least 10 are behavioral.

Behavioral means invoking real controller/build/projection/standalone services and asserting resulting
data, files, payloads or process plans. Source string/reflection tests do not count.

Required:

1. social projection default claimed fields;
2. social projection locked fields;
3. ambiguous/missing social contracts fail causally;
4. no-social composition returns no social card and unchanged historical result;
5. build result carries typed social summary;
6. workspace snapshot preserves last successful social summary;
7. failed invalid attempt preserves last-success social summary;
8. WinForms renders concise claimed social card;
9. WinForms renders locked card without claiming reward;
10. WinForms primary card contains no module IDs/hashes;
11. standalone human facts default `0→10→17`;
12. standalone human facts custom reward `0→10→19`;
13. cached build path starts zero Unity processes;
14. hidden smoke requires all five markers;
15. real disposable project save/reopen default values;
16. repeat build deterministic;
17. invalid attempt rollback;
18. source project manifest byte-identical;
19. new report text-integrity rejects NUL/forbidden control characters;
20. Goal154B/B1 core regressions remain GREEN.

Do not count historical Goal154A placeholder tests as product proof.

## J. Composition/regression matrix

Run focused:

```text
three social modules default
three social modules locked
faction module alone
faction + quest module
social modules + Goal153 ability/mana/status
all-current-optional catalog
default-off core-only project
```

Require:

```text
no dummy/proof data
quest gold 10 preserved
social card only when social facts exist
Goal153 facts remain present and unchanged
default-off historical package/final hashes preserved
```

No powerset.

## K. Evidence integrity

Create exactly 9 files in each mirrored root:

```text
goal154c-dashboard.json
product-closure-review.json
social-projection-proof.json
winforms-social-card-proof.json
real-project-lifecycle-proof.json
cached-standalone-social-proof.json
rollback-and-default-off-proof.json
artifact-scope-proof.json
goal154c-report.md
```

Procedural/export twins must be byte-identical.

Dashboard fields:

```text
status
goal154cTestsDiscovered
goal154cBehavioralTestsPassed
defaultReputationBefore
defaultReputationAfter
defaultGoldBefore
defaultGoldAfterQuest
defaultGoldAfterClaim
defaultTrustedRewardDelta
defaultSocialOutcome
lockedFinalGold
lockedSocialOutcome
saveReopenPassed
repeatBuildDeterministic
invalidAttemptRollbackPassed
sourceProjectByteIdentical
winFormsSocialCardPassed
standaloneFactsPassed
hostReused
hostRebuilt
unityProcessStartCount
hiddenSmokePassed
artifactScopeViolationCount
goal154Accepted=false
goal154cAccepted=false
manualGateReady=true
```

No GREEN field may be null, `PARTIAL`, `NOT_EXECUTED` or sourced only from a constant.

All new Markdown/JSON/text evidence must pass:

```text
no NUL
no forbidden C0 control characters except CR/LF/TAB
valid UTF-8
no mojibake markers
no escaped Cyrillic in user-facing JSON/Markdown where repository policy forbids it
```

Historical Goal154B1 evidence remains unchanged; the Goal154C report records its sanitized correction.

## L. Current-state publication

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

goal154cImplementationStatus=GREEN
goal154cAccepted=false
goal154cAcceptedByHuman=false
goal154cAcceptedByCodex=false
goal154cManualReviewPerformed=false
goal154cManualGateReady=true

goal154DefaultValues=0/10/5/10/7
goal154DefaultReputation=0->10
goal154DefaultGold=0->10->17
goal154DefaultSocialOutcome=claimed
goal154LockedFinalGold=10
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

## M. Command and investigation budget

```text
read-first: maximum 10 primary files
projection/models/build integration: maximum 12 minutes
WinForms social card: maximum 8 minutes
behavioral tests: maximum 14 minutes
real-project lifecycle: maximum 7 minutes
cached standalone smoke: maximum 4 minutes
total target wall clock: 42 minutes
maximum two testhost processes
Unity process count: 0
```

Rules:

```text
write/discover Goal154C tests before long commands
no unchanged command repetition
no timeout escalation loop
no full suite
no 85-case historical closure
no all-ProductSmoke
no Unity host build
no historical evidence rewrite
raw logs remain ignored
```

## N. Required validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal154C"
# require total >=14 and behavioral >=10

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

Then run the disposable real-project lifecycle and exactly one hidden standalone smoke.

Run artifact-scope validation last.


## O. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal154c-saved-project-social-closure.ps1
.devflow/scripts/run-goal154c-saved-project-social-closure.cmd

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/SocialRuntimeReviewProjectionService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs

src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/Goal154C/Goal154CSocialProjectionTests.cs
tests/LLMGameCreator.Tests/Application/Goal154C/Goal154CWorkspaceLifecycleTests.cs
tests/LLMGameCreator.Tests/Application/Goal154C/Goal154CWinFormsSocialCardTests.cs
tests/LLMGameCreator.Tests/Application/Goal154C/Goal154CStandaloneSocialFactsTests.cs
tests/LLMGameCreator.Tests/Application/Goal154C/Goal154CEvidenceTextIntegrityTests.cs
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal154SocialConsequenceWorkspaceTests.cs
tests/LLMGameCreator.Tests/Application/ProjectStandaloneBuild/Goal154CSocialStandaloneTests.cs

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

docs/agent-tasks/goal-154c-saved-project-winforms-and-cached-standalone-social-closure/
.llmgc/procedural/goal-154c-saved-project-winforms-and-cached-standalone-social-closure/
.llmgc/exports/goal-154c-saved-project-winforms-and-cached-standalone-social-closure/
```

If an exact compile/test failure proves one additional existing Application/WinForms/test path is
required:

```text
record exact reason
add exact path only
do not broaden an entire subtree
```

Forbidden without a newly reproduced P0/P1 and explicit evidence:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
catalogs/feature-modules/**
src/LLMGameCreator.Application/Design/FeatureModuleComposition/**
unity/**
samples/**
generator-library/**
provider/**
LLM/**
RAG/**
public GamePackage schema
```

The existing host cache and user game source are external runtime inputs, never committed artifacts.

## P. Publication

Create exactly one final commit:

```text
GREEN Goal 154C saved project WinForms and cached standalone social closure
```

or honest:

```text
BLOCKED Goal 154C saved project WinForms and cached standalone social closure
FAILED Goal 154C saved project WinForms and cached standalone social closure
```

Codex performs commit and push itself.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
Unity process start count=0
HostRebuilt=false
Goal154 family human acceptance=false
Goal154 manualGateReady=true only on GREEN
```

A coherent BLOCKED/FAILED task-owned result is still committed and pushed.

## GREEN criteria

```text
Goal154C test count >=14 and behavioral count >=10
generic typed social projection passes claimed and locked outcomes
no social modules leaves old build result unchanged
GameProjectBuildResult and workspace snapshot carry typed social result
failed attempt preserves last-success social card
WinForms shows one concise readable Russian social card
WinForms primary card contains no IDs/hashes
real goal148-manual disposable save/reopen/build/repeat passes
five default social parameter values persist exactly
default social facts are 0→10 and 0→10→17
parameter 7→9 changes package/final hashes and facts to 0→10→19
locked outcome remains GREEN with final gold 10
invalid attempt rollback preserves last success
source project manifest byte-identical
existing host cache reused
second payload also reuses same host
Unity process count=0
hidden smoke all markers GREEN
standalone human facts contain actual social results
no proof fixtures/artificial gold
Goal154B/B1 and Goal153 regressions GREEN
new evidence contains no forbidden control characters
procedural/export evidence byte-identical
artifact scope 0 violations
Goal154 implementation GREEN but human-unaccepted
Goal154C manualGateReady=true
one final commit pushed
```

## Final report

Return GREEN, BLOCKED or FAILED and include:

- model/reasoning used;
- product-closure review result;
- Goal154C discovered/behavioral test counts;
- typed social projection fields;
- WinForms card facts;
- default save/reopen/build/repeat result;
- custom reward 9 result;
- locked outcome;
- invalid-attempt rollback;
- source-project immutability;
- host cache key/reuse and Unity process count;
- hidden standalone markers/self-checks/social facts;
- Goal154B/B1 and Goal153 regressions;
- evidence text-integrity and mirror result;
- artifact scope;
- Goal154/154A/154B/154B1/154C flags;
- exact four-step manual gate;
- final SHA/push/HEAD/worktree;
- confirmation no human acceptance was claimed.
