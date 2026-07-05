using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaAcceptanceOperatorPack;

public sealed partial class OfflineGeoworldAlphaAcceptanceOperatorPackService
{
    private static string BuildPendingTemplateCopy(
        Goal110Metadata goal110,
        OfflineGeoworldAlphaAcceptanceOperatorDashboard dashboard)
    {
        var steps = ReadPendingTemplateSteps(goal110.ResultTemplateText);
        return Serialize(new
        {
            templateCopyOnly = true,
            sampleOnly = true,
            pendingOnly = true,
            notRealHumanResult = true,
            humanMustFillRealResultSeparately = true,
            doNotPlaceUnderManualResultPath = true,
            goalId = dashboard.SourceGoalIds[0],
            operatorPackGoalId = dashboard.GoalId,
            manualGate = dashboard.ManualGate,
            accepted = false,
            manualAcceptancePending = true,
            resultStatus = "pending_template_copy_only",
            checklistHash = dashboard.ChecklistHash,
            preferredManualResultPath = dashboard.PreferredManualResultPath,
            notes = "This Goal112 artifact is a pending template copy only. It is not a real human result and must not be used as acceptance evidence.",
            steps
        });
    }

    private static IReadOnlyList<object> ReadPendingTemplateSteps(string templateText)
    {
        if (string.IsNullOrWhiteSpace(templateText))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(templateText);
            if (!TryGetArray(document.RootElement, "steps", out var steps))
            {
                return [];
            }

            return steps.EnumerateArray()
                .Select(step => new
                {
                    stepId = StringProperty(step, "stepId"),
                    status = "pending",
                    notes = "human must complete this step in a real manual run",
                    evidenceRef = StringProperty(step, "evidenceRef")
                })
                .Cast<object>()
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string RenderRunbook(
        OfflineGeoworldAlphaAcceptanceOperatorDashboard dashboard,
        OfflineGeoworldAlphaAcceptanceResultPathMap pathMap)
    {
        var lines = new List<string>
        {
            "# Goal 112 Offline Geoworld Alpha Acceptance Operator Runbook",
            string.Empty,
            "This operator pack is readiness tooling only. It does not accept the Alpha slice, does not create a real human result, and is not final release packaging.",
            string.Empty,
            "## Active Gate",
            string.Empty,
            "- manualGate: " + dashboard.ManualGate,
            "- operatorStatus: " + dashboard.OperatorStatus,
            "- goal111DecisionStatus: " + dashboard.DecisionStatusFromGoal111,
            "- acceptedByCodex: false",
            "- humanAcceptanceStillRequired: true",
            string.Empty,
            "## Unity Runner",
            string.Empty,
            "- unityProjectPath: " + pathMap.UnityProjectPath,
            "- runnerWindow: " + pathMap.UnityRunnerPath,
            "- resultModel: " + pathMap.UnityResultModelPath,
            "- resultStore: " + pathMap.UnityResultStorePath,
            string.Empty,
            "## Result Paths",
            string.Empty,
            "- preferredManualResultPath: " + pathMap.PreferredManualResultPath,
            "- alternateCandidatePaths:"
        };
        lines.AddRange(pathMap.CandidateManualResultPaths.Select(path => "  - " + path));
        lines.AddRange(
        [
            string.Empty,
            "## Human Run Rules",
            string.Empty,
            "- A human must run the Goal110 checklist and decide the gate.",
            "- accepted=true is valid only when every required checklist step passed, there are no failed/pending/skipped/missing/malformed/duplicate/unknown steps, and the checklist hash matches Goal110.",
            "- accepted=false remains blocked for acceptance even if the JSON is well-formed.",
            "- failed, pending, skipped, malformed, duplicate, missing, unknown or hash-mismatched steps are not acceptance.",
            "- A pending template copy is not a real manual result.",
            string.Empty,
            "## Do Not Start Yet",
            string.Empty
        ]);
        lines.AddRange(dashboard.DoNotDoYet.Select(item => "- " + item));
        lines.AddRange(
        [
            string.Empty,
            "## Next Human Actions",
            string.Empty
        ]);
        lines.AddRange(dashboard.NextHumanActions.Select(item => "- " + item));
        lines.Add(string.Empty);
        return string.Join(Environment.NewLine, lines);
    }

    private static string RenderDocumentationRunbook(
        OfflineGeoworldAlphaAcceptanceOperatorDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Offline Geoworld Alpha Manual Acceptance Operator Pack",
            string.Empty,
            "Goal112 is acceptance operator tooling and RC readiness visibility only. It does not mean the Alpha is accepted and it does not start final release work.",
            string.Empty,
            "- manualGate: " + dashboard.ManualGate,
            "- operatorStatus: " + dashboard.OperatorStatus,
            "- goal111DecisionStatus: " + dashboard.DecisionStatusFromGoal111,
            "- acceptedByCodex: false",
            "- humanAcceptanceStillRequired: true",
            "- preferredManualResultPath: " + dashboard.PreferredManualResultPath,
            "- fullEvidence: " + OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ProceduralOutputDirectory,
            "- compactExport: " + OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ExportPackageDirectory,
            string.Empty,
            "The next human action remains running the Goal110 Unity checklist and placing the real result JSON at the preferred path. If no real result exists, the state remains pending.",
            string.Empty,
            "Do not start live geodata, providers, Runtime consumer, public schema, Lua, generator-library, final art, atlas, scene/prefab/project settings or release packaging from this handoff.",
            string.Empty
        };
        return string.Join(Environment.NewLine, lines);
    }

    private static string RenderPreflightReport(
        OfflineGeoworldAlphaAcceptanceOperatorDashboard dashboard,
        OfflineGeoworldAlphaAcceptanceOperatorQualityGateScan quality)
    {
        var lines = new List<string>
        {
            "# Goal 112 Offline Geoworld Alpha Acceptance Operator Preflight",
            string.Empty,
            "- implementationStatus: " + quality.ImplementationStatus,
            "- accepted: false",
            "- manualGate: " + dashboard.ManualGate + " required",
            "- operatorStatus: " + dashboard.OperatorStatus,
            "- goal111DecisionStatus: " + dashboard.DecisionStatusFromGoal111,
            "- preferredManualResultPath: " + dashboard.PreferredManualResultPath,
            "- checklistStepCount: " + dashboard.ChecklistStepCount,
            "- checklistHash: " + dashboard.ChecklistHash,
            "- resultTemplateHash: " + dashboard.ResultTemplateHash,
            "- manualResultPresent: " + dashboard.ManualResultPresent.ToString().ToLowerInvariant(),
            "- manualResultAvailableForHumanReview: "
            + dashboard.ManualResultAvailableForHumanReview.ToString().ToLowerInvariant(),
            "- acceptedByCodex: false",
            "- humanAcceptanceStillRequired: true",
            "- qualityGatePassed: " + quality.Passed.ToString().ToLowerInvariant()
        };
        if (dashboard.Errors.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("## Errors");
            lines.AddRange(dashboard.Errors.Select(error => "- " + error));
        }

        if (dashboard.Warnings.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("## Warnings");
            lines.AddRange(dashboard.Warnings.Select(warning => "- " + warning));
        }

        lines.Add(string.Empty);
        return string.Join(Environment.NewLine, lines);
    }

    private static string RenderExportReadme(
        OfflineGeoworldAlphaAcceptanceOperatorDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Goal 112 Offline Geoworld Alpha Acceptance Operator Pack",
            string.Empty,
            "This compact export summarizes the operator-readiness state for the open offline geoworld Alpha manual gate.",
            string.Empty,
            "- operatorStatus: " + dashboard.OperatorStatus,
            "- goal111DecisionStatus: " + dashboard.DecisionStatusFromGoal111,
            "- manualResultPresent: " + dashboard.ManualResultPresent.ToString().ToLowerInvariant(),
            "- acceptedByCodex: false",
            "- humanAcceptanceStillRequired: true",
            "- preferredManualResultPath: " + dashboard.PreferredManualResultPath,
            string.Empty,
            "The human gate remains open until a real manual result is supplied and explicitly accepted by the user.",
            string.Empty
        };
        return string.Join(Environment.NewLine, lines);
    }
}
