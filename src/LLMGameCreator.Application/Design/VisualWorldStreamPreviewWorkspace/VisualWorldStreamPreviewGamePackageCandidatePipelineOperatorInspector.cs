using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildCandidatePipelineOperatorGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadCandidatePipelineOperatorSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                GamePackageCandidatePipelineOperatorVocabulary.ProceduralOutputDirectory,
                GamePackageCandidatePipelineOperatorVocabulary.GoalId,
                BuildGoal132ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithCandidatePipelineOperatorSummary(entry, summary))
            .ToList();

        foreach (var file in BuildGoal132ExportFiles())
        {
            entries.Add(WithCandidatePipelineOperatorSummary(
                Goal132FileEntry(projectRoot, file.RelativePath, file.Kind),
                summary));
        }

        entries.Add(WithCandidatePipelineOperatorSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = GamePackageCandidatePipelineOperatorVocabulary.GoalId + ".summary",
                RelativePath = GamePackageCandidatePipelineOperatorVocabulary.DashboardRelativePath,
                ArtifactKind = "candidate_pipeline_operator_workspace_summary",
                SourceGoalId = GamePackageCandidatePipelineOperatorVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    GamePackageCandidatePipelineOperatorVocabulary.DashboardRelativePath,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "operatorStatus=" + summary.OperatorStatus
                                    + "; candidateCount=" + summary.CandidateCount
                                    + "; selectedCandidateId=" + summary.SelectedCandidateId,
                SafeRatingMetadataSummary =
                    "projectionOnly=true; manualUnityOptional=true; normalCommand="
                    + summary.NormalCommand
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "candidate_pipeline_operator_panel",
            "Goal 132 Candidate Pipeline Operator Panel",
            GamePackageCandidatePipelineOperatorVocabulary.GoalId,
            GamePackageCandidatePipelineOperatorVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal132ProceduralFiles() =>
    [
        (GamePackageCandidatePipelineOperatorVocabulary.DashboardFileName,
            "candidate_pipeline_operator_dashboard"),
        (GamePackageCandidatePipelineOperatorVocabulary.ResultFileName,
            "candidate_pipeline_operator_result"),
        (GamePackageCandidatePipelineOperatorVocabulary.ScriptScanFileName,
            "candidate_pipeline_operator_script_scan"),
        (GamePackageCandidatePipelineOperatorVocabulary.WinFormsScanFileName,
            "candidate_pipeline_operator_winforms_scan"),
        (GamePackageCandidatePipelineOperatorVocabulary.NegativeProofFileName,
            "candidate_pipeline_operator_negative_proof"),
        (GamePackageCandidatePipelineOperatorVocabulary.ReportFileName,
            "candidate_pipeline_operator_report"),
        (GamePackageCandidatePipelineOperatorVocabulary.FileIndexFileName,
            "candidate_pipeline_operator_file_index")
    ];

    private static IReadOnlyList<(string RelativePath, string Kind)> BuildGoal132ExportFiles() =>
        BuildGoal132ProceduralFiles()
            .Select(item => (
                RelativePath:
                GamePackageCandidatePipelineOperatorVocabulary.ExportPackageDirectory
                + "/"
                + item.FileName,
                Kind: "candidate_pipeline_operator_export_file"))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();

    private static VisualWorldPreviewArtifactEntry WithCandidatePipelineOperatorSummary(
        VisualWorldPreviewArtifactEntry entry,
        CandidatePipelineOperatorWorkspaceSummary summary) =>
        entry with
        {
            CandidatePipelineOperatorStatus = summary.OperatorStatus,
            CandidatePipelineOperatorNormalCommand = summary.NormalCommand,
            CandidatePipelineOperatorDryRunCommand = summary.DryRunCommand,
            CandidatePipelineOperatorResultPath = summary.ResultPath,
            CandidatePipelineOperatorSelectedCandidateId = summary.SelectedCandidateId,
            CandidatePipelineOperatorSelectedCandidateScore = summary.SelectedCandidateScore,
            CandidatePipelineOperatorCandidateCount = summary.CandidateCount,
            CandidatePipelineOperatorPassedCandidates = summary.PassedCandidates,
            CandidatePipelineOperatorFailedCandidates = summary.FailedCandidates,
            CandidatePipelineOperatorMatrixPassed = summary.MatrixPassed,
            CandidatePipelineOperatorLastExitCode = summary.LastExitCode,
            CandidatePipelineOperatorLastDurationMilliseconds =
                summary.LastDurationMilliseconds,
            CandidatePipelineOperatorOutputTail = summary.OutputTail,
            CandidatePipelineOperatorManualUnityOptional = summary.ManualUnityOptional,
            CandidatePipelineOperatorProjectionOnly = summary.ProjectionOnly,
            CandidatePipelineOperatorSamplePackageReadOnly = summary.SamplePackageReadOnly,
            CandidatePipelineOperatorWinFormsPanelPresent = summary.WinFormsPanelPresent,
            CandidatePipelineOperatorRefreshButtonPresent = summary.RefreshButtonPresent,
            CandidatePipelineOperatorCopyCommandButtonPresent = summary.CopyCommandButtonPresent,
            CandidatePipelineOperatorDryRunButtonPresent = summary.DryRunButtonPresent,
            CandidatePipelineOperatorRunButtonPresent = summary.RunButtonPresent,
            CandidatePipelineOperatorAsyncRunPresent = summary.AsyncRunPresent,
            CandidatePipelineOperatorResultPresent = summary.OperatorResultPresent,
            CandidatePipelineOperatorEvidencePath = summary.EvidencePath,
            CandidatePipelineOperatorExportPath = summary.ExportPath,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static CandidatePipelineOperatorWorkspaceSummary LoadCandidatePipelineOperatorSummary(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        using var dashboard = TryReadJson(
            projectRoot,
            GamePackageCandidatePipelineOperatorVocabulary.DashboardRelativePath,
            diagnostics);
        using var result = TryReadOptionalGoal132Json(
            projectRoot,
            GamePackageCandidatePipelineOperatorVocabulary.ResultRelativePath,
            diagnostics);

        return new CandidatePipelineOperatorWorkspaceSummary(
            OperatorStatus: Goal132String(dashboard?.RootElement, "operatorStatus"),
            NormalCommand: Goal132String(dashboard?.RootElement, "normalCommand"),
            DryRunCommand: Goal132String(dashboard?.RootElement, "dryRunCommand"),
            ResultPath: Goal132String(dashboard?.RootElement, "resultPath"),
            SelectedCandidateId: Goal132String(dashboard?.RootElement, "selectedCandidateId"),
            SelectedCandidateScore: dashboard is not null
                ? Goal132Int(dashboard.RootElement, "selectedCandidateScore")
                : 0,
            CandidateCount: dashboard is not null
                ? Goal132Int(dashboard.RootElement, "candidateCount")
                : 0,
            PassedCandidates: dashboard is not null
                ? Goal132Int(dashboard.RootElement, "passedCandidates")
                : 0,
            FailedCandidates: dashboard is not null
                ? Goal132Int(dashboard.RootElement, "failedCandidates")
                : 0,
            MatrixPassed: dashboard is not null && TryGetBool(dashboard.RootElement, "matrixPassed"),
            LastExitCode: dashboard is not null
                ? Goal132Int(dashboard.RootElement, "lastOperatorExitCode")
                : -1,
            LastDurationMilliseconds: dashboard is not null
                ? Goal132Long(dashboard.RootElement, "lastOperatorDurationMilliseconds")
                : 0,
            OutputTail: Goal132String(dashboard?.RootElement, "outputTail"),
            ManualUnityOptional:
                dashboard is not null && TryGetBool(dashboard.RootElement, "manualUnityOptional"),
            ProjectionOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionOnly"),
            SamplePackageReadOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "samplePackageReadOnly"),
            WinFormsPanelPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "winFormsPanelPresent"),
            RefreshButtonPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "refreshButtonPresent"),
            CopyCommandButtonPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "copyCommandButtonPresent"),
            DryRunButtonPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "dryRunButtonPresent"),
            RunButtonPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "runButtonPresent"),
            AsyncRunPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "asyncRunPresent"),
            OperatorResultPresent:
                result is not null
                && TryGetBool(result.RootElement, "operatorResultCaptured"),
            EvidencePath: Goal132String(dashboard?.RootElement, "evidencePath"),
            ExportPath: Goal132String(dashboard?.RootElement, "exportPath"),
            QualityGatePassed: Goal132String(dashboard?.RootElement, "operatorStatus")
                               == "GREEN_READY",
            RelativePaths: Goal132AllPathsRelative(projectRoot));
    }

    private static JsonDocument? TryReadOptionalGoal132Json(
        string projectRoot,
        string relativePath,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var fullPath = Resolve(projectRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        try
        {
            return JsonDocument.Parse(File.ReadAllText(fullPath, Encoding.UTF8));
        }
        catch (JsonException ex)
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal132.json.invalid",
                relativePath,
                ex.Message));
            return null;
        }
    }

    private static VisualWorldPreviewArtifactEntry Goal132FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = GamePackageCandidatePipelineOperatorVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = GamePackageCandidatePipelineOperatorVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal132 operator file exists" : "Goal132 operator file missing",
            SafeRatingMetadataSummary = "candidatePipelineOperatorArtifact=true; noManualInput=true"
        };
    }

    private static bool Goal132AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            GamePackageCandidatePipelineOperatorVocabulary.ProceduralOutputDirectory,
            GamePackageCandidatePipelineOperatorVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.AllDirectories)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal132String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int Goal132Int(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private static long Goal132Long(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt64(out var value)
            ? value
            : 0;

    private sealed record CandidatePipelineOperatorWorkspaceSummary(
        string OperatorStatus,
        string NormalCommand,
        string DryRunCommand,
        string ResultPath,
        string SelectedCandidateId,
        int SelectedCandidateScore,
        int CandidateCount,
        int PassedCandidates,
        int FailedCandidates,
        bool MatrixPassed,
        int LastExitCode,
        long LastDurationMilliseconds,
        string OutputTail,
        bool ManualUnityOptional,
        bool ProjectionOnly,
        bool SamplePackageReadOnly,
        bool WinFormsPanelPresent,
        bool RefreshButtonPresent,
        bool CopyCommandButtonPresent,
        bool DryRunButtonPresent,
        bool RunButtonPresent,
        bool AsyncRunPresent,
        bool OperatorResultPresent,
        string EvidencePath,
        string ExportPath,
        bool QualityGatePassed,
        bool RelativePaths);
}
