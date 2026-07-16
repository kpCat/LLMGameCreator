using System.Text;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161Q;

public sealed class Goal161QSmokeDiagnosticsTests
{
    private static readonly string[] RequiredMarkers =
    [
        "LLMGC_PROJECT_STANDALONE_LOAD_PASS",
        "LLMGC_PROJECT_STANDALONE_INTEGRITY_PASS",
        "LLMGC_PROJECT_STANDALONE_NAVIGATION_PASS",
        "LLMGC_PROJECT_STANDALONE_RUNTIME_AUTHORITY_PASS",
        "LLMGC_PROJECT_STANDALONE_SMOKE_PASS"
    ];

    [Fact]
    public void Behavioral_self_check_failure_prevents_process_invocation()
    {
        var service = new ProjectStandaloneBuildService(Goal161QForensics.RepositoryRoot());
        var preflight = new ProjectStandalonePayloadSelfCheckResult
        {
            Passed = false,
            FailedCheckCodes = ["standalone.payload.package_hash_mismatch"]
        };

        var result = service.RunSmoke("missing-never-started.exe", preflight);

        Assert.False(result.ProcessStarted);
        Assert.Equal(-1, result.ExitCode);
        Assert.Equal("standalone.payload.preflight_failed", result.NamedFailure);
    }

    [Fact]
    public void Behavioral_detailed_smoke_result_captures_exit_marker_and_bounded_player_exception()
    {
        using var files = SmokeFiles.Create(
            "LLMGC_PROJECT_STANDALONE_SMOKE_FAIL",
            "InvalidOperationException: Could not find a part of the path "
            + Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            + "\\payload.json");

        var result = ProjectStandaloneBuildService.InterpretSmokeArtifacts(
            2, files.MarkerPath, files.PlayerPath);

        Assert.False(result.Passed);
        Assert.Equal(2, result.ExitCode);
        Assert.Equal("LLMGC_PROJECT_STANDALONE_SMOKE_FAIL", result.SmokeMarkerText);
        Assert.True(result.PlayerLogPresent);
        Assert.Equal("standalone.player.payload_path_unreadable", result.NamedFailure);
        Assert.Contains(result.PlayerLogRelevantLines, line => line.Contains("<machine-root>", StringComparison.Ordinal));
        Assert.All(result.PlayerLogRelevantLines, line => Assert.True(line.Length <= 401));
    }

    [Fact]
    public void Behavioral_green_smoke_requires_all_five_markers_and_a_player_log()
    {
        var markers = string.Join(Environment.NewLine, RequiredMarkers);
        using var files = SmokeFiles.Create(markers, "LLMGC_PROJECT_STANDALONE_SMOKE_PASS");

        var result = ProjectStandaloneBuildService.InterpretSmokeArtifacts(
            0, files.MarkerPath, files.PlayerPath);

        Assert.True(result.Passed);
        Assert.True(result.ProcessStarted);
        Assert.Equal(string.Empty, result.NamedFailure);
    }

    [Fact]
    public void Behavioral_missing_player_log_is_a_named_failure_even_with_green_markers()
    {
        var markers = string.Join(Environment.NewLine, RequiredMarkers);
        using var files = SmokeFiles.Create(markers, null);

        var result = ProjectStandaloneBuildService.InterpretSmokeArtifacts(
            0, files.MarkerPath, files.PlayerPath);

        Assert.False(result.Passed);
        Assert.False(result.PlayerLogPresent);
        Assert.Equal("standalone.smoke.player_log_missing", result.NamedFailure);
    }

    private sealed class SmokeFiles : IDisposable
    {
        private SmokeFiles(string root)
        {
            Root = root;
            MarkerPath = Path.Combine(root, "marker.log");
            PlayerPath = Path.Combine(root, "player.log");
        }

        public string Root { get; }
        public string MarkerPath { get; }
        public string PlayerPath { get; }

        public static SmokeFiles Create(string marker, string? player)
        {
            var files = new SmokeFiles(Path.Combine(Path.GetTempPath(), "LLMGameCreator",
                "Goal161QTests", Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(files.Root);
            File.WriteAllText(files.MarkerPath, marker, new UTF8Encoding(false));
            if (player is not null)
                File.WriteAllText(files.PlayerPath, player, new UTF8Encoding(false));
            return files;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root)) Directory.Delete(Root, true);
        }
    }
}
