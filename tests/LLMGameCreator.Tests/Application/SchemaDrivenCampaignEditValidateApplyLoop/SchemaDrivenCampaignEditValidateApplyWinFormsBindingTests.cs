using System.Runtime.ExceptionServices;
using LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;
using LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.SchemaDrivenCampaignEditValidateApplyLoop;

public sealed class SchemaDrivenCampaignEditValidateApplyWinFormsBindingTests
{
    [Fact]
    public void WinFormsBindingInventoryHasRequiredGroups()
    {
        var result = new SchemaDrivenCampaignEditEvidenceService().Build(ProjectRoot());

        Assert.True(result.WinFormsBindingInventory.Passed);
        Assert.True(result.WinFormsBindingInventory.NavigationRegistered);
        Assert.True(result.WinFormsBindingInventory.ParentPageEditLoopTabDeclared);
        Assert.True(result.WinFormsBindingInventory.ParentPageEditEvidenceServiceLoaded);
        Assert.True(result.WinFormsBindingInventory.ParentPageEditLoopBound);
        Assert.True(result.WinFormsBindingInventory.ParentPageActivationBindsGoal075Data);
        foreach (var groupId in SchemaDrivenCampaignEditVocabulary.RequiredBindingGroups)
        {
            Assert.Contains(result.WinFormsBindingInventory.Groups, group => group.GroupId == groupId);
        }
    }

    [Fact]
    public void WorkspaceActivationBindsContainedGoal075EditLoop()
    {
        RunSta(() =>
        {
            using var page = new CampaignAuthoringReviewWorkspacePageControl();

            page.OnActivated();

            var editLoop = RequiredPrivateField<CampaignEditValidateApplyLoopControl>(page, "_editLoopControl");
            var editResult = RequiredPrivateField<SchemaDrivenCampaignEditBuildResult>(editLoop, "_result");
            Assert.Equal("GREEN", editResult.Report.ImplementationStatus);
            Assert.False(editResult.Report.Accepted);
            Assert.Equal(9, editResult.DiffMatrix.RowCount);
            Assert.True(editResult.WinFormsBindingInventory.ParentPageActivationBindsGoal075Data);
        });
    }

    [Fact]
    public void WinFormsControlsCanBeConstructedAndBound()
    {
        var root = ProjectRoot();
        var workspaceResult = new SchemaDrivenCampaignWorkspaceEvidenceService().Build(root);
        var editResult = new SchemaDrivenCampaignEditEvidenceService().Build(root);

        RunSta(() =>
        {
            using var page = new CampaignAuthoringReviewWorkspacePageControl();
            using var editLoop = new CampaignEditValidateApplyLoopControl();
            using var fields = new CampaignEditFieldSummaryControl();
            using var validation = new CampaignEditValidationControl();
            using var applyRollback = new CampaignEditApplyRollbackControl();

            page.Bind(workspaceResult, editResult);
            editLoop.Bind(editResult);
            editLoop.SelectRow(editResult.DiffMatrix.Rows[0].RowId);
            fields.Bind(editResult.FieldCatalog, editResult.ChangeSetCatalog, editResult.DiffMatrix.Rows[0].RowId);
            validation.Bind(editResult.ValidationMatrix, editResult.InvalidMatrix, editResult.DiffMatrix.Rows[0].RowId);
            applyRollback.Bind(
                editResult.ApplyRollbackLedger,
                editResult.DiffMatrix,
                editResult.PreviewExportRefreshPayload,
                editResult.DiffMatrix.Rows[0].RowId);
        });
    }

    [Fact]
    public void WinFormsBindingInventoryRejectsEditLoopTabWithoutParentBind()
    {
        var root = Path.Combine(Path.GetTempPath(), "llmgc-goal075-binding-" + Guid.NewGuid().ToString("N"));
        try
        {
            WriteFile(
                root,
                "src/LLMGameCreator.WinForms/CompositionRoot.cs",
                "public sealed class CompositionRoot { CampaignAuthoringReviewWorkspacePageControl? Page; }");
            WriteFile(
                root,
                "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/"
                    + "CampaignAuthoringReviewWorkspacePageControl.Designer.cs",
                """
                public sealed partial class CampaignAuthoringReviewWorkspacePageControl
                {
                    private object _editLoopTabPage;
                    private CampaignEditValidateApplyLoopControl _editLoopControl;
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
            WriteRequiredControl(root, "CampaignRowSelectorControl");
            WriteRequiredControl(root, "CampaignEditFieldSummaryControl");
            WriteRequiredControl(root, "CampaignEditValidationControl");
            WriteRequiredControl(root, "CampaignEditApplyRollbackControl");

            var inventory = new SchemaDrivenCampaignEditQualityGateScanner().BuildWinFormsBindingInventory(root);

            Assert.False(inventory.Passed);
            Assert.True(inventory.ParentPageEditLoopTabDeclared);
            Assert.False(inventory.ParentPageActivationBindsGoal075Data);
            Assert.Contains(
                inventory.Diagnostics,
                diagnostic => diagnostic.Code == "goal075.winforms.parent_activation_binding_missing");
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

    private static void WriteRequiredControl(string root, string controlName) =>
        WriteFile(
            root,
            "src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/" + controlName + ".cs",
            "public sealed class " + controlName + " { }");

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
