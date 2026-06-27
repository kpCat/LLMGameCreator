using System.Text.Json;
using LLMGameCreator.Application.Design.GameProfiles;
using Xunit;

namespace LLMGameCreator.Tests.Application.GameProfiles;

public sealed class GeneratedGameProfileContractAcceptanceTests
{
    [Fact]
    public async Task BuildsDeterministicArtifactsForThreeProfiles()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var profileDirectory = Path.Combine(repoRoot, "samples", "game-profiles");
        var service = new GeneratedGameProfileContractAcceptanceService();

        var first = service.BuildFromProfileDirectory(profileDirectory, temp.Path);
        var second = service.BuildFromProfileDirectory(profileDirectory, temp.Path);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.False(first.Report.Accepted);
        Assert.Equal(GeneratedGameProfileContractAcceptanceService.FinalGate, first.Report.FinalStatus);
        Assert.Equal(GeneratedGameProfileContractAcceptanceService.FinalGate, first.Report.ManualGate);
        Assert.Equal(GeneratedGameProfileContractAcceptanceService.PreviousAcceptedGate, first.Report.PreviousAcceptedGate);
        Assert.Equal(["S170", "S171", "S172", "S173", "S174", "S175", "S176", "S177"], first.Report.CompletedSlices);
        Assert.Equal("generated-game-profile-contract", first.Report.ProductSmokeRoute);
        Assert.Equal(3, first.Report.ValidProfileCount);
        Assert.Equal(3, first.Report.PipelinePlanCount);
        Assert.Equal(first.Report.ProfileArtifactHash, second.Report.ProfileArtifactHash);
        Assert.Equal(first.Report.PipelinePlanHash, second.Report.PipelinePlanHash);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(File.Exists(write.ProfilesJsonPath));
        Assert.True(File.Exists(write.PipelinePlanJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
    }

    [Fact]
    public void MapsEachProfileToExactPipelineStagesAndCapabilities()
    {
        var repoRoot = FindRepoRoot();
        var result = new GeneratedGameProfileContractAcceptanceService()
            .BuildFromProfileDirectory(Path.Combine(repoRoot, "samples", "game-profiles"), repoRoot);

        Assert.True(result.Report.ContractProofPassed, string.Join(Environment.NewLine, result.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.True(result.Report.AllPlansHaveExactStageIds);
        Assert.Contains("game_profile/frontier-survival-minimum-alpha", result.Report.ValidProfileIds);
        Assert.Contains("game_profile/gothic-mystery-investigation-alpha", result.Report.ValidProfileIds);
        Assert.Contains("game_profile/trade-caravan-social-economy-alpha", result.Report.ValidProfileIds);

        Assert.All(result.PipelinePlanArtifact.Plans, plan =>
        {
            Assert.Contains("stage/content_generation_scale_goal_010", plan.RequiredStageIds);
            Assert.Contains("stage/minimum_asset_pipeline_goal_011", plan.RequiredStageIds);
            Assert.Contains("stage/unity_generated_quest_completion_loop_goal_017", plan.RequiredStageIds);
            Assert.Contains("stage/unity_alpha_readable_presentation_goal_019", plan.RequiredStageIds);
            Assert.Contains("stage/minimum_playable_generated_game_goal_020", plan.RequiredStageIds);
            Assert.False(string.IsNullOrWhiteSpace(plan.ContentGenerationPackId));
            Assert.False(string.IsNullOrWhiteSpace(plan.UnityExportTargetId));
            Assert.False(plan.UnsupportedCapabilitiesTreatedAsComplete);
        });
    }

    [Fact]
    public void FutureRequiredCapabilitiesAreNotMarkedSupported()
    {
        var repoRoot = FindRepoRoot();
        var result = new GeneratedGameProfileContractAcceptanceService()
            .BuildFromProfileDirectory(Path.Combine(repoRoot, "samples", "game-profiles"), repoRoot);

        var gothic = Assert.Single(result.PipelinePlanArtifact.Plans, plan => plan.ProfileId == "game_profile/gothic-mystery-investigation-alpha");
        var trade = Assert.Single(result.PipelinePlanArtifact.Plans, plan => plan.ProfileId == "game_profile/trade-caravan-social-economy-alpha");

        Assert.Contains("capability/dialogue_clue_graph_future", gothic.FutureRequiredCapabilities);
        Assert.DoesNotContain("capability/dialogue_clue_graph_future", gothic.SupportedCapabilityIds);
        Assert.Contains(gothic.CapabilityStatuses, item => item.CapabilityId == "capability/dialogue_clue_graph_future" && item.Status == "future_required");
        Assert.Contains("capability/vendor_economy_future", trade.FutureRequiredCapabilities);
        Assert.DoesNotContain("capability/vendor_economy_future", trade.SupportedCapabilityIds);
    }

    [Fact]
    public void InvalidFakeAndLeakMatrixRejectsCausally()
    {
        var repoRoot = FindRepoRoot();
        var result = new GeneratedGameProfileContractAcceptanceService()
            .BuildFromProfileDirectory(Path.Combine(repoRoot, "samples", "game-profiles"), repoRoot);

        Assert.True(result.Report.InvalidMatrix.Passed, string.Join(Environment.NewLine, result.Report.InvalidMatrix.Diagnostics.Select(item => item.Code)));
        Assert.True(result.Report.InvalidMatrix.ScenarioCount >= 18);
        Assert.Equal(result.Report.InvalidMatrix.ScenarioCount, result.Report.InvalidMatrix.RejectedCount);
        Assert.All(result.Report.InvalidMatrix.Scenarios, scenario => Assert.False(scenario.ActualValid));
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "cross_family_leakage_gothic_to_frontier_package_ids");
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "copied_profile_report_without_profile_files");
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item => item.ScenarioId == "unsupported_topology_accepted_as_complete");
    }

    [Fact]
    public void MissingPreviousGateOrCopiedReportRejects()
    {
        var repoRoot = FindRepoRoot();
        var service = new GeneratedGameProfileContractAcceptanceService();

        var stale = service.BuildFromProfileDirectory(
            Path.Combine(repoRoot, "samples", "game-profiles"),
            repoRoot,
            new GeneratedGameProfileContractOptions { PreviousAcceptedGate = "minimum_playable_generated_game_verification required" });
        var copied = service.BuildFromProfileDirectory(
            Path.Combine(repoRoot, "samples", "game-profiles"),
            repoRoot,
            new GeneratedGameProfileContractOptions { CopiedReportWithoutProfileFiles = true });

        Assert.False(stale.Report.ContractProofPassed);
        Assert.Contains(stale.Report.Diagnostics, item => item.Code == "game_profile.previous_gate.mismatch");
        Assert.False(copied.Report.ContractProofPassed);
        Assert.Contains(copied.Report.Diagnostics, item => item.Code == "game_profile.profile_files.missing");
    }

    [Fact]
    public async Task WrittenReportKeepsManualGateRequired()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var service = new GeneratedGameProfileContractAcceptanceService();
        var result = service.BuildFromProfileDirectory(Path.Combine(repoRoot, "samples", "game-profiles"), temp.Path);
        var write = await service.WriteAsync(temp.Path, result);

        var report = JsonSerializer.Deserialize<GeneratedGameProfileContractReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.False(report.Accepted);
        Assert.Equal(GeneratedGameProfileContractAcceptanceService.FinalGate, report.FinalStatus);
        Assert.Equal(GeneratedGameProfileContractAcceptanceService.FinalGate, report.ManualGate);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.False(report.UnityBuildExecuted);
        Assert.True(report.NoExternalProviderLlmRagLuaMedia);
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
