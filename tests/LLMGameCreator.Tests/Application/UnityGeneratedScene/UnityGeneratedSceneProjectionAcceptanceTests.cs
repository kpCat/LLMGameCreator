using System.Text.Json;
using LLMGameCreator.Application.Design.UnityGeneratedScene;
using LLMGameCreator.Tests.Application.Assets;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnityGeneratedScene;

[Collection("UnityAlphaProductSmoke")]
public sealed class UnityGeneratedSceneProjectionAcceptanceTests
{
    [Fact]
    public async Task BuildsDeterministicProjectionArtifactsFromAcceptedAlphaEvidence()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(repoRoot, "samples", "content-generation-packs"), temp.Path);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(temp.Path, Path.Combine(repoRoot, "samples", "minimum-asset-pipeline"), content);
        var service = new UnityGeneratedSceneProjectionAcceptanceService();

        var first = service.BuildFromAcceptedEvidence(
            temp.Path,
            content,
            assets,
            new UnityGeneratedSceneProjectionOptions { RepositoryRootPath = repoRoot });
        var second = service.BuildFromAcceptedEvidence(
            temp.Path,
            content,
            assets,
            new UnityGeneratedSceneProjectionOptions { RepositoryRootPath = repoRoot });
        var write = await service.WriteAsync(temp.Path, first);

        Assert.False(first.Report.Accepted);
        Assert.Equal(UnityGeneratedSceneProjectionAcceptanceService.FinalGate, first.Report.FinalStatus);
        Assert.Equal("unity_playable_presentation_firewall_safe_build_verification passed", first.Report.PreviousAcceptedGate);
        Assert.Equal(["S122", "S123", "S124", "S125", "S126", "S127", "S128", "S129"], first.Report.CompletedSlices);
        Assert.Equal("unity-generated-scene-projection", first.Report.ProductSmokeRoute);
        Assert.Equal("frontier_survival", first.Report.SelectedStyleId);
        Assert.Equal(first.Report.ProjectionHash, second.Report.ProjectionHash);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(first.Report.SceneProjectionVerified, string.Join(Environment.NewLine, first.Report.Diagnostics.Select(item => item.Code)));
        Assert.True(first.Report.SceneNodesResolved);
        Assert.True(first.Report.GeneratedIdBindingVerified);
        Assert.True(first.Report.AssetBindingVerified);
        Assert.True(first.Report.InvalidMatrix.Passed);
        Assert.True(first.Report.InvalidMatrix.ScenarioCount >= 20);
        Assert.False(first.Report.PublicGamePackageSchemaChanged);
        Assert.False(first.Report.ProjectFilesChanged);
        Assert.False(first.Report.GeneratorLibraryChanged);
        Assert.True(File.Exists(write.ProjectionJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));

        var roundTrip = JsonSerializer.Deserialize<UnityGeneratedSceneProjectionReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(roundTrip);
        Assert.Equal(UnityGeneratedSceneProjectionAcceptanceService.FinalGate, roundTrip!.ManualGate);
    }

    [Fact]
    public void ProjectionCarriesSelectedIdsAndAvoidsGoal014FixedMarkerPositions()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(repoRoot, "samples", "content-generation-packs"), temp.Path);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(temp.Path, Path.Combine(repoRoot, "samples", "minimum-asset-pipeline"), content);
        var result = new UnityGeneratedSceneProjectionAcceptanceService().BuildFromAcceptedEvidence(
            temp.Path,
            content,
            assets,
            new UnityGeneratedSceneProjectionOptions { RepositoryRootPath = repoRoot });

        var projection = result.Report.Projection;
        Assert.Equal(result.Report.SelectedPackageId, projection.SelectedPackageId);
        Assert.Equal(result.Report.SelectedThreadId, projection.SelectedThreadId);
        Assert.Contains(projection.Nodes, node => node.NodeKind == "map" && node.SourceGeneratedId == projection.SelectedMapId);
        Assert.Contains(projection.Nodes, node => node.NodeKind == "npc" && node.SourceGeneratedId == projection.SelectedNpcId);
        Assert.Contains(projection.Nodes, node => node.NodeKind == "item" && node.SourceGeneratedId == projection.SelectedItemId);
        Assert.Contains(projection.Nodes, node => node.NodeKind == "command_status" && node.SourceGeneratedId == projection.CommandHints[0].CommandId);
        Assert.DoesNotContain(projection.Nodes, node => node.NodeKind == "npc" && node.X == 4 && node.Y == 1);
        Assert.DoesNotContain(projection.Nodes, node => node.NodeKind == "item" && node.X == 5 && node.Y == 3);
        Assert.DoesNotContain(projection.Nodes, node => node.NodeKind == "quest_event" && node.X == 2 && node.Y == 3);
    }

    [Fact]
    public void PlayLoopParserRequiresProjectionLoadNodesAndGeneratedCommandTargets()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(repoRoot, "samples", "content-generation-packs"), temp.Path);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(temp.Path, Path.Combine(repoRoot, "samples", "minimum-asset-pipeline"), content);
        var projection = new UnityGeneratedSceneProjectionAcceptanceService().BuildFromAcceptedEvidence(
            temp.Path,
            content,
            assets,
            new UnityGeneratedSceneProjectionOptions { RepositoryRootPath = repoRoot }).Report.Projection;

        var acceptedLines = BuildPlayLoopLines(projection);
        var accepted = UnityGeneratedSceneProjectionAcceptanceService.ValidatePlayLoopLines(acceptedLines, projection);
        var rejected = UnityGeneratedSceneProjectionAcceptanceService.ValidatePlayLoopLines(
            acceptedLines.Where(line => !line.StartsWith("alpha_runtime.scene_projection_loaded=", StringComparison.Ordinal)),
            projection);

        Assert.True(accepted.PlayLoopVerified, string.Join(Environment.NewLine, accepted.Diagnostics.Select(item => item.Code)));
        Assert.True(accepted.MovementVerified);
        Assert.True(accepted.InteractionVerified);
        Assert.False(rejected.PlayLoopVerified);
        Assert.Contains(rejected.Diagnostics, item => item.Code == "unity_generated_scene.play_loop.projection_not_loaded");
    }

    private static IEnumerable<string> BuildPlayLoopLines(UnityGeneratedSceneProjection projection)
    {
        var lines = new List<string>
        {
            "alpha_runtime.scene_projection_loaded=true",
            "alpha_runtime.scene_node_resolved.map=true",
            "alpha_runtime.scene_node_resolved.player=true",
            "alpha_runtime.scene_node_resolved.npc=true",
            "alpha_runtime.scene_node_resolved.item=true",
            "alpha_runtime.scene_node_resolved.quest_event=true",
            "alpha_runtime.scene_node_resolved.command_status=true",
            "alpha_runtime.movement.step.0.valid=true",
            "alpha_runtime.movement.step.1.valid=true",
            "alpha_runtime.movement.blocked.valid=false",
            "alpha_runtime.focus.selected_node_id=" + projection.Nodes.First(node => node.NodeKind == "npc").NodeId,
            "alpha_runtime.focus.selected=npc:" + projection.SelectedNpcId,
            "alpha_runtime.commands_executed=" + projection.CommandHints.Count,
            "alpha_runtime.state_transition.quest_start=true",
            "alpha_runtime.state_transition.dialogue_open=true",
            "alpha_runtime.state_transition.item_or_loot=true",
            "alpha_runtime.state_transition.event_application=true"
        };

        for (var index = 0; index < projection.CommandHints.Count; index++)
        {
            var command = projection.CommandHints[index];
            lines.Add("alpha_runtime.command_executed." + index + ".id=" + command.CommandId);
            lines.Add("alpha_runtime.command_executed." + index + ".type=" + command.CommandType);
            lines.Add("alpha_runtime.command_executed." + index + ".target_id=" + command.TargetId);
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
