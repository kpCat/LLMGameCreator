using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanGamePackageAssemblyArtifactService
{
    internal static readonly GeneratedArtifactRecord EmptyArtifact = new(
        string.Empty,
        string.Empty,
        string.Empty,
        "{}",
        string.Empty,
        string.Empty,
        "{}");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanGamePackageAssemblyArtifactService(IGeneratedArtifactRepository artifactRepository)
    {
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanGamePackageAssemblyArtifactResult> SaveAsync(
        GeneratorPlanGamePackageAssemblyResult assemblyResult,
        GeneratorPlanGamePackageAssemblyArtifactSaveRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assemblyResult);
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.AssemblyArtifactId))
        {
            throw new ArgumentException("Assembly artifact id is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.PackageDraftArtifactId))
        {
            throw new ArgumentException("Package draft artifact id is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.GeneratedBy))
        {
            throw new ArgumentException("GeneratedBy is required.", nameof(request));
        }

        var assemblyArtifact = BuildAssemblyArtifact(request, assemblyResult);
        var packageDraftArtifact = BuildPackageDraftArtifact(request, assemblyResult);
        var markdownArtifact = BuildMarkdownArtifact(request, assemblyResult);
        var assemblyValidationResults = GeneratorPlanGamePackageAssemblyPolicy.ToValidationResults(assemblyArtifact.Id, assemblyResult.Diagnostics);
        var packageValidationResults = GeneratorPlanGamePackageAssemblyPolicy.ToValidationResults(
            packageDraftArtifact.Id,
            assemblyResult.Diagnostics.Where(diagnostic => diagnostic.Code is GeneratorPlanGamePackageAssemblyDiagnosticCodes.PackageValidationError or GeneratorPlanGamePackageAssemblyDiagnosticCodes.PackageValidationWarning).ToList());
        var validationResults = assemblyValidationResults.Concat(packageValidationResults).ToList();

        await _artifactRepository.SaveGeneratedArtifactAsync(assemblyArtifact, cancellationToken).ConfigureAwait(false);
        await _artifactRepository.SaveValidationResultsAsync(assemblyArtifact.Id, assemblyValidationResults, cancellationToken).ConfigureAwait(false);
        await _artifactRepository.SaveGeneratedArtifactAsync(packageDraftArtifact, cancellationToken).ConfigureAwait(false);
        await _artifactRepository.SaveValidationResultsAsync(packageDraftArtifact.Id, packageValidationResults, cancellationToken).ConfigureAwait(false);

        if (markdownArtifact != null)
        {
            await _artifactRepository.SaveGeneratedArtifactAsync(markdownArtifact, cancellationToken).ConfigureAwait(false);
        }

        return new GeneratorPlanGamePackageAssemblyArtifactResult
        {
            AssemblyResult = assemblyResult,
            AssemblyArtifact = assemblyArtifact,
            PackageDraftArtifact = packageDraftArtifact,
            MarkdownArtifact = markdownArtifact,
            ValidationResults = validationResults
        };
    }

    private static GeneratedArtifactRecord BuildAssemblyArtifact(
        GeneratorPlanGamePackageAssemblyArtifactSaveRequest request,
        GeneratorPlanGamePackageAssemblyResult assemblyResult)
    {
        var json = JsonSerializer.Serialize(new GeneratorPlanGamePackageAssemblyArtifactSnapshot
        {
            GeneratedAtUtc = assemblyResult.GeneratedAtUtc,
            Ok = assemblyResult.Ok,
            Status = assemblyResult.Status,
            ApprovedArtifactSet = assemblyResult.ApprovedArtifactSet,
            Summary = assemblyResult.Summary,
            Mappings = assemblyResult.Mappings,
            Diagnostics = assemblyResult.Diagnostics,
            PackageArtifactId = request.PackageDraftArtifactId,
            MarkdownArtifactId = string.IsNullOrWhiteSpace(assemblyResult.MarkdownReport) ? string.Empty : request.MarkdownArtifactId
        }, JsonOptions);

        return new GeneratedArtifactRecord(
            request.AssemblyArtifactId.Trim(),
            GeneratorPlanGamePackageAssemblyArtifactIds.AssemblyArtifactKind,
            GeneratorPlanGamePackageAssemblyArtifactIds.AssemblyArtifactPath,
            json,
            request.GeneratedBy.Trim(),
            GeneratorPlanGamePackageAssemblyPolicy.ToValidationState(assemblyResult.Summary),
            BuildMetadataJson(assemblyResult));
    }

    private static GeneratedArtifactRecord BuildPackageDraftArtifact(
        GeneratorPlanGamePackageAssemblyArtifactSaveRequest request,
        GeneratorPlanGamePackageAssemblyResult assemblyResult)
    {
        var json = string.IsNullOrWhiteSpace(assemblyResult.PackageJson) ? "{}" : assemblyResult.PackageJson;
        return new GeneratedArtifactRecord(
            request.PackageDraftArtifactId.Trim(),
            GeneratorPlanGamePackageAssemblyArtifactIds.PackageDraftArtifactKind,
            GeneratorPlanGamePackageAssemblyArtifactIds.PackageDraftArtifactPath,
            json,
            request.GeneratedBy.Trim(),
            GeneratorPlanGamePackageAssemblyPolicy.ToValidationState(assemblyResult.Summary),
            BuildMetadataJson(assemblyResult));
    }

    private static GeneratedArtifactRecord? BuildMarkdownArtifact(
        GeneratorPlanGamePackageAssemblyArtifactSaveRequest request,
        GeneratorPlanGamePackageAssemblyResult assemblyResult)
    {
        if (string.IsNullOrWhiteSpace(assemblyResult.MarkdownReport))
        {
            return null;
        }

        var id = string.IsNullOrWhiteSpace(request.MarkdownArtifactId)
            ? GeneratorPlanGamePackageAssemblyArtifactIds.MarkdownArtifactId
            : request.MarkdownArtifactId.Trim();
        var json = JsonSerializer.Serialize(new GeneratorPlanGamePackageAssemblyMarkdownArtifactSnapshot
        {
            GeneratedAtUtc = assemblyResult.GeneratedAtUtc,
            Markdown = assemblyResult.MarkdownReport
        }, JsonOptions);

        return new GeneratedArtifactRecord(
            id,
            GeneratorPlanGamePackageAssemblyArtifactIds.MarkdownArtifactKind,
            GeneratorPlanGamePackageAssemblyArtifactIds.MarkdownArtifactPath,
            json,
            request.GeneratedBy.Trim(),
            GeneratorPlanGamePackageAssemblyPolicy.ToValidationState(assemblyResult.Summary),
            BuildMetadataJson(assemblyResult));
    }

    private static string BuildMetadataJson(GeneratorPlanGamePackageAssemblyResult assemblyResult)
    {
        return JsonSerializer.Serialize(new
        {
            packageId = assemblyResult.Package.Manifest.PackageId,
            title = assemblyResult.Package.Manifest.Title,
            status = assemblyResult.Status,
            approvedArtifactCount = assemblyResult.Summary.ApprovedArtifactCount,
            mappedArtifactCount = assemblyResult.Summary.MappedArtifactCount,
            unmappedArtifactCount = assemblyResult.Summary.UnmappedArtifactCount,
            validationErrorCount = assemblyResult.Summary.ValidationErrorCount,
            validationWarningCount = assemblyResult.Summary.ValidationWarningCount,
            exportFolderPath = assemblyResult.ExportFolderPath
        }, JsonOptions);
    }

    private sealed record GeneratorPlanGamePackageAssemblyArtifactSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public bool Ok { get; init; }
        public string Status { get; init; } = string.Empty;
        public GeneratorPlanApprovedArtifactSet ApprovedArtifactSet { get; init; } = new();
        public GeneratorPlanGamePackageAssemblySummary Summary { get; init; } = new();
        public IReadOnlyList<GeneratorPlanGamePackageAssemblyMapping> Mappings { get; init; } = Array.Empty<GeneratorPlanGamePackageAssemblyMapping>();
        public IReadOnlyList<GeneratorPlanGamePackageAssemblyDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanGamePackageAssemblyDiagnostic>();
        public string PackageArtifactId { get; init; } = string.Empty;
        public string MarkdownArtifactId { get; init; } = string.Empty;
    }

    private sealed record GeneratorPlanGamePackageAssemblyMarkdownArtifactSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public string Markdown { get; init; } = string.Empty;
    }
}
