# Goal 154 — Faction Reputation, Quest Consequences & Trusted Dialogue Reward Vertical Slice

## Identity

- Task ID: `goal-154-faction-reputation-quest-consequences-and-trusted-dialogue-reward-vertical-slice`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `ad2e404f1c938113a0c111d4c1fe1bfb55e0e836`
- Required base message: `GREEN Goal 153C product proof separation and outcome-aware qualification hotfix`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Reason: this is a major product vertical slice through existing generic Runtime seams. It adds three
data-driven FeatureModules, reusable nested package mutations, dialogue-choice execution and a
complete save/replay/PlayerAdapter lifecycle. Runtime concepts already exist, so Sol is unnecessary
unless an unknown P0/P1 architecture defect is discovered.

## Pre-approval

The owner approved execution by launching this task.

- Produce a concise internal plan.
- The approved GOAL satisfies the AGENTS.md planning requirement; do not ask for confirmation.
- Begin after base/worktree checks.
- Do not request intermediate manual testing.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No validation-candidate commits.
- Codex performs commit and push itself.

## Mandatory orientation

Read in this order:

```text
AGENTS.md
docs/GOAL_DESIGN_QUALITY_POLICY.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/UNITY_EXECUTION_POLICY.md
```

The first implementation evidence must contain the completed Goal-design quality review. An
unanswered required review item blocks GREEN.

## Unity execution budget

```text
Unity Editor invocation budget: 0
Cached standalone hidden-smoke budget: 1
```

Do not modify Unity host source.
Starting `Unity.exe` is a P1 violation.
Use the existing generic standalone host cache only.

## First required deliverable — record human acceptance

Record the exact owner statement:

```text
Я принимаю Goals153/153A/153B/153C: механики «Активные способности», «Мана и заклинания» и «Эффекты по ходам» с параметрами 2/12/3/5/1 успешно сохранены, собраны и воспроизведены; урон 2, мана 12→9, пять срабатываний по 1 и завершение эффекта отображаются корректно; standalone переиспользовал host cache без запуска Unity Editor.
```

Required normalized state:

```text
goal153Accepted=true
goal153AcceptedByHuman=true
goal153AcceptedByCodex=false
goal153ManualReviewPerformed=true

goal153aAccepted=true
goal153aAcceptedByHuman=true
goal153aAcceptedByCodex=false
goal153aManualReviewPerformed=true

goal153bAccepted=true
goal153bAcceptedByHuman=true
goal153bAcceptedByCodex=false
goal153bManualReviewPerformed=true

goal153cAccepted=true
goal153cAcceptedByHuman=true
goal153cAcceptedByCodex=false
goal153cManualReviewPerformed=true

acceptedCommit=ad2e404f1c938113a0c111d4c1fe1bfb55e0e836
configuredValues=2/12/3/5/1
abilityDamage=2
manaBefore=12
manaAfter=9
statusTickCount=5
statusTickDamage=1
statusTerminalOutcome=expired
standaloneSelfCheckAccepted=true
standaloneReadable=true
standaloneNavigationAccepted=true
hostCacheReusedWithoutUnityEditor=true
rawManualInputNotCommitted=true
acceptedByCodex=false
```

Update:

```text
docs/manual-acceptance/active-abilities-mana-turn-status-featuremodules.md
docs/manual-acceptance/goal153a-parameter-domain-turn-binding-event-atomicity.md
docs/manual-acceptance/goal153b-declarative-parameter-constraints-domain-integrity.md
docs/manual-acceptance/goal153c-product-proof-separation.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
```

Remove or rewrite stale active narrative that still claims:

```text
goal153_target is activated product content
training target health is 1001001 in the product
2999 EndTurn actions are the current product result
Goal153 family remains unaccepted
Goal153B audit is still next
```

Historical Goal153A/153B evidence files remain immutable. Correct only current source-of-truth prose.

Write one compact acceptance record under Goal154 procedural/export roots.
Do not commit the screenshot or raw manual input.

Goal154 itself remains:

```text
accepted=false
acceptedByHuman=false
acceptedByCodex=false
manualReviewPerformed=false
```

## Product claim

Add a real social-consequence loop to the normal project workflow:

```text
Игры
→ Механики
→ enable faction reputation
→ enable quest reputation consequences
→ enable reputation-gated dialogue reward
→ configure typed values
→ save/reopen
→ build and qualify
→ quest completion changes faction reputation
→ trusted dialogue choice becomes available
→ one-time trusted reward is claimed
→ repeated claim is unavailable
→ save/checkpoint/replay are equivalent
→ standalone shows readable social consequences
```

This is not a report-only Goal.

## New default-off FeatureModules

### Module A

```text
moduleId=feature.faction.reputation_standing
title=Фракции и репутация
defaultSelected=false
dependencies:
  feature.player_adapter.runtime_summary
```

Purpose:

```text
author configurable starting reputation for a declared faction
surface current reputation in Runtime/PlayerAdapter
provide generic faction target selection and semantic observations
```

The first starter-content binding uses the existing user-facing `faction/village`.
It must not add a hidden faction or proof-only faction.

### Module B

```text
moduleId=feature.quest.faction_reputation_consequences
title=Последствия квестов для репутации
defaultSelected=false
dependencies:
  feature.faction.reputation_standing
  feature.quest.objective_chain
```

Purpose:

```text
configure reputation gained on quest completion
configure reputation lost on quest failure
prove clamping, event truth, save/replay and quest-state consequences
```

The first starter-content binding extends the existing `quest/help_healer`.
No dummy quest is allowed in the activated package.

### Module C

```text
moduleId=feature.dialogue.reputation_gated_reward
title=Репутационные ветки диалога
defaultSelected=false
dependencies:
  feature.faction.reputation_standing
  feature.quest.faction_reputation_consequences
  feature.dialogue.basic
```

Purpose:

```text
add a trusted dialogue choice to the existing healer dialogue
show it only when reputation reaches a configured threshold
grant one configured reward exactly once
hide the choice after the reward is claimed
```

The first starter-content choice belongs to `dialogue/healer`.
It is declared user-facing starter content, not qualification data.

## Default user configuration

```text
startingReputation=0
questReputationReward=10
questFailurePenalty=5
trustedReputationThreshold=10
trustedGoldReward=7
```

Expected default lifecycle:

```text
initial village reputation: 0
trusted choice before completion: unavailable
help-healer quest completes
village reputation: 0 → 10
trusted choice: available
trusted reward claimed once
gold: 0 → 7
claim flag: true
trusted choice after claim: unavailable
quest state: completed
```

Do not hardcode these values in Runtime.
They are module defaults and regression fixtures, not product limits.

## Exact non-goals

This Goal does not implement:

```text
faction AI
dynamic political simulation
automatic relation bands
procedural dialogue generation
merchant price formulas
multiple simultaneous quest branches
live LLM dialogue
Unity-specific social logic
```

It must leave expansion-safe seams for these future lanes.

## Product/proof separation

Activated product data may contain only:

```text
the existing village faction with configured default reputation
configured reputation reward/failure consequence on the existing healer quest
one declared trusted choice in the existing healer dialogue
one one-time-claim flag written at Runtime
```

Forbidden activated data:

```text
dummy faction
dummy quest
dummy dialogue
proof NPC
qualification inventory
artificial player gold injected into package
special health/capacity values
test-only reputation entity
```

Automated playthrough may advance an objective through a Runtime command. That command is proof
orchestration and must not become package content.

## Parameter-domain table

### `startingReputation`

```text
owner=feature.faction.reputation_standing
type=integer
minimum=-100
default=0
maximum=100
package field=faction/village.defaultReputation
Runtime field=FactionState.Reputation
playthrough effect=initial reputation observation
save/replay field=faction reputation
```

### `questReputationReward`

```text
owner=feature.quest.faction_reputation_consequences
type=integer
minimum=0
default=10
maximum=100
package field=quest/help_healer rewards reputation amount
Runtime effect=completion reputation delta
save/replay field=faction reputation + quest completed state
```

### `questFailurePenalty`

```text
owner=feature.quest.faction_reputation_consequences
type=integer magnitude
minimum=0
default=5
maximum=100
package field=quest/help_healer failure effect reputation amount = -magnitude
Runtime effect=failure reputation delta
save/replay field=faction reputation + quest failed state
```

### `trustedReputationThreshold`

```text
owner=feature.dialogue.reputation_gated_reward
type=integer
minimum=-100
default=10
maximum=100
package field=trusted choice reputation_at_least requirement
Runtime effect=choice availability
playthrough shape=conditional reward action
save/replay field=none beyond reputation/choice visibility
```

### `trustedGoldReward`

```text
owner=feature.dialogue.reputation_gated_reward
type=integer
minimum=0
default=7
maximum=1000
package field=trusted choice gold reward
Runtime effect=gold delta
save/replay field=gold resource + claimed flag
```

Required domain coverage:

```text
minimum
default
maximum
one interior non-default
below-minimum rejection
above-maximum rejection
negative/positive reputation clamping
threshold unreachable after quest
threshold already satisfied at start
reward zero
reward maximum
```

Do not reject a valid configuration merely because one quest does not reach the trusted threshold.
The build must truthfully report `still_locked`.

## A. Generic package mutation support

Extend the reusable FeatureModule mutation layer. No module-ID switch is allowed.

Required target kinds or equivalent generic operations:

```text
definition_numeric_property
  add `factions` to the supported collection set

quest_output_upsert
  target=questId|rewards|kind|id

quest_output_amount
  target=questId|rewards|kind|id

quest_failure_output_upsert
  target=questId|failureEffects|kind|id

quest_failure_output_amount
  target=questId|failureEffects|kind|id

dialogue_choice_upsert
  target=dialogueId|nodeId|choiceId

dialogue_choice_requirement_amount
  target=dialogueId|nodeId|choiceId|kind|id

dialogue_choice_reward_amount
  target=dialogueId|nodeId|choiceId|kind|id
```

Naming may differ, but behavior must be typed and reusable.

### A1. Upsert rules

```text
missing child -> add
byte/semantic-equivalent child -> idempotent
conflicting child -> causal failure
multiple matches -> causal failure
module order reversed -> byte-identical package
```

### A2. Trusted choice payload

The module-defined starter choice must be human-facing and equivalent to:

```text
choiceId=trusted_village_reward
text=Попросить награду за доверие
requirements:
  reputation_at_least faction/village <trustedReputationThreshold>
  flag_not_equals flag/village_trusted_reward_claimed true
effects:
  set_flag flag/village_trusted_reward_claimed true
rewards:
  resource/gold <trustedGoldReward>
closeDialogue=true
```

The exact display text may be improved, but the semantics must remain.

### A3. Quest consequences

The quest module must:

```text
upsert/configure completion reputation output for faction/village
upsert/configure failure reputation output with negative penalty magnitude
```

The failure amount must be derived declaratively from `questFailurePenalty` through the existing safe
numeric expression system. Do not add a module-specific C# negation branch.

### A4. Activated-package diff classification

Every new operation must carry one of:

```text
declared_user_facing_mechanic
declared_user_facing_starter_content
authoring_identity_metadata
```

Forbidden fixture count must remain zero.

## B. Generic requirement and dialogue semantics

### B1. `flag_not_equals`

Add generic requirement support:

```text
kind=flag_not_equals
id=<flag ID>
value=<forbidden value>
```

Behavior:

```text
missing flag -> passes when missing value is not equal to forbidden value
existing different value -> passes
existing equal value -> fails causally
```

Update the package validator and Runtime requirement evaluator generically.

No trusted-choice ID or flag ID branch in Runtime.

### B2. Dialogue choice availability

Use existing requirements to compute visible choice IDs.

Required behavior:

```text
before threshold: trusted choice absent
at threshold: trusted choice present
above threshold: trusted choice present
before claim: present when threshold satisfied
after claim flag=true: absent
```

Opening/reopening a dialogue must always recompute choice availability from current Runtime state.
No stale cached choice list.

### B3. One-time reward

The claim flag and gold reward are one atomic dialogue choice transaction.

On success:

```text
flag changes to true
gold changes exactly once
choice closes dialogue
choice absent on reopen
```

On any late failure:

```text
state byte-identical
flag unchanged
gold unchanged
no success events for rolled-back effects
choice remains available if it was available before
```

## C. Runtime event truth and transaction boundaries

### C1. Faction events

Both direct faction commands and generic output application should emit structured args:

```text
factionId
before
after
delta
clamped=true|false
```

`delta` reflects actual after-before, not merely requested amount.

### C2. Quest completion/failure atomicity

Quest objective progression, completion rewards and failure effects operate against a working clone.

If a later output fails:

```text
original quest state unchanged
objective amount unchanged
faction reputation unchanged
no QuestCompleted/QuestFailed/QuestRewardGranted success event
no FactionReputationChanged success event
no OutputApplied success event for rolled-back state
```

A structured failure/validation event is allowed.

### C3. Dialogue atomicity

If any choice stage fails after cost/effect/reward evaluation:

```text
original dialogue state unchanged
faction/gold/flag unchanged
no DialogueChoiceSelected/DialogueEffectApplied/DialogueClosed success event
no resource/reputation/flag success event from the rolled-back choice
```

Add direct Runtime and canonical command-loop regressions.

## D. Capability primitives and selectors

Add generic capability primitives:

```text
runtime.command.close_dialogue
runtime.command.advance_quest_objective
runtime.command.choose_dialogue_option
runtime.presentation.inspect_faction
runtime.presentation.inspect_dialogue_choices
```

Reuse existing GameRuntime command types. Do not invent parallel Runtime services.

Add generic target selectors:

```text
faction_id
quest_objective_id
dialogue_choice_id
```

### D1. `quest_objective_id`

Arguments:

```text
questId
objectiveId
```

Require exactly one objective in the declared quest.

### D2. `dialogue_choice_id`

Arguments:

```text
dialogueId
nodeId
choiceId
```

Require exactly one declared choice.

### D3. Conditional execution predicate

Extend the existing typed predicate vocabulary with:

```text
dialogue_choice_available
```

Required bindings:

```text
dialogueId
nodeId
choiceId
```

When false:

```text
handler not called
state unchanged
no gameplay event
truthful conditional-skip journal entry
checkpoint/full replay makes the same decision
```

Unknown predicates remain rejected.

## E. Product playthrough

Do not replace the core playthrough actions. Append a social-consequence chain after the existing
`start_or_update_quest` action and before later higher-order actions where dependencies permit.

Required default chain:

```text
1. inspect initial faction reputation
2. close any currently open core dialogue
3. open dialogue/healer before quest completion
4. inspect available choices and prove trusted choice absent
5. close dialogue
6. advance objective/collect_red_herbs by its declared required amount
7. observe automatic quest completion and reputation 0→10
8. checkpoint after quest completion, before trusted claim
9. open dialogue/healer again
10. inspect trusted choice present
11. choose trusted_village_reward conditionally
12. observe gold 0→7 and claim flag=true
13. reopen dialogue/healer
14. inspect trusted choice absent after claim
15. close dialogue
16. show faction/quest/dialogue/gold human summary
17. continue existing canonical plan
```

The objective amount must be read from the actual quest definition. Do not hardcode `3` in generic
production code.

If configured reputation remains below the threshold:

```text
trusted action skips truthfully
build remains GREEN
social terminal outcome=still_locked
no gold is granted
```

If starting reputation already satisfies the threshold:

```text
trusted choice is available before completion
quest completion still applies its configured reward
claim remains one-time
```

## F. Runtime effect contracts

Add reusable metrics or equivalent generic observations:

```text
faction_reputation_equals
faction_reputation_delta_equals
quest_state_equals
dialogue_choice_availability
resource_delta_equals
flag_equals
social_terminal_outcome
```

Required social terminal outcomes:

```text
trusted_reward_claimed
trusted_choice_still_locked
trusted_reward_already_claimed
```

Do not parse localized display text to establish semantic truth when structured Runtime state/event
args exist.

## G. PlayerAdapter and standalone facts

Normal WinForms build result and standalone payload must expose concise human facts:

```text
Фракция: Деревня
Репутация: 0 → 10
Квест: завершён
Доверенная реплика: заблокирована → доступна → использована
Золото: 0 → 7
Повторная награда: недоступна
```

For a still-locked valid configuration:

```text
Доверенная реплика: всё ещё недоступна
Золото: без изменений
```

Raw IDs and hashes remain technical details, not the primary human view.

## H. State/event/rollback matrix

Codex must write this completed matrix to compact evidence before implementation completion.

### H1. Quest completion success

```text
state:
  objective completed
  quest completed
  reputation increased and clamped
success events:
  QuestObjectiveUpdated
  QuestCompleted
  QuestRewardGranted
  FactionReputationChanged
  JournalUpdated
checkpoint:
  after completion before dialogue reward
replay:
  identical state/event sequence
```

### H2. Quest completion failure

```text
state:
  byte-identical to before command
failure events:
  ValidationFailed only, or equivalent structured failure
forbidden:
  rolled-back objective/quest/reputation success events
```

### H3. Quest failure success

```text
state:
  quest failed
  reputation decreased and clamped
success events:
  QuestFailed
  FactionReputationChanged
  JournalUpdated
```

### H4. Trusted dialogue choice success

```text
state:
  gold increased
  claim flag=true
  dialogue closed
success events:
  DialogueChoiceSelected
  DialogueEffectApplied
  ResourceChanged
  OutputApplied
  DialogueClosed
```

### H5. Trusted dialogue choice late failure

```text
state:
  byte-identical
success events:
  none
choice visibility:
  unchanged from before
```

### H6. Conditional choice skip

```text
state hash unchanged
handler not invoked
runtime event count=0
journal contains causal skip reason
replay identical
```

## I. Save/load/replay requirements

### I1. Default completion path

Checkpoint after quest completion and before trusted reward:

```text
quest state=completed
reputation=10
trusted choice not yet claimed
claim flag missing/not true
gold unchanged
```

After reload:

```text
trusted choice visible
claim produces gold=7
claim flag=true
reopen hides choice
final state hash equals uninterrupted route
runtime event sequence equivalent
```

### I2. Failure path

```text
start quest
fail quest
reputation applies -questFailurePenalty
trusted choice visibility recomputed
save/reload preserves failed quest and reputation
```

### I3. One-time claim

```text
save after claim
reload
reopen dialogue
trusted choice absent
gold remains unchanged
```

## J. Activated-package diff gate

Produce an actual structured diff between the base package and each selected-module composition.

Required classifications:

```text
faction default reputation -> declared_user_facing_mechanic
quest completion/failure outputs -> declared_user_facing_mechanic
trusted dialogue choice -> declared_user_facing_starter_content
trusted choice requirement/effect/reward values -> declared_user_facing_mechanic
```

Require:

```text
forbidden proof-fixture count=0
unclassified mutation count=0
unexpected global rule/capacity change count=0
no unrelated dialogue/quest/faction altered
```

## K. Composition coverage

Required focused compositions:

```text
faction module only
faction + quest consequences
faction + quest consequences + dialogue reward
three social modules + equipment/attributes/progression
three social modules + active ability/mana/status
all-current-optional composition
default-off historical composition
```

No full powerset.

Dependency-invalid combinations must reject before package mutation.

## L. Parameter-domain scenarios

Execute at least:

```text
Default:
  0/10/5/10/7
  outcome=trusted_reward_claimed

Still locked:
  -100/0/5/100/7
  outcome=trusted_choice_still_locked

Already trusted:
  100/0/5/10/7
  choice available before quest, claim once

Positive clamp:
  95/100/5/10/7
  final reputation=100, actual delta=5

Negative clamp:
  -95/0/100/10/7 on failure
  final reputation=-100, actual delta=-5

Zero reward:
  0/10/5/10/0
  claim flag=true, gold unchanged, no duplicate claim

Maximum gold:
  0/10/5/10/1000
  gold delta=1000
```

Also cover every parameter's min/default/max/interior and out-of-range rejection.

## M. Generic architecture scan

Generic production C# must contain no literals for:

```text
feature.faction.reputation_standing
feature.quest.faction_reputation_consequences
feature.dialogue.reputation_gated_reward
startingReputation
questReputationReward
questFailurePenalty
trustedReputationThreshold
trustedGoldReward
faction/village
quest/help_healer
dialogue/healer
trusted_village_reward
flag/village_trusted_reward_claimed
objective/collect_red_herbs
```

These literals are allowed in catalog JSON, focused tests, Goal154 docs and compact evidence only.

Stable generic protocol vocabulary is allowed:

```text
flag_not_equals
reputation_at_least
dialogue_choice_available
faction_id
quest_objective_id
dialogue_choice_id
```

## N. Real saved-project lifecycle

Use the read-only actual source project:

```text
C:\Users\endim\AppData\Local\LLMGameCreator\Games\goal148-manual
```

Only a short disposable LocalAppData copy may be modified.

Required:

```text
open existing project
select all three new modules
set 0/10/5/10/7
save
close/reopen
build
repeat deterministic build
checkpoint/replay
assemble standalone payload
source project byte-identical
identity preserved
failed invalid-parameter build does not replace last valid composition
```

The activated package must contain no dummy faction/quest/dialogue/inventory.

## O. Standalone and Unity discipline

Use existing host cache only.

Required:

```text
HostReused=true
HostRebuilt=false
Unity process start count=0
one hidden standalone smoke
all self-check markers GREEN
payload facts show reputation, quest, choice, gold and one-time claim
payload contains no proof fixture
```

If cache is absent or invalid, publish BLOCKED. Do not launch Unity Editor.

## P. Goal-design pre-commit self-audit

Before commit, write machine-readable answers:

```text
Which valid parameter combinations were not fully executed?
Which activated data is starter content, and why is it user-facing?
Does any proof-only faction/quest/dialogue/inventory enter the product package?
Which new literals appear in generic production code?
Can a failed quest/dialogue operation leak success events?
Can a maximum valid value create invalid Runtime state?
Can the reward be claimed more than once?
Can an old saved project behave differently with new modules disabled?
Which conditional actions may skip, and are those skips replay-stable?
```

Any unanswered question or any proof-only activated data blocks GREEN.

## Q. Current-state routing

On GREEN:

```text
goal153Accepted=true
goal153aAccepted=true
goal153bAccepted=true
goal153cAccepted=true

goal154ImplementationStatus=GREEN
goal154Accepted=false
goal154AcceptedByHuman=false
goal154AcceptedByCodex=false
goal154ManualReviewPerformed=false
goal154ManualGateReady=true

nextAction=independent_goal154_audit_then_short_social_consequence_human_gate
```

Do not claim Goal154 human acceptance.

## R. Manual gate after independent audit

Keep the eventual human gate short:

```text
1. Enable the three social modules.
2. Use default values 0/10/5/10/7.
3. Build and confirm one GREEN social-consequence card.
4. Save/reopen and confirm values remain.
5. Launch cached standalone and confirm reputation 0→10, quest completed,
   trusted choice claimed once, gold 0→7 and repeat claim unavailable.
```

No manual hash or raw-event comparison.

## Command and investigation budget

Mandatory budget:

```text
read-first: maximum 12 primary files
Goal-design/pre-mortem pass: maximum 8 minutes
mutation/authoring implementation: maximum 15 minutes
Runtime/playthrough implementation: maximum 15 minutes
focused tests: maximum 18 minutes
real-project/cache proof: maximum 6 minutes
total target wall clock: 55 minutes
maximum two testhost processes
Unity Editor process count: 0
```

Command rules:

```text
no unchanged command repetition
no timeout escalation chain
isolate a failing filter before retry
one long command at a time
kill only owned test/build processes
```

Forbidden:

```text
full suite
85-case historical closure
all-ProductSmoke
historical snapshot repair
Unity host build
Unity.exe
manual user cleanup
raw logs/TRX in commit
```

## Required validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal154"
dotnet test ... --filter "FullyQualifiedName~Goal153C"
dotnet test ... --filter "FullyQualifiedName~Goal153B"
dotnet test ... --filter "FullyQualifiedName~Goal153A"
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

Then run exactly one existing-cache hidden standalone smoke with Unity process start count asserted
as zero.

## Artifact scope

Initially allowed:

```text
AGENTS.md
docs/GOAL_DESIGN_QUALITY_POLICY.md
docs/CONTEXT_INDEX.md

.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal154-faction-reputation-social-consequences.ps1
.devflow/scripts/run-goal154-faction-reputation-social-consequences.cmd

catalogs/feature-modules/manifest.json
catalogs/feature-modules/optional/faction-reputation-standing.featuremodule.json
catalogs/feature-modules/optional/quest-faction-reputation-consequences.featuremodule.json
catalogs/feature-modules/optional/dialogue-reputation-gated-reward.featuremodule.json

src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughModels.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughValidator.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughPlanner.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughExpansionService.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleLibraryValidator.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleLibraryFingerprintService.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterBindingService.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModulePackageMutationService.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleRuntimeEffectModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleRuntimeEffectEvaluator.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs

src/LLMGameCreator.Application/Validation/EncounterDefinitionValidator.cs

src/LLMGameCreator.Runtime.Abstractions/RuntimeContracts.cs
src/LLMGameCreator.Runtime.Abstractions/RuntimeServiceContracts.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
src/LLMGameCreator.Runtime/SelectedRuntimeVariantInteractiveSessionService.cs
src/LLMGameCreator.Runtime/RequirementEvaluator.cs
src/LLMGameCreator.Runtime/OutputApplier.cs
src/LLMGameCreator.Runtime/FactionRuntimeService.cs
src/LLMGameCreator.Runtime/QuestRuntimeService.cs
src/LLMGameCreator.Runtime/DialogueRuntimeService.cs
src/LLMGameCreator.Runtime/GameRuntimeService.cs

src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/Goal154/Goal154FeatureModuleContractTests.cs
tests/LLMGameCreator.Tests/Application/Goal154/Goal154PackageMutationAndDiffTests.cs
tests/LLMGameCreator.Tests/Application/Goal154/Goal154CapabilityPlaythroughTests.cs
tests/LLMGameCreator.Tests/Application/Goal154/Goal154ParameterDomainTests.cs
tests/LLMGameCreator.Tests/Application/Goal154/Goal154ArchitecturePolicyTests.cs
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal154FactionSocialWorkspaceTests.cs
tests/LLMGameCreator.Tests/Runtime/Goal154/Goal154FactionQuestDialogueRuntimeTests.cs
tests/LLMGameCreator.Tests/WinForms/Goal154ProjectsPageSocialSummaryTests.cs

# Exact existing tests may be amended only when required by compile/regression:
tests/LLMGameCreator.Tests/Application/FeatureModuleAuthoring/FeatureModuleLibraryAndParameterTests.cs
tests/LLMGameCreator.Tests/Application/FeatureModuleCertification/FeatureModuleCertificationAndCoverageTests.cs
tests/LLMGameCreator.Tests/RuntimeNarrativeTests.cs

docs/manual-acceptance/active-abilities-mana-turn-status-featuremodules.md
docs/manual-acceptance/goal153a-parameter-domain-turn-binding-event-atomicity.md
docs/manual-acceptance/goal153b-declarative-parameter-constraints-domain-integrity.md
docs/manual-acceptance/goal153c-product-proof-separation.md
docs/manual-acceptance/goal154-faction-reputation-social-consequences.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

docs/agent-tasks/goal-154-faction-reputation-quest-consequences-and-trusted-dialogue-reward-vertical-slice/
.llmgc/procedural/goal-154-faction-reputation-quest-consequences-and-trusted-dialogue-reward-vertical-slice/
.llmgc/exports/goal-154-faction-reputation-quest-consequences-and-trusted-dialogue-reward-vertical-slice/
```

If exact compilation/reproduction proves one additional existing Application/Runtime model or test
path is required:

```text
record exact reason
add only that exact path to artifact scope
never broaden to an entire unrelated subtree
```

Forbidden committed paths:

```text
unity/**
samples/**
generator-library/**
provider/**
LLM/**
RAG/**
.llmgc/manual/**
user project files
standalone build output
host cache
```

No public GamePackage schema change unless a P0 blocker is proven. The expected implementation uses
existing dialogue/quest/faction definitions plus internal FeatureModule contracts.

## Compact evidence

Maximum 10 files in each root, byte-identical procedural/export pairs:

```text
goal154-dashboard.json
goal153-family-human-acceptance-record.json
goal-design-quality-review.json
featuremodule-contracts-and-parameter-domain-proof.json
activated-package-diff-proof.json
runtime-state-event-rollback-proof.json
social-playthrough-checkpoint-replay-proof.json
real-project-and-cached-standalone-proof.json
artifact-scope-proof.json
goal154-report.md
```

`goal154-dashboard.json` must include:

```text
status
implementationStatus
manualGateReady
accepted
acceptedByHuman
acceptedByCodex
manualReviewPerformed
newModuleCount
configuredParameterCount
initialReputation
finalReputation
questState
trustedChoiceBefore
trustedChoiceAfterQuest
trustedChoiceAfterClaim
goldBefore
goldAfter
oneTimeClaimPassed
checkpointReplayPassed
fullReplayEquivalent
activatedProofFixtureCount
hostReused
hostRebuilt
unityProcessStartCount
artifactScopeViolationCount
```

Do not synthesize evidence fields without executable source tests.

## Publication

Create exactly one final commit:

```text
GREEN Goal 154 faction reputation quest consequences and trusted dialogue reward vertical slice
```

or an honest `BLOCKED`/`FAILED` commit.

Codex must push it.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit after required base
Unity process start count=0
Goals153/153A/153B/153C accepted=true by human
Goal154 accepted=false
Goal154 manualGateReady=true only on GREEN
```

## GREEN criteria

```text
exact Goal153-family human acceptance recorded
stale goal153_target/1001001/old-next-action narrative removed from current source-of-truth
three default-off social FeatureModules registered
five typed parameters saved/reopened and data-bound
generic nested mutations add no module-ID branches
flag_not_equals works generically
trusted reward is one-time and replay-stable
quest completion and failure reputation consequences are transactional
failed quest/dialogue operations leak no success events
faction events expose before/after/actual delta/clamp
choice availability recomputes from current state
unreachable threshold is truthful GREEN still_locked, not false failure
default path yields 0→10 reputation and 0→7 gold
positive/negative clamps report actual deltas
activated package has zero proof fixtures and zero unclassified mutations
old modules/default-off hashes preserved
all-current-optional composition GREEN
real saved-project lifecycle GREEN
standalone host reused with zero Unity starts
procedural/export evidence byte-identical
focused validation GREEN
artifact scope 0 violations
Goal154 remains human-unaccepted
one final commit pushed
```

## Final report

Return `GREEN`, `BLOCKED` or `FAILED`, then include:

- model/reasoning used;
- Goal153-family acceptance record;
- new module IDs and versions;
- five parameters and tested domain rows;
- activated package diff classifications;
- initial/completed/failed/clamped reputation outcomes;
- trusted choice locked/unlocked/claimed/repeat result;
- gold before/after;
- dialogue/quest rollback event-atomicity results;
- checkpoint/replay hashes and event equivalence;
- composition/default-off regressions;
- real-project lifecycle and source immutability;
- cache reuse and Unity process count;
- evidence mirror/schema result;
- focused tests;
- artifact scope;
- Goal154 acceptance flags;
- short five-step manual gate;
- final commit/push/HEAD/worktree;
- confirmation no Goal154 human acceptance was claimed.
