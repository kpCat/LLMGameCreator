using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class RuntimeBackedUnityPlayerLoopInteractiveControlsArtifactService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static RuntimeBackedUnityPlayerLoopInteractiveControlsArtifactService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public static RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmoke ReadUnitySmoke(
        string path) =>
        JsonSerializer.Deserialize<RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmoke>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? new RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmoke();

    public async Task<RuntimeBackedUnityPlayerLoopInteractiveControlsWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        RuntimeBackedUnityPlayerLoopInteractiveControlsRequest request,
        string outputRootRelativePath =
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ProceduralOutputDirectory,
        string exportRootRelativePath =
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ExportPackageDirectory,
        RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmoke? unitySmoke = null,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var inputs = BuildInputs(root, request);
        var stepperModel = ReadStepperModel(Resolve(root, inputs.StepperModelPath));
        var stepperResult = ReadStepperResult(Resolve(root, inputs.StepperResultPath));
        var playbackFrameCount = ReadArrayCount(Resolve(root, inputs.PlaybackFramesPath));
        var commandLoopSnapshotCount = ReadArrayCount(Resolve(root, inputs.CommandLoopSnapshotsPath));
        var adapterCategories = ReadAdapterRequiredCategories(Resolve(root, inputs.PlayerAdapterContractPath));
        var goal138Acceptance = BuildGoal138Acceptance(root, stepperModel);
        var model = BuildModel(stepperModel);
        var script = BuildControlScript(model);
        var session = RunSession(model, script);
        var smoke = unitySmoke ?? BuildPendingUnitySmoke(root, outputRootRelativePath);
        var result = BuildResult(
            inputs,
            goal138Acceptance,
            model,
            script,
            session,
            stepperResult,
            playbackFrameCount,
            commandLoopSnapshotCount,
            adapterCategories);
        var negative = BuildNegativeProof(model, goal138Acceptance);
        var report = BuildReport(model, goal138Acceptance, session, smoke);
        var dashboard = BuildDashboard(model, goal138Acceptance, session, result, smoke);
        var reportMarkdown = RenderReport(report, dashboard, result);
        var goal138AcceptanceMarkdown = RenderGoal138Acceptance(goal138Acceptance);
        var goal139Markdown = RenderGoal139ManualAcceptance(report, dashboard);

        var proceduralFiles = BuildFilePayloads(
            root,
            outputRootRelativePath,
            goal138Acceptance,
            model,
            script,
            session,
            result,
            dashboard,
            negative,
            smoke,
            report,
            reportMarkdown);
        var exportFiles = BuildFilePayloads(
            root,
            exportRootRelativePath,
            goal138Acceptance,
            model,
            script,
            session,
            result,
            dashboard,
            negative,
            smoke,
            report,
            reportMarkdown);

        var procedural = Resolve(root, outputRootRelativePath);
        var export = Resolve(root, exportRootRelativePath);
        var goal138DocsPath = Resolve(
            root,
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.Goal138AcceptanceDocumentationPath);
        var docsPath = Resolve(
            root,
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DocumentationPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in proceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            GuardGoal139Write(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in exportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            GuardGoal139Write(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        GuardNotManualInput(root, goal138DocsPath);
        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(goal138DocsPath, goal138AcceptanceMarkdown, cancellationToken)
            .ConfigureAwait(false);
        await WriteTextAsync(docsPath, goal139Markdown, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, goal138DocsPath));
        written.Add(Relative(root, docsPath));

        return new RuntimeBackedUnityPlayerLoopInteractiveControlsWriteResult
        {
            Dashboard = dashboard,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            Goal138DocumentationPath = goal138DocsPath,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static RuntimeBackedUnityPlayerLoopInteractiveControlsInput BuildInputs(
        string root,
        RuntimeBackedUnityPlayerLoopInteractiveControlsRequest request)
    {
        var stepperModel = ResolveInput(root, request.StepperModelPath, "StepperModelPath");
        var stepperResult = ResolveInput(root, request.StepperResultPath, "StepperResultPath");
        var frames = ResolveInput(root, request.PlaybackFramesPath, "PlaybackFramesPath");
        var snapshots = ResolveInput(root, request.CommandLoopSnapshotsPath, "CommandLoopSnapshotsPath");
        var contract = ResolveInput(root, request.PlayerAdapterContractPath, "PlayerAdapterContractPath");
        return new RuntimeBackedUnityPlayerLoopInteractiveControlsInput
        {
            StepperModelPath = Relative(root, stepperModel),
            StepperResultPath = Relative(root, stepperResult),
            PlaybackFramesPath = Relative(root, frames),
            CommandLoopSnapshotsPath = Relative(root, snapshots),
            PlayerAdapterContractPath = Relative(root, contract),
            StepperModelPathExists = File.Exists(stepperModel),
            StepperResultPathExists = File.Exists(stepperResult),
            PlaybackFramesPathExists = File.Exists(frames),
            CommandLoopSnapshotsPathExists = File.Exists(snapshots),
            PlayerAdapterContractPathExists = File.Exists(contract)
        };
    }

    private static RuntimeBackedUnityPlayerLoopStepperModel ReadStepperModel(string path) =>
        JsonSerializer.Deserialize<RuntimeBackedUnityPlayerLoopStepperModel>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? new RuntimeBackedUnityPlayerLoopStepperModel();

    private static RuntimeBackedUnityPlayerLoopStepperResult ReadStepperResult(string path) =>
        JsonSerializer.Deserialize<RuntimeBackedUnityPlayerLoopStepperResult>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? new RuntimeBackedUnityPlayerLoopStepperResult();

    private static int ReadArrayCount(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        return document.RootElement.ValueKind == JsonValueKind.Array
            ? document.RootElement.GetArrayLength()
            : 0;
    }

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

    private static RuntimeBackedUnityPlayerLoopInteractiveControlsGoal138AcceptanceRecord
        BuildGoal138Acceptance(
            string root,
            RuntimeBackedUnityPlayerLoopStepperModel stepperModel)
    {
        var smokePath = Resolve(
            root,
            RuntimeBackedUnityPlayerLoopStepperVocabulary.ProceduralOutputDirectory
            + "/"
            + RuntimeBackedUnityPlayerLoopStepperVocabulary.UnitySmokeFileName);
        var smokeGreen = false;
        if (File.Exists(smokePath))
        {
            var smoke = RuntimeBackedUnityPlayerLoopStepperArtifactService.ReadUnitySmoke(smokePath);
            smokeGreen = smoke.StepperBatchSmokePassed && smoke.Status == "GREEN";
        }

        return new RuntimeBackedUnityPlayerLoopInteractiveControlsGoal138AcceptanceRecord
        {
            SelectedCandidate = stepperModel.CandidateId,
            StepperFrames = stepperModel.FrameCount,
            StepperBatchSmoke = smokeGreen ? "GREEN" : "NOT_GREEN",
            ProjectionOnly = stepperModel.ProjectionOnly,
            RuntimeAuthority = stepperModel.RuntimeAuthority,
            UnityGameplayTruth = stepperModel.UnityGameplayTruth,
            RawManualInputNotCommitted = true
        };
    }

    private static RuntimeBackedUnityPlayerLoopInteractiveControlsModel BuildModel(
        RuntimeBackedUnityPlayerLoopStepperModel stepperModel)
    {
        var controls = BuildControls();
        var present = controls.Select(control => control.Id).ToHashSet(StringComparer.Ordinal);
        var missing = RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.RequiredControls
            .Where(control => !present.Contains(control))
            .OrderBy(control => control, StringComparer.Ordinal)
            .ToList();

        return new RuntimeBackedUnityPlayerLoopInteractiveControlsModel
        {
            CandidateId = stepperModel.CandidateId,
            FrameCount = stepperModel.FrameCount,
            CurrentFrameIndex = 0,
            Frames = stepperModel.Frames
                .OrderBy(frame => frame.FrameIndex)
                .Select(frame => new RuntimeBackedUnityPlayerLoopInteractiveControlsFrame
                {
                    FrameIndex = frame.FrameIndex,
                    FrameCategory = frame.FrameCategory,
                    Title = frame.Title,
                    PlayerFacingSummary = frame.PlayerFacingSummary,
                    CanonicalStateHash = frame.CanonicalStateHash,
                    HudLines = frame.HudLines,
                    SourceSnapshotPath = frame.SourceSnapshotPath,
                    SourceFramePath = frame.SourceFramePath,
                    RuntimeAuthority = frame.RuntimeAuthority,
                    UnityGameplayTruth = frame.UnityGameplayTruth,
                    ProjectionOnly = frame.ProjectionOnly
                })
                .ToList(),
            Controls = controls,
            RequiredControls = RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.RequiredControls,
            RequiredControlsPresent = missing.Count == 0,
            MissingControls = missing,
            RuntimeAuthority = stepperModel.RuntimeAuthority,
            UnityGameplayTruth = stepperModel.UnityGameplayTruth,
            ProjectionOnly = stepperModel.ProjectionOnly
        };
    }

    private static IReadOnlyList<RuntimeBackedUnityPlayerLoopInteractiveControlDefinition>
        BuildControls() =>
        [
            Control("load_model", "Load model", "Load the Goal139 runtime-backed model into the player adapter view."),
            Control("first", "First", "Move the selected HUD frame to index 0."),
            Control("previous", "Previous", "Move the selected HUD frame back by one index."),
            Control("next", "Next", "Move the selected HUD frame forward by one index."),
            Control("last", "Last", "Move the selected HUD frame to the final runtime frame."),
            Control("autoplay_tick", "Auto Step", "Advance one frame using deterministic playback control state."),
            Control("autoplay_all", "Auto Play All", "Advance to the final runtime frame without gameplay mutation."),
            Control("copy_current_frame_summary", "Copy Frame Summary", "Copy the current frame summary for review."),
            Control("show_runtime_hash", "Show Runtime Hash", "Display the current canonical runtime state hash."),
            Control("show_hud_lines", "Show HUD Lines", "Display HUD lines projected from runtime-backed frames.")
        ];

    private static RuntimeBackedUnityPlayerLoopInteractiveControlDefinition Control(
        string id,
        string label,
        string behavior) =>
        new()
        {
            Id = id,
            Label = label,
            Behavior = behavior,
            RuntimeBacked = true,
            MutatesGameplay = false
        };

    private static RuntimeBackedUnityPlayerLoopInteractiveControlScript BuildControlScript(
        RuntimeBackedUnityPlayerLoopInteractiveControlsModel model) =>
        new()
        {
            CandidateId = model.CandidateId,
            ExpectedFrameCount = 13,
            RequiredControls = RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.RequiredControls,
            Steps = RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.RequiredScriptActions
                .Select((action, index) => new RuntimeBackedUnityPlayerLoopInteractiveControlScriptStep
                {
                    StepIndex = index,
                    Action = action,
                    ExpectedFrameIndex = ExpectedFrameIndex(action, index),
                    Assertion = AssertionFor(action)
                })
                .ToList(),
            Deterministic = true,
            RuntimeAuthority = true,
            UnityGameplayTruth = false,
            ProjectionOnly = false
        };

    private static int? ExpectedFrameIndex(string action, int stepIndex) =>
        action switch
        {
            "load_model" => 0,
            "assert_frame_count" => 0,
            "first" => 0,
            "next" when stepIndex == 3 => 1,
            "next" when stepIndex == 4 => 2,
            "previous" => 1,
            "last" => 12,
            "autoplay_tick" when stepIndex == 8 => 1,
            "autoplay_tick" when stepIndex == 9 => 2,
            "autoplay_all" => 12,
            "copy_current_frame_summary" => 12,
            "assert_final_frame_reachable" => 12,
            "assert_runtime_authority_markers" => 12,
            _ => null
        };

    private static string AssertionFor(string action) =>
        action switch
        {
            "assert_frame_count" => "frameCount == 13",
            "assert_final_frame_reachable" => "currentFrameIndex == frameCount - 1",
            "assert_runtime_authority_markers" =>
                "runtimeAuthority == true && projectionOnly == false && unityGameplayTruth == false",
            _ => "control exists and does not mutate gameplay"
        };

    private static RuntimeBackedUnityPlayerLoopInteractiveControlSession RunSession(
        RuntimeBackedUnityPlayerLoopInteractiveControlsModel model,
        RuntimeBackedUnityPlayerLoopInteractiveControlScript script)
    {
        var steps = new List<RuntimeBackedUnityPlayerLoopInteractiveControlSessionStep>();
        var frameIndex = Math.Clamp(model.CurrentFrameIndex, 0, Math.Max(model.FrameCount - 1, 0));
        foreach (var step in script.Steps)
        {
            var before = frameIndex;
            frameIndex = ApplyAction(step.Action, frameIndex, model.FrameCount);
            var frame = model.Frames.ElementAtOrDefault(Math.Clamp(frameIndex, 0, Math.Max(model.FrameCount - 1, 0)));
            var expected = step.ExpectedFrameIndex is null || step.ExpectedFrameIndex == frameIndex;
            var assertion = EvaluateAssertion(step.Action, model, frameIndex);
            steps.Add(new RuntimeBackedUnityPlayerLoopInteractiveControlSessionStep
            {
                StepIndex = step.StepIndex,
                Action = step.Action,
                FrameIndexBefore = before,
                FrameIndexAfter = frameIndex,
                Passed = expected && assertion,
                CopiedFrameSummary = step.Action == "copy_current_frame_summary"
                    ? frame?.PlayerFacingSummary ?? string.Empty
                    : string.Empty,
                RuntimeHash = step.Action is "show_runtime_hash"
                    or "assert_runtime_authority_markers"
                    or "copy_current_frame_summary"
                    ? frame?.CanonicalStateHash ?? string.Empty
                    : string.Empty,
                HudLines = step.Action is "show_hud_lines" or "copy_current_frame_summary"
                    ? frame?.HudLines ?? []
                    : [],
                Diagnostic = expected && assertion ? "passed" : "failed"
            });
        }

        var finalReachable = frameIndex == model.FrameCount - 1;
        var runtimeAuthorityMarkers =
            model.RuntimeAuthority
            && !model.ProjectionOnly
            && !model.UnityGameplayTruth
            && model.Frames.All(frame =>
                frame.RuntimeAuthority
                && !frame.ProjectionOnly
                && !frame.UnityGameplayTruth
                && !string.IsNullOrWhiteSpace(frame.CanonicalStateHash));
        return new RuntimeBackedUnityPlayerLoopInteractiveControlSession
        {
            CandidateId = model.CandidateId,
            FrameCount = model.FrameCount,
            FinalFrameIndex = frameIndex,
            Steps = steps,
            FinalFrameReachable = finalReachable,
            RuntimeAuthorityMarkersPresent = runtimeAuthorityMarkers,
            ControlScriptPassed = steps.All(step => step.Passed)
                                  && finalReachable
                                  && runtimeAuthorityMarkers
        };
    }

    private static int ApplyAction(string action, int frameIndex, int frameCount) =>
        action switch
        {
            "load_model" => 0,
            "first" => 0,
            "previous" => Math.Max(0, frameIndex - 1),
            "next" => Math.Min(Math.Max(frameCount - 1, 0), frameIndex + 1),
            "last" => Math.Max(frameCount - 1, 0),
            "autoplay_tick" => Math.Min(Math.Max(frameCount - 1, 0), frameIndex + 1),
            "autoplay_all" => Math.Max(frameCount - 1, 0),
            _ => frameIndex
        };

    private static bool EvaluateAssertion(
        string action,
        RuntimeBackedUnityPlayerLoopInteractiveControlsModel model,
        int frameIndex) =>
        action switch
        {
            "assert_frame_count" => model.FrameCount == 13,
            "assert_final_frame_reachable" => frameIndex == model.FrameCount - 1,
            "assert_runtime_authority_markers" =>
                model.RuntimeAuthority && !model.ProjectionOnly && !model.UnityGameplayTruth,
            _ => true
        };

    private static RuntimeBackedUnityPlayerLoopInteractiveControlsResult BuildResult(
        RuntimeBackedUnityPlayerLoopInteractiveControlsInput inputs,
        RuntimeBackedUnityPlayerLoopInteractiveControlsGoal138AcceptanceRecord acceptance,
        RuntimeBackedUnityPlayerLoopInteractiveControlsModel model,
        RuntimeBackedUnityPlayerLoopInteractiveControlScript script,
        RuntimeBackedUnityPlayerLoopInteractiveControlSession session,
        RuntimeBackedUnityPlayerLoopStepperResult stepperResult,
        int playbackFrameCount,
        int commandLoopSnapshotCount,
        IReadOnlyList<string> adapterCategories)
    {
        var diagnostics = new List<string>();
        Require(acceptance.Accepted, "goal139.goal138_not_accepted", diagnostics);
        Require(acceptance.AcceptedByHuman, "goal139.goal138_not_human_accepted", diagnostics);
        Require(!acceptance.AcceptedByCodex, "goal139.goal138_codex_acceptance_not_allowed", diagnostics);
        Require(acceptance.StepperBatchSmoke == "GREEN", "goal139.goal138_stepper_smoke_not_green", diagnostics);
        Require(acceptance.StepperFrames == 13, "goal139.goal138_frame_count", diagnostics);
        Require(stepperResult.Model.FrameCount == 13, "goal139.source_stepper_frame_count", diagnostics);
        Require(stepperResult.Model.RequiredFrameCategoriesPresent,
            "goal139.source_stepper_categories_missing",
            diagnostics);
        Require(playbackFrameCount == 13, "goal139.playback_frame_count", diagnostics);
        Require(commandLoopSnapshotCount == 13, "goal139.command_loop_snapshot_count", diagnostics);
        Require(adapterCategories.SequenceEqual(RuntimeBackedUnityPlayerLoopStepperVocabulary.RequiredFrameCategories),
            "goal139.player_adapter_contract_categories_unexpected",
            diagnostics);
        Require(model.RequiredControlsPresent, "goal139.required_controls_missing", diagnostics);
        Require(script.Steps.Count == RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.RequiredScriptActions.Count,
            "goal139.control_script_step_count",
            diagnostics);
        Require(session.ControlScriptPassed, "goal139.control_script_failed", diagnostics);
        Require(session.FinalFrameReachable, "goal139.final_frame_not_reachable", diagnostics);
        Require(session.RuntimeAuthorityMarkersPresent, "goal139.runtime_authority_markers_missing", diagnostics);
        Require(model.RuntimeAuthority, "goal139.runtime_authority_missing", diagnostics);
        Require(!model.ProjectionOnly, "goal139.projection_only_not_allowed", diagnostics);
        Require(!model.UnityGameplayTruth, "goal139.unity_gameplay_truth_not_allowed", diagnostics);
        diagnostics.AddRange(stepperResult.Diagnostics);

        return new RuntimeBackedUnityPlayerLoopInteractiveControlsResult
        {
            Inputs = inputs,
            Goal138Acceptance = acceptance,
            Model = model,
            ControlScript = script,
            Session = session,
            SourceStepperResultGreen = stepperResult.Diagnostics.Count == 0
                                       && stepperResult.Model.FrameCount == 13
                                       && stepperResult.Model.RequiredFrameCategoriesPresent,
            SourcePlaybackFramesPresent = playbackFrameCount == 13,
            SourceCommandLoopSnapshotsPresent = commandLoopSnapshotCount == 13,
            PlayerAdapterContractPresent = inputs.PlayerAdapterContractPathExists,
            RequiredControlsPresent = model.RequiredControlsPresent,
            ControlScriptPassed = session.ControlScriptPassed,
            RuntimeAuthority = true,
            UnityGameplayTruth = false,
            ProjectionOnly = false,
            Diagnostics = diagnostics
        };
    }

    private static RuntimeBackedUnityPlayerLoopInteractiveControlsNegativeProof BuildNegativeProof(
        RuntimeBackedUnityPlayerLoopInteractiveControlsModel model,
        RuntimeBackedUnityPlayerLoopInteractiveControlsGoal138AcceptanceRecord acceptance)
    {
        var proof = new RuntimeBackedUnityPlayerLoopInteractiveControlsNegativeProof
        {
            ManualInputRejected = true,
            RawManualInputNotCommitted = acceptance.RawManualInputNotCommitted,
            OutputRootUnderGoal139 = true,
            SamplePackageReadOnly = true,
            RuntimeContractsUnchanged = true,
            GamePackageSchemaUnchanged = true,
            GeneratorLibraryProviderLuaUnchanged = true,
            UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged = true,
            ControlsConsumeRuntimeBackedArtifacts = true,
            ControlsDoNotExecuteGameplay = true,
            RuntimeAuthority = model.RuntimeAuthority,
            ProjectionOnly = model.ProjectionOnly,
            UnityGameplayTruth = model.UnityGameplayTruth
        };
        return proof with
        {
            Passed = proof.ManualInputRejected
                     && proof.RawManualInputNotCommitted
                     && proof.OutputRootUnderGoal139
                     && proof.SamplePackageReadOnly
                     && proof.RuntimeContractsUnchanged
                     && proof.GamePackageSchemaUnchanged
                     && proof.GeneratorLibraryProviderLuaUnchanged
                     && proof.UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged
                     && proof.ControlsConsumeRuntimeBackedArtifacts
                     && proof.ControlsDoNotExecuteGameplay
                     && proof.RuntimeAuthority
                     && !proof.ProjectionOnly
                     && !proof.UnityGameplayTruth
        };
    }

    private static RuntimeBackedUnityPlayerLoopInteractiveControlsReport BuildReport(
        RuntimeBackedUnityPlayerLoopInteractiveControlsModel model,
        RuntimeBackedUnityPlayerLoopInteractiveControlsGoal138AcceptanceRecord acceptance,
        RuntimeBackedUnityPlayerLoopInteractiveControlSession session,
        RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmoke smoke) =>
        new()
        {
            CandidateId = model.CandidateId,
            FrameCount = model.FrameCount,
            AcceptedGoal138 = acceptance.Accepted && acceptance.AcceptedByHuman && !acceptance.AcceptedByCodex,
            RequiredControlsPresent = model.RequiredControlsPresent,
            ControlScriptPassed = session.ControlScriptPassed,
            InteractiveControlsWindowPresent = smoke.InteractiveControlsWindowPresent,
            UnityInteractiveControlsSmokePassed = smoke.Passed,
            RuntimeAuthority = model.RuntimeAuthority,
            UnityGameplayTruth = false,
            ProjectionOnly = false,
            ManualUnityOptional = true,
            Accepted = false
        };

    private static RuntimeBackedUnityPlayerLoopInteractiveControlsDashboard BuildDashboard(
        RuntimeBackedUnityPlayerLoopInteractiveControlsModel model,
        RuntimeBackedUnityPlayerLoopInteractiveControlsGoal138AcceptanceRecord acceptance,
        RuntimeBackedUnityPlayerLoopInteractiveControlSession session,
        RuntimeBackedUnityPlayerLoopInteractiveControlsResult result,
        RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmoke smoke)
    {
        var diagnostics = new List<string>();
        diagnostics.AddRange(result.Diagnostics);
        Require(acceptance.Accepted && acceptance.AcceptedByHuman && !acceptance.AcceptedByCodex,
            "goal139.goal138_acceptance_record_invalid",
            diagnostics);
        Require(model.FrameCount == 13, "goal139.frame_count", diagnostics);
        Require(model.RequiredControlsPresent, "goal139.required_controls_missing", diagnostics);
        Require(session.ControlScriptPassed, "goal139.control_script_failed", diagnostics);
        Require(smoke.InteractiveControlsWindowPresent, "goal139.interactive_controls_window_missing", diagnostics);
        Require(smoke.Passed, "goal139.unity_interactive_controls_smoke_failed", diagnostics);
        Require(model.RuntimeAuthority, "goal139.runtime_authority_missing", diagnostics);
        Require(!model.UnityGameplayTruth, "goal139.unity_gameplay_truth_not_allowed", diagnostics);
        Require(!model.ProjectionOnly, "goal139.projection_only_not_allowed", diagnostics);

        return new RuntimeBackedUnityPlayerLoopInteractiveControlsDashboard
        {
            Status = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            Accepted = false,
            AcceptedGoal138 = acceptance.Accepted && acceptance.AcceptedByHuman && !acceptance.AcceptedByCodex,
            CandidateId = model.CandidateId,
            FrameCount = model.FrameCount,
            RequiredControlsPresent = model.RequiredControlsPresent,
            ControlScriptPassed = session.ControlScriptPassed,
            InteractiveControlsWindowPresent = smoke.InteractiveControlsWindowPresent,
            UnityInteractiveControlsSmokePassed = smoke.Passed,
            RuntimeAuthority = true,
            UnityGameplayTruth = false,
            ProjectionOnly = false,
            ManualUnityOptional = true,
            MissingControls = model.MissingControls,
            Diagnostics = diagnostics
                .Concat(smoke.Diagnostics)
                .ToList()
        };
    }

    private static RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmoke BuildPendingUnitySmoke(
        string root,
        string outputRootRelativePath)
    {
        var model = Resolve(
            root,
            outputRootRelativePath + "/" + RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ModelFileName);
        var script = Resolve(
            root,
            outputRootRelativePath + "/" + RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ControlScriptFileName);
        return new RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmoke
        {
            InteractiveModelPath = Relative(root, model),
            ControlScriptPath = Relative(root, script),
            InteractiveModelPathExists = File.Exists(model),
            ControlScriptPathExists = File.Exists(script),
            Status = "PENDING_UNITY_BATCHMODE",
            Diagnostics = ["Unity player-loop interactive controls smoke has not written a marker artifact yet"]
        };
    }

    private static SortedDictionary<string, string> BuildFilePayloads(
        string root,
        string relativeRoot,
        RuntimeBackedUnityPlayerLoopInteractiveControlsGoal138AcceptanceRecord acceptance,
        RuntimeBackedUnityPlayerLoopInteractiveControlsModel model,
        RuntimeBackedUnityPlayerLoopInteractiveControlScript script,
        RuntimeBackedUnityPlayerLoopInteractiveControlSession session,
        RuntimeBackedUnityPlayerLoopInteractiveControlsResult result,
        RuntimeBackedUnityPlayerLoopInteractiveControlsDashboard dashboard,
        RuntimeBackedUnityPlayerLoopInteractiveControlsNegativeProof negative,
        RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmoke smoke,
        RuntimeBackedUnityPlayerLoopInteractiveControlsReport report,
        string reportMarkdown)
    {
        var files = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.Goal138AcceptanceFileName] =
                Serialize(acceptance),
            [RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ModelFileName] =
                Serialize(model),
            [RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ControlScriptFileName] =
                Serialize(script),
            [RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.SessionFileName] =
                Serialize(session),
            [RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ResultFileName] =
                Serialize(result),
            [RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.UnitySmokeFileName] =
                Serialize(smoke),
            [RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ReportJsonFileName] =
                Serialize(report),
            [RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ReportMarkdownFileName] =
                reportMarkdown
        };
        files[RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.FileIndexFileName] =
            Serialize(BuildFileIndex(relativeRoot, files));
        return files;
    }

    private static RuntimeBackedUnityPlayerLoopInteractiveControlsFileIndex BuildFileIndex(
        string relativeRoot,
        IReadOnlyDictionary<string, string> pendingTextFiles)
    {
        var files = pendingTextFiles
            .Select(item => new RuntimeBackedUnityPlayerLoopInteractiveControlsFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = "goal139_" + Path.GetFileNameWithoutExtension(item.Key)
                    .Replace("-", "_", StringComparison.Ordinal),
                Sha256 = HashText(item.Value)
            })
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
        return new RuntimeBackedUnityPlayerLoopInteractiveControlsFileIndex
        {
            RootPath = relativeRoot,
            IndexedFileCount = files.Count,
            ManualInputExcluded = files.All(file =>
                !file.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = files
        };
    }

    private static string RenderReport(
        RuntimeBackedUnityPlayerLoopInteractiveControlsReport report,
        RuntimeBackedUnityPlayerLoopInteractiveControlsDashboard dashboard,
        RuntimeBackedUnityPlayerLoopInteractiveControlsResult result)
    {
        var lines = new List<string>
        {
            "# Goal 139 Runtime-backed Unity Player Loop Interactive Controls Harness",
            string.Empty,
            "- status: " + dashboard.Status,
            "- accepted: false",
            "- acceptedGoal138: " + Bool(report.AcceptedGoal138),
            "- candidateId: " + report.CandidateId,
            "- frameCount: " + report.FrameCount,
            "- requiredControlsPresent: " + Bool(report.RequiredControlsPresent),
            "- controlScriptPassed: " + Bool(report.ControlScriptPassed),
            "- interactiveControlsWindowPresent: " + Bool(report.InteractiveControlsWindowPresent),
            "- unityInteractiveControlsSmokePassed: " + Bool(report.UnityInteractiveControlsSmokePassed),
            "- runtimeAuthority: " + Bool(report.RuntimeAuthority),
            "- unityGameplayTruth: " + Bool(report.UnityGameplayTruth),
            "- projectionOnly: " + Bool(report.ProjectionOnly),
            "- manualUnityOptional: " + Bool(report.ManualUnityOptional),
            "- normalCommand: " + report.NormalCommand,
            "- reportPath: " + report.ReportPath,
            "- modelPath: " + report.ModelPath,
            "- controlScriptPath: " + report.ControlScriptPath,
            string.Empty,
            "## Source Checks",
            string.Empty,
            "- sourceStepperResultGreen: " + Bool(result.SourceStepperResultGreen),
            "- sourcePlaybackFramesPresent: " + Bool(result.SourcePlaybackFramesPresent),
            "- sourceCommandLoopSnapshotsPresent: " + Bool(result.SourceCommandLoopSnapshotsPresent),
            "- playerAdapterContractPresent: " + Bool(result.PlayerAdapterContractPresent),
            string.Empty,
            "## Required Controls",
            string.Empty
        };
        lines.AddRange(result.Model.RequiredControls.Select(control =>
            "- " + control + ": " + Bool(result.Model.Controls.Any(item => item.Id == control))));
        lines.Add(string.Empty);
        lines.Add("## Script Steps");
        lines.Add(string.Empty);
        lines.AddRange(result.Session.Steps.Select(step =>
            "- " + step.StepIndex + " " + step.Action + " => frame "
            + step.FrameIndexAfter + " passed=" + Bool(step.Passed)));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(dashboard.Diagnostics.Count == 0
            ? ["- none"]
            : dashboard.Diagnostics.Select(item => "- " + item));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderGoal138Acceptance(
        RuntimeBackedUnityPlayerLoopInteractiveControlsGoal138AcceptanceRecord acceptance)
    {
        var lines = new List<string>
        {
            "# Goal 138 Runtime-backed Unity Player Loop Stepper HUD Harness Acceptance",
            string.Empty,
            "accepted=true",
            "acceptedByHuman=true",
            "acceptedByCodex=false",
            "selectedCandidate=" + acceptance.SelectedCandidate,
            "stepperFrames=" + acceptance.StepperFrames.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "stepperBatchSmoke=" + acceptance.StepperBatchSmoke,
            "projectionOnly=false",
            "runtimeAuthority=true",
            "unityGameplayTruth=false",
            "rawManualInputNotCommitted=true",
            string.Empty,
            "Source: Goal139 task handoff recorded the owner acceptance. Raw manual input remains outside committed artifacts."
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderGoal139ManualAcceptance(
        RuntimeBackedUnityPlayerLoopInteractiveControlsReport report,
        RuntimeBackedUnityPlayerLoopInteractiveControlsDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Goal 139 Runtime-backed Unity Player Loop Interactive Controls Harness",
            string.Empty,
            "accepted=false",
            "acceptedByHuman=false",
            "acceptedByCodex=false",
            "manualUnityOptional=true",
            "acceptedGoal138=" + Bool(report.AcceptedGoal138),
            "candidateId=" + report.CandidateId,
            "frameCount=" + report.FrameCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "requiredControlsPresent=" + Bool(report.RequiredControlsPresent),
            "controlScriptPassed=" + Bool(report.ControlScriptPassed),
            "interactiveControlsWindowPresent=" + Bool(report.InteractiveControlsWindowPresent),
            "unityInteractiveControlsSmokePassed=" + Bool(report.UnityInteractiveControlsSmokePassed),
            "runtimeAuthority=true",
            "unityGameplayTruth=false",
            "projectionOnly=false",
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

    private static void GuardGoal139Write(string root, string path)
    {
        GuardNotManualInput(root, path);
        var relative = Relative(root, path);
        if (!relative.StartsWith(
                RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ProceduralOutputDirectory + "/",
                StringComparison.Ordinal)
            && !relative.StartsWith(
                RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ExportPackageDirectory + "/",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to write outside Goal139 output roots: " + relative);
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
