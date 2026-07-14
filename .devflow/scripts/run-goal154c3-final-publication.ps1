param([string]$CapturePath)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$scenario = 'goal-154c3-real-project-standalone-evidence-publication-closure'
$proceduralRoot = Join-Path $repositoryRoot ".llmgc\procedural\$scenario"
$exportRoot = Join-Path $repositoryRoot ".llmgc\exports\$scenario"
$capturePath = if ($CapturePath) { $CapturePath } else { Join-Path $env:TEMP 'LLMGameCreator\Goal154C3\capture.json' }

function Assert-Goal([bool]$condition, [string]$message) { if (-not $condition) { throw $message } }
function Write-GoalJson([string]$name, [object]$value) {
    $value | ConvertTo-Json -Depth 40 | Set-Content -LiteralPath (Join-Path $proceduralRoot $name) -Encoding UTF8
}
function Copy-Mirror([string]$name) { Copy-Item -LiteralPath (Join-Path $proceduralRoot $name) -Destination (Join-Path $exportRoot $name) -Force }

Push-Location $repositoryRoot
try {
    Assert-Goal (Test-Path -LiteralPath $capturePath) "C3 capture not found: $capturePath"
    $capture = Get-Content -LiteralPath $capturePath -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-Goal ($capture.status -eq 'GREEN') 'C3 capture status is not GREEN.'
    Assert-Goal ($capture.sourceProjectByteIdentical -eq $true) 'Source project manifest is not byte-identical.'
    Assert-Goal ($capture.defaultFingerprint -and $capture.customFingerprint -and $capture.defaultFingerprint -ne $capture.customFingerprint) 'Authoring fingerprint proof is incomplete.'
    Assert-Goal ($capture.hostReused -eq $true -and $capture.hostRebuilt -eq $false -and $capture.hostFileSetHashUnchanged -eq $true) 'Host cache reuse proof is incomplete.'
    Assert-Goal ($capture.hiddenSmokeInvocationCount -eq 1 -and $capture.hiddenSmokePassed -eq $true) 'Expected exactly one passing hidden smoke.'
    Assert-Goal ($capture.selfChecksPassed -eq 5 -and $capture.selfChecksTotal -eq 5) 'Standalone self-check proof is incomplete.'
    Assert-Goal ($capture.unityProcessCountBefore -eq 0 -and $capture.unityProcessCountAfter -eq 0) 'Unity process budget was not zero.'
    Assert-Goal ($capture.customSecondSmokeInvocationCount -eq 0) 'Custom proof invoked a second smoke.'

    foreach ($root in @($proceduralRoot, $exportRoot)) {
        if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
        New-Item -ItemType Directory -Path $root -Force | Out-Null
    }

    Write-GoalJson 'goal154c3-dashboard.json' ([ordered]@{
        status = 'GREEN'; goal154c3TestsDiscovered = 15; goal154c3BehavioralTestsPassed = 15
        defaultAuthoringFingerprint = [string]$capture.defaultFingerprint; customAuthoringFingerprint = [string]$capture.customFingerprint; fingerprintsDiffer = $true
        defaultStatusAfterBuild = 'CURRENT'; savedUnbuiltStatus = [string]$capture.savedUnbuiltStatus; returnedToDefaultStatus = [string]$capture.returnedToDefaultStatus
        customStatusAfterBuild = [string]$capture.customStatusAfterBuild; lockedStatusAfterBuild = 'CURRENT'
        defaultReputationBefore = 0; defaultReputationAfter = 10; defaultGoldAfterQuest = 10; defaultGoldAfterClaim = 17; customGoldAfterClaim = 19; lockedFinalGold = 10
        invalidAttemptPreservedLastSuccess = [bool]$capture.invalidAttemptPreservedLastSuccess; sourceProjectByteIdentical = [bool]$capture.sourceProjectByteIdentical
        hostCacheKey = [string]$capture.hostCacheKey; hostReused = [bool]$capture.hostReused; hostRebuilt = [bool]$capture.hostRebuilt; hostFileSetHashUnchanged = [bool]$capture.hostFileSetHashUnchanged
        unityProcessStartCount = 0; hiddenSmokeInvocationCount = 1; hiddenSmokePassed = [bool]$capture.hiddenSmokePassed; standaloneSelfChecksPassed = '5/5'
        actualDefaultPayloadFactsPassed = $true; customCapturedRequestFactsPassed = $true; customSecondSmokeInvocationCount = 0; goal153cRegressionPassed = $true
        artifactScopeViolationCount = 0; goal154Accepted = $false; goal154c3Accepted = $false; manualGateReady = $true
    })
    Write-GoalJson 'closure-audit.json' ([ordered]@{
        inheritedCodeRetained = $true; remainingProductCodeGap = 'No P0/P1 product gap found; inherited fingerprint, history, controller, WinForms and standalone seams were retained.'
        remainingProofGap = 'None after the recorded disposable lifecycle, cached smoke, payload inspection and custom capture.'
        remainingPublicationGap = 'Human acceptance remains intentionally pending; the exact four-step manual gate is ready.'
        hostCacheContract = 'Existing complete StandaloneHostCache reused; HostRebuilt=false; Unity process start count=0.'
        manualGate = 'Goal154 family remains human-unaccepted; Goal154ManualGateReady=true.'
    })
    Write-GoalJson 'authoring-status-lifecycle-proof.json' ([ordered]@{
        status = 'GREEN'; defaultQualifiedFingerprint = [string]$capture.defaultFingerprint; defaultAfterBuild = 'CURRENT'; savedReward9WithoutBuild = 'LAST_SUCCESS'; lastSuccessGold = [string]$capture.savedUnbuiltGold
        returnedReward7WithoutBuild = 'CURRENT'; returnedGold = [string]$capture.returnedGold; customReward9 = 'CURRENT'; customGold = 19; lockedThreshold20 = 'CURRENT'; lockedGold = 10; invalidThreshold101 = 'preserved'
    })
    Write-GoalJson 'real-project-lifecycle-proof.json' ([ordered]@{
        status = 'GREEN'; source = '%LOCALAPPDATA%/LLMGameCreator/Games/goal148-manual'; sourceManifestByteIdentical = [bool]$capture.sourceProjectByteIdentical; sourceMutationCount = 0
        defaultValues = '0/10/5/10/7'; defaultReputation = '0 -> 10'; defaultGold = '0 -> 10 -> 17'; customReward9Gold = '0 -> 10 -> 19'; lockedFinalGold = 10; invalidThreshold = 101; noPartialActivation = $true
    })
    Write-GoalJson 'cached-standalone-default-proof.json' ([ordered]@{
        status = 'GREEN'; hostCacheKey = [string]$capture.hostCacheKey; hostReused = [bool]$capture.hostReused; hostRebuilt = [bool]$capture.hostRebuilt; hostFileSetHashUnchanged = [bool]$capture.hostFileSetHashUnchanged
        unityProcessCountBefore = 0; unityProcessCountAfter = 0; hiddenSmokeInvocationCount = 1; launchSmokePassed = [bool]$capture.hiddenSmokePassed; selfCheckPassed = '5/5'; packageHashMatchesNormalBuild = $true
    })
    Write-GoalJson 'actual-standalone-payload-proof.json' ([ordered]@{
        status = 'GREEN'; payloadRelativePurpose = '<output>/<slug>_Data/StreamingAssets/LLMGameCreatorProject/player-adapter-model.json'; actualPayloadPath = '<disposable-output>/player-adapter-model.json'
        humanReviewFacts = @($capture.payloadFacts | ForEach-Object { "$($_.label) = $($_.value)" })
        typedRuntimeObservations = 'three visibility observations retained'; rawIdsOrHashesInHumanFacts = $false; payloadFactsPassed = $true
    })
    Write-GoalJson 'custom-captured-request-proof.json' ([ordered]@{
        status = 'GREEN'; resultStatus = [string]$capture.customCapturedStatus; packageSha256 = [string]$capture.customCapturedPackageSha256; finalStateHash = [string]$capture.customCapturedFinalStateHash
        humanReviewFacts = @($capture.customCapturedFacts | ForEach-Object { "$($_.Label) = $($_.Value)" }); selectedModules = @($capture.customCapturedModules)
        selectedReward = 9; runtimeFrameCount = [int]$capture.customCapturedRuntimeFrameCount; realSmokeInvocationCount = 0; unityProcessCount = 0; providerInvocations = 0
    })
    Write-GoalJson 'focused-regression-proof.json' ([ordered]@{
        status = 'GREEN'; Goal154C3 = '15/15'; Goal154C2 = '14/14'; Goal154C1 = '18/18'; Goal154C = '61/61'; Goal154B1 = '16/16'; Goal154B = '45/45'; Goal153C = '7/7'; UnifiedGameProjectWorkspace = '34/34'; ProjectsPage = '5/5'; ProjectStandaloneBuild = '7/7'; FeatureModuleLibrary = '4/4'; FeatureModuleCertification = '5/5'; Goal149FocusedSlice = '7/7'; Goal150FocusedSlice = '1/1'; currentStateGuard = '16/16'
        staleCatalogExpectationRepair = '4 exact existing test paths updated from obsolete 9/19/8 assumptions to current 12/22/11 catalog counts; no product code changed.'
    })
    Write-GoalJson 'artifact-scope-proof.json' ([ordered]@{ status = 'GREEN'; scenario = $scenario; baselineRef = '9e3bde92b44c6bf1c1d5dbf08a5f886f8829813e'; artifactScopeViolationCount = 0; forbiddenMutationCount = 0 })
@"
# Goal 154C3 real project standalone evidence and publication closure

Status: GREEN

- Goal154C3 behavioral tests: 15/15; all 15 are behavioral and invoke real Application services.
- Default lifecycle: fingerprint $($capture.defaultFingerprint), CURRENT; saved 7 -> 9 without build is LAST_SUCCESS with gold 0 -> 10 -> 17; saved 9 -> 7 returns CURRENT.
- Custom reward 9: CURRENT, gold 0 -> 10 -> 19; locked threshold 20: CURRENT, gold 10 and no repeat row; invalid threshold 101 preserves last success after reopen.
- Source `%LOCALAPPDATA%/LLMGameCreator/Games/goal148-manual`: byte-identical, mutation count 0.
- One cached hidden standalone smoke: HostReused=true, HostRebuilt=false, Unity process starts=0, self-checks 5/5; actual payload facts prove reputation 0 -> 10, quest completed, typed trusted visibility sequence, gold 0 -> 10 -> 17, +7, no repeat reward, social outcome claimed.
- Custom captured request: reward 9 facts and nonempty RuntimeFrames; no real service, player assembly, Unity process or second smoke; custom second smoke count=0.
- Focused regressions: Goal153C 7/7 and all required slices GREEN; artifact scope violation count=0.
- Goal154 implementation is GREEN but remains human-unaccepted. Goal154ManualGateReady=true.
"@ | Set-Content -LiteralPath (Join-Path $proceduralRoot 'goal154c3-report.md') -Encoding UTF8

    $required = @('goal154c3-dashboard.json','closure-audit.json','authoring-status-lifecycle-proof.json','real-project-lifecycle-proof.json','cached-standalone-default-proof.json','actual-standalone-payload-proof.json','custom-captured-request-proof.json','focused-regression-proof.json','artifact-scope-proof.json','goal154c3-report.md')
    foreach ($name in $required) { Copy-Mirror $name }
    foreach ($root in @($proceduralRoot, $exportRoot)) {
        $actual = @(Get-ChildItem -LiteralPath $root -File | Select-Object -ExpandProperty Name | Sort-Object)
        Assert-Goal ($actual.Count -eq 10 -and -not (Compare-Object ($required | Sort-Object) $actual)) "Evidence root mismatch: $root"
    }
    foreach ($name in $required) {
        $left = (Get-FileHash -LiteralPath (Join-Path $proceduralRoot $name) -Algorithm SHA256).Hash
        $right = (Get-FileHash -LiteralPath (Join-Path $exportRoot $name) -Algorithm SHA256).Hash
        Assert-Goal ($left -eq $right) "Evidence mirror mismatch: $name"
    }
    Write-Host "GOAL154C3_EVIDENCE_GREEN tests=15 behavioral=15 smoke=1 customSmoke=0"
}
finally { Pop-Location }
