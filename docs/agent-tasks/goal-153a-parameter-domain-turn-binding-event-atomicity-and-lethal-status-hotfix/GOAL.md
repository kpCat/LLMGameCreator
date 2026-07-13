# Goal 153A — Parameter-Domain Turn Binding, Event Atomicity & Lethal-Status Hotfix

## Identity
- Task ID: `goal-153a-parameter-domain-turn-binding-event-atomicity-and-lethal-status-hotfix`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `8664b19c8fddb60e347402d0dc92535630c99cf3`
- Required base message: `GREEN Goal 153 active abilities mana and turn-status FeatureModules vertical slice`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration
```text
Model: GPT-5.6 Sol
Reasoning effort: High
```

Reason: bounded P1 closure across generic Runtime turn semantics, transactional event truth and
data-driven playthrough expansion. Extra High is not required.

## Pre-approval
- The owner approved execution by launching this task.
- Produce a concise internal plan, but do not ask for confirmation.
- Do not request intermediate manual testing.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No validation-candidate commits.

## Unity execution budget
```text
Unity Editor invocation budget: 0
Cached standalone hidden-smoke budget: 1
```
Read and obey `docs/UNITY_EXECUTION_POLICY.md`. Do not modify Unity host source.

## Current state
Goals152/152A/152C remain accepted by human.
Goal153 is implemented but not accepted.

Until this hotfix is GREEN:
```text
goal153Accepted=false
goal153AcceptedByHuman=false
goal153AcceptedByCodex=false
goal153ManualReviewPerformed=false
goal153ManualGateReady=false
goal153ImplementationStatus=BLOCKED_PENDING_GOAL153A

goal153aAccepted=false
goal153aAcceptedByHuman=false
goal153aAcceptedByCodex=false
goal153aManualReviewPerformed=false
```

## Independent audit findings

### P1-A — duration parameter is not truly supported
`statusDurationTurns` allows `1..1000`, but the Goal153 catalog hardcodes exactly two target ticks.
No binding expands the playthrough according to the configured duration. Values above 2 therefore
remain valid in the UI but cannot reach expiry in qualification.

### P1-B — EndTurn action binding is ignored
The playthrough resolves target participant IDs for `runtime.command.end_turn`, but the canonical
handler creates a plain EndTurn command. Runtime may end another participant's turn while the plan
claims the target turn was executed.

### P1-C — rolled-back state can leak success events
Ability and status processing use cloned state, but events produced before a later failure may still
be returned and recorded in canonical snapshots:
```text
CostConsumed
DamageApplied
HealingApplied
StatusAdded
AbilityUsed
StatusTicked
ParticipantDefeated
EncounterEnded
```
No event may claim a mutation that was not committed.

### P1-D — lethal status damage does not resolve defeat
After status tick damage, Runtime advances the turn without generic defeat detection and encounter
resolution. A participant can have health 0 while `Alive=true`.

### P1-E — qualification fixture does not cover allowed parameter domain
Training-target health is fixed at 30 while valid values allow damage/ticks/duration up to 1000.
Many individually valid settings can kill the target before lifecycle/expiry proof.

## Primary objectives
1. Derive status lifecycle qualification from actual composed package duration.
2. Bind every generated EndTurn to the expected current participant.
3. Make failed ability/status operations event-atomic and state-atomic.
4. Resolve lethal status ticks via normal defeat/win/loss/end semantics.
5. Keep modules default-off and generic.
6. Preserve accepted old behavior/hashes.
7. Keep Unity process count at zero.


## A. Expected-current-participant EndTurn

### A1. Command contract
Use the existing command surface generically:
```text
GameRuntimeCommand.Type=EndTurn
GameRuntimeCommand.TargetId=<expected current participant ID, optional>
```
Existing callers without a target keep current behavior.

### A2. Runtime validation
`EncounterRuntimeService.EndTurn` accepts an optional expected participant ID.

When provided, the current participant must match. Otherwise return:
```text
encounter.turn.expected_participant_mismatch
```
with:
```text
Success=false
state byte-identical
turn/round unchanged
cooldowns/statuses unchanged
no successful turn/status events
```

### A3. Canonical binding
`CanonicalRuntimePlayerCommandLoopService` passes `step.ResolvedTargetId` into EndTurn.
The action-binding proof validates every expanded EndTurn participant.

## B. Data-driven status lifecycle expansion

Remove the fixed two-tick turn chain from `feature.combat.active_ability_loadout`.
The active module proves ability use/direct damage only.
The status module owns lifecycle expansion and expiry.
The mana module depends on ability use rather than a fixed expiry action.

### B1. Generic contract
Extend `FeatureModuleRuntimePlaythroughContract`/resolved actions, or add an equivalent generic
descriptor:
```text
kind=end_turn_until_status_expired
encounterId=<data>
targetParticipantId=<data>
statusId=<data>
sourceAbilityId=<data>
checkpointAfterTick=1
```
All IDs come from module data. No C# branch on module/ability/status/fixture IDs.

### B2. Planner behavior
The planner must inspect the composed package and:
1. resolve encounter participant order;
2. resolve the active ability's add-status effect;
3. read actual configured duration;
4. generate deterministic explicit EndTurn actions until that many target ticks occur;
5. attach expected current participant ID to every generated EndTurn;
6. mark the first target-tick action as checkpoint boundary;
7. keep the logical terminal action ID stable for downstream dependencies;
8. generate unique deterministic IDs and plan signature.

Required full qualification:
```text
duration=1
duration=2
duration=5
```

Required plan-only proof:
```text
duration=1000
target tick count=1000
all generated action IDs unique
all expected participant IDs present
deterministic bounded plan generation
```
Do not execute a 1000-tick full replay.

## C. Qualification target survivability
The training target is fixture data, not a gameplay limit.

Ensure:
```text
trainingTargetHealth >
maxAbilityBaseDamage + maxStatusTickDamage * maxStatusDurationTurns
```
or derive health from effective configured values plus safety margin.

Do not reduce user parameter ranges to fit the old health=30 fixture.

Representative full qualification:
```text
abilityBaseDamage=100
statusTickDamage=50
statusDurationTurns=5
target survives until configured expiry
```

## D. Event transactionality

### D1. Ability failure
If any later ability cost/effect/validation fails:
```text
state byte-identical
no CostConsumed
no DamageApplied/HealingApplied
no StatusAdded/Removed
no AbilityUsed
no ParticipantDefeated/EncounterEnded
```
A structured failure event is allowed.

### D2. EndTurn/status failure
If any status in the current EndTurn fails:
```text
state byte-identical
turn/round unchanged
all success events generated by the failed EndTurn discarded
```
This includes: first status succeeds, second status fails.

### D3. Canonical snapshots
A failed canonical step may not contain events claiming uncommitted mutations.
Add a regression through the actual canonical command-loop.

## E. Lethal status resolution
After all current-participant status effects succeed and before turn advancement:
1. run generic defeat detection;
2. run generic encounter completion/resolution;
3. if encounter ended, do not advance;
4. emit standard defeat/win/loss/end events;
5. grant rewards/consequences exactly once.

Required:
```text
status kills last enemy -> defeated + won + ended
status kills player -> defeated + lost + ended
nonlethal status -> normal decrement/expiry/turn advance
```

## F. Mana parameter relation
Do not silently proceed when:
```text
abilityManaCost > startingMana
```
Preferred: declarative cross-parameter constraint before activation.
Acceptable: actionable parameter-stage diagnostic before Runtime playthrough.

Diagnostic must name both parameter IDs and actual values.
Do not clamp or rewrite values.


## G. Save/load/replay
For duration 5:
```text
ability applies status
first status tick occurs
checkpoint captured
remainingTicks=4
reload preserves status provenance, mana, turn and round
continuation produces four more ticks
status expires
uninterrupted and resumed event sequences are equivalent
final hashes equal
```
Duplicate application must refresh one logical status without duplicates.

## H. Standalone/cache
No Unity host changes.

Required:
```text
HostReused=true
HostRebuilt=false
Unity process start count=0
all five hidden-smoke markers GREEN
human facts include:
  ability damage
  mana start/cost/remaining
  tick damage
  configured duration
  expiry
```

## I. State publication
On GREEN:
```text
goal153ImplementationStatus=GREEN
goal153ManualGateReady=true
goal153Accepted=false
goal153AcceptedByHuman=false

goal153aImplementationStatus=GREEN
goal153aManualGateReady=true
goal153aAccepted=false
goal153aAcceptedByHuman=false

nextAction=perform_goal153_combined_human_gate
```
No human acceptance may be claimed.

## Required regressions
```text
EndTurn expected participant succeeds
EndTurn participant mismatch is state/event atomic
late ability-effect failure leaks no success events
multi-status tick where second fails leaks no first-status events
canonical failed snapshot has no uncommitted events
lethal enemy status tick resolves victory
lethal player status tick resolves loss
nonlethal tick/expiry preserved
duration 1 full qualification
duration 2 full qualification
duration 5 checkpoint/replay qualification
duration 1000 plan-only expansion
high representative damage fixture survives
mana cost > starting mana rejected causally
all three modules with equipment/attributes/progression
all-current-optional catalog
disabled new modules preserve historical hashes
real goal148-manual disposable save/reopen
```

Do not weaken Goal153 tests. Extend them.

## Command and investigation budget
```text
read-first: maximum 10 primary files
architecture/root-cause pass: maximum 8 minutes
Runtime/planner implementation: maximum 15 minutes
focused tests: maximum 15 minutes
real-project/cache proof: maximum 5 minutes
total target: 43 minutes
maximum two testhost processes
Unity Editor process count: 0
```

Forbidden:
```text
full suite
85-case historical closure
all-ProductSmoke
historical snapshot repair
Unity host build
unchanged command repetition
```

## Required validation
```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal153A"
dotnet test ... --filter "FullyQualifiedName~Goal153"
dotnet test ... --filter "FullyQualifiedName~CapabilityDrivenRuntimePlaythrough"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"
dotnet test ... --filter "FullyQualifiedName~RuntimeEncounter"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```
Then one cached hidden standalone smoke, asserting zero Unity processes.

## Artifact scope
Initially allowed:
```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal153a-parameter-domain-turn-atomicity-hotfix.ps1
.devflow/scripts/run-goal153a-parameter-domain-turn-atomicity-hotfix.cmd

catalogs/feature-modules/optional/combat-active-ability-loadout.featuremodule.json
catalogs/feature-modules/optional/magic-mana-spellcasting.featuremodule.json
catalogs/feature-modules/optional/status-turn-effects.featuremodule.json

src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughModels.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughValidator.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughPlanner.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughExpansionService.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterBindingService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs

src/LLMGameCreator.Runtime.Abstractions/RuntimeContracts.cs
src/LLMGameCreator.Runtime.Abstractions/RuntimeServiceContracts.cs
src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
src/LLMGameCreator.Runtime/GameRuntimeService.cs
src/LLMGameCreator.Runtime/EncounterRuntimeService.cs
src/LLMGameCreator.Runtime/SelectedRuntimeVariantInteractiveSessionService.cs

tests/LLMGameCreator.Tests/Runtime/Goal153/Goal153AbilityManaStatusRuntimeTests.cs
tests/LLMGameCreator.Tests/Runtime/Goal153A/Goal153ATurnBindingEventAtomicityTests.cs
tests/LLMGameCreator.Tests/Application/Goal153/Goal153FeatureModuleDefinitionUpsertTests.cs
tests/LLMGameCreator.Tests/Application/Goal153A/Goal153AParameterizedLifecyclePlannerTests.cs
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal153AbilityManaStatusWorkspaceTests.cs
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal153AWorkspaceParameterDomainTests.cs

docs/manual-acceptance/active-abilities-mana-turn-status-featuremodules.md
docs/manual-acceptance/goal153a-parameter-domain-turn-binding-event-atomicity.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

docs/agent-tasks/goal-153a-parameter-domain-turn-binding-event-atomicity-and-lethal-status-hotfix/
.llmgc/procedural/goal-153a-parameter-domain-turn-binding-event-atomicity-and-lethal-status-hotfix/
.llmgc/exports/goal-153a-parameter-domain-turn-binding-event-atomicity-and-lethal-status-hotfix/
```

If another existing Runtime/Application test path is required, add only that exact path with reason.

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
Maximum 10 files per root:
```text
goal153a-dashboard.json
parameter-domain-expansion-proof.json
turn-binding-proof.json
event-atomicity-proof.json
lethal-status-resolution-proof.json
duration5-checkpoint-replay-proof.json
mana-constraint-proof.json
cached-standalone-proof.json
artifact-scope-proof.json
goal153a-report.md
```
Do not commit raw logs/TRX or copied projects.

## Publication
Create exactly one final commit:
```text
GREEN Goal 153A parameter-domain turn binding event atomicity and lethal-status hotfix
```
or honest BLOCKED/FAILED.

Codex must push it.

Required final state:
```text
HEAD == origin/main
worktree clean
Unity process start count=0
Goal153/Goal153A accepted=false
Goal153 manualGateReady=true only on GREEN
```

## GREEN criteria
```text
duration is data-driven, not fixed at two ticks
duration 1/2/5 full qualification GREEN
duration 1000 plan-only deterministic
every generated EndTurn proves expected participant
failed ability/status operations leak no success events
canonical failed snapshots contain no uncommitted events
lethal status resolves defeat/encounter correctly
qualification target survives allowed parameter domain
mana cross-parameter invalidity is causal
duration5 checkpoint/replay equivalent
accepted old modules/hashes preserved
real project lifecycle GREEN
cached hidden smoke GREEN
Unity invocation count=0
artifact scope 0 violations
Goal153/Goal153A remain human-unaccepted
one final commit pushed
```

## Final report
Return GREEN, BLOCKED or FAILED and include:
- model/reasoning used;
- exact root causes/fixes;
- duration 1/2/5 and duration1000 results;
- generated action counts and turn binding;
- event-atomicity cases;
- lethal enemy/player status results;
- mana constraint;
- checkpoint/replay hashes/events;
- old-module/hash regressions;
- real-project lifecycle;
- cache reuse and Unity process count;
- focused tests;
- artifact scope;
- Goal153/Goal153A flags;
- short combined manual gate;
- final commit/push/HEAD/worktree;
- confirmation no human acceptance claimed.
