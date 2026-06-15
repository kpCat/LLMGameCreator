param(
    [string]$Configuration = "Debug",
    [switch]$NoRestore
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment

$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath

Push-Location $RepoRoot
try {
    if (-not $NoRestore) {
        dotnet restore LLMGameCreator.sln
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }

    dotnet build LLMGameCreator.sln --configuration $Configuration --no-restore /p:EnableWindowsTargeting=true
    exit $LASTEXITCODE
}
finally {
    Pop-Location
}
