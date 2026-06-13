using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanPackageExportRunService
{
    private readonly GeneratorPlanDraftArtifactApprovalArtifactService _approvalArtifactService;
    private readonly GeneratorPlanDraftArtifactApprovalArtifactReader _approvalArtifactReader;
    private readonly GeneratorPlanGamePackageAssemblyService _assemblyService;
    private readonly GeneratorPlanGamePackageAssemblyArtifactService _assemblyArtifactService;
    private readonly GeneratorPlanPackageExportRunMarkdownRenderer _markdownRenderer;
    private readonly GeneratorPlanPackageExportRunArtifactService _runArtifactService;

    public GeneratorPlanPackageExportRunService(
        GeneratorPlanDraftArtifactApprovalArtifactService approvalArtifactService,
        GeneratorPlanDraftArtifactApprovalArtifactReader approvalArtifactReader,
        GeneratorPlanGamePackageAssemblyService assemblyService,
        GeneratorPlanGamePackageAssemblyArtifactService assemblyArtifactService,
        GeneratorPlanPackageExportRunMarkdownRenderer markdownRenderer,
        IGeneratedArtifactRepository artifactRepository)
    {
        _approvalArtifactService = approvalArtifactService;
        _approvalArtifactReader = approvalArtifactReader;
        _assemblyService = assemblyService;
        _assemblyArtifactService = assemblyArtifactService;
        _markdownRenderer = markdownRenderer;
        _runArtifactService = new GeneratorPlanPackageExportRunArtifactService(artifactRepository);
    }

    public async Task<GeneratorPlanPackageExportRunResult> RunAsync(
        GeneratorPlanPackageExportRunRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var generatedAtUtc = DateTimeOffset.UtcNow;
        var sourceExamplePath = request.SourceExamplePath.Trim();
        var exportFolderPath = request.ExportFolderPath.Trim();
        var packageJsonPath = string.IsNullOrWhiteSpace(exportFolderPath)
            ? string.Empty
            : Path.Combine(exportFolderPath, "package.json");
        var diagnostics = Validate(sourceExamplePath, exportFolderPath);
        var approvalArtifacts = new GeneratorPlanDraftArtifactApprovalArtifactResult();
        var assemblyResult = new GeneratorPlanGamePackageAssemblyResult();
        GeneratorPlanGamePackageAssemblyArtifactResult? assemblyArtifacts = null;

        if (diagnostics.Count == 0)
        {
            approvalArtifacts = await _approvalArtifactService.CaptureAsync(new GeneratorPlanDraftArtifactApprovalArtifactRequest
            {
                PreviewRequest = new GeneratorPlanPreviewRequest { SourcePath = sourceExamplePath },
                ApprovalRequest = request.ApprovalRequest with
                {
                    AutoApproveValidArtifacts = request.AutoApproveValidArtifacts,
                    RenderMarkdown = request.RenderMarkdown
                }
            }, cancellationToken).ConfigureAwait(false);
            diagnostics.AddRange(MapApprovalDiagnostics(approvalArtifacts.ApprovalResult.Diagnostics));

            if (!approvalArtifacts.ApprovalResult.Ok)
            {
                diagnostics.Add(Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Error,
                    GeneratorPlanPackageExportRunDiagnosticCodes.ApprovalFailed,
                    $"Approval failed with status '{approvalArtifacts.ApprovalResult.Status}'.",
                    approvalArtifacts.StagingArtifact.Id));
            }
            else
            {
                _ = await _approvalArtifactReader.ReadLatestAsync(cancellationToken).ConfigureAwait(false);
                assemblyResult = await _assemblyService.AssembleFromLatestApprovedArtifactSetAsync(request.AssemblyRequest with
                {
                    ExportPackageJson = true,
                    ExportFolderPath = exportFolderPath,
                    SerializePackageJson = true,
                    RenderMarkdown = request.RenderMarkdown
                }, cancellationToken).ConfigureAwait(false);
                diagnostics.AddRange(MapAssemblyDiagnostics(assemblyResult.Diagnostics));

                if (!assemblyResult.Ok)
                {
                    diagnostics.Add(Diagnostic(
                        GeneratorPlanPreviewDiagnosticSeverity.Error,
                        GeneratorPlanPackageExportRunDiagnosticCodes.AssemblyFailed,
                        $"Assembly failed with status '{assemblyResult.Status}'.",
                        exportFolderPath));
                }

                if (!string.IsNullOrWhiteSpace(assemblyResult.ExportFolderPath) && !File.Exists(packageJsonPath))
                {
                    diagnostics.Add(Diagnostic(
                        GeneratorPlanPreviewDiagnosticSeverity.Error,
                        GeneratorPlanPackageExportRunDiagnosticCodes.PackageJsonMissingAfterExport,
                        "package.json was not found after export.",
                        packageJsonPath));
                }

                assemblyArtifacts = await _assemblyArtifactService
                    .SaveAsync(assemblyResult, new GeneratorPlanGamePackageAssemblyArtifactSaveRequest(), cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        var result = BuildResult(
            generatedAtUtc,
            sourceExamplePath,
            exportFolderPath,
            packageJsonPath,
            approvalArtifacts,
            assemblyResult,
            assemblyArtifacts,
            diagnostics,
            request.RenderMarkdown);

        if (request.SaveArtifacts)
        {
            await _runArtifactService.SaveAsync(result, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    private GeneratorPlanPackageExportRunResult BuildResult(
        DateTimeOffset generatedAtUtc,
        string sourceExamplePath,
        string exportFolderPath,
        string packageJsonPath,
        GeneratorPlanDraftArtifactApprovalArtifactResult approvalArtifacts,
        GeneratorPlanGamePackageAssemblyResult assemblyResult,
        GeneratorPlanGamePackageAssemblyArtifactResult? assemblyArtifacts,
        IReadOnlyList<GeneratorPlanPackageExportRunDiagnostic> diagnostics,
        bool renderMarkdown)
    {
        var status = BuildStatus(diagnostics);
        var result = new GeneratorPlanPackageExportRunResult
        {
            Ok = status != GeneratorPlanPackageExportRunStatus.Failed,
            Status = status,
            GeneratedAtUtc = generatedAtUtc,
            SourceExamplePath = sourceExamplePath,
            ExportFolderPath = exportFolderPath,
            PackageJsonPath = packageJsonPath,
            ApprovalArtifacts = approvalArtifacts,
            AssemblyResult = assemblyResult,
            AssemblyArtifacts = assemblyArtifacts,
            Diagnostics = diagnostics
        };

        return result with
        {
            MarkdownReport = renderMarkdown ? _markdownRenderer.Render(result) : string.Empty
        };
    }

    private static List<GeneratorPlanPackageExportRunDiagnostic> Validate(string sourceExamplePath, string exportFolderPath)
    {
        var diagnostics = new List<GeneratorPlanPackageExportRunDiagnostic>();

        if (string.IsNullOrWhiteSpace(sourceExamplePath))
        {
            diagnostics.Add(Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanPackageExportRunDiagnosticCodes.MissingSourceExamplePath,
                "Source example path is required.",
                nameof(GeneratorPlanPackageExportRunRequest.SourceExamplePath)));
        }
        else if (!File.Exists(sourceExamplePath))
        {
            diagnostics.Add(Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanPackageExportRunDiagnosticCodes.SourceExampleNotFound,
                "Source example file was not found.",
                sourceExamplePath));
        }

        if (string.IsNullOrWhiteSpace(exportFolderPath))
        {
            diagnostics.Add(Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanPackageExportRunDiagnosticCodes.MissingExportFolderPath,
                "Export folder path is required.",
                nameof(GeneratorPlanPackageExportRunRequest.ExportFolderPath)));
        }

        return diagnostics;
    }

    private static string BuildStatus(IReadOnlyList<GeneratorPlanPackageExportRunDiagnostic> diagnostics)
    {
        if (diagnostics.Any(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error))
        {
            return GeneratorPlanPackageExportRunStatus.Failed;
        }

        return diagnostics.Any(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Warning)
            ? GeneratorPlanPackageExportRunStatus.SucceededWithWarnings
            : GeneratorPlanPackageExportRunStatus.Succeeded;
    }

    private static IEnumerable<GeneratorPlanPackageExportRunDiagnostic> MapApprovalDiagnostics(
        IReadOnlyList<GeneratorPlanDraftArtifactApprovalDiagnostic> diagnostics)
    {
        return diagnostics.Select(diagnostic => Diagnostic(
            diagnostic.Severity,
            GeneratorPlanPackageExportRunDiagnosticCodes.ApprovalDiagnostic,
            $"{diagnostic.Code}: {diagnostic.Message}",
            diagnostic.Target ?? diagnostic.ArtifactId ?? diagnostic.SnapshotId));
    }

    private static IEnumerable<GeneratorPlanPackageExportRunDiagnostic> MapAssemblyDiagnostics(
        IReadOnlyList<GeneratorPlanGamePackageAssemblyDiagnostic> diagnostics)
    {
        return diagnostics.Select(diagnostic => Diagnostic(
            diagnostic.Severity,
            GeneratorPlanPackageExportRunDiagnosticCodes.AssemblyDiagnostic,
            $"{diagnostic.Code}: {diagnostic.Message}",
            diagnostic.Target ?? diagnostic.ArtifactId ?? diagnostic.ArtifactKind));
    }

    private static GeneratorPlanPackageExportRunDiagnostic Diagnostic(
        string severity,
        string code,
        string message,
        string? target)
    {
        return new GeneratorPlanPackageExportRunDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            Target = target
        };
    }
}
