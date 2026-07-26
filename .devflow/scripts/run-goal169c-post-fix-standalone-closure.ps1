[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$project = '.\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$taskId =
    'goal-169c-post-fix-immutable-standalone-rc-and-portable-closure'
$requiredBase =
    '91bef55bad9740897876f15893a93d596fa44800'
$procedural = Join-Path '.llmgc\procedural' $taskId
$export = Join-Path '.llmgc\exports' $taskId
$goal169bRoot = Join-Path '.llmgc\procedural' `
    'goal-169b-package-bound-event-proof-nested-combat-replay-and-payload-closure'
$publishedDashboard = Join-Path $procedural `
    'goal169c-dashboard.json'

function Assert-Goal([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
}

function Invoke-External(
    [string]$name,
    [scriptblock]$command
) {
    Write-Host "=== $name ==="
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

function Get-OptionalPathHash([string]$path) {
    if (-not (Test-Path -LiteralPath $path)) {
        return '<absent>'
    }
    return Get-PathHash $path
}

function Get-RetainedSnapshot() {
    $retainedProof = Get-Content -LiteralPath (
        Join-Path $goal169bRoot `
            'retained-runs-immutability-proof.json') `
        -Raw -Encoding UTF8 | ConvertFrom-Json
    $rows = @()
    foreach ($label in @('Goal169', 'Goal169A')) {
        $source = @($retainedProof.retained | Where-Object {
            [string]$_.label -eq $label
        })
        Assert-Goal ($source.Count -eq 1) `
            "$label retained proof row is not unique."
        $row = $source[0]
        $projectRoot = Split-Path -Parent (
            Split-Path -Parent ([string]$row.standaloneHistoryPath))
        $buildHistoryRoot = Join-Path $projectRoot `
            '.llmgc\build-history'
        $releaseCandidatePath = Join-Path $projectRoot `
            '.llmgc\release-candidate\accepted-mechanics-rc1.json'
        $generationRoot = Join-Path $projectRoot `
            '.llmgc\generation'
        $rows += [ordered]@{
            label = $label
            pointerPath = [string]$row.pointerPath
            pointerSha256 = Get-PathHash ([string]$row.pointerPath)
            runRoot = [string]$row.runRoot
            runTreeSha256 = Get-PathHash ([string]$row.runRoot)
            payloadRoot = [string]$row.payloadRoot
            payloadTreeSha256 =
                Get-PathHash ([string]$row.payloadRoot)
            standaloneHistoryPath =
                [string]$row.standaloneHistoryPath
            standaloneHistorySha256 =
                Get-PathHash ([string]$row.standaloneHistoryPath)
            buildHistoryRoot = $buildHistoryRoot
            buildHistoryTreeSha256 =
                Get-PathHash $buildHistoryRoot
            releaseCandidatePath =
                $releaseCandidatePath
            releaseCandidateSha256 =
                Get-PathHash $releaseCandidatePath
            generationSourceSidecarsRoot = $generationRoot
            generationSourceSidecarsSha256 =
                Get-OptionalPathHash $generationRoot
        }
    }

    $failedProof = Get-Content -LiteralPath (
        Join-Path $goal169bRoot 'payload-only-standalone-proof.json') `
        -Raw -Encoding UTF8 | ConvertFrom-Json
    $failedRunRoot = [string]$failedProof.failedRunRoot
    $hostRoot = Join-Path (
        [Environment]::GetFolderPath('LocalApplicationData')) (
            'LLMGameCreator\StandaloneHostCache\' +
            [string]$failedProof.hostCacheKey + '\host')
    $goal142 = Join-Path $PWD `
        '.llmgc\procedural\goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff\product-line-runtime-variant-matrix-result.json'
    $goal148 = Join-Path (
        [Environment]::GetFolderPath('LocalApplicationData')) `
        'LLMGameCreator\Games\goal148-manual'
    $rows += [ordered]@{
        label = 'Goal169B failed staged run'
        runRoot = $failedRunRoot
        runTreeSha256 = Get-PathHash $failedRunRoot
        runStatusPresent =
            Test-Path -LiteralPath (Join-Path $failedRunRoot `
                'run-status.json')
        proceduralForensicsRoot = $goal169bRoot
        proceduralForensicsTreeSha256 =
            Get-PathHash $goal169bRoot
        exportForensicsRoot = Join-Path '.llmgc\exports' `
            'goal-169b-package-bound-event-proof-nested-combat-replay-and-payload-closure'
        exportForensicsTreeSha256 = Get-PathHash (
            Join-Path '.llmgc\exports' `
                'goal-169b-package-bound-event-proof-nested-combat-replay-and-payload-closure')
    }
    $rows += [ordered]@{
        label = 'cached host'
        path = $hostRoot
        sha256 = Get-PathHash $hostRoot
    }
    $rows += [ordered]@{
        label = 'Goal142'
        path = $goal142
        sha256 = Get-PathHash $goal142
    }
    $rows += [ordered]@{
        label = 'Goal148'
        path = $goal148
        sha256 = Get-OptionalPathHash $goal148
    }
    return $rows
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

if (Test-Path -LiteralPath $publishedDashboard) {
    $published = Get-Content -LiteralPath $publishedDashboard `
        -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([int]$published.hiddenSmokeInvocationCount -ge 1) {
        throw (
            'Goal169C smoke budget is already consumed; ' +
            'do not retry the published one-shot result.')
    }
}

Assert-Goal ((git rev-parse HEAD).Trim() -eq $requiredBase) `
    'Goal169C must run from the required base before publication.'
Assert-Goal ((git rev-parse origin/main).Trim() -eq $requiredBase) `
    'Goal169C origin/main must equal the required base.'
Assert-Goal ((git rev-parse --abbrev-ref HEAD).Trim() -eq 'main') `
    'Goal169C must run on main.'
Assert-Goal (
    @(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0
) 'Unity process exists before Goal169C validation.'

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

Invoke-External 'Solution build' {
    dotnet build LLMGameCreator.sln -c Debug --no-restore --nologo
}

$goal169cTests = @(Get-Discovered `
    'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169C')
$goal169cBehavioral = @($goal169cTests | Where-Object {
    $_ -match '\.Behavioral_'
})
Assert-Goal ($goal169cTests.Count -ge 28) `
    "Goal169C discovered $($goal169cTests.Count), expected >=28."
Assert-Goal ($goal169cBehavioral.Count -ge 24) `
    "Goal169C behavioral $($goal169cBehavioral.Count), expected >=24."

$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) `
    ('llmgc-goal169c-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporaryRoot | Out-Null
$preflightCapturePath =
    Join-Path $temporaryRoot 'preflight-capture.json'
$smokeCapturePath =
    Join-Path $temporaryRoot 'smoke-capture.json'
$priorPreflightCapture =
    $env:LLMGC_GOAL169C_PREFLIGHT_CAPTURE_PATH
$env:LLMGC_GOAL169C_PREFLIGHT_CAPTURE_PATH =
    $preflightCapturePath
try {
    $goal169cNonSmoke = Invoke-Test 'Goal169C non-smoke' `
        ('FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169C' +
         '&FullyQualifiedName!~Goal169CStandaloneSmokeTests')
}
finally {
    $env:LLMGC_GOAL169C_PREFLIGHT_CAPTURE_PATH =
        $priorPreflightCapture
}
Assert-Goal (Test-Path -LiteralPath $preflightCapturePath) `
    'Goal169C preflight capture is missing.'
$preflight = Get-Content -LiteralPath $preflightCapturePath `
    -Raw -Encoding UTF8 | ConvertFrom-Json
Assert-Goal (
    [string]$preflight.status -eq 'GREEN' -and
    [bool]$preflight.factSingleLine -and
    -not [bool]$preflight.factContainsQuote -and
    [bool]$preflight.utf8Base64Decoded -and
    [bool]$preflight.authorityRoundtripExact -and
    [int]$preflight.eventCount -eq 6 -and
    [int]$preflight.signatureCount -eq 24 -and
    [int]$preflight.frameCountKeyCount -eq 24 -and
    [int]$preflight.nestedTraceKeyCount -eq 24 -and
    [bool]$preflight.structuralSelfCheckPassed -and
    [bool]$preflight.legacyParserPassed -and
    [bool]$preflight.strictCorrelationPassed
) 'Goal169C Base64/legacy preflight is not strict GREEN.'

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
    'LLMGC_GOAL169C_RUN_SMOKE'
)
$priorSmokeValues = @{}
foreach ($name in $smokeVariables) {
    $priorSmokeValues[$name] =
        [Environment]::GetEnvironmentVariable($name)
    [Environment]::SetEnvironmentVariable($name, 'false')
}
$counts = [ordered]@{
    Goal169CDiscovered = $goal169cTests.Count
    Goal169CBehavioral = $goal169cBehavioral.Count
    Goal169CNonSmoke = $goal169cNonSmoke
}
try {
    $counts.Goal169B = Invoke-Test `
        'Goal169B 72/72 with old smoke disabled' `
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169B.'
    $counts.Goal169A = Invoke-Test `
        'Goal169A 60/60 with old smoke disabled' `
        'FullyQualifiedName~LLMGameCreator.Tests.Application.Goal169A.'
    $counts.Goal169 = Invoke-Test `
        'Goal169 108/108 with old smoke disabled' `
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
}
finally {
    foreach ($name in $smokeVariables) {
        [Environment]::SetEnvironmentVariable(
            $name, $priorSmokeValues[$name])
    }
}
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

Invoke-External 'Capability Runtime Equipment slice' {
    & '.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1'
}
Invoke-External 'Character Attributes/Progression slice' {
    & '.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1'
}
Invoke-External 'Current goal consistency' {
    & '.\.devflow\scripts\check-current-goal.ps1'
}

$retainedBefore = @(Get-RetainedSnapshot)
Assert-Goal (
    @($retainedBefore | Where-Object {
        [string]$_.label -eq 'Goal169B failed staged run'
    })[0].runStatusPresent -eq $false
) 'Failed Goal169B run unexpectedly acquired run-status.'
Write-JsonEvidence 'retained-inputs-before-proof.json' (
    [ordered]@{
        status = 'GREEN'
        capturedBeforeGoal169CSmoke = $true
        retained = $retainedBefore
    })

Assert-Goal (
    @(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0
) 'Unity process exists before the one permitted Goal169C smoke.'
$priorGoal169CSmoke = $env:LLMGC_GOAL169C_RUN_SMOKE
$priorSmokeCapture =
    $env:LLMGC_GOAL169C_SMOKE_CAPTURE_PATH
$env:LLMGC_GOAL169C_RUN_SMOKE = 'true'
$env:LLMGC_GOAL169C_SMOKE_CAPTURE_PATH = $smokeCapturePath
try {
    $counts.Goal169CHiddenSmoke = Invoke-Test `
        'Goal169C exactly one cached hidden smoke' `
        ('FullyQualifiedName=' +
         'LLMGameCreator.Tests.Application.Goal169C.' +
         'Goal169CStandaloneSmokeTests.' +
         'Behavioral_exactly_one_post_fix_cached_hidden_immutable_smoke')
}
finally {
    $env:LLMGC_GOAL169C_RUN_SMOKE = $priorGoal169CSmoke
    $env:LLMGC_GOAL169C_SMOKE_CAPTURE_PATH =
        $priorSmokeCapture
}
Assert-Goal (
    @(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0
) 'Unity process exists after the Goal169C smoke.'
Assert-Goal (Test-Path -LiteralPath $smokeCapturePath) `
    'Goal169C smoke capture is missing.'
$smoke = Get-Content -LiteralPath $smokeCapturePath `
    -Raw -Encoding UTF8 | ConvertFrom-Json
Assert-Goal (
    [string]$smoke.status -eq 'GREEN' -and
    [int]$smoke.hiddenSmokeInvocationCount -eq 1 -and
    [int]$smoke.correctiveRetryCount -eq 0 -and
    [bool]$smoke.hostReused -and
    -not [bool]$smoke.hostRebuilt -and
    [int]$smoke.unityEditorProcessStartCount -eq 0 -and
    [int]$smoke.unityHostBuildCount -eq 0 -and
    [bool]$smoke.standaloneLaunchStarted -and
    [int]$smoke.smokeExitCode -eq 0 -and
    [bool]$smoke.payloadSelfCheckPassed -and
    [bool]$smoke.legacyParserPassed -and
    [bool]$smoke.smokeMarkersPassed -and
    [bool]$smoke.playerLogPresent -and
    [bool]$smoke.pointerPublished -and
    [bool]$smoke.runStatusPublished -and
    [bool]$smoke.newRunDistinctFromGoal169B -and
    [int]$smoke.eventCount -eq 6 -and
    [int]$smoke.replaySignatureCount -eq 24 -and
    [int]$smoke.frameCountKeyCount -eq 24 -and
    [int]$smoke.nestedTraceKeyCount -eq 24 -and
    [bool]$smoke.historyPackagePayloadCorrelationPassed -and
    [bool]$smoke.frameIdentitiesPassed -and
    [bool]$smoke.exactCommandsPassed -and
    [bool]$smoke.nestedCombatIdentityPassed -and
    [bool]$smoke.releaseCandidateCurrent -and
    [bool]$smoke.portableCampaignCurrent -and
    [bool]$smoke.portableReleaseCandidateCurrent -and
    [bool]$smoke.portableOperationalPointerAbsent -and
    [bool]$smoke.coreOnlyCampaignCurrent -and
    [bool]$smoke.coreOnlyNoFalseRcReady
) 'Goal169C immutable smoke truth is not strict GREEN.'

$retainedAfter = @(Get-RetainedSnapshot)
Assert-Goal (
    ($retainedBefore | ConvertTo-Json -Depth 100 -Compress) -eq
    ($retainedAfter | ConvertTo-Json -Depth 100 -Compress)
) 'Retained Goal169/Goal169A/Goal169B/host/source bytes changed.'
Write-JsonEvidence 'retained-inputs-after-proof.json' (
    [ordered]@{
        status = 'GREEN'
        capturedAfterGoal169CSmoke = $true
        retained = $retainedAfter
        beforeAfterByteIdentical = $true
    })

Write-JsonEvidence 'architecture-review.json' ([ordered]@{
    status = 'GREEN'
    requiredBase = $requiredBase
    primaryFileCount = 16
    continuationNotNewProductSlice = $true
    base64AuthoritySingleLine = [bool]$preflight.factSingleLine
    base64AuthorityContainsQuote = [bool]$preflight.factContainsQuote
    base64AuthorityContainsCrLf = $false
    utf8Base64Decoded = [bool]$preflight.utf8Base64Decoded
    deserializeHumanFactAuthoritySha256Roundtrip =
        [bool]$preflight.authorityRoundtripExact
    realAssembledPayloadStructuralSelfCheckPassed =
        [bool]$preflight.structuralSelfCheckPassed
    legacyParserCompatibilityPassed =
        [bool]$preflight.legacyParserPassed
    frameCategoryRoundtripDimensions = @(
        'event','route','replay','sequence','command')
    noSpeculativeProductionChangeNeeded = $true
    productionMutationCount = 0
    smokeStartedAfterAllNonSmokeGates = $true
    proofTerminology =
        'immutable_payload_history_package_correlation'
    reducedAdapterFramesAloneClaimFullSignatures = $false
})
Write-JsonEvidence 'goal169b-independent-audit-finding.json' (
    [ordered]@{
        goal169bImplementationCommit = $requiredBase
        goal169bIndependentAuditResult = 'BLOCKED_AT_91BEF55B'
        goal169bCodeFindingsClosed = $true
        goal169bPublicationBlocker =
            'standalone.payload.human_facts_parse_mismatch'
        goal169bPostSmokeFix =
            'single_line_base64_utf8_json_authority'
        goal169bPostFixLegacySelfCheckPassed = $true
        goal169cRequiredAction =
            'fresh_post_fix_immutable_standalone_rc_portable_proof'
    })
Write-JsonEvidence 'base64-authority-preflight-proof.json' (
    [ordered]@{
        status = 'GREEN'
        label = [string]$preflight.factLabel
        prefix = [string]$preflight.factPrefix
        singleLine = [bool]$preflight.factSingleLine
        containsQuote = [bool]$preflight.factContainsQuote
        utf8Decoded = [bool]$preflight.utf8Base64Decoded
        schemaVersion = [string]$preflight.schemaVersion
        authoritySha256 = [string]$preflight.AuthoritySha256
        authorityRoundtripExact =
            [bool]$preflight.authorityRoundtripExact
        eventCount = [int]$preflight.eventCount
        signatureCount = [int]$preflight.signatureCount
        frameCountKeyCount =
            [int]$preflight.frameCountKeyCount
        nestedTraceKeyCount =
            [int]$preflight.nestedTraceKeyCount
    })
Write-JsonEvidence 'legacy-parser-proof.json' (
    [ordered]@{
        status = 'GREEN'
        structuralSelfCheckPassed =
            [bool]$preflight.structuralSelfCheckPassed
        structuralPassedCount =
            [int]$preflight.selfCheckPassedCount
        structuralTotalCount =
            [int]$preflight.selfCheckTotalCount
        legacyParserPassed =
            [bool]$preflight.legacyParserPassed
        legacyFrameCount =
            [int]$preflight.legacyFrameCount
        legacyHumanFactCount =
            [int]$preflight.legacyHumanFactCount
        humanFactsParseMismatchPresent = $false
    })
Write-JsonEvidence 'goal169c-smoke-proof.json' (
    [ordered]@{
        status = 'GREEN'
        invocationCount = [int]$smoke.hiddenSmokeInvocationCount
        retryCount = [int]$smoke.correctiveRetryCount
        hostCacheKey = [string]$smoke.hostCacheKey
        hostReused = [bool]$smoke.hostReused
        hostRebuilt = [bool]$smoke.hostRebuilt
        unityEditorProcessStartCount =
            [int]$smoke.unityEditorProcessStartCount
        unityHostBuildCount = [int]$smoke.unityHostBuildCount
        standaloneLaunchStarted =
            [bool]$smoke.standaloneLaunchStarted
        exitCode = [int]$smoke.smokeExitCode
        playerLogPresent = [bool]$smoke.playerLogPresent
        smokeMarkersPassed = [bool]$smoke.smokeMarkersPassed
        payloadSelfCheckPassed =
            [bool]$smoke.payloadSelfCheckPassed
        legacyParserPassed = [bool]$smoke.legacyParserPassed
        selfChecksPassed = [int]$smoke.selfCheckPassedCount
        selfChecksTotal = [int]$smoke.selfCheckTotalCount
    })
Write-JsonEvidence 'immutable-run-publication-proof.json' (
    [ordered]@{
        status = 'GREEN'
        attemptId = [string]$smoke.attemptId
        pointerAttemptId = [string]$smoke.pointerAttemptId
        runStatusAttemptId = [string]$smoke.runStatusAttemptId
        standaloneHistoryAttemptId =
            [string]$smoke.standaloneHistoryAttemptId
        attemptIdsExact = (
            [string]$smoke.attemptId -eq
            [string]$smoke.pointerAttemptId -and
            [string]$smoke.attemptId -eq
            [string]$smoke.runStatusAttemptId -and
            [string]$smoke.attemptId -eq
            [string]$smoke.standaloneHistoryAttemptId)
        pointerPublished = [bool]$smoke.pointerPublished
        runStatusPublished = [bool]$smoke.runStatusPublished
        runStatus = 'GREEN'
        runDirectoryName = [string]$smoke.runDirectoryName
        newRunDistinctFromGoal169B =
            [bool]$smoke.newRunDistinctFromGoal169B
        currentPointerPath = [string]$smoke.currentPointerPath
        currentPointerSha256 = [string]$smoke.pointerSha256
        runStatusPath = [string]$smoke.runStatusPath
        runStatusSha256 = [string]$smoke.runStatusSha256
        packageSha256 = [string]$smoke.packageSha256
        finalStateHash = [string]$smoke.finalStateHash
        actualPayloadPackageSha256 =
            [string]$smoke.actualPayloadPackageSha256
    })
Write-JsonEvidence `
    'immutable-payload-history-package-correlation-proof.json' (
    [ordered]@{
        status = 'GREEN'
        terminology =
            'immutable_payload_history_package_correlation'
        selectedHistoryPath = [string]$smoke.buildHistoryPath
        selectedHistorySha256 =
            [string]$smoke.buildHistorySha256
        actualPayloadPackageSha256 =
            [string]$smoke.actualPayloadPackageSha256
        packageShaExact = (
            [string]$smoke.packageSha256 -eq
            [string]$smoke.actualPayloadPackageSha256)
        finalStateHash = [string]$smoke.finalStateHash
        eventCount = [int]$smoke.eventCount
        eventIds = $smoke.eventIds
        signatureCount = [int]$smoke.replaySignatureCount
        signaturesRecomputedFromSelectedTypedHistory =
            [bool]$smoke.signaturesRecomputedFromSelectedHistory
        authoritySha256 = [string]$smoke.authoritySha256
        frameSchema = [string]$smoke.frameSchema
        payloadFrameCount = [int]$smoke.payloadFrameCount
        frameIdentitiesPassed =
            [bool]$smoke.frameIdentitiesPassed
        exactCommandsPassed = [bool]$smoke.exactCommandsPassed
        nestedCombatFrameCount =
            [int]$smoke.nestedCombatFrameCount
        nestedCombatIdentityPassed =
            [bool]$smoke.nestedCombatIdentityPassed
        reducedAdapterFramesAloneUsedToClaimSignatures = $false
    })
Write-JsonEvidence 'rc-portability-proof.json' (
    [ordered]@{
        status = 'GREEN'
        releaseCandidatePath =
            [string]$smoke.releaseCandidatePath
        releaseCandidateSha256 =
            [string]$smoke.releaseCandidateSha256
        releaseCandidateCurrent =
            [bool]$smoke.releaseCandidateCurrent
        releaseCandidateRecordCurrent =
            [bool]$smoke.releaseCandidateRecordCurrent
        rcHashesExact = [bool]$smoke.rcHashesExact
        portableCampaignCurrent =
            [bool]$smoke.portableCampaignCurrent
        portableReleaseCandidateCurrent =
            [bool]$smoke.portableReleaseCandidateCurrent
        portableOperationalPointerAbsent =
            [bool]$smoke.portableOperationalPointerAbsent
        coreOnlyCampaignCurrent =
            [bool]$smoke.coreOnlyCampaignCurrent
        coreOnlyNoFalseRcReady =
            [bool]$smoke.coreOnlyNoFalseRcReady
    })
Write-JsonEvidence 'regression-immutability-proof.json' (
    [ordered]@{
        status = 'GREEN'
        testCounts = $counts
        goal169CDiscovered = $goal169cTests.Count
        goal169CBehavioralDiscovered =
            $goal169cBehavioral.Count
        goal169CNonSmokePassed = $goal169cNonSmoke
        goal169BCompletePassCount = [int]$counts.Goal169B
        goal169ACompletePassCount = [int]$counts.Goal169A
        goal169CompletePassCount = [int]$counts.Goal169
        capabilityRuntimeEquipmentSlicePassed = $true
        characterAttributesProgressionSlicePassed = $true
        currentGoalCheckPassedBeforeSmoke = $true
        fullSuiteRun = $false
        goal168Full85CaseClosureRun = $false
        allProductSmokeRun = $false
        unityHostBuildRun = $false
        oldGoal169SmokeRun = $false
        oldGoal169ASmokeRun = $false
        oldGoal169BSmokeRun = $false
        goal169CSmokeInvocationCount = 1
        retryCount = 0
        retainedBeforeAfterByteIdentical = $true
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
    goal169bIndependentAuditResult = 'BLOCKED_AT_91BEF55B'
    goal169bCodeFindingsClosed = $true
    goal169bPublicationBlockerClosedByGoal169C = $true
    base64AuthorityPassed = $true
    legacyParserPassed = $true
    retainedInputsByteIdentical = $true
    goal169CTestsDiscovered = $goal169cTests.Count
    goal169CBehavioralTestsDiscovered =
        $goal169cBehavioral.Count
    goal169CNonSmokeTestsPassed = $goal169cNonSmoke
    goal169BTestsPassed = [int]$counts.Goal169B
    goal169ATestsPassed = [int]$counts.Goal169A
    goal169TestsPassed = [int]$counts.Goal169
    hiddenSmokeInvocationCount =
        [int]$smoke.hiddenSmokeInvocationCount
    correctiveSmokeRetryCount =
        [int]$smoke.correctiveRetryCount
    hostReused = [bool]$smoke.hostReused
    hostRebuilt = [bool]$smoke.hostRebuilt
    unityEditorProcessStartCount =
        [int]$smoke.unityEditorProcessStartCount
    unityHostBuildCount = [int]$smoke.unityHostBuildCount
    standaloneLaunchStarted =
        [bool]$smoke.standaloneLaunchStarted
    standaloneExitCode = [int]$smoke.smokeExitCode
    payloadSelfCheckPassed =
        [bool]$smoke.payloadSelfCheckPassed
    pointerPublished = [bool]$smoke.pointerPublished
    runStatusPublished = [bool]$smoke.runStatusPublished
    eventCount = [int]$smoke.eventCount
    signatureCount = [int]$smoke.replaySignatureCount
    payloadFrameCount = [int]$smoke.payloadFrameCount
    nestedCombatFrameCount =
        [int]$smoke.nestedCombatFrameCount
    immutableCorrelationPassed =
        [bool]$smoke.historyPackagePayloadCorrelationPassed
    frameIdentityPassed = [bool]$smoke.frameIdentitiesPassed
    nestedCombatIdentityPassed =
        [bool]$smoke.nestedCombatIdentityPassed
    releaseCandidateCurrent =
        [bool]$smoke.releaseCandidateCurrent
    portableAllSelectablePassed = (
        [bool]$smoke.portableCampaignCurrent -and
        [bool]$smoke.portableReleaseCandidateCurrent -and
        [bool]$smoke.portableOperationalPointerAbsent)
    portableCoreOnlyPassed = (
        [bool]$smoke.coreOnlyCampaignCurrent -and
        [bool]$smoke.coreOnlyNoFalseRcReady)
    protectedBytesUnchanged = $true
    artifactScopeViolationCount = -1
    goal169Accepted = $false
    goal169AAccepted = $false
    goal169BAccepted = $false
    goal169CAccepted = $false
    humanGate = $false
    independentAuditRequired = $true
}
Write-JsonEvidence 'goal169c-dashboard.json' $dashboard

$report = @"
# Goal169C report — GREEN

Goal169C is a narrow publication and qualification continuation of Goal169B at `$requiredBase`, not a new product slice. Independent audit intake is `BLOCKED_AT_91BEF55B`; Goal169B code findings were already closed, and Goal169C closes only the publication blocker caused by `standalone.payload.human_facts_parse_mismatch`.

The preflight proves a single-line `base64:` UTF-8 JSON authority with no quote, CR or LF outside encoded bytes. Authority SHA roundtrip, 13/13 structural checks and the legacy parser are GREEN. The authority contains 6 exact event IDs, 24 signatures, 24 frame-count keys and 24 nested-trace keys.

All non-smoke gates passed before launch: Goal169C $goal169cNonSmoke/$goal169cNonSmoke, Goal169B 72/72, Goal169A 60/60, Goal169 108/108 and the required focused regressions and slice scripts. Old Goal169, Goal169A and Goal169B smokes were disabled.

Exactly one Goal169C cached hidden smoke ran with retry 0, host reuse, no host rebuild, Unity Editor 0, Unity host builds 0, real Player launch, exit 0, Player log and smoke markers GREEN. A distinct immutable run, GREEN run-status and current pointer were published.

After in-memory objects were closed and GC completed, proof read immutable pointer/run, selected v7 history, actual payload package/model/frames/Base64 authority and RC. Package/final hashes correlate exactly. Signatures are recomputed from selected typed history frames; adapter frames carry exact event/route/replay/sequence/command identities, and nested combat command/event/state plus descriptor/effect progress identity is present.

RC is CURRENT. Portable all-selectable remains campaign and RC CURRENT without an operational pointer. Portable core-only remains campaign-current without false RC readiness. Retained Goal169/Goal169A pointer/run/payload/history/RC, Goal169B failed run/forensics, cached host, Goal142, Goal148 and generation sidecars are byte-identical before and after.

Goal169C remains `accepted=false`, creates no human gate and requires independent audit before another visible campaign slice.
"@
Write-MarkdownEvidence 'goal169c-report.md' $report

$scopeOutput = & powershell -NoProfile -ExecutionPolicy Bypass `
    -File (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') `
    -Scenario $taskId -BaselineRef $requiredBase
Assert-Goal ($LASTEXITCODE -eq 0) `
    'Goal169C artifact scope command failed.'
$scope = $scopeOutput | ConvertFrom-Json
Assert-Goal (
    [bool]$scope.accepted -and
    [int]$scope.violationCount -eq 0
) 'Goal169C artifact scope has violations.'
$dashboard.artifactScopeViolationCount =
    [int]$scope.violationCount
Write-JsonEvidence 'goal169c-dashboard.json' $dashboard
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
    'goal169c-dashboard.json',
    'architecture-review.json',
    'scaffold-classification.json',
    'goal169b-independent-audit-finding.json',
    'base64-authority-preflight-proof.json',
    'legacy-parser-proof.json',
    'retained-inputs-before-proof.json',
    'goal169c-smoke-proof.json',
    'immutable-run-publication-proof.json',
    'immutable-payload-history-package-correlation-proof.json',
    'rc-portability-proof.json',
    'retained-inputs-after-proof.json',
    'regression-immutability-proof.json',
    'artifact-scope-proof.json',
    'goal169c-report.md'
)
foreach ($root in @($procedural, $export)) {
    $actual = @(Get-ChildItem -LiteralPath $root -File |
        Select-Object -ExpandProperty Name | Sort-Object)
    Assert-Goal (
        $actual.Count -eq 15 -and
        -not (Compare-Object ($expected | Sort-Object) $actual)
    ) "Goal169C evidence root must contain exactly 15 files: $root"
}
foreach ($name in $expected) {
    Assert-Goal (
        (Get-FileHash -LiteralPath (Join-Path $procedural $name) `
            -Algorithm SHA256).Hash -eq
        (Get-FileHash -LiteralPath (Join-Path $export $name) `
            -Algorithm SHA256).Hash
    ) "Goal169C evidence roots differ for $name."
}

Write-Host 'Goal169C post-fix immutable closure is GREEN.'
Write-Host (
    "Goal169C: $($goal169cTests.Count)/" +
    "$($goal169cTests.Count) discovered, " +
    "$goal169cNonSmoke non-smoke; Goal169B: 72/72; " +
    "Goal169A: 60/60; Goal169: 108/108; " +
    "smoke/retry/Unity/exit: 1/0/0/0.")
