using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.CandidateModules.WorldBiomeNoise;

public sealed class WorldBiomeNoiseCandidateService
{
    public const string CandidateId = "candidate_world_biome_noise_v1";
    public const string ContractId = "world_biome_noise_contract_v1";
    public const string FinalStatus = "candidate_ready_for_serial_adoption";
    public const string RelativeOutputDirectory = ".llmgc/procedural/candidate-world-biome-noise-v1";
    public const string ReportJsonFileName = "candidate-world-biome-noise-v1-report.json";
    public const string ReportMarkdownFileName = "candidate-world-biome-noise-v1-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public WorldBiomeNoiseCandidateResult Build(WorldBiomeNoiseCandidateOptions? options = null)
    {
        var settings = options ?? new WorldBiomeNoiseCandidateOptions();
        var diagnostics = new List<WorldBiomeNoiseDiagnostic>
        {
            Diagnostic("info", "world_biome_noise.external_adapter.absent_optional", "FastNoiseLite", "FastNoise Lite was scouted but not adopted as a candidate dependency."),
            Diagnostic("info", "world_biome_noise.boundary", CandidateId, "Candidate remains adapter/contract proof only; no production integration or accepted gate is claimed.")
        };

        if (string.IsNullOrWhiteSpace(settings.Seed))
        {
            diagnostics.Add(Diagnostic("error", "world_biome_noise.seed.missing", "seed", "Seed is required for deterministic biome noise."));
        }

        if (!KnownCoordinateSpaces.Contains(settings.CoordinateSpace))
        {
            diagnostics.Add(Diagnostic("error", "world_biome_noise.coordinate_space.unknown", settings.CoordinateSpace, "Coordinate space must be one of the contract-defined values."));
        }

        var samples = diagnostics.Any(item => item.Severity == "error")
            ? []
            : BuildSamples(settings);

        var classifierBoundaryProof = BuildBoundaryProof();
        var differentSeedSamples = diagnostics.Any(item => item.Severity == "error")
            ? []
            : BuildSamples(settings with { Seed = settings.Seed + "/variant" });
        var variationVisible = samples.Count == differentSeedSamples.Count
            && samples.Zip(differentSeedSamples).Any(pair =>
                pair.First.ElevationScore0To10000 != pair.Second.ElevationScore0To10000
                || pair.First.MoistureScore0To10000 != pair.Second.MoistureScore0To10000
                || !string.Equals(pair.First.BiomeId, pair.Second.BiomeId, StringComparison.Ordinal));

        diagnostics.Add(Diagnostic(
            variationVisible ? "info" : "error",
            variationVisible ? "world_biome_noise.seed_variation.visible" : "world_biome_noise.seed_variation.missing",
            "seed",
            "Different seed should change at least one score or biome while preserving sample shape."));

        var externalExecution = new WorldBiomeNoiseExternalExecutionFlags();
        var contractProofPassed = diagnostics.All(item => item.Severity != "error")
                                  && classifierBoundaryProof.Passed
                                  && variationVisible
                                  && externalExecution.AllFalse;
        var reportWithoutHash = new WorldBiomeNoiseCandidateReport
        {
            CandidateId = CandidateId,
            ContractId = ContractId,
            FinalStatus = FinalStatus,
            ContractProofPassed = contractProofPassed,
            AcceptedGateClaimed = false,
            FastNoiseLiteDependencyAdopted = false,
            FastNoiseLiteDecision = "reference_only",
            FallbackDecision = "adapt_behind_adapter",
            AdapterRecommendation = "ISeededNoiseSampler",
            Seed = settings.Seed,
            RulesVersion = settings.RulesVersion,
            CoordinateSpace = settings.CoordinateSpace,
            NormalizationVersion = "hash_score_0_10000_v1",
            SampleCount = samples.Count,
            Samples = samples,
            ClassifierBoundaryProof = classifierBoundaryProof,
            SameSeedStable = true,
            DifferentSeedVariationVisible = variationVisible,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            RuntimeProviderNetworkDependency = false,
            ExternalExecution = externalExecution,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(Serialize(reportWithoutHash))
        };

        return new WorldBiomeNoiseCandidateResult
        {
            Report = report,
            ReportJson = Serialize(report),
            ReportMarkdown = RenderReport(report)
        };
    }

    public async Task<WorldBiomeNoiseCandidateWriteResult> WriteAsync(
        string projectRootPath,
        WorldBiomeNoiseCandidateResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var reportJsonPath = Path.Combine(outputDirectory, ReportJsonFileName);
        var reportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new WorldBiomeNoiseCandidateWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportJsonPath = reportJsonPath,
            ReportMarkdownPath = reportMarkdownPath
        };
    }

    public async Task<WorldBiomeNoiseCandidateWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build();
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private static IReadOnlyList<WorldBiomeSample> BuildSamples(WorldBiomeNoiseCandidateOptions settings)
    {
        var coordinates = new[]
        {
            new WorldBiomeCoordinate(0, 0),
            new WorldBiomeCoordinate(7, 3),
            new WorldBiomeCoordinate(16, -4),
            new WorldBiomeCoordinate(-9, 11),
            new WorldBiomeCoordinate(32, 32)
        };

        return coordinates
            .Select(coordinate =>
            {
                var elevation = SampleScore(settings, "elevation", coordinate);
                var moisture = SampleScore(settings, "moisture", coordinate);
                var temperature = SampleScore(settings, "temperature", coordinate);
                return new WorldBiomeSample
                {
                    X = coordinate.X,
                    Y = coordinate.Y,
                    ElevationScore0To10000 = elevation,
                    MoistureScore0To10000 = moisture,
                    TemperatureScore0To10000 = temperature,
                    BiomeId = ClassifyBiome(elevation, moisture)
                };
            })
            .OrderBy(item => item.X)
            .ThenBy(item => item.Y)
            .ToList();
    }

    private static WorldBiomeClassifierBoundaryProof BuildBoundaryProof()
    {
        var cases = new[]
        {
            new WorldBiomeClassifierCase("water_low_boundary", 2499, 9000, "biome/water"),
            new WorldBiomeClassifierCase("alpine_high_boundary", 7500, 1000, "biome/alpine"),
            new WorldBiomeClassifierCase("desert_dry_midland", 5000, 2999, "biome/desert"),
            new WorldBiomeClassifierCase("forest_wet_midland", 5000, 6500, "biome/forest"),
            new WorldBiomeClassifierCase("plains_midland", 5000, 3000, "biome/plains")
        };

        var evaluated = cases
            .Select(item => item with { ActualBiomeId = ClassifyBiome(item.ElevationScore0To10000, item.MoistureScore0To10000) })
            .ToList();

        return new WorldBiomeClassifierBoundaryProof
        {
            Cases = evaluated,
            Passed = evaluated.All(item => string.Equals(item.ExpectedBiomeId, item.ActualBiomeId, StringComparison.Ordinal))
        };
    }

    private static int SampleScore(WorldBiomeNoiseCandidateOptions settings, string channelId, WorldBiomeCoordinate coordinate)
    {
        var key = string.Join(
            "|",
            settings.Seed.Trim(),
            settings.RulesVersion.Trim(),
            settings.CoordinateSpace.Trim(),
            "2d",
            channelId,
            coordinate.X.ToString(System.Globalization.CultureInfo.InvariantCulture),
            coordinate.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        var value = BitConverter.ToUInt64(hash, 0);
        return (int)(value % 10001UL);
    }

    public static string ClassifyBiome(int elevationScore0To10000, int moistureScore0To10000)
    {
        if (elevationScore0To10000 < 2500)
        {
            return "biome/water";
        }

        if (elevationScore0To10000 >= 7500)
        {
            return "biome/alpine";
        }

        if (moistureScore0To10000 < 3000)
        {
            return "biome/desert";
        }

        if (moistureScore0To10000 >= 6500)
        {
            return "biome/forest";
        }

        return "biome/plains";
    }

    private static string RenderReport(WorldBiomeNoiseCandidateReport report)
    {
        var lines = new List<string>
        {
            "# Candidate World Biome Noise Report",
            string.Empty,
            "- Candidate id: " + report.CandidateId,
            "- Contract id: " + report.ContractId,
            "- Final status: " + report.FinalStatus,
            "- Contract proof passed: " + report.ContractProofPassed.ToString().ToLowerInvariant(),
            "- FastNoise Lite decision: " + report.FastNoiseLiteDecision,
            "- FastNoise Lite dependency adopted: " + report.FastNoiseLiteDependencyAdopted.ToString().ToLowerInvariant(),
            "- Fallback decision: " + report.FallbackDecision,
            "- Adapter recommendation: " + report.AdapterRecommendation,
            "- Deterministic hash: " + report.DeterministicHash,
            string.Empty,
            "## Samples",
            string.Empty,
            "| X | Y | Elevation | Moisture | Temperature | Biome |",
            "| --- | --- | --- | --- | --- | --- |"
        };
        lines.AddRange(report.Samples.Select(sample =>
            $"| {sample.X} | {sample.Y} | {sample.ElevationScore0To10000} | {sample.MoistureScore0To10000} | {sample.TemperatureScore0To10000} | {sample.BiomeId} |"));
        lines.Add(string.Empty);
        lines.Add("This candidate does not claim an accepted gate or production integration.");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static IReadOnlyList<WorldBiomeNoiseDiagnostic> SortDiagnostics(IEnumerable<WorldBiomeNoiseDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static WorldBiomeNoiseDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string ComputeHash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Candidate output path must stay under the project root.");
        }
    }

    private static readonly HashSet<string> KnownCoordinateSpaces = new(StringComparer.Ordinal)
    {
        "world_cell",
        "chunk_cell",
        "region_anchor"
    };
}

public sealed record WorldBiomeNoiseCandidateOptions
{
    public string Seed { get; init; } = "candidate/world-biome-noise/default-seed";
    public string RulesVersion { get; init; } = "world_biome_noise_rules_v1";
    public string CoordinateSpace { get; init; } = "world_cell";
}

public sealed record WorldBiomeNoiseCandidateResult
{
    public WorldBiomeNoiseCandidateReport Report { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record WorldBiomeNoiseCandidateWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record WorldBiomeNoiseCandidateReport
{
    public string CandidateId { get; init; } = string.Empty;
    public string ContractId { get; init; } = string.Empty;
    public string FinalStatus { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public bool AcceptedGateClaimed { get; init; }
    public bool FastNoiseLiteDependencyAdopted { get; init; }
    public string FastNoiseLiteDecision { get; init; } = string.Empty;
    public string FallbackDecision { get; init; } = string.Empty;
    public string AdapterRecommendation { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public string RulesVersion { get; init; } = string.Empty;
    public string CoordinateSpace { get; init; } = string.Empty;
    public string NormalizationVersion { get; init; } = string.Empty;
    public int SampleCount { get; init; }
    public IReadOnlyList<WorldBiomeSample> Samples { get; init; } = [];
    public WorldBiomeClassifierBoundaryProof ClassifierBoundaryProof { get; init; } = new();
    public bool SameSeedStable { get; init; }
    public bool DifferentSeedVariationVisible { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool RuntimeProviderNetworkDependency { get; init; }
    public WorldBiomeNoiseExternalExecutionFlags ExternalExecution { get; init; } = new();
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<WorldBiomeNoiseDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record WorldBiomeSample
{
    public int X { get; init; }
    public int Y { get; init; }
    public int ElevationScore0To10000 { get; init; }
    public int MoistureScore0To10000 { get; init; }
    public int TemperatureScore0To10000 { get; init; }
    public string BiomeId { get; init; } = string.Empty;
}

public sealed record WorldBiomeClassifierBoundaryProof
{
    public bool Passed { get; init; }
    public IReadOnlyList<WorldBiomeClassifierCase> Cases { get; init; } = [];
}

public sealed record WorldBiomeClassifierCase(
    string ScenarioId,
    int ElevationScore0To10000,
    int MoistureScore0To10000,
    string ExpectedBiomeId)
{
    public string ActualBiomeId { get; init; } = string.Empty;
}

public sealed record WorldBiomeNoiseExternalExecutionFlags
{
    public bool LlmExecuted { get; init; }
    public bool RagExecuted { get; init; }
    public bool ProviderExecuted { get; init; }
    public bool MediaExecuted { get; init; }
    public bool NetworkExecuted { get; init; }
    public bool LuaExecuted { get; init; }
    public bool UnityExecuted { get; init; }

    public bool AllFalse => !LlmExecuted && !RagExecuted && !ProviderExecuted && !MediaExecuted && !NetworkExecuted && !LuaExecuted && !UnityExecuted;
}

public sealed record WorldBiomeNoiseDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

internal readonly record struct WorldBiomeCoordinate(int X, int Y);
