# Visual Adult Layer Context Index

## Purpose

This index makes the existing visual-layer and adult-capable visual-layer documents part of the official project context spine. It is a routing document, not a new implementation task and not a content asset.

Goal 083 records the current policy boundary:

```text
Adult-capable visuals are rating-gated metadata, asset slots, overlays, candidate records and review decisions inside the shared visual/media pipeline.
They are not a separate generator and they are never runtime provider behavior.
```

## Source Documents

Read these together for future visual/media pipeline tasks:

1. `docs/context/VISUAL_WORLD_GENERATION_CONTEXT_BRIEF.md`
2. `docs/proposals/VISUAL_RULE_STACK_AND_DOMAIN_PROFILES.md`
3. `docs/proposals/VISUAL_WORLD_GRAMMAR_AND_PSEUDO3D_GENERATION.md`
4. `docs/proposals/PROCEDURAL_VISUAL_DETAIL_GENERATOR_STRATEGY.md`
5. `docs/proposals/PROCEDURAL_VISUAL_PART_PACKS.md`
6. `docs/proposals/PSEUDO3D_ASSET_PRESENTATION_CONTRACTS.md`
7. `docs/proposals/CREATURE_VISUAL_GENOME_AND_PRESENTATION.md`
8. `docs/proposals/ADULT_VISUAL_LAYER_STRATEGY.md`
9. `docs/proposals/VISUAL_PART_PACK_ADULT_EXTENSION.md`
10. `docs/context/METAMODULE_CARRIER_VISUAL_NSFW_CONTEXT_BRIEF.md`
11. `docs/context/ADULT_VISUAL_LAYER_DOCUMENTATION_MANIFEST.md`
12. `docs/agent-tasks/CODEX_TASK_ADULT_VISUAL_LAYER_DOCS_ONLY.md`
13. `docs/agent-tasks/CODEX_TASK_VISUAL_DETAIL_GENERATOR_CORE.md`
14. `docs/agent-tasks/CODEX_TASK_PROCEDURAL_VISUAL_PART_PACK_COMPILER.md`
15. `docs/agent-tasks/CODEX_TASK_VISUAL_GRAMMAR_RESOLVER.md`
16. `docs/agent-tasks/CODEX_TASK_PSEUDO3D_VISUAL_RECIPE_PROOF.md`
17. `docs/proposals/VISUAL_MEDIA_PIPELINE_IMPLEMENTATION_ROADMAP.md`

## Architectural Rules

- Runtime and Unity Player consume already-approved GamePackage data, manifests, catalogs and asset references.
- Runtime and Unity Player must not call LLMs, RAG, media providers, ComfyUI, Fooocus, InvokeAI or network generation services.
- GamePackage, manifests, catalogs, recipes and reviewed asset bindings are the source of truth.
- Prompt hints may be derived from reviewed metadata for editor-time provider candidates, but prompts are not source of truth.
- Provider output remains candidate media until deterministic validation and human review promote it.
- Codex may implement contracts, validators, deterministic generators, fixture metadata, review ledgers and tests when explicitly scoped.
- Codex must not dump large finished art inventories or generate production media assets as a substitute for a real pipeline.

## Safety / Rating Boundary

Adult-capable visual metadata is allowed only as opt-in policy data for adult project configurations. It must remain neutral, reviewable and export-gated.

Required adult-capable eligibility facts:

- `adultEnabled`
- adult character eligibility
- adult/sapient species
- humanoid or humanoid-compatible body plan
- `safeFallbackRequired`
- `candidateQuarantine`
- `reviewStatus`
- `exportPolicy`

Reject or quarantine any future record that implies age-ambiguous subjects, non-sapient/feral adult presentation, coercive framing, adult assets in safe/public builds, unreviewed provider promotion or prompt text as authority.

## Safe/Public Build Fallback Rules

- Every adult-capable slot must have a deterministic safe fallback or an explicit validation failure.
- Safe and public builds must resolve only safe-approved asset refs.
- Missing, blocked or unreviewed adult-capable candidates must not break Runtime or Unity Player.
- Export filters must fail closed when rating metadata is missing or contradictory.

## Provider Quarantine / Promotion Rules

- Provider integrations are editor-time only and require a separate approved task.
- Provider output starts as `candidateQuarantine`.
- Candidate records require provenance, source metadata, relative paths, byte/hash validation, rating labels and review status before promotion.
- Promotion must bind an approved asset ref through manifests/catalogs.
- Rejected or unreviewed candidates must not be visible to Runtime, Unity Player or safe/public exports.

## GamePackage And Asset-Catalog Source-Of-Truth Rules

- GamePackage and reviewed manifests/catalogs own runtime-facing asset references.
- Visual recipes, part packs and rating metadata are planning or editor-side sources until an approved contract maps them into package/runtime consumers.
- Future public schema changes require a separate contract, consumer proof and manual gate.
- Prompt text, provider job text and generated media files are not authoritative package state.

## Future Implementation Sequence

Use `docs/proposals/VISUAL_MEDIA_PIPELINE_IMPLEMENTATION_ROADMAP.md` as the bounded future sequence. The intended order is:

1. Visual asset contract and rating metadata.
2. Visual rule stack / recipe resolver.
3. Visual detail generator core.
4. Procedural visual part-pack compiler.
5. Pseudo-3D presentation contracts and sidecar proof.
6. Provider candidate quarantine and review ledger.
7. Deterministic safe fallback generation.
8. Adult/rating-gated extension metadata.
9. WinForms review workspace.
10. Unity/player consumption of already-approved asset refs only.

## Stop Conditions For Future Agents

Stop and ask for a new task if integration would require:

- C# source changes outside the selected implementation goal.
- Unity Runtime or project changes before an approved package/asset contract exists.
- Public GamePackage schema mutation without a dedicated contract and consumer proof.
- Provider integration, network calls or media generation in a docs-only or contract-only task.
- Real adult media assets, explicit prompt recipes or prompt dumps.
- Exporting unreviewed or adult-capable candidates into safe/public builds.
