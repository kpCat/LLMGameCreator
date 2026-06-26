using System.Text.Json;
using LLMGameCreator.Application.Design.AlphaBuild;
using LLMGameCreator.Application.Design.UnityGeneratedScene;
using LLMGameCreator.Application.Design.UnityQuestLoop;
using LLMGameCreator.Tests.Application.Assets;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnityQuestLoop;

[Collection("UnityAlphaProductSmoke")]
public sealed class UnityQuestCompletionLoopAcceptanceTests
{
    [Fact]
    public async Task BuildsDeterministicPlanStateAndReportArtifacts()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(repoRoot, "samples", "content-generation-packs"), temp.Path);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(temp.Path, Path.Combine(repoRoot, "samples", "minimum-asset-pipeline"), content);
        var service = new UnityQuestCompletionLoopAcceptanceService();

        var first = service.BuildFromAcceptedEvidence(
            temp.Path,
            content,
            assets,
            new UnityQuestCompletionLoopOptions { RepositoryRootPath = repoRoot });
        var second = service.BuildFromAcceptedEvidence(
            temp.Path,
            content,
            assets,
            new UnityQuestCompletionLoopOptions { RepositoryRootPath = repoRoot });
        var write = await service.WriteAsync(temp.Path, first);

        Assert.False(first.Report.Accepted);
        Assert.Equal(UnityQuestCompletionLoopAcceptanceService.FinalGate, first.Report.FinalStatus);
        Assert.Equal("unity_generated_runtime_state_loop_verification passed", first.Report.PreviousAcceptedGate);
        Assert.Equal(["S138", "S139", "S140", "S141", "S142", "S143", "S144", "S145"], first.Report.CompletedSlices);
        Assert.Equal("unity-quest-completion-loop", first.Report.ProductSmokeRoute);
        Assert.Equal("frontier_survival", first.Report.SelectedStyleId);
        Assert.Equal(first.Report.PlanHash, second.Report.PlanHash);
        Assert.Equal(first.Report.StateHash, second.Report.StateHash);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(first.Report.InvalidMatrix.Passed, string.Join(Environment.NewLine, first.Report.InvalidMatrix.Diagnostics.Select(item => item.Code)));
        Assert.True(first.Report.InvalidMatrix.ScenarioCount >= 22);
        Assert.False(first.Report.PublicGamePackageSchemaChanged);
        Assert.False(first.Report.ProjectFilesChanged);
        Assert.False(first.Report.GeneratorLibraryChanged);
        Assert.True(first.Report.NoExternalProviderLlmRagLuaMedia);
        Assert.True(File.Exists(write.PlanJsonPath));
        Assert.True(File.Exists(write.StateJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));

        var roundTrip = JsonSerializer.Deserialize<UnityQuestCompletionLoopReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(roundTrip);
        Assert.Equal(UnityQuestCompletionLoopAcceptanceService.FinalGate, roundTrip!.ManualGate);
    }

    [Fact]
    public void ParserAcceptsCompleteQuestLoopLines()
    {
        var (projection, plan) = BuildProjectionAndPlan();
        var lines = UnityQuestCompletionLoopAcceptanceService.BuildExpectedQuestLoopLines(projection, plan);

        var proof = UnityQuestCompletionLoopAcceptanceService.ValidateQuestLoopLines(lines, projection, plan);

        Assert.True(proof.QuestCompletionLoopVerified, string.Join(Environment.NewLine, proof.Diagnostics.Select(item => item.Code)));
        Assert.True(proof.QuestPlanVerified);
        Assert.True(proof.QuestPhaseTraceVerified);
        Assert.True(proof.ObjectiveChecklistVerified);
        Assert.True(proof.ObjectiveCommandCorrelationVerified);
        Assert.True(proof.QuestCompletedVerified);
        Assert.True(proof.RewardGrantedVerified);
        Assert.True(proof.RuntimeStateProof.RuntimeStateLoopVerified);
    }

    [Fact]
    public void ParserRejectsCompletionWithoutPhaseObjectiveAndRewardProof()
    {
        var (projection, plan) = BuildProjectionAndPlan();
        var lines = UnityQuestCompletionLoopAcceptanceService.BuildExpectedQuestLoopLines(projection, plan);

        var noPhase = UnityQuestCompletionLoopAcceptanceService.ValidateQuestLoopLines(
            lines.Where(line => !line.StartsWith("alpha_runtime.quest_phase.", StringComparison.Ordinal)),
            projection,
            plan);
        var noObjectives = UnityQuestCompletionLoopAcceptanceService.ValidateQuestLoopLines(
            lines.Where(line => !line.StartsWith("alpha_runtime.quest_objective.", StringComparison.Ordinal)),
            projection,
            plan);
        var noReward = UnityQuestCompletionLoopAcceptanceService.ValidateQuestLoopLines(
            lines.Select(line => line == "alpha_runtime.reward_granted.after=true" ? "alpha_runtime.reward_granted.after=false" : line),
            projection,
            plan);

        Assert.False(noPhase.QuestCompletionLoopVerified);
        Assert.False(noObjectives.QuestCompletionLoopVerified);
        Assert.False(noReward.QuestCompletionLoopVerified);
    }

    [Fact]
    public void ParserRejectsObjectiveCommandCorrelationMismatches()
    {
        var (projection, plan) = BuildProjectionAndPlan();
        var lines = UnityQuestCompletionLoopAcceptanceService.BuildExpectedQuestLoopLines(projection, plan);

        var wrongId = UnityQuestCompletionLoopAcceptanceService.ValidateQuestLoopLines(
            lines.Select(line => line.StartsWith("alpha_runtime.quest_objective.0.required_command_id=", StringComparison.Ordinal)
                ? "alpha_runtime.quest_objective.0.required_command_id=cmd/mismatch"
                : line),
            projection,
            plan);
        var wrongType = UnityQuestCompletionLoopAcceptanceService.ValidateQuestLoopLines(
            lines.Select(line => line.StartsWith("alpha_runtime.quest_objective.0.required_command_type=", StringComparison.Ordinal)
                ? "alpha_runtime.quest_objective.0.required_command_type=dialogue/open"
                : line),
            projection,
            plan);
        var wrongTarget = UnityQuestCompletionLoopAcceptanceService.ValidateQuestLoopLines(
            lines.Select(line => line.StartsWith("alpha_runtime.quest_objective.0.required_target_id=", StringComparison.Ordinal)
                ? "alpha_runtime.quest_objective.0.required_target_id=quest/mismatch"
                : line),
            projection,
            plan);

        Assert.False(wrongId.QuestCompletionLoopVerified);
        Assert.False(wrongType.QuestCompletionLoopVerified);
        Assert.False(wrongTarget.QuestCompletionLoopVerified);
    }

    [Fact]
    public void InvalidMatrixScenariosAreCausalAndRejected()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(repoRoot, "samples", "content-generation-packs"), temp.Path);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(temp.Path, Path.Combine(repoRoot, "samples", "minimum-asset-pipeline"), content);
        var service = new UnityQuestCompletionLoopAcceptanceService();

        var result = service.BuildFromAcceptedEvidence(
            temp.Path,
            content,
            assets,
            new UnityQuestCompletionLoopOptions { RepositoryRootPath = repoRoot });

        Assert.True(result.Report.InvalidMatrix.Passed, string.Join(Environment.NewLine, result.Report.InvalidMatrix.Diagnostics.Select(item => item.Code)));
        Assert.True(result.Report.InvalidMatrix.Scenarios.All(item => !item.ActualValid));
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "completion_claimed_without_phase_trace");
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "objective_step_command_id_mismatch");
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "state_leak_from_previous_run");
    }

    [Fact]
    public void PreviousGoal016EvidenceMustBePresentAndMatching()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(repoRoot, "samples", "content-generation-packs"), temp.Path);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(temp.Path, Path.Combine(repoRoot, "samples", "minimum-asset-pipeline"), content);
        var service = new UnityQuestCompletionLoopAcceptanceService();

        var result = service.BuildFromAcceptedEvidence(
            temp.Path,
            content,
            assets,
            new UnityQuestCompletionLoopOptions { RepositoryRootPath = repoRoot });

        Assert.True(result.Report.RuntimeStateLoopEvidenceVerified || result.Report.Diagnostics.All(item => item.Code != "unity_quest_loop.previous.runtime_report_missing"));
        Assert.Equal("unity_generated_runtime_state_loop_verification passed", result.Report.PreviousAcceptedGate);
        Assert.Equal(UnityQuestCompletionLoopAcceptanceService.FinalGate, result.Report.ManualGate);
    }

    private static (UnityGeneratedSceneProjection Projection, UnityQuestCompletionPlan Plan) BuildProjectionAndPlan()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(repoRoot, "samples", "content-generation-packs"), temp.Path);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(temp.Path, Path.Combine(repoRoot, "samples", "minimum-asset-pipeline"), content);
        var alpha = new AlphaRunnableBuildAcceptanceService()
            .BuildFromAcceptedEvidence(
                temp.Path,
                content,
                assets,
                new AlphaRunnableBuildOptions
                {
                    RepositoryRootPath = repoRoot,
                    RelativeOutputDirectoryOverride = UnityQuestCompletionLoopAcceptanceService.RelativeOutputDirectory
                })
            .Report;
        var projection = UnityGeneratedSceneProjectionAcceptanceService.BuildProjection(alpha);
        return (projection, UnityQuestCompletionLoopAcceptanceService.BuildPlan(projection));
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
