using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Projects;

public interface ICurrentGamePackageService
{
    string? CurrentFolder { get; }
    GamePackageDefinition? CurrentPackage { get; }
    event EventHandler? CurrentChanged;
    Task LoadAsync(string projectFolder, CancellationToken cancellationToken);
}

public sealed class CurrentGamePackageService : ICurrentGamePackageService
{
    private readonly IGamePackageRepository _repository;

    public CurrentGamePackageService(IGamePackageRepository repository)
    {
        _repository = repository;
    }

    public string? CurrentFolder { get; private set; }
    public GamePackageDefinition? CurrentPackage { get; private set; }
    public event EventHandler? CurrentChanged;

    public async Task LoadAsync(string projectFolder, CancellationToken cancellationToken)
    {
        CurrentPackage = await _repository.LoadAsync(projectFolder, cancellationToken).ConfigureAwait(false);
        CurrentFolder = projectFolder;
        CurrentChanged?.Invoke(this, EventArgs.Empty);
    }
}
