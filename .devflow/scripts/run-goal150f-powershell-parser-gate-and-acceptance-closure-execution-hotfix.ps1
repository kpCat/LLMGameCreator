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
$rawRoot = Join-Path $RepoRoot (Join-Path $RunRoot ('goal150f-' + $stamp))
$taskRoot = Join-Path $RepoRoot 'docs/agent-tasks/goal-150f-powershell-parser-gate-and-acceptance-closure-execution-hotfix'
$proceduralRoot = Join-Path $RepoRoot '.llmgc/procedural/goal-150f-powershell-parser-gate-and-acceptance-closure-execution-hotfix'
$exportRoot = Join-Path $RepoRoot '.llmgc/exports/goal-150f-powershell-parser-gate-and-acceptance-closure-execution-hotfix'
New-Item -ItemType Directory -Force -Path $rawRoot | Out-Null

function Write-Json([string]$Path, $Value) {
    [IO.File]::WriteAllText($Path, (($Value | ConvertTo-Json -Depth 40) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
}

function Test-PowerShellParse([string[]]$Paths) {
    $results = foreach ($path in $Paths) {
        $tokens = $null
        $errors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($path, [ref]$tokens, [ref]$errors) | Out-Null
        [pscustomobject]@{
            path = $path.Substring($RepoRoot.Length + 1).Replace('\', '/')
            passed = @($errors).Count -eq 0
            errors = @($errors | ForEach-Object { [ordered]@{ line = $_.Extent.StartLineNumber; column = $_.Extent.StartColumnNumber; message = $_.Message } })
        }
    }
    if (@($results | Where-Object { -not $_.passed }).Count -ne 0) { throw ('PowerShell parse gate failed: ' + (($results | Where-Object { -not $_.passed } | ConvertTo-Json -Depth 8 -Compress))) }
    return @($results)
}

function Get-SourceManifest {
    $roots = @('.devflow/scripts', '.devflow/artifact-scope', 'tests', 'src', 'catalogs')
    $lines = foreach ($root in $roots) {
        $fullRoot = Join-Path $RepoRoot $root
        if (Test-Path -LiteralPath $fullRoot) {
            Get-ChildItem -LiteralPath $fullRoot -Recurse -File | Sort-Object FullName | ForEach-Object {
                $relative = $_.FullName.Substring($RepoRoot.Length + 1).Replace('\', '/')
                "$relative $((Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant())"
            }
        }
    }
    $payload = ($lines -join "`n") + "`n"
    $bytes = [Text.UTF8Encoding]::new($false).GetBytes($payload)
    $algorithm = [Security.Cryptography.SHA256]::Create()
    try {
        $sha = $algorithm.ComputeHash($bytes)
    }
    finally {
        $algorithm.Dispose()
    }
    return [ordered]@{ sha256 = (-join ($sha | ForEach-Object { $_.ToString('x2') })); fileCount = @($lines).Count; lines = @($lines) }
}

$changedPs1 = @(
    (Join-Path $RepoRoot '.devflow/scripts/run-complete-test-suite.ps1'),
    (Join-Path $RepoRoot '.devflow/scripts/check-artifact-scope.ps1'),
    (Join-Path $RepoRoot '.devflow/scripts/run-goal150e-historical-test-identity-reconciliation-and-manual-gate-readiness-hotfix.ps1'),
    $ScriptPath
)
$parserResults = Test-PowerShellParse -Paths $changedPs1
Write-Json (Join-Path $rawRoot 'powershell-parser-proof.json') ([ordered]@{ status = 'GREEN'; scripts = $parserResults })

& dotnet test (Join-Path $RepoRoot 'tests/LLMGameCreator.Tests/LLMGameCreator.Tests.csproj') -c Debug --filter 'FullyQualifiedName~Goal150F'
if ($LASTEXITCODE -ne 0) { throw 'Goal150F contract tests failed.' }

& (Join-Path $RepoRoot '.devflow/scripts/run-goal150e-historical-test-identity-reconciliation-and-manual-gate-readiness-hotfix.ps1') -RunRoot $RunRoot -PreflightOnly
if ($LASTEXITCODE -ne 0) { throw 'Goal150E reconciliation preflight failed.' }
$preflightRoot = Get-ChildItem -LiteralPath (Join-Path $RepoRoot $RunRoot) -Directory | Where-Object Name -like 'goal150e-*' | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
$manifestPath = Join-Path $preflightRoot.FullName 'historical-identity-reconciliation-map.json'
$preflightSummary = Get-Content -LiteralPath (Join-Path $preflightRoot.FullName 'historical-identity-reconciliation-summary.json') -Raw -Encoding UTF8 | ConvertFrom-Json

$planRoot = Join-Path $rawRoot 'plan-only'
& (Join-Path $RepoRoot '.devflow/scripts/run-complete-test-suite.ps1') -Mode Goal150AcceptanceClosure -ReconciliationManifestPath $manifestPath -OutputRoot $planRoot -MaximumWallClockMinutes 20 -HeavyTestTimeoutSeconds 480 -PlanOnly
if ($LASTEXITCODE -ne 0) { throw 'Goal150F closure PlanOnly failed.' }
$plan = Get-Content -LiteralPath (Join-Path $planRoot 'validation-discovery-summary.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if ($preflightSummary.historicalIdentityCount -ne 85 -or $preflightSummary.resolvedHistoricalIdentityCount -ne 85 -or $preflightSummary.unresolvedHistoricalIdentityCount -ne 0 -or $preflightSummary.ambiguousHistoricalIdentityCount -ne 0 -or $plan.discovered -ne 85) { throw 'Goal150F PlanOnly reconciliation counts are not GREEN.' }

$smokeManifestPath = Join-Path $rawRoot 'one-test-reconciliation-manifest.json'
$smokeTest = 'LLMGameCreator.Tests.Devflow.RunGoal150EHistoricalIdentityReconciliationTests.Goal150E_runner_declares_exact_identity_reconciliation_and_current_case_accounting'
Write-Json $smokeManifestPath ([ordered]@{ historicalIdentityCount = 1; resolvedHistoricalIdentityCount = 1; currentExecutionCaseCount = 1; entries = @([ordered]@{ historicalIdentity = $smokeTest; currentExecutionIdentities = @($smokeTest) }) })
$smokeRoot = Join-Path $rawRoot 'one-test-closure-smoke'
& (Join-Path $RepoRoot '.devflow/scripts/run-complete-test-suite.ps1') -Mode Goal150AcceptanceClosure -ReconciliationManifestPath $smokeManifestPath -OutputRoot $smokeRoot -MaximumWallClockMinutes 20 -HeavyTestTimeoutSeconds 480
if ($LASTEXITCODE -ne 0) { throw 'Goal150F one-test closure smoke failed.' }
$smoke = Get-Content -LiteralPath (Join-Path $smokeRoot 'validation-result.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if ($smoke.counts.attemptedExecutionCaseCount -ne 1 -or $smoke.counts.executedCaseCount -ne 1 -or $smoke.counts.passedCaseCount -ne 1 -or $smoke.counts.failedCaseCount -ne 0 -or $smoke.counts.notRunCaseCount -ne 0 -or $smoke.counts.timedOutCaseCount -ne 0 -or $smoke.counts.missingResultCount -ne 0 -or $smoke.counts.duplicateResultCount -ne 0) { throw 'Goal150F one-test closure smoke accounting is not GREEN.' }
Write-Json (Join-Path $rawRoot 'closure-smoke-proof.json') $smoke

& (Join-Path $RepoRoot '.devflow/scripts/check-artifact-scope.ps1') -Scenario 'goal-150f-powershell-parser-gate-and-acceptance-closure-execution-hotfix' -BaselineRef 'a952e2918601804c47eafaf7a53f880f9aadac49' -ReportDirectory $rawRoot
if ($LASTEXITCODE -ne 0) { throw 'Goal150F artifact-scope baseline smoke failed.' }

$validatedManifest = Get-SourceManifest
Write-Json (Join-Path $rawRoot 'validated-source-manifest.json') $validatedManifest
if ($PreflightOnly) { Write-Host 'GOAL150F_PREPUBLICATION_GREEN'; exit 0 }

$closureRoot = Join-Path $rawRoot 'closure'
& (Join-Path $RepoRoot '.devflow/scripts/run-complete-test-suite.ps1') -Mode Goal150AcceptanceClosure -ReconciliationManifestPath $manifestPath -OutputRoot $closureRoot -MaximumWallClockMinutes 20 -HeavyTestTimeoutSeconds 480
if ($LASTEXITCODE -ne 0) { throw 'Goal150F acceptance closure failed.' }
$closure = Get-Content -LiteralPath (Join-Path $closureRoot 'validation-result.json') -Raw -Encoding UTF8 | ConvertFrom-Json
Write-Json (Join-Path $rawRoot 'closure-result.json') $closure
New-Item -ItemType Directory -Force -Path $proceduralRoot, $exportRoot | Out-Null
foreach ($root in @($proceduralRoot, $exportRoot)) {
    Copy-Item -LiteralPath (Join-Path $rawRoot 'powershell-parser-proof.json') -Destination (Join-Path $root 'powershell-parser-proof.json') -Force
    Copy-Item -LiteralPath (Join-Path $rawRoot 'closure-smoke-proof.json') -Destination (Join-Path $root 'closure-smoke-proof.json') -Force
    Copy-Item -LiteralPath (Join-Path $rawRoot 'closure-result.json') -Destination (Join-Path $root 'closure-result.json') -Force
    Copy-Item -LiteralPath (Join-Path $rawRoot 'goal-150f-powershell-parser-gate-and-acceptance-closure-execution-hotfix-artifact-scope-report.json') -Destination (Join-Path $root 'artifact-scope-proof.json') -Force
}
Write-Host 'GOAL150F_ACCEPTANCE_CLOSURE_GREEN'
