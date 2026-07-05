using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaHumanResultRevalidation;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualGateAcceptanceRecord;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaPostAcceptanceContinuationSelection;

public sealed partial class OfflineGeoworldAlphaPostAcceptanceContinuationSelectionService
{
    private static Goal116Evidence LoadGoal116Evidence(string root, List<string> errors)
    {
        var record = LoadJson(
            root,
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory
            + "/"
            + OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.AcceptanceRecordFileName,
            "goal117.goal116_record_missing",
            "goal117.goal116_record_malformed",
            errors);
        var dashboard = LoadJson(
            root,
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory
            + "/"
            + OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.DashboardFileName,
            "goal117.goal116_dashboard_missing",
            "goal117.goal116_dashboard_malformed",
            errors);
        var quality = LoadJson(
            root,
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory
            + "/"
            + OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.QualityGateScanFileName,
            "goal117.goal116_quality_missing",
            "goal117.goal116_quality_malformed",
            errors);

        return new Goal116Evidence(
            AcceptanceRecordPresent: record is not null,
            DashboardPresent: dashboard is not null,
            QualityPresent: quality is not null,
            ManualGate: StringProperty(record, "manualGate"),
            ManualGateStatus: StringProperty(record, "manualGateStatus"),
            HumanAccepted: BoolProperty(record, "humanAccepted"),
            SourceDecisionStatus: StringProperty(record, "sourceDecisionStatus"),
            ManualResultSha256: StringProperty(record, "manualResultSha256"),
            AcceptedByCodex: BoolProperty(record, "acceptedByCodex"),
            ManualInputNotCommitted: BoolProperty(record, "manualInputNotCommitted"),
            RawManualResultEmbeddedInArtifacts:
                BoolProperty(record, "rawManualResultEmbeddedInArtifacts"),
            DashboardRecommendedNextDecision:
                StringProperty(dashboard, "recommendedNextDecision"),
            QualityPassed: BoolProperty(quality, "passed"),
            QualityAccepted: BoolProperty(quality, "accepted"));
    }

    private static Goal115DecisionSnapshotEvidence LoadGoal115DecisionSnapshot(
        string root,
        List<string> errors)
    {
        var snapshot = LoadJson(
            root,
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ProceduralOutputDirectory
            + "/"
            + OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionSnapshotFileName,
            "goal117.goal115_snapshot_missing",
            "goal117.goal115_snapshot_malformed",
            errors);
        return new Goal115DecisionSnapshotEvidence(
            Present: snapshot is not null,
            DecisionStatus: StringProperty(snapshot, "decisionStatus"),
            ManualResultSha256: StringProperty(snapshot, "manualResultSha256"),
            AcceptedByCodex: BoolProperty(snapshot, "acceptedByCodex"),
            RequiredStepCount: NestedIntProperty(snapshot, "stepSummary", "requiredStepCount"),
            PassedStepCount: NestedIntProperty(snapshot, "stepSummary", "passedCount"));
    }

    private static bool ValidateGoal116Evidence(Goal116Evidence evidence, List<string> errors)
    {
        Require(evidence.AcceptanceRecordPresent, "goal117.goal116_record_present", errors);
        Require(evidence.DashboardPresent, "goal117.goal116_dashboard_present", errors);
        Require(evidence.QualityPresent, "goal117.goal116_quality_present", errors);
        Require(
            evidence.ManualGate
            == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.ManualGate,
            "goal117.goal116_manual_gate_unexpected",
            errors);
        Require(
            evidence.ManualGateStatus
            == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .ManualGateStatusAccepted,
            "goal117.goal116_manual_gate_not_accepted",
            errors);
        Require(evidence.HumanAccepted, "goal117.goal116_human_accepted_false", errors);
        Require(
            evidence.SourceDecisionStatus
            == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .SourceDecisionStatusGreenCandidate,
            "goal117.goal116_source_decision_not_green",
            errors);
        Require(
            evidence.ManualResultSha256
            == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .ExpectedManualResultSha256,
            "goal117.goal116_manual_sha_unexpected",
            errors);
        Require(!evidence.AcceptedByCodex, "goal117.goal116_accepted_by_codex_true", errors);
        Require(evidence.ManualInputNotCommitted, "goal117.goal116_manual_input_committed", errors);
        Require(!evidence.RawManualResultEmbeddedInArtifacts,
            "goal117.goal116_raw_manual_embedded", errors);
        Require(evidence.QualityPassed, "goal117.goal116_quality_not_passed", errors);
        return errors.Count == 0;
    }

    private static bool ValidateGoal115Snapshot(
        Goal115DecisionSnapshotEvidence evidence,
        List<string> errors)
    {
        Require(evidence.Present, "goal117.goal115_snapshot_present", errors);
        Require(
            evidence.DecisionStatus
            == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .SourceDecisionStatusGreenCandidate,
            "goal117.goal115_decision_not_green",
            errors);
        Require(
            evidence.ManualResultSha256
            == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .ExpectedManualResultSha256,
            "goal117.goal115_manual_sha_unexpected",
            errors);
        Require(!evidence.AcceptedByCodex, "goal117.goal115_accepted_by_codex_true", errors);
        Require(evidence.RequiredStepCount == 12, "goal117.goal115_required_step_count", errors);
        Require(evidence.PassedStepCount == 12, "goal117.goal115_passed_step_count", errors);
        return errors.Count == 0;
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

    private static int NestedIntProperty(
        JsonElement? element,
        string parentName,
        string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(parentName, out var parent)
        && parent.ValueKind == JsonValueKind.Object
        && parent.TryGetProperty(propertyName, out var property)
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private sealed record Goal116Evidence(
        bool AcceptanceRecordPresent,
        bool DashboardPresent,
        bool QualityPresent,
        string ManualGate,
        string ManualGateStatus,
        bool HumanAccepted,
        string SourceDecisionStatus,
        string ManualResultSha256,
        bool AcceptedByCodex,
        bool ManualInputNotCommitted,
        bool RawManualResultEmbeddedInArtifacts,
        string DashboardRecommendedNextDecision,
        bool QualityPassed,
        bool QualityAccepted);

    private sealed record Goal115DecisionSnapshotEvidence(
        bool Present,
        string DecisionStatus,
        string ManualResultSha256,
        bool AcceptedByCodex,
        int RequiredStepCount,
        int PassedStepCount);
}
