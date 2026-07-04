using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildOfflineGeoworldAlphaManualAcceptanceGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldAlphaManualAcceptanceSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ProceduralOutputDirectory,
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId,
                BuildGoal110ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldAlphaManualAcceptanceSummary(entry, summary))
            .ToList();

        foreach (var fileName in OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredExportFileNames)
        {
            entries.Add(WithOfflineGeoworldAlphaManualAcceptanceSummary(
                Goal110FileEntry(
                    projectRoot,
                    OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ExportPackageDirectory + "/" + fileName,
                    "offline_geoworld_alpha_manual_acceptance_export_file"),
                summary));
        }

        foreach (var fileName in OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.RequiredPayloadFileNames)
        {
            entries.Add(WithOfflineGeoworldAlphaManualAcceptanceSummary(
                Goal110FileEntry(
                    projectRoot,
                    OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.StreamingAssetsRelativeRoot + "/" + fileName,
                    "offline_geoworld_alpha_manual_acceptance_streamingassets_file"),
                summary));
        }

        entries.Add(WithOfflineGeoworldAlphaManualAcceptanceSummary(
            Goal110FileEntry(
                projectRoot,
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityResultScriptPath,
                "offline_geoworld_alpha_acceptance_result_script"),
            summary));
        entries.Add(WithOfflineGeoworldAlphaManualAcceptanceSummary(
            Goal110FileEntry(
                projectRoot,
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityResultStoreScriptPath,
                "offline_geoworld_alpha_acceptance_result_store_script"),
            summary));
        entries.Add(WithOfflineGeoworldAlphaManualAcceptanceSummary(
            Goal110FileEntry(
                projectRoot,
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityEditorWindowScriptPath,
                "offline_geoworld_alpha_acceptance_runner_window_script"),
            summary));
        entries.Add(WithOfflineGeoworldAlphaManualAcceptanceSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId + ".summary",
                RelativePath = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ProceduralOutputDirectory
                               + "/"
                               + OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.QualityGateScanFileName,
                ArtifactKind = "offline_geoworld_alpha_manual_acceptance_workspace_summary",
                SourceGoalId = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ProceduralOutputDirectory
                    + "/"
                    + OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.QualityGateScanFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.Passed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "steps=" + summary.ChecklistStepCount
                                    + "; automatedGate=" + summary.AutomatedGatePassed
                                    + "; manualPending=" + summary.ManualPending,
                SafeRatingMetadataSummary = "manualGate=required; notFinalRelease=true"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_alpha_manual_acceptance",
            "Goal 110 Offline Geoworld Alpha Manual Acceptance",
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal110ProceduralFiles() =>
    [
        (OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ManifestFileName,
            "offline_geoworld_alpha_manual_acceptance_manifest"),
        (OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ChecklistFileName,
            "offline_geoworld_alpha_manual_acceptance_checklist"),
        (OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ResultTemplateFileName,
            "offline_geoworld_alpha_manual_acceptance_result_template"),
        (OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.DashboardFileName,
            "offline_geoworld_alpha_manual_acceptance_dashboard"),
        (OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.FileIndexFileName,
            "offline_geoworld_alpha_manual_acceptance_file_index"),
        (OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ChecksumsFileName,
            "offline_geoworld_alpha_manual_acceptance_checksums"),
        (OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityScriptInventoryFileName,
            "offline_geoworld_alpha_manual_acceptance_unity_script_inventory"),
        (OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.EditorWindowInventoryFileName,
            "offline_geoworld_alpha_manual_acceptance_editor_window_inventory"),
        (OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.SimulatedProofFileName,
            "offline_geoworld_alpha_manual_acceptance_simulated_proof"),
        (OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.NegativeProofFileName,
            "offline_geoworld_alpha_manual_acceptance_negative_proof"),
        (OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.WorkspaceBindingInventoryFileName,
            "offline_geoworld_alpha_manual_acceptance_workspace_binding"),
        (OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.QualityGateScanFileName,
            "offline_geoworld_alpha_manual_acceptance_quality_gate")
    ];

    private static VisualWorldPreviewArtifactEntry WithOfflineGeoworldAlphaManualAcceptanceSummary(
        VisualWorldPreviewArtifactEntry entry,
        OfflineGeoworldAlphaManualAcceptanceWorkspaceSummary summary) =>
        entry with
        {
            OfflineGeoworldAlphaManualAcceptanceChecklistStepCount = summary.ChecklistStepCount,
            OfflineGeoworldAlphaManualAcceptancePayloadFileCount = summary.PayloadFileCount,
            OfflineGeoworldAlphaManualAcceptanceExportFileCount = summary.ExportFileCount,
            OfflineGeoworldAlphaManualAcceptanceAutomatedGatePassed = summary.AutomatedGatePassed,
            OfflineGeoworldAlphaManualAcceptanceManualPending = summary.ManualPending,
            OfflineGeoworldAlphaManualAcceptanceUnityRunnerReady = summary.UnityRunnerReady,
            OfflineGeoworldAlphaManualAcceptanceSimulatedProofPassed = summary.SimulatedProofPassed,
            OfflineGeoworldAlphaManualAcceptanceNegativeProofPassed = summary.NegativeProofPassed,
            OfflineGeoworldAlphaManualAcceptanceWorkspaceBindingPassed = summary.WorkspaceBindingPassed,
            OfflineGeoworldAlphaManualAcceptanceAlphaRuntimeBootstrapUnchanged =
                summary.AlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldAlphaManualAcceptanceQualityGatePassed = summary.QualityGatePassed,
            OfflineGeoworldAlphaManualAcceptanceResultTemplatePath = summary.ResultTemplatePath,
            OfflineGeoworldAlphaManualAcceptanceReleaseRiskLinks = summary.ReleaseRiskLinks,
            OfflineGeoworldAlphaManualAcceptanceMilestoneGateLinks = summary.MilestoneGateLinks,
            Goal110FilesDiscoveredByRelativePaths = summary.RelativePaths,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static OfflineGeoworldAlphaManualAcceptanceWorkspaceSummary
        LoadOfflineGeoworldAlphaManualAcceptanceSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ProceduralOutputDirectory;
        using var manifest = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ManifestFileName, diagnostics);
        using var checklist = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ChecklistFileName, diagnostics);
        using var dashboard = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.DashboardFileName, diagnostics);
        using var simulated = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.SimulatedProofFileName, diagnostics);
        using var negative = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.NegativeProofFileName, diagnostics);
        using var workspace = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.WorkspaceBindingInventoryFileName, diagnostics);
        using var quality = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.QualityGateScanFileName, diagnostics);

        var stepCount = checklist is null ? 0 : Goal110Int(checklist.RootElement, "stepCount");
        var payloadFileCount = manifest is null ? 0 : Goal110Int(manifest.RootElement, "payloadFileCount");
        var exportFileCount = manifest is null ? 0 : Goal110Int(manifest.RootElement, "exportFileCount");
        var automatedGate = manifest is not null && TryGetBool(manifest.RootElement, "automatedGatePassed");
        var manualPending = manifest is not null
                            && TryGetBool(manifest.RootElement, "manualAcceptancePending")
                            && !TryGetBool(manifest.RootElement, "accepted");
        var unityRunnerReady = dashboard is not null && TryGetBool(dashboard.RootElement, "unityRunnerReady");
        var simulatedPassed = simulated is not null && TryGetBool(simulated.RootElement, "passed");
        var negativePassed = negative is not null && TryGetBool(negative.RootElement, "passed");
        var workspacePassed = workspace is not null && TryGetBool(workspace.RootElement, "passed");
        var alphaUnchanged = manifest is not null
                             && TryGetBool(manifest.RootElement, "alphaRuntimeBootstrapUnchanged");
        var qualityPassed = quality is not null && TryGetBool(quality.RootElement, "passed");
        var resultTemplatePath = manifest is null
            ? string.Empty
            : Goal110String(manifest.RootElement, "resultTemplateRelativePath");
        var riskLinks = dashboard is null
            ? string.Empty
            : string.Join(",", ReadStringArray(dashboard.RootElement, "releaseRiskLinks"));
        var milestoneLinks = dashboard is null
            ? string.Empty
            : string.Join(",", ReadStringArray(dashboard.RootElement, "milestoneGateLinks"));
        var relativePaths = Goal110AllPathsRelative(projectRoot);
        var passed = stepCount >= 12
                     && payloadFileCount == 5
                     && exportFileCount == 7
                     && automatedGate
                     && manualPending
                     && unityRunnerReady
                     && simulatedPassed
                     && negativePassed
                     && workspacePassed
                     && alphaUnchanged
                     && qualityPassed
                     && relativePaths;
        AddIfFalse(passed, "goal110.workspace.summary_failed",
            "offline_geoworld_alpha_manual_acceptance", diagnostics);
        return new OfflineGeoworldAlphaManualAcceptanceWorkspaceSummary(
            passed,
            stepCount,
            payloadFileCount,
            exportFileCount,
            automatedGate,
            manualPending,
            unityRunnerReady,
            simulatedPassed,
            negativePassed,
            workspacePassed,
            alphaUnchanged,
            qualityPassed,
            resultTemplatePath,
            riskLinks,
            milestoneLinks,
            relativePaths);
    }

    private static VisualWorldPreviewArtifactEntry Goal110FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal110 manual acceptance file exists" : "Goal110 file missing",
            SafeRatingMetadataSummary = "metadataOnly=true; manualAcceptancePending=true"
        };
    }

    private static bool Goal110AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ProceduralOutputDirectory,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ExportPackageDirectory,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.StreamingAssetsRelativeRoot,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityResultScriptPath,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityResultStoreScriptPath,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityEditorWindowScriptPath
        };
        return roots.All(IsSafeRelativePath)
               && roots.Take(3).All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(IsSafeRelativePath));
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return value.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
    }

    private static int Goal110Int(JsonElement element, string propertyName) =>
        TryGetInt(element, propertyName, out var value) ? value : 0;

    private static string Goal110String(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private sealed record OfflineGeoworldAlphaManualAcceptanceWorkspaceSummary(
        bool Passed,
        int ChecklistStepCount,
        int PayloadFileCount,
        int ExportFileCount,
        bool AutomatedGatePassed,
        bool ManualPending,
        bool UnityRunnerReady,
        bool SimulatedProofPassed,
        bool NegativeProofPassed,
        bool WorkspaceBindingPassed,
        bool AlphaRuntimeBootstrapUnchanged,
        bool QualityGatePassed,
        string ResultTemplatePath,
        string ReleaseRiskLinks,
        string MilestoneGateLinks,
        bool RelativePaths);
}
