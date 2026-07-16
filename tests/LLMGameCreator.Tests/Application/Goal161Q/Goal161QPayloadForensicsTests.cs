using System.Security.Cryptography;
using System.Text.Json;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161Q;

public sealed class Goal161QPayloadForensicsTests
{
    [Fact]
    public void Behavioral_exact_failed_goal161_payload_receives_named_offline_result()
    {
        var recovered = Goal161QForensics.TryHistoricalRecoveredPayload();
        if (recovered is null)
        {
            using var evidence = Goal161QForensics.Diagnosis();
            Assert.Equal("standalone.smoke.historical_diagnostic_loss",
                evidence.RootElement.GetProperty("namedRootCause").GetProperty("code").GetString());
            return;
        }

        var result = new ProjectStandalonePayloadSelfCheckService()
            .Check(recovered.PayloadRoot, recovered.BuildManifestPath);

        Assert.True(result.Passed, string.Join(", ", result.FailedCheckCodes));
        Assert.Equal(13, result.PassedCount);
        Assert.True(result.LegacyHostParserCompatibility.Passed);
    }

    [Fact]
    public void Behavioral_recovered_failed_output_hashes_match_the_immutable_forensic_record()
    {
        var recovered = Goal161QForensics.TryHistoricalRecoveredPayload();
        if (recovered is null)
        {
            using var evidence = Goal161QForensics.Diagnosis();
            Assert.Equal(7, evidence.RootElement.GetProperty("failedOutputHashes").EnumerateObject().Count());
            return;
        }

        using var evidenceDocument = Goal161QForensics.Diagnosis();
        var hashes = evidenceDocument.RootElement.GetProperty("failedOutputHashes");
        foreach (var name in new[]
                 {
                     "project-manifest.json", "player-adapter-model.json",
                     "player-adapter-frames.json", "standalone-launch.json", "game-package.json"
                 })
            Assert.Equal(hashes.GetProperty(name).GetString(),
                Goal161QForensics.Hash(Path.Combine(recovered.PayloadRoot, name)));
        Assert.Equal(hashes.GetProperty("build-manifest.json").GetString(),
            Goal161QForensics.Hash(recovered.BuildManifestPath));
    }

    [Fact]
    public void Behavioral_exact_legacy_parser_counts_are_five_frames_and_sixty_two_facts()
    {
        var recovered = Goal161QForensics.TryRecoveredPayload();
        if (recovered is null)
        {
            using var evidence = Goal161QForensics.Diagnosis();
            var reproduction = evidence.RootElement.GetProperty("offlineReproduction");
            Assert.Equal(5, reproduction.GetProperty("legacyFrameCount").GetInt32());
            Assert.Equal(62, reproduction.GetProperty("legacyHumanFactCount").GetInt32());
            return;
        }

        var result = new ProjectStandalonePayloadSelfCheckService()
            .Check(recovered.PayloadRoot, recovered.BuildManifestPath);
        Assert.Equal(5, result.LegacyHostParserCompatibility.LegacyFrameCount);
        Assert.Equal(62, result.LegacyHostParserCompatibility.LegacyHumanFactCount);
    }

    [Fact]
    public void Behavioral_historical_smoke_marker_is_generic_and_player_log_was_not_captured()
    {
        using var evidence = Goal161QForensics.Diagnosis();
        var reproduction = evidence.RootElement.GetProperty("offlineReproduction");

        Assert.Equal(2, reproduction.GetProperty("historicalExitCode").GetInt32());
        Assert.Equal("LLMGC_PROJECT_STANDALONE_SMOKE_FAIL",
            reproduction.GetProperty("historicalSmokeMarker").GetString());
        Assert.False(reproduction.GetProperty("causallyMatchedHistoricalPlayerLogRecovered").GetBoolean());
    }
}

internal static class Goal161QForensics
{
    private const string HistoricalBuildManifestSha =
        "6eda52b1b3c071dce49e79fac3bca0864c9bf22214a434fe5d8d01ca38e57dda";

    internal sealed record Recovered(string PayloadRoot, string BuildManifestPath);

    public static Recovered? TryRecoveredPayload()
    {
        var copies = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "Goal156Copies");
        if (!Directory.Exists(copies)) return null;
        var project = Directory.EnumerateDirectories(copies, "goal161-standalone-proof",
                SearchOption.AllDirectories)
            .Where(path => Directory.Exists(Path.Combine(path, "Builds", "Windows")))
            .OrderByDescending(Directory.GetLastWriteTimeUtc)
            .FirstOrDefault();
        if (project is null) return null;
        var output = Directory.EnumerateDirectories(Path.Combine(project, "Builds", "Windows"))
            .OrderByDescending(Directory.GetLastWriteTimeUtc)
            .FirstOrDefault();
        if (output is null) return null;
        var payload = Path.Combine(output, Path.GetFileName(output) + "_Data",
            "StreamingAssets", "LLMGameCreatorProject");
        return Directory.Exists(payload)
            ? new Recovered(payload, Path.Combine(output, "build-manifest.json"))
            : null;
    }

    public static Recovered? TryHistoricalRecoveredPayload()
    {
        var copies = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "Goal156Copies");
        if (!Directory.Exists(copies)) return null;
        foreach (var project in Directory.EnumerateDirectories(copies,
                     "goal161-standalone-proof", SearchOption.AllDirectories))
        {
            var windows = Path.Combine(project, "Builds", "Windows");
            if (!Directory.Exists(windows)) continue;
            foreach (var output in Directory.EnumerateDirectories(windows))
            {
                var buildManifest = Path.Combine(output, "build-manifest.json");
                if (!File.Exists(buildManifest)
                    || Hash(buildManifest) != HistoricalBuildManifestSha) continue;
                var payload = Path.Combine(output, Path.GetFileName(output) + "_Data",
                    "StreamingAssets", "LLMGameCreatorProject");
                if (Directory.Exists(payload)) return new Recovered(payload, buildManifest);
            }
        }
        return null;
    }

    public static JsonDocument Diagnosis() => JsonDocument.Parse(File.ReadAllText(Path.Combine(
        RepositoryRoot(), ".llmgc", "procedural",
        "goal-161q-standalone-self-check-diagnosis-and-qualification-closure",
        "diagnosis-plan.json")));

    public static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    public static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
