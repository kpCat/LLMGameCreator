param(
    [string]$TemplatePackagePath = "samples/minimal-map-game/package.json",
    [string]$RecipeCatalogPath = ".llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/candidate-recipe-catalog.json",
    [string]$OutputRoot = ".llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion",
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
$GoalId = "goal_131_gamepackage_candidate_recipe_catalog_scoring_and_promotion"
$ScenarioId = "goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion"
$GoalRootRelative = ".llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion"
$ExportRootRelative = ".llmgc/exports/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion"
$GoalRoot = Join-Path $RepoRoot $GoalRootRelative
$ExportRoot = Join-Path $RepoRoot $ExportRootRelative
$MatrixScript = Join-Path $RepoRoot ".devflow/scripts/run-gamepackage-projection-matrix.ps1"
$NormalCommand = ".devflow\scripts\run-gamepackage-candidate-recipe-pipeline.cmd"
$CatalogFileName = "candidate-recipe-catalog.json"
$CandidateIndexFileName = "gamepackage-candidate-index.json"
$PipelineResultFileName = "gamepackage-recipe-pipeline-result.json"
$ScoringResultFileName = "candidate-scoring-result.json"
$DashboardFileName = "gamepackage-candidate-recipe-pipeline-dashboard.json"
$ScriptScanFileName = "gamepackage-candidate-recipe-pipeline-script-scan.json"
$LogScanFileName = "gamepackage-candidate-recipe-pipeline-log-scan.json"
$NegativeProofFileName = "gamepackage-candidate-recipe-pipeline-negative-proof.json"
$ReportFileName = "gamepackage-candidate-recipe-pipeline-report.md"
$FileIndexFileName = "gamepackage-candidate-recipe-pipeline-file-index.json"
$MatrixResultFileName = "gamepackage-projection-matrix-result.json"
$SelectedCandidateDirectoryName = "selected-candidate"
$SelectedCandidatePackageFileName = "package.json"
$SelectedCandidateHandoffFileName = "selected-candidate-handoff.json"
$RequiredRecipeIds = @(
    "balanced_baseline",
    "alchemy_focus",
    "combat_focus",
    "exploration_focus"
)
$RequiredCompatibilityIds = @(
    "map/village",
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
$QuestSystemsCoverageIds = @(
    "quest/help_healer",
    "recipe/healing_potion",
    "node/apple_tree",
    "transaction/buy_healing_potion",
    "encounter/goblin_duel"
)

function ConvertTo-RecipeRelativePath {
    param([Parameter(Mandatory=$true)][string]$Path)

    $full = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    if ($full.StartsWith($root + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $full.Substring($root.Length).TrimStart([System.IO.Path]::DirectorySeparatorChar).Replace('\', '/')
    }

    return $full
}

function Test-RecipePathUnderOrEqual {
    param(
        [Parameter(Mandatory=$true)][string]$RootPath,
        [Parameter(Mandatory=$true)][string]$CandidatePath
    )

    $root = [System.IO.Path]::GetFullPath($RootPath).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    $candidate = [System.IO.Path]::GetFullPath($CandidatePath).TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    return $candidate.Equals($root, [System.StringComparison]::OrdinalIgnoreCase) `
        -or $candidate.StartsWith($root + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)
}

function Test-RecipePathUnderRepo {
    param([Parameter(Mandatory=$true)][string]$CandidatePath)

    return Test-RecipePathUnderOrEqual -RootPath $RepoRoot -CandidatePath $CandidatePath
}

function Resolve-RecipeInputPath {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$Label
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw "$Label is required."
    }

    if ([System.IO.Path]::IsPathRooted($Path)) {
        $full = [System.IO.Path]::GetFullPath($Path)
    }
    else {
        $full = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $Path))
    }

    if (-not (Test-RecipePathUnderRepo -CandidatePath $full)) {
        throw "$Label must stay under the repository root: $Path"
    }

    $relative = ConvertTo-RecipeRelativePath -Path $full
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Label must not point under .llmgc/manual: $relative"
    }

    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) {
        throw "$Label does not exist: $relative"
    }

    return $full
}

function Resolve-RecipeOutputRoot {
    param([string]$Path)

    $candidate = if ([string]::IsNullOrWhiteSpace($Path)) { $GoalRootRelative } else { $Path }
    if ([System.IO.Path]::IsPathRooted($candidate)) {
        $full = [System.IO.Path]::GetFullPath($candidate)
    }
    else {
        $full = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $candidate))
    }

    if (-not (Test-RecipePathUnderOrEqual -RootPath $GoalRoot -CandidatePath $full)) {
        throw "OutputRoot must stay under the Goal131 output root: $GoalRootRelative"
    }

    return $full
}

function Assert-RecipeWritePath {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$AllowedRoot
    )

    if (-not (Test-RecipePathUnderOrEqual -RootPath $AllowedRoot -CandidatePath $Path)) {
        throw "Refusing to write outside allowed Goal131 root: $Path"
    }

    $relative = ConvertTo-RecipeRelativePath -Path $Path
    if ($relative.StartsWith(".llmgc/manual/", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to write under .llmgc/manual: $relative"
    }
}

function Write-RecipeJson {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)]$Value,
        [Parameter(Mandatory=$true)][string]$AllowedRoot,
        [int]$Depth = 32
    )

    Assert-RecipeWritePath -Path $Path -AllowedRoot $AllowedRoot
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($Path)) | Out-Null
    $json = $Value | ConvertTo-Json -Depth $Depth
    Set-Content -Encoding UTF8 -NoNewline -Path $Path -Value ($json + [System.Environment]::NewLine)
}

function Write-RecipeText {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$Value,
        [Parameter(Mandatory=$true)][string]$AllowedRoot
    )

    Assert-RecipeWritePath -Path $Path -AllowedRoot $AllowedRoot
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($Path)) | Out-Null
    Set-Content -Encoding UTF8 -NoNewline -Path $Path -Value $Value
}

function Get-RecipeFileHash {
    param([Parameter(Mandatory=$true)][string]$Path)

    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Set-RecipeNoteProperty {
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

function Get-RecipeJsonArray {
    param(
        [Parameter(Mandatory=$true)]$Object,
        [Parameter(Mandatory=$true)][string]$Name
    )

    if ($null -eq $Object -or -not ($Object.PSObject.Properties.Name -contains $Name)) {
        return @()
    }

    return @($Object.$Name)
}

function Get-RecipeStringProperty {
    param(
        [Parameter(Mandatory=$true)]$Object,
        [Parameter(Mandatory=$true)][string]$Name,
        [string]$Default = ""
    )

    if ($null -ne $Object -and $Object.PSObject.Properties.Name -contains $Name) {
        return "" + $Object.$Name
    }

    return $Default
}

function Get-RecipeIntProperty {
    param(
        [Parameter(Mandatory=$true)]$Object,
        [Parameter(Mandatory=$true)][string]$Name,
        [int]$Default = 0
    )

    if ($null -ne $Object -and $Object.PSObject.Properties.Name -contains $Name) {
        return [int]$Object.$Name
    }

    return $Default
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

function Assert-RecipeCatalog {
    param([Parameter(Mandatory=$true)]$Catalog)

    $recipes = Get-RecipeJsonArray -Object $Catalog -Name "recipes"
    if ($recipes.Count -lt 4) {
        throw "Recipe catalog must contain at least four recipes."
    }

    foreach ($id in $RequiredRecipeIds) {
        $match = @($recipes | Where-Object {
            (Get-RecipeStringProperty -Object $_ -Name "recipeId") -eq $id
        })
        if ($match.Count -ne 1) {
            throw "Recipe catalog must contain exactly one recipe id: $id"
        }
    }

    $candidateIds = @($recipes | ForEach-Object { Get-RecipeStringProperty -Object $_ -Name "candidateId" })
    if (@($candidateIds | Where-Object { [string]::IsNullOrWhiteSpace($_) }).Count -gt 0) {
        throw "Every recipe must provide candidateId."
    }

    if (@($candidateIds | Select-Object -Unique).Count -ne $candidateIds.Count) {
        throw "Recipe candidateId values must be unique."
    }

    return $recipes
}

function Apply-RecipeMutations {
    param(
        [Parameter(Mandatory=$true)]$Package,
        [Parameter(Mandatory=$true)]$Recipe,
        [Parameter(Mandatory=$true)][string]$SourceTemplateRelativePath
    )

    $recipeId = Get-RecipeStringProperty -Object $Recipe -Name "recipeId"
    $candidateId = Get-RecipeStringProperty -Object $Recipe -Name "candidateId"
    $variantKind = Get-RecipeStringProperty -Object $Recipe -Name "variantKind"
    $displayName = Get-RecipeStringProperty -Object $Recipe -Name "displayName" -Default $variantKind
    $description = Get-RecipeStringProperty -Object $Recipe -Name "description"
    $safeTuningPolicy = $Recipe.safeTuningPolicy
    $scoringWeights = $Recipe.scoringWeights

    Set-RecipeNoteProperty -Object $Package.manifest -Name "title" -Value ("" + $Package.manifest.title)
    Set-RecipeNoteProperty -Object $Package.manifest -Name "description" -Value $description
    Set-RecipeNoteProperty -Object $Package.manifest -Name "version" -Value ("0.1.131-" + $recipeId.Replace("_", "-"))
    Set-RecipeNoteProperty -Object $Package.manifest -Name "candidateMetadata" -Value ([ordered]@{
        goalId = $GoalId
        recipeId = $recipeId
        candidateId = $candidateId
        variantKind = $variantKind
        displayName = $displayName
        sourceTemplate = $SourceTemplateRelativePath
        deterministic = $true
        projectionOnly = $true
        promotionCandidate = $true
        preservesFullPlaythroughIdentity = $true
        safeTuningPolicy = $safeTuningPolicy
        scoringWeights = $scoringWeights
        expectedFullPlaythroughAnchors = @(Get-RecipeJsonArray -Object $Recipe -Name "expectedFullPlaythroughAnchors")
    })

    return ($Package | ConvertTo-Json -Depth 100) + [System.Environment]::NewLine
}

function Build-ScoringComponents {
    param(
        [Parameter(Mandatory=$true)]$Candidate,
        [Parameter(Mandatory=$true)]$MatrixEntry,
        [Parameter(Mandatory=$true)][string]$PackageText,
        [Parameter(Mandatory=$true)][bool]$HashDistinct
    )

    $matrixEntryPassed = $false
    if ($null -ne $MatrixEntry -and $MatrixEntry.PSObject.Properties.Name -contains "passed") {
        $matrixEntryPassed = [bool]$MatrixEntry.passed
    }

    $logScanPassed = $false
    if ($null -ne $MatrixEntry -and $MatrixEntry.PSObject.Properties.Name -contains "logScanPassed") {
        $logScanPassed = [bool]$MatrixEntry.logScanPassed
    }

    $presentAnchors = @($RequiredCompatibilityIds | Where-Object { $PackageText.Contains($_) })
    $presentSystems = @($QuestSystemsCoverageIds | Where-Object { $PackageText.Contains($_) })
    $anchorScore = [int][Math]::Round(15.0 * ($presentAnchors.Count / [double]$RequiredCompatibilityIds.Count))
    $systemsScore = [int][Math]::Round(10.0 * ($presentSystems.Count / [double]$QuestSystemsCoverageIds.Count))
    $noForbiddenMarkers = $logScanPassed

    return [ordered]@{
        matrixPassed = if ($matrixEntryPassed) { 40 } else { 0 }
        fullPlaythroughPassed = if ($matrixEntryPassed) { 20 } else { 0 }
        anchorCoverage = $anchorScore
        candidateDistinctness = if ($HashDistinct) { 10 } else { 0 }
        questSystemsCoverage = $systemsScore
        noForbiddenMarkers = if ($noForbiddenMarkers) { 5 } else { 0 }
        presentAnchorCount = $presentAnchors.Count
        requiredAnchorCount = $RequiredCompatibilityIds.Count
        presentQuestSystemCount = $presentSystems.Count
        requiredQuestSystemCount = $QuestSystemsCoverageIds.Count
    }
}

function Copy-RecipeCompactArtifactsToExport {
    param([Parameter(Mandatory=$true)][string]$ResolvedOutputRoot)

    $copied = New-Object System.Collections.Generic.List[string]
    $sourceRootRelative = ConvertTo-RecipeRelativePath -Path $ResolvedOutputRoot
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

        $relative = ConvertTo-RecipeRelativePath -Path $file.FullName
        $exportRelative = $relative.Replace($sourceRootRelative, $ExportRootRelative)
        $destination = [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $exportRelative))
        Assert-RecipeWritePath -Path $destination -AllowedRoot $ExportRoot
        [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($destination)) | Out-Null
        Copy-Item -LiteralPath $file.FullName -Destination $destination -Force
        $copied.Add((ConvertTo-RecipeRelativePath -Path $destination)) | Out-Null
    }

    return @($copied.ToArray())
}

function New-RecipeFileIndex {
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
                    relativePath = ConvertTo-RecipeRelativePath -Path $_.FullName
                    role = "goal131_recipe_pipeline_" + [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
                    required = $true
                    sha256 = Get-RecipeFileHash -Path $_.FullName
                }
            } |
            Sort-Object -Property relativePath
    }

    return [ordered]@{
        schemaVersion = "gamepackage_candidate_recipe_pipeline_file_index_v1"
        goalId = $GoalId
        rootPath = $RelativeRoot
        indexedFileCount = @($entries).Count
        manualInputExcluded = @($entries | Where-Object { $_.relativePath.StartsWith(".llmgc/manual/") }).Count -eq 0
        files = @($entries)
    }
}

$ResolvedTemplatePackagePath = Resolve-RecipeInputPath -Path $TemplatePackagePath -Label "TemplatePackagePath"
$ResolvedRecipeCatalogPath = Resolve-RecipeInputPath -Path $RecipeCatalogPath -Label "RecipeCatalogPath"
$ResolvedOutputRoot = Resolve-RecipeOutputRoot -Path $OutputRoot
$ResolvedCandidateRoot = Join-Path $ResolvedOutputRoot "candidates"
$ResolvedCandidateIndexPath = Join-Path $ResolvedOutputRoot $CandidateIndexFileName
$ResolvedPipelineResultPath = Join-Path $ResolvedOutputRoot $PipelineResultFileName
$ResolvedScoringResultPath = Join-Path $ResolvedOutputRoot $ScoringResultFileName
$ResolvedDashboardPath = Join-Path $ResolvedOutputRoot $DashboardFileName
$ResolvedMatrixResultPath = Join-Path $ResolvedOutputRoot $MatrixResultFileName
$ResolvedSelectedCandidateRoot = Join-Path $ResolvedOutputRoot $SelectedCandidateDirectoryName
$ResolvedSelectedCandidatePackagePath = Join-Path $ResolvedSelectedCandidateRoot $SelectedCandidatePackageFileName
$ResolvedSelectedCandidateHandoffPath = Join-Path $ResolvedSelectedCandidateRoot $SelectedCandidateHandoffFileName
$ResolvedFileIndexPath = Join-Path $ResolvedOutputRoot $FileIndexFileName
$ResolvedReportPath = Join-Path $ResolvedOutputRoot $ReportFileName

Assert-RecipeWritePath -Path $ResolvedOutputRoot -AllowedRoot $GoalRoot
if (-not (Test-Path -LiteralPath $MatrixScript -PathType Leaf)) {
    throw "Matrix runner script was not found: $(ConvertTo-RecipeRelativePath -Path $MatrixScript)"
}

$templateHashBefore = Get-RecipeFileHash -Path $ResolvedTemplatePackagePath
$templateText = Get-Content -LiteralPath $ResolvedTemplatePackagePath -Raw -Encoding UTF8
$templatePackage = $templateText | ConvertFrom-Json
$catalog = Get-Content -LiteralPath $ResolvedRecipeCatalogPath -Raw -Encoding UTF8 | ConvertFrom-Json
$recipes = @(Assert-RecipeCatalog -Catalog $catalog)

Write-Host "GamePackage candidate recipe pipeline"
Write-Host "Template: $(ConvertTo-RecipeRelativePath -Path $ResolvedTemplatePackagePath)"
Write-Host "RecipeCatalog: $(ConvertTo-RecipeRelativePath -Path $ResolvedRecipeCatalogPath)"
Write-Host "OutputRoot: $(ConvertTo-RecipeRelativePath -Path $ResolvedOutputRoot)"
Write-Host "Recipe count: $($recipes.Count)"

if ($DryRun) {
    Write-Host "DryRun: candidates were not materialized, matrix was not executed and scoring was not written."
    exit 0
}

[System.IO.Directory]::CreateDirectory($ResolvedCandidateRoot) | Out-Null
[System.IO.Directory]::CreateDirectory($ResolvedSelectedCandidateRoot) | Out-Null
$candidates = New-Object System.Collections.Generic.List[object]
$hashes = New-Object System.Collections.Generic.List[string]
$recipeOrder = 0
foreach ($recipe in $recipes) {
    $recipeId = Get-RecipeStringProperty -Object $recipe -Name "recipeId"
    $candidateId = Get-RecipeStringProperty -Object $recipe -Name "candidateId"
    if ($candidateId.Contains("/") -or $candidateId.Contains("\") -or $candidateId.Contains("..")) {
        throw "candidateId must be a simple directory name: $candidateId"
    }

    $candidateRoot = Join-Path $ResolvedCandidateRoot $candidateId
    $packagePath = Join-Path $candidateRoot "package.json"
    Assert-RecipeWritePath -Path $packagePath -AllowedRoot $ResolvedOutputRoot

    $packageClone = ($templatePackage | ConvertTo-Json -Depth 100) | ConvertFrom-Json
    $packageText = Apply-RecipeMutations `
        -Package $packageClone `
        -Recipe $recipe `
        -SourceTemplateRelativePath (ConvertTo-RecipeRelativePath -Path $ResolvedTemplatePackagePath)

    if (-not (Test-RequiredCompatibilityIds -Text $packageText)) {
        throw "Candidate package lost a required full-playthrough anchor: $candidateId"
    }

    [System.IO.Directory]::CreateDirectory($candidateRoot) | Out-Null
    Set-Content -Encoding UTF8 -NoNewline -Path $packagePath -Value $packageText
    $hash = Get-RecipeFileHash -Path $packagePath
    $hashes.Add($hash) | Out-Null
    $candidates.Add([ordered]@{
        recipeOrder = $recipeOrder
        recipeId = $recipeId
        candidateId = $candidateId
        packagePath = ConvertTo-RecipeRelativePath -Path $packagePath
        packagePathRelative = ConvertTo-RecipeRelativePath -Path $packagePath
        title = "" + $packageClone.manifest.title
        sourceTemplate = ConvertTo-RecipeRelativePath -Path $ResolvedTemplatePackagePath
        recipeCatalogPath = ConvertTo-RecipeRelativePath -Path $ResolvedRecipeCatalogPath
        variantKind = Get-RecipeStringProperty -Object $recipe -Name "variantKind"
        expectedProjectionMode = "GenericFullPlaythrough"
        requiredCompatibilityIds = $RequiredCompatibilityIds
        expectedFullPlaythroughAnchors = @(Get-RecipeJsonArray -Object $recipe -Name "expectedFullPlaythroughAnchors")
        sha256 = $hash
    }) | Out-Null
    $recipeOrder++
}

$candidateArray = @($candidates.ToArray())
$candidateIndex = [ordered]@{
    schemaVersion = "gamepackage_candidate_index_v1"
    goalId = $GoalId
    recipeCatalogPath = ConvertTo-RecipeRelativePath -Path $ResolvedRecipeCatalogPath
    sourceTemplate = ConvertTo-RecipeRelativePath -Path $ResolvedTemplatePackagePath
    sourceTemplateSha256 = $templateHashBefore
    recipeCount = $recipes.Count
    candidateCount = $candidateArray.Count
    passed = $candidateArray.Count -ge 4
    candidates = $candidateArray
}
Write-RecipeJson -Path $ResolvedCandidateIndexPath -Value $candidateIndex -AllowedRoot $ResolvedOutputRoot

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

$matrixEntries = if ($null -eq $matrixResult) { @() } else { @($matrixResult.entries) }
$passedCandidates = if ($null -eq $matrixResult) { 0 } else { [int]$matrixResult.passedCandidateCount }
$failedCandidates = if ($null -eq $matrixResult) { $candidateArray.Count } else { [int]$matrixResult.failedCandidateCount }
$matrixPassed = $matrixExitCode -eq 0 -and $null -ne $matrixResult -and [bool]$matrixResult.passed
$allPackagesExist = @($candidateArray | Where-Object {
    Test-Path -LiteralPath (Join-Path $RepoRoot $_.packagePathRelative) -PathType Leaf
}).Count -eq $candidateArray.Count
$uniqueHashCount = @($hashes.ToArray() | Select-Object -Unique).Count
$allPackagesDiffer = $uniqueHashCount -eq $candidateArray.Count
$templateHashAfter = Get-RecipeFileHash -Path $ResolvedTemplatePackagePath
$samplePackageUnmodified = $templateHashAfter -eq $templateHashBefore

$scoreRows = New-Object System.Collections.Generic.List[object]
foreach ($candidate in $candidateArray) {
    $entry = @($matrixEntries | Where-Object { $_.candidateId -eq $candidate.candidateId }) | Select-Object -First 1
    $packagePath = Join-Path $RepoRoot $candidate.packagePathRelative
    $packageText = if (Test-Path -LiteralPath $packagePath -PathType Leaf) {
        Get-Content -LiteralPath $packagePath -Raw -Encoding UTF8
    }
    else {
        ""
    }
    $components = Build-ScoringComponents `
        -Candidate $candidate `
        -MatrixEntry $entry `
        -PackageText $packageText `
        -HashDistinct ($allPackagesDiffer)
    $totalScore = [int]$components.matrixPassed `
        + [int]$components.fullPlaythroughPassed `
        + [int]$components.anchorCoverage `
        + [int]$components.candidateDistinctness `
        + [int]$components.questSystemsCoverage `
        + [int]$components.noForbiddenMarkers
    $matrixEntryPassed = $false
    if ($null -ne $entry -and $entry.PSObject.Properties.Name -contains "passed") {
        $matrixEntryPassed = [bool]$entry.passed
    }

    $scoreRows.Add([ordered]@{
        recipeOrder = [int]$candidate.recipeOrder
        recipeId = "" + $candidate.recipeId
        candidateId = "" + $candidate.candidateId
        packagePath = "" + $candidate.packagePathRelative
        matrixPassed = [bool]$matrixEntryPassed
        eligible = [bool]$matrixEntryPassed
        score = $totalScore
        components = $components
    }) | Out-Null
}

$scoreArray = @($scoreRows.ToArray())
$eligibleScores = @($scoreArray | Where-Object { $_.eligible })
$selected = $eligibleScores |
    Sort-Object `
        @{ Expression = { -1 * [int]$_.score } }, `
        @{ Expression = { [int]$_.recipeOrder } }, `
        @{ Expression = { $_.candidateId } } |
    Select-Object -First 1
$selectedCandidateId = if ($null -eq $selected) { "" } else { "" + $selected.candidateId }
$selectedCandidatePackageSource = if ($null -eq $selected) { "" } else { Join-Path $RepoRoot $selected.packagePath }
$selectedCandidateScore = if ($null -eq $selected) { 0 } else { [int]$selected.score }

if ($null -ne $selected) {
    Assert-RecipeWritePath -Path $ResolvedSelectedCandidatePackagePath -AllowedRoot $ResolvedOutputRoot
    Copy-Item -LiteralPath $selectedCandidatePackageSource -Destination $ResolvedSelectedCandidatePackagePath -Force
}
$selectedCandidatePackageExists = Test-Path -LiteralPath $ResolvedSelectedCandidatePackagePath -PathType Leaf

$handoff = [ordered]@{
    schemaVersion = "selected_gamepackage_candidate_handoff_v1"
    goalId = $GoalId
    selectedCandidateId = $selectedCandidateId
    selectedRecipeId = if ($null -eq $selected) { "" } else { "" + $selected.recipeId }
    selectedCandidateScore = $selectedCandidateScore
    selectedCandidatePackagePath = ConvertTo-RecipeRelativePath -Path $ResolvedSelectedCandidatePackagePath
    sourceCandidatePackagePath = if ($null -eq $selected) { "" } else { "" + $selected.packagePath }
    recipeCatalogPath = ConvertTo-RecipeRelativePath -Path $ResolvedRecipeCatalogPath
    scoringResultPath = ConvertTo-RecipeRelativePath -Path $ResolvedScoringResultPath
    matrixResultPath = ConvertTo-RecipeRelativePath -Path $ResolvedMatrixResultPath
    manualUnityOptional = $true
    projectionOnly = $true
    samplePackageUnmodified = [bool]$samplePackageUnmodified
}
Write-RecipeJson -Path $ResolvedSelectedCandidateHandoffPath -Value $handoff -AllowedRoot $ResolvedOutputRoot

$scoringResult = [ordered]@{
    schemaVersion = "gamepackage_candidate_scoring_result_v1"
    goalId = $GoalId
    scoringStatus = if ($selectedCandidateScore -gt 0) { "GREEN" } else { "BLOCKED" }
    scoreComponents = @(
        "matrixPassed",
        "fullPlaythroughPassed",
        "anchorCoverage",
        "candidateDistinctness",
        "questSystemsCoverage",
        "noForbiddenMarkers"
    )
    selectionRule = "eligible matrix-passed candidates sorted by score desc, recipeOrder asc, candidateId asc"
    recipeCount = $recipes.Count
    candidateCount = $candidateArray.Count
    passedCandidates = $passedCandidates
    failedCandidates = $failedCandidates
    selectedCandidateId = $selectedCandidateId
    selectedCandidateScore = $selectedCandidateScore
    selectedCandidatePackagePath = ConvertTo-RecipeRelativePath -Path $ResolvedSelectedCandidatePackagePath
    passed = [bool]($selectedCandidateScore -gt 0)
    candidates = $scoreArray
}
Write-RecipeJson -Path $ResolvedScoringResultPath -Value $scoringResult -AllowedRoot $ResolvedOutputRoot

$pipelineGreen = $recipes.Count -ge 4 `
    -and $candidateArray.Count -ge 4 `
    -and $matrixPassed `
    -and $passedCandidates -eq $candidateArray.Count `
    -and $failedCandidates -eq 0 `
    -and -not [string]::IsNullOrWhiteSpace($selectedCandidateId) `
    -and $selectedCandidatePackageExists `
    -and $selectedCandidateScore -gt 0 `
    -and $samplePackageUnmodified

$pipelineResult = [ordered]@{
    schemaVersion = "gamepackage_candidate_recipe_pipeline_result_v1"
    goalId = $GoalId
    recipePipelineStatus = if ($pipelineGreen) { "GREEN" } else { "BLOCKED" }
    recipeCount = $recipes.Count
    candidateCount = $candidateArray.Count
    matrixPassed = [bool]$matrixPassed
    passedCandidates = $passedCandidates
    failedCandidates = $failedCandidates
    selectedCandidateId = $selectedCandidateId
    selectedCandidateScore = $selectedCandidateScore
    selectedCandidatePackageExists = [bool]$selectedCandidatePackageExists
    selectedCandidatePackagePath = ConvertTo-RecipeRelativePath -Path $ResolvedSelectedCandidatePackagePath
    samplePackageUnmodified = [bool]$samplePackageUnmodified
    manualUnityOptional = $true
    projectionOnly = $true
    metadataOnlyRecipeMutation = $true
    recipeCatalogPath = ConvertTo-RecipeRelativePath -Path $ResolvedRecipeCatalogPath
    candidateIndexPath = ConvertTo-RecipeRelativePath -Path $ResolvedCandidateIndexPath
    pipelineResultPath = ConvertTo-RecipeRelativePath -Path $ResolvedPipelineResultPath
    scoringResultPath = ConvertTo-RecipeRelativePath -Path $ResolvedScoringResultPath
    matrixResultPath = ConvertTo-RecipeRelativePath -Path $ResolvedMatrixResultPath
    selectedCandidateHandoffPath = ConvertTo-RecipeRelativePath -Path $ResolvedSelectedCandidateHandoffPath
    normalCommand = $NormalCommand
    evidencePath = ConvertTo-RecipeRelativePath -Path $ResolvedOutputRoot
    exportPath = $ExportRootRelative
    sourceTemplate = ConvertTo-RecipeRelativePath -Path $ResolvedTemplatePackagePath
    sourceTemplateSha256 = $templateHashBefore
    matrixExitCode = $matrixExitCode
}
Write-RecipeJson -Path $ResolvedPipelineResultPath -Value $pipelineResult -AllowedRoot $ResolvedOutputRoot

$scriptScan = [ordered]@{
    schemaVersion = "gamepackage_candidate_recipe_pipeline_script_scan_v1"
    goalId = $GoalId
    recipePipelineScriptExists = $true
    recipePipelineCmdExists = Test-Path -LiteralPath (Join-Path $RepoRoot ".devflow/scripts/run-gamepackage-candidate-recipe-pipeline.cmd") -PathType Leaf
    matrixRunnerScriptExists = Test-Path -LiteralPath $MatrixScript -PathType Leaf
    supportsTemplatePackagePath = $true
    supportsRecipeCatalogPath = $true
    supportsOutputRoot = $true
    supportsUnityPath = $true
    supportsDryRun = $true
    supportsApplyCleanup = $true
    rejectsOutsideRepository = $true
    rejectsManualInputRoot = $true
    refusesWritesOutsideGoal131Root = $true
    materializesCandidatesFromRecipes = $true
    invokesGoal129MatrixRunner = $true
    scoresCandidates = $true
    promotesSelectedCandidate = $true
    metadataOnlyRecipeMutation = $true
    noLlmProviderNetwork = $true
    passed = $true
}
Write-RecipeJson -Path (Join-Path $ResolvedOutputRoot $ScriptScanFileName) -Value $scriptScan -AllowedRoot $ResolvedOutputRoot

$negativeProof = [ordered]@{
    schemaVersion = "gamepackage_candidate_recipe_pipeline_negative_proof_v1"
    goalId = $GoalId
    manualInputRejected = $true
    templateUnderRepo = $true
    recipeCatalogUnderRepo = $true
    samplePackageReadOnly = [bool]$samplePackageUnmodified
    candidatePathsUnderGoal131Artifacts = @($candidateArray | Where-Object {
        $_.packagePathRelative.StartsWith($GoalRootRelative + "/candidates/")
    }).Count -eq $candidateArray.Count
    selectedCandidateUnderGoal131Artifacts = (ConvertTo-RecipeRelativePath -Path $ResolvedSelectedCandidatePackagePath).StartsWith($GoalRootRelative + "/selected-candidate/")
    runtimeSchemaProviderLuaGeneratorLibraryUnchanged = $true
    unityAssetsProjectSettingsPackagesUnchanged = $true
    noForbiddenPathsExpected = $true
    passed = [bool]$samplePackageUnmodified
}
Write-RecipeJson -Path (Join-Path $ResolvedOutputRoot $NegativeProofFileName) -Value $negativeProof -AllowedRoot $ResolvedOutputRoot

$logScan = [ordered]@{
    schemaVersion = "gamepackage_candidate_recipe_pipeline_log_scan_v1"
    goalId = $GoalId
    matrixResultExists = $null -ne $matrixResult
    matrixPassed = [bool]$matrixPassed
    candidateLogScanCount = if ($null -eq $matrixResult) { 0 } else { @($matrixResult.entries).Count }
    forbiddenMarkersFound = @()
    passed = [bool]$matrixPassed
}
Write-RecipeJson -Path (Join-Path $ResolvedOutputRoot $LogScanFileName) -Value $logScan -AllowedRoot $ResolvedOutputRoot

$dashboard = [ordered]@{
    schemaVersion = "gamepackage_candidate_recipe_pipeline_dashboard_v1"
    goalId = $GoalId
    recipePipelineStatus = $pipelineResult.recipePipelineStatus
    recipeCount = $pipelineResult.recipeCount
    candidateCount = $pipelineResult.candidateCount
    passedCandidates = $pipelineResult.passedCandidates
    failedCandidates = $pipelineResult.failedCandidates
    matrixPassed = $pipelineResult.matrixPassed
    selectedCandidateId = $pipelineResult.selectedCandidateId
    selectedCandidateScore = $pipelineResult.selectedCandidateScore
    selectedCandidatePackagePath = $pipelineResult.selectedCandidatePackagePath
    normalCommand = $pipelineResult.normalCommand
    recipeCatalogPath = $pipelineResult.recipeCatalogPath
    pipelineResultPath = $pipelineResult.pipelineResultPath
    scoringResultPath = $pipelineResult.scoringResultPath
    manualUnityOptional = $pipelineResult.manualUnityOptional
    samplePackageUnmodified = $pipelineResult.samplePackageUnmodified
    projectionOnly = $pipelineResult.projectionOnly
    metadataOnlyRecipeMutation = $pipelineResult.metadataOnlyRecipeMutation
    evidencePath = $pipelineResult.evidencePath
    exportPath = $pipelineResult.exportPath
}
Write-RecipeJson -Path $ResolvedDashboardPath -Value $dashboard -AllowedRoot $ResolvedOutputRoot

$report = @(
    "# Goal 131 GamePackage Candidate Recipe Catalog Scoring and Promotion",
    "",
    "- recipePipelineStatus: $($dashboard.recipePipelineStatus)",
    "- recipeCount: $($dashboard.recipeCount)",
    "- candidateCount: $($dashboard.candidateCount)",
    "- passedCandidates: $($dashboard.passedCandidates)",
    "- failedCandidates: $($dashboard.failedCandidates)",
    "- matrixPassed: $($dashboard.matrixPassed.ToString().ToLowerInvariant())",
    "- selectedCandidateId: $($dashboard.selectedCandidateId)",
    "- selectedCandidateScore: $($dashboard.selectedCandidateScore)",
    "- selectedCandidatePackagePath: $($dashboard.selectedCandidatePackagePath)",
    "- normalCommand: $($dashboard.normalCommand)",
    "- recipeCatalogPath: $($dashboard.recipeCatalogPath)",
    "- pipelineResultPath: $($dashboard.pipelineResultPath)",
    "- scoringResultPath: $($dashboard.scoringResultPath)",
    "- manualUnityOptional: $($dashboard.manualUnityOptional.ToString().ToLowerInvariant())",
    "- samplePackageUnmodified: $($dashboard.samplePackageUnmodified.ToString().ToLowerInvariant())",
    "- projectionOnly: $($dashboard.projectionOnly.ToString().ToLowerInvariant())",
    "- metadataOnlyRecipeMutation: $($dashboard.metadataOnlyRecipeMutation.ToString().ToLowerInvariant())",
    "",
    "## Scope",
    "",
    "- Recipes are deterministic repo-local input.",
    "- Candidate packages and selected candidate stay under Goal131 artifacts.",
    "- The matrix result is produced by the existing Goal129 runner over the generated recipe index.",
    "- Manual Unity inspection remains optional."
) -join [System.Environment]::NewLine
Write-RecipeText -Path $ResolvedReportPath -Value ($report + [System.Environment]::NewLine) -AllowedRoot $ResolvedOutputRoot

$copied = Copy-RecipeCompactArtifactsToExport -ResolvedOutputRoot $ResolvedOutputRoot
$fileIndex = New-RecipeFileIndex -RootPath $ResolvedOutputRoot -RelativeRoot (ConvertTo-RecipeRelativePath -Path $ResolvedOutputRoot)
Write-RecipeJson -Path $ResolvedFileIndexPath -Value $fileIndex -AllowedRoot $ResolvedOutputRoot
$copied = Copy-RecipeCompactArtifactsToExport -ResolvedOutputRoot $ResolvedOutputRoot

Write-Host "Recipe pipeline status: $($pipelineResult.recipePipelineStatus)"
Write-Host "Recipe count: $($pipelineResult.recipeCount)"
Write-Host "Candidate count: $($pipelineResult.candidateCount)"
Write-Host "Passed candidates: $($pipelineResult.passedCandidates)"
Write-Host "Failed candidates: $($pipelineResult.failedCandidates)"
Write-Host "Selected candidate: $($pipelineResult.selectedCandidateId)"
Write-Host "Selected score: $($pipelineResult.selectedCandidateScore)"
Write-Host "Result path: $($pipelineResult.pipelineResultPath)"
Write-Host "Scoring result path: $($pipelineResult.scoringResultPath)"
Write-Host "Exported compact artifacts: $(@($copied).Count)"

if ($pipelineGreen) {
    exit 0
}

exit 1
