param(
    [string]$SelectedCandidateHandoffPath = ".llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/selected-candidate-handoff.json",
    [string]$SelectedCandidatePackagePath = ".llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/package.json",
    [string]$Goal134TranscriptPath = ".llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-transcript.json",
    [string]$Goal134StateSummaryPath = ".llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-state-summary.json",
    [string]$Goal135PlayerLoopPlanPath = ".llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/canonical-runtime-player-loop-plan.json",
    [string]$Goal135PlayerAdapterContractPath = ".llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/canonical-runtime-player-adapter-contract.json",
    [string]$OutputRoot = ".llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix",
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
$Goal136RootRelative = ".llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix"
$ExportRootRelative = ".llmgc/exports/goal-136-canonical-runtime-player-command-loop-execution-matrix"
$UnityProjectPath = Join-Path $RepoRoot "unity/LLMGameCreatorAlpha"
$FallbackUnityPath = "C:\Program Files\Unity\Hub\Editor\6000.1.10f1\Editor\Unity.exe"
$ExecuteMethod = "LLMGameCreatorAlpha.CanonicalRuntimePlayerCommandLoopAdapter.RunBatchmodeCanonicalRuntimePlayerCommandLoopSmoke"
$PassMarker = "GOAL136_CANONICAL_RUNTIME_PLAYER_COMMAND_LOOP_PASS"
$FailMarker = "GOAL136_CANONICAL_RUNTIME_PLAYER_COMMAND_LOOP_FAIL"
$CleanupScript = Join-Path $RepoRoot ".devflow/scripts/clean-unity-editor-noise.ps1"

function Test-Goal136PathUnderRoot {
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

function ConvertTo-Goal136RelativePath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $full = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith($root + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart([System.IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }

    return $full
}

function Resolve-Goal136InputPath {
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
    if (-not (Test-Goal136PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "$Name must stay under the repository root: $Path"
    }

    $relative = ConvertTo-Goal136RelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Name must not point under .llmgc/manual: $relative"
    }

    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) {
        throw "$Name does not exist: $relative"
    }

    return $full
}

function Resolve-Goal136OutputRoot {
    param([Parameter(Mandatory=$true)][string]$Path)

    $candidate = if ([System.IO.Path]::IsPathRooted($Path)) {
        $Path
    }
    else {
        Join-Path $RepoRoot $Path
    }

    $full = [System.IO.Path]::GetFullPath($candidate)
    if (-not (Test-Goal136PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "OutputRoot must stay under the repository root: $Path"
    }

    $relative = ConvertTo-Goal136RelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must not point under .llmgc/manual: $relative"
    }

    if (-not $relative.StartsWith($Goal136RootRelative + "/", [System.StringComparison]::OrdinalIgnoreCase) `
        -and -not $relative.Equals($Goal136RootRelative, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must stay under the Goal136 output root: $relative"
    }

    return $full
}

function Resolve-Goal136UnityPath {
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

function Write-Goal136Json {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)]$Value
    )

    $json = $Value | ConvertTo-Json -Depth 12
    $encoding = New-Object System.Text.UTF8Encoding -ArgumentList $false
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($Path)) | Out-Null
    [System.IO.File]::WriteAllText($Path, $json + [Environment]::NewLine, $encoding)
}

function Invoke-Goal136RuntimeProof {
    param([string]$UnitySmokePath)

    $env:LLMGC_GOAL136_SELECTED_CANDIDATE_HANDOFF_PATH = $ResolvedHandoffPath
    $env:LLMGC_GOAL136_SELECTED_CANDIDATE_PACKAGE_PATH = $ResolvedPackagePath
    $env:LLMGC_GOAL136_GOAL134_TRANSCRIPT_PATH = $ResolvedGoal134TranscriptPath
    $env:LLMGC_GOAL136_GOAL134_STATE_SUMMARY_PATH = $ResolvedGoal134StateSummaryPath
    $env:LLMGC_GOAL136_GOAL135_PLAYER_LOOP_PLAN_PATH = $ResolvedGoal135PlayerLoopPlanPath
    $env:LLMGC_GOAL136_GOAL135_PLAYER_ADAPTER_CONTRACT_PATH = $ResolvedGoal135PlayerAdapterContractPath
    $env:LLMGC_GOAL136_OUTPUT_ROOT = $ResolvedOutputRoot
    if ([string]::IsNullOrWhiteSpace($UnitySmokePath)) {
        Remove-Item Env:\LLMGC_GOAL136_UNITY_SMOKE_PATH -ErrorAction SilentlyContinue
    }
    else {
        $env:LLMGC_GOAL136_UNITY_SMOKE_PATH = $UnitySmokePath
    }

    Push-Location $RepoRoot
    try {
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter "FullyQualifiedName~CanonicalRuntimePlayerCommandLoopScriptRuntimeProof"
        if ($LASTEXITCODE -ne 0) {
            throw "Goal136 player command-loop proof test failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}

function Invoke-Goal136UnitySmoke {
    param(
        [Parameter(Mandatory=$true)][string]$ResolvedUnityPath,
        [Parameter(Mandatory=$true)][string]$SnapshotsPath,
        [Parameter(Mandatory=$true)][string]$ResultPath,
        [Parameter(Mandatory=$true)][string]$SmokePath
    )

    $logPath = Join-Path $ResolvedOutputRoot "unity-player-command-loop-smoke.log"
    if (-not (Test-Path -LiteralPath $ResolvedUnityPath -PathType Leaf)) {
        $smoke = [ordered]@{
            goalId = "goal_136_canonical_runtime_player_command_loop_execution_matrix"
            unityAvailable = $false
            snapshotsPathExists = Test-Path -LiteralPath $SnapshotsPath -PathType Leaf
            resultPathExists = Test-Path -LiteralPath $ResultPath -PathType Leaf
            passMarkerPresent = $false
            failMarkerPresent = $false
            snapshotContractPresent = $false
            unityPlayerConsumedCommandLoopSnapshots = $false
            passed = $false
            unityPath = $ResolvedUnityPath
            unityLogPath = ConvertTo-Goal136RelativePath -Path $logPath
            snapshotsPath = ConvertTo-Goal136RelativePath -Path $SnapshotsPath
            resultPath = ConvertTo-Goal136RelativePath -Path $ResultPath
            status = "BLOCKED_UNITY_NOT_FOUND"
            diagnostics = @("Unity executable was not found through explicit path, PATH, or known fallback.")
        }
        Write-Goal136Json -Path $SmokePath -Value $smoke
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
        "-llmgcCanonicalRuntimePlayerCommandLoopSnapshotsPath",
        $SnapshotsPath,
        "-llmgcCanonicalRuntimePlayerCommandLoopResultPath",
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
    $snapshotContractPresent = $logText.Contains("snapshotContractPresent=True") `
        -or $logText.Contains("snapshotContractPresent=true")
    $passed = $unityExitCode -eq 0 `
        -and $passMarkerPresent `
        -and -not $failMarkerPresent `
        -and $snapshotContractPresent
    $smoke = [ordered]@{
        goalId = "goal_136_canonical_runtime_player_command_loop_execution_matrix"
        unityAvailable = $true
        snapshotsPathExists = Test-Path -LiteralPath $SnapshotsPath -PathType Leaf
        resultPathExists = Test-Path -LiteralPath $ResultPath -PathType Leaf
        passMarkerPresent = $passMarkerPresent
        failMarkerPresent = $failMarkerPresent
        snapshotContractPresent = $snapshotContractPresent
        unityPlayerConsumedCommandLoopSnapshots = $passed
        passed = $passed
        unityPath = $ResolvedUnityPath
        unityLogPath = ConvertTo-Goal136RelativePath -Path $logPath
        snapshotsPath = ConvertTo-Goal136RelativePath -Path $SnapshotsPath
        resultPath = ConvertTo-Goal136RelativePath -Path $ResultPath
        status = if ($passed) { "GREEN" } else { "FAILED_UNITY_PLAYER_COMMAND_LOOP_SMOKE" }
        diagnostics = @("unityExitCode=$unityExitCode")
    }
    Write-Goal136Json -Path $SmokePath -Value $smoke
    return $passed
}

$ResolvedHandoffPath = Resolve-Goal136InputPath -Path $SelectedCandidateHandoffPath -Name "SelectedCandidateHandoffPath"
$ResolvedPackagePath = Resolve-Goal136InputPath -Path $SelectedCandidatePackagePath -Name "SelectedCandidatePackagePath"
$ResolvedGoal134TranscriptPath = Resolve-Goal136InputPath -Path $Goal134TranscriptPath -Name "Goal134TranscriptPath"
$ResolvedGoal134StateSummaryPath = Resolve-Goal136InputPath -Path $Goal134StateSummaryPath -Name "Goal134StateSummaryPath"
$ResolvedGoal135PlayerLoopPlanPath = Resolve-Goal136InputPath -Path $Goal135PlayerLoopPlanPath -Name "Goal135PlayerLoopPlanPath"
$ResolvedGoal135PlayerAdapterContractPath = Resolve-Goal136InputPath -Path $Goal135PlayerAdapterContractPath -Name "Goal135PlayerAdapterContractPath"
$ResolvedOutputRoot = Resolve-Goal136OutputRoot -Path $OutputRoot
$ResolvedUnityPath = Resolve-Goal136UnityPath -ExplicitPath $UnityPath
$SnapshotsPath = Join-Path $ResolvedOutputRoot "canonical-runtime-player-command-loop-snapshots.json"
$ResultPath = Join-Path $ResolvedOutputRoot "canonical-runtime-player-command-loop-result.json"
$SmokePath = Join-Path $ResolvedOutputRoot "unity-player-command-loop-smoke.json"

if ($DryRun) {
    Write-Host "Goal136 canonical runtime player command-loop dry run"
    Write-Host "RepoRoot: $RepoRoot"
    Write-Host "SelectedCandidateHandoffPath: $ResolvedHandoffPath"
    Write-Host "SelectedCandidatePackagePath: $ResolvedPackagePath"
    Write-Host "Goal134TranscriptPath: $ResolvedGoal134TranscriptPath"
    Write-Host "Goal134StateSummaryPath: $ResolvedGoal134StateSummaryPath"
    Write-Host "Goal135PlayerLoopPlanPath: $ResolvedGoal135PlayerLoopPlanPath"
    Write-Host "Goal135PlayerAdapterContractPath: $ResolvedGoal135PlayerAdapterContractPath"
    Write-Host "OutputRoot: $ResolvedOutputRoot"
    Write-Host "UnityPath: $ResolvedUnityPath"
    Write-Host "RuntimeProof: dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter FullyQualifiedName~CanonicalRuntimePlayerCommandLoopScriptRuntimeProof"
    Write-Host "UnityExecuteMethod: $ExecuteMethod"
    Write-Host "PassMarker: $PassMarker"
    Write-Host "ExportRoot: $ExportRootRelative"
    return
}

Invoke-Goal136RuntimeProof -UnitySmokePath ""
$unityPassed = Invoke-Goal136UnitySmoke `
    -ResolvedUnityPath $ResolvedUnityPath `
    -SnapshotsPath $SnapshotsPath `
    -ResultPath $ResultPath `
    -SmokePath $SmokePath
Invoke-Goal136RuntimeProof -UnitySmokePath $SmokePath

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
    throw "Goal136 Unity/player command-loop snapshot smoke did not pass. See $(ConvertTo-Goal136RelativePath -Path $SmokePath)."
}

Write-Host "Goal136 canonical runtime player command-loop passed."
