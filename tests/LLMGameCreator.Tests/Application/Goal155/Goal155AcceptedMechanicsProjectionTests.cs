using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal155;

public sealed class Goal155AcceptedMechanicsProjectionTests
{
    private readonly GameProjectAcceptedMechanicsSummaryService _service = new();

    [Fact]
    public void Behavioral_complete_profile_projects_all_typed_values_and_passes()
    {
        var summary = _service.Project(Complete());
        Assert.True(summary.Present && summary.Passed);
        Assert.Equal(22, summary.SelectedMechanicCount);
        Assert.Equal(14, summary.ConfiguredParameterCount);
        Assert.Equal(3, summary.EquipmentDamageBonus);
        Assert.Equal(6, summary.StatDamageBonus);
        Assert.Equal(9, summary.TotalAdditionalDamage);
        Assert.Equal(2, summary.AbilityDirectDamage);
        Assert.Equal(12, summary.ManaBefore);
        Assert.Equal(9, summary.ManaRemaining);
        Assert.Equal(1, summary.StatusTickDamage);
        Assert.True(summary.StatusExpired);
        Assert.Equal(10, summary.Social?.ReputationAfter);
        Assert.Equal(17, summary.Social?.GoldAfterClaim);
        Assert.Empty(summary.MissingFactKinds);
    }

    [Fact]
    public void Behavioral_missing_equipment_is_classified_without_failing_the_valid_build()
    {
        var build = Complete() with { EquipmentSlotSummary = string.Empty };
        var summary = _service.Project(build);
        Assert.True(build.Passed);
        Assert.True(summary.Present);
        Assert.False(summary.Passed);
        Assert.Contains("equipment", summary.MissingFactKinds);
    }

    [Theory]
    [InlineData("ability")]
    [InlineData("mana")]
    [InlineData("turn_status")]
    [InlineData("social")]
    public void Behavioral_missing_optional_family_facts_are_classified_independently(string kind)
    {
        var source = Complete();
        var build = kind switch
        {
            "ability" => source with { AbilitySummary = string.Empty },
            "mana" => source with { ManaSummary = string.Empty },
            "turn_status" => source with { StatusSummary = string.Empty },
            "social" => source with { Social = null },
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        var summary = _service.Project(build);
        Assert.False(summary.Passed);
        Assert.Contains(kind, summary.MissingFactKinds);
        Assert.DoesNotContain("equipment", summary.MissingFactKinds);
    }

    [Theory]
    [InlineData("checkpoint_reload")]
    [InlineData("full_replay")]
    [InlineData("action_binding")]
    public void Behavioral_replay_or_binding_failure_prevents_accepted_readiness(string kind)
    {
        var source = Complete();
        var build = kind switch
        {
            "checkpoint_reload" => source with { CheckpointReloadPassed = false },
            "full_replay" => source with { FullReplayEquivalent = false },
            "action_binding" => source with { ActionBindingPassed = false },
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        var summary = _service.Project(build);
        Assert.False(summary.Passed);
        Assert.Contains(kind, summary.MissingFactKinds);
    }

    [Fact]
    public void Behavioral_human_facts_are_concise_and_contain_no_ids_hashes_or_paths()
    {
        var facts = _service.Project(Complete()).HumanFacts;
        Assert.Contains(facts, fact => fact.Label == "Мана" && fact.Value == "12 → 9");
        Assert.Contains(facts, fact => fact.Label == "Эффект по ходам" && fact.Value == "урон 1; завершён");
        var text = string.Join("\n", facts.Select(fact => fact.Label + "=" + fact.Value));
        Assert.DoesNotContain("feature.", text, StringComparison.Ordinal);
        Assert.DoesNotContain("sha", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\\", text, StringComparison.Ordinal);
        Assert.DoesNotContain("/", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Behavioral_projection_is_deterministic_for_repeated_input()
    {
        var build = Complete();
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(_service.Project(build)),
            System.Text.Json.JsonSerializer.Serialize(_service.Project(build)));
    }

    internal static GameProjectBuildResult Complete() => new()
    {
        Status = "GREEN",
        Passed = true,
        SelectedMechanicCount = 22,
        ConfiguredParameterCount = 14,
        QualifiedAuthoringFingerprint = new string('a', 64),
        EquipmentSlotSummary = "Оружие экипировано",
        WeaponDamageBonus = 3,
        AttributesSummary = "stat/strength=8",
        ProgressionSummary = "amount=12:stage/level/2",
        StatDamageBonus = 6,
        TotalAdditionalDamage = 9,
        AbilitySummary = "Сосредоточенный удар",
        AbilityDirectDamage = 2,
        ManaSummary = "12 → 9 (стоимость 3)",
        ManaBefore = 12,
        ManaSpent = 3,
        ManaRemaining = 9,
        StatusSummary = "Горение, 5 ходов",
        StatusTickDamage = 1,
        StatusExpired = true,
        CheckpointReloadPassed = true,
        FullReplayEquivalent = true,
        ActionBindingPassed = true,
        Social = new GameProjectSocialSummary
        {
            Present = true,
            Passed = true,
            ReputationBefore = 0,
            ReputationAfter = 10,
            QuestState = "completed",
            GoldBefore = 0,
            GoldAfterQuest = 10,
            GoldAfterClaim = 17,
            TrustedRewardDelta = 7,
            RewardClaimed = true,
            CheckpointReplayPassed = true,
            FullReplayEquivalent = true,
            HumanFacts =
            [
                new GameProjectSocialHumanFact { Label = "Репутация", Value = "0 → 10" },
                new GameProjectSocialHumanFact { Label = "Золото", Value = "0 → 10 → 17" },
                new GameProjectSocialHumanFact { Label = "Социальный итог", Value = "награда получена" }
            ]
        }
    };
}
