using System.Reflection;
using System.Windows.Forms;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Tests.Application.Goal157;
using LLMGameCreator.Tests.Application.Goal160;
using LLMGameCreator.Tests.Application.Goal162;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal163;

[Collection(Goal160Collection.Name)]
public sealed class Goal163CampaignTruthUiTests
{
    [Fact]
    public void Behavioral_hud_contains_human_consequence_tab()
    {
        Goal157TestKit.RunSta(() =>
        {
            using var page = new GeneratedCampaignPageControl();
            var hud = Goal162ProjectsTestKit.Field<TabControl>(page, "_hud");

            Assert.Contains(hud.TabPages.Cast<TabPage>(), tab => tab.Text == "Последствия");
        });
    }

    [Fact]
    public void Behavioral_damage_consequence_renders_title_values_and_delta()
    {
        var snapshot = Snapshot(new GeneratedCampaignConsequence
        {
            Kind = GeneratedCampaignConsequenceKind.Damage,
            Title = "Противник получает урон",
            BeforeValue = "3", AfterValue = "1", Delta = "-2",
            Description = "Здоровье уменьшилось."
        });

        var text = Render(snapshot);

        Assert.Contains("Противник получает урон", text);
        Assert.Contains("3", text);
        Assert.Contains("1", text);
        Assert.Contains("-2", text);
    }

    [Fact]
    public void Behavioral_quest_completion_and_reputation_are_visible_together()
    {
        var text = Render(Snapshot(
            new GeneratedCampaignConsequence { Kind = GeneratedCampaignConsequenceKind.QuestCompleted, Title = "Задание завершено" },
            new GeneratedCampaignConsequence { Kind = GeneratedCampaignConsequenceKind.Reputation, Title = "Репутация улучшена", Delta = "+5" }));

        Assert.Contains("Задание завершено", text);
        Assert.Contains("Репутация улучшена", text);
        Assert.Contains("+5", text);
    }

    [Fact]
    public void Behavioral_save_load_and_migration_rows_are_human_readable()
    {
        var text = Render(Snapshot(
            new GeneratedCampaignConsequence { Kind = GeneratedCampaignConsequenceKind.Save, Title = "Игра сохранена" },
            new GeneratedCampaignConsequence { Kind = GeneratedCampaignConsequenceKind.Load, Title = "Игра продолжена" },
            new GeneratedCampaignConsequence { Kind = GeneratedCampaignConsequenceKind.Migration, Title = "Сохранение перенесено" }));

        Assert.Contains("Игра сохранена", text);
        Assert.Contains("Игра продолжена", text);
        Assert.Contains("Сохранение перенесено", text);
    }

    [Fact]
    public void Behavioral_primary_consequence_text_excludes_raw_ids_hashes_and_paths()
    {
        var snapshot = Snapshot(new GeneratedCampaignConsequence
        {
            Kind = GeneratedCampaignConsequenceKind.MapTravel,
            Title = "Переход в новый регион",
            Description = "Путь подтверждён игровым состоянием."
        }) with
        {
            TechnicalDetails = new Dictionary<string, string>
            {
                ["packageSha256"] = new string('a', 64),
                ["projectFolder"] = @"C:\secret\.llmgc",
                ["definitionId"] = "generated/map/raw"
            }
        };

        var text = Render(snapshot);

        Assert.DoesNotContain("generated/", text);
        Assert.DoesNotContain(".llmgc", text);
        Assert.DoesNotMatch("[A-Fa-f0-9]{48,}", text);
        Assert.DoesNotMatch("[A-Za-z]:\\\\", text);
    }

    [Fact]
    public void Behavioral_empty_consequence_timeline_has_clear_placeholder()
    {
        var text = Render(Snapshot());

        Assert.False(string.IsNullOrWhiteSpace(text));
        Assert.Equal("Пока нет данных.", text);
    }

    private static GeneratedCampaignSnapshot Snapshot(params GeneratedCampaignConsequence[] rows) => new()
    {
        Status = GeneratedCampaignSessionStatus.ACTIVE,
        ProjectTitle = "Проверочная кампания",
        CurrentRegionTitle = "Стартовый регион",
        StatusTitle = "Игра запущена",
        Consequences = rows
    };

    private static string Render(GeneratedCampaignSnapshot snapshot)
    {
        var text = string.Empty;
        Goal157TestKit.RunSta(() =>
        {
            using var page = new GeneratedCampaignPageControl();
            typeof(GeneratedCampaignPageControl).GetMethod("Bind", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(page, [snapshot]);
            text = Goal162ProjectsTestKit.Field<TabPage>(page, "_consequencesTab")
                .Controls.OfType<Label>().First().Text;
        });
        return text;
    }
}
