using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows.Forms;
using LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewPlaythrough;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.EditDrivenGamePackageRuntimePreviewPlaythrough;

public sealed class EditDrivenGamePackageRuntimePreviewPlaythroughQualityGateTests
{
    [Fact]
    public void QualityGateScansGoal081FilesEvidenceAndSourceShapes()
    {
        var result = new EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService().Build(ProjectRoot());

        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.QualityGateScan.ParentUiBindingPassed);
        Assert.False(result.QualityGateScan.ReportOnlySmokeDetected);
        Assert.False(result.QualityGateScan.EvidenceContainsAbsoluteLocalPaths);
        Assert.False(result.QualityGateScan.EvidenceContainsTimestampLikeValues);
        Assert.False(result.QualityGateScan.EvidenceContainsHeavyLogs);
        Assert.False(result.QualityGateScan.EvidenceContainsScratchTamperFiles);
        Assert.False(result.QualityGateScan.ForbiddenAreaEvidenceDetected);
        Assert.Equal(0, result.QualityGateScan.LinesOver500Count);
        Assert.Equal(0, result.QualityGateScan.FilesOver1000LinesCount);
        Assert.Equal(0, result.QualityGateScan.MinifiedSourceFileCount);
        Assert.Equal(0, result.QualityGateScan.ZeroLfSourceCount);
        Assert.Equal(0, result.QualityGateScan.CrOnlySourceCount);
        Assert.True(result.QualityGateScan.SyntheticCrOnlySourceRejected);
        Assert.True(result.QualityGateScan.SyntheticZeroLfOneLineSourceRejected);
        Assert.True(result.QualityGateScan.AlphaRuntimeBootstrapUnchanged);
        Assert.True(EditDrivenGamePackageRuntimePreviewPlaythroughQualityGateScanner.RejectsSuspiciousRawSourceBytes(
            Encoding.UTF8.GetBytes("public sealed class Broken\r{\r}\r")));
        Assert.True(EditDrivenGamePackageRuntimePreviewPlaythroughQualityGateScanner.RejectsSuspiciousRawSourceBytes(
            Encoding.UTF8.GetBytes("public sealed class Broken { public string V => \"" + new string('x', 520) + "\"; }")));
    }

    [Fact]
    public void WinFormsBindingInventoryFindsGoal081ControlAndParentActivation()
    {
        var result = new EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService().Build(ProjectRoot());

        Assert.True(result.WinFormsBindingInventory.Passed);
        Assert.True(result.WinFormsBindingInventory.ParentPagePlaythroughTabDeclared);
        Assert.True(result.WinFormsBindingInventory.ParentPagePlaythroughServiceLoaded);
        Assert.True(result.WinFormsBindingInventory.ParentPagePlaythroughControlBound);
        Assert.True(result.WinFormsBindingInventory.ParentPageActivationBindsGoal081Data);
        Assert.Contains(result.WinFormsBindingInventory.Groups, group => group.GroupId == "goal081_runtime_preview_playthrough_tab");
    }

    [Fact]
    public void WorkspaceActivationBindsGoal081RuntimePreviewPlaythroughControl()
    {
        RunSta(() =>
        {
            using var page = new CampaignAuthoringReviewWorkspacePageControl();

            page.OnActivated();

            var control = RequiredPrivateField<CampaignGamePackageRuntimePreviewPlaythroughControl>(
                page,
                "_runtimePreviewPlaythroughControl");
            var result = RequiredPrivateField<EditDrivenGamePackageRuntimePreviewPlaythroughBuildResult>(
                control,
                "_result");
            var statusLabel = RequiredPrivateField<Label>(control, "_statusLabel");
            Assert.Contains(EditDrivenGamePackageRuntimePreviewPlaythroughVocabulary.FinalGate, statusLabel.Text);
            Assert.Contains("status=GREEN", statusLabel.Text);
            Assert.True(result.PackageReadProof.Passed);
            Assert.True(result.Transcript.Passed);
            Assert.True(result.CoverageLedger.Passed);
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
