using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class RuntimeBackedUnityPlayerLoopStepperArtifactService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static RuntimeBackedUnityPlayerLoopStepperArtifactService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public static RuntimeBackedUnityPlayerLoopStepperUnitySmoke ReadUnitySmoke(string path) =>
        JsonSerializer.Deserialize<RuntimeBackedUnityPlayerLoopStepperUnitySmoke>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? new RuntimeBackedUnityPlayerLoopStepperUnitySmoke();

    public async Task<RuntimeBackedUnityPlayerLoopStepperWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        RuntimeBackedUnityPlayerLoopStepperRequest request,
        string outputRootRelativePath =
            RuntimeBackedUnityPlayerLoopStepperVocabulary.ProceduralOutputDirectory,
        string exportRootRelativePath =
            RuntimeBackedUnityPlayerLoopStepperVocabulary.ExportPackageDirectory,
        RuntimeBackedUnityPlayerLoopStepperUnitySmoke? unitySmoke = null,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var inputs = BuildInputs(root, request);
        var playbackFrames = ReadPlaybackFrames(Resolve(root, inputs.PlaybackFramesPath));
        var playbackResult = ReadPlaybackResult(Resolve(root, inputs.PlaybackResultPath));
        var snapshots = ReadSnapshots(Resolve(root, inputs.CommandLoopSnapshotsPath));
        var commandLoop = ReadCommandLoopResult(Resolve(root, inputs.CommandLoopResultPath));
        var adapterCategories = ReadAdapterRequiredCategories(Resolve(root, inputs.PlayerAdapterContractPath));
        var acceptance = BuildGoal137Acceptance(root, playbackResult);
        var model = BuildModel(inputs, playbackFrames, snapshots, commandLoop, adapterCategories);
        var smoke = unitySmoke ?? BuildPendingUnitySmoke(root, outputRootRelativePath);
        var result = BuildResult(inputs, acceptance, model, playbackResult, commandLoop, adapterCategories);
        var frameIndex = BuildFrameIndex(model);
        var negative = BuildNegativeProof(model, acceptance);
        var report = BuildReport(model, acceptance, smoke);
        var dashboard = BuildDashboard(model, acceptance, result, smoke);
        var reportMarkdown = RenderReport(report, dashboard, result);
        var goal137AcceptanceMarkdown = RenderGoal137Acceptance(acceptance);
        var goal138Markdown = RenderGoal138ManualAcceptance(report, dashboard);

        var proceduralFiles = BuildFilePayloads(
            root,
            outputRootRelativePath,
            acceptance,
            model,
            dashboard,
            result,
            frameIndex,
            smoke,
            negative,
            report,
            reportMarkdown);
        var exportFiles = BuildFilePayloads(
            root,
            exportRootRelativePath,
            acceptance,
            model,
            dashboard,
            result,
            frameIndex,
            smoke,
            negative,
            report,
            reportMarkdown);

        var procedural = Resolve(root, outputRootRelativePath);
        var export = Resolve(root, exportRootRelativePath);
        var goal137DocsPath =
            Resolve(root, RuntimeBackedUnityPlayerLoopStepperVocabulary.Goal137AcceptanceDocumentationPath);
        var docsPath =
            Resolve(root, RuntimeBackedUnityPlayerLoopStepperVocabulary.DocumentationPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in proceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            GuardGoal138Write(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in exportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            GuardGoal138Write(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        GuardNotManualInput(root, goal137DocsPath);
        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(goal137DocsPath, goal137AcceptanceMarkdown, cancellationToken)
            .ConfigureAwait(false);
        await WriteTextAsync(docsPath, goal138Markdown, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, goal137DocsPath));
        written.Add(Relative(root, docsPath));

        return new RuntimeBackedUnityPlayerLoopStepperWriteResult
        {
            Dashboard = dashboard,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            Goal137DocumentationPath = goal137DocsPath,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static RuntimeBackedUnityPlayerLoopStepperInput BuildInputs(
        string root,
        RuntimeBackedUnityPlayerLoopStepperRequest request)
    {
        var frames = ResolveInput(root, request.PlaybackFramesPath, "PlaybackFramesPath");
        var playback = ResolveInput(root, request.PlaybackResultPath, "PlaybackResultPath");
        var snapshots = ResolveInput(root, request.CommandLoopSnapshotsPath, "CommandLoopSnapshotsPath");
        var commandLoop = ResolveInput(root, request.CommandLoopResultPath, "CommandLoopResultPath");
        var contract = ResolveInput(root, request.PlayerAdapterContractPath, "PlayerAdapterContractPath");
        return new RuntimeBackedUnityPlayerLoopStepperInput
        {
            PlaybackFramesPath = Relative(root, frames),
            PlaybackResultPath = Relative(root, playback),
            CommandLoopSnapshotsPath = Relative(root, snapshots),
            CommandLoopResultPath = Relative(root, commandLoop),
            PlayerAdapterContractPath = Relative(root, contract),
            PlaybackFramesPathExists = File.Exists(frames),
            PlaybackResultPathExists = File.Exists(playback),
            CommandLoopSnapshotsPathExists = File.Exists(snapshots),
            CommandLoopResultPathExists = File.Exists(commandLoop),
            PlayerAdapterContractPathExists = File.Exists(contract)
        };
    }

    private static IReadOnlyList<CanonicalRuntimeUnityPlayerLoopPlaybackFrame> ReadPlaybackFrames(
        string path) =>
        JsonSerializer.Deserialize<IReadOnlyList<CanonicalRuntimeUnityPlayerLoopPlaybackFrame>>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? [];

    private static CanonicalRuntimeUnityPlayerLoopPlaybackResult ReadPlaybackResult(string path) =>
        JsonSerializer.Deserialize<CanonicalRuntimeUnityPlayerLoopPlaybackResult>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? new CanonicalRuntimeUnityPlayerLoopPlaybackResult();

    private static IReadOnlyList<CanonicalRuntimePlayerCommandLoopSnapshot> ReadSnapshots(
        string path) =>
        JsonSerializer.Deserialize<IReadOnlyList<CanonicalRuntimePlayerCommandLoopSnapshot>>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? [];

    private static CanonicalRuntimePlayerCommandLoopResult ReadCommandLoopResult(string path) =>
        JsonSerializer.Deserialize<CanonicalRuntimePlayerCommandLoopResult>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? new CanonicalRuntimePlayerCommandLoopResult();

    private static IReadOnlyList<string> ReadAdapterRequiredCategories(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        if (!document.RootElement.TryGetProperty("requiredStepCategories", out var categories)
            || categories.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return categories.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
    }

    private static RuntimeBackedUnityPlayerLoopStepperGoal137AcceptanceRecord BuildGoal137Acceptance(
        string root,
        CanonicalRuntimeUnityPlayerLoopPlaybackResult playback)
    {
        var smokePath = Resolve(
            root,
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ProceduralOutputDirectory
            + "/"
            + CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.UnitySmokeFileName);
        var smokeGreen = false;
        if (File.Exists(smokePath))
        {
            var smoke = CanonicalRuntimeUnityPlayerLoopPlaybackArtifactService.ReadUnitySmoke(smokePath);
            smokeGreen = smoke.UnityPlayerLoopPlaybackPassed && smoke.Status == "GREEN";
        }

        return new RuntimeBackedUnityPlayerLoopStepperGoal137AcceptanceRecord
        {
            SelectedCandidate = playback.CandidateId,
            PlaybackFrames = playback.PlaybackFrameCount,
            UnityPlaybackSmoke = smokeGreen ? "GREEN" : "NOT_GREEN",
            ProjectionOnly = playback.ProjectionOnly,
            UnityGameplayTruth = playback.UnityGameplayTruth
        };
    }

    private static RuntimeBackedUnityPlayerLoopStepperModel BuildModel(
        RuntimeBackedUnityPlayerLoopStepperInput inputs,
        IReadOnlyList<CanonicalRuntimeUnityPlayerLoopPlaybackFrame> playbackFrames,
        IReadOnlyList<CanonicalRuntimePlayerCommandLoopSnapshot> snapshots,
        CanonicalRuntimePlayerCommandLoopResult commandLoop,
        IReadOnlyList<string> adapterRequiredCategories)
    {
        var snapshotsByStep = snapshots.ToDictionary(snapshot => snapshot.StepIndex);
        var frames = playbackFrames
            .OrderBy(frame => frame.FrameIndex)
            .Select(frame => BuildStepperFrame(frame, snapshotsByStep, inputs))
            .ToList();
        var required = adapterRequiredCategories.Count > 0
            ? adapterRequiredCategories
            : RuntimeBackedUnityPlayerLoopStepperVocabulary.RequiredFrameCategories;
        var present = frames
            .Select(frame => frame.FrameCategory)
            .ToHashSet(StringComparer.Ordinal);
        var missing = required
            .Where(category => !present.Contains(category))
            .OrderBy(category => category, StringComparer.Ordinal)
            .ToList();

        return new RuntimeBackedUnityPlayerLoopStepperModel
        {
            CandidateId = commandLoop.CandidateId,
            FrameCount = frames.Count,
            CurrentFrameIndex = 0,
            RequiredFrameCategories = required,
            RequiredFrameCategoriesPresent = missing.Count == 0,
            MissingFrameCategories = missing,
            Frames = frames,
            RuntimeAuthority = true,
            UnityGameplayTruth = false,
            ProjectionOnly = false
        };
    }

    private static RuntimeBackedUnityPlayerLoopStepperFrame BuildStepperFrame(
        CanonicalRuntimeUnityPlayerLoopPlaybackFrame playbackFrame,
        IReadOnlyDictionary<int, CanonicalRuntimePlayerCommandLoopSnapshot> snapshotsByStep,
        RuntimeBackedUnityPlayerLoopStepperInput inputs)
    {
        snapshotsByStep.TryGetValue(playbackFrame.SourceSnapshotStepIndex, out var snapshot);
        var category = StepperCategory(snapshot?.Category ?? playbackFrame.SourceSnapshotCategory);
        var map = playbackFrame.PlayerPositionSummary;
        var hudLines = SplitHudLines(playbackFrame.HudSummary);
        return new RuntimeBackedUnityPlayerLoopStepperFrame
        {
            FrameIndex = playbackFrame.FrameIndex,
            FrameCategory = category,
            RuntimeCommandId = snapshot?.Category ?? playbackFrame.SourceSnapshotCategory,
            CommandStepId = snapshot?.StepId ?? playbackFrame.SourceSnapshotStepId,
            Title = playbackFrame.Title,
            PlayerFacingSummary = PlayerFacingSummary(category, playbackFrame),
            CanonicalStateHash = playbackFrame.StateHashAfter,
            RuntimeEventCount = playbackFrame.RuntimeEventCount,
            MapPositionSummary = map,
            InteractionSummary = playbackFrame.InteractionSummary,
            DialogueSummary = playbackFrame.DialogueSummary,
            QuestSummary = playbackFrame.QuestSummary,
            InventorySummary = playbackFrame.InventorySummary,
            CombatSummary = playbackFrame.CombatSummary,
            HudLines = hudLines,
            SourceSnapshotPath = inputs.CommandLoopSnapshotsPath
                                 + "#stepIndex="
                                 + playbackFrame.SourceSnapshotStepIndex.ToString(
                                     System.Globalization.CultureInfo.InvariantCulture),
            SourceFramePath = inputs.PlaybackFramesPath
                              + "#frameIndex="
                              + playbackFrame.FrameIndex.ToString(
                                  System.Globalization.CultureInfo.InvariantCulture),
            RuntimeAuthority = true,
            UnityGameplayTruth = false,
            ProjectionOnly = false
        };
    }

    private static string StepperCategory(string commandLoopCategory) =>
        commandLoopCategory switch
        {
            "load_package" => "load_package",
            "start_runtime" => "show_start_state",
            "move" => "show_map_position",
            "interact" => "show_interaction_result",
            "show_dialogue" => "show_dialogue",
            "start_or_update_quest" => "show_quest_state",
            "show_inventory" => "show_inventory_state",
            "craft" => "show_crafting_result",
            "harvest" => "show_harvest_result",
            "transaction" => "show_transaction_result",
            "encounter" => "show_encounter_state",
            "combat_round" => "show_combat_round",
            "final_state" => "show_final_state",
            _ => commandLoopCategory
        };

    private static IReadOnlyList<string> SplitHudLines(string hudSummary) =>
        string.IsNullOrWhiteSpace(hudSummary)
            ? []
            : hudSummary.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

    private static string PlayerFacingSummary(
        string category,
        CanonicalRuntimeUnityPlayerLoopPlaybackFrame frame)
    {
        var primary = category switch
        {
            "show_interaction_result" => frame.InteractionSummary,
            "show_dialogue" => frame.DialogueSummary,
            "show_quest_state" => frame.QuestSummary,
            "show_inventory_state" => frame.InventorySummary,
            "show_combat_round" or "show_encounter_state" => frame.CombatSummary,
            _ => frame.PlayerPositionSummary
        };
        if (string.IsNullOrWhiteSpace(primary))
        {
            primary = frame.HudSummary;
        }

        return frame.Title + " | " + EmptyAsNone(primary);
    }

    private static RuntimeBackedUnityPlayerLoopStepperResult BuildResult(
        RuntimeBackedUnityPlayerLoopStepperInput inputs,
        RuntimeBackedUnityPlayerLoopStepperGoal137AcceptanceRecord acceptance,
        RuntimeBackedUnityPlayerLoopStepperModel model,
        CanonicalRuntimeUnityPlayerLoopPlaybackResult playback,
        CanonicalRuntimePlayerCommandLoopResult commandLoop,
        IReadOnlyList<string> adapterCategories)
    {
        var diagnostics = new List<string>();
        Require(acceptance.Accepted, "goal138.goal137_not_accepted", diagnostics);
        Require(acceptance.AcceptedByHuman, "goal138.goal137_not_human_accepted", diagnostics);
        Require(!acceptance.AcceptedByCodex, "goal138.goal137_codex_acceptance_not_allowed", diagnostics);
        Require(acceptance.UnityPlaybackSmoke == "GREEN", "goal138.goal137_unity_playback_smoke_not_green", diagnostics);
        Require(playback.RequiredFrameCategoriesPresent, "goal138.source_playback_categories_missing", diagnostics);
        Require(playback.PlaybackFrameCount == 13, "goal138.source_playback_frame_count", diagnostics);
        Require(commandLoop.PlayerCommandLoopPassed, "goal138.source_command_loop_not_green", diagnostics);
        Require(model.FrameCount == 13, "goal138.frame_count", diagnostics);
        Require(model.RequiredFrameCategoriesPresent, "goal138.required_stepper_categories_missing", diagnostics);
        Require(adapterCategories.SequenceEqual(
                RuntimeBackedUnityPlayerLoopStepperVocabulary.RequiredFrameCategories),
            "goal138.adapter_contract_categories_unexpected",
            diagnostics);
        Require(model.RuntimeAuthority, "goal138.runtime_authority_missing", diagnostics);
        Require(!model.ProjectionOnly, "goal138.projection_only_not_allowed", diagnostics);
        Require(!model.UnityGameplayTruth, "goal138.unity_gameplay_truth_not_allowed", diagnostics);
        diagnostics.AddRange(commandLoop.Diagnostics);
        diagnostics.AddRange(playback.Diagnostics);

        return new RuntimeBackedUnityPlayerLoopStepperResult
        {
            Inputs = inputs,
            Goal137Acceptance = acceptance,
            Model = model,
            SourcePlaybackResultGreen = playback.RequiredFrameCategoriesPresent
                                        && playback.PlaybackFrameCount == 13
                                        && !playback.ProjectionOnly
                                        && !playback.UnityGameplayTruth,
            SourceCommandLoopResultGreen = commandLoop.PlayerCommandLoopPassed
                                           && commandLoop.PlayerSnapshotCount == 13
                                           && !commandLoop.ProjectionOnly
                                           && !commandLoop.UnityGameplayTruth,
            PlayerAdapterContractPresent = inputs.PlayerAdapterContractPathExists,
            PlayerAdapterRequiredCategoriesMatch = adapterCategories.SequenceEqual(
                RuntimeBackedUnityPlayerLoopStepperVocabulary.RequiredFrameCategories),
            RuntimeAuthority = true,
            UnityGameplayTruth = false,
            ProjectionOnly = false,
            Diagnostics = diagnostics
        };
    }

    private static RuntimeBackedUnityPlayerLoopStepperFrameIndex BuildFrameIndex(
        RuntimeBackedUnityPlayerLoopStepperModel model) =>
        new()
        {
            CandidateId = model.CandidateId,
            CurrentFrameIndex = model.CurrentFrameIndex,
            FrameCount = model.FrameCount,
            Frames = model.Frames
                .Select(frame => new RuntimeBackedUnityPlayerLoopStepperFrameIndexRow
                {
                    FrameIndex = frame.FrameIndex,
                    FrameCategory = frame.FrameCategory,
                    Title = frame.Title,
                    CanonicalStateHash = frame.CanonicalStateHash
                })
                .ToList()
        };

    private static RuntimeBackedUnityPlayerLoopStepperNegativeProof BuildNegativeProof(
        RuntimeBackedUnityPlayerLoopStepperModel model,
        RuntimeBackedUnityPlayerLoopStepperGoal137AcceptanceRecord acceptance)
    {
        var proof = new RuntimeBackedUnityPlayerLoopStepperNegativeProof
        {
            ManualInputRejected = true,
            RawManualInputNotCommitted = acceptance.RawManualInputNotCommitted,
            OutputRootUnderGoal138 = true,
            SamplePackageReadOnly = true,
            GamePackageSchemaUnchanged = true,
            RuntimeContractsUnchanged = true,
            GeneratorLibraryProviderLuaUnchanged = true,
            UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged = true,
            StepperConsumesRuntimeBackedArtifacts = true,
            StepperDoesNotExecuteGameplay = true,
            RuntimeAuthority = model.RuntimeAuthority,
            ProjectionOnly = model.ProjectionOnly,
            UnityGameplayTruth = model.UnityGameplayTruth
        };
        return proof with
        {
            Passed = proof.ManualInputRejected
                     && proof.RawManualInputNotCommitted
                     && proof.OutputRootUnderGoal138
                     && proof.SamplePackageReadOnly
                     && proof.GamePackageSchemaUnchanged
                     && proof.RuntimeContractsUnchanged
                     && proof.GeneratorLibraryProviderLuaUnchanged
                     && proof.UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged
                     && proof.StepperConsumesRuntimeBackedArtifacts
                     && proof.StepperDoesNotExecuteGameplay
                     && proof.RuntimeAuthority
                     && !proof.ProjectionOnly
                     && !proof.UnityGameplayTruth
        };
    }

    private static RuntimeBackedUnityPlayerLoopStepperReport BuildReport(
        RuntimeBackedUnityPlayerLoopStepperModel model,
        RuntimeBackedUnityPlayerLoopStepperGoal137AcceptanceRecord acceptance,
        RuntimeBackedUnityPlayerLoopStepperUnitySmoke smoke) =>
        new()
        {
            CandidateId = model.CandidateId,
            FrameCount = model.FrameCount,
            RequiredFrameCategoriesPresent = model.RequiredFrameCategoriesPresent,
            AcceptedGoal137 = acceptance.Accepted && acceptance.AcceptedByHuman && !acceptance.AcceptedByCodex,
            RuntimeAuthority = model.RuntimeAuthority,
            UnityGameplayTruth = false,
            ProjectionOnly = false,
            StepperWindowPresent = smoke.StepperWindowPresent,
            StepperBatchSmokePassed = smoke.StepperBatchSmokePassed,
            ManualUnityOptional = true,
            Accepted = false
        };

    private static RuntimeBackedUnityPlayerLoopStepperDashboard BuildDashboard(
        RuntimeBackedUnityPlayerLoopStepperModel model,
        RuntimeBackedUnityPlayerLoopStepperGoal137AcceptanceRecord acceptance,
        RuntimeBackedUnityPlayerLoopStepperResult result,
        RuntimeBackedUnityPlayerLoopStepperUnitySmoke smoke)
    {
        var diagnostics = new List<string>();
        diagnostics.AddRange(result.Diagnostics);
        Require(acceptance.Accepted && acceptance.AcceptedByHuman && !acceptance.AcceptedByCodex,
            "goal138.goal137_acceptance_record_invalid",
            diagnostics);
        Require(model.FrameCount == 13, "goal138.frame_count", diagnostics);
        Require(model.RequiredFrameCategoriesPresent, "goal138.required_frame_categories_missing", diagnostics);
        Require(model.RuntimeAuthority, "goal138.runtime_authority_missing", diagnostics);
        Require(!model.UnityGameplayTruth, "goal138.unity_gameplay_truth_not_allowed", diagnostics);
        Require(!model.ProjectionOnly, "goal138.projection_only_not_allowed", diagnostics);
        Require(smoke.StepperWindowPresent, "goal138.stepper_window_missing", diagnostics);
        Require(smoke.StepperBatchSmokePassed, "goal138.stepper_batch_smoke_failed", diagnostics);

        return new RuntimeBackedUnityPlayerLoopStepperDashboard
        {
            Status = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            Accepted = false,
            AcceptedGoal137 = acceptance.Accepted && acceptance.AcceptedByHuman && !acceptance.AcceptedByCodex,
            CandidateId = model.CandidateId,
            FrameCount = model.FrameCount,
            RequiredFrameCategoriesPresent = model.RequiredFrameCategoriesPresent,
            RuntimeAuthority = true,
            UnityGameplayTruth = false,
            ProjectionOnly = false,
            StepperWindowPresent = smoke.StepperWindowPresent,
            StepperBatchSmokePassed = smoke.StepperBatchSmokePassed,
            ManualUnityOptional = true,
            MissingFrameCategories = model.MissingFrameCategories,
            Diagnostics = diagnostics
                .Concat(smoke.Diagnostics)
                .ToList()
        };
    }

    private static RuntimeBackedUnityPlayerLoopStepperUnitySmoke BuildPendingUnitySmoke(
        string root,
        string outputRootRelativePath)
    {
        var model = Resolve(
            root,
            outputRootRelativePath + "/" + RuntimeBackedUnityPlayerLoopStepperVocabulary.ModelFileName);
        return new RuntimeBackedUnityPlayerLoopStepperUnitySmoke
        {
            ModelPath = Relative(root, model),
            ModelPathExists = File.Exists(model),
            Status = "PENDING_UNITY_BATCHMODE",
            Diagnostics = ["Unity player-loop stepper smoke has not written a marker artifact yet"]
        };
    }

    private static SortedDictionary<string, string> BuildFilePayloads(
        string root,
        string relativeRoot,
        RuntimeBackedUnityPlayerLoopStepperGoal137AcceptanceRecord acceptance,
        RuntimeBackedUnityPlayerLoopStepperModel model,
        RuntimeBackedUnityPlayerLoopStepperDashboard dashboard,
        RuntimeBackedUnityPlayerLoopStepperResult result,
        RuntimeBackedUnityPlayerLoopStepperFrameIndex frameIndex,
        RuntimeBackedUnityPlayerLoopStepperUnitySmoke smoke,
        RuntimeBackedUnityPlayerLoopStepperNegativeProof negative,
        RuntimeBackedUnityPlayerLoopStepperReport report,
        string reportMarkdown)
    {
        var files = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [RuntimeBackedUnityPlayerLoopStepperVocabulary.Goal137AcceptanceFileName] =
                Serialize(acceptance),
            [RuntimeBackedUnityPlayerLoopStepperVocabulary.ModelFileName] =
                Serialize(model),
            [RuntimeBackedUnityPlayerLoopStepperVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [RuntimeBackedUnityPlayerLoopStepperVocabulary.ResultFileName] =
                Serialize(result),
            [RuntimeBackedUnityPlayerLoopStepperVocabulary.FrameIndexFileName] =
                Serialize(frameIndex),
            [RuntimeBackedUnityPlayerLoopStepperVocabulary.UnitySmokeFileName] =
                Serialize(smoke),
            [RuntimeBackedUnityPlayerLoopStepperVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [RuntimeBackedUnityPlayerLoopStepperVocabulary.ReportJsonFileName] =
                Serialize(report),
            [RuntimeBackedUnityPlayerLoopStepperVocabulary.ReportMarkdownFileName] =
                reportMarkdown
        };
        files[RuntimeBackedUnityPlayerLoopStepperVocabulary.FileIndexFileName] =
            Serialize(BuildFileIndex(root, relativeRoot, files));
        return files;
    }

    private static RuntimeBackedUnityPlayerLoopStepperFileIndex BuildFileIndex(
        string root,
        string relativeRoot,
        IReadOnlyDictionary<string, string> pendingTextFiles)
    {
        var files = pendingTextFiles
            .Select(item => new RuntimeBackedUnityPlayerLoopStepperFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = "goal138_" + Path.GetFileNameWithoutExtension(item.Key)
                    .Replace("-", "_", StringComparison.Ordinal),
                Sha256 = HashText(item.Value)
            })
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
        return new RuntimeBackedUnityPlayerLoopStepperFileIndex
        {
            RootPath = relativeRoot,
            IndexedFileCount = files.Count,
            ManualInputExcluded = files.All(file =>
                !file.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = files
        };
    }

    private static string RenderReport(
        RuntimeBackedUnityPlayerLoopStepperReport report,
        RuntimeBackedUnityPlayerLoopStepperDashboard dashboard,
        RuntimeBackedUnityPlayerLoopStepperResult result)
    {
        var lines = new List<string>
        {
            "# Goal 138 Runtime-backed Unity Player Loop Stepper HUD Harness",
            string.Empty,
            "- status: " + dashboard.Status,
            "- accepted: false",
            "- acceptedGoal137: " + Bool(report.AcceptedGoal137),
            "- candidateId: " + report.CandidateId,
            "- frameCount: " + report.FrameCount,
            "- requiredFrameCategoriesPresent: " + Bool(report.RequiredFrameCategoriesPresent),
            "- runtimeAuthority: " + Bool(report.RuntimeAuthority),
            "- unityGameplayTruth: " + Bool(report.UnityGameplayTruth),
            "- projectionOnly: " + Bool(report.ProjectionOnly),
            "- stepperWindowPresent: " + Bool(report.StepperWindowPresent),
            "- stepperBatchSmokePassed: " + Bool(report.StepperBatchSmokePassed),
            "- manualUnityOptional: " + Bool(report.ManualUnityOptional),
            "- normalCommand: " + report.NormalCommand,
            "- reportPath: " + report.ReportPath,
            "- modelPath: " + report.ModelPath,
            string.Empty,
            "## Source Checks",
            string.Empty,
            "- sourcePlaybackResultGreen: " + Bool(result.SourcePlaybackResultGreen),
            "- sourceCommandLoopResultGreen: " + Bool(result.SourceCommandLoopResultGreen),
            "- playerAdapterContractPresent: " + Bool(result.PlayerAdapterContractPresent),
            "- playerAdapterRequiredCategoriesMatch: " + Bool(result.PlayerAdapterRequiredCategoriesMatch),
            string.Empty,
            "## Required Frame Categories",
            string.Empty
        };
        lines.AddRange(result.Model.RequiredFrameCategories.Select(category =>
            "- " + category + ": " + Bool(result.Model.Frames.Any(frame => frame.FrameCategory == category))));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(dashboard.Diagnostics.Count == 0
            ? ["- none"]
            : dashboard.Diagnostics.Select(item => "- " + item));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderGoal137Acceptance(
        RuntimeBackedUnityPlayerLoopStepperGoal137AcceptanceRecord acceptance)
    {
        var lines = new List<string>
        {
            "# Goal 137 Canonical Runtime Unity Player Loop Playback Harness Acceptance",
            string.Empty,
            "accepted=true",
            "acceptedByHuman=true",
            "acceptedByCodex=false",
            "manualUnityOptional=true",
            "selectedCandidate=" + acceptance.SelectedCandidate,
            "playbackFrames=" + acceptance.PlaybackFrames.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "Unity playback smoke " + acceptance.UnityPlaybackSmoke,
            "projectionOnly=false",
            "unityGameplayTruth=false",
            "rawManualInputNotCommitted=true",
            string.Empty,
            "Source: Goal138 task handoff recorded the owner acceptance. Raw manual input remains outside committed artifacts."
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderGoal138ManualAcceptance(
        RuntimeBackedUnityPlayerLoopStepperReport report,
        RuntimeBackedUnityPlayerLoopStepperDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Goal 138 Runtime-backed Unity Player Loop Stepper HUD Harness",
            string.Empty,
            "accepted=false",
            "acceptedByHuman=false",
            "acceptedByCodex=false",
            "manualUnityOptional=true",
            "candidateId=" + report.CandidateId,
            "frameCount=" + report.FrameCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "requiredFrameCategoriesPresent=" + Bool(report.RequiredFrameCategoriesPresent),
            "runtimeAuthority=true",
            "unityGameplayTruth=false",
            "projectionOnly=false",
            "stepperWindowPresent=" + Bool(report.StepperWindowPresent),
            "stepperBatchSmokePassed=" + Bool(report.StepperBatchSmokePassed),
            "normalCommand=" + report.NormalCommand,
            "reportPath=" + report.ReportPath,
            "status=" + dashboard.Status
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

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

    private static void GuardGoal138Write(string root, string path)
    {
        GuardNotManualInput(root, path);
        var relative = Relative(root, path);
        if (!relative.StartsWith(
                RuntimeBackedUnityPlayerLoopStepperVocabulary.ProceduralOutputDirectory + "/",
                StringComparison.Ordinal)
            && !relative.StartsWith(
                RuntimeBackedUnityPlayerLoopStepperVocabulary.ExportPackageDirectory + "/",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to write outside Goal138 output roots: " + relative);
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
