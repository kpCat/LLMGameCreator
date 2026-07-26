using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal169;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169A;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169ATypedMigrationTests
{
    [Fact]
    public void Behavioral_same_world_rebase_preserves_compatible_resolution()
    {
        var state = Goal169SaveMigrationState.Value;
        var fact = Fact(state.CompatiblePreview,
            state.Event.RegionalEventId);

        Assert.True(fact.Compatible);
        Assert.True(fact.ResolutionFlagPreserved);
        Assert.True(fact.StatusReset);
        Assert.Equal("active_dialogue_reset",
            fact.DroppedReason);
    }

    [Fact]
    public void Behavioral_definition_mismatch_drops_resolution_with_typed_reason()
    {
        var state = Goal169SaveMigrationState.Value;
        var fact = Fact(state.IncompatiblePreview,
            state.Event.RegionalEventId);

        Assert.False(fact.Compatible);
        Assert.False(fact.ResolutionFlagPreserved);
        Assert.True(fact.StatusReset);
        Assert.Equal("event_definition_mismatch",
            fact.DroppedReason);
    }

    [Fact]
    public void Behavioral_migration_facts_expose_source_and_target_fingerprints()
    {
        var state = Goal169SaveMigrationState.Value;
        var fact = Fact(state.IncompatiblePreview,
            state.Event.RegionalEventId);

        Assert.False(string.IsNullOrWhiteSpace(
            fact.SourceEventFingerprint));
        Assert.False(string.IsNullOrWhiteSpace(
            fact.TargetEventFingerprint));
    }

    [Fact]
    public void Behavioral_preview_and_result_expose_same_typed_facts()
    {
        var state = Goal169SaveMigrationState.Value;

        Assert.Equal(state.CompatiblePreview.RegionalEventFacts,
            state.CompatibleApplied.RegionalEventFacts);
        Assert.Equal(state.IncompatiblePreview.RegionalEventFacts,
            state.IncompatibleApplied.RegionalEventFacts);
    }

    [Fact]
    public void Behavioral_typed_preserve_agrees_with_aggregate_counts()
    {
        var state = Goal169SaveMigrationState.Value;
        var preserved = state.CompatiblePreview.RegionalEventFacts
            .Count(item => item.ResolutionFlagPreserved);

        Assert.True(preserved > 0);
        Assert.True(state.CompatiblePreview.PreservedCountsByKind
            .GetValueOrDefault("regional_event_resolution")
            >= preserved);
    }

    [Fact]
    public void Behavioral_typed_drop_agrees_with_aggregate_reasons()
    {
        var state = Goal169SaveMigrationState.Value;
        var dropped = state.IncompatiblePreview.RegionalEventFacts
            .Where(item => !item.Compatible
                           && !string.IsNullOrWhiteSpace(
                               item.DroppedReason))
            .ToList();

        Assert.NotEmpty(dropped);
        Assert.Contains(state.IncompatiblePreview.DroppedReasons,
            item => item.Contains(
                "generated_regional_event_incompatible",
                StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_world_migration_resets_incompatible_event_status()
    {
        var state = Goal169SaveMigrationState.Value;
        var fact = Fact(state.IncompatiblePreview,
            state.Event.RegionalEventId);

        Assert.Equal(GeneratedGameplaySaveStatus
            .WORLD_MIGRATION_REQUIRED,
            state.IncompatiblePreview.SourceStatus);
        Assert.True(fact.StatusReset);
        Assert.DoesNotContain(
            state.IncompatibleApplied.Session!.GameplayState.Flags,
            item => item.Id == state.Event.ResolutionFlagId);
    }

    [Fact]
    public void Behavioral_migration_leaves_no_ghost_event_or_dialogue()
    {
        var state = Goal169SaveMigrationState.Value;

        Assert.DoesNotContain(
            state.GhostApplied.Session!.GameplayState.Flags,
            item => item.Id == state.GhostEventId);
        Assert.Null(state.IncompatibleApplied.Session!
            .GameplayState.ActiveDialogue);
        Assert.Null(state.IncompatibleApplied.Session!
            .GameplayState.ActiveEncounter);
    }

    private static GeneratedCampaignRegionalEventMigrationFact Fact(
        GeneratedGameplaySaveMigrationPreview preview,
        string eventId) => Assert.Single(
        preview.RegionalEventFacts,
        item => item.RegionalEventId == eventId);
}
