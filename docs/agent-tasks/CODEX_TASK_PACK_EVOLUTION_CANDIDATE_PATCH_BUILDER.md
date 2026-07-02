# Codex Task — Pack Evolution Candidate Patch Builder

## Task ID

`pack-evolution-candidate-patch-builder-v1`

## Goal

Add a safe candidate patch builder that analyzes feedback/scoring reports and proposes pack tuning patches without applying them.

## Read first

- `docs/proposals/PACK_EVOLUTION_CANDIDATE_PATCHES.md`
- `docs/proposals/ADAPTIVE_GENERATOR_FEEDBACK_LOOP.md`
- `docs/proposals/GENERATION_QUALITY_SCORING.md`

## Allowed scope

Prefer:

```text
src/LLMGameCreator.Application/Design/AdaptiveGeneration/**
tests/LLMGameCreator.Tests/Application/AdaptiveGeneration/**
```

## Forbidden scope

Do not:

- auto-apply patches;
- change active packs;
- change GamePackage schema;
- change Unity/player;
- call LLM/providers;
- change production docs outside candidate report unless requested.

## Required models

- `PackEvolutionCandidatePatch`
- `PackEvolutionPatchChange`
- `WeightAdjustmentChange`
- `ForbiddenCombinationChange`
- `PreferredCombinationChange`
- `DensityLimitChange`
- `PatchEvidence`
- `PatchValidationReport`
- `PatchDiffReport`

## Required behavior

- build candidate patch from feedback records;
- include evidence ids;
- include old/new values when input pack provides them;
- validate patch shape;
- produce human-readable diff report;
- never apply patch automatically.

## Initial patch kinds

- increase/decrease weight;
- add forbidden combination;
- add preferred combination;
- add density limit.

## Tests

- repeated rejection creates decrease weight suggestion;
- repeated approval creates increase weight suggestion;
- rejected combination creates forbidden combination patch;
- patch without evidence is invalid;
- patch builder does not mutate source pack;
- diff report is deterministic.

## Stop conditions

Stop if real pack mutation is required.

## Final report

Report models, builder behavior, tests and limitations.
