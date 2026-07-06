using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal128ParameterizedGamePackageRunnerQuality
        BuildGoal128ParameterizedGamePackageRunnerQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "parameterized_gamepackage_projection_runner");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "parameterized_gamepackage_runner_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal128AllowedPath(entry.RelativePath));
        return new Goal128ParameterizedGamePackageRunnerQuality(
            GroupPresent: group is not null,
            RunnerStatus: summary?.ParameterizedGamePackageRunnerStatus ?? string.Empty,
            PackagePath: summary?.ParameterizedGamePackageRunnerPackagePath ?? string.Empty,
            PackagePathRelative: summary?.ParameterizedGamePackageRunnerPackagePathRelative
                                 ?? string.Empty,
            NormalCommand: summary?.ParameterizedGamePackageRunnerNormalCommand ?? string.Empty,
            ExampleCommandWithPackagePath:
                summary?.ParameterizedGamePackageRunnerExampleCommandWithPackagePath ?? string.Empty,
            ResultPath: summary?.ParameterizedGamePackageRunnerResultPath ?? string.Empty,
            LogPath: summary?.ParameterizedGamePackageRunnerLogPath ?? string.Empty,
            UnityExitCode: summary?.ParameterizedGamePackageRunnerUnityExitCode ?? -1,
            PassMarkerPresent:
                summary?.ParameterizedGamePackageRunnerPassMarkerPresent == true,
            CleanupApplied: summary?.ParameterizedGamePackageRunnerCleanupApplied == true,
            ManualUnityOptional: summary?.ParameterizedGamePackageRunnerManualUnityOptional == true,
            ProjectionOnly: summary?.ParameterizedGamePackageRunnerProjectionOnly == true,
            EvidencePath: summary?.ParameterizedGamePackageRunnerEvidencePath ?? string.Empty,
            ExportPath: summary?.ParameterizedGamePackageRunnerExportPath ?? string.Empty,
            ScriptScanPassed:
                summary?.ParameterizedGamePackageRunnerScriptScanPassed == true,
            UnitySourceScanPassed:
                summary?.ParameterizedGamePackageRunnerUnitySourceScanPassed == true,
            ResultPassed: summary?.ParameterizedGamePackageRunnerResultPassed == true,
            LogPassed: summary?.ParameterizedGamePackageRunnerLogPassed == true,
            Goal127Green: summary?.ParameterizedGamePackageRunnerGoal127Green == true,
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal128.parameterized_gamepackage_runner.goal127_green"
                && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal128.parameterized_gamepackage_runner.script_scan"
                    && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal128.parameterized_gamepackage_runner.unity_source_scan"
                    && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal128.parameterized_gamepackage_runner.result" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal128.parameterized_gamepackage_runner.log_scan" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal128.parameterized_gamepackage_runner.negative_proof"
                    && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal128.parameterized_gamepackage_runner.cleanup_applied"
                    && proof.Passed)
                && summary?.ParameterizedGamePackageRunnerStatus == "GREEN"
                && summary?.ParameterizedGamePackageRunnerPackagePathRelative
                    == ParameterizedGamePackageProjectionRunnerVocabulary.DefaultPackageRelativePath
                && summary?.ParameterizedGamePackageRunnerPassMarkerPresent == true
                && summary?.ParameterizedGamePackageRunnerCleanupApplied == true
                && summary?.ParameterizedGamePackageRunnerManualUnityOptional == true
                && summary?.ParameterizedGamePackageRunnerProjectionOnly == true,
            RelativePaths: relativePaths);
    }

    private static void AddGoal128ParameterizedGamePackageRunnerQualityDiagnostics(
        Goal128ParameterizedGamePackageRunnerQuality runner,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(binding.PageBindDisplaysParameterizedGamePackageRunner,
            "goal128.quality.winforms_binding",
            "winformsBinding",
            diagnostics);

        if (!runner.GroupPresent || runner.RunnerStatus != "GREEN")
        {
            return;
        }

        AddIfFalse(
            runner.PackagePathRelative
            == ParameterizedGamePackageProjectionRunnerVocabulary.DefaultPackageRelativePath,
            "goal128.quality.package_path",
            "parameterized_gamepackage_projection_runner",
            diagnostics);
        AddIfFalse(runner.PassMarkerPresent, "goal128.quality.pass_marker",
            "parameterized_gamepackage_projection_runner", diagnostics);
        AddIfFalse(runner.CleanupApplied, "goal128.quality.cleanup_applied",
            "parameterized_gamepackage_projection_runner", diagnostics);
        AddIfFalse(runner.ManualUnityOptional, "goal128.quality.manual_unity_optional",
            "parameterized_gamepackage_projection_runner", diagnostics);
        AddIfFalse(runner.ProjectionOnly, "goal128.quality.projection_only",
            "parameterized_gamepackage_projection_runner", diagnostics);
        AddIfFalse(runner.ScriptScanPassed, "goal128.quality.script_scan",
            "parameterized_gamepackage_projection_runner", diagnostics);
        AddIfFalse(runner.UnitySourceScanPassed, "goal128.quality.unity_source_scan",
            "parameterized_gamepackage_projection_runner", diagnostics);
        AddIfFalse(runner.ResultPassed, "goal128.quality.result",
            "parameterized_gamepackage_projection_runner", diagnostics);
        AddIfFalse(runner.LogPassed, "goal128.quality.log_scan",
            "parameterized_gamepackage_projection_runner", diagnostics);
        AddIfFalse(runner.Goal127Green, "goal128.quality.goal127_green",
            "parameterized_gamepackage_projection_runner", diagnostics);
        AddIfFalse(runner.QualityGatePassed, "goal128.quality.quality_gate",
            "parameterized_gamepackage_projection_runner", diagnostics);
        AddIfFalse(runner.RelativePaths, "goal128.quality.relative_paths",
            "parameterized_gamepackage_projection_runner", diagnostics);
    }

    private static bool Goal128AllowedPath(string path) =>
        path.StartsWith(
            ParameterizedGamePackageProjectionRunnerVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            ParameterizedGamePackageProjectionRunnerVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal128ParameterizedGamePackageRunnerQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal128ParameterizedGamePackageRunnerQuality runner,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            ParameterizedGamePackageRunnerGroupPresent = runner.GroupPresent,
            ParameterizedGamePackageRunnerStatus = runner.RunnerStatus,
            ParameterizedGamePackageRunnerPackagePath = runner.PackagePath,
            ParameterizedGamePackageRunnerPackagePathRelative = runner.PackagePathRelative,
            ParameterizedGamePackageRunnerNormalCommand = runner.NormalCommand,
            ParameterizedGamePackageRunnerExampleCommandWithPackagePath =
                runner.ExampleCommandWithPackagePath,
            ParameterizedGamePackageRunnerResultPath = runner.ResultPath,
            ParameterizedGamePackageRunnerLogPath = runner.LogPath,
            ParameterizedGamePackageRunnerUnityExitCode = runner.UnityExitCode,
            ParameterizedGamePackageRunnerPassMarkerPresent = runner.PassMarkerPresent,
            ParameterizedGamePackageRunnerCleanupApplied = runner.CleanupApplied,
            ParameterizedGamePackageRunnerManualUnityOptional = runner.ManualUnityOptional,
            ParameterizedGamePackageRunnerProjectionOnly = runner.ProjectionOnly,
            ParameterizedGamePackageRunnerEvidencePath = runner.EvidencePath,
            ParameterizedGamePackageRunnerExportPath = runner.ExportPath,
            ParameterizedGamePackageRunnerScriptScanPassed = runner.ScriptScanPassed,
            ParameterizedGamePackageRunnerUnitySourceScanPassed = runner.UnitySourceScanPassed,
            ParameterizedGamePackageRunnerResultPassed = runner.ResultPassed,
            ParameterizedGamePackageRunnerLogPassed = runner.LogPassed,
            ParameterizedGamePackageRunnerGoal127Green = runner.Goal127Green,
            ParameterizedGamePackageRunnerQualityGatePassed = runner.QualityGatePassed,
            Goal128FilesDiscoveredByRelativePaths = runner.RelativePaths,
            WinFormsParameterizedGamePackageRunnerBindingReal =
                binding.PageBindDisplaysParameterizedGamePackageRunner
        };

    private sealed record Goal128ParameterizedGamePackageRunnerQuality(
        bool GroupPresent,
        string RunnerStatus,
        string PackagePath,
        string PackagePathRelative,
        string NormalCommand,
        string ExampleCommandWithPackagePath,
        string ResultPath,
        string LogPath,
        int UnityExitCode,
        bool PassMarkerPresent,
        bool CleanupApplied,
        bool ManualUnityOptional,
        bool ProjectionOnly,
        string EvidencePath,
        string ExportPath,
        bool ScriptScanPassed,
        bool UnitySourceScanPassed,
        bool ResultPassed,
        bool LogPassed,
        bool Goal127Green,
        bool QualityGatePassed,
        bool RelativePaths);
}
