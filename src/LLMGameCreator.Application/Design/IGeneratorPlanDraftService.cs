namespace LLMGameCreator.Application.Design;

public interface IGeneratorPlanDraftService
{
    Task<GeneratorPlanDraftResult> CreateDraftPlanAsync(GeneratorPlanDraftRequest request, CancellationToken cancellationToken);
}
