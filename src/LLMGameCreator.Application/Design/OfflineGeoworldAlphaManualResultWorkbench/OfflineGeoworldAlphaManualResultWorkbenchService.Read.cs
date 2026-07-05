using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaAcceptanceOperatorPack;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;

namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultWorkbench;

public sealed partial class OfflineGeoworldAlphaManualResultWorkbenchService
{
    private static WorkbenchSource LoadSource(string root)
    {
        var goal110 = LoadGoal110(root);
        var goal111 = LoadGoal111(root);
        var goal112 = LoadGoal112(root);
        return new WorkbenchSource(goal110, goal111, goal112);
    }

    private static Goal110Source LoadGoal110(string root)
    {
        var export = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ExportPackageDirectory;
        var procedural = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ProceduralOutputDirectory;
        var streaming = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.StreamingAssetsRelativeRoot;
        var roots = new[] { export, procedural, streaming };
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
        var steps = ReadChecklistSteps(checklist.Text);
        var checksumChecklistHash = ReadChecksum(checksums.Text,
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ChecklistFileName);
        var checklistHash = !string.IsNullOrWhiteSpace(checksumChecklistHash)
            ? checksumChecklistHash
            : checklist.Exists
                ? HashFile(Resolve(root, checklist.RelativePath))
                : string.Empty;
        var resultSchema = ReadString(template.Text, "resultSchema");
        if (string.IsNullOrWhiteSpace(resultSchema))
        {
            resultSchema = OfflineGeoworldAlphaManualResultIntakeVocabulary.ResultSchema;
        }

        var files = new[] { checklist, template, dashboard, checksums, fileIndex, manifest }
            .Where(file => file.Exists)
            .ToList();
        var hashes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in files)
        {
            hashes[file.RelativePath] = HashFile(Resolve(root, file.RelativePath));
        }

        var exportPresent = Directory.Exists(Resolve(root, export));
        var proceduralPresent = Directory.Exists(Resolve(root, procedural));
        var streamingPresent = Directory.Exists(Resolve(root, streaming));
        var packagePresent = exportPresent
                             && proceduralPresent
                             && streamingPresent
                             && checklist.Exists
                             && template.Exists
                             && dashboard.Exists
                             && steps.Count > 0;

        return new Goal110Source(
            PackagePresent: packagePresent,
            ExportPackagePresent: exportPresent,
            ProceduralEvidencePresent: proceduralPresent,
            StreamingAssetsPresent: streamingPresent,
            ChecklistRead: checklist.Exists,
            ResultTemplateRead: template.Exists,
            DashboardRead: dashboard.Exists,
            ChecksumsRead: checksums.Exists,
            FileIndexRead: fileIndex.Exists,
            ManifestRead: manifest.Exists,
            RequiredSteps: steps,
            ChecklistHash: checklistHash,
            ResultSchema: resultSchema,
            ResultTemplateText: template.Text,
            Sha256ByRelativePath: hashes);
    }

    private static Goal111Source LoadGoal111(string root)
    {
        var decisionPath = OfflineGeoworldAlphaManualResultIntakeVocabulary.ProceduralOutputDirectory
                           + "/"
                           + OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionFileName;
        var path = Resolve(root, decisionPath);
        if (!File.Exists(path))
        {
            return new Goal111Source(
                DecisionPresent: false,
                DecisionValid: false,
                DecisionStatus: string.Empty,
                ManualResultPresent: false,
                CandidateResultPaths: OfflineGeoworldAlphaManualResultIntakeVocabulary
                    .DefaultCandidateResultRelativePaths,
                ResultFilePath: string.Empty,
                Errors: ["Goal111 decision file is missing."]);
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            var rootElement = document.RootElement;
            var decisionStatus = StringProperty(rootElement, "decisionStatus");
            var acceptedByCodex = BoolProperty(rootElement, "acceptedByCodex") == true;
            var humanRequired = BoolProperty(rootElement, "humanAcceptanceStillRequired") == true;
            var candidatePaths = ReadStringArray(rootElement, "candidateResultPaths");
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(decisionStatus))
            {
                errors.Add("Goal111 decisionStatus is missing.");
            }

            if (acceptedByCodex)
            {
                errors.Add("Goal111 decision must keep acceptedByCodex=false.");
            }

            if (!humanRequired)
            {
                errors.Add("Goal111 decision must keep humanAcceptanceStillRequired=true.");
            }

            return new Goal111Source(
                DecisionPresent: true,
                DecisionValid: errors.Count == 0,
                DecisionStatus: decisionStatus,
                ManualResultPresent: BoolProperty(rootElement, "resultFilePresent") == true,
                CandidateResultPaths: candidatePaths.Count == 0
                    ? OfflineGeoworldAlphaManualResultIntakeVocabulary.DefaultCandidateResultRelativePaths
                    : candidatePaths,
                ResultFilePath: StringProperty(rootElement, "resultFilePath"),
                Errors: errors);
        }
        catch (JsonException ex)
        {
            return new Goal111Source(
                DecisionPresent: true,
                DecisionValid: false,
                DecisionStatus: OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusInvalid,
                ManualResultPresent: false,
                CandidateResultPaths: OfflineGeoworldAlphaManualResultIntakeVocabulary
                    .DefaultCandidateResultRelativePaths,
                ResultFilePath: string.Empty,
                Errors: ["Goal111 decision JSON is malformed: " + ex.Message]);
        }
    }

    private static Goal112Source LoadGoal112(string root)
    {
        var basePath = OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ProceduralOutputDirectory;
        var dashboardRelative = basePath + "/"
                                + OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary
                                    .DashboardFileName;
        var pathMapRelative = basePath + "/"
                              + OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary
                                  .ResultPathMapFileName;
        var runbookRelative = basePath + "/"
                              + OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.RunbookFileName;
        var dashboardExists = File.Exists(Resolve(root, dashboardRelative));
        var pathMapExists = File.Exists(Resolve(root, pathMapRelative));
        var runbookExists = File.Exists(Resolve(root, runbookRelative));
        var operatorStatus = string.Empty;
        var preferredPath = OfflineGeoworldAlphaManualResultWorkbenchVocabulary.PreferredManualResultPath;
        var candidatePaths = new List<string>();

        if (dashboardExists)
        {
            using var dashboard = TryParseJson(root, dashboardRelative);
            operatorStatus = StringProperty(dashboard?.RootElement, "operatorStatus");
            preferredPath = StringProperty(dashboard?.RootElement, "preferredManualResultPath");
            candidatePaths.AddRange(ReadStringArray(dashboard?.RootElement, "candidateManualResultPaths"));
        }

        if (pathMapExists)
        {
            using var pathMap = TryParseJson(root, pathMapRelative);
            var mappedPreferred = StringProperty(pathMap?.RootElement, "preferredManualResultPath");
            if (!string.IsNullOrWhiteSpace(mappedPreferred))
            {
                preferredPath = mappedPreferred;
            }

            candidatePaths.AddRange(ReadStringArray(pathMap?.RootElement, "candidateManualResultPaths"));
        }

        return new Goal112Source(
            DashboardPresent: dashboardExists,
            PathMapPresent: pathMapExists,
            RunbookPresent: runbookExists,
            ArtifactsPresent: dashboardExists && pathMapExists && runbookExists,
            OperatorStatus: operatorStatus,
            PreferredManualResultPath: string.IsNullOrWhiteSpace(preferredPath)
                ? OfflineGeoworldAlphaManualResultWorkbenchVocabulary.PreferredManualResultPath
                : preferredPath,
            CandidateResultPaths: candidatePaths
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList());
    }

    private static IReadOnlyList<string> BuildCandidatePaths(
        WorkbenchSource source,
        IReadOnlyList<string> overrideCandidatePaths)
    {
        if (overrideCandidatePaths.Count > 0)
        {
            return overrideCandidatePaths
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        var paths = new List<string>
        {
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.PreferredManualResultPath
        };
        paths.AddRange(source.Goal111.CandidateResultPaths);
        paths.AddRange(source.Goal112.CandidateResultPaths);
        return paths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static OfflineGeoworldAlphaManualResultWorkbenchSourceLineage BuildSourceLineage(
        WorkbenchSource source)
    {
        var hashes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in source.Goal110.Sha256ByRelativePath)
        {
            hashes[item.Key] = item.Value;
        }

        return new OfflineGeoworldAlphaManualResultWorkbenchSourceLineage
        {
            Goal110PackagePresent = source.Goal110.PackagePresent,
            Goal110ChecklistRead = source.Goal110.ChecklistRead,
            Goal110ResultTemplateRead = source.Goal110.ResultTemplateRead,
            Goal110DashboardRead = source.Goal110.DashboardRead,
            Goal111DecisionPresent = source.Goal111.DecisionPresent,
            Goal111DecisionValid = source.Goal111.DecisionValid,
            Goal112DashboardPresent = source.Goal112.DashboardPresent,
            Goal112PathMapPresent = source.Goal112.PathMapPresent,
            Goal112RunbookPresent = source.Goal112.RunbookPresent,
            Goal112ArtifactsPresent = source.Goal112.ArtifactsPresent,
            Goal110ChecklistStepCount = source.Goal110.RequiredSteps.Count,
            Goal111DecisionStatus = source.Goal111.DecisionStatus,
            Goal112OperatorStatus = source.Goal112.OperatorStatus,
            Sha256ByRelativePath = hashes
        };
    }

    private static IReadOnlyList<OfflineGeoworldAlphaManualResultWorkbenchStep> ReadChecklistSteps(
        string text)
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
                .Select(step => new OfflineGeoworldAlphaManualResultWorkbenchStep
                {
                    StepId = StringProperty(step, "stepId"),
                    Order = IntProperty(step, "order"),
                    Title = StringProperty(step, "title"),
                    EvidenceField = StringProperty(step, "evidenceField"),
                    Required = BoolProperty(step, "required") != false
                })
                .Where(step => !string.IsNullOrWhiteSpace(step.StepId) && step.Required)
                .OrderBy(step => step.Order)
                .ThenBy(step => step.StepId, StringComparer.Ordinal)
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static Goal113File ReadFirstAvailable(string root, IReadOnlyList<string> roots, string fileName)
    {
        foreach (var relativeRoot in roots)
        {
            var relativePath = relativeRoot + "/" + fileName;
            var path = Resolve(root, relativePath);
            if (File.Exists(path))
            {
                return new Goal113File(relativePath, File.ReadAllText(path, Encoding.UTF8), true);
            }
        }

        return new Goal113File(string.Empty, string.Empty, false);
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
            return document.RootElement.TryGetProperty("sha256ByRelativePath", out var map)
                   && map.ValueKind == JsonValueKind.Object
                   && map.TryGetProperty(relativePath, out var value)
                   && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? string.Empty
                : string.Empty;
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

    private static JsonDocument? TryParseJson(string root, string relativePath)
    {
        try
        {
            var path = Resolve(root, relativePath);
            return File.Exists(path) ? JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8)) : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string StringProperty(JsonElement? element, string propertyName) =>
        element is not null ? StringProperty(element.Value, propertyName) : string.Empty;

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

    private static int IntProperty(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

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

    private static IReadOnlyList<string> ReadStringArray(JsonElement? element, string propertyName)
    {
        if (element is null
            || !TryGetArray(element.Value, propertyName, out var array))
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

    private sealed record Goal113File(string RelativePath, string Text, bool Exists);

    private sealed record ResultCandidate(string RelativePath, string Text, bool Exists);

    private sealed record WorkbenchSource(
        Goal110Source Goal110,
        Goal111Source Goal111,
        Goal112Source Goal112);

    private sealed record Goal110Source(
        bool PackagePresent,
        bool ExportPackagePresent,
        bool ProceduralEvidencePresent,
        bool StreamingAssetsPresent,
        bool ChecklistRead,
        bool ResultTemplateRead,
        bool DashboardRead,
        bool ChecksumsRead,
        bool FileIndexRead,
        bool ManifestRead,
        IReadOnlyList<OfflineGeoworldAlphaManualResultWorkbenchStep> RequiredSteps,
        string ChecklistHash,
        string ResultSchema,
        string ResultTemplateText,
        IReadOnlyDictionary<string, string> Sha256ByRelativePath);

    private sealed record Goal111Source(
        bool DecisionPresent,
        bool DecisionValid,
        string DecisionStatus,
        bool ManualResultPresent,
        IReadOnlyList<string> CandidateResultPaths,
        string ResultFilePath,
        IReadOnlyList<string> Errors);

    private sealed record Goal112Source(
        bool DashboardPresent,
        bool PathMapPresent,
        bool RunbookPresent,
        bool ArtifactsPresent,
        string OperatorStatus,
        string PreferredManualResultPath,
        IReadOnlyList<string> CandidateResultPaths);
}
