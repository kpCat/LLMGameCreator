using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Tests.Application.Goal155;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal155A;

public sealed class Goal155AControllerTruthTests
{
    [Fact]
    public void Behavioral_controller_reports_current_only_for_exact_current_project_and_never_ready_after_package_tamper()
    {
        using var project = Goal155RealProject.CopySource("goal155a-controller-tamper");
        var controller = Goal155RealProject.Open(project.Path);
        var build = controller.BuildAndQualify();
        Assert.True(build.Passed, string.Join("; ", build.Diagnostics));
        var current = controller.Snapshot();
        RemovePayload(project.Path);
        WriteRecord(project.Path, current, build);

        var ready = controller.Snapshot();
        File.AppendAllText(Path.Combine(project.Path, "package.json"), "tamper", new UTF8Encoding(false));
        var tampered = controller.Snapshot();

        Assert.Equal("CURRENT", ready.ReleaseCandidateRecordConfigurationStatus);
        Assert.Equal("CURRENT", ready.ReleaseCandidateConfigurationStatus);
        Assert.Equal("ABSENT", tampered.ReleaseCandidateRecordConfigurationStatus);
        Assert.Equal("BUILD_GREEN_STANDALONE_PENDING", tampered.ReleaseCandidateConfigurationStatus);
        Assert.Contains("rc.read.current_package_hash_mismatch", tampered.Diagnostics);
    }

    [Fact]
    public void Behavioral_controller_keeps_old_valid_record_as_last_success_after_new_green_build_without_standalone()
    {
        using var project = Goal155RealProject.CopySource("goal155a-controller-new-build");
        var controller = Goal155RealProject.Open(project.Path);
        var first = controller.BuildAndQualify();
        Assert.True(first.Passed, string.Join("; ", first.Diagnostics));
        RemovePayload(project.Path);
        WriteRecord(project.Path, controller.Snapshot(), first);
        Goal155RealProject.Set(controller, "feature.dialogue.reputation_gated_reward", "trustedGoldReward", 9);
        var second = controller.BuildAndQualify();

        var snapshot = controller.Snapshot();

        Assert.True(second.Passed, string.Join("; ", second.Diagnostics));
        Assert.Equal("LAST_SUCCESS", snapshot.ReleaseCandidateRecordConfigurationStatus);
        Assert.Equal("BUILD_GREEN_STANDALONE_PENDING", snapshot.ReleaseCandidateConfigurationStatus);
        Assert.Contains("rc.read.record_build_identity_differs_from_current", snapshot.Diagnostics);
    }

    private static void WriteRecord(
        string project,
        UnifiedGameProjectWorkspaceSnapshot snapshot,
        GameProjectBuildResult build)
    {
        var summary = Assert.IsType<GameProjectAcceptedMechanicsSummary>(build.AcceptedMechanics);
        var record = new GameProjectReleaseCandidateRecord
        {
            CompletedAtUtc = DateTimeOffset.UtcNow,
            ProjectPackageId = snapshot.ProjectPackageId,
            ProjectTitle = snapshot.ProjectTitle,
            ProjectVersion = snapshot.ProjectVersion,
            QualifiedAuthoringFingerprint = summary.QualifiedAuthoringFingerprint,
            PackageSha256 = build.PackageSha256,
            CompositionPackageSha256 = build.CompositionPackageSha256,
            FinalStateHash = build.FinalStateHash,
            AcceptedMechanicsSummary = summary,
            HostCacheKey = "goal155a-controller-fixture",
            HostReused = true,
            LaunchSmokePassed = true,
            SelfCheckPassedCount = 1,
            SelfCheckTotalCount = 1,
            StandalonePackageSha256 = build.PackageSha256,
            StandaloneFinalStateHash = build.FinalStateHash,
            PlayerAdapterModelSha256 = new string('a', 64),
            HumanFactsSha256 = new string('b', 64)
        };
        var path = new GameProjectReleaseCandidateRecordService().RecordPath(project);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(record, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }) + Environment.NewLine, new UTF8Encoding(false));
    }

    private static void RemovePayload(string project)
    {
        var builds = Path.Combine(project, "Builds");
        if (Directory.Exists(builds)) Directory.Delete(builds, true);
    }
}
