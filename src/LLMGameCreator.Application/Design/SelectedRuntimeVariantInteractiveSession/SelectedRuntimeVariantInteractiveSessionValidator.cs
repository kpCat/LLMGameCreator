using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.SelectedRuntimeVariantInteractiveSession;

public sealed record SelectedRuntimeVariantInteractiveSessionValidatedInput
{
    public string RepositoryRoot { get; init; } = string.Empty;
    public string OutputRoot { get; init; } = string.Empty;
    public string UnitySmokePath { get; init; } = string.Empty;
    public string PackagePath { get; init; } = string.Empty;
    public string PackageRelativePath { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string CandidateId { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public string ExpectedFinalStateHash { get; init; } = string.Empty;
    public GamePackageDefinition Package { get; init; } = new();
}

public sealed class SelectedRuntimeVariantInteractiveSessionValidator
{
    private static readonly JsonSerializerOptions PackageOptions = CreatePackageOptions();

    public SelectedRuntimeVariantInteractiveSessionValidatedInput Validate(
        string repositoryRootPath,
        SelectedRuntimeVariantInteractiveSessionRequest request)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        if (!File.Exists(Path.Combine(root, "LLMGameCreator.sln")))
        {
            throw new InvalidOperationException("Repository root was not found.");
        }

        var handoffPath = Input(root, request.SelectedHandoffPath, "SelectedHandoffPath");
        var packagePath = Input(root, request.SelectedPackagePath, "SelectedPackagePath");
        var outcomePath = Input(root, request.SelectedOutcomePath, "SelectedOutcomePath");
        var goal143Path = Input(root, request.Goal143HandoffPath, "Goal143HandoffPath");
        var outputRoot = Resolve(root, request.OutputRoot);
        var expectedOutputRoot = Resolve(
            root,
            SelectedRuntimeVariantInteractiveSessionVocabulary.ProceduralOutputDirectory);
        GuardUnder(outputRoot, expectedOutputRoot, "OutputRoot");
        var unitySmokePath = Resolve(root, request.UnitySmokePath);
        GuardUnder(unitySmokePath, expectedOutputRoot, "UnitySmokePath");
        GuardNotManual(root, outputRoot, "OutputRoot");

        using var handoff = JsonDocument.Parse(File.ReadAllText(handoffPath));
        using var outcome = JsonDocument.Parse(File.ReadAllText(outcomePath));
        using var goal143 = JsonDocument.Parse(File.ReadAllText(goal143Path));
        var selected = handoff.RootElement;
        var selectedOutcome = outcome.RootElement;
        var playerAdapter = goal143.RootElement;
        var packageHash = SelectedRuntimeVariantInteractiveSessionArtifactService.HashFile(packagePath);
        var packageRelative = Relative(root, packagePath);
        var candidate = String(selected, "candidateId");
        var variant = String(selected, "variantKind");
        var finalHash = String(selected, "finalStateHash");
        var valid = candidate == SelectedRuntimeVariantInteractiveSessionVocabulary.CandidateId
                    && variant == SelectedRuntimeVariantInteractiveSessionVocabulary.VariantKind
                    && Int(selected, "score") == 100
                    && Bool(selected, "runtimeSignificant")
                    && !Bool(selected, "projectionOnly")
                    && Bool(selected, "runtimeAuthority")
                    && String(selected, "packagePath") == packageRelative
                    && String(selected, "packageSha256") == packageHash
                    && packageHash == SelectedRuntimeVariantInteractiveSessionVocabulary.ExpectedPackageSha256
                    && finalHash == SelectedRuntimeVariantInteractiveSessionVocabulary.ExpectedFinalStateHash
                    && String(selectedOutcome, "candidateId") == candidate
                    && String(selectedOutcome, "finalStateHash") == finalHash
                    && String(playerAdapter, "candidateId") == candidate
                    && String(playerAdapter, "variantKind") == variant
                    && String(playerAdapter, "sourcePackageSha256") == packageHash
                    && String(playerAdapter, "finalStateHash") == finalHash
                    && Bool(playerAdapter, "selectedPackageSha256MatchesHandoff")
                    && Bool(playerAdapter, "selectedFinalStateHashMatches")
                    && Bool(playerAdapter, "runtimeAuthority")
                    && !Bool(playerAdapter, "projectionOnly")
                    && !Bool(playerAdapter, "unityGameplayTruth")
                    && NoFallback(packageRelative);
        if (!valid)
        {
            throw new InvalidOperationException(
                "Goal144 selected Goal142/Goal143 package, hash or final-state integrity failed.");
        }

        var package = JsonSerializer.Deserialize<GamePackageDefinition>(
                          File.ReadAllText(packagePath),
                          PackageOptions)
                      ?? throw new InvalidOperationException("Goal144 selected package did not parse.");
        return new SelectedRuntimeVariantInteractiveSessionValidatedInput
        {
            RepositoryRoot = root,
            OutputRoot = outputRoot,
            UnitySmokePath = unitySmokePath,
            PackagePath = packagePath,
            PackageRelativePath = packageRelative,
            PackageSha256 = packageHash,
            CandidateId = candidate,
            VariantKind = variant,
            ExpectedFinalStateHash = finalHash,
            Package = package
        };
    }

    private static JsonSerializerOptions CreatePackageOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }

    private static string Input(string root, string path, string name)
    {
        var full = Resolve(root, path);
        GuardUnder(full, root, name);
        GuardNotManual(root, full, name);
        if (!File.Exists(full)) throw new FileNotFoundException(name + " was not found.", full);
        return full;
    }

    private static bool NoFallback(string path) =>
        !path.Contains("minimal-map-game-balanced-baseline", StringComparison.Ordinal)
        && !path.Contains("goal-131", StringComparison.Ordinal)
        && !path.StartsWith("samples/minimal-map-game/", StringComparison.Ordinal);

    private static string String(JsonElement value, string name) =>
        value.TryGetProperty(name, out var property) ? property.GetString() ?? string.Empty : string.Empty;

    private static bool Bool(JsonElement value, string name) =>
        value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.True;

    private static int Int(JsonElement value, string name) =>
        value.TryGetProperty(name, out var property) && property.TryGetInt32(out var result) ? result : 0;

    private static string Resolve(string root, string path) =>
        Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));

    private static void GuardUnder(string path, string directory, string name)
    {
        var root = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar)
                   + Path.DirectorySeparatorChar;
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        var full = Path.GetFullPath(path);
        if (!full.Equals(root.TrimEnd(Path.DirectorySeparatorChar), comparison)
            && !full.StartsWith(root, comparison))
        {
            throw new InvalidOperationException(name + " must stay under its allowed repository root.");
        }
    }

    private static void GuardNotManual(string root, string path, string name)
    {
        if (Relative(root, path).StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Goal144 refuses .llmgc/manual path for " + name + ".");
        }
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');
}
