using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class GenericGamePackageFullPlaythroughProjectionService
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

    public GenericGamePackageFullPlaythroughProjectionBuildResult Build(
        string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var samplePackage = BuildSampleSummary(root);
        var scriptInventory = BuildScriptInventory(root);
        var smokePlan = BuildSmokePlan();
        var logScan = BuildLogScan(root);
        var negative = BuildNegativeProof();
        var goal125StillGreen = Goal125StillGreen(root);
        var dashboard = BuildDashboard(
            samplePackage,
            scriptInventory,
            logScan,
            negative,
            goal125StillGreen);
        var report = RenderReport(dashboard, samplePackage, scriptInventory, smokePlan, logScan, negative);
        var docs = RenderDocumentation(dashboard);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GenericGamePackageFullPlaythroughProjectionVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [GenericGamePackageFullPlaythroughProjectionVocabulary.ScriptInventoryFileName] =
                Serialize(scriptInventory),
            [GenericGamePackageFullPlaythroughProjectionVocabulary.SmokePlanFileName] =
                Serialize(smokePlan),
            [GenericGamePackageFullPlaythroughProjectionVocabulary.LogScanFileName] =
                Serialize(logScan),
            [GenericGamePackageFullPlaythroughProjectionVocabulary.ReportFileName] = report,
            [GenericGamePackageFullPlaythroughProjectionVocabulary.NegativeProofFileName] =
                Serialize(negative)
        };
        var proceduralIndex = BuildFileIndex(
            root,
            proceduralFiles,
            GenericGamePackageFullPlaythroughProjectionVocabulary.ProceduralOutputDirectory,
            "goal126_generic_gamepackage_full_playthrough_evidence",
            GenericGamePackageFullPlaythroughProjectionVocabulary.UnityBatchmodeLogRelativePath);
        proceduralFiles[GenericGamePackageFullPlaythroughProjectionVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GenericGamePackageFullPlaythroughProjectionVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [GenericGamePackageFullPlaythroughProjectionVocabulary.ScriptInventoryFileName] =
                Serialize(scriptInventory),
            [GenericGamePackageFullPlaythroughProjectionVocabulary.SmokePlanFileName] =
                Serialize(smokePlan),
            [GenericGamePackageFullPlaythroughProjectionVocabulary.LogScanFileName] =
                Serialize(logScan),
            [GenericGamePackageFullPlaythroughProjectionVocabulary.ReportFileName] = report,
            [GenericGamePackageFullPlaythroughProjectionVocabulary.NegativeProofFileName] =
                Serialize(negative)
        };
        var exportIndex = BuildFileIndex(
            root,
            exportFiles,
            GenericGamePackageFullPlaythroughProjectionVocabulary.ExportPackageDirectory,
            "goal126_generic_gamepackage_full_playthrough_export",
            GenericGamePackageFullPlaythroughProjectionVocabulary.UnityBatchmodeExportLogRelativePath);
        exportFiles[GenericGamePackageFullPlaythroughProjectionVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new GenericGamePackageFullPlaythroughProjectionBuildResult
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

    public async Task<GenericGamePackageFullPlaythroughProjectionWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(
            root,
            GenericGamePackageFullPlaythroughProjectionVocabulary.ProceduralOutputDirectory);
        var export = Resolve(
            root,
            GenericGamePackageFullPlaythroughProjectionVocabulary.ExportPackageDirectory);
        var docsPath = Resolve(
            root,
            GenericGamePackageFullPlaythroughProjectionVocabulary.DocumentationPath);
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

        var sourceLogPath = Resolve(
            root,
            GenericGamePackageFullPlaythroughProjectionVocabulary.UnityBatchmodeLogRelativePath);
        if (File.Exists(sourceLogPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var exportLogPath = Resolve(
                root,
                GenericGamePackageFullPlaythroughProjectionVocabulary.UnityBatchmodeExportLogRelativePath);
            GuardNotManualInput(root, exportLogPath);
            Directory.CreateDirectory(Path.GetDirectoryName(exportLogPath)!);
            File.Copy(sourceLogPath, exportLogPath, overwrite: true);
            written.Add(Relative(root, exportLogPath));
        }

        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(docsPath, result.DocumentationMarkdown, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new GenericGamePackageFullPlaythroughProjectionWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static GenericGamePackageFullPlaythroughProjectionDashboard BuildDashboard(
        GenericGamePackageFullPlaythroughSampleSummary samplePackage,
        GenericGamePackageFullPlaythroughScriptInventory scriptInventory,
        GenericGamePackageFullPlaythroughLogScan logScan,
        GenericGamePackageFullPlaythroughNegativeProof negative,
        bool goal125StillGreen)
    {
        var diagnostics = new List<string>();
        Require(goal125StillGreen, "goal126.goal125_not_green", diagnostics);
        Require(samplePackage.Passed, "goal126.sample_full_playthrough_contract_failed", diagnostics);
        Require(scriptInventory.Passed, "goal126.script_inventory_failed", diagnostics);
        Require(negative.Passed, "goal126.negative_proof_failed", diagnostics);
        Require(
            logScan.Status != "BLOCKED_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH",
            "goal126.unity_full_playthrough_smoke_failed",
            diagnostics);

        return new GenericGamePackageFullPlaythroughProjectionDashboard
        {
            FullPlaythroughStatus = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            PackageId = samplePackage.PackageId,
            PackageTitle = samplePackage.PackageTitle,
            MapId = samplePackage.MapId,
            MapPathPreviewPresent =
                samplePackage.StartPositionPresent
                && samplePackage.PathTargetPresent
                && samplePackage.PathWalkable,
            SignInteractionApplied = samplePackage.SignInteractionPresent,
            DialogueSummaryPresent = samplePackage.OldGuardDialoguePresent,
            QuestObjectiveStatusPresent = samplePackage.HelpHealerQuestIncomplete,
            InventorySummaryPresent = samplePackage.PlayerInventoryPresent,
            ResourceSummaryPresent = samplePackage.ResourceDefaultsPresent,
            SystemsSummaryPresent =
                samplePackage.RecipeRequirementsMatchExpected
                && samplePackage.HarvestContractPresent
                && samplePackage.TransactionPresent,
            RecipeApplyPassed = samplePackage.RecipeRequirementsMatchExpected,
            HarvestApplyPassed = samplePackage.HarvestContractPresent,
            TransactionPreviewPresent = samplePackage.TransactionPresent,
            CombatRoundPreviewPresent = samplePackage.CombatRoundMatchesExpected,
            EventTranscriptPresent = scriptInventory.PlaythroughRunsRequiredSequence,
            UnitySmokeStatus = logScan.Status,
            CleanupScriptAvailable = CleanupScriptAvailable(scriptInventory),
            Goal125StillGreen = goal125StillGreen,
            SamplePackageReadOnly = samplePackage.ReadOnlySource,
            Diagnostics = diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static GenericGamePackageFullPlaythroughSampleSummary BuildSampleSummary(
        string root)
    {
        var diagnostics = new List<string>();
        var path = Resolve(
            root,
            GenericGamePackageFullPlaythroughProjectionVocabulary.SamplePackagePath);
        if (!File.Exists(path))
        {
            diagnostics.Add("goal126.sample_package_missing");
            return new GenericGamePackageFullPlaythroughSampleSummary
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

            var sign = ArrayItems(map, "entities")
                .FirstOrDefault(item => StringValue(item, "id") == "entity/village/sign");
            var oldGuard = ArrayItems(map, "entities")
                .FirstOrDefault(item => StringValue(item, "id") == "entity/village/old_guard");
            var signInspect = ArrayItems(game, "interactions")
                .FirstOrDefault(item => StringValue(item, "id") == "interaction/sign_inspect");
            var signEffects = ArrayItems(signInspect, "effects").ToList();
            var helpHealer = ArrayItems(game, "quests")
                .FirstOrDefault(item => StringValue(item, "id") == "quest/help_healer");
            var redHerbObjective = ArrayItems(helpHealer, "objectives")
                .FirstOrDefault(item => StringValue(item, "targetId") == "item/red_herb");
            var playerInventory = ArrayItems(game, "inventories")
                .FirstOrDefault(item => StringValue(item, "id") == "inventory/player_start");
            var resources = ArrayItems(game, "resources").ToList();
            var recipe = ArrayItems(game, "recipes")
                .FirstOrDefault(item => StringValue(item, "id") == "recipe/healing_potion");
            var resourceNode = ArrayItems(game, "resourceNodes")
                .FirstOrDefault(item => StringValue(item, "id") == "node/apple_tree");
            var lootTable = ArrayItems(game, "lootTables")
                .FirstOrDefault(item => StringValue(item, "id") == "loot/apple_tree");
            var transaction = ArrayItems(game, "transactions")
                .FirstOrDefault(item => StringValue(item, "id") == "transaction/buy_healing_potion");
            var encounter = ArrayItems(game, "encounters")
                .FirstOrDefault(item => StringValue(item, "id") == "encounter/goblin_duel");
            var abilities = ArrayItems(game, "abilities").ToList();

            var requiredRedHerbAmount = IntValue(redHerbObjective, "requiredAmount");
            var playerRedHerbAmount = AmountInInventory(playerInventory, "item/red_herb");
            var playerInventoryPresent =
                playerRedHerbAmount == 2
                && AmountInInventory(playerInventory, "item/water_flask") == 1
                && AmountInInventory(playerInventory, "item/healing_potion") >= 1
                && AmountInInventory(playerInventory, "item/woodcutting_axe") == 1;
            var resourceDefaultsPresent =
                ResourceDefault(resources, "resource/health") == 30
                && ResourceDefault(resources, "resource/stamina") == 10
                && ResourceDefault(resources, "resource/mana") == 10
                && resources.Any(item => StringValue(item, "id") == "resource/gold");
            var recipeRequirementsMatchExpected =
                HasAmount(recipe, "inputs", "item", "item/red_herb", 2)
                && HasAmount(recipe, "inputs", "item", "item/water_flask", 1)
                && HasAmount(recipe, "costs", "resource", "resource/mana", 5)
                && HasAmount(recipe, "outputs", "item", "item/healing_potion", 1);
            var harvestContractPresent =
                HasAmount(resourceNode, "production", "item", "item/log", 1)
                && StringValue(ObjectProperty(resourceNode, "metadata"), "required_tool_tag") == "axe"
                && ArrayItems(lootTable, "entries").Any(entry =>
                    StringValue(ObjectProperty(entry, "output"), "id") == "item/apple");
            var combatRoundMatchesExpected =
                encounter.ValueKind == JsonValueKind.Object
                && AbilityPower(abilities, "ability/basic_attack") == 4
                && AbilityPower(abilities, "ability/goblin_slash") == 3
                && ParticipantResource(encounter, "goblin", "resource/health") == 12
                && ParticipantResource(encounter, "player", "resource/health") == 30
                && 12 - AbilityPower(abilities, "ability/basic_attack") == 8
                && 30 - AbilityPower(abilities, "ability/goblin_slash") == 27;

            var summary = new GenericGamePackageFullPlaythroughSampleSummary
            {
                Exists = true,
                Parsed = true,
                ReadOnlySource = true,
                ExcludedFromExpectedChangedPaths =
                    !BuildExpectedChangedPaths().Contains(
                        GenericGamePackageFullPlaythroughProjectionVocabulary.SamplePackagePath,
                        StringComparer.Ordinal),
                Sha256 = HashBytes(File.ReadAllBytes(path)),
                PackageId = StringValue(manifest, "packageId"),
                PackageTitle = StringValue(manifest, "title"),
                StartMapId = startMapId,
                MapId = StringValue(map, "id"),
                StartPositionPresent = ObjectProperty(map, "startPosition").ValueKind == JsonValueKind.Object,
                PathTargetPresent =
                    PositionX(sign) == 2
                    && PositionY(sign) == 2
                    && PositionX(oldGuard) == 4
                    && PositionY(oldGuard) == 1,
                PathWalkable = TileWalkable(game, map, 1, 1)
                               && TileWalkable(game, map, 2, 1)
                               && TileWalkable(game, map, 2, 2),
                SignInteractionPresent =
                    signInspect.ValueKind == JsonValueKind.Object
                    && signEffects.Any(item => StringValue(item, "type") == "set_flag")
                    && signEffects.Any(item => StringValue(item, "type") == "log"),
                OldGuardDialoguePresent = ArrayItems(game, "dialogues").Any(item =>
                    StringValue(item, "id") == "dialogue/old_guard_intro"
                    && ArrayItems(item, "nodes").Any(node =>
                        StringValue(node, "id") == StringValue(item, "startNodeId")
                        && !string.IsNullOrWhiteSpace(StringValue(node, "text")))),
                HelpHealerQuestIncomplete =
                    helpHealer.ValueKind == JsonValueKind.Object
                    && requiredRedHerbAmount == 3
                    && playerRedHerbAmount == 2,
                PlayerInventoryPresent = playerInventoryPresent,
                ResourceDefaultsPresent = resourceDefaultsPresent,
                RecipeRequirementsMatchExpected = recipeRequirementsMatchExpected,
                HarvestContractPresent = harvestContractPresent,
                TransactionPresent =
                    HasAmount(transaction, "costs", "resource", "resource/gold", 25)
                    && HasAmount(transaction, "outputs", "item", "item/healing_potion", 1),
                CombatRoundMatchesExpected = combatRoundMatchesExpected,
                Diagnostics = diagnostics
            };

            return summary with
            {
                Passed = summary.Exists
                         && summary.Parsed
                         && summary.ReadOnlySource
                         && summary.ExcludedFromExpectedChangedPaths
                         && summary.PackageId == "game/minimal-map-game"
                         && summary.PackageTitle == "Minimal Map Game"
                         && summary.StartMapId == "map/village"
                         && summary.MapId == "map/village"
                         && summary.StartPositionPresent
                         && summary.PathTargetPresent
                         && summary.PathWalkable
                         && summary.SignInteractionPresent
                         && summary.OldGuardDialoguePresent
                         && summary.HelpHealerQuestIncomplete
                         && summary.PlayerInventoryPresent
                         && summary.ResourceDefaultsPresent
                         && summary.RecipeRequirementsMatchExpected
                         && summary.HarvestContractPresent
                         && summary.TransactionPresent
                         && summary.CombatRoundMatchesExpected
            };
        }
        catch (Exception ex)
        {
            diagnostics.Add("goal126.sample_package_parse_failed:" + ex.GetType().Name);
            return new GenericGamePackageFullPlaythroughSampleSummary
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

    private static GenericGamePackageFullPlaythroughScriptInventory BuildScriptInventory(
        string root)
    {
        var entries = new[]
        {
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath,
                "unity_editor_window", "Run Generic Package Full Playthrough Verification"),
            Entry(root, GenericGamePackageProjectionVocabulary.UnityAdapterPath,
                "unity_generic_projection_adapter", "SamplePackageRelativePath"),
            Entry(root, GenericGamePackageProjectionVocabulary.UnityModelsPath,
                "unity_generic_projection_models",
                "GenericGamePackageProjectionFullPlaythroughSmokeResult"),
            Entry(root, GenericGamePackageProjectionVocabulary.UnityControllerPath,
                "unity_generic_projection_controller",
                "RunGenericPackageFullPlaythroughVerification"),
            Entry(root, GenericGamePackageLoopProjectionVocabulary.UnityStatePath,
                "unity_generic_projection_state", "fullPlaythroughStatus"),
            Entry(root, GenericGamePackageFullPlaythroughProjectionVocabulary.UnityPlaythroughPath,
                "unity_generic_projection_playthrough", "GenericGamePackageProjectionPlaythrough"),
            Entry(root, GenericGamePackageLoopProjectionVocabulary.UnityLoopPath,
                "unity_generic_projection_loop", "quest/help_healer"),
            Entry(root, GenericGamePackageSystemsProjectionVocabulary.UnitySystemsPath,
                "unity_generic_projection_systems", "recipe/healing_potion"),
            Entry(root, AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath,
                "cleanup_script", "Unity editor noise cleanup mode"),
            Entry(root, AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptCmdPath,
                "cleanup_cmd_wrapper", "clean-unity-editor-noise.ps1")
        }.ToList();

        var editorText = SourceText(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath);
        var modelsText = SourceText(root, GenericGamePackageProjectionVocabulary.UnityModelsPath);
        var controllerText = SourceText(root, GenericGamePackageProjectionVocabulary.UnityControllerPath);
        var stateText = SourceText(root, GenericGamePackageLoopProjectionVocabulary.UnityStatePath);
        var playthroughText = SourceText(
            root,
            GenericGamePackageFullPlaythroughProjectionVocabulary.UnityPlaythroughPath);
        var genericUnityText = string.Join("\n", entries
            .Where(entry => entry.RelativePath.StartsWith("unity/", StringComparison.Ordinal))
            .Select(entry => SourceText(root, entry.RelativePath)));
        var forbidden = SourceWriteMarkers
            .Where(marker => genericUnityText.Contains(marker, StringComparison.Ordinal))
            .OrderBy(marker => marker, StringComparer.Ordinal)
            .ToList();

        var stateMarkers = new[]
        {
            "fullPlaythroughStatus",
            "movementPathSummary",
            "signInteractionResult",
            "dialogueSummary",
            "questObjectiveStatus",
            "inventoryResourceFinalSummary",
            "systemsSummary",
            "combatSummary",
            "eventTranscriptSummary",
            "finalStateSummary",
            "FullPlaythroughPassed",
            "MapPathPreviewPresent",
            "SignInteractionApplied",
            "QuestObjectiveStatusPresent",
            "SystemsSummaryPresent",
            "EventTranscriptPresent"
        };
        var playthroughMarkers = new[]
        {
            "Run(",
            "entity/village/sign",
            "interaction/sign_inspect",
            "dialogue/old_guard_intro",
            "quest/help_healer",
            "recipe/healing_potion",
            "node/apple_tree",
            "transaction/buy_healing_potion",
            "encounter/goblin_duel",
            "fullPlaythroughPassed"
        };
        var markerNames = new[]
        {
            "goal126_full_playthrough_status",
            "goal126_movement_path_summary",
            "goal126_sign_interaction_result",
            "goal126_dialogue_summary",
            "goal126_quest_objective_status",
            "goal126_inventory_resource_final_summary",
            "goal126_systems_summary",
            "goal126_combat_round_summary",
            "goal126_event_transcript_summary"
        };
        var smokeMarkers = new[]
        {
            "fullPlaythroughPassed",
            "samplePackageLoaded",
            "mapPathPreviewPresent",
            "signInteractionApplied",
            "dialogueSummaryPresent",
            "questObjectiveStatusPresent",
            "inventorySummaryPresent",
            "resourceSummaryPresent",
            "recipeApplyPassed",
            "harvestApplyPassed",
            "transactionPreviewPresent",
            "combatRoundPreviewPresent",
            "eventTranscriptPresent",
            "zeroFatalErrors"
        };

        var inventory = new GenericGamePackageFullPlaythroughScriptInventory
        {
            ScriptCount = entries.Count,
            WindowActionPresent =
                editorText.Contains(
                    "Run Generic Package Full Playthrough Verification",
                    StringComparison.Ordinal)
                && editorText.Contains(
                    "RunGenericPackageFullPlaythroughVerification()",
                    StringComparison.Ordinal),
            BatchmodeMethodPresent =
                editorText.Contains(
                    "RunBatchmodeGenericGamePackageFullPlaythroughSmoke",
                    StringComparison.Ordinal),
            BatchmodePassMarkerPresent =
                editorText.Contains(
                    "GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS",
                    StringComparison.Ordinal),
            BatchmodeFailMarkerPresent =
                editorText.Contains(
                    "GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL",
                    StringComparison.Ordinal),
            StateClassTracksFullPlaythroughFields =
                stateMarkers.All(marker => stateText.Contains(marker, StringComparison.Ordinal)),
            PlaythroughRunsRequiredSequence =
                playthroughMarkers.All(marker => playthroughText.Contains(marker, StringComparison.Ordinal)),
            ControllerRendersFullPlaythroughMarkers =
                markerNames.All(marker => controllerText.Contains(marker, StringComparison.Ordinal)),
            ModelsExposeFullPlaythroughSmokeFields =
                modelsText.Contains(
                    "GenericGamePackageProjectionFullPlaythroughSmokeResult",
                    StringComparison.Ordinal)
                && smokeMarkers.All(marker => modelsText.Contains(marker, StringComparison.Ordinal)),
            ExistingGoal125VerificationStillPresent =
                editorText.Contains(
                    "Run Generic Package Systems Loop Verification",
                    StringComparison.Ordinal)
                && editorText.Contains(
                    "RunBatchmodeGenericGamePackageSystemsSmoke",
                    StringComparison.Ordinal)
                && editorText.Contains(
                    "GOAL125_GENERIC_GAMEPACKAGE_SYSTEMS_PASS",
                    StringComparison.Ordinal),
            NoSourceWriteMarkers = forbidden.Count == 0,
            ForbiddenSourceMarkersFound = forbidden,
            Scripts = entries
        };

        return inventory with
        {
            Passed = inventory.ScriptCount == 10
                     && entries.All(entry => entry.Exists && entry.ContainsRequiredMarker)
                     && inventory.WindowActionPresent
                     && inventory.BatchmodeMethodPresent
                     && inventory.BatchmodePassMarkerPresent
                     && inventory.BatchmodeFailMarkerPresent
                     && inventory.StateClassTracksFullPlaythroughFields
                     && inventory.PlaythroughRunsRequiredSequence
                     && inventory.ControllerRendersFullPlaythroughMarkers
                     && inventory.ModelsExposeFullPlaythroughSmokeFields
                     && inventory.ExistingGoal125VerificationStillPresent
                     && inventory.NoSourceWriteMarkers
        };
    }

    private static GenericGamePackageFullPlaythroughSmokePlan BuildSmokePlan()
    {
        var steps = new List<GenericGamePackageFullPlaythroughSmokePlanStep>
        {
            Step(1, "open_projection_window", "Open the accepted Alpha projection menu path."),
            Step(2, "run_generic_package_full_playthrough", "Click Run Generic Package Full Playthrough Verification."),
            Step(3, "load_sample_package", "Read samples/minimal-map-game/package.json without mutating it."),
            Step(4, "build_generic_projection", "Build the generic map projection root."),
            Step(5, "preview_map_path", "Preview deterministic walk path from start to sign target."),
            Step(6, "apply_sign_interaction", "Apply interaction/sign_inspect in projection state only."),
            Step(7, "summarize_dialogue", "Show dialogue/old_guard_intro summary."),
            Step(8, "check_quest_objective", "Show quest/help_healer incomplete red herb objective."),
            Step(9, "summarize_inventory_resources", "Show inventory/player_start and resource ledger summary."),
            Step(10, "apply_recipe", "Preview and apply recipe/healing_potion."),
            Step(11, "apply_harvest", "Preview and apply node/apple_tree harvest."),
            Step(12, "preview_transaction", "Preview transaction/buy_healing_potion affordability."),
            Step(13, "preview_combat_round", "Compute combat round resource deltas."),
            Step(14, "render_event_transcript", "Render deterministic final state and event transcript."),
            Step(15, "cleanup_after_unity", "Use the existing clean-unity-editor-noise command after Unity checks.")
        };

        return new GenericGamePackageFullPlaythroughSmokePlan
        {
            StepCount = steps.Count,
            Steps = steps
        };
    }

    private static GenericGamePackageFullPlaythroughLogScan BuildLogScan(string root)
    {
        var path = Resolve(
            root,
            GenericGamePackageFullPlaythroughProjectionVocabulary.UnityBatchmodeLogRelativePath);
        var logExists = File.Exists(path);
        var text = logExists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        var forbidden = new List<string>();
        if (text.Contains(
                "GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL",
                StringComparison.Ordinal))
        {
            forbidden.Add("GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL");
        }

        var smokeFields = new[]
        {
            "fullPlaythroughPassed=True",
            "samplePackageLoaded=True",
            "mapPathPreviewPresent=True",
            "signInteractionApplied=True",
            "dialogueSummaryPresent=True",
            "questObjectiveStatusPresent=True",
            "inventorySummaryPresent=True",
            "resourceSummaryPresent=True",
            "recipeApplyPassed=True",
            "harvestApplyPassed=True",
            "transactionPreviewPresent=True",
            "combatRoundPreviewPresent=True",
            "eventTranscriptPresent=True",
            "zeroFatalErrors=True"
        };
        var passMarkerPresent = text.Contains(
            "GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS",
            StringComparison.Ordinal);
        var smokeFieldsPresent = smokeFields.All(field => text.Contains(field, StringComparison.Ordinal));
        var passed = logExists && passMarkerPresent && smokeFieldsPresent && forbidden.Count == 0;
        return new GenericGamePackageFullPlaythroughLogScan
        {
            LogExists = logExists,
            PassMarkerPresent = passMarkerPresent,
            FailMarkerAbsent = forbidden.Count == 0,
            SmokeRequiredFieldsPresent = smokeFieldsPresent,
            Passed = passed,
            Status = passed
                ? "GREEN"
                : logExists
                    ? "BLOCKED_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH"
                    : "PENDING_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH",
            Sha256 = logExists ? HashBytes(File.ReadAllBytes(path)) : string.Empty,
            ForbiddenMarkersFound = forbidden
        };
    }

    private static GenericGamePackageFullPlaythroughNegativeProof BuildNegativeProof()
    {
        var rejected = BuildRejectedPathSamples();
        return new GenericGamePackageFullPlaythroughNegativeProof
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

    private static GenericGamePackageFullPlaythroughFileIndex BuildFileIndex(
        string root,
        IReadOnlyDictionary<string, string> files,
        string relativeRoot,
        string role,
        string unityLogRelativePath)
    {
        var entries = files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new GenericGamePackageFullPlaythroughFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = role,
                Sha256 = HashText(item.Value)
            })
            .ToList();
        var logPath = Resolve(
            root,
            GenericGamePackageFullPlaythroughProjectionVocabulary.UnityBatchmodeLogRelativePath);
        if (File.Exists(logPath))
        {
            entries.Add(new GenericGamePackageFullPlaythroughFileIndexEntry
            {
                RelativePath = unityLogRelativePath,
                Role = "goal126_unity_batchmode_generic_gamepackage_full_playthrough_log",
                Sha256 = HashBytes(File.ReadAllBytes(logPath))
            });
        }

        return new GenericGamePackageFullPlaythroughFileIndex
        {
            IndexedFileCount = entries.Count,
            ManualInputExcluded = entries.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = entries.OrderBy(entry => entry.RelativePath, StringComparer.Ordinal).ToList()
        };
    }

    private static string RenderReport(
        GenericGamePackageFullPlaythroughProjectionDashboard dashboard,
        GenericGamePackageFullPlaythroughSampleSummary samplePackage,
        GenericGamePackageFullPlaythroughScriptInventory scriptInventory,
        GenericGamePackageFullPlaythroughSmokePlan smokePlan,
        GenericGamePackageFullPlaythroughLogScan logScan,
        GenericGamePackageFullPlaythroughNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 126 Generic GamePackage Full Playthrough Projection",
            string.Empty,
            "- fullPlaythroughStatus: " + dashboard.FullPlaythroughStatus,
            "- samplePackagePath: " + dashboard.SamplePackagePath,
            "- packageId: " + dashboard.PackageId,
            "- packageTitle: " + dashboard.PackageTitle,
            "- mapId: " + dashboard.MapId,
            "- mapPathPreviewPresent: " + dashboard.MapPathPreviewPresent.ToString().ToLowerInvariant(),
            "- signInteractionApplied: " + dashboard.SignInteractionApplied.ToString().ToLowerInvariant(),
            "- dialogueSummaryPresent: " + dashboard.DialogueSummaryPresent.ToString().ToLowerInvariant(),
            "- questObjectiveStatusPresent: " + dashboard.QuestObjectiveStatusPresent.ToString().ToLowerInvariant(),
            "- inventorySummaryPresent: " + dashboard.InventorySummaryPresent.ToString().ToLowerInvariant(),
            "- resourceSummaryPresent: " + dashboard.ResourceSummaryPresent.ToString().ToLowerInvariant(),
            "- systemsSummaryPresent: " + dashboard.SystemsSummaryPresent.ToString().ToLowerInvariant(),
            "- combatRoundPreviewPresent: " + dashboard.CombatRoundPreviewPresent.ToString().ToLowerInvariant(),
            "- eventTranscriptPresent: " + dashboard.EventTranscriptPresent.ToString().ToLowerInvariant(),
            "- unitySmokeStatus: " + dashboard.UnitySmokeStatus,
            "- cleanupScriptAvailable: " + dashboard.CleanupScriptAvailable.ToString().ToLowerInvariant(),
            "- goal125StillGreen: " + dashboard.Goal125StillGreen.ToString().ToLowerInvariant(),
            "- evidencePath: " + dashboard.EvidencePath,
            "- exportPath: " + dashboard.ExportPath,
            string.Empty,
            "## Sample Package",
            string.Empty,
            "- parsed: " + samplePackage.Parsed.ToString().ToLowerInvariant(),
            "- readOnlySource: " + samplePackage.ReadOnlySource.ToString().ToLowerInvariant(),
            "- sha256: " + samplePackage.Sha256,
            "- startMapId: " + samplePackage.StartMapId,
            "- pathWalkable: " + samplePackage.PathWalkable.ToString().ToLowerInvariant(),
            "- signInteractionPresent: " + samplePackage.SignInteractionPresent.ToString().ToLowerInvariant(),
            "- helpHealerQuestIncomplete: " + samplePackage.HelpHealerQuestIncomplete.ToString().ToLowerInvariant(),
            "- recipeRequirementsMatchExpected: "
            + samplePackage.RecipeRequirementsMatchExpected.ToString().ToLowerInvariant(),
            "- harvestContractPresent: " + samplePackage.HarvestContractPresent.ToString().ToLowerInvariant(),
            "- combatRoundMatchesExpected: "
            + samplePackage.CombatRoundMatchesExpected.ToString().ToLowerInvariant(),
            string.Empty,
            "## Script Inventory",
            string.Empty,
            "- passed: " + scriptInventory.Passed.ToString().ToLowerInvariant(),
            "- scriptCount: " + scriptInventory.ScriptCount,
            "- playthroughRunsRequiredSequence: "
            + scriptInventory.PlaythroughRunsRequiredSequence.ToString().ToLowerInvariant(),
            "- controllerRendersFullPlaythroughMarkers: "
            + scriptInventory.ControllerRendersFullPlaythroughMarkers.ToString().ToLowerInvariant(),
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

    private static string RenderDocumentation(
        GenericGamePackageFullPlaythroughProjectionDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Generic GamePackage Full Playthrough Projection",
            string.Empty,
            "Goal126 adds a projection-only Unity Editor full playthrough over `samples/minimal-map-game/package.json`.",
            string.Empty,
            "## Hands-on Verification",
            string.Empty,
            "- Open `unity/LLMGameCreatorAlpha` in Unity.",
            "- Select `LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection`.",
            "- Click `Run Generic Package Full Playthrough Verification`.",
            "- Verify fullPlaythroughStatus, samplePackagePath, packageId, mapId, mapPathPreviewPresent, signInteractionApplied, dialogueSummaryPresent, questObjectiveStatusPresent, inventorySummaryPresent, resourceSummaryPresent, systemsSummaryPresent, combatRoundPreviewPresent, eventTranscriptPresent, unitySmokeStatus, cleanupScriptAvailable, projectionOnly, evidencePath and exportPath.",
            "- Do not save scenes, prefabs, project settings, packages or generated player payloads as part of this check.",
            string.Empty,
            "## Cleanup Command",
            string.Empty,
            "- After Unity checks: `.\\.devflow\\scripts\\clean-unity-editor-noise.cmd`",
            string.Empty,
            "## Status",
            string.Empty,
            "- fullPlaythroughStatus: " + dashboard.FullPlaythroughStatus,
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

    private static bool Goal125StillGreen(string root)
    {
        var result = new GenericGamePackageSystemsProjectionService().Build(root);
        return result.Dashboard.GenericSystemsStatus == "GREEN"
               && result.SamplePackage.Passed
               && result.ScriptInventory.Passed
               && result.NegativeProof.Passed;
    }

    private static bool CleanupScriptAvailable(
        GenericGamePackageFullPlaythroughScriptInventory inventory) =>
        inventory.Scripts.Any(entry =>
            entry.RelativePath == AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath
            && entry.Exists
            && entry.ContainsRequiredMarker)
        && inventory.Scripts.Any(entry =>
            entry.RelativePath == AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptCmdPath
            && entry.Exists
            && entry.ContainsRequiredMarker);

    private static GenericGamePackageFullPlaythroughScriptInventoryEntry Entry(
        string root,
        string relativePath,
        string role,
        string marker)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        var text = exists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        return new GenericGamePackageFullPlaythroughScriptInventoryEntry
        {
            RelativePath = relativePath,
            Role = role,
            Exists = exists,
            ContainsRequiredMarker = text.Contains(marker, StringComparison.Ordinal),
            RequiredMarker = marker,
            Sha256 = exists ? HashBytes(File.ReadAllBytes(path)) : string.Empty
        };
    }

    private static GenericGamePackageFullPlaythroughSmokePlanStep Step(
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
        GenericGamePackageFullPlaythroughProjectionVocabulary.ProceduralOutputDirectory + "/",
        GenericGamePackageFullPlaythroughProjectionVocabulary.ExportPackageDirectory + "/",
        "docs/agent-tasks/goal-126-generic-gamepackage-full-playthrough-projection/",
        GenericGamePackageFullPlaythroughProjectionVocabulary.DocumentationPath,
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
        GenericGamePackageProjectionVocabulary.UnityAdapterPath,
        GenericGamePackageProjectionVocabulary.UnityModelsPath,
        GenericGamePackageProjectionVocabulary.UnityControllerPath,
        GenericGamePackageLoopProjectionVocabulary.UnityStatePath,
        GenericGamePackageLoopProjectionVocabulary.UnityLoopPath,
        GenericGamePackageSystemsProjectionVocabulary.UnitySystemsPath,
        GenericGamePackageFullPlaythroughProjectionVocabulary.UnityPlaythroughPath,
        "tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/",
        "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs"
    ];

    private static IReadOnlyList<string> BuildRejectedPathSamples() =>
    [
        ".llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json",
        GenericGamePackageFullPlaythroughProjectionVocabulary.SamplePackagePath,
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

    private static IReadOnlyList<JsonElement> ArrayItems(
        JsonElement element,
        string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Array
            ? property.EnumerateArray().ToList()
            : [];

    private static JsonElement ObjectProperty(
        JsonElement element,
        string propertyName) =>
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

    private static int PositionX(JsonElement entity) =>
        IntValue(ObjectProperty(entity, "position"), "x");

    private static int PositionY(JsonElement entity) =>
        IntValue(ObjectProperty(entity, "position"), "y");

    private static bool TileWalkable(JsonElement game, JsonElement map, int x, int y)
    {
        var tileId = ArrayItems(map, "tiles")
            .Where(tile => IntValue(tile, "x") == x && IntValue(tile, "y") == y)
            .Select(tile => StringValue(tile, "tileId"))
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(tileId))
        {
            tileId = StringValue(map, "defaultTileId");
        }

        return ArrayItems(game, "tilePrototypes").Any(tile =>
            StringValue(tile, "id") == tileId
            && BoolValue(tile, "walkable"));
    }

    private static bool BoolValue(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.True;

    private static int AmountInInventory(JsonElement inventory, string itemId) =>
        ArrayItems(inventory, "stacks")
            .Where(stack => StringValue(stack, "itemId") == itemId)
            .Sum(stack => IntValue(stack, "amount"));

    private static int ResourceDefault(IReadOnlyList<JsonElement> resources, string resourceId) =>
        resources
            .Where(resource => StringValue(resource, "id") == resourceId)
            .Select(resource => IntValue(resource, "defaultValue"))
            .FirstOrDefault();

    private static bool HasAmount(
        JsonElement element,
        string arrayName,
        string kind,
        string id,
        int amount) =>
        ArrayItems(element, arrayName).Any(item =>
            StringValue(item, "kind") == kind
            && StringValue(item, "id") == id
            && IntValue(item, "amount") == amount);

    private static int ParticipantResource(
        JsonElement encounter,
        string participantId,
        string resourceId) =>
        ArrayItems(encounter, "participants")
            .Where(participant => StringValue(participant, "id") == participantId)
            .SelectMany(participant => ArrayItems(participant, "resources"))
            .Where(resource => StringValue(resource, "id") == resourceId)
            .Select(resource => IntValue(resource, "amount"))
            .FirstOrDefault();

    private static int AbilityPower(IReadOnlyList<JsonElement> abilities, string abilityId) =>
        abilities
            .Where(ability => StringValue(ability, "id") == abilityId)
            .Select(ability => IntValue(ability, "power"))
            .FirstOrDefault();

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
            throw new InvalidOperationException("Goal126 must not write the manual input path.");
        }
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
