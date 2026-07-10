using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.ProductLineRuntimeVariantMatrix;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal142Tests
{
    [Fact]
    public async Task WorkspaceDisplaysGoal142RuntimeVariantMatrixSurface()
    {
        var root = ProjectRoot();
        if (!File.Exists(Path.Combine(
            root,
            ProductLineRuntimeVariantMatrixVocabulary.DashboardRelativePath)))
        {
            await new ProductLineRuntimeVariantMatrixService(
                    RuntimeBackedPlayerCommandRoundtripService.CreateDefault())
                .BuildAndWriteAsync(root);
        }

        var service = new VisualWorldStreamPreviewWorkspaceService();

        var result = service.Build(root);

        Assert.Contains(result.Catalog.Groups, group =>
            group.GroupId == "product_line_runtime_variant_matrix"
            && group.EntryCount >= 16);
        Assert.True(result.Report.ProductLineRuntimeVariantQualityGatePassed);
        Assert.Equal("GREEN", result.Report.ProductLineRuntimeVariantMatrixStatus);
        Assert.Equal(4, result.Report.ProductLineRuntimeVariantCandidateCount);
        Assert.Equal(4, result.Report.ProductLineRuntimeVariantPassedCandidateCount);
        Assert.Equal(0, result.Report.ProductLineRuntimeVariantFailedCandidateCount);
        Assert.Equal(4, result.Report.ProductLineRuntimeVariantRuntimeSignificantCandidateCount);
        Assert.True(result.Report.ProductLineRuntimeVariantDistinctFinalStateHashCount >= 3);
        Assert.Equal(
            "minimal-map-game-exploration-resource-focus",
            result.Report.ProductLineRuntimeVariantSelectedCandidateId);
        Assert.Equal("exploration_resource_focus", result.Report.ProductLineRuntimeVariantSelectedVariantKind);
        Assert.True(result.Report.ProductLineRuntimeVariantSelectedScore > 0);
        Assert.True(result.Report.ProductLineRuntimeVariantSourceTemplateUnmodified);
        Assert.False(result.Report.ProductLineRuntimeVariantAccepted);
        Assert.Equal(
            ProductLineRuntimeVariantMatrixVocabulary.NormalCommand,
            result.Report.ProductLineRuntimeVariantNormalCommand);
        Assert.Equal(
            ProductLineRuntimeVariantMatrixVocabulary.MatrixResultRelativePath,
            result.Report.ProductLineRuntimeVariantMatrixResultPath);
        Assert.Equal(
            ProductLineRuntimeVariantMatrixVocabulary.SelectedHandoffRelativePath,
            result.Report.ProductLineRuntimeVariantSelectedHandoffPath);
        Assert.True(result.Report.ProductLineRuntimeVariantFilesDiscoveredByRelativePaths);
        Assert.True(result.WinFormsBindingInventory.PageBindDisplaysProductLineRuntimeVariantMatrix);
        Assert.Contains(result.ProofStatus.Proofs, proof =>
            proof.ProofId == "goal142.product_line_runtime_variant_matrix.distinctness"
            && proof.Passed);
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
