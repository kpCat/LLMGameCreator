using System.Reflection;
using System.Windows.Forms;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Runtime;
using LLMGameCreator.Tests.Application.Goal157;
using LLMGameCreator.Tests.Application.Goal160;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161;

[Collection(Goal160Collection.Name)]
public sealed class Goal161SaveUiOperationTests
{
    [Theory]
    [InlineData(GameProjectOperationKinds.RegenerationApply)]
    [InlineData(GameProjectOperationKinds.WorldHistoryRollbackApply)]
    [InlineData(GameProjectOperationKinds.Build)]
    [InlineData(GameProjectOperationKinds.Standalone)]
    public void Behavioral_save_rejects_while_world_or_build_operation_owns_project(string operationKind)
    {
        using var fixture = Goal161SaveFixture.Create("save-race-" + operationKind);
        using var owner = fixture.Services.Coordinator.TryAcquire(fixture.Project.Path, operationKind);
        var result = fixture.Services.Save.Save(fixture.Project.Path, "slot1", fixture.Session);
        Assert.True(owner.Acquired);
        Assert.False(result.Passed);
        Assert.Equal("project_operation.busy:" + operationKind, Assert.Single(result.Diagnostics));
    }

    [Fact]
    public void Behavioral_world_change_rejects_while_save_migration_owns_project()
    {
        using var fixture = Goal161SaveFixture.Create("migration-race");
        using var migration = fixture.Services.Coordinator.TryAcquire(
            fixture.Project.Path, GameProjectOperationKinds.GameplaySaveMigration);
        using var rejected = fixture.Services.Coordinator.TryAcquire(
            fixture.Project.Path, GameProjectOperationKinds.RegenerationApply);
        Assert.True(migration.Acquired);
        Assert.False(rejected.Acquired);
        Assert.Equal("project_operation.busy:gameplay_save_migration", rejected.Diagnostic);
    }

    [Fact]
    public void Behavioral_RuntimeSimulator_generated_project_selects_generated_save_service()
    {
        Goal157TestKit.RunSta(() =>
        {
            var state = Goal161MigrationState.Value;
            using var page = new RuntimeSimulatorPageControl(
                state.Bundle.Current, state.Bundle.Saves.GameplayRuntime, state.Bundle.Saves.Runtime,
                state.Bundle.Saves.Serializer, state.Bundle.Saves.Legacy,
                state.Bundle.Saves.Save, state.Bundle.Saves.Migration);
            var generated = (bool)typeof(RuntimeSimulatorPageControl).GetMethod(
                "GeneratedProject", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(page, [state.Project.Path])!;
            Assert.True(generated);
            Assert.Equal("Перенести сохранение", Field<Button>(page, "_migrateSnapshotButton").Text);
        });
    }

    [Fact]
    public void Behavioral_generated_raw_legacy_snapshot_is_listed_legacy_raw_and_not_direct_loaded()
    {
        using var fixture = Goal161SaveFixture.Create("legacy-raw");
        Assert.True(fixture.Services.Legacy.SaveSnapshot(
            fixture.Project.Path, "old", fixture.Session).Success);
        var list = fixture.Services.Save.List(fixture.Project.Path);
        var legacy = Assert.Single(list.Entries, entry => entry.LegacyRaw);
        Assert.Equal(GeneratedGameplaySaveStatus.LEGACY_RAW, legacy.Status);
        Assert.False(fixture.Services.Save.Load(fixture.Project.Path, "old").Passed);
    }

    [Fact]
    public void Behavioral_projects_save_card_reports_current_migration_and_invalid_counts()
    {
        Goal157TestKit.RunSta(() =>
        {
            var state = Goal161MigrationState.Value;
            var snapshot = state.Bundle.Controller.Snapshot();
            using var page = new ProjectsPageControl(null!, null!, null!, null!, state.Bundle.Controller);
            typeof(ProjectsPageControl).GetMethod("BindGeneratedGameplaySavesCard",
                BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, [snapshot]);
            var text = Field<Label>(page, "_generatedGameplaySavesCardLabel").Text;
            Assert.Contains("Игровые сохранения", text, StringComparison.Ordinal);
            Assert.Contains("Слотов    " + snapshot.GeneratedGameplaySaves?.SlotCount, text,
                StringComparison.Ordinal);
            Assert.Contains("Требуют переноса    " + snapshot.GeneratedGameplaySaveMigrationRequiredCount,
                text, StringComparison.Ordinal);
            Assert.Contains("Повреждено    " + snapshot.GeneratedGameplaySaveInvalidCount,
                text, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void Behavioral_save_manager_dialog_enables_explicit_migration_preview_only_for_stale_slot()
    {
        Goal157TestKit.RunSta(() =>
        {
            var state = Goal161MigrationState.Value;
            using var dialog = new GeneratedGameplaySavesDialog(state.Bundle.Controller);
            dialog.Show();
            var list = Field<ListView>(dialog, "_savesListView");
            list.Items[0].Selected = true;
            list.Items[0].Focused = true;
            System.Windows.Forms.Application.DoEvents();
            typeof(GeneratedGameplaySavesDialog).GetMethod("RefreshSelection",
                BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(dialog, null);
            Assert.Equal(1, dialog.EntryCount);
            Assert.True(dialog.CanPreview);
            Assert.False(dialog.CanApply);
            Assert.Contains("Требуется перенос", list.Items[0].SubItems[1].Text,
                StringComparison.Ordinal);
        });
    }

    [Fact]
    public void Behavioral_primary_save_ui_contains_no_full_hash_paths_or_generated_ids()
    {
        Goal157TestKit.RunSta(() =>
        {
            var state = Goal161MigrationState.Value;
            using var dialog = new GeneratedGameplaySavesDialog(state.Bundle.Controller);
            var list = Field<ListView>(dialog, "_savesListView");
            var visible = string.Join("|", list.Items.Cast<ListViewItem>()
                .SelectMany(item => item.SubItems.Cast<ListViewItem.ListViewSubItem>())
                .Select(item => item.Text));
            Assert.DoesNotContain(state.Migrated.MigratedRevisionSha256, visible, StringComparison.Ordinal);
            Assert.DoesNotContain(state.Project.Path, visible, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("generated/", visible, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(state.Migrated.MigratedRevisionSha256[..12], visible, StringComparison.Ordinal);
        });
    }

    private static T Field<T>(object instance, string name) where T : class =>
        (T)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(instance)!;
}
