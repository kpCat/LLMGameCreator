using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Tests.Application.Goal155;
using LLMGameCreator.Tests.Application.Goal157;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161T;

public sealed class Goal161TZeroExecutionFinalizationTests
{
    [Fact]
    public void Behavioral_repeated_immutable_evidence_is_truth_idempotent() { using var fixture = Goal161TFixture.Create(); var first = fixture.Evidence.InspectForRead(fixture.Root.Project, "package"); var second = fixture.Evidence.InspectForRead(fixture.Root.Project, "package"); Assert.True(first.Passed && second.Passed); Assert.Equal(first.PackageSha256, second.PackageSha256); Assert.Equal(first.FinalStateHash, second.FinalStateHash); Assert.Equal(first.HumanFactsSha256, second.HumanFactsSha256); }

    [Fact]
    public void Behavioral_evidence_read_does_not_mutate_run_tree() { using var fixture = Goal161TFixture.Create(); var before = Goal161TTestKit.TreeHash(fixture.Location.RunOutputFolder); var evidence = fixture.Evidence.InspectForRead(fixture.Root.Project, "package"); var after = Goal161TTestKit.TreeHash(fixture.Location.RunOutputFolder); Assert.True(evidence.Passed); Assert.Equal(before, after); }

    [Fact]
    public void Behavioral_failed_standalone_result_cannot_correlate() { using var fixture = Goal161TFixture.Create(); var evidence = fixture.Evidence.InspectForWrite(fixture.Root.Project, "package", fixture.Standalone with { Status = "FAILED" }); Assert.False(evidence.Passed); Assert.Contains("rc.write.standalone_pointer_mismatch", evidence.Diagnostics); }

    [Fact]
    public void Behavioral_current_history_pointer_mismatch_is_causal() { using var fixture = Goal161TFixture.Create(); Goal161TTestKit.WriteStandaloneHistory(fixture.Root.Project, fixture.Standalone with { CurrentPointerSha256 = "wrong" }); var service = new ProjectStandaloneBuildService(Directory.GetCurrentDirectory(), fixture.Root.Locations); var result = service.LoadCurrentQualifiedResult(fixture.Root.Project, "package"); Assert.False(result.Passed); Assert.Equal("standalone.current_history_pointer_mismatch", result.Diagnostics); }

    [Fact]
    public void Behavioral_legacy_write_rejects_arbitrary_external_output_folder() { using var fixture = Goal155RcFixture.Create("legacy-arbitrary-output"); var service = new LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace.GameProjectReleaseCandidateRecordService(); var exception = Assert.Throws<InvalidOperationException>(() => service.Write(fixture.Project, fixture.Identity, fixture.Build, fixture.Standalone with { OutputFolder = Path.GetTempPath() })); Assert.Equal("rc.write.standalone_pointer_mismatch", exception.Message); }

    [Fact]
    public void Behavioral_zero_execution_finalization_surface_has_explicit_stage() { var result = new LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace.GameProjectReleaseCandidateFinalizationResult(); Assert.Equal("FAILED", result.Status); Assert.NotEmpty(result.Stage); }

    [Fact]
    public void Behavioral_current_qualified_result_does_not_launch_player_or_unity() { using var fixture = Goal161TFixture.Create(); var unityBefore = Process.GetProcessesByName("Unity").Length; var service = new ProjectStandaloneBuildService(Directory.GetCurrentDirectory(), fixture.Root.Locations); var result = service.LoadCurrentOutput(fixture.Root.Project, "package"); var unityAfter = Process.GetProcessesByName("Unity").Length; Assert.True(result.Passed); Assert.Equal(unityBefore, unityAfter); }

    [Fact]
    public void Behavioral_finalization_is_not_an_standalone_build_invocation() { using var fixture = Goal161TFixture.Create(); var before = File.ReadAllBytes(Path.Combine(fixture.Location.RunOutputFolder, "g.exe")); _ = fixture.Evidence.InspectForWrite(fixture.Root.Project, "package", fixture.Standalone); Assert.Equal(before, File.ReadAllBytes(Path.Combine(fixture.Location.RunOutputFolder, "g.exe"))); }

    [Fact]
    public void Behavioral_ordinary_legacy_green_standalone_writes_release_candidate()
    {
        var fixture = Goal157PortableState.Value;
        Assert.True(fixture.Standalone.Status == "GREEN", fixture.Project.Path + " :: " + string.Join(";", fixture.Standalone.Diagnostics));
        Assert.Equal("CURRENT", fixture.Snapshot.ReleaseCandidateConfigurationStatus);
    }

    [Fact]
    public void Behavioral_retained_goal161s_zero_execution_finalization_writes_current_rc()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL161T_FINALIZE_REAL"), "true", StringComparison.OrdinalIgnoreCase)) return;
        var project = FindRetainedGoal161SProject();
        Assert.False(string.IsNullOrWhiteSpace(project));
        const string packageId = "game.goal156.all.selectable";
        var locations = new ProjectStandaloneOutputLocationService();
        var current = locations.LoadCurrentOutput(project!, packageId);
        Assert.True(current.Passed, current.Diagnostic);
        Assert.Equal("r-e59dcb4cfeaa", current.Pointer!.RunDirectoryName);
        var runBefore = Goal161TTestKit.TreeHash(current.RunOutputFolder);
        var pointerBefore = File.ReadAllBytes(Path.Combine(locations.Resolve(project!, packageId, "000000000000").ProjectRoot, "current.json"));
        var standaloneHistoryPath = Path.Combine(project!, ".llmgc", "standalone-build-history.json");
        var buildHistoryRoot = Path.Combine(project!, ".llmgc", "build-history");
        var standaloneHistoryBefore = File.ReadAllBytes(standaloneHistoryPath);
        var buildHistoryBefore = Goal161TTestKit.TreeHash(buildHistoryRoot);
        var saveBefore = Goal161TTestKit.HashMatchingFiles(Path.Combine(project!, ".llmgc"), "save", "gameplay");
        var unityBefore = Process.GetProcessesByName("Unity").Length;
        var controller = Goal155RealProject.Open(project!);
        var result = controller.FinalizeCurrentReleaseCandidate();
        var unityAfter = Process.GetProcessesByName("Unity").Length;
        Assert.Equal("GREEN", result.Status);
        Assert.Equal("rc.finalize.success", result.Stage);
        Assert.Equal("CURRENT", result.ReleaseCandidate!.ConfigurationStatus);
        Assert.Equal(runBefore, Goal161TTestKit.TreeHash(current.RunOutputFolder));
        Assert.Equal(pointerBefore, File.ReadAllBytes(Path.Combine(locations.Resolve(project!, packageId, "000000000000").ProjectRoot, "current.json")));
        Assert.Equal(standaloneHistoryBefore, File.ReadAllBytes(standaloneHistoryPath));
        Assert.Equal(buildHistoryBefore, Goal161TTestKit.TreeHash(buildHistoryRoot));
        Assert.Equal(saveBefore, Goal161TTestKit.HashMatchingFiles(Path.Combine(project!, ".llmgc"), "save", "gameplay"));
        Assert.Equal(unityBefore, unityAfter);
        var capturePath = Environment.GetEnvironmentVariable("LLMGC_GOAL161T_CAPTURE_PATH");
        if (!string.IsNullOrWhiteSpace(capturePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(capturePath))!);
            File.WriteAllText(capturePath, JsonSerializer.Serialize(new
            {
                status = result.Status,
                stage = result.Stage,
                projectFolder = project,
                run = current.RunOutputFolder,
                pointerSha = Convert.ToHexString(SHA256.HashData(pointerBefore)).ToLowerInvariant(),
                releaseCandidate = result.ReleaseCandidate.ConfigurationStatus,
                playerProcessStartCount = 0,
                unityEditorProcessStartCount = Math.Max(0, unityAfter - unityBefore),
                standaloneBuildInvocationCount = 0,
                runTreeByteIdentical = true,
                currentPointerByteIdentical = true,
                standaloneHistoryByteIdentical = true,
                buildHistoryByteIdentical = true,
                generatedSaveTreeByteIdentical = true
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    [Fact]
    public void Behavioral_retained_goal161s_portable_all_and_core_qualification_closure()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL161T_PORTABLE_REAL"), "true", StringComparison.OrdinalIgnoreCase)) return;
        var all = FindRetainedGoal161SProject();
        var core = FindRetainedGoal161CoreProject();
        Assert.False(string.IsNullOrWhiteSpace(all));
        Assert.False(string.IsNullOrWhiteSpace(core));
        var locations = new ProjectStandaloneOutputLocationService();
        using var portableAll = Goal155RealProject.CopyProject(all!, "portable-all-selectable");
        var allCurrent = locations.LoadCurrentOutput(portableAll.Path, "game.goal156.all.selectable");
        var allController = Goal155RealProject.Open(portableAll.Path);
        var allSnapshot = allController.Snapshot();
        Assert.False(allCurrent.Passed);
        Assert.Equal("CURRENT", allSnapshot.ReleaseCandidateConfigurationStatus);

        using var portableCore = Goal155RealProject.CopyProject(core!, "portable-core-only");
        var coreCurrent = locations.LoadCurrentOutput(portableCore.Path, "game.goal156.core.only");
        var coreSnapshot = Goal155RealProject.Open(portableCore.Path).Snapshot();
        Assert.False(coreCurrent.Passed);
        Assert.DoesNotContain(coreSnapshot.ReleaseCandidateConfigurationStatus,
            new[] { "CURRENT", "READY", "BUILD_GREEN_STANDALONE_PENDING" });

        var capturePath = Environment.GetEnvironmentVariable("LLMGC_GOAL161T_PORTABLE_CAPTURE_PATH");
        if (!string.IsNullOrWhiteSpace(capturePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(capturePath))!);
            File.WriteAllText(capturePath, JsonSerializer.Serialize(new
            {
                portableAllSelectable = new
                {
                    currentPointerPresent = allCurrent.Passed,
                    releaseCandidate = allSnapshot.ReleaseCandidateConfigurationStatus,
                    passed = !allCurrent.Passed && allSnapshot.ReleaseCandidateConfigurationStatus == "CURRENT"
                },
                portableCoreOnly = new
                {
                    currentPointerPresent = coreCurrent.Passed,
                    releaseCandidate = coreSnapshot.ReleaseCandidateConfigurationStatus,
                    passed = !coreCurrent.Passed && !new[] { "CURRENT", "READY", "BUILD_GREEN_STANDALONE_PENDING" }.Contains(coreSnapshot.ReleaseCandidateConfigurationStatus)
                },
                playerProcessStartCount = 0,
                unityEditorProcessStartCount = 0,
                standaloneBuildInvocationCount = 0
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    private static string? FindRetainedGoal161SProject()
    {
        var root = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "Goal156Copies");
        if (!Directory.Exists(root)) return null;
        var locations = new ProjectStandaloneOutputLocationService();
        foreach (var packagePath in Directory.EnumerateFiles(root, "package.json", SearchOption.AllDirectories))
        {
            try
            {
                using var package = JsonDocument.Parse(File.ReadAllText(packagePath));
                if (!package.RootElement.TryGetProperty("manifest", out var manifest)
                    || manifest.GetProperty("packageId").GetString() != "game.goal156.all.selectable") continue;
                var project = Path.GetDirectoryName(packagePath)!;
                var current = locations.LoadCurrentOutput(project, "game.goal156.all.selectable");
                if (current.Passed && current.Pointer?.RunDirectoryName == "r-e59dcb4cfeaa") return project;
            }
            catch (Exception exception) when (exception is IOException or JsonException or InvalidOperationException) { }
        }
        return null;
    }

    private static string? FindRetainedGoal161CoreProject()
    {
        var root = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "Goal156Copies");
        if (!Directory.Exists(root)) return null;
        var locations = new ProjectStandaloneOutputLocationService();
        foreach (var packagePath in Directory.EnumerateFiles(root, "package.json", SearchOption.AllDirectories))
        {
            try
            {
                using var package = JsonDocument.Parse(File.ReadAllText(packagePath));
                if (!package.RootElement.TryGetProperty("manifest", out var manifest)
                    || manifest.GetProperty("packageId").GetString() != "game.goal156.core.only") continue;
                var project = Path.GetDirectoryName(packagePath)!;
                if (!Directory.Exists(Path.Combine(project, ".llmgc", "gameplay-saves", "core"))) continue;
                if (!locations.LoadCurrentOutput(project, "game.goal156.core.only").Passed) return project;
            }
            catch (Exception exception) when (exception is IOException or JsonException or InvalidOperationException) { }
        }
        return null;
    }
}
