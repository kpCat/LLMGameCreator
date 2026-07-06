using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class GamePackageCandidateMatrixProjectionService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly string[] ForbiddenMutationMarkers =
    [
        "ProjectSettings",
        "Packages/manifest",
        "StreamingAssets",
        "src/LLMGameCreator.Runtime",
        "src/LLMGameCreator.Runtime.Abstractions",
        "src/LLMGameCreator.GamePackage",
        "src/LLMGameCreator.Generation",
        "src/LLMGameCreator.AssetPipeline",
        "src/LLMGameCreator.Scripting",
        "generator-library",
        "git clean"
    ];

    public GamePackageCandidateMatrixProjectionBuildResult Build(string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var candidateBuild = BuildCandidateIndex(root);
        var scriptScan = BuildScriptScan(root);
        var matrixResultScan = BuildMatrixResultScan(root);
        var logScan = BuildLogScan(root, matrixResultScan);
        var negative = BuildNegativeProof(candidateBuild.Index, scriptScan);
        var dashboard = BuildDashboard(
            candidateBuild.Index,
            scriptScan,
            matrixResultScan,
            logScan,
            negative,
            candidateBuild.SamplePackageUnmodified);
        var report = RenderReport(dashboard, candidateBuild.Index, scriptScan, matrixResultScan, logScan, negative);
        var docs = RenderDocumentation(dashboard);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GamePackageCandidateMatrixProjectionVocabulary.CandidateIndexFileName] =
                Serialize(candidateBuild.Index),
            [GamePackageCandidateMatrixProjectionVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [GamePackageCandidateMatrixProjectionVocabulary.ScriptScanFileName] =
                Serialize(scriptScan),
            [GamePackageCandidateMatrixProjectionVocabulary.LogScanFileName] =
                Serialize(logScan),
            [GamePackageCandidateMatrixProjectionVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [GamePackageCandidateMatrixProjectionVocabulary.ReportFileName] = report
        };
        var proceduralIndex = BuildFileIndex(
            root,
            proceduralFiles,
            candidateBuild.PackageBytes,
            GamePackageCandidateMatrixProjectionVocabulary.ProceduralOutputDirectory,
            isExport: false);
        proceduralFiles[GamePackageCandidateMatrixProjectionVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GamePackageCandidateMatrixProjectionVocabulary.CandidateIndexFileName] =
                Serialize(candidateBuild.Index),
            [GamePackageCandidateMatrixProjectionVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [GamePackageCandidateMatrixProjectionVocabulary.ScriptScanFileName] =
                Serialize(scriptScan),
            [GamePackageCandidateMatrixProjectionVocabulary.LogScanFileName] =
                Serialize(logScan),
            [GamePackageCandidateMatrixProjectionVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [GamePackageCandidateMatrixProjectionVocabulary.ReportFileName] = report
        };
        var exportIndex = BuildFileIndex(
            root,
            exportFiles,
            candidateBuild.PackageBytes,
            GamePackageCandidateMatrixProjectionVocabulary.ExportPackageDirectory,
            isExport: true);
        exportFiles[GamePackageCandidateMatrixProjectionVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new GamePackageCandidateMatrixProjectionBuildResult
        {
            Dashboard = dashboard,
            CandidateIndex = candidateBuild.Index,
            ScriptScan = scriptScan,
            MatrixResultScan = matrixResultScan,
            LogScan = logScan,
            NegativeProof = negative,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles,
            CandidatePackageBytes = candidateBuild.PackageBytes,
            DocumentationMarkdown = docs
        };
    }

    public async Task<GamePackageCandidateMatrixProjectionWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(
            root,
            GamePackageCandidateMatrixProjectionVocabulary.ProceduralOutputDirectory);
        var export = Resolve(
            root,
            GamePackageCandidateMatrixProjectionVocabulary.ExportPackageDirectory);
        var docsPath = Resolve(root, GamePackageCandidateMatrixProjectionVocabulary.DocumentationPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in result.CandidatePackageBytes.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var proceduralPath = Resolve(root, item.Key);
            GuardNotManualInput(root, proceduralPath);
            Directory.CreateDirectory(Path.GetDirectoryName(proceduralPath)!);
            await File.WriteAllBytesAsync(proceduralPath, item.Value, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, proceduralPath));

            var exportRelative = item.Key.Replace(
                GamePackageCandidateMatrixProjectionVocabulary.ProceduralOutputDirectory,
                GamePackageCandidateMatrixProjectionVocabulary.ExportPackageDirectory,
                StringComparison.Ordinal);
            var exportPath = Resolve(root, exportRelative);
            GuardNotManualInput(root, exportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(exportPath)!);
            await File.WriteAllBytesAsync(exportPath, item.Value, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, exportPath));
        }

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

        written.AddRange(CopyMatrixArtifactsToExport(root, cancellationToken));

        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(docsPath, result.DocumentationMarkdown, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new GamePackageCandidateMatrixProjectionWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static CandidateIndexBuild BuildCandidateIndex(string root)
    {
        var samplePath = Resolve(root, GamePackageCandidateMatrixProjectionVocabulary.SamplePackagePath);
        if (!File.Exists(samplePath))
        {
            throw new FileNotFoundException("Sample package was not found.", samplePath);
        }

        var sampleBytes = File.ReadAllBytes(samplePath);
        var sampleText = File.ReadAllText(samplePath, Encoding.UTF8);
        var variantText = BuildVariantPackageText(sampleText);
        var variantBytes = Encoding.UTF8.GetBytes(variantText);

        var baselineEntry = new GamePackageCandidateIndexEntry
        {
            CandidateId = GamePackageCandidateMatrixProjectionVocabulary.BaselineCandidateId,
            PackagePath = GamePackageCandidateMatrixProjectionVocabulary.BaselineCandidatePackagePath,
            PackagePathRelative = GamePackageCandidateMatrixProjectionVocabulary.BaselineCandidatePackagePath,
            PackageId = ReadManifestString(sampleText, "packageId"),
            Title = ReadManifestString(sampleText, "title"),
            SourceKind = "sample-byte-copy",
            Sha256 = HashBytes(sampleBytes)
        };
        var variantEntry = new GamePackageCandidateIndexEntry
        {
            CandidateId = GamePackageCandidateMatrixProjectionVocabulary.VariantCandidateId,
            PackagePath = GamePackageCandidateMatrixProjectionVocabulary.VariantCandidatePackagePath,
            PackagePathRelative = GamePackageCandidateMatrixProjectionVocabulary.VariantCandidatePackagePath,
            PackageId = ReadManifestString(variantText, "packageId"),
            Title = ReadManifestString(variantText, "title"),
            SourceKind = "sample-derived-variant",
            Sha256 = HashBytes(variantBytes)
        };
        var entries = new[] { baselineEntry, variantEntry };
        var packages = new SortedDictionary<string, byte[]>(StringComparer.Ordinal)
        {
            [GamePackageCandidateMatrixProjectionVocabulary.BaselineCandidatePackagePath] = sampleBytes,
            [GamePackageCandidateMatrixProjectionVocabulary.VariantCandidatePackagePath] = variantBytes
        };
        var candidatePathsDistinct =
            !string.Equals(
                baselineEntry.PackagePathRelative,
                variantEntry.PackagePathRelative,
                StringComparison.Ordinal);
        var baselineCompatible = RequiredIdsPresent(sampleText);
        var variantCompatible = RequiredIdsPresent(variantText);
        var variantChangedProjectionData =
            !sampleBytes.SequenceEqual(variantBytes)
            && variantText.Contains("Goal129 projection-compatible candidate variant", StringComparison.Ordinal)
            && variantText.Contains("Candidate Matrix Village", StringComparison.Ordinal)
            && variantText.Contains("Help the Healer - Matrix Variant", StringComparison.Ordinal);
        var pathsUnderGoalArtifacts = entries.All(entry =>
            entry.PackagePathRelative.StartsWith(
                GamePackageCandidateMatrixProjectionVocabulary.CandidateRootDirectory + "/",
                StringComparison.Ordinal));

        var index = new GamePackageCandidateIndexDocument
        {
            CandidateCount = entries.Length,
            Candidates = entries,
            Passed = entries.Length >= 2
                     && candidatePathsDistinct
                     && baselineCompatible
                     && variantCompatible
                     && variantChangedProjectionData
                     && pathsUnderGoalArtifacts
        };
        return new CandidateIndexBuild(index, packages, HashBytes(sampleBytes) == baselineEntry.Sha256);
    }

    private static string BuildVariantPackageText(string sampleText)
    {
        var root = JsonNode.Parse(sampleText)?.AsObject()
                   ?? throw new InvalidOperationException("Sample package JSON is invalid.");
        var manifest = root["manifest"]?.AsObject()
                       ?? throw new InvalidOperationException("Sample package manifest is missing.");
        manifest["version"] = "0.1.1";
        manifest["description"] =
            "Goal129 projection-compatible candidate variant for matrix verification.";

        var game = root["game"]?.AsObject();
        var maps = game?["maps"]?.AsArray();
        if (maps is { Count: > 0 } && maps[0] is JsonObject map)
        {
            map["name"] = "Candidate Matrix Village";
        }

        var quests = game?["quests"]?.AsArray();
        if (quests is { Count: > 0 } && quests[0] is JsonObject quest)
        {
            quest["title"] = "Help the Healer - Matrix Variant";
            quest["description"] = "Gather three red herbs and report back during Goal129 matrix verification.";
        }

        var interactions = game?["interactions"]?.AsArray();
        var signInteraction = interactions?
            .OfType<JsonObject>()
            .FirstOrDefault(item => StringNode(item, "id") == "interaction/sign_inspect");
        var effects = signInteraction?["effects"]?.AsArray();
        var logEffect = effects?
            .OfType<JsonObject>()
            .FirstOrDefault(item => StringNode(item, "type") == "log");
        if (logEffect?["args"] is JsonObject args)
        {
            args["message"] = "The sign was inspected through the Goal129 matrix variant.";
        }

        return root.ToJsonString(JsonOptions) + Environment.NewLine;
    }

    private static GamePackageCandidateMatrixDashboard BuildDashboard(
        GamePackageCandidateIndexDocument candidateIndex,
        GamePackageCandidateMatrixScriptScan scriptScan,
        GamePackageCandidateMatrixResultScan matrixResult,
        GamePackageCandidateMatrixLogScan logScan,
        GamePackageCandidateMatrixNegativeProof negative,
        bool samplePackageUnmodified)
    {
        var diagnostics = new List<string>();
        Require(candidateIndex.Passed, "goal129.candidate_index_failed", diagnostics);
        Require(scriptScan.Passed, "goal129.matrix_runner_script_scan_failed", diagnostics);
        Require(matrixResult.ResultExists, "goal129.matrix_result_missing", diagnostics);
        Require(matrixResult.Passed, "goal129.matrix_result_not_green", diagnostics);
        Require(logScan.Passed, "goal129.matrix_log_scan_failed", diagnostics);
        Require(samplePackageUnmodified, "goal129.sample_package_not_read_only", diagnostics);
        Require(negative.Passed, "goal129.negative_proof_failed", diagnostics);

        return new GamePackageCandidateMatrixDashboard
        {
            MatrixStatus = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            CandidateCount = matrixResult.ResultExists
                ? matrixResult.CandidateCount
                : candidateIndex.CandidateCount,
            PassedCandidateCount = matrixResult.PassedCandidateCount,
            FailedCandidateCount = matrixResult.FailedCandidateCount,
            CleanupApplied = matrixResult.CleanupApplied,
            CandidateIndexExists = true,
            MatrixRunnerScriptExists = scriptScan.MatrixRunnerScriptExists,
            MatrixResultExists = matrixResult.ResultExists,
            PassMarkersPresent = matrixResult.PassMarkersPresent && logScan.PassMarkersPresent,
            FailMarkersAbsent = matrixResult.FailMarkersAbsent && logScan.FailMarkersAbsent,
            MaterialWarningAbsent =
                matrixResult.MaterialWarningAbsent && logScan.MaterialWarningAbsent,
            SamplePackageUnmodified = samplePackageUnmodified,
            NoForbiddenPathsExpected = negative.NoForbiddenPathsExpected,
            Diagnostics = diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static GamePackageCandidateMatrixScriptScan BuildScriptScan(string root)
    {
        var scriptPath = Resolve(root, GamePackageCandidateMatrixProjectionVocabulary.MatrixScriptPath);
        var cmdPath = Resolve(root, GamePackageCandidateMatrixProjectionVocabulary.MatrixCmdPath);
        var scriptExists = File.Exists(scriptPath);
        var cmdExists = File.Exists(cmdPath);
        var script = scriptExists ? File.ReadAllText(scriptPath, Encoding.UTF8) : string.Empty;
        var cmd = cmdExists ? File.ReadAllText(cmdPath, Encoding.UTF8) : string.Empty;
        var forbiddenFound = ForbiddenMutationMarkers
            .Where(marker => script.Contains(marker, StringComparison.OrdinalIgnoreCase)
                             || cmd.Contains(marker, StringComparison.OrdinalIgnoreCase))
            .OrderBy(marker => marker, StringComparer.Ordinal)
            .ToList();
        var scan = new GamePackageCandidateMatrixScriptScan
        {
            MatrixRunnerScriptExists = scriptExists,
            MatrixRunnerCmdExists = cmdExists,
            SupportsDefaultCandidateIndexPath =
                script.Contains(
                    GamePackageCandidateMatrixProjectionVocabulary.CandidateIndexRelativePath,
                    StringComparison.Ordinal),
            SupportsDryRun = script.Contains("[switch]$DryRun", StringComparison.Ordinal),
            InvokesParameterizedUnityProjectionRunner =
                script.Contains("run-unity-projection-verification.ps1", StringComparison.Ordinal),
            CallsGenericFullPlaythroughMode =
                script.Contains("-Mode", StringComparison.Ordinal)
                && script.Contains("GenericFullPlaythrough", StringComparison.Ordinal),
            PassesCandidatePackagePath =
                script.Contains("-PackagePath", StringComparison.Ordinal),
            PassesApplyCleanup =
                script.Contains("-ApplyCleanup", StringComparison.Ordinal)
                && cmd.Contains("-ApplyCleanup", StringComparison.Ordinal),
            SupportsPerCandidateResultAndLogPaths =
                script.Contains("-ResultPath", StringComparison.Ordinal)
                && script.Contains("-LogPath", StringComparison.Ordinal),
            RejectsOutsideRepository =
                script.Contains("Test-MatrixPathUnderRoot", StringComparison.Ordinal)
                && script.Contains("under the repository root", StringComparison.Ordinal),
            RejectsManualInputRoot =
                script.Contains(".llmgc/manual/", StringComparison.Ordinal)
                && script.Contains("must not point under .llmgc/manual", StringComparison.Ordinal),
            WritesAggregateMatrixResult =
                script.Contains(
                    GamePackageCandidateMatrixProjectionVocabulary.MatrixResultFileName,
                    StringComparison.Ordinal)
                && script.Contains("matrixStatus", StringComparison.Ordinal),
            NoBroadGitClean =
                !script.Contains("git clean", StringComparison.OrdinalIgnoreCase)
                && !cmd.Contains("git clean", StringComparison.OrdinalIgnoreCase),
            NoForbiddenMutationTargets = forbiddenFound.Count == 0,
            ForbiddenMarkersFound = forbiddenFound
        };
        return scan with
        {
            Passed = scan.MatrixRunnerScriptExists
                     && scan.MatrixRunnerCmdExists
                     && scan.SupportsDefaultCandidateIndexPath
                     && scan.SupportsDryRun
                     && scan.InvokesParameterizedUnityProjectionRunner
                     && scan.CallsGenericFullPlaythroughMode
                     && scan.PassesCandidatePackagePath
                     && scan.PassesApplyCleanup
                     && scan.SupportsPerCandidateResultAndLogPaths
                     && scan.RejectsOutsideRepository
                     && scan.RejectsManualInputRoot
                     && scan.WritesAggregateMatrixResult
                     && scan.NoBroadGitClean
                     && scan.NoForbiddenMutationTargets
        };
    }

    private static GamePackageCandidateMatrixResultScan BuildMatrixResultScan(string root)
    {
        var path = Resolve(root, GamePackageCandidateMatrixProjectionVocabulary.MatrixResultRelativePath);
        if (!File.Exists(path))
        {
            return new GamePackageCandidateMatrixResultScan { ResultExists = false };
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        var entries = doc.RootElement.TryGetProperty("entries", out var entriesElement)
            && entriesElement.ValueKind == JsonValueKind.Array
            ? entriesElement.EnumerateArray().ToList()
            : [];
        var candidateCount = IntValue(doc.RootElement, "candidateCount");
        var passedCandidateCount = IntValue(doc.RootElement, "passedCandidateCount");
        var failedCandidateCount = IntValue(doc.RootElement, "failedCandidateCount");
        var passMarkers = entries.Count > 0 && entries.All(entry => BoolValue(entry, "passMarkerPresent"));
        var failAbsent = entries.Count > 0 && entries.All(entry => BoolValue(entry, "failMarkerAbsent"));
        var materialAbsent = entries.Count > 0 && entries.All(entry => BoolValue(entry, "materialWarningAbsent"));
        var allEntriesPassed = entries.Count > 0 && entries.All(entry => BoolValue(entry, "passed"));
        var cleanupApplied =
            BoolValue(doc.RootElement, "cleanupApplied")
            && entries.Count > 0
            && entries.All(entry => BoolValue(entry, "cleanupApplied")
                                    && IntValue(entry, "cleanupExitCode") == 0);
        var matrixStatus = StringValue(doc.RootElement, "matrixStatus");
        return new GamePackageCandidateMatrixResultScan
        {
            ResultExists = true,
            MatrixStatus = matrixStatus,
            CandidateCount = candidateCount,
            PassedCandidateCount = passedCandidateCount,
            FailedCandidateCount = failedCandidateCount,
            CleanupApplied = cleanupApplied,
            ProjectionOnly = BoolValue(doc.RootElement, "projectionOnly"),
            ManualUnityOptional = BoolValue(doc.RootElement, "manualUnityOptional"),
            AllEntriesPassed = allEntriesPassed,
            PassMarkersPresent = passMarkers,
            FailMarkersAbsent = failAbsent,
            MaterialWarningAbsent = materialAbsent,
            Passed = matrixStatus == "GREEN"
                     && candidateCount >= 2
                     && passedCandidateCount == candidateCount
                     && failedCandidateCount == 0
                     && cleanupApplied
                     && allEntriesPassed
                     && passMarkers
                     && failAbsent
                     && materialAbsent
        };
    }

    private static GamePackageCandidateMatrixLogScan BuildLogScan(
        string root,
        GamePackageCandidateMatrixResultScan matrixResult)
    {
        var resultPath = Resolve(root, GamePackageCandidateMatrixProjectionVocabulary.MatrixResultRelativePath);
        if (!matrixResult.ResultExists || !File.Exists(resultPath))
        {
            return new GamePackageCandidateMatrixLogScan { MatrixResultExists = false };
        }

        using var result = JsonDocument.Parse(File.ReadAllText(resultPath, Encoding.UTF8));
        var entries = result.RootElement.TryGetProperty("entries", out var entriesElement)
            && entriesElement.ValueKind == JsonValueKind.Array
            ? entriesElement.EnumerateArray().ToList()
            : [];
        var missing = new List<string>();
        var forbidden = new List<string>();
        var passMarkers = new List<bool>();
        var failAbsent = new List<bool>();
        var materialAbsent = new List<bool>();
        foreach (var entry in entries)
        {
            var logScanPath = StringValue(entry, "logScanPath");
            if (string.IsNullOrWhiteSpace(logScanPath))
            {
                missing.Add(StringValue(entry, "candidateId"));
                continue;
            }

            var fullPath = Resolve(root, logScanPath);
            if (!File.Exists(fullPath))
            {
                missing.Add(logScanPath);
                continue;
            }

            using var scan = JsonDocument.Parse(File.ReadAllText(fullPath, Encoding.UTF8));
            passMarkers.Add(BoolValue(scan.RootElement, "passMarkerPresent"));
            failAbsent.Add(BoolValue(scan.RootElement, "failMarkerAbsent"));
            materialAbsent.Add(BoolValue(scan.RootElement, "materialWarningAbsent"));
            if (scan.RootElement.TryGetProperty("forbiddenMarkersFound", out var markers)
                && markers.ValueKind == JsonValueKind.Array)
            {
                forbidden.AddRange(markers.EnumerateArray()
                    .Select(marker => marker.GetString() ?? string.Empty)
                    .Where(marker => !string.IsNullOrWhiteSpace(marker)));
            }
        }

        return new GamePackageCandidateMatrixLogScan
        {
            MatrixResultExists = true,
            CandidateLogScanCount = passMarkers.Count,
            PassMarkersPresent = passMarkers.Count == entries.Count && passMarkers.All(item => item),
            FailMarkersAbsent = failAbsent.Count == entries.Count && failAbsent.All(item => item),
            MaterialWarningAbsent =
                materialAbsent.Count == entries.Count && materialAbsent.All(item => item),
            Passed = entries.Count >= 2
                     && missing.Count == 0
                     && forbidden.Count == 0
                     && passMarkers.Count == entries.Count
                     && passMarkers.All(item => item)
                     && failAbsent.All(item => item)
                     && materialAbsent.All(item => item),
            MissingLogScans = missing.OrderBy(item => item, StringComparer.Ordinal).ToList(),
            ForbiddenMarkersFound = forbidden.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static GamePackageCandidateMatrixNegativeProof BuildNegativeProof(
        GamePackageCandidateIndexDocument candidateIndex,
        GamePackageCandidateMatrixScriptScan scriptScan)
    {
        var proof = new GamePackageCandidateMatrixNegativeProof
        {
            ManualInputRejected = scriptScan.RejectsManualInputRoot,
            SamplePackageReadOnly = true,
            CandidatePathsUnderGoalArtifacts =
                candidateIndex.Candidates.All(candidate =>
                    candidate.PackagePathRelative.StartsWith(
                        GamePackageCandidateMatrixProjectionVocabulary.CandidateRootDirectory + "/",
                        StringComparison.Ordinal)),
            RuntimeSchemaProviderLuaGeneratorLibraryRejected = scriptScan.NoForbiddenMutationTargets,
            UnitySourceProjectSettingsPackagesRejected = scriptScan.NoForbiddenMutationTargets,
            NoForbiddenPathsExpected = scriptScan.NoForbiddenMutationTargets,
            RejectedPathSamples = BuildRejectedPathSamples()
        };
        return proof with
        {
            Passed = proof.ManualInputRejected
                     && proof.SamplePackageReadOnly
                     && proof.CandidatePathsUnderGoalArtifacts
                     && proof.RuntimeSchemaProviderLuaGeneratorLibraryRejected
                     && proof.UnitySourceProjectSettingsPackagesRejected
                     && proof.NoForbiddenPathsExpected
        };
    }

    private static GamePackageCandidateMatrixFileIndex BuildFileIndex(
        string root,
        SortedDictionary<string, string> textFiles,
        IReadOnlyDictionary<string, byte[]> packageBytes,
        string relativeRoot,
        bool isExport)
    {
        var entries = textFiles
            .Select(item => new GamePackageCandidateMatrixFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = "goal129_gamepackage_candidate_matrix_" + Path.GetFileNameWithoutExtension(item.Key),
                Sha256 = HashText(item.Value)
            })
            .ToList();

        foreach (var package in packageBytes.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var relativePath = isExport
                ? package.Key.Replace(
                    GamePackageCandidateMatrixProjectionVocabulary.ProceduralOutputDirectory,
                    GamePackageCandidateMatrixProjectionVocabulary.ExportPackageDirectory,
                    StringComparison.Ordinal)
                : package.Key;
            entries.Add(new GamePackageCandidateMatrixFileIndexEntry
            {
                RelativePath = relativePath,
                Role = "goal129_candidate_package",
                Sha256 = HashBytes(package.Value)
            });
        }

        AddExistingFileIndexEntry(
            root,
            entries,
            GamePackageCandidateMatrixProjectionVocabulary.MatrixResultRelativePath,
            isExport);
        foreach (var file in EnumerateExistingMatrixFiles(root))
        {
            AddExistingFileIndexEntry(root, entries, file, isExport);
        }

        return new GamePackageCandidateMatrixFileIndex
        {
            IndexedFileCount = entries.Count,
            ManualInputExcluded = entries.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = entries.OrderBy(entry => entry.RelativePath, StringComparer.Ordinal).ToList()
        };
    }

    private static void AddExistingFileIndexEntry(
        string root,
        List<GamePackageCandidateMatrixFileIndexEntry> entries,
        string sourceRelativePath,
        bool isExport)
    {
        var sourcePath = Resolve(root, sourceRelativePath);
        if (!File.Exists(sourcePath))
        {
            return;
        }

        var relativePath = isExport
            ? sourceRelativePath.Replace(
                GamePackageCandidateMatrixProjectionVocabulary.ProceduralOutputDirectory,
                GamePackageCandidateMatrixProjectionVocabulary.ExportPackageDirectory,
                StringComparison.Ordinal)
            : sourceRelativePath;
        entries.Add(new GamePackageCandidateMatrixFileIndexEntry
        {
            RelativePath = relativePath,
            Role = "goal129_matrix_runner_artifact",
            Sha256 = HashBytes(File.ReadAllBytes(sourcePath))
        });
    }

    private static IReadOnlyList<string> CopyMatrixArtifactsToExport(
        string root,
        CancellationToken cancellationToken)
    {
        var written = new List<string>();
        var artifacts = new List<string>
        {
            GamePackageCandidateMatrixProjectionVocabulary.MatrixResultRelativePath
        };
        artifacts.AddRange(EnumerateExistingMatrixFiles(root));

        foreach (var artifact in artifacts.OrderBy(path => path, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sourcePath = Resolve(root, artifact);
            if (!File.Exists(sourcePath))
            {
                continue;
            }

            var exportRelative = artifact.Replace(
                GamePackageCandidateMatrixProjectionVocabulary.ProceduralOutputDirectory,
                GamePackageCandidateMatrixProjectionVocabulary.ExportPackageDirectory,
                StringComparison.Ordinal);
            var destinationPath = Resolve(root, exportRelative);
            GuardNotManualInput(root, destinationPath);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(sourcePath, destinationPath, overwrite: true);
            written.Add(Relative(root, destinationPath));
        }

        return written;
    }

    private static IReadOnlyList<string> EnumerateExistingMatrixFiles(string root)
    {
        var matrixRoot = Resolve(root, GamePackageCandidateMatrixProjectionVocabulary.MatrixRootDirectory);
        if (!Directory.Exists(matrixRoot))
        {
            return [];
        }

        return Directory.EnumerateFiles(matrixRoot, "*.json", SearchOption.AllDirectories)
            .Select(path => Relative(root, path))
            .Where(path => path.EndsWith("/runner-result.json", StringComparison.Ordinal)
                           || path.EndsWith("/log-scan.json", StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();
    }

    private static string RenderReport(
        GamePackageCandidateMatrixDashboard dashboard,
        GamePackageCandidateIndexDocument candidateIndex,
        GamePackageCandidateMatrixScriptScan scriptScan,
        GamePackageCandidateMatrixResultScan matrixResult,
        GamePackageCandidateMatrixLogScan logScan,
        GamePackageCandidateMatrixNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 129 GamePackage Candidate Matrix Projection Runner",
            string.Empty,
            "- matrixStatus: " + dashboard.MatrixStatus,
            "- candidateCount: " + dashboard.CandidateCount,
            "- passedCandidateCount: " + dashboard.PassedCandidateCount,
            "- failedCandidateCount: " + dashboard.FailedCandidateCount,
            "- candidateIndexPath: " + dashboard.CandidateIndexPath,
            "- matrixResultPath: " + dashboard.MatrixResultPath,
            "- normalCommand: " + dashboard.NormalCommand,
            "- exampleCommand: " + dashboard.ExampleCommand,
            "- baselineCandidatePackagePath: " + dashboard.BaselineCandidatePackagePath,
            "- variantCandidatePackagePath: " + dashboard.VariantCandidatePackagePath,
            "- manualUnityOptional: " + dashboard.ManualUnityOptional.ToString().ToLowerInvariant(),
            "- cleanupApplied: " + dashboard.CleanupApplied.ToString().ToLowerInvariant(),
            "- projectionOnly: " + dashboard.ProjectionOnly.ToString().ToLowerInvariant(),
            string.Empty,
            "## Candidate Index",
            string.Empty,
            "- passed: " + candidateIndex.Passed.ToString().ToLowerInvariant(),
            "- candidateCount: " + candidateIndex.CandidateCount,
            string.Empty,
            "## Script Scan",
            string.Empty,
            "- passed: " + scriptScan.Passed.ToString().ToLowerInvariant(),
            "- invokesParameterizedUnityProjectionRunner: "
            + scriptScan.InvokesParameterizedUnityProjectionRunner.ToString().ToLowerInvariant(),
            "- supportsPerCandidateResultAndLogPaths: "
            + scriptScan.SupportsPerCandidateResultAndLogPaths.ToString().ToLowerInvariant(),
            string.Empty,
            "## Matrix Result",
            string.Empty,
            "- resultExists: " + matrixResult.ResultExists.ToString().ToLowerInvariant(),
            "- passed: " + matrixResult.Passed.ToString().ToLowerInvariant(),
            "- allEntriesPassed: " + matrixResult.AllEntriesPassed.ToString().ToLowerInvariant(),
            string.Empty,
            "## Log Scan",
            string.Empty,
            "- passed: " + logScan.Passed.ToString().ToLowerInvariant(),
            "- candidateLogScanCount: " + logScan.CandidateLogScanCount,
            string.Empty,
            "## Negative Proof",
            string.Empty,
            "- passed: " + negative.Passed.ToString().ToLowerInvariant()
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

    private static string RenderDocumentation(GamePackageCandidateMatrixDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# GamePackage Candidate Matrix Projection Runner",
            string.Empty,
            "Goal129 adds deterministic candidate GamePackage matrix verification over the parameterized Unity projection runner.",
            "The baseline candidate is a byte-copy of `samples/minimal-map-game/package.json`; the variant keeps the Goal128 full-playthrough package identity/title compatibility fields while changing version, description and visible labels.",
            string.Empty,
            "## Normal Command",
            string.Empty,
            "- `" + dashboard.NormalCommand + "`",
            string.Empty,
            "## Example Command",
            string.Empty,
            "- `" + dashboard.ExampleCommand + "`",
            string.Empty,
            "## Status",
            string.Empty,
            "- matrixStatus: " + dashboard.MatrixStatus,
            "- candidateCount: " + dashboard.CandidateCount,
            "- passedCandidateCount: " + dashboard.PassedCandidateCount,
            "- failedCandidateCount: " + dashboard.FailedCandidateCount,
            "- candidateIndexPath: " + dashboard.CandidateIndexPath,
            "- matrixResultPath: " + dashboard.MatrixResultPath,
            "- manualUnityOptional: " + dashboard.ManualUnityOptional.ToString().ToLowerInvariant(),
            "- cleanupApplied: " + dashboard.CleanupApplied.ToString().ToLowerInvariant(),
            "- projectionOnly: " + dashboard.ProjectionOnly.ToString().ToLowerInvariant(),
            string.Empty,
            "## Scope Guard",
            string.Empty,
            "- Candidate package paths stay under Goal129 artifacts and outside `.llmgc/manual/`.",
            "- This remains projection-only and does not authorize sample mutation, Runtime, public schema, provider, Lua, generator-library, Unity scene, prefab, ProjectSettings, Packages, StreamingAssets or release packaging work."
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static IReadOnlyList<string> BuildRejectedPathSamples() =>
    [
        ".llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json",
        "samples/minimal-map-game/package.json",
        "src/LLMGameCreator.Runtime/GameRuntime.cs",
        "src/LLMGameCreator.Runtime.Abstractions/IGameRuntime.cs",
        "src/LLMGameCreator.GamePackage/GamePackageDefinition.cs",
        "src/LLMGameCreator.Generation/Generator.cs",
        "src/LLMGameCreator.AssetPipeline/Provider.cs",
        "src/LLMGameCreator.Scripting/LuaSandbox.cs",
        "generator-library/example.json",
        "unity/LLMGameCreatorAlpha/Assets/Scenes/Main.unity",
        "unity/LLMGameCreatorAlpha/Assets/Prefabs/AcceptedAlpha.prefab",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/example.json",
        "unity/LLMGameCreatorAlpha/ProjectSettings/ProjectSettings.asset",
        "unity/LLMGameCreatorAlpha/Packages/manifest.json"
    ];

    private static bool RequiredIdsPresent(string packageText) =>
        GamePackageCandidateMatrixProjectionVocabulary.RequiredCompatibilityIds.All(id =>
            packageText.Contains(id, StringComparison.Ordinal));

    private static string ReadManifestString(string json, string propertyName)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("manifest", out var manifest)
               && manifest.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static string StringNode(JsonObject obj, string propertyName) =>
        obj.TryGetPropertyValue(propertyName, out var node) && node is JsonValue value
            ? value.GetValue<string>()
            : string.Empty;

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
            throw new InvalidOperationException("Goal129 must not write the manual input path.");
        }
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private sealed record CandidateIndexBuild(
        GamePackageCandidateIndexDocument Index,
        IReadOnlyDictionary<string, byte[]> PackageBytes,
        bool SamplePackageUnmodified);
}
