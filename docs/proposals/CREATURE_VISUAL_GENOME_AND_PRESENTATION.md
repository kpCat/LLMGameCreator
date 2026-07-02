# Creature Visual Genome And Presentation

## Status

Proposal.

This document describes how LLMGameCreator can represent visually rich fantasy species and characters without asking LLMs or image models to generate every final image. It also defines where adult/NSFW presentation fits when a project explicitly enables it.

## Problem

Games such as the planned "Носитель Метамодулей" may need around 100 unique sapient species, each with:

- safe portraits;
- clothed full-body references;
- sex-presentation variants;
- clothing states;
- equipment states;
- damage/wound states;
- pseudo-3D presentation;
- optional adult-only visual variants;
- occasional hero scenes or relationship scenes.

Generating each variation as a separate one-off image is too expensive, inconsistent and hard to validate. The generator needs a compact visual genome that can be resolved into recipes and asset slots.

## Concept

A Creature Visual Genome is a stable data contract for the visual identity and presentation rules of a species or character family.

It should answer:

```text
What body plan is this?
What makes this species visually unique?
Which parts can vary by sex presentation?
Which parts can vary by clothing, equipment, wounds and state?
Which parts can be composed deterministically?
Which slots are safe/public?
Which slots are adult-only?
Which combinations are forbidden?
```

## High-level pipeline

```text
species semantic profile
+ CreatureVisualGenome
+ VisualRuleStack
+ project style profile
+ seed
→ CharacterVisualRecipe
→ safe asset slots
→ optional adult asset slots
→ deterministic preview/control assets
→ optional offline AI refinement
→ candidate quarantine
→ human review
→ promoted asset manifest binding
```

## Suggested model

```json
{
  "schemaVersion": "creature_visual_genome_v1",
  "speciesId": "metamodule/ashen_laminar",
  "status": "candidate",
  "maturityPolicy": "adult_slots_require_adult_character",
  "sapience": "sapient",
  "bodyPlan": {
    "kind": "humanoid_variant",
    "humanCompatibility": "humanoid_compatible",
    "silhouetteClass": "tall_graceful",
    "headPlan": "humanlike_face_with_nonhuman_markings",
    "torsoPlan": "humanlike_torso",
    "limbPlan": "two_arms_two_legs",
    "appendageSlots": ["horns", "tail_optional", "wing_optional"]
  },
  "surfacePlan": {
    "baseSurface": "skin",
    "secondarySurfaces": ["subtle_scales", "glow_markings"],
    "paletteFamilies": ["ash_gray", "violet_glow", "dark_gold"]
  },
  "sexPresentationProfiles": [
    "female",
    "male",
    "mixed"
  ],
  "clothingSlots": [
    "underlayer",
    "torso_outer",
    "legs_outer",
    "robe_or_cloak",
    "armor",
    "accessory"
  ],
  "equipmentSlots": [
    "weapon",
    "focus_item",
    "pack",
    "jewelry",
    "faction_emblem"
  ],
  "stateOverlays": [
    "wounds",
    "blood",
    "dirt",
    "wetness",
    "torn_clothing",
    "exhaustion",
    "magical_corruption"
  ],
  "projectionModes": [
    "portrait",
    "full_body_billboard",
    "layered_pseudo3d_billboard",
    "scene_card"
  ],
  "adultPresentation": {
    "enabledByDefault": false,
    "allowedRatings": ["suggestive", "adult_nude_reference"],
    "forbiddenRatings": ["adult_private_explicit"],
    "requiresFlags": ["adult_project", "adult_character", "sapient", "humanoid_compatible"],
    "safeFallbackRequired": true
  },
  "forbiddenTags": [
    "minor",
    "teen",
    "childlike",
    "young_looking",
    "feral",
    "non_sapient",
    "non_consensual"
  ]
}
```

## Body plan classes

Suggested first-class body-plan ids:

| Body plan | Meaning |
|---|---|
| `human` | Normal human. |
| `humanoid_variant` | Human-like body with fantasy traits. |
| `anthro_humanoid` | Anthro-coded but bipedal, sapient and human-compatible. |
| `alien_humanoid` | Non-earth species with human-compatible humanoid plan. |
| `monster_humanoid` | Monster-like but adult, sapient and humanoid-compatible. |
| `nonhumanoid_safe_only` | Non-humanoid creature; safe visuals only. |
| `feral_safe_only` | Animal/feral creature; never eligible for adult slots. |

Adult presentation should be allowed only for adult human, humanoid_variant, anthro_humanoid, alien_humanoid or monster_humanoid, and only when sapience/adult flags are present.

## Sex presentation profiles

Sex presentation is a visual/profile concept, not a hard biological implementation.

Suggested values:

| Profile | Use |
|---|---|
| `female` | Feminine adult presentation. |
| `male` | Masculine adult presentation. |
| `androgynous` | Adult ambiguous/neutral presentation. |
| `mixed` | Species-specific mixed profile. |
| `nonsexual_safe_only` | No sexualized/adult presentation. |

For some species, "mixed" may be a lore-specific presentation. It must still obey adult/sapience/humanoid constraints when used in adult slots.

## Character layer stack

Recommended character composition order:

1. base body silhouette;
2. base body surface;
3. species surface markings;
4. face/eye/mouth expression layer;
5. hair/mane/head ornament layer;
6. horns/ears/tail/wings/appendage layer;
7. underclothing layer;
8. outer clothing layer;
9. armor/equipment layer;
10. faction/role accessories;
11. wounds/dirt/wetness/exhaustion overlays;
12. magical/biological transformation overlays;
13. lighting/shadow normalization;
14. rating-gated adult-only overlay or replacement slots.

Adult-only layers must never be required for safe presentation.

## Clothing and equipment states

Clothing and equipment should be state-driven, not hand-authored per character.

Suggested state axes:

| Axis | Examples |
|---|---|
| `coverage` | full, light, ceremonial, armor, damaged. |
| `condition` | clean, dirty, torn, wet, bloodied, burned. |
| `role` | civilian, noble, cultist, warrior, worker, prisoner, ritual. |
| `material` | cloth, leather, metal, bone, chitin, plant, tech. |
| `faction` | emblem, color, trim, motif. |
| `adultPolicy` | safe_only, suggestive_allowed, adult_build_only. |

A clothing state can expose adult-coded presentation without replacing the whole character image.

## Damage and wound states

Damage visual states should be independent overlays when possible:

- scratch cluster;
- bruise/dirt;
- blood stain;
- torn fabric;
- armor dent;
- magical burn;
- corruption mark;
- exhaustion/illness pose cue.

These must be compatible with safe and adult builds. Explicit gore should be governed by a separate violence/gore rating policy if introduced later.

## Pseudo-3D presentation

A character can remain 2D while supporting pseudo-3D presentation through metadata:

```json
{
  "projectionMode": "layered_pseudo3d_billboard",
  "anchorPoints": {
    "head": [0.5, 0.15],
    "torso": [0.5, 0.45],
    "leftHand": [0.25, 0.52],
    "rightHand": [0.75, 0.52],
    "feet": [0.5, 0.95]
  },
  "depthLayers": [
    "back_wings",
    "body",
    "clothing",
    "front_arms",
    "equipment",
    "foreground_effects"
  ],
  "parallaxPolicy": "small_character_card_depth"
}
```

This allows:

- parallax;
- depth ordering;
- equipment overlays;
- expression swapping;
- damage overlays;
- light pseudo-3D scenes;
- AI-control images or masks.

## Adult visual slots

Adult slots are optional.

Suggested adult slot set:

| Slot | Purpose |
|---|---|
| `suggestive_portrait` | Romance/flirt presentation without nudity. |
| `adult_nude_reference` | Adult-only anatomy/body reference for species/character. |
| `adult_relationship_scene_card` | Adult-only reviewed scene image. |
| `adult_private_variant` | Private/local reviewed output; not normal export. |

A species may allow only `suggestive_portrait` and disallow all other adult slots.

## Human compatibility

Human compatibility should be explicit data, not assumed from a prompt.

Suggested values:

| Value | Meaning |
|---|---|
| `none` | Not human-compatible; safe visuals only. |
| `social_only` | Human-like interaction/romance possible, no adult visuals. |
| `humanoid_compatible` | Adult humanoid compatibility allowed if project policy enables it. |
| `lore_exception_review_required` | Requires manual review. |
| `blocked` | Never eligible for adult relationship visuals. |

## Prompt hints versus source of truth

Prompt hints are not source of truth. They are derived from reviewed visual data.

A future provider request should be derived like:

```text
CreatureVisualGenome
+ approved style profile
+ rating policy
+ slot id
+ seed
→ provider prompt hint
→ provider output
→ candidate quarantine
```

Provider output must not invent unreviewed body-plan eligibility or adult eligibility.

## Review checklist

Before promoting a species adult visual pack, review:

- adult-only flags are explicit;
- species is adult/sapient/humanoid-compatible;
- safe fallback exists;
- no minor/young-looking cues;
- no feral/non-sapient sexualization;
- no non-consensual framing;
- export policy is correct;
- asset ids are stable;
- paths are relative and safe;
- hashes match physical files;
- generated style does not destroy species identity;
- safe and adult variants remain visually consistent.

## Early MVP

First MVP should not generate final adult art. It should prove:

- schema vocabulary;
- sample species genomes;
- rating/export policy examples;
- deterministic slot planning;
- validation diagnostics;
- safe fallback behavior;
- no Runtime provider calls;
- no public GamePackage schema mutation.
