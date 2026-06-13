using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanPackageExportRunArtifactService
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

    public GeneratorPlanPackageExportRunArtifactService(IGeneratedArtifactRepository artifactRepository)
    {
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanPackageExportRunArtifactSaveResult> SaveAsync(
        GeneratorPlanPackageExportRunResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        var runArtifact = BuildRunArtifact(result);
        var markdownArtifact = BuildMarkdownArtifact(result);
        var validationResults = ToValidationResults(runArtifact.Id, result.Diagnostics);

        await _artifactRepository.SaveGeneratedArtifactAsync(runArtifact, cancellationToken).ConfigureAwait(false);
        await _artifactRepository.SaveValidationResultsAsync(runArtifact.Id, validationResults, cancellationToken).ConfigureAwait(false);

        if (markdownArtifact != null)
        {
            await _artifactRepository.SaveGeneratedArtifactAsync(markdownArtifact, cancellationToken).ConfigureAwait(false);
        }

        return new GeneratorPlanPackageExportRunArtifactSaveResult
        {
            RunArtifact = runArtifact,
            MarkdownArtifact = markdownArtifact,
            ValidationResults = validationResults
        };
    }

    private static GeneratedArtifactRecord BuildRunArtifact(GeneratorPlanPackageExportRunResult result)
    {
        var json = JsonSerializer.Serialize(new GeneratorPlanPackageExportRunArtifactSnapshot
        {
            GeneratedAtUtc = result.GeneratedAtUtc,
            Ok = result.Ok,
            Status = result.Status,
            SourceExamplePath = result.SourceExamplePath,
            ExportFolderPath = result.ExportFolderPath,
            PackageJsonPath = result.PackageJsonPath,
            ApprovalStatus = result.ApprovalArtifacts.ApprovalResult.Status,
            AssemblyStatus = result.AssemblyResult.Status,
            ApprovalArtifacts = result.ApprovalArtifacts,
            AssemblyArtifacts = result.AssemblyArtifacts,
            AssemblyResult = result.AssemblyResult,
            Diagnostics = result.Diagnostics
        }, JsonOptions);

        return new GeneratedArtifactRecord(
            GeneratorPlanPackageExportRunArtifactIds.RunArtifactId,
            GeneratorPlanPackageExportRunArtifactIds.RunArtifactKind,
            GeneratorPlanPackageExportRunArtifactIds.RunArtifactPath,
            json,
            GeneratorPlanPackageExportRunArtifactIds.GeneratedBy,
            ToValidationState(result.Diagnostics),
            BuildMetadataJson(result));
    }

    private static GeneratedArtifactRecord? BuildMarkdownArtifact(GeneratorPlanPackageExportRunResult result)
    {
        if (string.IsNullOrWhiteSpace(result.MarkdownReport))
        {
            return null;
        }

        var json = JsonSerializer.Serialize(new GeneratorPlanPackageExportRunMarkdownArtifactSnapshot
        {
            GeneratedAtUtc = result.GeneratedAtUtc,
            Markdown = result.MarkdownReport
        }, JsonOptions);

        return new GeneratedArtifactRecord(
            GeneratorPlanPackageExportRunArtifactIds.MarkdownArtifactId,
            GeneratorPlanPackageExportRunArtifactIds.MarkdownArtifactKind,
            GeneratorPlanPackageExportRunArtifactIds.MarkdownArtifactPath,
            json,
            GeneratorPlanPackageExportRunArtifactIds.GeneratedBy,
            ToValidationState(result.Diagnostics),
            BuildMetadataJson(result));
    }

    private static string BuildMetadataJson(GeneratorPlanPackageExportRunResult result)
    {
        return JsonSerializer.Serialize(new
        {
            sourceExamplePath = result.SourceExamplePath,
            exportFolderPath = result.ExportFolderPath,
            packageJsonPath = result.PackageJsonPath,
            status = result.Status,
            approvalStatus = result.ApprovalArtifacts.ApprovalResult.Status,
            assemblyStatus = result.AssemblyResult.Status,
            packageId = result.AssemblyResult.Package.Manifest.PackageId,
            title = result.AssemblyResult.Package.Manifest.Title,
            errorCount = result.Diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error),
            warningCount = result.Diagnostics.Count(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning)
        }, JsonOptions);
    }

    private static IReadOnlyList<GeneratedArtifactValidationResultRecord> ToValidationResults(
        string artifactId,
        IReadOnlyList<GeneratorPlanPackageExportRunDiagnostic> diagnostics)
    {
        return diagnostics
            .Where(diagnostic => diagnostic.Severity is GeneratorPlanPreviewDiagnosticSeverity.Error or GeneratorPlanPreviewDiagnosticSeverity.Warning)
            .OrderBy(diagnostic => GeneratorPlanPackageExportRunMarkdownRenderer.SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .Select((diagnostic, index) => new GeneratedArtifactValidationResultRecord(
                StableId(artifactId, index.ToString(), diagnostic.Severity, diagnostic.Code, diagnostic.Target ?? string.Empty, diagnostic.Message),
                artifactId,
                diagnostic.Severity,
                diagnostic.Code,
                diagnostic.Message,
                diagnostic.Target ?? artifactId,
                JsonSerializer.Serialize(new { diagnostic.Target })))
            .ToList();
    }

    private static string ToValidationState(IReadOnlyList<GeneratorPlanPackageExportRunDiagnostic> diagnostics)
    {
        if (diagnostics.Any(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error))
        {
            return GeneratorPlanGamePackageAssemblyValidationState.Invalid;
        }

        return diagnostics.Any(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning)
            ? GeneratorPlanGamePackageAssemblyValidationState.Warnings
            : GeneratorPlanGamePackageAssemblyValidationState.Valid;
    }

    private static string StableId(params string[] parts)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", parts)));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private sealed record GeneratorPlanPackageExportRunArtifactSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public bool Ok { get; init; }
        public string Status { get; init; } = string.Empty;
        public string SourceExamplePath { get; init; } = string.Empty;
        public string ExportFolderPath { get; init; } = string.Empty;
        public string PackageJsonPath { get; init; } = string.Empty;
        public string ApprovalStatus { get; init; } = string.Empty;
        public string AssemblyStatus { get; init; } = string.Empty;
        public GeneratorPlanDraftArtifactApprovalArtifactResult ApprovalArtifacts { get; init; } = new();
        public GeneratorPlanGamePackageAssemblyArtifactResult? AssemblyArtifacts { get; init; }
        public GeneratorPlanGamePackageAssemblyResult AssemblyResult { get; init; } = new();
        public IReadOnlyList<GeneratorPlanPackageExportRunDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanPackageExportRunDiagnostic>();
    }

    private sealed record GeneratorPlanPackageExportRunMarkdownArtifactSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public string Markdown { get; init; } = string.Empty;
    }
}
