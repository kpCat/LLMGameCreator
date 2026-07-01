# Codex Task — Procedural Visual Part Pack Compiler

## Task ID

`procedural-visual-part-pack-compiler-v1`

## Goal

Add a candidate-owned deterministic prototype that composes simple procedural visual parts, palettes and surface recipes into preview atlases and metadata.

This is not a production art task. It should prove the architecture for visual part packs.

## Context

LLMGameCreator should be able to build reusable visual packs where small semantic visual parts can be recolored, layered and composed into surfaces/facades/tiles. These packs can later be used as fallback art, fixture art, pseudo-3D surface textures, or control images for AI refinement.

## Read first

- `docs/proposals/PROCEDURAL_VISUAL_PART_PACKS.md`
- `docs/proposals/VISUAL_WORLD_GRAMMAR_AND_PSEUDO3D_GENERATION.md`
- `docs/proposals/PSEUDO3D_ASSET_PRESENTATION_CONTRACTS.md`
- Existing media asset campaign/materialization docs and code.
- Existing candidate module/service/test patterns.

## Allowed scope

Prefer candidate-owned paths, for example:

```text
src/LLMGameCreator.Application/Design/ProceduralVisualPartPacks/**
tests/LLMGameCreator.Tests/Application/ProceduralVisualPartPacks/**
docs/candidates/procedural-visual-part-packs/**
```

## Forbidden scope

Do not:

- integrate ComfyUI/Fooocus/InvokeAI;
- call external image providers;
- change Unity runtime/player;
- change public GamePackage schema;
- add large UI;
- attempt production-quality art;
- create thousands of hand-authored parts;
- modify `.sln` / `.csproj` unless unavoidable and clearly justified;
- add git/branch/push/rebase/cherry-pick instructions.

## Required model concepts

- `VisualPartDefinition`
- `VisualPartPack`
- `VisualPartKind`
- `SurfaceRole`
- `ProjectionMode`
- `VisualPalette`
- `ColorSlot`
- `SurfaceRecipe`
- `SurfaceLayerStack`
- `ProceduralSurfaceRenderJob`
- `GeneratedSurfaceAtlasManifest`
- `GeneratedSurfaceTile`
- `VisualPartPackDiagnostic`
- `VisualPartPackRenderReport`

## Minimal renderer

Implement a deterministic simple renderer that can output basic PNG previews or an equivalent repository-accepted image artifact format.

If adding an image library is not acceptable, use an internal simple rasterizer with BCL primitives or output SVG/textual vector previews first. Prefer no external dependencies unless already available in the repository.

The renderer should support at least:

- rectangle/polygon/line/circle-like primitives;
- layer order;
- palette color slots;
- seeded jitter;
- seeded rotation/mirroring if feasible;
- tile atlas layout;
- metadata for atlas rects and tile roles.

## Fixture packs

Add at least three fixture packs:

1. `fantasy_ruins`
2. `tech_hull`
3. `natural_forest`

Each pack should have:

- a palette;
- 8–20 simple visual part definitions;
- 2–3 surface recipes;
- deterministic output manifest.

Required outputs:

- floor surface variants;
- wall surface variants;
- decal sheet or overlay variants.

## Metadata output

Generated manifest should include:

- atlas id;
- generated asset ids;
- tile/surface role;
- projection mode;
- variant id;
- seed;
- palette id;
- source recipe id;
- fallback id if applicable;
- diagnostics.

## Tests

Add tests proving:

- deterministic atlas manifest for same seed;
- different seeds produce stable different variants;
- required recipes produce non-empty outputs;
- invalid part references produce diagnostics;
- unknown color slots produce diagnostics;
- forbidden surface-role usage is rejected or diagnosed;
- atlas rects do not overlap;
- metadata uses stable ids.

## Validation commands

Use existing repository validation pattern. If available:

```powershell
.\check-all.ps1
```

Or focused tests:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter ProceduralVisualPartPacks
```

## Stop conditions

Stop and report if:

- image output requires unacceptable dependencies;
- tests cannot be added without broad project changes;
- existing build is broken outside scope;
- implementation would require Unity or GamePackage schema changes.

## Final report

Report:

- files read;
- files changed;
- models/services added;
- fixture packs added;
- sample output paths;
- tests added;
- commands run;
- limitations of renderer;
- recommended next slice.
