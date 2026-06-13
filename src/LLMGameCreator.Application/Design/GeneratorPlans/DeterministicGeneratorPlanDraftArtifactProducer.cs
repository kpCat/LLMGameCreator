using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class DeterministicGeneratorPlanDraftArtifactProducer : IGeneratorPlanDraftArtifactProducer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    public Task<GeneratorPlanProducedDraftArtifact> ProduceAsync(
        GeneratorPlanDraftArtifactQueueItem queueItem,
        GeneratorPlanDraftArtifactProductionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(queueItem);
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var content = BuildContent(queueItem);
        var artifact = new GeneratorPlanProducedDraftArtifact
        {
            Id = BuildProducedId(request.BatchId, queueItem),
            QueueItemId = queueItem.Id,
            SourceExecutionStepId = queueItem.SourceExecutionStepId,
            ArtifactId = queueItem.ArtifactId,
            ArtifactKind = queueItem.ArtifactKind,
            ExpectedArtifactContract = queueItem.ExpectedArtifactContract,
            State = GeneratorPlanProducedDraftArtifactState.ReadyForApproval,
            ContentJson = content.ToJsonString(JsonOptions),
            ValidationGates = queueItem.ValidationGates.Select(gate => gate.GateId).ToList(),
            RequiresHumanApproval = request.RequireHumanApprovalByDefault || queueItem.RequiresHumanApproval
        };

        return Task.FromResult(artifact);
    }

    private static string BuildProducedId(string? batchId, GeneratorPlanDraftArtifactQueueItem queueItem)
    {
        var batch = string.IsNullOrWhiteSpace(batchId) ? "draft_artifact_production" : batchId.Trim();
        var artifact = string.IsNullOrWhiteSpace(queueItem.ArtifactId) ? queueItem.Id : queueItem.ArtifactId;
        return $"{batch}/produced/{GeneratorPlanDraftArtifactProductionPolicy.NormalizeSegment(artifact)}";
    }

    private static JsonObject BuildContent(GeneratorPlanDraftArtifactQueueItem queueItem)
    {
        var artifactKind = string.IsNullOrWhiteSpace(queueItem.ArtifactKind)
            ? queueItem.ExpectedArtifactContract
            : queueItem.ArtifactKind;
        var title = BuildTitle(queueItem);
        var root = new JsonObject
        {
            ["schema_version"] = "0.1",
            ["artifact_id"] = queueItem.ArtifactId,
            ["artifact_kind"] = artifactKind,
            ["expected_artifact_contract"] = queueItem.ExpectedArtifactContract,
            ["title"] = title,
            ["purpose"] = $"Deterministic draft payload for {artifactKind}.",
            ["source"] = new JsonObject
            {
                ["queue_item_id"] = queueItem.Id,
                ["execution_step_id"] = queueItem.SourceExecutionStepId
            },
            ["draft"] = true
        };

        AddContractFields(root, artifactKind, title, queueItem);
        return root;
    }

    private static void AddContractFields(JsonObject root, string artifactKind, string title, GeneratorPlanDraftArtifactQueueItem queueItem)
    {
        switch (artifactKind)
        {
            case "game_profile_v1":
                root["game"] = new JsonObject
                {
                    ["title"] = title,
                    ["genre"] = "data_driven_adventure",
                    ["camera"] = "top_down",
                    ["core_loop"] = "explore_collect_resolve"
                };
                root["pillars"] = new JsonArray("clear_goals", "readable_state", "safe_iteration");
                break;

            case "semantic_pack_v1":
                root["semantic_groups"] = new JsonArray(new JsonObject
                {
                    ["id"] = $"{NormalizeArtifactId(queueItem.ArtifactId)}/semantic_group/core",
                    ["title"] = "Core Semantics",
                    ["terms"] = new JsonArray("player", "world", "objective")
                });
                break;

            case "mechanics_pack_v1":
                root["mechanics"] = new JsonArray(new JsonObject
                {
                    ["id"] = $"{NormalizeArtifactId(queueItem.ArtifactId)}/mechanic/core_loop",
                    ["title"] = "Core Loop",
                    ["inputs"] = new JsonArray(queueItem.Inputs.Select(input => JsonValue.Create(input)).ToArray()),
                    ["validation_gates"] = new JsonArray(queueItem.ValidationGates.Select(gate => JsonValue.Create(gate.GateId)).ToArray())
                });
                break;

            case "scene_pack_v1":
                root["scenes"] = new JsonArray(new JsonObject
                {
                    ["id"] = $"{NormalizeArtifactId(queueItem.ArtifactId)}/scene/start",
                    ["title"] = "Start Scene",
                    ["purpose"] = "Introduce the draft play space."
                });
                break;

            case "entity_pack_v1":
                root["entities"] = new JsonArray(new JsonObject
                {
                    ["id"] = $"{NormalizeArtifactId(queueItem.ArtifactId)}/entity/player",
                    ["kind"] = "player",
                    ["components"] = new JsonArray("identity", "position")
                });
                break;

            case "quest_pack_v1":
                root["quests"] = new JsonArray(new JsonObject
                {
                    ["id"] = $"{NormalizeArtifactId(queueItem.ArtifactId)}/quest/first_goal",
                    ["title"] = "First Goal",
                    ["objectives"] = new JsonArray("inspect_start_scene")
                });
                break;

            default:
                root["draft_sections"] = new JsonArray(new JsonObject
                {
                    ["id"] = $"{NormalizeArtifactId(queueItem.ArtifactId)}/section/summary",
                    ["title"] = "Draft Summary",
                    ["notes"] = "Generic deterministic draft envelope for an unknown artifact kind."
                });
                break;
        }
    }

    private static string BuildTitle(GeneratorPlanDraftArtifactQueueItem queueItem)
    {
        if (!string.IsNullOrWhiteSpace(queueItem.ArtifactKind))
        {
            return queueItem.ArtifactKind.Replace('_', ' ');
        }

        if (!string.IsNullOrWhiteSpace(queueItem.ExpectedArtifactContract))
        {
            return queueItem.ExpectedArtifactContract.Replace('_', ' ');
        }

        return "Draft artifact";
    }

    private static string NormalizeArtifactId(string artifactId)
    {
        return GeneratorPlanDraftArtifactProductionPolicy.NormalizeSegment(artifactId).Replace('_', '/');
    }
}
