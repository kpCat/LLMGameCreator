using System.Text.Json;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanStrictLlmArtifactGenerationArtifactReader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanStrictLlmArtifactGenerationArtifactReader(IGeneratedArtifactRepository artifactRepository)
    {
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanStrictLlmArtifactGenerationArtifactReadResult> ReadLatestAsync(CancellationToken cancellationToken = default)
    {
        var artifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanStrictLlmArtifactGenerationArtifactIds.GenerationArtifactId, cancellationToken)
            .ConfigureAwait(false);

        if (artifact == null)
        {
            return new GeneratorPlanStrictLlmArtifactGenerationArtifactReadResult();
        }

        var validationResults = await _artifactRepository
            .ListValidationResultsByArtifactAsync(artifact.Id, cancellationToken)
            .ConfigureAwait(false);

        return new GeneratorPlanStrictLlmArtifactGenerationArtifactReadResult
        {
            Exists = true,
            GenerationArtifact = artifact,
            Result = ReadResult(artifact.Json),
            ValidationResults = validationResults
        };
    }

    private static GeneratorPlanStrictLlmArtifactGenerationResult ReadResult(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new GeneratorPlanStrictLlmArtifactGenerationResult();
        }

        try
        {
            var snapshot = JsonSerializer.Deserialize<GeneratorPlanStrictLlmArtifactGenerationAuditSnapshot>(json, JsonOptions);
            if (snapshot == null)
            {
                return new GeneratorPlanStrictLlmArtifactGenerationResult();
            }

            return new GeneratorPlanStrictLlmArtifactGenerationResult
            {
                Ok = snapshot.Ok,
                Status = snapshot.Status,
                GeneratedAtUtc = snapshot.GeneratedAtUtc,
                SourceCapabilitySelectionId = snapshot.SourceCapabilitySelectionId,
                RequestedContractIds = snapshot.RequestedContractIds,
                Artifacts = snapshot.GeneratedArtifacts.Select(artifact => new GeneratorPlanStrictLlmGeneratedArtifact
                {
                    ArtifactId = artifact.ArtifactId,
                    ArtifactKind = artifact.ArtifactKind,
                    ExpectedArtifactContract = artifact.ExpectedArtifactContract,
                    Valid = artifact.Valid,
                    Repaired = artifact.Repaired,
                    RequiresHumanApproval = artifact.RequiresHumanApproval
                }).ToList(),
                Attempts = snapshot.Attempts,
                Diagnostics = snapshot.Diagnostics
            };
        }
        catch (JsonException)
        {
            return new GeneratorPlanStrictLlmArtifactGenerationResult();
        }
    }

    private sealed record GeneratorPlanStrictLlmArtifactGenerationAuditSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public bool Ok { get; init; }
        public string Status { get; init; } = string.Empty;
        public string SourceCapabilitySelectionId { get; init; } = string.Empty;
        public IReadOnlyList<string> RequestedContractIds { get; init; } = Array.Empty<string>();
        public IReadOnlyList<GeneratorPlanStrictLlmArtifactAuditItem> GeneratedArtifacts { get; init; } = Array.Empty<GeneratorPlanStrictLlmArtifactAuditItem>();
        public IReadOnlyList<GeneratorPlanStrictLlmArtifactGenerationAttempt> Attempts { get; init; } = Array.Empty<GeneratorPlanStrictLlmArtifactGenerationAttempt>();
        public IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanStrictLlmArtifactDiagnostic>();
    }

    private sealed record GeneratorPlanStrictLlmArtifactAuditItem
    {
        public string ArtifactId { get; init; } = string.Empty;
        public string ArtifactKind { get; init; } = string.Empty;
        public string ExpectedArtifactContract { get; init; } = string.Empty;
        public bool Valid { get; init; }
        public bool Repaired { get; init; }
        public bool RequiresHumanApproval { get; init; }
    }
}
