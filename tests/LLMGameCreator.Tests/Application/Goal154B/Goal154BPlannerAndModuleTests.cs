using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;
using RuntimeInteractiveSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Tests.Application.Goal154B;

public sealed class Goal154BPlannerAndModuleTests
{
    [Fact]
    public void Behavioral_all_social_modules_plan_every_primitive_selector_and_checkpoint()
    {
        var fixture = Goal154BFixture.Create();

        Assert.True(fixture.Plan.OrderedActions.Count > 16);
        Assert.Equal("advance_healer_objective", fixture.Plan.CheckpointBoundaryActionId);
        Assert.All(fixture.Plan.RuntimePrimitiveIds, primitive => Assert.Contains(primitive, CapabilityRuntimePrimitiveIds.Supported));
        Assert.All(fixture.Plan.OrderedActions, action => Assert.False(string.IsNullOrWhiteSpace(action.ResolvedTargetId)));
        Assert.Contains(fixture.Plan.OrderedActions, action => action.ActionId == "advance_healer_objective"
            && action.DependsOnActionIds.Contains("close_healer_before_quest"));
        Assert.Equal(fixture.Plan.OrderedActions.Count,
            fixture.Plan.OrderedActions.Select(action => action.ActionId).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Behavioral_malformed_social_selectors_fail_with_causal_diagnostics()
    {
        var fixture = Goal154BFixture.Create();
        var cases = new[]
        {
            (Module: Goal154BFixture.MutateAction(fixture.Modules, "inspect_initial_faction_reputation",
                action => action with { Args = Args(("factionId", "faction/missing")) }), Expected: "faction_id"),
            (Module: Goal154BFixture.MutateAction(fixture.Modules, "advance_healer_objective",
                action => action with { Args = Args(("questId", "quest/help_healer"), ("objectiveId", "objective/missing"), ("amount", "10")) }), Expected: "quest_objective_id"),
            (Module: Goal154BFixture.MutateAction(fixture.Modules, "inspect_trusted_choice_before_quest",
                action => action with { Args = Args(("dialogueId", "dialogue/healer"), ("nodeId", "missing"), ("choiceId", "trusted_village_reward")) }), Expected: "dialogue_node_id"),
            (Module: Goal154BFixture.MutateAction(fixture.Modules, "claim_trusted_reward",
                action => action with { Args = Args(("dialogueId", "dialogue/healer"), ("nodeId", "start"),
                    ("choiceId", "missing"), ("executionPredicates", "dialogue_choice_available"),
                    ("unavailableOutcome", "still_locked")) }), Expected: "dialogue_choice_id")
        };

        foreach (var item in cases)
        {
            var result = new CapabilityDrivenRuntimePlaythroughPlanner().TryPlan(item.Module, fixture.Package);
            Assert.False(result.Passed);
            Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Contains(item.Expected, StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Behavioral_reverse_module_order_preserves_package_and_plan_signature()
    {
        var fixture = Goal154BFixture.Create();
        var reverseMutation = new FeatureModulePackageMutationService().Apply(fixture.BasePackageJson,
            fixture.Binding.EffectiveMutationOperations.Reverse().ToList());
        Assert.True(reverseMutation.Passed, string.Join("; ", reverseMutation.Diagnostics));
        Assert.Equal(fixture.PackageJson, reverseMutation.PackageJson);

        var reversePlan = new CapabilityDrivenRuntimePlaythroughPlanner().Plan(fixture.Modules.Reverse().ToList(), fixture.Package);
        Assert.Equal(fixture.Plan.ActionPlanSignature, reversePlan.ActionPlanSignature);
        Assert.Equal(fixture.Plan.OrderedActions.Select(action => action.ActionId),
            reversePlan.OrderedActions.Select(action => action.ActionId));
    }

    [Fact]
    public void Behavioral_each_valid_social_dependency_closure_plans()
    {
        var faction = Goal154BFixture.CreateSelected(Goal154BFixture.FactionModuleId);
        var quest = Goal154BFixture.CreateSelected(Goal154BFixture.FactionModuleId, Goal154BFixture.QuestModuleId);
        var dialogue = Goal154BFixture.Create();

        Assert.Contains(faction.Plan.OrderedActions, action => action.ActionId == "inspect_initial_faction_reputation");
        Assert.DoesNotContain(faction.Plan.OrderedActions, action => action.ActionId == "advance_healer_objective");
        Assert.Contains(quest.Plan.OrderedActions, action => action.ActionId == "advance_healer_objective");
        Assert.DoesNotContain(quest.Plan.OrderedActions, action => action.ActionId == "open_healer_before_quest");
        Assert.Contains(dialogue.Plan.OrderedActions, action => action.ActionId == "inspect_social_summary");
    }

    [Fact]
    public void Behavioral_default_off_social_composition_preserves_package_and_initial_state_hashes()
    {
        var root = Goal154BFixture.FindRoot();
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        var selected = library.Catalog.Modules.Where(module => module.Required).Select(module => module.ModuleId).ToList();
        var binding = new FeatureModuleParameterBindingService().Bind(library.Catalog, selected, []);
        Assert.True(binding.Passed, string.Join("; ", binding.Diagnostics));
        Assert.DoesNotContain(binding.EffectiveCatalog.Modules.Where(module => Goal154BFixture.SocialModuleIds.Contains(module.ModuleId)),
            module => selected.Contains(module.ModuleId));

        var baseline = Goal154BFixture.ReadBasePackage();
        var result = new FeatureModulePackageMutationService().Apply(baseline, []);
        Assert.True(result.Passed);
        Assert.Equal(Goal154BFixture.Hash(baseline), Goal154BFixture.Hash(result.PackageJson));
        var package = Goal154BFixture.Deserialize(baseline);
        var first = new GameRuntimeStateFactory().CreateInitialState(package).State;
        var second = new GameRuntimeStateFactory().CreateInitialState(package).State;
        Assert.Equal(Goal154BFixture.Stable(first), Goal154BFixture.Stable(second));
    }

    private static IReadOnlyDictionary<string, string> Args(params (string Key, string Value)[] values) =>
        values.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
}

internal sealed record Goal154BFixture(
    string BasePackageJson,
    string PackageJson,
    GamePackageDefinition Package,
    IReadOnlyList<FeatureModuleDefinition> Modules,
    IReadOnlyList<FeatureModuleDefinition> SocialModules,
    FeatureModuleParameterBindingResult Binding,
    CapabilityRuntimePlaythroughPlan Plan)
{
    public const string FactionModuleId = "feature.faction.reputation_standing";
    public const string QuestModuleId = "feature.quest.faction_reputation_consequences";
    public const string DialogueModuleId = "feature.dialogue.reputation_gated_reward";
    public static readonly string[] SocialModuleIds = [FactionModuleId, QuestModuleId, DialogueModuleId];

    public static Goal154BFixture Create(
        decimal startingReputation = 0,
        decimal questReputationReward = 10,
        decimal questFailurePenalty = 5,
        decimal trustedReputationThreshold = 10,
        decimal trustedGoldReward = 7) => CreateSelectedWithValues(SocialModuleIds,
    [
        Value(FactionModuleId, "startingReputation", startingReputation),
        Value(QuestModuleId, "questReputationReward", questReputationReward),
        Value(QuestModuleId, "questFailurePenalty", questFailurePenalty),
        Value(DialogueModuleId, "trustedReputationThreshold", trustedReputationThreshold),
        Value(DialogueModuleId, "trustedGoldReward", trustedGoldReward)
    ]);

    public static Goal154BFixture CreateSelected(params string[] socialModuleIds) =>
        CreateSelectedWithValues(socialModuleIds, []);

    private static Goal154BFixture CreateSelectedWithValues(
        IReadOnlyList<string> socialModuleIds,
        IReadOnlyList<FeatureModuleParameterValue> values)
    {
        var root = FindRoot();
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        Assert.True(library.Validation.Passed, string.Join("; ", library.Validation.Diagnostics));
        var selectedIds = library.Catalog.Modules.Where(module => module.Required).Select(module => module.ModuleId)
            .Concat(socialModuleIds).Distinct(StringComparer.Ordinal).ToList();
        var binding = new FeatureModuleParameterBindingService().Bind(library.Catalog, selectedIds, values);
        Assert.True(binding.Passed, string.Join("; ", binding.Diagnostics));
        var baseline = ReadBasePackage();
        var mutation = new FeatureModulePackageMutationService().Apply(baseline, binding.EffectiveMutationOperations);
        Assert.True(mutation.Passed, string.Join("; ", mutation.Diagnostics));
        var package = Deserialize(mutation.PackageJson);
        var modules = binding.EffectiveCatalog.Modules.Where(module => selectedIds.Contains(module.ModuleId)).ToList();
        var plan = new CapabilityDrivenRuntimePlaythroughPlanner().Plan(modules, package);
        return new Goal154BFixture(baseline, mutation.PackageJson, package, modules,
            modules.Where(module => SocialModuleIds.Contains(module.ModuleId)).ToList(), binding, plan);
    }

    public ProductLineRuntimeQualificationResult Qualify(string id) =>
        new ProductLineRuntimeQualifier(SelectedRuntimeVariantInteractiveSessionService.CreateDefault()).Qualify(Package,
            new ProductLineRuntimeQualificationRequest
            {
                SessionId = "goal154b-" + id,
                CandidateId = "goal154b",
                VariantKind = id,
                PackagePath = "in-memory/package.json",
                PackageSha256 = Hash(PackageJson),
                CheckpointId = "goal154b-checkpoint-" + id,
                FinalCheckpointId = "goal154b-final-" + id,
                CapabilityPlan = Plan
            });

    public Goal154BInteractiveExecution ExecuteActionByAction(string id)
    {
        var service = SelectedRuntimeVariantInteractiveSessionService.CreateDefault();
        var start = new SelectedRuntimeVariantInteractiveSessionStartRequest
        {
            SessionId = "goal154b-direct-" + id,
            CandidateId = "goal154b",
            VariantKind = id,
            PackagePath = "in-memory/package.json",
            PackageSha256 = Hash(PackageJson),
            CapabilityPlan = Plan
        };
        var session = service.StartSession(Package, start);
        var results = new List<SelectedRuntimeVariantInteractiveActionResult>();
        foreach (var action in Plan.OrderedActions)
        {
            var result = service.ExecuteAction(Package, session, new SelectedRuntimeVariantInteractiveActionRequest
            {
                ActionRequestId = start.SessionId + "-" + session.CurrentActionIndex.ToString("000"),
                SessionId = session.SessionId,
                ActionIndex = session.CurrentActionIndex,
                ActionId = action.ActionId
            });
            Assert.True(result.Status is "EXECUTED" or "SKIPPED",
                action.ActionId + ":" + string.Join("; ", result.Diagnostics));
            results.Add(result);
        }
        return new Goal154BInteractiveExecution(service, start, session, results);
    }

    public static IReadOnlyList<FeatureModuleDefinition> MutateAction(
        IReadOnlyList<FeatureModuleDefinition> modules,
        string actionId,
        Func<FeatureModuleRuntimePlaythroughContract, FeatureModuleRuntimePlaythroughContract> mutate) =>
        modules.Select(module => module.RuntimePlaythroughContracts.Any(action => action.ActionId == actionId)
            ? module with
            {
                RuntimePlaythroughContracts = module.RuntimePlaythroughContracts.Select(action =>
                    action.ActionId == actionId ? mutate(action) : action).ToList()
            }
            : module).ToList();

    public static GameRuntimeService CreateGameRuntime()
    {
        var requirements = new RequirementEvaluator();
        var costs = new CostConsumer();
        var outputs = new OutputApplier();
        var recipe = new RecipeRuntimeService(requirements, costs, outputs);
        var transaction = new TransactionRuntimeService(requirements, costs, outputs);
        var encounter = new EncounterRuntimeService(requirements, outputs);
        var quest = new QuestRuntimeService(requirements, outputs);
        var dialogue = new DialogueRuntimeService(requirements, costs, outputs, quest, transaction, encounter);
        return new GameRuntimeService(new GameRuntimeStateFactory(), recipe,
            new LootRuntimeService(requirements, outputs), transaction,
            new ResourceNetworkRuntimeService(requirements, costs, outputs),
            new UseItemRuntimeService(requirements, outputs),
            new InteractionRuntimeService(requirements, outputs, recipe, transaction,
                dialogueRuntimeService: dialogue, questRuntimeService: quest, encounterRuntimeService: encounter),
            questRuntimeService: quest, dialogueRuntimeService: dialogue, encounterRuntimeService: encounter,
            outputApplier: outputs);
    }

    public static FeatureModuleParameterValue Value(string moduleId, string parameterId, decimal value) => new()
    {
        ModuleId = moduleId,
        ParameterId = parameterId,
        Value = JsonSerializer.SerializeToElement(value)
    };

    public static string ReadBasePackage() => File.ReadAllText(Path.Combine(FindRoot(), ".llmgc", "procedural",
        "goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff", "candidates",
        "minimal-map-game-balanced-baseline", "package.json"));

    public static GamePackageDefinition Deserialize(string json) =>
        JsonSerializer.Deserialize<GamePackageDefinition>(json, JsonOptions)!;

    public static GamePackageDefinition ClonePackage(GamePackageDefinition package) =>
        Deserialize(JsonSerializer.Serialize(package, JsonOptions));

    public static string Serialize(GamePackageDefinition package) => JsonSerializer.Serialize(package, JsonOptions);

    public static string Stable(GameRuntimeState state) => JsonSerializer.Serialize(state, JsonOptions);

    public static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    public static double Gold(GameRuntimeState state) =>
        state.Resources.SingleOrDefault(resource => resource.ResourceId == "resource/gold" && resource.Scope == "global")?.Amount ?? 0;

    public static string FindRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln"))) return current.FullName;
            current = current.Parent;
        }
        throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}

internal sealed record Goal154BInteractiveExecution(
    ISelectedRuntimeVariantInteractiveSessionService Service,
    SelectedRuntimeVariantInteractiveSessionStartRequest Start,
    RuntimeInteractiveSession Session,
    IReadOnlyList<SelectedRuntimeVariantInteractiveActionResult> Results);
