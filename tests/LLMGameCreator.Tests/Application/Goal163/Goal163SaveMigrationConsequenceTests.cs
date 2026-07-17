using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal163;

[Collection(Goal160Collection.Name)]
public sealed class Goal163SaveMigrationConsequenceTests
{
    private readonly GeneratedCampaignConsequenceProjector _projector = new();

    [Fact]
    public void Behavioral_successful_save_uses_typed_result()
    {
        var outcome = _projector.ProjectSave(Goal163TestKit.CombatSession(),
            new GeneratedGameplaySaveResult { Passed = true, Status = GeneratedGameplaySaveStatus.CURRENT });

        Assert.True(outcome.Success);
        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.Save);
    }

    [Fact]
    public void Behavioral_deduplicated_save_reports_no_new_revision()
    {
        var outcome = _projector.ProjectSave(Goal163TestKit.CombatSession(),
            new GeneratedGameplaySaveResult { Passed = true, Deduplicated = true, Status = GeneratedGameplaySaveStatus.CURRENT });

        Assert.Contains(outcome.Consequences, item => item.AfterValue == "Без новой ревизии");
    }

    [Fact]
    public void Behavioral_failed_save_is_failure_not_success()
    {
        var outcome = _projector.ProjectSave(Goal163TestKit.CombatSession(),
            new GeneratedGameplaySaveResult { Diagnostics = ["save.invalid"] });

        Assert.False(outcome.Success);
        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.Failure);
    }

    [Fact]
    public void Behavioral_continue_uses_typed_load_result()
    {
        var session = Goal163TestKit.ReadyQuestSession();
        var outcome = _projector.ProjectLoad(session,
            new GeneratedGameplaySaveResult { Passed = true, Session = session, Status = GeneratedGameplaySaveStatus.CURRENT });

        Assert.True(outcome.Success);
        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.Load);
    }

    [Fact]
    public void Behavioral_migration_reports_preserved_and_dropped_counts()
    {
        var outcome = _projector.ProjectMigration(Goal163TestKit.ReadyQuestSession(),
            new GeneratedGameplaySaveMigrationResult
            {
                Passed = true,
                Preview = new GeneratedGameplaySaveMigrationPreview
                {
                    Passed = true,
                    PreservedCountsByKind = new Dictionary<string, int> { ["quest"] = 2 },
                    DroppedCountsByKind = new Dictionary<string, int> { ["map"] = 1 },
                    MapReset = true
                }
            });

        var migration = Assert.Single(outcome.Consequences);
        Assert.Equal(GeneratedCampaignConsequenceKind.Migration, migration.Kind);
        Assert.Contains("2", migration.Delta);
        Assert.Contains("1", migration.Delta);
    }

    [Fact]
    public void Behavioral_persisted_events_rebuild_only_supported_consequences()
    {
        var session = Goal163TestKit.CombatSession();
        session.MapEvents.Add(new RuntimeEvent { Type = RuntimeEventType.MapChanged });
        session.GameplayEvents.Add(new GameRuntimeEvent { Type = GameRuntimeEventType.DamageApplied });
        session.GameplayEvents.Add(new GameRuntimeEvent { Type = GameRuntimeEventType.EncounterWon });
        session.GameplayEvents.Add(new GameRuntimeEvent { Type = GameRuntimeEventType.LogMessageAdded });

        var rows = _projector.RebuildFromPersistedEvents(Goal163TestKit.FullPackage(), session);

        Assert.Contains(rows, item => item.Kind == GeneratedCampaignConsequenceKind.MapTravel);
        Assert.Contains(rows, item => item.Kind == GeneratedCampaignConsequenceKind.Damage);
        Assert.Contains(rows, item => item.Kind == GeneratedCampaignConsequenceKind.EncounterWon);
        Assert.DoesNotContain(rows, item => item.Kind == GeneratedCampaignConsequenceKind.Dialogue);
    }

    [Fact]
    public void Behavioral_rebuilt_timeline_is_bounded_below_global_limit()
    {
        var session = Goal163TestKit.CombatSession();
        for (var index = 0; index < 100; index++)
            session.GameplayEvents.Add(new GameRuntimeEvent { Type = GameRuntimeEventType.DamageApplied, Message = index.ToString() });

        var rows = _projector.RebuildFromPersistedEvents(Goal163TestKit.FullPackage(), session);

        Assert.True(rows.Count < GeneratedCampaignConsequenceTimeline.DefaultMaximumEntries);
    }
}
