param(
    [string]$SourceProject = "",
    [string]$OutputRoot = ""
)

$ErrorActionPreference = 'Stop'
$localRoot = [Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)
if ([string]::IsNullOrWhiteSpace($SourceProject)) { $SourceProject = Join-Path $localRoot 'LLMGameCreator\Games\goal148-manual' }
if ([string]::IsNullOrWhiteSpace($OutputRoot)) { $OutputRoot = Join-Path $localRoot 'LLMGameCreator\G152A' }
$env:LLMGC_GOAL152_REAL_STANDALONE_RUN = 'true'
$env:LLMGC_GOAL152_SOURCE_PROJECT = [IO.Path]::GetFullPath($SourceProject)
$env:LLMGC_GOAL152_OUTPUT_ROOT = [IO.Path]::GetFullPath($OutputRoot)
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter 'FullyQualifiedName~Goal152ProjectStandaloneBuildTests'
exit $LASTEXITCODE
