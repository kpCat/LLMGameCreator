# Goal 156 — Seeded Generated Project Creation, Modern Workspace & Cached Standalone

## Identity

- Task ID: `goal-156-seeded-generated-project-creation-modern-workspace-and-cached-standalone`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `ebaa4abac2273b185e6da0a3fb15e22fa2be3996`
- Required base message: `GREEN Goal 155A current-package-correlated release candidate record truth hotfix`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Sol
Reasoning effort: Extra High
```

Reason: this is the next major generated-product vertical slice. It connects the completed seeded
procedural kernel, formula/effect/action registry, tiny generated Runtime loop and Generated Package
MVP to the modern project creation, FeatureModule authoring, canonical Runtime/replay, WinForms and
cached standalone workflows. It must solve the architecture as one coherent slice rather than
creating a parallel demo workflow.

## Pre-approval

- The owner approved execution by launching this task.
- Produce a concise internal implementation plan; do not ask for confirmation.
- Do not request intermediate or final manual testing.
- Own all P0/P1 defects reproduced by the mandatory Goal156 matrix inside this single Goal.
- Record P2/P3 debt without spawning Goal156A.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No candidate/intermediate commits.
- Codex performs commit and standard push itself.

## Expected initial worktree

After unpacking this ZIP, the only permitted untracked files are:

```text
docs/agent-tasks/goal-156-seeded-generated-project-creation-modern-workspace-and-cached-standalone/GOAL.md
docs/agent-tasks/goal-156-seeded-generated-project-creation-modern-workspace-and-cached-standalone/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-156-seeded-generated-project-creation-modern-workspace-and-cached-standalone/README.md
```

Required:

```text
HEAD == origin/main == ebaa4abac2273b185e6da0a3fb15e22fa2be3996
branch=main
tracked diff count=0
staged diff count=0
unknown dirty/untracked count=0
```

The three task files are authorized and must be committed.
Any other dirty path blocks execution. Never use reset, stash, merge, rebase or destructive cleanup.

## Unity and standalone budget

```text
Unity Editor invocation budget: 0
Unity host build budget: 0
real hidden standalone smoke budget: exactly 1
visible automated standalone launch budget: 0
```

Use the existing generic host cache. If it is incomplete or would rebuild, publish BLOCKED without
starting Unity.

## Independent-audit intake

Goal155A closes the Goal155 P1.

Record:

```text
goal155IndependentAuditResult=BLOCKED_AT_7084244a
goal155IndependentAuditBlocker=rc_record_not_correlated_with_current_package_and_document
goal155AuditBlocker=closed_by_goal155a

goal155aIndependentAuditResult=GREEN_ACCEPTABLE_CANDIDATE_AT_EBAA4ABA
goal155aIndependentAuditPassed=true
goal155IndependentAuditRequired=false
goal155aIndependentAuditRequired=false
goal155MilestoneRcPassed=true
```

Goal155 and Goal155A remain `accepted=false`; no human gate is required. Preserve all Goal154 human
acceptance exactly.

### Goal155A audited truth

```text
exact record/package/document/identity/fingerprint -> CURRENT
missing/tampered package -> rejected/ABSENT
older valid build or authoring/metadata difference -> LAST_SUCCESS
missing current truth -> UNKNOWN
portable and build-history-independent valid record -> CURRENT
```

A nonblocking P2 remains: the top `gate_status/current_user_action` prose in the prior state file was
not fully synchronized with the lower `nextAction`. Correct it as part of Goal156 state publication;
do not create a docs-only hotfix.

## Product problem

The repository already contains completed product slices for:

```text
seeded procedural game kernel
formula/effect/action registry
tiny generated Runtime loop
Generated Package MVP
visible generated preview
one-click generated preview workflow
generation presets/options
modern project creation
FeatureModule authoring/composition
canonical Runtime/checkpoint/replay
accepted mechanics RC
cached Windows standalone
```

But these remain disconnected.

### Old generated lane

`OneClickGeneratedPreviewWorkflowService` writes generated sidecars and a Generated Package MVP under
a supplied folder. It is a preview/proving workflow, not a normal durable game project.

### Modern project lane

`GameProjectService.CreateAsync()` creates only a fixed template package.

`FeatureModuleParameterizedCompositionService` always resolves the immutable Goal142 balanced
baseline. Therefore a project package created by the old generated lane would be discarded during
the next modern project build.

### User-visible gap

The user cannot currently do:

```text
Игры
→ Новая игра
→ choose seed/mode/preset
→ create a real generated project
→ inspect generated world facts
→ enable/use modern mechanics
→ save/reopen
→ build through canonical Runtime/replay
→ build cached Windows standalone
```

Goal156 closes this exact gap.

## User-visible workflow

In the normal `Игры` page:

```text
Новая игра
→ Тип проекта: Сгенерированная игра
→ Folder/title/package/version
→ Seed
→ Generation mode
→ Preset
→ Mechanics profile
→ Создать
```

Initial supported mechanics profiles:

```text
all_selectable_defaults
core_only
```

Profile definitions are data-derived from the current FeatureModule library:

```text
all_selectable_defaults:
  every current selectable non-required module selected
  no explicit parameter values; effective defaults are used

core_only:
  no optional module selected
  no explicit parameter values
```

Do not hardcode selected module counts in production code.

Default creation type:

```text
seeded_generated
```

Legacy blank/template creation remains available and byte-compatible.

After successful creation:

```text
project appears in the normal game list
project opens immediately
generated-world card is visible
mechanics/parameters tabs work normally
build/repeat/reopen works
cached Windows standalone works
```

## Non-goals

Do not add:

```text
new Runtime primitives
new FeatureModules
new parameter types
public GamePackage schema changes
Unity scripts/scenes/prefabs/settings/packages changes
new Unity host build
provider/LLM/Lua/media execution
large/infinite runtime streaming
seed editing/regeneration after project creation
clean-machine installer
final release packaging
```

Seed regeneration is a later product slice. Goal156 creates and persists a generated project once,
then proves it survives normal authoring/build/standalone workflows.

## Mandatory architecture review

Read at most 16 primary files:

```text
AGENTS.md
docs/GOAL_DESIGN_QUALITY_POLICY.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/NARROW_ALPHA_EXPANSION_POLICY.md
docs/UNITY_EXECUTION_POLICY.md
ProceduralGameKernelService.cs
FormulaEffectActionRegistryService.cs
TinyGeneratedRuntimeLoopService.cs
GeneratedPackageMvpService.cs
OneClickGeneratedPreviewWorkflowService.cs
GameProjectService.cs
FeatureModuleParameterizedCompositionService.cs
FeatureModuleCompositionService.cs
GameProjectBuildAndQualificationService.cs
CreateGameDialog.cs
```

Before production edits create:

```text
.llmgc/procedural/goal-156-seeded-generated-project-creation-modern-workspace-and-cached-standalone/architecture-review.json
```

Required resolved sections:

```text
existingGenerationAssets
modernProjectBaselineConstraint
generatedOverlayContract
baselineAnchorPreservation
customBaseCompositionContract
projectCreationTransaction
generationSourceIntegrity
buildAndReplayFlow
standaloneCorrelation
failureRollbackMatrix
legacyProjectCompatibility
nonGoals
```

Each section must contain concrete decisions, affected types and behavioral tests. Empty or vague
sections block GREEN.


## A. Project creation contract

Extend `CreateGameProjectRequest` additively:

```text
CreationKind:
  template
  seeded_generated

GenerationSeed
GenerationMode
GenerationPresetId
MechanicsProfileId
CompactStyleHintIds[]
SelectedVariantIds[]
```

Existing callers that do not set new fields retain the old template behavior.

Add vocabularies:

```text
GameProjectCreationKinds.Template
GameProjectCreationKinds.SeededGenerated

GeneratedProjectMechanicsProfiles.AllSelectableDefaults
GeneratedProjectMechanicsProfiles.CoreOnly
```

### A1. Validation

For `seeded_generated` require:

```text
nonempty normalized seed
supported ProceduralGameGenerationMode
known preset resolved by GenerationPresetOptionsService
known mechanics profile
safe folder/package identity
no target folder
```

Do not silently replace an invalid user-provided mode/profile. Return causal validation errors.

The existing procedural kernel may normalize style/variant hints, but project-creation identity inputs
must be explicit and valid.

### A2. Atomic project creation

Create in a sibling temporary directory under the games root:

```text
.<folder>.creating-<random>
```

The random temporary suffix is operational only and never appears in artifacts/hashes.

Inside the temporary project:

```text
run generation
materialize generated base
write sidecars
write package.json
write project identity
initialize authoring
validate project
```

Only after all checks pass:

```text
atomic directory move temp -> final target
```

On any failure:

```text
final target absent
temporary directory removed
no current package replacement
no partial game-list entry
```

Do not overwrite an existing folder.

### A3. Legacy template lane

Existing `template` creation must remain byte/semantic compatible with required base:

```text
same NewGamePackageFactory output
same folders
same validation
no generation sidecars
no generated-source marker
```

## B. Generated project source record

Create a project-local internal record:

```text
.llmgc/generation/seeded-project-source.json
```

Schema:

```text
seeded_generated_project_source_v1
creationKind
seed
mode
presetId
styleHintIds[]
variantIds[]
mechanicsProfileId

planId
planSha256
rulePackId
rulePackSha256
tinyLoopStateSha256
generatedMvpPackageSha256
generatedOverlaySha256
generatedBasePackageSha256
goal142BaselinePackageSha256

generatedStartMapId
counts:
  regions
  factions
  actors
  itemsAndResources
  encounters
  questEvents

tinyLoop:
  passed
  initialStateHash
  finalStateHash
  stepCount
  rewardOrCostObserved
  stateChangeObserved
```

No timestamps, absolute paths, machine names or temporary paths.

### B1. Sidecar layout

Write deterministically:

```text
.llmgc/generation/generated-game-plan.json
.llmgc/generation/generated-game-plan.md
.llmgc/generation/formula-effect-action-rule-pack.json
.llmgc/generation/tiny-runtime-loop-state.json
.llmgc/generation/tiny-runtime-loop-report.md
.llmgc/generation/generated-package-mvp.json
.llmgc/generation/generated-project-overlay.json
.llmgc/generation/generated-base-package.json
.llmgc/generation/seeded-project-source.json
```

Use the existing services' deterministic serialized content, copied/re-rendered into this project-local
contract where needed.

All hashes in the source record are SHA-256 over actual UTF-8/file bytes with explicitly documented
meaning.

### B2. Integrity reader

Create a confined reader/service:

```text
SeededGeneratedProjectSourceService
```

It validates:

```text
exact schema
supported mode/preset/profile
all required sidecars exist
each sidecar hash matches source record
generated base package hash matches file
generated MVP/overlay references are valid
counts match actual plan/overlay
tiny loop passed and hash chain matches sidecar
```

Malformed/missing/hash-mismatched source is a causal build failure:

```text
generated_source.invalid_json
generated_source.unsupported_schema
generated_source.sidecar_missing
generated_source.sidecar_hash_mismatch
generated_source.count_mismatch
generated_source.tiny_loop_failed
```

Never silently fall back to the Goal142 baseline for a project marked `seeded_generated`.

Legacy projects without this record use the existing baseline lane unchanged.

## C. Generated additive overlay

Create:

```text
GeneratedProjectOverlayService
```

Inputs:

```text
immutable Goal142 balanced baseline package
GeneratedPackageMvpResult.Package
ProceduralGeneratedGamePlan
FormulaEffectActionRulePack
TinyGeneratedRuntimeLoopResult
project identity metadata only when creating final package
```

Output:

```text
GeneratedProjectOverlayDocument
GeneratedBasePackage
deterministic JSON
hashes
diagnostics
```

### C1. Additive-only merge

The generated base is:

```text
Goal142 balanced baseline
+ namespaced generated records
+ generated provenance
```

It must not modify/remove any baseline definition except:

```text
GeneratedContent metadata/provenance may be additively extended
Manifest description/source context may mention seeded generation
```

Before identity overlay, preserve baseline manifest package ID/title/version/start map.

The baseline start map remains the start map in Goal156. This preserves the accepted mechanics
qualification route. The generated start map is recorded and present in the package for future
generated-world activation work.

### C2. Record families

Merge generated MVP records for all supported existing GamePackage collections:

```text
tile prototypes
entity prototypes
maps
factions
actors/entities represented by map entities/prototypes
items
resources
abilities
encounters
quests
dialogues
interactions
formulas
generated-content profile/provenance
```

Do not invent a new public GamePackage field.

### C3. Collision rules

For each collection:

```text
generated ID absent in baseline -> add
generated ID present and canonical definitions byte/semantic equal -> retain one and record dedup
generated ID present and definitions differ -> fail
```

Failure:

```text
generated_overlay.id_collision:<kind>:<id>
```

No last-write-wins.

### C4. Baseline preservation proof

Produce a structured path/record inventory:

```text
every baseline ID exists after overlay
every baseline definition canonical JSON equals before
baseline manifest identity/start map unchanged before project identity overlay
generated records are additive
```

No fixed baseline record counts in production.

### C5. Reference validation

Validate all generated references after merge:

```text
map tile/entity prototypes
faction/home/actor links represented by generated provenance
encounter actors/rewards/abilities
quest items/encounters/rewards
dialogue quest/item/faction requirements/effects
interaction targets
formula/rule references supported by existing package/runtime validators
```

Use existing `GamePackageValidator` plus explicit overlay referential checks where the validator does
not cover generated provenance.

## D. Custom composition base

Introduce an internal typed descriptor:

```text
FeatureModuleCompositionBasePackage
  PackagePath
  PackageSha256
  SourceKind
  SourceIdentity
```

Source kinds:

```text
goal142_balanced_baseline
seeded_generated_base
```

### D1. FeatureModuleParameterizedCompositionService

Add an overload/optional request:

```text
MaterializeAndQualify(..., FeatureModuleCompositionBasePackage? basePackage)
```

Default null keeps the exact existing Goal142 baseline behavior.

When supplied:

```text
validate path is confined under project/staging or repository-approved source
hash actual bytes and require exact match
plan against this base path/hash
compose and qualify against the same base bytes
```

### D2. FeatureModuleCompositionService

Add a corresponding explicit-base overload. Remove no old overload.

All of these must use the explicit base consistently:

```text
forward plan
reverse-order plan
metadata mutations
standard mutations
materialization
package hash
order-independence proof
qualification
mutation audit
```

Do not resolve Goal142 internally after an explicit base has been provided.

### D3. Distinctness semantics

For a custom generated base:

```text
generated base with zero optional modules:
  valid even though it is not compared as a Goal142 candidate

selected modules:
  materialized package must differ from the generated base when selected mutations are nonempty
```

Do not require every custom-base package to be distinct from every Goal142 matrix candidate as a
generic validity condition.

Preserve existing historical Goal146/147 behavior and hashes for the default baseline lane.

### D4. Mutation compatibility

Every existing selected FeatureModule operation must still apply to preserved baseline anchors in the
generated base.

For `all_selectable_defaults` require:

```text
all selected modules satisfied
mutation audit GREEN
order independence GREEN
checkpoint/full replay/action binding GREEN
accepted mechanics summary Passed=true
```

No module JSON/version changes are expected. A necessary FeatureModule JSON change is a BLOCKED result
with migration design; do not stale existing projects in Goal156.


## E. GameProjectService integration

Keep `IGameProjectService.CreateAsync()` as the normal entrypoint.

Inject or create a focused collaborator:

```text
SeededGeneratedGameProjectCreationService
```

Template requests continue through `NewGamePackageFactory`.

Seeded requests use the atomic generated-project path.

### E1. Project identity

Final `package.json` must use requested:

```text
PackageId
Title
Version
FormatVersion from the current supported template
```

Identity overlay must not change generated/base gameplay semantics.

Create the normal project identity/authoring files through existing services, not duplicated JSON
writers where an existing typed service exists.

### E2. Authoring initialization

After generated `package.json` exists:

```text
open GameProjectFeatureModuleAuthoringService on the temporary project
apply mechanics profile
save
```

`all_selectable_defaults`:

```text
SelectedModuleIds = every current selectable non-required module, sorted
ParameterValues = empty
ModuleFingerprints = current fingerprints for selected modules
CatalogFingerprint = current catalog fingerprint
```

`core_only`:

```text
SelectedModuleIds = empty
ParameterValues = empty
```

The generated source/overlay hashes are not part of the semantic FeatureModule authoring fingerprint.
They have their own source record.

### E3. Creation result

Extend `GameProjectSummary` or return a typed creation result additively:

```text
CreationKind
GenerationSeed
GenerationMode
GenerationPresetId
MechanicsProfileId
GeneratedSourcePresent
GeneratedSourceStatus
GeneratedCounts
```

Legacy list/open behavior remains compatible for old projects.

## F. Modern build integration

In `GameProjectBuildAndQualificationService`:

1. inspect project for generated source record;
2. if absent, use existing Goal142 baseline lane unchanged;
3. if present, validate source and generated-base package;
4. copy the generated base into build staging;
5. pass an explicit `FeatureModuleCompositionBasePackage`;
6. perform current parameter binding/module composition;
7. overlay project identity as today;
8. qualify canonical Runtime/checkpoint/replay;
9. validate staged and activated package;
10. preserve/activate generated content and source support files transactionally.

### F1. No generation loss

For every successful generated-project build require:

```text
all generated overlay IDs still exist in composition package
all generated overlay IDs still exist in activated package
generated overlay canonical records unchanged by unrelated FeatureModules
generated base hash/source hashes remain unchanged
```

Selected modules may modify only declared baseline anchors or generated records explicitly targeted by
a future module. Goal156 modules target no generated IDs.

### F2. Transactional failure

On:

```text
source corruption
overlay collision
module mutation failure
qualification failure
staged validation failure
activation failure
```

require:

```text
current package byte-identical
authoring document restored
generated sidecars byte-identical
last successful build/history/generated summary preserved
no partial staging
```

### F3. Generated Runtime summary

Create a typed:

```text
GameProjectGeneratedWorldSummary
```

Fields:

```text
Present
Passed
Seed
Mode
PresetId
MechanicsProfileId
PlanSha256
OverlaySha256
GeneratedBasePackageSha256
RegionCount
FactionCount
ActorCount
ItemResourceCount
EncounterCount
QuestEventCount
GeneratedStartMapTitle
TinyLoopPassed
TinyLoopStepCount
TinyLoopInitialStateHash
TinyLoopFinalStateHash
RewardOrCostObserved
StateChangeObserved
PackageContentPreserved
HumanFacts[]
Diagnostics[]
```

No absolute paths.

Human facts:

```text
Seed
Режим генерации
Профиль
Регионы
Фракции
Персонажи
Предметы и ресурсы
Столкновения
Задания и события
Сгенерированный цикл
```

Example:

```text
Сгенерированный цикл: пройден; награда/затрата и изменение состояния подтверждены
```

Values are data-derived.

### F4. Persistence

Add generated summary to:

```text
GameProjectBuildResult
GameProjectBuildHistoryEntry
UnifiedGameProjectWorkspaceSnapshot
```

Restore from the same hash-validated GREEN history entry.

A generated summary is CURRENT only when:

```text
current source record hashes valid
current generated-base file hash valid
last successful build package/composition/final identity matches current document
qualified authoring fingerprint matches current authoring
```

Saved mechanic changes without build show generated summary as LAST_SUCCESS only for build-derived
facts. The source card itself remains CURRENT if generation sidecars are unchanged.

Older history without the field remains readable.

## G. WinForms creation and generated-world card

### G1. CreateGameDialog

Add Russian controls:

```text
Тип проекта
  Сгенерированная игра
  Пустой шаблон

Seed
Режим
Пресет
Профиль механик
```

Supported generation modes come from `ProceduralGameGenerationModes.Supported`, sorted and presented
with human labels.

Preset choices come from `GenerationPresetOptionsService` or its typed catalog, not duplicated
literals in UI.

Mechanics profiles:

```text
Все доступные механики
Только обязательные
```

Default seed:

```text
normalized folder name
```

It updates only while the user has not edited the seed manually.

Validation messages are Russian and causal.

No random/time-based default seed.

### G2. Create flow

`ProjectsPageControl.CreateNewGameAsync()` uses the extended request.

After success:

```text
refresh list
open generated project
show generated-world card
```

Do not invoke build or standalone automatically during interactive creation.

### G3. Generated-world card

Add one compact card:

```text
Сгенерированный мир
```

Rows:

```text
Seed
Режим
Пресет
Регионы
Фракции
Персонажи
Предметы и ресурсы
Столкновения
Задания и события
Сгенерированный цикл
Статус сборки
```

States:

```text
SOURCE_READY:
  generated source valid, project not yet built

BUILD_CURRENT:
  generated source + latest build current

LAST_SUCCESS:
  source current, latest generated build no longer matches mechanic authoring

INVALID:
  source diagnostics; no claim of generated readiness
```

No raw hashes/IDs/absolute paths in the card. Technical Details may include hashes and sidecar-relative
paths.

Layout:

```text
readable at 1100x720
word wrap
no clipping
does not duplicate RC/social card rows
```

## H. Standalone integration

For a generated project:

```text
BuildWindowsStandalone
```

must use the modern generated-base build lane.

Append generated-world HumanReviewFacts through a single typed formatter used by:

```text
WinForms generated-world card
standalone request
payload correlation tests
```

Actual payload must include:

```text
Seed
Режим генерации
Регионы
Фракции
Столкновения
Задания и события
Сгенерированный цикл
```

For `all_selectable_defaults`, also include accepted-mechanics facts and:

```text
Release Candidate=готов
```

RC record writes normally and is CURRENT.

No Unity host code change.

## I. Deterministic creation matrix

### I1. Same generation input

Create two projects with different project identity/folder but identical:

```text
seed
mode
preset
mechanics profile
style hints
variant IDs
```

Require byte-identical:

```text
generated plan
rule pack
tiny loop state/report
generated MVP package
generated overlay
generated base package before identity
source record except project-neutral identity-free fields
```

Final project `package.json` differs only where project identity/provenance explicitly requires it.

### I2. Different seed

Different seed with same options/profile:

```text
generated plan hash differs
overlay hash differs
generated base hash differs
at least one region/faction/encounter/quest-event selection differs
structure/reference validation remains GREEN
baseline definitions remain identical
```

Do not assert a hardcoded number of records beyond nonempty/minimum structural requirements.

### I3. Mode/preset matrix

At minimum test every currently supported generation mode with at least one compatible preset.

Use catalog-derived modes/presets; do not hardcode a fixed total.

Require each project:

```text
source valid
overlay valid
package valid
tiny loop passed
```

One Profile `all_selectable_defaults` receives full build/replay/standalone proof. Other matrix rows
may stop after creation/package/source validation.

## J. Real project proof

Use a new disposable games root under short LocalAppData, not the user's real `goal148-manual`.

Create through the actual `GameProjectService.CreateAsync()` path.

### J1. All-selectable generated project

Request:

```text
creationKind=seeded_generated
seed=goal156-accepted-world
mode=semi_procedural_regions
a real supported preset
mechanicsProfile=all_selectable_defaults
```

Require:

```text
project folder created atomically
project list discovers it
package identity correct
generated source valid
selected optional modules equal current selectable catalog set
explicit parameter count=0
effective default parameters visible
build GREEN
repeat build deterministic
fresh reopen generated summary BUILD_CURRENT
AcceptedMechanics Passed=true
generated overlay preserved
source sidecars unchanged
```

### J2. Core-only generated project

Require:

```text
creation/build GREEN
generated summary Passed=true
AcceptedMechanics Passed=false with MissingFactKinds
no RC READY before standalone
generated content preserved
```

### J3. Legacy template project

Create through old lane and require:

```text
no generated source record
package output same as required-base behavior
normal workspace build unchanged
```

### J4. Failure atomicity

Test:

```text
unsafe folder
unsupported mode
unknown preset
unknown mechanics profile
target exists
injected overlay ID collision
injected sidecar write failure
invalid generated reference
```

Require final target absent and no temp directory leak.

## K. Cached standalone proof

On the generated all-selectable project after repeat/fresh reopen:

```text
verify complete existing host cache
hash host file set
assert zero Unity processes
BuildWindowsStandalone exactly once
```

Require:

```text
Status=GREEN
HostReused=true
HostRebuilt=false
Unity process count before/after=0
hidden smoke passed
all self-checks passed
host files unchanged
actual payload package/composition/final hashes match normal build
actual payload generated-world facts match typed summary
actual payload accepted-mechanics facts complete
RC record CURRENT and current-package correlated
```

Copy the complete generated project to a second short path:

```text
source record valid
generated summary restored
AcceptedMechanics restored
RC record CURRENT
no build/Runtime/Unity execution
```


## L. Required behavioral tests

Create at least 36 Goal156 tests; at least 30 must be behavioral.

Behavioral means invoking real generation/create/overlay/composition/build/controller/WinForms/
standalone services and asserting files, package content, state or process plans. Reflection and
source-string tests do not count.

### Creation and legacy compatibility

1. template request preserves legacy package/folders;
2. seeded request creates final folder only after complete validation;
3. seeded request writes every required sidecar;
4. seeded request initializes all-selectable authoring from current catalog;
5. core-only profile initializes no optional modules;
6. project list discovers seeded project;
7. current package load/open works immediately;
8. existing target rejected with no mutation;
9. injected creation failure removes temp/final;
10. invalid mode/preset/profile rejected causally.

### Determinism and variation

11. same seed/options produces byte-identical plan;
12. same seed/options produces byte-identical rule pack;
13. same seed/options produces byte-identical tiny loop;
14. same seed/options produces byte-identical generated MVP;
15. same seed/options produces byte-identical overlay/base;
16. different project identities do not change identity-free generation hashes;
17. different seed changes overlay/base and content selection;
18. every supported mode produces valid nonempty source;
19. preset/style/variant ordering is deterministic;
20. no timestamp/path/machine data appears in sidecars.

### Overlay

21. every baseline ID/definition preserved canonically;
22. generated records are additive;
23. differing ID collision rejected;
24. equal definition collision deduplicated truthfully;
25. generated references validate;
26. baseline manifest/start map preserved before identity overlay;
27. generated start map recorded/present;
28. source counts match actual overlay/package;
29. generated provenance is deterministic;
30. Goal142 baseline source remains byte-identical.

### Explicit custom base composition

31. default null base preserves historical package/hash fixtures;
32. explicit generated base bytes/hash used by forward plan;
33. reverse-order proof uses same generated base;
34. tampered explicit base hash rejected;
35. all-selectable modules compose over generated base;
36. selected module mutations leave generated records unchanged;
37. core-only generated base qualifies without false Goal142 distinctness failure;
38. no explicit-base request leaks into another build.

### Build/workspace/history

39. generated all-selectable build GREEN;
40. repeat build deterministic;
41. fresh reopen restores generated summary/current status;
42. generated source corruption fails build and preserves last success;
43. generated overlay remains in composition and activated packages;
44. failed module/qualification transaction preserves source/package/history;
45. legacy project build uses old baseline lane;
46. generated summary persists in GREEN history;
47. old history without generated summary remains readable;
48. saved mechanics change yields LAST_SUCCESS build facts while source remains valid/current.

### UI

49. CreateGameDialog defaults to seeded generated;
50. folder default deterministically fills seed until manually edited;
51. mode/preset/profile choices are data-derived;
52. Russian validation messages for invalid generation input;
53. generated card SOURCE_READY after create;
54. generated card BUILD_CURRENT after build;
55. card LAST_SUCCESS after saved mechanic change;
56. invalid source cannot display ready card;
57. card contains no IDs/hashes/absolute paths;
58. layout formatter handles long values without clipping/duplicate facts.

### Standalone and portability

59. exactly one real hidden smoke on generated all-selectable project;
60. host reused/not rebuilt and Unity starts zero;
61. actual payload contains generated-world facts;
62. actual payload contains accepted-mechanics/RC facts;
63. payload hashes correlate with normal build;
64. host file set unchanged;
65. copied complete generated project restores generated/accepted/RC CURRENT without execution.

### Regressions

66. Goal155A truth matrix remains GREEN;
67. Goal155 Profile A/B/core-only remains GREEN;
68. Goal154D all-selected completion paths remain GREEN;
69. Goal153C/150/149 focused regressions remain GREEN;
70. procedural kernel/registry/tiny-loop/generated-package existing tests remain GREEN;
71. one-click generated preview existing tests remain GREEN;
72. source `goal148-manual` remains byte-identical.

Do not claim these numbers unless tests are discovered and executed.

## M. Focused validation

### Build/discovery

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal156"
```

Require:

```text
Goal156 discovered >=36
Goal156 behavioral >=30
```

### Focused filters

```powershell
dotnet test ... --filter "FullyQualifiedName~Goal156"
dotnet test ... --filter "FullyQualifiedName~Goal155A"
dotnet test ... --filter "FullyQualifiedName~Goal155"
dotnet test ... --filter "FullyQualifiedName~Goal154D"
dotnet test ... --filter "FullyQualifiedName~Goal153C"
dotnet test ... --filter "FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronization"
dotnet test ... --filter "FullyQualifiedName~Goal149"

dotnet test ... --filter "FullyQualifiedName~ProceduralGameKernel"
dotnet test ... --filter "FullyQualifiedName~FormulaEffectActionRegistry"
dotnet test ... --filter "FullyQualifiedName~TinyGeneratedRuntimeLoop"
dotnet test ... --filter "FullyQualifiedName~GeneratedPackageMvp"
dotnet test ... --filter "FullyQualifiedName~OneClickGeneratedPreview"

dotnet test ... --filter "FullyQualifiedName~FeatureModuleParameterizedComposition"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"
dotnet test ... --filter "FullyQualifiedName~ProjectLifecycle"
dotnet test ... --filter "FullyQualifiedName~ProjectStandaloneBuild"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleLibrary"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleCertification"
```

Run:

```powershell
.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Then run the real generated-project creation/build/portable matrix and exactly one hidden standalone
smoke.

Run artifact scope last.

### Forbidden

Do not run:

```text
full suite
85-case historical closure
all-ProductSmoke
Unity host build
visible automatic standalone launch
unchanged failed command retry
timeout escalation loop
```

A zero-match filter is failure.

## N. Evidence

Create exactly 14 files in each mirrored root:

```text
goal156-dashboard.json
architecture-review.json
goal155a-independent-audit-intake.json
creation-contract-proof.json
generation-determinism-proof.json
mode-preset-matrix-proof.json
overlay-baseline-preservation-proof.json
custom-base-composition-proof.json
generated-project-build-proof.json
generated-world-ui-proof.json
generated-standalone-payload-proof.json
failure-rollback-proof.json
artifact-scope-proof.json
goal156-report.md
```

Roots:

```text
.llmgc/procedural/goal-156-seeded-generated-project-creation-modern-workspace-and-cached-standalone/
.llmgc/exports/goal-156-seeded-generated-project-creation-modern-workspace-and-cached-standalone/
```

Procedural/export twins byte-identical by name and SHA-256.

### Dashboard fields

```text
status
candidateStatus
goal156TestsDiscovered
goal156BehavioralTestsPassed

goal155aIndependentAuditPassed
goal155MilestoneRcPassed

legacyTemplateCompatibilityPassed

sameSeedPlanStable
sameSeedRulePackStable
sameSeedTinyLoopStable
sameSeedMvpStable
sameSeedOverlayStable
differentSeedVariationPassed
supportedModeCount
supportedModeMatrixPassed

baselineDefinitionsPreserved
generatedRecordsAdditive
generatedReferenceValidationPassed
generatedCollisionRejectionPassed
goal142SourceByteIdentical

explicitGeneratedBaseUsed
explicitBaseHashValidated
generatedAllSelectableCompositionPassed
generatedCoreOnlyCompositionPassed
generatedRecordsPreservedAfterModules
historicalBaselineHashesPreserved

generatedProjectCreated
generatedProjectListed
generatedProjectOpened
generatedSourceValid
generatedSourceSidecarCount

allSelectableSelectedMechanicCount
allSelectableExplicitParameterCount
allSelectableBuildPassed
allSelectableRepeatBuildDeterministic
allSelectableFreshReopenCurrent
acceptedMechanicsPassed
generatedSummaryPassed

coreOnlyBuildPassed
coreOnlyGeneratedSummaryPassed
coreOnlyAcceptedMechanicsPassed=false

hostCacheKey
hostReused
hostRebuilt
hostFileSetHashUnchanged
unityProcessStartCount
hiddenSmokeInvocationCount
hiddenSmokePassed
standaloneSelfChecksPassed
actualPayloadGeneratedFactsPassed
actualPayloadAcceptedFactsPassed
releaseCandidateRecordCurrent
portableCopyCurrent

goal155aRegressionPassed
goal155RegressionPassed
goal154dRegressionPassed
goal153cRegressionPassed
goal150RegressionPassed
goal149RegressionPassed
proceduralLegacyRegressionPassed
sourceGoal148ByteIdentical

artifactScopeViolationCount
goal156Accepted=false
goal156ManualReviewRequired=false
goal156IndependentAuditRequired=true
```

No GREEN required value may be null/PARTIAL/NOT_EXECUTED/unverified constant.

## O. Documentation/state

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/ROADMAP_TO_FULL_GENERATOR.md
```

Create:

```text
docs/manual-acceptance/goal156-seeded-generated-project-creation.md
```

This is an automated goal, so the file states no manual gate.

### Required GREEN publication

```text
goal155aIndependentAuditResult=GREEN_ACCEPTABLE_CANDIDATE_AT_EBAA4ABA
goal155aIndependentAuditPassed=true
goal155IndependentAuditRequired=false
goal155aIndependentAuditRequired=false
goal155MilestoneRcPassed=true

goal156ImplementationStatus=GREEN
goal156CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal156Accepted=false
goal156AcceptedByHuman=false
goal156AcceptedByCodex=false
goal156ManualReviewRequired=false
goal156ManualGateReady=false
goal156IndependentAuditRequired=true

goal156SeededProjectCreationPassed=true
goal156LegacyTemplateCompatibilityPassed=true
goal156GeneratedOverlayPassed=true
goal156CustomBaseCompositionPassed=true
goal156AllSelectableGeneratedBuildPassed=true
goal156CoreOnlyGeneratedBuildPassed=true
goal156GeneratedSummaryPersistencePassed=true
goal156HostReused=true
goal156HostRebuilt=false
goal156UnityProcessStartCount=0
goal156HiddenSmokeInvocationCount=1
goal156PortableCopyPassed=true
goal156ArtifactScopeViolationCount=0

nextAction=independent_goal156_audit_and_plan_seed_regeneration_or_generated_world_activation
```

Correct stale active `gate_status/current_user_action` text to Goal156.

Release risk statement:

```text
Goal156 creates a real seeded generated project without LLM/provider/Lua/Unity generation.
The generated world is additive and preserved through modern FeatureModule build/replay/standalone.
The baseline start map remains active by design; switching gameplay start/travel into generated maps
is the next product decision, not hidden completion in Goal156.
```

This explicit limitation is not a P1 because the generated tiny loop is executed and generated content
is present/validated; generated-map activation is a later product slice.

P2/P3 go to debt with impact/defer rule.

## P. Text integrity

Scan all changed/task/evidence/docs files:

```text
valid UTF-8
no NUL
no forbidden C0 except CR/LF/TAB
no mojibake markers
no escaped Cyrillic in user-facing JSON/Markdown where policy forbids
no absolute disposable/source paths in committed evidence
```

Existing generation artifacts/historical evidence are immutable.

## Q. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal156-seeded-generated-project.ps1
.devflow/scripts/run-goal156-seeded-generated-project.cmd

src/LLMGameCreator.Application/Projects/CreateGameProjectRequest.cs
src/LLMGameCreator.Application/Projects/GameProjectService.cs
src/LLMGameCreator.Application/Projects/GameProjectSummary.cs
src/LLMGameCreator.Application/Projects/NewGamePackageFactory.cs
src/LLMGameCreator.Application/Projects/SeededGeneratedGameProjectCreationService.cs

src/LLMGameCreator.Application/Generation/Procedural/SeededGeneratedProjectModels.cs
src/LLMGameCreator.Application/Generation/Procedural/SeededGeneratedProjectSourceService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedProjectOverlayService.cs

src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterizedCompositionService.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionService.cs
src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionModels.cs

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildHistoryReader.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectGeneratedWorldSummaryService.cs

src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.WinForms/Pages/Projects/CreateGameDialog.cs
src/LLMGameCreator.WinForms/Pages/Projects/CreateGameDialog.Designer.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/Goal156/Goal156ProjectCreationTests.cs
tests/LLMGameCreator.Tests/Application/Goal156/Goal156DeterminismAndOverlayTests.cs
tests/LLMGameCreator.Tests/Application/Goal156/Goal156CustomBaseCompositionTests.cs
tests/LLMGameCreator.Tests/Application/Goal156/Goal156GeneratedWorkspaceTests.cs
tests/LLMGameCreator.Tests/Application/Goal156/Goal156WinFormsCreationTests.cs
tests/LLMGameCreator.Tests/Application/Goal156/Goal156StandaloneAndPortabilityTests.cs
tests/LLMGameCreator.Tests/Application/FeatureModuleAuthoring/FeatureModuleParameterizedCompositionTests.cs
tests/LLMGameCreator.Tests/ProjectLifecycleTests.cs
tests/LLMGameCreator.Tests/WinForms/UnifiedGameProjectWorkspaceTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/ROADMAP_TO_FULL_GENERATOR.md
docs/manual-acceptance/goal156-seeded-generated-project-creation.md

docs/agent-tasks/goal-156-seeded-generated-project-creation-modern-workspace-and-cached-standalone/
.llmgc/procedural/goal-156-seeded-generated-project-creation-modern-workspace-and-cached-standalone/
.llmgc/exports/goal-156-seeded-generated-project-creation-modern-workspace-and-cached-standalone/
```

If concrete compile/test failure proves one additional existing Application/WinForms/test/model path
is required, record exact reason and add exact path only.

Forbidden without a newly reproduced Goal156 P0/P1:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
catalogs/feature-modules/**
unity/**
samples/**
generator-library/**
provider/**
LLM/**
RAG/**
public GamePackage schema files
```

Do not change old procedural services unless a test proves a correctness defect. Prefer orchestration
and new mapping services.

## R. Command budget

```text
read-first and architecture review: 12 minutes
creation/source/overlay implementation: 18 minutes
custom-base composition integration: 18 minutes
build/history/generated summary: 14 minutes
WinForms creation/card: 12 minutes
behavioral tests: 24 minutes
real matrix + one standalone smoke: 12 minutes
regressions/evidence/docs/artifact scope: 16 minutes
target wall clock: 105 minutes
maximum two concurrent testhost processes
Unity process count: 0
```

Rules:

```text
write test inventory before production edits
create publication/evidence script before the long external matrix
no unchanged command repetition
no timeout escalation
after failure run only exact class/test
P0/P1 fixed in Goal156
P2/P3 debt only
do not defer docs/evidence/artifact scope after product code
```

## S. Publication

Create exactly one final commit:

```text
GREEN Goal 156 seeded generated project creation modern workspace and cached standalone
```

or honest BLOCKED/FAILED.

Codex performs standard push.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three Goal156 task files tracked
source goal148-manual unchanged
Goal142 baseline unchanged
Unity process count=0
HostRebuilt=false
hidden smoke count=1 only on GREEN
Goal154 acceptance unchanged
Goal155 milestone audit passed
Goal156 accepted=false
no human gate
```

## T. GREEN criteria

```text
Goal155A independent audit recorded GREEN
Goal156 tests >=36 / behavioral >=30 / all pass

legacy template creation unchanged
seeded creation is atomic and normal-project-visible
generation source/sidecars deterministic and valid
same seed stable / different seed varied
all supported modes validated

generated overlay additive and collision-safe
all baseline definitions preserved
Goal142 baseline byte-identical
generated references valid

explicit custom base used consistently
historical default baseline fixtures preserved
all-selectable modules compose over generated base
generated records survive module composition/build/activation

generated all-selectable project build/repeat/reopen GREEN
generated core-only project build GREEN
generated summary typed/persisted/restored
failure rollback preserves project/source/history

CreateGameDialog exposes seeded generation
generated-world card truthful/readable/no IDs/hashes/paths

one cached hidden standalone smoke
HostReused=true / HostRebuilt=false / Unity=0
actual payload generated + accepted facts correlate
RC record CURRENT
portable project restores generated/accepted/RC state without execution

Goal155A/155/154D/153C/150/149 and legacy procedural regressions GREEN
14+14 evidence mirrored
text integrity GREEN
artifact scope 0
goal156CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
one final commit pushed
```

## U. Final report

Return GREEN/BLOCKED/FAILED and include:

- model/reasoning;
- architecture-review result;
- Goal155A independent-audit intake;
- discovered/behavioral test counts;
- legacy template compatibility;
- exact generated creation request/profile;
- same-seed and different-seed hashes/variation;
- supported mode/preset matrix;
- overlay counts, baseline preservation and collision/reference proof;
- explicit custom-base composition proof;
- generated all-selectable/core-only build/repeat/reopen results;
- generated summary/UI states;
- failure rollback;
- host key/hash/reuse, Unity/smoke count;
- actual payload generated/accepted facts;
- RC record and portable copy;
- focused regressions;
- source/baseline immutability;
- evidence/text/artifact scope;
- Goal156 state/no-human-gate;
- final SHA/push/HEAD/worktree.
