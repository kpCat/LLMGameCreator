using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceOrchestrator;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class OfflineGeoworldAlphaSliceOrchestratorProductSmokeTests
{
    private static readonly HashSet<string> ForbiddenExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif",
        ".bmp",
        ".wav",
        ".ogg",
        ".mp3",
        ".mp4",
        ".asset",
        ".bytes"
    };

    [Fact]
    public void Goal108OfflineGeoworldAlphaSliceOrchestratorProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var result = new OfflineGeoworldAlphaSliceOrchestratorEvidenceService()
            .Build(repoRoot);
        var outputDirectoryPath = Path.Combine(
            repoRoot,
            OfflineGeoworldAlphaSliceVocabulary.RelativeOutputDirectory);
        var streamingAssetsDirectoryPath = Path.Combine(
            repoRoot,
            OfflineGeoworldAlphaSliceVocabulary.StreamingAssetsRelativeRoot);

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.QualityGateScan.Passed, string.Join(Environment.NewLine, result.QualityGateScan.Diagnostics));
        Assert.Equal(5, result.Manifest.PayloadFileCount);
        Assert.Equal(7, result.Manifest.ComponentCount);
        Assert.Equal(7, result.Manifest.ReadyComponentCount);
        Assert.True(result.Manifest.ObjectiveCount >= 5);
        Assert.Equal(result.Manifest.ObjectiveCount, result.Manifest.CompletedObjectiveCount);
        Assert.Equal("completed", result.Manifest.FinalStatus);
        Assert.True(result.Manifest.MetadataOnly);
        Assert.True(result.Manifest.AlphaToolingOnly);
        Assert.False(result.Manifest.ContainsRuntimeExecution);
        Assert.False(result.Manifest.ContainsProviderCalls);
        Assert.False(result.Manifest.ContainsFinalGameplay);
        Assert.False(result.Manifest.ContainsRealGeodataFetch);
        Assert.True(result.Manifest.AlphaRuntimeBootstrapUnchanged);
        Assert.True(result.SimulatedProof.Passed);
        Assert.True(result.SimulatedProof.HistoricalArtifactsUnchanged);
        Assert.True(result.NegativeProof.Passed);

        AssertFilesExist(outputDirectoryPath, OfflineGeoworldAlphaSliceVocabulary.RequiredPayloadFileNames);
        AssertFilesExist(streamingAssetsDirectoryPath, OfflineGeoworldAlphaSliceVocabulary.RequiredPayloadFileNames);
        Assert.Equal(
            OfflineGeoworldAlphaSliceVocabulary.RequiredPayloadFileNames.Count,
            Directory.EnumerateFiles(streamingAssetsDirectoryPath, "*.json", SearchOption.TopDirectoryOnly).Count());

        var changedFiles = Directory
            .EnumerateFiles(outputDirectoryPath, "*", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(streamingAssetsDirectoryPath, "*", SearchOption.AllDirectories))
            .ToList();
        Assert.DoesNotContain(
            changedFiles,
            path => ForbiddenExtensions.Contains(Path.GetExtension(path)));

        var combinedText = string.Join(Environment.NewLine, changedFiles.Select(File.ReadAllText));
        Assert.DoesNotContain(repoRoot, combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("UnityWebRequest", combinedText, StringComparison.Ordinal);
        Assert.DoesNotContain("HttpClient", combinedText, StringComparison.Ordinal);
        Assert.DoesNotContain("\"rawGeodataIncluded\": true", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"noRawGeodata\": false", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".geojson", combinedText, StringComparison.OrdinalIgnoreCase);

        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            streamingAssetsDirectoryPath,
            OfflineGeoworldAlphaSliceVocabulary.ManifestFileName)));
        using var components = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            streamingAssetsDirectoryPath,
            OfflineGeoworldAlphaSliceVocabulary.ComponentsFileName)));
        using var quality = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            outputDirectoryPath,
            OfflineGeoworldAlphaSliceVocabulary.QualityGateScanFileName)));

        Assert.Equal("GREEN", manifest.RootElement.GetProperty("implementationStatus").GetString());
        Assert.False(manifest.RootElement.GetProperty("accepted").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("alphaRuntimeBootstrapUnchanged").GetBoolean());
        Assert.Equal(7, components.RootElement.GetProperty("components").GetArrayLength());
        Assert.True(quality.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noScenePrefabSettingsProjectPackageMutation").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noExternalDependencyOrNewInputSystemMarkers").GetBoolean());

        Assert.True(result.UnityScriptInventory.Passed);
        Assert.True(result.UnityScriptInventory.ReadsApplicationStreamingAssetsPath);
        Assert.True(result.UnityScriptInventory.FindsGoal101To107Controllers);
        Assert.True(result.UnityScriptInventory.DoesNotReferenceAlphaRuntimeBootstrap);
        Assert.True(result.EditorWindowInventory.Passed);
        Assert.True(result.EditorWindowInventory.MenuItemMarkerPresent);
        Assert.True(result.EditorWindowInventory.CreateRigMethodPresent);
        Assert.True(result.EditorWindowInventory.ClearRigMethodPresent);
        Assert.True(result.EditorWindowInventory.VerifyMethodPresent);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(repoRoot);
        var alphaGroup = Assert.Single(
            workspace.Catalog.Groups,
            group => group.GroupId == "offline_geoworld_alpha_slice");
        Assert.Contains(
            alphaGroup.Entries,
            entry => entry.ArtifactKind == "offline_geoworld_alpha_slice_workspace_summary");
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaSliceGroupPresent);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaSliceFinalProofPassed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaSliceQualityGatePassed);

        Assert.DoesNotContain(
            result.QualityGateScan.ExpectedChangedPathPrefixes,
            path => path.StartsWith("src/LLMGameCreator.Runtime", StringComparison.Ordinal));
        Assert.DoesNotContain(
            result.QualityGateScan.ExpectedChangedPathPrefixes,
            path => path.StartsWith("src/LLMGameCreator.GamePackage", StringComparison.Ordinal));
        Assert.DoesNotContain(
            result.QualityGateScan.ExpectedChangedPathPrefixes,
            path => path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Goal108AAlphaSliceSourceSplitImmutabilityAuditProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var write = await new OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditService()
            .BuildAndWriteAsync(repoRoot);
        var result = write.Result;

        Assert.Equal("GREEN", result.QualityGate.ImplementationStatus);
        Assert.True(result.QualityGate.Passed, string.Join(Environment.NewLine, result.QualityGate.Diagnostics));
        Assert.True(result.SourceHealthBeforeAfter.SourceSplitCompleted);
        Assert.True(result.QualityGate.LargestGoal108OrchestratorFileBelow700Lines);
        Assert.True(result.QualityGate.ActualGitDiffAuditPerformed);
        Assert.False(result.QualityGate.Goal101To107ArtifactsModified);
        Assert.True(result.QualityGate.Goal108ClaimMatchesActualGitDiff);
        Assert.True(result.QualityGate.EvidenceTrustDebtStatusHonest);
        Assert.True(result.QualityGate.AlphaRuntimeBootstrapUnchanged);
        Assert.True(result.QualityGate.NoForbiddenAreasChanged);
        Assert.Empty(result.QualityGate.ForbiddenChangedPaths);

        Assert.True(result.HistoricalArtifactDiffAudit.Goal108ChangedPathCount > 0);
        Assert.Empty(result.HistoricalArtifactDiffAudit.Goal101To107ChangedPaths);
        Assert.All(result.HistoricalArtifactDiffAudit.Goal108ChangedPaths, path =>
            Assert.True(
                path.Contains("goal-108", StringComparison.OrdinalIgnoreCase)
                || path.Contains("Goal108", StringComparison.Ordinal),
                path));
        Assert.True(result.ImmutabilityTrustAudit.Goal108ClaimRead);
        Assert.True(result.ImmutabilityTrustAudit.Goal108HistoricalArtifactsUnchangedClaim);
        Assert.True(result.ImmutabilityTrustAudit.Goal108ClaimMatchesActualGitDiff);
        Assert.False(result.ImmutabilityTrustAudit.EvidenceTrustDebtRecorded);
        Assert.True(result.NegativeProof.Passed);

        AssertFilesExist(
            write.OutputDirectoryPath,
            OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditVocabulary.RequiredEvidenceFileNames);
        Assert.True(File.Exists(write.ReportMarkdownPath));
    }

    private static void AssertFilesExist(string directory, IReadOnlyList<string> fileNames)
    {
        foreach (var fileName in fileNames)
        {
            Assert.True(File.Exists(Path.Combine(directory, fileName)), fileName);
        }
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
