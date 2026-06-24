using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.RuntimePreview;

public sealed class VisibleGeneratedPlayablePreviewService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/visible-generated-playable-preview";
    public const string SnapshotJsonFileName = "visible-generated-playable-preview-snapshot.json";
    public const string ReportJsonFileName = "visible-generated-playable-preview-report.json";
    public const string ReportMarkdownFileName = "visible-generated-playable-preview-report.md";
    public const string ManualVerificationMarkdownFileName = "manual-verification.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly ProceduralGameKernelService _kernelService;
    private readonly FormulaEffectActionRegistryService _registryService;
    private readonly TinyGeneratedRuntimeLoopService _tinyLoopService;
    private readonly GeneratedPackageMvpService _packageMvpService;
    private readonly GeneratedPackageRuntimePreviewService _previewService;
    private readonly GeneratedMicrogameGoalPreviewService _microgameGoalPreviewService;
    private readonly GeneratedMicrogameChallengePreviewService _microgameChallengePreviewService;
    private readonly VisibleGeneratedPlayablePreviewMarkdownRenderer _markdownRenderer;
    private readonly IVisibleGeneratedPlayableRuntimeAdapter _runtimeAdapter;

    public VisibleGeneratedPlayablePreviewService(
        ProceduralGameKernelService? kernelService = null,
        FormulaEffectActionRegistryService? registryService = null,
        TinyGeneratedRuntimeLoopService? tinyLoopService = null,
        GeneratedPackageMvpService? packageMvpService = null,
        GeneratedPackageRuntimePreviewService? previewService = null,
        GeneratedMicrogameGoalPreviewService? microgameGoalPreviewService = null,
        GeneratedMicrogameChallengePreviewService? microgameChallengePreviewService = null,
        VisibleGeneratedPlayablePreviewMarkdownRenderer? markdownRenderer = null,
        IVisibleGeneratedPlayableRuntimeAdapter? runtimeAdapter = null)
    {
        _kernelService = kernelService ?? new ProceduralGameKernelService();
        _registryService = registryService ?? new FormulaEffectActionRegistryService();
        _tinyLoopService = tinyLoopService ?? new TinyGeneratedRuntimeLoopService();
        _packageMvpService = packageMvpService ?? new GeneratedPackageMvpService();
        _previewService = previewService ?? new GeneratedPackageRuntimePreviewService();
        _microgameGoalPreviewService = microgameGoalPreviewService ?? new GeneratedMicrogameGoalPreviewService();
        _microgameChallengePreviewService = microgameChallengePreviewService ?? new GeneratedMicrogameChallengePreviewService();
        _markdownRenderer = markdownRenderer ?? new VisibleGeneratedPlayablePreviewMarkdownRenderer();
        _runtimeAdapter = runtimeAdapter ?? new RuntimeAdapterUnavailable();
    }

    public VisibleGeneratedPlayablePreviewResult Generate(VisibleGeneratedPlayablePreviewRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var diagnostics = new List<VisibleGeneratedPlayablePreviewDiagnostic>();
        var planResult = _kernelService.Generate(new ProceduralGameKernelRequest
        {
            Seed = string.IsNullOrWhiteSpace(request.Seed) ? "visible-generated-playable-preview" : request.Seed.Trim(),
            Mode = string.IsNullOrWhiteSpace(request.Mode) ? ProceduralGameGenerationModes.SemiProceduralRegions : request.Mode.Trim(),
            CompactStyleHintIds = request.CompactStyleHintIds,
            SelectedVariantIds = request.SelectedVariantIds
        });
        var rulePackResult = _registryService.Generate(new FormulaEffectActionRegistryRequest { SourcePlan = planResult.Plan });
        var tinyLoopResult = _tinyLoopService.Run(new TinyGeneratedRuntimeLoopRequest
        {
            SourcePlan = planResult.Plan,
            RulePack = rulePackResult.RulePack,
            RulePackValidationReport = rulePackResult.ValidationReport
        });
        var packageMvpResult = _packageMvpService.Generate(new GeneratedPackageMvpRequest
        {
            SourcePlan = planResult.Plan,
            RulePack = rulePackResult.RulePack,
            RulePackValidationReport = rulePackResult.ValidationReport,
            TinyLoopResult = tinyLoopResult
        });

        var runtimeAttempt = RunRuntime(packageMvpResult.Package, diagnostics);
        var projectionState = BuildProjectionState(runtimeAttempt);
        var projection = _previewService.Build(packageMvpResult.Package, projectionState);
        var microgameGoal = _microgameGoalPreviewService.BuildFromRuntimeAttempt(packageMvpResult.Package, projection, runtimeAttempt);
        var microgameChallenge = _microgameChallengePreviewService.BuildFromRuntimeAttempt(packageMvpResult.Package, projection, microgameGoal, runtimeAttempt);
        diagnostics.AddRange(runtimeAttempt.Diagnostics);
        diagnostics.AddRange(microgameGoal.Diagnostics.Select(item => Diagnostic(item.Severity, item.Code, item.Target, item.Message)));
        diagnostics.AddRange(microgameChallenge.Diagnostics.Select(item => Diagnostic(item.Severity, item.Code, item.Target, item.Message)));
        diagnostics.Add(Diagnostic("info", "visible_generated_playable_preview.no_external_execution", "generation", "No LLM, provider, Lua, Unity or media execution was invoked."));

        var sourceHashes = new VisibleGeneratedPlayablePreviewSourceHashes
        {
            PlanHash = planResult.Plan.Metadata.DeterministicHash,
            RulePackHash = rulePackResult.RulePack.Metadata.DeterministicHash,
            TinyLoopStateHash = tinyLoopResult.State.DeterministicHash,
            GeneratedPackageFinalHash = packageMvpResult.Report.PackageHash
        };
        var sortedDiagnostics = SortDiagnostics(diagnostics);
        var snapshotWithoutHash = new VisibleGeneratedPlayablePreviewSnapshot
        {
            SourceHashes = sourceHashes,
            PackageId = packageMvpResult.Package.Manifest.PackageId,
            PackageTitle = packageMvpResult.Package.Manifest.Title,
            StartMapId = packageMvpResult.Package.Manifest.StartMapId,
            CurrentMapId = projection.CurrentMapId,
            RuntimeAttempt = runtimeAttempt,
            Projection = projection,
            MicrogameGoal = microgameGoal,
            MicrogameChallenge = microgameChallenge,
            Counts = BuildCounts(packageMvpResult.Package, projection, microgameGoal, microgameChallenge),
            RepresentativeGeneratedIds = BuildRepresentativeIds(projection),
            Diagnostics = sortedDiagnostics
        };
        var snapshotHash = ComputeHash(JsonSerializer.Serialize(snapshotWithoutHash, JsonOptions));
        var snapshot = snapshotWithoutHash with { DeterministicHash = snapshotHash };
        var report = new VisibleGeneratedPlayablePreviewReport
        {
            SnapshotHash = snapshotHash,
            StableSummary = BuildStableSummary(snapshot),
            RuntimeStartSucceeded = runtimeAttempt.RuntimeStartSucceeded,
            RuntimeCommandAttempted = runtimeAttempt.CommandAttempts.Count > 0,
            RuntimeCommandSucceeded = runtimeAttempt.CommandAttempts.Any(item => item.Succeeded),
            ActiveGoalSelected = microgameGoal.ActiveGoalSelected,
            GoalProgressAdvanced = microgameGoal.ProgressAdvancedByInteraction,
            ChallengeResolved = microgameChallenge.Resolved,
            RewardVisible = microgameChallenge.RewardVisible,
            CompletionVisible = microgameChallenge.CompletionVisible,
            DiagnosticCount = sortedDiagnostics.Count,
            SourceHashes = sourceHashes,
            Diagnostics = sortedDiagnostics
        };

        return new VisibleGeneratedPlayablePreviewResult
        {
            PlanResult = planResult,
            RulePackResult = rulePackResult,
            TinyLoopResult = tinyLoopResult,
            PackageMvpResult = packageMvpResult,
            Snapshot = snapshot,
            Report = report,
            SnapshotJson = JsonSerializer.Serialize(snapshot, JsonOptions),
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = _markdownRenderer.RenderReport(snapshot, report),
            ManualVerificationMarkdown = _markdownRenderer.RenderManualVerification(snapshot)
        };
    }

    public async Task<VisibleGeneratedPlayablePreviewWriteResult> WriteAsync(
        string projectRootPath,
        VisibleGeneratedPlayablePreviewResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural", "visible-generated-playable-preview"));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var snapshotJsonPath = Path.GetFullPath(Path.Combine(outputDirectory, SnapshotJsonFileName));
        var reportJsonPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportJsonFileName));
        var reportMarkdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportMarkdownFileName));
        var manualVerificationMarkdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ManualVerificationMarkdownFileName));
        EnsureContained(outputDirectory, snapshotJsonPath);
        EnsureContained(outputDirectory, reportJsonPath);
        EnsureContained(outputDirectory, reportMarkdownPath);
        EnsureContained(outputDirectory, manualVerificationMarkdownPath);

        await File.WriteAllTextAsync(snapshotJsonPath, result.SnapshotJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(manualVerificationMarkdownPath, result.ManualVerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new VisibleGeneratedPlayablePreviewWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            SnapshotJsonPath = snapshotJsonPath,
            ReportJsonPath = reportJsonPath,
            ReportMarkdownPath = reportMarkdownPath,
            ManualVerificationMarkdownPath = manualVerificationMarkdownPath
        };
    }

    private VisibleGeneratedPlayableRuntimeAttempt RunRuntime(
        GamePackageDefinition package,
        ICollection<VisibleGeneratedPlayablePreviewDiagnostic> diagnostics)
    {
        try
        {
            return _runtimeAdapter.Run(package);
        }
        catch (Exception ex)
        {
            diagnostics.Add(Diagnostic("error", "visible_generated_playable_preview.runtime_adapter_failed", package.Manifest.PackageId, ex.GetType().Name + ": " + ex.Message));
            return new VisibleGeneratedPlayableRuntimeAttempt
            {
                RuntimeStartAttempted = true,
                RuntimeStartSucceeded = false,
                StartMapId = package.Manifest.StartMapId,
                CurrentMapId = package.Manifest.StartMapId,
                Diagnostics =
                [
                    Diagnostic("error", "visible_generated_playable_preview.runtime_adapter_failed", package.Manifest.PackageId, ex.GetType().Name + ": " + ex.Message)
                ]
            };
        }
    }

    private static GameState? BuildProjectionState(VisibleGeneratedPlayableRuntimeAttempt runtimeAttempt)
    {
        if (!runtimeAttempt.RuntimeStartSucceeded || string.IsNullOrWhiteSpace(runtimeAttempt.CurrentMapId))
        {
            return null;
        }

        return new GameState
        {
            CurrentMapId = runtimeAttempt.CurrentMapId,
            PlayerPosition = new LLMGameCreator.Domain.Definitions.Position2D(runtimeAttempt.PlayerCurrentPosition.X, runtimeAttempt.PlayerCurrentPosition.Y),
            Mode = "map"
        };
    }

    private static VisibleGeneratedPlayablePreviewCounts BuildCounts(
        GamePackageDefinition package,
        GeneratedPackageRuntimePreviewModel projection,
        GeneratedMicrogameGoalPreviewModel? goal = null,
        GeneratedMicrogameChallengePreviewModel? challenge = null) => new()
    {
        PackageMaps = package.Game.Maps.Count,
        PackageItems = package.Game.Items.Count,
        PackageEncounters = package.Game.Encounters.Count,
        PackageQuests = package.Game.Quests.Count,
        Regions = projection.Regions.Count,
        Npcs = projection.Npcs.Count,
        Items = projection.Items.Count,
        Encounters = projection.Encounters.Count,
        Quests = projection.Quests.Count,
        ActiveGoals = goal?.ActiveGoalSelected == true ? 1 : 0,
        ActiveGoalCompletedSteps = goal?.CompletedStepCount ?? 0,
        ActiveGoalTotalSteps = goal?.StepCount ?? 0,
        ResolvedChallenges = challenge?.Resolved == true ? 1 : 0,
        VisibleRewards = challenge?.RewardVisible == true ? 1 : 0,
        VisibleCompletions = challenge?.CompletionVisible == true ? 1 : 0,
        Mechanics = projection.Mechanics.Count,
        ProvenanceRecords = projection.Provenance.Count
    };

    private static VisibleGeneratedPlayablePreviewRepresentativeIds BuildRepresentativeIds(
        GeneratedPackageRuntimePreviewModel projection) => new()
    {
        RegionIds = projection.Regions.Select(item => item.SourceId).Where(NotBlank).OrderBy(item => item, StringComparer.Ordinal).Take(5).ToList(),
        NpcIds = projection.Npcs.Select(item => item.SourceId).Where(NotBlank).OrderBy(item => item, StringComparer.Ordinal).Take(5).ToList(),
        ItemIds = projection.Items.Select(item => item.SourceId).Where(NotBlank).OrderBy(item => item, StringComparer.Ordinal).Take(5).ToList(),
        EncounterIds = projection.Encounters.Select(item => item.SourceId).Where(NotBlank).OrderBy(item => item, StringComparer.Ordinal).Take(5).ToList(),
        QuestIds = projection.Quests.Select(item => item.SourceId).Where(NotBlank).OrderBy(item => item, StringComparer.Ordinal).Take(5).ToList(),
        MechanicIds = projection.Mechanics.Select(item => item.SourceId).Where(NotBlank).OrderBy(item => item, StringComparer.Ordinal).Take(5).ToList(),
        ProvenanceArtifactIds = projection.Provenance.Select(item => item.ArtifactId).Where(NotBlank).OrderBy(item => item, StringComparer.Ordinal).Take(5).ToList()
    };

    private static string BuildStableSummary(VisibleGeneratedPlayablePreviewSnapshot snapshot) =>
        string.Join("; ", new[]
        {
            $"package={snapshot.PackageId}",
            $"currentMap={snapshot.CurrentMapId}",
            $"runtimeStart={snapshot.RuntimeAttempt.RuntimeStartSucceeded.ToString().ToLowerInvariant()}",
            $"commandAttempts={snapshot.RuntimeAttempt.CommandAttempts.Count}",
            $"regions={snapshot.Counts.Regions}",
            $"quests={snapshot.Counts.Quests}",
            $"activeGoals={snapshot.Counts.ActiveGoals}",
            $"goalProgress={snapshot.Counts.ActiveGoalCompletedSteps}/{snapshot.Counts.ActiveGoalTotalSteps}",
            $"challengeResolved={snapshot.MicrogameChallenge.Resolved.ToString().ToLowerInvariant()}",
            $"rewardVisible={snapshot.MicrogameChallenge.RewardVisible.ToString().ToLowerInvariant()}",
            $"completionVisible={snapshot.MicrogameChallenge.CompletionVisible.ToString().ToLowerInvariant()}",
            $"mechanics={snapshot.Counts.Mechanics}"
        });

    private static IReadOnlyList<VisibleGeneratedPlayablePreviewDiagnostic> SortDiagnostics(IEnumerable<VisibleGeneratedPlayablePreviewDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static VisibleGeneratedPlayablePreviewDiagnostic Diagnostic(string severity, string code, string target, string message) => new()
    {
        Severity = severity,
        Code = code,
        Target = target,
        Message = message
    };

    private static bool NotBlank(string value) => !string.IsNullOrWhiteSpace(value);

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
            throw new InvalidOperationException("Visible generated playable preview output path must stay under the project root.");
        }
    }

    private sealed class RuntimeAdapterUnavailable : IVisibleGeneratedPlayableRuntimeAdapter
    {
        public VisibleGeneratedPlayableRuntimeAttempt Run(GamePackageDefinition package) => new()
        {
            RuntimeStartAttempted = false,
            RuntimeStartSucceeded = false,
            StartMapId = package.Manifest.StartMapId,
            CurrentMapId = package.Manifest.StartMapId,
            Diagnostics =
            [
                Diagnostic("warning", "visible_generated_playable_preview.runtime_adapter_unavailable", package.Manifest.PackageId, "Application layer has no LLMGameCreator.Runtime project dependency; product smoke can supply DefaultGameRuntime through IVisibleGeneratedPlayableRuntimeAdapter.")
            ]
        };
    }
}
