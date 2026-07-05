using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaHumanResultRevalidation;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualGateAcceptanceRecord;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaPostAcceptanceContinuationSelection;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceExportPackage;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceOrchestrator;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAcceptedAlphaBaselineReview;

public sealed partial class OfflineGeoworldAcceptedAlphaBaselineReviewService
{
    private const string Goal114Root =
        ".llmgc/procedural/goal-114-unity-safe-mode-compile-hotfix";
    private const string Goal114DashboardFileName =
        "unity-safe-mode-compile-hotfix-dashboard.json";

    private static OfflineGeoworldAcceptedAlphaBaselineDashboard BuildDashboard(
        string root,
        OfflineGeoworldAcceptedAlphaBaselineSourceIndex sourceIndex)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var goal116 = LoadGoal116Evidence(root, errors);
        var goal117 = LoadGoal117Evidence(root, errors);
        var goal114 = LoadGoal114Evidence(root, errors);
        var goal109 = LoadGoal109Evidence(root, errors);
        var goal108 = LoadGoal108Evidence(root, errors);

        var acceptedRoots = BuildAcceptedEvidenceRoots(sourceIndex);
        var producedOnlyRoots = sourceIndex.Entries
            .Where(entry => entry.Classification != "accepted_baseline_proof")
            .Select(entry => entry.RelativeRoot)
            .ToList();
        var notes = BuildBlockedOrSupersededNotes();

        ValidateGoal116Evidence(goal116, errors);
        ValidateGoal117Evidence(goal117, errors);
        ValidateGoal114Evidence(goal114, errors);
        ValidateGoal109Evidence(goal109, errors);
        ValidateGoal108Evidence(goal108, errors);

        var baselineReady = errors.Count == 0;
        var baselineHash = ComputeBaselineHash(
            goal116.ManualGateStatus,
            goal116.ManualResultSha256,
            sourceIndex,
            acceptedRoots,
            notes);

        return new OfflineGeoworldAcceptedAlphaBaselineDashboard
        {
            BaselineHash = baselineHash,
            ManualGateStatus = goal116.ManualGateStatus,
            AcceptedByCodex = goal116.AcceptedByCodex,
            AcceptedBaselineReady = baselineReady,
            ManualResultSha256 = goal116.ManualResultSha256,
            IncludedSourceGoalCount = sourceIndex.IncludedSourceGoalCount,
            AcceptedEvidenceRoots = acceptedRoots,
            ProducedOnlyHistoricalRoots = producedOnlyRoots,
            BlockedOrSupersededNotes = notes,
            AcceptedEvidenceRootCount = acceptedRoots.Count,
            ProducedOnlyRootCount = producedOnlyRoots.Count,
            BlockedOrSupersededNoteCount = notes.Count,
            Goal116AcceptanceRecordPresent = goal116.AcceptanceRecordPresent,
            Goal116AcceptanceRecordValid = goal116.Valid,
            Goal117ContinuationSelectionPresent = goal117.DashboardPresent && goal117.MatrixPresent,
            Goal117ContinuationSelectionValid = goal117.Valid,
            Goal114UnitySafeModeCompileHotfixEvidencePresent = goal114.Present,
            Goal109PortableExportEvidencePresent = goal109.Present,
            Goal108AlphaSliceOrchestratorEvidencePresent = goal108.Present,
            Errors = errors.Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList(),
            Warnings = warnings.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldAcceptedAlphaBaselineSourceIndex BuildSourceIndex(string root)
    {
        var ids = OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.SourceGoalIds;
        var roots = OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.SourceGoalRoots;
        var entries = ids.Zip(roots, (id, relativeRoot) =>
            new OfflineGeoworldAcceptedAlphaBaselineSourceIndexEntry
            {
                SourceGoalId = id,
                RelativeRoot = relativeRoot,
                Classification = AcceptedBaselineRootIds().Contains(id)
                    ? "accepted_baseline_proof"
                    : "produced_only_historical",
                Present = Directory.Exists(Resolve(root, relativeRoot)),
                RequiredForAcceptedBaseline = RequiredBaselineRootIds().Contains(id)
            })
            .ToList();

        return new OfflineGeoworldAcceptedAlphaBaselineSourceIndex
        {
            IncludedSourceGoalCount = entries.Count,
            Goal098To117ChainIncluded = entries.Count == ids.Count
                                     && entries.All(entry => entry.Present),
            Entries = entries
        };
    }

    private static Goal116Evidence LoadGoal116Evidence(string root, List<string> errors)
    {
        var record = LoadJson(
            root,
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory
            + "/"
            + OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.AcceptanceRecordFileName,
            "goal118.goal116_record_missing",
            "goal118.goal116_record_malformed",
            errors);
        var dashboard = LoadJson(
            root,
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory
            + "/"
            + OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.DashboardFileName,
            "goal118.goal116_dashboard_missing",
            "goal118.goal116_dashboard_malformed",
            errors);
        var quality = LoadJson(
            root,
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory
            + "/"
            + OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.QualityGateScanFileName,
            "goal118.goal116_quality_missing",
            "goal118.goal116_quality_malformed",
            errors);

        var valid = record is not null
                    && dashboard is not null
                    && quality is not null
                    && StringProperty(record, "manualGateStatus")
                    == OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ManualGateStatusAccepted
                    && BoolProperty(record, "humanAccepted")
                    && !BoolProperty(record, "acceptedByCodex")
                    && BoolProperty(record, "manualInputNotCommitted")
                    && !BoolProperty(record, "rawManualResultEmbeddedInArtifacts")
                    && BoolProperty(quality, "passed");
        return new Goal116Evidence(
            AcceptanceRecordPresent: record is not null,
            ManualGate: StringProperty(record, "manualGate"),
            ManualGateStatus: StringProperty(record, "manualGateStatus"),
            HumanAccepted: BoolProperty(record, "humanAccepted"),
            ManualResultSha256: StringProperty(record, "manualResultSha256"),
            AcceptedByCodex: BoolProperty(record, "acceptedByCodex"),
            ManualInputNotCommitted: BoolProperty(record, "manualInputNotCommitted"),
            RawManualResultEmbeddedInArtifacts:
                BoolProperty(record, "rawManualResultEmbeddedInArtifacts"),
            RecommendedNextDecision: StringProperty(dashboard, "recommendedNextDecision"),
            QualityPassed: BoolProperty(quality, "passed"),
            Valid: valid);
    }

    private static Goal117Evidence LoadGoal117Evidence(string root, List<string> errors)
    {
        var dashboard = LoadJson(
            root,
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .ProceduralOutputDirectory
            + "/"
            + OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.DashboardFileName,
            "goal118.goal117_dashboard_missing",
            "goal118.goal117_dashboard_malformed",
            errors);
        var matrix = LoadJson(
            root,
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .ProceduralOutputDirectory
            + "/"
            + OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.MatrixFileName,
            "goal118.goal117_matrix_missing",
            "goal118.goal117_matrix_malformed",
            errors);
        var quality = LoadJson(
            root,
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .ProceduralOutputDirectory
            + "/"
            + OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.QualityGateScanFileName,
            "goal118.goal117_quality_missing",
            "goal118.goal117_quality_malformed",
            errors);
        var ready = CountLaneStatus(matrix, "READY");
        var candidates = CountLaneStatus(matrix, "CANDIDATE_");
        var blocked = CountLaneStatus(matrix, "BLOCKED_");
        var valid = dashboard is not null
                    && matrix is not null
                    && quality is not null
                    && StringProperty(dashboard, "recommendedNextLane")
                    == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                        .LaneAcceptedAlphaBaselineReview
                    && StringProperty(dashboard, "recommendedNextGoalId")
                    == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                        .RecommendedNextGoalId
                    && ready == 1
                    && candidates == 3
                    && blocked == 3
                    && BoolProperty(dashboard, "doNotStartAutomatically")
                    && BoolProperty(quality, "passed");
        return new Goal117Evidence(
            DashboardPresent: dashboard is not null,
            MatrixPresent: matrix is not null,
            RecommendedNextLane: StringProperty(dashboard, "recommendedNextLane"),
            RecommendedNextGoalId: StringProperty(dashboard, "recommendedNextGoalId"),
            ReadyLaneCount: ready,
            CandidateLaneCount: candidates,
            BlockedLaneCount: blocked,
            DoNotStartAutomatically: BoolProperty(dashboard, "doNotStartAutomatically"),
            QualityPassed: BoolProperty(quality, "passed"),
            Valid: valid);
    }

    private static BasicEvidence LoadGoal114Evidence(string root, List<string> errors)
    {
        var dashboard = LoadJson(
            root,
            Goal114Root + "/" + Goal114DashboardFileName,
            "goal118.goal114_dashboard_missing",
            "goal118.goal114_dashboard_malformed",
            errors);
        return new BasicEvidence(
            Present: dashboard is not null,
            ImplementationStatus: StringProperty(dashboard, "implementationStatus"),
            QualityPassed: BoolProperty(dashboard, "sourceScanPassed")
                           && BoolProperty(dashboard, "negativeProofPassed"));
    }

    private static BasicEvidence LoadGoal109Evidence(string root, List<string> errors)
    {
        var manifest = LoadJson(
            root,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.ProceduralOutputDirectory
            + "/"
            + OfflineGeoworldAlphaSliceExportPackageVocabulary.ManifestFileName,
            "goal118.goal109_manifest_missing",
            "goal118.goal109_manifest_malformed",
            errors);
        return new BasicEvidence(
            Present: manifest is not null,
            ImplementationStatus: StringProperty(manifest, "implementationStatus"),
            QualityPassed: !string.IsNullOrWhiteSpace(StringProperty(manifest, "exportPackageRoot"))
                           && BoolProperty(manifest, "alphaRuntimeBootstrapUnchanged"));
    }

    private static BasicEvidence LoadGoal108Evidence(string root, List<string> errors)
    {
        var manifest = LoadJson(
            root,
            OfflineGeoworldAlphaSliceVocabulary.RelativeOutputDirectory
            + "/"
            + OfflineGeoworldAlphaSliceVocabulary.ManifestFileName,
            "goal118.goal108_manifest_missing",
            "goal118.goal108_manifest_malformed",
            errors);
        return new BasicEvidence(
            Present: manifest is not null,
            ImplementationStatus: StringProperty(manifest, "implementationStatus"),
            QualityPassed: BoolProperty(manifest, "alphaRuntimeBootstrapUnchanged")
                           && !BoolProperty(manifest, "containsProviderCalls")
                           && !BoolProperty(manifest, "containsRuntimeExecution"));
    }

    private static void ValidateGoal116Evidence(Goal116Evidence evidence, List<string> errors)
    {
        Require(evidence.AcceptanceRecordPresent, "goal118.goal116_acceptance_record_present",
            errors);
        Require(
            evidence.ManualGate
            == OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ManualGate,
            "goal118.goal116_manual_gate_unexpected",
            errors);
        Require(
            evidence.ManualGateStatus
            == OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ManualGateStatusAccepted,
            "goal118.goal116_manual_gate_not_accepted",
            errors);
        Require(evidence.HumanAccepted, "goal118.goal116_human_accepted_false", errors);
        Require(
            evidence.ManualResultSha256
            == OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ExpectedManualResultSha256,
            "goal118.goal116_manual_sha_unexpected",
            errors);
        Require(!evidence.AcceptedByCodex, "goal118.goal116_accepted_by_codex_true", errors);
        Require(evidence.ManualInputNotCommitted, "goal118.goal116_manual_input_committed",
            errors);
        Require(!evidence.RawManualResultEmbeddedInArtifacts,
            "goal118.goal116_raw_manual_embedded", errors);
        Require(evidence.QualityPassed, "goal118.goal116_quality_not_passed", errors);
    }

    private static void ValidateGoal117Evidence(Goal117Evidence evidence, List<string> errors)
    {
        Require(evidence.DashboardPresent, "goal118.goal117_dashboard_present", errors);
        Require(evidence.MatrixPresent, "goal118.goal117_matrix_present", errors);
        Require(
            evidence.RecommendedNextLane
            == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .LaneAcceptedAlphaBaselineReview,
            "goal118.goal117_recommended_lane_unexpected",
            errors);
        Require(
            evidence.RecommendedNextGoalId
            == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .RecommendedNextGoalId,
            "goal118.goal117_recommended_goal_unexpected",
            errors);
        Require(evidence.ReadyLaneCount == 1, "goal118.goal117_ready_lane_count", errors);
        Require(evidence.CandidateLaneCount == 3, "goal118.goal117_candidate_lane_count",
            errors);
        Require(evidence.BlockedLaneCount == 3, "goal118.goal117_blocked_lane_count", errors);
        Require(evidence.DoNotStartAutomatically, "goal118.goal117_do_not_start", errors);
        Require(evidence.QualityPassed, "goal118.goal117_quality_not_passed", errors);
    }

    private static void ValidateGoal114Evidence(BasicEvidence evidence, List<string> errors)
    {
        Require(evidence.Present, "goal118.goal114_evidence_present", errors);
        Require(evidence.ImplementationStatus == "GREEN", "goal118.goal114_not_green", errors);
        Require(evidence.QualityPassed, "goal118.goal114_quality_not_passed", errors);
    }

    private static void ValidateGoal109Evidence(BasicEvidence evidence, List<string> errors)
    {
        Require(evidence.Present, "goal118.goal109_export_manifest_present", errors);
        Require(evidence.ImplementationStatus == "GREEN", "goal118.goal109_not_green", errors);
        Require(evidence.QualityPassed, "goal118.goal109_quality_not_passed", errors);
    }

    private static void ValidateGoal108Evidence(BasicEvidence evidence, List<string> errors)
    {
        Require(evidence.Present, "goal118.goal108_alpha_manifest_present", errors);
        Require(evidence.ImplementationStatus == "GREEN", "goal118.goal108_not_green", errors);
        Require(evidence.QualityPassed, "goal118.goal108_quality_not_passed", errors);
    }

    private static JsonElement? LoadJson(
        string root,
        string relativePath,
        string missingCode,
        string malformedCode,
        List<string> errors)
    {
        var path = Resolve(root, relativePath);
        if (!File.Exists(path))
        {
            errors.Add(missingCode);
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            errors.Add(malformedCode);
            return null;
        }
    }

    private static string StringProperty(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static bool BoolProperty(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        && property.GetBoolean();

    private static int CountLaneStatus(JsonElement? element, string statusPrefix)
    {
        if (element is null
            || !element.Value.TryGetProperty("lanes", out var lanes)
            || lanes.ValueKind != JsonValueKind.Array)
        {
            return 0;
        }

        return lanes.EnumerateArray()
            .Select(lane => StringProperty(lane, "status"))
            .Count(status => status.StartsWith(statusPrefix, StringComparison.Ordinal));
    }

    private static string StringProperty(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static IReadOnlySet<string> AcceptedBaselineRootIds() =>
        new HashSet<string>(
        [
            OfflineGeoworldAlphaSliceVocabulary.GoalId,
            OfflineGeoworldAlphaSliceExportPackageVocabulary.GoalId,
            "goal_114_unity_safe_mode_compile_hotfix",
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.GoalId,
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.GoalId,
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.GoalId
        ], StringComparer.Ordinal);

    private static IReadOnlySet<string> RequiredBaselineRootIds() => AcceptedBaselineRootIds();

    private static IReadOnlyList<string> BuildAcceptedEvidenceRoots(
        OfflineGeoworldAcceptedAlphaBaselineSourceIndex sourceIndex) =>
        sourceIndex.Entries
            .Where(entry => entry.Classification == "accepted_baseline_proof")
            .Select(entry => entry.RelativeRoot)
            .ToList();

    private static IReadOnlyList<string> BuildBlockedOrSupersededNotes() =>
    [
        "Goal102A source-format guard is superseded by Goal102B actual-source trust audit.",
        "Goal102B remains BLOCKED as historical evidence because actual target bytes were already readable.",
        "Goal108A is included only as source split and immutability audit evidence.",
        "The accepted baseline is not final release packaging.",
        "Runtime, public schema, Lua and generator-library consumers remain separate future gates.",
        "Live geodata, provider calls, network fetching and legal/provider review remain blocked.",
        "Unity scene, prefab, ProjectSettings, Packages and StreamingAssets changes are not authorized by Goal118.",
        "Final renderer, final art and atlas work require a separate explicit lane decision."
    ];

    private static string ComputeBaselineHash(
        string manualGateStatus,
        string manualResultSha256,
        OfflineGeoworldAcceptedAlphaBaselineSourceIndex sourceIndex,
        IReadOnlyList<string> acceptedRoots,
        IReadOnlyList<string> notes)
    {
        var lines = new List<string>
        {
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.BaselineId,
            manualGateStatus,
            manualResultSha256,
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.SourceGoalRange,
            sourceIndex.IncludedSourceGoalCount.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
        lines.AddRange(acceptedRoots.OrderBy(item => item, StringComparer.Ordinal));
        lines.AddRange(notes.OrderBy(item => item, StringComparer.Ordinal));
        return HashText(string.Join("\n", lines));
    }

    private sealed record Goal116Evidence(
        bool AcceptanceRecordPresent,
        string ManualGate,
        string ManualGateStatus,
        bool HumanAccepted,
        string ManualResultSha256,
        bool AcceptedByCodex,
        bool ManualInputNotCommitted,
        bool RawManualResultEmbeddedInArtifacts,
        string RecommendedNextDecision,
        bool QualityPassed,
        bool Valid);

    private sealed record Goal117Evidence(
        bool DashboardPresent,
        bool MatrixPresent,
        string RecommendedNextLane,
        string RecommendedNextGoalId,
        int ReadyLaneCount,
        int CandidateLaneCount,
        int BlockedLaneCount,
        bool DoNotStartAutomatically,
        bool QualityPassed,
        bool Valid);

    private sealed record BasicEvidence(bool Present, string ImplementationStatus, bool QualityPassed);
}
