using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal132CandidatePipelineOperatorQuality BuildGoal132CandidatePipelineOperatorQuality(
        IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "candidate_pipeline_operator_panel");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "candidate_pipeline_operator_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal132AllowedPath(entry.RelativePath));
        var qualityGatePassed =
            ProofPassed(proofs, "goal132.candidate_pipeline_operator.dashboard")
            && ProofPassed(proofs, "goal132.candidate_pipeline_operator.refresh_button")
            && ProofPassed(proofs, "goal132.candidate_pipeline_operator.copy_button")
            && ProofPassed(proofs, "goal132.candidate_pipeline_operator.dry_run_button")
            && ProofPassed(proofs, "goal132.candidate_pipeline_operator.run_button")
            && ProofPassed(proofs, "goal132.candidate_pipeline_operator.async_run")
            && ProofPassed(proofs, "goal132.candidate_pipeline_operator.result")
            && ProofPassed(proofs, "goal132.candidate_pipeline_operator.script_scan")
            && ProofPassed(proofs, "goal132.candidate_pipeline_operator.winforms_scan")
            && ProofPassed(proofs, "goal132.candidate_pipeline_operator.negative_proof")
            && ProofPassed(proofs, "goal132.candidate_pipeline_operator.manual_unity_optional")
            && ProofPassed(proofs, "goal132.candidate_pipeline_operator.projection_only")
            && ProofPassed(proofs, "goal132.candidate_pipeline_operator.sample_read_only")
            && summary?.CandidatePipelineOperatorStatus == "GREEN_READY"
            && summary?.CandidatePipelineOperatorNormalCommand
                == GamePackageCandidatePipelineOperatorVocabulary.NormalCommand
            && summary?.CandidatePipelineOperatorDryRunCommand
                == GamePackageCandidatePipelineOperatorVocabulary.DryRunCommand
            && summary?.CandidatePipelineOperatorResultPath
                == GamePackageCandidatePipelineOperatorVocabulary.Goal131ResultPath
            && !string.IsNullOrWhiteSpace(summary?.CandidatePipelineOperatorSelectedCandidateId)
            && summary?.CandidatePipelineOperatorSelectedCandidateScore > 0
            && summary?.CandidatePipelineOperatorCandidateCount >= 4
            && summary?.CandidatePipelineOperatorPassedCandidates
                == summary?.CandidatePipelineOperatorCandidateCount
            && summary?.CandidatePipelineOperatorFailedCandidates == 0
            && summary?.CandidatePipelineOperatorMatrixPassed == true
            && summary?.CandidatePipelineOperatorManualUnityOptional == true
            && summary?.CandidatePipelineOperatorProjectionOnly == true
            && summary?.CandidatePipelineOperatorSamplePackageReadOnly == true
            && summary?.CandidatePipelineOperatorWinFormsPanelPresent == true
            && summary?.CandidatePipelineOperatorRefreshButtonPresent == true
            && summary?.CandidatePipelineOperatorCopyCommandButtonPresent == true
            && summary?.CandidatePipelineOperatorDryRunButtonPresent == true
            && summary?.CandidatePipelineOperatorRunButtonPresent == true
            && summary?.CandidatePipelineOperatorAsyncRunPresent == true
            && summary?.CandidatePipelineOperatorResultPresent == true
            && summary?.CandidatePipelineOperatorEvidencePath
                == GamePackageCandidatePipelineOperatorVocabulary.ProceduralOutputDirectory
            && summary?.CandidatePipelineOperatorExportPath
                == GamePackageCandidatePipelineOperatorVocabulary.ExportPackageDirectory
            && relativePaths;

        return new Goal132CandidatePipelineOperatorQuality(
            GroupPresent: group is not null,
            OperatorStatus: summary?.CandidatePipelineOperatorStatus ?? string.Empty,
            NormalCommand: summary?.CandidatePipelineOperatorNormalCommand ?? string.Empty,
            DryRunCommand: summary?.CandidatePipelineOperatorDryRunCommand ?? string.Empty,
            ResultPath: summary?.CandidatePipelineOperatorResultPath ?? string.Empty,
            SelectedCandidateId: summary?.CandidatePipelineOperatorSelectedCandidateId ?? string.Empty,
            SelectedCandidateScore: summary?.CandidatePipelineOperatorSelectedCandidateScore ?? 0,
            CandidateCount: summary?.CandidatePipelineOperatorCandidateCount ?? 0,
            PassedCandidates: summary?.CandidatePipelineOperatorPassedCandidates ?? 0,
            FailedCandidates: summary?.CandidatePipelineOperatorFailedCandidates ?? 0,
            MatrixPassed: summary?.CandidatePipelineOperatorMatrixPassed == true,
            LastExitCode: summary?.CandidatePipelineOperatorLastExitCode ?? -1,
            LastDurationMilliseconds:
                summary?.CandidatePipelineOperatorLastDurationMilliseconds ?? 0,
            OutputTail: summary?.CandidatePipelineOperatorOutputTail ?? string.Empty,
            ManualUnityOptional: summary?.CandidatePipelineOperatorManualUnityOptional == true,
            ProjectionOnly: summary?.CandidatePipelineOperatorProjectionOnly == true,
            SamplePackageReadOnly: summary?.CandidatePipelineOperatorSamplePackageReadOnly == true,
            WinFormsPanelPresent: summary?.CandidatePipelineOperatorWinFormsPanelPresent == true,
            RefreshButtonPresent: summary?.CandidatePipelineOperatorRefreshButtonPresent == true,
            CopyCommandButtonPresent: summary?.CandidatePipelineOperatorCopyCommandButtonPresent == true,
            DryRunButtonPresent: summary?.CandidatePipelineOperatorDryRunButtonPresent == true,
            RunButtonPresent: summary?.CandidatePipelineOperatorRunButtonPresent == true,
            AsyncRunPresent: summary?.CandidatePipelineOperatorAsyncRunPresent == true,
            OperatorResultPresent: summary?.CandidatePipelineOperatorResultPresent == true,
            EvidencePath: summary?.CandidatePipelineOperatorEvidencePath ?? string.Empty,
            ExportPath: summary?.CandidatePipelineOperatorExportPath ?? string.Empty,
            QualityGatePassed: qualityGatePassed,
            RelativePaths: relativePaths);
    }

    private static void AddGoal132CandidatePipelineOperatorQualityDiagnostics(
        Goal132CandidatePipelineOperatorQuality candidateOperator,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(binding.PageBindDisplaysCandidatePipelineOperator,
            "goal132.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(candidateOperator.GroupPresent,
            "goal132.quality.group_present",
            "candidate_pipeline_operator_panel",
            diagnostics);

        if (!candidateOperator.GroupPresent || candidateOperator.OperatorStatus != "GREEN_READY")
        {
            return;
        }

        AddIfFalse(candidateOperator.NormalCommand
                   == GamePackageCandidatePipelineOperatorVocabulary.NormalCommand,
            "goal132.quality.normal_command",
            "candidate_pipeline_operator_panel",
            diagnostics);
        AddIfFalse(candidateOperator.ResultPath
                   == GamePackageCandidatePipelineOperatorVocabulary.Goal131ResultPath,
            "goal132.quality.result_path",
            "candidate_pipeline_operator_panel",
            diagnostics);
        AddIfFalse(candidateOperator.CandidateCount >= 4,
            "goal132.quality.candidate_count",
            "candidate_pipeline_operator_panel",
            diagnostics);
        AddIfFalse(candidateOperator.PassedCandidates == candidateOperator.CandidateCount,
            "goal132.quality.passed_candidates",
            "candidate_pipeline_operator_panel",
            diagnostics);
        AddIfFalse(candidateOperator.FailedCandidates == 0,
            "goal132.quality.failed_candidates",
            "candidate_pipeline_operator_panel",
            diagnostics);
        AddIfFalse(candidateOperator.MatrixPassed,
            "goal132.quality.matrix_passed",
            "candidate_pipeline_operator_panel",
            diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(candidateOperator.SelectedCandidateId),
            "goal132.quality.selected_candidate",
            "candidate_pipeline_operator_panel",
            diagnostics);
        AddIfFalse(candidateOperator.SelectedCandidateScore > 0,
            "goal132.quality.selected_score",
            "candidate_pipeline_operator_panel",
            diagnostics);
        AddIfFalse(candidateOperator.WinFormsPanelPresent
                   && candidateOperator.RefreshButtonPresent
                   && candidateOperator.CopyCommandButtonPresent
                   && candidateOperator.DryRunButtonPresent
                   && candidateOperator.RunButtonPresent,
            "goal132.quality.buttons_present",
            "candidate_pipeline_operator_panel",
            diagnostics);
        AddIfFalse(candidateOperator.AsyncRunPresent,
            "goal132.quality.async_run",
            "candidate_pipeline_operator_panel",
            diagnostics);
        AddIfFalse(candidateOperator.OperatorResultPresent,
            "goal132.quality.operator_result",
            "candidate_pipeline_operator_panel",
            diagnostics);
        AddIfFalse(candidateOperator.ManualUnityOptional,
            "goal132.quality.manual_unity_optional",
            "candidate_pipeline_operator_panel",
            diagnostics);
        AddIfFalse(candidateOperator.ProjectionOnly,
            "goal132.quality.projection_only",
            "candidate_pipeline_operator_panel",
            diagnostics);
        AddIfFalse(candidateOperator.SamplePackageReadOnly,
            "goal132.quality.sample_read_only",
            "candidate_pipeline_operator_panel",
            diagnostics);
        AddIfFalse(candidateOperator.QualityGatePassed,
            "goal132.quality.quality_gate",
            "candidate_pipeline_operator_panel",
            diagnostics);
        AddIfFalse(candidateOperator.RelativePaths,
            "goal132.quality.relative_paths",
            "candidate_pipeline_operator_panel",
            diagnostics);
    }

    private static bool Goal132AllowedPath(string path) =>
        path.StartsWith(
            GamePackageCandidatePipelineOperatorVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            GamePackageCandidatePipelineOperatorVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate ApplyGoal132CandidatePipelineOperatorQuality(
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        Goal132CandidatePipelineOperatorQuality candidateOperator,
        VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            CandidatePipelineOperatorGroupPresent = candidateOperator.GroupPresent,
            CandidatePipelineOperatorStatus = candidateOperator.OperatorStatus,
            CandidatePipelineOperatorNormalCommand = candidateOperator.NormalCommand,
            CandidatePipelineOperatorDryRunCommand = candidateOperator.DryRunCommand,
            CandidatePipelineOperatorResultPath = candidateOperator.ResultPath,
            CandidatePipelineOperatorSelectedCandidateId = candidateOperator.SelectedCandidateId,
            CandidatePipelineOperatorSelectedCandidateScore =
                candidateOperator.SelectedCandidateScore,
            CandidatePipelineOperatorCandidateCount = candidateOperator.CandidateCount,
            CandidatePipelineOperatorPassedCandidates = candidateOperator.PassedCandidates,
            CandidatePipelineOperatorFailedCandidates = candidateOperator.FailedCandidates,
            CandidatePipelineOperatorMatrixPassed = candidateOperator.MatrixPassed,
            CandidatePipelineOperatorLastExitCode = candidateOperator.LastExitCode,
            CandidatePipelineOperatorLastDurationMilliseconds =
                candidateOperator.LastDurationMilliseconds,
            CandidatePipelineOperatorOutputTail = candidateOperator.OutputTail,
            CandidatePipelineOperatorManualUnityOptional = candidateOperator.ManualUnityOptional,
            CandidatePipelineOperatorProjectionOnly = candidateOperator.ProjectionOnly,
            CandidatePipelineOperatorSamplePackageReadOnly = candidateOperator.SamplePackageReadOnly,
            CandidatePipelineOperatorWinFormsPanelPresent = candidateOperator.WinFormsPanelPresent,
            CandidatePipelineOperatorRefreshButtonPresent = candidateOperator.RefreshButtonPresent,
            CandidatePipelineOperatorCopyCommandButtonPresent =
                candidateOperator.CopyCommandButtonPresent,
            CandidatePipelineOperatorDryRunButtonPresent = candidateOperator.DryRunButtonPresent,
            CandidatePipelineOperatorRunButtonPresent = candidateOperator.RunButtonPresent,
            CandidatePipelineOperatorAsyncRunPresent = candidateOperator.AsyncRunPresent,
            CandidatePipelineOperatorResultPresent = candidateOperator.OperatorResultPresent,
            CandidatePipelineOperatorEvidencePath = candidateOperator.EvidencePath,
            CandidatePipelineOperatorExportPath = candidateOperator.ExportPath,
            CandidatePipelineOperatorQualityGatePassed = candidateOperator.QualityGatePassed,
            Goal132FilesDiscoveredByRelativePaths = candidateOperator.RelativePaths,
            WinFormsCandidatePipelineOperatorBindingReal =
                binding.PageBindDisplaysCandidatePipelineOperator
        };

    private sealed record Goal132CandidatePipelineOperatorQuality(
        bool GroupPresent,
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
