using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.SelectedRuntimeVariantPlayerAdapter;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.SelectedRuntimeVariantPlayerAdapter;

public sealed class SelectedRuntimeVariantPlayerAdapterValidatorTests
{
    [Fact]
    public void ValidatesAuthoritativeGoal142SelectedHandoff()
    {
        var result = new SelectedRuntimeVariantPlayerAdapterValidator().Validate(
            ProjectRoot(),
            new SelectedRuntimeVariantPlayerAdapterRequest());

        Assert.True(result.Validation.Passed);
        Assert.True(result.Validation.SelectedPackageSha256MatchesHandoff);
        Assert.True(result.Validation.SelectedFinalStateHashMatches);
        Assert.True(result.Validation.SourcePathsConsistent);
        Assert.True(result.Validation.NoBalancedBaselineFallback);
        Assert.False(result.Validation.SelectedHandoffAccepted);
    }

    [Fact]
    public void RejectsSelectedPackageHashMismatch()
    {
        using var temp = new TempSelectedSource(ProjectRoot());
        File.AppendAllText(temp.SelectedPackagePath, Environment.NewLine + " ");

        var error = Assert.Throws<InvalidOperationException>(() =>
            new SelectedRuntimeVariantPlayerAdapterValidator().Validate(
                temp.Root,
                new SelectedRuntimeVariantPlayerAdapterRequest()));

        Assert.Contains("integrity validation failed", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RejectsRerunFinalStateHashMismatch()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new SelectedRuntimeVariantPlayerAdapterService(new MismatchedRuntimeService())
                .BuildAndWriteAsync(ProjectRoot()));

        Assert.Contains("final state hash", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class MismatchedRuntimeService : IRuntimeBackedPlayerCommandRoundtripService
    {
        public RuntimeBackedPlayerCommandRoundtripResult Execute(
            GamePackageDefinition package,
            RuntimeBackedPlayerCommandRoundtripRequest request)
        {
            _ = package;
            return new RuntimeBackedPlayerCommandRoundtripResult
            {
                CandidateId = request.CandidateId,
                StateHashChain = ["mismatched-final-state-hash"]
            };
        }
    }

    private sealed class TempSelectedSource : IDisposable
    {
        private readonly string _sourceRoot;

        public TempSelectedSource(string sourceRoot)
        {
            _sourceRoot = sourceRoot;
            Root = Path.Combine(
                Path.GetTempPath(),
                "LLMGameCreator.Tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
            File.WriteAllText(Path.Combine(Root, "LLMGameCreator.sln"), string.Empty);
            foreach (var path in RequiredSourceFiles())
            {
                var source = Path.Combine(_sourceRoot, path.Replace('/', Path.DirectorySeparatorChar));
                var target = Path.Combine(Root, path.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(source, target);
            }
        }

        public string Root { get; }
        public string SelectedPackagePath => Path.Combine(
            Root,
            SelectedRuntimeVariantPlayerAdapterVocabulary.SourcePackagePath
                .Replace('/', Path.DirectorySeparatorChar));

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }

        private static IReadOnlyList<string> RequiredSourceFiles()
        {
            var goal142 = SelectedRuntimeVariantPlayerAdapterVocabulary.SourceGoal142Root;
            var candidate = SelectedRuntimeVariantPlayerAdapterVocabulary.CandidateId;
            return
            [
                SelectedRuntimeVariantPlayerAdapterVocabulary.SourceSelectedHandoffPath,
                SelectedRuntimeVariantPlayerAdapterVocabulary.SourcePackagePath,
                SelectedRuntimeVariantPlayerAdapterVocabulary.SourceOutcomePath,
                SelectedRuntimeVariantPlayerAdapterVocabulary.SourceRoundtripResultPath,
                goal142 + "/matrix/" + candidate + "/runtime-outcome-summary.json",
                goal142 + "/candidates/" + candidate + "/package.json"
            ];
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
