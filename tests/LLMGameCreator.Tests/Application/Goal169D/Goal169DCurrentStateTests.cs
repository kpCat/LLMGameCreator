using System.Text;
using System.Text.Json;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169D;

public sealed class Goal169DCurrentStateTests
{
    [Fact]
    public void Behavioral_current_state_records_goal169d_green_candidate()
    {
        using var state = ReadState();
        var root = state.RootElement;

        Assert.Equal(
            "GREEN",
            root.GetProperty("goal169dImplementationStatus")
                .GetString());
        Assert.Equal(
            "GREEN_ACCEPTABLE_CANDIDATE",
            root.GetProperty("goal169dCandidateStatus")
                .GetString());
        Assert.False(root.GetProperty("goal169dAccepted")
            .GetBoolean());
        Assert.True(root.GetProperty(
                "goal169dIndependentAuditRequired")
            .GetBoolean());
    }

    [Fact]
    public void Behavioral_current_state_records_exact_core_only_closure()
    {
        using var state = ReadState();
        var root = state.RootElement;

        Assert.Equal(
            "BLOCKED_AT_72F69BE1",
            root.GetProperty("goal169cCandidateStatus")
                .GetString());
        Assert.Equal(
            "invalid_creation_only_fixture",
            root.GetProperty("goal169cCoreOnlyBlocker")
                .GetString());
        Assert.Equal(
            "closed_by_goal169d",
            root.GetProperty("goal169cCoreOnlyBlockerClosure")
                .GetString());
        Assert.True(root.GetProperty(
                "goal169dQualifiedCoreOnlyBuildPassed")
            .GetBoolean());
        Assert.True(root.GetProperty(
                "goal169dCoreOnlyCampaignCurrent")
            .GetBoolean());
        Assert.True(root.GetProperty(
                "goal169dCoreOnlyPortablePassed")
            .GetBoolean());
        Assert.True(root.GetProperty(
                "goal169dCoreOnlyNoFalseRcReady")
            .GetBoolean());
    }

    [Fact]
    public void Behavioral_current_state_markdown_json_and_next_action_agree()
    {
        using var state = ReadState();
        var root = state.RootElement;
        var markdown = File.ReadAllText(Path.Combine(
            RepositoryRoot(),
            "docs",
            "CURRENT_GENERATOR_STATE.md"),
            Encoding.UTF8);
        var nextAction = root.GetProperty("nextAction")
            .GetString();

        Assert.Equal(
            "independent_goal169d_audit_then_plan_next_visible_campaign_slice",
            nextAction);
        Assert.Contains(nextAction!, markdown,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "goal169c_blocked_after_single_cached_smoke_portable_core_only_campaign_truth",
            root.GetProperty("gate_status").GetString() ?? string.Empty,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "independent_goal169c_blocker_audit_and_followup_without_retrying_consumed_smoke",
            root.GetProperty("current_user_action").GetString()
            ?? string.Empty,
            StringComparison.Ordinal);
    }

    private static JsonDocument ReadState() =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(
            RepositoryRoot(),
            "docs",
            "CURRENT_GENERATOR_STATE.json"),
            Encoding.UTF8));

    private static string RepositoryRoot()
    {
        for (var directory =
                 new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
            if (File.Exists(Path.Combine(
                    directory.FullName,
                    "LLMGameCreator.sln")))
                return directory.FullName;

        throw new InvalidOperationException(
            "LLMGameCreator repository root was not found.");
    }
}
