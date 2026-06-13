using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanPackageExportRunArtifactReader
{
    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanPackageExportRunArtifactReader(IGeneratedArtifactRepository artifactRepository)
    {
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanPackageExportRunArtifactReadResult> ReadLatestAsync(CancellationToken cancellationToken)
    {
        var runArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanPackageExportRunArtifactIds.RunArtifactId, cancellationToken)
            .ConfigureAwait(false);

        if (runArtifact == null)
        {
            return new GeneratorPlanPackageExportRunArtifactReadResult();
        }

        var markdownArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanPackageExportRunArtifactIds.MarkdownArtifactId, cancellationToken)
            .ConfigureAwait(false);
        var validationResults = await _artifactRepository
            .ListValidationResultsByArtifactAsync(runArtifact.Id, cancellationToken)
            .ConfigureAwait(false);

        return new GeneratorPlanPackageExportRunArtifactReadResult
        {
            Exists = true,
            RunArtifact = runArtifact,
            MarkdownArtifact = markdownArtifact,
            ValidationResults = validationResults
        };
    }
}
