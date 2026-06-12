namespace LLMGameCreator.Application.Design;

public interface IGeneratorPlanPreviewService
{
    Task<GeneratorPlanPreviewResult> CreatePreviewArtifactAsync(GeneratorPlanPreviewRequest request, CancellationToken cancellationToken);
}
