using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.RuntimePreview;

public sealed class MicrogameVariationAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/generated-microgame-variation";
    public const string ReportJsonFileName = "generated-microgame-variation-report.json";
    public const string ReportMarkdownFileName = "generated-microgame-variation-report.md";
    public const string ManualVerificationMarkdownFileName = "manual-configurable-microgame-verification.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly VisibleGeneratedPlayablePreviewService _visiblePreviewService;
    private readonly RuntimeBackedMicrogameStateAcceptanceService _runtimeBackedStateAcceptanceService;
    private readonly GenerationPresetOptionsService _generationOptionsService;

    public MicrogameVariationAcceptanceService(
        VisibleGeneratedPlayablePreviewService? visiblePreviewService = null,
        RuntimeBackedMicrogameStateAcceptanceService? runtimeBackedStateAcceptanceService = null,
        GenerationPresetOptionsService? generationOptionsService = null)
    {
        _visiblePreviewService = visiblePreviewService ?? new VisibleGeneratedPlayablePreviewService();
        _runtimeBackedStateAcceptanceService = runtimeBackedStateAcceptanceService ?? new RuntimeBackedMicrogameStateAcceptanceService();
        _generationOptionsService = generationOptionsService ?? new GenerationPresetOptionsService();
    }

    public MicrogameVariationAcceptanceResult Build(
        string? projectRootPath = null,
        IReadOnlyList<MicrogameVariationAcceptanceRequest>? variants = null)
    {
        var matrix = variants?.Count > 0 ? variants : BuildDefaultMatrix();
        var acceptedVariants = new List<MicrogameVariationAcceptedVariant>();
        var diagnostics = new List<MicrogameVariationAcceptanceDiagnostic>
        {
            Diagnostic("info", "generated_microgame_variation.no_external_execution", "matrix", "No LLM, provider, Lua, Unity or media execution was invoked."),
            Diagnostic("info", "generated_microgame_variation.manual_verification_required", "manual_configurable_microgame_verification", "Codex acceptance is headless; the next step is manual configurable microgame verification.")
        };

        for (var index = 0; index < matrix.Count; index++)
        {
            var request = matrix[index];
            var generationOptions = _generationOptionsService.Resolve(new GenerationPresetOptionsRequest
            {
                Seed = request.Seed,
                Mode = request.Mode,
                PresetId = request.PresetId,
                CompactStyleHintIds = request.CompactStyleHintIds,
                SelectedVariantIds = request.SelectedVariantIds
            });
            var visibleResult = _visiblePreviewService.Generate(new VisibleGeneratedPlayablePreviewRequest
            {
                Seed = generationOptions.Seed,
                Mode = generationOptions.Mode,
                PresetId = generationOptions.PresetId,
                CompactStyleHintIds = generationOptions.CompactStyleHintIds,
                SelectedVariantIds = generationOptions.SelectedVariantIds
            });
            var runtimeAcceptance = _runtimeBackedStateAcceptanceService.Build(visibleResult, projectRootPath);
            var accepted = BuildVariant(index + 1, generationOptions, visibleResult, runtimeAcceptance);
            acceptedVariants.Add(accepted);
            diagnostics.AddRange(accepted.Diagnostics);
        }

        var differenceSummary = BuildDifferenceSummary(acceptedVariants);
        var allAccepted = acceptedVariants.Count >= 3
            && acceptedVariants.All(item => item.Accepted)
            && differenceSummary.UniqueSeedCount >= 3
            && differenceSummary.UniquePresetCount >= 2
            && differenceSummary.UniquePackageIdCount >= 3;
        diagnostics.Add(Diagnostic(
            allAccepted ? "info" : "error",
            allAccepted ? "generated_microgame_variation.accepted" : "generated_microgame_variation.failed",
            "variant_matrix",
            allAccepted ? "All required deterministic generated microgame variants passed runtime-backed acceptance." : "One or more required generated microgame variants failed runtime-backed acceptance."));

        var reportWithoutHash = new MicrogameVariationAcceptanceReport
        {
            Accepted = allAccepted,
            ManualGate = "manual_configurable_microgame_verification",
            VariantCount = acceptedVariants.Count,
            DifferenceSummary = differenceSummary,
            Variants = acceptedVariants,
            Diagnostics = SortDiagnostics(diagnostics)
        };
        var hash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions));
        var report = reportWithoutHash with { DeterministicHash = hash };

        return new MicrogameVariationAcceptanceResult
        {
            Report = report,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            ManualVerificationMarkdown = RenderManualVerification(report)
        };
    }

    public async Task<MicrogameVariationAcceptanceWriteResult> WriteAsync(
        string projectRootPath,
        MicrogameVariationAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural", "generated-microgame-variation"));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var reportJsonPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportJsonFileName));
        var reportMarkdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportMarkdownFileName));
        var manualVerificationMarkdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ManualVerificationMarkdownFileName));
        EnsureContained(outputDirectory, reportJsonPath);
        EnsureContained(outputDirectory, reportMarkdownPath);
        EnsureContained(outputDirectory, manualVerificationMarkdownPath);

        await File.WriteAllTextAsync(reportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(manualVerificationMarkdownPath, result.ManualVerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new MicrogameVariationAcceptanceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportJsonPath = reportJsonPath,
            ReportMarkdownPath = reportMarkdownPath,
            ManualVerificationMarkdownPath = manualVerificationMarkdownPath
        };
    }

    private static MicrogameVariationAcceptedVariant BuildVariant(
        int ordinal,
        GenerationPresetOptions generationOptions,
        VisibleGeneratedPlayablePreviewResult visibleResult,
        RuntimeBackedMicrogameStateAcceptanceResult runtimeAcceptance)
    {
        var snapshot = runtimeAcceptance.Snapshot;
        var accepted = visibleResult.Report.RuntimeStartSucceeded
            && visibleResult.Report.ActiveGoalSelected
            && visibleResult.Report.GoalProgressAdvanced
            && visibleResult.Report.ChallengeResolved
            && visibleResult.Report.RewardVisible
            && visibleResult.Report.CompletionVisible
            && string.Equals(snapshot.GoalProgressStateSource, "runtime_state_quests", StringComparison.Ordinal)
            && string.Equals(snapshot.ChallengeStateSource, "runtime_state_flags_inventory_encounter", StringComparison.Ordinal)
            && !snapshot.GoalProgressFallbackPreviewJournalUsed
            && !snapshot.ChallengeFallbackPreviewProjectionUsed
            && snapshot.RuntimeRewardGranted
            && snapshot.RuntimeCompletionBacked;
        var diagnostics = new List<MicrogameVariationAcceptanceDiagnostic>
        {
            Diagnostic(
                accepted ? "info" : "error",
                accepted ? "generated_microgame_variation.variant_accepted" : "generated_microgame_variation.variant_failed",
                generationOptions.StableSummary,
                accepted ? "Variant passed runtime-backed microgame acceptance." : "Variant failed runtime-backed microgame acceptance.")
        };

        return new MicrogameVariationAcceptedVariant
        {
            Ordinal = ordinal,
            Accepted = accepted,
            GenerationOptions = generationOptions,
            PackageId = visibleResult.Snapshot.PackageId,
            PackageTitle = visibleResult.Snapshot.PackageTitle,
            SnapshotHash = visibleResult.Snapshot.DeterministicHash,
            RuntimeBackedStateHash = runtimeAcceptance.Snapshot.DeterministicHash,
            CurrentMapId = visibleResult.Snapshot.CurrentMapId,
            RepresentativeIds = visibleResult.Snapshot.RepresentativeGeneratedIds,
            ActiveGoalId = snapshot.ActiveGoalId,
            ActiveGoalTitle = snapshot.ActiveGoalTitle,
            ChallengeId = snapshot.ChallengeId,
            ChallengeTitle = snapshot.ChallengeTitle,
            RewardItemId = snapshot.RewardItemId,
            RewardTitle = snapshot.RewardTitle,
            CompletionStatus = snapshot.CompletionStatus,
            RuntimeStartSucceeded = snapshot.RuntimeStartSucceeded,
            ProgressAdvanced = snapshot.ProgressAdvanced,
            GoalProgressStateSource = snapshot.GoalProgressStateSource,
            ChallengeResolved = snapshot.ChallengeResolved,
            ChallengeStateSource = snapshot.ChallengeStateSource,
            RewardVisible = snapshot.RewardVisible,
            RuntimeRewardGranted = snapshot.RuntimeRewardGranted,
            CompletionVisible = snapshot.CompletionVisible,
            RuntimeCompletionBacked = snapshot.RuntimeCompletionBacked,
            GoalProgressFallbackPreviewJournalUsed = snapshot.GoalProgressFallbackPreviewJournalUsed,
            ChallengeFallbackPreviewProjectionUsed = snapshot.ChallengeFallbackPreviewProjectionUsed,
            Diagnostics = diagnostics
        };
    }

    private static MicrogameVariationDifferenceSummary BuildDifferenceSummary(
        IReadOnlyList<MicrogameVariationAcceptedVariant> variants)
    {
        var uniquePackageIds = Unique(variants.Select(item => item.PackageId));
        var uniqueTitles = Unique(variants.Select(item => item.PackageTitle));
        var uniqueMaps = Unique(variants.Select(item => item.CurrentMapId));
        var uniqueGoals = Unique(variants.Select(item => item.ActiveGoalId));
        var uniqueChallenges = Unique(variants.Select(item => item.ChallengeId));
        var uniqueRewards = Unique(variants.Select(item => item.RewardItemId));

        return new MicrogameVariationDifferenceSummary
        {
            UniqueSeedCount = Unique(variants.Select(item => item.GenerationOptions.Seed)).Count,
            UniquePresetCount = Unique(variants.Select(item => item.GenerationOptions.PresetId)).Count,
            UniquePackageIdCount = uniquePackageIds.Count,
            UniquePackageTitleCount = uniqueTitles.Count,
            UniqueMapCount = uniqueMaps.Count,
            UniqueActiveGoalCount = uniqueGoals.Count,
            UniqueChallengeCount = uniqueChallenges.Count,
            UniqueRewardCount = uniqueRewards.Count,
            PackageIds = uniquePackageIds,
            PackageTitles = uniqueTitles,
            CurrentMapIds = uniqueMaps,
            ActiveGoalIds = uniqueGoals,
            ChallengeIds = uniqueChallenges,
            RewardItemIds = uniqueRewards
        };
    }

    private static IReadOnlyList<MicrogameVariationAcceptanceRequest> BuildDefaultMatrix() =>
    [
        new MicrogameVariationAcceptanceRequest
        {
            Seed = GenerationPresetOptionsService.DefaultSeed,
            PresetId = GenerationPresetOptionsService.DefaultPresetId
        },
        new MicrogameVariationAcceptanceRequest
        {
            Seed = "goal002-variant-recover-resource",
            PresetId = "recover_resource"
        },
        new MicrogameVariationAcceptanceRequest
        {
            Seed = "goal002-variant-safe-faction-truce",
            PresetId = "safe_faction_truce"
        }
    ];

    private static string RenderReport(MicrogameVariationAcceptanceReport report)
    {
        var lines = new List<string>
        {
            "# Generated Microgame Variation Acceptance",
            string.Empty,
            "- Deterministic: true",
            "- External execution: none",
            $"- Accepted: `{report.Accepted.ToString().ToLowerInvariant()}`",
            $"- Snapshot hash: `{report.DeterministicHash}`",
            $"- Next manual gate: `{report.ManualGate}`",
            string.Empty,
            "## Difference Summary",
            string.Empty,
            $"- Unique seeds: `{report.DifferenceSummary.UniqueSeedCount}`",
            $"- Unique presets: `{report.DifferenceSummary.UniquePresetCount}`",
            $"- Unique package ids: `{report.DifferenceSummary.UniquePackageIdCount}`",
            $"- Unique package titles: `{report.DifferenceSummary.UniquePackageTitleCount}`",
            $"- Unique maps: `{report.DifferenceSummary.UniqueMapCount}`",
            $"- Unique active goals: `{report.DifferenceSummary.UniqueActiveGoalCount}`",
            $"- Unique challenges: `{report.DifferenceSummary.UniqueChallengeCount}`",
            $"- Unique rewards: `{report.DifferenceSummary.UniqueRewardCount}`",
            string.Empty,
            "## Variants",
            string.Empty
        };

        foreach (var variant in report.Variants)
        {
            lines.Add($"### Variant {variant.Ordinal}");
            lines.Add(string.Empty);
            lines.Add($"- Accepted: `{variant.Accepted.ToString().ToLowerInvariant()}`");
            lines.Add($"- Seed: `{variant.GenerationOptions.Seed}`");
            lines.Add($"- Preset: `{variant.GenerationOptions.PresetId}`");
            lines.Add($"- Package: `{variant.PackageTitle}` / `{variant.PackageId}`");
            lines.Add($"- Active goal: `{FirstNonEmpty(variant.ActiveGoalTitle, variant.ActiveGoalId, "none")}`");
            lines.Add($"- Challenge: `{FirstNonEmpty(variant.ChallengeTitle, variant.ChallengeId, "none")}`");
            lines.Add($"- Reward: `{FirstNonEmpty(variant.RewardTitle, variant.RewardItemId, "none")}`");
            lines.Add($"- Completion: `{FirstNonEmpty(variant.CompletionStatus, "none")}`");
            lines.Add($"- Goal source: `{FirstNonEmpty(variant.GoalProgressStateSource, "none")}`");
            lines.Add($"- Challenge source: `{FirstNonEmpty(variant.ChallengeStateSource, "none")}`");
            lines.Add(string.Empty);
        }

        lines.Add("## Diagnostics");
        lines.Add(string.Empty);
        lines.AddRange(report.Diagnostics.Count == 0
            ? ["- None"]
            : report.Diagnostics.Select(item => $"- `{item.Severity}` `{item.Code}` target=`{item.Target}`: {item.Message}"));

        return string.Join("\n", lines) + "\n";
    }

    private static string RenderManualVerification(MicrogameVariationAcceptanceReport report)
    {
        var lines = new List<string>
        {
            "# Manual Configurable Microgame Verification",
            string.Empty,
            "Use this after Product Slice 042. Codex does not perform this manual UI check.",
            string.Empty,
            "1. Start `LLMGameCreator.WinForms`.",
            "2. Open Runtime Preview.",
            "3. For each variant below, set the seed and preset.",
            "4. Click `Generate Preview`.",
            "5. Click `Start`.",
            "6. Confirm runtime-backed goal progress changes after interaction.",
            "7. Confirm challenge resolution, reward visibility and completed completion state.",
            "8. Confirm variants differ in package labels or generated content.",
            string.Empty,
            "Variant matrix:",
            string.Empty
        };

        foreach (var variant in report.Variants)
        {
            lines.Add($"- `{variant.GenerationOptions.Seed}` / `{variant.GenerationOptions.PresetId}` -> `{variant.PackageTitle}` / `{variant.PackageId}`");
        }

        lines.Add(string.Empty);
        lines.Add($"Headless acceptance status: `{report.Accepted.ToString().ToLowerInvariant()}`");
        lines.Add($"Next state marker: `{report.ManualGate}`");

        return string.Join("\n", lines) + "\n";
    }

    private static IReadOnlyList<MicrogameVariationAcceptanceDiagnostic> SortDiagnostics(
        IEnumerable<MicrogameVariationAcceptanceDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static IReadOnlyList<string> Unique(IEnumerable<string> values) =>
        values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToList();

    private static MicrogameVariationAcceptanceDiagnostic Diagnostic(
        string severity,
        string code,
        string target,
        string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

    private static string ComputeHash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static void EnsureContained(string rootPath, string candidatePath)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var candidate = Path.GetFullPath(candidatePath);
        if (!string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Generated microgame variation output path must stay under the project root.");
        }
    }
}

public sealed record MicrogameVariationAcceptanceRequest
{
    public string Seed { get; init; } = GenerationPresetOptionsService.DefaultSeed;
    public string Mode { get; init; } = GenerationPresetOptionsService.DefaultMode;
    public string PresetId { get; init; } = GenerationPresetOptionsService.DefaultPresetId;
    public IReadOnlyList<string> CompactStyleHintIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SelectedVariantIds { get; init; } = Array.Empty<string>();
}

public sealed record MicrogameVariationAcceptanceResult
{
    public MicrogameVariationAcceptanceReport Report { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string ManualVerificationMarkdown { get; init; } = string.Empty;
}

public sealed record MicrogameVariationAcceptanceWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string ManualVerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record MicrogameVariationAcceptanceReport
{
    public string SchemaVersion { get; init; } = "1";
    public string DeterministicHash { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public string ManualGate { get; init; } = string.Empty;
    public int VariantCount { get; init; }
    public MicrogameVariationDifferenceSummary DifferenceSummary { get; init; } = new();
    public IReadOnlyList<MicrogameVariationAcceptedVariant> Variants { get; init; } = Array.Empty<MicrogameVariationAcceptedVariant>();
    public IReadOnlyList<MicrogameVariationAcceptanceDiagnostic> Diagnostics { get; init; } = Array.Empty<MicrogameVariationAcceptanceDiagnostic>();
}

public sealed record MicrogameVariationAcceptedVariant
{
    public int Ordinal { get; init; }
    public bool Accepted { get; init; }
    public GenerationPresetOptions GenerationOptions { get; init; } = new();
    public string PackageId { get; init; } = string.Empty;
    public string PackageTitle { get; init; } = string.Empty;
    public string SnapshotHash { get; init; } = string.Empty;
    public string RuntimeBackedStateHash { get; init; } = string.Empty;
    public string CurrentMapId { get; init; } = string.Empty;
    public VisibleGeneratedPlayablePreviewRepresentativeIds RepresentativeIds { get; init; } = new();
    public string ActiveGoalId { get; init; } = string.Empty;
    public string ActiveGoalTitle { get; init; } = string.Empty;
    public string ChallengeId { get; init; } = string.Empty;
    public string ChallengeTitle { get; init; } = string.Empty;
    public string RewardItemId { get; init; } = string.Empty;
    public string RewardTitle { get; init; } = string.Empty;
    public string CompletionStatus { get; init; } = string.Empty;
    public bool RuntimeStartSucceeded { get; init; }
    public bool ProgressAdvanced { get; init; }
    public string GoalProgressStateSource { get; init; } = string.Empty;
    public bool ChallengeResolved { get; init; }
    public string ChallengeStateSource { get; init; } = string.Empty;
    public bool RewardVisible { get; init; }
    public bool RuntimeRewardGranted { get; init; }
    public bool CompletionVisible { get; init; }
    public bool RuntimeCompletionBacked { get; init; }
    public bool GoalProgressFallbackPreviewJournalUsed { get; init; }
    public bool ChallengeFallbackPreviewProjectionUsed { get; init; }
    public IReadOnlyList<MicrogameVariationAcceptanceDiagnostic> Diagnostics { get; init; } = Array.Empty<MicrogameVariationAcceptanceDiagnostic>();
}

public sealed record MicrogameVariationDifferenceSummary
{
    public int UniqueSeedCount { get; init; }
    public int UniquePresetCount { get; init; }
    public int UniquePackageIdCount { get; init; }
    public int UniquePackageTitleCount { get; init; }
    public int UniqueMapCount { get; init; }
    public int UniqueActiveGoalCount { get; init; }
    public int UniqueChallengeCount { get; init; }
    public int UniqueRewardCount { get; init; }
    public IReadOnlyList<string> PackageIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> PackageTitles { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> CurrentMapIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ActiveGoalIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ChallengeIds { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RewardItemIds { get; init; } = Array.Empty<string>();
}

public sealed record MicrogameVariationAcceptanceDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
