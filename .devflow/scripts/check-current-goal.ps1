param(
    [string]$Scenario = "",
    [string]$FocusedFilter = "",
    [string]$ProductSmokeFilter = "",
    [switch]$SkipRestore,
    [switch]$SkipBuild,
    [switch]$SkipArtifactScope,
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
    $RunId = "$(Get-Date -Format "yyyyMMdd_HHmmss")-check-current-goal"
}
elseif (-not $RunId.EndsWith("-check-current-goal", [System.StringComparison]::OrdinalIgnoreCase)) {
    $RunId = "$RunId-check-current-goal"
}

$RunDir = Join-Path $RepoRoot ".devflow\runs\$RunId"
$SummaryPath = Join-Path $RunDir "summary.json"
$LogIndexPath = Join-Path $RunDir "logs.txt"
$ProductSmokeProjectDir = Join-Path $RunDir "test-artifacts\project-root"
$ProductSmokePackageOutputDir = Join-Path $RunDir "test-artifacts\package-output"
$PreviousProductSmokeProjectDir = $env:LLMGC_PRODUCT_SMOKE_PROJECT_DIR
$PreviousProductSmokePackageOutputDir = $env:LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR
$StepResults = New-Object System.Collections.Generic.List[object]

function ConvertTo-CurrentGoalCommandText {
    param(
        [Parameter(Mandatory=$true)][string]$Exe,
        [Parameter(Mandatory=$true)][string[]]$ArgsList
    )

    return "$Exe $($ArgsList -join ' ')"
}

function Add-CurrentGoalStep {
    param(
        [Parameter(Mandatory=$true)][string]$Name,
        [Parameter(Mandatory=$true)][string]$Kind,
        [Parameter(Mandatory=$true)][string]$CommandText,
        [Parameter(Mandatory=$true)][string]$Status,
        [int]$ExitCode = 0,
        [string]$LogPath = "",
        [int]$ElapsedSeconds = 0,
        [bool]$TimedOut = $false,
        [string]$Reason = ""
    )

    $StepResults.Add([ordered]@{
        name = $Name
        kind = $Kind
        command = $CommandText
        status = $Status
        exit_code = $ExitCode
        elapsed_seconds = $ElapsedSeconds
        timed_out = $TimedOut
        log_path = if ([string]::IsNullOrWhiteSpace($LogPath)) { "" } else { ConvertTo-DevflowRelativePath -Path $LogPath -RepoRoot $RepoRoot }
        reason = $Reason
    }) | Out-Null
}

function Stop-CurrentGoalProcessTree {
    param([Parameter(Mandatory=$true)][int]$ProcessId)

    $childIds = @()
    try {
        $children = @(Get-CimInstance Win32_Process -Filter "ParentProcessId = $ProcessId" -ErrorAction Stop)
        foreach ($child in $children) {
            $childIds += [int]$child.ProcessId
            $childIds += Stop-CurrentGoalProcessTree -ProcessId ([int]$child.ProcessId)
        }
    }
    catch {
        $childIds = @()
    }

    foreach ($childId in ($childIds | Select-Object -Unique)) {
        try { Stop-Process -Id $childId -Force -ErrorAction SilentlyContinue } catch { }
    }

    try { Stop-Process -Id $ProcessId -Force -ErrorAction SilentlyContinue } catch { }
    return $childIds
}

function Invoke-CurrentGoalStep {
    param(
        [Parameter(Mandatory=$true)][string]$Name,
        [Parameter(Mandatory=$true)][string]$Exe,
        [Parameter(Mandatory=$true)][string[]]$ArgsList
    )

    $commandText = ConvertTo-CurrentGoalCommandText -Exe $Exe -ArgsList $ArgsList
    Write-DevflowStep -Message $Name -LogIndexPath $LogIndexPath
    Write-Host $commandText

    if ($DryRun) {
        Add-CurrentGoalStep -Name $Name -Kind "command" -CommandText $commandText -Status "planned" -Reason "DryRun"
        return
    }

    $stdoutPath = Join-Path $RunDir "$Name.log"
    $stderrPath = Join-Path $RunDir "$Name.err.log"
    if (Test-Path -LiteralPath $stdoutPath) { Remove-Item -LiteralPath $stdoutPath -Force }
    if (Test-Path -LiteralPath $stderrPath) { Remove-Item -LiteralPath $stderrPath -Force }

    Add-Content -Encoding UTF8 -Path $LogIndexPath -Value $stdoutPath
    $start = Get-Date
    $process = Start-Process -FilePath $Exe -ArgumentList $ArgsList -WorkingDirectory $RepoRoot -NoNewWindow -PassThru -RedirectStandardOutput $stdoutPath -RedirectStandardError $stderrPath
    $lastHeartbeat = Get-Date
    $timedOut = $false
    $timeoutAt = if ($TimeoutMinutes -gt 0) { $start.AddMinutes($TimeoutMinutes) } else { [DateTime]::MaxValue }

    while (-not $process.HasExited) {
        Start-Sleep -Seconds 1
        $now = Get-Date
        if ($HeartbeatSeconds -gt 0 -and (($now - $lastHeartbeat).TotalSeconds -ge $HeartbeatSeconds)) {
            $elapsed = [int](($now - $start).TotalSeconds)
            Write-Host "[heartbeat] $Name still running after ${elapsed}s"
            $lastHeartbeat = $now
        }

        if ($now -ge $timeoutAt) {
            $timedOut = $true
            Write-Host "[timeout] $Name exceeded TimeoutMinutes=$TimeoutMinutes"
            Stop-CurrentGoalProcessTree -ProcessId $process.Id | Out-Null
            break
        }
    }

    $elapsedSeconds = [int](((Get-Date) - $start).TotalSeconds)
    $exitCode = if ($timedOut) { -1 } else { [int]$process.ExitCode }
    $status = if ($timedOut) { "timed_out" } elseif ($exitCode -eq 0) { "passed" } else { "failed" }
    Add-CurrentGoalStep -Name $Name -Kind "command" -CommandText $commandText -Status $status -ExitCode $exitCode -LogPath $stdoutPath -ElapsedSeconds $elapsedSeconds -TimedOut:$timedOut

    if (Test-Path -LiteralPath $stdoutPath) {
        Get-Content -LiteralPath $stdoutPath -Encoding UTF8 | ForEach-Object { Write-Host $_ }
    }
    if (Test-Path -LiteralPath $stderrPath) {
        Get-Content -LiteralPath $stderrPath -Encoding UTF8 | ForEach-Object { Write-Host $_ }
    }

    if ($status -ne "passed") {
        throw "Step '$Name' failed with status $status and exit code $exitCode. Log: $stdoutPath"
    }
}

function Write-CurrentGoalSummary {
    param(
        [Parameter(Mandatory=$true)][string]$Status,
        [string]$ErrorMessage = ""
    )

    $summary = [ordered]@{
        schemaVersion = "check_current_goal_summary_v1"
        status = $Status
        dry_run = [bool]$DryRun
        run_id = $RunId
        run_dir = ConvertTo-DevflowRelativePath -Path $RunDir -RepoRoot $RepoRoot
        scenario = $Scenario
        focused_filter = $FocusedFilter
        product_smoke_filter = $ProductSmokeFilter
        skip_restore = [bool]$SkipRestore
        skip_build = [bool]$SkipBuild
        skip_artifact_scope = [bool]$SkipArtifactScope
        timeout_minutes = $TimeoutMinutes
        heartbeat_seconds = $HeartbeatSeconds
        product_smoke_project_dir = ConvertTo-DevflowRelativePath -Path $ProductSmokeProjectDir -RepoRoot $RepoRoot
        product_smoke_package_output_dir = ConvertTo-DevflowRelativePath -Path $ProductSmokePackageOutputDir -RepoRoot $RepoRoot
        steps = @($StepResults.ToArray())
    }

    if (-not [string]::IsNullOrWhiteSpace($ErrorMessage)) {
        $summary["error"] = $ErrorMessage
    }

    $summary | ConvertTo-Json -Depth 10 | Set-Content -Encoding UTF8 -Path $SummaryPath
}

New-Item -ItemType Directory -Force -Path $RunDir | Out-Null
New-Item -ItemType Directory -Force -Path $ProductSmokeProjectDir | Out-Null
New-Item -ItemType Directory -Force -Path $ProductSmokePackageOutputDir | Out-Null

Push-Location $RepoRoot
try {
    if (-not (Test-Path "LLMGameCreator.sln")) {
        throw "LLMGameCreator.sln not found. Current root: $RepoRoot"
    }

    $env:LLMGC_PRODUCT_SMOKE_PROJECT_DIR = $ProductSmokeProjectDir
    $env:LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR = $ProductSmokePackageOutputDir

    if (-not $SkipRestore) {
        Invoke-CurrentGoalStep -Name "restore" -Exe "dotnet" -ArgsList @("restore", "LLMGameCreator.sln")
    }
    else {
        Add-CurrentGoalStep -Name "restore" -Kind "command" -CommandText "dotnet restore LLMGameCreator.sln" -Status "skipped" -Reason "SkipRestore"
    }

    if (-not $SkipBuild) {
        Invoke-CurrentGoalStep -Name "build" -Exe "dotnet" -ArgsList @(
            "build",
            "LLMGameCreator.sln",
            "--configuration",
            "Debug",
            "--no-restore",
            "/p:EnableWindowsTargeting=true"
        )
    }
    else {
        Add-CurrentGoalStep -Name "build" -Kind "command" -CommandText "dotnet build LLMGameCreator.sln --configuration Debug --no-restore /p:EnableWindowsTargeting=true" -Status "skipped" -Reason "SkipBuild"
    }

    if (-not [string]::IsNullOrWhiteSpace($FocusedFilter)) {
        Invoke-CurrentGoalStep -Name "focused-tests" -Exe "dotnet" -ArgsList @(
            "test",
            "tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj",
            "--configuration",
            "Debug",
            "--no-build",
            "--filter",
            $FocusedFilter,
            "/p:EnableWindowsTargeting=true"
        )
    }
    else {
        Add-CurrentGoalStep -Name "focused-tests" -Kind "command" -CommandText "dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter <FocusedFilter>" -Status "skipped" -Reason "FocusedFilter not provided"
    }

    if (-not [string]::IsNullOrWhiteSpace($ProductSmokeFilter)) {
        Invoke-CurrentGoalStep -Name "product-smoke-tests" -Exe "dotnet" -ArgsList @(
            "test",
            "tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj",
            "--configuration",
            "Debug",
            "--no-build",
            "--filter",
            $ProductSmokeFilter,
            "/p:EnableWindowsTargeting=true"
        )
    }
    else {
        Add-CurrentGoalStep -Name "product-smoke-tests" -Kind "command" -CommandText "dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter <ProductSmokeFilter>" -Status "skipped" -Reason "ProductSmokeFilter not provided"
    }

    Invoke-CurrentGoalStep -Name "current-state-tests" -Exe "dotnet" -ArgsList @(
        "test",
        "tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj",
        "--configuration",
        "Debug",
        "--no-build",
        "--filter",
        "CurrentState",
        "/p:EnableWindowsTargeting=true"
    )

    if (-not $SkipArtifactScope -and -not [string]::IsNullOrWhiteSpace($Scenario)) {
        Invoke-CurrentGoalStep -Name "artifact-scope" -Exe "powershell" -ArgsList @(
            "-NoProfile",
            "-ExecutionPolicy",
            "Bypass",
            "-File",
            ".\.devflow\scripts\check-artifact-scope.ps1",
            "-Scenario",
            $Scenario
        )
    }
    else {
        $reason = if ($SkipArtifactScope) { "SkipArtifactScope" } else { "Scenario not provided" }
        Add-CurrentGoalStep -Name "artifact-scope" -Kind "command" -CommandText ".\.devflow\scripts\check-artifact-scope.ps1 -Scenario <Scenario>" -Status "skipped" -Reason $reason
    }

    Invoke-CurrentGoalStep -Name "git-diff-check" -Exe "git" -ArgsList @("diff", "--check")

    Write-CurrentGoalSummary -Status "passed"
    Write-Host ""
    Write-Host "CHECK-CURRENT-GOAL PASSED"
    Write-Host "Run directory: $RunDir"
}
catch {
    Write-CurrentGoalSummary -Status "failed" -ErrorMessage $_.Exception.Message
    Write-Error $_.Exception.Message
    Write-Host "Run directory: $RunDir"
    exit 1
}
finally {
    if ($null -eq $PreviousProductSmokeProjectDir) {
        Remove-Item Env:\LLMGC_PRODUCT_SMOKE_PROJECT_DIR -ErrorAction SilentlyContinue
    } else {
        $env:LLMGC_PRODUCT_SMOKE_PROJECT_DIR = $PreviousProductSmokeProjectDir
    }

    if ($null -eq $PreviousProductSmokePackageOutputDir) {
        Remove-Item Env:\LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR -ErrorAction SilentlyContinue
    } else {
        $env:LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR = $PreviousProductSmokePackageOutputDir
    }

    Pop-Location
}
