# Visual Media Pipeline Implementation Roadmap

## Purpose

This roadmap turns the existing visual/adult-layer documents into a practical future goal sequence. It is plan-only. Goal 083 does not implement providers, Runtime behavior, Unity rendering, public GamePackage schema changes, binary media, generated images or prompt dumps.

## Guiding Rules

- Keep the source of truth in GamePackage data, manifests, catalogs, recipes and reviewed asset bindings.
- Keep Runtime and Unity Player free of LLM/media provider calls.
- Treat provider output as quarantined candidates until validation and review promote it.
- Require deterministic safe fallbacks for safe/public builds.
- Treat adult-capable visuals as rating-gated extension metadata inside the shared visual/media pipeline.
- Do not use prompts or generated media as authoritative project state.

## Stage 1: Visual Asset Contract And Rating Metadata

Goal shape:

- define candidate-side visual asset slot metadata;
- define `rating`, `adultEnabled`, `safeFallbackRequired`, `candidateQuarantine`, `reviewStatus`, `exportPolicy`, `assetSlot` and `approvedAssetRef`;
- validate relative ids, fallback presence, rating/export combinations and source-of-truth rules;
- produce tiny metadata fixtures only.

Non-goals:

- no public GamePackage schema change unless a later contract proves consumers;
- no provider calls;
- no real media assets.

## Stage 2: Visual Rule Stack / Recipe Resolver

Goal shape:

- consume domain, biome, culture, settlement, object role, state and seed facts;
- resolve deterministic visual recipes;
- prove recipe stability and variation with fixture profiles;
- keep recipes sidecar/editor-owned until a package binding goal is selected.

Non-goals:

- no production art;
- no Unity-specific gameplay logic;
- no runtime provider calls.

## Stage 3: Visual Detail Generator Core

Goal shape:

- implement compact part families, generator versions, primitive kinds and validators;
- generate deterministic detail variants locally by seed;
- keep fixture packs small;
- prove same seed stability and different seed variation.

Non-goals:

- no large generated JSON/PNG dumps;
- no external image libraries unless already approved by a separate task;
- no provider integration.

## Stage 4: Procedural Visual Part-Pack Compiler

Goal shape:

- compose simple visual parts, palettes and surface recipes into deterministic preview metadata;
- prove layer ordering, palette slots, surface roles, projection modes and atlas metadata;
- use tiny fixture packs such as `fantasy_ruins`, `tech_hull` and `natural_forest`.

Non-goals:

- no commercial-quality art target;
- no broad UI;
- no Unity project mutation.

## Stage 5: Pseudo-3D Presentation Sidecar Proof

Goal shape:

- define sidecar contracts for surface textures, facades, billboards, pivots, scale, sorting, collision, fallbacks and state sets;
- prove a player-facing package can reference already-approved/fallback visual assets through sidecar manifests;
- keep Runtime/Unity consumers data-driven.

Non-goals:

- no new Unity renderer before the sidecar contract is validated;
- no public schema mutation without dedicated consumer proof.

## Stage 6: Provider Candidate Quarantine And Review

Goal shape:

- model editor-time provider candidate records;
- require provenance, rating, relative path, byte/hash facts, source recipe id, review status and promotion decision;
- reject fake-success, missing file, tampered hash, unsafe rating/export and unreviewed promotion cases.

Non-goals:

- no real provider calls in this stage;
- no network calls;
- no prompt dump as evidence.

## Stage 7: Deterministic Safe Fallback Generation

Goal shape:

- prove every visual slot resolves in safe/public builds;
- generate or bind deterministic placeholder/fallback metadata;
- fail closed when adult-capable metadata lacks safe fallback.

Non-goals:

- no adult media files;
- no provider output required for safe runtime behavior.

## Stage 8: Adult / Rating-Gated Extension Metadata

Goal shape:

- extend visual asset metadata and validators with adult-capable flags, body-plan eligibility, sapience/adult eligibility, export policies and safe fallback requirements;
- prove invalid adult-capable combinations are rejected;
- keep all fixtures metadata-only or placeholder-only.

Non-goals:

- no real adult art;
- no explicit prompt recipes;
- no separate adult generator.

## Stage 9: WinForms Review Workspace

Goal shape:

- expose candidate metadata, validation status, provenance, fallback, review status and promotion decisions through a bounded review workspace;
- follow existing WinForms UserControl and Application service patterns;
- keep UI out of direct JSON mutation and provider execution.

Non-goals:

- no broad UI redesign;
- no media generation from UI in this stage.

## Stage 10: Unity / Player Consumption Of Approved Asset References

Goal shape:

- consume already-approved asset refs, sidecar manifests and deterministic fallbacks;
- prove safe/public builds exclude adult-capable assets unless the export policy allows them;
- keep all provider workflows editor-side.

Non-goals:

- no provider calls in Unity Player;
- no game-specific C# logic in Unity;
- no unreviewed candidate media in StreamingAssets.

## Candidate Future Gates

- `visual_asset_contract_rating_metadata_verification`
- `visual_rule_stack_recipe_resolver_verification`
- `visual_detail_generator_core_verification`
- `procedural_visual_part_pack_compiler_verification`
- `pseudo3d_visual_presentation_sidecar_verification`
- `visual_provider_candidate_quarantine_verification`
- `visual_safe_fallback_generation_verification`
- `adult_visual_rating_metadata_verification`
- `visual_media_review_workspace_verification`
- `unity_approved_visual_asset_consumption_verification`

## Dependency Order

Do not start provider integration, adult-capable export behavior or Unity consumption before the metadata contract, fallback, quarantine and review gates exist. The first implementation goal should stay metadata/validator/fixture focused and should produce deterministic evidence before any media output path is widened.
