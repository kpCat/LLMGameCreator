# Goal 153C — Product/Proof Separation + Outcome-Aware Qualification Hotfix

## Identity

- Task ID: `goal-153c-product-proof-separation-and-outcome-aware-qualification-hotfix`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `b9c8a83453fcf5a262c4dd7b368252c53c35860b`
- Required base message: `GREEN Goal 153B declarative parameter constraints domain integrity and Goal quality gate hotfix`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Sol
Reasoning effort: High
```

Reason: this is a bounded P1 architecture correction. It removes qualification-only data from the
activated product package and adds generic outcome-aware capability execution while preserving the
Goal153/153A/153B Runtime semantics. Extra High is not required.

## Pre-approval

The owner approved execution by launching this task.

- Produce a concise internal plan, but do not ask for confirmation.
- Begin after base/worktree checks.
- Do not request intermediate manual testing.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No validation-candidate commits.

## Unity execution budget

```text
Unity Editor invocation budget: 0
Cached standalone hidden-smoke budget: 1
```

Read and obey `docs/UNITY_EXECUTION_POLICY.md`.
Do not modify Unity host source.

## Power-interruption context

The previous Codex session was interrupted by an accidental computer shutdown and later resumed.
That interruption is not itself a defect. The required base is a single published commit above
Goal153A. This Goal must nevertheless validate the actual repository state and must not rely on any
untracked session-local data.

## Current acceptance state

Goals152/152A/152C remain accepted by the owner.

Goals153/153A/153B remain unaccepted.

Until Goal153C is GREEN and independently audited:

```text
goal153ManualGateReady=false
goal153aManualGateReady=false
goal153bManualGateReady=false
goal153ImplementationStatus=BLOCKED_PENDING_GOAL153C

goal153cAccepted=false
goal153cAcceptedByHuman=false
goal153cAcceptedByCodex=false
goal153cManualReviewPerformed=false
```

Do not claim human acceptance.

## Independent audit findings

### P1-A — proof fixture is activated as game content

The active-ability module currently inserts into the actual user package:

```text
encounter participant:
  id=goal153_target
  title=Магическая мишень
  health=1001001
```

The module title is `Активные способности`; it does not tell the user that selecting it adds a
near-indestructible third combat participant to `encounter/goblin_duel`.

This target exists only so qualification can survive the full parameter range. It is proof data, not
a declared game mechanic or user-authored content.

### P1-B — proof requirement changes global health rules

The same module changes:

```text
resource/health.maxValue: 30 -> 1001001
```

This affects every health resource in the activated game, including healing/clamping semantics,
solely to support the proof target.

A mechanic module may not globally rewrite product health capacity to make its test scenario pass.

### P1-C — Goal quality policy was not enforced on its own Goal

`docs/GOAL_DESIGN_QUALITY_POLICY.md` requires explicit product/proof separation.

Goal153B evidence admits that the training target is “first-vertical-slice data”, but does not prove
that it is legitimate user-facing product content.

Labeling proof data as first-slice data is not sufficient.

### P2-A — evidence contract is incomplete

Goal153B committed only the procedural evidence root; the export mirror is absent.

`goal153b-dashboard.json` contains a real-project lifecycle payload rather than a Goal153B dashboard
schema with gate and acceptance fields.

This does not cause the gameplay P1, but evidence must be corrected without rewriting historical
Goal153B files.

### P2-B — module versions did not change with semantic contracts

The Goal153 module contracts changed materially across hotfixes while `moduleVersion` remained
`1.0.0`.

Current module versions must be advanced consistently and tested.

## Primary objectives

1. Remove every qualification-only participant and special capacity mutation from activated product
   packages.
2. Keep qualification fixtures strictly in in-memory tests/certification helpers.
3. Make project qualification operate on real project content.
4. Make the status lifecycle accept correct terminal gameplay outcomes:
   expiry, target defeat, or encounter termination.
5. Ensure generated conditional actions never fabricate success or failure after the encounter has
   already ended.
6. Keep default/moderate values proving real ticks and expiry.
7. Keep high valid values proving lethal outcomes without dummy content.
8. Preserve declarative constraints, resource-domain validation, event atomicity and replay.
9. Add an enforceable activated-package-diff quality gate for future FeatureModule Goals.
10. Produce correct mirrored compact evidence.
11. Keep Unity process count at zero.

## Non-negotiable activated-package contract

With the three Goal153 modules selected, the activated product package must not contain:

```text
goal153_target
Магическая мишень
training target
qualification target
proof target
resource/health.maxValue=1001001
any participant or definition whose only role is automated qualification
```

The package must retain the baseline health definition:

```text
resource/health.defaultValue=30
resource/health.minValue=0
resource/health.maxValue=30
```

unless a user-facing health/progression module explicitly changes it in a future Goal.

The active-ability module may add:

```text
the data-defined ability
the player's ability reference
human-facing ability summary/contracts
```

The mana module may add/adjust mana data.
The status module may add the status definition and ability status effect.

## A. Remove proof data from product mutations

From `feature.combat.active_ability_loadout`, remove:

```text
active.02_training_target
active.02a_training_target_health_capacity
```

and remove them from lineage/fingerprints.

Do not replace them with another hidden dummy participant.

Bump semantic module versions:

```text
active ability module: >=1.1.0
mana module: >=1.1.0
status module: >=1.1.0
```

Use consistent semantic-version reasoning and test it.

## B. Generic real-content target selection

Add a generic target selector to the capability planner, for example:

```text
hostile_encounter_participant
```

Contract arguments:

```text
encounterId
sourceParticipantId
selectionPolicy=first_by_encounter_order
```

Required behavior:

```text
resolve source participant exactly once
select an alive participant on another team
deterministic encounter-order selection
zero matches -> causal failure
ambiguous policy -> causal failure
no module/ability/status/entity ID branch
```

Update product ability/status contracts to use this selector.

The current baseline should select the existing `goblin`, but no production C# or module contract may
name `goblin` as a special case.

## C. Outcome-aware status lifecycle

### C1. Terminal outcomes

The lifecycle is valid when one of these real gameplay outcomes occurs:

```text
status expires after configured ticks
target is defeated
encounter ends because of win/loss
```

Default/moderate values must still prove the exact configured ticks and expiry.

High damage must prove the lethal terminal path instead of requiring an artificial survivor.

### C2. Conditional action execution

Extend playthrough actions/contracts with generic execution predicates or an equivalent typed model.

Required predicates:

```text
encounter_active
participant_alive
status_present
```

Expanded EndTurn actions for status lifecycle execute only while their declared conditions hold.

When a condition is false:

```text
action is deterministically skipped
Runtime handler is not called
state hash unchanged
no gameplay success event emitted
snapshot/action journal records a truthful skip reason
replay makes the same skip decision
required action does not become a false failure when its terminal outcome was already satisfied
```

Do not implement condition strings through arbitrary scripting.

### C3. Terminal observation

Add a generic Runtime-effect observation such as:

```text
status_terminal_outcome
```

It reports one of:

```text
expired
target_defeated
encounter_won
encounter_lost
encounter_ended
```

The status module's project contract accepts the appropriate terminal set.

The module self-tests must still separately verify exact expiry and lethal enemy/player behavior.

### C4. Checkpoint behavior

For a nonlethal duration-5 scenario:

```text
checkpoint after first tick
remainingTicks=4
reload and uninterrupted paths equivalent
four additional ticks
terminal outcome=expired
```

For immediate lethal ability damage:

```text
checkpoint/replay remain valid
remaining EndTurn actions skip truthfully
terminal outcome=target_defeated or encounter_ended
```

## D. Parameter-domain proof must remain proof-only

The max-domain survivability test must construct or adjust its high-capacity participant only in:

```text
test-local in-memory package
temporary certification package outside activated project output
```

It must never modify module `MutationOperations` or the real project package.

The proof derives:

```text
abilityBaseDamage.maximum
statusTickDamage.maximum
statusDurationTurns.maximum
```

from declarations and uses checked arithmetic.

Required test assertions:

```text
test fixture high capacity is sufficient
activated product package has no fixture
activated product health max remains 30
fixture package and product package have distinct hashes
fixture bytes are never activated, saved or sent to standalone payload
```


## E. Default-constraint and domain quality

Keep Goal153B declarative constraints.

Additionally, module-library validation must reject a module whose declared default parameter values
violate one of its own constraints.

Required:

```text
valid defaults -> library GREEN
invalid same-module defaults -> library rejected
invalid dependent defaults -> library rejected when both definitions are available
diagnostic names constraint and actual default values
```

Do not reintroduce any Goal153 IDs into generic C#.

Keep generic participant resource-domain validation and mana capacity consistency.

## F. Activated-package diff quality gate

Update `docs/GOAL_DESIGN_QUALITY_POLICY.md` and add executable tests.

Every ordinary FeatureModule Goal that changes package mutations must produce:

```text
base package vs activated product package structured diff
```

Every changed definition/participant/property is classified:

```text
declared user-facing mechanic
declared user-facing starter content
authoring identity/metadata
forbidden qualification/proof fixture
```

GREEN requires:

```text
forbidden qualification/proof fixture count=0
every product mutation maps to a declared module capability or user-visible content claim
no unexplained global capacity/rule change
```

The pre-commit self-audit must fail if it says proof-only product data exists.

Do not implement this as a fragile search for one Goal153 ID only. Use typed operation classification,
module claims and structured package diff. A focused regression may additionally assert absence of
the known Goal153 fixture IDs.

## G. Evidence integrity

Do not rewrite Goal153B historical evidence.

Goal153C must create byte-identical compact files under both:

```text
.llmgc/procedural/goal-153c-product-proof-separation-and-outcome-aware-qualification-hotfix/
.llmgc/exports/goal-153c-product-proof-separation-and-outcome-aware-qualification-hotfix/
```

Required files, maximum 10 per root:

```text
goal153c-dashboard.json
activated-package-diff-proof.json
product-proof-separation-proof.json
outcome-aware-qualification-proof.json
conditional-action-replay-proof.json
default-constraint-proof.json
module-version-proof.json
cached-standalone-proof.json
artifact-scope-proof.json
goal153c-report.md
```

Dashboard schema must contain:

```text
status
implementationStatus
manualGateReady
accepted
acceptedByHuman
acceptedByCodex
manualReviewPerformed
activatedFixtureCount
healthDefinitionUnchanged
defaultExpiryPassed
highDamageLethalPassed
conditionalSkipReplayPassed
hostReused
hostRebuilt
unityProcessStartCount
artifactScopeViolationCount
```

Procedural/export copies must be byte-identical.

## H. Required regression matrix

### H1. Product package

```text
active module only:
  no dummy participant
  no health-capacity mutation
  ability added to player
  existing hostile selected

active + mana:
  mana cost/start/remaining correct
  health definition unchanged

active + status:
  default values produce ticks and expiry on actual hostile

all three:
  project package contains no proof fixture

all three + equipment/attributes/progression:
  additive behavior and package qualification GREEN

all-current-optionals:
  GREEN without proof fixture
```

### H2. Parameter/outcome matrix

```text
abilityDamage=2, duration=5, tickDamage=1:
  five ticks, expiry

abilityDamage=1000:
  real hostile defeated
  encounter terminal
  conditional EndTurns skipped truthfully
  build/replay GREEN

tickDamage=1000 with nonlethal direct hit:
  lethal status tick
  defeat/end
  no post-end turn advance

duration=1/2/5:
  full execution where content survives

duration=1000:
  deterministic plan proof
  product package unchanged
  no dummy target
```

Do not require the real product target to survive maximum damage.

### H3. Event and replay

Preserve Goal153A:

```text
expected participant binding
ability failure event atomicity
multi-status rollback event atomicity
canonical failed snapshot truth
lethal enemy victory
lethal player loss
duration-5 checkpoint/replay
```

Add:

```text
conditional skipped actions replay identically
skipped action produces no gameplay mutation event
terminal outcome observation stable across reload
```

### H4. Existing saved project

On a disposable copy of `goal148-manual`:

```text
open
select modules
use moderate values 2/12/3/5/1
save
close/reopen
build
repeat deterministic build
standalone payload
```

Required:

```text
source project byte-identical
activated package has no goal153_target
health max remains 30
real goblin receives ability/status lifecycle
five ticks and expiry
identity preserved
transactional activation GREEN
```

### H5. Standalone

Use existing cache only:

```text
HostReused=true
HostRebuilt=false
Unity process start count=0
hidden smoke all markers GREEN
payload game-package contains no proof fixture
human facts show actual target and terminal outcome
```

## I. Current-state routing

Until Goal153C GREEN:

```text
goal153ManualGateReady=false
goal153aManualGateReady=false
goal153bManualGateReady=false
nextAction=complete_goal153c_product_proof_separation_then_independent_audit
```

On GREEN:

```text
goal153ImplementationStatus=GREEN
goal153aImplementationStatus=GREEN
goal153bImplementationStatus=GREEN
goal153cImplementationStatus=GREEN

goal153ManualGateReady=true
goal153aManualGateReady=true
goal153bManualGateReady=true
goal153cManualGateReady=true

goal153Accepted=false
goal153aAccepted=false
goal153bAccepted=false
goal153cAccepted=false
acceptedByCodex=false
manualReviewPerformed=false

nextAction=independent_goal153c_audit_then_combined_goal153_family_human_gate
```

No human acceptance may be claimed.

## Combined human gate after independent GREEN audit

Use moderate values that exercise the actual lifecycle without a proof target:

```text
abilityBaseDamage=2
startingMana=12
abilityManaCost=3
statusDurationTurns=5
statusTickDamage=1
```

Human review should be short:

```text
1. Enable the three mechanics and set 2/12/3/5/1.
2. Build and verify one GREEN card.
3. Confirm human summaries: damage 2, mana 12→9, five ticks of 1, expired.
4. Save/reopen and confirm values remain.
5. Launch cached standalone and confirm the same readable facts.
```

No hash comparison and no raw module-ID inspection.

## Command and investigation budget

```text
read-first: maximum 11 primary files
architecture/product-diff implementation: maximum 15 minutes
focused tests: maximum 18 minutes
real-project/cache proof: maximum 6 minutes
total target wall clock: 45 minutes
maximum two testhost processes
Unity Editor process count: 0
```

Rules:

```text
no unchanged command repetition
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

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal153C"
dotnet test ... --filter "FullyQualifiedName~Goal153B"
dotnet test ... --filter "FullyQualifiedName~Goal153A"
dotnet test ... --filter "FullyQualifiedName~Goal153"
dotnet test ... --filter "FullyQualifiedName~CapabilityDrivenRuntimePlaythrough"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleLibrary"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleCertification"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"
dotnet test ... --filter "FullyQualifiedName~RuntimeEncounter"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Then one cached hidden standalone smoke with Unity process count asserted zero.

## Artifact scope

Initially allowed:

```text
AGENTS.md
docs/GOAL_DESIGN_QUALITY_POLICY.md
docs/CONTEXT_INDEX.md

.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal153c-product-proof-separation.ps1
.devflow/scripts/run-goal153c-product-proof-separation.cmd

catalogs/feature-modules/optional/combat-active-ability-loadout.featuremodule.json
catalogs/feature-modules/optional/magic-mana-spellcasting.featuremodule.json
catalogs/feature-modules/optional/status-turn-effects.featuremodule.json

src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughModels.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughValidator.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughPlanner.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughExpansionService.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterConstraintEvaluator.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleLibraryValidator.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleLibraryFingerprintService.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleRuntimeEffectModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleRuntimeEffectEvaluator.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs

src/LLMGameCreator.Runtime/CanonicalRuntimePlayerCommandLoopService.cs
src/LLMGameCreator.Runtime/SelectedRuntimeVariantInteractiveSessionService.cs

tests/LLMGameCreator.Tests/Application/Goal153C/Goal153CActivatedPackageSeparationTests.cs
tests/LLMGameCreator.Tests/Application/Goal153C/Goal153COutcomeAwareQualificationTests.cs
tests/LLMGameCreator.Tests/Application/Goal153C/Goal153CGoalQualityGateTests.cs
tests/LLMGameCreator.Tests/Application/Goal153A/Goal153AParameterizedLifecyclePlannerTests.cs
tests/LLMGameCreator.Tests/Application/Goal153B/Goal153BDeclarativeParameterConstraintTests.cs
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal153AbilityManaStatusWorkspaceTests.cs
tests/LLMGameCreator.Tests/Runtime/Goal153A/Goal153ATurnBindingEventAtomicityTests.cs

docs/manual-acceptance/active-abilities-mana-turn-status-featuremodules.md
docs/manual-acceptance/goal153a-parameter-domain-turn-binding-event-atomicity.md
docs/manual-acceptance/goal153b-declarative-parameter-constraints-domain-integrity.md
docs/manual-acceptance/goal153c-product-proof-separation.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

docs/agent-tasks/goal-153c-product-proof-separation-and-outcome-aware-qualification-hotfix/
.llmgc/procedural/goal-153c-product-proof-separation-and-outcome-aware-qualification-hotfix/
.llmgc/exports/goal-153c-product-proof-separation-and-outcome-aware-qualification-hotfix/
```

If exact compilation proves one additional existing Application/Runtime model or test path is required:

1. record the exact reason;
2. add only that exact path to artifact scope;
3. do not broaden to an entire source subtree.

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

## Publication

Create exactly one final commit:

```text
GREEN Goal 153C product proof separation and outcome-aware qualification hotfix
```

or honest BLOCKED/FAILED.

Codex must push it.

Required final state:

```text
HEAD == origin/main
worktree clean
Unity process start count=0
Goal153/153A/153B/153C accepted=false
manual gate ready only on GREEN
```

## GREEN criteria

```text
activated product package contains no qualification participant
global health definition remains unchanged
ability/status use deterministic real-content target selection
default/moderate lifecycle ticks and expires
high direct damage follows lethal terminal outcome
high status damage follows lethal terminal outcome
conditional actions skip truthfully and replay identically
proof-only high-capacity fixture exists only in test/certification memory
module versions advanced
invalid default constraints rejected
activated-package diff quality gate enforced
Goal153A/B regressions remain GREEN
old accepted mechanics/hashes preserved
real saved-project lifecycle GREEN
procedural/export evidence byte-identical and correct schemas
cached hidden smoke GREEN
Unity invocation count=0
artifact scope 0 violations
Goal153 family remains human-unaccepted
one final commit pushed
```

## Final report

Return GREEN, BLOCKED or FAILED and include:

- model/reasoning used;
- exact removed product fixture operations;
- activated package diff and health definition result;
- real target selector result;
- default expiry and lethal terminal scenarios;
- conditional skip/replay result;
- default-constraint validation result;
- module version changes;
- Goal quality gate result;
- Goal153A/B regressions;
- old-module/hash regressions;
- real-project lifecycle;
- evidence mirror/schema result;
- cache reuse and Unity process count;
- focused tests;
- artifact scope;
- Goal153/153A/153B/153C flags;
- short combined human gate;
- final commit/push/HEAD/worktree;
- confirmation no human acceptance claimed.
