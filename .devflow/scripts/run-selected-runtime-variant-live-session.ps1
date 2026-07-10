param(
    [string]$SelectedHandoffPath = ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/selected-runtime-variant/selected-runtime-variant-handoff.json",
    [string]$SelectedPackagePath = ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/selected-runtime-variant/package.json",
    [string]$SelectedOutcomePath = ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/selected-runtime-variant/runtime-outcome-summary.json",
    [string]$Goal143HandoffPath = ".llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/selected-runtime-variant-playeradapter-handoff.json",
    [string]$OutputRoot = ".llmgc/procedural/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay",
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
$GoalRootRelative = ".llmgc/procedural/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay"
$ExportRootRelative = ".llmgc/exports/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay"
$PassMarker = "GOAL144_SELECTED_RUNTIME_VARIANT_LIVE_SESSION_PASS"
$FailMarker = "GOAL144_SELECTED_RUNTIME_VARIANT_LIVE_SESSION_FAIL"

function ConvertTo-Goal144RelativePath([string]$Path) {
    $full = [IO.Path]::GetFullPath($Path)
    $root = [IO.Path]::GetFullPath($RepoRoot).TrimEnd([IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart([IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }
    return $full
}

function Resolve-Goal144RepoPath([string]$Path, [string]$Name, [bool]$MustExist) {
    if ([string]::IsNullOrWhiteSpace($Path)) { throw "$Name is required." }
    $candidate = if ([IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }
    $full = [IO.Path]::GetFullPath($candidate)
    $prefix = [IO.Path]::GetFullPath($RepoRoot).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if (-not $full.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) { throw "$Name must stay under repository root." }
    $relative = ConvertTo-Goal144RelativePath $full
    if ($relative.StartsWith(".llmgc/manual/", [StringComparison]::OrdinalIgnoreCase)) { throw "Goal144 refuses .llmgc/manual path: $relative" }
    if ($MustExist -and -not (Test-Path -LiteralPath $full -PathType Leaf)) { throw "$Name was not found: $relative" }
    return $full
}

function Remove-Goal144Directory([string]$Path) {
    if (Test-Path -LiteralPath $Path -PathType Container) { Remove-Item -LiteralPath $Path -Recurse -Force }
}

function Copy-Goal144Directory([string]$Source, [string]$Destination) {
    if (Test-Path -LiteralPath $Source -PathType Container) {
        [IO.Directory]::CreateDirectory((Split-Path -Parent $Destination)) | Out-Null
        Copy-Item -LiteralPath $Source -Destination $Destination -Recurse -Force
    }
}

function Restore-Goal144Directory([string]$Destination, [string]$Backup, [bool]$Existed) {
    Remove-Goal144Directory $Destination
    if ($Existed) { Copy-Goal144Directory $Backup $Destination }
}

function Resolve-Goal144Unity([string]$ExplicitPath) {
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

function Assert-Goal144Integrity {
    $handoff = Get-Content $ResolvedHandoff -Raw -Encoding UTF8 | ConvertFrom-Json
    $outcome = Get-Content $ResolvedOutcome -Raw -Encoding UTF8 | ConvertFrom-Json
    $goal143 = Get-Content $ResolvedGoal143 -Raw -Encoding UTF8 | ConvertFrom-Json
    $hash = ((Get-FileHash $ResolvedPackage -Algorithm SHA256).Hash).ToLowerInvariant()
    if ($handoff.candidateId -ne "minimal-map-game-exploration-resource-focus" `
        -or $handoff.variantKind -ne "exploration_resource_focus" `
        -or [int]$handoff.score -ne 100 `
        -or $handoff.packageSha256 -ne $hash `
        -or $handoff.finalStateHash -ne "d7c04179cb76ca48ba9694905e491bead014c0f56f446f66331becd5e3211e54" `
        -or $outcome.finalStateHash -ne $handoff.finalStateHash `
        -or $goal143.sourcePackageSha256 -ne $hash `
        -or $goal143.finalStateHash -ne $handoff.finalStateHash `
        -or -not [bool]$goal143.selectedPackageSha256MatchesHandoff `
        -or -not [bool]$goal143.selectedFinalStateHashMatches) {
        throw "Goal144 Goal142/Goal143 identity, package hash or final-state integrity failed."
    }
    return $hash
}

function Invoke-Goal144Core([bool]$RequireUnity) {
    Push-Location $RepoRoot
    try {
        $env:LLMGC_GOAL144_SELECTED_HANDOFF_PATH = ConvertTo-Goal144RelativePath $ResolvedHandoff
        $env:LLMGC_GOAL144_SELECTED_PACKAGE_PATH = ConvertTo-Goal144RelativePath $ResolvedPackage
        $env:LLMGC_GOAL144_SELECTED_OUTCOME_PATH = ConvertTo-Goal144RelativePath $ResolvedOutcome
        $env:LLMGC_GOAL144_GOAL143_HANDOFF_PATH = ConvertTo-Goal144RelativePath $ResolvedGoal143
        $env:LLMGC_GOAL144_OUTPUT_ROOT = ConvertTo-Goal144RelativePath $ResolvedOutput
        $env:LLMGC_GOAL144_UNITY_SMOKE_PATH = ConvertTo-Goal144RelativePath $SmokePath
        $env:LLMGC_GOAL144_REQUIRE_UNITY_SMOKE = $RequireUnity.ToString().ToLowerInvariant()
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter "FullyQualifiedName~SelectedRuntimeVariantInteractiveSessionScriptProof"
        if ($LASTEXITCODE -ne 0) { throw "Goal144 Application drill failed with exit code $LASTEXITCODE." }
    }
    finally {
        @("LLMGC_GOAL144_SELECTED_HANDOFF_PATH","LLMGC_GOAL144_SELECTED_PACKAGE_PATH","LLMGC_GOAL144_SELECTED_OUTCOME_PATH","LLMGC_GOAL144_GOAL143_HANDOFF_PATH","LLMGC_GOAL144_OUTPUT_ROOT","LLMGC_GOAL144_UNITY_SMOKE_PATH","LLMGC_GOAL144_REQUIRE_UNITY_SMOKE") |
            ForEach-Object { Remove-Item ("Env:" + $_) -ErrorAction SilentlyContinue }
        Pop-Location
    }
}

function Invoke-Goal144UnitySmoke {
    $log = Join-Path $ResolvedOutput "unity-selected-runtime-variant-live-session-smoke.log"
    if ([string]::IsNullOrWhiteSpace($ResolvedUnity) -or -not (Test-Path $ResolvedUnity -PathType Leaf)) { throw "Goal144 Unity executable was not found." }
    $process = Start-Process -FilePath $ResolvedUnity -ArgumentList @(
        "-batchmode","-quit","-projectPath",(Join-Path $RepoRoot "unity/LLMGameCreatorAlpha"),
        "-executeMethod","LLMGameCreatorAlpha.CanonicalRuntimeUnitySelectedVariantLiveSessionHarness.RunBatchmodeSelectedRuntimeVariantLiveSessionSmoke",
        "-logFile",$log,"-llmgcGoal144ArtifactRoot",$ResolvedOutput) -WorkingDirectory $RepoRoot -Wait -PassThru -WindowStyle Hidden
    $text = if (Test-Path $log) { Get-Content $log -Raw -Encoding UTF8 } else { "" }
    $pass = $process.ExitCode -eq 0 -and $text.Contains($PassMarker) -and -not $text.Contains($FailMarker)
    $checks = [ordered]@{}
    foreach ($name in @("sessionArtifactsExist","selectedCandidateMatches","packageHashMatches","checkpointReloadPassed","fullReplayEquivalent","finalHashMatchesGoal142","selectedVariantEffectVisible","noFallback","runtimeAuthority")) {
        $checks[$name] = $text.Contains("$name=True")
    }
    $unityTruth = $text.Contains("unityGameplayTruth=True")
    $passed = $pass -and -not $unityTruth -and -not ($checks.Values -contains $false)
    $smoke = [ordered]@{
        schemaVersion="unity_selected_runtime_variant_live_session_smoke_v1"; goalId="goal_144_selected_runtime_variant_interactive_action_session_and_save_replay"
        status=if($passed){"GREEN"}else{"FAILED_UNITY_SMOKE"}; sessionArtifactsExist=$checks.sessionArtifactsExist
        selectedCandidateMatches=$checks.selectedCandidateMatches; packageHashMatches=$checks.packageHashMatches
        checkpointReloadPassed=$checks.checkpointReloadPassed; fullReplayEquivalent=$checks.fullReplayEquivalent
        finalHashMatchesGoal142=$checks.finalHashMatchesGoal142; selectedVariantEffectVisible=$checks.selectedVariantEffectVisible
        noFallback=$checks.noFallback; runtimeAuthority=$checks.runtimeAuthority; unityGameplayTruth=$unityTruth
        passMarkerPresent=$text.Contains($PassMarker); failMarkerPresent=$text.Contains($FailMarker); passed=$passed
        unityExitCode=$process.ExitCode; dashboardSha256=((Get-FileHash (Join-Path $ResolvedOutput "selected-runtime-variant-live-session-dashboard.json") -Algorithm SHA256).Hash).ToLowerInvariant()
        diagnostics=@("unityExitCode=$($process.ExitCode)")
    }
    $json = $smoke | ConvertTo-Json -Depth 8
    [IO.File]::WriteAllText($SmokePath, $json + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
    if (-not $passed) { if (Test-Path $log) { Get-Content $log -Tail 120 }; throw "Goal144 Unity live-session smoke failed." }
}

$ResolvedHandoff = Resolve-Goal144RepoPath $SelectedHandoffPath "SelectedHandoffPath" $true
$ResolvedPackage = Resolve-Goal144RepoPath $SelectedPackagePath "SelectedPackagePath" $true
$ResolvedOutcome = Resolve-Goal144RepoPath $SelectedOutcomePath "SelectedOutcomePath" $true
$ResolvedGoal143 = Resolve-Goal144RepoPath $Goal143HandoffPath "Goal143HandoffPath" $true
$ResolvedOutput = Resolve-Goal144RepoPath $OutputRoot "OutputRoot" $false
$ResolvedExport = Resolve-Goal144RepoPath $ExportRootRelative "ExportRoot" $false
if (-not (ConvertTo-Goal144RelativePath $ResolvedOutput).StartsWith($GoalRootRelative, [StringComparison]::OrdinalIgnoreCase)) { throw "OutputRoot must stay under Goal144 root." }
$ResolvedUnity = Resolve-Goal144Unity $UnityPath
$SmokePath = Join-Path $ResolvedOutput "unity-selected-runtime-variant-live-session-smoke.json"
$hash = Assert-Goal144Integrity
if ($DryRun) {
    Write-Host "GOAL144_SELECTED_RUNTIME_VARIANT_LIVE_SESSION_DRY_RUN_GREEN"
    Write-Host "SelectedCandidateId=minimal-map-game-exploration-resource-focus"
    Write-Host "SelectedPackageSha256=$hash"
    Write-Host "OutputRoot=$(ConvertTo-Goal144RelativePath $ResolvedOutput)"
    Write-Host "UnityPath=$ResolvedUnity"
    return
}

$backup = Join-Path ([IO.Path]::GetTempPath()) ("LLMGameCreator/goal144-script-" + [Guid]::NewGuid().ToString("N"))
$proceduralBackup = Join-Path $backup "procedural"
$exportBackup = Join-Path $backup "export"
$proceduralExisted = Test-Path $ResolvedOutput -PathType Container
$exportExisted = Test-Path $ResolvedExport -PathType Container
[IO.Directory]::CreateDirectory($backup) | Out-Null
Copy-Goal144Directory $ResolvedOutput $proceduralBackup
Copy-Goal144Directory $ResolvedExport $exportBackup
try {
    if ($ApplyCleanup) { Remove-Goal144Directory $ResolvedOutput; Remove-Goal144Directory $ResolvedExport }
    Invoke-Goal144Core $false
    Invoke-Goal144UnitySmoke
    Invoke-Goal144Core $true
    if ($ApplyCleanup) { & (Join-Path $RepoRoot ".devflow/scripts/clean-unity-editor-noise.ps1") -Apply; if ($LASTEXITCODE -ne 0) { throw "Unity cleanup failed." } }
}
catch {
    Restore-Goal144Directory $ResolvedOutput $proceduralBackup $proceduralExisted
    Restore-Goal144Directory $ResolvedExport $exportBackup $exportExisted
    throw
}
finally { Remove-Goal144Directory $backup }
Write-Host "GOAL144_SELECTED_RUNTIME_VARIANT_LIVE_SESSION_GREEN"
