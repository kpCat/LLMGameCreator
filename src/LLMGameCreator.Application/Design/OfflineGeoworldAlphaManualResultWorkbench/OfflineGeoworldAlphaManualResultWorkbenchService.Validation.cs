using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultWorkbench;

public sealed partial class OfflineGeoworldAlphaManualResultWorkbenchService
{
    private static OfflineGeoworldAlphaManualResultWorkbenchValidation ValidateCandidates(
        string root,
        Goal110Source goal110,
        IReadOnlyList<string> candidatePaths)
    {
        var candidates = candidatePaths
            .Select(path => ReadCandidate(root, path))
            .Where(candidate => candidate.Exists)
            .ToList();
        if (candidates.Count == 0 || candidates.All(candidate => string.IsNullOrWhiteSpace(candidate.Text)))
        {
            return BaseValidation(goal110) with
            {
                ManualResultPresent = false,
                Warnings = ["manual result file is missing from deterministic candidate paths"]
            };
        }

        var nonEmpty = candidates
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate.Text))
            .ToList();
        var contentHashes = nonEmpty
            .Select(candidate => HashText(candidate.Text))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (contentHashes.Count > 1)
        {
            return BaseValidation(goal110) with
            {
                ManualResultPresent = true,
                ResultFilePath = nonEmpty[0].RelativePath,
                Errors = ["multiple differing manual result files were found"]
            };
        }

        var selected = nonEmpty[0];
        var validation = ValidateResultText(goal110, selected.RelativePath, selected.Text);
        return nonEmpty.Count > 1
            ? validation with
            {
                Warnings = validation.Warnings
                    .Concat(["multiple identical manual result files were found; first path was used"])
                    .OrderBy(item => item, StringComparer.Ordinal)
                    .ToList()
            }
            : validation;
    }

    private static OfflineGeoworldAlphaManualResultWorkbenchValidation ValidateResultText(
        Goal110Source goal110,
        string resultRelativePath,
        string resultText)
    {
        try
        {
            using var document = JsonDocument.Parse(resultText);
            return ValidateResultJson(goal110, resultRelativePath, document.RootElement);
        }
        catch (JsonException ex)
        {
            return BaseValidation(goal110) with
            {
                ManualResultPresent = true,
                ResultFilePath = resultRelativePath,
                Errors = ["malformed manual result JSON: " + ex.Message]
            };
        }
    }

    private static OfflineGeoworldAlphaManualResultWorkbenchValidation ValidateResultJson(
        Goal110Source goal110,
        string resultRelativePath,
        JsonElement result)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var goalId = StringProperty(result, "goalId");
        if (!OfflineGeoworldAlphaManualResultIntakeVocabulary.AcceptedGoalIdAliases.Contains(
                goalId,
                StringComparer.Ordinal))
        {
            errors.Add("goalId does not match Goal110 manual acceptance identity");
        }

        if (!string.Equals(
                StringProperty(result, "manualGate"),
                OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ManualGate,
                StringComparison.Ordinal))
        {
            errors.Add("manualGate does not match offline_geoworld_alpha_manual_acceptance_verification");
        }

        var resultSchema = StringProperty(result, "resultSchema");
        if (!string.IsNullOrWhiteSpace(resultSchema)
            && !string.Equals(resultSchema, goal110.ResultSchema, StringComparison.Ordinal))
        {
            errors.Add("resultSchema does not match Goal110 result schema");
        }

        var checklistHash = StringProperty(result, "checklistHash");
        if (string.IsNullOrWhiteSpace(checklistHash))
        {
            errors.Add("checklistHash is missing");
        }
        else if (!string.Equals(checklistHash, goal110.ChecklistHash, StringComparison.Ordinal))
        {
            errors.Add("checklistHash does not match Goal110 checklist hash");
        }

        var accepted = BoolProperty(result, "accepted");
        if (accepted != true)
        {
            errors.Add("accepted must be true in a human-completed candidate, but Codex still does not accept it");
        }

        var stepResults = ReadStepResults(result);
        var summary = BuildStepSummary(goal110.RequiredSteps, stepResults);
        if (summary.MissingCount > 0)
        {
            errors.Add("missing required step ids: " + string.Join(",", summary.MissingStepIds));
        }

        if (summary.DuplicateCount > 0)
        {
            errors.Add("duplicate required step ids: " + string.Join(",", summary.DuplicateStepIds));
        }

        if (summary.UnknownCount > 0)
        {
            errors.Add("unknown step ids: " + string.Join(",", summary.UnknownStepIds));
        }

        if (summary.InvalidStatusCount > 0)
        {
            errors.Add("invalid step status values are present");
        }

        if (summary.MissingStatusCount > 0)
        {
            errors.Add("missing step status values are present");
        }

        if (summary.FailedCount > 0 || summary.PendingCount > 0 || summary.SkippedCount > 0)
        {
            errors.Add("failed, pending or skipped steps are not acceptable for human review");
        }

        var ready = errors.Count == 0
                    && summary.RequiredStepsPresentExactlyOnce
                    && summary.PassedCount == summary.RequiredStepCount
                    && accepted == true;
        return BaseValidation(goal110) with
        {
            ManualResultPresent = true,
            ReadyForHumanReview = ready,
            ResultFilePath = resultRelativePath,
            ChecklistHashActual = checklistHash,
            StepSummary = summary,
            Errors = errors.OrderBy(item => item, StringComparer.Ordinal).ToList(),
            Warnings = warnings.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldAlphaManualResultWorkbenchStepSummary BuildStepSummary(
        IReadOnlyList<OfflineGeoworldAlphaManualResultWorkbenchStep> requiredSteps,
        IReadOnlyList<ResultStep> resultSteps)
    {
        var requiredIds = requiredSteps.Select(step => step.StepId).ToHashSet(StringComparer.Ordinal);
        var requiredResultSteps = resultSteps.Where(step => requiredIds.Contains(step.StepId)).ToList();
        var groups = requiredResultSteps
            .GroupBy(step => step.StepId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        var missing = requiredIds.Where(id => !groups.ContainsKey(id)).OrderBy(id => id).ToList();
        var duplicates = groups
            .Where(item => item.Value.Count > 1)
            .Select(item => item.Key)
            .OrderBy(id => id)
            .ToList();
        var unknown = resultSteps
            .Where(step => !requiredIds.Contains(step.StepId))
            .Select(step => step.StepId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id)
            .ToList();
        var singles = groups.Where(item => item.Value.Count == 1).Select(item => item.Value[0]).ToList();
        var supported = OfflineGeoworldAlphaManualResultWorkbenchVocabulary.SupportedStatuses
            .ToHashSet(StringComparer.Ordinal);
        return new OfflineGeoworldAlphaManualResultWorkbenchStepSummary
        {
            RequiredStepCount = requiredIds.Count,
            ResultStepCount = resultSteps.Count,
            PassedCount = singles.Count(step => step.Status == "passed"),
            FailedCount = singles.Count(step => step.Status == "failed"),
            PendingCount = singles.Count(step => step.Status == "pending"),
            SkippedCount = singles.Count(step => step.Status == "skipped"),
            MissingCount = missing.Count,
            DuplicateCount = duplicates.Count,
            UnknownCount = unknown.Count,
            InvalidStatusCount = singles.Count(step =>
                !string.IsNullOrWhiteSpace(step.Status) && !supported.Contains(step.Status)),
            MissingStatusCount = singles.Count(step => string.IsNullOrWhiteSpace(step.Status)),
            RequiredStepsPresentExactlyOnce = missing.Count == 0 && duplicates.Count == 0,
            MissingStepIds = missing,
            DuplicateStepIds = duplicates,
            UnknownStepIds = unknown
        };
    }

    private static IReadOnlyList<ResultStep> ReadStepResults(JsonElement result)
    {
        if (!TryGetArray(result, "steps", out var steps)
            && !TryGetArray(result, "stepResults", out steps))
        {
            return [];
        }

        return steps.EnumerateArray()
            .Select(step => new ResultStep(
                StringProperty(step, "stepId"),
                StringProperty(step, "status").Trim().ToLowerInvariant()))
            .ToList();
    }

    private static OfflineGeoworldAlphaManualResultWorkbenchValidation BaseValidation(
        Goal110Source goal110) =>
        new()
        {
            ChecklistHashExpected = goal110.ChecklistHash,
            ChecklistHashActual = goal110.ChecklistHash,
            StepSummary = new OfflineGeoworldAlphaManualResultWorkbenchStepSummary
            {
                RequiredStepCount = goal110.RequiredSteps.Count
            }
        };

    private sealed record ResultStep(string StepId, string Status);
}
