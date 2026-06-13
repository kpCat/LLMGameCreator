using LLMGameCreator.Application.Design.Atlas;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class AtlasRegistryImportServiceTests
{
    [Fact]
    public async Task ImportAtlasRegistryImportsCompleteAtlasWithoutWarningsOrErrors()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);

        var result = await new AtlasRegistryImportService().ImportAtlasRegistryAsync(temp.Path, CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Equal(0, result.Summary.ErrorCount);
        Assert.Equal(0, result.Summary.WarningCount);
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Severity == AtlasDiagnosticSeverity.Warning);
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Severity == AtlasDiagnosticSeverity.Error);
    }

    [Fact]
    public async Task ImportAtlasRegistryReportsMissingKnownFilesAsWarnings()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateAtlasRootWithMissingKnownFiles(temp.Path);

        var result = await new AtlasRegistryImportService().ImportAtlasRegistryAsync(temp.Path, CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == AtlasDiagnosticCodes.MissingKnownFile &&
            diagnostic.Severity == AtlasDiagnosticSeverity.Warning);
    }

    [Fact]
    public async Task ImportAtlasRegistryReportsInvalidJsonAsError()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateAtlasRootWithInvalidJson(temp.Path);

        var result = await new AtlasRegistryImportService().ImportAtlasRegistryAsync(temp.Path, CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == AtlasDiagnosticCodes.InvalidJson &&
            diagnostic.Severity == AtlasDiagnosticSeverity.Error);
    }

    [Fact]
    public async Task ImportAtlasRegistryDetectsDuplicateIds()
    {
        using var temp = new AtlasTempDirectory();
        var atlasRoot = AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);
        AtlasTestFixture.WriteMinimalAtlasDocument(atlasRoot, "capability_atlas.json", "duplicate/v1", "First");
        AtlasTestFixture.WriteMinimalAtlasDocument(atlasRoot, "artifact_contracts.json", "duplicate/v1", "Second");

        var result = await new AtlasRegistryImportService().ImportAtlasRegistryAsync(temp.Path, CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == AtlasDiagnosticCodes.DuplicateId &&
            diagnostic.Severity == AtlasDiagnosticSeverity.Error &&
            diagnostic.Id == "duplicate/v1");
    }

    [Fact]
    public async Task ImportAtlasRegistryReportsMissingRoot()
    {
        using var temp = new AtlasTempDirectory();
        var missingRoot = Path.Combine(temp.Path, "missing");

        var result = await new AtlasRegistryImportService().ImportAtlasRegistryAsync(missingRoot, CancellationToken.None);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == AtlasDiagnosticCodes.MissingRoot &&
            diagnostic.Severity == AtlasDiagnosticSeverity.Error);
    }

    [Fact]
    public async Task ImportAtlasRegistryReportsUnknownReferenceAsWarning()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRootWithUnknownReference(temp.Path);

        var result = await new AtlasRegistryImportService().ImportAtlasRegistryAsync(temp.Path, CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == AtlasDiagnosticCodes.ReferenceUnknown &&
            diagnostic.Severity == AtlasDiagnosticSeverity.Warning &&
            diagnostic.Id == "feature_bundle/missing/v1");
    }

    [Fact]
    public async Task ImportAtlasRegistryDiscoversExamples()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);

        var result = await new AtlasRegistryImportService().ImportAtlasRegistryAsync(temp.Path, CancellationToken.None);

        var example = Assert.Single(result.Examples);
        Assert.Equal(AtlasTestFixture.ExampleId, example.ExampleId);
        Assert.Equal(AtlasTestFixture.ProfileId, example.SourceProfileId);
        Assert.Equal(1, example.StepCount);
        Assert.Contains(AtlasTestFixture.FeatureBundleId, example.SelectedFeatureBundles);
    }
}
