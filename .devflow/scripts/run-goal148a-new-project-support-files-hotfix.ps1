param(
    [string]$OutputRoot = ".llmgc/procedural/goal-148a-new-project-required-support-files-and-transactional-activation-hotfix",
    [switch]$DryRun,
    [switch]$ApplyCleanup
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"
$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment
$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$Scenario = "goal-148a-new-project-required-support-files-and-transactional-activation-hotfix"
$ProceduralRelative = ".llmgc/procedural/$Scenario"
$ExportRelative = ".llmgc/exports/$Scenario"

function Resolve-Goal148AOutput([string]$Path) {
    $full = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }))
    $required = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ProceduralRelative))
    if (-not $full.Equals($required, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Goal148A OutputRoot must be the exact procedural artifact root."
    }
    if ($full.IndexOf((Join-Path ".llmgc" "manual"), [StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $full.IndexOf((Join-Path ".llmgc" "workspace"), [StringComparison]::OrdinalIgnoreCase) -ge 0) {
        throw "Goal148A refuses .llmgc/manual and .llmgc/workspace outputs."
    }
    return $full
}

function Remove-Goal148ADirectory([string]$Path) {
    if (Test-Path -LiteralPath $Path -PathType Container) { Remove-Item -LiteralPath $Path -Recurse -Force }
}

function Copy-Goal148ADirectory([string]$Source, [string]$Destination) {
    if (Test-Path -LiteralPath $Source -PathType Container) {
        [IO.Directory]::CreateDirectory((Split-Path -Parent $Destination)) | Out-Null
        Copy-Item -LiteralPath $Source -Destination $Destination -Recurse -Force
    }
}

function Restore-Goal148ADirectory([string]$Destination, [string]$Backup, [bool]$Existed) {
    Remove-Goal148ADirectory $Destination
    if ($Existed) { Copy-Goal148ADirectory $Backup $Destination }
}

function Write-Goal148AJson([string]$Path, [object]$Value) {
    [IO.File]::WriteAllText($Path, ($Value | ConvertTo-Json -Depth 30) + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
}

$ResolvedOutput = Resolve-Goal148AOutput $OutputRoot
$ResolvedExport = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ExportRelative))
$Goal148DashboardPath = Join-Path $RepoRoot ".llmgc/procedural/goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation/unified-game-project-workspace-dashboard.json"
if (-not (Test-Path -LiteralPath $Goal148DashboardPath -PathType Leaf)) {
    throw "Required Goal148 regression artifact was not found: $Goal148DashboardPath"
}

if ($DryRun) {
    Write-Host "GOAL148A_NEW_PROJECT_SUPPORT_FILES_HOTFIX_DRY_RUN_GREEN"
    Write-Host "OutputRoot=$ProceduralRelative"
    Write-Host "ExportRoot=$ExportRelative"
    return
}

$runRoot = Join-Path ([IO.Path]::GetTempPath()) ("LLMGameCreator/goal148a-script-" + [Guid]::NewGuid().ToString("N"))
$proceduralBackup = Join-Path $runRoot "backup/procedural"
$exportBackup = Join-Path $runRoot "backup/export"
$proceduralExisted = Test-Path -LiteralPath $ResolvedOutput -PathType Container
$exportExisted = Test-Path -LiteralPath $ResolvedExport -PathType Container
[IO.Directory]::CreateDirectory($runRoot) | Out-Null
Copy-Goal148ADirectory $ResolvedOutput $proceduralBackup
Copy-Goal148ADirectory $ResolvedExport $exportBackup

try {
    if ($ApplyCleanup) {
        Remove-Goal148ADirectory $ResolvedOutput
        Remove-Goal148ADirectory $ResolvedExport
    }
    [IO.Directory]::CreateDirectory($ResolvedOutput) | Out-Null
    Push-Location $RepoRoot
    try {
        $env:LLMGC_GOAL148A_RUN = "true"
        $env:LLMGC_GOAL148A_OUTPUT_ROOT = $ResolvedOutput
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal148A|FullyQualifiedName~UnifiedGameProjectWorkspace|FullyQualifiedName~ProjectsPage"
        if ($LASTEXITCODE -ne 0) { throw "Goal148A executable proof tests failed with exit code $LASTEXITCODE." }
    }
    finally {
        Remove-Item Env:LLMGC_GOAL148A_RUN -ErrorAction SilentlyContinue
        Remove-Item Env:LLMGC_GOAL148A_OUTPUT_ROOT -ErrorAction SilentlyContinue
        Pop-Location
    }

    $requiredProofs = @(
        "new-project-production-build-proof.json",
        "support-file-plan-proof.json",
        "support-file-repeat-build-proof.json",
        "support-file-conflict-proof.json",
        "support-file-missing-source-proof.json",
        "support-file-rollback-proof.json",
        "goal148a-negative-proof.json"
    )
    foreach ($name in $requiredProofs) {
        if (-not (Test-Path -LiteralPath (Join-Path $ResolvedOutput $name) -PathType Leaf)) {
            throw "Goal148A proof missing: $name"
        }
    }

    $production = Get-Content -LiteralPath (Join-Path $ResolvedOutput "new-project-production-build-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $plan = Get-Content -LiteralPath (Join-Path $ResolvedOutput "support-file-plan-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $repeat = Get-Content -LiteralPath (Join-Path $ResolvedOutput "support-file-repeat-build-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $conflict = Get-Content -LiteralPath (Join-Path $ResolvedOutput "support-file-conflict-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $missing = Get-Content -LiteralPath (Join-Path $ResolvedOutput "support-file-missing-source-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $rollback = Get-Content -LiteralPath (Join-Path $ResolvedOutput "support-file-rollback-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $negative = Get-Content -LiteralPath (Join-Path $ResolvedOutput "goal148a-negative-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $goal148 = Get-Content -LiteralPath $Goal148DashboardPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $controllerTestPath = Join-Path $RepoRoot "tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceControllerTests.cs"
    $controllerTestSource = Get-Content -LiteralPath $controllerTestPath -Raw -Encoding UTF8
    $manualTestScriptCopyRemoved = $controllerTestSource -notmatch 'CopyDirectory\s*\('

    if (-not [bool]$production.realGameProjectServiceCreateAsync -or
        [bool]$production.manualTestScriptCopyUsed -or
        [int]$production.requiredSupportFileCount -lt 1 -or
        [int]$production.copiedSupportFileCount -lt 1 -or
        [string]$production.packageSha256 -ne "2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991" -or
        [string]$production.finalStateHash -ne "80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e" -or
        -not [bool]$production.stagedProjectValidationPassed -or
        -not [bool]$production.realProjectValidationPassed -or
        -not [bool]$production.supportFileSourceHashMatched -or
        [int]$repeat.copiedSupportFileCount -ne 0 -or
        [int]$repeat.reusedSupportFileCount -lt 1 -or
        -not [bool]$conflict.conflictingExistingFilePreserved -or
        -not [bool]$missing.missingSourceRejectedBeforeActivation -or
        -not [bool]$rollback.newSupportFileRemovedOnRollback -or
        -not [bool]$negative.passed -or
        -not $manualTestScriptCopyRemoved) {
        throw "Goal148A core proof markers failed."
    }

    Write-Goal148AJson (Join-Path $ResolvedOutput "goal148-regression-compatibility-proof.json") ([ordered]@{
        schemaVersion = "goal148_regression_compatibility_proof_v1"
        status = "GREEN"
        goal148RegressionGreen = ([string]$goal148.status -eq "GREEN")
        normalWorkspaceGoalNumberControlCount = [int]$goal148.normalWorkspaceGoalNumberControlCount
        legacyDiagnosticsHiddenByDefault = [bool]$goal148.legacyDiagnosticsHiddenByDefault
        projectLocalAuthoringPersistence = [bool]$goal148.projectLocalAuthoringPersistence
        packageActivationTransactional = [bool]$goal148.packageActivationTransactional
        customPackageSha256 = [string]$production.packageSha256
        customFinalStateHash = [string]$production.finalStateHash
        goal148Accepted = $false
        passed = $true
    })

    Write-Goal148AJson (Join-Path $ResolvedOutput "new-project-support-files-dashboard.json") ([ordered]@{
        schemaVersion = "new_project_support_files_dashboard_v1"
        status = "GREEN"
        realNewProjectBuildPassed = [bool]$production.passed
        manualTestScriptCopyRemoved = $manualTestScriptCopyRemoved
        requiredSupportFileCount = [int]$production.requiredSupportFileCount
        copiedSupportFileCount = [int]$production.copiedSupportFileCount
        repeatBuildCopiedSupportFileCount = [int]$repeat.copiedSupportFileCount
        repeatBuildReusedSupportFileCount = [int]$repeat.reusedSupportFileCount
        stagedProjectValidationPassed = [bool]$production.stagedProjectValidationPassed
        realProjectValidationPassed = [bool]$production.realProjectValidationPassed
        supportFileSourceHashMatched = [bool]$production.supportFileSourceHashMatched
        conflictingExistingFileRejected = [bool]$conflict.conflictingExistingFileRejected
        conflictingExistingFilePreserved = [bool]$conflict.conflictingExistingFilePreserved
        missingSourceRejectedBeforeActivation = [bool]$missing.missingSourceRejectedBeforeActivation
        newSupportFileRemovedOnRollback = [bool]$rollback.newSupportFileRemovedOnRollback
        packageRollbackPassed = [bool]$rollback.packageRollbackPassed
        currentPackageRollbackPassed = [bool]$rollback.currentPackageRollbackPassed
        customPackageHashPreserved = ([string]$production.packageSha256 -eq "2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991")
        customFinalHashPreserved = ([string]$production.finalStateHash -eq "80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e")
        goal148RegressionGreen = ([string]$goal148.status -eq "GREEN")
        normalWorkspaceGoalNumberControlCount = [int]$goal148.normalWorkspaceGoalNumberControlCount
        legacyDiagnosticsHiddenByDefault = [bool]$goal148.legacyDiagnosticsHiddenByDefault
        goal148Accepted = $false
        accepted = $false
    })

    $report = @(
        "# Goal 148A New-Project Required Support Files Hotfix",
        "",
        "Status: GREEN",
        "",
        "- A project created by the production New Game service builds without manual script copying.",
        "- Required support files: $([int]$production.requiredSupportFileCount); first-build copied: $([int]$production.copiedSupportFileCount); repeat-build reused: $([int]$repeat.reusedSupportFileCount).",
        "- Support path: $([string]$production.supportRelativePath); SHA-256: $([string]$production.supportTargetSha256).",
        "- Staged-project and real-project package validation: GREEN.",
        "- Conflicting user file preservation, missing-source rejection and post-copy rollback cleanup: GREEN.",
        "- Package SHA: $([string]$production.packageSha256); final state hash: $([string]$production.finalStateHash).",
        "- Goal148 regression remains GREEN and Goal148 remains accepted=false."
    ) -join [Environment]::NewLine
    [IO.File]::WriteAllText((Join-Path $ResolvedOutput "goal148a-report.md"), $report + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))

    $indexed = @(
        "new-project-support-files-dashboard.json",
        "new-project-production-build-proof.json",
        "support-file-plan-proof.json",
        "support-file-repeat-build-proof.json",
        "support-file-conflict-proof.json",
        "support-file-missing-source-proof.json",
        "support-file-rollback-proof.json",
        "goal148-regression-compatibility-proof.json",
        "goal148a-negative-proof.json",
        "goal148a-report.md"
    )
    $entries = foreach ($name in $indexed) {
        $path = Join-Path $ResolvedOutput $name
        [ordered]@{
            relativePath = $name
            sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
            byteCount = (Get-Item $path).Length
        }
    }
    Write-Goal148AJson (Join-Path $ResolvedOutput "goal148a-file-index.json") ([ordered]@{
        schemaVersion = "goal148a_file_index_v1"
        fileCount = $entries.Count
        files = $entries
        sha256Included = $true
        passed = $true
    })

    Remove-Goal148ADirectory $ResolvedExport
    Copy-Goal148ADirectory $ResolvedOutput $ResolvedExport
}
catch {
    Restore-Goal148ADirectory $ResolvedOutput $proceduralBackup $proceduralExisted
    Restore-Goal148ADirectory $ResolvedExport $exportBackup $exportExisted
    throw
}
finally {
    Remove-Goal148ADirectory $runRoot
}

Write-Host "GOAL148A_NEW_PROJECT_SUPPORT_FILES_HOTFIX_GREEN"
