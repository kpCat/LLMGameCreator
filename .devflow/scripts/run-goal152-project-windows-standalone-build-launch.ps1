param(
    [string]$SourceProject = "$env:LOCALAPPDATA\LLMGameCreator\Games\goal148-manual",
    [string]$OutputRoot = ".llmgc\procedural\goal-152-accepted-mechanics-milestone-and-project-scoped-windows-standalone-build-launch\real-project-proof"
)

$ErrorActionPreference = 'Stop'
$env:LLMGC_GOAL152_REAL_STANDALONE_RUN = 'true'
$env:LLMGC_GOAL152_SOURCE_PROJECT = [IO.Path]::GetFullPath($SourceProject)
$env:LLMGC_GOAL152_OUTPUT_ROOT = [IO.Path]::GetFullPath($OutputRoot)
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter 'FullyQualifiedName~Goal152ProjectStandaloneBuildTests'
exit $LASTEXITCODE
