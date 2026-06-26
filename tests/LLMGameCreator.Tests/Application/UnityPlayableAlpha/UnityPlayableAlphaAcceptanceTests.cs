using System.Text.Json;
using LLMGameCreator.Application.Design.AlphaBuild;
using LLMGameCreator.Application.Design.UnityPlayableAlpha;
using LLMGameCreator.Tests.Application.Assets;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnityPlayableAlpha;

[Collection("UnityAlphaProductSmoke")]
public sealed class UnityPlayableAlphaAcceptanceTests
{
    [Fact]
    public async Task BuildsGoal014ArtifactsFromAcceptedGoal013Evidence()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(repoRoot, "samples", "content-generation-packs"), temp.Path);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(temp.Path, Path.Combine(repoRoot, "samples", "minimum-asset-pipeline"), content);
        var service = new UnityPlayableAlphaAcceptanceService();

        var result = service.BuildFromAcceptedEvidence(
            temp.Path,
            content,
            assets,
            new UnityPlayableAlphaOptions { RepositoryRootPath = repoRoot });
        var write = await service.WriteAsync(temp.Path, result);

        Assert.False(result.Report.Accepted);
        Assert.Equal(UnityPlayableAlphaAcceptanceService.FinalGate, result.Report.FinalStatus);
        Assert.Equal("alpha_runnable_windows_build_verification passed", result.Report.PreviousAcceptedGate);
        Assert.Equal(["S114", "S115", "S116", "S117", "S118", "S119", "S120", "S121"], result.Report.CompletedSlices);
        Assert.Equal("unity-playable-alpha", result.Report.ProductSmokeRoute);
        Assert.StartsWith(UnityPlayableAlphaAcceptanceService.RelativeOutputDirectory, result.Report.DeterministicReportRelativePath, StringComparison.Ordinal);
        Assert.Equal("frontier_survival", result.Report.SelectedStyleId);
        Assert.NotEmpty(result.Report.SelectedPackageId);
        Assert.True(result.Report.FirewallSafeBuild.StaticChecksPassed, string.Join(Environment.NewLine, result.Report.FirewallSafeBuild.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(result.Report.FirewallSafeBuild.FirewallSafeBuildVerified);
        Assert.False(result.Report.VisiblePresentationVerified);
        Assert.False(result.Report.MovementVerified);
        Assert.False(result.Report.InteractionVerified);
        Assert.True(result.Report.InvalidMatrix.Passed);
        Assert.True(result.Report.InvalidMatrix.ScenarioCount >= 20);
        Assert.False(result.Report.RuntimePreviewDependency);
        Assert.False(result.Report.PublicGamePackageSchemaChanged);
        Assert.False(result.Report.ProjectFilesChanged);
        Assert.False(result.Report.GeneratorLibraryChanged);
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        Assert.True(Directory.Exists(write.StagingDirectoryPath));
        Assert.True(Directory.Exists(write.BuildDirectoryPath));
        Assert.DoesNotContain(@"C:\", result.VerificationMarkdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/", result.VerificationMarkdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"Users\", result.VerificationMarkdown, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("- Unity command: (omitted; local machine paths are not part of compact deterministic root artifacts)", result.VerificationMarkdown);

        var roundTrip = JsonSerializer.Deserialize<UnityPlayableAlphaReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(roundTrip);
        Assert.Equal(UnityPlayableAlphaAcceptanceService.FinalGate, roundTrip!.ManualGate);
    }

    [Fact]
    public void PresentationLogParserAcceptsVisibleMovementAndInteractionProof()
    {
        using var temp = new TempDirectory();
        var candidate = BuildCandidate();
        var logPath = Path.Combine(temp.Path, "alpha-player-play-loop.log");
        File.WriteAllLines(logPath, BuildPlayableLog(candidate));

        var proof = UnityPlayableAlphaAcceptanceService.ValidatePresentationLog(logPath, candidate);

        Assert.True(proof.VisiblePresentationVerified, string.Join(Environment.NewLine, proof.Diagnostics.Select(item => item.Code)));
        Assert.True(proof.MovementVerified, string.Join(Environment.NewLine, proof.Diagnostics.Select(item => item.Code)));
        Assert.True(proof.InteractionVerified, string.Join(Environment.NewLine, proof.Diagnostics.Select(item => item.Code)));
        Assert.True(proof.PlayLoopVerified);
        Assert.Equal("1,1", proof.InitialPosition);
        Assert.Equal("2,2", proof.FinalMovementPosition);
        Assert.Equal("0,2", proof.BlockedMovementPosition);
        Assert.StartsWith("npc:", proof.FocusSelection, StringComparison.Ordinal);
    }

    [Fact]
    public void FirewallStaticChecksRejectDevelopmentProfilerAndDebugFlags()
    {
        var rejected = UnityPlayableAlphaAcceptanceService.ValidateFirewallSafeBuildScript(
            "options = BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging; AutoconnectProfiler(); scriptDebugging = true;");
        var accepted = UnityPlayableAlphaAcceptanceService.ValidateFirewallSafeBuildScript(
            "var buildOptions = new BuildPlayerOptions { options = BuildOptions.None };");

        Assert.False(rejected.FirewallSafeBuildVerified);
        Assert.Contains(rejected.Diagnostics, item => item.Code == "unity_playable_alpha.firewall.development_build_flag");
        Assert.Contains(rejected.Diagnostics, item => item.Code == "unity_playable_alpha.firewall.profiler_build_flag");
        Assert.Contains(rejected.Diagnostics, item => item.Code == "unity_playable_alpha.firewall.debug_build_flag");
        Assert.True(accepted.FirewallSafeBuildVerified);
        Assert.Equal("BuildOptions.None", accepted.BuildOptions);
    }

    private static AlphaBuildCandidate BuildCandidate() =>
        new()
        {
            CommandHints =
            [
                new AlphaBuildCommandHint { CommandId = "cmd-001", CommandType = "quest/start" },
                new AlphaBuildCommandHint { CommandId = "cmd-002", CommandType = "dialogue/open" },
                new AlphaBuildCommandHint { CommandId = "cmd-003", CommandType = "dialogue/choose" },
                new AlphaBuildCommandHint { CommandId = "cmd-004", CommandType = "loot/roll" },
                new AlphaBuildCommandHint { CommandId = "cmd-005", CommandType = "event/add_item" }
            ]
        };

    private static IEnumerable<string> BuildPlayableLog(AlphaBuildCandidate candidate)
    {
        var lines = new List<string>
        {
            "alpha_runtime.visible_presentation_initialized=true",
            "alpha_runtime.visible_component.map=true",
            "alpha_runtime.visible_component.player_marker=true",
            "alpha_runtime.visible_component.npc_marker=true",
            "alpha_runtime.visible_component.item_marker=true",
            "alpha_runtime.visible_component.status_panel=true",
            "alpha_runtime.visible_component.command_log=true",
            "alpha_runtime.movement.initial_position=1,1",
            "alpha_runtime.movement.step.0.valid=true",
            "alpha_runtime.movement.step.0.position=2,1",
            "alpha_runtime.movement.step.1.valid=true",
            "alpha_runtime.movement.step.1.position=2,2",
            "alpha_runtime.movement.blocked.valid=false",
            "alpha_runtime.movement.blocked.position=0,2",
            "alpha_runtime.focus.selected=npc:dialogue/001",
            "alpha_runtime.commands_executed=5"
        };
        for (var index = 0; index < candidate.CommandHints.Count; index++)
        {
            lines.Add("alpha_runtime.command_executed." + index + ".id=" + candidate.CommandHints[index].CommandId);
            lines.Add("alpha_runtime.command_executed." + index + ".type=" + candidate.CommandHints[index].CommandType);
        }

        return lines;
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
