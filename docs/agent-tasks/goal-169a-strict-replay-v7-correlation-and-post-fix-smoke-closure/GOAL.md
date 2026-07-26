# Goal 169A — Strict Replay, v7 Correlation & Post-Fix Smoke Closure

## Identity

- Task ID: `goal-169a-strict-replay-v7-correlation-and-post-fix-smoke-closure`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `f861229c0202b4b372127cb25e2c135345f0b0a6`
- Required base message: `BLOCKED Goal 169 profile neutral relationships and reactive regional events`

This file is the complete instruction source for a new isolated Codex dialog.

```text
Model: GPT-5.6 Terra
Reasoning: High
```

Goal169 is a substantial implementation. Preserve it and close its remaining truth gaps before
starting another product layer.

## Publication contract

- Plan is approved by launching this task.
- Do not ask for confirmation or manual testing.
- No intermediate commits.
- Publish exactly one honest GREEN/BLOCKED/FAILED commit.
- Push `origin/main` for every result.
- Never leave commit or push to the user.

## Initial worktree

After unpacking, only these untracked files are allowed:

```text
docs/agent-tasks/goal-169a-strict-replay-v7-correlation-and-post-fix-smoke-closure/GOAL.md
docs/agent-tasks/goal-169a-strict-replay-v7-correlation-and-post-fix-smoke-closure/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-169a-strict-replay-v7-correlation-and-post-fix-smoke-closure/README.md
```

Require:

```text
HEAD == origin/main == f861229c0202b4b372127cb25e2c135345f0b0a6
branch=main
no other changes
```

Never reset, revert, stash, merge or rebase.

## Process budgets

```text
Unity Editor starts: 0
Unity host builds: 0
Goal169 smoke reruns: 0
new Goal169A cached hidden smoke: exactly 1
Goal169A smoke retries: 0
visible launch/manual gate: 0
maximum concurrent testhost: 2
```

The old Goal169 smoke remains immutable historical evidence. The new Goal169A smoke proves the
post-smoke code plus this continuation's replay/history fixes.

---

# A. Scaffold classification

Before production edits create:

```text
.llmgc/procedural/goal-169a-strict-replay-v7-correlation-and-post-fix-smoke-closure/scaffold-classification.json
```

Classify every touched Goal169 file:

```text
KEEP_AND_COMPLETE
REFACTOR
REPLACE
REMOVE_AS_UNUSED
```

At minimum classify regional event models/binding/overlay/qualification, history reader, migration,
Goal169 smoke/tests/runner/evidence. Record a concrete reason for every REPLACE/REMOVE.

# B. Independent-audit intake

Record:

```text
goal169ImplementationCommit=f861229c0202b4b372127cb25e2c135345f0b0a6
goal169OriginalStatus=BLOCKED
goal169OriginalSmokeBlocker=payload_move_command_not_explicit
goal169IndependentAuditResult=BLOCKED_AT_F861229C
goal169P1Replay=final_state_only_replay_truth
goal169P1History=event_overlay_inventory_branch_qualification_not_strictly_correlated
goal169P1ChallengeRegion=challenge_event_uses_relationship_home_region
goal169P1InventoryMigration=incomplete_semantic_inventory_and_unused_typed_migration_fact
goal169P1RelationshipHistory=qualified_arc_count_not_required
goal169SmokeProofGap=post_smoke_Move_fix_not_in_current_payload
```

Preserve: six relationship profiles, unavailable Runtime count zero, exact health/stat/status combat,
honest NOT_EVALUATED_AT_BUILD save truth, regional events/UI/v7/save/regeneration/migration,
RC/portable, source/host immutability.

# C. Strict regional-event replay

Current code accepts replay from two Passed routes plus equal FinalStateHash. This is insufficient.

Add typed route identity:

```text
LOCKED_PROBE
RESOLUTION
```

For every event execute independently:

```text
LOCKED_PROBE replay 1 from fresh Runtime.Start
LOCKED_PROBE replay 2 from fresh Runtime.Start
RESOLUTION replay 1 from fresh Runtime.Start
RESOLUTION replay 2 from fresh Runtime.Start
```

Extend typed frames/proofs to capture:

```text
route kind, replay index, sequence index
event ID
command type/hash
map event hash
gameplay event hash
status before/after
before/after stable state hash
available choice IDs hash
observed reputation delta
observed resolution flag
passed
```

Persist typed per-event/per-route replay signatures.

Replay is GREEN only when both independent traces have exact:

```text
frame count/order
commands
map/gameplay Runtime events
statuses
before/after states
available choices
relationship/event flags
quest states
encounter result
reputation sequence/delta
final state
```

Add adversarial tests rejecting same final state with different command order, missing Interact,
different Runtime event, different choices, different intermediate reputation/status, or bare
direction title versus `Move.<Direction>`.

Diagnostics must identify the mismatched dimension.

# D. Strict v7 history truth

For event-bearing v7 require and recompute:

```text
summary.Overlay exists and Passed
Overlay.OutputPackageSha256 == entry.PackageSha256
RegionalEventOverlaySha256 == canonical hash(Overlay)
EventInventory canonical-equal Overlay.Inventory
RegionalEventInventorySha256 == canonical hash(EventInventory)
Overlay.InventorySha256 == RegionalEventInventorySha256
EventCount == inventory == bindings == qualifications
```

Do not accept merely nonempty hashes.

For every relationship BranchQualification:

```text
Available SUPPORT   -> exactly one SUPPORT_GRATITUDE
Available CHALLENGE -> exactly one CHALLENGE_AFTERMATH
Available REFUSE    -> exactly one REFUSAL_FALLOUT
Available=false     -> zero matching events
```

Require exact one-to-one correlation across:

```text
relationship branch matrix
overlay bindings
overlay inventory
summary inventory
event qualifications
runtime frames/replay proofs
```

Recompute per-kind counts. Reject duplicate/orphan/missing events.

For v7 relationship history require:

```text
ArcQuestCount == QualifiedArcQuestCount
branch counts/matrix hash exact
Support arc lengths match assigned quest counts
```

The BLOCKED Goal169 v7 row must not become strict-current via default values. Add a narrow proof
schema/version field if needed, without changing public GamePackage or save schema.

Physical tamper matrix must reject changes to hashes, event kind/branch/relationship, actor/faction,
region/map, prototype/entity/interaction, dialogue/flag, placement, prerequisite/reward fingerprint,
qualification ID/kind, replay signature/frame ownership and QualifiedArcQuestCount.

# E. Exact Challenge event region

Regional event binding must receive strict generation provenance or an equally strict typed
encounter-region authority.

For Challenge:

1. Resolve exact `ChallengeEncounterId`.
2. Resolve exact generated `sourceEncounterSeedId`.
3. Resolve one ProceduralEncounterSeed and its RegionId.
4. Resolve one current generated map.
5. Place event in that encounter region.

Home-region fallback is allowed only when exact generated region provenance is absent and the home
region/map is uniquely resolvable. Ambiguous/conflicting provenance is failure.

Persist:

```text
ChallengeEncounterId
ChallengeEncounterSourceId
TargetRegionDerivation=EXACT_CHALLENGE_ENCOUNTER_REGION|RELATIONSHIP_HOME_FALLBACK
TargetRegionFingerprint
```

Test same-region, cross-region, fallback, ambiguity, mismatch and reordered determinism.

# F. Expanded semantic event inventory

Extend event inventory with:

```text
RegionalEventId, EventKind, RelationshipId, RelationshipBranch
ActorSeedId, ActorEntityId, FactionId
RegionId, MapId
EntityPrototypeId, MapEntityId, InteractionId
DialogueId, ResolutionFlagId
SourceQuestId, ChallengeEncounterId
TargetRegionDerivation
X, Y
PrerequisiteFingerprint
RewardDerivationFingerprint
EventSemanticFingerprint
```

`EventSemanticFingerprint` is the canonical hash of all semantic fields except itself.

Inventory must be uniquely derived from overlay bindings and be used by history, regeneration seals,
semantic validation and migration. Tampering any field rejects before commit.

# G. Typed migration truth

`GeneratedCampaignRegionalEventMigrationFact` must become real production truth.

Migration compatibility must use exact source and target selected v7 event inventories, not only
dialogue canonical equality.

Compatible resolved event requires exact semantic identity/fingerprints for event kind, relationship,
branch, actor/faction/region, event definitions, prerequisite and reward derivation.

Extend typed fact:

```text
RegionalEventId
Compatible
ResolutionFlagPreserved
StatusReset
SourceEventFingerprint
TargetEventFingerprint
DroppedReason
```

Expose facts in an application-level preview/result without changing public persisted save schema.

Test same-world preserve, incompatible drop, world-migration reset/preserve policy, active dialogue
reset and no ghost flag/marker/action/dialogue. Typed facts must agree with aggregate counts.

# H. Retained smoke and one post-fix smoke

Historical Goal169 retained evidence must stay byte-identical:

```text
run
current pointer
standalone history
build history
payload
RC
```

It records standalone GREEN, self-checks 5/5, 84 bare directions and 0 explicit `Move.*`.
Do not rewrite or reinterpret it.

Keep current source behavior:

```text
PlayerCommand.Move(direction)
frame title = Move.<Direction>
```

Non-smoke tests require explicit prefix and reject bare Up/Down/Left/Right.

After all non-smoke fixes and regressions, run exactly one new Goal169A cached hidden smoke:

```text
retry=0
HostReused=true
HostRebuilt=false
Unity=0
exit=0
self-checks all pass
```

New payload requires:

```text
explicit Move.* count > 0
bare direction count = 0
Interact/OpenDialogue/ChooseDialogueOption present
LOCKED_PROBE replay 1+2
RESOLUTION replay 1+2
strict replay signatures exact
regional event facts exact
package/final/history hashes exact
RC CURRENT
portable all-selectable CURRENT
portable core-only no false RC readiness
```

Failure of this one smoke means honest BLOCKED/FAILED; no retry.

# I. Build and primary truth

When events exist, primary history/player payload uses strict event final hash, typed frames and strict
replay signature. Never derive primary replay flags from an unmeasured boolean.

Keep history v7 when backward-compatible additive strict fields suffice. Genuine v6 remains
REGIONAL_EVENTS_PENDING/PROJECT_NOT_READY.

# J. Tests

Create at least 48 Goal169A tests; at least 42 behavioral.

Mandatory groups:

```text
strict locked/resolution independent replay
all replay dimension comparisons
adversarial same-final-state divergence rejection
v7 hash recomputation and branch-event graph
binding/inventory/qualification/frame one-to-one correlation
full tamper matrix
QualifiedArcQuestCount rejection
same/cross/fallback/ambiguous Challenge region
expanded inventory semantic hash/seal
typed migration preserve/reset/drop/no-ghost
retained Goal169 evidence immutability
post-fix explicit Move payload
one Goal169A smoke, RC, portable
```

Regressions:

```text
Goal169 focused 108/108 with Goal169 smoke disabled
Goal168 focused
Goal167 94/94
Goal166 59/59
Goal165 55/55
Goal164 61/61
Goal163–157 focused
GeneratedCampaign
GeneratedGameplaySave
RuntimeSimulator
UnifiedGameProjectWorkspace
GameProjectOperationCoordinator
ProjectStandaloneBuild filters
```

No source-string-only assertion counts as behavioral proof.

Do not run full suite, 85-case historical closure, all-ProductSmoke, Unity host build, Goal169 smoke,
more than one Goal169A smoke or a retry.

# K. Evidence

Create exactly 15 byte-identical files in each root:

```text
.llmgc/procedural/goal-169a-strict-replay-v7-correlation-and-post-fix-smoke-closure/
.llmgc/exports/goal-169a-strict-replay-v7-correlation-and-post-fix-smoke-closure/
```

Files:

```text
goal169a-dashboard.json
architecture-review.json
scaffold-classification.json
goal169-independent-audit-finding.json
retained-goal169-smoke-intake.json
strict-event-replay-proof.json
v7-event-correlation-proof.json
challenge-region-proof.json
expanded-event-inventory-proof.json
typed-event-migration-proof.json
post-fix-standalone-proof.json
rc-portability-proof.json
regression-immutability-proof.json
artifact-scope-proof.json
goal169a-report.md
```

Dashboard must include blocker closure flags, replay counts/signatures, all correlation/tamper results,
region derivations, inventory/migration facts, retained-byte equality, smoke counts, explicit/bare Move
counts, host/Unity/RC/portable, regressions/immutability/scope and `accepted=false`.

Every GREEN field comes from typed capture, not constants.

# L. Docs/state

Update generator state/queue/gates/risks/debt/strategy/roadmap/context and Goal169 manual acceptance.
Create:

```text
docs/manual-acceptance/goal169a-strict-replay-v7-correlation-post-fix-smoke.md
```

GREEN state must record Goal169 `BLOCKED_AT_F861229C`, all blockers
`closed_by_goal169a`, Goal169A `GREEN_ACCEPTABLE_CANDIDATE`, accepted=false, no human gate, one smoke,
zero retry, host reused, Unity0 and independent audit required.

# M. Artifact scope

Allowed initially:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal169a-strict-event-closure.ps1
.devflow/scripts/run-goal169a-strict-event-closure.cmd
src/LLMGameCreator.Application/Generation/Procedural/GeneratedCampaignRegionalEvent*.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectGeneratedCampaignRegionalEventQualificationService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedCampaignRelationshipModels.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedCampaignRelationshipBindingService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectGeneratedCampaignRelationshipQualificationService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegeneration*.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplayDefinitionFingerprintService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplaySaveMigrationService.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedGameplaySaveModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProject*.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs
tests/LLMGameCreator.Tests/Application/Goal169A/**
tests/LLMGameCreator.Tests/Application/Goal169/**
required focused regression test files
docs generator state/queue/gates/risks/debt/strategy/roadmap/context/manual acceptance
docs/agent-tasks/goal-169a-strict-replay-v7-correlation-and-post-fix-smoke-closure/**
.llmgc/procedural/goal-169a-strict-replay-v7-correlation-and-post-fix-smoke-closure/**
.llmgc/exports/goal-169a-strict-replay-v7-correlation-and-post-fix-smoke-closure/**
```

One exact additional existing path is allowed only after a concrete compile/test failure with reason
recorded in architecture/scope evidence.

Forbidden:

```text
Runtime/Runtime.Abstractions
GamePackage/Domain
FeatureModule catalogs
ProceduralGameKernel*
GeneratedPackageMvp*
source generation sidecars
Unity
standalone implementation
RC implementation
cached host
retained Goal169 run/pointer/history/payload/RC
```

Artifact scope violations must be zero.

# N. Command budget

```text
classification/architecture: 14m
strict replay: 22m
v7 correlation/tampers: 24m
Challenge region/inventory: 16m
typed migration: 18m
tests: 28m
regressions/evidence/docs/scope: 20m
one smoke/RC/portable: 16m
target: 135m
```

No smoke before all non-smoke work is GREEN.

# O. Publication

Final commit message on GREEN:

```text
GREEN Goal 169A strict replay v7 correlation and post fix smoke closure
```

or honest BLOCKED/FAILED.

Required final:

```text
exactly one commit from f861229c0202b4b372127cb25e2c135345f0b0a6
push origin/main
HEAD == origin/main
worktree clean
three task files tracked
old Goal169 retained evidence byte-identical
Goal169A smoke=1, retry=0
HostReused=true, HostRebuilt=false, Unity=0
Goal142/Goal148/source sidecars/cached host unchanged
Goal169/Goal169A accepted=false
no human gate
```

# P. GREEN criteria

```text
all independent-audit blockers closed
strict two-by-two route replay and adversarial rejection
v7 hashes and graph correlation exact
QualifiedArcQuestCount exact
Challenge encounter region exact
expanded inventory sealed
typed migration facts drive compatibility
no ghost event
retained Goal169 evidence immutable
new smoke explicit Move > 0 and bare directions = 0
RC/portable current
Goal169A >=48 / >=42 behavioral / all pass
Goal169 108/108 without old smoke
required regressions GREEN
15+15 evidence, text integrity, scope 0
one final commit pushed
```

# Q. Final report

Return GREEN/BLOCKED/FAILED and include:

- model/reasoning and base;
- scaffold classifications;
- every audit blocker and closure;
- replay route counts/signatures and adversarial tests;
- strict v7 correlation/tamper matrix;
- Challenge region derivation;
- expanded inventory;
- typed migration facts;
- retained Goal169 immutability;
- new smoke/Move/host/Unity/retry/RC/portable;
- tests/regressions/evidence/scope;
- final SHA/message/push;
- HEAD==origin/main and clean worktree;
- explicit confirmation that Codex committed and pushed for any status.
