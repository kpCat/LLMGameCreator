# Goal 158 — Generated Region Travel, Runtime State & Standalone Vertical Slice

## Identity

- Task ID: `goal-158-generated-region-travel-runtime-and-standalone-vertical-slice`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `8939aea01f759e1c22409c7fbce871cba113d856`
- Required base message: `GREEN Goal 157 generated world provenance and Runtime start activation vertical slice`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Sol
Reasoning effort: Extra High
```

Reason: this is a major playable-product vertical slice. It introduces one genuinely missing generic
Runtime interaction primitive—data-driven map transition—and uses it to connect the already generated
region graph to the modern two-lane project build, replay, history, UI, standalone and portable state.
It must preserve Goal156/157 source artifacts and accepted FeatureModule compatibility.

## Pre-approval

- The owner approved execution by launching this task.
- Produce a concise internal implementation plan; do not ask for confirmation.
- Do not request intermediate or final manual testing.
- Own all P0/P1 defects reproduced by the mandatory Goal158 matrix inside this Goal.
- Record P2/P3 debt without creating Goal158A.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No candidate/intermediate commits.
- Codex performs commit and standard push itself.

## Expected initial worktree

After unpacking this ZIP, only these untracked files are permitted:

```text
docs/agent-tasks/goal-158-generated-region-travel-runtime-and-standalone-vertical-slice/GOAL.md
docs/agent-tasks/goal-158-generated-region-travel-runtime-and-standalone-vertical-slice/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-158-generated-region-travel-runtime-and-standalone-vertical-slice/README.md
```

Require:

```text
HEAD == origin/main == 8939aea01f759e1c22409c7fbce871cba113d856
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

Use the existing generic host cache. If it is incomplete or would rebuild, publish BLOCKED without
starting Unity.

## Goal157 independent-audit intake

Independent audit verdict:

```text
goal157IndependentAuditResult=GREEN_ACCEPTABLE_CANDIDATE_AT_8939AEA0
goal157IndependentAuditPassed=true
goal157IndependentAuditRequired=false
```

No P0/P1 remains in Goal157.

Audited truths:

```text
declared source request regenerates exact plan/rule/tiny/MVP/overlay/base
canonical Goal142 baseline is reverified
Lane A owns AcceptedMechanics/Social qualification
Lane B owns generated start package/final state/frames
real IGameRuntime Start → Move Right → Interact succeeds
generated target, replay and state roundtrip are proven
history/standalone/RC/portable truth is correlated
13×2 evidence and all three Goal157 task files are actually committed
```

Goal157 remains accepted=false and creates no human gate.

### Deferred P2

`PresetId` remains descriptive metadata while the complete effective style/variant lists are the
actual deterministic generation inputs. Changing only a preset label while retaining explicit
effective options does not change the generated world. Record this as P2 metadata ambiguity for a
future source-schema/seed-regeneration slice; do not block Goal158 or mutate v1 source records.

## Product gap

Goal157 starts and interacts inside one generated map. The generated plan and package already contain:

```text
multiple generated regions
multiple generated maps
directed ProceduralRegionConnection records
generated actors/items/encounters/quests in different regions
```

But the real player cannot leave the starting generated map.

`IGameRuntime` currently supports:

```text
Move
Interact
UseItem
UseAbility
Wait
ChooseDialogueOption
```

`DefaultGameRuntime.Interact()` can open dialogue or show text, but cannot change maps.

The old `ConnectedWorldTravelAcceptanceService` contains private Goal007 fixture-specific travel logic.
It is proof history, not a reusable Runtime primitive and not bound to generated plans.

## Goal158 product outcome

```text
generated source plan connections
→ deterministic travel-gate overlay on generated maps
→ actual player package
→ Runtime Start
→ generated interaction in origin region
→ path to generated travel gate
→ Interact gate
→ atomic map transition
→ generated interaction in destination region
→ replay
→ state save/load
→ cached standalone frames/facts
```

This must work from the current generated source without regenerating or rewriting Goal156/157
sidecars.

## Core architecture

### Source lane

Unchanged:

```text
seeded_generated_project_source_v1
plan/rule/tiny/MVP/overlay/base byte-identical
```

Travel gates are not added to source artifacts.

### Lane A — accepted mechanics compatibility

Unchanged from Goal157:

```text
module-composed generated base
baseline start map
AcceptedMechanics/Social qualification
```

### Lane B — player travel package

```text
Lane A module-composed package
+ deterministic generated travel overlay derived from validated plan connections
+ generated start map activation
+ project identity
```

Primary project hashes, FinalStateHash and RuntimeFrames come from the complete generated travel route.

## Non-goals

Do not add:

```text
new FeatureModules
new parameter types
public GamePackage schema fields
Unity project/host changes
seed regeneration
region-selection UI
infinite/chunk streaming
travel economy or random encounters
provider/LLM/Lua/media execution
clean-machine packaging
```

One generic data-driven map-transition Runtime contract is authorized.


## Mandatory architecture review

Read at most 16 primary files:

```text
AGENTS.md
docs/GOAL_DESIGN_QUALITY_POLICY.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/UNITY_EXECUTION_POLICY.md
RuntimeContracts.cs
DefaultGameRuntime.cs
ConnectedWorldTravelAcceptanceService.cs
ProceduralGameKernelModels.cs
GeneratedPackageMvpService.cs
SeededGeneratedProjectSourceService.cs
GameProjectGeneratedWorldActivationService.cs
GameProjectBuildAndQualificationService.cs
GameProjectGeneratedWorldSummaryService.cs
GameProjectBuildHistoryReader.cs
UnifiedGameProjectWorkspaceController.cs
ProjectsPageControl.cs
```

Before production edits write:

```text
.llmgc/procedural/goal-158-generated-region-travel-runtime-and-standalone-vertical-slice/architecture-review.json
```

Required resolved sections:

```text
goal157IndependentAudit
existingTravelProofVersusRuntimeGap
genericMapTransitionContract
runtimeAtomicity
validatedPlanAuthority
regionMapBinding
travelGateOverlay
deterministicGatePlacement
routePlanner
originInteraction
destinationInteraction
replayAndRoundtrip
twoLanePreservation
historyAndUiTruth
standaloneCorrelation
legacyCompatibility
rollbackMatrix
nonGoals
```

Every section names exact types, inputs, outputs, invariants and behavioral tests.

## A. Generic Runtime map-transition interaction contract

Use the existing arbitrary `ComponentDefinition.Args` surface. Do not add a GamePackage schema field.

Add a shared vocabulary in `LLMGameCreator.Runtime.Abstractions`, naming flexible:

```text
MapTransitionInteractionContract
```

Required constants:

```text
ComponentType = interactable
TransitionKindKey = transitionKind
TransitionKindMap = map
ConnectionIdKey = connectionId
SourceMapIdKey = sourceMapId
DestinationMapIdKey = destinationMapId
DestinationXKey = destinationX
DestinationYKey = destinationY
FromRegionIdKey = fromRegionId
ToRegionIdKey = toRegionId
```

Add one Runtime event at the end of the existing enum without renumbering prior values:

```text
RuntimeEventType.MapChanged
```

No new `PlayerCommandType` is required. Travel is initiated by the ordinary `Interact` command.

### A1. Backward-compatible interaction

When no map-transition argument is present:

```text
existing text interaction behavior unchanged
existing dialogue interaction behavior unchanged
existing no-nearby-object behavior unchanged
```

### A2. Transition detection

When the nearby interactable has:

```text
transitionKind=map
```

require all of:

```text
connectionId nonempty
sourceMapId == state.CurrentMapId
destinationMapId identifies exactly one package map
destinationX/destinationY parse as invariant integers
destination coordinates are inside destination map
destination tile exists and is walkable
```

`fromRegionId/toRegionId` are required nonempty correlation metadata.

### A3. Atomic success

Only after every validation passes:

```text
emit InteractionTriggered for gate entity
set CurrentMapId = destinationMapId
set PlayerPosition = destination coordinates
emit MapChanged targeting destinationMapId
```

`MapChanged.Args`:

```text
connectionId
sourceMapId
destinationMapId
fromRegionId
toRegionId
destinationX
destinationY
```

Return `Success=true`.

### A4. Atomic failure

For incomplete, malformed or invalid transition args:

```text
Success=false
state CurrentMapId/position/flags unchanged
no InteractionTriggered success event
no MapChanged event
one Error event with causal code/message
```

Required error categories:

```text
map_transition.contract_incomplete
map_transition.source_map_mismatch
map_transition.destination_map_missing
map_transition.destination_position_invalid
map_transition.destination_tile_blocked
```

Do not catch and convert unrelated ordinary interactions into travel.

### A5. Direct Runtime tests

Prove directly against `DefaultGameRuntime`:

```text
successful map transition
missing destination
blocked destination
wrong source map
malformed coordinates
legacy text interaction
legacy dialogue interaction
no nearby interaction
existing RuntimeEventType numeric values unchanged
```

## B. Validated generated plan authority

Extend `SeededGeneratedProjectSourceValidationResult` with non-persisted typed fields:

```text
RegeneratedPlan
RegeneratedPlanJson
ResolvedGenerationOptions
```

These fields are populated only when strict Goal157 full-chain validation passes.

Do not deserialize the plan a second time elsewhere as independent authority.

The persisted `seeded_generated_project_source_v1` file and all required sidecars remain byte-identical.

## C. Region-to-map binding

Create a generic Application service, naming flexible:

```text
GeneratedWorldRegionMapBindingService
```

Inputs:

```text
strict validated RegeneratedPlan
module-composed compatibility package
validated generated-content records
```

For every plan region require exactly one map binding.

Resolve through generated provenance:

```text
GeneratedContent.Regions.SourceId == plan RegionId
SceneIds contains exactly one package map ID for Goal158
package contains that map exactly once
```

Do not infer map IDs by string concatenation.

For every plan connection require:

```text
unique ConnectionId
FromRegionId exists
ToRegionId exists
from and destination map bindings exist
from != to
```

Required diagnostics:

```text
generated_travel.region_binding_missing
generated_travel.region_binding_ambiguous
generated_travel.connection_duplicate
generated_travel.connection_region_missing
generated_travel.connection_self_loop
generated_travel.destination_map_missing
```

A generated project with no usable inter-region connection cannot claim travel readiness.

## D. Deterministic travel-gate overlay

Create:

```text
GeneratedWorldTravelOverlayService
GeneratedWorldTravelOverlayDocument
GeneratedWorldTravelOverlayResult
```

Inputs:

```text
strict source validation
Lane A module-composed compatibility package
```

Output player package is a clone with deterministic travel additions.

### D1. Travel prototype

Add one namespaced prototype only when absent:

```text
entity_prototype/generated_region_travel_gate
```

It may contain presentation/text metadata but destination-specific args remain on instances.

If the ID exists with an unequal definition, fail collision.

### D2. One gate per directed connection

For every plan connection, add one gate entity to its source map.

Entity ID is deterministic and data-derived, for example:

```text
entity/generated_travel_gate/<stable-id-segment-or-hash-of-connection-id>
```

Do not use array index alone as identity.

Each gate has a local `interactable` component with the exact map-transition contract:

```text
transitionKind=map
connectionId=<plan connection id>
sourceMapId=<bound source map>
destinationMapId=<bound destination map>
destinationX=<destination map start X>
destinationY=<destination map start Y>
fromRegionId=<plan from region>
toRegionId=<plan to region>
text=<human destination title>
```

### D3. Gate placement

Placement is deterministic and data-driven.

For each source map:

1. collect walkable cells;
2. exclude map start position;
3. exclude blocked/collidable entity cells;
4. exclude all existing entity positions to avoid ambiguous interactions;
5. require a reachable adjacent player cell for each gate;
6. sort candidate cells deterministically;
7. assign sorted outgoing connections to sorted safe cells.

Do not hardcode a maximum gate count.

If there are insufficient safe cells, fail:

```text
generated_travel.gate_placement_insufficient
```

Do not resize or rewrite source maps in Goal158.

### D4. Controlled delta proof

The Lane B travel package may differ from Lane A only by:

```text
Manifest.StartMapId
the single travel-gate prototype
travel-gate entities appended to generated maps
project identity fields after identity overlay
```

All pre-existing Game/AssetCatalog/ScriptCatalog/GeneratedContent records remain canonical-equal.

Create typed fingerprints for:

```text
pre-travel records
travel prototype
each gate entity
post-travel generated maps
```

Any other delta fails:

```text
generated_travel.unexpected_package_delta
generated_travel.id_collision
```

### D5. Overlay hash

Document includes:

```text
schemaVersion=generated_world_travel_overlay_v1
sourceRequestSha256
planSha256
compatibilityPackageSha256
travelOverlaySha256
playerCompositionPackageSha256
regionBindingCount
connectionCount
gateCount
prototypeFingerprint
gateFingerprints[]
mapFingerprintsBefore[]
mapFingerprintsAfter[]
```

No timestamps or paths.


## E. Generated travel route planner

Create an Application-layer planner:

```text
GeneratedWorldTravelRoutePlanner
```

It plans commands; `IGameRuntime` remains execution authority.

Inputs:

```text
strict source plan
travel-overlaid player package
generated start region/map
```

### E1. Destination selection

Choose a deterministic reachable destination region that:

```text
differs from start region
is reachable through directed plan connections
has at least one generated non-gate interactable entity
```

Selection order:

```text
shortest connection count
then destination region ID ordinal
then route connection IDs ordinal
```

No fixed region IDs.

Require at least one connection. Multi-hop routes are supported.

### E2. Grid path planning

For each map segment, compute a deterministic shortest movement path from current position to a
walkable cell adjacent to the target entity/gate.

Use package data:

```text
map bounds
tile walkability
collidable entity positions
```

Do not mutate Runtime state while planning.

Direction tie order is fixed and documented, for example:

```text
Up, Left, Right, Down
```

The specific order is not important; determinism is.

Failures:

```text
generated_travel.target_unreachable
generated_travel.gate_unreachable
generated_travel.destination_interactable_missing
```

### E3. Planned loop

The complete planned route:

```text
Start generated start map
navigate/interact with a generated non-gate origin entity
for each route connection:
  navigate adjacent to matching gate
  Interact gate and require MapChanged
navigate/interact with generated non-gate destination entity
```

Goal157's initial generated interaction remains part of the product route but is no longer assumed to
be exactly one Move Right.

## F. Runtime-owned route execution

Extend or compose the Goal157 activation service cleanly. Naming flexible:

```text
GameProjectGeneratedRegionTravelActivationService
```

Use real `IGameRuntime`.

For every planned movement command require:

```text
command succeeds
actual position equals planned position
actual map equals planned map
```

For every gate interaction require:

```text
InteractionTriggered targets planned gate
exactly one MapChanged event
MapChanged args correlate with plan connection/bindings
actual state enters destination map at expected coordinates
```

For origin/destination interactions require target belongs to generated source content and is not a
travel gate.

### F1. Typed route summary

Create:

```text
GameProjectGeneratedRegionTravelSummary
```

Fields:

```text
Present
Passed
OriginRegionId
OriginRegionTitle
OriginMapId
OriginMapTitle
DestinationRegionId
DestinationRegionTitle
DestinationMapId
DestinationMapTitle
ConnectionIds[]
TransitionCount
VisitedRegionIds[]
VisitedMapIds[]
MovementCommandCount
OriginInteractionObserved
TravelGateInteractionsPassed
DestinationInteractionObserved
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
Начальный регион
Взаимодействие в начальном регионе
Переход между регионами
Посещено регионов
Регион назначения
Взаимодействие после перехода
Повтор маршрута
Сохранение состояния
```

No IDs/hashes in human facts.

### F2. Runtime frames

Frames are ordered and indexed from zero.

Categories:

```text
generated_start
generated_origin_interaction
generated_navigation
generated_travel
generated_destination_interaction
```

Action IDs may contain connection/entity IDs only in technical payload, not user facts.

Primary build `RuntimeFrames` uses this complete route.

### F3. Replay

Start a fresh state and execute the exact planned route.

Require identical:

```text
per-command success
ordered positions/maps
InteractionTriggered targets
MapChanged events/args
visited regions/maps
final state hash
```

### F4. State roundtrip

Serialize and restore the final map state using existing `IRuntimeStateSerializer`.

Require exact:

```text
CurrentMapId
PlayerPosition
Mode
Flags
canonical map-state hash
```

### F5. Negative route tests

Reject:

```text
missing gate
wrong gate connection args
unreachable gate
invalid destination map
Runtime suppresses MapChanged
Runtime transitions to wrong map
destination has no generated interaction
replay diverges
roundtrip diverges
```

## G. Goal157 activation compatibility

Preserve `GameProjectGeneratedWorldActivationSummary` as the Goal157 start/first-interaction proof.

For a Goal158 build:

```text
GeneratedWorldActivation remains Passed
GeneratedRegionTravel is separately Passed
```

The new complete route may internally supersede the old three-frame script, but old Goal157 build
history remains readable.

Do not reinterpret an old Goal157 history row as travel-ready.

## H. Build integration

### H1. Generated project flow

`GameProjectBuildAndQualificationService`:

1. strict source validation;
2. Lane A FeatureModule compatibility qualification;
3. accepted mechanics/social projection;
4. build deterministic travel overlay over Lane A composition package;
5. activate generated start map and project identity;
6. execute complete generated region route;
7. validate final player package;
8. activate package transactionally;
9. persist history.

### H2. Primary authority

For Goal158 generated builds:

```text
PackageSha256 = final identity-overlaid travel package
CompositionPackageSha256 = pre-identity travel-overlaid player composition
FinalStateHash = complete route final state
RuntimeFrames = complete route frames
CheckpointReloadPassed = complete route state roundtrip
FullReplayEquivalent = complete route replay
ActionBindingPassed = every planned Runtime command/event/map binding correlated
```

Lane A typed compatibility remains unchanged.

### H3. Legacy and core-only

Legacy project:

```text
existing single-lane behavior and hashes unchanged
no travel overlay/summary
```

Generated `core_only`:

```text
travel build GREEN
AcceptedMechanics Passed=false
GeneratedWorldActivation Passed=true
GeneratedRegionTravel Passed=true
RC READY not claimed unless accepted-mechanics contract is complete
```

### H4. Rollback

Failure in overlay/planning/Runtime/replay/roundtrip/final validation preserves:

```text
package.json bytes
authoring
source/sidecars
old history
old activation/travel summary
old RC record
```

No staging leak.

## I. History and current truth

Add to build result/history/snapshot:

```text
GeneratedWorldTravelOverlay
GeneratedRegionTravel
```

History eligibility for a new Goal158 generated row requires:

```text
GeneratedWorld Passed and preserved
GeneratedWorldActivation Passed
GeneratedRegionTravel Passed
route ReplayEquivalent
route StateRoundtripPassed
current package/composition/final hashes match document
```

Old Goal157 history:

```text
source + start activation may restore
GeneratedRegionTravel absent
must not claim TRAVEL_CURRENT
```

### I1. Generated card states

Extend generated-world status vocabulary:

```text
SOURCE_READY
START_CURRENT
TRAVEL_CURRENT
LAST_SUCCESS
INVALID
```

Rules:

```text
SOURCE_READY:
  source valid, no current activation evidence

START_CURRENT:
  current Goal157 activation exists, no current travel evidence

TRAVEL_CURRENT:
  current source/authoring/package history and travel evidence all match

LAST_SUCCESS:
  source valid, prior travel build exists but authoring/build no longer current

INVALID:
  source/package/history correlation invalid
```

`BUILD_CURRENT` historical value remains readable and maps to `START_CURRENT`, never `TRAVEL_CURRENT`.

### I2. Card rows

Existing rows plus:

```text
Начальный регион
Переход между регионами
Посещено регионов
Регион назначения
Взаимодействие после перехода
```

Status text:

```text
TRAVEL_CURRENT → Сгенерированный маршрут проверен
START_CURRENT → Игровой старт проверен; переходы ещё не подтверждены
```

No IDs/hashes/paths.

Technical details show:

```text
travel overlay SHA
connection/gate/transition counts
origin/destination region/map IDs
route connection IDs
primary travel final hash
```

## J. Standalone and RC

Standalone request uses Goal158 primary hashes, route frames and facts.

Actual payload must prove:

```text
start map generated
at least one MapChanged route frame/event representation
final map is generated destination map
final state hash equals route final hash
origin/destination interaction facts present
accepted-mechanics facts present for all-selectable
Release Candidate=готов
```

RC record remains current and uses primary travel package/composition/final hashes.

Do not modify Unity host code.

## K. Portable copy

A complete built/standalone Goal158 project copied to another short path restores without execution:

```text
strict source CURRENT
GeneratedWorld TRAVEL_CURRENT
Goal157 activation Passed
GeneratedRegionTravel Passed
AcceptedMechanics compatibility Passed when all-selectable
RC CURRENT
current package start map generated
```

No build/Runtime/Unity starts.


## L. Required behavioral tests

Create at least 44 Goal158 tests; at least 38 behavioral.

### Runtime contract

1. direct valid transition changes map and coordinates;
2. MapChanged event has exact data-derived args;
3. wrong source map fails atomically;
4. missing destination map fails atomically;
5. malformed coordinates fail atomically;
6. blocked destination fails atomically;
7. incomplete transition contract fails atomically;
8. legacy text interaction unchanged;
9. legacy dialogue interaction unchanged;
10. no-nearby interaction unchanged;
11. existing RuntimeEventType values remain stable and MapChanged is additive.

### Binding and travel overlay

12. every generated region has one exact map binding;
13. missing/ambiguous binding rejected;
14. duplicate/missing/self-loop connection rejected;
15. one gate created for every directed connection;
16. gate IDs deterministic and unique;
17. gate args match exact connection/map/region bindings;
18. gate positions are walkable/distinct/reachable;
19. existing entity positions are not reused;
20. insufficient safe cells rejected;
21. travel overlay deterministic under repeated build;
22. all pre-existing records canonical-equal;
23. only prototype/gate/start-map controlled deltas accepted;
24. collision with unequal travel prototype/entity rejected;
25. Goal156/157 source sidecars byte-identical after travel overlay.

### Route planner and Runtime

26. destination selection is deterministic and reachable;
27. route uses plan connection IDs, not inferred strings;
28. origin generated interaction succeeds;
29. path to gate is shortest/deterministic;
30. gate interaction emits MapChanged;
31. actual state enters planned destination;
32. destination generated interaction succeeds;
33. visited generated maps/regions contain at least two distinct values;
34. multi-hop route works when nearest eligible destination requires it;
35. unreachable gate fails;
36. missing destination interactable fails;
37. wrong Runtime map transition fails correlation;
38. suppressed MapChanged fails;
39. full route replay equivalent;
40. full route state roundtrip exact.

### Build/history/UI

41. all-selectable generated build GREEN with travel;
42. accepted mechanics/social Lane A remain GREEN;
43. primary hashes/frames belong to travel route;
44. repeat build deterministic;
45. fresh reopen restores TRAVEL_CURRENT;
46. old Goal157 history restores START_CURRENT, not TRAVEL_CURRENT;
47. authoring change yields LAST_SUCCESS;
48. travel failure rolls back package/history/source/RC;
49. core-only travel build GREEN and AcceptedMechanics false;
50. legacy single-lane hashes unchanged;
51. generated card contains travel rows;
52. card has no IDs/hashes/paths;
53. technical details contain travel overlay/route fields.

### Standalone/portable

54. exactly one hidden smoke;
55. host reused/not rebuilt and Unity zero;
56. actual payload starts generated and ends on generated destination;
57. payload final hash matches complete route;
58. payload contains travel and destination interaction facts;
59. payload accepted facts and RC ready present;
60. RC record primary hashes current;
61. portable copy restores TRAVEL_CURRENT/accepted/RC without execution.

### Regressions

62. Goal157 provenance/two-lane/start activation regressions GREEN;
63. Goal156 creation/custom-base regressions GREEN;
64. Goal155A/155 regressions GREEN;
65. Goal154D/153C/150/149 regressions GREEN;
66. existing DefaultGameRuntime move/interact tests GREEN;
67. historical ConnectedWorldTravel acceptance tests GREEN;
68. procedural generation/preview regressions GREEN;
69. Goal142 and goal148-manual byte-identical.

Do not claim counts unless tests are discovered/executed.

## M. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal158"
# require >=44 total and >=38 behavioral

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
dotnet test ... --filter "FullyQualifiedName~ConnectedWorldTravel"
dotnet test ... --filter "FullyQualifiedName~ProceduralGameKernel"
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
portable-copy proof.

Run artifact scope last.

Do not run:

```text
full suite
85-case historical closure
all-ProductSmoke
Unity host build
visible automatic standalone launch
unchanged failed command retry
timeout escalation
```

## N. Evidence

Create exactly 13 files in each mirrored root:

```text
goal158-dashboard.json
architecture-review.json
goal157-independent-audit-intake.json
runtime-map-transition-contract-proof.json
region-binding-gate-overlay-proof.json
travel-overlay-delta-proof.json
generated-route-runtime-proof.json
route-replay-roundtrip-proof.json
accepted-mechanics-compatibility-proof.json
generated-travel-history-ui-proof.json
standalone-portability-proof.json
artifact-scope-proof.json
goal158-report.md
```

Roots:

```text
.llmgc/procedural/goal-158-generated-region-travel-runtime-and-standalone-vertical-slice/
.llmgc/exports/goal-158-generated-region-travel-runtime-and-standalone-vertical-slice/
```

Twins byte-identical.

### Dashboard

```text
status
candidateStatus
goal158TestsDiscovered
goal158BehavioralTestsPassed

goal157IndependentAuditPassed

runtimeMapTransitionPassed
runtimeMapTransitionAtomicFailurePassed
legacyRuntimeInteractionPassed
mapChangedEventPassed

regionBindingPassed
planConnectionValidationPassed
travelGateCount
planConnectionCount
gateCountMatchesConnections
gatePlacementPassed
travelOverlayDeterministic
travelOverlayControlledDeltaPassed
sourceSidecarsByteIdentical

originInteractionPassed
transitionCount
visitedRegionCount
visitedMapCount
destinationInteractionPassed
routeMovementCommandCount
routeReplayEquivalent
routeStateRoundtripPassed

allSelectableTravelBuildPassed
allSelectableAcceptedMechanicsPassed
allSelectableSocialPassed
coreOnlyTravelBuildPassed
legacySingleLaneRegressionPassed
repeatBuildDeterministic
freshReopenTravelCurrent
oldGoal157HistoryStartOnlyPassed
rollbackPassed

hostCacheKey
hostReused
hostRebuilt
hostFileSetHashUnchanged
unityProcessStartCount
hiddenSmokeInvocationCount
hiddenSmokePassed
standaloneSelfChecksPassed
actualPayloadTravelFactsPassed
actualPayloadAcceptedFactsPassed
releaseCandidateRecordCurrent
portableCopyTravelCurrent

goal157RegressionPassed
goal156RegressionPassed
goal155aRegressionPassed
goal155RegressionPassed
goal154dRegressionPassed
goal153cRegressionPassed
goal150RegressionPassed
goal149RegressionPassed
defaultRuntimeRegressionPassed
connectedWorldRegressionPassed
proceduralLegacyRegressionPassed
goal142SourceByteIdentical
sourceGoal148ByteIdentical

artifactScopeViolationCount
goal158Accepted=false
goal158ManualReviewRequired=false
goal158IndependentAuditRequired=true
```

No required GREEN field may be null/PARTIAL/NOT_EXECUTED/unverified constant.

## O. State and docs

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
docs/manual-acceptance/goal157-generated-world-runtime-start-activation.md
```

Create:

```text
docs/manual-acceptance/goal158-generated-region-travel.md
```

No human gate.

Required GREEN state:

```text
goal157IndependentAuditResult=GREEN_ACCEPTABLE_CANDIDATE_AT_8939AEA0
goal157IndependentAuditPassed=true
goal157IndependentAuditRequired=false

goal158ImplementationStatus=GREEN
goal158CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal158Accepted=false
goal158AcceptedByHuman=false
goal158AcceptedByCodex=false
goal158ManualReviewRequired=false
goal158ManualGateReady=false
goal158IndependentAuditRequired=true

goal158RuntimeMapTransitionPassed=true
goal158GeneratedTravelOverlayPassed=true
goal158GeneratedRegionTravelPassed=true
goal158RouteReplayPassed=true
goal158RouteStateRoundtripPassed=true
goal158AcceptedMechanicsCompatibilityPassed=true
goal158GeneratedTravelHistoryPassed=true
goal158HostReused=true
goal158HostRebuilt=false
goal158UnityProcessStartCount=0
goal158HiddenSmokeInvocationCount=1
goal158PortableCopyPassed=true
goal158ArtifactScopeViolationCount=0

nextAction=independent_goal158_audit_and_plan_transactional_seed_regeneration
```

Release risk statement:

```text
Generated projects now start in generated content and traverse at least one generated plan connection
through a generic Runtime-owned map transition, with deterministic replay/save/standalone truth.
Seed regeneration remains the next explicit product decision.
```

Record preset-label ambiguity as P2.

## P. Text integrity

Scan changed/task/evidence/docs:

```text
valid UTF-8
no NUL
no forbidden C0 except CR/LF/TAB
no mojibake markers
no escaped Cyrillic where policy forbids
no absolute disposable paths in evidence
```

Historical artifacts immutable.


## Q. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal158-generated-region-travel.ps1
.devflow/scripts/run-goal158-generated-region-travel.cmd

src/LLMGameCreator.Runtime.Abstractions/RuntimeContracts.cs
src/LLMGameCreator.Runtime/DefaultGameRuntime.cs

src/LLMGameCreator.Application/Generation/Procedural/SeededGeneratedProjectModels.cs
src/LLMGameCreator.Application/Generation/Procedural/SeededGeneratedProjectSourceService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedWorldRegionMapBindingService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedWorldTravelOverlayService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedWorldTravelRoutePlanner.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectGeneratedWorldActivationService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectGeneratedRegionTravelActivationService.cs

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildHistoryReader.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectGeneratedWorldSummaryService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectAcceptedMechanicsSummaryService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectReleaseCandidateRecordService.cs

src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs

tests/LLMGameCreator.Tests/Application/Goal158/Goal158RuntimeMapTransitionTests.cs
tests/LLMGameCreator.Tests/Application/Goal158/Goal158RegionBindingAndGateOverlayTests.cs
tests/LLMGameCreator.Tests/Application/Goal158/Goal158TravelRouteRuntimeTests.cs
tests/LLMGameCreator.Tests/Application/Goal158/Goal158TravelBuildCompatibilityTests.cs
tests/LLMGameCreator.Tests/Application/Goal158/Goal158HistoryUiRollbackTests.cs
tests/LLMGameCreator.Tests/Application/Goal158/Goal158StandaloneAndPortabilityTests.cs
tests/LLMGameCreator.Tests/Application/Goal157/Goal157RuntimeActivationTests.cs
tests/LLMGameCreator.Tests/Application/Goal157/Goal157WorkspaceUiTests.cs
tests/LLMGameCreator.Tests/Application/Goal157/Goal157StandaloneAndPortabilityTests.cs
tests/LLMGameCreator.Tests/RuntimeUnifiedBridgeTests.cs
tests/LLMGameCreator.Tests/SmokeTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/ROADMAP_TO_FULL_GENERATOR.md
docs/manual-acceptance/goal157-generated-world-runtime-start-activation.md
docs/manual-acceptance/goal158-generated-region-travel.md

docs/agent-tasks/goal-158-generated-region-travel-runtime-and-standalone-vertical-slice/
.llmgc/procedural/goal-158-generated-region-travel-runtime-and-standalone-vertical-slice/
.llmgc/exports/goal-158-generated-region-travel-runtime-and-standalone-vertical-slice/
```

One exact additional Runtime/Application/test path may be added only after a concrete compile/test
failure and with recorded reason.

Forbidden without a newly reproduced Goal158 P0/P1:

```text
src/LLMGameCreator.GamePackage/**
catalogs/feature-modules/**
unity/**
samples/**
generator-library/**
provider/**
LLM/**
RAG/**
```

No FeatureModule catalog or source-sidecar generation changes are expected.

## R. Command budget

```text
read-first/architecture review: 12 minutes
Runtime map-transition contract: 14 minutes
binding/gate overlay: 20 minutes
route planner/Runtime execution: 22 minutes
build/history/UI/standalone integration: 20 minutes
behavioral tests: 28 minutes
real matrix + one smoke: 12 minutes
regressions/evidence/docs/artifact scope: 18 minutes
target wall clock: 125 minutes
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
P0/P1 fixed inside Goal158
P2/P3 debt only
do not defer evidence/docs/artifact scope
```

## S. Publication

Create exactly one final commit:

```text
GREEN Goal 158 generated region travel Runtime and standalone vertical slice
```

or honest BLOCKED/FAILED.

Codex performs standard push.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three Goal158 task files tracked
Goal156/157 source sidecars unchanged
Goal142 and goal148-manual unchanged
Unity starts=0
HostRebuilt=false
hidden smoke=1 only on GREEN
Goal154 acceptance unchanged
Goal155 RC milestone passed
Goal157 accepted=false/no human gate
Goal158 accepted=false/no human gate
```

## T. GREEN criteria

```text
Goal157 independent audit recorded GREEN
Goal158 tests >=44 / behavioral >=38 / all pass

generic map transition works via Interact
legacy interactions remain compatible
invalid transitions are state/event atomic
MapChanged is additive and correlated

every plan region/connection has validated bindings
one deterministic gate per directed connection
gate placement safe/reachable
travel overlay only adds authorized records
source v1 sidecars byte-identical

real Runtime origin interaction
at least one real generated region transition
real destination interaction
visited generated regions/maps >=2
route replay equivalent
route state roundtrip exact

Lane A accepted mechanics/social remain GREEN
Lane B primary hashes/frames use complete travel route
all-selectable/core-only generated builds GREEN
legacy single-lane hashes preserved
build/repeat/reopen/history/UI truthful
old Goal157 history cannot claim TRAVEL_CURRENT
rollback preserves last success/source/RC

one cached hidden standalone smoke
HostReused=true / HostRebuilt=false / Unity=0
actual payload route/final hash/travel facts correlate
accepted facts and RC current
portable copy TRAVEL_CURRENT without execution

Goal157/156/155A/155/154D/153C/150/149 and Runtime/connected-world/procedural regressions GREEN
13+13 evidence mirrored
text integrity GREEN
artifact scope 0
goal158CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
one final commit pushed
```

## U. Final report

Return GREEN/BLOCKED/FAILED and include:

- model/reasoning;
- architecture review;
- Goal157 independent-audit intake;
- Goal158 discovered/behavioral counts;
- Runtime map-transition contract/atomic failures;
- region bindings and connection/gate counts;
- gate placement and controlled package delta;
- selected route and visited regions/maps;
- origin/travel/destination Runtime facts;
- replay/state roundtrip;
- Lane A compatibility and Lane B primary hashes;
- all-selectable/core-only/legacy build results;
- history/UI/rollback;
- host/Unity/smoke;
- actual payload/RC/portable copy;
- regressions;
- source/baseline immutability;
- evidence/text/artifact scope;
- state/no-human-gate;
- final SHA/push/HEAD/worktree.
