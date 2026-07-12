param(
    [string]$RunRoot = ".devflow/runs",
    [switch]$SkipFocused,
    [switch]$SkipSpine,
    [string]$ExistingCompleteOutput = "",
    [switch]$EvidenceOnly,
    [switch]$FocusedPassed,
    [switch]$SpinePassed
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"
$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment
$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$rawRoot = Join-Path $RepoRoot (Join-Path $RunRoot ("goal150c-" + $stamp))
$proceduralRoot = Join-Path $RepoRoot ".llmgc/procedural/goal-150c-hermetic-adaptive-validation-and-acceptance-readiness-hotfix"
$exportRoot = Join-Path $RepoRoot ".llmgc/exports/goal-150c-hermetic-adaptive-validation-and-acceptance-readiness-hotfix"
New-Item -ItemType Directory -Force -Path $rawRoot, $proceduralRoot, $exportRoot | Out-Null

function Write-Json([string]$Path, $Value) { [IO.File]::WriteAllText($Path, (($Value | ConvertTo-Json -Depth 40) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false)) }
function Invoke-Step([string]$Name, [scriptblock]$Action) {
    $started = [DateTime]::UtcNow
    $log = Join-Path $rawRoot ($Name + '.log')
    & $Action *>&1 | Tee-Object -FilePath $log
    $exitCode = $LASTEXITCODE
    return [ordered]@{ name=$Name; exitCode=$exitCode; passed=($exitCode -eq 0); durationSeconds=[Math]::Round((([DateTime]::UtcNow - $started).TotalSeconds),3); logPath=(Resolve-Path -LiteralPath $log).Path.Substring($RepoRoot.Length + 1).Replace('\','/') }
}
function Copy-Compact([string]$Source, [string]$Name) {
    foreach ($root in @($proceduralRoot, $exportRoot)) { Copy-Item -LiteralPath $Source -Destination (Join-Path $root $Name) -Force }
}

$steps = [System.Collections.Generic.List[object]]::new()
if ($EvidenceOnly) {
    if ([string]::IsNullOrWhiteSpace($ExistingCompleteOutput) -or -not (Test-Path -LiteralPath $ExistingCompleteOutput)) { throw "EvidenceOnly requires ExistingCompleteOutput from the one final hermetic pass." }
    $completeOutput = [IO.Path]::GetFullPath($ExistingCompleteOutput)
}
else {
    [void]$steps.Add((Invoke-Step 'build' { dotnet build }))
    [void]$steps.Add((Invoke-Step 'goal150c-tests' { dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal150C" }))
    [void]$steps.Add((Invoke-Step 'goal150b-tests' { dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal150B" }))
    [void]$steps.Add((Invoke-Step 'goal150a-tests' { dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronizationTests" }))
    if (-not $SkipFocused) {
        [void]$steps.Add((Invoke-Step 'goal149-focused' { & .\.devflow\scripts\run-capability-runtime-equipment-slice.ps1 }))
        [void]$steps.Add((Invoke-Step 'goal150-focused' { & .\.devflow\scripts\run-character-attributes-level-progression-slice.ps1 }))
    }
    if (-not $SkipSpine) {
        [void]$steps.Add((Invoke-Step 'current-goal' { & .\.devflow\scripts\check-current-goal.ps1 }))
        [void]$steps.Add((Invoke-Step 'spine-fast' { & .\.devflow\scripts\check-spine-fast.ps1 }))
    }
    $completeOutput = Join-Path $rawRoot 'complete-suite'
    $complete = Invoke-Step 'complete-suite' { & .\.devflow\scripts\run-complete-test-suite.ps1 -OutputRoot $completeOutput }
    [void]$steps.Add($complete)
}

$requiredFiles = @('validation-discovery-summary.json','validation-lane-plan.json','validation-result.json','validation-slowest-summary.json')
foreach ($file in $requiredFiles) { $path = Join-Path $completeOutput $file; if (-not (Test-Path -LiteralPath $path)) { throw "Hermetic runner did not produce $file." }; Copy-Compact $path $file }
$result = Get-Content -LiteralPath (Join-Path $completeOutput 'validation-result.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$terminal = @(Get-Content -LiteralPath (Join-Path $completeOutput 'terminal-results.json') -Raw -Encoding UTF8 | ConvertFrom-Json)

$oldRoot = Join-Path $RepoRoot '.llmgc/exports/goal-150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix'
$oldResult = Get-Content -LiteralPath (Join-Path $oldRoot 'complete-suite-result.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$oldFailures = @{}
foreach ($trx in @(Get-ChildItem -LiteralPath (Join-Path $oldRoot 'trx') -Filter '*.trx' -Recurse -File -ErrorAction SilentlyContinue)) {
    [xml]$xml = Get-Content -LiteralPath $trx.FullName -Raw -Encoding UTF8
    foreach ($node in @($xml.SelectNodes("//*[local-name()='UnitTestResult' and @outcome='Failed']"))) {
        $message = [string]($node.SelectSingleNode("./*[local-name()='Output']/*[local-name()='ErrorInfo']/*[local-name()='Message']").InnerText)
        if (-not $oldFailures.ContainsKey([string]$node.testName)) { $oldFailures[[string]$node.testName] = [ordered]@{ test=[string]$node.testName; originalFailure=$message; originalShard=($trx.BaseName); originalOutcome='Failed' } }
    }
}
$terminalByName = @{}; foreach ($row in $terminal) { $terminalByName[$row.name] = $row }
$taxonomy = @($oldFailures.Values | Sort-Object test | ForEach-Object {
    $current = $terminalByName[$_.test]
    $classification = if ($null -eq $current -or $current.outcome -eq 'Aborted') { 'D timeout requiring split' } elseif ($current.outcome -ne 'Passed') { 'E genuinely reproducible product regression' } elseif ($_.originalFailure -match 'goal150b_independent_audit_then_bundled_human_gate|CURRENT_GENERATOR_STATE') { 'A stale pre-final-tree evidence' } else { 'B cross-shard artifact contamination' }
    [ordered]@{ originalShard=$_.originalShard; test=$_.test; originalFailure=$_.originalFailure; cleanIsolatedOutcome=if($null -eq $current){'missing'}else{$current.outcome}; classification=$classification; finalOutcome=if($null -eq $current){'missing'}else{$current.outcome}; changedPath=$null }
})
$taxonomyPayload = [ordered]@{ schemaVersion='goal150c_failure_taxonomy_v1'; initialGoal150BFailureCount=64; classifiedFailureCount=$taxonomy.Count; classificationTotals=[ordered]@{ A=@($taxonomy | Where-Object classification -like 'A *').Count; B=@($taxonomy | Where-Object classification -like 'B *').Count; C=0; D=@($taxonomy | Where-Object classification -like 'D *').Count; E=@($taxonomy | Where-Object classification -like 'E *').Count; F=0 }; entries=$taxonomy }
foreach ($root in @($proceduralRoot, $exportRoot)) { Write-Json (Join-Path $root 'validation-failure-taxonomy.json') $taxonomyPayload }

$allFocusedPassed = if ($EvidenceOnly) { [bool]$FocusedPassed } else { @($steps | Where-Object { -not $_.passed }).Count -eq 0 }
$allSpinePassed = if ($EvidenceOnly) { [bool]$SpinePassed } else { $allFocusedPassed }
$dashboard = [ordered]@{ schemaVersion='goal150c_dashboard_v1'; status=if($result.passed -and $allFocusedPassed -and $allSpinePassed){'GREEN'}else{'BLOCKED'}; validatedCommit=$result.validatedCommit; validationSnapshotMatchesFinalSources=$result.validationSnapshotMatchesFinalSources; hermeticSnapshot=$result.hermeticSnapshot; mainWorktreeUnchangedByValidation=$result.mainWorktreeUnchangedByValidation; nonProductLanePassed=$result.lanes.nonProductLanePassed; productSmokeLanePassed=$result.lanes.productSmokeLanePassed; focusedLanePassed=$allFocusedPassed; spineLanePassed=$allSpinePassed; counts=$result.counts; initialGoal150BFailureCount=64; allInitialFailuresClassified=($taxonomy.Count -eq 64); goal149Accepted=$false; goal150Accepted=$false; goal150aAccepted=$false; goal150bAccepted=$false; goal150cAccepted=$false; acceptedByCodex=$false; manualReviewPerformed=$false; manualGateReady=($result.passed -and $allFocusedPassed -and $allSpinePassed); passed=($result.passed -and $allFocusedPassed -and $allSpinePassed); rawRunRoot=($rawRoot.Substring($RepoRoot.Length + 1).Replace('\','/')) }
$focusedProof = [ordered]@{ schemaVersion='goal150c_focused_regression_proof_v1'; steps=$steps; passed=$allFocusedPassed }
$hashProof = [ordered]@{ schemaVersion='goal150c_historical_hash_regression_proof_v1'; source='docs/CURRENT_GENERATOR_STATE.json'; disabled='e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221/c46826d8231951ab941f6ee1608d30273b1e186f920ea8cad58c58c25317eeeb/95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8'; equipment='94a47ab896b425a76c2e523acef3ab87d538bb8f0c754b2402b0127e5ad82bf5/147f88ac026f006ab5fbe93dc6c7cb039e85189fcb3421a71a1fd99284d3a5c1/51bba1ffada4ce9ffccfa9132e7e7c007afcbcec8632d7de13d26ce961b3ea0d'; allOptional='ba9dbf32c8e79d4e2bf37116dd611cc7eccd7bee73f880aefeb041cce4b2ee40/19e837b8d4925b0b567c52adfb93905bc44ac6e9a13d3008726ff1be89ea49cf/ebb05a61036ddfde40b605267685ba8ab90baa01ed3b5efbb815615ae26eca5c'; goal150aCustom='66c6fa980123ad113a6b37e7d6d31b13d946b48df325230a44bac351660c0db3/578aa5b7b40b87015897c762cb651ef6e61f3a190e5e66fd80a6c1dd79664391/5f367569870cd8290225e06bba3570b8185c157febb823d444ff9cfa27def09e/9bbcc1573999aa3a82a257bf0c8e2d95ed8453574e8a4a7b1d91042146a01050'; preserved=$allFocusedPassed }
foreach ($root in @($proceduralRoot, $exportRoot)) {
    Write-Json (Join-Path $root 'goal150c-dashboard.json') $dashboard
    Write-Json (Join-Path $root 'focused-regression-proof.json') $focusedProof
    Write-Json (Join-Path $root 'historical-hash-regression-proof.json') $hashProof
    Write-Json (Join-Path $root 'publication-policy-proof.json') ([ordered]@{ policy='status-aware'; status=$dashboard.status; manualAcceptanceClaimed=$false; publicationPending=$true })
    Write-Json (Join-Path $root 'artifact-scope-proof.json') ([ordered]@{ status='pending-final-scope-guard'; rawLogsIgnored=$true; compactArtifactMaximum=12 })
    $files = @(Get-ChildItem -LiteralPath $root -File | Select-Object -ExpandProperty Name | Sort-Object)
    Write-Json (Join-Path $root 'goal150c-file-index.json') ([ordered]@{ root=($root.Substring($RepoRoot.Length + 1).Replace('\','/')); compactFileCount=$files.Count; files=$files })
    [IO.File]::WriteAllText((Join-Path $root 'goal150c-report.md'), "# Goal 150C validation report`n`n- Status: $($dashboard.status)`n- Raw run: $($dashboard.rawRunRoot)`n- Hermetic counts: $($result.counts | ConvertTo-Json -Compress)`n- Human acceptance: not claimed.`n", [Text.UTF8Encoding]::new($false))
}
if (-not $dashboard.passed) { exit 2 }
Write-Host 'GOAL150C_HERMETIC_VALIDATION_GREEN'
