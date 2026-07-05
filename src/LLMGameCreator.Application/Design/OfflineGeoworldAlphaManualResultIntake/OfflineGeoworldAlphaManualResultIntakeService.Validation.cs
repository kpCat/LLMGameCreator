using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;

public sealed partial class OfflineGeoworldAlphaManualResultIntakeService
{
    private static Goal110Metadata LoadGoal110Metadata(string root)
    {
        var goal110Export = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary
            .ExportPackageDirectory;
        var goal110Procedural = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary
            .ProceduralOutputDirectory;
        var goal110Streaming = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary
            .StreamingAssetsRelativeRoot;
        var roots = new[] { goal110Export, goal110Procedural, goal110Streaming };
        var checklist = ReadFirstAvailable(root, roots,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ChecklistFileName);
        var template = ReadFirstAvailable(root, roots,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ResultTemplateFileName);
        var dashboard = ReadFirstAvailable(root, roots,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.DashboardFileName);
        var checksums = ReadFirstAvailable(root, roots,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ChecksumsFileName);
        var fileIndex = ReadFirstAvailable(root, roots,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.FileIndexFileName);
        var manifest = ReadFirstAvailable(root, roots,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ManifestFileName);
        var requiredSteps = ReadChecklistSteps(checklist.Text);
        var checklistHashActual = checklist.Exists ? HashFile(Resolve(root, checklist.RelativePath)) : string.Empty;
        var checksumChecklistHash = ReadChecksum(checksums.Text,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ChecklistFileName);
        var templateChecklistHash = ReadString(template.Text, "checklistHash");
        var expectedHash = !string.IsNullOrWhiteSpace(checksumChecklistHash)
            ? checksumChecklistHash
            : templateChecklistHash;
        var resultSchema = ReadString(template.Text, "resultSchema");
        if (string.IsNullOrWhiteSpace(resultSchema))
        {
            resultSchema = OfflineGeoworldAlphaManualResultIntakeVocabulary.ResultSchema;
        }

        var files = new[]
            {
                checklist,
                template,
                dashboard,
                checksums,
                fileIndex,
                manifest
            }
            .Where(file => file.Exists)
            .ToList();
        var hashes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in files)
        {
            hashes[file.RelativePath] = HashFile(Resolve(root, file.RelativePath));
        }

        var lineage = new OfflineGeoworldAlphaManualResultInputPackageLineage
        {
            Goal110ExportPackagePresent = Directory.Exists(Resolve(root, goal110Export)),
            Goal110ProceduralEvidencePresent = Directory.Exists(Resolve(root, goal110Procedural)),
            Goal110StreamingAssetsPresent = Directory.Exists(Resolve(root, goal110Streaming)),
            ChecklistRead = checklist.Exists,
            ResultTemplateRead = template.Exists,
            DashboardRead = dashboard.Exists,
            ChecksumsRead = checksums.Exists,
            FileIndexRead = fileIndex.Exists,
            ManifestRead = manifest.Exists,
            Goal110AcceptedFalse = manifest.Exists && !ReadBool(manifest.Text, "accepted"),
            Goal110ManualAcceptancePending =
                manifest.Exists && ReadBool(manifest.Text, "manualAcceptancePending"),
            Goal110AutomatedGatePassed =
                manifest.Exists && ReadBool(manifest.Text, "automatedGatePassed"),
            ChecklistStepCount = requiredSteps.Count,
            LoadedMetadataFileCount = files.Count,
            Sha256ByRelativePath = hashes
        };

        return new Goal110Metadata(
            lineage,
            requiredSteps,
            expectedHash,
            checklistHashActual,
            resultSchema);
    }

    private static OfflineGeoworldAlphaManualResultDecision BuildDecision(
        string root,
        Goal110Metadata metadata,
        IReadOnlyList<string> candidateResultRelativePaths)
    {
        var candidatePaths = candidateResultRelativePaths.Count == 0
            ? OfflineGeoworldAlphaManualResultIntakeVocabulary.DefaultCandidateResultRelativePaths
            : candidateResultRelativePaths;
        var candidates = candidatePaths
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => ReadCandidate(root, path))
            .Where(candidate => candidate.Exists)
            .ToList();
        if (candidates.Count == 0 || candidates.All(candidate => string.IsNullOrWhiteSpace(candidate.Text)))
        {
            var warnings = candidates.Count == 0
                ? new[] { "manual result file is missing from deterministic candidate paths" }
                : new[] { "manual result file is empty" };
            return PendingDecision(metadata, candidatePaths, warnings);
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
            return InvalidDecision(
                metadata,
                candidatePaths,
                nonEmpty[0].RelativePath,
                ["multiple differing manual result files were found"]);
        }

        var selected = nonEmpty[0];
        if (nonEmpty.Count > 1)
        {
            return ValidateResultText(root, metadata, selected.RelativePath, selected.Text) with
            {
                Warnings =
                    ["multiple identical manual result files were found; first sorted path was used"]
            };
        }

        return ValidateResultText(root, metadata, selected.RelativePath, selected.Text) with
        {
            CandidateResultPaths = candidatePaths
        };
    }

    private static OfflineGeoworldAlphaManualResultDecision ValidateResultText(
        string root,
        Goal110Metadata metadata,
        string resultRelativePath,
        string resultText)
    {
        try
        {
            using var document = JsonDocument.Parse(resultText);
            return ValidateResultJson(root, metadata, resultRelativePath, document.RootElement);
        }
        catch (JsonException ex)
        {
            return InvalidDecision(
                metadata,
                OfflineGeoworldAlphaManualResultIntakeVocabulary.DefaultCandidateResultRelativePaths,
                resultRelativePath,
                ["malformed manual result JSON: " + ex.Message]);
        }
    }

    private static OfflineGeoworldAlphaManualResultDecision ValidateResultJson(
        string root,
        Goal110Metadata metadata,
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

        var manualGate = StringProperty(result, "manualGate");
        if (!string.Equals(manualGate, OfflineGeoworldAlphaManualResultIntakeVocabulary.ManualGate,
                StringComparison.Ordinal))
        {
            errors.Add("manualGate does not match offline_geoworld_alpha_manual_acceptance_verification");
        }

        var resultSchema = StringProperty(result, "resultSchema");
        if (!string.IsNullOrWhiteSpace(resultSchema)
            && !string.Equals(resultSchema, metadata.ResultSchema, StringComparison.Ordinal))
        {
            errors.Add("resultSchema does not match Goal110 result schema");
        }
        else if (string.IsNullOrWhiteSpace(resultSchema))
        {
            warnings.Add("resultSchema is missing; accepted only for Goal110 Unity result compatibility");
        }

        var checklistHash = StringProperty(result, "checklistHash");
        if (string.IsNullOrWhiteSpace(checklistHash))
        {
            errors.Add("checklistHash is missing");
        }
        else if (!string.Equals(checklistHash, metadata.ChecklistHashExpected, StringComparison.Ordinal))
        {
            errors.Add("checklistHash does not match Goal110 checklist hash");
        }

        var accepted = BoolProperty(result, "accepted");
        if (!accepted.HasValue)
        {
            warnings.Add("accepted flag is missing and is treated as false");
        }

        var steps = ReadStepResults(result);
        var summary = BuildStepSummary(metadata.RequiredSteps, steps);
        if (summary.UnknownCount > 0)
        {
            warnings.Add("unknown extra step ids were ignored: "
                         + string.Join(",", summary.UnknownStepIds));
        }

        if (summary.DuplicateCount > 0)
        {
            errors.Add("duplicate required step ids: " + string.Join(",", summary.DuplicateStepIds));
        }

        if (summary.InvalidStatusCount > 0)
        {
            errors.Add("invalid step status values are present");
        }

        var incomplete = summary.MissingCount > 0
                         || summary.MissingStatusCount > 0
                         || summary.FailedCount > 0
                         || summary.PendingCount > 0
                         || summary.SkippedCount > 0;
        var status = errors.Count > 0
            ? OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusInvalid
            : incomplete
                ? OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusIncomplete
                : accepted == true
                    ? OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusGreenCandidate
                    : OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusAcceptedFalse;
        var acceptable = status == OfflineGeoworldAlphaManualResultIntakeVocabulary
            .DecisionStatusGreenCandidate;
        var resultPath = string.IsNullOrWhiteSpace(resultRelativePath)
            ? string.Empty
            : resultRelativePath;
        _ = root;
        return BaseDecision(metadata) with
        {
            DecisionStatus = status,
            AcceptableCandidate = acceptable,
            ResultFilePresent = true,
            ResultFilePath = resultPath,
            StepSummary = summary,
            Errors = errors
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList(),
            Warnings = warnings
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList(),
            DecisionSummary = acceptable
                ? "valid manual result available for human gate decision"
                : "valid manual result available for human gate decision: false"
        };
    }

    private static OfflineGeoworldAlphaManualResultStepSummary BuildStepSummary(
        IReadOnlyList<Goal110ChecklistStep> requiredSteps,
        IReadOnlyList<ResultStep> resultSteps)
    {
        var requiredIds = requiredSteps.Select(step => step.StepId).ToHashSet(StringComparer.Ordinal);
        var requiredResultSteps = resultSteps
            .Where(step => requiredIds.Contains(step.StepId))
            .ToList();
        var groups = requiredResultSteps
            .GroupBy(step => step.StepId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        var missing = requiredIds
            .Where(id => !groups.ContainsKey(id))
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
        var duplicate = groups
            .Where(item => item.Value.Count > 1)
            .Select(item => item.Key)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
        var unknown = resultSteps
            .Where(step => !requiredIds.Contains(step.StepId))
            .Select(step => step.StepId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
        var singleRequiredSteps = groups
            .Where(item => item.Value.Count == 1)
            .Select(item => item.Value[0])
            .ToList();
        var supported = OfflineGeoworldAlphaManualResultIntakeVocabulary.SupportedStatuses
            .ToHashSet(StringComparer.Ordinal);
        return new OfflineGeoworldAlphaManualResultStepSummary
        {
            RequiredStepCount = requiredIds.Count,
            ResultStepCount = resultSteps.Count,
            PassedCount = singleRequiredSteps.Count(step => step.Status == "passed"),
            FailedCount = singleRequiredSteps.Count(step => step.Status == "failed"),
            PendingCount = singleRequiredSteps.Count(step => step.Status == "pending"),
            SkippedCount = singleRequiredSteps.Count(step => step.Status == "skipped"),
            MissingCount = missing.Count,
            DuplicateCount = duplicate.Count,
            UnknownCount = unknown.Count,
            InvalidStatusCount = singleRequiredSteps.Count(step =>
                !string.IsNullOrWhiteSpace(step.Status) && !supported.Contains(step.Status)),
            MissingStatusCount = singleRequiredSteps.Count(step => string.IsNullOrWhiteSpace(step.Status)),
            RequiredStepsPresentExactlyOnce = missing.Count == 0 && duplicate.Count == 0,
            MissingStepIds = missing,
            DuplicateStepIds = duplicate,
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

    private static IReadOnlyList<Goal110ChecklistStep> ReadChecklistSteps(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(text);
            if (!TryGetArray(document.RootElement, "steps", out var steps))
            {
                return [];
            }

            return steps.EnumerateArray()
                .Select(step => new Goal110ChecklistStep(
                    StringProperty(step, "stepId"),
                    !step.TryGetProperty("required", out var required)
                    || required.ValueKind != JsonValueKind.False))
                .Where(step => !string.IsNullOrWhiteSpace(step.StepId) && step.Required)
                .OrderBy(step => step.StepId, StringComparer.Ordinal)
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static OfflineGeoworldAlphaManualResultDecision PendingDecision(
        Goal110Metadata metadata,
        IReadOnlyList<string> candidatePaths,
        IReadOnlyList<string> warnings) =>
        BaseDecision(metadata) with
        {
            CandidateResultPaths = candidatePaths,
            DecisionStatus = OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending,
            Warnings = warnings
        };

    private static OfflineGeoworldAlphaManualResultDecision InvalidDecision(
        Goal110Metadata metadata,
        IReadOnlyList<string> candidatePaths,
        string resultFilePath,
        IReadOnlyList<string> errors) =>
        BaseDecision(metadata) with
        {
            CandidateResultPaths = candidatePaths,
            DecisionStatus = OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusInvalid,
            ResultFilePresent = !string.IsNullOrWhiteSpace(resultFilePath),
            ResultFilePath = resultFilePath,
            Errors = errors
        };

    private static OfflineGeoworldAlphaManualResultDecision BaseDecision(Goal110Metadata metadata) =>
        new()
        {
            ChecklistHashExpected = metadata.ChecklistHashExpected,
            ChecklistHashActual = metadata.ChecklistHashActual,
            InputPackageLineage = metadata.Lineage,
            StepSummary = new OfflineGeoworldAlphaManualResultStepSummary
            {
                RequiredStepCount = metadata.RequiredSteps.Count
            }
        };

    private static Goal110File ReadFirstAvailable(string root, IReadOnlyList<string> roots, string fileName)
    {
        foreach (var relativeRoot in roots)
        {
            var relativePath = relativeRoot + "/" + fileName;
            var path = Resolve(root, relativePath);
            if (File.Exists(path))
            {
                return new Goal110File(relativePath, File.ReadAllText(path, Encoding.UTF8), true);
            }
        }

        return new Goal110File(string.Empty, string.Empty, false);
    }

    private static ResultCandidate ReadCandidate(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path)
            ? new ResultCandidate(relativePath, File.ReadAllText(path, Encoding.UTF8), true)
            : new ResultCandidate(relativePath, string.Empty, false);
    }

    private static string ReadChecksum(string text, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(text);
            if (!document.RootElement.TryGetProperty("sha256ByRelativePath", out var map)
                || map.ValueKind != JsonValueKind.Object
                || !map.TryGetProperty(relativePath, out var value)
                || value.ValueKind != JsonValueKind.String)
            {
                return string.Empty;
            }

            return value.GetString() ?? string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private static string ReadString(string text, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(text);
            return StringProperty(document.RootElement, propertyName);
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private static bool ReadBool(string text, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(text);
            return BoolProperty(document.RootElement, propertyName) == true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string StringProperty(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static bool? BoolProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property)
            || property.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            return null;
        }

        return property.GetBoolean();
    }

    private static bool TryGetArray(JsonElement element, string propertyName, out JsonElement array)
    {
        if (element.TryGetProperty(propertyName, out array)
            && array.ValueKind == JsonValueKind.Array)
        {
            return true;
        }

        array = default;
        return false;
    }

    private sealed record Goal110File(string RelativePath, string Text, bool Exists);

    private sealed record ResultCandidate(string RelativePath, string Text, bool Exists);

    private sealed record Goal110ChecklistStep(string StepId, bool Required);

    private sealed record ResultStep(string StepId, string Status);

    private sealed record Goal110Metadata(
        OfflineGeoworldAlphaManualResultInputPackageLineage Lineage,
        IReadOnlyList<Goal110ChecklistStep> RequiredSteps,
        string ChecklistHashExpected,
        string ChecklistHashActual,
        string ResultSchema)
    {
        public bool PackagePresent =>
            Lineage.Goal110ExportPackagePresent
            && Lineage.Goal110ProceduralEvidencePresent
            && Lineage.Goal110StreamingAssetsPresent
            && Lineage.ChecklistRead
            && Lineage.ResultTemplateRead
            && Lineage.DashboardRead
            && Lineage.ChecksumsRead
            && Lineage.FileIndexRead
            && Lineage.ManifestRead
            && RequiredSteps.Count > 0;
    }
}
