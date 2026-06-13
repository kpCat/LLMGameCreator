using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.Atlas;

public sealed class AtlasRegistryPreviewArtifactReader
{
    private readonly IGeneratedArtifactRepository _artifactRepository;

    public AtlasRegistryPreviewArtifactReader(IGeneratedArtifactRepository artifactRepository)
    {
        _artifactRepository = artifactRepository;
    }

    public async Task<AtlasRegistryPreviewArtifactReadResult> ReadLatestAsync(CancellationToken cancellationToken)
    {
        var resultArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(AtlasRegistryPreviewArtifactIds.ResultArtifactId, cancellationToken)
            .ConfigureAwait(false);

        if (resultArtifact == null)
        {
            return new AtlasRegistryPreviewArtifactReadResult();
        }

        var markdownArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(AtlasRegistryPreviewArtifactIds.MarkdownArtifactId, cancellationToken)
            .ConfigureAwait(false);

        var validationResults = await _artifactRepository
            .ListValidationResultsByArtifactAsync(resultArtifact.Id, cancellationToken)
            .ConfigureAwait(false);

        return new AtlasRegistryPreviewArtifactReadResult
        {
            Exists = true,
            ResultArtifact = resultArtifact,
            MarkdownArtifact = markdownArtifact,
            ValidationResults = validationResults
        };
    }
}
