param(
    [string]$CatalogRoot = "catalogs/feature-modules",
    [string]$WorkspaceRoot = "",
    [string]$CertificationCacheRoot = "",
    [string]$CompositionId = "goal147-custom-alchemy-combat-exploration",
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
$Scenario = "goal-147-persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification"
$ProceduralRelative = ".llmgc/procedural/$Scenario"
$ExportRelative = ".llmgc/exports/$Scenario"
$PassMarker = "GOAL147_SAVED_FEATUREMODULE_COMPOSITION_PASS"
$FailMarker = "GOAL147_SAVED_FEATUREMODULE_COMPOSITION_FAIL"

function ConvertTo-Goal147RelativePath([string]$Path) {
    $full = [IO.Path]::GetFullPath($Path)
    $root = [IO.Path]::GetFullPath($RepoRoot).TrimEnd([IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart([IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }
    return $full
}

function Assert-Goal147NotManual([string]$Path, [string]$Name) {
    $relative = ConvertTo-Goal147RelativePath $Path
    if ($relative.StartsWith(".llmgc/manual/", [StringComparison]::OrdinalIgnoreCase)) {
        throw "Goal147 refuses .llmgc/manual path for $Name."
    }
}

function Resolve-Goal147Catalog([string]$Path) {
    $full = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }))
    $repo = [IO.Path]::GetFullPath($RepoRoot).TrimEnd([IO.Path]::DirectorySeparatorChar)
    if (-not $full.StartsWith($repo + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw "CatalogRoot must stay under repository root."
    }
    Assert-Goal147NotManual $full "CatalogRoot"
    if (-not (Test-Path -LiteralPath (Join-Path $full "catalog.json") -PathType Leaf)) {
        throw "FeatureModule catalog manifest was not found."
    }
    return $full
}

function Resolve-Goal147Workspace([string]$Path, [string]$Fallback, [string]$Name) {
    $full = if ([string]::IsNullOrWhiteSpace($Path)) { [IO.Path]::GetFullPath($Fallback) } else {
        [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }))
    }
    Assert-Goal147NotManual $full $Name
    return $full
}

function Resolve-Goal147Unity([string]$ExplicitPath) {
    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) { return [IO.Path]::GetFullPath($ExplicitPath) }
    $command = Get-Command Unity.exe -ErrorAction SilentlyContinue
    if ($null -ne $command) { return [IO.Path]::GetFullPath($command.Source) }
    foreach ($candidate in @(
        "C:\Program Files\Unity\Hub\Editor\6000.1.10f1\Editor\Unity.exe",
        "C:\Program Files\Unity\Hub\Editor\6000.1.9f1\Editor\Unity.exe",
        "C:\Program Files\Unity\Hub\Editor\6000.0.43f1\Editor\Unity.exe")) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) { return $candidate }
    }
    return ""
}

function Remove-Goal147Directory([string]$Path) {
    if (Test-Path -LiteralPath $Path -PathType Container) { Remove-Item -LiteralPath $Path -Recurse -Force }
}

function Copy-Goal147Directory([string]$Source, [string]$Destination) {
    if (Test-Path -LiteralPath $Source -PathType Container) {
        [IO.Directory]::CreateDirectory((Split-Path -Parent $Destination)) | Out-Null
        Copy-Item -LiteralPath $Source -Destination $Destination -Recurse -Force
    }
}

function Restore-Goal147Directory([string]$Destination, [string]$Backup, [bool]$Existed) {
    Remove-Goal147Directory $Destination
    if ($Existed) { Copy-Goal147Directory $Backup $Destination }
}

function Assert-Goal147CatalogFiles {
    $manifest = Get-Content -LiteralPath (Join-Path $ResolvedCatalog "catalog.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([string]$manifest.schemaVersion -ne "featuremodule_library_manifest_v1") { throw "Unsupported FeatureModule catalog schema." }
    if ([int]$manifest.requiredCoreModuleCount -ne 10 -or [int]$manifest.optionalModuleCount -ne 3 -or [int]$manifest.moduleFileCount -ne 13) {
        throw "FeatureModule catalog count contract failed."
    }
    foreach ($relative in @($manifest.moduleFiles)) {
        $path = [IO.Path]::GetFullPath((Join-Path $ResolvedCatalog ([string]$relative)))
        if (-not $path.StartsWith($ResolvedCatalog.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
            throw "FeatureModule file path escape rejected."
        }
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "FeatureModule file was not found: $relative" }
    }
}

function Invoke-Goal147Core([bool]$RequireUnity) {
    Push-Location $RepoRoot
    try {
        $phase = if ($RequireUnity) { "post-unity" } else { "pre-unity" }
        $env:LLMGC_GOAL147_RUN = "true"
        $env:LLMGC_GOAL147_CATALOG_ROOT = $ResolvedCatalog
        $env:LLMGC_GOAL147_WORKSPACE_ROOT = Join-Path $ResolvedWorkspace $phase
        $env:LLMGC_GOAL147_CACHE_ROOT = Join-Path $ResolvedCache $phase
        $env:LLMGC_GOAL147_OUTPUT_ROOT = ConvertTo-Goal147RelativePath $ResolvedOutput
        $env:LLMGC_GOAL147_COMPOSITION_ID = $CompositionId
        $env:LLMGC_GOAL147_UNITY_SMOKE_PATH = ConvertTo-Goal147RelativePath $SmokePath
        $env:LLMGC_GOAL147_REQUIRE_UNITY_SMOKE = $RequireUnity.ToString().ToLowerInvariant()
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter "FullyQualifiedName~FeatureModuleAuthoringScriptProof"
        if ($LASTEXITCODE -ne 0) { throw "Goal147 core proof failed with exit code $LASTEXITCODE." }
    }
    finally {
        @("LLMGC_GOAL147_RUN","LLMGC_GOAL147_CATALOG_ROOT","LLMGC_GOAL147_WORKSPACE_ROOT","LLMGC_GOAL147_CACHE_ROOT","LLMGC_GOAL147_OUTPUT_ROOT","LLMGC_GOAL147_COMPOSITION_ID","LLMGC_GOAL147_UNITY_SMOKE_PATH","LLMGC_GOAL147_REQUIRE_UNITY_SMOKE") |
            ForEach-Object { Remove-Item ("Env:" + $_) -ErrorAction SilentlyContinue }
        Pop-Location
    }
}

function Invoke-Goal147UnitySmoke {
    if ([string]::IsNullOrWhiteSpace($ResolvedUnity) -or -not (Test-Path -LiteralPath $ResolvedUnity -PathType Leaf)) {
        throw "Goal147 Unity executable was not found."
    }
    $log = Join-Path $ResolvedOutput "unity-saved-featuremodule-composition-smoke.log"
    $process = Start-Process -FilePath $ResolvedUnity -ArgumentList @(
        "-batchmode","-quit","-projectPath",(Join-Path $RepoRoot "unity/LLMGameCreatorAlpha"),
        "-executeMethod","LLMGameCreatorAlpha.CanonicalRuntimeUnitySavedFeatureModuleCompositionHarness.RunBatchmodeSavedFeatureModuleCompositionSmoke",
        "-logFile",$log,"-llmgcGoal147ArtifactRoot",$ResolvedOutput) -WorkingDirectory $RepoRoot -Wait -PassThru -WindowStyle Hidden
    $text = if (Test-Path -LiteralPath $log) { Get-Content -LiteralPath $log -Raw -Encoding UTF8 } else { "" }
    $proof = Get-Content -LiteralPath (Join-Path $ResolvedOutput "parameterized-composition-materialization-proof.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $passed = $process.ExitCode -eq 0 -and $text.Contains($PassMarker) -and -not $text.Contains($FailMarker) `
        -and [bool]$proof.passed -and [bool]$proof.checkpointReloadPassed -and [bool]$proof.fullReplayEquivalent -and [bool]$proof.actionBindingPassed
    $smoke = [ordered]@{
        status=if($passed){"GREEN"}else{"FAILED_UNITY_SMOKE"}
        savedCompositionLoaded=$text.Contains("savedCompositionLoaded=True")
        catalogFingerprintMatches=$text.Contains("catalogFingerprintMatches=True")
        selectedModuleFingerprintsMatch=$text.Contains("selectedModuleFingerprintsMatch=True")
        parameterValuesLoaded=$text.Contains("parameterValuesLoaded=True")
        packageShaMatches=$text.Contains("packageShaMatches=True")
        runtimeQualificationPassed=$text.Contains("runtimeQualificationPassed=True")
        checkpointReloadPassed=$text.Contains("checkpointReloadPassed=True")
        fullReplayEquivalent=$text.Contains("fullReplayEquivalent=True")
        actionBindingPassed=$text.Contains("actionBindingPassed=True")
        runtimeAuthority=$true; unityGameplayTruth=$false
        passMarkerPresent=$text.Contains($PassMarker); failMarkerPresent=$text.Contains($FailMarker)
        unityExitCode=$process.ExitCode; passed=$passed
    }
    [IO.File]::WriteAllText($SmokePath, ($smoke | ConvertTo-Json -Depth 8) + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
    if (-not $passed) { if (Test-Path -LiteralPath $log) { Get-Content -LiteralPath $log -Tail 120 }; throw "Goal147 Unity smoke failed." }
}

$runRoot = Join-Path ([IO.Path]::GetTempPath()) ("LLMGameCreator/goal147-script-" + [Guid]::NewGuid().ToString("N"))
$ResolvedCatalog = Resolve-Goal147Catalog $CatalogRoot
$ResolvedWorkspace = Resolve-Goal147Workspace $WorkspaceRoot (Join-Path $runRoot "workspace") "WorkspaceRoot"
$ResolvedCache = Resolve-Goal147Workspace $CertificationCacheRoot (Join-Path $runRoot "cache") "CertificationCacheRoot"
$ResolvedOutput = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ProceduralRelative))
$ResolvedExport = [IO.Path]::GetFullPath((Join-Path $RepoRoot $ExportRelative))
$ResolvedUnity = Resolve-Goal147Unity $UnityPath
$SmokePath = Join-Path $ResolvedOutput "unity-saved-featuremodule-composition-smoke.json"
Assert-Goal147NotManual $ResolvedOutput "OutputRoot"
Assert-Goal147CatalogFiles

if ($DryRun) {
    Write-Host "GOAL147_FEATUREMODULE_AUTHORING_DRY_RUN_GREEN"
    Write-Host "CatalogRoot=$(ConvertTo-Goal147RelativePath $ResolvedCatalog)"
    Write-Host "WorkspaceRoot=$ResolvedWorkspace"
    Write-Host "CertificationCacheRoot=$ResolvedCache"
    Write-Host "UnityPath=$ResolvedUnity"
    return
}

$proceduralBackup = Join-Path $runRoot "backup/procedural"
$exportBackup = Join-Path $runRoot "backup/export"
$proceduralExisted = Test-Path -LiteralPath $ResolvedOutput -PathType Container
$exportExisted = Test-Path -LiteralPath $ResolvedExport -PathType Container
[IO.Directory]::CreateDirectory($runRoot) | Out-Null
Copy-Goal147Directory $ResolvedOutput $proceduralBackup
Copy-Goal147Directory $ResolvedExport $exportBackup
try {
    if ($ApplyCleanup) { Remove-Goal147Directory $ResolvedOutput; Remove-Goal147Directory $ResolvedExport }
    Invoke-Goal147Core $false
    Invoke-Goal147UnitySmoke
    Invoke-Goal147Core $true
    if ($ApplyCleanup) {
        & (Join-Path $RepoRoot ".devflow/scripts/clean-unity-editor-noise.ps1") -Apply
        if ($LASTEXITCODE -ne 0) { throw "Unity cleanup failed." }
    }
}
catch {
    Restore-Goal147Directory $ResolvedOutput $proceduralBackup $proceduralExisted
    Restore-Goal147Directory $ResolvedExport $exportBackup $exportExisted
    throw
}
finally { Remove-Goal147Directory $runRoot }

Write-Host "GOAL147_FEATUREMODULE_AUTHORING_GREEN"
