# Goal 127 Unity Projection Verification Runner

- runnerStatus: GREEN
- runnerScriptPath: .devflow/scripts/run-unity-projection-verification.ps1
- runnerCmdPath: .devflow/scripts/run-unity-projection-verification.cmd
- runnerCommand: .devflow\scripts\run-unity-projection-verification.cmd
- mode: GenericFullPlaythrough
- unityExecuteMethod: LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeGenericGamePackageFullPlaythroughSmoke
- lastResultPath: .llmgc/procedural/goal-127-winforms-unity-projection-verification-runner/unity-projection-verification-runner-result.json
- lastLogPath: .llmgc/procedural/goal-127-winforms-unity-projection-verification-runner/unity-batchmode-generic-full-playthrough-runner.log
- passMarkerPresent: true
- failMarkerAbsent: true
- materialWarningAbsent: true
- cleanupApplied: true
- cleanupScriptAvailable: true
- cleanupCommand: .\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply
- manualUnityClickingRequired: false
- evidencePath: .llmgc/procedural/goal-127-winforms-unity-projection-verification-runner
- exportPath: .llmgc/exports/goal-127-winforms-unity-projection-verification-runner

## Goal126 Evidence

- passed: true
- fullPlaythroughStatusGreen: true
- goal126PassMarkerPresent: true

## Script Scan

- passed: true
- executeMethodPresent: true
- cleanupDelegatesToBoundedScript: true
- noBroadGitClean: true
- noForbiddenMutationTargets: true

## Result Scan

- resultExists: true
- passed: true
- unityExitCode: 0
- cleanupExitCode: 0

## Log Scan

- logExists: true
- passed: true
- forbiddenMarkerCount: 0

## Negative Proof

- passed: true
