using System.Reflection;
using System.Windows.Forms;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Tests.Application.Goal157;
using LLMGameCreator.Tests.Application.Goal160;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal162;

[Collection(Goal160Collection.Name)]
public sealed class Goal162WinFormsWorkspaceTests
{
    [Fact]
    public void Behavioral_campaign_page_is_registered_immediately_before_runtime_simulator()
    {
        Goal157TestKit.RunSta(() =>
        {
            using var campaign = new GeneratedCampaignPageControl();
            using var simulator = new RuntimeSimulatorPageControl();
            var registry = new EditorPageRegistry([simulator, campaign]);

            Assert.Equal("generated-campaign-player", registry.Pages[0].Id);
            Assert.Equal("runtime-simulator", registry.Pages[1].Id);
            Assert.Equal("Играть", registry.Pages[0].Title);
        });
    }

    [Fact]
    public void Behavioral_campaign_workspace_uses_three_bounded_primary_columns()
    {
        Goal157TestKit.RunSta(() =>
        {
            using var page = new GeneratedCampaignPageControl();
            var workspace = Goal162ProjectsTestKit.Field<TableLayoutPanel>(page, "_workspace");
            var root = Goal162ProjectsTestKit.Field<TableLayoutPanel>(page, "_rootLayout");

            Assert.Equal(3, workspace.ColumnCount);
            Assert.Equal(3, root.RowCount);
            Assert.False(page.AutoScroll);
            Assert.True(page.MinimumSize.Width >= 900);
        });
    }

    [Fact]
    public void Behavioral_technical_details_are_collapsed_by_default_and_explicitly_toggle()
    {
        Goal157TestKit.RunSta(() =>
        {
            using var page = new GeneratedCampaignPageControl();
            using var host = new Form();
            host.Controls.Add(page);
            page.Dock = DockStyle.Fill;
            host.Show();
            System.Windows.Forms.Application.DoEvents();
            var technical = Goal162ProjectsTestKit.Field<TextBox>(page, "_technical");
            var toggle = Goal162ProjectsTestKit.Field<CheckBox>(page, "_technicalToggle");

            Assert.False(technical.Visible);
            toggle.Checked = true;
            InvokeTechnicalToggleChanged(page);
            Assert.Equal(120F,
                Goal162ProjectsTestKit.Field<TableLayoutPanel>(page, "_rootLayout").RowStyles[2].Height);
            toggle.Checked = false;
            InvokeTechnicalToggleChanged(page);
            Assert.Equal(0F,
                Goal162ProjectsTestKit.Field<TableLayoutPanel>(page, "_rootLayout").RowStyles[2].Height);
        });
    }

    [Fact]
    public void Behavioral_active_campaign_binds_real_map_context_hud_and_actions()
    {
        var service = Goal162TestKit.Service();
        var snapshot = service.StartNew();
        Goal157TestKit.RunSta(() =>
        {
            using var page = new GeneratedCampaignPageControl(service);
            page.OnActivated();
            var map = Goal162ProjectsTestKit.Field<GeneratedCampaignMapControl>(page, "_map");
            var actions = Goal162ProjectsTestKit.Field<FlowLayoutPanel>(page, "_actions");
            var hud = Goal162ProjectsTestKit.Field<TabControl>(page, "_hud");

            Assert.Equal(snapshot.Map?.Width, map.Projection?.Width);
            Assert.Equal(snapshot.Actions.Count, actions.Controls.OfType<Button>().Count());
            Assert.Equal(7, hud.TabPages.Count);
            Assert.Contains(hud.TabPages.Cast<TabPage>(),
                tab => tab.Text == "Отношения");
            Assert.All(hud.TabPages.Cast<TabPage>(), tab => Assert.NotEmpty(tab.Controls));
        });
    }

    [Fact]
    public void Behavioral_primary_campaign_controls_do_not_render_raw_ids_hashes_or_paths()
    {
        var service = Goal162TestKit.Service();
        service.StartNew();
        Goal157TestKit.RunSta(() =>
        {
            using var page = new GeneratedCampaignPageControl(service);
            page.OnActivated();
            var technical = Goal162ProjectsTestKit.Field<TextBox>(page, "_technical");
            var text = string.Join(Environment.NewLine, Descendants(page)
                .Where(control => control != technical)
                .Select(control => control.Text));

            Assert.DoesNotContain("generated/", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(".llmgc", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotMatch("[A-Fa-f0-9]{48,}", text);
            Assert.DoesNotMatch("[A-Za-z]:\\\\", text);
        });
    }

    [Fact]
    public void Behavioral_action_buttons_share_one_durable_tooltip_component()
    {
        var service = Goal162TestKit.Service();
        service.StartNew();
        Goal157TestKit.RunSta(() =>
        {
            using var page = new GeneratedCampaignPageControl(service);
            page.OnActivated();
            var actions = Goal162ProjectsTestKit.Field<FlowLayoutPanel>(page, "_actions")
                .Controls.OfType<Button>().ToList();
            var tooltip = Goal162ProjectsTestKit.Field<ToolTip>(page, "_actionToolTip");

            Assert.NotEmpty(actions);
            Assert.All(actions, button => Assert.False(string.IsNullOrWhiteSpace(tooltip.GetToolTip(button))));
        });
    }

    [Fact]
    public void Behavioral_map_control_emits_only_in_bounds_cell_clicks()
    {
        Goal157TestKit.RunSta(() =>
        {
            using var map = new GeneratedCampaignMapControl
            {
                Projection = new GeneratedCampaignMapProjection
                {
                    Width = 2,
                    Height = 1,
                    Cells =
                    [
                        new GeneratedCampaignMapCell { X = 0, Y = 0, Walkable = true, PrimaryTitle = "Поле" },
                        new GeneratedCampaignMapCell { X = 1, Y = 0, Walkable = true, PrimaryTitle = "Дорога" }
                    ]
                }
            };
            var clicks = new List<(int X, int Y)>();
            map.CellClicked += (_, cell) => clicks.Add(cell);
            InvokeMouseClick(map, 8, 8);
            InvokeMouseClick(map, 500, 500);

            Assert.Equal([(0, 0)], clicks);
        });
    }

    [Fact]
    public void Behavioral_save_picker_enables_continue_only_for_current_save()
    {
        Goal157TestKit.RunSta(() =>
        {
            using var dialog = new GeneratedCampaignSavePickerDialog([
                new GeneratedGameplaySaveEntry
                {
                    SlotName = "поход",
                    Status = GeneratedGameplaySaveStatus.CURRENT,
                    RevisionCount = 2,
                    SavedWorldTitle = "Синий овраг",
                    CurrentWorldTitle = "Синий овраг"
                }
            ]);
            var list = Goal162ProjectsTestKit.Field<ListView>(dialog, "_list");
            dialog.Show();
            System.Windows.Forms.Application.DoEvents();
            list.Items[0].Selected = true;
            InvokeSelectionChanged(dialog);

            Assert.True(Goal162ProjectsTestKit.Field<Button>(dialog, "_continue").Enabled);
            Assert.False(Goal162ProjectsTestKit.Field<Button>(dialog, "_migrate").Enabled);
            Assert.Contains("Синий овраг", Goal162ProjectsTestKit.Field<Label>(dialog, "_details").Text);
        });
    }

    [Fact]
    public void Behavioral_save_picker_enables_only_explicit_migration_for_stale_world()
    {
        Goal157TestKit.RunSta(() =>
        {
            using var dialog = new GeneratedCampaignSavePickerDialog([
                new GeneratedGameplaySaveEntry
                {
                    SlotName = "старый поход",
                    Status = GeneratedGameplaySaveStatus.WORLD_MIGRATION_REQUIRED,
                    RevisionCount = 1,
                    SavedWorldTitle = "Старый мир",
                    CurrentWorldTitle = "Новый мир"
                }
            ]);
            var list = Goal162ProjectsTestKit.Field<ListView>(dialog, "_list");
            dialog.Show();
            System.Windows.Forms.Application.DoEvents();
            list.Items[0].Selected = true;
            InvokeSelectionChanged(dialog);

            Assert.False(Goal162ProjectsTestKit.Field<Button>(dialog, "_continue").Enabled);
            Assert.True(Goal162ProjectsTestKit.Field<Button>(dialog, "_migrate").Enabled);
        });
    }

    private static IEnumerable<Control> Descendants(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;
            foreach (var nested in Descendants(child)) yield return nested;
        }
    }

    private static void InvokeMouseClick(GeneratedCampaignMapControl control, int x, int y)
    {
        typeof(GeneratedCampaignMapControl).GetMethod("OnMouseClick",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(control, [new MouseEventArgs(MouseButtons.Left, 1, x, y, 0)]);
    }

    private static void InvokeSelectionChanged(GeneratedCampaignSavePickerDialog dialog)
    {
        typeof(GeneratedCampaignSavePickerDialog).GetMethod("SelectionChanged",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(dialog, [null, EventArgs.Empty]);
    }

    private static void InvokeTechnicalToggleChanged(GeneratedCampaignPageControl page)
    {
        typeof(GeneratedCampaignPageControl).GetMethod("TechnicalToggleChanged",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(page, [null, EventArgs.Empty]);
    }
}
