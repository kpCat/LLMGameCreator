using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldUnityPreviewRunner;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private const string Goal101SourceGoalId =
        "goal_101_offline_geoworld_unity_preview_runner";
    private const string Goal101SourceRoot =
        ".llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner";
    private const string Goal101StreamingAssetsRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal101";

    private static VisualWorldPreviewArtifactGroup BuildOfflineGeoworldUnityPreviewGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldUnityPreviewSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                Goal101SourceRoot,
                Goal101SourceGoalId,
                [
                    (OfflineGeoworldUnityPreviewRunnerVocabulary.ReportMarkdownFileName,
                        "offline_geoworld_unity_preview_report"),
                    (OfflineGeoworldUnityPreviewRunnerVocabulary.CommandCatalogFileName,
                        "offline_geoworld_unity_preview_command_catalog"),
                    (OfflineGeoworldUnityPreviewRunnerVocabulary.StyleLegendFileName,
                        "offline_geoworld_unity_preview_style_legend"),
                    (OfflineGeoworldUnityPreviewRunnerVocabulary.TravelWindowScriptFileName,
                        "offline_geoworld_unity_preview_travel_window"),
                    (OfflineGeoworldUnityPreviewRunnerVocabulary.StreamingAssetsLedgerFileName,
                        "offline_geoworld_unity_preview_streamingassets_ledger"),
                    (OfflineGeoworldUnityPreviewRunnerVocabulary.UnityScriptInventoryFileName,
                        "offline_geoworld_unity_preview_script_inventory"),
                    (OfflineGeoworldUnityPreviewRunnerVocabulary.SimulatedCommandProofFileName,
                        "offline_geoworld_unity_preview_simulated_command_proof"),
                    (OfflineGeoworldUnityPreviewRunnerVocabulary.NegativeProofFileName,
                        "offline_geoworld_unity_preview_negative_proof"),
                    (OfflineGeoworldUnityPreviewRunnerVocabulary.WorkspaceBindingInventoryFileName,
                        "offline_geoworld_unity_preview_workspace_binding_inventory"),
                    (OfflineGeoworldUnityPreviewRunnerVocabulary.SourceLineageFileName,
                        "offline_geoworld_unity_preview_source_lineage"),
                    (OfflineGeoworldUnityPreviewRunnerVocabulary.QualityGateScanFileName,
                        "offline_geoworld_unity_preview_quality_gate")
                ],
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldUnityPreviewSummary(entry, summary))
            .ToList();

        foreach (var fileName in OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredPayloadFileNames)
        {
            var relativePath = Goal101StreamingAssetsRoot + "/" + fileName;
            var exists = File.Exists(Resolve(projectRoot, relativePath));
            entries.Add(WithOfflineGeoworldUnityPreviewSummary(
                new VisualWorldPreviewArtifactEntry
                {
                    Id = Goal101SourceGoalId + ".payload." + Path.GetFileNameWithoutExtension(fileName),
                    RelativePath = relativePath,
                    ArtifactKind = "offline_geoworld_unity_preview_streamingassets_payload",
                    SourceGoalId = Goal101SourceGoalId,
                    Sha256 = exists
                        ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                        : string.Empty,
                    Status = exists
                        ? VisualWorldPreviewArtifactStatus.Passed
                        : VisualWorldPreviewArtifactStatus.Failed,
                    DiagnosticSummary = exists ? "mirrored preview payload exists" : "mirrored preview payload missing",
                    SafeRatingMetadataSummary = "metadataOnly=true; relativePath=true"
                },
                summary));
        }

        foreach (var scriptPath in UnityScriptPaths())
        {
            var exists = File.Exists(Resolve(projectRoot, scriptPath));
            entries.Add(WithOfflineGeoworldUnityPreviewSummary(
                new VisualWorldPreviewArtifactEntry
                {
                    Id = Goal101SourceGoalId + ".script." + Path.GetFileNameWithoutExtension(scriptPath),
                    RelativePath = scriptPath,
                    ArtifactKind = "offline_geoworld_unity_preview_script",
                    SourceGoalId = Goal101SourceGoalId,
                    Sha256 = exists
                        ? HashFor(projectRoot, scriptPath, new Dictionary<string, string>(StringComparer.Ordinal))
                        : string.Empty,
                    Status = exists
                        ? VisualWorldPreviewArtifactStatus.Passed
                        : VisualWorldPreviewArtifactStatus.Failed,
                    DiagnosticSummary = exists ? "Unity preview runner script exists" : "Unity preview runner script missing",
                    MetadataOnly = true
                },
                summary));
        }

        entries.Add(WithOfflineGeoworldUnityPreviewSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = Goal101SourceGoalId + ".summary",
                RelativePath = Goal101SourceRoot + "/"
                    + OfflineGeoworldUnityPreviewRunnerVocabulary.QualityGateScanFileName,
                ArtifactKind = "offline_geoworld_unity_preview_workspace_summary",
                SourceGoalId = Goal101SourceGoalId,
                Sha256 = HashFor(
                    projectRoot,
                    Goal101SourceRoot + "/"
                    + OfflineGeoworldUnityPreviewRunnerVocabulary.QualityGateScanFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.Passed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "commands=" + summary.CommandCount
                    + "; kinds=" + summary.CommandKindCount
                    + "; travelSteps=" + summary.TravelWindowStepCount
                    + "; payloads=" + summary.PayloadFileCount,
                SafeRatingMetadataSummary = summary.CommandKindCoverageSummary
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_unity_preview",
            "Goal 101 Offline Geoworld Unity Preview",
            Goal101SourceGoalId,
            Goal101SourceRoot,
            entries,
            groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactEntry WithOfflineGeoworldUnityPreviewSummary(
        VisualWorldPreviewArtifactEntry entry,
        OfflineGeoworldUnityPreviewWorkspaceSummary summary) =>
        entry with
        {
            PayloadFileCount = summary.PayloadFileCount,
            OfflineGeoworldUnityPreviewCommandCount = summary.CommandCount,
            OfflineGeoworldUnityPreviewCommandKindCount = summary.CommandKindCount,
            OfflineGeoworldUnityPreviewTravelWindowStepCount = summary.TravelWindowStepCount,
            OfflineGeoworldUnityPreviewKindCoverageSummary = summary.CommandKindCoverageSummary,
            OfflineGeoworldUnityPreviewUnityScriptsReady = summary.UnityScriptsReady,
            OfflineGeoworldUnityPreviewSimulatedCommandProofPassed =
                summary.SimulatedCommandProofPassed,
            OfflineGeoworldUnityPreviewQualityGatePassed = summary.QualityGatePassed,
            NegativeProofPassed = summary.NegativeProofPassed,
            AlphaRuntimeBootstrapUnchanged = summary.AlphaRuntimeBootstrapUnchanged,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static OfflineGeoworldUnityPreviewWorkspaceSummary LoadOfflineGeoworldUnityPreviewSummary(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        using var manifest = TryReadJson(
            projectRoot,
            Goal101SourceRoot + "/" + OfflineGeoworldUnityPreviewRunnerVocabulary.ManifestFileName,
            diagnostics);
        using var proof = TryReadJson(
            projectRoot,
            Goal101SourceRoot + "/"
            + OfflineGeoworldUnityPreviewRunnerVocabulary.SimulatedCommandProofFileName,
            diagnostics);
        using var scripts = TryReadJson(
            projectRoot,
            Goal101SourceRoot + "/"
            + OfflineGeoworldUnityPreviewRunnerVocabulary.UnityScriptInventoryFileName,
            diagnostics);
        using var negative = TryReadJson(
            projectRoot,
            Goal101SourceRoot + "/" + OfflineGeoworldUnityPreviewRunnerVocabulary.NegativeProofFileName,
            diagnostics);
        using var quality = TryReadJson(
            projectRoot,
            Goal101SourceRoot + "/"
            + OfflineGeoworldUnityPreviewRunnerVocabulary.QualityGateScanFileName,
            diagnostics);

        var commandCount = manifest is null ? 0 : ReadGoal101Int(manifest.RootElement, "commandCount");
        var kindCount = manifest is null ? 0 : ReadGoal101Int(manifest.RootElement, "commandKindCount");
        var travelSteps = manifest is null ? 0 : ReadGoal101Int(manifest.RootElement, "travelWindowStepCount");
        var payloadCount = manifest is null ? 0 : ReadGoal101Int(manifest.RootElement, "payloadFileCount");
        var kindSummary = proof is null ? string.Empty : ReadCommandKindCountsSummary(proof.RootElement);
        var scriptsReady = scripts is not null && TryGetBool(scripts.RootElement, "passed");
        var simulatedPassed = proof is not null && TryGetBool(proof.RootElement, "passed");
        var negativePassed = negative is not null && TryGetBool(negative.RootElement, "passed");
        var qualityPassed = quality is not null && TryGetBool(quality.RootElement, "passed");
        var alphaUnchanged = quality is not null
            && TryGetBool(quality.RootElement, "alphaRuntimeBootstrapUnchanged");
        var relativePaths = IsSafeRelativePath(Goal101SourceRoot)
                            && IsSafeRelativePath(Goal101StreamingAssetsRoot)
                            && UnityScriptPaths().All(IsSafeRelativePath);
        var passed = commandCount == 18
                     && kindCount == 10
                     && travelSteps >= 4
                     && payloadCount == 5
                     && !string.IsNullOrWhiteSpace(kindSummary)
                     && scriptsReady
                     && simulatedPassed
                     && negativePassed
                     && alphaUnchanged
                     && qualityPassed
                     && relativePaths;
        AddIfFalse(passed, "goal101.workspace.summary_failed", "offline_geoworld_unity_preview", diagnostics);
        return new OfflineGeoworldUnityPreviewWorkspaceSummary(
            Passed: passed,
            CommandCount: commandCount,
            CommandKindCount: kindCount,
            TravelWindowStepCount: travelSteps,
            PayloadFileCount: payloadCount,
            CommandKindCoverageSummary: kindSummary,
            UnityScriptsReady: scriptsReady,
            SimulatedCommandProofPassed: simulatedPassed,
            NegativeProofPassed: negativePassed,
            AlphaRuntimeBootstrapUnchanged: alphaUnchanged,
            QualityGatePassed: qualityPassed);
    }

    private static string ReadCommandKindCountsSummary(JsonElement element)
    {
        if (!element.TryGetProperty("commandCountByKind", out var counts)
            || counts.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        return string.Join(
            "; ",
            counts.EnumerateObject()
                .OrderBy(item => item.Name, StringComparer.Ordinal)
                .Select(item => item.Name + "=" + item.Value.GetInt32()));
    }

    private static int ReadGoal101Int(JsonElement element, string propertyName) =>
        TryGetInt(element, propertyName, out var value) ? value : 0;

    private static IReadOnlyList<string> UnityScriptPaths() =>
    [
        OfflineGeoworldUnityPreviewRunnerVocabulary.UnityPreviewRunnerScriptPath,
        OfflineGeoworldUnityPreviewRunnerVocabulary.UnityPrimitiveFactoryScriptPath,
        OfflineGeoworldUnityPreviewRunnerVocabulary.UnityTravelWindowScriptPath
    ];

    private sealed record OfflineGeoworldUnityPreviewWorkspaceSummary(
        bool Passed,
        int CommandCount,
        int CommandKindCount,
        int TravelWindowStepCount,
        int PayloadFileCount,
        string CommandKindCoverageSummary,
        bool UnityScriptsReady,
        bool SimulatedCommandProofPassed,
        bool NegativeProofPassed,
        bool AlphaRuntimeBootstrapUnchanged,
        bool QualityGatePassed);
}
