using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows.Forms;
using LLMGameCreator.Application.Design.EditDrivenUnityAlphaStreamingAssetsHandoff;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.EditDrivenUnityAlphaStreamingAssetsHandoff;

[Collection(EditDrivenUnityAlphaStreamingAssetsHandoffTestCollection.Name)]
public sealed class EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateTests
{
    [Fact]
    public async Task QualityGateScansSourcePayloadAndAlphaBootstrapBaseline()
    {
        var result = (await new EditDrivenUnityAlphaStreamingAssetsHandoffEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;

        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.QualityGateScan.ParentUiBindingPassed);
        Assert.False(result.QualityGateScan.EvidenceContainsAbsoluteLocalPaths);
        Assert.False(result.QualityGateScan.EvidenceContainsTimestampLikeValues);
        Assert.False(result.QualityGateScan.EvidenceContainsHeavyLogs);
        Assert.False(result.QualityGateScan.EvidenceContainsScratchTamperFiles);
        Assert.Equal(0, result.QualityGateScan.LinesOver500Count);
        Assert.Equal(0, result.QualityGateScan.FilesOver1000LinesCount);
        Assert.Equal(0, result.QualityGateScan.MinifiedSourceFileCount);
        Assert.Equal(0, result.QualityGateScan.ZeroLfSourceCount);
        Assert.Equal(0, result.QualityGateScan.CrOnlySourceCount);
        Assert.True(result.QualityGateScan.SyntheticCrOnlySourceRejected);
        Assert.True(result.QualityGateScan.SyntheticZeroLfOneLineSourceRejected);
        Assert.True(result.QualityGateScan.AlphaRuntimeBootstrapUnchanged);
        Assert.Equal(3672, result.QualityGateScan.AlphaRuntimeBootstrapAfterLineCount);
        Assert.Equal(
            EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.AlphaRuntimeBootstrapExpectedHash,
            result.QualityGateScan.AlphaRuntimeBootstrapAfterHash);
        Assert.True(result.QualityGateScan.UnityProbeBelow300Lines);
        Assert.True(result.QualityGateScan.UnityProbeUsesStreamingAssetsPath);
        Assert.True(result.QualityGateScan.UnityProbeNoRuntimeProviderLlmMediaDependency);
    }

    [Fact]
    public void RawSourceAndBootstrapGuardsRejectSyntheticBadInputs()
    {
        Assert.True(EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScanner.RejectsSuspiciousRawSourceBytes(
            Encoding.UTF8.GetBytes("public sealed class Broken\r{\r}\r")));
        Assert.True(EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScanner.RejectsSuspiciousRawSourceBytes(
            Encoding.UTF8.GetBytes("public sealed class Broken { public string V => \"" + new string('x', 520) + "\"; }")));
        Assert.False(EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScanner.AlphaRuntimeBootstrapMatchesBaseline(
            Encoding.UTF8.GetBytes("public sealed class Changed\n{\n}\n")));
    }

    [Fact]
    public async Task WinFormsBindingInventoryFindsGoal082ControlAndParentActivation()
    {
        var result = (await new EditDrivenUnityAlphaStreamingAssetsHandoffEvidenceService()
            .BuildAndWriteAsync(ProjectRoot())).Result;

        Assert.True(result.WinFormsBindingInventory.Passed);
        Assert.True(result.WinFormsBindingInventory.ParentPageHandoffTabDeclared);
        Assert.True(result.WinFormsBindingInventory.ParentPageHandoffServiceLoaded);
        Assert.True(result.WinFormsBindingInventory.ParentPageHandoffControlBound);
        Assert.True(result.WinFormsBindingInventory.ParentPageActivationBindsGoal082Data);
        Assert.Contains(
            result.WinFormsBindingInventory.Groups,
            group => group.GroupId == "goal082_unity_alpha_streamingassets_handoff_tab");
    }

    [Fact]
    public void WorkspaceActivationBindsGoal082HandoffControl()
    {
        RunSta(() =>
        {
            using var page = new CampaignAuthoringReviewWorkspacePageControl();

            page.OnActivated();

            var control = RequiredPrivateField<CampaignUnityAlphaStreamingAssetsHandoffControl>(
                page,
                "_unityAlphaStreamingAssetsHandoffControl");
            var result = RequiredPrivateField<EditDrivenUnityAlphaStreamingAssetsHandoffBuildResult>(
                control,
                "_result");
            var statusLabel = RequiredPrivateField<Label>(control, "_statusLabel");
            Assert.Contains(EditDrivenUnityAlphaStreamingAssetsHandoffVocabulary.FinalGate, statusLabel.Text);
            Assert.Contains("status=GREEN", statusLabel.Text);
            Assert.True(result.ProbeReadProof.Passed);
            Assert.True(result.NegativeProof.Passed);
            Assert.True(result.CommandTranscriptProof.Passed);
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
