using LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;
using Xunit;

namespace LLMGameCreator.Tests.Application.SchemaDrivenCampaignEditValidateApplyLoop;

public sealed class SchemaDrivenCampaignEditValidateApplyQualityGateTests
{
    [Fact]
    public void QualityGateScansChangedReadableFiles()
    {
        var result = new SchemaDrivenCampaignEditEvidenceService().Build(ProjectRoot());

        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.QualityGateScan.CompositionRootScanned);
        Assert.True(result.QualityGateScan.Goal074WinFormsFilesScanned);
        Assert.False(result.QualityGateScan.ReportOnlyTestDetected);
        Assert.Equal(0, result.QualityGateScan.LinesOver500Count);
        Assert.Equal(0, result.QualityGateScan.FilesOver1000LinesCount);
        Assert.Equal(0, result.QualityGateScan.MinifiedSourceFileCount);
        Assert.True(result.QualityGateScan.MaxLineLength <= 500);
        Assert.Contains(
            result.QualityGateScan.Files,
            file => file.RelativePath == "src/LLMGameCreator.WinForms/CompositionRoot.cs");
    }

    [Fact]
    public void QualityGateRejectsMinifiedOneLineSource()
    {
        var root = Path.Combine(Path.GetTempPath(), "llmgc-goal075-quality-" + Guid.NewGuid().ToString("N"));
        try
        {
            var seamRoot = Path.Combine(
                root,
                "src",
                "LLMGameCreator.Application",
                "Design",
                "SchemaDrivenCampaignEditValidateApplyLoop");
            Directory.CreateDirectory(seamRoot);
            File.WriteAllText(Path.Combine(seamRoot, "Minified.cs"), "public sealed class Minified { }");
            var scan = new SchemaDrivenCampaignEditQualityGateScanner().Scan(root);

            Assert.False(scan.Passed);
            Assert.Equal(1, scan.MinifiedSourceFileCount);
            Assert.Contains(scan.Diagnostics, diagnostic => diagnostic.Code == "goal075.quality.minified_source");
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
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
