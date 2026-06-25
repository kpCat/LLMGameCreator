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
    } elseif ($Scenario -eq "procedural-game-kernel") {
        Join-Path $PackageOutputDir ".llmgc\procedural\generated-game-plan.json"
    } elseif ($Scenario -eq "formula-effect-action-registry") {
        Join-Path $PackageOutputDir ".llmgc\procedural\formula-effect-action-rule-pack.json"
    } elseif ($Scenario -eq "tiny-generated-runtime-loop") {
        Join-Path $PackageOutputDir ".llmgc\procedural\tiny-runtime-loop-state.json"
    } elseif ($Scenario -eq "generated-package-mvp") {
        Join-Path $PackageOutputDir ".llmgc\procedural\generated-package-mvp\package.json"
    } elseif ($Scenario -eq "runtime-backed-microgame-state" -or $Scenario -eq "generation-preset-options") {
        Join-Path $PackageOutputDir ".llmgc\procedural\runtime-backed-microgame-state\runtime-backed-microgame-state-snapshot.json"
    } elseif ($Scenario -eq "generated-microgame-variation") {
        Join-Path $PackageOutputDir ".llmgc\procedural\generated-microgame-variation\generated-microgame-variation-report.json"
    } elseif ($Scenario -eq "extension-spine") {
        Join-Path $PackageOutputDir ".llmgc\procedural\extension-spine\extension-spine-scenario-report.json"
    } elseif ($Scenario -eq "generated-microgame-loop" -or $Scenario -eq "runtime-owned-goal-progress" -or $Scenario -eq "runtime-reward-challenge-state") {
        Join-Path $PackageOutputDir ".llmgc\procedural\generated-microgame-loop\generated-microgame-loop-snapshot.json"
    } elseif ($Scenario -eq "visible-generated-playable-preview" -or $Scenario -eq "one-click-generated-preview-workflow" -or $Scenario -eq "generated-microgame-goal-loop" -or $Scenario -eq "generated-microgame-challenge-loop") {
        Join-Path $PackageOutputDir ".llmgc\procedural\visible-generated-playable-preview\visible-generated-playable-preview-snapshot.json"
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
    elseif ($Scenario -eq "composition-report-export") {
        $TestFilter = "FullyQualifiedName~CompositionReportExportProductSmoke"
    }
    elseif ($Scenario -eq "composition-workbench-readonly") {
        $TestFilter = "FullyQualifiedName~CompositionWorkbenchReadonlyProductSmoke"
    }
    elseif ($Scenario -eq "unity-target-contract") {
        $TestFilter = "FullyQualifiedName~UnityTargetContractProductSmoke"
    }
    elseif ($Scenario -eq "unity-archive-export-dry-run") {
        $TestFilter = "FullyQualifiedName~UnityArchiveExportDryRunProductSmoke"
    }
    elseif ($Scenario -eq "unity-archive-materialization") {
        $TestFilter = "FullyQualifiedName~UnityArchiveMaterializationProductSmoke"
    }
    elseif ($Scenario -eq "unity-archive-game-data-payload") {
        $TestFilter = "FullyQualifiedName~UnityArchiveGameDataPayloadProductSmoke"
    }
    elseif ($Scenario -eq "unity-archive-request-pipeline") {
        $TestFilter = "FullyQualifiedName~UnityArchiveRequestPipelineProductSmoke"
    }
    elseif ($Scenario -eq "unity-archive-provider-job-plan") {
        $TestFilter = "FullyQualifiedName~UnityArchiveProviderJobPlanProductSmoke"
    }
    elseif ($Scenario -eq "unity-archive-fulfillment-state") {
        $TestFilter = "FullyQualifiedName~UnityArchiveFulfillmentStateProductSmoke"
    }
elseif ($Scenario -eq "unity-archive-review-snapshot") {
	$TestFilter = "FullyQualifiedName~UnityArchiveReviewSnapshotProductSmoke"
	}
	elseif ($Scenario -eq "unity-archive-review-history") {
	$TestFilter = "FullyQualifiedName~UnityArchiveReviewHistoryProductSmoke"
	}
    elseif ($Scenario -eq "unity-archive-review-ui-readonly") {
        $TestFilter = "FullyQualifiedName~UnityArchiveReviewReadonlyProductSmoke"
    }
    elseif ($Scenario -eq "unity-archive-manual-provider-import") {
        $TestFilter = "FullyQualifiedName~UnityArchiveManualProviderImportProductSmoke"
    }
    elseif ($Scenario -eq "unity-archive-manual-import-workflow-ui") {
        $TestFilter = "FullyQualifiedName~UnityArchiveManualImportWorkflowUiProductSmoke"
    }
    elseif ($Scenario -eq "semantic-catalog-foundation") {
        $TestFilter = "FullyQualifiedName~SemanticCatalogFoundationProductSmoke"
    }
    elseif ($Scenario -eq "procedural-game-kernel") {
        $TestFilter = "FullyQualifiedName~ProceduralGameKernelProductSmoke"
    }
    elseif ($Scenario -eq "formula-effect-action-registry") {
        $TestFilter = "FullyQualifiedName~FormulaEffectActionRegistryProductSmoke"
    }
    elseif ($Scenario -eq "tiny-generated-runtime-loop") {
        $TestFilter = "FullyQualifiedName~TinyGeneratedRuntimeLoopProductSmoke"
    }
    elseif ($Scenario -eq "generated-package-mvp") {
        $TestFilter = "FullyQualifiedName~GeneratedPackageMvpProductSmoke"
    }
    elseif ($Scenario -eq "visible-generated-playable-preview") {
        $TestFilter = "FullyQualifiedName~VisibleGeneratedPlayablePreviewProductSmoke"
    }
    elseif ($Scenario -eq "one-click-generated-preview-workflow") {
        $TestFilter = "FullyQualifiedName~OneClickGeneratedPreviewWorkflowProductSmoke"
    }
    elseif ($Scenario -eq "generated-microgame-goal-loop") {
        $TestFilter = "FullyQualifiedName~GeneratedMicrogameGoalLoopProductSmoke"
    }
    elseif ($Scenario -eq "generated-microgame-challenge-loop") {
        $TestFilter = "FullyQualifiedName~GeneratedMicrogameChallengeLoopProductSmoke"
    }
    elseif ($Scenario -eq "generated-microgame-loop") {
        $TestFilter = "FullyQualifiedName~GeneratedMicrogameLoopProductSmoke"
    }
    elseif ($Scenario -eq "runtime-owned-goal-progress") {
        $TestFilter = "FullyQualifiedName~RuntimeOwnedGoalProgressProductSmoke"
    }
    elseif ($Scenario -eq "runtime-reward-challenge-state") {
        $TestFilter = "FullyQualifiedName~RuntimeRewardChallengeStateProductSmoke"
    }
    elseif ($Scenario -eq "runtime-backed-microgame-state") {
        $TestFilter = "FullyQualifiedName~RuntimeBackedMicrogameStateProductSmoke"
    }
    elseif ($Scenario -eq "generation-preset-options") {
        $TestFilter = "FullyQualifiedName~GenerationPresetOptionsProductSmoke"
    }
    elseif ($Scenario -eq "generated-microgame-variation") {
        $TestFilter = "FullyQualifiedName~GeneratedMicrogameVariationProductSmoke"
    }
    elseif ($Scenario -eq "extension-spine") {
        $TestFilter = "FullyQualifiedName~ExtensionSpineScenarioHarnessProductSmoke"
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
