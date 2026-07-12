param(
    [string]$OutputRoot = ".llmgc/procedural/goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice",
    [switch]$DryRun,
    [switch]$ApplyCleanup
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"
$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment
$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$Scenario = "goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice"
$ProceduralRelative = ".llmgc/procedural/$Scenario"
$ExportRelative = ".llmgc/exports/$Scenario"

function Resolve-Goal150Output([string]$Path) {
    $full = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }))
    $required = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ProceduralRelative))
    if (-not $full.Equals($required, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Goal150 OutputRoot must be the exact procedural artifact root."
    }
    if ($full.IndexOf((Join-Path ".llmgc" "manual"), [StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $full.IndexOf((Join-Path ".llmgc" "workspace"), [StringComparison]::OrdinalIgnoreCase) -ge 0) {
        throw "Goal150 refuses .llmgc/manual and .llmgc/workspace outputs."
    }
    return $full
}

function Remove-Goal150Directory([string]$Path) {
    if (Test-Path -LiteralPath $Path -PathType Container) { Remove-Item -LiteralPath $Path -Recurse -Force }
}

function Copy-Goal150Directory([string]$Source, [string]$Destination) {
    if (Test-Path -LiteralPath $Source -PathType Container) {
        [IO.Directory]::CreateDirectory((Split-Path -Parent $Destination)) | Out-Null
        Copy-Item -LiteralPath $Source -Destination $Destination -Recurse -Force
    }
}

function Restore-Goal150Directory([string]$Destination, [string]$Backup, [bool]$Existed) {
    Remove-Goal150Directory $Destination
    if ($Existed) { Copy-Goal150Directory $Backup $Destination }
}

$ResolvedOutput = Resolve-Goal150Output $OutputRoot
$ResolvedExport = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ExportRelative))
$RequiredInputs = @(
    (Join-Path $RepoRoot "catalogs/feature-modules/optional/character-attributes.featuremodule.json"),
    (Join-Path $RepoRoot "catalogs/feature-modules/optional/character-level-progression.featuremodule.json")
)
foreach ($path in $RequiredInputs) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Required Goal150 input was not found: $path" }
}

if ($DryRun) {
    Write-Host "GOAL150_CHARACTER_ATTRIBUTES_LEVEL_PROGRESSION_DRY_RUN_GREEN"
    Write-Host "OutputRoot=$ProceduralRelative"
    Write-Host "ExportRoot=$ExportRelative"
    return
}

$runRoot = Join-Path ([IO.Path]::GetTempPath()) ("LLMGameCreator/goal150-script-" + [Guid]::NewGuid().ToString("N"))
$proceduralBackup = Join-Path $runRoot "backup/procedural"
$exportBackup = Join-Path $runRoot "backup/export"
$proceduralExisted = Test-Path -LiteralPath $ResolvedOutput -PathType Container
$exportExisted = Test-Path -LiteralPath $ResolvedExport -PathType Container
[IO.Directory]::CreateDirectory($runRoot) | Out-Null
Copy-Goal150Directory $ResolvedOutput $proceduralBackup
Copy-Goal150Directory $ResolvedExport $exportBackup

try {
    if ($ApplyCleanup) {
        Remove-Goal150Directory $ResolvedOutput
        Remove-Goal150Directory $ResolvedExport
    }
    Push-Location $RepoRoot
    try {
        $env:LLMGC_GOAL150_RUN = "true"
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal150ArtifactProof"
        if ($LASTEXITCODE -ne 0) { throw "Goal150 executable proof tests failed with exit code $LASTEXITCODE." }
    }
    finally {
        Remove-Item Env:LLMGC_GOAL150_RUN -ErrorAction SilentlyContinue
        Pop-Location
    }

    $required = @(
        "goal150-dashboard.json",
        "character-attributes-module-proof.json",
        "level-progression-module-proof.json",
        "extended-mutation-engine-proof.json",
        "attributes-runtime-state-proof.json",
        "progression-runtime-state-proof.json",
        "attributes-without-combat-proof.json",
        "progression-without-combat-proof.json",
        "attributes-combat-proof.json",
        "equipment-attributes-additivity-proof.json",
        "attributes-progression-composition-proof.json",
        "full-current-optional-set-proof.json",
        "goal149-disabled-hash-regression-proof.json",
        "goal149-equipment-hash-regression-proof.json",
        "additive-catalog-compatibility-proof.json",
        "goal150-save-replay-proof.json",
        "goal150-certification-proof.json",
        "goal150-negative-proof.json",
        "goal150-regression-compatibility-proof.json",
        "goal150-file-index.json",
        "goal150-report.md"
    )
    foreach ($name in $required) {
        foreach ($artifactRoot in @($ResolvedOutput, $ResolvedExport)) {
            if (-not (Test-Path -LiteralPath (Join-Path $artifactRoot $name) -PathType Leaf)) {
                throw "Goal150 proof missing: $artifactRoot/$name"
            }
        }
        $proceduralHash = (Get-FileHash -LiteralPath (Join-Path $ResolvedOutput $name) -Algorithm SHA256).Hash
        $exportHash = (Get-FileHash -LiteralPath (Join-Path $ResolvedExport $name) -Algorithm SHA256).Hash
        if ($proceduralHash -ne $exportHash) { throw "Goal150 procedural/export proof mismatch: $name" }
    }
    $dashboard = Get-Content -LiteralPath (Join-Path $ResolvedOutput "goal150-dashboard.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $negative = Get-Content -LiteralPath (Join-Path $ResolvedOutput "goal150-negative-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([string]$dashboard.status -ne "GREEN" -or -not [bool]$negative.passed -or
        [bool]$dashboard.goal150Accepted -or [bool]$dashboard.accepted) {
        throw "Goal150 dashboard or negative proof markers failed."
    }
}
catch {
    Restore-Goal150Directory $ResolvedOutput $proceduralBackup $proceduralExisted
    Restore-Goal150Directory $ResolvedExport $exportBackup $exportExisted
    throw
}
finally {
    Remove-Goal150Directory $runRoot
}

Write-Host "GOAL150_CHARACTER_ATTRIBUTES_LEVEL_PROGRESSION_GREEN"
