# Goal 129 GamePackage Candidate Matrix Projection Runner

- matrixStatus: GREEN
- candidateCount: 2
- passedCandidateCount: 2
- failedCandidateCount: 0
- candidateIndexPath: .llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/gamepackage-candidate-index.json
- matrixResultPath: .llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/gamepackage-projection-matrix-result.json
- normalCommand: .devflow\scripts\run-gamepackage-projection-matrix.cmd
- exampleCommand: .devflow\scripts\run-gamepackage-projection-matrix.cmd -CandidateIndexPath .llmgc\procedural\goal-129-gamepackage-candidate-matrix-projection-runner\gamepackage-candidate-index.json
- baselineCandidatePackagePath: .llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/candidates/minimal-map-game-baseline/package.json
- variantCandidatePackagePath: .llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/candidates/minimal-map-game-variant/package.json
- manualUnityOptional: true
- cleanupApplied: true
- projectionOnly: true

## Candidate Index

- passed: true
- candidateCount: 2

## Script Scan

- passed: true
- invokesParameterizedUnityProjectionRunner: true
- supportsPerCandidateResultAndLogPaths: true

## Matrix Result

- resultExists: true
- passed: true
- allEntriesPassed: true

## Log Scan

- passed: true
- candidateLogScanCount: 2

## Negative Proof

- passed: true
