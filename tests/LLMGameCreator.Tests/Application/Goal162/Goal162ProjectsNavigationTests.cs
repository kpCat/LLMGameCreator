using System.Reflection;
using System.Windows.Forms;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal157;
using LLMGameCreator.Tests.Application.Goal160;
using LLMGameCreator.Tests.Application.Goal161;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal162;

[Collection(Goal160Collection.Name)]
public sealed class Goal162ProjectsNavigationTests
{
    [Fact]
    public void Behavioral_source_ready_generated_project_offers_build_and_play()
    {
        var presentation = Goal162ProjectsTestKit.Presentation(Goal162ProjectsState.Value.SourceReady);

        Assert.True(presentation.Enabled);
        Assert.Equal("Собрать и играть", presentation.Title);
        Assert.False(presentation.Current);
    }

    [Fact]
    public void Behavioral_travel_current_project_requires_campaign_qualification_before_play()
    {
        var presentation = Goal162ProjectsTestKit.Presentation(Goal157BuildState.Value.Reopen);

        Assert.True(presentation.Enabled);
        Assert.Equal("Собрать и играть", presentation.Title);
        Assert.False(presentation.Current);
    }

    [Fact]
    public void Behavioral_legacy_project_disables_play_with_human_reason()
    {
        var presentation = Goal162ProjectsTestKit.Presentation(new UnifiedGameProjectWorkspaceSnapshot
        {
            ProjectTitle = "Обычный проект"
        });

        Assert.False(presentation.Enabled);
        Assert.Equal("Играть", presentation.Title);
        Assert.Contains("сгенерированном проекте", presentation.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Behavioral_busy_generated_project_disables_play_without_changing_intent()
    {
        var busy = Goal157BuildState.Value.Reopen with { ProjectOperationBusy = true };
        var presentation = Goal162ProjectsTestKit.Presentation(busy);

        Assert.False(presentation.Enabled);
        Assert.Equal("Собрать и играть", presentation.Title);
        Assert.Contains("Дождитесь", presentation.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Behavioral_build_and_play_runs_one_build_and_requests_campaign_navigation()
    {
        var state = Goal162ProjectsState.Value;

        Assert.Equal(1, state.BuildHistoryAfter - state.BuildHistoryBefore);
        Assert.Equal("generated-campaign-player", state.NavigationPageId);
    }

    [Fact]
    public void Behavioral_build_and_play_requires_green_campaign_current_result()
    {
        var snapshot = Goal162ProjectsState.Value.AfterBuild;

        Assert.Equal("CAMPAIGN_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.True(snapshot.GeneratedWorldActivation?.Passed);
        Assert.True(snapshot.GeneratedRegionTravel?.Passed);
        Assert.True(snapshot.AcceptedMechanicsCompatibility?.Passed);
    }

    [Fact]
    public void Behavioral_projects_navigation_does_not_auto_start_campaign_session()
    {
        var state = Goal162ProjectsState.Value;

        Assert.Equal(0, state.CampaignService.RuntimeStartInvocationCount);
        Assert.Equal(GeneratedCampaignSessionStatus.READY, state.CampaignService.Refresh().Status);
    }

    [Fact]
    public void Behavioral_failed_build_stays_on_projects_and_shows_causal_diagnostic()
    {
        var failure = Goal162ProjectsState.Value.Failure;

        Assert.Null(failure.NavigationPageId);
        Assert.Contains("generated", failure.DiagnosticText, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual("CAMPAIGN_CURRENT", failure.Snapshot.GeneratedWorld?.Status);
    }
}

internal static class Goal162ProjectsState
{
    private static readonly Lazy<Goal162ProjectsFixture> Fixture = new(Create);
    public static Goal162ProjectsFixture Value => Fixture.Value;

    private static Goal162ProjectsFixture Create()
    {
        var project = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "goal162-projects-build-play");
        var bundle = Goal161WorldBundle.Create(project.Path);
        var sourceReady = bundle.Controller.Snapshot();
        Assert.Equal("SOURCE_READY", sourceReady.GeneratedWorld?.Status);
        var campaign = Goal162TestKit.Service(bundle);
        var before = HistoryCount(project.Path);
        string? navigation = null;
        Goal157TestKit.RunSta(() =>
        {
            var navigationService = new EditorPageNavigationService();
            navigationService.NavigationRequested += (_, pageId) => navigation = pageId;
            using var page = new ProjectsPageControl(
                bundle.Current,
                null!,
                null!,
                null!,
                bundle.Controller,
                navigationService);
            Goal162ProjectsTestKit.AwaitWithMessagePump(Goal162ProjectsTestKit.InvokePlay(page));
        });
        var after = HistoryCount(project.Path);
        var afterBuild = bundle.Controller.Snapshot();

        var failedProject = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "goal162-projects-build-failure");
        var failedBundle = Goal161WorldBundle.Create(failedProject.Path);
        var sidecar = Path.Combine(failedProject.Path, ".llmgc", "generation",
            SeededGeneratedProjectVocabulary.GeneratedBasePackageJsonFileName);
        File.AppendAllText(sidecar, " ");
        string? failedNavigation = null;
        string diagnostic = string.Empty;
        Goal157TestKit.RunSta(() =>
        {
            var navigationService = new EditorPageNavigationService();
            navigationService.NavigationRequested += (_, pageId) => failedNavigation = pageId;
            using var page = new ProjectsPageControl(
                failedBundle.Current,
                null!,
                null!,
                null!,
                failedBundle.Controller,
                navigationService);
            Goal162ProjectsTestKit.AwaitWithMessagePump(Goal162ProjectsTestKit.InvokePlay(page));
            diagnostic = Goal162ProjectsTestKit.Field<TextBox>(page, "_buildResultTextBox").Text;
        });
        var failure = new Goal162ProjectsFailure(failedProject, failedBundle, failedNavigation,
            diagnostic, failedBundle.Controller.Snapshot());
        return new Goal162ProjectsFixture(project, bundle, sourceReady, before, after, afterBuild,
            navigation, campaign, failure);
    }

    private static int HistoryCount(string project)
    {
        var root = Path.Combine(project, UnifiedGameProjectWorkspaceVocabulary.BuildHistoryRelativeRoot
            .Replace('/', Path.DirectorySeparatorChar));
        return Directory.Exists(root) ? Directory.EnumerateFiles(root, "*.json").Count() : 0;
    }
}

internal sealed record Goal162ProjectsFixture(
    GeneratedProject Project,
    Goal161WorldBundle Bundle,
    UnifiedGameProjectWorkspaceSnapshot SourceReady,
    int BuildHistoryBefore,
    int BuildHistoryAfter,
    UnifiedGameProjectWorkspaceSnapshot AfterBuild,
    string? NavigationPageId,
    GeneratedCampaignSessionService CampaignService,
    Goal162ProjectsFailure Failure);

internal sealed record Goal162ProjectsFailure(
    GeneratedProject Project,
    Goal161WorldBundle Bundle,
    string? NavigationPageId,
    string DiagnosticText,
    UnifiedGameProjectWorkspaceSnapshot Snapshot);

internal sealed record Goal162PlayPresentation(bool Enabled, string Title, string Reason, bool Current);

internal static class Goal162ProjectsTestKit
{
    public static Goal162PlayPresentation Presentation(
        UnifiedGameProjectWorkspaceSnapshot snapshot,
        bool busy = false)
    {
        var method = typeof(ProjectsPageControl).GetMethod("GeneratedCampaignPlay",
            BindingFlags.Static | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("Generated campaign play projection was not found.");
        var result = method.Invoke(null, new object[] { snapshot, busy })
                     ?? throw new InvalidOperationException("Generated campaign play projection returned null.");
        var type = result.GetType();
        return new Goal162PlayPresentation(
            (bool)type.GetProperty("Enabled")!.GetValue(result)!,
            (string)type.GetProperty("Title")!.GetValue(result)!,
            (string)type.GetProperty("Reason")!.GetValue(result)!,
            (bool)type.GetProperty("Current")!.GetValue(result)!);
    }

    public static Task InvokePlay(ProjectsPageControl page)
    {
        var method = typeof(ProjectsPageControl).GetMethod("PlayGeneratedCampaignAsync",
            BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("Projects play route was not found.");
        return (Task)(method.Invoke(page, null)
                      ?? throw new InvalidOperationException("Projects play route returned null."));
    }

    public static void AwaitWithMessagePump(Task task)
    {
        var deadline = DateTime.UtcNow.AddMinutes(3);
        while (!task.IsCompleted && DateTime.UtcNow < deadline)
        {
            System.Windows.Forms.Application.DoEvents();
            Thread.Yield();
        }

        Assert.True(task.IsCompleted, "Projects play route did not complete within three minutes.");
        task.GetAwaiter().GetResult();
    }

    public static T Field<T>(object owner, string name) where T : class =>
        (T)(owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(owner)
            ?? throw new InvalidOperationException("Field was not found: " + name));
}
