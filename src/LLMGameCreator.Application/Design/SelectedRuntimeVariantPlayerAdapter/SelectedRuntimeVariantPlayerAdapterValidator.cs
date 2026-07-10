using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.SelectedRuntimeVariantPlayerAdapter;

public sealed record SelectedRuntimeVariantPlayerAdapterSourceValidation
{
    public bool SelectedCandidateIdMatches { get; init; }
    public bool SelectedRecipeIdMatches { get; init; }
    public bool SelectedVariantKindMatches { get; init; }
    public bool SelectedScoreMatches { get; init; }
    public bool SelectedHandoffAccepted { get; init; }
    public bool SelectedRuntimeSignificant { get; init; }
    public bool SelectedProjectionOnly { get; init; }
    public bool SelectedRuntimeAuthority { get; init; }
    public bool SelectedPackageExists { get; init; }
    public bool SelectedPackageSha256MatchesHandoff { get; init; }
    public bool SelectedRoundtripResultExists { get; init; }
    public bool SelectedOutcomeExists { get; init; }
    public bool SelectedOutcomeCandidateMatches { get; init; }
    public bool SelectedRoundtripCandidateMatches { get; init; }
    public bool SelectedFinalStateHashMatches { get; init; }
    public bool SourcePathsConsistent { get; init; }
    public bool NoBalancedBaselineFallback { get; init; }
    public bool Passed { get; init; }
}

public sealed record SelectedRuntimeVariantPlayerAdapterValidatedInput
{
    public string RepositoryRoot { get; init; } = string.Empty;
    public string OutputRoot { get; init; } = string.Empty;
    public string HandoffPath { get; init; } = string.Empty;
    public string PackagePath { get; init; } = string.Empty;
    public string OutcomePath { get; init; } = string.Empty;
    public string RoundtripResultPath { get; init; } = string.Empty;
    public string UnitySmokePath { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public GamePackageDefinition Package { get; init; } = new();
    public ProductLineRuntimeVariantSelectedHandoff SourceHandoff { get; init; } = new();
    public ProductLineRuntimeVariantRuntimeOutcomeSummary SourceOutcome { get; init; } = new();
    public RuntimeBackedPlayerCommandRoundtripResult SourceRoundtrip { get; init; } = new();
    public SelectedRuntimeVariantPlayerAdapterSourceValidation Validation { get; init; } = new();
}

public sealed class SelectedRuntimeVariantPlayerAdapterValidator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    static SelectedRuntimeVariantPlayerAdapterValidator()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    private readonly SelectedRuntimeVariantPlayerAdapterArtifactService _artifacts;

    public SelectedRuntimeVariantPlayerAdapterValidator(
        SelectedRuntimeVariantPlayerAdapterArtifactService? artifacts = null)
    {
        _artifacts = artifacts ?? new SelectedRuntimeVariantPlayerAdapterArtifactService();
    }

    public SelectedRuntimeVariantPlayerAdapterValidatedInput Validate(
        string repositoryRootPath,
        SelectedRuntimeVariantPlayerAdapterRequest request)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var goal142Root = Resolve(root, SelectedRuntimeVariantPlayerAdapterVocabulary.SourceGoal142Root);
        var selectedRoot = Path.Combine(goal142Root, "selected-runtime-variant");
        var expectedRoundtripRoot = Path.Combine(
            goal142Root,
            "matrix",
            SelectedRuntimeVariantPlayerAdapterVocabulary.CandidateId);
        var goal143Root = Resolve(
            root,
            SelectedRuntimeVariantPlayerAdapterVocabulary.ProceduralOutputDirectory);

        var handoffPath = ResolveInput(root, request.SelectedHandoffPath, "SelectedHandoffPath");
        var packagePath = ResolveInput(root, request.SelectedPackagePath, "SelectedPackagePath");
        var outcomePath = ResolveInput(root, request.SelectedOutcomePath, "SelectedOutcomePath");
        var roundtripPath = ResolveInput(
            root,
            request.SelectedRoundtripResultPath,
            "SelectedRoundtripResultPath");
        var outputRoot = Resolve(root, request.OutputRoot);
        var unitySmokePath = Resolve(root, request.UnitySmokePath);

        GuardUnder(handoffPath, selectedRoot, "SelectedHandoffPath");
        GuardUnder(packagePath, selectedRoot, "SelectedPackagePath");
        GuardUnder(outcomePath, goal142Root, "SelectedOutcomePath");
        GuardUnder(roundtripPath, expectedRoundtripRoot, "SelectedRoundtripResultPath");
        GuardUnder(outputRoot, goal143Root, "OutputRoot");
        GuardUnder(unitySmokePath, goal143Root, "UnitySmokePath");

        var handoff = _artifacts.ReadJson<ProductLineRuntimeVariantSelectedHandoff>(handoffPath);
        var outcome = _artifacts.ReadJson<ProductLineRuntimeVariantRuntimeOutcomeSummary>(outcomePath);
        var roundtrip = _artifacts.ReadJson<RuntimeBackedPlayerCommandRoundtripResult>(roundtripPath);
        var packageJson = File.ReadAllText(packagePath, Encoding.UTF8);
        var package = JsonSerializer.Deserialize<GamePackageDefinition>(packageJson, JsonOptions)
                      ?? throw new InvalidOperationException(
                          "Selected Goal142 package could not be deserialized.");
        var packageHash = SelectedRuntimeVariantPlayerAdapterArtifactService.HashFile(packagePath);

        var handoffOutcomePath = ResolveInput(
            root,
            handoff.RuntimeOutcomeSummaryPath,
            "Goal142 handoff runtimeOutcomeSummaryPath");
        var handoffPackagePath = ResolveInput(
            root,
            handoff.PackagePath,
            "Goal142 handoff packagePath");
        var handoffRoundtripPath = ResolveInput(
            root,
            handoff.RoundtripResultPath,
            "Goal142 handoff roundtripResultPath");
        var sourceRoundtripPackagePath = ResolveInput(
            root,
            roundtrip.Inputs.PackagePath,
            "Goal142 roundtrip packagePath");

        var finalHash = FinalHash(roundtrip);
        var candidateMatches = handoff.CandidateId ==
                               SelectedRuntimeVariantPlayerAdapterVocabulary.CandidateId;
        var recipeMatches = handoff.RecipeId ==
                            SelectedRuntimeVariantPlayerAdapterVocabulary.RecipeId;
        var variantMatches = handoff.VariantKind ==
                             SelectedRuntimeVariantPlayerAdapterVocabulary.VariantKind;
        var scoreMatches = handoff.Score == SelectedRuntimeVariantPlayerAdapterVocabulary.Score;
        var packageHashMatches = packageHash == handoff.PackageSha256
                                 && SelectedRuntimeVariantPlayerAdapterArtifactService.HashFile(
                                     handoffPackagePath) == packageHash
                                 && SelectedRuntimeVariantPlayerAdapterArtifactService.HashFile(
                                     sourceRoundtripPackagePath) == packageHash;
        var outcomeCandidateMatches = outcome.CandidateId == handoff.CandidateId
                                      && outcome.RecipeId == handoff.RecipeId
                                      && outcome.VariantKind == handoff.VariantKind;
        var roundtripCandidateMatches = roundtrip.CandidateId == handoff.CandidateId;
        var finalHashMatches = !string.IsNullOrWhiteSpace(finalHash)
                               && finalHash == handoff.FinalStateHash
                               && finalHash == outcome.FinalStateHash;
        var sourcePathsConsistent = SamePath(handoffPackagePath, packagePath)
                                    && SamePath(handoffRoundtripPath, roundtripPath)
                                    && SameFileBytes(handoffOutcomePath, outcomePath);
        var noFallback = !Relative(root, packagePath).Contains(
                             "minimal-map-game-balanced-baseline",
                             StringComparison.Ordinal)
                         && !Relative(root, packagePath).Contains(
                             "goal-131-gamepackage-candidate",
                             StringComparison.Ordinal)
                         && !Relative(root, packagePath).StartsWith(
                             "samples/minimal-map-game/",
                             StringComparison.Ordinal);

        var validation = new SelectedRuntimeVariantPlayerAdapterSourceValidation
        {
            SelectedCandidateIdMatches = candidateMatches,
            SelectedRecipeIdMatches = recipeMatches,
            SelectedVariantKindMatches = variantMatches,
            SelectedScoreMatches = scoreMatches,
            SelectedHandoffAccepted = handoff.Accepted,
            SelectedRuntimeSignificant = handoff.RuntimeSignificant,
            SelectedProjectionOnly = handoff.ProjectionOnly,
            SelectedRuntimeAuthority = handoff.RuntimeAuthority,
            SelectedPackageExists = File.Exists(packagePath),
            SelectedPackageSha256MatchesHandoff = packageHashMatches,
            SelectedRoundtripResultExists = File.Exists(roundtripPath),
            SelectedOutcomeExists = File.Exists(outcomePath),
            SelectedOutcomeCandidateMatches = outcomeCandidateMatches,
            SelectedRoundtripCandidateMatches = roundtripCandidateMatches,
            SelectedFinalStateHashMatches = finalHashMatches,
            SourcePathsConsistent = sourcePathsConsistent,
            NoBalancedBaselineFallback = noFallback,
            Passed = candidateMatches
                     && recipeMatches
                     && variantMatches
                     && scoreMatches
                     && !handoff.Accepted
                     && handoff.RuntimeSignificant
                     && !handoff.ProjectionOnly
                     && handoff.RuntimeAuthority
                     && packageHashMatches
                     && outcomeCandidateMatches
                     && roundtripCandidateMatches
                     && finalHashMatches
                     && sourcePathsConsistent
                     && noFallback
        };
        if (!validation.Passed)
        {
            throw new InvalidOperationException(
                "Goal142 selected runtime variant integrity validation failed: "
                + JsonSerializer.Serialize(validation, JsonOptions));
        }

        return new SelectedRuntimeVariantPlayerAdapterValidatedInput
        {
            RepositoryRoot = root,
            OutputRoot = outputRoot,
            HandoffPath = handoffPath,
            PackagePath = packagePath,
            OutcomePath = outcomePath,
            RoundtripResultPath = roundtripPath,
            UnitySmokePath = unitySmokePath,
            PackageSha256 = packageHash,
            Package = package,
            SourceHandoff = handoff,
            SourceOutcome = outcome,
            SourceRoundtrip = roundtrip,
            Validation = validation
        };
    }

    private static string ResolveRepositoryRoot(string path)
    {
        var root = Path.GetFullPath(path);
        if (!File.Exists(Path.Combine(root, "LLMGameCreator.sln")))
        {
            throw new InvalidOperationException("Repository root was not found: " + path);
        }

        return root;
    }

    private static string ResolveInput(string root, string path, string name)
    {
        var full = Resolve(root, path);
        GuardUnder(full, root, name);
        GuardNotManual(root, full, name);
        if (!File.Exists(full))
        {
            throw new FileNotFoundException(name + " was not found.", full);
        }

        return full;
    }

    private static string Resolve(string root, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException("Path is required.");
        }

        return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));
    }

    private static void GuardUnder(string path, string directory, string name)
    {
        if (!IsUnder(path, directory))
        {
            throw new InvalidOperationException(name + " must stay under its allowed repository root.");
        }
    }

    private static void GuardNotManual(string root, string path, string name)
    {
        if (Relative(root, path).StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Goal143 refuses .llmgc/manual path for " + name + ".");
        }
    }

    private static bool IsUnder(string path, string directory)
    {
        var fullPath = Path.GetFullPath(path);
        var fullDirectory = Path.GetFullPath(directory);
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return fullPath.Equals(fullDirectory, comparison)
               || fullPath.StartsWith(
                   fullDirectory.TrimEnd(
                       Path.DirectorySeparatorChar,
                       Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar,
                   comparison);
    }

    private static bool SamePath(string left, string right) =>
        string.Equals(
            Path.GetFullPath(left),
            Path.GetFullPath(right),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    private static bool SameFileBytes(string left, string right) =>
        SelectedRuntimeVariantPlayerAdapterArtifactService.HashFile(left)
        == SelectedRuntimeVariantPlayerAdapterArtifactService.HashFile(right);

    private static string FinalHash(RuntimeBackedPlayerCommandRoundtripResult result) =>
        result.StateHashChain.LastOrDefault()
        ?? result.Snapshots.LastOrDefault()?.StateHashAfter
        ?? string.Empty;

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');
}
