# Codex Task — Pseudo-3D Visual Recipe Proof

## Task ID

`pseudo3d-visual-recipe-proof-v1`

## Goal

Prove that deterministic visual recipes can produce pseudo-3D presentation metadata for buildings, vegetation, rocks and surface textures without invoking external providers or changing Unity runtime.

This should be a data/manifest proof, not a full renderer.

## Read first

- `docs/proposals/PSEUDO3D_ASSET_PRESENTATION_CONTRACTS.md`
- `docs/proposals/VISUAL_WORLD_GRAMMAR_AND_PSEUDO3D_GENERATION.md`
- `docs/proposals/PROCEDURAL_VISUAL_PART_PACKS.md`
- Current media asset campaign docs/code.
- Current Unity Alpha media-bound package docs if relevant.

## Allowed scope

Prefer candidate-owned paths:

```text
src/LLMGameCreator.Application/Design/Pseudo3DVisualPresentation/**
tests/LLMGameCreator.Tests/Application/Pseudo3DVisualPresentation/**
docs/candidates/pseudo3d-visual-presentation/**
```

## Forbidden scope

Do not:

- change Unity runtime/player;
- add real rendering dependencies;
- integrate providers;
- change public GamePackage schema;
- add large UI;
- attempt full 3D model support;
- add git/branch/push/rebase/cherry-pick instructions.

## Required concepts

- `SurfaceTextureContract`
- `FacadeContract`
- `BillboardContract`
- `BillboardActorContract`
- `GridRaycastPresentationContract`
- `Pseudo3DPlacementHint`
- `Pseudo3DVisualBindingManifest`
- `Pseudo3DValidationDiagnostic`
- `Pseudo3DPresentationReport`

## Fixture proof

Create fixtures for:

1. `building/swamp_necropolis_poor_dwelling`
2. `tree/dead_willow`
3. `rock/mossy_dark_stone`
4. `surface/fantasy_ruins_floor`
5. `surface/fantasy_ruins_wall`
6. `monster/slime_swamp_billboard`

Each must include:

- asset ids or placeholder ids;
- fallback ids;
- size/height;
- pivot;
- collision/footprint policy;
- sort policy;
- presentation mode compatibility.

## Validators

Validate:

- missing fallback;
- missing pivot;
- invalid height/scale;
- facade layer missing;
- billboard missing required idle state;
- wall/floor/ceiling surface roles mismatched;
- unknown projection mode;
- unsupported presentation mode.

## Tests

Add focused tests proving:

- valid fixtures produce clean report;
- invalid missing fallback produces diagnostic;
- building facade contract is stable and deterministic;
- actor billboard falls back missing attack to idle if policy says so;
- surface contracts distinguish wall/floor/ceiling;
- generated binding manifest uses stable ids.

## Output

This task should produce JSON manifests and reports only. No external rendering is required.

## Validation commands

Use existing repository validation pattern. If available:

```powershell
.\check-all.ps1
```

Or focused tests:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter Pseudo3DVisualPresentation
```

## Stop conditions

Stop and report if:

- Unity changes appear necessary;
- GamePackage schema changes appear necessary;
- current repo build/test is broken outside scope;
- existing asset/media abstractions conflict with proposed data.

## Final report

Report:

- files read;
- files changed;
- fixtures added;
- manifests added;
- validators added;
- tests added;
- commands run;
- unresolved design questions.
