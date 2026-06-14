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

        var content = BuildContent(queueItem, request.SourceContext);
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

    private static JsonObject BuildContent(
        GeneratorPlanDraftArtifactQueueItem queueItem,
        GeneratorPlanDraftArtifactSourceContext sourceContext)
    {
        var artifactKind = string.IsNullOrWhiteSpace(queueItem.ArtifactKind)
            ? queueItem.ExpectedArtifactContract
            : queueItem.ArtifactKind;
        var title = BuildTitle(queueItem, sourceContext);
        var purpose = BuildPurpose(artifactKind, sourceContext);
        var root = new JsonObject
        {
            ["schema_version"] = "0.1",
            ["artifact_id"] = queueItem.ArtifactId,
            ["artifact_kind"] = artifactKind,
            ["expected_artifact_contract"] = queueItem.ExpectedArtifactContract,
            ["title"] = title,
            ["purpose"] = purpose,
            ["source"] = new JsonObject
            {
                ["queue_item_id"] = queueItem.Id,
                ["execution_step_id"] = queueItem.SourceExecutionStepId
            },
            ["source_context"] = BuildSourceContextJson(sourceContext),
            ["draft"] = true
        };

        AddContractFields(root, artifactKind, title, purpose, queueItem, sourceContext);
        return root;
    }

    private static void AddContractFields(
        JsonObject root,
        string artifactKind,
        string title,
        string purpose,
        GeneratorPlanDraftArtifactQueueItem queueItem,
        GeneratorPlanDraftArtifactSourceContext sourceContext)
    {
        var terms = ExtractTerms(sourceContext, title, purpose);

        switch (artifactKind)
        {
            case "game_profile_v1":
                root["game"] = new JsonObject
                {
                    ["title"] = title,
                    ["description"] = purpose,
                    ["genre"] = InferGenre(sourceContext),
                    ["camera"] = InferCamera(sourceContext),
                    ["core_loop"] = ToJsonArray(BuildCoreLoop(terms))
                };
                root["pillars"] = ToJsonArray(BuildPillars(terms));
                break;

            case "semantic_pack_v1":
                root["semantic_groups"] = new JsonArray(new JsonObject
                {
                    ["id"] = "semantic/core",
                    ["title"] = "Core Semantics",
                    ["terms"] = ToJsonArray(terms)
                });
                break;

            case "mechanics_pack_v1":
                var mechanicTitle = BuildMechanicTitle(terms);
                root["mechanics"] = new JsonArray(new JsonObject
                {
                    ["id"] = "mechanic/core_action",
                    ["name"] = mechanicTitle,
                    ["title"] = mechanicTitle,
                    ["description"] = BuildMechanicDescription(purpose, terms),
                    ["inputs"] = ToJsonArray(queueItem.Inputs),
                    ["validation_gates"] = ToJsonArray(queueItem.ValidationGates.Select(gate => gate.GateId))
                });
                break;

            case "scene_pack_v1":
                var sceneTitle = BuildSceneTitle(sourceContext, title);
                root["scenes"] = new JsonArray(new JsonObject
                {
                    ["id"] = "scene/start",
                    ["title"] = sceneTitle,
                    ["description"] = BuildSceneDescription(sceneTitle, purpose)
                });
                break;

            case "entity_pack_v1":
                root["entities"] = new JsonArray(
                    new JsonObject
                    {
                        ["id"] = "entity/player",
                        ["kind"] = "player",
                        ["title"] = BuildPlayerTitle(terms),
                        ["components"] = new JsonArray("identity", "position")
                    },
                    new JsonObject
                    {
                        ["id"] = "entity/guide",
                        ["kind"] = "npc",
                        ["title"] = BuildGuideTitle(terms),
                        ["components"] = new JsonArray("identity", "dialogue_hint")
                    });
                break;

            case "quest_pack_v1":
                var questTitle = BuildQuestTitle(terms);
                root["quests"] = new JsonArray(new JsonObject
                {
                    ["id"] = "quest/intro",
                    ["title"] = questTitle,
                    ["description"] = BuildQuestDescription(questTitle, purpose),
                    ["steps"] = ToJsonArray(BuildQuestSteps(terms))
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

    private static JsonObject BuildSourceContextJson(GeneratorPlanDraftArtifactSourceContext sourceContext)
    {
        var stepTitlesByContract = new JsonObject();
        foreach (var (key, value) in sourceContext.StepTitlesByContract.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            stepTitlesByContract[key] = value;
        }

        var stepTitlesById = new JsonObject();
        foreach (var (key, value) in sourceContext.StepTitlesById.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            stepTitlesById[key] = value;
        }

        return new JsonObject
        {
            ["example_id"] = sourceContext.ExampleId,
            ["title"] = sourceContext.Title,
            ["purpose"] = sourceContext.Purpose,
            ["source_profile_id"] = sourceContext.SourceProfileId,
            ["selected_feature_bundles"] = ToJsonArray(sourceContext.SelectedFeatureBundles),
            ["target_artifacts"] = ToJsonArray(sourceContext.TargetArtifacts),
            ["step_titles_by_contract"] = stepTitlesByContract,
            ["step_titles_by_id"] = stepTitlesById
        };
    }

    private static string BuildTitle(
        GeneratorPlanDraftArtifactQueueItem queueItem,
        GeneratorPlanDraftArtifactSourceContext sourceContext)
    {
        if (!string.IsNullOrWhiteSpace(sourceContext.Title))
        {
            return sourceContext.Title.Trim();
        }

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

    private static string BuildPurpose(string artifactKind, GeneratorPlanDraftArtifactSourceContext sourceContext)
    {
        if (!string.IsNullOrWhiteSpace(sourceContext.Purpose))
        {
            return sourceContext.Purpose.Trim();
        }

        return $"Deterministic draft payload for {artifactKind}.";
    }

    private static JsonArray ToJsonArray(IEnumerable<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(JsonValue.Create(value));
        }

        return array;
    }

    private static string NormalizeArtifactId(string artifactId)
    {
        return GeneratorPlanDraftArtifactProductionPolicy.NormalizeSegment(artifactId).Replace('_', '/');
    }

    private static string InferGenre(GeneratorPlanDraftArtifactSourceContext sourceContext)
    {
        var text = BuildSearchText(sourceContext);
        if (ContainsAny(text, "survival", "craft", "repair"))
        {
            return "cozy_survival_adventure";
        }

        if (ContainsAny(text, "mystery", "investigate", "discover"))
        {
            return "exploration_mystery";
        }

        return "data_driven_adventure";
    }

    private static string InferCamera(GeneratorPlanDraftArtifactSourceContext sourceContext)
    {
        var text = BuildSearchText(sourceContext);
        return ContainsAny(text, "side", "platform") ? "side_view" : "top_down";
    }

    private static IReadOnlyList<string> BuildCoreLoop(IReadOnlyList<string> terms)
    {
        var actions = new List<string> { "explore" };

        if (terms.Any(term => term is "repair" or "restore" or "lantern" or "tower"))
        {
            actions.Add("repair");
        }
        else if (terms.Any(term => term is "collect" or "resource" or "survival"))
        {
            actions.Add("collect");
        }

        actions.Add(terms.Any(term => term is "discover" or "mystery" or "storm") ? "discover" : "resolve");
        return actions.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static IReadOnlyList<string> BuildPillars(IReadOnlyList<string> terms)
    {
        var pillars = new List<string> { "clear_goals", "readable_state" };
        if (terms.Any(term => term is "cozy" or "lantern" or "repair"))
        {
            pillars.Add("meaningful_repair");
        }
        else
        {
            pillars.Add("safe_iteration");
        }

        return pillars;
    }

    private static string BuildSceneTitle(GeneratorPlanDraftArtifactSourceContext sourceContext, string fallbackTitle)
    {
        if (!string.IsNullOrWhiteSpace(sourceContext.Title))
        {
            return sourceContext.Title.Trim();
        }

        return sourceContext.StepTitlesByContract.TryGetValue("scene_pack_v1", out var stepTitle) && !string.IsNullOrWhiteSpace(stepTitle)
            ? stepTitle.Trim()
            : fallbackTitle;
    }

    private static string BuildSceneDescription(string sceneTitle, string purpose)
    {
        return string.IsNullOrWhiteSpace(purpose)
            ? $"Starting scene for {sceneTitle}."
            : $"Starting scene for {sceneTitle}: {purpose}";
    }

    private static string BuildPlayerTitle(IReadOnlyList<string> terms)
    {
        return terms.Contains("lantern", StringComparer.OrdinalIgnoreCase) ? "Lantern Keeper" : "Keeper";
    }

    private static string BuildGuideTitle(IReadOnlyList<string> terms)
    {
        var noun = terms.FirstOrDefault(term => term is "lantern" or "outpost" or "tower" or "storm");
        return string.IsNullOrWhiteSpace(noun) ? "Field Guide" : $"{ToDisplayWord(noun)} Guide";
    }

    private static string BuildQuestTitle(IReadOnlyList<string> terms)
    {
        if (terms.Contains("lantern", StringComparer.OrdinalIgnoreCase))
        {
            return "Restore the First Lantern";
        }

        if (terms.Contains("outpost", StringComparer.OrdinalIgnoreCase))
        {
            return "Secure the First Outpost";
        }

        return "Begin the First Expedition";
    }

    private static string BuildQuestDescription(string questTitle, string purpose)
    {
        return string.IsNullOrWhiteSpace(purpose)
            ? $"Intro quest: {questTitle}."
            : $"{questTitle}: {purpose}";
    }

    private static IReadOnlyList<string> BuildQuestSteps(IReadOnlyList<string> terms)
    {
        if (terms.Contains("lantern", StringComparer.OrdinalIgnoreCase))
        {
            return ["Inspect the outpost", "Find the first lantern part", "Return to the guide"];
        }

        return ["Inspect the starting area", "Find a useful clue", "Return to the guide"];
    }

    private static string BuildMechanicTitle(IReadOnlyList<string> terms)
    {
        if (terms.Contains("lantern", StringComparer.OrdinalIgnoreCase))
        {
            return "Lantern Repair";
        }

        if (terms.Contains("survival", StringComparer.OrdinalIgnoreCase))
        {
            return "Resource Recovery";
        }

        return "Focused Action";
    }

    private static string BuildMechanicDescription(string purpose, IReadOnlyList<string> terms)
    {
        var focus = terms.Contains("lantern", StringComparer.OrdinalIgnoreCase) ? "repair lantern devices" : "advance the core loop";
        return string.IsNullOrWhiteSpace(purpose)
            ? $"Allows the player to {focus}."
            : $"Allows the player to {focus}: {purpose}";
    }

    private static IReadOnlyList<string> ExtractTerms(
        GeneratorPlanDraftArtifactSourceContext sourceContext,
        string title,
        string purpose)
    {
        var values = new List<string> { title, purpose, sourceContext.ExampleId, sourceContext.SourceProfileId };
        values.AddRange(sourceContext.SelectedFeatureBundles);
        values.AddRange(sourceContext.TargetArtifacts);

        var terms = values
            .SelectMany(SplitTerms)
            .Where(term => term.Length >= 3)
            .Where(term => !StopWords.Contains(term))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(12)
            .ToList();

        return terms.Count == 0 ? ["player", "world", "objective"] : terms;
    }

    private static IEnumerable<string> SplitTerms(string value)
    {
        var chars = value.ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : ' ')
            .ToArray();

        return new string(chars)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static string BuildSearchText(GeneratorPlanDraftArtifactSourceContext sourceContext)
    {
        return string.Join(' ', sourceContext.Title, sourceContext.Purpose, string.Join(' ', sourceContext.SelectedFeatureBundles)).ToLowerInvariant();
    }

    private static bool ContainsAny(string text, params string[] terms)
    {
        return terms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static string ToDisplayWord(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
    }

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "about",
        "after",
        "and",
        "for",
        "from",
        "game",
        "profile",
        "test",
        "the",
        "v1",
        "with"
    };
}
