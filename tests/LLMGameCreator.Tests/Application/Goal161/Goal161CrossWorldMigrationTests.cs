using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal157;
using LLMGameCreator.Tests.Application.Goal159;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161;

[Collection(Goal160Collection.Name)]
public sealed class Goal161CrossWorldMigrationTests
{
    [Fact]
    public void Behavioral_regeneration_leaves_save_tree_byte_identical_and_requires_world_migration()
    {
        var state = Goal161MigrationState.Value;
        Assert.Equal(state.SaveTreeBeforeRegeneration, state.SaveTreeAfterRegeneration);
        Assert.Equal(GeneratedGameplaySaveStatus.WORLD_MIGRATION_REQUIRED, state.StaleLoad.Status);
        Assert.False(state.StaleLoad.Passed);
    }

    [Fact]
    public void Behavioral_migration_preview_is_zero_write()
    {
        var state = Goal161MigrationState.Value;
        Assert.True(state.Preview.Passed, string.Join(Environment.NewLine, state.Preview.Diagnostics));
        Assert.Equal(state.SaveTreeAfterRegeneration, state.SaveTreeAfterPreview);
    }

    [Fact]
    public void Behavioral_cross_world_map_resets_to_current_generated_start()
    {
        var state = Goal161MigrationState.Value;
        Assert.True(state.Preview.MapReset);
        Assert.Equal(state.Regenerated.AuthoritativeSnapshot?.GeneratedWorldActivation?.GeneratedStartMapId,
            state.Migrated.Session?.MapState.CurrentMapId);
    }

    [Fact]
    public void Behavioral_transient_events_encounter_dialogue_and_tick_are_reset()
    {
        var session = Assert.IsType<UnifiedRuntimeSession>(Goal161MigrationState.Value.Migrated.Session);
        Assert.Equal(0, session.GameplayState.Tick);
        Assert.Empty(session.MapEvents);
        Assert.Empty(session.GameplayEvents);
        Assert.Null(session.GameplayState.ActiveEncounter);
        Assert.Null(session.GameplayState.ActiveDialogue);
        Assert.Equal("player", session.MapState.Flags["owner"]);
        Assert.DoesNotContain("remembered_map", session.MapState.Flags.Keys);
    }

    [Fact]
    public void Behavioral_portable_baseline_state_is_preserved_by_canonical_definition()
    {
        var state = Goal161MigrationState.Value;
        Assert.True(state.Preview.PreservedCountsByKind.Values.Sum() > 0);
        Assert.NotEmpty(state.Preview.PreservedDefinitionIds);
        Assert.Equal(state.Preview.PreservedDefinitionIds,
            state.Migrated.Revision?.Migration?.PreservedDefinitionIds);
    }

    [Fact]
    public void Behavioral_dropped_references_have_explicit_reasons()
    {
        var migration = Assert.IsType<GeneratedGameplaySaveMigration>(
            Goal161MigrationState.Value.Migrated.Revision?.Migration);
        Assert.NotEmpty(migration.DroppedDefinitionIds);
        Assert.Equal(migration.DroppedDefinitionIds.Count, migration.DroppedReasons.Count);
        Assert.All(migration.DroppedReasons, reason => Assert.False(string.IsNullOrWhiteSpace(reason)));
    }

    [Fact]
    public void Behavioral_migration_preview_counts_match_committed_report()
    {
        var state = Goal161MigrationState.Value;
        var migration = Assert.IsType<GeneratedGameplaySaveMigration>(state.Migrated.Revision?.Migration);
        Assert.Equal(state.Preview.PreservedCountsByKind, migration.PreservedCounts);
        Assert.Equal(state.Preview.DroppedCountsByKind, migration.DroppedCounts);
    }

    [Fact]
    public void Behavioral_caller_modified_preview_is_rejected_without_write()
    {
        var state = Goal161MigrationState.Value;
        Assert.False(state.TamperedApply.Passed);
        Assert.Contains("generated_save.migration_preview_mismatch", state.TamperedApply.Diagnostics);
        Assert.Equal(state.SaveTreeAfterPreview, state.SaveTreeAfterTamperedApply);
    }

    [Fact]
    public void Behavioral_migration_creates_new_revision_and_retains_source_revision()
    {
        var state = Goal161MigrationState.Value;
        Assert.True(state.Migrated.Passed, string.Join(Environment.NewLine, state.Migrated.Diagnostics));
        Assert.NotEqual(state.Saved.RevisionSha256, state.Migrated.MigratedRevisionSha256);
        Assert.Contains(state.SlotAfterMigration.Revisions,
            revision => revision.RevisionSha256 == state.Saved.RevisionSha256);
        Assert.Equal(state.Migrated.MigratedRevisionSha256,
            state.SlotAfterMigration.Manifest?.CurrentRevisionSha256);
    }

    [Fact]
    public void Behavioral_migrated_revision_is_current_and_exact_load_succeeds()
    {
        var load = Goal161MigrationState.Value.MigratedLoad;
        Assert.True(load.Passed, string.Join(Environment.NewLine, load.Diagnostics));
        Assert.Equal(GeneratedGameplaySaveStatus.CURRENT, load.Status);
        Assert.Equal(load.Revision?.UnifiedRuntimeSessionJson,
            Goal161MigrationState.Value.Bundle.Saves.Serializer.Serialize(load.Session!));
    }

    [Fact]
    public void Behavioral_no_dropped_reference_remains_in_migrated_session()
    {
        var state = Goal161MigrationState.Value;
        var migration = Assert.IsType<GeneratedGameplaySaveMigration>(state.Migrated.Revision?.Migration);
        var json = state.Bundle.Saves.Serializer.Serialize(state.Migrated.Session!);
        Assert.All(migration.DroppedDefinitionIds, id => Assert.DoesNotContain(id, json, StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_history_rollback_leaves_save_tree_byte_identical()
    {
        var state = Goal161MigrationState.Value;
        Assert.True(state.Rollback.Applied, string.Join(Environment.NewLine, state.Rollback.Diagnostics));
        Assert.Equal(state.SaveTreeBeforeRollback, state.SaveTreeAfterRollback);
    }

    [Fact]
    public void Behavioral_original_revision_becomes_current_after_historical_world_restore()
    {
        var load = Goal161MigrationState.Value.OriginalRevisionAfterRollback;
        Assert.True(load.Passed, string.Join(Environment.NewLine, load.Diagnostics));
        Assert.Equal(GeneratedGameplaySaveStatus.CURRENT, load.Status);
    }

    [Fact]
    public void Behavioral_migrated_revision_becomes_world_migration_required_after_restore()
    {
        var load = Goal161MigrationState.Value.MigratedRevisionAfterRollback;
        Assert.False(load.Passed);
        Assert.Equal(GeneratedGameplaySaveStatus.WORLD_MIGRATION_REQUIRED, load.Status);
    }
}

internal static class Goal161MigrationState
{
    private static readonly Lazy<Goal161MigrationFixture> Fixture = new(Goal161MigrationFixture.Create);
    public static Goal161MigrationFixture Value => Fixture.Value;
}

internal sealed record Goal161MigrationFixture(
    GeneratedProject Project,
    Goal161WorldBundle Bundle,
    GeneratedGameplaySaveResult Saved,
    GeneratedGameplaySaveResult SameWorldLoad,
    SortedDictionary<string, string> SaveTreeBeforeRegeneration,
    SortedDictionary<string, string> SaveTreeAfterRegeneration,
    GeneratedGameplaySaveResult StaleLoad,
    GameProjectSeedRegenerationResult Regenerated,
    GeneratedGameplaySaveMigrationPreview Preview,
    SortedDictionary<string, string> SaveTreeAfterPreview,
    GeneratedGameplaySaveMigrationResult TamperedApply,
    SortedDictionary<string, string> SaveTreeAfterTamperedApply,
    GeneratedGameplaySaveMigrationResult Migrated,
    GeneratedGameplaySaveStoreReadResult SlotAfterMigration,
    GeneratedGameplaySaveResult MigratedLoad,
    GamePackageDefinition MigratedPackage,
    SeededGeneratedProjectSourceValidationResult MigratedSource,
    GeneratedWorldTravelRoutePlan Route,
    IReadOnlyList<UnifiedRuntimeResult> FirstRouteResults,
    IReadOnlyList<UnifiedRuntimeResult> ReplayRouteResults,
    string FirstRouteSessionJson,
    string ReplayRouteSessionJson,
    SortedDictionary<string, string> SaveTreeBeforeRollback,
    GameProjectGeneratedWorldRollbackResult Rollback,
    SortedDictionary<string, string> SaveTreeAfterRollback,
    GeneratedGameplaySaveResult OriginalRevisionAfterRollback,
    GeneratedGameplaySaveResult MigratedRevisionAfterRollback)
{
    public static Goal161MigrationFixture Create()
    {
        var project = Goal156TestKit.Copy(Goal157BuildState.Value.Project, "goal161-shared-migration");
        var bundle = Goal161WorldBundle.Create(project.Path);
        var package = Goal156TestKit.Load(project.Path);
        var session = bundle.Saves.Runtime.Start(package).Session;
        session.GameplayState.Tick = 42;
        session.MapState.Flags["remembered_map"] = session.MapState.CurrentMapId;
        session.MapState.Flags["owner"] = "player";
        session.MapEvents.Add(new RuntimeEvent { Type = RuntimeEventType.Message, Message = "transient" });
        session.GameplayEvents.Add(new GameRuntimeEvent { Type = GameRuntimeEventType.LogMessageAdded,
            Message = "transient" });
        var saved = bundle.Saves.Save.Save(project.Path, "campaign", session);
        Assert.True(saved.Passed, string.Join(Environment.NewLine, saved.Diagnostics));
        var sameWorld = bundle.Saves.Save.Load(project.Path, "campaign");
        Assert.True(sameWorld.Passed, string.Join(Environment.NewLine, sameWorld.Diagnostics));
        var beforeRegeneration = Goal159TestKit.TreeHashes(bundle.Saves.Store.RootPath(project.Path));
        var request = bundle.Controller.CreateGeneratedWorldRegenerationRequest(
            Goal159TestKit.ChangedRequest(bundle.Controller.Snapshot(), "goal161-world-b"));
        var candidate = bundle.Controller.PreviewGeneratedWorldRegeneration(request);
        Assert.Equal("GREEN", candidate.Status);
        var regenerated = bundle.Controller.ApplyGeneratedWorldRegeneration(request, candidate);
        Assert.True(regenerated.Applied, string.Join(Environment.NewLine, regenerated.Diagnostics));
        var afterRegeneration = Goal159TestKit.TreeHashes(bundle.Saves.Store.RootPath(project.Path));
        var stale = bundle.Saves.Save.Load(project.Path, "campaign");
        var preview = bundle.Saves.Migration.Preview(project.Path, "campaign");
        var afterPreview = Goal159TestKit.TreeHashes(bundle.Saves.Store.RootPath(project.Path));
        var tampered = bundle.Saves.Migration.Apply(new GeneratedGameplaySaveMigrationApplyRequest
        {
            ProjectFolder = project.Path,
            SlotName = preview.SlotName,
            SourceRevisionSha256 = preview.SourceRevisionSha256,
            CandidateSessionSha256 = new string('0', 64)
        });
        var afterTampered = Goal159TestKit.TreeHashes(bundle.Saves.Store.RootPath(project.Path));
        var migrated = bundle.Saves.Migration.Apply(new GeneratedGameplaySaveMigrationApplyRequest
        {
            ProjectFolder = project.Path,
            SlotName = preview.SlotName,
            SourceRevisionSha256 = preview.SourceRevisionSha256,
            CandidateSessionSha256 = preview.CandidateSessionSha256
        });
        Assert.True(migrated.Passed, string.Join(Environment.NewLine, migrated.Diagnostics));
        var slot = bundle.Saves.Store.ReadSlot(project.Path, "campaign");
        var migratedLoad = bundle.Saves.Save.Load(project.Path, "campaign");
        var migratedPackage = Goal156TestKit.Load(project.Path);
        var migratedSource = bundle.Source.Validate(project.Path);
        var route = new GeneratedWorldTravelRoutePlanner().Plan(migratedSource, migratedPackage);
        Assert.True(route.Passed, string.Join(Environment.NewLine, route.Diagnostics));
        var firstSession = bundle.Saves.Serializer.DeserializeUnifiedSession(
            migratedLoad.Revision!.UnifiedRuntimeSessionJson);
        var first = ExecuteRoute(bundle, migratedPackage, firstSession, route);
        var replaySession = bundle.Saves.Serializer.DeserializeUnifiedSession(
            migratedLoad.Revision!.UnifiedRuntimeSessionJson);
        var replay = ExecuteRoute(bundle, migratedPackage, replaySession, route);
        var firstJson = bundle.Saves.Serializer.Serialize(first[^1].Session);
        var replayJson = bundle.Saves.Serializer.Serialize(replay[^1].Session);
        var beforeRollback = Goal159TestKit.TreeHashes(bundle.Saves.Store.RootPath(project.Path));
        var rollbackRequest = bundle.Controller.CreateGeneratedWorldRollbackRequest(saved.Revision!.WorldId);
        var rollbackPreview = bundle.Controller.PreviewGeneratedWorldRollback(rollbackRequest);
        Assert.Equal("GREEN", rollbackPreview.Status);
        var rollback = bundle.Controller.ApplyGeneratedWorldRollback(rollbackRequest, rollbackPreview);
        Assert.True(rollback.Applied, string.Join(Environment.NewLine, rollback.Diagnostics));
        var afterRollback = Goal159TestKit.TreeHashes(bundle.Saves.Store.RootPath(project.Path));
        return new Goal161MigrationFixture(
            project, bundle, saved, sameWorld, beforeRegeneration, afterRegeneration, stale, regenerated,
            preview, afterPreview, tampered, afterTampered, migrated, slot, migratedLoad,
            migratedPackage, migratedSource, route, first, replay, firstJson, replayJson,
            beforeRollback, rollback, afterRollback,
            bundle.Saves.Save.LoadRevision(project.Path, "campaign", saved.RevisionSha256),
            bundle.Saves.Save.LoadRevision(project.Path, "campaign", migrated.MigratedRevisionSha256));
    }

    private static IReadOnlyList<UnifiedRuntimeResult> ExecuteRoute(
        Goal161WorldBundle bundle,
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        GeneratedWorldTravelRoutePlan route)
    {
        var results = new List<UnifiedRuntimeResult>();
        var runtime = new DefaultGameRuntime();
        foreach (var action in route.Actions)
        {
            var mapResult = runtime.Execute(package, session.MapState, action.Command);
            Assert.True(mapResult.Success, string.Join(Environment.NewLine,
                mapResult.Events.Where(item => item.Type == RuntimeEventType.Error)
                    .Select(item => item.Message)));
            session.MapState = mapResult.State;
            session.MapEvents.AddRange(mapResult.Events);
            Assert.Equal(action.ExpectedMapId, session.MapState.CurrentMapId);
            results.Add(new UnifiedRuntimeResult
            {
                Success = true,
                Session = session,
                MapEvents = mapResult.Events.ToList(),
                Message = mapResult.Success ? "Player command executed." : "Player command failed."
            });
        }
        return results;
    }
}
