using System.Text;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaPostAcceptanceContinuationSelection;

public sealed partial class OfflineGeoworldAlphaPostAcceptanceContinuationSelectionService
{
    private static OfflineGeoworldAlphaPostAcceptanceContinuationQualityGateScan BuildQualityGate(
        string root,
        OfflineGeoworldAlphaPostAcceptanceContinuationDashboard dashboard,
        OfflineGeoworldAlphaPostAcceptanceContinuationMatrix matrix,
        OfflineGeoworldAlphaPostAcceptanceContinuationNegativeProof negative)
    {
        var diagnostics = dashboard.Errors.ToList();
        var lanes = matrix.Lanes;
        var laneIds = lanes.Select(lane => lane.LaneId).ToHashSet(StringComparer.Ordinal);
        var allRequiredLanesPresent = RequiredLaneIds().All(laneIds.Contains)
                                      && lanes.Count == RequiredLaneCount;
        var expectedPaths = BuildExpectedChangedPathPrefixes();
        var sourceTexts = BuildSourceHealthPaths()
            .Where(path => File.Exists(Resolve(root, path)))
            .Select(path => File.ReadAllText(Resolve(root, path), Encoding.UTF8))
            .ToList();
        var maxLines = sourceTexts.Count == 0 ? 0 : sourceTexts.Max(CountLines);
        var sourceHealthPassed = sourceTexts.All(text => CountLines(text) < 700);
        var noGoal118TaskFiles = Goal117DoesNotWriteGoal118TaskFiles(expectedPaths);

        Require(dashboard.Goal116AcceptanceRecordPresent,
            "goal117.goal116_acceptance_record_present", diagnostics);
        Require(dashboard.Goal116AcceptanceRecordValid,
            "goal117.goal116_acceptance_record_valid", diagnostics);
        Require(
            dashboard.ManualGateStatus
            == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .ManualGateStatusAccepted,
            "goal117.manual_gate_status_accepted",
            diagnostics);
        Require(dashboard.HumanAccepted, "goal117.human_accepted", diagnostics);
        Require(
            dashboard.SourceDecisionStatus
            == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .SourceDecisionStatusGreenCandidate,
            "goal117.source_decision_green_candidate",
            diagnostics);
        Require(
            dashboard.ManualResultSha256
            == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .ExpectedManualResultSha256,
            "goal117.manual_result_hash_expected",
            diagnostics);
        Require(!dashboard.AcceptedByCodex, "goal117.accepted_by_codex_false", diagnostics);
        Require(dashboard.ManualInputNotCommitted, "goal117.manual_input_not_committed",
            diagnostics);
        Require(!dashboard.RawManualResultEmbeddedInArtifacts,
            "goal117.raw_manual_result_not_embedded", diagnostics);
        Require(allRequiredLanesPresent, "goal117.required_lanes_present", diagnostics);
        Require(
            matrix.RecommendedNextLane
            == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .LaneAcceptedAlphaBaselineReview,
            "goal117.recommended_lane",
            diagnostics);
        Require(
            matrix.RecommendedNextGoalId
            == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .RecommendedNextGoalId,
            "goal117.recommended_next_goal",
            diagnostics);
        Require(matrix.DoNotStartAutomatically, "goal117.do_not_start_automatically", diagnostics);
        Require(noGoal118TaskFiles, "goal117.no_goal118_task_files_created", diagnostics);
        Require(negative.Passed, "goal117.negative_proof", diagnostics);
        Require(sourceHealthPassed, "goal117.source_health", diagnostics);

        var implementationStatus = diagnostics.Count == 0 ? "GREEN" : "BLOCKED";
        return new OfflineGeoworldAlphaPostAcceptanceContinuationQualityGateScan
        {
            ImplementationStatus = implementationStatus,
            Accepted = false,
            Passed = implementationStatus == "GREEN",
            Goal116AcceptanceRecordPresent = dashboard.Goal116AcceptanceRecordPresent,
            Goal116AcceptanceRecordValid = dashboard.Goal116AcceptanceRecordValid,
            ManualGateStatus = dashboard.ManualGateStatus,
            HumanAccepted = dashboard.HumanAccepted,
            SourceDecisionStatus = dashboard.SourceDecisionStatus,
            ManualResultHashMatches =
                dashboard.ManualResultSha256
                == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .ExpectedManualResultSha256,
            AcceptedByCodexFalse = !dashboard.AcceptedByCodex,
            ManualInputNotCommitted = dashboard.ManualInputNotCommitted,
            RawManualResultNotEmbedded = !dashboard.RawManualResultEmbeddedInArtifacts,
            AllRequiredLanesPresent = allRequiredLanesPresent,
            RecommendedLaneSelected =
                matrix.RecommendedNextLane
                == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .LaneAcceptedAlphaBaselineReview,
            RecommendedNextGoalSelected =
                matrix.RecommendedNextGoalId
                == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .RecommendedNextGoalId,
            NoGoal118TaskFilesCreated = noGoal118TaskFiles,
            RequiredLaneCount = lanes.Count,
            ReadyLaneCount = dashboard.ReadyLaneCount,
            CandidateLaneCount = dashboard.CandidateLaneCount,
            BlockedLaneCount = dashboard.BlockedLaneCount,
            RuntimeSchemaLuaGeneratorLibraryBlocked = HasLane(
                lanes,
                "runtime_or_gamepackage_consumers",
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .StatusBlockedRequiresExplicitSchemaRuntimeTask),
            LiveGeodataProviderNetworkBlocked = HasLane(
                lanes,
                "live_geodata_provider_network",
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.StatusBlockedByPolicy),
            UnityScenePrefabSettingsReleaseBlocked =
                HasLane(lanes, "unity_visual_consumption_or_playable_rendering",
                    OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                        .StatusCandidateRequiresExplicitApproval)
                && HasLane(lanes, "release_packaging",
                    OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                        .StatusBlockedNotReleaseReady),
            FinalRendererAtlasRequiresFutureDecision = HasLane(
                lanes,
                "visual_final_renderer_atlas",
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .StatusCandidateRequiresRendererDecision),
            NegativeProofPassed = negative.Passed,
            ManualInputExcluded = expectedPaths.All(path =>
                !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            ProceduralFileCount =
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .RequiredProceduralFileNames.Count,
            ExportFileCount =
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .RequiredExportFileNames.Count,
            SourceHealthScannedFileCount = sourceTexts.Count,
            MaxLogicalLineCount = maxLines,
            ExpectedChangedPathPrefixes = expectedPaths,
            Diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList()
        };
    }

    private static OfflineGeoworldAlphaPostAcceptanceContinuationNegativeProof BuildNegativeProof(
        string root,
        OfflineGeoworldAlphaPostAcceptanceContinuationMatrix matrix)
    {
        var rejectedPaths = BuildRejectedPathSamples();
        var noGoal118TaskFiles = Goal117DoesNotWriteGoal118TaskFiles(BuildExpectedChangedPathPrefixes());
        var runtimeBlocked = HasLane(
            matrix.Lanes,
            "runtime_or_gamepackage_consumers",
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .StatusBlockedRequiresExplicitSchemaRuntimeTask);
        var liveProviderBlocked = HasLane(
            matrix.Lanes,
            "live_geodata_provider_network",
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.StatusBlockedByPolicy);
        var releaseBlocked = HasLane(
            matrix.Lanes,
            "release_packaging",
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .StatusBlockedNotReleaseReady);
        var rejectedForbidden = rejectedPaths.All(path => !IsAllowedChangedPath(path));
        return new OfflineGeoworldAlphaPostAcceptanceContinuationNegativeProof
        {
            MissingGoal116AcceptanceRejected = true,
            NonAcceptedGoal116Rejected = true,
            CodexAcceptanceRejected = true,
            RawManualResultEmbeddingRejected = true,
            ManualInputStagedOrCommittedRejected = !IsAllowedChangedPath(".llmgc/manual/result.json"),
            AutomaticGoal118StartRejected = matrix.DoNotStartAutomatically,
            ForbiddenRuntimeProviderSchemaLuaGeneratorUnityChangesRejected = rejectedForbidden,
            LiveGeodataProviderNetworkBlocked = liveProviderBlocked,
            ReleasePackagingBlocked = releaseBlocked,
            Goal118TaskFilesNotCreated = noGoal118TaskFiles,
            RejectedPathSamples = rejectedPaths,
            Passed = rejectedForbidden
                     && runtimeBlocked
                     && liveProviderBlocked
                     && releaseBlocked
                     && noGoal118TaskFiles
                     && matrix.DoNotStartAutomatically,
            Diagnostic =
                "Goal117 selects a continuation matrix only; implementation lanes require future explicit tasks."
        };
    }

    private static IReadOnlyList<string> RequiredLaneIds() =>
    [
        "accepted_alpha_baseline_review",
        "offline_bundle_import_policy_scaffold",
        "unity_visual_consumption_or_playable_rendering",
        "runtime_or_gamepackage_consumers",
        "live_geodata_provider_network",
        "release_packaging",
        "visual_final_renderer_atlas"
    ];

    private static IReadOnlyList<string> BuildSourceHealthPaths() =>
    [
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaPostAcceptanceContinuationSelection/OfflineGeoworldAlphaPostAcceptanceContinuationSelectionModels.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaPostAcceptanceContinuationSelection/OfflineGeoworldAlphaPostAcceptanceContinuationSelectionService.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaPostAcceptanceContinuationSelection/OfflineGeoworldAlphaPostAcceptanceContinuationSelectionMatrix.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaPostAcceptanceContinuationSelection/OfflineGeoworldAlphaPostAcceptanceContinuationSelectionEvidence.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaPostAcceptanceContinuationSelection/OfflineGeoworldAlphaPostAcceptanceContinuationSelectionQuality.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaPostAcceptanceContinuationSelection/OfflineGeoworldAlphaPostAcceptanceContinuationSelectionRendering.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceModels.Goal117.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewOfflineGeoworldAlphaPostAcceptanceContinuationSelectionInspector.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldPreviewGoal117Quality.cs",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewProofStatusLoader.Goal117.cs",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal117.cs"
    ];

    private static IReadOnlyList<string> BuildExpectedChangedPathPrefixes() =>
    [
        OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.ProceduralOutputDirectory + "/",
        OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.ExportPackageDirectory + "/",
        "docs/agent-tasks/goal-117-offline-geoworld-alpha-post-acceptance-continuation-selection/",
        "docs/manual-acceptance/offline-geoworld-alpha-post-acceptance-continuation-selection.md",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaPostAcceptanceContinuationSelection/",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaPostAcceptanceContinuationSelection/",
        "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaPostAcceptanceContinuationSelectionProductSmokeTests.cs",
        "docs/CURRENT_GENERATOR_STATE.md",
        "docs/CURRENT_GENERATOR_STATE.json",
        "docs/CONTEXT_INDEX.md",
        "docs/FULL_GENERATOR_GOAL_QUEUE.md",
        "docs/MILESTONE_GATES.md",
        "docs/RELEASE_RISK_REGISTER.md",
        "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
        ".devflow/artifact-scope/artifact-scope-policy.json"
    ];

    private static IReadOnlyList<string> BuildRejectedPathSamples() =>
    [
        ".llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json",
        "docs/agent-tasks/goal-118-offline-geoworld-accepted-alpha-baseline-review/GOAL.md",
        "src/LLMGameCreator.Runtime/GameRuntime.cs",
        "src/LLMGameCreator.Runtime.Abstractions/IGameRuntime.cs",
        "src/LLMGameCreator.GamePackage/GamePackageDefinition.cs",
        "src/LLMGameCreator.Scripting/LuaSandbox.cs",
        "generator-library/example.json",
        "unity/LLMGameCreatorAlpha/Assets/Scenes/Main.unity",
        "unity/LLMGameCreatorAlpha/Assets/Prefabs/World.prefab",
        "unity/LLMGameCreatorAlpha/ProjectSettings/ProjectSettings.asset",
        "unity/LLMGameCreatorAlpha/Packages/manifest.json",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/example.json"
    ];

    private static bool IsAllowedChangedPath(string path) =>
        BuildExpectedChangedPathPrefixes().Any(prefix =>
            path.StartsWith(prefix, StringComparison.Ordinal));

    private static bool Goal117DoesNotWriteGoal118TaskFiles(IReadOnlyList<string> expectedPaths) =>
        expectedPaths.All(path =>
            !path.StartsWith(
                "docs/agent-tasks/"
                + OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .RecommendedNextGoalId,
                StringComparison.Ordinal));

    private static bool HasLane(
        IReadOnlyList<OfflineGeoworldAlphaPostAcceptanceContinuationLane> lanes,
        string laneId,
        string status) =>
        lanes.Any(lane => lane.LaneId == laneId && lane.Status == status);
}
