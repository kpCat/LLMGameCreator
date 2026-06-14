using System.Text.Json;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanStrictLlmEvaluationArtifactReader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanStrictLlmEvaluationArtifactReader(IGeneratedArtifactRepository artifactRepository)
    {
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanStrictLlmEvaluationArtifactReadResult> ReadLatestAsync(CancellationToken cancellationToken = default)
    {
        var evaluationArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanStrictLlmEvaluationArtifactIds.EvaluationArtifactId, cancellationToken)
            .ConfigureAwait(false);

        if (evaluationArtifact == null)
        {
            return new GeneratorPlanStrictLlmEvaluationArtifactReadResult();
        }

        var markdownArtifact = await _artifactRepository
            .GetGeneratedArtifactByIdAsync(GeneratorPlanStrictLlmEvaluationArtifactIds.MarkdownArtifactId, cancellationToken)
            .ConfigureAwait(false);
        var validationResults = await _artifactRepository
            .ListValidationResultsByArtifactAsync(evaluationArtifact.Id, cancellationToken)
            .ConfigureAwait(false);

        return new GeneratorPlanStrictLlmEvaluationArtifactReadResult
        {
            Exists = true,
            EvaluationArtifact = evaluationArtifact,
            MarkdownArtifact = markdownArtifact,
            Result = ReadResult(evaluationArtifact.Json),
            MarkdownReport = ReadMarkdown(markdownArtifact?.Json),
            ValidationResults = validationResults
        };
    }

    private static GeneratorPlanStrictLlmEvaluationResult ReadResult(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new GeneratorPlanStrictLlmEvaluationResult();
        }

        try
        {
            return JsonSerializer.Deserialize<GeneratorPlanStrictLlmEvaluationResult>(json, JsonOptions)
                ?? new GeneratorPlanStrictLlmEvaluationResult();
        }
        catch (JsonException)
        {
            return new GeneratorPlanStrictLlmEvaluationResult();
        }
    }

    private static string ReadMarkdown(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return TryGetProperty(document.RootElement, "Markdown", out var markdown) && markdown.ValueKind == JsonValueKind.String
                ? markdown.GetString() ?? string.Empty
                : string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.TryGetProperty(propertyName, out value))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return false;
        }

        var camel = char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
        return element.TryGetProperty(camel, out value);
    }
}
