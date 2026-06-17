Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment

$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath

$RequiredFiles = @(
    "LLMGameCreator.sln",
    "AGENTS.md",
    "docs\CONTEXT_INDEX.md",
    "docs\CURRENT_GENERATOR_STATE.md",
    ".devflow\LOCAL_AGENT_ROLE.md",
    ".devflow\AUTONOMOUS_RUNBOOK.md",
    ".devflow\STOP_CONDITIONS.md",
    ".devflow\TASK_GRAPH.json",
    ".devflow\KNOWN_WARNINGS.json",
    ".devflow\VERIFICATION_MATRIX.md",
    ".devflow\NEXT_TASK.md"
)

Push-Location $RepoRoot
try {
    foreach ($file in $RequiredFiles) {
        if (-not (Test-Path $file)) {
            throw "Required file missing: $file"
        }
    }

    $taskGraphRaw = Get-Content -Raw -Encoding UTF8 ".devflow\TASK_GRAPH.json"
    $taskGraph = $taskGraphRaw | ConvertFrom-Json

    if (-not $taskGraph.schema_version) {
        throw "TASK_GRAPH.json: schema_version is missing."
    }

    if (-not $taskGraph.tasks) {
        throw "TASK_GRAPH.json: tasks array is missing or empty."
    }

    $ids = @{}
    foreach ($task in $taskGraph.tasks) {
        if (-not $task.id) {
            throw "TASK_GRAPH.json: task without id."
        }

        if ($ids.ContainsKey($task.id)) {
            throw "TASK_GRAPH.json: duplicate task id: $($task.id)"
        }

        $ids[$task.id] = $true

        if (-not $task.title) {
            throw "TASK_GRAPH.json: task '$($task.id)' missing title."
        }

        if (-not $task.status) {
            throw "TASK_GRAPH.json: task '$($task.id)' missing status."
        }

        if ($null -eq $task.requires_approval) {
            throw "TASK_GRAPH.json: task '$($task.id)' missing requires_approval."
        }
    }

    $knownWarningsRaw = Get-Content -Raw -Encoding UTF8 ".devflow\KNOWN_WARNINGS.json"
    $knownWarnings = $knownWarningsRaw | ConvertFrom-Json
    if (-not $knownWarnings.known_warnings) {
        throw "KNOWN_WARNINGS.json: known_warnings array is missing or empty."
    }

    $nextRaw = Get-Content -Raw -Encoding UTF8 ".devflow\NEXT_TASK.md"
    if ($nextRaw -notmatch "BASELINE-001|[A-Z0-9]+-[0-9]+|STOP_REVIEW") {
        Write-Warning "NEXT_TASK.md does not clearly contain a task id."
    }

    Write-Host "Devflow state check passed. Tasks: $($taskGraph.tasks.Count). Known warnings: $($knownWarnings.known_warnings.Count)."
    exit 0
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}
finally {
    Pop-Location
}
