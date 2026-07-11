using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.ProductLineRuntimeQualification;

public sealed class ProductLineRuntimeQualificationTests
{
    [Fact]
    public void Qualifier_runs_the_shared_canonical_action_plan_checkpoint_and_full_replay()
    {
        var root = FindRoot();
        var path = Path.Combine(root, ".llmgc", "procedural",
            "goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff",
            "candidates", "minimal-map-game-balanced-baseline", "package.json");
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        var package = JsonSerializer.Deserialize<GamePackageDefinition>(File.ReadAllText(path), options)!;
        using var stream = File.OpenRead(path);
        var hash = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        var result = new ProductLineRuntimeQualifier(SelectedRuntimeVariantInteractiveSessionService.CreateDefault())
            .Qualify(package, new ProductLineRuntimeQualificationRequest
            {
                SessionId = "qualification-test",
                CandidateId = "minimal-map-game-balanced-baseline",
                VariantKind = "balanced_baseline",
                PackagePath = Path.GetRelativePath(root, path).Replace('\\', '/'),
                PackageSha256 = hash,
                CheckpointId = "qualification-test-checkpoint",
                FinalCheckpointId = "qualification-test-final"
            });

        Assert.Equal(13, ProductLineRuntimeQualifier.CanonicalActionPlan.Count);
        Assert.Equal(14, result.ActionCatalog.Count);
        Assert.True(result.InvalidActionStateUnchanged);
        Assert.True(result.ActionDescriptorExecutionBindingPassed);
        Assert.True(result.CheckpointReplay.Passed);
        Assert.Equal(8, result.CheckpointReplay.ReplayedActionCount);
        Assert.True(result.FinalReplay.Passed);
        Assert.Equal(13, result.FinalReplay.ReplayedActionCount);
        Assert.Equal(result.Session.CurrentStateHash, result.FinalReplay.ActualStateHash);
    }

    private static string FindRoot()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }
        throw new InvalidOperationException("Repository root was not found.");
    }
}
