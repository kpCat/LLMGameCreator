param(
    [string]$CandidateIndexPath = ".llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/gamepackage-candidate-index.json",
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
$GoalRootRelative = ".llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner"
$GoalRoot = Join-Path $RepoRoot $GoalRootRelative
$MatrixRoot = Join-Path $GoalRoot "matrix"
$MatrixResultPath = Join-Path $GoalRoot "gamepackage-projection-matrix-result.json"
$RunnerScript = Join-Path $RepoRoot ".devflow/scripts/run-unity-projection-verification.ps1"
$PassMarker = "GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS"
$FailMarker = "GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL"
$MaterialWarningMarker = "Instantiating material due to calling renderer.material during edit mode"
$RendererMaterialMarker = "UnityEngine.Renderer:get_material()"

function ConvertTo-MatrixRelativePath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $normalized = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    if ($normalized.StartsWith($root + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $normalized.Substring($root.Length).TrimStart([System.IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }

    return $normalized
}

function Test-MatrixPathUnderRoot {
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

function Resolve-MatrixInputPath {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$Label,
        [bool]$MustExist = $true
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw "$Label is required."
    }

    if ([System.IO.Path]::IsPathRooted($Path)) {
        $full = [System.IO.Path]::GetFullPath($Path)
    }
    else {
        $full = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $Path))
    }

    if (-not (Test-MatrixPathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "$Label must stay under the repository root: $Path"
    }

    $relative = ConvertTo-MatrixRelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Label must not point under .llmgc/manual: $relative"
    }

    if ($MustExist -and -not (Test-Path -LiteralPath $full -PathType Leaf)) {
        throw "$Label does not exist: $relative"
    }

    return $full
}

function Format-MatrixCommand {
    param(
        [Parameter(Mandatory=$true)][string]$Exe,
        [Parameter(Mandatory=$true)][string[]]$ArgsList
    )

    $parts = @($Exe) + $ArgsList
    return ($parts | ForEach-Object {
        $value = "$_"
        if ($value.Contains(" ") -or $value.Contains(";")) {
            '"' + $value.Replace('"', '\"') + '"'
        }
        else {
            $value
        }
    }) -join " "
}

function Get-MatrixProperty {
    param(
        [Parameter(Mandatory=$true)]$Object,
        [Parameter(Mandatory=$true)][string]$Name
    )

    if ($Object.PSObject.Properties.Name -contains $Name) {
        return "" + $Object.$Name
    }

    return ""
}

function Read-MatrixBool {
    param(
        [Parameter(Mandatory=$true)]$Object,
        [Parameter(Mandatory=$true)][string]$Name
    )

    if (-not ($Object.PSObject.Properties.Name -contains $Name)) {
        return $false
    }

    return [bool]$Object.$Name
}

function Read-MatrixInt {
    param(
        [Parameter(Mandatory=$true)]$Object,
        [Parameter(Mandatory=$true)][string]$Name
    )

    if (-not ($Object.PSObject.Properties.Name -contains $Name)) {
        return -1
    }

    return [int]$Object.$Name
}

function Read-CandidateIndex {
    param([Parameter(Mandatory=$true)][string]$Path)

    $doc = Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
    if ($doc.PSObject.Properties.Name -contains "candidates") {
        return @($doc.candidates)
    }

    return @($doc)
}

function Write-MatrixJson {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)]$Value
    )

    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($Path)) | Out-Null
    $Value | ConvertTo-Json -Depth 12 | Set-Content -Encoding UTF8 -Path $Path
}

function New-LogScan {
    param(
        [Parameter(Mandatory=$true)][string]$CandidateId,
        [Parameter(Mandatory=$true)][string]$LogPath,
        [Parameter(Mandatory=$true)][string]$LogScanPath
    )

    $text = ""
    $exists = Test-Path -LiteralPath $LogPath -PathType Leaf
    if ($exists) {
        $text = Get-Content -LiteralPath $LogPath -Raw -Encoding UTF8
    }

    $forbidden = New-Object System.Collections.Generic.List[string]
    if ($text.Contains($FailMarker)) {
        $forbidden.Add($FailMarker) | Out-Null
    }
    if ($text.Contains($MaterialWarningMarker)) {
        $forbidden.Add($MaterialWarningMarker) | Out-Null
    }
    if ($text.Contains($RendererMaterialMarker)) {
        $forbidden.Add($RendererMaterialMarker) | Out-Null
    }

    $passMarkerPresent = $text.Contains($PassMarker)
    $scan = [ordered]@{
        schemaVersion = "gamepackage_candidate_matrix_log_scan_v1"
        goalId = "goal_129_gamepackage_candidate_matrix_projection_runner"
        candidateId = $CandidateId
        logPath = ConvertTo-MatrixRelativePath -Path $LogPath
        logExists = [bool]$exists
        passMarkerPresent = [bool]$passMarkerPresent
        failMarkerAbsent = [bool](-not $forbidden.Contains($FailMarker))
        materialWarningAbsent = [bool](-not $forbidden.Contains($MaterialWarningMarker) -and -not $forbidden.Contains($RendererMaterialMarker))
        forbiddenMarkersFound = @($forbidden.ToArray())
        passed = [bool]($exists -and $passMarkerPresent -and $forbidden.Count -eq 0)
    }

    Write-MatrixJson -Path $LogScanPath -Value $scan
    return $scan
}

$ResolvedCandidateIndexPath = Resolve-MatrixInputPath -Path $CandidateIndexPath -Label "CandidateIndexPath"
if (-not (Test-Path -LiteralPath $RunnerScript -PathType Leaf)) {
    throw "Unity projection runner script was not found: $(ConvertTo-MatrixRelativePath -Path $RunnerScript)"
}

$candidates = @(Read-CandidateIndex -Path $ResolvedCandidateIndexPath)
if ($candidates.Count -lt 1) {
    throw "Candidate index does not contain candidates: $(ConvertTo-MatrixRelativePath -Path $ResolvedCandidateIndexPath)"
}

[System.IO.Directory]::CreateDirectory($MatrixRoot) | Out-Null
$entries = New-Object System.Collections.Generic.List[object]

foreach ($candidate in $candidates) {
    $candidateId = Get-MatrixProperty -Object $candidate -Name "candidateId"
    if ([string]::IsNullOrWhiteSpace($candidateId)) {
        throw "Candidate entry is missing candidateId."
    }
    if ($candidateId.Contains("/") -or $candidateId.Contains("\") -or $candidateId.Contains("..")) {
        throw "CandidateId must be a simple directory name: $candidateId"
    }

    $packagePath = Get-MatrixProperty -Object $candidate -Name "packagePathRelative"
    if ([string]::IsNullOrWhiteSpace($packagePath)) {
        $packagePath = Get-MatrixProperty -Object $candidate -Name "packagePath"
    }

    $resolvedPackagePath = Resolve-MatrixInputPath -Path $packagePath -Label "Candidate package path"
    $candidateRoot = Join-Path $MatrixRoot $candidateId
    $resultPath = Join-Path $candidateRoot "runner-result.json"
    $logPath = Join-Path $candidateRoot "unity.log"
    $logScanPath = Join-Path $candidateRoot "log-scan.json"
    [System.IO.Directory]::CreateDirectory($candidateRoot) | Out-Null

    $runnerArgs = @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $RunnerScript,
        "-Mode",
        "GenericFullPlaythrough",
        "-PackagePath",
        $resolvedPackagePath,
        "-EvidenceRoot",
        $candidateRoot,
        "-ResultPath",
        $resultPath,
        "-LogPath",
        $logPath,
        "-ApplyCleanup"
    )
    if (-not [string]::IsNullOrWhiteSpace($UnityPath)) {
        $runnerArgs += @("-UnityPath", $UnityPath)
    }

    $runnerCommandText = Format-MatrixCommand -Exe "powershell" -ArgsList $runnerArgs
    Write-Host "Candidate $candidateId command: $runnerCommandText"

    if ($DryRun) {
        $entries.Add([ordered]@{
            candidateId = $candidateId
            packagePathRelative = ConvertTo-MatrixRelativePath -Path $resolvedPackagePath
            runnerCommand = $runnerCommandText
            resultPath = ConvertTo-MatrixRelativePath -Path $resultPath
            logPath = ConvertTo-MatrixRelativePath -Path $logPath
            logScanPath = ConvertTo-MatrixRelativePath -Path $logScanPath
            cleanupApplied = $false
            cleanupExitCode = -1
            dryRun = $true
            passed = $false
        }) | Out-Null
        continue
    }

    & powershell @runnerArgs
    $runnerExitCode = $LASTEXITCODE

    $runnerResult = $null
    if (Test-Path -LiteralPath $resultPath -PathType Leaf) {
        $runnerResult = Get-Content -LiteralPath $resultPath -Raw -Encoding UTF8 | ConvertFrom-Json
    }

    $logScan = New-LogScan -CandidateId $candidateId -LogPath $logPath -LogScanPath $logScanPath
    $unityExitCode = if ($null -eq $runnerResult) { -1 } else { Read-MatrixInt -Object $runnerResult -Name "unityExitCode" }
    $cleanupExitCode = if ($null -eq $runnerResult) { -1 } else { Read-MatrixInt -Object $runnerResult -Name "cleanupExitCode" }
    $passMarkerPresent = if ($null -eq $runnerResult) { $false } else { Read-MatrixBool -Object $runnerResult -Name "passMarkerPresent" }
    $failMarkerAbsent = if ($null -eq $runnerResult) { $false } else { Read-MatrixBool -Object $runnerResult -Name "failMarkerAbsent" }
    $materialWarningAbsent = if ($null -eq $runnerResult) { $false } else { Read-MatrixBool -Object $runnerResult -Name "materialWarningAbsent" }
    $cleanupApplied = if ($null -eq $runnerResult) { $false } else { Read-MatrixBool -Object $runnerResult -Name "cleanupApplied" }
    $runnerPassed = if ($null -eq $runnerResult) { $false } else { Read-MatrixBool -Object $runnerResult -Name "passed" }
    $entryPassed =
        $runnerExitCode -eq 0 `
        -and $runnerPassed `
        -and $unityExitCode -eq 0 `
        -and $passMarkerPresent `
        -and $failMarkerAbsent `
        -and $materialWarningAbsent `
        -and $cleanupApplied `
        -and $cleanupExitCode -eq 0 `
        -and [bool]$logScan.passed

    $entries.Add([ordered]@{
        candidateId = $candidateId
        packagePathRelative = ConvertTo-MatrixRelativePath -Path $resolvedPackagePath
        runnerCommand = $runnerCommandText
        runnerExitCode = $runnerExitCode
        unityExitCode = $unityExitCode
        passMarkerPresent = [bool]$passMarkerPresent
        failMarkerAbsent = [bool]$failMarkerAbsent
        materialWarningAbsent = [bool]$materialWarningAbsent
        cleanupApplied = [bool]$cleanupApplied
        cleanupExitCode = $cleanupExitCode
        resultPath = ConvertTo-MatrixRelativePath -Path $resultPath
        logPath = ConvertTo-MatrixRelativePath -Path $logPath
        logScanPath = ConvertTo-MatrixRelativePath -Path $logScanPath
        logScanPassed = [bool]$logScan.passed
        dryRun = $false
        passed = [bool]$entryPassed
    }) | Out-Null
}

$entryArray = @($entries.ToArray())
$candidateCount = $entryArray.Count
$passedCandidateCount = @($entryArray | Where-Object { $_.passed }).Count
$failedCandidateCount = $candidateCount - $passedCandidateCount
$allPassed = -not $DryRun -and $candidateCount -gt 0 -and $failedCandidateCount -eq 0
$aggregate = [ordered]@{
    schemaVersion = "gamepackage_candidate_projection_matrix_result_v1"
    goalId = "goal_129_gamepackage_candidate_matrix_projection_runner"
    matrixStatus = if ($allPassed) { "GREEN" } elseif ($DryRun) { "DRY_RUN" } else { "BLOCKED" }
    candidateIndexPath = ConvertTo-MatrixRelativePath -Path $ResolvedCandidateIndexPath
    matrixResultPath = ConvertTo-MatrixRelativePath -Path $MatrixResultPath
    candidateCount = $candidateCount
    passedCandidateCount = $passedCandidateCount
    failedCandidateCount = $failedCandidateCount
    passed = [bool]$allPassed
    manualUnityOptional = $true
    cleanupApplied = [bool]($candidateCount -gt 0 -and @($entryArray | Where-Object { -not $_.cleanupApplied }).Count -eq 0)
    projectionOnly = $true
    entries = $entryArray
}

if (-not $DryRun) {
    Write-MatrixJson -Path $MatrixResultPath -Value $aggregate
    Write-Host "Matrix status: $($aggregate.matrixStatus)"
    Write-Host "Candidate count: $candidateCount"
    Write-Host "Passed candidates: $passedCandidateCount"
    Write-Host "Failed candidates: $failedCandidateCount"
    Write-Host "Result path: $(ConvertTo-MatrixRelativePath -Path $MatrixResultPath)"
}
else {
    Write-Host "DryRun: Unity was not executed and cleanup was not applied."
}

if ($allPassed -or $DryRun) {
    exit 0
}

exit 1
