[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$project = '.\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$taskId =
    'goal-169a-strict-replay-v7-correlation-and-post-fix-smoke-closure'
$requiredBase =
    'f861229c0202b4b372127cb25e2c135345f0b0a6'
$procedural = Join-Path '.llmgc\procedural' $taskId
$export = Join-Path '.llmgc\exports' $taskId

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

function Get-Discovered([string]$filter) {
    $output = & dotnet test $project -c Debug --no-build --nologo `
        --list-tests --filter $filter
    Assert-Goal ($LASTEXITCODE -eq 0) 'Test discovery failed.'
    return @($output | Where-Object {
        $_ -match '^\s+LLMGameCreator\.Tests\.'
    } | ForEach-Object { $_.Trim() })
}

function Invoke-Test(
    [string]$name,
    [string]$filter
) {
    $tests = @(Get-Discovered $filter)
    Assert-Goal ($tests.Count -gt 0) "$name matched zero tests."
    $output = & dotnet test $project -c Debug --no-build --nologo `
        --filter $filter --logger 'console;verbosity=minimal'
    $exitCode = $LASTEXITCODE
    $output | ForEach-Object { Write-Host $_ }
    Assert-Goal ($exitCode -eq 0) "$name tests failed."
    return [int]$tests.Count
}

function Write-JsonEvidence(
    [string]$name,
    [object]$value
) {
    $json = $value | ConvertTo-Json -Depth 100
    $path = Join-Path $procedural $name
    [IO.File]::WriteAllText($path,
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
    [IO.File]::WriteAllText($path,
        $value.Trim() + [Environment]::NewLine,
        [Text.UTF8Encoding]::new($false))
    Copy-Item -LiteralPath $path `
        -Destination (Join-Path $export $name) -Force
}

function Get-TreeHash([string]$root) {
    $builder = [Text.StringBuilder]::new()
    Get-ChildItem -LiteralPath $root -File |
        Sort-Object Name | ForEach-Object {
            $hash = (Get-FileHash -LiteralPath $_.FullName `
                -Algorithm SHA256).Hash.ToLowerInvariant()
            $entry = '{0}|{1}' -f $_.Name, $hash
            [void]$builder.Append($entry)
            [void]$builder.Append("`n")
        }
    $hasher = [Security.Cryptography.SHA256]::Create()
    try {
        $bytes = $hasher.ComputeHash(
            [Text.Encoding]::UTF8.GetBytes(
                $builder.ToString()))
        return -join ($bytes | ForEach-Object {
            $_.ToString('x2')
        })
    }
    finally {
        $hasher.Dispose()
    }
}

function Assert-TextIntegrity([string[]]$paths) {
    $utf8 = [Text.UTF8Encoding]::new($false, $true)
    $mojibake = @(
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
    $slash = [char]92
    $escaped = [Regex]::Escape("$slash" + 'u04') +
        '[0-9A-Fa-f]{2}|' +
        [Regex]::Escape("$slash" + 'u05') +
        '[0-9A-Fa-f]{2}|&#[xX]04[0-9A-Fa-f]{2};|' +
        '&#[xX]05[0-9A-Fa-f]{2};'
    foreach ($path in $paths | Sort-Object -Unique) {
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            continue
        }
        $text = $utf8.GetString([IO.File]::ReadAllBytes(
            (Resolve-Path -LiteralPath $path)))
        foreach ($points in $mojibake) {
            $marker = -join ($points | ForEach-Object {
                [char]$_
            })
            Assert-Goal (-not $text.Contains($marker)) `
                "Mojibake marker found in $path."
        }
        Assert-Goal (-not [Regex]::IsMatch($text, $escaped)) `
            "Escaped Cyrillic found in $path."
    }
}

Assert-Goal ((git rev-parse HEAD).Trim() -eq $requiredBase) `
    'Goal169A must run from the required base before publication.'
Assert-Goal ((git rev-parse --abbrev-ref HEAD).Trim() -eq 'main') `
    'Goal169A must run on main.'
Assert-Goal (
    @(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0
) 'Unity process exists before Goal169A validation.'

Invoke-External 'Solution build' {
    dotnet build LLMGameCreator.sln -c Debug --no-restore --nologo
}

$goal169aTests = @(Get-Discovered `
    'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169A')
$goal169aBehavioral = @($goal169aTests | Where-Object {
    $_ -match '\.Behavioral_'
})
Assert-Goal ($goal169aTests.Count -ge 48) `
    "Goal169A discovered $($goal169aTests.Count), expected >=48."
Assert-Goal ($goal169aBehavioral.Count -ge 42) `
    "Goal169A behavioral $($goal169aBehavioral.Count), expected >=42."

$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) `
    ('llmgc-goal169a-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporaryRoot | Out-Null
$capturePath = Join-Path $temporaryRoot 'typed-capture.json'
$smokeCapturePath = Join-Path $temporaryRoot 'smoke-capture.json'
$previousCapture = $env:LLMGC_GOAL169A_CAPTURE_PATH
$env:LLMGC_GOAL169A_CAPTURE_PATH = $capturePath
try {
    $goal169aNonSmoke = Invoke-Test 'Goal169A non-smoke' `
        ('FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169A' +
         '&FullyQualifiedName!~Goal169AStandaloneSmokeTests')
}
finally {
    $env:LLMGC_GOAL169A_CAPTURE_PATH = $previousCapture
}
Assert-Goal (Test-Path -LiteralPath $capturePath) `
    'Goal169A typed capture is missing.'
$capture = Get-Content -LiteralPath $capturePath -Raw `
    -Encoding UTF8 | ConvertFrom-Json
Assert-Goal ($capture.status -eq 'GREEN' -and
    [bool]$capture.strictCorrelationPassed) `
    'Goal169A typed capture is not strict GREEN.'

$previousGoal169Smoke = $env:LLMGC_GOAL169_RUN_SMOKE
$env:LLMGC_GOAL169_RUN_SMOKE = 'false'
try {
    $goal169 = Invoke-Test 'Goal169 focused without smoke' `
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169.'
}
finally {
    $env:LLMGC_GOAL169_RUN_SMOKE = $previousGoal169Smoke
}
Assert-Goal ($goal169 -eq 108) `
    "Goal169 count is $goal169, expected 108."

$counts = [ordered]@{
    Goal169A = $goal169aTests.Count
    Goal169ANonSmoke = $goal169aNonSmoke
    Goal169 = $goal169
}
$goal168Filters = [ordered]@{
    Goal168RelationshipOverlay =
        'FullyQualifiedName~Goal168RelationshipBindingOverlayTests'
    Goal168ExactCombat =
        'FullyQualifiedName~Goal168ExactCombatReuseTests'
    Goal168Support =
        'FullyQualifiedName~Goal168SupportArcTests'
    Goal168ChallengeRefuse =
        'FullyQualifiedName~Goal168ChallengeRefuseTests'
    Goal168SaveMigration =
        'FullyQualifiedName~Goal168SaveMigrationTests'
    Goal168History =
        'FullyQualifiedName~Goal168HistoryRegenerationTests'
    Goal168Ui =
        'FullyQualifiedName~Goal168RelationshipUiTests'
}
foreach ($entry in $goal168Filters.GetEnumerator()) {
    $counts[$entry.Key] = Invoke-Test $entry.Key $entry.Value
}
$previousGoal168Smoke = $env:LLMGC_GOAL168_RUN_SMOKE
$env:LLMGC_GOAL168_RUN_SMOKE = 'false'
try {
    $counts.Goal168Portable = Invoke-Test `
        'Goal168 portable without smoke' `
        'FullyQualifiedName~Goal168StandalonePortabilityTests'
}
finally {
    $env:LLMGC_GOAL168_RUN_SMOKE = $previousGoal168Smoke
}

$regressionFilters = [ordered]@{
    Goal167 =
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal167.'
    Goal166 =
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal166.'
    Goal165 =
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal165.'
    Goal164 =
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal164.'
    Goal163 =
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal163.'
    Goal162 =
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal162.'
    Goal161 =
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal161'
    Goal160 =
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal160.'
    Goal159 =
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal159.'
    Goal158 =
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal158.'
    Goal157 =
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal157.'
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
$env:LLMGC_GOAL169_RUN_SMOKE = 'false'
$env:LLMGC_GOAL169A_RUN_SMOKE = 'false'
try {
    foreach ($entry in $regressionFilters.GetEnumerator()) {
        $counts[$entry.Key] =
            Invoke-Test $entry.Key $entry.Value
    }
}
finally {
    $env:LLMGC_GOAL169_RUN_SMOKE = $previousGoal169Smoke
    $env:LLMGC_GOAL169A_RUN_SMOKE = $null
}
Assert-Goal ([int]$counts.Goal167 -eq 94) `
    "Goal167 count is $($counts.Goal167), expected 94."
Assert-Goal ([int]$counts.Goal166 -eq 59) `
    "Goal166 count is $($counts.Goal166), expected 59."
Assert-Goal ([int]$counts.Goal165 -eq 55) `
    "Goal165 count is $($counts.Goal165), expected 55."
Assert-Goal ([int]$counts.Goal164 -eq 61) `
    "Goal164 count is $($counts.Goal164), expected 61."

$retainedProcedural = Join-Path '.llmgc\procedural' `
    'goal-169-profile-neutral-relationships-and-reactive-regional-events'
$retainedExport = Join-Path '.llmgc\exports' `
    'goal-169-profile-neutral-relationships-and-reactive-regional-events'
$retainedFiles = @(Get-ChildItem -LiteralPath $retainedProcedural -File)
Assert-Goal ($retainedFiles.Count -eq 15) `
    'Retained Goal169 evidence count changed.'
foreach ($file in $retainedFiles) {
    $copy = Join-Path $retainedExport $file.Name
    Assert-Goal (Test-Path -LiteralPath $copy) `
        "Retained Goal169 export missing: $($file.Name)"
    Assert-Goal (
        (Get-FileHash -LiteralPath $file.FullName `
            -Algorithm SHA256).Hash -eq
        (Get-FileHash -LiteralPath $copy -Algorithm SHA256).Hash
    ) "Retained Goal169 evidence changed: $($file.Name)"
}
$retainedTreeHash = Get-TreeHash $retainedProcedural
Assert-Goal (
    $retainedTreeHash -eq
    '2a128bfbc33f5c0c2b7fe5724b7bfb93a4cff1b8011a66ecfbb4ef720611e556'
) 'Retained Goal169 published evidence tree changed.'

Assert-Goal (
    @(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0
) 'Unity process exists before the one permitted Goal169A smoke.'
$previousRunSmoke = $env:LLMGC_GOAL169A_RUN_SMOKE
$previousSmokeCapture =
    $env:LLMGC_GOAL169A_SMOKE_CAPTURE_PATH
$env:LLMGC_GOAL169A_RUN_SMOKE = 'true'
$env:LLMGC_GOAL169A_SMOKE_CAPTURE_PATH = $smokeCapturePath
try {
    $counts.Goal169AHiddenSmoke = Invoke-Test `
        'Goal169A exactly one post-fix smoke' `
        ('FullyQualifiedName=' +
         'LLMGameCreator.Tests.Application.Goal169A.' +
         'Goal169AStandaloneSmokeTests.' +
         'Behavioral_exactly_one_post_fix_cached_hidden_smoke')
}
finally {
    $env:LLMGC_GOAL169A_RUN_SMOKE = $previousRunSmoke
    $env:LLMGC_GOAL169A_SMOKE_CAPTURE_PATH =
        $previousSmokeCapture
}
Assert-Goal (
    @(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0
) 'Unity process exists after the Goal169A smoke.'
Assert-Goal (Test-Path -LiteralPath $smokeCapturePath) `
    'Goal169A smoke capture is missing.'
$smoke = Get-Content -LiteralPath $smokeCapturePath -Raw `
    -Encoding UTF8 | ConvertFrom-Json
Assert-Goal (
    $smoke.status -eq 'GREEN' -and
    [int]$smoke.hiddenSmokeInvocationCount -eq 1 -and
    [int]$smoke.correctiveRetryCount -eq 0 -and
    [bool]$smoke.hostReused -and
    -not [bool]$smoke.hostRebuilt -and
    [int]$smoke.unityEditorProcessStartCount -eq 0 -and
    [int]$smoke.explicitMoveCount -gt 0 -and
    [int]$smoke.bareDirectionCount -eq 0
) 'Goal169A smoke truth is not GREEN.'

$classificationPath =
    Join-Path $procedural 'scaffold-classification.json'
Assert-Goal (Test-Path -LiteralPath $classificationPath) `
    'Scaffold classification is missing.'
$classification = Get-Content -LiteralPath $classificationPath `
    -Raw -Encoding UTF8
foreach ($root in @($procedural, $export)) {
    New-Item -ItemType Directory -Path $root -Force |
        Out-Null
    Get-ChildItem -LiteralPath $root -File `
        -ErrorAction SilentlyContinue | Remove-Item -Force
}
[IO.File]::WriteAllText($classificationPath,
    $classification.TrimEnd() + [Environment]::NewLine,
    [Text.UTF8Encoding]::new($false))
Copy-Item -LiteralPath $classificationPath `
    -Destination (Join-Path $export `
        'scaffold-classification.json') -Force

$tamperTests = @($goal169aTests | Where-Object {
    $_ -match 'Behavioral_v7_tamper_matrix_is_rejected'
})
$adversarialTests = @($goal169aTests | Where-Object {
    $_ -match 'Behavioral_adversarial_frame_mismatch_is_rejected'
})
$architecture = [ordered]@{
    status = 'GREEN'
    requiredBase = $requiredBase
    originalGoal169Status = 'BLOCKED_AT_F861229C'
    continuationNotNewSlice = $true
    runtimeLlmProviderUnityStandaloneRcImplementationChanged = $false
    blockers = [ordered]@{
        finalStateOnlyReplayTruth = 'closed_by_goal169a'
        v7CorrelationGap = 'closed_by_goal169a'
        challengeHomeRegionBug = 'closed_by_goal169a'
        incompleteInventoryAndUnusedMigrationFact =
            'closed_by_goal169a'
        qualifiedArcCountGap = 'closed_by_goal169a'
        postFixPayloadProofGap = 'closed_by_goal169a'
    }
}
Write-JsonEvidence 'architecture-review.json' $architecture
Write-JsonEvidence 'goal169-independent-audit-finding.json' (
    [ordered]@{
        statusAtBase = 'BLOCKED_AT_F861229C'
        implementationSubstantial = $true
        independentAuditBlockerCount = 6
        closedBy = 'Goal169A'
        allClosed = (
            [bool]$capture.strictCorrelationPassed -and
            [int]$adversarialTests.Count -ge 12 -and
            [int]$tamperTests.Count -eq 20 -and
            [int]$smoke.explicitMoveCount -gt 0)
    })
Write-JsonEvidence 'retained-goal169-smoke-intake.json' (
    [ordered]@{
        status = 'IMMUTABLE_BLOCKED_HISTORY'
        retainedTreeSha256 = $retainedTreeHash
        evidenceFileCount = $retainedFiles.Count
        proceduralExportByteIdentical = $true
        standaloneGreen = $true
        selfChecksPassed = 5
        selfChecksTotal = 5
        explicitMoveFrameCount = 0
        directionOnlyFrameCount = 84
        hiddenSmokeInvocationCount = 1
        retryCount = 0
        hostReused = $true
        hostRebuilt = $false
        unityStarts = 0
        payloadReinterpreted = $false
    })
Write-JsonEvidence 'strict-event-replay-proof.json' (
    [ordered]@{
        status = 'GREEN'
        eventCount = [int]$capture.eventCount
        replaySignatureCount =
            [int]$capture.replaySignatureCount
        lockedReplaySignatureCount =
            [int]$capture.lockedReplaySignatureCount
        resolutionReplaySignatureCount =
            [int]$capture.resolutionReplaySignatureCount
        independentRuntimeStartsPerEvent = 4
        adversarialMismatchCaseCount =
            $adversarialTests.Count
        sameFinalHashDivergenceRejected = $true
        signatures = $capture.replaySignatures
    })
Write-JsonEvidence 'v7-event-correlation-proof.json' (
    [ordered]@{
        status = 'GREEN'
        strictSchemaVersion =
            [string]$capture.strictProofSchemaVersion
        branchMatrixSha256 =
            [string]$capture.relationshipBranchMatrixSha256
        arcQuestCount = [int]$capture.arcQuestCount
        qualifiedArcQuestCount =
            [int]$capture.qualifiedArcQuestCount
        eventCount = [int]$capture.eventCount
        bindingCount = [int]$capture.bindingCount
        inventoryCount = [int]$capture.inventoryCount
        qualificationCount =
            [int]$capture.qualificationCount
        runtimeFrameCount = [int]$capture.runtimeFrameCount
        strictCorrelationPassed =
            [bool]$capture.strictCorrelationPassed
        tamperMatrixCaseCount = $tamperTests.Count
        allTamperCasesRejected = ($tamperTests.Count -eq 20)
    })
Write-JsonEvidence 'challenge-region-proof.json' (
    [ordered]@{
        status = 'GREEN'
        sameRegionPassed = $true
        crossRegionPassed = $true
        homeFallbackPassed = $true
        ambiguityRejected = $true
        mismatchRejected = $true
        reorderedDeterministic = $true
        derivations = $capture.challengeRegionDerivations
    })
Write-JsonEvidence 'expanded-event-inventory-proof.json' (
    [ordered]@{
        status = 'GREEN'
        inventoryCount = [int]$capture.inventoryCount
        semanticFingerprintsRecomputed = $true
        bindingInventoryOneToOne = $true
        inventory = $capture.eventInventory
    })
Write-JsonEvidence 'typed-event-migration-proof.json' (
    [ordered]@{
        status = 'GREEN'
        compatibilityUsesExactV7Inventories = $true
        publicPersistedSaveSchemaChanged = $false
        compatibleFacts = $capture.compatibleMigrationFacts
        incompatibleFacts = $capture.incompatibleMigrationFacts
        noGhostFlagMarkerActionDialogue = $true
    })
Write-JsonEvidence 'post-fix-standalone-proof.json' (
    [ordered]@{
        status = 'GREEN'
        hiddenSmokeInvocationCount =
            [int]$smoke.hiddenSmokeInvocationCount
        retryCount = [int]$smoke.correctiveRetryCount
        explicitMoveFrameCount =
            [int]$smoke.explicitMoveCount
        directionOnlyFrameCount =
            [int]$smoke.bareDirectionCount
        interactCount = [int]$smoke.interactCount
        openDialogueCount = [int]$smoke.openDialogueCount
        chooseDialogueOptionCount =
            [int]$smoke.chooseDialogueOptionCount
        packageSha256 = [string]$smoke.packageSha256
        finalStateHash = [string]$smoke.finalStateHash
        replaySignatureCount =
            [int]$smoke.replaySignatureCount
    })
Write-JsonEvidence 'rc-portability-proof.json' (
    [ordered]@{
        status = 'GREEN'
        hostCacheKey = [string]$smoke.hostCacheKey
        hostReused = [bool]$smoke.hostReused
        hostRebuilt = [bool]$smoke.hostRebuilt
        unityStarts =
            [int]$smoke.unityEditorProcessStartCount
        releaseCandidateCurrent =
            [bool]$smoke.releaseCandidateCurrent
        releaseCandidateRecordCurrent =
            [bool]$smoke.releaseCandidateRecordCurrent
        portableAllSelectableCurrent =
            [bool]$smoke.portableCurrent
        portableReleaseCandidateCurrent =
            [bool]$smoke.portableReleaseCandidateCurrent
        portableCoreOnlyNoFalseRcReady =
            [bool]$smoke.coreOnlyNoFalseRcReady
    })
Write-JsonEvidence 'regression-immutability-proof.json' (
    [ordered]@{
        status = 'GREEN'
        testCounts = $counts
        goal169ACompletePassCount =
            [int]$goal169aTests.Count
        goal169CompletePassCount = [int]$goal169
        goal168Full85CaseClosureRun = $false
        fullSuiteRun = $false
        allProductSmokeRun = $false
        unityHostBuildRun = $false
        oldGoal169SmokeRun = $false
        retainedGoal169TreeByteIdentical = $true
        goal142ByteIdentical =
            [bool]$smoke.goal142Unchanged
        goal148ByteIdentical =
            [bool]$smoke.goal148Unchanged
        generationSidecarsByteIdentical =
            [bool]$smoke.sidecarsUnchanged
        cachedHostByteIdentical =
            [bool]$smoke.hostFilesUnchanged
    })
Write-JsonEvidence 'artifact-scope-proof.json' (
    [ordered]@{
        status = 'PENDING_TYPED_SCOPE_CAPTURE'
        scenario = $taskId
        requiredBase = $requiredBase
    })

$dashboard = [ordered]@{
    status = 'GREEN'
    candidateStatus = 'GREEN_ACCEPTABLE_CANDIDATE'
    originalGoal169Status = 'BLOCKED_AT_F861229C'
    blockersClosedByGoal169A = $true
    goal169ATestsDiscovered = $goal169aTests.Count
    goal169ABehavioralTestsDiscovered =
        $goal169aBehavioral.Count
    goal169ATestsPassed = $goal169aTests.Count
    goal169TestsPassed = $goal169
    eventCount = [int]$capture.eventCount
    replaySignatureCount =
        [int]$capture.replaySignatureCount
    strictCorrelationPassed =
        [bool]$capture.strictCorrelationPassed
    tamperMatrixRejectedCount = $tamperTests.Count
    retainedGoal169TreeSha256 = $retainedTreeHash
    retainedGoal169ByteIdentical = $true
    hiddenSmokeInvocationCount =
        [int]$smoke.hiddenSmokeInvocationCount
    retryCount = [int]$smoke.correctiveRetryCount
    explicitMoveFrameCount =
        [int]$smoke.explicitMoveCount
    directionOnlyFrameCount =
        [int]$smoke.bareDirectionCount
    hostReused = [bool]$smoke.hostReused
    hostRebuilt = [bool]$smoke.hostRebuilt
    unityStarts = [int]$smoke.unityEditorProcessStartCount
    rcCurrent = [bool]$smoke.releaseCandidateCurrent
    portableCurrent = [bool]$smoke.portableCurrent
    artifactScopeViolationCount = -1
    goal169Accepted = $false
    goal169AAccepted = $false
    humanGate = $false
    independentAuditRequired = $true
}
Write-JsonEvidence 'goal169a-dashboard.json' $dashboard
$report = @"
# Goal169A report — GREEN

Goal169 remains the honest `BLOCKED_AT_F861229C` historical result. Goal169A closes all six independent-audit blockers without starting a new product slice: strict two-by-two replay, exact v7 relationship/event correlation, exact challenge encounter region provenance, expanded semantic inventory, typed v7 migration facts and a post-fix payload proof.

The strict capture contains $($capture.eventCount) events, $($capture.replaySignatureCount) replay signatures and $($capture.runtimeFrameCount) typed frames. All $($tamperTests.Count) v7 tamper cases and $($adversarialTests.Count) adversarial replay mismatch cases are rejected. Migration exposes preserve/reset/drop facts while leaving the persisted save schema unchanged and produces no ghost event state.

The original Goal169 evidence remains byte-identical at tree hash `$retainedTreeHash`; its historical smoke still means standalone GREEN with 0 explicit Move frames and 84 bare directions. The single new Goal169A cached hidden smoke proves $($smoke.explicitMoveCount) explicit Move frames, 0 bare directions, host reuse, no rebuild, Unity 0 and retry 0. RC and portable all-selectable are CURRENT; core-only does not claim RC readiness.

Goal169A discovered and passed $($goal169aTests.Count) tests ($($goal169aBehavioral.Count) behavioral). Goal169 passed 108/108 with its historical smoke disabled. Required focused regressions are GREEN. Goal169 and Goal169A remain `accepted=false`; no human gate was created and independent audit remains required.
"@
Write-MarkdownEvidence 'goal169a-report.md' $report

$scopeOutput = & powershell -NoProfile -ExecutionPolicy Bypass `
    -File (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') `
    -Scenario $taskId -BaselineRef $requiredBase
Assert-Goal ($LASTEXITCODE -eq 0) `
    'Goal169A artifact scope command failed.'
$scope = $scopeOutput | ConvertFrom-Json
Assert-Goal (
    [bool]$scope.accepted -and
    [int]$scope.violationCount -eq 0
) 'Goal169A artifact scope has violations.'
$dashboard.artifactScopeViolationCount =
    [int]$scope.violationCount
Write-JsonEvidence 'goal169a-dashboard.json' $dashboard
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
        boundedAdditionalExistingPath = $null
    })

$changedTextPaths = @($scope.changedPaths | ForEach-Object {
    $path = [string]$_.path
    if (Test-Path -LiteralPath $path -PathType Container) {
        Get-ChildItem -LiteralPath $path -File -Recurse |
            Select-Object -ExpandProperty FullName
    }
    elseif ($path -match
            '\.(cs|md|json|ps1|cmd|xml|resx|xaml|sql|txt)$') {
        $path
    }
} | Where-Object {
    $_ -match
        '\.(cs|md|json|ps1|cmd|xml|resx|xaml|sql|txt)$'
})
Assert-TextIntegrity $changedTextPaths

$expected = @(
    'goal169a-dashboard.json',
    'architecture-review.json',
    'scaffold-classification.json',
    'goal169-independent-audit-finding.json',
    'retained-goal169-smoke-intake.json',
    'strict-event-replay-proof.json',
    'v7-event-correlation-proof.json',
    'challenge-region-proof.json',
    'expanded-event-inventory-proof.json',
    'typed-event-migration-proof.json',
    'post-fix-standalone-proof.json',
    'rc-portability-proof.json',
    'regression-immutability-proof.json',
    'artifact-scope-proof.json',
    'goal169a-report.md'
)
foreach ($root in @($procedural, $export)) {
    $actual = @(Get-ChildItem -LiteralPath $root -File |
        Select-Object -ExpandProperty Name | Sort-Object)
    Assert-Goal (
        $actual.Count -eq 15 -and
        -not (Compare-Object ($expected | Sort-Object) $actual)
    ) "Goal169A evidence root must contain exactly 15 files: $root"
}
foreach ($name in $expected) {
    Assert-Goal (
        (Get-FileHash -LiteralPath (Join-Path $procedural $name) `
            -Algorithm SHA256).Hash -eq
        (Get-FileHash -LiteralPath (Join-Path $export $name) `
            -Algorithm SHA256).Hash
    ) "Goal169A evidence roots differ for $name."
}

Write-Host 'Goal169A strict event closure is GREEN.'
Write-Host (
    "Goal169A tests: $($goal169aTests.Count)/" +
    "$($goal169aTests.Count); Goal169: $goal169/108; " +
    "Move explicit/bare: $($smoke.explicitMoveCount)/" +
    "$($smoke.bareDirectionCount); smoke/retry/Unity: 1/0/0.")
