using LLMGameCreator.Application.Design.EditDrivenSpineQualityConsolidation;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class EditDrivenSpineQualityConsolidationProductSmokeTests
{
    private const string Goal078NegativeProofPath =
        ".llmgc/procedural/goal-078-edit-driven-review-package-playable-session/tamper-negative-proof.json";

    [Fact]
    public async Task Goal079EditDrivenSpineQualityConsolidationWritesAndVerifiesDashboardArtifacts()
    {
        var root = ProjectRoot();
        var service = new EditDrivenSpineQualityConsolidationEvidenceService();
        var write = await service.BuildAndWriteAsync(root);
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.QualityGateScan.Passed);
        Assert.Equal(EditDrivenSpineQualityConsolidationVocabulary.FinalGate, result.Report.ManualGate);

        var dashboard = ReadArtifact<EditDrivenSpineQualityConsolidationReadinessDashboard>(
            write.OutputDirectoryPath,
            "acceptance-readiness-dashboard.json");
        var chain = ReadArtifact<EditDrivenSpineQualityConsolidationChainManifest>(
            write.OutputDirectoryPath,
            "spine-chain-manifest.json");
        var negative = ReadArtifact<EditDrivenSpineQualityConsolidationNegativeProofIndex>(
            write.OutputDirectoryPath,
            "negative-proof-index.json");
        var debt = ReadArtifact<EditDrivenSpineQualityConsolidationDebtClassification>(
            write.OutputDirectoryPath,
            "quality-debt-classification.json");

        Assert.True(dashboard.PackageReadProofPassed);
        Assert.True(dashboard.ReplayProofPassed);
        Assert.True(dashboard.ReplayFinalHashMatchesOriginal);
        Assert.Equal(0, dashboard.P0Count);
        Assert.Equal(0, dashboard.P1Count);
        Assert.Equal(5, chain.ChainItemCount);
        Assert.All(chain.ChainItems, item => Assert.False(string.IsNullOrWhiteSpace(item.ReportHash)));
        Assert.True(negative.Passed);
        Assert.Contains(debt.Debts, item => item.Severity == "P2");
        Assert.Contains(debt.Debts, item => item.FindingId == "GQ-P3-ADAPTIVE-DOCS-CONTEXT-INDEXING");

        var proof = File.ReadAllText(Path.Combine(root, Goal078NegativeProofPath));
        var tampered = ReplaceFirst(proof, "\"actualStatus\": \"rejected\"", "\"actualStatus\": \"accepted\"");
        var tamperedCopyPath = Path.Combine(Path.GetTempPath(), "goal079-tampered-negative-proof.json");
        try
        {
            await File.WriteAllTextAsync(tamperedCopyPath, tampered);
            var blocked = service.Build(root, new EditDrivenSpineQualityConsolidationBuildOptions
            {
                ArtifactTextOverridesByRelativePath = new Dictionary<string, string?>(StringComparer.Ordinal)
                {
                    [Goal078NegativeProofPath] = await File.ReadAllTextAsync(tamperedCopyPath)
                }
            });

            Assert.Equal("BLOCKED", blocked.Report.ImplementationStatus);
            Assert.False(blocked.NegativeProofIndex.Passed);
            Assert.Contains(blocked.NegativeProofIndex.Diagnostics, diagnostic =>
                diagnostic.Code == "goal079.negative.scenario_not_real_rejection");
        }
        finally
        {
            if (File.Exists(tamperedCopyPath))
            {
                File.Delete(tamperedCopyPath);
            }
        }
    }

    private static T ReadArtifact<T>(string outputRoot, string fileName)
    {
        var json = File.ReadAllText(Path.Combine(outputRoot, fileName));
        var value = EditDrivenSpineQualityConsolidationHash.Deserialize<T>(json);
        Assert.NotNull(value);
        return value!;
    }

    private static string ReplaceFirst(string text, string oldValue, string newValue)
    {
        var index = text.IndexOf(oldValue, StringComparison.Ordinal);
        return index < 0
            ? text
            : text[..index] + newValue + text[(index + oldValue.Length)..];
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
