using System.Runtime.ExceptionServices;
using LLMGameCreator.Application.Design.EditDrivenPlayablePreviewRefresh;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.EditDrivenPlayablePreviewRefresh;

public sealed class EditDrivenPlayablePreviewRefreshWinFormsBindingTests
{
    [Fact]
    public void WinFormsBindingInventoryFindsGoal076ControlAndParentActivation()
    {
        var result = new EditDrivenPlayablePreviewRefreshEvidenceService().Build(ProjectRoot());

        Assert.True(result.WinFormsBindingInventory.Passed);
        Assert.True(result.WinFormsBindingInventory.ParentPageRefreshTabDeclared);
        Assert.True(result.WinFormsBindingInventory.ParentPageRefreshEvidenceServiceLoaded);
        Assert.True(result.WinFormsBindingInventory.ParentPageRefreshControlBound);
        Assert.True(result.WinFormsBindingInventory.ParentPageActivationBindsGoal076Data);
        Assert.Contains(result.WinFormsBindingInventory.Groups, group => group.GroupId == "playable_refresh_status");
    }

    [Fact]
    public void WorkspaceActivationBindsGoal076PlayableRefreshControl()
    {
        RunSta(() =>
        {
            using var page = new CampaignAuthoringReviewWorkspacePageControl();

            page.OnActivated();

            var control = RequiredPrivateField<CampaignPlayableRefreshControl>(page, "_playableRefreshControl");
            var refreshResult = RequiredPrivateField<EditDrivenPlayablePreviewRefreshBuildResult>(control, "_result");
            Assert.Equal("GREEN", refreshResult.Report.ImplementationStatus);
            Assert.False(refreshResult.Report.Accepted);
            Assert.True(refreshResult.StagedHandoffProof.Passed);
            Assert.True(refreshResult.TamperNegativeProof.Passed);
        });
    }

    [Fact]
    public void BindingInventoryRejectsRefreshTabWithoutParentBind()
    {
        var root = Path.Combine(Path.GetTempPath(), "llmgc-goal076-binding-" + Guid.NewGuid().ToString("N"));
        try
        {
            WriteFile(
                root,
                "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                    + "CampaignPlayableRefreshControl.cs",
                "public sealed class CampaignPlayableRefreshControl { }");
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
                    public void OnActivated()
                    {
                    }
                }
                """);

            var inventory = new EditDrivenPlayablePreviewRefreshQualityGateScanner().BuildWinFormsBindingInventory(root);

            Assert.False(inventory.Passed);
            Assert.True(inventory.ParentPageRefreshTabDeclared);
            Assert.False(inventory.ParentPageActivationBindsGoal076Data);
            Assert.Contains(inventory.Diagnostics, diagnostic => diagnostic.Code == "goal076.winforms.refresh_service_missing");
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static void RunSta(Action action)
    {
        Exception? caught = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                caught = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (caught is not null)
        {
            ExceptionDispatchInfo.Capture(caught).Throw();
        }
    }

    private static T RequiredPrivateField<T>(object owner, string fieldName)
    {
        var field = owner.GetType().GetField(
            fieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(field);
        var value = field!.GetValue(owner);
        Assert.NotNull(value);
        return Assert.IsType<T>(value);
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
