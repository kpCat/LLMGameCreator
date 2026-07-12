param()

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$authorized = @(
    'unity/LLMGameCreatorAlpha/Packages/packages-lock.json',
    'unity/LLMGameCreatorAlpha/ProjectSettings/AudioManager.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/ClusterInputManager.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/DynamicsManager.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/EditorBuildSettings.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/EditorSettings.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/GraphicsSettings.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/InputManager.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/MemorySettings.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/MultiplayerManager.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/NavMeshAreas.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/Physics2DSettings.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/PresetManager.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/ProjectSettings.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/QualitySettings.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/SceneTemplateSettings.json',
    'unity/LLMGameCreatorAlpha/ProjectSettings/TagManager.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/TimeManager.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/UnityConnectSettings.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/VFXManager.asset',
    'unity/LLMGameCreatorAlpha/ProjectSettings/VersionControlSettings.asset'
)

$inventory = foreach ($relative in $authorized) {
    $path = [IO.Path]::GetFullPath((Join-Path $root $relative))
    $inside = $path.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)
    if (!$inside -or !(Test-Path -LiteralPath $path -PathType Leaf)) { throw "Exact cleanup precondition failed: $relative" }
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    git ls-files --error-unmatch -- $relative 2>$null | Out-Null
    $trackedExitCode = $LASTEXITCODE
    $ErrorActionPreference = $previousErrorActionPreference
    if ($trackedExitCode -eq 0) { throw "Tracked path cannot be deleted: $relative" }
    $item = Get-Item -LiteralPath $path -Force
    if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { throw "Reparse path cannot be deleted: $relative" }
    [pscustomobject]@{ path = $relative; bytes = $item.Length; sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash }
}

$rawRoot = Join-Path $env:LOCALAPPDATA 'LLMGameCreator\Goal152C'
New-Item -ItemType Directory -Path $rawRoot -Force | Out-Null
$inventory | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $rawRoot 'exact-cleanup-before.json') -Encoding UTF8
foreach ($record in $inventory) { Remove-Item -LiteralPath (Join-Path $root $record.path) -Force }
if (($authorized | Where-Object { Test-Path -LiteralPath (Join-Path $root $_) }).Count -ne 0) { throw 'Exact cleanup did not remove every authorized path.' }
[pscustomobject]@{ authorizedDeleted = $inventory.Count; authorizedRemaining = 0 } | ConvertTo-Json
