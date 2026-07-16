using System.Reflection;
using System.Windows.Forms;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.WinForms.Pages;
using LLMGameCreator.Tests.Application.Goal157;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal160;

[Collection(Goal160Collection.Name)]
public sealed class Goal160WorkspaceUiTests
{
    [Fact]
    public void Behavioral_history_dialog_lists_current_first_and_previous_worlds()
    {
        Goal157TestKit.RunSta(() =>
        {
            using var dialog = new GeneratedWorldHistoryDialog(Goal160RollbackState.Value.FinalHistory);
            var list = Field<ListView>(dialog, "_worldsListView");
            Assert.Equal(2, list.Items.Count);
            Assert.Equal("Да", list.Items[0].Text);
            Assert.Equal(string.Empty, list.Items[1].Text);
        });
    }

    [Fact]
    public void Behavioral_current_entry_cannot_be_restored()
    {
        Goal157TestKit.RunSta(() =>
        {
            using var dialog = new GeneratedWorldHistoryDialog(Goal160RollbackState.Value.FinalHistory);
            var list = Field<ListView>(dialog, "_worldsListView");
            list.Items[0].Selected = true;
            Assert.False(dialog.CanRestore);
        });
    }

    [Fact]
    public void Behavioral_noncurrent_entry_enables_check_and_restore()
    {
        Goal157TestKit.RunSta(() =>
        {
            using var dialog = new GeneratedWorldHistoryDialog(Goal160RollbackState.Value.FinalHistory);
            dialog.Show();
            var list = Field<ListView>(dialog, "_worldsListView");
            list.Items[0].Selected = false;
            list.Items[1].Selected = true;
            list.Items[1].Focused = true;
            System.Windows.Forms.Application.DoEvents();
            typeof(GeneratedWorldHistoryDialog).GetMethod("RefreshSelection",
                BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(dialog, null);
            Assert.True(dialog.CanRestore);
            Assert.Equal("Проверить и восстановить", Field<Button>(dialog, "_restoreButton").Text);
        });
    }

    [Fact]
    public void Behavioral_primary_history_list_is_data_derived_without_world_ids()
    {
        Goal157TestKit.RunSta(() =>
        {
            var history = Goal160RollbackState.Value.FinalHistory;
            using var dialog = new GeneratedWorldHistoryDialog(history);
            var list = Field<ListView>(dialog, "_worldsListView");
            var visible = string.Join("|", list.Items.Cast<ListViewItem>()
                .SelectMany(item => item.SubItems.Cast<ListViewItem.ListViewSubItem>())
                .Select(item => item.Text));
            Assert.All(history.Entries, entry => Assert.DoesNotContain(entry.WorldId, visible, StringComparison.Ordinal));
            Assert.Contains(history.Entries.Single(entry => entry.IsCurrent).Manifest!.Seed, visible, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void Behavioral_result_card_shows_history_restore_and_standalone_pending()
    {
        var snapshot = Goal160RollbackState.Value.Result.AuthoritativeSnapshot!;
        var text = (string)typeof(ProjectsPageControl).GetMethod("FormatWorldHistoryCard",
            BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [snapshot])!;
        Assert.Contains("Сохранённых миров", text, StringComparison.Ordinal);
        Assert.Contains("восстановление из истории", text, StringComparison.Ordinal);
        Assert.Contains("требуется повторная проверка", text, StringComparison.Ordinal);
        Assert.DoesNotContain(Goal160RollbackState.Value.TargetWorldId, text, StringComparison.Ordinal);
    }

    [Fact]
    public void Behavioral_technical_details_expose_world_seal_and_transaction_ids()
    {
        Goal157TestKit.RunSta(() =>
        {
            var state = Goal160RollbackState.Value;
            using var page = new ProjectsPageControl(null!, null!, null!, null!, state.Bundle.Controller);
            typeof(ProjectsPageControl).GetMethod("BindTechnicalDetails",
                BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, [state.Result.AuthoritativeSnapshot!]);
            var text = Field<TextBox>(page, "_technicalDetailsTextBox").Text;
            Assert.Contains(state.WorldChangeRecord.FromWorldId, text, StringComparison.Ordinal);
            Assert.Contains(state.WorldChangeRecord.ToWorldId, text, StringComparison.Ordinal);
            Assert.Contains(state.WorldChangeRecord.CandidateSealSha256, text, StringComparison.Ordinal);
            Assert.Contains("Transaction state: committed", text, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void Behavioral_history_button_is_visible_for_valid_generated_project()
    {
        Goal157TestKit.RunSta(() =>
        {
            var state = Goal160RollbackState.Value;
            using var page = new ProjectsPageControl(null!, null!, null!, null!, state.Bundle.Controller);
            using var host = new Form();
            host.Controls.Add(page);
            host.Show();
            typeof(ProjectsPageControl).GetMethod("ShowWorkspace",
                BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, null);
            typeof(ProjectsPageControl).GetMethod("BindWorkspace",
                BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, [state.Result.AuthoritativeSnapshot!]);
            var button = Field<Button>(page, "_generatedWorldHistoryButton");
            Assert.True(button.Visible);
            Assert.True(button.Enabled);
        });
    }

    private static T Field<T>(object instance, string name) where T : class =>
        (T)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(instance)!;
}
