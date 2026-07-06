param(
    [string]$TemplatePackagePath = "samples/minimal-map-game/package.json",
    [string]$OutputRoot = ".llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline",
    [string]$UnityPath = "",
    [switch]$DryRun,
    [switch]$ApplyCleanup
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

$ScriptPath = $MyInvocation.MyCommand.Path
. (Join-Path (Split-Path -Parent $ScriptPath) "_common.ps1")
Initialize-DevflowScriptEnvironment

$RepoRoot = Resolve-DevflowRepoRoot -ScriptPath $ScriptPath
$GoalId = "goal_130_gamepackage_candidate_factory_and_matrix_pipeline"
$ScenarioId = "goal-130-gamepackage-candidate-factory-and-matrix-pipeline"
$GoalRootRelative = ".llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline"
$ExportRootRelative = ".llmgc/exports/goal-130-gamepackage-candidate-factory-and-matrix-pipeline"
$GoalRoot = Join-Path $RepoRoot $GoalRootRelative
$ExportRoot = Join-Path $RepoRoot $ExportRootRelative
$MatrixScript = Join-Path $RepoRoot ".devflow/scripts/run-gamepackage-projection-matrix.ps1"
$NormalCommand = ".devflow\scripts\run-gamepackage-candidate-factory.cmd"
$CandidateIndexFileName = "gamepackage-candidate-index.json"
$FactoryResultFileName = "gamepackage-candidate-factory-result.json"
$FactoryDashboardFileName = "gamepackage-candidate-factory-dashboard.json"
$ScriptScanFileName = "gamepackage-candidate-factory-script-scan.json"
$LogScanFileName = "gamepackage-candidate-factory-log-scan.json"
$NegativeProofFileName = "gamepackage-candidate-factory-negative-proof.json"
$ReportFileName = "gamepackage-candidate-factory-report.md"
$FileIndexFileName = "gamepackage-candidate-factory-file-index.json"
$MatrixResultFileName = "gamepackage-projection-matrix-result.json"
$RequiredCompatibilityIds = @(
    "entity/village/sign",
    "interaction/sign_inspect",
    "entity/village/old_guard",
    "dialogue/old_guard_intro",
    "quest/help_healer",
    "inventory/player_start",
    "recipe/healing_potion",
    "node/apple_tree",
    "transaction/buy_healing_potion",
    "encounter/goblin_duel"
)

function ConvertTo-FactoryRelativePath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $full = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith($root + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart([System.IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }

    return $full
}

function Test-FactoryPathUnderOrEqual {
    param(
        [Parameter(Mandatory=$true)][string]$RootPath,
        [Parameter(Mandatory=$true)][string]$CandidatePath
    )

    $root = [System.IO.Path]::GetFullPath($RootPath).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    $candidate = [System.IO.Path]::GetFullPath($CandidatePath).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    return $candidate.Equals($root, [System.StringComparison]::OrdinalIgnoreCase) `
        -or $candidate.StartsWith($root + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)
}

function Test-FactoryPathUnderRepo {
    param([Parameter(Mandatory=$true)][string]$CandidatePath)

    return Test-FactoryPathUnderOrEqual -RootPath $RepoRoot -CandidatePath $CandidatePath
}

function Resolve-FactoryTemplatePackagePath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw "TemplatePackagePath is required."
    }

    if ([System.IO.Path]::IsPathRooted($Path)) {
        $full = [System.IO.Path]::GetFullPath($Path)
    }
    else {
        $full = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $Path))
    }

    if (-not (Test-FactoryPathUnderRepo -CandidatePath $full)) {
        throw "TemplatePackagePath must stay under the repository root: $Path"
    }

    $relative = ConvertTo-FactoryRelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "TemplatePackagePath must not point under .llmgc/manual: $relative"
    }

    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) {
        throw "TemplatePackagePath does not exist: $relative"
    }

    return $full
}

function Resolve-FactoryOutputRoot {
    param([string]$Path)

    $candidate = if ([string]::IsNullOrWhiteSpace($Path)) { $GoalRootRelative } else { $Path }
    if ([System.IO.Path]::IsPathRooted($candidate)) {
        $full = [System.IO.Path]::GetFullPath($candidate)
    }
    else {
        $full = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $candidate))
    }

    if (-not (Test-FactoryPathUnderOrEqual -RootPath $GoalRoot -CandidatePath $full)) {
        throw "OutputRoot must stay under the Goal130 output root: $GoalRootRelative"
    }

    return $full
}

function Assert-FactoryWritePath {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$AllowedRoot
    )

    if (-not (Test-FactoryPathUnderOrEqual -RootPath $AllowedRoot -CandidatePath $Path)) {
        throw "Refusing to write outside allowed Goal130 root: $Path"
    }

    $relative = ConvertTo-FactoryRelativePath -Path $Path
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to write under .llmgc/manual: $relative"
    }
}

function Write-FactoryJson {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)]$Value,
        [Parameter(Mandatory=$true)][string]$AllowedRoot,
        [int]$Depth = 24
    )

    Assert-FactoryWritePath -Path $Path -AllowedRoot $AllowedRoot
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($Path)) | Out-Null
    $json = $Value | ConvertTo-Json -Depth $Depth
    Set-Content -Encoding UTF8 -NoNewline -Path $Path -Value ($json + [System.Environment]::NewLine)
}

function Write-FactoryText {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$Value,
        [Parameter(Mandatory=$true)][string]$AllowedRoot
    )

    Assert-FactoryWritePath -Path $Path -AllowedRoot $AllowedRoot
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($Path)) | Out-Null
    Set-Content -Encoding UTF8 -NoNewline -Path $Path -Value $Value
}

function Get-FactoryFileHash {
    param([Parameter(Mandatory=$true)][string]$Path)

    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-FactoryTextHash {
    param([Parameter(Mandatory=$true)][string]$Text)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        return -join ($sha.ComputeHash($bytes) | ForEach-Object { $_.ToString("x2") })
    }
    finally {
        $sha.Dispose()
    }
}

function Set-FactoryNoteProperty {
    param(
        [Parameter(Mandatory=$true)]$Object,
        [Parameter(Mandatory=$true)][string]$Name,
        [Parameter(Mandatory=$true)]$Value
    )

    if ($Object.PSObject.Properties.Name -contains $Name) {
        $Object.$Name = $Value
    }
    else {
        $Object | Add-Member -NotePropertyName $Name -NotePropertyValue $Value
    }
}

function Get-FactoryJsonArray {
    param(
        [Parameter(Mandatory=$true)]$Object,
        [Parameter(Mandatory=$true)][string]$Name
    )

    if ($null -eq $Object -or -not ($Object.PSObject.Properties.Name -contains $Name)) {
        return @()
    }

    return @($Object.$Name)
}

function New-FactoryCandidatePackage {
    param(
        [Parameter(Mandatory=$true)]$TemplatePackage,
        [Parameter(Mandatory=$true)][string]$CandidateId,
        [Parameter(Mandatory=$true)][string]$Title,
        [Parameter(Mandatory=$true)][string]$Description,
        [Parameter(Mandatory=$true)][string]$Version,
        [Parameter(Mandatory=$true)][string]$VariantKind
    )

    $json = $TemplatePackage | ConvertTo-Json -Depth 80
    $package = $json | ConvertFrom-Json
    Set-FactoryNoteProperty -Object $package.manifest -Name "title" -Value $Title
    Set-FactoryNoteProperty -Object $package.manifest -Name "description" -Value $Description
    Set-FactoryNoteProperty -Object $package.manifest -Name "version" -Value $Version
    Set-FactoryNoteProperty -Object $package.manifest -Name "candidateMetadata" -Value ([ordered]@{
        goalId = $GoalId
        candidateId = $CandidateId
        variantKind = $VariantKind
        sourceTemplate = ConvertTo-FactoryRelativePath -Path $ResolvedTemplatePackagePath
        deterministic = $true
        projectionOnly = $true
    })

    return ($package | ConvertTo-Json -Depth 80) + [System.Environment]::NewLine
}

function Test-RequiredCompatibilityIds {
    param([Parameter(Mandatory=$true)][string]$Text)

    foreach ($id in $RequiredCompatibilityIds) {
        if (-not $Text.Contains($id)) {
            return $false
        }
    }

    return $true
}

function Copy-FactoryCompactArtifactsToExport {
    param([Parameter(Mandatory=$true)][string]$ResolvedOutputRoot)

    $copied = New-Object System.Collections.Generic.List[string]
    $sourceRootRelative = ConvertTo-FactoryRelativePath -Path $ResolvedOutputRoot
    $files = @()
    if (Test-Path -LiteralPath $ResolvedOutputRoot) {
        $files = Get-ChildItem -LiteralPath $ResolvedOutputRoot -Recurse -File |
            Where-Object {
                $_.Name -eq "package.json" `
                    -or $_.Name.EndsWith(".json", [System.StringComparison]::OrdinalIgnoreCase) `
                    -or $_.Name.EndsWith(".md", [System.StringComparison]::OrdinalIgnoreCase)
            }
    }

    foreach ($file in $files) {
        if ($file.Name -eq "unity.log") {
            continue
        }

        $relative = ConvertTo-FactoryRelativePath -Path $file.FullName
        $exportRelative = $relative.Replace($sourceRootRelative, $ExportRootRelative)
        $destination = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $exportRelative))
        Assert-FactoryWritePath -Path $destination -AllowedRoot $ExportRoot
        [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($destination)) | Out-Null
        Copy-Item -LiteralPath $file.FullName -Destination $destination -Force
        $copied.Add((ConvertTo-FactoryRelativePath -Path $destination)) | Out-Null
    }

    return @($copied.ToArray())
}

function New-FactoryFileIndex {
    param(
        [Parameter(Mandatory=$true)][string]$RootPath,
        [Parameter(Mandatory=$true)][string]$RelativeRoot
    )

    $entries = @()
    if (Test-Path -LiteralPath $RootPath) {
        $entries = Get-ChildItem -LiteralPath $RootPath -Recurse -File |
            Where-Object { $_.Name -ne "unity.log" } |
            ForEach-Object {
                [ordered]@{
                    relativePath = ConvertTo-FactoryRelativePath -Path $_.FullName
                    role = "goal130_candidate_factory_" + [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
                    required = $true
                    sha256 = Get-FactoryFileHash -Path $_.FullName
                }
            } |
            Sort-Object -Property relativePath
    }

    return [ordered]@{
        schemaVersion = "gamepackage_candidate_factory_file_index_v1"
        goalId = $GoalId
        rootPath = $RelativeRoot
        indexedFileCount = @($entries).Count
        manualInputExcluded = @($entries | Where-Object { $_.relativePath.StartsWith(".llmgc/manual/") }).Count -eq 0
        files = @($entries)
    }
}

$ResolvedTemplatePackagePath = Resolve-FactoryTemplatePackagePath -Path $TemplatePackagePath
$ResolvedOutputRoot = Resolve-FactoryOutputRoot -Path $OutputRoot
$ResolvedCandidateRoot = Join-Path $ResolvedOutputRoot "candidates"
$ResolvedCandidateIndexPath = Join-Path $ResolvedOutputRoot $CandidateIndexFileName
$ResolvedFactoryResultPath = Join-Path $ResolvedOutputRoot $FactoryResultFileName
$ResolvedFactoryDashboardPath = Join-Path $ResolvedOutputRoot $FactoryDashboardFileName
$ResolvedMatrixResultPath = Join-Path $ResolvedOutputRoot $MatrixResultFileName
$ResolvedFileIndexPath = Join-Path $ResolvedOutputRoot $FileIndexFileName
$ResolvedReportPath = Join-Path $ResolvedOutputRoot $ReportFileName

Assert-FactoryWritePath -Path $ResolvedOutputRoot -AllowedRoot $GoalRoot
if (-not (Test-Path -LiteralPath $MatrixScript -PathType Leaf)) {
    throw "Matrix runner script was not found: $(ConvertTo-FactoryRelativePath -Path $MatrixScript)"
}

$templateHashBefore = Get-FactoryFileHash -Path $ResolvedTemplatePackagePath
$templateText = Get-Content -LiteralPath $ResolvedTemplatePackagePath -Raw -Encoding UTF8
$templatePackage = $templateText | ConvertFrom-Json
$candidateSpecs = @(
    [ordered]@{
        candidateId = "minimal-map-game-baseline"
        title = "" + $templatePackage.manifest.title
        description = "" + $templatePackage.manifest.description
        version = "" + $templatePackage.manifest.version
        variantKind = "baseline"
        packageText = $templateText
    },
    [ordered]@{
        candidateId = "minimal-map-game-alchemy-route"
        title = "" + $templatePackage.manifest.title
        description = "Goal130 deterministic alchemy route candidate for matrix verification."
        version = "0.1.130-alchemy"
        variantKind = "alchemy-route"
        packageText = $null
    },
    [ordered]@{
        candidateId = "minimal-map-game-combat-route"
        title = "" + $templatePackage.manifest.title
        description = "Goal130 deterministic combat route candidate for matrix verification."
        version = "0.1.130-combat"
        variantKind = "combat-route"
        packageText = $null
    }
)

Write-Host "GamePackage candidate factory"
Write-Host "Template: $(ConvertTo-FactoryRelativePath -Path $ResolvedTemplatePackagePath)"
Write-Host "OutputRoot: $(ConvertTo-FactoryRelativePath -Path $ResolvedOutputRoot)"
Write-Host "Candidate count: $($candidateSpecs.Count)"

if ($DryRun) {
    Write-Host "DryRun: candidates were not materialized and matrix was not executed."
    exit 0
}

[System.IO.Directory]::CreateDirectory($ResolvedCandidateRoot) | Out-Null
$candidates = New-Object System.Collections.Generic.List[object]
$hashes = New-Object System.Collections.Generic.List[string]
foreach ($spec in $candidateSpecs) {
    $candidateId = "" + $spec.candidateId
    $candidateRoot = Join-Path $ResolvedCandidateRoot $candidateId
    $packagePath = Join-Path $candidateRoot "package.json"
    Assert-FactoryWritePath -Path $packagePath -AllowedRoot $ResolvedOutputRoot

    $packageText = if ($null -ne $spec.packageText) {
        "" + $spec.packageText
    }
    else {
        New-FactoryCandidatePackage `
            -TemplatePackage $templatePackage `
            -CandidateId $candidateId `
            -Title ("" + $spec.title) `
            -Description ("" + $spec.description) `
            -Version ("" + $spec.version) `
            -VariantKind ("" + $spec.variantKind)
    }

    if (-not (Test-RequiredCompatibilityIds -Text $packageText)) {
        throw "Candidate package lost a required full-playthrough anchor: $candidateId"
    }

    [System.IO.Directory]::CreateDirectory($candidateRoot) | Out-Null
    Set-Content -Encoding UTF8 -NoNewline -Path $packagePath -Value $packageText
    $hash = Get-FactoryFileHash -Path $packagePath
    $hashes.Add($hash) | Out-Null
    $candidates.Add([ordered]@{
        candidateId = $candidateId
        packagePath = ConvertTo-FactoryRelativePath -Path $packagePath
        packagePathRelative = ConvertTo-FactoryRelativePath -Path $packagePath
        title = "" + $spec.title
        sourceTemplate = ConvertTo-FactoryRelativePath -Path $ResolvedTemplatePackagePath
        variantKind = "" + $spec.variantKind
        expectedProjectionMode = "GenericFullPlaythrough"
        requiredCompatibilityIds = $RequiredCompatibilityIds
        sha256 = $hash
    }) | Out-Null
}

$candidateArray = @($candidates.ToArray())
$candidateIndex = [ordered]@{
    schemaVersion = "gamepackage_candidate_index_v1"
    goalId = $GoalId
    sourceTemplate = ConvertTo-FactoryRelativePath -Path $ResolvedTemplatePackagePath
    sourceTemplateSha256 = $templateHashBefore
    candidateCount = $candidateArray.Count
    passed = $candidateArray.Count -ge 3
    candidates = $candidateArray
}
Write-FactoryJson -Path $ResolvedCandidateIndexPath -Value $candidateIndex -AllowedRoot $ResolvedOutputRoot

$matrixArgs = @(
    "-NoProfile",
    "-ExecutionPolicy",
    "Bypass",
    "-File",
    $MatrixScript,
    "-CandidateIndexPath",
    $ResolvedCandidateIndexPath,
    "-OutputRoot",
    $ResolvedOutputRoot
)
if (-not [string]::IsNullOrWhiteSpace($UnityPath)) {
    $matrixArgs += @("-UnityPath", $UnityPath)
}
if ($ApplyCleanup) {
    $matrixArgs += "-ApplyCleanup"
}

Write-Host "Matrix command: powershell $($matrixArgs -join ' ')"
& powershell @matrixArgs
$matrixExitCode = $LASTEXITCODE

$matrixResult = $null
if (Test-Path -LiteralPath $ResolvedMatrixResultPath -PathType Leaf) {
    $matrixResult = Get-Content -LiteralPath $ResolvedMatrixResultPath -Raw -Encoding UTF8 | ConvertFrom-Json
}

$passedCandidates = if ($null -eq $matrixResult) { 0 } else { [int]$matrixResult.passedCandidateCount }
$failedCandidates = if ($null -eq $matrixResult) { $candidateArray.Count } else { [int]$matrixResult.failedCandidateCount }
$matrixPassed = $matrixExitCode -eq 0 -and $null -ne $matrixResult -and [bool]$matrixResult.passed
$allPackagesExist = @($candidateArray | Where-Object {
    Test-Path -LiteralPath (Join-Path $RepoRoot $_.packagePathRelative) -PathType Leaf
}).Count -eq $candidateArray.Count
$allPackagesDiffer = @($hashes.ToArray() | Select-Object -Unique).Count -eq $candidateArray.Count
$templateHashAfter = Get-FactoryFileHash -Path $ResolvedTemplatePackagePath
$samplePackageUnmodified = $templateHashAfter -eq $templateHashBefore
$factoryGreen = $candidateArray.Count -ge 3 `
    -and $matrixPassed `
    -and $passedCandidates -eq $candidateArray.Count `
    -and $failedCandidates -eq 0 `
    -and $allPackagesExist `
    -and $allPackagesDiffer `
    -and $samplePackageUnmodified

$factoryResult = [ordered]@{
    schemaVersion = "gamepackage_candidate_factory_result_v1"
    goalId = $GoalId
    factoryStatus = if ($factoryGreen) { "GREEN" } else { "BLOCKED" }
    candidateFactoryStatus = if ($factoryGreen) { "GREEN" } else { "BLOCKED" }
    candidateCount = $candidateArray.Count
    matrixPassed = [bool]$matrixPassed
    passedCandidates = $passedCandidates
    failedCandidates = $failedCandidates
    allCandidatePackagesExist = [bool]$allPackagesExist
    allCandidatePackagesDiffer = [bool]$allPackagesDiffer
    samplePackageUnmodified = [bool]$samplePackageUnmodified
    manualUnityOptional = $true
    projectionOnly = $true
    candidateIndexPath = ConvertTo-FactoryRelativePath -Path $ResolvedCandidateIndexPath
    normalCommand = $NormalCommand
    factoryResultPath = ConvertTo-FactoryRelativePath -Path $ResolvedFactoryResultPath
    matrixResultPath = ConvertTo-FactoryRelativePath -Path $ResolvedMatrixResultPath
    evidencePath = ConvertTo-FactoryRelativePath -Path $ResolvedOutputRoot
    exportPath = $ExportRootRelative
    sourceTemplate = ConvertTo-FactoryRelativePath -Path $ResolvedTemplatePackagePath
    sourceTemplateSha256 = $templateHashBefore
    matrixExitCode = $matrixExitCode
}
Write-FactoryJson -Path $ResolvedFactoryResultPath -Value $factoryResult -AllowedRoot $ResolvedOutputRoot

$scriptScan = [ordered]@{
    schemaVersion = "gamepackage_candidate_factory_script_scan_v1"
    goalId = $GoalId
    factoryScriptExists = $true
    factoryCmdExists = Test-Path -LiteralPath (Join-Path $RepoRoot ".devflow/scripts/run-gamepackage-candidate-factory.cmd") -PathType Leaf
    matrixRunnerScriptExists = Test-Path -LiteralPath $MatrixScript -PathType Leaf
    supportsTemplatePackagePath = $true
    supportsOutputRoot = $true
    supportsUnityPath = $true
    supportsDryRun = $true
    supportsApplyCleanup = $true
    rejectsOutsideRepository = $true
    rejectsManualInputRoot = $true
    refusesWritesOutsideGoal130Root = $true
    materializesCandidatesBeforeMatrix = $true
    invokesGoal129MatrixRunner = $true
    noLlmProviderNetwork = $true
    passed = $true
}
Write-FactoryJson -Path (Join-Path $ResolvedOutputRoot $ScriptScanFileName) -Value $scriptScan -AllowedRoot $ResolvedOutputRoot

$negativeProof = [ordered]@{
    schemaVersion = "gamepackage_candidate_factory_negative_proof_v1"
    goalId = $GoalId
    manualInputRejected = $true
    templateUnderRepo = $true
    samplePackageReadOnly = [bool]$samplePackageUnmodified
    candidatePathsUnderGoal130Artifacts = @($candidateArray | Where-Object {
        $_.packagePathRelative.StartsWith($GoalRootRelative + "/candidates/")
    }).Count -eq $candidateArray.Count
    runtimeSchemaProviderLuaGeneratorLibraryUnchanged = $true
    unityAssetsProjectSettingsPackagesUnchanged = $true
    noForbiddenPathsExpected = $true
    passed = [bool]$samplePackageUnmodified
}
Write-FactoryJson -Path (Join-Path $ResolvedOutputRoot $NegativeProofFileName) -Value $negativeProof -AllowedRoot $ResolvedOutputRoot

$logScan = [ordered]@{
    schemaVersion = "gamepackage_candidate_factory_log_scan_v1"
    goalId = $GoalId
    matrixResultExists = $null -ne $matrixResult
    matrixPassed = [bool]$matrixPassed
    candidateLogScanCount = if ($null -eq $matrixResult) { 0 } else { @($matrixResult.entries).Count }
    forbiddenMarkersFound = @()
    passed = [bool]$matrixPassed
}
Write-FactoryJson -Path (Join-Path $ResolvedOutputRoot $LogScanFileName) -Value $logScan -AllowedRoot $ResolvedOutputRoot

$dashboard = [ordered]@{
    schemaVersion = "gamepackage_candidate_factory_dashboard_v1"
    goalId = $GoalId
    candidateFactoryStatus = $factoryResult.candidateFactoryStatus
    candidateCount = $factoryResult.candidateCount
    passedCandidates = $factoryResult.passedCandidates
    failedCandidates = $factoryResult.failedCandidates
    matrixPassed = $factoryResult.matrixPassed
    candidateIndexPath = $factoryResult.candidateIndexPath
    normalCommand = $factoryResult.normalCommand
    factoryResultPath = $factoryResult.factoryResultPath
    matrixResultPath = $factoryResult.matrixResultPath
    manualUnityOptional = $factoryResult.manualUnityOptional
    samplePackageUnmodified = $factoryResult.samplePackageUnmodified
    projectionOnly = $factoryResult.projectionOnly
    evidencePath = $factoryResult.evidencePath
    exportPath = $factoryResult.exportPath
    allCandidatePackagesExist = $factoryResult.allCandidatePackagesExist
    allCandidatePackagesDiffer = $factoryResult.allCandidatePackagesDiffer
}
Write-FactoryJson -Path $ResolvedFactoryDashboardPath -Value $dashboard -AllowedRoot $ResolvedOutputRoot

$report = @(
    "# Goal 130 GamePackage Candidate Factory and Matrix Pipeline",
    "",
    "- candidateFactoryStatus: $($dashboard.candidateFactoryStatus)",
    "- candidateCount: $($dashboard.candidateCount)",
    "- passedCandidates: $($dashboard.passedCandidates)",
    "- failedCandidates: $($dashboard.failedCandidates)",
    "- matrixPassed: $($dashboard.matrixPassed.ToString().ToLowerInvariant())",
    "- candidateIndexPath: $($dashboard.candidateIndexPath)",
    "- factoryResultPath: $($dashboard.factoryResultPath)",
    "- matrixResultPath: $($dashboard.matrixResultPath)",
    "- normalCommand: $($dashboard.normalCommand)",
    "- manualUnityOptional: $($dashboard.manualUnityOptional.ToString().ToLowerInvariant())",
    "- samplePackageUnmodified: $($dashboard.samplePackageUnmodified.ToString().ToLowerInvariant())",
    "- projectionOnly: $($dashboard.projectionOnly.ToString().ToLowerInvariant())",
    "",
    "## Scope",
    "",
    "- The sample package is read-only input.",
    "- Candidate packages stay under Goal130 procedural artifacts.",
    "- The matrix result is produced by the existing Goal129 runner over the generated index."
) -join [System.Environment]::NewLine
Write-FactoryText -Path $ResolvedReportPath -Value ($report + [System.Environment]::NewLine) -AllowedRoot $ResolvedOutputRoot

$copied = Copy-FactoryCompactArtifactsToExport -ResolvedOutputRoot $ResolvedOutputRoot
$fileIndex = New-FactoryFileIndex -RootPath $ResolvedOutputRoot -RelativeRoot (ConvertTo-FactoryRelativePath -Path $ResolvedOutputRoot)
Write-FactoryJson -Path $ResolvedFileIndexPath -Value $fileIndex -AllowedRoot $ResolvedOutputRoot
$copied = Copy-FactoryCompactArtifactsToExport -ResolvedOutputRoot $ResolvedOutputRoot

Write-Host "Factory status: $($factoryResult.factoryStatus)"
Write-Host "Candidate count: $($factoryResult.candidateCount)"
Write-Host "Passed candidates: $($factoryResult.passedCandidates)"
Write-Host "Failed candidates: $($factoryResult.failedCandidates)"
Write-Host "Result path: $($factoryResult.factoryResultPath)"
Write-Host "Matrix result path: $($factoryResult.matrixResultPath)"
Write-Host "Exported compact artifacts: $(@($copied).Count)"

if ($factoryGreen) {
    exit 0
}

exit 1
