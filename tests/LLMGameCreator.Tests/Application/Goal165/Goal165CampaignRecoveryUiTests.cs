using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Tests.Application.Goal157;
using LLMGameCreator.Tests.Application.Goal162;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal165;

public sealed class Goal165CampaignRecoveryUiTests
{
    [Fact]
    public void Behavioral_defeated_snapshot_hides_ordinary_actions_and_shows_recovery_actions()
    {
        var snapshot = Snapshot();

        Assert.Equal(GeneratedCampaignSessionStatus.DEFEATED, snapshot.Status);
        Assert.DoesNotContain(snapshot.Actions, action => action.Kind is GeneratedCampaignActionKind.MoveUp
            or GeneratedCampaignActionKind.MoveDown or GeneratedCampaignActionKind.MoveLeft
            or GeneratedCampaignActionKind.MoveRight or GeneratedCampaignActionKind.Interact);
        Assert.Contains(snapshot.Actions, action => action.Kind == GeneratedCampaignActionKind.RetryEncounter);
    }

    [Fact]
    public void Behavioral_defeated_recovery_context_uses_human_title()
    {
        var snapshot = Snapshot();
        Goal157TestKit.RunSta(() =>
        {
            using var page = new GeneratedCampaignPageControl();
            Bind(page, snapshot);

            Assert.Equal("Поражение", ContextTitle(page));
        });
    }

    [Fact]
    public void Behavioral_defeated_ui_has_no_enabled_movement_or_interact_action()
    {
        var snapshot = Snapshot();

        Assert.DoesNotContain(snapshot.Actions, action => action.Enabled && action.Kind is
            GeneratedCampaignActionKind.MoveUp or GeneratedCampaignActionKind.MoveDown
            or GeneratedCampaignActionKind.MoveLeft or GeneratedCampaignActionKind.MoveRight
            or GeneratedCampaignActionKind.Interact);
    }

    [Fact]
    public void Behavioral_recovery_primary_ui_excludes_raw_ids_hashes_paths_and_diagnostics()
    {
        var snapshot = Snapshot();
        Goal157TestKit.RunSta(() =>
        {
            using var page = new GeneratedCampaignPageControl();
            Bind(page, snapshot);
            var primary = string.Join(Environment.NewLine, ActionTitles(page).Append(ContextTitle(page)));

            Assert.DoesNotContain("encounter/", primary, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("campaign.", primary, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotMatch("[A-Fa-f0-9]{48,}", primary);
            Assert.DoesNotMatch("[A-Za-z]:\\\\", primary);
        });
    }

    [Fact]
    public void Contract_recovery_controls_fit_the_1100x720_campaign_surface()
    {
        var snapshot = Snapshot();
        Goal157TestKit.RunSta(() =>
        {
            using var page = new GeneratedCampaignPageControl { Size = new Size(1100, 720) };
            Bind(page, snapshot);

            Assert.True(page.Width >= 1100);
            Assert.True(page.Height >= 720);
            Assert.Equal(3, ActionTitles(page).Count);
        });
    }

    private static GeneratedCampaignSnapshot Snapshot()
    {
        var recovery = Goal165RecoveryHarness.WithCheckpoint().Recovery.Project(false,
            "Нет совместимого сохранения для продолжения.");
        return new GeneratedCampaignSnapshot
        {
            Status = GeneratedCampaignSessionStatus.DEFEATED,
            StatusTitle = "Поражение",
            ProjectTitle = "Проверочная кампания",
            CurrentRegionTitle = "Начальный регион",
            Recovery = recovery,
            Actions = Goal165RecoveryHarness.WithCheckpoint().Recovery.RecoveryActions(recovery),
            TechnicalDetails = new Dictionary<string, string>
            {
                ["projectFolder"] = @"C:\not-for-primary-ui",
                ["packageSha256"] = new string('a', 64)
            }
        };
    }

    private static void Bind(GeneratedCampaignPageControl page, GeneratedCampaignSnapshot snapshot) =>
        typeof(GeneratedCampaignPageControl).GetMethod("Bind", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(page, [snapshot]);

    private static string ContextTitle(GeneratedCampaignPageControl page) =>
        Goal162ProjectsTestKit.Field<Label>(page, "_contextTitle").Text;

    private static IReadOnlyList<string> ActionTitles(GeneratedCampaignPageControl page) =>
        Goal162ProjectsTestKit.Field<FlowLayoutPanel>(page, "_actions").Controls
            .OfType<Button>().Select(button => button.Text).ToList();
}
