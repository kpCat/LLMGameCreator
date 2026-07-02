using System.Runtime.ExceptionServices;
using LLMGameCreator.Application.Design.EditDrivenPlayableReviewPackageMaterialization;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.EditDrivenPlayableReviewPackageMaterialization;

public sealed class EditDrivenPlayableReviewPackageMaterializationWinFormsBindingTests
{
    [Fact]
    public void WinFormsBindingInventoryFindsGoal077ControlAndParentActivation()
    {
        var result = new EditDrivenPlayableReviewPackageMaterializationEvidenceService().Build(ProjectRoot());

        Assert.True(result.WinFormsBindingInventory.Passed);
        Assert.True(result.WinFormsBindingInventory.ParentPageReviewPackageTabDeclared);
        Assert.True(result.WinFormsBindingInventory.ParentPageReviewPackageEvidenceServiceLoaded);
        Assert.True(result.WinFormsBindingInventory.ParentPageReviewPackageControlBound);
        Assert.True(result.WinFormsBindingInventory.ParentPageActivationBindsGoal077Data);
        Assert.Contains(result.WinFormsBindingInventory.Groups, group => group.GroupId == "review_package_status");
    }

    [Fact]
    public void WorkspaceActivationBindsGoal077ReviewPackageControl()
    {
        RunSta(() =>
        {
            using var page = new CampaignAuthoringReviewWorkspacePageControl();

            page.OnActivated();

            var control = RequiredPrivateField<CampaignReviewPackageControl>(page, "_reviewPackageControl");
            var result = RequiredPrivateField<EditDrivenPlayableReviewPackageMaterializationBuildResult>(control, "_result");
            Assert.Equal("GREEN", result.Report.ImplementationStatus);
            Assert.False(result.Report.Accepted);
            Assert.True(result.StagedPackageReadProof.Passed);
            Assert.True(result.TamperNegativeProof.Passed);
            Assert.Equal(18, result.PackageFileLedger.Files.Count(file => file.Role == "target"));
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
