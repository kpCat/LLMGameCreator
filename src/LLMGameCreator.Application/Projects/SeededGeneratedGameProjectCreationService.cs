using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Projects;

public sealed class SeededGeneratedGameProjectCreationService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _repositoryRoot;
    private readonly IGamePackageRepository _repository;
    private readonly IGamePackageValidator _validator;
    private readonly NewGamePackageFactory _templateFactory;
    private readonly GenerationPresetOptionsService _presetOptions;
    private readonly IGeneratedProjectBaselineProvider _baselineProvider;
    private readonly SeededGeneratedProjectArtifactFactory _artifactFactory;

    public SeededGeneratedGameProjectCreationService(
        string repositoryRoot,
        IGamePackageRepository repository,
        IGamePackageValidator validator,
        NewGamePackageFactory templateFactory,
        GenerationPresetOptionsService? presetOptions = null,
        VisibleGeneratedPlayablePreviewService? generation = null,
        GeneratedProjectOverlayService? overlay = null,
        SeededGeneratedProjectSourceService? sourceService = null,
        IGeneratedProjectBaselineProvider? baselineProvider = null,
        SeededGeneratedProjectArtifactFactory? artifactFactory = null)
    {
        _repositoryRoot = Path.GetFullPath(repositoryRoot);
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _templateFactory = templateFactory ?? throw new ArgumentNullException(nameof(templateFactory));
        _presetOptions = presetOptions ?? new GenerationPresetOptionsService();
        var generationService = generation ?? new VisibleGeneratedPlayablePreviewService(
            generationOptionsService: _presetOptions);
        var overlayService = overlay ?? new GeneratedProjectOverlayService(_validator);
        _baselineProvider = baselineProvider ?? new Goal142GeneratedProjectBaselineProvider(_repositoryRoot);
        _artifactFactory = artifactFactory ?? new SeededGeneratedProjectArtifactFactory(
            _baselineProvider,
            _validator,
            _presetOptions,
            generationService,
            overlayService);
        SourceService = sourceService ?? new SeededGeneratedProjectSourceService(
            _validator,
            _presetOptions,
            overlayService,
            baselineProvider: _baselineProvider);
    }

    public SeededGeneratedProjectSourceService SourceService { get; }

    public async Task<string> CreateAsync(
        CreateGameProjectRequest request,
        string gamesRootPath,
        string targetFolder,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateGenerationRequest(request);
        var gamesRoot = Path.GetFullPath(gamesRootPath);
        var target = Path.GetFullPath(targetFolder);
        EnsureContained(gamesRoot, target, "generated_project.target_path_escape");
        if (Directory.Exists(target) || File.Exists(target))
            throw new InvalidOperationException("generated_project.target_exists");

        var temporary = Path.GetFullPath(Path.Combine(
            gamesRoot,
            "." + request.FolderName.Trim() + ".creating-" + Guid.NewGuid().ToString("N")));
        EnsureContained(gamesRoot, temporary, "generated_project.temporary_path_escape");
        if (Directory.Exists(temporary) || File.Exists(temporary))
            throw new InvalidOperationException("generated_project.temporary_exists");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(temporary);
            Directory.CreateDirectory(Path.Combine(temporary, "assets"));
            Directory.CreateDirectory(Path.Combine(temporary, "scripts"));
            Directory.CreateDirectory(Path.Combine(temporary, "saves"));

            var generationRequest = new SeededGeneratedProjectGenerationRequest
            {
                Seed = request.GenerationSeed.Trim(),
                Mode = request.GenerationMode.Trim(),
                PresetId = request.GenerationPresetId.Trim(),
                CompactStyleHintIds = request.CompactStyleHintIds,
                SelectedVariantIds = request.SelectedVariantIds
            };

            var generationRoot = Resolve(temporary, SeededGeneratedProjectVocabulary.GenerationRelativeRoot);
            var artifacts = _artifactFactory.Create(new SeededGeneratedProjectArtifactFactoryRequest
            {
                GenerationRequest = generationRequest,
                MechanicsProfileId = request.MechanicsProfileId.Trim(),
                OutputDirectory = generationRoot
            });
            if (!artifacts.Passed)
                throw new InvalidOperationException("generated_project.generation_failed:"
                                                    + string.Join(";", artifacts.Diagnostics));
            cancellationToken.ThrowIfCancellationRequested();

            var package = DeserializePackage(artifacts.Overlay.GeneratedBasePackageJson);
            var templateManifest = _templateFactory.Create(request).Manifest;
            package.Manifest.PackageId = request.PackageId.Trim();
            package.Manifest.Title = request.Title.Trim();
            package.Manifest.Version = request.Version.Trim();
            package.Manifest.FormatVersion = templateManifest.FormatVersion;
            package.Manifest.Description = "Seeded generated project created by LLMGameCreator.";
            await _repository.SaveAsync(temporary, package, cancellationToken).ConfigureAwait(false);
            MaterializeSupportFiles(temporary, package);

            var validation = _validator.Validate(package, temporary);
            if (!validation.IsValid)
                throw new InvalidOperationException("generated_project.package_invalid:"
                                                    + string.Join(";", validation.Issues.Select(issue => issue.Code)));
            InitializeAuthoring(temporary, package, request.MechanicsProfileId.Trim());
            var sourceValidation = SourceService.Validate(temporary);
            if (!sourceValidation.Present || !sourceValidation.Passed)
                throw new InvalidOperationException("generated_project.source_invalid:"
                                                    + string.Join(";", sourceValidation.Diagnostics));

            cancellationToken.ThrowIfCancellationRequested();
            if (Directory.Exists(target) || File.Exists(target))
                throw new InvalidOperationException("generated_project.target_exists");
            Directory.Move(temporary, target);
            return target;
        }
        catch
        {
            if (Directory.Exists(temporary)) Directory.Delete(temporary, recursive: true);
            throw;
        }
    }

    private void InitializeAuthoring(string projectFolder, GamePackageDefinition package, string mechanicsProfileId)
    {
        var authoring = new GameProjectFeatureModuleAuthoringService(_repositoryRoot);
        var state = authoring.OpenProject(projectFolder, package);
        var desired = mechanicsProfileId == GeneratedProjectMechanicsProfiles.AllSelectableDefaults
            ? state.Library.Catalog.Modules.Where(module => module.Selectable && !module.Required)
                .Select(module => module.ModuleId).ToHashSet(StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);
        foreach (var module in state.Library.Catalog.Modules.Where(module => module.Selectable && !module.Required)
                     .OrderBy(module => module.ModuleId, StringComparer.Ordinal))
            authoring.SetModuleSelected(module.ModuleId, desired.Contains(module.ModuleId));
        var document = authoring.Save();
        if (!document.SelectedModuleIds.SequenceEqual(desired.OrderBy(value => value, StringComparer.Ordinal), StringComparer.Ordinal)
            || document.ParameterValues.Count != 0)
            throw new InvalidOperationException("generated_project.mechanics_profile_initialization_failed");
    }

    private void MaterializeSupportFiles(string projectFolder, GamePackageDefinition package)
    {
        var materializer = new GameProjectSupportFileMaterializer();
        var plan = materializer.CreatePlan(
            package,
            projectFolder,
            new NarrowAlphaTemplateSupportFileSource(Path.Combine(_repositoryRoot, "samples", "minimal-map-game")));
        if (!plan.IsValid)
            throw new InvalidOperationException("generated_project.support_files_invalid:" + string.Join(";", plan.Diagnostics));
        foreach (var entry in GameProjectSupportFileMaterializer.UniqueEntries(plan))
        {
            if (entry.ActivationAction == GameProjectSupportFileActivationAction.Reuse) continue;
            Directory.CreateDirectory(Path.GetDirectoryName(entry.TargetPath)!);
            File.Copy(entry.SourcePath, entry.TargetPath, overwrite: false);
            if (!string.Equals(GameProjectSupportFileMaterializer.HashFile(entry.TargetPath), entry.SourceSha256, StringComparison.Ordinal))
                throw new InvalidOperationException("generated_project.support_file_hash_mismatch:" + entry.RelativePath);
        }
    }

    private void ValidateGenerationRequest(CreateGameProjectRequest request)
    {
        if (!string.Equals(request.CreationKind, GameProjectCreationKinds.SeededGenerated, StringComparison.Ordinal))
            throw new ArgumentException("generated_project.creation_kind_invalid", nameof(request));
        if (string.IsNullOrWhiteSpace(request.GenerationSeed))
            throw new ArgumentException("generated_project.seed_required", nameof(request));
        if (!ProceduralGameGenerationModes.Supported.Contains(request.GenerationMode?.Trim() ?? string.Empty))
            throw new ArgumentException("generated_project.mode_unsupported", nameof(request));
        if (!_presetOptions.GetPresets().Any(preset => preset.PresetId == request.GenerationPresetId?.Trim()))
            throw new ArgumentException("generated_project.preset_unknown", nameof(request));
        if (!GeneratedProjectMechanicsProfiles.Supported.Contains(request.MechanicsProfileId?.Trim() ?? string.Empty, StringComparer.Ordinal))
            throw new ArgumentException("generated_project.profile_unknown", nameof(request));
    }

    private static void AssertResolvedRequest(CreateGameProjectRequest request, GenerationPresetOptions options)
    {
        if (!string.Equals(options.Seed, request.GenerationSeed.Trim(), StringComparison.Ordinal)
            || !string.Equals(options.Mode, request.GenerationMode.Trim(), StringComparison.Ordinal)
            || !string.Equals(options.PresetId, request.GenerationPresetId.Trim(), StringComparison.Ordinal))
            throw new InvalidOperationException("generated_project.options_silently_replaced");
    }

    private static GamePackageDefinition DeserializePackage(string json) =>
        JsonSerializer.Deserialize<GamePackageDefinition>(json, JsonOptions)
        ?? throw new InvalidOperationException("generated_project.package_deserialization_failed");

    private static Task WriteAsync(string path, string value, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        return File.WriteAllTextAsync(path, value, Utf8WithoutBom, cancellationToken);
    }

    private static string Resolve(string root, string relative)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(fullRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(fullRoot, path, "generated_project.path_escape");
        return path;
    }

    private static void EnsureContained(string root, string path, string message)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fullPath = Path.GetFullPath(path);
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!fullPath.Equals(fullRoot, comparison)
            && !fullPath.StartsWith(fullRoot + Path.DirectorySeparatorChar, comparison))
            throw new InvalidOperationException(message);
    }
}
