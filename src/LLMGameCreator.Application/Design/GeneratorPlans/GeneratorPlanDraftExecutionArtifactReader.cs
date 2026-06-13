using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftExecutionArtifactReader
{
    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanDraftExecutionArtifactReader(IGeneratedArtifactRepository artifactRepository)
    {
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanDraftExecutionArtifactReadResult> ReadLatestAsync(CancellationToken cancellationToken)
    {
        var resultArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanDraftExecutionArtifactIds.ResultArtifactId, cancellationToken)
            .ConfigureAwait(false);

        if (resultArtifact == null)
        {
            return new GeneratorPlanDraftExecutionArtifactReadResult();
        }

        var markdownArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanDraftExecutionArtifactIds.MarkdownArtifactId, cancellationToken)
            .ConfigureAwait(false);

        var validationResults = await _artifactRepository
            .ListValidationResultsByArtifactAsync(resultArtifact.Id, cancellationToken)
            .ConfigureAwait(false);

        return new GeneratorPlanDraftExecutionArtifactReadResult
        {
            Exists = true,
            ResultArtifact = resultArtifact,
            MarkdownArtifact = markdownArtifact,
            ValidationResults = validationResults
        };
    }
}
