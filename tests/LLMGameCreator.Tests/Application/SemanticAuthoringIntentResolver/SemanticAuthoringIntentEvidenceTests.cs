using System.Text.Json;
using LLMGameCreator.Application.Design.SemanticAuthoringIntentResolver;
using Xunit;

namespace LLMGameCreator.Tests.Application.SemanticAuthoringIntentResolver;

public sealed class SemanticAuthoringIntentEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndKeepsBoundariesExplicit()
    {
        var service = new SemanticAuthoringIntentEvidenceService();

        var first = service.Build();
        var second = service.Build();

        Assert.Equal(first.WorkspaceSchemaSummaryJson, second.WorkspaceSchemaSummaryJson);
        Assert.Equal(first.MetamoduleLoreSkeletonJson, second.MetamoduleLoreSkeletonJson);
        Assert.Equal(first.ManualVsAutoAuthoringMatrixJson, second.ManualVsAutoAuthoringMatrixJson);
        Assert.Equal(first.FrontierResolutionJson, second.FrontierResolutionJson);
        Assert.Equal(first.GothicResolutionJson, second.GothicResolutionJson);
        Assert.Equal(first.CaravanResolutionJson, second.CaravanResolutionJson);
        Assert.Equal(first.MetamoduleKingdomsResolutionJson, second.MetamoduleKingdomsResolutionJson);
        Assert.Equal(first.InvalidMatrixJson, second.InvalidMatrixJson);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(first.Report.ContractProofPassed, string.Join(Environment.NewLine, first.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.False(first.Report.Accepted);
        Assert.Equal(SemanticAuthoringIntentEvidenceService.FinalGate, first.Report.ManualGate);
        Assert.False(first.Report.FinalDialogueProseGenerated);
        Assert.False(first.Report.FinalGamePackageMaterialized);
        Assert.False(first.Report.UiChanged);
        Assert.False(first.Report.RuntimeBehaviorChanged);
        Assert.False(first.Report.UnityBuildExecuted);
        Assert.False(first.Report.LlmRagProviderMediaLuaExecuted);
    }

    [Fact]
    public async Task EvidenceArtifactsAreWrittenAndInspectable()
    {
        using var temp = new TempDirectory();
        var write = await new SemanticAuthoringIntentEvidenceService().BuildAndWriteAsync(temp.Path);

        Assert.True(File.Exists(write.WorkspaceSchemaSummaryJsonPath));
        Assert.True(File.Exists(write.MetamoduleLoreSkeletonJsonPath));
        Assert.True(File.Exists(write.ManualVsAutoAuthoringMatrixJsonPath));
        Assert.True(File.Exists(write.FrontierResolutionJsonPath));
        Assert.True(File.Exists(write.GothicResolutionJsonPath));
        Assert.True(File.Exists(write.CaravanResolutionJsonPath));
        Assert.True(File.Exists(write.MetamoduleKingdomsResolutionJsonPath));
        Assert.True(File.Exists(write.InvalidMatrixJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        using var skeleton = JsonDocument.Parse(await File.ReadAllTextAsync(write.MetamoduleLoreSkeletonJsonPath));
        using var invalid = JsonDocument.Parse(await File.ReadAllTextAsync(write.InvalidMatrixJsonPath));

        Assert.True(skeleton.RootElement.GetProperty("evidenceSummary").GetProperty("speciesArchetypeSlotCount").GetInt32() >= 100);
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("semantic_authoring_intent_resolver_verification required", await File.ReadAllTextAsync(write.ReportMarkdownPath));
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
