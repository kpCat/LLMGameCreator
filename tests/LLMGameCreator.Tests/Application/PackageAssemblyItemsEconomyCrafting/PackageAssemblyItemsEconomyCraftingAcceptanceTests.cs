using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Design.PackageAssemblyItemsEconomyCrafting;
using LLMGameCreator.Application.Validation;
using Xunit;

namespace LLMGameCreator.Tests.Application.PackageAssemblyItemsEconomyCrafting;

public sealed class PackageAssemblyItemsEconomyCraftingAcceptanceTests
{
    [Fact]
    public async Task BuildsDeterministicItemsEconomyCraftingArtifacts()
    {
        using var temp = new TempDirectory();
        CopyEvidenceArtifacts(FindRepoRoot(), temp.Path);
        var service = new PackageAssemblyItemsEconomyCraftingAcceptanceService();

        var first = await service.BuildAsync(temp.Path);
        var second = await service.BuildAsync(temp.Path);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.False(first.Report.Accepted);
        Assert.Equal(PackageAssemblyItemsEconomyCraftingAcceptanceService.FinalGate, first.Report.FinalStatus);
        Assert.Equal(PackageAssemblyItemsEconomyCraftingAcceptanceService.FinalGate, first.Report.ManualGate);
        Assert.Equal(PackageAssemblyItemsEconomyCraftingAcceptanceService.PreviousAcceptedGate, first.Report.PreviousAcceptedGate);
        Assert.True(first.Report.Goal026EvidenceVerified);
        Assert.True(first.Report.Goal025EvidenceVerified);
        Assert.True(first.Report.Goal024EvidenceVerified);
        Assert.True(first.Report.Goal023EvidenceVerified);
        Assert.True(first.Report.RealConsumerPassed);
        Assert.True(first.Report.SyntheticConsumerPassed);
        Assert.True(first.Report.AntiOverfitProofPassed);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.Equal(first.Report.PackageSummaryHash, second.Report.PackageSummaryHash);
        Assert.True(File.Exists(write.MappingContractProofJsonPath));
        Assert.True(File.Exists(write.InputFixturesJsonPath));
        Assert.True(File.Exists(write.AssemblyReportJsonPath));
        Assert.True(File.Exists(write.PackageSummaryJsonPath));
        Assert.True(File.Exists(write.AntiOverfitFixturesJsonPath));
        Assert.True(File.Exists(write.InvalidMatrixJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        Assert.True(File.Exists(write.ScopeReportJsonPath));
    }

    [Fact]
    public async Task RealAndSyntheticConsumersProduceEconomyPackageRecords()
    {
        var result = await new PackageAssemblyItemsEconomyCraftingAcceptanceService().BuildAsync(FindRepoRoot());

        Assert.Contains(result.PackageSummary.ConsumerSummaries, summary =>
            summary.ConsumerId == "goal027_real_consumer_trade_caravan"
            && summary.ItemCount >= 2
            && summary.ResourceCount >= 1
            && summary.RecipeCount >= 1
            && summary.LootTableCount >= 1
            && summary.TransactionCount >= 1
            && (summary.InventoryCount >= 1 || summary.EquipmentSlotCount >= 1));
        Assert.Contains(result.PackageSummary.ConsumerSummaries, summary =>
            summary.ConsumerId == "vendor_crafting_transaction"
            && summary.PrimaryItemId.Contains("finished", StringComparison.Ordinal)
            && summary.PrimaryRecipeId.Contains("finished", StringComparison.Ordinal)
            && summary.TransactionCount >= 1);
        Assert.NotEqual(
            result.PackageSummary.ConsumerSummaries[0].PrimaryRecipeId,
            result.PackageSummary.ConsumerSummaries[1].PrimaryRecipeId);
    }

    [Fact]
    public void AssemblerMapsItemsResourcesRecipesLootTransactionsInventoriesAndSlots()
    {
        var artifactSet = new GeneratorPlanApprovedArtifactSet
        {
            SchemaVersion = "0.1",
            SnapshotId = "snapshot/items-economy",
            SourceProductionBatchId = "batch/items-economy",
            ApprovedArtifacts =
            [
                Artifact("artifact/01-items", "item_pack_v1", """{"items":[{"id":"craft_hammer","name":"Craft Hammer","kind":"tool","tags":["tool"]},{"id":"iron_ingot","name":"Iron Ingot","kind":"material","tags":["metal"]}]}"""),
                Artifact("artifact/02-resources", "resource_pack_v1", """{"resources":[{"id":"forge_heat","name":"Forge Heat","kind":"abstract","min_value":0,"max_value":10}]}"""),
                Artifact("artifact/03-recipes", "recipe_pack_v1", """{"recipes":[{"id":"forge_ingot","name":"Forge Ingot","inputs":[{"kind":"item","id":"item/craft/hammer","amount":1},{"kind":"resource","id":"resource/forge/heat","amount":2}],"outputs":[{"kind":"item","id":"item/iron/ingot","amount":1}]}]}"""),
                Artifact("artifact/04-loot", "loot_pack_v1", """{"loot_tables":[{"id":"forge_stock","name":"Forge Stock","entries":[{"id":"hammer_stock","outputs":[{"kind":"item","id":"item/craft/hammer","amount":1}],"weight":1,"min_count":1,"max_count":1}]}]}"""),
                Artifact("artifact/05-transactions", "transaction_pack_v1", """{"transactions":[{"id":"buy_hammer","name":"Buy Hammer","stock_loot_table_id":"loot/forge/stock","costs":[{"kind":"resource","id":"resource/forge/heat","amount":1}],"outputs":[{"kind":"item","id":"item/craft/hammer","amount":1}]}]}"""),
                Artifact("artifact/06-inventory", "inventory_pack_v1", """{"inventories":[{"id":"forge_inventory","owner_kind":"station","owner_id":"entity/forge","slots":4,"stacks":[{"item_id":"item/iron/ingot","amount":2}]}]}"""),
                Artifact("artifact/07-equipment", "equipment_pack_v1", """{"equipment_slots":[{"id":"tool_hand","name":"Tool Hand","allowed_tags":["tool"],"allowed_kinds":["tool"]}]}""")
            ]
        };

        var assembled = new GeneratorPlanGamePackageAssembler().Assemble(artifactSet, DateTimeOffset.Parse("2026-06-28T00:00:00Z"));
        var validation = new GamePackageValidator().Validate(assembled.Package);

        Assert.True(validation.IsValid, string.Join(Environment.NewLine, validation.Issues.Select(issue => issue.ToString())));
        Assert.Contains(assembled.Package.Game.Items, item => item.Id == "item/craft/hammer");
        Assert.Contains(assembled.Package.Game.Resources, resource => resource.Id == "resource/forge/heat");
        Assert.Contains(assembled.Package.Game.Recipes, recipe => recipe.Id == "recipe/forge/ingot" && recipe.Inputs.Count == 2 && recipe.Outputs.Single().Id == "item/iron/ingot");
        Assert.Contains(assembled.Package.Game.LootTables, loot => loot.Id == "loot/forge/stock");
        Assert.Contains(assembled.Package.Game.Transactions, transaction => transaction.Id == "transaction/buy/hammer" && transaction.StockLootTableId == "loot/forge/stock");
        Assert.Contains(assembled.Package.Game.Inventories, inventory => inventory.Id == "inventory/forge/inventory" && inventory.Stacks.Single().ItemId == "item/iron/ingot");
        Assert.Contains(assembled.Package.Game.EquipmentSlots, slot => slot.Id == "equipment/tool/hand");
        Assert.Equal(2, assembled.Package.GeneratedContent.Items.Count);
    }

    [Fact]
    public async Task InvalidMatrixRejectsRequiredScenarios()
    {
        var result = await new PackageAssemblyItemsEconomyCraftingAcceptanceService().BuildAsync(FindRepoRoot());

        Assert.True(result.InvalidMatrix.Passed);
        Assert.Equal(18, result.InvalidMatrix.ScenarioCount);
        Assert.Equal(result.InvalidMatrix.ScenarioCount, result.InvalidMatrix.RejectedCount);
        Assert.All(result.InvalidMatrix.Scenarios, scenario => Assert.False(scenario.ActualValid));
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "missing_accepted_goal026_gate");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "recipe_output_references_unknown_item_or_resource");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "inventory_stack_references_unknown_item_id");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "goal028_or_s220_started_marker");
    }

    [Fact]
    public async Task RejectsMissingEvidenceAndAntiOverfitFailure()
    {
        var repoRoot = FindRepoRoot();
        var service = new PackageAssemblyItemsEconomyCraftingAcceptanceService();

        var missingGate = await service.BuildAsync(repoRoot, new PackageAssemblyItemsEconomyCraftingOptions { PreviousAcceptedGate = "package_assembly_dialogue_quests_expansion_verification required" });
        var missingGoal026 = await service.BuildAsync(repoRoot, new PackageAssemblyItemsEconomyCraftingOptions { MissingGoal026Evidence = true });
        var antiOverfit = await service.BuildAsync(repoRoot, new PackageAssemblyItemsEconomyCraftingOptions { SyntheticAntiOverfitFixtureMissing = true });

        Assert.False(missingGate.Report.ContractProofPassed);
        Assert.Contains(missingGate.Report.Diagnostics, item => item.Code == "package_items_economy_crafting.previous_gate.missing");
        Assert.False(missingGoal026.Report.ContractProofPassed);
        Assert.Contains(missingGoal026.Report.Diagnostics, item => item.Code == "package_items_economy_crafting.goal026_evidence.missing");
        Assert.False(antiOverfit.Report.AntiOverfitProofPassed);
    }

    [Fact]
    public async Task WrittenReportRoundTripsManualGateAndHasNoTopLevelErrors()
    {
        using var temp = new TempDirectory();
        CopyEvidenceArtifacts(FindRepoRoot(), temp.Path);
        var service = new PackageAssemblyItemsEconomyCraftingAcceptanceService();
        var result = await service.BuildAsync(temp.Path);
        var write = await service.WriteAsync(temp.Path, result);

        var report = JsonSerializer.Deserialize<PackageAssemblyItemsEconomyCraftingReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.Equal(PackageAssemblyItemsEconomyCraftingAcceptanceService.FinalGate, report.ManualGate);
        Assert.True(report.ContractProofPassed, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.DoesNotContain(report.Diagnostics, item => item.Severity == "error");
        Assert.True(report.InvalidMatrix.Passed);
    }

    [Fact]
    public void CurrentStatePreservesGoal027AcceptedBeforeLaterPackageAssemblyGoals()
    {
        var repoRoot = FindRepoRoot();
        using var state = JsonDocument.Parse(File.ReadAllText(Path.Combine(repoRoot, "docs", "CURRENT_GENERATOR_STATE.json")));
        var root = state.RootElement;

        Assert.Equal(
            "passed_by_user_prompt_before_goal_028",
            root.GetProperty("package_assembly_items_economy_crafting_expansion_verification").GetProperty("status").GetString());
    }

    private static GeneratorPlanApprovedArtifact Artifact(string id, string kind, string contentJson) =>
        new()
        {
            ArtifactId = id,
            ArtifactKind = kind,
            ExpectedArtifactContract = kind,
            ContentJson = contentJson
        };

    private static void CopyEvidenceArtifacts(string sourceRepoRoot, string targetRoot)
    {
        CopyArtifactFamily(sourceRepoRoot, targetRoot, ".llmgc", "procedural", "capability-bundle-pipeline-inputs");
        CopyArtifactFamily(sourceRepoRoot, targetRoot, ".llmgc", "procedural", "rich-package-assembly-coverage-audit");
        CopyArtifactFamily(sourceRepoRoot, targetRoot, ".llmgc", "procedural", "package-assembly-world-entities");
        CopyArtifactFamily(sourceRepoRoot, targetRoot, ".llmgc", "procedural", "package-assembly-dialogue-quests");
    }

    private static void CopyArtifactFamily(string sourceRepoRoot, string targetRoot, params string[] pathSegments)
    {
        var source = Path.Combine(new[] { sourceRepoRoot }.Concat(pathSegments).ToArray());
        var target = Path.Combine(new[] { targetRoot }.Concat(pathSegments).ToArray());
        Directory.CreateDirectory(target);
        foreach (var file in Directory.EnumerateFiles(source))
        {
            File.Copy(file, Path.Combine(target, Path.GetFileName(file)), overwrite: true);
        }
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
