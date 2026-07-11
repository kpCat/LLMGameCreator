param(
    [string]$Goal142Root = ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff",
    [string]$OutputRoot = ".llmgc/procedural/goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix",
    [string]$SelectedModuleIds = "",
    [string]$CompositionId = "",
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
$Goal146RootRelative = ".llmgc/procedural/goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix"
$ExportRootRelative = ".llmgc/exports/goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix"
$PassMarker = "GOAL146_FEATUREMODULE_COMPOSITION_MATRIX_PASS"
$FailMarker = "GOAL146_FEATUREMODULE_COMPOSITION_MATRIX_FAIL"

function ConvertTo-Goal146RelativePath([string]$Path) {
    $full = [IO.Path]::GetFullPath($Path)
    $root = [IO.Path]::GetFullPath($RepoRoot).TrimEnd([IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart([IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }
    return $full
}

function Resolve-Goal146RepoPath([string]$Path, [string]$Name, [bool]$MustExist, [bool]$Directory) {
    if ([string]::IsNullOrWhiteSpace($Path)) { throw "$Name is required." }
    $candidate = if ([IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }
    $full = [IO.Path]::GetFullPath($candidate)
    $prefix = [IO.Path]::GetFullPath($RepoRoot).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if (-not $full.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) { throw "$Name must stay under repository root." }
    $relative = ConvertTo-Goal146RelativePath $full
    if ($relative.StartsWith(".llmgc/manual/", [StringComparison]::OrdinalIgnoreCase)) { throw "Goal146 refuses .llmgc/manual path: $relative" }
    if ($MustExist) {
        $exists = if ($Directory) { [IO.Directory]::Exists($full) } else { [IO.File]::Exists($full) }
        if (-not $exists) { throw "$Name was not found: $relative" }
    }
    return $full
}

function Resolve-Goal146Unity([string]$ExplicitPath) {
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

function Remove-Goal146Directory([string]$Path) {
    if (Test-Path -LiteralPath $Path -PathType Container) { Remove-Item -LiteralPath $Path -Recurse -Force }
}

function Copy-Goal146Directory([string]$Source, [string]$Destination) {
    if (Test-Path -LiteralPath $Source -PathType Container) {
        [IO.Directory]::CreateDirectory((Split-Path -Parent $Destination)) | Out-Null
        Copy-Item -LiteralPath $Source -Destination $Destination -Recurse -Force
    }
}

function Restore-Goal146Directory([string]$Destination, [string]$Backup, [bool]$Existed) {
    Remove-Goal146Directory $Destination
    if ($Existed) { Copy-Goal146Directory $Backup $Destination }
}

function Assert-Goal146Inputs {
    $catalog = Get-Content (Join-Path $ResolvedGoal142 "product-line-runtime-variant-catalog.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $matrix = Get-Content (Join-Path $ResolvedGoal142 "product-line-runtime-variant-matrix-result.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $baseline = @($matrix.candidates | Where-Object candidateId -eq "minimal-map-game-balanced-baseline")
    if ($baseline.Count -ne 1 -or [int]$matrix.candidateCount -ne 4 -or [int]$matrix.passedCandidateCount -ne 4) {
        throw "Goal142 catalog/baseline validation failed."
    }
    $package = Resolve-Goal146RepoPath ([string]$baseline[0].packagePath) "Goal142 baseline package" $true $false
    if (-not $package.StartsWith($ResolvedGoal142 + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Goal142 baseline path escape rejected."
    }
    $hash = ((Get-FileHash $package -Algorithm SHA256).Hash).ToLowerInvariant()
    if ($hash -ne [string]$baseline[0].packageSha256) { throw "Goal142 baseline package hash mismatch rejected." }
    $known = @($catalog.variants | Where-Object recipeId -ne "balanced_baseline" | ForEach-Object { "feature.profile." + [string]$_.recipeId })
    $selected = if ([string]::IsNullOrWhiteSpace($SelectedModuleIds)) { $known } elseif ($SelectedModuleIds -eq "none") { @() } else { @($SelectedModuleIds.Split(',') | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne "" }) }
    foreach ($id in $selected) { if ($known -notcontains $id) { throw "Unknown FeatureModule rejected: $id" } }
    return @($selected)
}

function Invoke-Goal146Core([bool]$RequireUnity) {
    Push-Location $RepoRoot
    try {
        $env:LLMGC_GOAL146_RUN = "true"
        $env:LLMGC_GOAL146_GOAL142_ROOT = ConvertTo-Goal146RelativePath $ResolvedGoal142
        $env:LLMGC_GOAL146_OUTPUT_ROOT = ConvertTo-Goal146RelativePath $ResolvedOutput
        $env:LLMGC_GOAL146_SELECTED_MODULE_IDS = $SelectedModuleIds
        $env:LLMGC_GOAL146_COMPOSITION_ID = $CompositionId
        $env:LLMGC_GOAL146_UNITY_SMOKE_PATH = ConvertTo-Goal146RelativePath $SmokePath
        $env:LLMGC_GOAL146_REQUIRE_UNITY_SMOKE = $RequireUnity.ToString().ToLowerInvariant()
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter "FullyQualifiedName~FeatureModuleCompositionScriptProof"
        if ($LASTEXITCODE -ne 0) { throw "Goal146 Application matrix failed with exit code $LASTEXITCODE." }
    }
    finally {
        @("LLMGC_GOAL146_RUN","LLMGC_GOAL146_GOAL142_ROOT","LLMGC_GOAL146_OUTPUT_ROOT","LLMGC_GOAL146_SELECTED_MODULE_IDS","LLMGC_GOAL146_COMPOSITION_ID","LLMGC_GOAL146_UNITY_SMOKE_PATH","LLMGC_GOAL146_REQUIRE_UNITY_SMOKE") |
            ForEach-Object { Remove-Item ("Env:" + $_) -ErrorAction SilentlyContinue }
        Pop-Location
    }
}

function Invoke-Goal146UnitySmoke {
    $log = Join-Path $ResolvedOutput "unity-featuremodule-composition-matrix-smoke.log"
    if ([string]::IsNullOrWhiteSpace($ResolvedUnity) -or -not (Test-Path $ResolvedUnity -PathType Leaf)) { throw "Goal146 Unity executable was not found." }
    $process = Start-Process -FilePath $ResolvedUnity -ArgumentList @(
        "-batchmode","-quit","-projectPath",(Join-Path $RepoRoot "unity/LLMGameCreatorAlpha"),
        "-executeMethod","LLMGameCreatorAlpha.CanonicalRuntimeUnityFeatureModuleCompositionMatrixHarness.RunBatchmodeFeatureModuleCompositionMatrixSmoke",
        "-logFile",$log,"-llmgcGoal146ArtifactRoot",$ResolvedOutput) -WorkingDirectory $RepoRoot -Wait -PassThru -WindowStyle Hidden
    $text = if (Test-Path $log) { Get-Content $log -Raw -Encoding UTF8 } else { "" }
    $matrix = Get-Content (Join-Path $ResolvedOutput "featuremodule-composition-matrix-result.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $selection = Get-Content (Join-Path $ResolvedOutput "featuremodule-composition-selection-handoff.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $passed = $process.ExitCode -eq 0 -and $text.Contains($PassMarker) -and -not $text.Contains($FailMarker) `
        -and [int]$matrix.compositionCount -eq 8 -and [int]$matrix.passedCompositionCount -eq 8 `
        -and [int]$matrix.distinctPackageSha256Count -eq 8 -and [int]$matrix.distinctFinalStateHashCount -eq 8 `
        -and [int]$matrix.multiModuleCompositionCount -eq 4 `
        -and @($selection.selectedOptionalModuleIds).Count -eq $ResolvedSelectedModules.Count `
        -and @($selection.semanticEffects).Count -eq $ResolvedSelectedModules.Count `
        -and [bool]$matrix.allOrderIndependenceProofsPassed `
        -and [bool]$matrix.allCheckpointReloadsPassed -and [bool]$matrix.allFullReplaysEquivalent `
        -and [bool]$matrix.allActionBindingsPassed -and [bool]$selection.packageDistinctFromGoal142Candidates
    $smoke = [ordered]@{
        status=if($passed){"GREEN"}else{"FAILED_UNITY_SMOKE"}; compositionCount=[int]$matrix.compositionCount
        passedCompositionCount=[int]$matrix.passedCompositionCount; distinctPackageSha256Count=[int]$matrix.distinctPackageSha256Count
        distinctFinalStateHashCount=[int]$matrix.distinctFinalStateHashCount; multiModuleCompositionCount=[int]$matrix.multiModuleCompositionCount
        selectedCompositionExists=($null -ne ($matrix.compositions | Where-Object compositionId -eq $selection.compositionId))
        selectedCompositionModuleCount=@($selection.selectedOptionalModuleIds).Count
        selectedPackageDistinctFromGoal142Candidates=[bool]$selection.packageDistinctFromGoal142Candidates
        selectedCombinedEffectCount=@($selection.semanticEffects).Count
        allOrderIndependenceProofsPassed=[bool]$matrix.allOrderIndependenceProofsPassed
        allCheckpointReloadsPassed=[bool]$matrix.allCheckpointReloadsPassed
        allFullReplaysEquivalent=[bool]$matrix.allFullReplaysEquivalent
        allActionBindingsPassed=[bool]$matrix.allActionBindingsPassed
        runtimeAuthority=$true; unityGameplayTruth=$false; passMarkerPresent=$text.Contains($PassMarker)
        failMarkerPresent=$text.Contains($FailMarker); unityExitCode=$process.ExitCode; passed=$passed
    }
    [IO.File]::WriteAllText($SmokePath, ($smoke | ConvertTo-Json -Depth 8) + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
    if (-not $passed) { if (Test-Path $log) { Get-Content $log -Tail 120 }; throw "Goal146 Unity matrix smoke failed." }
}

$ResolvedGoal142 = Resolve-Goal146RepoPath $Goal142Root "Goal142Root" $true $true
$ResolvedOutput = Resolve-Goal146RepoPath $OutputRoot "OutputRoot" $false $true
$ResolvedExport = Resolve-Goal146RepoPath $ExportRootRelative "ExportRoot" $false $true
if (-not (ConvertTo-Goal146RelativePath $ResolvedOutput).StartsWith($Goal146RootRelative, [StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputRoot must stay under the Goal146 output root."
}
$ResolvedUnity = Resolve-Goal146Unity $UnityPath
$SmokePath = Join-Path $ResolvedOutput "unity-featuremodule-composition-matrix-smoke.json"
$ResolvedSelectedModules = Assert-Goal146Inputs
if ($DryRun) {
    Write-Host "GOAL146_FEATUREMODULE_COMPOSITION_MATRIX_DRY_RUN_GREEN"
    Write-Host "SelectedModuleCount=$($ResolvedSelectedModules.Count)"
    Write-Host "OutputRoot=$(ConvertTo-Goal146RelativePath $ResolvedOutput)"
    Write-Host "UnityPath=$ResolvedUnity"
    return
}

$backup = Join-Path ([IO.Path]::GetTempPath()) ("LLMGameCreator/goal146-script-" + [Guid]::NewGuid().ToString("N"))
$proceduralBackup = Join-Path $backup "procedural"
$exportBackup = Join-Path $backup "export"
$proceduralExisted = Test-Path $ResolvedOutput -PathType Container
$exportExisted = Test-Path $ResolvedExport -PathType Container
[IO.Directory]::CreateDirectory($backup) | Out-Null
Copy-Goal146Directory $ResolvedOutput $proceduralBackup
Copy-Goal146Directory $ResolvedExport $exportBackup
try {
    if ($ApplyCleanup) { Remove-Goal146Directory $ResolvedOutput; Remove-Goal146Directory $ResolvedExport }
    Invoke-Goal146Core $false
    Invoke-Goal146UnitySmoke
    Invoke-Goal146Core $true
    if ($ApplyCleanup) {
        & (Join-Path $RepoRoot ".devflow/scripts/clean-unity-editor-noise.ps1") -Apply
        if ($LASTEXITCODE -ne 0) { throw "Unity cleanup failed." }
    }
}
catch {
    Restore-Goal146Directory $ResolvedOutput $proceduralBackup $proceduralExisted
    Restore-Goal146Directory $ResolvedExport $exportBackup $exportExisted
    throw
}
finally { Remove-Goal146Directory $backup }
Write-Host "GOAL146_FEATUREMODULE_COMPOSITION_MATRIX_GREEN"
