namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaPostAcceptanceContinuationSelection;

public sealed partial class OfflineGeoworldAlphaPostAcceptanceContinuationSelectionService
{
    private static IReadOnlyList<OfflineGeoworldAlphaPostAcceptanceContinuationLane>
        BuildMatrixLanes() =>
        [
            new()
            {
                LaneId = OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .LaneAcceptedAlphaBaselineReview,
                Status = OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .StatusReady,
                RecommendedNextGoalId =
                    OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                        .RecommendedNextGoalId,
                IsRecommended = true,
                RequiresExplicitFutureApproval = false,
                Boundaries =
                [
                    "review accepted Goal116 baseline",
                    "no live geodata",
                    "no providers",
                    "no Runtime/schema/Lua/generator-library changes"
                ]
            },
            new()
            {
                LaneId = "offline_bundle_import_policy_scaffold",
                Status = OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .StatusCandidateRequiresExplicitApproval,
                RequiresExplicitFutureApproval = true,
                Boundaries =
                [
                    "no network",
                    "no providers",
                    "no Runtime",
                    "no public schema"
                ]
            },
            new()
            {
                LaneId = "unity_visual_consumption_or_playable_rendering",
                Status = OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .StatusCandidateRequiresExplicitApproval,
                RequiresExplicitFutureApproval = true,
                Boundaries =
                [
                    "no Unity scenes",
                    "no Unity prefabs",
                    "no Unity project settings in Goal117"
                ]
            },
            new()
            {
                LaneId = "runtime_or_gamepackage_consumers",
                Status = OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .StatusBlockedRequiresExplicitSchemaRuntimeTask,
                RequiresExplicitFutureApproval = true,
                Boundaries =
                [
                    "requires explicit schema task",
                    "requires explicit Runtime task",
                    "not authorized by Goal117"
                ]
            },
            new()
            {
                LaneId = "live_geodata_provider_network",
                Status = OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .StatusBlockedByPolicy,
                RequiresExplicitFutureApproval = true,
                Boundaries =
                [
                    "blocked by provider policy",
                    "blocked by live network policy",
                    "requires legal/provider review"
                ]
            },
            new()
            {
                LaneId = "release_packaging",
                Status = OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .StatusBlockedNotReleaseReady,
                RequiresExplicitFutureApproval = true,
                Boundaries =
                [
                    "not release ready",
                    "requires separate release packaging gate"
                ]
            },
            new()
            {
                LaneId = "visual_final_renderer_atlas",
                Status = OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .StatusCandidateRequiresRendererDecision,
                RequiresExplicitFutureApproval = true,
                Boundaries =
                [
                    "requires renderer decision",
                    "requires atlas/output contract decision",
                    "not started by Goal117"
                ]
            }
        ];

    private static OfflineGeoworldAlphaPostAcceptanceContinuationMatrix BuildMatrix(
        OfflineGeoworldAlphaPostAcceptanceContinuationDashboard dashboard)
    {
        var lanes = BuildMatrixLanes();
        return new OfflineGeoworldAlphaPostAcceptanceContinuationMatrix
        {
            ManualGateStatus = dashboard.ManualGateStatus,
            HumanAccepted = dashboard.HumanAccepted,
            LaneCount = lanes.Count,
            Lanes = lanes
        };
    }
}
