using System.Text.Json;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal155;

public sealed class Goal155HumanAcceptanceLedgerTests
{
    private const string AcceptedCommit = "fc2ac34db60d2627e1cafc86493396937bf63fe4";
    private const string Statement = "Я принимаю Goals154/154A/154B/154B1/154C/154C1/154C2/154C3/154D: в реальном проекте со всеми 22 выбранными механиками и 10 настроенными параметрами социальные механики успешно собраны и воспроизведены без отключения Alchemy Focus и других профилей; репутация изменилась 0→10, квест завершился, доверенная награда была получена один раз, золото изменилось 0→10→17; значения и карточка сохранились после повторного открытия, standalone показал те же факты и переиспользовал host cache без запуска Unity Editor.";

    [Fact]
    public void Exact_owner_acceptance_statement_is_recorded_byte_for_byte()
    {
        using var record = AcceptanceRecord();
        Assert.Equal(Statement, record.RootElement.GetProperty("exactStatement").GetString());
        Assert.Contains(Statement, File.ReadAllText(Path.Combine(Root(), "docs", "manual-acceptance",
            "goal154-family-human-acceptance.md")), StringComparison.Ordinal);
    }

    [Fact]
    public void Every_named_goal154_family_flag_is_human_true_and_never_codex_true()
    {
        using var state = JsonDocument.Parse(File.ReadAllText(Path.Combine(Root(), "docs", "CURRENT_GENERATOR_STATE.json")));
        foreach (var goal in new[] { "goal154", "goal154a", "goal154b", "goal154b1", "goal154c", "goal154c1", "goal154c2", "goal154c3", "goal154d" })
        {
            Assert.True(state.RootElement.GetProperty(goal + "Accepted").GetBoolean(), goal);
            Assert.True(state.RootElement.GetProperty(goal + "AcceptedByHuman").GetBoolean(), goal);
            Assert.False(state.RootElement.GetProperty(goal + "AcceptedByCodex").GetBoolean(), goal);
            Assert.True(state.RootElement.GetProperty(goal + "ManualReviewPerformed").GetBoolean(), goal);
        }
    }

    [Fact]
    public void Historical_intermediate_implementation_statuses_remain_honest()
    {
        using var state = JsonDocument.Parse(File.ReadAllText(Path.Combine(Root(), "docs", "CURRENT_GENERATOR_STATE.json")));
        Assert.Contains("FAILED", state.RootElement.GetProperty("goal154aImplementationStatus").GetString(), StringComparison.Ordinal);
        Assert.Equal("GREEN", state.RootElement.GetProperty("goal154bImplementationStatus").GetString());
        Assert.Equal("GREEN", state.RootElement.GetProperty("goal154dImplementationStatus").GetString());
    }

    [Fact]
    public void Accepted_implementation_commit_equals_required_base()
    {
        using var record = AcceptanceRecord();
        Assert.Equal(AcceptedCommit, record.RootElement.GetProperty("acceptedAtRepositoryBase").GetString());
        using var state = JsonDocument.Parse(File.ReadAllText(Path.Combine(Root(), "docs", "CURRENT_GENERATOR_STATE.json")));
        Assert.Equal(AcceptedCommit, state.RootElement.GetProperty("goal154FamilyAcceptedImplementationCommit").GetString());
    }

    private static JsonDocument AcceptanceRecord() => JsonDocument.Parse(File.ReadAllText(Path.Combine(
        Root(), ".llmgc", "procedural",
        "goal-155-accepted-mechanics-release-candidate-integration-and-operator-readiness",
        "goal154-human-acceptance-record.json")));

    internal static string Root()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln"))) return directory.FullName;
        throw new DirectoryNotFoundException();
    }
}
