param(
    [string]$Scenario = "",
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
    $RunId = "$(Get-Date -Format "yyyyMMdd_HHmmss")-check-spine-fast"
}
elseif (-not $RunId.EndsWith("-check-spine-fast", [System.StringComparison]::OrdinalIgnoreCase)) {
    $RunId = "$RunId-check-spine-fast"
}

$RunDir = Join-Path $RepoRoot ".devflow\runs\$RunId"
$SummaryPath = Join-Path $RunDir "summary.json"
$LogIndexPath = Join-Path $RunDir "logs.txt"
$ProductSmokeProjectDir = Join-Path $RunDir "test-artifacts\project-root"
$ProductSmokePackageOutputDir = Join-Path $RunDir "test-artifacts\package-output"
$PreviousProductSmokeProjectDir = $env:LLMGC_PRODUCT_SMOKE_PROJECT_DIR
$PreviousProductSmokePackageOutputDir = $env:LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR
$StepResults = New-Object System.Collections.Generic.List[object]

$FocusedFilters = @(
    @{ name = "VisualAssetContractRatingMetadata"; filter = "FullyQualifiedName~Application.VisualAssetContractRatingMetadata" },
    @{ name = "VisualPartPackRuleStack"; filter = "FullyQualifiedName~Application.VisualPartPackRuleStack" },
    @{ name = "DeterministicVisualMicrotileMaterializer"; filter = "FullyQualifiedName~Application.DeterministicVisualMicrotileMaterializer" },
    @{ name = "DeterministicVisualMapPatchComposer"; filter = "FullyQualifiedName~Application.DeterministicVisualMapPatchComposer" },
    @{ name = "DeterministicVisualRegionComposer"; filter = "FullyQualifiedName~Application.DeterministicVisualRegionComposer" }
)

$ProductSmokeFilters = @(
    @{ name = "VisualAssetContractRatingMetadataProductSmoke"; filter = "FullyQualifiedName~ProductSmoke.VisualAssetContractRatingMetadataProductSmokeTests" },
    @{ name = "VisualPartPackRuleStackProductSmoke"; filter = "FullyQualifiedName~ProductSmoke.VisualPartPackRuleStackProductSmokeTests" },
    @{ name = "DeterministicVisualMicrotileMaterializerProductSmoke"; filter = "FullyQualifiedName~ProductSmoke.DeterministicVisualMicrotileMaterializerProductSmokeTests" },
    @{ name = "DeterministicVisualMapPatchComposerProductSmoke"; filter = "FullyQualifiedName~ProductSmoke.DeterministicVisualMapPatchComposerProductSmokeTests" },
    @{ name = "DeterministicVisualRegionComposerProductSmoke"; filter = "FullyQualifiedName~ProductSmoke.DeterministicVisualRegionComposerProductSmokeTests" }
)

function ConvertTo-SpineFastCommandText {
    param(
        [Parameter(Mandatory=$true)][string]$Exe,
        [Parameter(Mandatory=$true)][string[]]$ArgsList
    )

    return "$Exe $($ArgsList -join ' ')"
}

function Add-SpineFastStep {
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

function Stop-SpineFastProcessTree {
    param([Parameter(Mandatory=$true)][int]$ProcessId)

    $childIds = @()
    try {
        $children = @(Get-CimInstance Win32_Process -Filter "ParentProcessId = $ProcessId" -ErrorAction Stop)
        foreach ($child in $children) {
            $childIds += [int]$child.ProcessId
            $childIds += Stop-SpineFastProcessTree -ProcessId ([int]$child.ProcessId)
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

function Invoke-SpineFastStep {
    param(
        [Parameter(Mandatory=$true)][string]$Name,
        [Parameter(Mandatory=$true)][string]$Exe,
        [Parameter(Mandatory=$true)][string[]]$ArgsList
    )

    $commandText = ConvertTo-SpineFastCommandText -Exe $Exe -ArgsList $ArgsList
    Write-DevflowStep -Message $Name -LogIndexPath $LogIndexPath
    Write-Host $commandText

    if ($DryRun) {
        Add-SpineFastStep -Name $Name -Kind "command" -CommandText $commandText -Status "planned" -Reason "DryRun"
        return
    }

    $safeName = $Name -replace '[^A-Za-z0-9_.-]', '-'
    $stdoutPath = Join-Path $RunDir "$safeName.log"
    $stderrPath = Join-Path $RunDir "$safeName.err.log"
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
            Stop-SpineFastProcessTree -ProcessId $process.Id | Out-Null
            break
        }
    }

    $elapsedSeconds = [int](((Get-Date) - $start).TotalSeconds)
    $exitCode = if ($timedOut) { -1 } else { [int]$process.ExitCode }
    $status = if ($timedOut) { "timed_out" } elseif ($exitCode -eq 0) { "passed" } else { "failed" }
    Add-SpineFastStep -Name $Name -Kind "command" -CommandText $commandText -Status $status -ExitCode $exitCode -LogPath $stdoutPath -ElapsedSeconds $elapsedSeconds -TimedOut:$timedOut

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

function Write-SpineFastSummary {
    param(
        [Parameter(Mandatory=$true)][string]$Status,
        [string]$ErrorMessage = ""
    )

    $summary = [ordered]@{
        schemaVersion = "check_spine_fast_summary_v1"
        status = $Status
        dry_run = [bool]$DryRun
        run_id = $RunId
        run_dir = ConvertTo-DevflowRelativePath -Path $RunDir -RepoRoot $RepoRoot
        scenario = $Scenario
        timeout_minutes = $TimeoutMinutes
        heartbeat_seconds = $HeartbeatSeconds
        skip_restore = [bool]$SkipRestore
        skip_build = [bool]$SkipBuild
        skip_artifact_scope = [bool]$SkipArtifactScope
        focused_filters = @($FocusedFilters | ForEach-Object { $_.name })
        product_smoke_filters = @($ProductSmokeFilters | ForEach-Object { $_.name })
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
        Invoke-SpineFastStep -Name "restore" -Exe "dotnet" -ArgsList @("restore", "LLMGameCreator.sln")
    }
    else {
        Add-SpineFastStep -Name "restore" -Kind "command" -CommandText "dotnet restore LLMGameCreator.sln" -Status "skipped" -Reason "SkipRestore"
    }

    if (-not $SkipBuild) {
        Invoke-SpineFastStep -Name "build" -Exe "dotnet" -ArgsList @(
            "build",
            "LLMGameCreator.sln",
            "--configuration",
            "Debug",
            "--no-restore",
            "/p:EnableWindowsTargeting=true"
        )
    }
    else {
        Add-SpineFastStep -Name "build" -Kind "command" -CommandText "dotnet build LLMGameCreator.sln --configuration Debug --no-restore /p:EnableWindowsTargeting=true" -Status "skipped" -Reason "SkipBuild"
    }

    Invoke-SpineFastStep -Name "current-state-tests" -Exe "dotnet" -ArgsList @(
        "test",
        "tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj",
        "--configuration",
        "Debug",
        "--no-build",
        "--filter",
        "CurrentState",
        "/p:EnableWindowsTargeting=true"
    )

    foreach ($filter in $FocusedFilters) {
        Invoke-SpineFastStep -Name "focused-$($filter.name)" -Exe "dotnet" -ArgsList @(
            "test",
            "tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj",
            "--configuration",
            "Debug",
            "--no-build",
            "--filter",
            $filter.filter,
            "/p:EnableWindowsTargeting=true"
        )
    }

    foreach ($filter in $ProductSmokeFilters) {
        Invoke-SpineFastStep -Name "product-smoke-$($filter.name)" -Exe "dotnet" -ArgsList @(
            "test",
            "tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj",
            "--configuration",
            "Debug",
            "--no-build",
            "--filter",
            $filter.filter,
            "/p:EnableWindowsTargeting=true"
        )
    }

    Invoke-SpineFastStep -Name "git-diff-check" -Exe "git" -ArgsList @("diff", "--check")

    if (-not $SkipArtifactScope -and -not [string]::IsNullOrWhiteSpace($Scenario)) {
        Invoke-SpineFastStep -Name "artifact-scope" -Exe "powershell" -ArgsList @(
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
        Add-SpineFastStep -Name "artifact-scope" -Kind "command" -CommandText ".\.devflow\scripts\check-artifact-scope.ps1 -Scenario <Scenario>" -Status "skipped" -Reason $reason
    }

    Write-SpineFastSummary -Status "passed"
    Write-Host ""
    Write-Host "CHECK-SPINE-FAST PASSED"
    Write-Host "Run directory: $RunDir"
}
catch {
    Write-SpineFastSummary -Status "failed" -ErrorMessage $_.Exception.Message
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
