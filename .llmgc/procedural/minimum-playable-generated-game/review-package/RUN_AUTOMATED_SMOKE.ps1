Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"
Push-Location $PSScriptRoot
try {
    New-Item -ItemType Directory -Force -Path .\logs | Out-Null
    $launchLog = ".\logs\manual-alpha-player-launch.log"
    $playLoopLog = ".\logs\manual-alpha-player-play-loop.log"
    $arguments = @(
        "-batchmode",
        "-nographics",
        "-alphaSmokeExit",
        "-alphaPlayLoopSmokeExit",
        "-alphaLogPath",
        $launchLog,
        "-alphaPlayLoopLogPath",
        $playLoopLog
    )
    $process = Start-Process -FilePath ".\LLMGameCreatorAlpha.exe" -ArgumentList $arguments -Wait -PassThru
    if ($process.ExitCode -ne 0) { throw "Automated Alpha smoke failed with exit code $($process.ExitCode)." }
    if (-not (Test-Path -LiteralPath $launchLog)) { throw "Automated Alpha smoke did not produce launch log." }
    if (-not (Test-Path -LiteralPath $playLoopLog)) { throw "Automated Alpha smoke did not produce play-loop log." }
    $launchLines = Get-Content -LiteralPath $launchLog
    $playLoopLines = Get-Content -LiteralPath $playLoopLog
    if ($launchLines -notcontains "alpha_runtime.launch_completed=true") { throw "Launch log is missing alpha_runtime.launch_completed=true." }
    foreach ($marker in @(
        "alpha_runtime.play_loop_completed=true",
        "alpha_runtime.quest_loop_completed=true",
        "alpha_runtime.quest_completed.after=true",
        "alpha_runtime.reward_granted.after=true"
    )) {
        if ($playLoopLines -notcontains $marker) { throw "Play-loop log is missing $marker." }
    }
}
finally {
    Pop-Location
}
