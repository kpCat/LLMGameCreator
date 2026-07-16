using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal159;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161;

[Collection(Goal160Collection.Name)]
public sealed class Goal161SameWorldLoadTests
{
    [Fact]
    public void Behavioral_current_generated_save_loads_exact_unified_session()
    {
        var state = Goal161MigrationState.Value;
        Assert.True(state.SameWorldLoad.Passed);
        Assert.Equal(state.Saved.RevisionSha256, state.SameWorldLoad.RevisionSha256);
        Assert.Equal(GeneratedGameplaySaveStatus.CURRENT, state.SameWorldLoad.Status);
    }

    [Fact]
    public void Behavioral_exact_serialized_session_equality_is_preserved()
    {
        var state = Goal161MigrationState.Value;
        Assert.Equal(state.Saved.Revision?.UnifiedRuntimeSessionJson,
            state.Bundle.Saves.Serializer.Serialize(state.SameWorldLoad.Session!));
    }

    [Fact]
    public void Behavioral_map_and_gameplay_hashes_validate_on_exact_load()
    {
        var revision = Goal161MigrationState.Value.SameWorldLoad.Revision!;
        Assert.Equal(64, revision.MapStateSha256.Length);
        Assert.Equal(64, revision.GameplayStateSha256.Length);
        Assert.Equal(64, revision.UnifiedRuntimeSessionSha256.Length);
        Assert.True(Goal161MigrationState.Value.SameWorldLoad.Passed);
    }

    [Fact]
    public void Behavioral_load_does_not_start_or_reset_runtime_session()
    {
        var loaded = Goal161MigrationState.Value.SameWorldLoad.Session!;
        Assert.Equal(42, loaded.GameplayState.Tick);
        Assert.Contains(loaded.MapEvents, item => item.Message == "transient");
        Assert.Contains(loaded.GameplayEvents, item => item.Message == "transient");
    }

    [Fact]
    public void Behavioral_same_world_package_mismatch_reports_package_rebase_required()
    {
        using var fixture = Goal161SaveFixture.Create("package-rebase");
        var saved = fixture.Services.Save.Save(fixture.Project.Path, "slot1", fixture.Session);
        var stale = saved.Revision! with
        {
            RevisionSha256 = string.Empty,
            PackageSha256 = new string('a', 64)
        };
        Assert.True(fixture.Services.Store.WriteRevision(fixture.Project.Path, "stale", stale).Passed);
        var load = fixture.Services.Save.Load(fixture.Project.Path, "stale");
        Assert.False(load.Passed);
        Assert.Equal(GeneratedGameplaySaveStatus.PACKAGE_REBASE_REQUIRED, load.Status);
    }

    [Fact]
    public void Behavioral_direct_load_of_world_migratable_save_is_rejected()
    {
        var stale = Goal161MigrationState.Value.StaleLoad;
        Assert.False(stale.Passed);
        Assert.Equal(GeneratedGameplaySaveStatus.WORLD_MIGRATION_REQUIRED, stale.Status);
    }

    [Fact]
    public void Behavioral_load_is_zero_write()
    {
        var state = Goal161MigrationState.Value;
        var before = Goal159TestKit.TreeHashes(state.Bundle.Saves.Store.RootPath(state.Project.Path));
        _ = state.Bundle.Saves.Save.LoadRevision(state.Project.Path, "campaign", state.Saved.RevisionSha256);
        Assert.Equal(before, Goal159TestKit.TreeHashes(state.Bundle.Saves.Store.RootPath(state.Project.Path)));
    }

    [Fact]
    public void Behavioral_exact_load_preserves_current_map_and_position()
    {
        var state = Goal161MigrationState.Value;
        Assert.Equal(state.Saved.Session?.MapState.CurrentMapId,
            state.SameWorldLoad.Session?.MapState.CurrentMapId);
        Assert.Equal(state.Saved.Session?.MapState.PlayerPosition.X,
            state.SameWorldLoad.Session?.MapState.PlayerPosition.X);
        Assert.Equal(state.Saved.Session?.MapState.PlayerPosition.Y,
            state.SameWorldLoad.Session?.MapState.PlayerPosition.Y);
    }

    [Fact]
    public void Behavioral_revision_is_bound_to_project_world_source_build_and_authoring_truth()
    {
        var revision = Goal161MigrationState.Value.Saved.Revision!;
        Assert.All(new[]
        {
            revision.ProjectIdentityFingerprint, revision.WorldId, revision.SourceRecordSha256,
            revision.SourceRequestSha256, revision.PlanSha256, revision.OverlaySha256,
            revision.GeneratedBasePackageSha256, revision.PackageSha256,
            revision.CompositionPackageSha256, revision.QualifiedAuthoringFingerprint,
            revision.SelectedBuildHistorySha256
        }, value => Assert.Equal(64, value.Length));
        Assert.False(string.IsNullOrWhiteSpace(revision.SelectedBuildHistoryFileName));
    }

    [Fact]
    public void Behavioral_save_revision_uses_generated_gameplay_schema()
    {
        var revision = Goal161MigrationState.Value.Saved.Revision!;
        Assert.Equal("generated_gameplay_save_v1", revision.SchemaVersion);
        Assert.Equal("generated_gameplay_save_slot_v1",
            Goal161MigrationState.Value.Bundle.Saves.Store.ReadSlot(
                Goal161MigrationState.Value.Project.Path, "campaign").Manifest?.SchemaVersion);
    }
}
