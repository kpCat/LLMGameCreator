using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Design.PackageAssemblyCombatProgression;
using LLMGameCreator.Application.Validation;
using Xunit;

namespace LLMGameCreator.Tests.Application.PackageAssemblyCombatProgression;

public sealed class PackageAssemblyCombatProgressionAcceptanceTests
{
    [Fact]
    public async Task BuildsDeterministicCombatProgressionArtifacts()
    {
        using var temp = new TempDirectory();
        CopyEvidenceArtifacts(FindRepoRoot(), temp.Path);
        var service = new PackageAssemblyCombatProgressionAcceptanceService();

        var first = await service.BuildAsync(temp.Path);
        var second = await service.BuildAsync(temp.Path);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.False(first.Report.Accepted);
        Assert.Equal(PackageAssemblyCombatProgressionAcceptanceService.FinalGate, first.Report.FinalStatus);
        Assert.Equal(PackageAssemblyCombatProgressionAcceptanceService.FinalGate, first.Report.ManualGate);
        Assert.Equal(PackageAssemblyCombatProgressionAcceptanceService.PreviousAcceptedGate, first.Report.PreviousAcceptedGate);
        Assert.True(first.Report.Goal027EvidenceVerified);
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
    public async Task RealAndSyntheticConsumersProduceCombatProgressionPackageRecords()
    {
        var result = await new PackageAssemblyCombatProgressionAcceptanceService().BuildAsync(FindRepoRoot());

        Assert.Contains(result.PackageSummary.ConsumerSummaries, summary =>
            summary.ConsumerId == "goal028_real_consumer_frontier_survival"
            && summary.StatCount >= 1
            && summary.AbilityCount >= 1
            && summary.StatusCount >= 1
            && summary.ProgressionStageCount >= 1
            && summary.EncounterParticipantCount >= 1
            && summary.EncounterActionCount >= 1);
        Assert.Contains(result.PackageSummary.ConsumerSummaries, summary =>
            summary.ConsumerId == "alternate_encounter_status_progression"
            && summary.PrimaryEncounterId.Contains("precision", StringComparison.Ordinal)
            && summary.PrimaryProgressionId.Contains("precision", StringComparison.Ordinal));
        Assert.NotEqual(
            result.PackageSummary.ConsumerSummaries[0].PrimaryEncounterId,
            result.PackageSummary.ConsumerSummaries[1].PrimaryEncounterId);
    }

    [Fact]
    public void AssemblerMapsStatsAbilitiesStatusesProgressionsAndEncounters()
    {
        var artifactSet = new GeneratorPlanApprovedArtifactSet
        {
            SchemaVersion = "0.1",
            SnapshotId = "snapshot/combat-progression",
            SourceProductionBatchId = "batch/combat-progression",
            ApprovedArtifacts =
            [
                Artifact("artifact/01-entity", "entity_pack_v1", """{"entities":[{"id":"training_dummy","title":"Training Dummy","kind":"target"}]}"""),
                Artifact("artifact/02-resource", "resource_pack_v1", """{"resources":[{"id":"focus","name":"Focus","kind":"combat","min_value":0,"max_value":8}]}"""),
                Artifact("artifact/03-loot", "loot_pack_v1", """{"loot_tables":[{"id":"training_reward","name":"Training Reward","entries":[{"id":"reward","outputs":[{"kind":"resource","id":"resource/focus","amount":1}],"weight":1}]}]}"""),
                Artifact("artifact/04-stat", "stat_pack_v1", """{"stats":[{"id":"precision","name":"Precision","kind":"attribute","default_value":1,"min_value":0,"max_value":10}]}"""),
                Artifact("artifact/05-status", "status_pack_v1", """{"statuses":[{"id":"guarded","name":"Guarded","kind":"stance"}]}"""),
                Artifact("artifact/06-ability", "ability_pack_v1", """{"abilities":[{"id":"focus_shot","name":"Focus Shot","resource_id":"resource/focus","costs":[{"kind":"resource","id":"resource/focus","amount":1}],"effects":[{"type":"add_status","status_id":"status/guarded","amount":1}]}]}"""),
                Artifact("artifact/07-progression", "progression_pack_v1", """{"progressions":[{"id":"precision_drill","name":"Precision Drill","stages":[{"id":"steady","name":"Steady","required_amount":1,"outputs":[{"kind":"status","id":"status/guarded","amount":1}]}]}]}"""),
                Artifact("artifact/08-encounter", "encounter_pack_v1", """{"encounters":[{"id":"precision_drill_test","title":"Precision Drill Test","loot_table_id":"loot/training/reward","participants":[{"id":"target/training_dummy","name":"Training Dummy","entity_prototype_id":"entity/training/dummy","stats":[{"kind":"stat","id":"stat/precision","amount":1}],"resources":[{"kind":"resource","id":"resource/focus","amount":3}],"abilities":["ability/focus/shot"]}],"actions":[{"id":"action/focus_shot","name":"Focus Shot","ability_id":"ability/focus/shot","outputs":[{"kind":"status","id":"status/guarded","amount":1}]}]}]}""")
            ]
        };

        var assembled = new GeneratorPlanGamePackageAssembler().Assemble(artifactSet, DateTimeOffset.Parse("2026-06-28T00:00:00Z"));
        var validation = new GamePackageValidator().Validate(assembled.Package);

        Assert.True(validation.IsValid, string.Join(Environment.NewLine, validation.Issues.Select(issue => issue.ToString())));
        Assert.Contains(assembled.Package.Game.Stats, stat => stat.Id == "stat/precision");
        Assert.Contains(assembled.Package.Game.Abilities, ability => ability.Id == "ability/focus/shot" && ability.ResourceId == "resource/focus");
        Assert.Contains(assembled.Package.Game.Statuses, status => status.Id == "status/guarded");
        Assert.Contains(assembled.Package.Game.Progressions, progression => progression.Id == "progression/precision/drill" && progression.Stages.Single().Id == "stage/steady");
        Assert.Contains(assembled.Package.Game.Encounters, encounter => encounter.Id == "encounter/precision/drill/test" && encounter.Participants.Count == 1 && encounter.Actions.Count == 1);
        Assert.Single(assembled.Package.GeneratedContent.Encounters);
        Assert.Single(assembled.Package.GeneratedContent.Mechanics);
    }

    [Fact]
    public async Task InvalidMatrixRejectsRequiredScenarios()
    {
        var result = await new PackageAssemblyCombatProgressionAcceptanceService().BuildAsync(FindRepoRoot());

        Assert.True(result.InvalidMatrix.Passed);
        Assert.Equal(20, result.InvalidMatrix.ScenarioCount);
        Assert.Equal(result.InvalidMatrix.ScenarioCount, result.InvalidMatrix.RejectedCount);
        Assert.All(result.InvalidMatrix.Scenarios, scenario => Assert.False(scenario.ActualValid));
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "missing_accepted_goal027_gate");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "ability_references_unknown_resource_id");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "encounter_participant_or_action_unknown_ability_id");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "goal029_or_s227_started_marker");
    }

    [Fact]
    public async Task RejectsMissingEvidenceAndAntiOverfitFailure()
    {
        var repoRoot = FindRepoRoot();
        var service = new PackageAssemblyCombatProgressionAcceptanceService();

        var missingGate = await service.BuildAsync(repoRoot, new PackageAssemblyCombatProgressionOptions { PreviousAcceptedGate = "package_assembly_items_economy_crafting_expansion_verification required" });
        var missingGoal027 = await service.BuildAsync(repoRoot, new PackageAssemblyCombatProgressionOptions { MissingGoal027Evidence = true });
        var antiOverfit = await service.BuildAsync(repoRoot, new PackageAssemblyCombatProgressionOptions { SyntheticAntiOverfitFixtureMissing = true });

        Assert.False(missingGate.Report.ContractProofPassed);
        Assert.Contains(missingGate.Report.Diagnostics, item => item.Code == "package_combat_progression.previous_gate.missing");
        Assert.False(missingGoal027.Report.ContractProofPassed);
        Assert.Contains(missingGoal027.Report.Diagnostics, item => item.Code == "package_combat_progression.goal027_evidence.missing");
        Assert.False(antiOverfit.Report.AntiOverfitProofPassed);
    }

    [Fact]
    public async Task WrittenReportRoundTripsManualGateAndHasNoTopLevelErrors()
    {
        using var temp = new TempDirectory();
        CopyEvidenceArtifacts(FindRepoRoot(), temp.Path);
        var service = new PackageAssemblyCombatProgressionAcceptanceService();
        var result = await service.BuildAsync(temp.Path);
        var write = await service.WriteAsync(temp.Path, result);

        var report = JsonSerializer.Deserialize<PackageAssemblyCombatProgressionReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.Equal(PackageAssemblyCombatProgressionAcceptanceService.FinalGate, report.ManualGate);
        Assert.True(report.ContractProofPassed, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.DoesNotContain(report.Diagnostics, item => item.Severity == "error");
        Assert.True(report.InvalidMatrix.Passed);
    }

    [Fact]
    public void CurrentStateRecordsGoal027AcceptedBeforeGoal028()
    {
        var repoRoot = FindRepoRoot();
        using var state = JsonDocument.Parse(File.ReadAllText(Path.Combine(repoRoot, "docs", "CURRENT_GENERATOR_STATE.json")));
        var root = state.RootElement;

        Assert.Equal("goal_028_package_assembly_expansion_4_combat_progression", root.GetProperty("last_completed_product_slice_id").GetString());
        Assert.Equal(PackageAssemblyCombatProgressionAcceptanceService.FinalGate, root.GetProperty("gate_status").GetString());
        Assert.Contains(PackageAssemblyCombatProgressionAcceptanceService.PreviousAcceptedGate, root.GetProperty("recommended_next_decision").GetString());
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
        CopyArtifactFamily(sourceRepoRoot, targetRoot, ".llmgc", "procedural", "package-assembly-items-economy-crafting");
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
