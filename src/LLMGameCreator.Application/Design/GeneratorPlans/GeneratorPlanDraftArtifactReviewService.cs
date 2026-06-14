using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftArtifactReviewService
{
    private static readonly JsonSerializerOptions ReadJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly GeneratorPlanDraftArtifactApprovalArtifactReader _reader;
    private readonly GeneratorPlanDraftArtifactApprovalArtifactService _artifactService;
    private readonly GeneratorPlanDraftArtifactApprovalValidator _validator;
    private readonly GeneratorPlanDraftArtifactApprovalMarkdownRenderer _markdownRenderer;

    public GeneratorPlanDraftArtifactReviewService(
        GeneratorPlanDraftArtifactApprovalArtifactReader reader,
        GeneratorPlanDraftArtifactApprovalArtifactService artifactService,
        GeneratorPlanDraftArtifactApprovalValidator validator,
        GeneratorPlanDraftArtifactApprovalMarkdownRenderer markdownRenderer)
    {
        _reader = reader;
        _artifactService = artifactService;
        _validator = validator;
        _markdownRenderer = markdownRenderer;
    }

    public async Task<GeneratorPlanDraftArtifactReviewLoadResult> LoadLatestAsync(CancellationToken cancellationToken)
    {
        var latest = await _reader.ReadLatestAsync(cancellationToken).ConfigureAwait(false);
        if (!latest.Exists || latest.StagingArtifact == null)
        {
            return new GeneratorPlanDraftArtifactReviewLoadResult
            {
                Exists = false,
                Message = "No draft artifact staging snapshot found."
            };
        }

        var snapshot = ReadSnapshot(latest.StagingArtifact.Json);
        return new GeneratorPlanDraftArtifactReviewLoadResult
        {
            Exists = true,
            Message = "Latest draft artifact staging snapshot loaded.",
            StagingArtifact = latest.StagingArtifact,
            ApprovedArtifactSetArtifact = latest.ApprovedArtifactSetArtifact,
            Snapshot = snapshot,
            ValidationResults = latest.ValidationResults
        };
    }

    public async Task<GeneratorPlanDraftArtifactReviewDecisionResult> ApplyDecisionsToLatestAsync(
        GeneratorPlanDraftArtifactReviewDecisionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var latest = await LoadLatestAsync(cancellationToken).ConfigureAwait(false);
        if (!latest.Exists || latest.StagingArtifact == null)
        {
            return new GeneratorPlanDraftArtifactReviewDecisionResult
            {
                Ok = false,
                Status = "not_found",
                Diagnostics =
                [
                    GeneratorPlanDraftArtifactApprovalPolicy.Diagnostic(
                        GeneratorPlanPreviewDiagnosticSeverity.Error,
                        GeneratorPlanDraftArtifactApprovalDiagnosticCodes.ReviewStagingArtifactMissing,
                        "No draft artifact staging snapshot found.")
                ]
            };
        }

        var decisionByArtifact = request.Decisions
            .Where(decision => !string.IsNullOrWhiteSpace(decision.ArtifactId))
            .GroupBy(decision => decision.ArtifactId.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);

        var diagnostics = SelectPersistentDiagnostics(latest.Snapshot.Diagnostics).ToList();
        var items = latest.Snapshot.Items
            .Select(item => ApplyDecision(latest.Snapshot.Id, item, decisionByArtifact, diagnostics))
            .ToList();

        AddUnknownDecisionDiagnostics(latest.Snapshot.Id, latest.Snapshot.Items, decisionByArtifact, diagnostics);

        var snapshot = latest.Snapshot with
        {
            Items = items,
            Diagnostics = diagnostics
        };
        snapshot = _validator.Validate(snapshot);

        var result = new GeneratorPlanDraftArtifactApprovalResult
        {
            Ok = snapshot.Status != GeneratorPlanDraftArtifactStagingStatus.Invalid,
            Status = snapshot.Status,
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            Snapshot = snapshot,
            MarkdownReport = request.RenderMarkdown ? _markdownRenderer.Render(snapshot) : string.Empty,
            Diagnostics = snapshot.Diagnostics
        };

        var saved = await _artifactService.SaveAsync(
            result,
            new GeneratorPlanDraftArtifactApprovalArtifactSaveRequest
            {
                GeneratedBy = string.IsNullOrWhiteSpace(request.GeneratedBy)
                    ? "artifact_review_ui"
                    : request.GeneratedBy.Trim()
            },
            cancellationToken).ConfigureAwait(false);

        return new GeneratorPlanDraftArtifactReviewDecisionResult
        {
            Ok = result.Ok,
            Status = result.Status,
            Snapshot = result.Snapshot,
            StagingArtifact = saved.StagingArtifact,
            ApprovedArtifactSetArtifact = saved.ApprovedArtifactSetArtifact,
            MarkdownArtifact = saved.MarkdownArtifact,
            Diagnostics = result.Diagnostics
        };
    }

    public Task<GeneratorPlanDraftArtifactApprovalArtifactResult> CaptureReviewFromExampleAsync(
        string examplePath,
        bool renderMarkdown,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(examplePath))
        {
            throw new ArgumentException("Example path is required.", nameof(examplePath));
        }

        return _artifactService.CaptureAsync(
            new GeneratorPlanDraftArtifactApprovalArtifactRequest
            {
                PreviewRequest = new GeneratorPlanPreviewRequest
                {
                    SourcePath = examplePath.Trim()
                },
                ApprovalRequest = new GeneratorPlanDraftArtifactApprovalRequest
                {
                    AutoApproveValidArtifacts = false,
                    RenderMarkdown = renderMarkdown
                },
                GeneratedBy = "artifact_review_ui"
            },
            cancellationToken);
    }

    private static GeneratorPlanDraftArtifactStagingSnapshot ReadSnapshot(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new GeneratorPlanDraftArtifactStagingSnapshot();
        }

        var wrapper = JsonSerializer.Deserialize<StoredStagingSnapshot>(json, ReadJsonOptions);
        return wrapper?.Snapshot ?? new GeneratorPlanDraftArtifactStagingSnapshot();
    }

    private static GeneratorPlanDraftArtifactApprovalItem ApplyDecision(
        string snapshotId,
        GeneratorPlanDraftArtifactApprovalItem item,
        IReadOnlyDictionary<string, GeneratorPlanDraftArtifactApprovalDecision> decisions,
        ICollection<GeneratorPlanDraftArtifactApprovalDiagnostic> diagnostics)
    {
        if (!decisions.TryGetValue(item.ArtifactId, out var decision))
        {
            return item;
        }

        var decisionKind = NormalizeDecision(decision.Decision);
        if (string.IsNullOrWhiteSpace(decisionKind) || decisionKind == GeneratorPlanDraftArtifactApprovalDecisionKind.Pending)
        {
            return item;
        }

        if (item.State == GeneratorPlanDraftArtifactApprovalItemState.Blocked
            && decisionKind != GeneratorPlanDraftArtifactApprovalDecisionKind.RepairRequested)
        {
            diagnostics.Add(GeneratorPlanDraftArtifactApprovalPolicy.Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Warning,
                GeneratorPlanDraftArtifactApprovalDiagnosticCodes.ReviewBlockedDecisionIgnored,
                "Blocked artifacts cannot be approved or rejected from artifact review.",
                snapshotId,
                item.ArtifactId,
                "decision"));
            return item;
        }

        if (decisionKind == GeneratorPlanDraftArtifactApprovalDecisionKind.Approved
            && (!IsValidJson(item.ContentJson) || item.ValidationIssues.Count > 0 || string.IsNullOrWhiteSpace(item.ExpectedArtifactContract)))
        {
            diagnostics.Add(GeneratorPlanDraftArtifactApprovalPolicy.Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Error,
                GeneratorPlanDraftArtifactApprovalDiagnosticCodes.ReviewApproveInvalidArtifact,
                "Artifact review cannot approve an item with invalid JSON, validation issues, or missing contract.",
                snapshotId,
                item.ArtifactId,
                "decision"));
            return item.State == GeneratorPlanDraftArtifactApprovalItemState.Pending ? item : item with { State = GeneratorPlanDraftArtifactApprovalItemState.Pending };
        }

        return item with
        {
            State = ToItemState(decisionKind, item.State),
            DecisionReasonCode = decision.ReasonCode.Trim(),
            DecisionComment = decision.Comment.Trim(),
            DecidedAtUtc = decision.DecidedAtUtc == default ? DateTimeOffset.UtcNow : decision.DecidedAtUtc
        };
    }

    private static void AddUnknownDecisionDiagnostics(
        string snapshotId,
        IReadOnlyList<GeneratorPlanDraftArtifactApprovalItem> items,
        IReadOnlyDictionary<string, GeneratorPlanDraftArtifactApprovalDecision> decisions,
        ICollection<GeneratorPlanDraftArtifactApprovalDiagnostic> diagnostics)
    {
        var known = items.Select(item => item.ArtifactId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var artifactId in decisions.Keys.Where(artifactId => !known.Contains(artifactId)).OrderBy(artifactId => artifactId, StringComparer.OrdinalIgnoreCase))
        {
            diagnostics.Add(GeneratorPlanDraftArtifactApprovalPolicy.Diagnostic(
                GeneratorPlanPreviewDiagnosticSeverity.Warning,
                GeneratorPlanDraftArtifactApprovalDiagnosticCodes.ReviewUnknownArtifactDecision,
                $"Artifact review decision ignored because artifact id was not found: {artifactId}",
                snapshotId,
                artifactId,
                "decision"));
        }
    }

    private static IReadOnlyList<GeneratorPlanDraftArtifactApprovalDiagnostic> SelectPersistentDiagnostics(
        IReadOnlyList<GeneratorPlanDraftArtifactApprovalDiagnostic> diagnostics)
    {
        return diagnostics
            .Where(diagnostic => diagnostic.Code == GeneratorPlanDraftArtifactApprovalDiagnosticCodes.ProductionDiagnostic)
            .ToList();
    }

    private static string NormalizeDecision(string decision)
    {
        return decision.Trim().ToLowerInvariant();
    }

    private static string ToItemState(string decision, string currentState)
    {
        return decision switch
        {
            GeneratorPlanDraftArtifactApprovalDecisionKind.Approved => GeneratorPlanDraftArtifactApprovalItemState.Approved,
            GeneratorPlanDraftArtifactApprovalDecisionKind.Rejected => GeneratorPlanDraftArtifactApprovalItemState.Rejected,
            GeneratorPlanDraftArtifactApprovalDecisionKind.RepairRequested => GeneratorPlanDraftArtifactApprovalItemState.RepairRequested,
            _ => currentState
        };
    }

    private static bool IsValidJson(string contentJson)
    {
        try
        {
            using var _ = JsonDocument.Parse(string.IsNullOrWhiteSpace(contentJson) ? string.Empty : contentJson);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private sealed record StoredStagingSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public bool Ok { get; init; }
        public string Status { get; init; } = string.Empty;
        public GeneratorPlanDraftArtifactStagingSnapshot Snapshot { get; init; } = new();
        public IReadOnlyList<GeneratorPlanDraftArtifactApprovalDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanDraftArtifactApprovalDiagnostic>();
    }
}
