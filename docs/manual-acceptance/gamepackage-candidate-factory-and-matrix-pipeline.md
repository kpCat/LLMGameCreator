# GamePackage Candidate Factory and Matrix Pipeline

Goal130 adds a deterministic repo-local GamePackage candidate factory that materializes projection-compatible packages and runs the Goal129 matrix runner over the generated index.

## Normal Command

- `.devflow\scripts\run-gamepackage-candidate-factory.cmd`

## Status

- candidateFactoryStatus: GREEN
- candidateCount: 3
- passedCandidates: 3
- failedCandidates: 0
- matrixPassed: true
- candidateIndexPath: .llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/gamepackage-candidate-index.json
- factoryResultPath: .llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/gamepackage-candidate-factory-result.json
- matrixResultPath: .llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/gamepackage-projection-matrix-result.json
- manualUnityOptional: true
- samplePackageUnmodified: true
- projectionOnly: true

## Scope Guard

- The sample package remains read-only.
- Candidate packages stay under Goal130 artifacts.
- Runtime, public schema, provider, Lua, generator-library, Unity Assets, ProjectSettings, Packages, StreamingAssets and release packaging remain outside this goal.
