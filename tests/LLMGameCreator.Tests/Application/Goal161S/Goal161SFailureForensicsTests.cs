using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161S;

public sealed class Goal161SFailureForensicsTests
{
    [Theory]
    [InlineData("temp_pointer_write")]
    [InlineData("temporary_pointer_validation")]
    [InlineData("atomic_replace")]
    public void Behavioral_pointer_failure_preserves_prior_pointer(string stage)
    {
        using var root = new Goal161STempRoot(); var first = root.Resolve("a1b2c3d4e5f6"); Goal161STempRoot.WriteGreenRun(root.Locations, first); Assert.True(root.Locations.PublishCurrentPointer(first, Goal161STempRoot.Pointer(first)).Passed); var prior = File.ReadAllBytes(first.CurrentPointerPath);
        var second = root.Resolve("b1b2c3d4e5f6"); Goal161STempRoot.WriteGreenRun(root.Locations, second); var faulty = new ProjectStandaloneOutputLocationService(Path.Combine(root.Path, "o"), actual => { if (actual == stage) throw new InvalidOperationException(actual); }); var result = faulty.PublishCurrentPointer(second, Goal161STempRoot.Pointer(second));
        Assert.False(result.Passed); Assert.Equal(stage, result.Stage); Assert.True(result.PriorCurrentPreserved); Assert.Equal(prior, File.ReadAllBytes(first.CurrentPointerPath)); Assert.True(Directory.Exists(second.RunOutputFolder));
    }
    [Fact] public void Behavioral_failed_run_without_pointer_is_not_current() { using var root = new Goal161STempRoot(); var location = root.Resolve("a1b2c3d4e5f6"); Goal161STempRoot.WriteGreenRun(root.Locations, location); var faulty = new ProjectStandaloneOutputLocationService(Path.Combine(root.Path, "o"), _ => throw new InvalidOperationException("temp_pointer_write")); var result = faulty.PublishCurrentPointer(location, Goal161STempRoot.Pointer(location)); Assert.False(result.Passed); Assert.False(faulty.LoadCurrentOutput(root.Project, "package").Passed); }
    [Fact] public void Behavioral_failed_publication_exposes_exact_diagnostic() { using var root = new Goal161STempRoot(); var location = root.Resolve("a1b2c3d4e5f6"); Goal161STempRoot.WriteGreenRun(root.Locations, location); var faulty = new ProjectStandaloneOutputLocationService(Path.Combine(root.Path, "o"), _ => throw new InvalidOperationException("atomic_replace")); var result = faulty.PublishCurrentPointer(location, Goal161STempRoot.Pointer(location)); Assert.Equal("atomic_replace", result.Stage); Assert.Contains("InvalidOperationException: atomic_replace", result.Diagnostic); }
}
