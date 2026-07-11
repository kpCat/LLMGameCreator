using LLMGameCreator.Application.Design.ProductLineInteractiveSessionMatrix;
using LLMGameCreator.Runtime;
using System.Text.Json.Nodes;
using Xunit;

namespace LLMGameCreator.Tests.Application.ProductLineInteractiveSessionMatrix;

[Collection("UnityAlphaProductSmoke")]
public sealed class ProductLineInteractiveSessionMatrixTests
{
    [Fact]
    public void Discovery_reads_and_validates_all_goal142_candidates_in_deterministic_order()
    {
        var root = TestRepositoryRoot.Find();
        var result = new Goal142CandidateDiscovery().Discover(
            root,
            ProductLineInteractiveSessionMatrixVocabulary.Goal142Root);

        Assert.Equal(4, result.Candidates.Count);
        Assert.Equal(
            result.Candidates.Select(candidate => candidate.Candidate.CandidateId).OrderBy(id => id, StringComparer.Ordinal),
            result.Candidates.Select(candidate => candidate.Candidate.CandidateId));
        Assert.Equal("minimal-map-game-exploration-resource-focus", result.DefaultSelectedCandidateId);
        Assert.Equal(4, result.Candidates.Select(candidate => candidate.Candidate.PackageSha256).Distinct().Count());
    }

    [Fact]
    public void Selection_rejects_unknown_failed_and_duplicate_metadata()
    {
        var candidates = new Goal142CandidateDiscovery().Discover(
            TestRepositoryRoot.Find(),
            ProductLineInteractiveSessionMatrixVocabulary.Goal142Root).Candidates;
        Assert.Throws<InvalidOperationException>(() =>
            ProductLineInteractiveSessionMatrixService.ResolveSelectable(candidates, "unknown"));
        var failed = candidates.Select(candidate => candidate with
        {
            Candidate = candidate.Candidate with { Passed = false }
        }).ToList();
        Assert.Throws<InvalidOperationException>(() =>
            ProductLineInteractiveSessionMatrixService.ResolveSelectable(failed, failed[0].Candidate.CandidateId));
        Assert.Throws<InvalidOperationException>(() =>
            Goal142CandidateDiscovery.AssertNoDuplicate(new[] { "same", "same" }, "candidate ID"));
        Assert.Throws<InvalidOperationException>(() =>
            Goal142CandidateDiscovery.AssertNoDuplicate(new[] { "same/path", "same/path" }, "package path"));
    }

    [Fact]
    public void Selection_controller_preserves_candidate_and_uses_selected_package_after_matrix_refresh()
    {
        var root = TestRepositoryRoot.Find();
        var controller = new ProductLineInteractiveSessionSelectionController(
            SelectedRuntimeVariantInteractiveSessionService.CreateDefault());

        controller.LoadCandidateMatrix(root);
        Assert.Equal("minimal-map-game-exploration-resource-focus", controller.SelectedCandidateId);

        const string combatId = "minimal-map-game-combat-focus";
        var combat = controller.Candidates.Single(candidate => candidate.CandidateId == combatId);
        controller.SelectCandidate(combatId);
        var session = controller.StartSelected();

        Assert.Equal(combatId, controller.SelectedCandidateId);
        Assert.Equal(combatId, session.CandidateId);
        Assert.Equal(combat.PackageSha256, session.PackageSha256);
        Assert.DoesNotContain("exploration", session.CandidateId, StringComparison.Ordinal);
        Assert.DoesNotContain("balanced-baseline", session.CandidateId, StringComparison.Ordinal);

        _ = controller.Candidates.ToList();
        controller.LoadCandidateMatrix(root);

        Assert.Equal(combatId, controller.SelectedCandidateId);
        Assert.Same(session, controller.Session);
        Assert.Equal(combat.PackageSha256, controller.Session!.PackageSha256);
    }

    [Fact]
    public void Candidate_change_clears_previous_session_checkpoint_action_and_replay_state()
    {
        var controller = new ProductLineInteractiveSessionSelectionController(
            SelectedRuntimeVariantInteractiveSessionService.CreateDefault());
        controller.LoadCandidateMatrix(TestRepositoryRoot.Find());
        var explorationSession = controller.StartSelected();
        controller.ExecuteSelectedAction("start_runtime");
        controller.SaveCheckpoint();
        controller.ReloadCheckpoint();

        Assert.NotNull(explorationSession);
        Assert.NotNull(controller.Session);
        Assert.NotNull(controller.Checkpoint);
        Assert.NotNull(controller.LastActionResult);
        Assert.NotNull(controller.LastReplayResult);

        const string combatId = "minimal-map-game-combat-focus";
        var combat = controller.Candidates.Single(candidate => candidate.CandidateId == combatId);
        controller.SelectCandidate(combatId);

        Assert.Null(controller.Session);
        Assert.Null(controller.Checkpoint);
        Assert.Null(controller.LastActionResult);
        Assert.Null(controller.LastReplayResult);

        var combatSession = controller.StartSelected();
        Assert.Equal(combatId, combatSession.CandidateId);
        Assert.Equal(combat.PackageSha256, combatSession.PackageSha256);
    }

    [Fact]
    public void Discovery_rejects_hash_metadata_and_path_tampering()
    {
        using var repository = TamperRepository.Create(TestRepositoryRoot.Find());
        var discovery = new Goal142CandidateDiscovery();
        File.AppendAllText(repository.FirstPackagePath, " ");
        Assert.Contains("SHA mismatch", Assert.Throws<InvalidOperationException>(() =>
            discovery.Discover(repository.Root, repository.Goal142Relative)).Message);

        repository.Restore();
        var handoff = JsonNode.Parse(File.ReadAllText(repository.FirstHandoffPath))!.AsObject();
        handoff["recipeId"] = "tampered_recipe";
        File.WriteAllText(repository.FirstHandoffPath, handoff.ToJsonString());
        Assert.Contains("metadata mismatch", Assert.Throws<InvalidOperationException>(() =>
            discovery.Discover(repository.Root, repository.Goal142Relative)).Message);

        repository.Restore();
        var matrixPath = Path.Combine(repository.Goal142Root, "product-line-runtime-variant-matrix-result.json");
        var matrix = JsonNode.Parse(File.ReadAllText(matrixPath))!.AsObject();
        matrix["candidates"]![0]!["packagePath"] = "escaped/package.json";
        File.WriteAllText(matrixPath, matrix.ToJsonString());
        Assert.Contains("escaped", Assert.Throws<InvalidOperationException>(() =>
            discovery.Discover(repository.Root, repository.Goal142Relative)).Message);
    }

    [Fact]
    public async Task Matrix_executes_fresh_sessions_replay_and_semantic_focus_proofs()
    {
        var root = TestRepositoryRoot.Find();
        var service = new ProductLineInteractiveSessionMatrixService(
            SelectedRuntimeVariantInteractiveSessionService.CreateDefault());

        var result = await service.RunAndWriteAsync(root);

        Assert.Equal("GREEN", result.Artifacts.Matrix.Status);
        Assert.Equal(4, result.Artifacts.Matrix.CandidateCount);
        Assert.Equal(4, result.Artifacts.Matrix.PassedCandidateCount);
        Assert.Equal(4, result.Artifacts.Matrix.DistinctFinalStateHashCount);
        Assert.True(result.Artifacts.Matrix.AllCandidateCheckpointReloadsPassed);
        Assert.True(result.Artifacts.Matrix.AllCandidateFullReplaysEquivalent);
        Assert.True(result.Artifacts.Matrix.AllCandidateActionBindingsPassed);
        Assert.True(result.Artifacts.Comparison.AllFocusEffectsObserved);
        Assert.All(result.Artifacts.Matrix.Candidates, candidate => Assert.True(candidate.RuntimeEvaluated));
        Assert.Equal(3, result.Artifacts.Matrix.Candidates.Count(candidate => candidate.RuntimeMutated));
        Assert.Single(result.Artifacts.Matrix.Candidates, candidate => candidate.ControlCandidate);
    }
}

public sealed class ProductLineInteractiveSessionMatrixScriptProof
{
    [Fact]
    public async Task Run_goal145_application_matrix_from_script_environment()
    {
        var goal142Root = Environment.GetEnvironmentVariable("LLMGC_GOAL145_GOAL142_ROOT");
        if (string.IsNullOrWhiteSpace(goal142Root)) return;
        var root = TestRepositoryRoot.Find();
        var outputRoot = Environment.GetEnvironmentVariable("LLMGC_GOAL145_OUTPUT_ROOT")
                         ?? ProductLineInteractiveSessionMatrixVocabulary.ProceduralRoot;
        var selectedCandidateId = Environment.GetEnvironmentVariable("LLMGC_GOAL145_SELECTED_CANDIDATE_ID")
                                  ?? string.Empty;
        var unitySmokePath = Environment.GetEnvironmentVariable("LLMGC_GOAL145_UNITY_SMOKE_PATH")
                             ?? ProductLineInteractiveSessionMatrixVocabulary.ProceduralRoot
                             + "/unity-product-line-interactive-session-matrix-smoke.json";
        var requireUnity = string.Equals(
            Environment.GetEnvironmentVariable("LLMGC_GOAL145_REQUIRE_UNITY_SMOKE"),
            "true",
            StringComparison.OrdinalIgnoreCase);
        var runner = new ProductLineInteractiveSessionMatrixOperatorRunner(
            new ProductLineInteractiveSessionMatrixService(
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault()));
        var result = await runner.RunAsync(root, new ProductLineInteractiveSessionMatrixRequest
        {
            Goal142Root = goal142Root,
            OutputRoot = outputRoot,
            SelectedCandidateId = selectedCandidateId,
            UnitySmokePath = unitySmokePath
        });
        Assert.Equal(4, result.Artifacts.Matrix.PassedCandidateCount);
        Assert.Equal(4, result.Artifacts.Matrix.DistinctFinalStateHashCount);
        Assert.True(result.Artifacts.Matrix.AllFocusEffectsObserved);
        Assert.Equal(
            string.IsNullOrWhiteSpace(selectedCandidateId)
                ? "minimal-map-game-exploration-resource-focus"
                : selectedCandidateId,
            result.Artifacts.Selection.SelectedCandidateId);
        if (requireUnity)
        {
            Assert.True(result.Artifacts.UnitySmoke.Passed);
            Assert.Equal("GREEN", result.Artifacts.Dashboard.Status);
        }
    }
}

internal static class TestRepositoryRoot
{
    public static string Find()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            var parent = Directory.GetParent(current);
            if (parent is null) break;
            current = parent.FullName;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}

internal sealed class TamperRepository : IDisposable
{
    private readonly string _source;

    private TamperRepository(string root, string source)
    {
        Root = root;
        _source = source;
    }

    public string Root { get; }
    public string Goal142Relative => ProductLineInteractiveSessionMatrixVocabulary.Goal142Root;
    public string Goal142Root => Path.Combine(Root, Goal142Relative.Replace('/', Path.DirectorySeparatorChar));
    public string FirstPackagePath => Directory.EnumerateFiles(
        Path.Combine(Goal142Root, "candidates"), "package.json", SearchOption.AllDirectories).Order().First();
    public string FirstHandoffPath => Directory.EnumerateFiles(
        Path.Combine(Goal142Root, "candidates"), "candidate-handoff.json", SearchOption.AllDirectories).Order().First();

    public static TamperRepository Create(string sourceRoot)
    {
        var root = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "LLMGameCreator.sln"), string.Empty);
        var source = Path.Combine(sourceRoot,
            ProductLineInteractiveSessionMatrixVocabulary.Goal142Root.Replace('/', Path.DirectorySeparatorChar));
        var result = new TamperRepository(root, source);
        result.Restore();
        return result;
    }

    public void Restore()
    {
        if (Directory.Exists(Goal142Root)) Directory.Delete(Goal142Root, true);
        foreach (var file in Directory.EnumerateFiles(_source, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(Goal142Root, Path.GetRelativePath(_source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(Root)) Directory.Delete(Root, true);
    }
}
