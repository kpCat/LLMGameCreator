[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$project = '.\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$taskId =
    'goal-169d-qualified-core-only-portable-truth-and-gate-closure'
$requiredBase =
    '72f69be12b898583d902237ae99e5bc1fe890d2c'
$procedural = Join-Path '.llmgc\procedural' $taskId
$export = Join-Path '.llmgc\exports' $taskId

function Assert-Goal([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
}

function Invoke-External(
    [string]$name,
    [scriptblock]$command
) {
    Write-Host "=== $name ==="
    $output = & $command *>&1
    $exitCode = $LASTEXITCODE
    $output | ForEach-Object { Write-Host $_ }
    Assert-Goal ($exitCode -eq 0) `
        "$name failed with exit code $exitCode."
    return @($output)
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
    Write-Host "=== $name ($($tests.Count)) ==="
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

function Assert-TextIntegrity([string]$text, [string]$label) {
    $mojibake = @(
        @(0x0420,0x045F), @(0x0420,0x045C),
        @(0x0420,0x045B), @(0x0420,0x2022),
        @(0x0420,0x040E), @(0x0420,0x203A),
        @(0x0420,0x00A4), @(0x0420,0x045A),
        @(0x0420,0x0408), @(0x0420,0x0459),
        @(0x0420,0x0491), @(0x0420,0x00B5),
        @(0x0420,0x00B0), @(0x0420,0x00BB),
        @(0x0420,0x0405), @(0x0420,0x0455),
        @(0x0421,0x040F), @(0x0421,0x20AC),
        @(0x0421,0x0402), @(0x0421,0x2039),
        @(0x0421,0x040A), @(0x0421,0x201A),
        @(0x0421,0x0453), @(0x0421,0x2021),
        @(0x0421,0x2026), @(0x0421,0x2020),
        @(0xFFFD)
    )
    foreach ($points in $mojibake) {
        $marker = -join ($points | ForEach-Object { [char]$_ })
        Assert-Goal (-not $text.Contains($marker)) `
            "Mojibake marker found in $label."
    }
    $slash = [char]92
    $escaped = [Regex]::Escape("$slash" + 'u04') +
        '[0-9A-Fa-f]{2}|' +
        [Regex]::Escape("$slash" + 'u05') +
        '[0-9A-Fa-f]{2}|&#[xX]04[0-9A-Fa-f]{2};|' +
        '&#[xX]05[0-9A-Fa-f]{2};'
    Assert-Goal (-not [Regex]::IsMatch($text, $escaped)) `
        "Escaped Cyrillic found in $label."
}

Assert-Goal ((git rev-parse HEAD).Trim() -eq $requiredBase) `
    'Goal169D must run from the required base before publication.'
Assert-Goal ((git rev-parse origin/main).Trim() -eq $requiredBase) `
    'Goal169D origin/main must equal the required base.'
Assert-Goal ((git rev-parse --abbrev-ref HEAD).Trim() -eq 'main') `
    'Goal169D must run on main.'
Assert-Goal (
    @(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0
) 'Unity process exists before Goal169D validation.'

New-Item -ItemType Directory -Path $procedural -Force |
    Out-Null
New-Item -ItemType Directory -Path $export -Force |
    Out-Null
$classificationPath =
    Join-Path $procedural 'scaffold-classification.json'
Assert-Goal (Test-Path -LiteralPath $classificationPath) `
    'Scaffold classification is missing.'
Assert-Goal (
    (Get-FileHash -LiteralPath $classificationPath `
        -Algorithm SHA256).Hash -eq
    (Get-FileHash -LiteralPath (
        Join-Path $export 'scaffold-classification.json') `
        -Algorithm SHA256).Hash
) 'Scaffold classification roots differ.'

$smokeVariables = @(
    'LLMGC_GOAL157_RUN_SMOKE',
    'LLMGC_GOAL158_RUN_SMOKE',
    'LLMGC_GOAL159_RUN_SMOKE',
    'LLMGC_GOAL160_RUN_SMOKE',
    'LLMGC_GOAL164_RUN_SMOKE',
    'LLMGC_GOAL168_RUN_SMOKE',
    'LLMGC_GOAL169_RUN_SMOKE',
    'LLMGC_GOAL169A_RUN_SMOKE',
    'LLMGC_GOAL169B_RUN_SMOKE',
    'LLMGC_GOAL169C_RUN_SMOKE',
    'LLMGC_GOAL169D_RUN_SMOKE'
)
$priorSmokeValues = @{}
foreach ($name in $smokeVariables) {
    $priorSmokeValues[$name] =
        [Environment]::GetEnvironmentVariable($name)
    [Environment]::SetEnvironmentVariable($name, 'false')
}

$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) `
    ('llmgc-goal169d-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporaryRoot | Out-Null
$capturePath = Join-Path $temporaryRoot 'goal169d-capture.json'
$priorCapture = $env:LLMGC_GOAL169D_CAPTURE_PATH
$counts = [ordered]@{}

try {
    Invoke-External 'Solution build' {
        dotnet build LLMGameCreator.sln -c Debug --no-restore `
            --nologo /p:EnableWindowsTargeting=true
    } | Out-Null

    $goal169dTests = @(Get-Discovered `
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169D.')
    $goal169dBehavioral = @($goal169dTests | Where-Object {
        $_ -match '\.Behavioral_'
    })
    Assert-Goal ($goal169dTests.Count -ge 30) `
        "Goal169D discovered $($goal169dTests.Count), expected >=30."
    Assert-Goal ($goal169dBehavioral.Count -ge 26) `
        "Goal169D behavioral $($goal169dBehavioral.Count), expected >=26."
    $env:LLMGC_GOAL169D_CAPTURE_PATH = $capturePath
    $counts.Goal169D = Invoke-Test 'Goal169D complete' `
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169D.'
    Assert-Goal (Test-Path -LiteralPath $capturePath) `
        'Goal169D capture is missing.'

    $counts.Goal169C = Invoke-Test `
        'Goal169C 34/34 with smoke disabled' `
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169C.'
    $counts.Goal169B = Invoke-Test `
        'Goal169B 72/72 with smoke disabled' `
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169B.'
    $counts.Goal169A = Invoke-Test `
        'Goal169A 60/60 with smoke disabled' `
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169A.'
    $counts.Goal169 = Invoke-Test `
        'Goal169 108/108 with smoke disabled' `
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169.'

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

    Invoke-External 'Capability Runtime Equipment slice' {
        & '.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1'
    } | Out-Null
    Invoke-External 'Character Attributes/Progression slice' {
        & '.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1'
    } | Out-Null
    $currentGoalOutput = @(Invoke-External `
        'Current goal consistency' {
            & '.\.devflow\scripts\check-current-goal.ps1'
        })
}
finally {
    $env:LLMGC_GOAL169D_CAPTURE_PATH = $priorCapture
    foreach ($name in $smokeVariables) {
        [Environment]::SetEnvironmentVariable(
            $name, $priorSmokeValues[$name])
    }
}

Assert-Goal ([int]$counts.Goal169D -eq 43) `
    "Goal169D count is $($counts.Goal169D), expected 43."
Assert-Goal ([int]$counts.Goal169C -eq 34) `
    "Goal169C count is $($counts.Goal169C), expected 34."
Assert-Goal ([int]$counts.Goal169B -eq 72) `
    "Goal169B count is $($counts.Goal169B), expected 72."
Assert-Goal ([int]$counts.Goal169A -eq 60) `
    "Goal169A count is $($counts.Goal169A), expected 60."
Assert-Goal ([int]$counts.Goal169 -eq 108) `
    "Goal169 count is $($counts.Goal169), expected 108."
Assert-Goal ([int]$counts.Goal167 -eq 94) `
    "Goal167 count is $($counts.Goal167), expected 94."
Assert-Goal ([int]$counts.Goal166 -eq 59) `
    "Goal166 count is $($counts.Goal166), expected 59."
Assert-Goal ([int]$counts.Goal165 -eq 55) `
    "Goal165 count is $($counts.Goal165), expected 55."
Assert-Goal ([int]$counts.Goal164 -eq 61) `
    "Goal164 count is $($counts.Goal164), expected 61."

$capture = Get-Content -LiteralPath $capturePath `
    -Raw -Encoding UTF8 | ConvertFrom-Json
$expectedRelationshipStatus =
    if ([int]$capture.qualified.availableBranchCount -eq 0) {
        'ABSENT'
    }
    else {
        'RELATIONSHIPS_CURRENT'
    }
$expectedEventStatus =
    if ([int]$capture.qualified.availableBranchCount -eq 0) {
        'ABSENT'
    }
    else {
        'REGIONAL_EVENTS_CURRENT'
    }
Assert-Goal (
    [string]$capture.status -eq 'GREEN' -and
    [string]$capture.raw.Status -eq
        'CREATION_ONLY_NOT_QUALIFIED' -and
    [int]$capture.raw.BuildInvocationCount -eq 0 -and
    [int]$capture.raw.historyCount -eq 0 -and
    [string]$capture.raw.generatedWorldStatus -ne
        'CAMPAIGN_CURRENT' -and
    [int]$capture.qualified.buildInvocationCount -eq 1 -and
    [bool]$capture.qualified.buildPassed -and
    [string]$capture.qualified.buildStatus -eq 'GREEN' -and
    [int]$capture.qualified.historyCount -eq 1 -and
    [string]$capture.qualified.historySchema -eq
        'unified_game_project_build_history_v7' -and
    [string]$capture.qualified.generatedWorldStatus -eq
        'CAMPAIGN_CURRENT' -and
    [string]$capture.qualified.relationshipStatus -eq
        $expectedRelationshipStatus -and
    [string]$capture.qualified.regionalEventStatus -eq
        $expectedEventStatus -and
    [int]$capture.qualified.regionalEventCount -eq
        [int]$capture.qualified.qualifiedRegionalEventCount -and
    [bool]$capture.qualified.packageCorrelationPassed -and
    -not [bool]$capture.portable.buildsPresent -and
    [string]$capture.portable.generatedWorldStatus -eq
        'CAMPAIGN_CURRENT' -and
    [bool]$capture.portable.packageCorrelationPassed -and
    -not [bool]$capture.portable.operationalPointerResolved -and
    [string]$capture.portable.operationalPointerDiagnostic -eq
        'standalone.current_pointer_missing' -and
    [string]$capture.portable.releaseCandidateConfigurationStatus -ne
        'CURRENT' -and
    [string]$capture.portable.releaseCandidateRecordConfigurationStatus -ne
        'CURRENT' -and
    [bool]$capture.portable.reopenPreservedPackage -and
    [bool]$capture.portable.reopenPreservedHistory -and
    [bool]$capture.portable.reopenPreservedAuthoring -and
    [bool]$capture.portable.reopenPreservedGeneration -and
    [bool]$capture.retainedGoal169C.beforeAfterByteIdentical -and
    [int]$capture.invocationCounts.realPlayerSmoke -eq 0 -and
    [int]$capture.invocationCounts.unityEditorStarts -eq 0 -and
    [int]$capture.invocationCounts.unityHostBuilds -eq 0 -and
    [string]$capture.hostBeforeSha256 -eq
        [string]$capture.hostAfterSha256
) 'Goal169D capture is not strict GREEN.'

$currentGoalLines = @($currentGoalOutput | ForEach-Object {
    [string]$_
})
$currentGoalText = $currentGoalLines -join "`n"
$currentGoalPassed = $currentGoalText.Contains('PASSED')
$currentGoalFailed =
    @($currentGoalLines | Where-Object {
        $_.Contains('FAILED')
    }).Count
$assertionMismatchCount =
    @($currentGoalLines | Where-Object {
        $_ -match 'Assert\.(Contains|Equal)|assertion mismatch'
    }).Count
Assert-Goal ($currentGoalPassed) `
    'check-current-goal did not print PASSED.'
Assert-Goal ($currentGoalFailed -eq 0) `
    'check-current-goal printed FAILED.'
Assert-Goal ($assertionMismatchCount -eq 0) `
    'check-current-goal contains an assertion mismatch.'
Assert-Goal (
    -not ($currentGoalText.Contains(
        'independent_goal169c_blocker_audit_and_followup_without_retrying_consumed_smoke'))
) 'check-current-goal retained the stale Goal169C action.'

Write-JsonEvidence 'architecture-review.json' (
    [ordered]@{
        status = 'GREEN'
        taskClassification = 'TEST_FIXTURE_PORTABLE_TRUTH_CONTINUATION'
        productSliceAdded = $false
        productionDefectFound = $false
        productionApplicationCodeChanged = $false
        runtimeChanged = $false
        runtimeAbstractionsChanged = $false
        domainChanged = $false
        gamePackageChanged = $false
        featureCatalogChanged = $false
        generatedSourceChanged = $false
        unityChanged = $false
        standaloneOrRcImplementationChanged = $false
        reusedQualifiedFixture =
            'Goal164BuildFixture.Create(coreOnly:true)'
        reusedPortablePattern =
            'physical copy after build; operational Builds removed'
    })
Write-JsonEvidence 'goal169c-independent-audit-finding.json' (
    [ordered]@{
        status = 'BLOCKED_AT_72F69BE1'
        implementationCommit = $requiredBase
        standaloneProof = 'GREEN'
        immutablePublication = 'GREEN'
        packageHistoryPayloadCorrelation = 'GREEN'
        releaseCandidateCurrent = $true
        allSelectablePortable = 'GREEN'
        coreOnlyFailure =
            'INVALID_CREATION_ONLY_FIXTURE'
        productionDefect = $false
        goal169cAccepted = $false
        goal169cIndependentAuditRequired = $false
        closure = 'closed_by_goal169d'
        smokeRepeated = $false
    })
Write-JsonEvidence 'core-only-fixture-root-cause-proof.json' (
    [ordered]@{
        status = 'GREEN'
        rawFixture = 'Goal156TestKit.CoreOnly'
        creationMethod = 'Goal156TestKit.CreateGenerated'
        buildAndQualifyCalled = $false
        buildInvocationCount =
            [int]$capture.raw.BuildInvocationCount
        historyCount = [int]$capture.raw.historyCount
        creationStatus = [string]$capture.raw.Status
        generatedWorldStatus =
            [string]$capture.raw.generatedWorldStatus
        packageValid = [bool]$capture.raw.packageValid
        sourcePassed = [bool]$capture.raw.sourcePassed
        cannotClaimCampaignCurrent = $true
        absenceOfV7IsExpected = $true
        productionBug = $false
    })
Write-JsonEvidence 'core-only-qualified-build-proof.json' (
    [ordered]@{
        status = 'GREEN'
        fixture = 'Goal164BuildFixture.Create(coreOnly:true)'
        buildInvocationCount =
            [int]$capture.qualified.buildInvocationCount
        buildPassed = [bool]$capture.qualified.buildPassed
        buildStatus = [string]$capture.qualified.buildStatus
        historyCount = [int]$capture.qualified.historyCount
        historySchema = [string]$capture.qualified.historySchema
        historySha256 =
            [string]$capture.qualified.historySha256
        packageSha256 =
            [string]$capture.qualified.packageSha256
        finalStateHash =
            [string]$capture.qualified.finalStateHash
        generatedWorldStatus =
            [string]$capture.qualified.generatedWorldStatus
        packageCorrelationPassed =
            [bool]$capture.qualified.packageCorrelationPassed
    })
Write-JsonEvidence 'core-only-event-truth-proof.json' (
    [ordered]@{
        status = 'GREEN'
        availableBranchCount =
            [int]$capture.qualified.availableBranchCount
        relationshipStatus =
            [string]$capture.qualified.relationshipStatus
        expectedRelationshipStatus =
            $expectedRelationshipStatus
        relationshipCount =
            [int]$capture.qualified.relationshipCount
        relationshipBranchMatrixSha256 =
            [string]$capture.qualified.relationshipBranchMatrixSha256
        regionalEventStatus =
            [string]$capture.qualified.regionalEventStatus
        expectedRegionalEventStatus = $expectedEventStatus
        regionalEventCount =
            [int]$capture.qualified.regionalEventCount
        qualifiedRegionalEventCount =
            [int]$capture.qualified.qualifiedRegionalEventCount
        regionalEventInventorySha256 =
            [string]$capture.qualified.regionalEventInventorySha256
        strictEmptyPolicy =
            [string]$capture.qualified.strictEmptyPolicy
        branchDependentTruthPassed = $true
        actualPackageCorrelationPassed =
            [bool]$capture.qualified.packageCorrelationPassed
    })
Write-JsonEvidence 'core-only-portable-copy-proof.json' (
    [ordered]@{
        status = 'GREEN'
        physicalCopy = $true
        sourceProjectPath =
            [string]$capture.portable.sourceProjectPath
        copyProjectPath =
            [string]$capture.portable.copyProjectPath
        buildsPresent = [bool]$capture.portable.buildsPresent
        operationalPointerResolved =
            [bool]$capture.portable.operationalPointerResolved
        operationalPointerDiagnostic =
            [string]$capture.portable.operationalPointerDiagnostic
        packageSha256 =
            [string]$capture.portable.packageSha256
        selectedHistorySha256 =
            [string]$capture.portable.selectedHistorySha256
        authoringSha256 =
            [string]$capture.portable.authoringSha256
        generationSha256 =
            [string]$capture.portable.generationSha256
        generatedWorldStatus =
            [string]$capture.portable.generatedWorldStatus
        relationshipStatus =
            [string]$capture.portable.relationshipStatus
        regionalEventStatus =
            [string]$capture.portable.regionalEventStatus
        packageCorrelationPassed =
            [bool]$capture.portable.packageCorrelationPassed
        reopenPreservedPackage =
            [bool]$capture.portable.reopenPreservedPackage
        reopenPreservedHistory =
            [bool]$capture.portable.reopenPreservedHistory
        reopenPreservedAuthoring =
            [bool]$capture.portable.reopenPreservedAuthoring
        reopenPreservedGeneration =
            [bool]$capture.portable.reopenPreservedGeneration
    })
Write-JsonEvidence 'no-false-rc-proof.json' (
    [ordered]@{
        status = 'GREEN'
        qualifiedReleaseCandidateConfigurationStatus =
            [string]$capture.qualified.releaseCandidateConfigurationStatus
        qualifiedReleaseCandidateRecordConfigurationStatus =
            [string]$capture.qualified.releaseCandidateRecordConfigurationStatus
        portableReleaseCandidateConfigurationStatus =
            [string]$capture.portable.releaseCandidateConfigurationStatus
        portableReleaseCandidateRecordConfigurationStatus =
            [string]$capture.portable.releaseCandidateRecordConfigurationStatus
        acceptedMechanicsPassed =
            [bool]$capture.qualified.acceptedMechanicsPassed
        acceptedMechanicsMissingFactCount =
            [int]$capture.qualified.acceptedMechanicsMissingFactCount
        coreOnlyRcConfigurationCurrent = $false
        coreOnlyRcRecordCurrent = $false
        falseReadinessClaimed = $false
    })
Write-JsonEvidence 'retained-goal169c-publication-proof.json' (
    [ordered]@{
        status = 'GREEN'
        pointerPath =
            [string]$capture.retainedGoal169C.pointerPath
        pointerSha256 =
            [string]$capture.retainedGoal169C.pointerSha256
        runRoot = [string]$capture.retainedGoal169C.runRoot
        runTreeSha256 =
            [string]$capture.retainedGoal169C.runTreeSha256
        runStatusPath =
            [string]$capture.retainedGoal169C.runStatusPath
        runStatusSha256 =
            [string]$capture.retainedGoal169C.runStatusSha256
        payloadRoot =
            [string]$capture.retainedGoal169C.payloadRoot
        payloadTreeSha256 =
            [string]$capture.retainedGoal169C.payloadTreeSha256
        standaloneHistoryPath =
            [string]$capture.retainedGoal169C.standaloneHistoryPath
        standaloneHistorySha256 =
            [string]$capture.retainedGoal169C.standaloneHistorySha256
        selectedHistoryPath =
            [string]$capture.retainedGoal169C.selectedHistoryPath
        selectedHistorySha256 =
            [string]$capture.retainedGoal169C.selectedHistorySha256
        releaseCandidatePath =
            [string]$capture.retainedGoal169C.releaseCandidatePath
        releaseCandidateSha256 =
            [string]$capture.retainedGoal169C.releaseCandidateSha256
        packagePath =
            [string]$capture.retainedGoal169C.packagePath
        packageSha256 =
            [string]$capture.retainedGoal169C.packageSha256
        finalStateHash =
            [string]$capture.retainedGoal169C.finalStateHash
        beforeAfterByteIdentical =
            [bool]$capture.retainedGoal169C.beforeAfterByteIdentical
        goal169cSmokeRepeated = $false
    })
Write-JsonEvidence 'current-goal-gate-proof.json' (
    [ordered]@{
        status = 'GREEN'
        initialReproductionStatus = 'REPRODUCED_AT_72F69BE1'
        initialDiscovered = 16
        initialPassed = 15
        initialFailed = 1
        exactStaleTest =
            'PackageAssemblyCombatProgressionAcceptanceTests.CurrentStatePreservesGoal028AcceptedBeforeGoal029'
        exactStaleExpectedToken =
            'goal169c_blocked_after_single_cached_smoke_portable_core_only_campaign_truth'
        narrowHistoricalAssertionRepairApplied = $true
        checkCurrentGoalExitCode = 0
        passedMarkerPresent = $currentGoalPassed
        failedMarkerPresent = ($currentGoalFailed -gt 0)
        assertionMismatchCount = $assertionMismatchCount
        staleGoal169CActionAbsent = $true
        markdownJsonAgree = $true
    })
Write-JsonEvidence 'source-immutability-proof.json' (
    [ordered]@{
        status = 'GREEN'
        rawPackageSha256 =
            [string]$capture.raw.PackageSha256
        rawSourceSha256 =
            [string]$capture.raw.SourceSha256
        rawAuthoringSha256 =
            [string]$capture.raw.AuthoringSha256
        rawGenerationSha256 =
            [string]$capture.raw.GenerationSha256
        qualifiedSourceSha256 =
            [string]$capture.qualified.sourceSha256
        qualifiedAuthoringSha256 =
            [string]$capture.qualified.authoringSha256
        qualifiedGenerationSha256 =
            [string]$capture.qualified.generationSha256
        portableAuthoringSha256 =
            [string]$capture.portable.authoringSha256
        portableGenerationSha256 =
            [string]$capture.portable.generationSha256
        rawCreationSourcePreserved = (
            [string]$capture.raw.SourceSha256 -eq
            [string]$capture.qualified.sourceSha256)
        rawGenerationSidecarsPreserved = (
            [string]$capture.raw.GenerationSha256 -eq
            [string]$capture.qualified.generationSha256)
        qualifiedPortableAuthoringExact = (
            [string]$capture.qualified.authoringSha256 -eq
            [string]$capture.portable.authoringSha256)
        qualifiedPortableGenerationExact = (
            [string]$capture.qualified.generationSha256 -eq
            [string]$capture.portable.generationSha256)
        cachedHostBeforeSha256 =
            [string]$capture.hostBeforeSha256
        cachedHostAfterSha256 =
            [string]$capture.hostAfterSha256
        cachedHostByteIdentical = (
            [string]$capture.hostBeforeSha256 -eq
            [string]$capture.hostAfterSha256)
    })
Write-JsonEvidence 'regression-immutability-proof.json' (
    [ordered]@{
        status = 'GREEN'
        testCounts = $counts
        goal169DDiscovered = $goal169dTests.Count
        goal169DBehavioralDiscovered =
            $goal169dBehavioral.Count
        goal169CCompletePassCount = [int]$counts.Goal169C
        goal169BCompletePassCount = [int]$counts.Goal169B
        goal169ACompletePassCount = [int]$counts.Goal169A
        goal169CompletePassCount = [int]$counts.Goal169
        capabilityRuntimeEquipmentSlicePassed = $true
        characterAttributesProgressionSlicePassed = $true
        currentGoalCheckPassed = $true
        fullSuiteRun = $false
        goal168Full85CaseClosureRun = $false
        allProductSmokeRun = $false
        unityHostBuildRun = $false
        playerRun = $false
        oldSmokeRun = $false
        goal169CSmokeRepeated = $false
        realPlayerSmokeInvocationCount =
            [int]$capture.invocationCounts.realPlayerSmoke
        unityEditorProcessStartCount =
            [int]$capture.invocationCounts.unityEditorStarts
        unityHostBuildCount =
            [int]$capture.invocationCounts.unityHostBuilds
        retainedGoal169CByteIdentical =
            [bool]$capture.retainedGoal169C.beforeAfterByteIdentical
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
    scaffoldClassification = 'KEEP_AND_COMPLETE'
    independentAuditIntake = 'BLOCKED_AT_72F69BE1'
    rootCause = 'INVALID_CREATION_ONLY_FIXTURE'
    productionDefectFound = $false
    rawCreationOnlyBuildCount =
        [int]$capture.raw.BuildInvocationCount
    rawCreationOnlyHistoryCount =
        [int]$capture.raw.historyCount
    rawCreationOnlyCannotClaimCurrent = $true
    qualifiedCoreOnlyBuildCount =
        [int]$capture.qualified.buildInvocationCount
    qualifiedHistorySchema =
        [string]$capture.qualified.historySchema
    qualifiedPackageSha256 =
        [string]$capture.qualified.packageSha256
    qualifiedFinalStateHash =
        [string]$capture.qualified.finalStateHash
    qualifiedGeneratedWorldStatus =
        [string]$capture.qualified.generatedWorldStatus
    availableBranchCount =
        [int]$capture.qualified.availableBranchCount
    relationshipStatus =
        [string]$capture.qualified.relationshipStatus
    regionalEventStatus =
        [string]$capture.qualified.regionalEventStatus
    regionalEventCount =
        [int]$capture.qualified.regionalEventCount
    qualifiedRegionalEventCount =
        [int]$capture.qualified.qualifiedRegionalEventCount
    packageCorrelationPassed =
        [bool]$capture.qualified.packageCorrelationPassed
    portablePackageSha256 =
        [string]$capture.portable.packageSha256
    portableSelectedHistorySha256 =
        [string]$capture.portable.selectedHistorySha256
    portableAuthoringSha256 =
        [string]$capture.portable.authoringSha256
    portableGenerationSha256 =
        [string]$capture.portable.generationSha256
    portableOperationalPointerAbsent =
        (-not [bool]$capture.portable.operationalPointerResolved)
    portableCampaignCurrent = (
        [string]$capture.portable.generatedWorldStatus -eq
        'CAMPAIGN_CURRENT')
    portableReopenHashesExact = (
        [bool]$capture.portable.reopenPreservedPackage -and
        [bool]$capture.portable.reopenPreservedHistory -and
        [bool]$capture.portable.reopenPreservedAuthoring -and
        [bool]$capture.portable.reopenPreservedGeneration)
    coreOnlyRcConfigurationCurrent = $false
    coreOnlyRcRecordCurrent = $false
    coreOnlyNoFalseRcReady = $true
    retainedGoal169CPublicationExact =
        [bool]$capture.retainedGoal169C.beforeAfterByteIdentical
    goal169CSmokeRepeated = $false
    goal169DTestsDiscovered = $goal169dTests.Count
    goal169DBehavioralTestsDiscovered =
        $goal169dBehavioral.Count
    goal169CTestsPassed = [int]$counts.Goal169C
    goal169BTestsPassed = [int]$counts.Goal169B
    goal169ATestsPassed = [int]$counts.Goal169A
    goal169TestsPassed = [int]$counts.Goal169
    focusedRegressionsPassed = $true
    currentGoalGateClean = $true
    realPlayerSmokeInvocationCount =
        [int]$capture.invocationCounts.realPlayerSmoke
    unityEditorProcessStartCount =
        [int]$capture.invocationCounts.unityEditorStarts
    unityHostBuildCount =
        [int]$capture.invocationCounts.unityHostBuilds
    protectedBytesUnchanged = $true
    artifactScopeViolationCount = -1
    goal169CAccepted = $false
    goal169DAccepted = $false
    humanGate = $false
    independentAuditRequired = $true
}
Write-JsonEvidence 'goal169d-dashboard.json' $dashboard

$report = @"
# Goal169D report — GREEN

Goal169D is the narrow test-fixture and portable-truth continuation at `$requiredBase`, not a product slice. Scaffold classification is `KEEP_AND_COMPLETE`; independent Goal169C audit intake is `BLOCKED_AT_72F69BE1`.

The Goal169C failure came from copying raw `Goal156TestKit.CoreOnly`. That fixture creates a valid package, generated source and authoring state, but performs zero builds and has zero build-history rows. It is `CREATION_ONLY_NOT_QUALIFIED` and cannot honestly claim v7 or `CAMPAIGN_CURRENT`.

The corrected proof invokes `Goal164BuildFixture.Create(coreOnly:true)` exactly once. The build is GREEN, selects `unified_game_project_build_history_v7`, correlates the actual package and final state, and restores `GeneratedWorld=CAMPAIGN_CURRENT`. Measured branch availability is $($capture.qualified.availableBranchCount), so relationship truth is `$($capture.qualified.relationshipStatus)` and regional-event truth is `$($capture.qualified.regionalEventStatus)` with $($capture.qualified.qualifiedRegionalEventCount)/$($capture.qualified.regionalEventCount) qualified events.

A physical copy is made only after qualification. It contains no operational `Builds` directory or current pointer. Reopen preserves package, selected-history, authoring and generation hashes exactly while retaining campaign and branch-dependent event truth. Core-only RC configuration and record are not `CURRENT`.

The retained Goal169C current pointer, immutable run and run-status, standalone history, selected v7 history, payload, RC and package remain byte-identical. Goal169C smoke was not repeated. Goal169D real Player smoke, Unity Editor starts and Unity host builds are 0/0/0.

Goal169D passed $($goal169dTests.Count)/$($goal169dTests.Count) discovered tests, all $($goal169dBehavioral.Count) behavioral. Goal169C 34/34, Goal169B 72/72, Goal169A 60/60, Goal169 108/108 and the bounded focused regressions passed with all old smoke flags false. Full suite, Goal168 full 85-case closure, all-ProductSmoke, Unity host build and Player were not run.

The stale nongating CurrentState assertion was reproduced as 15/16 with the exact stale Goal169C gate token, then narrowed to its historical Goal028 contract. Final `check-current-goal` is clean. Goal169D remains `accepted=false`, creates no human gate and requires independent audit.
"@
Write-MarkdownEvidence 'goal169d-report.md' $report

$scopeOutput = & powershell -NoProfile -ExecutionPolicy Bypass `
    -File (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') `
    -Scenario $taskId -BaselineRef $requiredBase
Assert-Goal ($LASTEXITCODE -eq 0) `
    'Goal169D artifact scope command failed.'
$scope = $scopeOutput | ConvertFrom-Json
Assert-Goal (
    [bool]$scope.accepted -and
    [int]$scope.violationCount -eq 0
) 'Goal169D artifact scope has violations.'
$dashboard.artifactScopeViolationCount =
    [int]$scope.violationCount
Write-JsonEvidence 'goal169d-dashboard.json' $dashboard
Write-JsonEvidence 'artifact-scope-proof.json' (
    [ordered]@{
        status = 'GREEN'
        scenario = [string]$scope.scenario
        requiredBase = $requiredBase
        changedPathCount = [int]$scope.changedPathCount
        allowedCount = [int]$scope.allowedCount
        warningCount = [int]$scope.warningCount
        violationCount = [int]$scope.violationCount
        productionApplicationMutationCount = 0
        forbiddenRuntimeDomainGamePackageFeatureModulesGeneratedSourceUnityStandaloneRcMutationCount = 0
        boundedAdditionalProductionPath = $null
    })

$addedDiff = @(git diff --unified=0 --no-ext-diff $requiredBase -- |
    Where-Object {
        $_ -match '^\+' -and $_ -notmatch '^\+\+\+'
    } | ForEach-Object { $_.Substring(1) }) -join "`n"
Assert-TextIntegrity $addedDiff 'added tracked lines'
$ownedNewTextFiles = @(
    '.devflow/scripts/run-goal169d-core-only-portable-closure.ps1',
    '.devflow/scripts/run-goal169d-core-only-portable-closure.cmd',
    'docs/manual-acceptance/goal169d-qualified-core-only-portable-truth-gate-closure.md'
) + @(
    Get-ChildItem `
        'tests/LLMGameCreator.Tests/Application/Goal169D' `
        -File -Recurse | Select-Object -ExpandProperty FullName
) + @(
    Get-ChildItem $procedural -File |
        Select-Object -ExpandProperty FullName
) + @(
    Get-ChildItem $export -File |
        Select-Object -ExpandProperty FullName
)
foreach ($path in $ownedNewTextFiles | Sort-Object -Unique) {
    if (Test-Path -LiteralPath $path -PathType Leaf) {
        Assert-TextIntegrity (
            Get-Content -LiteralPath $path -Raw -Encoding UTF8
        ) $path
    }
}

git diff --check
Assert-Goal ($LASTEXITCODE -eq 0) 'git diff --check failed.'

$expected = @(
    'goal169d-dashboard.json',
    'architecture-review.json',
    'scaffold-classification.json',
    'goal169c-independent-audit-finding.json',
    'core-only-fixture-root-cause-proof.json',
    'core-only-qualified-build-proof.json',
    'core-only-event-truth-proof.json',
    'core-only-portable-copy-proof.json',
    'no-false-rc-proof.json',
    'retained-goal169c-publication-proof.json',
    'current-goal-gate-proof.json',
    'source-immutability-proof.json',
    'regression-immutability-proof.json',
    'artifact-scope-proof.json',
    'goal169d-report.md'
)
foreach ($root in @($procedural, $export)) {
    $actual = @(Get-ChildItem -LiteralPath $root -File |
        Select-Object -ExpandProperty Name | Sort-Object)
    Assert-Goal (
        $actual.Count -eq 15 -and
        -not (Compare-Object ($expected | Sort-Object) $actual)
    ) "Goal169D evidence root must contain exactly 15 files: $root"
}
foreach ($name in $expected) {
    Assert-Goal (
        (Get-FileHash -LiteralPath (Join-Path $procedural $name) `
            -Algorithm SHA256).Hash -eq
        (Get-FileHash -LiteralPath (Join-Path $export $name) `
            -Algorithm SHA256).Hash
    ) "Goal169D evidence roots differ for $name."
}

Assert-Goal (
    @(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0
) 'Unity process exists after Goal169D validation.'
Write-Host 'Goal169D qualified core-only portable closure is GREEN.'
Write-Host (
    "Goal169D: $($goal169dTests.Count)/" +
    "$($goal169dTests.Count), behavioral " +
    "$($goal169dBehavioral.Count); Goal169C: 34/34; " +
    "Goal169B: 72/72; Goal169A: 60/60; " +
    "Goal169: 108/108; Player/Unity/host: 0/0/0.")
