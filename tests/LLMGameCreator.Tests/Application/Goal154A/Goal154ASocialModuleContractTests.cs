using Xunit;

namespace LLMGameCreator.Tests.Application.Goal154A;

public sealed class Goal154ASocialModuleContractTests
{
    [Fact] public void Faction_module_is_versioned_and_declares_runtime_effect_contract() => Goal154ATestFiles.AssertContains("faction-reputation-standing.featuremodule.json", "\"moduleVersion\": \"1.1.0\"", "faction_reputation_initialized");
    [Fact] public void Faction_module_declares_runtime_playthrough_contract() => Goal154ATestFiles.AssertContains("faction-reputation-standing.featuremodule.json", "runtime.presentation.inspect_faction", "runtimePlaythroughContracts");
    [Fact] public void Quest_module_declares_effect_and_playthrough_contracts() => Goal154ATestFiles.AssertContains("quest-faction-reputation-consequences.featuremodule.json", "faction_reputation_transition_truthful", "runtime.command.advance_quest_objective");
    [Fact] public void Dialogue_module_declares_effect_and_playthrough_contracts() => Goal154ATestFiles.AssertContains("dialogue-reputation-gated-reward.featuremodule.json", "trusted_reward_social_outcome", "runtime.command.choose_dialogue_option");
}

internal static class Goal154ATestFiles
{
    public static void AssertContains(string fileName, params string[] expected)
    {
        var root = FindRoot();
        var text = File.ReadAllText(Path.Combine(root, "catalogs", "feature-modules", "optional", fileName));
        foreach (var value in expected) Assert.Contains(value, text, StringComparison.Ordinal);
    }

    public static string FindRoot()
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
