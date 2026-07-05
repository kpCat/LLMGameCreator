using System.Security.Cryptography;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaHumanResultRevalidation;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class OfflineGeoworldAlphaHumanResultRevalidationProductSmokeTests
{
    [Fact]
    public async Task Goal115OfflineGeoworldAlphaHumanResultRevalidationProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var manualResultPath = Path.Combine(
            repoRoot,
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ManualResultRelativePath
                .Replace('/', Path.DirectorySeparatorChar));
        var manualResultExists = File.Exists(manualResultPath);
        var manualResultSha = manualResultExists ? Sha256File(manualResultPath) : string.Empty;

        var write = await new OfflineGeoworldAlphaHumanResultRevalidationService()
            .BuildAndWriteAsync(repoRoot);
        var result = write.Result;

        Assert.False(result.DecisionSnapshot.AcceptedByCodex);
        Assert.True(result.DecisionSnapshot.HumanAcceptanceStillRequired);
        Assert.True(result.DecisionSnapshot.ManualGateRemainsHumanDecision);
        Assert.True(result.DecisionSnapshot.ManualInputNotCommitted);
        Assert.True(result.DecisionSnapshot.NotFinalReleaseOrRuntimeBuild);
        Assert.True(result.DecisionSnapshot.NoRuntimeProviderOrNetworkChanges);
        Assert.True(result.DecisionSnapshot.NoUnityFileChangesRequired);
        Assert.True(result.NegativeProof.Passed);
        AssertFilesExist(
            write.ProceduralOutputDirectoryPath,
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.RequiredProceduralFileNames);
        AssertFilesExist(
            write.ExportPackageDirectoryPath,
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.RequiredExportFileNames);
        Assert.True(File.Exists(write.DocumentationPath));
        Assert.All(write.WrittenFiles, path =>
            Assert.False(path.StartsWith(".llmgc/manual/", StringComparison.Ordinal), path));
        Assert.DoesNotContain(
            write.WrittenFiles,
            path => path.StartsWith("unity/", StringComparison.Ordinal));

        if (manualResultExists)
        {
            Assert.Equal("GREEN", result.QualityGateScan.ImplementationStatus);
            Assert.True(result.QualityGateScan.Passed, string.Join(
                Environment.NewLine,
                result.QualityGateScan.Diagnostics));
            Assert.Equal(
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusGreenCandidate,
                result.DecisionSnapshot.DecisionStatus);
            Assert.True(result.DecisionSnapshot.AcceptableCandidate);
            Assert.Equal(manualResultSha, result.DecisionSnapshot.ManualResultSha256);
            Assert.Equal(12, result.DecisionSnapshot.StepSummary.RequiredStepCount);
            Assert.Equal(12, result.DecisionSnapshot.StepSummary.PassedCount);
            Assert.Equal(
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.RecommendedHumanDecisionReady,
                result.DecisionSnapshot.RecommendedHumanDecision);
        }
        else
        {
            Assert.Equal("BLOCKED", result.QualityGateScan.ImplementationStatus);
            Assert.False(result.QualityGateScan.Passed);
            Assert.Equal(
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusPending,
                result.DecisionSnapshot.DecisionStatus);
            Assert.False(result.DecisionSnapshot.AcceptableCandidate);
        }

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(repoRoot);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_alpha_human_result_revalidation");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind
                    == "offline_geoworld_alpha_human_result_revalidation_workspace_summary");

        Assert.Equal(
            result.DecisionSnapshot.DecisionStatus,
            summary.OfflineGeoworldAlphaHumanResultRevalidationDecisionStatus);
        Assert.False(summary.OfflineGeoworldAlphaHumanResultRevalidationAcceptedByCodex);
        Assert.True(summary.OfflineGeoworldAlphaHumanResultRevalidationHumanAcceptanceStillRequired);
        Assert.True(summary.OfflineGeoworldAlphaHumanResultRevalidationManualGateRemainsHumanDecision);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysOfflineGeoworldAlphaHumanResultRevalidation);
        Assert.Contains(
            "offlineGeoworldAlphaHumanResultRevalidationDecisionStatus: "
            + result.DecisionSnapshot.DecisionStatus,
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
    }

    private static void AssertFilesExist(string directory, IReadOnlyList<string> fileNames)
    {
        foreach (var fileName in fileNames)
        {
            Assert.True(File.Exists(Path.Combine(directory, fileName)), fileName);
        }
    }

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }
}
