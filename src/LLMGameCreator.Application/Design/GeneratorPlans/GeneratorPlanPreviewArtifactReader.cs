using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanPreviewArtifactReader
{
    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanPreviewArtifactReader(IGeneratedArtifactRepository artifactRepository)
    {
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanPreviewArtifactReadResult> ReadLatestAsync(CancellationToken cancellationToken)
    {
        var resultArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanPreviewArtifactIds.ResultArtifactId, cancellationToken)
            .ConfigureAwait(false);

        if (resultArtifact == null)
        {
            return new GeneratorPlanPreviewArtifactReadResult();
        }

        var markdownArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanPreviewArtifactIds.MarkdownArtifactId, cancellationToken)
            .ConfigureAwait(false);

        var validationResults = await _artifactRepository
            .ListValidationResultsByArtifactAsync(resultArtifact.Id, cancellationToken)
            .ConfigureAwait(false);

        return new GeneratorPlanPreviewArtifactReadResult
        {
            Exists = true,
            ResultArtifact = resultArtifact,
            MarkdownArtifact = markdownArtifact,
            ValidationResults = validationResults
        };
    }
}
