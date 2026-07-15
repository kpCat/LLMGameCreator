Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$tests = Join-Path $root 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$scenario = 'goal-155a-current-package-correlated-release-candidate-record-truth-hotfix'
$baseline = '7084244a67bd863f128a6bfb67d5fa5031bf0832'

Remove-Item Env:LLMGC_GOAL155_RUN_SMOKE -ErrorAction SilentlyContinue
Remove-Item Env:LLMGC_GOAL155A_RUN_SMOKE -ErrorAction SilentlyContinue
Push-Location $root
try {
    dotnet build
    if ($LASTEXITCODE -ne 0) { throw 'Goal155A build failed.' }

    $filters = @('Goal155A','Goal155','Goal154D','Goal153C','Goal150AParameterizedRuntimeContractSynchronization',
        'Goal149','UnifiedGameProjectWorkspace','ProjectsPage','FeatureModuleLibrary','FeatureModuleCertification')
    foreach ($filter in $filters) {
        dotnet test $tests -c Debug --no-build --filter "FullyQualifiedName~$filter"
        if ($LASTEXITCODE -ne 0) { throw "Goal155A filter failed: $filter" }
    }

    & (Join-Path $PSScriptRoot 'run-capability-runtime-equipment-slice.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Goal149 slice failed.' }
    & (Join-Path $PSScriptRoot 'run-character-attributes-level-progression-slice.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Goal150 slice failed.' }
    & (Join-Path $PSScriptRoot 'check-current-goal.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Current-goal guard failed.' }

    & (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') -Scenario $scenario -BaselineRef $baseline
    if ($LASTEXITCODE -ne 0) { throw 'Goal155A artifact scope failed.' }
}
finally { Pop-Location }
