using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Runtime;

public sealed class CharacterAttributesProgressionRuntimeTests
{
    [Fact]
    public void CharacterAttributes_current_player_stat_overrides_explicit_participant_value_at_encounter_start()
    {
        var package = Package();
        var ability = package.Game.Abilities.Single(item => item.Id == "ability/basic_attack");
        ability.Metadata["source_stat_damage_stat_id"] = "stat/strength";
        ability.Metadata["source_stat_damage_baseline"] = "5";
        ability.Metadata["source_stat_damage_per_point"] = "1";
        package.Game.Stats.Single(item => item.Id == "stat/strength").DefaultValue = 7;
        var runtime = CanonicalRuntimePlayerCommandLoopService.CreateDefault();
        var session = runtime.BeginSession(package, new CanonicalRuntimePlayerCommandLoopRequest
        {
            CandidateId = "attributes-precedence",
            CapabilityPlan = null
        });
        runtime.ExecuteRange(package, session, new CanonicalRuntimePlayerCommandLoopExecutionRequest
        {
            RequestedOperation = "through-encounter",
            RuntimeCommandStartIndex = 0,
            RuntimeCommandEndIndex = 10
        });
        var player = session.RuntimeSession.GameplayState.ActiveEncounter!.Participants.Single(item => item.Id == "player");
        Assert.Equal(7, player.Stats.Single(item => item.StatId == "stat/strength").Value);
    }

    [Fact]
    public void LevelProgression_command_delegates_to_output_stage_resolution_and_rejects_invalid_input()
    {
        var package = Package();
        var runtime = CreateRuntime();
        var state = runtime.CreateInitialState(package).State;
        var changed = runtime.Execute(package, state,
            GameRuntimeCommand.ChangeProgression("progression/character_level", 10));
        Assert.True(changed.Success);
        var progression = state.Progressions.Single(item => item.ProgressionId == "progression/character_level");
        Assert.Equal(10, progression.Amount);
        Assert.Equal("level/2", progression.StageId);
        Assert.Contains(changed.Events, item => item.Type == GameRuntimeEventType.ProgressionStageChanged);

        var before = progression.Amount;
        Assert.False(runtime.Execute(package, state,
            GameRuntimeCommand.ChangeProgression("progression/missing", 10)).Success);
        Assert.False(runtime.Execute(package, state,
            GameRuntimeCommand.ChangeProgression("progression/character_level", double.NaN)).Success);
        Assert.Equal(before, progression.Amount);
    }

    private static GameRuntimeService CreateRuntime()
    {
        var requirements = new RequirementEvaluator();
        var costs = new CostConsumer();
        var outputs = new OutputApplier();
        var recipe = new RecipeRuntimeService(requirements, costs, outputs);
        var transaction = new TransactionRuntimeService(requirements, costs, outputs);
        var useItem = new UseItemRuntimeService(requirements, outputs);
        return new GameRuntimeService(
            new GameRuntimeStateFactory(), recipe, new LootRuntimeService(requirements, outputs), transaction,
            new ResourceNetworkRuntimeService(requirements, costs, outputs), useItem,
            new InteractionRuntimeService(requirements, outputs, recipe, transaction), outputApplier: outputs);
    }

    private static GamePackageDefinition Package()
    {
        var root = FindRoot();
        return JsonSerializer.Deserialize<GamePackageDefinition>(
            File.ReadAllText(Path.Combine(root, "samples", "minimal-map-game", "package.json")), Options)!;
    }

    private static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static string FindRoot()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }
        throw new InvalidOperationException("Repository root was not found.");
    }
}
