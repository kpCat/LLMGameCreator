param(
    [string]$CommandLoopSnapshotsPath = ".llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-snapshots.json",
    [string]$CommandLoopResultPath = ".llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/canonical-runtime-player-command-loop-result.json",
    [string]$PlayerAdapterContractPath = ".llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/canonical-runtime-player-adapter-contract.json",
    [string]$StateSummaryPath = ".llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-state-summary.json",
    [string]$OutputRoot = ".llmgc/procedural/goal-137-canonical-runtime-unity-player-loop-playback-harness",
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
$Goal137RootRelative = ".llmgc/procedural/goal-137-canonical-runtime-unity-player-loop-playback-harness"
$ExportRootRelative = ".llmgc/exports/goal-137-canonical-runtime-unity-player-loop-playback-harness"
$UnityProjectPath = Join-Path $RepoRoot "unity/LLMGameCreatorAlpha"
$FallbackUnityPath = "C:\Program Files\Unity\Hub\Editor\6000.1.10f1\Editor\Unity.exe"
$ExecuteMethod = "LLMGameCreatorAlpha.CanonicalRuntimeUnityPlayerLoopPlaybackAdapter.RunBatchmodeCanonicalRuntimeUnityPlayerLoopPlaybackSmoke"
$PassMarker = "GOAL137_CANONICAL_RUNTIME_UNITY_PLAYER_LOOP_PLAYBACK_PASS"
$FailMarker = "GOAL137_CANONICAL_RUNTIME_UNITY_PLAYER_LOOP_PLAYBACK_FAIL"
$CleanupScript = Join-Path $RepoRoot ".devflow/scripts/clean-unity-editor-noise.ps1"

function Test-Goal137PathUnderRoot {
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

function ConvertTo-Goal137RelativePath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $full = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith($root + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart([System.IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }

    return $full
}

function Resolve-Goal137InputPath {
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
    if (-not (Test-Goal137PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "$Name must stay under the repository root: $Path"
    }

    $relative = ConvertTo-Goal137RelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Name must not point under .llmgc/manual: $relative"
    }

    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) {
        throw "$Name does not exist: $relative"
    }

    return $full
}

function Resolve-Goal137OutputRoot {
    param([Parameter(Mandatory=$true)][string]$Path)

    $candidate = if ([System.IO.Path]::IsPathRooted($Path)) {
        $Path
    }
    else {
        Join-Path $RepoRoot $Path
    }

    $full = [System.IO.Path]::GetFullPath($candidate)
    if (-not (Test-Goal137PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "OutputRoot must stay under the repository root: $Path"
    }

    $relative = ConvertTo-Goal137RelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must not point under .llmgc/manual: $relative"
    }

    if (-not $relative.StartsWith($Goal137RootRelative + "/", [System.StringComparison]::OrdinalIgnoreCase) `
        -and -not $relative.Equals($Goal137RootRelative, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must stay under the Goal137 output root: $relative"
    }

    return $full
}

function Resolve-Goal137UnityPath {
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

function Write-Goal137Json {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)]$Value
    )

    $json = $Value | ConvertTo-Json -Depth 12
    $encoding = New-Object System.Text.UTF8Encoding -ArgumentList $false
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($Path)) | Out-Null
    [System.IO.File]::WriteAllText($Path, $json + [Environment]::NewLine, $encoding)
}

function Invoke-Goal137RuntimeProof {
    param([string]$UnitySmokePath)

    $env:LLMGC_GOAL137_COMMAND_LOOP_SNAPSHOTS_PATH = $ResolvedCommandLoopSnapshotsPath
    $env:LLMGC_GOAL137_COMMAND_LOOP_RESULT_PATH = $ResolvedCommandLoopResultPath
    $env:LLMGC_GOAL137_PLAYER_ADAPTER_CONTRACT_PATH = $ResolvedPlayerAdapterContractPath
    $env:LLMGC_GOAL137_STATE_SUMMARY_PATH = $ResolvedStateSummaryPath
    $env:LLMGC_GOAL137_OUTPUT_ROOT = $ResolvedOutputRoot
    if ([string]::IsNullOrWhiteSpace($UnitySmokePath)) {
        Remove-Item Env:\LLMGC_GOAL137_UNITY_SMOKE_PATH -ErrorAction SilentlyContinue
    }
    else {
        $env:LLMGC_GOAL137_UNITY_SMOKE_PATH = $UnitySmokePath
    }

    Push-Location $RepoRoot
    try {
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter "FullyQualifiedName~CanonicalRuntimeUnityPlayerLoopPlaybackScriptProof"
        if ($LASTEXITCODE -ne 0) {
            throw "Goal137 Unity/player loop playback proof test failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}

function Invoke-Goal137UnitySmoke {
    param(
        [Parameter(Mandatory=$true)][string]$ResolvedUnityPath,
        [Parameter(Mandatory=$true)][string]$FramesPath,
        [Parameter(Mandatory=$true)][string]$ResultPath,
        [Parameter(Mandatory=$true)][string]$SmokePath
    )

    $logPath = Join-Path $ResolvedOutputRoot "unity-player-loop-playback-smoke.log"
    if (-not (Test-Path -LiteralPath $ResolvedUnityPath -PathType Leaf)) {
        $smoke = [ordered]@{
            goalId = "goal_137_canonical_runtime_unity_player_loop_playback_harness"
            unityAvailable = $false
            framesPathExists = Test-Path -LiteralPath $FramesPath -PathType Leaf
            resultPathExists = Test-Path -LiteralPath $ResultPath -PathType Leaf
            passMarkerPresent = $false
            failMarkerPresent = $false
            frameCountPassed = $false
            requiredFrameCategoriesPresent = $false
            runtimeAuthorityMarkersPresent = $false
            unityPlayerLoopPlaybackPassed = $false
            passed = $false
            unityPath = $ResolvedUnityPath
            unityLogPath = ConvertTo-Goal137RelativePath -Path $logPath
            framesPath = ConvertTo-Goal137RelativePath -Path $FramesPath
            resultPath = ConvertTo-Goal137RelativePath -Path $ResultPath
            status = "BLOCKED_UNITY_NOT_FOUND"
            diagnostics = @("Unity executable was not found through explicit path, PATH, or known fallback.")
        }
        Write-Goal137Json -Path $SmokePath -Value $smoke
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
        "-llmgcCanonicalRuntimePlaybackFramesPath",
        $FramesPath,
        "-llmgcCanonicalRuntimePlaybackResultPath",
        $ResultPath
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
        -and $runtimeAuthorityMarkersPresent
    $smoke = [ordered]@{
        goalId = "goal_137_canonical_runtime_unity_player_loop_playback_harness"
        unityAvailable = $true
        framesPathExists = Test-Path -LiteralPath $FramesPath -PathType Leaf
        resultPathExists = Test-Path -LiteralPath $ResultPath -PathType Leaf
        passMarkerPresent = $passMarkerPresent
        failMarkerPresent = $failMarkerPresent
        frameCountPassed = $frameCountPassed
        requiredFrameCategoriesPresent = $requiredFrameCategoriesPresent
        runtimeAuthorityMarkersPresent = $runtimeAuthorityMarkersPresent
        unityPlayerLoopPlaybackPassed = $passed
        passed = $passed
        unityPath = $ResolvedUnityPath
        unityLogPath = ConvertTo-Goal137RelativePath -Path $logPath
        framesPath = ConvertTo-Goal137RelativePath -Path $FramesPath
        resultPath = ConvertTo-Goal137RelativePath -Path $ResultPath
        status = if ($passed) { "GREEN" } else { "FAILED_UNITY_PLAYER_LOOP_PLAYBACK_SMOKE" }
        diagnostics = @("unityExitCode=$unityExitCode")
    }
    Write-Goal137Json -Path $SmokePath -Value $smoke
    return $passed
}

$ResolvedCommandLoopSnapshotsPath = Resolve-Goal137InputPath -Path $CommandLoopSnapshotsPath -Name "CommandLoopSnapshotsPath"
$ResolvedCommandLoopResultPath = Resolve-Goal137InputPath -Path $CommandLoopResultPath -Name "CommandLoopResultPath"
$ResolvedPlayerAdapterContractPath = Resolve-Goal137InputPath -Path $PlayerAdapterContractPath -Name "PlayerAdapterContractPath"
$ResolvedStateSummaryPath = Resolve-Goal137InputPath -Path $StateSummaryPath -Name "StateSummaryPath"
$ResolvedOutputRoot = Resolve-Goal137OutputRoot -Path $OutputRoot
$ResolvedUnityPath = Resolve-Goal137UnityPath -ExplicitPath $UnityPath
$FramesPath = Join-Path $ResolvedOutputRoot "canonical-runtime-unity-player-loop-playback-frames.json"
$ResultPath = Join-Path $ResolvedOutputRoot "canonical-runtime-unity-player-loop-playback-result.json"
$SmokePath = Join-Path $ResolvedOutputRoot "unity-player-loop-playback-smoke.json"

if ($DryRun) {
    Write-Host "Goal137 canonical runtime Unity/player loop playback dry run"
    Write-Host "RepoRoot: $RepoRoot"
    Write-Host "CommandLoopSnapshotsPath: $ResolvedCommandLoopSnapshotsPath"
    Write-Host "CommandLoopResultPath: $ResolvedCommandLoopResultPath"
    Write-Host "PlayerAdapterContractPath: $ResolvedPlayerAdapterContractPath"
    Write-Host "StateSummaryPath: $ResolvedStateSummaryPath"
    Write-Host "OutputRoot: $ResolvedOutputRoot"
    Write-Host "UnityPath: $ResolvedUnityPath"
    Write-Host "RuntimeProof: dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter FullyQualifiedName~CanonicalRuntimeUnityPlayerLoopPlaybackScriptProof"
    Write-Host "UnityExecuteMethod: $ExecuteMethod"
    Write-Host "PassMarker: $PassMarker"
    Write-Host "ExportRoot: $ExportRootRelative"
    return
}

Invoke-Goal137RuntimeProof -UnitySmokePath ""
$unityPassed = Invoke-Goal137UnitySmoke `
    -ResolvedUnityPath $ResolvedUnityPath `
    -FramesPath $FramesPath `
    -ResultPath $ResultPath `
    -SmokePath $SmokePath
Invoke-Goal137RuntimeProof -UnitySmokePath $SmokePath

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
    throw "Goal137 Unity/player loop playback smoke did not pass. See $(ConvertTo-Goal137RelativePath -Path $SmokePath)."
}

Write-Host "Goal137 canonical runtime Unity/player loop playback passed."
