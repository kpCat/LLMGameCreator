using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161S;

public sealed class Goal161SQualificationTests
{
    [Fact] public void Behavioral_fresh_service_resolves_current_output() { using var root = new Goal161STempRoot(); var location = root.Resolve("a1b2c3d4e5f6"); Goal161STempRoot.WriteGreenRun(root.Locations, location); Assert.True(root.Locations.PublishCurrentPointer(location, Goal161STempRoot.Pointer(location)).Passed); var fresh = new ProjectStandaloneOutputLocationService(Path.Combine(root.Path, "o")); var current = fresh.LoadCurrentOutput(root.Project, "package"); Assert.True(current.Passed, current.Diagnostic); Assert.Equal(Path.Combine(location.RunOutputFolder, "g.exe"), current.ExecutablePath); }
    [Fact] public void Behavioral_run_status_is_required_for_current_pointer() { using var root = new Goal161STempRoot(); var location = root.Resolve("a1b2c3d4e5f6"); Goal161STempRoot.WriteGreenRun(root.Locations, location); File.Delete(Path.Combine(location.RunOutputFolder, "run-status.json")); var result = root.Locations.PublishCurrentPointer(location, Goal161STempRoot.Pointer(location)); Assert.False(result.Passed); Assert.False(File.Exists(location.CurrentPointerPath)); }
    [Fact] public void Behavioral_current_pointer_requires_green_payload_self_check() { using var root = new Goal161STempRoot(); var location = root.Resolve("a1b2c3d4e5f6"); Goal161STempRoot.WriteGreenRun(root.Locations, location); File.WriteAllText(Path.Combine(location.RunOutputFolder, "g_Data", "StreamingAssets", "LLMGameCreatorProject", "player-adapter-frames.json"), "[]"); var result = root.Locations.PublishCurrentPointer(location, Goal161STempRoot.Pointer(location)); Assert.False(result.Passed); }
}
