using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.Application.Projects;

public interface IGameProjectService
{
    Task<IReadOnlyList<GameProjectSummary>> ListAsync(string gamesRootPath, CancellationToken cancellationToken);
    Task<GameProjectSummary> CreateAsync(CreateGameProjectRequest request, CancellationToken cancellationToken);
}

public sealed class GameProjectService : IGameProjectService
{
    private readonly IGamePackageRepository _repository;
    private readonly IGamePackageValidator _validator;
    private readonly NewGamePackageFactory _factory;
    private readonly SeededGeneratedGameProjectCreationService? _seededCreation;
    private readonly SeededGeneratedProjectSourceService _generatedSource;

    public GameProjectService(
        IGamePackageRepository repository,
        IGamePackageValidator validator,
        NewGamePackageFactory factory)
        : this(repository, validator, factory, null)
    {
    }

    public GameProjectService(
        IGamePackageRepository repository,
        IGamePackageValidator validator,
        NewGamePackageFactory factory,
        SeededGeneratedGameProjectCreationService? seededCreation)
    {
        _repository = repository;
        _validator = validator;
        _factory = factory;
        _seededCreation = seededCreation;
        _generatedSource = seededCreation?.SourceService ?? new SeededGeneratedProjectSourceService(validator);
    }

    public async Task<IReadOnlyList<GameProjectSummary>> ListAsync(string gamesRootPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(gamesRootPath) || !Directory.Exists(gamesRootPath))
        {
            return Array.Empty<GameProjectSummary>();
        }

        var root = Path.GetFullPath(gamesRootPath);
        var candidateFolders = new List<string>();

        if (File.Exists(Path.Combine(root, "package.json")))
        {
            candidateFolders.Add(root);
        }

        foreach (var directory in Directory.EnumerateDirectories(root).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            if (File.Exists(Path.Combine(directory, "package.json")))
            {
                candidateFolders.Add(Path.GetFullPath(directory));
            }
        }

        var summaries = new List<GameProjectSummary>();
        foreach (var folder in candidateFolders)
        {
            summaries.Add(await CreateSummaryAsync(folder, cancellationToken).ConfigureAwait(false));
        }

        return summaries
            .OrderBy(summary => summary.Title ?? summary.FolderName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(summary => summary.FolderName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(summary => summary.FolderPath, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<GameProjectSummary> CreateAsync(CreateGameProjectRequest request, CancellationToken cancellationToken)
    {
        var gamesRootPath = ValidateGamesRootPath(request.GamesRootPath);
        var folderName = ValidateFolderName(request.FolderName);

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Title must not be empty.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.PackageId))
        {
            throw new ArgumentException("PackageId must not be empty.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Version))
        {
            throw new ArgumentException("Version must not be empty.", nameof(request));
        }

        var targetFolder = Path.GetFullPath(Path.Combine(gamesRootPath, folderName));
        EnsureInsideRoot(gamesRootPath, targetFolder);

        if (Directory.Exists(targetFolder))
        {
            throw new InvalidOperationException($"Project folder already exists: {targetFolder}");
        }

        var creationKind = string.IsNullOrWhiteSpace(request.CreationKind)
            ? GameProjectCreationKinds.Template
            : request.CreationKind.Trim();
        if (creationKind == GameProjectCreationKinds.SeededGenerated)
        {
            if (_seededCreation is null)
                throw new InvalidOperationException("Seeded generated project creation is not configured.");
            await _seededCreation.CreateAsync(request, gamesRootPath, targetFolder, cancellationToken).ConfigureAwait(false);
            return await CreateSummaryAsync(targetFolder, cancellationToken).ConfigureAwait(false);
        }
        if (creationKind != GameProjectCreationKinds.Template)
            throw new ArgumentException("Unknown project creation kind: " + creationKind, nameof(request));

        Directory.CreateDirectory(targetFolder);
        Directory.CreateDirectory(Path.Combine(targetFolder, "assets"));
        Directory.CreateDirectory(Path.Combine(targetFolder, "scripts"));
        Directory.CreateDirectory(Path.Combine(targetFolder, "saves"));

        var package = _factory.Create(request);
        await _repository.SaveAsync(targetFolder, package, cancellationToken).ConfigureAwait(false);

        return await CreateSummaryAsync(targetFolder, cancellationToken).ConfigureAwait(false);
    }

    private async Task<GameProjectSummary> CreateSummaryAsync(string folder, CancellationToken cancellationToken)
    {
        var summary = new GameProjectSummary
        {
            FolderPath = folder,
            FolderName = Path.GetFileName(folder),
            HasPackageFile = File.Exists(Path.Combine(folder, "package.json"))
        };

        if (!summary.HasPackageFile)
        {
            summary.IsValidPackage = false;
            summary.ErrorMessage = "package.json not found.";
            return summary;
        }

        try
        {
            var package = await _repository.LoadAsync(folder, cancellationToken).ConfigureAwait(false);
            var report = _validator.Validate(package, folder);

            summary.PackageId = package.Manifest.PackageId;
            summary.Title = package.Manifest.Title;
            summary.Version = package.Manifest.Version;
            summary.ErrorCount = report.Issues.Count(IsError);
            summary.WarningCount = report.Issues.Count(issue => issue.Severity == ValidationSeverity.Warning);
            summary.HasValidationErrors = !report.IsValid;
            summary.IsValidPackage = report.IsValid;
            summary.ErrorMessage = report.IsValid ? null : report.Issues.FirstOrDefault(IsError)?.Message;

            var generated = _generatedSource.Validate(folder);
            if (generated.Present)
            {
                summary.CreationKind = GameProjectCreationKinds.SeededGenerated;
                summary.GenerationSeed = generated.Source?.Seed ?? string.Empty;
                summary.GenerationMode = generated.Source?.Mode ?? string.Empty;
                summary.GenerationPresetId = generated.Source?.PresetId ?? string.Empty;
                summary.MechanicsProfileId = generated.Source?.MechanicsProfileId ?? string.Empty;
                summary.GeneratedSourcePresent = true;
                summary.GeneratedSourceStatus = generated.Status;
                summary.GeneratedCounts = generated.Source?.Counts ?? new GeneratedProjectCounts();
                if (!generated.Passed)
                {
                    summary.IsValidPackage = false;
                    summary.HasValidationErrors = true;
                    summary.ErrorCount++;
                    summary.ErrorMessage ??= generated.Diagnostics.FirstOrDefault();
                }
            }
        }
        catch (Exception ex) when (ex is IOException || ex is InvalidOperationException || ex is System.Text.Json.JsonException || ex is UnauthorizedAccessException)
        {
            summary.IsValidPackage = false;
            summary.HasValidationErrors = true;
            summary.ErrorCount = 1;
            summary.ErrorMessage = ex.Message;
        }

        return summary;
    }

    private static bool IsError(ValidationIssue issue)
    {
        return issue.Severity == ValidationSeverity.Error || issue.Severity == ValidationSeverity.Critical;
    }

    private static string ValidateGamesRootPath(string gamesRootPath)
    {
        if (string.IsNullOrWhiteSpace(gamesRootPath))
        {
            throw new ArgumentException("GamesRootPath must not be empty.", nameof(gamesRootPath));
        }

        Directory.CreateDirectory(gamesRootPath);
        return Path.GetFullPath(gamesRootPath);
    }

    private static string ValidateFolderName(string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName))
        {
            throw new ArgumentException("FolderName must not be empty.", nameof(folderName));
        }

        var trimmed = folderName.Trim();
        if (Path.IsPathRooted(trimmed))
        {
            throw new ArgumentException("FolderName must be relative.", nameof(folderName));
        }

        if (trimmed.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(part => part == ".."))
        {
            throw new ArgumentException("FolderName must not contain path traversal.", nameof(folderName));
        }

        return trimmed;
    }

    private static void EnsureInsideRoot(string root, string targetFolder)
    {
        var rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;

        if (!targetFolder.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Project folder must stay inside GamesRootPath.");
        }
    }
}
