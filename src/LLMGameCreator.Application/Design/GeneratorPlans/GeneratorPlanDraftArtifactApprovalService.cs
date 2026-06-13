using System.Text.Json;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftArtifactApprovalService
{
    private readonly GeneratorPlanDraftArtifactProductionService _productionService;
    private readonly GeneratorPlanDraftArtifactApprovalValidator _validator;
    private readonly GeneratorPlanDraftArtifactApprovalMarkdownRenderer _markdownRenderer;

    public GeneratorPlanDraftArtifactApprovalService()
        : this(
            new GeneratorPlanDraftArtifactProductionService(),
            new GeneratorPlanDraftArtifactApprovalValidator(),
            new GeneratorPlanDraftArtifactApprovalMarkdownRenderer())
    {
    }

    public GeneratorPlanDraftArtifactApprovalService(
        GeneratorPlanDraftArtifactProductionService productionService,
        GeneratorPlanDraftArtifactApprovalValidator validator,
        GeneratorPlanDraftArtifactApprovalMarkdownRenderer markdownRenderer)
    {
        _productionService = productionService;
        _validator = validator;
        _markdownRenderer = markdownRenderer;
    }

    public GeneratorPlanDraftArtifactApprovalResult CreateSnapshot(
        GeneratorPlanDraftArtifactProductionResult productionResult,
        GeneratorPlanDraftArtifactApprovalRequest request)
    {
        ArgumentNullException.ThrowIfNull(productionResult);
        ArgumentNullException.ThrowIfNull(request);

        var generatedAtUtc = DateTimeOffset.UtcNow;
        var snapshotId = string.IsNullOrWhiteSpace(request.SnapshotId)
            ? $"draft_artifact_staging/{productionResult.Batch.Id}"
            : request.SnapshotId.Trim();
        var decisionByArtifact = request.Decisions
            .Where(decision => !string.IsNullOrWhiteSpace(decision.ArtifactId))
            .GroupBy(decision => decision.ArtifactId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);
        var diagnostics = MapProductionDiagnostics(snapshotId, productionResult).ToList();
        var items = productionResult.Batch.Artifacts
            .OrderBy(artifact => artifact.ArtifactId, StringComparer.OrdinalIgnoreCase)
            .Select(artifact => BuildItem(snapshotId, productionResult.Batch.Id, artifact, decisionByArtifact, request))
            .ToList();

        var snapshot = new GeneratorPlanDraftArtifactStagingSnapshot
        {
            Id = snapshotId,
            SourceProductionBatchId = productionResult.Batch.Id,
            SourcePreviewExampleId = productionResult.Batch.SourcePreviewExampleId,
            SourcePath = productionResult.Batch.SourcePath,
            Items = items,
            Diagnostics = diagnostics
        };

        snapshot = _validator.Validate(snapshot);
        var markdown = request.RenderMarkdown ? _markdownRenderer.Render(snapshot) : string.Empty;

        return new GeneratorPlanDraftArtifactApprovalResult
        {
            Ok = snapshot.Status != GeneratorPlanDraftArtifactStagingStatus.Invalid,
            Status = snapshot.Status,
            GeneratedAtUtc = generatedAtUtc,
            ProductionResult = productionResult,
            Snapshot = snapshot,
            MarkdownReport = markdown,
            Diagnostics = snapshot.Diagnostics
        };
    }

    public async Task<GeneratorPlanDraftArtifactApprovalResult> CreateSnapshotFromExampleAsync(
        string examplePath,
        GeneratorPlanDraftArtifactApprovalRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(examplePath))
        {
            throw new ArgumentException("Example path is required.", nameof(examplePath));
        }

        ArgumentNullException.ThrowIfNull(request);

        var productionResult = await _productionService
            .ProduceFromExampleAsync(examplePath, new GeneratorPlanDraftArtifactProductionRequest { RenderMarkdown = false }, cancellationToken)
            .ConfigureAwait(false);

        return CreateSnapshot(productionResult, request);
    }

    private static GeneratorPlanDraftArtifactApprovalItem BuildItem(
        string snapshotId,
        string batchId,
        GeneratorPlanProducedDraftArtifact artifact,
        IReadOnlyDictionary<string, GeneratorPlanDraftArtifactApprovalDecision> decisionByArtifact,
        GeneratorPlanDraftArtifactApprovalRequest request)
    {
        decisionByArtifact.TryGetValue(artifact.ArtifactId, out var decision);

        var validationIssues = new List<string>();
        if (!IsValidJson(artifact.ContentJson))
        {
            validationIssues.Add(GeneratorPlanDraftArtifactApprovalDiagnosticCodes.ItemInvalidJson);
        }

        var state = artifact.State == GeneratorPlanProducedDraftArtifactState.Blocked
            ? GeneratorPlanDraftArtifactApprovalItemState.Blocked
            : GeneratorPlanDraftArtifactApprovalItemState.Pending;

        if (request.AutoApproveValidArtifacts
            && artifact.State == GeneratorPlanProducedDraftArtifactState.ReadyForApproval
            && validationIssues.Count == 0
            && !artifact.RequiresHumanApproval)
        {
            state = GeneratorPlanDraftArtifactApprovalItemState.Approved;
        }

        if (request.AutoApproveValidArtifacts
            && artifact.State == GeneratorPlanProducedDraftArtifactState.ReadyForApproval
            && validationIssues.Count == 0
            && artifact.RequiresHumanApproval)
        {
            state = GeneratorPlanDraftArtifactApprovalItemState.Approved;
        }

        if (decision != null)
        {
            state = decision.Decision switch
            {
                GeneratorPlanDraftArtifactApprovalDecisionKind.Approved => GeneratorPlanDraftArtifactApprovalItemState.Approved,
                GeneratorPlanDraftArtifactApprovalDecisionKind.Rejected => GeneratorPlanDraftArtifactApprovalItemState.Rejected,
                GeneratorPlanDraftArtifactApprovalDecisionKind.RepairRequested => GeneratorPlanDraftArtifactApprovalItemState.RepairRequested,
                _ => artifact.State == GeneratorPlanProducedDraftArtifactState.Blocked
                    ? GeneratorPlanDraftArtifactApprovalItemState.Blocked
                    : GeneratorPlanDraftArtifactApprovalItemState.Pending
            };
        }

        return new GeneratorPlanDraftArtifactApprovalItem
        {
            ArtifactId = artifact.ArtifactId,
            ArtifactKind = artifact.ArtifactKind,
            State = state,
            SourceProductionBatchId = batchId,
            QueueItemId = artifact.QueueItemId,
            SourceExecutionStepId = artifact.SourceExecutionStepId,
            ExpectedArtifactContract = artifact.ExpectedArtifactContract,
            ContentJson = artifact.ContentJson,
            RequiresHumanApproval = artifact.RequiresHumanApproval,
            RepairRequestId = artifact.RepairRequestId,
            DecisionReasonCode = decision?.ReasonCode.Trim() ?? string.Empty,
            DecisionComment = decision?.Comment.Trim() ?? string.Empty,
            ValidationIssues = validationIssues
        };
    }

    private static IEnumerable<GeneratorPlanDraftArtifactApprovalDiagnostic> MapProductionDiagnostics(
        string snapshotId,
        GeneratorPlanDraftArtifactProductionResult productionResult)
    {
        return productionResult.Diagnostics.Select(diagnostic => GeneratorPlanDraftArtifactApprovalPolicy.Diagnostic(
            diagnostic.Severity,
            GeneratorPlanDraftArtifactApprovalDiagnosticCodes.ProductionDiagnostic,
            $"Draft artifact production diagnostic {diagnostic.Code}: {diagnostic.Message}",
            snapshotId,
            diagnostic.ArtifactId,
            diagnostic.Target ?? diagnostic.QueueItemId ?? diagnostic.BatchId));
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
}
