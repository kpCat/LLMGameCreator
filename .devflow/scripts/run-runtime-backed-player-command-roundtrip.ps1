param(
    [string]$SelectedCandidatePackagePath = ".llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/package.json",
    [string]$SelectedCandidateHandoffPath = ".llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/selected-candidate-handoff.json",
    [string]$ControlsUxModelPath = ".llmgc/procedural/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard/runtime-backed-player-loop-controls-ux-model.json",
    [string]$ControlsUxResultPath = ".llmgc/procedural/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard/runtime-backed-player-loop-controls-ux-result.json",
    [string]$ControlsUxScriptPath = ".llmgc/procedural/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard/runtime-backed-player-loop-controls-ux-script.json",
    [string]$CommandLoopSnapshotsPath = ".llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-snapshots.json",
    [string]$CommandLoopResultPath = ".llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-result.json",
    [string]$OutputRoot = ".llmgc/procedural/goal-141-runtime-backed-unity-player-command-roundtrip-bridge",
    [string]$UnityPath = "",
    [switch]$DryRun,
    [switch]$ApplyCleanup
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment

$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$Goal141RootRelative = ".llmgc/procedural/goal-141-runtime-backed-unity-player-command-roundtrip-bridge"
$ExportRootRelative = ".llmgc/exports/goal-141-runtime-backed-unity-player-command-roundtrip-bridge"
$UnityProjectPath = Join-Path $RepoRoot "unity/LLMGameCreatorAlpha"
$ExecuteMethod = "LLMGameCreatorAlpha.CanonicalRuntimeUnityPlayerCommandRoundtripHarness.RunBatchmodeRuntimeBackedPlayerCommandRoundtripSmoke"
$PassMarker = "GOAL141_RUNTIME_BACKED_PLAYER_COMMAND_ROUNDTRIP_PASS"
$FailMarker = "GOAL141_RUNTIME_BACKED_PLAYER_COMMAND_ROUNDTRIP_FAIL"

function Test-Goal141PathUnderRoot {
    param(
        [Parameter(Mandatory=$true)][string]$RootPath,
        [Parameter(Mandatory=$true)][string]$CandidatePath
    )

    $root = [System.IO.Path]::GetFullPath($RootPath).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    $candidate = [System.IO.Path]::GetFullPath($CandidatePath)
    return $candidate.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)
}

function ConvertTo-Goal141RelativePath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $full = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith($root + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart([System.IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }

    return $full
}

function Assert-Goal141NotManualPath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $relative = ConvertTo-Goal141RelativePath -Path $Path
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::Ordinal)) {
        throw "Goal141 refuses .llmgc/manual input or output: $relative"
    }
}

function Resolve-Goal141InputPath {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$Name
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw "$Name is required."
    }

    $full = if ([System.IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }
    $full = [System.IO.Path]::GetFullPath($full)
    if (-not (Test-Goal141PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "$Name must stay under repository root: $Path"
    }

    Assert-Goal141NotManualPath -Path $full
    return $full
}

function Resolve-Goal141OutputRoot {
    param([Parameter(Mandatory=$true)][string]$Path)

    $full = if ([System.IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }
    $full = [System.IO.Path]::GetFullPath($full)
    if (-not (Test-Goal141PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "OutputRoot must stay under repository root: $Path"
    }

    Assert-Goal141NotManualPath -Path $full
    $relative = ConvertTo-Goal141RelativePath -Path $full
    if (-not $relative.StartsWith($Goal141RootRelative + "/", [System.StringComparison]::OrdinalIgnoreCase) `
        -and -not $relative.Equals($Goal141RootRelative, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must stay under the Goal141 output root: $relative"
    }

    return $full
}

function Resolve-Goal141UnityPath {
    param([string]$ExplicitPath)

    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {
        return [System.IO.Path]::GetFullPath($ExplicitPath)
    }

    $command = Get-Command "Unity.exe" -ErrorAction SilentlyContinue
    if ($null -ne $command -and -not [string]::IsNullOrWhiteSpace($command.Source)) {
        return [System.IO.Path]::GetFullPath($command.Source)
    }

    $candidates = @(
        "C:\Program Files\Unity\Hub\Editor\6000.1.10f1\Editor\Unity.exe",
        "C:\Program Files\Unity\Hub\Editor\6000.1.9f1\Editor\Unity.exe",
        "C:\Program Files\Unity\Hub\Editor\6000.0.43f1\Editor\Unity.exe"
    )
    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return $candidate
        }
    }

    return ""
}

function Write-Goal141Json {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)]$Value
    )

    $json = $Value | ConvertTo-Json -Depth 16
    $directory = Split-Path -Parent $Path
    if (-not (Test-Path -LiteralPath $directory -PathType Container)) {
        New-Item -ItemType Directory -Path $directory | Out-Null
    }

    [System.IO.File]::WriteAllText($Path, $json + [Environment]::NewLine, [System.Text.UTF8Encoding]::new($false))
}

function Test-Goal141Contains {
    param(
        [string]$Text,
        [string]$Needle
    )

    return -not [string]::IsNullOrEmpty($Text) `
        -and $Text.IndexOf($Needle, [System.StringComparison]::Ordinal) -ge 0
}

function Invoke-Goal141RuntimeProof {
    param(
        [Parameter(Mandatory=$true)][string]$ResolvedOutputRoot,
        [string]$UnitySmokePath
    )

    Push-Location $RepoRoot
    try {
        $env:LLMGC_GOAL141_OUTPUT_ROOT = ConvertTo-Goal141RelativePath -Path $ResolvedOutputRoot
        $env:LLMGC_GOAL141_SELECTED_CANDIDATE_PACKAGE_PATH = $ResolvedSelectedCandidatePackagePath
        $env:LLMGC_GOAL141_SELECTED_CANDIDATE_HANDOFF_PATH = $ResolvedSelectedCandidateHandoffPath
        $env:LLMGC_GOAL141_CONTROLS_UX_MODEL_PATH = $ResolvedControlsUxModelPath
        $env:LLMGC_GOAL141_CONTROLS_UX_RESULT_PATH = $ResolvedControlsUxResultPath
        $env:LLMGC_GOAL141_CONTROLS_UX_SCRIPT_PATH = $ResolvedControlsUxScriptPath
        $env:LLMGC_GOAL141_COMMAND_LOOP_SNAPSHOTS_PATH = $ResolvedCommandLoopSnapshotsPath
        $env:LLMGC_GOAL141_COMMAND_LOOP_RESULT_PATH = $ResolvedCommandLoopResultPath
        if ([string]::IsNullOrWhiteSpace($UnitySmokePath)) {
            Remove-Item Env:\LLMGC_GOAL141_UNITY_SMOKE_PATH -ErrorAction SilentlyContinue
        } else {
            $env:LLMGC_GOAL141_UNITY_SMOKE_PATH = $UnitySmokePath
        }

        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter "FullyQualifiedName~RuntimeBackedPlayerCommandRoundtripScriptProof"
        if ($LASTEXITCODE -ne 0) {
            throw "Goal141 runtime-backed player command roundtrip proof test failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Remove-Item Env:\LLMGC_GOAL141_OUTPUT_ROOT -ErrorAction SilentlyContinue
        Remove-Item Env:\LLMGC_GOAL141_SELECTED_CANDIDATE_PACKAGE_PATH -ErrorAction SilentlyContinue
        Remove-Item Env:\LLMGC_GOAL141_SELECTED_CANDIDATE_HANDOFF_PATH -ErrorAction SilentlyContinue
        Remove-Item Env:\LLMGC_GOAL141_CONTROLS_UX_MODEL_PATH -ErrorAction SilentlyContinue
        Remove-Item Env:\LLMGC_GOAL141_CONTROLS_UX_RESULT_PATH -ErrorAction SilentlyContinue
        Remove-Item Env:\LLMGC_GOAL141_CONTROLS_UX_SCRIPT_PATH -ErrorAction SilentlyContinue
        Remove-Item Env:\LLMGC_GOAL141_COMMAND_LOOP_SNAPSHOTS_PATH -ErrorAction SilentlyContinue
        Remove-Item Env:\LLMGC_GOAL141_COMMAND_LOOP_RESULT_PATH -ErrorAction SilentlyContinue
        Remove-Item Env:\LLMGC_GOAL141_UNITY_SMOKE_PATH -ErrorAction SilentlyContinue
        Pop-Location
    }
}

function Invoke-Goal141UnitySmoke {
    param(
        [Parameter(Mandatory=$true)][string]$ResolvedOutputRoot,
        [Parameter(Mandatory=$true)][string]$SmokePath
    )

    $modelPath = Join-Path $ResolvedOutputRoot "runtime-backed-player-command-roundtrip-model.json"
    $resultPath = Join-Path $ResolvedOutputRoot "runtime-backed-player-command-roundtrip-result.json"
    $logPath = Join-Path $ResolvedOutputRoot "unity-player-command-roundtrip-smoke.log"

    if ([string]::IsNullOrWhiteSpace($ResolvedUnityPath) -or -not (Test-Path -LiteralPath $ResolvedUnityPath -PathType Leaf)) {
        $smoke = [ordered]@{
            goalId = "goal_141_runtime_backed_unity_player_command_roundtrip_bridge"
            unityAvailable = $false
            modelPathExists = Test-Path -LiteralPath $modelPath -PathType Leaf
            roundtripRequestCountPassed = $false
            presentationOnlyRequestCountPassed = $false
            presentationOnlyRuntimeExecutionCountPassed = $false
            requestResponseCorrelationPassed = $false
            sequentialCursorContinuityPassed = $false
            copySummaryStateUnchanged = $false
            loadModelStateUnchanged = $false
            noControlIntentMappedToUnrelatedGameplayCommand = $false
            runtimeSnapshotResponsePresent = $false
            runtimeAuthorityMarkersPresent = $false
            unityConsumesRoundtripResult = $false
            unityGameplayTruth = $false
            passMarkerPresent = $false
            failMarkerPresent = $false
            passed = $false
            unityPath = $ResolvedUnityPath
            unityLogPath = ConvertTo-Goal141RelativePath -Path $logPath
            modelPath = ConvertTo-Goal141RelativePath -Path $modelPath
            resultPath = ConvertTo-Goal141RelativePath -Path $resultPath
            status = "BLOCKED_UNITY_NOT_FOUND"
            diagnostics = @("Unity executable was not found.")
        }
        Write-Goal141Json -Path $SmokePath -Value $smoke
        return $false
    }

    $args = @(
        "-batchmode",
        "-quit",
        "-nographics",
        "-projectPath",
        $UnityProjectPath,
        "-executeMethod",
        $ExecuteMethod,
        "-llmgcRuntimeBackedPlayerCommandRoundtripModelPath",
        $modelPath,
        "-llmgcRuntimeBackedPlayerCommandRoundtripResultPath",
        $resultPath,
        "-logFile",
        $logPath
    )

    & $ResolvedUnityPath @args
    $unityExitCode = $LASTEXITCODE
    $logText = if (Test-Path -LiteralPath $logPath -PathType Leaf) {
        [System.IO.File]::ReadAllText($logPath)
    } else {
        ""
    }

    $passMarkerPresent = Test-Goal141Contains -Text $logText -Needle $PassMarker
    $failMarkerPresent = Test-Goal141Contains -Text $logText -Needle $FailMarker
    $modelPathExists = (Test-Goal141Contains -Text $logText -Needle "modelPathExists=True") `
        -or (Test-Goal141Contains -Text $logText -Needle "modelPathExists=true")
    $requestCountPassed = (Test-Goal141Contains -Text $logText -Needle "roundtripRequestCountPassed=True") `
        -or (Test-Goal141Contains -Text $logText -Needle "roundtripRequestCountPassed=true")
    $presentationOnlyRequestCountPassed = (Test-Goal141Contains -Text $logText -Needle "presentationOnlyRequestCountPassed=True") `
        -or (Test-Goal141Contains -Text $logText -Needle "presentationOnlyRequestCountPassed=true")
    $presentationOnlyRuntimeExecutionCountPassed = (Test-Goal141Contains -Text $logText -Needle "presentationOnlyRuntimeExecutionCountPassed=True") `
        -or (Test-Goal141Contains -Text $logText -Needle "presentationOnlyRuntimeExecutionCountPassed=true")
    $requestResponseCorrelationPassed = (Test-Goal141Contains -Text $logText -Needle "requestResponseCorrelationPassed=True") `
        -or (Test-Goal141Contains -Text $logText -Needle "requestResponseCorrelationPassed=true")
    $sequentialCursorContinuityPassed = (Test-Goal141Contains -Text $logText -Needle "sequentialCursorContinuityPassed=True") `
        -or (Test-Goal141Contains -Text $logText -Needle "sequentialCursorContinuityPassed=true")
    $copySummaryStateUnchanged = (Test-Goal141Contains -Text $logText -Needle "copySummaryStateUnchanged=True") `
        -or (Test-Goal141Contains -Text $logText -Needle "copySummaryStateUnchanged=true")
    $loadModelStateUnchanged = (Test-Goal141Contains -Text $logText -Needle "loadModelStateUnchanged=True") `
        -or (Test-Goal141Contains -Text $logText -Needle "loadModelStateUnchanged=true")
    $noControlIntentMappedToUnrelatedGameplayCommand = (Test-Goal141Contains -Text $logText -Needle "noControlIntentMappedToUnrelatedGameplayCommand=True") `
        -or (Test-Goal141Contains -Text $logText -Needle "noControlIntentMappedToUnrelatedGameplayCommand=true")
    $snapshotResponsePresent = (Test-Goal141Contains -Text $logText -Needle "runtimeSnapshotResponsePresent=True") `
        -or (Test-Goal141Contains -Text $logText -Needle "runtimeSnapshotResponsePresent=true")
    $runtimeAuthorityMarkersPresent = (Test-Goal141Contains -Text $logText -Needle "runtimeAuthorityMarkersPresent=True") `
        -or (Test-Goal141Contains -Text $logText -Needle "runtimeAuthorityMarkersPresent=true")
    $unityConsumesRoundtripResult = (Test-Goal141Contains -Text $logText -Needle "unityConsumesRoundtripResult=True") `
        -or (Test-Goal141Contains -Text $logText -Needle "unityConsumesRoundtripResult=true")
    $unityGameplayTruth = (Test-Goal141Contains -Text $logText -Needle "unityGameplayTruth=True") `
        -or (Test-Goal141Contains -Text $logText -Needle "unityGameplayTruth=true")
    $passed = $unityExitCode -eq 0 `
        -and $passMarkerPresent `
        -and -not $failMarkerPresent `
        -and $modelPathExists `
        -and $requestCountPassed `
        -and $presentationOnlyRequestCountPassed `
        -and $presentationOnlyRuntimeExecutionCountPassed `
        -and $requestResponseCorrelationPassed `
        -and $sequentialCursorContinuityPassed `
        -and $copySummaryStateUnchanged `
        -and $loadModelStateUnchanged `
        -and $noControlIntentMappedToUnrelatedGameplayCommand `
        -and $snapshotResponsePresent `
        -and $runtimeAuthorityMarkersPresent `
        -and $unityConsumesRoundtripResult `
        -and -not $unityGameplayTruth

    $diagnostics = @(
        "unityExitCode=$unityExitCode",
        "passMarkerPresent=$passMarkerPresent",
        "failMarkerPresent=$failMarkerPresent",
        "modelPathExists=$modelPathExists",
        "roundtripRequestCountPassed=$requestCountPassed",
        "presentationOnlyRequestCountPassed=$presentationOnlyRequestCountPassed",
        "presentationOnlyRuntimeExecutionCountPassed=$presentationOnlyRuntimeExecutionCountPassed",
        "requestResponseCorrelationPassed=$requestResponseCorrelationPassed",
        "sequentialCursorContinuityPassed=$sequentialCursorContinuityPassed",
        "copySummaryStateUnchanged=$copySummaryStateUnchanged",
        "loadModelStateUnchanged=$loadModelStateUnchanged",
        "noControlIntentMappedToUnrelatedGameplayCommand=$noControlIntentMappedToUnrelatedGameplayCommand",
        "runtimeSnapshotResponsePresent=$snapshotResponsePresent",
        "runtimeAuthorityMarkersPresent=$runtimeAuthorityMarkersPresent",
        "unityConsumesRoundtripResult=$unityConsumesRoundtripResult",
        "unityGameplayTruth=$unityGameplayTruth"
    )

    $smoke = [ordered]@{
        goalId = "goal_141_runtime_backed_unity_player_command_roundtrip_bridge"
        unityAvailable = $true
        modelPathExists = $modelPathExists
        roundtripRequestCountPassed = $requestCountPassed
        presentationOnlyRequestCountPassed = $presentationOnlyRequestCountPassed
        presentationOnlyRuntimeExecutionCountPassed = $presentationOnlyRuntimeExecutionCountPassed
        requestResponseCorrelationPassed = $requestResponseCorrelationPassed
        sequentialCursorContinuityPassed = $sequentialCursorContinuityPassed
        copySummaryStateUnchanged = $copySummaryStateUnchanged
        loadModelStateUnchanged = $loadModelStateUnchanged
        noControlIntentMappedToUnrelatedGameplayCommand = $noControlIntentMappedToUnrelatedGameplayCommand
        runtimeSnapshotResponsePresent = $snapshotResponsePresent
        runtimeAuthorityMarkersPresent = $runtimeAuthorityMarkersPresent
        unityConsumesRoundtripResult = $unityConsumesRoundtripResult
        unityGameplayTruth = $unityGameplayTruth
        passMarkerPresent = $passMarkerPresent
        failMarkerPresent = $failMarkerPresent
        passed = $passed
        unityPath = $ResolvedUnityPath
        unityLogPath = ConvertTo-Goal141RelativePath -Path $logPath
        modelPath = ConvertTo-Goal141RelativePath -Path $modelPath
        resultPath = ConvertTo-Goal141RelativePath -Path $resultPath
        status = if ($passed) { "GREEN" } else { "BLOCKED" }
        diagnostics = $diagnostics
    }
    Write-Goal141Json -Path $SmokePath -Value $smoke
    return $passed
}

$ResolvedSelectedCandidatePackagePath = Resolve-Goal141InputPath -Path $SelectedCandidatePackagePath -Name "SelectedCandidatePackagePath"
$ResolvedSelectedCandidateHandoffPath = Resolve-Goal141InputPath -Path $SelectedCandidateHandoffPath -Name "SelectedCandidateHandoffPath"
$ResolvedControlsUxModelPath = Resolve-Goal141InputPath -Path $ControlsUxModelPath -Name "ControlsUxModelPath"
$ResolvedControlsUxResultPath = Resolve-Goal141InputPath -Path $ControlsUxResultPath -Name "ControlsUxResultPath"
$ResolvedControlsUxScriptPath = Resolve-Goal141InputPath -Path $ControlsUxScriptPath -Name "ControlsUxScriptPath"
$ResolvedCommandLoopSnapshotsPath = Resolve-Goal141InputPath -Path $CommandLoopSnapshotsPath -Name "CommandLoopSnapshotsPath"
$ResolvedCommandLoopResultPath = Resolve-Goal141InputPath -Path $CommandLoopResultPath -Name "CommandLoopResultPath"
$ResolvedOutputRoot = Resolve-Goal141OutputRoot -Path $OutputRoot
$ResolvedUnityPath = Resolve-Goal141UnityPath -ExplicitPath $UnityPath

if ($DryRun) {
    Write-Host "Goal141 runtime-backed player command roundtrip dry run"
    Write-Host "SelectedCandidatePackagePath: $ResolvedSelectedCandidatePackagePath"
    Write-Host "SelectedCandidateHandoffPath: $ResolvedSelectedCandidateHandoffPath"
    Write-Host "ControlsUxModelPath: $ResolvedControlsUxModelPath"
    Write-Host "ControlsUxResultPath: $ResolvedControlsUxResultPath"
    Write-Host "ControlsUxScriptPath: $ResolvedControlsUxScriptPath"
    Write-Host "CommandLoopSnapshotsPath: $ResolvedCommandLoopSnapshotsPath"
    Write-Host "CommandLoopResultPath: $ResolvedCommandLoopResultPath"
    Write-Host "OutputRoot: $ResolvedOutputRoot"
    Write-Host "UnityPath: $ResolvedUnityPath"
    Write-Host "RuntimeProof: dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter FullyQualifiedName~RuntimeBackedPlayerCommandRoundtripScriptProof"
    Write-Host "UnityExecuteMethod: $ExecuteMethod"
    Write-Host "PassMarker: $PassMarker"
    Write-Host "FailMarker: $FailMarker"
    exit 0
}

if (-not (Test-Path -LiteralPath $ResolvedOutputRoot -PathType Container)) {
    New-Item -ItemType Directory -Path $ResolvedOutputRoot | Out-Null
}

Invoke-Goal141RuntimeProof -ResolvedOutputRoot $ResolvedOutputRoot -UnitySmokePath ""
$smokePath = Join-Path $ResolvedOutputRoot "unity-player-command-roundtrip-smoke.json"
$unityPassed = Invoke-Goal141UnitySmoke -ResolvedOutputRoot $ResolvedOutputRoot -SmokePath $smokePath
Invoke-Goal141RuntimeProof -ResolvedOutputRoot $ResolvedOutputRoot -UnitySmokePath $smokePath

if ($ApplyCleanup) {
    & (Join-Path $RepoRoot ".devflow/scripts/clean-unity-editor-noise.ps1") -Apply
    if ($LASTEXITCODE -ne 0) {
        throw "Unity editor noise cleanup failed with exit code $LASTEXITCODE."
    }
}

if (-not $unityPassed) {
    throw "Goal141 Unity/player command roundtrip smoke did not pass. See $(ConvertTo-Goal141RelativePath -Path $smokePath)."
}

Write-Host "Goal141 runtime-backed player command roundtrip passed."
