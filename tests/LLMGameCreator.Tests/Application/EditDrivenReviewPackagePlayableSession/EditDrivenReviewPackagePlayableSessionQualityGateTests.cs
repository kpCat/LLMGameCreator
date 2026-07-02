using LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;
using Xunit;

namespace LLMGameCreator.Tests.Application.EditDrivenReviewPackagePlayableSession;

public sealed class EditDrivenReviewPackagePlayableSessionQualityGateTests
{
    [Fact]
    public void QualityGateScansGoal078FilesAndAlphaBootstrapWithoutBloat()
    {
        var result = new EditDrivenReviewPackagePlayableSessionEvidenceService().Build(ProjectRoot());

        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.QualityGateScan.ParentUiBindingPassed);
        Assert.False(result.QualityGateScan.ReportOnlySmokeDetected);
        Assert.Equal(0, result.QualityGateScan.LinesOver500Count);
        Assert.Equal(0, result.QualityGateScan.FilesOver1000LinesCount);
        Assert.Equal(0, result.QualityGateScan.MinifiedSourceFileCount);
        Assert.True(result.QualityGateScan.AlphaRuntimeBootstrapUnchanged);
        Assert.Contains(
            result.QualityGateScan.Files,
            file => file.RelativePath == "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs");
    }

    [Fact]
    public void QualityGateRejectsPlaySessionTabWithoutParentBindAndReportOnlySmoke()
    {
        var root = Path.Combine(Path.GetTempPath(), "llmgc-goal078-quality-" + Guid.NewGuid().ToString("N"));
        try
        {
            WriteFile(
                root,
                "tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenReviewPackagePlayableSessionProductSmokeTests.cs",
                """
                public sealed class Smoke
                {
                    public string ImplementationStatus => "GREEN";
                }
                """);
            WritePlaySessionBindingFilesWithoutParentBind(root);

            var scanner = new EditDrivenReviewPackagePlayableSessionQualityGateScanner();
            var scan = scanner.Scan(
                root,
                expectedAlphaRuntimeBootstrapHash: "",
                evidencePayloads: new Dictionary<string, string>(StringComparer.Ordinal));
            var inventory = scanner.BuildWinFormsBindingInventory(root);

            Assert.False(scan.Passed);
            Assert.True(scan.ReportOnlySmokeDetected);
            Assert.False(scan.ParentUiBindingPassed);
            Assert.Contains(scan.Diagnostics, diagnostic => diagnostic.Code == "goal078.quality.report_only_smoke");
            Assert.Contains(scan.Diagnostics, diagnostic => diagnostic.Code == "goal078.quality.parent_ui_non_binding");
            Assert.False(inventory.Passed);
            Assert.True(inventory.ParentPagePlaySessionTabDeclared);
            Assert.False(inventory.ParentPageActivationBindsGoal078Data);
            Assert.Contains(inventory.Diagnostics, diagnostic => diagnostic.Code == "goal078.winforms.play_session_service_missing");
            Assert.Contains(inventory.Diagnostics, diagnostic => diagnostic.Code == "goal078.winforms.play_session_control_bind_missing");
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static void WritePlaySessionBindingFilesWithoutParentBind(string root)
    {
        WriteFile(
            root,
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                + "CampaignReviewPackagePlaySessionControl.cs",
            """
            public sealed class CampaignReviewPackagePlaySessionControl
            {
                public void Bind(object result)
                {
                }
            }
            """);
        WriteFile(
            root,
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                + "CampaignAuthoringReviewWorkspacePageControl.Designer.cs",
            """
            public sealed partial class CampaignAuthoringReviewWorkspacePageControl
            {
                private object _playSessionTabPage;
                private CampaignReviewPackagePlaySessionControl _playSessionControl;
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
