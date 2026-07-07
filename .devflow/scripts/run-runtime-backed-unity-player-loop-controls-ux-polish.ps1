param(
    [string]$InteractiveControlsModelPath = ".llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/runtime-backed-player-loop-interactive-controls-model.json",
    [string]$InteractiveControlsResultPath = ".llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/runtime-backed-player-loop-interactive-controls-result.json",
    [string]$InteractiveControlsScriptPath = ".llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/runtime-backed-player-loop-interactive-controls-script.json",
    [string]$OutputRoot = ".llmgc/procedural/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard",
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
$Goal140RootRelative = ".llmgc/procedural/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard"
$ExportRootRelative = ".llmgc/exports/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard"
$UnityProjectPath = Join-Path $RepoRoot "unity/LLMGameCreatorAlpha"
$FallbackUnityPath = "C:\Program Files\Unity\Hub\Editor\6000.1.10f1\Editor\Unity.exe"
$ExecuteMethod = "LLMGameCreatorAlpha.CanonicalRuntimeUnityPlayerLoopInteractiveControlsHarness.RunBatchmodeRuntimeBackedUnityPlayerLoopControlsUxSmoke"
$PassMarker = "GOAL140_RUNTIME_BACKED_UNITY_PLAYER_LOOP_CONTROLS_UX_PASS"
$FailMarker = "GOAL140_RUNTIME_BACKED_UNITY_PLAYER_LOOP_CONTROLS_UX_FAIL"
$CleanupScript = Join-Path $RepoRoot ".devflow/scripts/clean-unity-editor-noise.ps1"

function Test-Goal140PathUnderRoot {
    param(
        [Parameter(Mandatory=$true)][string]$RootPath,
        [Parameter(Mandatory=$true)][string]$CandidatePath
    )

    $root = [System.IO.Path]::GetFullPath($RootPath).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    $candidate = [System.IO.Path]::GetFullPath($CandidatePath)
    return $candidate.StartsWith(
        $root + [System.IO.Path]::DirectorySeparatorChar,
        [System.StringComparison]::OrdinalIgnoreCase)
}

function ConvertTo-Goal140RelativePath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $full = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith($root + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart([System.IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }

    return $full
}

function Resolve-Goal140InputPath {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$Name
    )

    $candidate = if ([System.IO.Path]::IsPathRooted($Path)) {
        $Path
    }
    else {
        Join-Path $RepoRoot $Path
    }

    $full = [System.IO.Path]::GetFullPath($candidate)
    if (-not (Test-Goal140PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "$Name must stay under the repository root: $Path"
    }

    $relative = ConvertTo-Goal140RelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Name must not point under .llmgc/manual: $relative"
    }

    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) {
        throw "$Name does not exist: $relative"
    }

    return $full
}

function Resolve-Goal140OutputRoot {
    param([Parameter(Mandatory=$true)][string]$Path)

    $candidate = if ([System.IO.Path]::IsPathRooted($Path)) {
        $Path
    }
    else {
        Join-Path $RepoRoot $Path
    }

    $full = [System.IO.Path]::GetFullPath($candidate)
    if (-not (Test-Goal140PathUnderRoot -RootPath $RepoRoot -CandidatePath $full)) {
        throw "OutputRoot must stay under the repository root: $Path"
    }

    $relative = ConvertTo-Goal140RelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must not point under .llmgc/manual: $relative"
    }

    if (-not $relative.StartsWith($Goal140RootRelative + "/", [System.StringComparison]::OrdinalIgnoreCase) `
        -and -not $relative.Equals($Goal140RootRelative, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must stay under the Goal140 output root: $relative"
    }

    return $full
}

function Resolve-Goal140UnityPath {
    param([string]$ExplicitPath)

    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {
        return [System.IO.Path]::GetFullPath($ExplicitPath)
    }

    $command = Get-Command "Unity.exe" -ErrorAction SilentlyContinue
    if ($null -ne $command -and -not [string]::IsNullOrWhiteSpace($command.Source)) {
        return [System.IO.Path]::GetFullPath($command.Source)
    }

    return $FallbackUnityPath
}

function Write-Goal140Json {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)]$Value
    )

    $json = $Value | ConvertTo-Json -Depth 12
    $encoding = New-Object System.Text.UTF8Encoding -ArgumentList $false
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($Path)) | Out-Null
    [System.IO.File]::WriteAllText($Path, $json + [Environment]::NewLine, $encoding)
}

function Count-Goal140TextOccurrences {
    param(
        [Parameter(Mandatory=$true)][AllowEmptyString()][string]$Text,
        [Parameter(Mandatory=$true)][string]$Needle
    )

    if ([string]::IsNullOrEmpty($Text) -or [string]::IsNullOrEmpty($Needle)) {
        return 0
    }

    $count = 0
    $index = 0
    while ($index -ge 0) {
        $index = $Text.IndexOf($Needle, $index, [System.StringComparison]::Ordinal)
        if ($index -ge 0) {
            $count++
            $index += $Needle.Length
        }
    }

    return $count
}

function Test-Goal140Contains {
    param(
        [Parameter(Mandatory=$true)][AllowEmptyString()][string]$Text,
        [Parameter(Mandatory=$true)][string]$Needle
    )

    return $Text.IndexOf($Needle, [System.StringComparison]::Ordinal) -ge 0
}

function Get-Goal140UnityEditorNoiseCounts {
    param([Parameter(Mandatory=$true)][AllowEmptyString()][string]$LogText)

    $nullRefs = Count-Goal140TextOccurrences -Text $LogText -Needle "NullReferenceException"
    $known = 0
    $diagnostics = New-Object System.Collections.Generic.List[string]
    if ((Test-Goal140Contains -Text $LogText -Needle "BuildProfileContext") `
        -and (Test-Goal140Contains -Text $LogText -Needle "CreateOrLoad") `
        -and $nullRefs -gt 0) {
        $known = 1
        $diagnostics.Add("knownUnityEditorBuildProfileNoise=classified") | Out-Null
    }

    $unclassified = [Math]::Max(0, $nullRefs - $known)
    $blocking = $unclassified
    if ($unclassified -gt 0) {
        $diagnostics.Add("unpairedNullReferenceException=$unclassified") | Out-Null
    }

    if (Test-Goal140Contains -Text $LogText -Needle $FailMarker) {
        $blocking++
        $diagnostics.Add("goal140FailMarkerPresent=true") | Out-Null
    }

    if ((Test-Goal140Contains -Text $LogText -Needle "CanonicalRuntimeUnityPlayerLoopInteractiveControls") `
        -and (Test-Goal140Contains -Text $LogText -Needle "Exception") `
        -and $known -eq 0) {
        $blocking++
        $diagnostics.Add("playerLoopHarnessExceptionPresent=true") | Out-Null
    }

    if ($diagnostics.Count -eq 0) {
        $diagnostics.Add("unityLogNoKnownBlockingNoiseMarkers=true") | Out-Null
    }

    return [pscustomobject]@{
        knownUnityEditorNoiseCount = $known
        blockingUnityErrorCount = $blocking
        unclassifiedUnityErrorCount = $unclassified
        diagnostics = @($diagnostics)
    }
}

function Write-Goal140UnityNoiseClassification {
    param(
        [Parameter(Mandatory=$true)][AllowEmptyString()][string]$LogText,
        [Parameter(Mandatory=$true)][string]$SourceLogPath,
        [Parameter(Mandatory=$true)][string]$ClassificationPath
    )

    $fixture = "BuildProfileContext asset exists but could not be loaded`nNullReferenceException: Object reference not set to an instance of an object`nUnityEditor.Build.Profile.BuildProfileContext.CreateOrLoad"
    $fixtureCounts = Get-Goal140UnityEditorNoiseCounts -LogText $fixture
    $actualCounts = Get-Goal140UnityEditorNoiseCounts -LogText $LogText
    $knownClassified = $fixtureCounts.knownUnityEditorNoiseCount -gt 0
    $classification = [ordered]@{
        goalId = "goal_140_runtime_backed_unity_player_loop_controls_ux_polish_and_noise_guard"
        knownUnityEditorBuildProfileNoiseClassified = $knownClassified
        knownUnityEditorNoiseCount = $actualCounts.knownUnityEditorNoiseCount
        blockingUnityErrorCount = $actualCounts.blockingUnityErrorCount
        unclassifiedUnityErrorCount = $actualCounts.unclassifiedUnityErrorCount
        fixtureKnownUnityEditorBuildProfileNoiseClassified = $knownClassified
        sourceLogPath = $SourceLogPath
        knownMarkers = @("BuildProfileContext", "CreateOrLoad", "NullReferenceException")
        blockingMarkers = @($FailMarker, "unpaired NullReferenceException", "player-loop exception")
        diagnostics = @($actualCounts.diagnostics)
        passed = $knownClassified `
            -and $actualCounts.blockingUnityErrorCount -eq 0 `
            -and $actualCounts.unclassifiedUnityErrorCount -eq 0
    }
    Write-Goal140Json -Path $ClassificationPath -Value $classification
}

function Invoke-Goal140RuntimeProof {
    param(
        [string]$UnitySmokePath,
        [string]$UnityNoiseClassificationPath
    )

    $env:LLMGC_GOAL140_INTERACTIVE_CONTROLS_MODEL_PATH = $ResolvedInteractiveControlsModelPath
    $env:LLMGC_GOAL140_INTERACTIVE_CONTROLS_RESULT_PATH = $ResolvedInteractiveControlsResultPath
    $env:LLMGC_GOAL140_INTERACTIVE_CONTROLS_SCRIPT_PATH = $ResolvedInteractiveControlsScriptPath
    $env:LLMGC_GOAL140_OUTPUT_ROOT = $ResolvedOutputRoot
    if ([string]::IsNullOrWhiteSpace($UnitySmokePath)) {
        Remove-Item Env:\LLMGC_GOAL140_UNITY_SMOKE_PATH -ErrorAction SilentlyContinue
    }
    else {
        $env:LLMGC_GOAL140_UNITY_SMOKE_PATH = $UnitySmokePath
    }

    if ([string]::IsNullOrWhiteSpace($UnityNoiseClassificationPath)) {
        Remove-Item Env:\LLMGC_GOAL140_UNITY_NOISE_CLASSIFICATION_PATH -ErrorAction SilentlyContinue
    }
    else {
        $env:LLMGC_GOAL140_UNITY_NOISE_CLASSIFICATION_PATH = $UnityNoiseClassificationPath
    }

    Push-Location $RepoRoot
    try {
        & dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter "FullyQualifiedName~RuntimeBackedUnityPlayerLoopControlsUxPolishScriptProof"
        if ($LASTEXITCODE -ne 0) {
            throw "Goal140 runtime-backed Unity player-loop controls UX proof test failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}

function Invoke-Goal140UnitySmoke {
    param(
        [Parameter(Mandatory=$true)][string]$ResolvedUnityPath,
        [Parameter(Mandatory=$true)][string]$ModelPath,
        [Parameter(Mandatory=$true)][string]$ScriptPath,
        [Parameter(Mandatory=$true)][string]$SmokePath
    )

    $logPath = Join-Path $ResolvedOutputRoot "unity-player-loop-controls-ux-smoke.log"
    if (-not (Test-Path -LiteralPath $ResolvedUnityPath -PathType Leaf)) {
        $smoke = [ordered]@{
            goalId = "goal_140_runtime_backed_unity_player_loop_controls_ux_polish_and_noise_guard"
            unityAvailable = $false
            modelPathExists = Test-Path -LiteralPath $ModelPath -PathType Leaf
            frameCountPassed = $false
            requiredControlsPresent = $false
            humanReadableFrameNumberingPresent = $false
            stepOnceSemanticsClear = $false
            playAllToEndSemanticsClear = $false
            copyFrameSummaryStatusPresent = $false
            runtimeAuthorityMarkersPresent = $false
            unityGameplayTruth = $false
            passed = $false
            unityPath = $ResolvedUnityPath
            unityLogPath = ConvertTo-Goal140RelativePath -Path $logPath
            modelPath = ConvertTo-Goal140RelativePath -Path $ModelPath
            scriptPath = ConvertTo-Goal140RelativePath -Path $ScriptPath
            status = "BLOCKED_UNITY_NOT_FOUND"
            diagnostics = @("Unity executable was not found through explicit path, PATH, or known fallback.")
        }
        Write-Goal140Json -Path $SmokePath -Value $smoke
        Write-Goal140UnityNoiseClassification -LogText "" -SourceLogPath (ConvertTo-Goal140RelativePath -Path $logPath) -ClassificationPath $NoiseClassificationPath
        return $false
    }

    $args = @(
        "-batchmode",
        "-quit",
        "-projectPath",
        $UnityProjectPath,
        "-executeMethod",
        $ExecuteMethod,
        "-logFile",
        $logPath,
        "-llmgcRuntimeBackedUnityPlayerLoopControlsUxModelPath",
        $ModelPath,
        "-llmgcRuntimeBackedUnityPlayerLoopControlsUxScriptPath",
        $ScriptPath
    )

    Push-Location $RepoRoot
    try {
        & $ResolvedUnityPath @args
        $unityExitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }

    $logText = ""
    if (Test-Path -LiteralPath $logPath) {
        $logText = Get-Content -LiteralPath $logPath -Raw -Encoding UTF8
    }

    $passMarkerPresent = Test-Goal140Contains -Text $logText -Needle $PassMarker
    $failMarkerPresent = Test-Goal140Contains -Text $logText -Needle $FailMarker
    $modelPathExists = Test-Goal140Contains -Text $logText -Needle "modelPathExists=True"
    $frameCountPassed = (Test-Goal140Contains -Text $logText -Needle "frameCountPassed=True") `
        -or (Test-Goal140Contains -Text $logText -Needle "frameCountPassed=true")
    $requiredControlsPresent = (Test-Goal140Contains -Text $logText -Needle "requiredControlsPresent=True") `
        -or (Test-Goal140Contains -Text $logText -Needle "requiredControlsPresent=true")
    $humanReadableFrameNumberingPresent = (Test-Goal140Contains -Text $logText -Needle "humanReadableFrameNumberingPresent=True") `
        -or (Test-Goal140Contains -Text $logText -Needle "humanReadableFrameNumberingPresent=true")
    $stepOnceSemanticsClear = (Test-Goal140Contains -Text $logText -Needle "stepOnceSemanticsClear=True") `
        -or (Test-Goal140Contains -Text $logText -Needle "stepOnceSemanticsClear=true")
    $playAllToEndSemanticsClear = (Test-Goal140Contains -Text $logText -Needle "playAllToEndSemanticsClear=True") `
        -or (Test-Goal140Contains -Text $logText -Needle "playAllToEndSemanticsClear=true")
    $copyFrameSummaryStatusPresent = (Test-Goal140Contains -Text $logText -Needle "copyFrameSummaryStatusPresent=True") `
        -or (Test-Goal140Contains -Text $logText -Needle "copyFrameSummaryStatusPresent=true")
    $runtimeAuthorityMarkersPresent = (Test-Goal140Contains -Text $logText -Needle "runtimeAuthorityMarkersPresent=True") `
        -or (Test-Goal140Contains -Text $logText -Needle "runtimeAuthorityMarkersPresent=true")
    $passed = $unityExitCode -eq 0 `
        -and $passMarkerPresent `
        -and -not $failMarkerPresent `
        -and $modelPathExists `
        -and $frameCountPassed `
        -and $requiredControlsPresent `
        -and $humanReadableFrameNumberingPresent `
        -and $stepOnceSemanticsClear `
        -and $playAllToEndSemanticsClear `
        -and $copyFrameSummaryStatusPresent `
        -and $runtimeAuthorityMarkersPresent
    $smoke = [ordered]@{
        goalId = "goal_140_runtime_backed_unity_player_loop_controls_ux_polish_and_noise_guard"
        unityAvailable = $true
        modelPathExists = $modelPathExists
        frameCountPassed = $frameCountPassed
        requiredControlsPresent = $requiredControlsPresent
        humanReadableFrameNumberingPresent = $humanReadableFrameNumberingPresent
        stepOnceSemanticsClear = $stepOnceSemanticsClear
        playAllToEndSemanticsClear = $playAllToEndSemanticsClear
        copyFrameSummaryStatusPresent = $copyFrameSummaryStatusPresent
        runtimeAuthorityMarkersPresent = $runtimeAuthorityMarkersPresent
        unityGameplayTruth = $false
        passed = $passed
        unityPath = $ResolvedUnityPath
        unityLogPath = ConvertTo-Goal140RelativePath -Path $logPath
        modelPath = ConvertTo-Goal140RelativePath -Path $ModelPath
        scriptPath = ConvertTo-Goal140RelativePath -Path $ScriptPath
        status = if ($passed) { "GREEN" } else { "FAILED_UNITY_PLAYER_LOOP_CONTROLS_UX_SMOKE" }
        diagnostics = @("unityExitCode=$unityExitCode")
    }
    Write-Goal140Json -Path $SmokePath -Value $smoke
    Write-Goal140UnityNoiseClassification -LogText $logText -SourceLogPath (ConvertTo-Goal140RelativePath -Path $logPath) -ClassificationPath $NoiseClassificationPath
    return $passed
}

$ResolvedInteractiveControlsModelPath = Resolve-Goal140InputPath -Path $InteractiveControlsModelPath -Name "InteractiveControlsModelPath"
$ResolvedInteractiveControlsResultPath = Resolve-Goal140InputPath -Path $InteractiveControlsResultPath -Name "InteractiveControlsResultPath"
$ResolvedInteractiveControlsScriptPath = Resolve-Goal140InputPath -Path $InteractiveControlsScriptPath -Name "InteractiveControlsScriptPath"
$ResolvedOutputRoot = Resolve-Goal140OutputRoot -Path $OutputRoot
$ResolvedUnityPath = Resolve-Goal140UnityPath -ExplicitPath $UnityPath
$ModelPath = Join-Path $ResolvedOutputRoot "runtime-backed-player-loop-controls-ux-model.json"
$ScriptOutputPath = Join-Path $ResolvedOutputRoot "runtime-backed-player-loop-controls-ux-script.json"
$SmokePath = Join-Path $ResolvedOutputRoot "unity-player-loop-controls-ux-smoke.json"
$NoiseClassificationPath = Join-Path $ResolvedOutputRoot "unity-editor-noise-classification.json"

if ($DryRun) {
    Write-Host "Goal140 runtime-backed Unity player-loop controls UX polish dry run"
    Write-Host "RepoRoot: $RepoRoot"
    Write-Host "InteractiveControlsModelPath: $ResolvedInteractiveControlsModelPath"
    Write-Host "InteractiveControlsResultPath: $ResolvedInteractiveControlsResultPath"
    Write-Host "InteractiveControlsScriptPath: $ResolvedInteractiveControlsScriptPath"
    Write-Host "OutputRoot: $ResolvedOutputRoot"
    Write-Host "UnityPath: $ResolvedUnityPath"
    Write-Host "RuntimeProof: dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter FullyQualifiedName~RuntimeBackedUnityPlayerLoopControlsUxPolishScriptProof"
    Write-Host "UnityExecuteMethod: $ExecuteMethod"
    Write-Host "PassMarker: $PassMarker"
    Write-Host "FailMarker: $FailMarker"
    Write-Host "NoiseGuard: BuildProfileContext/CreateOrLoad NullReferenceException classified as knownUnityEditorBuildProfileNoise; unpaired NullReferenceException is blocking"
    Write-Host "ExportRoot: $ExportRootRelative"
    return
}

Invoke-Goal140RuntimeProof -UnitySmokePath "" -UnityNoiseClassificationPath ""
$unityPassed = Invoke-Goal140UnitySmoke `
    -ResolvedUnityPath $ResolvedUnityPath `
    -ModelPath $ModelPath `
    -ScriptPath $ScriptOutputPath `
    -SmokePath $SmokePath
Invoke-Goal140RuntimeProof -UnitySmokePath $SmokePath -UnityNoiseClassificationPath $NoiseClassificationPath

if ($ApplyCleanup) {
    & $CleanupScript -Apply
    if ($LASTEXITCODE -ne 0) {
        $firstCleanupExitCode = $LASTEXITCODE
        Start-Sleep -Seconds 1
        & $CleanupScript -Apply
        if ($LASTEXITCODE -ne 0) {
            throw "Unity cleanup failed with exit code $LASTEXITCODE after retry; first exit code was $firstCleanupExitCode."
        }
    }
}

if (-not $unityPassed) {
    throw "Goal140 Unity/player loop controls UX smoke did not pass. See $(ConvertTo-Goal140RelativePath -Path $SmokePath)."
}

Write-Host "Goal140 runtime-backed Unity player-loop controls UX polish passed."
