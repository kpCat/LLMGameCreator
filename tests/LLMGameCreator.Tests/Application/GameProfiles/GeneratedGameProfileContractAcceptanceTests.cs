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
        var options = OptionsWithGoal020Evidence(repoRoot);

        var first = service.BuildFromProfileDirectory(profileDirectory, temp.Path, options);
        var second = service.BuildFromProfileDirectory(profileDirectory, temp.Path, options);
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
    public void ValidReportHasNoErrorDiagnosticsAndDependsOnGoal020Evidence()
    {
        var repoRoot = FindRepoRoot();
        var result = new GeneratedGameProfileContractAcceptanceService()
            .BuildFromProfileDirectory(
                Path.Combine(repoRoot, "samples", "game-profiles"),
                repoRoot,
                OptionsWithGoal020Evidence(repoRoot));

        Assert.True(result.Report.ContractProofPassed, string.Join(Environment.NewLine, result.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.DoesNotContain(result.Report.Diagnostics, item => item.Severity == "error");
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "game_profile.goal020_evidence.present");
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

    [Fact]
    public void MissingGoal020CompactEvidenceRejectsCausally()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var result = new GeneratedGameProfileContractAcceptanceService()
            .BuildFromProfileDirectory(
                Path.Combine(repoRoot, "samples", "game-profiles"),
                repoRoot,
                new GeneratedGameProfileContractOptions
                {
                    Goal020EvidenceDirectoryPath = Path.Combine(temp.Path, "missing-goal020-evidence")
                });

        Assert.False(result.Report.ContractProofPassed);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "game_profile.goal020_evidence.report_missing");
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item =>
            item.ScenarioId == "missing_accepted_goal020_evidence" &&
            item.Diagnostics.Any(diagnostic => diagnostic.Code == "game_profile.goal020_evidence.report_missing"));
    }

    [Fact]
    public void EmptyRequiredProfileFieldsRejectThroughValidation()
    {
        var repoRoot = FindRepoRoot();
        var service = new GeneratedGameProfileContractAcceptanceService();
        var cases = new (Func<GeneratedGameProfile, GeneratedGameProfile> Mutate, string ExpectedCode)[]
        {
            (profile => profile with { TargetExperience = string.Empty }, "game_profile.target_experience.missing"),
            (profile => profile with { ProgressionScope = string.Empty }, "game_profile.progression_scope.missing"),
            (profile => profile with { ContentScale = profile.ContentScale with { Target = string.Empty } }, "game_profile.content_scale.target_missing"),
            (profile => profile with { AssetPolicy = profile.AssetPolicy with { Mode = string.Empty } }, "game_profile.asset_policy.mode_missing"),
            (profile => profile with { AssetPolicy = profile.AssetPolicy with { FallbackPolicy = string.Empty } }, "game_profile.asset_policy.fallback_missing"),
            (profile => profile with { SelectedCapabilityIds = [] }, "game_profile.capability.selected_missing"),
            (profile => profile with { ExpectedDownstreamPipelineSlices = [] }, "game_profile.pipeline.required_stages_empty"),
            (profile => profile with { ExpectedDownstreamPipelineSlices = profile.ExpectedDownstreamPipelineSlices.Where(stage => stage != "stage/minimum_playable_generated_game_goal_020").ToList() }, "game_profile.pipeline.required_stage_missing")
        };

        foreach (var testCase in cases)
        {
            using var profileDirectory = CreateProfileDirectoryWithMutation(testCase.Mutate);
            var result = service.BuildFromProfileDirectory(profileDirectory.Path, repoRoot, OptionsWithGoal020Evidence(repoRoot));

            Assert.False(result.Report.ContractProofPassed);
            Assert.Contains(result.Report.Diagnostics, item => item.Code == testCase.ExpectedCode);
        }
    }

    [Fact]
    public void InvalidMatrixUsesSharedValidatorsForDuplicateAndFutureTopology()
    {
        var repoRoot = FindRepoRoot();
        var result = new GeneratedGameProfileContractAcceptanceService()
            .BuildFromProfileDirectory(
                Path.Combine(repoRoot, "samples", "game-profiles"),
                repoRoot,
                OptionsWithGoal020Evidence(repoRoot));

        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item =>
            item.ScenarioId == "duplicate_profile_ids" &&
            item.Diagnostics.Any(diagnostic => diagnostic.Code == "game_profile.profile_id.duplicate"));
        Assert.Contains(result.Report.InvalidMatrix.Scenarios, item =>
            item.ScenarioId == "unsupported_topology_accepted_as_complete" &&
            item.Diagnostics.Any(diagnostic => diagnostic.Code == "game_profile.topology.future_required_not_explicit"));
    }

    [Fact]
    public void CurrentStateKeepsGoal021RecordAfterLaterGoalHandoff()
    {
        var repoRoot = FindRepoRoot();
        using var state = JsonDocument.Parse(File.ReadAllText(Path.Combine(repoRoot, "docs", "CURRENT_GENERATOR_STATE.json")));
        var lastCompleted = state.RootElement.GetProperty("last_completed_product_slice_id").GetString();

        Assert.Equal(
            "goal_021_generated_game_profile_contract_refresh",
            state.RootElement.GetProperty("goal_021_generated_game_profile_contract_refresh").GetProperty("slice_id").GetString());
        Assert.Contains(
            lastCompleted,
            new[]
            {
                "goal_021_generated_game_profile_contract_refresh",
                "goal_022_development_complexity_stabilization",
                "goal_023_capability_bundle_pipeline_inputs",
                "goal_024_rich_package_assembly_coverage_audit",
                "goal_025_package_assembly_expansion_1_world_and_entities",
                "goal_026_package_assembly_expansion_2_dialogue_and_quests",
                "goal_027_package_assembly_expansion_3_items_economy_crafting",
                "goal_028_package_assembly_expansion_4_combat_progression"
            });
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

    private static GeneratedGameProfileContractOptions OptionsWithGoal020Evidence(string repoRoot) =>
        new()
        {
            Goal020EvidenceDirectoryPath = Path.Combine(repoRoot, ".llmgc", "procedural", "minimum-playable-generated-game")
        };

    private static TempDirectory CreateProfileDirectoryWithMutation(Func<GeneratedGameProfile, GeneratedGameProfile> mutate)
    {
        var repoRoot = FindRepoRoot();
        var sourcePath = Path.Combine(repoRoot, "samples", "game-profiles", "frontier-survival-minimum-alpha.game-profile.json");
        var source = JsonSerializer.Deserialize<GeneratedGameProfile>(
            File.ReadAllText(sourcePath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        var temp = new TempDirectory();
        var mutated = mutate(source);
        var json = JsonSerializer.Serialize(
            mutated,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
        File.WriteAllText(Path.Combine(temp.Path, "mutated.game-profile.json"), json);
        return temp;
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
