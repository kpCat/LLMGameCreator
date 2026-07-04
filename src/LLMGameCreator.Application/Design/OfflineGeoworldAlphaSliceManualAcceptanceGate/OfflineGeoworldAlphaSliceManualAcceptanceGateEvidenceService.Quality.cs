using System.Text;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;

public sealed partial class OfflineGeoworldAlphaSliceManualAcceptanceGateEvidenceService
{
    private static OfflineGeoworldAlphaAcceptanceQualityGateScan BuildQualityGate(
        string root,
        OfflineGeoworldAlphaAcceptanceManifest manifest,
        OfflineGeoworldAlphaAcceptanceChecklist checklist,
        OfflineGeoworldAlphaAcceptanceResultTemplate resultTemplate,
        OfflineGeoworldAlphaReleaseGateDashboard dashboard,
        OfflineGeoworldAlphaAcceptanceFileIndex fileIndex,
        OfflineGeoworldAlphaAcceptanceChecksums checksums,
        OfflineGeoworldAlphaAcceptanceSourceLineage lineage,
        OfflineGeoworldAlphaAcceptanceUnityScriptInventory unity,
        OfflineGeoworldAlphaAcceptanceEditorWindowInventory editor,
        OfflineGeoworldAlphaAcceptanceSimulatedProof simulated,
        OfflineGeoworldAlphaAcceptanceNegativeProof negative,
        OfflineGeoworldAlphaAcceptanceWorkspaceBindingInventory workspace,
        IReadOnlyDictionary<string, string> exportFiles)
    {
        var diagnostics = new List<string>();
        void Require(bool condition, string code)
        {
            if (!condition)
            {
                diagnostics.Add(code);
            }
        }

        var sourceFiles = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.SourceHealthFiles
            .Where(path => File.Exists(Resolve(root, path)))
            .Select(path => new
            {
                Path = path,
                Text = File.ReadAllText(Resolve(root, path), Encoding.UTF8)
            })
            .ToList();
        var outputText = string.Join("\n", exportFiles.Values);
        var noAbsolutePaths = !Path.IsPathFullyQualified(outputText)
                              && !ContainsAny(outputText, root, "C:\\", "C:/", "\\\\");
        var noRawGeodata = !ContainsAny(outputText, ".osm", ".pbf", ".mbtiles", ".gpkg", ".geojson",
            "\"rawGeodataIncluded\": true", "\"noRawGeodata\": false");
        var noNetwork = !ContainsAny(outputText, "UnityWebRequest", "HttpClient", "http://", "https://",
            "\"networkProviderMarker\": true");
        var noBinary = exportFiles.Keys.All(path => !IsBinaryOrRasterMedia(path))
                       && !ContainsAny(outputText, ".png", ".jpg", ".jpeg", ".webp", ".gif", ".bmp",
                           ".wav", ".mp3", ".ogg", ".mp4", ".bytes");
        var noSceneSettings = sourceFiles
            .Where(item => item.Path.StartsWith("unity/", StringComparison.Ordinal))
            .All(item => !ContainsAny(item.Text, "InitializeOnLoad", "PostProcessScene",
                "EditorBuildSettings", "SceneManager.SaveScene", "AssetDatabase.CreateAsset",
                ".unity", ".prefab", "ProjectSettings", "Packages/manifest.json"));
        var sourceHealthPassed = sourceFiles.Count >= 7
                                 && sourceFiles.All(item => CountLines(item.Text) < 700);
        var manifestPassed = manifest.PayloadFileCount == 5
                             && manifest.ExportFileCount == 7
                             && manifest.ChecklistStepCount >= 12
                             && manifest.Goal109PackageFileCount == 6
                             && manifest.Goal109IndexedFileCount == 5
                             && manifest.Goal109SourceComponentCount == 7
                             && manifest.Goal109AcceptedFalse
                             && manifest.Goal109CleanImportProofPassed
                             && manifest.Goal109NegativeProofPassed
                             && manifest.UnityAcceptanceRunnerReady
                             && manifest.ManualAcceptancePending
                             && manifest.AlphaRuntimeBootstrapUnchanged;
        var checklistPassed = checklist.StepCount >= 12
                              && OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary
                                  .RequiredChecklistStepIds
                                  .All(id => checklist.Steps.Any(step => step.StepId == id));
        var templatePassed = resultTemplate.Steps.Count == checklist.StepCount
                             && resultTemplate.ManualAcceptancePending
                             && !string.IsNullOrWhiteSpace(resultTemplate.ChecklistHash);
        var dashboardPassed = dashboard.ManualAcceptancePending
                              && dashboard.PackageReady
                              && dashboard.CleanImportProofPassed
                              && dashboard.UnityRunnerReady
                              && dashboard.ReleaseRiskLinks.Count >= 3
                              && dashboard.MilestoneGateLinks.Count >= 2;
        var fileIndexPassed = fileIndex.IndexedFileCount == 6
                              && fileIndex.Files.All(file =>
                                  file.PackageRelativePath && IsSafeRelativePath(file.RelativePath));
        var checksumsPassed = checksums.HashedFileCount == fileIndex.IndexedFileCount
                              && fileIndex.Files.All(file =>
                                  checksums.Sha256ByRelativePath.ContainsKey(file.RelativePath));
        var sourceLineagePassed = lineage.Goal109PackageManifestRead
                                  && lineage.Goal109FileIndexRead
                                  && lineage.Goal109ChecksumsRead
                                  && lineage.Goal109CleanImportProofRead
                                  && lineage.Goal109NegativeProofRead
                                  && lineage.Goal109QualityGateRead
                                  && lineage.Goal109AcceptedFalse
                                  && lineage.Goal109CleanImportProofPassed
                                  && lineage.Goal109NegativeProofPassed
                                  && lineage.Goal109SourceHashCount > 0;

        Require(manifestPassed, "goal110.manifest");
        Require(checklistPassed, "goal110.checklist");
        Require(templatePassed, "goal110.result_template");
        Require(dashboardPassed, "goal110.dashboard");
        Require(fileIndexPassed, "goal110.file_index");
        Require(checksumsPassed, "goal110.checksums");
        Require(sourceLineagePassed, "goal110.source_lineage");
        Require(unity.Passed, "goal110.unity_scripts");
        Require(editor.Passed, "goal110.editor_window");
        Require(simulated.Passed, "goal110.simulated_proof");
        Require(negative.Passed, "goal110.negative_proof");
        Require(workspace.Passed, "goal110.workspace_binding");
        Require(noAbsolutePaths, "goal110.no_absolute_paths");
        Require(noRawGeodata, "goal110.no_raw_geodata");
        Require(noNetwork, "goal110.no_network_provider_markers");
        Require(noBinary, "goal110.no_binary_or_raster_media");
        Require(noSceneSettings, "goal110.no_scene_prefab_settings_project_package_mutation");
        Require(sourceHealthPassed, "goal110.source_health");

        return new OfflineGeoworldAlphaAcceptanceQualityGateScan
        {
            Passed = diagnostics.Count == 0,
            ManifestPassed = manifestPassed,
            ChecklistPassed = checklistPassed,
            ResultTemplatePassed = templatePassed,
            DashboardPassed = dashboardPassed,
            FileIndexPassed = fileIndexPassed,
            ChecksumsPassed = checksumsPassed,
            SourceLineagePassed = sourceLineagePassed,
            UnityScriptInventoryPassed = unity.Passed,
            EditorWindowInventoryPassed = editor.Passed,
            SimulatedProofPassed = simulated.Passed,
            NegativeProofPassed = negative.Passed,
            WorkspaceBindingPassed = workspace.Passed,
            AutomatedGatePassed = manifest.AutomatedGatePassed && simulated.AutomatedGatePassed,
            AlphaRuntimeBootstrapUnchanged = manifest.AlphaRuntimeBootstrapUnchanged,
            NoRawGeodata = noRawGeodata,
            NoAbsolutePaths = noAbsolutePaths,
            NoBinaryOrRasterMedia = noBinary,
            NoNetworkProviderMarkers = noNetwork,
            NoScenePrefabSettingsProjectPackageMutation = noSceneSettings,
            SourceHealthLimitsPassed = sourceHealthPassed,
            PayloadFileCount = manifest.PayloadFileCount,
            ExportFileCount = manifest.ExportFileCount,
            IndexedFileCount = fileIndex.IndexedFileCount,
            ChecklistStepCount = checklist.StepCount,
            NegativeRejectedCount = negative.RejectedCount,
            ScannedCSharpFileCount = sourceFiles.Count,
            MaxLogicalLineCount = sourceFiles.Count == 0 ? 0 : sourceFiles.Max(item => CountLines(item.Text)),
            ExportAggregateHash = HashText(string.Join(
                "\n",
                exportFiles.OrderBy(item => item.Key, StringComparer.Ordinal)
                    .Select(item => item.Key + ":" + HashText(item.Value)))),
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceManualAcceptanceGate/",
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
                "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaSliceManualAcceptanceGate/",
                "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaSliceManualAcceptanceGateProductSmokeTests.cs",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResult.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResultStore.cs",
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldAlphaAcceptanceRunnerWindow.cs",
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal110/",
                ".llmgc/procedural/goal-110-offline-geoworld-alpha-manual-acceptance-gate/",
                ".llmgc/exports/goal-110-offline-geoworld-alpha-acceptance/",
                "docs/agent-tasks/goal-110-offline-geoworld-alpha-manual-acceptance-gate/",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                "docs/MILESTONE_GATES.md",
                "docs/RELEASE_RISK_REGISTER.md",
                "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
                ".devflow/artifact-scope/artifact-scope-policy.json"
            ],
            Diagnostics = diagnostics
        };
    }

    private static OfflineGeoworldAlphaAcceptanceReport BuildReport(
        OfflineGeoworldAlphaAcceptanceManifest manifest,
        OfflineGeoworldAlphaAcceptanceSourceLineage lineage,
        OfflineGeoworldAlphaAcceptanceUnityScriptInventory unity,
        OfflineGeoworldAlphaAcceptanceEditorWindowInventory editor,
        OfflineGeoworldAlphaAcceptanceSimulatedProof simulated,
        OfflineGeoworldAlphaAcceptanceNegativeProof negative,
        OfflineGeoworldAlphaAcceptanceWorkspaceBindingInventory workspace,
        OfflineGeoworldAlphaAcceptanceQualityGateScan quality) =>
        new()
        {
            ImplementationStatus = quality.Passed ? "GREEN" : "FAILED",
            Accepted = false,
            AutomatedGatePassed = quality.AutomatedGatePassed,
            PayloadFileCount = quality.PayloadFileCount,
            ExportFileCount = quality.ExportFileCount,
            ChecklistStepCount = quality.ChecklistStepCount,
            Goal109CleanImportProofPassed = lineage.Goal109CleanImportProofPassed,
            SimulatedProofPassed = simulated.Passed,
            NegativeProofPassed = negative.Passed,
            UnityScriptInventoryPassed = unity.Passed,
            EditorWindowInventoryPassed = editor.Passed,
            WorkspaceBindingPassed = workspace.Passed,
            AlphaRuntimeBootstrapUnchanged = manifest.AlphaRuntimeBootstrapUnchanged,
            SimulatedResultHash = simulated.SyntheticResultHash
        };

    private static string RenderReport(
        OfflineGeoworldAlphaAcceptanceReport report,
        string deterministicReportHash)
    {
        var lines = new List<string>
        {
            "# Goal 110 Offline Geoworld Alpha Manual Acceptance Gate Report",
            string.Empty,
            "- implementationStatus: " + report.ImplementationStatus,
            "- accepted: false",
            "- manualGate: " + report.ManualGate + " required",
            "- deterministicReportHash: " + deterministicReportHash,
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 110 turns the Goal109 portable Alpha Slice export into an explicit manual acceptance runner and release-gate dashboard. It remains metadata-only Alpha tooling and is not final release packaging, real geodata ingestion, provider/network work, Runtime consumption, scene/prefab mutation or final art.",
            string.Empty,
            "## Readiness",
            string.Empty,
            "- manualAcceptancePending: true",
            "- automatedGatePassed: " + report.AutomatedGatePassed.ToString().ToLowerInvariant(),
            "- payloadFileCount: " + report.PayloadFileCount,
            "- exportFileCount: " + report.ExportFileCount,
            "- checklistStepCount: " + report.ChecklistStepCount,
            "- goal109CleanImportProofPassed: "
            + report.Goal109CleanImportProofPassed.ToString().ToLowerInvariant(),
            "- simulatedProofPassed: " + report.SimulatedProofPassed.ToString().ToLowerInvariant(),
            "- negativeProofPassed: " + report.NegativeProofPassed.ToString().ToLowerInvariant(),
            "- unityScriptInventoryPassed: "
            + report.UnityScriptInventoryPassed.ToString().ToLowerInvariant(),
            "- editorWindowInventoryPassed: "
            + report.EditorWindowInventoryPassed.ToString().ToLowerInvariant(),
            "- workspaceBindingPassed: " + report.WorkspaceBindingPassed.ToString().ToLowerInvariant(),
            "- alphaRuntimeBootstrapUnchanged: "
            + report.AlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "- simulatedResultHash: " + report.SimulatedResultHash
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}
