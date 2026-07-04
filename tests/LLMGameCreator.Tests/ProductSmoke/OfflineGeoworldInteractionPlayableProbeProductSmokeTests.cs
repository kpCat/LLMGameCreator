using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldInteractionPlayableProbe;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class OfflineGeoworldInteractionPlayableProbeProductSmokeTests
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
    public async Task Goal105OfflineGeoworldInteractionPlayableProbeProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var write = await new OfflineGeoworldInteractionPlayableProbeEvidenceService()
            .BuildAndWriteAsync(repoRoot);
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(OfflineGeoworldInteractionPlayableProbeVocabulary.FinalGate, result.Report.ManualGate);
        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.QualityGateScan.Goal104Consumed);
        Assert.True(result.QualityGateScan.InteractionPayloadCreated);
        Assert.True(result.QualityGateScan.UnityScriptInventorySafetyPassed);
        Assert.True(result.QualityGateScan.SimulatedSessionProofPassed);
        Assert.True(result.QualityGateScan.NegativeProofPassed);
        Assert.True(result.QualityGateScan.WorkspaceBindingPassed);
        Assert.True(result.QualityGateScan.AlphaRuntimeBootstrapUnchanged);
        Assert.True(result.Report.TargetCount >= 8);
        Assert.True(result.Report.ActionKindCount >= 5);
        Assert.True(result.Report.ScriptedEventCount >= 6);
        Assert.True(result.Report.StateDeltaCount >= 6);
        Assert.True(result.Report.DeterministicStateHashChainPassed);

        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldInteractionPlayableProbeVocabulary.ManifestFileName)));
        using var targets = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldInteractionPlayableProbeVocabulary.TargetsFileName)));
        using var actions = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldInteractionPlayableProbeVocabulary.ActionsFileName)));
        using var session = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldInteractionPlayableProbeVocabulary.SessionScriptFileName)));
        using var deltas = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldInteractionPlayableProbeVocabulary.StateDeltaPlanFileName)));

        Assert.Equal(6, manifest.RootElement.GetProperty("payloadFileCount").GetInt32());
        Assert.True(manifest.RootElement.GetProperty("targetCount").GetInt32() >= 8);
        Assert.True(manifest.RootElement.GetProperty("actionKindCount").GetInt32() >= 5);
        Assert.True(manifest.RootElement.GetProperty("scriptedEventCount").GetInt32() >= 6);
        Assert.True(manifest.RootElement.GetProperty("stateDeltaCount").GetInt32() >= 6);
        Assert.True(manifest.RootElement.GetProperty("metadataOnly").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("stateDeltasSeparateFromBaseData").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noRawGeodata").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noAbsolutePaths").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noBinaryOrRasterMedia").GetBoolean());
        Assert.False(manifest.RootElement.GetProperty("containsRuntimeExecution").GetBoolean());
        Assert.False(manifest.RootElement.GetProperty("containsProviderCalls").GetBoolean());
        Assert.True(targets.RootElement.GetProperty("targetCount").GetInt32() >= 8);
        Assert.True(actions.RootElement.GetProperty("actionKindCount").GetInt32() >= 5);
        Assert.Equal(6, session.RootElement.GetProperty("eventCount").GetInt32());
        Assert.Equal(6, deltas.RootElement.GetProperty("stateDeltaCount").GetInt32());
        Assert.False(deltas.RootElement.GetProperty("mutatesBaseDataDirectly").GetBoolean());
        Assert.Equal(
            deltas.RootElement.GetProperty("stateDeltaCount").GetInt32() + 1,
            deltas.RootElement.GetProperty("stateHashChain").GetArrayLength());

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

        Assert.True(result.UnityScriptInventory.ControllerExists);
        Assert.True(result.UnityScriptInventory.TargetScriptExists);
        Assert.True(result.UnityScriptInventory.StateDeltaLogExists);
        Assert.True(result.UnityScriptInventory.ControllerUsesApplicationStreamingAssetsPath);
        Assert.True(result.UnityScriptInventory.ControllerBindsTargetsByIdOrName);
        Assert.True(result.UnityScriptInventory.ControllerSupportsNearestTargetSelection);
        Assert.True(result.UnityScriptInventory.ControllerExecutesScriptedAndManualActions);
        Assert.True(result.UnityScriptInventory.StateDeltaLogInMemoryOnly);
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
        Assert.True(result.EditorWindowInventory.ManualButtonOnly);
        Assert.True(result.EditorWindowInventory.HasNoAutoRunImportMarker);
        Assert.True(result.EditorWindowInventory.SourceFile.HasNoProviderNetworkMarkers);
        Assert.True(result.EditorWindowInventory.SourceFile.DoesNotReferenceAlphaRuntimeBootstrap);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(repoRoot);
        Assert.True(workspace.QualityGateScan.Passed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldInteractionGroupPresent);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldInteractionTargetCount >= 8);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldInteractionUnityScriptsReady);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldInteractionEditorWindowReady);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldInteractionUnitySafetyScanPassed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldInteractionSimulatedSessionProofPassed);
        Assert.True(workspace.QualityGateScan.Goal105FilesDiscoveredByRelativePaths);

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
        Assert.Contains("offline_geoworld_interaction_playable_probe_verification required", report);
        Assert.Contains("targetCount:", report);
        Assert.Contains("actionKindCount:", report);
        Assert.Contains("scriptedEventCount: 6", report);
        Assert.Contains("stateDeltaCount: 6", report);
        Assert.Contains("stateHashChainPassed: true", report);
        Assert.Contains("unityScriptsReady: true", report);
        Assert.Contains("editorWindowReady: true", report);
        Assert.Contains("simulatedSessionProofPassed: true", report);
        Assert.Contains("negativeProofPassed: true", report);
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
