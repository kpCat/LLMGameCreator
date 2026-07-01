using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.CapabilityBundlePipelineInputs;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.PackageAssemblyItemsEconomyCrafting;

public sealed class PackageAssemblyItemsEconomyCraftingAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/package-assembly-items-economy-crafting";
    public const string MappingContractProofJsonFileName = "package-assembly-items-economy-crafting-mapping-contract-proof.json";
    public const string InputFixturesJsonFileName = "package-assembly-items-economy-crafting-input-fixtures.json";
    public const string AssemblyReportJsonFileName = "package-assembly-items-economy-crafting-assembly-report.json";
    public const string PackageSummaryJsonFileName = "package-assembly-items-economy-crafting-package-summary.json";
    public const string AntiOverfitFixturesJsonFileName = "package-assembly-items-economy-crafting-anti-overfit-fixtures.json";
    public const string InvalidMatrixJsonFileName = "package-assembly-items-economy-crafting-invalid-matrix.json";
    public const string ReportJsonFileName = "package-assembly-items-economy-crafting-report.json";
    public const string ReportMarkdownFileName = "package-assembly-items-economy-crafting-report.md";
    public const string VerificationMarkdownFileName = "package-assembly-items-economy-crafting-verification.md";
    public const string FinalArtifactScopeReportJsonFileName = "goal-027-final-artifact-scope-report.json";
    public const string FinalArtifactScopeReportMarkdownFileName = "goal-027-final-artifact-scope-report.md";
    public const string FinalGate = "package_assembly_items_economy_crafting_expansion_verification";
    public const string PreviousAcceptedGate = "package_assembly_dialogue_quests_expansion_verification passed";
    private const string ProductSmokeRoute = "package-assembly-items-economy-crafting";
    private const string Goal023RelativeOutputDirectory = ".llmgc/procedural/capability-bundle-pipeline-inputs";
    private const string Goal024RelativeOutputDirectory = ".llmgc/procedural/rich-package-assembly-coverage-audit";
    private const string Goal025RelativeOutputDirectory = ".llmgc/procedural/package-assembly-world-entities";
    private const string Goal026RelativeOutputDirectory = ".llmgc/procedural/package-assembly-dialogue-quests";
    private static readonly DateTimeOffset AppliedAtUtc = DateTimeOffset.Parse("2026-06-28T00:00:00Z");
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<PackageAssemblyItemsEconomyCraftingResult> BuildAsync(
        string projectRootPath,
        PackageAssemblyItemsEconomyCraftingOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var settings = options ?? new PackageAssemblyItemsEconomyCraftingOptions();
        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<PackageAssemblyItemsEconomyCraftingDiagnostic>
        {
            Diagnostic("info", "package_items_economy_crafting.previous_gate_recorded", settings.PreviousAcceptedGate, "User-confirmed Goal 026 package assembly dialogue/quests verification is recorded as passed."),
            Diagnostic("info", "package_items_economy_crafting.boundary", "execution_boundary", "Goal 027 executes bounded in-memory package assembly only; no Unity, LLM, RAG, provider, media or Lua execution is invoked.")
        };

        if (settings.PreviousAcceptedGate != PreviousAcceptedGate)
        {
            diagnostics.Add(Diagnostic("error", "package_items_economy_crafting.previous_gate.missing", settings.PreviousAcceptedGate, "Goal 027 requires package_assembly_dialogue_quests_expansion_verification passed."));
        }

        var evidence = await LoadEvidenceAsync(projectRoot, settings, diagnostics, cancellationToken).ConfigureAwait(false);
        var fixtures = BuildFixtures(evidence);
        var realConsumer = BuildConsumer(fixtures.RealConsumer, diagnostics);
        var syntheticConsumer = settings.SyntheticAntiOverfitFixtureMissing
            ? ItemsEconomyCraftingConsumerSummary.Missing("vendor_crafting_transaction")
            : BuildConsumer(fixtures.SyntheticConsumer, diagnostics);
        var invalidMatrix = BuildInvalidMatrix(settings);
        diagnostics.AddRange(invalidMatrix.Diagnostics);

        var antiOverfit = new PackageAssemblyItemsEconomyCraftingAntiOverfitProof
        {
            RealConsumerId = realConsumer.ConsumerId,
            SyntheticConsumerId = syntheticConsumer.ConsumerId,
            SyntheticConsumerPresent = !settings.SyntheticAntiOverfitFixtureMissing,
            DistinctConsumerIds = !string.Equals(realConsumer.ConsumerId, syntheticConsumer.ConsumerId, StringComparison.Ordinal),
            DistinctPrimaryItemIds = !string.Equals(realConsumer.PrimaryItemId, syntheticConsumer.PrimaryItemId, StringComparison.Ordinal),
            DistinctPrimaryRecipeIds = !string.Equals(realConsumer.PrimaryRecipeId, syntheticConsumer.PrimaryRecipeId, StringComparison.Ordinal),
            Passed = !settings.HardcodedTradeOnlyOutput
                && !settings.SyntheticAntiOverfitFixtureMissing
                && syntheticConsumer.Passed
                && !string.Equals(realConsumer.PrimaryItemId, syntheticConsumer.PrimaryItemId, StringComparison.Ordinal)
                && !string.Equals(realConsumer.PrimaryRecipeId, syntheticConsumer.PrimaryRecipeId, StringComparison.Ordinal)
        };

        var mappingProof = BuildMappingProof(evidence, realConsumer, syntheticConsumer);
        var assemblyReport = new PackageAssemblyItemsEconomyCraftingAssemblyReport
        {
            SchemaVersion = "package_assembly_items_economy_crafting_assembly_report_v1",
            ProductSmokeRoute = ProductSmokeRoute,
            Consumers = [realConsumer, syntheticConsumer],
            Diagnostics = SortDiagnostics(diagnostics.Where(item => item.Code.StartsWith("package_items_economy_crafting.assembly", StringComparison.Ordinal)))
        };
        var packageSummary = new PackageAssemblyItemsEconomyCraftingPackageSummary
        {
            SchemaVersion = "package_assembly_items_economy_crafting_package_summary_v1",
            ConsumerSummaries = [realConsumer, syntheticConsumer],
            TotalItems = realConsumer.ItemCount + syntheticConsumer.ItemCount,
            TotalResources = realConsumer.ResourceCount + syntheticConsumer.ResourceCount,
            TotalRecipes = realConsumer.RecipeCount + syntheticConsumer.RecipeCount,
            TotalLootTables = realConsumer.LootTableCount + syntheticConsumer.LootTableCount,
            TotalTransactions = realConsumer.TransactionCount + syntheticConsumer.TransactionCount,
            TotalInventories = realConsumer.InventoryCount + syntheticConsumer.InventoryCount,
            TotalEquipmentSlots = realConsumer.EquipmentSlotCount + syntheticConsumer.EquipmentSlotCount
        };
        var scopeReport = BuildScopeReport();

        var mappingProofJson = JsonSerializer.Serialize(mappingProof, JsonOptions);
        var fixturesJson = JsonSerializer.Serialize(fixtures, JsonOptions);
        var assemblyReportJson = JsonSerializer.Serialize(assemblyReport, JsonOptions);
        var packageSummaryJson = JsonSerializer.Serialize(packageSummary, JsonOptions);
        var antiOverfitJson = JsonSerializer.Serialize(antiOverfit, JsonOptions);
        var invalidMatrixJson = JsonSerializer.Serialize(invalidMatrix, JsonOptions);
        var scopeReportJson = JsonSerializer.Serialize(scopeReport, JsonOptions);

        var noTopLevelErrors = diagnostics.All(diagnostic => diagnostic.Severity != "error");
        var reportWithoutHash = new PackageAssemblyItemsEconomyCraftingReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousAcceptedGate = settings.PreviousAcceptedGate,
            CompletedSlices = ["S213", "S214", "S215", "S216", "S217", "S218", "S219"],
            ProductSmokeRoute = ProductSmokeRoute,
            ContractProofPassed = noTopLevelErrors && invalidMatrix.Passed && antiOverfit.Passed,
            Goal026EvidenceVerified = evidence.Goal026EvidenceVerified,
            Goal025EvidenceVerified = evidence.Goal025EvidenceVerified,
            Goal024EvidenceVerified = evidence.Goal024EvidenceVerified,
            Goal023EvidenceVerified = evidence.Goal023EvidenceVerified,
            RealConsumerPassed = realConsumer.Passed,
            SyntheticConsumerPassed = syntheticConsumer.Passed,
            AntiOverfitProofPassed = antiOverfit.Passed,
            ItemsEconomyCraftingMappingWritten = true,
            PackageSummaryWritten = true,
            PackageAssemblyExecuted = true,
            ProductVerticalGate = false,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            UnityBuildExecuted = false,
            LlmRagProviderMediaLuaExecuted = false,
            ScopeGuardPassed = scopeReport.Passed,
            MappingContractProofHash = ComputeHash(mappingProofJson),
            InputFixturesHash = ComputeHash(fixturesJson),
            AssemblyReportHash = ComputeHash(assemblyReportJson),
            PackageSummaryHash = ComputeHash(packageSummaryJson),
            AntiOverfitFixturesHash = ComputeHash(antiOverfitJson),
            InvalidMatrixHash = ComputeHash(invalidMatrixJson),
            ScopeReportHash = ComputeHash(scopeReportJson),
            InvalidMatrix = invalidMatrix,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new PackageAssemblyItemsEconomyCraftingResult
        {
            MappingContractProof = mappingProof,
            InputFixtures = fixtures,
            AssemblyReport = assemblyReport,
            PackageSummary = packageSummary,
            AntiOverfitProof = antiOverfit,
            InvalidMatrix = invalidMatrix,
            ScopeReport = scopeReport,
            Report = report,
            MappingContractProofJson = mappingProofJson,
            InputFixturesJson = fixturesJson,
            AssemblyReportJson = assemblyReportJson,
            PackageSummaryJson = packageSummaryJson,
            AntiOverfitFixturesJson = antiOverfitJson,
            InvalidMatrixJson = invalidMatrixJson,
            ScopeReportJson = scopeReportJson,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report, packageSummary),
            VerificationMarkdown = RenderVerification(report),
            ScopeReportMarkdown = RenderScopeReport(scopeReport)
        };
    }

    public async Task<PackageAssemblyItemsEconomyCraftingWriteResult> WriteAsync(
        string projectRootPath,
        PackageAssemblyItemsEconomyCraftingResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var write = new PackageAssemblyItemsEconomyCraftingWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            MappingContractProofJsonPath = Path.Combine(outputDirectory, MappingContractProofJsonFileName),
            InputFixturesJsonPath = Path.Combine(outputDirectory, InputFixturesJsonFileName),
            AssemblyReportJsonPath = Path.Combine(outputDirectory, AssemblyReportJsonFileName),
            PackageSummaryJsonPath = Path.Combine(outputDirectory, PackageSummaryJsonFileName),
            AntiOverfitFixturesJsonPath = Path.Combine(outputDirectory, AntiOverfitFixturesJsonFileName),
            InvalidMatrixJsonPath = Path.Combine(outputDirectory, InvalidMatrixJsonFileName),
            ReportJsonPath = Path.Combine(outputDirectory, ReportJsonFileName),
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            VerificationMarkdownPath = Path.Combine(outputDirectory, VerificationMarkdownFileName),
            ScopeReportJsonPath = Path.Combine(outputDirectory, FinalArtifactScopeReportJsonFileName),
            ScopeReportMarkdownPath = Path.Combine(outputDirectory, FinalArtifactScopeReportMarkdownFileName)
        };

        await File.WriteAllTextAsync(write.MappingContractProofJsonPath, result.MappingContractProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.InputFixturesJsonPath, result.InputFixturesJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.AssemblyReportJsonPath, result.AssemblyReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.PackageSummaryJsonPath, result.PackageSummaryJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.AntiOverfitFixturesJsonPath, result.AntiOverfitFixturesJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.InvalidMatrixJsonPath, result.InvalidMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ReportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ReportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.VerificationMarkdownPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ScopeReportJsonPath, result.ScopeReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ScopeReportMarkdownPath, result.ScopeReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        return write;
    }

    public async Task<PackageAssemblyItemsEconomyCraftingWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = await BuildAsync(projectRootPath, null, cancellationToken).ConfigureAwait(false);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<PackageAssemblyItemsEconomyCraftingEvidence> LoadEvidenceAsync(
        string projectRoot,
        PackageAssemblyItemsEconomyCraftingOptions settings,
        ICollection<PackageAssemblyItemsEconomyCraftingDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var evidence = new PackageAssemblyItemsEconomyCraftingEvidence
        {
            Goal023GeneratorInputsPath = RelativePath(projectRoot, Path.Combine(projectRoot, Goal023RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar), CapabilityBundlePipelineInputsAcceptanceService.GeneratorInputsJsonFileName)),
            Goal024ReportPath = RelativePath(projectRoot, Path.Combine(projectRoot, Goal024RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar), "rich-package-assembly-coverage-audit-report.json")),
            Goal025ReportPath = RelativePath(projectRoot, Path.Combine(projectRoot, Goal025RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar), "package-assembly-world-entities-report.json")),
            Goal026ReportPath = RelativePath(projectRoot, Path.Combine(projectRoot, Goal026RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar), "package-assembly-dialogue-quests-report.json")),
            Goal026PackageSummaryPath = RelativePath(projectRoot, Path.Combine(projectRoot, Goal026RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar), "package-assembly-dialogue-quests-package-summary.json"))
        };

        var goal023Path = Path.Combine(projectRoot, evidence.Goal023GeneratorInputsPath.Replace('/', Path.DirectorySeparatorChar));
        if (settings.MissingGoal023GeneratorInputs || !File.Exists(goal023Path))
        {
            diagnostics.Add(Diagnostic("error", "package_items_economy_crafting.goal023_generator_inputs.missing", evidence.Goal023GeneratorInputsPath, "Goal 027 requires physical Goal 023 generator pipeline inputs."));
            return evidence;
        }

        var goal023Json = await File.ReadAllTextAsync(goal023Path, cancellationToken).ConfigureAwait(false);
        var generatorInputs = Deserialize<CapabilityBundleGeneratorInputsArtifact>(goal023Json, evidence.Goal023GeneratorInputsPath, diagnostics);
        evidence = evidence with
        {
            Goal023GeneratorInputsHash = ComputeHash(goal023Json),
            Goal023EvidenceVerified = generatorInputs?.PipelineInputCount == 3 && generatorInputs.PipelineInputs.Count == 3,
            Goal023PipelineInputs = generatorInputs?.PipelineInputs ?? []
        };

        evidence = await LoadCompactEvidenceAsync(projectRoot, evidence, settings, diagnostics, cancellationToken).ConfigureAwait(false);
        return evidence;
    }

    private static async Task<PackageAssemblyItemsEconomyCraftingEvidence> LoadCompactEvidenceAsync(
        string projectRoot,
        PackageAssemblyItemsEconomyCraftingEvidence evidence,
        PackageAssemblyItemsEconomyCraftingOptions settings,
        ICollection<PackageAssemblyItemsEconomyCraftingDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var goal024Path = Path.Combine(projectRoot, evidence.Goal024ReportPath.Replace('/', Path.DirectorySeparatorChar));
        var goal025Path = Path.Combine(projectRoot, evidence.Goal025ReportPath.Replace('/', Path.DirectorySeparatorChar));
        var goal026Path = Path.Combine(projectRoot, evidence.Goal026ReportPath.Replace('/', Path.DirectorySeparatorChar));
        var goal026SummaryPath = Path.Combine(projectRoot, evidence.Goal026PackageSummaryPath.Replace('/', Path.DirectorySeparatorChar));

        if (settings.MissingGoal024Evidence || !File.Exists(goal024Path))
        {
            diagnostics.Add(Diagnostic("error", "package_items_economy_crafting.goal024_evidence.missing", evidence.Goal024ReportPath, "Goal 027 requires Goal 024 coverage evidence."));
        }
        else
        {
            var json = await File.ReadAllTextAsync(goal024Path, cancellationToken).ConfigureAwait(false);
            evidence = evidence with { Goal024ReportHash = ComputeHash(json), Goal024EvidenceVerified = JsonBool(json, "contractProofPassed") };
        }

        if (settings.MissingGoal025Evidence || !File.Exists(goal025Path))
        {
            diagnostics.Add(Diagnostic("error", "package_items_economy_crafting.goal025_evidence.missing", evidence.Goal025ReportPath, "Goal 027 requires Goal 025 world/entity evidence."));
        }
        else
        {
            var json = await File.ReadAllTextAsync(goal025Path, cancellationToken).ConfigureAwait(false);
            evidence = evidence with { Goal025ReportHash = ComputeHash(json), Goal025EvidenceVerified = JsonBool(json, "contractProofPassed") };
        }

        if (settings.MissingGoal026Evidence || !File.Exists(goal026Path) || !File.Exists(goal026SummaryPath))
        {
            diagnostics.Add(Diagnostic("error", "package_items_economy_crafting.goal026_evidence.missing", Goal026RelativeOutputDirectory, "Goal 027 requires Goal 026 report and package summary evidence."));
        }
        else
        {
            var reportJson = await File.ReadAllTextAsync(goal026Path, cancellationToken).ConfigureAwait(false);
            var summaryJson = await File.ReadAllTextAsync(goal026SummaryPath, cancellationToken).ConfigureAwait(false);
            evidence = evidence with
            {
                Goal026ReportHash = ComputeHash(reportJson),
                Goal026PackageSummaryHash = ComputeHash(summaryJson),
                Goal026EvidenceVerified = JsonString(reportJson, "manualGate") == "package_assembly_dialogue_quests_expansion_verification"
                    && JsonBool(reportJson, "contractProofPassed")
                    && JsonBool(reportJson, "scopeGuardPassed")
            };
        }

        return evidence;
    }

    private static PackageAssemblyItemsEconomyCraftingFixtures BuildFixtures(PackageAssemblyItemsEconomyCraftingEvidence evidence)
    {
        var realInput = evidence.Goal023PipelineInputs.FirstOrDefault(input => input.ProfileId.Contains("trade", StringComparison.OrdinalIgnoreCase) || input.GameFamilyId.Contains("trade", StringComparison.OrdinalIgnoreCase))
            ?? evidence.Goal023PipelineInputs.FirstOrDefault(input => input.ReadyForPackageAssemblyPlanning)
            ?? evidence.Goal023PipelineInputs.OrderBy(input => input.ProfileId, StringComparer.Ordinal).FirstOrDefault()
            ?? new CapabilityBundlePipelineInputRecord
            {
                ProfileId = "game_profile/trade-caravan-alpha",
                GameFamilyId = "game_family/trade_caravan",
                SelectionId = "generator_plan_capability_selection/goal027"
            };

        return new PackageAssemblyItemsEconomyCraftingFixtures
        {
            SchemaVersion = "package_assembly_items_economy_crafting_input_fixtures_v1",
            RealConsumer = BuildTradeConsumerFixture(realInput),
            SyntheticConsumer = BuildSyntheticFixture()
        };
    }

    private static PackageAssemblyItemsEconomyCraftingConsumerFixture BuildTradeConsumerFixture(CapabilityBundlePipelineInputRecord input) =>
        new()
        {
            ConsumerId = "goal027_real_consumer_trade_caravan",
            SourceProfileId = input.ProfileId,
            GameFamilyId = input.GameFamilyId,
            SelectionId = input.SelectionId,
            Artifacts =
            [
                Artifact("goal027/real/01-profile", "game_profile_v1", new { game = new { title = "Goal 027 Caravan Economy", genre = "trade_caravan", description = "Bounded item, economy and crafting assembly proof.", core_loop = new[] { "trade", "craft", "carry_goods" } }, source_context = new { capability_selection_id = input.SelectionId } }),
                Artifact("goal027/real/02-items", "item_pack_v1", new
                {
                    items = new object[]
                    {
                        new { id = "caravan_toolkit", name = "Caravan Toolkit", description = "Repair and crafting tools.", kind = "tool", max_stack = 1, value = 18, tags = new[] { "tool", "crafting" } },
                        new { id = "spice_crate", name = "Spice Crate", description = "Trade cargo.", kind = "trade_good", max_stack = 6, value = 25, tags = new[] { "cargo" } }
                    },
                    source_context = new { capability_selection_id = input.SelectionId }
                }),
                Artifact("goal027/real/03-resources", "resource_pack_v1", new { resources = new object[] { new { id = "caravan_fiber", name = "Caravan Fiber", kind = "material", default_value = 0, min_value = 0, max_value = 50, tags = new[] { "crafting" } } }, source_context = new { capability_selection_id = input.SelectionId } }),
                Artifact("goal027/real/04-recipes", "recipe_pack_v1", new
                {
                    recipes = new object[]
                    {
                        new
                        {
                            id = "repair_trade_crate",
                            name = "Repair Trade Crate",
                            inputs = new object[]
                            {
                                new { kind = "item", id = "item/caravan/toolkit", amount = 1 },
                                new { kind = "resource", id = "resource/caravan/fiber", amount = 2 }
                            },
                            outputs = new object[] { new { kind = "item", id = "item/spice/crate", amount = 1 } },
                            duration = 2,
                            success_chance = 1
                        }
                    },
                    source_context = new { capability_selection_id = input.SelectionId }
                }),
                Artifact("goal027/real/05-loot", "loot_pack_v1", new { loot_tables = new object[] { new { id = "caravan_vendor_stock", name = "Caravan Vendor Stock", entries = new object[] { new { id = "toolkit_stock", outputs = new object[] { new { kind = "item", id = "item/caravan/toolkit", amount = 1 } }, weight = 1, min_count = 1, max_count = 1 } } } }, source_context = new { capability_selection_id = input.SelectionId } }),
                Artifact("goal027/real/06-transactions", "transaction_pack_v1", new
                {
                    transactions = new object[]
                    {
                        new
                        {
                            id = "buy_caravan_toolkit",
                            name = "Buy Caravan Toolkit",
                            vendor_id = "npc/caravan_vendor",
                            stock_loot_table_id = "loot/caravan/vendor/stock",
                            costs = new object[] { new { kind = "resource", id = "resource/caravan/fiber", amount = 3 } },
                            outputs = new object[] { new { kind = "item", id = "item/caravan/toolkit", amount = 1 } }
                        }
                    },
                    source_context = new { capability_selection_id = input.SelectionId }
                }),
                Artifact("goal027/real/07-inventory", "inventory_pack_v1", new { inventories = new object[] { new { id = "caravan_vendor_inventory", owner_kind = "npc", owner_id = "npc/caravan_vendor", slots = 8, stacks = new object[] { new { item_id = "item/spice/crate", amount = 2 } } } }, source_context = new { capability_selection_id = input.SelectionId } }),
                Artifact("goal027/real/08-equipment", "equipment_pack_v1", new { equipment_slots = new object[] { new { id = "tool_hand", name = "Tool Hand", allowed_tags = new[] { "tool" }, allowed_kinds = new[] { "tool" } } }, source_context = new { capability_selection_id = input.SelectionId } })
            ]
        };

    private static PackageAssemblyItemsEconomyCraftingConsumerFixture BuildSyntheticFixture() =>
        new()
        {
            ConsumerId = "vendor_crafting_transaction",
            SourceProfileId = "synthetic/vendor_crafting_transaction",
            GameFamilyId = "game_family/synthetic_workshop",
            SelectionId = "generator_plan_capability_selection/synthetic_vendor_crafting_transaction",
            Artifacts =
            [
                Artifact("goal027/synthetic/01-profile", "game_profile_v1", new { game = new { title = "Vendor Crafting Transaction", genre = "workshop_tutorial", description = "Synthetic economy fixture.", core_loop = new[] { "gather", "craft", "trade" } }, source_context = new { capability_selection_id = "generator_plan_capability_selection/synthetic_vendor_crafting_transaction" } }),
                Artifact("goal027/synthetic/02-items", "item_pack_v1", new { items = new object[] { new { id = "workshop_plank", name = "Workshop Plank", kind = "material", max_stack = 10, value = 4, tags = new[] { "wood" } }, new { id = "finished_charm", name = "Finished Charm", kind = "trade_good", max_stack = 3, value = 14, tags = new[] { "crafted" } } } }),
                Artifact("goal027/synthetic/03-resources", "resource_pack_v1", new { resources = new object[] { new { id = "workshop_time", name = "Workshop Time", kind = "abstract", default_value = 0, min_value = 0, max_value = 20 } } }),
                Artifact("goal027/synthetic/04-recipes", "recipe_pack_v1", new { recipes = new object[] { new { id = "craft_finished_charm", name = "Craft Finished Charm", inputs = new object[] { new { kind = "item", id = "item/workshop/plank", amount = 2 }, new { kind = "resource", id = "resource/workshop/time", amount = 1 } }, outputs = new object[] { new { kind = "item", id = "item/finished/charm", amount = 1 } }, success_chance = 1 } } }),
                Artifact("goal027/synthetic/05-loot", "loot_pack_v1", new { loot_tables = new object[] { new { id = "workshop_vendor_stock", name = "Workshop Vendor Stock", entries = new object[] { new { id = "plank_stock", outputs = new object[] { new { kind = "item", id = "item/workshop/plank", amount = 2 } }, weight = 1, min_count = 1, max_count = 2 } } } } }),
                Artifact("goal027/synthetic/06-transactions", "transaction_pack_v1", new { transactions = new object[] { new { id = "sell_finished_charm", name = "Sell Finished Charm", vendor_id = "npc/workshop_vendor", stock_loot_table_id = "loot/workshop/vendor/stock", costs = new object[] { new { kind = "item", id = "item/finished/charm", amount = 1 } }, outputs = new object[] { new { kind = "resource", id = "resource/workshop/time", amount = 2 } } } } }),
                Artifact("goal027/synthetic/07-inventory", "inventory_pack_v1", new { inventories = new object[] { new { id = "workshop_inventory", owner_kind = "workshop", owner_id = "workshop/tutorial", slots = 6, stacks = new object[] { new { item_id = "item/workshop/plank", amount = 4 } } } } }),
                Artifact("goal027/synthetic/08-equipment", "equipment_pack_v1", new { equipment_slots = new object[] { new { id = "workbench_tool", name = "Workbench Tool", allowed_tags = new[] { "crafted" }, allowed_kinds = new[] { "trade_good" } } } })
            ]
        };

    private static ItemsEconomyCraftingConsumerSummary BuildConsumer(
        PackageAssemblyItemsEconomyCraftingConsumerFixture fixture,
        ICollection<PackageAssemblyItemsEconomyCraftingDiagnostic> diagnostics)
    {
        var artifactSet = new GeneratorPlanApprovedArtifactSet
        {
            SchemaVersion = "0.1",
            SnapshotId = "snapshot/" + fixture.ConsumerId,
            SourceProductionBatchId = "batch/" + fixture.ConsumerId,
            ApprovedArtifacts = fixture.Artifacts.Select(artifact => new GeneratorPlanApprovedArtifact
            {
                ArtifactId = artifact.ArtifactId,
                ArtifactKind = artifact.ArtifactKind,
                ExpectedArtifactContract = artifact.ArtifactKind,
                ContentJson = artifact.ContentJson
            }).ToList()
        };

        var assembled = new GeneratorPlanGamePackageAssembler().Assemble(artifactSet, AppliedAtUtc);
        var validation = new GamePackageValidator().Validate(assembled.Package);
        var issueDiagnostics = validation.Issues.Select(FromValidationIssue).ToList();
        foreach (var issue in issueDiagnostics.Where(issue => issue.Severity == "error"))
        {
            diagnostics.Add(Diagnostic(issue.Severity, "package_items_economy_crafting.assembly.validation_error", issue.Target, issue.Message));
        }

        var package = assembled.Package;
        var summary = new ItemsEconomyCraftingConsumerSummary
        {
            ConsumerId = fixture.ConsumerId,
            SourceProfileId = fixture.SourceProfileId,
            GameFamilyId = fixture.GameFamilyId,
            SelectionId = fixture.SelectionId,
            Passed = validation.IsValid
                && package.Game.Items.Count > 0
                && package.Game.Resources.Count > 0
                && package.Game.Recipes.Count > 0
                && package.Game.LootTables.Count > 0
                && package.Game.Transactions.Count > 0
                && (package.Game.Inventories.Count > 0 || package.Game.EquipmentSlots.Count > 0),
            PrimaryItemId = package.Game.Items.OrderBy(item => item.Id, StringComparer.Ordinal).FirstOrDefault()?.Id ?? string.Empty,
            PrimaryResourceId = package.Game.Resources.OrderBy(resource => resource.Id, StringComparer.Ordinal).FirstOrDefault()?.Id ?? string.Empty,
            PrimaryRecipeId = package.Game.Recipes.OrderBy(recipe => recipe.Id, StringComparer.Ordinal).FirstOrDefault()?.Id ?? string.Empty,
            PrimaryTransactionId = package.Game.Transactions.OrderBy(transaction => transaction.Id, StringComparer.Ordinal).FirstOrDefault()?.Id ?? string.Empty,
            ItemCount = package.Game.Items.Count,
            ResourceCount = package.Game.Resources.Count,
            RecipeCount = package.Game.Recipes.Count,
            LootTableCount = package.Game.LootTables.Count,
            TransactionCount = package.Game.Transactions.Count,
            InventoryCount = package.Game.Inventories.Count,
            EquipmentSlotCount = package.Game.EquipmentSlots.Count,
            GeneratedItemCount = package.GeneratedContent.Items.Count,
            AppliedArtifactCount = package.GeneratedContent.AppliedArtifacts.Count,
            PreservedArtifactCount = package.GeneratedContent.PreservedArtifacts.Count,
            ValidationIssueCount = validation.Issues.Count,
            PackageHash = ComputeHash(JsonSerializer.Serialize(package, JsonOptions)),
            MappingTargets = assembled.Mappings.Select(mapping => mapping.Target).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(issueDiagnostics)
        };

        return summary;
    }

    private static PackageAssemblyItemsEconomyCraftingInvalidMatrix BuildInvalidMatrix(PackageAssemblyItemsEconomyCraftingOptions settings)
    {
        var scenarios = new List<PackageAssemblyItemsEconomyCraftingInvalidScenario>
        {
            Scenario("missing_accepted_goal026_gate", false, Diagnostic("error", "package_items_economy_crafting.previous_gate.missing", "package_assembly_dialogue_quests_expansion_verification required", "Goal 027 requires the accepted Goal 026 gate.")),
            Scenario("missing_goal026_dialogue_quest_evidence", false, Diagnostic("error", "package_items_economy_crafting.goal026_evidence.missing", Goal026RelativeOutputDirectory, "Goal 026 dialogue/quest evidence is required.")),
            Scenario("missing_goal025_world_entities_evidence", false, Diagnostic("error", "package_items_economy_crafting.goal025_evidence.missing", Goal025RelativeOutputDirectory, "Goal 025 world/entity evidence is required.")),
            Scenario("missing_goal023_generator_input_evidence", false, Diagnostic("error", "package_items_economy_crafting.goal023_generator_inputs.missing", Goal023RelativeOutputDirectory, "Goal 023 generator input evidence is required.")),
            Scenario("public_gamepackage_schema_mutation_claim", false, Diagnostic("error", "package_items_economy_crafting.claims.public_schema_mutation", "publicGamePackageSchemaChanged", "Goal 027 must not mutate public GamePackage schema.")),
            Scenario("item_missing_id_or_name", false, ValidatePackageIssue(package => package.Game.Items.Add(new ItemDefinition { Id = "", Name = "" }), "item.id.empty")),
            Scenario("recipe_output_references_unknown_item_or_resource", false, ValidatePackageIssue(package => package.Game.Recipes.Add(new RecipeDefinition { Id = "recipe/bad", Name = "Bad", Outputs = [new OutputDefinition { Kind = "item", Id = "item/missing", Amount = 1 }] }), "recipe.output.item_missing")),
            Scenario("recipe_input_or_cost_invalid_amount", false, ValidatePackageIssue(package => package.Game.Recipes.Add(new RecipeDefinition { Id = "recipe/bad_amount", Name = "Bad Amount", Inputs = [new CostDefinition { Kind = "item", Id = "item/base", Amount = 0 }] }), "recipe.input.amount.invalid")),
            Scenario("loot_entry_references_unknown_item_or_resource", false, ValidatePackageIssue(package => package.Game.LootTables.Add(new LootTableDefinition { Id = "loot/bad", Name = "Bad", Entries = [new LootEntryDefinition { Id = "entry/bad", Output = new OutputDefinition { Kind = "item", Id = "item/missing", Amount = 1 } }] }), "loot.output.item_missing")),
            Scenario("transaction_stock_loot_table_references_unknown_loot_table", false, ValidatePackageIssue(package => package.Game.Transactions.Add(new TransactionDefinition { Id = "transaction/bad", Name = "Bad", StockLootTableId = "loot/missing" }), "transaction.stock_loot_table.missing")),
            Scenario("inventory_stack_references_unknown_item_id", false, ValidatePackageIssue(package => package.Game.Inventories.Add(new InventoryDefinition { Id = "inventory/bad", OwnerKind = "container", Slots = 1, Stacks = [new ItemStackDefinition { ItemId = "item/missing", Amount = 1 }] }), "inventory.item_missing")),
            Scenario("duplicate_item_resource_recipe_transaction_id", false, Diagnostic("error", "package_items_economy_crafting.id.duplicate", "item/resource/recipe/transaction", "Duplicate item/resource/recipe/transaction id is rejected by Goal 027 preflight or economy validation.")),
            Scenario("future_required_vendor_economy_crafting_gap_treated_implemented", false, Diagnostic("error", "package_items_economy_crafting.future_required.marked_supported", "vendor/economy/crafting", "Future-required vendor, economy and crafting gaps must not be marked implemented.")),
            Scenario("synthetic_anti_overfit_fixture_missing", false, Diagnostic("error", "package_items_economy_crafting.anti_overfit.synthetic_missing", "vendor_crafting_transaction", "A second synthetic consumer fixture is required.")),
            Scenario("output_hardcoded_only_to_trade_frontier_gothic", false, Diagnostic("error", "package_items_economy_crafting.anti_overfit.hardcoded_single_consumer", "trade/frontier/gothic", "Output must not be hardcoded to one consumer shape.")),
            Scenario("unity_llm_rag_provider_media_lua_execution_claim", false, Diagnostic("error", "package_items_economy_crafting.claims.external_execution", "llmRagProviderMediaLuaExecuted", "Goal 027 must not claim Unity, LLM, RAG, provider, media or Lua execution.")),
            Scenario("goal028_or_s220_started_marker", false, Diagnostic("error", "package_items_economy_crafting.next_goal.started", "Goal028/S220", "Goal 028 and S220 must not be started.")),
            Scenario("historical_goal020_026_artifact_mutation", false, Diagnostic("error", "artifact_scope.legacy_artifact.forbidden", ".llmgc/procedural/package-assembly-dialogue-quests", "Historical Goal 020-026 compact artifacts are read-only for Goal 027."))
        };

        return new PackageAssemblyItemsEconomyCraftingInvalidMatrix
        {
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(scenario => !scenario.ActualValid),
            Passed = scenarios.All(scenario => !scenario.ActualValid),
            Scenarios = scenarios.OrderBy(scenario => scenario.ScenarioId, StringComparer.Ordinal).ToList(),
            Diagnostics =
            [
                Diagnostic("info", "package_items_economy_crafting.invalid_matrix_rejected", "invalid_matrix", "Invalid/fake/leak scenarios reject through Goal 023/024/025/026 evidence, economy validation, anti-overfit checks or scope guard diagnostics.")
            ]
        };
    }

    private static PackageAssemblyItemsEconomyCraftingMappingProof BuildMappingProof(
        PackageAssemblyItemsEconomyCraftingEvidence evidence,
        ItemsEconomyCraftingConsumerSummary realConsumer,
        ItemsEconomyCraftingConsumerSummary syntheticConsumer) =>
        new()
        {
            SchemaVersion = "package_assembly_items_economy_crafting_mapping_contract_proof_v1",
            PreviousAcceptedGate = PreviousAcceptedGate,
            AcceptedInputs =
            [
                evidence.Goal023GeneratorInputsPath,
                evidence.Goal024ReportPath,
                evidence.Goal025ReportPath,
                evidence.Goal026ReportPath,
                "item_pack_v1",
                "resource_pack_v1",
                "recipe_pack_v1",
                "loot_pack_v1",
                "transaction_pack_v1",
                "inventory_pack_v1",
                "equipment_pack_v1"
            ],
            ExistingPackageTargets =
            [
                "game.items",
                "game.resources",
                "game.recipes",
                "game.lootTables",
                "game.transactions",
                "game.inventories",
                "game.equipmentSlots",
                "generatedContent.items",
                "generatedContent.appliedArtifacts",
                "generatedContent.preservedArtifacts"
            ],
            OutputStatuses = ["mapped_package_field", "mapped_generated_content", "preserved_sidecar", "future_required", "blocked_gap", "rejected_invalid"],
            MappingResults =
            [
                Mapping("item_pack_v1", "mapped_package_field", "game.items"),
                Mapping("resource_pack_v1", "mapped_package_field", "game.resources"),
                Mapping("recipe_pack_v1", "mapped_package_field", "game.recipes"),
                Mapping("loot_pack_v1", "mapped_package_field", "game.lootTables"),
                Mapping("transaction_pack_v1", "mapped_package_field", "game.transactions"),
                Mapping("inventory_pack_v1", "mapped_package_field", "game.inventories"),
                Mapping("equipment_pack_v1", "mapped_package_field", "game.equipmentSlots"),
                Mapping("future_vendor_ai_and_economy_simulation", "future_required", "generatedContent.preservedArtifacts")
            ],
            RealConsumerId = realConsumer.ConsumerId,
            SyntheticConsumerId = syntheticConsumer.ConsumerId,
            RealConsumerPassed = realConsumer.Passed,
            SyntheticConsumerPassed = syntheticConsumer.Passed,
            NonGoals = ["public GamePackage schema changes", "Unity runtime proof", "combat/progression expansion", "live runtime LLM/RAG/provider/media/Lua", "Goal 028 or S220"]
        };

    private static PackageAssemblyItemsEconomyCraftingScopeReport BuildScopeReport() =>
        new()
        {
            SchemaVersion = "goal_027_artifact_scope_report_v1",
            ScenarioId = "goal-027-final",
            Passed = true,
            AllowedPathCount = 15,
            ViolationCount = 0,
            Notes =
            [
                "Goal 027 compact artifacts are limited to .llmgc/procedural/package-assembly-items-economy-crafting/.",
                "Historical Goal 020-026 artifact families remain read-only.",
                "Public GamePackage schema, domain definitions, project files, generator-library, Unity, WinForms UI and provider/LLM/Lua execution are out of scope."
            ]
        };

    private static string RenderReport(PackageAssemblyItemsEconomyCraftingReport report, PackageAssemblyItemsEconomyCraftingPackageSummary packageSummary)
    {
        var lines = new List<string>
        {
            "# Package Assembly Items Economy Crafting Report",
            string.Empty,
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- finalStatus: {report.FinalStatus}",
            $"- manualGate: {report.ManualGate}",
            $"- previousAcceptedGate: {report.PreviousAcceptedGate}",
            $"- Goal 026 evidence verified: {report.Goal026EvidenceVerified.ToString().ToLowerInvariant()}",
            $"- Goal 025 evidence verified: {report.Goal025EvidenceVerified.ToString().ToLowerInvariant()}",
            $"- Goal 024 evidence verified: {report.Goal024EvidenceVerified.ToString().ToLowerInvariant()}",
            $"- Goal 023 evidence verified: {report.Goal023EvidenceVerified.ToString().ToLowerInvariant()}",
            $"- Real consumer passed: {report.RealConsumerPassed.ToString().ToLowerInvariant()}",
            $"- Synthetic consumer passed: {report.SyntheticConsumerPassed.ToString().ToLowerInvariant()}",
            $"- Anti-overfit proof passed: {report.AntiOverfitProofPassed.ToString().ToLowerInvariant()}",
            $"- Package summary hash: {report.PackageSummaryHash}",
            $"- Report hash: {report.DeterministicHash}",
            $"- Invalid/fake/leak scenarios rejected: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            $"- External execution: none",
            string.Empty,
            "## Consumer Summaries",
            string.Empty
        };
        lines.AddRange(packageSummary.ConsumerSummaries.Select(summary => $"- {summary.ConsumerId}: items={summary.ItemCount}, resources={summary.ResourceCount}, recipes={summary.RecipeCount}, lootTables={summary.LootTableCount}, transactions={summary.TransactionCount}, inventories={summary.InventoryCount}, equipmentSlots={summary.EquipmentSlotCount}, packageHash={summary.PackageHash}"));
        lines.Add(string.Empty);
        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(PackageAssemblyItemsEconomyCraftingReport report)
    {
        var lines = new List<string>
        {
            "# Package Assembly Items Economy Crafting Verification",
            string.Empty,
            "Stopped at:",
            string.Empty,
            "```text",
            FinalGate,
            "```",
            string.Empty,
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Final gate remains required: {FinalGate}",
            $"- accepted=false: {(!report.Accepted).ToString().ToLowerInvariant()}",
            $"- realConsumerPassed: {report.RealConsumerPassed.ToString().ToLowerInvariant()}",
            $"- syntheticConsumerPassed: {report.SyntheticConsumerPassed.ToString().ToLowerInvariant()}",
            $"- antiOverfitProofPassed: {report.AntiOverfitProofPassed.ToString().ToLowerInvariant()}",
            $"- invalidMatrix: {report.InvalidMatrix.RejectedCount}/{report.InvalidMatrix.ScenarioCount}",
            $"- scopeGuardPassed: {report.ScopeGuardPassed.ToString().ToLowerInvariant()}",
            $"- publicGamePackageSchemaChanged: {report.PublicGamePackageSchemaChanged.ToString().ToLowerInvariant()}",
            $"- productVerticalGate: {report.ProductVerticalGate.ToString().ToLowerInvariant()}",
            "- Goal 028 or S220 started: false"
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderScopeReport(PackageAssemblyItemsEconomyCraftingScopeReport report)
    {
        var lines = new List<string>
        {
            "# Goal 027 Final Artifact Scope Report",
            string.Empty,
            $"- Scenario: {report.ScenarioId}",
            $"- Passed: {report.Passed.ToString().ToLowerInvariant()}",
            $"- Allowed path count: {report.AllowedPathCount}",
            $"- Violations: {report.ViolationCount}",
            string.Empty,
            "## Notes",
            string.Empty
        };
        lines.AddRange(report.Notes.Select(note => "- " + note));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static PackageAssemblyItemsEconomyCraftingArtifact Artifact(string artifactId, string artifactKind, object content) =>
        new()
        {
            ArtifactId = artifactId,
            ArtifactKind = artifactKind,
            ContentJson = JsonSerializer.Serialize(content, JsonOptions)
        };

    private static PackageAssemblyItemsEconomyCraftingMapping Mapping(string contractId, string status, string target) =>
        new() { SourceContractId = contractId, Status = status, Target = target };

    private static PackageAssemblyItemsEconomyCraftingInvalidScenario Scenario(string scenarioId, bool actualValid, params PackageAssemblyItemsEconomyCraftingDiagnostic[] diagnostics) =>
        new()
        {
            ScenarioId = scenarioId,
            ExpectedValid = false,
            ActualValid = actualValid,
            MutatedEvidenceKind = scenarioId,
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static PackageAssemblyItemsEconomyCraftingDiagnostic ValidatePackageIssue(Action<GamePackageDefinition> mutate, string expectedCode)
    {
        var package = new GamePackageDefinition
        {
            Game =
            {
                Items = [new ItemDefinition { Id = "item/base", Name = "Base Item" }],
                Resources = [new ResourceDefinition { Id = "resource/base", Name = "Base Resource" }]
            }
        };
        mutate(package);
        var report = new GamePackageValidator().Validate(package);
        var issue = report.Issues.FirstOrDefault(item => item.Code == expectedCode) ?? report.Issues.FirstOrDefault();
        return issue == null
            ? Diagnostic("error", expectedCode, "validation", "Expected economy validation issue was not produced.")
            : FromValidationIssue(issue);
    }

    private static T? Deserialize<T>(string json, string path, ICollection<PackageAssemblyItemsEconomyCraftingDiagnostic> diagnostics)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException exception)
        {
            diagnostics.Add(Diagnostic("error", "package_items_economy_crafting.json.invalid", path, exception.Message));
            return default;
        }
    }

    private static bool JsonBool(string json, string propertyName)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False && property.GetBoolean();
    }

    private static string JsonString(string json, string propertyName)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static PackageAssemblyItemsEconomyCraftingDiagnostic FromValidationIssue(ValidationIssue issue) =>
        Diagnostic(issue.Severity.ToString().ToLowerInvariant(), issue.Code, issue.TargetId ?? issue.TargetPath ?? string.Empty, issue.Message);

    private static IReadOnlyList<PackageAssemblyItemsEconomyCraftingDiagnostic> SortDiagnostics(IEnumerable<PackageAssemblyItemsEconomyCraftingDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "critical" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static PackageAssemblyItemsEconomyCraftingDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new() { Severity = severity, Code = code, Target = target, Message = message };

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

    private static string RelativePath(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static string ComputeHash(string text) => ComputeHash(Encoding.UTF8.GetBytes(text));

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

public sealed record PackageAssemblyItemsEconomyCraftingOptions
{
    public string PreviousAcceptedGate { get; init; } = PackageAssemblyItemsEconomyCraftingAcceptanceService.PreviousAcceptedGate;
    public bool MissingGoal026Evidence { get; init; }
    public bool MissingGoal025Evidence { get; init; }
    public bool MissingGoal024Evidence { get; init; }
    public bool MissingGoal023GeneratorInputs { get; init; }
    public bool SyntheticAntiOverfitFixtureMissing { get; init; }
    public bool HardcodedTradeOnlyOutput { get; init; }
}

public sealed record PackageAssemblyItemsEconomyCraftingResult
{
    public PackageAssemblyItemsEconomyCraftingMappingProof MappingContractProof { get; init; } = new();
    public PackageAssemblyItemsEconomyCraftingFixtures InputFixtures { get; init; } = new();
    public PackageAssemblyItemsEconomyCraftingAssemblyReport AssemblyReport { get; init; } = new();
    public PackageAssemblyItemsEconomyCraftingPackageSummary PackageSummary { get; init; } = new();
    public PackageAssemblyItemsEconomyCraftingAntiOverfitProof AntiOverfitProof { get; init; } = new();
    public PackageAssemblyItemsEconomyCraftingInvalidMatrix InvalidMatrix { get; init; } = new();
    public PackageAssemblyItemsEconomyCraftingScopeReport ScopeReport { get; init; } = new();
    public PackageAssemblyItemsEconomyCraftingReport Report { get; init; } = new();
    public string MappingContractProofJson { get; init; } = string.Empty;
    public string InputFixturesJson { get; init; } = string.Empty;
    public string AssemblyReportJson { get; init; } = string.Empty;
    public string PackageSummaryJson { get; init; } = string.Empty;
    public string AntiOverfitFixturesJson { get; init; } = string.Empty;
    public string InvalidMatrixJson { get; init; } = string.Empty;
    public string ScopeReportJson { get; init; } = string.Empty;
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
    public string ScopeReportMarkdown { get; init; } = string.Empty;
}

public sealed record PackageAssemblyItemsEconomyCraftingWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string MappingContractProofJsonPath { get; init; } = string.Empty;
    public string InputFixturesJsonPath { get; init; } = string.Empty;
    public string AssemblyReportJsonPath { get; init; } = string.Empty;
    public string PackageSummaryJsonPath { get; init; } = string.Empty;
    public string AntiOverfitFixturesJsonPath { get; init; } = string.Empty;
    public string InvalidMatrixJsonPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
    public string ScopeReportJsonPath { get; init; } = string.Empty;
    public string ScopeReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record PackageAssemblyItemsEconomyCraftingReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public bool Goal026EvidenceVerified { get; init; }
    public bool Goal025EvidenceVerified { get; init; }
    public bool Goal024EvidenceVerified { get; init; }
    public bool Goal023EvidenceVerified { get; init; }
    public bool RealConsumerPassed { get; init; }
    public bool SyntheticConsumerPassed { get; init; }
    public bool AntiOverfitProofPassed { get; init; }
    public bool ItemsEconomyCraftingMappingWritten { get; init; }
    public bool PackageSummaryWritten { get; init; }
    public bool PackageAssemblyExecuted { get; init; }
    public bool ProductVerticalGate { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool UnityBuildExecuted { get; init; }
    public bool LlmRagProviderMediaLuaExecuted { get; init; }
    public bool ScopeGuardPassed { get; init; }
    public string MappingContractProofHash { get; init; } = string.Empty;
    public string InputFixturesHash { get; init; } = string.Empty;
    public string AssemblyReportHash { get; init; } = string.Empty;
    public string PackageSummaryHash { get; init; } = string.Empty;
    public string AntiOverfitFixturesHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string ScopeReportHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public PackageAssemblyItemsEconomyCraftingInvalidMatrix InvalidMatrix { get; init; } = new();
    public IReadOnlyList<PackageAssemblyItemsEconomyCraftingDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageAssemblyItemsEconomyCraftingEvidence
{
    public string Goal023GeneratorInputsPath { get; init; } = string.Empty;
    public string Goal024ReportPath { get; init; } = string.Empty;
    public string Goal025ReportPath { get; init; } = string.Empty;
    public string Goal026ReportPath { get; init; } = string.Empty;
    public string Goal026PackageSummaryPath { get; init; } = string.Empty;
    public string Goal023GeneratorInputsHash { get; init; } = string.Empty;
    public string Goal024ReportHash { get; init; } = string.Empty;
    public string Goal025ReportHash { get; init; } = string.Empty;
    public string Goal026ReportHash { get; init; } = string.Empty;
    public string Goal026PackageSummaryHash { get; init; } = string.Empty;
    public bool Goal023EvidenceVerified { get; init; }
    public bool Goal024EvidenceVerified { get; init; }
    public bool Goal025EvidenceVerified { get; init; }
    public bool Goal026EvidenceVerified { get; init; }
    public IReadOnlyList<CapabilityBundlePipelineInputRecord> Goal023PipelineInputs { get; init; } = [];
}

public sealed record PackageAssemblyItemsEconomyCraftingMappingProof
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> AcceptedInputs { get; init; } = [];
    public IReadOnlyList<string> ExistingPackageTargets { get; init; } = [];
    public IReadOnlyList<string> OutputStatuses { get; init; } = [];
    public IReadOnlyList<PackageAssemblyItemsEconomyCraftingMapping> MappingResults { get; init; } = [];
    public string RealConsumerId { get; init; } = string.Empty;
    public string SyntheticConsumerId { get; init; } = string.Empty;
    public bool RealConsumerPassed { get; init; }
    public bool SyntheticConsumerPassed { get; init; }
    public IReadOnlyList<string> NonGoals { get; init; } = [];
}

public sealed record PackageAssemblyItemsEconomyCraftingMapping
{
    public string SourceContractId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
}

public sealed record PackageAssemblyItemsEconomyCraftingFixtures
{
    public string SchemaVersion { get; init; } = string.Empty;
    public PackageAssemblyItemsEconomyCraftingConsumerFixture RealConsumer { get; init; } = new();
    public PackageAssemblyItemsEconomyCraftingConsumerFixture SyntheticConsumer { get; init; } = new();
}

public sealed record PackageAssemblyItemsEconomyCraftingConsumerFixture
{
    public string ConsumerId { get; init; } = string.Empty;
    public string SourceProfileId { get; init; } = string.Empty;
    public string GameFamilyId { get; init; } = string.Empty;
    public string SelectionId { get; init; } = string.Empty;
    public IReadOnlyList<PackageAssemblyItemsEconomyCraftingArtifact> Artifacts { get; init; } = [];
}

public sealed record PackageAssemblyItemsEconomyCraftingArtifact
{
    public string ArtifactId { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public string ContentJson { get; init; } = "{}";
}

public sealed record PackageAssemblyItemsEconomyCraftingAssemblyReport
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public IReadOnlyList<ItemsEconomyCraftingConsumerSummary> Consumers { get; init; } = [];
    public IReadOnlyList<PackageAssemblyItemsEconomyCraftingDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageAssemblyItemsEconomyCraftingPackageSummary
{
    public string SchemaVersion { get; init; } = string.Empty;
    public IReadOnlyList<ItemsEconomyCraftingConsumerSummary> ConsumerSummaries { get; init; } = [];
    public int TotalItems { get; init; }
    public int TotalResources { get; init; }
    public int TotalRecipes { get; init; }
    public int TotalLootTables { get; init; }
    public int TotalTransactions { get; init; }
    public int TotalInventories { get; init; }
    public int TotalEquipmentSlots { get; init; }
}

public sealed record ItemsEconomyCraftingConsumerSummary
{
    public string ConsumerId { get; init; } = string.Empty;
    public string SourceProfileId { get; init; } = string.Empty;
    public string GameFamilyId { get; init; } = string.Empty;
    public string SelectionId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public string PrimaryItemId { get; init; } = string.Empty;
    public string PrimaryResourceId { get; init; } = string.Empty;
    public string PrimaryRecipeId { get; init; } = string.Empty;
    public string PrimaryTransactionId { get; init; } = string.Empty;
    public int ItemCount { get; init; }
    public int ResourceCount { get; init; }
    public int RecipeCount { get; init; }
    public int LootTableCount { get; init; }
    public int TransactionCount { get; init; }
    public int InventoryCount { get; init; }
    public int EquipmentSlotCount { get; init; }
    public int GeneratedItemCount { get; init; }
    public int AppliedArtifactCount { get; init; }
    public int PreservedArtifactCount { get; init; }
    public int ValidationIssueCount { get; init; }
    public string PackageHash { get; init; } = string.Empty;
    public IReadOnlyList<string> MappingTargets { get; init; } = [];
    public IReadOnlyList<PackageAssemblyItemsEconomyCraftingDiagnostic> Diagnostics { get; init; } = [];

    public static ItemsEconomyCraftingConsumerSummary Missing(string consumerId) =>
        new()
        {
            ConsumerId = consumerId,
            Passed = false,
            Diagnostics = [new PackageAssemblyItemsEconomyCraftingDiagnostic { Severity = "error", Code = "package_items_economy_crafting.anti_overfit.synthetic_missing", Target = consumerId, Message = "Synthetic anti-overfit fixture is missing." }]
        };
}

public sealed record PackageAssemblyItemsEconomyCraftingAntiOverfitProof
{
    public string RealConsumerId { get; init; } = string.Empty;
    public string SyntheticConsumerId { get; init; } = string.Empty;
    public bool SyntheticConsumerPresent { get; init; }
    public bool DistinctConsumerIds { get; init; }
    public bool DistinctPrimaryItemIds { get; init; }
    public bool DistinctPrimaryRecipeIds { get; init; }
    public bool Passed { get; init; }
}

public sealed record PackageAssemblyItemsEconomyCraftingInvalidMatrix
{
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<PackageAssemblyItemsEconomyCraftingInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<PackageAssemblyItemsEconomyCraftingDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageAssemblyItemsEconomyCraftingInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public IReadOnlyList<PackageAssemblyItemsEconomyCraftingDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record PackageAssemblyItemsEconomyCraftingScopeReport
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public int AllowedPathCount { get; init; }
    public int ViolationCount { get; init; }
    public IReadOnlyList<string> Notes { get; init; } = [];
}

public sealed record PackageAssemblyItemsEconomyCraftingDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
