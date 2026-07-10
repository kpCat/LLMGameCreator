# Goal142A Operator Self-Lock Hotfix

- status: GREEN
- operatorUsesInProcessService: true
- operatorStartsCompilerProcess: false
- operatorStartsDotnetTestProcess: false
- runningWinFormsOutputCopyAttempt: false
- buttonDisabledWhileRunning: true
- workspaceRefreshedAfterSuccess: true
- previousArtifactsPreservedOnFailure: true
- partialArtifactsRemovedOrRolledBackOnFailure: true
- successfulRunRegeneratesGoal142Artifacts: true
- manualAttemptFailureRecorded: true
- goal142Accepted: false

The WinForms `Run Runtime Variant Matrix` action uses
`ProductLineRuntimeVariantMatrixOperatorRunner` and the existing in-process
`ProductLineRuntimeVariantMatrixService`. It no longer launches the external
PowerShell proof route.

The operator runner snapshots both canonical Goal142 roots outside the
repository, removes stale state, and restores the previous roots exactly when
generation throws. The failure-injection regression writes partial procedural
and export artifacts before throwing and verifies restoration byte-for-byte.

The external PowerShell command remains available for automation. It now takes
the same out-of-repository snapshot before optional cleanup and restores it on
test, proof, hash, or dashboard failure.

The observed `exitCode=1` attempt is recorded as an operator defect and is not
Goal142 acceptance. One corrected WinForms button retry remains required.
