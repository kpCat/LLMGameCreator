using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldObjectiveAcceptanceRun;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class OfflineGeoworldObjectiveAcceptanceRunProductSmokeTests
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
    public async Task Goal107OfflineGeoworldObjectiveAcceptanceRunProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var write = await new OfflineGeoworldObjectiveAcceptanceRunEvidenceService()
            .BuildAndWriteAsync(repoRoot);
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(OfflineGeoworldObjectiveAcceptanceRunVocabulary.FinalGate, result.Report.ManualGate);
        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.QualityGateScan.Goal106Consumed);
        Assert.True(result.QualityGateScan.ObjectivePayloadCreated);
        Assert.True(result.QualityGateScan.ReplayAcceptanceProofPassed);
        Assert.True(result.QualityGateScan.NegativeProofPassed);
        Assert.True(result.QualityGateScan.UnityScriptsReady);
        Assert.True(result.QualityGateScan.EditorWindowReady);
        Assert.True(result.QualityGateScan.WorkspaceBindingPassed);
        Assert.True(result.QualityGateScan.AlphaQualityConsolidationPassed);
        Assert.True(result.Report.ObjectiveCount >= 6);
        Assert.Equal(result.Report.ObjectiveCount, result.Report.CompletedObjectiveCount);
        Assert.Equal("completed", result.Report.FinalStatus);

        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.ManifestFileName)));
        using var objectives = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.ObjectivesFileName)));
        using var run = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.AcceptanceRunFileName)));
        using var completion = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.CompletionStateFileName)));
        using var proof = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.ReplayAcceptanceProofFileName)));

        Assert.Equal(6, manifest.RootElement.GetProperty("payloadFileCount").GetInt32());
        Assert.True(manifest.RootElement.GetProperty("objectiveCount").GetInt32() >= 6);
        Assert.True(manifest.RootElement.GetProperty("metadataOnly").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noRawGeodata").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noAbsolutePaths").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noBinaryOrRasterMedia").GetBoolean());
        Assert.False(manifest.RootElement.GetProperty("containsRuntimeExecution").GetBoolean());
        Assert.False(manifest.RootElement.GetProperty("containsProviderCalls").GetBoolean());
        Assert.Equal(
            manifest.RootElement.GetProperty("objectiveCount").GetInt32(),
            objectives.RootElement.GetProperty("objectiveCount").GetInt32());
        Assert.Equal(
            objectives.RootElement.GetProperty("objectiveCount").GetInt32(),
            run.RootElement.GetProperty("replayStepCount").GetInt32());
        Assert.Equal(
            objectives.RootElement.GetProperty("objectiveCount").GetInt32(),
            completion.RootElement.GetProperty("completedObjectiveCount").GetInt32());
        Assert.Equal("completed", completion.RootElement.GetProperty("finalStatus").GetString());
        Assert.True(proof.RootElement.GetProperty("checkpointResumeApplied").GetBoolean());
        Assert.True(proof.RootElement.GetProperty("completionTransitionsPassed").GetBoolean());
        Assert.True(proof.RootElement.GetProperty("stateDeltaLinkagePassed").GetBoolean());

        var payloadFiles = Directory.EnumerateFiles(write.StreamingAssetsDirectoryPath, "*", SearchOption.AllDirectories)
            .ToArray();
        Assert.Equal(6, payloadFiles.Length);
        Assert.DoesNotContain(payloadFiles, path => ForbiddenOutputExtensions.Contains(Path.GetExtension(path)));
        foreach (var path in payloadFiles)
        {
            var text = await File.ReadAllTextAsync(path);
            Assert.DoesNotContain(repoRoot, text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("rawGeodataIncluded\": true", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("noRawGeodata\": false", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("UnityWebRequest", text, StringComparison.Ordinal);
            Assert.DoesNotContain("HttpClient", text, StringComparison.Ordinal);
            Assert.DoesNotContain(".png", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(".geojson", text, StringComparison.OrdinalIgnoreCase);
        }

        Assert.True(result.UnityScriptInventory.ObjectiveStateExists);
        Assert.True(result.UnityScriptInventory.ObjectiveTrackerExists);
        Assert.True(result.UnityScriptInventory.ObjectiveAcceptanceControllerExists);
        Assert.True(result.UnityScriptInventory.ReadsApplicationStreamingAssetsPath);
        Assert.True(result.UnityScriptInventory.ReadsGoal107Root);
        Assert.True(result.UnityScriptInventory.IntegratesGoal105InteractionController);
        Assert.True(result.UnityScriptInventory.IntegratesGoal106ReplayAndSaveLoadControllers);
        Assert.True(result.UnityScriptInventory.SupportsManualAdvanceAndReplayChecks);
        Assert.All(result.UnityScriptInventory.Files, file =>
        {
            Assert.True(file.Exists, file.RelativePath);
            Assert.True(file.NotMinified, file.RelativePath);
            Assert.True(file.HasNoProviderNetworkMarkers, file.RelativePath);
            Assert.True(file.DoesNotReferenceAlphaRuntimeBootstrap, file.RelativePath);
            Assert.True(file.HasNoExternalDependencyMarkers, file.RelativePath);
            Assert.False(Path.IsPathFullyQualified(file.RelativePath), file.RelativePath);
        });
        Assert.True(result.EditorWindowInventory.MenuItemMarkerPresent);
        Assert.True(result.EditorWindowInventory.CreateRigMethodPresent);
        Assert.True(result.EditorWindowInventory.ClearRigMethodPresent);
        Assert.True(result.EditorWindowInventory.AcceptanceInstructionsPresent);
        Assert.True(result.EditorWindowInventory.ManualButtonOnly);
        Assert.True(result.EditorWindowInventory.HasNoAutoRunImportMarker);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(repoRoot);
        Assert.True(workspace.QualityGateScan.Passed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldObjectiveAcceptanceGroupPresent);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldObjectiveCount >= 6);
        Assert.Equal(
            workspace.QualityGateScan.OfflineGeoworldObjectiveCount,
            workspace.QualityGateScan.OfflineGeoworldObjectiveCompletedCount);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldObjectiveReplayAcceptanceProofPassed);
        Assert.True(workspace.QualityGateScan.Goal107FilesDiscoveredByRelativePaths);

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
        Assert.Contains("offline_geoworld_objective_acceptance_run_verification required", report);
        Assert.Contains("objectiveCount:", report);
        Assert.Contains("replayAcceptanceProofPassed: true", report);
        Assert.Contains("negativeProofPassed: true", report);
        Assert.Contains("alphaRuntimeBootstrapUnchanged: true", report);
        Assert.Contains("checkpointResumeApplied: true", report);
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
