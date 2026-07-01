# Codex Task — Semantic Visual Grammar Resolver

## Task ID

`visual-grammar-resolver-v1`

## Goal

Add a candidate-owned Semantic Visual Grammar layer that turns compact semantic features into deterministic visual recipes.

This task must not generate real art. It must establish the data/contracts/services needed for future pseudo-3D visual generation.

## Context

LLMGameCreator should not ask LLM to generate every house, tree, stone, NPC, tile, texture, or creature. LLM should only help create compact domain/culture/religion/material/motif profiles. The application should generate visual recipes deterministically from profiles, local tags and seeds.

Core formula:

```text
domain/biome/settlement/object semantic features
→ VisualRuleStack
→ VisualGrammarResolver
→ VisualRecipe
```

## Read first

- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/proposals/VISUAL_WORLD_GRAMMAR_AND_PSEUDO3D_GENERATION.md`
- `docs/proposals/VISUAL_RULE_STACK_AND_DOMAIN_PROFILES.md`
- `docs/proposals/PSEUDO3D_ASSET_PRESENTATION_CONTRACTS.md`
- Existing Application/Design candidate module patterns.
- Existing tests for candidate-owned deterministic services.

## Allowed scope

Prefer a candidate-owned namespace, for example:

```text
src/LLMGameCreator.Application/Design/SemanticVisualGrammar/**
tests/LLMGameCreator.Tests/Application/SemanticVisualGrammar/**
docs/candidates/semantic-visual-grammar/**
```

Exact paths may be adjusted to match current repository conventions.

## Forbidden scope

Do not:

- change public GamePackage schema;
- change Unity runtime/player;
- integrate ComfyUI/Fooocus/InvokeAI;
- call external providers;
- add network calls;
- change `.sln` / `.csproj` unless explicitly necessary and justified;
- add large UI;
- generate production images;
- add git/branch/push/rebase/cherry-pick instructions.

## Required model concepts

Add minimal immutable/data-first models:

- `VisualRuleSource`
- `VisualRuleStack`
- `VisualRuleWeight`
- `DomainVisualProfile`
- `BiomeVisualProfile`
- `SettlementTierProfile`
- `PopulationVisualProfile`
- `ObjectRoleVisualProfile`
- `VisualGrammarRequest`
- `VisualRecipe`
- `BuildingVisualRecipe`
- `Pseudo3DPresentationHint`
- `VisualGrammarDiagnostic`
- `VisualGrammarResolutionReport`

## Required service

Implement:

```text
SemanticVisualGrammarResolver
```

Input:

```json
{
  "worldProfileId": "world/dark_fantasy",
  "domainId": "domain/necropolis",
  "objectKind": "building",
  "role": "dwelling",
  "localTags": ["biome/swamp", "condition/damaged", "wealth/poor"],
  "seed": 123
}
```

Output:

```json
{
  "recipeKind": "building_visual_recipe",
  "shapeGrammar": "crooked_stilt_hut",
  "materials": {
    "walls": "dark_rotten_wood",
    "foundation": "mossy_dark_stone",
    "roof": "wet_reed_thatch"
  },
  "motifs": ["small_ancestor_shrine", "green_soul_lantern"],
  "pseudo3d": {
    "mode": "facade_billboard",
    "pivot": "bottom_center",
    "fallback": "building/generic_swamp_hut"
  }
}
```

## Fixture profiles

Add deterministic fixture profiles for at least:

1. `domain/necropolis`
2. `domain/solar_order` or equivalent light/order domain
3. `domain/tech_future` or equivalent sci-fi domain

And at least three object requests:

1. poor dwelling in necropolis swamp village;
2. official temple/palace building in same domain;
3. habitation module in tech/sci-fi settlement.

## Validation

Resolver must produce diagnostics for:

- missing domain profile;
- unknown object kind;
- unknown role;
- empty material resolution;
- missing pseudo-3D fallback;
- contradictory tags where easy to detect;
- forbidden motif/material selected.

## Tests

Add tests proving:

- deterministic output for same seed;
- different seed can change variant but not violate constraints;
- dwelling in necropolis controlled village remains a dwelling, not a castle;
- official necropolis temple receives stronger domain/religion influence;
- sci-fi habitation module uses tech materials/motifs;
- forbidden materials/motifs do not appear;
- missing profile returns diagnostics instead of throwing unhandled exceptions.

## Validation commands

Use existing repository validation pattern. If available:

```powershell
.\check-all.ps1
```

Or focused tests:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter SemanticVisualGrammar
```

## Stop conditions

Stop and report if:

- implementation requires GamePackage schema changes;
- implementation requires Unity changes;
- existing candidate module patterns are incompatible;
- tests cannot be added without project file changes;
- repository build/test is already broken outside this scope.

## Final report

Report:

- files read;
- files changed;
- models added;
- services added;
- fixtures added;
- tests added;
- validation commands run;
- any skipped items and why;
- next recommended slice.
