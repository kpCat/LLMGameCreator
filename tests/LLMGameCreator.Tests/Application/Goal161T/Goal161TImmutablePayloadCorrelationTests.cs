using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Tests.Application.Goal161S;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161T;

public sealed class Goal161TImmutablePayloadCorrelationTests
{
    [Fact]
    public void Behavioral_immutable_current_pointer_resolves_payload() { using var fixture = Goal161TFixture.Create(); var evidence = fixture.Evidence.InspectForRead(fixture.Root.Project, "package"); Assert.True(evidence.Passed, string.Join(";", evidence.Diagnostics)); Assert.Equal("immutable_current_pointer", evidence.SourceKind); }

    [Fact]
    public void Behavioral_payload_root_is_runs_g_data_not_project_builds() { using var fixture = Goal161TFixture.Create(); var evidence = fixture.Evidence.InspectForRead(fixture.Root.Project, "package"); Assert.StartsWith(Path.Combine(fixture.Root.Locations.Root, fixture.Location.ProjectToken, "runs"), evidence.RunOutputFolder, StringComparison.OrdinalIgnoreCase); Assert.DoesNotContain(Path.Combine(fixture.Root.Project, "Builds", "Windows"), evidence.RunOutputFolder, StringComparison.OrdinalIgnoreCase); }

    [Fact]
    public void Behavioral_expected_standalone_result_exact_correlation_passes() { using var fixture = Goal161TFixture.Create(); var evidence = fixture.Evidence.InspectForWrite(fixture.Root.Project, "package", fixture.Standalone); Assert.True(evidence.Passed, string.Join(";", evidence.Diagnostics)); }

    [Fact]
    public void Behavioral_attempt_mismatch_is_rejected() { using var fixture = Goal161TFixture.Create(); var evidence = fixture.Evidence.InspectForWrite(fixture.Root.Project, "package", fixture.Standalone with { AttemptId = "wrong" }); Assert.False(evidence.Passed); Assert.Contains("rc.write.standalone_pointer_mismatch", evidence.Diagnostics); }

    [Fact]
    public void Behavioral_project_token_mismatch_is_rejected() { using var fixture = Goal161TFixture.Create(); var evidence = fixture.Evidence.InspectForWrite(fixture.Root.Project, "package", fixture.Standalone with { OutputProjectToken = "wrong" }); Assert.False(evidence.Passed); Assert.Contains("rc.write.standalone_pointer_mismatch", evidence.Diagnostics); }

    [Fact]
    public void Behavioral_run_directory_mismatch_is_rejected() { using var fixture = Goal161TFixture.Create(); var evidence = fixture.Evidence.InspectForWrite(fixture.Root.Project, "package", fixture.Standalone with { OutputRunDirectoryName = "r-b1b2c3d4e5f6" }); Assert.False(evidence.Passed); Assert.Contains("rc.write.standalone_pointer_mismatch", evidence.Diagnostics); }

    [Fact]
    public void Behavioral_current_pointer_sha_mismatch_is_rejected() { using var fixture = Goal161TFixture.Create(); var evidence = fixture.Evidence.InspectForWrite(fixture.Root.Project, "package", fixture.Standalone with { CurrentPointerSha256 = "wrong" }); Assert.False(evidence.Passed); Assert.Contains("rc.write.standalone_pointer_mismatch", evidence.Diagnostics); }

    [Fact]
    public void Behavioral_output_folder_and_executable_mismatch_are_rejected() { using var fixture = Goal161TFixture.Create(); var evidence = fixture.Evidence.InspectForWrite(fixture.Root.Project, "package", fixture.Standalone with { OutputFolder = fixture.Root.Project, ExecutablePath = fixture.Root.Project }); Assert.False(evidence.Passed); Assert.Contains("rc.write.standalone_pointer_mismatch", evidence.Diagnostics); }

    [Fact]
    public void Behavioral_missing_immutable_model_is_rejected() { using var fixture = Goal161TFixture.Create(); File.Delete(Path.Combine(fixture.Location.RunOutputFolder, "g_Data", "StreamingAssets", "LLMGameCreatorProject", "player-adapter-model.json")); var evidence = fixture.Evidence.InspectForRead(fixture.Root.Project, "package"); Assert.False(evidence.Passed); Assert.Contains("rc.payload.current_pointer_invalid", evidence.Diagnostics[0]); }

    [Fact]
    public void Behavioral_immutable_payload_hash_mismatch_is_rejected() { using var fixture = Goal161TFixture.Create(); var manifest = Path.Combine(fixture.Location.RunOutputFolder, "g_Data", "StreamingAssets", "LLMGameCreatorProject", "project-manifest.json"); var json = JsonNode.Parse(File.ReadAllText(manifest))!.AsObject(); json["finalStateHash"] = "wrong"; File.WriteAllText(manifest, json.ToJsonString()); var evidence = fixture.Evidence.InspectForRead(fixture.Root.Project, "package"); Assert.False(evidence.Passed); Assert.Contains("rc.payload.current_run_hash_mismatch", evidence.Diagnostics[0]); }
}

internal sealed class Goal161TFixture : IDisposable
{
    private Goal161TFixture(Goal161STempRoot root, ProjectStandaloneOutputLocation location, ProjectStandaloneBuildResult standalone)
    { Root = root; Location = location; Standalone = standalone; Evidence = new ProjectStandalonePayloadEvidenceService(root.Locations); }

    public Goal161STempRoot Root { get; }
    public ProjectStandaloneOutputLocation Location { get; }
    public ProjectStandaloneBuildResult Standalone { get; }
    public ProjectStandalonePayloadEvidenceService Evidence { get; }

    public static Goal161TFixture Create()
    {
        var root = new Goal161STempRoot("goal161t-" + Guid.NewGuid().ToString("N"));
        var location = root.Resolve("a1b2c3d4e5f6");
        Goal161STempRoot.WriteGreenRun(root.Locations, location);
        var modelPath = Path.Combine(location.RunOutputFolder, "g_Data", "StreamingAssets", "LLMGameCreatorProject", "player-adapter-model.json");
        var model = JsonNode.Parse(File.ReadAllText(modelPath))!.AsObject();
        model["finalStateHash"] = "final";
        File.WriteAllText(modelPath, model.ToJsonString());
        Assert.True(root.Locations.PublishCurrentPointer(location, Goal161STempRoot.Pointer(location)).Passed);
        var pointer = root.Locations.LoadCurrentOutput(root.Project, "package").Pointer!;
        var pointerSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(location.CurrentPointerPath))).ToLowerInvariant();
        var standalone = new ProjectStandaloneBuildResult
        {
            AttemptId = pointer.PublishedAttemptId, Status = "GREEN", ProjectFolder = root.Project,
            OutputFolder = location.RunOutputFolder, ExecutablePath = Path.Combine(location.RunOutputFolder, "g.exe"),
            PackageSha256 = pointer.PackageSha256, FinalStateHash = pointer.FinalStateHash,
            HostCacheKey = pointer.HostCacheKey, HostReused = true, HostRebuilt = false,
            LaunchSmokePassed = true, PayloadSelfCheckPassed = true, LegacyHostParserCompatibilityPassed = true,
            SelfCheckPassedCount = 13, SelfCheckTotalCount = 13, SmokeExitCode = 0,
            OutputLocationKind = ProjectStandaloneBuildVocabulary.ImmutableOutputLocationKind,
            OutputProjectToken = pointer.ProjectToken, OutputRunDirectoryName = pointer.RunDirectoryName,
            CurrentPointerPath = location.CurrentPointerPath, CurrentPointerSha256 = pointerSha,
            BuildManifestPath = Path.Combine(location.RunOutputFolder, "build-manifest.json")
        };
        return new Goal161TFixture(root, location, standalone);
    }

    public void Dispose() => Root.Dispose();
}
