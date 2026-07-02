# Visual Part Pack Adult Extension

## Status

Proposal.

This document extends the visual part pack idea with rating-gated adult-capable character and creature presentation. It assumes the existing direction where VisualPartPack, palettes, layer rules, surface recipes and deterministic renderers create reusable visual building blocks.

## Core rule

Adult-capable parts are still visual parts.

They must obey the same rules as other parts:

- stable ids;
- known part kind;
- known layer role;
- compatibility constraints;
- palette/color-slot validation;
- projection-mode compatibility;
- deterministic generation;
- manifest output;
- review before promotion;
- fallback behavior.

The only special rule is stricter rating and export policy.

## Extended part metadata

Suggested extension fields:

```json
{
  "partId": "part/creature/body_marking/glow_veins_01",
  "role": "body_marking",
  "shapeKind": "branching_polyline",
  "sizeClass": "midi",
  "compatibleSurfaceRoles": ["creature_part", "character_body"],
  "compatibleBodyPlans": ["humanoid_variant", "alien_humanoid", "monster_humanoid"],
  "compatibleSexPresentationProfiles": ["female", "male", "androgynous", "mixed"],
  "contentRatings": ["safe", "suggestive"],
  "exportPolicies": ["public_safe", "mature_optional"],
  "requiresFlags": [],
  "forbiddenTags": ["minor", "feral", "non_sapient"],
  "safeFallbackPartId": "part/creature/body_marking/generic_subtle_01"
}
```

For adult-only parts:

```json
{
  "partId": "part/character/adult_reference/body_silhouette_mature_humanoid_01",
  "role": "adult_body_reference",
  "shapeKind": "silhouette_mask",
  "sizeClass": "macro",
  "compatibleSurfaceRoles": ["character_body"],
  "compatibleBodyPlans": ["human", "humanoid_variant", "alien_humanoid", "monster_humanoid"],
  "compatibleSexPresentationProfiles": ["female", "male", "androgynous", "mixed"],
  "contentRatings": ["adult_nude_reference"],
  "exportPolicies": ["adult_build_only", "private_local_only"],
  "requiresFlags": ["adult_project", "adult_character", "sapient", "humanoid_compatible"],
  "forbiddenTags": ["minor", "young_looking", "feral", "non_sapient", "non_consensual"],
  "safeFallbackPartId": "part/character/body/safe_clothed_silhouette_01"
}
```

## Adult-capable part roles

Suggested roles:

| Role | Meaning |
|---|---|
| `body_surface` | Skin/scales/chitin/body material. |
| `body_marking` | Pattern, tattoo, glow, stripe, scale accents. |
| `appendage` | Horns, wings, tail, ears, spines, claws. |
| `hair_or_mane` | Hair, mane, crest, tentacle-like safe head ornament if allowed. |
| `face_feature` | Eyes, expression, mouth/teeth style. |
| `clothing_underlayer` | Base clothing. |
| `clothing_outer` | Main outfit. |
| `armor_layer` | Armor/equipment overlay. |
| `equipment_overlay` | Weapon, bag, jewelry, artifact. |
| `state_overlay` | Dirt, wounds, blood, wetness, exhaustion, torn fabric. |
| `mature_body_silhouette` | Adult-safe silhouette cue; rating may vary. |
| `adult_body_reference` | Adult-only anatomy/reference part. |
| `adult_scene_mask` | Adult-only scene composition mask/control part. |

Avoid mixing identity-defining species parts with adult-only overlays. Species identity should remain visible in safe builds.

## Rating rules

A part may be allowed in multiple ratings, but adult-only ratings require explicit flags.

Suggested valid combinations:

| Part rating | Required flags |
|---|---|
| `safe` | none |
| `suggestive` | `mature_project` or `adult_project` if configured |
| `adult_nude_reference` | `adult_project`, `adult_character`, `sapient`, `humanoid_compatible` |
| `adult_erotic_scene` | `adult_project`, `adult_character`, `sapient`, `humanoid_compatible`, `scene_review_required` |
| `adult_private_explicit` | all adult flags plus `private_local_only` export |

Invalid combinations should reject before rendering or provider request creation.

## Export filtering

The part resolver must not allow adult-only parts into normal public output.

Suggested behavior:

```text
requested safe build + adult-only part
→ reject or replace with safeFallbackPartId
→ report diagnostic
```

```text
requested adult build + adult-only part + missing flags
→ reject
```

```text
private local review pack + adult-only part + all flags present
→ allow into quarantine/review pack
```

## Layering rules

Adult extension should preserve normal character layering.

Suggested layer stack:

1. base silhouette;
2. body surface;
3. species markings;
4. face/expression;
5. hair/appendages;
6. clothing underlayer;
7. clothing outer layer;
8. armor/equipment;
9. state overlays;
10. adult-only replacement/reference layer;
11. scene/light normalization.

Adult-only reference layers should usually be replacement slots, not overlays on top of safe clothed art.

## Compatibility constraints

Every adult-capable part must declare:

- compatible body plans;
- compatible sex-presentation profiles;
- compatible projection modes;
- allowed ratings;
- allowed export policies;
- required flags;
- forbidden tags;
- fallback part id or explicit `noFallbackAllowed` reason.

## Provider/refinement use

A VisualPartPack can produce:

```text
deterministic silhouette
+ body/appendage masks
+ clothing masks
+ state overlay masks
+ pose/control image
```

Those can be passed to ComfyUI/InvokeAI/Fooocus-style offline workflows in a future provider step. The output remains a candidate until validated and reviewed.

No provider-specific prompt should be the source of truth.

## Validation diagnostics

Suggested diagnostics:

| Code | Meaning |
|---|---|
| `AVP001` | adult part used without adult project flag. |
| `AVP002` | adult part used for non-adult or age-ambiguous character. |
| `AVP003` | adult part used for non-sapient/feral body plan. |
| `AVP004` | adult part has no safe fallback. |
| `AVP005` | adult part exported to public_safe build. |
| `AVP006` | forbidden tag appears in part, recipe or prompt hint. |
| `AVP007` | adult provider output promoted without review. |
| `AVP008` | body-plan compatibility is missing. |
| `AVP009` | sex-presentation compatibility is missing. |
| `AVP010` | rating/export policy combination is invalid. |

## Fixture strategy

Use tiny fixtures only:

- one safe humanoid species;
- one humanoid fantasy species with suggestive allowed;
- one adult-capable humanoid species with adult slots disabled by default;
- one nonhumanoid safe-only species;
- one invalid feral/adult combination;
- one missing fallback case;
- one public export rejection case.

Do not add real NSFW art fixtures into the repository. Use placeholder/vector metadata and rating labels.

## Relationship to Creature Visual Genome

CreatureVisualGenome chooses what a species may have.

VisualPartPack supplies the reusable parts.

VisualRecipe resolves the chosen parts.

Asset pipeline binds reviewed outputs.

```text
CreatureVisualGenome
→ allowed visual features and adult policy

VisualPartPack
→ reusable parts and constraints

VisualRecipe
→ concrete composition for one slot/seed/state

Asset manifest
→ reviewed file binding and export policy
```

## MVP boundary

A first implementation should be docs/fixtures/tests only:

- data contracts in candidate space;
- validation diagnostics;
- no real adult art files;
- no ComfyUI integration;
- no Runtime/Unity changes;
- no public GamePackage schema mutation.
