# Goal 054 — Media Materialization And Media-Bound Review Package Smoke

## Goal

Turn Goal 053 media campaign/binding evidence into concrete deterministic media artifacts and a media-bound preview/export/review payload across the three current generated game families.

This goal must prove that the generator can move from governed media requests/bindings to physical media files and media-bound consumer manifests without calling external providers and without changing public GamePackage schema.

## Expected gate

```text
media_materialization_review_package_verification required
```

The gate must stay required/not passed inside this goal.

## What should become real

```text
Goal 047 dry-run + Goal 053 media campaign
 -> deterministic media materialization queue
 -> valid fixture PNG/WAV files or clearly typed deterministic fixture media files
 -> hashes/provenance/license sidecars
 -> media-bound preview/export payloads
 -> review-package or review-bundle manifest
 -> multi-family smoke proof
```

## Strong requirement: not paper-only

At least one physical deterministic media file per promoted binding must be written and hashed. Preferred fixture formats:

- PNG for image slots;
- WAV PCM for audio slots.

If valid PNG/WAV is not safely implementable within the allowed scope, Codex may instead produce typed deterministic fixture media files only if it reports BLOCKED and explains why real media materialization could not be proven.

## Families

Must cover the current three families:

- `map_panel_rpg`
- `survival_sandbox`
- `first_person_grid_dungeon`

## No provider calls

No image/audio provider, network import, model call, ComfyUI/Fooocus/Stability/Freesound/OpenGameArt/Pixabay call, web download, or runtime LLM/RAG may be used.

## Expected artifacts

Under:

```text
.llmgc/procedural/goal-054-media-materialization-review-package/
```

Required compact artifacts:

- `source-manifest.json`
- `media-materialization-queue.json`
- `materialized-media-inventory.json`
- `media-provenance-license-ledger.json`
- `media-binding-validation.json`
- `media-review-package-manifest.json`
- `preview-export-media-payloads.json`
- `family-media-smoke-map-panel-rpg.json`
- `family-media-smoke-survival-sandbox.json`
- `family-media-smoke-first-person-grid-dungeon.json`
- `invalid-media-materialization-matrix.json`
- `media-materialization-review-package-report.md`

Also expected:

- a `media/` or `review-package/media/` subfolder with deterministic physical media files;
- sidecar hash/provenance records for all physical files;
- no absolute paths in JSON/report evidence.

## Allowed implementation style

Small Application-layer components are preferred:

- source loader for Goal 053/047 evidence;
- materialization plan builder;
- deterministic image fixture writer;
- deterministic audio fixture writer;
- media review package builder;
- validator;
- evidence writer.

Keep code split into reasonably small files. Do not create one huge class.

## Optional Unity review proof

If existing Unity Alpha/review-package infrastructure can consume the produced media manifest through a narrow bounded seam and produce deterministic proof logs without broad Unity refactoring, Codex may include an optional media-aware review-package smoke. This is optional, not required for GREEN unless the task file makes it safely provable.

If Unity becomes risky, do not touch Unity. Prove the media-bound review bundle at Application-layer instead.
