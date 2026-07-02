using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows.Forms;
using LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewBridge;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.EditDrivenGamePackageRuntimePreviewBridge;

public sealed class EditDrivenGamePackageRuntimePreviewBridgeQualityGateTests
{
    [Fact]
    public void QualityGateScansGoal080FilesAndRejectsRawSourceShapes()
    {
        var result = new EditDrivenGamePackageRuntimePreviewBridgeEvidenceService().Build(ProjectRoot());

        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.QualityGateScan.ParentUiBindingPassed);
        Assert.False(result.QualityGateScan.ReportOnlySmokeDetected);
        Assert.Equal(0, result.QualityGateScan.LinesOver500Count);
        Assert.Equal(0, result.QualityGateScan.FilesOver1000LinesCount);
        Assert.Equal(0, result.QualityGateScan.MinifiedSourceFileCount);
        Assert.Equal(0, result.QualityGateScan.ZeroLfSourceCount);
        Assert.Equal(0, result.QualityGateScan.CrOnlySourceCount);
        Assert.True(result.QualityGateScan.SyntheticCrOnlySourceRejected);
        Assert.True(result.QualityGateScan.SyntheticZeroLfOneLineSourceRejected);
        Assert.True(EditDrivenGamePackageRuntimePreviewBridgeQualityGateScanner.RejectsSuspiciousRawSourceBytes(
            Encoding.UTF8.GetBytes("public sealed class Broken\r{\r}\r")));
        Assert.True(EditDrivenGamePackageRuntimePreviewBridgeQualityGateScanner.RejectsSuspiciousRawSourceBytes(
            Encoding.UTF8.GetBytes("public sealed class Broken { public string V => \"" + new string('x', 520) + "\"; }")));
    }

    [Fact]
    public void WinFormsBindingInventoryFindsGoal080ControlAndParentActivation()
    {
        var result = new EditDrivenGamePackageRuntimePreviewBridgeEvidenceService().Build(ProjectRoot());

        Assert.True(result.WinFormsBindingInventory.Passed);
        Assert.True(result.WinFormsBindingInventory.ParentPageRuntimePreviewBridgeTabDeclared);
        Assert.True(result.WinFormsBindingInventory.ParentPageRuntimePreviewBridgeServiceLoaded);
        Assert.True(result.WinFormsBindingInventory.ParentPageRuntimePreviewBridgeControlBound);
        Assert.True(result.WinFormsBindingInventory.ParentPageActivationBindsGoal080Data);
        Assert.Contains(result.WinFormsBindingInventory.Groups, group => group.GroupId == "goal080_runtime_preview_bridge_tab");
    }

    [Fact]
    public void WorkspaceActivationBindsGoal080RuntimePreviewBridgeControl()
    {
        RunSta(() =>
        {
            using var page = new CampaignAuthoringReviewWorkspacePageControl();

            page.OnActivated();

            var control = RequiredPrivateField<CampaignGamePackageRuntimePreviewBridgeControl>(
                page,
                "_runtimePreviewBridgeControl");
            var statusLabel = RequiredPrivateField<Label>(control, "_statusLabel");
            Assert.Contains(EditDrivenGamePackageRuntimePreviewBridgeVocabulary.FinalGate, statusLabel.Text);
            Assert.Contains("status=GREEN", statusLabel.Text);
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
