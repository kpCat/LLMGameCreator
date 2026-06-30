using System.Text.Json;
using LLMGameCreator.Application.Design.FullGeneratorVariabilityRegressionMatrix;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

[Collection("UnityAlphaProductSmoke")]
public sealed class FullGeneratorVariabilityRegressionMatrixProductSmokeTests
{
    [Fact]
    public async Task FullGeneratorVariabilityRegressionMatrixProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var outputRoot = ResolveOutputFolder(repoRoot);
        var service = new FullGeneratorVariabilityMatrixEvidenceService();
        var write = await service.BuildAndWriteAsync(
            outputRoot,
            new FullGeneratorVariabilityMatrixOptions
            {
                RepositoryRootPath = repoRoot,
                ExecuteUnityProof = true,
                CleanupUnityWorkProject = true,
                UnityBuildTimeoutSeconds = 900,
                PlayerLaunchTimeoutSeconds = 120
            });

        AssertFile(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.SourceManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.SeedProfileMatrixJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.VarianceMetricsJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.ReplayProofJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.ReviewPackageMatrixManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.PreviewExportMatrixPayloadJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.UnityCommandPlanJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.UnityPlayerProofJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.InvalidMatrixJsonFileName);
        AssertFile(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.ArtifactScopeReportMarkdownFileName);
        Assert.True(File.Exists(write.ReportMarkdownPath));
        AssertFile(write.StagingDirectoryPath, FullGeneratorVariabilityMatrixVocabulary.UnityMatrixCommandPlanStagingRelativePath);

        foreach (var familyId in FullGeneratorVariabilityMatrixVocabulary.FamilyIds)
        {
            foreach (var seedId in FullGeneratorVariabilityMatrixVocabulary.SeedIds)
            {
                AssertFile(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.RowFileName(familyId, seedId));
            }
        }

        using var sourceManifest = Parse(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.SourceManifestJsonFileName);
        using var matrix = Parse(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.SeedProfileMatrixJsonFileName);
        using var variance = Parse(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.VarianceMetricsJsonFileName);
        using var replay = Parse(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.ReplayProofJsonFileName);
        using var commandPlan = Parse(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.UnityCommandPlanJsonFileName);
        using var proof = Parse(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.UnityPlayerProofJsonFileName);
        using var invalid = Parse(write.OutputDirectoryPath, FullGeneratorVariabilityMatrixEvidenceService.InvalidMatrixJsonFileName);
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.False(sourceManifest.RootElement.GetProperty("accepted").GetBoolean());
        Assert.True(sourceManifest.RootElement.GetProperty("goal058AcceptedByUserHandoff").GetBoolean());
        Assert.True(matrix.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(variance.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(replay.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(commandPlan.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("accepted=false", report);
        Assert.Contains("manualGate=full_generator_variability_regression_matrix_verification", report);
        Assert.Contains("goal058AcceptedByUserHandoff=true", report);
        Assert.Contains("matrixRowsPassed=true", report);
        Assert.Contains("varianceMetricsPassed=true", report);
        Assert.Contains("replayDeterminismPassed=true", report);
        Assert.Contains("invalidMatrixPassed=true", report);

        var status = ExtractReportValue(report, "implementationStatus");
        Assert.Contains(status, new[] { "GREEN", "BLOCKED" });
        if (status == "GREEN")
        {
            Assert.True(proof.RootElement.GetProperty("passed").GetBoolean());
            Assert.Equal(0, proof.RootElement.GetProperty("unityExitCode").GetInt32());
            Assert.Equal(0, proof.RootElement.GetProperty("playerExitCode").GetInt32());
            Assert.Contains("full_generator_matrix_loaded=true", report);
            Assert.Contains("full_generator_matrix_completed=true", report);
            foreach (var row in write.Result.MatrixRowsByRowId.Values)
            {
                Assert.Contains("matrix_row_started=" + row.RowId, report);
                Assert.Contains("matrix_row_completed=" + row.RowId, report);
            }
        }
        else
        {
            Assert.False(proof.RootElement.GetProperty("passed").GetBoolean());
            Assert.Contains("allMatrixMarkersMatched=false", report);
            Assert.Contains(write.Result.Report.Diagnostics, item => item.Code.StartsWith("goal059.unity.", StringComparison.Ordinal));
        }
    }

    private static void AssertFile(string directoryPath, string relativePath) =>
        Assert.True(File.Exists(Path.Combine(directoryPath, relativePath.Replace('/', Path.DirectorySeparatorChar))), "Missing evidence file: " + relativePath);

    private static JsonDocument Parse(string directoryPath, string fileName) =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(directoryPath, fileName)));

    private static string ExtractReportValue(string report, string key)
    {
        foreach (var line in report.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
        {
            var prefix = key + "=";
            if (line.StartsWith(prefix, StringComparison.Ordinal))
            {
                return line[prefix.Length..].Trim();
            }
        }

        return string.Empty;
    }

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
