using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Projects;

public interface ICurrentGamePackageService
{
    string? CurrentFolder { get; }
    GamePackageDefinition? CurrentPackage { get; }
    event EventHandler? CurrentChanged;
    Task LoadAsync(string projectFolder, CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
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
        CurrentPackage = await _repository.LoadAsync(projectFolder, cancellationToken);
        CurrentFolder = projectFolder;
        CurrentChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (CurrentPackage == null)
        {
            throw new InvalidOperationException("No current game package is loaded.");
        }

        if (string.IsNullOrWhiteSpace(CurrentFolder))
        {
            throw new InvalidOperationException("Current game package folder is not set.");
        }

        await _repository.SaveAsync(CurrentFolder, CurrentPackage, cancellationToken);
        CurrentChanged?.Invoke(this, EventArgs.Empty);
    }
}
