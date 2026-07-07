using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildRuntimeBackedUnityPlayerLoopControlsUxGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadRuntimeBackedUnityPlayerLoopControlsUxSummary(projectRoot);
        var entries = BuildCoreEntries(
                projectRoot,
                RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ProceduralOutputDirectory,
                RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId,
                BuildGoal140ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithRuntimeBackedUnityPlayerLoopControlsUxSummary(entry, summary))
            .ToList();

        foreach (var file in BuildGoal140ExportFiles())
        {
            entries.Add(WithRuntimeBackedUnityPlayerLoopControlsUxSummary(
                Goal140FileEntry(projectRoot, file.RelativePath, file.Kind),
                summary));
        }

        entries.Add(WithRuntimeBackedUnityPlayerLoopControlsUxSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId + ".summary",
                RelativePath = RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.DashboardRelativePath,
                ArtifactKind = "runtime_backed_unity_player_loop_controls_ux_workspace_summary",
                SourceGoalId = RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.DashboardRelativePath,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "selectedCandidate=" + summary.SelectedCandidate
                                    + "; frameCount=" + summary.FrameCount
                                    + "; humanReadableFrameNumbering="
                                    + summary.HumanReadableFrameNumbering.ToString().ToLowerInvariant()
                                    + "; unityControlsUxSmokePassed="
                                    + summary.UnityControlsUxSmokePassed.ToString().ToLowerInvariant(),
                SafeRatingMetadataSummary =
                    "runtimeAuthority=true; projectionOnly=false; unityGameplayTruth=false"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "runtime_backed_unity_player_loop_controls_ux",
            "Goal 140 Runtime-backed Unity Player Loop Controls UX",
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId,
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal140ProceduralFiles() =>
    [
        (RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.Goal139AcceptanceFileName,
            "runtime_backed_unity_player_loop_controls_ux_goal139_acceptance"),
        (RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.DashboardFileName,
            "runtime_backed_unity_player_loop_controls_ux_dashboard"),
        (RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ResultFileName,
            "runtime_backed_unity_player_loop_controls_ux_result"),
        (RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ModelFileName,
            "runtime_backed_unity_player_loop_controls_ux_model"),
        (RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ScriptFileName,
            "runtime_backed_unity_player_loop_controls_ux_script"),
        (RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.UnitySmokeFileName,
            "unity_player_loop_controls_ux_smoke"),
        (RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.UnityNoiseClassificationFileName,
            "unity_editor_noise_classification"),
        (RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ReportJsonFileName,
            "runtime_backed_unity_player_loop_controls_ux_one_click_report_json"),
        (RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ReportMarkdownFileName,
            "runtime_backed_unity_player_loop_controls_ux_one_click_report_markdown"),
        (RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.NegativeProofFileName,
            "runtime_backed_unity_player_loop_controls_ux_negative_proof"),
        (RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.FileIndexFileName,
            "runtime_backed_unity_player_loop_controls_ux_file_index")
    ];

    private static IReadOnlyList<(string RelativePath, string Kind)> BuildGoal140ExportFiles() =>
        BuildGoal140ProceduralFiles()
            .Select(item => (
                RelativePath:
                RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ExportPackageDirectory
                + "/"
                + item.FileName,
                Kind: "runtime_backed_unity_player_loop_controls_ux_export_file"))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();

    private static VisualWorldPreviewArtifactEntry WithRuntimeBackedUnityPlayerLoopControlsUxSummary(
        VisualWorldPreviewArtifactEntry entry,
        RuntimeBackedUnityPlayerLoopControlsUxWorkspaceSummary summary) =>
        entry with
        {
            RuntimeBackedUnityPlayerLoopControlsUxAcceptedGoal139 = summary.AcceptedGoal139,
            RuntimeBackedUnityPlayerLoopControlsUxSelectedCandidate = summary.SelectedCandidate,
            RuntimeBackedUnityPlayerLoopControlsUxFrameCount = summary.FrameCount,
            RuntimeBackedUnityPlayerLoopControlsUxHumanReadableFrameNumbering =
                summary.HumanReadableFrameNumbering,
            RuntimeBackedUnityPlayerLoopControlsUxStepOnceSemanticsClear =
                summary.StepOnceSemanticsClear,
            RuntimeBackedUnityPlayerLoopControlsUxPlayAllToEndSemanticsClear =
                summary.PlayAllToEndSemanticsClear,
            RuntimeBackedUnityPlayerLoopControlsUxKnownUnityEditorNoiseClassified =
                summary.KnownUnityEditorNoiseClassified,
            RuntimeBackedUnityPlayerLoopControlsUxBlockingUnityErrorCount =
                summary.BlockingUnityErrorCount,
            RuntimeBackedUnityPlayerLoopControlsUxUnclassifiedUnityErrorCount =
                summary.UnclassifiedUnityErrorCount,
            RuntimeBackedUnityPlayerLoopControlsUxUnitySmokePassed =
                summary.UnityControlsUxSmokePassed,
            RuntimeBackedUnityPlayerLoopControlsUxRuntimeAuthority = summary.RuntimeAuthority,
            RuntimeBackedUnityPlayerLoopControlsUxUnityGameplayTruth = summary.UnityGameplayTruth,
            RuntimeBackedUnityPlayerLoopControlsUxProjectionOnly = summary.ProjectionOnly,
            RuntimeBackedUnityPlayerLoopControlsUxNormalCommand = summary.NormalCommand,
            RuntimeBackedUnityPlayerLoopControlsUxReportPath = summary.ReportPath,
            RuntimeBackedUnityPlayerLoopControlsUxAccepted = summary.Accepted,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static RuntimeBackedUnityPlayerLoopControlsUxWorkspaceSummary
        LoadRuntimeBackedUnityPlayerLoopControlsUxSummary(string projectRoot)
    {
        using var dashboard = TryReadJson(
            projectRoot,
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.DashboardRelativePath,
            []);
        return new RuntimeBackedUnityPlayerLoopControlsUxWorkspaceSummary(
            AcceptedGoal139: dashboard is not null && TryGetBool(dashboard.RootElement, "acceptedGoal139"),
            SelectedCandidate: Goal138String(dashboard?.RootElement, "selectedCandidate"),
            FrameCount: dashboard is not null ? Goal138Int(dashboard.RootElement, "frameCount") : 0,
            HumanReadableFrameNumbering:
                dashboard is not null && TryGetBool(dashboard.RootElement, "humanReadableFrameNumbering"),
            StepOnceSemanticsClear:
                dashboard is not null && TryGetBool(dashboard.RootElement, "stepOnceSemanticsClear"),
            PlayAllToEndSemanticsClear:
                dashboard is not null && TryGetBool(dashboard.RootElement, "playAllToEndSemanticsClear"),
            KnownUnityEditorNoiseClassified:
                dashboard is not null && TryGetBool(dashboard.RootElement, "knownUnityEditorNoiseClassified"),
            BlockingUnityErrorCount:
                dashboard is not null ? Goal138Int(dashboard.RootElement, "blockingUnityErrorCount") : 0,
            UnclassifiedUnityErrorCount:
                dashboard is not null ? Goal138Int(dashboard.RootElement, "unclassifiedUnityErrorCount") : 0,
            UnityControlsUxSmokePassed:
                dashboard is not null && TryGetBool(dashboard.RootElement, "unityControlsUxSmokePassed"),
            RuntimeAuthority:
                dashboard is not null && TryGetBool(dashboard.RootElement, "runtimeAuthority"),
            UnityGameplayTruth:
                dashboard is not null && TryGetBool(dashboard.RootElement, "unityGameplayTruth"),
            ProjectionOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionOnly"),
            NormalCommand: Goal138String(dashboard?.RootElement, "normalCommand"),
            ReportPath: Goal138String(dashboard?.RootElement, "reportPath"),
            Accepted: dashboard is not null && TryGetBool(dashboard.RootElement, "accepted"),
            QualityGatePassed: Goal138String(dashboard?.RootElement, "status") == "GREEN",
            RelativePaths: Goal140AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal140FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists
                ? "Goal140 runtime-backed Unity player loop controls UX file exists"
                : "Goal140 runtime-backed Unity player loop controls UX file missing",
            SafeRatingMetadataSummary =
                "runtimeBackedUnityPlayerLoopControlsUxArtifact=true; noManualInput=true"
        };
    }

    private static bool Goal140AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ProceduralOutputDirectory,
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.AllDirectories)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private sealed record RuntimeBackedUnityPlayerLoopControlsUxWorkspaceSummary(
        bool AcceptedGoal139,
        string SelectedCandidate,
        int FrameCount,
        bool HumanReadableFrameNumbering,
        bool StepOnceSemanticsClear,
        bool PlayAllToEndSemanticsClear,
        bool KnownUnityEditorNoiseClassified,
        int BlockingUnityErrorCount,
        int UnclassifiedUnityErrorCount,
        bool UnityControlsUxSmokePassed,
        bool RuntimeAuthority,
        bool UnityGameplayTruth,
        bool ProjectionOnly,
        string NormalCommand,
        string ReportPath,
        bool Accepted,
        bool QualityGatePassed,
        bool RelativePaths);
}
