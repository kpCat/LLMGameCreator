using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed record GeneratedProjectBaseline
{
    public string PackagePath { get; init; } = string.Empty;
    public byte[] PackageBytes { get; init; } = [];
    public string PackageJson { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string CandidateId { get; init; } = string.Empty;
    public string SourceIdentity { get; init; } = string.Empty;
}

public interface IGeneratedProjectBaselineProvider
{
    GeneratedProjectBaseline Resolve();
}

public sealed class Goal142GeneratedProjectBaselineProvider : IGeneratedProjectBaselineProvider
{
    private readonly string _repositoryRoot;

    public Goal142GeneratedProjectBaselineProvider(string repositoryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);
        _repositoryRoot = Path.GetFullPath(repositoryRoot);
    }

    public GeneratedProjectBaseline Resolve()
    {
        var matrixPath = Confined(
            _repositoryRoot,
            FeatureModuleCompositionVocabulary.Goal142Root + "/"
            + ProductLineRuntimeVariantMatrixVocabulary.MatrixResultFileName);
        using var matrix = JsonDocument.Parse(File.ReadAllText(matrixPath, Encoding.UTF8));
        var row = matrix.RootElement.GetProperty("candidates").EnumerateArray()
            .Single(candidate => string.Equals(
                candidate.GetProperty("candidateId").GetString(),
                FeatureModuleCompositionVocabulary.BaselineCandidateId,
                StringComparison.Ordinal));
        var relativePackagePath = row.GetProperty("packagePath").GetString();
        var expectedSha256 = row.GetProperty("packageSha256").GetString();
        if (string.IsNullOrWhiteSpace(relativePackagePath) || string.IsNullOrWhiteSpace(expectedSha256))
            throw new InvalidOperationException("generated_source.baseline_unavailable");

        var packagePath = Confined(_repositoryRoot, relativePackagePath);
        if (!File.Exists(packagePath))
            throw new InvalidOperationException("generated_source.baseline_unavailable");
        var bytes = File.ReadAllBytes(packagePath);
        var actualSha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        if (!string.Equals(actualSha256, expectedSha256, StringComparison.Ordinal))
            throw new InvalidOperationException("generated_source.baseline_hash_mismatch");

        return new GeneratedProjectBaseline
        {
            PackagePath = packagePath,
            PackageBytes = bytes,
            PackageJson = Encoding.UTF8.GetString(bytes),
            PackageSha256 = actualSha256,
            CandidateId = FeatureModuleCompositionVocabulary.BaselineCandidateId,
            SourceIdentity = Path.GetRelativePath(_repositoryRoot, matrixPath).Replace('\\', '/')
                             + "#" + FeatureModuleCompositionVocabulary.BaselineCandidateId
        };
    }

    private static string Confined(string root, string relative)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(fullRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!path.Equals(fullRoot, comparison)
            && !path.StartsWith(fullRoot + Path.DirectorySeparatorChar, comparison))
            throw new InvalidOperationException("generated_source.baseline_unavailable");
        return path;
    }
}
