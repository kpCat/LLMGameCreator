# Goal 141A Player Command Roundtrip Semantic Correctness Hotfix

- status: GREEN
- roundtripSemanticCorrectnessPassed: true
- totalControlRequestCount: 6
- runtimeRoutedRequestCount: 4
- presentationOnlyRequestCount: 2
- runtimeExecutedRequestCount: 4
- presentationOnlyRuntimeExecutionCount: 0
- runtimeMutatingPresentationRequestCount: 0
- responseCount: 6
- requestResponseCorrelationPassed: true
- sequentialCursorContinuityPassed: true
- stateHashContinuityPassed: true
- copySummaryStateUnchanged: true
- loadModelStateUnchanged: true
- playAllExecutedRemainingCommands: true
- noControlIntentMappedToUnrelatedGameplayCommand: true
- runtimeAuthority: true
- projectionOnly: false
- unityGameplayTruth: false

## Regression Proof

- copyFrameSummaryNotMappedToBasicAttack: true
- copyFrameSummaryRuntimeExecutedFalse: true
- copyFrameSummaryStateHashUnchanged: true
- loadModelRuntimeExecutedFalse: true
- loadModelCanonicalStepRuntimeExecutedFalse: true
- runtimeExecutedNotSourcedFromAggregateLoopPassed: true
- requestsCreatedBeforeRuntimeExecution: true
- noFixedControlToSnapshotIndexExecutionProof: true
- runtimeExecutedRequiresExecutedCommandCount: true
- requestResponseIdsMatch: true
- stateHashContinuityPassed: true
- regressionProofPassed: true

## Diagnostics

- unityExitCode=0
- passMarkerPresent=True
- failMarkerPresent=False
- modelPathExists=True
- roundtripRequestCountPassed=True
- presentationOnlyRequestCountPassed=True
- presentationOnlyRuntimeExecutionCountPassed=True
- requestResponseCorrelationPassed=True
- sequentialCursorContinuityPassed=True
- copySummaryStateUnchanged=True
- loadModelStateUnchanged=True
- noControlIntentMappedToUnrelatedGameplayCommand=True
- runtimeSnapshotResponsePresent=True
- runtimeAuthorityMarkersPresent=True
- unityConsumesRoundtripResult=True
- unityGameplayTruth=False
