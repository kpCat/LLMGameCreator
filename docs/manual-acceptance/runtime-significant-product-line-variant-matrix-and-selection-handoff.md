# Runtime-Significant Product-Line Variant Matrix And Selection Handoff

Status: produced for review
Gate: `runtime_significant_product_line_variant_matrix_and_selection_handoff_verification required`
Implementation status: GREEN
Accepted: false
Accepted by Codex: false

Goal142 builds a deterministic runtime-significant product-line variant matrix
from the read-only template `samples/minimal-map-game/package.json`. It does
not accept Goal141 and does not mutate the source template.

## Evidence

- Normal command: `.devflow\scripts\run-product-line-runtime-variant-matrix.cmd`
- Evidence root: `.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/`
- Export root: `.llmgc/exports/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/`
- Dashboard: `.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/product-line-runtime-variant-matrix-dashboard.json`
- Matrix result: `.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/product-line-runtime-variant-matrix-result.json`
- Distinctness proof: `.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/product-line-runtime-variant-distinctness-proof.json`
- Selected handoff: `.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/selected-runtime-variant/selected-runtime-variant-handoff.json`

## Result

```text
matrixStatus=GREEN
candidateCount=4
passedCandidateCount=4
failedCandidateCount=0
runtimeSignificantCandidateCount=4
distinctFinalStateHashCount=4
selectedCandidateId=minimal-map-game-exploration-resource-focus
selectedVariantKind=exploration_resource_focus
selectedScore=100
sourceTemplateUnmodified=true
accepted=false
```

The selected variant is runtime-significant because its structured resource and
transaction mutations produce a distinct Runtime-backed final state hash. The
selection is not metadata-only.

## Candidate Summary

- `minimal-map-game-balanced-baseline`: validation GREEN; Runtime roundtrip GREEN; baseline comparison candidate.
- `minimal-map-game-alchemy-focus`: validation GREEN; Runtime roundtrip GREEN; inventory and recipe outputs differ from baseline.
- `minimal-map-game-combat-focus`: validation GREEN; Runtime roundtrip GREEN; encounter and ability effects differ from baseline.
- `minimal-map-game-exploration-resource-focus`: validation GREEN; Runtime roundtrip GREEN; loot, resource-node and transaction outputs differ from baseline; selected candidate.

## Boundaries

Goal142 does not authorize sample mutation, `.llmgc/manual/**`, public
GamePackage schema changes, Generation, AssetPipeline, Scripting/Lua,
provider/media/LLM/RAG, generator-library, Unity scene/prefab/project-settings
packages/StreamingAssets, final art/atlas, final gameplay or release packaging.
