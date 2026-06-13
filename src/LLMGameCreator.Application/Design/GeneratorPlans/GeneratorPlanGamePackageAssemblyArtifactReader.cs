using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanGamePackageAssemblyArtifactReader
{
    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanGamePackageAssemblyArtifactReader(IGeneratedArtifactRepository artifactRepository)
    {
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanGamePackageAssemblyArtifactReadResult> ReadLatestAsync(CancellationToken cancellationToken)
    {
        var assemblyArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanGamePackageAssemblyArtifactIds.AssemblyArtifactId, cancellationToken)
            .ConfigureAwait(false);

        if (assemblyArtifact == null)
        {
            return new GeneratorPlanGamePackageAssemblyArtifactReadResult();
        }

        var packageDraftArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanGamePackageAssemblyArtifactIds.PackageDraftArtifactId, cancellationToken)
            .ConfigureAwait(false);
        var markdownArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanGamePackageAssemblyArtifactIds.MarkdownArtifactId, cancellationToken)
            .ConfigureAwait(false);
        var validationResults = new List<GeneratedArtifactValidationResultRecord>();
        validationResults.AddRange(await _artifactRepository
            .ListValidationResultsByArtifactAsync(assemblyArtifact.Id, cancellationToken)
            .ConfigureAwait(false));

        if (packageDraftArtifact != null)
        {
            validationResults.AddRange(await _artifactRepository
                .ListValidationResultsByArtifactAsync(packageDraftArtifact.Id, cancellationToken)
                .ConfigureAwait(false));
        }

        return new GeneratorPlanGamePackageAssemblyArtifactReadResult
        {
            Exists = true,
            AssemblyArtifact = assemblyArtifact,
            PackageDraftArtifact = packageDraftArtifact,
            MarkdownArtifact = markdownArtifact,
            ValidationResults = validationResults
        };
    }
}
