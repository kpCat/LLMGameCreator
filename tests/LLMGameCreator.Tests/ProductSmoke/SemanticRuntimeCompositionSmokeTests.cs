using System.Text.Json;
using LLMGameCreator.Application.Design.Semantics;
using LLMGameCreator.Tests.Application.Semantics;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class SemanticRuntimeCompositionSmokeTests
{
    [Fact]
    public async Task SemanticRuntimeCompositionProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var service = SemanticRuntimeCompositionAcceptanceTests.CreateService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        var json = await File.ReadAllTextAsync(write.ReportJsonPath);
        var report = JsonSerializer.Deserialize<SemanticSelectedRuntimeCompositionReport>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        Assert.True(report.Accepted);
        Assert.Equal("semantic_selected_runtime_composition_artifact_verification", report.ManualGate);
        Assert.True(report.SemanticSelectedIdsExecutedInRuntime);
        Assert.True(report.InvalidScenarioRejected);
        Assert.True(report.CrossVariantIsolationPassed);
        Assert.False(report.ExternalExecution.LlmExecuted);
        Assert.False(report.ExternalExecution.RagExecuted);
        Assert.False(report.ExternalExecution.ProviderExecuted);
        Assert.False(report.ExternalExecution.LuaExecuted);
        Assert.False(report.ExternalExecution.UnityExecuted);
        Assert.False(report.ExternalExecution.MediaExecuted);

        var overlay = report.Scenarios.Single(item => item.ScenarioId == "core_genre_project_overlay");
        Assert.Equal(
            ["interaction/take_cache_item", "interaction/talk_contact", "interaction/use_reward_on_contact"],
            overlay.CompositionPlan.MaterializedInteractions.Select(item => item.InteractionPatternId).OrderBy(item => item, StringComparer.Ordinal).ToList());
        Assert.All(report.Scenarios.Where(item => item.ExpectedValid), scenario =>
        {
            Assert.True(scenario.SemanticSelectedIdsExecutedInRuntime);
            Assert.All(scenario.CompositionPlan.SelectedQuestObjectives, objective =>
                Assert.Contains(scenario.RuntimeEvidence.ObjectiveInteractionCorrelations, correlation =>
                    correlation.PackageObjectiveId == objective.PackageObjectiveId &&
                    objective.RequiredInteractionPatternIds.Contains(correlation.InteractionPatternId) &&
                    correlation.InteractionSucceeded &&
                    correlation.ObjectiveAdvanceSucceeded));
            Assert.True(scenario.RuntimeEvidence.StateDelta.RewardAmountAfter > scenario.RuntimeEvidence.StateDelta.RewardAmountBefore);
            Assert.Equal(scenario.RuntimeEvidence.StateEvidence, scenario.RuntimeEvidence.RestoredStateEvidence);
        });

        var invalid = report.Scenarios.Single(item => item.ScenarioId == "invalid_conflict_rejection");
        Assert.False(invalid.ActualValid);
        Assert.False(invalid.RuntimeEvidence.RuntimeAttempted);
        Assert.Contains(invalid.Diagnostics, item => item.Code == "semantic_guided.excludes_conflict");
    }

    private static string ResolveProjectFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? tempPath : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
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
