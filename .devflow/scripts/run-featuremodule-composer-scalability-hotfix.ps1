param(
    [string]$OutputRoot = ".llmgc/procedural/goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix",
    [string]$UnityPath = "",
    [switch]$DryRun,
    [switch]$ApplyCleanup
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"
$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment
$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$Scenario = "goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix"
$ProceduralRelative = ".llmgc/procedural/$Scenario"
$ExportRelative = ".llmgc/exports/$Scenario"

function ConvertTo-Goal146ARelativePath([string]$Path) {
    $full = [IO.Path]::GetFullPath($Path)
    $root = [IO.Path]::GetFullPath($RepoRoot).TrimEnd([IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart([IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }
    return $full
}

function Resolve-Goal146AOutput([string]$Path) {
    if ([string]::IsNullOrWhiteSpace($Path)) { throw "OutputRoot is required." }
    $full = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }))
    $allowed = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ProceduralRelative)).TrimEnd([IO.Path]::DirectorySeparatorChar)
    if ($full -ne $allowed -and -not $full.StartsWith($allowed + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must stay under the Goal146A procedural root."
    }
    $relative = ConvertTo-Goal146ARelativePath $full
    if ($relative.StartsWith(".llmgc/manual/", [StringComparison]::OrdinalIgnoreCase)) {
        throw "Goal146A refuses .llmgc/manual path: $relative"
    }
    return $full
}

function Remove-Goal146ADirectory([string]$Path) {
    if (Test-Path -LiteralPath $Path -PathType Container) { Remove-Item -LiteralPath $Path -Recurse -Force }
}

function Copy-Goal146ADirectory([string]$Source, [string]$Destination) {
    if (Test-Path -LiteralPath $Source -PathType Container) {
        [IO.Directory]::CreateDirectory((Split-Path -Parent $Destination)) | Out-Null
        Copy-Item -LiteralPath $Source -Destination $Destination -Recurse -Force
    }
}

function Restore-Goal146ADirectory([string]$Destination, [string]$Backup, [bool]$Existed) {
    Remove-Goal146ADirectory $Destination
    if ($Existed) { Copy-Goal146ADirectory $Backup $Destination }
}

$ResolvedOutput = Resolve-Goal146AOutput $OutputRoot
$ResolvedExport = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ExportRelative))
$Goal146Runner = Join-Path $RepoRoot ".devflow/scripts/run-featuremodule-composition-runtime-matrix.ps1"
if (-not (Test-Path -LiteralPath $Goal146Runner -PathType Leaf)) { throw "Goal146 matrix runner was not found." }

if ($DryRun) {
    $dryArgs = @{ DryRun = $true }
    if (-not [string]::IsNullOrWhiteSpace($UnityPath)) { $dryArgs.UnityPath = $UnityPath }
    & $Goal146Runner @dryArgs
    if ($LASTEXITCODE -ne 0) { throw "Goal146 dry run failed." }
    Write-Host "GOAL146A_FEATUREMODULE_COMPOSER_SCALABILITY_DRY_RUN_GREEN"
    Write-Host "OutputRoot=$(ConvertTo-Goal146ARelativePath $ResolvedOutput)"
    return
}

$backup = Join-Path ([IO.Path]::GetTempPath()) ("LLMGameCreator/goal146a-script-" + [Guid]::NewGuid().ToString("N"))
$proceduralBackup = Join-Path $backup "procedural"
$exportBackup = Join-Path $backup "export"
$proceduralExisted = Test-Path -LiteralPath $ResolvedOutput -PathType Container
$exportExisted = Test-Path -LiteralPath $ResolvedExport -PathType Container
[IO.Directory]::CreateDirectory($backup) | Out-Null
Copy-Goal146ADirectory $ResolvedOutput $proceduralBackup
Copy-Goal146ADirectory $ResolvedExport $exportBackup

try {
    if ($ApplyCleanup) { Remove-Goal146ADirectory $ResolvedOutput; Remove-Goal146ADirectory $ResolvedExport }
    $goal146Args = @{}
    if ($ApplyCleanup) { $goal146Args.ApplyCleanup = $true }
    if (-not [string]::IsNullOrWhiteSpace($UnityPath)) { $goal146Args.UnityPath = $UnityPath }
    & $Goal146Runner @goal146Args
    if ($LASTEXITCODE -ne 0) { throw "Goal146 Runtime matrix or Unity smoke failed." }

    Push-Location $RepoRoot
    try {
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~FeatureModuleComposition|FullyQualifiedName~Goal146|FullyQualifiedName~ProductLineRuntimeQualification|FullyQualifiedName~Goal145"
        if ($LASTEXITCODE -ne 0) { throw "Goal146A focused regressions failed." }
        $env:LLMGC_GOAL146A_RUN = "true"
        $env:LLMGC_GOAL146A_OUTPUT_ROOT = ConvertTo-Goal146ARelativePath $ResolvedOutput
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~FeatureModuleComposerScalabilityScriptProof"
        if ($LASTEXITCODE -ne 0) { throw "Goal146A artifact proof failed." }
    }
    finally {
        Remove-Item Env:LLMGC_GOAL146A_RUN -ErrorAction SilentlyContinue
        Remove-Item Env:LLMGC_GOAL146A_OUTPUT_ROOT -ErrorAction SilentlyContinue
        Pop-Location
    }

    $dashboardPath = Join-Path $ResolvedOutput "generic-composer-scalability-dashboard.json"
    $dashboard = Get-Content -LiteralPath $dashboardPath -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([string]$dashboard.status -ne "GREEN" -or -not [bool]$dashboard.syntheticFourthModulePassed `
        -or [bool]$dashboard.syntheticFourthFullPowersetEnumerated `
        -or [bool]$dashboard.largeCatalogFullPowersetEnumerated) {
        throw "Goal146A dashboard contract failed."
    }
}
catch {
    Restore-Goal146ADirectory $ResolvedOutput $proceduralBackup $proceduralExisted
    Restore-Goal146ADirectory $ResolvedExport $exportBackup $exportExisted
    throw
}
finally {
    Remove-Goal146ADirectory $backup
}

Write-Host "GOAL146A_FEATUREMODULE_COMPOSER_SCALABILITY_GREEN"
