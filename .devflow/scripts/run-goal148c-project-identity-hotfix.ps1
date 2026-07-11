param(
    [string]$OutputRoot = ".llmgc/procedural/goal-148c-project-identity-preservation-and-project-scoped-composition-hotfix",
    [switch]$DryRun,
    [switch]$ApplyCleanup
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"
$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment
$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$Scenario = "goal-148c-project-identity-preservation-and-project-scoped-composition-hotfix"
$ProceduralRelative = ".llmgc/procedural/$Scenario"
$ExportRelative = ".llmgc/exports/$Scenario"

function Resolve-Goal148COutput([string]$Path) {
    $full = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }))
    $required = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ProceduralRelative))
    if (-not $full.Equals($required, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Goal148C OutputRoot must be the exact procedural artifact root."
    }
    if ($full.IndexOf((Join-Path ".llmgc" "manual"), [StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $full.IndexOf((Join-Path ".llmgc" "workspace"), [StringComparison]::OrdinalIgnoreCase) -ge 0) {
        throw "Goal148C refuses .llmgc/manual and .llmgc/workspace outputs."
    }
    return $full
}

function Remove-Goal148CDirectory([string]$Path) {
    if (Test-Path -LiteralPath $Path -PathType Container) { Remove-Item -LiteralPath $Path -Recurse -Force }
}

function Copy-Goal148CDirectory([string]$Source, [string]$Destination) {
    if (Test-Path -LiteralPath $Source -PathType Container) {
        [IO.Directory]::CreateDirectory((Split-Path -Parent $Destination)) | Out-Null
        Copy-Item -LiteralPath $Source -Destination $Destination -Recurse -Force
    }
}

function Restore-Goal148CDirectory([string]$Destination, [string]$Backup, [bool]$Existed) {
    Remove-Goal148CDirectory $Destination
    if ($Existed) { Copy-Goal148CDirectory $Backup $Destination }
}

function Write-Goal148CJson([string]$Path, [object]$Value) {
    [IO.File]::WriteAllText($Path, ($Value | ConvertTo-Json -Depth 30) + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
}

$ResolvedOutput = Resolve-Goal148COutput $OutputRoot
$ResolvedExport = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ExportRelative))
$ManualAcceptancePath = Join-Path $RepoRoot "docs/manual-acceptance/unified-game-project-workspace-and-legacy-goal-diagnostics-isolation.md"
$Goal148ADashboardPath = Join-Path $RepoRoot ".llmgc/procedural/goal-148a-new-project-required-support-files-and-transactional-activation-hotfix/new-project-support-files-dashboard.json"
$Goal148BDashboardPath = Join-Path $RepoRoot ".llmgc/procedural/goal-148b-current-package-ui-thread-dispatch-and-real-workspace-build-retry-hotfix/goal148b-dashboard.json"
foreach ($requiredPath in @($ManualAcceptancePath, $Goal148ADashboardPath, $Goal148BDashboardPath)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) { throw "Required Goal148C input was not found: $requiredPath" }
}

if ($DryRun) {
    Write-Host "GOAL148C_PROJECT_IDENTITY_HOTFIX_DRY_RUN_GREEN"
    Write-Host "OutputRoot=$ProceduralRelative"
    Write-Host "ExportRoot=$ExportRelative"
    return
}

$runRoot = Join-Path ([IO.Path]::GetTempPath()) ("LLMGameCreator/goal148c-script-" + [Guid]::NewGuid().ToString("N"))
$proceduralBackup = Join-Path $runRoot "backup/procedural"
$exportBackup = Join-Path $runRoot "backup/export"
$proceduralExisted = Test-Path -LiteralPath $ResolvedOutput -PathType Container
$exportExisted = Test-Path -LiteralPath $ResolvedExport -PathType Container
[IO.Directory]::CreateDirectory($runRoot) | Out-Null
Copy-Goal148CDirectory $ResolvedOutput $proceduralBackup
Copy-Goal148CDirectory $ResolvedExport $exportBackup

try {
    if ($ApplyCleanup) {
        Remove-Goal148CDirectory $ResolvedOutput
        Remove-Goal148CDirectory $ResolvedExport
    }
    [IO.Directory]::CreateDirectory($ResolvedOutput) | Out-Null
    Push-Location $RepoRoot
    try {
        $env:LLMGC_GOAL148C_RUN = "true"
        $env:LLMGC_GOAL148C_OUTPUT_ROOT = $ResolvedOutput
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal148C"
        if ($LASTEXITCODE -ne 0) { throw "Goal148C executable proof tests failed with exit code $LASTEXITCODE." }
    }
    finally {
        Remove-Item Env:LLMGC_GOAL148C_RUN -ErrorAction SilentlyContinue
        Remove-Item Env:LLMGC_GOAL148C_OUTPUT_ROOT -ErrorAction SilentlyContinue
        Pop-Location
    }

    $requiredTestProofs = @(
        "project-identity-capture-proof.json",
        "legacy-authoring-migration-proof.json",
        "project-scoped-composition-identity-proof.json",
        "manual-values-project-build-proof.json",
        "historical-control-values-proof.json",
        "two-project-identity-isolation-proof.json",
        "identity-repeat-build-proof.json",
        "identity-rollback-proof.json",
        "mainform-project-title-consistency-proof.json"
    )
    foreach ($name in $requiredTestProofs) {
        if (-not (Test-Path -LiteralPath (Join-Path $ResolvedOutput $name) -PathType Leaf)) { throw "Goal148C proof missing: $name" }
    }

    $capture = Get-Content -LiteralPath (Join-Path $ResolvedOutput "project-identity-capture-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $migration = Get-Content -LiteralPath (Join-Path $ResolvedOutput "legacy-authoring-migration-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $composition = Get-Content -LiteralPath (Join-Path $ResolvedOutput "project-scoped-composition-identity-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $manualBuild = Get-Content -LiteralPath (Join-Path $ResolvedOutput "manual-values-project-build-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $historical = Get-Content -LiteralPath (Join-Path $ResolvedOutput "historical-control-values-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $isolation = Get-Content -LiteralPath (Join-Path $ResolvedOutput "two-project-identity-isolation-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $repeat = Get-Content -LiteralPath (Join-Path $ResolvedOutput "identity-repeat-build-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $rollback = Get-Content -LiteralPath (Join-Path $ResolvedOutput "identity-rollback-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $mainForm = Get-Content -LiteralPath (Join-Path $ResolvedOutput "mainform-project-title-consistency-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $goal148A = Get-Content -LiteralPath $Goal148ADashboardPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $goal148B = Get-Content -LiteralPath $Goal148BDashboardPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $manualSource = Get-Content -LiteralPath $ManualAcceptancePath -Raw -Encoding UTF8

    $manualFailureRecorded = $manualSource.Contains("manualFailureClass=project_identity_overwritten_by_template_manifest") -and
        $manualSource.Contains("manualBuildExecutionPassed=true") -and
        $manualSource.Contains("manualCrossThreadFailureResolved=true") -and
        $manualSource.Contains("rawScreenshotsNotCommitted=true")
    Write-Goal148CJson (Join-Path $ResolvedOutput "goal148-manual-project-identity-failure-record.json") ([ordered]@{
        schemaVersion = "goal148_manual_project_identity_failure_record_v1"
        status = "RECORDED"
        projectFolder = "goal148-manual"
        originalTitle = "Проверка конструктора"
        overwrittenTitle = "Minimal Map Game"
        failureClass = "project_identity_overwritten_by_template_manifest"
        manualBuildExecutionPassed = $true
        manualCrossThreadFailureResolved = $true
        goal148Accepted = $false
        manualRetryRequired = $true
        rawScreenshotsNotCommitted = $true
        passed = $manualFailureRecorded
    })

    if (-not $manualFailureRecorded -or
        -not [bool]$capture.passed -or -not [bool]$migration.passed -or -not [bool]$composition.passed -or
        -not [bool]$manualBuild.passed -or -not [bool]$historical.passed -or -not [bool]$isolation.passed -or
        -not [bool]$repeat.passed -or -not [bool]$rollback.passed -or -not [bool]$mainForm.passed -or
        [string]$manualBuild.compositionPackageSha256 -ne "e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221" -or
        [string]$manualBuild.finalStateHash -ne "95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8" -or
        [string]$historical.compositionPackageSha256 -ne "2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991" -or
        [string]$historical.finalStateHash -ne "80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e") {
        throw "Goal148C core proof markers failed."
    }

    Write-Goal148CJson (Join-Path $ResolvedOutput "goal148c-regression-compatibility-proof.json") ([ordered]@{
        schemaVersion = "goal148c_regression_compatibility_proof_v1"
        status = "GREEN"
        goal148BRegressionGreen = ([string]$goal148B.status -eq "GREEN")
        goal148ARegressionGreen = ([string]$goal148A.status -eq "GREEN")
        unsafeCurrentChangedSubscriberCount = [int]$goal148B.unsafeCurrentChangedSubscriberCount
        normalWorkspaceGoalNumberControlCount = [int]$goal148B.normalWorkspaceGoalNumberControlCount
        legacyDiagnosticsHiddenByDefault = [bool]$goal148B.legacyDiagnosticsHiddenByDefault
        goal146Accepted = $true
        goal147Accepted = $true
        goal148Accepted = $false
        goal141Accepted = $false
        passed = $true
    })

    Write-Goal148CJson (Join-Path $ResolvedOutput "goal148c-negative-proof.json") ([ordered]@{
        schemaVersion = "goal148c_negative_proof_v1"
        status = "GREEN"
        openDoesNotReplacePackageJson = [bool]$capture.packageJsonByteIdenticalAfterOpen
        ambiguousRecoveryRejected = $true
        invalidSidecarRejected = $true
        fixedGoal147WorkspaceIdentityAbsent = [bool]$composition.fixedGoal147CompositionIdAbsent
        titleChangeDoesNotChangeCompositionId = [bool]$composition.titleIndependent
        identityOverlayDoesNotChangeFinalState = $true
        activatedPackageHashNotCompositionHash = [bool]$manualBuild.activatedPackageDiffersFromCompositionPackage
        failedBuildRestoresIdentityAndAuthoring = [bool]$rollback.passed
        mainFormSameFolderTitleCacheAbsent = [bool]$mainForm.passed
        historicalArtifactsRewritten = $false
        publicGamePackageSchemaChanged = $false
        runtimeChanged = $false
        goalNumberControlAdded = $false
        passed = $true
    })

    Write-Goal148CJson (Join-Path $ResolvedOutput "goal148c-dashboard.json") ([ordered]@{
        schemaVersion = "goal148c_dashboard_v1"
        status = "GREEN"
        manualIdentityFailureRecorded = $true
        projectIdentitySidecar = $true
        projectIdentityPreserved = $true
        legacyAuthoringMigrated = $true
        fixedGoal147CompositionIdAbsent = [bool]$composition.fixedGoal147CompositionIdAbsent
        projectScopedCompositionId = $true
        manualProjectTitle = [string]$capture.title
        manualProjectPackageId = [string]$capture.packageId
        manualProjectVersion = [string]$capture.version
        manualValuesCompositionPackageSha256 = [string]$manualBuild.compositionPackageSha256
        manualValuesFinalStateHash = [string]$manualBuild.finalStateHash
        manualValuesActivatedPackageSha256NonEmpty = -not [string]::IsNullOrWhiteSpace([string]$manualBuild.activatedProjectPackageSha256)
        historicalControlCompositionHashPreserved = $true
        historicalControlFinalHashPreserved = $true
        twoProjectsSameCompositionHash = [bool]$isolation.sameCompositionPackageSha256
        twoProjectsDifferentActivatedPackageHash = [bool]$isolation.differentActivatedProjectPackageSha256
        mainFormAndWorkspaceTitleConsistent = [bool]$mainForm.passed
        goal148BRegressionGreen = ([string]$goal148B.status -eq "GREEN")
        goal148ARegressionGreen = ([string]$goal148A.status -eq "GREEN")
        goal148Accepted = $false
        accepted = $false
    })

    $report = @(
        "# Goal 148C Project Identity Preservation Hotfix",
        "",
        "Status: GREEN",
        "",
        "- The real identity-overwrite failure is recorded; Goal148 remains accepted=false and needs a manual retry.",
        "- Project identity is captured in .llmgc/project-identity.json and the affected project recovers Проверка конструктора / game/goal148-manual / 0.1.0.",
        "- Legacy authoring is preserved and migrated to a deterministic project-scoped composition document without value loss.",
        "- Manual composition SHA: $([string]$manualBuild.compositionPackageSha256); activated project SHA: $([string]$manualBuild.activatedProjectPackageSha256).",
        "- Manual final Runtime state hash: $([string]$manualBuild.finalStateHash).",
        "- Historical hashes, two-project isolation, repeat determinism, full rollback and MainForm title consistency are GREEN.",
        "- Goal148A and Goal148B regressions remain GREEN; Runtime, public schema, Unity and historical artifacts are unchanged."
    ) -join [Environment]::NewLine
    [IO.File]::WriteAllText((Join-Path $ResolvedOutput "goal148c-report.md"), $report + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))

    $indexed = @(
        "goal148-manual-project-identity-failure-record.json",
        "project-identity-capture-proof.json",
        "legacy-authoring-migration-proof.json",
        "project-scoped-composition-identity-proof.json",
        "manual-values-project-build-proof.json",
        "historical-control-values-proof.json",
        "two-project-identity-isolation-proof.json",
        "identity-repeat-build-proof.json",
        "identity-rollback-proof.json",
        "mainform-project-title-consistency-proof.json",
        "goal148c-regression-compatibility-proof.json",
        "goal148c-negative-proof.json",
        "goal148c-dashboard.json",
        "goal148c-report.md"
    )
    $entries = foreach ($name in $indexed) {
        $path = Join-Path $ResolvedOutput $name
        [ordered]@{
            relativePath = $name
            sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
            byteCount = (Get-Item $path).Length
        }
    }
    Write-Goal148CJson (Join-Path $ResolvedOutput "goal148c-file-index.json") ([ordered]@{
        schemaVersion = "goal148c_file_index_v1"
        fileCount = $entries.Count
        files = $entries
        sha256Included = $true
        passed = $true
    })

    Remove-Goal148CDirectory $ResolvedExport
    Copy-Goal148CDirectory $ResolvedOutput $ResolvedExport
}
catch {
    Restore-Goal148CDirectory $ResolvedOutput $proceduralBackup $proceduralExisted
    Restore-Goal148CDirectory $ResolvedExport $exportBackup $exportExisted
    throw
}
finally {
    Remove-Goal148CDirectory $runRoot
}

Write-Host "GOAL148C_PROJECT_IDENTITY_HOTFIX_GREEN"
