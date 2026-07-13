# Goal 154B — Executable Social Runtime Lifecycle Core Closure

## Identity

- Task ID: `goal-154b-executable-social-runtime-lifecycle-core-closure`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `bcfde451cf2dfac8571531e640bc9bcb2d12b4ae`
- Required base message: `FAILED Goal 154A social lifecycle Runtime proof and one-time reward closure`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Sol
Reasoning effort: High
```

Reason: two Terra sessions produced a useful partial foundation but did not close the executable
Runtime lifecycle. This task is deliberately narrower: capability planning, canonical execution,
conditional social actions, Runtime-effect evaluation, rollback truth and replay only. WinForms,
real saved-project and standalone integration are explicitly deferred to Goal154C after an
independent audit.

Extra High is not required.

## Pre-approval

The owner approved execution by launching this task.

- Produce a concise internal plan; do not ask for confirmation.
- Continue from the published FAILED foundation. Do not revert Goal154/154A and do not restart them.
- Do not request intermediate human testing.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No candidate or intermediate commits.
- Codex performs commit and push itself.

## Unity execution budget

```text
Unity Editor invocation budget: 0
standalone smoke budget: 0
```

This Goal does not touch UI or standalone. Starting `Unity.exe` is a P1 violation.

## Current accepted state

Goals153/153A/153B/153C are accepted by the owner and must remain accepted.

Goals154/154A are historical honest FAILED partial implementations.

Goal154B remains:

```text
accepted=false
acceptedByHuman=false
acceptedByCodex=false
manualReviewPerformed=false
manualGateReady=false
```

even when implementation GREEN. A later Goal154C will complete user workflow and standalone before
any human gate.

## Actual repository audit at required base

The following facts are mandatory context, not hypotheses.

### Useful inherited foundation

- three default-off social modules and five typed parameters exist;
- all three module versions are `1.1.0`;
- nested faction/quest/dialogue mutation handlers exist;
- `flag_not_equals` exists;
- `SetReputation` now records actual before/requested/after/delta/clamped values;
- resource and flag outputs have structured arguments;
- quest/dialogue services contain partial rollback event cleanup;
- Goal153-family human acceptance is recorded.

### Missing executable architecture

The modules declare these primitives:

```text
runtime.command.advance_quest_objective
runtime.command.fail_quest
runtime.command.choose_dialogue_option
runtime.command.close_dialogue
runtime.presentation.inspect_faction
runtime.presentation.inspect_dialogue_choices
runtime.presentation.inspect_social_summary
```

but they are absent from `CapabilityRuntimePrimitiveIds.Supported` and canonical command handlers.

The modules also use target selectors absent from the planner:

```text
faction_id
quest_objective_id
dialogue_node_id
dialogue_choice_id
```

Therefore selecting the modules cannot currently produce a valid capability plan.

### Broken dialogue sequence

The current `claim_trusted_reward` action never opens `dialogue/healer`.

`ChooseDialogueOption` requires an active open dialogue and the current node to contain the choice.
The old required-core dialogue action opens `dialogue/old_guard_intro`, not the healer dialogue.

The module sequence must explicitly open/inspect/close the healer dialogue before and after the quest
and claim.

### Tests are discovery placeholders, not lifecycle proof

Exactly 18 Goal154A tests exist, but they only perform:

```text
string-presence assertions
method-existence reflection
constant equality
```

They do not execute composition, capability planning, Runtime state, commands, checkpoint or replay.

They remain historical evidence of the FAILED attempt, but none count as behavioral proof for
Goal154B.

### Evidence/CI

Goal154A evidence honestly says `PARTIAL` or `NOT_EXECUTED`.
Artifact scope, real project and standalone were not run.
GitHub Actions/status checks are absent.

## Goal154B product boundary

Goal154B closes only the executable social Runtime core:

```text
module data
→ package mutation
→ capability plan
→ canonical/interactive Runtime
→ quest completion or still-locked outcome
→ one-time reward
→ checkpoint/full replay
→ effect observations
```

Goal154B does **not** implement:

```text
WinForms social card
real goal148-manual lifecycle
standalone humanReviewFacts
cached standalone smoke
human acceptance
```

Those are the next product closure Goal154C, not hidden omissions.

## Required default lifecycle

Module parameters:

```text
startingReputation=0
questReputationReward=10
questFailurePenalty=5
trustedReputationThreshold=10
trustedGoldReward=7
```

Required actual Runtime result:

```text
initial faction/village reputation = 0
trusted choice before quest completion = unavailable
quest/help_healer = completed
reputation 0 → 10
trusted choice after completion = available
trusted choice executes exactly once
gold 0 → 7
flag/village_trusted_reward_claimed = true
trusted choice after claim = unavailable
socialOutcome = claimed
```

Checkpoint boundary:

```text
after quest completion/reputation change
before trusted reward claim
```

Required:

```text
checkpoint continuation equals uninterrupted continuation
full replay equals uninterrupted execution
final state hashes equal
social events equivalent
```

## Valid alternative lifecycle

Configuration:

```text
startingReputation=0
questReputationReward=10
trustedReputationThreshold=20
trustedGoldReward=7
```

Required:

```text
quest completes
reputation becomes 10
trusted choice remains unavailable
claim action skips truthfully
gold unchanged
claim flag absent/not true
socialOutcome=still_locked
Runtime qualification GREEN
checkpoint/full replay equivalent
```

This is a valid product outcome, not an error.

## Existing product data only

Use the existing package content:

```text
faction/village
quest/help_healer
objective/collect_red_herbs
dialogue/healer
node=start
inventory/player_start
resource/gold
```

These IDs may appear in module JSON, tests and evidence.

They must not appear in generic production branches.

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


## A. Replace placeholder tests with behavioral proof

The existing 18 Goal154A tests may remain, but they do not count toward Goal154B GREEN.

A **behavioral test** must directly call at least one real service:

```text
FeatureModuleParameterBindingService
FeatureModulePackageMutationService
CapabilityDrivenRuntimePlaythroughValidator/Planner
CanonicalRuntimePlayerCommandLoopService
SelectedRuntimeVariantInteractiveSessionService
GameRuntimeService
QuestRuntimeService
DialogueRuntimeService
FactionRuntimeService
```

and assert actual resulting:

```text
package content
state fields
Runtime events
diagnostics
snapshot/journal entries
checkpoint/replay hash
```

Reflection, `Assert.Contains` on source text, method-existence and constant-equality tests are
source-contract tests only.

Required:

```text
Goal154B behavioral tests discovered >= 16
Goal154B source-contract tests may be additional
all behavioral tests GREEN
```

Create a machine-readable inventory:

```text
testName
proofKind=behavioral|source_contract
servicesInvoked
stateOrEventAssertions
```

The runner must fail if behavioral count is below 16.

## B. Generic capability primitive closure

Extend `CapabilityRuntimePrimitiveIds`:

```text
AdvanceQuestObjective = runtime.command.advance_quest_objective
FailQuest = runtime.command.fail_quest
ChooseDialogueOption = runtime.command.choose_dialogue_option
CloseDialogue = runtime.command.close_dialogue

InspectFaction = runtime.presentation.inspect_faction
InspectDialogueChoices = runtime.presentation.inspect_dialogue_choices
InspectSocialSummary = runtime.presentation.inspect_social_summary
```

Add all to `Supported`.

Add generic canonical handlers:

```text
advance_quest_objective:
  questId from args/target resolution
  objectiveId from args
  amount from args

fail_quest:
  questId from args/target

choose_dialogue_option:
  choiceId from resolved target/args
  inventoryId optional

close_dialogue:
  no arbitrary IDs; closes current active dialogue
```

Add every command to `PrimitiveCommandKind`.

Presentation primitives:

```text
do not invoke gameplay handlers
produce truthful snapshots/summaries from current Runtime state
remain replay-stable
```

Unknown primitives remain rejected.

## C. Generic social target selectors

Extend target resolution without starter-content branches.

### `faction_id`

```text
args.id or args.factionId
must match exactly one package faction
```

### `quest_objective_id`

Args:

```text
questId
objectiveId
```

Required:

```text
quest exists exactly once
objective exists exactly once across root/stages
resolved target is objectiveId
```

### `dialogue_node_id`

Args:

```text
dialogueId
nodeId
```

Both must exist exactly once.

### `dialogue_choice_id`

Args:

```text
dialogueId
nodeId
choiceId
```

All must exist exactly once.

Also add referenced-argument validation for:

```text
factionId
objectiveId with questId
nodeId with dialogueId
choiceId with dialogueId/nodeId
```

Malformed and ambiguous paths are causal planner failures.

## D. Correct the module action graph

Update module JSON so the actual executable order is explicit.

### D1. Faction module

```text
inspect_initial_faction_reputation
depends on start_runtime
presentation only
```

### D2. Quest module

```text
advance_healer_objective
depends on:
  start_or_update_quest
  inspect_initial_faction_reputation
checkpointBoundaryAfter=true
```

The objective amount may exceed the required amount; Runtime clamps objective progress to completion.

### D3. Dialogue module

Required actions:

```text
open_healer_before_quest
inspect_trusted_choice_before_quest
close_healer_before_quest

open_healer_after_quest
inspect_trusted_choice_after_quest
claim_trusted_reward
close_healer_after_claim_if_open

open_healer_after_outcome
inspect_trusted_choice_after_outcome
close_healer_final

inspect_social_summary
```

Dependencies must encode this exact graph.

The claim action executes only when `dialogue_choice_available` passes.
The close-after-claim action executes only when `dialogue_open` passes.

`open_healer_before_quest` must occur before `advance_healer_objective`.
`open_healer_after_quest` occurs after checkpoint boundary.
No action may rely only on numeric order when a semantic dependency exists.

## E. Typed conditional social predicates

Extend the existing conditional execution vocabulary generically.

### E1. `dialogue_choice_available`

Arguments:

```text
dialogueId
nodeId
choiceId
inventoryId optional
unavailableOutcome=still_locked
```

Evaluation uses the same `IRequirementEvaluator` semantics as `DialogueRuntimeService`.

Required distinctions:

```text
choice available -> execute handler
valid unmet requirements -> truthful skip with still_locked
choice missing -> failure
dialogue/node missing -> failure
unknown requirement kind -> failure
malformed unavailableOutcome -> failure
```

Truthful skip:

```text
Runtime handler not called
state hash unchanged
no gameplay mutation event
snapshot/journal status=SKIPPED
diagnostic includes failed requirement codes and socialOutcome=still_locked
action cursor advances deterministically
checkpoint/replay makes same decision
required action remains GREEN because declared valid outcome satisfied
```

### E2. `dialogue_open`

Arguments:

```text
dialogueId optional
```

If no dialogue is open:

```text
truthful skip
state unchanged
no Runtime event
```

If expected dialogue ID is supplied and another dialogue is open:

```text
failure, not skip
```

Unknown predicates remain rejected.

### E3. Do not conflate skips

The existing status-terminal conditional skip must remain unchanged.
Social `still_locked` is not an encounter terminal outcome.

## F. Runtime-effect evaluator closure

Add generic observations driven entirely by contract fields.

### `faction_reputation_initialized`

Reads the initial Runtime state and compares the faction's reputation to expected value.

### `quest_state_equals`

Reads final/current quest state for target quest.

### `faction_reputation_transition_truthful`

Finds the relevant `FactionReputationChanged` event and validates:

```text
factionId
before
requested
after
delta
clamped
after equals final state
delta equals after-before
clamped truth matches package min/max
```

### `dialogue_choice_visibility_sequence`

Validates snapshots/events in order:

```text
before quest: unavailable
after quest: available when threshold reached, otherwise unavailable
after claim: unavailable
```

### `resource_transition_truthful`

Validates structured resource event and final resource state.

### `flag_equals`

Validates final flag state.

### `trusted_reward_social_outcome`

Computes one of:

```text
claimed
still_locked
already_claimed
```

For normal first-run qualification only `claimed|still_locked` are accepted.

No metric branches on module/faction/quest/dialogue/choice IDs.
IDs and expected values come from the contract.

## G. Structured event truth

Preserve the partial Goal154A improvements and fully test them.

### Reputation events

Invariant-culture args:

```text
factionId
before
requested
after
delta
clamped
```

`SetReputation` uses the actual previous state.

### Resource events

For `change_resource` reward output:

```text
resourceId
scope
before
requestedDelta
after
actualDelta
clamped
```

All numbers invariant culture.

### Flag events

For `set_flag`:

```text
flagId
before
after
```

### Rollback

When a higher-level quest or dialogue action fails after earlier outputs:

```text
original state byte-identical
no reputation/resource/flag success event escapes
no QuestCompleted/QuestFailed/DialogueChoiceSelected success event escapes
one causal ValidationFailed event is allowed
```


## H. Required behavioral regression matrix

At minimum create these direct behavioral tests.

### H1. Planner and module contracts

1. all three social modules selected:
   ```text
   planner Passed=true
   every social primitive supported
   every target resolved
   dependency graph acyclic
   checkpoint action is advance_healer_objective
   ```
2. malformed faction/quest/node/choice selector fails causally.
3. reverse selected-module order produces identical package and plan signature.
4. each dependency closure works where selection is valid.
5. default-off composition preserves pre-Goal154 package/final hashes.

### H2. Default claimed lifecycle

6. actual parameterized composition with `0/10/5/10/7`.
7. execute the real interactive capability session action by action.
8. assert:
   ```text
   rep 0→10
   quest completed
   visibility unavailable→available→unavailable
   gold 0→7
   flag true
   one choice selection
   socialOutcome=claimed
   ```
9. checkpoint after quest, reload, finish claim, compare final hash/events.
10. full replay equals uninterrupted path.

### H3. Still locked

11. use `0/10/5/20/7`.
12. assert quest completed, rep 10, conditional claim `SKIPPED`, gold unchanged, flag absent,
    socialOutcome `still_locked`.
13. checkpoint/full replay equivalent.

### H4. Already claimed/direct negative

14. build state where trusted reward was already claimed, reopen healer and invoke the same choice
    directly.
15. assert failure, state hash unchanged, no reward/flag/dialogue success events.

### H5. Reputation clamps

16. `starting=95, reward=10`:
    ```text
    final=100
    requested=10
    actual delta=5
    clamped=true
    ```
17. `starting=-95, failurePenalty=10`, start then fail quest:
    ```text
    final=-100
    requested=-10
    actual delta=-5
    clamped=true
    quest failed
    ```

### H6. Rollback

18. quest completion outputs: valid reputation then missing resource.
19. quest failure effects: valid negative reputation then invalid output.
20. dialogue outputs: valid flag then missing resource.
21. nested dialogue action failure after earlier outputs.
22. for each: state byte-identical and zero leaked success events.

### H7. Event culture

23. run under at least one comma-decimal culture.
24. parse all structured social numeric args with invariant culture and assert exact values.

### H8. Parameter domain

25. min/default/max/interior for all five parameters through actual binding/composition.
26. below/above/fractional/step/unknown/unselected rejection.
27. threshold > starting+reward remains valid `still_locked`.
28. reward zero claims once, flag true, gold unchanged.

At least 16 of these must be separate behavioral test methods. All required scenarios must execute.

## I. Package and library validation

Add generic validation where missing:

```text
faction reputation output references exactly one faction
reputation requirement references exactly one faction
dialogue choice IDs unique in node
dialogue choice target node valid
flag_not_equals accepted as stable requirement vocabulary
selected nontrivial optional module has at least one effect contract
selected nontrivial optional module has at least one playthrough contract
every required primitive supported
every effect/action reference resolvable
```

Do not enforce this by Goal154 IDs.

## J. Mutation regression

Behaviorally prove inherited nested handlers:

```text
quest output amount
quest failure output upsert/amount
dialogue choice upsert
dialogue requirement amount
dialogue reward amount
faction definition numeric property
```

For each relevant handler:

```text
add/update success
byte-equivalent idempotency
conflict rejection
wrong owner/collection/kind/id rejection
duplicate target rejection
failure returns original package bytes
```

No test may count a source string assertion as mutation proof.

## K. Product/proof separation

Materialize the activated package and compare to base.

Required:

```text
proof fixture count=0
dummy content count=0
unclassified mutation count=0
only existing faction/quest/dialogue extended
no artificial starting gold
no new inventory/NPC/faction/quest/dialogue
no unrelated global capacity/rule changes
```

Proof orchestration actions do not appear in the package.

## L. Scope explicitly deferred to Goal154C

Do not spend Goal154B time on:

```text
WinForms social card
ProjectStandaloneBuildService human facts
real goal148-manual save/reopen
cached standalone smoke
Unity
human gate
```

Do not create fake NOT_EXECUTED evidence for these. State explicitly:

```text
deferredTo=Goal154C
```

## M. Current-state routing

On Goal154B GREEN:

```text
goal154ImplementationStatus=FAILED historical foundation
goal154aImplementationStatus=FAILED historical partial closure

goal154bImplementationStatus=GREEN
goal154bAccepted=false
goal154bAcceptedByHuman=false
goal154bAcceptedByCodex=false
goal154bManualReviewPerformed=false
goal154bManualGateReady=false

goal154CoreRuntimeLifecyclePassed=true
goal154DefaultClaimedPassed=true
goal154StillLockedPassed=true
goal154CheckpointReplayPassed=true
goal154RollbackEventTruthPassed=true

goal154ManualGateReady=false
nextProductGoal=goal154c_saved_project_winforms_standalone_social_closure
```

Preserve Goals153/153A/153B/153C human acceptance.

Do not claim Goal154 human acceptance.

## Command and investigation budget

```text
read-first: maximum 10 primary files
completion audit: maximum 5 minutes
test replacement/skeleton: maximum 5 minutes
primitive/selector/conditional implementation: maximum 12 minutes
effect evaluator and Runtime fixes: maximum 10 minutes
focused behavioral tests: maximum 15 minutes
total target wall clock: 42 minutes
maximum two testhost processes
Unity process count: 0
```

Rules:

```text
write behavioral tests before long regression commands
do not run placeholder Goal154A tests as evidence of lifecycle correctness
no unchanged command repetition
no timeout escalation
run only failing exact test/class after a concrete fix
no full suite
no 85-case closure
no all-ProductSmoke
no real-project or standalone proof
```

## Required validation

### Discovery and behavioral inventory

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal154B"
```

Require:

```text
Goal154B total discovered >= 16
Goal154B behavioral inventory count >= 16
```

### Focused

```powershell
dotnet test ... --filter "FullyQualifiedName~Goal154B"
dotnet test ... --filter "FullyQualifiedName~Goal154A"
dotnet test ... --filter "FullyQualifiedName~Goal153C"
dotnet test ... --filter "FullyQualifiedName~CapabilityDrivenRuntimePlaythrough"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleLibrary"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleCertification"
dotnet test ... --filter "FullyQualifiedName~RuntimeNarrative"
dotnet test ... --filter "FullyQualifiedName~RuntimeEncounter"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Run artifact-scope validation.

No Unity and no standalone smoke.

## Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal154b-social-runtime-core.ps1
.devflow/scripts/run-goal154b-social-runtime-core.cmd

catalogs/feature-modules/optional/faction-reputation-standing.featuremodule.json
catalogs/feature-modules/optional/quest-faction-reputation-consequences.featuremodule.json
catalogs/feature-modules/optional/dialogue-reputation-gated-reward.featuremodule.json

src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughModels.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughValidator.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughPlanner.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleLibraryValidator.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModulePackageMutationService.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleRuntimeEffectModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleRuntimeEffectEvaluator.cs

src/LLMGameCreator.Runtime.Abstractions/CanonicalRuntimePlayerCommandLoopContracts.cs
src/LLMGameCreator.Runtime.Abstractions/SelectedRuntimeVariantInteractiveSessionContracts.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
src/LLMGameCreator.Runtime/SelectedRuntimeVariantInteractiveSessionService.cs
src/LLMGameCreator.Runtime/GameRuntimeService.cs
src/LLMGameCreator.Runtime/FactionRuntimeService.cs
src/LLMGameCreator.Runtime/QuestRuntimeService.cs
src/LLMGameCreator.Runtime/DialogueRuntimeService.cs
src/LLMGameCreator.Runtime/OutputApplier.cs
src/LLMGameCreator.Runtime/RequirementEvaluator.cs

tests/LLMGameCreator.Tests/Application/Goal154A/Goal154ASocialModuleContractTests.cs
tests/LLMGameCreator.Tests/Application/Goal154A/Goal154ASocialRuntimeLifecycleTests.cs
tests/LLMGameCreator.Tests/Application/Goal154A/Goal154ARollbackEventTruthTests.cs
tests/LLMGameCreator.Tests/Application/Goal154A/Goal154AParameterDomainTests.cs
tests/LLMGameCreator.Tests/Application/Goal154A/Goal154ASavedProjectStandaloneTests.cs

tests/LLMGameCreator.Tests/Application/Goal154B/Goal154BPlannerAndModuleTests.cs
tests/LLMGameCreator.Tests/Application/Goal154B/Goal154BClaimedAndLockedLifecycleTests.cs
tests/LLMGameCreator.Tests/Application/Goal154B/Goal154BRollbackAndEventTruthTests.cs
tests/LLMGameCreator.Tests/Application/Goal154B/Goal154BParameterAndMutationTests.cs
tests/LLMGameCreator.Tests/Runtime/Goal154B/Goal154BRuntimeSocialTests.cs

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

docs/agent-tasks/goal-154b-executable-social-runtime-lifecycle-core-closure/
.llmgc/procedural/goal-154b-executable-social-runtime-lifecycle-core-closure/
.llmgc/exports/goal-154b-executable-social-runtime-lifecycle-core-closure/
```

If exact compilation proves one more existing model/test path is required:

```text
record exact reason
add exact path only
do not broaden source prefixes
```

Forbidden:

```text
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/**
unity/**
samples/**
generator-library/**
provider/**
LLM/**
RAG/**
public GamePackage schema changes
```


## Compact evidence

Create exactly 8 files in each root, byte-identical by relative path:

```text
goal154b-dashboard.json
behavioral-test-inventory.json
capability-plan-proof.json
claimed-and-locked-lifecycle-proof.json
rollback-event-truth-proof.json
checkpoint-replay-proof.json
activated-package-and-artifact-scope-proof.json
goal154b-report.md
```

Dashboard must include:

```text
status
behavioralTestsDiscovered
behavioralTestsPassed
sourceContractTestsPassed
defaultClaimedPassed
stillLockedPassed
alreadyClaimedAtomic
upperClampPassed
lowerClampPassed
rollbackMatrixPassed
checkpointReloadPassed
fullReplayEquivalent
defaultOffHashesPreserved
activatedProofFixtureCount
artifactScopeViolationCount
goal154Accepted=false
goal154aAccepted=false
goal154bAccepted=false
manualGateReady=false
deferredTo=Goal154C
```

No field needed for GREEN may be `null`, `NOT_EXECUTED`, `PARTIAL` or inferred from source text.

Do not commit raw logs/TRX.

## Publication

Create exactly one final commit:

```text
GREEN Goal 154B executable social Runtime lifecycle core closure
```

or honest:

```text
BLOCKED Goal 154B executable social Runtime lifecycle core closure
FAILED Goal 154B executable social Runtime lifecycle core closure
```

Codex performs:

```powershell
git commit
git push origin main
git rev-parse HEAD
git rev-parse origin/main
git status --short
git rev-list --count <required-base>..HEAD
```

Required:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
Unity process start count=0
```

A coherent FAILED/BLOCKED result with task-owned changes must still be committed and pushed. Never ask
the owner to push manually.

## GREEN criteria

```text
at least 16 genuine behavioral Goal154B tests discovered and passed
placeholder/string/reflection tests not counted as lifecycle proof
all social command/presentation primitives supported
all social selectors resolved generically
module action graph explicitly opens/closes healer dialogue
default claimed lifecycle passes through actual Runtime
still_locked is a truthful replay-stable skip
direct second claim is state/event atomic
positive and negative clamp events are truthful
quest/dialogue rollback leaks no success events
structured numbers are invariant-culture
checkpoint continuation and full replay equivalent
nested mutations are behaviorally tested
activated package has no proof fixtures or artificial gold
default-off historical hashes preserved
Goal153 regressions remain GREEN
artifact scope has 0 violations
Goal154/154A/154B remain human-unaccepted
Goal154 manualGateReady=false
one final commit pushed
```

## Final report

Return exactly `GREEN`, `BLOCKED` or `FAILED`, then include:

- model/reasoning used;
- inherited foundation retained;
- placeholder tests versus behavioral test counts;
- supported primitives/selectors;
- final module action graph;
- default claimed lifecycle values;
- still_locked values and skip proof;
- second-claim result;
- clamp results;
- rollback matrix;
- checkpoint/replay hashes and event equivalence;
- mutation/default-off/package-diff proof;
- Goal153 regressions;
- artifact scope;
- Goal154/154A/154B flags;
- explicit deferred Goal154C scope;
- final commit/push/HEAD/worktree;
- confirmation no human acceptance was claimed.
