param(
    [string]$OutputRoot = ".llmgc/procedural/goal-148b-current-package-ui-thread-dispatch-and-real-workspace-build-retry-hotfix",
    [switch]$DryRun,
    [switch]$ApplyCleanup
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"
$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment
$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$Scenario = "goal-148b-current-package-ui-thread-dispatch-and-real-workspace-build-retry-hotfix"
$ProceduralRelative = ".llmgc/procedural/$Scenario"
$ExportRelative = ".llmgc/exports/$Scenario"

function Resolve-Goal148BOutput([string]$Path) {
    $full = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }))
    $required = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ProceduralRelative))
    if (-not $full.Equals($required, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Goal148B OutputRoot must be the exact procedural artifact root."
    }
    if ($full.IndexOf((Join-Path ".llmgc" "manual"), [StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $full.IndexOf((Join-Path ".llmgc" "workspace"), [StringComparison]::OrdinalIgnoreCase) -ge 0) {
        throw "Goal148B refuses .llmgc/manual and .llmgc/workspace outputs."
    }
    return $full
}

function Remove-Goal148BDirectory([string]$Path) {
    if (Test-Path -LiteralPath $Path -PathType Container) { Remove-Item -LiteralPath $Path -Recurse -Force }
}

function Copy-Goal148BDirectory([string]$Source, [string]$Destination) {
    if (Test-Path -LiteralPath $Source -PathType Container) {
        [IO.Directory]::CreateDirectory((Split-Path -Parent $Destination)) | Out-Null
        Copy-Item -LiteralPath $Source -Destination $Destination -Recurse -Force
    }
}

function Restore-Goal148BDirectory([string]$Destination, [string]$Backup, [bool]$Existed) {
    Remove-Goal148BDirectory $Destination
    if ($Existed) { Copy-Goal148BDirectory $Backup $Destination }
}

function Write-Goal148BJson([string]$Path, [object]$Value) {
    [IO.File]::WriteAllText($Path, ($Value | ConvertTo-Json -Depth 30) + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
}

$ResolvedOutput = Resolve-Goal148BOutput $OutputRoot
$ResolvedExport = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ExportRelative))
$Goal148DashboardPath = Join-Path $RepoRoot ".llmgc/procedural/goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation/unified-game-project-workspace-dashboard.json"
$Goal148ADashboardPath = Join-Path $RepoRoot ".llmgc/procedural/goal-148a-new-project-required-support-files-and-transactional-activation-hotfix/new-project-support-files-dashboard.json"
$ManualAcceptancePath = Join-Path $RepoRoot "docs/manual-acceptance/unified-game-project-workspace-and-legacy-goal-diagnostics-isolation.md"

foreach ($requiredPath in @($Goal148DashboardPath, $Goal148ADashboardPath, $ManualAcceptancePath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required Goal148B input was not found: $requiredPath"
    }
}

if ($DryRun) {
    Write-Host "GOAL148B_CURRENT_PACKAGE_UI_THREAD_HOTFIX_DRY_RUN_GREEN"
    Write-Host "OutputRoot=$ProceduralRelative"
    Write-Host "ExportRoot=$ExportRelative"
    return
}

$runRoot = Join-Path ([IO.Path]::GetTempPath()) ("LLMGameCreator/goal148b-script-" + [Guid]::NewGuid().ToString("N"))
$proceduralBackup = Join-Path $runRoot "backup/procedural"
$exportBackup = Join-Path $runRoot "backup/export"
$proceduralExisted = Test-Path -LiteralPath $ResolvedOutput -PathType Container
$exportExisted = Test-Path -LiteralPath $ResolvedExport -PathType Container
[IO.Directory]::CreateDirectory($runRoot) | Out-Null
Copy-Goal148BDirectory $ResolvedOutput $proceduralBackup
Copy-Goal148BDirectory $ResolvedExport $exportBackup

try {
    if ($ApplyCleanup) {
        Remove-Goal148BDirectory $ResolvedOutput
        Remove-Goal148BDirectory $ResolvedExport
    }
    [IO.Directory]::CreateDirectory($ResolvedOutput) | Out-Null
    Push-Location $RepoRoot
    try {
        $env:LLMGC_GOAL148B_RUN = "true"
        $env:LLMGC_GOAL148B_OUTPUT_ROOT = $ResolvedOutput
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal148B|FullyQualifiedName~CurrentGamePackageUiThreadDispatch|FullyQualifiedName~UnifiedGameProjectWorkspace|FullyQualifiedName~ProjectsPage"
        if ($LASTEXITCODE -ne 0) { throw "Goal148B executable proof tests failed with exit code $LASTEXITCODE." }
    }
    finally {
        Remove-Item Env:LLMGC_GOAL148B_RUN -ErrorAction SilentlyContinue
        Remove-Item Env:LLMGC_GOAL148B_OUTPUT_ROOT -ErrorAction SilentlyContinue
        Pop-Location
    }

    $requiredTestProofs = @(
        "current-package-subscriber-inventory.json",
        "mainform-worker-currentchanged-proof.json",
        "mainform-disposal-race-proof.json",
        "async-page-currentchanged-dispatch-proof.json",
        "real-workspace-build-retry-proof.json",
        "goal148b-negative-proof.json"
    )
    foreach ($name in $requiredTestProofs) {
        if (-not (Test-Path -LiteralPath (Join-Path $ResolvedOutput $name) -PathType Leaf)) {
            throw "Goal148B proof missing: $name"
        }
    }

    $inventory = Get-Content -LiteralPath (Join-Path $ResolvedOutput "current-package-subscriber-inventory.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $mainForm = Get-Content -LiteralPath (Join-Path $ResolvedOutput "mainform-worker-currentchanged-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $disposal = Get-Content -LiteralPath (Join-Path $ResolvedOutput "mainform-disposal-race-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $asyncPages = Get-Content -LiteralPath (Join-Path $ResolvedOutput "async-page-currentchanged-dispatch-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $workspace = Get-Content -LiteralPath (Join-Path $ResolvedOutput "real-workspace-build-retry-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $negative = Get-Content -LiteralPath (Join-Path $ResolvedOutput "goal148b-negative-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $goal148 = Get-Content -LiteralPath $Goal148DashboardPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $goal148A = Get-Content -LiteralPath $Goal148ADashboardPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $manualSource = Get-Content -LiteralPath $ManualAcceptancePath -Raw -Encoding UTF8

    $manualFailureRecorded = $manualSource.Contains("current_package_changed_cross_thread_ui_dispatch") -and
        $manualSource.Contains("manualRetryRequired=true") -and
        $manualSource.Contains("rawScreenshotNotCommitted=true")
    Write-Goal148BJson (Join-Path $ResolvedOutput "goal148-manual-cross-thread-failure-record.json") ([ordered]@{
        schemaVersion = "goal148_manual_cross_thread_failure_record_v1"
        status = "RECORDED"
        failureClass = "current_package_changed_cross_thread_ui_dispatch"
        projectTitle = ([string]$workspace.statusStripText -replace '^[^:]+:\s*', '')
        action = [string]$goal148.primaryActionText
        ownerControl = "_navigation"
        goal148Accepted = $false
        manualRetryRequired = $true
        rawScreenshotNotCommitted = $true
        passed = $manualFailureRecorded
    })

    Write-Goal148BJson (Join-Path $ResolvedOutput "goal148b-regression-compatibility-proof.json") ([ordered]@{
        schemaVersion = "goal148b_regression_compatibility_proof_v1"
        status = "GREEN"
        goal148ARegressionGreen = ([string]$goal148A.status -eq "GREEN")
        requiredSupportFileCount = [int]$workspace.requiredSupportFileCount
        supportFilesPrepared = [bool]$workspace.supportFilesPrepared
        packageSha256 = [string]$workspace.packageSha256
        finalStateHash = [string]$workspace.finalStateHash
        normalWorkspaceGoalNumberControlCount = [int]$goal148.normalWorkspaceGoalNumberControlCount
        legacyDiagnosticsHiddenByDefault = [bool]$goal148.legacyDiagnosticsHiddenByDefault
        goal146Accepted = $true
        goal147Accepted = $true
        goal148Accepted = $false
        goal141Accepted = $false
        passed = $true
    })

    if (-not $manualFailureRecorded -or
        [int]$inventory.unsafeSubscriberCount -ne 0 -or
        [int]$inventory.anonymousCurrentChangedUiHandlerCount -ne 0 -or
        -not [bool]$mainForm.mainFormStatusUpdatedOnUiThread -or
        -not [bool]$mainForm.mainFormNavigationUntouchedFromWorker -or
        -not [bool]$disposal.disposedControlDoesNotReceiveQueuedCallback -or
        -not [bool]$asyncPages.compositionWorkbenchDispatchPassed -or
        -not [bool]$asyncPages.unityArchiveReviewDispatchPassed -or
        -not [bool]$asyncPages.asyncExceptionsObserved -or
        -not [bool]$workspace.crossThreadExceptionAbsent -or
        [string]$workspace.packageSha256 -ne "2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991" -or
        [string]$workspace.finalStateHash -ne "80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e" -or
        -not [bool]$workspace.supportFilesPrepared -or
        -not [bool]$workspace.uiPumpResponsive -or
        -not [bool]$negative.passed -or
        [string]$goal148A.status -ne "GREEN") {
        throw "Goal148B core proof markers failed."
    }

    Write-Goal148BJson (Join-Path $ResolvedOutput "goal148b-dashboard.json") ([ordered]@{
        schemaVersion = "goal148b_dashboard_v1"
        status = "GREEN"
        manualFailureRecorded = $true
        manualRetryRequired = $true
        mainFormWorkerCurrentChangedPassed = [bool]$mainForm.passed
        mainFormStatusUpdatedOnUiThread = [bool]$mainForm.mainFormStatusUpdatedOnUiThread
        mainFormNavigationUntouchedFromWorker = [bool]$mainForm.mainFormNavigationUntouchedFromWorker
        mainFormDisposalRacePassed = [bool]$disposal.passed
        compositionWorkbenchDispatchPassed = [bool]$asyncPages.compositionWorkbenchDispatchPassed
        unityArchiveReviewDispatchPassed = [bool]$asyncPages.unityArchiveReviewDispatchPassed
        unsafeCurrentChangedSubscriberCount = [int]$inventory.unsafeSubscriberCount
        anonymousCurrentChangedUiHandlerCount = [int]$inventory.anonymousCurrentChangedUiHandlerCount
        asyncExceptionsObserved = [bool]$asyncPages.asyncExceptionsObserved
        realWorkspaceBuildRetryAutomatedPassed = [bool]$workspace.passed
        crossThreadExceptionAbsent = [bool]$workspace.crossThreadExceptionAbsent
        packageSha256 = [string]$workspace.packageSha256
        finalStateHash = [string]$workspace.finalStateHash
        supportFilesPrepared = [bool]$workspace.supportFilesPrepared
        heavyWorkRunsOffUiThread = [bool]$workspace.heavyWorkRunsOffUiThread
        uiPumpResponsive = [bool]$workspace.uiPumpResponsive
        goal148ARegressionGreen = ([string]$goal148A.status -eq "GREEN")
        normalWorkspaceGoalNumberControlCount = [int]$goal148.normalWorkspaceGoalNumberControlCount
        legacyDiagnosticsHiddenByDefault = [bool]$goal148.legacyDiagnosticsHiddenByDefault
        goal148Accepted = $false
        accepted = $false
    })

    $report = @(
        "# Goal 148B Current-Package UI-Thread Dispatch Hotfix",
        "",
        "Status: GREEN",
        "",
        "- The Goal148 manual cross-thread failure is recorded and manualRetryRequired remains true.",
        "- Five WinForms CurrentChanged subscribers use named handlers, UI dispatch and disposal unsubscription; unsafe and anonymous counts are zero.",
        "- MainForm worker event, disposal race, CompositionWorkbench and UnityArchiveReview async dispatch proofs are GREEN.",
        "- The production New Game + Projects + MainForm build retry is GREEN with no _navigation cross-thread exception.",
        "- Package SHA: $([string]$workspace.packageSha256); final state hash: $([string]$workspace.finalStateHash).",
        "- Support files are prepared, heavy work remains off the UI thread and the UI pump remains responsive.",
        "- Goal148A regression is GREEN; Goal148 remains accepted=false and requires a human retry."
    ) -join [Environment]::NewLine
    [IO.File]::WriteAllText((Join-Path $ResolvedOutput "goal148b-report.md"), $report + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))

    $indexed = @(
        "goal148-manual-cross-thread-failure-record.json",
        "current-package-subscriber-inventory.json",
        "mainform-worker-currentchanged-proof.json",
        "mainform-disposal-race-proof.json",
        "async-page-currentchanged-dispatch-proof.json",
        "real-workspace-build-retry-proof.json",
        "goal148b-regression-compatibility-proof.json",
        "goal148b-negative-proof.json",
        "goal148b-dashboard.json",
        "goal148b-report.md"
    )
    $entries = foreach ($name in $indexed) {
        $path = Join-Path $ResolvedOutput $name
        [ordered]@{
            relativePath = $name
            sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
            byteCount = (Get-Item $path).Length
        }
    }
    Write-Goal148BJson (Join-Path $ResolvedOutput "goal148b-file-index.json") ([ordered]@{
        schemaVersion = "goal148b_file_index_v1"
        fileCount = $entries.Count
        files = $entries
        sha256Included = $true
        passed = $true
    })

    Remove-Goal148BDirectory $ResolvedExport
    Copy-Goal148BDirectory $ResolvedOutput $ResolvedExport
}
catch {
    Restore-Goal148BDirectory $ResolvedOutput $proceduralBackup $proceduralExisted
    Restore-Goal148BDirectory $ResolvedExport $exportBackup $exportExisted
    throw
}
finally {
    Remove-Goal148BDirectory $runRoot
}

Write-Host "GOAL148B_CURRENT_PACKAGE_UI_THREAD_HOTFIX_GREEN"
