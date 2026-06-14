using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanCapabilitySelectionArtifactReader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanCapabilitySelectionArtifactReader(IGeneratedArtifactRepository artifactRepository)
    {
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanCapabilitySelectionArtifactReadResult> ReadLatestAsync(CancellationToken cancellationToken)
    {
        var artifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanCapabilitySelectionArtifactIds.SelectionArtifactId, cancellationToken)
            .ConfigureAwait(false);

        if (artifact == null)
        {
            return new GeneratorPlanCapabilitySelectionArtifactReadResult();
        }

        var validationResults = await _artifactRepository
            .ListValidationResultsByArtifactAsync(artifact.Id, cancellationToken)
            .ConfigureAwait(false);

        return new GeneratorPlanCapabilitySelectionArtifactReadResult
        {
            Exists = true,
            SelectionArtifact = artifact,
            Selection = ReadSelection(artifact.Json),
            ValidationResults = validationResults
        };
    }

    private static GeneratorPlanCapabilitySelection ReadSelection(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new GeneratorPlanCapabilitySelection();
        }

        try
        {
            return JsonSerializer.Deserialize<GeneratorPlanCapabilitySelection>(json, JsonOptions) ?? new GeneratorPlanCapabilitySelection();
        }
        catch (JsonException)
        {
            return new GeneratorPlanCapabilitySelection();
        }
    }
}
