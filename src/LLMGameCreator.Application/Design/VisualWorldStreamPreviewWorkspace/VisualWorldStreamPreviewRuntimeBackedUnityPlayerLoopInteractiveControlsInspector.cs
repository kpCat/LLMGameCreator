using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildRuntimeBackedUnityPlayerLoopInteractiveControlsGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadRuntimeBackedUnityPlayerLoopInteractiveControlsSummary(projectRoot);
        var entries = BuildCoreEntries(
                projectRoot,
                RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ProceduralOutputDirectory,
                RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.GoalId,
                BuildGoal139ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithRuntimeBackedUnityPlayerLoopInteractiveControlsSummary(entry, summary))
            .ToList();

        foreach (var file in BuildGoal139ExportFiles())
        {
            entries.Add(WithRuntimeBackedUnityPlayerLoopInteractiveControlsSummary(
                Goal139FileEntry(projectRoot, file.RelativePath, file.Kind),
                summary));
        }

        entries.Add(WithRuntimeBackedUnityPlayerLoopInteractiveControlsSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.GoalId + ".summary",
                RelativePath = RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DashboardRelativePath,
                ArtifactKind =
                    "runtime_backed_unity_player_loop_interactive_controls_workspace_summary",
                SourceGoalId = RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DashboardRelativePath,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "candidateId=" + summary.CandidateId
                                    + "; frameCount=" + summary.FrameCount
                                    + "; controlScriptPassed="
                                    + summary.ControlScriptPassed.ToString().ToLowerInvariant()
                                    + "; unityInteractiveControlsSmokePassed="
                                    + summary.UnityInteractiveControlsSmokePassed.ToString().ToLowerInvariant(),
                SafeRatingMetadataSummary =
                    "runtimeAuthority=true; projectionOnly=false; unityGameplayTruth=false"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "runtime_backed_unity_player_loop_interactive_controls",
            "Goal 139 Runtime-backed Unity Player Loop Interactive Controls",
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.GoalId,
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal139ProceduralFiles() =>
    [
        (RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.Goal138AcceptanceFileName,
            "runtime_backed_unity_player_loop_interactive_controls_goal138_acceptance"),
        (RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ModelFileName,
            "runtime_backed_unity_player_loop_interactive_controls_model"),
        (RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ControlScriptFileName,
            "runtime_backed_unity_player_loop_interactive_controls_script"),
        (RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.SessionFileName,
            "runtime_backed_unity_player_loop_interactive_controls_session"),
        (RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ResultFileName,
            "runtime_backed_unity_player_loop_interactive_controls_result"),
        (RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DashboardFileName,
            "runtime_backed_unity_player_loop_interactive_controls_dashboard"),
        (RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.NegativeProofFileName,
            "runtime_backed_unity_player_loop_interactive_controls_negative_proof"),
        (RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.FileIndexFileName,
            "runtime_backed_unity_player_loop_interactive_controls_file_index"),
        (RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.UnitySmokeFileName,
            "unity_player_loop_interactive_controls_smoke"),
        (RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ReportJsonFileName,
            "runtime_backed_unity_player_loop_interactive_controls_one_click_report_json"),
        (RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ReportMarkdownFileName,
            "runtime_backed_unity_player_loop_interactive_controls_one_click_report_markdown")
    ];

    private static IReadOnlyList<(string RelativePath, string Kind)> BuildGoal139ExportFiles() =>
        BuildGoal139ProceduralFiles()
            .Select(item => (
                RelativePath:
                RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ExportPackageDirectory
                + "/"
                + item.FileName,
                Kind: "runtime_backed_unity_player_loop_interactive_controls_export_file"))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();

    private static VisualWorldPreviewArtifactEntry
        WithRuntimeBackedUnityPlayerLoopInteractiveControlsSummary(
            VisualWorldPreviewArtifactEntry entry,
            RuntimeBackedUnityPlayerLoopInteractiveControlsWorkspaceSummary summary) =>
        entry with
        {
            RuntimeBackedUnityPlayerLoopInteractiveControlsAcceptedGoal138 =
                summary.AcceptedGoal138,
            RuntimeBackedUnityPlayerLoopInteractiveControlsCandidateId = summary.CandidateId,
            RuntimeBackedUnityPlayerLoopInteractiveControlsFrameCount = summary.FrameCount,
            RuntimeBackedUnityPlayerLoopInteractiveControlsRequiredControlsPresent =
                summary.RequiredControlsPresent,
            RuntimeBackedUnityPlayerLoopInteractiveControlsControlScriptPassed =
                summary.ControlScriptPassed,
            RuntimeBackedUnityPlayerLoopInteractiveControlsWindowPresent =
                summary.InteractiveControlsWindowPresent,
            RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmokePassed =
                summary.UnityInteractiveControlsSmokePassed,
            RuntimeBackedUnityPlayerLoopInteractiveControlsRuntimeAuthority = summary.RuntimeAuthority,
            RuntimeBackedUnityPlayerLoopInteractiveControlsUnityGameplayTruth =
                summary.UnityGameplayTruth,
            RuntimeBackedUnityPlayerLoopInteractiveControlsProjectionOnly = summary.ProjectionOnly,
            RuntimeBackedUnityPlayerLoopInteractiveControlsNormalCommand = summary.NormalCommand,
            RuntimeBackedUnityPlayerLoopInteractiveControlsReportPath = summary.ReportPath,
            RuntimeBackedUnityPlayerLoopInteractiveControlsManualUnityOptional =
                summary.ManualUnityOptional,
            RuntimeBackedUnityPlayerLoopInteractiveControlsAccepted = summary.Accepted,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static RuntimeBackedUnityPlayerLoopInteractiveControlsWorkspaceSummary
        LoadRuntimeBackedUnityPlayerLoopInteractiveControlsSummary(string projectRoot)
    {
        using var dashboard = TryReadJson(
            projectRoot,
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DashboardRelativePath,
            []);
        return new RuntimeBackedUnityPlayerLoopInteractiveControlsWorkspaceSummary(
            AcceptedGoal138:
                dashboard is not null && TryGetBool(dashboard.RootElement, "acceptedGoal138"),
            CandidateId: Goal138String(dashboard?.RootElement, "candidateId"),
            FrameCount: dashboard is not null
                ? Goal138Int(dashboard.RootElement, "frameCount")
                : 0,
            RequiredControlsPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "requiredControlsPresent"),
            ControlScriptPassed:
                dashboard is not null && TryGetBool(dashboard.RootElement, "controlScriptPassed"),
            InteractiveControlsWindowPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "interactiveControlsWindowPresent"),
            UnityInteractiveControlsSmokePassed:
                dashboard is not null && TryGetBool(dashboard.RootElement, "unityInteractiveControlsSmokePassed"),
            RuntimeAuthority:
                dashboard is not null && TryGetBool(dashboard.RootElement, "runtimeAuthority"),
            UnityGameplayTruth:
                dashboard is not null && TryGetBool(dashboard.RootElement, "unityGameplayTruth"),
            ProjectionOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionOnly"),
            NormalCommand: Goal138String(dashboard?.RootElement, "normalCommand"),
            ReportPath: Goal138String(dashboard?.RootElement, "reportPath"),
            ManualUnityOptional:
                dashboard is not null && TryGetBool(dashboard.RootElement, "manualUnityOptional"),
            Accepted:
                dashboard is not null && TryGetBool(dashboard.RootElement, "accepted"),
            QualityGatePassed:
                Goal138String(dashboard?.RootElement, "status") == "GREEN",
            RelativePaths: Goal139AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal139FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists
                ? "Goal139 runtime-backed Unity player loop interactive controls file exists"
                : "Goal139 runtime-backed Unity player loop interactive controls file missing",
            SafeRatingMetadataSummary =
                "runtimeBackedUnityPlayerLoopInteractiveControlsArtifact=true; noManualInput=true"
        };
    }

    private static bool Goal139AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ProceduralOutputDirectory,
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.AllDirectories)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private sealed record RuntimeBackedUnityPlayerLoopInteractiveControlsWorkspaceSummary(
        bool AcceptedGoal138,
        string CandidateId,
        int FrameCount,
        bool RequiredControlsPresent,
        bool ControlScriptPassed,
        bool InteractiveControlsWindowPresent,
        bool UnityInteractiveControlsSmokePassed,
        bool RuntimeAuthority,
        bool UnityGameplayTruth,
        bool ProjectionOnly,
        string NormalCommand,
        string ReportPath,
        bool ManualUnityOptional,
        bool Accepted,
        bool QualityGatePassed,
        bool RelativePaths);
}
