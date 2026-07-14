using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal155;

public sealed class Goal155WinFormsRcCardTests
{
    [Fact]
    public void Behavioral_build_green_pending_card_is_visible_readable_and_replaces_duplicate_social_card()
    {
        RunSta(() =>
        {
            using var page = new ProjectsPageControl { Size = new Size(1100, 720) };
            Bind(page, Snapshot("BUILD_GREEN_STANDALONE_PENDING"));
            var panel = Field<Panel>(page, "_releaseCandidateCardPanel");
            Assert.True(LocalVisible(panel));
            Assert.False(LocalVisible(Field<Panel>(page, "_socialCardPanel")));
            Assert.Contains("Принятые механики — Release Candidate", Field<Label>(page, "_releaseCandidateCardLabel").Text);
            Assert.Contains("Windows RC ещё не подтверждён", Field<Label>(page, "_releaseCandidateCardLabel").Text);
        });
    }

    [Fact]
    public void Behavioral_ready_card_shows_exact_integrated_profile_values()
    {
        RunSta(() =>
        {
            using var page = new ProjectsPageControl();
            Bind(page, Snapshot("CURRENT"));
            var text = Field<Label>(page, "_releaseCandidateCardLabel").Text;
            Assert.Contains("Статус: RC готов", text);
            Assert.Contains("+3 / +6 / +9", text);
            Assert.Contains("урон 2; 12 → 9", text);
            Assert.Contains("1 за ход; завершён", text);
            Assert.Contains("0 → 10", text);
            Assert.Contains("0 → 10 → 17", text);
            Assert.Contains("cache reused; проверки пройдены", text);
        });
    }

    [Theory]
    [InlineData("LAST_SUCCESS", "последняя успешная RC-проверка")]
    [InlineData("UNKNOWN", "соответствие текущим настройкам не подтверждено")]
    public void Behavioral_last_success_and_unknown_headings_are_truthful(string state, string expected)
    {
        RunSta(() =>
        {
            using var page = new ProjectsPageControl();
            Bind(page, Snapshot(state));
            Assert.Contains(expected, Field<Label>(page, "_releaseCandidateCardLabel").Text);
        });
    }

    [Fact]
    public void Behavioral_card_has_no_ids_hashes_paths_and_fits_normal_project_dimensions()
    {
        RunSta(() =>
        {
            using var page = new ProjectsPageControl { Size = new Size(1100, 720) };
            page.CreateControl();
            Bind(page, Snapshot("CURRENT"));
            page.PerformLayout();
            var panel = Field<Panel>(page, "_releaseCandidateCardPanel");
            var label = Field<Label>(page, "_releaseCandidateCardLabel");
            Assert.True(panel.Height >= 200);
            Assert.Equal(DockStyle.Fill, label.Dock);
            Assert.DoesNotContain("feature.", label.Text, StringComparison.Ordinal);
            Assert.DoesNotContain("sha", label.Text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\\", label.Text, StringComparison.Ordinal);
            Assert.DoesNotContain("C:", label.Text, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void Behavioral_absent_or_incomplete_summary_never_claims_ready()
    {
        RunSta(() =>
        {
            using var page = new ProjectsPageControl();
            Bind(page, new UnifiedGameProjectWorkspaceSnapshot
            {
                AcceptedMechanics = new GameProjectAcceptedMechanicsSummary
                {
                    Present = true,
                    Passed = false,
                    MissingFactKinds = ["ability"]
                },
                ReleaseCandidateConfigurationStatus = "ABSENT"
            });
            Assert.False(LocalVisible(Field<Panel>(page, "_releaseCandidateCardPanel")));
            Assert.DoesNotContain("RC готов", Field<Label>(page, "_releaseCandidateCardLabel").Text);
        });
    }

    private static UnifiedGameProjectWorkspaceSnapshot Snapshot(string status)
    {
        var build = Goal155AcceptedMechanicsProjectionTests.Complete();
        var accepted = new GameProjectAcceptedMechanicsSummaryService().Project(build);
        return new UnifiedGameProjectWorkspaceSnapshot
        {
            AcceptedMechanics = accepted,
            Social = build.Social,
            SocialConfigurationStatus = status is "LAST_SUCCESS" or "UNKNOWN" ? status : "CURRENT",
            ReleaseCandidateConfigurationStatus = status
        };
    }

    private static void Bind(ProjectsPageControl page, UnifiedGameProjectWorkspaceSnapshot snapshot) =>
        page.GetType().GetMethod("BindWorkspace", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, [snapshot]);

    private static T Field<T>(object target, string name) where T : class =>
        (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(target)!;

    private static bool LocalVisible(Control control) => (bool)typeof(Control)
        .GetMethod("GetState", BindingFlags.Instance | BindingFlags.NonPublic)!
        .Invoke(control, [2])!;

    private static void RunSta(Action action)
    {
        Exception? captured = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception exception) { captured = exception; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (captured is not null) throw new Xunit.Sdk.XunitException(captured.ToString());
    }
}
