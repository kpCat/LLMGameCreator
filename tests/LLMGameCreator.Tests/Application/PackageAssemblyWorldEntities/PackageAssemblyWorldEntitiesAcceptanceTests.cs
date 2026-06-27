using System.Text.Json;
using LLMGameCreator.Application.Design.PackageAssemblyWorldEntities;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Validation;
using Xunit;

namespace LLMGameCreator.Tests.Application.PackageAssemblyWorldEntities;

public sealed class PackageAssemblyWorldEntitiesAcceptanceTests
{
    [Fact]
    public async Task BuildsDeterministicWorldEntityArtifacts()
    {
        using var temp = new TempDirectory();
        CopyEvidenceArtifacts(FindRepoRoot(), temp.Path);
        var service = new PackageAssemblyWorldEntitiesAcceptanceService();

        var first = await service.BuildAsync(temp.Path);
        var second = await service.BuildAsync(temp.Path);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.False(first.Report.Accepted);
        Assert.Equal(PackageAssemblyWorldEntitiesAcceptanceService.FinalGate, first.Report.FinalStatus);
        Assert.Equal(PackageAssemblyWorldEntitiesAcceptanceService.FinalGate, first.Report.ManualGate);
        Assert.Equal(PackageAssemblyWorldEntitiesAcceptanceService.PreviousAcceptedGate, first.Report.PreviousAcceptedGate);
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
    public async Task RealAndSyntheticConsumersProduceMapsPrototypesAndPlacements()
    {
        var result = await new PackageAssemblyWorldEntitiesAcceptanceService().BuildAsync(FindRepoRoot());

        Assert.Contains(result.PackageSummary.ConsumerSummaries, summary =>
            summary.ConsumerId == "goal025_real_consumer_trade_caravan"
            && summary.MapCount >= 2
            && summary.EntityPrototypeCount >= 3
            && summary.MapPlacementCount >= 2);
        Assert.Contains(result.PackageSummary.ConsumerSummaries, summary =>
            summary.ConsumerId == "npc_city_walk"
            && summary.PrimaryMapId.Contains("arcade", StringComparison.Ordinal)
            && summary.PrimaryEntityPrototypeId.Contains("city", StringComparison.Ordinal)
            && summary.MapPlacementCount >= 2);
        Assert.NotEqual(
            result.PackageSummary.ConsumerSummaries[0].PrimaryMapId,
            result.PackageSummary.ConsumerSummaries[1].PrimaryMapId);
    }

    [Fact]
    public async Task AssemblerMapsExplicitEntityAndNpcPlacementsWithoutSchemaChanges()
    {
        var artifactSet = new GeneratorPlanApprovedArtifactSet
        {
            SchemaVersion = "0.1",
            SnapshotId = "snapshot/placement",
            SourceProductionBatchId = "batch/placement",
            ApprovedArtifacts =
            [
                Artifact("artifact/01-scene", "scene_pack_v1", """{"scenes":[{"id":"scene/start","title":"Start"},{"id":"scene/plaza","title":"Plaza"}]}"""),
                Artifact("artifact/02-entity", "entity_pack_v1", """{"entities":[{"id":"entity/vendor","kind":"npc","title":"Vendor","scene_id":"scene/plaza","position":{"x":2,"y":2},"instance_id":"entity/instance/vendor"}]}"""),
                Artifact("artifact/03-npc", "npc_pack_v1", """{"npcs":[{"id":"npc/courier","name":"Courier","scene_id":"scene/plaza","position":{"x":3,"y":2},"instance_id":"entity/instance/courier"}]}""")
            ]
        };

        var assembled = new GeneratorPlanGamePackageAssembler().Assemble(artifactSet, DateTimeOffset.Parse("2026-06-28T00:00:00Z"));
        var validation = new GamePackageValidator().Validate(assembled.Package);
        var plaza = assembled.Package.Game.Maps.Single(map => map.Id == "map/draft/scene/plaza");

        Assert.True(validation.IsValid, string.Join(Environment.NewLine, validation.Issues.Select(issue => issue.ToString())));
        Assert.Contains(assembled.Package.Game.EntityPrototypes, prototype => prototype.Id == "entity/vendor");
        Assert.Contains(assembled.Package.Game.EntityPrototypes, prototype => prototype.Id == "entity/npc/courier");
        Assert.Contains(plaza.Entities, entity => entity.Id == "entity/instance/vendor" && entity.PrototypeId == "entity/vendor");
        Assert.Contains(plaza.Entities, entity => entity.Id == "entity/instance/courier" && entity.PrototypeId == "entity/npc/courier");
    }

    [Fact]
    public async Task InvalidMatrixRejectsRequiredScenarios()
    {
        var result = await new PackageAssemblyWorldEntitiesAcceptanceService().BuildAsync(FindRepoRoot());

        Assert.True(result.InvalidMatrix.Passed);
        Assert.Equal(15, result.InvalidMatrix.ScenarioCount);
        Assert.Equal(result.InvalidMatrix.ScenarioCount, result.InvalidMatrix.RejectedCount);
        Assert.All(result.InvalidMatrix.Scenarios, scenario => Assert.False(scenario.ActualValid));
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "missing_accepted_modular_policy_gate");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "entity_placement_unknown_map");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "entity_placement_unknown_prototype");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "out_of_bounds_map_placement");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "goal026_or_s206_started_marker");
    }

    [Fact]
    public async Task RejectsMissingEvidenceAndAntiOverfitFailure()
    {
        var repoRoot = FindRepoRoot();
        var service = new PackageAssemblyWorldEntitiesAcceptanceService();

        var missingGate = await service.BuildAsync(repoRoot, new PackageAssemblyWorldEntitiesOptions { PreviousAcceptedGate = "modular_contract_goal_policy_adoption_verification required" });
        var missingGoal024 = await service.BuildAsync(repoRoot, new PackageAssemblyWorldEntitiesOptions { MissingGoal024CoverageAuditEvidence = true });
        var antiOverfit = await service.BuildAsync(repoRoot, new PackageAssemblyWorldEntitiesOptions { SyntheticAntiOverfitFixtureMissing = true });

        Assert.False(missingGate.Report.ContractProofPassed);
        Assert.Contains(missingGate.Report.Diagnostics, item => item.Code == "package_world_entities.previous_gate.missing");
        Assert.False(missingGoal024.Report.ContractProofPassed);
        Assert.Contains(missingGoal024.Report.Diagnostics, item => item.Code == "package_world_entities.goal024_evidence.missing");
        Assert.False(antiOverfit.Report.AntiOverfitProofPassed);
    }

    [Fact]
    public async Task WrittenReportRoundTripsManualGateAndHasNoTopLevelErrors()
    {
        using var temp = new TempDirectory();
        CopyEvidenceArtifacts(FindRepoRoot(), temp.Path);
        var service = new PackageAssemblyWorldEntitiesAcceptanceService();
        var result = await service.BuildAsync(temp.Path);
        var write = await service.WriteAsync(temp.Path, result);

        var report = JsonSerializer.Deserialize<PackageAssemblyWorldEntitiesReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.Equal(PackageAssemblyWorldEntitiesAcceptanceService.FinalGate, report.ManualGate);
        Assert.True(report.ContractProofPassed, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.DoesNotContain(report.Diagnostics, item => item.Severity == "error");
        Assert.True(report.InvalidMatrix.Passed);
    }

    [Fact]
    public void CurrentStateRecordsModularGateAcceptedBeforeGoal025()
    {
        var repoRoot = FindRepoRoot();
        using var state = JsonDocument.Parse(File.ReadAllText(Path.Combine(repoRoot, "docs", "CURRENT_GENERATOR_STATE.json")));
        var root = state.RootElement;

        Assert.Equal("goal_025_package_assembly_expansion_1_world_and_entities", root.GetProperty("last_completed_product_slice_id").GetString());
        Assert.Equal(PackageAssemblyWorldEntitiesAcceptanceService.FinalGate, root.GetProperty("gate_status").GetString());
        Assert.Contains(PackageAssemblyWorldEntitiesAcceptanceService.PreviousAcceptedGate, root.GetProperty("recommended_next_decision").GetString());
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
