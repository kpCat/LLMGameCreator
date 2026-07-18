param(
    [switch]$NoBuild,
    [switch]$SkipSmoke
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$tests = Join-Path $root 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$configuration = 'Debug'

if (-not $NoBuild) {
    dotnet build $root
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

dotnet test $tests -c $configuration --no-build --filter 'FullyQualifiedName~Goal167'
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (-not $SkipSmoke) {
    Write-Output 'Goal167 standalone smoke is intentionally dispatched by the bounded product matrix only.'
}
