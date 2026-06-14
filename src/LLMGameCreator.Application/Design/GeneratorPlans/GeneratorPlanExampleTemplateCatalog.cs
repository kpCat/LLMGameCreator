using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanExampleTemplateCatalog
{
    public const string GameProfileArtifact = "game_profile_v1";
    public const string ScenePackArtifact = "scene_pack_v1";
    public const string EntityPackArtifact = "entity_pack_v1";
    public const string QuestPackArtifact = "quest_pack_v1";
    public const string MechanicsPackArtifact = "mechanics_pack_v1";
    public const string SemanticPackArtifact = "semantic_pack_v1";

    private static readonly IReadOnlyList<string> DefaultTargetArtifacts =
    [
        GameProfileArtifact,
        ScenePackArtifact,
        EntityPackArtifact,
        QuestPackArtifact,
        MechanicsPackArtifact,
        SemanticPackArtifact
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    private readonly IReadOnlyList<GeneratorPlanExampleTemplate> _templates;

    public GeneratorPlanExampleTemplateCatalog()
    {
        _templates =
        [
            BuildTemplate(new TemplateSeed(
                "sky-lantern-outpost",
                "Sky Lantern Outpost",
                "Cozy Survival",
                "A cozy survival repair adventure about restoring lantern towers after a storm.",
                "profile/sky-lantern-outpost/v1",
                ["feature_bundle/cozy_survival/v1", "feature_bundle/lantern_repair/v1"],
                "Define Sky Lantern Outpost profile",
                "Build the lantern outpost start scene",
                "Add the lantern guide",
                "Create the first lantern repair quest",
                "Add lantern repair mechanic",
                "Collect sky lantern vocabulary")),
            BuildTemplate(new TemplateSeed(
                "clockwork-orchard",
                "Clockwork Orchard",
                "Automation Crafting",
                "An automation and crafting orchard maintenance adventure about keeping mechanical trees running.",
                "profile/clockwork-orchard/v1",
                ["feature_bundle/automation_crafting/v1", "feature_bundle/orchard_maintenance/v1"],
                "Define Clockwork Orchard profile",
                "Build the orchard workyard scene",
                "Add the orchard caretaker",
                "Create the first gearfruit repair quest",
                "Add orchard maintenance mechanic",
                "Collect clockwork orchard vocabulary")),
            BuildTemplate(new TemplateSeed(
                "storm-glass-lighthouse",
                "Storm Glass Lighthouse",
                "Exploration Mystery",
                "An exploration mystery repair adventure about restoring a lighthouse that reads approaching storms.",
                "profile/storm-glass-lighthouse/v1",
                ["feature_bundle/exploration_mystery/v1", "feature_bundle/lighthouse_repair/v1"],
                "Define Storm Glass Lighthouse profile",
                "Build the lighthouse island scene",
                "Add the storm glass keeper",
                "Create the first beacon repair quest",
                "Add storm glass calibration mechanic",
                "Collect lighthouse mystery vocabulary")),
            BuildTemplate(new TemplateSeed(
                "moss-courier-trail",
                "Moss Courier Trail",
                "Peaceful Delivery",
                "A peaceful delivery and exploration adventure about carrying messages between moss-grown waystations.",
                "profile/moss-courier-trail/v1",
                ["feature_bundle/peaceful_delivery/v1", "feature_bundle/trail_exploration/v1"],
                "Define Moss Courier Trail profile",
                "Build the first moss trail scene",
                "Add the trail postmaster",
                "Create the first waystation delivery quest",
                "Add route planning mechanic",
                "Collect moss courier vocabulary")),
            BuildTemplate(new TemplateSeed(
                "underroot-signal",
                "Underroot Signal",
                "Underground Restoration",
                "An underground signal restoration adventure about reconnecting root relays below an old settlement.",
                "profile/underroot-signal/v1",
                ["feature_bundle/underground_exploration/v1", "feature_bundle/signal_restoration/v1"],
                "Define Underroot Signal profile",
                "Build the root relay cavern scene",
                "Add the relay surveyor",
                "Create the first signal restoration quest",
                "Add relay tuning mechanic",
                "Collect underroot signal vocabulary"))
        ];
    }

    public IReadOnlyList<GeneratorPlanExampleTemplateSummary> ListTemplates()
    {
        return _templates.Select(template => template.Summary).ToList();
    }

    public GeneratorPlanExampleTemplate? GetTemplate(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return _templates.FirstOrDefault(template => string.Equals(template.Summary.Id, id.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static GeneratorPlanExampleTemplate BuildTemplate(TemplateSeed seed)
    {
        var summary = new GeneratorPlanExampleTemplateSummary
        {
            Id = seed.Id,
            Title = seed.Title,
            Description = seed.Description,
            Category = seed.Category,
            TargetArtifacts = DefaultTargetArtifacts
        };

        return new GeneratorPlanExampleTemplate
        {
            Summary = summary,
            FileName = $"{seed.Id}.example.json",
            Json = BuildJson(seed)
        };
    }

    private static string BuildJson(TemplateSeed seed)
    {
        var root = new JsonObject
        {
            ["schema_version"] = "0.1",
            ["example_id"] = $"example/{seed.Id}/v1",
            ["title"] = seed.Title,
            ["purpose"] = seed.Description,
            ["source_profile"] = new JsonObject
            {
                ["id"] = seed.SourceProfileId
            },
            ["selected_feature_bundles"] = ToJsonArray(seed.FeatureBundles),
            ["target_artifacts"] = ToJsonArray(DefaultTargetArtifacts),
            ["steps"] = new JsonArray(
                BuildStep("step/profile", 1, seed.ProfileStepTitle, GameProfileArtifact, "stage_profile", "repair_profile"),
                BuildStep("step/scene", 2, seed.SceneStepTitle, ScenePackArtifact, "stage_scene", "repair_scene"),
                BuildStep("step/entity", 3, seed.EntityStepTitle, EntityPackArtifact, "stage_entity", "repair_entity"),
                BuildStep("step/quest", 4, seed.QuestStepTitle, QuestPackArtifact, "stage_quest", "repair_quest"),
                BuildStep("step/mechanics", 5, seed.MechanicsStepTitle, MechanicsPackArtifact, "stage_mechanics", "repair_mechanics"),
                BuildStep("step/semantic", 6, seed.SemanticStepTitle, SemanticPackArtifact, "stage_semantic", "repair_semantic"))
        };

        return root.ToJsonString(JsonOptions);
    }

    private static JsonObject BuildStep(
        string id,
        int order,
        string title,
        string artifact,
        string onSuccess,
        string onFailure)
    {
        return new JsonObject
        {
            ["id"] = id,
            ["order"] = order,
            ["title"] = title,
            ["producer_role"] = "role/designer_llm/v1",
            ["context_pack_template"] = "context_template/design_discussion/v1",
            ["expected_artifact_contract"] = artifact,
            ["inputs"] = new JsonArray(JsonValue.Create(artifact)),
            ["validation_gates"] = new JsonArray(JsonValue.Create("validation.level_0_json_shape")),
            ["on_success"] = onSuccess,
            ["on_failure"] = onFailure
        };
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

    private sealed record TemplateSeed(
        string Id,
        string Title,
        string Category,
        string Description,
        string SourceProfileId,
        IReadOnlyList<string> FeatureBundles,
        string ProfileStepTitle,
        string SceneStepTitle,
        string EntityStepTitle,
        string QuestStepTitle,
        string MechanicsStepTitle,
        string SemanticStepTitle);
}
