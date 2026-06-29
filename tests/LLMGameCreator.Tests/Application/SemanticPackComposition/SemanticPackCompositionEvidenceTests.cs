using System.Text.Json;
using LLMGameCreator.Application.Design.SemanticPackComposition;
using Xunit;

namespace LLMGameCreator.Tests.Application.SemanticPackComposition;

public sealed class SemanticPackCompositionEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndKeepsBoundariesExplicit()
    {
        var service = new SemanticPackCompositionEvidenceService();

        var first = service.Build();
        var second = service.Build();

        Assert.Equal(first.CatalogSummaryJson, second.CatalogSummaryJson);
        Assert.Equal(first.CompositionMatrixJson, second.CompositionMatrixJson);
        Assert.Equal(first.FrontierPlanJson, second.FrontierPlanJson);
        Assert.Equal(first.GothicPlanJson, second.GothicPlanJson);
        Assert.Equal(first.CaravanPlanJson, second.CaravanPlanJson);
        Assert.Equal(first.CrossArtifactLinkageReportJson, second.CrossArtifactLinkageReportJson);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(first.Report.BlueprintProofPassed, string.Join(Environment.NewLine, first.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.False(first.Report.Accepted);
        Assert.Equal(SemanticPackCompositionEvidenceService.FinalGate, first.Report.FinalStatus);
        Assert.Equal(SemanticPackCompositionEvidenceService.FinalGate, first.Report.ManualGate);
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
        var service = new SemanticPackCompositionEvidenceService();
        var result = service.Build();
        var write = await service.WriteAsync(temp.Path, result);

        Assert.True(File.Exists(write.CatalogSummaryJsonPath));
        Assert.True(File.Exists(write.CompositionMatrixJsonPath));
        Assert.True(File.Exists(write.FrontierPlanJsonPath));
        Assert.True(File.Exists(write.GothicPlanJsonPath));
        Assert.True(File.Exists(write.CaravanPlanJsonPath));
        Assert.True(File.Exists(write.CrossArtifactLinkageReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.Contains("semantic_pack_composition_blueprint_verification required", await File.ReadAllTextAsync(write.ReportMarkdownPath));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(write.CatalogSummaryJsonPath));
        using var frontier = JsonDocument.Parse(await File.ReadAllTextAsync(write.FrontierPlanJsonPath));
        using var links = JsonDocument.Parse(await File.ReadAllTextAsync(write.CrossArtifactLinkageReportJsonPath));

        Assert.True(catalog.RootElement.GetProperty("packCount").GetInt32() >= 10);
        Assert.Equal("frontier_survival", frontier.RootElement.GetProperty("profileId").GetString());
        Assert.True(frontier.RootElement.GetProperty("crossArtifactLinks").GetArrayLength() >= 4);
        Assert.Equal(3, links.RootElement.GetProperty("scenarioCount").GetInt32());
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
