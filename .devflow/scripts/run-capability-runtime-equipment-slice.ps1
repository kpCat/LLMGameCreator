param(
    [string]$OutputRoot = ".llmgc/procedural/goal-149-capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice",
    [switch]$DryRun,
    [switch]$ApplyCleanup
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"
$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment
$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$Scenario = "goal-149-capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice"
$ProceduralRelative = ".llmgc/procedural/$Scenario"
$ExportRelative = ".llmgc/exports/$Scenario"

function Resolve-Goal149Output([string]$Path) {
    $full = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }))
    $required = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ProceduralRelative))
    if (-not $full.Equals($required, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Goal149 OutputRoot must be the exact procedural artifact root."
    }
    if ($full.IndexOf((Join-Path ".llmgc" "manual"), [StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $full.IndexOf((Join-Path ".llmgc" "workspace"), [StringComparison]::OrdinalIgnoreCase) -ge 0) {
        throw "Goal149 refuses .llmgc/manual and .llmgc/workspace outputs."
    }
    return $full
}

function Remove-Goal149Directory([string]$Path) {
    if (Test-Path -LiteralPath $Path -PathType Container) { Remove-Item -LiteralPath $Path -Recurse -Force }
}

function Copy-Goal149Directory([string]$Source, [string]$Destination) {
    if (Test-Path -LiteralPath $Source -PathType Container) {
        [IO.Directory]::CreateDirectory((Split-Path -Parent $Destination)) | Out-Null
        Copy-Item -LiteralPath $Source -Destination $Destination -Recurse -Force
    }
}

function Restore-Goal149Directory([string]$Destination, [string]$Backup, [bool]$Existed) {
    Remove-Goal149Directory $Destination
    if ($Existed) { Copy-Goal149Directory $Backup $Destination }
}

$ResolvedOutput = Resolve-Goal149Output $OutputRoot
$ResolvedExport = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ExportRelative))
$RequiredInputs = @(
    (Join-Path $RepoRoot "docs/manual-acceptance/unified-game-project-workspace-and-legacy-goal-diagnostics-isolation.md"),
    (Join-Path $RepoRoot "catalogs/feature-modules/optional/equipment-weapon-loadout.featuremodule.json")
)
foreach ($path in $RequiredInputs) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Required Goal149 input was not found: $path" }
}

if ($DryRun) {
    Write-Host "GOAL149_CAPABILITY_RUNTIME_EQUIPMENT_DRY_RUN_GREEN"
    Write-Host "OutputRoot=$ProceduralRelative"
    Write-Host "ExportRoot=$ExportRelative"
    return
}

$runRoot = Join-Path ([IO.Path]::GetTempPath()) ("LLMGameCreator/goal149-script-" + [Guid]::NewGuid().ToString("N"))
$proceduralBackup = Join-Path $runRoot "backup/procedural"
$exportBackup = Join-Path $runRoot "backup/export"
$proceduralExisted = Test-Path -LiteralPath $ResolvedOutput -PathType Container
$exportExisted = Test-Path -LiteralPath $ResolvedExport -PathType Container
[IO.Directory]::CreateDirectory($runRoot) | Out-Null
Copy-Goal149Directory $ResolvedOutput $proceduralBackup
Copy-Goal149Directory $ResolvedExport $exportBackup

try {
    if ($ApplyCleanup) {
        Remove-Goal149Directory $ResolvedOutput
        Remove-Goal149Directory $ResolvedExport
    }
    Push-Location $RepoRoot
    try {
        $env:LLMGC_GOAL149_RUN = "true"
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal149"
        if ($LASTEXITCODE -ne 0) { throw "Goal149 executable proof tests failed with exit code $LASTEXITCODE." }
    }
    finally {
        Remove-Item Env:LLMGC_GOAL149_RUN -ErrorAction SilentlyContinue
        Pop-Location
    }

    $required = @(
        "goal148-human-acceptance-record.json",
        "capability-runtime-playthrough-contract-catalog.json",
        "capability-runtime-playthrough-plan.json",
        "capability-runtime-playthrough-dashboard.json",
        "legacy-project-additive-compatibility-proof.json",
        "legacy-project-hash-regression-proof.json",
        "equipment-module-definition-proof.json",
        "equipment-enabled-build-proof.json",
        "equipment-disabled-build-proof.json",
        "equipment-without-combat-proof.json",
        "combat-without-equipment-proof.json",
        "equipment-save-replay-proof.json",
        "equipment-negative-proof.json",
        "goal149-regression-compatibility-proof.json",
        "goal149-negative-proof.json",
        "goal149-file-index.json",
        "goal149-report.md"
    )
    foreach ($name in $required) {
        foreach ($artifactRoot in @($ResolvedOutput, $ResolvedExport)) {
            if (-not (Test-Path -LiteralPath (Join-Path $artifactRoot $name) -PathType Leaf)) {
                throw "Goal149 proof missing: $artifactRoot/$name"
            }
        }
    }
    $dashboard = Get-Content -LiteralPath (Join-Path $ResolvedOutput "capability-runtime-playthrough-dashboard.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $negative = Get-Content -LiteralPath (Join-Path $ResolvedOutput "goal149-negative-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([string]$dashboard.status -ne "GREEN" -or -not [bool]$negative.passed -or
        [bool]$dashboard.goal149Accepted -or [bool]$dashboard.accepted) {
        throw "Goal149 dashboard or negative proof markers failed."
    }
}
catch {
    Restore-Goal149Directory $ResolvedOutput $proceduralBackup $proceduralExisted
    Restore-Goal149Directory $ResolvedExport $exportBackup $exportExisted
    throw
}
finally {
    Remove-Goal149Directory $runRoot
}

Write-Host "GOAL149_CAPABILITY_RUNTIME_EQUIPMENT_GREEN"
