# Goal 143 Selected Runtime Variant End-to-End PlayerAdapter Handoff

Status: accepted by human handoff
Gate: `selected_runtime_variant_end_to_end_playeradapter_handoff_verification accepted`
Implementation status: GREEN
Accepted: true
Accepted by human: true
Accepted by Codex: false
Raw manual input committed: false
Manual Unity optional: true

## Result

```text
goal142Accepted=true
goal143Accepted=true
selectedCandidateId=minimal-map-game-exploration-resource-focus
selectedRecipeId=exploration_resource_focus
selectedVariantKind=exploration_resource_focus
selectedScore=100
selectedPackageSha256MatchesHandoff=true
selectedFinalStateHashMatches=true
frameCount=15
requestCount=6
snapshotCount=15
runtimeRoutedRequestCount=4
presentationOnlyRequestCount=2
presentationOnlyRuntimeExecutionCount=0
selectedVariantEffectVisible=true
noBalancedBaselineFallback=true
unitySmokePassed=true
runtimeAuthority=true
projectionOnly=false
unityGameplayTruth=false
```

Goal143 consumes only the Goal142 selected handoff and package. The package
SHA-256 is `27b426b087eb6dfd4567facbf76b1463a7ab1a46ff0e834ba849c95aa1858565`.
The repeated Runtime execution ends at
`d7c04179cb76ca48ba9694905e491bead014c0f56f446f66331becd5e3211e54`,
matching the Goal142 selected outcome.

The selected variant effect is visible in the exploration/resource inventory
summary: the player finishes with four apples and four healing potions, while
Goal142 recorded that this inventory summary differs from the balanced baseline.
This proof is not based on package hash inequality alone.

## Operator and player boundaries

- WinForms calls the in-process Application operator and starts no compiler or test process.
- Failed regeneration restores previous Goal143 procedural/export artifacts.
- Unity reads the Goal143 model, frames and handoff only.
- Runtime remains gameplay truth; Unity does not execute or mutate gameplay.

## Evidence

- Normal command: `.devflow\scripts\run-selected-runtime-variant-playeradapter-handoff.cmd`
- Procedural root: `.llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/`
- Export root: `.llmgc/exports/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/`
- Handoff: `.llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/selected-runtime-variant-playeradapter-handoff.json`
- Model: `.llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/selected-runtime-variant-playeradapter-model.json`
- Frames: `.llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/selected-runtime-variant-playeradapter-frames.json`
- Unity smoke: `.llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/unity-selected-runtime-variant-playeradapter-smoke.json`

Goal143 does not authorize `.llmgc/manual/**`, sample mutation, public
GamePackage schema changes, provider/LLM/RAG/media work, Lua/generator-library,
Unity scene/prefab/StreamingAssets/project-settings/packages changes, final art,
final gameplay or release packaging.

## Human decision recorded by Goal144

```text
Я принимаю Goal143 selected_runtime_variant_end_to_end_playeradapter_handoff_verification GREEN. selectedCandidate=minimal-map-game-exploration-resource-focus, selectedVariant=exploration_resource_focus, selectedScore=100, packageHashMatch=true, finalStateHashMatch=true, requestCount=6, snapshotCount=15, frameCount=15, selectedVariantEffectVisible=true, noBalancedBaselineFallback=true, operatorUsesInProcessService=true, operatorStatus=GREEN, unitySmoke=GREEN, projectionOnly=false, runtimeAuthority=true, unityGameplayTruth=false.
```

The raw manual input is not stored under `.llmgc/manual/**`. This committed
document records the bounded acceptance facts with `acceptedByHuman=true` and
`acceptedByCodex=false`.
