param(
    [string]$OutputRoot = ".devflow/runs/complete-test-suite",
    [int]$MonolithicTimeoutSeconds = 900,
    [int]$ShardTimeoutSeconds = 180,
    [int]$ClassesPerShard = 12,
    [switch]$SkipMonolithic
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"
$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment
$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$Project = Join-Path $RepoRoot "tests/LLMGameCreator.Tests/LLMGameCreator.Tests.csproj"
$ResolvedOutput = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($OutputRoot)) { $OutputRoot } else { Join-Path $RepoRoot $OutputRoot }))

if ($MonolithicTimeoutSeconds -lt 1 -or $MonolithicTimeoutSeconds -gt 900) { throw "Monolithic timeout must be between 1 and 900 seconds." }
if ($ShardTimeoutSeconds -lt 1) { throw "Shard timeout must be positive." }
if ($ClassesPerShard -lt 1) { throw "ClassesPerShard must be positive." }
if (Test-Path -LiteralPath $ResolvedOutput) { Remove-Item -LiteralPath $ResolvedOutput -Recurse -Force }
[IO.Directory]::CreateDirectory($ResolvedOutput) | Out-Null
[IO.Directory]::CreateDirectory((Join-Path $ResolvedOutput "trx")) | Out-Null
[IO.Directory]::CreateDirectory((Join-Path $ResolvedOutput "logs")) | Out-Null

function Write-Json([string]$Name, $Value) {
    $json = $Value | ConvertTo-Json -Depth 20
    [IO.File]::WriteAllText((Join-Path $ResolvedOutput $Name), $json + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
}

function Get-RelativePath([string]$BasePath, [string]$Path) {
    $baseFull = [IO.Path]::GetFullPath($BasePath).TrimEnd('\', '/')
    $pathFull = [IO.Path]::GetFullPath($Path)
    if (-not $pathFull.StartsWith($baseFull + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Path is outside the expected root: $Path"
    }
    return $pathFull.Substring($baseFull.Length + 1).Replace('\', '/')
}

function Invoke-CapturedProcess(
    [string]$Name,
    [string[]]$Arguments,
    [int]$TimeoutSeconds,
    [hashtable]$Environment = @{}) {
    $stdout = Join-Path $ResolvedOutput "logs/$Name.stdout.log"
    $stderr = Join-Path $ResolvedOutput "logs/$Name.stderr.log"
    $argumentLine = ($Arguments | ForEach-Object {
        if ($_ -match '[\s"]') { '"' + ($_ -replace '"', '\"') + '"' } else { $_ }
    }) -join ' '
    $previous = @{}
    foreach ($key in $Environment.Keys) {
        $previous[$key] = [Environment]::GetEnvironmentVariable($key, "Process")
        [Environment]::SetEnvironmentVariable($key, [string]$Environment[$key], "Process")
    }
    $started = [DateTime]::UtcNow
    try {
        $process = Start-Process -FilePath "dotnet" -ArgumentList $argumentLine -WorkingDirectory $RepoRoot `
            -RedirectStandardOutput $stdout -RedirectStandardError $stderr -PassThru -WindowStyle Hidden
        $peakWorkingSet = 0L
        $peakProcessCount = 0
        $lastCpuSeconds = 0.0
        $timedOut = $false
        while (-not $process.WaitForExit(1000)) {
            $process.Refresh()
            $elapsed = ([DateTime]::UtcNow - $started).TotalSeconds
            try {
                $related = @(Get-Process dotnet,testhost -ErrorAction SilentlyContinue)
                $peakProcessCount = [Math]::Max($peakProcessCount, $related.Count)
                $workingSet = ($related | Measure-Object WorkingSet64 -Sum).Sum
                if ($null -ne $workingSet) { $peakWorkingSet = [Math]::Max($peakWorkingSet, [long]$workingSet) }
                $cpu = ($related | Measure-Object CPU -Sum).Sum
                if ($null -ne $cpu) { $lastCpuSeconds = [double]$cpu }
            } catch { }
            if ($elapsed -ge $TimeoutSeconds) {
                $timedOut = $true
                & taskkill /PID $process.Id /T /F *> $null
                $process.WaitForExit()
                break
            }
        }
        $process.WaitForExit()
        $process.Refresh()
        $ended = [DateTime]::UtcNow
        $exitCode = if ($timedOut) { -1 } else { $process.ExitCode }
        return [ordered]@{
            name = $Name
            command = "dotnet " + $argumentLine
            startedAtUtc = $started.ToString("O")
            endedAtUtc = $ended.ToString("O")
            durationSeconds = [Math]::Round(($ended - $started).TotalSeconds, 3)
            timeoutSeconds = $TimeoutSeconds
            timedOut = $timedOut
            exitCode = $exitCode
            peakWorkingSetBytes = $peakWorkingSet
            observedDotnetAndTesthostProcessCount = $peakProcessCount
            observedAggregateCpuSeconds = [Math]::Round($lastCpuSeconds, 3)
            stdoutPath = Get-RelativePath $ResolvedOutput $stdout
            stderrPath = Get-RelativePath $ResolvedOutput $stderr
        }
    }
    finally {
        foreach ($key in $Environment.Keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key], "Process") }
    }
}

function Read-Trx([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return @() }
    [xml]$xml = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    $nodes = $xml.SelectNodes("//*[local-name()='UnitTestResult']")
    return @($nodes | ForEach-Object {
        $duration = [TimeSpan]::Zero
        if ($_.duration) { [TimeSpan]::TryParse([string]$_.duration, [ref]$duration) | Out-Null }
        [pscustomobject]@{
            name = [string]$_.testName
            outcome = [string]$_.outcome
            durationSeconds = [Math]::Round($duration.TotalSeconds, 6)
        }
    })
}

function Resolve-TestClass([string]$TestName) {
    $baseName = $TestName
    $parenthesis = $baseName.IndexOf('(')
    if ($parenthesis -ge 0) { $baseName = $baseName.Substring(0, $parenthesis) }
    $lastDot = $baseName.LastIndexOf('.')
    if ($lastDot -lt 1) { throw "Unable to resolve test class: $TestName" }
    return $baseName.Substring(0, $lastDot)
}

$dotnetInfoPath = Join-Path $ResolvedOutput "dotnet-info.txt"
& dotnet --info *> $dotnetInfoPath
$listPath = Join-Path $ResolvedOutput "logs/discovery.stdout.log"
$listErrorPath = Join-Path $ResolvedOutput "logs/discovery.stderr.log"
& dotnet test $Project -c Debug --no-build --list-tests 1> $listPath 2> $listErrorPath
if ($LASTEXITCODE -ne 0) { throw "Complete-suite discovery failed with exit code $LASTEXITCODE." }
$tests = @(Get-Content -LiteralPath $listPath -Encoding UTF8 | ForEach-Object { $_.Trim() } |
    Where-Object { $_ -like "LLMGameCreator.Tests.*" })
if ($tests.Count -eq 0) { throw "Complete-suite discovery returned zero tests." }
$duplicateDiscovery = @($tests | Group-Object | Where-Object Count -gt 1)
$inventory = @($tests | ForEach-Object {
    [pscustomobject]@{ name = $_; className = Resolve-TestClass $_ }
})
$classes = @($inventory.className | Sort-Object -Unique)
$shards = @()
for ($offset = 0; $offset -lt $classes.Count; $offset += $ClassesPerShard) {
    $end = [Math]::Min($classes.Count - 1, $offset + $ClassesPerShard - 1)
    $assignedClasses = @($classes[$offset..$end])
    $assignedTests = @($inventory | Where-Object { $assignedClasses -contains $_.className })
    $shards += [pscustomobject]@{
        shardId = "shard-{0:D4}" -f ($shards.Count + 1)
        classes = $assignedClasses
        discoveredCount = $assignedTests.Count
        testNames = @($assignedTests.name)
    }
}

$assignmentCounts = @{}
foreach ($shard in $shards) {
    foreach ($name in $shard.testNames) {
        $currentCount = if ($assignmentCounts.ContainsKey($name)) { [int]$assignmentCounts[$name] } else { 0 }
        $assignmentCounts[$name] = 1 + $currentCount
    }
}
$missingAssignments = @($tests | Where-Object { -not $assignmentCounts.ContainsKey($_) })
$duplicateAssignments = @($assignmentCounts.GetEnumerator() | Where-Object Value -gt 1)
Write-Json "complete-suite-discovery.json" ([ordered]@{
    schemaVersion = "complete_test_suite_discovery_v1"
    discoveredCount = $tests.Count
    classCount = $classes.Count
    duplicateDiscoveryCount = $duplicateDiscovery.Count
    testSdkVersion = "17.11.1"
    xunitVersion = "2.9.2"
    xunitRunnerVisualStudioVersion = "2.8.2"
    collectionParallelismDisabledBy = "tests/LLMGameCreator.Tests/UnityAlphaProductSmokeCollection.cs"
    tests = $inventory
})
Write-Json "complete-suite-shard-plan.json" ([ordered]@{
    schemaVersion = "complete_test_suite_shard_plan_v1"
    partitionKind = "disjoint_class_groups"
    classesPerShard = $ClassesPerShard
    maximumSimultaneousTesthostProcesses = 1
    shardTimeoutSeconds = $ShardTimeoutSeconds
    discoveredCount = $tests.Count
    assignedCount = ($shards | Measure-Object discoveredCount -Sum).Sum
    missingAssignmentCount = $missingAssignments.Count
    duplicateAssignmentCount = $duplicateAssignments.Count
    shards = $shards
})

$monolithic = if ($SkipMonolithic) {
    [ordered]@{ status = "SKIPPED"; reason = "SkipMonolithic was specified" }
} else {
    $trx = Join-Path $ResolvedOutput "trx/monolithic.trx"
    $run = Invoke-CapturedProcess "monolithic" @(
        "test", $Project, "-c", "Debug", "--no-build", "--logger", "trx;LogFileName=$trx",
        "--logger", "console;verbosity=detailed"
    ) $MonolithicTimeoutSeconds
    $results = @(Read-Trx $trx)
    [ordered]@{
        status = if ($run.timedOut) { "TIMEOUT" } elseif ($run.exitCode -eq 0) { "GREEN" } else { "FAILED" }
        run = $run
        executedCount = $results.Count
        passedCount = @($results | Where-Object outcome -eq "Passed").Count
        failedCount = @($results | Where-Object outcome -eq "Failed").Count
        skippedCount = @($results | Where-Object outcome -eq "NotExecuted").Count
        lastCompletedTest = if ($results.Count -gt 0) { $results[-1].name } else { $null }
    }
}
Write-Json "monolithic-suite-diagnostic.json" $monolithic

$shardResults = @()
$allResults = @()
foreach ($shard in $shards) {
    $filterParts = @($shard.classes | ForEach-Object { "FullyQualifiedName~$_." })
    $filter = $filterParts -join "|"
    $trx = Join-Path $ResolvedOutput ("trx/" + $shard.shardId + ".trx")
    $run = Invoke-CapturedProcess $shard.shardId @(
        "test", $Project, "-c", "Debug", "--no-build", "--filter", $filter,
        "--logger", "trx;LogFileName=$trx", "--logger", "console;verbosity=detailed"
    ) $ShardTimeoutSeconds
    $results = @(Read-Trx $trx)
    $allResults += $results
    $failed = @($results | Where-Object outcome -eq "Failed").Count
    $skipped = @($results | Where-Object outcome -eq "NotExecuted").Count
    $missing = [Math]::Max(0, [int]$shard.discoveredCount - $results.Count)
    $duplicate = [Math]::Max(0, $results.Count - [int]$shard.discoveredCount)
    $shardResults += [pscustomobject]@{
        shardId = $shard.shardId
        classes = $shard.classes
        discoveredCount = $shard.discoveredCount
        executedCount = $results.Count
        passedCount = @($results | Where-Object outcome -eq "Passed").Count
        failedCount = $failed
        skippedCount = $skipped
        missingCount = $missing
        duplicateCount = $duplicate
        aborted = [bool]($run.timedOut -or ($results.Count -eq 0 -and [int]$shard.discoveredCount -gt 0))
        lastCompletedTest = if ($results.Count -gt 0) { $results[-1].name } else { $null }
        run = $run
    }
}

$slowTests = @($allResults | Sort-Object durationSeconds -Descending | Select-Object -First 20)
$classDurations = @($allResults | ForEach-Object {
    [pscustomobject]@{ className = Resolve-TestClass $_.name; durationSeconds = $_.durationSeconds }
} | Group-Object className | ForEach-Object {
    [pscustomobject]@{
        className = $_.Name
        durationSeconds = [Math]::Round(($_.Group | Measure-Object durationSeconds -Sum).Sum, 6)
        executedCount = $_.Count
    }
} | Sort-Object durationSeconds -Descending | Select-Object -First 20)
Write-Json "complete-suite-slowest-tests.json" ([ordered]@{
    schemaVersion = "complete_test_suite_slowest_tests_v1"
    slowestTests = $slowTests
    slowestClasses = $classDurations
    slowestShards = @($shardResults | Sort-Object { $_.run.durationSeconds } -Descending | Select-Object -First 20)
})

$counts = [ordered]@{
    discovered = $tests.Count
    assigned = ($shards | Measure-Object discoveredCount -Sum).Sum
    executed = $allResults.Count
    passed = @($allResults | Where-Object outcome -eq "Passed").Count
    failed = @($allResults | Where-Object outcome -eq "Failed").Count
    skipped = @($allResults | Where-Object outcome -eq "NotExecuted").Count
    missing = ($shardResults | Measure-Object missingCount -Sum).Sum
    duplicate = ($shardResults | Measure-Object duplicateCount -Sum).Sum
    aborted = @($shardResults | Where-Object aborted).Count
}
$passed = $counts.missing -eq 0 -and $counts.duplicate -eq 0 -and $counts.failed -eq 0 -and $counts.aborted -eq 0 `
    -and $missingAssignments.Count -eq 0 -and $duplicateAssignments.Count -eq 0 -and $duplicateDiscovery.Count -eq 0
$runStart = if ($SkipMonolithic) { [DateTime]::UtcNow } else { [DateTime]$monolithic.run.startedAtUtc }
$touchedFixedRoots = @(Get-ChildItem -LiteralPath (Join-Path $RepoRoot ".llmgc") -File -Recurse -ErrorAction SilentlyContinue |
    Where-Object { $_.LastWriteTimeUtc -ge $runStart } |
    ForEach-Object { Get-RelativePath $RepoRoot $_.FullName } | Sort-Object -Unique)
$summary = [ordered]@{
    schemaVersion = "complete_test_suite_result_v1"
    status = if ($passed) { "GREEN" } else { "BLOCKED" }
    passed = $passed
    counts = $counts
    monolithicStatus = $monolithic.status
    xunitCollectionParallelismCausesContention = $false
    xunitCollectionParallelismEvidence = "Assembly-level CollectionBehavior already disables test collection parallelization."
    touchedFixedArtifactPathsDuringRun = $touchedFixedRoots
    shards = $shardResults
}
Write-Json "complete-suite-result.json" $summary
$summary | ConvertTo-Json -Depth 20
if (-not $passed) { exit 2 }
Write-Host "COMPLETE_TEST_SUITE_GREEN"
