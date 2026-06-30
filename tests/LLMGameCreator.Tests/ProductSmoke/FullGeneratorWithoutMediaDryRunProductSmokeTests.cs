using System.Text.Json;
using LLMGameCreator.Application.Design.FullGeneratorWithoutMediaDryRun;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class FullGeneratorWithoutMediaDryRunProductSmokeTests
{
    [Fact]
    public async Task FullGeneratorWithoutMediaDryRunProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var outputRoot = ResolveOutputFolder(repoRoot);
        var service = new FullGeneratorWithoutMediaDryRunEvidenceService();
        var result = service.Build(repoRoot);

        var write = await service.WriteAsync(outputRoot, result);

        AssertFile(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.SourceManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.ReviewPromotionLedgerJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.RepairDiagnosticsMatrixJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.MapPanelFamilyDryRunJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.SurvivalFamilyDryRunJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.GridDungeonFamilyDryRunJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.RuntimePreviewValidationMatrixJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.ExportProfileSelectionMatrixJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.PackageCompatibilitySummaryJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.OneClickDryRunSummaryJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.InvalidFakeLeakMatrixJsonFileName);
        Assert.True(File.Exists(write.ReportMarkdownPath));

        using var manifest = Parse(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.SourceManifestJsonFileName);
        using var ledger = Parse(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.ReviewPromotionLedgerJsonFileName);
        using var runtime = Parse(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.RuntimePreviewValidationMatrixJsonFileName);
        using var export = Parse(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.ExportProfileSelectionMatrixJsonFileName);
        using var package = Parse(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.PackageCompatibilitySummaryJsonFileName);
        using var oneClick = Parse(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.OneClickDryRunSummaryJsonFileName);
        using var invalid = Parse(write.OutputDirectoryPath, FullGeneratorWithoutMediaDryRunEvidenceService.InvalidFakeLeakMatrixJsonFileName);
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.False(manifest.RootElement.GetProperty("accepted").GetBoolean());
        Assert.Equal("without_media", manifest.RootElement.GetProperty("mediaPolicy").GetString());
        Assert.Equal(3, manifest.RootElement.GetProperty("selectedFamilyIds").GetArrayLength());
        Assert.True(ledger.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(12, ledger.RootElement.GetProperty("transitionCount").GetInt32());
        Assert.True(runtime.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(export.RootElement.GetProperty("passed").GetBoolean());
        Assert.False(package.RootElement.GetProperty("packageMaterializationAttempted").GetBoolean());
        Assert.True(package.RootElement.GetProperty("compatibilityProofPassed").GetBoolean());
        Assert.Equal("GREEN", oneClick.RootElement.GetProperty("status").GetString());
        Assert.Equal(12, oneClick.RootElement.GetProperty("evidenceFileCount").GetInt32());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("implementationStatus=GREEN", report);
        Assert.Contains("accepted=false", report);
        Assert.Contains("goal043AcceptedByUserHandoff=true", report);
        Assert.Contains("packageProofPassed=true", report);
        Assert.Contains("providerCalled: false", report);
        Assert.Contains("mediaGenerated: false", report);
        Assert.Contains("unityExecuted: false", report);
        Assert.Contains("runtimeSourceChanged: false", report);
        Assert.Contains("full_generator_without_media_verification required", report);
    }

    private static void AssertFile(string directoryPath, string fileName) =>
        Assert.True(File.Exists(Path.Combine(directoryPath, fileName)), "Missing evidence file: " + fileName);

    private static JsonDocument Parse(string directoryPath, string fileName) =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(directoryPath, fileName)));

    private static string ResolveOutputFolder(string repoRoot)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var outputFolder = string.IsNullOrWhiteSpace(configured) ? repoRoot : configured;
        Directory.CreateDirectory(outputFolder);
        return outputFolder;
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
}
