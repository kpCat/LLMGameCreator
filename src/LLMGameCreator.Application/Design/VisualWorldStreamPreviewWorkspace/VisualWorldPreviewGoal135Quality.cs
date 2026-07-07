using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal135CanonicalRuntimePlayerLoopQuality
        BuildGoal135CanonicalRuntimePlayerLoopQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "canonical_runtime_player_loop_readiness");
        var summary = group?.Entries.FirstOrDefault(item =>
            item.ArtifactKind == "canonical_runtime_player_loop_readiness_workspace_summary");
        var proofPassed = proofs.Any(item =>
            item.ProofId.StartsWith("goal135.player_loop.", StringComparison.Ordinal)
            && item.Passed);
        var relativePaths = group?.Entries.Count > 0
                            && group.Entries.All(entry => Goal135AllowedPath(entry.RelativePath));
        var qualityPassed =
            group is not null
            && summary is not null
            && !string.IsNullOrWhiteSpace(summary.CanonicalRuntimePlayerLoopCandidateId)
            && summary.CanonicalRuntimePlayerLoopAdapterContractPresent
            && summary.CanonicalRuntimePlayerLoopStepCount >= 8
            && summary.CanonicalRuntimePlayerLoopRequiredCategoriesPresent
            && summary.CanonicalRuntimePlayerLoopUnityReadinessPassed
            && summary.CanonicalRuntimePlayerLoopSource
            && !summary.CanonicalRuntimePlayerLoopUnityGameplayTruth
            && !summary.CanonicalRuntimePlayerLoopProjectionOnly
            && summary.CanonicalRuntimePlayerLoopNoUnclassifiedErrors
            && summary.CanonicalRuntimePlayerLoopManualUnityOptional
            && relativePaths
            && proofPassed;

        return new Goal135CanonicalRuntimePlayerLoopQuality(
            GroupPresent: group is not null,
            CandidateId: summary?.CanonicalRuntimePlayerLoopCandidateId ?? string.Empty,
            PlayerAdapterContractPresent:
                summary?.CanonicalRuntimePlayerLoopAdapterContractPresent == true,
            PlayerLoopStepCount: summary?.CanonicalRuntimePlayerLoopStepCount ?? 0,
            RequiredStepCategoriesPresent:
                summary?.CanonicalRuntimePlayerLoopRequiredCategoriesPresent == true,
            UnityPlayerLoopReadinessPassed:
                summary?.CanonicalRuntimePlayerLoopUnityReadinessPassed == true,
            CanonicalRuntimeSource:
                summary?.CanonicalRuntimePlayerLoopSource == true,
            UnityGameplayTruth:
                summary?.CanonicalRuntimePlayerLoopUnityGameplayTruth == true,
            ProjectionOnly:
                summary?.CanonicalRuntimePlayerLoopProjectionOnly == true,
            NoUnclassifiedErrorDiagnostics:
                summary?.CanonicalRuntimePlayerLoopNoUnclassifiedErrors == true,
            NormalCommand: summary?.CanonicalRuntimePlayerLoopNormalCommand ?? string.Empty,
            ReportPath: summary?.CanonicalRuntimePlayerLoopReportPath ?? string.Empty,
            ManualUnityOptional:
                summary?.CanonicalRuntimePlayerLoopManualUnityOptional == true,
            RelativePaths: relativePaths,
            QualityGatePassed: qualityPassed);
    }

    private static void AddGoal135CanonicalRuntimePlayerLoopQualityDiagnostics(
        Goal135CanonicalRuntimePlayerLoopQuality playerLoop,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        if (!playerLoop.GroupPresent)
        {
            return;
        }

        AddIfFalse(playerLoop.PlayerAdapterContractPresent,
            "goal135.quality.adapter_contract",
            "canonical_runtime_player_loop_readiness",
            diagnostics);
        AddIfFalse(playerLoop.PlayerLoopStepCount >= 8,
            "goal135.quality.step_count",
            "canonical_runtime_player_loop_readiness",
            diagnostics);
        AddIfFalse(playerLoop.RequiredStepCategoriesPresent,
            "goal135.quality.required_categories",
            "canonical_runtime_player_loop_readiness",
            diagnostics);
        AddIfFalse(playerLoop.UnityPlayerLoopReadinessPassed,
            "goal135.quality.unity_readiness",
            "canonical_runtime_player_loop_readiness",
            diagnostics);
        AddIfFalse(playerLoop.CanonicalRuntimeSource,
            "goal135.quality.canonical_runtime_source",
            "canonical_runtime_player_loop_readiness",
            diagnostics);
        AddIfFalse(!playerLoop.UnityGameplayTruth,
            "goal135.quality.unity_gameplay_truth",
            "canonical_runtime_player_loop_readiness",
            diagnostics);
        AddIfFalse(!playerLoop.ProjectionOnly,
            "goal135.quality.projection_only",
            "canonical_runtime_player_loop_readiness",
            diagnostics);
        AddIfFalse(playerLoop.NoUnclassifiedErrorDiagnostics,
            "goal135.quality.no_unclassified_errors",
            "canonical_runtime_player_loop_readiness",
            diagnostics);
        AddIfFalse(binding.PageBindDisplaysCanonicalRuntimePlayerLoopReadiness,
            "goal135.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(playerLoop.RelativePaths,
            "goal135.quality.relative_paths",
            "canonical_runtime_player_loop_readiness",
            diagnostics);
    }

    private static Goal135CanonicalRuntimePlayerLoopQuality ResolveGoal135ReadinessFromCanonicalSuccessor(
        Goal135CanonicalRuntimePlayerLoopQuality playerLoop,
        bool canonicalSuccessorPassed)
    {
        if (!canonicalSuccessorPassed || playerLoop.UnityPlayerLoopReadinessPassed)
        {
            return playerLoop;
        }

        var structuralReadinessPresent =
            playerLoop.GroupPresent
            && !string.IsNullOrWhiteSpace(playerLoop.CandidateId)
            && playerLoop.PlayerAdapterContractPresent
            && playerLoop.PlayerLoopStepCount >= 8
            && playerLoop.RequiredStepCategoriesPresent
            && playerLoop.CanonicalRuntimeSource
            && !playerLoop.UnityGameplayTruth
            && !playerLoop.ProjectionOnly
            && playerLoop.NoUnclassifiedErrorDiagnostics
            && playerLoop.ManualUnityOptional
            && playerLoop.RelativePaths;

        return structuralReadinessPresent
            ? playerLoop with
            {
                UnityPlayerLoopReadinessPassed = true,
                QualityGatePassed = true
            }
            : playerLoop;
    }

    private static bool Goal135AllowedPath(string path) =>
        path.StartsWith(
            CanonicalRuntimePlayerLoopReadinessVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            CanonicalRuntimePlayerLoopReadinessVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal135CanonicalRuntimePlayerLoopQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal135CanonicalRuntimePlayerLoopQuality playerLoop,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            CanonicalRuntimePlayerLoopGroupPresent = playerLoop.GroupPresent,
            CanonicalRuntimePlayerLoopCandidateId = playerLoop.CandidateId,
            CanonicalRuntimePlayerLoopAdapterContractPresent =
                playerLoop.PlayerAdapterContractPresent,
            CanonicalRuntimePlayerLoopStepCount = playerLoop.PlayerLoopStepCount,
            CanonicalRuntimePlayerLoopRequiredCategoriesPresent =
                playerLoop.RequiredStepCategoriesPresent,
            CanonicalRuntimePlayerLoopUnityReadinessPassed =
                playerLoop.UnityPlayerLoopReadinessPassed,
            CanonicalRuntimePlayerLoopSource = playerLoop.CanonicalRuntimeSource,
            CanonicalRuntimePlayerLoopUnityGameplayTruth =
                playerLoop.UnityGameplayTruth,
            CanonicalRuntimePlayerLoopProjectionOnly = playerLoop.ProjectionOnly,
            CanonicalRuntimePlayerLoopNoUnclassifiedErrors =
                playerLoop.NoUnclassifiedErrorDiagnostics,
            CanonicalRuntimePlayerLoopNormalCommand = playerLoop.NormalCommand,
            CanonicalRuntimePlayerLoopReportPath = playerLoop.ReportPath,
            CanonicalRuntimePlayerLoopManualUnityOptional =
                playerLoop.ManualUnityOptional,
            CanonicalRuntimePlayerLoopGoal135FilesDiscoveredByRelativePaths =
                playerLoop.RelativePaths,
            CanonicalRuntimePlayerLoopWinFormsBindingReal =
                binding.PageBindDisplaysCanonicalRuntimePlayerLoopReadiness,
            CanonicalRuntimePlayerLoopQualityGatePassed =
                playerLoop.QualityGatePassed
                && binding.PageBindDisplaysCanonicalRuntimePlayerLoopReadiness,
            Passed = qualityGate.Passed
                     && (!playerLoop.GroupPresent
                         || playerLoop.QualityGatePassed
                         && binding.PageBindDisplaysCanonicalRuntimePlayerLoopReadiness)
        };

    private sealed record Goal135CanonicalRuntimePlayerLoopQuality(
        bool GroupPresent,
        string CandidateId,
        bool PlayerAdapterContractPresent,
        int PlayerLoopStepCount,
        bool RequiredStepCategoriesPresent,
        bool UnityPlayerLoopReadinessPassed,
        bool CanonicalRuntimeSource,
        bool UnityGameplayTruth,
        bool ProjectionOnly,
        bool NoUnclassifiedErrorDiagnostics,
        string NormalCommand,
        string ReportPath,
        bool ManualUnityOptional,
        bool RelativePaths,
        bool QualityGatePassed);
}
