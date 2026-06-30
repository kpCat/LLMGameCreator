param()
$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$Manifest = Join-Path $Root 'package-selection-matrix.json'
if (-not (Test-Path -LiteralPath $Manifest)) { throw 'package-selection-matrix.json is missing.' }
$Rows = (Get-Content -Raw -LiteralPath $Manifest | ConvertFrom-Json).rows
Write-Host ('Goal061 manual review rows: ' + $Rows.Count)
foreach ($Row in $Rows) {
  $PackagePath = Join-Path (Split-Path -Parent $Root) $Row.packageRelativePath
  if (-not (Test-Path -LiteralPath $PackagePath)) { throw ('Missing package: ' + $Row.rowId) }
  Write-Host ($Row.rowId + ' ' + $Row.packageId)
}
Write-Host 'full_campaign_playable_review_package_rc_verification required'
