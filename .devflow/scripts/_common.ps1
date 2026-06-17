Set-StrictMode -Version 2.0

function Initialize-DevflowScriptEnvironment {
    $utf8NoBom = New-Object System.Text.UTF8Encoding -ArgumentList $false

    try { [Console]::InputEncoding = $utf8NoBom } catch { }
    try { [Console]::OutputEncoding = $utf8NoBom } catch { }
    $global:OutputEncoding = $utf8NoBom

    $env:DOTNET_CLI_UI_LANGUAGE = "en"
    $env:VSLANG = "1033"
    $env:DOTNET_NOLOGO = "1"
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
    $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
    $env:NUGET_XMLDOC_MODE = "skip"
    $env:MSBUILDDISABLENODEREUSE = "1"

    if ($env:OS -like "Windows*") {
        try { & chcp.com 65001 | Out-Null } catch { }
    }
}

function Resolve-DevflowRepoRoot {
    param([string]$ScriptPath)

    $scriptDir = Split-Path -Parent $ScriptPath
    return (Resolve-Path (Join-Path $scriptDir "..\..")).Path
}

function Write-DevflowUtf8File {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$Content
    )

    $utf8NoBom = New-Object System.Text.UTF8Encoding -ArgumentList $false
    [System.IO.File]::WriteAllText($Path, $Content, $utf8NoBom)
}

function Write-DevflowStep {
    param(
        [Parameter(Mandatory=$true)][string]$Message,
        [string]$LogIndexPath = ""
    )

    Write-Host ""
    Write-Host "=== $Message ==="
    if (-not [string]::IsNullOrWhiteSpace($LogIndexPath)) {
        Add-Content -Encoding UTF8 -Path $LogIndexPath -Value "=== $Message ==="
    }
}

function Invoke-DevflowLoggedCommand {
    param(
        [Parameter(Mandatory=$true)][string]$Name,
        [Parameter(Mandatory=$true)][string]$Exe,
        [Parameter(Mandatory=$true)][string[]]$ArgsList,
        [Parameter(Mandatory=$true)][string]$RunDir,
        [string]$LogIndexPath = ""
    )

    $logPath = Join-Path $RunDir "$Name.log"
    Write-DevflowStep -Message $Name -LogIndexPath $LogIndexPath
    if (-not [string]::IsNullOrWhiteSpace($LogIndexPath)) {
        Add-Content -Encoding UTF8 -Path $LogIndexPath -Value $logPath
    }

    Write-Host "$Exe $($ArgsList -join ' ')"

    # Do not use Tee-Object here. In Windows PowerShell it writes text files in
    # UTF-16 by default, and its pipeline output becomes part of the function
    # return value. check-all expects this function to return only the log path.
    if (Test-Path $logPath) {
        Remove-Item -LiteralPath $logPath -Force
    }

    & $Exe @ArgsList 2>&1 | ForEach-Object {
        $line = "$_"
        Write-Host $line
        Add-Content -Encoding UTF8 -Path $logPath -Value $line
    }
    $exit = $LASTEXITCODE

    if ($exit -ne 0) {
        throw "Command '$Name' failed with exit code $exit. Log: $logPath"
    }

    return [string]$logPath
}

function ConvertTo-DevflowRelativePath {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$RepoRoot
    )

    $normalizedPath = $Path.Replace('\', '/')
	$normalizedRoot = $RepoRoot.Replace('\', '/').TrimEnd('/')

    if ($normalizedPath.StartsWith($normalizedRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $normalizedPath.Substring($normalizedRoot.Length).TrimStart('/')
    }

    return $normalizedPath.TrimStart('./')
}

function Get-DevflowBuildWarnings {
    param(
        [Parameter(Mandatory=$true)][string]$BuildLogPath,
        [Parameter(Mandatory=$true)][string]$RepoRoot
    )

    $warnings = New-Object System.Collections.Generic.List[object]
    if (-not (Test-Path $BuildLogPath)) {
        return $warnings
    }

    $lines = Get-Content -Encoding UTF8 -Path $BuildLogPath
    foreach ($line in $lines) {
        if ($line -notmatch "(?i)\bwarning\s+[A-Z]{2}\d{4}\b") {
            continue
        }

        $code = ""
        if ($line -match "(?i)\bwarning\s+(?<code>[A-Z]{2}\d{4})\b") {
            $code = $Matches['code'].ToUpperInvariant()
        }

        $path = ""
        if ($line -match "^(?<path>[A-Za-z]:\\[^\(\[]+?\.(cs|csproj|props|targets))") {
            $path = ConvertTo-DevflowRelativePath -Path $Matches['path'] -RepoRoot $RepoRoot
        }

        $warnings.Add([pscustomobject]@{
            code = $code
            path = $path
            message = $line.Trim()
        }) | Out-Null
    }

    return $warnings
}

function Test-DevflowWarningMatchesBaseline {
    param(
        [Parameter(Mandatory=$true)]$Warning,
        [Parameter(Mandatory=$true)]$KnownWarning
    )

    if ($Warning.code -ne $KnownWarning.code) {
        return $false
    }

    $warningPath = ("" + $Warning.path).Replace('\', '/')
	$knownPath = ("" + $KnownWarning.path).Replace('\', '/')
    if (-not [string]::IsNullOrWhiteSpace($knownPath)) {
        if (-not $warningPath.EndsWith($knownPath, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $false
        }
    }

    if ($KnownWarning.PSObject.Properties.Name -contains "contains") {
        $needle = "" + $KnownWarning.contains
        if (-not [string]::IsNullOrWhiteSpace($needle)) {
            if (("" + $Warning.message).IndexOf($needle, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
                return $false
            }
        }
    }

    return $true
}

function Assert-DevflowKnownWarnings {
    param(
        [Parameter(Mandatory=$true)][string]$BuildLogPath,
        [Parameter(Mandatory=$true)][string]$KnownWarningsPath,
        [Parameter(Mandatory=$true)][string]$RepoRoot,
        [Parameter(Mandatory=$true)][string]$RunDir,
        [switch]$AllowUnexpectedWarnings
    )

    $warnings = @(Get-DevflowBuildWarnings -BuildLogPath $BuildLogPath -RepoRoot $RepoRoot)
    $knownWarnings = @()
    $policyFailOnUnexpected = $true

    if (Test-Path $KnownWarningsPath) {
        $knownRaw = Get-Content -Raw -Encoding UTF8 -Path $KnownWarningsPath
        $knownDoc = $knownRaw | ConvertFrom-Json
        if ($knownDoc.known_warnings) {
            $knownWarnings = @($knownDoc.known_warnings)
        }
        if ($knownDoc.policy -and ($null -ne $knownDoc.policy.fail_on_unexpected_warnings)) {
            $policyFailOnUnexpected = [bool]$knownDoc.policy.fail_on_unexpected_warnings
        }
    }

    $knownSeen = New-Object System.Collections.Generic.List[object]
    $unexpected = New-Object System.Collections.Generic.List[object]

    foreach ($warning in $warnings) {
        $matched = $false
        foreach ($known in $knownWarnings) {
            if (Test-DevflowWarningMatchesBaseline -Warning $warning -KnownWarning $known) {
                $knownSeen.Add($warning) | Out-Null
                $matched = $true
                break
            }
        }

        if (-not $matched) {
            $unexpected.Add($warning) | Out-Null
        }
    }

    $warningReport = [ordered]@{
        total_warnings = $warnings.Count
        known_warnings = $knownSeen.Count
        unexpected_warnings = $unexpected.Count
        policy_fail_on_unexpected_warnings = $policyFailOnUnexpected
        warnings = $warnings
        unexpected = $unexpected
    }

    $warningReportPath = Join-Path $RunDir "warnings.json"
    $warningReport | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path $warningReportPath

    Write-Host "Warnings: total=$($warnings.Count), known=$($knownSeen.Count), unexpected=$($unexpected.Count)"
    Write-Host "Warning report: $warningReportPath"

    if (($unexpected.Count -gt 0) -and $policyFailOnUnexpected -and (-not $AllowUnexpectedWarnings)) {
        Write-Host "Unexpected warnings:"
        foreach ($warning in $unexpected) {
            Write-Host "- $($warning.code) $($warning.path) :: $($warning.message)"
        }
        throw "Build produced $($unexpected.Count) unexpected warning(s). Add intentional warnings to .devflow/KNOWN_WARNINGS.json or fix them."
    }

    return $warningReport
}
