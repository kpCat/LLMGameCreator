param(
    [string]$PlaybackFramesPath = ".llmgc/procedural/goal-137-canonical-runtime-unity-player-loop-playback-harness/canonical-runtime-unity-player-loop-playback-frames.json",
    [string]$PlaybackResultPath = ".llmgc/procedural/goal-137-canonical-runtime-unity-player-loop-playback-harness/canonical-runtime-unity-player-loop-playback-result.json",
    [string]$CommandLoopSnapshotsPath = ".llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-snapshots.json",
    [string]$CommandLoopResultPath = ".llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-result.json",
    [string]$PlayerAdapterContractPath = ".llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/canonical-runtime-player-adapter-contract.json",
    [string]$OutputRoot = ".llmgc/procedural/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness",
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
$Goal138RootRelative = ".llmgc/procedural/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness"
$ExportRootRelative = ".llmgc/exports/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness"
$UnityProjectPath = Join-Path $RepoRoot "unity/LLMGameCreatorAlpha"
$FallbackUnityPath = "C:\Program Files\Unity\Hub\Editor\6000.1.10f1\Editor\Unity.exe"
$ExecuteMethod = "LLMGameCreatorAlpha.CanonicalRuntimeUnityPlayerLoopStepperHarness.RunBatchmodeRuntimeBackedUnityPlayerLoopStepperSmoke"
$PassMarker = "GOAL138_RUNTIME_BACKED_UNITY_PLAYER_LOOP_STEPPER_PASS"
$FailMarker = "GOAL138_RUNTIME_BACKED_UNITY_PLAYER_LOOP_STEPPER_FAIL"
$CleanupScript = Join-Path $RepoRoot ".devflow/scripts/clean-unity-editor-noise.ps1"
$StepperWindowSource = Join-Path $RepoRoot "unity/LLMGameCreatorAlpha/Assets/Editor/CanonicalRuntimeUnityPlayerLoopStepperWindow.cs"

function Test-Goal138PathUnderRoot {
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

function ConvertTo-Goal138RelativePath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $full = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith($root + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart([System.IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }

    return $full
}

function Resolve-Goal138InputPath {
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
    if (-not (Test-Goal138PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "$Name must stay under the repository root: $Path"
    }

    $relative = ConvertTo-Goal138RelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Name must not point under .llmgc/manual: $relative"
    }

    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) {
        throw "$Name does not exist: $relative"
    }

    return $full
}

function Resolve-Goal138OutputRoot {
    param([Parameter(Mandatory=$true)][string]$Path)

    $candidate = if ([System.IO.Path]::IsPathRooted($Path)) {
        $Path
    }
    else {
        Join-Path $RepoRoot $Path
    }

    $full = [System.IO.Path]::GetFullPath($candidate)
    if (-not (Test-Goal138PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "OutputRoot must stay under the repository root: $Path"
    }

    $relative = ConvertTo-Goal138RelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must not point under .llmgc/manual: $relative"
    }

    if (-not $relative.StartsWith($Goal138RootRelative + "/", [System.StringComparison]::OrdinalIgnoreCase) `
        -and -not $relative.Equals($Goal138RootRelative, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must stay under the Goal138 output root: $relative"
    }

    return $full
}

function Resolve-Goal138UnityPath {
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

function Write-Goal138Json {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)]$Value
    )

    $json = $Value | ConvertTo-Json -Depth 12
    $encoding = New-Object System.Text.UTF8Encoding -ArgumentList $false
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($Path)) | Out-Null
    [System.IO.File]::WriteAllText($Path, $json + [Environment]::NewLine, $encoding)
}

function Invoke-Goal138RuntimeProof {
    param([string]$UnitySmokePath)

    $env:LLMGC_GOAL138_PLAYBACK_FRAMES_PATH = $ResolvedPlaybackFramesPath
    $env:LLMGC_GOAL138_PLAYBACK_RESULT_PATH = $ResolvedPlaybackResultPath
    $env:LLMGC_GOAL138_COMMAND_LOOP_SNAPSHOTS_PATH = $ResolvedCommandLoopSnapshotsPath
    $env:LLMGC_GOAL138_COMMAND_LOOP_RESULT_PATH = $ResolvedCommandLoopResultPath
    $env:LLMGC_GOAL138_PLAYER_ADAPTER_CONTRACT_PATH = $ResolvedPlayerAdapterContractPath
    $env:LLMGC_GOAL138_OUTPUT_ROOT = $ResolvedOutputRoot
    if ([string]::IsNullOrWhiteSpace($UnitySmokePath)) {
        Remove-Item Env:\LLMGC_GOAL138_UNITY_SMOKE_PATH -ErrorAction SilentlyContinue
    }
    else {
        $env:LLMGC_GOAL138_UNITY_SMOKE_PATH = $UnitySmokePath
    }

    Push-Location $RepoRoot
    try {
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter "FullyQualifiedName~RuntimeBackedUnityPlayerLoopStepperScriptProof"
        if ($LASTEXITCODE -ne 0) {
            throw "Goal138 runtime-backed Unity player-loop stepper proof test failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}

function Invoke-Goal138UnitySmoke {
    param(
        [Parameter(Mandatory=$true)][string]$ResolvedUnityPath,
        [Parameter(Mandatory=$true)][string]$ModelPath,
        [Parameter(Mandatory=$true)][string]$SmokePath
    )

    $logPath = Join-Path $ResolvedOutputRoot "unity-player-loop-stepper-smoke.log"
    $stepperWindowPresent = Test-Path -LiteralPath $StepperWindowSource -PathType Leaf
    if (-not (Test-Path -LiteralPath $ResolvedUnityPath -PathType Leaf)) {
        $smoke = [ordered]@{
            goalId = "goal_138_runtime_backed_unity_player_loop_stepper_hud_harness"
            unityAvailable = $false
            modelPathExists = Test-Path -LiteralPath $ModelPath -PathType Leaf
            passMarkerPresent = $false
            failMarkerPresent = $false
            frameCountPassed = $false
            requiredFrameCategoriesPresent = $false
            runtimeAuthorityMarkersPresent = $false
            stepperWindowPresent = $stepperWindowPresent
            stepperBatchSmokePassed = $false
            passed = $false
            unityPath = $ResolvedUnityPath
            unityLogPath = ConvertTo-Goal138RelativePath -Path $logPath
            modelPath = ConvertTo-Goal138RelativePath -Path $ModelPath
            status = "BLOCKED_UNITY_NOT_FOUND"
            diagnostics = @("Unity executable was not found through explicit path, PATH, or known fallback.")
        }
        Write-Goal138Json -Path $SmokePath -Value $smoke
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
        "-llmgcRuntimeBackedUnityPlayerLoopStepperModelPath",
        $ModelPath
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
    $requiredFrameCategoriesPresent = $logText.Contains("requiredFrameCategoriesPresent=True") `
        -or $logText.Contains("requiredFrameCategoriesPresent=true")
    $runtimeAuthorityMarkersPresent = $logText.Contains("runtimeAuthorityMarkersPresent=True") `
        -or $logText.Contains("runtimeAuthorityMarkersPresent=true")
    $passed = $unityExitCode -eq 0 `
        -and $passMarkerPresent `
        -and -not $failMarkerPresent `
        -and $frameCountPassed `
        -and $requiredFrameCategoriesPresent `
        -and $runtimeAuthorityMarkersPresent `
        -and $stepperWindowPresent
    $smoke = [ordered]@{
        goalId = "goal_138_runtime_backed_unity_player_loop_stepper_hud_harness"
        unityAvailable = $true
        modelPathExists = Test-Path -LiteralPath $ModelPath -PathType Leaf
        passMarkerPresent = $passMarkerPresent
        failMarkerPresent = $failMarkerPresent
        frameCountPassed = $frameCountPassed
        requiredFrameCategoriesPresent = $requiredFrameCategoriesPresent
        runtimeAuthorityMarkersPresent = $runtimeAuthorityMarkersPresent
        stepperWindowPresent = $stepperWindowPresent
        stepperBatchSmokePassed = $passed
        passed = $passed
        unityPath = $ResolvedUnityPath
        unityLogPath = ConvertTo-Goal138RelativePath -Path $logPath
        modelPath = ConvertTo-Goal138RelativePath -Path $ModelPath
        status = if ($passed) { "GREEN" } else { "FAILED_UNITY_PLAYER_LOOP_STEPPER_SMOKE" }
        diagnostics = @("unityExitCode=$unityExitCode")
    }
    Write-Goal138Json -Path $SmokePath -Value $smoke
    return $passed
}

$ResolvedPlaybackFramesPath = Resolve-Goal138InputPath -Path $PlaybackFramesPath -Name "PlaybackFramesPath"
$ResolvedPlaybackResultPath = Resolve-Goal138InputPath -Path $PlaybackResultPath -Name "PlaybackResultPath"
$ResolvedCommandLoopSnapshotsPath = Resolve-Goal138InputPath -Path $CommandLoopSnapshotsPath -Name "CommandLoopSnapshotsPath"
$ResolvedCommandLoopResultPath = Resolve-Goal138InputPath -Path $CommandLoopResultPath -Name "CommandLoopResultPath"
$ResolvedPlayerAdapterContractPath = Resolve-Goal138InputPath -Path $PlayerAdapterContractPath -Name "PlayerAdapterContractPath"
$ResolvedOutputRoot = Resolve-Goal138OutputRoot -Path $OutputRoot
$ResolvedUnityPath = Resolve-Goal138UnityPath -ExplicitPath $UnityPath
$ModelPath = Join-Path $ResolvedOutputRoot "runtime-backed-player-loop-stepper-model.json"
$SmokePath = Join-Path $ResolvedOutputRoot "unity-player-loop-stepper-smoke.json"

if ($DryRun) {
    Write-Host "Goal138 runtime-backed Unity player-loop stepper dry run"
    Write-Host "RepoRoot: $RepoRoot"
    Write-Host "PlaybackFramesPath: $ResolvedPlaybackFramesPath"
    Write-Host "PlaybackResultPath: $ResolvedPlaybackResultPath"
    Write-Host "CommandLoopSnapshotsPath: $ResolvedCommandLoopSnapshotsPath"
    Write-Host "CommandLoopResultPath: $ResolvedCommandLoopResultPath"
    Write-Host "PlayerAdapterContractPath: $ResolvedPlayerAdapterContractPath"
    Write-Host "OutputRoot: $ResolvedOutputRoot"
    Write-Host "UnityPath: $ResolvedUnityPath"
    Write-Host "RuntimeProof: dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter FullyQualifiedName~RuntimeBackedUnityPlayerLoopStepperScriptProof"
    Write-Host "UnityExecuteMethod: $ExecuteMethod"
    Write-Host "PassMarker: $PassMarker"
    Write-Host "ExportRoot: $ExportRootRelative"
    return
}

Invoke-Goal138RuntimeProof -UnitySmokePath ""
$unityPassed = Invoke-Goal138UnitySmoke `
    -ResolvedUnityPath $ResolvedUnityPath `
    -ModelPath $ModelPath `
    -SmokePath $SmokePath
Invoke-Goal138RuntimeProof -UnitySmokePath $SmokePath

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
    throw "Goal138 Unity/player loop stepper smoke did not pass. See $(ConvertTo-Goal138RelativePath -Path $SmokePath)."
}

Write-Host "Goal138 runtime-backed Unity player-loop stepper passed."
