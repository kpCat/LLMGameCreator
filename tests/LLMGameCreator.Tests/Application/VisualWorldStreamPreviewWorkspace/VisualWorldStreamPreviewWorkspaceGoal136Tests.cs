using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal136Tests
{
    [Fact]
    public async Task Goal136CanonicalRuntimePlayerCommandLoopSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        var handoffPath = Path.Combine(
            root,
            CanonicalRuntimePlayerCommandLoopVocabulary.DefaultSelectedCandidateHandoffPath);
        var packagePath = Path.Combine(
            root,
            CanonicalRuntimePlayerCommandLoopVocabulary.DefaultSelectedCandidatePackagePath);
        var request = new CanonicalRuntimePlayerCommandLoopRequest
        {
            CandidateId = CanonicalRuntimeSelectedCandidatePlaythroughArtifactService
                .ReadCandidateId(handoffPath),
            HandoffPath = Relative(root, handoffPath),
            PackagePath = Relative(root, packagePath),
            Goal134TranscriptPath =
                CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal134TranscriptPath,
            Goal134StateSummaryPath =
                CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal134StateSummaryPath,
            Goal135PlayerLoopPlanPath =
                CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal135PlayerLoopPlanPath,
            Goal135PlayerAdapterContractPath =
                CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal135PlayerAdapterContractPath
        };
        var package =
            CanonicalRuntimeSelectedCandidatePlaythroughArtifactService.LoadPackage(packagePath);
        var runtimeResult = CanonicalRuntimePlayerCommandLoopService
            .CreateDefault()
            .Execute(package, request);
        await new CanonicalRuntimePlayerCommandLoopArtifactService()
            .BuildAndWriteAsync(
                root,
                request,
                runtimeResult,
                unitySmoke: PassedUnitySmoke(root));

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(workspace.Catalog.Groups, item =>
            item.GroupId == "canonical_runtime_player_command_loop");
        var summary = Assert.Single(group.Entries, entry =>
            entry.ArtifactKind == "canonical_runtime_player_command_loop_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory.PageBindDisplaysCanonicalRuntimePlayerCommandLoop);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopGroupPresent);
        Assert.Equal(
            "minimal-map-game-balanced-baseline",
            workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopCandidateId);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopPassed);
        Assert.Equal(13, workspace.QualityGateScan.CanonicalRuntimePlayerCommandCount);
        Assert.Equal(
            workspace.QualityGateScan.CanonicalRuntimePlayerCommandCount,
            workspace.QualityGateScan.CanonicalRuntimePlayerSnapshotCount);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopRuntimeEventCount >= 10);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopAllRequiredCategoriesPresent);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopUnityConsumedSnapshots);
        Assert.False(workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopProjectionOnly);
        Assert.False(workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopUnityGameplayTruth);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopNoUnclassifiedErrors);
        Assert.Equal(
            CanonicalRuntimePlayerCommandLoopVocabulary.NormalCommand,
            workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopNormalCommand);
        Assert.Equal(
            CanonicalRuntimePlayerCommandLoopVocabulary.ReportMarkdownRelativePath,
            workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopReportPath);
        Assert.Equal(
            CanonicalRuntimePlayerCommandLoopVocabulary.MatrixResultRelativePath,
            workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopMatrixResultPath);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopManualUnityOptional);
        Assert.False(workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopAccepted);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopGoal136FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerCommandLoopQualityGatePassed);
        Assert.Equal(
            "minimal-map-game-balanced-baseline",
            summary.CanonicalRuntimePlayerCommandLoopCandidateId);

        Assert.Contains(
            "## Canonical Runtime Player Command Loop",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "playerCommandLoopPassed: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "unityPlayerConsumedCommandLoopSnapshots: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "accepted: false",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
    }

    private static CanonicalRuntimePlayerCommandLoopUnitySmoke PassedUnitySmoke(string root)
    {
        var snapshots = Path.Combine(
            root,
            CanonicalRuntimePlayerCommandLoopVocabulary.ProceduralOutputDirectory,
            CanonicalRuntimePlayerCommandLoopVocabulary.SnapshotsFileName);
        var result = Path.Combine(
            root,
            CanonicalRuntimePlayerCommandLoopVocabulary.ProceduralOutputDirectory,
            CanonicalRuntimePlayerCommandLoopVocabulary.ResultFileName);
        return new CanonicalRuntimePlayerCommandLoopUnitySmoke
        {
            UnityAvailable = true,
            SnapshotsPathExists = true,
            ResultPathExists = true,
            PassMarkerPresent = true,
            FailMarkerPresent = false,
            SnapshotContractPresent = true,
            UnityPlayerConsumedCommandLoopSnapshots = true,
            Passed = true,
            UnityPath = "test-unity",
            SnapshotsPath = Relative(root, snapshots),
            ResultPath = Relative(root, result),
            Status = "GREEN"
        };
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

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
