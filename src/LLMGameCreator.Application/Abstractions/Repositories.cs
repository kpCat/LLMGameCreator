using LLMGameCreator.Application.Settings;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Abstractions;

public interface IGamePackageRepository
{
    Task<GamePackageDefinition> LoadAsync(string projectFolder, CancellationToken cancellationToken);
    Task SaveAsync(string projectFolder, GamePackageDefinition package, CancellationToken cancellationToken);
}

public interface IAppSettingsRepository
{
    Task<AppSettings> LoadAsync(CancellationToken cancellationToken);
    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken);
}
