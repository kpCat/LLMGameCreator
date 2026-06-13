using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftArtifactApprovalArtifactReader
{
    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanDraftArtifactApprovalArtifactReader(IGeneratedArtifactRepository artifactRepository)
    {
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanDraftArtifactApprovalArtifactReadResult> ReadLatestAsync(CancellationToken cancellationToken)
    {
        var stagingArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanDraftArtifactApprovalArtifactIds.StagingArtifactId, cancellationToken)
            .ConfigureAwait(false);

        if (stagingArtifact == null)
        {
            return new GeneratorPlanDraftArtifactApprovalArtifactReadResult();
        }

        var markdownArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanDraftArtifactApprovalArtifactIds.MarkdownArtifactId, cancellationToken)
            .ConfigureAwait(false);
        var approvedArtifactSetArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanDraftArtifactApprovalArtifactIds.ApprovedArtifactSetArtifactId, cancellationToken)
            .ConfigureAwait(false);
        var validationResults = await _artifactRepository
            .ListValidationResultsByArtifactAsync(stagingArtifact.Id, cancellationToken)
            .ConfigureAwait(false);

        return new GeneratorPlanDraftArtifactApprovalArtifactReadResult
        {
            Exists = true,
            StagingArtifact = stagingArtifact,
            MarkdownArtifact = markdownArtifact,
            ApprovedArtifactSetArtifact = approvedArtifactSetArtifact,
            ValidationResults = validationResults,
            Worklist = ReadWorklist(stagingArtifact.Json)
        };
    }

    private static IReadOnlyList<GeneratorPlanDraftArtifactApprovalWorklistItem> ReadWorklist(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<GeneratorPlanDraftArtifactApprovalWorklistItem>();
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (!TryGetProperty(document.RootElement, "Snapshot", out var snapshot)
                || !TryGetProperty(snapshot, "Items", out var items)
                || items.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<GeneratorPlanDraftArtifactApprovalWorklistItem>();
            }

            var worklist = new List<GeneratorPlanDraftArtifactApprovalWorklistItem>();
            foreach (var item in items.EnumerateArray())
            {
                worklist.Add(new GeneratorPlanDraftArtifactApprovalWorklistItem
                {
                    ArtifactId = GetString(item, "ArtifactId"),
                    ArtifactKind = GetString(item, "ArtifactKind"),
                    State = GetString(item, "State"),
                    RequiresHumanApproval = GetBool(item, "RequiresHumanApproval"),
                    RepairRequestId = GetString(item, "RepairRequestId"),
                    ReasonCode = GetString(item, "DecisionReasonCode")
                });
            }

            return worklist;
        }
        catch (JsonException)
        {
            return Array.Empty<GeneratorPlanDraftArtifactApprovalWorklistItem>();
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
