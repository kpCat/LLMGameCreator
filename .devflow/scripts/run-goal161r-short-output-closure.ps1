param()

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$testsProject = Join-Path $repositoryRoot 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$runRoot = Join-Path $repositoryRoot '.devflow\runs\goal161r-validation'
$ledgerPath = Join-Path $repositoryRoot '.devflow\runs\goal161r-hidden-smoke-ledger.json'
$goal161qLedger = Join-Path $repositoryRoot '.devflow\runs\goal161q-hidden-smoke-ledger.json'
$capturePath = Join-Path $runRoot 'qualification-capture.json'
$utf8 = [Text.UTF8Encoding]::new($false)

function Assert-Goal([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
}

function Write-Json([string]$path, [object]$value) {
    $parent = Split-Path -Parent $path
    if (-not (Test-Path -LiteralPath $parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }
    [IO.File]::WriteAllText($path, (($value | ConvertTo-Json -Depth 30) + [Environment]::NewLine), $utf8)
}

Push-Location $repositoryRoot
try {
    Assert-Goal (-not (Test-Path -LiteralPath $ledgerPath)) 'Goal161R hidden smoke ledger already exists; corrective retry budget is zero.'
    Assert-Goal (Test-Path -LiteralPath $goal161qLedger) 'Goal161Q ledger is required before Goal161R qualification.'
    $qLedger = Get-Content -LiteralPath $goal161qLedger -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-Goal ($qLedger.status -eq 'BLOCKED' -and [int]$qLedger.invocationCount -eq 1) 'Goal161Q ledger does not prove one consumed diagnostic smoke.'
    Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) 'Unity process exists before Goal161R qualification.'

    $cacheKey = '6af4d5eb5b42f956110555b58fb4e276'
    $hostRoot = Join-Path $env:LOCALAPPDATA "LLMGameCreator\StandaloneHostCache\$cacheKey\host"
    Assert-Goal (Test-Path -LiteralPath (Join-Path $hostRoot 'LLMGameCreatorProjectHost.exe')) 'Goal161R expected cached host executable is missing.'
    Assert-Goal (Test-Path -LiteralPath (Join-Path $hostRoot 'host-cache-manifest.json')) 'Goal161R expected host cache manifest is missing.'

    Write-Json $ledgerPath ([ordered]@{
        schemaVersion = 'goal161r_hidden_smoke_ledger_v1'; status = 'STARTED'; invocationCount = 1
        correctiveRetryCount = 0; expectedHostCacheKey = $cacheKey; startedAtUtc = [DateTime]::UtcNow.ToString('o')
    })

    $oldSmoke = [Environment]::GetEnvironmentVariable('LLMGC_GOAL161_RUN_SMOKE')
    $oldCapture = [Environment]::GetEnvironmentVariable('LLMGC_GOAL161_CAPTURE_PATH')
    [Environment]::SetEnvironmentVariable('LLMGC_GOAL161_RUN_SMOKE', 'true')
    [Environment]::SetEnvironmentVariable('LLMGC_GOAL161_CAPTURE_PATH', $capturePath)
    try {
        & dotnet test $testsProject -c Debug --no-build --nologo --filter 'FullyQualifiedName=LLMGameCreator.Tests.Application.Goal161.Goal161StandaloneAndPortabilityTests.Behavioral_exactly_one_cached_hidden_smoke_runs_after_migration' --logger 'console;verbosity=minimal'
        $testExit = $LASTEXITCODE
    }
    finally {
        [Environment]::SetEnvironmentVariable('LLMGC_GOAL161_RUN_SMOKE', $oldSmoke)
        [Environment]::SetEnvironmentVariable('LLMGC_GOAL161_CAPTURE_PATH', $oldCapture)
    }

    Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) 'Unity process exists after Goal161R qualification.'
    if ($testExit -ne 0) {
        Write-Json $ledgerPath ([ordered]@{
            schemaVersion = 'goal161r_hidden_smoke_ledger_v1'; status = 'BLOCKED'; invocationCount = 1
            correctiveRetryCount = 0; expectedHostCacheKey = $cacheKey; testExitCode = $testExit
            completedAtUtc = [DateTime]::UtcNow.ToString('o')
        })
        throw 'Goal161R single hidden standalone smoke failed; retry is forbidden.'
    }

    Assert-Goal (Test-Path -LiteralPath $capturePath) 'Goal161R qualification capture is missing.'
    $capture = Get-Content -LiteralPath $capturePath -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-Goal ($capture.status -eq 'GREEN') 'Goal161R capture status is not GREEN.'
    Assert-Goal ([int]$capture.hiddenSmokeInvocationCount -eq 1) 'Goal161R smoke count is not exactly one.'
    Assert-Goal ($capture.outputLocationKind -eq 'short_local_appdata') 'Goal161R output location is not short_local_appdata.'
    Assert-Goal ([int]$capture.maximumPlayerPathLength -le 240 -and [bool]$capture.playerPathBudgetPassed) 'Goal161R player path budget failed.'
    Assert-Goal ([bool]$capture.payloadSelfCheckPassed -and [bool]$capture.legacyHostParserCompatibilityPassed) 'Goal161R payload preflight failed.'
    Assert-Goal ([bool]$capture.HostReused -and -not [bool]$capture.HostRebuilt) 'Goal161R cached host reuse contract failed.'
    Assert-Goal ([int]$capture.unityProcessStartCount -eq 0) 'Goal161R Unity Editor start count is nonzero.'
    Assert-Goal ([int]$capture.smokeExitCode -eq 0 -and [bool]$capture.playerLogPresent) 'Goal161R smoke exit or Player.log failed.'
    Assert-Goal ([bool]$capture.allSelectableReleaseCandidateCurrent) 'Goal161R release candidate is not CURRENT.'
    Assert-Goal ([bool]$capture.portableAllSelectablePassed -and [bool]$capture.portableCoreOnlyPassed -and [bool]$capture.coreOnlyNoFalseRcReady) 'Goal161R portability truth failed.'

    Write-Json $ledgerPath ([ordered]@{
        schemaVersion = 'goal161r_hidden_smoke_ledger_v1'; status = 'GREEN'; invocationCount = 1
        correctiveRetryCount = 0; hostCacheKey = [string]$capture.HostCacheKey; hostReused = [bool]$capture.HostReused
        hostRebuilt = [bool]$capture.HostRebuilt; unityEditorProcessStartCount = [int]$capture.unityProcessStartCount
        smokeExitCode = [int]$capture.smokeExitCode; playerLogPresent = [bool]$capture.playerLogPresent
        outputLocationKind = [string]$capture.outputLocationKind; outputProjectToken = [string]$capture.outputProjectToken
        maximumPlayerPathLength = [int]$capture.maximumPlayerPathLength; completedAtUtc = [DateTime]::UtcNow.ToString('o')
    })
    Write-Host 'GOAL161R_QUALIFICATION_GREEN smoke=1 retry=0 hostReused=true hostRebuilt=false unity=0 exit=0 pathBudget=240'
}
finally {
    Pop-Location
}
