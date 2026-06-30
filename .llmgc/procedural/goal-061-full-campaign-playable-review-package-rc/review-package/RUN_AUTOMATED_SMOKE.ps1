param()
$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$Plan = Join-Path $Root 'StreamingAssets\LLMGameCreatorAlpha\review-package-rc\unity-player-command-plan.json'
if (-not (Test-Path -LiteralPath $Plan)) { throw 'Unity command plan is missing.' }
$CommandPlan = Get-Content -Raw -LiteralPath $Plan | ConvertFrom-Json
if ($CommandPlan.rows.Count -ne 9) { throw 'Expected 9 package rows.' }
foreach ($Row in $CommandPlan.rows) {
  $PackagePath = Join-Path (Split-Path -Parent $Root) ('review-package\' + ($Row.packageRelativePath -replace '^review-package-rc/', ''))
  if (-not (Test-Path -LiteralPath $PackagePath)) { throw ('Missing package for ' + $Row.rowId) }
  if (-not $Row.packageHashVerified) { throw ('Package hash was not verified for ' + $Row.rowId) }
}
Write-Host 'review_package_rc_loaded=true'
Write-Host ('review_package_rc_id=' + $CommandPlan.reviewPackageRcId)
Write-Host 'review_package_rc_proof=goal061'
