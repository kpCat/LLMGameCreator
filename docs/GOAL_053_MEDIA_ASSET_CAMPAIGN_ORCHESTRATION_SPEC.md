# Goal 053 — Media Asset Campaign Orchestration And Binding Dry Run

## Summary

Goal 053 is the first full-generator media-campaign slice after the accepted full generator without-media dry run. It accepts Goal 047 by user handoff, then adds deterministic media request/review/promotion/binding proof across the three family dry-runs.

It must not generate real AI media, call providers, access network services, bundle third-party tools or change Runtime/UI/Unity/GamePackage schema.

## Gate

```text
media_asset_campaign_orchestration_verification required
```

## What must become real

The generator can take the Goal 047 family dry-run facts and produce a complete media campaign plan with:

- image, portrait, tile, icon, UI, SFX, ambient loop and music-stinger requests;
- style packs and prompt/input skeletons without final generated prose dependence;
- license/provenance ledger;
- quarantined media candidates;
- review/promotion decisions;
- deterministic fixture media outputs;
- binding manifest to generated family/template/runtime ids;
- export/preview-compatible payload proof;
- invalid/fake/leak diagnostics.

## Design constraints

- BCL-only.
- Application-layer only.
- Deterministic JSON evidence.
- Fixture media may be tiny deterministic placeholder files, but they must be generated from code, small, stable and explicitly marked as fixture assets.
- No real image/audio generation.
- No provider/LLM/RAG/media model calls.
- No network/import.
- No ComfyUI/Fooocus/Stability/Freesound/OpenGameArt integration beyond documented future adapter contracts.
- No Runtime/UI/Unity/GamePackage schema changes.

## Required scenarios

Use the three accepted Goal 043 / Goal 047 families:

```text
map_panel_rpg
survival_sandbox
first_person_grid_dungeon
```

Also include at least one metamodule/world-scale media-volume stress summary based on the existing metamodule/species/kingdom counts where available, without generating 112 real media files.

## Required artifact folder

```text
.llmgc/procedural/goal-053-media-asset-campaign-orchestration/
```

Required compact artifacts:

```text
media-campaign-source-manifest.json
media-slot-catalog.json
media-request-queue.json
media-style-policy.json
media-license-provenance-ledger.json
media-candidate-quarantine.json
media-review-promotion-ledger.json
media-binding-manifest.json
media-fixture-file-inventory.json
preview-export-media-payloads.json
invalid-media-diagnostics-matrix.json
media-asset-campaign-orchestration-report.md
```

## Expected proof

The compact report must contain:

```text
media_asset_campaign_orchestration_verification required
implementationStatus=GREEN|BLOCKED|FAILED
accepted=false
realProviderCalled=false
realMediaGenerationCalled=false
fixtureMediaProduced=true
familyCount=3
bindingManifestPassed=true
licenseLedgerPassed=true
invalidMatrixPassed=true
```

If implementation cannot honestly produce fixture files/bindings across the three families, it must commit/push `BLOCKED`.
