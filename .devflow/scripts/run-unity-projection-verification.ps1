param(
    [ValidateSet("GenericFullPlaythrough")]
    [string]$Mode = "GenericFullPlaythrough",
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
$ProjectPath = Join-Path $RepoRoot "unity/LLMGameCreatorAlpha"
$EvidenceRoot = Join-Path $RepoRoot ".llmgc/procedural/goal-127-winforms-unity-projection-verification-runner"
$LogPath = Join-Path $EvidenceRoot "unity-batchmode-generic-full-playthrough-runner.log"
$ResultPath = Join-Path $EvidenceRoot "unity-projection-verification-runner-result.json"
$CleanupScript = Join-Path $RepoRoot ".devflow/scripts/clean-unity-editor-noise.ps1"
$ExecuteMethod = "LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeGenericGamePackageFullPlaythroughSmoke"
$PassMarker = "GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS"
$FailMarker = "GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL"
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

[System.IO.Directory]::CreateDirectory($EvidenceRoot) | Out-Null
$ResolvedUnityPath = Resolve-RunnerUnityPath -ExplicitPath $UnityPath
$UnityArgs = @(
    "-batchmode",
    "-quit",
    "-projectPath",
    $ProjectPath,
    "-executeMethod",
    $ExecuteMethod,
    "-logFile",
    $LogPath
)
$UnityCommandText = Format-RunnerCommand -Exe $ResolvedUnityPath -ArgsList $UnityArgs
$CleanupArgs = @("-NoProfile", "-ExecutionPolicy", "Bypass", "-File", $CleanupScript, "-Apply")
$CleanupCommandText = Format-RunnerCommand -Exe "powershell" -ArgsList $CleanupArgs

Write-Host "Unity projection verification mode: $Mode"
Write-Host "Unity command: $UnityCommandText"
Write-Host "Cleanup command: $CleanupCommandText"

if ($DryRun) {
    Write-Host "DryRun: Unity was not executed and cleanup was not applied."
    exit 0
}

if (-not (Test-Path -LiteralPath $ResolvedUnityPath)) {
    Write-RunnerResult `
        -ResolvedUnityPath $ResolvedUnityPath `
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
Write-Host "Result path: $(ConvertTo-RunnerRelativePath -Path $ResultPath)"
Write-Host "Log path: $(ConvertTo-RunnerRelativePath -Path $LogPath)"

if ($passed) {
    exit 0
}

exit 1
