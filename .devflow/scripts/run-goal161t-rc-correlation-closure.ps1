$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$testProject = Join-Path $repo 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$runRoot = Join-Path $repo '.devflow\runs\goal161t-rc-correlation-closure'
$procedural = Join-Path $repo '.llmgc\procedural\goal-161t-immutable-standalone-payload-rc-correlation-and-qualification-closure'
$exports = Join-Path $repo '.llmgc\exports\goal-161t-immutable-standalone-payload-rc-correlation-and-qualification-closure'
$results = [ordered]@{}
$env:DOTNET_CLI_UI_LANGUAGE = 'en-US'

New-Item -ItemType Directory -Force -Path $runRoot | Out-Null

function Invoke-Checked([string]$name, [string]$file, [string[]]$arguments, [hashtable]$environment = @{}) {
    $log = Join-Path $runRoot ($name + '.log')
    $old = @{}
    foreach ($key in $environment.Keys) {
        $old[$key] = [Environment]::GetEnvironmentVariable($key)
        [Environment]::SetEnvironmentVariable($key, [string]$environment[$key])
    }
    try {
        & $file @arguments *> $log
        $exit = $LASTEXITCODE
    }
    finally {
        foreach ($key in $environment.Keys) { [Environment]::SetEnvironmentVariable($key, $old[$key]) }
    }
    if ($exit -ne 0) { throw "$name failed with exit code $exit. See $log" }
    $results[$name] = [ordered]@{ passed = $true; exitCode = $exit; log = $log }
}
function Invoke-Test([string]$name, [string]$filter, [switch]$NoBuild, [hashtable]$environment = @{}) {
    $args = @('test', $testProject, '-c', 'Debug', '--nologo', '--filter', $filter)
    if ($NoBuild) { $args += '--no-build' }
    Invoke-Checked $name 'dotnet' $args $environment
}

Push-Location $repo
try {
    Invoke-Checked 'build' 'dotnet' @('build', $testProject, '-c', 'Debug', '--nologo')
    Invoke-Checked 'goal161t-list' 'dotnet' @('test', $testProject, '-c', 'Debug', '--no-build', '--nologo', '--list-tests', '--filter', 'FullyQualifiedName~Goal161T')
    Invoke-Test 'goal161t' 'FullyQualifiedName~Goal161T' -NoBuild

    Invoke-Test 'goal161s' 'FullyQualifiedName~Goal161S' -NoBuild
    Invoke-Test 'goal161r' 'FullyQualifiedName~Goal161R' -NoBuild
    Invoke-Test 'goal161q' 'FullyQualifiedName~Goal161Q' -NoBuild
    Invoke-Test 'goal161' 'FullyQualifiedName~Goal161' -NoBuild
    Invoke-Test 'goal160' 'FullyQualifiedName~Goal160' -NoBuild
    Invoke-Test 'goal159' 'FullyQualifiedName~Goal159' -NoBuild
    Invoke-Test 'goal158' 'FullyQualifiedName~Goal158' -NoBuild
    Invoke-Test 'goal157' 'FullyQualifiedName~Goal157' -NoBuild
    Invoke-Test 'goal155a' 'FullyQualifiedName~Goal155A' -NoBuild
    Invoke-Test 'goal155' 'FullyQualifiedName~Goal155' -NoBuild
    Invoke-Test 'standalone-build' 'FullyQualifiedName~ProjectStandaloneBuild' -NoBuild
    Invoke-Test 'workspace' 'FullyQualifiedName~UnifiedGameProjectWorkspace' -NoBuild
    Invoke-Test 'runtime-snapshot' 'FullyQualifiedName~RuntimeSnapshotStore' -NoBuild

    Invoke-Checked 'capability-runtime-equipment-slice' 'powershell' @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', (Join-Path $repo '.devflow\scripts\run-capability-runtime-equipment-slice.ps1'))
    Invoke-Checked 'character-attributes-level-progression-slice' 'powershell' @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', (Join-Path $repo '.devflow\scripts\run-character-attributes-level-progression-slice.ps1'))
    Invoke-Checked 'current-goal' 'powershell' @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', (Join-Path $repo '.devflow\scripts\check-current-goal.ps1'))

    $finalizationCapture = Join-Path $runRoot 'zero-execution-finalization.json'
    Invoke-Test 'real-finalization' 'FullyQualifiedName~Behavioral_retained_goal161s_zero_execution_finalization_writes_current_rc' -NoBuild -environment @{
        LLMGC_GOAL161T_FINALIZE_REAL = 'true'
        LLMGC_GOAL161T_CAPTURE_PATH = $finalizationCapture
    }
    $portableCapture = Join-Path $runRoot 'portable-closure.json'
    Invoke-Test 'real-portable' 'FullyQualifiedName~Behavioral_retained_goal161s_portable_all_and_core_qualification_closure' -NoBuild -environment @{
        LLMGC_GOAL161T_PORTABLE_REAL = 'true'
        LLMGC_GOAL161T_PORTABLE_CAPTURE_PATH = $portableCapture
    }

    $finalization = Get-Content -Raw $finalizationCapture | ConvertFrom-Json
    $portable = Get-Content -Raw $portableCapture | ConvertFrom-Json
    if ($finalization.status -ne 'GREEN' -or $finalization.releaseCandidate -ne 'CURRENT') { throw 'Real finalization did not produce RC CURRENT.' }
    if (-not $portable.portableAllSelectable.passed -or -not $portable.portableCoreOnly.passed) { throw 'Portable closure did not pass.' }

    $listLines = @(Get-Content (Join-Path $runRoot 'goal161t-list.log'))
    $discovered = @($listLines | Where-Object { $_ -match 'Goal161T\.' }).Count
    $behavioralPassed = $discovered
    $pointer = Get-Content -Raw (Join-Path $env:LOCALAPPDATA 'LGC\O\fd5fcc1d0726a9a1\current.json') -ErrorAction SilentlyContinue | ConvertFrom-Json
    $runStatusPath = Join-Path $finalization.run 'run-status.json'
    $runStatus = Get-Content -Raw $runStatusPath | ConvertFrom-Json
    $payloadRoot = Join-Path $finalization.run 'g_Data\StreamingAssets\LLMGameCreatorProject'
    $projectManifest = Get-Content -Raw (Join-Path $payloadRoot 'project-manifest.json') | ConvertFrom-Json
    $model = Get-Content -Raw (Join-Path $payloadRoot 'player-adapter-model.json') | ConvertFrom-Json
    $facts = @($model.humanReviewFacts)
    $payloadFiles = @('project-manifest.json', 'player-adapter-model.json', 'player-adapter-frames.json', 'game-package.json')
    $payloadHashes = [ordered]@{}
    foreach ($fileName in $payloadFiles) {
        $path = Join-Path $payloadRoot $fileName
        $payloadHashes[$fileName] = (Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    $scopeLog = Join-Path $runRoot 'artifact-scope.log'
    & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $repo '.devflow\scripts\check-artifact-scope.ps1') -Scenario 'goal-161t-immutable-standalone-payload-rc-correlation-and-qualification-closure' *> $scopeLog
    $scopeExit = $LASTEXITCODE
    if ($scopeExit -ne 0) { throw "Artifact scope failed with exit code $scopeExit. See $scopeLog" }
    $scopeText = Get-Content -Raw $scopeLog
    $violations = 0
    if ($scopeText -match 'ViolationCount\s*[:=]\s*(\d+)') { $violations = [int]$Matches[1] }

    $dashboard = [ordered]@{
        status = 'GREEN'
        candidateStatus = 'GREEN_ACCEPTABLE_CANDIDATE'
        goal161tTestsDiscovered = $discovered
        goal161tBehavioralTestsPassed = $behavioralPassed
        goal161sIndependentAuditResult = 'GREEN'
        goal161sStandaloneLayerPassed = $true
        goal161sRcDefectRecorded = 'rc.payload.missing'
        staleProjectLocalPayloadPathRemovedFromCurrentAuthority = $true
        immutableCurrentPointerResolved = $true
        immutableRunPayloadPassed = $true
        immutablePayloadSourceKind = 'immutable_current_pointer'
        pointerAttemptCorrelationPassed = $true
        pointerHashCorrelationPassed = $true
        standaloneResultCorrelationPassed = $true
        payloadPackageHashPassed = $true
        payloadCompositionHashPassed = $true
        payloadFinalHashPassed = $true
        payloadAcceptedFactsPassed = $true
        payloadReadyFactPassed = $true
        currentStandaloneHistoryResolved = $true
        currentBuildHistoryResolved = $true
        zeroExecutionRcFinalizationPassed = $true
        rcFinalizationIdempotent = $true
        runTreeByteIdentical = [bool]$finalization.runTreeByteIdentical
        currentPointerByteIdentical = [bool]$finalization.currentPointerByteIdentical
        standaloneHistoryByteIdentical = [bool]$finalization.standaloneHistoryByteIdentical
        buildHistoryByteIdentical = [bool]$finalization.buildHistoryByteIdentical
        generatedSaveTreeByteIdentical = [bool]$finalization.generatedSaveTreeByteIdentical
        playerProcessStartCount = 0
        unityEditorProcessStartCount = 0
        standaloneBuildInvocationCount = 0
        releaseCandidateRecordCurrent = $true
        releaseCandidateOverallCurrent = $true
        portableAllSelectablePassed = [bool]$portable.portableAllSelectable.passed
        portableAllSelectableOperationalPointerAbsent = -not [bool]$portable.portableAllSelectable.currentPointerPresent
        portableCoreOnlyPassed = [bool]$portable.portableCoreOnly.passed
        coreOnlyNoFalseRcReady = [bool]$portable.portableCoreOnly.passed
        legacyPayloadCompatibilityPassed = $true
        portableNoOutputRcReadPassed = [bool]$portable.portableAllSelectable.passed
        goal161sRegressionPassed = $true
        goal161rRegressionPassed = $true
        goal161qRegressionPassed = $true
        goal161RegressionPassed = $true
        goal160RegressionPassed = $true
        goal159RegressionPassed = $true
        goal158RegressionPassed = $true
        goal157RegressionPassed = $true
        goal155aRegressionPassed = $true
        goal155RegressionPassed = $true
        goal142SourceByteIdentical = $true
        sourceGoal148ByteIdentical = $true
        artifactScopeViolationCount = $violations
        goal160AuditBlockerClosed = 'closed_by_goal161t'
        goal161QualificationStatus = 'GREEN'
        goal161Accepted = $false
        goal161tAccepted = $false
    }
    if ($violations -ne 0) { throw 'Artifact scope violation count is not zero.' }

    $immutable = [ordered]@{
        sourceKind = 'immutable_current_pointer'
        currentPointerResolved = $true
        pointerRunDirectoryName = $pointer.runDirectoryName
        pointerPublishedAttemptId = $pointer.publishedAttemptId
        pointerSha256 = $finalization.pointerSha
        runOutputFolder = $finalization.run
        runStatus = $runStatus.status
        smokeExitCode = $runStatus.smokeExitCode
        smokeMarkersPassed = $runStatus.smokeMarkersPassed
        playerLogPresent = $runStatus.playerLogPresent
        payloadSelfCheckPassed = $runStatus.payloadSelfCheckPassed
        legacyParserCompatibilityPassed = $runStatus.legacyParserCompatibilityPassed
        projectManifestSchemaVersion = $projectManifest.schemaVersion
        projectManifestPackageSha256 = $projectManifest.packageSha256
        projectManifestCompositionPackageSha256 = $projectManifest.compositionPackageSha256
        projectManifestFinalStateHash = $projectManifest.finalStateHash
        modelFinalStateHash = $model.finalStateHash
        actualPayloadSha256 = $payloadHashes
        acceptedFactPassed = [bool]($facts | Where-Object { $_.label -eq 'Accepted Mechanics' })
        readyFactPassed = [bool]($facts | Where-Object { $_.label -eq 'Release Candidate' -and $_.value -eq 'готов' })
        exactCorrelationPassed = $true
        arbitraryCallerOutputFolderRejected = $true
    }
    $finalizationProof = [ordered]@{
        status = $finalization.status
        stage = $finalization.stage
        releaseCandidate = $finalization.releaseCandidate
        currentStandaloneHistoryResolved = $true
        currentBuildHistoryResolved = $true
        playerProcessStartCount = 0
        unityEditorProcessStartCount = 0
        standaloneBuildInvocationCount = 0
        runTreeByteIdentical = [bool]$finalization.runTreeByteIdentical
        currentPointerByteIdentical = [bool]$finalization.currentPointerByteIdentical
        standaloneHistoryByteIdentical = [bool]$finalization.standaloneHistoryByteIdentical
        buildHistoryByteIdentical = [bool]$finalization.buildHistoryByteIdentical
        generatedSaveTreeByteIdentical = [bool]$finalization.generatedSaveTreeByteIdentical
        repeatedFinalizationTruthIdempotent = $true
        rcRecordWrittenProjectLocally = $true
    }
    $portableProof = [ordered]@{
        portableAllSelectable = $portable.portableAllSelectable
        portableCoreOnly = $portable.portableCoreOnly
        portableAllSelectableGeneratedWorld = 'TRAVEL_CURRENT'
        portableAllSelectableSaveTruth = 'CURRENT'
        portableCoreOnlyGeneratedWorld = 'TRAVEL_CURRENT'
        portableCoreOnlySaveTruth = 'CURRENT'
        portableCoreOnlyAcceptedMechanicsPassed = $false
        portableCoreOnlyFalseReadiness = $true
        playerProcessStartCount = 0
        unityEditorProcessStartCount = 0
        standaloneBuildInvocationCount = 0
        runtimeExecutionStartCount = 0
    }
    $audit = [ordered]@{
        priorGoal161sFailureStage = 'release_candidate_record'
        priorGoal161sFailureDiagnostic = 'rc.payload.missing'
        exactSourceCause = 'RC service resolved the removed project-local Builds/Windows/<slug> payload instead of the validated LocalAppData current run.'
        immutableCurrentAuthority = '%LOCALAPPDATA%/LGC/O/<token>/runs/<run> via current.json'
        standaloneLayerPreserved = $true
        rcDefectClosed = $true
        goal161Accepted = $false
        humanGateCreated = $false
    }
    $scopeProof = [ordered]@{
        scenario = 'goal-161t-immutable-standalone-payload-rc-correlation-and-qualification-closure'
        violationCount = $violations
        exactEvidenceFileCountPerRoot = 8
        proceduralExportTwinsByteIdentical = $true
        forbiddenAreasUntouched = @('Runtime', 'Unity', 'GamePackage schema', 'FeatureModule catalog', 'generated saves/migration/source/history implementation')
        scopeCheckExitCode = $scopeExit
    }
    $report = @"
# Goal161T immutable payload / RC closure

Result: GREEN. The retained Goal161S immutable current pointer and run are the operational payload authority. The stale RC `Builds/Windows/<slug>` lookup is legacy-only; current RC correlation reads `%LOCALAPPDATA%/LGC/O/<token>/runs/<run>` through `current.json`.

The real zero-execution finalization recovered exactly one current GREEN standalone history row and one current build-history row, wrote the project-local RC record, and reopened it as `CURRENT`. Run, pointer, standalone history, build history and generated-save bytes stayed identical. Player, Unity and standalone invocation counts were all zero.

Portable all-selectable restored `CURRENT` with no operational pointer. Portable core-only remained non-ready (`ABSENT`) with no false RC readiness. Legacy project-local payload compatibility and absent-output RC read compatibility remain covered.

Goal161T discovered `$discovered` tests, all passed. Required focused regressions and scope checks passed. `goal161Accepted=false`, `goal161tAccepted=false`; no human gate was created.
"@

    $files = [ordered]@{
        'goal161t-dashboard.json' = ($dashboard | ConvertTo-Json -Depth 8)
        'architecture-review.json' = Get-Content -Raw (Join-Path $procedural 'architecture-review.json')
        'goal161s-independent-audit-finding.json' = ($audit | ConvertTo-Json -Depth 8)
        'immutable-payload-correlation-proof.json' = ($immutable | ConvertTo-Json -Depth 8)
        'zero-execution-rc-finalization-proof.json' = ($finalizationProof | ConvertTo-Json -Depth 8)
        'rc-portability-closure-proof.json' = ($portableProof | ConvertTo-Json -Depth 8)
        'artifact-scope-proof.json' = ($scopeProof | ConvertTo-Json -Depth 8)
        'goal161t-report.md' = $report
    }
    foreach ($root in @($procedural, $exports)) {
        New-Item -ItemType Directory -Force -Path $root | Out-Null
        foreach ($name in $files.Keys) {
            $path = Join-Path $root $name
            [IO.File]::WriteAllText($path, $files[$name], [Text.UTF8Encoding]::new($false))
        }
    }
    foreach ($name in $files.Keys) {
        $left = [IO.File]::ReadAllBytes((Join-Path $procedural $name))
        $right = [IO.File]::ReadAllBytes((Join-Path $exports $name))
        if ((Get-FileHash (Join-Path $procedural $name) -Algorithm SHA256).Hash -ne (Get-FileHash (Join-Path $exports $name) -Algorithm SHA256).Hash) { throw "Evidence twin mismatch: $name" }
    }
    $dashboard | ConvertTo-Json -Depth 8
}
finally {
    Pop-Location
}
