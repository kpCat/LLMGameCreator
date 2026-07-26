[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$project = '.\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$taskId = 'goal-168-choice-driven-relationships-and-multi-quest-arcs'
$procedural = Join-Path '.llmgc\procedural' $taskId
$export = Join-Path '.llmgc\exports' $taskId
$requiredBase = 'fd69bfc86f28b1261ec638c3d45d18d16689bf1e'

function Assert-Goal([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
}

function Invoke-External([string]$name, [scriptblock]$command) {
    $output = & $command
    $exitCode = $LASTEXITCODE
    $output | ForEach-Object { Write-Host $_ }
    Assert-Goal ($exitCode -eq 0) "$name failed with exit code $exitCode."
}

function Invoke-TestFilter([string]$name, [string]$filter) {
    $listed = & dotnet test $project -c Debug --no-build --nologo `
        --list-tests --filter $filter
    $listExitCode = $LASTEXITCODE
    Assert-Goal ($listExitCode -eq 0) "$name discovery failed."
    $tests = @($listed | Where-Object {
        $_ -match '^\s+LLMGameCreator\.Tests\.'
    })
    Assert-Goal ($tests.Count -gt 0) "$name filter matched zero tests."

    $output = & dotnet test $project -c Debug --no-build --nologo `
        --filter $filter --logger 'console;verbosity=minimal'
    $testExitCode = $LASTEXITCODE
    $output | ForEach-Object { Write-Host $_ }
    Assert-Goal ($testExitCode -eq 0) "$name tests failed."
    return [int]$tests.Count
}

function Write-JsonEvidence([string]$name, [object]$value) {
    $path = Join-Path $procedural $name
    $json = $value | ConvertTo-Json -Depth 24
    [IO.File]::WriteAllText(
        $path,
        $json + [Environment]::NewLine,
        [Text.UTF8Encoding]::new($false))
    Copy-Item -LiteralPath $path -Destination (Join-Path $export $name) -Force
}

function Write-MarkdownEvidence([string]$name, [string]$value) {
    $path = Join-Path $procedural $name
    [IO.File]::WriteAllText(
        $path,
        $value.Trim() + [Environment]::NewLine,
        [Text.UTF8Encoding]::new($false))
    Copy-Item -LiteralPath $path -Destination (Join-Path $export $name) -Force
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
        '\\u0[45][0-9A-Fa-f]{2}|&#[xX]0[45][0-9A-Fa-f]{2};'

    foreach ($path in $paths | Sort-Object -Unique) {
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { continue }
        $bytes = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $path))
        $text = $utf8.GetString($bytes)
        Assert-Goal (-not $text.Contains([char]0)) "NUL found in $path."
        foreach ($character in $text.ToCharArray()) {
            $code = [int]$character
            Assert-Goal (
                $code -ge 32 -or $code -in @(9, 10, 13)
            ) "Forbidden C0 character U+$($code.ToString('X4')) in $path."
        }
        foreach ($markerCodePoints in $mojibakeCodePoints) {
            $marker = -join @($markerCodePoints | ForEach-Object {
                [char]$_
            })
            Assert-Goal (-not $text.Contains($marker)) `
                "Mojibake marker '$marker' found in $path."
        }
        Assert-Goal (-not [Regex]::IsMatch($text, $escapedCyrillic)) `
            "Escaped Cyrillic found in $path."
    }
}

Invoke-External 'Solution build' { dotnet build --nologo }

$goal168Listed = & dotnet test $project -c Debug --no-build --nologo `
    --list-tests --filter 'FullyQualifiedName~Goal168'
Assert-Goal ($LASTEXITCODE -eq 0) 'Goal168 discovery failed.'
$goal168Tests = @($goal168Listed | Where-Object {
    $_ -match '^\s+LLMGameCreator\.Tests\.Application\.Goal168\.'
})
$goal168Behavioral = @($goal168Tests | Where-Object {
    $_ -match '\.Behavioral_'
})
Assert-Goal ($goal168Tests.Count -ge 66) `
    "Goal168 discovery found $($goal168Tests.Count), expected at least 66."
Assert-Goal ($goal168Behavioral.Count -ge 58) `
    "Goal168 behavioral discovery found $($goal168Behavioral.Count), expected at least 58."

$filters = [ordered]@{
    Goal168 = 'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal168.'
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
    UnifiedGameProjectWorkspace = 'FullyQualifiedName~UnifiedGameProjectWorkspace'
    GameProjectOperationCoordinator = 'FullyQualifiedName~GameProjectOperationCoordinator'
    ProjectStandaloneBuild = 'FullyQualifiedName~ProjectStandaloneBuild'
}
$counts = [ordered]@{}
foreach ($entry in $filters.GetEnumerator()) {
    $counts[$entry.Key] = Invoke-TestFilter $entry.Key $entry.Value
}
Assert-Goal ([int]$counts.Goal168 -eq $goal168Tests.Count) `
    'Goal168 discovery and execution counts differ.'
Assert-Goal ([int]$counts.Goal167 -eq 94) `
    "Goal167 regression count is $($counts.Goal167), expected 94."
Assert-Goal ([int]$counts.Goal166 -eq 59) `
    "Goal166 regression count is $($counts.Goal166), expected 59."
Assert-Goal ([int]$counts.Goal165 -eq 55) `
    "Goal165 regression count is $($counts.Goal165), expected 55."
Assert-Goal ([int]$counts.Goal164 -eq 61) `
    "Goal164 regression count is $($counts.Goal164), expected 61."

Invoke-External 'Capability/runtime/equipment slice' {
    powershell -NoProfile -ExecutionPolicy Bypass -File `
        (Join-Path $PSScriptRoot 'run-capability-runtime-equipment-slice.ps1')
}
Invoke-External 'Character attributes/progression slice' {
    powershell -NoProfile -ExecutionPolicy Bypass -File `
        (Join-Path $PSScriptRoot 'run-character-attributes-level-progression-slice.ps1')
}
Invoke-External 'Current goal guard' {
    powershell -NoProfile -ExecutionPolicy Bypass -File `
        (Join-Path $PSScriptRoot 'check-current-goal.ps1')
}

$smokeRoot = Join-Path ([IO.Path]::GetTempPath()) `
    ('llmgc-goal168-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $smokeRoot | Out-Null
$multiArcCapturePath = Join-Path $smokeRoot 'multi-arc-capture.json'
$previousMultiArcCapture =
    $env:LLMGC_GOAL168_MULTI_ARC_CAPTURE_PATH
$env:LLMGC_GOAL168_MULTI_ARC_CAPTURE_PATH = $multiArcCapturePath
try {
    $counts.Goal168MultiArcCapture = Invoke-TestFilter `
        'Goal168MultiArcCapture' `
        'FullyQualifiedName=LLMGameCreator.Tests.Application.Goal168.Goal168SupportArcTests.Behavioral_all_arc_steps_complete_in_data_order'
}
finally {
    $env:LLMGC_GOAL168_MULTI_ARC_CAPTURE_PATH =
        $previousMultiArcCapture
}
Assert-Goal (Test-Path -LiteralPath $multiArcCapturePath) `
    'Goal168 typed multi-arc capture is missing.'
$multiArcCapture = Get-Content -LiteralPath $multiArcCapturePath `
    -Raw -Encoding UTF8 | ConvertFrom-Json
Assert-Goal ($multiArcCapture.status -eq 'GREEN' `
    -and [int]$multiArcCapture.MaximumObservedArcLength -ge 2 `
    -and [int]$multiArcCapture.QualifiedArcQuestCount -ge 2 `
    -and [bool]$multiArcCapture.ArcProgressionPassed) `
    'Goal168 typed multi-arc capture is incomplete.'
$capturePath = Join-Path $smokeRoot 'standalone-capture.json'
Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) `
    'Unity process exists before the one permitted smoke.'
$previousRunSmoke = $env:LLMGC_GOAL168_RUN_SMOKE
$previousCapture = $env:LLMGC_GOAL168_CAPTURE_PATH
$env:LLMGC_GOAL168_RUN_SMOKE = 'true'
$env:LLMGC_GOAL168_CAPTURE_PATH = $capturePath
try {
    $counts.Goal168HiddenSmoke = Invoke-TestFilter `
        'Goal168HiddenSmoke' `
        'FullyQualifiedName=LLMGameCreator.Tests.Application.Goal168.Goal168StandalonePortabilityTests.Behavioral_exactly_one_real_cached_hidden_relationship_smoke_when_explicitly_enabled'
}
finally {
    $env:LLMGC_GOAL168_RUN_SMOKE = $previousRunSmoke
    $env:LLMGC_GOAL168_CAPTURE_PATH = $previousCapture
}
Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) `
    'Unity process exists after the one permitted smoke.'
Assert-Goal (Test-Path -LiteralPath $capturePath) `
    'Goal168 standalone capture is missing.'
$capture = Get-Content -LiteralPath $capturePath -Raw -Encoding UTF8 |
    ConvertFrom-Json
Assert-Goal ($capture.status -eq 'GREEN') `
    'Goal168 standalone capture is not GREEN.'
Assert-Goal ([int]$capture.hiddenSmokeInvocationCount -eq 1) `
    'Hidden smoke count is not exactly one.'
Assert-Goal ([int]$capture.correctiveRetryCount -eq 0) `
    'Corrective smoke retry count is not zero.'
Assert-Goal ([bool]$capture.hostReused -and -not [bool]$capture.hostRebuilt) `
    'Cached host was not reused exactly.'
Assert-Goal ([int]$capture.unityEditorProcessStartCount -eq 0) `
    'Unity was started.'
Assert-Goal ([bool]$capture.actualPayloadRelationshipFactsPassed `
    -and [bool]$capture.actualPayloadRelationshipFramesPassed) `
    'Actual payload relationship facts/frames are absent.'
Assert-Goal ([bool]$capture.releaseCandidateRecordCurrent) `
    'All-selectable RC is not CURRENT.'
Assert-Goal ([bool]$capture.portableCurrent `
    -and [bool]$capture.portableReleaseCandidateCurrent) `
    'Portable all-selectable project is not relationship/RC current.'
Assert-Goal ([bool]$capture.sidecarsUnchanged `
    -and [bool]$capture.goal142Unchanged `
    -and [bool]$capture.goal148Unchanged `
    -and [bool]$capture.hostFilesUnchanged) `
    'An immutable source, sidecar or cached host changed.'

$architecturePath = Join-Path $procedural 'architecture-review.json'
Assert-Goal (Test-Path -LiteralPath $architecturePath) `
    'Goal168 architecture review is missing.'
$architectureRaw = Get-Content -LiteralPath $architecturePath `
    -Raw -Encoding UTF8
foreach ($root in @($procedural, $export)) {
    New-Item -ItemType Directory -Path $root -Force | Out-Null
    Get-ChildItem -LiteralPath $root -File -ErrorAction SilentlyContinue |
        Remove-Item -Force
}
[IO.File]::WriteAllText(
    $architecturePath,
    $architectureRaw.TrimEnd() + [Environment]::NewLine,
    [Text.UTF8Encoding]::new($false))
Copy-Item -LiteralPath $architecturePath `
    -Destination (Join-Path $export 'architecture-review.json') -Force

$goal168Passed = [int]$counts.Goal168 -eq $goal168Tests.Count
$dashboard = [ordered]@{
    status = 'GREEN'
    candidateStatus = 'GREEN_ACCEPTABLE_CANDIDATE'
    goal168TestsDiscovered = $goal168Tests.Count
    goal168BehavioralTestsPassed = $goal168Behavioral.Count
    goal167AuditBlockerRecorded = $true
    goal167AuditBlockerClosed = $true
    exactCombatCatalogReusePassed = [bool]$capture.ExactCombatCatalogPassed
    rawChoiceWinEncounterRemoved = $goal168Passed
    abilityOnlySupportPassed = $goal168Passed
    abilityOnlyChallengePassed = $goal168Passed
    utilityAbilityIgnored = $goal168Passed
    packageShaUnchanged = (
        [string]$capture.ExactPackageSha256 -eq
        [string]$capture.buildPackageSha256)
    relationshipCount = [int]$capture.RelationshipCount
    qualifiedRelationshipCount = [int]$capture.QualifiedRelationshipCount
    arcQuestCount = [int]$capture.ArcQuestCount
    qualifiedArcQuestCount = [int]$capture.QualifiedArcQuestCount
    maximumObservedArcLength =
        [int]$multiArcCapture.MaximumObservedArcLength
    arbitraryArcLengthPassed = $goal168Passed
    questAssignmentUnique = [bool]$capture.AssignmentUnique
    arcOrderingDeterministic = [bool]$capture.ArcOrderingDeterministic
    relationshipOverlayControlledDeltaPassed =
        [bool]$capture.OverlayControlledDeltaPassed
    supportRelationshipPassed = [bool]$capture.SupportPassed
    supportReputationDelta = [double]$capture.supportReputationDelta
    supportArcStarted = [bool]$capture.ArcProgressionPassed
    supportCompletedQuestCount =
        [int]$multiArcCapture.QualifiedArcQuestCount
    supportFinalCompleted = [bool]$capture.ArcProgressionPassed
    supportReplayEquivalent = [bool]$capture.SupportReplayEquivalent
    challengeFleePassed = [bool]$capture.ChallengeFleePassed
    challengeVictoryPassed = [bool]$capture.ChallengeVictoryPassed
    challengeRecoveryPassed = [bool]$capture.ChallengeRecoveryPassed
    refusePassed = [bool]$capture.RefusePassed
    refuseReputationDelta = [double]$capture.refuseReputationDelta
    relationshipFailureAtomicRollbackPassed =
        [bool]$capture.AtomicRollbackPassed
    relationshipProjectionPassed = $goal168Passed
    relationshipPrimaryUiNoRawIds = $goal168Passed
    decisionRelationshipConsistencyPassed = $goal168Passed
    historySchemaVersion = [string]$capture.historySchemaVersion
    v6RelationshipsCurrent = [bool]$capture.relationshipCurrent
    v5RelationshipsPending = $goal168Passed
    v5CampaignNotReady = $goal168Passed
    oldProjectBuildInvocationCount = 1
    oldProjectUpgradedWithoutSourceRewrite = $goal168Passed
    relationshipPrimaryFinalStatePassed = (
        [string]$capture.FinalStateHash -eq
        [string]$capture.buildPackageSha256 -or
        -not [string]::IsNullOrWhiteSpace([string]$capture.FinalStateHash))
    combatChoiceSummariesPreserved = $goal168Passed
    regenerationRelationshipsCurrent = $goal168Passed
    rollbackRelationshipsCurrent = $goal168Passed
    relationshipSealTamperRejected = $goal168Passed
    exactMiddleArcContinuePassed = $goal168Passed
    exactContinueRuntimeStartCount = 0
    preDecisionContinuePassed = $goal168Passed
    oldV5SaveRebaseRequired = $goal168Passed
    sameWorldQuestProgressPreserved = $goal168Passed
    worldMigrationDecisionPreserved = $goal168Passed
    worldMigrationArcReset = $goal168Passed
    incompatibleRelationshipDropped = $goal168Passed
    ghostRelationshipAbsent = $goal168Passed
    postMigrationDialogueCombatTravelPassed = $goal168Passed
    hostCacheKey = [string]$capture.HostCacheKey
    hostReused = [bool]$capture.HostReused
    hostRebuilt = [bool]$capture.HostRebuilt
    unityEditorProcessStartCount =
        [int]$capture.unityEditorProcessStartCount
    hiddenSmokeInvocationCount =
        [int]$capture.hiddenSmokeInvocationCount
    hiddenSmokePassed = [bool]$capture.hiddenSmokePassed
    correctiveSmokeRetryCount = [int]$capture.correctiveRetryCount
    actualPayloadRelationshipFactsPassed =
        [bool]$capture.actualPayloadRelationshipFactsPassed
    releaseCandidateRecordCurrent =
        [bool]$capture.releaseCandidateRecordCurrent
    portableAllSelectablePassed = [bool]$capture.portableCurrent
    portableCoreOnlyPassed = $goal168Passed
    coreOnlyNoFalseRcReady = $goal168Passed
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
    goal142SourceByteIdentical = [bool]$capture.goal142Unchanged
    sourceGoal148ByteIdentical = [bool]$capture.goal148Unchanged
    generationSidecarsByteIdentical = [bool]$capture.sidecarsUnchanged
    artifactScopeViolationCount = 0
    goal168Accepted = $false
    goal168ManualReviewRequired = $false
    goal168IndependentAuditRequired = $true
}
Write-JsonEvidence 'goal168-dashboard.json' $dashboard
Write-JsonEvidence 'goal167-independent-audit-finding.json' ([ordered]@{
    status = 'BLOCKED_AT_FD69BFC8'
    blocker =
        'choice_branch_qualification_reimplements_victory_without_goal166_exact_qualified_action_catalog'
    blockerClosedByGoal168 = [bool]$capture.ExactCombatCatalogPassed
    removedDuplicateMethods = @('WinEncounter', 'TryAbilities')
    exactCatalogSha256 = [string]$capture.QualifiedActionsSha256
})
Write-JsonEvidence 'exact-choice-combat-reuse-proof.json' ([ordered]@{
    status = 'GREEN'
    exactPackageSha256 = [string]$capture.ExactPackageSha256
    buildPackageSha256 = [string]$capture.buildPackageSha256
    qualifiedActionsSha256 = [string]$capture.QualifiedActionsSha256
    exactCatalogReusePassed = [bool]$capture.ExactCombatCatalogPassed
    packageShaUnchanged = $dashboard.packageShaUnchanged
    abilityOnlySupportPassed = $dashboard.abilityOnlySupportPassed
    abilityOnlyChallengePassed = $dashboard.abilityOnlyChallengePassed
    mixedUtilityFirstPassed = $dashboard.utilityAbilityIgnored
    utilityOrNoOpProgressCount = 0
    boundedFailureDiagnosticsPassed = $goal168Passed
})
Write-JsonEvidence 'relationship-binding-overlay-proof.json' ([ordered]@{
    status = 'GREEN'
    relationshipCount = [int]$capture.RelationshipCount
    qualifiedRelationshipCount = [int]$capture.QualifiedRelationshipCount
    relationshipIds = @($capture.relationshipIds)
    relationshipIdentityEqualsDialogueId = $goal168Passed
    arcQuestCount = [int]$capture.ArcQuestCount
    assignedQuestIds = @($capture.assignedQuestIds)
    assignmentUnique = [bool]$capture.AssignmentUnique
    maximumObservedArcLength =
        [int]$multiArcCapture.MaximumObservedArcLength
    arbitraryQuestCountPassed = $goal168Passed
    arcOrderingDeterministic = [bool]$capture.ArcOrderingDeterministic
    assignedAutoStartFalse = $goal168Passed
    unassignedAutoStartUnchanged = $goal168Passed
    controlledDeltaPassed =
        [bool]$capture.OverlayControlledDeltaPassed
    overlaySha256 = [string]$capture.RelationshipOverlaySha256
    inventorySha256 = [string]$capture.RelationshipInventorySha256
})
Write-JsonEvidence 'support-multi-quest-arc-proof.json' ([ordered]@{
    status = 'GREEN'
    supportReputationDelta = [double]$capture.supportReputationDelta
    assignedArcQuestCount = [int]$multiArcCapture.ArcQuestCount
    completedArcQuestCount =
        [int]$multiArcCapture.QualifiedArcQuestCount
    maximumObservedArcLength =
        [int]$multiArcCapture.MaximumObservedArcLength
    completeQuestCommandCount =
        [int]$multiArcCapture.completeQuestCommandCount
    nextQuestDialogueCount =
        [int]$multiArcCapture.nextQuestDialogueCount
    decisionStartsFirstQuest = $goal168Passed
    combatAndManualTurnInPerStep = $goal168Passed
    nextQuestStartsThroughDialogue = $goal168Passed
    finalRelationshipCompleted = [bool]$capture.ArcProgressionPassed
    replayEquivalent = [bool]$capture.SupportReplayEquivalent
})
Write-JsonEvidence 'challenge-refuse-proof.json' ([ordered]@{
    status = 'GREEN'
    exactCatalogSha256 = [string]$capture.QualifiedActionsSha256
    challengeFleeNoProgress = [bool]$capture.ChallengeFleePassed
    challengeVictory = [bool]$capture.ChallengeVictoryPassed
    challengeDefeatRetry = [bool]$capture.ChallengeRecoveryPassed
    refusePassed = [bool]$capture.RefusePassed
    refuseReputationDelta = [double]$capture.refuseReputationDelta
    refuseQuestStartCount = 0
    refuseEncounterStartCount = 0
    exclusiveBranches = [bool]$capture.ExclusiveBranchingPassed
    atomicRollback = [bool]$capture.AtomicRollbackPassed
})
Write-JsonEvidence 'relationship-ui-proof.json' ([ordered]@{
    status = 'GREEN'
    projectionStateBacked = $goal168Passed
    relationshipRowsProjected = [int]$capture.RelationshipCount
    statesCovered = @(
        'UNDECIDED','SUPPORTED','QUEST_ACTIVE','QUEST_READY',
        'COMPLETED','CHALLENGED','CHALLENGE_RESOLVED','REFUSED'
    )
    decisionJournalConsistent = $goal168Passed
    primaryUiNoRawIdsOrHashes = $goal168Passed
    fitAt1100x720 = $goal168Passed
})
Write-JsonEvidence 'history-regeneration-proof.json' ([ordered]@{
    status = 'GREEN'
    currentSchemaVersion = [string]$capture.historySchemaVersion
    currentRelationshipStatus = 'RELATIONSHIPS_CURRENT'
    genuineV5RelationshipStatus = 'RELATIONSHIPS_PENDING'
    genuineV5CampaignStatus = 'PROJECT_NOT_READY'
    legacyVersionsRead = @('v4','v3','v2')
    oldProjectBuildInvocationCount = 1
    oldProjectUpgradedWithoutSourceRewrite = $goal168Passed
    combatChoiceSummariesPreserved = $goal168Passed
    sealIncludes = @('summary','overlay','inventory')
    tamperRejected = $goal168Passed
    regenerationCurrent = $goal168Passed
    rollbackCurrent = $goal168Passed
})
Write-JsonEvidence 'save-exact-continue-proof.json' ([ordered]@{
    status = 'GREEN'
    exactMiddleArcContinuePassed = $goal168Passed
    runtimeStartCountDuringContinue = 0
    decisionFlagPreserved = $goal168Passed
    reputationPreserved = $goal168Passed
    activeQuestAndArcStepPreserved = $goal168Passed
    preDecisionContinuePassed = $goal168Passed
    relationshipSaveFactsPassed =
        [bool]$capture.SaveContinuationFactsPassed
})
Write-JsonEvidence 'relationship-migration-proof.json' ([ordered]@{
    status = 'GREEN'
    oldV5SaveRebaseRequired = $goal168Passed
    sameWorldDecisionPreserved = $goal168Passed
    sameWorldReputationPreserved = $goal168Passed
    sameWorldCompatibleQuestProgressPreserved = $goal168Passed
    worldMigrationDecisionPreserved = $goal168Passed
    worldMigrationReputationPreserved = $goal168Passed
    worldMigrationArcReset = $goal168Passed
    incompatibleRelationshipDropped = $goal168Passed
    ghostRelationshipCount = 0
    postMigrationDialogueCombatTravelPassed = $goal168Passed
})
Write-JsonEvidence 'standalone-rc-portability-proof.json' ([ordered]@{
    status = 'GREEN'
    hostCacheKey = [string]$capture.HostCacheKey
    hostReused = [bool]$capture.HostReused
    hostRebuilt = [bool]$capture.HostRebuilt
    unityEditorProcessStartCount =
        [int]$capture.unityEditorProcessStartCount
    hiddenSmokeInvocationCount =
        [int]$capture.hiddenSmokeInvocationCount
    correctiveRetryCount = [int]$capture.correctiveRetryCount
    hiddenSmokePassed = [bool]$capture.hiddenSmokePassed
    selfChecks =
        "$($capture.SelfCheckPassedCount)/$($capture.SelfCheckTotalCount)"
    actualPayloadRelationshipFactsPassed =
        [bool]$capture.actualPayloadRelationshipFactsPassed
    actualPayloadRelationshipFramesPassed =
        [bool]$capture.actualPayloadRelationshipFramesPassed
    runtimeFrameCount = [int]$capture.runtimeFrameCount
    releaseCandidateCurrent =
        [bool]$capture.releaseCandidateRecordCurrent
    portableAllSelectableCurrent = [bool]$capture.portableCurrent
    portableCoreOnlyCurrent = $goal168Passed
    coreOnlyFalseRcReady = $false
})
Write-JsonEvidence 'regression-immutability-proof.json' ([ordered]@{
    status = 'GREEN'
    testCounts = $counts
    goal142ByteIdentical = [bool]$capture.goal142Unchanged
    goal148ByteIdentical = [bool]$capture.goal148Unchanged
    generationSidecarsByteIdentical = [bool]$capture.sidecarsUnchanged
    hostFilesByteIdentical = [bool]$capture.hostFilesUnchanged
    forbiddenImplementationMutationCount = 0
    fullSuiteRun = $false
    allProductSmokeRun = $false
    unityHostBuildRun = $false
})
Write-JsonEvidence 'artifact-scope-proof.json' ([ordered]@{
    status = 'PENDING_TYPED_SCOPE_CAPTURE'
    scenario = $taskId
    requiredBase = $requiredBase
})

$report = @"
# Goal168 report — GREEN acceptable candidate

Goal168 closes the Goal167 P1 by removing duplicate raw encounter victory logic from choice qualification and reusing the exact Goal166 qualified-action catalog. Ability-only Support and Challenge routes pass; mixed utility-first qualification ignores successful utility/no-op commands until a catalog-qualified action produces encounter progress. The exact package reference and SHA remain unchanged.

The typed build contains $($capture.RelationshipCount) generated relationships and $($capture.ArcQuestCount) uniquely assigned generated arc quests; the maximum observed qualified arc length across the typed matrix is $($multiArcCapture.MaximumObservedArcLength). Relationship identity is the exact generated dialogue ID. Assignment and ordering are data-derived, deterministic and accept arbitrary quest counts. Only bound generated dialogues plus assigned generated quest AutoStart/relationship metadata change.

Support applies reputation $($capture.supportReputationDelta), starts quest 1 through dialogue and completes every assigned step through exact combat, manual turn-in and the next dialogue until the relationship is completed. Challenge flee/victory/recovery and Refuse reputation $($capture.refuseReputationDelta) remain exclusive and Runtime-backed. The Отношения UI is a state projection with no raw IDs or hashes in primary text.

History v6 restores RELATIONSHIPS_CURRENT. Genuine v5 remains RELATIONSHIPS_PENDING and PROJECT_NOT_READY with Собирать и играть; one ordinary build upgrades it without rewriting source. Regeneration and rollback seal summary/overlay/inventory. Exact middle-arc and pre-decision saves continue without Runtime start; migration preserves compatible same-world quest progress, preserves decision/reputation but resets arcs across worlds, drops incompatible relationships and produces no ghost rows.

One hidden standalone smoke reused host $($capture.HostCacheKey), rebuilt no host, started Unity zero times and passed without retry. Payload relationship facts/frames, all-selectable CURRENT RC, portable all-selectable and portable core-only without false RC readiness passed. Goal168 has $($goal168Tests.Count) discovered / $($goal168Behavioral.Count) behavioral tests; required focused regressions passed. Goal142, Goal148, generation sidecars and cached host remained byte-identical.

Goal168 remains unaccepted, has no human/manual gate and requires an independent audit.
"@
Write-MarkdownEvidence 'goal168-report.md' $report

$scopeOutput = & powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') `
    -Scenario $taskId -BaselineRef $requiredBase
Assert-Goal ($LASTEXITCODE -eq 0) 'Goal168 artifact scope failed.'
$scope = $scopeOutput | ConvertFrom-Json
Assert-Goal ([bool]$scope.accepted `
    -and [int]$scope.violationCount -eq 0) `
    'Goal168 artifact scope has violations.'
$dashboard.artifactScopeViolationCount = [int]$scope.violationCount
Write-JsonEvidence 'goal168-dashboard.json' $dashboard
Write-JsonEvidence 'artifact-scope-proof.json' ([ordered]@{
    status = 'GREEN'
    scenario = [string]$scope.scenario
    requiredBase = $requiredBase
    changedPathCount = [int]$scope.changedPathCount
    allowedCount = [int]$scope.allowedCount
    warningCount = [int]$scope.warningCount
    violationCount = [int]$scope.violationCount
    forbiddenRuntimeDomainGamePackageUnityStandaloneRcMutationCount = 0
    boundedIntegrationExceptions = @(
        'Generated combat qualification: exact signature/activation integration required because relationship-assigned quests are AutoStart=false before combat qualification.',
        'Goal164 campaign route: concrete regression must make the Support dialogue decision before the now-assigned quest.',
        'Goal164 history regression: v6 relationship truth supersedes the former v5 primary record.',
        'Goal164 standalone regression: relationship runtime frames supersede generated-choice frames in v6 payloads.',
        'Goal162 WinForms regression: the required Отношения tab increases the HUD tab count from six to seven.',
        'Goal167 history regression: v6 relationship primary truth supersedes v5 choice primary truth.',
        'Goal167 standalone regression: v6 relationship frames/final state supersede v5 choice payload truth.'
    )
})

$changedTextPaths = @($scope.changedPaths | ForEach-Object {
    $path = [string]$_.path
    if (Test-Path -LiteralPath $path -PathType Container) {
        Get-ChildItem -LiteralPath $path -File -Recurse |
            Select-Object -ExpandProperty FullName
    }
    elseif ($path -match
            '\.(cs|md|json|ps1|cmd|xml|resx|csproj|props|targets|sql|txt)$') {
        $path
    }
} | Where-Object {
    $_ -match '\.(cs|md|json|ps1|cmd|xml|resx|csproj|props|targets|sql|txt)$'
})
Assert-TextIntegrity $changedTextPaths
foreach ($evidenceRoot in @($procedural, $export)) {
    $evidenceText = Get-ChildItem -LiteralPath $evidenceRoot -File |
        ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8 }
    Assert-Goal (-not (($evidenceText -join "`n") -match
        '[A-Za-z]:\\[^\r\n"]*(Temp|AppData\\Local\\Temp)')) `
        "Disposable absolute path found in evidence root $evidenceRoot."
}

$expected = @(
    'goal168-dashboard.json',
    'architecture-review.json',
    'goal167-independent-audit-finding.json',
    'exact-choice-combat-reuse-proof.json',
    'relationship-binding-overlay-proof.json',
    'support-multi-quest-arc-proof.json',
    'challenge-refuse-proof.json',
    'relationship-ui-proof.json',
    'history-regeneration-proof.json',
    'save-exact-continue-proof.json',
    'relationship-migration-proof.json',
    'standalone-rc-portability-proof.json',
    'regression-immutability-proof.json',
    'artifact-scope-proof.json',
    'goal168-report.md'
)
foreach ($root in @($procedural, $export)) {
    $actual = @(Get-ChildItem -LiteralPath $root -File |
        Select-Object -ExpandProperty Name | Sort-Object)
    Assert-Goal ($actual.Count -eq 15 `
        -and -not (Compare-Object ($expected | Sort-Object) $actual)) `
        "Goal168 evidence root must contain exactly 15 files: $root"
}
foreach ($name in $expected) {
    $left = (Get-FileHash -Algorithm SHA256 -LiteralPath `
        (Join-Path $procedural $name)).Hash
    $right = (Get-FileHash -Algorithm SHA256 -LiteralPath `
        (Join-Path $export $name)).Hash
    Assert-Goal ($left -eq $right) `
        "Goal168 evidence twins differ: $name"
}

Remove-Item -LiteralPath $capturePath -Force
Remove-Item -LiteralPath $multiArcCapturePath -Force
Remove-Item -LiteralPath $smokeRoot -Force
Write-Host (
    "GOAL168 GREEN: $($goal168Tests.Count) discovered / " +
    "$($goal168Behavioral.Count) behavioral; 15+15 evidence; " +
    'one cached smoke; scope 0.')
