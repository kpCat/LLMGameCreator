using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceExportPackage;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class OfflineGeoworldAlphaSliceExportPackageProductSmokeTests
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
    public async Task Goal109OfflineGeoworldAlphaSliceExportPackageProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var write = await new OfflineGeoworldAlphaSliceExportPackageEvidenceService()
            .BuildAndWriteAsync(repoRoot);
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.QualityGateScan.Passed, string.Join(Environment.NewLine, result.QualityGateScan.Diagnostics));
        Assert.True(result.CleanImportProof.Passed, string.Join(Environment.NewLine, result.CleanImportProof.Diagnostics));
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.UnityScriptInventory.Passed);
        Assert.True(result.EditorWindowInventory.Passed);
        Assert.True(result.WorkspaceBindingInventory.Passed);
        Assert.True(result.Manifest.MetadataOnly);
        Assert.True(result.Manifest.AlphaToolingOnly);
        Assert.True(result.Manifest.PortableDirectoryPackage);
        Assert.False(result.Manifest.Accepted);
        Assert.True(result.Manifest.AlphaRuntimeBootstrapUnchanged);
        Assert.True(result.Manifest.Goal108AImmutabilityAuditIncluded);
        Assert.True(result.Manifest.Goal101To107HistoricalArtifactsUnchanged);
        Assert.Equal(6, result.Manifest.PackageFileCount);
        Assert.Equal(5, result.Manifest.IndexedFileCount);
        Assert.Equal(7, result.Manifest.SourceComponentCount);
        Assert.Equal(7, result.Manifest.ReadySourceComponentCount);
        Assert.Equal(5, result.CleanImportProof.ReadFileCount);

        AssertFilesExist(write.ProceduralOutputDirectoryPath,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.RequiredPackageFileNames);
        AssertFilesExist(write.ProceduralOutputDirectoryPath,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.RequiredEvidenceFileNames);
        AssertFilesExist(write.ExportPackageDirectoryPath,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.RequiredPackageFileNames);
        AssertFilesExist(write.StreamingAssetsDirectoryPath,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.RequiredPackageFileNames);

        var changedFiles = Directory
            .EnumerateFiles(write.ProceduralOutputDirectoryPath, "*", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(write.ExportPackageDirectoryPath, "*", SearchOption.AllDirectories))
            .Concat(Directory.EnumerateFiles(write.StreamingAssetsDirectoryPath, "*", SearchOption.AllDirectories))
            .ToList();
        Assert.DoesNotContain(
            changedFiles,
            path => ForbiddenExtensions.Contains(Path.GetExtension(path)));

        var combinedText = string.Join(Environment.NewLine, changedFiles.Select(File.ReadAllText));
        Assert.DoesNotContain(repoRoot, combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("UnityWebRequest", combinedText, StringComparison.Ordinal);
        Assert.DoesNotContain("HttpClient", combinedText, StringComparison.Ordinal);
        Assert.DoesNotContain("http://", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"rawGeodataIncluded\": true", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"noRawGeodata\": false", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".geojson", combinedText, StringComparison.OrdinalIgnoreCase);

        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            write.ExportPackageDirectoryPath,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.ManifestFileName)));
        using var clean = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            write.ProceduralOutputDirectoryPath,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.CleanImportProofFileName)));
        using var quality = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            write.ProceduralOutputDirectoryPath,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.QualityGateScanFileName)));

        Assert.False(manifest.RootElement.GetProperty("accepted").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("alphaRuntimeBootstrapUnchanged").GetBoolean());
        Assert.True(clean.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(clean.RootElement.GetProperty("checksumsMatch").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noScenePrefabSettingsProjectPackageMutation").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("sourceHealthLimitsPassed").GetBoolean());

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(repoRoot);
        var alphaExportGroup = Assert.Single(
            workspace.Catalog.Groups,
            group => group.GroupId == "offline_geoworld_alpha_export_package");
        var summary = Assert.Single(
            alphaExportGroup.Entries,
            entry => entry.ArtifactKind == "offline_geoworld_alpha_export_package_workspace_summary");
        Assert.Equal(6, summary.OfflineGeoworldAlphaExportPackageFileCount);
        Assert.Equal("matched", summary.OfflineGeoworldAlphaExportChecksumStatus);
        Assert.True(summary.OfflineGeoworldAlphaExportCleanImportProofPassed);
        Assert.True(summary.OfflineGeoworldAlphaExportUnityVerifierReady);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaExportPackageGroupPresent);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaExportQualityGatePassed);

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
