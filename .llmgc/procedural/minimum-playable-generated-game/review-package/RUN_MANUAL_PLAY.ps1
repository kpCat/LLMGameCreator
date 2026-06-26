Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"
Push-Location $PSScriptRoot
try {
    & .\LLMGameCreatorAlpha.exe
}
finally {
    Pop-Location
}
