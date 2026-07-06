param(
    [ValidateSet("GenericFullPlaythrough")]
    [string]$Mode = "GenericFullPlaythrough",
    [string]$UnityPath = "",
    [string]$PackagePath = "samples/minimal-map-game/package.json",
    [string]$EvidenceRoot = "",
    [string]$ResultPath = "",
    [string]$LogPath = "",
    [switch]$DryRun,
    [switch]$ApplyCleanup
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment

$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$ProjectPath = Join-Path $RepoRoot "unity/LLMGameCreatorAlpha"
$CleanupScript = Join-Path $RepoRoot ".devflow/scripts/clean-unity-editor-noise.ps1"
$ExecuteMethod = "LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeParameterizedGamePackageFullPlaythroughSmoke"
$PassMarker = "GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS"
$FailMarker = "GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL"
$MaterialWarningMarker = "Instantiating material due to calling renderer.material during edit mode"
$RendererMaterialMarker = "UnityEngine.Renderer:get_material()"
$FallbackUnityPath = "C:\Program Files\Unity\Hub\Editor\6000.1.10f1\Editor\Unity.exe"

function Resolve-RunnerUnityPath {
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

function ConvertTo-RunnerRelativePath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $full = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith($root + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart([System.IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }

    return $full
}

function Resolve-RunnerOutputRoot {
    param([string]$ExplicitPath)

    $candidate = $ExplicitPath
    if ([string]::IsNullOrWhiteSpace($candidate)) {
        $candidate = ".llmgc/procedural/goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface"
    }

    if ([System.IO.Path]::IsPathRooted($candidate)) {
        $full = [System.IO.Path]::GetFullPath($candidate)
    }
    else {
        $full = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $candidate))
    }

    if (-not (Test-RunnerPathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "EvidenceRoot must stay under the repository root: $candidate"
    }

    $relative = ConvertTo-RunnerRelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "EvidenceRoot must not point under .llmgc/manual: $relative"
    }

    return $full
}

function Resolve-RunnerOutputFile {
    param(
        [Parameter(Mandatory=$true)][string]$ExplicitPath,
        [Parameter(Mandatory=$true)][string]$DefaultFileName
    )

    $candidate = $ExplicitPath
    if ([string]::IsNullOrWhiteSpace($candidate)) {
        $candidate = Join-Path $EvidenceRoot $DefaultFileName
    }
    elseif (-not [System.IO.Path]::IsPathRooted($candidate)) {
        $candidate = Join-Path $RepoRoot $candidate
    }

    $full = [System.IO.Path]::GetFullPath($candidate)
    if (-not (Test-RunnerPathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "Output path must stay under the repository root: $ExplicitPath"
    }

    $relative = ConvertTo-RunnerRelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Output path must not point under .llmgc/manual: $relative"
    }

    return $full
}

function Test-RunnerPathUnderRoot {
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

function Resolve-RunnerPackagePath {
    param([string]$ExplicitPath)

    $candidate = $ExplicitPath
    if ([string]::IsNullOrWhiteSpace($candidate)) {
        $candidate = "samples/minimal-map-game/package.json"
    }

    if ([System.IO.Path]::IsPathRooted($candidate)) {
        $full = [System.IO.Path]::GetFullPath($candidate)
    }
    else {
        $full = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $candidate))
    }

    if (-not (Test-RunnerPathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "PackagePath must stay under the repository root: $candidate"
    }

    $relative = ConvertTo-RunnerRelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "PackagePath must not point under .llmgc/manual: $relative"
    }

    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) {
        throw "PackagePath does not exist: $relative"
    }

    return $full
}

function Format-RunnerCommand {
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

function Read-RunnerLogText {
    param([Parameter(Mandatory=$true)][int]$UnityExitCode)

    $text = ""
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        if (Test-Path -LiteralPath $LogPath) {
            try {
                $text = Get-Content -LiteralPath $LogPath -Raw -Encoding UTF8
            }
            catch {
                $text = ""
            }
        }

        if ($text.Contains($PassMarker) `
            -or $text.Contains($FailMarker) `
            -or $text.Contains($MaterialWarningMarker) `
            -or $text.Contains($RendererMaterialMarker)) {
            return $text
        }

        if ($UnityExitCode -ne 0 -and $text.Length -gt 0) {
            return $text
        }

        Start-Sleep -Milliseconds 500
    }

    return $text
}

function Normalize-RunnerLogFile {
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        if (-not (Test-Path -LiteralPath $LogPath)) {
            Start-Sleep -Milliseconds 500
            continue
        }

        try {
            $text = Get-Content -LiteralPath $LogPath -Raw -Encoding UTF8
            $normalized = [regex]::Replace($text, "[ `t]+(`r?`n)", '$1')
            $normalized = [regex]::Replace($normalized, "[ `t]+$", "")
            if ($normalized -ne $text) {
                $encoding = New-Object System.Text.UTF8Encoding -ArgumentList $false
                [System.IO.File]::WriteAllText($LogPath, $normalized, $encoding)
            }

            return
        }
        catch [System.IO.IOException] {
            Start-Sleep -Milliseconds 500
        }
    }

    throw "Unity log normalization failed because the log file stayed locked: $LogPath"
}

function Write-RunnerResult {
    param(
        [Parameter(Mandatory=$true)][string]$ResolvedUnityPath,
        [Parameter(Mandatory=$true)][string]$ResolvedPackagePath,
        [Parameter(Mandatory=$true)][int]$UnityExitCode,
        [Parameter(Mandatory=$true)][bool]$PassMarkerPresent,
        [Parameter(Mandatory=$true)][bool]$FailMarkerAbsent,
        [Parameter(Mandatory=$true)][bool]$MaterialWarningAbsent,
        [Parameter(Mandatory=$true)][bool]$CleanupApplied,
        [Parameter(Mandatory=$true)][int]$CleanupExitCode,
        [Parameter(Mandatory=$true)][bool]$Passed
    )

    $result = [ordered]@{
        mode = $Mode
        packagePath = $ResolvedPackagePath
        packagePathRelative = ConvertTo-RunnerRelativePath -Path $ResolvedPackagePath
        unityPath = $ResolvedUnityPath
        unityExitCode = $UnityExitCode
        passMarkerPresent = $PassMarkerPresent
        failMarkerAbsent = $FailMarkerAbsent
        materialWarningAbsent = $MaterialWarningAbsent
        cleanupApplied = $CleanupApplied
        cleanupExitCode = $CleanupExitCode
        passed = $Passed
        logPath = ConvertTo-RunnerRelativePath -Path $LogPath
    }

    [System.IO.Directory]::CreateDirectory($EvidenceRoot) | Out-Null
    $result | ConvertTo-Json -Depth 6 | Set-Content -Encoding UTF8 -Path $ResultPath
}

if ($Mode -ne "GenericFullPlaythrough") {
    throw "Unsupported Unity projection verification mode: $Mode"
}

$EvidenceRoot = Resolve-RunnerOutputRoot -ExplicitPath $EvidenceRoot
$ResultPath = Resolve-RunnerOutputFile -ExplicitPath $ResultPath -DefaultFileName "parameterized-gamepackage-runner-result.json"
$LogPath = Resolve-RunnerOutputFile -ExplicitPath $LogPath -DefaultFileName "unity-batchmode-parameterized-gamepackage-full-playthrough.log"

[System.IO.Directory]::CreateDirectory($EvidenceRoot) | Out-Null
[System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($ResultPath)) | Out-Null
[System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($LogPath)) | Out-Null
$ResolvedUnityPath = Resolve-RunnerUnityPath -ExplicitPath $UnityPath
$ResolvedPackagePath = Resolve-RunnerPackagePath -ExplicitPath $PackagePath
$UnityArgs = @(
    "-batchmode",
    "-quit",
    "-projectPath",
    $ProjectPath,
    "-executeMethod",
    $ExecuteMethod,
    "-logFile",
    $LogPath,
    "-llmgcPackagePath",
    $ResolvedPackagePath
)
$UnityCommandText = Format-RunnerCommand -Exe $ResolvedUnityPath -ArgsList $UnityArgs
$CleanupArgs = @("-NoProfile", "-ExecutionPolicy", "Bypass", "-File", $CleanupScript, "-Apply")
$CleanupCommandText = Format-RunnerCommand -Exe "powershell" -ArgsList $CleanupArgs

Write-Host "Unity projection verification mode: $Mode"
Write-Host "GamePackage path: $(ConvertTo-RunnerRelativePath -Path $ResolvedPackagePath)"
Write-Host "Unity command: $UnityCommandText"
Write-Host "Cleanup command: $CleanupCommandText"

if ($DryRun) {
    Write-Host "DryRun: Unity was not executed and cleanup was not applied."
    exit 0
}

if (-not (Test-Path -LiteralPath $ResolvedUnityPath)) {
    Write-RunnerResult `
        -ResolvedUnityPath $ResolvedUnityPath `
        -ResolvedPackagePath $ResolvedPackagePath `
        -UnityExitCode -1 `
        -PassMarkerPresent $false `
        -FailMarkerAbsent $false `
        -MaterialWarningAbsent $false `
        -CleanupApplied $false `
        -CleanupExitCode -1 `
        -Passed $false
    Write-Host "Unity executable was not found: $ResolvedUnityPath"
    exit 1
}

if (Test-Path -LiteralPath $LogPath) {
    Remove-Item -LiteralPath $LogPath -Force
}

Push-Location $RepoRoot
try {
    & $ResolvedUnityPath @UnityArgs
    $unityExit = $LASTEXITCODE
}
finally {
    Pop-Location
}

$logText = Read-RunnerLogText -UnityExitCode $unityExit
Normalize-RunnerLogFile
$logText = Read-RunnerLogText -UnityExitCode $unityExit

$passMarkerPresent = $logText.Contains($PassMarker)
$failMarkerAbsent = -not $logText.Contains($FailMarker)
$materialWarningAbsent =
    -not $logText.Contains($MaterialWarningMarker) `
    -and -not $logText.Contains($RendererMaterialMarker)

$cleanupExit = -1
$cleanupApplied = $false
if ($ApplyCleanup) {
    & powershell @CleanupArgs
    $cleanupExit = $LASTEXITCODE
    $cleanupApplied = $true
}

$passed = $unityExit -eq 0 `
    -and $passMarkerPresent `
    -and $failMarkerAbsent `
    -and $materialWarningAbsent `
    -and ((-not $ApplyCleanup) -or $cleanupExit -eq 0)

Write-RunnerResult `
    -ResolvedUnityPath $ResolvedUnityPath `
    -ResolvedPackagePath $ResolvedPackagePath `
    -UnityExitCode $unityExit `
    -PassMarkerPresent $passMarkerPresent `
    -FailMarkerAbsent $failMarkerAbsent `
    -MaterialWarningAbsent $materialWarningAbsent `
    -CleanupApplied $cleanupApplied `
    -CleanupExitCode $cleanupExit `
    -Passed $passed

Write-Host "Unity exit code: $unityExit"
Write-Host "Pass marker present: $passMarkerPresent"
Write-Host "Fail marker absent: $failMarkerAbsent"
Write-Host "Material warning absent: $materialWarningAbsent"
Write-Host "Cleanup applied: $cleanupApplied"
Write-Host "Cleanup exit code: $cleanupExit"
Write-Host "Package path: $(ConvertTo-RunnerRelativePath -Path $ResolvedPackagePath)"
Write-Host "Result path: $(ConvertTo-RunnerRelativePath -Path $ResultPath)"
Write-Host "Log path: $(ConvertTo-RunnerRelativePath -Path $LogPath)"

if ($passed) {
    exit 0
}

exit 1
