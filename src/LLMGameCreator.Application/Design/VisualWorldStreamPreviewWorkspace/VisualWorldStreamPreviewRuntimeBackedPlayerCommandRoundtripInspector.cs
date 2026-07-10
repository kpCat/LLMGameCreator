using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildRuntimeBackedPlayerCommandRoundtripGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadRuntimeBackedPlayerCommandRoundtripSummary(projectRoot);
        var entries = BuildCoreEntries(
                projectRoot,
                RuntimeBackedPlayerCommandRoundtripVocabulary.ProceduralOutputDirectory,
                RuntimeBackedPlayerCommandRoundtripVocabulary.GoalId,
                BuildGoal141ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithRuntimeBackedPlayerCommandRoundtripSummary(entry, summary))
            .ToList();

        foreach (var file in BuildGoal141ExportFiles())
        {
            entries.Add(WithRuntimeBackedPlayerCommandRoundtripSummary(
                Goal141FileEntry(projectRoot, file.RelativePath, file.Kind),
                summary));
        }

        entries.Add(WithRuntimeBackedPlayerCommandRoundtripSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = RuntimeBackedPlayerCommandRoundtripVocabulary.GoalId + ".summary",
                RelativePath = RuntimeBackedPlayerCommandRoundtripVocabulary.DashboardRelativePath,
                ArtifactKind = "runtime_backed_player_command_roundtrip_workspace_summary",
                SourceGoalId = RuntimeBackedPlayerCommandRoundtripVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    RuntimeBackedPlayerCommandRoundtripVocabulary.DashboardRelativePath,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "candidateId=" + summary.CandidateId
                                    + "; roundtripRequestCount=" + summary.RequestCount
                                    + "; runtimeExecutedRequestCount=" + summary.ExecutedRequestCount
                                    + "; roundtripSnapshotCount=" + summary.SnapshotCount,
                SafeRatingMetadataSummary =
                    "runtimeAuthority=true; projectionOnly=false; unityGameplayTruth=false"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "runtime_backed_player_command_roundtrip",
            "Goal 141 Runtime-backed Player Command Roundtrip",
            RuntimeBackedPlayerCommandRoundtripVocabulary.GoalId,
            RuntimeBackedPlayerCommandRoundtripVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal141ProceduralFiles() =>
    [
        (RuntimeBackedPlayerCommandRoundtripVocabulary.Goal140AcceptanceFileName,
            "runtime_backed_player_command_roundtrip_goal140_acceptance"),
        (RuntimeBackedPlayerCommandRoundtripVocabulary.RequestFileName,
            "runtime_backed_player_command_roundtrip_request"),
        (RuntimeBackedPlayerCommandRoundtripVocabulary.ResultFileName,
            "runtime_backed_player_command_roundtrip_result"),
        (RuntimeBackedPlayerCommandRoundtripVocabulary.SessionFileName,
            "runtime_backed_player_command_roundtrip_session"),
        (RuntimeBackedPlayerCommandRoundtripVocabulary.SnapshotsFileName,
            "runtime_backed_player_command_roundtrip_snapshots"),
        (RuntimeBackedPlayerCommandRoundtripVocabulary.ModelFileName,
            "runtime_backed_player_command_roundtrip_model"),
        (RuntimeBackedPlayerCommandRoundtripVocabulary.DashboardFileName,
            "runtime_backed_player_command_roundtrip_dashboard"),
        (RuntimeBackedPlayerCommandRoundtripVocabulary.NegativeProofFileName,
            "runtime_backed_player_command_roundtrip_negative_proof"),
        (RuntimeBackedPlayerCommandRoundtripVocabulary.FileIndexFileName,
            "runtime_backed_player_command_roundtrip_file_index"),
        (RuntimeBackedPlayerCommandRoundtripVocabulary.UnitySmokeFileName,
            "unity_player_command_roundtrip_smoke"),
        (RuntimeBackedPlayerCommandRoundtripVocabulary.ReportJsonFileName,
            "runtime_backed_player_command_roundtrip_one_click_report_json"),
        (RuntimeBackedPlayerCommandRoundtripVocabulary.ReportMarkdownFileName,
            "runtime_backed_player_command_roundtrip_one_click_report_markdown")
    ];

    private static IReadOnlyList<(string RelativePath, string Kind)> BuildGoal141ExportFiles() =>
        BuildGoal141ProceduralFiles()
            .Select(item => (
                RelativePath:
                RuntimeBackedPlayerCommandRoundtripVocabulary.ExportPackageDirectory
                + "/"
                + item.FileName,
                Kind: "runtime_backed_player_command_roundtrip_export_file"))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();

    private static VisualWorldPreviewArtifactEntry WithRuntimeBackedPlayerCommandRoundtripSummary(
        VisualWorldPreviewArtifactEntry entry,
        RuntimeBackedPlayerCommandRoundtripWorkspaceSummary summary) =>
        entry with
        {
            RuntimeBackedPlayerCommandRoundtripGoal140Accepted = summary.Goal140Accepted,
            RuntimeBackedPlayerCommandRoundtripCandidateId = summary.CandidateId,
            RuntimeBackedPlayerCommandRoundtripTotalControlRequestCount =
                summary.TotalControlRequestCount,
            RuntimeBackedPlayerCommandRoundtripRequestCount = summary.RequestCount,
            RuntimeBackedPlayerCommandRoundtripRuntimeRoutedRequestCount =
                summary.RuntimeRoutedRequestCount,
            RuntimeBackedPlayerCommandRoundtripPresentationOnlyRequestCount =
                summary.PresentationOnlyRequestCount,
            RuntimeBackedPlayerCommandRoundtripExecutedRequestCount = summary.ExecutedRequestCount,
            RuntimeBackedPlayerCommandRoundtripPresentationOnlyRuntimeExecutionCount =
                summary.PresentationOnlyRuntimeExecutionCount,
            RuntimeBackedPlayerCommandRoundtripRuntimeMutatingPresentationRequestCount =
                summary.RuntimeMutatingPresentationRequestCount,
            RuntimeBackedPlayerCommandRoundtripResponseCount = summary.ResponseCount,
            RuntimeBackedPlayerCommandRoundtripSnapshotCount = summary.SnapshotCount,
            RuntimeBackedPlayerCommandRoundtripControlRequestBridgePresent =
                summary.ControlRequestBridgePresent,
            RuntimeBackedPlayerCommandRoundtripStateHashChainPresent = summary.StateHashChainPresent,
            RuntimeBackedPlayerCommandRoundtripRequestResponseCorrelationPassed =
                summary.RequestResponseCorrelationPassed,
            RuntimeBackedPlayerCommandRoundtripSequentialCursorContinuityPassed =
                summary.SequentialCursorContinuityPassed,
            RuntimeBackedPlayerCommandRoundtripStateHashContinuityPassed =
                summary.StateHashContinuityPassed,
            RuntimeBackedPlayerCommandRoundtripCopySummaryStateUnchanged =
                summary.CopySummaryStateUnchanged,
            RuntimeBackedPlayerCommandRoundtripLoadModelStateUnchanged =
                summary.LoadModelStateUnchanged,
            RuntimeBackedPlayerCommandRoundtripNoUnrelatedGameplayMapping =
                summary.NoUnrelatedGameplayMapping,
            RuntimeBackedPlayerCommandRoundtripSemanticCorrectnessPassed =
                summary.SemanticCorrectnessPassed,
            RuntimeBackedPlayerCommandRoundtripRuntimeAuthority = summary.RuntimeAuthority,
            RuntimeBackedPlayerCommandRoundtripProjectionOnly = summary.ProjectionOnly,
            RuntimeBackedPlayerCommandRoundtripUnityGameplayTruth = summary.UnityGameplayTruth,
            RuntimeBackedPlayerCommandRoundtripUnityConsumesRoundtripResult =
                summary.UnityConsumesRoundtripResult,
            RuntimeBackedPlayerCommandRoundtripNormalCommand = summary.NormalCommand,
            RuntimeBackedPlayerCommandRoundtripReportPath = summary.ReportPath,
            RuntimeBackedPlayerCommandRoundtripManualUnityOptional = summary.ManualUnityOptional,
            RuntimeBackedPlayerCommandRoundtripAccepted = summary.Accepted,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static RuntimeBackedPlayerCommandRoundtripWorkspaceSummary
        LoadRuntimeBackedPlayerCommandRoundtripSummary(string projectRoot)
    {
        using var dashboard = TryReadJson(
            projectRoot,
            RuntimeBackedPlayerCommandRoundtripVocabulary.DashboardRelativePath,
            []);
        return new RuntimeBackedPlayerCommandRoundtripWorkspaceSummary(
            Goal140Accepted: dashboard is not null && TryGetBool(dashboard.RootElement, "goal140Accepted"),
            CandidateId: Goal138String(dashboard?.RootElement, "candidateId"),
            TotalControlRequestCount: dashboard is not null
                ? Goal138Int(dashboard.RootElement, "totalControlRequestCount")
                : 0,
            RequestCount: dashboard is not null ? Goal138Int(dashboard.RootElement, "roundtripRequestCount") : 0,
            RuntimeRoutedRequestCount: dashboard is not null
                ? Goal138Int(dashboard.RootElement, "runtimeRoutedRequestCount")
                : 0,
            PresentationOnlyRequestCount: dashboard is not null
                ? Goal138Int(dashboard.RootElement, "presentationOnlyRequestCount")
                : 0,
            ExecutedRequestCount: dashboard is not null
                ? Goal138Int(dashboard.RootElement, "runtimeExecutedRequestCount")
                : 0,
            PresentationOnlyRuntimeExecutionCount: dashboard is not null
                ? Goal138Int(dashboard.RootElement, "presentationOnlyRuntimeExecutionCount")
                : 0,
            RuntimeMutatingPresentationRequestCount: dashboard is not null
                ? Goal138Int(dashboard.RootElement, "runtimeMutatingPresentationRequestCount")
                : 0,
            ResponseCount: dashboard is not null
                ? Goal138Int(dashboard.RootElement, "responseCount")
                : 0,
            SnapshotCount: dashboard is not null ? Goal138Int(dashboard.RootElement, "roundtripSnapshotCount") : 0,
            ControlRequestBridgePresent: dashboard is not null
                                         && TryGetBool(dashboard.RootElement, "controlRequestBridgePresent"),
            StateHashChainPresent: dashboard is not null
                                   && TryGetBool(dashboard.RootElement, "stateHashChainPresent"),
            RequestResponseCorrelationPassed: dashboard is not null
                                              && TryGetBool(
                                                  dashboard.RootElement,
                                                  "requestResponseCorrelationPassed"),
            SequentialCursorContinuityPassed: dashboard is not null
                                              && TryGetBool(
                                                  dashboard.RootElement,
                                                  "sequentialCursorContinuityPassed"),
            StateHashContinuityPassed: dashboard is not null
                                       && TryGetBool(dashboard.RootElement, "stateHashContinuityPassed"),
            CopySummaryStateUnchanged: dashboard is not null
                                       && TryGetBool(dashboard.RootElement, "copySummaryStateUnchanged"),
            LoadModelStateUnchanged: dashboard is not null
                                     && TryGetBool(dashboard.RootElement, "loadModelStateUnchanged"),
            NoUnrelatedGameplayMapping: dashboard is not null
                                        && TryGetBool(
                                            dashboard.RootElement,
                                            "noControlIntentMappedToUnrelatedGameplayCommand"),
            SemanticCorrectnessPassed: dashboard is not null
                                       && TryGetBool(
                                           dashboard.RootElement,
                                           "roundtripSemanticCorrectnessPassed"),
            RuntimeAuthority: dashboard is not null && TryGetBool(dashboard.RootElement, "runtimeAuthority"),
            ProjectionOnly: dashboard is not null && TryGetBool(dashboard.RootElement, "projectionOnly"),
            UnityGameplayTruth: dashboard is not null && TryGetBool(dashboard.RootElement, "unityGameplayTruth"),
            UnityConsumesRoundtripResult: dashboard is not null
                                          && TryGetBool(dashboard.RootElement, "unityConsumesRoundtripResult"),
            NormalCommand: Goal138String(dashboard?.RootElement, "normalCommand"),
            ReportPath: Goal138String(dashboard?.RootElement, "reportPath"),
            ManualUnityOptional: dashboard is not null && TryGetBool(dashboard.RootElement, "manualUnityOptional"),
            Accepted: dashboard is not null && TryGetBool(dashboard.RootElement, "accepted"),
            QualityGatePassed: Goal138String(dashboard?.RootElement, "status") == "GREEN",
            RelativePaths: Goal141AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal141FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = RuntimeBackedPlayerCommandRoundtripVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = RuntimeBackedPlayerCommandRoundtripVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists
                ? "Goal141 runtime-backed player command roundtrip file exists"
                : "Goal141 runtime-backed player command roundtrip file missing",
            SafeRatingMetadataSummary =
                "runtimeBackedPlayerCommandRoundtripArtifact=true; noManualInput=true"
        };
    }

    private static bool Goal141AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            RuntimeBackedPlayerCommandRoundtripVocabulary.ProceduralOutputDirectory,
            RuntimeBackedPlayerCommandRoundtripVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.AllDirectories)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private sealed record RuntimeBackedPlayerCommandRoundtripWorkspaceSummary(
        bool Goal140Accepted,
        string CandidateId,
        int TotalControlRequestCount,
        int RequestCount,
        int RuntimeRoutedRequestCount,
        int PresentationOnlyRequestCount,
        int ExecutedRequestCount,
        int PresentationOnlyRuntimeExecutionCount,
        int RuntimeMutatingPresentationRequestCount,
        int ResponseCount,
        int SnapshotCount,
        bool ControlRequestBridgePresent,
        bool StateHashChainPresent,
        bool RequestResponseCorrelationPassed,
        bool SequentialCursorContinuityPassed,
        bool StateHashContinuityPassed,
        bool CopySummaryStateUnchanged,
        bool LoadModelStateUnchanged,
        bool NoUnrelatedGameplayMapping,
        bool SemanticCorrectnessPassed,
        bool RuntimeAuthority,
        bool ProjectionOnly,
        bool UnityGameplayTruth,
        bool UnityConsumesRoundtripResult,
        string NormalCommand,
        string ReportPath,
        bool ManualUnityOptional,
        bool Accepted,
        bool QualityGatePassed,
        bool RelativePaths);
}
