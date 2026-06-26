using System.Text.Json;
using LLMGameCreator.Application.Design.UnityGeneratedScene;
using LLMGameCreator.Application.Design.UnityRuntimeState;
using LLMGameCreator.Tests.Application.Assets;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnityRuntimeState;

[Collection("UnityAlphaProductSmoke")]
public sealed class UnityRuntimeStateLoopAcceptanceTests
{
    [Fact]
    public async Task BuildsDeterministicRuntimeStateLoopArtifacts()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(repoRoot, "samples", "content-generation-packs"), temp.Path);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(temp.Path, Path.Combine(repoRoot, "samples", "minimum-asset-pipeline"), content);
        var service = new UnityRuntimeStateLoopAcceptanceService();

        var first = service.BuildFromAcceptedEvidence(
            temp.Path,
            content,
            assets,
            new UnityRuntimeStateLoopOptions { RepositoryRootPath = repoRoot });
        var second = service.BuildFromAcceptedEvidence(
            temp.Path,
            content,
            assets,
            new UnityRuntimeStateLoopOptions { RepositoryRootPath = repoRoot });
        var write = await service.WriteAsync(temp.Path, first);

        Assert.False(first.Report.Accepted);
        Assert.Equal(UnityRuntimeStateLoopAcceptanceService.FinalGate, first.Report.FinalStatus);
        Assert.Equal("unity_generated_scene_content_projection_verification passed", first.Report.PreviousAcceptedGate);
        Assert.Equal(["S130", "S131", "S132", "S133", "S134", "S135", "S136", "S137"], first.Report.CompletedSlices);
        Assert.Equal("unity-runtime-state-loop", first.Report.ProductSmokeRoute);
        Assert.Equal("frontier_survival", first.Report.SelectedStyleId);
        Assert.Equal(first.Report.StateLoopHash, second.Report.StateLoopHash);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(first.Report.SceneProjectionVerified, string.Join(Environment.NewLine, first.Report.Diagnostics.Select(item => item.Code)));
        Assert.True(first.Report.InvalidMatrix.Passed, string.Join(Environment.NewLine, first.Report.InvalidMatrix.Diagnostics.Select(item => item.Code)));
        Assert.True(first.Report.InvalidMatrix.ScenarioCount >= 20);
        Assert.False(first.Report.PublicGamePackageSchemaChanged);
        Assert.False(first.Report.ProjectFilesChanged);
        Assert.False(first.Report.GeneratorLibraryChanged);
        Assert.True(first.Report.NoExternalProviderLlmRagLuaMedia);
        Assert.True(File.Exists(write.StateJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));

        var roundTrip = JsonSerializer.Deserialize<UnityRuntimeStateLoopReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(roundTrip);
        Assert.Equal(UnityRuntimeStateLoopAcceptanceService.FinalGate, roundTrip!.ManualGate);
    }

    [Fact]
    public void StateLoopParserRequiresBeforeAfterAndCommandCorrelation()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(repoRoot, "samples", "content-generation-packs"), temp.Path);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(temp.Path, Path.Combine(repoRoot, "samples", "minimum-asset-pipeline"), content);
        var projection = UnityGeneratedSceneProjectionAcceptanceService.BuildProjection(
            new LLMGameCreator.Application.Design.AlphaBuild.AlphaRunnableBuildAcceptanceService()
                .BuildFromAcceptedEvidence(
                    temp.Path,
                    content,
                    assets,
                    new LLMGameCreator.Application.Design.AlphaBuild.AlphaRunnableBuildOptions
                    {
                        RepositoryRootPath = repoRoot,
                        RelativeOutputDirectoryOverride = UnityRuntimeStateLoopAcceptanceService.RelativeOutputDirectory
                    })
                .Report);

        var acceptedLines = UnityRuntimeStateLoopAcceptanceService.BuildExpectedStateLoopLines(projection);
        var accepted = UnityRuntimeStateLoopAcceptanceService.ValidateStateLoopLines(acceptedLines, projection);
        var noBeforeAfter = UnityRuntimeStateLoopAcceptanceService.ValidateStateLoopLines(
            acceptedLines.Where(line => !line.StartsWith("alpha_runtime.state.before.", StringComparison.Ordinal) &&
                                        !line.StartsWith("alpha_runtime.state.after.", StringComparison.Ordinal)),
            projection);
        var wrongTarget = UnityRuntimeStateLoopAcceptanceService.ValidateStateLoopLines(
            acceptedLines.Select(line => line.StartsWith("alpha_runtime.command_state_transition.2.target_id=", StringComparison.Ordinal)
                ? "alpha_runtime.command_state_transition.2.target_id=item/mismatch"
                : line),
            projection);

        Assert.True(accepted.RuntimeStateLoopVerified, string.Join(Environment.NewLine, accepted.Diagnostics.Select(item => item.Code)));
        Assert.True(accepted.StateTransitionTraceVerified);
        Assert.True(accepted.QuestStateVerified);
        Assert.True(accepted.DialogueStateVerified);
        Assert.True(accepted.InventoryStateVerified);
        Assert.True(accepted.EventStateVerified);
        Assert.True(accepted.FocusVerified);
        Assert.True(accepted.CommandStateTransitionCount >= 6);
        Assert.False(noBeforeAfter.RuntimeStateLoopVerified);
        Assert.False(wrongTarget.RuntimeStateLoopVerified);
        Assert.Contains(wrongTarget.Diagnostics, item => item.Code == "unity_runtime_state.transition.command_mismatch" || item.Code == "unity_runtime_state.transition.state_command_correlation_failed");
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
