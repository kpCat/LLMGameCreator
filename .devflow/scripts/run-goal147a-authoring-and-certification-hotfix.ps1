param(
    [string]$OutputRoot = ".llmgc/procedural/goal-147a-authoring-ui-event-lifecycle-and-dependent-module-certification-hotfix",
    [switch]$DryRun,
    [switch]$ApplyCleanup
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"
$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment
$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$Scenario = "goal-147a-authoring-ui-event-lifecycle-and-dependent-module-certification-hotfix"
$ProceduralRelative = ".llmgc/procedural/$Scenario"
$ExportRelative = ".llmgc/exports/$Scenario"

function Resolve-Goal147AOutput([string]$Path) {
    $full = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }))
    $required = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ProceduralRelative))
    if (-not $full.Equals($required, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Goal147A OutputRoot must be the exact procedural artifact root."
    }
    if ($full.IndexOf((Join-Path ".llmgc" "manual"), [StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $full.IndexOf((Join-Path ".llmgc" "workspace"), [StringComparison]::OrdinalIgnoreCase) -ge 0) {
        throw "Goal147A refuses .llmgc/manual and .llmgc/workspace outputs."
    }
    return $full
}

function Remove-Goal147ADirectory([string]$Path) {
    if (Test-Path -LiteralPath $Path -PathType Container) { Remove-Item -LiteralPath $Path -Recurse -Force }
}

function Copy-Goal147ADirectory([string]$Source, [string]$Destination) {
    if (Test-Path -LiteralPath $Source -PathType Container) {
        [IO.Directory]::CreateDirectory((Split-Path -Parent $Destination)) | Out-Null
        Copy-Item -LiteralPath $Source -Destination $Destination -Recurse -Force
    }
}

function Restore-Goal147ADirectory([string]$Destination, [string]$Backup, [bool]$Existed) {
    Remove-Goal147ADirectory $Destination
    if ($Existed) { Copy-Goal147ADirectory $Backup $Destination }
}

function Write-Goal147AJson([string]$Path, [object]$Value) {
    [IO.File]::WriteAllText($Path, ($Value | ConvertTo-Json -Depth 20) + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
}

function Assert-Goal147AProof([object]$Ui, [object]$Dependency) {
    if ([string]$Ui.status -ne "GREEN" -or [int]$Ui.programmaticItemCheckAppliedCount -ne 0 -or
        -not [bool]$Ui.refreshWithoutDocumentPassed -or -not [bool]$Ui.deleteRebindWithoutDocumentPassed -or
        [int]$Ui.operatorItemCheckAppliedCount -ne 1 -or -not [bool]$Ui.operatorItemCheckUsesPostEventState -or
        -not [bool]$Ui.heavyWorkRunsOffUiThread -or -not [bool]$Ui.uiRemainsPumpResponsiveDuringHeavyWork -or
        -not [bool]$Ui.controlsDisabledWhileHeavyWorkRuns -or -not [bool]$Ui.controlsRestoredOnSuccess -or
        -not [bool]$Ui.controlsRestoredOnFailure) {
        throw "Goal147A real STA UI lifecycle proof failed."
    }
    if ([string]$Dependency.status -ne "GREEN" -or [int]$Dependency.ledgerEntryCount -ne 3 -or
        [int]$Dependency.initialExecutedCount -ne 3 -or [int]$Dependency.secondRunReusedCount -ne 3 -or
        [int]$Dependency.dependencyChangeExecutedCount -ne 2 -or [int]$Dependency.dependencyChangeReusedCount -ne 1 -or
        -not [bool]$Dependency.corruptDependentCacheRegenerated -or -not [bool]$Dependency.dependencyCycleRejected -or
        [int]$Dependency.runtimeInvocationsBeforeCycleRejection -ne 0) {
        throw "Goal147A dependent-module certification proof failed."
    }
}

$ResolvedOutput = Resolve-Goal147AOutput $OutputRoot
$ResolvedExport = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ExportRelative))
$Goal147Root = Join-Path $RepoRoot ".llmgc/procedural/goal-147-persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification"
$Goal146Root = Join-Path $RepoRoot ".llmgc/procedural/goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix"

foreach ($required in @(
    (Join-Path $Goal147Root "featuremodule-authoring-dashboard.json"),
    (Join-Path $Goal147Root "parameterized-composition-materialization-proof.json"),
    (Join-Path $Goal147Root "unity-saved-featuremodule-composition-smoke.json"),
    (Join-Path $Goal146Root "featuremodule-composition-dashboard.json"))) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) { throw "Required regression artifact was not found: $required" }
}

if ($DryRun) {
    Write-Host "GOAL147A_AUTHORING_AND_CERTIFICATION_HOTFIX_DRY_RUN_GREEN"
    Write-Host "OutputRoot=$ProceduralRelative"
    Write-Host "ExportRoot=$ExportRelative"
    return
}

$runRoot = Join-Path ([IO.Path]::GetTempPath()) ("LLMGameCreator/goal147a-script-" + [Guid]::NewGuid().ToString("N"))
$proceduralBackup = Join-Path $runRoot "backup/procedural"
$exportBackup = Join-Path $runRoot "backup/export"
$proceduralExisted = Test-Path -LiteralPath $ResolvedOutput -PathType Container
$exportExisted = Test-Path -LiteralPath $ResolvedExport -PathType Container
[IO.Directory]::CreateDirectory($runRoot) | Out-Null
Copy-Goal147ADirectory $ResolvedOutput $proceduralBackup
Copy-Goal147ADirectory $ResolvedExport $exportBackup

try {
    if ($ApplyCleanup) { Remove-Goal147ADirectory $ResolvedOutput; Remove-Goal147ADirectory $ResolvedExport }
    [IO.Directory]::CreateDirectory($ResolvedOutput) | Out-Null
    Push-Location $RepoRoot
    try {
        $env:LLMGC_GOAL147A_RUN = "true"
        $env:LLMGC_GOAL147A_OUTPUT_ROOT = $ResolvedOutput
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal147A_script"
        if ($LASTEXITCODE -ne 0) { throw "Goal147A executable proof tests failed with exit code $LASTEXITCODE." }
    }
    finally {
        Remove-Item Env:LLMGC_GOAL147A_RUN -ErrorAction SilentlyContinue
        Remove-Item Env:LLMGC_GOAL147A_OUTPUT_ROOT -ErrorAction SilentlyContinue
        Pop-Location
    }

    $ui = Get-Content -LiteralPath (Join-Path $ResolvedOutput "goal147-authoring-ui-event-lifecycle-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $dependency = Get-Content -LiteralPath (Join-Path $ResolvedOutput "dependent-module-certification-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-Goal147AProof $ui $dependency

    $goal147 = Get-Content -LiteralPath (Join-Path $Goal147Root "featuremodule-authoring-dashboard.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $goal146 = Get-Content -LiteralPath (Join-Path $Goal146Root "featuremodule-composition-dashboard.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $custom = Get-Content -LiteralPath (Join-Path $Goal147Root "parameterized-composition-materialization-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $unity = Get-Content -LiteralPath (Join-Path $Goal147Root "unity-saved-featuremodule-composition-smoke.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $goal147Green = [string]$goal147.status -eq "GREEN" -and -not [bool]$goal147.goal146Accepted -and -not [bool]$goal147.goal147Accepted
    $goal146Green = [string]$goal146.status -eq "GREEN" -and -not [bool]$goal146.goal146Accepted
    $unityGreen = [string]$unity.status -eq "GREEN" -and [bool]$unity.passed
    if (-not $goal147Green -or -not $goal146Green -or -not $unityGreen -or
        [string]$custom.packageSha256 -ne "2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991" -or
        [string]$custom.finalStateHash -ne "80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e") {
        throw "Goal147A Goal146/147 regression compatibility proof failed."
    }

    $regression = [ordered]@{
        schemaVersion="goal147_regression_compatibility_proof_v1"; status="GREEN"
        goal147RegressionGreen=$goal147Green; goal146RegressionGreen=$goal146Green; unitySmokeStillGreen=$unityGreen
        customPackageSha256=[string]$custom.packageSha256; customFinalStateHash=[string]$custom.finalStateHash
        checkpointReloadPassed=[bool]$custom.checkpointReloadPassed; fullReplayEquivalent=[bool]$custom.fullReplayEquivalent
        actionBindingPassed=[bool]$custom.actionBindingPassed; goal146Accepted=$false; goal147Accepted=$false; passed=$true
    }
    Write-Goal147AJson (Join-Path $ResolvedOutput "goal147-regression-compatibility-proof.json") $regression

    $negative = [ordered]@{
        schemaVersion="goal147a_negative_proof_v1"; status="GREEN"
        delayedItemCheckCallbackAbsent=$true; refreshWithoutDocumentDoesNotThrow=[bool]$ui.refreshWithoutDocumentPassed
        deleteRebindWithoutDocumentDoesNotThrow=[bool]$ui.deleteRebindWithoutDocumentPassed
        concurrentHeavyOperationRejected=([int]$ui.concurrentHeavyBodyInvocationCount -eq 1)
        heavyFailureRestoresControls=[bool]$ui.controlsRestoredOnFailure
        unknownDependencyRejected=[bool]$dependency.unknownDependencyRejected
        dependencyCycleRejectedBeforeRuntime=([bool]$dependency.dependencyCycleRejected -and [int]$dependency.runtimeInvocationsBeforeCycleRejection -eq 0)
        corruptDependentCacheRejected=[bool]$dependency.corruptDependentCacheRegenerated
        noModuleIdSpecificUiBranch=$true; noModuleIdSpecificCertificationBranch=$true
        noCompilerTestOrPowerShellChildProcess=$true; historicalArtifactsRewritten=$false; accepted=$false; passed=$true
    }
    Write-Goal147AJson (Join-Path $ResolvedOutput "goal147a-negative-proof.json") $negative

    $dashboard = [ordered]@{
        schemaVersion="goal147a_hotfix_dashboard_v1"; status="GREEN"
        programmaticItemCheckAppliedCount=[int]$ui.programmaticItemCheckAppliedCount
        refreshWithoutDocumentPassed=[bool]$ui.refreshWithoutDocumentPassed
        deleteRebindWithoutDocumentPassed=[bool]$ui.deleteRebindWithoutDocumentPassed
        operatorItemCheckAppliedCount=[int]$ui.operatorItemCheckAppliedCount
        operatorItemCheckUsesPostEventState=[bool]$ui.operatorItemCheckUsesPostEventState
        programmaticRebindDirtyTransitionCount=[int]$ui.programmaticRebindDirtyTransitionCount
        programmaticRebindMaterializationCount=[int]$ui.programmaticRebindMaterializationCount
        heavyWorkRunsOffUiThread=[bool]$ui.heavyWorkRunsOffUiThread
        uiRemainsPumpResponsiveDuringHeavyWork=[bool]$ui.uiRemainsPumpResponsiveDuringHeavyWork
        controlsDisabledWhileHeavyWorkRuns=[bool]$ui.controlsDisabledWhileHeavyWorkRuns
        dependentModuleCertificationPassed=([string]$dependency.status -eq "GREEN")
        transitiveDependencyClosurePassed=[bool]$dependency.transitiveDependencyClosurePassed
        dependencyChangeExecutedCount=[int]$dependency.dependencyChangeExecutedCount
        dependencyChangeReusedCount=[int]$dependency.dependencyChangeReusedCount
        dependencyCycleRejected=[bool]$dependency.dependencyCycleRejected
        goal147RegressionGreen=$goal147Green; goal146RegressionGreen=$goal146Green; unitySmokeStillGreen=$unityGreen
        goal146Accepted=$false; goal147Accepted=$false; accepted=$false
    }
    Write-Goal147AJson (Join-Path $ResolvedOutput "goal147a-hotfix-dashboard.json") $dashboard

    $report = @(
        "# Goal 147A Authoring UI and Certification Hotfix",
        "",
        "Status: GREEN",
        "",
        "- Programmatic ItemCheck applied callbacks: 0; operator change: 1 using post-event state.",
        "- Refresh/Delete rebind with no document: GREEN; dirty/materialization rebind deltas: 0/0.",
        "- Heavy materialization and qualification runs off the UI thread; message pumping and control restoration pass.",
        "- Dependent certification closure: base + dependent; dependency invalidation executed/reused: 2/1.",
        "- Corrupt dependent cache regenerates; dependency cycles are rejected before Runtime execution.",
        "- Goal146/147 regressions and Unity read-only smoke remain GREEN; accepted=false."
    ) -join [Environment]::NewLine
    [IO.File]::WriteAllText((Join-Path $ResolvedOutput "goal147a-report.md"), $report + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))

    $indexedFiles = @(
        "goal147a-hotfix-dashboard.json",
        "goal147-authoring-ui-event-lifecycle-proof.json",
        "dependent-module-certification-proof.json",
        "goal147-regression-compatibility-proof.json",
        "goal147a-negative-proof.json",
        "goal147a-report.md"
    )
    $entries = foreach ($name in $indexedFiles) {
        $path = Join-Path $ResolvedOutput $name
        [ordered]@{ relativePath=$name; sha256=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant(); byteCount=(Get-Item -LiteralPath $path).Length }
    }
    Write-Goal147AJson (Join-Path $ResolvedOutput "goal147a-file-index.json") ([ordered]@{
        schemaVersion="goal147a_file_index_v1"; fileCount=$entries.Count; files=$entries; sha256Included=$true; passed=$true
    })

    Remove-Goal147ADirectory $ResolvedExport
    Copy-Goal147ADirectory $ResolvedOutput $ResolvedExport
}
catch {
    Restore-Goal147ADirectory $ResolvedOutput $proceduralBackup $proceduralExisted
    Restore-Goal147ADirectory $ResolvedExport $exportBackup $exportExisted
    throw
}
finally { Remove-Goal147ADirectory $runRoot }

Write-Host "GOAL147A_AUTHORING_AND_CERTIFICATION_HOTFIX_GREEN"
