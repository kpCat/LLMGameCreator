using System.Text.Json;
using LLMGameCreator.Application.Design.ContentGeneration;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class ContentGenerationScaleSmokeTests
{
    [Fact]
    public async Task ContentGenerationScaleProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var packDirectory = Path.Combine(FindRepoRoot(), "samples", "content-generation-packs");
        var service = ContentGenerationScaleAcceptanceTestFactory.CreateService();

        var write = await service.BuildAndWriteAsync(projectRoot, packDirectory);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));

        var report = JsonSerializer.Deserialize<ContentGenerationScaleReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.True(report.Accepted);
        Assert.Equal("content_generation_at_scale_artifact_verification", report.ManualGate);
        Assert.Equal(3, report.ValidPackCount);
        Assert.True(report.RuntimeThreadsAccepted >= 6);
        Assert.Equal(report.RuntimeThreadCount, report.RuntimeThreadsAccepted);
        Assert.True(report.ObjectiveKindDistribution.Count >= 3);
        Assert.True(report.EventActionDistribution.Count >= 3);
        Assert.True(report.ReplayEvidence.Passed);
        Assert.True(report.VariationEvidence.Passed);
        Assert.True(report.IsolationEvidence.Passed);
        Assert.True(report.InvalidMatrix.Passed);
        Assert.All(report.Packs, pack =>
        {
            Assert.True(pack.Counts.TotalInstances >= 200);
            Assert.True(pack.PackageAudit.ValidatorClean);
            Assert.True(pack.PackageAudit.StructuralAuditPassed);
            Assert.True(pack.PackageAudit.GeneratedContentHashMatchesCatalog);
            Assert.True(pack.RepetitionMetrics.MaxSharePassed);
            Assert.Equal(0, pack.RepetitionMetrics.DuplicateDialogueLines);
            Assert.All(pack.RuntimeThreads, thread =>
            {
                Assert.True(thread.RuntimeEvidence.RuntimeBoundary.UsedGameRuntimeService);
                Assert.True(thread.RuntimeEvidence.StateDelta.QuestProgressChanged);
                Assert.True(thread.RuntimeEvidence.StateDelta.RewardItemChanged);
                Assert.All(thread.Commands.Where(command => !string.IsNullOrWhiteSpace(command.ExpectedChangedFlagId)), command =>
                    Assert.Contains(command.ExpectedChangedFlagId, thread.RuntimeEvidence.StateDelta.ChangedFlagIds));
                Assert.All(thread.Commands.Where(command => !string.IsNullOrWhiteSpace(command.ExpectedChangedFactionId)), command =>
                    Assert.Contains(command.ExpectedChangedFactionId, thread.RuntimeEvidence.StateDelta.ChangedFactionIds));
                Assert.True(thread.RuntimeEvidence.SaveLoadRoundtripPassed);
                Assert.True(thread.RuntimeEvidence.IsolationPassed);
            });
        });
        Assert.False(report.ExternalExecution.LlmExecuted);
        Assert.False(report.ExternalExecution.RagExecuted);
        Assert.False(report.ExternalExecution.ProviderExecuted);
        Assert.False(report.ExternalExecution.LuaExecuted);
        Assert.False(report.ExternalExecution.UnityExecuted);
        Assert.False(report.ExternalExecution.MediaExecuted);
    }

    private static string ResolveProjectFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? tempPath : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
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
