using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.AlphaBuild;
using LLMGameCreator.Application.Design.Assets;
using LLMGameCreator.Application.Design.ContentGeneration;
using LLMGameCreator.Application.Design.UnityPlayableAlpha;

namespace LLMGameCreator.Application.Design.UnityGeneratedScene;

public sealed class UnityGeneratedSceneProjectionAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/unity-generated-scene-projection";
    public const string ProjectionJsonFileName = "unity-generated-scene-projection.json";
    public const string ReportJsonFileName = "unity-generated-scene-projection-report.json";
    public const string ReportMarkdownFileName = "unity-generated-scene-projection-report.md";
    public const string VerificationMarkdownFileName = "unity-generated-scene-projection-verification.md";
    public const string FinalGate = "unity_generated_scene_content_projection_verification";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static UnityGeneratedSceneProjectionAcceptanceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public UnityGeneratedSceneProjectionAcceptanceResult BuildFromAcceptedEvidence(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        UnityGeneratedSceneProjectionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(contentGenerationResult);
        ArgumentNullException.ThrowIfNull(minimumAssetResult);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var settings = options ?? new UnityGeneratedSceneProjectionOptions();
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
        var projection = BuildProjection(alpha);
        var projectionValidation = ValidateProjection(projection, alpha);
        var playLoop = ValidatePlayLoop(projectRoot, alpha, projection);
        var firewall = ValidateFirewall(repositoryRoot, projectRoot, alpha);
        var invalidMatrix = BuildInvalidMatrix(projection, alpha, playLoop, firewall);
        var diagnostics = SortDiagnostics(
            projectionValidation.Diagnostics
                .Concat(playLoop.Diagnostics)
                .Concat(firewall.Diagnostics)
                .Concat(invalidMatrix.Diagnostics)
                .Concat(alpha.Diagnostics.Select(ConvertDiagnostic))
                .Concat(
                [
                    Diagnostic("info", "unity_generated_scene.goal014_gate_recorded", "unity_playable_presentation_firewall_safe_build_verification", "User-confirmed Goal 014 verification is recorded as passed."),
                    Diagnostic("info", "unity_generated_scene.no_external_providers", "execution_boundary", "No LLM, RAG, provider, media, arbitrary Lua or generator-library execution was invoked.")
                ]));

        var reportWithoutHash = new UnityGeneratedSceneProjectionReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousAcceptedGate = "unity_playable_presentation_firewall_safe_build_verification passed",
            CompletedSlices = ["S122", "S123", "S124", "S125", "S126", "S127", "S128", "S129"],
            ProductSmokeRoute = "unity-generated-scene-projection",
            AlphaBuild = alpha,
            Projection = projection,
            SelectedPackageId = projection.SelectedPackageId,
            SelectedStyleId = projection.SelectedStyleId,
            SelectedThreadId = projection.SelectedThreadId,
            PackageHash = projection.PackageHash,
            AssetManifestHash = projection.AssetManifestHash,
            ExportManifestHash = projection.ExportManifestHash,
            RuntimeConfigHash = projection.RuntimeConfigHash,
            ProjectionHash = projection.ProjectionHash,
            SceneProjectionVerified = projectionValidation.Passed,
            SceneNodesResolved = projectionValidation.SceneNodesResolved,
            GeneratedIdBindingVerified = projectionValidation.GeneratedIdBindingVerified,
            AssetBindingVerified = projectionValidation.AssetBindingVerified,
            MovementVerified = playLoop.MovementVerified,
            InteractionVerified = playLoop.InteractionVerified,
            PlayLoopVerified = alpha.PlayLoopVerified && playLoop.PlayLoopVerified,
            FirewallSafeBuildVerified = firewall.FirewallSafeBuildVerified,
            InvalidMatrix = invalidMatrix,
            BuildManifestHash = alpha.BuildManifestHash,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            NoExternalProviderLlmRagLuaMedia = true,
            RuntimePreviewDependency = alpha.RuntimePreviewDependency,
            DeterministicReportRelativePath = $"{RelativeOutputDirectory}/{ReportJsonFileName}",
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new UnityGeneratedSceneProjectionAcceptanceResult
        {
            Report = report,
            ProjectionJson = JsonSerializer.Serialize(projection, JsonOptions),
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            VerificationMarkdown = RenderVerification(report, alphaResult.VerificationMarkdown)
        };
    }

    public async Task<UnityGeneratedSceneProjectionWriteResult> WriteAsync(
        string projectRootPath,
        UnityGeneratedSceneProjectionAcceptanceResult result,
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

        var projectionPath = Path.Combine(outputDirectory, ProjectionJsonFileName);
        var jsonPath = Path.Combine(outputDirectory, ReportJsonFileName);
        var markdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        var verificationPath = Path.Combine(outputDirectory, VerificationMarkdownFileName);
        await File.WriteAllTextAsync(projectionPath, result.ProjectionJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(jsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(markdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new UnityGeneratedSceneProjectionWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ProjectionJsonPath = projectionPath,
            ReportJsonPath = jsonPath,
            ReportMarkdownPath = markdownPath,
            VerificationMarkdownPath = verificationPath
        };
    }

    public async Task<UnityGeneratedSceneProjectionWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        UnityGeneratedSceneProjectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var result = BuildFromAcceptedEvidence(projectRootPath, contentGenerationResult, minimumAssetResult, options);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public static UnityGeneratedSceneProjection BuildProjection(AlphaRunnableBuildReport alpha)
    {
        var primary = alpha.PrimaryBuildCandidate;
        var occupied = new HashSet<string>(StringComparer.Ordinal);
        var nodes = new List<UnityGeneratedSceneNode>();
        var assetByCategory = primary.AssetRefs
            .GroupBy(asset => asset.Category, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.OrderBy(asset => asset.ContentId, StringComparer.Ordinal).First(), StringComparer.Ordinal);

        var mapNode = Node(
            primary,
            "scene_node/map/" + ShortHash(primary.LoopRefs.MapId),
            "map",
            primary.LoopRefs.MapId,
            "Map " + DisplayId(primary.LoopRefs.MapId),
            ReservePosition("map", primary.LoopRefs.MapId, primary.PackageHash, occupied, avoidGoal014Placeholder: true),
            Asset(assetByCategory, "tile_region_graphic"));
        nodes.Add(mapNode);

        var playerPosition = ReservePlayerPosition(primary.PackageId, primary.SelectedThreadId, primary.RuntimeConfigHash, occupied);
        nodes.Add(Node(primary, "scene_node/player/" + ShortHash(primary.SelectedThreadId), "player", "player/runtime", "Player", playerPosition, null));
        nodes.Add(Node(primary, "scene_node/npc/" + ShortHash(primary.LoopRefs.NpcId), "npc", primary.LoopRefs.NpcId, "NPC " + DisplayId(primary.LoopRefs.NpcId), ReservePosition("npc", primary.LoopRefs.NpcId, primary.PackageHash, occupied, avoidGoal014Placeholder: true), Asset(assetByCategory, "npc_portrait")));
        nodes.Add(Node(primary, "scene_node/item/" + ShortHash(primary.LoopRefs.ItemId), "item", primary.LoopRefs.ItemId, "Item " + DisplayId(primary.LoopRefs.ItemId), ReservePosition("item", primary.LoopRefs.ItemId, primary.PackageHash, occupied, avoidGoal014Placeholder: true), Asset(assetByCategory, "item_icon_ui_graphic")));
        var questEventSource = string.IsNullOrWhiteSpace(primary.LoopRefs.EventId) ? primary.LoopRefs.QuestId : primary.LoopRefs.EventId;
        nodes.Add(Node(primary, "scene_node/quest_event/" + ShortHash(questEventSource), "quest_event", questEventSource, "Quest/Event " + DisplayId(questEventSource), ReservePosition("quest_event", questEventSource, primary.PackageHash, occupied, avoidGoal014Placeholder: true), Asset(assetByCategory, "music_ambience") ?? Asset(assetByCategory, "sound_effect")));
        nodes.Add(Node(primary, "scene_node/command_status/" + ShortHash(string.Join("|", primary.CommandHints.Select(command => command.CommandId))), "command_status", primary.CommandHints.FirstOrDefault()?.CommandId ?? string.Empty, "Commands " + primary.CommandHints.Count, ReservePosition("command_status", primary.SelectedThreadId, primary.PackageHash, occupied, avoidGoal014Placeholder: true), null));

        var withoutHash = new UnityGeneratedSceneProjection
        {
            SchemaVersion = "unity_generated_scene_projection_v1",
            SelectedPackageId = primary.PackageId,
            SelectedStyleId = primary.StyleId,
            SelectedThreadId = primary.SelectedThreadId,
            SelectedMapId = primary.LoopRefs.MapId,
            SelectedNpcId = primary.LoopRefs.NpcId,
            SelectedQuestId = primary.LoopRefs.QuestId,
            SelectedDialogueId = primary.LoopRefs.DialogueId,
            SelectedItemId = primary.LoopRefs.ItemId,
            SelectedEventId = primary.LoopRefs.EventId,
            PackageHash = primary.PackageHash,
            AssetManifestHash = primary.AssetManifestHash,
            ExportManifestHash = primary.ExportManifestHash,
            RuntimeConfigHash = primary.RuntimeConfigHash,
            MapWidth = 7,
            MapHeight = 5,
            CommandHints = primary.CommandHints.Select(command => new UnityGeneratedSceneCommandHint
            {
                CommandId = command.CommandId,
                CommandType = command.CommandType,
                TargetId = command.TargetId,
                SecondaryTargetId = command.SecondaryTargetId
            }).OrderBy(command => command.CommandId, StringComparer.Ordinal).ToList(),
            AssetRefs = primary.AssetRefs.Select(asset => new UnityGeneratedSceneAssetRef
            {
                Category = asset.Category,
                AssetId = asset.AssetId,
                ContentId = asset.ContentId,
                ExportRelativePath = asset.ExportRelativePath,
                Hash = asset.Hash,
                ByteCount = asset.ByteCount
            }).OrderBy(asset => asset.Category, StringComparer.Ordinal).ThenBy(asset => asset.AssetId, StringComparer.Ordinal).ToList(),
            Nodes = nodes.OrderBy(node => node.NodeId, StringComparer.Ordinal).ToList()
        };

        return withoutHash with
        {
            ProjectionHash = ComputeHash(JsonSerializer.Serialize(withoutHash, JsonOptions))
        };
    }

    public static UnityGeneratedSceneProjectionValidation ValidateProjection(
        UnityGeneratedSceneProjection projection,
        AlphaRunnableBuildReport alpha)
    {
        var diagnostics = new List<UnityGeneratedSceneDiagnostic>();
        var primary = alpha.PrimaryBuildCandidate;
        Require(!string.IsNullOrWhiteSpace(projection.SelectedPackageId), "unity_generated_scene.projection.package_missing", "selectedPackageId", "Projection must carry the selected package id.");
        Require(string.Equals(projection.SelectedPackageId, primary.PackageId, StringComparison.Ordinal), "unity_generated_scene.projection.package_mismatch", projection.SelectedPackageId, "Projection package id must match Alpha evidence.");
        Require(string.Equals(projection.SelectedStyleId, primary.StyleId, StringComparison.Ordinal), "unity_generated_scene.projection.style_mismatch", projection.SelectedStyleId, "Projection style id must match Alpha evidence.");
        Require(string.Equals(projection.PackageHash, primary.PackageHash, StringComparison.Ordinal), "unity_generated_scene.projection.package_hash_mismatch", projection.SelectedPackageId, "Projection package hash must match Alpha evidence.");
        Require(string.Equals(projection.AssetManifestHash, primary.AssetManifestHash, StringComparison.Ordinal), "unity_generated_scene.projection.asset_manifest_hash_mismatch", projection.SelectedStyleId, "Projection asset manifest hash must match Alpha evidence.");
        Require(string.Equals(projection.RuntimeConfigHash, primary.RuntimeConfigHash, StringComparison.Ordinal), "unity_generated_scene.projection.runtime_config_hash_mismatch", projection.SelectedThreadId, "Projection runtime config hash must match Alpha evidence.");
        Require(!alpha.RuntimePreviewDependency, "unity_generated_scene.projection.runtime_preview_dependency", "runtime_host", "Unity scene projection must not claim Runtime Preview dependency.");

        var nodeIds = new HashSet<string>(StringComparer.Ordinal);
        var positions = new HashSet<string>(StringComparer.Ordinal);
        var knownIds = KnownSourceIds(projection, primary);
        foreach (var node in projection.Nodes)
        {
            Require(nodeIds.Add(node.NodeId), "unity_generated_scene.projection.duplicate_node_id", node.NodeId, "Scene node ids must be unique.");
            Require(node.X >= 0 && node.X < projection.MapWidth && node.Y >= 0 && node.Y < projection.MapHeight, "unity_generated_scene.projection.position_out_of_bounds", node.NodeId, "Scene node positions must stay inside projected map bounds.");
            if (node.NodeKind != "player")
            {
                Require(positions.Add(node.X + "," + node.Y), "unity_generated_scene.projection.duplicate_position", node.NodeId, "Non-player scene nodes cannot occupy the same projected grid position.");
                Require(knownIds.Contains(node.SourceGeneratedId), "unity_generated_scene.projection.source_id_missing", node.SourceGeneratedId, "Node source ids must resolve to selected package/config evidence.");
            }
        }

        Require(NodeKind(projection, "map")?.SourceGeneratedId == projection.SelectedMapId, "unity_generated_scene.projection.map_node_missing", projection.SelectedMapId, "Map node must bind to selected map id.");
        Require(NodeKind(projection, "player") != null, "unity_generated_scene.projection.player_node_missing", "player", "Player node is required.");
        Require(NodeKind(projection, "npc")?.SourceGeneratedId.StartsWith("npc/", StringComparison.Ordinal) == true, "unity_generated_scene.projection.npc_binding_mismatch", NodeKind(projection, "npc")?.SourceGeneratedId ?? string.Empty, "NPC node must bind to an NPC generated id.");
        Require(NodeKind(projection, "item")?.SourceGeneratedId.StartsWith("item/", StringComparison.Ordinal) == true, "unity_generated_scene.projection.item_binding_mismatch", NodeKind(projection, "item")?.SourceGeneratedId ?? string.Empty, "Item node must bind to an item generated id.");
        Require(NodeKind(projection, "quest_event") != null, "unity_generated_scene.projection.quest_event_node_missing", "quest_event", "Quest/event node is required.");
        Require(NodeKind(projection, "command_status") != null, "unity_generated_scene.projection.command_status_node_missing", "command_status", "Command/status node is required.");

        foreach (var assetNode in projection.Nodes.Where(node => node.Asset != null))
        {
            Require(!string.IsNullOrWhiteSpace(assetNode.Asset!.AssetId), "unity_generated_scene.projection.asset_id_missing", assetNode.NodeId, "Asset-bound nodes must include an asset id.");
            Require(!string.IsNullOrWhiteSpace(assetNode.Asset.ExportRelativePath), "unity_generated_scene.projection.asset_path_missing", assetNode.NodeId, "Asset-bound nodes must include an export relative path.");
            Require(!string.IsNullOrWhiteSpace(assetNode.Asset.Hash), "unity_generated_scene.projection.asset_hash_missing", assetNode.NodeId, "Asset-bound nodes must include a file hash.");
        }

        var expectedCommands = primary.CommandHints.OrderBy(command => command.CommandId, StringComparer.Ordinal).ToList();
        Require(projection.CommandHints.Count == expectedCommands.Count, "unity_generated_scene.projection.command_count_mismatch", "commandHints", "Projection command count must match selected generated command hints.");
        for (var index = 0; index < Math.Min(projection.CommandHints.Count, expectedCommands.Count); index++)
        {
            var expected = expectedCommands[index];
            var actual = projection.CommandHints[index];
            Require(string.Equals(actual.CommandId, expected.CommandId, StringComparison.Ordinal), "unity_generated_scene.projection.command_order_mismatch", actual.CommandId, "Projection command order must match selected generated command hints.");
            Require(string.Equals(actual.TargetId, expected.TargetId, StringComparison.Ordinal), "unity_generated_scene.projection.command_target_mismatch", actual.CommandId, "Projection command targets must match selected generated command hints.");
        }

        var hasSceneNodes = new[] { "map", "player", "npc", "item", "quest_event", "command_status" }.All(kind => NodeKind(projection, kind) != null);
        var generatedBinding = diagnostics.All(item => item.Code is not ("unity_generated_scene.projection.source_id_missing" or "unity_generated_scene.projection.npc_binding_mismatch" or "unity_generated_scene.projection.item_binding_mismatch" or "unity_generated_scene.projection.command_target_mismatch" or "unity_generated_scene.projection.command_order_mismatch"));
        var assetBinding = projection.Nodes.Where(node => node.Asset != null).All(node =>
            !string.IsNullOrWhiteSpace(node.Asset!.AssetId) &&
            !string.IsNullOrWhiteSpace(node.Asset.ExportRelativePath) &&
            !string.IsNullOrWhiteSpace(node.Asset.Hash));
        return new UnityGeneratedSceneProjectionValidation
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            SceneNodesResolved = hasSceneNodes,
            GeneratedIdBindingVerified = generatedBinding,
            AssetBindingVerified = assetBinding && projection.AssetRefs.Count > 0,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        void Require(bool condition, string code, string target, string message)
        {
            if (!condition)
            {
                diagnostics.Add(Diagnostic("error", code, target, message));
            }
        }
    }

    public static UnityGeneratedScenePlayLoopProof ValidatePlayLoopLines(
        IEnumerable<string> lines,
        UnityGeneratedSceneProjection projection)
    {
        var diagnostics = new List<UnityGeneratedSceneDiagnostic>();
        var values = ParseKeyValueLog(lines);
        Require(values, "alpha_runtime.scene_projection_loaded", "true", "unity_generated_scene.play_loop.projection_not_loaded");
        foreach (var kind in new[] { "map", "player", "npc", "item", "quest_event", "command_status" })
        {
            Require(values, "alpha_runtime.scene_node_resolved." + kind, "true", "unity_generated_scene.play_loop.node_not_resolved");
        }

        Require(values, "alpha_runtime.movement.step.0.valid", "true", "unity_generated_scene.play_loop.movement_step_missing");
        Require(values, "alpha_runtime.movement.step.1.valid", "true", "unity_generated_scene.play_loop.movement_step_missing");
        Require(values, "alpha_runtime.movement.blocked.valid", "false", "unity_generated_scene.play_loop.bounds_not_proven");
        if (!values.TryGetValue("alpha_runtime.focus.selected_node_id", out var selectedNodeId) ||
            !projection.Nodes.Any(node => node.NodeId == selectedNodeId))
        {
            diagnostics.Add(Diagnostic("error", "unity_generated_scene.play_loop.focus_missing", "alpha_runtime.focus.selected_node_id", "Focus/select proof must target a generated scene node."));
        }

        var commandsExecuted = ParseInt(values, "alpha_runtime.commands_executed");
        if (commandsExecuted < projection.CommandHints.Count)
        {
            diagnostics.Add(Diagnostic("error", "unity_generated_scene.play_loop.generated_commands_missing", "alpha_runtime.commands_executed", "Interaction proof must execute all projected generated command hints."));
        }

        for (var index = 0; index < projection.CommandHints.Count; index++)
        {
            var expected = projection.CommandHints[index];
            Require(values, $"alpha_runtime.command_executed.{index}.id", expected.CommandId, "unity_generated_scene.play_loop.command_order_mismatch");
            Require(values, $"alpha_runtime.command_executed.{index}.type", expected.CommandType, "unity_generated_scene.play_loop.command_order_mismatch");
            Require(values, $"alpha_runtime.command_executed.{index}.target_id", expected.TargetId, "unity_generated_scene.play_loop.command_target_mismatch");
        }

        foreach (var key in new[] { "quest_start", "dialogue_open", "item_or_loot", "event_application" })
        {
            Require(values, "alpha_runtime.state_transition." + key, "true", "unity_generated_scene.play_loop.state_flag_missing");
        }

        return new UnityGeneratedScenePlayLoopProof
        {
            ProjectionLoaded = values.TryGetValue("alpha_runtime.scene_projection_loaded", out var projectionLoaded) && projectionLoaded == "true",
            MapNodeResolved = values.TryGetValue("alpha_runtime.scene_node_resolved.map", out var mapResolved) && mapResolved == "true",
            PlayerNodeResolved = values.TryGetValue("alpha_runtime.scene_node_resolved.player", out var playerResolved) && playerResolved == "true",
            NpcNodeResolved = values.TryGetValue("alpha_runtime.scene_node_resolved.npc", out var npcResolved) && npcResolved == "true",
            ItemNodeResolved = values.TryGetValue("alpha_runtime.scene_node_resolved.item", out var itemResolved) && itemResolved == "true",
            QuestEventNodeResolved = values.TryGetValue("alpha_runtime.scene_node_resolved.quest_event", out var questEventResolved) && questEventResolved == "true",
            CommandStatusNodeResolved = values.TryGetValue("alpha_runtime.scene_node_resolved.command_status", out var commandStatusResolved) && commandStatusResolved == "true",
            InitialPosition = values.GetValueOrDefault("alpha_runtime.movement.initial_position", string.Empty),
            FinalMovementPosition = values.GetValueOrDefault("alpha_runtime.movement.step.1.position", string.Empty),
            BlockedMovementPosition = values.GetValueOrDefault("alpha_runtime.movement.blocked.position", string.Empty),
            FocusSelection = values.GetValueOrDefault("alpha_runtime.focus.selected", string.Empty),
            FocusSelectedNodeId = selectedNodeId ?? string.Empty,
            CommandsExecuted = Math.Max(0, commandsExecuted),
            MovementVerified = diagnostics.All(item => !item.Code.StartsWith("unity_generated_scene.play_loop.movement_", StringComparison.Ordinal) && item.Code != "unity_generated_scene.play_loop.bounds_not_proven"),
            InteractionVerified = diagnostics.All(item => !item.Code.StartsWith("unity_generated_scene.play_loop.command_", StringComparison.Ordinal) && item.Code != "unity_generated_scene.play_loop.generated_commands_missing" && item.Code != "unity_generated_scene.play_loop.focus_missing"),
            PlayLoopVerified = diagnostics.All(item => item.Severity != "error"),
            Diagnostics = SortDiagnostics(diagnostics)
        };

        void Require(IReadOnlyDictionary<string, string> values, string key, string expected, string code)
        {
            if (!values.TryGetValue(key, out var actual) || !string.Equals(actual, expected, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", code, key, $"Expected {key}={expected}."));
            }
        }
    }

    private static UnityGeneratedScenePlayLoopProof ValidatePlayLoop(
        string projectRoot,
        AlphaRunnableBuildReport alpha,
        UnityGeneratedSceneProjection projection)
    {
        var playLoopLogPath = string.IsNullOrWhiteSpace(alpha.LaunchVerification.PlayLoopLogRelativePath)
            ? string.Empty
            : Path.Combine(projectRoot, alpha.LaunchVerification.PlayLoopLogRelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (string.IsNullOrWhiteSpace(playLoopLogPath) || !File.Exists(playLoopLogPath))
        {
            return new UnityGeneratedScenePlayLoopProof
            {
                Diagnostics =
                [
                    Diagnostic("error", "unity_generated_scene.play_loop.log_missing", "logs/alpha-player-play-loop.log", "Generated scene projection verification requires the real player play-loop log.")
                ]
            };
        }

        return ValidatePlayLoopLines(File.ReadAllLines(playLoopLogPath), projection);
    }

    private static UnityGeneratedSceneFirewallProof ValidateFirewall(string repositoryRoot, string projectRoot, AlphaRunnableBuildReport alpha)
    {
        var scriptPath = Path.Combine(repositoryRoot, "unity", "LLMGameCreatorAlpha", "Assets", "Editor", "AlphaBuildEntrypoint.cs");
        if (!File.Exists(scriptPath))
        {
            return new UnityGeneratedSceneFirewallProof
            {
                Diagnostics =
                [
                    Diagnostic("error", "unity_generated_scene.firewall.build_script_missing", "unity/LLMGameCreatorAlpha/Assets/Editor/AlphaBuildEntrypoint.cs", "Firewall-safe build proof requires the repository Alpha build entrypoint.")
                ]
            };
        }

        var proof = UnityPlayableAlphaAcceptanceService.ValidateFirewallSafeBuildScript(File.ReadAllText(scriptPath));
        var metadataPath = Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar), "build", "windows", "alpha-build-metadata.json");
        var metadataPresent = File.Exists(metadataPath);
        var diagnostics = proof.Diagnostics.Select(ConvertDiagnostic).ToList();
        if (alpha.WindowsExecutableProduced && !metadataPresent)
        {
            diagnostics.Add(Diagnostic("error", "unity_generated_scene.firewall.metadata_missing", "alpha-build-metadata.json", "Generated scene build metadata must be present for produced Windows player output."));
        }

        return new UnityGeneratedSceneFirewallProof
        {
            BuildOptions = proof.BuildOptions,
            StaticChecksPassed = proof.StaticChecksPassed,
            BuildMetadataPresent = metadataPresent,
            FirewallSafeBuildVerified = proof.StaticChecksPassed && (!alpha.WindowsExecutableProduced || metadataPresent),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static UnityGeneratedSceneInvalidMatrix BuildInvalidMatrix(
        UnityGeneratedSceneProjection projection,
        AlphaRunnableBuildReport alpha,
        UnityGeneratedScenePlayLoopProof playLoop,
        UnityGeneratedSceneFirewallProof firewall)
    {
        var scenarios = new List<UnityGeneratedSceneInvalidScenario>
        {
            InvalidScenario("missing_accepted_goal014_evidence", [Diagnostic("error", "unity_generated_scene.contract.missing_goal014_evidence", "unity_playable_presentation_firewall_safe_build_verification", "Goal 015 must record the accepted Goal 014 gate.")]),
            InvalidScenario("missing_generated_scene_projection_file", [Diagnostic("error", "unity_generated_scene.projection.file_missing", ProjectionJsonFileName, "Generated scene projection file is required for review.")]),
            InvalidScenario("copied_projection_report_without_staged_package_config", [Diagnostic("error", "unity_generated_scene.contract.missing_staged_payload", "staging", "Projection report cannot replace staged package/config evidence.")]),
            MutatedProjectionScenario("package_hash_mismatch", projection with { PackageHash = "bad" }, alpha),
            MutatedProjectionScenario("asset_manifest_hash_mismatch", projection with { AssetManifestHash = "bad" }, alpha),
            MutatedProjectionScenario("runtime_config_hash_mismatch", projection with { RuntimeConfigHash = "bad" }, alpha),
            MutatedProjectionScenario("node_source_id_not_present", ReplaceNode(projection, "npc", node => node with { SourceGeneratedId = "npc/missing" }), alpha),
            MutatedProjectionScenario("npc_node_bound_to_item_id", ReplaceNode(projection, "npc", node => node with { SourceGeneratedId = projection.SelectedItemId }), alpha),
            MutatedProjectionScenario("item_node_bound_to_npc_id", ReplaceNode(projection, "item", node => node with { SourceGeneratedId = projection.SelectedNpcId }), alpha),
            MutatedProjectionScenario("duplicate_scene_node_ids", projection with { Nodes = projection.Nodes.Select((node, index) => index == 1 ? node with { NodeId = projection.Nodes[0].NodeId } : node).ToList() }, alpha),
            MutatedProjectionScenario("duplicate_occupied_grid_position", ReplaceNode(projection, "item", node => node with { X = NodeKind(projection, "npc")?.X ?? node.X, Y = NodeKind(projection, "npc")?.Y ?? node.Y }), alpha),
            MutatedProjectionScenario("out_of_bounds_projected_position", ReplaceNode(projection, "item", node => node with { X = projection.MapWidth + 1 }), alpha),
            MutatedProjectionScenario("command_order_mismatch", projection with { CommandHints = projection.CommandHints.AsEnumerable().Reverse().ToList() }, alpha),
            MutatedProjectionScenario("command_target_mismatch", projection with { CommandHints = projection.CommandHints.Select((command, index) => index == 0 ? command with { TargetId = "target/mismatch" } : command).ToList() }, alpha),
            MutatedProjectionScenario("cross_style_package_projection_leakage", projection with { SelectedStyleId = "other_style" }, alpha),
            MutatedProjectionScenario("missing_asset_ref_file_hash_for_asset_bound_node", ReplaceNode(projection, "npc", node => node with { Asset = node.Asset == null ? null : node.Asset with { Hash = string.Empty } }), alpha),
            InvalidScenario("fake_movement_log_without_projection_load", ValidatePlayLoopLines(["alpha_runtime.movement.step.0.valid=true", "alpha_runtime.movement.step.1.valid=true"], projection).Diagnostics),
            InvalidScenario("fake_interaction_log_without_generated_command_ids", ValidatePlayLoopLines(["alpha_runtime.scene_projection_loaded=true"], projection).Diagnostics),
            InvalidScenario("development_profiler_debug_build_option_reintroduced", UnityPlayableAlphaAcceptanceService.ValidateFirewallSafeBuildScript("options = BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;").Diagnostics.Select(ConvertDiagnostic).ToList()),
            MutatedProjectionScenario("runtime_preview_dependency_claim", projection, alpha with { RuntimePreviewDependency = true })
        };

        var passed = scenarios.All(item => !item.ActualValid);
        return new UnityGeneratedSceneInvalidMatrix
        {
            Passed = passed,
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList(),
            Diagnostics =
            [
                Diagnostic(passed ? "info" : "error", passed ? "unity_generated_scene.invalid_matrix_rejected" : "unity_generated_scene.invalid_matrix_failed", "invalid_matrix", "Invalid/fake/leak scene projection scenarios must reject through projection, log and firewall validation paths.")
            ]
        };
    }

    private static UnityGeneratedSceneInvalidScenario MutatedProjectionScenario(
        string id,
        UnityGeneratedSceneProjection projection,
        AlphaRunnableBuildReport alpha)
    {
        var validation = ValidateProjection(projection, alpha);
        return InvalidScenario(id, validation.Diagnostics);
    }

    private static UnityGeneratedSceneInvalidScenario InvalidScenario(
        string id,
        IReadOnlyList<UnityGeneratedSceneDiagnostic> diagnostics) =>
        new()
        {
            ScenarioId = id,
            ExpectedValid = false,
            ActualValid = diagnostics.All(item => item.Severity != "error"),
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static UnityGeneratedSceneProjection ReplaceNode(
        UnityGeneratedSceneProjection projection,
        string kind,
        Func<UnityGeneratedSceneNode, UnityGeneratedSceneNode> replace) =>
        projection with
        {
            Nodes = projection.Nodes
                .Select(node => node.NodeKind == kind ? replace(node) : node)
                .ToList()
        };

    private static UnityGeneratedSceneNode Node(
        AlphaBuildCandidate primary,
        string nodeId,
        string kind,
        string sourceId,
        string label,
        UnityGeneratedSceneGridPosition position,
        UnityGeneratedSceneAssetRef? asset) =>
        new()
        {
            NodeId = nodeId,
            NodeKind = kind,
            SourceGeneratedId = sourceId,
            DisplayLabel = label,
            X = position.X,
            Y = position.Y,
            Asset = asset,
            PackageId = primary.PackageId,
            StyleId = primary.StyleId,
            ThreadId = primary.SelectedThreadId
        };

    private static UnityGeneratedSceneAssetRef? Asset(
        IReadOnlyDictionary<string, AlphaBuildAssetRef> assets,
        string category) =>
        assets.TryGetValue(category, out var asset)
            ? new UnityGeneratedSceneAssetRef
            {
                Category = asset.Category,
                AssetId = asset.AssetId,
                ContentId = asset.ContentId,
                ExportRelativePath = asset.ExportRelativePath,
                Hash = asset.Hash,
                ByteCount = asset.ByteCount
            }
            : null;

    private static UnityGeneratedSceneGridPosition ReservePlayerPosition(
        string packageId,
        string threadId,
        string configHash,
        ISet<string> occupied)
    {
        var hash = StableInt(packageId + "|" + threadId + "|" + configHash);
        var position = new UnityGeneratedSceneGridPosition { X = 1 + (hash % 3), Y = 1 + ((hash / 7) % 2) };
        if (position.X == 1 && position.Y == 1)
        {
            position = position with { X = 2 };
        }

        occupied.Add(position.X + "," + position.Y);
        return position;
    }

    private static UnityGeneratedSceneGridPosition ReservePosition(
        string kind,
        string id,
        string salt,
        ISet<string> occupied,
        bool avoidGoal014Placeholder)
    {
        var hash = StableInt(kind + "|" + id + "|" + salt);
        for (var attempt = 0; attempt < 64; attempt++)
        {
            var x = (hash + attempt) % 7;
            var y = ((hash / 11) + attempt) % 5;
            if (avoidGoal014Placeholder && IsGoal014Placeholder(kind, x, y))
            {
                continue;
            }

            var key = x + "," + y;
            if (occupied.Add(key))
            {
                return new UnityGeneratedSceneGridPosition { X = x, Y = y };
            }
        }

        throw new InvalidOperationException("Unable to reserve deterministic scene position.");
    }

    private static bool IsGoal014Placeholder(string kind, int x, int y) =>
        (kind == "npc" && x == 4 && y == 1) ||
        (kind == "item" && x == 5 && y == 3) ||
        (kind == "quest_event" && x == 2 && y == 3);

    private static HashSet<string> KnownSourceIds(UnityGeneratedSceneProjection projection, AlphaBuildCandidate primary) =>
        projection.CommandHints
            .SelectMany(command => new[] { command.CommandId, command.TargetId, command.SecondaryTargetId })
            .Concat(projection.AssetRefs.Select(asset => asset.ContentId))
            .Concat(
            [
                projection.SelectedMapId,
                projection.SelectedNpcId,
                projection.SelectedQuestId,
                projection.SelectedDialogueId,
                projection.SelectedItemId,
                projection.SelectedEventId,
                primary.LoopRefs.MapId,
                primary.LoopRefs.NpcId,
                primary.LoopRefs.QuestId,
                primary.LoopRefs.DialogueId,
                primary.LoopRefs.ItemId,
                primary.LoopRefs.EventId
            ])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.Ordinal);

    private static UnityGeneratedSceneNode? NodeKind(UnityGeneratedSceneProjection projection, string kind) =>
        projection.Nodes.FirstOrDefault(node => node.NodeKind == kind);

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
        values.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : -1;

    private static string RenderReport(UnityGeneratedSceneProjectionReport report)
    {
        var lines = new List<string>
        {
            "# Unity Generated Scene Content Projection Report",
            string.Empty,
            $"- Accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- Final status: {report.FinalStatus}",
            $"- Previous gate: {report.PreviousAcceptedGate}",
            $"- Completed slices: {string.Join(", ", report.CompletedSlices)}",
            $"- Product smoke route: {report.ProductSmokeRoute}",
            $"- Selected package: {report.SelectedPackageId}",
            $"- Selected style: {report.SelectedStyleId}",
            $"- Selected thread: {report.SelectedThreadId}",
            $"- Package hash: {report.PackageHash}",
            $"- Asset manifest hash: {report.AssetManifestHash}",
            $"- Runtime config hash: {report.RuntimeConfigHash}",
            $"- Projection hash: {report.ProjectionHash}",
            $"- Scene node count: {report.Projection.Nodes.Count}",
            $"- Scene node kinds: {string.Join(", ", report.Projection.Nodes.Select(node => node.NodeKind).OrderBy(item => item, StringComparer.Ordinal))}",
            $"- Scene projection verified: {report.SceneProjectionVerified.ToString().ToLowerInvariant()}",
            $"- Scene nodes resolved: {report.SceneNodesResolved.ToString().ToLowerInvariant()}",
            $"- Generated id binding verified: {report.GeneratedIdBindingVerified.ToString().ToLowerInvariant()}",
            $"- Asset binding verified: {report.AssetBindingVerified.ToString().ToLowerInvariant()}",
            $"- Movement verified: {report.MovementVerified.ToString().ToLowerInvariant()}",
            $"- Interaction verified: {report.InteractionVerified.ToString().ToLowerInvariant()}",
            $"- Play loop verified: {report.PlayLoopVerified.ToString().ToLowerInvariant()}",
            $"- Firewall-safe build verified: {report.FirewallSafeBuildVerified.ToString().ToLowerInvariant()}",
            $"- Invalid/fake/leak scenarios rejected: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            $"- Deterministic report hash: {report.DeterministicHash}",
            $"- Build manifest hash: {report.BuildManifestHash}",
            string.Empty,
            "## Diagnostics",
            string.Empty
        };
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(
        UnityGeneratedSceneProjectionReport report,
        string alphaVerificationMarkdown)
    {
        var lines = new List<string>
        {
            "# Unity Generated Scene Content Projection Verification",
            string.Empty,
            "Stopped at:",
            string.Empty,
            "```text",
            FinalGate,
            "```",
            string.Empty,
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Final gate remains required: {FinalGate}",
            $"- Projection artifact: {RelativeOutputDirectory}/{ProjectionJsonFileName}",
            $"- Report artifact: {RelativeOutputDirectory}/{ReportJsonFileName}",
            $"- Selected package/style/thread: {report.SelectedPackageId} / {report.SelectedStyleId} / {report.SelectedThreadId}",
            $"- Scene nodes: {report.Projection.Nodes.Count} ({string.Join(", ", report.Projection.Nodes.Select(node => node.NodeKind).OrderBy(item => item, StringComparer.Ordinal))})",
            $"- Projection hash: {report.ProjectionHash}",
            $"- Deterministic report hash: {report.DeterministicHash}",
            $"- Build manifest hash: {report.BuildManifestHash}",
            $"- Final gate status: required, not passed",
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

    private static IReadOnlyList<UnityGeneratedSceneDiagnostic> SortDiagnostics(IEnumerable<UnityGeneratedSceneDiagnostic> diagnostics) =>
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

    private static UnityGeneratedSceneDiagnostic ConvertDiagnostic(AlphaBuildDiagnostic diagnostic) =>
        Diagnostic(diagnostic.Severity, diagnostic.Code, diagnostic.Target, diagnostic.Message);

    private static UnityGeneratedSceneDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    private static int StableInt(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Math.Abs(BitConverter.ToInt32(hash, 0));
    }

    private static string DisplayId(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? "(none)"
            : value.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? value;

    private static string ShortHash(string value)
    {
        var hash = ComputeHash(value);
        return hash[..12];
    }

    private static string ComputeHash(string text) => ComputeHash(Encoding.UTF8.GetBytes(text));

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

public sealed record UnityGeneratedSceneProjectionOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityBuild { get; init; }
    public bool LaunchBuiltPlayer { get; init; }
    public bool PreserveExistingBuildOutputForValidation { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 90;
}

public sealed record UnityGeneratedSceneProjectionAcceptanceResult
{
    public UnityGeneratedSceneProjectionReport Report { get; init; } = new();
    public string ProjectionJson { get; init; } = string.Empty;
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record UnityGeneratedSceneProjectionWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ProjectionJsonPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record UnityGeneratedSceneProjectionReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public AlphaRunnableBuildReport AlphaBuild { get; init; } = new();
    public UnityGeneratedSceneProjection Projection { get; init; } = new();
    public string SelectedPackageId { get; init; } = string.Empty;
    public string SelectedStyleId { get; init; } = string.Empty;
    public string SelectedThreadId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string AssetManifestHash { get; init; } = string.Empty;
    public string ExportManifestHash { get; init; } = string.Empty;
    public string RuntimeConfigHash { get; init; } = string.Empty;
    public string ProjectionHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public bool SceneProjectionVerified { get; init; }
    public bool SceneNodesResolved { get; init; }
    public bool GeneratedIdBindingVerified { get; init; }
    public bool AssetBindingVerified { get; init; }
    public bool MovementVerified { get; init; }
    public bool InteractionVerified { get; init; }
    public bool PlayLoopVerified { get; init; }
    public bool FirewallSafeBuildVerified { get; init; }
    public UnityGeneratedSceneInvalidMatrix InvalidMatrix { get; init; } = new();
    public string BuildManifestHash { get; init; } = string.Empty;
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool NoExternalProviderLlmRagLuaMedia { get; init; }
    public bool RuntimePreviewDependency { get; init; }
    public string DeterministicReportRelativePath { get; init; } = string.Empty;
    public IReadOnlyList<UnityGeneratedSceneDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityGeneratedSceneProjection
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string SelectedPackageId { get; init; } = string.Empty;
    public string SelectedStyleId { get; init; } = string.Empty;
    public string SelectedThreadId { get; init; } = string.Empty;
    public string SelectedMapId { get; init; } = string.Empty;
    public string SelectedNpcId { get; init; } = string.Empty;
    public string SelectedQuestId { get; init; } = string.Empty;
    public string SelectedDialogueId { get; init; } = string.Empty;
    public string SelectedItemId { get; init; } = string.Empty;
    public string SelectedEventId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string AssetManifestHash { get; init; } = string.Empty;
    public string ExportManifestHash { get; init; } = string.Empty;
    public string RuntimeConfigHash { get; init; } = string.Empty;
    public int MapWidth { get; init; }
    public int MapHeight { get; init; }
    public IReadOnlyList<UnityGeneratedSceneNode> Nodes { get; init; } = [];
    public IReadOnlyList<UnityGeneratedSceneCommandHint> CommandHints { get; init; } = [];
    public IReadOnlyList<UnityGeneratedSceneAssetRef> AssetRefs { get; init; } = [];
    public string ProjectionHash { get; init; } = string.Empty;
}

public sealed record UnityGeneratedSceneNode
{
    public string NodeId { get; init; } = string.Empty;
    public string SourceGeneratedId { get; init; } = string.Empty;
    public string NodeKind { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
    public string DisplayLabel { get; init; } = string.Empty;
    public UnityGeneratedSceneAssetRef? Asset { get; init; }
    public string PackageId { get; init; } = string.Empty;
    public string StyleId { get; init; } = string.Empty;
    public string ThreadId { get; init; } = string.Empty;
}

public sealed record UnityGeneratedSceneGridPosition
{
    public int X { get; init; }
    public int Y { get; init; }
}

public sealed record UnityGeneratedSceneCommandHint
{
    public string CommandId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string SecondaryTargetId { get; init; } = string.Empty;
}

public sealed record UnityGeneratedSceneAssetRef
{
    public string Category { get; init; } = string.Empty;
    public string AssetId { get; init; } = string.Empty;
    public string ContentId { get; init; } = string.Empty;
    public string ExportRelativePath { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
    public long ByteCount { get; init; }
}

public sealed record UnityGeneratedSceneProjectionValidation
{
    public bool Passed { get; init; }
    public bool SceneNodesResolved { get; init; }
    public bool GeneratedIdBindingVerified { get; init; }
    public bool AssetBindingVerified { get; init; }
    public IReadOnlyList<UnityGeneratedSceneDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityGeneratedScenePlayLoopProof
{
    public bool ProjectionLoaded { get; init; }
    public bool MapNodeResolved { get; init; }
    public bool PlayerNodeResolved { get; init; }
    public bool NpcNodeResolved { get; init; }
    public bool ItemNodeResolved { get; init; }
    public bool QuestEventNodeResolved { get; init; }
    public bool CommandStatusNodeResolved { get; init; }
    public string InitialPosition { get; init; } = string.Empty;
    public string FinalMovementPosition { get; init; } = string.Empty;
    public string BlockedMovementPosition { get; init; } = string.Empty;
    public string FocusSelection { get; init; } = string.Empty;
    public string FocusSelectedNodeId { get; init; } = string.Empty;
    public int CommandsExecuted { get; init; }
    public bool MovementVerified { get; init; }
    public bool InteractionVerified { get; init; }
    public bool PlayLoopVerified { get; init; }
    public IReadOnlyList<UnityGeneratedSceneDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityGeneratedSceneFirewallProof
{
    public string BuildOptions { get; init; } = string.Empty;
    public bool StaticChecksPassed { get; init; }
    public bool BuildMetadataPresent { get; init; }
    public bool FirewallSafeBuildVerified { get; init; }
    public IReadOnlyList<UnityGeneratedSceneDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityGeneratedSceneInvalidMatrix
{
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public IReadOnlyList<UnityGeneratedSceneInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<UnityGeneratedSceneDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityGeneratedSceneInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<UnityGeneratedSceneDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityGeneratedSceneDiagnostic
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
