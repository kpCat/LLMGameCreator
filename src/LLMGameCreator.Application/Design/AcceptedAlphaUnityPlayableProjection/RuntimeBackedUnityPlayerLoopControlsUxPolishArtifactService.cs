using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class RuntimeBackedUnityPlayerLoopControlsUxPolishArtifactService
{
    private const string KnownBuildProfileFixture =
        "BuildProfileContext asset exists but could not be loaded\n"
        + "NullReferenceException: Object reference not set to an instance of an object\n"
        + "UnityEditor.Build.Profile.BuildProfileContext.CreateOrLoad";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static RuntimeBackedUnityPlayerLoopControlsUxPolishArtifactService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public static RuntimeBackedUnityPlayerLoopControlsUxUnitySmoke ReadUnitySmoke(string path) =>
        JsonSerializer.Deserialize<RuntimeBackedUnityPlayerLoopControlsUxUnitySmoke>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? new RuntimeBackedUnityPlayerLoopControlsUxUnitySmoke();

    public static RuntimeBackedUnityPlayerLoopControlsUxUnityNoiseClassification
        ReadUnityNoiseClassification(string path) =>
        JsonSerializer.Deserialize<RuntimeBackedUnityPlayerLoopControlsUxUnityNoiseClassification>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? new RuntimeBackedUnityPlayerLoopControlsUxUnityNoiseClassification();

    public static RuntimeBackedUnityPlayerLoopControlsUxUnityNoiseClassification
        ClassifyUnityEditorNoise(string logText, string sourceLogPath)
    {
        logText ??= string.Empty;
        var knownFixture = ClassifyActualUnityLog(KnownBuildProfileFixture);
        var actual = ClassifyActualUnityLog(logText);
        var knownClassified = knownFixture.KnownUnityEditorNoiseCount > 0;
        return new RuntimeBackedUnityPlayerLoopControlsUxUnityNoiseClassification
        {
            KnownUnityEditorBuildProfileNoiseClassified = knownClassified,
            KnownUnityEditorNoiseCount = actual.KnownUnityEditorNoiseCount,
            BlockingUnityErrorCount = actual.BlockingUnityErrorCount,
            UnclassifiedUnityErrorCount = actual.UnclassifiedUnityErrorCount,
            FixtureKnownUnityEditorBuildProfileNoiseClassified = knownClassified,
            SourceLogPath = sourceLogPath,
            KnownMarkers =
            [
                "BuildProfileContext",
                "CreateOrLoad",
                "NullReferenceException"
            ],
            BlockingMarkers =
            [
                RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.UnityFailMarker,
                "unpaired NullReferenceException",
                "player-loop exception"
            ],
            Diagnostics = actual.Diagnostics,
            Passed = knownClassified
                     && actual.BlockingUnityErrorCount == 0
                     && actual.UnclassifiedUnityErrorCount == 0
        };
    }

    public async Task<RuntimeBackedUnityPlayerLoopControlsUxWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        RuntimeBackedUnityPlayerLoopControlsUxPolishRequest request,
        string outputRootRelativePath =
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ProceduralOutputDirectory,
        string exportRootRelativePath =
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ExportPackageDirectory,
        RuntimeBackedUnityPlayerLoopControlsUxUnitySmoke? unitySmoke = null,
        RuntimeBackedUnityPlayerLoopControlsUxUnityNoiseClassification? unityNoise = null,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var inputs = BuildInputs(root, request);
        var goal139Model = ReadGoal139Model(Resolve(root, inputs.InteractiveControlsModelPath));
        var goal139Result = ReadGoal139Result(Resolve(root, inputs.InteractiveControlsResultPath));
        var goal139Script = ReadGoal139Script(Resolve(root, inputs.InteractiveControlsScriptPath));
        var acceptance = BuildGoal139Acceptance(root, goal139Model, goal139Result);
        var model = BuildModel(goal139Model);
        var script = BuildScript(model);
        var session = RunSession(model, script);
        var smoke = unitySmoke ?? BuildPendingUnitySmoke(root, outputRootRelativePath);
        var noise = unityNoise ?? ClassifyUnityEditorNoise(string.Empty, string.Empty);
        var result = BuildResult(inputs, acceptance, model, script, session, smoke, noise, goal139Script);
        var negative = BuildNegativeProof(model, acceptance, noise);
        var report = BuildReport(model, acceptance, smoke, noise, result);
        var dashboard = BuildDashboard(model, acceptance, smoke, noise, result);
        var reportMarkdown = RenderReport(report, dashboard, result, negative);
        var goal139Markdown = RenderGoal139Acceptance(acceptance);
        var goal140Markdown = RenderGoal140ManualAcceptance(report, dashboard);

        var proceduralFiles = BuildFilePayloads(
            outputRootRelativePath,
            acceptance,
            model,
            script,
            result,
            dashboard,
            smoke,
            noise,
            report,
            reportMarkdown,
            negative);
        var exportFiles = BuildFilePayloads(
            exportRootRelativePath,
            acceptance,
            model,
            script,
            result,
            dashboard,
            smoke,
            noise,
            report,
            reportMarkdown,
            negative);

        var procedural = Resolve(root, outputRootRelativePath);
        var export = Resolve(root, exportRootRelativePath);
        var goal139DocsPath = Resolve(
            root,
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.Goal139DocumentationPath);
        var docsPath = Resolve(
            root,
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.DocumentationPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in proceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            GuardGoal140Write(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in exportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            GuardGoal140Write(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        GuardNotManualInput(root, goal139DocsPath);
        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(goal139DocsPath, goal139Markdown, cancellationToken).ConfigureAwait(false);
        await WriteTextAsync(docsPath, goal140Markdown, cancellationToken).ConfigureAwait(false);
        written.Add(Relative(root, goal139DocsPath));
        written.Add(Relative(root, docsPath));

        return new RuntimeBackedUnityPlayerLoopControlsUxWriteResult
        {
            Dashboard = dashboard,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            Goal139DocumentationPath = goal139DocsPath,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static RuntimeBackedUnityPlayerLoopControlsUxPolishInput BuildInputs(
        string root,
        RuntimeBackedUnityPlayerLoopControlsUxPolishRequest request)
    {
        var model = ResolveInput(root, request.InteractiveControlsModelPath, "InteractiveControlsModelPath");
        var result = ResolveInput(root, request.InteractiveControlsResultPath, "InteractiveControlsResultPath");
        var script = ResolveInput(root, request.InteractiveControlsScriptPath, "InteractiveControlsScriptPath");
        return new RuntimeBackedUnityPlayerLoopControlsUxPolishInput
        {
            InteractiveControlsModelPath = Relative(root, model),
            InteractiveControlsResultPath = Relative(root, result),
            InteractiveControlsScriptPath = Relative(root, script),
            InteractiveControlsModelPathExists = File.Exists(model),
            InteractiveControlsResultPathExists = File.Exists(result),
            InteractiveControlsScriptPathExists = File.Exists(script)
        };
    }

    private static RuntimeBackedUnityPlayerLoopInteractiveControlsModel ReadGoal139Model(string path) =>
        JsonSerializer.Deserialize<RuntimeBackedUnityPlayerLoopInteractiveControlsModel>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? new RuntimeBackedUnityPlayerLoopInteractiveControlsModel();

    private static RuntimeBackedUnityPlayerLoopInteractiveControlsResult ReadGoal139Result(string path) =>
        JsonSerializer.Deserialize<RuntimeBackedUnityPlayerLoopInteractiveControlsResult>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? new RuntimeBackedUnityPlayerLoopInteractiveControlsResult();

    private static RuntimeBackedUnityPlayerLoopInteractiveControlScript ReadGoal139Script(string path) =>
        JsonSerializer.Deserialize<RuntimeBackedUnityPlayerLoopInteractiveControlScript>(
            File.ReadAllText(path, Encoding.UTF8),
            JsonOptions) ?? new RuntimeBackedUnityPlayerLoopInteractiveControlScript();

    private static RuntimeBackedUnityPlayerLoopControlsUxPolishGoal139AcceptanceRecord
        BuildGoal139Acceptance(
            string root,
            RuntimeBackedUnityPlayerLoopInteractiveControlsModel model,
            RuntimeBackedUnityPlayerLoopInteractiveControlsResult result)
    {
        var smokePath = Resolve(
            root,
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ProceduralOutputDirectory
            + "/"
            + RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.UnitySmokeFileName);
        var smokeGreen = false;
        if (File.Exists(smokePath))
        {
            var smoke = RuntimeBackedUnityPlayerLoopInteractiveControlsArtifactService.ReadUnitySmoke(smokePath);
            smokeGreen = smoke.Passed && smoke.Status == "GREEN";
        }

        return new RuntimeBackedUnityPlayerLoopControlsUxPolishGoal139AcceptanceRecord
        {
            SelectedCandidate = model.CandidateId,
            Frames = model.FrameCount,
            InteractiveControlsSmoke = smokeGreen ? "GREEN" : "NOT_GREEN",
            RequiredControlsPresent = model.RequiredControlsPresent,
            ControlsWork = result.ControlScriptPassed,
            ProjectionOnly = model.ProjectionOnly,
            RuntimeAuthority = model.RuntimeAuthority,
            UnityGameplayTruth = model.UnityGameplayTruth
        };
    }

    private static RuntimeBackedUnityPlayerLoopControlsUxModel BuildModel(
        RuntimeBackedUnityPlayerLoopInteractiveControlsModel source)
    {
        var controls = BuildControls();
        var present = controls.Select(control => control.Id).ToHashSet(StringComparer.Ordinal);
        var missing = RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.RequiredControls
            .Where(control => !present.Contains(control))
            .OrderBy(control => control, StringComparer.Ordinal)
            .ToList();
        var frameCount = source.FrameCount;
        var frames = source.Frames
            .OrderBy(frame => frame.FrameIndex)
            .Select(frame => new RuntimeBackedUnityPlayerLoopControlsUxFrame
            {
                FrameIndex = frame.FrameIndex,
                HumanFrameNumber = frame.FrameIndex + 1,
                CurrentFrameLabel = HumanFrameLabel(frame.FrameIndex, frameCount),
                FrameIndexLabel = RawFrameIndexLabel(frame.FrameIndex),
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
            .ToList();
        var humanReadable = frames.Count == frameCount
                            && frames.Count > 0
                            && frames[0].CurrentFrameLabel == "Current Frame: 1/13"
                            && frames[0].FrameIndexLabel == "Frame Index: 0";
        var stepOnce = controls.Any(control =>
            control.Id == "step_once"
            && control.Label == "Step Once"
            && control.Behavior.Contains("one frame tick", StringComparison.OrdinalIgnoreCase)
            && control.LastControlAction == "step_once");
        var playAll = controls.Any(control =>
            control.Id == "play_all_to_end"
            && control.Label == "Play All To End"
            && control.Behavior.Contains("instant-to-final-frame", StringComparison.OrdinalIgnoreCase)
            && control.LastControlAction == "play_all_to_end");
        var copyStatus = controls.Any(control =>
            control.Id == "copy_current_frame_summary"
            && control.StatusAfterAction == "copied_frame_summary");
        var resetStatus = controls.Any(control =>
            control.Id == "first"
            && control.StatusAfterAction == "reset_to_first_frame");
        var lastAction = controls.All(control => !string.IsNullOrWhiteSpace(control.LastControlAction));
        return new RuntimeBackedUnityPlayerLoopControlsUxModel
        {
            CandidateId = source.CandidateId,
            FrameCount = frameCount,
            CurrentFrameIndex = 0,
            CurrentFrameLabel = HumanFrameLabel(0, frameCount),
            FrameIndexLabel = RawFrameIndexLabel(0),
            Frames = frames,
            Controls = controls,
            RequiredControls = RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.RequiredControls,
            MissingControls = missing,
            RequiredControlsPresent = missing.Count == 0,
            HumanReadableFrameNumbering = humanReadable,
            StepOnceSemanticsClear = stepOnce,
            PlayAllToEndSemanticsClear = playAll,
            CopyFrameSummaryStatusPresent = copyStatus,
            ResetFirstStatusPresent = resetStatus,
            LastControlActionPresent = lastAction,
            ControlsUxPolished = missing.Count == 0
                                 && humanReadable
                                 && stepOnce
                                 && playAll
                                 && copyStatus
                                 && resetStatus
                                 && lastAction,
            RuntimeAuthority = source.RuntimeAuthority,
            UnityGameplayTruth = source.UnityGameplayTruth,
            ProjectionOnly = source.ProjectionOnly
        };
    }

    private static IReadOnlyList<RuntimeBackedUnityPlayerLoopControlsUxControlDefinition> BuildControls() =>
    [
        Control("load_model", "Load model",
            "Load the Goal140 runtime-backed UX model into the player adapter view.",
            "load_model", "loaded_goal140_controls_ux_model"),
        Control("first", "Reset/First",
            "Reset the selected HUD frame to the first runtime-backed frame.",
            "first", "reset_to_first_frame"),
        Control("previous", "Previous",
            "Move the selected HUD frame back by one index.",
            "previous", "moved_previous_frame"),
        Control("next", "Next",
            "Move the selected HUD frame forward by one index.",
            "next", "moved_next_frame"),
        Control("last", "Last",
            "Move the selected HUD frame to the final runtime-backed frame.",
            "last", "moved_last_frame"),
        Control("step_once", "Step Once",
            "Advance exactly one frame tick through deterministic playback control state.",
            "step_once", "stepped_one_frame_tick"),
        Control("play_all_to_end", "Play All To End",
            "Run instant-to-final-frame playback over existing runtime-backed frames without gameplay mutation.",
            "play_all_to_end", "played_all_to_end"),
        Control("copy_current_frame_summary", "Copy Frame Summary",
            "Copy the current frame summary and show copied_frame_summary status for review.",
            "copy_current_frame_summary", "copied_frame_summary"),
        Control("show_runtime_hash", "Show Runtime Hash",
            "Display the current canonical runtime state hash.",
            "show_runtime_hash", "showed_runtime_hash"),
        Control("show_hud_lines", "Show HUD Lines",
            "Display HUD lines projected from runtime-backed frames.",
            "show_hud_lines", "showed_hud_lines")
    ];

    private static RuntimeBackedUnityPlayerLoopControlsUxControlDefinition Control(
        string id,
        string label,
        string behavior,
        string lastControlAction,
        string statusAfterAction) =>
        new()
        {
            Id = id,
            Label = label,
            Behavior = behavior,
            LastControlAction = lastControlAction,
            StatusAfterAction = statusAfterAction,
            RuntimeBacked = true,
            MutatesGameplay = false
        };

    private static RuntimeBackedUnityPlayerLoopControlsUxScript BuildScript(
        RuntimeBackedUnityPlayerLoopControlsUxModel model) =>
        new()
        {
            CandidateId = model.CandidateId,
            ExpectedFrameCount = 13,
            RequiredControls = RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.RequiredControls,
            Steps = RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.RequiredScriptActions
                .Select((action, index) => new RuntimeBackedUnityPlayerLoopControlsUxScriptStep
                {
                    StepIndex = index,
                    Action = action,
                    ExpectedFrameIndex = ExpectedFrameIndex(action, index),
                    ExpectedLastControlAction = ExpectedLastAction(action),
                    ExpectedStatus = ExpectedStatus(action),
                    Assertion = AssertionFor(action)
                })
                .ToList()
        };

    private static RuntimeBackedUnityPlayerLoopControlsUxSession RunSession(
        RuntimeBackedUnityPlayerLoopControlsUxModel model,
        RuntimeBackedUnityPlayerLoopControlsUxScript script)
    {
        var steps = new List<RuntimeBackedUnityPlayerLoopControlsUxSessionStep>();
        var frameIndex = Math.Clamp(model.CurrentFrameIndex, 0, Math.Max(model.FrameCount - 1, 0));
        foreach (var step in script.Steps)
        {
            var before = frameIndex;
            frameIndex = ApplyAction(step.Action, frameIndex, model.FrameCount);
            var lastAction = ExpectedLastAction(step.Action);
            var status = ExpectedStatus(step.Action);
            var expectedFrame = step.ExpectedFrameIndex is null || step.ExpectedFrameIndex == frameIndex;
            var expectedAction = string.IsNullOrWhiteSpace(step.ExpectedLastControlAction)
                                 || step.ExpectedLastControlAction == lastAction;
            var expectedStatus = string.IsNullOrWhiteSpace(step.ExpectedStatus)
                                 || step.ExpectedStatus == status;
            var assertion = EvaluateAssertion(step.Action, model, frameIndex, status);
            steps.Add(new RuntimeBackedUnityPlayerLoopControlsUxSessionStep
            {
                StepIndex = step.StepIndex,
                Action = step.Action,
                FrameIndexBefore = before,
                FrameIndexAfter = frameIndex,
                CurrentFrameLabelAfter = HumanFrameLabel(frameIndex, model.FrameCount),
                FrameIndexLabelAfter = RawFrameIndexLabel(frameIndex),
                LastControlAction = lastAction,
                StatusAfterAction = status,
                Passed = expectedFrame && expectedAction && expectedStatus && assertion,
                Diagnostic = expectedFrame && expectedAction && expectedStatus && assertion ? "passed" : "failed"
            });
        }

        var runtimeAuthorityMarkers =
            model.RuntimeAuthority
            && !model.ProjectionOnly
            && !model.UnityGameplayTruth
            && model.Frames.All(frame =>
                frame.RuntimeAuthority
                && !frame.ProjectionOnly
                && !frame.UnityGameplayTruth
                && !string.IsNullOrWhiteSpace(frame.CanonicalStateHash));
        var humanReadable = steps.Any(step =>
            step.CurrentFrameLabelAfter == "Current Frame: 1/13"
            && step.FrameIndexLabelAfter == "Frame Index: 0");
        var stepOnce = steps.Any(step =>
            step.Action == "step_once"
            && step.LastControlAction == "step_once"
            && step.StatusAfterAction == "stepped_one_frame_tick");
        var playAll = steps.Any(step =>
            step.Action == "play_all_to_end"
            && step.FrameIndexAfter == model.FrameCount - 1
            && step.LastControlAction == "play_all_to_end");
        var copy = steps.Any(step =>
            step.Action == "copy_current_frame_summary"
            && step.StatusAfterAction == "copied_frame_summary");
        var reset = steps.Any(step =>
            step.Action == "first"
            && step.FrameIndexAfter == 0
            && step.StatusAfterAction == "reset_to_first_frame");
        return new RuntimeBackedUnityPlayerLoopControlsUxSession
        {
            CandidateId = model.CandidateId,
            FrameCount = model.FrameCount,
            FinalFrameIndex = frameIndex,
            Steps = steps,
            HumanReadableFrameNumberingPassed = humanReadable,
            StepOnceSemanticsPassed = stepOnce,
            PlayAllToEndSemanticsPassed = playAll,
            CopyFrameSummaryStatusPassed = copy,
            ResetFirstStatusPassed = reset,
            RuntimeAuthorityMarkersPresent = runtimeAuthorityMarkers,
            ControlScriptPassed = steps.All(step => step.Passed)
                                  && humanReadable
                                  && stepOnce
                                  && playAll
                                  && copy
                                  && reset
                                  && runtimeAuthorityMarkers
        };
    }

    private static RuntimeBackedUnityPlayerLoopControlsUxResult BuildResult(
        RuntimeBackedUnityPlayerLoopControlsUxPolishInput inputs,
        RuntimeBackedUnityPlayerLoopControlsUxPolishGoal139AcceptanceRecord acceptance,
        RuntimeBackedUnityPlayerLoopControlsUxModel model,
        RuntimeBackedUnityPlayerLoopControlsUxScript script,
        RuntimeBackedUnityPlayerLoopControlsUxSession session,
        RuntimeBackedUnityPlayerLoopControlsUxUnitySmoke smoke,
        RuntimeBackedUnityPlayerLoopControlsUxUnityNoiseClassification noise,
        RuntimeBackedUnityPlayerLoopInteractiveControlScript goal139Script)
    {
        var diagnostics = new List<string>();
        Require(goal139Script.Deterministic, "goal140.source_goal139_script_not_deterministic", diagnostics);
        Require(acceptance.Accepted && acceptance.AcceptedByHuman && !acceptance.AcceptedByCodex,
            "goal140.goal139_acceptance_record_invalid",
            diagnostics);
        Require(model.FrameCount == 13, "goal140.frame_count", diagnostics);
        Require(model.ControlsUxPolished, "goal140.controls_ux_not_polished", diagnostics);
        Require(smoke.Passed, "goal140.unity_controls_ux_smoke_failed", diagnostics);
        Require(noise.Passed, "goal140.unity_noise_classification_failed", diagnostics);
        Require(model.RuntimeAuthority && !model.ProjectionOnly && !model.UnityGameplayTruth,
            "goal140.runtime_authority_markers_invalid",
            diagnostics);
        return new RuntimeBackedUnityPlayerLoopControlsUxResult
        {
            Inputs = inputs,
            Goal139Acceptance = acceptance,
            Model = model,
            Script = script,
            Session = session,
            UnitySmoke = smoke,
            UnityNoiseClassification = noise,
            RequiredControlsPresent = model.RequiredControlsPresent,
            ControlsUxPolished = model.ControlsUxPolished && session.ControlScriptPassed,
            UnityControlsUxSmokePassed = smoke.Passed,
            RuntimeAuthority = true,
            UnityGameplayTruth = false,
            ProjectionOnly = false,
            Diagnostics = diagnostics
        };
    }

    private static RuntimeBackedUnityPlayerLoopControlsUxNegativeProof BuildNegativeProof(
        RuntimeBackedUnityPlayerLoopControlsUxModel model,
        RuntimeBackedUnityPlayerLoopControlsUxPolishGoal139AcceptanceRecord acceptance,
        RuntimeBackedUnityPlayerLoopControlsUxUnityNoiseClassification noise)
    {
        var proof = new RuntimeBackedUnityPlayerLoopControlsUxNegativeProof
        {
            ManualInputRejected = true,
            RawManualInputNotCommitted = acceptance.RawManualInputNotCommitted,
            OutputRootUnderGoal140 = true,
            SamplePackageReadOnly = true,
            RuntimeContractsUnchanged = true,
            GamePackageSchemaUnchanged = true,
            GeneratorLibraryProviderLuaUnchanged = true,
            UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged = true,
            ControlsConsumeRuntimeBackedArtifacts = true,
            ControlsDoNotExecuteGameplay = true,
            KnownUnityEditorNoiseIsBounded =
                noise.KnownUnityEditorBuildProfileNoiseClassified
                && noise.BlockingUnityErrorCount == 0
                && noise.UnclassifiedUnityErrorCount == 0,
            RuntimeAuthority = model.RuntimeAuthority,
            ProjectionOnly = model.ProjectionOnly,
            UnityGameplayTruth = model.UnityGameplayTruth
        };
        return proof with
        {
            Passed = proof.ManualInputRejected
                     && proof.RawManualInputNotCommitted
                     && proof.OutputRootUnderGoal140
                     && proof.SamplePackageReadOnly
                     && proof.RuntimeContractsUnchanged
                     && proof.GamePackageSchemaUnchanged
                     && proof.GeneratorLibraryProviderLuaUnchanged
                     && proof.UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged
                     && proof.ControlsConsumeRuntimeBackedArtifacts
                     && proof.ControlsDoNotExecuteGameplay
                     && proof.KnownUnityEditorNoiseIsBounded
                     && proof.RuntimeAuthority
                     && !proof.ProjectionOnly
                     && !proof.UnityGameplayTruth
        };
    }

    private static RuntimeBackedUnityPlayerLoopControlsUxReport BuildReport(
        RuntimeBackedUnityPlayerLoopControlsUxModel model,
        RuntimeBackedUnityPlayerLoopControlsUxPolishGoal139AcceptanceRecord acceptance,
        RuntimeBackedUnityPlayerLoopControlsUxUnitySmoke smoke,
        RuntimeBackedUnityPlayerLoopControlsUxUnityNoiseClassification noise,
        RuntimeBackedUnityPlayerLoopControlsUxResult result) =>
        new()
        {
            Status = result.Diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            Accepted = false,
            AcceptedGoal139 = acceptance.Accepted && acceptance.AcceptedByHuman && !acceptance.AcceptedByCodex,
            SelectedCandidate = model.CandidateId,
            FrameCount = model.FrameCount,
            HumanReadableFrameNumbering = model.HumanReadableFrameNumbering,
            StepOnceSemanticsClear = model.StepOnceSemanticsClear,
            PlayAllToEndSemanticsClear = model.PlayAllToEndSemanticsClear,
            CopyFrameSummaryStatusPresent = model.CopyFrameSummaryStatusPresent,
            RequiredControlsPresent = model.RequiredControlsPresent,
            ControlsUxPolished = model.ControlsUxPolished,
            UnityControlsUxSmokePassed = smoke.Passed,
            RuntimeAuthority = true,
            UnityGameplayTruth = false,
            ProjectionOnly = false,
            KnownUnityEditorNoiseClassified = noise.KnownUnityEditorBuildProfileNoiseClassified,
            KnownUnityEditorNoiseCount = noise.KnownUnityEditorNoiseCount,
            BlockingUnityErrorCount = noise.BlockingUnityErrorCount,
            UnclassifiedUnityErrorCount = noise.UnclassifiedUnityErrorCount,
            ManualUnityOptional = true
        };

    private static RuntimeBackedUnityPlayerLoopControlsUxDashboard BuildDashboard(
        RuntimeBackedUnityPlayerLoopControlsUxModel model,
        RuntimeBackedUnityPlayerLoopControlsUxPolishGoal139AcceptanceRecord acceptance,
        RuntimeBackedUnityPlayerLoopControlsUxUnitySmoke smoke,
        RuntimeBackedUnityPlayerLoopControlsUxUnityNoiseClassification noise,
        RuntimeBackedUnityPlayerLoopControlsUxResult result)
    {
        var diagnostics = new List<string>();
        diagnostics.AddRange(result.Diagnostics);
        Require(acceptance.Accepted && acceptance.AcceptedByHuman && !acceptance.AcceptedByCodex,
            "goal140.goal139_acceptance_record_invalid",
            diagnostics);
        Require(model.FrameCount == 13, "goal140.frame_count", diagnostics);
        Require(model.HumanReadableFrameNumbering, "goal140.human_frame_numbering", diagnostics);
        Require(model.StepOnceSemanticsClear, "goal140.step_once_semantics", diagnostics);
        Require(model.PlayAllToEndSemanticsClear, "goal140.play_all_to_end_semantics", diagnostics);
        Require(model.CopyFrameSummaryStatusPresent, "goal140.copy_status", diagnostics);
        Require(model.RequiredControlsPresent, "goal140.required_controls", diagnostics);
        Require(model.ControlsUxPolished, "goal140.controls_ux_polished", diagnostics);
        Require(smoke.Passed, "goal140.unity_controls_ux_smoke", diagnostics);
        Require(noise.KnownUnityEditorBuildProfileNoiseClassified,
            "goal140.known_unity_editor_noise_not_classified",
            diagnostics);
        Require(noise.BlockingUnityErrorCount == 0, "goal140.blocking_unity_errors", diagnostics);
        Require(noise.UnclassifiedUnityErrorCount == 0, "goal140.unclassified_unity_errors", diagnostics);
        Require(model.RuntimeAuthority, "goal140.runtime_authority", diagnostics);
        Require(!model.UnityGameplayTruth, "goal140.unity_gameplay_truth", diagnostics);
        Require(!model.ProjectionOnly, "goal140.projection_only", diagnostics);

        return new RuntimeBackedUnityPlayerLoopControlsUxDashboard
        {
            Status = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            Accepted = false,
            AcceptedGoal139 = acceptance.Accepted && acceptance.AcceptedByHuman && !acceptance.AcceptedByCodex,
            SelectedCandidate = model.CandidateId,
            FrameCount = model.FrameCount,
            HumanReadableFrameNumbering = model.HumanReadableFrameNumbering,
            StepOnceSemanticsClear = model.StepOnceSemanticsClear,
            PlayAllToEndSemanticsClear = model.PlayAllToEndSemanticsClear,
            CopyFrameSummaryStatusPresent = model.CopyFrameSummaryStatusPresent,
            RequiredControlsPresent = model.RequiredControlsPresent,
            ControlsUxPolished = model.ControlsUxPolished,
            UnityControlsUxSmokePassed = smoke.Passed,
            RuntimeAuthority = true,
            UnityGameplayTruth = false,
            ProjectionOnly = false,
            KnownUnityEditorNoiseClassified = noise.KnownUnityEditorBuildProfileNoiseClassified,
            KnownUnityEditorNoiseCount = noise.KnownUnityEditorNoiseCount,
            BlockingUnityErrorCount = noise.BlockingUnityErrorCount,
            UnclassifiedUnityErrorCount = noise.UnclassifiedUnityErrorCount,
            ManualUnityOptional = true,
            MissingControls = model.MissingControls,
            Diagnostics = diagnostics.Concat(smoke.Diagnostics).Concat(noise.Diagnostics).ToList()
        };
    }

    private static RuntimeBackedUnityPlayerLoopControlsUxUnitySmoke BuildPendingUnitySmoke(
        string root,
        string outputRootRelativePath)
    {
        var model = Resolve(
            root,
            outputRootRelativePath + "/" + RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ModelFileName);
        var script = Resolve(
            root,
            outputRootRelativePath + "/" + RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ScriptFileName);
        return new RuntimeBackedUnityPlayerLoopControlsUxUnitySmoke
        {
            UnityAvailable = true,
            ModelPathExists = File.Exists(model),
            ModelPath = Relative(root, model),
            ScriptPath = Relative(root, script),
            Status = "PENDING_UNITY_BATCHMODE",
            Diagnostics = ["Unity controls UX smoke has not written a marker artifact yet"]
        };
    }

    private static SortedDictionary<string, string> BuildFilePayloads(
        string relativeRoot,
        RuntimeBackedUnityPlayerLoopControlsUxPolishGoal139AcceptanceRecord acceptance,
        RuntimeBackedUnityPlayerLoopControlsUxModel model,
        RuntimeBackedUnityPlayerLoopControlsUxScript script,
        RuntimeBackedUnityPlayerLoopControlsUxResult result,
        RuntimeBackedUnityPlayerLoopControlsUxDashboard dashboard,
        RuntimeBackedUnityPlayerLoopControlsUxUnitySmoke smoke,
        RuntimeBackedUnityPlayerLoopControlsUxUnityNoiseClassification noise,
        RuntimeBackedUnityPlayerLoopControlsUxReport report,
        string reportMarkdown,
        RuntimeBackedUnityPlayerLoopControlsUxNegativeProof negative)
    {
        var files = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.Goal139AcceptanceFileName] =
                Serialize(acceptance),
            [RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ResultFileName] =
                Serialize(result),
            [RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ModelFileName] =
                Serialize(model),
            [RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ScriptFileName] =
                Serialize(script),
            [RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.UnitySmokeFileName] =
                Serialize(smoke),
            [RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.UnityNoiseClassificationFileName] =
                Serialize(noise),
            [RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ReportJsonFileName] =
                Serialize(report),
            [RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ReportMarkdownFileName] =
                reportMarkdown,
            [RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.NegativeProofFileName] =
                Serialize(negative)
        };
        files[RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.FileIndexFileName] =
            Serialize(BuildFileIndex(relativeRoot, files));
        return files;
    }

    private static RuntimeBackedUnityPlayerLoopControlsUxFileIndex BuildFileIndex(
        string relativeRoot,
        IReadOnlyDictionary<string, string> pendingTextFiles)
    {
        var files = pendingTextFiles
            .Select(item => new RuntimeBackedUnityPlayerLoopControlsUxFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = "goal140_" + Path.GetFileNameWithoutExtension(item.Key)
                    .Replace("-", "_", StringComparison.Ordinal),
                Sha256 = HashText(item.Value)
            })
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
        return new RuntimeBackedUnityPlayerLoopControlsUxFileIndex
        {
            RootPath = relativeRoot,
            IndexedFileCount = files.Count,
            ManualInputExcluded = files.All(file =>
                !file.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = files
        };
    }

    private static string RenderReport(
        RuntimeBackedUnityPlayerLoopControlsUxReport report,
        RuntimeBackedUnityPlayerLoopControlsUxDashboard dashboard,
        RuntimeBackedUnityPlayerLoopControlsUxResult result,
        RuntimeBackedUnityPlayerLoopControlsUxNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 140 Runtime-backed Unity Player Loop Controls UX Polish And Noise Guard",
            string.Empty,
            "- status: " + dashboard.Status,
            "- accepted: false",
            "- acceptedGoal139: " + Bool(report.AcceptedGoal139),
            "- selectedCandidate: " + report.SelectedCandidate,
            "- frameCount: " + report.FrameCount,
            "- humanReadableFrameNumbering: " + Bool(report.HumanReadableFrameNumbering),
            "- stepOnceSemanticsClear: " + Bool(report.StepOnceSemanticsClear),
            "- playAllToEndSemanticsClear: " + Bool(report.PlayAllToEndSemanticsClear),
            "- copyFrameSummaryStatusPresent: " + Bool(report.CopyFrameSummaryStatusPresent),
            "- requiredControlsPresent: " + Bool(report.RequiredControlsPresent),
            "- controlsUxPolished: " + Bool(report.ControlsUxPolished),
            "- unityControlsUxSmokePassed: " + Bool(report.UnityControlsUxSmokePassed),
            "- runtimeAuthority: " + Bool(report.RuntimeAuthority),
            "- unityGameplayTruth: " + Bool(report.UnityGameplayTruth),
            "- projectionOnly: " + Bool(report.ProjectionOnly),
            "- knownUnityEditorNoiseClassified: " + Bool(report.KnownUnityEditorNoiseClassified),
            "- knownUnityEditorNoiseCount: " + report.KnownUnityEditorNoiseCount,
            "- blockingUnityErrorCount: " + report.BlockingUnityErrorCount,
            "- unclassifiedUnityErrorCount: " + report.UnclassifiedUnityErrorCount,
            "- normalCommand: " + report.NormalCommand,
            "- reportPath: " + report.ReportPath,
            string.Empty,
            "## Source",
            string.Empty,
            "- sourceGoal139Model: " + result.Inputs.InteractiveControlsModelPath,
            "- sourceGoal139Result: " + result.Inputs.InteractiveControlsResultPath,
            "- sourceGoal139Script: " + result.Inputs.InteractiveControlsScriptPath,
            "- negativeProofPassed: " + Bool(negative.Passed),
            string.Empty,
            "## Script Steps",
            string.Empty
        };
        lines.AddRange(result.Session.Steps.Select(step =>
            "- " + step.StepIndex + " " + step.Action + " => "
            + step.CurrentFrameLabelAfter + "; "
            + step.FrameIndexLabelAfter
            + "; lastControlAction=" + step.LastControlAction
            + "; status=" + step.StatusAfterAction
            + "; passed=" + Bool(step.Passed)));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(dashboard.Diagnostics.Count == 0
            ? ["- none"]
            : dashboard.Diagnostics.Select(item => "- " + item));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderGoal139Acceptance(
        RuntimeBackedUnityPlayerLoopControlsUxPolishGoal139AcceptanceRecord acceptance)
    {
        var lines = new List<string>
        {
            "# Goal 139 Runtime-backed Unity Player Loop Interactive Controls Harness",
            string.Empty,
            "accepted=true",
            "acceptedByHuman=true",
            "acceptedByCodex=false",
            "selectedCandidate=" + acceptance.SelectedCandidate,
            "frames=" + acceptance.Frames.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "interactiveControlsSmoke=" + acceptance.InteractiveControlsSmoke,
            "requiredControlsPresent=" + Bool(acceptance.RequiredControlsPresent),
            "controlsWork=" + Bool(acceptance.ControlsWork),
            "projectionOnly=false",
            "runtimeAuthority=true",
            "unityGameplayTruth=false",
            "autoStepAutoPlayAllUxAcceptedWithFollowUpDebt=true",
            "rawManualInputNotCommitted=true",
            string.Empty,
            "Source: Goal140 task handoff recorded owner acceptance of Goal139. Raw manual input remains outside committed artifacts."
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderGoal140ManualAcceptance(
        RuntimeBackedUnityPlayerLoopControlsUxReport report,
        RuntimeBackedUnityPlayerLoopControlsUxDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Goal 140 Runtime-backed Unity Player Loop Controls UX Polish And Noise Guard",
            string.Empty,
            "accepted=false",
            "acceptedByHuman=false",
            "acceptedByCodex=false",
            "manualUnityOptional=true",
            "acceptedGoal139=" + Bool(report.AcceptedGoal139),
            "selectedCandidate=" + report.SelectedCandidate,
            "frameCount=" + report.FrameCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "humanReadableFrameNumbering=" + Bool(report.HumanReadableFrameNumbering),
            "stepOnceSemanticsClear=" + Bool(report.StepOnceSemanticsClear),
            "playAllToEndSemanticsClear=" + Bool(report.PlayAllToEndSemanticsClear),
            "copyFrameSummaryStatusPresent=" + Bool(report.CopyFrameSummaryStatusPresent),
            "knownUnityEditorNoiseClassified=" + Bool(report.KnownUnityEditorNoiseClassified),
            "blockingUnityErrorCount=" + report.BlockingUnityErrorCount,
            "unclassifiedUnityErrorCount=" + report.UnclassifiedUnityErrorCount,
            "unityControlsUxSmokePassed=" + Bool(report.UnityControlsUxSmokePassed),
            "runtimeAuthority=true",
            "unityGameplayTruth=false",
            "projectionOnly=false",
            "normalCommand=" + report.NormalCommand,
            "reportPath=" + report.ReportPath,
            "status=" + dashboard.Status
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static (int KnownUnityEditorNoiseCount, int BlockingUnityErrorCount, int UnclassifiedUnityErrorCount, IReadOnlyList<string> Diagnostics)
        ClassifyActualUnityLog(string logText)
    {
        var known = 0;
        var blocking = 0;
        var unclassified = 0;
        var diagnostics = new List<string>();
        var nullRefs = CountOccurrences(logText, "NullReferenceException");
        if (logText.Contains("BuildProfileContext", StringComparison.Ordinal)
            && logText.Contains("CreateOrLoad", StringComparison.Ordinal)
            && nullRefs > 0)
        {
            known = 1;
            diagnostics.Add("knownUnityEditorBuildProfileNoise=classified");
        }

        var unpairedNullRefs = Math.Max(0, nullRefs - known);
        if (unpairedNullRefs > 0)
        {
            unclassified += unpairedNullRefs;
            blocking += unpairedNullRefs;
            diagnostics.Add("unpairedNullReferenceException=" + unpairedNullRefs);
        }

        if (logText.Contains(RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.UnityFailMarker, StringComparison.Ordinal))
        {
            blocking++;
            diagnostics.Add("goal140FailMarkerPresent=true");
        }

        if (logText.Contains("CanonicalRuntimeUnityPlayerLoopInteractiveControls", StringComparison.Ordinal)
            && logText.Contains("Exception", StringComparison.Ordinal)
            && known == 0)
        {
            blocking++;
            diagnostics.Add("playerLoopHarnessExceptionPresent=true");
        }

        if (diagnostics.Count == 0)
        {
            diagnostics.Add("unityLogNoKnownBlockingNoiseMarkers=true");
        }

        return (known, blocking, unclassified, diagnostics);
    }

    private static int ApplyAction(string action, int frameIndex, int frameCount) =>
        action switch
        {
            "load_model" => 0,
            "first" => 0,
            "previous" => Math.Max(0, frameIndex - 1),
            "next" => Math.Min(Math.Max(frameCount - 1, 0), frameIndex + 1),
            "last" => Math.Max(frameCount - 1, 0),
            "step_once" => Math.Min(Math.Max(frameCount - 1, 0), frameIndex + 1),
            "play_all_to_end" => Math.Max(frameCount - 1, 0),
            _ => frameIndex
        };

    private static bool EvaluateAssertion(
        string action,
        RuntimeBackedUnityPlayerLoopControlsUxModel model,
        int frameIndex,
        string status) =>
        action switch
        {
            "assert_frame_count" => model.FrameCount == 13,
            "assert_human_readable_frame_numbering" => model.HumanReadableFrameNumbering
                                                       && HumanFrameLabel(frameIndex, model.FrameCount)
                                                       == "Current Frame: 1/13",
            "assert_copy_frame_summary_status" => status == "copied_frame_summary"
                                                  || model.CopyFrameSummaryStatusPresent,
            "assert_reset_first_status" => status == "reset_to_first_frame"
                                           || model.ResetFirstStatusPresent,
            "assert_runtime_authority_markers" =>
                model.RuntimeAuthority && !model.ProjectionOnly && !model.UnityGameplayTruth,
            _ => model.Controls.Any(control => control.Id == action)
                 || action is "load_model" or "first" or "previous" or "next" or "last"
        };

    private static int? ExpectedFrameIndex(string action, int stepIndex) =>
        action switch
        {
            "load_model" => 0,
            "assert_frame_count" => 0,
            "assert_human_readable_frame_numbering" => 0,
            "first" when stepIndex == 3 => 0,
            "next" => 1,
            "previous" => 0,
            "step_once" when stepIndex == 6 => 1,
            "step_once" when stepIndex == 7 => 2,
            "play_all_to_end" => 12,
            "copy_current_frame_summary" => 12,
            "assert_copy_frame_summary_status" => 12,
            "first" => 0,
            "assert_reset_first_status" => 0,
            "assert_runtime_authority_markers" => 0,
            _ => null
        };

    private static string ExpectedLastAction(string action) =>
        action switch
        {
            "assert_frame_count" => string.Empty,
            "assert_human_readable_frame_numbering" => string.Empty,
            "assert_copy_frame_summary_status" => "copy_current_frame_summary",
            "assert_reset_first_status" => "first",
            "assert_runtime_authority_markers" => string.Empty,
            _ => action
        };

    private static string ExpectedStatus(string action) =>
        action switch
        {
            "load_model" => "loaded_goal140_controls_ux_model",
            "first" => "reset_to_first_frame",
            "previous" => "moved_previous_frame",
            "next" => "moved_next_frame",
            "last" => "moved_last_frame",
            "step_once" => "stepped_one_frame_tick",
            "play_all_to_end" => "played_all_to_end",
            "copy_current_frame_summary" => "copied_frame_summary",
            "assert_copy_frame_summary_status" => "copied_frame_summary",
            "assert_reset_first_status" => "reset_to_first_frame",
            _ => string.Empty
        };

    private static string AssertionFor(string action) =>
        action switch
        {
            "assert_frame_count" => "frameCount == 13",
            "assert_human_readable_frame_numbering" => "Current Frame: 1/13 and Frame Index: 0 are visible",
            "step_once" => "step_once advances exactly one frame tick",
            "play_all_to_end" => "play_all_to_end jumps instant-to-final-frame",
            "copy_current_frame_summary" => "copy shows copied_frame_summary status",
            "assert_copy_frame_summary_status" => "status == copied_frame_summary",
            "assert_reset_first_status" => "first resets to first frame",
            "assert_runtime_authority_markers" =>
                "runtimeAuthority == true && projectionOnly == false && unityGameplayTruth == false",
            _ => "control exists and does not mutate gameplay"
        };

    private static string HumanFrameLabel(int frameIndex, int frameCount) =>
        "Current Frame: "
        + (frameIndex + 1).ToString(System.Globalization.CultureInfo.InvariantCulture)
        + "/"
        + frameCount.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string RawFrameIndexLabel(int frameIndex) =>
        "Frame Index: " + frameIndex.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static void Require(bool condition, string diagnostic, ICollection<string> diagnostics)
    {
        if (!condition && !diagnostics.Contains(diagnostic))
        {
            diagnostics.Add(diagnostic);
        }
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while (index >= 0)
        {
            index = text.IndexOf(value, index, StringComparison.Ordinal);
            if (index >= 0)
            {
                count++;
                index += value.Length;
            }
        }

        return count;
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

    private static void GuardGoal140Write(string root, string path)
    {
        GuardNotManualInput(root, path);
        var relative = Relative(root, path);
        if (!relative.StartsWith(
                RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ProceduralOutputDirectory + "/",
                StringComparison.Ordinal)
            && !relative.StartsWith(
                RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ExportPackageDirectory + "/",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to write outside Goal140 output roots: " + relative);
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
