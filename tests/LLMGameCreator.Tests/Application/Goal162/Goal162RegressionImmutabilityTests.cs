using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal157;
using LLMGameCreator.Tests.Application.Goal159;
using LLMGameCreator.Tests.Application.Goal160;
using LLMGameCreator.Tests.Application.Goal161;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal162;

[Collection(Goal160Collection.Name)]
public sealed class Goal162RegressionImmutabilityTests
{
    [Fact]
    public void Regression_runtime_simulator_source_files_remain_byte_identical_to_baseline()
    {
        var root = Goal156TestKit.RepositoryRoot;

        Assert.Equal("ebbff1bd7cc5ba508edc24c0080bd780916f7fbc46c2c14081afca6dd478f13e",
            Goal156TestKit.Hash(Path.Combine(root, "src", "LLMGameCreator.WinForms", "Pages",
                "RuntimeSimulator", "RuntimeSimulatorPageControl.cs")));
        Assert.Equal("0a9faecd26068f951f6d227b0d320128fc32cb0e12d3790f4a99cc6de12d9ce0",
            Goal156TestKit.Hash(Path.Combine(root, "src", "LLMGameCreator.WinForms", "Pages",
                "RuntimeSimulator", "RuntimeSimulatorPageControl.Designer.cs")));
    }

    [Fact]
    public void Regression_campaign_play_and_save_leave_all_non_save_project_bytes_unchanged()
    {
        var state = Goal162ImmutabilityState.Value;

        Assert.Equal(state.NonSaveBefore, state.NonSaveAfter);
        Assert.Equal(state.PackageBefore, state.PackageAfter);
    }

    [Fact]
    public void Regression_build_history_and_generated_source_bytes_are_unchanged()
    {
        var state = Goal162ImmutabilityState.Value;
        var protectedBefore = state.NonSaveBefore.Where(item =>
            item.Key.Contains("build", StringComparison.OrdinalIgnoreCase)
            || item.Key.Contains("generation", StringComparison.OrdinalIgnoreCase)).ToList();
        var protectedAfter = state.NonSaveAfter.Where(item =>
            item.Key.Contains("build", StringComparison.OrdinalIgnoreCase)
            || item.Key.Contains("generation", StringComparison.OrdinalIgnoreCase)).ToList();

        Assert.NotEmpty(protectedBefore);
        Assert.Equal(protectedBefore, protectedAfter);
    }

    [Fact]
    public void Regression_only_generated_gameplay_save_files_may_change()
    {
        var state = Goal162ImmutabilityState.Value;
        var changed = state.FullBefore.Keys.Union(state.FullAfter.Keys, StringComparer.Ordinal)
            .Where(key => !state.FullBefore.TryGetValue(key, out var before)
                          || !state.FullAfter.TryGetValue(key, out var after)
                          || before != after).ToList();

        Assert.NotEmpty(changed);
        Assert.All(changed, key => Assert.StartsWith(".llmgc/gameplay-saves/", key,
            StringComparison.Ordinal));
    }

    [Fact]
    public void Regression_goal142_baseline_source_is_byte_identical()
    {
        var state = Goal162ImmutabilityState.Value;

        Assert.Equal(state.Goal142Before, state.Goal142After);
        Assert.False(string.IsNullOrWhiteSpace(state.Goal142After));
    }

    [Fact]
    public void Regression_goal148_manual_source_tree_is_byte_identical()
    {
        var state = Goal162ImmutabilityState.Value;

        Assert.NotEmpty(state.Goal148Before);
        Assert.Equal(state.Goal148Before, state.Goal148After);
    }

    [Fact]
    public void Regression_legacy_project_is_unavailable_to_campaign_without_runtime_start()
    {
        var state = Goal162ImmutabilityState.Value;

        Assert.Equal(GeneratedCampaignSessionStatus.PROJECT_NOT_GENERATED, state.Legacy.Status);
        Assert.Equal(0, state.LegacyService.RuntimeStartInvocationCount);
        Assert.Null(state.Legacy.Map);
        Assert.Empty(state.Legacy.Actions);
    }

    [Fact]
    public void Regression_campaign_slice_does_not_create_standalone_or_release_outputs()
    {
        var state = Goal162ImmutabilityState.Value;
        var outputKeys = state.NonSaveAfter.Keys.Where(key =>
            key.Contains("standalone", StringComparison.OrdinalIgnoreCase)
            || key.Contains("release-candidate", StringComparison.OrdinalIgnoreCase)
            || key.EndsWith("current.json", StringComparison.OrdinalIgnoreCase)).ToList();

        Assert.Equal(state.NonSaveBefore.Keys.Where(key => outputKeys.Contains(key, StringComparer.Ordinal)),
            outputKeys);
        Assert.Equal(1, state.CampaignService.RuntimeStartInvocationCount);
    }
}

[Collection(Goal160Collection.Name)]
public sealed class GeneratedGameplaySaveGoal162RegressionTests
{
    [Fact]
    public void Regression_generated_gameplay_save_route_remains_current_and_resumable()
    {
        var state = Goal162SaveMigrationState.Value;

        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, state.Continued.Status);
        Assert.Contains(state.SaveList.Entries, entry =>
            entry.SlotName == state.SlotName && entry.Status == GeneratedGameplaySaveStatus.CURRENT);
        Assert.Equal(state.BeforeSave.SessionSha256, state.Continued.SessionSha256);
    }
}

[Collection(Goal160Collection.Name)]
public sealed class GameProjectOperationCoordinatorGoal162RegressionTests
{
    [Fact]
    public void Regression_game_project_operation_coordinator_keeps_campaign_build_exclusive()
    {
        using var project = Goal156TestKit.Scope("goal162-operation-coordinator");
        var coordinator = new GameProjectOperationCoordinator();
        using var build = coordinator.TryAcquire(project.Root, GameProjectOperationKinds.Build);
        using var rejected = coordinator.TryAcquire(project.Root, GameProjectOperationKinds.RegenerationApply);

        Assert.True(build.Acquired);
        Assert.False(rejected.Acquired);
        Assert.Equal("project_operation.busy:build", rejected.Diagnostic);
    }
}

internal static class Goal162ImmutabilityState
{
    private static readonly Lazy<Goal162ImmutabilityFixture> Fixture = new(Create);
    public static Goal162ImmutabilityFixture Value => Fixture.Value;

    private static Goal162ImmutabilityFixture Create()
    {
        var goal142 = Goal156TestKit.Goal142BaselinePath;
        var goal148 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LLMGameCreator", "Games", "goal148-manual");
        Assert.True(File.Exists(goal142));
        Assert.True(Directory.Exists(goal148));
        var goal142Before = Goal156TestKit.Hash(goal142);
        var goal148Before = Goal159TestKit.TreeHashes(goal148);

        var project = Goal156TestKit.Copy(Goal157BuildState.Value.Project, "goal162-immutability");
        var bundle = Goal161WorldBundle.Create(project.Path);
        var fullBefore = Goal159TestKit.TreeHashes(project.Path);
        var nonSaveBefore = ExcludingSaves(fullBefore);
        var packageBefore = Goal156TestKit.Hash(Path.Combine(project.Path, "package.json"));
        var service = Goal162TestKit.Service(bundle);
        var started = service.StartNew();
        var move = started.Actions.First(action => action.Enabled
            && action.Kind is GeneratedCampaignActionKind.MoveUp
                or GeneratedCampaignActionKind.MoveDown
                or GeneratedCampaignActionKind.MoveLeft
                or GeneratedCampaignActionKind.MoveRight);
        service.Execute(move.ActionId);
        service.Save("immutability-check");
        var fullAfter = Goal159TestKit.TreeHashes(project.Path);
        var nonSaveAfter = ExcludingSaves(fullAfter);
        var packageAfter = Goal156TestKit.Hash(Path.Combine(project.Path, "package.json"));

        var current = new CurrentGamePackageService(new JsonGamePackageRepository());
        current.LoadAsync(goal148, CancellationToken.None).GetAwaiter().GetResult();
        var legacyServices = Goal161ServiceBundle.Create();
        var legacyService = new GeneratedCampaignSessionService(
            current,
            new GeneratedCampaignSessionTruthService(current, legacyServices.Validator,
                legacyServices.Coordinator),
            legacyServices.Runtime,
            legacyServices.Save,
            legacyServices.Migration,
            new GeneratedCampaignActionPlanner(),
            new GeneratedCampaignProjectionService(),
            new GeneratedCampaignEventPresenter());
        var legacy = legacyService.Refresh();
        return new Goal162ImmutabilityFixture(project, bundle, service, fullBefore, fullAfter,
            nonSaveBefore, nonSaveAfter, packageBefore, packageAfter, goal142Before,
            Goal156TestKit.Hash(goal142), goal148Before, Goal159TestKit.TreeHashes(goal148),
            legacyService, legacy);
    }

    private static SortedDictionary<string, string> ExcludingSaves(
        SortedDictionary<string, string> source) => new(source.Where(item =>
                !item.Key.StartsWith(".llmgc/gameplay-saves/", StringComparison.Ordinal))
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal),
            StringComparer.Ordinal);
}

internal sealed record Goal162ImmutabilityFixture(
    GeneratedProject Project,
    Goal161WorldBundle Bundle,
    GeneratedCampaignSessionService CampaignService,
    SortedDictionary<string, string> FullBefore,
    SortedDictionary<string, string> FullAfter,
    SortedDictionary<string, string> NonSaveBefore,
    SortedDictionary<string, string> NonSaveAfter,
    string PackageBefore,
    string PackageAfter,
    string Goal142Before,
    string Goal142After,
    SortedDictionary<string, string> Goal148Before,
    SortedDictionary<string, string> Goal148After,
    GeneratedCampaignSessionService LegacyService,
    GeneratedCampaignSnapshot Legacy);
