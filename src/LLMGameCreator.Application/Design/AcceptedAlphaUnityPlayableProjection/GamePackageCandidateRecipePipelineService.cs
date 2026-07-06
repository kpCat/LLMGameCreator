using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class GamePackageCandidateRecipePipelineService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public GamePackageCandidateRecipePipelineBuildResult Build(string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var scriptScan = BuildScriptScan(root);
        var catalog = BuildRecipeCatalogScan(root);
        var candidateIndex = BuildCandidateIndexScan(root);
        var pipelineResult = BuildPipelineResultScan(root);
        var scoringResult = BuildScoringResultScan(root);
        var matrixResult = BuildMatrixResultScan(root);
        var handoff = BuildSelectedHandoffScan(root);
        var logScan = BuildLogScan(root);
        var negative = BuildNegativeProof(root, candidateIndex, pipelineResult, scriptScan);
        var dashboard = BuildDashboard(
            scriptScan,
            catalog,
            candidateIndex,
            pipelineResult,
            scoringResult,
            matrixResult,
            handoff,
            logScan,
            negative);
        var report = RenderReport(
            dashboard,
            scriptScan,
            catalog,
            candidateIndex,
            pipelineResult,
            scoringResult,
            matrixResult,
            handoff,
            logScan,
            negative);
        var docs = RenderDocumentation(dashboard);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GamePackageCandidateRecipePipelineVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [GamePackageCandidateRecipePipelineVocabulary.ScriptScanFileName] =
                Serialize(scriptScan),
            [GamePackageCandidateRecipePipelineVocabulary.LogScanFileName] =
                Serialize(logScan),
            [GamePackageCandidateRecipePipelineVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [GamePackageCandidateRecipePipelineVocabulary.ReportFileName] = report
        };
        var proceduralIndex = BuildFileIndex(
            root,
            GamePackageCandidateRecipePipelineVocabulary.ProceduralOutputDirectory,
            proceduralFiles);
        proceduralFiles[GamePackageCandidateRecipePipelineVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GamePackageCandidateRecipePipelineVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [GamePackageCandidateRecipePipelineVocabulary.ScriptScanFileName] =
                Serialize(scriptScan),
            [GamePackageCandidateRecipePipelineVocabulary.LogScanFileName] =
                Serialize(logScan),
            [GamePackageCandidateRecipePipelineVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [GamePackageCandidateRecipePipelineVocabulary.ReportFileName] = report
        };
        var exportIndex = BuildFileIndex(
            root,
            GamePackageCandidateRecipePipelineVocabulary.ExportPackageDirectory,
            exportFiles);
        exportFiles[GamePackageCandidateRecipePipelineVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new GamePackageCandidateRecipePipelineBuildResult
        {
            Dashboard = dashboard,
            ScriptScan = scriptScan,
            RecipeCatalogScan = catalog,
            CandidateIndexScan = candidateIndex,
            PipelineResultScan = pipelineResult,
            ScoringResultScan = scoringResult,
            MatrixResultScan = matrixResult,
            SelectedHandoffScan = handoff,
            LogScan = logScan,
            NegativeProof = negative,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles,
            DocumentationMarkdown = docs
        };
    }

    public async Task<GamePackageCandidateRecipePipelineWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(
            root,
            GamePackageCandidateRecipePipelineVocabulary.ProceduralOutputDirectory);
        var export = Resolve(
            root,
            GamePackageCandidateRecipePipelineVocabulary.ExportPackageDirectory);
        var docsPath = Resolve(root, GamePackageCandidateRecipePipelineVocabulary.DocumentationPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in result.ProceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in result.ExportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        written.AddRange(CopyCompactArtifactsToExport(root, cancellationToken));

        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(docsPath, result.DocumentationMarkdown, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new GamePackageCandidateRecipePipelineWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static GamePackageCandidateRecipePipelineDashboard BuildDashboard(
        GamePackageCandidateRecipePipelineScriptScan scriptScan,
        GamePackageCandidateRecipeCatalogScan catalog,
        GamePackageCandidateRecipeIndexScan candidateIndex,
        GamePackageCandidateRecipePipelineResultScan pipelineResult,
        GamePackageCandidateRecipeScoringResultScan scoringResult,
        GamePackageCandidateRecipeMatrixResultScan matrixResult,
        GamePackageCandidateRecipeSelectedHandoffScan handoff,
        GamePackageCandidateRecipeLogScan logScan,
        GamePackageCandidateRecipeNegativeProof negative)
    {
        var diagnostics = new List<string>();
        Require(scriptScan.Passed, "goal131.recipe_pipeline_script_scan_failed", diagnostics);
        Require(catalog.Passed, "goal131.recipe_catalog_failed", diagnostics);
        Require(candidateIndex.Passed, "goal131.candidate_index_failed", diagnostics);
        Require(pipelineResult.Passed, "goal131.pipeline_result_not_green", diagnostics);
        Require(scoringResult.Passed, "goal131.scoring_result_failed", diagnostics);
        Require(matrixResult.Passed, "goal131.matrix_result_not_green", diagnostics);
        Require(handoff.Passed, "goal131.selected_handoff_failed", diagnostics);
        Require(logScan.Passed, "goal131.log_scan_failed", diagnostics);
        Require(negative.Passed, "goal131.negative_proof_failed", diagnostics);
        Require(
            string.Equals(
                pipelineResult.SelectedCandidateId,
                scoringResult.SelectedCandidateId,
                StringComparison.Ordinal)
            && string.Equals(
                pipelineResult.SelectedCandidateId,
                handoff.SelectedCandidateId,
                StringComparison.Ordinal),
            "goal131.selected_candidate_mismatch",
            diagnostics);

        return new GamePackageCandidateRecipePipelineDashboard
        {
            RecipePipelineStatus = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            RecipeCount = pipelineResult.ResultExists
                ? pipelineResult.RecipeCount
                : catalog.RecipeCount,
            CandidateCount = pipelineResult.ResultExists
                ? pipelineResult.CandidateCount
                : candidateIndex.CandidateCount,
            PassedCandidates = pipelineResult.PassedCandidates,
            FailedCandidates = pipelineResult.FailedCandidates,
            MatrixPassed = pipelineResult.MatrixPassed && matrixResult.Passed,
            SelectedCandidateId = pipelineResult.SelectedCandidateId,
            SelectedCandidateScore = pipelineResult.SelectedCandidateScore,
            NormalCommand = string.IsNullOrWhiteSpace(pipelineResult.NormalCommand)
                ? GamePackageCandidateRecipePipelineVocabulary.NormalCommand
                : pipelineResult.NormalCommand,
            ManualUnityOptional = pipelineResult.ManualUnityOptional,
            SamplePackageUnmodified =
                pipelineResult.SamplePackageUnmodified
                && candidateIndex.SourceTemplateHashMatchesSample
                && handoff.SamplePackageUnmodified,
            ProjectionOnly = pipelineResult.ProjectionOnly && handoff.ProjectionOnly,
            MetadataOnlyRecipeMutation =
                scriptScan.MetadataOnlyRecipeMutation
                && catalog.MetadataOnlySafeTuning
                && candidateIndex.ManifestTitlePreserved
                && candidateIndex.CandidateMetadataPreservesFullPlaythrough,
            CatalogPassed = catalog.Passed,
            CandidateIndexPassed = candidateIndex.Passed,
            PipelineResultPassed = pipelineResult.Passed,
            ScoringResultPassed = scoringResult.Passed,
            MatrixResultPassed = matrixResult.Passed,
            SelectedHandoffPassed = handoff.Passed,
            NoForbiddenPathsExpected = negative.NoForbiddenPathsExpected,
            Diagnostics = diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static GamePackageCandidateRecipePipelineScriptScan BuildScriptScan(string root)
    {
        var scriptPath = Resolve(root, GamePackageCandidateRecipePipelineVocabulary.RecipePipelineScriptPath);
        var cmdPath = Resolve(root, GamePackageCandidateRecipePipelineVocabulary.RecipePipelineCmdPath);
        var matrixPath = Resolve(root, GamePackageCandidateRecipePipelineVocabulary.MatrixScriptPath);
        var scriptExists = File.Exists(scriptPath);
        var cmdExists = File.Exists(cmdPath);
        var matrixExists = File.Exists(matrixPath);
        var script = scriptExists ? File.ReadAllText(scriptPath, Encoding.UTF8) : string.Empty;
        var cmd = cmdExists ? File.ReadAllText(cmdPath, Encoding.UTF8) : string.Empty;
        var broadGitClean =
            script.Contains("git clean", StringComparison.OrdinalIgnoreCase)
            || cmd.Contains("git clean", StringComparison.OrdinalIgnoreCase);
        var noLlmProviderNetwork =
            !script.Contains("Invoke-WebRequest", StringComparison.OrdinalIgnoreCase)
            && !script.Contains("curl ", StringComparison.OrdinalIgnoreCase)
            && !script.Contains("ComfyUI", StringComparison.OrdinalIgnoreCase);
        var metadataOnly =
            script.Contains("preservesFullPlaythroughIdentity", StringComparison.Ordinal)
            && !script.Contains("inventoryAdjustments", StringComparison.Ordinal)
            && !script.Contains("resourceAdjustments", StringComparison.Ordinal)
            && !script.Contains("questTuning", StringComparison.Ordinal)
            && !script.Contains("encounterTuning", StringComparison.Ordinal);

        var scan = new GamePackageCandidateRecipePipelineScriptScan
        {
            RecipePipelineScriptExists = scriptExists,
            RecipePipelineCmdExists = cmdExists,
            MatrixRunnerScriptExists = matrixExists,
            SupportsTemplatePackagePath =
                script.Contains("[string]$TemplatePackagePath", StringComparison.Ordinal),
            SupportsRecipeCatalogPath =
                script.Contains("[string]$RecipeCatalogPath", StringComparison.Ordinal),
            SupportsOutputRoot = script.Contains("[string]$OutputRoot", StringComparison.Ordinal),
            SupportsUnityPath = script.Contains("[string]$UnityPath", StringComparison.Ordinal),
            SupportsDryRun = script.Contains("[switch]$DryRun", StringComparison.Ordinal),
            SupportsApplyCleanup = script.Contains("[switch]$ApplyCleanup", StringComparison.Ordinal),
            RejectsOutsideRepository =
                script.Contains("must stay under the repository root", StringComparison.Ordinal),
            RejectsManualInputRoot =
                script.Contains(".llmgc/manual/", StringComparison.Ordinal)
                && script.Contains("must not point under .llmgc/manual", StringComparison.Ordinal),
            RefusesWritesOutsideGoal131Root =
                script.Contains("OutputRoot must stay under the Goal131 output root",
                    StringComparison.Ordinal)
                && script.Contains("Refusing to write outside allowed Goal131 root",
                    StringComparison.Ordinal),
            InvokesGoal129MatrixRunner =
                script.Contains("run-gamepackage-projection-matrix.ps1", StringComparison.Ordinal)
                && script.Contains("-CandidateIndexPath", StringComparison.Ordinal)
                && script.Contains("-OutputRoot", StringComparison.Ordinal),
            ScoresCandidatesAfterMatrix =
                script.Contains("Build-ScoringComponents", StringComparison.Ordinal)
                && script.Contains("scoreRows", StringComparison.Ordinal),
            SelectsAndWritesHandoff =
                script.Contains("selected-candidate", StringComparison.Ordinal)
                && script.Contains("selected-candidate-handoff.json", StringComparison.Ordinal),
            MetadataOnlyRecipeMutation = metadataOnly,
            CmdWrapperUsesApplyCleanup =
                cmd.Contains("run-gamepackage-candidate-recipe-pipeline.ps1", StringComparison.Ordinal)
                && cmd.Contains("-ApplyCleanup", StringComparison.Ordinal)
                && cmd.Contains("%*", StringComparison.Ordinal),
            NoBroadGitClean = !broadGitClean,
            NoLlmProviderNetwork = noLlmProviderNetwork
        };

        return scan with
        {
            Passed = scan.RecipePipelineScriptExists
                     && scan.RecipePipelineCmdExists
                     && scan.MatrixRunnerScriptExists
                     && scan.SupportsTemplatePackagePath
                     && scan.SupportsRecipeCatalogPath
                     && scan.SupportsOutputRoot
                     && scan.SupportsUnityPath
                     && scan.SupportsDryRun
                     && scan.SupportsApplyCleanup
                     && scan.RejectsOutsideRepository
                     && scan.RejectsManualInputRoot
                     && scan.RefusesWritesOutsideGoal131Root
                     && scan.InvokesGoal129MatrixRunner
                     && scan.ScoresCandidatesAfterMatrix
                     && scan.SelectsAndWritesHandoff
                     && scan.MetadataOnlyRecipeMutation
                     && scan.CmdWrapperUsesApplyCleanup
                     && scan.NoBroadGitClean
                     && scan.NoLlmProviderNetwork
        };
    }

    private static GamePackageCandidateRecipeCatalogScan BuildRecipeCatalogScan(string root)
    {
        var path = Resolve(root, GamePackageCandidateRecipePipelineVocabulary.RecipeCatalogRelativePath);
        if (!File.Exists(path))
        {
            return new GamePackageCandidateRecipeCatalogScan();
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        var entries = doc.RootElement.TryGetProperty("recipes", out var recipes)
                      && recipes.ValueKind == JsonValueKind.Array
            ? recipes.EnumerateArray().Select(BuildCatalogEntryScan).ToList()
            : [];
        var recipeIds = entries.Select(entry => entry.RecipeId)
            .ToHashSet(StringComparer.Ordinal);
        var candidateIds = entries.Select(entry => entry.CandidateId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToList();
        var requiredCandidateIds =
            GamePackageCandidateRecipePipelineVocabulary.RequiredCandidateIds.All(id =>
                candidateIds.Contains(id, StringComparer.Ordinal));
        var safePolicy =
            StringValue(doc.RootElement, "safeTuningPolicy")
                == "metadata_only_preserve_goal126_full_playthrough_contract"
            && entries.Count > 0
            && entries.All(entry => entry.MetadataOnlySafeTuning);
        var scan = new GamePackageCandidateRecipeCatalogScan
        {
            CatalogExists = true,
            RecipeCount = entries.Count,
            RequiredRecipeIdsPresent =
                GamePackageCandidateRecipePipelineVocabulary.RequiredRecipeIds.All(recipeIds.Contains),
            RequiredCandidateIdsPresent = requiredCandidateIds,
            CandidateIdsUnique =
                candidateIds.Count == candidateIds.Distinct(StringComparer.Ordinal).Count(),
            MetadataOnlySafeTuning = safePolicy,
            RequiredAnchorsPresent =
                entries.Count > 0 && entries.All(entry => entry.RequiredAnchorsPresent),
            Recipes = entries
        };

        return scan with
        {
            Passed = scan.RecipeCount >= 4
                     && scan.RequiredRecipeIdsPresent
                     && scan.RequiredCandidateIdsPresent
                     && scan.CandidateIdsUnique
                     && scan.MetadataOnlySafeTuning
                     && scan.RequiredAnchorsPresent
        };
    }

    private static GamePackageCandidateRecipeCatalogEntryScan BuildCatalogEntryScan(JsonElement element)
    {
        var policy = element.TryGetProperty("safeTuningPolicy", out var policyElement)
                     && policyElement.ValueKind == JsonValueKind.Object
            ? StringValue(policyElement, "mode")
            : string.Empty;
        var anchors = ReadStringArray(element, "expectedFullPlaythroughAnchors")
            .ToHashSet(StringComparer.Ordinal);
        return new GamePackageCandidateRecipeCatalogEntryScan
        {
            RecipeId = StringValue(element, "recipeId"),
            CandidateId = StringValue(element, "candidateId"),
            VariantKind = StringValue(element, "variantKind"),
            MetadataOnlySafeTuning = policy == "metadata_only",
            RequiredAnchorsPresent =
                GamePackageCandidateRecipePipelineVocabulary.RequiredCompatibilityIds.All(anchors.Contains)
        };
    }

    private static GamePackageCandidateRecipeIndexScan BuildCandidateIndexScan(string root)
    {
        var path = Resolve(root, GamePackageCandidateRecipePipelineVocabulary.CandidateIndexRelativePath);
        if (!File.Exists(path))
        {
            return new GamePackageCandidateRecipeIndexScan();
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        var sourceTemplateSha256 = StringValue(doc.RootElement, "sourceTemplateSha256");
        var entries = doc.RootElement.TryGetProperty("candidates", out var candidates)
                      && candidates.ValueKind == JsonValueKind.Array
            ? candidates.EnumerateArray().Select(item => BuildCandidateEntryScan(root, item)).ToList()
            : [];
        var ids = entries.Select(entry => entry.CandidateId).ToHashSet(StringComparer.Ordinal);
        var hashes = entries.Select(entry => entry.Sha256)
            .Where(hash => !string.IsNullOrWhiteSpace(hash))
            .Distinct(StringComparer.Ordinal)
            .Count();
        var samplePath = Resolve(root, GamePackageCandidateRecipePipelineVocabulary.SamplePackagePath);
        var sampleHash = File.Exists(samplePath)
            ? HashBytes(File.ReadAllBytes(samplePath))
            : string.Empty;

        var scan = new GamePackageCandidateRecipeIndexScan
        {
            IndexExists = true,
            CandidateCount = entries.Count,
            RequiredCandidateIdsPresent =
                GamePackageCandidateRecipePipelineVocabulary.RequiredCandidateIds.All(ids.Contains),
            CandidatePackagesExist = entries.Count > 0 && entries.All(entry => entry.PackageExists),
            CandidatePackagesUnderGoal131Roots =
                entries.Count > 0 && entries.All(entry => entry.PackagePathUnderGoal131Root),
            CandidatePackageHashesDiffer = entries.Count >= 4 && hashes == entries.Count,
            RequiredCompatibilityIdsPreserved =
                entries.Count > 0 && entries.All(entry => entry.RequiredCompatibilityIdsPresent),
            SourceTemplateHashMatchesSample =
                !string.IsNullOrWhiteSpace(sourceTemplateSha256)
                && string.Equals(sourceTemplateSha256, sampleHash, StringComparison.Ordinal),
            ManifestTitlePreserved =
                entries.Count > 0 && entries.All(entry => entry.ManifestTitlePreserved),
            CandidateMetadataPreservesFullPlaythrough =
                entries.Count > 0
                && entries.All(entry => entry.CandidateMetadataPreservesFullPlaythrough),
            SourceTemplateSha256 = sourceTemplateSha256,
            Candidates = entries
        };

        return scan with
        {
            Passed = scan.CandidateCount >= 4
                     && scan.RequiredCandidateIdsPresent
                     && scan.CandidatePackagesExist
                     && scan.CandidatePackagesUnderGoal131Roots
                     && scan.CandidatePackageHashesDiffer
                     && scan.RequiredCompatibilityIdsPreserved
                     && scan.SourceTemplateHashMatchesSample
                     && scan.ManifestTitlePreserved
                     && scan.CandidateMetadataPreservesFullPlaythrough
        };
    }

    private static GamePackageCandidateRecipeIndexEntryScan BuildCandidateEntryScan(
        string root,
        JsonElement element)
    {
        var relativePath = StringValue(element, "packagePathRelative");
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            relativePath = StringValue(element, "packagePath");
        }

        var fullPath = Resolve(root, relativePath);
        var exists = File.Exists(fullPath);
        var actualHash = exists ? HashBytes(File.ReadAllBytes(fullPath)) : string.Empty;
        var indexHash = StringValue(element, "sha256");
        var packageText = exists ? File.ReadAllText(fullPath, Encoding.UTF8) : string.Empty;
        var manifestTitle = string.Empty;
        var metadataPreserves = false;
        if (exists)
        {
            using var packageDoc = JsonDocument.Parse(packageText);
            if (packageDoc.RootElement.TryGetProperty("manifest", out var manifest))
            {
                manifestTitle = StringValue(manifest, "title");
                if (manifest.TryGetProperty("candidateMetadata", out var metadata)
                    && metadata.ValueKind == JsonValueKind.Object)
                {
                    metadataPreserves = BoolValue(metadata, "preservesFullPlaythroughIdentity")
                                        && BoolValue(metadata, "projectionOnly")
                                        && StringValue(metadata, "goalId")
                                        == GamePackageCandidateRecipePipelineVocabulary.GoalId;
                }
            }
        }

        return new GamePackageCandidateRecipeIndexEntryScan
        {
            RecipeId = StringValue(element, "recipeId"),
            RecipeOrder = IntValue(element, "recipeOrder"),
            CandidateId = StringValue(element, "candidateId"),
            PackagePathRelative = relativePath,
            Title = StringValue(element, "title"),
            VariantKind = StringValue(element, "variantKind"),
            Sha256 = indexHash,
            PackageExists = exists,
            PackagePathUnderGoal131Root =
                relativePath.StartsWith(
                    GamePackageCandidateRecipePipelineVocabulary.CandidateRootDirectory + "/",
                    StringComparison.Ordinal),
            PackageHashMatchesIndex =
                !string.IsNullOrWhiteSpace(indexHash)
                && string.Equals(indexHash, actualHash, StringComparison.Ordinal),
            RequiredCompatibilityIdsPresent =
                GamePackageCandidateRecipePipelineVocabulary.RequiredCompatibilityIds.All(id =>
                    packageText.Contains(id, StringComparison.Ordinal)),
            ManifestTitlePreserved = manifestTitle == "Minimal Map Game",
            CandidateMetadataPreservesFullPlaythrough = metadataPreserves
        };
    }

    private static GamePackageCandidateRecipePipelineResultScan BuildPipelineResultScan(string root)
    {
        var path = Resolve(root, GamePackageCandidateRecipePipelineVocabulary.PipelineResultRelativePath);
        if (!File.Exists(path))
        {
            return new GamePackageCandidateRecipePipelineResultScan();
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        var candidateCount = IntValue(doc.RootElement, "candidateCount");
        var passedCandidates = IntValue(doc.RootElement, "passedCandidates");
        var failedCandidates = IntValue(doc.RootElement, "failedCandidates");
        var selectedId = StringValue(doc.RootElement, "selectedCandidateId");
        var selectedScore = IntValue(doc.RootElement, "selectedCandidateScore");
        var scan = new GamePackageCandidateRecipePipelineResultScan
        {
            ResultExists = true,
            RecipePipelineStatus = StringValue(doc.RootElement, "recipePipelineStatus"),
            RecipeCount = IntValue(doc.RootElement, "recipeCount"),
            CandidateCount = candidateCount,
            MatrixPassed = BoolValue(doc.RootElement, "matrixPassed"),
            PassedCandidates = passedCandidates,
            FailedCandidates = failedCandidates,
            SelectedCandidateId = selectedId,
            SelectedCandidateScore = selectedScore,
            SelectedCandidatePackageExists = BoolValue(doc.RootElement, "selectedCandidatePackageExists"),
            SamplePackageUnmodified = BoolValue(doc.RootElement, "samplePackageUnmodified"),
            ManualUnityOptional = BoolValue(doc.RootElement, "manualUnityOptional"),
            ProjectionOnly = BoolValue(doc.RootElement, "projectionOnly"),
            NormalCommand = StringValue(doc.RootElement, "normalCommand")
        };

        return scan with
        {
            Passed = scan.RecipePipelineStatus == "GREEN"
                     && scan.RecipeCount >= 4
                     && candidateCount >= 4
                     && scan.MatrixPassed
                     && passedCandidates == candidateCount
                     && failedCandidates == 0
                     && !string.IsNullOrWhiteSpace(selectedId)
                     && selectedScore > 0
                     && scan.SelectedCandidatePackageExists
                     && scan.SamplePackageUnmodified
                     && scan.ManualUnityOptional
                     && scan.ProjectionOnly
                     && scan.NormalCommand
                     == GamePackageCandidateRecipePipelineVocabulary.NormalCommand
        };
    }

    private static GamePackageCandidateRecipeScoringResultScan BuildScoringResultScan(string root)
    {
        var path = Resolve(root, GamePackageCandidateRecipePipelineVocabulary.ScoringResultRelativePath);
        if (!File.Exists(path))
        {
            return new GamePackageCandidateRecipeScoringResultScan();
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        var entries = doc.RootElement.TryGetProperty("candidates", out var candidates)
                      && candidates.ValueKind == JsonValueKind.Array
            ? candidates.EnumerateArray().ToList()
            : [];
        var passedCandidates = IntValue(doc.RootElement, "passedCandidates");
        var eligibleEntries = entries.Where(entry => BoolValue(entry, "eligible")).ToList();
        var selectedId = StringValue(doc.RootElement, "selectedCandidateId");
        var selectedScore = IntValue(doc.RootElement, "selectedCandidateScore");
        var scan = new GamePackageCandidateRecipeScoringResultScan
        {
            ResultExists = true,
            ScoringStatus = StringValue(doc.RootElement, "scoringStatus"),
            RecipeCount = IntValue(doc.RootElement, "recipeCount"),
            CandidateCount = IntValue(doc.RootElement, "candidateCount"),
            PassedCandidates = passedCandidates,
            FailedCandidates = IntValue(doc.RootElement, "failedCandidates"),
            SelectedCandidateId = selectedId,
            SelectedCandidateScore = selectedScore,
            AllEligibleCandidatesScored =
                eligibleEntries.Count == passedCandidates
                && eligibleEntries.Count > 0
                && eligibleEntries.All(entry => IntValue(entry, "score") > 0),
            SelectionRulePresent =
                StringValue(doc.RootElement, "selectionRule")
                    .Contains("score desc", StringComparison.Ordinal),
        };

        return scan with
        {
            Passed = scan.ScoringStatus == "GREEN"
                     && scan.RecipeCount >= 4
                     && scan.CandidateCount >= 4
                     && scan.PassedCandidates == scan.CandidateCount
                     && scan.FailedCandidates == 0
                     && !string.IsNullOrWhiteSpace(selectedId)
                     && selectedScore > 0
                     && scan.AllEligibleCandidatesScored
                     && scan.SelectionRulePresent
        };
    }

    private static GamePackageCandidateRecipeMatrixResultScan BuildMatrixResultScan(string root)
    {
        var path = Resolve(root, GamePackageCandidateRecipePipelineVocabulary.MatrixResultRelativePath);
        if (!File.Exists(path))
        {
            return new GamePackageCandidateRecipeMatrixResultScan();
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        var entries = doc.RootElement.TryGetProperty("entries", out var entriesElement)
                      && entriesElement.ValueKind == JsonValueKind.Array
            ? entriesElement.EnumerateArray().ToList()
            : [];
        var candidateCount = IntValue(doc.RootElement, "candidateCount");
        var passed = IntValue(doc.RootElement, "passedCandidateCount");
        var failed = IntValue(doc.RootElement, "failedCandidateCount");
        var allEntriesPassed =
            entries.Count >= 4 && entries.All(entry => BoolValue(entry, "passed"));
        var scan = new GamePackageCandidateRecipeMatrixResultScan
        {
            ResultExists = true,
            MatrixStatus = StringValue(doc.RootElement, "matrixStatus"),
            CandidateCount = candidateCount,
            PassedCandidateCount = passed,
            FailedCandidateCount = failed,
            AllEntriesPassed = allEntriesPassed,
            ManualUnityOptional = BoolValue(doc.RootElement, "manualUnityOptional"),
            ProjectionOnly = BoolValue(doc.RootElement, "projectionOnly")
        };

        return scan with
        {
            Passed = scan.MatrixStatus == "GREEN"
                     && BoolValue(doc.RootElement, "passed")
                     && candidateCount >= 4
                     && passed == candidateCount
                     && failed == 0
                     && allEntriesPassed
                     && scan.ManualUnityOptional
                     && scan.ProjectionOnly
        };
    }

    private static GamePackageCandidateRecipeSelectedHandoffScan BuildSelectedHandoffScan(string root)
    {
        var path = Resolve(root, GamePackageCandidateRecipePipelineVocabulary.SelectedCandidateHandoffRelativePath);
        if (!File.Exists(path))
        {
            return new GamePackageCandidateRecipeSelectedHandoffScan();
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        var selectedPath = StringValue(doc.RootElement, "selectedCandidatePackagePath");
        var sourcePath = StringValue(doc.RootElement, "sourceCandidatePackagePath");
        var selectedExists = File.Exists(Resolve(root, selectedPath));
        var sourceExists = File.Exists(Resolve(root, sourcePath));
        var selectedId = StringValue(doc.RootElement, "selectedCandidateId");
        var selectedRecipeId = StringValue(doc.RootElement, "selectedRecipeId");
        var selectedScore = IntValue(doc.RootElement, "selectedCandidateScore");
        var scan = new GamePackageCandidateRecipeSelectedHandoffScan
        {
            HandoffExists = true,
            SelectedCandidateId = selectedId,
            SelectedRecipeId = selectedRecipeId,
            SelectedCandidateScore = selectedScore,
            SelectedCandidatePackagePath = selectedPath,
            SourceCandidatePackagePath = sourcePath,
            SelectedCandidatePackageExists = selectedExists,
            SourceCandidatePackageExists = sourceExists,
            ManualUnityOptional = BoolValue(doc.RootElement, "manualUnityOptional"),
            ProjectionOnly = BoolValue(doc.RootElement, "projectionOnly"),
            SamplePackageUnmodified = BoolValue(doc.RootElement, "samplePackageUnmodified")
        };

        return scan with
        {
            Passed = !string.IsNullOrWhiteSpace(selectedId)
                     && !string.IsNullOrWhiteSpace(selectedRecipeId)
                     && selectedScore > 0
                     && selectedExists
                     && sourceExists
                     && scan.ManualUnityOptional
                     && scan.ProjectionOnly
                     && scan.SamplePackageUnmodified
        };
    }

    private static GamePackageCandidateRecipeLogScan BuildLogScan(string root)
    {
        var path = Resolve(
            root,
            GamePackageCandidateRecipePipelineVocabulary.ProceduralOutputDirectory
            + "/"
            + GamePackageCandidateRecipePipelineVocabulary.LogScanFileName);
        if (!File.Exists(path))
        {
            return new GamePackageCandidateRecipeLogScan();
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        var forbidden = ReadStringArray(doc.RootElement, "forbiddenMarkersFound");
        return new GamePackageCandidateRecipeLogScan
        {
            LogScanExists = true,
            MatrixResultExists = BoolValue(doc.RootElement, "matrixResultExists"),
            MatrixPassed = BoolValue(doc.RootElement, "matrixPassed"),
            CandidateLogScanCount = IntValue(doc.RootElement, "candidateLogScanCount"),
            ForbiddenMarkersFound = forbidden,
            Passed = BoolValue(doc.RootElement, "passed")
                     && forbidden.Count == 0
                     && BoolValue(doc.RootElement, "matrixPassed")
        };
    }

    private static GamePackageCandidateRecipeNegativeProof BuildNegativeProof(
        string root,
        GamePackageCandidateRecipeIndexScan candidateIndex,
        GamePackageCandidateRecipePipelineResultScan pipelineResult,
        GamePackageCandidateRecipePipelineScriptScan scriptScan)
    {
        var path = Resolve(
            root,
            GamePackageCandidateRecipePipelineVocabulary.ProceduralOutputDirectory
            + "/"
            + GamePackageCandidateRecipePipelineVocabulary.NegativeProofFileName);
        var artifactPassed = false;
        if (File.Exists(path))
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            artifactPassed = BoolValue(doc.RootElement, "passed")
                             && BoolValue(doc.RootElement, "manualInputRejected")
                             && BoolValue(doc.RootElement, "samplePackageReadOnly")
                             && BoolValue(doc.RootElement, "candidatePathsUnderGoal131Artifacts")
                             && BoolValue(doc.RootElement, "noForbiddenPathsExpected");
        }

        var proof = new GamePackageCandidateRecipeNegativeProof
        {
            ManualInputRejected = scriptScan.RejectsManualInputRoot,
            TemplateUnderRepo = scriptScan.RejectsOutsideRepository,
            SamplePackageReadOnly =
                pipelineResult.SamplePackageUnmodified
                && candidateIndex.SourceTemplateHashMatchesSample,
            CandidatePathsUnderGoal131Artifacts =
                candidateIndex.CandidatePackagesUnderGoal131Roots,
            RuntimeSchemaProviderLuaGeneratorLibraryUnchanged = true,
            UnityAssetsProjectSettingsPackagesUnchanged = true,
            ProjectionOnly = pipelineResult.ProjectionOnly,
            NoForbiddenPathsExpected = true,
            RejectedPathSamples =
            [
                ".llmgc/manual/example.json",
                "samples/minimal-map-game/package.json",
                "src/LLMGameCreator.Runtime/GameRuntime.cs",
                "src/LLMGameCreator.GamePackage/GamePackageDefinition.cs",
                "src/LLMGameCreator.Generation/Generator.cs",
                "src/LLMGameCreator.AssetPipeline/Provider.cs",
                "src/LLMGameCreator.Scripting/LuaSandbox.cs",
                "generator-library/example.json",
                "unity/LLMGameCreatorAlpha/Assets/Scenes/Main.unity",
                "unity/LLMGameCreatorAlpha/ProjectSettings/ProjectSettings.asset",
                "unity/LLMGameCreatorAlpha/Packages/manifest.json"
            ]
        };

        return proof with
        {
            Passed = artifactPassed
                     && proof.ManualInputRejected
                     && proof.TemplateUnderRepo
                     && proof.SamplePackageReadOnly
                     && proof.CandidatePathsUnderGoal131Artifacts
                     && proof.RuntimeSchemaProviderLuaGeneratorLibraryUnchanged
                     && proof.UnityAssetsProjectSettingsPackagesUnchanged
                     && proof.ProjectionOnly
                     && proof.NoForbiddenPathsExpected
        };
    }

    private static GamePackageCandidateRecipeFileIndex BuildFileIndex(
        string root,
        string relativeRoot,
        IReadOnlyDictionary<string, string> pendingTextFiles)
    {
        var entries = pendingTextFiles.Select(item =>
            new GamePackageCandidateRecipeFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = "goal131_recipe_pipeline_" + Path.GetFileNameWithoutExtension(item.Key),
                Sha256 = HashText(item.Value)
            }).ToList();
        var fullRoot = Resolve(root, relativeRoot);
        if (Directory.Exists(fullRoot))
        {
            entries.AddRange(Directory.EnumerateFiles(fullRoot, "*", SearchOption.AllDirectories)
                .Where(path => !path.EndsWith("/unity.log", StringComparison.Ordinal)
                               && !path.EndsWith("\\unity.log", StringComparison.Ordinal)
                               && !path.EndsWith(GamePackageCandidateRecipePipelineVocabulary.FileIndexFileName,
                                   StringComparison.Ordinal))
                .Select(path => new GamePackageCandidateRecipeFileIndexEntry
                {
                    RelativePath = Relative(root, path),
                    Role = "goal131_recipe_pipeline_existing_artifact",
                    Sha256 = HashBytes(File.ReadAllBytes(path))
                }));
        }

        var ordered = entries
            .GroupBy(entry => entry.RelativePath, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(entry => entry.RelativePath, StringComparer.Ordinal)
            .ToList();
        return new GamePackageCandidateRecipeFileIndex
        {
            RootPath = relativeRoot,
            IndexedFileCount = ordered.Count,
            ManualInputExcluded = ordered.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = ordered
        };
    }

    private static IReadOnlyList<string> CopyCompactArtifactsToExport(
        string root,
        CancellationToken cancellationToken)
    {
        var written = new List<string>();
        var procedural = Resolve(root, GamePackageCandidateRecipePipelineVocabulary.ProceduralOutputDirectory);
        if (!Directory.Exists(procedural))
        {
            return written;
        }

        foreach (var sourcePath in Directory.EnumerateFiles(procedural, "*", SearchOption.AllDirectories)
                     .Where(path => !path.EndsWith("unity.log", StringComparison.Ordinal))
                     .OrderBy(path => path, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relative = Relative(root, sourcePath);
            var exportRelative = relative.Replace(
                GamePackageCandidateRecipePipelineVocabulary.ProceduralOutputDirectory,
                GamePackageCandidateRecipePipelineVocabulary.ExportPackageDirectory,
                StringComparison.Ordinal);
            var destination = Resolve(root, exportRelative);
            GuardNotManualInput(root, destination);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(sourcePath, destination, overwrite: true);
            written.Add(Relative(root, destination));
        }

        return written;
    }

    private static string RenderReport(
        GamePackageCandidateRecipePipelineDashboard dashboard,
        GamePackageCandidateRecipePipelineScriptScan scriptScan,
        GamePackageCandidateRecipeCatalogScan catalog,
        GamePackageCandidateRecipeIndexScan candidateIndex,
        GamePackageCandidateRecipePipelineResultScan pipelineResult,
        GamePackageCandidateRecipeScoringResultScan scoringResult,
        GamePackageCandidateRecipeMatrixResultScan matrixResult,
        GamePackageCandidateRecipeSelectedHandoffScan handoff,
        GamePackageCandidateRecipeLogScan logScan,
        GamePackageCandidateRecipeNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 131 GamePackage Candidate Recipe Catalog Scoring and Promotion",
            string.Empty,
            "- recipePipelineStatus: " + dashboard.RecipePipelineStatus,
            "- recipeCount: " + dashboard.RecipeCount,
            "- candidateCount: " + dashboard.CandidateCount,
            "- passedCandidates: " + dashboard.PassedCandidates,
            "- failedCandidates: " + dashboard.FailedCandidates,
            "- matrixPassed: " + dashboard.MatrixPassed.ToString().ToLowerInvariant(),
            "- selectedCandidateId: " + dashboard.SelectedCandidateId,
            "- selectedCandidateScore: " + dashboard.SelectedCandidateScore,
            "- recipeCatalogPath: " + dashboard.RecipeCatalogPath,
            "- pipelineResultPath: " + dashboard.PipelineResultPath,
            "- scoringResultPath: " + dashboard.ScoringResultPath,
            "- matrixResultPath: " + dashboard.MatrixResultPath,
            "- selectedCandidatePackagePath: " + dashboard.SelectedCandidatePackagePath,
            "- selectedCandidateHandoffPath: " + dashboard.SelectedCandidateHandoffPath,
            "- normalCommand: " + dashboard.NormalCommand,
            "- manualUnityOptional: " + dashboard.ManualUnityOptional.ToString().ToLowerInvariant(),
            "- samplePackageUnmodified: " + dashboard.SamplePackageUnmodified.ToString().ToLowerInvariant(),
            "- projectionOnly: " + dashboard.ProjectionOnly.ToString().ToLowerInvariant(),
            "- metadataOnlyRecipeMutation: "
            + dashboard.MetadataOnlyRecipeMutation.ToString().ToLowerInvariant(),
            string.Empty,
            "## Scans",
            string.Empty,
            "- scriptScanPassed: " + scriptScan.Passed.ToString().ToLowerInvariant(),
            "- catalogPassed: " + catalog.Passed.ToString().ToLowerInvariant(),
            "- candidateIndexPassed: " + candidateIndex.Passed.ToString().ToLowerInvariant(),
            "- pipelineResultPassed: " + pipelineResult.Passed.ToString().ToLowerInvariant(),
            "- scoringResultPassed: " + scoringResult.Passed.ToString().ToLowerInvariant(),
            "- matrixResultPassed: " + matrixResult.Passed.ToString().ToLowerInvariant(),
            "- selectedHandoffPassed: " + handoff.Passed.ToString().ToLowerInvariant(),
            "- logScanPassed: " + logScan.Passed.ToString().ToLowerInvariant(),
            "- negativeProofPassed: " + negative.Passed.ToString().ToLowerInvariant()
        };
        if (dashboard.Diagnostics.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("## Diagnostics");
            lines.Add(string.Empty);
            lines.AddRange(dashboard.Diagnostics.Select(item => "- " + item));
        }

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderDocumentation(GamePackageCandidateRecipePipelineDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# GamePackage Candidate Recipe Catalog Scoring and Promotion",
            string.Empty,
            "Goal131 adds a deterministic recipe catalog over the existing GamePackage candidate matrix route. The catalog creates projection-safe metadata-only candidates, scores only candidates that pass the real matrix runner, and writes a selected candidate handoff for manual review.",
            string.Empty,
            "## Normal Command",
            string.Empty,
            "- `" + dashboard.NormalCommand + "`",
            string.Empty,
            "## Status",
            string.Empty,
            "- recipePipelineStatus: " + dashboard.RecipePipelineStatus,
            "- recipeCount: " + dashboard.RecipeCount,
            "- candidateCount: " + dashboard.CandidateCount,
            "- passedCandidates: " + dashboard.PassedCandidates,
            "- failedCandidates: " + dashboard.FailedCandidates,
            "- matrixPassed: " + dashboard.MatrixPassed.ToString().ToLowerInvariant(),
            "- selectedCandidateId: " + dashboard.SelectedCandidateId,
            "- selectedCandidateScore: " + dashboard.SelectedCandidateScore,
            "- pipelineResultPath: " + dashboard.PipelineResultPath,
            "- scoringResultPath: " + dashboard.ScoringResultPath,
            "- selectedCandidatePackagePath: " + dashboard.SelectedCandidatePackagePath,
            "- manualUnityOptional: " + dashboard.ManualUnityOptional.ToString().ToLowerInvariant(),
            "- samplePackageUnmodified: " + dashboard.SamplePackageUnmodified.ToString().ToLowerInvariant(),
            "- projectionOnly: " + dashboard.ProjectionOnly.ToString().ToLowerInvariant(),
            string.Empty,
            "## Scope Guard",
            string.Empty,
            "- The sample package remains read-only.",
            "- Generated candidates stay under Goal131 procedural artifacts.",
            "- The selected candidate is copied only to the Goal131 selected-candidate handoff path.",
            "- Runtime, public schema, provider, Lua, generator-library, Unity Assets, ProjectSettings, Packages, StreamingAssets and release packaging remain outside this goal."
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Array
            ? property.EnumerateArray()
                .Select(item => item.GetString() ?? string.Empty)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList()
            : [];

    private static string StringValue(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static bool BoolValue(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.True;

    private static int IntValue(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private static void Require(bool condition, string code, List<string> errors)
    {
        if (!condition)
        {
            errors.Add(code);
        }
    }

    private static string ResolveRepositoryRoot(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Repository root path is required.", nameof(path));
        }

        return Path.GetFullPath(path);
    }

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine;

    private static string Resolve(string root, string relativePath) =>
        Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

    private static async Task WriteTextAsync(
        string path,
        string text,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)
                                  ?? throw new InvalidOperationException("Missing directory."));
        await File.WriteAllTextAsync(path, text, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
    }

    private static void GuardNotManualInput(string root, string path)
    {
        var relative = Relative(root, path);
        if (relative.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Goal131 must not write the manual input path.");
        }
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
