using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal127UnityProjectionVerificationRunnerQuality
        BuildGoal127UnityProjectionVerificationRunnerQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "unity_projection_verification_runner");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "unity_projection_verification_runner_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal127AllowedPath(entry.RelativePath));
        return new Goal127UnityProjectionVerificationRunnerQuality(
            GroupPresent: group is not null,
            RunnerStatus: summary?.UnityProjectionVerificationRunnerStatus ?? string.Empty,
            Mode: summary?.UnityProjectionVerificationRunnerMode ?? string.Empty,
            RunnerScriptPath: summary?.UnityProjectionVerificationRunnerScriptPath ?? string.Empty,
            RunnerCmdPath: summary?.UnityProjectionVerificationRunnerCmdPath ?? string.Empty,
            RunnerCommand: summary?.UnityProjectionVerificationRunnerCommand ?? string.Empty,
            UnityExecuteMethod: summary?.UnityProjectionVerificationRunnerExecuteMethod ?? string.Empty,
            ResultPath: summary?.UnityProjectionVerificationRunnerResultPath ?? string.Empty,
            LogPath: summary?.UnityProjectionVerificationRunnerLogPath ?? string.Empty,
            PassMarkerPresent:
                summary?.UnityProjectionVerificationRunnerPassMarkerPresent == true,
            CleanupApplied: summary?.UnityProjectionVerificationRunnerCleanupApplied == true,
            CleanupScriptAvailable:
                summary?.UnityProjectionVerificationRunnerCleanupScriptAvailable == true,
            CleanupCommand: summary?.UnityProjectionVerificationRunnerCleanupCommand ?? string.Empty,
            ManualUnityClickingRequired:
                summary?.UnityProjectionVerificationRunnerManualUnityClickingRequired == true,
            EvidencePath: summary?.UnityProjectionVerificationRunnerEvidencePath ?? string.Empty,
            ExportPath: summary?.UnityProjectionVerificationRunnerExportPath ?? string.Empty,
            ScriptScanPassed:
                summary?.UnityProjectionVerificationRunnerScriptScanPassed == true,
            ResultPassed: summary?.UnityProjectionVerificationRunnerResultPassed == true,
            LogPassed: summary?.UnityProjectionVerificationRunnerLogPassed == true,
            Goal126FullPlaythroughGreen:
                summary?.UnityProjectionVerificationRunnerGoal126FullPlaythroughGreen == true,
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal127.unity_projection_runner.goal126_green" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal127.unity_projection_runner.script_scan" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal127.unity_projection_runner.result" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal127.unity_projection_runner.log_scan" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal127.unity_projection_runner.negative_proof" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal127.unity_projection_runner.cleanup_applied" && proof.Passed)
                && summary?.UnityProjectionVerificationRunnerStatus == "GREEN"
                && summary?.UnityProjectionVerificationRunnerMode
                    == UnityProjectionVerificationRunnerVocabulary.Mode
                && summary?.UnityProjectionVerificationRunnerExecuteMethod
                    == UnityProjectionVerificationRunnerVocabulary.UnityBatchmodeExecuteMethod
                && summary?.UnityProjectionVerificationRunnerPassMarkerPresent == true
                && summary?.UnityProjectionVerificationRunnerCleanupApplied == true
                && summary?.UnityProjectionVerificationRunnerCleanupScriptAvailable == true
                && summary?.UnityProjectionVerificationRunnerManualUnityClickingRequired == false,
            RelativePaths: relativePaths);
    }

    private static void AddGoal127UnityProjectionVerificationRunnerQualityDiagnostics(
        Goal127UnityProjectionVerificationRunnerQuality runner,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(runner.GroupPresent, "goal127.quality.runner_group",
            "unity_projection_verification_runner", diagnostics);
        AddIfFalse(runner.RunnerStatus == "GREEN", "goal127.quality.runner_status",
            "unity_projection_verification_runner", diagnostics);
        AddIfFalse(runner.Mode == UnityProjectionVerificationRunnerVocabulary.Mode,
            "goal127.quality.mode", "unity_projection_verification_runner", diagnostics);
        AddIfFalse(
            runner.UnityExecuteMethod
            == UnityProjectionVerificationRunnerVocabulary.UnityBatchmodeExecuteMethod,
            "goal127.quality.execute_method",
            "unity_projection_verification_runner",
            diagnostics);
        AddIfFalse(runner.PassMarkerPresent, "goal127.quality.pass_marker",
            "unity_projection_verification_runner", diagnostics);
        AddIfFalse(runner.CleanupApplied, "goal127.quality.cleanup_applied",
            "unity_projection_verification_runner", diagnostics);
        AddIfFalse(runner.CleanupScriptAvailable, "goal127.quality.cleanup_script",
            "unity_projection_verification_runner", diagnostics);
        AddIfFalse(!runner.ManualUnityClickingRequired, "goal127.quality.manual_clicking",
            "unity_projection_verification_runner", diagnostics);
        AddIfFalse(runner.ScriptScanPassed, "goal127.quality.script_scan",
            "unity_projection_verification_runner", diagnostics);
        AddIfFalse(runner.ResultPassed, "goal127.quality.result",
            "unity_projection_verification_runner", diagnostics);
        AddIfFalse(runner.LogPassed, "goal127.quality.log_scan",
            "unity_projection_verification_runner", diagnostics);
        AddIfFalse(runner.Goal126FullPlaythroughGreen, "goal127.quality.goal126_green",
            "unity_projection_verification_runner", diagnostics);
        AddIfFalse(runner.QualityGatePassed, "goal127.quality.quality_gate",
            "unity_projection_verification_runner", diagnostics);
        AddIfFalse(runner.RelativePaths, "goal127.quality.relative_paths",
            "unity_projection_verification_runner", diagnostics);
        AddIfFalse(binding.PageBindDisplaysUnityProjectionVerificationRunner,
            "goal127.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
    }

    private static bool Goal127AllowedPath(string path) =>
        path.StartsWith(
            UnityProjectionVerificationRunnerVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            UnityProjectionVerificationRunnerVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal127UnityProjectionVerificationRunnerQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal127UnityProjectionVerificationRunnerQuality runner,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            UnityProjectionVerificationRunnerGroupPresent = runner.GroupPresent,
            UnityProjectionVerificationRunnerStatus = runner.RunnerStatus,
            UnityProjectionVerificationRunnerMode = runner.Mode,
            UnityProjectionVerificationRunnerScriptPath = runner.RunnerScriptPath,
            UnityProjectionVerificationRunnerCmdPath = runner.RunnerCmdPath,
            UnityProjectionVerificationRunnerCommand = runner.RunnerCommand,
            UnityProjectionVerificationRunnerExecuteMethod = runner.UnityExecuteMethod,
            UnityProjectionVerificationRunnerResultPath = runner.ResultPath,
            UnityProjectionVerificationRunnerLogPath = runner.LogPath,
            UnityProjectionVerificationRunnerPassMarkerPresent = runner.PassMarkerPresent,
            UnityProjectionVerificationRunnerCleanupApplied = runner.CleanupApplied,
            UnityProjectionVerificationRunnerCleanupScriptAvailable = runner.CleanupScriptAvailable,
            UnityProjectionVerificationRunnerCleanupCommand = runner.CleanupCommand,
            UnityProjectionVerificationRunnerManualUnityClickingRequired =
                runner.ManualUnityClickingRequired,
            UnityProjectionVerificationRunnerEvidencePath = runner.EvidencePath,
            UnityProjectionVerificationRunnerExportPath = runner.ExportPath,
            UnityProjectionVerificationRunnerScriptScanPassed = runner.ScriptScanPassed,
            UnityProjectionVerificationRunnerResultPassed = runner.ResultPassed,
            UnityProjectionVerificationRunnerLogPassed = runner.LogPassed,
            UnityProjectionVerificationRunnerGoal126FullPlaythroughGreen =
                runner.Goal126FullPlaythroughGreen,
            UnityProjectionVerificationRunnerQualityGatePassed = runner.QualityGatePassed,
            Goal127FilesDiscoveredByRelativePaths = runner.RelativePaths,
            WinFormsUnityProjectionVerificationRunnerBindingReal =
                binding.PageBindDisplaysUnityProjectionVerificationRunner
        };

    private sealed record Goal127UnityProjectionVerificationRunnerQuality(
        bool GroupPresent,
        string RunnerStatus,
        string Mode,
        string RunnerScriptPath,
        string RunnerCmdPath,
        string RunnerCommand,
        string UnityExecuteMethod,
        string ResultPath,
        string LogPath,
        bool PassMarkerPresent,
        bool CleanupApplied,
        bool CleanupScriptAvailable,
        string CleanupCommand,
        bool ManualUnityClickingRequired,
        string EvidencePath,
        string ExportPath,
        bool ScriptScanPassed,
        bool ResultPassed,
        bool LogPassed,
        bool Goal126FullPlaythroughGreen,
        bool QualityGatePassed,
        bool RelativePaths);
}
