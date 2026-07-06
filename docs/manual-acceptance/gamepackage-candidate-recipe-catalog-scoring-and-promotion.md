# GamePackage Candidate Recipe Catalog Scoring and Promotion

Goal131 adds a deterministic recipe catalog over the existing GamePackage candidate matrix route. The catalog creates projection-safe metadata-only candidates, scores only candidates that pass the real matrix runner, and writes a selected candidate handoff for manual review.

## Normal Command

- `.devflow\scripts\run-gamepackage-candidate-recipe-pipeline.cmd`

## Status

- recipePipelineStatus: GREEN
- recipeCount: 4
- candidateCount: 4
- passedCandidates: 4
- failedCandidates: 0
- matrixPassed: true
- selectedCandidateId: minimal-map-game-balanced-baseline
- selectedCandidateScore: 100
- pipelineResultPath: .llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/gamepackage-recipe-pipeline-result.json
- scoringResultPath: .llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/candidate-scoring-result.json
- selectedCandidatePackagePath: .llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/package.json
- manualUnityOptional: true
- samplePackageUnmodified: true
- projectionOnly: true

## Scope Guard

- The sample package remains read-only.
- Generated candidates stay under Goal131 procedural artifacts.
- The selected candidate is copied only to the Goal131 selected-candidate handoff path.
- Runtime, public schema, provider, Lua, generator-library, Unity Assets, ProjectSettings, Packages, StreamingAssets and release packaging remain outside this goal.
