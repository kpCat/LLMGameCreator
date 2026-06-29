using System.Text.Json;
using LLMGameCreator.Application.Design.DynamicSemanticFeatures;
using Xunit;

namespace LLMGameCreator.Tests.Application.DynamicSemanticFeatures;

public sealed class DynamicSemanticFeatureEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndKeepsBoundariesExplicit()
    {
        var service = new DynamicSemanticFeatureEvidenceService();

        var first = service.Build();
        var second = service.Build();

        Assert.Equal(first.FeatureCatalogSummaryJson, second.FeatureCatalogSummaryJson);
        Assert.Equal(first.InfluenceRuleSummaryJson, second.InfluenceRuleSummaryJson);
        Assert.Equal(first.AuthoringSchemaMatrixJson, second.AuthoringSchemaMatrixJson);
        Assert.Equal(first.FrontierStateJson, second.FrontierStateJson);
        Assert.Equal(first.GothicStateJson, second.GothicStateJson);
        Assert.Equal(first.CaravanStateJson, second.CaravanStateJson);
        Assert.Equal(first.MetamoduleKingdomsStateJson, second.MetamoduleKingdomsStateJson);
        Assert.Equal(first.InvalidMatrixJson, second.InvalidMatrixJson);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(first.Report.ContractProofPassed, string.Join(Environment.NewLine, first.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.False(first.Report.Accepted);
        Assert.Equal(DynamicSemanticFeatureEvidenceService.FinalGate, first.Report.ManualGate);
        Assert.False(first.Report.PublicGamePackageSchemaChanged);
        Assert.False(first.Report.ProjectFilesChanged);
        Assert.False(first.Report.GeneratorLibraryChanged);
        Assert.False(first.Report.UnityBuildExecuted);
        Assert.False(first.Report.LlmRagProviderMediaLuaExecuted);
        Assert.False(first.Report.RuntimeBehaviorChanged);
    }

    [Fact]
    public async Task EvidenceArtifactsAreWrittenAndInspectable()
    {
        using var temp = new TempDirectory();
        var service = new DynamicSemanticFeatureEvidenceService();
        var write = await service.BuildAndWriteAsync(temp.Path);

        Assert.True(File.Exists(write.FeatureCatalogSummaryJsonPath));
        Assert.True(File.Exists(write.InfluenceRuleSummaryJsonPath));
        Assert.True(File.Exists(write.AuthoringSchemaMatrixJsonPath));
        Assert.True(File.Exists(write.FrontierStateJsonPath));
        Assert.True(File.Exists(write.GothicStateJsonPath));
        Assert.True(File.Exists(write.CaravanStateJsonPath));
        Assert.True(File.Exists(write.MetamoduleKingdomsStateJsonPath));
        Assert.True(File.Exists(write.InvalidMatrixJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.Contains("dynamic_semantic_feature_system_verification required", await File.ReadAllTextAsync(write.ReportMarkdownPath));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(write.FeatureCatalogSummaryJsonPath));
        using var metamodule = JsonDocument.Parse(await File.ReadAllTextAsync(write.MetamoduleKingdomsStateJsonPath));
        using var invalid = JsonDocument.Parse(await File.ReadAllTextAsync(write.InvalidMatrixJsonPath));

        Assert.True(catalog.RootElement.GetProperty("featureCount").GetInt32() >= 17);
        Assert.Equal("metamodule_kingdoms", metamodule.RootElement.GetProperty("scenarioId").GetString());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
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
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
