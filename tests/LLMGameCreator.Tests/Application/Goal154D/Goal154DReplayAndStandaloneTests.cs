using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;
using RuntimeInteractiveSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Tests.Application.Goal154D;

public sealed class Goal154DReplayAndStandaloneTests
{
    [Theory]
    [InlineData(2, "EXECUTED")]
    [InlineData(4, "SKIPPED")]
    public void Behavioral_checkpoint_and_full_replay_preserve_completion_path_events_hash_and_social_facts(
        int startingHerbs,
        string expectedAdvanceStatus)
    {
        var fixture = Goal154DFixture.Create(startingHerbs);
        var service = SelectedRuntimeVariantInteractiveSessionService.CreateDefault();
        var start = new SelectedRuntimeVariantInteractiveSessionStartRequest
        {
            SessionId = "goal154d-replay-" + startingHerbs,
            CandidateId = "goal154d",
            VariantKind = "replay-" + startingHerbs,
            PackagePath = "in-memory/package.json",
            PackageSha256 = Goal154DFixture.Hash(fixture.PackageJson),
            CapabilityPlan = fixture.Plan
        };
        var uninterrupted = service.StartSession(fixture.Package, start);
        SelectedRuntimeVariantInteractiveCheckpoint? checkpoint = null;
        foreach (var action in fixture.Plan.OrderedActions)
        {
            Execute(service, fixture, uninterrupted, action.ActionId);
            if (action.ActionId == fixture.Plan.CheckpointBoundaryActionId)
                checkpoint = service.SaveCheckpoint(uninterrupted, "goal154d-checkpoint-" + startingHerbs,
                    "2026-07-14T00:00:00Z");
        }
        Assert.NotNull(checkpoint);
        var finalCheckpoint = service.SaveCheckpoint(uninterrupted, "goal154d-final-" + startingHerbs,
            "2026-07-14T00:00:00Z");
        var checkpointReplay = service.ReloadCheckpoint(fixture.Package, start, checkpoint!);
        Assert.True(checkpointReplay.Passed, string.Join("; ", checkpointReplay.Diagnostics));
        foreach (var action in fixture.Plan.OrderedActions.Skip(checkpointReplay.Session.CurrentActionIndex))
            Execute(service, fixture, checkpointReplay.Session, action.ActionId);
        var finalReplay = service.ReloadCheckpoint(fixture.Package, start, finalCheckpoint);
        Assert.True(finalReplay.Passed, string.Join("; ", finalReplay.Diagnostics));

        var originalAdvance = uninterrupted.ActionJournal.Single(item => item.ActionId == Goal154DFixture.AdvanceActionId);
        var checkpointAdvance = checkpointReplay.Session.ActionJournal.Single(item =>
            item.ActionId == Goal154DFixture.AdvanceActionId);
        var replayAdvance = finalReplay.Session.ActionJournal.Single(item =>
            item.ActionId == Goal154DFixture.AdvanceActionId);

        Assert.Equal(expectedAdvanceStatus, originalAdvance.Status);
        Assert.Equal(originalAdvance.Status, checkpointAdvance.Status);
        Assert.Equal(originalAdvance.Status, replayAdvance.Status);
        Assert.Equal(uninterrupted.CurrentStateHash, checkpointReplay.Session.CurrentStateHash);
        Assert.Equal(uninterrupted.CurrentStateHash, finalReplay.Session.CurrentStateHash);
        Assert.Equal(SocialEventSignature(uninterrupted), SocialEventSignature(checkpointReplay.Session));
        Assert.Equal(SocialEventSignature(uninterrupted), SocialEventSignature(finalReplay.Session));
        Assert.Equal(Project(fixture, uninterrupted).HumanFacts, Project(fixture, checkpointReplay.Session).HumanFacts);
        Assert.Equal(Project(fixture, uninterrupted).HumanFacts, Project(fixture, finalReplay.Session).HumanFacts);
        Assert.Equal(uninterrupted.ActionJournal.Select(item => item.Status),
            finalReplay.Session.ActionJournal.Select(item => item.Status));
    }

    [Fact]
    public void Behavioral_exact_owner_cached_hidden_smoke_reuses_host_without_unity_build()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL154D_RUN_SMOKE"), "true",
                StringComparison.OrdinalIgnoreCase))
            return;

        using var project = Goal154DAllSelectedRealProjectTests.PrepareExactOwnerCopy();
        var controller = Open(project.Path);
        var build = controller.BuildAndQualify();
        Assert.True(build.Passed, string.Join("; ", build.Diagnostics));
        var beforeUnity = System.Diagnostics.Process.GetProcessesByName("Unity").Length;

        var standalone = controller.BuildWindowsStandalone();

        Assert.Equal("GREEN", standalone.Status);
        Assert.True(standalone.HostReused);
        Assert.False(standalone.HostRebuilt);
        Assert.True(standalone.LaunchSmokePassed);
        Assert.Equal(5, standalone.SelfCheckTotalCount);
        Assert.Equal(5, standalone.SelfCheckPassedCount);
        Assert.Equal(0, beforeUnity);
        Assert.Empty(System.Diagnostics.Process.GetProcessesByName("Unity"));
        var payloadPath = Path.Combine(standalone.OutputFolder,
            Path.GetFileNameWithoutExtension(standalone.ExecutablePath) + "_Data", "StreamingAssets",
            "LLMGameCreatorProject", "player-adapter-model.json");
        using var payload = JsonDocument.Parse(File.ReadAllText(payloadPath));
        var facts = payload.RootElement.GetProperty("humanReviewFacts").EnumerateArray()
            .ToDictionary(item => item.GetProperty("label").GetString() ?? string.Empty,
                item => item.GetProperty("value").GetString() ?? string.Empty, StringComparer.Ordinal);
        Assert.Equal("0 → 10", facts["Репутация"]);
        Assert.Equal("0 → 10 → 17", facts["Золото"]);
        Assert.Equal("награда получена", facts["Социальный итог"]);
    }

    private static IReadOnlyList<string> SocialEventSignature(RuntimeInteractiveSession session) =>
        session.CanonicalSession.Snapshots.SelectMany(snapshot => snapshot.RuntimeEvents)
            .Where(item => item.EventType is "QuestCompleted" or "QuestRewardGranted"
                or "FactionReputationChanged" or "ResourceChanged" or "DialogueChoiceSelected")
            .Select(item => item.EventType + "|" + item.TargetId + "|"
                            + string.Join(",", item.Args.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                                .Select(pair => pair.Key + "=" + pair.Value)))
            .ToList();

    private static void Execute(
        ISelectedRuntimeVariantInteractiveSessionService service,
        Goal154DFixture fixture,
        RuntimeInteractiveSession session,
        string actionId)
    {
        var result = service.ExecuteAction(fixture.Package, session,
            new SelectedRuntimeVariantInteractiveActionRequest
            {
                ActionRequestId = session.SessionId + "-" + session.CurrentActionIndex.ToString("000"),
                SessionId = session.SessionId,
                ActionIndex = session.CurrentActionIndex,
                ActionId = actionId
            });
        Assert.True(result.Status is "EXECUTED" or "SKIPPED", string.Join("; ", result.Diagnostics));
    }

    private static GameProjectSocialSummary Project(Goal154DFixture fixture, RuntimeInteractiveSession session)
    {
        var observations = new FeatureModuleRuntimeEffectEvaluator().Evaluate(
            fixture.SocialModules, session, new RuntimeInteractiveSession(), fixture.Package);
        return new SocialRuntimeReviewProjectionService().Project(
            fixture.SocialModules, fixture.Package, fixture.Plan, session, observations, true, true);
    }

    private static UnifiedGameProjectWorkspaceController Open(string project)
    {
        var root = Goal154DFixture.FindRoot();
        var repository = new JsonGamePackageRepository();
        var current = new CurrentGamePackageService(repository);
        current.LoadAsync(project, CancellationToken.None).GetAwaiter().GetResult();
        var controller = new UnifiedGameProjectWorkspaceController(current,
            new GameProjectFeatureModuleAuthoringService(root),
            new GameProjectBuildAndQualificationService(root,
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault(), repository,
                new GamePackageValidator(), current),
            standaloneBuild: new ProjectStandaloneBuildService(root));
        controller.OpenProject(project);
        return controller;
    }
}
