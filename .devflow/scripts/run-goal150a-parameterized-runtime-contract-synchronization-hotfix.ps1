param(
    [string]$OutputRoot = ".llmgc/procedural/goal-150a-parameterized-runtime-contract-synchronization-hotfix",
    [switch]$DryRun,
    [switch]$ApplyCleanup
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"
$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment
$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$Scenario = "goal-150a-parameterized-runtime-contract-synchronization-hotfix"
$ProceduralRelative = ".llmgc/procedural/$Scenario"
$ExportRelative = ".llmgc/exports/$Scenario"

function Resolve-Goal150AOutput([string]$Path) {
    $full = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }))
    $required = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ProceduralRelative))
    if (-not $full.Equals($required, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Goal150A OutputRoot must be the exact procedural artifact root."
    }
    if ($full.IndexOf((Join-Path ".llmgc" "manual"), [StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $full.IndexOf((Join-Path ".llmgc" "workspace"), [StringComparison]::OrdinalIgnoreCase) -ge 0) {
        throw "Goal150A refuses .llmgc/manual and .llmgc/workspace outputs."
    }
    return $full
}

function Remove-Goal150ADirectory([string]$Path) {
    if (Test-Path -LiteralPath $Path -PathType Container) { Remove-Item -LiteralPath $Path -Recurse -Force }
}

function Copy-Goal150ADirectory([string]$Source, [string]$Destination) {
    if (Test-Path -LiteralPath $Source -PathType Container) {
        [IO.Directory]::CreateDirectory((Split-Path -Parent $Destination)) | Out-Null
        Copy-Item -LiteralPath $Source -Destination $Destination -Recurse -Force
    }
}

function Restore-Goal150ADirectory([string]$Destination, [string]$Backup, [bool]$Existed) {
    Remove-Goal150ADirectory $Destination
    if ($Existed) { Copy-Goal150ADirectory $Backup $Destination }
}

$ResolvedOutput = Resolve-Goal150AOutput $OutputRoot
$ResolvedExport = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ExportRelative))
if ($DryRun) {
    Write-Host "GOAL150A_PARAMETERIZED_RUNTIME_CONTRACT_SYNCHRONIZATION_DRY_RUN_GREEN"
    Write-Host "OutputRoot=$ProceduralRelative"
    Write-Host "ExportRoot=$ExportRelative"
    return
}

$runRoot = Join-Path ([IO.Path]::GetTempPath()) ("LLMGameCreator/goal150a-script-" + [Guid]::NewGuid().ToString("N"))
$proceduralBackup = Join-Path $runRoot "backup/procedural"
$exportBackup = Join-Path $runRoot "backup/export"
$proceduralExisted = Test-Path -LiteralPath $ResolvedOutput -PathType Container
$exportExisted = Test-Path -LiteralPath $ResolvedExport -PathType Container
$historicalRelativePaths = @(
    ".llmgc/exports/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/selected-runtime-variant-live-session-checkpoint.json",
    ".llmgc/exports/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/selected-runtime-variant-live-session-file-index.json",
    ".llmgc/exports/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/candidates/minimal-map-game-alchemy-focus/checkpoint.json",
    ".llmgc/exports/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/candidates/minimal-map-game-balanced-baseline/checkpoint.json",
    ".llmgc/exports/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/candidates/minimal-map-game-combat-focus/checkpoint.json",
    ".llmgc/exports/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/candidates/minimal-map-game-exploration-resource-focus/checkpoint.json",
    ".llmgc/exports/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/product-line-interactive-session-file-index.json",
    ".llmgc/procedural/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/selected-runtime-variant-live-session-checkpoint.json",
    ".llmgc/procedural/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/selected-runtime-variant-live-session-file-index.json",
    ".llmgc/procedural/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/candidates/minimal-map-game-alchemy-focus/checkpoint.json",
    ".llmgc/procedural/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/candidates/minimal-map-game-balanced-baseline/checkpoint.json",
    ".llmgc/procedural/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/candidates/minimal-map-game-combat-focus/checkpoint.json",
    ".llmgc/procedural/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/candidates/minimal-map-game-exploration-resource-focus/checkpoint.json",
    ".llmgc/procedural/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/product-line-interactive-session-file-index.json"
)
$historicalBackupRoot = Join-Path $runRoot "backup/historical"
[IO.Directory]::CreateDirectory($runRoot) | Out-Null
Copy-Goal150ADirectory $ResolvedOutput $proceduralBackup
Copy-Goal150ADirectory $ResolvedExport $exportBackup
foreach ($relative in $historicalRelativePaths) {
    $source = Join-Path $RepoRoot $relative
    $backup = Join-Path $historicalBackupRoot $relative
    [IO.Directory]::CreateDirectory((Split-Path -Parent $backup)) | Out-Null
    Copy-Item -LiteralPath $source -Destination $backup -Force
}

function Restore-Goal150AHistoricalFiles {
    foreach ($relative in $historicalRelativePaths) {
        Copy-Item -LiteralPath (Join-Path $historicalBackupRoot $relative) -Destination (Join-Path $RepoRoot $relative) -Force
    }
}

function Assert-Goal150AHistoricalFilesUnchanged {
    foreach ($relative in $historicalRelativePaths) {
        $before = (Get-FileHash -LiteralPath (Join-Path $historicalBackupRoot $relative) -Algorithm SHA256).Hash
        $after = (Get-FileHash -LiteralPath (Join-Path $RepoRoot $relative) -Algorithm SHA256).Hash
        if ($before -ne $after) { throw "Goal150A historical artifact bytes changed: $relative" }
    }
}

try {
    if ($ApplyCleanup) {
        Remove-Goal150ADirectory $ResolvedOutput
        Remove-Goal150ADirectory $ResolvedExport
    }
    Push-Location $RepoRoot
    try {
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronizationTests"
        if ($LASTEXITCODE -ne 0) { throw "Goal150A focused contract tests failed with exit code $LASTEXITCODE." }
        $env:LLMGC_GOAL150A_RUN = "true"
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal150AArtifactProofTests"
        if ($LASTEXITCODE -ne 0) { throw "Goal150A executable artifact proof failed with exit code $LASTEXITCODE." }
    }
    finally {
        Remove-Item Env:LLMGC_GOAL150A_RUN -ErrorAction SilentlyContinue
        Pop-Location
    }

    $required = @(
        "goal150a-dashboard.json",
        "base-defect-analysis.json",
        "effective-value-binding-contract-proof.json",
        "custom-parameter-workspace-build-proof.json",
        "custom-parameter-runtime-effects-proof.json",
        "custom-parameter-save-reopen-proof.json",
        "multiple-stat-scaled-abilities-proof.json",
        "negative-binding-proof.json",
        "incremental-certification-proof.json",
        "default-hash-regression-proof.json",
        "historical-artifact-integrity-proof.json",
        "artifact-scope-proof.json",
        "goal150a-file-index.json",
        "goal150a-report.md"
    )
    foreach ($name in $required) {
        foreach ($artifactRoot in @($ResolvedOutput, $ResolvedExport)) {
            if (-not (Test-Path -LiteralPath (Join-Path $artifactRoot $name) -PathType Leaf)) {
                throw "Goal150A proof missing: $artifactRoot/$name"
            }
        }
        $proceduralHash = (Get-FileHash -LiteralPath (Join-Path $ResolvedOutput $name) -Algorithm SHA256).Hash
        $exportHash = (Get-FileHash -LiteralPath (Join-Path $ResolvedExport $name) -Algorithm SHA256).Hash
        if ($proceduralHash -ne $exportHash) { throw "Goal150A procedural/export proof mismatch: $name" }
    }

    $dashboard = Get-Content -LiteralPath (Join-Path $ResolvedOutput "goal150a-dashboard.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([string]$dashboard.status -ne "GREEN" -or -not [bool]$dashboard.passed -or
        [bool]$dashboard.goal149Accepted -or [bool]$dashboard.goal150Accepted -or
        [bool]$dashboard.goal150aAccepted -or [bool]$dashboard.acceptedByCodex -or
        [bool]$dashboard.accepted -or [bool]$dashboard.manualReviewPerformed) {
        throw "Goal150A dashboard status or acceptance markers failed."
    }

    $genericSources = @(
        "src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterBindingService.cs",
        "src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleEffectiveValueExpression.cs",
        "src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterizedCompositionService.cs",
        "src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs"
    )
    $forbiddenDispatchLiterals = @(
        '"feature.equipment.weapon_loadout"', '"feature.character.attributes"',
        '"feature.character.level_progression"', '"weaponDamageBonus"', '"startingStrength"',
        '"damagePerStrengthPoint"', '"level2RequiredExperience"', '"stat/strength"',
        '"progression/character_level"', '"gain_character_experience"'
    )
    foreach ($source in $genericSources) {
        $text = Get-Content -LiteralPath (Join-Path $RepoRoot $source) -Raw -Encoding UTF8
        foreach ($literal in $forbiddenDispatchLiterals) {
            if ($text.IndexOf($literal, [StringComparison]::Ordinal) -ge 0) {
                throw "Goal150A generic service contains forbidden identifier dispatch literal: $source -> $literal"
            }
        }
    }

    Assert-Goal150AHistoricalFilesUnchanged
    $declaredChangedPaths = @(
        ".devflow/artifact-scope/artifact-scope-policy.json",
        ".devflow/scripts/run-goal150a-parameterized-runtime-contract-synchronization-hotfix.ps1",
        ".devflow/scripts/run-goal150a-parameterized-runtime-contract-synchronization-hotfix.cmd",
        "catalogs/feature-modules/optional/equipment-weapon-loadout.featuremodule.json",
        "catalogs/feature-modules/optional/character-attributes.featuremodule.json",
        "catalogs/feature-modules/optional/character-level-progression.featuremodule.json",
        "docs/CONTEXT_INDEX.md", "docs/CURRENT_GENERATOR_STATE.json", "docs/CURRENT_GENERATOR_STATE.md",
        "docs/FULL_GENERATOR_GOAL_QUEUE.md",
        "docs/manual-acceptance/capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice.md",
        "docs/manual-acceptance/character-attributes-and-level-progression-featuremodules-vertical-slice.md",
        "src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterModels.cs",
        "src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleEffectiveValueExpression.cs",
        "src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterBindingService.cs",
        "src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleLibraryFingerprintService.cs",
        "src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleLibraryValidator.cs",
        "src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterizedCompositionService.cs",
        "src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionModels.cs",
        "src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs",
        "tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal150AParameterizedRuntimeContractSynchronizationTests.cs",
        "tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal150AArtifactProofTests.cs",
        "tests/LLMGameCreator.Tests/Devflow/RunGoal150AParameterizedRuntimeContractSynchronizationHotfixScriptTests.cs",
        "docs/agent-tasks/goal-150a-parameterized-runtime-contract-synchronization-hotfix/GOAL.md",
        "$ProceduralRelative/goal150a-dashboard.json",
        "$ExportRelative/goal150a-dashboard.json"
    )
    $scopeArguments = @()
    foreach ($path in $declaredChangedPaths) { $scopeArguments += @("-ChangedPath", $path) }
    $scopeJson = & (Join-Path $RepoRoot ".devflow/scripts/check-artifact-scope.ps1") -Scenario $Scenario -RemainingArguments $scopeArguments
    if ($LASTEXITCODE -ne 0) { throw "Goal150A artifact scope guard failed." }
    $scope = $scopeJson | ConvertFrom-Json
    if (-not [bool]$scope.accepted -or [int]$scope.violationCount -ne 0) {
        throw "Goal150A artifact scope report contains violations."
    }
}
catch {
    Restore-Goal150ADirectory $ResolvedOutput $proceduralBackup $proceduralExisted
    Restore-Goal150ADirectory $ResolvedExport $exportBackup $exportExisted
    Restore-Goal150AHistoricalFiles
    throw
}
finally {
    Remove-Goal150ADirectory $runRoot
}

Write-Host "GOAL150A_PARAMETERIZED_RUNTIME_CONTRACT_SYNCHRONIZATION_GREEN"
