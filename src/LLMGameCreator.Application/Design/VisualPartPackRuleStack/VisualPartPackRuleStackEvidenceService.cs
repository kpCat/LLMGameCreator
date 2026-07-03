using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.VisualPartPackRuleStack;

public sealed class VisualPartPackRuleStackEvidenceService
{
    public const string ReportMarkdownFileName = "visual-part-pack-rule-stack-report.md";
    public const string CatalogJsonFileName = "visual-part-pack-catalog.json";
    public const string ValidationMatrixJsonFileName = "visual-part-pack-validation-matrix.json";
    public const string NegativeProofJsonFileName = "visual-part-pack-negative-proof.json";
    public const string DeepsearchLineageJsonFileName = "deepsearch-lineage-inventory.json";
    public const string Goal084BindingJsonFileName = "goal084-contract-binding-matrix.json";
    public const string WaterCoverageJsonFileName = "water-biome-coverage-matrix.json";
    public const string QualityGateScanJsonFileName = "quality-gate-scan.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static VisualPartPackRuleStackEvidenceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public VisualRuleStackEvidenceResult Build(string? projectRootPath = null)
    {
        var manifest = VisualPartPackRuleStackFixtures.BuildDefaultManifest();
        var catalog = new VisualPartPackCatalog
        {
            Accepted = false,
            FixturePackCount = manifest.PartPacks.Count,
            FixturePackIds = manifest.PartPacks.Select(item => item.PackId).Order(StringComparer.Ordinal).ToList(),
            Manifest = manifest
        };
        var validationMatrix = BuildValidationMatrix(manifest);
        var negativeProof = BuildNegativeProof(manifest);
        var deepsearchLineage = BuildDeepsearchLineageInventory(projectRootPath);
        var goal084Binding = BuildGoal084BindingMatrix(projectRootPath);
        var waterCoverage = BuildWaterBiomeCoverageMatrix(manifest);
        var qualityGate = VisualPartPackRuleStackQualityGateScanner.Build(
            manifest,
            validationMatrix,
            negativeProof,
            deepsearchLineage,
            goal084Binding,
            waterCoverage);

        var catalogJson = Serialize(catalog);
        var validationJson = Serialize(validationMatrix);
        var negativeJson = Serialize(negativeProof);
        var deepsearchJson = Serialize(deepsearchLineage);
        var goal084Json = Serialize(goal084Binding);
        var waterJson = Serialize(waterCoverage);
        var qualityJson = Serialize(qualityGate);

        var reportWithoutHash = new VisualPartPackRuleStackReport
        {
            Accepted = false,
            ContractModelsImplemented = true,
            ValidatorImplemented = true,
            FixturesImplemented = true,
            ValidFixturesPassed = validationMatrix.Passed,
            NegativeProofPassed = negativeProof.Passed,
            DeepsearchLineagePassed = deepsearchLineage.Passed,
            Goal084BindingPassed = goal084Binding.Passed,
            WaterBiomeCoveragePassed = waterCoverage.Passed,
            FixturePackCount = catalog.FixturePackCount,
            NegativeScenarioCount = negativeProof.ScenarioCount,
            CatalogHash = VisualPartPackRuleStackHash.Compute(catalogJson),
            ValidationMatrixHash = VisualPartPackRuleStackHash.Compute(validationJson),
            NegativeProofHash = VisualPartPackRuleStackHash.Compute(negativeJson),
            DeepsearchLineageHash = VisualPartPackRuleStackHash.Compute(deepsearchJson),
            Goal084BindingHash = VisualPartPackRuleStackHash.Compute(goal084Json),
            WaterBiomeCoverageHash = VisualPartPackRuleStackHash.Compute(waterJson),
            QualityGateHash = VisualPartPackRuleStackHash.Compute(qualityJson)
        };
        var reportMarkdownWithoutHash = RenderReport(reportWithoutHash, catalog, validationMatrix, negativeProof, deepsearchLineage, goal084Binding, waterCoverage, qualityGate, deterministicReportHash: string.Empty);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = VisualPartPackRuleStackHash.Compute(reportMarkdownWithoutHash)
        };
        var reportMarkdown = RenderReport(report, catalog, validationMatrix, negativeProof, deepsearchLineage, goal084Binding, waterCoverage, qualityGate, report.DeterministicReportHash);

        return new VisualRuleStackEvidenceResult
        {
            Catalog = catalog,
            ValidationMatrix = validationMatrix,
            NegativeProof = negativeProof,
            DeepsearchLineageInventory = deepsearchLineage,
            Goal084ContractBindingMatrix = goal084Binding,
            WaterBiomeCoverageMatrix = waterCoverage,
            QualityGateScan = qualityGate,
            Report = report,
            CatalogJson = catalogJson,
            ValidationMatrixJson = validationJson,
            NegativeProofJson = negativeJson,
            DeepsearchLineageJson = deepsearchJson,
            Goal084BindingMatrixJson = goal084Json,
            WaterBiomeCoverageMatrixJson = waterJson,
            QualityGateScanJson = qualityJson,
            ReportMarkdown = reportMarkdown
        };
    }

    public async Task<VisualPartPackRuleStackWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VisualPartPackRuleStackWriteResult> WriteAsync(
        string projectRootPath,
        VisualRuleStackEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, VisualPartPackRuleStackVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var write = new VisualPartPackRuleStackWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            CatalogJsonPath = Path.Combine(outputDirectory, CatalogJsonFileName),
            ValidationMatrixJsonPath = Path.Combine(outputDirectory, ValidationMatrixJsonFileName),
            NegativeProofJsonPath = Path.Combine(outputDirectory, NegativeProofJsonFileName),
            DeepsearchLineageJsonPath = Path.Combine(outputDirectory, DeepsearchLineageJsonFileName),
            Goal084BindingMatrixJsonPath = Path.Combine(outputDirectory, Goal084BindingJsonFileName),
            WaterBiomeCoverageMatrixJsonPath = Path.Combine(outputDirectory, WaterCoverageJsonFileName),
            QualityGateScanJsonPath = Path.Combine(outputDirectory, QualityGateScanJsonFileName)
        };

        await File.WriteAllTextAsync(write.ReportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.CatalogJsonPath, result.CatalogJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ValidationMatrixJsonPath, result.ValidationMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.NegativeProofJsonPath, result.NegativeProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.DeepsearchLineageJsonPath, result.DeepsearchLineageJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.Goal084BindingMatrixJsonPath, result.Goal084BindingMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.WaterBiomeCoverageMatrixJsonPath, result.WaterBiomeCoverageMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.QualityGateScanJsonPath, result.QualityGateScanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        return write;
    }

    public static VisualPartPackValidationMatrix BuildValidationMatrix(VisualPartPackManifest manifest)
    {
        var validation = VisualPartPackRuleStackValidator.Validate(manifest);
        var rows = manifest.PartPacks
            .OrderBy(item => item.PackId, StringComparer.Ordinal)
            .Select(pack => new VisualPartPackValidationRow
            {
                PackId = pack.PackId,
                Kind = pack.Kind,
                Passed = validation.Passed,
                PartCount = pack.Parts.Count,
                RecipeCount = manifest.Recipes.Count(item => item.PackId == pack.PackId),
                HasSafeFallback = !string.IsNullOrWhiteSpace(pack.SafeFallbackPackId),
                Diagnostics = validation.Diagnostics
                    .Where(item => string.Equals(item.Target, pack.PackId, StringComparison.Ordinal))
                    .ToList()
            })
            .ToList();

        return new VisualPartPackValidationMatrix
        {
            Passed = validation.Passed
                && rows.Count == VisualPartPackRuleStackFixtures.RequiredFixturePackIds.Count
                && VisualPartPackRuleStackFixtures.RequiredFixturePackIds.All(id => rows.Any(row => row.PackId == id && row.Passed)),
            FixturePackCount = rows.Count,
            Rows = rows
        };
    }

    public static VisualPartPackNegativeProof BuildNegativeProof(VisualPartPackManifest baseline)
    {
        var tile = Pack(baseline, "fantasy_overworld_tile_part_pack");
        var water = Pack(baseline, "water_coast_river_marsh_part_pack");
        var creature = Pack(baseline, "creature_bodyplan_equipment_part_pack");
        var ui = Pack(baseline, "ui_theme_icon_effect_part_pack");
        var adult = Pack(baseline, "adult_rating_gated_extension_metadata_only");
        var firstRecipe = baseline.Recipes.First(item => item.PackId == tile.PackId);
        var waterRecipe = baseline.Recipes.First(item => item.PackId == water.PackId);

        var cycleRecipes = baseline.Recipes
            .Select(recipe =>
                recipe.RecipeId == firstRecipe.RecipeId
                    ? recipe with { DependsOnRecipeIds = [waterRecipe.RecipeId] }
                    : recipe.RecipeId == waterRecipe.RecipeId
                        ? recipe with { DependsOnRecipeIds = [firstRecipe.RecipeId] }
                        : recipe)
            .ToList();

        var scenarios = new List<VisualPartPackNegativeScenario>
        {
            Invalid("duplicate_ids", "duplicate pack ids", baseline with { PartPacks = [.. baseline.PartPacks, tile] }),
            Invalid("absolute_path_rejected", "absolute metadata path", ReplacePack(baseline, tile with { MetadataRelativePath = "C:/unsafe/asset.png" })),
            Invalid("missing_layered_masks_sockets_anchors", "layered part missing required refs", ReplacePack(baseline, ReplacePart(tile, tile.Parts[0] with { MaskIds = [], SocketIds = [], AnchorIds = [] }))),
            Invalid("unknown_palette_ref", "unknown recipe palette ref", ReplaceRecipe(baseline, firstRecipe with { PaletteProfileId = "palette/unknown_visual_ref" })),
            Invalid("missing_adult_safe_fallback", "adult extension missing fallback pack", ReplacePack(baseline, adult with { SafeFallbackPackId = "" })),
            Invalid("adult_without_eligible_body_plan", "adult extension lacks eligible body-plan metadata", ReplacePack(baseline, adult with { CreatureBodyPlanProfiles = [adult.CreatureBodyPlanProfiles[0] with { AdultEligible = false, Sapient = false, HumanoidCompatible = false, NonSapient = true }] })),
            Invalid("water_without_coast_river_lake", "water pack missing coast river lake marsh coverage", ReplacePack(baseline, water with { WaterProfiles = [water.WaterProfiles[0] with { WaterKinds = ["sea"], CoastAware = false, RiverAware = false, LakeAware = false, MarshAware = false }] })),
            Invalid("tile_without_transition_autotile", "tile pack missing transition and autotile rules", ReplacePack(baseline, tile with { TerrainTransitionRules = [], AutoTileRules = [] })),
            Invalid("creature_without_body_plan_rules", "creature pack missing body-plan rules", ReplacePack(baseline, creature with { CreatureBodyPlanProfiles = [] })),
            Invalid("equipment_overlay_without_socket", "equipment overlay missing socket compatibility", ReplacePack(baseline, creature with { EquipmentOverlayProfiles = [creature.EquipmentOverlayProfiles[0] with { CompatibleSocketIds = [] }] })),
            Invalid("ui_effect_without_safe_fallback", "UI/effect pack missing fallback", ReplacePack(baseline, ui with { SafeFallbackPackId = "" })),
            Invalid("prompt_text_as_source_of_truth", "prompt source-of-truth claim", baseline with { PromptTextIsSourceOfTruth = true, SourceOfTruthKind = "provider_prompt_text" }),
            Invalid("provider_candidate_treated_as_approved", "provider candidate marked approved", ReplacePack(baseline, adult with { ProviderState = VisualPartProviderState.CandidateQuarantine, ReviewStatus = VisualPartReviewStatus.ApprovedMetadata })),
            Invalid("cyclic_recipe_dependencies", "cyclic recipe dependencies", baseline with { Recipes = cycleRecipes }),
            Invalid("unsafe_export_policy_contradiction", "adult metadata exported public-safe", ReplacePack(baseline, adult with { Rating = VisualContentRating.AdultMetadataOnly, ExportPolicy = VisualPartExportPolicy.PublicSafe })),
            Invalid("unknown_recipe_ref", "unknown dependency recipe ref", ReplaceRecipe(baseline, firstRecipe with { DependsOnRecipeIds = ["recipe/unknown_visual_ref/v1"] }))
        };

        return new VisualPartPackNegativeProof
        {
            Passed = scenarios.Count >= 16 && scenarios.All(item => item.ExpectedValid == item.ActualValid && !item.ActualValid && item.Diagnostics.Any(diagnostic => diagnostic.Severity == "error")),
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedValid == item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    private static DeepsearchLineageInventory BuildDeepsearchLineageInventory(string? projectRootPath)
    {
        var records = DeepsearchInputs()
            .Select(item => BuildDeepsearchRecord(projectRootPath, item.Path, item.Tags))
            .ToList();
        var contextIndex = ReadText(projectRootPath, "docs/CONTEXT_INDEX.md");
        var queue = ReadText(projectRootPath, "docs/FULL_GENERATOR_GOAL_QUEUE.md");
        var indexed = records.All(item => contextIndex.Contains(item.Path, StringComparison.Ordinal));
        var routed = records.All(item => queue.Contains(item.Path, StringComparison.Ordinal));

        return new DeepsearchLineageInventory
        {
            Passed = records.Count == 8 && records.All(item => item.Exists) && indexed && routed,
            IndexedInContextIndex = indexed,
            RoutedInFullGeneratorGoalQueue = routed,
            DocumentCount = records.Count,
            Records = records
        };
    }

    private static Goal084ContractBindingMatrix BuildGoal084BindingMatrix(string? projectRootPath)
    {
        const string catalogPath = ".llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-asset-contract-catalog.json";
        const string reportPath = ".llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-asset-contract-rating-metadata-report.md";
        var catalogText = ReadText(projectRootPath, catalogPath);
        var reportText = ReadText(projectRootPath, reportPath);
        var exists = !string.IsNullOrWhiteSpace(catalogText);
        var acceptedFalse = catalogText.Contains("\"accepted\": false", StringComparison.Ordinal)
            && reportText.Contains("accepted: false", StringComparison.Ordinal);
        var rows = VisualPartPackRuleStackFixtures.Goal084SlotBindings
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new Goal084ContractBindingRow
            {
                PackId = item.Key,
                Goal084SlotId = item.Value,
                SlotExistsInGoal084Catalog = catalogText.Contains($"\"assetSlot\": \"{item.Value}\"", StringComparison.Ordinal)
                    || catalogText.Contains($"\"{item.Value}\"", StringComparison.Ordinal)
            })
            .ToList();

        return new Goal084ContractBindingMatrix
        {
            Passed = exists && acceptedFalse && rows.All(item => item.SlotExistsInGoal084Catalog),
            Goal084ArtifactExists = exists,
            Goal084AcceptedFalse = acceptedFalse,
            Goal084CatalogHash = exists ? VisualPartPackRuleStackHash.Compute(catalogText) : string.Empty,
            Rows = rows
        };
    }

    private static WaterBiomeCoverageMatrix BuildWaterBiomeCoverageMatrix(VisualPartPackManifest manifest)
    {
        var pack = Pack(manifest, "water_coast_river_marsh_part_pack");
        var waterKinds = pack.WaterProfiles.SelectMany(item => item.WaterKinds).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var placements = pack.ObjectPlacementRules.Select(item => item.ObjectKind).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var coast = waterKinds.Contains("coast") || pack.WaterProfiles.Any(item => item.CoastAware);
        var river = waterKinds.Contains("river") || pack.WaterProfiles.Any(item => item.RiverAware);
        var lake = waterKinds.Contains("lake") || pack.WaterProfiles.Any(item => item.LakeAware);
        var marsh = waterKinds.Contains("marsh") || pack.WaterProfiles.Any(item => item.MarshAware);
        var sea = waterKinds.Contains("sea");
        var bridge = placements.Contains("bridge");
        var dock = placements.Contains("dock");
        var waterObject = placements.Contains("water_object");

        return new WaterBiomeCoverageMatrix
        {
            Passed = sea && lake && river && coast && marsh && bridge && dock && waterObject,
            PackId = pack.PackId,
            SeaCovered = sea,
            LakeCovered = lake,
            RiverCovered = river,
            CoastCovered = coast,
            MarshCovered = marsh,
            BridgeCovered = bridge,
            DockCovered = dock,
            WaterObjectCovered = waterObject
        };
    }

    private static VisualPartPackNegativeScenario Invalid(string id, string mutation, VisualPartPackManifest manifest)
    {
        var validation = VisualPartPackRuleStackValidator.Validate(manifest);
        return new VisualPartPackNegativeScenario
        {
            ScenarioId = id,
            CausalMutation = mutation,
            ExpectedValid = false,
            ActualValid = validation.Passed,
            Diagnostics = validation.Diagnostics
        };
    }

    private static VisualPartPackDefinition Pack(VisualPartPackManifest manifest, string packId) =>
        manifest.PartPacks.Single(item => item.PackId == packId);

    private static VisualPartPackManifest ReplacePack(VisualPartPackManifest manifest, VisualPartPackDefinition replacement) =>
        manifest with
        {
            PartPacks = manifest.PartPacks.Select(item => item.PackId == replacement.PackId ? replacement : item).ToList()
        };

    private static VisualPartPackDefinition ReplacePart(VisualPartPackDefinition pack, VisualPartDefinition replacement) =>
        pack with
        {
            Parts = pack.Parts.Select(item => item.PartId == replacement.PartId ? replacement : item).ToList()
        };

    private static VisualPartPackManifest ReplaceRecipe(VisualPartPackManifest manifest, VisualPartPackRecipe replacement) =>
        manifest with
        {
            Recipes = manifest.Recipes.Select(item => item.RecipeId == replacement.RecipeId ? replacement : item).ToList()
        };

    private static DeepsearchLineageRecord BuildDeepsearchRecord(
        string? projectRootPath,
        string relativePath,
        IReadOnlyList<string> tags)
    {
        var text = ReadText(projectRootPath, relativePath);
        return new DeepsearchLineageRecord
        {
            Path = relativePath,
            Exists = !string.IsNullOrWhiteSpace(text),
            Sha256 = string.IsNullOrWhiteSpace(text) ? string.Empty : VisualPartPackRuleStackHash.Compute(text),
            PurposeTags = tags
        };
    }

    private static IReadOnlyList<(string Path, IReadOnlyList<string> Tags)> DeepsearchInputs() =>
    [
        ("docs/deepsearch/01_PROCEDURAL_VISUAL_SYNTHESIS_CORE_AND_PART_PACKS.md", ["part_pack_core", "adapter_boundary", "metadata_contract"]),
        ("docs/deepsearch/02_TILE_BIOME_WATER_WORLD_MAP_GENERATION.md", ["tile_biome", "water", "world_map"]),
        ("docs/deepsearch/03_PSEUDO3D_FIRST_PERSON_FROM_2D_ASSETS.md", ["pseudo3d", "first_person", "presentation_sidecar"]),
        ("docs/deepsearch/04_CREATURE_NPC_APPEARANCE_BODYPLAN_PAPERDOLL.md", ["creature", "body_plan", "paperdoll"]),
        ("docs/deepsearch/05_SETTLEMENTS_CITIES_CARAVANS_LIVING_WORLD_VISUALS.md", ["settlement", "city", "living_world_visuals"]),
        ("docs/deepsearch/06_UI_THEMES_EFFECTS_WEATHER_DAYNIGHT_VFX.md", ["ui_theme", "weather", "day_night", "vfx"]),
        ("docs/deepsearch/07_MEDIA_PIPELINE_PROVIDER_QUARANTINE_PROVENANCE_RATING_ADULT.md", ["provider_quarantine", "provenance", "rating", "adult_metadata"]),
        ("docs/deepsearch/08_EXISTING_LIBRARIES_AND_TOOLS_SCOUTING.md", ["libraries", "optional_adapters", "licensing"])
    ];

    private static string RenderReport(
        VisualPartPackRuleStackReport report,
        VisualPartPackCatalog catalog,
        VisualPartPackValidationMatrix validationMatrix,
        VisualPartPackNegativeProof negativeProof,
        DeepsearchLineageInventory deepsearchLineage,
        Goal084ContractBindingMatrix goal084Binding,
        WaterBiomeCoverageMatrix waterCoverage,
        VisualPartPackQualityGateScan qualityGate,
        string deterministicReportHash)
    {
        var lines = new List<string>
        {
            "# Goal 085 Visual Part-Pack Rule Stack Report",
            string.Empty,
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- manualGate: {report.ManualGate} required",
            $"- deterministicReportHash: {deterministicReportHash}",
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 085 adds a BCL-only Application-side visual part-pack contract and rule-stack validator. Evidence is metadata-only and does not generate images, call providers, mutate Runtime, mutate Unity or change the public GamePackage schema.",
            string.Empty,
            "## Contract Types",
            string.Empty,
            "- VisualPartPackManifest",
            "- VisualPartDefinition",
            "- VisualPartLayer",
            "- VisualMaskDefinition",
            "- VisualSocketDefinition",
            "- VisualAnchorDefinition",
            "- VisualPaletteProfile",
            "- VisualPaletteSwapRule",
            "- VisualOverlayRule",
            "- VisualBiomeProfile",
            "- VisualWaterProfile",
            "- VisualTerrainTransitionRule",
            "- VisualAutoTileRule",
            "- VisualObjectPlacementRule",
            "- VisualCreatureBodyPlanProfile",
            "- VisualEquipmentOverlayProfile",
            "- VisualUiThemeProfile",
            "- VisualEffectProfile",
            "- VisualPartPackRecipe",
            "- VisualRuleStackValidationResult",
            "- VisualRuleStackEvidenceResult",
            string.Empty,
            "## Fixture Packs",
            string.Empty
        };
        lines.AddRange(catalog.FixturePackIds.Select(id => $"- {id}"));
        lines.AddRange(
        [
            string.Empty,
            "## Validation",
            string.Empty,
            $"- validFixturesPassed: {validationMatrix.Passed.ToString().ToLowerInvariant()}",
            $"- negativeProofPassed: {negativeProof.Passed.ToString().ToLowerInvariant()}",
            $"- negativeScenarioCount: {negativeProof.ScenarioCount}",
            $"- rejectedNegativeScenarioCount: {negativeProof.RejectedCount}",
            string.Empty,
            "## Deepsearch Lineage",
            string.Empty,
            $"- documentCount: {deepsearchLineage.DocumentCount}",
            $"- allDocumentsConsumed: {deepsearchLineage.Passed.ToString().ToLowerInvariant()}",
            $"- indexedInContextIndex: {deepsearchLineage.IndexedInContextIndex.ToString().ToLowerInvariant()}",
            $"- routedInFullGeneratorGoalQueue: {deepsearchLineage.RoutedInFullGeneratorGoalQueue.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Goal084 Binding",
            string.Empty,
            $"- goal084BindingPassed: {goal084Binding.Passed.ToString().ToLowerInvariant()}",
            $"- goal084AcceptedFalse: {goal084Binding.Goal084AcceptedFalse.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Water And Biome Coverage",
            string.Empty,
            $"- sea: {waterCoverage.SeaCovered.ToString().ToLowerInvariant()}",
            $"- lake: {waterCoverage.LakeCovered.ToString().ToLowerInvariant()}",
            $"- river: {waterCoverage.RiverCovered.ToString().ToLowerInvariant()}",
            $"- coast: {waterCoverage.CoastCovered.ToString().ToLowerInvariant()}",
            $"- marsh: {waterCoverage.MarshCovered.ToString().ToLowerInvariant()}",
            $"- bridge: {waterCoverage.BridgeCovered.ToString().ToLowerInvariant()}",
            $"- dock: {waterCoverage.DockCovered.ToString().ToLowerInvariant()}",
            $"- waterObject: {waterCoverage.WaterObjectCovered.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Boundaries",
            string.Empty,
            $"- adultMetadataOnlyFallbackBound: {qualityGate.AdultMetadataOnlyFallbackBound.ToString().ToLowerInvariant()}",
            $"- noForbiddenFilesChanged: {qualityGate.NoForbiddenFilesChanged.ToString().ToLowerInvariant()}",
            $"- noExternalDependenciesAdded: {qualityGate.NoExternalDependenciesAdded.ToString().ToLowerInvariant()}",
            $"- noImagesMediaBinaryAssetsAdded: {qualityGate.NoImagesMediaBinaryAssetsAdded.ToString().ToLowerInvariant()}",
            $"- noProviderIntegrationAdded: {qualityGate.NoProviderIntegrationAdded.ToString().ToLowerInvariant()}",
            $"- noRuntimeOrUnityChanged: {qualityGate.NoRuntimeOrUnityChanged.ToString().ToLowerInvariant()}",
            $"- noPublicGamePackageSchemaChanged: {qualityGate.NoPublicGamePackageSchemaChanged.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            $"- catalogHash: {report.CatalogHash}",
            $"- validationMatrixHash: {report.ValidationMatrixHash}",
            $"- negativeProofHash: {report.NegativeProofHash}",
            $"- deepsearchLineageHash: {report.DeepsearchLineageHash}",
            $"- goal084BindingHash: {report.Goal084BindingHash}",
            $"- waterBiomeCoverageHash: {report.WaterBiomeCoverageHash}",
            $"- qualityGateHash: {report.QualityGateHash}"
        ]);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string ReadText(string? projectRootPath, string relativePath)
    {
        var fullPath = ResolveOptionalPath(projectRootPath, relativePath);
        return fullPath != null && File.Exists(fullPath)
            ? File.ReadAllText(fullPath, Encoding.UTF8)
            : string.Empty;
    }

    private static string? ResolveOptionalPath(string? projectRootPath, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            return null;
        }

        return Path.GetFullPath(Path.Combine(Path.GetFullPath(projectRootPath), relativePath.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }
}
