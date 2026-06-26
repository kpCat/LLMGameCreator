Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"
Push-Location $PSScriptRoot
try {
    New-Item -ItemType Directory -Force -Path .\logs | Out-Null
    & .\LLMGameCreatorAlpha.exe -batchmode -nographics -alphaSmokeExit -alphaPlayLoopSmokeExit -alphaLogPath .\logs\manual-alpha-player-launch.log -alphaPlayLoopLogPath .\logs\manual-alpha-player-play-loop.log
    if ($LASTEXITCODE -ne 0) { throw "Automated Alpha smoke failed with exit code $LASTEXITCODE." }
}
finally {
    Pop-Location
}
