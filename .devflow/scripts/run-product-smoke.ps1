param(
    [string]$Scenario = "baseline-strict-package-assembly"
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment

$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$RunStamp = Get-Date -Format "yyyyMMdd_HHmmss"
$RunDir = Join-Path $RepoRoot ".devflow\runs\$RunStamp-product-smoke"
$SummaryPath = Join-Path $RunDir "product-smoke-summary.json"
$MarkdownPath = Join-Path $RunDir "product-smoke-summary.md"
$LogIndexPath = Join-Path $RunDir "logs.txt"
$TestResultsDir = Join-Path $RunDir "test-results"
$PackageOutputDir = Join-Path $RunDir "package-output"
$TestFilter = "FullyQualifiedName~ProductSmoke"
$ProductSmokeCommand = "dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter $TestFilter"

New-Item -ItemType Directory -Force -Path $RunDir | Out-Null
New-Item -ItemType Directory -Force -Path $TestResultsDir | Out-Null
New-Item -ItemType Directory -Force -Path $PackageOutputDir | Out-Null

function Write-ProductSmokeSummary {
    param(
        [Parameter(Mandatory=$true)][string]$Status,
        [string]$ErrorMessage = ""
    )

    $packageJsonPath = if ($Scenario -eq "active-package-quest-dialogue-preview") {
        Join-Path $PackageOutputDir ".llmgc\package-assembly\package.json"
    } else {
        Join-Path $PackageOutputDir "package.json"
    }
    $summary = [ordered]@{
        status = $Status
        scenario = $Scenario
        timestamp_utc = (Get-Date).ToUniversalTime().ToString("o")
        repo_root = "$RepoRoot"
        run_dir = "$RunDir"
        test_results_dir = "$TestResultsDir"
        package_output_dir = "$PackageOutputDir"
        package_json_path = "$packageJsonPath"
        package_json_exists = [bool](Test-Path $packageJsonPath)
        command = $ProductSmokeCommand
        no_llm_provider = $true
    }

    if (-not [string]::IsNullOrWhiteSpace($ErrorMessage)) {
        $summary.error = $ErrorMessage
    }

    $summary | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path $SummaryPath

    $markdown = @(
        "# Product Smoke Summary",
        "",
        "- Status: $Status",
        "- Scenario: $Scenario",
        "- Run directory: $RunDir",
        "- Test results: $TestResultsDir",
        "- Package output: $PackageOutputDir",
        "- Package JSON exists: $([bool](Test-Path $packageJsonPath))",
        "- LLM/provider calls: none"
    )

    if (-not [string]::IsNullOrWhiteSpace($ErrorMessage)) {
        $markdown += "- Error: $ErrorMessage"
    }

    Write-DevflowUtf8File -Path $MarkdownPath -Content (($markdown -join [Environment]::NewLine) + [Environment]::NewLine)
}

$PreviousPackageOutput = $env:LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR
$PreviousProjectDir = $env:LLMGC_PRODUCT_SMOKE_PROJECT_DIR

Push-Location $RepoRoot
try {
    if ($Scenario -eq "baseline-strict-package-assembly") {
        $TestFilter = "FullyQualifiedName~BaselineStrictArtifactsPackageAssemblySmokeTests"
    }
    elseif ($Scenario -eq "generated-package-runtime-preview") {
        $TestFilter = "FullyQualifiedName~GeneratedPackageRuntimePreviewSmoke"
    }
    elseif ($Scenario -eq "expanded-contract-batch-smoke") {
        $TestFilter = "FullyQualifiedName~ExpandedContractBatchSmoke"
    }
    elseif ($Scenario -eq "generated-content-interaction-preview") {
        $TestFilter = "FullyQualifiedName~GeneratedContentInteractionPreviewProductSmoke"
    }
    elseif ($Scenario -eq "active-package-quest-dialogue-preview") {
        $TestFilter = "FullyQualifiedName~ActivePackageQuestDialoguePreviewProductSmoke"
    }
    elseif ($Scenario -eq "generated-map-placement-preview") {
        $TestFilter = "FullyQualifiedName~GeneratedMapPlacementPreviewProductSmoke"
    }
    elseif ($Scenario -eq "content-language-policy") {
        $TestFilter = "FullyQualifiedName~ContentLanguagePolicyProductSmoke"
    }
    elseif ($Scenario -eq "game-blueprint-capability-compatibility") {
        $TestFilter = "FullyQualifiedName~GameBlueprintCapabilityCompatibilityProductSmoke"
    }
    elseif ($Scenario -eq "generator-catalog-contract") {
        $TestFilter = "FullyQualifiedName~GeneratorCatalogContractProductSmoke"
    }
    elseif ($Scenario -eq "composition-diagnostics-report") {
        $TestFilter = "FullyQualifiedName~CompositionDiagnosticsReportProductSmoke"
    }
    else {
        throw "Unknown product smoke scenario: $Scenario"
    }

    $ProductSmokeCommand = "dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter $TestFilter"
    $env:LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR = $PackageOutputDir
    $env:LLMGC_PRODUCT_SMOKE_PROJECT_DIR = $PackageOutputDir

    Invoke-DevflowLoggedCommand -Name "product-smoke-test" -Exe "dotnet" -ArgsList @(
        "test",
        "tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj",
        "--configuration",
        "Debug",
        "--filter",
        $TestFilter,
        "--results-directory",
        $TestResultsDir,
        "--logger",
        "trx;LogFileName=product-smoke.trx",
        "/p:EnableWindowsTargeting=true"
    ) -RunDir $RunDir -LogIndexPath $LogIndexPath | Out-Null

    Write-ProductSmokeSummary -Status "passed"

    Write-Host ""
    Write-Host "PRODUCT SMOKE PASSED"
    Write-Host "Scenario: $Scenario"
    Write-Host "Run directory: $RunDir"
}
catch {
    Write-ProductSmokeSummary -Status "failed" -ErrorMessage $_.Exception.Message
    Write-Error $_.Exception.Message
    Write-Host "Run directory: $RunDir"
    exit 1
}
finally {
    $env:LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR = $PreviousPackageOutput
    $env:LLMGC_PRODUCT_SMOKE_PROJECT_DIR = $PreviousProjectDir
    Pop-Location
}
