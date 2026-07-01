using LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;
using Xunit;

namespace LLMGameCreator.Tests.Application.SchemaDrivenCampaignEditValidateApplyLoop;

public sealed class SchemaDrivenCampaignEditValidateApplyEvidenceTests
{
    [Fact]
    public async Task EvidenceWriterCreatesEveryRequiredArtifact()
    {
        var service = new SchemaDrivenCampaignEditEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());

        Assert.True(Directory.Exists(write.OutputDirectoryPath));
        Assert.Equal("GREEN", write.Result.Report.ImplementationStatus);
        foreach (var fileName in SchemaDrivenCampaignEditEvidenceService.RequiredArtifactNames())
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }

        Assert.DoesNotContain(write.Result.SourceManifest.SourceArtifacts, artifact =>
            Path.IsPathFullyQualified(artifact.ArtifactRelativePath));
    }

    [Fact]
    public void EvidenceIsDeterministicAcrossBuilds()
    {
        var service = new SchemaDrivenCampaignEditEvidenceService();
        var first = service.Build(ProjectRoot());
        var second = service.Build(ProjectRoot());

        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.Equal(
            first.ChangeSetCatalog.Candidates.Select(candidate => candidate.CandidateId),
            second.ChangeSetCatalog.Candidates.Select(candidate => candidate.CandidateId));
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
