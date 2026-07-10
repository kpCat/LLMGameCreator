param(
    [string]$TemplatePackagePath = "samples/minimal-map-game/package.json",
    [string]$VariantCatalogPath = ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/product-line-runtime-variant-catalog.json",
    [string]$OutputRoot = ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff",
    [switch]$DryRun,
    [switch]$ApplyCleanup
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment

$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$Goal142RootRelative = ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff"
$ExportRootRelative = ".llmgc/exports/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff"

function Test-Goal142PathUnderRoot {
    param(
        [Parameter(Mandatory=$true)][string]$RootPath,
        [Parameter(Mandatory=$true)][string]$CandidatePath
    )

    $root = [System.IO.Path]::GetFullPath($RootPath).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    $candidate = [System.IO.Path]::GetFullPath($CandidatePath)
    return $candidate.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)
}

function ConvertTo-Goal142RelativePath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $full = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith($root + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart([System.IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }

    return $full
}

function Assert-Goal142NotManualPath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $relative = ConvertTo-Goal142RelativePath -Path $Path
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::Ordinal)) {
        throw "Goal142 refuses .llmgc/manual input or output: $relative"
    }
}

function Resolve-Goal142PathUnderRoot {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$Name
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw "$Name is required."
    }

    $full = if ([System.IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }
    $full = [System.IO.Path]::GetFullPath($full)
    if (-not (Test-Goal142PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "$Name must stay under repository root: $Path"
    }

    Assert-Goal142NotManualPath -Path $full
    return $full
}

function Resolve-Goal142InputPath {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$Name
    )

    $full = Resolve-Goal142PathUnderRoot -Path $Path -Name $Name
    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) {
        throw "$Name was not found: $Path"
    }

    return $full
}

function Resolve-Goal142OutputPath {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$Name
    )

    $full = Resolve-Goal142PathUnderRoot -Path $Path -Name $Name
    $relative = ConvertTo-Goal142RelativePath -Path $full
    if (-not $relative.StartsWith($Goal142RootRelative + "/", [System.StringComparison]::OrdinalIgnoreCase) `
        -and -not $relative.Equals($Goal142RootRelative, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Name must stay under the Goal142 output root: $relative"
    }

    return $full
}

$ResolvedTemplatePackagePath = Resolve-Goal142InputPath -Path $TemplatePackagePath -Name "TemplatePackagePath"
$ResolvedVariantCatalogPath = Resolve-Goal142OutputPath -Path $VariantCatalogPath -Name "VariantCatalogPath"
$ResolvedOutputRoot = Resolve-Goal142OutputPath -Path $OutputRoot -Name "OutputRoot"
$ResolvedExportRoot = Resolve-Goal142PathUnderRoot -Path $ExportRootRelative -Name "ExportRoot"

if ($DryRun) {
    $templateHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $ResolvedTemplatePackagePath).Hash.ToLowerInvariant()
    Write-Host "Goal142 dry run passed."
    Write-Host "TemplatePackagePath=$((ConvertTo-Goal142RelativePath -Path $ResolvedTemplatePackagePath))"
    Write-Host "VariantCatalogPath=$((ConvertTo-Goal142RelativePath -Path $ResolvedVariantCatalogPath))"
    Write-Host "OutputRoot=$((ConvertTo-Goal142RelativePath -Path $ResolvedOutputRoot))"
    Write-Host "SourceTemplateSha256=$templateHash"
    Write-Host "CandidateIds=minimal-map-game-balanced-baseline,minimal-map-game-alchemy-focus,minimal-map-game-combat-focus,minimal-map-game-exploration-resource-focus"
    return
}

if ($ApplyCleanup) {
    if (Test-Path -LiteralPath $ResolvedOutputRoot -PathType Container) {
        Remove-Item -LiteralPath $ResolvedOutputRoot -Recurse -Force
    }

    if (Test-Path -LiteralPath $ResolvedExportRoot -PathType Container) {
        Remove-Item -LiteralPath $ResolvedExportRoot -Recurse -Force
    }
}

$templateHashBefore = (Get-FileHash -Algorithm SHA256 -LiteralPath $ResolvedTemplatePackagePath).Hash.ToLowerInvariant()

Push-Location $RepoRoot
try {
    $env:LLMGC_GOAL142_TEMPLATE_PACKAGE_PATH = ConvertTo-Goal142RelativePath -Path $ResolvedTemplatePackagePath
    $env:LLMGC_GOAL142_VARIANT_CATALOG_PATH = ConvertTo-Goal142RelativePath -Path $ResolvedVariantCatalogPath
    $env:LLMGC_GOAL142_OUTPUT_ROOT = ConvertTo-Goal142RelativePath -Path $ResolvedOutputRoot

    & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter "FullyQualifiedName~ProductLineRuntimeVariantMatrixScriptProof"
    if ($LASTEXITCODE -ne 0) {
        throw "Goal142 runtime-significant variant matrix proof test failed with exit code $LASTEXITCODE."
    }
}
finally {
    Remove-Item Env:\LLMGC_GOAL142_TEMPLATE_PACKAGE_PATH -ErrorAction SilentlyContinue
    Remove-Item Env:\LLMGC_GOAL142_VARIANT_CATALOG_PATH -ErrorAction SilentlyContinue
    Remove-Item Env:\LLMGC_GOAL142_OUTPUT_ROOT -ErrorAction SilentlyContinue
    Pop-Location
}

$templateHashAfter = (Get-FileHash -Algorithm SHA256 -LiteralPath $ResolvedTemplatePackagePath).Hash.ToLowerInvariant()
if ($templateHashBefore -ne $templateHashAfter) {
    throw "Goal142 source template hash changed: $templateHashBefore != $templateHashAfter"
}

$dashboardPath = Join-Path $ResolvedOutputRoot "product-line-runtime-variant-matrix-dashboard.json"
if (-not (Test-Path -LiteralPath $dashboardPath -PathType Leaf)) {
    throw "Goal142 dashboard was not written: $dashboardPath"
}

$dashboard = Get-Content -LiteralPath $dashboardPath -Raw | ConvertFrom-Json
if ($dashboard.matrixStatus -ne "GREEN") {
    throw "Goal142 matrix status is not GREEN: $($dashboard.matrixStatus)"
}

Write-Host "GOAL142_PRODUCT_LINE_RUNTIME_VARIANT_MATRIX_GREEN"
Write-Host "SelectedCandidateId=$($dashboard.selectedCandidateId)"
Write-Host "DistinctFinalStateHashCount=$($dashboard.distinctFinalStateHashCount)"
Write-Host "SourceTemplateUnmodified=$($dashboard.sourceTemplateUnmodified)"
