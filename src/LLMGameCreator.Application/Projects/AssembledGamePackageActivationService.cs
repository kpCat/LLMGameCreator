using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Validation;

namespace LLMGameCreator.Application.Projects;

public sealed class AssembledGamePackageActivationService
{
    private readonly IGamePackageRepository _repository;
    private readonly IGamePackageValidator _validator;
    private readonly ICurrentGamePackageService _currentGamePackageService;

    public AssembledGamePackageActivationService(
        IGamePackageRepository repository,
        IGamePackageValidator validator,
        ICurrentGamePackageService currentGamePackageService)
    {
        _repository = repository;
        _validator = validator;
        _currentGamePackageService = currentGamePackageService;
    }

    public async Task<AssembledGamePackageActivationResult> ActivateLatestAsync(CancellationToken cancellationToken)
    {
        var projectFolder = _currentGamePackageService.CurrentFolder;
        if (string.IsNullOrWhiteSpace(projectFolder))
        {
            return Failure("project_not_open", "No current project folder is available.", string.Empty);
        }

        var assemblyFolder = Path.Combine(projectFolder, ".llmgc", "package-assembly");
        var packagePath = Path.Combine(assemblyFolder, "package.json");
        if (!File.Exists(packagePath))
        {
            return Failure("assembled_package_not_found", $"Assembled package was not found: {packagePath}", packagePath);
        }

        try
        {
            var package = await _repository.LoadAsync(assemblyFolder, cancellationToken).ConfigureAwait(false);
            var report = _validator.Validate(package, projectFolder);
            var diagnostics = report.Issues.Select(issue => issue.ToString()).ToList();
            if (!report.IsValid)
            {
                return new AssembledGamePackageActivationResult
                {
                    Status = "validation_failed",
                    SourcePath = packagePath,
                    PackageTitle = package.Manifest.Title,
                    Diagnostics = diagnostics
                };
            }

            _currentGamePackageService.ReplaceCurrent(package);
            return new AssembledGamePackageActivationResult
            {
                Ok = true,
                Status = "activated",
                SourcePath = packagePath,
                PackageTitle = package.Manifest.Title,
                Diagnostics = diagnostics
            };
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or System.Text.Json.JsonException)
        {
            return Failure("load_failed", ex.Message, packagePath);
        }
    }

    private static AssembledGamePackageActivationResult Failure(string status, string message, string sourcePath)
    {
        return new AssembledGamePackageActivationResult
        {
            Status = status,
            SourcePath = sourcePath,
            Diagnostics = new[] { message }
        };
    }
}

public sealed record AssembledGamePackageActivationResult
{
    public bool Ok { get; init; }
    public string Status { get; init; } = string.Empty;
    public string SourcePath { get; init; } = string.Empty;
    public string PackageTitle { get; init; } = string.Empty;
    public IReadOnlyList<string> Diagnostics { get; init; } = Array.Empty<string>();
}
