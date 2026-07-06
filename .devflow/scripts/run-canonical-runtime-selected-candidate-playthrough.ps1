param(
    [string]$SelectedCandidateHandoffPath = ".llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/selected-candidate-handoff.json",
    [string]$SelectedCandidatePackagePath = ".llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/package.json",
    [string]$OutputRoot = ".llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix",
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
$Goal134RootRelative = ".llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix"
$ExportRootRelative = ".llmgc/exports/goal-134-canonical-runtime-selected-candidate-playthrough-matrix"
$UnityProjectPath = Join-Path $RepoRoot "unity/LLMGameCreatorAlpha"
$FallbackUnityPath = "C:\Program Files\Unity\Hub\Editor\6000.1.10f1\Editor\Unity.exe"
  $ExecuteMethod = "LLMGameCreatorAlpha.CanonicalRuntimeSelectedCandidateTranscriptAdapter.RunBatchmodeCanonicalRuntimeSelectedCandidateTranscriptSmoke"
$PassMarker = "GOAL134_CANONICAL_RUNTIME_TRANSCRIPT_PLAYER_PASS"
$FailMarker = "GOAL134_CANONICAL_RUNTIME_TRANSCRIPT_PLAYER_FAIL"
$CleanupScript = Join-Path $RepoRoot ".devflow/scripts/clean-unity-editor-noise.ps1"

function Test-Goal134PathUnderRoot {
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

function ConvertTo-Goal134RelativePath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $full = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith($root + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart([System.IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }

    return $full
}

function Resolve-Goal134InputPath {
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
    if (-not (Test-Goal134PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "$Name must stay under the repository root: $Path"
    }

    $relative = ConvertTo-Goal134RelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Name must not point under .llmgc/manual: $relative"
    }

    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) {
        throw "$Name does not exist: $relative"
    }

    return $full
}

function Resolve-Goal134OutputRoot {
    param([Parameter(Mandatory=$true)][string]$Path)

    $candidate = if ([System.IO.Path]::IsPathRooted($Path)) {
        $Path
    }
    else {
        Join-Path $RepoRoot $Path
    }

    $full = [System.IO.Path]::GetFullPath($candidate)
    if (-not (Test-Goal134PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "OutputRoot must stay under the repository root: $Path"
    }

    $relative = ConvertTo-Goal134RelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must not point under .llmgc/manual: $relative"
    }

    if (-not $relative.StartsWith($Goal134RootRelative + "/", [System.StringComparison]::OrdinalIgnoreCase) `
        -and -not $relative.Equals($Goal134RootRelative, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must stay under the Goal134 output root: $relative"
    }

    return $full
}

function Resolve-Goal134UnityPath {
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

function Write-Goal134Json {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)]$Value
    )

    $json = $Value | ConvertTo-Json -Depth 12
    $encoding = New-Object System.Text.UTF8Encoding -ArgumentList $false
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($Path)) | Out-Null
    [System.IO.File]::WriteAllText($Path, $json + [Environment]::NewLine, $encoding)
}

function Invoke-Goal134RuntimeProof {
    param([string]$UnitySmokePath)

    $env:LLMGC_GOAL134_SELECTED_CANDIDATE_HANDOFF_PATH = $ResolvedHandoffPath
    $env:LLMGC_GOAL134_SELECTED_CANDIDATE_PACKAGE_PATH = $ResolvedPackagePath
    $env:LLMGC_GOAL134_OUTPUT_ROOT = $ResolvedOutputRoot
    if ([string]::IsNullOrWhiteSpace($UnitySmokePath)) {
        Remove-Item Env:\LLMGC_GOAL134_UNITY_SMOKE_PATH -ErrorAction SilentlyContinue
    }
    else {
        $env:LLMGC_GOAL134_UNITY_SMOKE_PATH = $UnitySmokePath
    }

    Push-Location $RepoRoot
    try {
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter "FullyQualifiedName~CanonicalRuntimeSelectedCandidatePlaythroughScriptRuntimeProof"
        if ($LASTEXITCODE -ne 0) {
            throw "Goal134 canonical runtime proof test failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}

function Invoke-Goal134UnitySmoke {
    param(
        [Parameter(Mandatory=$true)][string]$ResolvedUnityPath,
        [Parameter(Mandatory=$true)][string]$TranscriptPath,
        [Parameter(Mandatory=$true)][string]$StateSummaryPath,
        [Parameter(Mandatory=$true)][string]$SmokePath
    )

    $logPath = Join-Path $ResolvedOutputRoot "unity-player-canonical-transcript-smoke.log"
    if (-not (Test-Path -LiteralPath $ResolvedUnityPath -PathType Leaf)) {
        $smoke = [ordered]@{
            goalId = "goal_134_canonical_runtime_selected_candidate_playthrough_matrix"
            unityAvailable = $false
            transcriptPathExists = Test-Path -LiteralPath $TranscriptPath -PathType Leaf
            stateSummaryPathExists = Test-Path -LiteralPath $StateSummaryPath -PathType Leaf
            passMarkerPresent = $false
            failMarkerPresent = $false
            unityPlayerConsumedCanonicalTranscript = $false
            passed = $false
            unityPath = $ResolvedUnityPath
            unityLogPath = ConvertTo-Goal134RelativePath -Path $logPath
            transcriptPath = ConvertTo-Goal134RelativePath -Path $TranscriptPath
            stateSummaryPath = ConvertTo-Goal134RelativePath -Path $StateSummaryPath
            status = "BLOCKED_UNITY_NOT_FOUND"
            diagnostics = @("Unity executable was not found through explicit path, PATH, or known fallback.")
        }
        Write-Goal134Json -Path $SmokePath -Value $smoke
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
        "-llmgcCanonicalRuntimeTranscriptPath",
        $TranscriptPath,
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
    $passed = $unityExitCode -eq 0 -and $passMarkerPresent -and -not $failMarkerPresent
    $smoke = [ordered]@{
        goalId = "goal_134_canonical_runtime_selected_candidate_playthrough_matrix"
        unityAvailable = $true
        transcriptPathExists = Test-Path -LiteralPath $TranscriptPath -PathType Leaf
        stateSummaryPathExists = Test-Path -LiteralPath $StateSummaryPath -PathType Leaf
        passMarkerPresent = $passMarkerPresent
        failMarkerPresent = $failMarkerPresent
        unityPlayerConsumedCanonicalTranscript = $passed
        passed = $passed
        unityPath = $ResolvedUnityPath
        unityLogPath = ConvertTo-Goal134RelativePath -Path $logPath
        transcriptPath = ConvertTo-Goal134RelativePath -Path $TranscriptPath
        stateSummaryPath = ConvertTo-Goal134RelativePath -Path $StateSummaryPath
        status = if ($passed) { "GREEN" } else { "FAILED_UNITY_TRANSCRIPT_SMOKE" }
        diagnostics = @("unityExitCode=$unityExitCode")
    }
    Write-Goal134Json -Path $SmokePath -Value $smoke
    return $passed
}

$ResolvedHandoffPath = Resolve-Goal134InputPath -Path $SelectedCandidateHandoffPath -Name "SelectedCandidateHandoffPath"
$ResolvedPackagePath = Resolve-Goal134InputPath -Path $SelectedCandidatePackagePath -Name "SelectedCandidatePackagePath"
$ResolvedOutputRoot = Resolve-Goal134OutputRoot -Path $OutputRoot
$ResolvedUnityPath = Resolve-Goal134UnityPath -ExplicitPath $UnityPath
$TranscriptPath = Join-Path $ResolvedOutputRoot "canonical-runtime-transcript.json"
$StateSummaryPath = Join-Path $ResolvedOutputRoot "canonical-runtime-state-summary.json"
$SmokePath = Join-Path $ResolvedOutputRoot "unity-player-canonical-transcript-smoke.json"

if ($DryRun) {
    Write-Host "Goal134 canonical runtime selected-candidate playthrough dry run"
    Write-Host "RepoRoot: $RepoRoot"
    Write-Host "SelectedCandidateHandoffPath: $ResolvedHandoffPath"
    Write-Host "SelectedCandidatePackagePath: $ResolvedPackagePath"
    Write-Host "OutputRoot: $ResolvedOutputRoot"
    Write-Host "UnityPath: $ResolvedUnityPath"
    Write-Host "RuntimeProof: dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter FullyQualifiedName~CanonicalRuntimeSelectedCandidatePlaythroughScriptRuntimeProof"
    Write-Host "UnityExecuteMethod: $ExecuteMethod"
    Write-Host "PassMarker: $PassMarker"
    Write-Host "ExportRoot: $ExportRootRelative"
    return
}

Invoke-Goal134RuntimeProof -UnitySmokePath ""
$unityPassed = Invoke-Goal134UnitySmoke `
    -ResolvedUnityPath $ResolvedUnityPath `
    -TranscriptPath $TranscriptPath `
    -StateSummaryPath $StateSummaryPath `
    -SmokePath $SmokePath
Invoke-Goal134RuntimeProof -UnitySmokePath $SmokePath

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
    throw "Goal134 Unity/player transcript smoke did not pass. See $(ConvertTo-Goal134RelativePath -Path $SmokePath)."
}

Write-Host "Goal134 canonical runtime selected-candidate playthrough passed."
