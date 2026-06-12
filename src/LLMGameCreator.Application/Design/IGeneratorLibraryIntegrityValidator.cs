namespace LLMGameCreator.Application.Design;

public interface IGeneratorLibraryIntegrityValidator
{
    Task<GeneratorLibraryIntegrityReport> ValidateAsync(
        string repositoryRootOrLibraryRoot,
        CancellationToken cancellationToken);
}
