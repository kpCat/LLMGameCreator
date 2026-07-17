using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal162;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal165;

public sealed class Goal165RegressionImmutabilityTests
{
    [Fact]
    public void Behavioral_goal164_generated_campaign_regression_is_green()
    {
        var fixture = Goal164TestKit.AllSelectable;

        Assert.True(fixture.Build.Passed, string.Join(Environment.NewLine, fixture.Build.Diagnostics));
        Assert.Equal("CAMPAIGN_CURRENT", fixture.Build.GeneratedEncounterCombat?.Status);
    }

    [Fact]
    public void Behavioral_goal163_goal162_save_and_campaign_paths_remain_available()
    {
        var route = Goal164CampaignState.AllSelectable;

        Assert.Equal("CAMPAIGN_CURRENT", route.Build.Snapshot.GeneratedWorld?.Status);
        Assert.NotEmpty(route.AfterTurnIn.Consequences);
        Assert.Equal(route.Saved.CurrentMapTitle, route.Continued.CurrentMapTitle);
    }

    [Fact]
    public void Behavioral_physical_core_only_portable_copy_has_no_operational_pointer_or_false_rc_readiness()
    {
        var coreOnly = Goal164PortableState.CoreOnly.Build;
        using var portable = Goal156TestKit.Copy(coreOnly.Project, "goal165-core-only-portable-copy");
        var snapshot = Goal156TestKit.OpenWorkspace(portable.Path).Snapshot();
        var location = new ProjectStandaloneOutputLocationService().Resolve(
            portable.Path, portable.Request.PackageId, "goal165core");

        Assert.Equal("CAMPAIGN_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.Equal("CAMPAIGN_CURRENT", snapshot.GeneratedEncounterCombat?.Status);
        Assert.NotNull(snapshot.AcceptedMechanics);
        Assert.NotEqual("CURRENT", snapshot.ReleaseCandidateConfigurationStatus);
        Assert.NotEqual("CURRENT", snapshot.ReleaseCandidateRecordConfigurationStatus);
        Assert.False(File.Exists(location.CurrentPointerPath));
    }
}
