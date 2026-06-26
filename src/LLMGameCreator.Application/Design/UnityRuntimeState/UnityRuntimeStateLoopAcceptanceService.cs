using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.AlphaBuild;
using LLMGameCreator.Application.Design.Assets;
using LLMGameCreator.Application.Design.ContentGeneration;
using LLMGameCreator.Application.Design.UnityGeneratedScene;
using LLMGameCreator.Application.Design.UnityPlayableAlpha;

namespace LLMGameCreator.Application.Design.UnityRuntimeState;

public sealed class UnityRuntimeStateLoopAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/unity-runtime-state-loop";
    public const string StateJsonFileName = "unity-runtime-state-loop-state.json";
    public const string ReportJsonFileName = "unity-runtime-state-loop-report.json";
    public const string ReportMarkdownFileName = "unity-runtime-state-loop-report.md";
    public const string VerificationMarkdownFileName = "unity-runtime-state-loop-verification.md";
    public const string FinalGate = "unity_generated_runtime_state_loop_verification";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static UnityRuntimeStateLoopAcceptanceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public UnityRuntimeStateLoopAcceptanceResult BuildFromAcceptedEvidence(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        UnityRuntimeStateLoopOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(contentGenerationResult);
        ArgumentNullException.ThrowIfNull(minimumAssetResult);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var settings = options ?? new UnityRuntimeStateLoopOptions();
        var projectRoot = Path.GetFullPath(projectRootPath);
        var repositoryRoot = ResolveRepositoryRoot(projectRoot, settings.RepositoryRootPath);
        var alphaService = new AlphaRunnableBuildAcceptanceService();
        var alphaResult = alphaService.BuildFromAcceptedEvidence(
            projectRoot,
            contentGenerationResult,
            minimumAssetResult,
            new AlphaRunnableBuildOptions
            {
                RepositoryRootPath = repositoryRoot,
                RelativeOutputDirectoryOverride = RelativeOutputDirectory,
                ExecuteUnityBuild = settings.ExecuteUnityBuild,
                LaunchBuiltPlayer = settings.LaunchBuiltPlayer,
                PreserveExistingBuildOutputForValidation = settings.PreserveExistingBuildOutputForValidation,
                CleanupUnityWorkProject = settings.CleanupUnityWorkProject,
                UnityBuildTimeoutSeconds = settings.UnityBuildTimeoutSeconds,
                PlayerLaunchTimeoutSeconds = settings.PlayerLaunchTimeoutSeconds
            });

        var alpha = alphaResult.Report;
        var projection = UnityGeneratedSceneProjectionAcceptanceService.BuildProjection(alpha);
        var projectionValidation = UnityGeneratedSceneProjectionAcceptanceService.ValidateProjection(projection, alpha);
        var previousEvidence = ValidatePreviousGoal015Evidence(repositoryRoot, projection);
        var playLoop = ValidatePlayLoop(projectRoot, alpha, projection);
        var firewall = ValidateFirewall(repositoryRoot, projectRoot, alpha);
        var state = BuildStateModel(projection, playLoop);
        var invalidMatrix = BuildInvalidMatrix(projection, playLoop, firewall);
        var diagnostics = SortDiagnostics(
            previousEvidence.Diagnostics
                .Concat(ConvertDiagnostics(projectionValidation.Diagnostics))
                .Concat(playLoop.Diagnostics)
                .Concat(firewall.Diagnostics)
                .Concat(invalidMatrix.Diagnostics)
                .Concat(alpha.Diagnostics.Select(ConvertDiagnostic))
                .Concat(
                [
                    Diagnostic("info", "unity_runtime_state.goal015_gate_recorded", "unity_generated_scene_content_projection_verification", "User-confirmed Goal 015 generated scene projection verification is recorded as passed."),
                    Diagnostic("info", "unity_runtime_state.no_external_providers", "execution_boundary", "No LLM, RAG, provider, media, arbitrary Lua or generator-library execution was invoked.")
                ]));

        var stateWithoutHash = state with { StateHash = string.Empty };
        state = stateWithoutHash with
        {
            StateHash = ComputeHash(JsonSerializer.Serialize(stateWithoutHash, JsonOptions))
        };

        var reportWithoutHash = new UnityRuntimeStateLoopReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousAcceptedGate = "unity_generated_scene_content_projection_verification passed",
            CompletedSlices = ["S130", "S131", "S132", "S133", "S134", "S135", "S136", "S137"],
            ProductSmokeRoute = "unity-runtime-state-loop",
            AlphaBuild = alpha,
            Projection = projection,
            State = state,
            SelectedPackageId = projection.SelectedPackageId,
            SelectedStyleId = projection.SelectedStyleId,
            SelectedThreadId = projection.SelectedThreadId,
            SelectedMapId = projection.SelectedMapId,
            SelectedSceneNodeIds = projection.Nodes.Select(node => node.NodeId).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            SelectedCommandHints = projection.CommandHints,
            RuntimeStateLoopVerified = playLoop.RuntimeStateLoopVerified,
            StateTransitionTraceVerified = playLoop.StateTransitionTraceVerified,
            QuestStateVerified = playLoop.QuestStateVerified,
            DialogueStateVerified = playLoop.DialogueStateVerified,
            InventoryStateVerified = playLoop.InventoryStateVerified,
            EventStateVerified = playLoop.EventStateVerified,
            MovementVerified = playLoop.MovementVerified,
            FocusVerified = playLoop.FocusVerified,
            InteractionVerified = playLoop.InteractionVerified,
            PlayLoopVerified = alpha.PlayLoopVerified && playLoop.PlayLoopVerified,
            PreviousSceneProjectionEvidenceVerified = previousEvidence.Passed,
            SceneProjectionVerified = projectionValidation.Passed,
            FirewallSafeBuildVerified = firewall.FirewallSafeBuildVerified,
            InvalidMatrix = invalidMatrix,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            NoExternalProviderLlmRagLuaMedia = true,
            RuntimePreviewDependency = alpha.RuntimePreviewDependency,
            StateLoopHash = state.StateHash,
            BuildManifestHash = alpha.BuildManifestHash,
            DeterministicReportRelativePath = $"{RelativeOutputDirectory}/{ReportJsonFileName}",
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new UnityRuntimeStateLoopAcceptanceResult
        {
            Report = report,
            StateJson = JsonSerializer.Serialize(state, JsonOptions),
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            VerificationMarkdown = RenderVerification(report, alphaResult.VerificationMarkdown)
        };
    }

    public async Task<UnityRuntimeStateLoopWriteResult> WriteAsync(
        string projectRootPath,
        UnityRuntimeStateLoopAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var statePath = Path.Combine(outputDirectory, StateJsonFileName);
        var jsonPath = Path.Combine(outputDirectory, ReportJsonFileName);
        var markdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        var verificationPath = Path.Combine(outputDirectory, VerificationMarkdownFileName);
        await File.WriteAllTextAsync(statePath, result.StateJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(jsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(markdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new UnityRuntimeStateLoopWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            StateJsonPath = statePath,
            ReportJsonPath = jsonPath,
            ReportMarkdownPath = markdownPath,
            VerificationMarkdownPath = verificationPath
        };
    }

    public async Task<UnityRuntimeStateLoopWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        UnityRuntimeStateLoopOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var result = BuildFromAcceptedEvidence(projectRootPath, contentGenerationResult, minimumAssetResult, options);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public static UnityRuntimeStateLoopProof ValidateStateLoopLines(
        IEnumerable<string> lines,
        UnityGeneratedSceneProjection projection)
    {
        var diagnostics = new List<UnityRuntimeStateDiagnostic>();
        var values = ParseKeyValueLog(lines);
        var sceneProof = UnityGeneratedSceneProjectionAcceptanceService.ValidatePlayLoopLines(
            values.Select(pair => pair.Key + "=" + pair.Value),
            projection);
        diagnostics.AddRange(ConvertDiagnostics(sceneProof.Diagnostics));

        Require(values, "alpha_runtime.package_id", projection.SelectedPackageId, "unity_runtime_state.play_loop.package_id_mismatch");
        Require(values, "alpha_runtime.selected_style_id", projection.SelectedStyleId, "unity_runtime_state.play_loop.style_id_mismatch");
        Require(values, "alpha_runtime.selected_thread_id", projection.SelectedThreadId, "unity_runtime_state.play_loop.thread_id_mismatch");
        Require(values, "alpha_runtime.map_bounds", projection.MapWidth + "x" + projection.MapHeight, "unity_runtime_state.play_loop.map_bounds_missing");

        var playerNode = projection.Nodes.FirstOrDefault(node => node.NodeKind == "player");
        var playerPosition = playerNode == null ? string.Empty : playerNode.X + "," + playerNode.Y;
        Require(values, "alpha_runtime.projected_player_node_position", playerPosition, "unity_runtime_state.play_loop.projected_player_missing");
        Require(values, "alpha_runtime.movement.initial_position", playerPosition, "unity_runtime_state.play_loop.initial_position_mismatch");
        if (values.GetValueOrDefault("alpha_runtime.movement.step.1.position", playerPosition) == playerPosition)
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_state.play_loop.movement_not_changed", "alpha_runtime.movement.step.1.position", "Player movement must change position after starting at the projected player node."));
        }

        Require(values, "alpha_runtime.state.before.quest_started", "false", "unity_runtime_state.state.before_after_missing");
        Require(values, "alpha_runtime.state.after.quest_started", "true", "unity_runtime_state.state.before_after_missing");
        Require(values, "alpha_runtime.state.before.dialogue_opened", "false", "unity_runtime_state.state.before_after_missing");
        Require(values, "alpha_runtime.state.after.dialogue_opened", "true", "unity_runtime_state.state.before_after_missing");
        Require(values, "alpha_runtime.state.before.dialogue_choice_selected", "false", "unity_runtime_state.state.before_after_missing");
        Require(values, "alpha_runtime.state.after.dialogue_choice_selected", "true", "unity_runtime_state.state.before_after_missing");
        Require(values, "alpha_runtime.state.before.item_obtained", "false", "unity_runtime_state.state.before_after_missing");
        Require(values, "alpha_runtime.state.after.item_obtained", "true", "unity_runtime_state.state.before_after_missing");
        Require(values, "alpha_runtime.state.before.inventory_item_count", "0", "unity_runtime_state.state.before_after_missing");
        Require(values, "alpha_runtime.state.after.inventory_item_count", "1", "unity_runtime_state.state.before_after_missing");
        Require(values, "alpha_runtime.state.before.event_applied", "false", "unity_runtime_state.state.before_after_missing");
        Require(values, "alpha_runtime.state.after.event_applied", "true", "unity_runtime_state.state.before_after_missing");

        var transitions = ParseTransitions(values);
        if (transitions.Count < 5)
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_state.transition.count_too_low", "alpha_runtime.command_state_transition_count", "State loop must record command-correlated state transitions."));
        }

        var expectedCommands = projection.CommandHints.ToDictionary(command => command.CommandId, StringComparer.Ordinal);
        foreach (var transition in transitions)
        {
            if (!expectedCommands.TryGetValue(transition.CommandId, out var expected) ||
                !string.Equals(expected.CommandType, transition.CommandType, StringComparison.Ordinal) ||
                !string.Equals(expected.TargetId, transition.TargetId, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_state.transition.command_mismatch", transition.CommandId, "Transition command id/type/target must match projection command evidence."));
                continue;
            }

            if (string.Equals(transition.Before, transition.After, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_state.transition.no_before_after_delta", transition.StateKey, "Transition before/after values must differ."));
            }

            if (!StateChangeAllowedByCommand(transition, projection))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_state.transition.state_command_correlation_failed", transition.StateKey, "State changes must be caused by matching generated command ids, types and targets."));
            }
        }

        RequireTransition(transitions, "questStarted", "quest/start", projection.SelectedQuestId, diagnostics);
        RequireTransition(transitions, "dialogueOpened", "dialogue/open", projection.SelectedDialogueId, diagnostics);
        RequireTransition(transitions, "dialogueChoiceSelected", "dialogue/choose", string.Empty, diagnostics);
        RequireTransition(transitions, "itemObtained", "event/add_item", projection.SelectedItemId, diagnostics);
        RequireTransition(transitions, "inventoryItemCount", "event/add_item", projection.SelectedItemId, diagnostics);
        RequireTransition(transitions, "eventApplied", "event/add_item", projection.SelectedItemId, diagnostics);

        var focusedNodeId = values.GetValueOrDefault("alpha_runtime.focus.selected_node_id", string.Empty);
        var focusVerified = projection.Nodes.Any(node => node.NodeId == focusedNodeId && node.NodeKind != "player");
        if (!focusVerified)
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_state.focus.target_not_generated_node", focusedNodeId, "Focus target must be a generated scene node."));
        }

        var transitionTraceVerified = diagnostics.All(item => !item.Code.StartsWith("unity_runtime_state.transition.", StringComparison.Ordinal) && item.Code != "unity_runtime_state.state.before_after_missing");
        var questVerified = HasTransition(transitions, "questStarted") && values.GetValueOrDefault("alpha_runtime.state.after.quest_started") == "true";
        var dialogueVerified = HasTransition(transitions, "dialogueOpened") && HasTransition(transitions, "dialogueChoiceSelected");
        var inventoryVerified = HasTransition(transitions, "itemObtained") && HasTransition(transitions, "inventoryItemCount") && values.GetValueOrDefault("alpha_runtime.state.after.inventory_item_count") == "1";
        var eventVerified = HasTransition(transitions, "eventApplied") && values.GetValueOrDefault("alpha_runtime.state.after.event_applied") == "true";

        return new UnityRuntimeStateLoopProof
        {
            RuntimeStateLoopVerified = diagnostics.All(item => item.Severity != "error"),
            StateTransitionTraceVerified = transitionTraceVerified,
            QuestStateVerified = questVerified,
            DialogueStateVerified = dialogueVerified,
            InventoryStateVerified = inventoryVerified,
            EventStateVerified = eventVerified,
            MovementVerified = sceneProof.MovementVerified && diagnostics.All(item => item.Code != "unity_runtime_state.play_loop.movement_not_changed" && item.Code != "unity_runtime_state.play_loop.projected_player_missing" && item.Code != "unity_runtime_state.play_loop.initial_position_mismatch"),
            FocusVerified = focusVerified,
            InteractionVerified = sceneProof.InteractionVerified && transitionTraceVerified,
            PlayLoopVerified = diagnostics.All(item => item.Severity != "error"),
            PlayerPositionBefore = values.GetValueOrDefault("alpha_runtime.movement.initial_position", string.Empty),
            PlayerPositionAfter = values.GetValueOrDefault("alpha_runtime.movement.step.1.position", string.Empty),
            BlockedMovementPosition = values.GetValueOrDefault("alpha_runtime.movement.blocked.position", string.Empty),
            FocusedNodeId = focusedNodeId,
            FocusedSourceId = projection.Nodes.FirstOrDefault(node => node.NodeId == focusedNodeId)?.SourceGeneratedId ?? string.Empty,
            LastCommandId = values.GetValueOrDefault("alpha_runtime.state.after.last_command_id", string.Empty),
            LastCommandType = values.GetValueOrDefault("alpha_runtime.state.after.last_command_type", string.Empty),
            LastCommandTargetId = values.GetValueOrDefault("alpha_runtime.state.after.last_command_target_id", string.Empty),
            StatusText = values.GetValueOrDefault("alpha_runtime.state.after.status_text", string.Empty),
            CommandStateTransitionCount = transitions.Count,
            Transitions = transitions,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        void Require(IReadOnlyDictionary<string, string> parsed, string key, string expected, string code)
        {
            if (!parsed.TryGetValue(key, out var actual) || !string.Equals(actual, expected, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", code, key, $"Expected {key}={expected}."));
            }
        }
    }

    public static IReadOnlyList<string> BuildExpectedStateLoopLines(UnityGeneratedSceneProjection projection)
    {
        var lines = new List<string>
        {
            "alpha_runtime.play_loop_started=true",
            "alpha_runtime.visible_presentation_initialized=true",
            "alpha_runtime.visible_component.map=true",
            "alpha_runtime.visible_component.player_marker=true",
            "alpha_runtime.visible_component.npc_marker=true",
            "alpha_runtime.visible_component.item_marker=true",
            "alpha_runtime.visible_component.status_panel=true",
            "alpha_runtime.visible_component.command_log=true",
            "alpha_runtime.payload_root_exists=true",
            "alpha_runtime.config_loaded=true",
            "alpha_runtime.package_loaded=true",
            "alpha_runtime.asset_manifest_loaded=true",
            "alpha_runtime.package_id=" + projection.SelectedPackageId,
            "alpha_runtime.selected_style_id=" + projection.SelectedStyleId,
            "alpha_runtime.package_hash=" + projection.PackageHash,
            "alpha_runtime.asset_manifest_hash=" + projection.AssetManifestHash,
            "alpha_runtime.runtime_config_hash=" + projection.RuntimeConfigHash,
            "alpha_runtime.start_map_id=" + projection.SelectedMapId,
            "alpha_runtime.selected_thread_id=" + projection.SelectedThreadId,
            "alpha_runtime.selected_npc_id=" + projection.SelectedNpcId,
            "alpha_runtime.selected_quest_id=" + projection.SelectedQuestId,
            "alpha_runtime.selected_dialogue_id=" + projection.SelectedDialogueId,
            "alpha_runtime.selected_item_id=" + projection.SelectedItemId,
            "alpha_runtime.selected_event_id=" + projection.SelectedEventId,
            "alpha_runtime.command_hint_count=" + projection.CommandHints.Count,
            "alpha_runtime.asset_ref_count=" + projection.AssetRefs.Count,
            "alpha_runtime.scene_projection_loaded=true",
            "alpha_runtime.scene_node_count=" + projection.Nodes.Count
        };

        foreach (var kind in new[] { "map", "player", "npc", "item", "quest_event", "command_status" })
        {
            var node = projection.Nodes.First(node => node.NodeKind == kind);
            lines.Add("alpha_runtime.scene_node_resolved." + kind + "=true");
            lines.Add("alpha_runtime.scene_node." + kind + ".id=" + node.NodeId);
            lines.Add("alpha_runtime.scene_node." + kind + ".source_id=" + node.SourceGeneratedId);
            lines.Add("alpha_runtime.scene_node." + kind + ".position=" + node.X + "," + node.Y);
            lines.Add("alpha_runtime.scene_node." + kind + ".label=" + node.DisplayLabel);
        }

        lines.Add("alpha_runtime.ref_resolved.map=true");
        lines.Add("alpha_runtime.ref_resolved.npc=true");
        lines.Add("alpha_runtime.ref_resolved.quest=true");
        lines.Add("alpha_runtime.ref_resolved.dialogue=true");
        lines.Add("alpha_runtime.ref_resolved.item=true");
        lines.Add("alpha_runtime.ref_resolved.event=true");

        var player = projection.Nodes.First(node => node.NodeKind == "player");
        var initial = player.X + "," + player.Y;
        var moved = (player.X + 1) + "," + (player.Y + 1);
        var blocked = "0," + (player.Y + 1);
        lines.Add("alpha_runtime.map_bounds=" + projection.MapWidth + "x" + projection.MapHeight);
        lines.Add("alpha_runtime.projected_player_node_position=" + initial);
        lines.Add("alpha_runtime.movement.initial_position=" + initial);
        lines.Add("alpha_runtime.movement.step.0.valid=true");
        lines.Add("alpha_runtime.movement.step.0.position=" + (player.X + 1) + "," + player.Y);
        lines.Add("alpha_runtime.movement.step.1.valid=true");
        lines.Add("alpha_runtime.movement.step.1.position=" + moved);
        lines.Add("alpha_runtime.movement.blocked.valid=false");
        lines.Add("alpha_runtime.movement.blocked.position=" + blocked);
        var focus = projection.Nodes.First(node => node.NodeKind == "item");
        lines.Add("alpha_runtime.focus.selected=" + focus.NodeKind + ":" + focus.SourceGeneratedId);
        lines.Add("alpha_runtime.focus.selected_node_id=" + focus.NodeId);

        var state = new MutableState();
        var transitionIndex = 0;
        for (var index = 0; index < projection.CommandHints.Count; index++)
        {
            var command = projection.CommandHints[index];
            var before = state.Copy();
            ApplyExpected(command, projection, state);
            var after = state.Copy();
            lines.Add("alpha_runtime.command_executed." + index + ".id=" + command.CommandId);
            lines.Add("alpha_runtime.command_executed." + index + ".type=" + command.CommandType);
            lines.Add("alpha_runtime.command_executed." + index + ".target_id=" + command.TargetId);
            lines.Add("alpha_runtime.command_executed." + index + ".secondary_target_id=" + command.SecondaryTargetId);
            transitionIndex = AppendExpectedTransitions(lines, transitionIndex, command, before, after);
        }

        lines.Add("alpha_runtime.command_state_transition_count=" + transitionIndex);
        lines.Add("alpha_runtime.state.before.quest_started=false");
        lines.Add("alpha_runtime.state.after.quest_started=true");
        lines.Add("alpha_runtime.state.before.quest_completed_candidate=false");
        lines.Add("alpha_runtime.state.after.quest_completed_candidate=true");
        lines.Add("alpha_runtime.state.before.dialogue_opened=false");
        lines.Add("alpha_runtime.state.after.dialogue_opened=true");
        lines.Add("alpha_runtime.state.before.dialogue_choice_selected=false");
        lines.Add("alpha_runtime.state.after.dialogue_choice_selected=true");
        lines.Add("alpha_runtime.state.before.item_obtained=false");
        lines.Add("alpha_runtime.state.after.item_obtained=true");
        lines.Add("alpha_runtime.state.before.inventory_item_count=0");
        lines.Add("alpha_runtime.state.after.inventory_item_count=1");
        lines.Add("alpha_runtime.state.before.event_applied=false");
        lines.Add("alpha_runtime.state.after.event_applied=true");
        lines.Add("alpha_runtime.state.after.last_command_id=" + projection.CommandHints.Last().CommandId);
        lines.Add("alpha_runtime.state.after.last_command_type=" + projection.CommandHints.Last().CommandType);
        lines.Add("alpha_runtime.state.after.last_command_target_id=" + projection.CommandHints.Last().TargetId);
        lines.Add("alpha_runtime.state.after.status_text=Executed " + projection.CommandHints.Last().CommandType + " -> " + projection.CommandHints.Last().TargetId);
        lines.Add("alpha_runtime.state_transition.quest_start=true");
        lines.Add("alpha_runtime.state_transition.dialogue_open=true");
        lines.Add("alpha_runtime.state_transition.dialogue_choice=true");
        lines.Add("alpha_runtime.state_transition.item_or_loot=true");
        lines.Add("alpha_runtime.state_transition.event_application=true");
        lines.Add("alpha_runtime.quest_started=true");
        lines.Add("alpha_runtime.dialogue_seen=true");
        lines.Add("alpha_runtime.dialogue_choice_selected=true");
        lines.Add("alpha_runtime.item_obtained=true");
        lines.Add("alpha_runtime.inventory_item_count=1");
        lines.Add("alpha_runtime.event_applied=true");
        lines.Add("alpha_runtime.commands_executed=" + projection.CommandHints.Count);
        lines.Add("alpha_runtime.play_loop_completed=true");
        return lines;
    }

    private static UnityRuntimeStateLoopProof ValidatePlayLoop(
        string projectRoot,
        AlphaRunnableBuildReport alpha,
        UnityGeneratedSceneProjection projection)
    {
        var playLoopLogPath = string.IsNullOrWhiteSpace(alpha.LaunchVerification.PlayLoopLogRelativePath)
            ? string.Empty
            : Path.Combine(projectRoot, alpha.LaunchVerification.PlayLoopLogRelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (string.IsNullOrWhiteSpace(playLoopLogPath) || !File.Exists(playLoopLogPath))
        {
            return new UnityRuntimeStateLoopProof
            {
                Diagnostics =
                [
                    Diagnostic("error", "unity_runtime_state.play_loop.log_missing", "logs/alpha-player-play-loop.log", "Runtime state loop verification requires the real player play-loop log.")
                ]
            };
        }

        return ValidateStateLoopLines(File.ReadAllLines(playLoopLogPath), projection);
    }

    private static UnityRuntimeStatePreviousEvidenceProof ValidatePreviousGoal015Evidence(
        string repositoryRoot,
        UnityGeneratedSceneProjection projection)
    {
        var diagnostics = new List<UnityRuntimeStateDiagnostic>();
        var projectionPath = Path.Combine(repositoryRoot, UnityGeneratedSceneProjectionAcceptanceService.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar), UnityGeneratedSceneProjectionAcceptanceService.ProjectionJsonFileName);
        var reportPath = Path.Combine(repositoryRoot, UnityGeneratedSceneProjectionAcceptanceService.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar), UnityGeneratedSceneProjectionAcceptanceService.ReportJsonFileName);
        if (!File.Exists(projectionPath))
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_state.previous.projection_missing", UnityGeneratedSceneProjectionAcceptanceService.ProjectionJsonFileName, "Goal 016 must reuse accepted Goal 015 scene projection evidence."));
        }

        if (!File.Exists(reportPath))
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_state.previous.report_missing", UnityGeneratedSceneProjectionAcceptanceService.ReportJsonFileName, "Goal 016 must record the accepted Goal 015 report evidence."));
        }

        if (File.Exists(reportPath))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(reportPath));
            var root = document.RootElement;
            var finalStatus = root.TryGetProperty("finalStatus", out var finalStatusElement) ? finalStatusElement.GetString() : string.Empty;
            var projectionHash = root.TryGetProperty("projectionHash", out var projectionHashElement) ? projectionHashElement.GetString() : string.Empty;
            if (!string.Equals(finalStatus, UnityGeneratedSceneProjectionAcceptanceService.FinalGate, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_state.previous.final_gate_mismatch", finalStatus ?? string.Empty, "Goal 015 report must be the generated scene projection gate."));
            }

            if (!string.IsNullOrWhiteSpace(projectionHash) &&
                !string.Equals(projectionHash, projection.ProjectionHash, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "unity_runtime_state.previous.projection_hash_mismatch", projectionHash, "Goal 016 projection must match accepted Goal 015 projection evidence."));
            }
        }

        return new UnityRuntimeStatePreviousEvidenceProof
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            ProjectionRelativePath = $"{UnityGeneratedSceneProjectionAcceptanceService.RelativeOutputDirectory}/{UnityGeneratedSceneProjectionAcceptanceService.ProjectionJsonFileName}",
            ReportRelativePath = $"{UnityGeneratedSceneProjectionAcceptanceService.RelativeOutputDirectory}/{UnityGeneratedSceneProjectionAcceptanceService.ReportJsonFileName}",
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static UnityRuntimeStateFirewallProof ValidateFirewall(string repositoryRoot, string projectRoot, AlphaRunnableBuildReport alpha)
    {
        var scriptPath = Path.Combine(repositoryRoot, "unity", "LLMGameCreatorAlpha", "Assets", "Editor", "AlphaBuildEntrypoint.cs");
        if (!File.Exists(scriptPath))
        {
            return new UnityRuntimeStateFirewallProof
            {
                Diagnostics =
                [
                    Diagnostic("error", "unity_runtime_state.firewall.build_script_missing", "unity/LLMGameCreatorAlpha/Assets/Editor/AlphaBuildEntrypoint.cs", "Firewall-safe build proof requires the repository Alpha build entrypoint.")
                ]
            };
        }

        var proof = UnityPlayableAlphaAcceptanceService.ValidateFirewallSafeBuildScript(File.ReadAllText(scriptPath));
        var metadataPath = Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar), "build", "windows", "alpha-build-metadata.json");
        var metadataPresent = File.Exists(metadataPath);
        var diagnostics = proof.Diagnostics.Select(ConvertDiagnostic).ToList();
        if (alpha.WindowsExecutableProduced && !metadataPresent)
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_state.firewall.metadata_missing", "alpha-build-metadata.json", "Runtime state loop build metadata must be present for produced Windows player output."));
        }

        return new UnityRuntimeStateFirewallProof
        {
            BuildOptions = proof.BuildOptions,
            StaticChecksPassed = proof.StaticChecksPassed,
            BuildMetadataPresent = metadataPresent,
            FirewallSafeBuildVerified = proof.StaticChecksPassed && (!alpha.WindowsExecutableProduced || metadataPresent),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static UnityRuntimeStateLoopState BuildStateModel(
        UnityGeneratedSceneProjection projection,
        UnityRuntimeStateLoopProof proof)
    {
        return new UnityRuntimeStateLoopState
        {
            SchemaVersion = "unity_runtime_state_loop_v1",
            SelectedPackageId = projection.SelectedPackageId,
            SelectedStyleId = projection.SelectedStyleId,
            SelectedThreadId = projection.SelectedThreadId,
            SelectedMapId = projection.SelectedMapId,
            SelectedSceneNodeIds = projection.Nodes.Select(node => node.NodeId).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            SelectedCommandHints = projection.CommandHints,
            PlayerPositionBefore = proof.PlayerPositionBefore,
            PlayerPositionAfter = proof.PlayerPositionAfter,
            BlockedMovementPosition = proof.BlockedMovementPosition,
            FocusedNodeId = proof.FocusedNodeId,
            FocusedSourceId = proof.FocusedSourceId,
            QuestStateBefore = false,
            QuestStateAfter = proof.QuestStateVerified,
            QuestCompletedCandidate = proof.QuestStateVerified && proof.DialogueStateVerified && proof.InventoryStateVerified && proof.EventStateVerified,
            DialogueOpenedBefore = false,
            DialogueOpenedAfter = proof.DialogueStateVerified,
            DialogueChoiceSelectedBefore = false,
            DialogueChoiceSelectedAfter = proof.DialogueStateVerified,
            ItemObtainedBefore = false,
            ItemObtainedAfter = proof.InventoryStateVerified,
            InventoryItemCountBefore = 0,
            InventoryItemCountAfter = proof.InventoryStateVerified ? 1 : 0,
            EventAppliedBefore = false,
            EventAppliedAfter = proof.EventStateVerified,
            LastCommandId = proof.LastCommandId,
            LastCommandType = proof.LastCommandType,
            LastCommandTargetId = proof.LastCommandTargetId,
            StatusText = proof.StatusText,
            CommandExecutionTrace = proof.Transitions
        };
    }

    private static UnityRuntimeStateInvalidMatrix BuildInvalidMatrix(
        UnityGeneratedSceneProjection projection,
        UnityRuntimeStateLoopProof playLoop,
        UnityRuntimeStateFirewallProof firewall)
    {
        var baseline = BuildExpectedStateLoopLines(projection);
        var scenarios = new List<UnityRuntimeStateInvalidScenario>
        {
            InvalidScenario("missing_accepted_goal015_evidence", [Diagnostic("error", "unity_runtime_state.contract.missing_goal015_evidence", "unity_generated_scene_content_projection_verification", "Goal 016 must record the accepted Goal 015 gate.")]),
            InvalidScenario("missing_scene_projection_evidence", [Diagnostic("error", "unity_runtime_state.contract.missing_scene_projection", UnityGeneratedSceneProjectionAcceptanceService.ProjectionJsonFileName, "Runtime state loop cannot be accepted without scene projection evidence.")]),
            InvalidScenario("copied_state_loop_report_without_player_log", [Diagnostic("error", "unity_runtime_state.play_loop.log_missing", "logs/alpha-player-play-loop.log", "State-loop report cannot replace real player play-loop evidence.")]),
            LinesScenario("fake_state_changed_without_before_after_trace", baseline.Where(line => !line.StartsWith("alpha_runtime.state.before.", StringComparison.Ordinal) && !line.StartsWith("alpha_runtime.state.after.", StringComparison.Ordinal) && !line.StartsWith("alpha_runtime.command_state_transition.", StringComparison.Ordinal))),
            LinesScenario("quest_state_changed_by_non_quest_command", ReplaceLine(baseline, "alpha_runtime.command_state_transition.5.command_type=", "alpha_runtime.command_state_transition.5.command_type=dialogue/open")),
            LinesScenario("dialogue_state_changed_without_dialogue_command", ReplaceLine(baseline, "alpha_runtime.command_state_transition.4.command_type=", "alpha_runtime.command_state_transition.4.command_type=quest/start")),
            LinesScenario("item_state_changed_without_item_command", ReplaceLine(baseline, "alpha_runtime.command_state_transition.1.command_type=", "alpha_runtime.command_state_transition.1.command_type=dialogue/open")),
            LinesScenario("event_state_changed_without_event_command", ReplaceLine(baseline, "alpha_runtime.command_state_transition.3.command_type=", "alpha_runtime.command_state_transition.3.command_type=dialogue/open")),
            LinesScenario("inventory_count_changed_without_matching_item_target", ReplaceLine(baseline, "alpha_runtime.command_state_transition.2.target_id=", "alpha_runtime.command_state_transition.2.target_id=item/mismatch")),
            LinesScenario("command_id_mismatch", ReplaceLine(baseline, "alpha_runtime.command_state_transition.0.command_id=", "alpha_runtime.command_state_transition.0.command_id=cmd/mismatch")),
            LinesScenario("command_type_mismatch", ReplaceLine(baseline, "alpha_runtime.command_state_transition.0.command_type=", "alpha_runtime.command_state_transition.0.command_type=quest/start")),
            LinesScenario("command_target_mismatch", ReplaceLine(baseline, "alpha_runtime.command_state_transition.0.target_id=", "alpha_runtime.command_state_transition.0.target_id=target/mismatch")),
            LinesScenario("command_order_mismatch", ReplaceLine(baseline, "alpha_runtime.command_executed.0.id=", "alpha_runtime.command_executed.0.id=" + projection.CommandHints[1].CommandId)),
            LinesScenario("focus_target_not_generated_scene_node", ReplaceLine(baseline, "alpha_runtime.focus.selected_node_id=", "alpha_runtime.focus.selected_node_id=scene_node/missing")),
            LinesScenario("movement_proof_without_projected_player_node", baseline.Where(line => !line.StartsWith("alpha_runtime.projected_player_node_position=", StringComparison.Ordinal))),
            LinesScenario("blocked_bounds_proof_without_projected_map_bounds", baseline.Where(line => !line.StartsWith("alpha_runtime.map_bounds=", StringComparison.Ordinal))),
            LinesScenario("state_leak_from_previous_run", ReplaceLine(baseline, "alpha_runtime.state.before.quest_started=", "alpha_runtime.state.before.quest_started=true")),
            LinesScenario("cross_style_state_projection_leakage", ReplaceLine(baseline, "alpha_runtime.selected_style_id=", "alpha_runtime.selected_style_id=other_style")),
            InvalidScenario("runtime_preview_dependency_claim", [Diagnostic("error", "unity_runtime_state.contract.runtime_preview_dependency", "runtime_host", "Unity runtime state loop must not claim Runtime Preview dependency.")]),
            InvalidScenario("development_profiler_debug_build_option_reintroduced", UnityPlayableAlphaAcceptanceService.ValidateFirewallSafeBuildScript("options = BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;").Diagnostics.Select(ConvertDiagnostic).ToList())
        };

        var passed = scenarios.All(item => !item.ActualValid) &&
            (!playLoop.PlayLoopVerified || playLoop.RuntimeStateLoopVerified) &&
            firewall.FirewallSafeBuildVerified;
        return new UnityRuntimeStateInvalidMatrix
        {
            Passed = passed,
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList(),
            Diagnostics =
            [
                Diagnostic(passed ? "info" : "error", passed ? "unity_runtime_state.invalid_matrix_rejected" : "unity_runtime_state.invalid_matrix_failed", "invalid_matrix", "Invalid/fake/leak runtime state scenarios must reject through state-loop, log and firewall validation paths.")
            ]
        };

        UnityRuntimeStateInvalidScenario LinesScenario(string id, IEnumerable<string> lines) =>
            InvalidScenario(id, ValidateStateLoopLines(lines, projection).Diagnostics);
    }

    private static bool StateChangeAllowedByCommand(
        UnityRuntimeStateTransition transition,
        UnityGeneratedSceneProjection projection)
    {
        return transition.StateKey switch
        {
            "questStarted" => transition.CommandType == "quest/start" && transition.TargetId == projection.SelectedQuestId,
            "dialogueOpened" => transition.CommandType == "dialogue/open" && transition.TargetId == projection.SelectedDialogueId,
            "dialogueChoiceSelected" => transition.CommandType == "dialogue/choose",
            "itemObtained" => transition.CommandType is "event/add_item" or "loot/roll" && (transition.TargetId == projection.SelectedItemId || transition.CommandType == "loot/roll"),
            "inventoryItemCount" => transition.CommandType == "event/add_item" && transition.TargetId == projection.SelectedItemId,
            "eventApplied" => transition.CommandType.StartsWith("event/", StringComparison.Ordinal) && transition.SecondaryTargetId == projection.SelectedEventId,
            "questCompletedCandidate" => transition.CommandType == "quest/start" && transition.TargetId == projection.SelectedQuestId,
            _ => false
        };
    }

    private static void RequireTransition(
        IReadOnlyList<UnityRuntimeStateTransition> transitions,
        string stateKey,
        string commandType,
        string targetId,
        ICollection<UnityRuntimeStateDiagnostic> diagnostics)
    {
        if (!transitions.Any(transition =>
            transition.StateKey == stateKey &&
            transition.CommandType == commandType &&
            (string.IsNullOrWhiteSpace(targetId) || transition.TargetId == targetId)))
        {
            diagnostics.Add(Diagnostic("error", "unity_runtime_state.transition.required_missing", stateKey, "Required state transition was not tied to the expected generated command."));
        }
    }

    private static bool HasTransition(IReadOnlyList<UnityRuntimeStateTransition> transitions, string stateKey) =>
        transitions.Any(transition => transition.StateKey == stateKey);

    private static IReadOnlyList<UnityRuntimeStateTransition> ParseTransitions(IReadOnlyDictionary<string, string> values)
    {
        var count = ParseInt(values, "alpha_runtime.command_state_transition_count");
        var transitions = new List<UnityRuntimeStateTransition>();
        for (var index = 0; index < count; index++)
        {
            transitions.Add(new UnityRuntimeStateTransition
            {
                Index = index,
                CommandId = values.GetValueOrDefault($"alpha_runtime.command_state_transition.{index}.command_id", string.Empty),
                CommandType = values.GetValueOrDefault($"alpha_runtime.command_state_transition.{index}.command_type", string.Empty),
                TargetId = values.GetValueOrDefault($"alpha_runtime.command_state_transition.{index}.target_id", string.Empty),
                SecondaryTargetId = values.GetValueOrDefault($"alpha_runtime.command_state_transition.{index}.secondary_target_id", string.Empty),
                StateKey = values.GetValueOrDefault($"alpha_runtime.command_state_transition.{index}.state_key", string.Empty),
                Before = values.GetValueOrDefault($"alpha_runtime.command_state_transition.{index}.before", string.Empty),
                After = values.GetValueOrDefault($"alpha_runtime.command_state_transition.{index}.after", string.Empty)
            });
        }

        return transitions;
    }

    private static void ApplyExpected(
        UnityGeneratedSceneCommandHint command,
        UnityGeneratedSceneProjection projection,
        MutableState state)
    {
        if (command.CommandType == "quest/start")
        {
            state.QuestStarted = command.TargetId == projection.SelectedQuestId;
        }
        else if (command.CommandType == "dialogue/open")
        {
            state.DialogueOpened = command.TargetId == projection.SelectedDialogueId;
        }
        else if (command.CommandType == "dialogue/choose")
        {
            state.DialogueChoiceSelected = !string.IsNullOrWhiteSpace(command.TargetId);
        }
        else if (command.CommandType == "event/add_item")
        {
            state.ItemObtained = command.TargetId == projection.SelectedItemId;
            state.InventoryItemCount = state.ItemObtained ? 1 : state.InventoryItemCount;
            state.EventApplied = command.SecondaryTargetId == projection.SelectedEventId;
        }
        else if (command.CommandType == "loot/roll")
        {
            state.ItemObtained = !string.IsNullOrWhiteSpace(projection.SelectedItemId);
            state.InventoryItemCount = state.ItemObtained ? 1 : state.InventoryItemCount;
        }

        state.QuestCompletedCandidate = state.QuestStarted &&
            state.DialogueOpened &&
            state.DialogueChoiceSelected &&
            state.ItemObtained &&
            state.EventApplied;
    }

    private static int AppendExpectedTransitions(
        ICollection<string> lines,
        int transitionIndex,
        UnityGeneratedSceneCommandHint command,
        MutableState before,
        MutableState after)
    {
        transitionIndex = AppendChanged(lines, transitionIndex, command, "questStarted", before.QuestStarted.ToString().ToLowerInvariant(), after.QuestStarted.ToString().ToLowerInvariant());
        transitionIndex = AppendChanged(lines, transitionIndex, command, "questCompletedCandidate", before.QuestCompletedCandidate.ToString().ToLowerInvariant(), after.QuestCompletedCandidate.ToString().ToLowerInvariant());
        transitionIndex = AppendChanged(lines, transitionIndex, command, "dialogueOpened", before.DialogueOpened.ToString().ToLowerInvariant(), after.DialogueOpened.ToString().ToLowerInvariant());
        transitionIndex = AppendChanged(lines, transitionIndex, command, "dialogueChoiceSelected", before.DialogueChoiceSelected.ToString().ToLowerInvariant(), after.DialogueChoiceSelected.ToString().ToLowerInvariant());
        transitionIndex = AppendChanged(lines, transitionIndex, command, "itemObtained", before.ItemObtained.ToString().ToLowerInvariant(), after.ItemObtained.ToString().ToLowerInvariant());
        transitionIndex = AppendChanged(lines, transitionIndex, command, "inventoryItemCount", before.InventoryItemCount.ToString(), after.InventoryItemCount.ToString());
        transitionIndex = AppendChanged(lines, transitionIndex, command, "eventApplied", before.EventApplied.ToString().ToLowerInvariant(), after.EventApplied.ToString().ToLowerInvariant());
        return transitionIndex;
    }

    private static int AppendChanged(
        ICollection<string> lines,
        int transitionIndex,
        UnityGeneratedSceneCommandHint command,
        string stateKey,
        string before,
        string after)
    {
        if (before == after)
        {
            return transitionIndex;
        }

        lines.Add("alpha_runtime.command_state_transition." + transitionIndex + ".command_id=" + command.CommandId);
        lines.Add("alpha_runtime.command_state_transition." + transitionIndex + ".command_type=" + command.CommandType);
        lines.Add("alpha_runtime.command_state_transition." + transitionIndex + ".target_id=" + command.TargetId);
        lines.Add("alpha_runtime.command_state_transition." + transitionIndex + ".secondary_target_id=" + command.SecondaryTargetId);
        lines.Add("alpha_runtime.command_state_transition." + transitionIndex + ".state_key=" + stateKey);
        lines.Add("alpha_runtime.command_state_transition." + transitionIndex + ".before=" + before);
        lines.Add("alpha_runtime.command_state_transition." + transitionIndex + ".after=" + after);
        return transitionIndex + 1;
    }

    private static IEnumerable<string> ReplaceLine(IEnumerable<string> lines, string prefix, string replacement)
    {
        var replaced = false;
        foreach (var line in lines)
        {
            if (!replaced && line.StartsWith(prefix, StringComparison.Ordinal))
            {
                replaced = true;
                yield return replacement;
            }
            else
            {
                yield return line;
            }
        }
    }

    private static UnityRuntimeStateInvalidScenario InvalidScenario(
        string id,
        IReadOnlyList<UnityRuntimeStateDiagnostic> diagnostics) =>
        new()
        {
            ScenarioId = id,
            ExpectedValid = false,
            ActualValid = diagnostics.All(item => item.Severity != "error"),
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static string RenderReport(UnityRuntimeStateLoopReport report)
    {
        var lines = new List<string>
        {
            "# Unity Generated Runtime State Loop Report",
            string.Empty,
            $"- Accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- Final status: {report.FinalStatus}",
            $"- Previous gate: {report.PreviousAcceptedGate}",
            $"- Completed slices: {string.Join(", ", report.CompletedSlices)}",
            $"- Product smoke route: {report.ProductSmokeRoute}",
            $"- Selected package: {report.SelectedPackageId}",
            $"- Selected style: {report.SelectedStyleId}",
            $"- Selected thread: {report.SelectedThreadId}",
            $"- Runtime state loop verified: {report.RuntimeStateLoopVerified.ToString().ToLowerInvariant()}",
            $"- State transition trace verified: {report.StateTransitionTraceVerified.ToString().ToLowerInvariant()}",
            $"- Quest/dialogue/inventory/event: {report.QuestStateVerified.ToString().ToLowerInvariant()} / {report.DialogueStateVerified.ToString().ToLowerInvariant()} / {report.InventoryStateVerified.ToString().ToLowerInvariant()} / {report.EventStateVerified.ToString().ToLowerInvariant()}",
            $"- Movement/focus/interaction/play-loop: {report.MovementVerified.ToString().ToLowerInvariant()} / {report.FocusVerified.ToString().ToLowerInvariant()} / {report.InteractionVerified.ToString().ToLowerInvariant()} / {report.PlayLoopVerified.ToString().ToLowerInvariant()}",
            $"- Command/state transition count: {report.State.CommandExecutionTrace.Count}",
            $"- State-loop hash: {report.StateLoopHash}",
            $"- Deterministic report hash: {report.DeterministicHash}",
            $"- Build manifest hash: {report.BuildManifestHash}",
            $"- Invalid/fake/leak scenarios rejected: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            string.Empty,
            "## Diagnostics",
            string.Empty
        };
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(
        UnityRuntimeStateLoopReport report,
        string alphaVerificationMarkdown)
    {
        var lines = new List<string>
        {
            "# Unity Generated Runtime State Loop Verification",
            string.Empty,
            "Stopped at:",
            string.Empty,
            "```text",
            FinalGate,
            "```",
            string.Empty,
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Final gate remains required: {FinalGate}",
            $"- State artifact: {RelativeOutputDirectory}/{StateJsonFileName}",
            $"- Report artifact: {RelativeOutputDirectory}/{ReportJsonFileName}",
            $"- Selected package/style/thread: {report.SelectedPackageId} / {report.SelectedStyleId} / {report.SelectedThreadId}",
            $"- Runtime state fields: questStarted, questCompletedCandidate, dialogueOpened, dialogueChoiceSelected, itemObtained, inventoryItemCount, eventApplied, lastCommandId, lastCommandType, lastCommandTargetId, statusText",
            $"- Command/state transition count: {report.State.CommandExecutionTrace.Count}",
            $"- State-loop hash: {report.StateLoopHash}",
            $"- Deterministic report hash: {report.DeterministicHash}",
            $"- Build manifest hash: {report.BuildManifestHash}",
            $"- Final gate status: required, not passed",
            $"- Future post-goal work started: false",
            string.Empty,
            "## Underlying Alpha Build Verification",
            string.Empty,
            SanitizeEmbeddedAlphaVerification(alphaVerificationMarkdown).TrimEnd()
        };

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string SanitizeEmbeddedAlphaVerification(string markdown)
    {
        var lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            if (line.StartsWith("- Unity command:", StringComparison.Ordinal) ||
                line.StartsWith("- Launch command:", StringComparison.Ordinal) ||
                line.StartsWith("- Play-loop command:", StringComparison.Ordinal))
            {
                lines[index] = line[..line.IndexOf(':')] + ": (omitted; local machine paths are not part of compact deterministic root artifacts)";
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static Dictionary<string, string> ParseKeyValueLog(IEnumerable<string> lines)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            var separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            values[line[..separator]] = line[(separator + 1)..];
        }

        return values;
    }

    private static int ParseInt(IReadOnlyDictionary<string, string> values, string key) =>
        values.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : 0;

    private static string ResolveRepositoryRoot(string projectRoot, string overrideRoot)
    {
        if (!string.IsNullOrWhiteSpace(overrideRoot))
        {
            return Path.GetFullPath(overrideRoot);
        }

        var current = new DirectoryInfo(projectRoot);
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? projectRoot;
    }

    private static void EnsureContained(string root, string path)
    {
        if (!IsContained(root, path))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }

    private static bool IsContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        return pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<UnityRuntimeStateDiagnostic> ConvertDiagnostics(IEnumerable<UnityGeneratedSceneDiagnostic> diagnostics) =>
        diagnostics.Select(item => Diagnostic(item.Severity, item.Code, item.Target, item.Message)).ToList();

    private static UnityRuntimeStateDiagnostic ConvertDiagnostic(AlphaBuildDiagnostic diagnostic) =>
        Diagnostic(diagnostic.Severity, diagnostic.Code, diagnostic.Target, diagnostic.Message);

    private static IReadOnlyList<UnityRuntimeStateDiagnostic> SortDiagnostics(IEnumerable<UnityRuntimeStateDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static UnityRuntimeStateDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    private static string ComputeHash(string text) => ComputeHash(Encoding.UTF8.GetBytes(text));

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private sealed record MutableState
    {
        public bool QuestStarted { get; set; }
        public bool QuestCompletedCandidate { get; set; }
        public bool DialogueOpened { get; set; }
        public bool DialogueChoiceSelected { get; set; }
        public bool ItemObtained { get; set; }
        public int InventoryItemCount { get; set; }
        public bool EventApplied { get; set; }

        public MutableState Copy() => this with { };
    }
}

public sealed record UnityRuntimeStateLoopOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityBuild { get; init; }
    public bool LaunchBuiltPlayer { get; init; }
    public bool PreserveExistingBuildOutputForValidation { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 90;
}

public sealed record UnityRuntimeStateLoopAcceptanceResult
{
    public UnityRuntimeStateLoopReport Report { get; init; } = new();
    public string StateJson { get; init; } = string.Empty;
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record UnityRuntimeStateLoopWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StateJsonPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record UnityRuntimeStateLoopReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public AlphaRunnableBuildReport AlphaBuild { get; init; } = new();
    public UnityGeneratedSceneProjection Projection { get; init; } = new();
    public UnityRuntimeStateLoopState State { get; init; } = new();
    public string SelectedPackageId { get; init; } = string.Empty;
    public string SelectedStyleId { get; init; } = string.Empty;
    public string SelectedThreadId { get; init; } = string.Empty;
    public string SelectedMapId { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedSceneNodeIds { get; init; } = [];
    public IReadOnlyList<UnityGeneratedSceneCommandHint> SelectedCommandHints { get; init; } = [];
    public bool RuntimeStateLoopVerified { get; init; }
    public bool StateTransitionTraceVerified { get; init; }
    public bool QuestStateVerified { get; init; }
    public bool DialogueStateVerified { get; init; }
    public bool InventoryStateVerified { get; init; }
    public bool EventStateVerified { get; init; }
    public bool MovementVerified { get; init; }
    public bool FocusVerified { get; init; }
    public bool InteractionVerified { get; init; }
    public bool PlayLoopVerified { get; init; }
    public bool PreviousSceneProjectionEvidenceVerified { get; init; }
    public bool SceneProjectionVerified { get; init; }
    public bool FirewallSafeBuildVerified { get; init; }
    public UnityRuntimeStateInvalidMatrix InvalidMatrix { get; init; } = new();
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool NoExternalProviderLlmRagLuaMedia { get; init; }
    public bool RuntimePreviewDependency { get; init; }
    public string StateLoopHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public string BuildManifestHash { get; init; } = string.Empty;
    public string DeterministicReportRelativePath { get; init; } = string.Empty;
    public IReadOnlyList<UnityRuntimeStateDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityRuntimeStateLoopState
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string SelectedPackageId { get; init; } = string.Empty;
    public string SelectedStyleId { get; init; } = string.Empty;
    public string SelectedThreadId { get; init; } = string.Empty;
    public string SelectedMapId { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedSceneNodeIds { get; init; } = [];
    public IReadOnlyList<UnityGeneratedSceneCommandHint> SelectedCommandHints { get; init; } = [];
    public string PlayerPositionBefore { get; init; } = string.Empty;
    public string PlayerPositionAfter { get; init; } = string.Empty;
    public string BlockedMovementPosition { get; init; } = string.Empty;
    public string FocusedNodeId { get; init; } = string.Empty;
    public string FocusedSourceId { get; init; } = string.Empty;
    public bool QuestStateBefore { get; init; }
    public bool QuestStateAfter { get; init; }
    public bool QuestCompletedCandidate { get; init; }
    public bool DialogueOpenedBefore { get; init; }
    public bool DialogueOpenedAfter { get; init; }
    public bool DialogueChoiceSelectedBefore { get; init; }
    public bool DialogueChoiceSelectedAfter { get; init; }
    public bool ItemObtainedBefore { get; init; }
    public bool ItemObtainedAfter { get; init; }
    public int InventoryItemCountBefore { get; init; }
    public int InventoryItemCountAfter { get; init; }
    public bool EventAppliedBefore { get; init; }
    public bool EventAppliedAfter { get; init; }
    public string LastCommandId { get; init; } = string.Empty;
    public string LastCommandType { get; init; } = string.Empty;
    public string LastCommandTargetId { get; init; } = string.Empty;
    public string StatusText { get; init; } = string.Empty;
    public IReadOnlyList<UnityRuntimeStateTransition> CommandExecutionTrace { get; init; } = [];
    public string StateHash { get; init; } = string.Empty;
}

public sealed record UnityRuntimeStateLoopProof
{
    public bool RuntimeStateLoopVerified { get; init; }
    public bool StateTransitionTraceVerified { get; init; }
    public bool QuestStateVerified { get; init; }
    public bool DialogueStateVerified { get; init; }
    public bool InventoryStateVerified { get; init; }
    public bool EventStateVerified { get; init; }
    public bool MovementVerified { get; init; }
    public bool FocusVerified { get; init; }
    public bool InteractionVerified { get; init; }
    public bool PlayLoopVerified { get; init; }
    public string PlayerPositionBefore { get; init; } = string.Empty;
    public string PlayerPositionAfter { get; init; } = string.Empty;
    public string BlockedMovementPosition { get; init; } = string.Empty;
    public string FocusedNodeId { get; init; } = string.Empty;
    public string FocusedSourceId { get; init; } = string.Empty;
    public string LastCommandId { get; init; } = string.Empty;
    public string LastCommandType { get; init; } = string.Empty;
    public string LastCommandTargetId { get; init; } = string.Empty;
    public string StatusText { get; init; } = string.Empty;
    public int CommandStateTransitionCount { get; init; }
    public IReadOnlyList<UnityRuntimeStateTransition> Transitions { get; init; } = [];
    public IReadOnlyList<UnityRuntimeStateDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityRuntimeStateTransition
{
    public int Index { get; init; }
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string SecondaryTargetId { get; init; } = string.Empty;
    public string StateKey { get; init; } = string.Empty;
    public string Before { get; init; } = string.Empty;
    public string After { get; init; } = string.Empty;
}

public sealed record UnityRuntimeStatePreviousEvidenceProof
{
    public bool Passed { get; init; }
    public string ProjectionRelativePath { get; init; } = string.Empty;
    public string ReportRelativePath { get; init; } = string.Empty;
    public IReadOnlyList<UnityRuntimeStateDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityRuntimeStateFirewallProof
{
    public string BuildOptions { get; init; } = string.Empty;
    public bool StaticChecksPassed { get; init; }
    public bool BuildMetadataPresent { get; init; }
    public bool FirewallSafeBuildVerified { get; init; }
    public IReadOnlyList<UnityRuntimeStateDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityRuntimeStateInvalidMatrix
{
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public IReadOnlyList<UnityRuntimeStateInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<UnityRuntimeStateDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityRuntimeStateInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<UnityRuntimeStateDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityRuntimeStateDiagnostic
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
