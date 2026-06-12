namespace LLMGameCreator.Application.Design;

public interface IGeneratorPlanPipelineService
{
    Task<GeneratorPlanPipelineResult> PreparePatchPipelineAsync(string planId, CancellationToken cancellationToken);
    Task<GamePackagePatchApplyResult> ApplyPreparedPatchAsync(string patchArtifactId, CancellationToken cancellationToken);
}
