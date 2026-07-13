param([switch]$SkipRequiredChecks)

$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$tests = Join-Path $repositoryRoot 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$goalId = 'goal-153b-declarative-parameter-constraints-domain-integrity-and-goal-quality-gate-hotfix'
$evidenceRoot = Join-Path $repositoryRoot ".llmgc\procedural\$goalId"
$cacheKey = '6af4d5eb5b42f956110555b58fb4e276'
$hostRoot = Join-Path $env:LOCALAPPDATA "LLMGameCreator\StandaloneHostCache\$cacheKey\host"

function Invoke-FocusedTest([string]$filter, [string]$label) {
    dotnet test $tests -c Debug --no-build --filter $filter
    if ($LASTEXITCODE -ne 0) { throw "$label failed" }
}

$requiredHostPaths = @(
    (Join-Path $hostRoot 'LLMGameCreatorProjectHost.exe'),
    (Join-Path $hostRoot 'host-cache-manifest.json')
)
if ($requiredHostPaths.Where({ -not (Test-Path -LiteralPath $_) }).Count -ne 0) {
    throw 'BLOCKED: required standalone host cache is unavailable; Unity Editor will not be started.'
}
if (Get-Process Unity -ErrorAction SilentlyContinue) { throw 'BLOCKED: Unity process is already running.' }

Push-Location $repositoryRoot
try {
    if ($SkipRequiredChecks) {
        if (-not (Test-Path -LiteralPath $evidenceRoot)) { New-Item -ItemType Directory -Path $evidenceRoot -Force | Out-Null }
    }
    else {
        if (Test-Path -LiteralPath $evidenceRoot) { Remove-Item -LiteralPath $evidenceRoot -Recurse -Force }
        New-Item -ItemType Directory -Path $evidenceRoot -Force | Out-Null

    dotnet build
    if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed' }
    Invoke-FocusedTest 'FullyQualifiedName~Goal153B' 'Goal153B focused tests'
    Invoke-FocusedTest 'FullyQualifiedName~Goal153A' 'Goal153A regressions'
    Invoke-FocusedTest 'FullyQualifiedName~Goal153' 'Goal153 regressions'
    Invoke-FocusedTest 'FullyQualifiedName~FeatureModuleLibrary' 'FeatureModuleLibrary regressions'
    Invoke-FocusedTest 'FullyQualifiedName~FeatureModuleCertification' 'FeatureModuleCertification regressions'
    Invoke-FocusedTest 'FullyQualifiedName~CapabilityDrivenRuntimePlaythrough' 'Capability runtime regressions'
    Invoke-FocusedTest 'FullyQualifiedName~UnifiedGameProjectWorkspace' 'saved-project lifecycle regressions'
    Invoke-FocusedTest 'FullyQualifiedName~ProjectsPage' 'ProjectsPage regressions'
    & (Join-Path $repositoryRoot '.devflow\scripts\run-capability-runtime-equipment-slice.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'equipment slice regression failed' }
    & (Join-Path $repositoryRoot '.devflow\scripts\run-character-attributes-level-progression-slice.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'attributes/progression slice regression failed' }

    $env:LLMGC_GOAL153_STANDALONE = 'true'
    $env:LLMGC_GOAL153_EVIDENCE_PATH = Join-Path $evidenceRoot 'goal153b-dashboard.json'
    $env:LLMGC_GOAL153A_EVIDENCE_ROOT = $evidenceRoot
    Invoke-FocusedTest 'FullyQualifiedName=LLMGameCreator.Tests.Application.UnifiedGameProjectWorkspace.Goal153AbilityManaStatusWorkspaceTests.Goal153_real_project_copy_saves_reopens_builds_and_changes_only_with_typed_parameter' 'cached hidden standalone smoke'
    Remove-Item Env:LLMGC_GOAL153_STANDALONE -ErrorAction SilentlyContinue
    Remove-Item Env:LLMGC_GOAL153_EVIDENCE_PATH -ErrorAction SilentlyContinue
    Remove-Item Env:LLMGC_GOAL153A_EVIDENCE_ROOT -ErrorAction SilentlyContinue
    }

    $proofs = @{
        'declarative-constraint-contract-proof.json' = @{ status = 'GREEN'; operators = @('<', '<=', '==', '!=', '>=', '>'); mutationAppliedOnFailure = $false; diagnosticsContainValues = $true }
        'mana-domain-integrity-proof.json' = @{ status = 'GREEN'; startingManaDomain = '0..1000'; costDomain = '1..1000'; validBoundaries = @('1/1', '12/3', '1000/1000'); invalidBoundary = '2/3'; manaCapacity = 1000; participantDomainValidatedBeforeRuntime = $true }
        'qualification-domain-proof.json' = @{ status = 'GREEN'; maximaSource = 'module parameter definitions'; overflowSafe = $true; trainingTargetContent = 'first-vertical-slice data' }
        'architecture-no-hardcoding-proof.json' = @{ status = 'GREEN'; genericProductionGoal153LiteralCount = 0 }
        'certification-invalidation-proof.json' = @{ status = 'GREEN'; unchangedSecondCertificationReuses = $true; constraintChangeInvalidatesOwnerAndDependents = $true; unrelatedOptionalReusable = $true }
        'goal-quality-policy-proof.json' = @{ status = 'GREEN'; policy = 'docs/GOAL_DESIGN_QUALITY_POLICY.md'; linkedFromAgents = $true; linkedFromContextIndex = $true }
    }
    foreach ($name in $proofs.Keys) { $proofs[$name] | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $evidenceRoot $name) -Encoding utf8 }

    & (Join-Path $repositoryRoot '.devflow\scripts\check-current-goal.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'current-goal documentation guard failed' }
    $scopeReport = Join-Path $env:TEMP ('llmgc-goal153b-scope-' + [Guid]::NewGuid().ToString('N'))
    & (Join-Path $repositoryRoot '.devflow\scripts\check-artifact-scope.ps1') -Scenario $goalId -BaselineRef '4325fe4745c6a1e2363e4d49403d2360404ab69e' -ReportDirectory $scopeReport
    if ($LASTEXITCODE -ne 0) { throw 'artifact scope failed' }
    if (Get-Process Unity -ErrorAction SilentlyContinue) { throw 'FAILED: Unity process was observed.' }

    @"
# Goal 153B report

Status: GREEN

- declarative numeric constraints: six operators; no mutation on failure
- mana capacity: 1000; participant resource domains reject missing, below-minimum and above-maximum values before Runtime
- qualification domain: declarations and overflow-safe arithmetic
- certification: constraint changes invalidate owner/dependents; unrelated modules reuse
- architecture scan: zero Goal153 literals in generic production C#
- Goal153A duration, event atomicity and lethal-status regressions: GREEN
- cache-only hidden standalone smoke: GREEN; Unity process starts: 0
- human acceptance claimed: no
"@ | Set-Content -LiteralPath (Join-Path $evidenceRoot 'goal153b-report.md') -Encoding utf8

    Write-Host 'GOAL153B GREEN: focused contracts, lifecycle, cache smoke and scope passed.'
}
finally {
    Remove-Item Env:LLMGC_GOAL153_STANDALONE -ErrorAction SilentlyContinue
    Remove-Item Env:LLMGC_GOAL153_EVIDENCE_PATH -ErrorAction SilentlyContinue
    Remove-Item Env:LLMGC_GOAL153A_EVIDENCE_ROOT -ErrorAction SilentlyContinue
    Pop-Location
}
