# Goal 154A — Social Lifecycle Runtime Proof & One-Time Reward Closure

## Identity

- Task ID: `goal-154a-social-lifecycle-runtime-proof-and-one-time-reward-closure`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `7a63b6388907e978ed0f57b953fec93cb1a36935`
- Required base message: `FAILED Goal 154 faction reputation quest consequences and trusted dialogue reward vertical slice`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Goal154 already published a bounded partial foundation. Complete it rather than reverting or
restarting it. Create and push exactly one final GREEN/BLOCKED/FAILED commit. Do not ask for plan
confirmation or intermediate manual testing. No candidate commits.

## Mandatory orientation

Read:

```text
AGENTS.md
docs/GOAL_DESIGN_QUALITY_POLICY.md
docs/CURRENT_GENERATOR_STATE.md
docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/UNITY_EXECUTION_POLICY.md
Goal154 GOAL.md and FAILED report
```

Before production edits, write a compact completion audit:

```text
usable Goal154 files
absent contracts
unproved Runtime semantics
partial changes that can affect old projects
valid parameter combinations not receiving full replay, with reason
```

Any unanswered item blocks GREEN.

## Unity budget

```text
Unity Editor invocation budget: 0
cached standalone hidden-smoke budget: 1
```

Do not modify Unity host sources. Starting `Unity.exe` is a P1 violation.

## Current state

The base honestly records:

```text
Goal153/153A/153B/153C accepted by human
Goal154 implementationStatus=FAILED
Goal154 accepted=false
Goal154 manualGateReady=false
three modules and five parameters registered
generic nested mutations partially implemented
flag_not_equals implemented
rollback event cleanup partially implemented
Goal154 focused-test count=0
no lifecycle, standalone or artifact-scope proof
```

Preserve Goal153-family acceptance. Goal154/154A remain human-unaccepted.

## Independent audit findings

### P1-A — false-green capability risk

All three modules currently have:

```text
runtimeEffectContracts=[]
runtimePlaythroughContracts=[]
```

They can change the package without proving initial reputation, quest consequence, choice visibility,
one-time claim or gold/flag state.

### P1-B — unsupported declared primitives

Goal154 declarations name command/presentation primitives absent from
`CapabilityRuntimePrimitiveIds.Supported` and canonical handlers.

### P1-C — incorrect SetReputation event truth

`SetReputation()` reports the new value as both `before` and `after`, forcing `delta=0`.
Some structured social/resource numeric args are culture-sensitive.

### P1-D — rollback and one-time behavior unproved

No tests cover quest reward failure after reputation mutation, quest failure-effect rollback,
dialogue reward failure after flag mutation, nested dialogue action failure, or a second claim.

### P1-E — no human-readable social result

Build and standalone do not surface:

```text
Репутация: 0 → 10
Квест: завершён
Доверенная реплика: недоступна → доступна → получена → недоступна
Золото: 0 → 7
```

### P2 — semantic versions

Complete modules must be version `1.1.0`.

## Product result

Complete this real lifecycle:

```text
initial reputation
→ trusted choice absent
→ existing healer quest completes
→ reputation changes
→ trusted choice unlocks when threshold is met
→ reward executes once
→ gold and claim flag change atomically
→ trusted choice disappears
→ checkpoint/full replay remain equivalent
```

A valid unreachable threshold is GREEN with:

```text
socialOutcome=still_locked
rewardClaimed=false
goldDelta=0
```

## Existing product data only

Use existing:

```text
faction/village
quest/help_healer
objective/collect_red_herbs
dialogue/healer
node=start
inventory/player_start
resource/gold
```

These IDs may exist in module JSON/tests/evidence, never in generic production branches.

Forbidden activated content:

```text
dummy faction
dummy quest
dummy dialogue
proof NPC
qualification inventory
artificial starting gold
test-only social entity
```

`AdvanceQuestObjective` is proof orchestration only and does not enter package content.


## A. Finish and version the modules

Set all three Goal154 modules to `moduleVersion=1.1.0`. Keep default-off.

### A1. Faction module

Add a real playthrough/effect contract proving package-defined initial reputation.

Required observation:

```text
faction_reputation_initialized
```

Faction ID comes from module data.

### A2. Quest consequence module

Add a playthrough action:

```text
runtime.command.advance_quest_objective
```

Args:

```text
questId
objectiveId
amount
```

It completes the existing quest and is the checkpoint boundary.

Required observations:

```text
quest_state_equals completed
faction_reputation_transition_truthful
```

The transition validates:

```text
before
requested
after
actual delta
clamped
final Runtime state
```

Requested and actual delta may differ under clamping.

### A3. Dialogue reward module

Required sequence:

```text
open healer before completion
close healer before completion
open healer after quest completion
conditionally choose trusted reward
conditionally close dialogue if it remained open
open healer after claim/skip
close healer
```

Required outcome:

```text
trusted_reward_social_outcome
```

Allowed:

```text
claimed
still_locked
already_claimed
```

Normal first-pass plan accepts `claimed|still_locked`. `already_claimed` is for direct negative tests.

Add meaningful `runtimeEffectContracts` and `runtimePlaythroughContracts` to every module.
An enabled module with no executable/effect contract must fail catalog/certification validation.

## B. Generic capability primitives

Extend `CapabilityRuntimePrimitiveIds` and canonical handlers:

```text
runtime.command.advance_quest_objective
runtime.command.fail_quest
runtime.command.choose_dialogue_option
runtime.command.close_dialogue

runtime.presentation.inspect_faction
runtime.presentation.inspect_dialogue_choices
runtime.presentation.inspect_social_summary
```

Mappings:

```text
advance_quest_objective -> AdvanceQuestObjective
fail_quest -> FailQuest
choose_dialogue_option -> ChooseDialogueOption
close_dialogue -> CloseDialogue
```

Presentation primitives execute no gameplay handler.

`PrimitiveCommandKind` and action-binding validation cover all command primitives.
No Goal154/module/parameter/starter-content literal in generic C#.

## C. Generic conditional social execution

Extend the typed conditional mechanism with:

```text
dialogue_choice_available
dialogue_open
```

### `dialogue_choice_available`

Args:

```text
dialogueId
nodeId
choiceId
inventoryId optional
unavailableOutcome=still_locked
```

Evaluate the actual choice conditions/requirements against current Runtime state.

If available: execute normally.

If unavailable due valid unmet requirements and `unavailableOutcome=still_locked`:

```text
handler not called
state hash unchanged
gameplay event count=0
journal and snapshot contain truthful skip reason plus causal requirement diagnostics
replay makes the same decision
```

Unknown dialogue/node/choice, malformed requirements, or unsupported outcome are failures, not skips.

A direct second claim after the claim flag is true must fail transactionally with state unchanged
and zero success events.

### `dialogue_open`

A close action may skip truthfully when the trusted choice already closed the dialogue.

Unknown predicates remain rejected.

## D. Structured event truth

### D1. Faction

`ChangeReputation` and `SetReputation` emit invariant-culture args:

```text
factionId
before
requested
after
delta
clamped
```

For Set:

```text
before=actual previous
requested=absolute requested value
delta=after-before
clamped=requested!=after
```

### D2. Resource reward

Resource-change events expose invariant:

```text
resourceId
scope
before
requestedDelta
after
actualDelta
clamped
```

### D3. Flags

`set_flag` exposes:

```text
flagId
before
after
```

### D4. Rollback

Failed higher-level quest/dialogue transactions return at most a failure event. They must not leak
success events from reputation/resource/flag mutations performed on the discarded clone.

## E. Default lifecycle

Parameters:

```text
startingReputation=0
questReputationReward=10
questFailurePenalty=5
trustedReputationThreshold=10
trustedGoldReward=7
```

Required:

```text
initial reputation=0
trusted before completion=unavailable
quest=completed
reputation 0→10
trusted after completion=available
claim executes once
gold 0→7
claim flag=true
trusted after claim=unavailable
socialOutcome=claimed
```

Checkpoint after quest completion/reputation and before claim.

Require uninterrupted/resumed event equivalence and equal final hashes.


## F. Parameter-domain matrix

Required full Runtime paths:

```text
default:          0 / 10 / 5 / 10 / 7 -> claimed
still locked:     0 / 10 / 5 / 20 / 7 -> still_locked, gold delta 0
already trusted: 20 / 0  / 5 / 10 / 7 -> claimed once
zero reward:      0 / 10 / 5 / 10 / 0 -> claimed, gold unchanged, flag true
upper clamp:     95 / 10 / 5 / 10 / 7 -> final rep 100, actual delta 5, clamped true
lower clamp:    -95 / 10 /10 / 10 / 7 -> fail quest final rep -100, actual delta -5, clamped true
interior custom:  3 / 12 / 4 / 15 / 9 -> final rep 15, claimed, gold 9
```

Required authoring rejection:

```text
every parameter below minimum
every parameter above maximum
fractional input for integer
invalid step
unknown parameter
unselected module parameter
```

No cross-parameter rule may reject `threshold > starting + reward`; that is a valid locked outcome.

Extreme parameter cases may use focused Runtime tests rather than all being run through standalone.

## G. Failure/rollback matrix

### Quest completion

Construct test-local quest outputs:

```text
valid reputation output
then invalid missing resource output
```

Required:

```text
quest remains active
reputation unchanged
state byte-identical
no QuestCompleted
no QuestRewardGranted
no FactionReputationChanged
no OutputApplied success event
one causal failure diagnostic/event allowed
```

### Quest failure

Construct test-local failure effects:

```text
valid negative reputation
then invalid output
```

Required state/event rollback as above; quest remains active.

### Dialogue claim

Construct test-local choice:

```text
set claim flag
then invalid reward
```

Required:

```text
flag unchanged
gold unchanged
dialogue state unchanged
no DialogueChoiceSelected/DialogueEffectApplied/ResourceChanged success event
```

### Nested dialogue actions

When `AdvanceQuestObjective`, `SetQuestStage`, transaction, or encounter action fails inside a choice,
the entire choice transaction rolls back and does not publish earlier success events.

Do not weaken unrelated Runtime behavior to make these tests pass.

## H. Package and library validation

Add generic package validation where missing:

```text
reputation/faction_reputation/change_reputation output references exactly one faction
reputation_at_least/faction_relation requirement references exactly one faction
flag_not_equals is accepted as a stable requirement kind
trusted choice IDs remain unique
choice target node references remain valid
```

Catalog validation requires every selected nontrivial Goal154 module to have:

```text
at least one mutation or declared no-package-mutation reason
at least one Runtime effect contract
at least one playthrough contract
all required primitives supported
all effect/action references valid
```

Apply this generically, not through Goal154 IDs.

## I. Mutation regression contract

Prove all new nested mutation handlers:

```text
add when missing
byte-equivalent idempotent
conflict-safe
wrong owner/collection/kind/id rejected
duplicate existing target rejected
reverse selected-module order byte-identical
failure returns original package bytes
```

For the normal saved-project pipeline, two repeated builds from the same saved authoring state must
produce byte-identical activated package and final state.

Do not require reapplying a default upsert onto an already parameter-modified activated package
unless the repository's existing composition contract uses activated output as input.

## J. Runtime effect observations

Add generic metrics, names may vary:

```text
faction_reputation_initialized
faction_reputation_transition_truthful
quest_state_equals
dialogue_choice_visibility_sequence
trusted_reward_social_outcome
resource_transition_truthful
flag_equals
```

Contracts use IDs/expected values from module data.

`trusted_reward_social_outcome=claimed` requires:

```text
choice absent before reputation threshold
choice present after threshold
choice selected once
flag true
gold transition truthful
choice absent after claim
```

`still_locked` requires:

```text
choice absent after quest
conditional claim skipped
flag not true
gold unchanged
```

## K. User-facing build and PlayerAdapter

Normal `Игры → Сборка и проверка` must show one concise social section:

```text
Репутация деревни: 0 → 10
Состояние квеста: завершён
Доверенная реплика: недоступна → доступна → получена → недоступна
Золото: 0 → 7
Повторная награда: недоступна
Социальный итог: награда получена
```

For locked configurations:

```text
Социальный итог: порог репутации ещё не достигнут
```

Raw IDs, hashes and event details remain technical details.

Populate standalone `humanReviewFacts` generically from qualified Runtime state/events.
Do not hardcode the default numbers in UI/standalone code.

## L. Real saved-project lifecycle

Use read-only source:

```text
%LOCALAPPDATA%\LLMGameCreator\Games\goal148-manual
```

and a short disposable LocalAppData copy.

Required:

```text
source manifest before/after byte-identical
open existing project
select all three social modules
set default 0/10/5/10/7
save
close/reopen
values preserved
build GREEN
repeat build byte-identical
one-time reward lifecycle GREEN
checkpoint/full replay GREEN
project identity preserved
failed invalid-parameter build leaves last success/configuration truthful
```

Then change one social parameter, e.g. gold `7→9`:

```text
package hash changes
final state hash changes
human fact changes
source project remains byte-identical
```

## M. Standalone cache proof

Use existing host cache only.

Required:

```text
HostReused=true
HostRebuilt=false
Unity process starts=0
one hidden smoke
all five standalone smoke markers GREEN
payload contains no proof fixture
human facts contain reputation/quest/choice/gold/social outcome
```

No visible automated player window.

## N. Activated product diff

Structured diff must classify every social mutation.

Required:

```text
proof fixture count=0
dummy content count=0
unclassified mutation count=0
existing faction/quest/dialogue only
no artificial starting gold
no global unrelated rule/capacity changes
```

Default-off path must preserve pre-Goal154 package/final hashes.

## O. Test discovery guard

The previous Goal failed because the required filter matched zero tests.

Create at least these test classes:

```text
Goal154ASocialModuleContractTests
Goal154ASocialRuntimeLifecycleTests
Goal154ARollbackEventTruthTests
Goal154AParameterDomainTests
Goal154ASavedProjectStandaloneTests
```

Before running focused tests:

```powershell
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal154A"
```

Parse the output and require:

```text
discovered Goal154A tests >= 18
```

Zero or fewer than 18 is an automatic FAILED result. Do not treat a zero-match `dotnet test` exit
code as GREEN.

Required focused cases include all sections F–N plus:

```text
three modules independently where dependencies permit
direct dependency closures
interaction with Goal153 modules
all-current-optional catalog
default-off unchanged path
module order independence
SetReputation event correctness
invariant-culture event args under a comma-decimal culture
second trusted claim state/event atomicity
```


## P. State publication

During execution:

```text
goal154ImplementationStatus=FAILED_PENDING_GOAL154A
goal154ManualGateReady=false
goal154aAccepted=false
goal154aManualGateReady=false
```

On GREEN:

```text
goal154ImplementationStatus=GREEN
goal154ManualGateReady=true
goal154Accepted=false
goal154AcceptedByHuman=false
goal154AcceptedByCodex=false

goal154aImplementationStatus=GREEN
goal154aManualGateReady=true
goal154aAccepted=false
goal154aAcceptedByHuman=false
goal154aAcceptedByCodex=false
goal154aManualReviewPerformed=false

nextAction=independent_goal154a_audit_then_combined_goal154_human_gate
```

Preserve Goals153/153A/153B/153C as accepted by human.
Do not claim Goal154 human acceptance.

## Command and investigation budget

```text
read-first: maximum 12 primary files
completion audit and test skeletons: maximum 8 minutes
generic primitive/effect implementation: maximum 14 minutes
Runtime rollback and event truth: maximum 10 minutes
focused tests: maximum 18 minutes
real-project/cache proof: maximum 6 minutes
total target wall clock: 50 minutes
maximum two testhost processes
Unity Editor process count: 0
```

Rules:

```text
write/discover Goal154A tests before long regression runs
no unchanged command repetition
no timeout escalation loop
isolate a failing test/filter instead
no full suite
no 85-case historical closure
no all-ProductSmoke
no historical snapshot repair
no Unity host build
raw logs remain ignored
```

## Required validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal154A"
# assert discovered count >= 18

dotnet test ... --filter "FullyQualifiedName~Goal154A"
dotnet test ... --filter "FullyQualifiedName~Goal154"
dotnet test ... --filter "FullyQualifiedName~Goal153C|FullyQualifiedName~Goal153B|FullyQualifiedName~Goal153A|FullyQualifiedName~Goal153"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleLibrary"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleCertification"
dotnet test ... --filter "FullyQualifiedName~CapabilityDrivenRuntimePlaythrough"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"
dotnet test ... --filter "FullyQualifiedName~RuntimeNarrative"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Then one cached hidden standalone smoke and artifact-scope validation.

Do not use a filter expression unsupported by the current test runner; shard filters instead.

## Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal154a-social-lifecycle-closure.ps1
.devflow/scripts/run-goal154a-social-lifecycle-closure.cmd

catalogs/feature-modules/manifest.json
catalogs/feature-modules/optional/faction-reputation-standing.featuremodule.json
catalogs/feature-modules/optional/quest-faction-reputation-consequences.featuremodule.json
catalogs/feature-modules/optional/dialogue-reputation-gated-reward.featuremodule.json

src/LLMGameCreator.Application/Validation/EncounterDefinitionValidator.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleLibraryValidator.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModulePackageMutationService.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleRuntimeEffectModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleRuntimeEffectEvaluator.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughModels.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughValidator.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughPlanner.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughExpansionService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildModels.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildService.cs

src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerCommandLoopContracts.cs
src/LLMGameCreator.Runtime.Abstractions/SelectedRuntimeVariantInteractiveSessionContracts.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
src/LLMGameCreator.Runtime/SelectedRuntimeVariantInteractiveSessionService.cs
src/LLMGameCreator.Runtime/FactionRuntimeService.cs
src/LLMGameCreator.Runtime/QuestRuntimeService.cs
src/LLMGameCreator.Runtime/DialogueRuntimeService.cs
src/LLMGameCreator.Runtime/OutputApplier.cs
src/LLMGameCreator.Runtime/RequirementEvaluator.cs

src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/Goal154A/Goal154ASocialModuleContractTests.cs
tests/LLMGameCreator.Tests/Application/Goal154A/Goal154ASocialRuntimeLifecycleTests.cs
tests/LLMGameCreator.Tests/Application/Goal154A/Goal154ARollbackEventTruthTests.cs
tests/LLMGameCreator.Tests/Application/Goal154A/Goal154AParameterDomainTests.cs
tests/LLMGameCreator.Tests/Application/Goal154A/Goal154ASavedProjectStandaloneTests.cs
tests/LLMGameCreator.Tests/Application/FeatureModuleAuthoring/FeatureModuleLibraryAndParameterTests.cs
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal154SocialConsequenceWorkspaceTests.cs
tests/LLMGameCreator.Tests/Runtime/Goal154A/Goal154ASocialRuntimeTests.cs

docs/manual-acceptance/goal154-faction-reputation-social-consequences.md
docs/manual-acceptance/goal154a-social-lifecycle-runtime-proof.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

docs/agent-tasks/goal-154a-social-lifecycle-runtime-proof-and-one-time-reward-closure/
.llmgc/procedural/goal-154a-social-lifecycle-runtime-proof-and-one-time-reward-closure/
.llmgc/exports/goal-154a-social-lifecycle-runtime-proof-and-one-time-reward-closure/
```

If an exact compile failure proves one additional existing model/test path is necessary:

```text
record the exact reason
add only that path to artifact scope
do not broaden to a source subtree
```

Forbidden:

```text
unity/**
samples/**
generator-library/**
provider/**
LLM/**
RAG/**
public GamePackage schema changes
```

## Compact evidence

Create exactly 10 files under each root, byte-identical by relative name:

```text
goal154a-dashboard.json
completion-audit.json
module-contract-proof.json
social-lifecycle-proof.json
rollback-event-truth-proof.json
parameter-domain-proof.json
saved-project-proof.json
cached-standalone-proof.json
artifact-scope-proof.json
goal154a-report.md
```

Dashboard includes actual values and acceptance flags. No placeholder/null result may be GREEN.
Do not commit raw logs, TRX, copied projects, screenshots or manual text.

## Publication

Create exactly one final commit:

```text
GREEN Goal 154A social lifecycle Runtime proof and one-time reward closure
```

or honest BLOCKED/FAILED.

Codex pushes it.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
Unity process start count=0
Goal154/154A accepted=false
manual gate ready only on GREEN
```

## GREEN criteria

```text
Goal154A tests discovered >=18 and all GREEN
all three modules version 1.1.0
every module has meaningful effect and playthrough contracts
all new command primitives supported and bound
default lifecycle 0→10 and gold 0→7 claimed once
still_locked valid and replay-stable
failure penalty and positive/negative clamps truthful
SetReputation event before/requested/after/delta/clamped correct
structured social numeric args invariant-culture
quest/dialogue rollback leaks zero success events
second claim state/event atomic
parameter min/default/max/interior/invalid coverage
new mutation handlers add/idempotent/conflict-safe
no generic Goal154/starter-content literals
default-off historical hashes preserved
Goal153 family regressions GREEN
real saved-project lifecycle GREEN
concise WinForms/standalone social facts
cache reused, Unity starts 0
procedural/export evidence mirrored
artifact scope 0 violations
one final commit pushed
```

## Final report

Return GREEN, BLOCKED or FAILED and include:

- model/reasoning used;
- inherited Goal154 foundation and completed gaps;
- discovered/executed Goal154A test count;
- modules/parameters/versions;
- default reputation/quest/choice/gold lifecycle;
- still_locked and already-claimed results;
- completion/failure/clamp results;
- rollback event-truth matrix;
- checkpoint/full replay hashes and event equivalence;
- mutation/order/default-off proofs;
- real-project lifecycle;
- WinForms/standalone human facts;
- cache reuse and Unity process count;
- evidence mirror;
- artifact scope;
- Goal154/154A flags;
- short manual gate;
- final commit/push/HEAD/worktree;
- confirmation no human acceptance claimed.
