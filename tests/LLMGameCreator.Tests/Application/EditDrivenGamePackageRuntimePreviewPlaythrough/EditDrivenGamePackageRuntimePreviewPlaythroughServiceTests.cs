using LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewPlaythrough;
using Xunit;

namespace LLMGameCreator.Tests.Application.EditDrivenGamePackageRuntimePreviewPlaythrough;

public sealed class EditDrivenGamePackageRuntimePreviewPlaythroughServiceTests
{
    [Fact]
    public async Task ServiceBuildsDiskBackedGoal081PlaythroughArtifacts()
    {
        var service = new EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.SourceArtifactManifest.Goal080AcceptedByHandoff);
        Assert.True(result.PackageReadProof.Passed);
        Assert.True(result.CommandScript.Passed);
        Assert.True(result.Transcript.Passed);
        Assert.True(result.StateHashChain.Passed);
        Assert.True(result.CoverageLedger.Passed);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.WinFormsBindingInventory.Passed);
        Assert.True(result.QualityGateScan.Passed);
        Assert.Equal(9, result.Report.RowCount);
        Assert.Equal(18, result.Report.TargetCount);
        Assert.Equal(57, result.Report.Goal078ActionCount);
        Assert.Equal(result.CommandScript.CommandCount, result.Transcript.CommandCount);

        foreach (var fileName in EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService.RequiredArtifactNames())
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }
    }

    [Fact]
    public void CommandScriptReplayAndCoverageAreDeterministic()
    {
        var service = new EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService();
        var first = service.Build(ProjectRoot());
        var second = service.Build(ProjectRoot());

        Assert.Equal(first.Report.CommandScriptHash, second.Report.CommandScriptHash);
        Assert.Equal(first.Report.TranscriptHash, second.Report.TranscriptHash);
        Assert.Equal(first.Transcript.FinalStateHash, second.Transcript.FinalStateHash);
        Assert.NotEqual(first.Transcript.InitialStateHash, first.Transcript.FinalStateHash);
        Assert.True(first.Transcript.ReplayFinalHashMatchesOriginal);
        Assert.True(first.StateHashChain.ReplayRerunFinalHashMatchesFirstRun);
        Assert.Equal(9, first.CoverageLedger.CoveredRowCount);
        Assert.Equal(18, first.CoverageLedger.CoveredTargetCount);
        Assert.Equal(57, first.CoverageLedger.CoveredGoal078ActionCount);
        Assert.Equal(
            57,
            first.CommandScript.Commands
                .SelectMany(command => command.CoveredGoal078ActionIds)
                .Distinct(StringComparer.Ordinal)
                .Count());
        Assert.Equal(9, first.CommandScript.Commands.Count(command => command.CommandType == "complete_scenario"));
        Assert.Equal(18, first.CommandScript.Commands.Count(command => command.CommandType == "collect_projected_target"));
    }

    [Fact]
    public void NegativeProofRejectsAllRequiredGoal081Scenarios()
    {
        var result = new EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService().Build(ProjectRoot());

        Assert.True(result.NegativeProof.Passed);
        AssertScenarioRejected(result, "missing_projected_gamepackage_payload");
        AssertScenarioRejected(result, "tampered_projected_gamepackage_payload");
        AssertScenarioRejected(result, "missing_player_readable_bridge_index");
        AssertScenarioRejected(result, "command_script_nonexistent_target");
        AssertScenarioRejected(result, "replay_order_mismatch");
        AssertScenarioRejected(result, "fake_success_without_package_read");
        AssertScenarioRejected(result, "source_goal080_lineage_hash_mismatch");
    }

    private static void AssertScenarioRejected(
        EditDrivenGamePackageRuntimePreviewPlaythroughBuildResult result,
        string scenarioId)
    {
        var scenario = Assert.Single(result.NegativeProof.Scenarios, item => item.ScenarioId == scenarioId);
        Assert.Equal("rejected", scenario.ActualStatus);
        Assert.NotEmpty(scenario.Diagnostics);
    }

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
