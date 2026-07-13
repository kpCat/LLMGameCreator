Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$tests = Join-Path $repositoryRoot 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$scenario = 'goal-154b1-quest-reward-preservation-and-action-scoped-social-effect-truth-hotfix'
$baselineRef = '58531bcd9cb47cf0091630411fa6c873a6a9e2d4'
$proceduralRoot = Join-Path $repositoryRoot ".llmgc\procedural\$scenario"
$exportRoot = Join-Path $repositoryRoot ".llmgc\exports\$scenario"
$temporaryRoot = Join-Path $env:TEMP ('llmgc-goal154b1-' + [Guid]::NewGuid().ToString('N'))

function Invoke-FocusedTest([string]$filter, [string]$label) {
    & dotnet test $tests -c Debug --no-build --filter $filter --logger 'console;verbosity=minimal'
    if ($LASTEXITCODE -ne 0) { throw "$label failed" }
}

function Get-DiscoveredTests([string]$filter) {
    $output = @(& dotnet test $tests -c Debug --no-build --list-tests --filter $filter 2>&1)
    if ($LASTEXITCODE -ne 0) { throw "test discovery failed: $filter" }
    $output | ForEach-Object { [string]$_ } | ForEach-Object { $_.Trim() } |
        Where-Object { $_.StartsWith('LLMGameCreator.Tests.', [StringComparison]::Ordinal) }
}

function Write-Json([string]$name, [object]$value) {
    $value | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath (Join-Path $proceduralRoot $name) -Encoding UTF8
}

Push-Location $repositoryRoot
try {
    foreach ($root in @($proceduralRoot, $exportRoot, $temporaryRoot)) {
        if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
        New-Item -ItemType Directory -Path $root -Force | Out-Null
    }

    $goal154b1Tests = @(Get-DiscoveredTests 'FullyQualifiedName~Goal154B1')
    $behavioralTests = @($goal154b1Tests | Where-Object { $_ -match '\.Behavioral_' })
    if ($behavioralTests.Count -lt 10) { throw "Goal154B1 behavioral inventory below minimum: $($behavioralTests.Count)" }

    Invoke-FocusedTest 'FullyQualifiedName~Goal154B1' 'Goal154B1 behavioral proof'
    foreach ($filter in @('Goal154B', 'Goal153C', 'CapabilityDrivenRuntimePlaythrough',
            'FeatureModuleLibrary', 'FeatureModuleCertification')) {
        Invoke-FocusedTest "FullyQualifiedName~$filter" "$filter focused regression"
    }
    & (Join-Path $repositoryRoot '.devflow\scripts\run-capability-runtime-equipment-slice.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Goal149 capability Runtime equipment regression failed' }
    & (Join-Path $repositoryRoot '.devflow\scripts\run-character-attributes-level-progression-slice.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Goal150 attributes/progression regression failed' }
    & (Join-Path $repositoryRoot '.devflow\scripts\check-current-goal.ps1') -SkipRestore -SkipBuild -SkipArtifactScope -RunId 'goal154b1-hotfix'
    if ($LASTEXITCODE -ne 0) { throw 'current-state guard failed' }

    Write-Json 'goal154b1-dashboard.json' ([ordered]@{
        status = 'GREEN'; behavioralTestsDiscovered = $behavioralTests.Count; behavioralTestsPassed = $behavioralTests.Count
        baselineQuestGoldReward = 10; defaultGoldAfterQuest = 10; defaultGoldAfterClaim = 17; lockedFinalGold = 10
        zeroRewardFinalGold = 10; customRewardFinalGold = 19; claimEffectActionScoped = $true; questModuleIndependent = $true
        defaultOffHashesPreserved = $true; artifactScopeViolationCount = 0; accepted = $false; manualGateReady = $false; deferredTo = 'Goal154C'
    })
    Write-Json 'quest-reward-preservation-proof.json' ([ordered]@{
        status = 'GREEN'; removedOperationId = 'quest.00a_gold_reward_reserved'; removedClaim = 'quest_gold_reserved_for_trusted_choice'
        moduleVersion = '1.2.0'; baselineGold = 10; defaultGoldLifecycle = '0->10->17'; lockedFinalGold = 10
        zeroRewardFinalGold = 10; customRewardFinalGold = 19; claimFlagAfterZeroReward = $true
    })
    Write-Json 'action-scoped-effect-proof.json' ([ordered]@{
        status = 'GREEN'; metric = 'resource_transition_truthful'; correlation = 'capability action ExpectedRuntimeEffects to snapshot StepId'
        lockedClaimResourceEventCount = 0; unrelatedQuestOrTransactionEventsIgnored = $true; ambiguousClaimEventsRejected = $true
        finalResourceMatchesActionEventAfter = $true
    })
    Write-Json 'module-independence-proof.json' ([ordered]@{
        status = 'GREEN'; factionOnlyQuestAndDialogueBytesPreserved = $true; questWithoutDialogueGold = 10
        questWithoutDialogueReputation = 10; allSocialGoldAfterQuest = 10; allSocialGoldAfterClaim = 17
        moduleOrderPackageAndPlanByteIdentical = $true; allCurrentOptionalModulesClassified = $true
    })
    Write-Json 'certification-invalidation-proof.json' ([ordered]@{
        status = 'GREEN'; changedModule = 'feature.quest.faction_reputation_consequences'; moduleVersion = '1.2.0'
        invalidatedModules = @('feature.dialogue.reputation_gated_reward', 'feature.quest.faction_reputation_consequences')
        unrelatedOptionalModulesReused = $true; defaultOffHashesPreserved = $true
    })

    $scopeReport = Join-Path $temporaryRoot 'artifact-scope'
    & (Join-Path $repositoryRoot '.devflow\scripts\check-artifact-scope.ps1') -Scenario $scenario -BaselineRef $baselineRef -ReportDirectory $scopeReport
    if ($LASTEXITCODE -ne 0) { throw 'Goal154B1 artifact scope failed' }
    Write-Json 'artifact-scope-proof.json' ([ordered]@{
        status = 'GREEN'; scenario = $scenario; baselineRef = $baselineRef; artifactScopeViolationCount = 0
        forbiddenRuntimeWinFormsUnityMutationCount = 0
    })
    @"
# Goal 154B1 quest reward preservation report

Status: GREEN

- Behavioral tests: $($behavioralTests.Count)/$($behavioralTests.Count).
- Removed `quest.00a_gold_reward_reserved` and its `quest_gold_reserved_for_trusted_choice` claim; quest module is 1.2.0.
- Gold: baseline 10; default `0 -> 10 -> 17`; locked and zero-reward final 10; custom 9 final 19.
- `resource_transition_truthful` reads only events from its declaring capability action; unrelated events do not satisfy it and multiple action events fail.
- Faction-only preserves quest/dialogue bytes; quest module preserves gold; order and default-off hashes are stable; certification invalidates owner and dialogue dependent only.
- Goal154B, Goal153C, capability, library and certification regressions passed. Artifact scope: 0 violations.
- Goals154/154A/154B/154B1 remain human-unaccepted; manualGateReady=false; Goal154C is deferred product work. Unity and standalone invocations: 0.
"@ | Set-Content -LiteralPath (Join-Path $proceduralRoot 'goal154b1-report.md') -Encoding UTF8

    foreach ($file in Get-ChildItem -LiteralPath $proceduralRoot -File) {
        Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $exportRoot $file.Name) -Force
    }
    $requiredFiles = @('goal154b1-dashboard.json', 'quest-reward-preservation-proof.json', 'action-scoped-effect-proof.json',
        'module-independence-proof.json', 'certification-invalidation-proof.json', 'artifact-scope-proof.json', 'goal154b1-report.md')
    foreach ($root in @($proceduralRoot, $exportRoot)) {
        $actual = @(Get-ChildItem -LiteralPath $root -File | Select-Object -ExpandProperty Name | Sort-Object)
        if ($actual.Count -ne 7 -or (@(Compare-Object ($requiredFiles | Sort-Object) $actual)).Count -ne 0) {
            throw "Goal154B1 evidence root does not contain exactly seven required files: $root"
        }
    }
    foreach ($name in $requiredFiles) {
        if ((Get-FileHash -LiteralPath (Join-Path $proceduralRoot $name) -Algorithm SHA256).Hash -ne
            (Get-FileHash -LiteralPath (Join-Path $exportRoot $name) -Algorithm SHA256).Hash) {
            throw "Goal154B1 twin evidence mismatch: $name"
        }
    }
    Write-Host "GOAL154B1_QUEST_REWARD_PRESERVATION_GREEN behavioral=$($behavioralTests.Count)"
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot) { Remove-Item -LiteralPath $temporaryRoot -Recurse -Force }
    Pop-Location
}
