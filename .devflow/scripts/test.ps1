param(
    [string]$Configuration = "Debug",
    [switch]$NoBuild
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment

$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath

Push-Location $RepoRoot
try {
    $argsList = @(
        "test",
        "tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj",
        "--configuration",
        $Configuration,
        "/p:EnableWindowsTargeting=true"
    )

    if ($NoBuild) {
        $argsList += "--no-build"
    }

    & dotnet @argsList
    exit $LASTEXITCODE
}
finally {
    Pop-Location
}
