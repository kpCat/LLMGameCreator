# Goal 169B — Package-Bound Event Proof, Nested Combat Replay & Payload Closure

## Identity

- Task ID: `goal-169b-package-bound-event-proof-nested-combat-replay-and-payload-closure`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `d012b8ac40a9c6ded421ec4bbcbddd9cc3b8d385`
- Required base message: `GREEN Goal 169A strict replay v7 correlation and post fix smoke closure`

New isolated Codex dialog. Complete instruction source.

```text
Model: GPT-5.6 Terra
Reasoning: High
```

Goal169A is substantive and must be preserved. Goal169B closes four remaining proof-boundary P1s
before any new product feature.

## Publication contract

- Plan approved by launching this task.
- No extra confirmation or manual testing.
- No intermediate commits.
- Exactly one final GREEN/BLOCKED/FAILED commit.
- Always `push origin/main`; never leave publication to the user.

## Initial worktree

Only these unpacked files may initially be untracked:

```text
docs/agent-tasks/goal-169b-package-bound-event-proof-nested-combat-replay-and-payload-closure/GOAL.md
docs/agent-tasks/goal-169b-package-bound-event-proof-nested-combat-replay-and-payload-closure/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-169b-package-bound-event-proof-nested-combat-replay-and-payload-closure/README.md
```

Require `HEAD == origin/main == d012b8ac40a9c6ded421ec4bbcbddd9cc3b8d385`, branch `main`, no other changes.
Never reset, revert, stash, merge or rebase.

## Budgets

```text
Unity starts=0
Unity host builds=0
Goal169/Goal169A smoke reruns=0
new Goal169B cached hidden smoke=exactly 1
Goal169B retry=0
manual/visible launch=0
max testhost=2
```

Old Goal169 and Goal169A immutable outputs are retained historical evidence.

---

## A. Scaffold classification

Before production edits create:

```text
.llmgc/procedural/goal-169b-package-bound-event-proof-nested-combat-replay-and-payload-closure/scaffold-classification.json
```

Classify every touched Goal169A file as KEEP_AND_COMPLETE / REFACTOR / REPLACE / REMOVE_AS_UNUSED.
At minimum classify replay, correlation, qualification, models, binding, overlay, history reader,
build service, fingerprints, migration, smoke/tests/runner/evidence.

## B. Audit intake

Record:

```text
goal169aImplementationCommit=d012b8ac40a9c6ded421ec4bbcbddd9cc3b8d385
goal169aIndependentAuditResult=BLOCKED_AT_D012B8AC
P1NestedCombatReplay=nested_exact_combat_collapsed_to_synthetic_frame
P1QualificationIdentity=qualification_signature_frame_ids_not_equal_to_binding_inventory_ids
P1ActualPackageCorrelation=persisted_graph_checked_against_sha_string_not_actual_definitions
P1MigrationDefinitions=prototype_and_map_entity_not_in_compatibility
P1PayloadProof=payload_not_self_contained_for_route_and_signature_truth
P1AbsentProfile=empty_event_graph_not_strictly_sealed
```

Preserve profile-neutral relationships, effect-neutral combat, events/UI, Challenge provenance,
v7/v6, save/migration, RC/portable, Goal169A 60/60, Goal169 108/108 and protected bytes.

---

## C. Nested exact-combat replay

Current event prerequisite uses `GeneratedCampaignExactCombatRouteService.Execute`, then emits one
synthetic `Prerequisite.ExactCombatVictory` frame. This loses actual commands/events/action
fingerprints and intermediate encounter states.

Add a typed nested combat trace containing canonical:

```text
sequence
command type and identity
qualified descriptor fingerprint
ability definition SHA
observed effect class/fingerprint
map/gameplay event sequence
before/after encounter-state hash
turn/round
progress/outcome
```

Either expand every nested combat step into event frames or persist a typed trace whose hashes are
independently recomputed by qualification, history and payload validation. A single final-state
frame is insufficient.

Both independent RESOLUTION replays must compare nested command/event/action/effect/state chains.
Reject same-final-state fixtures with changed command order, ability, inserted utility no-op,
Runtime event, effect fingerprint or turn chain.

---

## D. Exact ID-set correlation

Require canonical set equality across:

```text
Overlay.Bindings IDs
Overlay.Inventory IDs
Summary.EventInventory IDs
EventQualifications IDs
ReplaySignature event/route/replay keys
RuntimeFrame event/route/replay/sequence keys
```

For every inventory row require exactly one full-identity binding, one matching qualification, four
matching signatures and all matching frames.

Require:

```text
qualification ID/kind/relationship/branch == inventory/binding
RuntimeCommandCount == owned frame count
sum signature frame counts == owned frame count
no duplicate route/replay/sequence
no gaps
no orphan IDs
```

Coordinated tamper tests must rename qualification+signatures+frames and recompute final/signature
hashes; reject. Also reject swaps, ghost qualifications, route duplication and frame reassignment.

---

## E. Actual-package event definition authority

Current correlation receives only package SHA. Add a package-backed validator using the exact loaded
`GamePackageDefinition`.

Extend event binding/inventory with canonical hashes:

```text
DialogueDefinitionSha256
InteractionDefinitionSha256
EntityPrototypeDefinitionSha256
MapEntityDefinitionSha256
SourceQuestDefinitionSha256 (when used)
ChallengeEncounterDefinitionSha256 (when used)
```

Persist and verify map ID/position/prototype, interactable dialogue/interaction references,
resolution choice ID, requirements hash, effects hash and event metadata hash.
`EventSemanticFingerprint` must include these exact definition hashes.

Use one package-backed validator in:

```text
build qualification
current history/project selection
campaign session truth
regeneration semantic validation
save migration inventory loading
RC/portable current recovery
```

Do not trust persisted Passed/count/hash/fingerprint fields without recomputation.

Keep actual package unchanged and coherently rewrite history IDs/hashes/qualifications: reject.
Modify a disposable actual package while retaining old history: reject.

---

## F. Strict event-absent profile

When no branch can produce an event require exact:

```text
Present=false, Passed=true, Status=ABSENT
all counts=0
explicit exact empty overlay policy
bindings/inventory/fingerprints empty
empty inventory hash exact
branch matrix exact
frames/signatures/qualifications empty
actual package has zero generated regional-event records
exact package SHA and canonical empty final state
```

Reject ghost overlay or package event records.

---

## G. Migration definition compatibility

`EventDefinitionsExact` currently omits prototype/map-entity behavior.

Extend definition inventory with exact `entity_prototype` and exact map entity kind if needed.
Resolved event compatibility requires exact source/target:

```text
semantic fingerprint
dialogue
interaction
entity prototype
map entity
quest/encounter when used
prerequisite/reward/region fingerprints
```

Define explicit placement-only policy. Never preserve a resolution flag when prototype,
interactable reference, dialogue effect or event semantics changed.

Extend typed migration facts with:

```text
DefinitionCorrelationPassed
MarkerDefinitionPreserved
PrototypeDefinitionPreserved
DialogueDefinitionPreserved
InteractionDefinitionPreserved
PlacementChanged
PlacementPolicy
```

Test prototype/component, map entity reference, interaction target and dialogue effect changes;
all incompatible cases drop without ghost flag/marker/action/dialogue.

Public persisted save schema remains unchanged.

---

## H. Self-contained standalone payload proof

Current project frames encode event+command+replay but omit RouteKind and SequenceIndex. Payload
alone cannot distinguish LOCKED_PROBE replay 1 from RESOLUTION replay 1.

Using existing payload-supported fields, create a versioned unambiguous frame identity:

```text
RegionalEventId | RouteKind | ReplayIndex | SequenceIndex | CommandIdentity
```

Keep human title/parser compatibility.

Expose a strict payload signature authority using existing generic payload/support-file mechanisms,
without modifying Unity or standalone implementation. It must contain:

```text
strict schema
package/final/inventory hashes
six event IDs
24 event/route/replay signatures
component hashes and frame counts
nested combat trace hashes
```

Goal169B smoke must close/discard in-memory build objects, then read only immutable pointer/run,
selected history, actual package, player-adapter model/frames, strict replay payload authority and
RC record. Reconstruct event/route/replay/sequence ownership and compare all 24 payload signatures
with persisted history.

Payload tamper tests: route, replay, sequence gap, event ID, nested trace removal, signature hash,
history/payload mismatch.

---

## I. Retained outputs and one new smoke

Require byte-identical old Goal169 and Goal169A run/pointer/history/payload/RC, cached host,
Goal142, Goal148 and generation sidecars.

After all non-smoke checks pass, run exactly one Goal169B cached hidden smoke:

```text
retry=0
HostReused=true
HostRebuilt=false
Unity=0
exit=0
all self-checks pass
payload-only strict proof GREEN
RC record/configuration CURRENT
portable all-selectable CURRENT
portable core-only no false RC readiness
```

Failure means honest BLOCKED/FAILED; no retry.

---

## J. Required tests

Create >=52 Goal169B tests, >=46 behavioral.

Mandatory coverage:

```text
nested Support/Challenge combat traces
combat command/event/action/effect/state replay comparison
six same-final adversarial combat mismatches
exact ID set equality and coordinated rename/swap/ghost rejection
actual-package dialogue/interaction/prototype/map-entity/reference correlation
history/package coordinated tamper rejection
strict empty-event profile and ghost rejection
prototype/map-entity/dialogue/interaction migration compatibility
typed migration facts/counts/no ghosts
payload frame parser and route/replay/sequence ownership
24 payload signatures and history match
payload tamper matrix
retained Goal169/169A immutability
one Goal169B smoke, RC, portable
```

Regressions:

```text
Goal169A 60/60 with smoke disabled
Goal169 108/108 with smoke disabled
Goal168 focused
Goal167=94, Goal166=59, Goal165=55, Goal164=61
Goal163–157 focused
GeneratedCampaign, GeneratedGameplaySave, RuntimeSimulator,
UnifiedGameProjectWorkspace, coordinator, standalone filters
```

No source-string-only assertion counts as behavioral proof.

Do not run full suite, Goal168 85-case closure, all-ProductSmoke, Unity host build, old smokes,
more than one Goal169B smoke or retry.

---

## K. Evidence

Create exactly 15 byte-identical files in each root:

```text
.llmgc/procedural/goal-169b-package-bound-event-proof-nested-combat-replay-and-payload-closure/
.llmgc/exports/goal-169b-package-bound-event-proof-nested-combat-replay-and-payload-closure/
```

Files:

```text
goal169b-dashboard.json
architecture-review.json
scaffold-classification.json
goal169a-independent-audit-finding.json
nested-combat-replay-proof.json
identity-set-correlation-proof.json
package-definition-correlation-proof.json
event-absent-proof.json
migration-definition-proof.json
payload-frame-contract-proof.json
payload-only-standalone-proof.json
retained-runs-immutability-proof.json
regression-immutability-proof.json
artifact-scope-proof.json
goal169b-report.md
```

All values typed/measured. Dashboard must include blocker closures, nested trace counts, ID/package
correlation, absent profile, migration facts, payload routes/signatures, retained hashes, smoke/RC/
portable, regressions, protected bytes, scope and accepted=false.

---

## L. Docs/state

Update current generator state/index/queue/gates/risks/debt/strategy/roadmap and Goal169A acceptance.
Create:

```text
docs/manual-acceptance/goal169b-package-bound-event-proof-payload-closure.md
```

GREEN state records Goal169A `BLOCKED_AT_D012B8AC`, all six findings `closed_by_goal169b`,
Goal169B `GREEN_ACCEPTABLE_CANDIDATE`, accepted=false, no human gate, independent audit required,
one smoke, zero retry, host reused, Unity0, portable GREEN, scope0.

---

## M. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal169b-package-bound-event-proof.ps1
.devflow/scripts/run-goal169b-package-bound-event-proof.cmd
regional event models/replay/correlation/qualification/binding/overlay
GeneratedCampaignExactCombatRouteService and internal result models
GeneratedGameplayDefinitionFingerprintService
GeneratedGameplaySaveMigrationService/Models
regeneration seal/validator
GameProjectBuildAndQualificationService
GameProjectBuildHistoryReader
GameProjectWorkspaceModels
GeneratedCampaignSessionTruthService
tests Goal169B/Goal169A/Goal169 and exact focused regressions
listed generator docs/manual acceptance
task/evidence roots
```

One exact additional application/test path only after concrete compile/test failure with reason in
architecture and scope evidence.

Forbidden:

```text
Runtime/Runtime.Abstractions
GamePackage/Domain
FeatureModule catalogs
ProceduralGameKernel*
GeneratedPackageMvp*
source sidecars
Unity
ProjectStandaloneBuild implementation
RC implementation
cached host
retained Goal169/Goal169A outputs
```

Scope violations=0.

---

## N. Command budget

```text
classification/architecture 14m
nested combat trace/replay 22m
ID/package correlation 26m
absent/migration 18m
payload authority 22m
tests 30m
regressions/evidence/docs/scope 20m
one smoke/RC/portable 16m
target 145m
```

No smoke before all non-smoke work is GREEN.

---

## O. Publication and GREEN criteria

GREEN commit:

```text
GREEN Goal 169B package bound event proof nested combat replay and payload closure
```

or honest BLOCKED/FAILED.

Final requirements:

```text
one commit from d012b8ac40a9c6ded421ec4bbcbddd9cc3b8d385
push origin/main
HEAD==origin/main
clean worktree
three task files tracked
old Goal169/Goal169A outputs byte-identical
Goal169B smoke=1, retry=0, host reused/no rebuild, Unity=0
protected source/host bytes unchanged
Goal169/169A/169B accepted=false
no human gate
```

GREEN requires every audit finding closed, nested combat replay exact, coordinated identity/history
forgeries rejected, actual package definitions correlated, strict absent profile, migration marker/
prototype truth, self-contained payload-only replay proof, RC/portable GREEN, >=52/>=46 tests all
pass, old regressions GREEN, 15+15 evidence, text integrity and scope0.

## P. Final report

Return GREEN/BLOCKED/FAILED and include model/base, classifications, each blocker/closure, nested
combat traces and adversarial results, ID/package correlation/tampers, absent profile, migration
definition facts, payload-only route/signature proof, retained immutability, smoke/RC/portable,
tests/evidence/scope, final SHA/message/push, HEAD==origin/main, clean worktree and explicit
confirmation Codex committed and pushed for every status.
