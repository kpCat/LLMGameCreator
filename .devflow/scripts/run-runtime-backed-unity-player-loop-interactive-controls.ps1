param(
    [string]$StepperModelPath = ".llmgc/procedural/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness/runtime-backed-player-loop-stepper-model.json",
    [string]$StepperResultPath = ".llmgc/procedural/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness/runtime-backed-player-loop-stepper-result.json",
    [string]$PlaybackFramesPath = ".llmgc/procedural/goal-137-canonical-runtime-unity-player-loop-playback-harness/canonical-runtime-unity-player-loop-playback-frames.json",
    [string]$CommandLoopSnapshotsPath = ".llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-snapshots.json",
    [string]$PlayerAdapterContractPath = ".llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/canonical-runtime-player-adapter-contract.json",
    [string]$OutputRoot = ".llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness",
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
$Goal139RootRelative = ".llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness"
$ExportRootRelative = ".llmgc/exports/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness"
$UnityProjectPath = Join-Path $RepoRoot "unity/LLMGameCreatorAlpha"
$FallbackUnityPath = "C:\Program Files\Unity\Hub\Editor\6000.1.10f1\Editor\Unity.exe"
$ExecuteMethod = "LLMGameCreatorAlpha.CanonicalRuntimeUnityPlayerLoopInteractiveControlsHarness.RunBatchmodeRuntimeBackedUnityPlayerLoopInteractiveControlsSmoke"
$PassMarker = "GOAL139_RUNTIME_BACKED_UNITY_PLAYER_LOOP_INTERACTIVE_CONTROLS_PASS"
$FailMarker = "GOAL139_RUNTIME_BACKED_UNITY_PLAYER_LOOP_INTERACTIVE_CONTROLS_FAIL"
$CleanupScript = Join-Path $RepoRoot ".devflow/scripts/clean-unity-editor-noise.ps1"
$InteractiveControlsWindowSource = Join-Path $RepoRoot "unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnityPlayerLoopInteractiveControlsWindow.cs"

function Test-Goal139PathUnderRoot {
    param(
        [Parameter(Mandatory=$true)][string]$RootPath,
        [Parameter(Mandatory=$true)][string]$CandidatePath
    )

    $root = [System.IO.Path]::GetFullPath($RootPath).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    $candidate = [System.IO.Path]::GetFullPath($CandidatePath)
    return $candidate.StartsWith(
        $root + [System.IO.Path]::DirectorySeparatorChar,
        [System.StringComparison]::OrdinalIgnoreCase)
}

function ConvertTo-Goal139RelativePath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $full = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith($root + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart([System.IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }

    return $full
}

function Resolve-Goal139InputPath {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$Name
    )

    $candidate = if ([System.IO.Path]::IsPathRooted($Path)) {
        $Path
    }
    else {
        Join-Path $RepoRoot $Path
    }

    $full = [System.IO.Path]::GetFullPath($candidate)
    if (-not (Test-Goal139PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "$Name must stay under the repository root: $Path"
    }

    $relative = ConvertTo-Goal139RelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Name must not point under .llmgc/manual: $relative"
    }

    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) {
        throw "$Name does not exist: $relative"
    }

    return $full
}

function Resolve-Goal139OutputRoot {
    param([Parameter(Mandatory=$true)][string]$Path)

    $candidate = if ([System.IO.Path]::IsPathRooted($Path)) {
        $Path
    }
    else {
        Join-Path $RepoRoot $Path
    }

    $full = [System.IO.Path]::GetFullPath($candidate)
    if (-not (Test-Goal139PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "OutputRoot must stay under the repository root: $Path"
    }

    $relative = ConvertTo-Goal139RelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must not point under .llmgc/manual: $relative"
    }

    if (-not $relative.StartsWith($Goal139RootRelative + "/", [System.StringComparison]::OrdinalIgnoreCase) `
        -and -not $relative.Equals($Goal139RootRelative, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must stay under the Goal139 output root: $relative"
    }

    return $full
}

function Resolve-Goal139UnityPath {
    param([string]$ExplicitPath)

    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {
        return [System.IO.Path]::GetFullPath($ExplicitPath)
    }

    $command = Get-Command "Unity.exe" -ErrorAction SilentlyContinue
    if ($null -ne $command -and -not [string]::IsNullOrWhiteSpace($command.Source)) {
        return [System.IO.Path]::GetFullPath($command.Source)
    }

    return $FallbackUnityPath
}

function Write-Goal139Json {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)]$Value
    )

    $json = $Value | ConvertTo-Json -Depth 12
    $encoding = New-Object System.Text.UTF8Encoding -ArgumentList $false
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($Path)) | Out-Null
    [System.IO.File]::WriteAllText($Path, $json + [Environment]::NewLine, $encoding)
}

function Invoke-Goal139RuntimeProof {
    param([string]$UnitySmokePath)

    $env:LLMGC_GOAL139_STEPPER_MODEL_PATH = $ResolvedStepperModelPath
    $env:LLMGC_GOAL139_STEPPER_RESULT_PATH = $ResolvedStepperResultPath
    $env:LLMGC_GOAL139_PLAYBACK_FRAMES_PATH = $ResolvedPlaybackFramesPath
    $env:LLMGC_GOAL139_COMMAND_LOOP_SNAPSHOTS_PATH = $ResolvedCommandLoopSnapshotsPath
    $env:LLMGC_GOAL139_PLAYER_ADAPTER_CONTRACT_PATH = $ResolvedPlayerAdapterContractPath
    $env:LLMGC_GOAL139_OUTPUT_ROOT = $ResolvedOutputRoot
    if ([string]::IsNullOrWhiteSpace($UnitySmokePath)) {
        Remove-Item Env:\LLMGC_GOAL139_UNITY_SMOKE_PATH -ErrorAction SilentlyContinue
    }
    else {
        $env:LLMGC_GOAL139_UNITY_SMOKE_PATH = $UnitySmokePath
    }

    Push-Location $RepoRoot
    try {
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter "FullyQualifiedName~RuntimeBackedUnityPlayerLoopInteractiveControlsScriptProof"
        if ($LASTEXITCODE -ne 0) {
            throw "Goal139 runtime-backed Unity player-loop interactive controls proof test failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}

function Invoke-Goal139UnitySmoke {
    param(
        [Parameter(Mandatory=$true)][string]$ResolvedUnityPath,
        [Parameter(Mandatory=$true)][string]$ModelPath,
        [Parameter(Mandatory=$true)][string]$ControlScriptPath,
        [Parameter(Mandatory=$true)][string]$SmokePath
    )

    $logPath = Join-Path $ResolvedOutputRoot "unity-player-loop-interactive-controls-smoke.log"
    $interactiveControlsWindowPresent = Test-Path -LiteralPath $InteractiveControlsWindowSource -PathType Leaf
    if (-not (Test-Path -LiteralPath $ResolvedUnityPath -PathType Leaf)) {
        $smoke = [ordered]@{
            goalId = "goal_139_runtime_backed_unity_player_loop_interactive_controls_harness"
            unityAvailable = $false
            interactiveModelPathExists = Test-Path -LiteralPath $ModelPath -PathType Leaf
            controlScriptPathExists = Test-Path -LiteralPath $ControlScriptPath -PathType Leaf
            frameCountPassed = $false
            requiredControlsPresent = $false
            controlScriptPassed = $false
            runtimeAuthorityMarkersPresent = $false
            interactiveControlsWindowPresent = $interactiveControlsWindowPresent
            unityGameplayTruth = $false
            passed = $false
            unityPath = $ResolvedUnityPath
            unityLogPath = ConvertTo-Goal139RelativePath -Path $logPath
            interactiveModelPath = ConvertTo-Goal139RelativePath -Path $ModelPath
            controlScriptPath = ConvertTo-Goal139RelativePath -Path $ControlScriptPath
            status = "BLOCKED_UNITY_NOT_FOUND"
            diagnostics = @("Unity executable was not found through explicit path, PATH, or known fallback.")
        }
        Write-Goal139Json -Path $SmokePath -Value $smoke
        return $false
    }

    $args = @(
        "-batchmode",
        "-quit",
        "-projectPath",
        $UnityProjectPath,
        "-executeMethod",
        $ExecuteMethod,
        "-logFile",
        $logPath,
        "-llmgcRuntimeBackedUnityPlayerLoopInteractiveControlsModelPath",
        $ModelPath,
        "-llmgcRuntimeBackedUnityPlayerLoopInteractiveControlsScriptPath",
        $ControlScriptPath
    )

    Push-Location $RepoRoot
    try {
        & $ResolvedUnityPath @args
        $unityExitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }

    $logText = ""
    if (Test-Path -LiteralPath $logPath) {
        $logText = Get-Content -LiteralPath $logPath -Raw -Encoding UTF8
    }

    $passMarkerPresent = $logText.Contains($PassMarker)
    $failMarkerPresent = $logText.Contains($FailMarker)
    $frameCountPassed = $logText.Contains("frameCountPassed=True") `
        -or $logText.Contains("frameCountPassed=true")
    $requiredControlsPresent = $logText.Contains("requiredControlsPresent=True") `
        -or $logText.Contains("requiredControlsPresent=true")
    $controlScriptPassed = $logText.Contains("controlScriptPassed=True") `
        -or $logText.Contains("controlScriptPassed=true")
    $runtimeAuthorityMarkersPresent = $logText.Contains("runtimeAuthorityMarkersPresent=True") `
        -or $logText.Contains("runtimeAuthorityMarkersPresent=true")
    $passed = $unityExitCode -eq 0 `
        -and $passMarkerPresent `
        -and -not $failMarkerPresent `
        -and $frameCountPassed `
        -and $requiredControlsPresent `
        -and $controlScriptPassed `
        -and $runtimeAuthorityMarkersPresent `
        -and $interactiveControlsWindowPresent
    $smoke = [ordered]@{
        goalId = "goal_139_runtime_backed_unity_player_loop_interactive_controls_harness"
        unityAvailable = $true
        interactiveModelPathExists = Test-Path -LiteralPath $ModelPath -PathType Leaf
        controlScriptPathExists = Test-Path -LiteralPath $ControlScriptPath -PathType Leaf
        frameCountPassed = $frameCountPassed
        requiredControlsPresent = $requiredControlsPresent
        controlScriptPassed = $controlScriptPassed
        runtimeAuthorityMarkersPresent = $runtimeAuthorityMarkersPresent
        interactiveControlsWindowPresent = $interactiveControlsWindowPresent
        unityGameplayTruth = $false
        passed = $passed
        unityPath = $ResolvedUnityPath
        unityLogPath = ConvertTo-Goal139RelativePath -Path $logPath
        interactiveModelPath = ConvertTo-Goal139RelativePath -Path $ModelPath
        controlScriptPath = ConvertTo-Goal139RelativePath -Path $ControlScriptPath
        status = if ($passed) { "GREEN" } else { "FAILED_UNITY_PLAYER_LOOP_INTERACTIVE_CONTROLS_SMOKE" }
        diagnostics = @("unityExitCode=$unityExitCode")
    }
    Write-Goal139Json -Path $SmokePath -Value $smoke
    return $passed
}

$ResolvedStepperModelPath = Resolve-Goal139InputPath -Path $StepperModelPath -Name "StepperModelPath"
$ResolvedStepperResultPath = Resolve-Goal139InputPath -Path $StepperResultPath -Name "StepperResultPath"
$ResolvedPlaybackFramesPath = Resolve-Goal139InputPath -Path $PlaybackFramesPath -Name "PlaybackFramesPath"
$ResolvedCommandLoopSnapshotsPath = Resolve-Goal139InputPath -Path $CommandLoopSnapshotsPath -Name "CommandLoopSnapshotsPath"
$ResolvedPlayerAdapterContractPath = Resolve-Goal139InputPath -Path $PlayerAdapterContractPath -Name "PlayerAdapterContractPath"
$ResolvedOutputRoot = Resolve-Goal139OutputRoot -Path $OutputRoot
$ResolvedUnityPath = Resolve-Goal139UnityPath -ExplicitPath $UnityPath
$ModelPath = Join-Path $ResolvedOutputRoot "runtime-backed-player-loop-interactive-controls-model.json"
$ControlScriptPath = Join-Path $ResolvedOutputRoot "runtime-backed-player-loop-interactive-controls-script.json"
$SmokePath = Join-Path $ResolvedOutputRoot "unity-player-loop-interactive-controls-smoke.json"

if ($DryRun) {
    Write-Host "Goal139 runtime-backed Unity player-loop interactive controls dry run"
    Write-Host "RepoRoot: $RepoRoot"
    Write-Host "StepperModelPath: $ResolvedStepperModelPath"
    Write-Host "StepperResultPath: $ResolvedStepperResultPath"
    Write-Host "PlaybackFramesPath: $ResolvedPlaybackFramesPath"
    Write-Host "CommandLoopSnapshotsPath: $ResolvedCommandLoopSnapshotsPath"
    Write-Host "PlayerAdapterContractPath: $ResolvedPlayerAdapterContractPath"
    Write-Host "OutputRoot: $ResolvedOutputRoot"
    Write-Host "UnityPath: $ResolvedUnityPath"
    Write-Host "RuntimeProof: dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter FullyQualifiedName~RuntimeBackedUnityPlayerLoopInteractiveControlsScriptProof"
    Write-Host "UnityExecuteMethod: $ExecuteMethod"
    Write-Host "PassMarker: $PassMarker"
    Write-Host "ExportRoot: $ExportRootRelative"
    return
}

Invoke-Goal139RuntimeProof -UnitySmokePath ""
$unityPassed = Invoke-Goal139UnitySmoke `
    -ResolvedUnityPath $ResolvedUnityPath `
    -ModelPath $ModelPath `
    -ControlScriptPath $ControlScriptPath `
    -SmokePath $SmokePath
Invoke-Goal139RuntimeProof -UnitySmokePath $SmokePath

if ($ApplyCleanup) {
    & $CleanupScript -Apply
    if ($LASTEXITCODE -ne 0) {
        $firstCleanupExitCode = $LASTEXITCODE
        Start-Sleep -Seconds 1
        & $CleanupScript -Apply
        if ($LASTEXITCODE -ne 0) {
            throw "Unity cleanup failed with exit code $LASTEXITCODE after retry; first exit code was $firstCleanupExitCode."
        }
    }
}

if (-not $unityPassed) {
    throw "Goal139 Unity/player loop interactive controls smoke did not pass. See $(ConvertTo-Goal139RelativePath -Path $SmokePath)."
}

Write-Host "Goal139 runtime-backed Unity player-loop interactive controls passed."
