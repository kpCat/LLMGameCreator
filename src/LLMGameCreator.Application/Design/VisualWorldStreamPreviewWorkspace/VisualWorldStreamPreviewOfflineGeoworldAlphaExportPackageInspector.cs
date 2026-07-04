using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceExportPackage;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildOfflineGeoworldAlphaExportPackageGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldAlphaExportPackageSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                OfflineGeoworldAlphaSliceExportPackageVocabulary.ProceduralOutputDirectory,
                OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId,
                BuildGoal109ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldAlphaExportPackageSummary(entry, summary))
            .ToList();

        foreach (var fileName in OfflineGeoworldAlphaSliceExportPackageVocabulary.RequiredPackageFileNames)
        {
            entries.Add(WithOfflineGeoworldAlphaExportPackageSummary(
                Goal109FileEntry(
                    projectRoot,
                    OfflineGeoworldAlphaSliceExportPackageVocabulary.ExportPackageDirectory + "/" + fileName,
                    "offline_geoworld_alpha_export_package_file"),
                summary));
            entries.Add(WithOfflineGeoworldAlphaExportPackageSummary(
                Goal109FileEntry(
                    projectRoot,
                    OfflineGeoworldAlphaSliceExportPackageVocabulary.StreamingAssetsRelativeRoot + "/" + fileName,
                    "offline_geoworld_alpha_export_streamingassets_file"),
                summary));
        }

        entries.Add(WithOfflineGeoworldAlphaExportPackageSummary(
            Goal109FileEntry(
                projectRoot,
                OfflineGeoworldAlphaSliceExportPackageVocabulary.UnityVerifierScriptPath,
                "offline_geoworld_alpha_export_unity_verifier_script"),
            summary));
        entries.Add(WithOfflineGeoworldAlphaExportPackageSummary(
            Goal109FileEntry(
                projectRoot,
                OfflineGeoworldAlphaSliceExportPackageVocabulary.UnityEditorWindowScriptPath,
                "offline_geoworld_alpha_export_editor_window_script"),
            summary));
        entries.Add(WithOfflineGeoworldAlphaExportPackageSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId + ".summary",
                RelativePath = OfflineGeoworldAlphaSliceExportPackageVocabulary.ProceduralOutputDirectory
                               + "/"
                               + OfflineGeoworldAlphaSliceExportPackageVocabulary.QualityGateScanFileName,
                ArtifactKind = "offline_geoworld_alpha_export_package_workspace_summary",
                SourceGoalId = OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    OfflineGeoworldAlphaSliceExportPackageVocabulary.ProceduralOutputDirectory
                    + "/"
                    + OfflineGeoworldAlphaSliceExportPackageVocabulary.QualityGateScanFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.Passed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "packageFiles=" + summary.PackageFileCount
                                    + "; indexedFiles=" + summary.IndexedFileCount
                                    + "; checksums=" + summary.ChecksumStatus,
                SafeRatingMetadataSummary = "manualGate=" + summary.AcceptanceGateStatus
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_alpha_export_package",
            "Goal 109 Offline Geoworld Alpha Export Package",
            OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal109ProceduralFiles() =>
    [
        (OfflineGeoworldAlphaSliceExportPackageVocabulary.ManifestFileName,
            "offline_geoworld_alpha_export_manifest"),
        (OfflineGeoworldAlphaSliceExportPackageVocabulary.FileIndexFileName,
            "offline_geoworld_alpha_export_file_index"),
        (OfflineGeoworldAlphaSliceExportPackageVocabulary.ChecksumsFileName,
            "offline_geoworld_alpha_export_checksums"),
        (OfflineGeoworldAlphaSliceExportPackageVocabulary.RunbookFileName,
            "offline_geoworld_alpha_export_runbook"),
        (OfflineGeoworldAlphaSliceExportPackageVocabulary.AcceptanceGateFileName,
            "offline_geoworld_alpha_export_acceptance_gate"),
        (OfflineGeoworldAlphaSliceExportPackageVocabulary.ReadmeFileName,
            "offline_geoworld_alpha_export_readme"),
        (OfflineGeoworldAlphaSliceExportPackageVocabulary.CleanImportProofFileName,
            "offline_geoworld_alpha_export_clean_import_proof"),
        (OfflineGeoworldAlphaSliceExportPackageVocabulary.NegativeProofFileName,
            "offline_geoworld_alpha_export_negative_proof"),
        (OfflineGeoworldAlphaSliceExportPackageVocabulary.UnityScriptInventoryFileName,
            "offline_geoworld_alpha_export_unity_script_inventory"),
        (OfflineGeoworldAlphaSliceExportPackageVocabulary.EditorWindowInventoryFileName,
            "offline_geoworld_alpha_export_editor_window_inventory"),
        (OfflineGeoworldAlphaSliceExportPackageVocabulary.WorkspaceBindingInventoryFileName,
            "offline_geoworld_alpha_export_workspace_binding_inventory"),
        (OfflineGeoworldAlphaSliceExportPackageVocabulary.SourceLineageFileName,
            "offline_geoworld_alpha_export_source_lineage"),
        (OfflineGeoworldAlphaSliceExportPackageVocabulary.QualityGateScanFileName,
            "offline_geoworld_alpha_export_quality_gate")
    ];

    private static VisualWorldPreviewArtifactEntry WithOfflineGeoworldAlphaExportPackageSummary(
        VisualWorldPreviewArtifactEntry entry,
        OfflineGeoworldAlphaExportPackageWorkspaceSummary summary) =>
        entry with
        {
            OfflineGeoworldAlphaExportPackageFileCount = summary.PackageFileCount,
            OfflineGeoworldAlphaExportIndexedFileCount = summary.IndexedFileCount,
            OfflineGeoworldAlphaExportChecksumStatus = summary.ChecksumStatus,
            OfflineGeoworldAlphaExportCleanImportProofPassed = summary.CleanImportProofPassed,
            OfflineGeoworldAlphaExportNegativeProofPassed = summary.NegativeProofPassed,
            OfflineGeoworldAlphaExportUnityVerifierReady = summary.UnityVerifierReady,
            OfflineGeoworldAlphaExportEditorWindowReady = summary.EditorWindowReady,
            OfflineGeoworldAlphaExportWorkspaceBindingPassed = summary.WorkspaceBindingPassed,
            OfflineGeoworldAlphaExportSourceLineagePassed = summary.SourceLineagePassed,
            OfflineGeoworldAlphaExportRunbookSummary = summary.RunbookSummary,
            OfflineGeoworldAlphaExportAcceptanceGateStatus = summary.AcceptanceGateStatus,
            OfflineGeoworldAlphaExportAlphaRuntimeBootstrapUnchanged =
                summary.AlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldAlphaExportQualityGatePassed = summary.QualityGatePassed,
            Goal109FilesDiscoveredByRelativePaths = summary.RelativePaths,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static OfflineGeoworldAlphaExportPackageWorkspaceSummary
        LoadOfflineGeoworldAlphaExportPackageSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAlphaSliceExportPackageVocabulary.ProceduralOutputDirectory;
        using var manifest = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceExportPackageVocabulary.ManifestFileName, diagnostics);
        using var index = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceExportPackageVocabulary.FileIndexFileName, diagnostics);
        using var checksums = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceExportPackageVocabulary.ChecksumsFileName, diagnostics);
        using var acceptance = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceExportPackageVocabulary.AcceptanceGateFileName, diagnostics);
        using var clean = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceExportPackageVocabulary.CleanImportProofFileName, diagnostics);
        using var negative = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceExportPackageVocabulary.NegativeProofFileName, diagnostics);
        using var unity = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceExportPackageVocabulary.UnityScriptInventoryFileName, diagnostics);
        using var editor = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceExportPackageVocabulary.EditorWindowInventoryFileName, diagnostics);
        using var workspace = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceExportPackageVocabulary.WorkspaceBindingInventoryFileName, diagnostics);
        using var lineage = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceExportPackageVocabulary.SourceLineageFileName, diagnostics);
        using var quality = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceExportPackageVocabulary.QualityGateScanFileName, diagnostics);

        var packageFileCount = manifest is null ? 0 : Goal109Int(manifest.RootElement, "packageFileCount");
        var indexedFileCount = index is null ? 0 : Goal109Int(index.RootElement, "indexedFileCount");
        var hashedFileCount = checksums is null ? 0 : Goal109Int(checksums.RootElement, "hashedFileCount");
        var checksumStatus = hashedFileCount == indexedFileCount && indexedFileCount == 5
            ? "matched"
            : "incomplete";
        var acceptanceStatus = acceptance is not null
                               && !TryGetBool(acceptance.RootElement, "accepted")
                               && Goal109String(acceptance.RootElement, "manualGate")
                               == OfflineGeoworldAlphaSliceExportPackageVocabulary.FinalGate
            ? "required"
            : string.Empty;
        var runbookText = ReadOptionalText(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceExportPackageVocabulary.RunbookFileName);
        var runbookSummary = runbookText.Contains("manual gate", StringComparison.OrdinalIgnoreCase)
            ? "manual gate required"
            : "runbook unavailable";
        var cleanPassed = clean is not null && TryGetBool(clean.RootElement, "passed");
        var negativePassed = negative is not null && TryGetBool(negative.RootElement, "passed");
        var unityPassed = unity is not null && TryGetBool(unity.RootElement, "passed");
        var editorPassed = editor is not null && TryGetBool(editor.RootElement, "passed");
        var workspacePassed = workspace is not null && TryGetBool(workspace.RootElement, "passed");
        var lineagePassed = lineage is not null
                            && TryGetBool(lineage.RootElement, "goal108ManifestRead")
                            && TryGetBool(lineage.RootElement, "goal108AImmutabilityAuditRead");
        var alphaUnchanged = manifest is not null
                             && TryGetBool(manifest.RootElement, "alphaRuntimeBootstrapUnchanged");
        var qualityPassed = quality is not null && TryGetBool(quality.RootElement, "passed");
        var relativePaths = Goal109AllPathsRelative(projectRoot);
        var passed = packageFileCount == 6
                     && indexedFileCount == 5
                     && checksumStatus == "matched"
                     && cleanPassed
                     && negativePassed
                     && unityPassed
                     && editorPassed
                     && workspacePassed
                     && lineagePassed
                     && alphaUnchanged
                     && qualityPassed
                     && relativePaths
                     && acceptanceStatus == "required";
        AddIfFalse(passed, "goal109.workspace.summary_failed",
            "offline_geoworld_alpha_export_package", diagnostics);
        return new OfflineGeoworldAlphaExportPackageWorkspaceSummary(
            passed,
            packageFileCount,
            indexedFileCount,
            checksumStatus,
            cleanPassed,
            negativePassed,
            unityPassed,
            editorPassed,
            workspacePassed,
            lineagePassed,
            runbookSummary,
            acceptanceStatus,
            alphaUnchanged,
            qualityPassed,
            relativePaths);
    }

    private static VisualWorldPreviewArtifactEntry Goal109FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal109 package file exists" : "Goal109 package file missing",
            SafeRatingMetadataSummary = "metadataOnly=true; exportPackage=alphaToolingOnly"
        };
    }

    private static bool Goal109AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            OfflineGeoworldAlphaSliceExportPackageVocabulary.ProceduralOutputDirectory,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.ExportPackageDirectory,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.StreamingAssetsRelativeRoot,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.UnityVerifierScriptPath,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.UnityEditorWindowScriptPath
        };
        return roots.All(IsSafeRelativePath)
               && roots.Take(3).All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(IsSafeRelativePath));
    }

    private static int Goal109Int(JsonElement element, string propertyName) =>
        TryGetInt(element, propertyName, out var value) ? value : 0;

    private static string Goal109String(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private sealed record OfflineGeoworldAlphaExportPackageWorkspaceSummary(
        bool Passed,
        int PackageFileCount,
        int IndexedFileCount,
        string ChecksumStatus,
        bool CleanImportProofPassed,
        bool NegativeProofPassed,
        bool UnityVerifierReady,
        bool EditorWindowReady,
        bool WorkspaceBindingPassed,
        bool SourceLineagePassed,
        string RunbookSummary,
        string AcceptanceGateStatus,
        bool AlphaRuntimeBootstrapUnchanged,
        bool QualityGatePassed,
        bool RelativePaths);
}
