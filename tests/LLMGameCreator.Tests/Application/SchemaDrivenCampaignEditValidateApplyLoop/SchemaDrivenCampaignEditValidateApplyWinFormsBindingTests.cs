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
        foreach (var groupId in SchemaDrivenCampaignEditVocabulary.RequiredBindingGroups)
        {
            Assert.Contains(result.WinFormsBindingInventory.Groups, group => group.GroupId == groupId);
        }
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
