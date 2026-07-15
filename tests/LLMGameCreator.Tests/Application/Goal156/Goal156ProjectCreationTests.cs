using System.Security.Cryptography;
using System.Text.Json;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal156;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class Goal156Collection
{
    public const string Name = "Goal156";
}

[Collection(Goal156Collection.Name)]
public sealed class Goal156ProjectCreationTests
{
    [Fact]
    public async Task Behavioral_legacy_template_lane_keeps_minimal_template_semantics()
    {
        using var scope = Goal156TestKit.Scope("legacy");
        var request = Goal156TestKit.TemplateRequest(scope.Root, "legacy-project");

        var summary = await scope.Service.CreateAsync(request, CancellationToken.None);
        var actual = await Goal156TestKit.Repository.LoadAsync(summary.FolderPath, CancellationToken.None);
        var expected = new NewGamePackageFactory().Create(request);

        Assert.Equal(GameProjectCreationKinds.Template, summary.CreationKind);
        Assert.False(summary.GeneratedSourcePresent);
        Assert.Equal(JsonSerializer.Serialize(expected.Manifest), JsonSerializer.Serialize(actual.Manifest));
        Assert.Equal(JsonSerializer.Serialize(expected.Game), JsonSerializer.Serialize(actual.Game));
    }

    [Fact]
    public void Behavioral_seeded_creation_is_atomic_and_immediately_loadable()
    {
        var project = Goal156TestKit.AllSelectable;
        var package = Goal156TestKit.Load(project.Path);

        Assert.True(File.Exists(Path.Combine(project.Path, "package.json")));
        Assert.DoesNotContain(Directory.EnumerateDirectories(project.Root),
            path => Path.GetFileName(path).Contains(".creating-", StringComparison.Ordinal));
        Assert.Equal(project.Request.PackageId, package.Manifest.PackageId);
        Assert.True(new GamePackageValidator().Validate(package, project.Path).IsValid);
    }

    [Fact]
    public void Behavioral_all_selectable_defaults_uses_the_current_catalog_without_a_fixed_count()
    {
        var state = Goal156TestKit.Authoring(Goal156TestKit.AllSelectable.Path);
        var expected = state.Library.Catalog.Modules
            .Where(module => module.Selectable && !module.Required)
            .Select(module => module.ModuleId)
            .OrderBy(value => value, StringComparer.Ordinal).ToList();

        Assert.NotEmpty(expected);
        Assert.Equal(expected, state.Document.SelectedModuleIds);
        Assert.Empty(state.Document.ParameterValues);
    }

    [Fact]
    public void Behavioral_core_only_selects_no_optional_modules_or_parameter_overrides()
    {
        var state = Goal156TestKit.Authoring(Goal156TestKit.CoreOnly.Path);

        Assert.Empty(state.Document.SelectedModuleIds);
        Assert.Empty(state.Document.ParameterValues);
        Assert.Contains(state.Library.Catalog.Modules, module => module.Required);
    }

    [Fact]
    public async Task Behavioral_project_listing_recognizes_a_current_generated_source()
    {
        var project = Goal156TestKit.AllSelectable;

        var listed = await project.Service.ListAsync(project.Root, CancellationToken.None);
        var summary = Assert.Single(listed);

        Assert.Equal(GameProjectCreationKinds.SeededGenerated, summary.CreationKind);
        Assert.True(summary.GeneratedSourcePresent);
        Assert.Equal("CURRENT", summary.GeneratedSourceStatus);
        Assert.Equal(project.Request.GenerationSeed, summary.GenerationSeed);
        Assert.True(summary.IsValidPackage);
    }

    [Fact]
    public async Task Behavioral_invalid_mode_fails_causally_and_removes_the_transaction_directory()
    {
        using var scope = Goal156TestKit.Scope("invalid-mode");
        var request = Goal156TestKit.GeneratedRequest(scope.Root, "invalid", "seed");
        request.GenerationMode = "unknown-mode";

        var error = await Assert.ThrowsAsync<ArgumentException>(() =>
            scope.Service.CreateAsync(request, CancellationToken.None));

        Assert.Contains("mode", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(Directory.EnumerateFileSystemEntries(scope.Root));
    }

    [Fact]
    public async Task Behavioral_invalid_mechanics_profile_fails_without_a_partial_project()
    {
        using var scope = Goal156TestKit.Scope("invalid-profile");
        var request = Goal156TestKit.GeneratedRequest(scope.Root, "invalid", "seed");
        request.MechanicsProfileId = "fixed-count-profile";

        var error = await Assert.ThrowsAsync<ArgumentException>(() =>
            scope.Service.CreateAsync(request, CancellationToken.None));

        Assert.Contains("profile", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(Directory.EnumerateFileSystemEntries(scope.Root));
    }

    [Fact]
    public async Task Behavioral_existing_target_is_preserved_and_no_temporary_directory_survives()
    {
        using var scope = Goal156TestKit.Scope("existing-target");
        var target = Path.Combine(scope.Root, "existing");
        Directory.CreateDirectory(target);
        File.WriteAllText(Path.Combine(target, "owner.txt"), "keep");
        var request = Goal156TestKit.GeneratedRequest(scope.Root, "existing", "seed");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            scope.Service.CreateAsync(request, CancellationToken.None));

        Assert.Equal("keep", File.ReadAllText(Path.Combine(target, "owner.txt")));
        Assert.DoesNotContain(Directory.EnumerateDirectories(scope.Root),
            path => Path.GetFileName(path).Contains(".creating-", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_generated_source_has_the_exact_required_sidecar_set()
    {
        var generationRoot = Path.Combine(Goal156TestKit.AllSelectable.Path, ".llmgc", "generation");
        var actual = Directory.EnumerateFiles(generationRoot)
            .Select(path => Path.GetFileName(path)!).OrderBy(value => value, StringComparer.Ordinal).ToList();
        var expected = SeededGeneratedProjectVocabulary.RequiredSidecarFileNames
            .Append(SeededGeneratedProjectVocabulary.SourceJsonFileName)
            .OrderBy(value => value, StringComparer.Ordinal).ToList();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Behavioral_seeded_source_uses_project_local_relative_contract_paths()
    {
        var validation = Goal156TestKit.SourceService.Validate(Goal156TestKit.AllSelectable.Path);

        Assert.True(validation.Passed, string.Join(Environment.NewLine, validation.Diagnostics));
        Assert.StartsWith(Goal156TestKit.AllSelectable.Path, validation.SourcePath, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("..", Path.GetRelativePath(Goal156TestKit.AllSelectable.Path, validation.SourcePath));
        Assert.Equal(SeededGeneratedProjectVocabulary.SourceSchemaVersion, validation.Source!.SchemaVersion);
    }
}

internal static class Goal156TestKit
{
    private static readonly Lazy<GeneratedProject> AllSelectableLazy = new(() =>
        CreateGenerated("all-selectable", "goal156-stable-seed", GeneratedProjectMechanicsProfiles.AllSelectableDefaults));
    private static readonly Lazy<GeneratedProject> AllSelectableRepeatLazy = new(() =>
        CreateGenerated("all-selectable-repeat", "goal156-stable-seed", GeneratedProjectMechanicsProfiles.AllSelectableDefaults));
    private static readonly Lazy<GeneratedProject> DifferentSeedLazy = new(() =>
        CreateGenerated("different-seed", "goal156-visible-variation", GeneratedProjectMechanicsProfiles.AllSelectableDefaults));
    private static readonly Lazy<GeneratedProject> CoreOnlyLazy = new(() =>
        CreateGenerated("core-only", "goal156-core-seed", GeneratedProjectMechanicsProfiles.CoreOnly));

    public static string RepositoryRoot { get; } = FindRepositoryRoot();
    public static JsonGamePackageRepository Repository { get; } = new();
    public static GamePackageValidator Validator { get; } = new();
    public static SeededGeneratedGameProjectCreationService Creation { get; } = new(
        RepositoryRoot, Repository, Validator, new NewGamePackageFactory());
    public static SeededGeneratedProjectSourceService SourceService => Creation.SourceService;
    public static GeneratedProject AllSelectable => AllSelectableLazy.Value;
    public static GeneratedProject AllSelectableRepeat => AllSelectableRepeatLazy.Value;
    public static GeneratedProject DifferentSeed => DifferentSeedLazy.Value;
    public static GeneratedProject CoreOnly => CoreOnlyLazy.Value;

    public static TestScope Scope(string name) => new(name);

    public static CreateGameProjectRequest GeneratedRequest(
        string root,
        string folder,
        string seed,
        string profile = GeneratedProjectMechanicsProfiles.AllSelectableDefaults,
        string mode = GenerationPresetOptionsService.DefaultMode,
        string preset = GenerationPresetOptionsService.DefaultPresetId) => new()
        {
            GamesRootPath = root,
            FolderName = folder,
            Title = "Goal156 " + folder,
            PackageId = "game.goal156." + folder.Replace('-', '.'),
            Version = "1.0.0",
            CreationKind = GameProjectCreationKinds.SeededGenerated,
            GenerationSeed = seed,
            GenerationMode = mode,
            GenerationPresetId = preset,
            MechanicsProfileId = profile
        };

    public static CreateGameProjectRequest TemplateRequest(string root, string folder) => new()
    {
        GamesRootPath = root,
        FolderName = folder,
        Title = "Legacy Template",
        PackageId = "game.goal156.legacy",
        Version = "0.1.0"
    };

    public static GamePackageDefinition Load(string project) =>
        Repository.LoadAsync(project, CancellationToken.None).GetAwaiter().GetResult();

    public static GameProjectAuthoringState Authoring(string project)
    {
        var service = new GameProjectFeatureModuleAuthoringService(RepositoryRoot);
        return service.OpenProject(project, Load(project));
    }

    public static UnifiedGameProjectWorkspaceController OpenWorkspace(
        string project,
        IProjectStandaloneBuildService? standalone = null,
        IGameRuntime? runtime = null,
        IRuntimeStateSerializer? stateSerializer = null)
    {
        var current = new CurrentGamePackageService(Repository);
        current.LoadAsync(project, CancellationToken.None).GetAwaiter().GetResult();
        var source = SourceService;
        var summary = new GameProjectGeneratedWorldSummaryService();
        var controller = new UnifiedGameProjectWorkspaceController(
            current,
            new GameProjectFeatureModuleAuthoringService(RepositoryRoot),
            new GameProjectBuildAndQualificationService(
                RepositoryRoot,
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault(),
                Repository,
                Validator,
                current,
                generatedSource: source,
                generatedSummary: summary,
                generatedActivation: new GameProjectGeneratedWorldActivationService(
                    runtime ?? new DefaultGameRuntime(),
                    stateSerializer ?? new RuntimeStateSerializer(),
                    Validator)),
            standaloneBuild: standalone,
            generatedSourceService: source,
            generatedWorldSummaryService: summary);
        controller.OpenProject(project);
        return controller;
    }

    public static GeneratedProject Copy(GeneratedProject source, string suffix)
    {
        var root = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "Goal156Copies", Guid.NewGuid().ToString("N"));
        var target = Path.Combine(root, suffix);
        CopyDirectory(source.Path, target);
        return new GeneratedProject(root, target, source.Request, source.Service, DeleteOnDispose: true);
    }

    public static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    public static string Goal142BaselinePath => Path.Combine(RepositoryRoot, ".llmgc", "procedural",
        "goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff", "candidates",
        "minimal-map-game-balanced-baseline", "package.json");

    private static GeneratedProject CreateGenerated(string folder, string seed, string profile)
    {
        var root = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "Goal156Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var request = GeneratedRequest(root, folder, seed, profile);
        var service = new GameProjectService(Repository, Validator, new NewGamePackageFactory(), Creation);
        var summary = service.CreateAsync(request, CancellationToken.None).GetAwaiter().GetResult();
        return new GeneratedProject(root, summary.FolderPath, request, service, DeleteOnDispose: false);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln"))) return directory.FullName;
        throw new InvalidOperationException("LLMGameCreator repository root was not found.");
    }

    private static void CopyDirectory(string source, string target)
    {
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var destination = Path.Combine(target, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: true);
        }
    }
}

internal sealed class TestScope : IDisposable
{
    public TestScope(string name)
    {
        Root = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "Goal156Scopes", name + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Root);
        Service = new GameProjectService(Goal156TestKit.Repository, Goal156TestKit.Validator,
            new NewGamePackageFactory(), Goal156TestKit.Creation);
    }

    public string Root { get; }
    public GameProjectService Service { get; }

    public void Dispose()
    {
        if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
    }
}

internal sealed record GeneratedProject(
    string Root,
    string Path,
    CreateGameProjectRequest Request,
    GameProjectService Service,
    bool DeleteOnDispose) : IDisposable
{
    public void Dispose()
    {
        if (DeleteOnDispose && Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
    }
}
