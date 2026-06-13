using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanApprovedArtifactSetReader
{
    public GeneratorPlanApprovedArtifactSet Read(GeneratedArtifactRecord artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        return ReadJson(artifact.Json);
    }

    public GeneratorPlanApprovedArtifactSet ReadJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("Approved artifact set JSON is required.", nameof(json));
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var artifacts = new List<GeneratorPlanApprovedArtifact>();

            if (TryGetProperty(root, "approved_artifacts", out var approvedArtifacts)
                && approvedArtifacts.ValueKind == JsonValueKind.Array)
            {
                foreach (var artifact in approvedArtifacts.EnumerateArray())
                {
                    artifacts.Add(new GeneratorPlanApprovedArtifact
                    {
                        ArtifactId = GetString(artifact, "artifact_id"),
                        ArtifactKind = GetString(artifact, "artifact_kind"),
                        ExpectedArtifactContract = GetString(artifact, "expected_artifact_contract"),
                        ContentJson = ReadContentJson(artifact)
                    });
                }
            }

            return new GeneratorPlanApprovedArtifactSet
            {
                SchemaVersion = GetString(root, "schema_version"),
                SnapshotId = GetString(root, "snapshot_id"),
                SourceProductionBatchId = GetString(root, "source_production_batch_id"),
                ApprovedArtifacts = artifacts
            };
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("Approved artifact set JSON is invalid.", nameof(json), exception);
        }
    }

    private static string ReadContentJson(JsonElement artifact)
    {
        if (!TryGetProperty(artifact, "content_json", out var content))
        {
            return "{}";
        }

        if (content.ValueKind == JsonValueKind.String)
        {
            return content.GetString() ?? "{}";
        }

        return content.GetRawText();
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return TryGetProperty(element, propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.TryGetProperty(propertyName, out value))
        {
            return true;
        }

        var pascal = string.Concat(propertyName
            .Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
        return element.TryGetProperty(pascal, out value);
    }
}
