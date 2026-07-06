# WinForms Candidate Pipeline Operator Panel

Goal132 adds a WinForms operator panel for the existing Goal131 GamePackage candidate recipe pipeline. The panel surfaces status, command paths and selected-candidate proof, and can run dry-run or cleanup pipeline commands asynchronously.

## Normal Command

- `.devflow\scripts\run-gamepackage-candidate-recipe-pipeline.cmd`

## Current Status

- operatorStatus: GREEN_READY
- resultPath: .llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/gamepackage-recipe-pipeline-result.json
- selectedCandidateId: minimal-map-game-balanced-baseline
- selectedCandidateScore: 100
- candidateCount: 4
- passedCandidates: 4
- failedCandidates: 0
- matrixPassed: true
- manualUnityOptional: true

## Scope Guard

- Manual Unity inspection remains optional.
- The sample package stays read-only.
- Runtime, public schema, provider, Lua, generator-library, final art, Unity Assets, StreamingAssets, ProjectSettings, Packages and release packaging remain outside this goal.
