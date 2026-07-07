param(
    [string]$CanonicalRuntimeTranscriptPath = ".llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-transcript.json",
    [string]$CanonicalRuntimeStateSummaryPath = ".llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-state-summary.json",
    [string]$CanonicalRuntimeDashboardPath = ".llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/canonical-runtime-dashboard.json",
    [string]$OutputRoot = ".llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness",
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
$Goal135RootRelative = ".llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness"
$ExportRootRelative = ".llmgc/exports/goal-135-canonical-runtime-playable-player-loop-readiness"
$UnityProjectPath = Join-Path $RepoRoot "unity/LLMGameCreatorAlpha"
$FallbackUnityPath = "C:\Program Files\Unity\Hub\Editor\6000.1.10f1\Editor\Unity.exe"
$ExecuteMethod = "LLMGameCreatorAlpha.CanonicalRuntimePlayerLoopReadinessAdapter.RunBatchmodeCanonicalRuntimePlayerLoopReadinessSmoke"
$PassMarker = "GOAL135_CANONICAL_RUNTIME_PLAYER_LOOP_READINESS_PASS"
$FailMarker = "GOAL135_CANONICAL_RUNTIME_PLAYER_LOOP_READINESS_FAIL"
$CleanupScript = Join-Path $RepoRoot ".devflow/scripts/clean-unity-editor-noise.ps1"

function Test-Goal135PathUnderRoot {
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

function ConvertTo-Goal135RelativePath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $full = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith($root + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart([System.IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }

    return $full
}

function Resolve-Goal135InputPath {
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
    if (-not (Test-Goal135PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "$Name must stay under the repository root: $Path"
    }

    $relative = ConvertTo-Goal135RelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Name must not point under .llmgc/manual: $relative"
    }

    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) {
        throw "$Name does not exist: $relative"
    }

    return $full
}

function Resolve-Goal135OutputRoot {
    param([Parameter(Mandatory=$true)][string]$Path)

    $candidate = if ([System.IO.Path]::IsPathRooted($Path)) {
        $Path
    }
    else {
        Join-Path $RepoRoot $Path
    }

    $full = [System.IO.Path]::GetFullPath($candidate)
    if (-not (Test-Goal135PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "OutputRoot must stay under the repository root: $Path"
    }

    $relative = ConvertTo-Goal135RelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must not point under .llmgc/manual: $relative"
    }

    if (-not $relative.StartsWith($Goal135RootRelative + "/", [System.StringComparison]::OrdinalIgnoreCase) `
        -and -not $relative.Equals($Goal135RootRelative, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must stay under the Goal135 output root: $relative"
    }

    return $full
}

function Resolve-Goal135UnityPath {
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

function Write-Goal135Json {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)]$Value
    )

    $json = $Value | ConvertTo-Json -Depth 12
    $encoding = New-Object System.Text.UTF8Encoding -ArgumentList $false
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($Path)) | Out-Null
    [System.IO.File]::WriteAllText($Path, $json + [Environment]::NewLine, $encoding)
}

function Invoke-Goal135RuntimeProof {
    param([string]$UnitySmokePath)

    $env:LLMGC_GOAL135_CANONICAL_RUNTIME_TRANSCRIPT_PATH = $ResolvedTranscriptPath
    $env:LLMGC_GOAL135_CANONICAL_RUNTIME_STATE_SUMMARY_PATH = $ResolvedStateSummaryPath
    $env:LLMGC_GOAL135_CANONICAL_RUNTIME_DASHBOARD_PATH = $ResolvedDashboardPath
    $env:LLMGC_GOAL135_OUTPUT_ROOT = $ResolvedOutputRoot
    if ([string]::IsNullOrWhiteSpace($UnitySmokePath)) {
        Remove-Item Env:\LLMGC_GOAL135_UNITY_SMOKE_PATH -ErrorAction SilentlyContinue
    }
    else {
        $env:LLMGC_GOAL135_UNITY_SMOKE_PATH = $UnitySmokePath
    }

    Push-Location $RepoRoot
    try {
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter "FullyQualifiedName~CanonicalRuntimePlayerLoopReadinessScriptRuntimeProof"
        if ($LASTEXITCODE -ne 0) {
            throw "Goal135 player-loop readiness proof test failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}

function Invoke-Goal135UnitySmoke {
    param(
        [Parameter(Mandatory=$true)][string]$ResolvedUnityPath,
        [Parameter(Mandatory=$true)][string]$PlanPath,
        [Parameter(Mandatory=$true)][string]$StateSummaryPath,
        [Parameter(Mandatory=$true)][string]$SmokePath
    )

    $logPath = Join-Path $ResolvedOutputRoot "unity-player-loop-readiness-smoke.log"
    if (-not (Test-Path -LiteralPath $ResolvedUnityPath -PathType Leaf)) {
        $smoke = [ordered]@{
            goalId = "goal_135_canonical_runtime_playable_player_loop_readiness"
            unityAvailable = $false
            planPathExists = Test-Path -LiteralPath $PlanPath -PathType Leaf
            stateSummaryPathExists = Test-Path -LiteralPath $StateSummaryPath -PathType Leaf
            passMarkerPresent = $false
            failMarkerPresent = $false
            requiredStepCategoriesPresent = $false
            canonicalAuthorityMarkersPresent = $false
            unityPlayerLoopReadinessPassed = $false
            passed = $false
            unityPath = $ResolvedUnityPath
            unityLogPath = ConvertTo-Goal135RelativePath -Path $logPath
            planPath = ConvertTo-Goal135RelativePath -Path $PlanPath
            stateSummaryPath = ConvertTo-Goal135RelativePath -Path $StateSummaryPath
            status = "BLOCKED_UNITY_NOT_FOUND"
            diagnostics = @("Unity executable was not found through explicit path, PATH, or known fallback.")
        }
        Write-Goal135Json -Path $SmokePath -Value $smoke
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
        "-llmgcCanonicalRuntimePlayerLoopPlanPath",
        $PlanPath,
        "-llmgcCanonicalRuntimeStateSummaryPath",
        $StateSummaryPath
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
    $requiredCategoriesPresent = $logText.Contains("requiredStepCategoriesPresent=True") `
        -or $logText.Contains("requiredStepCategoriesPresent=true")
    $canonicalAuthorityMarkersPresent = $logText.Contains("canonicalAuthorityMarkersPresent=True") `
        -or $logText.Contains("canonicalAuthorityMarkersPresent=true")
    $passed = $unityExitCode -eq 0 `
        -and $passMarkerPresent `
        -and -not $failMarkerPresent `
        -and $requiredCategoriesPresent `
        -and $canonicalAuthorityMarkersPresent
    $smoke = [ordered]@{
        goalId = "goal_135_canonical_runtime_playable_player_loop_readiness"
        unityAvailable = $true
        planPathExists = Test-Path -LiteralPath $PlanPath -PathType Leaf
        stateSummaryPathExists = Test-Path -LiteralPath $StateSummaryPath -PathType Leaf
        passMarkerPresent = $passMarkerPresent
        failMarkerPresent = $failMarkerPresent
        requiredStepCategoriesPresent = $requiredCategoriesPresent
        canonicalAuthorityMarkersPresent = $canonicalAuthorityMarkersPresent
        unityPlayerLoopReadinessPassed = $passed
        passed = $passed
        unityPath = $ResolvedUnityPath
        unityLogPath = ConvertTo-Goal135RelativePath -Path $logPath
        planPath = ConvertTo-Goal135RelativePath -Path $PlanPath
        stateSummaryPath = ConvertTo-Goal135RelativePath -Path $StateSummaryPath
        status = if ($passed) { "GREEN" } else { "FAILED_UNITY_PLAYER_LOOP_READINESS_SMOKE" }
        diagnostics = @("unityExitCode=$unityExitCode")
    }
    Write-Goal135Json -Path $SmokePath -Value $smoke
    return $passed
}

$ResolvedTranscriptPath = Resolve-Goal135InputPath -Path $CanonicalRuntimeTranscriptPath -Name "CanonicalRuntimeTranscriptPath"
$ResolvedStateSummaryPath = Resolve-Goal135InputPath -Path $CanonicalRuntimeStateSummaryPath -Name "CanonicalRuntimeStateSummaryPath"
$ResolvedDashboardPath = Resolve-Goal135InputPath -Path $CanonicalRuntimeDashboardPath -Name "CanonicalRuntimeDashboardPath"
$ResolvedOutputRoot = Resolve-Goal135OutputRoot -Path $OutputRoot
$ResolvedUnityPath = Resolve-Goal135UnityPath -ExplicitPath $UnityPath
$PlanPath = Join-Path $ResolvedOutputRoot "canonical-runtime-player-loop-plan.json"
$SmokePath = Join-Path $ResolvedOutputRoot "unity-player-loop-readiness-smoke.json"

if ($DryRun) {
    Write-Host "Goal135 canonical runtime player-loop readiness dry run"
    Write-Host "RepoRoot: $RepoRoot"
    Write-Host "CanonicalRuntimeTranscriptPath: $ResolvedTranscriptPath"
    Write-Host "CanonicalRuntimeStateSummaryPath: $ResolvedStateSummaryPath"
    Write-Host "CanonicalRuntimeDashboardPath: $ResolvedDashboardPath"
    Write-Host "OutputRoot: $ResolvedOutputRoot"
    Write-Host "UnityPath: $ResolvedUnityPath"
    Write-Host "RuntimeProof: dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter FullyQualifiedName~CanonicalRuntimePlayerLoopReadinessScriptRuntimeProof"
    Write-Host "UnityExecuteMethod: $ExecuteMethod"
    Write-Host "PassMarker: $PassMarker"
    Write-Host "ExportRoot: $ExportRootRelative"
    return
}

Invoke-Goal135RuntimeProof -UnitySmokePath ""
$unityPassed = Invoke-Goal135UnitySmoke `
    -ResolvedUnityPath $ResolvedUnityPath `
    -PlanPath $PlanPath `
    -StateSummaryPath $ResolvedStateSummaryPath `
    -SmokePath $SmokePath
Invoke-Goal135RuntimeProof -UnitySmokePath $SmokePath

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
    throw "Goal135 Unity/player loop readiness smoke did not pass. See $(ConvertTo-Goal135RelativePath -Path $SmokePath)."
}

Write-Host "Goal135 canonical runtime player-loop readiness passed."
