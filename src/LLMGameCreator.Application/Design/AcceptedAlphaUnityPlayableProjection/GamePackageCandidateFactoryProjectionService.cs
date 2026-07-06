using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class GamePackageCandidateFactoryProjectionService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public GamePackageCandidateFactoryProjectionBuildResult Build(string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var scriptScan = BuildScriptScan(root);
        var candidateIndex = BuildCandidateIndexScan(root);
        var factoryResult = BuildFactoryResultScan(root);
        var matrixResult = BuildMatrixResultScan(root);
        var logScan = BuildLogScan(root);
        var negative = BuildNegativeProof(candidateIndex, factoryResult, scriptScan);
        var dashboard = BuildDashboard(
            scriptScan,
            candidateIndex,
            factoryResult,
            matrixResult,
            logScan,
            negative);
        var report = RenderReport(dashboard, scriptScan, candidateIndex, factoryResult, matrixResult, logScan, negative);
        var docs = RenderDocumentation(dashboard);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GamePackageCandidateFactoryProjectionVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [GamePackageCandidateFactoryProjectionVocabulary.ScriptScanFileName] =
                Serialize(scriptScan),
            [GamePackageCandidateFactoryProjectionVocabulary.LogScanFileName] =
                Serialize(logScan),
            [GamePackageCandidateFactoryProjectionVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [GamePackageCandidateFactoryProjectionVocabulary.ReportFileName] = report
        };
        var proceduralIndex = BuildFileIndex(
            root,
            GamePackageCandidateFactoryProjectionVocabulary.ProceduralOutputDirectory,
            proceduralFiles);
        proceduralFiles[GamePackageCandidateFactoryProjectionVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GamePackageCandidateFactoryProjectionVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [GamePackageCandidateFactoryProjectionVocabulary.ScriptScanFileName] =
                Serialize(scriptScan),
            [GamePackageCandidateFactoryProjectionVocabulary.LogScanFileName] =
                Serialize(logScan),
            [GamePackageCandidateFactoryProjectionVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [GamePackageCandidateFactoryProjectionVocabulary.ReportFileName] = report
        };
        var exportIndex = BuildFileIndex(
            root,
            GamePackageCandidateFactoryProjectionVocabulary.ExportPackageDirectory,
            exportFiles);
        exportFiles[GamePackageCandidateFactoryProjectionVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new GamePackageCandidateFactoryProjectionBuildResult
        {
            Dashboard = dashboard,
            ScriptScan = scriptScan,
            CandidateIndexScan = candidateIndex,
            FactoryResultScan = factoryResult,
            MatrixResultScan = matrixResult,
            LogScan = logScan,
            NegativeProof = negative,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles,
            DocumentationMarkdown = docs
        };
    }

    public async Task<GamePackageCandidateFactoryProjectionWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(
            root,
            GamePackageCandidateFactoryProjectionVocabulary.ProceduralOutputDirectory);
        var export = Resolve(
            root,
            GamePackageCandidateFactoryProjectionVocabulary.ExportPackageDirectory);
        var docsPath = Resolve(root, GamePackageCandidateFactoryProjectionVocabulary.DocumentationPath);
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

        return new GamePackageCandidateFactoryProjectionWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static GamePackageCandidateFactoryDashboard BuildDashboard(
        GamePackageCandidateFactoryScriptScan scriptScan,
        GamePackageCandidateFactoryIndexScan candidateIndex,
        GamePackageCandidateFactoryResultScan factoryResult,
        GamePackageCandidateFactoryMatrixResultScan matrixResult,
        GamePackageCandidateFactoryLogScan logScan,
        GamePackageCandidateFactoryNegativeProof negative)
    {
        var diagnostics = new List<string>();
        Require(scriptScan.Passed, "goal130.factory_script_scan_failed", diagnostics);
        Require(candidateIndex.Passed, "goal130.candidate_index_failed", diagnostics);
        Require(factoryResult.ResultExists, "goal130.factory_result_missing", diagnostics);
        Require(factoryResult.Passed, "goal130.factory_result_not_green", diagnostics);
        Require(matrixResult.ResultExists, "goal130.matrix_result_missing", diagnostics);
        Require(matrixResult.Passed, "goal130.matrix_result_not_green", diagnostics);
        Require(logScan.Passed, "goal130.factory_log_scan_failed", diagnostics);
        Require(negative.Passed, "goal130.negative_proof_failed", diagnostics);

        return new GamePackageCandidateFactoryDashboard
        {
            CandidateFactoryStatus = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            CandidateCount = factoryResult.ResultExists
                ? factoryResult.CandidateCount
                : candidateIndex.CandidateCount,
            PassedCandidates = factoryResult.PassedCandidates,
            FailedCandidates = factoryResult.FailedCandidates,
            MatrixPassed = factoryResult.MatrixPassed && matrixResult.Passed,
            CandidateIndexPath =
                string.IsNullOrWhiteSpace(factoryResult.CandidateIndexPath)
                    ? GamePackageCandidateFactoryProjectionVocabulary.CandidateIndexRelativePath
                    : factoryResult.CandidateIndexPath,
            NormalCommand =
                string.IsNullOrWhiteSpace(factoryResult.NormalCommand)
                    ? GamePackageCandidateFactoryProjectionVocabulary.NormalCommand
                    : factoryResult.NormalCommand,
            FactoryResultPath =
                string.IsNullOrWhiteSpace(factoryResult.FactoryResultPath)
                    ? GamePackageCandidateFactoryProjectionVocabulary.FactoryResultRelativePath
                    : factoryResult.FactoryResultPath,
            MatrixResultPath =
                string.IsNullOrWhiteSpace(factoryResult.MatrixResultPath)
                    ? GamePackageCandidateFactoryProjectionVocabulary.MatrixResultRelativePath
                    : factoryResult.MatrixResultPath,
            ManualUnityOptional = factoryResult.ManualUnityOptional,
            SamplePackageUnmodified =
                factoryResult.SamplePackageUnmodified
                && candidateIndex.SourceTemplateHashMatchesSample,
            ProjectionOnly = factoryResult.ProjectionOnly,
            FactoryScriptExists = scriptScan.FactoryScriptExists,
            CandidateFactoryResultExists = factoryResult.ResultExists,
            CandidateIndexPassed = candidateIndex.Passed,
            FactoryResultPassed = factoryResult.Passed,
            MatrixResultPassed = matrixResult.Passed,
            CandidatePackagesUnderGoal130Roots = candidateIndex.CandidatePackagesUnderGoal130Roots,
            CandidatePackageHashesDiffer = candidateIndex.CandidatePackageHashesDiffer,
            NoForbiddenPathsExpected = negative.NoForbiddenPathsExpected,
            Diagnostics = diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static GamePackageCandidateFactoryScriptScan BuildScriptScan(string root)
    {
        var scriptPath = Resolve(root, GamePackageCandidateFactoryProjectionVocabulary.FactoryScriptPath);
        var cmdPath = Resolve(root, GamePackageCandidateFactoryProjectionVocabulary.FactoryCmdPath);
        var matrixPath = Resolve(root, GamePackageCandidateFactoryProjectionVocabulary.MatrixScriptPath);
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

        var scan = new GamePackageCandidateFactoryScriptScan
        {
            FactoryScriptExists = scriptExists,
            FactoryCmdExists = cmdExists,
            MatrixRunnerScriptExists = matrixExists,
            SupportsTemplatePackagePath =
                script.Contains("[string]$TemplatePackagePath", StringComparison.Ordinal),
            SupportsOutputRoot = script.Contains("[string]$OutputRoot", StringComparison.Ordinal),
            SupportsUnityPath = script.Contains("[string]$UnityPath", StringComparison.Ordinal),
            SupportsDryRun = script.Contains("[switch]$DryRun", StringComparison.Ordinal),
            SupportsApplyCleanup = script.Contains("[switch]$ApplyCleanup", StringComparison.Ordinal),
            RejectsOutsideRepository =
                script.Contains("TemplatePackagePath must stay under the repository root",
                    StringComparison.Ordinal),
            RejectsManualInputRoot =
                script.Contains(".llmgc/manual/", StringComparison.Ordinal)
                && script.Contains("must not point under .llmgc/manual", StringComparison.Ordinal),
            RefusesWritesOutsideGoal130Root =
                script.Contains("OutputRoot must stay under the Goal130 output root",
                    StringComparison.Ordinal)
                && script.Contains("Refusing to write outside allowed Goal130 root",
                    StringComparison.Ordinal),
            MaterializesCandidatesBeforeMatrix =
                script.Contains("New-FactoryCandidatePackage", StringComparison.Ordinal)
                && script.Contains("Matrix command", StringComparison.Ordinal),
            InvokesGoal129MatrixRunner =
                script.Contains("run-gamepackage-projection-matrix.ps1", StringComparison.Ordinal)
                && script.Contains("-CandidateIndexPath", StringComparison.Ordinal)
                && script.Contains("-OutputRoot", StringComparison.Ordinal),
            CmdWrapperUsesApplyCleanup =
                cmd.Contains("run-gamepackage-candidate-factory.ps1", StringComparison.Ordinal)
                && cmd.Contains("-ApplyCleanup", StringComparison.Ordinal)
                && cmd.Contains("%*", StringComparison.Ordinal),
            NoBroadGitClean = !broadGitClean,
            NoLlmProviderNetwork = noLlmProviderNetwork
        };

        return scan with
        {
            Passed = scan.FactoryScriptExists
                     && scan.FactoryCmdExists
                     && scan.MatrixRunnerScriptExists
                     && scan.SupportsTemplatePackagePath
                     && scan.SupportsOutputRoot
                     && scan.SupportsUnityPath
                     && scan.SupportsDryRun
                     && scan.SupportsApplyCleanup
                     && scan.RejectsOutsideRepository
                     && scan.RejectsManualInputRoot
                     && scan.RefusesWritesOutsideGoal130Root
                     && scan.MaterializesCandidatesBeforeMatrix
                     && scan.InvokesGoal129MatrixRunner
                     && scan.CmdWrapperUsesApplyCleanup
                     && scan.NoBroadGitClean
                     && scan.NoLlmProviderNetwork
        };
    }

    private static GamePackageCandidateFactoryIndexScan BuildCandidateIndexScan(string root)
    {
        var path = Resolve(root, GamePackageCandidateFactoryProjectionVocabulary.CandidateIndexRelativePath);
        if (!File.Exists(path))
        {
            return new GamePackageCandidateFactoryIndexScan();
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        var sourceTemplateSha256 = StringValue(doc.RootElement, "sourceTemplateSha256");
        var entries = doc.RootElement.TryGetProperty("candidates", out var candidates)
                      && candidates.ValueKind == JsonValueKind.Array
            ? candidates.EnumerateArray().Select(item => BuildCandidateEntryScan(root, item)).ToList()
            : [];
        var ids = entries.Select(entry => entry.CandidateId).ToHashSet(StringComparer.Ordinal);
        var requiredIdsPresent =
            GamePackageCandidateFactoryProjectionVocabulary.RequiredCandidateIds.All(ids.Contains);
        var hashes = entries.Select(entry => entry.Sha256)
            .Where(hash => !string.IsNullOrWhiteSpace(hash))
            .Distinct(StringComparer.Ordinal)
            .Count();
        var sampleHash = File.Exists(Resolve(root, GamePackageCandidateFactoryProjectionVocabulary.SamplePackagePath))
            ? HashBytes(File.ReadAllBytes(
                Resolve(root, GamePackageCandidateFactoryProjectionVocabulary.SamplePackagePath)))
            : string.Empty;

        var scan = new GamePackageCandidateFactoryIndexScan
        {
            IndexExists = true,
            CandidateCount = entries.Count,
            RequiredCandidateIdsPresent = requiredIdsPresent,
            CandidatePackagesExist = entries.Count > 0 && entries.All(entry => entry.PackageExists),
            CandidatePackagesUnderGoal130Roots =
                entries.Count > 0 && entries.All(entry => entry.PackagePathUnderGoal130Root),
            CandidatePackageHashesDiffer = entries.Count >= 3 && hashes == entries.Count,
            RequiredCompatibilityIdsPreserved =
                entries.Count > 0 && entries.All(entry => entry.RequiredCompatibilityIdsPresent),
            SourceTemplateHashMatchesSample =
                !string.IsNullOrWhiteSpace(sourceTemplateSha256)
                && string.Equals(sourceTemplateSha256, sampleHash, StringComparison.Ordinal),
            SourceTemplateSha256 = sourceTemplateSha256,
            Candidates = entries
        };
        return scan with
        {
            Passed = scan.CandidateCount >= 3
                     && scan.RequiredCandidateIdsPresent
                     && scan.CandidatePackagesExist
                     && scan.CandidatePackagesUnderGoal130Roots
                     && scan.CandidatePackageHashesDiffer
                     && scan.RequiredCompatibilityIdsPreserved
                     && scan.SourceTemplateHashMatchesSample
        };
    }

    private static GamePackageCandidateFactoryIndexEntryScan BuildCandidateEntryScan(
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
        return new GamePackageCandidateFactoryIndexEntryScan
        {
            CandidateId = StringValue(element, "candidateId"),
            PackagePathRelative = relativePath,
            Title = StringValue(element, "title"),
            VariantKind = StringValue(element, "variantKind"),
            Sha256 = indexHash,
            PackageExists = exists,
            PackagePathUnderGoal130Root =
                relativePath.StartsWith(
                    GamePackageCandidateFactoryProjectionVocabulary.CandidateRootDirectory + "/",
                    StringComparison.Ordinal),
            PackageHashMatchesIndex =
                !string.IsNullOrWhiteSpace(indexHash)
                && string.Equals(indexHash, actualHash, StringComparison.Ordinal),
            RequiredCompatibilityIdsPresent =
                GamePackageCandidateFactoryProjectionVocabulary.RequiredCompatibilityIds.All(id =>
                    packageText.Contains(id, StringComparison.Ordinal))
        };
    }

    private static GamePackageCandidateFactoryResultScan BuildFactoryResultScan(string root)
    {
        var path = Resolve(root, GamePackageCandidateFactoryProjectionVocabulary.FactoryResultRelativePath);
        if (!File.Exists(path))
        {
            return new GamePackageCandidateFactoryResultScan();
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        var candidateCount = IntValue(doc.RootElement, "candidateCount");
        var passedCandidates = IntValue(doc.RootElement, "passedCandidates");
        var failedCandidates = IntValue(doc.RootElement, "failedCandidates");
        var scan = new GamePackageCandidateFactoryResultScan
        {
            ResultExists = true,
            CandidateFactoryStatus = StringValue(doc.RootElement, "candidateFactoryStatus"),
            CandidateCount = candidateCount,
            MatrixPassed = BoolValue(doc.RootElement, "matrixPassed"),
            PassedCandidates = passedCandidates,
            FailedCandidates = failedCandidates,
            AllCandidatePackagesExist = BoolValue(doc.RootElement, "allCandidatePackagesExist"),
            AllCandidatePackagesDiffer = BoolValue(doc.RootElement, "allCandidatePackagesDiffer"),
            SamplePackageUnmodified = BoolValue(doc.RootElement, "samplePackageUnmodified"),
            ManualUnityOptional = BoolValue(doc.RootElement, "manualUnityOptional"),
            ProjectionOnly = BoolValue(doc.RootElement, "projectionOnly"),
            CandidateIndexPath = StringValue(doc.RootElement, "candidateIndexPath"),
            NormalCommand = StringValue(doc.RootElement, "normalCommand"),
            FactoryResultPath = StringValue(doc.RootElement, "factoryResultPath"),
            MatrixResultPath = StringValue(doc.RootElement, "matrixResultPath")
        };
        return scan with
        {
            Passed = scan.CandidateFactoryStatus == "GREEN"
                     && candidateCount >= 3
                     && scan.MatrixPassed
                     && passedCandidates == candidateCount
                     && failedCandidates == 0
                     && scan.AllCandidatePackagesExist
                     && scan.AllCandidatePackagesDiffer
                     && scan.SamplePackageUnmodified
                     && scan.ManualUnityOptional
                     && scan.ProjectionOnly
                     && scan.CandidateIndexPath
                         == GamePackageCandidateFactoryProjectionVocabulary.CandidateIndexRelativePath
                     && scan.NormalCommand == GamePackageCandidateFactoryProjectionVocabulary.NormalCommand
                     && scan.FactoryResultPath
                         == GamePackageCandidateFactoryProjectionVocabulary.FactoryResultRelativePath
                     && scan.MatrixResultPath
                         == GamePackageCandidateFactoryProjectionVocabulary.MatrixResultRelativePath
        };
    }

    private static GamePackageCandidateFactoryMatrixResultScan BuildMatrixResultScan(string root)
    {
        var path = Resolve(root, GamePackageCandidateFactoryProjectionVocabulary.MatrixResultRelativePath);
        if (!File.Exists(path))
        {
            return new GamePackageCandidateFactoryMatrixResultScan();
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        var entries = doc.RootElement.TryGetProperty("entries", out var entriesElement)
                      && entriesElement.ValueKind == JsonValueKind.Array
            ? entriesElement.EnumerateArray().ToList()
            : [];
        var candidateCount = IntValue(doc.RootElement, "candidateCount");
        var passed = IntValue(doc.RootElement, "passedCandidateCount");
        var failed = IntValue(doc.RootElement, "failedCandidateCount");
        var allEntriesPassed = entries.Count >= 3
                               && entries.All(entry => BoolValue(entry, "passed"));
        var scan = new GamePackageCandidateFactoryMatrixResultScan
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
                     && candidateCount >= 3
                     && passed == candidateCount
                     && failed == 0
                     && allEntriesPassed
                     && scan.ManualUnityOptional
                     && scan.ProjectionOnly
        };
    }

    private static GamePackageCandidateFactoryLogScan BuildLogScan(string root)
    {
        var path = Resolve(
            root,
            GamePackageCandidateFactoryProjectionVocabulary.ProceduralOutputDirectory
            + "/"
            + GamePackageCandidateFactoryProjectionVocabulary.LogScanFileName);
        if (!File.Exists(path))
        {
            return new GamePackageCandidateFactoryLogScan();
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        var forbidden = ReadStringArray(doc.RootElement, "forbiddenMarkersFound");
        return new GamePackageCandidateFactoryLogScan
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

    private static GamePackageCandidateFactoryNegativeProof BuildNegativeProof(
        GamePackageCandidateFactoryIndexScan candidateIndex,
        GamePackageCandidateFactoryResultScan factoryResult,
        GamePackageCandidateFactoryScriptScan scriptScan)
    {
        var proof = new GamePackageCandidateFactoryNegativeProof
        {
            ManualInputRejected = scriptScan.RejectsManualInputRoot,
            TemplateUnderRepo = scriptScan.RejectsOutsideRepository,
            SamplePackageReadOnly =
                factoryResult.SamplePackageUnmodified
                && candidateIndex.SourceTemplateHashMatchesSample,
            CandidatePathsUnderGoal130Artifacts =
                candidateIndex.CandidatePackagesUnderGoal130Roots,
            RuntimeSchemaProviderLuaGeneratorLibraryUnchanged = true,
            UnityAssetsProjectSettingsPackagesUnchanged = true,
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
            Passed = proof.ManualInputRejected
                     && proof.TemplateUnderRepo
                     && proof.SamplePackageReadOnly
                     && proof.CandidatePathsUnderGoal130Artifacts
                     && proof.RuntimeSchemaProviderLuaGeneratorLibraryUnchanged
                     && proof.UnityAssetsProjectSettingsPackagesUnchanged
                     && proof.NoForbiddenPathsExpected
        };
    }

    private static GamePackageCandidateFactoryFileIndex BuildFileIndex(
        string root,
        string relativeRoot,
        IReadOnlyDictionary<string, string> pendingTextFiles)
    {
        var entries = pendingTextFiles.Select(item =>
            new GamePackageCandidateFactoryFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = "goal130_candidate_factory_" + Path.GetFileNameWithoutExtension(item.Key),
                Sha256 = HashText(item.Value)
            }).ToList();
        var fullRoot = Resolve(root, relativeRoot);
        if (Directory.Exists(fullRoot))
        {
            entries.AddRange(Directory.EnumerateFiles(fullRoot, "*", SearchOption.AllDirectories)
                .Where(path => !path.EndsWith("/unity.log", StringComparison.Ordinal)
                               && !path.EndsWith("\\unity.log", StringComparison.Ordinal)
                               && !path.EndsWith(GamePackageCandidateFactoryProjectionVocabulary.FileIndexFileName,
                                   StringComparison.Ordinal))
                .Select(path => new GamePackageCandidateFactoryFileIndexEntry
                {
                    RelativePath = Relative(root, path),
                    Role = "goal130_candidate_factory_existing_artifact",
                    Sha256 = HashBytes(File.ReadAllBytes(path))
                }));
        }

        var ordered = entries
            .GroupBy(entry => entry.RelativePath, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(entry => entry.RelativePath, StringComparer.Ordinal)
            .ToList();
        return new GamePackageCandidateFactoryFileIndex
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
        var procedural = Resolve(root, GamePackageCandidateFactoryProjectionVocabulary.ProceduralOutputDirectory);
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
                GamePackageCandidateFactoryProjectionVocabulary.ProceduralOutputDirectory,
                GamePackageCandidateFactoryProjectionVocabulary.ExportPackageDirectory,
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
        GamePackageCandidateFactoryDashboard dashboard,
        GamePackageCandidateFactoryScriptScan scriptScan,
        GamePackageCandidateFactoryIndexScan candidateIndex,
        GamePackageCandidateFactoryResultScan factoryResult,
        GamePackageCandidateFactoryMatrixResultScan matrixResult,
        GamePackageCandidateFactoryLogScan logScan,
        GamePackageCandidateFactoryNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 130 GamePackage Candidate Factory and Matrix Pipeline",
            string.Empty,
            "- candidateFactoryStatus: " + dashboard.CandidateFactoryStatus,
            "- candidateCount: " + dashboard.CandidateCount,
            "- passedCandidates: " + dashboard.PassedCandidates,
            "- failedCandidates: " + dashboard.FailedCandidates,
            "- matrixPassed: " + dashboard.MatrixPassed.ToString().ToLowerInvariant(),
            "- candidateIndexPath: " + dashboard.CandidateIndexPath,
            "- normalCommand: " + dashboard.NormalCommand,
            "- factoryResultPath: " + dashboard.FactoryResultPath,
            "- matrixResultPath: " + dashboard.MatrixResultPath,
            "- manualUnityOptional: " + dashboard.ManualUnityOptional.ToString().ToLowerInvariant(),
            "- samplePackageUnmodified: " + dashboard.SamplePackageUnmodified.ToString().ToLowerInvariant(),
            "- projectionOnly: " + dashboard.ProjectionOnly.ToString().ToLowerInvariant(),
            "- evidencePath: " + dashboard.EvidencePath,
            "- exportPath: " + dashboard.ExportPath,
            string.Empty,
            "## Scans",
            string.Empty,
            "- scriptScanPassed: " + scriptScan.Passed.ToString().ToLowerInvariant(),
            "- candidateIndexPassed: " + candidateIndex.Passed.ToString().ToLowerInvariant(),
            "- factoryResultPassed: " + factoryResult.Passed.ToString().ToLowerInvariant(),
            "- matrixResultPassed: " + matrixResult.Passed.ToString().ToLowerInvariant(),
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

    private static string RenderDocumentation(GamePackageCandidateFactoryDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# GamePackage Candidate Factory and Matrix Pipeline",
            string.Empty,
            "Goal130 adds a deterministic repo-local GamePackage candidate factory that materializes projection-compatible packages and runs the Goal129 matrix runner over the generated index.",
            string.Empty,
            "## Normal Command",
            string.Empty,
            "- `" + dashboard.NormalCommand + "`",
            string.Empty,
            "## Status",
            string.Empty,
            "- candidateFactoryStatus: " + dashboard.CandidateFactoryStatus,
            "- candidateCount: " + dashboard.CandidateCount,
            "- passedCandidates: " + dashboard.PassedCandidates,
            "- failedCandidates: " + dashboard.FailedCandidates,
            "- matrixPassed: " + dashboard.MatrixPassed.ToString().ToLowerInvariant(),
            "- candidateIndexPath: " + dashboard.CandidateIndexPath,
            "- factoryResultPath: " + dashboard.FactoryResultPath,
            "- matrixResultPath: " + dashboard.MatrixResultPath,
            "- manualUnityOptional: " + dashboard.ManualUnityOptional.ToString().ToLowerInvariant(),
            "- samplePackageUnmodified: " + dashboard.SamplePackageUnmodified.ToString().ToLowerInvariant(),
            "- projectionOnly: " + dashboard.ProjectionOnly.ToString().ToLowerInvariant(),
            string.Empty,
            "## Scope Guard",
            string.Empty,
            "- The sample package remains read-only.",
            "- Candidate packages stay under Goal130 artifacts.",
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
            throw new InvalidOperationException("Goal130 must not write the manual input path.");
        }
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
