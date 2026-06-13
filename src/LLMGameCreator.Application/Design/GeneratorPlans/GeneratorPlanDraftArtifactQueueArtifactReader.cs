using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftArtifactQueueArtifactReader
{
    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanDraftArtifactQueueArtifactReader(IGeneratedArtifactRepository artifactRepository)
    {
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanDraftArtifactQueueArtifactReadResult> ReadLatestAsync(CancellationToken cancellationToken)
    {
        var resultArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanDraftArtifactQueueArtifactIds.ResultArtifactId, cancellationToken)
            .ConfigureAwait(false);

        if (resultArtifact == null)
        {
            return new GeneratorPlanDraftArtifactQueueArtifactReadResult();
        }

        var markdownArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanDraftArtifactQueueArtifactIds.MarkdownArtifactId, cancellationToken)
            .ConfigureAwait(false);

        var validationResults = await _artifactRepository
            .ListValidationResultsByArtifactAsync(resultArtifact.Id, cancellationToken)
            .ConfigureAwait(false);

        return new GeneratorPlanDraftArtifactQueueArtifactReadResult
        {
            Exists = true,
            ResultArtifact = resultArtifact,
            MarkdownArtifact = markdownArtifact,
            ValidationResults = validationResults
        };
    }
}
