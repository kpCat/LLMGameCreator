[CmdletBinding()]
param(
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$testProject = Join-Path $repositoryRoot 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'

Push-Location $repositoryRoot
try {
    dotnet build
    if ($LASTEXITCODE -ne 0) { throw 'Goal166 build failed.' }
    dotnet test $testProject -c $Configuration --no-build --list-tests --filter 'FullyQualifiedName~Goal166'
    if ($LASTEXITCODE -ne 0) { throw 'Goal166 test discovery failed.' }
    dotnet test $testProject -c $Configuration --no-build --filter 'FullyQualifiedName~Goal166'
    if ($LASTEXITCODE -ne 0) { throw 'Goal166 focused tests failed.' }
}
finally {
    Pop-Location
}
