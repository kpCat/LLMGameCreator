# Goal 153B — Declarative Parameter Constraints, Domain Integrity & Goal Quality Gate Hotfix

## Identity

- Task ID: `goal-153b-declarative-parameter-constraints-domain-integrity-and-goal-quality-gate-hotfix`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `4325fe4745c6a1e2363e4d49403d2360404ab69e`
- Required base message: `GREEN Goal 153A parameter-domain turn binding event atomicity and lethal-status hotfix`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Reason: Goal153A Runtime semantics are already implemented. This is a bounded Application/catalog
architecture hotfix: replace one module-specific relation branch with a reusable declarative
constraint contract, close mana resource-domain inconsistency and institutionalize a stronger Goal
quality gate. Sol is unnecessary unless an unknown Runtime defect is discovered.

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
Do not modify Unity host sources.

## Current state

Goal153 and Goal153A remain human-unaccepted.

Goal153A successfully fixed:

```text
configured duration expansion
expected-participant EndTurn binding
state/event atomicity
lethal status encounter resolution
duration 1/2/5 replay
duration 1000 plan-only determinism
cached standalone reuse with zero Unity process starts
```

Preserve all of those results.

Until Goal153B is independently audited:

```text
goal153ManualGateReady=false
goal153ImplementationStatus=BLOCKED_PENDING_GOAL153B
goal153aManualGateReady=false

goal153bAccepted=false
goal153bAcceptedByHuman=false
goal153bAcceptedByCodex=false
goal153bManualReviewPerformed=false
```

## Independent audit findings

### P1-A — module-specific parameter relation is hardcoded in generic production code

`FeatureModuleParameterBindingService.ValidateParameterRelations()` contains literal references to:

```text
feature.magic.mana_spellcasting
startingMana
abilityManaCost
```

This violates the central FeatureModule contract:

```text
adding an ordinary mechanic must not require editing generic Composer/authoring services
module compatibility and parameter rules must be declared by the module
```

The relation is also absent from the module definition/fingerprint, so the catalog is not a complete
description of its own valid parameter domain.

### P1-B — parameter domain exceeds the resource definition domain

The mana resource definition currently declares:

```text
maxValue=100
```

but `startingMana` permits values through `1000`.

The Runtime encounter state can therefore contain:

```text
Amount=1000
Capacity=100
```

without a causal package/authoring rejection.

### P2 — qualification-domain guard is hand-calculated rather than declaration-linked

The training target health `1001001` currently covers the declared maxima, but the proof hardcodes
the same numeric maxima. A future data-only parameter-range change could silently invalidate the
fixture.

The hotfix must make the invariant test derive maxima from actual module parameter definitions, not
duplicate magic constants.

## Primary objectives

1. Remove every Goal153 module/parameter literal from generic production services.
2. Add a reusable declarative numeric parameter-constraint contract to FeatureModules.
3. Move `abilityManaCost <= startingMana` into the mana module JSON.
4. Ensure parameter constraints participate in validation, fingerprints and certification
   invalidation.
5. Make mana resource capacity consistent with the entire declared authoring domain.
6. Reject encounter participant resource amounts outside resource min/max before Runtime execution.
7. Derive qualification-domain invariants from actual declarations.
8. Add a permanent repository Goal-design quality policy so future tasks systematically cover
   parameter domains, rollback truth, product/proof separation and architectural no-hardcoding.
9. Preserve Goal153A Runtime behavior and all accepted earlier mechanics.
10. Keep Unity Editor process count at zero.

## A. Declarative parameter-constraint contract

### A1. Model

Add a module-owned collection to `FeatureModuleDefinition`, for example:

```text
parameterConstraints[]
```

Recommended record:

```text
constraintId
kind=numeric_compare
leftExpression
operator
rightExpression
diagnosticCode
message
```

Equivalent naming is acceptable.

Supported operators:

```text
<
<=
==
!=
>=
>
```

Expressions use the existing safe numeric expression evaluator and may reference only:

```text
selected effective parameter values
```

No scripting, reflection, C# compilation, file/network/environment access or arbitrary methods.

### A2. Evaluation stage

Evaluate constraints:

```text
after canonical effective parameter values are resolved
before mutation operations/effective catalog/package activation
```

On failure:

```text
Passed=false
effective values may be returned for UI diagnostics
no mutation operation is applied
no package or saved valid composition is changed
diagnostic code and message are stable
diagnostic includes actual left/right values and referenced parameter IDs
```

### A3. Validation rules

Reject before activation:

```text
empty/duplicate constraint ID
unsupported constraint kind
unsupported operator
unknown parameter reference
reference to an unselected module
nonnumeric parameter/expression
invalid arithmetic
division by zero
duplicate semantic constraint target where applicable
```

A constraint may reference parameters in another selected module only when the declaring module has
a dependency on that module. Otherwise reject the catalog/selection.

### A4. Generic architecture

Forbidden in generic production C#:

```text
feature.magic.mana_spellcasting
startingMana
abilityManaCost
feature.combat.active_ability_loadout
feature.status.turn_effects
ability/arcane_impulse
status/arcane_burn
goal153_target
```

These identifiers are allowed in:

```text
catalog JSON
focused tests
Goal153B docs/evidence
```

Add a static architecture test scanning changed/current generic production files.

### A5. Fingerprint and certification

The complete normalized constraint definition must participate in module fingerprints.

Required incremental behavior:

```text
unchanged second certification reuses modules
changing only mana constraint invalidates mana module and dependents
unrelated optional modules remain reusable
constraint diagnostic/message-only change follows the repository's established semantic-fingerprint policy
```

Do not suppress genuine selected-module staleness.

## B. Migrate the mana relation to data

In:

```text
catalogs/feature-modules/optional/magic-mana-spellcasting.featuremodule.json
```

declare generically:

```text
abilityManaCost <= startingMana
```

Remove `ValidateParameterRelations()` and all Goal153 identifiers from
`FeatureModuleParameterBindingService`.

Required failure example:

```text
startingMana=2
abilityManaCost=3
stage=parameter_binding
package unchanged
diagnostic identifies both values
```

Required success boundaries:

```text
startingMana=0 cannot pair with positive cost
startingMana=1, cost=1
startingMana=12, cost=3
startingMana=1000, cost=1000
```

Do not clamp or rewrite values.

## C. Mana resource-domain integrity

Update the data-defined mana resource so its capacity covers the declared `startingMana` range.

At minimum:

```text
resource/mana.maxValue >= startingMana.maximum
resource/mana.defaultValue is within min/max
encounter player mana amount is within min/max for every accepted parameter value
```

Use data and generic validation, not module-ID branches.

Add generic pre-Runtime package/playthrough validation:

```text
for every encounter participant resource:
  referenced ResourceDefinition exists exactly once
  amount is finite
  amount >= minValue when present
  amount <= maxValue when present
```

Failure is causal and occurs before state mutation.

Required tests:

```text
startingMana=1000 -> amount=1000, capacity>=1000, build GREEN
amount above max -> rejected before Runtime
amount below min -> rejected before Runtime
missing resource definition -> rejected
unrelated existing resource behavior preserved
```

## D. Declaration-linked qualification-domain proof

Do not hardcode `1000 + 1000 * 1000` in the proof.

The test must read:

```text
abilityBaseDamage.maximum
statusTickDamage.maximum
statusDurationTurns.maximum
training-target health from the composed package
```

Then assert:

```text
targetHealth >
abilityMaximum + tickDamageMaximum * durationMaximum
```

Also verify:

```text
all values finite
overflow-safe arithmetic
changing a copied parameter maximum above fixture capacity makes the certification test fail
```

This does not authorize new hardcoded limits in Runtime.

Record the current training-target content as an explicit first-vertical-slice limitation; it is data,
not a C# branch. Do not expand this hotfix into a new encounter/content system.

## E. Permanent Goal design quality policy

Create:

```text
docs/GOAL_DESIGN_QUALITY_POLICY.md
```

Link it from:

```text
AGENTS.md
docs/CONTEXT_INDEX.md
```

Every future Codex GOAL.md must include the following mandatory design review before implementation.

### E1. Product claim and non-goals

```text
exact user-visible result
exact source of gameplay truth
what is fixture/proof-only
what must never enter activated product data
```

### E2. Parameter-domain table

For every typed parameter:

```text
minimum
default
maximum
invalid values
cross-parameter relations
package fields affected
Runtime effects affected
playthrough shape affected
save/replay fields affected
```

Tests must cover:

```text
minimum
default
maximum
one interior non-default
invalid below/above
every cross-parameter boundary
```

A plan-only test may replace an extreme full replay only when the GOAL states why and separately
proves bounded deterministic planning.

### E3. State/event/rollback matrix

For every new command/effect:

```text
success state mutation
success events
failure state
failure events
transaction boundary
checkpoint behavior
replay behavior
```

No success event may represent rolled-back state.

### E4. Product-versus-proof separation

The task must explicitly answer:

```text
Does any test target, dummy entity, special health value, fixture encounter or proof file enter the
activated user package?
```

If yes, the GOAL must justify it as actual product content; otherwise it must remain isolated.

### E5. Generic architecture scan

Every normal FeatureModule Goal must fail when generic production services contain literal:

```text
module IDs
parameter IDs
fixture ability/status/item/entity IDs
composition IDs
Goal IDs
```

unless the identifier belongs to a documented stable protocol vocabulary.

### E6. Composition coverage

At minimum:

```text
module independently where dependencies permit
each direct dependency combination
one interaction with previously accepted mechanics
all-current-optional golden composition when practical
default-off unchanged path
```

No powerset requirement.

### E7. Real saved-project lifecycle

For user-facing authoring changes:

```text
open existing project
save
close/reopen
build
repeat deterministic build
failed build rollback
project identity preservation
```

### E8. Operational budgets

Every task states:

```text
recommended model/reasoning
read budget
command budget
Unity invocation budget
long-command retry policy
publication policy
```

### E9. Pre-commit architectural self-audit

Before commit Codex must answer in machine-readable evidence:

```text
Which valid parameter combinations were not executed?
Which product data exists only to satisfy a proof?
Which new literals appear in generic production code?
Can any failure leak events for rolled-back state?
Can a maximum valid value create invalid Runtime state?
Can an old saved project behave differently?
```

Any unanswered item blocks GREEN.

## F. Regression matrix

Required focused tests:

```text
synthetic module with reusable same-module numeric constraint
synthetic dependent module with allowed cross-module constraint
cross-module constraint without dependency rejected
unknown/unselected parameter reference rejected
all six comparison operators
constraint fingerprint/invalidation
mana min/default/max/boundary matrix
mana amount/capacity consistency
participant resource domain validation
static no-Goal153-ID scan in generic production C#
Goal153 duration 1/2/5 and 1000 plan-only regressions
Goal153A event atomicity and lethal-status regressions
all three modules with equipment/attributes/progression
all-current-optional catalog
disabled new modules preserve historical hashes
goal148-manual disposable save/reopen/build/repeat
```

## G. Current-state routing

Until Goal153B is GREEN:

```text
goal153ManualGateReady=false
goal153aManualGateReady=false
nextAction=complete_goal153b_declarative_constraint_hotfix_then_independent_audit
```

On GREEN:

```text
goal153ImplementationStatus=GREEN
goal153aImplementationStatus=GREEN
goal153bImplementationStatus=GREEN
goal153ManualGateReady=true
goal153aManualGateReady=true
goal153bManualGateReady=true

goal153Accepted=false
goal153aAccepted=false
goal153bAccepted=false
acceptedByCodex=false
manualReviewPerformed=false

nextAction=independent_goal153b_audit_then_combined_goal153_family_human_gate
```

Do not claim human acceptance.

## Command and investigation budget

```text
read-first: maximum 9 primary files
architecture/model implementation: maximum 12 minutes
focused tests: maximum 15 minutes
real-project/cache proof: maximum 5 minutes
total target wall clock: 35 minutes
maximum two testhost processes
Unity Editor process count: 0
```

Rules:

```text
no unchanged command repetition
no full suite
no 85-case closure
no all-ProductSmoke
no historical snapshot repair
no Unity host build
raw logs remain ignored
```

## Required validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal153B"
dotnet test ... --filter "FullyQualifiedName~Goal153A"
dotnet test ... --filter "FullyQualifiedName~Goal153"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleLibrary"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleCertification"
dotnet test ... --filter "FullyQualifiedName~CapabilityDrivenRuntimePlaythrough"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"

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
.devflow/scripts/run-goal153b-declarative-constraints-domain-integrity.ps1
.devflow/scripts/run-goal153b-declarative-constraints-domain-integrity.cmd

catalogs/feature-modules/optional/magic-mana-spellcasting.featuremodule.json

src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterModels.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterBindingService.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterConstraintEvaluator.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleLibraryValidator.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleLibraryFingerprintService.cs
src/LLMGameCreator.Application/Design/CapabilityDrivenRuntimePlaythrough/CapabilityDrivenRuntimePlaythroughValidator.cs

tests/LLMGameCreator.Tests/Application/Goal153B/Goal153BDeclarativeParameterConstraintTests.cs
tests/LLMGameCreator.Tests/Application/Goal153B/Goal153BManaDomainIntegrityTests.cs
tests/LLMGameCreator.Tests/Application/Goal153B/Goal153BArchitecturePolicyTests.cs
tests/LLMGameCreator.Tests/Application/FeatureModuleAuthoring/FeatureModuleLibraryAndParameterTests.cs
tests/LLMGameCreator.Tests/Application/FeatureModuleCertification/FeatureModuleCertificationAndCoverageTests.cs
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal153AbilityManaStatusWorkspaceTests.cs

docs/manual-acceptance/active-abilities-mana-turn-status-featuremodules.md
docs/manual-acceptance/goal153a-parameter-domain-turn-binding-event-atomicity.md
docs/manual-acceptance/goal153b-declarative-parameter-constraints-domain-integrity.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

docs/agent-tasks/goal-153b-declarative-parameter-constraints-domain-integrity-and-goal-quality-gate-hotfix/
.llmgc/procedural/goal-153b-declarative-parameter-constraints-domain-integrity-and-goal-quality-gate-hotfix/
.llmgc/exports/goal-153b-declarative-parameter-constraints-domain-integrity-and-goal-quality-gate-hotfix/
```

If an exact compile/test failure proves another existing Application test/model path is required, add
only that exact path with a recorded reason.

Forbidden:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
unity/**
samples/**
generator-library/**
provider/**
LLM/**
RAG/**
public GamePackage schema changes
```

## Compact evidence

Maximum 9 files per root:

```text
goal153b-dashboard.json
declarative-constraint-contract-proof.json
mana-domain-integrity-proof.json
qualification-domain-proof.json
architecture-no-hardcoding-proof.json
certification-invalidation-proof.json
goal-quality-policy-proof.json
cached-standalone-proof.json
goal153b-report.md
```

Do not commit raw logs/TRX, copied projects or user data.

## Publication

Create exactly one final commit:

```text
GREEN Goal 153B declarative parameter constraints domain integrity and Goal quality gate hotfix
```

or honest BLOCKED/FAILED.

Codex must push it.

Required final state:

```text
HEAD == origin/main
worktree clean
Unity process start count=0
Goal153/153A/153B accepted=false
manual gate ready only on GREEN
```

## GREEN criteria

```text
no Goal153 module/parameter literals in generic production services
mana relation declared in module data
generic constraint evaluator covers operators and negative cases
constraints fingerprinted and invalidate certification correctly
startingMana full domain fits resource capacity
invalid participant resource domains rejected before Runtime
qualification target invariant derived from declarations
Goal design quality policy linked from AGENTS.md
Goal153A duration/turn/event/lethal regressions remain GREEN
old accepted mechanics/hashes preserved
real-project lifecycle GREEN
cached hidden smoke GREEN
Unity invocation count=0
artifact scope 0 violations
Goal153 family remains human-unaccepted
one final commit pushed
```

## Final report

Return GREEN, BLOCKED or FAILED and include:

- model/reasoning used;
- hardcoded relation removal;
- declarative constraint shape and operators;
- mana min/default/max/boundary results;
- resource amount/capacity results;
- fingerprint/invalidation result;
- static architecture scan;
- declaration-linked qualification invariant;
- Goal153A regression results;
- real-project lifecycle;
- cache reuse and Unity process count;
- Goal quality policy proof;
- focused tests;
- artifact scope;
- Goal153/153A/153B flags;
- short combined human gate;
- final commit/push/HEAD/worktree;
- confirmation no human acceptance claimed.
