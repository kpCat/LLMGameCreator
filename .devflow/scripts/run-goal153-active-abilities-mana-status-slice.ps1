param([switch]$SkipStandaloneSmoke)
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$tests = Join-Path $repositoryRoot 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$cacheKey = '6af4d5eb5b42f956110555b58fb4e276'
$hostRoot = Join-Path $env:LOCALAPPDATA "LLMGameCreator\StandaloneHostCache\$cacheKey\host"
$requiredHostPaths = @(
    (Join-Path $hostRoot 'LLMGameCreatorProjectHost.exe'),
    (Join-Path $hostRoot 'LLMGameCreatorProjectHost_Data'),
    (Join-Path $hostRoot 'UnityPlayer.dll'),
    (Join-Path $hostRoot 'MonoBleedingEdge'),
    (Join-Path $hostRoot 'host-cache-manifest.json')
)
if ($requiredHostPaths.Where({ -not (Test-Path -LiteralPath $_) }).Count -ne 0) {
    throw 'BLOCKED: required Goal152C standalone host cache is unavailable; Unity Editor will not be started.'
}
if (Get-Process Unity -ErrorAction SilentlyContinue) {
    throw 'BLOCKED: Unity process is already running; Goal153 requires a zero-Unity-process proof.'
}

Push-Location $repositoryRoot
try {
    dotnet build
    if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed' }

    if (-not $SkipStandaloneSmoke) { $env:LLMGC_GOAL153_STANDALONE = 'true' }
    dotnet test $tests -c Debug --no-build --filter 'FullyQualifiedName~Goal153'
    if ($LASTEXITCODE -ne 0) { throw 'Goal153 focused tests failed' }
    Remove-Item Env:LLMGC_GOAL153_STANDALONE -ErrorAction SilentlyContinue

    & (Join-Path $repositoryRoot '.devflow\scripts\run-capability-runtime-equipment-slice.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'accepted equipment regression failed' }
    & (Join-Path $repositoryRoot '.devflow\scripts\run-character-attributes-level-progression-slice.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'accepted attributes/progression regression failed' }
    & (Join-Path $repositoryRoot '.devflow\scripts\check-current-goal.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'current-goal documentation guard failed' }

    if (Get-Process Unity -ErrorAction SilentlyContinue) {
        throw 'FAILED: Unity process was observed after the cache-only standalone smoke.'
    }
    Write-Host 'GOAL153 GREEN: focused Runtime/workspace/replay and one cached hidden standalone smoke passed.'
}
finally {
    Remove-Item Env:LLMGC_GOAL153_STANDALONE -ErrorAction SilentlyContinue
    Pop-Location
}
