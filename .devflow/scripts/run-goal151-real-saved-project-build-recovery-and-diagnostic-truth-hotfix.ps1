param(
    [string]$SourceProject = 'C:\Users\endim\AppData\Local\LLMGameCreator\Games\goal148-manual',
    [string]$OutputRoot = ''
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
if (-not (Test-Path -LiteralPath $SourceProject -PathType Container)) {
    throw "Goal151 source project was not found: $SourceProject"
}
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repo ('.devflow\runs\goal151-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ'))
}
$OutputRoot = [IO.Path]::GetFullPath($OutputRoot)
New-Item -ItemType Directory -Force -Path $OutputRoot | Out-Null

$previousRun = $env:LLMGC_GOAL151_REAL_COPY_RUN
$previousSource = $env:LLMGC_GOAL151_SOURCE_PROJECT
$previousOutput = $env:LLMGC_GOAL151_OUTPUT_ROOT
try {
    $env:LLMGC_GOAL151_REAL_COPY_RUN = 'true'
    $env:LLMGC_GOAL151_SOURCE_PROJECT = [IO.Path]::GetFullPath($SourceProject)
    $env:LLMGC_GOAL151_OUTPUT_ROOT = $OutputRoot
    dotnet test (Join-Path $repo 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj') -c Debug --no-build `
        --filter 'FullyQualifiedName~Goal151RealSavedProjectBuildRecoveryTests.Actual_copy_runner'
    if ($LASTEXITCODE -ne 0) { throw "Goal151 real-copy runner failed with exit code $LASTEXITCODE" }
}
finally {
    $env:LLMGC_GOAL151_REAL_COPY_RUN = $previousRun
    $env:LLMGC_GOAL151_SOURCE_PROJECT = $previousSource
    $env:LLMGC_GOAL151_OUTPUT_ROOT = $previousOutput
}

Write-Output "GOAL151_REAL_COPY_GREEN outputRoot=$OutputRoot"
