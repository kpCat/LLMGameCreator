# Codex Task — Generation Quality Scorer

## Task ID

`generation-quality-scorer-v1`

## Goal

Add deterministic scoring for generated candidates using score profiles, hard gates and soft metrics.

This task should build on or sit next to Generation Feedback Ledger.

## Read first

- `docs/proposals/GENERATION_QUALITY_SCORING.md`
- `docs/proposals/ADAPTIVE_GENERATOR_FEEDBACK_LOOP.md`
- Existing validator/report patterns.

## Allowed scope

Prefer:

```text
src/LLMGameCreator.Application/Design/AdaptiveGeneration/**
tests/LLMGameCreator.Tests/Application/AdaptiveGeneration/**
```

## Forbidden scope

Do not:

- call LLM/providers;
- use image AI;
- add ML dependencies;
- change GamePackage schema;
- change Unity/player;
- auto-apply tuning patches.

## Required models

- `GenerationQualityScore`
- `ScoreComponent`
- `ScorePenalty`
- `ScoreProfile`
- `HardGateResult`
- `ScoringDiagnostic`
- `GenerationQualityReport`

## Required scoring components

Implement deterministic placeholder/practical scoring for:

- hard gate diagnostics;
- loreFit by required/forbidden tags;
- asset/recipe completeness;
- pseudo3dFit;
- repetition by recipe/tag similarity;
- overDecorationPenalty;
- performanceCost from density/layer counts if available.

## Tests

- forbidden tag causes hard gate failure;
- missing fallback lowers pseudo3dFit / hard gate depending profile;
- repeated candidate gets repetition penalty;
- overdecorated poor dwelling gets penalty;
- same input gives same score;
- score report is deterministic.

## Stop conditions

Stop if scoring requires visual image analysis or external dependencies.

## Final report

Report added models, scoring components, tests and known limitations.
