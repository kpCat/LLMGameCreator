# Goal 157 — Generated-World Provenance & Runtime Start Activation Vertical Slice

## Identity
- Task ID: `goal-157-generated-world-provenance-and-runtime-start-activation-vertical-slice`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `12ef8a4dca81911a2f270bc24477a31a884291b8`
- Required base message: `GREEN Goal 156 seeded generated project creation modern workspace and cached standalone`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration
```text
Model: GPT-5.6 Sol
Reasoning effort: Extra High
```

This is a major product vertical slice, not a child hotfix. It closes the Goal156 provenance P1 and
activates the generated package as the real player start/runtime route while preserving accepted
FeatureModule compatibility qualification.

## Pre-approval
- The owner approved execution by launching this task.
- Produce a concise internal plan; do not ask for confirmation.
- Do not request manual testing.
- Own all P0/P1 defects reproduced by the mandatory Goal157 matrix inside this Goal.
- P2/P3 go to debt; do not create Goal157A.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.

## Initial worktree
After unpacking, only these untracked files are allowed:
```text
docs/agent-tasks/goal-157-generated-world-provenance-and-runtime-start-activation-vertical-slice/GOAL.md
docs/agent-tasks/goal-157-generated-world-provenance-and-runtime-start-activation-vertical-slice/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-157-generated-world-provenance-and-runtime-start-activation-vertical-slice/README.md
```
Require HEAD==origin/main==`12ef8a4dca81911a2f270bc24477a31a884291b8`, branch main and no other dirt. No reset/stash/merge/rebase.

## Unity budget
```text
Unity Editor: 0
Unity host build: 0
real hidden standalone smoke: exactly 1
visible automatic launch: 0
```

## Goal156 audit intake
Record:
```text
goal156IndependentAuditResult=BLOCKED_AT_12EF8A4D
goal156IndependentAuditBlocker=generated_source_request_not_correlated_with_deterministic_plan_and_overlay_chain
goal156AuditBlocker=closed_by_goal157 only on GREEN
```

Preserve Goal156 foundations:
```text
atomic seeded creation
legacy template lane
all_selectable_defaults/core_only
project-local generation source/sidecars
additive overlay/custom composition base
modern build/history/UI
accepted mechanics/RC
cached standalone and portable copy
```

## P1 provenance finding
Current source validation checks sidecar hashes and regenerates rule-pack/tiny-loop/MVP from the
stored plan, but it does not regenerate the plan from declared seed/mode/preset/style/variants and
does not rebuild overlay/base from the canonical Goal142 baseline.

Concrete false-truth case:
```text
create valid project
edit only seeded-project-source.json seed
leave plan and all sidecars unchanged
source validation remains GREEN
UI shows fake seed for a world generated from another seed
```

Mode/style/variant provenance and self-consistent altered overlay/base have the same gap.

## Product outcome
```text
declared generation request
→ reproducible plan/rule/loop/MVP/overlay/base
→ modern FeatureModule compatibility qualification
→ final player package starts on generated map
→ real Runtime starts, moves and interacts with generated content
→ deterministic activation replay
→ cached standalone uses generated activation frames/facts
```

## Two-lane build model

### Lane A — accepted mechanics compatibility
Package retains baseline start map and supplies:
```text
AcceptedMechanics
equipment/attributes/progression/ability/mana/status/social facts
FeatureModule semantic observations
compatibility checkpoint/replay/action binding
compatibility final hash
```

### Lane B — generated player activation
Same module-composed package except:
```text
Manifest.StartMapId = generated source GeneratedStartMapId
then project identity overlay
```
This lane supplies primary:
```text
project package/composition hashes
FinalStateHash
RuntimeFrames
activation replay
standalone payload
RC record
```

The two pre-identity packages may differ only in startMapId and necessary self-describing metadata.

## Non-goals
```text
new Runtime primitives
new FeatureModules
public GamePackage schema changes
Unity project changes
host rebuild
seed regeneration UI
multi-region travel UI
provider/LLM/Lua/media execution
infinite streaming
```


## Mandatory architecture review

Read at most 14 primary files:
```text
AGENTS.md
docs/GOAL_DESIGN_QUALITY_POLICY.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/UNITY_EXECUTION_POLICY.md
SeededGeneratedGameProjectCreationService.cs
SeededGeneratedProjectSourceService.cs
GeneratedProjectOverlayService.cs
ProceduralGameKernelService.cs
VisibleGeneratedPlayablePreviewService.cs
GeneratedPlayableRuntimePreviewAdapter.cs
FeatureModuleParameterizedCompositionService.cs
GameProjectBuildAndQualificationService.cs
GameProjectAcceptedMechanicsSummaryService.cs
ProjectStandaloneBuild contracts/controller
```

Before production edits write:
```text
.llmgc/procedural/goal-157-generated-world-provenance-and-runtime-start-activation-vertical-slice/architecture-review.json
```

Required resolved sections:
```text
goal156AuditFinding
deterministicRequestRegeneration
canonicalBaselineProvider
fullArtifactChainComparison
compatibilityLane
activationLane
hashAndStateAuthority
acceptedMechanicsSeparation
runtimeActivationCommands
checkpointReplayContract
standaloneCorrelation
rollbackMatrix
legacyCompatibility
nonGoals
```

Every section names concrete types, inputs, outputs and behavioral tests. Vague sections block GREEN.

## A. Strict generated-source provenance

Upgrade `SeededGeneratedProjectSourceService` with the repository/application generation authority it
needs to reproduce the full chain.

### A1. Canonical request resolution

From the source record build:
```text
GenerationPresetOptionsRequest:
  Seed
  Mode
  PresetId
  CompactStyleHintIds
  SelectedVariantIds
```

Call `GenerationPresetOptionsService.Resolve()`.

Require exact ordinal equality:
```text
resolved seed == source seed
resolved mode == source mode
resolved preset == source preset
resolved sorted style hints == source sorted style hints
resolved sorted variant ids == source sorted variant ids
```

Failures:
```text
generated_source.request_resolution_mismatch
generated_source.seed_mismatch
generated_source.mode_mismatch
generated_source.style_hints_mismatch
generated_source.variant_ids_mismatch
```

Do not silently normalize a persisted source into another world.

### A2. Regenerate plan from request

Call `ProceduralGameKernelService.Generate()` with the resolved options.

Require byte-identical:
```text
generated-game-plan.json
generated-game-plan.md
```

Also require:
```text
source PlanId == regenerated PlanId
source PlanSha256 == actual plan file SHA
source seed/mode == plan metadata
source style/variant lists == plan profile
```

Failures:
```text
generated_source.plan_regeneration_mismatch
generated_source.plan_metadata_mismatch
generated_source.plan_profile_mismatch
```

Editing only seed/mode/style/variants in the source record must fail.

### A3. Regenerate the downstream chain

From the regenerated plan:
```text
FormulaEffectActionRegistryService.Generate
TinyGeneratedRuntimeLoopService.Run
GeneratedPackageMvpService.Generate
NamespaceGeneratedPackage
```

Require exact byte equality for rule pack, tiny state/report and namespaced MVP.

Keep existing causal diagnostics, but the regenerated plan—not the stored plan—is authority.

### A4. Canonical Goal142 baseline provider

Create/inject a focused baseline provider:
```text
IGeneratedProjectBaselineProvider
Goal142GeneratedProjectBaselineProvider
```

Output:
```text
path
actual package bytes
actual SHA-256
candidate/source identity
```

It must read the canonical Goal142 matrix row and verify the actual package bytes.

Production source validation, creation and build use the same provider.

Require:
```text
source Goal142BaselinePackageSha256 == current canonical baseline SHA
overlay Goal142BaselinePackageSha256 == canonical baseline SHA
```

Failure:
```text
generated_source.baseline_hash_mismatch
generated_source.baseline_unavailable
```

Portable projects remain supported because the installed application/repository supplies the canonical
baseline; absolute project paths are not used.

### A5. Rebuild overlay/base

Rebuild:
```text
GeneratedProjectOverlayService.Build(
  canonical baseline bytes/hash,
  regenerated namespaced MVP,
  regenerated plan)
```

Require byte-identical:
```text
generated-project-overlay.json
generated-base-package.json
```

Require the source direct hashes and sidecar inventory match the rebuilt outputs.

Failures:
```text
generated_source.overlay_regeneration_mismatch
generated_source.base_regeneration_mismatch
generated_source.overlay_hash_chain_mismatch
```

A self-consistent forged overlay/base must not pass.

### A6. Schema compatibility

Do not require a source schema bump unless new persisted fields are necessary.

Existing valid Goal156 v1 sources must continue to validate under stricter reproduction.

Malformed or unverifiable v1 sources fail causally; do not auto-rewrite them.

## B. Generated player package activation

Create an Application service:
```text
GameProjectGeneratedWorldActivationService
```

Inputs:
```text
module-composed compatibility package
generated source validation result
project identity
IGameRuntime
```

### B1. Build the player package

Clone the compatibility package.

Require:
```text
generated start map exists exactly once
generated start map has a valid start position
generated start map contains generated interactable content
```

Set:
```text
Manifest.StartMapId = source.GeneratedStartMapId
```

Do not change any Game collection record.

Produce:
```text
compatibilityPackageSha256
playerCompositionPackageJson
playerCompositionPackageSha256
canonical gameplay-record diff
manifest diff
```

Require the canonical gameplay-record diff to be empty.

The only non-identity semantic change is `manifest.startMapId`.

### B2. Project identity

Apply existing project identity overlay to the player composition package.

Require:
```text
project package identity exact
start map remains generated
identity overlay does not change gameplay collections
```

The activated project package—not the compatibility package—is written to `package.json`.

## C. Runtime-owned generated activation

Use the existing `IGameRuntime` implementation. Do not implement a second movement/interact engine.

Execute a deterministic action script:
```text
Start(final player package)
Move Right
Interact
```

The exact commands match the existing generated playable preview contract.

Require:
```text
start succeeds on generated start map
current map remains generated map
move succeeds
interact succeeds
at least one generated entity/interaction is observed
Runtime events nonempty
state changes after commands
```

### C1. Data-derived generated correlation

Derive expected generated IDs from:
```text
source overlay generated records
final package map/entities/interactions
```

Do not hardcode generated map/entity/interaction IDs.

Require the interaction events/target to correlate with a generated record or generated provenance
record.

### C2. Deterministic replay

Start a fresh Runtime state and repeat the exact command script.

Require:
```text
same per-step success
same map and positions
same ordered event semantics
same final state hash
same activation facts
```

### C3. State roundtrip

Use the existing Runtime save/state serialization seam when available.

Require:
```text
serialize final state
restore
canonical state hash equal
current generated map equal
```

If no public save seam exists, use the repository's accepted canonical state serializer. Do not add a
new public Runtime schema.

### C4. Invalid activation

Reject causally:
```text
generated start map missing
generated start position invalid
move cannot execute
interaction cannot execute
interaction resolves only baseline content
replay differs
state roundtrip differs
```

No generated project build is GREEN from mere package presence.

## D. Typed activation result

Create:
```text
GameProjectGeneratedWorldActivationSummary
```

Fields:
```text
Present
Passed
GeneratedStartMapId
GeneratedStartMapTitle
StartSucceeded
MoveSucceeded
InteractSucceeded
GeneratedInteractionObserved
InitialStateHash
FinalStateHash
ReplayFinalStateHash
ReplayEquivalent
StateRoundtripPassed
RuntimeFrames[]
HumanFacts[]
Diagnostics[]
```

Human facts:
```text
Игровой старт
Движение
Взаимодействие
Сгенерированное содержимое
Повтор
Сохранение состояния
```

No IDs/hashes in human facts.

Example:
```text
Игровой старт: сгенерированная карта
Движение: пройдено
Взаимодействие: пройдено
Сгенерированное содержимое: подтверждено
Повтор: идентичен
```


## E. Accepted mechanics compatibility result

The accepted mechanics lane remains mandatory for `all_selectable_defaults`.

Create a typed internal/build model:
```text
GameProjectAcceptedMechanicsCompatibilityResult
```

Fields:
```text
Passed
CompatibilityCompositionPackageSha256
CompatibilityActivatedPackageSha256 optional
CompatibilityFinalStateHash
CheckpointReloadPassed
FullReplayEquivalent
ActionBindingPassed
RuntimeFrames optional technical
AcceptedMechanics
Social
Diagnostics
```

### E1. No global-field confusion

For generated activated projects:

```text
GameProjectBuildResult.PackageSha256
GameProjectBuildResult.CompositionPackageSha256
GameProjectBuildResult.FinalStateHash
GameProjectBuildResult.RuntimeFrames
```

must describe Lane B, the final player package and generated activation.

`AcceptedMechanics` and `Social` come from Lane A and carry their own compatibility hashes/flags.

Update `GameProjectAcceptedMechanicsSummary` additively with:
```text
QualificationPackageSha256
QualificationFinalStateHash
QualificationCheckpointReloadPassed
QualificationFullReplayEquivalent
QualificationActionBindingPassed
```

Legacy projects populate these from their single existing lane.

The summary service uses these typed compatibility fields rather than assuming the build's primary
FinalStateHash represents accepted mechanics.

### E2. Build GREEN contract

Generated all-selectable build is GREEN only when:
```text
source provenance passed
compatibility lane passed
accepted mechanics passed
social projection passed
activation lane passed
final player package validation passed
generated records preserved
```

Generated core-only:
```text
compatibility lane build valid
AcceptedMechanics Passed=false with MissingFactKinds
activation lane passed
overall project build GREEN
```

## F. Build/history/controller integration

### F1. GameProjectBuildAndQualificationService

For legacy projects:
```text
existing single-lane behavior and hashes unchanged
```

For generated projects:
1. strict source reproduction;
2. explicit generated base FeatureModule composition;
3. Lane A qualification and accepted facts;
4. generated-record preservation;
5. create Lane B player composition with generated start map;
6. project identity overlay over Lane B;
7. validate final package;
8. Runtime activation/replay/state roundtrip;
9. activate final player package transactionally;
10. write build history.

Primary build fields use Lane B.

### F2. History

Persist:
```text
GeneratedWorld
GeneratedWorldActivation
AcceptedMechanics compatibility hashes/flags
primary player package/final hashes
```

Restore from one matching GREEN history row.

Old rows remain readable.

A history row for a generated project is eligible only when:
```text
generated source hashes current
player package/document hashes current
qualified authoring fingerprint current or truthfully LAST_SUCCESS
```

### F3. Generated card

Extend the existing card, not add a duplicate card.

Rows include:
```text
Seed
Режим
Пресет
counts
Сгенерированный цикл
Игровой старт
Движение
Взаимодействие
Статус сборки
```

States:
```text
SOURCE_READY
BUILD_CURRENT
LAST_SUCCESS
INVALID
```

`BUILD_CURRENT` for a Goal157 generated project requires `GeneratedWorldActivation.Passed=true`.

Before Goal157 build, an existing valid Goal156 project remains `SOURCE_READY` even if it has an old
Goal156 history row without activation evidence. Do not infer activation.

### F4. Technical details

Expose:
```text
source request hash/plan hash
compatibility package/final hash
player composition/project/final hash
generated start map ID
activation step statuses
```

No raw IDs/hashes in the primary card.

## G. Standalone

Use the final player package and activation RuntimeFrames.

Append through typed formatters:
```text
generated-world source facts
generated activation facts
accepted mechanics facts when complete
Release Candidate=готов when current
```

Actual payload must prove:
```text
project manifest start map == generated start map
player adapter final state hash == activation FinalStateHash
RuntimeFrames begin from generated activation
generated activation human facts present
accepted mechanics facts present for all-selectable
```

RC record uses Lane B package/composition/final hashes and remains CURRENT under Goal155A truth rules.

Do not modify Unity host code.

## H. Portable project

Copy a complete built/standalone generated project.

Without build, Runtime or Unity:
```text
source provenance valid
generated summary restored
activation summary restored
accepted mechanics restored
RC record CURRENT
package start map generated
```

No absolute path dependency.

## I. Rollback

Failure at any stage:
```text
source reproduction
compatibility qualification
player-package activation mapping
Runtime start/move/interact
activation replay
state roundtrip
final package validation
standalone
```

must preserve:
```text
current package bytes
authoring document
source/sidecars
last successful build/history
last activation summary
RC record
```

No staging/temp leak.

## J. Creation behavior

New Goal157 generated projects may still be created with baseline start map before first modern build.

After first successful build:
```text
package start map becomes generated start map
```

Do not automatically build during interactive creation.

Legacy template projects are unchanged.

## K. Behavioral tests

Create at least 40 Goal157 tests; at least 34 behavioral.

### Provenance
1. exact valid Goal156 source reproduces full chain;
2. editing only source seed fails;
3. source mode mismatch fails;
4. source style hints mismatch fails;
5. source variant IDs mismatch fails;
6. stored plan from another seed fails even with rewritten sidecar hashes;
7. stored plan metadata mismatch fails;
8. altered rule pack fails;
9. altered tiny loop fails;
10. altered MVP fails;
11. self-consistent altered overlay/base fails canonical rebuild;
12. source baseline hash mismatch fails;
13. existing valid v1 source remains valid;
14. failed validation changes no files.

### Player package
15. compatibility package retains baseline start;
16. player package uses generated start;
17. all gameplay collections equal between lanes;
18. generated start map exists and is generated;
19. identity overlay preserves generated start;
20. missing start map fails;
21. invalid start position fails.

### Runtime activation
22. Runtime starts on generated map;
23. Move Right succeeds;
24. Interact succeeds;
25. generated target/provenance observed;
26. state changes;
27. deterministic replay equivalent;
28. state roundtrip exact;
29. failed move rejects activation;
30. failed interact rejects activation;
31. baseline-only interaction cannot satisfy generated correlation.

### Compatibility
32. all-selectable accepted mechanics still pass;
33. accepted facts remain expected effective defaults;
34. social facts remain correct;
35. compatibility hashes differ from player activation hashes where expected;
36. primary FinalStateHash belongs to activation lane;
37. core-only overall build GREEN but AcceptedMechanics false;
38. legacy build remains single-lane and hash-compatible.

### Build/history/UI
39. generated build/repeat deterministic;
40. fresh reopen restores activation BUILD_CURRENT;
41. old Goal156 history without activation does not claim BUILD_CURRENT;
42. saved mechanic change yields LAST_SUCCESS;
43. source provenance failure preserves last success;
44. activation failure rolls back package/history;
45. generated card contains activation rows;
46. card has no IDs/hashes/paths;
47. technical details expose both lanes.

### Standalone/portable
48. exactly one hidden smoke;
49. host reused/not rebuilt/Unity zero;
50. actual package start map generated;
51. payload final hash matches activation;
52. payload generated activation facts present;
53. payload accepted facts present;
54. RC record CURRENT;
55. portable copy restores source/activation/accepted/RC without execution.

### Regressions
56. Goal156 creation/custom-base/overlay regressions GREEN;
57. Goal155A/155 regressions GREEN;
58. Goal154D/153C/150/149 regressions GREEN;
59. existing procedural preview/runtime tests GREEN;
60. Goal142 and goal148-manual source byte-identical.

Do not claim list counts unless discovered/executed.

## L. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal157"
# require >=40 total and >=34 behavioral

dotnet test ... --filter "FullyQualifiedName~Goal157"
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
dotnet test ... --filter "FullyQualifiedName~VisibleGeneratedPlayable"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleParameterizedComposition"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"
dotnet test ... --filter "FullyQualifiedName~ProjectStandaloneBuild"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleLibrary"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleCertification"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Then run one real generated all-selectable build/repeat/reopen, exactly one hidden standalone smoke and
portable-copy proof. Run artifact scope last.

Do not run full suite, 85-case closure, all-ProductSmoke or Unity host build.


## M. Evidence

Create exactly 13 files in each mirrored root:

```text
goal157-dashboard.json
architecture-review.json
goal156-independent-audit-finding.json
source-request-regeneration-proof.json
canonical-chain-rebuild-proof.json
compatibility-player-package-diff-proof.json
generated-runtime-activation-proof.json
activation-replay-roundtrip-proof.json
accepted-mechanics-compatibility-proof.json
generated-build-history-ui-proof.json
standalone-portability-proof.json
artifact-scope-proof.json
goal157-report.md
```

Roots:

```text
.llmgc/procedural/goal-157-generated-world-provenance-and-runtime-start-activation-vertical-slice/
.llmgc/exports/goal-157-generated-world-provenance-and-runtime-start-activation-vertical-slice/
```

Twins byte-identical by relative name and SHA-256.

Dashboard fields:

```text
status
candidateStatus
goal157TestsDiscovered
goal157BehavioralTestsPassed

goal156IndependentAuditBlockerRecorded
goal156AuditBlockerClosed

sourceSeedCorrelationPassed
sourceModeCorrelationPassed
sourceStyleCorrelationPassed
sourceVariantCorrelationPassed
planRegenerationPassed
rulePackRegenerationPassed
tinyLoopRegenerationPassed
mvpRegenerationPassed
canonicalBaselineCorrelationPassed
overlayRegenerationPassed
generatedBaseRegenerationPassed

compatibilityBaselineStartMapPreserved
playerGeneratedStartMapActivated
gameplayCollectionsEquivalentBetweenLanes
projectIdentityPreserved

runtimeStartSucceeded
runtimeStartMapIsGenerated
runtimeMoveSucceeded
runtimeInteractSucceeded
generatedInteractionObserved
activationStateChanged
activationReplayEquivalent
activationStateRoundtripPassed

allSelectableCompatibilityPassed
allSelectableAcceptedMechanicsPassed
allSelectableSocialPassed
coreOnlyBuildPassed
legacySingleLaneRegressionPassed

generatedBuildPassed
generatedRepeatBuildDeterministic
generatedFreshReopenCurrent
oldGoal156HistoryNotPromoted
generatedCardActivationFactsPassed

hostCacheKey
hostReused
hostRebuilt
hostFileSetHashUnchanged
unityProcessStartCount
hiddenSmokeInvocationCount
hiddenSmokePassed
standaloneSelfChecksPassed
actualPackageGeneratedStartMapPassed
actualPayloadActivationHashPassed
actualPayloadActivationFactsPassed
actualPayloadAcceptedFactsPassed
releaseCandidateRecordCurrent
portableCopyCurrent

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
goal157Accepted=false
goal157ManualReviewRequired=false
goal157IndependentAuditRequired=true
```

No required GREEN field may be null/PARTIAL/NOT_EXECUTED/unverified constant.

## N. State/docs

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
docs/manual-acceptance/goal156-seeded-generated-project-creation.md
```

Create:

```text
docs/manual-acceptance/goal157-generated-world-runtime-start-activation.md
```

No human gate.

Required GREEN publication:

```text
goal156IndependentAuditResult=BLOCKED_AT_12EF8A4D
goal156IndependentAuditBlocker=generated_source_request_not_correlated_with_deterministic_plan_and_overlay_chain
goal156AuditBlocker=closed_by_goal157

goal156ImplementationStatus=GREEN
goal156CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal156Accepted=false
goal156IndependentAuditRequired=false

goal157ImplementationStatus=GREEN
goal157CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal157Accepted=false
goal157AcceptedByHuman=false
goal157AcceptedByCodex=false
goal157ManualReviewRequired=false
goal157ManualGateReady=false
goal157IndependentAuditRequired=true

goal157SourceProvenancePassed=true
goal157CanonicalChainRebuildPassed=true
goal157CompatibilityLanePassed=true
goal157GeneratedStartMapActivated=true
goal157RuntimeStartMoveInteractPassed=true
goal157ActivationReplayPassed=true
goal157ActivationStateRoundtripPassed=true
goal157AcceptedMechanicsCompatibilityPassed=true
goal157GeneratedHistoryPersistencePassed=true
goal157HostReused=true
goal157HostRebuilt=false
goal157UnityProcessStartCount=0
goal157HiddenSmokeInvocationCount=1
goal157PortableCopyPassed=true
goal157ArtifactScopeViolationCount=0

nextAction=independent_goal157_audit_and_plan_generated_region_travel_or_seed_regeneration
```

Release risk statement:
```text
Generated project provenance is now reproducible from its declared request.
The final player package starts on generated content and executes a Runtime-owned move/interact loop.
Accepted modern mechanics remain a separately typed compatibility proof.
Multi-region travel and seed regeneration remain explicit future product choices.
```

## O. Text integrity

Scan actual changed/task/evidence/docs files:

```text
valid UTF-8
no NUL
no forbidden C0 except CR/LF/TAB
no mojibake markers
no escaped Cyrillic in user-facing JSON/Markdown where policy forbids
no absolute disposable/source paths in committed evidence
```

Historical artifacts remain immutable.

## P. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal157-generated-world-activation.ps1
.devflow/scripts/run-goal157-generated-world-activation.cmd

src/LLMGameCreator.Application/Generation/Procedural/SeededGeneratedProjectSourceService.cs
src/LLMGameCreator.Application/Generation/Procedural/SeededGeneratedProjectModels.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedProjectBaselineProvider.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectGeneratedWorldActivationService.cs

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildHistoryReader.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectGeneratedWorldSummaryService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectAcceptedMechanicsSummaryService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectReleaseCandidateRecordService.cs

src/LLMGameCreator.Application/Projects/SeededGeneratedGameProjectCreationService.cs
src/LLMGameCreator.Application/Projects/GameProjectService.cs

src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs

tests/LLMGameCreator.Tests/Application/Goal157/Goal157SourceProvenanceTests.cs
tests/LLMGameCreator.Tests/Application/Goal157/Goal157PlayerPackageActivationTests.cs
tests/LLMGameCreator.Tests/Application/Goal157/Goal157RuntimeActivationTests.cs
tests/LLMGameCreator.Tests/Application/Goal157/Goal157CompatibilityBuildTests.cs
tests/LLMGameCreator.Tests/Application/Goal157/Goal157WorkspaceUiTests.cs
tests/LLMGameCreator.Tests/Application/Goal157/Goal157StandaloneAndPortabilityTests.cs
tests/LLMGameCreator.Tests/Application/Goal156/Goal156DeterminismAndOverlayTests.cs
tests/LLMGameCreator.Tests/Application/Goal156/Goal156GeneratedWorkspaceTests.cs
tests/LLMGameCreator.Tests/Application/Goal156/Goal156StandaloneAndPortabilityTests.cs
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
docs/manual-acceptance/goal157-generated-world-runtime-start-activation.md

docs/agent-tasks/goal-157-generated-world-provenance-and-runtime-start-activation-vertical-slice/
.llmgc/procedural/goal-157-generated-world-provenance-and-runtime-start-activation-vertical-slice/
.llmgc/exports/goal-157-generated-world-provenance-and-runtime-start-activation-vertical-slice/
```

One exact additional Application/RuntimePreview/test path may be added only after a concrete
compile/test failure and with recorded reason.

Forbidden without a newly reproduced Goal157 P0/P1:

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

Use existing IGameRuntime through injection/composition; do not edit Runtime projects.

## Q. Command budget

```text
read-first/architecture review: 12 minutes
provenance chain closure: 18 minutes
two-lane build/typed models: 24 minutes
Runtime activation/replay: 16 minutes
history/UI/standalone integration: 14 minutes
behavioral tests: 26 minutes
real matrix + one smoke: 12 minutes
regressions/evidence/docs/artifact scope: 16 minutes
target wall clock: 120 minutes
maximum two concurrent testhost processes
Unity process count: 0
```

Rules:
```text
write test inventory before production edits
write publication script before long external proof
no unchanged command repetition
no timeout escalation
after failure run only exact class/test
P0/P1 fixed inside Goal157
P2/P3 debt only
do not defer evidence/docs/artifact scope
```

## R. Publication

Create exactly one final commit:

```text
GREEN Goal 157 generated world provenance and Runtime start activation vertical slice
```

or honest BLOCKED/FAILED.

Codex performs standard push.

Required final:
```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three Goal157 task files tracked
Goal142 and goal148-manual unchanged
Unity count=0
HostRebuilt=false
hidden smoke=1 only on GREEN
Goal154 acceptance unchanged
Goal155 milestone remains passed
Goal156 accepted=false/no human gate
Goal157 accepted=false/no human gate
```

## S. GREEN criteria

```text
Goal156 provenance P1 recorded and closed
declared source request regenerates exact plan
canonical baseline regenerates exact overlay/base
false seed/mode/style/variant truth rejected

compatibility and player packages typed/separate
player package generated start active
gameplay collections equal between lanes
project identity exact

real IGameRuntime start/move/interact on generated content
generated target correlation
deterministic replay
state roundtrip
primary build hashes/frames belong to activation lane

accepted mechanics compatibility remains GREEN
core-only remains buildable
legacy single-lane hashes preserved

build/repeat/reopen/history/UI truthful
old Goal156 history cannot claim activation
rollback preserves last success

one cached hidden standalone smoke
actual package/payload generated start and activation facts
accepted facts/RC current
portable project current without execution

Goal156/155A/155/154D/153C/150/149 and procedural regressions GREEN
13+13 evidence mirrored
text integrity GREEN
artifact scope 0
goal157CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
one final commit pushed
```

## T. Final report

Return GREEN/BLOCKED/FAILED and include:

- model/reasoning;
- architecture review;
- exact Goal156 P1 reproduction;
- source request/plan/full-chain regeneration;
- canonical baseline proof;
- compatibility/player package diff;
- generated Runtime start/move/interact;
- replay/state roundtrip;
- accepted mechanics compatibility;
- primary versus compatibility hashes;
- build/repeat/reopen/history/UI;
- rollback;
- host/Unity/smoke;
- actual package/payload/RC;
- portable copy;
- regressions;
- source immutability;
- evidence/text/artifact scope;
- state/no-human-gate;
- final SHA/push/HEAD/worktree.
