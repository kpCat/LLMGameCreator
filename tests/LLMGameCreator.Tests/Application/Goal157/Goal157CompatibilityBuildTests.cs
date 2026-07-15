using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal156;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal157;

[Collection(Goal156Collection.Name)]
public sealed class Goal157CompatibilityBuildTests
{
    [Fact]
    public void Behavioral_all_selectable_accepted_mechanics_still_pass_in_lane_a()
    {
        var build = Goal157BuildState.Value.First;

        Assert.True(build.Passed, string.Join(Environment.NewLine, build.Diagnostics));
        Assert.True(build.AcceptedMechanics?.Passed, string.Join(Environment.NewLine,
            build.AcceptedMechanics?.Diagnostics ?? []));
        Assert.True(build.AcceptedMechanicsCompatibility?.Passed);
    }

    [Fact]
    public void Behavioral_accepted_mechanics_keep_expected_effective_defaults()
    {
        var accepted = Assert.IsType<GameProjectAcceptedMechanicsSummary>(Goal157BuildState.Value.First.AcceptedMechanics);

        Assert.Equal(2, accepted.EquipmentDamageBonus);
        Assert.Equal(2, accepted.StatDamageBonus);
        Assert.Equal(4, accepted.TotalAdditionalDamage);
        Assert.Equal(2, accepted.AbilityDirectDamage);
        Assert.Equal(12, accepted.ManaBefore);
        Assert.Equal(3, accepted.ManaSpent);
        Assert.Equal(9, accepted.ManaRemaining);
        Assert.Equal(1, accepted.StatusTickDamage);
        Assert.True(accepted.StatusExpired);
    }

    [Fact]
    public void Behavioral_social_facts_remain_correct_in_compatibility_lane()
    {
        var social = Assert.IsType<GameProjectSocialSummary>(Goal157BuildState.Value.First.AcceptedMechanicsCompatibility?.Social);

        Assert.True(social.Passed);
        Assert.Equal(0, social.ReputationBefore);
        Assert.Equal(10, social.ReputationAfter);
        Assert.Equal(0, social.GoldBefore);
        Assert.Equal(10, social.GoldAfterQuest);
        Assert.Equal(17, social.GoldAfterClaim);
        Assert.Equal(7, social.TrustedRewardDelta);
        Assert.Equal("claimed", social.SocialOutcome);
    }

    [Fact]
    public void Behavioral_compatibility_hashes_differ_from_player_activation_hashes()
    {
        var build = Goal157BuildState.Value.First;
        var compatibility = Assert.IsType<GameProjectAcceptedMechanicsCompatibilityResult>(build.AcceptedMechanicsCompatibility);

        Assert.NotEqual(compatibility.CompatibilityCompositionPackageSha256, build.CompositionPackageSha256);
        Assert.NotEqual(compatibility.CompatibilityActivatedPackageSha256, build.PackageSha256);
    }

    [Fact]
    public void Behavioral_primary_final_state_hash_belongs_to_complete_travel_route()
    {
        var build = Goal157BuildState.Value.First;
        var activation = Assert.IsType<GameProjectGeneratedWorldActivationSummary>(build.GeneratedWorldActivation);
        var travel = Assert.IsType<GameProjectGeneratedRegionTravelSummary>(build.GeneratedRegionTravel);

        Assert.True(activation.Passed);
        Assert.Equal(travel.FinalStateHash, build.FinalStateHash);
        Assert.NotEqual(activation.FinalStateHash, build.FinalStateHash);
        Assert.NotEqual(build.AcceptedMechanicsCompatibility?.CompatibilityFinalStateHash, build.FinalStateHash);
    }

    [Fact]
    public void Behavioral_accepted_summary_carries_lane_a_qualification_hashes_and_flags()
    {
        var build = Goal157BuildState.Value.First;
        var accepted = Assert.IsType<GameProjectAcceptedMechanicsSummary>(build.AcceptedMechanics);
        var compatibility = Assert.IsType<GameProjectAcceptedMechanicsCompatibilityResult>(build.AcceptedMechanicsCompatibility);

        Assert.Equal(compatibility.CompatibilityActivatedPackageSha256, accepted.QualificationPackageSha256);
        Assert.Equal(compatibility.CompatibilityFinalStateHash, accepted.QualificationFinalStateHash);
        Assert.Equal(compatibility.CheckpointReloadPassed, accepted.QualificationCheckpointReloadPassed);
        Assert.Equal(compatibility.FullReplayEquivalent, accepted.QualificationFullReplayEquivalent);
        Assert.Equal(compatibility.ActionBindingPassed, accepted.QualificationActionBindingPassed);
    }

    [Fact]
    public void Behavioral_core_only_build_is_green_but_accepted_mechanics_is_false()
    {
        var build = Goal157BuildState.Value.Core;

        Assert.True(build.Passed, string.Join(Environment.NewLine, build.Diagnostics));
        Assert.Equal("GREEN", build.Status);
        Assert.False(build.AcceptedMechanics?.Passed);
        Assert.NotEmpty(build.AcceptedMechanics?.MissingFactKinds ?? []);
        Assert.True(build.GeneratedWorldActivation?.Passed);
    }

    [Fact]
    public void Behavioral_legacy_project_remains_exact_single_lane_hash_behavior()
    {
        var build = Goal157BuildState.Value.Legacy;
        var compatibility = Assert.IsType<GameProjectAcceptedMechanicsCompatibilityResult>(build.AcceptedMechanicsCompatibility);

        Assert.True(build.Passed, string.Join(Environment.NewLine, build.Diagnostics));
        Assert.Null(build.GeneratedWorld);
        Assert.Null(build.GeneratedWorldActivation);
        Assert.Equal(compatibility.CompatibilityCompositionPackageSha256, build.CompositionPackageSha256);
        Assert.Equal(compatibility.CompatibilityActivatedPackageSha256, build.PackageSha256);
        Assert.Equal(compatibility.CompatibilityFinalStateHash, build.FinalStateHash);
    }

    [Fact]
    public void Contract_generated_primary_runtime_contract_is_complete_travel_route()
    {
        var build = Goal157BuildState.Value.First;
        var travel = Assert.IsType<GameProjectGeneratedRegionTravelSummary>(build.GeneratedRegionTravel);

        Assert.Equal("generated-region-travel-v1", build.RuntimePlaythroughPlanId);
        Assert.Contains("OriginInteraction", build.PlaythroughSignature, StringComparison.Ordinal);
        Assert.Contains("GateInteraction", build.PlaythroughSignature, StringComparison.Ordinal);
        Assert.Contains("DestinationInteraction", build.PlaythroughSignature, StringComparison.Ordinal);
        Assert.Equal(travel.ConnectionIds.Count + 2, build.CapabilityCount);
        Assert.Equal(build.PlannedActionCount + 1, build.RuntimeFrames.Count);
        Assert.Equal(travel.RuntimeFrames.Count, build.RuntimeFrames.Count);
        Assert.NotEqual(build.RuntimeFrames.Count, build.AcceptedMechanicsCompatibility?.RuntimeFrames.Count);
    }
}

internal static class Goal157BuildState
{
    private static readonly Lazy<Goal157BuildFixture> Fixture = new(Goal157BuildFixture.Create);
    public static Goal157BuildFixture Value => Fixture.Value;
}

internal sealed record Goal157BuildFixture(
    GeneratedProject Project,
    GameProjectBuildResult First,
    GameProjectBuildResult Repeat,
    UnifiedGameProjectWorkspaceSnapshot Reopen,
    GeneratedProject CoreProject,
    GameProjectBuildResult Core,
    GameProjectBuildResult Legacy)
{
    public static Goal157BuildFixture Create()
    {
        var project = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "goal157-build-fixture");
        var first = Goal157TestKit.OpenTravelWorkspace(project.Path).BuildAndQualify();
        var repeat = Goal157TestKit.OpenTravelWorkspace(project.Path).BuildAndQualify();
        var reopen = Goal157TestKit.OpenTravelWorkspace(project.Path).Snapshot();
        var core = Goal156TestKit.Copy(Goal156TestKit.CoreOnly, "goal157-core-fixture");
        var coreBuild = Goal157TestKit.OpenTravelWorkspace(core.Path).BuildAndQualify();
        using var scope = Goal156TestKit.Scope("goal157-legacy");
        var legacySummary = scope.Service.CreateAsync(Goal156TestKit.TemplateRequest(scope.Root, "legacy"),
            CancellationToken.None).GetAwaiter().GetResult();
        var legacy = Goal157TestKit.OpenTravelWorkspace(legacySummary.FolderPath).BuildAndQualify();
        return new Goal157BuildFixture(project, first, repeat, reopen, core, coreBuild, legacy);
    }
}

internal static partial class Goal157TestKit
{
    public static UnifiedGameProjectWorkspaceController OpenTravelWorkspace(
        string project,
        IProjectStandaloneBuildService? standalone = null,
        IGameRuntime? runtime = null,
        IRuntimeStateSerializer? stateSerializer = null)
    {
        var current = new CurrentGamePackageService(Goal156TestKit.Repository);
        current.LoadAsync(project, CancellationToken.None).GetAwaiter().GetResult();
        var source = Goal156TestKit.SourceService;
        var summary = new GameProjectGeneratedWorldSummaryService();
        var selectedRuntime = runtime ?? new DefaultGameRuntime();
        var serializer = stateSerializer ?? new RuntimeStateSerializer();
        var controller = new UnifiedGameProjectWorkspaceController(
            current,
            new GameProjectFeatureModuleAuthoringService(Goal156TestKit.RepositoryRoot),
            new GameProjectBuildAndQualificationService(
                Goal156TestKit.RepositoryRoot,
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault(),
                Goal156TestKit.Repository,
                Goal156TestKit.Validator,
                current,
                generatedSource: source,
                generatedSummary: summary,
                generatedActivation: new GameProjectGeneratedWorldActivationService(
                    selectedRuntime,
                    serializer,
                    Goal156TestKit.Validator),
                generatedTravelOverlay: new GeneratedWorldTravelOverlayService(),
                generatedTravelActivation: new GameProjectGeneratedRegionTravelActivationService(
                    selectedRuntime,
                    serializer)),
            standaloneBuild: standalone,
            generatedSourceService: source,
            generatedWorldSummaryService: summary);
        controller.OpenProject(project);
        return controller;
    }
}
