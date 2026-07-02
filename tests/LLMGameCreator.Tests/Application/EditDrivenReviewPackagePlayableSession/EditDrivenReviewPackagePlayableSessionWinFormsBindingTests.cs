using System.Runtime.ExceptionServices;
using LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.EditDrivenReviewPackagePlayableSession;

public sealed class EditDrivenReviewPackagePlayableSessionWinFormsBindingTests
{
    [Fact]
    public void WinFormsBindingInventoryFindsGoal078ControlAndParentActivation()
    {
        var result = new EditDrivenReviewPackagePlayableSessionEvidenceService().Build(ProjectRoot());

        Assert.True(result.WinFormsBindingInventory.Passed);
        Assert.True(result.WinFormsBindingInventory.ParentPagePlaySessionTabDeclared);
        Assert.True(result.WinFormsBindingInventory.ParentPagePlaySessionEvidenceServiceLoaded);
        Assert.True(result.WinFormsBindingInventory.ParentPagePlaySessionControlBound);
        Assert.True(result.WinFormsBindingInventory.ParentPageActivationBindsGoal078Data);
        Assert.Contains(result.WinFormsBindingInventory.Groups, group => group.GroupId == "review_package_play_session_status");
    }

    [Fact]
    public void WorkspaceActivationBindsGoal078PlaySessionControl()
    {
        RunSta(() =>
        {
            using var page = new CampaignAuthoringReviewWorkspacePageControl();

            page.OnActivated();

            var control = RequiredPrivateField<CampaignReviewPackagePlaySessionControl>(page, "_playSessionControl");
            var result = RequiredPrivateField<EditDrivenReviewPackagePlayableSessionBuildResult>(control, "_result");
            Assert.Equal("GREEN", result.Report.ImplementationStatus);
            Assert.False(result.Report.Accepted);
            Assert.True(result.PackageReadProof.Passed);
            Assert.True(result.ReplayProof.Passed);
            Assert.True(result.TamperNegativeProof.Passed);
            Assert.Equal(18, result.ActionLog.Actions.Count(action => action.ActionType == "inspect_target"));
        });
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
