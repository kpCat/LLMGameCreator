using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

public sealed class ProductLineStrategyRebaselineServiceTests
{
    [Fact]
    public async Task Goal133AProductLineStrategyRebaselineWritesGreenEvidence()
    {
        var root = ProjectRoot();
        var write = await new ProductLineStrategyRebaselineService()
            .BuildAndWriteAsync(root);
        var result = write.Result;

        Assert.Equal("GREEN", result.Dashboard.ImplementationStatus);
        Assert.Equal(
            ProductLineStrategyRebaselineVocabulary.Gate,
            result.Dashboard.Gate);
        Assert.False(result.Dashboard.Accepted);
        Assert.True(result.Dashboard.ProductLineCombiner);
        Assert.True(result.Dashboard.NotPromptToGame);
        Assert.True(result.Dashboard.LlmOptionalAuthoringOnly);
        Assert.True(result.Dashboard.NewDocsPresent);
        Assert.True(result.Dashboard.AgentsRoutingUpdated);
        Assert.True(result.Dashboard.ContextIndexRoutingUpdated);
        Assert.True(result.Dashboard.CurrentStateUpdated);
        Assert.True(result.Dashboard.QueueUpdated);
        Assert.Equal(
            ProductLineStrategyRebaselineVocabulary.NextGoal,
            result.Dashboard.NextGoal);
        Assert.True(result.Dashboard.ManualUnityOptional);
        Assert.True(result.Dashboard.ProjectionOnlyStopCondition);
        Assert.True(result.Dashboard.RuntimeUnchanged);
        Assert.True(result.Dashboard.UnityUnchanged);
        Assert.True(result.Dashboard.SchemaUnchanged);
        Assert.True(result.Dashboard.SamplePackageUnchanged);
        Assert.True(result.Dashboard.ManualInputUnchanged);
        Assert.Empty(result.Dashboard.Diagnostics);

        Assert.True(result.DocScan.Passed);
        Assert.True(result.DocScan.RequiredSeamsPresent);
        Assert.True(result.DocScan.RequiredPolicyStatementsPresent);
        Assert.True(result.DocScan.OldGoal133Rerouted);
        Assert.True(result.DocScan.Goal131EvidencePresent);
        Assert.True(result.DocScan.Goal132EvidencePresent);
        Assert.True(result.DocScan.ArtifactScopeScenarioPresent);
        Assert.Empty(result.DocScan.MissingMarkers);

        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.NegativeProof.RuntimeAbstractionsUnchanged);
        Assert.True(result.NegativeProof.ProviderMediaLuaGeneratorLibraryUnchanged);
        Assert.DoesNotContain(result.NegativeProof.PlannedWrites, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));

        Assert.Contains(result.ProceduralFileIndex.Files, file =>
            file.RelativePath == ProductLineStrategyRebaselineVocabulary.DashboardRelativePath);
        Assert.Contains(result.ProceduralFileIndex.Files, file =>
            file.RelativePath == ProductLineStrategyRebaselineVocabulary.DocScanRelativePath);
        Assert.Contains(result.ProceduralFileIndex.Files, file =>
            file.RelativePath == ProductLineStrategyRebaselineVocabulary.NegativeProofRelativePath);
        Assert.Contains(write.WrittenFiles, path =>
            path == ProductLineStrategyRebaselineVocabulary.ExportPackageDirectory
            + "/"
            + ProductLineStrategyRebaselineVocabulary.DashboardFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == ProductLineStrategyRebaselineVocabulary.ExportPackageDirectory
            + "/"
            + ProductLineStrategyRebaselineVocabulary.DocScanFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == ProductLineStrategyRebaselineVocabulary.DocumentationPath);
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
    }

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
