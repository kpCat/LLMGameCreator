param(
    [string]$OutputName = ""
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment

$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$Stamp = Get-Date -Format "yyyyMMdd_HHmmss"

if ([string]::IsNullOrWhiteSpace($OutputName)) {
    $OutputName = "LLMGC_Devflow_Diagnostics_$Stamp.zip"
}

$WorkDir = Join-Path $RepoRoot ".devflow\runs\$Stamp-diagnostics"
$OutPath = Join-Path $RepoRoot ".devflow\runs\$OutputName"

New-Item -ItemType Directory -Force -Path $WorkDir | Out-Null

function Copy-IfExists {
    param(
        [string]$Source,
        [string]$DestinationFolder
    )

    if (Test-Path $Source) {
        New-Item -ItemType Directory -Force -Path $DestinationFolder | Out-Null
        Copy-Item -Path $Source -Destination $DestinationFolder -Recurse -Force
    }
}

Push-Location $RepoRoot
try {
    Copy-IfExists ".devflow\*.md" (Join-Path $WorkDir "devflow")
    Copy-IfExists ".devflow\TASK_GRAPH.json" (Join-Path $WorkDir "devflow")
    Copy-IfExists ".devflow\KNOWN_WARNINGS.json" (Join-Path $WorkDir "devflow")
    Copy-IfExists ".devflow\scripts" (Join-Path $WorkDir "devflow")
    Copy-IfExists "docs\CURRENT_GENERATOR_STATE.md" (Join-Path $WorkDir "docs")
    Copy-IfExists "docs\CURRENT_GENERATOR_STATE.json" (Join-Path $WorkDir "docs")
    Copy-IfExists ".llmgc\generator-plans\generator_plan_strict_llm_evaluation.json" (Join-Path $WorkDir "llmgc-generator-plans")
    Copy-IfExists ".llmgc\generator-plans\generator_plan_strict_llm_evaluation_report.md" (Join-Path $WorkDir "llmgc-generator-plans")

    $latestCheckAll = Get-ChildItem ".devflow\runs" -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like "*check-all" } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($latestCheckAll) {
        Copy-Item -Path $latestCheckAll.FullName -Destination (Join-Path $WorkDir "latest-check-all") -Recurse -Force
    }

    $manifest = [ordered]@{
        created_utc = (Get-Date).ToUniversalTime().ToString("o")
        repo_root = "$RepoRoot"
        includes_git = $false
        includes_secrets = $false
        dotnet_cli_ui_language = $env:DOTNET_CLI_UI_LANGUAGE
        code_page = "65001"
        note = "No git directory, API keys or provider secrets are intentionally collected."
    }

    $manifest | ConvertTo-Json -Depth 8 | Set-Content -Encoding UTF8 -Path (Join-Path $WorkDir "diagnostics_manifest.json")

    if (Test-Path $OutPath) {
        Remove-Item $OutPath -Force
    }

    Compress-Archive -Path (Join-Path $WorkDir "*") -DestinationPath $OutPath -Force
    Write-Host "Diagnostics bundle created: $OutPath"
}
finally {
    Pop-Location
}
