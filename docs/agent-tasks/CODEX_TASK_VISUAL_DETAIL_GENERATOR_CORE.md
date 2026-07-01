# Codex Task — Visual Detail Generator Core

## Task ID

`visual-detail-generator-core-v1`

## Goal

Add a candidate-owned deterministic Visual Detail Generator core.

The goal is not to generate thousands of detail records. The goal is to implement the schema, generator rules, validators, fixtures and tests so LLMGameCreator can locally generate many visual detail variants from compact part families and seeds.

## Context

Important strategic decision:

```text
Codex must not dump 10 000 visual parts into the repo.
Codex must implement compiler/generator/validator.
LLMGameCreator must generate variants locally and deterministically.
```

This generator will later support pseudo-3D visual generation, surface recipes, building facades, clothing details, creature details, environment details and AI-refinement control images.

## Read first

- `docs/proposals/PROCEDURAL_VISUAL_DETAIL_GENERATOR_STRATEGY.md`
- `docs/proposals/PROCEDURAL_VISUAL_PART_PACKS.md`
- `docs/proposals/VISUAL_WORLD_GRAMMAR_AND_PSEUDO3D_GENERATION.md`
- `docs/proposals/PSEUDO3D_ASSET_PRESENTATION_CONTRACTS.md`
- Existing candidate-owned Application/Design module patterns.
- Existing media asset campaign/materialization docs/code if relevant.

## Allowed scope

Prefer candidate-owned paths:

```text
src/LLMGameCreator.Application/Design/VisualDetailGeneration/**
tests/LLMGameCreator.Tests/Application/VisualDetailGeneration/**
docs/candidates/visual-detail-generator/**
```

Exact paths may be adjusted to match repository conventions.

## Forbidden scope

Do not:

- generate thousands of JSON detail records;
- generate production art;
- integrate ComfyUI/Fooocus/InvokeAI;
- call external providers;
- add network calls;
- change Unity runtime/player;
- change public GamePackage schema;
- add large UI;
- write generated atlas dumps into the main repo;
- modify `.sln` / `.csproj` unless unavoidable and justified;
- use git/branch/push/rebase/cherry-pick instructions.

## Required model concepts

Add minimal immutable/data-first models:

- `VisualDetailGeneratorId`
- `VisualDetailGeneratorVersion`
- `VisualPartFamily`
- `VisualPartKind`
- `VisualPrimitiveGeneratorKind`
- `VisualPartParameterRange`
- `VisualPartVariant`
- `VisualPartVariantRequest`
- `VisualPartVariantManifest`
- `VisualDetailVocabulary`
- `VisualDetailPack`
- `VisualProjectionMode`
- `VisualSurfaceRole`
- `VisualColorSlot`
- `VisualPalette`
- `VisualDetailDiagnostic`
- `VisualDetailGenerationReport`

## Required primitive generators

Implement a small deterministic set:

1. `branching_polyline`
   - cracks, roots, lightning, veins, cables.

2. `blob_mask`
   - moss, dirt, stains, puddles, rust.

3. `panel_grid`
   - sci-fi plates, metal floor, wall panels.

4. `rivet_line`
   - bolts, rivets, decorative points.

5. `stripe_pattern`
   - warning stripes, ritual stripes, cloth bands.

Optional if easy:

6. `scratch_cluster`
7. `edge_trim`
8. `rune_glyph`

These can be represented as vector-like primitives or lightweight internal shapes. Production-quality raster art is not required.

## Determinism

For the same:

```text
generator version + part family + request seed + parameters
```

the output manifest must be stable.

If visual preview output is implemented, it should also be stable.

## Output

The generator should output metadata/manifest first.

Example:

```json
{
  "variantId": "partvariant/crack/thin_branching/000184",
  "partFamilyId": "partfamily/crack/thin_branching",
  "generatorId": "visual-detail-generator/core",
  "generatorVersion": "1.0.0",
  "seed": 184,
  "primitiveKind": "branching_polyline",
  "resolvedParameters": {
    "branchCount": 3,
    "length": 0.62,
    "curvature": 0.21,
    "width": 0.018
  },
  "compatibleSurfaceRoles": ["floor", "wall", "ceiling"],
  "compatibleMaterials": ["stone", "ice", "bone"],
  "projectionModes": ["top_down", "front_wall"],
  "semanticTags": ["damage", "crack", "stone_compatible"],
  "cachePolicy": "materialize_on_demand"
}
```

## Fixture packs

Add small fixture packs, not huge dumps:

1. `fantasy_ruins`
   - cracks, moss, stone slabs, runes, bone trims.

2. `tech_hull`
   - panels, rivets, cables, warning stripes, scratches.

3. `natural_forest`
   - leaves, grass patches, roots, mud blobs, bark lines.

Each pack should contain roughly 10–20 part families, not thousands of variants.

## Validators

Add diagnostics for:

- missing generator id/version;
- unknown primitive kind;
- invalid parameter range;
- min > max;
- missing semantic role;
- empty compatible surface roles;
- missing color slots;
- unsupported projection mode;
- unsupported surface role;
- forbidden material/surface pairing;
- unstable id;
- duplicate family id;
- non-deterministic output detected in tests.

## Tests

Add tests proving:

- same request gives same manifest;
- different seed gives different but valid manifest;
- invalid parameter ranges produce diagnostics;
- unknown primitive kind produces diagnostics;
- fixture packs are valid;
- generated variants preserve compatible surface roles;
- generated variants preserve semantic tags;
- no test fixture requires thousands of generated records;
- stable ids are produced for fixture variants.

## Optional preview

If there is already an acceptable repository pattern for lightweight image/vector output, add tiny deterministic preview output.

If this would require new dependencies or project-file changes, skip preview and produce vector-shape metadata only.

## Validation commands

Use existing repository validation pattern. If available:

```powershell
.\check-all.ps1
```

Or focused tests:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualDetailGeneration
```

## Stop conditions

Stop and report if:

- image preview requires new dependencies;
- implementation requires GamePackage schema changes;
- implementation requires Unity changes;
- tests cannot be added without broad project changes;
- existing build/test is already broken outside this scope.

## Final report

Report:

- files read;
- files changed;
- models added;
- primitive generators added;
- fixture packs added;
- validators added;
- tests added;
- validation commands run;
- skipped preview/output items and why;
- recommended next slice.
