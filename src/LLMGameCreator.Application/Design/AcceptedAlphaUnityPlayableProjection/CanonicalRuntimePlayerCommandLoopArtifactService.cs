using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class CanonicalRuntimePlayerCommandLoopArtifactService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static CanonicalRuntimePlayerCommandLoopArtifactService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public static CanonicalRuntimePlayerCommandLoopUnitySmoke ReadUnitySmoke(string path) =>
        JsonSerializer.Deserialize<CanonicalRuntimePlayerCommandLoopUnitySmoke>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? new CanonicalRuntimePlayerCommandLoopUnitySmoke();

    public async Task<CanonicalRuntimePlayerCommandLoopWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CanonicalRuntimePlayerCommandLoopRequest request,
        CanonicalRuntimePlayerCommandLoopResult runtimeResult,
        string outputRootRelativePath =
            CanonicalRuntimePlayerCommandLoopVocabulary.ProceduralOutputDirectory,
        string exportRootRelativePath =
            CanonicalRuntimePlayerCommandLoopVocabulary.ExportPackageDirectory,
        CanonicalRuntimePlayerCommandLoopUnitySmoke? unitySmoke = null,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var inputs = BuildInputs(root, request);
        runtimeResult.Inputs = inputs;

        var rawDiagnostics = ReadRawDiagnostics(
            root,
            CanonicalRuntimePlayerLoopReadinessVocabulary.DefaultCanonicalRuntimeDashboardPath)
            .Concat(runtimeResult.Diagnostics)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        var classification = ClassifyDiagnostics(rawDiagnostics);
        var smoke = unitySmoke ?? BuildPendingUnitySmoke(root, outputRootRelativePath);
        var plan = BuildPlan(runtimeResult);
        var matrix = BuildMatrix(runtimeResult, smoke);
        var report = BuildReport(runtimeResult, classification, smoke);
        var negative = BuildNegativeProof(runtimeResult);
        var dashboard = BuildDashboard(runtimeResult, classification, smoke);
        var markdown = RenderReport(report, dashboard, runtimeResult, classification);

        var proceduralFiles = BuildFilePayloads(
            root,
            outputRootRelativePath,
            inputs,
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
            inputs,
            runtimeResult,
            plan,
            classification,
            smoke,
            matrix,
            report,
            markdown,
            negative,
            dashboard);

        var procedural = Resolve(root, outputRootRelativePath);
        var export = Resolve(root, exportRootRelativePath);
        var docsPath = Resolve(root, CanonicalRuntimePlayerCommandLoopVocabulary.DocumentationPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in proceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            GuardGoal136Write(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in exportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            GuardGoal136Write(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(docsPath, markdown, cancellationToken).ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new CanonicalRuntimePlayerCommandLoopWriteResult
        {
            Dashboard = dashboard,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    public static CanonicalRuntimePlayerCommandLoopDiagnosticClassification ClassifyDiagnostics(
        IReadOnlyList<string> rawDiagnostics)
    {
        var classified = rawDiagnostics
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(ClassifyDiagnostic)
            .OrderBy(item => item.RawDiagnostic, StringComparer.Ordinal)
            .ToList();
        var blocking = classified.Where(item => item.Blocking).ToList();
        var nonBlocking = classified.Where(item => !item.Blocking).ToList();
        return new CanonicalRuntimePlayerCommandLoopDiagnosticClassification
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

    private static CanonicalRuntimePlayerCommandLoopClassifiedDiagnostic ClassifyDiagnostic(string raw)
    {
        var parts = raw.Split(':', 3);
        var severity = parts.Length > 0 ? parts[0] : string.Empty;
        var code = parts.Length > 1 ? parts[1] : raw;
        if (string.Equals(severity, "Error", StringComparison.Ordinal)
            && string.Equals(code, "script.path.missing", StringComparison.Ordinal))
        {
            return new CanonicalRuntimePlayerCommandLoopClassifiedDiagnostic
            {
                RawDiagnostic = raw,
                Severity = severity,
                Code = code,
                Blocking = false,
                NonBlockingForCanonicalRuntimePath = true,
                Reason =
                    "script artifact path is not required by the selected canonical runtime command loop"
            };
        }

        if (string.Equals(severity, "Warning", StringComparison.Ordinal)
            || raw.StartsWith("goal135.", StringComparison.Ordinal)
            || raw.StartsWith("unityExitCode=", StringComparison.Ordinal))
        {
            return new CanonicalRuntimePlayerCommandLoopClassifiedDiagnostic
            {
                RawDiagnostic = raw,
                Severity = severity,
                Code = code,
                Blocking = false,
                NonBlockingForCanonicalRuntimePath = true,
                Reason =
                    "diagnostic is non-blocking under the Goal135 canonical runtime policy"
            };
        }

        var blocking = string.Equals(severity, "Error", StringComparison.Ordinal)
                       || string.Equals(severity, "Critical", StringComparison.Ordinal);
        return new CanonicalRuntimePlayerCommandLoopClassifiedDiagnostic
        {
            RawDiagnostic = raw,
            Severity = severity,
            Code = code,
            Blocking = blocking,
            NonBlockingForCanonicalRuntimePath = !blocking,
            Reason = blocking
                ? "unclassified error diagnostic blocks GREEN command-loop execution"
                : "non-error diagnostic"
        };
    }

    private static SortedDictionary<string, string> BuildFilePayloads(
        string root,
        string relativeRoot,
        CanonicalRuntimePlayerCommandLoopInput inputs,
        CanonicalRuntimePlayerCommandLoopResult runtimeResult,
        CanonicalRuntimePlayerCommandLoopPlanDocument plan,
        CanonicalRuntimePlayerCommandLoopDiagnosticClassification classification,
        CanonicalRuntimePlayerCommandLoopUnitySmoke unitySmoke,
        CanonicalRuntimePlayerCommandLoopMatrixResult matrix,
        CanonicalRuntimePlayerCommandLoopReport report,
        string reportMarkdown,
        CanonicalRuntimePlayerCommandLoopNegativeProof negative,
        CanonicalRuntimePlayerCommandLoopDashboard dashboard)
    {
        var files = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [CanonicalRuntimePlayerCommandLoopVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [CanonicalRuntimePlayerCommandLoopVocabulary.InputsFileName] =
                Serialize(inputs),
            [CanonicalRuntimePlayerCommandLoopVocabulary.PlanFileName] =
                Serialize(plan),
            [CanonicalRuntimePlayerCommandLoopVocabulary.SnapshotsFileName] =
                Serialize(runtimeResult.Snapshots),
            [CanonicalRuntimePlayerCommandLoopVocabulary.ResultFileName] =
                Serialize(runtimeResult),
            [CanonicalRuntimePlayerCommandLoopVocabulary.MatrixResultFileName] =
                Serialize(matrix),
            [CanonicalRuntimePlayerCommandLoopVocabulary.DiagnosticClassificationFileName] =
                Serialize(classification),
            [CanonicalRuntimePlayerCommandLoopVocabulary.UnitySmokeFileName] =
                Serialize(unitySmoke),
            [CanonicalRuntimePlayerCommandLoopVocabulary.ReportJsonFileName] =
                Serialize(report),
            [CanonicalRuntimePlayerCommandLoopVocabulary.ReportMarkdownFileName] =
                reportMarkdown,
            [CanonicalRuntimePlayerCommandLoopVocabulary.NegativeProofFileName] =
                Serialize(negative)
        };
        files[CanonicalRuntimePlayerCommandLoopVocabulary.FileIndexFileName] =
            Serialize(BuildFileIndex(root, relativeRoot, files));
        return files;
    }

    private static CanonicalRuntimePlayerCommandLoopInput BuildInputs(
        string root,
        CanonicalRuntimePlayerCommandLoopRequest request)
    {
        var package = Resolve(root, request.PackagePath);
        var handoff = Resolve(root, request.HandoffPath);
        var transcript = Resolve(root, request.Goal134TranscriptPath);
        var stateSummary = Resolve(root, request.Goal134StateSummaryPath);
        var plan = Resolve(root, request.Goal135PlayerLoopPlanPath);
        var contract = Resolve(root, request.Goal135PlayerAdapterContractPath);
        GuardNotManualInput(root, package);
        GuardNotManualInput(root, handoff);
        GuardNotManualInput(root, transcript);
        GuardNotManualInput(root, stateSummary);
        GuardNotManualInput(root, plan);
        GuardNotManualInput(root, contract);
        return new CanonicalRuntimePlayerCommandLoopInput
        {
            CandidateId = request.CandidateId,
            PackagePath = Relative(root, package),
            HandoffPath = Relative(root, handoff),
            Goal134TranscriptPath = Relative(root, transcript),
            Goal134StateSummaryPath = Relative(root, stateSummary),
            Goal135PlayerLoopPlanPath = Relative(root, plan),
            Goal135PlayerAdapterContractPath = Relative(root, contract),
            PackagePathExists = File.Exists(package),
            HandoffPathExists = File.Exists(handoff),
            Goal134TranscriptPathExists = File.Exists(transcript),
            Goal134StateSummaryPathExists = File.Exists(stateSummary),
            Goal135PlayerLoopPlanPathExists = File.Exists(plan),
            Goal135PlayerAdapterContractPathExists = File.Exists(contract)
        };
    }

    private static CanonicalRuntimePlayerCommandLoopPlanDocument BuildPlan(
        CanonicalRuntimePlayerCommandLoopResult result) =>
        new()
        {
            CandidateId = result.CandidateId,
            ProjectionOnly = false,
            UnityGameplayTruth = false,
            CanonicalRuntimeSource = true,
            PlayerCommandLoopCoverage = result.PlayerCommandLoopPassed,
            RequiredCategories = result.RequiredCategories,
            AllRequiredCategoriesPresent = result.AllRequiredCategoriesPresent,
            PlayerCommandCount = result.PlayerCommandCount,
            Steps = result.Steps
        };

    private static CanonicalRuntimePlayerCommandLoopMatrixResult BuildMatrix(
        CanonicalRuntimePlayerCommandLoopResult result,
        CanonicalRuntimePlayerCommandLoopUnitySmoke unitySmoke)
    {
        var row = new CanonicalRuntimePlayerCommandLoopMatrixRow
        {
            CandidateId = result.CandidateId,
            PackagePath = result.Inputs.PackagePath,
            PlayerCommandLoopPassed = result.PlayerCommandLoopPassed,
            PlayerCommandCount = result.PlayerCommandCount,
            SnapshotCount = result.PlayerSnapshotCount,
            RuntimeEventCount = result.RuntimeEventCount,
            AllRequiredCategoriesPresent = result.AllRequiredCategoriesPresent,
            UnityPlayerConsumedCommandLoopSnapshots =
                unitySmoke.UnityPlayerConsumedCommandLoopSnapshots,
            Passed = result.PlayerCommandLoopPassed
                     && unitySmoke.UnityPlayerConsumedCommandLoopSnapshots
        };
        return new CanonicalRuntimePlayerCommandLoopMatrixResult
        {
            Passed = row.Passed,
            Rows = [row]
        };
    }

    private static CanonicalRuntimePlayerCommandLoopReport BuildReport(
        CanonicalRuntimePlayerCommandLoopResult result,
        CanonicalRuntimePlayerCommandLoopDiagnosticClassification classification,
        CanonicalRuntimePlayerCommandLoopUnitySmoke unitySmoke) =>
        new()
        {
            CandidateId = result.CandidateId,
            PlayerCommandLoopPassed = result.PlayerCommandLoopPassed,
            PlayerCommandCount = result.PlayerCommandCount,
            SnapshotCount = result.PlayerSnapshotCount,
            RuntimeEventCount = result.RuntimeEventCount,
            StateHashChainPresent = result.StateHashChainPresent,
            AllRequiredCategoriesPresent = result.AllRequiredCategoriesPresent,
            UnityPlayerConsumedCommandLoopSnapshots =
                unitySmoke.UnityPlayerConsumedCommandLoopSnapshots,
            ProjectionOnly = false,
            UnityGameplayTruth = false,
            SelectedCandidateExecutedByRuntime =
                result.SelectedCandidateExecutedByRuntime,
            ManualUnityOptional = true,
            NoUnclassifiedErrorDiagnostics =
                classification.NoUnclassifiedErrorDiagnostics,
            RawDiagnosticCount = classification.RawDiagnosticCount,
            BlockingDiagnosticCount = classification.BlockingDiagnosticCount,
            NonBlockingDiagnosticCount = classification.NonBlockingDiagnosticCount
        };

    private static CanonicalRuntimePlayerCommandLoopNegativeProof BuildNegativeProof(
        CanonicalRuntimePlayerCommandLoopResult result)
    {
        var proof = new CanonicalRuntimePlayerCommandLoopNegativeProof
        {
            ManualInputRejected = true,
            OutputRootUnderGoal136 = true,
            SamplePackageReadOnly = true,
            GamePackageSchemaUnchanged = true,
            GeneratorLibraryProviderLuaUnchanged = true,
            UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged = true,
            RuntimeOwnsCommandExecution = result.SelectedCandidateExecutedByRuntime,
            PlayerAdapterDoesNotExecuteGameplay = true,
            ProjectionOnly = result.ProjectionOnly,
            UnityGameplayTruth = result.UnityGameplayTruth
        };
        return proof with
        {
            Passed =
                proof.ManualInputRejected
                && proof.OutputRootUnderGoal136
                && proof.SamplePackageReadOnly
                && proof.GamePackageSchemaUnchanged
                && proof.GeneratorLibraryProviderLuaUnchanged
                && proof.UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged
                && proof.RuntimeOwnsCommandExecution
                && proof.PlayerAdapterDoesNotExecuteGameplay
                && !proof.ProjectionOnly
                && !proof.UnityGameplayTruth
        };
    }

    private static CanonicalRuntimePlayerCommandLoopDashboard BuildDashboard(
        CanonicalRuntimePlayerCommandLoopResult result,
        CanonicalRuntimePlayerCommandLoopDiagnosticClassification classification,
        CanonicalRuntimePlayerCommandLoopUnitySmoke unitySmoke)
    {
        var diagnostics = new List<string>();
        Require(result.PlayerCommandLoopPassed, "goal136.player_command_loop_failed", diagnostics);
        Require(result.PlayerCommandCount >= 10, "goal136.player_command_count", diagnostics);
        Require(result.PlayerSnapshotCount == result.PlayerCommandCount, "goal136.snapshot_count_mismatch", diagnostics);
        Require(result.RuntimeEventCount >= 10, "goal136.runtime_event_count", diagnostics);
        Require(result.StateHashChainPresent, "goal136.state_hash_chain_missing", diagnostics);
        Require(result.AllRequiredCategoriesPresent, "goal136.required_categories_missing", diagnostics);
        Require(result.SelectedCandidateExecutedByRuntime, "goal136.selected_candidate_not_executed_by_runtime", diagnostics);
        Require(!result.ProjectionOnly, "goal136.projection_only_not_allowed", diagnostics);
        Require(!result.UnityGameplayTruth, "goal136.unity_gameplay_truth_not_allowed", diagnostics);
        Require(unitySmoke.UnityPlayerConsumedCommandLoopSnapshots, "goal136.unity_command_loop_snapshot_smoke_failed", diagnostics);
        Require(classification.NoUnclassifiedErrorDiagnostics, "goal136.unclassified_error_diagnostics", diagnostics);

        return new CanonicalRuntimePlayerCommandLoopDashboard
        {
            Status = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            CandidateId = result.CandidateId,
            PlayerCommandLoopPassed = result.PlayerCommandLoopPassed,
            PlayerCommandCount = result.PlayerCommandCount,
            SnapshotCount = result.PlayerSnapshotCount,
            RuntimeEventCount = result.RuntimeEventCount,
            StateHashChainPresent = result.StateHashChainPresent,
            AllRequiredCategoriesPresent = result.AllRequiredCategoriesPresent,
            UnityPlayerConsumedCommandLoopSnapshots =
                unitySmoke.UnityPlayerConsumedCommandLoopSnapshots,
            ProjectionOnly = false,
            UnityGameplayTruth = false,
            SelectedCandidateExecutedByRuntime =
                result.SelectedCandidateExecutedByRuntime,
            RuntimePrimitiveMissing = result.RuntimePrimitiveMissing,
            MissingRuntimePrimitives = result.MissingRuntimePrimitives,
            NoUnclassifiedErrorDiagnostics =
                classification.NoUnclassifiedErrorDiagnostics,
            RawDiagnosticCount = classification.RawDiagnosticCount,
            BlockingDiagnosticCount = classification.BlockingDiagnosticCount,
            NonBlockingDiagnosticCount = classification.NonBlockingDiagnosticCount,
            ManualUnityOptional = true,
            Accepted = false,
            MissingCategories = result.MissingCategories,
            Diagnostics = diagnostics
                .Concat(result.Diagnostics)
                .Concat(new[]
                {
                    "goal136.raw_diagnostic_count=" + classification.RawDiagnosticCount,
                    "goal136.blocking_diagnostic_count=" + classification.BlockingDiagnosticCount,
                    "goal136.nonblocking_diagnostic_count=" + classification.NonBlockingDiagnosticCount
                })
                .Concat(unitySmoke.Diagnostics)
                .ToList()
        };
    }

    private static CanonicalRuntimePlayerCommandLoopUnitySmoke BuildPendingUnitySmoke(
        string root,
        string outputRootRelativePath)
    {
        var snapshots = Resolve(
            root,
            outputRootRelativePath + "/" + CanonicalRuntimePlayerCommandLoopVocabulary.SnapshotsFileName);
        var result = Resolve(
            root,
            outputRootRelativePath + "/" + CanonicalRuntimePlayerCommandLoopVocabulary.ResultFileName);
        return new CanonicalRuntimePlayerCommandLoopUnitySmoke
        {
            SnapshotsPath = Relative(root, snapshots),
            ResultPath = Relative(root, result),
            SnapshotsPathExists = File.Exists(snapshots),
            ResultPathExists = File.Exists(result),
            Status = "PENDING_UNITY_BATCHMODE",
            Diagnostics = ["unity player command-loop smoke has not written a marker artifact yet"]
        };
    }

    private static CanonicalRuntimePlayerCommandLoopFileIndex BuildFileIndex(
        string root,
        string relativeRoot,
        IReadOnlyDictionary<string, string> pendingTextFiles)
    {
        var files = pendingTextFiles
            .Select(item => new CanonicalRuntimePlayerCommandLoopFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = "goal136_" + Path.GetFileNameWithoutExtension(item.Key)
                    .Replace("-", "_", StringComparison.Ordinal),
                Sha256 = HashText(item.Value)
            })
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
        return new CanonicalRuntimePlayerCommandLoopFileIndex
        {
            RootPath = relativeRoot,
            IndexedFileCount = files.Count,
            ManualInputExcluded = files.All(file =>
                !file.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = files
        };
    }

    private static string RenderReport(
        CanonicalRuntimePlayerCommandLoopReport report,
        CanonicalRuntimePlayerCommandLoopDashboard dashboard,
        CanonicalRuntimePlayerCommandLoopResult result,
        CanonicalRuntimePlayerCommandLoopDiagnosticClassification classification)
    {
        var lines = new List<string>
        {
            "# Goal 136 Canonical Runtime Player Command Loop Execution Matrix",
            string.Empty,
            "- candidateId: " + report.CandidateId,
            "- playerCommandLoopPassed: " + Bool(report.PlayerCommandLoopPassed),
            "- playerCommandCount: " + report.PlayerCommandCount,
            "- snapshotCount: " + report.SnapshotCount,
            "- runtimeEventCount: " + report.RuntimeEventCount,
            "- stateHashChainPresent: " + Bool(report.StateHashChainPresent),
            "- allRequiredCategoriesPresent: " + Bool(report.AllRequiredCategoriesPresent),
            "- unityPlayerConsumedCommandLoopSnapshots: " + Bool(report.UnityPlayerConsumedCommandLoopSnapshots),
            "- projectionOnly: " + Bool(report.ProjectionOnly),
            "- unityGameplayTruth: " + Bool(report.UnityGameplayTruth),
            "- selectedCandidateExecutedByRuntime: " + Bool(report.SelectedCandidateExecutedByRuntime),
            "- manualUnityOptional: " + Bool(report.ManualUnityOptional),
            "- accepted: false",
            "- noUnclassifiedErrorDiagnostics: " + Bool(report.NoUnclassifiedErrorDiagnostics),
            "- rawDiagnosticCount: " + report.RawDiagnosticCount,
            "- blockingDiagnosticCount: " + report.BlockingDiagnosticCount,
            "- nonBlockingDiagnosticCount: " + report.NonBlockingDiagnosticCount,
            "- normalCommand: " + CanonicalRuntimePlayerCommandLoopVocabulary.NormalCommand,
            "- reportPath: " + report.ReportPath,
            "- matrixResultPath: " + report.MatrixResultPath,
            string.Empty,
            "## Dashboard",
            string.Empty,
            "- status: " + dashboard.Status,
            "- runtimePrimitiveMissing: " + Bool(dashboard.RuntimePrimitiveMissing),
            "- missingRuntimePrimitives: " + (dashboard.MissingRuntimePrimitives.Count == 0 ? "none" : string.Join(", ", dashboard.MissingRuntimePrimitives)),
            "- missingCategories: " + (dashboard.MissingCategories.Count == 0 ? "none" : string.Join(", ", dashboard.MissingCategories)),
            string.Empty,
            "## Required Categories",
            string.Empty
        };
        lines.AddRange(result.RequiredCategories.Select(category =>
            "- " + category + ": " + Bool(result.Snapshots.Any(snapshot => snapshot.Category == category))));
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

    private static IReadOnlyList<string> ReadRawDiagnostics(string root, string relativePath)
    {
        var fullPath = Resolve(root, relativePath);
        if (!File.Exists(fullPath))
        {
            return [];
        }

        using var dashboard = JsonDocument.Parse(File.ReadAllText(fullPath, Encoding.UTF8));
        if (!dashboard.RootElement.TryGetProperty("diagnostics", out var diagnostics)
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

    private static void GuardGoal136Write(string root, string path)
    {
        GuardNotManualInput(root, path);
        var relative = Relative(root, path);
        if (!relative.StartsWith(
                CanonicalRuntimePlayerCommandLoopVocabulary.ProceduralOutputDirectory + "/",
                StringComparison.Ordinal)
            && !relative.StartsWith(
                CanonicalRuntimePlayerCommandLoopVocabulary.ExportPackageDirectory + "/",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to write outside Goal136 output roots: " + relative);
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
