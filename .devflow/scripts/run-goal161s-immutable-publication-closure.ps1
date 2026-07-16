param()

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$testsProject = Join-Path $repositoryRoot 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$runRoot = Join-Path $repositoryRoot '.devflow\runs\goal161s-immutable-publication-closure'
$ledgerPath = Join-Path $runRoot 'hidden-smoke-ledger.json'
$capturePath = Join-Path $runRoot 'qualification-capture.json'
$utf8 = [Text.UTF8Encoding]::new($false)

function Write-Json([string]$path, [object]$value) {
    New-Item -ItemType Directory -Path (Split-Path -Parent $path) -Force | Out-Null
    [IO.File]::WriteAllText($path, (($value | ConvertTo-Json -Depth 30) + [Environment]::NewLine), $utf8)
}
function Assert-Goal([bool]$condition, [string]$message) { if (-not $condition) { throw $message } }

Push-Location $repositoryRoot
try {
    Assert-Goal (-not (Test-Path -LiteralPath $ledgerPath)) 'Goal161S hidden smoke ledger already exists; retry is forbidden.'
    Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) 'Unity process exists before Goal161S qualification.'
    $cacheKey = '6af4d5eb5b42f956110555b58fb4e276'
    $hostRoot = Join-Path $env:LOCALAPPDATA "LLMGameCreator\StandaloneHostCache\$cacheKey\host"
    Assert-Goal (Test-Path -LiteralPath (Join-Path $hostRoot 'LLMGameCreatorProjectHost.exe')) 'Expected cached host is missing.'
    Write-Json $ledgerPath ([ordered]@{ schemaVersion='goal161s_hidden_smoke_ledger_v1'; status='STARTED'; invocationCount=1; correctiveRetryCount=0; expectedHostCacheKey=$cacheKey; startedAtUtc=[DateTime]::UtcNow.ToString('o') })
    $oldSmoke = [Environment]::GetEnvironmentVariable('LLMGC_GOAL161_RUN_SMOKE')
    $oldCapture = [Environment]::GetEnvironmentVariable('LLMGC_GOAL161_CAPTURE_PATH')
    [Environment]::SetEnvironmentVariable('LLMGC_GOAL161_RUN_SMOKE', 'true')
    [Environment]::SetEnvironmentVariable('LLMGC_GOAL161_CAPTURE_PATH', $capturePath)
    try { & dotnet test $testsProject -c Debug --no-build --nologo --filter 'FullyQualifiedName=LLMGameCreator.Tests.Application.Goal161.Goal161StandaloneAndPortabilityTests.Behavioral_exactly_one_cached_hidden_smoke_runs_after_migration' --logger 'console;verbosity=minimal'; $testExit=$LASTEXITCODE }
    finally { [Environment]::SetEnvironmentVariable('LLMGC_GOAL161_RUN_SMOKE', $oldSmoke); [Environment]::SetEnvironmentVariable('LLMGC_GOAL161_CAPTURE_PATH', $oldCapture) }
    Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) 'Unity process exists after Goal161S qualification.'
    if ($testExit -ne 0) { Write-Json $ledgerPath ([ordered]@{ schemaVersion='goal161s_hidden_smoke_ledger_v1'; status='BLOCKED'; invocationCount=1; correctiveRetryCount=0; testExitCode=$testExit; capturePresent=(Test-Path -LiteralPath $capturePath); completedAtUtc=[DateTime]::UtcNow.ToString('o') }); throw 'Goal161S single hidden standalone smoke failed; retry is forbidden.' }
    Assert-Goal (Test-Path -LiteralPath $capturePath) 'Goal161S qualification capture is missing.'
    $capture = Get-Content -LiteralPath $capturePath -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-Goal ($capture.status -eq 'GREEN') 'Goal161S standalone result is not GREEN.'
    Assert-Goal ($capture.outputLocationKind -eq 'immutable_short_local_appdata_run') 'Goal161S immutable output kind is missing.'
    Assert-Goal (-not [string]::IsNullOrWhiteSpace($capture.outputRunDirectoryName)) 'Goal161S run directory is missing.'
    Assert-Goal ((Test-Path -LiteralPath $capture.currentPointerPath) -and (Test-Path -LiteralPath $capture.runStatusPath)) 'Goal161S pointer or run status is missing.'
    Assert-Goal ([int]$capture.maximumPlayerPathLength -le 240 -and [bool]$capture.playerPathBudgetPassed) 'Goal161S path budget failed.'
    Assert-Goal ([bool]$capture.payloadSelfCheckPassed -and [bool]$capture.legacyHostParserCompatibilityPassed) 'Goal161S preflight failed.'
    Assert-Goal ([bool]$capture.HostReused -and -not [bool]$capture.HostRebuilt -and [int]$capture.unityProcessStartCount -eq 0) 'Goal161S host/Unity contract failed.'
    Assert-Goal ([int]$capture.smokeExitCode -eq 0 -and [bool]$capture.playerLogPresent) 'Goal161S player smoke failed.'
    Assert-Goal ([bool]$capture.allSelectableReleaseCandidateCurrent -and [bool]$capture.portableAllSelectablePassed -and [bool]$capture.portableCoreOnlyPassed -and [bool]$capture.coreOnlyNoFalseRcReady) 'Goal161S RC or portability contract failed.'
    Write-Json $ledgerPath ([ordered]@{ schemaVersion='goal161s_hidden_smoke_ledger_v1'; status='GREEN'; invocationCount=1; correctiveRetryCount=0; hostCacheKey=[string]$capture.HostCacheKey; hostReused=[bool]$capture.HostReused; hostRebuilt=[bool]$capture.HostRebuilt; unityEditorProcessStartCount=[int]$capture.unityProcessStartCount; smokeExitCode=[int]$capture.smokeExitCode; playerLogPresent=[bool]$capture.playerLogPresent; outputProjectToken=[string]$capture.outputProjectToken; outputRunDirectoryName=[string]$capture.outputRunDirectoryName; maximumPlayerPathLength=[int]$capture.maximumPlayerPathLength; completedAtUtc=[DateTime]::UtcNow.ToString('o') })
    Write-Host 'GOAL161S_QUALIFICATION_GREEN smoke=1 retry=0 hostReused=true hostRebuilt=false unity=0 exit=0 pointer=atomic'
}
finally { Pop-Location }
