using System.Text.Json;
using LLMGameCreator.Application.Design.CapabilityBundlePipelineInputs;
using Xunit;

namespace LLMGameCreator.Tests.Application.CapabilityBundlePipelineInputs;

public sealed class CapabilityBundlePipelineInputsAcceptanceTests
{
    [Fact]
    public async Task BuildsDeterministicArtifactsForThreeAcceptedProfiles()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var profileDirectory = Path.Combine(repoRoot, "samples", "game-profiles");
        var service = new CapabilityBundlePipelineInputsAcceptanceService();

        var first = await service.BuildAsync(temp.Path, profileDirectory);
        var second = await service.BuildAsync(temp.Path, profileDirectory);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.False(first.Report.Accepted);
        Assert.Equal(CapabilityBundlePipelineInputsAcceptanceService.FinalGate, first.Report.FinalStatus);
        Assert.Equal(CapabilityBundlePipelineInputsAcceptanceService.FinalGate, first.Report.ManualGate);
        Assert.Equal(CapabilityBundlePipelineInputsAcceptanceService.PreviousAcceptedGate, first.Report.PreviousAcceptedGate);
        Assert.Equal(["S185", "S186", "S187", "S188", "S189", "S190", "S191"], first.Report.CompletedSlices);
        Assert.Equal(3, first.Report.ValidProfileCount);
        Assert.Equal(3, first.Report.PipelineInputCount);
        Assert.Equal(first.Report.ProfileRequestArtifactHash, second.Report.ProfileRequestArtifactHash);
        Assert.Equal(first.Report.SelectionArtifactHash, second.Report.SelectionArtifactHash);
        Assert.Equal(first.Report.GeneratorInputsArtifactHash, second.Report.GeneratorInputsArtifactHash);
        Assert.Equal(first.Report.GapReportHash, second.Report.GapReportHash);
        Assert.Equal(first.Report.InvalidMatrixHash, second.Report.InvalidMatrixHash);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(File.Exists(write.ProfileRequestsJsonPath));
        Assert.True(File.Exists(write.SelectionJsonPath));
        Assert.True(File.Exists(write.GeneratorInputsJsonPath));
        Assert.True(File.Exists(write.GapReportJsonPath));
        Assert.True(File.Exists(write.InvalidMatrixJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
    }

    [Fact]
    public async Task ProfileToSelectorRequestsIncludeConcreteVariantIds()
    {
        var repoRoot = FindRepoRoot();
        var result = await new CapabilityBundlePipelineInputsAcceptanceService()
            .BuildAsync(repoRoot, Path.Combine(repoRoot, "samples", "game-profiles"));

        Assert.True(result.Report.ContractProofPassed, string.Join(Environment.NewLine, result.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.Equal(3, result.ProfileRequestsArtifact.RequestCount);
        Assert.All(result.ProfileRequestsArtifact.Requests, request =>
        {
            Assert.False(string.IsNullOrWhiteSpace(request.SelectorRequest.PresentationModeId));
            Assert.False(string.IsNullOrWhiteSpace(request.SelectorRequest.WorldTopologyId));
            Assert.False(string.IsNullOrWhiteSpace(request.SelectorRequest.InventoryModelId));
            Assert.False(string.IsNullOrWhiteSpace(request.SelectorRequest.CombatModelId));
            Assert.False(string.IsNullOrWhiteSpace(request.SelectorRequest.ProgressionModelId));
            Assert.False(string.IsNullOrWhiteSpace(request.SelectorRequest.PathfindingProfileId));
            Assert.False(string.IsNullOrWhiteSpace(request.SelectorRequest.NpcBehaviorModelId));
            Assert.NotEmpty(request.SelectorRequest.SelectedFeatureBundleIds);
            Assert.NotEmpty(request.SelectorRequest.SelectedRuntimeTargetIds);
        });
    }

    [Fact]
    public async Task GeneratorInputsContainContractsValidatorsRuntimeTargetsOrExplicitGaps()
    {
        var repoRoot = FindRepoRoot();
        var result = await new CapabilityBundlePipelineInputsAcceptanceService()
            .BuildAsync(repoRoot, Path.Combine(repoRoot, "samples", "game-profiles"));

        Assert.Equal(3, result.GeneratorInputsArtifact.PipelineInputCount);
        Assert.All(result.GeneratorInputsArtifact.PipelineInputs, input =>
        {
            Assert.NotEmpty(input.SelectedFeatureBundleIds);
            Assert.True(input.ResolvedArtifactContractIds.Count > 0 || input.BlockedGapIds.Count > 0);
            Assert.True(input.ResolvedValidatorIds.Count > 0 || input.BlockedGapIds.Count > 0);
            Assert.NotEmpty(input.ResolvedRuntimeTargetIds);
            Assert.Contains("stage/minimum_playable_generated_game_goal_020", input.ExpectedDownstreamGenerationStages);
            Assert.DoesNotContain(input.PackageAssemblyCandidateInputs, value => value.Contains("packageAssemblyExecuted=true", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public async Task FutureCapabilitiesRemainFutureRequiredAndAtlasIncompatibilitiesAreExplicit()
    {
        var repoRoot = FindRepoRoot();
        var result = await new CapabilityBundlePipelineInputsAcceptanceService()
            .BuildAsync(repoRoot, Path.Combine(repoRoot, "samples", "game-profiles"));

        var gothic = Assert.Single(result.GeneratorInputsArtifact.PipelineInputs, input => input.ProfileId == "game_profile/gothic-mystery-investigation-alpha");
        var trade = Assert.Single(result.GeneratorInputsArtifact.PipelineInputs, input => input.ProfileId == "game_profile/trade-caravan-social-economy-alpha");

        Assert.Contains("capability/dialogue_clue_graph_future", gothic.FutureRequiredCapabilityIds);
        Assert.DoesNotContain("capability/dialogue_clue_graph_future", gothic.SupportedNowCapabilityIds);
        Assert.Contains("capability/vendor_economy_future", trade.FutureRequiredCapabilityIds);
        Assert.DoesNotContain("capability/vendor_economy_future", trade.SupportedNowCapabilityIds);
        Assert.Contains(result.GapReportArtifact.Gaps, gap => gap.Status == "blocked_gap" && gap.Code.Contains("incompatible", StringComparison.OrdinalIgnoreCase));
        Assert.True(result.GapReportArtifact.BlockedGapCount > 0);
    }

    [Fact]
    public async Task InvalidFakeLeakMatrixRejectsRequiredScenarios()
    {
        var repoRoot = FindRepoRoot();
        var result = await new CapabilityBundlePipelineInputsAcceptanceService()
            .BuildAsync(repoRoot, Path.Combine(repoRoot, "samples", "game-profiles"));

        Assert.True(result.InvalidMatrix.Passed, string.Join(Environment.NewLine, result.InvalidMatrix.Diagnostics.Select(item => item.Code)));
        Assert.True(result.InvalidMatrix.ScenarioCount >= 16);
        Assert.Equal(result.InvalidMatrix.ScenarioCount, result.InvalidMatrix.RejectedCount);
        Assert.All(result.InvalidMatrix.Scenarios, scenario => Assert.False(scenario.ActualValid));
        Assert.Contains(result.InvalidMatrix.Scenarios, item => item.ScenarioId == "missing_accepted_goal022_gate");
        Assert.Contains(result.InvalidMatrix.Scenarios, item => item.ScenarioId == "unknown_feature_bundle_id");
        Assert.Contains(result.InvalidMatrix.Scenarios, item => item.ScenarioId == "future_capability_marked_supported_now");
        Assert.Contains(result.InvalidMatrix.Scenarios, item => item.ScenarioId == "historical_goal021_or_goal020_artifact_mutation");
    }

    [Fact]
    public async Task ReportKeepsPlanningBoundaryAndNoTopLevelErrors()
    {
        var repoRoot = FindRepoRoot();
        var result = await new CapabilityBundlePipelineInputsAcceptanceService()
            .BuildAsync(repoRoot, Path.Combine(repoRoot, "samples", "game-profiles"));

        Assert.True(result.Report.ContractProofPassed, string.Join(Environment.NewLine, result.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.DoesNotContain(result.Report.Diagnostics, item => item.Severity == "error");
        Assert.True(result.Report.CapabilitySelectionStarted);
        Assert.False(result.Report.PackageAssemblyExecuted);
        Assert.False(result.Report.PublicGamePackageSchemaChanged);
        Assert.False(result.Report.ProjectFilesChanged);
        Assert.False(result.Report.GeneratorLibraryChanged);
        Assert.False(result.Report.UnityBuildExecuted);
        Assert.False(result.Report.LlmRagProviderMediaLuaExecuted);
        Assert.True(result.Report.ScopeGuardPassed);
    }

    [Fact]
    public async Task MissingPreviousGateOrCopiedSelectionRejects()
    {
        var repoRoot = FindRepoRoot();
        var service = new CapabilityBundlePipelineInputsAcceptanceService();
        var profileDirectory = Path.Combine(repoRoot, "samples", "game-profiles");

        var stale = await service.BuildAsync(
            repoRoot,
            profileDirectory,
            options: new CapabilityBundlePipelineInputsOptions { PreviousAcceptedGate = "development_complexity_stabilization_verification required" });
        var copied = await service.BuildAsync(
            repoRoot,
            profileDirectory,
            options: new CapabilityBundlePipelineInputsOptions { CopiedCapabilitySelectionReportWithoutProfiles = true });

        Assert.False(stale.Report.ContractProofPassed);
        Assert.Contains(stale.Report.Diagnostics, item => item.Code == "capability_bundle.goal022_gate.missing");
        Assert.False(copied.Report.ContractProofPassed);
        Assert.Contains(copied.Report.Diagnostics, item => item.Code == "capability_bundle.profile_files.missing");
    }

    [Fact]
    public async Task WrittenReportRoundTripsManualGate()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var service = new CapabilityBundlePipelineInputsAcceptanceService();
        var result = await service.BuildAsync(temp.Path, Path.Combine(repoRoot, "samples", "game-profiles"));
        var write = await service.WriteAsync(temp.Path, result);

        var report = JsonSerializer.Deserialize<CapabilityBundlePipelineInputsReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.False(report.Accepted);
        Assert.Equal(CapabilityBundlePipelineInputsAcceptanceService.FinalGate, report.FinalStatus);
        Assert.Equal(CapabilityBundlePipelineInputsAcceptanceService.FinalGate, report.ManualGate);
        Assert.Equal(3, report.PipelineInputCount);
        Assert.True(report.InvalidMatrix.Passed);
    }

    [Fact]
    public void CurrentStateKeepsGoal023RecordAfterLaterGoalHandoff()
    {
        var repoRoot = FindRepoRoot();
        using var state = JsonDocument.Parse(File.ReadAllText(Path.Combine(repoRoot, "docs", "CURRENT_GENERATOR_STATE.json")));
        var root = state.RootElement;

        Assert.Equal("goal_025_package_assembly_expansion_1_world_and_entities", root.GetProperty("last_completed_product_slice_id").GetString());
        Assert.Equal("package_assembly_world_entities_expansion_verification", root.GetProperty("gate_status").GetString());
        Assert.Equal("goal_023_capability_bundle_pipeline_inputs", root.GetProperty("goal_023_capability_bundle_pipeline_inputs").GetProperty("slice_id").GetString());
        Assert.Contains(
            "development_complexity_stabilization_verification passed",
            root.GetProperty("goal_023_capability_bundle_pipeline_inputs").GetProperty("summary").GetString());
        Assert.Contains(
            "capability_bundle_pipeline_inputs_verification passed",
            root.GetProperty("goal_023_capability_bundle_pipeline_inputs").GetProperty("summary").GetString());
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
