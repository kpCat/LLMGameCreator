using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Design.PackageAssemblyDialogueQuests;
using LLMGameCreator.Application.Validation;
using Xunit;

namespace LLMGameCreator.Tests.Application.PackageAssemblyDialogueQuests;

public sealed class PackageAssemblyDialogueQuestsAcceptanceTests
{
    [Fact]
    public async Task BuildsDeterministicDialogueQuestArtifacts()
    {
        using var temp = new TempDirectory();
        CopyEvidenceArtifacts(FindRepoRoot(), temp.Path);
        var service = new PackageAssemblyDialogueQuestsAcceptanceService();

        var first = await service.BuildAsync(temp.Path);
        var second = await service.BuildAsync(temp.Path);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.False(first.Report.Accepted);
        Assert.Equal(PackageAssemblyDialogueQuestsAcceptanceService.FinalGate, first.Report.FinalStatus);
        Assert.Equal(PackageAssemblyDialogueQuestsAcceptanceService.FinalGate, first.Report.ManualGate);
        Assert.Equal(PackageAssemblyDialogueQuestsAcceptanceService.PreviousAcceptedGate, first.Report.PreviousAcceptedGate);
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
    public async Task RealAndSyntheticConsumersProduceQuestStagesDialogueNodesAndChoices()
    {
        var result = await new PackageAssemblyDialogueQuestsAcceptanceService().BuildAsync(FindRepoRoot());

        Assert.Contains(result.PackageSummary.ConsumerSummaries, summary =>
            summary.ConsumerId == "goal026_real_consumer_gothic_mystery"
            && summary.QuestCount >= 1
            && summary.QuestStageCount >= 2
            && summary.QuestObjectiveCount >= 2
            && summary.DialogueCount >= 1
            && summary.DialogueNodeCount >= 2
            && summary.DialogueChoiceCount >= 2
            && summary.QuestLinkedChoiceCount >= 2);
        Assert.Contains(result.PackageSummary.ConsumerSummaries, summary =>
            summary.ConsumerId == "rumor_board_tutorial"
            && summary.PrimaryQuestId.Contains("rumor", StringComparison.Ordinal)
            && summary.PrimaryDialogueId.Contains("rumor", StringComparison.Ordinal)
            && summary.DialogueChoiceCount >= 1);
        Assert.NotEqual(
            result.PackageSummary.ConsumerSummaries[0].PrimaryDialogueId,
            result.PackageSummary.ConsumerSummaries[1].PrimaryDialogueId);
    }

    [Fact]
    public void AssemblerMapsQuestStagesDialogueNodesAndQuestLinkedChoicesWithoutSchemaChanges()
    {
        var artifactSet = new GeneratorPlanApprovedArtifactSet
        {
            SchemaVersion = "0.1",
            SnapshotId = "snapshot/dialogue-quest",
            SourceProductionBatchId = "batch/dialogue-quest",
            ApprovedArtifacts =
            [
                Artifact("artifact/01-quest", "quest_pack_v1", """
                {
                  "quests": [
                    {
                      "id": "intro_thread",
                      "title": "Intro Thread",
                      "objectives": [{ "id": "speak", "kind": "choose_dialogue", "target_id": "dialogue/intro/guide", "required_amount": 1 }],
                      "stages": [
                        { "id": "start", "text": "Start.", "next_stage_id": "done", "objectives": [{ "id": "ask", "kind": "choose_dialogue", "target_id": "dialogue/intro/guide", "required_amount": 1 }] },
                        { "id": "done", "text": "Done." }
                      ]
                    }
                  ]
                }
                """),
                Artifact("artifact/02-dialogue", "dialogue_pack_v1", """
                {
                  "dialogues": [
                    {
                      "id": "intro_guide",
                      "title": "Intro Guide",
                      "start_node_id": "start",
                      "nodes": [
                        { "id": "start", "text": "Ready?", "choices": [{ "id": "accept", "text": "Yes", "target_node_id": "end", "start_quest_id": "quest/intro/thread" }] },
                        { "id": "end", "text": "Go.", "choices": [{ "id": "close", "text": "Close", "close_dialogue": true, "advance_quest_id": "quest/intro/thread", "set_quest_stage_id": "done" }] }
                      ]
                    }
                  ]
                }
                """)
            ]
        };

        var assembled = new GeneratorPlanGamePackageAssembler().Assemble(artifactSet, DateTimeOffset.Parse("2026-06-28T00:00:00Z"));
        var validation = new GamePackageValidator().Validate(assembled.Package);
        var quest = Assert.Single(assembled.Package.Game.Quests);
        var dialogue = Assert.Single(assembled.Package.Game.Dialogues);

        Assert.True(validation.IsValid, string.Join(Environment.NewLine, validation.Issues.Select(issue => issue.ToString())));
        Assert.Equal("quest/intro/thread", quest.Id);
        Assert.Equal(2, quest.Stages.Count);
        Assert.Contains(quest.Objectives, objective => objective.TargetId == "dialogue/intro/guide");
        Assert.Equal("dialogue/intro/guide", dialogue.Id);
        Assert.Equal("start", dialogue.StartNodeId);
        Assert.Contains(dialogue.Nodes.SelectMany(node => node.Choices), choice => choice.StartQuestId == quest.Id);
        Assert.Contains(dialogue.Nodes.SelectMany(node => node.Choices), choice => choice.AdvanceQuestId == quest.Id && choice.SetQuestStageId == "stage/done");
        Assert.Single(assembled.Package.GeneratedContent.Dialogues);
        Assert.Single(assembled.Package.GeneratedContent.Quests);
    }

    [Fact]
    public async Task InvalidMatrixRejectsRequiredScenarios()
    {
        var result = await new PackageAssemblyDialogueQuestsAcceptanceService().BuildAsync(FindRepoRoot());

        Assert.True(result.InvalidMatrix.Passed);
        Assert.Equal(16, result.InvalidMatrix.ScenarioCount);
        Assert.Equal(result.InvalidMatrix.ScenarioCount, result.InvalidMatrix.RejectedCount);
        Assert.All(result.InvalidMatrix.Scenarios, scenario => Assert.False(scenario.ActualValid));
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "missing_accepted_goal025_gate");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "dialogue_choice_references_unknown_quest_id");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "quest_stage_references_unknown_next_stage");
        Assert.Contains(result.InvalidMatrix.Scenarios, scenario => scenario.ScenarioId == "goal027_or_s213_started_marker");
    }

    [Fact]
    public async Task RejectsMissingEvidenceAndAntiOverfitFailure()
    {
        var repoRoot = FindRepoRoot();
        var service = new PackageAssemblyDialogueQuestsAcceptanceService();

        var missingGate = await service.BuildAsync(repoRoot, new PackageAssemblyDialogueQuestsOptions { PreviousAcceptedGate = "package_assembly_world_entities_expansion_verification required" });
        var missingGoal025 = await service.BuildAsync(repoRoot, new PackageAssemblyDialogueQuestsOptions { MissingGoal025Evidence = true });
        var antiOverfit = await service.BuildAsync(repoRoot, new PackageAssemblyDialogueQuestsOptions { SyntheticAntiOverfitFixtureMissing = true });

        Assert.False(missingGate.Report.ContractProofPassed);
        Assert.Contains(missingGate.Report.Diagnostics, item => item.Code == "package_dialogue_quests.previous_gate.missing");
        Assert.False(missingGoal025.Report.ContractProofPassed);
        Assert.Contains(missingGoal025.Report.Diagnostics, item => item.Code == "package_dialogue_quests.goal025_evidence.missing");
        Assert.False(antiOverfit.Report.AntiOverfitProofPassed);
    }

    [Fact]
    public async Task WrittenReportRoundTripsManualGateAndHasNoTopLevelErrors()
    {
        using var temp = new TempDirectory();
        CopyEvidenceArtifacts(FindRepoRoot(), temp.Path);
        var service = new PackageAssemblyDialogueQuestsAcceptanceService();
        var result = await service.BuildAsync(temp.Path);
        var write = await service.WriteAsync(temp.Path, result);

        var report = JsonSerializer.Deserialize<PackageAssemblyDialogueQuestsReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.Equal(PackageAssemblyDialogueQuestsAcceptanceService.FinalGate, report.ManualGate);
        Assert.True(report.ContractProofPassed, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.DoesNotContain(report.Diagnostics, item => item.Severity == "error");
        Assert.True(report.InvalidMatrix.Passed);
    }

    [Fact]
    public void CurrentStateRecordsGoal025AcceptedBeforeGoal026()
    {
        var repoRoot = FindRepoRoot();
        using var state = JsonDocument.Parse(File.ReadAllText(Path.Combine(repoRoot, "docs", "CURRENT_GENERATOR_STATE.json")));
        var root = state.RootElement;

        Assert.Equal("goal_026_package_assembly_expansion_2_dialogue_and_quests", root.GetProperty("last_completed_product_slice_id").GetString());
        Assert.Equal(PackageAssemblyDialogueQuestsAcceptanceService.FinalGate, root.GetProperty("gate_status").GetString());
        Assert.Contains(PackageAssemblyDialogueQuestsAcceptanceService.PreviousAcceptedGate, root.GetProperty("recommended_next_decision").GetString());
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
