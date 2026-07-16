using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161S;

public sealed class Goal161SRunLocationTests
{
    [Fact] public void Behavioral_run_path_is_deterministic_and_safe() { using var root = new Goal161STempRoot(); var one = root.Resolve("a1b2c3d4e5f6"); var two = root.Resolve("a1b2c3d4e5f6"); Assert.Equal(one.RunOutputFolder, two.RunOutputFolder); Assert.Equal("r-a1b2c3d4e5f6", one.RunDirectoryName); }
    [Fact] public void Behavioral_different_attempts_have_different_runs() { using var root = new Goal161STempRoot(); Assert.NotEqual(root.Resolve("a1b2c3d4e5f6").RunOutputFolder, root.Resolve("b1b2c3d4e5f6").RunOutputFolder); }
    [Fact] public void Behavioral_current_pointer_path_is_project_deterministic() { using var root = new Goal161STempRoot(); Assert.Equal(root.Resolve("a1b2c3d4e5f6").CurrentPointerPath, root.Resolve("b1b2c3d4e5f6").CurrentPointerPath); }
    [Fact] public void Behavioral_run_is_under_confined_runs_root() { using var root = new Goal161STempRoot(); var location = root.Resolve("a1b2c3d4e5f6"); Assert.StartsWith(location.RunsFolder + Path.DirectorySeparatorChar, location.RunOutputFolder, StringComparison.OrdinalIgnoreCase); }
    [Fact] public void Behavioral_unsafe_attempt_is_rejected() { using var root = new Goal161STempRoot(); Assert.Throws<InvalidOperationException>(() => root.Locations.Resolve(root.Project, "package", "../unsafe")); }
    [Fact] public void Behavioral_path_budget_includes_pointer_and_run_status() { using var root = new Goal161STempRoot(); var location = root.Resolve("a1b2c3d4e5f6"); Goal161STempRoot.WriteGreenRun(root.Locations, location); var result = root.Locations.ValidatePlayerPathBudget(location.RunOutputFolder, Path.Combine(location.RunOutputFolder, "smoke-markers.log"), Path.Combine(location.RunOutputFolder, "Player.log"), location.CurrentPointerPath, Path.Combine(location.RunOutputFolder, "run-status.json")); Assert.True(result.Passed, string.Join("\n", result.Diagnostics)); }
    [Fact] public void Behavioral_long_project_uses_short_operational_run() { var work = Path.Combine(Path.GetTempPath(), new string('p', 140)); var output = Path.Combine(Path.GetTempPath(), "o-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(work); try { var service = new ProjectStandaloneOutputLocationService(output); var location = service.Resolve(work, "package", "a1b2c3d4e5f6"); Goal161STempRoot.WriteGreenRun(service, location); var result = service.ValidatePlayerPathBudget(location.RunOutputFolder, Path.Combine(location.RunOutputFolder, "smoke-markers.log"), Path.Combine(location.RunOutputFolder, "Player.log"), location.CurrentPointerPath, Path.Combine(location.RunOutputFolder, "run-status.json")); Assert.True(result.MaximumAbsolutePathLength <= 240); } finally { if (Directory.Exists(work)) Directory.Delete(work, true); if (Directory.Exists(output)) Directory.Delete(output, true); } }
}

internal sealed class Goal161STempRoot : IDisposable
{
    public Goal161STempRoot(string? suffix = null) { Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "llmgc-161s-" + (suffix ?? Guid.NewGuid().ToString("N"))); Project = System.IO.Path.Combine(Path, "project"); Directory.CreateDirectory(Project); Locations = new ProjectStandaloneOutputLocationService(System.IO.Path.Combine(Path, "o")); }
    public string Path { get; } public string Project { get; } public ProjectStandaloneOutputLocationService Locations { get; }
    public ProjectStandaloneOutputLocation Resolve(string attempt) => Locations.Resolve(Project, "package", attempt);
    public static void WriteGreenRun(ProjectStandaloneOutputLocationService service, ProjectStandaloneOutputLocation location)
    {
        var run = location.RunOutputFolder; var payload = System.IO.Path.Combine(run, "g_Data", "StreamingAssets", "LLMGameCreatorProject"); Directory.CreateDirectory(payload); File.WriteAllText(System.IO.Path.Combine(run, "g.exe"), "exe"); File.WriteAllText(System.IO.Path.Combine(run, "UnityPlayer.dll"), "dll"); Directory.CreateDirectory(System.IO.Path.Combine(run, "MonoBleedingEdge"));
        var package = "{\"name\":\"game\"}"; var packageHash = HashText(package);
        File.WriteAllText(System.IO.Path.Combine(payload, "game-package.json"), package);
        File.WriteAllText(System.IO.Path.Combine(payload, "project-manifest.json"), JsonSerializer.Serialize(new { schemaVersion = "llmgc_project_standalone_v2", projectPackageId = "package", projectTitle = "Game", projectVersion = "1", packageSha256 = packageHash, compositionPackageSha256 = "composition", finalStateHash = "final", selectedModuleIds = Array.Empty<string>(), effectiveParameters = Array.Empty<object>(), requiredMechanicCount = 0, selectedOptionalMechanicCount = 0, activeMechanicCount = 0, configuredParameterCount = 0, runtimeAuthority = true, unityGameplayTruth = false, projectionOnly = false }));
        File.WriteAllText(System.IO.Path.Combine(payload, "player-adapter-model.json"), "{\"schemaVersion\":\"llmgc_player_adapter_model_v2\",\"equipmentDamageBonus\":0,\"totalAdditionalDamage\":0,\"humanReviewFacts\":[{\"label\":\"fact\",\"value\":\"value\"}]}");
        File.WriteAllText(System.IO.Path.Combine(payload, "player-adapter-frames.json"), "[{\"index\":0,\"title\":\"frame\",\"category\":\"test\",\"stateHash\":\"final\"}]"); File.WriteAllText(System.IO.Path.Combine(payload, "standalone-launch.json"), "{\"schemaVersion\":\"llmgc_standalone_launch_v2\"}");
        File.WriteAllText(System.IO.Path.Combine(run, "build-manifest.json"), "{\"schemaVersion\":\"llmgc_project_standalone_build_v1\"}"); File.WriteAllText(System.IO.Path.Combine(run, "smoke-markers.log"), string.Join("\n", new[] { "LLMGC_PROJECT_STANDALONE_LOAD_PASS", "LLMGC_PROJECT_STANDALONE_INTEGRITY_PASS", "LLMGC_PROJECT_STANDALONE_NAVIGATION_PASS", "LLMGC_PROJECT_STANDALONE_RUNTIME_AUTHORITY_PASS", "LLMGC_PROJECT_STANDALONE_SMOKE_PASS" })); File.WriteAllText(System.IO.Path.Combine(run, "Player.log"), "green");
        service.WriteRunStatus(location, new ProjectStandaloneRunStatus { Status = "GREEN", AttemptId = location.RunDirectoryName[2..], PackageSha256 = packageHash, FinalStateHash = "final", PayloadSelfCheckPassed = true, LegacyParserCompatibilityPassed = true, MaximumPlayerPathLength = 100, PlayerPathBudgetLimit = 240, SmokeExitCode = 0, SmokeMarkersPassed = true, PlayerLogPresent = true, HostCacheKey = "host", HostReused = true });
    }
    public static ProjectStandaloneCurrentPointer Pointer(ProjectStandaloneOutputLocation location) { var run = location.RunOutputFolder; var package = HashText(File.ReadAllText(System.IO.Path.Combine(run, "g_Data", "StreamingAssets", "LLMGameCreatorProject", "game-package.json"))); return new ProjectStandaloneCurrentPointer { ProjectToken = location.ProjectToken, RunDirectoryName = location.RunDirectoryName, PackageSha256 = package, CompositionPackageSha256 = "composition", FinalStateHash = "final", HostCacheKey = "host", PayloadSelfCheckSha256 = "s", SmokeMarkerSha256 = "m", PlayerLogSha256 = "p", SmokeExitCode = 0, PublishedAttemptId = location.RunDirectoryName[2..] }; }
    private static string HashText(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, true); }
}
