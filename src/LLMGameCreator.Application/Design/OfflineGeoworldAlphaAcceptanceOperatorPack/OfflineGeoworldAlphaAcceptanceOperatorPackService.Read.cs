using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaAcceptanceOperatorPack;

public sealed partial class OfflineGeoworldAlphaAcceptanceOperatorPackService
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
        var checklistHash = checklist.Exists
            ? HashFile(Resolve(root, checklist.RelativePath))
            : string.Empty;
        var resultTemplateHash = template.Exists
            ? HashFile(Resolve(root, template.RelativePath))
            : string.Empty;
        var checksumChecklistHash = ReadChecksum(checksums.Text,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ChecklistFileName);
        if (!string.IsNullOrWhiteSpace(checksumChecklistHash))
        {
            checklistHash = checksumChecklistHash;
        }

        var lineage = new Goal110Lineage(
            Goal110ExportPackagePresent: Directory.Exists(Resolve(root, goal110Export)),
            Goal110ProceduralEvidencePresent: Directory.Exists(Resolve(root, goal110Procedural)),
            Goal110StreamingAssetsPresent: Directory.Exists(Resolve(root, goal110Streaming)),
            ChecklistRead: checklist.Exists,
            ResultTemplateRead: template.Exists,
            DashboardRead: dashboard.Exists,
            ChecksumsRead: checksums.Exists,
            FileIndexRead: fileIndex.Exists,
            ManifestRead: manifest.Exists,
            Goal110AcceptedFalse: manifest.Exists && !ReadBool(manifest.Text, "accepted"),
            Goal110ManualAcceptancePending:
            manifest.Exists && ReadBool(manifest.Text, "manualAcceptancePending"),
            Goal110AutomatedGatePassed:
            manifest.Exists && ReadBool(manifest.Text, "automatedGatePassed"));

        return new Goal110Metadata(
            Lineage: lineage,
            ChecklistStepCount: requiredSteps.Count,
            ChecklistHash: checklistHash,
            ResultTemplateHash: resultTemplateHash,
            ResultTemplateText: template.Text,
            Goal110MetadataFileCount: new[] { checklist, template, dashboard, checksums, fileIndex, manifest }
                .Count(file => file.Exists));
    }

    private static Goal111DecisionEvidence LoadGoal111Decision(string root)
    {
        var decisionPath =
            OfflineGeoworldAlphaManualResultIntakeVocabulary.ProceduralOutputDirectory
            + "/"
            + OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionFileName;
        var exportDashboardPath =
            OfflineGeoworldAlphaManualResultIntakeVocabulary.ExportPackageDirectory
            + "/"
            + OfflineGeoworldAlphaManualResultIntakeVocabulary.ExportDashboardFileName;
        var exportReadmePath =
            OfflineGeoworldAlphaManualResultIntakeVocabulary.ExportPackageDirectory
            + "/"
            + OfflineGeoworldAlphaManualResultIntakeVocabulary.ExportReadmeFileName;
        var exportIndexPath =
            OfflineGeoworldAlphaManualResultIntakeVocabulary.ExportPackageDirectory
            + "/"
            + OfflineGeoworldAlphaManualResultIntakeVocabulary.FileIndexFileName;
        var fullDecisionPath = Resolve(root, decisionPath);
        if (!File.Exists(fullDecisionPath))
        {
            return new Goal111DecisionEvidence(
                DecisionPresent: false,
                DecisionValid: false,
                DecisionStatus: string.Empty,
                ManualResultPresent: false,
                ManualResultAvailableForHumanReview: false,
                AcceptedByCodex: false,
                HumanAcceptanceStillRequired: true,
                CandidateResultPaths:
                OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.CandidateManualResultPaths,
                ResultFilePath: string.Empty,
                ExportDashboardPresent: File.Exists(Resolve(root, exportDashboardPath)),
                ExportReadmePresent: File.Exists(Resolve(root, exportReadmePath)),
                ExportIndexPresent: File.Exists(Resolve(root, exportIndexPath)),
                Errors: ["Goal111 decision file is missing."]);
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(fullDecisionPath, Encoding.UTF8));
            var rootElement = document.RootElement;
            var decisionStatus = StringProperty(rootElement, "decisionStatus");
            var acceptedByCodex = TryGetBool(rootElement, "acceptedByCodex");
            var humanStillRequired = TryGetBool(rootElement, "humanAcceptanceStillRequired");
            var candidatePaths = ReadStringArray(rootElement, "candidateResultPaths");
            var manualResultPresent = TryGetBool(rootElement, "resultFilePresent");
            var decisionValid = !string.IsNullOrWhiteSpace(decisionStatus)
                                && !acceptedByCodex
                                && humanStillRequired;
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(decisionStatus))
            {
                errors.Add("Goal111 decisionStatus is missing.");
            }

            if (acceptedByCodex)
            {
                errors.Add("Goal111 decision must not set acceptedByCodex=true.");
            }

            if (!humanStillRequired)
            {
                errors.Add("Goal111 decision must keep humanAcceptanceStillRequired=true.");
            }

            return new Goal111DecisionEvidence(
                DecisionPresent: true,
                DecisionValid: decisionValid,
                DecisionStatus: decisionStatus,
                ManualResultPresent: manualResultPresent,
                ManualResultAvailableForHumanReview: string.Equals(
                    decisionStatus,
                    OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusGreenCandidate,
                    StringComparison.Ordinal),
                AcceptedByCodex: acceptedByCodex,
                HumanAcceptanceStillRequired: humanStillRequired,
                CandidateResultPaths: candidatePaths.Count == 0
                    ? OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.CandidateManualResultPaths
                    : candidatePaths,
                ResultFilePath: StringProperty(rootElement, "resultFilePath"),
                ExportDashboardPresent: File.Exists(Resolve(root, exportDashboardPath)),
                ExportReadmePresent: File.Exists(Resolve(root, exportReadmePath)),
                ExportIndexPresent: File.Exists(Resolve(root, exportIndexPath)),
                Errors: errors);
        }
        catch (JsonException ex)
        {
            return new Goal111DecisionEvidence(
                DecisionPresent: true,
                DecisionValid: false,
                DecisionStatus: OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusInvalid,
                ManualResultPresent: false,
                ManualResultAvailableForHumanReview: false,
                AcceptedByCodex: false,
                HumanAcceptanceStillRequired: true,
                CandidateResultPaths:
                OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.CandidateManualResultPaths,
                ResultFilePath: string.Empty,
                ExportDashboardPresent: File.Exists(Resolve(root, exportDashboardPath)),
                ExportReadmePresent: File.Exists(Resolve(root, exportReadmePath)),
                ExportIndexPresent: File.Exists(Resolve(root, exportIndexPath)),
                Errors: ["Goal111 decision JSON is malformed: " + ex.Message]);
        }
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

    private static bool ReadBool(string text, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(text);
            return TryGetBool(document.RootElement, propertyName);
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

    private static bool TryGetBool(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        && property.GetBoolean();

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

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
    {
        if (!TryGetArray(element, propertyName, out var array))
        {
            return [];
        }

        return array.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
    }

    private sealed record Goal110File(string RelativePath, string Text, bool Exists);

    private sealed record Goal110ChecklistStep(string StepId, bool Required);

    private sealed record Goal110Lineage(
        bool Goal110ExportPackagePresent,
        bool Goal110ProceduralEvidencePresent,
        bool Goal110StreamingAssetsPresent,
        bool ChecklistRead,
        bool ResultTemplateRead,
        bool DashboardRead,
        bool ChecksumsRead,
        bool FileIndexRead,
        bool ManifestRead,
        bool Goal110AcceptedFalse,
        bool Goal110ManualAcceptancePending,
        bool Goal110AutomatedGatePassed);

    private sealed record Goal110Metadata(
        Goal110Lineage Lineage,
        int ChecklistStepCount,
        string ChecklistHash,
        string ResultTemplateHash,
        string ResultTemplateText,
        int Goal110MetadataFileCount)
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
            && Lineage.Goal110AcceptedFalse
            && Lineage.Goal110ManualAcceptancePending
            && Lineage.Goal110AutomatedGatePassed
            && ChecklistStepCount > 0;
    }

    private sealed record Goal111DecisionEvidence(
        bool DecisionPresent,
        bool DecisionValid,
        string DecisionStatus,
        bool ManualResultPresent,
        bool ManualResultAvailableForHumanReview,
        bool AcceptedByCodex,
        bool HumanAcceptanceStillRequired,
        IReadOnlyList<string> CandidateResultPaths,
        string ResultFilePath,
        bool ExportDashboardPresent,
        bool ExportReadmePresent,
        bool ExportIndexPresent,
        IReadOnlyList<string> Errors);
}
