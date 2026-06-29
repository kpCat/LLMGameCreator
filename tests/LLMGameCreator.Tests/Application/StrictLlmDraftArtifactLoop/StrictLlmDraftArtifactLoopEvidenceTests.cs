using System.Text.Json;
using LLMGameCreator.Application.Design.StrictLlmDraftArtifactLoop;
using Xunit;

namespace LLMGameCreator.Tests.Application.StrictLlmDraftArtifactLoop;

public sealed class StrictLlmDraftArtifactLoopEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndKeepsBoundariesExplicit()
    {
        var service = new StrictLlmDraftArtifactLoopEvidenceService();

        var first = service.Build();
        var second = service.Build();

        Assert.Equal(first.ArtifactJsonByFileName[StrictLlmDraftArtifactLoopEvidenceService.ContractSummaryJsonFileName], second.ArtifactJsonByFileName[StrictLlmDraftArtifactLoopEvidenceService.ContractSummaryJsonFileName]);
        Assert.Equal(first.ArtifactJsonByFileName[StrictLlmDraftArtifactLoopEvidenceService.RequestMatrixJsonFileName], second.ArtifactJsonByFileName[StrictLlmDraftArtifactLoopEvidenceService.RequestMatrixJsonFileName]);
        Assert.Equal(first.ArtifactJsonByFileName[StrictLlmDraftArtifactLoopEvidenceService.CandidateMatrixJsonFileName], second.ArtifactJsonByFileName[StrictLlmDraftArtifactLoopEvidenceService.CandidateMatrixJsonFileName]);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(first.Report.ContractProofPassed, string.Join(Environment.NewLine, first.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.False(first.Report.Accepted);
        Assert.Equal(StrictLlmDraftArtifactLoopEvidenceService.FinalGate, first.Report.ManualGate);
        Assert.False(first.Report.ProviderLlmRagCalled);
        Assert.False(first.Report.FinalProseGeneratedOrPromoted);
        Assert.False(first.Report.GamePackageMaterialized);
        Assert.False(first.Report.RuntimeUiUnityLuaGeneratorLibraryTouched);
        Assert.DoesNotContain(Environment.NewLine, first.ArtifactJsonByFileName[StrictLlmDraftArtifactLoopEvidenceService.RequestMatrixJsonFileName]);
    }

    [Fact]
    public async Task EvidenceArtifactsAreWrittenAndInspectable()
    {
        using var temp = new TempDirectory();
        var write = await new StrictLlmDraftArtifactLoopEvidenceService().BuildAndWriteAsync(temp.Path);

        var names = write.WrittenFiles.Select(path => Path.GetFileName(path) ?? string.Empty).OrderBy(item => item, StringComparer.Ordinal).ToArray();
        Assert.Equal(
            [
                "candidate-quarantine-matrix.json",
                "draft-loop-contract-summary.json",
                "draft-request-matrix.json",
                "invalid-draft-diagnostics-matrix.json",
                "promotion-decision-matrix.json",
                "repair-request-matrix.json",
                "strict-draft-plan-caravan.json",
                "strict-draft-plan-frontier.json",
                "strict-draft-plan-gothic.json",
                "strict-draft-plan-metamodule-kingdoms.json",
                "strict-llm-draft-artifact-loop-report.md"
            ],
            names);

        using var metamodule = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, "strict-draft-plan-metamodule-kingdoms.json")));
        using var invalid = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, "invalid-draft-diagnostics-matrix.json")));

        Assert.True(metamodule.RootElement.GetProperty("speciesArchetypeSlotRequestCount").GetInt32() >= 100);
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("strict_llm_draft_artifact_loop_verification required", report);
        Assert.Contains("accepted=false", report);
        Assert.Contains("No provider/LLM/RAG call happened", report);
        Assert.Contains("No final prose was generated or promoted", report);
        Assert.Contains("No GamePackage materialization happened", report);
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
