using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldUnityPreviewRunner;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityEditorPreviewTool;

public sealed partial class OfflineGeoworldUnityEditorPreviewToolEvidenceService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public OfflineGeoworldUnityEditorPreviewBuildResult Build(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var context = ReadGoal101Context(root);
        var inventory = BuildToolInventory(root);
        var proof = BuildSimulatedActionProof(root, context, inventory);
        var negative = BuildNegativeProof();
        var binding = BuildWorkspaceBindingInventory(root);
        var lineage = BuildSourceLineage(root, context);
        var quality = BuildQualityGate(root, context, inventory, proof, negative, binding, lineage);
        return BuildResult(inventory, proof, negative, binding, lineage, quality);
    }

    public async Task<OfflineGeoworldUnityEditorPreviewWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var result = Build(root);
        var outputDirectory = Resolve(
            root,
            OfflineGeoworldUnityEditorPreviewToolVocabulary.RelativeOutputDirectory);
        ResetDirectory(root, outputDirectory);

        var written = new List<string>();
        foreach (var item in result.EvidenceJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(outputDirectory, item.Key);
            await File.WriteAllTextAsync(path, item.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        var reportPath = Path.Combine(
            outputDirectory,
            OfflineGeoworldUnityEditorPreviewToolVocabulary.ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, reportPath));

        return new OfflineGeoworldUnityEditorPreviewWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldUnityEditorPreviewBuildResult BuildResult(
        OfflineGeoworldUnityEditorToolInventory inventory,
        OfflineGeoworldUnityEditorSimulatedActionProof proof,
        OfflineGeoworldUnityEditorNegativeProof negative,
        OfflineGeoworldUnityEditorWorkspaceBindingInventory binding,
        OfflineGeoworldUnityEditorSourceLineage lineage,
        OfflineGeoworldUnityEditorQualityGateScan quality)
    {
        var evidence = BuildEvidencePayloads(inventory, proof, negative, binding, lineage, quality);
        var reportWithoutHash = BuildReport(inventory, proof, negative, binding, quality, evidence);
        var reportMarkdownWithoutHash = RenderReport(reportWithoutHash, quality, proof);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = Hash(reportMarkdownWithoutHash)
        };
        var reportMarkdown = RenderReport(report, quality, proof);
        return new OfflineGeoworldUnityEditorPreviewBuildResult
        {
            ToolInventory = inventory,
            SimulatedActionProof = proof,
            NegativeProof = negative,
            WorkspaceBindingInventory = binding,
            SourceLineage = lineage,
            QualityGateScan = quality,
            Report = report,
            ReportMarkdown = reportMarkdown,
            EvidenceJsonByFileName = evidence
        };
    }

    private static Goal101EditorPreviewContext ReadGoal101Context(string root)
    {
        var diagnostics = new List<OfflineGeoworldUnityEditorPreviewDiagnostic>();
        var report = ReadOptionalText(
            root,
            OfflineGeoworldUnityEditorPreviewToolVocabulary.Goal101SourceRoot
            + "/"
            + OfflineGeoworldUnityPreviewRunnerVocabulary.ReportMarkdownFileName);
        using var manifestDoc = ReadJson(
            root,
            OfflineGeoworldUnityEditorPreviewToolVocabulary.Goal101SourceRoot
            + "/"
            + OfflineGeoworldUnityPreviewRunnerVocabulary.ManifestFileName,
            diagnostics);
        using var commandDoc = ReadJson(
            root,
            OfflineGeoworldUnityEditorPreviewToolVocabulary.Goal101SourceRoot
            + "/"
            + OfflineGeoworldUnityPreviewRunnerVocabulary.CommandCatalogFileName,
            diagnostics);
        using var travelDoc = ReadJson(
            root,
            OfflineGeoworldUnityEditorPreviewToolVocabulary.Goal101SourceRoot
            + "/"
            + OfflineGeoworldUnityPreviewRunnerVocabulary.TravelWindowScriptFileName,
            diagnostics);
        using var simulatedDoc = ReadJson(
            root,
            OfflineGeoworldUnityEditorPreviewToolVocabulary.Goal101SourceRoot
            + "/"
            + OfflineGeoworldUnityPreviewRunnerVocabulary.SimulatedCommandProofFileName,
            diagnostics);
        using var negativeDoc = ReadJson(
            root,
            OfflineGeoworldUnityEditorPreviewToolVocabulary.Goal101SourceRoot
            + "/"
            + OfflineGeoworldUnityPreviewRunnerVocabulary.NegativeProofFileName,
            diagnostics);
        using var qualityDoc = ReadJson(
            root,
            OfflineGeoworldUnityEditorPreviewToolVocabulary.Goal101SourceRoot
            + "/"
            + OfflineGeoworldUnityPreviewRunnerVocabulary.QualityGateScanFileName,
            diagnostics);
        using var scriptsDoc = ReadJson(
            root,
            OfflineGeoworldUnityEditorPreviewToolVocabulary.Goal101SourceRoot
            + "/"
            + OfflineGeoworldUnityPreviewRunnerVocabulary.UnityScriptInventoryFileName,
            diagnostics);

        var manifest = manifestDoc is null
            ? new OfflineGeoworldPreviewRunnerManifest()
            : Deserialize<OfflineGeoworldPreviewRunnerManifest>(manifestDoc.RootElement.GetRawText())
              ?? new OfflineGeoworldPreviewRunnerManifest();
        var commands = commandDoc is null
            ? new OfflineGeoworldPreviewFeatureCommandCatalog()
            : Deserialize<OfflineGeoworldPreviewFeatureCommandCatalog>(commandDoc.RootElement.GetRawText())
              ?? new OfflineGeoworldPreviewFeatureCommandCatalog();
        var travel = travelDoc is null
            ? new OfflineGeoworldPreviewTravelWindowScript()
            : Deserialize<OfflineGeoworldPreviewTravelWindowScript>(travelDoc.RootElement.GetRawText())
              ?? new OfflineGeoworldPreviewTravelWindowScript();

        var acceptedFalse = report.Contains("- accepted: false", StringComparison.OrdinalIgnoreCase)
                            || (manifestDoc is not null
                                && !TryGetBool(manifestDoc.RootElement, "accepted"));
        var countsOk = manifest.CommandCount == 18
                       && manifest.CommandKindCount == 10
                       && manifest.PayloadFileCount == 5
                       && manifest.TravelWindowStepCount >= 4
                       && commands.CommandCount == 18
                       && commands.CommandKindCount == 10
                       && travel.StepCount >= 4;
        var payloadFilesExist = OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredPayloadFileNames.All(fileName =>
            File.Exists(Resolve(
                root,
                OfflineGeoworldUnityEditorPreviewToolVocabulary.Goal101StreamingAssetsRelativeRoot
                + "/"
                + fileName)));
        var simulatedPassed = simulatedDoc is not null && TryGetBool(simulatedDoc.RootElement, "passed");
        var negativePassed = negativeDoc is not null && TryGetBool(negativeDoc.RootElement, "passed");
        var qualityPassed = qualityDoc is not null && TryGetBool(qualityDoc.RootElement, "passed");
        var scriptsReady = scriptsDoc is not null && TryGetBool(scriptsDoc.RootElement, "passed");
        var alphaUnchanged = qualityDoc is not null
            && TryGetBool(qualityDoc.RootElement, "alphaRuntimeBootstrapUnchanged");

        AddIfFalse(acceptedFalse, "goal102.source.goal101_accepted", "Goal101", diagnostics);
        AddIfFalse(countsOk, "goal102.source.goal101_counts", "Goal101", diagnostics);
        AddIfFalse(payloadFilesExist, "goal102.source.goal101_payload", "Goal101 StreamingAssets", diagnostics);
        AddIfFalse(simulatedPassed, "goal102.source.goal101_simulated", "Goal101", diagnostics);
        AddIfFalse(negativePassed, "goal102.source.goal101_negative", "Goal101", diagnostics);
        AddIfFalse(qualityPassed, "goal102.source.goal101_quality", "Goal101", diagnostics);
        AddIfFalse(scriptsReady, "goal102.source.goal101_scripts", "Goal101", diagnostics);
        AddIfFalse(alphaUnchanged, "goal102.source.goal101_alpha", "Goal101", diagnostics);

        return new Goal101EditorPreviewContext(
            Manifest: manifest,
            Commands: commands,
            TravelWindowScript: travel,
            ManifestJson: manifestDoc?.RootElement.GetRawText() ?? string.Empty,
            CommandCatalogJson: commandDoc?.RootElement.GetRawText() ?? string.Empty,
            TravelWindowJson: travelDoc?.RootElement.GetRawText() ?? string.Empty,
            Goal101AcceptedFalse: acceptedFalse,
            Goal101CountsProven: countsOk,
            Goal101PayloadFilesExist: payloadFilesExist,
            Goal101SimulatedCommandProofPassed: simulatedPassed,
            Goal101NegativeProofPassed: negativePassed,
            Goal101QualityGatePassed: qualityPassed,
            Goal101UnityScriptsReady: scriptsReady,
            Goal101AlphaRuntimeBootstrapUnchanged: alphaUnchanged,
            Diagnostics: SortDiagnostics(diagnostics));
    }

    private static OfflineGeoworldUnityEditorReport BuildReport(
        OfflineGeoworldUnityEditorToolInventory inventory,
        OfflineGeoworldUnityEditorSimulatedActionProof proof,
        OfflineGeoworldUnityEditorNegativeProof negative,
        OfflineGeoworldUnityEditorWorkspaceBindingInventory binding,
        OfflineGeoworldUnityEditorQualityGateScan quality,
        IReadOnlyDictionary<string, string> evidence) =>
        new()
        {
            CommandCount = proof.CommandCount,
            CommandKindCount = proof.CommandKindCount,
            TravelWindowStepCount = proof.TravelWindowStepCount,
            ExpectedObjectCount = proof.ExpectedObjectCount,
            UnityPayloadFileCount = quality.UnityPayloadFileCount,
            EditorWindowScriptReady = inventory.Passed,
            SimulatedActionProofPassed = proof.Passed,
            ClearOperationProofPassed = proof.ClearOperationModelPassed,
            NegativeProofPassed = negative.Passed,
            WorkspaceBindingPassed = binding.Passed,
            AlphaRuntimeBootstrapUnchanged = quality.AlphaRuntimeBootstrapUnchanged,
            QualityGatePassed = quality.Passed,
            ToolInventoryHash =
                Hash(evidence[OfflineGeoworldUnityEditorPreviewToolVocabulary.ToolInventoryFileName]),
            SimulatedActionProofHash =
                Hash(evidence[OfflineGeoworldUnityEditorPreviewToolVocabulary.SimulatedActionProofFileName]),
            NegativeProofHash =
                Hash(evidence[OfflineGeoworldUnityEditorPreviewToolVocabulary.NegativeProofFileName]),
            WorkspaceBindingInventoryHash =
                Hash(evidence[OfflineGeoworldUnityEditorPreviewToolVocabulary.WorkspaceBindingInventoryFileName]),
            SourceLineageHash =
                Hash(evidence[OfflineGeoworldUnityEditorPreviewToolVocabulary.SourceLineageFileName]),
            QualityGateHash =
                Hash(evidence[OfflineGeoworldUnityEditorPreviewToolVocabulary.QualityGateScanFileName])
        };

    private static string RenderReport(
        OfflineGeoworldUnityEditorReport report,
        OfflineGeoworldUnityEditorQualityGateScan quality,
        OfflineGeoworldUnityEditorSimulatedActionProof proof) =>
        string.Join(Environment.NewLine,
        [
            "# Goal 102 Offline Geoworld Unity Editor Preview Tool",
            string.Empty,
            "- implementationStatus: " + report.ImplementationStatus,
            "- accepted: false",
            "- manualGate: " + report.ManualGate + " required",
            "- deterministicReportHash: " + report.DeterministicReportHash,
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal102 adds Unity Editor-only tooling and read-only workspace evidence over the real Goal101 offline geoworld preview runner payload. It lets a reviewer open a Unity Editor window, refresh payload status, create placeholder preview objects on demand and clear them. It does not implement Runtime gameplay, scene or prefab production, final art, atlas output, real geodata fetching, providers, Lua, public schema changes or release build behavior.",
            string.Empty,
            "## Counts",
            string.Empty,
            "- commandCount: " + report.CommandCount,
            "- commandKindCount: " + report.CommandKindCount,
            "- travelWindowStepCount: " + report.TravelWindowStepCount,
            "- expectedObjectCount: " + report.ExpectedObjectCount,
            "- unityPayloadFileCount: " + report.UnityPayloadFileCount,
            string.Empty,
            "## Unity Editor Tool",
            string.Empty,
            "- editorWindowScriptPath: " + quality.EditorWindowScriptPath,
            "- menuItemMarker: " + quality.MenuItemMarker,
            "- payloadPath: " + quality.Goal101PayloadPath,
            "- manualInstructions: " + quality.ManualInstructions,
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
            "- goal101Consumed: " + quality.Goal101Consumed.ToString().ToLowerInvariant(),
            "- editorWindowScriptReady: " + quality.EditorWindowScriptReady.ToString().ToLowerInvariant(),
            "- menuItemMarkerPresent: " + quality.MenuItemMarkerPresent.ToString().ToLowerInvariant(),
            "- goal101PayloadPathMarkerPresent: " + quality.Goal101PayloadPathMarkerPresent.ToString().ToLowerInvariant(),
            "- createPreviewObjectsMethodPresent: " + quality.CreatePreviewObjectsMethodPresent.ToString().ToLowerInvariant(),
            "- clearPreviewObjectsMethodPresent: " + quality.ClearPreviewObjectsMethodPresent.ToString().ToLowerInvariant(),
            "- simulatedActionProofPassed: " + report.SimulatedActionProofPassed.ToString().ToLowerInvariant(),
            "- clearOperationProofPassed: " + report.ClearOperationProofPassed.ToString().ToLowerInvariant(),
            "- negativeProofPassed: " + report.NegativeProofPassed.ToString().ToLowerInvariant(),
            "- workspaceBindingPassed: " + report.WorkspaceBindingPassed.ToString().ToLowerInvariant(),
            "- alphaRuntimeBootstrapUnchanged: " + report.AlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "- noNetworkOrProviderImplementation: " + quality.NoNetworkOrProviderImplementation.ToString().ToLowerInvariant(),
            "- noRawGeodataDump: " + quality.NoRawGeodataDump.ToString().ToLowerInvariant(),
            "- noAbsolutePaths: " + quality.NoAbsolutePaths.ToString().ToLowerInvariant(),
            "- noBinaryOrRasterMedia: " + quality.NoBinaryOrRasterMedia.ToString().ToLowerInvariant(),
            "- noScenePrefabSettingsChanges: " + quality.NoScenePrefabSettingsChanges.ToString().ToLowerInvariant(),
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            "- toolInventoryHash: " + report.ToolInventoryHash,
            "- simulatedActionProofHash: " + report.SimulatedActionProofHash,
            "- negativeProofHash: " + report.NegativeProofHash,
            "- workspaceBindingInventoryHash: " + report.WorkspaceBindingInventoryHash,
            "- sourceLineageHash: " + report.SourceLineageHash,
            "- qualityGateHash: " + report.QualityGateHash
        ]) + Environment.NewLine;

    private static IReadOnlyDictionary<string, string> BuildEvidencePayloads(
        OfflineGeoworldUnityEditorToolInventory inventory,
        OfflineGeoworldUnityEditorSimulatedActionProof proof,
        OfflineGeoworldUnityEditorNegativeProof negative,
        OfflineGeoworldUnityEditorWorkspaceBindingInventory binding,
        OfflineGeoworldUnityEditorSourceLineage lineage,
        OfflineGeoworldUnityEditorQualityGateScan quality) =>
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldUnityEditorPreviewToolVocabulary.ToolInventoryFileName] =
                Serialize(inventory),
            [OfflineGeoworldUnityEditorPreviewToolVocabulary.SimulatedActionProofFileName] =
                Serialize(proof),
            [OfflineGeoworldUnityEditorPreviewToolVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [OfflineGeoworldUnityEditorPreviewToolVocabulary.WorkspaceBindingInventoryFileName] =
                Serialize(binding),
            [OfflineGeoworldUnityEditorPreviewToolVocabulary.SourceLineageFileName] =
                Serialize(lineage),
            [OfflineGeoworldUnityEditorPreviewToolVocabulary.QualityGateScanFileName] =
                Serialize(quality)
        };

    private sealed record Goal101EditorPreviewContext(
        OfflineGeoworldPreviewRunnerManifest Manifest,
        OfflineGeoworldPreviewFeatureCommandCatalog Commands,
        OfflineGeoworldPreviewTravelWindowScript TravelWindowScript,
        string ManifestJson,
        string CommandCatalogJson,
        string TravelWindowJson,
        bool Goal101AcceptedFalse,
        bool Goal101CountsProven,
        bool Goal101PayloadFilesExist,
        bool Goal101SimulatedCommandProofPassed,
        bool Goal101NegativeProofPassed,
        bool Goal101QualityGatePassed,
        bool Goal101UnityScriptsReady,
        bool Goal101AlphaRuntimeBootstrapUnchanged,
        IReadOnlyList<OfflineGeoworldUnityEditorPreviewDiagnostic> Diagnostics);
}
