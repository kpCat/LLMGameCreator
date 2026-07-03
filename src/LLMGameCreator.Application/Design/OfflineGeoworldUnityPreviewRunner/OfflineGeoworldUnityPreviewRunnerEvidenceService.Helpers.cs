using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityPreviewRunner;

public sealed partial class OfflineGeoworldUnityPreviewRunnerEvidenceService
{
    private static OfflineGeoworldPreviewReport BuildReport(
        Goal101Payload payload,
        OfflineGeoworldPreviewStreamingAssetsLedger ledger,
        OfflineGeoworldPreviewUnityScriptInventory scripts,
        OfflineGeoworldPreviewSimulatedCommandProof proof,
        OfflineGeoworldPreviewNegativeProof negative,
        OfflineGeoworldPreviewWorkspaceBindingInventory binding,
        OfflineGeoworldPreviewQualityGateScan quality,
        IReadOnlyDictionary<string, string> evidence) =>
        new()
        {
            CommandCount = payload.Manifest.CommandCount,
            CommandKindCount = payload.Manifest.CommandKindCount,
            TravelWindowStepCount = payload.Manifest.TravelWindowStepCount,
            UnityPayloadFileCount = ledger.PayloadFileCount,
            SimulatedCommandProofPassed = proof.Passed,
            NegativeProofPassed = negative.Passed,
            WorkspaceBindingPassed = binding.Passed,
            UnityScriptsReady = scripts.Passed,
            AlphaRuntimeBootstrapUnchanged = quality.AlphaRuntimeBootstrapUnchanged,
            QualityGatePassed = quality.Passed,
            CommandCatalogHash = Hash(Serialize(payload.CommandCatalog)),
            StyleLegendHash = Hash(Serialize(payload.StyleLegend)),
            TravelWindowScriptHash = Hash(Serialize(payload.TravelWindowScript)),
            ManifestHash = Hash(Serialize(payload.Manifest)),
            StreamingAssetsLedgerHash =
                Hash(evidence[OfflineGeoworldUnityPreviewRunnerVocabulary.StreamingAssetsLedgerFileName]),
            UnityScriptInventoryHash =
                Hash(evidence[OfflineGeoworldUnityPreviewRunnerVocabulary.UnityScriptInventoryFileName]),
            SimulatedCommandProofHash =
                Hash(evidence[OfflineGeoworldUnityPreviewRunnerVocabulary.SimulatedCommandProofFileName]),
            NegativeProofHash =
                Hash(evidence[OfflineGeoworldUnityPreviewRunnerVocabulary.NegativeProofFileName]),
            WorkspaceBindingInventoryHash =
                Hash(evidence[OfflineGeoworldUnityPreviewRunnerVocabulary.WorkspaceBindingInventoryFileName]),
            SourceLineageHash =
                Hash(evidence[OfflineGeoworldUnityPreviewRunnerVocabulary.SourceLineageFileName]),
            QualityGateHash =
                Hash(evidence[OfflineGeoworldUnityPreviewRunnerVocabulary.QualityGateScanFileName])
        };

    private static string RenderReport(
        OfflineGeoworldPreviewReport report,
        OfflineGeoworldPreviewQualityGateScan quality,
        OfflineGeoworldPreviewSimulatedCommandProof proof) =>
        string.Join(Environment.NewLine,
        [
            "# Goal 101 Offline Geoworld Unity Preview Runner",
            string.Empty,
            "- implementationStatus: " + report.ImplementationStatus,
            "- accepted: false",
            "- manualGate: " + report.ManualGate + " required",
            "- deterministicReportHash: " + report.DeterministicReportHash,
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal101 consumes the real Goal100 offline geoworld visual cache Unity handoff payload and writes metadata-only preview commands, a style legend and travel-window demo metadata for a standalone Unity Alpha preview runner. It creates placeholder-object instructions only and does not implement final Runtime consumption, full gameplay, real geodata fetching, final art, atlas or scene/prefab production.",
            string.Empty,
            "## Counts",
            string.Empty,
            "- commandCount: " + report.CommandCount,
            "- commandKindCount: " + report.CommandKindCount,
            "- travelWindowStepCount: " + report.TravelWindowStepCount,
            "- unityPayloadFileCount: " + report.UnityPayloadFileCount,
            string.Empty,
            "## Command Kinds",
            string.Empty,
            string.Join(
                Environment.NewLine,
                proof.CommandCountByKind.Select(item => "- " + item.Key + ": " + item.Value)),
            string.Empty,
            "## Quality Gate",
            string.Empty,
            "- qualityGatePassed: " + quality.Passed.ToString().ToLowerInvariant(),
            "- goal100Consumed: " + quality.Goal100Consumed.ToString().ToLowerInvariant(),
            "- previewCommandsBuilt: " + quality.PreviewCommandsBuilt.ToString().ToLowerInvariant(),
            "- allCommandKindsMapped: " + quality.AllCommandKindsMapped.ToString().ToLowerInvariant(),
            "- travelWindowDemoBuilt: " + quality.TravelWindowDemoBuilt.ToString().ToLowerInvariant(),
            "- unityPayloadCreated: " + quality.UnityPayloadCreated.ToString().ToLowerInvariant(),
            "- unityScriptsReady: " + quality.UnityScriptsReady.ToString().ToLowerInvariant(),
            "- simulatedCommandProofPassed: " + report.SimulatedCommandProofPassed.ToString().ToLowerInvariant(),
            "- negativeProofPassed: " + report.NegativeProofPassed.ToString().ToLowerInvariant(),
            "- workspaceBindingPassed: " + report.WorkspaceBindingPassed.ToString().ToLowerInvariant(),
            "- alphaRuntimeBootstrapUnchanged: " + report.AlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "- noNetworkOrProviderImplementation: " + quality.NoNetworkOrProviderImplementation.ToString().ToLowerInvariant(),
            "- noRawGeodataDump: " + quality.NoRawGeodataDump.ToString().ToLowerInvariant(),
            "- noBinaryOrRasterMedia: " + quality.NoBinaryOrRasterMedia.ToString().ToLowerInvariant(),
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            "- commandCatalogHash: " + report.CommandCatalogHash,
            "- styleLegendHash: " + report.StyleLegendHash,
            "- travelWindowScriptHash: " + report.TravelWindowScriptHash,
            "- manifestHash: " + report.ManifestHash,
            "- streamingAssetsLedgerHash: " + report.StreamingAssetsLedgerHash,
            "- unityScriptInventoryHash: " + report.UnityScriptInventoryHash,
            "- simulatedCommandProofHash: " + report.SimulatedCommandProofHash,
            "- negativeProofHash: " + report.NegativeProofHash,
            "- workspaceBindingInventoryHash: " + report.WorkspaceBindingInventoryHash,
            "- sourceLineageHash: " + report.SourceLineageHash,
            "- qualityGateHash: " + report.QualityGateHash
        ]) + Environment.NewLine;

    private static IReadOnlyDictionary<string, string> ReadPayloadFiles(string root)
    {
        var directory = Resolve(root, OfflineGeoworldUnityPreviewRunnerVocabulary.StreamingAssetsRelativeRoot);
        var payload = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var fileName in OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredPayloadFileNames)
        {
            var path = Path.Combine(directory, fileName);
            if (File.Exists(path))
            {
                payload[fileName] = File.ReadAllText(path, Encoding.UTF8);
            }
        }

        return payload;
    }

    private static IEnumerable<string> CandidateSourceFiles(string root)
    {
        var paths = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        AddDirectory(paths, root, "src/LLMGameCreator.Application/Design/OfflineGeoworldUnityPreviewRunner");
        AddDirectory(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace");
        AddDirectory(paths, root, "tests/LLMGameCreator.Tests/Application/OfflineGeoworldUnityPreviewRunner");
        AddDirectory(paths, root, "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace");
        paths.Add(Resolve(root, "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldUnityPreviewRunnerProductSmokeTests.cs"));
        paths.Add(Resolve(root, "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs"));
        paths.Add(Resolve(root, OfflineGeoworldUnityPreviewRunnerVocabulary.UnityPreviewRunnerScriptPath));
        paths.Add(Resolve(root, OfflineGeoworldUnityPreviewRunnerVocabulary.UnityPrimitiveFactoryScriptPath));
        paths.Add(Resolve(root, OfflineGeoworldUnityPreviewRunnerVocabulary.UnityTravelWindowScriptPath));
        return paths;
    }

    private static void AddDirectory(ISet<string> paths, string root, string relativePath)
    {
        var directory = Resolve(root, relativePath);
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.TopDirectoryOnly))
        {
            paths.Add(file);
        }
    }

    private static (string RelativePath, int Lines) ScanSourceFile(string root, string path)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        return (Relative(root, path), CountLines(text));
    }

    private static OfflineGeoworldPreviewSourceLineageRecord SourceLineageRecord(
        string root,
        string relativePath,
        string purpose)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        return new OfflineGeoworldPreviewSourceLineageRecord
        {
            RelativePath = relativePath,
            Exists = exists,
            Sha256 = exists ? HashFile(path) : string.Empty,
            Purpose = purpose
        };
    }

    private static JsonDocument? ReadJson(
        string root,
        string relativePath,
        List<OfflineGeoworldUnityPreviewDiagnostic> diagnostics)
    {
        var path = Resolve(root, relativePath);
        if (!File.Exists(path))
        {
            diagnostics.Add(OfflineGeoworldUnityPreviewDiagnostic.Error(
                "goal101.json.missing",
                relativePath,
                "Required source JSON file is missing."));
            return null;
        }

        try
        {
            return JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        }
        catch (JsonException ex)
        {
            diagnostics.Add(OfflineGeoworldUnityPreviewDiagnostic.Error(
                "goal101.json.invalid",
                relativePath,
                ex.Message));
            return null;
        }
    }

    private static string PayloadRole(string fileName) =>
        fileName switch
        {
            var name when name == OfflineGeoworldUnityPreviewRunnerVocabulary.ManifestFileName =>
                "manifest",
            var name when name == OfflineGeoworldUnityPreviewRunnerVocabulary.FeatureCommandsFileName =>
                "feature_commands",
            var name when name == OfflineGeoworldUnityPreviewRunnerVocabulary.TravelWindowScriptFileName =>
                "travel_window_script",
            var name when name == OfflineGeoworldUnityPreviewRunnerVocabulary.StyleLegendFileName =>
                "style_legend",
            var name when name == OfflineGeoworldUnityPreviewRunnerVocabulary.ReadmeFileName =>
                "readme",
            _ => "payload"
        };

    private static OfflineGeoworldPreviewNegativeScenario Scenario(
        string id,
        string mutation,
        string code,
        string target) =>
        new()
        {
            ScenarioId = id,
            CausalMutation = mutation,
            ActualStatus = "rejected",
            Diagnostics =
            [
                OfflineGeoworldUnityPreviewDiagnostic.Error(
                    code,
                    target,
                    "Goal101 negative proof rejected the mutated preview runner payload.")
            ]
        };

    private static string Serialize<T>(T value) =>
        OfflineGeoworldUnityPreviewJson.Serialize(value);

    private static string Hash(string text) =>
        OfflineGeoworldUnityPreviewHash.Sha256Text(text);

    private static string HashFile(string path) =>
        OfflineGeoworldUnityPreviewHash.Sha256File(path);
}
