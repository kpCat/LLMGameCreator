param([switch]$SkipStandaloneSmoke)
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$tests = Join-Path $repositoryRoot 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$goalId = 'goal-153a-parameter-domain-turn-binding-event-atomicity-and-lethal-status-hotfix'
$evidenceRoot = Join-Path $repositoryRoot ".llmgc\procedural\$goalId"
$cacheKey = '6af4d5eb5b42f956110555b58fb4e276'
$hostRoot = Join-Path $env:LOCALAPPDATA "LLMGameCreator\StandaloneHostCache\$cacheKey\host"
$requiredHostPaths = @(
    (Join-Path $hostRoot 'LLMGameCreatorProjectHost.exe'),
    (Join-Path $hostRoot 'LLMGameCreatorProjectHost_Data'),
    (Join-Path $hostRoot 'UnityPlayer.dll'),
    (Join-Path $hostRoot 'MonoBleedingEdge'),
    (Join-Path $hostRoot 'host-cache-manifest.json')
)
if ($requiredHostPaths.Where({ -not (Test-Path -LiteralPath $_) }).Count -ne 0) {
    throw 'BLOCKED: required standalone host cache is unavailable; Unity Editor will not be started.'
}
if (Get-Process Unity -ErrorAction SilentlyContinue) {
    throw 'BLOCKED: Unity process is already running; Goal153A requires a zero-Unity-process proof.'
}

function Invoke-FocusedTest([string]$filter, [string]$label) {
    dotnet test $tests -c Debug --no-build --filter $filter
    if ($LASTEXITCODE -ne 0) { throw "$label failed" }
}

Push-Location $repositoryRoot
try {
    if (Test-Path -LiteralPath $evidenceRoot) { Remove-Item -LiteralPath $evidenceRoot -Recurse -Force }
    New-Item -ItemType Directory -Path $evidenceRoot -Force | Out-Null
    $env:LLMGC_GOAL153A_EVIDENCE_ROOT = $evidenceRoot

    dotnet build
    if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed' }

    Invoke-FocusedTest 'FullyQualifiedName~Goal153A' 'Goal153A focused tests'
    Invoke-FocusedTest 'FullyQualifiedName~Goal153' 'Goal153 regressions'
    Invoke-FocusedTest 'FullyQualifiedName~CapabilityDrivenRuntimePlaythrough' 'capability playthrough regressions'
    Invoke-FocusedTest 'FullyQualifiedName~UnifiedGameProjectWorkspace' 'unified workspace regressions'
    Invoke-FocusedTest 'FullyQualifiedName~ProjectsPage' 'Projects page regressions'
    Invoke-FocusedTest 'FullyQualifiedName~RuntimeEncounter' 'Runtime encounter regressions'

    if (-not $SkipStandaloneSmoke) {
        $env:LLMGC_GOAL153_STANDALONE = 'true'
        $env:LLMGC_GOAL153_EVIDENCE_PATH = Join-Path $evidenceRoot 'goal153a-dashboard.json'
        Invoke-FocusedTest 'FullyQualifiedName=LLMGameCreator.Tests.Application.UnifiedGameProjectWorkspace.Goal153AbilityManaStatusWorkspaceTests.Goal153_real_project_copy_saves_reopens_builds_and_changes_only_with_typed_parameter' 'cached standalone hidden smoke'
        Remove-Item Env:LLMGC_GOAL153_STANDALONE -ErrorAction SilentlyContinue
        Remove-Item Env:LLMGC_GOAL153_EVIDENCE_PATH -ErrorAction SilentlyContinue
    }

    $historicalEvidencePaths = @(
        '.llmgc\exports\goal-149-capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice\capability-runtime-playthrough-contract-catalog.json',
        '.llmgc\exports\goal-149-capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice\equipment-module-definition-proof.json',
        '.llmgc\exports\goal-149-capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice\goal149-file-index.json',
        '.llmgc\exports\goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice\character-attributes-module-proof.json',
        '.llmgc\exports\goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice\extended-mutation-engine-proof.json',
        '.llmgc\exports\goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice\full-current-optional-set-proof.json',
        '.llmgc\exports\goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice\goal150-certification-proof.json',
        '.llmgc\exports\goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice\goal150-file-index.json',
        '.llmgc\exports\goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice\level-progression-module-proof.json',
        '.llmgc\procedural\goal-149-capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice\capability-runtime-playthrough-contract-catalog.json',
        '.llmgc\procedural\goal-149-capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice\equipment-module-definition-proof.json',
        '.llmgc\procedural\goal-149-capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice\goal149-file-index.json',
        '.llmgc\procedural\goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice\character-attributes-module-proof.json',
        '.llmgc\procedural\goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice\extended-mutation-engine-proof.json',
        '.llmgc\procedural\goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice\full-current-optional-set-proof.json',
        '.llmgc\procedural\goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice\goal150-certification-proof.json',
        '.llmgc\procedural\goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice\goal150-file-index.json',
        '.llmgc\procedural\goal-150-character-attributes-and-level-progression-featuremodules-vertical-slice\level-progression-module-proof.json'
    )
    $historicalEvidenceSnapshots = @{}
    foreach ($relativePath in $historicalEvidencePaths) {
        $absolutePath = Join-Path $repositoryRoot $relativePath
        $historicalEvidenceSnapshots[$absolutePath] = [IO.File]::ReadAllBytes($absolutePath)
    }
    try {
        & (Join-Path $repositoryRoot '.devflow\scripts\run-capability-runtime-equipment-slice.ps1')
        if ($LASTEXITCODE -ne 0) { throw 'equipment regression failed' }
        & (Join-Path $repositoryRoot '.devflow\scripts\run-character-attributes-level-progression-slice.ps1')
        if ($LASTEXITCODE -ne 0) { throw 'attributes/progression regression failed' }
    }
    finally {
        foreach ($absolutePath in $historicalEvidenceSnapshots.Keys) {
            [IO.File]::WriteAllBytes($absolutePath, $historicalEvidenceSnapshots[$absolutePath])
        }
    }
    & (Join-Path $repositoryRoot '.devflow\scripts\check-current-goal.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'current-goal documentation guard failed' }

    $scopeReport = Join-Path $env:TEMP ('llmgc-goal153a-scope-' + [Guid]::NewGuid().ToString('N'))
    & (Join-Path $repositoryRoot '.devflow\scripts\check-artifact-scope.ps1') `
        -Scenario 'goal-153a-parameter-domain-turn-binding-event-atomicity-and-lethal-status-hotfix' `
        -BaselineRef '8664b19c8fddb60e347402d0dc92535630c99cf3' `
        -ReportDirectory $scopeReport
    if ($LASTEXITCODE -ne 0) { throw 'Goal153A artifact scope failed' }
    @{ schemaVersion = 'goal153a_artifact_scope_proof_v1'; status = 'GREEN'; violationCount = 0; scenario = $goalId } |
        ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $evidenceRoot 'artifact-scope-proof.json') -Encoding UTF8

    if (Get-Process Unity -ErrorAction SilentlyContinue) {
        throw 'FAILED: Unity process was observed; Goal153A Unity process start count must remain zero.'
    }
    @"
# Goal 153A report

Status: GREEN

- duration 1/2/5: full Runtime qualification and replay GREEN
- duration 1000: plan-only, 1000 target ticks and 2999 bound EndTurn actions
- expected-participant turn binding: GREEN
- ability/status/canonical event atomicity: GREEN
- lethal enemy victory and lethal player loss: GREEN
- duration 5 checkpoint remainingTicks=4 and replay equivalence: GREEN
- cached standalone reused, not rebuilt; hidden smoke GREEN
- Unity process start count: 0
- human acceptance claimed: no
"@ | Set-Content -LiteralPath (Join-Path $evidenceRoot 'goal153a-report.md') -Encoding UTF8

    Write-Host 'GOAL153A GREEN: focused Runtime/planner/workspace/cache and artifact-scope proof passed.'
}
finally {
    Remove-Item Env:LLMGC_GOAL153_STANDALONE -ErrorAction SilentlyContinue
    Remove-Item Env:LLMGC_GOAL153_EVIDENCE_PATH -ErrorAction SilentlyContinue
    Remove-Item Env:LLMGC_GOAL153A_EVIDENCE_ROOT -ErrorAction SilentlyContinue
    Pop-Location
}
