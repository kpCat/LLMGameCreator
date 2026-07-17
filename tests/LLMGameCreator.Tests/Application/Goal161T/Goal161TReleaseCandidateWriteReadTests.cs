using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Tests.Application.Goal155;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161T;

public sealed class Goal161TReleaseCandidateWriteReadTests
{
    [Fact]
    public void Behavioral_actual_payload_missing_accepted_fact_is_rejected() { using var fixture = Goal155RcFixture.Create("missing-accepted-fact"); var model = JsonNode.Parse(File.ReadAllText(fixture.PlayerAdapterModelPath))!.AsObject(); model["humanReviewFacts"] = new JsonArray(new JsonObject { ["label"] = "Release Candidate", ["value"] = "готов" }); File.WriteAllText(fixture.PlayerAdapterModelPath, model.ToJsonString()); var exception = Assert.Throws<InvalidOperationException>(() => fixture.Write()); Assert.Equal("rc.write.actual_payload_missing_accepted_fact", exception.Message); }

    [Fact]
    public void Behavioral_actual_payload_missing_ready_fact_is_rejected() { using var fixture = Goal155RcFixture.Create("missing-ready-fact"); var model = JsonNode.Parse(File.ReadAllText(fixture.PlayerAdapterModelPath))!.AsObject(); var facts = model["humanReviewFacts"]!.AsArray(); foreach (var fact in facts.OfType<JsonObject>().Where(fact => (string?)fact["label"] == "Release Candidate")) fact["value"] = "не готов"; File.WriteAllText(fixture.PlayerAdapterModelPath, model.ToJsonString()); var exception = Assert.Throws<InvalidOperationException>(() => fixture.Write()); Assert.Equal("rc.write.actual_payload_missing_ready_fact", exception.Message); }

    [Fact]
    public void Behavioral_invalid_existing_pointer_does_not_fall_back_to_legacy_payload() { using var fixture = Goal161TFixture.Create(); var legacy = Path.Combine(fixture.Root.Project, "Builds", "Windows", "package", "package_Data", "StreamingAssets", "LLMGameCreatorProject"); Directory.CreateDirectory(legacy); File.WriteAllText(Path.Combine(legacy, "project-manifest.json"), "{}"); File.WriteAllText(Path.Combine(legacy, "player-adapter-model.json"), "{}"); File.Delete(Path.Combine(fixture.Location.RunOutputFolder, "g.exe")); var evidence = fixture.Evidence.InspectForRead(fixture.Root.Project, "package"); Assert.Equal("immutable_current_pointer", evidence.SourceKind); Assert.Contains("rc.payload.current_pointer_invalid", evidence.Diagnostics[0]); }

    [Fact]
    public void Behavioral_legacy_project_local_payload_remains_readable() { using var fixture = Goal155RcFixture.Create("legacy-readable"); var evidence = new ProjectStandalonePayloadEvidenceService().InspectForRead(fixture.Project, fixture.Identity.PackageId); Assert.True(evidence.Passed, string.Join(";", evidence.Diagnostics)); Assert.Equal("legacy_project_local_output", evidence.SourceKind); }

    [Fact]
    public void Behavioral_portable_no_output_release_candidate_read_remains_current() { using var fixture = Goal155RcFixture.Create("portable-no-output"); fixture.Write(); var payload = Path.GetDirectoryName(fixture.PlayerAdapterModelPath)!; Directory.Delete(Path.GetFullPath(Path.Combine(payload, "..", "..", "..", "..", "..")), true); var read = fixture.Read(); Assert.Equal("CURRENT", read.ConfigurationStatus); Assert.NotNull(read.Record); }

    [Fact]
    public void Behavioral_portable_tampered_package_is_rejected() { using var fixture = Goal155RcFixture.Create("portable-tampered-package"); fixture.Write(); File.AppendAllText(Path.Combine(fixture.Project, "package.json"), "tampered"); var read = fixture.Read(); Assert.Null(read.Record); Assert.Contains("rc.read.current_package_hash_mismatch", read.Diagnostics); }

    [Fact]
    public void Behavioral_current_standalone_history_exact_row_is_selected() { using var fixture = Goal161TFixture.Create(); Goal161TTestKit.WriteStandaloneHistory(fixture.Root.Project, fixture.Standalone); var service = new ProjectStandaloneBuildService(Directory.GetCurrentDirectory(), fixture.Root.Locations); var result = service.LoadCurrentQualifiedResult(fixture.Root.Project, "package"); Assert.True(result.Passed, result.Diagnostics); Assert.Equal(fixture.Standalone.AttemptId, result.Result!.AttemptId); }

    [Fact]
    public void Behavioral_missing_standalone_history_is_rejected() { using var fixture = Goal161TFixture.Create(); var service = new ProjectStandaloneBuildService(Directory.GetCurrentDirectory(), fixture.Root.Locations); var result = service.LoadCurrentQualifiedResult(fixture.Root.Project, "package"); Assert.False(result.Passed); Assert.Equal("standalone.current_history_missing", result.Diagnostics); }

    [Fact]
    public void Behavioral_ambiguous_standalone_history_is_rejected() { using var fixture = Goal161TFixture.Create(); Goal161TTestKit.WriteStandaloneHistory(fixture.Root.Project, fixture.Standalone, fixture.Standalone); var service = new ProjectStandaloneBuildService(Directory.GetCurrentDirectory(), fixture.Root.Locations); var result = service.LoadCurrentQualifiedResult(fixture.Root.Project, "package"); Assert.False(result.Passed); Assert.Contains("standalone.current_history_", result.Diagnostics); }
}
internal static class Goal161TTestKit
{
    public static void WriteStandaloneHistory(string projectFolder, params ProjectStandaloneBuildResult[] rows)
    {
        var path = Path.Combine(projectFolder, ".llmgc", ProjectStandaloneBuildVocabulary.HistoryRelativePath.Split('/').Last());
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static string TreeHash(string root)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        using var buffer = new MemoryStream();
        foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                     .OrderBy(path => Path.GetRelativePath(root, path), StringComparer.OrdinalIgnoreCase))
        {
            var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
            var name = System.Text.Encoding.UTF8.GetBytes(relative + "\n");
            buffer.Write(name);
            buffer.Write(File.ReadAllBytes(path));
        }
        return Convert.ToHexString(sha.ComputeHash(buffer.ToArray())).ToLowerInvariant();
    }

    public static string HashMatchingFiles(string root, params string[] fragments)
    {
        if (!Directory.Exists(root)) return string.Empty;
        var paths = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => fragments.Any(fragment => path.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToList();
        if (paths.Count == 0) return string.Empty;
        return TreeHashForPaths(root, paths);
    }

    private static string TreeHashForPaths(string root, IReadOnlyList<string> paths)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        using var buffer = new MemoryStream();
        foreach (var path in paths)
        {
            buffer.Write(System.Text.Encoding.UTF8.GetBytes(Path.GetRelativePath(root, path).Replace('\\', '/') + "\n"));
            buffer.Write(File.ReadAllBytes(path));
        }
        return Convert.ToHexString(sha.ComputeHash(buffer.ToArray())).ToLowerInvariant();
    }
}
