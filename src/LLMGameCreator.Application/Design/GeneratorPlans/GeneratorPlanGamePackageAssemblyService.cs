using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Validation;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanGamePackageAssemblyService
{
    private static readonly JsonSerializerOptions PackageJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly GeneratorPlanGamePackageAssembler _assembler;
    private readonly IGamePackageValidator _packageValidator;
    private readonly GeneratorPlanGamePackageAssemblyValidator _assemblyValidator;
    private readonly GeneratorPlanGamePackageAssemblyMarkdownRenderer _markdownRenderer;
    private readonly IGamePackageRepository? _packageRepository;
    private readonly GeneratorPlanDraftArtifactApprovalArtifactReader? _approvalArtifactReader;
    private readonly GeneratorPlanApprovedArtifactSetReader _approvedArtifactSetReader;

    static GeneratorPlanGamePackageAssemblyService()
    {
        PackageJsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public GeneratorPlanGamePackageAssemblyService()
        : this(
            new GeneratorPlanGamePackageAssembler(),
            new GamePackageValidator(),
            new GeneratorPlanGamePackageAssemblyValidator(),
            new GeneratorPlanGamePackageAssemblyMarkdownRenderer(),
            null,
            null,
            new GeneratorPlanApprovedArtifactSetReader())
    {
    }

    public GeneratorPlanGamePackageAssemblyService(
        GeneratorPlanGamePackageAssembler assembler,
        IGamePackageValidator packageValidator,
        GeneratorPlanGamePackageAssemblyValidator assemblyValidator,
        GeneratorPlanGamePackageAssemblyMarkdownRenderer markdownRenderer,
        IGamePackageRepository? packageRepository = null,
        GeneratorPlanDraftArtifactApprovalArtifactReader? approvalArtifactReader = null,
        GeneratorPlanApprovedArtifactSetReader? approvedArtifactSetReader = null)
    {
        _assembler = assembler;
        _packageValidator = packageValidator;
        _assemblyValidator = assemblyValidator;
        _markdownRenderer = markdownRenderer;
        _packageRepository = packageRepository;
        _approvalArtifactReader = approvalArtifactReader;
        _approvedArtifactSetReader = approvedArtifactSetReader ?? new GeneratorPlanApprovedArtifactSetReader();
    }

    public async Task<GeneratorPlanGamePackageAssemblyResult> AssembleFromLatestApprovedArtifactSetAsync(
        GeneratorPlanGamePackageAssemblyRequest request,
        CancellationToken cancellationToken = default)
    {
        if (_approvalArtifactReader == null)
        {
            throw new InvalidOperationException("Approval artifact reader is required to assemble from latest approved artifact set.");
        }

        var latest = await _approvalArtifactReader.ReadLatestAsync(cancellationToken).ConfigureAwait(false);
        if (!latest.Exists || latest.ApprovedArtifactSetArtifact == null)
        {
            var emptySet = new GeneratorPlanApprovedArtifactSet();
            return await AssembleFromApprovedArtifactSetAsync(emptySet, request, cancellationToken).ConfigureAwait(false);
        }

        var artifactSet = _approvedArtifactSetReader.Read(latest.ApprovedArtifactSetArtifact);
        return await AssembleFromApprovedArtifactSetAsync(artifactSet, request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GeneratorPlanGamePackageAssemblyResult> AssembleFromApprovedArtifactSetAsync(
        GeneratorPlanApprovedArtifactSet approvedArtifactSet,
        GeneratorPlanGamePackageAssemblyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(approvedArtifactSet);
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var generatedAtUtc = DateTimeOffset.UtcNow;
        var assemblerResult = _assembler.Assemble(approvedArtifactSet, request.AppliedAtUtc ?? generatedAtUtc);
        var validationReport = _packageValidator.Validate(assemblerResult.Package);
        var assemblyDiagnostics = new List<GeneratorPlanGamePackageAssemblyDiagnostic>(assemblerResult.Diagnostics);
        var packageJson = request.SerializePackageJson
            ? SerializePackage(assemblerResult.Package, assemblyDiagnostics)
            : string.Empty;
        var diagnostics = _assemblyValidator.Validate(
            approvedArtifactSet,
            request,
            packageJson,
            validationReport,
            assemblyDiagnostics).ToList();

        if (request.ExportPackageJson && !string.IsNullOrWhiteSpace(request.ExportFolderPath))
        {
            await ExportAsync(request.ExportFolderPath, assemblerResult.Package, diagnostics, cancellationToken).ConfigureAwait(false);
        }

        var summary = GeneratorPlanGamePackageAssemblyPolicy.BuildSummary(
            approvedArtifactSet,
            assemblerResult.Package,
            diagnostics,
            assemblerResult.Mappings,
            validationReport);
        var status = GeneratorPlanGamePackageAssemblyPolicy.BuildStatus(summary, validationReport != null);
        var result = new GeneratorPlanGamePackageAssemblyResult
        {
            Ok = status is GeneratorPlanGamePackageAssemblyStatus.ValidPackage or GeneratorPlanGamePackageAssemblyStatus.Ready,
            Status = status,
            GeneratedAtUtc = generatedAtUtc,
            ApprovedArtifactSet = approvedArtifactSet,
            Package = assemblerResult.Package,
            PackageJson = packageJson,
            PackageValidationReport = validationReport,
            Diagnostics = diagnostics,
            Summary = summary,
            Mappings = assemblerResult.Mappings,
            ExportFolderPath = request.ExportPackageJson ? request.ExportFolderPath : null
        };

        return result with
        {
            MarkdownReport = request.RenderMarkdown ? _markdownRenderer.Render(result) : string.Empty
        };
    }

    private static string SerializePackage(
        GamePackage.GamePackageDefinition package,
        ICollection<GeneratorPlanGamePackageAssemblyDiagnostic> diagnostics)
    {
        try
        {
            return JsonSerializer.Serialize(package, PackageJsonOptions);
        }
        catch (JsonException exception)
        {
            diagnostics.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanGamePackageAssemblyDiagnosticCodes.PackageSerializationError,
                $"Package serialization failed: {exception.Message}",
                target: "package_json"));
            return string.Empty;
        }
    }

    private async Task ExportAsync(
        string exportFolderPath,
        GamePackage.GamePackageDefinition package,
        ICollection<GeneratorPlanGamePackageAssemblyDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        if (_packageRepository == null)
        {
            diagnostics.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanGamePackageAssemblyDiagnosticCodes.ExportFailed,
                "Package repository is required when package export is requested.",
                target: exportFolderPath));
            return;
        }

        try
        {
            await _packageRepository.SaveAsync(exportFolderPath, package, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        {
            diagnostics.Add(GeneratorPlanGamePackageAssemblyPolicy.Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanGamePackageAssemblyDiagnosticCodes.ExportFailed,
                $"Package export failed: {exception.Message}",
                target: exportFolderPath));
        }
    }
}
