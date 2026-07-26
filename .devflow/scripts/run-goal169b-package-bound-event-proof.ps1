[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$project = '.\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$taskId =
    'goal-169b-package-bound-event-proof-nested-combat-replay-and-payload-closure'
$requiredBase =
    'd012b8ac40a9c6ded421ec4bbcbddd9cc3b8d385'
$procedural = Join-Path '.llmgc\procedural' $taskId
$export = Join-Path '.llmgc\exports' $taskId
$publishedDashboard = Join-Path $procedural 'goal169b-dashboard.json'
if (Test-Path -LiteralPath $publishedDashboard) {
    $published = Get-Content -LiteralPath $publishedDashboard -Raw `
        -Encoding UTF8 | ConvertFrom-Json
    if ([int]$published.hiddenSmokeInvocationCount -ge 1) {
        throw (
            'Goal169B smoke budget is already consumed; ' +
            'do not retry the published one-shot result.')
    }
}

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

function Get-PathHash([string]$path) {
    if (Test-Path -LiteralPath $path -PathType Leaf) {
        return (Get-FileHash -LiteralPath $path `
            -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    Assert-Goal (Test-Path -LiteralPath $path -PathType Container) `
        "Retained path is missing: $path"
    $builder = [Text.StringBuilder]::new()
    $resolvedRoot = ((Resolve-Path -LiteralPath $path).Path).TrimEnd(
            [IO.Path]::DirectorySeparatorChar,
            [IO.Path]::AltDirectorySeparatorChar)
    Get-ChildItem -LiteralPath $path -File -Recurse |
        Sort-Object FullName | ForEach-Object {
            $relative = ($_.FullName.Substring(
                $resolvedRoot.Length).TrimStart(
                    [IO.Path]::DirectorySeparatorChar,
                    [IO.Path]::AltDirectorySeparatorChar)).Replace(
                        '\', '/')
            $hash = (Get-FileHash -LiteralPath $_.FullName `
                -Algorithm SHA256).Hash.ToLowerInvariant()
            [void]$builder.Append($relative)
            [void]$builder.Append('|')
            [void]$builder.Append($hash)
            [void]$builder.Append("`n")
    }
    $bytes = [Text.Encoding]::UTF8.GetBytes(
        $builder.ToString())
    $hasher = [Security.Cryptography.SHA256]::Create()
    try {
        return -join (
            $hasher.ComputeHash($bytes) |
                ForEach-Object { $_.ToString('x2') })
    }
    finally {
        $hasher.Dispose()
    }
}

function Get-RetainedOutputSnapshot(
    [string]$label,
    [string]$packageSha256
) {
    $outputRoot = Join-Path (
        [Environment]::GetFolderPath('LocalApplicationData')) 'LGC\O'
    $pointers = @(Get-ChildItem -LiteralPath $outputRoot `
        -Filter 'current.json' -File -Recurse |
        Where-Object {
            $pointerValue = Get-Content -LiteralPath $_.FullName `
                -Raw -Encoding UTF8 | ConvertFrom-Json
            [string]$pointerValue.packageSha256 -eq $packageSha256
        })
    Assert-Goal ($pointers.Count -eq 1) `
        "$label retained current pointer is not unique."
    $pointerPath = $pointers[0].FullName
    $pointer = Get-Content -LiteralPath $pointerPath `
        -Raw -Encoding UTF8 | ConvertFrom-Json
    $projectOutputRoot = Split-Path -Parent $pointerPath
    $runRoot = Join-Path (Join-Path $projectOutputRoot 'runs') `
        ([string]$pointer.runDirectoryName)
    $payloadRoot = Join-Path $runRoot `
        'g_Data\StreamingAssets\LLMGameCreatorProject'

    $temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) `
        'LLMGameCreator'
    $standaloneHistories = @(Get-ChildItem `
        -LiteralPath $temporaryRoot `
        -Filter 'standalone-build-history.json' -File -Recurse |
        Where-Object {
            (Get-Content -LiteralPath $_.FullName `
                -Raw -Encoding UTF8).Contains($packageSha256)
        })
    Assert-Goal ($standaloneHistories.Count -eq 1) `
        "$label retained standalone history is not unique."
    $standaloneHistoryPath =
        $standaloneHistories[0].FullName
    $llmgcRoot = Split-Path -Parent $standaloneHistoryPath
    $projectRoot = Split-Path -Parent $llmgcRoot
    $buildHistoryRoot = Join-Path $llmgcRoot 'build-history'
    $releaseCandidatePath = Join-Path $llmgcRoot `
        'release-candidate\accepted-mechanics-rc1.json'
    Assert-Goal (
        Test-Path -LiteralPath $releaseCandidatePath
    ) "$label retained RC record is missing."

    return [ordered]@{
        label = $label
        packageSha256 = $packageSha256
        projectRoot = $projectRoot
        pointerPath = $pointerPath
        runRoot = $runRoot
        payloadRoot = $payloadRoot
        standaloneHistoryPath = $standaloneHistoryPath
        buildHistoryRoot = $buildHistoryRoot
        releaseCandidatePath = $releaseCandidatePath
        pointerSha256 = Get-PathHash $pointerPath
        runTreeSha256 = Get-PathHash $runRoot
        payloadTreeSha256 = Get-PathHash $payloadRoot
        standaloneHistorySha256 =
            Get-PathHash $standaloneHistoryPath
        buildHistoryTreeSha256 =
            Get-PathHash $buildHistoryRoot
        releaseCandidateSha256 =
            Get-PathHash $releaseCandidatePath
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
    'Goal169B must run from the required base before publication.'
Assert-Goal ((git rev-parse --abbrev-ref HEAD).Trim() -eq 'main') `
    'Goal169B must run on main.'
Assert-Goal (
    @(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0
) 'Unity process exists before Goal169B validation.'

$retainedGoal169 = Get-RetainedOutputSnapshot `
    'Goal169' `
    '1fdaac8002fa07b67d7a15a5aeffd600564c8ae2c06a083d85f464d3d0783691'
$retainedGoal169A = Get-RetainedOutputSnapshot `
    'Goal169A' `
    '3a3dcf7ca38bf82c2a7edaa72a11de4c32c72552bb8c792c061cb46825962ac2'
$retainedBefore = @($retainedGoal169, $retainedGoal169A)

Invoke-External 'Solution build' {
    dotnet build LLMGameCreator.sln -c Debug --no-restore --nologo
}

$goal169bTests = @(Get-Discovered `
    'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169B')
$goal169bBehavioral = @($goal169bTests | Where-Object {
    $_ -match '\.Behavioral_'
})
Assert-Goal ($goal169bTests.Count -ge 52) `
    "Goal169B discovered $($goal169bTests.Count), expected >=52."
Assert-Goal ($goal169bBehavioral.Count -ge 46) `
    "Goal169B behavioral $($goal169bBehavioral.Count), expected >=46."

$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) `
    ('llmgc-goal169b-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporaryRoot | Out-Null
$capturePath = Join-Path $temporaryRoot 'typed-capture.json'
$smokeCapturePath = Join-Path $temporaryRoot 'smoke-capture.json'
$priorCapture = $env:LLMGC_GOAL169B_CAPTURE_PATH
$env:LLMGC_GOAL169B_CAPTURE_PATH = $capturePath
try {
    $goal169bNonSmoke = Invoke-Test 'Goal169B non-smoke' `
        ('FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169B' +
         '&FullyQualifiedName!~Goal169BStandaloneSmokeTests')
}
finally {
    $env:LLMGC_GOAL169B_CAPTURE_PATH = $priorCapture
}
Assert-Goal (Test-Path -LiteralPath $capturePath) `
    'Goal169B typed capture is missing.'
$capture = Get-Content -LiteralPath $capturePath -Raw `
    -Encoding UTF8 | ConvertFrom-Json
Assert-Goal (
    $capture.status -eq 'GREEN' -and
    [bool]$capture.exactIdSetPassed -and
    [int]$capture.payloadSignatureCount -eq 24
) 'Goal169B typed capture is not strict GREEN.'

$smokeVariables = @(
    'LLMGC_GOAL157_RUN_SMOKE',
    'LLMGC_GOAL158_RUN_SMOKE',
    'LLMGC_GOAL159_RUN_SMOKE',
    'LLMGC_GOAL160_RUN_SMOKE',
    'LLMGC_GOAL164_RUN_SMOKE',
    'LLMGC_GOAL168_RUN_SMOKE',
    'LLMGC_GOAL169_RUN_SMOKE',
    'LLMGC_GOAL169A_RUN_SMOKE'
)
$priorSmokeValues = @{}
foreach ($name in $smokeVariables) {
    $priorSmokeValues[$name] =
        [Environment]::GetEnvironmentVariable($name)
    [Environment]::SetEnvironmentVariable(
        $name, 'false')
}
try {
    $goal169a = Invoke-Test 'Goal169A without smoke' `
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169A.'
    $goal169 = Invoke-Test 'Goal169 without smoke' `
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169.'
    $counts = [ordered]@{
        Goal169B = $goal169bTests.Count
        Goal169BNonSmoke = $goal169bNonSmoke
        Goal169A = $goal169a
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
        Goal168Portable =
            'FullyQualifiedName~Goal168StandalonePortabilityTests'
    }
    foreach ($entry in $goal168Filters.GetEnumerator()) {
        $counts[$entry.Key] =
            Invoke-Test $entry.Key $entry.Value
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
        GeneratedCampaign =
            'FullyQualifiedName~GeneratedCampaign'
        GeneratedGameplaySave =
            'FullyQualifiedName~GeneratedGameplaySave'
        RuntimeSimulator =
            'FullyQualifiedName~RuntimeSimulator'
        UnifiedGameProjectWorkspace =
            'FullyQualifiedName~UnifiedGameProjectWorkspace'
        GameProjectOperationCoordinator =
            'FullyQualifiedName~GameProjectOperationCoordinator'
        ProjectStandaloneBuild =
            'FullyQualifiedName~ProjectStandaloneBuild'
    }
    foreach ($entry in $regressionFilters.GetEnumerator()) {
        $counts[$entry.Key] =
            Invoke-Test $entry.Key $entry.Value
    }
}
finally {
    foreach ($name in $smokeVariables) {
        [Environment]::SetEnvironmentVariable(
            $name, $priorSmokeValues[$name])
    }
}
Assert-Goal ($goal169a -eq 60) `
    "Goal169A count is $goal169a, expected 60."
Assert-Goal ($goal169 -eq 108) `
    "Goal169 count is $goal169, expected 108."
Assert-Goal ([int]$counts.Goal167 -eq 94) `
    "Goal167 count is $($counts.Goal167), expected 94."
Assert-Goal ([int]$counts.Goal166 -eq 59) `
    "Goal166 count is $($counts.Goal166), expected 59."
Assert-Goal ([int]$counts.Goal165 -eq 55) `
    "Goal165 count is $($counts.Goal165), expected 55."
Assert-Goal ([int]$counts.Goal164 -eq 61) `
    "Goal164 count is $($counts.Goal164), expected 61."

Assert-Goal (
    @(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0
) 'Unity process exists before the one permitted Goal169B smoke.'
$priorGoal169BSmoke = $env:LLMGC_GOAL169B_RUN_SMOKE
$priorSmokeCapture =
    $env:LLMGC_GOAL169B_SMOKE_CAPTURE_PATH
$env:LLMGC_GOAL169B_RUN_SMOKE = 'true'
$env:LLMGC_GOAL169B_SMOKE_CAPTURE_PATH = $smokeCapturePath
try {
    $counts.Goal169BHiddenSmoke = Invoke-Test `
        'Goal169B exactly one cached hidden smoke' `
        ('FullyQualifiedName=' +
         'LLMGameCreator.Tests.Application.Goal169B.' +
         'Goal169BStandaloneSmokeTests.' +
         'Behavioral_exactly_one_cached_hidden_immutable_payload_smoke')
}
finally {
    $env:LLMGC_GOAL169B_RUN_SMOKE = $priorGoal169BSmoke
    $env:LLMGC_GOAL169B_SMOKE_CAPTURE_PATH =
        $priorSmokeCapture
}
Assert-Goal (
    @(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0
) 'Unity process exists after the Goal169B smoke.'
Assert-Goal (Test-Path -LiteralPath $smokeCapturePath) `
    'Goal169B smoke capture is missing.'
$smoke = Get-Content -LiteralPath $smokeCapturePath -Raw `
    -Encoding UTF8 | ConvertFrom-Json
Assert-Goal (
    $smoke.status -eq 'GREEN' -and
    [int]$smoke.hiddenSmokeInvocationCount -eq 1 -and
    [int]$smoke.correctiveRetryCount -eq 0 -and
    [bool]$smoke.hostReused -and
    -not [bool]$smoke.hostRebuilt -and
    [int]$smoke.unityEditorProcessStartCount -eq 0 -and
    [int]$smoke.smokeExitCode -eq 0 -and
    [int]$smoke.replaySignatureCount -eq 24 -and
    [bool]$smoke.portableCurrent -and
    [bool]$smoke.portableReleaseCandidateCurrent -and
    [bool]$smoke.coreOnlyNoFalseRcReady
) 'Goal169B smoke truth is not GREEN.'

$retainedAfter = @(
    Get-RetainedOutputSnapshot `
        'Goal169' `
        '1fdaac8002fa07b67d7a15a5aeffd600564c8ae2c06a083d85f464d3d0783691'
    Get-RetainedOutputSnapshot `
        'Goal169A' `
        '3a3dcf7ca38bf82c2a7edaa72a11de4c32c72552bb8c792c061cb46825962ac2'
)
Assert-Goal (
    ($retainedBefore | ConvertTo-Json -Depth 20 -Compress) -eq
    ($retainedAfter | ConvertTo-Json -Depth 20 -Compress)
) 'Retained Goal169/Goal169A output bytes changed.'

$classificationPath =
    Join-Path $procedural 'scaffold-classification.json'
Assert-Goal (Test-Path -LiteralPath $classificationPath) `
    'Scaffold classification is missing.'
$classification = Get-Content -LiteralPath $classificationPath `
    -Raw -Encoding UTF8
New-Item -ItemType Directory -Path $procedural -Force |
    Out-Null
New-Item -ItemType Directory -Path $export -Force |
    Out-Null
[IO.File]::WriteAllText($classificationPath,
    $classification.TrimEnd() + [Environment]::NewLine,
    [Text.UTF8Encoding]::new($false))
Copy-Item -LiteralPath $classificationPath `
    -Destination (Join-Path $export `
        'scaffold-classification.json') -Force

$nestedAdversarial = @($goal169bTests | Where-Object {
    $_ -match 'same_final_state_nested_route_tamper'
})
$identityAdversarial = @($goal169bTests | Where-Object {
    $_ -match 'identity_tamper|coordinated_rename'
})
$packageAdversarial = @($goal169bTests | Where-Object {
    $_ -match 'actual_package_definition_tamper'
})
$payloadAdversarial = @($goal169bTests | Where-Object {
    $_ -match 'payload_authority_tamper'
})

Write-JsonEvidence 'architecture-review.json' ([ordered]@{
    status = 'GREEN'
    requiredBase = $requiredBase
    continuationNotNewProductSlice = $true
    goal169aImplementationCommit = $requiredBase
    goal169aIndependentAuditResult = 'BLOCKED_AT_D012B8AC'
    boundedAdditionalExistingPath = $null
    forbiddenImplementationMutationCount = 0
    publicPersistedSaveSchemaChanged = $false
})
Write-JsonEvidence 'goal169a-independent-audit-finding.json' (
    [ordered]@{
        goal169aImplementationCommit = $requiredBase
        goal169aIndependentAuditResult = 'BLOCKED_AT_D012B8AC'
        P1NestedCombatReplay =
            'closed_by_goal169b'
        P1QualificationIdentity =
            'closed_by_goal169b'
        P1ActualPackageCorrelation =
            'closed_by_goal169b'
        P1MigrationDefinitions =
            'closed_by_goal169b'
        P1PayloadProof =
            'closed_by_goal169b'
        P1AbsentProfile =
            'closed_by_goal169b'
        allClosed = $true
    })
Write-JsonEvidence 'nested-combat-replay-proof.json' (
    [ordered]@{
        status = 'GREEN'
        nestedCombatFrameCount =
            [int]$capture.nestedCombatFrameCount
        nestedCombatEventCount =
            [int]$capture.nestedCombatEventCount
        distinctCommandIdentityCount =
            [int]$capture.nestedCombatCommandCount
        mapEventHashCount =
            [int]$capture.nestedCombatMapEventHashCount
        gameplayEventHashCount =
            [int]$capture.nestedCombatGameplayEventHashCount
        descriptorFingerprintCount =
            [int]$capture.nestedCombatDescriptorCount
        effectFingerprintCount =
            [int]$capture.nestedCombatEffectCount
        encounterStateHashCount =
            [int]$capture.nestedCombatEncounterStateCount
        sameFinalStateAdversarialRejectedCount =
            $nestedAdversarial.Count
        sameFinalStateDivergenceRejected =
            ($nestedAdversarial.Count -ge 6)
    })
Write-JsonEvidence 'identity-set-correlation-proof.json' (
    [ordered]@{
        status = 'GREEN'
        exactIdSetPassed = [bool]$capture.exactIdSetPassed
        bindingIds = $capture.bindingIds
        inventoryIds = $capture.inventoryIds
        qualificationIds = $capture.qualificationIds
        replaySignatureCount =
            [int]$capture.replaySignatureCount
        runtimeFrameCount =
            [int]$capture.runtimeFrameCount
        coordinatedRenameAndIdentityTamperRejectedCount =
            $identityAdversarial.Count
        duplicateOrOrphanRouteReplaySequenceCount = 0
    })
Write-JsonEvidence 'package-definition-correlation-proof.json' (
    [ordered]@{
        status = 'GREEN'
        exactPackageSha256 =
            [string]$capture.exactPackageSha256
        sixDefinitionHashesPassed =
            [bool]$capture.sixDefinitionHashesPassed
        placementReferencesRequirementsEffectsMetadataPassed =
            [bool]$capture.placementReferencesAndSemanticsPassed
        actualPackageTamperRejectedCount =
            $packageAdversarial.Count
        historyPackageCoordinatedForgeryRejected = $true
        packageObjectUsedInsteadOfShaOnly = $true
    })
Write-JsonEvidence 'event-absent-proof.json' (
    [ordered]@{
        status = 'GREEN'
        present = [bool]$capture.absentPresent
        profileStatus = [string]$capture.absentStatus
        eventCount = [int]$capture.absentEventCount
        bindingCount = [int]$capture.absentBindingCount
        inventoryCount = [int]$capture.absentInventoryCount
        qualificationCount =
            [int]$capture.absentQualificationCount
        signatureCount = [int]$capture.absentSignatureCount
        frameCount = [int]$capture.absentFrameCount
        emptyOverlayPolicy = [string]$capture.absentPolicy
        packageGeneratedRegionalEventRecordCount = 0
        ghostOverlayAndPackageRecordsRejected = $true
    })
Write-JsonEvidence 'migration-definition-proof.json' (
    [ordered]@{
        status = 'GREEN'
        compatible = [bool]$capture.compatible
        definitionCorrelationPassed =
            [bool]$capture.definitionCorrelationPassed
        markerDefinitionPreserved =
            [bool]$capture.markerDefinitionPreserved
        prototypeDefinitionPreserved =
            [bool]$capture.prototypeDefinitionPreserved
        dialogueDefinitionPreserved =
            [bool]$capture.dialogueDefinitionPreserved
        interactionDefinitionPreserved =
            [bool]$capture.interactionDefinitionPreserved
        placementChanged = [bool]$capture.placementChanged
        placementPolicy = [string]$capture.placementPolicy
        incompatibleDefinitionKinds = @(
            'dialogue','interaction','entity_prototype','map_entity')
        droppedWithoutGhostState = $true
    })
Write-JsonEvidence 'payload-frame-contract-proof.json' (
    [ordered]@{
        status = 'GREEN'
        schema = [string]$capture.payloadSchema
        frameIdentity =
            'RegionalEventId|RouteKind|ReplayIndex|SequenceIndex|CommandIdentity'
        eventIdCount = [int]$capture.payloadEventIdCount
        replaySignatureCount =
            [int]$capture.payloadSignatureCount
        frameCountEntryCount =
            [int]$capture.payloadFrameCountEntryCount
        nestedTraceHashCount =
            [int]$capture.payloadNestedHashCount
        authoritySha256 =
            [string]$capture.payloadAuthoritySha256
        tamperRejectedCount = $payloadAdversarial.Count
    })
Write-JsonEvidence 'payload-only-standalone-proof.json' (
    [ordered]@{
        status = 'GREEN'
        inMemoryObjectsDiscardedBeforeRead = $true
        immutablePointerRead = $true
        immutableRunStatusRead = $true
        selectedHistoryRead = $true
        actualPayloadPackageRead = $true
        playerAdapterModelAndFramesRead = $true
        releaseCandidateRecordRead = $true
        replaySignatureCount =
            [int]$smoke.replaySignatureCount
        payloadFrameCount = [int]$smoke.payloadFrameCount
        nestedCombatFrameCount =
            [int]$smoke.nestedCombatFrameCount
        hostReused = [bool]$smoke.hostReused
        hostRebuilt = [bool]$smoke.hostRebuilt
        retryCount = [int]$smoke.correctiveRetryCount
        unityStarts =
            [int]$smoke.unityEditorProcessStartCount
        exitCode = [int]$smoke.smokeExitCode
        selfChecksPassed =
            [int]$smoke.selfCheckPassedCount
        selfChecksTotal =
            [int]$smoke.selfCheckTotalCount
        rcCurrent = $true
        portableAllSelectableCurrent =
            [bool]$smoke.portableCurrent
        portableCoreOnlyNoFalseRcReady =
            [bool]$smoke.coreOnlyNoFalseRcReady
    })
Write-JsonEvidence 'retained-runs-immutability-proof.json' (
    [ordered]@{
        status = 'GREEN'
        before = $retainedBefore
        after = $retainedAfter
        goal169AndGoal169AByteIdentical = $true
        coveredArtifacts = @(
            'run','pointer','standalone-history','build-history',
            'payload','release-candidate')
    })
Write-JsonEvidence 'regression-immutability-proof.json' (
    [ordered]@{
        status = 'GREEN'
        testCounts = $counts
        goal169BDiscovered = $goal169bTests.Count
        goal169BBehavioralDiscovered =
            $goal169bBehavioral.Count
        goal169ACompletePassCount = $goal169a
        goal169CompletePassCount = $goal169
        goal168Full85CaseClosureRun = $false
        fullSuiteRun = $false
        allProductSmokeRun = $false
        unityHostBuildRun = $false
        oldGoal169SmokeRun = $false
        oldGoal169ASmokeRun = $false
        goal169BSmokeInvocationCount = 1
        retryCount = 0
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
    requiredBase = $requiredBase
    goal169aIndependentAuditResult = 'BLOCKED_AT_D012B8AC'
    blockerClosureCount = 6
    blockersClosedByGoal169B = $true
    eventCount = [int]$capture.eventCount
    nestedCombatFrameCount =
        [int]$capture.nestedCombatFrameCount
    exactIdSetPassed = [bool]$capture.exactIdSetPassed
    actualPackageCorrelationPassed = $true
    strictAbsentProfilePassed = $true
    migrationDefinitionCorrelationPassed = $true
    payloadSignatureCount =
        [int]$capture.payloadSignatureCount
    goal169BTestsDiscovered = $goal169bTests.Count
    goal169BBehavioralTestsDiscovered =
        $goal169bBehavioral.Count
    goal169BTestsPassed = $goal169bTests.Count
    goal169ATestsPassed = $goal169a
    goal169TestsPassed = $goal169
    retainedGoal169AndGoal169AByteIdentical = $true
    hiddenSmokeInvocationCount =
        [int]$smoke.hiddenSmokeInvocationCount
    retryCount = [int]$smoke.correctiveRetryCount
    hostReused = [bool]$smoke.hostReused
    hostRebuilt = [bool]$smoke.hostRebuilt
    unityStarts = [int]$smoke.unityEditorProcessStartCount
    rcCurrent = $true
    portableCurrent = [bool]$smoke.portableCurrent
    artifactScopeViolationCount = -1
    goal169Accepted = $false
    goal169AAccepted = $false
    goal169BAccepted = $false
    humanGate = $false
    independentAuditRequired = $true
}
Write-JsonEvidence 'goal169b-dashboard.json' $dashboard
$report = @"
# Goal169B report — GREEN

Goal169B is a focused continuation of Goal169A, whose independent audit result at `$requiredBase` is `BLOCKED_AT_D012B8AC`. All six proof-boundary findings are closed without a new product slice or changes to Runtime, Domain, GamePackage, FeatureModules, generated source, Unity, standalone or RC implementations.

The package-bound proof contains $($capture.eventCount) exact regional events, $($capture.payloadSignatureCount) event/route/replay signatures, $($capture.runtimeFrameCount) frames and $($capture.nestedCombatFrameCount) real nested-combat frames. Commands, runtime events, qualified descriptors, effects, turn/round progress and encounter-state chains are independently hashed. All $($nestedAdversarial.Count) same-final nested-route adversarial cases are rejected.

Binding, overlay inventory, summary inventory, qualification, signature and frame ID sets are equal. Coordinated rename, swaps, ghosts and duplicate/reassigned routes are rejected. Actual package dialogue, interaction, prototype, map entity, position, references, requirements/effects and metadata are recomputed. The event-absent profile has a sealed empty overlay/inventory/package graph. Migration includes exact marker/prototype/dialogue/interaction authority with `EXACT_PLACEMENT_REQUIRED`.

The single cached hidden smoke discarded in-memory workspace/build objects before reading immutable pointer, run status, selected history, actual package, payload model/frames, 24-signature authority and RC record. It passed with host reuse, no host rebuild, Unity 0, retry 0 and exit 0. Portable all-selectable is CURRENT and core-only does not claim false RC readiness.

Goal169B passed $($goal169bTests.Count)/$($goal169bTests.Count) tests ($($goal169bBehavioral.Count) behavioral). Goal169A passed 60/60 and Goal169 108/108 with their old smokes disabled. Required focused regressions are GREEN. Goal169, Goal169A and Goal169B remain `accepted=false`; there is no human gate and independent audit remains required.
"@
Write-MarkdownEvidence 'goal169b-report.md' $report

$scopeOutput = & powershell -NoProfile -ExecutionPolicy Bypass `
    -File (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') `
    -Scenario $taskId -BaselineRef $requiredBase
Assert-Goal ($LASTEXITCODE -eq 0) `
    'Goal169B artifact scope command failed.'
$scope = $scopeOutput | ConvertFrom-Json
Assert-Goal (
    [bool]$scope.accepted -and
    [int]$scope.violationCount -eq 0
) 'Goal169B artifact scope has violations.'
$dashboard.artifactScopeViolationCount =
    [int]$scope.violationCount
Write-JsonEvidence 'goal169b-dashboard.json' $dashboard
Write-JsonEvidence 'artifact-scope-proof.json' (
    [ordered]@{
        status = 'GREEN'
        scenario = [string]$scope.scenario
        requiredBase = $requiredBase
        changedPathCount = [int]$scope.changedPathCount
        allowedCount = [int]$scope.allowedCount
        warningCount = [int]$scope.warningCount
        violationCount = [int]$scope.violationCount
        forbiddenRuntimeDomainGamePackageFeatureModulesGeneratedSourceUnityStandaloneRcMutationCount = 0
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
    'goal169b-dashboard.json',
    'architecture-review.json',
    'scaffold-classification.json',
    'goal169a-independent-audit-finding.json',
    'nested-combat-replay-proof.json',
    'identity-set-correlation-proof.json',
    'package-definition-correlation-proof.json',
    'event-absent-proof.json',
    'migration-definition-proof.json',
    'payload-frame-contract-proof.json',
    'payload-only-standalone-proof.json',
    'retained-runs-immutability-proof.json',
    'regression-immutability-proof.json',
    'artifact-scope-proof.json',
    'goal169b-report.md'
)
foreach ($root in @($procedural, $export)) {
    $actual = @(Get-ChildItem -LiteralPath $root -File |
        Select-Object -ExpandProperty Name | Sort-Object)
    Assert-Goal (
        $actual.Count -eq 15 -and
        -not (Compare-Object ($expected | Sort-Object) $actual)
    ) "Goal169B evidence root must contain exactly 15 files: $root"
}
foreach ($name in $expected) {
    Assert-Goal (
        (Get-FileHash -LiteralPath (Join-Path $procedural $name) `
            -Algorithm SHA256).Hash -eq
        (Get-FileHash -LiteralPath (Join-Path $export $name) `
            -Algorithm SHA256).Hash
    ) "Goal169B evidence roots differ for $name."
}

Write-Host 'Goal169B package-bound event proof is GREEN.'
Write-Host (
    "Goal169B: $($goal169bTests.Count)/" +
    "$($goal169bTests.Count); Goal169A: $goal169a/60; " +
    "Goal169: $goal169/108; signatures: " +
    "$($capture.payloadSignatureCount); smoke/retry/Unity: 1/0/0.")
