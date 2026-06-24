using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Projects;

namespace LLMGameCreator.Application.RuntimePreview;

public sealed class OneClickGeneratedPreviewWorkflowService
{
    private readonly VisibleGeneratedPlayablePreviewService _visiblePreviewService;
    private readonly ProceduralGameKernelService _kernelService;
    private readonly FormulaEffectActionRegistryService _registryService;
    private readonly TinyGeneratedRuntimeLoopService _tinyLoopService;
    private readonly GeneratedPackageMvpService _packageMvpService;
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly SemaphoreSlim _runGate = new(1, 1);

    public OneClickGeneratedPreviewWorkflowService(
        VisibleGeneratedPlayablePreviewService? visiblePreviewService = null,
        ProceduralGameKernelService? kernelService = null,
        FormulaEffectActionRegistryService? registryService = null,
        TinyGeneratedRuntimeLoopService? tinyLoopService = null,
        GeneratedPackageMvpService? packageMvpService = null,
        ICurrentGamePackageService? currentGamePackageService = null)
    {
        _visiblePreviewService = visiblePreviewService ?? new VisibleGeneratedPlayablePreviewService();
        _kernelService = kernelService ?? new ProceduralGameKernelService();
        _registryService = registryService ?? new FormulaEffectActionRegistryService();
        _tinyLoopService = tinyLoopService ?? new TinyGeneratedRuntimeLoopService();
        _packageMvpService = packageMvpService ?? new GeneratedPackageMvpService();
        _currentGamePackageService = currentGamePackageService;
    }

    public async Task<OneClickGeneratedPreviewWorkflowResult> ExecuteAsync(
        OneClickGeneratedPreviewWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await _runGate.WaitAsync(0, cancellationToken).ConfigureAwait(false))
        {
            return Failure(
                "already_running",
                Diagnostic("warning", "one_click_generated_preview.already_running", "workflow", "A one-click generated preview workflow is already running."));
        }

        try
        {
            await Task.Yield();

            var projectRoot = ResolveProjectRoot(request);
            Directory.CreateDirectory(projectRoot);

            var visibleResult = _visiblePreviewService.Generate(new VisibleGeneratedPlayablePreviewRequest
            {
                Seed = request.Seed,
                Mode = request.Mode,
                CompactStyleHintIds = request.CompactStyleHintIds,
                SelectedVariantIds = request.SelectedVariantIds
            });

            var planWrite = await _kernelService
                .WriteAsync(projectRoot, visibleResult.PlanResult, cancellationToken)
                .ConfigureAwait(false);
            var rulePackWrite = await _registryService
                .WriteAsync(projectRoot, visibleResult.RulePackResult, cancellationToken)
                .ConfigureAwait(false);
            var tinyLoopWrite = await _tinyLoopService
                .WriteAsync(projectRoot, visibleResult.TinyLoopResult, cancellationToken)
                .ConfigureAwait(false);
            var packageWrite = await _packageMvpService
                .WriteAsync(projectRoot, visibleResult.PackageMvpResult, cancellationToken)
                .ConfigureAwait(false);
            var visibleWrite = await _visiblePreviewService
                .WriteAsync(projectRoot, visibleResult, cancellationToken)
                .ConfigureAwait(false);

            var currentReplaced = false;
            if (request.ReplaceCurrentPackage && _currentGamePackageService != null)
            {
                _currentGamePackageService.ReplaceCurrent(visibleResult.PackageMvpResult.Package);
                currentReplaced = true;
            }

            var package = visibleResult.PackageMvpResult.Package;
            var diagnostics = SortDiagnostics(ToWorkflowDiagnostics(visibleResult.Report.Diagnostics)
                .Concat(new[]
                {
                    Diagnostic("info", "one_click_generated_preview.no_external_execution", "workflow", "No LLM, provider, Lua, Unity or media execution was invoked."),
                    currentReplaced
                        ? Diagnostic("info", "one_click_generated_preview.current_package_replaced", package.Manifest.PackageId, "Generated package was loaded as the current package.")
                        : Diagnostic("warning", "one_click_generated_preview.current_package_not_replaced", package.Manifest.PackageId, "No current-package service was available, so the generated package was not loaded into editor state.")
                }));

            return new OneClickGeneratedPreviewWorkflowResult
            {
                Ok = true,
                Status = "generated_preview_ready",
                GeneratedPackage = package,
                PackageId = package.Manifest.PackageId,
                PackageTitle = package.Manifest.Title,
                ProjectRootPath = projectRoot,
                Paths = new OneClickGeneratedPreviewWorkflowPaths
                {
                    PlanJsonPath = planWrite.JsonPath,
                    RulePackJsonPath = rulePackWrite.RulePackJsonPath,
                    TinyRuntimeLoopStateJsonPath = tinyLoopWrite.StateJsonPath,
                    GeneratedPackageOutputDirectoryPath = packageWrite.OutputDirectoryPath,
                    GeneratedPackageJsonPath = packageWrite.PackageJsonPath,
                    VisiblePreviewOutputDirectoryPath = visibleWrite.OutputDirectoryPath,
                    VisiblePreviewSnapshotJsonPath = visibleWrite.SnapshotJsonPath,
                    VisiblePreviewReportJsonPath = visibleWrite.ReportJsonPath,
                    VisiblePreviewReportMarkdownPath = visibleWrite.ReportMarkdownPath,
                    ManualVerificationMarkdownPath = visibleWrite.ManualVerificationMarkdownPath
                },
                StableSummary = BuildStableSummary(visibleResult),
                CurrentPackageReplaced = currentReplaced,
                VisiblePreviewResult = visibleResult,
                Diagnostics = diagnostics
            };
        }
        finally
        {
            _runGate.Release();
        }
    }

    private string ResolveProjectRoot(OneClickGeneratedPreviewWorkflowRequest request)
    {
        var configured = FirstNonEmpty(request.ProjectRootPath, _currentGamePackageService?.CurrentFolder);
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return Path.GetFullPath(configured);
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.GetFullPath(Path.Combine(localAppData, "LLMGameCreator", "one-click-generated-preview"));
    }

    private static string BuildStableSummary(VisibleGeneratedPlayablePreviewResult result) =>
        string.Join("; ", new[]
        {
            $"package={result.Snapshot.PackageId}",
            $"title={result.Snapshot.PackageTitle}",
            $"runtimeStart={result.Report.RuntimeStartSucceeded.ToString().ToLowerInvariant()}",
            $"runtimeCommand={result.Report.RuntimeCommandSucceeded.ToString().ToLowerInvariant()}",
            $"regions={result.Snapshot.Counts.Regions}",
            $"npcs={result.Snapshot.Counts.Npcs}",
            $"items={result.Snapshot.Counts.Items}",
            $"encounters={result.Snapshot.Counts.Encounters}",
            $"quests={result.Snapshot.Counts.Quests}",
            $"mechanics={result.Snapshot.Counts.Mechanics}"
        });

    private static OneClickGeneratedPreviewWorkflowResult Failure(
        string status,
        OneClickGeneratedPreviewWorkflowDiagnostic diagnostic) => new()
    {
        Status = status,
        Diagnostics = [diagnostic]
    };

    private static IEnumerable<OneClickGeneratedPreviewWorkflowDiagnostic> ToWorkflowDiagnostics(
        IEnumerable<VisibleGeneratedPlayablePreviewDiagnostic> diagnostics) =>
        diagnostics.Select(item => Diagnostic(item.Severity, item.Code, item.Target, item.Message));

    private static IReadOnlyList<OneClickGeneratedPreviewWorkflowDiagnostic> SortDiagnostics(
        IEnumerable<OneClickGeneratedPreviewWorkflowDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static OneClickGeneratedPreviewWorkflowDiagnostic Diagnostic(
        string severity,
        string code,
        string target,
        string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
}
