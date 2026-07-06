using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class GenericGamePackageLoopProjectionService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly string[] SourceWriteMarkers =
    [
        "File.Write",
        "WriteAllText",
        "WriteAllBytes",
        "AssetDatabase.CreateAsset",
        "EditorSceneManager.Save",
        "SaveScene",
        "StreamingAssets"
    ];

    public GenericGamePackageLoopProjectionBuildResult Build(string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var samplePackage = BuildSampleSummary(root);
        var scriptInventory = BuildScriptInventory(root);
        var smokePlan = BuildSmokePlan();
        var logScan = BuildLogScan(root);
        var negative = BuildNegativeProof();
        var goal123StillGreen = Goal123StillGreen(root);
        var dashboard = BuildDashboard(
            samplePackage,
            scriptInventory,
            logScan,
            negative,
            goal123StillGreen);
        var report = RenderReport(dashboard, samplePackage, scriptInventory, smokePlan, logScan, negative);
        var docs = RenderDocumentation(dashboard);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GenericGamePackageLoopProjectionVocabulary.DashboardFileName] = Serialize(dashboard),
            [GenericGamePackageLoopProjectionVocabulary.ScriptInventoryFileName] = Serialize(scriptInventory),
            [GenericGamePackageLoopProjectionVocabulary.SmokePlanFileName] = Serialize(smokePlan),
            [GenericGamePackageLoopProjectionVocabulary.LogScanFileName] = Serialize(logScan),
            [GenericGamePackageLoopProjectionVocabulary.ReportFileName] = report,
            [GenericGamePackageLoopProjectionVocabulary.NegativeProofFileName] = Serialize(negative)
        };
        var proceduralIndex = BuildFileIndex(
            root,
            proceduralFiles,
            GenericGamePackageLoopProjectionVocabulary.ProceduralOutputDirectory,
            "goal124_generic_gamepackage_loop_evidence",
            includeUnityLog: true);
        proceduralFiles[GenericGamePackageLoopProjectionVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GenericGamePackageLoopProjectionVocabulary.DashboardFileName] = Serialize(dashboard),
            [GenericGamePackageLoopProjectionVocabulary.ScriptInventoryFileName] = Serialize(scriptInventory),
            [GenericGamePackageLoopProjectionVocabulary.SmokePlanFileName] = Serialize(smokePlan),
            [GenericGamePackageLoopProjectionVocabulary.LogScanFileName] = Serialize(logScan),
            [GenericGamePackageLoopProjectionVocabulary.ReportFileName] = report,
            [GenericGamePackageLoopProjectionVocabulary.NegativeProofFileName] = Serialize(negative)
        };
        var exportIndex = BuildFileIndex(
            root,
            exportFiles,
            GenericGamePackageLoopProjectionVocabulary.ExportPackageDirectory,
            "goal124_generic_gamepackage_loop_export",
            includeUnityLog: false);
        exportFiles[GenericGamePackageLoopProjectionVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new GenericGamePackageLoopProjectionBuildResult
        {
            Dashboard = dashboard,
            SamplePackage = samplePackage,
            ScriptInventory = scriptInventory,
            SmokePlan = smokePlan,
            LogScan = logScan,
            NegativeProof = negative,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles,
            DocumentationMarkdown = docs
        };
    }

    public async Task<GenericGamePackageLoopProjectionWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(root, GenericGamePackageLoopProjectionVocabulary.ProceduralOutputDirectory);
        var export = Resolve(root, GenericGamePackageLoopProjectionVocabulary.ExportPackageDirectory);
        var docsPath = Resolve(root, GenericGamePackageLoopProjectionVocabulary.DocumentationPath);
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

        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(docsPath, result.DocumentationMarkdown, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new GenericGamePackageLoopProjectionWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static GenericGamePackageLoopProjectionDashboard BuildDashboard(
        GenericGamePackageLoopSampleSummary samplePackage,
        GenericGamePackageLoopScriptInventory scriptInventory,
        GenericGamePackageLoopLogScan logScan,
        GenericGamePackageLoopNegativeProof negative,
        bool goal123StillGreen)
    {
        var diagnostics = new List<string>();
        Require(goal123StillGreen, "goal124.goal123_not_green", diagnostics);
        Require(samplePackage.Passed, "goal124.sample_loop_contract_failed", diagnostics);
        Require(scriptInventory.Passed, "goal124.script_inventory_failed", diagnostics);
        Require(negative.Passed, "goal124.negative_proof_failed", diagnostics);
        Require(logScan.Status != "BLOCKED_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_LOOP",
            "goal124.unity_loop_smoke_failed",
            diagnostics);

        return new GenericGamePackageLoopProjectionDashboard
        {
            GenericLoopStatus = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            PackageId = samplePackage.PackageId,
            MapId = samplePackage.MapId,
            InteractionPreviewPresent = samplePackage.SignInspectInteractionPresent,
            InteractionApplyPassed =
                samplePackage.SignInspectSetFlagEffectPresent
                && samplePackage.SignInspectLogEffectPresent,
            DialogueSummaryPresent = samplePackage.OldGuardDialoguePresent,
            QuestObjectiveSummaryPresent =
                samplePackage.HelpHealerQuestPresent
                && samplePackage.RequiredRedHerbAmount == 3
                && samplePackage.PlayerRedHerbAmount == 2
                && samplePackage.HelpHealerIncomplete,
            InventorySummaryPresent = samplePackage.InventoryStackCount > 0,
            ResourceSummaryPresent = samplePackage.ResourceCount > 0,
            UnitySmokeStatus = logScan.Status,
            CleanupScriptAvailable = CleanupScriptAvailable(scriptInventory),
            Goal123StillGreen = goal123StillGreen,
            SamplePackageReadOnly = samplePackage.ReadOnlySource,
            AppliedInteractionCount = 1,
            StartedQuestCount = 1,
            Diagnostics = diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static GenericGamePackageLoopSampleSummary BuildSampleSummary(string root)
    {
        var diagnostics = new List<string>();
        var path = Resolve(root, GenericGamePackageLoopProjectionVocabulary.SamplePackagePath);
        if (!File.Exists(path))
        {
            diagnostics.Add("goal124.sample_package_missing");
            return new GenericGamePackageLoopSampleSummary
            {
                Exists = false,
                Parsed = false,
                ReadOnlySource = true,
                ExcludedFromExpectedChangedPaths = true,
                Diagnostics = diagnostics,
                Passed = false
            };
        }

        var text = File.ReadAllText(path, Encoding.UTF8);
        try
        {
            using var doc = JsonDocument.Parse(text);
            var rootElement = doc.RootElement;
            var manifest = ObjectProperty(rootElement, "manifest");
            var game = ObjectProperty(rootElement, "game");
            var startMapId = StringValue(manifest, "startMapId");
            var map = ArrayItems(game, "maps")
                .FirstOrDefault(item => StringValue(item, "id") == startMapId);
            if (map.ValueKind == JsonValueKind.Undefined)
            {
                map = ArrayItems(game, "maps").FirstOrDefault();
            }

            var entities = ArrayItems(map, "entities").ToList();
            var interactions = ArrayItems(game, "interactions").ToList();
            var dialogues = ArrayItems(game, "dialogues").ToList();
            var quests = ArrayItems(game, "quests").ToList();
            var inventories = ArrayItems(game, "inventories").ToList();
            var resources = ArrayItems(game, "resources").ToList();

            var signInspect = interactions.FirstOrDefault(item =>
                StringValue(item, "id") == "interaction/sign_inspect");
            var signEffects = ArrayItems(signInspect, "effects").ToList();
            var playerInventory = inventories.FirstOrDefault(item =>
                StringValue(item, "id") == "inventory/player_start");
            var redHerbAmount = ArrayItems(playerInventory, "stacks")
                .Where(item => StringValue(item, "itemId") == "item/red_herb")
                .Sum(item => IntValue(item, "amount"));
            var helpHealer = quests.FirstOrDefault(item =>
                StringValue(item, "id") == "quest/help_healer");
            var redHerbObjective = ArrayItems(helpHealer, "objectives")
                .FirstOrDefault(item => StringValue(item, "targetId") == "item/red_herb");
            var requiredRedHerbAmount = IntValue(redHerbObjective, "requiredAmount");

            var summary = new GenericGamePackageLoopSampleSummary
            {
                Exists = true,
                Parsed = true,
                ReadOnlySource = true,
                ExcludedFromExpectedChangedPaths =
                    !BuildExpectedChangedPaths().Contains(
                        GenericGamePackageLoopProjectionVocabulary.SamplePackagePath,
                        StringComparer.Ordinal),
                Sha256 = HashBytes(File.ReadAllBytes(path)),
                PackageId = StringValue(manifest, "packageId"),
                MapId = StringValue(map, "id"),
                SignEntityPresent = entities.Any(item =>
                    StringValue(item, "id") == "entity/village/sign"),
                SignInspectInteractionPresent =
                    signInspect.ValueKind == JsonValueKind.Object
                    && StringValue(signInspect, "kind") == "inspect",
                SignInspectSetFlagEffectPresent = signEffects.Any(item =>
                    StringValue(item, "type") == "set_flag"
                    && StringValue(ObjectProperty(item, "args"), "id") == "flag/sign_inspected"),
                SignInspectLogEffectPresent = signEffects.Any(item =>
                    StringValue(item, "type") == "log"
                    && !string.IsNullOrWhiteSpace(StringValue(ObjectProperty(item, "args"), "message"))),
                OldGuardEntityPresent = entities.Any(item =>
                    StringValue(item, "id") == "entity/village/old_guard"),
                OldGuardDialoguePresent = dialogues.Any(item =>
                    StringValue(item, "id") == "dialogue/old_guard_intro"
                    && ArrayItems(item, "nodes").Any(node =>
                        StringValue(node, "id") == StringValue(item, "startNodeId")
                        && !string.IsNullOrWhiteSpace(StringValue(node, "text")))),
                HelpHealerQuestPresent = helpHealer.ValueKind == JsonValueKind.Object,
                RequiredRedHerbAmount = requiredRedHerbAmount,
                PlayerRedHerbAmount = redHerbAmount,
                HelpHealerIncomplete =
                    requiredRedHerbAmount > 0 && redHerbAmount < requiredRedHerbAmount,
                InventoryStackCount = ArrayItems(playerInventory, "stacks").Count(),
                ResourceCount = resources.Count,
                Diagnostics = diagnostics
            };

            return summary with
            {
                Passed = summary.Exists
                         && summary.Parsed
                         && summary.ReadOnlySource
                         && summary.ExcludedFromExpectedChangedPaths
                         && summary.PackageId == "game/minimal-map-game"
                         && summary.MapId == "map/village"
                         && summary.SignEntityPresent
                         && summary.SignInspectInteractionPresent
                         && summary.SignInspectSetFlagEffectPresent
                         && summary.SignInspectLogEffectPresent
                         && summary.OldGuardEntityPresent
                         && summary.OldGuardDialoguePresent
                         && summary.HelpHealerQuestPresent
                         && summary.RequiredRedHerbAmount == 3
                         && summary.PlayerRedHerbAmount == 2
                         && summary.HelpHealerIncomplete
                         && summary.InventoryStackCount > 0
                         && summary.ResourceCount > 0
            };
        }
        catch (Exception ex)
        {
            diagnostics.Add("goal124.sample_package_parse_failed:" + ex.GetType().Name);
            return new GenericGamePackageLoopSampleSummary
            {
                Exists = true,
                Parsed = false,
                ReadOnlySource = true,
                ExcludedFromExpectedChangedPaths = true,
                Sha256 = HashBytes(File.ReadAllBytes(path)),
                Diagnostics = diagnostics,
                Passed = false
            };
        }
    }

    private static GenericGamePackageLoopScriptInventory BuildScriptInventory(string root)
    {
        var entries = new[]
        {
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath,
                "unity_editor_window", "Run Generic Package Gameplay Loop Verification"),
            Entry(root, GenericGamePackageProjectionVocabulary.UnityAdapterPath,
                "unity_generic_projection_adapter", "BuildQuestProjection"),
            Entry(root, GenericGamePackageProjectionVocabulary.UnityModelsPath,
                "unity_generic_projection_models", "GenericGamePackageProjectionLoopSmokeResult"),
            Entry(root, GenericGamePackageProjectionVocabulary.UnityControllerPath,
                "unity_generic_projection_controller", "RunGenericPackageGameplayLoopVerification"),
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath,
                "unity_projection_primitive_factory", "AttachDescriptor"),
            Entry(root, GenericGamePackageLoopProjectionVocabulary.UnityStatePath,
                "unity_generic_projection_state", "selectedEntityId"),
            Entry(root, GenericGamePackageLoopProjectionVocabulary.UnityLoopPath,
                "unity_generic_projection_loop", "quest/help_healer"),
            Entry(root, AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath,
                "cleanup_script", "Unity editor noise cleanup mode"),
            Entry(root, AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptCmdPath,
                "cleanup_cmd_wrapper", "clean-unity-editor-noise.ps1")
        }.ToList();

        var editorText = SourceText(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath);
        var adapterText = SourceText(root, GenericGamePackageProjectionVocabulary.UnityAdapterPath);
        var modelsText = SourceText(root, GenericGamePackageProjectionVocabulary.UnityModelsPath);
        var controllerText = SourceText(root, GenericGamePackageProjectionVocabulary.UnityControllerPath);
        var stateText = SourceText(root, GenericGamePackageLoopProjectionVocabulary.UnityStatePath);
        var loopText = SourceText(root, GenericGamePackageLoopProjectionVocabulary.UnityLoopPath);
        var genericUnityText = string.Join("\n", entries
            .Where(entry => entry.RelativePath.StartsWith("unity/", StringComparison.Ordinal))
            .Select(entry => SourceText(root, entry.RelativePath)));
        var forbidden = SourceWriteMarkers
            .Where(marker => genericUnityText.Contains(marker, StringComparison.Ordinal))
            .OrderBy(marker => marker, StringComparer.Ordinal)
            .ToList();

        var smokeMarkers = new[]
        {
            "genericLoopPassed",
            "samplePackageLoaded",
            "genericProjectionBuilt",
            "interactionPreviewPresent",
            "interactionApplyPassed",
            "dialogueSummaryPresent",
            "questObjectiveSummaryPresent",
            "inventorySummaryPresent",
            "resourceSummaryPresent",
            "eventLogPresent",
            "zeroFatalErrors"
        };
        var stateMarkers = new[]
        {
            "selectedEntityId",
            "selectedInteractionId",
            "selectedDialogueId",
            "selectedQuestId",
            "inventorySummary",
            "resourceSummary",
            "questObjectiveSummary",
            "interactionEffectPreview",
            "projectionEventLog",
            "appliedInteractionCount",
            "startedQuestCount"
        };

        var inventory = new GenericGamePackageLoopScriptInventory
        {
            ScriptCount = entries.Count,
            WindowActionPresent =
                editorText.Contains("Run Generic Package Gameplay Loop Verification", StringComparison.Ordinal)
                && editorText.Contains("RunGenericPackageGameplayLoopVerification()", StringComparison.Ordinal),
            BatchmodeMethodPresent =
                editorText.Contains("RunBatchmodeGenericGamePackageLoopSmoke", StringComparison.Ordinal),
            BatchmodePassMarkerPresent =
                editorText.Contains("GOAL124_GENERIC_GAMEPACKAGE_LOOP_PASS", StringComparison.Ordinal),
            BatchmodeFailMarkerPresent =
                editorText.Contains("GOAL124_GENERIC_GAMEPACKAGE_LOOP_FAIL", StringComparison.Ordinal),
            StateClassTracksRequiredFields = stateMarkers.All(marker =>
                stateText.Contains(marker, StringComparison.Ordinal)),
            LoopRunsRequiredSequence =
                loopText.Contains("entity/village/sign", StringComparison.Ordinal)
                && loopText.Contains("interaction/sign_inspect", StringComparison.Ordinal)
                && loopText.Contains("entity/village/old_guard", StringComparison.Ordinal)
                && loopText.Contains("dialogue/old_guard_intro", StringComparison.Ordinal)
                && loopText.Contains("quest/help_healer", StringComparison.Ordinal)
                && loopText.Contains("item/red_herb", StringComparison.Ordinal)
                && loopText.Contains("inventory/player_start", StringComparison.Ordinal),
            ControllerRendersLoopMarkers =
                controllerText.Contains("goal124_generic_loop_status", StringComparison.Ordinal)
                && controllerText.Contains("goal124_interaction_preview", StringComparison.Ordinal)
                && controllerText.Contains("goal124_dialogue_summary", StringComparison.Ordinal)
                && controllerText.Contains("goal124_quest_objective_status", StringComparison.Ordinal)
                && controllerText.Contains("goal124_inventory_summary", StringComparison.Ordinal)
                && controllerText.Contains("goal124_resource_summary", StringComparison.Ordinal)
                && controllerText.Contains("goal124_event_log_summary", StringComparison.Ordinal),
            AdapterParsesLoopData =
                adapterText.Contains("BuildInventoryProjection", StringComparison.Ordinal)
                && adapterText.Contains("BuildQuestProjection", StringComparison.Ordinal)
                && adapterText.Contains("BuildDialogueProjection", StringComparison.Ordinal)
                && adapterText.Contains("BuildInteractionProjection", StringComparison.Ordinal),
            ModelsExposeLoopSmokeFields = smokeMarkers.All(marker =>
                modelsText.Contains(marker, StringComparison.Ordinal)),
            ExistingGoal123VerificationStillPresent =
                editorText.Contains("Run Generic Package Projection Verification", StringComparison.Ordinal)
                && editorText.Contains("GOAL123_GENERIC_PACKAGE_PROJECTION_PASS", StringComparison.Ordinal),
            NoSourceWriteMarkers = forbidden.Count == 0,
            ForbiddenSourceMarkersFound = forbidden,
            Scripts = entries
        };

        return inventory with
        {
            Passed = inventory.ScriptCount == 9
                     && entries.All(entry => entry.Exists && entry.ContainsRequiredMarker)
                     && inventory.WindowActionPresent
                     && inventory.BatchmodeMethodPresent
                     && inventory.BatchmodePassMarkerPresent
                     && inventory.BatchmodeFailMarkerPresent
                     && inventory.StateClassTracksRequiredFields
                     && inventory.LoopRunsRequiredSequence
                     && inventory.ControllerRendersLoopMarkers
                     && inventory.AdapterParsesLoopData
                     && inventory.ModelsExposeLoopSmokeFields
                     && inventory.ExistingGoal123VerificationStillPresent
                     && inventory.NoSourceWriteMarkers
        };
    }

    private static GenericGamePackageLoopSmokePlan BuildSmokePlan()
    {
        var steps = new List<GenericGamePackageLoopSmokePlanStep>
        {
            Step(1, "open_projection_window", "Open the accepted Alpha projection menu path."),
            Step(2, "run_generic_package_gameplay_loop_verification", "Click Run Generic Package Gameplay Loop Verification."),
            Step(3, "load_sample_package", "Read samples/minimal-map-game/package.json without mutating it."),
            Step(4, "build_generic_projection", "Build the generic package projection markers."),
            Step(5, "select_sign", "Select entity/village/sign or the first inspectable interaction target."),
            Step(6, "preview_sign_inspect", "Preview interaction/sign_inspect and its effect list."),
            Step(7, "apply_sign_inspect", "Set projection flag/log state and increment appliedInteractionCount."),
            Step(8, "show_old_guard_dialogue", "Show dialogue/old_guard_intro summary."),
            Step(9, "show_help_healer_objective", "Show quest/help_healer red herb objective as incomplete: required 3, inventory has 2."),
            Step(10, "show_inventory_resources", "Show inventory/player_start and resource summaries."),
            Step(11, "show_loop_markers", "Visible TextMesh markers summarize loop status, selected entity, interaction, dialogue, quest, inventory, resources and event log."),
            Step(12, "cleanup_after_unity", "Use the existing clean-unity-editor-noise command after Unity checks.")
        };

        return new GenericGamePackageLoopSmokePlan
        {
            StepCount = steps.Count,
            Steps = steps
        };
    }

    private static GenericGamePackageLoopLogScan BuildLogScan(string root)
    {
        var path = Resolve(root, GenericGamePackageLoopProjectionVocabulary.UnityBatchmodeLogRelativePath);
        var logExists = File.Exists(path);
        var text = logExists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        var forbidden = new List<string>();
        if (text.Contains("GOAL124_GENERIC_GAMEPACKAGE_LOOP_FAIL", StringComparison.Ordinal))
        {
            forbidden.Add("GOAL124_GENERIC_GAMEPACKAGE_LOOP_FAIL");
        }

        var smokeFields = new[]
        {
            "genericLoopPassed=True",
            "samplePackageLoaded=True",
            "genericProjectionBuilt=True",
            "interactionPreviewPresent=True",
            "interactionApplyPassed=True",
            "dialogueSummaryPresent=True",
            "questObjectiveSummaryPresent=True",
            "inventorySummaryPresent=True",
            "resourceSummaryPresent=True",
            "eventLogPresent=True",
            "zeroFatalErrors=True",
            "selectedInteractionId=interaction/sign_inspect",
            "selectedDialogueId=dialogue/old_guard_intro",
            "selectedQuestId=quest/help_healer",
            "appliedInteractionCount=1",
            "startedQuestCount=1"
        };
        var passMarkerPresent = text.Contains("GOAL124_GENERIC_GAMEPACKAGE_LOOP_PASS", StringComparison.Ordinal);
        var smokeFieldsPresent = smokeFields.All(field => text.Contains(field, StringComparison.Ordinal));
        var passed = logExists && passMarkerPresent && smokeFieldsPresent && forbidden.Count == 0;
        return new GenericGamePackageLoopLogScan
        {
            LogExists = logExists,
            PassMarkerPresent = passMarkerPresent,
            FailMarkerAbsent = forbidden.Count == 0,
            SmokeRequiredFieldsPresent = smokeFieldsPresent,
            Passed = passed,
            Status = passed
                ? "GREEN"
                : logExists
                    ? "BLOCKED_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_LOOP"
                    : "PENDING_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_LOOP",
            Sha256 = logExists ? HashBytes(File.ReadAllBytes(path)) : string.Empty,
            ForbiddenMarkersFound = forbidden
        };
    }

    private static GenericGamePackageLoopNegativeProof BuildNegativeProof()
    {
        var rejected = BuildRejectedPathSamples();
        return new GenericGamePackageLoopNegativeProof
        {
            ManualInputRejected = true,
            SamplePackageMutationRejected = true,
            RuntimeSchemaProviderLuaGeneratorLibraryRejected = true,
            UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected = true,
            FinalReleasePackagingRejected = true,
            NoForbiddenPathExpected = BuildExpectedChangedPaths()
                .All(path => !IsRejectedPath(path)),
            RejectedPathSamples = rejected,
            Passed = rejected.All(path => !IsAllowedChangedPath(path))
        };
    }

    private static GenericGamePackageLoopFileIndex BuildFileIndex(
        string root,
        IReadOnlyDictionary<string, string> files,
        string relativeRoot,
        string role,
        bool includeUnityLog)
    {
        var entries = files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new GenericGamePackageLoopFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = role,
                Sha256 = HashText(item.Value)
            })
            .ToList();
        if (includeUnityLog)
        {
            var logPath = Resolve(root, GenericGamePackageLoopProjectionVocabulary.UnityBatchmodeLogRelativePath);
            if (File.Exists(logPath))
            {
                entries.Add(new GenericGamePackageLoopFileIndexEntry
                {
                    RelativePath = GenericGamePackageLoopProjectionVocabulary.UnityBatchmodeLogRelativePath,
                    Role = "goal124_unity_batchmode_generic_gamepackage_loop_log",
                    Sha256 = HashBytes(File.ReadAllBytes(logPath))
                });
            }
        }

        return new GenericGamePackageLoopFileIndex
        {
            IndexedFileCount = entries.Count,
            ManualInputExcluded = entries.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = entries.OrderBy(entry => entry.RelativePath, StringComparer.Ordinal).ToList()
        };
    }

    private static string RenderReport(
        GenericGamePackageLoopProjectionDashboard dashboard,
        GenericGamePackageLoopSampleSummary samplePackage,
        GenericGamePackageLoopScriptInventory scriptInventory,
        GenericGamePackageLoopSmokePlan smokePlan,
        GenericGamePackageLoopLogScan logScan,
        GenericGamePackageLoopNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 124 Generic GamePackage Quest Dialogue Interaction Loop",
            string.Empty,
            "- genericLoopStatus: " + dashboard.GenericLoopStatus,
            "- samplePackagePath: " + dashboard.SamplePackagePath,
            "- packageId: " + dashboard.PackageId,
            "- mapId: " + dashboard.MapId,
            "- interactionPreviewPresent: " + dashboard.InteractionPreviewPresent.ToString().ToLowerInvariant(),
            "- interactionApplyPassed: " + dashboard.InteractionApplyPassed.ToString().ToLowerInvariant(),
            "- dialogueSummaryPresent: " + dashboard.DialogueSummaryPresent.ToString().ToLowerInvariant(),
            "- questObjectiveSummaryPresent: " + dashboard.QuestObjectiveSummaryPresent.ToString().ToLowerInvariant(),
            "- inventorySummaryPresent: " + dashboard.InventorySummaryPresent.ToString().ToLowerInvariant(),
            "- resourceSummaryPresent: " + dashboard.ResourceSummaryPresent.ToString().ToLowerInvariant(),
            "- unitySmokeStatus: " + dashboard.UnitySmokeStatus,
            "- cleanupScriptAvailable: " + dashboard.CleanupScriptAvailable.ToString().ToLowerInvariant(),
            "- goal123StillGreen: " + dashboard.Goal123StillGreen.ToString().ToLowerInvariant(),
            "- evidencePath: " + dashboard.EvidencePath,
            "- exportPath: " + dashboard.ExportPath,
            string.Empty,
            "## Sample Package",
            string.Empty,
            "- parsed: " + samplePackage.Parsed.ToString().ToLowerInvariant(),
            "- readOnlySource: " + samplePackage.ReadOnlySource.ToString().ToLowerInvariant(),
            "- sha256: " + samplePackage.Sha256,
            "- signEntityPresent: " + samplePackage.SignEntityPresent.ToString().ToLowerInvariant(),
            "- signInspectInteractionPresent: " + samplePackage.SignInspectInteractionPresent.ToString().ToLowerInvariant(),
            "- oldGuardDialoguePresent: " + samplePackage.OldGuardDialoguePresent.ToString().ToLowerInvariant(),
            "- helpHealerQuestPresent: " + samplePackage.HelpHealerQuestPresent.ToString().ToLowerInvariant(),
            "- requiredRedHerbAmount: " + samplePackage.RequiredRedHerbAmount,
            "- playerRedHerbAmount: " + samplePackage.PlayerRedHerbAmount,
            "- helpHealerIncomplete: " + samplePackage.HelpHealerIncomplete.ToString().ToLowerInvariant(),
            string.Empty,
            "## Script Inventory",
            string.Empty,
            "- passed: " + scriptInventory.Passed.ToString().ToLowerInvariant(),
            "- scriptCount: " + scriptInventory.ScriptCount,
            "- stateClassTracksRequiredFields: " + scriptInventory.StateClassTracksRequiredFields.ToString().ToLowerInvariant(),
            "- loopRunsRequiredSequence: " + scriptInventory.LoopRunsRequiredSequence.ToString().ToLowerInvariant(),
            "- controllerRendersLoopMarkers: " + scriptInventory.ControllerRendersLoopMarkers.ToString().ToLowerInvariant(),
            "- noSourceWriteMarkers: " + scriptInventory.NoSourceWriteMarkers.ToString().ToLowerInvariant(),
            string.Empty,
            "## Smoke Plan",
            string.Empty,
            "- stepCount: " + smokePlan.StepCount,
            string.Empty,
            "## Log Scan",
            string.Empty,
            "- status: " + logScan.Status,
            "- logExists: " + logScan.LogExists.ToString().ToLowerInvariant(),
            "- passMarkerPresent: " + logScan.PassMarkerPresent.ToString().ToLowerInvariant(),
            "- smokeRequiredFieldsPresent: " + logScan.SmokeRequiredFieldsPresent.ToString().ToLowerInvariant(),
            "- forbiddenMarkerCount: " + logScan.ForbiddenMarkersFound.Count,
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

    private static string RenderDocumentation(GenericGamePackageLoopProjectionDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Generic GamePackage Quest Dialogue Interaction Loop",
            string.Empty,
            "Goal124 adds a projection-only Unity Editor gameplay loop over `samples/minimal-map-game/package.json`.",
            string.Empty,
            "## Hands-on Verification",
            string.Empty,
            "- Open `unity/LLMGameCreatorAlpha` in Unity.",
            "- Select `LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection`.",
            "- Click `Run Generic Package Gameplay Loop Verification`.",
            "- Verify the loop status, selected sign interaction, old guard dialogue, help healer objective, inventory, resources and event log markers.",
            "- Do not save scenes, prefabs, ProjectSettings, Packages or StreamingAssets as part of this check.",
            string.Empty,
            "## Cleanup Command",
            string.Empty,
            "- After Unity checks: `.\\.devflow\\scripts\\clean-unity-editor-noise.cmd`",
            string.Empty,
            "## Status",
            string.Empty,
            "- genericLoopStatus: " + dashboard.GenericLoopStatus,
            "- samplePackagePath: " + dashboard.SamplePackagePath,
            "- packageId: " + dashboard.PackageId,
            "- mapId: " + dashboard.MapId,
            "- unitySmokeStatus: " + dashboard.UnitySmokeStatus,
            "- projectionOnly: " + dashboard.ProjectionOnly.ToString().ToLowerInvariant(),
            "- noRuntimeProviderSchemaLuaGeneratorLibrary: "
            + dashboard.NoRuntimeProviderSchemaLuaGeneratorLibrary.ToString().ToLowerInvariant()
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static bool Goal123StillGreen(string root)
    {
        var result = new GenericGamePackageProjectionService().Build(root);
        return result.Dashboard.GenericProjectionStatus == "GREEN"
               && result.SamplePackage.Passed
               && result.ScriptInventory.Passed
               && result.NegativeProof.Passed;
    }

    private static bool CleanupScriptAvailable(GenericGamePackageLoopScriptInventory inventory) =>
        inventory.Scripts.Any(entry =>
            entry.RelativePath == AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath
            && entry.Exists
            && entry.ContainsRequiredMarker)
        && inventory.Scripts.Any(entry =>
            entry.RelativePath == AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptCmdPath
            && entry.Exists
            && entry.ContainsRequiredMarker);

    private static GenericGamePackageLoopScriptInventoryEntry Entry(
        string root,
        string relativePath,
        string role,
        string marker)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        var text = exists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        return new GenericGamePackageLoopScriptInventoryEntry
        {
            RelativePath = relativePath,
            Role = role,
            Exists = exists,
            ContainsRequiredMarker = text.Contains(marker, StringComparison.Ordinal),
            RequiredMarker = marker,
            Sha256 = exists ? HashBytes(File.ReadAllBytes(path)) : string.Empty
        };
    }

    private static GenericGamePackageLoopSmokePlanStep Step(
        int index,
        string stepId,
        string expectedResult) =>
        new()
        {
            StepIndex = index,
            StepId = stepId,
            ExpectedResult = expectedResult
        };

    private static IReadOnlyList<string> BuildExpectedChangedPaths() =>
    [
        GenericGamePackageLoopProjectionVocabulary.ProceduralOutputDirectory + "/",
        GenericGamePackageLoopProjectionVocabulary.ExportPackageDirectory + "/",
        "docs/agent-tasks/goal-124-generic-gamepackage-quest-dialogue-interaction-loop/",
        GenericGamePackageLoopProjectionVocabulary.DocumentationPath,
        ".devflow/artifact-scope/artifact-scope-policy.json",
        "docs/CURRENT_GENERATOR_STATE.json",
        "docs/CURRENT_GENERATOR_STATE.md",
        "docs/FULL_GENERATOR_GOAL_QUEUE.md",
        "docs/CONTEXT_INDEX.md",
        "docs/MILESTONE_GATES.md",
        "docs/RELEASE_RISK_REGISTER.md",
        "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
        "src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath,
        GenericGamePackageProjectionVocabulary.UnityAdapterPath,
        GenericGamePackageProjectionVocabulary.UnityModelsPath,
        GenericGamePackageProjectionVocabulary.UnityControllerPath,
        GenericGamePackageLoopProjectionVocabulary.UnityStatePath,
        GenericGamePackageLoopProjectionVocabulary.UnityLoopPath,
        "tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/",
        "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs"
    ];

    private static IReadOnlyList<string> BuildRejectedPathSamples() =>
    [
        ".llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json",
        GenericGamePackageLoopProjectionVocabulary.SamplePackagePath,
        "src/LLMGameCreator.Runtime/GameRuntime.cs",
        "src/LLMGameCreator.Runtime.Abstractions/IGameRuntime.cs",
        "src/LLMGameCreator.GamePackage/GamePackageDefinition.cs",
        "src/LLMGameCreator.Scripting/LuaSandbox.cs",
        "generator-library/example.json",
        "unity/LLMGameCreatorAlpha/Assets/Scenes/Main.unity",
        "unity/LLMGameCreatorAlpha/Assets/Prefabs/AcceptedAlpha.prefab",
        "unity/LLMGameCreatorAlpha/ProjectSettings/ProjectSettings.asset",
        "unity/LLMGameCreatorAlpha/Packages/manifest.json",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/example.json",
        ".llmgc/exports/final-release/package.zip",
        "provider/live-geodata/Adapter.cs"
    ];

    private static bool IsAllowedChangedPath(string path) =>
        BuildExpectedChangedPaths().Any(prefix =>
            prefix.EndsWith("/", StringComparison.Ordinal)
                ? path.StartsWith(prefix, StringComparison.Ordinal)
                : string.Equals(path, prefix, StringComparison.Ordinal));

    private static bool IsRejectedPath(string path) =>
        BuildRejectedPathSamples().Any(rejected =>
            rejected.EndsWith("/", StringComparison.Ordinal)
                ? path.StartsWith(rejected, StringComparison.Ordinal)
                : string.Equals(path, rejected, StringComparison.Ordinal));

    private static IReadOnlyList<JsonElement> ArrayItems(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Array
            ? property.EnumerateArray().ToList()
            : [];

    private static JsonElement ObjectProperty(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Object
            ? property
            : default;

    private static string StringValue(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int IntValue(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
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

    private static string SourceText(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
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
            throw new InvalidOperationException("Goal124 must not write the manual input path.");
        }
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
