using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldUnityPlayModeTravelPreview;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class OfflineGeoworldUnityPlayModeTravelPreviewProductSmokeTests
{
    private static readonly HashSet<string> ForbiddenOutputExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".osm",
        ".pbf",
        ".mbtiles",
        ".gpkg",
        ".geojson",
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
    public async Task Goal103OfflineGeoworldUnityPlayModeTravelPreviewProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var write = await new OfflineGeoworldPlayModeTravelPreviewEvidenceService()
            .BuildAndWriteAsync(repoRoot);
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.Report.StepCount >= 4);
        Assert.Equal(18, result.Report.ObjectCount);
        Assert.True(result.Report.UnityScriptsReady);
        Assert.True(result.Report.EditorWindowReady);
        Assert.True(result.Report.SimulatedExecutionProofPassed);
        Assert.True(result.Report.NegativeProofPassed);
        Assert.True(result.Report.Goal102BClosureRecorded);
        Assert.True(result.Report.AlphaRuntimeBootstrapUnchanged);

        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldPlayModeTravelPreviewVocabulary.ManifestFileName)));
        using var steps = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldPlayModeTravelPreviewVocabulary.StepsFileName)));
        using var chunks = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldPlayModeTravelPreviewVocabulary.ChunkVisibilityFileName)));
        using var objects = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldPlayModeTravelPreviewVocabulary.ObjectStateIndexFileName)));

        Assert.Equal(5, manifest.RootElement.GetProperty("payloadFileCount").GetInt32());
        Assert.True(manifest.RootElement.GetProperty("stepCount").GetInt32() >= 4);
        Assert.Equal(18, manifest.RootElement.GetProperty("objectCount").GetInt32());
        Assert.True(manifest.RootElement.GetProperty("metadataOnly").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noRawGeodata").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noAbsolutePaths").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noBinaryOrRasterMedia").GetBoolean());
        Assert.False(manifest.RootElement.GetProperty("containsRuntimeExecution").GetBoolean());
        Assert.False(manifest.RootElement.GetProperty("containsProviderCalls").GetBoolean());
        Assert.Equal(steps.RootElement.GetProperty("stepCount").GetInt32(), chunks.RootElement.GetProperty("stepCount").GetInt32());
        Assert.Equal(18, objects.RootElement.GetProperty("objectCount").GetInt32());

        var payloadFiles = Directory.EnumerateFiles(write.StreamingAssetsDirectoryPath, "*", SearchOption.AllDirectories)
            .ToArray();
        Assert.Equal(5, payloadFiles.Length);
        Assert.DoesNotContain(payloadFiles, path => ForbiddenOutputExtensions.Contains(Path.GetExtension(path)));
        foreach (var path in payloadFiles)
        {
            var text = await File.ReadAllTextAsync(path);
            Assert.DoesNotContain(repoRoot, text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("rawGeodataIncluded\": true", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("noRawGeodata\": false", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("UnityWebRequest", text, StringComparison.Ordinal);
            Assert.DoesNotContain("HttpClient", text, StringComparison.Ordinal);
        }

        Assert.All(result.UnityScriptInventory.Files, file =>
        {
            Assert.True(file.Exists, file.RelativePath);
            Assert.True(file.HasNoProviderNetworkMarkers, file.RelativePath);
            Assert.True(file.DoesNotReferenceAlphaRuntimeBootstrap, file.RelativePath);
            Assert.False(Path.IsPathFullyQualified(file.RelativePath), file.RelativePath);
        });
        Assert.True(result.EditorWindowInventory.SourceFile.HasNoProviderNetworkMarkers);
        Assert.True(result.EditorWindowInventory.SourceFile.DoesNotReferenceAlphaRuntimeBootstrap);
        Assert.True(result.EditorWindowInventory.SourceFile.HasNoScenePrefabSettingsMutationMarkers);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(repoRoot);
        Assert.True(workspace.QualityGateScan.Passed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldPlayModeTravelGroupPresent);
        Assert.Equal(18, workspace.QualityGateScan.OfflineGeoworldPlayModeTravelObjectCount);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldPlayModeTravelUnityScriptsReady);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldPlayModeTravelEditorWindowReady);
        Assert.True(workspace.QualityGateScan.Goal103FilesDiscoveredByRelativePaths);

        var outputFiles = Directory.EnumerateFiles(write.OutputDirectoryPath, "*", SearchOption.AllDirectories)
            .ToArray();
        Assert.DoesNotContain(outputFiles, path => ForbiddenOutputExtensions.Contains(Path.GetExtension(path)));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item =>
            item.StartsWith("src/LLMGameCreator.Runtime", StringComparison.Ordinal));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item =>
            item.StartsWith("src/LLMGameCreator.GamePackage", StringComparison.Ordinal));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item =>
            item.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase));

        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("offline_geoworld_playmode_travel_preview_verification required", report);
        Assert.Contains("stepCount:", report);
        Assert.Contains("objectCount: 18", report);
        Assert.Contains("unityScriptsReady: true", report);
        Assert.Contains("goal102bClosureRecorded: true", report);
        Assert.Contains("alphaRuntimeBootstrapUnchanged: true", report);
        Assert.DoesNotContain(repoRoot, report, StringComparison.OrdinalIgnoreCase);
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
