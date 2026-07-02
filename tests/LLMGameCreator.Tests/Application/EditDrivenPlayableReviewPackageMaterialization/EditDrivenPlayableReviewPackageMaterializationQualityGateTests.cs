using LLMGameCreator.Application.Design.EditDrivenPlayableReviewPackageMaterialization;
using Xunit;

namespace LLMGameCreator.Tests.Application.EditDrivenPlayableReviewPackageMaterialization;

public sealed class EditDrivenPlayableReviewPackageMaterializationQualityGateTests
{
    [Fact]
    public void QualityGateScansGoal077FilesAndAlphaBootstrapWithoutBloat()
    {
        var result = new EditDrivenPlayableReviewPackageMaterializationEvidenceService().Build(ProjectRoot());

        Assert.True(result.QualityGateScan.Passed);
        Assert.Equal(18, result.QualityGateScan.ReviewPackageTargetFileCount);
        Assert.True(result.QualityGateScan.ParentUiBindingPassed);
        Assert.False(result.QualityGateScan.ReportOnlySmokeDetected);
        Assert.Equal(0, result.QualityGateScan.LinesOver500Count);
        Assert.Equal(0, result.QualityGateScan.FilesOver1000LinesCount);
        Assert.Equal(0, result.QualityGateScan.MinifiedSourceFileCount);
        Assert.True(result.QualityGateScan.AlphaRuntimeBootstrapLineCount > 3000);
        Assert.Equal(
            "read_only_hash_recorded_no_goal077_write_path",
            result.QualityGateScan.AlphaRuntimeBootstrapNoChangeStatus);
        Assert.Contains(
            result.QualityGateScan.Files,
            file => file.RelativePath == "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs");
    }

    [Fact]
    public void QualityGateRejectsMissingTargetsReportOnlySmokeAndParentMissingBind()
    {
        var root = Path.Combine(Path.GetTempPath(), "llmgc-goal077-quality-" + Guid.NewGuid().ToString("N"));
        try
        {
            WriteFile(
                root,
                "tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenPlayableReviewPackageMaterializationProductSmokeTests.cs",
                """
                public sealed class Smoke
                {
                    public string ImplementationStatus => "GREEN";
                }
                """);
            WriteReviewPackageBindingFilesWithoutParentBind(root);

            var scanner = new EditDrivenPlayableReviewPackageMaterializationQualityGateScanner();
            var scan = scanner.Scan(
                root,
                reviewPackageTargetFileCount: 0,
                evidencePayloads: new Dictionary<string, string>(StringComparer.Ordinal));
            var inventory = scanner.BuildWinFormsBindingInventory(root);

            Assert.False(scan.Passed);
            Assert.True(scan.ReportOnlySmokeDetected);
            Assert.False(scan.ParentUiBindingPassed);
            Assert.Contains(scan.Diagnostics, diagnostic => diagnostic.Code == "goal077.quality.review_package_targets_missing");
            Assert.Contains(scan.Diagnostics, diagnostic => diagnostic.Code == "goal077.quality.report_only_smoke");
            Assert.Contains(scan.Diagnostics, diagnostic => diagnostic.Code == "goal077.quality.parent_ui_non_binding");
            Assert.False(inventory.Passed);
            Assert.Contains(inventory.Diagnostics, diagnostic => diagnostic.Code == "goal077.winforms.review_package_service_missing");
            Assert.Contains(inventory.Diagnostics, diagnostic => diagnostic.Code == "goal077.winforms.review_package_control_bind_missing");
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static void WriteReviewPackageBindingFilesWithoutParentBind(string root)
    {
        WriteFile(
            root,
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/CampaignReviewPackageControl.cs",
            """
            public sealed class CampaignReviewPackageControl
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
                private object _reviewPackageTabPage;
                private CampaignReviewPackageControl _reviewPackageControl;
            }
            """);
        WriteFile(
            root,
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                + "CampaignAuthoringReviewWorkspacePageControl.cs",
            """
            public sealed partial class CampaignAuthoringReviewWorkspacePageControl
            {
                public void OnActivated()
                {
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
