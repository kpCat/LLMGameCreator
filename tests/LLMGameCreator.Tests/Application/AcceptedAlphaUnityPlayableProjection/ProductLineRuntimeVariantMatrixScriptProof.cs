using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.ProductLineRuntimeVariantMatrix;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

public sealed class ProductLineRuntimeVariantMatrixScriptProof
{
    [Fact]
    public async Task WritesGoal142RuntimeSignificantVariantMatrixArtifacts()
    {
        var root = ProjectRoot();
        var request = new ProductLineRuntimeVariantMatrixRequest
        {
            TemplatePackagePath = EnvOrDefault(
                "LLMGC_GOAL142_TEMPLATE_PACKAGE_PATH",
                ProductLineRuntimeVariantMatrixVocabulary.TemplatePackagePath),
            VariantCatalogPath = EnvOrDefault(
                "LLMGC_GOAL142_VARIANT_CATALOG_PATH",
                ProductLineRuntimeVariantMatrixVocabulary.CatalogRelativePath),
            OutputRoot = EnvOrDefault(
                "LLMGC_GOAL142_OUTPUT_ROOT",
                ProductLineRuntimeVariantMatrixVocabulary.ProceduralOutputDirectory)
        };

        var write = await new ProductLineRuntimeVariantMatrixService(
                RuntimeBackedPlayerCommandRoundtripService.CreateDefault())
            .BuildAndWriteAsync(root, request);

        Assert.Equal("GREEN", write.Dashboard.MatrixStatus);
        Assert.False(write.Dashboard.Accepted);
        Assert.Equal(4, write.Dashboard.CandidateCount);
        Assert.Equal(4, write.Dashboard.PassedCandidateCount);
        Assert.Equal(0, write.Dashboard.FailedCandidateCount);
        Assert.Equal(4, write.Dashboard.RuntimeSignificantCandidateCount);
        Assert.True(write.Dashboard.DistinctFinalStateHashCount >= 3);
        Assert.True(write.Dashboard.SourceTemplateUnmodified);
        Assert.Equal(
            "minimal-map-game-exploration-resource-focus",
            write.Dashboard.SelectedCandidateId);
        Assert.Equal("exploration_resource_focus", write.Dashboard.SelectedVariantKind);
        Assert.True(write.Dashboard.SelectedScore > 0);
        Assert.False(write.SelectedHandoff.Accepted);
        Assert.True(write.SelectedHandoff.RuntimeAuthority);
        Assert.False(write.SelectedHandoff.ProjectionOnly);
        Assert.True(write.DistinctnessProof.Passed);
        Assert.True(write.DistinctnessProof.AllPackageHashesDistinct);
        Assert.True(write.DistinctnessProof.AllMutationAuditsPassed);
        Assert.True(write.DistinctnessProof.AllRoundtripSemanticProofsPassed);
        Assert.True(write.DistinctnessProof.AlchemyRuntimeEffectObserved);
        Assert.True(write.DistinctnessProof.CombatRuntimeEffectObserved);
        Assert.True(write.DistinctnessProof.ExplorationRuntimeEffectObserved);
        Assert.True(write.DistinctnessProof.NoMetadataOnlyVariantAccepted);

        foreach (var candidateId in ProductLineRuntimeVariantMatrixVocabulary.CandidateIds)
        {
            var row = write.MatrixResult.Candidates.Single(item => item.CandidateId == candidateId);
            Assert.True(row.PackageValidation.Passed);
            Assert.True(row.MutationAudit.Passed);
            Assert.True(row.RuntimeOutcomeSummary.RoundtripSemanticProofPassed);
            Assert.True(row.Passed);
            Assert.True(File.Exists(Path.Combine(
                root,
                request.OutputRoot,
                "candidates",
                candidateId,
                "package.json")));
            Assert.True(File.Exists(Path.Combine(
                root,
                request.OutputRoot,
                "matrix",
                candidateId,
                "roundtrip-result.json")));
        }

        foreach (var fileName in RequiredAggregateFiles())
        {
            Assert.Contains(write.WrittenFiles, path => path == request.OutputRoot + "/" + fileName);
            Assert.Contains(write.WrittenFiles, path =>
                path == ProductLineRuntimeVariantMatrixVocabulary.ExportPackageDirectory + "/" + fileName);
        }

        Assert.Contains(write.WrittenFiles, path =>
            path == ProductLineRuntimeVariantMatrixVocabulary.SelectedHandoffRelativePath);
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
    }

    private static IReadOnlyList<string> RequiredAggregateFiles() =>
    [
        ProductLineRuntimeVariantMatrixVocabulary.CatalogFileName,
        ProductLineRuntimeVariantMatrixVocabulary.DashboardFileName,
        ProductLineRuntimeVariantMatrixVocabulary.MatrixResultFileName,
        ProductLineRuntimeVariantMatrixVocabulary.MutationSummaryFileName,
        ProductLineRuntimeVariantMatrixVocabulary.DistinctnessProofFileName,
        ProductLineRuntimeVariantMatrixVocabulary.ScoreboardFileName,
        ProductLineRuntimeVariantMatrixVocabulary.NegativeProofFileName,
        ProductLineRuntimeVariantMatrixVocabulary.FileIndexFileName,
        ProductLineRuntimeVariantMatrixVocabulary.OneClickReportJsonFileName,
        ProductLineRuntimeVariantMatrixVocabulary.OneClickReportMarkdownFileName
    ];

    private static string EnvOrDefault(string name, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
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
