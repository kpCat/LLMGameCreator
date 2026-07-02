using LLMGameCreator.Application.Design.EditDrivenPlayablePreviewRefresh;
using LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;
using Xunit;

namespace LLMGameCreator.Tests.Application.EditDrivenPlayablePreviewRefresh;

public sealed class EditDrivenPlayablePreviewRefreshServiceTests
{
    [Fact]
    public void ServiceConsumesGoal075EvidenceAndRows()
    {
        var root = ProjectRoot();
        var goal075 = new SchemaDrivenCampaignEditEvidenceService().Build(root);
        var result = new EditDrivenPlayablePreviewRefreshEvidenceService().Build(root);

        Assert.True(result.SourceArtifactManifest.Goal075AcceptedByUserHandoff);
        Assert.True(result.SourceArtifactManifest.Goal075ReportWasGreenProducedForReview);
        Assert.True(result.SourceArtifactManifest.Goal075ParentActivationBindingPassed);
        Assert.Equal(EditDrivenPlayablePreviewRefreshHash.Sha256(EditDrivenPlayablePreviewRefreshHash.Serialize(goal075.Report)), result.Report.SourceGoal075ReportHash);
        Assert.Equal(goal075.ApplyRollbackLedger.Rows.Select(row => row.RowId).OrderBy(id => id, StringComparer.Ordinal), result.StateTransitionProof.Rows.Select(row => row.RowId).OrderBy(id => id, StringComparer.Ordinal));
        Assert.Contains(result.SourceArtifactManifest.SourceArtifacts, item => item.ArtifactRelativePath.EndsWith("apply-rollback-ledger.json", StringComparison.Ordinal));
    }

    [Fact]
    public void StateTransitionHashesProveBeforeAfterRollbackAndReplay()
    {
        var result = new EditDrivenPlayablePreviewRefreshEvidenceService().Build(ProjectRoot());

        Assert.True(result.StateTransitionProof.Passed);
        Assert.Equal(9, result.StateTransitionProof.RowCount);
        Assert.Equal(9, result.StateTransitionProof.StateChangingRowCount);
        Assert.NotEqual(result.Report.BeforeStateHash, result.Report.AfterStateHash);
        Assert.Equal(result.Report.BeforeStateHash, result.Report.RollbackStateHash);
        Assert.Equal(result.Report.AfterStateHash, result.Report.ReplayStateHash);
        Assert.All(result.StateTransitionProof.Rows, row =>
        {
            Assert.NotEqual(row.BeforeHash, row.AfterHash);
            Assert.Equal(row.BeforeHash, row.RollbackHash);
            Assert.Equal(row.AfterHash, row.ReplayHash);
            Assert.NotEmpty(row.AppliedChanges);
            Assert.NotEmpty(row.PackageLogicalTargets);
        });
    }

    [Fact]
    public void RefreshPlanTargetsAreDerivedFromChangedRows()
    {
        var result = new EditDrivenPlayablePreviewRefreshEvidenceService().Build(ProjectRoot());

        Assert.True(result.GamePackageRefreshPlan.Passed);
        Assert.False(result.GamePackageRefreshPlan.PublicGamePackageSchemaMutationRequired);
        Assert.Equal(result.StateTransitionProof.RowCount, result.GamePackageRefreshPlan.RowCount);
        Assert.Equal(result.StateTransitionProof.Rows.Sum(row => row.AppliedChanges.Count), result.GamePackageRefreshPlan.TargetCount);
        foreach (var row in result.StateTransitionProof.Rows)
        {
            var planRow = Assert.Single(result.GamePackageRefreshPlan.Rows, item => item.RowId == row.RowId);
            Assert.Equal(row.PreviewRefreshKey, planRow.RefreshKey);
            Assert.Equal(row.AppliedChanges.Select(change => change.FieldId).OrderBy(id => id, StringComparer.Ordinal), planRow.Targets.Select(target => target.FieldId).OrderBy(id => id, StringComparer.Ordinal));
            Assert.All(planRow.Targets, target => Assert.StartsWith("gamepackage/generated-content/", target.LogicalPackagePath, StringComparison.Ordinal));
        }
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
