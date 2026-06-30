# Goal 055 — Media-Bound Playable Review Package Smoke Spec

## Goal id

`goal_055_media_bound_playable_review_package_smoke`

## Gate

`media_bound_playable_review_package_verification required`

## Strategic purpose

Goal 054 proved deterministic physical PNG/WAV/bundle fixture materialization. Goal 055 must prove those physical media artifacts can be staged into a review/playable package and consumed by a bounded player-facing path across the three generated families:

- `map_panel_rpg`
- `survival_sandbox`
- `first_person_grid_dungeon`

This goal is a bridge from media materialization to media-bound playable review. It must not become a real provider/media-generation task.

## Required proof chain

```text
Goal 047 full generator without media dry-run
 -> Goal 053 media campaign governance
 -> Goal 054 physical media materialization
 -> Goal 055 media-bound review/playable package staging
 -> Unity-compatible media load/binding proof
 -> three-family media-bound smoke evidence
```

## Non-goals

- No real media provider calls.
- No network imports.
- No LLM/RAG/provider calls.
- No Lua execution.
- No public GamePackage schema changes.
- No Runtime/Runtime.Abstractions changes unless the task stops and reports BLOCKED.
- No WinForms/UI changes.
- No generator-library changes.
- No new NuGet dependencies.

## Acceptable Unity scope

Goal 055 may make a narrow change under `unity/LLMGameCreatorAlpha/Assets/**` only if it is required to load/validate the media-bound manifest or emit deterministic media-load proof lines. The change must be small, defensive, and backwards-compatible with existing Alpha scenarios.

Heavy Unity build/player outputs must not be tracked unless existing repo policy already tracks a compact review artifact. Compact logs, manifests and proof JSON under `.llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/` are allowed.

## Expected artifacts

Under `.llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/`:

- `source-manifest.json`
- `media-bound-review-package-manifest.json`
- `streaming-assets-media-manifest.json`
- `media-bound-preview-payloads.json`
- `unity-media-load-contract.json`
- `unity-media-load-proof-map-panel-rpg.json`
- `unity-media-load-proof-survival-sandbox.json`
- `unity-media-load-proof-first-person-grid-dungeon.json`
- `media-bound-family-smoke-matrix.json`
- `invalid-media-bound-package-diagnostics-matrix.json`
- `artifact-scope-report.json`
- `media-bound-playable-review-package-report.md`
- compact package/readme/checklist files as needed
- physical staged PNG/WAV/bundle copies with stable names and SHA-256 hashes

## Required report markers

The final report markdown must include:

```text
media_bound_playable_review_package_verification required
implementationStatus: GREEN|BLOCKED|FAILED
accepted: false
Goal054AcceptedByUserHandoff: true
providerCalls: false
networkImports: false
llmCalls: false
luaExecuted: false
publicGamePackageSchemaChanged: false
```
