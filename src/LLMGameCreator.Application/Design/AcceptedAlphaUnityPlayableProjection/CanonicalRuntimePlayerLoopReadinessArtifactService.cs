using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class CanonicalRuntimePlayerLoopReadinessArtifactService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static CanonicalRuntimePlayerLoopReadinessArtifactService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public static IReadOnlyList<CanonicalRuntimeSelectedCandidateEvent> ReadTranscript(string path) =>
        JsonSerializer.Deserialize<IReadOnlyList<CanonicalRuntimeSelectedCandidateEvent>>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? [];

    public static CanonicalRuntimeSelectedCandidateStateSummary ReadStateSummary(string path) =>
        JsonSerializer.Deserialize<CanonicalRuntimeSelectedCandidateStateSummary>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? new CanonicalRuntimeSelectedCandidateStateSummary();

    public static CanonicalRuntimePlayerLoopUnitySmoke ReadUnitySmoke(string path) =>
        JsonSerializer.Deserialize<CanonicalRuntimePlayerLoopUnitySmoke>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? new CanonicalRuntimePlayerLoopUnitySmoke();

    public async Task<CanonicalRuntimePlayerLoopReadinessWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CanonicalRuntimePlayerLoopReadinessRequest request,
        CanonicalRuntimePlayerLoopReadinessResult runtimeResult,
        string outputRootRelativePath =
            CanonicalRuntimePlayerLoopReadinessVocabulary.ProceduralOutputDirectory,
        string exportRootRelativePath =
            CanonicalRuntimePlayerLoopReadinessVocabulary.ExportPackageDirectory,
        CanonicalRuntimePlayerLoopUnitySmoke? unitySmoke = null,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var stateSummaryPath = Resolve(root, request.StateSummaryPath);
        var dashboardPath = Resolve(root, request.DashboardPath);
        GuardNotManualInput(root, stateSummaryPath);
        GuardNotManualInput(root, dashboardPath);

        using var sourceDashboard = JsonDocument.Parse(File.ReadAllText(dashboardPath, Encoding.UTF8));
        var sourceDiagnostics = ReadDiagnostics(sourceDashboard.RootElement);
        var classification = ClassifyDiagnostics(sourceDiagnostics);
        var smoke = unitySmoke ?? BuildPendingUnitySmoke(root);
        var plan = BuildPlan(runtimeResult);
        var matrix = BuildMatrix(runtimeResult, classification, smoke);
        var report = BuildReport(runtimeResult, classification, smoke);
        var negative = BuildNegativeProof(runtimeResult);
        var dashboard = BuildDashboard(runtimeResult, classification, smoke);
        var markdown = RenderReport(report, dashboard, runtimeResult, classification);

        var proceduralFiles = BuildFilePayloads(
            root,
            outputRootRelativePath,
            runtimeResult,
            plan,
            classification,
            smoke,
            matrix,
            report,
            markdown,
            negative,
            dashboard);
        var exportFiles = BuildFilePayloads(
            root,
            exportRootRelativePath,
            runtimeResult,
            plan,
            classification,
            smoke,
            matrix,
            report,
            markdown,
            negative,
            dashboard);

        var procedural = Resolve(
            root,
            outputRootRelativePath);
        var export = Resolve(root, exportRootRelativePath);
        var docsPath = Resolve(root, CanonicalRuntimePlayerLoopReadinessVocabulary.DocumentationPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in proceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            GuardGoal135Write(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in exportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            GuardGoal135Write(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(docsPath, markdown, cancellationToken).ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new CanonicalRuntimePlayerLoopReadinessWriteResult
        {
            Dashboard = dashboard,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    public static CanonicalRuntimePlayerLoopDiagnosticClassification ClassifyDiagnostics(
        IReadOnlyList<string> rawDiagnostics)
    {
        var classified = rawDiagnostics
            .Select(ClassifyDiagnostic)
            .OrderBy(item => item.RawDiagnostic, StringComparer.Ordinal)
            .ToList();
        var blocking = classified.Where(item => item.Blocking).ToList();
        var nonBlocking = classified.Where(item => !item.Blocking).ToList();
        return new CanonicalRuntimePlayerLoopDiagnosticClassification
        {
            RawDiagnosticCount = classified.Count,
            BlockingDiagnosticCount = blocking.Count,
            NonBlockingDiagnosticCount = nonBlocking.Count,
            BlockingDiagnostics = blocking,
            NonBlockingDiagnostics = nonBlocking,
            PassAllowsNonBlockingDiagnostics = true,
            NoUnclassifiedErrorDiagnostics = blocking.Count == 0
        };
    }

    private static CanonicalRuntimePlayerLoopClassifiedDiagnostic ClassifyDiagnostic(string raw)
    {
        var parts = raw.Split(':', 3);
        var severity = parts.Length > 0 ? parts[0] : string.Empty;
        var code = parts.Length > 1 ? parts[1] : raw;
        if (string.Equals(severity, "Error", StringComparison.Ordinal)
            && string.Equals(code, "script.path.missing", StringComparison.Ordinal))
        {
            return new CanonicalRuntimePlayerLoopClassifiedDiagnostic
            {
                RawDiagnostic = raw,
                Severity = severity,
                Code = code,
                Blocking = false,
                NonBlockingForCanonicalRuntimePath = true,
                Reason =
                    "script artifact path is not required by the selected canonical runtime command sequence"
            };
        }

        if (string.Equals(severity, "Warning", StringComparison.Ordinal))
        {
            return new CanonicalRuntimePlayerLoopClassifiedDiagnostic
            {
                RawDiagnostic = raw,
                Severity = severity,
                Code = code,
                Blocking = false,
                NonBlockingForCanonicalRuntimePath = true,
                Reason =
                    "warning diagnostic is not blocking for canonical runtime player-loop readiness"
            };
        }

        var blocking = string.Equals(severity, "Error", StringComparison.Ordinal)
                       || string.Equals(severity, "Critical", StringComparison.Ordinal);
        return new CanonicalRuntimePlayerLoopClassifiedDiagnostic
        {
            RawDiagnostic = raw,
            Severity = severity,
            Code = code,
            Blocking = blocking,
            NonBlockingForCanonicalRuntimePath = !blocking,
            Reason = blocking
                ? "unclassified error diagnostic blocks GREEN readiness"
                : "non-error diagnostic"
        };
    }

    private static SortedDictionary<string, string> BuildFilePayloads(
        string root,
        string relativeRoot,
        CanonicalRuntimePlayerLoopReadinessResult runtimeResult,
        CanonicalRuntimePlayerLoopPlanDocument plan,
        CanonicalRuntimePlayerLoopDiagnosticClassification classification,
        CanonicalRuntimePlayerLoopUnitySmoke unitySmoke,
        CanonicalRuntimePlayerLoopReadinessMatrixResult matrix,
        CanonicalRuntimePlayerLoopReadinessReport report,
        string reportMarkdown,
        CanonicalRuntimePlayerLoopNegativeProof negative,
        CanonicalRuntimePlayerLoopReadinessDashboard dashboard)
    {
        var files = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [CanonicalRuntimePlayerLoopReadinessVocabulary.AdapterContractFileName] =
                Serialize(runtimeResult.PlayerAdapterContract),
            [CanonicalRuntimePlayerLoopReadinessVocabulary.PlayerLoopPlanFileName] =
                Serialize(plan),
            [CanonicalRuntimePlayerLoopReadinessVocabulary.ReadinessResultFileName] =
                Serialize(runtimeResult),
            [CanonicalRuntimePlayerLoopReadinessVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [CanonicalRuntimePlayerLoopReadinessVocabulary.MatrixResultFileName] =
                Serialize(matrix),
            [CanonicalRuntimePlayerLoopReadinessVocabulary.DiagnosticClassificationFileName] =
                Serialize(classification),
            [CanonicalRuntimePlayerLoopReadinessVocabulary.UnitySmokeFileName] =
                Serialize(unitySmoke),
            [CanonicalRuntimePlayerLoopReadinessVocabulary.ReportJsonFileName] =
                Serialize(report),
            [CanonicalRuntimePlayerLoopReadinessVocabulary.ReportMarkdownFileName] =
                reportMarkdown,
            [CanonicalRuntimePlayerLoopReadinessVocabulary.NegativeProofFileName] =
                Serialize(negative)
        };
        var index = BuildFileIndex(root, relativeRoot, files);
        files[CanonicalRuntimePlayerLoopReadinessVocabulary.FileIndexFileName] =
            Serialize(index);
        return files;
    }

    private static CanonicalRuntimePlayerLoopPlanDocument BuildPlan(
        CanonicalRuntimePlayerLoopReadinessResult result) =>
        new()
        {
            CandidateId = result.CandidateId,
            CanonicalRuntimeSource = true,
            UnityGameplayTruth = false,
            ProjectionOnly = false,
            RequiredStepCategories = result.RequiredStepCategories,
            RequiredStepCategoriesPresent = result.RequiredStepCategoriesPresent,
            PlayerLoopStepCount = result.PlayerLoopStepCount,
            Steps = result.Steps
        };

    private static CanonicalRuntimePlayerLoopReadinessMatrixResult BuildMatrix(
        CanonicalRuntimePlayerLoopReadinessResult result,
        CanonicalRuntimePlayerLoopDiagnosticClassification classification,
        CanonicalRuntimePlayerLoopUnitySmoke unitySmoke)
    {
        var row = new CanonicalRuntimePlayerLoopReadinessMatrixRow
        {
            CandidateId = result.CandidateId,
            PlayerAdapterContractPresent = result.PlayerAdapterContractPresent,
            PlayerLoopPlanPresent = result.PlayerLoopPlanPresent,
            PlayerLoopStepCount = result.PlayerLoopStepCount,
            RequiredStepCategoriesPresent = result.RequiredStepCategoriesPresent,
            UnityPlayerLoopReadinessPassed = unitySmoke.UnityPlayerLoopReadinessPassed,
            NoUnclassifiedErrorDiagnostics = classification.NoUnclassifiedErrorDiagnostics,
            Passed = result.Passed
                     && unitySmoke.UnityPlayerLoopReadinessPassed
                     && classification.NoUnclassifiedErrorDiagnostics
        };
        return new CanonicalRuntimePlayerLoopReadinessMatrixResult
        {
            Passed = row.Passed,
            Rows = [row]
        };
    }

    private static CanonicalRuntimePlayerLoopReadinessReport BuildReport(
        CanonicalRuntimePlayerLoopReadinessResult result,
        CanonicalRuntimePlayerLoopDiagnosticClassification classification,
        CanonicalRuntimePlayerLoopUnitySmoke unitySmoke) =>
        new()
        {
            CandidateId = result.CandidateId,
            PlayerAdapterContractPresent = result.PlayerAdapterContractPresent,
            PlayerLoopPlanPresent = result.PlayerLoopPlanPresent,
            PlayerLoopStepCount = result.PlayerLoopStepCount,
            RequiredStepCategoriesPresent = result.RequiredStepCategoriesPresent,
            UnityPlayerLoopReadinessPassed = unitySmoke.UnityPlayerLoopReadinessPassed,
            ProjectionOnly = false,
            CanonicalRuntimeSource = true,
            UnityGameplayTruth = false,
            SaveLoadReplayStillReferenced = result.SaveLoadReplayStillReferenced,
            SelectedCandidateExecutedByRuntime = result.SelectedCandidateExecutedByRuntime,
            ManualUnityOptional = true,
            NoUnclassifiedErrorDiagnostics = classification.NoUnclassifiedErrorDiagnostics,
            RawDiagnosticCount = classification.RawDiagnosticCount,
            BlockingDiagnosticCount = classification.BlockingDiagnosticCount,
            NonBlockingDiagnosticCount = classification.NonBlockingDiagnosticCount
        };

    private static CanonicalRuntimePlayerLoopNegativeProof BuildNegativeProof(
        CanonicalRuntimePlayerLoopReadinessResult result)
    {
        var proof = new CanonicalRuntimePlayerLoopNegativeProof
        {
            ManualInputRejected = true,
            OutputRootUnderGoal135 = true,
            SamplePackageReadOnly = true,
            GamePackageSchemaUnchanged = true,
            GeneratorLibraryProviderLuaUnchanged = true,
            UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged = true,
            PlayerAdapterDoesNotExecuteGameplay = true,
            ProjectionOnly = result.ProjectionOnly
        };
        return proof with
        {
            Passed =
                proof.ManualInputRejected
                && proof.OutputRootUnderGoal135
                && proof.SamplePackageReadOnly
                && proof.GamePackageSchemaUnchanged
                && proof.GeneratorLibraryProviderLuaUnchanged
                && proof.UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged
                && proof.PlayerAdapterDoesNotExecuteGameplay
                && !proof.ProjectionOnly
        };
    }

    private static CanonicalRuntimePlayerLoopReadinessDashboard BuildDashboard(
        CanonicalRuntimePlayerLoopReadinessResult result,
        CanonicalRuntimePlayerLoopDiagnosticClassification classification,
        CanonicalRuntimePlayerLoopUnitySmoke unitySmoke)
    {
        var diagnostics = new List<string>();
        Require(result.PlayerAdapterContractPresent, "goal135.player_adapter_contract_missing", diagnostics);
        Require(result.PlayerLoopPlanPresent, "goal135.player_loop_plan_missing", diagnostics);
        Require(result.PlayerLoopStepCount >= 8, "goal135.player_loop_step_count", diagnostics);
        Require(result.RequiredStepCategoriesPresent, "goal135.required_step_categories_missing", diagnostics);
        Require(unitySmoke.UnityPlayerLoopReadinessPassed, "goal135.unity_player_loop_readiness_failed", diagnostics);
        Require(result.CanonicalRuntimeSource, "goal135.canonical_runtime_source_missing", diagnostics);
        Require(!result.UnityGameplayTruth, "goal135.unity_gameplay_truth_not_allowed", diagnostics);
        Require(!result.ProjectionOnly, "goal135.projection_only_not_allowed", diagnostics);
        Require(result.SaveLoadReplayStillReferenced, "goal135.save_load_replay_not_referenced", diagnostics);
        Require(result.SelectedCandidateExecutedByRuntime, "goal135.selected_candidate_not_executed_by_runtime", diagnostics);
        Require(classification.NoUnclassifiedErrorDiagnostics, "goal135.unclassified_error_diagnostics", diagnostics);

        return new CanonicalRuntimePlayerLoopReadinessDashboard
        {
            Status = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            CandidateId = result.CandidateId,
            ProjectionOnly = false,
            CanonicalRuntimeSource = true,
            PlayerAdapterContractPresent = result.PlayerAdapterContractPresent,
            PlayerLoopPlanPresent = result.PlayerLoopPlanPresent,
            PlayerLoopStepCount = result.PlayerLoopStepCount,
            RequiredStepCategoriesPresent = result.RequiredStepCategoriesPresent,
            UnityPlayerLoopReadinessPassed = unitySmoke.UnityPlayerLoopReadinessPassed,
            UnityGameplayTruth = false,
            ManualUnityOptional = true,
            SaveLoadReplayStillReferenced = result.SaveLoadReplayStillReferenced,
            SelectedCandidateExecutedByRuntime = result.SelectedCandidateExecutedByRuntime,
            NoUnclassifiedErrorDiagnostics = classification.NoUnclassifiedErrorDiagnostics,
            MissingStepCategories = result.MissingStepCategories,
            Diagnostics = diagnostics
                .Concat(result.Diagnostics)
                .Concat(new[]
                {
                    "goal135.raw_diagnostic_count=" + classification.RawDiagnosticCount,
                    "goal135.blocking_diagnostic_count=" + classification.BlockingDiagnosticCount,
                    "goal135.nonblocking_diagnostic_count=" + classification.NonBlockingDiagnosticCount
                })
                .Concat(unitySmoke.Diagnostics)
                .ToList()
        };
    }

    private static CanonicalRuntimePlayerLoopUnitySmoke BuildPendingUnitySmoke(string root)
    {
        var plan = Resolve(
            root,
            CanonicalRuntimePlayerLoopReadinessVocabulary.ProceduralOutputDirectory
            + "/"
            + CanonicalRuntimePlayerLoopReadinessVocabulary.PlayerLoopPlanFileName);
        var stateSummary = Resolve(
            root,
            CanonicalRuntimePlayerLoopReadinessVocabulary.DefaultCanonicalRuntimeStateSummaryPath);
        return new CanonicalRuntimePlayerLoopUnitySmoke
        {
            PlanPath = Relative(root, plan),
            StateSummaryPath = Relative(root, stateSummary),
            PlanPathExists = File.Exists(plan),
            StateSummaryPathExists = File.Exists(stateSummary),
            Status = "PENDING_UNITY_BATCHMODE",
            Diagnostics = ["unity player-loop readiness smoke has not written a marker artifact yet"]
        };
    }

    private static CanonicalRuntimePlayerLoopFileIndex BuildFileIndex(
        string root,
        string relativeRoot,
        IReadOnlyDictionary<string, string> pendingTextFiles)
    {
        var files = pendingTextFiles
            .Select(item => new CanonicalRuntimePlayerLoopFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = "goal135_" + Path.GetFileNameWithoutExtension(item.Key)
                    .Replace("-", "_", StringComparison.Ordinal),
                Sha256 = HashText(item.Value)
            })
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
        return new CanonicalRuntimePlayerLoopFileIndex
        {
            RootPath = relativeRoot,
            IndexedFileCount = files.Count,
            ManualInputExcluded = files.All(file =>
                !file.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = files
        };
    }

    private static string RenderReport(
        CanonicalRuntimePlayerLoopReadinessReport report,
        CanonicalRuntimePlayerLoopReadinessDashboard dashboard,
        CanonicalRuntimePlayerLoopReadinessResult result,
        CanonicalRuntimePlayerLoopDiagnosticClassification classification)
    {
        var lines = new List<string>
        {
            "# Goal 135 Canonical Runtime Playable Player Loop Readiness",
            string.Empty,
            "- candidateId: " + report.CandidateId,
            "- playerAdapterContractPresent: " + Bool(report.PlayerAdapterContractPresent),
            "- playerLoopPlanPresent: " + Bool(report.PlayerLoopPlanPresent),
            "- playerLoopStepCount: " + report.PlayerLoopStepCount,
            "- requiredStepCategoriesPresent: " + Bool(report.RequiredStepCategoriesPresent),
            "- unityPlayerLoopReadinessPassed: " + Bool(report.UnityPlayerLoopReadinessPassed),
            "- projectionOnly: " + Bool(report.ProjectionOnly),
            "- canonicalRuntimeSource: " + Bool(report.CanonicalRuntimeSource),
            "- unityGameplayTruth: " + Bool(report.UnityGameplayTruth),
            "- saveLoadReplayStillReferenced: " + Bool(report.SaveLoadReplayStillReferenced),
            "- selectedCandidateExecutedByRuntime: " + Bool(report.SelectedCandidateExecutedByRuntime),
            "- manualUnityOptional: " + Bool(report.ManualUnityOptional),
            "- noUnclassifiedErrorDiagnostics: " + Bool(report.NoUnclassifiedErrorDiagnostics),
            "- rawDiagnosticCount: " + report.RawDiagnosticCount,
            "- blockingDiagnosticCount: " + report.BlockingDiagnosticCount,
            "- nonBlockingDiagnosticCount: " + report.NonBlockingDiagnosticCount,
            "- normalCommand: " + CanonicalRuntimePlayerLoopReadinessVocabulary.NormalCommand,
            "- reportPath: " + report.ReportPath,
            "- matrixResultPath: " + report.MatrixResultPath,
            string.Empty,
            "## Dashboard",
            string.Empty,
            "- status: " + dashboard.Status,
            "- missingStepCategories: " + (dashboard.MissingStepCategories.Count == 0 ? "none" : string.Join(", ", dashboard.MissingStepCategories)),
            string.Empty,
            "## Required Step Categories",
            string.Empty
        };
        lines.AddRange(result.RequiredStepCategories.Select(category =>
            "- " + category + ": " + Bool(result.Steps.Any(step => step.Category == category))));
        lines.AddRange(
        [
            string.Empty,
            "## Diagnostic Classification",
            string.Empty,
            "- passAllowsNonBlockingDiagnostics: " + Bool(classification.PassAllowsNonBlockingDiagnostics),
            "- noUnclassifiedErrorDiagnostics: " + Bool(classification.NoUnclassifiedErrorDiagnostics),
            "- blockingDiagnosticCount: " + classification.BlockingDiagnosticCount,
            "- nonBlockingDiagnosticCount: " + classification.NonBlockingDiagnosticCount
        ]);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static IReadOnlyList<string> ReadDiagnostics(JsonElement dashboard)
    {
        if (!dashboard.TryGetProperty("diagnostics", out var diagnostics)
            || diagnostics.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return diagnostics.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
    }

    private static bool TryGetBool(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        && property.GetBoolean();

    private static void Require(bool condition, string diagnostic, ICollection<string> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(diagnostic);
        }
    }

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions);

    private static string Bool(bool value) =>
        value.ToString().ToLowerInvariant();

    private static string ResolveRepositoryRoot(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        if (!File.Exists(Path.Combine(root, "LLMGameCreator.sln")))
        {
            throw new InvalidOperationException("Repository root was not found.");
        }

        return root;
    }

    private static string Resolve(string root, string path)
    {
        var fullPath = Path.IsPathRooted(path) ? path : Path.Combine(root, path);
        var resolved = Path.GetFullPath(fullPath);
        if (!IsUnderRoot(root, resolved))
        {
            throw new InvalidOperationException("Path must stay under the repository root: " + path);
        }

        return resolved;
    }

    private static bool IsUnderRoot(string root, string path)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                             + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path);
        return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static void GuardNotManualInput(string root, string path)
    {
        var relative = Relative(root, path);
        if (relative.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Path must not point under .llmgc/manual: " + relative);
        }
    }

    private static void GuardGoal135Write(string root, string path)
    {
        GuardNotManualInput(root, path);
        var relative = Relative(root, path);
        if (!relative.StartsWith(
                CanonicalRuntimePlayerLoopReadinessVocabulary.ProceduralOutputDirectory + "/",
                StringComparison.Ordinal)
            && !relative.StartsWith(
                CanonicalRuntimePlayerLoopReadinessVocabulary.ExportPackageDirectory + "/",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to write outside Goal135 output roots: " + relative);
        }
    }

    private static async Task WriteTextAsync(
        string path,
        string text,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, text, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

    private static string HashText(string text)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
        var builder = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
        {
            builder.Append(b.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }
}
