# Goal 145 Operator-Selectable Product-Line Runtime Sessions and Cross-Variant Save/Replay Matrix

Status: accepted by human handoff
Gate: `operator_selectable_product_line_runtime_sessions_and_cross_variant_save_replay_matrix_verification accepted`
Implementation status: GREEN
Accepted: true
Accepted by human: true
Accepted by Codex: false
Raw manual input committed: false
Manual Unity optional: true

## Human decision

```text
Я принимаю Goal145 operator_selectable_product_line_runtime_sessions_and_cross_variant_save_replay_matrix_verification GREEN. candidateCount=4, passedCandidateCount=4, distinctFinalStateHashCount=4, defaultSelection=minimal-map-game-exploration-resource-focus, combatSelectionStable=true, combatPackageSha256=4528af180259dd0d3dd11c97de4048ed4ee43ea2c77209cf5b311061ea702497, programmaticBindInvokesSelectionCount=0, programmaticRestoreInvokesSelectionCount=0, operatorCommitInvokesSelectionCount=1, maximumSelectionCallbackDepth=1, allCandidateCheckpointReloadsPassed=true, allCandidateFullReplaysEquivalent=true, allCandidateActionBindingsPassed=true, allFocusEffectsObserved=true, operatorStatus=GREEN, unitySmoke=GREEN, projectionOnly=false, runtimeAuthority=true, unityGameplayTruth=false.
```

## Result

```text
goal144Accepted=true
goal144AcceptedByHuman=true
goal144AcceptedByCodex=false
productLineInteractiveSessionMatrix=true
candidateCount=4
passedCandidateCount=4
failedCandidateCount=0
runtimeEvaluatedCandidateCount=4
runtimeMutatedCandidateCount=3
controlCandidateCount=1
distinctFinalStateHashCount=4
allCandidatePackageHashesDistinct=true
allCandidateCheckpointReloadsPassed=true
allCandidateFullReplaysEquivalent=true
allCandidateActionBindingsPassed=true
sameRuntimeServiceUsedForAllCandidates=true
sameCanonicalActionPlanUsedForAllCandidates=true
allFocusEffectsObserved=true
operatorSelectableCandidateCount=4
activeSelectedCandidateId=minimal-map-game-exploration-resource-focus
crossCandidateCheckpointRejected=true
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
unitySmokePassed=true
goal145Accepted=true
goal145AcceptedByHuman=true
goal145AcceptedByCodex=false
rawManualInputNotCommitted=true
```

Fresh semantic comparisons against the balanced Runtime control:

- alchemy: healing potion quantity 4 instead of 3, with remaining red herbs and water;
- combat: goblin health 10 instead of 8 after the same basic attack step;
- exploration/resource: apple/log/healing-potion quantities 4/2/4 instead of 3/1/3.

The default active candidate comes from the accepted Goal142 selected handoff.
The operator may select any passing candidate; changing selection resets the
in-memory session and checkpoint. Cross-candidate checkpoint replay is rejected.
WinForms uses in-process Application/Runtime services, and Unity is a read-only
consumer of Goal145 evidence.

Normal command:
`.devflow\scripts\run-product-line-interactive-session-matrix.cmd`

Evidence:
`.llmgc/procedural/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/`

Goal145 does not authorize `.llmgc/manual/**`, sample or public GamePackage
schema mutation, provider/LLM/RAG/media, Lua/generator-library, or Unity gameplay
truth, scenes, prefabs, StreamingAssets, ProjectSettings or Packages changes.

## Goal 145A selector lifecycle hotfix

The WinForms candidate combo now handles only real operator commits through
`SelectionChangeCommitted`; guarded programmatic `DataSource` binding and
`SelectedValue` restoration invoke selection logic zero times. One combat
operator commit is applied exactly once with maximum callback depth 1, remains
selected through session/action/checkpoint/replay/matrix refreshes and starts
the combat package SHA. Candidate changes reset prior session, checkpoint, last
action and last replay state. The repository owner accepted Goal145 through the
exact decision above; `acceptedByCodex=false` remains preserved.
