using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.ProductLineInteractiveSessionMatrix;

public sealed record Goal142DiscoveredCandidate
{
    public ProductLineInteractiveSessionCandidate Candidate { get; init; } = new();
    public string FullPackagePath { get; init; } = string.Empty;
    public GamePackageDefinition Package { get; init; } = new();
}

public sealed record Goal142CandidateDiscoveryResult
{
    public string RepositoryRoot { get; init; } = string.Empty;
    public string Goal142Root { get; init; } = string.Empty;
    public string DefaultSelectedCandidateId { get; init; } = string.Empty;
    public IReadOnlyList<Goal142DiscoveredCandidate> Candidates { get; init; } = [];
}

public sealed class Goal142CandidateDiscovery
{
    private static readonly JsonSerializerOptions PackageOptions = CreatePackageOptions();

    public Goal142CandidateDiscoveryResult Discover(
        string repositoryRootPath,
        string goal142RootPath)
    {
        var repositoryRoot = Path.GetFullPath(repositoryRootPath);
        if (!File.Exists(Path.Combine(repositoryRoot, "LLMGameCreator.sln")))
        {
            throw new InvalidOperationException("Repository root was not found.");
        }

        var goal142Root = Resolve(repositoryRoot, goal142RootPath);
        GuardUnder(goal142Root, repositoryRoot, "Goal142Root");
        GuardNotManual(repositoryRoot, goal142Root, "Goal142Root");
        if (!Directory.Exists(goal142Root))
        {
            throw new DirectoryNotFoundException("Goal142 artifact root was not found: " + goal142Root);
        }

        using var matrix = Read(Path.Combine(goal142Root, "product-line-runtime-variant-matrix-result.json"));
        using var catalog = Read(Path.Combine(goal142Root, "product-line-runtime-variant-catalog.json"));
        using var scoreboard = Read(Path.Combine(goal142Root, "product-line-runtime-variant-scoreboard.json"));
        using var selected = Read(Path.Combine(goal142Root, "selected-runtime-variant", "selected-runtime-variant-handoff.json"));

        var catalogRows = catalog.RootElement.GetProperty("variants").EnumerateArray()
            .ToDictionary(row => Text(row, "candidateId"), StringComparer.Ordinal);
        var scoreRows = scoreboard.RootElement.GetProperty("scores").EnumerateArray()
            .ToDictionary(row => Text(row, "candidateId"), StringComparer.Ordinal);
        var matrixRows = matrix.RootElement.GetProperty("candidates").EnumerateArray().ToList();
        AssertNoDuplicate(matrixRows.Select(row => Text(row, "candidateId")), "candidate ID");
        AssertNoDuplicate(matrixRows.Select(row => Text(row, "packagePath")), "package path");

        var candidates = new List<Goal142DiscoveredCandidate>();
        foreach (var row in matrixRows.OrderBy(row => Text(row, "candidateId"), StringComparer.Ordinal))
        {
            var candidateId = Text(row, "candidateId");
            if (!catalogRows.TryGetValue(candidateId, out var catalogRow)
                || !scoreRows.TryGetValue(candidateId, out var scoreRow))
            {
                throw new InvalidOperationException("Goal142 candidate metadata is incomplete: " + candidateId);
            }

            var candidatePath = Resolve(repositoryRoot, Text(row, "packagePath"));
            var expectedCandidateDirectory = Path.Combine(goal142Root, "candidates", candidateId);
            GuardUnder(candidatePath, expectedCandidateDirectory, "candidate package path");
            var expectedPackagePath = Path.Combine(expectedCandidateDirectory, "package.json");
            if (!candidatePath.Equals(expectedPackagePath, Comparison))
            {
                throw new InvalidOperationException("Goal142 candidate package path metadata mismatch: " + candidateId);
            }

            if (!File.Exists(candidatePath))
            {
                throw new FileNotFoundException("Goal142 candidate package is missing.", candidatePath);
            }

            using var handoff = Read(Path.Combine(expectedCandidateDirectory, "candidate-handoff.json"));
            var recipeId = Text(row, "recipeId");
            var variantKind = Text(row, "variantKind");
            var packageHash = HashFile(candidatePath);
            var expectedHash = Text(row, "packageSha256");
            if (packageHash != expectedHash)
            {
                throw new InvalidOperationException("Goal142 candidate package SHA mismatch: " + candidateId);
            }

            var metadataMatches = Text(catalogRow, "recipeId") == recipeId
                                  && Text(catalogRow, "variantKind") == variantKind
                                  && Text(scoreRow, "recipeId") == recipeId
                                  && Text(scoreRow, "variantKind") == variantKind
                                  && Text(handoff.RootElement, "candidateId") == candidateId
                                  && Text(handoff.RootElement, "recipeId") == recipeId
                                  && Text(handoff.RootElement, "variantKind") == variantKind
                                  && Resolve(repositoryRoot, Text(handoff.RootElement, "packagePath")) == candidatePath
                                  && Bool(row, "passed")
                                  && Bool(scoreRow, "eligible")
                                  && Bool(handoff.RootElement, "runtimeSignificant")
                                  && Bool(catalogRow, "runtimeSignificant");
            if (!metadataMatches || !PackageMetadataMatches(candidatePath, candidateId, recipeId, variantKind))
            {
                throw new InvalidOperationException("Goal142 candidate metadata mismatch: " + candidateId);
            }

            var package = JsonSerializer.Deserialize<GamePackageDefinition>(
                              File.ReadAllText(candidatePath),
                              PackageOptions)
                          ?? throw new InvalidOperationException("Goal142 candidate package did not parse: " + candidateId);
            candidates.Add(new Goal142DiscoveredCandidate
            {
                Candidate = new ProductLineInteractiveSessionCandidate
                {
                    CandidateId = candidateId,
                    RecipeId = recipeId,
                    VariantKind = variantKind,
                    Score = Number(scoreRow, "score"),
                    PackagePath = Relative(repositoryRoot, candidatePath),
                    PackageSha256 = packageHash,
                    ControlCandidate = variantKind == "balanced_baseline",
                    RuntimeMutated = variantKind != "balanced_baseline",
                    Passed = true
                },
                FullPackagePath = candidatePath,
                Package = package
            });
        }

        var defaultSelectedCandidateId = Text(selected.RootElement, "candidateId");
        if (!candidates.Any(candidate => candidate.Candidate.CandidateId == defaultSelectedCandidateId
                                         && candidate.Candidate.Passed))
        {
            throw new InvalidOperationException("Goal142 selected handoff references an unknown or failed candidate.");
        }

        return new Goal142CandidateDiscoveryResult
        {
            RepositoryRoot = repositoryRoot,
            Goal142Root = goal142Root,
            DefaultSelectedCandidateId = defaultSelectedCandidateId,
            Candidates = candidates
        };
    }

    public static void AssertNoDuplicate(IEnumerable<string> values, string kind)
    {
        var duplicate = values.GroupBy(value => value, StringComparer.Ordinal)
            .FirstOrDefault(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() != 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException("Duplicate or empty Goal142 " + kind + " rejected: " + duplicate.Key);
        }
    }

    public static void GuardUnder(string path, string directory, string name)
    {
        var full = Path.GetFullPath(path);
        var root = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar)
                   + Path.DirectorySeparatorChar;
        if (!full.Equals(root.TrimEnd(Path.DirectorySeparatorChar), Comparison)
            && !full.StartsWith(root, Comparison))
        {
            throw new InvalidOperationException(name + " escaped its allowed root.");
        }
    }

    public static string HashFile(string path)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(sha.ComputeHash(stream)).ToLowerInvariant();
    }

    private static bool PackageMetadataMatches(
        string packagePath,
        string candidateId,
        string recipeId,
        string variantKind)
    {
        using var package = Read(packagePath);
        var profile = package.RootElement.GetProperty("generatedContent").GetProperty("profile");
        using var source = JsonDocument.Parse(Text(profile, "sourceContextJson"));
        return Text(source.RootElement, "candidateId") == candidateId
               && Text(source.RootElement, "recipeId") == recipeId
               && Text(source.RootElement, "variantKind") == variantKind;
    }

    private static JsonDocument Read(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Required Goal142 artifact was not found.", path);
        return JsonDocument.Parse(File.ReadAllText(path));
    }

    private static JsonSerializerOptions CreatePackageOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }

    private static string Text(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) ? value.GetString() ?? string.Empty : string.Empty;

    private static bool Bool(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;

    private static int Number(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.TryGetInt32(out var number) ? number : 0;

    private static void GuardNotManual(string root, string path, string name)
    {
        if (Relative(root, path).StartsWith(".llmgc/manual/", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Goal145 refuses .llmgc/manual path for " + name + ".");
        }
    }

    private static string Resolve(string root, string path) =>
        Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static StringComparison Comparison => OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;
}
