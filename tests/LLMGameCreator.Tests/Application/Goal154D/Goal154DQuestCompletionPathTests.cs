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

namespace LLMGameCreator.Tests.Application.Goal154D;

public sealed class Goal154DQuestCompletionPathTests
{
    [Fact]
    public void Behavioral_characterization_alchemy_refresh_completes_before_redundant_advance()
    {
        var fixture = Goal154DFixture.Create(startingHerbs: 4);
        var execution = fixture.ExecuteThroughAdvance();
        var quest = execution.Session.CanonicalSession.RuntimeSession.GameplayState.Quests
            .Single(item => item.QuestId == Goal154DFixture.QuestId);
        var startSnapshot = execution.Session.CanonicalSession.Snapshots.Single(item =>
            item.StepId == "capability." + Goal154DFixture.StartActionId);

        Assert.Equal("completed", quest.State);
        Assert.True(quest.Objectives.Single(item => item.ObjectiveId == Goal154DFixture.ObjectiveId).Completed);
        Assert.Contains(startSnapshot.RuntimeEvents, item => item.EventType == "QuestCompleted" && item.TargetId == Goal154DFixture.QuestId);
        Assert.Contains(startSnapshot.RuntimeEvents, item => item.EventType == "QuestRewardGranted" && item.TargetId == Goal154DFixture.QuestId);
        Assert.Equal("SKIPPED", execution.Advance.Status);
        Assert.Equal(execution.Advance.StateHashBefore, execution.Advance.StateHashAfter);
        Assert.Contains(execution.Advance.Diagnostics, item => item.Contains("questAlreadyCompletedBeforeAdvance=true", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(2, "EXECUTED", "explicit_advance")]
    [InlineData(3, "SKIPPED", "already_completed")]
    [InlineData(4, "SKIPPED", "already_completed")]
    [InlineData(20, "SKIPPED", "already_completed")]
    public void Behavioral_actual_parameter_binding_preserves_completion_boundary(
        int startingHerbs,
        string expectedStatus,
        string expectedPath)
    {
        var fixture = Goal154DFixture.Create(startingHerbs);
        var execution = fixture.ExecuteThroughAdvance();
        var quest = execution.Session.CanonicalSession.RuntimeSession.GameplayState.Quests
            .Single(item => item.QuestId == Goal154DFixture.QuestId);

        Assert.Equal(startingHerbs, fixture.StartingHerbs);
        Assert.Equal(3, fixture.RequiredHerbs);
        Assert.Equal("completed", quest.State);
        Assert.Equal(expectedStatus, execution.Advance.Status);
        Assert.Contains(execution.Advance.Diagnostics, item => item.Contains("questCompletionPath=" + expectedPath, StringComparison.Ordinal));
        if (expectedStatus == "SKIPPED")
        {
            Assert.False(execution.Advance.RuntimeExecuted);
            Assert.False(execution.Advance.RuntimeMutation);
            Assert.Equal(0, execution.Advance.RuntimeEventCount);
            Assert.Equal(execution.Advance.StateHashBefore, execution.Advance.StateHashAfter);
        }
        else
        {
            Assert.True(execution.Advance.RuntimeExecuted);
            Assert.True(execution.Advance.RuntimeMutation);
        }
    }
}

internal sealed record Goal154DFixture(
    GamePackageDefinition Package,
    IReadOnlyList<FeatureModuleDefinition> Modules,
    IReadOnlyList<FeatureModuleDefinition> SocialModules,
    CapabilityRuntimePlaythroughPlan Plan,
    int StartingHerbs,
    int RequiredHerbs,
    string PackageJson)
{
    internal const string AlchemyModuleId = "feature.profile.alchemy_focus";
    internal const string FactionModuleId = "feature.faction.reputation_standing";
    internal const string QuestModuleId = "feature.quest.faction_reputation_consequences";
    internal const string DialogueModuleId = "feature.dialogue.reputation_gated_reward";
    internal const string QuestId = "quest/help_healer";
    internal const string ObjectiveId = "objective/collect_red_herbs";
    internal const string StartActionId = "start_or_update_quest";
    internal const string AdvanceActionId = "advance_healer_objective";

    internal static Goal154DFixture Create(int startingHerbs)
    {
        var root = FindRoot();
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        Assert.True(library.Validation.Passed, string.Join("; ", library.Validation.Diagnostics));
        var optional = new[] { AlchemyModuleId, FactionModuleId, QuestModuleId, DialogueModuleId };
        var values = new List<FeatureModuleParameterValue>
        {
            Value(AlchemyModuleId, "startingRedHerbQuantity", startingHerbs),
            Value(FactionModuleId, "startingReputation", 0),
            Value(QuestModuleId, "questReputationReward", 10),
            Value(QuestModuleId, "questFailurePenalty", 5),
            Value(DialogueModuleId, "trustedReputationThreshold", 10),
            Value(DialogueModuleId, "trustedGoldReward", 7)
        };
        var document = new FeatureModuleCompositionDocument
        {
            CompositionId = "goal154d-boundary-" + startingHerbs,
            BaseCandidateId = "minimal-map-game-balanced-baseline",
            SelectedModuleIds = optional,
            ParameterValues = values,
            CatalogFingerprint = library.CatalogFingerprint,
            ModuleFingerprints = optional.ToDictionary(id => id, id => library.ModuleFingerprints[id], StringComparer.Ordinal)
        };
        var output = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "Goal154D", Guid.NewGuid().ToString("N"));
        FeatureModuleParameterizedCompositionResult materialized;
        try
        {
            materialized = new FeatureModuleParameterizedCompositionService(
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault()).MaterializeAndQualify(
                root, library, document, output, useCapabilityDrivenRuntimePlaythrough: true);
            var packagePath = Path.Combine(output, "compositions", document.CompositionId, "package.json");
            Assert.True(File.Exists(packagePath), string.Join("; ", materialized.Diagnostics));
            var packageJson = File.ReadAllText(packagePath);
            var package = JsonSerializer.Deserialize<GamePackageDefinition>(packageJson, JsonOptions)!;
            var selectedIds = library.Catalog.Modules.Where(module => module.Required).Select(module => module.ModuleId)
                .Concat(optional).Distinct(StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal);
            var modules = materialized.Plan.ParameterBinding.EffectiveCatalog.Modules
                .Where(module => selectedIds.Contains(module.ModuleId)).ToList();
            var social = modules.Where(module => optional.Skip(1).Contains(module.ModuleId, StringComparer.Ordinal)).ToList();
            var plan = new CapabilityDrivenRuntimePlaythroughPlanner().Plan(modules, package);
            var quantity = (int)package.Game.Inventories.Single(item => item.Id == "inventory/player_start")
                .Stacks.Single(item => item.ItemId == "item/red_herb").Amount;
            var required = (int)package.Game.Quests.Single(item => item.Id == QuestId)
                .Objectives.Single(item => item.Id == ObjectiveId).RequiredAmount;
            return new Goal154DFixture(package, modules, social, plan, quantity, required, packageJson);
        }
        finally
        {
            if (Directory.Exists(output)) Directory.Delete(output, true);
        }
    }

    internal Goal154DAdvanceExecution ExecuteThroughAdvance()
    {
        var beforeAdvance = ExecuteBeforeAdvance();
        var advance = beforeAdvance.ExecuteAdvance(Package);
        Assert.True(advance.Status is "EXECUTED" or "SKIPPED", string.Join("; ", advance.Diagnostics));
        return new Goal154DAdvanceExecution(beforeAdvance.Service, beforeAdvance.Start, beforeAdvance.Session, advance);
    }

    internal ProductLineRuntimeQualificationResult Qualify(string id) =>
        new ProductLineRuntimeQualifier(SelectedRuntimeVariantInteractiveSessionService.CreateDefault()).Qualify(Package,
            new ProductLineRuntimeQualificationRequest
            {
                SessionId = "goal154d-qualification-" + id,
                CandidateId = "goal154d",
                VariantKind = id,
                PackagePath = "in-memory/package.json",
                PackageSha256 = Hash(PackageJson),
                CheckpointId = "goal154d-checkpoint-" + id,
                FinalCheckpointId = "goal154d-final-" + id,
                CapabilityPlan = Plan
            });

    internal Goal154DBeforeAdvanceExecution ExecuteBeforeAdvance()
    {
        var service = SelectedRuntimeVariantInteractiveSessionService.CreateDefault();
        var start = new SelectedRuntimeVariantInteractiveSessionStartRequest
        {
            SessionId = "goal154d-" + StartingHerbs + "-" + Guid.NewGuid().ToString("N"),
            CandidateId = "goal154d",
            VariantKind = "starting-herbs-" + StartingHerbs,
            PackagePath = "in-memory/package.json",
            PackageSha256 = Hash(PackageJson),
            CapabilityPlan = Plan
        };
        var session = service.StartSession(Package, start);
        foreach (var action in Plan.OrderedActions)
        {
            if (action.ActionId == AdvanceActionId) break;
            var result = service.ExecuteAction(Package, session, new SelectedRuntimeVariantInteractiveActionRequest
            {
                ActionRequestId = start.SessionId + "-" + session.CurrentActionIndex.ToString("000"),
                SessionId = session.SessionId,
                ActionIndex = session.CurrentActionIndex,
                ActionId = action.ActionId
            });
            Assert.True(result.Status is "EXECUTED" or "SKIPPED",
                action.ActionId + ":" + string.Join("; ", result.Diagnostics));
        }
        Assert.Equal(AdvanceActionId, Plan.OrderedActions[session.CurrentActionIndex].ActionId);
        return new Goal154DBeforeAdvanceExecution(service, start, session);
    }

    internal static string FindRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln"))) return current.FullName;
            current = current.Parent;
        }
        throw new DirectoryNotFoundException("Repository root was not found.");
    }

    internal static FeatureModuleParameterValue Value(string moduleId, string parameterId, decimal value) => new()
    {
        ModuleId = moduleId,
        ParameterId = parameterId,
        Value = JsonSerializer.SerializeToElement(value)
    };

    internal static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    internal static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}

internal sealed record Goal154DAdvanceExecution(
    ISelectedRuntimeVariantInteractiveSessionService Service,
    SelectedRuntimeVariantInteractiveSessionStartRequest Start,
    RuntimeInteractiveSession Session,
    SelectedRuntimeVariantInteractiveActionResult Advance);

internal sealed record Goal154DBeforeAdvanceExecution(
    ISelectedRuntimeVariantInteractiveSessionService Service,
    SelectedRuntimeVariantInteractiveSessionStartRequest Start,
    RuntimeInteractiveSession Session)
{
    internal SelectedRuntimeVariantInteractiveActionResult ExecuteAdvance(GamePackageDefinition package) =>
        Service.ExecuteAction(package, Session, new SelectedRuntimeVariantInteractiveActionRequest
        {
            ActionRequestId = Start.SessionId + "-" + Session.CurrentActionIndex.ToString("000"),
            SessionId = Session.SessionId,
            ActionIndex = Session.CurrentActionIndex,
            ActionId = Goal154DFixture.AdvanceActionId
        });
}
