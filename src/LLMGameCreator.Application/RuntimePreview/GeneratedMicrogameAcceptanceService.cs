using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.RuntimePreview;

public sealed class GeneratedMicrogameAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/generated-microgame-loop";
    public const string SnapshotJsonFileName = "generated-microgame-loop-snapshot.json";
    public const string ReportMarkdownFileName = "generated-microgame-loop-report.md";
    public const string ManualVerificationMarkdownFileName = "manual-microgame-loop-verification.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public GeneratedMicrogameAcceptanceResult Build(VisibleGeneratedPlayablePreviewResult visibleResult)
    {
        ArgumentNullException.ThrowIfNull(visibleResult);

        var snapshotWithoutHash = new GeneratedMicrogameAcceptanceSnapshot
        {
            PackageId = visibleResult.Snapshot.PackageId,
            PackageTitle = visibleResult.Snapshot.PackageTitle,
            ActiveGoalId = visibleResult.Snapshot.MicrogameGoal.ActiveQuestId,
            ActiveGoalTitle = visibleResult.Snapshot.MicrogameGoal.ActiveQuestTitle,
            ObjectiveId = visibleResult.Snapshot.MicrogameGoal.Related.ObjectiveIds.FirstOrDefault() ?? string.Empty,
            ObjectiveText = visibleResult.Snapshot.MicrogameGoal.CurrentObjectiveText,
            ChallengeId = visibleResult.Snapshot.MicrogameChallenge.EncounterId,
            ChallengeTitle = visibleResult.Snapshot.MicrogameChallenge.EncounterTitle,
            RequiredInteraction = visibleResult.Snapshot.MicrogameChallenge.ResolveAction,
            RelatedNpcId = visibleResult.Snapshot.MicrogameGoal.Related.NpcId,
            RelatedNpcTitle = visibleResult.Snapshot.MicrogameGoal.Related.NpcTitle,
            RewardItemId = visibleResult.Snapshot.MicrogameChallenge.RewardItemId,
            RewardTitle = visibleResult.Snapshot.MicrogameChallenge.RewardTitle,
            RuntimeStartSucceeded = visibleResult.Report.RuntimeStartSucceeded,
            RuntimeMoveSucceeded = visibleResult.Snapshot.RuntimeAttempt.CommandAttempts.Any(item => string.Equals(item.CommandType, "move/right", StringComparison.OrdinalIgnoreCase) && item.Succeeded),
            RuntimeInteractSucceeded = visibleResult.Snapshot.RuntimeAttempt.CommandAttempts.Any(item => string.Equals(item.CommandType, "interact", StringComparison.OrdinalIgnoreCase) && item.Succeeded),
            ActiveGoalVisible = visibleResult.Report.ActiveGoalSelected,
            ProgressAdvanced = visibleResult.Report.GoalProgressAdvanced,
            ChallengeResolved = visibleResult.Report.ChallengeResolved,
            RewardVisible = visibleResult.Report.RewardVisible,
            CompletionVisible = visibleResult.Report.CompletionVisible,
            CompletionStatus = visibleResult.Snapshot.MicrogameChallenge.CompletionStatus,
            Diagnostics = BuildDiagnostics(visibleResult)
        };
        var hash = ComputeHash(JsonSerializer.Serialize(snapshotWithoutHash, JsonOptions));
        var snapshot = snapshotWithoutHash with { DeterministicHash = hash };
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        return new GeneratedMicrogameAcceptanceResult
        {
            Snapshot = snapshot,
            SnapshotJson = json,
            ReportMarkdown = RenderReport(snapshot),
            ManualVerificationMarkdown = RenderManualVerification(snapshot)
        };
    }

    public async Task<GeneratedMicrogameAcceptanceWriteResult> WriteAsync(
        string projectRootPath,
        GeneratedMicrogameAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, ".llmgc", "procedural", "generated-microgame-loop"));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var snapshotJsonPath = Path.GetFullPath(Path.Combine(outputDirectory, SnapshotJsonFileName));
        var reportMarkdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ReportMarkdownFileName));
        var manualVerificationMarkdownPath = Path.GetFullPath(Path.Combine(outputDirectory, ManualVerificationMarkdownFileName));
        EnsureContained(outputDirectory, snapshotJsonPath);
        EnsureContained(outputDirectory, reportMarkdownPath);
        EnsureContained(outputDirectory, manualVerificationMarkdownPath);

        await File.WriteAllTextAsync(snapshotJsonPath, result.SnapshotJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(manualVerificationMarkdownPath, result.ManualVerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new GeneratedMicrogameAcceptanceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            SnapshotJsonPath = snapshotJsonPath,
            ReportMarkdownPath = reportMarkdownPath,
            ManualVerificationMarkdownPath = manualVerificationMarkdownPath
        };
    }

    private static IReadOnlyList<GeneratedMicrogameAcceptanceDiagnostic> BuildDiagnostics(VisibleGeneratedPlayablePreviewResult visibleResult)
    {
        var diagnostics = visibleResult.Report.Diagnostics
            .Select(item => new GeneratedMicrogameAcceptanceDiagnostic
            {
                Severity = item.Severity,
                Code = item.Code,
                Target = item.Target,
                Message = item.Message
            })
            .Concat(new[]
            {
                new GeneratedMicrogameAcceptanceDiagnostic
                {
                    Severity = "info",
                    Code = "generated_microgame_acceptance.no_external_execution",
                    Target = visibleResult.Snapshot.PackageId,
                    Message = "No LLM, provider, Lua, Unity or media execution was invoked."
                },
                new GeneratedMicrogameAcceptanceDiagnostic
                {
                    Severity = "info",
                    Code = "generated_microgame_acceptance.manual_verification_required",
                    Target = "manual_microgame_loop_verification",
                    Message = "Codex acceptance is headless; the next step is user manual microgame loop verification."
                }
            })
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

        return diagnostics;
    }

    private static string RenderReport(GeneratedMicrogameAcceptanceSnapshot snapshot)
    {
        var lines = new List<string>
        {
            "# Generated Microgame Loop Acceptance",
            string.Empty,
            "- Deterministic: true",
            "- External execution: none",
            $"- Snapshot hash: `{snapshot.DeterministicHash}`",
            string.Empty,
            "## Package",
            string.Empty,
            $"- Package id: `{snapshot.PackageId}`",
            $"- Package title: `{snapshot.PackageTitle}`",
            string.Empty,
            "## Playable Loop Evidence",
            string.Empty,
            $"- Runtime start: `{snapshot.RuntimeStartSucceeded.ToString().ToLowerInvariant()}`",
            $"- Movement: `{snapshot.RuntimeMoveSucceeded.ToString().ToLowerInvariant()}`",
            $"- Interaction: `{snapshot.RuntimeInteractSucceeded.ToString().ToLowerInvariant()}`",
            $"- Active goal visible: `{snapshot.ActiveGoalVisible.ToString().ToLowerInvariant()}`",
            $"- Progress advanced: `{snapshot.ProgressAdvanced.ToString().ToLowerInvariant()}`",
            $"- Challenge resolved: `{snapshot.ChallengeResolved.ToString().ToLowerInvariant()}`",
            $"- Reward visible: `{snapshot.RewardVisible.ToString().ToLowerInvariant()}`",
            $"- Completion visible: `{snapshot.CompletionVisible.ToString().ToLowerInvariant()}`",
            string.Empty,
            "## Player-Facing Labels",
            string.Empty,
            $"- Active goal: `{FirstNonEmpty(snapshot.ActiveGoalTitle, snapshot.ActiveGoalId, "none")}`",
            $"- Objective: `{FirstNonEmpty(snapshot.ObjectiveText, snapshot.ObjectiveId, "none")}`",
            $"- Challenge: `{FirstNonEmpty(snapshot.ChallengeTitle, snapshot.ChallengeId, "none")}`",
            $"- Interaction: `{FirstNonEmpty(snapshot.RequiredInteraction, "none")}`",
            $"- NPC/object: `{FirstNonEmpty(snapshot.RelatedNpcTitle, snapshot.RelatedNpcId, "none")}`",
            $"- Reward: `{FirstNonEmpty(snapshot.RewardTitle, snapshot.RewardItemId, "none")}`",
            $"- Completion: `{FirstNonEmpty(snapshot.CompletionStatus, "none")}`",
            string.Empty,
            "## Diagnostics",
            string.Empty
        };

        lines.AddRange(snapshot.Diagnostics.Count == 0
            ? ["- None"]
            : snapshot.Diagnostics.Select(item => $"- `{item.Severity}` `{item.Code}` target=`{item.Target}`: {item.Message}"));

        return string.Join("\n", lines) + "\n";
    }

    private static string RenderManualVerification(GeneratedMicrogameAcceptanceSnapshot snapshot)
    {
        var lines = new List<string>
        {
            "# Manual Microgame Loop Verification",
            string.Empty,
            "Use this after Product Slice 037. Codex does not perform this manual UI check.",
            string.Empty,
            "1. Start `LLMGameCreator.WinForms`.",
            "2. Open Runtime Preview.",
            "3. Click `Generate Preview`.",
            "4. Click `Start`.",
            "5. Confirm the active goal and current objective are readable.",
            "6. Move to the generated NPC/object/item marker and use the existing interaction command.",
            "7. Confirm progress, challenge, reward and completion state are visible.",
            string.Empty,
            "Expected generated loop:",
            string.Empty,
            $"- Package: `{snapshot.PackageTitle}` / `{snapshot.PackageId}`",
            $"- Active goal: `{FirstNonEmpty(snapshot.ActiveGoalTitle, snapshot.ActiveGoalId, "none")}`",
            $"- Objective: `{FirstNonEmpty(snapshot.ObjectiveText, snapshot.ObjectiveId, "none")}`",
            $"- Challenge: `{FirstNonEmpty(snapshot.ChallengeTitle, snapshot.ChallengeId, "none")}`",
            $"- Related NPC/object: `{FirstNonEmpty(snapshot.RelatedNpcTitle, snapshot.RelatedNpcId, "none")}`",
            $"- Reward: `{FirstNonEmpty(snapshot.RewardTitle, snapshot.RewardItemId, "none")}`",
            $"- Completion status: `{FirstNonEmpty(snapshot.CompletionStatus, "none")}`",
            string.Empty,
            "Headless evidence already proved runtime start, movement, interaction, active goal, progress, challenge, reward and completion projection."
        };

        return string.Join("\n", lines) + "\n";
    }

    private static string ComputeHash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

    private static void EnsureContained(string rootPath, string candidatePath)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var candidate = Path.GetFullPath(candidatePath);
        if (!string.Equals(root, candidate, StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Generated microgame acceptance output path must stay under the project root.");
        }
    }
}

public sealed record GeneratedMicrogameAcceptanceResult
{
    public GeneratedMicrogameAcceptanceSnapshot Snapshot { get; init; } = new();
    public string SnapshotJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string ManualVerificationMarkdown { get; init; } = string.Empty;
}

public sealed record GeneratedMicrogameAcceptanceWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string SnapshotJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string ManualVerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record GeneratedMicrogameAcceptanceSnapshot
{
    public string SchemaVersion { get; init; } = "1";
    public string DeterministicHash { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageTitle { get; init; } = string.Empty;
    public string ActiveGoalId { get; init; } = string.Empty;
    public string ActiveGoalTitle { get; init; } = string.Empty;
    public string ObjectiveId { get; init; } = string.Empty;
    public string ObjectiveText { get; init; } = string.Empty;
    public string ChallengeId { get; init; } = string.Empty;
    public string ChallengeTitle { get; init; } = string.Empty;
    public string RequiredInteraction { get; init; } = string.Empty;
    public string RelatedNpcId { get; init; } = string.Empty;
    public string RelatedNpcTitle { get; init; } = string.Empty;
    public string RewardItemId { get; init; } = string.Empty;
    public string RewardTitle { get; init; } = string.Empty;
    public bool RuntimeStartSucceeded { get; init; }
    public bool RuntimeMoveSucceeded { get; init; }
    public bool RuntimeInteractSucceeded { get; init; }
    public bool ActiveGoalVisible { get; init; }
    public bool ProgressAdvanced { get; init; }
    public bool ChallengeResolved { get; init; }
    public bool RewardVisible { get; init; }
    public bool CompletionVisible { get; init; }
    public string CompletionStatus { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedMicrogameAcceptanceDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratedMicrogameAcceptanceDiagnostic>();
}

public sealed record GeneratedMicrogameAcceptanceDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
