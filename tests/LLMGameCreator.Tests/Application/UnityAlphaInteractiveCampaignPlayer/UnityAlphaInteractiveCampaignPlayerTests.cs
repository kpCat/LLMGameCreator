using System.Text.Json;
using LLMGameCreator.Application.Design.UnityAlphaInteractiveCampaignPlayer;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnityAlphaInteractiveCampaignPlayer;

public sealed class UnityAlphaInteractiveCampaignSourceLoadingTests
{
    [Fact]
    public void SourceLoaderConsumesGoal070EvidenceAndPreflightHandoff()
    {
        var source = new UnityAlphaInteractiveCampaignSourceLoader().Load(UnityAlphaInteractiveCampaignProjectRootLocator.ProjectRoot());

        Assert.True(source.Goal070AcceptedByUserHandoff);
        Assert.True(source.Goal070TimelineEvidenceConsumed);
        Assert.True(source.Goal070UnityProofConsumed);
        Assert.Equal(9, source.Rows.Count);
        Assert.Equal(3, source.FamilyIds.Count);
        Assert.Equal(3, source.SeedIds.Count);
        Assert.NotEmpty(source.BaseStagingFiles);
        Assert.All(source.Rows, row =>
        {
            Assert.NotEmpty(row.Goal070RowHash);
            Assert.NotEqual(row.Goal070InitialStateHash, row.Goal070FinalStateHash);
            Assert.True(row.Steps.Count >= 2);
            Assert.All(row.Steps, step => Assert.NotEqual(step.StateBeforeHash, step.StateAfterHash));
        });
    }
}

public sealed class UnityAlphaInteractiveCampaignBuildTests
{
    [Fact]
    public void BuildCreatesNineSelectableRowsAndScriptedStateTransitions()
    {
        var result = new UnityAlphaInteractiveCampaignEvidenceService().Build(UnityAlphaInteractiveCampaignProjectRootLocator.ProjectRoot());

        Assert.True(result.SourceManifest.Goal070AcceptedByUserHandoff);
        Assert.True(result.SourceManifest.Goal070TimelineEvidenceConsumed);
        Assert.True(result.SourceManifest.Goal070UnityProofConsumed);
        Assert.True(result.Matrix.Passed);
        Assert.Equal(9, result.Matrix.RowCount);
        Assert.Equal(9, result.Matrix.StateChangingRowCount);
        Assert.True(result.Matrix.ActionCount >= 18);
        Assert.True(result.Selector.Passed);
        Assert.Equal(3, result.Selector.Families.Count);
        Assert.All(result.Selector.Families, family =>
        {
            Assert.Equal(3, family.SeedIds.Count);
            Assert.Equal(3, family.RowIds.Count);
        });
    }

    [Fact]
    public void ActionsReplayHudAndInvalidMatrixCoverGoal071Contract()
    {
        var result = new UnityAlphaInteractiveCampaignEvidenceService().Build(UnityAlphaInteractiveCampaignProjectRootLocator.ProjectRoot());

        Assert.True(result.InputActionScript.Passed);
        Assert.True(result.StateTransitionLedger.Passed);
        Assert.Equal(result.InputActionScript.ActionCount, result.StateTransitionLedger.TransitionCount);
        Assert.True(result.SaveLoadReplayProof.Passed);
        Assert.Equal(9, result.SaveLoadReplayProof.SaveLoadPassedRowCount);
        Assert.Equal(9, result.SaveLoadReplayProof.ReplayPassedRowCount);
        Assert.True(result.HudContract.Passed);
        Assert.Contains("familyId", result.HudContract.RequiredFields);
        Assert.Contains("stateBeforeHash", result.HudContract.RequiredFields);
        Assert.Contains("stateAfterHash", result.HudContract.RequiredFields);
        Assert.True(result.PreviewExportPayload.Passed);
        Assert.True(result.InvalidMatrix.Passed);
        var invalidIds = result.InvalidMatrix.Scenarios.Select(item => item.ScenarioId).ToHashSet(StringComparer.Ordinal);
        foreach (var required in UnityAlphaInteractiveCampaignVocabulary.RequiredInvalidScenarioIds)
        {
            Assert.Contains(required, invalidIds);
        }

        Assert.All(result.InputActionScript.Actions, action =>
        {
            Assert.True(action.DeltaApplied);
            Assert.NotEqual(action.StateBeforeHash, action.StateAfterHash);
        });
    }
}

public sealed class UnityAlphaInteractiveCampaignUnityPlanTests
{
    [Fact]
    public void UnityCommandPlanContainsRequiredInteractiveMarkers()
    {
        var result = new UnityAlphaInteractiveCampaignEvidenceService().Build(UnityAlphaInteractiveCampaignProjectRootLocator.ProjectRoot());

        Assert.True(result.UnityCommandPlan.Passed);
        Assert.False(result.UnityCommandPlan.Accepted);
        Assert.Equal(9, result.UnityCommandPlan.Rows.Count);
        foreach (var marker in UnityAlphaInteractiveCampaignBuilder.RequiredUnityMarkers())
        {
            Assert.Contains(marker, result.UnityCommandPlan.ExpectedPlayerMarkers);
        }

        Assert.All(result.UnityCommandPlan.Rows, row =>
        {
            Assert.True(row.StepIds.Count >= 2);
            Assert.Equal(row.StepIds.Count, row.InputIds.Count);
            Assert.Equal(row.StepIds.Count, row.StateBeforeHashes.Count);
            Assert.Equal(row.StepIds.Count, row.StateAfterHashes.Count);
            Assert.Contains("interactive_campaign_selected_row=" + row.RowId, row.ExpectedPlayerMarkers);
            Assert.Contains("interactive_campaign_family=" + row.FamilyId, row.ExpectedPlayerMarkers);
            Assert.Contains("interactive_campaign_seed=" + row.SeedId, row.ExpectedPlayerMarkers);
            Assert.Contains("interactive_campaign_row_completed=" + row.RowId, row.ExpectedPlayerMarkers);
        });
    }
}

public sealed class UnityAlphaInteractiveCampaignEvidenceWriteTests
{
    [Fact]
    public async Task WriteAsyncEmitsRequiredDeterministicArtifactsAndStagingCommandPlan()
    {
        var service = new UnityAlphaInteractiveCampaignEvidenceService();
        var result = service.Build(UnityAlphaInteractiveCampaignProjectRootLocator.ProjectRoot());
        var second = service.Build(UnityAlphaInteractiveCampaignProjectRootLocator.ProjectRoot());
        var tempRoot = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", "Goal071Write", Guid.NewGuid().ToString("N"));

        try
        {
            var write = await service.WriteAsync(tempRoot, result);
            Assert.Equal(result.Report.MatrixHash, second.Report.MatrixHash);
            Assert.Equal(result.Report.InputActionScriptHash, second.Report.InputActionScriptHash);
            Assert.Equal(result.Report.StateTransitionLedgerHash, second.Report.StateTransitionLedgerHash);
            Assert.Equal(result.Report.HudContractHash, second.Report.HudContractHash);
            Assert.Equal(result.Report.InvalidMatrixHash, second.Report.InvalidMatrixHash);
            Assert.DoesNotContain(result.Report.Diagnostics, item => item.Severity == "error" && !item.Code.StartsWith("goal071.unity.", StringComparison.Ordinal));

            foreach (var fileName in RequiredJsonFiles())
            {
                var path = Path.Combine(write.OutputDirectoryPath, fileName);
                Assert.True(File.Exists(path), "Missing artifact: " + fileName);
                using var _ = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            }

            var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
            Assert.Contains("unity_alpha_interactive_campaign_player_verification required", report);
            Assert.Contains("accepted=false", report);
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, UnityAlphaInteractiveCampaignVocabulary.StagingRoot, UnityAlphaInteractiveCampaignVocabulary.UnityInteractiveCommandPlanStagingRelativePath.Replace('/', Path.DirectorySeparatorChar))));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    private static IReadOnlyList<string> RequiredJsonFiles() =>
    [
        UnityAlphaInteractiveCampaignEvidenceService.SourceManifestJsonFileName,
        UnityAlphaInteractiveCampaignEvidenceService.MatrixJsonFileName,
        UnityAlphaInteractiveCampaignEvidenceService.SelectorJsonFileName,
        UnityAlphaInteractiveCampaignEvidenceService.CommandPlanJsonFileName,
        UnityAlphaInteractiveCampaignEvidenceService.InputScriptJsonFileName,
        UnityAlphaInteractiveCampaignEvidenceService.StateTransitionLedgerJsonFileName,
        UnityAlphaInteractiveCampaignEvidenceService.SaveLoadReplayProofJsonFileName,
        UnityAlphaInteractiveCampaignEvidenceService.HudContractJsonFileName,
        UnityAlphaInteractiveCampaignEvidenceService.PlayerProofSummaryJsonFileName,
        UnityAlphaInteractiveCampaignEvidenceService.InvalidDiagnosticsMatrixJsonFileName,
        UnityAlphaInteractiveCampaignEvidenceService.PreviewExportPayloadJsonFileName,
        UnityAlphaInteractiveCampaignEvidenceService.ArtifactScopeReportJsonFileName
    ];
}

internal static class UnityAlphaInteractiveCampaignProjectRootLocator
{
    public static string ProjectRoot()
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
