param(
    [int]$TimeoutMinutes = 45,
    [int]$HeartbeatSeconds = 60,
    [switch]$DryRun,
    [string]$RunId = ""
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment

$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
if ([string]::IsNullOrWhiteSpace($RunId)) {
    $RunId = "$(Get-Date -Format "yyyyMMdd_HHmmss")-check-all-observed"
}
elseif (-not $RunId.EndsWith("-check-all-observed", [System.StringComparison]::OrdinalIgnoreCase)) {
    $RunId = "$RunId-check-all-observed"
}

$RunDir = Join-Path $RepoRoot ".devflow\runs\$RunId"
$SummaryPath = Join-Path $RunDir "summary.json"
$OutputLogPath = Join-Path $RunDir "check-all-observed.log"
$ErrorLogPath = Join-Path $RunDir "check-all-observed.err.log"

function Stop-ObservedProcessTree {
    param([Parameter(Mandatory=$true)][int]$ProcessId)

    $childIds = @()
    try {
        $children = @(Get-CimInstance Win32_Process -Filter "ParentProcessId = $ProcessId" -ErrorAction Stop)
        foreach ($child in $children) {
            $childIds += [int]$child.ProcessId
            $childIds += Stop-ObservedProcessTree -ProcessId ([int]$child.ProcessId)
        }
    }
    catch {
        $childIds = @()
    }

    foreach ($childId in ($childIds | Select-Object -Unique)) {
        try { Stop-Process -Id $childId -Force -ErrorAction SilentlyContinue } catch { }
    }

    try { Stop-Process -Id $ProcessId -Force -ErrorAction SilentlyContinue } catch { }
    return @($childIds | Select-Object -Unique)
}

function Get-ObservedLogMetrics {
    param([Parameter(Mandatory=$true)][string]$LogPath)

    $metrics = [ordered]@{
        check_all_run_dir = ""
        non_product_passed = $null
        non_product_failed = $null
        non_product_skipped = $null
        warning_count_from_log = $null
    }

    if (-not (Test-Path -LiteralPath $LogPath)) {
        return $metrics
    }

    $lines = Get-Content -LiteralPath $LogPath -Encoding UTF8
    foreach ($line in $lines) {
        if ($line -match "Run directory:\s*(?<dir>.+)$") {
            $metrics.check_all_run_dir = $Matches["dir"].Trim()
        }

        if ($line -match "Passed:\s*(?<passed>\d+),\s*Failed:\s*(?<failed>\d+),\s*Skipped:\s*(?<skipped>\d+)") {
            $metrics.non_product_passed = [int]$Matches["passed"]
            $metrics.non_product_failed = [int]$Matches["failed"]
            $metrics.non_product_skipped = [int]$Matches["skipped"]
        }
    }

    return $metrics
}

function Write-ObservedSummary {
    param(
        [Parameter(Mandatory=$true)][string]$Status,
        [int]$ExitCode = 0,
        [int]$ElapsedSeconds = 0,
        [bool]$TimedOut = $false,
        [int[]]$StoppedProcessIds = @(),
        [string]$ErrorMessage = ""
    )

    $metrics = Get-ObservedLogMetrics -LogPath $OutputLogPath
    $checkAllSummary = $null
    $checkAllWarnings = $null
    $checkAllRunDir = "" + $metrics.check_all_run_dir
    if (-not [string]::IsNullOrWhiteSpace($checkAllRunDir)) {
        $summaryCandidate = Join-Path $checkAllRunDir "summary.json"
        $warningsCandidate = Join-Path $checkAllRunDir "warnings.json"
        if (Test-Path -LiteralPath $summaryCandidate) {
            try { $checkAllSummary = Get-Content -Raw -Encoding UTF8 -LiteralPath $summaryCandidate | ConvertFrom-Json } catch { $checkAllSummary = $null }
        }
        if (Test-Path -LiteralPath $warningsCandidate) {
            try { $checkAllWarnings = Get-Content -Raw -Encoding UTF8 -LiteralPath $warningsCandidate | ConvertFrom-Json } catch { $checkAllWarnings = $null }
        }
    }

    $summary = [ordered]@{
        schemaVersion = "check_all_observed_summary_v1"
        status = $Status
        dry_run = [bool]$DryRun
        run_id = $RunId
        run_dir = ConvertTo-DevflowRelativePath -Path $RunDir -RepoRoot $RepoRoot
        command = ".\.devflow\scripts\check-all.ps1"
        timeout_minutes = $TimeoutMinutes
        heartbeat_seconds = $HeartbeatSeconds
        elapsed_seconds = $ElapsedSeconds
        exit_code = $ExitCode
        timed_out = $TimedOut
        stopped_process_ids = @($StoppedProcessIds)
        output_log = ConvertTo-DevflowRelativePath -Path $OutputLogPath -RepoRoot $RepoRoot
        error_log = ConvertTo-DevflowRelativePath -Path $ErrorLogPath -RepoRoot $RepoRoot
        check_all_run_dir = if ([string]::IsNullOrWhiteSpace($checkAllRunDir)) { "" } else { ConvertTo-DevflowRelativePath -Path $checkAllRunDir -RepoRoot $RepoRoot }
        warnings_total = if ($null -ne $checkAllSummary) { $checkAllSummary.warnings_total } else { $null }
        warnings_unexpected = if ($null -ne $checkAllSummary) { $checkAllSummary.warnings_unexpected } elseif ($null -ne $checkAllWarnings) { $checkAllWarnings.unexpected_warnings } else { $null }
        non_product_passed = $metrics.non_product_passed
        non_product_failed = $metrics.non_product_failed
        non_product_skipped = $metrics.non_product_skipped
        raw_trx_or_logs_copied_into_tracked_evidence = $false
    }

    if (-not [string]::IsNullOrWhiteSpace($ErrorMessage)) {
        $summary["error"] = $ErrorMessage
    }

    $summary | ConvertTo-Json -Depth 10 | Set-Content -Encoding UTF8 -Path $SummaryPath
}

New-Item -ItemType Directory -Force -Path $RunDir | Out-Null

Push-Location $RepoRoot
try {
    if (-not (Test-Path "LLMGameCreator.sln")) {
        throw "LLMGameCreator.sln not found. Current root: $RepoRoot"
    }

    if ($DryRun) {
        "" | Set-Content -Encoding UTF8 -Path $OutputLogPath
        "" | Set-Content -Encoding UTF8 -Path $ErrorLogPath
        Write-Host "DRY RUN: .\.devflow\scripts\check-all.ps1 would run with TimeoutMinutes=$TimeoutMinutes and HeartbeatSeconds=$HeartbeatSeconds"
        Write-ObservedSummary -Status "planned" -ExitCode 0 -ElapsedSeconds 0 -TimedOut:$false
        Write-Host "Run directory: $RunDir"
        exit 0
    }

    if (Test-Path -LiteralPath $OutputLogPath) { Remove-Item -LiteralPath $OutputLogPath -Force }
    if (Test-Path -LiteralPath $ErrorLogPath) { Remove-Item -LiteralPath $ErrorLogPath -Force }

    $args = @(
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        ".\.devflow\scripts\check-all.ps1"
    )

    Write-DevflowStep -Message "Observed full check-all"
    Write-Host "powershell $($args -join ' ')"
    $start = Get-Date
    $process = Start-Process -FilePath "powershell" -ArgumentList $args -WorkingDirectory $RepoRoot -NoNewWindow -PassThru -RedirectStandardOutput $OutputLogPath -RedirectStandardError $ErrorLogPath
    $lastHeartbeat = Get-Date
    $timeoutAt = if ($TimeoutMinutes -gt 0) { $start.AddMinutes($TimeoutMinutes) } else { [DateTime]::MaxValue }
    $timedOut = $false
    $stoppedIds = @()

    while (-not $process.HasExited) {
        Start-Sleep -Seconds 1
        $now = Get-Date
        if ($HeartbeatSeconds -gt 0 -and (($now - $lastHeartbeat).TotalSeconds -ge $HeartbeatSeconds)) {
            $elapsed = [int](($now - $start).TotalSeconds)
            Write-Host "[heartbeat] check-all still running after ${elapsed}s"
            $lastHeartbeat = $now
        }

        if ($now -ge $timeoutAt) {
            $timedOut = $true
            Write-Host "[timeout] check-all exceeded TimeoutMinutes=$TimeoutMinutes"
            $stoppedIds = @(Stop-ObservedProcessTree -ProcessId $process.Id)
            break
        }
    }

    $elapsedSeconds = [int](((Get-Date) - $start).TotalSeconds)
    $exitCode = if ($timedOut) { -1 } else { [int]$process.ExitCode }
    $status = if ($timedOut) { "timed_out" } elseif ($exitCode -eq 0) { "passed" } else { "failed" }

    if (Test-Path -LiteralPath $OutputLogPath) {
        Get-Content -LiteralPath $OutputLogPath -Encoding UTF8 | ForEach-Object { Write-Host $_ }
    }
    if (Test-Path -LiteralPath $ErrorLogPath) {
        Get-Content -LiteralPath $ErrorLogPath -Encoding UTF8 | ForEach-Object { Write-Host $_ }
    }

    Write-ObservedSummary -Status $status -ExitCode $exitCode -ElapsedSeconds $elapsedSeconds -TimedOut:$timedOut -StoppedProcessIds $stoppedIds
    Write-Host ""
    Write-Host "CHECK-ALL-OBSERVED $($status.ToUpperInvariant())"
    Write-Host "Run directory: $RunDir"

    if ($status -ne "passed") {
        exit 1
    }
}
catch {
    Write-ObservedSummary -Status "failed" -ExitCode 1 -ElapsedSeconds 0 -TimedOut:$false -ErrorMessage $_.Exception.Message
    Write-Error $_.Exception.Message
    Write-Host "Run directory: $RunDir"
    exit 1
}
finally {
    Pop-Location
}
