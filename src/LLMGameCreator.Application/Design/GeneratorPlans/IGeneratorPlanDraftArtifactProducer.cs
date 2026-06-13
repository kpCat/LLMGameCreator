namespace LLMGameCreator.Application.Design.GeneratorPlans;

public interface IGeneratorPlanDraftArtifactProducer
{
    Task<GeneratorPlanProducedDraftArtifact> ProduceAsync(
        GeneratorPlanDraftArtifactQueueItem queueItem,
        GeneratorPlanDraftArtifactProductionRequest request,
        CancellationToken cancellationToken = default);
}
