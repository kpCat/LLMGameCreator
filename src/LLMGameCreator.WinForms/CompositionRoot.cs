using DryIoc;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design;
using LLMGameCreator.Application.Editing;
using LLMGameCreator.Application.Generation;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.AssetPipeline;
using LLMGameCreator.Infrastructure.Generation;
using LLMGameCreator.Infrastructure.Logging;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Scripting;
using LLMGameCreator.WinForms.Pages;
using Microsoft.Extensions.Logging;

namespace LLMGameCreator.WinForms;

public sealed class CompositionRoot : IDisposable
{
    private readonly Container _container;
    private readonly ILoggerFactory _loggerFactory;

    public CompositionRoot()
    {
        _container = new Container();

        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LLMGameCreator");
        var logsPath = Path.Combine(appData, "Logs");
        var logFilePath = Path.Combine(logsPath, "LLMGameCreator.log");
        var settingsPath = Path.Combine(appData, "appsettings.json");

        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new SimpleFileLoggerProvider(logFilePath));
        });

        _container.RegisterDelegate<ILoggerFactory>(_ => _loggerFactory, Reuse.Singleton);
        _container.RegisterDelegate<IAppSettingsRepository>(_ => new JsonAppSettingsRepository(settingsPath), Reuse.Singleton);
        _container.Register<IGamePackageRepository, JsonGamePackageRepository>(Reuse.Singleton);
        _container.Register<SqliteDesignDatabase>(Reuse.Singleton);
        _container.RegisterDelegate<IDesignDatabaseInitializer>(resolver => resolver.Resolve<SqliteDesignDatabase>(), Reuse.Singleton);
        _container.RegisterDelegate<IDesignKnowledgeRepository>(resolver => resolver.Resolve<SqliteDesignDatabase>(), Reuse.Singleton);
        _container.RegisterDelegate<IGeneratorLibraryRegistry>(resolver => resolver.Resolve<SqliteDesignDatabase>(), Reuse.Singleton);
        _container.RegisterDelegate<IGeneratorPlanRepository>(resolver => resolver.Resolve<SqliteDesignDatabase>(), Reuse.Singleton);
        _container.RegisterDelegate<IGeneratedArtifactRepository>(resolver => resolver.Resolve<SqliteDesignDatabase>(), Reuse.Singleton);
        _container.Register<IGeneratorLibraryImporter, GeneratorLibraryImportService>(Reuse.Singleton);
        _container.Register<IGeneratorLibraryIntegrityValidator, GeneratorLibraryIntegrityValidator>(Reuse.Singleton);
        _container.Register<GeneratorPlanValidator>(Reuse.Singleton);
        _container.Register<IGeneratorPlanDraftService, GeneratorPlanDraftService>(Reuse.Singleton);
        _container.Register<IGeneratorPlanReviewService, GeneratorPlanReviewService>(Reuse.Singleton);
        _container.Register<IGeneratorPlanPreviewService, GeneratorPlanPreviewService>(Reuse.Singleton);
        _container.Register<ICurrentGamePackageService, CurrentGamePackageService>(Reuse.Singleton);
        _container.Register<NewGamePackageFactory>(Reuse.Singleton);
        _container.Register<IGameProjectService, GameProjectService>(Reuse.Singleton);
        _container.Register<IGamePackageValidator, GamePackageValidator>(Reuse.Singleton);
        _container.Register<IPackageEditorService, PackageEditorService>(Reuse.Singleton);
        _container.RegisterDelegate<ILlmChatClient>(_ => new OpenAiCompatibleLlmChatClient(), Reuse.Singleton);
        _container.Register<IFirstPlayableSliceGenerator, FirstPlayableSliceGenerator>(Reuse.Singleton);
        _container.Register<IGameRuntime, DefaultGameRuntime>(Reuse.Singleton);
        _container.Register<IScriptEngine, NullScriptEngine>(Reuse.Singleton);
        _container.Register<IAssetGenerationProvider, NullAssetGenerationProvider>(Reuse.Singleton);

        _container.RegisterDelegate<DashboardPageControl>(resolver => new DashboardPageControl(
            resolver.Resolve<ICurrentGamePackageService>(),
            resolver.Resolve<IPackageEditorService>()), Reuse.Singleton);

        _container.RegisterDelegate<ProjectsPageControl>(resolver => new ProjectsPageControl(
            resolver.Resolve<ICurrentGamePackageService>(),
            resolver.Resolve<IAppSettingsRepository>(),
            resolver.Resolve<IGameProjectService>(),
            resolver.Resolve<IGamePackageValidator>()), Reuse.Singleton);

        _container.RegisterDelegate<GenerationPageControl>(resolver => new GenerationPageControl(
            resolver.Resolve<ICurrentGamePackageService>(),
            resolver.Resolve<IFirstPlayableSliceGenerator>(),
            resolver.Resolve<IGamePackageValidator>(),
            resolver.Resolve<IAppSettingsRepository>()), Reuse.Singleton);

        _container.RegisterDelegate<ValidationPageControl>(resolver => new ValidationPageControl(
            resolver.Resolve<ICurrentGamePackageService>(),
            resolver.Resolve<IGamePackageValidator>()), Reuse.Singleton);

        _container.RegisterDelegate<RuntimePreviewPageControl>(resolver => new RuntimePreviewPageControl(
            resolver.Resolve<ICurrentGamePackageService>(),
            resolver.Resolve<IGameRuntime>()), Reuse.Singleton);

        _container.RegisterDelegate<GeneratorLibraryPageControl>(resolver => new GeneratorLibraryPageControl(
            resolver.Resolve<ICurrentGamePackageService>(),
            resolver.Resolve<IDesignDatabaseInitializer>(),
            resolver.Resolve<IGeneratorLibraryImporter>(),
            resolver.Resolve<IGeneratorLibraryRegistry>(),
            resolver.Resolve<IGeneratorLibraryIntegrityValidator>(),
            resolver.Resolve<IGeneratorPlanDraftService>(),
            resolver.Resolve<IGeneratorPlanRepository>(),
            resolver.Resolve<IGeneratorPlanReviewService>(),
            resolver.Resolve<IGeneratorPlanPreviewService>()), Reuse.Singleton);

        _container.RegisterDelegate<AssetsPageControl>(resolver => new AssetsPageControl(
            resolver.Resolve<ICurrentGamePackageService>()), Reuse.Singleton);

        _container.RegisterDelegate<SettingsPageControl>(resolver => new SettingsPageControl(
            resolver.Resolve<IAppSettingsRepository>()), Reuse.Singleton);

        _container.RegisterDelegate<IEditorPageRegistry>(resolver => new EditorPageRegistry(new IEditorPage[]
        {
            resolver.Resolve<DashboardPageControl>(),
            resolver.Resolve<ProjectsPageControl>(),
            resolver.Resolve<GenerationPageControl>(),
            resolver.Resolve<ValidationPageControl>(),
            resolver.Resolve<GeneratorLibraryPageControl>(),
            resolver.Resolve<RuntimePreviewPageControl>(),
            resolver.Resolve<AssetsPageControl>(),
            resolver.Resolve<SettingsPageControl>()
        }), Reuse.Singleton);

        _container.RegisterDelegate<MainForm>(resolver => new MainForm(
    resolver.Resolve<IEditorPageRegistry>(),
    resolver.Resolve<ICurrentGamePackageService>(),
    resolver.Resolve<ILoggerFactory>()), Reuse.Singleton);
    }

    public MainForm ResolveMainForm() => _container.Resolve<MainForm>();

    public void Dispose()
    {
        _container.Dispose();
        _loggerFactory.Dispose();
    }
}
