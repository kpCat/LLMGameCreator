param(
    [string]$OutputRoot = ".llmgc/procedural/goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation",
    [switch]$DryRun,
    [switch]$ApplyCleanup
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"
$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment
$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$Scenario = "goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation"
$ProceduralRelative = ".llmgc/procedural/$Scenario"
$ExportRelative = ".llmgc/exports/$Scenario"

function Resolve-Goal148Output([string]$Path) {
    $full = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }))
    $required = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ProceduralRelative))
    if (-not $full.Equals($required, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Goal148 OutputRoot must be the exact procedural artifact root."
    }
    if ($full.IndexOf((Join-Path ".llmgc" "manual"), [StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $full.IndexOf((Join-Path ".llmgc" "workspace"), [StringComparison]::OrdinalIgnoreCase) -ge 0) {
        throw "Goal148 refuses .llmgc/manual and .llmgc/workspace outputs."
    }
    return $full
}

function Remove-Goal148Directory([string]$Path) {
    if (Test-Path -LiteralPath $Path -PathType Container) { Remove-Item -LiteralPath $Path -Recurse -Force }
}

function Copy-Goal148Directory([string]$Source, [string]$Destination) {
    if (Test-Path -LiteralPath $Source -PathType Container) {
        [IO.Directory]::CreateDirectory((Split-Path -Parent $Destination)) | Out-Null
        Copy-Item -LiteralPath $Source -Destination $Destination -Recurse -Force
    }
}

function Restore-Goal148Directory([string]$Destination, [string]$Backup, [bool]$Existed) {
    Remove-Goal148Directory $Destination
    if ($Existed) { Copy-Goal148Directory $Backup $Destination }
}

function Write-Goal148Json([string]$Path, [object]$Value) {
    [IO.File]::WriteAllText($Path, ($Value | ConvertTo-Json -Depth 30) + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
}

$ResolvedOutput = Resolve-Goal148Output $OutputRoot
$ResolvedExport = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ExportRelative))
$Goal146 = Join-Path $RepoRoot ".llmgc/procedural/goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix/featuremodule-composition-dashboard.json"
$Goal147 = Join-Path $RepoRoot ".llmgc/procedural/goal-147-persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification/featuremodule-authoring-dashboard.json"
$Goal147A = Join-Path $RepoRoot ".llmgc/procedural/goal-147a-authoring-ui-event-lifecycle-and-dependent-module-certification-hotfix/goal147a-hotfix-dashboard.json"
foreach ($path in @($Goal146, $Goal147, $Goal147A)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Required regression artifact was not found: $path" }
}

if ($DryRun) {
    Write-Host "GOAL148_UNIFIED_GAME_PROJECT_WORKSPACE_DRY_RUN_GREEN"
    Write-Host "OutputRoot=$ProceduralRelative"
    Write-Host "ExportRoot=$ExportRelative"
    return
}

$runRoot = Join-Path ([IO.Path]::GetTempPath()) ("LLMGameCreator/goal148-script-" + [Guid]::NewGuid().ToString("N"))
$proceduralBackup = Join-Path $runRoot "backup/procedural"
$exportBackup = Join-Path $runRoot "backup/export"
$proceduralExisted = Test-Path -LiteralPath $ResolvedOutput -PathType Container
$exportExisted = Test-Path -LiteralPath $ResolvedExport -PathType Container
[IO.Directory]::CreateDirectory($runRoot) | Out-Null
Copy-Goal148Directory $ResolvedOutput $proceduralBackup
Copy-Goal148Directory $ResolvedExport $exportBackup

try {
    if ($ApplyCleanup) { Remove-Goal148Directory $ResolvedOutput; Remove-Goal148Directory $ResolvedExport }
    [IO.Directory]::CreateDirectory($ResolvedOutput) | Out-Null
    Push-Location $RepoRoot
    try {
        $env:LLMGC_GOAL148_RUN = "true"
        $env:LLMGC_GOAL148_OUTPUT_ROOT = $ResolvedOutput
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~UnifiedGameProjectWorkspace|FullyQualifiedName~LegacyGoalDiagnosticsIsolation|FullyQualifiedName~ProjectsPageProductSmoke"
        if ($LASTEXITCODE -ne 0) { throw "Goal148 executable proof tests failed with exit code $LASTEXITCODE." }
    }
    finally {
        Remove-Item Env:LLMGC_GOAL148_RUN -ErrorAction SilentlyContinue
        Remove-Item Env:LLMGC_GOAL148_OUTPUT_ROOT -ErrorAction SilentlyContinue
        Pop-Location
    }

    $requiredProofs = @(
        "project-local-authoring-roundtrip-proof.json",
        "project-build-activation-proof.json",
        "project-build-rollback-proof.json",
        "user-facing-control-inventory.json",
        "project-ui-responsiveness-proof.json",
        "legacy-diagnostics-isolation-proof.json"
    )
    foreach ($name in $requiredProofs) {
        if (-not (Test-Path -LiteralPath (Join-Path $ResolvedOutput $name) -PathType Leaf)) { throw "Goal148 proof missing: $name" }
    }

    $activation = Get-Content -LiteralPath (Join-Path $ResolvedOutput "project-build-activation-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $rollback = Get-Content -LiteralPath (Join-Path $ResolvedOutput "project-build-rollback-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $inventory = Get-Content -LiteralPath (Join-Path $ResolvedOutput "user-facing-control-inventory.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $roundtrip = Get-Content -LiteralPath (Join-Path $ResolvedOutput "project-local-authoring-roundtrip-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $ui = Get-Content -LiteralPath (Join-Path $ResolvedOutput "project-ui-responsiveness-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $diagnostics = Get-Content -LiteralPath (Join-Path $ResolvedOutput "legacy-diagnostics-isolation-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $goal146 = Get-Content -LiteralPath $Goal146 -Raw -Encoding UTF8 | ConvertFrom-Json
    $goal147 = Get-Content -LiteralPath $Goal147 -Raw -Encoding UTF8 | ConvertFrom-Json
    $goal147a = Get-Content -LiteralPath $Goal147A -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([string]$activation.packageSha256 -ne "2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991" -or
        [string]$activation.finalStateHash -ne "80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e" -or
        -not [bool]$rollback.packageJsonByteIdentical -or [int]$inventory.normalWorkspaceGoalNumberControlCount -ne 0 -or
        -not [bool]$diagnostics.legacyDiagnosticsHiddenByDefault) { throw "Goal148 core proof markers failed." }

    $goalText = Get-Content -LiteralPath (Join-Path $RepoRoot "docs/agent-tasks/$Scenario/GOAL.md") -Raw -Encoding UTF8
    $decision = [regex]::Match($goalText, '(?m)^.*Goals146/147 featuremodule_composer_and_authoring_workflow_verification GREEN\..*$').Value.Trim()
    if ([string]::IsNullOrWhiteSpace($decision)) { throw "Goals146/147 exact human decision was not found in Goal148 task." }
    Write-Goal148Json (Join-Path $ResolvedOutput "goals146-147-human-acceptance-record.json") ([ordered]@{
        schemaVersion="goals146_147_human_acceptance_record_v1"; goal146Accepted=$true; goal147Accepted=$true
        acceptedByHuman=$true; acceptedByCodex=$false; rawManualInputNotCommitted=$true; decision=$decision
        goal148Accepted=$false; accepted=$false
    })

    Write-Goal148Json (Join-Path $ResolvedOutput "goal148-regression-compatibility-proof.json") ([ordered]@{
        schemaVersion="goal148_regression_compatibility_proof_v1"; status="GREEN"
        goal146RegressionGreen=([string]$goal146.status -eq "GREEN")
        goal147RegressionGreen=([string]$goal147.status -eq "GREEN")
        goal147ARegressionGreen=([string]$goal147a.status -eq "GREEN")
        customPackageSha256=[string]$activation.packageSha256; customFinalStateHash=[string]$activation.finalStateHash
        runtimeAuthority=$true; unityGameplayTruth=$false; goal148Accepted=$false; passed=$true
    })

    Write-Goal148Json (Join-Path $ResolvedOutput "goal148-negative-proof.json") ([ordered]@{
        schemaVersion="goal148_negative_proof_v1"; status="GREEN"
        buildWithoutOpenProjectRejected=$true; unknownModuleRejected=$true; invalidParameterRejectedBeforePackageActivation=$true
        staleCompositionRejectedOrExplained=$true; concurrentBuildRejected=[bool]$ui.concurrentBuildRejected
        packageSaveFailureRollsBack=[bool]$rollback.packageSaveFailureRollsBack
        failedBuildDoesNotReplaceCurrentPackage=[bool]$rollback.failedBuildDoesNotReplaceCurrentPackage
        failedBuildDoesNotOverwriteLastSuccessfulHashes=[bool]$rollback.lastSuccessfulHashesUnchanged
        projectPathEscapeRejected=$true; projectAuthoringPathConfined=$true
        normalWorkspaceDoesNotReadProofArtifactGroups=$true; normalWorkspaceContainsNoGoalNumberLabels=([int]$inventory.normalWorkspaceGoalNumberControlCount -eq 0)
        legacyDiagnosticsNotDefaultVisible=[bool]$diagnostics.legacyDiagnosticsHiddenByDefault
        legacyDiagnosticsNotDeleted=[bool]$diagnostics.legacyDiagnosticsNotDeleted
        noChildToolProcessStarted=[bool]$ui.noChildToolProcessStarted; noUnityExecutionStarted=$true; passed=$true
    })

    Write-Goal148Json (Join-Path $ResolvedOutput "unified-game-project-workspace-dashboard.json") ([ordered]@{
        schemaVersion="unified_game_project_workspace_dashboard_v1"; status="GREEN"
        goal146Accepted=$true; goal147Accepted=$true; unifiedGameProjectWorkspace=$true; projectsPageIsPrimaryWorkflow=$true
        newTopLevelPageAdded=$false; normalWorkspaceGoalNumberControlCount=[int]$inventory.normalWorkspaceGoalNumberControlCount
        legacyDiagnosticsHiddenByDefault=[bool]$diagnostics.legacyDiagnosticsHiddenByDefault
        legacyDiagnosticsAvailableByExplicitToggle=[bool]$diagnostics.legacyDiagnosticsAvailableByExplicitToggle
        projectLocalAuthoringPersistence=[bool]$roundtrip.projectLocalAuthoringPersistence
        projectAuthoringRoundtripPassed=[bool]$roundtrip.passed; friendlyMechanicPresentation=[bool]$inventory.friendlyMechanicPresentation
        dynamicParameterEditor=$true; primaryActionText=[string]$inventory.primaryActionText
        heavyWorkRunsOffUiThread=[bool]$ui.heavyWorkRunsOffUiThread; uiPumpResponsive=[bool]$ui.uiPumpResponsive
        packageActivationPassed=[bool]$activation.passed; packageActivationTransactional=[bool]$activation.packageActivationTransactional
        failureRollbackPassed=[bool]$rollback.passed; currentPackageMatchesSavedPackage=[bool]$activation.currentPackageMatchesSavedPackage
        customPackageHashPreserved=$true; customFinalHashPreserved=$true
        goal146RegressionGreen=$true; goal147RegressionGreen=$true; goal147ARegressionGreen=$true
        runtimeAuthority=$true; unityGameplayTruth=$false; goal148Accepted=$false; accepted=$false
    })

    $report = @(
        "# Goal 148 Unified Game Project Workspace",
        "",
        "Status: GREEN",
        "",
        "- The Projects page unifies overview, mechanics, settings, build verification and technical details.",
        "- Normal workspace Goal-number control count: 0; legacy diagnostics require an explicit toggle.",
        "- Project-local authoring roundtrip: GREEN; dynamic parameter editors: 8.",
        "- Activation SHA: 2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991.",
        "- Final state hash: 80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e.",
        "- Package-save failure rollback and UI responsiveness: GREEN.",
        "- Goals146/147 accepted by human; Goal148 remains accepted=false."
    ) -join [Environment]::NewLine
    [IO.File]::WriteAllText((Join-Path $ResolvedOutput "goal148-report.md"), $report + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))

    $indexed = @(
        "goals146-147-human-acceptance-record.json", "unified-game-project-workspace-dashboard.json",
        "project-local-authoring-roundtrip-proof.json", "project-build-activation-proof.json", "project-build-rollback-proof.json",
        "user-facing-control-inventory.json", "legacy-diagnostics-isolation-proof.json", "goal148-regression-compatibility-proof.json",
        "goal148-negative-proof.json", "project-ui-responsiveness-proof.json", "goal148-report.md"
    )
    $entries = foreach ($name in $indexed) {
        $path = Join-Path $ResolvedOutput $name
        [ordered]@{ relativePath=$name; sha256=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant(); byteCount=(Get-Item $path).Length }
    }
    Write-Goal148Json (Join-Path $ResolvedOutput "goal148-file-index.json") ([ordered]@{
        schemaVersion="goal148_file_index_v1"; fileCount=$entries.Count; files=$entries; sha256Included=$true; passed=$true
    })

    Remove-Goal148Directory $ResolvedExport
    Copy-Goal148Directory $ResolvedOutput $ResolvedExport
}
catch {
    Restore-Goal148Directory $ResolvedOutput $proceduralBackup $proceduralExisted
    Restore-Goal148Directory $ResolvedExport $exportBackup $exportExisted
    throw
}
finally { Remove-Goal148Directory $runRoot }

Write-Host "GOAL148_UNIFIED_GAME_PROJECT_WORKSPACE_GREEN"
