using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class OfflineGeoworldAlphaSliceManualAcceptanceGateProductSmokeTests
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
    public async Task Goal110OfflineGeoworldAlphaManualAcceptanceGateProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var write = await new OfflineGeoworldAlphaSliceManualAcceptanceGateEvidenceService()
            .BuildAndWriteAsync(repoRoot);
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(
            result.QualityGateScan.Passed,
            string.Join(Environment.NewLine, result.QualityGateScan.Diagnostics));
        Assert.True(result.Manifest.AutomatedGatePassed);
        Assert.False(result.Manifest.Accepted);
        Assert.True(result.Manifest.ManualAcceptancePending);
        Assert.True(result.Manifest.AlphaRuntimeBootstrapUnchanged);
        Assert.True(result.UnityScriptInventory.Passed);
        Assert.True(result.EditorWindowInventory.Passed);
        Assert.True(result.SimulatedProof.Passed);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.WorkspaceBindingInventory.Passed);
        Assert.Equal(5, result.Manifest.PayloadFileCount);
        Assert.Equal(7, result.Manifest.ExportFileCount);
        Assert.Equal(12, result.Checklist.StepCount);

        AssertFilesExist(write.ProceduralOutputDirectoryPath,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredPayloadFileNames);
        AssertFilesExist(write.ProceduralOutputDirectoryPath,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredEvidenceFileNames);
        AssertFilesExist(write.ExportPackageDirectoryPath,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredExportFileNames);
        AssertFilesExist(write.StreamingAssetsDirectoryPath,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredPayloadFileNames);

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
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ManifestFileName)));
        using var checklist = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            write.ExportPackageDirectoryPath,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ChecklistFileName)));
        using var quality = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            write.ProceduralOutputDirectoryPath,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.QualityGateScanFileName)));

        Assert.False(manifest.RootElement.GetProperty("accepted").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("manualAcceptancePending").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("alphaRuntimeBootstrapUnchanged").GetBoolean());
        Assert.Equal(12, checklist.RootElement.GetProperty("stepCount").GetInt32());
        Assert.True(quality.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("manualAcceptancePending").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noScenePrefabSettingsProjectPackageMutation").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("sourceHealthLimitsPassed").GetBoolean());

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(repoRoot);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_alpha_manual_acceptance");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind == "offline_geoworld_alpha_manual_acceptance_workspace_summary");
        Assert.Equal(12, summary.OfflineGeoworldAlphaManualAcceptanceChecklistStepCount);
        Assert.Equal(5, summary.OfflineGeoworldAlphaManualAcceptancePayloadFileCount);
        Assert.True(summary.OfflineGeoworldAlphaManualAcceptanceManualPending);
        Assert.True(summary.OfflineGeoworldAlphaManualAcceptanceUnityRunnerReady);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualAcceptanceGroupPresent);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualAcceptanceQualityGatePassed);

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
