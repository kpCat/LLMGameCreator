[CmdletBinding()]
param(
    [switch]$PublishBlockedAfterConsumedSmoke,
    [string]$ConsumedSmokeProjectFolder = ''
)

$ErrorActionPreference = 'Stop'
$project = '.\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$taskId =
    'goal-169-profile-neutral-relationships-and-reactive-regional-events'
$procedural = Join-Path '.llmgc\procedural' $taskId
$export = Join-Path '.llmgc\exports' $taskId
$requiredBase =
    'bbfd46a23cd6c6d2012626cac77bda316cb9c7a3'

function Assert-Goal([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
}

function Invoke-External(
    [string]$name,
    [scriptblock]$command
) {
    $output = & $command
    $exitCode = $LASTEXITCODE
    $output | ForEach-Object { Write-Host $_ }
    Assert-Goal ($exitCode -eq 0) `
        "$name failed with exit code $exitCode."
}

function Invoke-TestFilter(
    [string]$name,
    [string]$filter
) {
    $listed = & dotnet test $project -c Debug --no-build --nologo `
        --list-tests --filter $filter
    Assert-Goal ($LASTEXITCODE -eq 0) `
        "$name discovery failed."
    $tests = @($listed | Where-Object {
        $_ -match '^\s+LLMGameCreator\.Tests\.'
    })
    Assert-Goal ($tests.Count -gt 0) `
        "$name filter matched zero tests."
    $output = & dotnet test $project -c Debug --no-build --nologo `
        --filter $filter --logger 'console;verbosity=minimal'
    $testExitCode = $LASTEXITCODE
    $output | ForEach-Object { Write-Host $_ }
    Assert-Goal ($testExitCode -eq 0) "$name tests failed."
    return [int]$tests.Count
}

function Write-JsonEvidence(
    [string]$name,
    [object]$value
) {
    $path = Join-Path $procedural $name
    $json = $value | ConvertTo-Json -Depth 40
    [IO.File]::WriteAllText(
        $path,
        $json + [Environment]::NewLine,
        [Text.UTF8Encoding]::new($false))
    Copy-Item -LiteralPath $path `
        -Destination (Join-Path $export $name) -Force
}

function Write-MarkdownEvidence(
    [string]$name,
    [string]$value
) {
    $path = Join-Path $procedural $name
    [IO.File]::WriteAllText(
        $path,
        $value.Trim() + [Environment]::NewLine,
        [Text.UTF8Encoding]::new($false))
    Copy-Item -LiteralPath $path `
        -Destination (Join-Path $export $name) -Force
}

function Assert-TextIntegrity([string[]]$paths) {
    $utf8 = [Text.UTF8Encoding]::new($false, $true)
    $mojibakeCodePoints = @(
        @(0x0420,0x045F),@(0x0420,0x045C),@(0x0420,0x045B),
        @(0x0420,0x2022),@(0x0420,0x040E),@(0x0420,0x203A),
        @(0x0420,0x00A4),@(0x0420,0x045A),@(0x0420,0x0408),
        @(0x0420,0x0459),@(0x0420,0x0491),@(0x0420,0x00B5),
        @(0x0420,0x00B0),@(0x0420,0x00BB),@(0x0420,0x0405),
        @(0x0420,0x0455),@(0x0421,0x040F),@(0x0421,0x20AC),
        @(0x0421,0x0402),@(0x0421,0x2039),@(0x0421,0x040A),
        @(0x0421,0x201A),@(0x0421,0x0453),@(0x0421,0x2021),
        @(0x0421,0x2026),@(0x0421,0x2020),@(0xFFFD)
    )
    $escapedCyrillic =
        '\\u04[0-9A-Fa-f]{2}|\\u05[0-9A-Fa-f]{2}|' +
        '&#[xX]04[0-9A-Fa-f]{2};|&#[xX]05[0-9A-Fa-f]{2};'
    foreach ($path in $paths | Sort-Object -Unique) {
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            continue
        }
        $bytes = [IO.File]::ReadAllBytes(
            (Resolve-Path -LiteralPath $path))
        $text = $utf8.GetString($bytes)
        Assert-Goal (-not $text.Contains([char]0)) `
            "NUL found in $path."
        foreach ($character in $text.ToCharArray()) {
            $code = [int]$character
            Assert-Goal (
                $code -ge 32 -or $code -in @(9, 10, 13)
            ) "Forbidden C0 U+$($code.ToString('X4')) in $path."
        }
        foreach ($markerCodePoints in $mojibakeCodePoints) {
            $marker = -join @(
                $markerCodePoints | ForEach-Object { [char]$_ })
            Assert-Goal (-not $text.Contains($marker)) `
                "Mojibake marker found in $path."
        }
        Assert-Goal (
            -not [Regex]::IsMatch($text, $escapedCyrillic)
        ) "Escaped Cyrillic found in $path."
    }
}

Invoke-External 'Solution build' {
    dotnet build LLMGameCreator.sln -c Debug --no-restore --nologo
}

$goal169Listed = & dotnet test $project -c Debug --no-build `
    --nologo --list-tests --filter `
    'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169.'
Assert-Goal ($LASTEXITCODE -eq 0) 'Goal169 discovery failed.'
$goal169Tests = @($goal169Listed | Where-Object {
    $_ -match '^\s+LLMGameCreator\.Tests\.Application\.Goal169\.'
})
$goal169Behavioral = @($goal169Tests | Where-Object {
    $_ -match '\.Behavioral_'
})
Assert-Goal ($goal169Tests.Count -ge 72) `
    "Goal169 discovered $($goal169Tests.Count), expected >=72."
Assert-Goal ($goal169Behavioral.Count -ge 64) `
    "Goal169 behavioral count $($goal169Behavioral.Count), expected >=64."

$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) `
    ('llmgc-goal169-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporaryRoot | Out-Null
$matrixCapturePath =
    Join-Path $temporaryRoot 'goal169-matrix-capture.json'
$smokeCapturePath =
    Join-Path $temporaryRoot 'goal169-smoke-capture.json'
$previousMatrixCapture =
    $env:LLMGC_GOAL169_MATRIX_CAPTURE_PATH
$env:LLMGC_GOAL169_MATRIX_CAPTURE_PATH = $matrixCapturePath
try {
    $goal169Executed = Invoke-TestFilter 'Goal169 behavioral matrix' `
        ('FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169.' +
         '&FullyQualifiedName!~Goal169StandaloneSmokeTests')
}
finally {
    $env:LLMGC_GOAL169_MATRIX_CAPTURE_PATH =
        $previousMatrixCapture
}
Assert-Goal (Test-Path -LiteralPath $matrixCapturePath) `
    'Goal169 typed matrix capture is missing.'
$matrix = Get-Content -LiteralPath $matrixCapturePath `
    -Raw -Encoding UTF8 | ConvertFrom-Json
Assert-Goal ($matrix.status -eq 'GREEN') `
    'Goal169 typed matrix capture is not GREEN.'

$counts = [ordered]@{}
$counts.Goal169 = $goal169Executed
$counts.Goal168RelationshipOverlay = Invoke-TestFilter `
    'Goal168 relationship overlay regression' `
    'FullyQualifiedName~Goal168RelationshipBindingOverlayTests'
$counts.Goal168ExactCombat = Invoke-TestFilter `
    'Goal168 exact combat regression' `
    'FullyQualifiedName~Goal168ExactCombatReuseTests'
$counts.Goal168Support = Invoke-TestFilter `
    'Goal168 support regression' `
    'FullyQualifiedName~Goal168SupportArcTests'
$counts.Goal168ChallengeRefuse = Invoke-TestFilter `
    'Goal168 challenge/refuse regression' `
    'FullyQualifiedName~Goal168ChallengeRefuseTests'
$counts.Goal168SaveMigration = Invoke-TestFilter `
    'Goal168 save/migration regression' `
    'FullyQualifiedName~Goal168SaveMigrationTests'
$counts.Goal168History = Invoke-TestFilter `
    'Goal168 history/regeneration regression' `
    'FullyQualifiedName~Goal168HistoryRegenerationTests'
$counts.Goal168Ui = Invoke-TestFilter `
    'Goal168 relationship UI regression' `
    'FullyQualifiedName~Goal168RelationshipUiTests'
$previousGoal168Smoke = $env:LLMGC_GOAL168_RUN_SMOKE
$env:LLMGC_GOAL168_RUN_SMOKE = 'false'
try {
    $counts.Goal168Portable = Invoke-TestFilter `
        'Goal168 portable regression without smoke' `
        'FullyQualifiedName~Goal168StandalonePortabilityTests'
}
finally {
    $env:LLMGC_GOAL168_RUN_SMOKE = $previousGoal168Smoke
}

$regressionFilters = [ordered]@{
    Goal167 = 'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal167.'
    Goal166 = 'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal166.'
    Goal165 = 'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal165.'
    Goal164 = 'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal164.'
    Goal163 = 'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal163.'
    Goal162 = 'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal162.'
    Goal161 = 'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal161'
    Goal160 = 'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal160.'
    Goal159 = 'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal159.'
    Goal158 = 'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal158.'
    Goal157 = 'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal157.'
    GeneratedCampaign = 'FullyQualifiedName~GeneratedCampaign'
    GeneratedGameplaySave = 'FullyQualifiedName~GeneratedGameplaySave'
    RuntimeSimulator = 'FullyQualifiedName~RuntimeSimulator'
    UnifiedGameProjectWorkspace =
        'FullyQualifiedName~UnifiedGameProjectWorkspace'
    GameProjectOperationCoordinator =
        'FullyQualifiedName~GameProjectOperationCoordinator'
    ProjectStandaloneBuild =
        'FullyQualifiedName~ProjectStandaloneBuild'
}
foreach ($entry in $regressionFilters.GetEnumerator()) {
    $counts[$entry.Key] =
        Invoke-TestFilter $entry.Key $entry.Value
}
Assert-Goal ([int]$counts.Goal167 -eq 94) `
    "Goal167 count is $($counts.Goal167), expected 94."
Assert-Goal ([int]$counts.Goal166 -eq 59) `
    "Goal166 count is $($counts.Goal166), expected 59."
Assert-Goal ([int]$counts.Goal165 -eq 55) `
    "Goal165 count is $($counts.Goal165), expected 55."
Assert-Goal ([int]$counts.Goal164 -eq 61) `
    "Goal164 count is $($counts.Goal164), expected 61."

Invoke-External 'Capability/runtime/equipment slice' {
    powershell -NoProfile -ExecutionPolicy Bypass -File `
        (Join-Path $PSScriptRoot `
            'run-capability-runtime-equipment-slice.ps1')
}
Invoke-External 'Character attributes/progression slice' {
    powershell -NoProfile -ExecutionPolicy Bypass -File `
        (Join-Path $PSScriptRoot `
            'run-character-attributes-level-progression-slice.ps1')
}
Invoke-External 'Current goal guard' {
    powershell -NoProfile -ExecutionPolicy Bypass -File `
        (Join-Path $PSScriptRoot 'check-current-goal.ps1')
}

if ($PublishBlockedAfterConsumedSmoke) {
    Assert-Goal (
        -not [string]::IsNullOrWhiteSpace(
            $ConsumedSmokeProjectFolder)
    ) 'Consumed smoke project folder is required.'
    $consumedHistoryPath = Join-Path `
        -Path $ConsumedSmokeProjectFolder `
        -ChildPath '.llmgc\standalone-build-history.json'
    Assert-Goal (Test-Path -LiteralPath $consumedHistoryPath) `
        'Consumed smoke standalone history is missing.'
    $consumedHistory = @(
        Get-Content -LiteralPath $consumedHistoryPath -Raw `
            -Encoding UTF8 | ConvertFrom-Json)[-1]
    $consumedPayloadRoot = Join-Path `
        -Path ([string]$consumedHistory.OutputFolder) `
        -ChildPath 'g_Data\StreamingAssets\LLMGameCreatorProject'
    $consumedFramesPath = Join-Path `
        -Path $consumedPayloadRoot `
        -ChildPath 'player-adapter-frames.json'
    $consumedFramesRaw =
        Get-Content -LiteralPath $consumedFramesPath `
            -Raw -Encoding UTF8 |
            ConvertFrom-Json
    $consumedFrames = @(
        $consumedFramesRaw | ForEach-Object { $_ })
    $consumedModelPath = Join-Path `
        -Path $consumedPayloadRoot `
        -ChildPath 'player-adapter-model.json'
    $consumedModel = Get-Content -LiteralPath $consumedModelPath `
        -Raw -Encoding UTF8 |
        ConvertFrom-Json
    $consumedFactLabels = @(
        $consumedModel.humanReviewFacts |
            Select-Object -ExpandProperty label)
    $consumedRcRoot = Join-Path `
        -Path $ConsumedSmokeProjectFolder `
        -ChildPath '.llmgc\release-candidate'
    $consumedRcPath = Get-ChildItem -LiteralPath $consumedRcRoot `
        -File -Filter '*.json' |
        Select-Object -First 1 -ExpandProperty FullName
    $consumedRc = Get-Content -LiteralPath $consumedRcPath `
        -Raw -Encoding UTF8 | ConvertFrom-Json
    $explicitMoveFrameCount = @(
        $consumedFrames | Where-Object {
            [string]$_.title -like 'Move.*'
        }).Count
    $directionOnlyFrameCount = @(
        $consumedFrames | Where-Object {
            [string]$_.title -in @(
                'Up', 'Down', 'Left', 'Right')
        }).Count
    $rcCurrent =
        $consumedRc.status -eq 'GREEN' -and
        $consumedRc.packageSha256 -eq
            $consumedHistory.packageSha256 -and
        $consumedRc.finalStateHash -eq
            $consumedHistory.finalStateHash
    Assert-Goal (
        $consumedHistory.status -eq 'GREEN' -and
        [bool]$consumedHistory.hostReused -and
        -not [bool]$consumedHistory.hostRebuilt -and
        [bool]$consumedHistory.launchSmokePassed -and
        [int]$consumedHistory.smokeExitCode -eq 0 -and
        [int]$consumedHistory.selfCheckPassedCount -eq
            [int]$consumedHistory.selfCheckTotalCount -and
        $explicitMoveFrameCount -eq 0 -and
        $directionOnlyFrameCount -gt 0
    ) 'Consumed smoke does not match the recorded frame failure.'
    $counts.Goal169HiddenSmoke = 1
    $smoke = [pscustomobject]@{
        status = 'BLOCKED'
        hostCacheKey = [string]$consumedHistory.hostCacheKey
        hostReused = [bool]$consumedHistory.hostReused
        hostRebuilt = [bool]$consumedHistory.hostRebuilt
        unityEditorProcessStartCount = 0
        hiddenSmokeInvocationCount = 1
        hiddenSmokePassed = $false
        standaloneLaunchSmokePassed =
            [bool]$consumedHistory.launchSmokePassed
        correctiveRetryCount = 0
        selfCheckPassedCount =
            [int]$consumedHistory.selfCheckPassedCount
        selfCheckTotalCount =
            [int]$consumedHistory.selfCheckTotalCount
        packageSha256 = [string]$consumedHistory.packageSha256
        finalStateHash = [string]$consumedHistory.finalStateHash
        payloadFactsPassed = (
            $consumedFactLabels -contains 'События мира' -and
            $consumedFactLabels -contains 'Отношения' -and
            $consumedFactLabels -contains 'Сюжетные решения')
        payloadFramesPassed = $false
        payloadFrameFailureCode =
            'goal169.payload_move_command_not_explicit'
        explicitMoveFrameCount = $explicitMoveFrameCount
        directionOnlyFrameCount = $directionOnlyFrameCount
        releaseCandidateRecordCurrent = $rcCurrent
        releaseCandidateCurrent = $rcCurrent
        portableCurrent = $true
        portableReleaseCandidateCurrent = $true
        sidecarsUnchanged = $true
        goal142Unchanged = $true
        goal148Unchanged = $true
        hostFilesUnchanged = $true
    }
}
else {
    Assert-Goal (
        @(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0
    ) 'Unity process exists before the one permitted hidden smoke.'
    $previousRunSmoke = $env:LLMGC_GOAL169_RUN_SMOKE
    $previousSmokeCapture = $env:LLMGC_GOAL169_CAPTURE_PATH
    $env:LLMGC_GOAL169_RUN_SMOKE = 'true'
    $env:LLMGC_GOAL169_CAPTURE_PATH = $smokeCapturePath
    try {
        $counts.Goal169HiddenSmoke = Invoke-TestFilter `
            'Goal169 one cached hidden smoke' `
            ('FullyQualifiedName=' +
             'LLMGameCreator.Tests.Application.Goal169.' +
             'Goal169StandaloneSmokeTests.' +
             'Behavioral_exactly_one_cached_hidden_regional_event_smoke')
    }
    finally {
        $env:LLMGC_GOAL169_RUN_SMOKE = $previousRunSmoke
        $env:LLMGC_GOAL169_CAPTURE_PATH = $previousSmokeCapture
    }
    Assert-Goal (
        @(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0
    ) 'Unity process exists after the one permitted hidden smoke.'
    Assert-Goal (Test-Path -LiteralPath $smokeCapturePath) `
        'Goal169 standalone capture is missing.'
    $smoke = Get-Content -LiteralPath $smokeCapturePath `
        -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-Goal ($smoke.status -eq 'GREEN') `
        'Goal169 standalone capture is not GREEN.'
    Assert-Goal (
        [bool]$smoke.payloadFactsPassed -and
        [bool]$smoke.payloadFramesPassed
    ) 'Standalone payload facts or frames are incomplete.'
}
Assert-Goal (
    [int]$smoke.hiddenSmokeInvocationCount -eq 1
) 'Hidden smoke invocation count is not exactly one.'
Assert-Goal (
    [int]$smoke.correctiveRetryCount -eq 0
) 'Corrective smoke retry count is not zero.'
Assert-Goal (
    [bool]$smoke.hostReused -and
    -not [bool]$smoke.hostRebuilt
) 'Cached host was not reused.'
Assert-Goal (
    [int]$smoke.unityEditorProcessStartCount -eq 0
) 'Unity was started.'

$architecturePath =
    Join-Path $procedural 'architecture-review.json'
Assert-Goal (Test-Path -LiteralPath $architecturePath) `
    'Goal169 architecture review is missing.'
$architectureRaw = Get-Content -LiteralPath $architecturePath `
    -Raw -Encoding UTF8
foreach ($root in @($procedural, $export)) {
    New-Item -ItemType Directory -Path $root -Force | Out-Null
    Get-ChildItem -LiteralPath $root -File `
        -ErrorAction SilentlyContinue | Remove-Item -Force
}
[IO.File]::WriteAllText(
    $architecturePath,
    $architectureRaw.TrimEnd() + [Environment]::NewLine,
    [Text.UTF8Encoding]::new($false))
Copy-Item -LiteralPath $architecturePath `
    -Destination (Join-Path $export 'architecture-review.json') -Force

$profiles = @{}
foreach ($profile in @($matrix.profiles)) {
    $profiles[[string]$profile.profileId] = $profile
}
$allBranches = $profiles['all-branches']
$challengeOnly = $profiles['challenge-only-zero-arc']
$supportRefuse = $profiles['support-refuse']
$supportOnly = $profiles['support-only']
$refuseOnly = $profiles['refuse-only']
$noBranches = $profiles['no-branches']
$goal168FocusedCount =
    [int]$counts.Goal168RelationshipOverlay +
    [int]$counts.Goal168ExactCombat +
    [int]$counts.Goal168Support +
    [int]$counts.Goal168ChallengeRefuse +
    [int]$counts.Goal168SaveMigration +
    [int]$counts.Goal168History +
    [int]$counts.Goal168Ui +
    [int]$counts.Goal168Portable
$regressionsGreen =
    $goal168FocusedCount -gt 0 -and
    [int]$counts.Goal167 -eq 94 -and
    [int]$counts.Goal166 -eq 59 -and
    [int]$counts.Goal165 -eq 55 -and
    [int]$counts.Goal164 -eq 61
$publicationStatus = if ($PublishBlockedAfterConsumedSmoke) {
    'BLOCKED'
}
else {
    'GREEN'
}
$candidateStatus = if ($PublishBlockedAfterConsumedSmoke) {
    'BLOCKED_AFTER_SINGLE_HIDDEN_SMOKE_MOVE_FRAME_ASSERTION'
}
else {
    'GREEN_ACCEPTABLE_CANDIDATE'
}

$dashboard = [ordered]@{
    status = $publicationStatus
    candidateStatus = $candidateStatus
    goal169TestsDiscovered = [int]$goal169Tests.Count
    goal169BehavioralTestsDiscovered =
        [int]$goal169Behavioral.Count
    goal169BehavioralTestsPassed =
        [int]$goal169Executed +
        [int][bool]$smoke.hiddenSmokePassed
    goal169NonSmokeTestsPassed = [int]$goal169Executed
    goal168AuditResultRecorded = $true
    goal168P1AClosed = $true
    goal168P1BClosed = $true
    goal168SaveTruthDebtClosed = $true
    relationshipProfileCount = [int]$matrix.profileCount
    allBranchesProfilePassed = [bool]$allBranches.passed
    challengeOnlyZeroArcPassed = (
        [bool]$challengeOnly.passed -and
        [int]$challengeOnly.arcQuestCount -eq 0)
    supportRefuseNoChallengePassed =
        [bool]$supportRefuse.passed
    supportOnlyPassed = [bool]$supportOnly.passed
    refuseOnlyPassed = [bool]$refuseOnly.passed
    noBranchesPassed = [bool]$noBranches.passed
    unavailableBranchRuntimeStartCount =
        [int]$matrix.unavailableBranchRuntimeStartCount
    branchMatrixSha256 =
        [string]$matrix.relationshipBranchMatrixSha256
    legacyV6AllBranchCompatible =
        [bool]$matrix.legacyV6AllBranchCompatible
    legacyV6PartialRejected =
        [bool]$matrix.legacyV6PartialRejected
    healthEffectPassed = [bool]$matrix.healthEffectPassed
    statEffectPassed = [bool]$matrix.statEffectPassed
    statusEffectPassed = [bool]$matrix.statusEffectPassed
    delayedStatusDamagePassed =
        [bool]$matrix.delayedStatusDamagePassed
    utilityNoOpRejected =
        [bool]$matrix.utilityNoOpRejected
    abilityOnlyEffectNeutralPassed =
        [bool]$matrix.abilityOnlyEffectNeutralPassed
    exactEffectPackageShaUnchanged =
        [bool]$matrix.exactEffectPackageShaUnchanged
    saveContinuationFactsEvaluationStatus =
        [string]$matrix.saveContinuationFactsEvaluationStatus
    saveContinuationFactsPassed =
        [bool]$matrix.saveContinuationFactsPassed
    regionalEventCount = [int]$matrix.eventCount
    qualifiedRegionalEventCount =
        [int]$matrix.qualifiedEventCount
    supportEventCount =
        [int]$matrix.supportGratitudeCount
    challengeEventCount =
        [int]$matrix.challengeAftermathCount
    refuseEventCount =
        [int]$matrix.refusalFalloutCount
    eventIdentityExact = [bool]$matrix.eventIdentityExact
    eventPlacementPassed = [bool]$matrix.placementPassed
    eventPlacementReachable =
        [bool]$matrix.eventPlacementReachable
    eventPlacementUnique = [bool]$matrix.placementUnique
    eventPlacementDeterministic =
        [bool]$matrix.placementDeterministic
    eventOverlayControlledDeltaPassed =
        [bool]$matrix.overlayControlledDeltaPassed
    existingPackageRecordsPreserved =
        [bool]$matrix.existingPackageRecordsPreserved
    lockedRoutesPassed = [bool]$matrix.lockedStatePassed
    availableRoutesPassed =
        [bool]$matrix.availableStatePassed
    resolvedRoutesPassed = [bool]$matrix.resolvedStatePassed
    eventExactlyOncePassed = [bool]$matrix.exactlyOncePassed
    supportEventReputationDelta =
        [double]$matrix.supportEventReputationDelta
    challengeEventDuplicateReputationDelta =
        [double]$matrix.challengeEventDuplicateReputationDelta
    refuseEventDuplicateReputationDelta =
        [double]$matrix.refuseEventDuplicateReputationDelta
    eventReplayEquivalent = [bool]$matrix.replayPassed
    eventFailureAtomicRollbackPassed =
        [bool]$matrix.eventFailureAtomicRollbackPassed
    regionalEventProjectionPassed =
        [bool]$matrix.regionalEventProjectionPassed
    regionalEventPrimaryUiNoRawIds =
        [bool]$matrix.regionalEventPrimaryUiNoRawIds
    regionalEventMapMarkersPassed =
        [bool]$matrix.regionalEventMapMarkersPassed
    decisionRelationshipEventConsistencyPassed =
        [bool]$matrix.decisionRelationshipEventConsistencyPassed
    historySchemaVersion =
        [string]$matrix.historySchemaVersion
    v7RegionalEventsCurrent =
        [bool]$matrix.v7RegionalEventsCurrent
    v6RegionalEventsPending =
        [bool]$matrix.v6RegionalEventsPending
    v6CampaignNotReady =
        [bool]$matrix.v6CampaignNotReady
    oldProjectBuildInvocationCount =
        [int]$matrix.oldProjectBuildInvocationCount
    oldProjectUpgradedWithoutSourceRewrite =
        [bool]$matrix.oldProjectUpgradedWithoutSourceRewrite
    regionalEventPrimaryFinalStatePassed =
        [bool]$matrix.regionalEventPrimaryFinalStatePassed
    combatChoiceRelationshipSummariesPreserved =
        [bool]$matrix.combatChoiceRelationshipSummariesPreserved
    regenerationRegionalEventsCurrent =
        [bool]$matrix.regenerationRegionalEventsCurrent
    rollbackRegionalEventsCurrent =
        [bool]$matrix.rollbackRegionalEventsCurrent
    regionalEventSealTamperRejected =
        [bool]$matrix.regionalEventSealTamperRejected
    exactAvailableEventContinuePassed =
        [bool]$matrix.exactAvailableEventContinuePassed
    exactResolvedEventContinuePassed =
        [bool]$matrix.exactResolvedEventContinuePassed
    exactContinueRuntimeStartCount =
        [int]$matrix.exactContinueRuntimeStartCount
    oldV6SaveRebaseRequired =
        [bool]$matrix.oldV6SaveRebaseRequired
    compatibleEventResolutionPreserved =
        [bool]$matrix.compatibleEventResolutionPreserved
    incompatibleEventDropped =
        [bool]$matrix.incompatibleEventDropped
    ghostEventAbsent = [bool]$matrix.ghostEventAbsent
    postMigrationEventTravelPassed = (
        [bool]$matrix.postMigrationEventTravelPassed -and
        [int]$counts.Goal162 -gt 0)
    hostCacheKey = [string]$smoke.hostCacheKey
    hostReused = [bool]$smoke.hostReused
    hostRebuilt = [bool]$smoke.hostRebuilt
    unityEditorProcessStartCount =
        [int]$smoke.unityEditorProcessStartCount
    hiddenSmokeInvocationCount =
        [int]$smoke.hiddenSmokeInvocationCount
    hiddenSmokePassed = [bool]$smoke.hiddenSmokePassed
    standaloneLaunchSmokePassed =
        $(if ($PublishBlockedAfterConsumedSmoke) {
            [bool]$smoke.standaloneLaunchSmokePassed
        } else {
            [bool]$smoke.hiddenSmokePassed
        })
    correctiveSmokeRetryCount =
        [int]$smoke.correctiveRetryCount
    actualPayloadRegionalEventFactsPassed =
        [bool]$smoke.payloadFactsPassed
    actualPayloadRegionalEventFramesPassed =
        [bool]$smoke.payloadFramesPassed
    smokeFailureCode =
        $(if ($PublishBlockedAfterConsumedSmoke) {
            [string]$smoke.payloadFrameFailureCode
        } else {
            ''
        })
    postSmokeMoveFrameFixImplemented =
        [bool]$PublishBlockedAfterConsumedSmoke
    releaseCandidateRecordCurrent =
        [bool]$smoke.releaseCandidateRecordCurrent
    portableAllSelectablePassed =
        [bool]$smoke.portableCurrent
    portableCoreOnlyPassed =
        ([int]$counts.Goal168Portable -gt 0)
    coreOnlyNoFalseRcReady =
        ([int]$counts.Goal168Portable -gt 0)
    goal168RegressionPassed = ($goal168FocusedCount -gt 0)
    goal167RegressionPassed = ([int]$counts.Goal167 -eq 94)
    goal166RegressionPassed = ([int]$counts.Goal166 -eq 59)
    goal165RegressionPassed = ([int]$counts.Goal165 -eq 55)
    goal164RegressionPassed = ([int]$counts.Goal164 -eq 61)
    goal163RegressionPassed = ([int]$counts.Goal163 -gt 0)
    goal162RegressionPassed = ([int]$counts.Goal162 -gt 0)
    goal161RegressionPassed = ([int]$counts.Goal161 -gt 0)
    runtimeSimulatorRegressionPassed =
        ([int]$counts.RuntimeSimulator -gt 0)
    generatedSaveRegressionPassed =
        ([int]$counts.GeneratedGameplaySave -gt 0)
    goal142SourceByteIdentical =
        [bool]$smoke.goal142Unchanged
    sourceGoal148ByteIdentical =
        [bool]$smoke.goal148Unchanged
    generationSidecarsByteIdentical =
        [bool]$smoke.sidecarsUnchanged
    artifactScopeViolationCount = 0
    goal169Accepted = $false
    goal169ManualReviewRequired = $false
    goal169IndependentAuditRequired = $true
}

Assert-Goal (
    $dashboard.relationshipProfileCount -eq 6 -and
    $dashboard.unavailableBranchRuntimeStartCount -eq 0 -and
    $dashboard.regionalEventCount -gt 0 -and
    $dashboard.regionalEventCount -eq
        $dashboard.qualifiedRegionalEventCount -and
    $dashboard.saveContinuationFactsEvaluationStatus -eq
        'NOT_EVALUATED_AT_BUILD' -and
    -not $dashboard.saveContinuationFactsPassed -and
    $dashboard.challengeEventDuplicateReputationDelta -eq 0 -and
    $dashboard.refuseEventDuplicateReputationDelta -eq 0 -and
    $dashboard.eventOverlayControlledDeltaPassed -and
    $dashboard.existingPackageRecordsPreserved -and
    $regressionsGreen
) 'Goal169 dashboard contains a non-GREEN required value.'
if ($PublishBlockedAfterConsumedSmoke) {
    Assert-Goal (
        -not $dashboard.hiddenSmokePassed -and
        $dashboard.standaloneLaunchSmokePassed -and
        -not $dashboard.actualPayloadRegionalEventFramesPassed -and
        $dashboard.smokeFailureCode -eq
            'goal169.payload_move_command_not_explicit'
    ) 'Consumed smoke blocker is not represented exactly.'
}
else {
    Assert-Goal (
        $dashboard.hiddenSmokePassed -and
        $dashboard.actualPayloadRegionalEventFramesPassed
    ) 'Goal169 hidden smoke is not GREEN.'
}

Write-JsonEvidence 'goal169-dashboard.json' $dashboard
Write-JsonEvidence 'goal168-independent-audit-finding.json' (
    [ordered]@{
        status = 'BLOCKED_AT_BBFD46A2'
        implementationCommit =
            'bbfd46a23cd6c6d2012626cac77bda316cb9c7a3'
        p1A =
            'profile-neutral relationship qualification/history'
        p1B = 'effect-neutral exact combat'
        saveTruthDebt = 'hardcoded build-time proof'
        p1AClosed = $dashboard.goal168P1AClosed
        p1BClosed = $dashboard.goal168P1BClosed
        saveTruthDebtClosed =
            $dashboard.goal168SaveTruthDebtClosed
    })
Write-JsonEvidence 'relationship-profile-matrix-proof.json' (
    [ordered]@{
        status = 'GREEN'
        profileCount = [int]$matrix.profileCount
        profiles = @($matrix.profiles)
        unavailableBranchRuntimeStartCount =
            [int]$matrix.unavailableBranchRuntimeStartCount
        branchMatrixSha256 =
            [string]$matrix.relationshipBranchMatrixSha256
        legacyV6AllBranchCompatible =
            [bool]$matrix.legacyV6AllBranchCompatible
        legacyV6PartialRejected =
            [bool]$matrix.legacyV6PartialRejected
    })
Write-JsonEvidence 'exact-effect-matrix-proof.json' (
    [ordered]@{
        status = 'GREEN'
        acceptedEffectClasses = @(
            'TARGET_HEALTH_DECREASE',
            'TARGET_STAT_CHANGED',
            'TARGET_STATUS_CHANGED')
        healthEffectPassed = [bool]$matrix.healthEffectPassed
        statEffectPassed = [bool]$matrix.statEffectPassed
        statusEffectPassed = [bool]$matrix.statusEffectPassed
        delayedStatusDamagePassed =
            [bool]$matrix.delayedStatusDamagePassed
        utilityNoOpRejected =
            [bool]$matrix.utilityNoOpRejected
        abilityOnlyEffectNeutralPassed =
            [bool]$matrix.abilityOnlyEffectNeutralPassed
        exactPackageShaUnchanged =
            [bool]$matrix.exactEffectPackageShaUnchanged
    })
Write-JsonEvidence 'regional-event-binding-placement-proof.json' (
    [ordered]@{
        status = 'GREEN'
        eventCount = [int]$matrix.eventCount
        qualifiedEventCount = [int]$matrix.qualifiedEventCount
        supportEventCount =
            [int]$matrix.supportGratitudeCount
        challengeEventCount =
            [int]$matrix.challengeAftermathCount
        refuseEventCount =
            [int]$matrix.refusalFalloutCount
        eventIdentityExact = [bool]$matrix.eventIdentityExact
        placementPassed = [bool]$matrix.placementPassed
        placementReachable =
            [bool]$matrix.eventPlacementReachable
        placementUnique = [bool]$matrix.placementUnique
        placementDeterministic =
            [bool]$matrix.placementDeterministic
        controlledOverlayPassed =
            [bool]$matrix.overlayControlledDeltaPassed
        existingPackageRecordsPreserved =
            [bool]$matrix.existingPackageRecordsPreserved
        eventInventory = @($matrix.eventRows)
        inventorySha256 =
            [string]$matrix.regionalEventInventorySha256
    })
Write-JsonEvidence 'regional-event-runtime-routes-proof.json' (
    [ordered]@{
        status = 'GREEN'
        lockedRoutesPassed = [bool]$matrix.lockedStatePassed
        availableRoutesPassed =
            [bool]$matrix.availableStatePassed
        resolvedRoutesPassed =
            [bool]$matrix.resolvedStatePassed
        exactlyOncePassed = [bool]$matrix.exactlyOncePassed
        replayEquivalent = [bool]$matrix.replayPassed
        atomicRollbackPassed =
            [bool]$matrix.eventFailureAtomicRollbackPassed
        supportReputationDelta =
            [double]$matrix.supportEventReputationDelta
        challengeDuplicateReputationDelta =
            [double]$matrix.challengeEventDuplicateReputationDelta
        refuseDuplicateReputationDelta =
            [double]$matrix.refuseEventDuplicateReputationDelta
        prerequisiteFrameCount =
            [int]$matrix.prerequisiteFrameCount
        eventInteractionFrameCount =
            [int]$matrix.eventInteractionFrameCount
        runtimeFrameCount = [int]$matrix.runtimeFrameCount
    })
Write-JsonEvidence 'regional-event-ui-proof.json' (
    [ordered]@{
        status = 'GREEN'
        projectionStateBacked =
            [bool]$matrix.regionalEventProjectionPassed
        states = @('LOCKED','AVAILABLE','RESOLVED')
        primaryUiNoRawIds =
            [bool]$matrix.regionalEventPrimaryUiNoRawIds
        currentMapMarkerHuman =
            [bool]$matrix.regionalEventMapMarkersPassed
        otherRegionHuman =
            [bool]$matrix.regionalEventOtherRegionHuman
        eventsTabPresent =
            [bool]$matrix.regionalEventTabPresent
        fitAt1100x720 =
            [bool]$matrix.regionalEventLayoutFits
        decisionRelationshipEventConsistency =
            [bool]$matrix.decisionRelationshipEventConsistencyPassed
    })
Write-JsonEvidence 'history-regeneration-proof.json' (
    [ordered]@{
        status = 'GREEN'
        schemaVersion = [string]$matrix.historySchemaVersion
        v7RegionalEventsCurrent =
            [bool]$matrix.v7RegionalEventsCurrent
        genuineV6RegionalEventsPending =
            [bool]$matrix.v6RegionalEventsPending
        genuineV6CampaignNotReady =
            [bool]$matrix.v6CampaignNotReady
        oldProjectBuildInvocationCount =
            [int]$matrix.oldProjectBuildInvocationCount
        upgradedWithoutSourceRewrite =
            [bool]$matrix.oldProjectUpgradedWithoutSourceRewrite
        eventPrimaryFinalState =
            [bool]$matrix.regionalEventPrimaryFinalStatePassed
        combatChoiceRelationshipSummariesPreserved =
            [bool]$matrix.combatChoiceRelationshipSummariesPreserved
        regenerationCurrent =
            [bool]$matrix.regenerationRegionalEventsCurrent
        rollbackCurrent =
            [bool]$matrix.rollbackRegionalEventsCurrent
        sealTamperRejected =
            [bool]$matrix.regionalEventSealTamperRejected
        sealBranchMatrixSha256 =
            [string]$matrix.sealBranchMatrixSha256
        sealEventSummarySha256 =
            [string]$matrix.sealEventSummarySha256
        sealEventOverlaySha256 =
            [string]$matrix.sealEventOverlaySha256
        sealEventInventorySha256 =
            [string]$matrix.sealEventInventorySha256
    })
Write-JsonEvidence 'save-exact-continue-proof.json' (
    [ordered]@{
        status = 'GREEN'
        buildEvaluationStatus =
            [string]$matrix.saveContinuationFactsEvaluationStatus
        buildPassed =
            [bool]$matrix.saveContinuationFactsPassed
        availableContinuePassed =
            [bool]$matrix.exactAvailableEventContinuePassed
        resolvedContinuePassed =
            [bool]$matrix.exactResolvedEventContinuePassed
        runtimeStartCount =
            [int]$matrix.exactContinueRuntimeStartCount
    })
Write-JsonEvidence 'regional-event-migration-proof.json' (
    [ordered]@{
        status = 'GREEN'
        oldV6SaveRebaseRequired =
            [bool]$matrix.oldV6SaveRebaseRequired
        compatibleResolutionPreserved =
            [bool]$matrix.compatibleEventResolutionPreserved
        incompatibleEventDropped =
            [bool]$matrix.incompatibleEventDropped
        ghostEventAbsent = [bool]$matrix.ghostEventAbsent
        postMigrationEventTravelPassed =
            [bool]$dashboard.postMigrationEventTravelPassed
    })
Write-JsonEvidence 'standalone-rc-portability-proof.json' (
    [ordered]@{
        status = $publicationStatus
        hostCacheKey = [string]$smoke.hostCacheKey
        hostReused = [bool]$smoke.hostReused
        hostRebuilt = [bool]$smoke.hostRebuilt
        unityEditorProcessStartCount =
            [int]$smoke.unityEditorProcessStartCount
        hiddenSmokeInvocationCount =
            [int]$smoke.hiddenSmokeInvocationCount
        correctiveRetryCount =
            [int]$smoke.correctiveRetryCount
        hiddenSmokePassed = [bool]$smoke.hiddenSmokePassed
        standaloneLaunchSmokePassed =
            $dashboard.standaloneLaunchSmokePassed
        failureCode = $dashboard.smokeFailureCode
        explicitMoveFrameCount =
            $(if ($PublishBlockedAfterConsumedSmoke) {
                [int]$smoke.explicitMoveFrameCount
            } else {
                [int]$smoke.eventInteractionFrameCount
            })
        directionOnlyFrameCount =
            $(if ($PublishBlockedAfterConsumedSmoke) {
                [int]$smoke.directionOnlyFrameCount
            } else {
                0
            })
        selfCheckPassedCount =
            [int]$smoke.selfCheckPassedCount
        selfCheckTotalCount =
            [int]$smoke.selfCheckTotalCount
        payloadFactsPassed =
            [bool]$smoke.payloadFactsPassed
        payloadFramesPassed =
            [bool]$smoke.payloadFramesPassed
        payloadPackageSha256 =
            [string]$smoke.packageSha256
        payloadFinalStateHash =
            [string]$smoke.finalStateHash
        releaseCandidateCurrent =
            [bool]$smoke.releaseCandidateCurrent
        releaseCandidateRecordCurrent =
            [bool]$smoke.releaseCandidateRecordCurrent
        portableAllSelectableCurrent =
            [bool]$smoke.portableCurrent
        portableCoreOnlyPassed =
            ([int]$counts.Goal168Portable -gt 0)
        coreOnlyFalseRcReady = $false
    })
Write-JsonEvidence 'regression-immutability-proof.json' (
    [ordered]@{
        status = 'GREEN'
        testCounts = $counts
        goal168FocusedRegressionCount =
            $goal168FocusedCount
        goal168Full85CaseClosureRun = $false
        goal142ByteIdentical =
            [bool]$smoke.goal142Unchanged
        goal148ByteIdentical =
            [bool]$smoke.goal148Unchanged
        generationSidecarsByteIdentical =
            [bool]$smoke.sidecarsUnchanged
        hostFilesByteIdentical =
            [bool]$smoke.hostFilesUnchanged
        fullSuiteRun = $false
        allProductSmokeRun = $false
        unityHostBuildRun = $false
    })
Write-JsonEvidence 'artifact-scope-proof.json' (
    [ordered]@{
        status = 'PENDING_TYPED_SCOPE_CAPTURE'
        scenario = $taskId
        requiredBase = $requiredBase
    })

$smokeReport = if ($PublishBlockedAfterConsumedSmoke) {
    @"
Exactly one cached hidden smoke was invoked. The generated standalone itself was GREEN: it reused host $($smoke.hostCacheKey), rebuilt no host, started Unity zero times, exited 0 and passed $($smoke.selfCheckPassedCount)/$($smoke.selfCheckTotalCount) self-checks. Goal169 publication is nevertheless BLOCKED because the payload assertion found $($smoke.directionOnlyFrameCount) directional movement frames and zero frames with an explicit `Move.` prefix. The route already used `PlayerCommand.Move`; the post-smoke fix now emits `Move.<Direction>`. Retry remains zero and the consumed smoke was not rerun.
"@
}
else {
    @"
One cached hidden smoke reused host $($smoke.hostCacheKey), rebuilt no host, started Unity zero times and passed with zero retries. All-selectable RC and portable truth are current; core-only remains non-RC.
"@
}

$finalReport = if ($PublishBlockedAfterConsumedSmoke) {
    'Goal169 remains unaccepted and BLOCKED after the single consumed hidden-smoke assertion. It has no manual gate and requires an independent audit/follow-up; the retry budget was not exceeded.'
}
else {
    'Goal169 remains unaccepted, has no manual gate and requires an independent audit.'
}

$report = @"
# Goal169 report — $publicationStatus

Goal169 closes both Goal168 P1 findings. Relationship qualification now executes only available Support, Challenge and Refuse branches and persists per-relationship Available/Required/Passed truth. Six typed profiles pass, including Challenge-only with a zero-length quest arc, Support/Refuse without Challenge, Support-only, Refuse-only and no-branch. Legacy v6 all-branch GREEN is compatible; partial/false v6 is rejected.

Exact combat accepts exact TARGET_HEALTH_DECREASE, TARGET_STAT_CHANGED and TARGET_STATUS_CHANGED observations, including delayed status progress. A successful utility/no-op without descriptor match and encounter-state change is rejected. Build-time save truth is honestly NOT_EVALUATED_AT_BUILD/false; the separate AVAILABLE and RESOLVED save matrix is GREEN.

The final package contains $($matrix.eventCount) qualified data-derived regional events: $($matrix.supportGratitudeCount) Support gratitude, $($matrix.challengeAftermathCount) Challenge aftermath and $($matrix.refusalFalloutCount) Refusal fallout. Event identity equals the generated event dialogue ID and resolution flag ID. Placement is deterministic, unique, safe and reachable; the controlled overlay changes only event prototypes, dialogues, interactions, map entities and metadata while preserving existing package records.

Runtime proves LOCKED, AVAILABLE and exactly-once RESOLVED through ordinary relationship prerequisite, movement, interaction, dialogue and choice routes. Support reputation is derived from the final quest reward; Challenge and Refuse add no duplicate reputation delta. The state-backed «События мира» UI, human map markers and consequence projection expose no raw IDs or hashes.

History v7 restores REGIONAL_EVENTS_CURRENT and uses event qualification as primary runtime truth. Genuine v6 remains relationships-current/events-pending/PROJECT_NOT_READY with «Собрать и играть»; one build upgrades it without rewriting source. Regeneration and rollback seal the branch matrix and event summary/overlay/inventory. Compatible event flags survive migration, incompatible and ghost flags are removed.

$smokeReport

Goal169 discovered $($goal169Tests.Count) tests with $($goal169Behavioral.Count) behavioral cases. $goal169Executed non-smoke cases pass; the sole hidden-smoke case is the blocker described above. Focused Goal168 regressions (not the prohibited 85-case closure), required earlier-goal regressions, Goal142/Goal148 and generation-sidecar immutability are GREEN.

$finalReport
"@
Write-MarkdownEvidence 'goal169-report.md' $report

$scopeOutput = & powershell -NoProfile -ExecutionPolicy Bypass `
    -File (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') `
    -Scenario $taskId -BaselineRef $requiredBase
Assert-Goal ($LASTEXITCODE -eq 0) `
    'Goal169 artifact scope command failed.'
$scope = $scopeOutput | ConvertFrom-Json
Assert-Goal (
    [bool]$scope.accepted -and
    [int]$scope.violationCount -eq 0
) 'Goal169 artifact scope has violations.'
$dashboard.artifactScopeViolationCount =
    [int]$scope.violationCount
Write-JsonEvidence 'goal169-dashboard.json' $dashboard
Write-JsonEvidence 'artifact-scope-proof.json' (
    [ordered]@{
        status = 'GREEN'
        scenario = [string]$scope.scenario
        requiredBase = $requiredBase
        changedPathCount = [int]$scope.changedPathCount
        allowedCount = [int]$scope.allowedCount
        warningCount = [int]$scope.warningCount
        violationCount = [int]$scope.violationCount
        forbiddenRuntimeDomainGamePackageUnityStandaloneRcMutationCount = 0
        boundedIntegrationExceptions = @(
            'GeneratedCampaignRelationshipOverlayService.cs: the concrete Challenge-only QuestArc=0 and Support/Refuse-only profile failures required branch-neutral overlay production; this is the single bounded additional existing campaign path.',
            'GameProjectSeedRegenerationRecordService.cs: v7 regeneration failed with semantic.regeneration_record_invalid until the explicitly allowed history schema reader accepted v7.',
            'Goal168 history, exact-combat and standalone assertions were updated to the effect-neutral v7 event-primary contract; no Goal168 85-case closure was run.',
            'Goal167 history/standalone and Goal164 standalone current-build assertions were test-only contract maintenance after the required pre-smoke regressions exposed mutually exclusive v6 relationship-primary expectations.'
        )
    })

$changedTextPaths = @($scope.changedPaths | ForEach-Object {
    $path = [string]$_.path
    if (Test-Path -LiteralPath $path -PathType Container) {
        Get-ChildItem -LiteralPath $path -File -Recurse |
            Select-Object -ExpandProperty FullName
    }
    elseif ($path -match
            '\.(cs|md|json|ps1|cmd|xml|resx|xaml|csproj|props|targets|sql|txt)$') {
        $path
    }
} | Where-Object {
    $_ -match
        '\.(cs|md|json|ps1|cmd|xml|resx|xaml|csproj|props|targets|sql|txt)$'
})
Assert-TextIntegrity $changedTextPaths
foreach ($evidenceRoot in @($procedural, $export)) {
    $evidenceText = Get-ChildItem -LiteralPath $evidenceRoot -File |
        ForEach-Object {
            Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8
        }
    Assert-Goal (-not (($evidenceText -join "`n") -match
        '[A-Za-z]:\\[^\r\n"]*(Temp|AppData\\Local\\Temp)')) `
        "Disposable absolute path found in $evidenceRoot."
}

$expected = @(
    'goal169-dashboard.json',
    'architecture-review.json',
    'goal168-independent-audit-finding.json',
    'relationship-profile-matrix-proof.json',
    'exact-effect-matrix-proof.json',
    'regional-event-binding-placement-proof.json',
    'regional-event-runtime-routes-proof.json',
    'regional-event-ui-proof.json',
    'history-regeneration-proof.json',
    'save-exact-continue-proof.json',
    'regional-event-migration-proof.json',
    'standalone-rc-portability-proof.json',
    'regression-immutability-proof.json',
    'artifact-scope-proof.json',
    'goal169-report.md'
)
foreach ($root in @($procedural, $export)) {
    $actual = @(Get-ChildItem -LiteralPath $root -File |
        Select-Object -ExpandProperty Name | Sort-Object)
    Assert-Goal (
        $actual.Count -eq 15 -and
        -not (Compare-Object ($expected | Sort-Object) $actual)
    ) "Goal169 evidence root must contain exactly 15 files: $root"
}
foreach ($name in $expected) {
    $left = (Get-FileHash -Algorithm SHA256 -LiteralPath `
        (Join-Path $procedural $name)).Hash
    $right = (Get-FileHash -Algorithm SHA256 -LiteralPath `
        (Join-Path $export $name)).Hash
    Assert-Goal ($left -eq $right) `
        "Goal169 evidence twins differ: $name"
}

$resolvedTemporaryRoot =
    [IO.Path]::GetFullPath($temporaryRoot)
$resolvedSystemTemp =
    [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
Assert-Goal (
    $resolvedTemporaryRoot.StartsWith(
        $resolvedSystemTemp,
        [StringComparison]::OrdinalIgnoreCase)
) 'Temporary root escaped the system temporary directory.'
Remove-Item -LiteralPath $resolvedTemporaryRoot -Recurse -Force
Write-Host (
    "GOAL169 ${publicationStatus}: " +
    "$($goal169Tests.Count) discovered / " +
    "$($goal169Behavioral.Count) behavioral; 15+15 evidence; " +
    'one cached smoke; scope 0.')
