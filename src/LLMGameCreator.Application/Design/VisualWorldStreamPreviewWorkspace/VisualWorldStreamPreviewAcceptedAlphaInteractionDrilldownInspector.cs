using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildAcceptedAlphaInteractionDrilldownGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadAcceptedAlphaInteractionDrilldownSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                AcceptedAlphaInteractionDrilldownVerificationVocabulary.ProceduralOutputDirectory,
                AcceptedAlphaInteractionDrilldownVerificationVocabulary.GoalId,
                BuildGoal121ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithAcceptedAlphaInteractionDrilldownSummary(entry, summary))
            .ToList();

        foreach (var fileName in AcceptedAlphaInteractionDrilldownVerificationVocabulary.RequiredExportFileNames)
        {
            entries.Add(WithAcceptedAlphaInteractionDrilldownSummary(
                Goal121FileEntry(
                    projectRoot,
                    AcceptedAlphaInteractionDrilldownVerificationVocabulary.ExportPackageDirectory
                    + "/"
                    + fileName,
                    "accepted_alpha_interaction_drilldown_export_file"),
                summary));
        }

        entries.Add(WithAcceptedAlphaInteractionDrilldownSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = AcceptedAlphaInteractionDrilldownVerificationVocabulary.GoalId + ".summary",
                RelativePath =
                    AcceptedAlphaInteractionDrilldownVerificationVocabulary.ProceduralOutputDirectory
                    + "/"
                    + AcceptedAlphaInteractionDrilldownVerificationVocabulary.DashboardFileName,
                ArtifactKind = "accepted_alpha_interaction_drilldown_workspace_summary",
                SourceGoalId = AcceptedAlphaInteractionDrilldownVerificationVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    AcceptedAlphaInteractionDrilldownVerificationVocabulary.ProceduralOutputDirectory
                    + "/"
                    + AcceptedAlphaInteractionDrilldownVerificationVocabulary.DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "fullVerificationStatus=" + summary.FullVerificationStatus
                                    + "; unityBatchmodeLogStatus=" + summary.UnityBatchmodeLogStatus,
                SafeRatingMetadataSummary =
                    "manualUnityEditorVerificationOnly=true; noRuntimeProviderSchemaLua=true"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "accepted_alpha_interaction_drilldown_verification",
            "Goal 121 Accepted Alpha Interaction Drilldown",
            AcceptedAlphaInteractionDrilldownVerificationVocabulary.GoalId,
            AcceptedAlphaInteractionDrilldownVerificationVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal121ProceduralFiles() =>
    [
        (AcceptedAlphaInteractionDrilldownVerificationVocabulary.DashboardFileName,
            "accepted_alpha_interaction_drilldown_dashboard"),
        (AcceptedAlphaInteractionDrilldownVerificationVocabulary.ScriptInventoryFileName,
            "accepted_alpha_interaction_drilldown_script_inventory"),
        (AcceptedAlphaInteractionDrilldownVerificationVocabulary.SmokePlanFileName,
            "accepted_alpha_interaction_drilldown_smoke_plan"),
        (AcceptedAlphaInteractionDrilldownVerificationVocabulary.LogScanFileName,
            "accepted_alpha_interaction_drilldown_log_scan"),
        (AcceptedAlphaInteractionDrilldownVerificationVocabulary.ReportFileName,
            "accepted_alpha_interaction_drilldown_report"),
        (AcceptedAlphaInteractionDrilldownVerificationVocabulary.NegativeProofFileName,
            "accepted_alpha_interaction_drilldown_negative_proof"),
        (AcceptedAlphaInteractionDrilldownVerificationVocabulary.FileIndexFileName,
            "accepted_alpha_interaction_drilldown_file_index")
    ];

    private static VisualWorldPreviewArtifactEntry WithAcceptedAlphaInteractionDrilldownSummary(
        VisualWorldPreviewArtifactEntry entry,
        AcceptedAlphaInteractionDrilldownWorkspaceSummary summary) =>
        entry with
        {
            AcceptedAlphaInteractionDrilldownFullVerificationStatus = summary.FullVerificationStatus,
            AcceptedAlphaInteractionDrilldownUnityMenuPath = summary.UnityMenuPath,
            AcceptedAlphaInteractionDrilldownOneClickButtonPresent = summary.OneClickButtonPresent,
            AcceptedAlphaInteractionDrilldownDrilldownFieldsPresent =
                summary.DrilldownFieldsPresent,
            AcceptedAlphaInteractionDrilldownInteractionPreviewPresent =
                summary.InteractionPreviewPresent,
            AcceptedAlphaInteractionDrilldownObjectiveReplayDetailsPresent =
                summary.ObjectiveReplayDetailsPresent,
            AcceptedAlphaInteractionDrilldownBatchmodeFullVerificationMarker =
                summary.BatchmodeFullVerificationMarker,
            AcceptedAlphaInteractionDrilldownCleanupScriptAvailable =
                summary.CleanupScriptAvailable,
            AcceptedAlphaInteractionDrilldownMaterialWarningGuardPresent =
                summary.MaterialWarningGuardPresent,
            AcceptedAlphaInteractionDrilldownHumanManualStepsReducedToOneButton =
                summary.HumanManualStepsReducedToOneButton,
            AcceptedAlphaInteractionDrilldownUnityBatchmodeLogStatus =
                summary.UnityBatchmodeLogStatus,
            AcceptedAlphaInteractionDrilldownEvidencePath = summary.EvidencePath,
            AcceptedAlphaInteractionDrilldownExportPath = summary.ExportPath,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static AcceptedAlphaInteractionDrilldownWorkspaceSummary
        LoadAcceptedAlphaInteractionDrilldownSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = AcceptedAlphaInteractionDrilldownVerificationVocabulary.ProceduralOutputDirectory;
        using var dashboard = TryReadJson(
            projectRoot,
            root + "/" + AcceptedAlphaInteractionDrilldownVerificationVocabulary.DashboardFileName,
            diagnostics);
        return new AcceptedAlphaInteractionDrilldownWorkspaceSummary(
            FullVerificationStatus: Goal121String(dashboard?.RootElement, "fullVerificationStatus"),
            UnityMenuPath: Goal121String(dashboard?.RootElement, "unityMenuPath"),
            OneClickButtonPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "oneClickButtonPresent"),
            DrilldownFieldsPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "drilldownFieldsPresent"),
            InteractionPreviewPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "interactionPreviewPresent"),
            ObjectiveReplayDetailsPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "objectiveReplayDetailsPresent"),
            BatchmodeFullVerificationMarker:
                Goal121String(dashboard?.RootElement, "batchmodeFullVerificationMarker"),
            CleanupScriptAvailable:
                dashboard is not null && TryGetBool(dashboard.RootElement, "cleanupScriptAvailable"),
            MaterialWarningGuardPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "materialWarningGuardPresent"),
            HumanManualStepsReducedToOneButton:
                dashboard is not null && TryGetBool(dashboard.RootElement, "humanManualStepsReducedToOneButton"),
            UnityBatchmodeLogStatus:
                Goal121String(dashboard?.RootElement, "unityBatchmodeLogStatus"),
            EvidencePath: Goal121String(dashboard?.RootElement, "evidencePath"),
            ExportPath: Goal121String(dashboard?.RootElement, "exportPath"),
            QualityGatePassed:
                Goal121String(dashboard?.RootElement, "fullVerificationStatus")
                == AcceptedAlphaInteractionDrilldownVerificationVocabulary.FullVerificationStatus,
            RelativePaths: Goal121AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal121FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = AcceptedAlphaInteractionDrilldownVerificationVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = AcceptedAlphaInteractionDrilldownVerificationVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal121 drilldown file exists" : "Goal121 file missing",
            SafeRatingMetadataSummary = "metadataOnly=true; noManualInput=true"
        };
    }

    private static bool Goal121AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            AcceptedAlphaInteractionDrilldownVerificationVocabulary.ProceduralOutputDirectory,
            AcceptedAlphaInteractionDrilldownVerificationVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal121String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private sealed record AcceptedAlphaInteractionDrilldownWorkspaceSummary(
        string FullVerificationStatus,
        string UnityMenuPath,
        bool OneClickButtonPresent,
        bool DrilldownFieldsPresent,
        bool InteractionPreviewPresent,
        bool ObjectiveReplayDetailsPresent,
        string BatchmodeFullVerificationMarker,
        bool CleanupScriptAvailable,
        bool MaterialWarningGuardPresent,
        bool HumanManualStepsReducedToOneButton,
        string UnityBatchmodeLogStatus,
        string EvidencePath,
        string ExportPath,
        bool QualityGatePassed,
        bool RelativePaths);
}
