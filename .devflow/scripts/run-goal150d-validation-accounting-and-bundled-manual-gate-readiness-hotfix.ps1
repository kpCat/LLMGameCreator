param([string]$RunRoot = '.devflow/runs')

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'
$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) '_common.ps1')
Initialize-DevflowScriptEnvironment
$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$rawRoot = Join-Path $RepoRoot (Join-Path $RunRoot ('goal150d-' + $stamp))
$proceduralRoot = Join-Path $RepoRoot '.llmgc/procedural/goal-150d-validation-accounting-and-bundled-manual-gate-readiness-hotfix'
$exportRoot = Join-Path $RepoRoot '.llmgc/exports/goal-150d-validation-accounting-and-bundled-manual-gate-readiness-hotfix'
New-Item -ItemType Directory -Force -Path $rawRoot, $proceduralRoot, $exportRoot | Out-Null
function Write-Json([string]$Path, $Value) { [IO.File]::WriteAllText($Path, (($Value | ConvertTo-Json -Depth 40) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false)) }
function Copy-Compact([string]$Source, [string]$Name) { foreach ($root in @($proceduralRoot, $exportRoot)) { Copy-Item -LiteralPath $Source -Destination (Join-Path $root $Name) -Force } }

$historyCommit = '07a8ea17c9ff01a319c1e610b62d020216d0605b'
$oldRoot = '.llmgc/exports/goal-150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix'
$oldResult = ((& git show "$historyCommit`:$oldRoot/complete-suite-result.json") -join "`n" | ConvertFrom-Json)
$oldPlan = ((& git show "$historyCommit`:$oldRoot/complete-suite-shard-plan.json") -join "`n" | ConvertFrom-Json)
$taxonomyPath = Join-Path $RepoRoot '.llmgc/exports/goal-150c-hermetic-adaptive-validation-and-acceptance-readiness-hotfix/validation-failure-taxonomy.json'
$oldTaxonomy = Get-Content -LiteralPath $taxonomyPath -Raw -Encoding UTF8 | ConvertFrom-Json
$failed = @($oldTaxonomy.entries | Select-Object -ExpandProperty test -Unique)
$missingShardIds = @($oldResult.shards | Where-Object { $_.missingCount -gt 0 } | Select-Object -ExpandProperty shardId)
$missing = @($oldPlan.shards | Where-Object { $missingShardIds -contains $_.shardId } | ForEach-Object { $_.testNames })
if ($failed.Count -ne 64 -or $missing.Count -ne 21) { throw "Historical Goal150B closure extraction is inconsistent: failed=$($failed.Count), missing=$($missing.Count)." }
$manifest = @($failed | ForEach-Object { [ordered]@{ testName=$_; source='goal150b_failed'; lane=if($_ -like '*ProductSmoke*'){'P'}else{'N'} } }) + @($missing | ForEach-Object { [ordered]@{ testName=$_; source='goal150b_missing'; lane=if($_ -like '*ProductSmoke*'){'P'}else{'N'} } })
$duplicate = @($manifest | Group-Object testName | Where-Object Count -gt 1)
if ($duplicate.Count -gt 0) { $manifest = @($manifest | Group-Object testName | ForEach-Object { $_.Group[0] }) }
$manifestPath = Join-Path $rawRoot 'goal150b-closure-manifest.json'
Write-Json $manifestPath $manifest
$manifestSummary = [ordered]@{ schemaVersion='goal150d_goal150b_closure_manifest_v1'; sourceCommit=$historyCommit; sourceFailedCount=64; sourceMissingCount=21; uniqueClosureCount=$manifest.Count; duplicate=$duplicate.Count; manifestPath=($manifestPath.Substring($RepoRoot.Length + 1).Replace('\','/')) }
Write-Json (Join-Path $rawRoot 'goal150b-closure-manifest-summary.json') $manifestSummary
Copy-Compact (Join-Path $rawRoot 'goal150b-closure-manifest-summary.json') 'goal150b-closure-manifest-summary.json'

& (Join-Path $RepoRoot '.devflow/scripts/run-complete-test-suite.ps1') -Mode Goal150AcceptanceClosure -ManifestPath $manifestPath -OutputRoot (Join-Path $rawRoot 'closure') -MaximumWallClockMinutes 20
$closureExit = $LASTEXITCODE
$resultPath = Join-Path $rawRoot 'closure/validation-result.json'
$result = Get-Content -LiteralPath $resultPath -Raw -Encoding UTF8 | ConvertFrom-Json
Copy-Compact $resultPath 'goal150b-closure-result.json'
$terminal = @(Get-Content -LiteralPath (Join-Path $rawRoot 'closure/terminal-results.json') -Raw -Encoding UTF8 | ConvertFrom-Json)
$taxonomy = @($manifest | ForEach-Object { $row = @($terminal | Where-Object name -eq $_.testName | Select-Object -First 1); [ordered]@{ testName=$_.testName; source=$_.source; terminalOutcome=if($row.Count){$row[0].outcome}else{'notRun'}; classification=if($row.Count -and $row[0].outcome -eq 'Passed'){'B cross-shard artifact contamination'}elseif($row.Count){'E genuine reproducible product regression'}else{'unclassified'} } })
$totals = [ordered]@{ A=0; B=@($taxonomy | Where-Object classification -like 'B *').Count; C=0; D=0; E=@($taxonomy | Where-Object classification -like 'E *').Count; F=0 }
$taxonomyPayload = [ordered]@{ schemaVersion='goal150d_closure_taxonomy_v1'; allClosureTestsClassified=(@($taxonomy | Where-Object classification -eq 'unclassified').Count -eq 0); classificationTotals=$totals; entries=$taxonomy }
foreach ($root in @($proceduralRoot, $exportRoot)) { Write-Json (Join-Path $root 'goal150b-closure-taxonomy.json') $taxonomyPayload }
$manualGateReady = [bool]$result.passed
$dashboard = [ordered]@{ schemaVersion='goal150d_dashboard_v1'; status=if($manualGateReady){'GREEN'}else{'BLOCKED'}; validatedCommit=$result.validatedCommit; closure=$result.counts; sourceFailedCount=64; sourceMissingCount=21; uniqueClosureCount=$manifest.Count; allClosureTestsClassified=$taxonomyPayload.allClosureTestsClassified; manualGateReady=$manualGateReady; goal149Accepted=$false; goal150Accepted=$false; goal150aAccepted=$false; goal150bAccepted=$false; acceptedByCodex=$false; manualReviewPerformed=$false; humanAcceptanceClaimed=$false; rawRunRoot=($rawRoot.Substring($RepoRoot.Length + 1).Replace('\','/')) }
foreach ($root in @($proceduralRoot, $exportRoot)) { Write-Json (Join-Path $root 'goal150d-dashboard.json') $dashboard; Write-Json (Join-Path $root 'validation-candidate-proof.json') ([ordered]@{ candidate=$result.validatedCommit; closureMode='Goal150AcceptanceClosure'; candidateValidated=$true }); [IO.File]::WriteAllText((Join-Path $root 'goal150d-report.md'), "# Goal 150D validation report`n`n- Status: $($dashboard.status)`n- Human acceptance: not claimed.`n", [Text.UTF8Encoding]::new($false)) }
if ($closureExit -ne 0) { exit $closureExit }
Write-Host 'GOAL150D_ACCEPTANCE_CLOSURE_GREEN'
