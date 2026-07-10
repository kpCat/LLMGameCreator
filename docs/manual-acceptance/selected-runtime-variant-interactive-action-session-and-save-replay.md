# Goal 144 Selected Runtime Variant Interactive Action Session and Save Replay

Status: produced for review
Gate: `selected_runtime_variant_interactive_action_session_and_save_replay_verification required`
Implementation status: GREEN
Accepted: false
Accepted by Codex: false
Manual Unity optional: true

## Result

```text
goal143Accepted=true
goal144Accepted=false
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

Normal command:
`.devflow\scripts\run-selected-runtime-variant-live-session.cmd`

Evidence:
`.llmgc/procedural/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/`

Goal144 does not authorize `.llmgc/manual/**`, sample or public GamePackage
schema mutation, provider/LLM/RAG/media, Lua/generator-library, or Unity gameplay
truth, scenes, prefabs, StreamingAssets, ProjectSettings or Packages changes.
