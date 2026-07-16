param()

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$testsProject = Join-Path $repositoryRoot 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$runRoot = Join-Path $repositoryRoot '.devflow\runs\goal161q-validation'
$ledgerPath = Join-Path $repositoryRoot '.devflow\runs\goal161q-hidden-smoke-ledger.json'
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
    Assert-Goal (-not (Test-Path -LiteralPath $ledgerPath)) 'Goal161Q hidden smoke ledger already exists; corrective retry budget is zero.'
    Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) 'Unity process exists before Goal161Q qualification.'

    $cacheKey = '6af4d5eb5b42f956110555b58fb4e276'
    $hostRoot = Join-Path $env:LOCALAPPDATA "LLMGameCreator\StandaloneHostCache\$cacheKey\host"
    Assert-Goal (Test-Path -LiteralPath (Join-Path $hostRoot 'LLMGameCreatorProjectHost.exe')) 'Goal161Q expected cached host executable is missing.'
    Assert-Goal (Test-Path -LiteralPath (Join-Path $hostRoot 'host-cache-manifest.json')) 'Goal161Q expected host cache manifest is missing.'

    Write-Json $ledgerPath ([ordered]@{
        schemaVersion = 'goal161q_hidden_smoke_ledger_v1'
        status = 'STARTED'
        invocationCount = 1
        correctiveRetryCount = 0
        expectedHostCacheKey = $cacheKey
        startedAtUtc = [DateTime]::UtcNow.ToString('o')
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

    Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) 'Unity process exists after Goal161Q qualification.'
    if ($testExit -ne 0) {
        Write-Json $ledgerPath ([ordered]@{
            schemaVersion = 'goal161q_hidden_smoke_ledger_v1'
            status = 'BLOCKED'
            invocationCount = 1
            correctiveRetryCount = 0
            expectedHostCacheKey = $cacheKey
            testExitCode = $testExit
            completedAtUtc = [DateTime]::UtcNow.ToString('o')
        })
        throw 'Goal161Q single hidden standalone smoke failed; retry is forbidden.'
    }

    Assert-Goal (Test-Path -LiteralPath $capturePath) 'Goal161Q qualification capture is missing.'
    $capture = Get-Content -LiteralPath $capturePath -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-Goal ($capture.status -eq 'GREEN') 'Goal161Q capture status is not GREEN.'
    Assert-Goal ([int]$capture.hiddenSmokeInvocationCount -eq 1) 'Goal161Q capture smoke count is not exactly one.'
    Assert-Goal ([bool]$capture.payloadSelfCheckPassed) 'Goal161Q payload preflight did not pass.'
    Assert-Goal ([bool]$capture.legacyHostParserCompatibilityPassed) 'Goal161Q legacy parser compatibility did not pass.'
    Assert-Goal ([bool]$capture.HostReused -and -not [bool]$capture.HostRebuilt) 'Goal161Q cached host reuse contract failed.'
    Assert-Goal ([int]$capture.unityProcessStartCount -eq 0) 'Goal161Q Unity Editor start count is nonzero.'
    Assert-Goal ([int]$capture.smokeExitCode -eq 0) 'Goal161Q player exit code is nonzero.'
    Assert-Goal ([bool]$capture.playerLogPresent) 'Goal161Q confined Player.log was not captured.'
    Assert-Goal ([bool]$capture.allSelectableReleaseCandidateCurrent) 'Goal161Q all-selectable release candidate is not CURRENT.'
    Assert-Goal ([bool]$capture.portableAllSelectablePassed) 'Goal161Q all-selectable portability failed.'
    Assert-Goal ([bool]$capture.portableCoreOnlyPassed -and [bool]$capture.coreOnlyNoFalseRcReady) 'Goal161Q core-only portability truth failed.'

    Write-Json $ledgerPath ([ordered]@{
        schemaVersion = 'goal161q_hidden_smoke_ledger_v1'
        status = 'GREEN'
        invocationCount = 1
        correctiveRetryCount = 0
        hostCacheKey = [string]$capture.HostCacheKey
        hostReused = [bool]$capture.HostReused
        hostRebuilt = [bool]$capture.HostRebuilt
        unityEditorProcessStartCount = [int]$capture.unityProcessStartCount
        smokeExitCode = [int]$capture.smokeExitCode
        playerLogPresent = [bool]$capture.playerLogPresent
        completedAtUtc = [DateTime]::UtcNow.ToString('o')
    })
    Write-Host 'GOAL161Q_QUALIFICATION_GREEN smoke=1 retry=0 hostReused=true hostRebuilt=false unity=0 exit=0'
}
finally {
    Pop-Location
}
