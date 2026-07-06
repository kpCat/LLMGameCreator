# Goal 132 WinForms Candidate Pipeline Operator Panel

- operatorStatus: GREEN_READY
- normalCommand: .devflow\scripts\run-gamepackage-candidate-recipe-pipeline.cmd
- dryRunCommand: powershell -NoProfile -ExecutionPolicy Bypass -File .devflow\scripts\run-gamepackage-candidate-recipe-pipeline.ps1 -DryRun
- resultPath: .llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/gamepackage-recipe-pipeline-result.json
- selectedCandidateId: minimal-map-game-balanced-baseline
- selectedCandidateScore: 100
- candidateCount: 4
- passedCandidates: 4
- failedCandidates: 0
- matrixPassed: true
- lastOperatorExitCode: -1
- lastOperatorDurationMilliseconds: 0
- manualUnityOptional: true
- projectionOnly: true
- samplePackageReadOnly: true

## Scans

- scriptScanPassed: true
- winFormsPanelPresent: true
- refreshButtonPresent: true
- copyCommandButtonPresent: true
- dryRunButtonPresent: true
- runButtonPresent: true
- asyncRunPresent: true
- negativeProofPassed: true
