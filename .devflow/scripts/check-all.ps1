param(
    [string]$Configuration = "Debug",
    [switch]$SkipRestore,
    [switch]$SkipTests,
    [switch]$AllowUnexpectedWarnings
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment

$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$RunStamp = Get-Date -Format "yyyyMMdd_HHmmss"
$RunDir = Join-Path $RepoRoot ".devflow\runs\$RunStamp-check-all"
New-Item -ItemType Directory -Force -Path $RunDir | Out-Null

$SummaryPath = Join-Path $RunDir "summary.json"
$LogIndexPath = Join-Path $RunDir "logs.txt"
$PreviousProductSmokeProjectDir = $env:LLMGC_PRODUCT_SMOKE_PROJECT_DIR
$PreviousProductSmokePackageOutputDir = $env:LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR
$CheckAllProductSmokeProjectDir = Join-Path $RunDir "test-artifacts\project-root"
$CheckAllProductSmokePackageOutputDir = Join-Path $RunDir "test-artifacts\package-output"

Push-Location $RepoRoot
try {
    if (-not (Test-Path "LLMGameCreator.sln")) {
        throw "LLMGameCreator.sln not found. Current root: $RepoRoot"
    }

    Write-DevflowStep -Message "Environment" -LogIndexPath $LogIndexPath
    dotnet --info 2>&1 | Tee-Object -FilePath (Join-Path $RunDir "dotnet-info.log")
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet --info failed."
    }

    Write-DevflowStep -Message "Devflow state check" -LogIndexPath $LogIndexPath
    & powershell -NoProfile -ExecutionPolicy Bypass -File ".\.devflow\scripts\check-devflow-state.ps1" 2>&1 |
        Tee-Object -FilePath (Join-Path $RunDir "devflow-state.log")
    if ($LASTEXITCODE -ne 0) {
        throw "Devflow state check failed."
    }

    if (-not $SkipRestore) {
        Invoke-DevflowLoggedCommand -Name "restore" -Exe "dotnet" -ArgsList @(
            "restore",
            "LLMGameCreator.sln"
        ) -RunDir $RunDir -LogIndexPath $LogIndexPath | Out-Null
    }

    $buildLogPath = Invoke-DevflowLoggedCommand -Name "build" -Exe "dotnet" -ArgsList @(
        "build",
        "LLMGameCreator.sln",
        "--configuration",
        $Configuration,
        "--no-restore",
        "/p:EnableWindowsTargeting=true"
    ) -RunDir $RunDir -LogIndexPath $LogIndexPath

    $warningReport = Assert-DevflowKnownWarnings `
        -BuildLogPath $buildLogPath `
        -KnownWarningsPath ".devflow\KNOWN_WARNINGS.json" `
        -RepoRoot $RepoRoot `
        -RunDir $RunDir `
        -AllowUnexpectedWarnings:$AllowUnexpectedWarnings

    if (-not $SkipTests) {
        $TestResultsDir = Join-Path $RunDir "test-results"
        New-Item -ItemType Directory -Force -Path $TestResultsDir | Out-Null
        New-Item -ItemType Directory -Force -Path $CheckAllProductSmokeProjectDir | Out-Null
        New-Item -ItemType Directory -Force -Path $CheckAllProductSmokePackageOutputDir | Out-Null

        $RepoProceduralRoot = Join-Path $RepoRoot ".llmgc\procedural"
        if (Test-Path $RepoProceduralRoot) {
            New-Item -ItemType Directory -Force -Path (Join-Path $CheckAllProductSmokeProjectDir ".llmgc") | Out-Null
            Copy-Item -LiteralPath $RepoProceduralRoot -Destination (Join-Path $CheckAllProductSmokeProjectDir ".llmgc") -Recurse -Force
        }

        $env:LLMGC_PRODUCT_SMOKE_PROJECT_DIR = $CheckAllProductSmokeProjectDir
        $env:LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR = $CheckAllProductSmokePackageOutputDir

        Invoke-DevflowLoggedCommand -Name "test" -Exe "dotnet" -ArgsList @(
            "test",
            "tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj",
            "--configuration",
            $Configuration,
            "--no-build",
            "--filter",
            "FullyQualifiedName!~ProductSmoke",
            "--results-directory",
            $TestResultsDir,
            "--logger",
            "trx;LogFileName=llmgc-tests.trx",
            "/p:EnableWindowsTargeting=true"
        ) -RunDir $RunDir -LogIndexPath $LogIndexPath | Out-Null
    }

    $summary = [ordered]@{
        status = "passed"
        timestamp_utc = (Get-Date).ToUniversalTime().ToString("o")
        repo_root = "$RepoRoot"
        run_dir = "$RunDir"
        configuration = $Configuration
        skip_restore = [bool]$SkipRestore
        skip_tests = [bool]$SkipTests
        warnings_total = [int]$warningReport.total_warnings
        warnings_known = [int]$warningReport.known_warnings
        warnings_unexpected = [int]$warningReport.unexpected_warnings
        dotnet_cli_ui_language = $env:DOTNET_CLI_UI_LANGUAGE
        vslang = $env:VSLANG
        code_page = "65001"
        product_smoke_project_dir = "$CheckAllProductSmokeProjectDir"
        product_smoke_package_output_dir = "$CheckAllProductSmokePackageOutputDir"
        product_smoke_seeded_procedural_baseline = [bool](Test-Path (Join-Path $CheckAllProductSmokeProjectDir ".llmgc\procedural"))
    }

    $summary | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path $SummaryPath

    Write-Host ""
    Write-Host "CHECK-ALL PASSED"
    Write-Host "Run directory: $RunDir"
}
catch {
    $summary = [ordered]@{
        status = "failed"
        timestamp_utc = (Get-Date).ToUniversalTime().ToString("o")
        repo_root = "$RepoRoot"
        run_dir = "$RunDir"
        error = $_.Exception.Message
        dotnet_cli_ui_language = $env:DOTNET_CLI_UI_LANGUAGE
        vslang = $env:VSLANG
        code_page = "65001"
    }

    $summary | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path $SummaryPath

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
