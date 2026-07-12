param(
    [string]$RunRoot = '.devflow/runs',
    [switch]$PreflightOnly
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'
$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) '_common.ps1')
Initialize-DevflowScriptEnvironment
$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$rawRoot = Join-Path $RepoRoot (Join-Path $RunRoot ('goal150e-' + $stamp))
$taskRoot = Join-Path $RepoRoot 'docs/agent-tasks/goal-150e-historical-test-identity-reconciliation-and-manual-gate-readiness-hotfix'
$aliasPath = Join-Path $taskRoot 'historical-identity-aliases.json'
$proceduralRoot = Join-Path $RepoRoot '.llmgc/procedural/goal-150e-historical-test-identity-reconciliation-and-manual-gate-readiness-hotfix'
$exportRoot = Join-Path $RepoRoot '.llmgc/exports/goal-150e-historical-test-identity-reconciliation-and-manual-gate-readiness-hotfix'
New-Item -ItemType Directory -Force -Path $rawRoot | Out-Null

function Write-Json([string]$Path, $Value) { [IO.File]::WriteAllText($Path, (($Value | ConvertTo-Json -Depth 40) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false)) }
function Copy-Compact([string]$Source, [string]$Name) { foreach ($root in @($proceduralRoot, $exportRoot)) { Copy-Item -LiteralPath $Source -Destination (Join-Path $root $Name) -Force } }
function Get-IdentityParts([string]$Identity) {
    $withoutDisplay = $Identity.Split('(')[0]
    $lastDot = $withoutDisplay.LastIndexOf('.')
    if ($lastDot -lt 1) { throw "Invalid discovered test identity '$Identity'." }
    [pscustomobject]@{ fullIdentity = $Identity; className = $withoutDisplay.Substring(0, $lastDot); methodName = $withoutDisplay.Substring($lastDot + 1); displaySuffix = if ($Identity.Length -gt $withoutDisplay.Length) { $Identity.Substring($withoutDisplay.Length) } else { '' } }
}

$historyCommit = '07a8ea17c9ff01a319c1e610b62d020216d0605b'
$taxonomyCommit = (& git -C $RepoRoot rev-parse HEAD).Trim()
$oldRoot = '.llmgc/exports/goal-150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix'
$taxonomyPath = '.llmgc/exports/goal-150c-hermetic-adaptive-validation-and-acceptance-readiness-hotfix/validation-failure-taxonomy.json'
$oldResult = ((& git -C $RepoRoot show "$historyCommit`:$oldRoot/complete-suite-result.json") -join "`n" | ConvertFrom-Json)
$oldPlan = ((& git -C $RepoRoot show "$historyCommit`:$oldRoot/complete-suite-shard-plan.json") -join "`n" | ConvertFrom-Json)
$oldTaxonomy = ((& git -C $RepoRoot show "$taxonomyCommit`:$taxonomyPath") -join "`n" | ConvertFrom-Json)
$failed = @($oldTaxonomy.entries | Select-Object -ExpandProperty test -Unique)
$missingShardIds = @($oldResult.shards | Where-Object { $_.missingCount -gt 0 } | Select-Object -ExpandProperty shardId)
$missing = @($oldPlan.shards | Where-Object { $missingShardIds -contains $_.shardId } | ForEach-Object { $_.testNames })
if ($failed.Count -ne 64 -or $missing.Count -ne 21) { throw "Historical Goal150B extraction is inconsistent: failed=$($failed.Count), missing=$($missing.Count)." }
$historical = @($failed | ForEach-Object { [pscustomobject]@{ identity = [string]$_; source = 'goal150b_failed' } }) + @($missing | ForEach-Object { [pscustomobject]@{ identity = [string]$_; source = 'goal150b_missing' } })
$duplicates = @($historical | Group-Object identity | Where-Object Count -gt 1)
$historical = @($historical | Group-Object identity | ForEach-Object { $_.Group[0] } | Sort-Object identity)
if ($historical.Count -ne 85 -or $duplicates.Count -ne 0) { throw "Historical Goal150B source set must contain exactly 85 unique identities; actual=$($historical.Count), duplicates=$($duplicates.Count)." }

$discoveryLog = Join-Path $rawRoot 'current-discovery.log'
& dotnet test (Join-Path $RepoRoot 'tests/LLMGameCreator.Tests/LLMGameCreator.Tests.csproj') -c Debug --no-build --list-tests 1> $discoveryLog 2> (Join-Path $rawRoot 'current-discovery.stderr.log')
if ($LASTEXITCODE -ne 0) { throw 'Current test discovery failed.' }
$current = @(Get-Content -LiteralPath $discoveryLog -Encoding UTF8 | ForEach-Object { $_.Trim() } | Where-Object { $_ -like 'LLMGameCreator.Tests.*' } | Sort-Object -Unique)
$currentByFull = @{}; $currentByMethod = @{}
foreach ($identity in $current) { $parts = Get-IdentityParts $identity; $currentByFull[$identity] = $parts; $key = "$($parts.className).$($parts.methodName)"; if (-not $currentByMethod.ContainsKey($key)) { $currentByMethod[$key] = @() }; $currentByMethod[$key] += $parts }
if (-not (Test-Path -LiteralPath $aliasPath -PathType Leaf)) { throw "Task-local identity alias manifest is missing: $aliasPath" }
$aliasDocument = Get-Content -LiteralPath $aliasPath -Raw -Encoding UTF8 | ConvertFrom-Json
$aliases = if ($aliasDocument.PSObject.Properties.Name -contains 'aliases') { @($aliasDocument.aliases) } else { @() }
$aliasByHistorical = @{}; foreach ($alias in $aliases) { if ($aliasByHistorical.ContainsKey($alias.historicalIdentity)) { throw "Duplicate explicit alias for $($alias.historicalIdentity)." }; $aliasByHistorical[$alias.historicalIdentity] = $alias }

$entries = foreach ($source in $historical) {
    $parts = Get-IdentityParts $source.identity
    $methodKey = "$($parts.className).$($parts.methodName)"
    $resolution = $null; $currentExecution = @(); $classification = 'B'; $rationale = ''
    if ($currentByFull.ContainsKey($source.identity)) { $resolution = 'exact'; $currentExecution = @($source.identity); $rationale = 'Historical full display identity is present in current discovery.' }
    elseif ($currentByMethod.ContainsKey($methodKey)) { $resolution = 'canonical_method'; $currentExecution = @($currentByMethod[$methodKey] | Select-Object -ExpandProperty fullIdentity | Sort-Object); $rationale = 'Historical display suffix changed; the current canonical Class.Method remains present.' }
    elseif ($aliasByHistorical.ContainsKey($source.identity)) {
        $alias = $aliasByHistorical[$source.identity]; $resolution = [string]$alias.resolution; $currentExecution = @($alias.currentExecutionIdentities | Sort-Object); $classification = if ($resolution -eq 'explicit_rename' -or $resolution -eq 'retired_with_replacement_coverage') { 'F' } else { throw "Unsupported alias resolution '$resolution'." }; $rationale = [string]$alias.rationale
        if ([string]::IsNullOrWhiteSpace($rationale) -or $currentExecution.Count -eq 0 -or @($currentExecution | Where-Object { -not $currentByFull.ContainsKey($_) }).Count -ne 0) { throw "Explicit alias for $($source.identity) lacks a rationale or current discovered replacement." }
    }
    [pscustomobject][ordered]@{ historicalIdentity=$source.identity; historicalClass=$parts.className; historicalMethod=$parts.methodName; historicalDisplaySuffix=$parts.displaySuffix; source=$source.source; resolution=$resolution; currentExecutionIdentities=$currentExecution; classification=$classification; rationale=$rationale }
}
$unresolved = @($entries | Where-Object { [string]::IsNullOrWhiteSpace($_.resolution) })
$ambiguous = @($entries | Where-Object { $_.resolution -eq 'canonical_method' -and $_.currentExecutionIdentities.Count -eq 0 })
$executionCases = @($entries | ForEach-Object { $_.currentExecutionIdentities } | Sort-Object -Unique)
$exactEntries = @($entries | Where-Object { $_.resolution -eq 'exact' })
$canonicalEntries = @($entries | Where-Object { $_.resolution -eq 'canonical_method' })
$renamedEntries = @($entries | Where-Object { $_.resolution -eq 'explicit_rename' })
$retiredEntries = @($entries | Where-Object { $_.resolution -eq 'retired_with_replacement_coverage' })
$summary = [ordered]@{ schemaVersion='goal150e_historical_identity_reconciliation_v1'; historyCommit=$historyCommit; taxonomyCommit=$taxonomyCommit; historicalSourceCount=$historical.Count; historicalIdentityCount=$entries.Count; resolvedHistoricalIdentityCount=($entries.Count - $unresolved.Count); exactCount=$exactEntries.Count; canonicalMethodCount=$canonicalEntries.Count; explicitRenameCount=$renamedEntries.Count; retiredWithCoverageCount=$retiredEntries.Count; unresolvedHistoricalIdentityCount=$unresolved.Count; ambiguousHistoricalIdentityCount=$ambiguous.Count; currentDiscoveryCount=$current.Count; currentExecutionCaseCount=$executionCases.Count; aliasesConsumed=@($aliases).Count }
$manifest = [ordered]@{ schemaVersion='goal150e_closure_execution_manifest_v1'; historicalIdentityCount=$entries.Count; resolvedHistoricalIdentityCount=($entries.Count - $unresolved.Count); currentExecutionCaseCount=$executionCases.Count; entries=$entries }
Write-Json (Join-Path $rawRoot 'historical-identity-reconciliation-summary.json') $summary
Write-Json (Join-Path $rawRoot 'historical-identity-reconciliation-map.json') $manifest
Write-Json (Join-Path $rawRoot 'preflight-report.json') ([ordered]@{ status=if($summary.unresolvedHistoricalIdentityCount -eq 0 -and $summary.ambiguousHistoricalIdentityCount -eq 0){'GREEN'}else{'BLOCKED'}; summary=$summary; unresolvedIdentities=@($unresolved | Select-Object -ExpandProperty historicalIdentity); ambiguousIdentities=@($ambiguous | Select-Object -ExpandProperty historicalIdentity); rawRunRoot=($rawRoot.Substring($RepoRoot.Length + 1).Replace('\','/')) })
if ($summary.historicalSourceCount -ne 85 -or $summary.unresolvedHistoricalIdentityCount -ne 0 -or $summary.ambiguousHistoricalIdentityCount -ne 0) { Write-Host 'GOAL150E_PREFLIGHT_BLOCKED'; exit 2 }
Write-Host 'GOAL150E_PREFLIGHT_GREEN'
if ($PreflightOnly) { exit 0 }

New-Item -ItemType Directory -Force -Path $proceduralRoot, $exportRoot | Out-Null
$closureRoot = Join-Path $rawRoot 'closure'
& (Join-Path $RepoRoot '.devflow/scripts/run-complete-test-suite.ps1') -Mode Goal150AcceptanceClosure -ReconciliationManifestPath (Join-Path $rawRoot 'historical-identity-reconciliation-map.json') -OutputRoot $closureRoot -MaximumWallClockMinutes 20 -HeavyTestTimeoutSeconds 480
$closureExit = $LASTEXITCODE
$closureResult = Get-Content -LiteralPath (Join-Path $closureRoot 'validation-result.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$terminal = @(Get-Content -LiteralPath (Join-Path $closureRoot 'terminal-results.json') -Raw -Encoding UTF8 | ConvertFrom-Json)
$classificationEntries = foreach ($entry in $entries) {
    $currentRows = @($entry.currentExecutionIdentities | ForEach-Object { $identity = $_; @($terminal | Where-Object name -eq $identity | Select-Object -First 1) })
    $classification = $entry.classification
    if ($classification -ne 'F') { $classification = if ($currentRows.Count -ne $entry.currentExecutionIdentities.Count) { 'unclassified' } elseif (@($currentRows | Where-Object outcome -eq 'Failed').Count -gt 0) { 'E' } elseif ($closureResult.counts.timedOutCaseCount -gt 0) { 'D' } elseif (@($currentRows | Where-Object outcome -ne 'Passed').Count -gt 0) { 'unclassified' } else { 'B' } }
    [ordered]@{ historicalIdentity=$entry.historicalIdentity; resolution=$entry.resolution; currentExecutionIdentities=$entry.currentExecutionIdentities; classification=$classification; rationale=$entry.rationale }
}
$classificationTotals = [ordered]@{ A=@($classificationEntries | Where-Object classification -eq 'A').Count; B=@($classificationEntries | Where-Object classification -eq 'B').Count; C=@($classificationEntries | Where-Object classification -eq 'C').Count; D=@($classificationEntries | Where-Object classification -eq 'D').Count; E=@($classificationEntries | Where-Object classification -eq 'E').Count; F=@($classificationEntries | Where-Object classification -eq 'F').Count }
$classificationPayload = [ordered]@{ schemaVersion='goal150e_closure_classification_v1'; allHistoricalIdentitiesClassified=(@($classificationEntries | Where-Object classification -eq 'unclassified').Count -eq 0); classificationTotals=$classificationTotals; entries=$classificationEntries }
$dashboard = [ordered]@{ schemaVersion='goal150e_dashboard_v1'; status=if($closureExit -eq 0 -and $classificationPayload.allHistoricalIdentitiesClassified){'GREEN'}else{'BLOCKED'}; validatedCommit=$closureResult.validatedCommit; reconciliation=$summary; closure=$closureResult.counts; allHistoricalIdentitiesClassified=$classificationPayload.allHistoricalIdentitiesClassified; manualGateReady=($closureExit -eq 0 -and $classificationPayload.allHistoricalIdentitiesClassified); goal149Accepted=$false; goal150Accepted=$false; goal150aAccepted=$false; goal150bAccepted=$false; acceptedByCodex=$false; manualReviewPerformed=$false; humanAcceptanceClaimed=$false; rawRunRoot=($rawRoot.Substring($RepoRoot.Length + 1).Replace('\','/')) }
Write-Json (Join-Path $rawRoot 'closure-execution-result.json') $closureResult
Write-Json (Join-Path $rawRoot 'closure-classification-summary.json') $classificationPayload
foreach ($pair in @(@{ source=(Join-Path $rawRoot 'historical-identity-reconciliation-summary.json'); name='historical-identity-reconciliation-summary.json' }, @{ source=(Join-Path $rawRoot 'historical-identity-reconciliation-map.json'); name='historical-identity-reconciliation-map.json' }, @{ source=(Join-Path $rawRoot 'closure-execution-result.json'); name='closure-execution-result.json' }, @{ source=(Join-Path $rawRoot 'closure-classification-summary.json'); name='closure-classification-summary.json' })) { Copy-Compact $pair.source $pair.name }
foreach ($root in @($proceduralRoot, $exportRoot)) { Write-Json (Join-Path $root 'goal150e-dashboard.json') $dashboard; [IO.File]::WriteAllText((Join-Path $root 'goal150e-report.md'), "# Goal 150E reconciliation report`n`n- Status: $($dashboard.status)`n- Human acceptance: not claimed.`n", [Text.UTF8Encoding]::new($false)) }
if ($closureExit -ne 0) { exit $closureExit }
Write-Host 'GOAL150E_ACCEPTANCE_CLOSURE_GREEN'
