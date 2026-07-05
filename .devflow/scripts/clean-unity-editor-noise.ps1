param(
    [switch]$DryRun,
    [switch]$Apply,
    [switch]$AllowStaged
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment

if ($DryRun -and $Apply) {
    throw "Use either -DryRun or -Apply, not both."
}

if (-not $DryRun -and -not $Apply) {
    $DryRun = $true
}

$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$NeverRemoveExtensions = @(".cs", ".json", ".md", ".unity", ".prefab")
$ProjectVersionPath = "unity/LLMGameCreatorAlpha/ProjectSettings/ProjectVersion.txt"

function Invoke-CleanupGitStatus {
    Push-Location $RepoRoot
    try {
        $output = & git status --porcelain=v1 --untracked-files=all 2>&1
        $exit = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }

    if ($exit -ne 0) {
        throw "git status --porcelain=v1 --untracked-files=all failed: $($output -join [Environment]::NewLine)"
    }

    return @($output | ForEach-Object { "$_" } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function ConvertTo-CleanupPath {
    param([Parameter(Mandatory=$true)][string]$StatusLine)

    if ($StatusLine.Length -lt 4) {
        return ""
    }

    $path = $StatusLine.Substring(3).Trim()
    if ($path.Contains(" -> ")) {
        $path = ($path -split " -> ")[-1].Trim()
    }

    return $path.Trim('"').Replace('\', '/')
}

function Test-StagedStatusLine {
    param([Parameter(Mandatory=$true)][string]$StatusLine)

    if ($StatusLine.Length -lt 2) {
        return $false
    }

    $indexStatus = $StatusLine.Substring(0, 1)
    return $indexStatus -ne " " -and $indexStatus -ne "?"
}

function Test-NeverRemovePath {
    param([Parameter(Mandatory=$true)][string]$Path)

    foreach ($extension in $NeverRemoveExtensions) {
        if ($Path.EndsWith($extension, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

function Test-UntrackedRemovalTarget {
    param([Parameter(Mandatory=$true)][string]$Path)

    if ($Path -eq "unity/LLMGameCreatorAlpha/Packages/packages-lock.json") {
        return $true
    }

    if (Test-NeverRemovePath -Path $Path) {
        return $false
    }

    if (($Path.StartsWith("unity/LLMGameCreatorAlpha/Assets/", [System.StringComparison]::Ordinal)) -and ($Path.EndsWith(".meta", [System.StringComparison]::OrdinalIgnoreCase))) {
        return $true
    }

    if (($Path.StartsWith("unity/LLMGameCreatorAlpha/ProjectSettings/", [System.StringComparison]::Ordinal)) -and ($Path.EndsWith(".asset", [System.StringComparison]::OrdinalIgnoreCase))) {
        return $true
    }

    return $false
}

function Test-TrackedRestoreTarget {
    param([Parameter(Mandatory=$true)][string]$Path)

    return $Path -eq $ProjectVersionPath
}

function Resolve-CleanupPath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $full = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $Path.Replace('/', [System.IO.Path]::DirectorySeparatorChar)))
    $root = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    if (-not $full.StartsWith($root + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Cleanup path escapes repository root: $Path"
    }

    return $full
}

function Get-CleanupTargets {
    param([Parameter(Mandatory=$true)][string[]]$StatusLines)

    $remove = New-Object System.Collections.Generic.List[string]
    $restore = New-Object System.Collections.Generic.List[string]
    $staged = New-Object System.Collections.Generic.List[string]

    foreach ($line in $StatusLines) {
        $path = ConvertTo-CleanupPath -StatusLine $line
        if ([string]::IsNullOrWhiteSpace($path)) {
            continue
        }

        if ((Test-StagedStatusLine -StatusLine $line) -and -not $AllowStaged) {
            $staged.Add($path) | Out-Null
            continue
        }

        $isUntracked = $line.StartsWith("?? ", [System.StringComparison]::Ordinal)
        if ($isUntracked -and (Test-UntrackedRemovalTarget -Path $path)) {
            $remove.Add($path) | Out-Null
            continue
        }

        $worktreeStatus = $line.Substring(1, 1)
        if ($worktreeStatus -eq "M" -and (Test-TrackedRestoreTarget -Path $path)) {
            $restore.Add($path) | Out-Null
        }
    }

    return [pscustomobject]@{
        remove = @($remove | Sort-Object -Unique)
        restore = @($restore | Sort-Object -Unique)
        staged = @($staged | Sort-Object -Unique)
    }
}

$statusLines = Invoke-CleanupGitStatus
$targets = Get-CleanupTargets -StatusLines $statusLines

Write-Host "Unity editor noise cleanup mode: $(if ($Apply) { 'apply' } else { 'dry-run' })"
Write-Host "Scanned with: git status --porcelain=v1 --untracked-files=all"

if ($targets.staged.Count -gt 0) {
    Write-Host "Refusing cleanup because staged files are present. Re-run with -AllowStaged to override."
    $targets.staged | ForEach-Object { Write-Host "staged: $_" }
    exit 1
}

if ($targets.remove.Count -eq 0 -and $targets.restore.Count -eq 0) {
    Write-Host "No Unity editor noise cleanup targets found."
}

foreach ($path in $targets.remove) {
    Write-Host "$(if ($Apply) { 'remove' } else { 'would remove' }): $path"
    if ($Apply) {
        $full = Resolve-CleanupPath -Path $path
        if (Test-Path -LiteralPath $full) {
            Remove-Item -LiteralPath $full -Force
        }
    }
}

foreach ($path in $targets.restore) {
    Write-Host "$(if ($Apply) { 'restore' } else { 'would restore' }): $path"
    if ($Apply) {
        Push-Location $RepoRoot
        try {
            & git restore -- $ProjectVersionPath
            $exit = $LASTEXITCODE
        }
        finally {
            Pop-Location
        }

        if ($exit -ne 0) {
            throw "git restore -- $ProjectVersionPath failed."
        }
    }
}

if ($Apply) {
    $afterStatusLines = Invoke-CleanupGitStatus
    $remaining = Get-CleanupTargets -StatusLines $afterStatusLines
    if ($remaining.remove.Count -gt 0 -or $remaining.restore.Count -gt 0) {
        Write-Host "Unity editor noise cleanup targets remain after apply."
        $remaining.remove | ForEach-Object { Write-Host "remaining remove: $_" }
        $remaining.restore | ForEach-Object { Write-Host "remaining restore: $_" }
        exit 1
    }
}

Write-Host "Final status:"
Invoke-CleanupGitStatus | ForEach-Object { Write-Host $_ }
