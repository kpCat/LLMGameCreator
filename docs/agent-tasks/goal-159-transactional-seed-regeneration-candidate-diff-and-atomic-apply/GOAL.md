# Goal 159 — Transactional Seed Regeneration, Candidate Diff & Atomic Apply

## Identity

- Task ID: `goal-159-transactional-seed-regeneration-candidate-diff-and-atomic-apply`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `9a350c63829e699ce24bbd5ef33611c6c8d79537`
- Required base message: `GREEN Goal 158 generated region travel Runtime and standalone vertical slice`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Sol
Reasoning effort: Extra High
```

Reason: this is a major project-lifecycle and saved-source migration vertical slice. It introduces
safe regeneration of an existing generated game while preserving project identity, FeatureModule
authoring, accepted mechanics, travel qualification, history and rollback truth. It also resolves the
v1 preset/request ambiguity through a backward-compatible internal source v2. This is not a simple
“rewrite the seed files” task.

## Pre-approval

- The owner approved execution by launching this task.
- Produce a concise internal implementation plan; do not ask for confirmation.
- Do not request intermediate or final manual testing.
- Own all P0/P1 defects reproduced by the mandatory Goal159 matrix inside this Goal.
- Record P2/P3 debt without creating Goal159A.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No candidate/intermediate commits.
- Codex performs commit and standard push itself.

## Expected initial worktree

After unpacking this ZIP, only these untracked files are permitted:

```text
docs/agent-tasks/goal-159-transactional-seed-regeneration-candidate-diff-and-atomic-apply/GOAL.md
docs/agent-tasks/goal-159-transactional-seed-regeneration-candidate-diff-and-atomic-apply/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-159-transactional-seed-regeneration-candidate-diff-and-atomic-apply/README.md
```

Require:

```text
HEAD == origin/main == 9a350c63829e699ce24bbd5ef33611c6c8d79537
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

The regeneration operation itself does not run standalone. The single smoke is a final automated
proof on the successfully regenerated project.

## Goal158 independent-audit intake

Record:

```text
goal158IndependentAuditResult=GREEN_ACCEPTABLE_CANDIDATE_AT_9A350C63
goal158IndependentAuditPassed=true
goal158IndependentAuditRequired=false
```

Audited truths:

```text
generic Interact-driven map transition is backward compatible and atomic on failure
MapChanged=8 is additive without enum renumbering
strict plan region/map/connection bindings are data-derived
one deterministic safe gate exists per directed connection
travel overlay contains only authorized package deltas
real Runtime visits at least two generated maps/regions
origin/gate/destination interactions correlate
route replay and map-state roundtrip pass
Lane A accepted mechanics and Lane B travel authority remain separated
history/UI/standalone/RC/portable truth is correlated
13×2 evidence and all three task files are committed
```

Goal158 remains accepted=false and creates no human gate.

### Nonblocking independent-audit P2 to close here

A genuine Goal157 `unified_game_project_build_history_v2` row can still carry
`GeneratedWorld.Status=BUILD_CURRENT`. The UI renders this as start-only truth, but the typed status
is not normalized to `START_CURRENT` when its primary final hash equals the Goal157 activation hash.

Goal159 must normalize every eligible v2 generated row to:

```text
START_CURRENT
```

and prove that no v2 row can expose `TRAVEL_CURRENT`.

This is a bounded history-reader correction inside the next major Goal, not a separate hotfix.

## Product problem

Generated projects can now:

```text
create
build
start in generated content
interact
travel between regions
save/reopen
build cached standalone
```

But changing the seed/mode/preset means creating a new project.

The existing project's identity, mechanics and parameters cannot be retained while replacing only its
generated world safely.

A naïve regeneration would risk:

```text
partial sidecar replacement
package/source mismatch
lost mechanic selections or parameter overrides
old history falsely matching a new world
old RC falsely remaining CURRENT
failed candidate destroying the current playable project
concurrent authoring changes being overwritten
```

## Goal159 product outcome

In the normal project workspace:

```text
Сгенерированный мир
→ Перегенерировать мир
→ choose seed / mode / preset
→ Проверить и применить
```

The operation performs:

```text
capture current truth
→ build complete isolated candidate project
→ strict v2 provenance
→ current FeatureModules and parameters
→ accepted-mechanics compatibility
→ generated start and region travel
→ candidate diff
→ final validation
→ optimistic concurrency recheck
→ rollback-safe atomic apply
```

On success:

```text
project identity unchanged
mechanic selections/parameters unchanged
new generated source current
new travel build TRAVEL_CURRENT
old RC retained as LAST_SUCCESS
overall RC state BUILD_GREEN_STANDALONE_PENDING
```

After the ordinary Windows standalone build:

```text
new RC CURRENT
```

On any failure, the original project remains byte-identical except for a causal failed-attempt record
that is written only outside the authoritative project state or in an explicitly non-authoritative
diagnostic log.

## Non-goals

Do not add:

```text
new Runtime primitives
new FeatureModules
new public GamePackage fields
Unity project/host changes
automatic seed randomization
world merge between old and new seeds
save-game migration between generated worlds
undo/redo UI for historical regenerations
provider/LLM/Lua/media generation
infinite streaming
clean-machine release packaging
```

Persistent one-click rollback to an earlier successful world may be a later product slice. Goal159
guarantees failure rollback, not user-selected historical rollback.


## Mandatory architecture review

Read at most 18 primary files:

```text
AGENTS.md
docs/GOAL_DESIGN_QUALITY_POLICY.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/UNITY_EXECUTION_POLICY.md
SeededGeneratedProjectModels.cs
SeededGeneratedProjectSourceService.cs
SeededGeneratedGameProjectCreationService.cs
GenerationPresetOptionsService.cs
GeneratedProjectOverlayService.cs
GameProjectBuildAndQualificationService.cs
GameProjectBuildTransaction.cs
GameProjectFeatureModuleAuthoringService.cs
GameProjectBuildHistoryReader.cs
GameProjectReleaseCandidateRecordService.cs
UnifiedGameProjectWorkspaceController.cs
GameProjectGeneratedWorldSummaryService.cs
CreateGameDialog.cs
ProjectsPageControl.cs
```

Before production edits write:

```text
.llmgc/procedural/goal-159-transactional-seed-regeneration-candidate-diff-and-atomic-apply/architecture-review.json
```

Required resolved sections:

```text
goal158IndependentAudit
v1SourceCompatibility
v2RequestResolutionContract
sharedGenerationArtifactFactory
candidateProjectIsolation
authoringAndIdentityPreservation
optimisticConcurrencyContract
candidateQualificationMatrix
generatedWorldDiffContract
atomicApplyJournal
packageSupportHistoryCommit
releaseCandidateTransition
failureRollbackMatrix
uiWorkflow
portableCopy
legacyCompatibility
nonGoals
```

Each section names concrete types, input/output files, commit order, rollback order and behavioral
tests. Empty or vague sections block GREEN.

## A. Backward-compatible generated source v2

Add:

```text
seeded_generated_project_source_v2
```

Do not remove v1 support.

### A1. Typed request and resolution

Create:

```text
SeededGeneratedProjectGenerationRequest
  Seed
  Mode
  PresetId
  CompactStyleHintIds[]
  SelectedVariantIds[]

SeededGeneratedProjectResolvedOptions
  Seed
  Mode
  PresetId
  CompactStyleHintIds[]
  SelectedVariantIds[]
  StableSummary
  PresetDefinitionSha256
  StyleOverridesApplied
  VariantOverridesApplied
```

`CompactStyleHintIds` and `SelectedVariantIds` in the request are overrides:

```text
empty -> preset/default values are resolved
nonempty -> exact explicit overrides are resolved
```

`PresetDefinitionSha256` is a deterministic hash of the exact current preset definition:

```text
presetId
title
sorted default style hints
```

No timestamp/path.

### A2. V2 source record

The v2 record contains:

```text
schemaVersion
creationKind
generationRequest
resolvedGenerationOptions
mechanicsProfileId
existing plan/rule/tiny/MVP/overlay/base/provenance/count fields
```

Do not duplicate the old top-level seed/mode/preset/style/variant fields in v2.

Exact properties are validated by schema version.

### A3. V1 reading

For v1:

```text
GenerationRequest is inferred from the persisted effective values
ResolvedGenerationOptions equals the persisted effective values
PresetDefinitionSha256 is calculated from the current matching preset
RequestOrigin=legacy_v1_effective_options
```

The existing strict Goal157 chain still validates.

V1 projects are not rewritten on ordinary open/build.

### A4. V2 writing

New generated projects and successful regenerations write v2.

The validator:

1. validates exact v2 properties;
2. resolves `generationRequest`;
3. requires byte/semantic equality with `resolvedGenerationOptions`;
4. verifies preset definition hash;
5. regenerates the full Goal157 chain;
6. verifies direct and sidecar hashes.

Required failures:

```text
generated_source.v2_request_resolution_mismatch
generated_source.preset_definition_mismatch
generated_source.request_options_mismatch
generated_source.resolved_options_mismatch
```

### A5. Preset causality

For a v2 request with no style override:

```text
changing PresetId changes resolved style hints and candidate request/source hash
```

For explicit style overrides:

```text
the record truthfully states StyleOverridesApplied=true
the explicit resolved style list is authoritative
the preset remains descriptive/default metadata, not falsely claimed as the sole content cause
```

This closes the Goal157/158 preset-label ambiguity without pretending an override-free world.

## B. Shared deterministic generation artifact factory

Refactor the current creation orchestration into a reusable service, naming flexible:

```text
SeededGeneratedProjectArtifactFactory
```

Input:

```text
canonical baseline provider
v2 generation request
mechanicsProfileId metadata
output directory
```

Output:

```text
resolved options
plan
rule pack
tiny loop
namespaced MVP
overlay
generated base
v2 source record
exact deterministic sidecar byte set
hashes/counts/diagnostics
```

The factory does not:

```text
create project identity
initialize FeatureModule authoring
activate package
write build history
write RC record
```

Both new-project creation and regeneration use the same factory.

### B1. Creation compatibility

Existing `seeded_generated` creation switches to v2 source output while preserving:

```text
same requested package identity
same deterministic gameplay output for the same effective options
same all_selectable_defaults/core_only authoring
same atomic folder creation
```

V1 fixture projects remain readable.

Legacy template creation remains byte-compatible.

## C. Regeneration request and preconditions

Create:

```text
GameProjectSeedRegenerationRequest
```

Fields:

```text
ProjectFolder
GenerationRequest
ExpectedSourceRecordSha256
ExpectedQualifiedAuthoringFingerprint
ExpectedAuthoringRevision
ExpectedActivatedPackageSha256
ExpectedCompositionPackageSha256
ExpectedFinalStateHash
ExpectedProjectIdentityFingerprint
ExpectedReleaseCandidateRecordSha256 optional
```

The UI/controller obtains these from the current snapshot immediately before opening the dialog.

### C1. Valid project

Regeneration is allowed only when:

```text
generated source Present=true and Passed=true
project identity valid
authoring validation/fingerprint valid
no normal build/standalone/regeneration currently running
current package bytes match authoring document activated hash
current history truth is START_CURRENT, TRAVEL_CURRENT or LAST_SUCCESS
```

A source-only project that has never built may regenerate, but candidate qualification must complete
before apply.

Legacy/template projects reject:

```text
regeneration.not_generated_project
```

### C2. Semantic no-op

Reject before candidate creation when the resolved new request equals the current resolved options:

```text
regeneration.no_semantic_change
```

No files or histories change.

### C3. Optimistic concurrency

Immediately before candidate generation and again immediately before apply, require every expected
token to still match actual project state.

Failures:

```text
regeneration.source_changed
regeneration.authoring_changed
regeneration.package_changed
regeneration.identity_changed
regeneration.release_candidate_changed
regeneration.concurrent_operation
```

Do not silently apply against a newer user edit/build.

## D. Isolated candidate project

Create a short disposable candidate root under:

```text
%LOCALAPPDATA%\LLMGameCreator\RegenerationCandidates\<attempt-id>\
```

Never generate inside the authoritative project folder before apply.

### D1. Candidate clone

Copy the complete authoritative project into the candidate root, excluding only:

```text
build staging/temp directories
standalone output directories
transient lock files
```

Preserve:

```text
project identity
FeatureModule authoring document
parameter values
catalog/module fingerprints
history
support files
old RC record for truth testing
```

### D2. Candidate world replacement

Inside the candidate only:

```text
replace .llmgc/generation with the factory v2 artifact set
replace package.json with candidate generated base + current project identity as the prebuild package
clear only current qualified-build hashes in candidate authoring as required to prevent old-history matching
do not change selected modules or parameter values
```

Old build histories may remain in the candidate but must not match the new source/package.

### D3. Actual qualification

Run the real modern workflow against the candidate:

```text
GameProjectBuildAndQualificationService
Lane A accepted-mechanics compatibility
Lane B generated start
Goal158 travel overlay and full route
history write
fresh controller reopen
```

Require:

```text
build GREEN
generated source CURRENT
GeneratedWorld TRAVEL_CURRENT
GeneratedWorldActivation Passed
GeneratedRegionTravel Passed
authoring fingerprint unchanged from original
selected module IDs exact
parameter JSON exact
project identity exact
accepted mechanics behavior appropriate to current selection
repeat candidate build deterministic
fresh reopen current
```

Do not use a test-only fake builder for product qualification.

### D4. Candidate RC truth

Before standalone:

```text
old copied RC record must be LAST_SUCCESS or ABSENT
overall candidate RC status BUILD_GREEN_STANDALONE_PENDING when AcceptedMechanics complete
```

It must never be CURRENT for the new candidate package.

## E. Typed world diff

Create:

```text
GameProjectSeedRegenerationDiff
```

Compare current strict source/package with candidate strict source/package.

Fields:

```text
OldSeed
NewSeed
OldMode
NewMode
OldPresetId
NewPresetId
OldSourceRequestSha256
NewSourceRequestSha256
OldPlanSha256
NewPlanSha256
OldOverlaySha256
NewOverlaySha256
OldGeneratedBaseSha256
NewGeneratedBaseSha256

OldCounts
NewCounts

AddedRecordCount
RemovedRecordCount
ChangedRecordCount
UnchangedRecordCount

AddedByCollection
RemovedByCollection
ChangedByCollection

OldStartRegionTitle
NewStartRegionTitle
OldTravelDestinationTitle
NewTravelDestinationTitle

GameplayChanged
AuthoringPreserved
ProjectIdentityPreserved
Diagnostics
```

Record comparison is data-derived from canonical generated record fingerprints.

No fixed expected content count.

### E1. Meaningful variation

For a changed seed/mode/preset request require:

```text
NewSourceRequestSha256 differs
at least one of plan/overlay/base differs
GameplayChanged=true
at least one added/removed/changed generated record or generated map fingerprint
```

A request that resolves differently but produces no gameplay difference is rejected unless the only
change is an explicitly requested metadata-only override and that status is clearly represented.

Default Goal159 UI does not offer metadata-only regeneration.

## F. Regeneration preview/result

Create:

```text
GameProjectSeedRegenerationPreview
GameProjectSeedRegenerationResult
```

Preview/result include:

```text
AttemptId
Status
Stage
CurrentSourceSummary
CandidateSourceSummary
Diff
CandidateBuild
CandidateSnapshot
ExpectedTruthTokens
Diagnostics
Applied
RollbackApplied
AuthoritativeFilesChanged
```

No absolute paths in user-facing facts.

Human facts:

```text
Seed
Режим
Пресет
Регионы
Фракции
Персонажи
Столкновения
Задания и события
Изменено записей
Механики и параметры
Игровой маршрут
```


## G. Rollback-safe atomic apply

Create:

```text
GameProjectSeedRegenerationTransaction
```

The transaction owns exact snapshots/hashes of authoritative paths before mutation.

### G1. Authoritative paths

At minimum:

```text
package.json
.llmgc/generation/
.llmgc/authoring/
.llmgc/project-identity.json or current identity path
.llmgc/build-history/
required activated support files that differ
```

The existing RC record is deliberately preserved unchanged.

Transient staging/lock/diagnostic files are not authoritative.

### G2. Journal

Before any authoritative mutation write a confined transaction journal:

```text
.llmgc/regeneration/transactions/<attempt-id>/journal.json
```

Schema:

```text
seed_regeneration_transaction_v1
attemptId
state=prepared|applying|committed|rolling_back|rolled_back
authoritativeRelativePaths[]
beforeSha256 map
candidateSha256 map
appliedStepIds[]
```

No timestamps are required for identity. An operational started/completed time is allowed but excluded
from deterministic hashes.

Journal writes are atomic.

### G3. Apply order

Required order:

1. acquire in-process regeneration gate and project lock file;
2. recheck optimistic concurrency;
3. create backups under the transaction root;
4. set journal `applying`;
5. atomically swap `.llmgc/generation` directory;
6. atomically replace support files that changed;
7. atomically replace `package.json`;
8. atomically replace authoring/identity documents;
9. copy exactly one new GREEN candidate build-history entry under a collision-free final name;
10. write last-success regeneration record;
11. validate authoritative project from disk;
12. set journal `committed`;
13. remove temporary backups and lock.

Do not copy the candidate's old copied history set back to the authoritative project.

### G4. Failure rollback

On any failure after step 4:

```text
journal state=rolling_back
restore every changed authoritative file/directory from backup in reverse order
remove newly added history/record files
validate before hashes
journal state=rolled_back
release lock
```

Return:

```text
Applied=false
RollbackApplied=true
```

If rollback cannot restore exact hashes, publish FAILED/BLOCKED; do not claim safety.

### G5. Crash recovery

On opening a project or starting regeneration:

```text
detect nonterminal journal
if before backups are complete, restore before state
if journal committed and current hashes equal candidate hashes, finalize cleanup
otherwise reject project mutation and show regeneration.recovery_required
```

Automated tests simulate process interruption at multiple apply steps.

Do not delete evidence before recovery decision.

## H. Candidate-to-authoritative truth

After successful apply require:

```text
strict source v2 CURRENT
actual package hash equals candidate activated package hash
document package/composition/final hashes equal candidate build
project identity exact and unchanged
selected modules exact and unchanged
parameter values byte/semantic exact and unchanged
catalog/module fingerprints unchanged
GeneratedWorld TRAVEL_CURRENT
GeneratedWorldActivation Passed
GeneratedRegionTravel Passed
AcceptedMechanics/Compatibility match candidate
new build history selected
old histories preserved
old RC record retained but LAST_SUCCESS
overall RC status BUILD_GREEN_STANDALONE_PENDING when accepted mechanics complete
```

Do not copy candidate absolute paths into history or records.

### H1. Regeneration record

Write:

```text
.llmgc/regeneration/last-successful-regeneration.json
```

Schema:

```text
seed_regeneration_result_v1
attemptId
status=GREEN
oldSourceRecordSha256
newSourceRecordSha256
oldRequestSha256
newRequestSha256
oldPlanSha256
newPlanSha256
oldOverlaySha256
newOverlaySha256
oldGeneratedBaseSha256
newGeneratedBaseSha256
oldPackageSha256
newPackageSha256
newCompositionPackageSha256
newFinalStateHash
qualifiedAuthoringFingerprint
selectedModuleCount
configuredParameterCount
diff
candidateBuildHistoryFileName
previousReleaseCandidateRecordSha256
previousReleaseCandidateStatus=LAST_SUCCESS|ABSENT
```

No absolute paths.

Reader validates record against current source/document/package/history.

A failed attempt never replaces the last successful record.

## I. Workspace/controller integration

Extend `IUnifiedGameProjectWorkspaceController` additively:

```text
CanRegenerateGeneratedWorld
PreviewGeneratedWorldRegeneration(...)
ApplyGeneratedWorldRegeneration(...)
RegenerationRunning
LastRegenerationAttempt
```

Naming may vary.

### I1. Snapshot

Add typed:

```text
RegenerationAvailable
CurrentGenerationRequest
CurrentResolvedGenerationOptions
LastSuccessfulRegeneration
LastRegenerationAttempt
```

Generated source v1 is projected through the inferred typed request.
Legacy projects expose regeneration unavailable.

### I2. Concurrency

Normal build, standalone and regeneration are mutually exclusive.

Attempts during another operation return causal rejection without waiting indefinitely.

### I3. Open/recovery

`OpenProject()` performs regeneration-journal recovery before reading package/authoring/history.

Recovery diagnostics are visible in Technical Details.

## J. WinForms workflow

### J1. Button

Add one ordinary Projects-page button:

```text
Перегенерировать мир
```

Visible/enabled only for a valid generated project and when no build/standalone/regeneration is
running.

No new top-level page.

### J2. Dialog

Create:

```text
RegenerateGeneratedWorldDialog
```

Russian controls:

```text
Текущий seed
Новый seed
Режим генерации
Пресет
Дополнительные стили (advanced optional)
Дополнительные варианты (advanced optional)
```

Initial values:

```text
current source request
new seed defaults to current seed
mode/preset current
advanced overrides reflect current request
```

No random/time-based seed.

`Проверить и применить` is disabled until the request is semantically different.

Mode and preset lists are data-derived.

Advanced IDs are edited as a deterministic newline list:

```text
trim
remove blanks
distinct ordinal
sort ordinal
```

Validation messages are Russian and causal.

### J3. Execution

On confirm:

```text
show busy state
run complete candidate qualification and atomic apply
refresh/open same project
show result
```

Do not ask the user to inspect raw files.

### J4. Result card

Extend the existing `Сгенерированный мир` card or add one compact subordinate section:

```text
Последняя перегенерация
```

Rows:

```text
Seed                     old → new
Режим                    old → new when changed
Пресет                   old → new when changed
Регионы                  old → new
Фракции                  old → new
Персонажи                old → new
Столкновения             old → new
Задания и события        old → new
Изменено записей
Механики и параметры     сохранены
Маршрут                  проверен
Windows standalone       требуется повторная проверка
```

No IDs/hashes/paths.

After a successful standalone on the new world:

```text
Windows standalone       подтверждён
```

### J5. Technical details

Include:

```text
regeneration attempt/status/stage
old/new request/plan/overlay/base hashes
added/removed/changed counts by collection
transaction journal status
new build-history filename
old RC status and new RC status
```

## K. Standalone and RC after regeneration

Regeneration does not automatically run standalone.

Immediately after apply:

```text
old RC record exists unchanged
RC record configuration status=LAST_SUCCESS
overall RC status=BUILD_GREEN_STANDALONE_PENDING
```

Run the ordinary `BuildWindowsStandalone()` exactly once in automated proof.

Require:

```text
candidate build repeats deterministically
HostReused=true
HostRebuilt=false
Unity starts=0
hidden smoke passed
actual payload new seed/generated/travel facts
actual payload accepted-mechanics facts
payload package/composition/final hashes match regenerated build
new RC record CURRENT
regeneration result card says standalone confirmed
```

## L. Portable copy

Copy the complete regenerated project after standalone.

Without build/Runtime/Unity:

```text
v2 source CURRENT
last regeneration record valid
GeneratedWorld TRAVEL_CURRENT
activation/travel Passed
AcceptedMechanics compatibility restored
RC CURRENT
last regeneration human facts restored
```

No absolute path dependency.

## M. Legacy and v1 behavior

### M1. Legacy template

```text
regeneration unavailable
normal build/hash behavior unchanged
no v2 source or regeneration files
```

### M2. V1 generated project

```text
ordinary open/build remains valid
regeneration dialog shows inferred request
successful regeneration upgrades only that project to v2
mechanics/parameters/identity preserved
```

### M3. New generated project

New creation writes v2 and remains behaviorally equivalent for the same effective generation options.

## N. History normalization P2

Correct `GameProjectBuildHistoryReader`:

For every eligible v2 history row:

```text
GeneratedWorld.Status becomes START_CURRENT
GeneratedRegionTravel remains absent
never TRAVEL_CURRENT
```

This is independent of whether `entry.FinalStateHash == activation.FinalStateHash`.

Add a regression using a genuine Goal157-shaped v2 row, not a v3 row with fields removed.

Historical JSON files are not rewritten.


## O. Required behavioral tests

Create at least 50 Goal159 tests; at least 44 behavioral.

### V2 source and creation

1. new generated creation writes exact v2 source;
2. same effective options preserve Goal158 gameplay output;
3. v2 request resolves exact options;
4. preset definition hash validated;
5. no style override uses preset defaults;
6. explicit style override is represented truthfully;
7. explicit variant override is represented truthfully;
8. v2 request/resolution mismatch fails;
9. preset definition mismatch fails;
10. valid v1 source remains readable without rewrite;
11. v1 inferred request is exposed;
12. v1 successful regeneration writes v2;
13. template lane remains byte-compatible.

### Preconditions and no-op

14. legacy project rejects regeneration;
15. invalid generated source rejects;
16. semantic no-op changes nothing;
17. source token mismatch rejects;
18. authoring fingerprint/revision mismatch rejects;
19. package/document hash mismatch rejects;
20. identity mismatch rejects;
21. RC token mismatch rejects;
22. concurrent build/standalone/regeneration rejects atomically.

### Candidate isolation

23. candidate root is outside authoritative project;
24. transient/staging directories excluded from clone;
25. candidate source v2 strict validation passes;
26. candidate selected modules equal original;
27. candidate parameter JSON equals original;
28. candidate identity equals original;
29. candidate Lane A accepted compatibility passes when current selection is complete;
30. candidate travel build passes;
31. candidate repeat deterministic;
32. candidate fresh reopen TRAVEL_CURRENT;
33. copied old RC becomes LAST_SUCCESS/ABSENT, never CURRENT;
34. candidate failure changes no authoritative file.

### Diff

35. different seed produces meaningful gameplay diff;
36. changed mode produces meaningful diff;
37. changed preset without overrides changes resolved options/diff;
38. added/removed/changed counts match canonical record comparison;
39. unchanged mechanics/identity reflected in diff;
40. no fixed content count assumed;
41. same semantic request rejected as no-op.

### Transaction and rollback

42. successful apply replaces source/package/document exactly with candidate;
43. only one new history entry added;
44. old histories retained;
45. old RC bytes retained;
46. old RC status LAST_SUCCESS after apply;
47. apply journal reaches committed;
48. failure after generation directory swap rolls back exact hashes;
49. failure after package replace rolls back exact hashes;
50. failure after authoring replace rolls back exact hashes;
51. failure after history add rolls back and removes new history;
52. failure before final validation rolls back;
53. rollback validates all before hashes;
54. nonterminal prepared journal recovery;
55. nonterminal applying journal recovery;
56. committed journal cleanup recovery;
57. incomplete backup yields recovery-required without mutation;
58. stale optimistic token detected at final recheck.

### Workspace/UI

59. controller exposes regeneration only for generated project;
60. dialog preloads current request;
61. default seed is current and no-op apply disabled;
62. mode/preset choices data-derived;
63. advanced ID normalization deterministic;
64. successful apply refreshes same project;
65. result card shows old→new and preserved mechanics;
66. result card shows standalone pending;
67. no IDs/hashes/paths in result card;
68. technical details show transaction/diff hashes;
69. failed attempt does not replace last successful regeneration result.

### History/RC/standalone/portable

70. genuine Goal157 v2 row normalizes to START_CURRENT;
71. v2 row never TRAVEL_CURRENT;
72. regenerated project fresh reopen TRAVEL_CURRENT;
73. regenerated build current + old RC gives BUILD_GREEN_STANDALONE_PENDING;
74. exactly one real hidden smoke after regeneration;
75. host reused/not rebuilt and Unity zero;
76. payload contains new source/travel facts;
77. payload contains accepted-mechanics facts;
78. payload hashes match regenerated build;
79. new RC CURRENT after standalone;
80. portable copy restores v2/regeneration/travel/accepted/RC without execution.

### Regressions

81. Goal158 travel/Runtime transition regressions GREEN;
82. Goal157 provenance/two-lane regressions GREEN;
83. Goal156 creation/custom-base regressions GREEN;
84. Goal155A/155 regressions GREEN;
85. Goal154D/153C/150/149 regressions GREEN;
86. Runtime map transition/legacy interactions GREEN;
87. procedural generation/preview regressions GREEN;
88. Goal142 and goal148-manual byte-identical.

Do not claim list counts unless tests are discovered and executed.

## P. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal159"
# require >=50 total and >=44 behavioral

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

dotnet test ... --filter "FullyQualifiedName~DefaultGameRuntime"
dotnet test ... --filter "FullyQualifiedName~ProceduralGameKernel"
dotnet test ... --filter "FullyQualifiedName~GeneratedPackageMvp"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleParameterizedComposition"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"
dotnet test ... --filter "FullyQualifiedName~ProjectLifecycle"
dotnet test ... --filter "FullyQualifiedName~ProjectStandaloneBuild"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleLibrary"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleCertification"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Then run:

```text
one real existing v1 generated-project regeneration
one new v2 generated-project regeneration
candidate/repeat/reopen
failure injection matrix
exactly one regenerated-project hidden standalone smoke
portable-copy proof
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

## Q. Evidence

Create exactly 14 files in each mirrored root:

```text
goal159-dashboard.json
architecture-review.json
goal158-independent-audit-intake.json
source-v2-migration-proof.json
shared-artifact-factory-proof.json
candidate-isolation-qualification-proof.json
regeneration-diff-proof.json
optimistic-concurrency-proof.json
atomic-apply-journal-proof.json
failure-recovery-matrix-proof.json
regeneration-history-ui-proof.json
standalone-portability-proof.json
artifact-scope-proof.json
goal159-report.md
```

Roots:

```text
.llmgc/procedural/goal-159-transactional-seed-regeneration-candidate-diff-and-atomic-apply/
.llmgc/exports/goal-159-transactional-seed-regeneration-candidate-diff-and-atomic-apply/
```

Twins byte-identical.

### Dashboard fields

```text
status
candidateStatus
goal159TestsDiscovered
goal159BehavioralTestsPassed

goal158IndependentAuditPassed
goal158HistoryP2Normalized

newCreationWritesV2
v1OpenUnchanged
v1RegenerationUpgradedToV2
v2RequestResolutionPassed
presetDefinitionCorrelationPassed
explicitOverrideTruthPassed

semanticNoOpRejected
optimisticConcurrencyPassed
candidateIsolationPassed
candidateBuildPassed
candidateRepeatDeterministic
candidateFreshReopenTravelCurrent
candidateAuthoringPreserved
candidateIdentityPreserved
candidateOldRcNotCurrent

oldSourceRequestSha256
newSourceRequestSha256
oldPlanSha256
newPlanSha256
oldOverlaySha256
newOverlaySha256
oldGeneratedBaseSha256
newGeneratedBaseSha256
addedRecordCount
removedRecordCount
changedRecordCount
gameplayChanged

atomicApplyPassed
oneNewHistoryEntryAdded
oldHistoryPreserved
oldRcBytesPreserved
oldRcLastSuccessAfterApply
journalCommitted
failureRollbackMatrixPassed
crashRecoveryMatrixPassed
authoritativeBeforeHashesRestoredOnFailure

regeneratedTravelCurrent
regenerationRecordCurrent
regenerationCardPassed
standalonePendingAfterApply

hostCacheKey
hostReused
hostRebuilt
hostFileSetHashUnchanged
unityProcessStartCount
hiddenSmokeInvocationCount
hiddenSmokePassed
standaloneSelfChecksPassed
actualPayloadNewWorldFactsPassed
actualPayloadAcceptedFactsPassed
releaseCandidateRecordCurrent
portableCopyCurrent

goal158RegressionPassed
goal157RegressionPassed
goal156RegressionPassed
goal155aRegressionPassed
goal155RegressionPassed
goal154dRegressionPassed
goal153cRegressionPassed
goal150RegressionPassed
goal149RegressionPassed
proceduralLegacyRegressionPassed
goal142SourceByteIdentical
sourceGoal148ByteIdentical

artifactScopeViolationCount
goal159Accepted=false
goal159ManualReviewRequired=false
goal159IndependentAuditRequired=true
```

No required GREEN field may be null/PARTIAL/NOT_EXECUTED/unverified constant.


## R. State and docs

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
docs/manual-acceptance/goal158-generated-region-travel.md
```

Create:

```text
docs/manual-acceptance/goal159-transactional-seed-regeneration.md
```

No human gate.

Required GREEN state:

```text
goal158IndependentAuditResult=GREEN_ACCEPTABLE_CANDIDATE_AT_9A350C63
goal158IndependentAuditPassed=true
goal158IndependentAuditRequired=false

goal159ImplementationStatus=GREEN
goal159CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal159Accepted=false
goal159AcceptedByHuman=false
goal159AcceptedByCodex=false
goal159ManualReviewRequired=false
goal159ManualGateReady=false
goal159IndependentAuditRequired=true

goal159GeneratedSourceV2Passed=true
goal159V1CompatibilityPassed=true
goal159CandidateIsolationPassed=true
goal159RegenerationDiffPassed=true
goal159OptimisticConcurrencyPassed=true
goal159AtomicApplyPassed=true
goal159RollbackRecoveryPassed=true
goal159AuthoringPreservationPassed=true
goal159IdentityPreservationPassed=true
goal159TravelRequalificationPassed=true
goal159OldRcLastSuccessPassed=true
goal159HostReused=true
goal159HostRebuilt=false
goal159UnityProcessStartCount=0
goal159HiddenSmokeInvocationCount=1
goal159PortableCopyPassed=true
goal159ArtifactScopeViolationCount=0

nextAction=independent_goal159_audit_and_plan_generated_save_migration_or_world_history_rollback
```

Release risk statement:

```text
Existing generated projects can now be transactionally regenerated from a new deterministic request.
The candidate is fully qualified before apply; project identity, FeatureModule authoring and prior RC
evidence are preserved, and failure/crash recovery restores exact authoritative hashes.
User-selectable historical world rollback and cross-seed gameplay-save migration remain future work.
```

Close the preset-label P2 through the v2 requested/resolved options model.
Close the v2 `BUILD_CURRENT` normalization P2.

## S. Text integrity

Scan actual changed/task/evidence/docs files:

```text
valid UTF-8
no NUL
no forbidden C0 except CR/LF/TAB
no mojibake markers
no escaped Cyrillic where policy forbids
no absolute candidate/source paths in committed evidence
```

Historical evidence immutable.

## T. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal159-transactional-regeneration.ps1
.devflow/scripts/run-goal159-transactional-regeneration.cmd

src/LLMGameCreator.Application/Generation/Procedural/SeededGeneratedProjectModels.cs
src/LLMGameCreator.Application/Generation/Procedural/SeededGeneratedProjectSourceService.cs
src/LLMGameCreator.Application/Generation/Procedural/SeededGeneratedProjectArtifactFactory.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationModels.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationDiffService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationTransaction.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationRecordService.cs

src/LLMGameCreator.Application/Projects/SeededGeneratedGameProjectCreationService.cs
src/LLMGameCreator.Application/Projects/GameProjectService.cs

src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/GameProjectFeatureModuleAuthoringService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildHistoryReader.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectGeneratedWorldSummaryService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectReleaseCandidateRecordService.cs

src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.WinForms/Pages/Projects/RegenerateGeneratedWorldDialog.cs
src/LLMGameCreator.WinForms/Pages/Projects/RegenerateGeneratedWorldDialog.Designer.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/Goal159/Goal159SourceV2Tests.cs
tests/LLMGameCreator.Tests/Application/Goal159/Goal159CandidateIsolationTests.cs
tests/LLMGameCreator.Tests/Application/Goal159/Goal159RegenerationDiffTests.cs
tests/LLMGameCreator.Tests/Application/Goal159/Goal159AtomicApplyRollbackTests.cs
tests/LLMGameCreator.Tests/Application/Goal159/Goal159CrashRecoveryTests.cs
tests/LLMGameCreator.Tests/Application/Goal159/Goal159WorkspaceUiTests.cs
tests/LLMGameCreator.Tests/Application/Goal159/Goal159StandaloneAndPortabilityTests.cs
tests/LLMGameCreator.Tests/Application/Goal158/Goal158HistoryUiRollbackTests.cs
tests/LLMGameCreator.Tests/Application/Goal156/Goal156ProjectCreationTests.cs
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
docs/manual-acceptance/goal158-generated-region-travel.md
docs/manual-acceptance/goal159-transactional-seed-regeneration.md

docs/agent-tasks/goal-159-transactional-seed-regeneration-candidate-diff-and-atomic-apply/
.llmgc/procedural/goal-159-transactional-seed-regeneration-candidate-diff-and-atomic-apply/
.llmgc/exports/goal-159-transactional-seed-regeneration-candidate-diff-and-atomic-apply/
```

One exact additional existing Application/WinForms/test path may be added only after a concrete
compile/test failure and with recorded reason.

Forbidden without a newly reproduced Goal159 P0/P1:

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

No Runtime or FeatureModule semantics change is expected.

## U. Command budget

```text
read-first/architecture review: 14 minutes
v2 source + shared artifact factory: 22 minutes
candidate isolation/qualification/diff: 24 minutes
transaction/journal/recovery: 28 minutes
controller/UI/history/RC integration: 20 minutes
behavioral tests: 32 minutes
real v1/v2 matrix + one smoke: 14 minutes
regressions/evidence/docs/artifact scope: 18 minutes
target wall clock: 145 minutes
maximum two concurrent testhost processes
Unity process count: 0
```

Rules:

```text
write complete test inventory before production edits
write publication/evidence script before long candidate matrix
no unchanged command repetition
no timeout escalation
after failure run only exact class/test
P0/P1 fixed inside Goal159
P2/P3 debt only
do not defer evidence/docs/artifact scope
```

## V. Publication

Create exactly one final commit:

```text
GREEN Goal 159 transactional seed regeneration candidate diff and atomic apply
```

or honest BLOCKED/FAILED.

Codex performs standard push.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three Goal159 task files tracked
Goal142 and goal148-manual unchanged
Unity starts=0
HostRebuilt=false
hidden smoke=1 only on GREEN
Goal154 acceptance unchanged
Goal155 RC milestone passed
Goal158 independent audit passed
Goal159 accepted=false/no human gate
```

## W. GREEN criteria

```text
Goal158 independent audit recorded GREEN
Goal159 tests >=50 / behavioral >=44 / all pass

new generated projects write strict v2
valid v1 opens/builds unchanged
v1 regeneration upgrades only on success
preset/request/resolution truth is explicit

semantic no-op rejected
optimistic concurrency checked twice
candidate completely isolated
candidate uses real build/travel qualification
authoring/identity exact
meaningful diff generated

journaled apply commits exact candidate
one new history entry
old histories and RC bytes preserved
old RC becomes LAST_SUCCESS
failed apply restores exact before hashes
crash recovery matrix passed
last successful regeneration record validated

fresh authoritative reopen TRAVEL_CURRENT
UI workflow/card truthful
v2 history normalized to START_CURRENT
standalone pending after apply

one cached hidden smoke after regeneration
HostReused=true / HostRebuilt=false / Unity=0
actual payload new source/travel/accepted facts
new RC CURRENT
portable copy current without execution

Goal158/157/156/155A/155/154D/153C/150/149 and procedural regressions GREEN
14+14 evidence mirrored
text integrity GREEN
artifact scope 0
goal159CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
one final commit pushed
```

## X. Final report

Return GREEN/BLOCKED/FAILED and include:

- model/reasoning;
- architecture review;
- Goal158 independent-audit intake;
- discovered/behavioral test counts;
- v1/v2 source results and preset/override truth;
- exact regeneration request;
- candidate isolation/build/repeat/reopen;
- authoring/identity preservation;
- old/new hashes and generated diff counts;
- optimistic concurrency matrix;
- apply journal/order;
- failure/crash rollback results;
- authoritative reopen/history/RC statuses;
- UI/card facts;
- host/Unity/smoke;
- actual payload/new RC/portable copy;
- regressions;
- source/baseline immutability;
- evidence/text/artifact scope;
- state/no-human-gate;
- final SHA/push/HEAD/worktree.
