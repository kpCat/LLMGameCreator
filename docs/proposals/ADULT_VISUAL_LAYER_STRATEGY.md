# Adult Visual Layer Strategy

## Status

Proposal / anti-forgetting documentation.

This document records the long-term direction for adult-capable visual generation in LLMGameCreator. It is intentionally documentation-only: it does not authorize a Runtime dependency on LLMs, ComfyUI, Fooocus, image providers, live prompts, network services or external media generation.

## Purpose

LLMGameCreator targets rich 2D / 2.5D / pseudo-3D generated games where the final playable runtime consumes validated GamePackage data and bound assets. Some generated games may intentionally support adult-only visual content, including NSFW character presentation and adult relationship scenes.

The adult visual layer must not become a separate ad-hoc NSFW image generator. It must be a rating-gated extension of the same composable visual system:

```text
semantic visual facts
+ visual part packs
+ palettes
+ layer stacks
+ body plans
+ state overlays
+ deterministic recipes
+ asset slots
+ manifest validation
+ review/promotion
```

AI image tools can help create master art, scene art, reference sheets and refinement candidates, but the source of truth remains GamePackage data, manifests and reviewed assets.

## Non-goals

Do not use this proposal to:

- call ComfyUI/Fooocus/InvokeAI/media providers from Runtime or Unity Player;
- generate live prompts in Runtime;
- make an LLM generate each NPC, tile, wound, clothing state or scene;
- dump thousands of hand-authored visual part JSON files into the repository;
- change public GamePackage schema without a separate reviewed contract;
- implement production-quality NSFW art generation in the first slice;
- bypass review, provenance, rating, export and fallback rules.

## Strategic rule

```text
Adult/NSFW visuals are not a separate generator.
They are rating-gated visual slots and overlays inside the composable visual recipe system.
```

The same rules that govern safe visuals also govern adult visuals:

- stable ids;
- deterministic recipe resolution;
- explicit asset slots;
- explicit source/provenance;
- local materialization or reviewed provider output;
- hash/byte validation;
- quarantine before promotion;
- export filtering;
- fallback behavior.

## Content boundary

Adult visual content must be limited to adult, sapient, humanoid-compatible fantasy characters or clearly adult human characters.

The adult layer must reject or quarantine any configuration that implies:

- minors, childlike, teen-like, young-looking or age-ambiguous subjects;
- feral/non-sapient animals or non-humanoid beast forms in sexualized slots;
- non-consensual scenes or coercive sexual framing;
- incest framing;
- hidden adult assets in safe/public builds;
- provider prompts or tags that conflict with the project rating policy.

Fantasy species may be human-compatible only when the species is defined as:

```text
adult-capable
+ sapient
+ humanoid or humanoid-compatible
+ relationship/consent compatible
+ allowed by project adult policy
```

For games inspired by adult fantasy RPGs, the intended target is not bestiality or random monster pornography. The target is adult relationships and erotic presentation involving humans and sapient fantasy peoples/species whose body plans are designed for human-compatible interaction.

## Rating model

Suggested rating ids for visual assets and slots:

| Rating | Meaning |
|---|---|
| `safe` | Suitable for normal builds and public UI. |
| `suggestive` | Adult-coded pose/outfit/flirt presentation without nudity or explicit action. |
| `adult_nude_reference` | Adult-only nude/anatomy reference for a character/species; not a normal public UI asset. |
| `adult_erotic_scene` | Adult-only erotic scene illustration; review-gated and export-filtered. |
| `adult_private_explicit` | Private/local explicit asset; never exported unless the project explicitly enables adult private builds. |

The names are planning ids. Implementation may rename them, but the distinction must remain.

## Export policy

Every adult-capable asset must carry an explicit export policy.

Suggested export policies:

| Policy | Meaning |
|---|---|
| `public_safe` | Can be exported into normal builds. |
| `mature_optional` | Can be exported only if the project enables mature content. |
| `adult_build_only` | Can be exported only into adult builds. |
| `private_local_only` | May remain in a local review pack but must not ship by default. |
| `blocked` | Rejected or quarantined. |

Safe builds must have deterministic fallbacks for adult slots. Missing or blocked adult assets must not break Runtime.

## Asset slot taxonomy

Adult layer slots should be separate from normal presentation slots.

Suggested character slots:

| Slot | Purpose |
|---|---|
| `portrait_safe` | Normal character portrait. |
| `portrait_suggestive` | Optional flirt/romance portrait. |
| `full_body_clothed` | Normal body/clothing reference. |
| `full_body_equipped` | Equipment/armor state. |
| `body_state_overlay` | Dirt, wounds, exhaustion, transformation state. |
| `clothing_state_overlay` | Torn, wet, damaged, ceremonial, casual or armor states. |
| `adult_nude_reference` | Adult-only body/anatomy reference. |
| `adult_relationship_scene` | Adult-only scene card. |
| `adult_private_variant` | Private review-only adult variant. |

The existence of an adult slot does not mean every species or NPC must have such assets. Adult slot generation should be sparse, opt-in and review-driven.

## Composable visual model

The adult layer should compose over the same visual recipe pipeline as safe visuals:

```text
CreatureVisualGenome
+ VisualRuleStack
+ SpeciesBodyPlan
+ SexPresentationProfile
+ ClothingState
+ EquipmentState
+ DamageState
+ AdultPresentationPolicy
+ seed
→ VisualRecipe
→ composed preview/control image/fallback
→ optional AI refinement candidate
→ quarantine
→ human review
→ promoted asset binding
```

The adult layer must not override identity, anatomy, age policy, sapience policy or export policy.

## Role of LLM

LLM may help with:

- species visual concepts;
- body-plan vocabulary;
- sex-presentation vocabulary;
- clothing/equipment/wound state vocabulary;
- prompt hints for offline provider runs;
- contradiction repair suggestions;
- review checklist drafts.

LLM must not be responsible for:

- id/path creation;
- asset slot allocation;
- policy enforcement;
- age/sapience/rating validation;
- export filtering;
- final asset promotion;
- runtime behavior;
- live image generation.

## Role of Codex/implementation agents

Codex should not generate thousands of visual detail records or art assets.

Codex may later implement:

- contracts;
- validators;
- fixture packs;
- deterministic recipe resolvers;
- review manifests;
- export filters;
- sample reports;
- tests.

Codex must not call external image providers or add ComfyUI/Fooocus integration unless the task explicitly authorizes provider work.

## Role of ComfyUI and Civitai models

ComfyUI/Civitai models are external/offline authoring aids, not Runtime dependencies.

Recommended use:

1. Generate master art for important species or characters.
2. Generate reference sheets for species body plans.
3. Refine deterministic silhouettes, masks or control images produced by LLMGameCreator.
4. Produce adult-only candidate art packs for review.
5. Help create reusable part families, such as horns, wings, clothing trims, body markings or surface motifs.

Provider output must enter candidate quarantine first. It becomes a real asset only after validation and review.

## Relationship to Visual Part Packs

Adult visuals should extend VisualPartPack semantics, not bypass them.

Relevant part families include:

- mature humanoid body silhouettes;
- sex presentation silhouettes;
- species-specific appendages;
- body surface markings;
- clothing layers;
- equipment layers;
- wound/damage overlays;
- dirt/wetness/exhaustion overlays;
- adult-only body/anatomy reference overlays;
- adult-scene composition masks.

All adult-capable part families require compatibility constraints.

## Validation expectations

A future implementation should produce diagnostics for:

- adult asset slot without adult project flag;
- adult slot assigned to non-adult/age-ambiguous character;
- adult slot assigned to non-sapient or feral body plan;
- missing required safe fallback;
- unsafe rating exported into public build;
- provider output promoted without review;
- prompt tags conflicting with rating policy;
- duplicate adult asset ids;
- adult asset path outside allowed artifact root;
- mismatched hash/byte count;
- cross-project adult asset leakage.

## Staged adoption

Recommended staged path:

1. Documentation-only adult visual strategy.
2. Data-only rating/export vocabulary in candidate docs/fixtures.
3. Creature Visual Genome proposal and sample schemas.
4. VisualPartPack adult extension proposal.
5. Application-layer validator proof with tiny fixtures only.
6. Review package proof with placeholder assets.
7. Optional offline provider/refinement contract.
8. Production art workflow only after the validator/review path is proven.

The first implementation slice should stay BCL-only and docs/fixtures/tests focused.
