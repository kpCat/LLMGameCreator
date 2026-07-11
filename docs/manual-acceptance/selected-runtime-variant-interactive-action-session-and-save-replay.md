# Goal 144 Selected Runtime Variant Interactive Action Session and Save Replay

Status: accepted
Gate: `selected_runtime_variant_interactive_action_session_and_save_replay_verification passed`
Implementation status: GREEN
Accepted: true
Accepted by human: true
Accepted by Codex: false
Raw manual input committed: false
Manual Unity optional: true

## Human acceptance

```text
Я принимаю Goal144 selected_runtime_variant_interactive_action_session_and_save_replay_verification GREEN. selectedCandidate=minimal-map-game-exploration-resource-focus, actionDescriptorCount=14, runtimeRoutedActionDescriptorCount=11, presentationOnlyActionDescriptorCount=3, executedRuntimeActionCount=11, actionDescriptorExecutionBindingPassed=true, harvestTarget=node/apple_tree, basicAttackTarget=goblin, invalidActionStateUnchanged=true, checkpointReloadByReplayPassed=true, checkpointReplayedActionCount=8, finalReplayActionCount=13, replayEvidenceFrozenBeforeContinuation=true, fullReplayEquivalent=true, finalStateHashMatchesGoal142=true, operatorStatus=GREEN, unitySmoke=GREEN, projectionOnly=false, runtimeAuthority=true, unityGameplayTruth=false.
```

## Result

```text
goal143Accepted=true
goal144Accepted=true
goal144AcceptedByHuman=true
goal144AcceptedByCodex=false
goal144ActionExecutionBindingCorrected=true
actionDescriptorExecutionBindingPassed=true
allRuntimeActionTargetsMatchExecutedSteps=true
allRuntimeActionCommandKindsMatchExecutedSteps=true
harvestActionTargetId=node/apple_tree
harvestExecutedTargetId=node/apple_tree
basicAttackActionTargetId=goblin
basicAttackExecutedTargetId=goblin
checkpointReplayedActionCount=8
finalReplayActionCount=13
replayEvidenceFrozenBeforeContinuation=true
selectedRuntimeVariantInteractiveSession=true
selectedCandidateId=minimal-map-game-exploration-resource-focus
selectedVariantKind=exploration_resource_focus
selectedPackageSha256=27b426b087eb6dfd4567facbf76b1463a7ab1a46ff0e834ba849c95aa1858565
actionDescriptorCount=14
runtimeRoutedActionDescriptorCount=11
presentationOnlyActionDescriptorCount=3
executedRuntimeActionCount=11
invalidActionStateUnchanged=true
checkpointReloadByReplayPassed=true
checkpointStateHashRestored=true
fullReplayEquivalent=true
finalStateHashMatchesGoal142=true
selectedVariantEffectVisible=true
noBalancedBaselineFallback=true
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
unitySmokePassed=true
```

The checkpoint contains the correlated action journal, expected action index and
expected hash. Reload starts a fresh Runtime session and replays the journal; no
opaque Runtime object is used as the persisted checkpoint authority.

Goal144A corrected the descriptor/execution binding and replay evidence before
manual review. The action selector shows action id, target id, canonical step
and route. Goal144 is accepted by the exact human statement above; Codex did not
accept it and raw manual input is not tracked under `.llmgc/manual/**`.

Normal command:
`.devflow\scripts\run-selected-runtime-variant-live-session.cmd`

Evidence:
`.llmgc/procedural/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/`

Goal144 does not authorize `.llmgc/manual/**`, sample or public GamePackage
schema mutation, provider/LLM/RAG/media, Lua/generator-library, or Unity gameplay
truth, scenes, prefabs, StreamingAssets, ProjectSettings or Packages changes.
