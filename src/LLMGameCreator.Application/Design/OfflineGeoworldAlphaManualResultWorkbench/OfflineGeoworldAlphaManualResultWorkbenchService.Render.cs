using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultWorkbench;

public sealed partial class OfflineGeoworldAlphaManualResultWorkbenchService
{
    private static string BuildDraftTemplate(
        Goal110Source goal110,
        OfflineGeoworldAlphaManualResultWorkbenchDashboard dashboard)
    {
        var steps = goal110.RequiredSteps
            .OrderBy(step => step.Order)
            .ThenBy(step => step.StepId, StringComparer.Ordinal)
            .Select(step => new
            {
                stepId = step.StepId,
                status = "pending",
                notes = "replace with real human observation from the Unity runner",
                evidenceRef = string.IsNullOrWhiteSpace(step.EvidenceField)
                    ? step.StepId + "Evidence"
                    : step.EvidenceField
            })
            .ToArray();

        return Serialize(new
        {
            draftTemplateOnly = true,
            templateCopyOnly = true,
            notRealHumanResult = true,
            humanMustFillRealResultSeparately = true,
            doNotPlaceUnderManualResultPath = true,
            goalId = dashboard.SourceGoalIds[0],
            workbenchGoalId = dashboard.GoalId,
            manualGate = dashboard.ManualGate,
            resultSchema = goal110.ResultSchema,
            accepted = false,
            acceptedByCodex = false,
            humanAcceptanceStillRequired = true,
            manualAcceptancePending = true,
            resultStatus = "draft_template_only",
            checklistHash = dashboard.ChecklistHash,
            preferredManualResultPath = dashboard.PreferredManualResultPath,
            notes = "Copy this draft only after a human run, fill real evidence, and save the real result at the preferred manual path.",
            steps
        });
    }

    private static OfflineGeoworldAlphaManualResultWorkbenchFieldMap BuildFieldMap(
        Goal110Source goal110)
    {
        var fields = new List<OfflineGeoworldAlphaManualResultWorkbenchFieldMapEntry>
        {
            Field("$.accepted", "Set true only after the human completes every required checklist step.", "Goal110 result template"),
            Field("$.operatorNotes", "Summarize real manual observations from the Unity runner.", "Goal110 result template"),
            Field("$.checklistHash", "Keep the Goal110 checklist hash unchanged.", "Goal110 checklist"),
            Field("$.steps[*].status", "Use passed only for real completed steps; failed/pending/skipped are not acceptance.", "Goal110 checklist"),
            Field("$.steps[*].notes", "Record concise real human notes for each step.", "Goal110 checklist"),
            Field("$.steps[*].evidenceRef", "Point to the matching real evidence field or artifact note.", "Goal110 checklist")
        };
        fields.AddRange(goal110.RequiredSteps.Select(step =>
            Field(
                "$.steps[?(@.stepId=='" + step.StepId + "')]",
                "Complete and record real evidence for: " + step.Title,
                "Goal110 checklist step " + step.Order)));

        return new OfflineGeoworldAlphaManualResultWorkbenchFieldMap
        {
            Fields = fields
        };
    }

    private static OfflineGeoworldAlphaManualResultWorkbenchFieldMapEntry Field(
        string path,
        string action,
        string source) =>
        new()
        {
            JsonPath = path,
            RequiredHumanAction = action,
            Source = source
        };

    private static OfflineGeoworldAlphaManualResultWorkbenchNegativeProof BuildInvalidResultProof(
        Goal110Source goal110)
    {
        var validation = ValidateResultText(
            goal110,
            "offline-geoworld-alpha-acceptance-result.json",
            "{ invalid json");
        return new OfflineGeoworldAlphaManualResultWorkbenchNegativeProof
        {
            ScenarioId = "malformed_manual_result_json_does_not_accept_alpha",
            Passed = validation.ManualResultPresent
                     && validation.Errors.Any(error =>
                         error.Contains("malformed", StringComparison.OrdinalIgnoreCase))
                     && !validation.ReadyForHumanReview
                     && !validation.AcceptedByCodex
                     && validation.HumanAcceptanceStillRequired,
            ManualResultPresent = validation.ManualResultPresent,
            AcceptedByCodex = false,
            WorkbenchStatus = OfflineGeoworldAlphaManualResultWorkbenchVocabulary
                .WorkbenchStatusResultInvalid,
            Diagnostic = "Malformed manual result JSON is rejected and cannot become Codex acceptance."
        };
    }

    private static object BuildExportDashboard(
        OfflineGeoworldAlphaManualResultWorkbenchDashboard dashboard,
        OfflineGeoworldAlphaManualResultWorkbenchQualityGateScan quality) =>
        new
        {
            dashboard.GoalId,
            dashboard.ManualGate,
            dashboard.WorkbenchStatus,
            dashboard.Goal111DecisionStatus,
            dashboard.Goal112OperatorStatus,
            dashboard.ManualResultPresent,
            dashboard.PreferredManualResultPath,
            dashboard.DraftTemplatePath,
            dashboard.AcceptedByCodex,
            dashboard.HumanAcceptanceStillRequired,
            dashboard.DoesNotWritePreferredManualResultPath,
            dashboard.DraftTemplateOnly,
            dashboard.NotFinalReleaseOrRuntimeBuild,
            dashboard.NoRuntimeProviderOrNetworkChanges,
            dashboard.NoUnityFileChangesRequired,
            dashboard.ChecklistStepCount,
            qualityGatePassed = quality.Passed
        };

    private static string RenderReport(
        OfflineGeoworldAlphaManualResultWorkbenchDashboard dashboard,
        OfflineGeoworldAlphaManualResultWorkbenchQualityGateScan quality)
    {
        var lines = new List<string>
        {
            "# Goal 113 Offline Geoworld Alpha Manual Result Workbench Report",
            string.Empty,
            "- implementationStatus: " + quality.ImplementationStatus,
            "- accepted: false",
            "- manualGate: " + dashboard.ManualGate + " required",
            "- workbenchStatus: " + dashboard.WorkbenchStatus,
            "- goal111DecisionStatus: " + dashboard.Goal111DecisionStatus,
            "- goal112OperatorStatus: " + dashboard.Goal112OperatorStatus,
            "- manualResultPresent: " + dashboard.ManualResultPresent.ToString().ToLowerInvariant(),
            "- acceptedByCodex: false",
            "- humanAcceptanceStillRequired: true",
            "- preferredManualResultPath: " + dashboard.PreferredManualResultPath,
            "- draftTemplatePath: " + dashboard.DraftTemplatePath,
            "- doesNotWritePreferredManualResultPath: true",
            "- draftTemplateOnly: true",
            "- notFinalReleaseOrRuntimeBuild: true",
            "- noRuntimeProviderOrNetworkChanges: true",
            "- noUnityFileChangesRequired: true",
            "- checklistHash: " + dashboard.ChecklistHash,
            "- checklistStepCount: " + dashboard.ChecklistStepCount,
            "- qualityGatePassed: " + quality.Passed.ToString().ToLowerInvariant(),
            string.Empty,
            "## Validation",
            string.Empty,
            "- realManualResultPath: "
            + (string.IsNullOrWhiteSpace(dashboard.RealManualResultPath)
                ? "(none)"
                : dashboard.RealManualResultPath),
            "- readyForHumanReview: "
            + dashboard.Validation.ReadyForHumanReview.ToString().ToLowerInvariant(),
            "- passedSteps: " + dashboard.Validation.StepSummary.PassedCount,
            "- failedSteps: " + dashboard.Validation.StepSummary.FailedCount,
            "- pendingSteps: " + dashboard.Validation.StepSummary.PendingCount,
            "- skippedSteps: " + dashboard.Validation.StepSummary.SkippedCount,
            "- missingSteps: " + dashboard.Validation.StepSummary.MissingCount,
            "- duplicateSteps: " + dashboard.Validation.StepSummary.DuplicateCount,
            "- unknownSteps: " + dashboard.Validation.StepSummary.UnknownCount,
            "- invalidStatusSteps: " + dashboard.Validation.StepSummary.InvalidStatusCount,
            string.Empty,
            "## Required Steps",
            string.Empty
        };
        lines.AddRange(dashboard.RequiredSteps.Select(step =>
            "- " + step.Order + ". " + step.StepId + " - " + step.Title));
        lines.Add(string.Empty);
        lines.Add("## Next Human Actions");
        lines.Add(string.Empty);
        lines.AddRange(dashboard.NextHumanActions.Select(action => "- " + action));
        lines.Add(string.Empty);
        lines.Add("## Do Not Start Yet");
        lines.Add(string.Empty);
        lines.AddRange(dashboard.DoNotStartYet.Select(item => "- " + item));
        AddDiagnostics(lines, "Errors", dashboard.Errors);
        AddDiagnostics(lines, "Warnings", dashboard.Warnings);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderRunbook(OfflineGeoworldAlphaManualResultWorkbenchDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Goal 113 Offline Geoworld Alpha Manual Result Workbench Runbook",
            string.Empty,
            "This workbench is authoring and review tooling only. It does not accept the Alpha gate and does not create the real manual result.",
            string.Empty,
            "## Active Gate",
            string.Empty,
            "- manualGate: " + dashboard.ManualGate,
            "- workbenchStatus: " + dashboard.WorkbenchStatus,
            "- goal111DecisionStatus: " + dashboard.Goal111DecisionStatus,
            "- goal112OperatorStatus: " + dashboard.Goal112OperatorStatus,
            "- acceptedByCodex: false",
            "- humanAcceptanceStillRequired: true",
            string.Empty,
            "## Result Paths",
            string.Empty,
            "- preferredManualResultPath: " + dashboard.PreferredManualResultPath,
            "- draftTemplatePath: " + dashboard.DraftTemplatePath,
            "- draftTemplateOnly: true",
            "- doesNotWritePreferredManualResultPath: true",
            "- candidateManualResultPaths:"
        };
        lines.AddRange(dashboard.CandidateManualResultPaths.Select(path => "  - " + path));
        lines.AddRange(
        [
            string.Empty,
            "## Checklist",
            string.Empty,
            "- checklistHash: " + dashboard.ChecklistHash,
            "- checklistStepCount: " + dashboard.ChecklistStepCount
        ]);
        lines.AddRange(dashboard.RequiredSteps.Select(step =>
            "- " + step.Order + ". " + step.StepId + " - " + step.Title));
        lines.AddRange(
        [
            string.Empty,
            "## Human Run Rules",
            string.Empty,
            "- Copy the Goal113 draft only as a starting point.",
            "- Save the real manually-created JSON only at the preferred manual result path.",
            "- Every required step must appear exactly once with status passed.",
            "- Duplicate, missing, unknown, failed, pending, skipped or malformed steps are rejected.",
            "- A valid candidate still requires explicit human gate acceptance.",
            string.Empty,
            "## Do Not Start Yet",
            string.Empty
        ]);
        lines.AddRange(dashboard.DoNotStartYet.Select(item => "- " + item));
        lines.AddRange(
        [
            string.Empty,
            "## Next Human Actions",
            string.Empty
        ]);
        lines.AddRange(dashboard.NextHumanActions.Select(action => "- " + action));
        lines.Add(string.Empty);
        return string.Join(Environment.NewLine, lines);
    }

    private static string RenderDocumentation(
        OfflineGeoworldAlphaManualResultWorkbenchDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Offline Geoworld Alpha Manual Result Workbench",
            string.Empty,
            "Goal113 makes the open manual-result step practical, but it does not accept the Alpha gate.",
            string.Empty,
            "## Open The Unity Runner",
            string.Empty,
            "- Open `unity/LLMGameCreatorAlpha` in Unity.",
            "- Open `LLMGameCreator/Offline Geoworld Alpha Acceptance Runner`.",
            "- Run every Goal110 checklist step and record real observations.",
            string.Empty,
            "## Result JSON",
            string.Empty,
            "- Preferred real result path: `" + dashboard.PreferredManualResultPath + "`.",
            "- Draft template path: `" + dashboard.DraftTemplatePath + "`.",
            "- The draft is safe to copy from, but it is not acceptance evidence.",
            "- Do not commit a real `.llmgc/manual/**` result from Codex.",
            string.Empty,
            "## Re-run Validation",
            string.Empty,
            "- Re-run Goal111 manual result intake after placing a real result.",
            "- Re-run Goal112 operator pack to refresh readiness visibility.",
            "- Re-run Goal113 workbench to inspect validation and next actions.",
            string.Empty,
            "## Do Not Start Before Human Acceptance",
            string.Empty
        };
        lines.AddRange(dashboard.DoNotStartYet.Select(item => "- " + item));
        lines.Add(string.Empty);
        return string.Join(Environment.NewLine, lines);
    }

    private static string RenderExportReadme(
        OfflineGeoworldAlphaManualResultWorkbenchDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Goal 113 Offline Geoworld Alpha Manual Result Workbench",
            string.Empty,
            "This compact export summarizes the manual-result workbench for the open offline geoworld Alpha manual gate.",
            string.Empty,
            "- workbenchStatus: " + dashboard.WorkbenchStatus,
            "- manualResultPresent: " + dashboard.ManualResultPresent.ToString().ToLowerInvariant(),
            "- acceptedByCodex: false",
            "- humanAcceptanceStillRequired: true",
            "- preferredManualResultPath: " + dashboard.PreferredManualResultPath,
            "- draftTemplatePath: " + dashboard.DraftTemplatePath,
            string.Empty,
            "The human gate remains open until a real manual result is supplied and explicitly accepted by the user.",
            string.Empty
        };
        return string.Join(Environment.NewLine, lines);
    }

    private static void AddDiagnostics(
        List<string> lines,
        string title,
        IReadOnlyList<string> diagnostics)
    {
        if (diagnostics.Count == 0)
        {
            return;
        }

        lines.Add(string.Empty);
        lines.Add("## " + title);
        lines.Add(string.Empty);
        lines.AddRange(diagnostics.Select(item => "- " + item));
    }
}
