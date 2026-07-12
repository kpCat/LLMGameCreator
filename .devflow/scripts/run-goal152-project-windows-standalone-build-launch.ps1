param(
    [string]$SourceProject = "$env:LOCALAPPDATA\LLMGameCreator\Games\goal148-manual",
    [string]$OutputRoot = "$env:LOCALAPPDATA\LLMGameCreator\Goal152StandaloneProof"
)

$ErrorActionPreference = 'Stop'
$env:LLMGC_GOAL152_REAL_STANDALONE_RUN = 'true'
$env:LLMGC_GOAL152_SOURCE_PROJECT = [IO.Path]::GetFullPath($SourceProject)
$env:LLMGC_GOAL152_OUTPUT_ROOT = [IO.Path]::GetFullPath($OutputRoot)
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter 'FullyQualifiedName~Goal152ProjectStandaloneBuildTests'
exit $LASTEXITCODE
