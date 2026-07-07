using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildRuntimeBackedUnityPlayerLoopStepperGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadRuntimeBackedUnityPlayerLoopStepperSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                RuntimeBackedUnityPlayerLoopStepperVocabulary.ProceduralOutputDirectory,
                RuntimeBackedUnityPlayerLoopStepperVocabulary.GoalId,
                BuildGoal138ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithRuntimeBackedUnityPlayerLoopStepperSummary(entry, summary))
            .ToList();

        foreach (var file in BuildGoal138ExportFiles())
        {
            entries.Add(WithRuntimeBackedUnityPlayerLoopStepperSummary(
                Goal138FileEntry(projectRoot, file.RelativePath, file.Kind),
                summary));
        }

        entries.Add(WithRuntimeBackedUnityPlayerLoopStepperSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = RuntimeBackedUnityPlayerLoopStepperVocabulary.GoalId + ".summary",
                RelativePath = RuntimeBackedUnityPlayerLoopStepperVocabulary.DashboardRelativePath,
                ArtifactKind = "runtime_backed_unity_player_loop_stepper_workspace_summary",
                SourceGoalId = RuntimeBackedUnityPlayerLoopStepperVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    RuntimeBackedUnityPlayerLoopStepperVocabulary.DashboardRelativePath,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "candidateId=" + summary.CandidateId
                                    + "; frameCount=" + summary.FrameCount
                                    + "; stepperBatchSmokePassed="
                                    + summary.StepperBatchSmokePassed.ToString().ToLowerInvariant(),
                SafeRatingMetadataSummary =
                    "runtimeAuthority=true; projectionOnly=false; unityGameplayTruth=false"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "runtime_backed_unity_player_loop_stepper",
            "Goal 138 Runtime-backed Unity Player Loop Stepper",
            RuntimeBackedUnityPlayerLoopStepperVocabulary.GoalId,
            RuntimeBackedUnityPlayerLoopStepperVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal138ProceduralFiles() =>
    [
        (RuntimeBackedUnityPlayerLoopStepperVocabulary.Goal137AcceptanceFileName,
            "runtime_backed_unity_player_loop_stepper_goal137_acceptance"),
        (RuntimeBackedUnityPlayerLoopStepperVocabulary.ModelFileName,
            "runtime_backed_unity_player_loop_stepper_model"),
        (RuntimeBackedUnityPlayerLoopStepperVocabulary.DashboardFileName,
            "runtime_backed_unity_player_loop_stepper_dashboard"),
        (RuntimeBackedUnityPlayerLoopStepperVocabulary.ResultFileName,
            "runtime_backed_unity_player_loop_stepper_result"),
        (RuntimeBackedUnityPlayerLoopStepperVocabulary.FrameIndexFileName,
            "runtime_backed_unity_player_loop_stepper_frame_index"),
        (RuntimeBackedUnityPlayerLoopStepperVocabulary.UnitySmokeFileName,
            "unity_player_loop_stepper_smoke"),
        (RuntimeBackedUnityPlayerLoopStepperVocabulary.NegativeProofFileName,
            "runtime_backed_unity_player_loop_stepper_negative_proof"),
        (RuntimeBackedUnityPlayerLoopStepperVocabulary.FileIndexFileName,
            "runtime_backed_unity_player_loop_stepper_file_index"),
        (RuntimeBackedUnityPlayerLoopStepperVocabulary.ReportJsonFileName,
            "runtime_backed_unity_player_loop_stepper_one_click_report_json"),
        (RuntimeBackedUnityPlayerLoopStepperVocabulary.ReportMarkdownFileName,
            "runtime_backed_unity_player_loop_stepper_one_click_report_markdown")
    ];

    private static IReadOnlyList<(string RelativePath, string Kind)> BuildGoal138ExportFiles() =>
        BuildGoal138ProceduralFiles()
            .Select(item => (
                RelativePath:
                RuntimeBackedUnityPlayerLoopStepperVocabulary.ExportPackageDirectory
                + "/"
                + item.FileName,
                Kind: "runtime_backed_unity_player_loop_stepper_export_file"))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();

    private static VisualWorldPreviewArtifactEntry WithRuntimeBackedUnityPlayerLoopStepperSummary(
        VisualWorldPreviewArtifactEntry entry,
        RuntimeBackedUnityPlayerLoopStepperWorkspaceSummary summary) =>
        entry with
        {
            RuntimeBackedUnityPlayerLoopStepperAcceptedGoal137 = summary.AcceptedGoal137,
            RuntimeBackedUnityPlayerLoopStepperCandidateId = summary.CandidateId,
            RuntimeBackedUnityPlayerLoopStepperFrameCount = summary.FrameCount,
            RuntimeBackedUnityPlayerLoopStepperRequiredCategoriesPresent =
                summary.RequiredFrameCategoriesPresent,
            RuntimeBackedUnityPlayerLoopStepperRuntimeAuthority = summary.RuntimeAuthority,
            RuntimeBackedUnityPlayerLoopStepperUnityGameplayTruth = summary.UnityGameplayTruth,
            RuntimeBackedUnityPlayerLoopStepperProjectionOnly = summary.ProjectionOnly,
            RuntimeBackedUnityPlayerLoopStepperWindowPresent = summary.StepperWindowPresent,
            RuntimeBackedUnityPlayerLoopStepperBatchSmokePassed = summary.StepperBatchSmokePassed,
            RuntimeBackedUnityPlayerLoopStepperNormalCommand = summary.NormalCommand,
            RuntimeBackedUnityPlayerLoopStepperReportPath = summary.ReportPath,
            RuntimeBackedUnityPlayerLoopStepperManualUnityOptional = summary.ManualUnityOptional,
            RuntimeBackedUnityPlayerLoopStepperAccepted = summary.Accepted,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static RuntimeBackedUnityPlayerLoopStepperWorkspaceSummary
        LoadRuntimeBackedUnityPlayerLoopStepperSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        using var dashboard = TryReadJson(
            projectRoot,
            RuntimeBackedUnityPlayerLoopStepperVocabulary.DashboardRelativePath,
            diagnostics);
        return new RuntimeBackedUnityPlayerLoopStepperWorkspaceSummary(
            AcceptedGoal137:
                dashboard is not null && TryGetBool(dashboard.RootElement, "acceptedGoal137"),
            CandidateId: Goal138String(dashboard?.RootElement, "candidateId"),
            FrameCount: dashboard is not null
                ? Goal138Int(dashboard.RootElement, "frameCount")
                : 0,
            RequiredFrameCategoriesPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "requiredFrameCategoriesPresent"),
            RuntimeAuthority:
                dashboard is not null && TryGetBool(dashboard.RootElement, "runtimeAuthority"),
            UnityGameplayTruth:
                dashboard is not null && TryGetBool(dashboard.RootElement, "unityGameplayTruth"),
            ProjectionOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionOnly"),
            StepperWindowPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "stepperWindowPresent"),
            StepperBatchSmokePassed:
                dashboard is not null && TryGetBool(dashboard.RootElement, "stepperBatchSmokePassed"),
            NormalCommand: Goal138String(dashboard?.RootElement, "normalCommand"),
            ReportPath: Goal138String(dashboard?.RootElement, "reportPath"),
            ManualUnityOptional:
                dashboard is not null && TryGetBool(dashboard.RootElement, "manualUnityOptional"),
            Accepted:
                dashboard is not null && TryGetBool(dashboard.RootElement, "accepted"),
            QualityGatePassed:
                Goal138String(dashboard?.RootElement, "status") == "GREEN",
            RelativePaths: Goal138AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal138FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = RuntimeBackedUnityPlayerLoopStepperVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = RuntimeBackedUnityPlayerLoopStepperVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists
                ? "Goal138 runtime-backed Unity player loop stepper file exists"
                : "Goal138 runtime-backed Unity player loop stepper file missing",
            SafeRatingMetadataSummary =
                "runtimeBackedUnityPlayerLoopStepperArtifact=true; noManualInput=true"
        };
    }

    private static bool Goal138AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            RuntimeBackedUnityPlayerLoopStepperVocabulary.ProceduralOutputDirectory,
            RuntimeBackedUnityPlayerLoopStepperVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.AllDirectories)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal138String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int Goal138Int(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private sealed record RuntimeBackedUnityPlayerLoopStepperWorkspaceSummary(
        bool AcceptedGoal137,
        string CandidateId,
        int FrameCount,
        bool RequiredFrameCategoriesPresent,
        bool RuntimeAuthority,
        bool UnityGameplayTruth,
        bool ProjectionOnly,
        bool StepperWindowPresent,
        bool StepperBatchSmokePassed,
        string NormalCommand,
        string ReportPath,
        bool ManualUnityOptional,
        bool Accepted,
        bool QualityGatePassed,
        bool RelativePaths);
}
