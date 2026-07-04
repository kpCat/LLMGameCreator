using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldSessionPersistenceReplay;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class OfflineGeoworldSessionPersistenceReplayProductSmokeTests
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
    public async Task Goal106OfflineGeoworldSessionPersistenceReplayProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var write = await new OfflineGeoworldSessionPersistenceReplayEvidenceService()
            .BuildAndWriteAsync(repoRoot);
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(OfflineGeoworldSessionPersistenceReplayVocabulary.FinalGate, result.Report.ManualGate);
        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.QualityGateScan.Goal105Consumed);
        Assert.True(result.QualityGateScan.SessionPayloadCreated);
        Assert.True(result.QualityGateScan.SaveLoadReplayProofPassed);
        Assert.True(result.QualityGateScan.NegativeProofPassed);
        Assert.True(result.QualityGateScan.UnityScriptsReady);
        Assert.True(result.QualityGateScan.EditorWindowReady);
        Assert.True(result.QualityGateScan.WorkspaceBindingPassed);
        Assert.True(result.QualityGateScan.AlphaRuntimeBootstrapUnchanged);
        Assert.True(result.Report.ReplayStepCount >= 6);
        Assert.True(result.Report.StateDeltaCount >= 6);
        Assert.True(result.Report.CheckpointStepIndex >= 3);
        Assert.True(result.Report.SimulatedSaveLoadReplayProofPassed);

        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldSessionPersistenceReplayVocabulary.ManifestFileName)));
        using var initial = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldSessionPersistenceReplayVocabulary.InitialStateFileName)));
        using var deltaLog = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldSessionPersistenceReplayVocabulary.DeltaLogFileName)));
        using var replay = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldSessionPersistenceReplayVocabulary.ReplayScriptFileName)));
        using var checklist = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldSessionPersistenceReplayVocabulary.AcceptanceChecklistFileName)));

        Assert.Equal(6, manifest.RootElement.GetProperty("payloadFileCount").GetInt32());
        Assert.True(manifest.RootElement.GetProperty("replayStepCount").GetInt32() >= 6);
        Assert.True(manifest.RootElement.GetProperty("stateDeltaCount").GetInt32() >= 6);
        Assert.True(manifest.RootElement.GetProperty("checkpointStepIndex").GetInt32() >= 3);
        Assert.True(manifest.RootElement.GetProperty("metadataOnly").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noRawGeodata").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noAbsolutePaths").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noBinaryOrRasterMedia").GetBoolean());
        Assert.False(manifest.RootElement.GetProperty("containsRuntimeExecution").GetBoolean());
        Assert.False(manifest.RootElement.GetProperty("containsProviderCalls").GetBoolean());
        Assert.True(initial.RootElement.GetProperty("targetCount").GetInt32() >= 8);
        Assert.True(initial.RootElement.GetProperty("actionCount").GetInt32() >= 8);
        Assert.Equal(
            replay.RootElement.GetProperty("replayStepCount").GetInt32(),
            deltaLog.RootElement.GetProperty("deltaCount").GetInt32());
        Assert.Equal(
            deltaLog.RootElement.GetProperty("deltaCount").GetInt32() + 1,
            deltaLog.RootElement.GetProperty("stateHashChain").GetArrayLength());
        Assert.True(checklist.RootElement.GetProperty("stepCount").GetInt32() > 0);

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

        Assert.True(result.UnityScriptInventory.SaveLoadControllerExists);
        Assert.True(result.UnityScriptInventory.ReplayControllerExists);
        Assert.True(result.UnityScriptInventory.SnapshotModelExists);
        Assert.True(result.UnityScriptInventory.ReadsApplicationStreamingAssetsPath);
        Assert.True(result.UnityScriptInventory.UsesApplicationPersistentDataPath);
        Assert.True(result.UnityScriptInventory.ReadsGoal106Root);
        Assert.True(result.UnityScriptInventory.IntegratesGoal105ControllerAndDeltaLog);
        Assert.True(result.UnityScriptInventory.SupportsSaveLoadDeleteSnapshot);
        Assert.True(result.UnityScriptInventory.SupportsReplayStepping);
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
        Assert.True(result.EditorWindowInventory.AcceptanceChecklistUiPresent);
        Assert.True(result.EditorWindowInventory.ManualButtonOnly);
        Assert.True(result.EditorWindowInventory.HasNoAutoRunImportMarker);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(repoRoot);
        Assert.True(workspace.QualityGateScan.Passed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldSessionReplayGroupPresent);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldSessionReplayStepCount >= 6);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldSessionStateDeltaCount >= 6);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldSessionUnityScriptsReady);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldSessionEditorWindowReady);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldSessionSimulatedReplayProofPassed);
        Assert.True(workspace.QualityGateScan.Goal106FilesDiscoveredByRelativePaths);

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
        Assert.Contains("offline_geoworld_session_persistence_replay_verification required", report);
        Assert.Contains("replayStepCount:", report);
        Assert.Contains("stateDeltaCount:", report);
        Assert.Contains("checkpointStepIndex:", report);
        Assert.Contains("simulatedSaveLoadReplayProofPassed: true", report);
        Assert.Contains("negativeProofPassed: true", report);
        Assert.Contains("alphaRuntimeBootstrapUnchanged: true", report);
        Assert.Contains("replayResumedToFinalHash: true", report);
        Assert.Contains("corruptedSnapshotRejected: true", report);
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
