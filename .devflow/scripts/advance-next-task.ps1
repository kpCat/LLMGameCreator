Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$ScriptPath = $MyInvocation.MyCommand.Path
$ScriptDir = Split-Path -Parent $ScriptPath
$DevflowDir = Split-Path -Parent $ScriptDir
$NextTaskPath = Join-Path $DevflowDir "NEXT_TASK.md"
$QueuePath = Join-Path $DevflowDir "task-queue.json"

function Write-DevflowUtf8NoBom {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Content
    )

    $encoding = New-Object System.Text.UTF8Encoding -ArgumentList $false
    [System.IO.File]::WriteAllText($Path, $Content, $encoding)
}

function Get-QueueIds {
    param([Parameter(Mandatory = $true)]$Queue)

    return @($Queue.queue | ForEach-Object { [string]$_.id })
}

if (-not (Test-Path -LiteralPath $NextTaskPath)) {
    throw "NEXT_TASK.md not found: $NextTaskPath"
}

if (-not (Test-Path -LiteralPath $QueuePath)) {
    throw "task-queue.json not found: $QueuePath"
}

$nextRaw = [System.IO.File]::ReadAllText($NextTaskPath, [System.Text.Encoding]::UTF8)
if ($nextRaw -match "(?m)^\s*Task id:\s*(?<id>[A-Za-z0-9_]+)\s*$") {
    $currentId = $Matches['id']
}
else {
    throw "NEXT_TASK.md does not contain a readable 'Task id:' line."
}

$queueRaw = [System.IO.File]::ReadAllText($QueuePath, [System.Text.Encoding]::UTF8)
$queue = $queueRaw | ConvertFrom-Json

if (-not $queue.queue) {
    throw "task-queue.json does not contain a non-empty 'queue' array."
}

$queueItems = @($queue.queue)
$currentIndex = -1
for ($index = 0; $index -lt $queueItems.Count; $index++) {
    if ([string]$queueItems[$index].id -eq $currentId) {
        $currentIndex = $index
        break
    }
}

if ($currentIndex -lt 0) {
    $knownIds = (Get-QueueIds -Queue $queue) -join ", "
    throw "NEXT_TASK.md Task id '$currentId' is not present in task-queue.json. Known ids: $knownIds"
}

if ($currentId -eq "STOP_REVIEW") {
    throw "NEXT_TASK.md is already STOP_REVIEW; do not advance to future work."
}

if ($currentIndex -ge ($queueItems.Count - 1)) {
    throw "NEXT_TASK.md Task id '$currentId' is the final queue entry; no next task is available."
}

$nextItem = $queueItems[$currentIndex + 1]
$nextId = [string]$nextItem.id

if (-not $nextItem.next_task_lines) {
    throw "task-queue.json entry '$nextId' is missing next_task_lines."
}

$nextBlock = ((@($nextItem.next_task_lines) | ForEach-Object { [string]$_ }) -join "`r`n") + "`r`n"
Write-DevflowUtf8NoBom -Path $NextTaskPath -Content $nextBlock

if ($nextId -eq "STOP_REVIEW") {
    Write-Host "Advanced NEXT_TASK.md from '$currentId' to '$nextId'. STOP_REVIEW written; do not start future work."
}
else {
    Write-Host "Advanced NEXT_TASK.md from '$currentId' to '$nextId'. The script only updated NEXT_TASK.md; do not execute the next task in this run."
}
