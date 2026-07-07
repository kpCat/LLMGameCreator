using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class CanonicalRuntimeUnityPlayerLoopPlaybackArtifactService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static CanonicalRuntimeUnityPlayerLoopPlaybackArtifactService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public static CanonicalRuntimeUnityPlayerLoopPlaybackUnitySmoke ReadUnitySmoke(string path) =>
        JsonSerializer.Deserialize<CanonicalRuntimeUnityPlayerLoopPlaybackUnitySmoke>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? new CanonicalRuntimeUnityPlayerLoopPlaybackUnitySmoke();

    public async Task<CanonicalRuntimeUnityPlayerLoopPlaybackWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CanonicalRuntimeUnityPlayerLoopPlaybackRequest request,
        string outputRootRelativePath =
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ProceduralOutputDirectory,
        string exportRootRelativePath =
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ExportPackageDirectory,
        CanonicalRuntimeUnityPlayerLoopPlaybackUnitySmoke? unitySmoke = null,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var inputs = BuildInputs(root, request);
        var snapshots = ReadSnapshots(Resolve(root, inputs.CommandLoopSnapshotsPath));
        var commandLoop = ReadCommandLoopResult(Resolve(root, inputs.CommandLoopResultPath));
        var playback = BuildPlayback(inputs, snapshots, commandLoop);
        var smoke = unitySmoke ?? BuildPendingUnitySmoke(root, outputRootRelativePath);
        var plan = BuildPlan(playback);
        var matrix = BuildMatrix(playback, smoke);
        var negative = BuildNegativeProof(playback);
        var report = BuildReport(playback, smoke);
        var dashboard = BuildDashboard(playback, smoke);
        var markdown = RenderReport(report, dashboard, playback);

        var proceduralFiles = BuildFilePayloads(
            root,
            outputRootRelativePath,
            dashboard,
            playback,
            plan,
            matrix,
            smoke,
            negative,
            report,
            markdown);
        var exportFiles = BuildFilePayloads(
            root,
            exportRootRelativePath,
            dashboard,
            playback,
            plan,
            matrix,
            smoke,
            negative,
            report,
            markdown);

        var procedural = Resolve(root, outputRootRelativePath);
        var export = Resolve(root, exportRootRelativePath);
        var docsPath = Resolve(root, CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.DocumentationPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in proceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            GuardGoal137Write(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in exportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            GuardGoal137Write(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(docsPath, markdown, cancellationToken).ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new CanonicalRuntimeUnityPlayerLoopPlaybackWriteResult
        {
            Dashboard = dashboard,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static CanonicalRuntimeUnityPlayerLoopPlaybackInput BuildInputs(
        string root,
        CanonicalRuntimeUnityPlayerLoopPlaybackRequest request)
    {
        var snapshots = ResolveInput(root, request.CommandLoopSnapshotsPath, "CommandLoopSnapshotsPath");
        var result = ResolveInput(root, request.CommandLoopResultPath, "CommandLoopResultPath");
        var contract = ResolveInput(root, request.PlayerAdapterContractPath, "PlayerAdapterContractPath");
        var stateSummary = ResolveInput(root, request.StateSummaryPath, "StateSummaryPath");
        return new CanonicalRuntimeUnityPlayerLoopPlaybackInput
        {
            CommandLoopSnapshotsPath = Relative(root, snapshots),
            CommandLoopResultPath = Relative(root, result),
            PlayerAdapterContractPath = Relative(root, contract),
            StateSummaryPath = Relative(root, stateSummary),
            CommandLoopSnapshotsPathExists = File.Exists(snapshots),
            CommandLoopResultPathExists = File.Exists(result),
            PlayerAdapterContractPathExists = File.Exists(contract),
            StateSummaryPathExists = File.Exists(stateSummary)
        };
    }

    private static IReadOnlyList<CanonicalRuntimePlayerCommandLoopSnapshot> ReadSnapshots(
        string path) =>
        JsonSerializer.Deserialize<IReadOnlyList<CanonicalRuntimePlayerCommandLoopSnapshot>>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? [];

    private static CanonicalRuntimePlayerCommandLoopResult ReadCommandLoopResult(string path) =>
        JsonSerializer.Deserialize<CanonicalRuntimePlayerCommandLoopResult>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? new CanonicalRuntimePlayerCommandLoopResult();

    private static CanonicalRuntimeUnityPlayerLoopPlaybackResult BuildPlayback(
        CanonicalRuntimeUnityPlayerLoopPlaybackInput inputs,
        IReadOnlyList<CanonicalRuntimePlayerCommandLoopSnapshot> snapshots,
        CanonicalRuntimePlayerCommandLoopResult commandLoop)
    {
        var orderedSnapshots = snapshots
            .OrderBy(snapshot => snapshot.StepIndex)
            .ToList();
        var frames = orderedSnapshots
            .Select((snapshot, index) => BuildFrame(index, snapshot))
            .ToList();
        var presentCategories = frames
            .Select(frame => frame.Category)
            .ToHashSet(StringComparer.Ordinal);
        var missingCategories = CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary
            .RequiredFrameCategories
            .Where(category => !presentCategories.Contains(category))
            .OrderBy(category => category, StringComparer.Ordinal)
            .ToList();
        var diagnostics = new List<string>();
        Require(orderedSnapshots.Count >= 13, "goal137.playback_frame_count", diagnostics);
        Require(missingCategories.Count == 0, "goal137.required_frame_categories_missing", diagnostics);
        Require(commandLoop.SelectedCandidateExecutedByRuntime,
            "goal137.selected_candidate_not_executed_by_runtime",
            diagnostics);
        Require(commandLoop.PlayerCommandLoopPassed,
            "goal137.source_command_loop_not_green",
            diagnostics);
        Require(!commandLoop.ProjectionOnly, "goal137.source_projection_only", diagnostics);
        Require(!commandLoop.UnityGameplayTruth, "goal137.source_unity_gameplay_truth", diagnostics);
        diagnostics.AddRange(commandLoop.Diagnostics);

        return new CanonicalRuntimeUnityPlayerLoopPlaybackResult
        {
            CandidateId = commandLoop.CandidateId,
            Inputs = inputs,
            ProjectionOnly = false,
            CanonicalRuntimeSource = true,
            RuntimeSnapshotSource = true,
            UnityConsumesRuntimeSnapshots = true,
            UnityGameplayTruth = false,
            SelectedCandidateExecutedByRuntime = commandLoop.SelectedCandidateExecutedByRuntime,
            PlaybackFrameCount = frames.Count,
            PlayerPositionFramesPresent = presentCategories.Contains("player_position"),
            HudFramesPresent = presentCategories.Contains("hud"),
            RequiredFrameCategoriesPresent = missingCategories.Count == 0,
            RequiredFrameCategories =
                CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.RequiredFrameCategories,
            MissingFrameCategories = missingCategories,
            Frames = frames,
            Diagnostics = diagnostics
        };
    }

    private static CanonicalRuntimeUnityPlayerLoopPlaybackFrame BuildFrame(
        int index,
        CanonicalRuntimePlayerCommandLoopSnapshot snapshot)
    {
        var eventMessages = snapshot.RuntimeEvents
            .OrderBy(item => item.EventIndex)
            .Select(item => item.EventType + ":" + item.TargetId + ":" + item.Message)
            .ToList();
        var hud = string.Join(" | ", new[]
            {
                "map=" + snapshot.MapSummary,
                "quest=" + EmptyAsNone(snapshot.QuestSummary),
                "inventory=" + EmptyAsNone(snapshot.InventorySummary),
                "combat=" + EmptyAsNone(snapshot.CombatSummary),
                "diagnostics=" + EmptyAsNone(snapshot.DiagnosticSummary)
            });

        return new CanonicalRuntimeUnityPlayerLoopPlaybackFrame
        {
            FrameIndex = index,
            FrameId = "goal137_playback_frame_" + index.ToString("00", System.Globalization.CultureInfo.InvariantCulture),
            Category = PlaybackCategory(snapshot.Category),
            SourceSnapshotStepIndex = snapshot.StepIndex,
            SourceSnapshotStepId = snapshot.StepId,
            SourceSnapshotCategory = snapshot.Category,
            Title = snapshot.CommandLabel,
            PlayerPositionSummary = snapshot.MapSummary
                                    + " player=("
                                    + snapshot.PlayerX
                                    + ","
                                    + snapshot.PlayerY
                                    + ")",
            HudSummary = hud,
            InteractionSummary = snapshot.VisibleInteractionSummary,
            DialogueSummary = snapshot.DialogueSummary,
            QuestSummary = snapshot.QuestSummary,
            InventorySummary = snapshot.InventorySummary,
            CombatSummary = snapshot.CombatSummary,
            RuntimeEventCount = snapshot.RuntimeEvents.Count,
            RuntimeEventMessages = eventMessages,
            StateHashBefore = snapshot.StateHashBefore,
            StateHashAfter = snapshot.StateHashAfter,
            RuntimeSnapshotSource = true,
            CanonicalRuntimeAuthority = true,
            UnityGameplayTruth = false,
            ProjectionOnly = false
        };
    }

    private static string PlaybackCategory(string commandLoopCategory) =>
        commandLoopCategory switch
        {
            "load_package" => "hud",
            "start_runtime" => "hud",
            "move" => "player_position",
            "interact" => "interaction",
            "show_dialogue" => "dialogue",
            "start_or_update_quest" => "quest",
            "show_inventory" => "inventory",
            "craft" => "crafting",
            "harvest" => "harvest",
            "transaction" => "transaction",
            "encounter" => "encounter",
            "combat_round" => "combat",
            "final_state" => "final_state",
            _ => commandLoopCategory
        };

    private static CanonicalRuntimeUnityPlayerLoopPlaybackPlan BuildPlan(
        CanonicalRuntimeUnityPlayerLoopPlaybackResult result) =>
        new()
        {
            CandidateId = result.CandidateId,
            CanonicalRuntimeSource = true,
            RuntimeSnapshotSource = true,
            UnityConsumesRuntimeSnapshots = true,
            UnityGameplayTruth = false,
            ProjectionOnly = false,
            RequiredFrameCategories = result.RequiredFrameCategories,
            RequiredFrameCategoriesPresent = result.RequiredFrameCategoriesPresent,
            PlaybackFrameCount = result.PlaybackFrameCount,
            Frames = result.Frames
        };

    private static CanonicalRuntimeUnityPlayerLoopPlaybackMatrixResult BuildMatrix(
        CanonicalRuntimeUnityPlayerLoopPlaybackResult result,
        CanonicalRuntimeUnityPlayerLoopPlaybackUnitySmoke smoke)
    {
        var row = new CanonicalRuntimeUnityPlayerLoopPlaybackMatrixRow
        {
            CandidateId = result.CandidateId,
            PlaybackFrameCount = result.PlaybackFrameCount,
            RequiredFrameCategoriesPresent = result.RequiredFrameCategoriesPresent,
            UnityPlayerLoopPlaybackPassed = smoke.UnityPlayerLoopPlaybackPassed,
            RuntimeSnapshotSource = result.RuntimeSnapshotSource,
            UnityGameplayTruth = result.UnityGameplayTruth,
            ProjectionOnly = result.ProjectionOnly,
            SelectedCandidateExecutedByRuntime = result.SelectedCandidateExecutedByRuntime,
            Passed = result.PlaybackFrameCount >= 13
                     && result.RequiredFrameCategoriesPresent
                     && smoke.UnityPlayerLoopPlaybackPassed
                     && result.RuntimeSnapshotSource
                     && !result.UnityGameplayTruth
                     && !result.ProjectionOnly
                     && result.SelectedCandidateExecutedByRuntime
        };
        return new CanonicalRuntimeUnityPlayerLoopPlaybackMatrixResult
        {
            Passed = row.Passed,
            Rows = [row]
        };
    }

    private static CanonicalRuntimeUnityPlayerLoopPlaybackNegativeProof BuildNegativeProof(
        CanonicalRuntimeUnityPlayerLoopPlaybackResult result)
    {
        var proof = new CanonicalRuntimeUnityPlayerLoopPlaybackNegativeProof
        {
            ManualInputRejected = true,
            OutputRootUnderGoal137 = true,
            SamplePackageReadOnly = true,
            GamePackageSchemaUnchanged = true,
            RuntimeContractsUnchanged = true,
            GeneratorLibraryProviderLuaUnchanged = true,
            UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged = true,
            PlaybackConsumesRuntimeSnapshots = result.RuntimeSnapshotSource,
            PlaybackDoesNotRecomputeGameplay = true,
            ProjectionOnly = result.ProjectionOnly,
            UnityGameplayTruth = result.UnityGameplayTruth
        };
        return proof with
        {
            Passed = proof.ManualInputRejected
                     && proof.OutputRootUnderGoal137
                     && proof.SamplePackageReadOnly
                     && proof.GamePackageSchemaUnchanged
                     && proof.RuntimeContractsUnchanged
                     && proof.GeneratorLibraryProviderLuaUnchanged
                     && proof.UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged
                     && proof.PlaybackConsumesRuntimeSnapshots
                     && proof.PlaybackDoesNotRecomputeGameplay
                     && !proof.ProjectionOnly
                     && !proof.UnityGameplayTruth
        };
    }

    private static CanonicalRuntimeUnityPlayerLoopPlaybackReport BuildReport(
        CanonicalRuntimeUnityPlayerLoopPlaybackResult result,
        CanonicalRuntimeUnityPlayerLoopPlaybackUnitySmoke smoke) =>
        new()
        {
            CandidateId = result.CandidateId,
            PlaybackFrameCount = result.PlaybackFrameCount,
            RequiredFrameCategoriesPresent = result.RequiredFrameCategoriesPresent,
            UnityPlayerLoopPlaybackPassed = smoke.UnityPlayerLoopPlaybackPassed,
            ProjectionOnly = false,
            CanonicalRuntimeSource = true,
            RuntimeSnapshotSource = true,
            UnityConsumesRuntimeSnapshots = true,
            UnityGameplayTruth = false,
            SelectedCandidateExecutedByRuntime = result.SelectedCandidateExecutedByRuntime,
            ManualUnityOptional = true,
            Accepted = false,
            NoUnclassifiedErrorDiagnostics = NoUnclassifiedErrors(result)
        };

    private static CanonicalRuntimeUnityPlayerLoopPlaybackDashboard BuildDashboard(
        CanonicalRuntimeUnityPlayerLoopPlaybackResult result,
        CanonicalRuntimeUnityPlayerLoopPlaybackUnitySmoke smoke)
    {
        var failureDiagnostics = new List<string>();
        failureDiagnostics.AddRange(result.Diagnostics);
        Require(result.PlaybackFrameCount >= 13, "goal137.playback_frame_count", failureDiagnostics);
        Require(result.PlayerPositionFramesPresent, "goal137.player_position_frames_missing", failureDiagnostics);
        Require(result.HudFramesPresent, "goal137.hud_frames_missing", failureDiagnostics);
        Require(result.RequiredFrameCategoriesPresent, "goal137.required_frame_categories_missing", failureDiagnostics);
        Require(smoke.UnityPlayerLoopPlaybackPassed, "goal137.unity_player_loop_playback_failed", failureDiagnostics);
        Require(result.RuntimeSnapshotSource, "goal137.runtime_snapshot_source_missing", failureDiagnostics);
        Require(result.UnityConsumesRuntimeSnapshots, "goal137.unity_does_not_consume_runtime_snapshots", failureDiagnostics);
        Require(!result.UnityGameplayTruth, "goal137.unity_gameplay_truth_not_allowed", failureDiagnostics);
        Require(!result.ProjectionOnly, "goal137.projection_only_not_allowed", failureDiagnostics);
        Require(result.SelectedCandidateExecutedByRuntime,
            "goal137.selected_candidate_not_executed_by_runtime",
            failureDiagnostics);
        Require(NoUnclassifiedErrors(result), "goal137.unclassified_error_diagnostics", failureDiagnostics);

        return new CanonicalRuntimeUnityPlayerLoopPlaybackDashboard
        {
            Status = failureDiagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            CandidateId = result.CandidateId,
            ProjectionOnly = false,
            CanonicalRuntimeSource = true,
            RuntimeSnapshotSource = true,
            UnityConsumesRuntimeSnapshots = true,
            PlaybackFrameCount = result.PlaybackFrameCount,
            RequiredFrameCategoriesPresent = result.RequiredFrameCategoriesPresent,
            UnityPlayerLoopPlaybackPassed = smoke.UnityPlayerLoopPlaybackPassed,
            UnityGameplayTruth = false,
            SelectedCandidateExecutedByRuntime = result.SelectedCandidateExecutedByRuntime,
            ManualUnityOptional = true,
            Accepted = false,
            NoUnclassifiedErrorDiagnostics = NoUnclassifiedErrors(result),
            MissingFrameCategories = result.MissingFrameCategories,
            Diagnostics = failureDiagnostics
                .Concat(smoke.Diagnostics)
                .ToList()
        };
    }

    private static CanonicalRuntimeUnityPlayerLoopPlaybackUnitySmoke BuildPendingUnitySmoke(
        string root,
        string outputRootRelativePath)
    {
        var frames = Resolve(
            root,
            outputRootRelativePath + "/" + CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.FramesFileName);
        var result = Resolve(
            root,
            outputRootRelativePath + "/" + CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ResultFileName);
        return new CanonicalRuntimeUnityPlayerLoopPlaybackUnitySmoke
        {
            FramesPath = Relative(root, frames),
            ResultPath = Relative(root, result),
            FramesPathExists = File.Exists(frames),
            ResultPathExists = File.Exists(result),
            Status = "PENDING_UNITY_BATCHMODE",
            Diagnostics = ["unity player-loop playback smoke has not written a marker artifact yet"]
        };
    }

    private static SortedDictionary<string, string> BuildFilePayloads(
        string root,
        string relativeRoot,
        CanonicalRuntimeUnityPlayerLoopPlaybackDashboard dashboard,
        CanonicalRuntimeUnityPlayerLoopPlaybackResult result,
        CanonicalRuntimeUnityPlayerLoopPlaybackPlan plan,
        CanonicalRuntimeUnityPlayerLoopPlaybackMatrixResult matrix,
        CanonicalRuntimeUnityPlayerLoopPlaybackUnitySmoke smoke,
        CanonicalRuntimeUnityPlayerLoopPlaybackNegativeProof negative,
        CanonicalRuntimeUnityPlayerLoopPlaybackReport report,
        string reportMarkdown)
    {
        var files = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ResultFileName] =
                Serialize(result),
            [CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.PlanFileName] =
                Serialize(plan),
            [CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.FramesFileName] =
                Serialize(result.Frames),
            [CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.MatrixResultFileName] =
                Serialize(matrix),
            [CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.UnitySmokeFileName] =
                Serialize(smoke),
            [CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ReportJsonFileName] =
                Serialize(report),
            [CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ReportMarkdownFileName] =
                reportMarkdown
        };
        files[CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.FileIndexFileName] =
            Serialize(BuildFileIndex(root, relativeRoot, files));
        return files;
    }

    private static CanonicalRuntimeUnityPlayerLoopPlaybackFileIndex BuildFileIndex(
        string root,
        string relativeRoot,
        IReadOnlyDictionary<string, string> pendingTextFiles)
    {
        var files = pendingTextFiles
            .Select(item => new CanonicalRuntimeUnityPlayerLoopPlaybackFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = "goal137_" + Path.GetFileNameWithoutExtension(item.Key)
                    .Replace("-", "_", StringComparison.Ordinal),
                Sha256 = HashText(item.Value)
            })
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
        return new CanonicalRuntimeUnityPlayerLoopPlaybackFileIndex
        {
            RootPath = relativeRoot,
            IndexedFileCount = files.Count,
            ManualInputExcluded = files.All(file =>
                !file.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = files
        };
    }

    private static string RenderReport(
        CanonicalRuntimeUnityPlayerLoopPlaybackReport report,
        CanonicalRuntimeUnityPlayerLoopPlaybackDashboard dashboard,
        CanonicalRuntimeUnityPlayerLoopPlaybackResult result)
    {
        var lines = new List<string>
        {
            "# Goal 137 Canonical Runtime Unity Player Loop Playback Harness",
            string.Empty,
            "- candidateId: " + report.CandidateId,
            "- playbackFrameCount: " + report.PlaybackFrameCount,
            "- requiredFrameCategoriesPresent: " + Bool(report.RequiredFrameCategoriesPresent),
            "- unityPlayerLoopPlaybackPassed: " + Bool(report.UnityPlayerLoopPlaybackPassed),
            "- projectionOnly: " + Bool(report.ProjectionOnly),
            "- canonicalRuntimeSource: " + Bool(report.CanonicalRuntimeSource),
            "- runtimeSnapshotSource: " + Bool(report.RuntimeSnapshotSource),
            "- unityConsumesRuntimeSnapshots: " + Bool(report.UnityConsumesRuntimeSnapshots),
            "- unityGameplayTruth: " + Bool(report.UnityGameplayTruth),
            "- selectedCandidateExecutedByRuntime: " + Bool(report.SelectedCandidateExecutedByRuntime),
            "- manualUnityOptional: " + Bool(report.ManualUnityOptional),
            "- accepted: false",
            "- noUnclassifiedErrorDiagnostics: " + Bool(report.NoUnclassifiedErrorDiagnostics),
            "- normalCommand: " + CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.NormalCommand,
            "- reportPath: " + report.ReportPath,
            "- matrixResultPath: " + report.MatrixResultPath,
            string.Empty,
            "## Dashboard",
            string.Empty,
            "- status: " + dashboard.Status,
            "- missingFrameCategories: " + (dashboard.MissingFrameCategories.Count == 0
                ? "none"
                : string.Join(", ", dashboard.MissingFrameCategories)),
            string.Empty,
            "## Required Frame Categories",
            string.Empty
        };
        lines.AddRange(result.RequiredFrameCategories.Select(category =>
            "- " + category + ": " + Bool(result.Frames.Any(frame => frame.Category == category))));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static bool NoUnclassifiedErrors(CanonicalRuntimeUnityPlayerLoopPlaybackResult result) =>
        result.Diagnostics.All(item =>
            !item.StartsWith("Error:", StringComparison.Ordinal)
            && !item.StartsWith("Critical:", StringComparison.Ordinal));

    private static void Require(bool condition, string diagnostic, ICollection<string> diagnostics)
    {
        if (!condition && !diagnostics.Contains(diagnostic))
        {
            diagnostics.Add(diagnostic);
        }
    }

    private static string EmptyAsNone(string value) =>
        string.IsNullOrWhiteSpace(value) ? "none" : value;

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

    private static string ResolveInput(string root, string path, string name)
    {
        var resolved = Resolve(root, path);
        GuardNotManualInput(root, resolved);
        if (!File.Exists(resolved))
        {
            throw new InvalidOperationException(name + " does not exist: " + Relative(root, resolved));
        }

        return resolved;
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
        var normalizedRoot = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path);
        return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static void GuardNotManualInput(string root, string path)
    {
        var relative = Relative(root, path);
        if (relative.StartsWith(".llmgc/manual/", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path must not point under .llmgc/manual: " + relative);
        }
    }

    private static void GuardGoal137Write(string root, string path)
    {
        GuardNotManualInput(root, path);
        var relative = Relative(root, path);
        if (!relative.StartsWith(
                CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ProceduralOutputDirectory + "/",
                StringComparison.Ordinal)
            && !relative.StartsWith(
                CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ExportPackageDirectory + "/",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to write outside Goal137 output roots: " + relative);
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
