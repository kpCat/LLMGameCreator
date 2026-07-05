using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class AcceptedAlphaUnityPlayableProjectionProductSmokeTests
{
    [Fact]
    public async Task ProductSmokeWritesProjectionArtifactsAndWorkspaceSurface()
    {
        var root = ProjectRoot();
        var write = await new AcceptedAlphaUnityPlayableProjectionService()
            .BuildAndWriteAsync(root);
        var hotfix = await new AcceptedAlphaUnityMaterialWarningHotfixService()
            .BuildAndWriteAsync(root);
        var usability = await new AcceptedAlphaProjectionUsabilityService()
            .BuildAndWriteAsync(root);

        Assert.Equal("GREEN", write.Result.QualityGateScan.ImplementationStatus);
        Assert.True(write.Result.QualityGateScan.Passed);
        Assert.True(write.Result.Dashboard.AcceptedBaselineReady);
        Assert.Equal(
            AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath,
            write.Result.Dashboard.UnityMenuPath);
        Assert.Contains(write.WrittenFiles, path =>
            path == AcceptedAlphaUnityPlayableProjectionVocabulary
                .ProceduralOutputDirectory
            + "/"
            + AcceptedAlphaUnityPlayableProjectionVocabulary.DashboardFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == AcceptedAlphaUnityPlayableProjectionVocabulary
                .ExportPackageDirectory
            + "/"
            + AcceptedAlphaUnityPlayableProjectionVocabulary.ScriptInventoryFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == AcceptedAlphaUnityPlayableProjectionVocabulary.DocumentationPath);
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith("unity/", StringComparison.Ordinal));
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.Contains("StreamingAssets", StringComparison.Ordinal));
        Assert.True(hotfix.Result.ScriptScan.Passed);
        Assert.True(hotfix.Result.Dashboard.RendererMaterialSourceAccessAbsent);
        Assert.True(hotfix.Result.Dashboard.MaterialAssignmentSourceAccessAbsent);
        Assert.True(hotfix.Result.Dashboard.MaterialPropertyBlockUsed);
        Assert.True(hotfix.Result.NegativeProof.Passed);
        Assert.Contains(hotfix.WrittenFiles, path =>
            path == AcceptedAlphaUnityMaterialWarningHotfixVocabulary
                .ProceduralOutputDirectory
            + "/"
            + AcceptedAlphaUnityMaterialWarningHotfixVocabulary.DashboardFileName);
        Assert.Contains(hotfix.WrittenFiles, path =>
            path == AcceptedAlphaUnityMaterialWarningHotfixVocabulary
                .ProceduralOutputDirectory
            + "/"
            + AcceptedAlphaUnityMaterialWarningHotfixVocabulary.LogScanFileName);
        Assert.DoesNotContain(hotfix.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Equal("GREEN", usability.Result.Dashboard.UsabilityStatus);
        Assert.True(usability.Result.Dashboard.LegendPresent);
        Assert.True(usability.Result.Dashboard.MarkerDescriptorPresent);
        Assert.True(usability.Result.Dashboard.SelectionControlsPresent);
        Assert.True(usability.Result.Dashboard.FocusCameraControlPresent);
        Assert.True(usability.Result.Dashboard.CleanupScriptContractPassed);
        Assert.Contains(usability.WrittenFiles, path =>
            path == AcceptedAlphaProjectionUsabilityVocabulary
                .ProceduralOutputDirectory
            + "/"
            + AcceptedAlphaProjectionUsabilityVocabulary.DashboardFileName);
        Assert.Contains(usability.WrittenFiles, path =>
            path == AcceptedAlphaProjectionUsabilityVocabulary
                .ExportPackageDirectory
            + "/"
            + AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptScanFileName);
        Assert.Contains(usability.WrittenFiles, path =>
            path == AcceptedAlphaProjectionUsabilityVocabulary.DocumentationPath);
        Assert.DoesNotContain(usability.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysAcceptedAlphaUnityPlayableProjection);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysAcceptedAlphaProjectionUsability);
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "accepted_alpha_unity_playable_projection");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "accepted_alpha_projection_usability");
        Assert.True(workspace.QualityGateScan.AcceptedAlphaUnityPlayableProjectionQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal119FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.AcceptedAlphaProjectionUsabilityQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal120FilesDiscoveredByRelativePaths);
        Assert.Contains(
            "acceptedAlphaUnityPlayableProjectionGeneratedRootName: "
            + AcceptedAlphaUnityPlayableProjectionVocabulary.GeneratedRootName,
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "acceptedAlphaProjectionUsabilityCleanupScriptPath: "
            + AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath,
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
