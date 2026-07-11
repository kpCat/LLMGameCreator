# Goal 145 Operator-Selectable Product-Line Runtime Sessions and Cross-Variant Save/Replay Matrix

Status: produced for review
Gate: `operator_selectable_product_line_runtime_sessions_and_cross_variant_save_replay_matrix_verification required`
Implementation status: GREEN
Accepted: false
Accepted by human: false
Accepted by Codex: false
Manual Unity optional: true

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
goal145Accepted=false
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
