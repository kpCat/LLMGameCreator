# Goal 161 — Profile-Neutral World Change & Generated Gameplay Save Migration

## Identity

- Task ID: `goal-161-profile-neutral-world-change-and-generated-gameplay-save-migration`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `d8dd05e7be8d87496c75a15a0c2f7ab2e454d0dc`
- Required base message: `GREEN Goal 160 sealed regeneration commit and generated world history rollback`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Sol
Reasoning effort: Extra High
```

Reason: this is a major player-state lifecycle vertical slice. It closes one independent-audit P1 in
Goal160's semantic commit validator and converts the existing raw Runtime snapshot facility into a
generated-world-aware save system with immutable revisions, exact same-world restoration, controlled
migration after regeneration/history rollback, WinForms management, standalone correlation and
portable truth. It must not invent a second Runtime state model.

## Pre-approval

- The owner approved execution by launching this task.
- Produce a concise internal plan; do not ask for confirmation.
- Do not request intermediate or final manual testing.
- Own all P0/P1 defects reproduced by the mandatory Goal161 matrix inside this Goal.
- Record P2/P3 debt without creating Goal161A.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No candidate/intermediate commits.
- Codex performs commit and standard push itself.

## Expected initial worktree

After unpacking, only these untracked files are permitted:

```text
docs/agent-tasks/goal-161-profile-neutral-world-change-and-generated-gameplay-save-migration/GOAL.md
docs/agent-tasks/goal-161-profile-neutral-world-change-and-generated-gameplay-save-migration/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-161-profile-neutral-world-change-and-generated-gameplay-save-migration/README.md
```

Require:

```text
HEAD == origin/main == d8dd05e7be8d87496c75a15a0c2f7ab2e454d0dc
branch=main
tracked diff count=0
staged diff count=0
unknown dirty/untracked count=0
```

Any other dirt blocks execution. Never use reset, stash, merge, rebase or destructive cleanup.

## Unity and standalone budget

```text
Unity Editor invocation budget: 0
Unity host build budget: 0
real hidden standalone smoke budget: exactly 1
visible automated standalone launch budget: 0
```

The smoke runs after a successful save migration and ordinary standalone rebuild.

## Goal160 independent-audit result

Record:

```text
goal160IndependentAuditResult=BLOCKED_AT_D8DD05E7
goal160IndependentAuditBlocker=semantic_commit_validator_requires_complete_accepted_mechanics_for_core_only_generated_projects
goal160AuditBlocker=closed_by_goal161 only on GREEN
```

Goal160 implementation remains useful and GREEN; accepted=false; no human gate.

### Useful Goal160 foundation to preserve

```text
one process-wide operation coordinator in the product CompositionRoot
cross-process project mutation lock
sealed regeneration/rollback candidates
truth-token and authoritative-inventory recheck inside transaction
journal validating state
semantic validation before committed/backup cleanup
exact rollback and crash recovery
strict generated-world history containing generation artifacts only
history rollback rebuilt with current identity/mechanics
old RC LAST_SUCCESS then new RC CURRENT after standalone
```

Do not restart these systems.

## Independent-audit P1

Goal158 explicitly established the supported `core_only` generated profile:

```text
project build GREEN
generated start/travel GREEN
AcceptedMechanics Present=true, Passed=false
MissingFactKinds nonempty
RC READY not claimed
```

Goal160's `GameProjectSeedRegenerationCommitValidator` currently requires every selected history row to
contain:

```text
AcceptedMechanics Present=true, Passed=true
AcceptedMechanicsCompatibility Passed=true
```

Therefore a valid core-only generated project can:

```text
create rollback/regeneration candidate
build/repeat/reopen TRAVEL_CURRENT
seal candidate
enter transaction
```

but always fails semantic validation and rolls back.

This is a real product regression from the supported Goal158/159 contract.

## Required profile-neutral correction

Semantic commit validation must verify exact candidate truth, not impose all-selectable readiness.

Add to the candidate seal:

```text
MechanicsProfileId
AcceptedMechanicsSummarySha256
AcceptedMechanicsCompatibilitySha256
ExpectedCandidateRcRecordStatus
ExpectedCandidateRcOverallStatus
```

The hashes are over canonical typed objects from the sealed candidate snapshot/build.

Commit validation requires:

```text
GeneratedWorld/Activation/Travel complete
AcceptedMechanicsCompatibility Passed=true
current history AcceptedMechanics exactly matches sealed summary
current history compatibility exactly matches sealed compatibility result
current RC record/overall statuses match the sealed candidate expectation
```

Readiness rules:

```text
all_selectable_defaults with complete current selected catalog:
  AcceptedMechanics Passed=true
  candidate overall RC BUILD_GREEN_STANDALONE_PENDING

core_only:
  AcceptedMechanics Present=true
  AcceptedMechanics Passed=false
  MissingFactKinds nonempty
  candidate RC ABSENT or LAST_SUCCESS
  candidate overall status must not claim RC READY/PENDING from incomplete accepted mechanics

custom user-edited selection:
  exact sealed AcceptedMechanics truth is accepted
  RC readiness is derived by the existing generic accepted-mechanics projection
```

Do not branch production correctness on fixed selected-module counts.

Add real core-only regeneration and history-rollback apply tests.

## Product problem: raw Runtime snapshots

The existing `RuntimeSnapshotStore` writes only:

```text
.llmgc/runtime-saves/<slot>.runtime.json
```

containing raw `UnifiedRuntimeSession`.

It does not bind the session to:

```text
project identity
generated WorldId
source request/plan/base
actual package/build hashes
authoring fingerprint
definition fingerprints
```

After regeneration or history rollback, a raw snapshot may contain:

```text
old map ID and position
old generated item/quest/faction/encounter/dialogue/status IDs
old package ID
world-bound events and flags
```

Loading it directly into the current generated package is unsafe and untruthful.

## Goal161 product outcome

Generated projects receive a first-class save system:

```text
play in Runtime Simulator
→ save generated gameplay slot
→ exact same-world load
→ regenerate or restore another world
→ slot becomes Migration required
→ preview preservation/drop report
→ migrate into current world
→ load migrated revision
→ continue movement/travel/gameplay
→ build cached standalone with save-migration facts
```

The original save revision is immutable.

Legacy/template projects continue using the existing raw Runtime snapshot workflow unchanged.

## Non-goals

Do not add:

```text
new Runtime commands or state fields
new FeatureModules or public GamePackage fields
Unity project/host changes
automatic migration without user action
cross-project save import
cloud saves
multiplayer state
merging generated quest/world progression
restoring an old package to satisfy an old save
provider/LLM/Lua/media execution
```

Generated-world migration resets world-bound location/transient state by explicit policy. It does not
pretend to preserve incompatible generated content.


## Mandatory architecture review

Read at most 18 primary files:

```text
AGENTS.md
docs/GOAL_DESIGN_QUALITY_POLICY.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/UNITY_EXECUTION_POLICY.md
RuntimeContracts.cs
RuntimeStateSerializer.cs
RuntimeSnapshotStore.cs
UnifiedGameRuntimeService.cs
RuntimeSimulatorPageControl.cs
GameProjectSeedRegenerationCommitValidator.cs
GameProjectSeedRegenerationCandidateSealService.cs
GameProjectSeedRegenerationService.cs
GameProjectGeneratedWorldRollbackService.cs
GeneratedWorldHistoryService.cs
GameProjectOperationCoordinator.cs
UnifiedGameProjectWorkspaceController.cs
ProjectsPageControl.cs
GameProjectReleaseCandidateRecordService.cs
```

Before production edits write:

```text
.llmgc/procedural/goal-161-profile-neutral-world-change-and-generated-gameplay-save-migration/architecture-review.json
```

Required resolved sections:

```text
goal160IndependentAudit
profileNeutralCommitValidation
existingRawSnapshotBoundary
generatedSaveEnvelope
immutableRevisionStore
definitionFingerprintInventory
sameWorldLoadContract
crossWorldMigrationPolicy
mapStateMigration
gameplayStateMigration
generatedReferenceClassification
operationLeaseIntegration
worldChangeStatusPropagation
runtimeSimulatorWorkflow
projectsSaveManagementUi
standaloneCorrelation
portableCopy
legacySnapshotCompatibility
failureRollbackMatrix
nonGoals
```

Every section names exact types, inputs, outputs, status transitions and behavioral tests.

## A. Generated gameplay-save storage

Create Application-layer services and models, naming flexible:

```text
GeneratedGameplaySaveService
GeneratedGameplaySaveStore
GeneratedGameplaySaveValidator
GeneratedGameplaySaveMigrationService
GeneratedGameplayDefinitionFingerprintService
```

Project-relative root:

```text
.llmgc/gameplay-saves/
```

Slot layout:

```text
.llmgc/gameplay-saves/<slot-name>/
  slot.json
  revisions/
    <revision-sha256>.json
```

Do not reuse `.llmgc/runtime-saves` for generated-aware saves.

### A1. Slot names

Use the existing safe vocabulary:

```text
letters
digits
dot
dash
underscore
```

Reject:

```text
blank
..
directory separators
path escape
```

Diagnostics:

```text
generated_save.slot_invalid
generated_save.path_escape
```

No fixed number of slots or revisions.

### A2. Slot manifest

Schema:

```text
generated_gameplay_save_slot_v1
SlotName
CurrentRevisionSha256
RevisionSha256s[]
```

Rules:

```text
revision list unique and ordinal-sorted or stable append order documented
current revision exists in the list
every referenced revision file validates
no unreferenced file may silently become current
```

Update `slot.json` atomically after immutable revision write.

### A3. Save revision envelope

Schema:

```text
generated_gameplay_save_v1
RevisionSha256
ParentRevisionSha256 optional
Migration optional

ProjectPackageId
ProjectIdentityFingerprint
WorldId
SourceRecordSha256
SourceRequestSha256
PlanSha256
OverlaySha256
GeneratedBasePackageSha256

PackageSha256
CompositionPackageSha256
QualifiedAuthoringFingerprint
SelectedBuildHistoryFileName
SelectedBuildHistorySha256

UnifiedRuntimeSessionJson
UnifiedRuntimeSessionSha256
MapStateSha256
GameplayStateSha256

CurrentMapId
CurrentRegionSourceId
DefinitionFingerprints[]
GeneratedReferenceIds[]
PortableFlagKeys[]
SaveFacts
```

Optional operational display timestamp may exist but is excluded from `RevisionSha256`.

`RevisionSha256` is SHA-256 over the canonical envelope with:

```text
RevisionSha256 empty
operational timestamp omitted
```

Revision file name must equal the revision hash.

### A4. Migration object

```text
generated_gameplay_save_migration_v1
SourceRevisionSha256
SourceWorldId
TargetWorldId
SourcePackageSha256
TargetPackageSha256
MigrationPolicyId
MapReset
PreservedCounts
DroppedCounts
PreservedDefinitionIds[]
DroppedDefinitionIds[]
DroppedReasons[]
```

No absolute paths.

### A5. Immutable revisions

If a revision file already exists:

```text
equal bytes -> deduplicated no-op
unequal bytes at same hash/path -> generated_save.revision_collision
```

Never overwrite an existing revision.

A migration creates a new revision and changes only the slot manifest pointer.

The source revision remains valid and loadable when its historical world/package is restored later.

## B. Current project truth

Create a typed capture:

```text
GeneratedGameplaySaveProjectTruth
```

Fields:

```text
ProjectFolder
Identity
IdentityFingerprint
StrictGeneratedSource
WorldId
ActualPackage
PackageSha256
CompositionPackageSha256
QualifiedAuthoringFingerprint
SelectedBuildHistoryFileName
SelectedBuildHistorySha256
GeneratedStartMapId
GeneratedRegionMapBindings
DefinitionFingerprintInventory
```

Require:

```text
generated source current
package bytes match document activated hash
build history is current GREEN travel history
GeneratedWorld TRAVEL_CURRENT
GeneratedWorldActivation Passed
GeneratedRegionTravel Passed
```

A source-only or stale generated project cannot save/load/migrate gameplay state.

Diagnostics:

```text
generated_save.project_not_ready
generated_save.package_changed
generated_save.history_not_current
generated_save.travel_not_current
```

Use the current Goal160 operation coordinator and cross-process project lock while capturing and
writing/loading/migrating.

## C. Definition fingerprint inventory

Create canonical typed entries:

```text
GeneratedGameplayDefinitionFingerprint
  Kind
  Id
  CanonicalSha256
  Generated
  SourceId optional
```

Derive from the actual current package and `GeneratedContent` provenance.

Supported kinds include every Runtime-session reference family actually present:

```text
map
item
resource
stat
progression
status
quest
dialogue
faction
encounter
ability
interaction
entity
equipment_slot when a package definition exists
```

Do not invent placeholder definitions.

### C1. Session reference extraction

Extract all IDs referenced by:

```text
MapState.CurrentMapId
MapState.Flags keys and values when exact IDs appear
GameplayState.PackageId
GameplayState.CurrentMapId
Inventories and item stacks
Equipment slots/items
Resources
Stats
Progressions
Runtime flags
Statuses and targets
Active encounter and participant references
QuestStates and Quests/Objectives
Active dialogue
Factions
Gameplay Metadata values when exact IDs appear
```

Store fingerprints for definitions that exist.

Store unresolved references separately and reject save creation unless they are allowed scalar IDs
such as player/inventory owner IDs.

### C2. Generated classification

An ID is generated when correlated through:

```text
GeneratedContent records
strict generated MVP maps/entities/provenance
Goal158 travel-gate namespace
```

Do not classify arbitrary IDs as generated only because a string contains “generated”.

Travel-gate definitions are generated and world-bound.

### C3. Canonical equality

A referenced definition is portable only when the current package contains exactly one same-kind,
same-ID definition with the same canonical SHA-256.

ID equality alone is insufficient.

## D. Save creation

Input:

```text
project folder
slot name
actual UnifiedRuntimeSession from Runtime Simulator/current player session
```

Require:

```text
session serializes/deserializes exactly
MapState current map exists
position in bounds and walkable
GameplayState.PackageId equals current project package ID or is normalized from empty only
GameplayState.CurrentMapId empty or equals MapState.CurrentMapId
all referenced definitions valid
no active state references missing definitions
```

Store exact session.

Same session/current truth saved again:

```text
same revision hash
deduplicated
slot remains current
```

Save creation uses operation kind:

```text
gameplay_save
```

Add it to `GameProjectOperationKinds`.

No build/standalone/regeneration/history rollback can overlap.

## E. Save status model

Create:

```text
GeneratedGameplaySaveStatus
```

Values:

```text
CURRENT
PACKAGE_REBASE_REQUIRED
WORLD_MIGRATION_REQUIRED
INVALID
LEGACY_RAW
```

### E1. CURRENT

Require:

```text
project identity matches
WorldId matches
source request/plan/base hashes match
actual package/composition/authoring/history hashes match
session and definition fingerprints validate
```

### E2. PACKAGE_REBASE_REQUIRED

Use when:

```text
same WorldId
package/build/authoring differs
```

Migration may preserve compatible definitions but resets invalid/transient state as required.

### E3. WORLD_MIGRATION_REQUIRED

Use when:

```text
identity matches
WorldId differs
```

Do not permit direct load.

### E4. INVALID

Examples:

```text
session hash mismatch
revision/slot manifest mismatch
foreign project identity
tampered definition inventory
missing referenced revision
malformed JSON
current package itself invalid
```

### E5. LEGACY_RAW

Existing `.llmgc/runtime-saves/*.runtime.json` entries remain:

```text
listed for legacy/template projects
loadable through existing RuntimeSnapshotStore for legacy projects
not automatically loaded into a generated project
```

A generated project surfaces them as unverified legacy snapshots with a causal warning.

Do not delete or rewrite them.


## F. Exact same-world load

For `CURRENT` revision:

1. acquire operation kind `gameplay_load`;
2. recapture project truth under the lease;
3. reread and validate slot/revision;
4. deserialize the exact `UnifiedRuntimeSession`;
5. validate every reference and map position against the current package;
6. return the exact session.

Require canonical equality:

```text
serialized loaded session == stored session JSON
MapState hash matches
GameplayState hash matches
```

Do not run `Start()` or reset any field on exact load.

A load does not mutate save files.

## G. Controlled save migration

Migration accepts only:

```text
PACKAGE_REBASE_REQUIRED
WORLD_MIGRATION_REQUIRED
```

It produces a preview first, then atomically writes a new revision.

Operation kind:

```text
gameplay_save_migration
```

### G1. Cross-world map policy

For `WORLD_MIGRATION_REQUIRED`:

```text
MapState.CurrentMapId = current generated start map
MapState.PlayerPosition = current start position
MapState.Mode = "map"
GameplayState.CurrentMapId = current generated start map
```

Always set:

```text
MapReset=true
```

Do not guess equivalent coordinates or map IDs across seeds.

For package rebase within the same world:

```text
retain current map and position only when exact map definition fingerprint matches
and the position remains valid/walkable
otherwise reset to generated start
```

### G2. Transient state policy

Always reset across world migration:

```text
MapEvents
GameplayEvents
ActiveEncounter
ActiveDialogue
Statuses with transient/active target state
Tick -> 0
```

For package rebase, active encounter/dialogue are also reset unless the task can prove an exact
canonical definition and stable participant/node state. Default Goal161 policy is reset.

Report each reset.

### G3. Portable state policy

Preserve only entries whose referenced definitions are canonically equal in the target package.

Families:

```text
inventory stacks
equipment items
resources
stats
progressions
faction reputation
quest state only for canonically equal non-generated quests/objectives
statuses only when definition and target are portable and status is not transient
```

Generated old-world references normally fail equality and are dropped.

Baseline/FeatureModule definitions that remain canonically equal are preserved.

### G4. Flags and metadata

For:

```text
MapState.Flags
GameplayState.Flags
GameplayState.Metadata
inventory/equipment/quest/faction metadata
```

Preserve a key/value pair only when:

```text
neither key nor value exactly references a dropped generated/world-bound ID
all exact definition references remain portable
value is scalar UTF-8 text within existing limits
```

Otherwise drop and report.

Do not use substring heuristics against arbitrary words.

### G5. Owner and player IDs

Stable structural IDs such as:

```text
player
player inventory owner
global scope
```

may be retained when not tied to a package definition.

Document the allowlist as Runtime structural vocabulary, not generated content counts.

### G6. Package identity

Set:

```text
GameplayState.PackageId = current project package ID
```

All migrated Runtime states must validate against the current package.

### G7. Migration preview

Create:

```text
GeneratedGameplaySaveMigrationPreview
```

Fields:

```text
SlotName
SourceRevisionSha256
SourceStatus
SourceWorldId
TargetWorldId
SourcePackageSha256
TargetPackageSha256
MapReset
PreservedCountsByKind
DroppedCountsByKind
PreservedDefinitionIds
DroppedDefinitionIds
DroppedReasons
CandidateSessionSha256
CandidateMapStateSha256
CandidateGameplayStateSha256
Diagnostics
Passed
```

No save mutation occurs during preview.

### G8. Apply

Apply accepts only:

```text
SlotName
SourceRevisionSha256
CandidateSessionSha256
```

Use cached immutable preview.

Before write:

```text
recheck current project truth
revalidate source revision
recompute candidate session and migration report
require exact hashes
```

Write immutable revision, then atomically update slot manifest.

Failure leaves the entire slot byte-identical.

## H. Save migration validation

After migration require:

```text
new revision status CURRENT
exact load succeeds
current package validates every retained reference
current map/start position valid
no dropped definition remains referenced
source revision still valid and immutable
migration object matches actual preserved/dropped state
```

Then execute on the loaded migrated session:

```text
at least one valid Move command
one generated travel gate transition when a route is reachable
one destination generated interaction
```

Use real `IUnifiedGameRuntimeService`/`IGameRuntime`.

Replay the post-load command sequence from a fresh load and require the same final state hash/events.

## I. World-change integration

Regeneration and history rollback do not mutate save slots.

Immediately after a world change:

```text
old saves are dynamically WORLD_MIGRATION_REQUIRED
or PACKAGE_REBASE_REQUIRED
```

The world-change transaction's authoritative inventory already includes gameplay saves and the shared
operation lease prevents concurrent save mutation.

Add automated assertions:

```text
save tree byte-identical during regeneration
save tree byte-identical during history rollback
status changes only because current world/package truth changed
```

After restoring the exact historical world that matches the original revision:

```text
the original old revision becomes CURRENT again
```

This proves immutable revisions are reusable and world identity is truthful.

## J. Profile-neutral world-change commit

Extend candidate seal and semantic commit validator as described above.

### J1. All-selectable

Require:

```text
AcceptedMechanics exact sealed hash
AcceptedMechanics Passed=true
RC expected LAST_SUCCESS/ABSENT before standalone
overall status BUILD_GREEN_STANDALONE_PENDING when complete
```

### J2. Core-only

Require:

```text
AcceptedMechanics exact sealed hash
AcceptedMechanics Present=true
AcceptedMechanics Passed=false
MissingFactKinds nonempty
AcceptedMechanicsCompatibility Passed=true
GeneratedWorld/Activation/Travel Passed=true
RC does not claim CURRENT/READY/PENDING readiness
```

Regeneration and history rollback apply must commit successfully.

### J3. Custom selection

Require exact sealed typed summaries and generic RC-status agreement.

No special module IDs or counts.

## K. Workspace integration

Extend snapshot:

```text
GeneratedGameplaySaves
GeneratedGameplaySaveCurrentCount
GeneratedGameplaySaveMigrationRequiredCount
GeneratedGameplaySaveInvalidCount
LastGeneratedGameplaySaveMigration
```

Add controller/service methods:

```text
ListGeneratedGameplaySaves()
PreviewGeneratedGameplaySaveMigration(slot)
ApplyGeneratedGameplaySaveMigration(preview)
```

The Runtime Simulator may call save/load through the generated save service directly with the current
session, or through a focused controller facade.

### K1. Operation state

Add operation kinds:

```text
gameplay_save
gameplay_load
gameplay_save_migration
```

Save/load/migration reject while:

```text
build
standalone
authoring save
regeneration
history rollback
recovery
```

World change operations reject while save mutation/load is active.

## L. Runtime Simulator integration

For a valid generated project:

```text
Save snapshot
Load snapshot
List snapshots
```

use the generated-aware service.

Legacy/template projects keep the old `IRuntimeSnapshotStore`.

Add Russian generated-save status output:

```text
Текущее
Требуется обновление пакета
Требуется перенос в новый мир
Повреждено
Старое непроверенное сохранение
```

Add one button:

```text
Перенести сохранение
```

Enabled only for a selected migratable generated save.

On migration success, load the new current revision into `_session`.

Do not auto-migrate on Load.

### L1. Existing raw saves

When a generated project contains legacy raw snapshots:

```text
list them
mark LEGACY_RAW
direct generated-aware load rejected
```

The original legacy buttons/behavior remain for legacy projects.

## M. Projects save card and manager

Add one compact card:

```text
Игровые сохранения
```

Rows:

```text
Слотов
Текущих
Требуют переноса
Повреждено
Последний перенос
```

Buttons:

```text
Управление сохранениями
```

Create:

```text
GeneratedGameplaySavesDialog
```

The dialog lists:

```text
slot
status
current revision short display token
saved world seed/title
current world seed/title
preserved/dropped summary when migrated
```

Technical full hashes remain outside the primary UI.

Actions:

```text
Проверить перенос
Перенести в текущий мир
```

Do not permit deleting revisions in Goal161.

## N. Save human facts and standalone

Create typed:

```text
GeneratedGameplaySavesSummary
```

Human facts after a successful current migration:

```text
Игровое сохранение        перенесено
Мир сохранения            текущий
Позиция                    сброшена на старт
Сохранено данных           <data-derived count>
Сброшено данных            <data-derived count>
Проверка после загрузки    пройдена
```

Append these facts to:

```text
Projects card
standalone HumanReviewFacts
actual payload correlation
```

Do not place the full save session in standalone payload.

The ordinary standalone build does not load a user save; it proves the current package and migration
truth are correlated.

## O. Portable copy

A complete project copy after save migration and standalone restores without execution:

```text
slot manifests/revisions validate
current migrated revision CURRENT
old source revision remains WORLD_MIGRATION_REQUIRED or CURRENT depending on copied current world
migration report valid
GeneratedWorld TRAVEL_CURRENT
AcceptedMechanics truth matches profile
RC CURRENT when profile is RC-complete
```

For core-only:

```text
save current/migration truth restores
RC remains ABSENT/LAST_SUCCESS as appropriate
no false READY claim
```


## P. Required behavioral tests

Create at least 52 Goal161 tests; at least 46 behavioral.

### Profile-neutral commit correction

1. all-selectable regeneration apply remains GREEN;
2. all-selectable history rollback remains GREEN;
3. real core-only regeneration candidate builds TRAVEL_CURRENT;
4. core-only regeneration semantic commit succeeds;
5. real core-only history rollback candidate builds TRAVEL_CURRENT;
6. core-only rollback semantic commit succeeds;
7. core-only AcceptedMechanics remains false with MissingFactKinds;
8. core-only RC never claims READY/PENDING/CURRENT;
9. custom partial selection exact sealed summary commits;
10. tampered AcceptedMechanics summary fails semantic validation;
11. tampered compatibility summary fails;
12. candidate RC-status mismatch fails and rolls back.

### Save storage and validation

13. generated save creates immutable revision and slot manifest;
14. repeated identical save deduplicates;
15. changed session creates a new revision with parent;
16. prior revision bytes remain unchanged;
17. invalid slot names/path escape rejected;
18. revision filename/hash mismatch rejected;
19. slot current pointer mismatch rejected;
20. session hash mismatch rejected;
21. definition fingerprint tamper rejected;
22. foreign project identity rejected;
23. current map/position validation enforced;
24. unresolved package definition reference rejected;
25. legacy raw snapshots remain untouched.

### Same-world exact load

26. CURRENT generated save loads exact session;
27. exact serialized session equality;
28. map/gameplay hashes equal;
29. no Runtime Start/reset occurs during load;
30. same-world package mismatch reports PACKAGE_REBASE_REQUIRED;
31. direct load of migratable/invalid save rejected.

### Cross-world/package migration

32. regeneration leaves save tree byte-identical and changes status to WORLD_MIGRATION_REQUIRED;
33. history rollback leaves save tree byte-identical;
34. migration preview is zero-write;
35. cross-world map resets to new generated start;
36. transient events/encounter/dialogue/tick reset;
37. canonically equal baseline inventory/resources/stats/progression/faction data preserved;
38. changed/missing generated items/quests/statuses dropped;
39. flag/metadata exact generated references dropped;
40. portable scalar structural IDs retained;
41. migration preview counts match actual candidate session;
42. caller-modified migration preview rejected;
43. source revision tamper after preview rejected;
44. current world/package race after preview rejected;
45. migration apply writes one immutable revision and updates manifest atomically;
46. migration failure leaves slot tree byte-identical;
47. migrated revision becomes CURRENT;
48. exact migrated load succeeds;
49. no dropped reference remains in migrated session;
50. migration object matches preserved/dropped lists.

### Runtime continuation

51. loaded migrated session executes valid movement;
52. loaded migrated session crosses a generated travel gate;
53. destination generated interaction succeeds;
54. repeat load/command sequence final hash/events equivalent.

### Historical world reuse

55. restore the original historical world after migration;
56. original source revision becomes CURRENT again;
57. migrated revision becomes WORLD_MIGRATION_REQUIRED for that restored world;
58. no revision rewrite occurs during world changes.

### UI/operation/standalone/portable

59. save operation rejects during regeneration/rollback/build/standalone;
60. world-change operation rejects during save migration;
61. Runtime Simulator generated save/list/load uses generated service;
62. Runtime Simulator legacy project uses raw snapshot store;
63. generated legacy raw snapshot is listed as LEGACY_RAW and not direct-loaded;
64. Projects save card reports current/migration/invalid counts;
65. saves dialog migration actions/statuses truthful;
66. primary UI contains no full hashes/paths/generated IDs;
67. migration facts append to standalone request;
68. exactly one hidden smoke after migration;
69. host reused/not rebuilt and Unity zero;
70. actual payload contains save-migration and accepted/travel facts;
71. all-selectable RC CURRENT after standalone;
72. core-only portable/save truth restores without false RC readiness;
73. all-selectable portable copy restores slots/revisions/migration/RC without execution.

### Regressions

74. Goal160 sealed regeneration/history rollback regressions GREEN;
75. Goal159 v1/v2 regeneration regressions GREEN;
76. Goal158 travel regressions GREEN;
77. Goal157 provenance/two-lane regressions GREEN;
78. Goal156 creation/custom-base regressions GREEN;
79. Goal155A/155 regressions GREEN;
80. Goal154D/153C/150/149 regressions GREEN;
81. RuntimeSnapshotStore legacy tests GREEN;
82. Runtime Simulator existing command behavior GREEN;
83. operation coordinator/race regressions GREEN;
84. Goal142 and goal148-manual byte-identical.

Do not claim list counts unless tests are actually discovered and executed.

## Q. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal161"
# require >=52 total and >=46 behavioral

dotnet test ... --filter "FullyQualifiedName~Goal161"
dotnet test ... --filter "FullyQualifiedName~Goal160"
dotnet test ... --filter "FullyQualifiedName~Goal159"
dotnet test ... --filter "FullyQualifiedName~Goal158"
dotnet test ... --filter "FullyQualifiedName~Goal157"
dotnet test ... --filter "FullyQualifiedName~Goal156"
dotnet test ... --filter "FullyQualifiedName~Goal155A"
dotnet test ... --filter "FullyQualifiedName~Goal155"
dotnet test ... --filter "FullyQualifiedName~Goal154D"
dotnet test ... --filter "FullyQualifiedName~Goal153C"
dotnet test ... --filter "FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronization"
dotnet test ... --filter "FullyQualifiedName~Goal149"

dotnet test ... --filter "FullyQualifiedName~RuntimeSnapshotStore"
dotnet test ... --filter "FullyQualifiedName~RuntimeSimulator"
dotnet test ... --filter "FullyQualifiedName~DefaultGameRuntime"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"
dotnet test ... --filter "FullyQualifiedName~ProjectLifecycle"
dotnet test ... --filter "FullyQualifiedName~ProjectStandaloneBuild"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleLibrary"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleCertification"
dotnet test ... --filter "FullyQualifiedName~ProceduralGameKernel"
dotnet test ... --filter "FullyQualifiedName~GeneratedPackageMvp"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Then run the real product matrix:

```text
all-selectable generated project:
  start unified Runtime
  produce nontrivial map + gameplay state
  save exact slot
  same-world load
  regenerate world
  migrate save
  load and travel/interact
  restore historical world
  verify original revision current again

core-only generated project:
  regenerate apply
  history rollback apply
  save/migrate/load
  no RC-ready claim

exactly one hidden standalone smoke after all-selectable migration
portable copies for all-selectable and core-only
```

Run artifact scope last.

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

## R. Evidence

Create exactly 14 files in each mirrored root:

```text
goal161-dashboard.json
architecture-review.json
goal160-independent-audit-finding.json
profile-neutral-commit-proof.json
generated-save-schema-store-proof.json
definition-fingerprint-proof.json
same-world-load-proof.json
cross-world-migration-proof.json
migration-runtime-continuation-proof.json
world-change-save-status-proof.json
save-ui-operation-proof.json
standalone-portability-proof.json
artifact-scope-proof.json
goal161-report.md
```

Roots:

```text
.llmgc/procedural/goal-161-profile-neutral-world-change-and-generated-gameplay-save-migration/
.llmgc/exports/goal-161-profile-neutral-world-change-and-generated-gameplay-save-migration/
```

Procedural/export twins must be byte-identical.

### Dashboard fields

```text
status
candidateStatus
goal161TestsDiscovered
goal161BehavioralTestsPassed

goal160IndependentAuditBlockerRecorded
goal160AuditBlockerClosed

allSelectableRegenerationCommitPassed
allSelectableRollbackCommitPassed
coreOnlyRegenerationCommitPassed
coreOnlyRollbackCommitPassed
coreOnlyAcceptedMechanicsIncompleteTruthPassed
customSelectionProfileNeutralPassed

saveSlotCreated
saveRevisionCount
immutableRevisionPassed
saveDedupPassed
saveSchemaValidationPassed
definitionFingerprintValidationPassed
legacyRawSnapshotPreserved

sameWorldExactLoadPassed
sameWorldSessionHashExact
packageRebaseStatusPassed
worldMigrationStatusPassed

sourceRevisionSha256
migratedRevisionSha256
sourceWorldId
targetWorldId
mapResetPassed
preservedReferenceCount
droppedReferenceCount
migrationPreviewPassed
migrationAtomicApplyPassed
sourceRevisionImmutable
migratedRevisionCurrent
postMigrationRuntimeMovePassed
postMigrationTravelPassed
postMigrationDestinationInteractionPassed
postMigrationReplayEquivalent

originalWorldRestored
originalRevisionCurrentAgain
saveTreeUnchangedDuringWorldChanges

operationLeaseSaveRacePassed
runtimeSimulatorGeneratedWorkflowPassed
legacyRuntimeSnapshotWorkflowPassed
projectsSaveCardPassed

hostCacheKey
hostReused
hostRebuilt
hostFileSetHashUnchanged
unityProcessStartCount
hiddenSmokeInvocationCount
hiddenSmokePassed
standaloneSelfChecksPassed
actualPayloadSaveMigrationFactsPassed
actualPayloadAcceptedFactsPassed
allSelectableReleaseCandidateCurrent
coreOnlyNoFalseRcReady
portableAllSelectablePassed
portableCoreOnlyPassed

goal160RegressionPassed
goal159RegressionPassed
goal158RegressionPassed
goal157RegressionPassed
goal156RegressionPassed
goal155aRegressionPassed
goal155RegressionPassed
goal154dRegressionPassed
goal153cRegressionPassed
goal150RegressionPassed
goal149RegressionPassed
legacySnapshotRegressionPassed
goal142SourceByteIdentical
sourceGoal148ByteIdentical

artifactScopeViolationCount
goal161Accepted=false
goal161ManualReviewRequired=false
goal161IndependentAuditRequired=true
```

No required GREEN field may be null/PARTIAL/NOT_EXECUTED/unverified constant.


## S. State and docs

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
docs/manual-acceptance/goal160-generated-world-history-rollback.md
```

Create:

```text
docs/manual-acceptance/goal161-generated-gameplay-save-migration.md
```

No human gate.

Required GREEN state:

```text
goal160IndependentAuditResult=BLOCKED_AT_D8DD05E7
goal160IndependentAuditBlocker=semantic_commit_validator_requires_complete_accepted_mechanics_for_core_only_generated_projects
goal160AuditBlocker=closed_by_goal161

goal160ImplementationStatus=GREEN
goal160CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal160Accepted=false
goal160IndependentAuditRequired=false

goal161ImplementationStatus=GREEN
goal161CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal161Accepted=false
goal161AcceptedByHuman=false
goal161AcceptedByCodex=false
goal161ManualReviewRequired=false
goal161ManualGateReady=false
goal161IndependentAuditRequired=true

goal161ProfileNeutralWorldChangePassed=true
goal161CoreOnlyRegenerationPassed=true
goal161CoreOnlyRollbackPassed=true
goal161GeneratedSaveStorePassed=true
goal161SameWorldLoadPassed=true
goal161CrossWorldMigrationPassed=true
goal161MigrationRuntimeContinuationPassed=true
goal161SaveUiPassed=true
goal161HostReused=true
goal161HostRebuilt=false
goal161UnityProcessStartCount=0
goal161HiddenSmokeInvocationCount=1
goal161PortableAllSelectablePassed=true
goal161PortableCoreOnlyPassed=true
goal161ArtifactScopeViolationCount=0

nextAction=independent_goal161_audit_and_plan_player_driven_generated_campaign_session
```

Release risk statement:

```text
Generated gameplay saves are now bound to exact world/package/build truth. Same-world loads are exact;
world or package changes require an explicit migration that resets location/transient state and
preserves only canonically compatible definitions. Original revisions remain immutable. Gameplay
campaign UX beyond Runtime Simulator remains future work.
```

Record that raw legacy Runtime snapshots remain intentionally unverified for generated worlds.

## T. Text integrity

Scan actual changed/task/evidence/docs files:

```text
valid UTF-8
no NUL
no forbidden C0 except CR/LF/TAB
no mojibake markers
no escaped Cyrillic where policy forbids
no absolute disposable/source paths in committed evidence
```

Historical evidence remains immutable.

## U. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal161-generated-save-migration.ps1
.devflow/scripts/run-goal161-generated-save-migration.cmd

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectOperationCoordinator.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectReleaseCandidateRecordService.cs

src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationModels.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationCandidateSealService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationCommitValidator.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectGeneratedWorldRollbackService.cs

src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplaySaveModels.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplayDefinitionFingerprintService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplaySaveStore.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplaySaveValidator.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplaySaveMigrationService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplaySaveService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplaySavesSummaryService.cs

src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.WinForms/Pages/RuntimeSimulator/RuntimeSimulatorPageControl.cs
src/LLMGameCreator.WinForms/Pages/RuntimeSimulator/RuntimeSimulatorPageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/Projects/GeneratedGameplaySavesDialog.cs
src/LLMGameCreator.WinForms/Pages/Projects/GeneratedGameplaySavesDialog.Designer.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/Goal161/Goal161ProfileNeutralCommitTests.cs
tests/LLMGameCreator.Tests/Application/Goal161/Goal161GeneratedSaveStoreTests.cs
tests/LLMGameCreator.Tests/Application/Goal161/Goal161SameWorldLoadTests.cs
tests/LLMGameCreator.Tests/Application/Goal161/Goal161CrossWorldMigrationTests.cs
tests/LLMGameCreator.Tests/Application/Goal161/Goal161MigrationRuntimeContinuationTests.cs
tests/LLMGameCreator.Tests/Application/Goal161/Goal161SaveUiOperationTests.cs
tests/LLMGameCreator.Tests/Application/Goal161/Goal161StandaloneAndPortabilityTests.cs
tests/LLMGameCreator.Tests/Application/Goal160/Goal160SemanticCommitRollbackTests.cs
tests/LLMGameCreator.Tests/Application/Goal160/Goal160WorldRollbackCandidateTests.cs
tests/LLMGameCreator.Tests/RuntimeSnapshotStoreTests.cs
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
docs/manual-acceptance/goal160-generated-world-history-rollback.md
docs/manual-acceptance/goal161-generated-gameplay-save-migration.md

docs/agent-tasks/goal-161-profile-neutral-world-change-and-generated-gameplay-save-migration/
.llmgc/procedural/goal-161-profile-neutral-world-change-and-generated-gameplay-save-migration/
.llmgc/exports/goal-161-profile-neutral-world-change-and-generated-gameplay-save-migration/
```

If a concrete compile/test failure proves one additional existing Application/WinForms/test path is
required, record the exact reason and add only that path.

Forbidden without a newly reproduced Goal161 P0/P1:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
catalogs/feature-modules/**
unity/**
samples/**
generator-library/**
provider/**
LLM/**
RAG/**
```

Use existing Runtime state/session/serializer/snapshot contracts. Do not modify Runtime schemas.

## V. Command budget

```text
read-first/architecture review: 14 minutes
profile-neutral seal/commit fix: 14 minutes
save schema/store/definition inventory: 24 minutes
same-world load and validation: 16 minutes
cross-world migration engine: 28 minutes
Runtime continuation and world-change integration: 20 minutes
WinForms/controller/standalone integration: 20 minutes
behavioral tests: 34 minutes
real all-selectable/core-only matrix + one smoke: 16 minutes
regressions/evidence/docs/artifact scope: 20 minutes
target wall clock: 165 minutes
maximum two concurrent testhost processes
Unity process count: 0
```

Rules:

```text
write complete test inventory before production edits
write publication/evidence script before long external proof
no unchanged command repetition
no timeout escalation
after failure run only exact class/test
P0/P1 fixed inside Goal161
P2/P3 debt only
do not defer evidence/docs/artifact scope
```

## W. Publication

Create exactly one final commit:

```text
GREEN Goal 161 profile neutral world change and generated gameplay save migration
```

or honest BLOCKED/FAILED.

Codex performs standard push.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three Goal161 task files tracked
Goal142 and goal148-manual unchanged
Unity starts=0
HostRebuilt=false
hidden smoke=1 only on GREEN
Goal154 acceptance unchanged
Goal155 RC milestone passed
Goal160 accepted=false/no human gate
Goal161 accepted=false/no human gate
```

## X. GREEN criteria

```text
Goal160 P1 recorded and closed
Goal161 tests >=52 / behavioral >=46 / all pass

all-selectable regeneration/rollback remain GREEN
core-only regeneration/rollback now commit GREEN
custom partial selection profile-neutral truth passes
candidate seal/semantic validator exact-summary correlation

generated save schema/store immutable revisions
definition fingerprint validation
exact same-world load
stale world/package statuses truthful
controlled migration preview/apply
map/transient reset truthful
compatible baseline/FeatureModule state preserved
incompatible generated state dropped and reported
source revision immutable
migrated revision current
post-load movement/travel/destination interaction/replay GREEN
original revision current again after exact historical world restore

operation lease prevents save/world-change races
Runtime Simulator generated-aware flow
legacy raw snapshot behavior unchanged
Projects save card/dialog truthful
one cached hidden standalone smoke
HostReused=true / HostRebuilt=false / Unity=0
payload save-migration/travel/accepted facts correlate
all-selectable RC current
core-only no false RC readiness
portable all-selectable/core-only truth

Goal160/159/158/157/156/155A/155/154D/153C/150/149 and legacy snapshot regressions GREEN
14+14 evidence mirrored
text integrity GREEN
artifact scope 0
goal161CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
one final commit pushed
```

## Y. Final report

Return GREEN/BLOCKED/FAILED and include:

- model/reasoning;
- architecture review;
- exact Goal160 P1 reproduction;
- all-selectable/core-only/custom commit results;
- discovered/behavioral test counts;
- save schema/slot/revision facts;
- current save status and exact-load proof;
- source/target world/package hashes;
- preserved/dropped migration counts and reasons;
- map/transient reset;
- migrated revision/load/Runtime continuation/replay;
- historical-world original revision reuse;
- operation race matrix;
- Runtime Simulator and Projects UI;
- host/Unity/smoke;
- actual payload/RC/portable results;
- regressions;
- source/baseline immutability;
- evidence/text/artifact scope;
- state/no-human-gate;
- final SHA/push/HEAD/worktree.
