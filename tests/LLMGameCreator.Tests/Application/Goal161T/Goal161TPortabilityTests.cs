using System.Diagnostics;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161T;

public sealed class Goal161TPortabilityTests
{
    [Fact]
    public void Behavioral_portable_all_selectable_without_operational_pointer_has_absent_evidence() { using var fixture = Goal161TFixture.Create(); File.Delete(fixture.Location.CurrentPointerPath); var evidence = fixture.Evidence.InspectForRead(fixture.Root.Project, "package"); Assert.Equal("absent", evidence.SourceKind); Assert.True(evidence.Passed); }

    [Fact]
    public void Behavioral_portable_core_only_has_no_false_rc_ready_status() { var status = GameProjectReleaseCandidateRecordService.ResolveOverallStatus(null, false, "package", "composition", "final", new GameProjectReleaseCandidateReadResult()); Assert.Equal("ABSENT", status); Assert.DoesNotContain(status, new[] { "CURRENT", "READY", "BUILD_GREEN_STANDALONE_PENDING" }); }

    [Fact]
    public void Behavioral_portable_rc_truth_uses_project_local_record_not_machine_output() { using var fixture = LLMGameCreator.Tests.Application.Goal155.Goal155RcFixture.Create("portable-record-only"); fixture.Write(); var root = Path.GetDirectoryName(fixture.PlayerAdapterModelPath)!; Directory.Delete(Path.GetFullPath(Path.Combine(root, "..", "..", "..", "..", "..")), true); var read = fixture.Read(); Assert.Equal("CURRENT", read.ConfigurationStatus); }

    [Fact]
    public void Behavioral_portable_and_evidence_reads_keep_player_unity_counts_zero() { using var fixture = Goal161TFixture.Create(); var unityBefore = Process.GetProcessesByName("Unity").Length; _ = fixture.Evidence.InspectForRead(fixture.Root.Project, "package"); var unityAfter = Process.GetProcessesByName("Unity").Length; Assert.Equal(unityBefore, unityAfter); }
}
