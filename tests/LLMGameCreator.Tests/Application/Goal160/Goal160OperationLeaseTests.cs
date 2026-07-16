using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal160;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class Goal160Collection
{
    public const string Name = "Goal160 serialized world lifecycle";
}

[Collection(Goal160Collection.Name)]
public sealed class Goal160OperationLeaseTests
{
    [Fact]
    public void Behavioral_build_lease_rejects_regeneration_preview()
    {
        using var project = OperationProject.Create();
        var coordinator = new GameProjectOperationCoordinator();
        using var build = coordinator.TryAcquire(project.Path, GameProjectOperationKinds.Build);
        using var rejected = coordinator.TryAcquire(project.Path, GameProjectOperationKinds.RegenerationPreview);
        Assert.True(build.Acquired);
        Assert.False(rejected.Acquired);
        Assert.Equal("project_operation.busy:build", rejected.Diagnostic);
    }

    [Fact]
    public void Behavioral_regeneration_preview_rejects_build()
    {
        using var project = OperationProject.Create();
        var coordinator = new GameProjectOperationCoordinator();
        using var regeneration = coordinator.TryAcquire(project.Path, GameProjectOperationKinds.RegenerationPreview);
        using var rejected = coordinator.TryAcquire(project.Path, GameProjectOperationKinds.Build);
        Assert.Equal("project_operation.busy:regeneration_preview", rejected.Diagnostic);
    }

    [Fact]
    public void Behavioral_regeneration_apply_rejects_standalone()
    {
        using var project = OperationProject.Create();
        var coordinator = new GameProjectOperationCoordinator();
        using var regeneration = coordinator.TryAcquire(project.Path, GameProjectOperationKinds.RegenerationApply);
        using var rejected = coordinator.TryAcquire(project.Path, GameProjectOperationKinds.Standalone);
        Assert.Equal("project_operation.busy:regeneration_apply", rejected.Diagnostic);
    }

    [Theory]
    [InlineData("authoring_save")]
    [InlineData("build")]
    [InlineData("regeneration_preview")]
    public void Behavioral_standalone_rejects_other_mutation_routes(string operationKind)
    {
        using var project = OperationProject.Create();
        var coordinator = new GameProjectOperationCoordinator();
        using var standalone = coordinator.TryAcquire(project.Path, GameProjectOperationKinds.Standalone);
        using var rejected = coordinator.TryAcquire(project.Path, operationKind);
        Assert.False(rejected.Acquired);
        Assert.Equal("project_operation.busy:standalone", rejected.Diagnostic);
    }

    [Fact]
    public async Task Behavioral_two_simultaneous_entries_have_one_owner()
    {
        using var project = OperationProject.Create();
        var coordinator = new GameProjectOperationCoordinator();
        using var gate = new ManualResetEventSlim(false);
        GameProjectOperationLease? first = null;
        var task = Task.Run(() =>
        {
            first = coordinator.TryAcquire(project.Path, GameProjectOperationKinds.Build);
            gate.Set();
        });
        Assert.True(gate.Wait(TimeSpan.FromSeconds(5)));
        using var second = coordinator.TryAcquire(project.Path, GameProjectOperationKinds.Build);
        await task;
        Assert.True(first!.Acquired);
        Assert.False(second.Acquired);
        first.Dispose();
    }

    [Fact]
    public void Behavioral_cross_coordinator_project_lock_rejects_second_owner()
    {
        using var project = OperationProject.Create();
        var firstCoordinator = new GameProjectOperationCoordinator();
        var secondCoordinator = new GameProjectOperationCoordinator();
        using var first = firstCoordinator.TryAcquire(project.Path, GameProjectOperationKinds.Build);
        using var second = secondCoordinator.TryAcquire(project.Path, GameProjectOperationKinds.RegenerationApply);
        Assert.True(first.Acquired);
        Assert.Equal("project_operation.busy:external", second.Diagnostic);
    }

    [Fact]
    public void Behavioral_child_candidate_build_requires_matching_owner()
    {
        using var project = OperationProject.Create();
        using var candidate = OperationProject.Create();
        var coordinator = new GameProjectOperationCoordinator();
        var foreign = new GameProjectOperationCoordinator();
        using var owner = coordinator.TryAcquire(project.Path, GameProjectOperationKinds.RegenerationPreview);
        using var child = coordinator.TryAcquireChild(owner, candidate.Path, GameProjectOperationKinds.Build);
        using var foreignOwner = foreign.TryAcquire(OperationProject.CreatePath(), GameProjectOperationKinds.Build);
        using var rejected = coordinator.TryAcquireChild(foreignOwner, candidate.Path, GameProjectOperationKinds.Build);
        Assert.True(child.Acquired);
        Assert.Equal(owner.OperationId, child.OwnerOperationIdForTests());
        Assert.Equal("project_operation.lease_invalid", rejected.Diagnostic);
    }

    [Fact]
    public void Behavioral_disposed_lease_is_rejected()
    {
        using var project = OperationProject.Create();
        var coordinator = new GameProjectOperationCoordinator();
        var owner = coordinator.TryAcquire(project.Path, GameProjectOperationKinds.Build);
        owner.Dispose();
        Assert.False(coordinator.IsCurrent(owner, project.Path));
        using var child = coordinator.TryAcquireChild(owner, project.Path, GameProjectOperationKinds.Build);
        Assert.Equal("project_operation.lease_invalid", child.Diagnostic);
    }

    [Fact]
    public void Behavioral_lock_released_after_success()
    {
        using var project = OperationProject.Create();
        var coordinator = new GameProjectOperationCoordinator();
        using (var first = coordinator.TryAcquire(project.Path, GameProjectOperationKinds.Build))
            Assert.True(first.Acquired);
        using var second = coordinator.TryAcquire(project.Path, GameProjectOperationKinds.RegenerationPreview);
        Assert.True(second.Acquired);
    }

    [Fact]
    public void Behavioral_lock_released_after_exception()
    {
        using var project = OperationProject.Create();
        var coordinator = new GameProjectOperationCoordinator();
        Assert.Throws<InvalidOperationException>((Action)(() =>
        {
            using var lease = coordinator.TryAcquire(project.Path, GameProjectOperationKinds.Build);
            throw new InvalidOperationException("injected");
        }));
        using var next = coordinator.TryAcquire(project.Path, GameProjectOperationKinds.Recovery);
        Assert.True(next.Acquired);
    }
}

internal static class Goal160LeaseTestExtensions
{
    public static string OwnerOperationIdForTests(this GameProjectOperationLease lease) =>
        lease.IsChildForTests() ? lease.OperationId.Length > 0 ? lease.OwnerFromReflection() : string.Empty : lease.OperationId;

    private static bool IsChildForTests(this GameProjectOperationLease lease) =>
        (bool)(typeof(GameProjectOperationLease).GetProperty("IsChild",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(lease) ?? false);

    private static string OwnerFromReflection(this GameProjectOperationLease lease) =>
        (string)(typeof(GameProjectOperationLease).GetProperty("OwnerOperationId",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(lease) ?? string.Empty);
}

internal sealed class OperationProject : IDisposable
{
    private OperationProject(string path) => Path = path;
    public string Path { get; }
    public static OperationProject Create() => new(CreatePath());
    public static string CreatePath()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator", "Goal160Operations",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
    public void Dispose()
    {
        if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true);
    }
}
