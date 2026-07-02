using LLMGameCreator.Application.Design.EditDrivenPlayablePreviewRefresh;
using Xunit;

namespace LLMGameCreator.Tests.Application.EditDrivenPlayablePreviewRefresh;

public sealed class EditDrivenPlayablePreviewRefreshQualityGateTests
{
    [Fact]
    public void QualityGateScansGoal076FilesAndAlphaBootstrapWithoutBloat()
    {
        var result = new EditDrivenPlayablePreviewRefreshEvidenceService().Build(ProjectRoot());

        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.QualityGateScan.ParentUiBindingPassed);
        Assert.False(result.QualityGateScan.ReportOnlySmokeDetected);
        Assert.Equal(0, result.QualityGateScan.LinesOver500Count);
        Assert.Equal(0, result.QualityGateScan.FilesOver1000LinesCount);
        Assert.Equal(0, result.QualityGateScan.MinifiedSourceFileCount);
        Assert.True(result.QualityGateScan.AlphaRuntimeBootstrapLineCount > 3000);
        Assert.Contains(
            result.QualityGateScan.Files,
            file => file.RelativePath == "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs");
    }

    [Fact]
    public void QualityGateRejectsMinifiedSourceAndReportOnlySmoke()
    {
        var root = Path.Combine(Path.GetTempPath(), "llmgc-goal076-quality-" + Guid.NewGuid().ToString("N"));
        try
        {
            WriteFile(
                root,
                "src/LLMGameCreator.Application/Design/EditDrivenPlayablePreviewRefresh/Minified.cs",
                "public sealed class Minified { }");
            WriteFile(
                root,
                "tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenPlayablePreviewRefreshProductSmokeTests.cs",
                "public sealed class Smoke { string ImplementationStatus = \"GREEN\"; }");
            WriteBindingFiles(root);

            var scan = new EditDrivenPlayablePreviewRefreshQualityGateScanner().Scan(root);

            Assert.False(scan.Passed);
            Assert.True(scan.MinifiedSourceFileCount >= 1);
            Assert.True(scan.ReportOnlySmokeDetected);
            Assert.Contains(scan.Diagnostics, diagnostic => diagnostic.Code == "goal076.quality.minified_source");
            Assert.Contains(scan.Diagnostics, diagnostic => diagnostic.Code == "goal076.quality.report_only_smoke");
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static void WriteBindingFiles(string root)
    {
        WriteFile(
            root,
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignPlayableRefreshControl.cs",
            """
            public sealed class CampaignPlayableRefreshControl
            {
                public void Bind(object result) { }
            }
            """);
        WriteFile(
            root,
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                + "CampaignAuthoringReviewWorkspacePageControl.Designer.cs",
            """
            public sealed partial class CampaignAuthoringReviewWorkspacePageControl
            {
                private object _playableRefreshTabPage;
                private CampaignPlayableRefreshControl _playableRefreshControl;
            }
            """);
        WriteFile(
            root,
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                + "CampaignAuthoringReviewWorkspacePageControl.cs",
            """
            public sealed partial class CampaignAuthoringReviewWorkspacePageControl
            {
                private EditDrivenPlayablePreviewRefreshEvidenceService _playableRefreshService;
                public void OnActivated()
                {
                    var refreshResult = _playableRefreshService.Build(root);
                    _playableRefreshControl.Bind(refreshResult);
                }
            }
            """);
    }

    private static void WriteFile(string root, string relativePath, string contents)
    {
        var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, contents);
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
