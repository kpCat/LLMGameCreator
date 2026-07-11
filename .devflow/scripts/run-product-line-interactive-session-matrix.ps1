param(
    [string]$Goal142Root = ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff",
    [string]$OutputRoot = ".llmgc/procedural/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix",
    [string]$SelectedCandidateId = "",
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
$Goal145RootRelative = ".llmgc/procedural/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix"
$ExportRootRelative = ".llmgc/exports/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix"
$PassMarker = "GOAL145_PRODUCT_LINE_INTERACTIVE_SESSION_MATRIX_PASS"
$FailMarker = "GOAL145_PRODUCT_LINE_INTERACTIVE_SESSION_MATRIX_FAIL"

function ConvertTo-Goal145RelativePath([string]$Path) {
    $full = [IO.Path]::GetFullPath($Path)
    $root = [IO.Path]::GetFullPath($RepoRoot).TrimEnd([IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart([IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }
    return $full
}

function Resolve-Goal145RepoPath([string]$Path, [string]$Name, [bool]$MustExist, [bool]$Directory) {
    if ([string]::IsNullOrWhiteSpace($Path)) { throw "$Name is required." }
    $candidate = if ([IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }
    $full = [IO.Path]::GetFullPath($candidate)
    $prefix = [IO.Path]::GetFullPath($RepoRoot).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if (-not $full.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) { throw "$Name must stay under repository root." }
    $relative = ConvertTo-Goal145RelativePath $full
    if ($relative.StartsWith(".llmgc/manual/", [StringComparison]::OrdinalIgnoreCase)) { throw "Goal145 refuses .llmgc/manual path: $relative" }
    if ($MustExist) {
        $kind = if ($Directory) { [IO.Directory]::Exists($full) } else { [IO.File]::Exists($full) }
        if (-not $kind) { throw "$Name was not found: $relative" }
    }
    return $full
}

function Resolve-Goal145Unity([string]$ExplicitPath) {
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

function Remove-Goal145Directory([string]$Path) {
    if (Test-Path -LiteralPath $Path -PathType Container) { Remove-Item -LiteralPath $Path -Recurse -Force }
}

function Copy-Goal145Directory([string]$Source, [string]$Destination) {
    if (Test-Path -LiteralPath $Source -PathType Container) {
        [IO.Directory]::CreateDirectory((Split-Path -Parent $Destination)) | Out-Null
        Copy-Item -LiteralPath $Source -Destination $Destination -Recurse -Force
    }
}

function Restore-Goal145Directory([string]$Destination, [string]$Backup, [bool]$Existed) {
    Remove-Goal145Directory $Destination
    if ($Existed) { Copy-Goal145Directory $Backup $Destination }
}

function Assert-Goal145CandidateInputs {
    $matrix = Get-Content (Join-Path $ResolvedGoal142 "product-line-runtime-variant-matrix-result.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $catalog = Get-Content (Join-Path $ResolvedGoal142 "product-line-runtime-variant-catalog.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $scoreboard = Get-Content (Join-Path $ResolvedGoal142 "product-line-runtime-variant-scoreboard.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $selected = Get-Content (Join-Path $ResolvedGoal142 "selected-runtime-variant/selected-runtime-variant-handoff.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([int]$matrix.candidateCount -lt 4 -or [int]$matrix.passedCandidateCount -ne [int]$matrix.candidateCount) {
        throw "Goal142 matrix does not expose four passing candidates."
    }
    $ids = @($matrix.candidates | ForEach-Object { [string]$_.candidateId })
    $paths = @($matrix.candidates | ForEach-Object { [string]$_.packagePath })
    if (($ids | Select-Object -Unique).Count -ne $ids.Count) { throw "Duplicate Goal142 candidate ID rejected." }
    if (($paths | Select-Object -Unique).Count -ne $paths.Count) { throw "Duplicate Goal142 package path rejected." }
    foreach ($row in $matrix.candidates) {
        $package = Resolve-Goal145RepoPath ([string]$row.packagePath) "Goal142 candidate package" $true $false
        $expectedRoot = Join-Path $ResolvedGoal142 ("candidates/" + [string]$row.candidateId)
        if (-not $package.StartsWith([IO.Path]::GetFullPath($expectedRoot) + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
            throw "Goal142 candidate path escape rejected: $($row.candidateId)"
        }
        $hash = ((Get-FileHash $package -Algorithm SHA256).Hash).ToLowerInvariant()
        if ($hash -ne [string]$row.packageSha256) { throw "Goal142 candidate package SHA mismatch: $($row.candidateId)" }
        $catalogRow = @($catalog.variants | Where-Object candidateId -eq $row.candidateId)
        $scoreRow = @($scoreboard.scores | Where-Object candidateId -eq $row.candidateId)
        if ($catalogRow.Count -ne 1 -or $scoreRow.Count -ne 1 `
            -or $catalogRow[0].recipeId -ne $row.recipeId -or $scoreRow[0].variantKind -ne $row.variantKind) {
            throw "Goal142 candidate metadata mismatch: $($row.candidateId)"
        }
    }
    $resolvedSelection = if ([string]::IsNullOrWhiteSpace($SelectedCandidateId)) { [string]$selected.candidateId } else { $SelectedCandidateId }
    if ($ids -notcontains $resolvedSelection) { throw "Unknown selected candidate rejected: $resolvedSelection" }
    return $resolvedSelection
}

function Invoke-Goal145Core([bool]$RequireUnity) {
    Push-Location $RepoRoot
    try {
        $env:LLMGC_GOAL145_GOAL142_ROOT = ConvertTo-Goal145RelativePath $ResolvedGoal142
        $env:LLMGC_GOAL145_OUTPUT_ROOT = ConvertTo-Goal145RelativePath $ResolvedOutput
        $env:LLMGC_GOAL145_SELECTED_CANDIDATE_ID = $ResolvedSelectedCandidateId
        $env:LLMGC_GOAL145_UNITY_SMOKE_PATH = ConvertTo-Goal145RelativePath $SmokePath
        $env:LLMGC_GOAL145_REQUIRE_UNITY_SMOKE = $RequireUnity.ToString().ToLowerInvariant()
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter "FullyQualifiedName~ProductLineInteractiveSessionMatrixScriptProof"
        if ($LASTEXITCODE -ne 0) { throw "Goal145 Application matrix failed with exit code $LASTEXITCODE." }
    }
    finally {
        @("LLMGC_GOAL145_GOAL142_ROOT","LLMGC_GOAL145_OUTPUT_ROOT","LLMGC_GOAL145_SELECTED_CANDIDATE_ID","LLMGC_GOAL145_UNITY_SMOKE_PATH","LLMGC_GOAL145_REQUIRE_UNITY_SMOKE") |
            ForEach-Object { Remove-Item ("Env:" + $_) -ErrorAction SilentlyContinue }
        Pop-Location
    }
}

function Invoke-Goal145UnitySmoke {
    $log = Join-Path $ResolvedOutput "unity-product-line-interactive-session-matrix-smoke.log"
    if ([string]::IsNullOrWhiteSpace($ResolvedUnity) -or -not (Test-Path $ResolvedUnity -PathType Leaf)) { throw "Goal145 Unity executable was not found." }
    $process = Start-Process -FilePath $ResolvedUnity -ArgumentList @(
        "-batchmode","-quit","-projectPath",(Join-Path $RepoRoot "unity/LLMGameCreatorAlpha"),
        "-executeMethod","LLMGameCreatorAlpha.CanonicalRuntimeUnityProductLineInteractiveSessionMatrixHarness.RunBatchmodeProductLineInteractiveSessionMatrixSmoke",
        "-logFile",$log,"-llmgcGoal145ArtifactRoot",$ResolvedOutput) -WorkingDirectory $RepoRoot -Wait -PassThru -WindowStyle Hidden
    $text = if (Test-Path $log) { Get-Content $log -Raw -Encoding UTF8 } else { "" }
    $matrix = Get-Content (Join-Path $ResolvedOutput "product-line-interactive-session-matrix-result.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $selection = Get-Content (Join-Path $ResolvedOutput "product-line-interactive-session-selection-handoff.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $selectedPackage = Resolve-Goal145RepoPath ([string]$selection.selectedPackagePath) "SelectedPackagePath" $true $false
    $selectedHash = ((Get-FileHash $selectedPackage -Algorithm SHA256).Hash).ToLowerInvariant()
    $passed = $process.ExitCode -eq 0 -and $text.Contains($PassMarker) -and -not $text.Contains($FailMarker) `
        -and [int]$matrix.candidateCount -ge 4 -and [int]$matrix.passedCandidateCount -eq [int]$matrix.candidateCount `
        -and [int]$matrix.distinctFinalStateHashCount -ge 4 -and [bool]$matrix.allCandidateCheckpointReloadsPassed `
        -and [bool]$matrix.allCandidateFullReplaysEquivalent -and [bool]$matrix.allCandidateActionBindingsPassed `
        -and [bool]$matrix.allFocusEffectsObserved -and $selectedHash -eq [string]$selection.selectedPackageSha256
    $smoke = [ordered]@{
        schemaVersion="unity_product_line_interactive_session_matrix_smoke_v1"; status=if($passed){"GREEN"}else{"FAILED_UNITY_SMOKE"}
        candidateCount=[int]$matrix.candidateCount; passedCandidateCount=[int]$matrix.passedCandidateCount
        distinctFinalStateHashCount=[int]$matrix.distinctFinalStateHashCount; selectedCandidateExists=($null -ne ($matrix.candidates | Where-Object candidateId -eq $selection.selectedCandidateId))
        selectedCandidatePackageHashMatches=($selectedHash -eq [string]$selection.selectedPackageSha256)
        allCandidateCheckpointReloadsPassed=[bool]$matrix.allCandidateCheckpointReloadsPassed
        allCandidateFullReplaysEquivalent=[bool]$matrix.allCandidateFullReplaysEquivalent
        allCandidateActionBindingsPassed=[bool]$matrix.allCandidateActionBindingsPassed
        allFocusEffectsObserved=[bool]$matrix.allFocusEffectsObserved; runtimeAuthority=$true; unityGameplayTruth=$false
        passMarkerPresent=$text.Contains($PassMarker); failMarkerPresent=$text.Contains($FailMarker); passed=$passed
        unityExitCode=$process.ExitCode; diagnostics=@("unityExitCode=$($process.ExitCode)")
    }
    [IO.File]::WriteAllText($SmokePath, ($smoke | ConvertTo-Json -Depth 8) + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
    if (-not $passed) { if (Test-Path $log) { Get-Content $log -Tail 120 }; throw "Goal145 Unity matrix smoke failed." }
}

$ResolvedGoal142 = Resolve-Goal145RepoPath $Goal142Root "Goal142Root" $true $true
$ResolvedOutput = Resolve-Goal145RepoPath $OutputRoot "OutputRoot" $false $true
$ResolvedExport = Resolve-Goal145RepoPath $ExportRootRelative "ExportRoot" $false $true
if (-not (ConvertTo-Goal145RelativePath $ResolvedOutput).StartsWith($Goal145RootRelative, [StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputRoot must stay under the Goal145 output root."
}
$ResolvedUnity = Resolve-Goal145Unity $UnityPath
$SmokePath = Join-Path $ResolvedOutput "unity-product-line-interactive-session-matrix-smoke.json"
$ResolvedSelectedCandidateId = Assert-Goal145CandidateInputs
if ($DryRun) {
    Write-Host "GOAL145_PRODUCT_LINE_INTERACTIVE_SESSION_MATRIX_DRY_RUN_GREEN"
    Write-Host "CandidateCount=4"
    Write-Host "SelectedCandidateId=$ResolvedSelectedCandidateId"
    Write-Host "OutputRoot=$(ConvertTo-Goal145RelativePath $ResolvedOutput)"
    Write-Host "UnityPath=$ResolvedUnity"
    return
}

$backup = Join-Path ([IO.Path]::GetTempPath()) ("LLMGameCreator/goal145-script-" + [Guid]::NewGuid().ToString("N"))
$proceduralBackup = Join-Path $backup "procedural"
$exportBackup = Join-Path $backup "export"
$proceduralExisted = Test-Path $ResolvedOutput -PathType Container
$exportExisted = Test-Path $ResolvedExport -PathType Container
[IO.Directory]::CreateDirectory($backup) | Out-Null
Copy-Goal145Directory $ResolvedOutput $proceduralBackup
Copy-Goal145Directory $ResolvedExport $exportBackup
try {
    if ($ApplyCleanup) {
        Remove-Goal145Directory $ResolvedOutput
        Remove-Goal145Directory $ResolvedExport
    }
    Invoke-Goal145Core $false
    Invoke-Goal145UnitySmoke
    Invoke-Goal145Core $true
    if ($ApplyCleanup) {
        & (Join-Path $RepoRoot ".devflow/scripts/clean-unity-editor-noise.ps1") -Apply
        if ($LASTEXITCODE -ne 0) { throw "Unity cleanup failed." }
    }
}
catch {
    Restore-Goal145Directory $ResolvedOutput $proceduralBackup $proceduralExisted
    Restore-Goal145Directory $ResolvedExport $exportBackup $exportExisted
    throw
}
finally { Remove-Goal145Directory $backup }
Write-Host "GOAL145_PRODUCT_LINE_INTERACTIVE_SESSION_MATRIX_GREEN"
