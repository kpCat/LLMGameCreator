using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualGateAcceptanceRecord;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class OfflineGeoworldAlphaManualGateAcceptanceRecordProductSmokeTests
{
    [Fact]
    public async Task ProductSmokeWritesAcceptanceRecordArtifactsAndWorkspaceSurface()
    {
        var root = ProjectRoot();
        var write = await new OfflineGeoworldAlphaManualGateAcceptanceRecordService()
            .BuildAndWriteAsync(root);

        Assert.Equal("GREEN", write.Result.QualityGateScan.ImplementationStatus);
        Assert.True(write.Result.QualityGateScan.Passed);
        Assert.Contains(write.WrittenFiles, path =>
            path == OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory
            + "/"
            + OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.AcceptanceRecordFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ExportPackageDirectory
            + "/"
            + OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.AcceptanceRecordFileName);
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith("unity/", StringComparison.Ordinal));

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysOfflineGeoworldAlphaManualGateAcceptanceRecord);
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "offline_geoworld_alpha_manual_gate_acceptance_record");
        Assert.True(workspace.QualityGateScan
            .OfflineGeoworldAlphaManualGateAcceptanceQualityGatePassed);
        Assert.Contains(
            "offlineGeoworldAlphaManualGateAcceptanceHumanAccepted: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
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
