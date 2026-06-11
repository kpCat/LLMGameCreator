using DryIoc;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.AssetPipeline;
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
        _container.Register<ICurrentGamePackageService, CurrentGamePackageService>(Reuse.Singleton);
        _container.Register<IGamePackageValidator, GamePackageValidator>(Reuse.Singleton);
        _container.Register<IGameRuntime, DefaultGameRuntime>(Reuse.Singleton);
        _container.Register<IScriptEngine, NullScriptEngine>(Reuse.Singleton);
        _container.Register<IAssetGenerationProvider, NullAssetGenerationProvider>(Reuse.Singleton);

        _container.Register<DashboardPageControl>(Reuse.Singleton);
        _container.Register<ProjectsPageControl>(Reuse.Singleton);
        _container.Register<GenerationPageControl>(Reuse.Singleton);
        _container.Register<ValidationPageControl>(Reuse.Singleton);
        _container.Register<RuntimePreviewPageControl>(Reuse.Singleton);
        _container.Register<AssetsPageControl>(Reuse.Singleton);
        _container.Register<SettingsPageControl>(Reuse.Singleton);

        _container.RegisterDelegate<IEditorPageRegistry>(resolver => new EditorPageRegistry(new IEditorPage[]
        {
            resolver.Resolve<DashboardPageControl>(),
            resolver.Resolve<ProjectsPageControl>(),
            resolver.Resolve<GenerationPageControl>(),
            resolver.Resolve<ValidationPageControl>(),
            resolver.Resolve<RuntimePreviewPageControl>(),
            resolver.Resolve<AssetsPageControl>(),
            resolver.Resolve<SettingsPageControl>()
        }), Reuse.Singleton);

        _container.Register<MainForm>(Reuse.Singleton);
    }

    public MainForm ResolveMainForm() => _container.Resolve<MainForm>();

    public void Dispose()
    {
        _container.Dispose();
        _loggerFactory.Dispose();
    }
}
