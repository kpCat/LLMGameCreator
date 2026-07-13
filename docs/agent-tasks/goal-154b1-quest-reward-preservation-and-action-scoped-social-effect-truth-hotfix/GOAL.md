# Goal 154B1 — Quest Reward Preservation + Action-Scoped Social Effect Truth Hotfix

## Identity

- Task ID: `goal-154b1-quest-reward-preservation-and-action-scoped-social-effect-truth-hotfix`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `58531bcd9cb47cf0091630411fa6c873a6a9e2d4`
- Required base message: `GREEN Goal 154B executable social Runtime lifecycle core closure`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Goal154B's executable Runtime core is largely sound. This is a narrow P1 product-semantics and
effect-correlation hotfix.

## Pre-approval

- The owner approved execution by launching this task.
- Produce a concise internal plan; do not ask for confirmation.
- Do not request intermediate manual testing.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No candidate/intermediate commits.
- Codex performs commit and push itself.

## Budgets

```text
Unity Editor invocation budget: 0
standalone smoke budget: 0
WinForms work budget: 0
real saved-project work budget: 0
```

Goal154C remains responsible for WinForms, real-project and standalone integration.

## Current state

Goals153/153A/153B/153C remain accepted by human.
Goals154/154A/154B/154B1 remain human-unaccepted.
No manual gate is ready.

Goal154 and Goal154A remain historical honest FAILED partial implementations.
Goal154B is a GREEN Runtime-core commit with the independent P1 blocker below.

## Independent audit findings

### P1-A — reputation module removes an unrelated quest reward

The baseline `quest/help_healer` already grants:

```text
resource/gold amount=10
reputation faction/village amount=5
```

Goal154B added to `feature.quest.faction_reputation_consequences`:

```text
quest.00a_gold_reward_reserved
quest/help_healer rewards resource/gold amount: 10 -> 0
```

Selecting the reputation-consequence module without the dialogue-reward module therefore removes the
quest's normal gold reward and provides no replacement.

This violates module independence, user-facing claim truth and existing-content preservation.

### P1-B — earlier expected lifecycle ignored baseline content

The earlier expected `gold 0 -> 7` was wrong because the existing quest grants 10 gold.

Correct default lifecycle:

```text
initial gold=0
quest completion gold=10
trusted claim gold=17
trusted claim delta=+7
```

Correct locked lifecycle:

```text
quest completion gold=10
claim skipped
final gold=10
```

Do not rewrite the game to force a convenient expected number.

### P1-C — trusted resource metric can read unrelated gold events

`ResourceTransitionTruthful()` currently searches all snapshots for the last `ResourceChanged`
event for the resource. In `still_locked`, the quest's legitimate gold event must not count as
evidence that the skipped trusted claim changed gold.

The metric must be correlated to the action that declares `resource_transition_truthful`.

## Primary objectives

1. Preserve the existing healer quest 10-gold reward.
2. Remove all quest-gold-zeroing mutations/claims.
3. Correct claimed/locked/zero/custom gold expectations.
4. Scope trusted resource-effect evaluation to the trusted claim action.
5. Prove social modules remain independent.
6. Preserve Goal154B planner, conditions, rollback and replay.
7. Keep manualGateReady=false and defer product surfaces to Goal154C.
8. Keep Unity process count zero.

## A. Remove the unrelated gold mutation

From `quest-faction-reputation-consequences.featuremodule.json`, remove:

```text
activated_package_diff:quest_gold_reserved_for_trusted_choice:...
quest.00a_gold_reward_reserved
sourceLineage operation quest.00a_gold_reward_reserved
```

The quest reputation module may mutate only:

```text
reputation completion amount
reputation failure effect definition/amount
```

It may not modify gold, items, inventory, objectives, dialogue choices or unrelated rewards.

Bump:

```text
feature.quest.faction_reputation_consequences moduleVersion=1.2.0
```

Its fingerprint must invalidate the module and dialogue dependent while unrelated optional modules
remain reusable.


## B. Correct lifecycle expectations

### Default claimed — `0/10/5/10/7`

```text
gold initial=0
gold after quest=10
reputation 0→10
trusted choice available
trusted claim requestedDelta=7
gold 10→17
claim flag=true
trusted choice unavailable after claim
socialOutcome=claimed
```

### Still locked — threshold 20

```text
quest completed
reputation=10
gold after quest=10
claim action SKIPPED
claim-action ResourceChanged count=0
final gold=10
claim flag absent/not true
socialOutcome=still_locked
```

### Zero trusted reward

```text
trustedGoldReward=0
quest gold 0→10
claim executes once
claim resource event requestedDelta=0 actualDelta=0
final gold=10
claim flag=true
socialOutcome=claimed
```

### Custom trusted reward

```text
trustedGoldReward=9
quest gold=10
claim gold 10→19
trusted claim actualDelta=9
```

### Quest-consequence module without dialogue module

```text
quest completion grants existing 10 gold
reputation changes by configured value
no trusted choice exists
no claim flag exists
```

### Faction module alone

```text
only faction default reputation changes
quest rewards byte-equivalent to base
dialogues byte-equivalent to base
```

## C. Action-scoped effect correlation

Use existing plan correlation:

```text
CapabilityRuntimePlaythroughAction.ExpectedRuntimeEffects
snapshot StepId = capability.<actionId>
```

Update `ResourceTransitionTruthful()` to use only events from
`EventsForMetric(session, contract.MetricKind)` or an equivalent generic action-scoped mechanism.

Required:

```text
exactly one relevant ResourceChanged event -> validate it
zero relevant events + still_locked -> not_applicable
zero relevant events + claim executed -> failure
more than one relevant event -> ambiguous/failure
quest, transaction and unrelated resource events must not count
final resource state must equal event after
```

Forbidden generic hardcoding:

```text
claim_trusted_reward
resource/gold
trusted_village_reward
Goal154
```

Audit the other new social metrics:

```text
faction_reputation_transition_truthful
flag_equals
trusted_reward_social_outcome
dialogue_choice_visibility_sequence
```

Add focused negative tests for any correlation gap found. Do not refactor unrelated metrics.

## D. Product-diff and independence gate

Materialize:

```text
faction module only
faction + quest consequence
all three social modules
all current optional modules
```

Require:

```text
faction only:
  only faction default reputation changes

faction + quest:
  only reputation completion/failure changes
  original quest gold reward remains 10

all social:
  quest gold remains 10
  trusted choice adds configured reward

all current optional:
  no unclassified/proof mutation
```

A reputation-only module must fail quality validation if it mutates non-reputation quest rewards
without a separately declared user-facing capability.

Implement this through mutation classification/capability scope, not one module-ID special case.


## E. Behavioral regressions

Create at least 10 real behavioral Goal154B1 tests.

Required:

1. baseline healer quest gold reward is 10;
2. faction module alone preserves quest/dialogue bytes;
3. quest reputation module alone preserves gold 10 and changes reputation;
4. all social default gives gold `0→10→17`;
5. still_locked gives final gold 10 and no claim-action resource event;
6. zero trusted reward gives final gold 10 and claim flag true;
7. custom reward 9 gives final gold 19;
8. action-scoped evaluator ignores quest gold event for trusted claim metric;
9. an injected unrelated later gold event cannot satisfy claim contract;
10. multiple claim-action resource events are rejected as ambiguous;
11. module order yields byte-identical package and plan;
12. default-off historical package/final hashes stay unchanged;
13. module version/fingerprint invalidates owner and dialogue dependent only;
14. Goal154B claimed/locked/checkpoint/rollback regressions remain GREEN.

Every behavioral test must invoke actual binding, mutation, planner, Runtime or evaluator services.
Source-string/reflection tests do not count.

## F. Current state

On GREEN:

```text
goal154ImplementationStatus=FAILED historical foundation
goal154aImplementationStatus=FAILED historical partial closure

goal154bImplementationStatus=GREEN
goal154bAuditBlocker=closed_by_goal154b1

goal154b1ImplementationStatus=GREEN
goal154b1Accepted=false
goal154b1AcceptedByHuman=false
goal154b1AcceptedByCodex=false
goal154b1ManualReviewPerformed=false
goal154b1ManualGateReady=false

goal154QuestGoldRewardPreserved=true
goal154DefaultGoldLifecycle=0->10->17
goal154LockedFinalGold=10
goal154TrustedEffectActionScoped=true
goal154ManualGateReady=false
nextProductGoal=goal154c_saved_project_winforms_standalone_social_closure
```

Do not claim human acceptance.
Historical Goal154B evidence remains immutable. Create Goal154B1 evidence separately.

## Command budget

```text
read-first: maximum 8 primary files
product-semantic correction: maximum 6 minutes
effect correlation: maximum 8 minutes
behavioral tests: maximum 12 minutes
focused regressions/artifact scope: maximum 8 minutes
total target wall clock: 28 minutes
maximum two testhost processes
Unity process count: 0
```

Rules:

```text
no full suite
no historical closure
no all-ProductSmoke
no WinForms
no real saved-project
no standalone
no Unity
no unchanged command repetition
```

## Required validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal154B1"
# require >=10 behavioral tests

dotnet test ... --filter "FullyQualifiedName~Goal154B1"
dotnet test ... --filter "FullyQualifiedName~Goal154B"
dotnet test ... --filter "FullyQualifiedName~Goal153C"
dotnet test ... --filter "FullyQualifiedName~CapabilityDrivenRuntimePlaythrough"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleLibrary"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleCertification"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Run artifact-scope validation.


## Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal154b1-quest-reward-preservation.ps1
.devflow/scripts/run-goal154b1-quest-reward-preservation.cmd

catalogs/feature-modules/optional/quest-faction-reputation-consequences.featuremodule.json

src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleLibraryValidator.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleRuntimeEffectEvaluator.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleRuntimeEffectModels.cs

tests/LLMGameCreator.Tests/Application/Goal154B1/Goal154B1QuestRewardPreservationTests.cs
tests/LLMGameCreator.Tests/Application/Goal154B1/Goal154B1ActionScopedEffectTests.cs
tests/LLMGameCreator.Tests/Application/Goal154B1/Goal154B1CompositionCertificationTests.cs
tests/LLMGameCreator.Tests/Application/Goal154B/Goal154BClaimedAndLockedLifecycleTests.cs
tests/LLMGameCreator.Tests/Application/Goal154B/Goal154BParameterAndMutationTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/goal154b-executable-social-runtime-core.md
docs/manual-acceptance/goal154b1-quest-reward-preservation.md

docs/agent-tasks/goal-154b1-quest-reward-preservation-and-action-scoped-social-effect-truth-hotfix/
.llmgc/procedural/goal-154b1-quest-reward-preservation-and-action-scoped-social-effect-truth-hotfix/
.llmgc/exports/goal-154b1-quest-reward-preservation-and-action-scoped-social-effect-truth-hotfix/
```

If an exact compile/test failure proves another existing test/model path is required, add only that
exact path with a recorded reason.

Forbidden:

```text
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/**
src/LLMGameCreator.Runtime/**
unity/**
samples/**
generator-library/**
provider/**
LLM/**
RAG/**
public GamePackage schema changes
```

## Compact evidence

Create exactly 7 files in each mirrored root:

```text
goal154b1-dashboard.json
quest-reward-preservation-proof.json
action-scoped-effect-proof.json
module-independence-proof.json
certification-invalidation-proof.json
artifact-scope-proof.json
goal154b1-report.md
```

Dashboard fields:

```text
status
behavioralTestsDiscovered
behavioralTestsPassed
baselineQuestGoldReward
defaultGoldAfterQuest
defaultGoldAfterClaim
lockedFinalGold
zeroRewardFinalGold
customRewardFinalGold
claimEffectActionScoped
questModuleIndependent
defaultOffHashesPreserved
artifactScopeViolationCount
accepted=false
manualGateReady=false
deferredTo=Goal154C
```

No GREEN field may be null, PARTIAL or NOT_EXECUTED.

## Publication

Create exactly one final commit:

```text
GREEN Goal 154B1 quest reward preservation and action-scoped social effect truth hotfix
```

or honest BLOCKED/FAILED.

Codex pushes it.

Required:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
Unity process count=0
Goal154 family remains human-unaccepted
manualGateReady=false
```

## GREEN criteria

```text
existing healer quest 10-gold reward preserved
reputation-only module no longer mutates gold
default gold lifecycle 0→10→17
locked final gold 10
zero trusted reward final gold 10 with claim flag true
custom reward 9 final gold 19
trusted resource metric action-scoped
unrelated resource events cannot satisfy claim contract
ambiguous claim events rejected
module independence and order proven
certification invalidation correct
Goal154B lifecycle/rollback/replay regressions GREEN
Goal153 regressions GREEN
artifact scope 0 violations
one final commit pushed
```

## Final report

Return GREEN, BLOCKED or FAILED and include:

- model/reasoning;
- removed mutation/claim;
- module version and certification invalidation;
- baseline/default/locked/zero/custom gold values;
- action-scoped effect correlation;
- module independence and package diff;
- Goal154B/Goal153 regressions;
- artifact scope;
- Goal154-family flags and deferred Goal154C;
- final SHA/push/HEAD/worktree;
- confirmation no human acceptance claimed.
