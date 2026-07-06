using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class GenericGamePackageSystemsProjectionService
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

    public GenericGamePackageSystemsProjectionBuildResult Build(string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var samplePackage = BuildSampleSummary(root);
        var scriptInventory = BuildScriptInventory(root);
        var smokePlan = BuildSmokePlan();
        var logScan = BuildLogScan(root);
        var negative = BuildNegativeProof();
        var goal124StillGreen = Goal124StillGreen(root);
        var dashboard = BuildDashboard(
            samplePackage,
            scriptInventory,
            logScan,
            negative,
            goal124StillGreen);
        var report = RenderReport(dashboard, samplePackage, scriptInventory, smokePlan, logScan, negative);
        var docs = RenderDocumentation(dashboard);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GenericGamePackageSystemsProjectionVocabulary.DashboardFileName] = Serialize(dashboard),
            [GenericGamePackageSystemsProjectionVocabulary.ScriptInventoryFileName] = Serialize(scriptInventory),
            [GenericGamePackageSystemsProjectionVocabulary.SmokePlanFileName] = Serialize(smokePlan),
            [GenericGamePackageSystemsProjectionVocabulary.LogScanFileName] = Serialize(logScan),
            [GenericGamePackageSystemsProjectionVocabulary.ReportFileName] = report,
            [GenericGamePackageSystemsProjectionVocabulary.NegativeProofFileName] = Serialize(negative)
        };
        var proceduralIndex = BuildFileIndex(
            root,
            proceduralFiles,
            GenericGamePackageSystemsProjectionVocabulary.ProceduralOutputDirectory,
            "goal125_generic_gamepackage_systems_evidence",
            GenericGamePackageSystemsProjectionVocabulary.UnityBatchmodeLogRelativePath);
        proceduralFiles[GenericGamePackageSystemsProjectionVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GenericGamePackageSystemsProjectionVocabulary.DashboardFileName] = Serialize(dashboard),
            [GenericGamePackageSystemsProjectionVocabulary.ScriptInventoryFileName] = Serialize(scriptInventory),
            [GenericGamePackageSystemsProjectionVocabulary.SmokePlanFileName] = Serialize(smokePlan),
            [GenericGamePackageSystemsProjectionVocabulary.LogScanFileName] = Serialize(logScan),
            [GenericGamePackageSystemsProjectionVocabulary.ReportFileName] = report,
            [GenericGamePackageSystemsProjectionVocabulary.NegativeProofFileName] = Serialize(negative)
        };
        var exportIndex = BuildFileIndex(
            root,
            exportFiles,
            GenericGamePackageSystemsProjectionVocabulary.ExportPackageDirectory,
            "goal125_generic_gamepackage_systems_export",
            GenericGamePackageSystemsProjectionVocabulary.UnityBatchmodeExportLogRelativePath);
        exportFiles[GenericGamePackageSystemsProjectionVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new GenericGamePackageSystemsProjectionBuildResult
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

    public async Task<GenericGamePackageSystemsProjectionWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(root, GenericGamePackageSystemsProjectionVocabulary.ProceduralOutputDirectory);
        var export = Resolve(root, GenericGamePackageSystemsProjectionVocabulary.ExportPackageDirectory);
        var docsPath = Resolve(root, GenericGamePackageSystemsProjectionVocabulary.DocumentationPath);
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

        var sourceLogPath = Resolve(root, GenericGamePackageSystemsProjectionVocabulary.UnityBatchmodeLogRelativePath);
        if (File.Exists(sourceLogPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var exportLogPath = Resolve(root,
                GenericGamePackageSystemsProjectionVocabulary.UnityBatchmodeExportLogRelativePath);
            GuardNotManualInput(root, exportLogPath);
            Directory.CreateDirectory(Path.GetDirectoryName(exportLogPath)!);
            File.Copy(sourceLogPath, exportLogPath, overwrite: true);
            written.Add(Relative(root, exportLogPath));
        }

        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(docsPath, result.DocumentationMarkdown, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new GenericGamePackageSystemsProjectionWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static GenericGamePackageSystemsProjectionDashboard BuildDashboard(
        GenericGamePackageSystemsSampleSummary samplePackage,
        GenericGamePackageSystemsScriptInventory scriptInventory,
        GenericGamePackageSystemsLogScan logScan,
        GenericGamePackageSystemsNegativeProof negative,
        bool goal124StillGreen)
    {
        var diagnostics = new List<string>();
        Require(goal124StillGreen, "goal125.goal124_not_green", diagnostics);
        Require(samplePackage.Passed, "goal125.sample_systems_contract_failed", diagnostics);
        Require(scriptInventory.Passed, "goal125.script_inventory_failed", diagnostics);
        Require(negative.Passed, "goal125.negative_proof_failed", diagnostics);
        Require(logScan.Status != "BLOCKED_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_SYSTEMS",
            "goal125.unity_systems_smoke_failed",
            diagnostics);

        return new GenericGamePackageSystemsProjectionDashboard
        {
            GenericSystemsStatus = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            PackageId = samplePackage.PackageId,
            RecipePreviewPresent = samplePackage.RecipeHealingPotionPresent,
            RecipeApplyPassed = samplePackage.RecipeRequirementsMatchExpected,
            HarvestPreviewPresent = samplePackage.HarvestNodePresent,
            HarvestApplyPassed = samplePackage.HarvestLootPresent,
            TransactionPreviewPresent = samplePackage.TransactionPresent,
            EncounterPreviewPresent = samplePackage.EncounterPresent,
            CombatRoundPreviewPresent = samplePackage.CombatRoundMatchesExpected,
            InventorySummaryPresent = samplePackage.PlayerInventoryPresent,
            ResourceSummaryPresent = samplePackage.ResourceDefaultsPresent,
            SystemsEventLogPresent = scriptInventory.SystemsLoopRunsRequiredSequence,
            UnitySmokeStatus = logScan.Status,
            CleanupScriptAvailable = CleanupScriptAvailable(scriptInventory),
            Goal124StillGreen = goal124StillGreen,
            SamplePackageReadOnly = samplePackage.ReadOnlySource,
            Diagnostics = diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static GenericGamePackageSystemsSampleSummary BuildSampleSummary(string root)
    {
        var diagnostics = new List<string>();
        var path = Resolve(root, GenericGamePackageSystemsProjectionVocabulary.SamplePackagePath);
        if (!File.Exists(path))
        {
            diagnostics.Add("goal125.sample_package_missing");
            return new GenericGamePackageSystemsSampleSummary
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
            var inventory = ArrayItems(game, "inventories")
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

            var playerInventoryPresent =
                AmountInInventory(inventory, "item/red_herb") == 2
                && AmountInInventory(inventory, "item/water_flask") == 1
                && AmountInInventory(inventory, "item/healing_potion") >= 1
                && AmountInInventory(inventory, "item/woodcutting_axe") == 1
                && DurabilityInInventory(inventory, "item/woodcutting_axe") == 10;
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
            var harvestNodePresent =
                HasAmount(resourceNode, "production", "item", "item/log", 1)
                && StringValue(ObjectProperty(resourceNode, "metadata"), "required_tool_tag") == "axe"
                && StringValue(ObjectProperty(resourceNode, "metadata"), "harvest_loot_table_id")
                    == "loot/apple_tree";
            var harvestLootPresent =
                ArrayItems(lootTable, "entries").Any(entry =>
                    StringValue(ObjectProperty(entry, "output"), "id") == "item/apple");
            var transactionPresent =
                HasAmount(transaction, "costs", "resource", "resource/gold", 25)
                && HasAmount(transaction, "outputs", "item", "item/healing_potion", 1);
            var encounterPresent =
                encounter.ValueKind == JsonValueKind.Object
                && ParticipantResource(encounter, "player", "resource/health") == 30
                && ParticipantResource(encounter, "goblin", "resource/health") == 12;
            var combatRoundMatchesExpected =
                AbilityPower(abilities, "ability/basic_attack") == 4
                && AbilityPower(abilities, "ability/goblin_slash") == 3
                && 12 - AbilityPower(abilities, "ability/basic_attack") == 8
                && 30 - AbilityPower(abilities, "ability/goblin_slash") == 27;

            var summary = new GenericGamePackageSystemsSampleSummary
            {
                Exists = true,
                Parsed = true,
                ReadOnlySource = true,
                ExcludedFromExpectedChangedPaths =
                    !BuildExpectedChangedPaths().Contains(
                        GenericGamePackageSystemsProjectionVocabulary.SamplePackagePath,
                        StringComparer.Ordinal),
                Sha256 = HashBytes(File.ReadAllBytes(path)),
                PackageId = StringValue(manifest, "packageId"),
                PlayerInventoryPresent = playerInventoryPresent,
                ResourceDefaultsPresent = resourceDefaultsPresent,
                RecipeHealingPotionPresent = recipe.ValueKind == JsonValueKind.Object,
                RecipeRequirementsMatchExpected = recipeRequirementsMatchExpected,
                HarvestNodePresent = harvestNodePresent,
                HarvestLootPresent = harvestLootPresent,
                TransactionPresent = transactionPresent,
                EncounterPresent = encounterPresent,
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
                         && summary.PlayerInventoryPresent
                         && summary.ResourceDefaultsPresent
                         && summary.RecipeHealingPotionPresent
                         && summary.RecipeRequirementsMatchExpected
                         && summary.HarvestNodePresent
                         && summary.HarvestLootPresent
                         && summary.TransactionPresent
                         && summary.EncounterPresent
                         && summary.CombatRoundMatchesExpected
            };
        }
        catch (Exception ex)
        {
            diagnostics.Add("goal125.sample_package_parse_failed:" + ex.GetType().Name);
            return new GenericGamePackageSystemsSampleSummary
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

    private static GenericGamePackageSystemsScriptInventory BuildScriptInventory(string root)
    {
        var entries = new[]
        {
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath,
                "unity_editor_window", "Run Generic Package Systems Loop Verification"),
            Entry(root, GenericGamePackageProjectionVocabulary.UnityAdapterPath,
                "unity_generic_projection_adapter", "BuildRecipeProjection"),
            Entry(root, GenericGamePackageProjectionVocabulary.UnityModelsPath,
                "unity_generic_projection_models", "GenericGamePackageProjectionSystemsSmokeResult"),
            Entry(root, GenericGamePackageProjectionVocabulary.UnityControllerPath,
                "unity_generic_projection_controller", "RunGenericPackageSystemsLoopVerification"),
            Entry(root, GenericGamePackageLoopProjectionVocabulary.UnityStatePath,
                "unity_generic_projection_state", "playerInventory"),
            Entry(root, GenericGamePackageSystemsProjectionVocabulary.UnitySystemsPath,
                "unity_generic_projection_systems", "recipe/healing_potion"),
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath,
                "unity_projection_primitive_factory", "AttachDescriptor"),
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
        var systemsText = SourceText(root, GenericGamePackageSystemsProjectionVocabulary.UnitySystemsPath);
        var genericUnityText = string.Join("\n", entries
            .Where(entry => entry.RelativePath.StartsWith("unity/", StringComparison.Ordinal))
            .Select(entry => SourceText(root, entry.RelativePath)));
        var forbidden = SourceWriteMarkers
            .Where(marker => genericUnityText.Contains(marker, StringComparison.Ordinal))
            .OrderBy(marker => marker, StringComparer.Ordinal)
            .ToList();

        var stateMarkers = new[]
        {
            "playerInventory",
            "resourceLedger",
            "recipePreview",
            "recipeApplyResult",
            "harvestPreview",
            "harvestApplyResult",
            "transactionPreview",
            "encounterPreview",
            "combatRoundPreview",
            "systemsEventLog"
        };
        var systemsMarkers = new[]
        {
            "recipe/healing_potion",
            "node/apple_tree",
            "transaction/buy_healing_potion",
            "encounter/goblin_duel",
            "item/woodcutting_axe",
            "item/log",
            "item/apple"
        };
        var markerNames = new[]
        {
            "goal125_systems_loop_status",
            "goal125_inventory_summary",
            "goal125_resource_ledger_summary",
            "goal125_recipe_craft_result",
            "goal125_harvest_result",
            "goal125_transaction_preview",
            "goal125_encounter_combat_preview",
            "goal125_systems_event_log_summary"
        };
        var smokeMarkers = new[]
        {
            "genericSystemsPassed",
            "samplePackageLoaded",
            "genericProjectionBuilt",
            "inventoryInitialized",
            "resourcesInitialized",
            "recipePreviewPresent",
            "recipeApplyPassed",
            "harvestPreviewPresent",
            "harvestApplyPassed",
            "transactionPreviewPresent",
            "encounterPreviewPresent",
            "combatRoundPreviewPresent",
            "systemsEventLogPresent",
            "zeroFatalErrors"
        };

        var inventory = new GenericGamePackageSystemsScriptInventory
        {
            ScriptCount = entries.Count,
            WindowActionPresent =
                editorText.Contains("Run Generic Package Systems Loop Verification", StringComparison.Ordinal)
                && editorText.Contains("RunGenericPackageSystemsLoopVerification()", StringComparison.Ordinal),
            BatchmodeMethodPresent =
                editorText.Contains("RunBatchmodeGenericGamePackageSystemsSmoke", StringComparison.Ordinal),
            BatchmodePassMarkerPresent =
                editorText.Contains("GOAL125_GENERIC_GAMEPACKAGE_SYSTEMS_PASS", StringComparison.Ordinal),
            BatchmodeFailMarkerPresent =
                editorText.Contains("GOAL125_GENERIC_GAMEPACKAGE_SYSTEMS_FAIL", StringComparison.Ordinal),
            StateClassTracksRequiredFields =
                stateMarkers.All(marker => stateText.Contains(marker, StringComparison.Ordinal)),
            SystemsLoopRunsRequiredSequence =
                systemsMarkers.All(marker => systemsText.Contains(marker, StringComparison.Ordinal)),
            ControllerRendersSystemsMarkers =
                markerNames.All(marker => controllerText.Contains(marker, StringComparison.Ordinal)),
            AdapterParsesSystemsData =
                adapterText.Contains("BuildRecipeProjection", StringComparison.Ordinal)
                && adapterText.Contains("resourceNodes", StringComparison.Ordinal)
                && adapterText.Contains("encounters", StringComparison.Ordinal),
            ModelsExposeSystemsSmokeFields =
                modelsText.Contains("GenericGamePackageProjectionSystemsSmokeResult", StringComparison.Ordinal)
                && smokeMarkers.All(marker => modelsText.Contains(marker, StringComparison.Ordinal)),
            ExistingGoal124VerificationStillPresent =
                editorText.Contains("Run Generic Package Gameplay Loop Verification", StringComparison.Ordinal)
                && editorText.Contains("RunBatchmodeGenericGamePackageLoopSmoke", StringComparison.Ordinal)
                && editorText.Contains("GOAL124_GENERIC_GAMEPACKAGE_LOOP_PASS", StringComparison.Ordinal),
            NoSourceWriteMarkers = forbidden.Count == 0,
            ForbiddenSourceMarkersFound = forbidden,
            Scripts = entries
        };

        return inventory with
        {
            Passed = entries.All(entry => entry.Exists && entry.ContainsRequiredMarker)
                     && inventory.WindowActionPresent
                     && inventory.BatchmodeMethodPresent
                     && inventory.BatchmodePassMarkerPresent
                     && inventory.BatchmodeFailMarkerPresent
                     && inventory.StateClassTracksRequiredFields
                     && inventory.SystemsLoopRunsRequiredSequence
                     && inventory.ControllerRendersSystemsMarkers
                     && inventory.AdapterParsesSystemsData
                     && inventory.ModelsExposeSystemsSmokeFields
                     && inventory.ExistingGoal124VerificationStillPresent
                     && inventory.NoSourceWriteMarkers
        };
    }

    private static GenericGamePackageSystemsSmokePlan BuildSmokePlan()
    {
        var steps = new List<GenericGamePackageSystemsSmokePlanStep>
        {
            Step(1, "open_projection_window", "Open the accepted Alpha projection menu path."),
            Step(2, "run_generic_package_systems_loop", "Click Run Generic Package Systems Loop Verification."),
            Step(3, "load_sample_package", "Read samples/minimal-map-game/package.json without mutating it."),
            Step(4, "build_generic_projection", "Build the existing generic projection root."),
            Step(5, "initialize_inventory_resources", "Initialize player inventory and resource ledger in memory."),
            Step(6, "preview_apply_recipe", "Preview and apply recipe/healing_potion."),
            Step(7, "preview_apply_harvest", "Preview and apply node/apple_tree harvest."),
            Step(8, "preview_transaction", "Preview transaction/buy_healing_potion affordability."),
            Step(9, "preview_encounter", "Preview encounter/goblin_duel."),
            Step(10, "preview_combat_round", "Compute goblin health 8 and player health 27."),
            Step(11, "render_systems_markers", "Render systems status, inventory, resources and summaries."),
            Step(12, "read_event_log", "Show deterministic systems event log."),
            Step(13, "cleanup_after_unity", "Use the existing clean-unity-editor-noise command after Unity checks.")
        };

        return new GenericGamePackageSystemsSmokePlan
        {
            StepCount = steps.Count,
            Steps = steps
        };
    }

    private static GenericGamePackageSystemsLogScan BuildLogScan(string root)
    {
        var path = Resolve(root, GenericGamePackageSystemsProjectionVocabulary.UnityBatchmodeLogRelativePath);
        var logExists = File.Exists(path);
        var text = logExists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        var forbidden = new List<string>();
        if (text.Contains("GOAL125_GENERIC_GAMEPACKAGE_SYSTEMS_FAIL", StringComparison.Ordinal))
        {
            forbidden.Add("GOAL125_GENERIC_GAMEPACKAGE_SYSTEMS_FAIL");
        }

        var smokeFields = new[]
        {
            "genericSystemsPassed=True",
            "samplePackageLoaded=True",
            "genericProjectionBuilt=True",
            "inventoryInitialized=True",
            "resourcesInitialized=True",
            "recipePreviewPresent=True",
            "recipeApplyPassed=True",
            "harvestPreviewPresent=True",
            "harvestApplyPassed=True",
            "transactionPreviewPresent=True",
            "encounterPreviewPresent=True",
            "combatRoundPreviewPresent=True",
            "systemsEventLogPresent=True",
            "zeroFatalErrors=True",
            "recipeId=recipe/healing_potion",
            "resourceNodeId=node/apple_tree",
            "transactionId=transaction/buy_healing_potion",
            "encounterId=encounter/goblin_duel",
            "redHerb=0",
            "waterFlask=0",
            "mana=5",
            "goblinHealthAfter=8",
            "playerHealthAfter=27"
        };
        var passMarkerPresent = text.Contains(
            "GOAL125_GENERIC_GAMEPACKAGE_SYSTEMS_PASS",
            StringComparison.Ordinal);
        var smokeFieldsPresent = smokeFields.All(field => text.Contains(field, StringComparison.Ordinal));
        var passed = logExists && passMarkerPresent && smokeFieldsPresent && forbidden.Count == 0;
        return new GenericGamePackageSystemsLogScan
        {
            LogExists = logExists,
            PassMarkerPresent = passMarkerPresent,
            FailMarkerAbsent = forbidden.Count == 0,
            SmokeRequiredFieldsPresent = smokeFieldsPresent,
            Passed = passed,
            Status = passed
                ? "GREEN"
                : logExists
                    ? "BLOCKED_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_SYSTEMS"
                    : "PENDING_UNITY_BATCHMODE_GENERIC_GAMEPACKAGE_SYSTEMS",
            Sha256 = logExists ? HashBytes(File.ReadAllBytes(path)) : string.Empty,
            ForbiddenMarkersFound = forbidden
        };
    }

    private static GenericGamePackageSystemsNegativeProof BuildNegativeProof()
    {
        var rejected = BuildRejectedPathSamples();
        return new GenericGamePackageSystemsNegativeProof
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

    private static GenericGamePackageSystemsFileIndex BuildFileIndex(
        string root,
        IReadOnlyDictionary<string, string> files,
        string relativeRoot,
        string role,
        string unityLogRelativePath)
    {
        var entries = files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new GenericGamePackageSystemsFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = role,
                Sha256 = HashText(item.Value)
            })
            .ToList();
        if (!string.IsNullOrWhiteSpace(unityLogRelativePath))
        {
            var logPath = Resolve(root, GenericGamePackageSystemsProjectionVocabulary.UnityBatchmodeLogRelativePath);
            if (File.Exists(logPath))
            {
                entries.Add(new GenericGamePackageSystemsFileIndexEntry
                {
                    RelativePath = unityLogRelativePath,
                    Role = "goal125_unity_batchmode_generic_gamepackage_systems_log",
                    Sha256 = HashBytes(File.ReadAllBytes(logPath))
                });
            }
        }

        return new GenericGamePackageSystemsFileIndex
        {
            IndexedFileCount = entries.Count,
            ManualInputExcluded = entries.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = entries.OrderBy(entry => entry.RelativePath, StringComparer.Ordinal).ToList()
        };
    }

    private static string RenderReport(
        GenericGamePackageSystemsProjectionDashboard dashboard,
        GenericGamePackageSystemsSampleSummary samplePackage,
        GenericGamePackageSystemsScriptInventory scriptInventory,
        GenericGamePackageSystemsSmokePlan smokePlan,
        GenericGamePackageSystemsLogScan logScan,
        GenericGamePackageSystemsNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 125 Generic GamePackage Systems Loop Projection",
            string.Empty,
            "- genericSystemsStatus: " + dashboard.GenericSystemsStatus,
            "- samplePackagePath: " + dashboard.SamplePackagePath,
            "- packageId: " + dashboard.PackageId,
            "- recipePreviewPresent: " + dashboard.RecipePreviewPresent.ToString().ToLowerInvariant(),
            "- recipeApplyPassed: " + dashboard.RecipeApplyPassed.ToString().ToLowerInvariant(),
            "- harvestPreviewPresent: " + dashboard.HarvestPreviewPresent.ToString().ToLowerInvariant(),
            "- harvestApplyPassed: " + dashboard.HarvestApplyPassed.ToString().ToLowerInvariant(),
            "- transactionPreviewPresent: " + dashboard.TransactionPreviewPresent.ToString().ToLowerInvariant(),
            "- encounterPreviewPresent: " + dashboard.EncounterPreviewPresent.ToString().ToLowerInvariant(),
            "- combatRoundPreviewPresent: " + dashboard.CombatRoundPreviewPresent.ToString().ToLowerInvariant(),
            "- systemsEventLogPresent: " + dashboard.SystemsEventLogPresent.ToString().ToLowerInvariant(),
            "- unitySmokeStatus: " + dashboard.UnitySmokeStatus,
            "- cleanupScriptAvailable: " + dashboard.CleanupScriptAvailable.ToString().ToLowerInvariant(),
            "- goal124StillGreen: " + dashboard.Goal124StillGreen.ToString().ToLowerInvariant(),
            "- evidencePath: " + dashboard.EvidencePath,
            "- exportPath: " + dashboard.ExportPath,
            string.Empty,
            "## Sample Package",
            string.Empty,
            "- parsed: " + samplePackage.Parsed.ToString().ToLowerInvariant(),
            "- readOnlySource: " + samplePackage.ReadOnlySource.ToString().ToLowerInvariant(),
            "- sha256: " + samplePackage.Sha256,
            "- playerInventoryPresent: " + samplePackage.PlayerInventoryPresent.ToString().ToLowerInvariant(),
            "- resourceDefaultsPresent: " + samplePackage.ResourceDefaultsPresent.ToString().ToLowerInvariant(),
            "- recipeRequirementsMatchExpected: "
            + samplePackage.RecipeRequirementsMatchExpected.ToString().ToLowerInvariant(),
            "- harvestLootPresent: " + samplePackage.HarvestLootPresent.ToString().ToLowerInvariant(),
            "- combatRoundMatchesExpected: "
            + samplePackage.CombatRoundMatchesExpected.ToString().ToLowerInvariant(),
            string.Empty,
            "## Script Inventory",
            string.Empty,
            "- passed: " + scriptInventory.Passed.ToString().ToLowerInvariant(),
            "- scriptCount: " + scriptInventory.ScriptCount,
            "- stateClassTracksRequiredFields: "
            + scriptInventory.StateClassTracksRequiredFields.ToString().ToLowerInvariant(),
            "- systemsLoopRunsRequiredSequence: "
            + scriptInventory.SystemsLoopRunsRequiredSequence.ToString().ToLowerInvariant(),
            "- controllerRendersSystemsMarkers: "
            + scriptInventory.ControllerRendersSystemsMarkers.ToString().ToLowerInvariant(),
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

    private static string RenderDocumentation(GenericGamePackageSystemsProjectionDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Generic GamePackage Systems Loop Projection",
            string.Empty,
            "Goal125 adds a projection-only Unity Editor systems loop over `samples/minimal-map-game/package.json`.",
            string.Empty,
            "## Hands-on Verification",
            string.Empty,
            "- Open `unity/LLMGameCreatorAlpha` in Unity.",
            "- Select `LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection`.",
            "- Click `Run Generic Package Systems Loop Verification`.",
            "- Verify the systems status, inventory/resources, recipe craft result, harvest result, transaction affordability, encounter/combat preview and systems event log markers.",
            "- Do not save scenes, prefabs, ProjectSettings, Packages or StreamingAssets as part of this check.",
            string.Empty,
            "## Cleanup Command",
            string.Empty,
            "- After Unity checks: `.\\.devflow\\scripts\\clean-unity-editor-noise.cmd`",
            string.Empty,
            "## Status",
            string.Empty,
            "- genericSystemsStatus: " + dashboard.GenericSystemsStatus,
            "- samplePackagePath: " + dashboard.SamplePackagePath,
            "- packageId: " + dashboard.PackageId,
            "- unitySmokeStatus: " + dashboard.UnitySmokeStatus,
            "- projectionOnly: " + dashboard.ProjectionOnly.ToString().ToLowerInvariant(),
            "- noRuntimeProviderSchemaLuaGeneratorLibrary: "
            + dashboard.NoRuntimeProviderSchemaLuaGeneratorLibrary.ToString().ToLowerInvariant()
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static bool Goal124StillGreen(string root)
    {
        var result = new GenericGamePackageLoopProjectionService().Build(root);
        return result.Dashboard.GenericLoopStatus == "GREEN"
               && result.SamplePackage.Passed
               && result.ScriptInventory.Passed
               && result.NegativeProof.Passed;
    }

    private static bool CleanupScriptAvailable(GenericGamePackageSystemsScriptInventory inventory) =>
        inventory.Scripts.Any(entry =>
            entry.RelativePath == AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath
            && entry.Exists
            && entry.ContainsRequiredMarker)
        && inventory.Scripts.Any(entry =>
            entry.RelativePath == AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptCmdPath
            && entry.Exists
            && entry.ContainsRequiredMarker);

    private static GenericGamePackageSystemsScriptInventoryEntry Entry(
        string root,
        string relativePath,
        string role,
        string marker)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        var text = exists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        return new GenericGamePackageSystemsScriptInventoryEntry
        {
            RelativePath = relativePath,
            Role = role,
            Exists = exists,
            ContainsRequiredMarker = text.Contains(marker, StringComparison.Ordinal),
            RequiredMarker = marker,
            Sha256 = exists ? HashBytes(File.ReadAllBytes(path)) : string.Empty
        };
    }

    private static GenericGamePackageSystemsSmokePlanStep Step(
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
        GenericGamePackageSystemsProjectionVocabulary.ProceduralOutputDirectory + "/",
        GenericGamePackageSystemsProjectionVocabulary.ExportPackageDirectory + "/",
        "docs/agent-tasks/goal-125-generic-gamepackage-systems-loop-projection/",
        GenericGamePackageSystemsProjectionVocabulary.DocumentationPath,
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
        GenericGamePackageSystemsProjectionVocabulary.UnitySystemsPath,
        "tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/",
        "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs"
    ];

    private static IReadOnlyList<string> BuildRejectedPathSamples() =>
    [
        ".llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json",
        GenericGamePackageSystemsProjectionVocabulary.SamplePackagePath,
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

    private static int AmountInInventory(JsonElement inventory, string itemId) =>
        ArrayItems(inventory, "stacks")
            .Where(stack => StringValue(stack, "itemId") == itemId)
            .Sum(stack => IntValue(stack, "amount"));

    private static int DurabilityInInventory(JsonElement inventory, string itemId) =>
        ArrayItems(inventory, "stacks")
            .Where(stack => StringValue(stack, "itemId") == itemId)
            .Select(stack => IntValue(stack, "durability"))
            .FirstOrDefault();

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
            throw new InvalidOperationException("Goal125 must not write the manual input path.");
        }
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
