using System.Text;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceExportPackage;

public sealed partial class OfflineGeoworldAlphaSliceExportPackageEvidenceService
{
    private static OfflineGeoworldAlphaSliceExportQualityGateScan BuildQualityGate(
        string root,
        OfflineGeoworldAlphaSliceExportManifest manifest,
        OfflineGeoworldAlphaSliceExportFileIndex fileIndex,
        OfflineGeoworldAlphaSliceExportChecksums checksums,
        OfflineGeoworldAlphaSliceExportCleanImportProof cleanImport,
        OfflineGeoworldAlphaSliceExportNegativeProof negative,
        OfflineGeoworldAlphaSliceExportUnityScriptInventory unity,
        OfflineGeoworldAlphaSliceExportEditorWindowInventory editor,
        OfflineGeoworldAlphaSliceExportWorkspaceBindingInventory workspace,
        OfflineGeoworldAlphaSliceExportSourceLineage lineage,
        IReadOnlyDictionary<string, string> packageFiles)
    {
        var diagnostics = new List<string>();
        void Require(bool condition, string code)
        {
            if (!condition)
            {
                diagnostics.Add(code);
            }
        }

        var sourceFiles = OfflineGeoworldAlphaSliceExportPackageVocabulary.SourceHealthFiles
            .Where(path => File.Exists(Resolve(root, path)))
            .Select(path => new
            {
                Path = path,
                Text = File.ReadAllText(Resolve(root, path), Encoding.UTF8)
            })
            .ToList();
        var noSceneMutation = sourceFiles
            .Where(item => item.Path.StartsWith("unity/", StringComparison.Ordinal))
            .All(item => !ContainsAny(item.Text, "EditorBuildSettings", "SceneManager.SaveScene",
                ".unity", ".prefab", "ProjectSettings", "Packages/manifest.json"));
        var sourceHealthPassed = sourceFiles.Count >= 5
                                 && sourceFiles.All(item => CountLines(item.Text) < 700);
        var manifestPassed = manifest.PackageFileCount == 6
                             && manifest.IndexedFileCount == 5
                             && manifest.SourceComponentCount == 7
                             && manifest.ReadySourceComponentCount == 7
                             && manifest.Goal108AcceptedFalse
                             && manifest.Goal108AImmutabilityAuditIncluded
                             && manifest.Goal101To107HistoricalArtifactsUnchanged
                             && manifest.AlphaRuntimeBootstrapUnchanged
                             && manifest.ManualGates.Count >= 9;
        var fileIndexPassed = fileIndex.IndexedFileCount == 5
                              && fileIndex.Files.All(file => file.PackageRelativePath
                                                             && IsSafeRelativePath(file.RelativePath));
        var checksumsPassed = checksums.HashedFileCount == fileIndex.IndexedFileCount
                              && fileIndex.Files.All(file =>
                                  checksums.Sha256ByRelativePath.ContainsKey(file.RelativePath));
        var sourceLineagePassed = lineage.Goal108ManifestRead
                                  && lineage.Goal108ComponentsRead
                                  && lineage.Goal108AImmutabilityAuditRead
                                  && lineage.Goal108AHistoricalDiffAuditRead
                                  && lineage.ComponentCount == 7
                                  && lineage.ReadyComponentCount == 7
                                  && lineage.SourceHashCount > 0;
        Require(manifestPassed, "goal109.manifest");
        Require(fileIndexPassed, "goal109.file_index");
        Require(checksumsPassed, "goal109.checksums");
        Require(cleanImport.Passed, "goal109.clean_import");
        Require(negative.Passed, "goal109.negative_proof");
        Require(unity.Passed, "goal109.unity_verifier");
        Require(editor.Passed, "goal109.editor_window");
        Require(workspace.Passed, "goal109.workspace_binding");
        Require(sourceLineagePassed, "goal109.source_lineage");
        Require(noSceneMutation, "goal109.no_scene_settings_mutation");
        Require(sourceHealthPassed, "goal109.source_health");

        return new OfflineGeoworldAlphaSliceExportQualityGateScan
        {
            Passed = diagnostics.Count == 0,
            ManifestPassed = manifestPassed,
            FileIndexPassed = fileIndexPassed,
            ChecksumsPassed = checksumsPassed,
            CleanImportProofPassed = cleanImport.Passed,
            NegativeProofPassed = negative.Passed,
            UnityScriptInventoryPassed = unity.Passed,
            EditorWindowInventoryPassed = editor.Passed,
            WorkspaceBindingPassed = workspace.Passed,
            SourceLineagePassed = sourceLineagePassed,
            AlphaRuntimeBootstrapUnchanged = manifest.AlphaRuntimeBootstrapUnchanged,
            NoRawGeodata = cleanImport.NoRawGeodata || !cleanImport.PackageRootReadAttempted,
            NoAbsolutePaths = cleanImport.NoAbsolutePaths || !cleanImport.PackageRootReadAttempted,
            NoBinaryOrRasterMedia = cleanImport.NoBinaryOrRasterMedia || !cleanImport.PackageRootReadAttempted,
            NoNetworkProviderMarkers = cleanImport.NoNetworkProviderMarkers || !cleanImport.PackageRootReadAttempted,
            NoScenePrefabSettingsProjectPackageMutation = noSceneMutation,
            SourceHealthLimitsPassed = sourceHealthPassed,
            PackageFileCount = packageFiles.Count,
            IndexedFileCount = fileIndex.IndexedFileCount,
            SourceComponentCount = lineage.ComponentCount,
            ReadySourceComponentCount = lineage.ReadyComponentCount,
            NegativeRejectedCount = negative.RejectedCount,
            ScannedCSharpFileCount = sourceFiles.Count,
            MaxLogicalLineCount = sourceFiles.Count == 0 ? 0 : sourceFiles.Max(item => CountLines(item.Text)),
            PackageAggregateHash = HashText(string.Join(
                "\n",
                packageFiles.OrderBy(item => item.Key, StringComparer.Ordinal)
                    .Select(item => item.Key + ":" + HashText(item.Value)))),
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceExportPackage/",
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
                "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaSliceExportPackage/",
                "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaSliceExportPackageProductSmokeTests.cs",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaSlicePackageVerifier.cs",
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldAlphaSlicePackageWindow.cs",
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal109/",
                ".llmgc/procedural/goal-109-offline-geoworld-alpha-slice-export-package/",
                ".llmgc/exports/goal-109-offline-geoworld-alpha-slice/",
                "docs/agent-tasks/goal-109-offline-geoworld-alpha-slice-export-package/",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
                ".devflow/artifact-scope/artifact-scope-policy.json"
            ],
            Diagnostics = diagnostics
        };
    }

    private static OfflineGeoworldAlphaSliceExportReport BuildReport(
        OfflineGeoworldAlphaSliceExportManifest manifest,
        OfflineGeoworldAlphaSliceExportCleanImportProof cleanImport,
        OfflineGeoworldAlphaSliceExportNegativeProof negative,
        OfflineGeoworldAlphaSliceExportUnityScriptInventory unity,
        OfflineGeoworldAlphaSliceExportEditorWindowInventory editor,
        OfflineGeoworldAlphaSliceExportWorkspaceBindingInventory workspace,
        OfflineGeoworldAlphaSliceExportQualityGateScan quality) =>
        new()
        {
            ImplementationStatus = quality.Passed ? "GREEN" : "FAILED",
            Accepted = false,
            PackageFileCount = quality.PackageFileCount,
            IndexedFileCount = quality.IndexedFileCount,
            SourceComponentCount = quality.SourceComponentCount,
            ReadySourceComponentCount = quality.ReadySourceComponentCount,
            CleanImportProofPassed = cleanImport.Passed,
            NegativeProofPassed = negative.Passed,
            NegativeRejectedCount = negative.RejectedCount,
            UnityScriptInventoryPassed = unity.Passed,
            EditorWindowInventoryPassed = editor.Passed,
            WorkspaceBindingPassed = workspace.Passed,
            AlphaRuntimeBootstrapUnchanged = manifest.AlphaRuntimeBootstrapUnchanged,
            Goal107FinalAcceptanceHash = manifest.Goal107FinalAcceptanceHash
        };

    private static string RenderReport(
        OfflineGeoworldAlphaSliceExportReport report,
        string deterministicReportHash)
    {
        var lines = new List<string>
        {
            "# Goal 109 Offline Geoworld Alpha Slice Export Package Report",
            string.Empty,
            "- implementationStatus: " + report.ImplementationStatus,
            "- accepted: false",
            "- manualGate: " + report.ManualGate + " required",
            "- deterministicReportHash: " + deterministicReportHash,
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 109 packages the Goal108 offline geoworld Alpha Slice into a portable directory package with file index, checksums, runbook, acceptance gate, clean-import proof, Unity verifier readiness and Visual World Stream Preview Workspace inspection. It is not a final Runtime, release build, real geodata import, provider path, scene/prefab mutation or final-art path.",
            string.Empty,
            "## Readiness",
            string.Empty,
            "- packageFileCount: " + report.PackageFileCount,
            "- indexedFileCount: " + report.IndexedFileCount,
            "- sourceComponentCount: " + report.SourceComponentCount,
            "- readySourceComponentCount: " + report.ReadySourceComponentCount,
            "- cleanImportProofPassed: " + report.CleanImportProofPassed.ToString().ToLowerInvariant(),
            "- negativeProofPassed: " + report.NegativeProofPassed.ToString().ToLowerInvariant(),
            "- negativeRejectedCount: " + report.NegativeRejectedCount,
            "- unityScriptInventoryPassed: " + report.UnityScriptInventoryPassed.ToString().ToLowerInvariant(),
            "- editorWindowInventoryPassed: " + report.EditorWindowInventoryPassed.ToString().ToLowerInvariant(),
            "- workspaceBindingPassed: " + report.WorkspaceBindingPassed.ToString().ToLowerInvariant(),
            "- alphaRuntimeBootstrapUnchanged: " + report.AlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant(),
            "- goal107FinalAcceptanceHash: " + report.Goal107FinalAcceptanceHash
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}
