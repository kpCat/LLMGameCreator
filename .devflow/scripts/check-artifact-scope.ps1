param(
    [string]$PolicyPath = "",
    [string]$Scenario = "artifact-scope",
    [string]$BaselineRef = "",
    [string]$ReportDirectory = "",
    [switch]$FailOnTrackedIgnored,
    [Parameter(ValueFromRemainingArguments=$true)][string[]]$RemainingArguments = @()
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment

$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$AllowedPath = @()
$AllowedPathPrefix = @()
$ChangedPath = @()

for ($argumentIndex = 0; $argumentIndex -lt $RemainingArguments.Count; $argumentIndex++) {
    $argument = $RemainingArguments[$argumentIndex]
    if ($argument -eq "-AllowedPath") {
        $argumentIndex++
        if ($argumentIndex -ge $RemainingArguments.Count) {
            throw "-AllowedPath requires a value."
        }
        $AllowedPath += $RemainingArguments[$argumentIndex]
    }
    elseif ($argument -eq "-AllowedPathPrefix") {
        $argumentIndex++
        if ($argumentIndex -ge $RemainingArguments.Count) {
            throw "-AllowedPathPrefix requires a value."
        }
        $AllowedPathPrefix += $RemainingArguments[$argumentIndex]
    }
    elseif ($argument -eq "-ChangedPath") {
        $argumentIndex++
        if ($argumentIndex -ge $RemainingArguments.Count) {
            throw "-ChangedPath requires a value."
        }
        $ChangedPath += $RemainingArguments[$argumentIndex]
    }
    else {
        throw "Unknown artifact scope argument: $argument"
    }
}

if ([string]::IsNullOrWhiteSpace($PolicyPath)) {
    $PolicyPath = Join-Path $RepoRoot ".devflow\artifact-scope\artifact-scope-policy.json"
}

function ConvertTo-ScopeRelativePath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $normalized = $Path.Replace('\', '/').Trim()
    $repo = $RepoRoot.Replace('\', '/').TrimEnd('/')

    if ($normalized.StartsWith($repo, [System.StringComparison]::OrdinalIgnoreCase)) {
        $normalized = $normalized.Substring($repo.Length).TrimStart('/')
    }

    if ($normalized.StartsWith("./", [System.StringComparison]::Ordinal)) {
        $normalized = $normalized.Substring(2)
    }

    return $normalized.TrimStart('/')
}

function Test-ScopePrefix {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$Prefix
    )

    $candidate = ConvertTo-ScopeRelativePath -Path $Path
    $normalizedPrefix = ConvertTo-ScopeRelativePath -Path $Prefix
    if (-not $normalizedPrefix.EndsWith("/", [System.StringComparison]::Ordinal)) {
        $normalizedPrefix = $normalizedPrefix + "/"
    }

    return $candidate.StartsWith($normalizedPrefix, [System.StringComparison]::OrdinalIgnoreCase)
}

function Test-ScopeGlob {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$Pattern
    )

    $candidate = ConvertTo-ScopeRelativePath -Path $Path
    $normalizedPattern = ConvertTo-ScopeRelativePath -Path $Pattern
    $escaped = [Regex]::Escape($normalizedPattern)
    $escaped = $escaped.Replace('\*\*', '.*')
    $escaped = $escaped.Replace('\*', '[^/]*')
    return [Regex]::IsMatch($candidate, "^$escaped$", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
}

function Add-UniquePath {
    param(
        [Parameter(Mandatory=$true)]$List,
        [Parameter(Mandatory=$true)]$Seen,
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$Status,
        [bool]$Tracked = $true
    )

    $relative = ConvertTo-ScopeRelativePath -Path $Path
    if ([string]::IsNullOrWhiteSpace($relative)) {
        return
    }

    if (-not $Seen.ContainsKey($relative)) {
        $Seen[$relative] = $true
        $List.Add([pscustomobject]@{
            path = $relative
            status = $Status
            tracked = $Tracked
        }) | Out-Null
    }
}

function Invoke-ScopeGit {
    param([Parameter(Mandatory=$true)][string[]]$ArgsList)

    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $output = & git @ArgsList 2>&1
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    $lines = @($output | ForEach-Object { "$_" } | Where-Object {
        -not $_.StartsWith("warning: ", [System.StringComparison]::OrdinalIgnoreCase)
    })

    return [pscustomobject]@{
        exit_code = $exitCode
        output = $lines
    }
}

function Get-GitChangedPaths {
    $list = New-Object System.Collections.Generic.List[object]
    $seen = @{}

    if ($ChangedPath.Count -gt 0) {
        foreach ($path in $ChangedPath) {
            Add-UniquePath -List $list -Seen $seen -Path $path -Status "test" -Tracked $true
        }
        return $list
    }

    Push-Location $RepoRoot
    try {
        $trackedFiles = @{}
        $trackedResult = Invoke-ScopeGit -ArgsList @("ls-files")
        if ($trackedResult.exit_code -ne 0) {
            throw "git ls-files failed: $($trackedResult.output -join [Environment]::NewLine)"
        }
        foreach ($tracked in $trackedResult.output) {
            $trackedFiles[(ConvertTo-ScopeRelativePath -Path $tracked)] = $true
        }

        $diffArgs = @("diff", "--name-status")
        if (-not [string]::IsNullOrWhiteSpace($BaselineRef)) {
            $diffArgs += $BaselineRef
        }

        $diffResult = Invoke-ScopeGit -ArgsList $diffArgs
        if ($diffResult.exit_code -ne 0) {
            throw "git $($diffArgs -join ' ') failed: $($diffResult.output -join [Environment]::NewLine)"
        }
        foreach ($line in $diffResult.output) {
            $parts = $line -split "`t", 2
            if ($parts.Count -ne 2) { continue }
            $status = $parts[0]
            $path = $parts[1]
            if ($status.StartsWith("R") -and $path.Contains("`t")) { $path = ($path -split "`t")[-1] }
            Add-UniquePath -List $list -Seen $seen -Path $path -Status $status -Tracked $true
        }

        $statusResult = Invoke-ScopeGit -ArgsList @("status", "--porcelain")
        if ($statusResult.exit_code -ne 0) {
            throw "git status --porcelain failed: $($statusResult.output -join [Environment]::NewLine)"
        }
        foreach ($line in $statusResult.output) {
            if ([string]::IsNullOrWhiteSpace($line) -or $line.Length -lt 4) {
                continue
            }

            $status = $line.Substring(0, 2)
            $path = $line.Substring(3)
            if ($path.Contains(" -> ")) {
                $path = ($path -split " -> ")[-1]
            }

            $relative = ConvertTo-ScopeRelativePath -Path $path
            $isTracked = $trackedFiles.ContainsKey($relative) -or ($status -ne "??")
            Add-UniquePath -List $list -Seen $seen -Path $relative -Status $status.Trim() -Tracked:$isTracked
        }
    }
    finally {
        Pop-Location
    }

    return $list
}

function Get-ArrayProperty {
    param(
        [Parameter(Mandatory=$true)]$Object,
        [Parameter(Mandatory=$true)][string]$PropertyName
    )

    if ($Object.PSObject.Properties.Name -contains $PropertyName) {
        return @($Object.$PropertyName)
    }

    return @()
}

function Test-ExactAllowed {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string[]]$Allowed
    )

    $relative = ConvertTo-ScopeRelativePath -Path $Path
    foreach ($allowedPath in $Allowed) {
        if ($relative.Equals((ConvertTo-ScopeRelativePath -Path $allowedPath), [System.StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

function Test-PrefixAllowed {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string[]]$Allowed
    )

    foreach ($prefix in $Allowed) {
        if (Test-ScopePrefix -Path $Path -Prefix $prefix) {
            return $true
        }
    }

    return $false
}

function Classify-ChangedPath {
    param(
        [Parameter(Mandatory=$true)]$Change,
        [Parameter(Mandatory=$true)]$Policy,
        [Parameter(Mandatory=$true)][string[]]$ExactAllowed,
        [Parameter(Mandatory=$true)][string[]]$PrefixAllowed,
        [Parameter(Mandatory=$true)][AllowEmptyCollection()][string[]]$DeletedExactAllowed,
        [Parameter(Mandatory=$true)][AllowEmptyCollection()][string[]]$DeletedPrefixAllowed
    )

    $path = $Change.path
    $extension = [System.IO.Path]::GetExtension($path)

    if ($Change.status.StartsWith("D", [System.StringComparison]::OrdinalIgnoreCase)) {
        if ((Test-ExactAllowed -Path $path -Allowed $DeletedExactAllowed) -or (Test-PrefixAllowed -Path $path -Allowed $DeletedPrefixAllowed)) {
            return [pscustomobject]@{ severity = "info"; accepted = $true; category = "allowed_declared_diagnostic_deletion"; code = "artifact_scope.allowed.deletion"; message = "Deletion is explicitly allowed for compact evidence retention." }
        }
    }

    if (Test-ExactAllowed -Path $path -Allowed $ExactAllowed) {
        $category = if ($path.StartsWith("docs/agent-tasks/", [System.StringComparison]::OrdinalIgnoreCase) -or $path.Contains("GOAL_022")) { "allowed_task_doc" } else { "allowed_current_goal_change" }
        return [pscustomobject]@{ severity = "info"; accepted = $true; category = $category; code = "artifact_scope.allowed.exact"; message = "Path is explicitly allowed." }
    }

    if (Test-PrefixAllowed -Path $path -Allowed $PrefixAllowed) {
        $category = if ($path.StartsWith(".llmgc/procedural/", [System.StringComparison]::OrdinalIgnoreCase)) { "allowed_current_goal_artifact" } else { "allowed_current_goal_change" }
        return [pscustomobject]@{ severity = "info"; accepted = $true; category = $category; code = "artifact_scope.allowed.prefix"; message = "Path prefix is explicitly allowed." }
    }

    foreach ($pattern in @(Get-ArrayProperty -Object $Policy -PropertyName "trackedHeavyOutputPatterns")) {
        if (Test-ScopeGlob -Path $path -Pattern ("" + $pattern)) {
            $severity = if ($FailOnTrackedIgnored) { "error" } else { "warning" }
            $accepted = -not $FailOnTrackedIgnored
            return [pscustomobject]@{ severity = $severity; accepted = $accepted; category = "tracked_heavy_output_warning"; code = "artifact_scope.heavy_output"; message = "Path matches a heavy generated output pattern." }
        }
    }

    foreach ($exact in @(Get-ArrayProperty -Object $Policy -PropertyName "standardForbiddenExactPaths")) {
        if ((ConvertTo-ScopeRelativePath -Path $path).Equals((ConvertTo-ScopeRelativePath -Path ("" + $exact)), [System.StringComparison]::OrdinalIgnoreCase)) {
            return [pscustomobject]@{ severity = "error"; accepted = $false; category = "disallowed_project_file_mutation"; code = "artifact_scope.project_file.forbidden"; message = "Project/solution file mutation is forbidden unless explicitly allowed." }
        }
    }

    foreach ($forbiddenExtension in @(Get-ArrayProperty -Object $Policy -PropertyName "standardForbiddenExtensions")) {
        if ($extension.Equals(("" + $forbiddenExtension), [System.StringComparison]::OrdinalIgnoreCase)) {
            return [pscustomobject]@{ severity = "error"; accepted = $false; category = "disallowed_project_file_mutation"; code = "artifact_scope.project_file.forbidden"; message = "Project file mutation is forbidden unless explicitly allowed." }
        }
    }

    foreach ($prefix in @(Get-ArrayProperty -Object $Policy -PropertyName "publicGamePackageSchemaPathPrefixes")) {
        $schemaPath = ConvertTo-ScopeRelativePath -Path ("" + $prefix)
        $candidatePath = ConvertTo-ScopeRelativePath -Path $path
        if ($candidatePath.Equals($schemaPath, [System.StringComparison]::OrdinalIgnoreCase) -or (Test-ScopePrefix -Path $path -Prefix $schemaPath)) {
            return [pscustomobject]@{ severity = "error"; accepted = $false; category = "disallowed_public_gamepackage_schema_mutation"; code = "artifact_scope.gamepackage_schema.forbidden"; message = "Public GamePackage schema mutation is forbidden unless explicitly allowed." }
        }
    }

    foreach ($prefix in @(Get-ArrayProperty -Object $Policy -PropertyName "standardForbiddenPathPrefixes")) {
        if (Test-ScopePrefix -Path $path -Prefix ("" + $prefix)) {
            $category = if ($path.StartsWith("generator-library/", [System.StringComparison]::OrdinalIgnoreCase)) { "disallowed_generator_library_mutation" } else { "disallowed_forbidden_path_mutation" }
            return [pscustomobject]@{ severity = "error"; accepted = $false; category = $category; code = "artifact_scope.forbidden_path"; message = "Path is forbidden unless explicitly allowed." }
        }
    }

    foreach ($prefix in @(Get-ArrayProperty -Object $Policy -PropertyName "unityBuildEntrypointPathPrefixes")) {
        if ((Test-ScopePrefix -Path $path -Prefix ("" + $prefix)) -or (ConvertTo-ScopeRelativePath -Path $path).Equals((ConvertTo-ScopeRelativePath -Path ("" + $prefix)), [System.StringComparison]::OrdinalIgnoreCase)) {
            return [pscustomobject]@{ severity = "error"; accepted = $false; category = "disallowed_unity_entrypoint_mutation"; code = "artifact_scope.unity_entrypoint.forbidden"; message = "Unity build/player entrypoint mutation is forbidden unless explicitly allowed." }
        }
    }

    foreach ($prefix in @(Get-ArrayProperty -Object $Policy -PropertyName "historicalArtifactRoots")) {
        if (Test-ScopePrefix -Path $path -Prefix ("" + $prefix)) {
            return [pscustomobject]@{ severity = "error"; accepted = $false; category = "disallowed_legacy_artifact_mutation"; code = "artifact_scope.legacy_artifact.forbidden"; message = "Historical compact artifact mutation is forbidden for this scenario." }
        }
    }

    return [pscustomobject]@{ severity = "error"; accepted = $false; category = "disallowed_unlisted_change"; code = "artifact_scope.unlisted_change"; message = "Path is not in the declared allowed scope." }
}

if (-not (Test-Path -LiteralPath $PolicyPath)) {
    throw "Artifact scope policy not found: $PolicyPath"
}

$policy = Get-Content -Raw -Encoding UTF8 -LiteralPath $PolicyPath | ConvertFrom-Json
$exactAllowed = New-Object System.Collections.Generic.List[string]
$prefixAllowed = New-Object System.Collections.Generic.List[string]
$deletedExactAllowed = New-Object System.Collections.Generic.List[string]
$deletedPrefixAllowed = New-Object System.Collections.Generic.List[string]

foreach ($path in $AllowedPath) {
    $exactAllowed.Add((ConvertTo-ScopeRelativePath -Path $path)) | Out-Null
}

foreach ($prefix in $AllowedPathPrefix) {
    $normalizedPrefix = ConvertTo-ScopeRelativePath -Path $prefix
    if (-not $normalizedPrefix.EndsWith("/", [System.StringComparison]::Ordinal)) {
        $normalizedPrefix += "/"
    }
    $prefixAllowed.Add($normalizedPrefix) | Out-Null
}

foreach ($prefix in @(Get-ArrayProperty -Object $policy -PropertyName "defaultMutableRoots")) {
    $normalizedPrefix = ConvertTo-ScopeRelativePath -Path ("" + $prefix)
    if (-not $normalizedPrefix.EndsWith("/", [System.StringComparison]::Ordinal)) {
        $normalizedPrefix += "/"
    }
    $prefixAllowed.Add($normalizedPrefix) | Out-Null
}

foreach ($scenarioPolicy in @(Get-ArrayProperty -Object $policy -PropertyName "scenarioAllowlists")) {
    if (("" + $scenarioPolicy.scenario).Equals($Scenario, [System.StringComparison]::OrdinalIgnoreCase)) {
        foreach ($path in @(Get-ArrayProperty -Object $scenarioPolicy -PropertyName "allowedPaths")) {
            $exactAllowed.Add((ConvertTo-ScopeRelativePath -Path ("" + $path))) | Out-Null
        }
        foreach ($prefix in @(Get-ArrayProperty -Object $scenarioPolicy -PropertyName "allowedPathPrefixes")) {
            $normalizedPrefix = ConvertTo-ScopeRelativePath -Path ("" + $prefix)
            if (-not $normalizedPrefix.EndsWith("/", [System.StringComparison]::Ordinal)) {
                $normalizedPrefix += "/"
            }
            $prefixAllowed.Add($normalizedPrefix) | Out-Null
        }
        foreach ($path in @(Get-ArrayProperty -Object $scenarioPolicy -PropertyName "allowedDeletedPaths")) {
            $deletedExactAllowed.Add((ConvertTo-ScopeRelativePath -Path ("" + $path))) | Out-Null
        }
        foreach ($prefix in @(Get-ArrayProperty -Object $scenarioPolicy -PropertyName "allowedDeletionPathPrefixes")) {
            $normalizedPrefix = ConvertTo-ScopeRelativePath -Path ("" + $prefix)
            if (-not $normalizedPrefix.EndsWith("/", [System.StringComparison]::Ordinal)) { $normalizedPrefix += "/" }
            $deletedPrefixAllowed.Add($normalizedPrefix) | Out-Null
        }
    }
}

$changes = @(Get-GitChangedPaths)
$classified = New-Object System.Collections.Generic.List[object]
$diagnostics = New-Object System.Collections.Generic.List[object]
$violationCount = 0
$warningCount = 0
$allowedCount = 0

foreach ($change in $changes) {
    $classification = Classify-ChangedPath -Change $change -Policy $policy -ExactAllowed @($exactAllowed) -PrefixAllowed @($prefixAllowed) -DeletedExactAllowed @($deletedExactAllowed) -DeletedPrefixAllowed @($deletedPrefixAllowed)
    if ($classification.accepted) {
        $allowedCount++
    }
    elseif ($classification.severity -eq "warning") {
        $warningCount++
    }
    else {
        $violationCount++
    }

    $classified.Add([ordered]@{
        path = $change.path
        git_status = $change.status
        tracked = [bool]$change.tracked
        accepted = [bool]$classification.accepted
        severity = $classification.severity
        category = $classification.category
        code = $classification.code
        message = $classification.message
    }) | Out-Null

    if ($classification.severity -ne "info") {
        $diagnostics.Add([ordered]@{
            severity = $classification.severity
            code = $classification.code
            path = $change.path
            category = $classification.category
            message = $classification.message
        }) | Out-Null
    }
}

$accepted = ($violationCount -eq 0)
$report = [ordered]@{
    schemaVersion = "artifact_scope_report_v1"
    scenario = $Scenario
    accepted = $accepted
    policyPath = ConvertTo-ScopeRelativePath -Path $PolicyPath
    baselineRef = $BaselineRef
    failOnTrackedIgnored = [bool]$FailOnTrackedIgnored
    changedPathCount = $changes.Count
    allowedCount = $allowedCount
    warningCount = $warningCount
    violationCount = $violationCount
    diagnostics = $diagnostics
    changedPaths = $classified
}

$json = $report | ConvertTo-Json -Depth 10
if (-not [string]::IsNullOrWhiteSpace($ReportDirectory)) {
    New-Item -ItemType Directory -Force -Path $ReportDirectory | Out-Null
    $safeScenario = ($Scenario -replace '[^A-Za-z0-9_.-]', '-')
    $jsonPath = Join-Path $ReportDirectory "$safeScenario-artifact-scope-report.json"
    $markdownPath = Join-Path $ReportDirectory "$safeScenario-artifact-scope-report.md"
    Write-DevflowUtf8File -Path $jsonPath -Content ($json + [Environment]::NewLine)

    $lines = @(
        "# Artifact Scope Report",
        "",
        "- Scenario: $Scenario",
        "- Accepted: $accepted",
        "- Changed paths: $($changes.Count)",
        "- Allowed: $allowedCount",
        "- Warnings: $warningCount",
        "- Violations: $violationCount",
        ""
    )
    foreach ($entry in $classified) {
        $lines += "- $($entry.severity) $($entry.category): $($entry.path)"
    }
    Write-DevflowUtf8File -Path $markdownPath -Content (($lines -join [Environment]::NewLine) + [Environment]::NewLine)
}

Write-Output $json
if (-not $accepted) {
    exit 1
}
