using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Composition;

public sealed class UnityArchiveExportDryRunService
{
    public const string RelativeOutputDirectory = ".llmgc/unity-export-dry-run";
    public const string PlanJsonFileName = "unity-archive-plan.json";
    public const string PlanMarkdownFileName = "unity-archive-plan.md";
    public const string ArchiveManifestJsonFileName = "unity-archive-manifest.json";
    public const string ValidationReportJsonFileName = "validation-report.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly UnityTargetContractValidator _validator;
    private readonly UnityArchiveExportPlanMarkdownRenderer _renderer;

    public UnityArchiveExportDryRunService(
        UnityTargetContractValidator validator,
        UnityArchiveExportPlanMarkdownRenderer renderer)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    public async Task<UnityArchiveExportDryRunResult> ExportAsync(
        UnityArchiveExportDryRunRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.DesignBrief);
        ArgumentNullException.ThrowIfNull(request.TargetProfile);
        ArgumentNullException.ThrowIfNull(request.ArchiveManifest);
        ArgumentNullException.ThrowIfNull(request.RuntimeModules);
        if (string.IsNullOrWhiteSpace(request.ProjectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(request));
        }

        var projectRoot = Path.GetFullPath(request.ProjectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "unity-export-dry-run"));
        EnsureContained(projectRoot, outputDirectory, "Unity export dry-run output directory");

        var diagnosticItems = CreateDiagnostics(request);
        var plannedFiles = CreatePlannedFiles(request.ArchiveManifest, outputDirectory, diagnosticItems);
        var diagnostics = OrderDiagnostics(diagnosticItems);
        var readiness = ResolveReadiness(diagnostics);
        var plan = new UnityArchiveExportPlan
        {
            Readiness = readiness,
            DesignBriefId = request.DesignBrief.BriefId.Trim(),
            TargetProfileId = request.TargetProfile.TargetProfileId.Trim(),
            ArchiveGameId = request.ArchiveManifest.GameId.Trim(),
            RuntimeModuleIds = Normalize(request.ArchiveManifest.RuntimeModuleIds),
            PlannedFiles = plannedFiles,
            Diagnostics = diagnostics
        };

        Directory.CreateDirectory(outputDirectory);
        var planJsonPath = OutputPath(outputDirectory, PlanJsonFileName);
        var planMarkdownPath = OutputPath(outputDirectory, PlanMarkdownFileName);
        var manifestJsonPath = OutputPath(outputDirectory, ArchiveManifestJsonFileName);
        var validationReportJsonPath = OutputPath(outputDirectory, ValidationReportJsonFileName);

        await WriteJsonAsync(planJsonPath, plan, cancellationToken).ConfigureAwait(false);
        await System.IO.File.WriteAllTextAsync(
            planMarkdownPath,
            _renderer.Render(plan),
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await WriteJsonAsync(manifestJsonPath, request.ArchiveManifest, cancellationToken).ConfigureAwait(false);
        await WriteJsonAsync(
            validationReportJsonPath,
            new UnityArchiveExportValidationReport { Readiness = readiness, Diagnostics = diagnostics },
            cancellationToken).ConfigureAwait(false);

        return new UnityArchiveExportDryRunResult
        {
            OutputDirectoryPath = outputDirectory,
            PlanJsonPath = planJsonPath,
            PlanMarkdownPath = planMarkdownPath,
            ArchiveManifestJsonPath = manifestJsonPath,
            ValidationReportJsonPath = validationReportJsonPath,
            Plan = plan
        };
    }

    private List<UnityArchiveExportDiagnostic> CreateDiagnostics(UnityArchiveExportDryRunRequest request)
    {
        var result = _validator.ValidateArchive(
            request.ArchiveManifest,
            [request.TargetProfile],
            request.RuntimeModules);
        var diagnostics = result.Diagnostics.Select(ConvertDiagnostic).ToList();
        var archiveModuleIds = Normalize(request.ArchiveManifest.RuntimeModuleIds);
        var catalogModuleIds = Normalize(request.RuntimeModules.Select(module => module.ModuleId));

        foreach (var moduleId in Normalize(request.TargetProfile.RequiredRuntimeModuleIds))
        {
            if (!catalogModuleIds.Contains(moduleId, StringComparer.OrdinalIgnoreCase) ||
                !archiveModuleIds.Contains(moduleId, StringComparer.OrdinalIgnoreCase))
            {
                diagnostics.Add(Diagnostic(
                    UnityArchiveExportDiagnosticSeverity.Error,
                    UnityArchiveExportDiagnosticCodes.MissingRequiredRuntimeModule,
                    $"Required target runtime module '{moduleId}' is not available in the archive export plan.",
                    request.TargetProfile.TargetProfileId,
                    moduleId));
            }
        }

        foreach (var moduleId in Normalize(request.DesignBrief.ExpectedUnityRuntimeModuleIds))
        {
            if (!archiveModuleIds.Contains(moduleId, StringComparer.OrdinalIgnoreCase))
            {
                diagnostics.Add(Diagnostic(
                    UnityArchiveExportDiagnosticSeverity.Error,
                    UnityArchiveExportDiagnosticCodes.MissingBriefRuntimeModule,
                    $"Design brief expects runtime module '{moduleId}', but the archive does not select it.",
                    request.DesignBrief.BriefId,
                    moduleId));
            }
        }

        var modulesById = request.RuntimeModules
            .Where(module => !string.IsNullOrWhiteSpace(module.ModuleId))
            .GroupBy(module => module.ModuleId.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        foreach (var moduleId in archiveModuleIds)
        {
            if (modulesById.TryGetValue(moduleId, out var module) &&
                module.Maturity == UnityContractMaturity.PlannedFuture)
            {
                diagnostics.Add(Diagnostic(
                    UnityArchiveExportDiagnosticSeverity.Warning,
                    UnityArchiveExportDiagnosticCodes.FutureRuntimeModule,
                    $"Archive export is blocked by planned future runtime module '{moduleId}'.",
                    request.ArchiveManifest.GameId,
                    moduleId));
            }
        }

        return diagnostics;
    }

    private static IReadOnlyList<UnityArchivePlannedFile> CreatePlannedFiles(
        UnityGameArchiveManifest archive,
        string outputDirectory,
        ICollection<UnityArchiveExportDiagnostic> diagnostics)
    {
        var candidates = new List<UnityArchivePlannedFile>
        {
            PlannedFile("manifest/unity-game-archive.json", "archive_manifest", archive.GameId),
            PlannedFile("composition/game-design-brief.json", "design_brief", archive.DesignBriefId),
            PlannedFile("runtime/modules-index.json", "runtime_modules", archive.TargetProfileId),
            PlannedFile("lua/modules-index.json", "lua_modules", archive.GameId)
        };
        candidates.AddRange(archive.DataPackages.Select(path => PlannedFile(path, "data_package", archive.GameId)));
        candidates.AddRange(archive.UiLayouts.Select(layout =>
            PlannedFile($"ui/layouts/{layout.LayoutId}.json", "ui_layout", layout.LayoutId)));
        if (archive.AssetRequests.Count > 0)
        {
            candidates.Add(PlannedFile("assets/asset-requests.json", "asset_requests", archive.GameId));
        }

        if (archive.AudioRequests.Count > 0)
        {
            candidates.Add(PlannedFile("audio/audio-requests.json", "audio_requests", archive.GameId));
        }

        candidates.AddRange(archive.LocalizationFiles.Select(path => PlannedFile(path, "localization", archive.ContentLanguage)));

        var safeFiles = new List<UnityArchivePlannedFile>();
        foreach (var candidate in candidates)
        {
            if (!TryNormalizeRelativePath(outputDirectory, candidate.RelativePath, out var relativePath))
            {
                diagnostics.Add(Diagnostic(
                    UnityArchiveExportDiagnosticSeverity.Error,
                    UnityArchiveExportDiagnosticCodes.UnsafePlannedPath,
                    $"Planned archive path '{candidate.RelativePath}' must stay under the dry-run output directory.",
                    archive.GameId,
                    candidate.RelativePath));
                continue;
            }

            safeFiles.Add(candidate with { RelativePath = relativePath });
        }

        return safeFiles
            .DistinctBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
            .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(file => file.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private static UnityArchiveExportReadiness ResolveReadiness(
        IReadOnlyList<UnityArchiveExportDiagnostic> diagnostics)
    {
        if (diagnostics.Any(diagnostic =>
                diagnostic.Severity == UnityArchiveExportDiagnosticSeverity.Error &&
                diagnostic.Code == UnityArchiveExportDiagnosticCodes.UnsafePlannedPath))
        {
            return UnityArchiveExportReadiness.Invalid;
        }

        if (diagnostics.Any(diagnostic =>
                diagnostic.Code is UnityArchiveExportDiagnosticCodes.MissingRequiredRuntimeModule or
                    UnityArchiveExportDiagnosticCodes.MissingBriefRuntimeModule ||
                diagnostic.Severity == UnityArchiveExportDiagnosticSeverity.Error &&
                diagnostic.RelatedId.Contains("module", StringComparison.OrdinalIgnoreCase)))
        {
            return UnityArchiveExportReadiness.MissingRequirements;
        }

        if (diagnostics.Any(diagnostic => diagnostic.Severity == UnityArchiveExportDiagnosticSeverity.Error))
        {
            return UnityArchiveExportReadiness.Invalid;
        }

        if (diagnostics.Any(diagnostic => diagnostic.Code == UnityArchiveExportDiagnosticCodes.FutureRuntimeModule))
        {
            return UnityArchiveExportReadiness.BlockedByFutureModules;
        }

        return diagnostics.Any(diagnostic => diagnostic.Severity == UnityArchiveExportDiagnosticSeverity.Warning)
            ? UnityArchiveExportReadiness.ExportableWithWarnings
            : UnityArchiveExportReadiness.ExportableNow;
    }

    private static UnityArchiveExportDiagnostic ConvertDiagnostic(UnityTargetContractDiagnostic diagnostic)
    {
        var severity = diagnostic.Severity switch
        {
            UnityTargetContractDiagnosticSeverity.Error => UnityArchiveExportDiagnosticSeverity.Error,
            UnityTargetContractDiagnosticSeverity.Warning => UnityArchiveExportDiagnosticSeverity.Warning,
            _ => UnityArchiveExportDiagnosticSeverity.Info
        };
        var code = diagnostic.Code == UnityTargetContractDiagnosticCodes.FutureRuntimeModule
            ? UnityArchiveExportDiagnosticCodes.FutureRuntimeModule
            : UnityArchiveExportDiagnosticCodes.ContractDiagnostic;
        return Diagnostic(severity, code, $"[{diagnostic.Code}] {diagnostic.Message}", diagnostic.TargetId, diagnostic.RelatedId);
    }

    private static IReadOnlyList<UnityArchiveExportDiagnostic> OrderDiagnostics(
        IEnumerable<UnityArchiveExportDiagnostic> diagnostics)
    {
        return diagnostics
            .Distinct()
            .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.TargetId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.RelatedId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool TryNormalizeRelativePath(
        string outputDirectory,
        string candidate,
        out string relativePath)
    {
        relativePath = candidate?.Trim().Replace('\\', '/') ?? string.Empty;
        if (string.IsNullOrWhiteSpace(relativePath) ||
            Path.IsPathRooted(relativePath) ||
            relativePath.Contains(':', StringComparison.Ordinal) ||
            relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Any(segment => segment is "." or ".."))
        {
            return false;
        }

        var fullPath = Path.GetFullPath(Path.Combine(outputDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        return IsContained(outputDirectory, fullPath);
    }

    private static string OutputPath(string outputDirectory, string fileName)
    {
        var path = Path.GetFullPath(Path.Combine(outputDirectory, fileName));
        EnsureContained(outputDirectory, path, "Unity export dry-run file");
        return path;
    }

    private static async Task WriteJsonAsync<T>(
        string path,
        T value,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        await System.IO.File.WriteAllTextAsync(path, json, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
    }

    private static IReadOnlyList<string> Normalize(IEnumerable<string> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ThenBy(value => value, StringComparer.Ordinal)
            .ToList();
    }

    private static UnityArchivePlannedFile PlannedFile(string path, string kind, string sourceId)
    {
        return new UnityArchivePlannedFile { RelativePath = path, Kind = kind, SourceId = sourceId };
    }

    private static UnityArchiveExportDiagnostic Diagnostic(
        UnityArchiveExportDiagnosticSeverity severity,
        string code,
        string message,
        string targetId,
        string relatedId = "")
    {
        return new UnityArchiveExportDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            TargetId = targetId,
            RelatedId = relatedId
        };
    }

    private static void EnsureContained(string rootPath, string candidatePath, string pathLabel)
    {
        if (!IsContained(rootPath, candidatePath))
        {
            throw new InvalidOperationException($"{pathLabel} must stay under '{Path.GetFullPath(rootPath)}'.");
        }
    }

    private static bool IsContained(string rootPath, string candidatePath)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var candidate = Path.GetFullPath(candidatePath);
        return string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) ||
               candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static int SeverityOrder(UnityArchiveExportDiagnosticSeverity severity)
    {
        return severity switch
        {
            UnityArchiveExportDiagnosticSeverity.Error => 0,
            UnityArchiveExportDiagnosticSeverity.Warning => 1,
            _ => 2
        };
    }
}
