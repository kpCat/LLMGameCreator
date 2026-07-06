# GamePackage Candidate Matrix Projection Runner

Goal129 adds deterministic candidate GamePackage matrix verification over the parameterized Unity projection runner.
The baseline candidate is a byte-copy of `samples/minimal-map-game/package.json`; the variant keeps the Goal128 full-playthrough package identity/title compatibility fields while changing version, description and visible labels.

## Normal Command

- `.devflow\scripts\run-gamepackage-projection-matrix.cmd`

## Example Command

- `.devflow\scripts\run-gamepackage-projection-matrix.cmd -CandidateIndexPath .llmgc\procedural\goal-129-gamepackage-candidate-matrix-projection-runner\gamepackage-candidate-index.json`

## Status

- matrixStatus: GREEN
- candidateCount: 2
- passedCandidateCount: 2
- failedCandidateCount: 0
- candidateIndexPath: .llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/gamepackage-candidate-index.json
- matrixResultPath: .llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/gamepackage-projection-matrix-result.json
- manualUnityOptional: true
- cleanupApplied: true
- projectionOnly: true

## Scope Guard

- Candidate package paths stay under Goal129 artifacts and outside `.llmgc/manual/`.
- This remains projection-only and does not authorize sample mutation, Runtime, public schema, provider, Lua, generator-library, Unity scene, prefab, ProjectSettings, Packages, StreamingAssets or release packaging work.
