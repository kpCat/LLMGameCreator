using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal165;

public sealed class Goal165DefeatCheckpointTests
{
    [Fact]
    public void Behavioral_checkpoint_is_captured_before_successful_start_encounter()
    {
        var harness = Goal165RecoveryHarness.Create();
        var checkpoint = harness.Recovery.Prepare(harness.Truth, harness.Package, harness.PreEncounter,
            "encounter/fixture", "Проверочная встреча");
        harness.Recovery.Commit(checkpoint);

        Assert.Equal(Goal165RecoveryHarness.SessionHash(harness.PreEncounter), checkpoint.PreEncounterSessionSha256);
        Assert.Equal("encounter/fixture", harness.Recovery.Checkpoint!.EncounterId);
    }

    [Fact]
    public void Behavioral_failed_or_disabled_start_captures_no_checkpoint()
    {
        var harness = Goal165RecoveryHarness.Create();

        Assert.Null(harness.Recovery.Checkpoint);
    }

    [Fact]
    public void Contract_checkpoint_serialization_roundtrips_exactly()
    {
        var harness = Goal165RecoveryHarness.WithCheckpoint();
        var restored = harness.Recovery.Restore(harness.Truth, harness.Package);

        Assert.True(restored.Passed);
        Assert.Equal(Goal165RecoveryHarness.SessionHash(harness.PreEncounter),
            Goal165RecoveryHarness.SessionHash(restored.Session!));
    }

    [Fact]
    public void Behavioral_victory_checkpoint_can_be_cleared()
    {
        var harness = Goal165RecoveryHarness.WithCheckpoint();
        harness.Recovery.Clear();

        Assert.Null(harness.Recovery.Checkpoint);
    }

    [Fact]
    public void Behavioral_flee_checkpoint_can_be_cleared()
    {
        var harness = Goal165RecoveryHarness.WithCheckpoint();
        harness.Recovery.Clear();

        Assert.Null(harness.Recovery.Checkpoint);
    }

    [Fact]
    public void Behavioral_genuine_defeat_retains_checkpoint()
    {
        var harness = Goal165RecoveryHarness.WithCheckpoint();
        var defeated = Goal165RecoveryHarness.Encounter(active: false, playerAlive: false, opponentAlive: true);

        Assert.True(GeneratedCampaignRecoveryService.IsDefeat(defeated));
        Assert.NotNull(harness.Recovery.Checkpoint);
    }

    [Fact]
    public void Behavioral_defeat_requires_no_living_player_participant()
    {
        Assert.False(GeneratedCampaignRecoveryService.IsDefeat(
            Goal165RecoveryHarness.Encounter(active: false, playerAlive: true, opponentAlive: true)));
        Assert.False(GeneratedCampaignRecoveryService.IsDefeat(
            Goal165RecoveryHarness.Encounter(active: true, playerAlive: false, opponentAlive: true)));
    }

    [Fact]
    public void Behavioral_nondefeat_encounter_end_does_not_enter_defeated()
    {
        var victory = Goal165RecoveryHarness.Encounter(active: false, playerAlive: true, opponentAlive: false);

        Assert.False(GeneratedCampaignRecoveryService.IsDefeat(victory));
        Assert.True(GeneratedCampaignRecoveryService.IsVictory(victory));
    }

    [Fact]
    public void Behavioral_project_switch_invalidates_checkpoint()
    {
        var harness = Goal165RecoveryHarness.WithCheckpoint();
        var switched = harness.Truth with { WorldId = "other-world" };
        var restored = harness.Recovery.Restore(switched, harness.Package);

        Assert.True(restored.Stale);
        Assert.True(harness.Recovery.Checkpoint!.Invalidated);
    }

    [Fact]
    public void Behavioral_world_package_or_authoring_drift_makes_checkpoint_stale()
    {
        var harness = Goal165RecoveryHarness.WithCheckpoint();
        var drifted = harness.Truth with { QualifiedAuthoringFingerprint = "changed" };
        var restored = harness.Recovery.Restore(drifted, harness.Package);

        Assert.True(restored.Stale);
        Assert.Contains("изменился", restored.HumanReason, StringComparison.OrdinalIgnoreCase);
    }
}

internal sealed record Goal165RecoveryHarness(
    GeneratedCampaignRecoveryService Recovery,
    GeneratedCampaignProjectTruth Truth,
    GamePackageDefinition Package,
    UnifiedRuntimeSession PreEncounter)
{
    public static Goal165RecoveryHarness Create() => new(
        new GeneratedCampaignRecoveryService(),
        new GeneratedCampaignProjectTruth
        {
            ProjectIdentityFingerprint = "project",
            WorldId = "world",
            PackageSha256 = "package",
            CompositionPackageSha256 = "composition",
            FinalStateHash = "state",
            SelectedBuildHistorySha256 = "history",
            QualifiedAuthoringFingerprint = "authoring",
            GeneratedStartMapId = "map/start"
        },
        Goal164TestKit.Clone(Goal164TestKit.AllSelectable.Package),
        Encounter(active: true, playerAlive: true, opponentAlive: true));

    public static Goal165RecoveryHarness WithCheckpoint()
    {
        var value = Create();
        value.Recovery.Commit(value.Recovery.Prepare(value.Truth, value.Package, value.PreEncounter,
            "encounter/fixture", "Проверочная встреча"));
        return value;
    }

    public static UnifiedRuntimeSession Encounter(bool active, bool playerAlive, bool opponentAlive) => new()
    {
        GameplayState = new GameRuntimeState
        {
            ActiveEncounter = new EncounterRuntimeState
            {
                EncounterId = "encounter/fixture",
                Active = active,
                Participants =
                [
                    new EncounterParticipantState { Id = "player", Team = "player", Alive = playerAlive },
                    new EncounterParticipantState { Id = "opponent", Team = "opponent", Alive = opponentAlive }
                ]
            }
        }
    };

    public static string SessionHash(UnifiedRuntimeSession session) =>
        GeneratedCampaignRecoveryService.SessionSha(session);
}
