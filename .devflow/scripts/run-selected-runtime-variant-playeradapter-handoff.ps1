param(
    [string]$SelectedHandoffPath = ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/selected-runtime-variant/selected-runtime-variant-handoff.json",
    [string]$SelectedPackagePath = ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/selected-runtime-variant/package.json",
    [string]$SelectedOutcomePath = ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/selected-runtime-variant/runtime-outcome-summary.json",
    [string]$SelectedRoundtripResultPath = ".llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/matrix/minimal-map-game-exploration-resource-focus/roundtrip-result.json",
    [string]$OutputRoot = ".llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff",
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
$Goal143RootRelative = ".llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff"
$ExportRootRelative = ".llmgc/exports/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff"
$UnityProjectPath = Join-Path $RepoRoot "unity/LLMGameCreatorAlpha"
$ExecuteMethod = "LLMGameCreatorAlpha.CanonicalRuntimeUnitySelectedVariantPlayerAdapterHarness.RunBatchmodeSelectedRuntimeVariantPlayerAdapterSmoke"
$PassMarker = "GOAL143_SELECTED_RUNTIME_VARIANT_PLAYERADAPTER_PASS"
$FailMarker = "GOAL143_SELECTED_RUNTIME_VARIANT_PLAYERADAPTER_FAIL"

function Test-Goal143PathUnderRoot {
    param(
        [Parameter(Mandatory=$true)][string]$RootPath,
        [Parameter(Mandatory=$true)][string]$CandidatePath
    )

    $root = [System.IO.Path]::GetFullPath($RootPath).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    $candidate = [System.IO.Path]::GetFullPath($CandidatePath)
    return $candidate.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)
}

function ConvertTo-Goal143RelativePath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $full = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith(
            $root + [System.IO.Path]::DirectorySeparatorChar,
            [System.StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart(
            [System.IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }

    return $full
}

function Assert-Goal143NotManualPath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $relative = ConvertTo-Goal143RelativePath -Path $Path
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Goal143 refuses .llmgc/manual input or output: $relative"
    }
}

function Resolve-Goal143PathUnderRoot {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$Name
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw "$Name is required."
    }

    $candidate = if ([System.IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $RepoRoot $Path }
    $full = [System.IO.Path]::GetFullPath($candidate)
    if (-not (Test-Goal143PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "$Name must stay under repository root: $Path"
    }

    Assert-Goal143NotManualPath -Path $full
    return $full
}

function Resolve-Goal143InputPath {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$Name
    )

    $full = Resolve-Goal143PathUnderRoot -Path $Path -Name $Name
    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) {
        throw "$Name was not found: $Path"
    }

    return $full
}

function Resolve-Goal143OutputRoot {
    param([Parameter(Mandatory=$true)][string]$Path)

    $full = Resolve-Goal143PathUnderRoot -Path $Path -Name "OutputRoot"
    $relative = ConvertTo-Goal143RelativePath -Path $full
    if (-not $relative.Equals($Goal143RootRelative, [System.StringComparison]::OrdinalIgnoreCase) `
        -and -not $relative.StartsWith(
            $Goal143RootRelative + "/",
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must stay under the Goal143 output root: $relative"
    }

    return $full
}

function Resolve-Goal143UnityPath {
    param([string]$ExplicitPath)

    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {
        return [System.IO.Path]::GetFullPath($ExplicitPath)
    }

    $command = Get-Command "Unity.exe" -ErrorAction SilentlyContinue
    if ($null -ne $command -and -not [string]::IsNullOrWhiteSpace($command.Source)) {
        return [System.IO.Path]::GetFullPath($command.Source)
    }

    $candidates = @(
        "C:\Program Files\Unity\Hub\Editor\6000.1.10f1\Editor\Unity.exe",
        "C:\Program Files\Unity\Hub\Editor\6000.1.9f1\Editor\Unity.exe",
        "C:\Program Files\Unity\Hub\Editor\6000.0.43f1\Editor\Unity.exe"
    )
    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return $candidate
        }
    }

    return ""
}

function Write-Goal143Json {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)]$Value
    )

    $json = $Value | ConvertTo-Json -Depth 16
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($Path)) | Out-Null
    [System.IO.File]::WriteAllText(
        $Path,
        $json + [Environment]::NewLine,
        [System.Text.UTF8Encoding]::new($false))
}

function Copy-Goal143Directory {
    param(
        [Parameter(Mandatory=$true)][string]$Source,
        [Parameter(Mandatory=$true)][string]$Destination
    )

    if (-not (Test-Path -LiteralPath $Source -PathType Container)) {
        return
    }

    [System.IO.Directory]::CreateDirectory((Split-Path -Parent $Destination)) | Out-Null
    Copy-Item -LiteralPath $Source -Destination $Destination -Recurse -Force
}

function Remove-Goal143Directory {
    param([Parameter(Mandatory=$true)][string]$Path)

    if (Test-Path -LiteralPath $Path -PathType Container) {
        Remove-Item -LiteralPath $Path -Recurse -Force
    }
}

function Restore-Goal143Directory {
    param(
        [Parameter(Mandatory=$true)][string]$Destination,
        [Parameter(Mandatory=$true)][string]$Backup,
        [Parameter(Mandatory=$true)][bool]$PreviouslyExisted
    )

    Remove-Goal143Directory -Path $Destination
    if ($PreviouslyExisted) {
        Copy-Goal143Directory -Source $Backup -Destination $Destination
    }
}

function Test-Goal143ContainsBoolean {
    param(
        [string]$Text,
        [string]$Name,
        [bool]$Value
    )

    $expected = $Value.ToString()
    return $Text.Contains("$Name=$expected") `
        -or $Text.Contains("$Name=$($expected.ToLowerInvariant())")
}

function Assert-Goal143SelectedIntegrity {
    $handoff = Get-Content -LiteralPath $ResolvedSelectedHandoffPath -Raw -Encoding UTF8 |
        ConvertFrom-Json
    $outcome = Get-Content -LiteralPath $ResolvedSelectedOutcomePath -Raw -Encoding UTF8 |
        ConvertFrom-Json
    $roundtrip = Get-Content -LiteralPath $ResolvedSelectedRoundtripResultPath -Raw -Encoding UTF8 |
        ConvertFrom-Json
    $packageHash = ((Get-FileHash `
        -LiteralPath $ResolvedSelectedPackagePath `
        -Algorithm SHA256).Hash).ToLowerInvariant()
    $relativePackage = ConvertTo-Goal143RelativePath -Path $ResolvedSelectedPackagePath
    $relativeRoundtrip = ConvertTo-Goal143RelativePath -Path $ResolvedSelectedRoundtripResultPath
    $handoffOutcome = Resolve-Goal143InputPath `
        -Path $handoff.runtimeOutcomeSummaryPath `
        -Name "Goal142 handoff outcome"
    $handoffOutcomeHash = (Get-FileHash -LiteralPath $handoffOutcome -Algorithm SHA256).Hash
    $selectedOutcomeHash = (Get-FileHash -LiteralPath $ResolvedSelectedOutcomePath -Algorithm SHA256).Hash
    $finalHash = [string]$roundtrip.stateHashChain[-1]

    if ($handoff.candidateId -ne "minimal-map-game-exploration-resource-focus" `
        -or $handoff.recipeId -ne "exploration_resource_focus" `
        -or $handoff.variantKind -ne "exploration_resource_focus" `
        -or [int]$handoff.score -ne 100 `
        -or [bool]$handoff.accepted `
        -or -not [bool]$handoff.runtimeSignificant `
        -or [bool]$handoff.projectionOnly `
        -or -not [bool]$handoff.runtimeAuthority) {
        throw "Goal143 selected handoff identity or authority markers are invalid."
    }

    if ($handoff.packagePath -ne $relativePackage `
        -or $handoff.roundtripResultPath -ne $relativeRoundtrip) {
        throw "Goal143 selected handoff/package/result paths disagree."
    }

    if ($packageHash -ne $handoff.packageSha256) {
        throw "Goal143 selected package SHA-256 does not match Goal142 handoff."
    }

    if ($handoffOutcomeHash -ne $selectedOutcomeHash `
        -or $outcome.candidateId -ne $handoff.candidateId `
        -or $roundtrip.candidateId -ne $handoff.candidateId `
        -or $finalHash -ne $handoff.finalStateHash `
        -or $finalHash -ne $outcome.finalStateHash) {
        throw "Goal143 selected outcome or roundtrip does not match Goal142 handoff."
    }

    return $packageHash
}

function Invoke-Goal143CoreProof {
    param([bool]$RequireUnitySmoke)

    Push-Location $RepoRoot
    try {
        $env:LLMGC_GOAL143_SELECTED_HANDOFF_PATH = ConvertTo-Goal143RelativePath -Path $ResolvedSelectedHandoffPath
        $env:LLMGC_GOAL143_SELECTED_PACKAGE_PATH = ConvertTo-Goal143RelativePath -Path $ResolvedSelectedPackagePath
        $env:LLMGC_GOAL143_SELECTED_OUTCOME_PATH = ConvertTo-Goal143RelativePath -Path $ResolvedSelectedOutcomePath
        $env:LLMGC_GOAL143_SELECTED_ROUNDTRIP_RESULT_PATH = ConvertTo-Goal143RelativePath -Path $ResolvedSelectedRoundtripResultPath
        $env:LLMGC_GOAL143_OUTPUT_ROOT = ConvertTo-Goal143RelativePath -Path $ResolvedOutputRoot
        $env:LLMGC_GOAL143_UNITY_SMOKE_PATH = ConvertTo-Goal143RelativePath -Path $SmokePath
        $env:LLMGC_GOAL143_REQUIRE_UNITY_SMOKE = $RequireUnitySmoke.ToString().ToLowerInvariant()
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj `
            -c Debug `
            --filter "FullyQualifiedName~SelectedRuntimeVariantPlayerAdapterScriptProof"
        if ($LASTEXITCODE -ne 0) {
            throw "Goal143 Application proof failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Remove-Item Env:\LLMGC_GOAL143_SELECTED_HANDOFF_PATH -ErrorAction SilentlyContinue
        Remove-Item Env:\LLMGC_GOAL143_SELECTED_PACKAGE_PATH -ErrorAction SilentlyContinue
        Remove-Item Env:\LLMGC_GOAL143_SELECTED_OUTCOME_PATH -ErrorAction SilentlyContinue
        Remove-Item Env:\LLMGC_GOAL143_SELECTED_ROUNDTRIP_RESULT_PATH -ErrorAction SilentlyContinue
        Remove-Item Env:\LLMGC_GOAL143_OUTPUT_ROOT -ErrorAction SilentlyContinue
        Remove-Item Env:\LLMGC_GOAL143_UNITY_SMOKE_PATH -ErrorAction SilentlyContinue
        Remove-Item Env:\LLMGC_GOAL143_REQUIRE_UNITY_SMOKE -ErrorAction SilentlyContinue
        Pop-Location
    }
}

function Invoke-Goal143UnitySmoke {
    $logPath = Join-Path $ResolvedOutputRoot "unity-selected-runtime-variant-playeradapter-smoke.log"
    $modelHash = (Get-FileHash -LiteralPath $ModelPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $framesHash = (Get-FileHash -LiteralPath $FramesPath -Algorithm SHA256).Hash.ToLowerInvariant()
    if ([string]::IsNullOrWhiteSpace($ResolvedUnityPath) `
        -or -not (Test-Path -LiteralPath $ResolvedUnityPath -PathType Leaf)) {
        Write-Goal143Json -Path $SmokePath -Value ([ordered]@{
            schemaVersion = "unity_selected_runtime_variant_playeradapter_smoke_v1"
            goalId = "goal_143_selected_runtime_variant_end_to_end_playeradapter_handoff"
            status = "BLOCKED_UNITY_NOT_FOUND"
            unityAvailable = $false
            modelPathExists = $true
            framesPathExists = $true
            candidateIsGoal142Selection = $false
            selectedPackageSha256MatchesHandoff = $false
            selectedFinalStateHashMatches = $false
            frameCountPassed = $false
            selectedVariantEffectVisible = $false
            noBalancedBaselineFallback = $false
            runtimeAuthorityMarkersPresent = $false
            unityConsumesSelectedVariantPlayerAdapter = $false
            unityGameplayTruth = $false
            passMarkerPresent = $false
            failMarkerPresent = $false
            passed = $false
            unityExitCode = -1
            modelPath = ConvertTo-Goal143RelativePath -Path $ModelPath
            framesPath = ConvertTo-Goal143RelativePath -Path $FramesPath
            modelSha256 = $modelHash
            framesSha256 = $framesHash
            unityPath = $ResolvedUnityPath
            unityLogPath = ConvertTo-Goal143RelativePath -Path $logPath
            diagnostics = @("Unity executable was not found.")
        })
        return $false
    }

    $arguments = @(
        "-batchmode",
        "-quit",
        "-projectPath", $UnityProjectPath,
        "-executeMethod", $ExecuteMethod,
        "-logFile", $logPath,
        "-llmgcSelectedVariantPlayerAdapterModelPath", $ModelPath,
        "-llmgcSelectedVariantPlayerAdapterFramesPath", $FramesPath,
        "-llmgcSelectedVariantPlayerAdapterHandoffPath", $HandoffOutputPath
    )
    $unityProcess = Start-Process `
        -FilePath $ResolvedUnityPath `
        -ArgumentList $arguments `
        -WorkingDirectory $RepoRoot `
        -Wait `
        -PassThru `
        -WindowStyle Hidden
    $unityExitCode = $unityProcess.ExitCode

    $logText = if (Test-Path -LiteralPath $logPath -PathType Leaf) {
        Get-Content -LiteralPath $logPath -Raw -Encoding UTF8
    } else { "" }
    $passMarkerPresent = $logText.Contains($PassMarker)
    $failMarkerPresent = $logText.Contains($FailMarker)
    $checks = [ordered]@{
        candidateIsGoal142Selection = Test-Goal143ContainsBoolean $logText "candidateIsGoal142Selection" $true
        selectedPackageSha256MatchesHandoff = Test-Goal143ContainsBoolean $logText "selectedPackageSha256MatchesHandoff" $true
        selectedFinalStateHashMatches = Test-Goal143ContainsBoolean $logText "selectedFinalStateHashMatches" $true
        frameCountPassed = Test-Goal143ContainsBoolean $logText "frameCountPassed" $true
        selectedVariantEffectVisible = Test-Goal143ContainsBoolean $logText "selectedVariantEffectVisible" $true
        noBalancedBaselineFallback = Test-Goal143ContainsBoolean $logText "noBalancedBaselineFallback" $true
        runtimeAuthorityMarkersPresent = Test-Goal143ContainsBoolean $logText "runtimeAuthorityMarkersPresent" $true
        unityConsumesSelectedVariantPlayerAdapter = Test-Goal143ContainsBoolean $logText "unityConsumesSelectedVariantPlayerAdapter" $true
        unityGameplayTruth = Test-Goal143ContainsBoolean $logText "unityGameplayTruth" $true
    }
    $passed = $unityExitCode -eq 0 `
        -and $passMarkerPresent `
        -and -not $failMarkerPresent `
        -and $checks.candidateIsGoal142Selection `
        -and $checks.selectedPackageSha256MatchesHandoff `
        -and $checks.selectedFinalStateHashMatches `
        -and $checks.frameCountPassed `
        -and $checks.selectedVariantEffectVisible `
        -and $checks.noBalancedBaselineFallback `
        -and $checks.runtimeAuthorityMarkersPresent `
        -and $checks.unityConsumesSelectedVariantPlayerAdapter `
        -and -not $checks.unityGameplayTruth
    Write-Goal143Json -Path $SmokePath -Value ([ordered]@{
        schemaVersion = "unity_selected_runtime_variant_playeradapter_smoke_v1"
        goalId = "goal_143_selected_runtime_variant_end_to_end_playeradapter_handoff"
        status = if ($passed) { "GREEN" } else { "FAILED_UNITY_SMOKE" }
        unityAvailable = $true
        modelPathExists = Test-Path -LiteralPath $ModelPath -PathType Leaf
        framesPathExists = Test-Path -LiteralPath $FramesPath -PathType Leaf
        candidateIsGoal142Selection = $checks.candidateIsGoal142Selection
        selectedPackageSha256MatchesHandoff = $checks.selectedPackageSha256MatchesHandoff
        selectedFinalStateHashMatches = $checks.selectedFinalStateHashMatches
        frameCountPassed = $checks.frameCountPassed
        selectedVariantEffectVisible = $checks.selectedVariantEffectVisible
        noBalancedBaselineFallback = $checks.noBalancedBaselineFallback
        runtimeAuthorityMarkersPresent = $checks.runtimeAuthorityMarkersPresent
        unityConsumesSelectedVariantPlayerAdapter = $checks.unityConsumesSelectedVariantPlayerAdapter
        unityGameplayTruth = $checks.unityGameplayTruth
        passMarkerPresent = $passMarkerPresent
        failMarkerPresent = $failMarkerPresent
        passed = $passed
        unityExitCode = $unityExitCode
        modelPath = ConvertTo-Goal143RelativePath -Path $ModelPath
        framesPath = ConvertTo-Goal143RelativePath -Path $FramesPath
        modelSha256 = $modelHash
        framesSha256 = $framesHash
        unityPath = $ResolvedUnityPath
        unityLogPath = ConvertTo-Goal143RelativePath -Path $logPath
        diagnostics = @("unityExitCode=$unityExitCode")
    })
    return $passed
}

$ResolvedSelectedHandoffPath = Resolve-Goal143InputPath -Path $SelectedHandoffPath -Name "SelectedHandoffPath"
$ResolvedSelectedPackagePath = Resolve-Goal143InputPath -Path $SelectedPackagePath -Name "SelectedPackagePath"
$ResolvedSelectedOutcomePath = Resolve-Goal143InputPath -Path $SelectedOutcomePath -Name "SelectedOutcomePath"
$ResolvedSelectedRoundtripResultPath = Resolve-Goal143InputPath `
    -Path $SelectedRoundtripResultPath `
    -Name "SelectedRoundtripResultPath"
$ResolvedOutputRoot = Resolve-Goal143OutputRoot -Path $OutputRoot
$ResolvedExportRoot = Resolve-Goal143PathUnderRoot -Path $ExportRootRelative -Name "ExportRoot"
$ResolvedUnityPath = Resolve-Goal143UnityPath -ExplicitPath $UnityPath
$ModelPath = Join-Path $ResolvedOutputRoot "selected-runtime-variant-playeradapter-model.json"
$FramesPath = Join-Path $ResolvedOutputRoot "selected-runtime-variant-playeradapter-frames.json"
$HandoffOutputPath = Join-Path $ResolvedOutputRoot "selected-runtime-variant-playeradapter-handoff.json"
$SmokePath = Join-Path $ResolvedOutputRoot "unity-selected-runtime-variant-playeradapter-smoke.json"
$packageHash = Assert-Goal143SelectedIntegrity

if ($DryRun) {
    Write-Host "Goal143 selected runtime variant PlayerAdapter dry run passed."
    Write-Host "SelectedCandidateId=minimal-map-game-exploration-resource-focus"
    Write-Host "SelectedVariantKind=exploration_resource_focus"
    Write-Host "SelectedScore=100"
    Write-Host "SelectedPackageSha256=$packageHash"
    Write-Host "SelectedHandoffPath=$((ConvertTo-Goal143RelativePath $ResolvedSelectedHandoffPath))"
    Write-Host "SelectedPackagePath=$((ConvertTo-Goal143RelativePath $ResolvedSelectedPackagePath))"
    Write-Host "SelectedOutcomePath=$((ConvertTo-Goal143RelativePath $ResolvedSelectedOutcomePath))"
    Write-Host "SelectedRoundtripResultPath=$((ConvertTo-Goal143RelativePath $ResolvedSelectedRoundtripResultPath))"
    Write-Host "OutputRoot=$((ConvertTo-Goal143RelativePath $ResolvedOutputRoot))"
    Write-Host "UnityPath=$ResolvedUnityPath"
    Write-Host "UnityExecuteMethod=$ExecuteMethod"
    Write-Host "PassMarker=$PassMarker"
    Write-Host "FailMarker=$FailMarker"
    return
}

$backupRoot = [System.IO.Path]::GetFullPath((Join-Path `
    ([System.IO.Path]::GetTempPath()) `
    ("LLMGameCreator/goal143-script-" + [Guid]::NewGuid().ToString("N"))))
if (Test-Goal143PathUnderRoot -RootPath $RepoRoot -CandidatePath $backupRoot) {
    throw "Goal143 transaction backup must stay outside the repository: $backupRoot"
}

$proceduralBackup = Join-Path $backupRoot "procedural"
$exportBackup = Join-Path $backupRoot "export"
$proceduralExisted = Test-Path -LiteralPath $ResolvedOutputRoot -PathType Container
$exportExisted = Test-Path -LiteralPath $ResolvedExportRoot -PathType Container
[System.IO.Directory]::CreateDirectory($backupRoot) | Out-Null
Copy-Goal143Directory -Source $ResolvedOutputRoot -Destination $proceduralBackup
Copy-Goal143Directory -Source $ResolvedExportRoot -Destination $exportBackup

try {
    if ($ApplyCleanup) {
        Remove-Goal143Directory -Path $ResolvedOutputRoot
        Remove-Goal143Directory -Path $ResolvedExportRoot
    }

    Invoke-Goal143CoreProof -RequireUnitySmoke $false
    $unityPassed = Invoke-Goal143UnitySmoke
    if (-not $unityPassed) {
        if (Test-Path -LiteralPath $SmokePath -PathType Leaf) {
            Write-Host (Get-Content -LiteralPath $SmokePath -Raw -Encoding UTF8)
        }
        $unityLogPath = Join-Path `
            $ResolvedOutputRoot `
            "unity-selected-runtime-variant-playeradapter-smoke.log"
        if (Test-Path -LiteralPath $unityLogPath -PathType Leaf) {
            Get-Content -LiteralPath $unityLogPath -Encoding UTF8 -Tail 120 |
                ForEach-Object { Write-Host $_ }
        }
        throw "Goal143 Unity/player consumer smoke failed."
    }

    Invoke-Goal143CoreProof -RequireUnitySmoke $true
    if ($ApplyCleanup) {
        & (Join-Path $RepoRoot ".devflow/scripts/clean-unity-editor-noise.ps1") -Apply
        if ($LASTEXITCODE -ne 0) {
            throw "Goal143 Unity cleanup failed with exit code $LASTEXITCODE."
        }
    }
}
catch {
    Restore-Goal143Directory `
        -Destination $ResolvedOutputRoot `
        -Backup $proceduralBackup `
        -PreviouslyExisted $proceduralExisted
    Restore-Goal143Directory `
        -Destination $ResolvedExportRoot `
        -Backup $exportBackup `
        -PreviouslyExisted $exportExisted
    throw
}
finally {
    Remove-Goal143Directory -Path $backupRoot
}

Write-Host "GOAL143_SELECTED_RUNTIME_VARIANT_PLAYERADAPTER_GREEN"
Write-Host "SelectedCandidateId=minimal-map-game-exploration-resource-focus"
Write-Host "SelectedVariantKind=exploration_resource_focus"
Write-Host "SelectedScore=100"
Write-Host "SelectedPackageSha256MatchesHandoff=true"
Write-Host "SelectedFinalStateHashMatches=true"
Write-Host "RuntimeAuthority=true"
Write-Host "ProjectionOnly=false"
Write-Host "UnityGameplayTruth=false"
