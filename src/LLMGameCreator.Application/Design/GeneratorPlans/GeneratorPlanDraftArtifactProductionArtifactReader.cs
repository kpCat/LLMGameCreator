using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftArtifactProductionArtifactReader
{
    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanDraftArtifactProductionArtifactReader(IGeneratedArtifactRepository artifactRepository)
    {
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanDraftArtifactProductionArtifactReadResult> ReadLatestAsync(CancellationToken cancellationToken)
    {
        var batchArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanDraftArtifactProductionArtifactIds.BatchArtifactId, cancellationToken)
            .ConfigureAwait(false);

        if (batchArtifact == null)
        {
            return new GeneratorPlanDraftArtifactProductionArtifactReadResult();
        }

        var markdownArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanDraftArtifactProductionArtifactIds.MarkdownArtifactId, cancellationToken)
            .ConfigureAwait(false);
        var batchValidationResults = await _artifactRepository
            .ListValidationResultsByArtifactAsync(batchArtifact.Id, cancellationToken)
            .ConfigureAwait(false);
        var worklist = ReadWorklist(batchArtifact.Json);
        var producedArtifacts = new List<GeneratedArtifactRecord>();
        var validationResults = new List<GeneratedArtifactValidationResultRecord>(batchValidationResults);

        foreach (var item in worklist)
        {
            var artifact = await _artifactRepository
                .GetGeneratedArtifactByIdAsync(item.ArtifactId, cancellationToken)
                .ConfigureAwait(false);

            if (artifact == null)
            {
                continue;
            }

            producedArtifacts.Add(artifact);
            validationResults.AddRange(await _artifactRepository
                .ListValidationResultsByArtifactAsync(artifact.Id, cancellationToken)
                .ConfigureAwait(false));
        }

        return new GeneratorPlanDraftArtifactProductionArtifactReadResult
        {
            Exists = true,
            BatchArtifact = batchArtifact,
            MarkdownArtifact = markdownArtifact,
            ProducedArtifacts = producedArtifacts,
            ValidationResults = validationResults,
            Worklist = worklist
        };
    }

    private static IReadOnlyList<GeneratorPlanDraftArtifactWorklistItem> ReadWorklist(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<GeneratorPlanDraftArtifactWorklistItem>();
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (!TryGetProperty(document.RootElement, "Batch", out var batch)
                || !TryGetProperty(batch, "Artifacts", out var artifacts)
                || artifacts.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<GeneratorPlanDraftArtifactWorklistItem>();
            }

            var worklist = new List<GeneratorPlanDraftArtifactWorklistItem>();
            foreach (var artifact in artifacts.EnumerateArray())
            {
                worklist.Add(new GeneratorPlanDraftArtifactWorklistItem
                {
                    ArtifactId = GetString(artifact, "ArtifactId"),
                    ArtifactKind = GetString(artifact, "ArtifactKind"),
                    State = GetString(artifact, "State"),
                    RequiresHumanApproval = GetBool(artifact, "RequiresHumanApproval"),
                    RepairRequestId = GetString(artifact, "RepairRequestId")
                });
            }

            return worklist;
        }
        catch (JsonException)
        {
            return Array.Empty<GeneratorPlanDraftArtifactWorklistItem>();
        }
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        return element.TryGetProperty(propertyName, out value)
            || element.TryGetProperty(char.ToLowerInvariant(propertyName[0]) + propertyName[1..], out value);
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return TryGetProperty(element, propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static bool GetBool(JsonElement element, string propertyName)
    {
        return TryGetProperty(element, propertyName, out var property)
            && property.ValueKind == JsonValueKind.True;
    }
}
