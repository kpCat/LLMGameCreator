param(
    [string]$OutputRoot = ".devflow/runs/goal150c-complete-test-suite",
    [int]$ShardTimeoutSeconds = 300,
    [int]$HeavyTestTimeoutSeconds = 480,
    [int]$ClassesPerShard = 16,
    [int]$MaximumWallClockMinutes = 35,
    [switch]$PlanOnly,
    [ValidateSet('FullSuite', 'Goal150AcceptanceClosure')]
    [string]$Mode = 'FullSuite',
    [string]$ManifestPath = '',
    [string]$ReconciliationManifestPath = ''
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment
$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$ProjectRelativePath = "tests/LLMGameCreator.Tests/LLMGameCreator.Tests.csproj"
$Project = Join-Path $RepoRoot $ProjectRelativePath
$ResolvedOutput = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($OutputRoot)) { $OutputRoot } else { Join-Path $RepoRoot $OutputRoot }))

if ($ShardTimeoutSeconds -lt 1 -or $ShardTimeoutSeconds -gt 300) { throw "Shard timeout must be between 1 and 300 seconds." }
if ($HeavyTestTimeoutSeconds -lt $ShardTimeoutSeconds -or $HeavyTestTimeoutSeconds -gt 480) { throw "Heavy-test timeout must be between shard timeout and 480 seconds." }
if ($ClassesPerShard -lt 2) { throw "ClassesPerShard must be at least two; initial shards must not be one-class shards." }
if ($MaximumWallClockMinutes -lt 1 -or $MaximumWallClockMinutes -gt 35) { throw "MaximumWallClockMinutes must be between 1 and 35." }
if (Test-Path -LiteralPath $ResolvedOutput) { Remove-Item -LiteralPath $ResolvedOutput -Recurse -Force }
New-Item -ItemType Directory -Force -Path $ResolvedOutput, (Join-Path $ResolvedOutput "logs"), (Join-Path $ResolvedOutput "trx"), (Join-Path $ResolvedOutput "test-results"), (Join-Path $ResolvedOutput "temp") | Out-Null

function Write-Json([string]$Name, $Value, [int]$Depth = 32) {
    [IO.File]::WriteAllText((Join-Path $ResolvedOutput $Name), (($Value | ConvertTo-Json -Depth $Depth) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
}

function Get-RelativeOutputPath([string]$Path) {
    $full = [IO.Path]::GetFullPath($Path)
    $base = $ResolvedOutput.TrimEnd('\', '/')
    if (-not $full.StartsWith($base + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw "Path is outside output root: $Path" }
    return $full.Substring($base.Length + 1).Replace('\', '/')
}

function Read-Trx([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return @() }
    [xml]$xml = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    return @($xml.SelectNodes("//*[local-name()='UnitTestResult']") | ForEach-Object {
        $duration = [TimeSpan]::Zero
        if ($_.duration) { [TimeSpan]::TryParse([string]$_.duration, [ref]$duration) | Out-Null }
        [pscustomobject]@{ name = [string]$_.testName; outcome = [string]$_.outcome; durationSeconds = [Math]::Round($duration.TotalSeconds, 6) }
    })
}

function Get-ClassName([string]$TestName) {
    $name = $TestName
    $parameterIndex = $name.IndexOf('(')
    if ($parameterIndex -ge 0) { $name = $name.Substring(0, $parameterIndex) }
    $lastDot = $name.LastIndexOf('.')
    if ($lastDot -lt 1) { throw "Unable to determine class name for '$TestName'." }
    return $name.Substring(0, $lastDot)
}

function Get-MethodName([string]$TestName) {
    $name = $TestName
    $parameterIndex = $name.IndexOf('(')
    if ($parameterIndex -ge 0) { $name = $name.Substring(0, $parameterIndex) }
    return $name.Substring($name.LastIndexOf('.') + 1)
}

function New-DisposableWorktree {
    $parent = Join-Path ([IO.Path]::GetTempPath()) ("llmgc-goal150c-" + [guid]::NewGuid().ToString("N"))
    $worktree = Join-Path $parent "snapshot"
    New-Item -ItemType Directory -Force -Path $parent | Out-Null
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        & git -C $RepoRoot worktree add --detach $worktree HEAD 1> (Join-Path $ResolvedOutput "logs/worktree-add.stdout.log") 2> (Join-Path $ResolvedOutput "logs/worktree-add.stderr.log")
        $exitCode = $LASTEXITCODE
    }
    finally { $ErrorActionPreference = $previousErrorActionPreference }
    if ($exitCode -ne 0) { throw "Unable to create disposable exact-HEAD validation worktree." }
    return [pscustomobject]@{ parent = $parent; path = $worktree; head = (& git -C $worktree rev-parse HEAD).Trim() }
}

function Remove-DisposableWorktree($Snapshot) {
    if ($null -eq $Snapshot) { return }
    try { & git -C $RepoRoot worktree remove --force $Snapshot.path *> $null } catch { }
    if (Test-Path -LiteralPath $Snapshot.parent) { Remove-Item -LiteralPath $Snapshot.parent -Recurse -Force }
}

function Reset-DisposableWorktree($Snapshot, [string]$ShardId) {
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try { & git -C $Snapshot.path reset --hard $Snapshot.head *> $null; $exitCode = $LASTEXITCODE }
    finally { $ErrorActionPreference = $previousErrorActionPreference }
    if ($exitCode -ne 0) { throw "Unable to reset disposable worktree before $ShardId." }
    # Historical .llmgc baselines are tracked candidate inputs.  Disposable
    # ProductSmoke output belongs only under the external shard environment.
}

function Initialize-ShardEnvironment($Snapshot, [string]$ShardId) {
    $root = Join-Path $ResolvedOutput ("temp/" + $ShardId)
    $projectRoot = Join-Path $root "project-root"
    $packageRoot = Join-Path $root "package-output"
    $testRoot = Join-Path $ResolvedOutput ("test-results/" + $ShardId)
    $tempRoot = Join-Path $root "process-temp"
    New-Item -ItemType Directory -Force -Path $projectRoot, $packageRoot, $testRoot, $tempRoot | Out-Null
    $baseline = Join-Path $Snapshot.path ".llmgc/procedural"
    if (Test-Path -LiteralPath $baseline) {
        New-Item -ItemType Directory -Force -Path (Join-Path $projectRoot ".llmgc") | Out-Null
        Copy-Item -LiteralPath $baseline -Destination (Join-Path $projectRoot ".llmgc") -Recurse -Force
    }
    return [ordered]@{
        LLMGC_PRODUCT_SMOKE_PROJECT_DIR = $projectRoot
        LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR = $packageRoot
        TEMP = $tempRoot
        TMP = $tempRoot
        shardProjectRoot = $projectRoot
        shardPackageRoot = $packageRoot
        shardTestResultsRoot = $testRoot
        shardTempRoot = $tempRoot
    }
}

function Invoke-Dotnet([string]$Name, [string[]]$Arguments, [int]$TimeoutSeconds, [string]$WorkingDirectory, [hashtable]$Environment) {
    $stdout = Join-Path $ResolvedOutput ("logs/" + $Name + ".stdout.log")
    $stderr = Join-Path $ResolvedOutput ("logs/" + $Name + ".stderr.log")
    $line = ($Arguments | ForEach-Object { if ($_ -match '[\s"]') { '"' + ($_ -replace '"', '\"') + '"' } else { $_ } }) -join ' '
    $previous = @{}
    foreach ($key in $Environment.Keys) { $previous[$key] = [Environment]::GetEnvironmentVariable($key, 'Process'); [Environment]::SetEnvironmentVariable($key, [string]$Environment[$key], 'Process') }
    $started = [DateTime]::UtcNow
    try {
        $process = Start-Process -FilePath dotnet -ArgumentList $line -WorkingDirectory $WorkingDirectory -RedirectStandardOutput $stdout -RedirectStandardError $stderr -PassThru -WindowStyle Hidden
        $timedOut = $false
        while (-not $process.WaitForExit(1000)) {
            if (([DateTime]::UtcNow - $started).TotalSeconds -ge $TimeoutSeconds) { $timedOut = $true; & taskkill /PID $process.Id /T /F *> $null; $process.WaitForExit(); break }
        }
        $process.WaitForExit()
        $process.Refresh()
        $ended = [DateTime]::UtcNow
        $exitCode = -1
        if (-not $timedOut) {
            $exitCode = 0
            if ($process.ExitCode) { $exitCode = [int]$process.ExitCode }
        }
        return [ordered]@{ name = $Name; command = "dotnet " + $line; startedAtUtc = $started.ToString('O'); endedAtUtc = $ended.ToString('O'); durationSeconds = [Math]::Round(($ended - $started).TotalSeconds, 3); timeoutSeconds = $TimeoutSeconds; timedOut = $timedOut; exitCode = $exitCode; stdoutPath = Get-RelativeOutputPath $stdout; stderrPath = Get-RelativeOutputPath $stderr }
    }
    finally { foreach ($key in $Environment.Keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key], 'Process') } }
}

function Get-Filter([string[]]$Classes, [string[]]$ExactTests) {
    if ($ExactTests.Count -gt 0) {
        return (@($ExactTests | ForEach-Object { "FullyQualifiedName~$(Get-ClassName $_).$(Get-MethodName $_)" }) -join '|')
    }
    return (@($Classes | ForEach-Object { "FullyQualifiedName~$_" }) -join '|')
}

function Get-Lane([string]$ClassName) { if ($ClassName -like '*ProductSmoke*') { return 'P' } return 'N' }

$snapshot = $null
$runStarted = [DateTime]::UtcNow
$terminal = @{}
$attempts = [System.Collections.Generic.List[object]]::new()
$groups = @()
$manifestMissingCount = 0
$reconciliationSummary = $null
$attemptedExecutionIds = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$timedOutExecutionIds = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$missingResultExecutionIds = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$duplicateResultCount = 0
try {
    $snapshot = New-DisposableWorktree
    $baselineHead = $snapshot.head
    $mainHeadBefore = (& git -C $RepoRoot rev-parse HEAD).Trim()
    $mainStatusBefore = (& git -C $RepoRoot status --porcelain=v1) -join "`n"
    if ($baselineHead -ne $mainHeadBefore) { throw "Disposable snapshot does not match main HEAD." }

    $build = Invoke-Dotnet 'snapshot-build' @('build', 'LLMGameCreator.sln', '-c', 'Debug', '/p:EnableWindowsTargeting=true') 300 $snapshot.path @{}
    Write-Json 'snapshot-build.json' $build
    if ($build.exitCode -ne 0) { throw "Disposable snapshot build failed; see $($build.stdoutPath)." }
    $discoveryLog = Join-Path $ResolvedOutput 'logs/discovery.stdout.log'
    $discoveryError = Join-Path $ResolvedOutput 'logs/discovery.stderr.log'
    & dotnet test (Join-Path $snapshot.path $ProjectRelativePath) -c Debug --no-build --list-tests 1> $discoveryLog 2> $discoveryError
    if ($LASTEXITCODE -ne 0) { throw "Complete-suite discovery failed." }
    $discoveredTests = @(Get-Content -LiteralPath $discoveryLog -Encoding UTF8 | ForEach-Object { $_.Trim() } | Where-Object { $_ -like 'LLMGameCreator.Tests.*' } | Sort-Object -Unique)
    $tests = $discoveredTests
    $manifestEntries = @()
    if ($Mode -eq 'Goal150AcceptanceClosure') {
        $closureManifestPath = if (-not [string]::IsNullOrWhiteSpace($ReconciliationManifestPath)) { $ReconciliationManifestPath } else { $ManifestPath }
        if ([string]::IsNullOrWhiteSpace($closureManifestPath) -or -not (Test-Path -LiteralPath $closureManifestPath -PathType Leaf)) { throw 'Goal150AcceptanceClosure requires a JSON manifest path.' }
        $manifestDocument = Get-Content -LiteralPath $closureManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
        $manifestEntries = if ($manifestDocument.PSObject.Properties.Name -contains 'entries') { @($manifestDocument.entries) } else { @($manifestDocument) }
        if ($manifestEntries.Count -eq 0) { throw 'Goal150AcceptanceClosure manifest is empty.' }
        if ($manifestDocument.PSObject.Properties.Name -contains 'historicalIdentityCount') { $reconciliationSummary = $manifestDocument }
        $executionEntries = @($manifestEntries | ForEach-Object {
            if ($_.PSObject.Properties.Name -contains 'currentExecutionIdentities') {
                $_.currentExecutionIdentities | ForEach-Object { [pscustomobject]@{ testName = [string]$_; lane = if ([string]$_ -like '*ProductSmoke*') { 'P' } else { 'N' } }
            }
            else { [pscustomobject]@{ testName = [string]$_.testName; lane = [string]$_.lane } }
        })
        $duplicates = @($executionEntries | Group-Object testName | Where-Object Count -gt 1)
        $executionEntries = @($executionEntries | Group-Object testName | ForEach-Object { $_.Group[0] })
        $missingManifest = @($executionEntries | Where-Object { $discoveredTests -notcontains $_.testName })
        $manifestMissingCount = $missingManifest.Count
        if ($manifestMissingCount -ne 0) { throw "Goal150AcceptanceClosure manifest contains $manifestMissingCount current execution identities absent from discovery." }
        $tests = @($executionEntries | Select-Object -ExpandProperty testName)
        $manifestEntries = $executionEntries
    }
    if ($tests.Count -eq 0) { throw 'Complete-suite discovery returned zero tests.' }
    $inventory = @($tests | ForEach-Object { $entry = @($manifestEntries | Where-Object testName -eq $_ | Select-Object -First 1); [pscustomobject]@{ name = $_; className = Get-ClassName $_; lane = if ($entry.Count -gt 0) { $entry[0].lane } else { Get-Lane (Get-ClassName $_) } } })
    $duplicateDiscovery = @($tests | Group-Object | Where-Object Count -gt 1)
    $assignment = @{}
    foreach ($test in $inventory) { $assignment[$test.name] = 1 }
    Write-Json 'validation-discovery-summary.json' ([ordered]@{ schemaVersion = 'goal150c_discovery_v1'; snapshotHead = $baselineHead; discovered = $tests.Count; discoveredAtUtc = [DateTime]::UtcNow.ToString('O'); duplicateDiscovery = $duplicateDiscovery.Count; tests = $inventory })

    $lanePlan = @()
    if ($Mode -eq 'Goal150AcceptanceClosure') {
        foreach ($methodGroup in @($inventory | Group-Object { "$($_.className).$(Get-MethodName $_.name)" } | Sort-Object Name)) {
            $members = @($methodGroup.Group)
            $lanePlan += [pscustomobject]@{ id = "closure-{0:D3}" -f ($lanePlan.Count + 1); lane = $members[0].lane; classes = @($members[0].className); tests = @($members | Select-Object -ExpandProperty name); exactTests = @($members | Select-Object -ExpandProperty name); initial = $true }
        }
    }
    else { foreach ($lane in @('N', 'P')) {
        $classes = @($inventory | Where-Object lane -eq $lane | Select-Object -ExpandProperty className -Unique | Sort-Object)
        for ($offset = 0; $offset -lt $classes.Count; $offset += $ClassesPerShard) {
            $last = [Math]::Min($classes.Count - 1, $offset + $ClassesPerShard - 1)
            $members = @($classes[$offset..$last])
            $lanePlan += [pscustomobject]@{ id = "$lane-{0:D3}" -f ($lanePlan.Count + 1); lane = $lane; classes = $members; tests = @($inventory | Where-Object { $members -contains $_.className } | Select-Object -ExpandProperty name); initial = $true }
        }
    } }
    Write-Json 'validation-lane-plan.json' ([ordered]@{ schemaVersion = 'goal150c_lane_plan_v1'; partitionKind = 'deterministic_namespace_class_groups'; initialClassesPerShard = $ClassesPerShard; maximumSimultaneousTesthostProcesses = 1; lanes = @([ordered]@{ id='N'; filter='FullyQualifiedName!~ProductSmoke' }, [ordered]@{ id='P'; filter='FullyQualifiedName~ProductSmoke'; environment='unique project/package roots per shard' }); groups = $lanePlan })
    if ($PlanOnly) { Write-Host 'COMPLETE_TEST_SUITE_PLAN_READY'; return }

    function Invoke-Group($Group, [int]$Depth, [bool]$Retry) {
        if (([DateTime]::UtcNow - $runStarted).TotalMinutes -ge $MaximumWallClockMinutes) { return }
        $suffix = if ($Retry) { 'retry' } else { "d$Depth" }
        $attemptId = "$($Group.id)-$suffix-$($attempts.Count + 1)"
        $groupExactTests = @()
        if ($Group.PSObject.Properties.Name -contains 'exactTests') { $groupExactTests = @($Group.exactTests) }
        Reset-DisposableWorktree $snapshot $attemptId
        $environment = Initialize-ShardEnvironment $snapshot $attemptId
        $trx = Join-Path $ResolvedOutput ("trx/" + $attemptId + '.trx')
        $filter = Get-Filter @($Group.classes) $groupExactTests
        $run = Invoke-Dotnet $attemptId @('test', $ProjectRelativePath, '-c', 'Debug', '--no-build', '--filter', $filter, '--results-directory', $environment.shardTestResultsRoot, '--logger', "trx;LogFileName=$trx", '--logger', 'console;verbosity=minimal') ($(if ($groupExactTests.Count -eq 1) { $HeavyTestTimeoutSeconds } else { $ShardTimeoutSeconds })) $snapshot.path $environment
        $results = @(Read-Trx $trx)
        $byName = @{}; foreach ($row in $results) { if (-not $byName.ContainsKey($row.name)) { $byName[$row.name] = @() }; $byName[$row.name] += $row }
        $expected = @($Group.tests | Where-Object { -not $terminal.ContainsKey($_) })
        foreach ($name in $expected) { [void]$attemptedExecutionIds.Add($name) }
        $passedNow = @($expected | Where-Object { $byName.ContainsKey($_) -and $byName[$_][0].outcome -eq 'Passed' })
        foreach ($name in $passedNow) { $row=$byName[$name][0]; $terminal[$name] = [pscustomobject]@{ name=$name; outcome='Passed'; durationSeconds=$row.durationSeconds; lane=$Group.lane; groupId=$Group.id; reason='terminal_pass' } }
        if ($Mode -eq 'Goal150AcceptanceClosure') {
            foreach ($name in $expected) {
                if ($byName.ContainsKey($name)) {
                    if ($byName[$name].Count -gt 1) { $script:duplicateResultCount += ($byName[$name].Count - 1) }
                    $row = $byName[$name][0]
                    $terminal[$name] = [pscustomobject]@{ name=$name; outcome=$row.outcome; durationSeconds=$row.durationSeconds; lane=$Group.lane; groupId=$Group.id; reason='closure_terminal_result' }
                }
                elseif ($run.timedOut -and $expected.Count -eq 1) { [void]$timedOutExecutionIds.Add($name) }
                elseif (-not $run.timedOut) { [void]$missingResultExecutionIds.Add($name) }
            }
        }
        $pending = @($expected | Where-Object { -not $terminal.ContainsKey($_) })
        [void]$script:attempts.Add([pscustomobject]@{ id=$attemptId; lane=$Group.lane; groupId=$Group.id; depth=$Depth; retry=$Retry; classes=$Group.classes; expectedCount=$expected.Count; passedTerminalCount=$passedNow.Count; pendingCount=$pending.Count; run=$run; productSmokeProjectRoot=(Get-RelativeOutputPath $environment.shardProjectRoot); productSmokePackageRoot=(Get-RelativeOutputPath $environment.shardPackageRoot); trxPath=(Get-RelativeOutputPath $trx) })
        if ($pending.Count -eq 0 -or $Mode -eq 'Goal150AcceptanceClosure') { return }
        if (-not $Retry -and $groupExactTests.Count -ne 1) { Invoke-Group $Group $Depth $true; return }
        if ($groupExactTests.Count -eq 1) {
            $name = $groupExactTests[0]
            $row = if ($byName.ContainsKey($name)) { $byName[$name] } else { $null }
            # A single_test_timeout is never fabricated as a terminal result;
            # it remains in the launched-attempt record and contributes to timedOut.
            if ($null -ne $row -and -not $run.timedOut) { $terminal[$name] = [pscustomobject]@{ name=$name; outcome=$row.outcome; durationSeconds=$row.durationSeconds; lane=$Group.lane; groupId=$Group.id; reason='single_test_terminal' } }
            return
        }
        if ($Group.classes.Count -gt 1) {
            $middle = [Math]::Ceiling($Group.classes.Count / 2)
            foreach ($index in 0,1) {
                $slice = @($(if ($index -eq 0) { $Group.classes[0..($middle - 1)] } else { $Group.classes[$middle..($Group.classes.Count - 1)] }))
                if ($slice.Count -gt 0) {
                    $childTests = @($pending | Where-Object { $slice -contains (Get-ClassName $_) })
                    if ($childTests.Count -gt 0) { Invoke-Group ([pscustomobject]@{ id="$($Group.id)-$index"; lane=$Group.lane; classes=$slice; tests=$childTests; exactTests=@() }) ($Depth + 1) $false }
                }
            }
            return
        }
        foreach ($name in $pending) { Invoke-Group ([pscustomobject]@{ id="$($Group.id)-one"; lane=$Group.lane; classes=$Group.classes; tests=@($name); exactTests=@($name) }) ($Depth + 1) $false }
    }

    foreach ($group in $lanePlan) { Invoke-Group $group 0 $false }
    $mainHeadAfter = (& git -C $RepoRoot rev-parse HEAD).Trim()
    $mainStatusAfter = (& git -C $RepoRoot status --porcelain=v1) -join "`n"
    $timedOut = if ($Mode -eq 'Goal150AcceptanceClosure') { $timedOutExecutionIds.Count } else { @($attempts | Where-Object { $_.run.timedOut } | ForEach-Object { $_.classes } | ForEach-Object { $_ } | Select-Object -Unique).Count }
    $notRun = if ($Mode -eq 'Goal150AcceptanceClosure') { @($tests | Where-Object { -not $attemptedExecutionIds.Contains($_) }).Count } else { @($tests | Where-Object { -not $terminal.ContainsKey($_) }).Count - $timedOut }
    $missingResult = if ($Mode -eq 'Goal150AcceptanceClosure') { $missingResultExecutionIds.Count + $manifestMissingCount } else { $manifestMissingCount }
    $duplicate = if ($Mode -eq 'Goal150AcceptanceClosure') { $duplicateResultCount } else { 0 }
    $counts = [ordered]@{ discovered=$tests.Count; assigned=$inventory.Count; attempted=$attempts.Count; executed=$terminal.Count; passed=@($terminal.Values | Where-Object outcome -eq 'Passed').Count; failed=@($terminal.Values | Where-Object outcome -eq 'Failed').Count; skipped=@($terminal.Values | Where-Object outcome -eq 'NotExecuted').Count; notRun=[Math]::Max(0,$notRun); timedOut=$timedOut; missing=$missingResult; duplicate=$duplicate; historicalIdentityCount=if($null -ne $reconciliationSummary){[int]$reconciliationSummary.historicalIdentityCount}else{$null}; resolvedHistoricalIdentityCount=if($null -ne $reconciliationSummary){[int]$reconciliationSummary.resolvedHistoricalIdentityCount}else{$null}; currentExecutionCaseCount=$tests.Count; attemptedExecutionCaseCount=$attemptedExecutionIds.Count; executedCaseCount=$terminal.Count; passedCaseCount=@($terminal.Values | Where-Object outcome -eq 'Passed').Count; failedCaseCount=@($terminal.Values | Where-Object outcome -eq 'Failed').Count; skippedCaseCount=@($terminal.Values | Where-Object outcome -eq 'NotExecuted').Count; notRunCaseCount=[Math]::Max(0,$notRun); timedOutCaseCount=$timedOut; missingResultCount=$missingResult; duplicateResultCount=$duplicate }
    $passed = $counts.currentExecutionCaseCount -eq $counts.executedCaseCount -and $counts.executedCaseCount -eq ($counts.passedCaseCount + $counts.failedCaseCount + $counts.skippedCaseCount) -and $counts.currentExecutionCaseCount -eq ($counts.executedCaseCount + $counts.notRunCaseCount + $counts.timedOutCaseCount + $counts.missingResultCount) -and $counts.failedCaseCount -eq 0 -and $counts.notRunCaseCount -eq 0 -and $counts.timedOutCaseCount -eq 0 -and $counts.missingResultCount -eq 0 -and $counts.duplicateResultCount -eq 0
    Write-Json 'terminal-results.json' @($terminal.Values | Sort-Object name)
    Write-Json 'validation-slowest-summary.json' ([ordered]@{ schemaVersion='goal150c_slowest_v1'; slowestTerminalTests=@($terminal.Values | Sort-Object durationSeconds -Descending | Select-Object -First 20); slowestAttempts=@($attempts | Sort-Object { $_.run.durationSeconds } -Descending | Select-Object -First 20) })
    Write-Json 'validation-result.json' ([ordered]@{ schemaVersion='goal150e_validation_accounting_v1'; mode=$Mode; status=if($passed){'GREEN'}else{'BLOCKED'}; passed=$passed; validatedCommit=$baselineHead; validationSnapshotMatchesFinalSources=($baselineHead -eq $mainHeadAfter); hermeticSnapshot=$true; mainWorktreeUnchangedByValidation=($mainHeadBefore -eq $mainHeadAfter -and $mainStatusBefore -eq $mainStatusAfter); maximumSimultaneousTesthostProcesses=1; counts=$counts; lanes=@{ nonProductLanePassed=(@($terminal.Values | Where-Object { $_.lane -eq 'N' -and $_.outcome -ne 'Passed' }).Count -eq 0); productSmokeLanePassed=(@($terminal.Values | Where-Object { $_.lane -eq 'P' -and $_.outcome -ne 'Passed' }).Count -eq 0) }; attempts=$attempts.Count; rawOutputRoot=$ResolvedOutput })
    if (-not $passed) { exit 2 }
    Write-Host 'COMPLETE_TEST_SUITE_GREEN'
}
finally { Remove-DisposableWorktree $snapshot }
