using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Tests.Application.Goal164;
using LLMGameCreator.Tests.Application.Goal156;
using System.Text.Json;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal167;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal167StandalonePortabilityTests
{
    [Fact]
    public void Behavioral_standalone_payload_uses_exact_v7_regional_event_primary_hashes()
    {
        var state = Goal164PortableState.AllSelectable;

        var history = JsonSerializer.Deserialize<GameProjectBuildHistoryEntry>(
            File.ReadAllText(state.Build.Build.BuildHistoryPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.Equal(GameProjectBuildHistoryReader.SchemaVersionV7, history?.SchemaVersion);
        Assert.Equal(state.Build.Build.PackageSha256, state.Service.Request?.PackageSha256);
        Assert.Equal(state.Build.Build.CompositionPackageSha256,
            state.Service.Request?.CompositionPackageSha256);
        Assert.Equal(state.Build.Build.GeneratedCampaignRegionalEvents?.FinalStateHash,
            state.Service.Request?.FinalStateHash);
    }

    [Fact]
    public void Behavioral_standalone_payload_contains_regional_event_frames_and_earlier_human_facts()
    {
        var request = Goal164PortableState.AllSelectable.Service.Request!;

        Assert.NotEmpty(request.RuntimeFrames);
        Assert.All(request.RuntimeFrames,
            frame => Assert.Equal("generated-regional-event",
                frame.Category));
        Assert.Contains(request.HumanReviewFacts, item => item.Label == "Сюжетные решения");
        Assert.Contains(request.HumanReviewFacts, item => item.Label == "Взаимоисключающие ветви"
            && item.Value == "подтверждены Runtime");
        Assert.Contains(request.HumanReviewFacts, item => item.Label == "Постоянные флаги решений");
        Assert.Contains(request.HumanReviewFacts,
            item => item.Label == "Отношения");
        Assert.Contains(request.HumanReviewFacts,
            item => item.Label == "События мира");
    }

    [Fact]
    public void Behavioral_all_selectable_portable_profile_is_choice_current_and_rc_current()
    {
        var state = Goal164PortableState.AllSelectable;

        Assert.Equal("GREEN", state.Standalone.Status);
        Assert.Equal("CHOICE_CURRENT", state.Snapshot.GeneratedCampaignChoices?.Status);
        Assert.Equal("CAMPAIGN_CURRENT", state.Snapshot.GeneratedWorld?.Status);
        Assert.Equal("CURRENT", state.Snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Equal("CURRENT", state.Snapshot.ReleaseCandidateRecordConfigurationStatus);
    }

    [Fact]
    public void Behavioral_core_only_portable_profile_is_choice_current_without_false_rc_ready()
    {
        var state = Goal164PortableState.CoreOnly;

        Assert.Equal("GREEN", state.Standalone.Status);
        Assert.Equal("CHOICE_CURRENT", state.Snapshot.GeneratedCampaignChoices?.Status);
        Assert.Equal("CAMPAIGN_CURRENT", state.Snapshot.GeneratedWorld?.Status);
        Assert.NotEqual("CURRENT", state.Snapshot.ReleaseCandidateConfigurationStatus);
        Assert.NotEqual("CURRENT", state.Snapshot.ReleaseCandidateRecordConfigurationStatus);
        Assert.True(state.Snapshot.SelectedMechanicCount
                    < Goal164PortableState.AllSelectable.Snapshot.SelectedMechanicCount);
    }

    [Fact]
    public void Behavioral_core_only_branch_save_continues_without_runtime_restart()
    {
        var route = Goal164CampaignState.CoreOnly;

        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, route.Continued.Status);
        Assert.Equal(route.Saved.SessionSha256, route.Continued.SessionSha256);
        Assert.Equal("CHOICE_CURRENT", route.Build.Snapshot.GeneratedCampaignChoices?.Status);
    }

    [Fact]
    public void Behavioral_physical_all_selectable_copy_restores_v5_choice_and_rc_current()
    {
        var source = Goal164PortableState.AllSelectable.Build;
        using var portable = Goal156TestKit.Copy(source.Project, "goal167-portable-all");
        RemoveOperationalOutput(portable.Path);
        var snapshot = Goal156TestKit.OpenWorkspace(portable.Path).Snapshot();

        Assert.False(Directory.Exists(Path.Combine(portable.Path, "Builds")));
        Assert.Equal("CHOICE_CURRENT", snapshot.GeneratedCampaignChoices?.Status);
        Assert.Equal("CAMPAIGN_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.Equal("CURRENT", snapshot.ReleaseCandidateConfigurationStatus);
    }

    [Fact]
    public void Behavioral_physical_core_only_copy_has_choice_current_without_false_rc()
    {
        var source = Goal164PortableState.CoreOnly.Build;
        using var portable = Goal156TestKit.Copy(source.Project, "goal167-portable-core");
        RemoveOperationalOutput(portable.Path);
        var snapshot = Goal156TestKit.OpenWorkspace(portable.Path).Snapshot();

        Assert.False(Directory.Exists(Path.Combine(portable.Path, "Builds")));
        Assert.Equal("CHOICE_CURRENT", snapshot.GeneratedCampaignChoices?.Status);
        Assert.Equal("CAMPAIGN_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.NotEqual("CURRENT", snapshot.ReleaseCandidateConfigurationStatus);
        Assert.NotEqual("CURRENT", snapshot.ReleaseCandidateRecordConfigurationStatus);
    }

    private static void RemoveOperationalOutput(string project)
    {
        var builds = Path.GetFullPath(Path.Combine(project, "Builds"));
        if (Directory.Exists(builds)) Directory.Delete(builds, recursive: true);
    }
}
