using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftArtifactApprovalArtifactService
{
    internal static readonly GeneratedArtifactRecord EmptyArtifact = new(
        string.Empty,
        string.Empty,
        string.Empty,
        "{}",
        string.Empty,
        string.Empty,
        "{}");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    private readonly GeneratorPlanDraftArtifactApprovalService _approvalService;
    private readonly IGeneratedArtifactRepository _artifactRepository;

    public GeneratorPlanDraftArtifactApprovalArtifactService(
        GeneratorPlanDraftArtifactApprovalService approvalService,
        IGeneratedArtifactRepository artifactRepository)
    {
        _approvalService = approvalService;
        _artifactRepository = artifactRepository;
    }

    public async Task<GeneratorPlanDraftArtifactApprovalArtifactResult> CaptureAsync(
        GeneratorPlanDraftArtifactApprovalArtifactRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.PreviewRequest.SourcePath))
        {
            throw new ArgumentException("Preview source path is required.", nameof(request));
        }

        var approvalResult = await _approvalService
            .CreateSnapshotFromExampleAsync(request.PreviewRequest.SourcePath, request.ApprovalRequest, cancellationToken)
            .ConfigureAwait(false);

        return await SaveAsync(
            approvalResult,
            new GeneratorPlanDraftArtifactApprovalArtifactSaveRequest
            {
                StagingArtifactId = request.StagingArtifactId,
                MarkdownArtifactId = request.MarkdownArtifactId,
                ApprovedArtifactSetArtifactId = request.ApprovedArtifactSetArtifactId,
                GeneratedBy = request.GeneratedBy
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<GeneratorPlanDraftArtifactApprovalArtifactResult> SaveAsync(
        GeneratorPlanDraftArtifactApprovalResult approvalResult,
        GeneratorPlanDraftArtifactApprovalArtifactSaveRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(approvalResult);
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.StagingArtifactId))
        {
            throw new ArgumentException("Staging artifact id is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.ApprovedArtifactSetArtifactId))
        {
            throw new ArgumentException("Approved artifact set artifact id is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.GeneratedBy))
        {
            throw new ArgumentException("GeneratedBy is required.", nameof(request));
        }

        var stagingArtifact = BuildStagingArtifact(request, approvalResult);
        var markdownArtifact = BuildMarkdownArtifact(request, approvalResult);
        var approvedArtifactSetArtifact = BuildApprovedArtifactSetArtifact(request, approvalResult);
        var validationResults = GeneratorPlanDraftArtifactApprovalPolicy.ToValidationResults(stagingArtifact.Id, approvalResult.Diagnostics);

        await _artifactRepository.SaveGeneratedArtifactAsync(stagingArtifact, cancellationToken).ConfigureAwait(false);
        await _artifactRepository.SaveValidationResultsAsync(stagingArtifact.Id, validationResults, cancellationToken).ConfigureAwait(false);

        if (markdownArtifact != null)
        {
            await _artifactRepository.SaveGeneratedArtifactAsync(markdownArtifact, cancellationToken).ConfigureAwait(false);
        }

        await _artifactRepository.SaveGeneratedArtifactAsync(approvedArtifactSetArtifact, cancellationToken).ConfigureAwait(false);

        return new GeneratorPlanDraftArtifactApprovalArtifactResult
        {
            ApprovalResult = approvalResult,
            StagingArtifact = stagingArtifact,
            MarkdownArtifact = markdownArtifact,
            ApprovedArtifactSetArtifact = approvedArtifactSetArtifact,
            ValidationResults = validationResults
        };
    }

    private static GeneratedArtifactRecord BuildStagingArtifact(
        GeneratorPlanDraftArtifactApprovalArtifactSaveRequest request,
        GeneratorPlanDraftArtifactApprovalResult approvalResult)
    {
        var json = JsonSerializer.Serialize(new GeneratorPlanDraftArtifactApprovalArtifactSnapshot
        {
            GeneratedAtUtc = approvalResult.GeneratedAtUtc,
            Ok = approvalResult.Ok,
            Status = approvalResult.Status,
            Snapshot = approvalResult.Snapshot,
            Diagnostics = approvalResult.Diagnostics
        }, JsonOptions);

        return new GeneratedArtifactRecord(
            request.StagingArtifactId.Trim(),
            GeneratorPlanDraftArtifactApprovalArtifactIds.StagingArtifactKind,
            GeneratorPlanDraftArtifactApprovalArtifactIds.StagingArtifactPath,
            json,
            request.GeneratedBy.Trim(),
            GeneratorPlanDraftArtifactApprovalPolicy.ToValidationState(approvalResult.Snapshot.Summary),
            BuildMetadataJson(approvalResult));
    }

    private static GeneratedArtifactRecord? BuildMarkdownArtifact(
        GeneratorPlanDraftArtifactApprovalArtifactSaveRequest request,
        GeneratorPlanDraftArtifactApprovalResult approvalResult)
    {
        if (string.IsNullOrWhiteSpace(approvalResult.MarkdownReport))
        {
            return null;
        }

        var id = string.IsNullOrWhiteSpace(request.MarkdownArtifactId)
            ? GeneratorPlanDraftArtifactApprovalArtifactIds.MarkdownArtifactId
            : request.MarkdownArtifactId.Trim();

        var json = JsonSerializer.Serialize(new GeneratorPlanDraftArtifactApprovalMarkdownArtifactSnapshot
        {
            GeneratedAtUtc = approvalResult.GeneratedAtUtc,
            Markdown = approvalResult.MarkdownReport
        }, JsonOptions);

        return new GeneratedArtifactRecord(
            id,
            GeneratorPlanDraftArtifactApprovalArtifactIds.MarkdownArtifactKind,
            GeneratorPlanDraftArtifactApprovalArtifactIds.MarkdownArtifactPath,
            json,
            request.GeneratedBy.Trim(),
            GeneratorPlanDraftArtifactApprovalPolicy.ToValidationState(approvalResult.Snapshot.Summary),
            BuildMetadataJson(approvalResult));
    }

    private static GeneratedArtifactRecord BuildApprovedArtifactSetArtifact(
        GeneratorPlanDraftArtifactApprovalArtifactSaveRequest request,
        GeneratorPlanDraftArtifactApprovalResult approvalResult)
    {
        var json = BuildApprovedArtifactSetJson(approvalResult.Snapshot);

        return new GeneratedArtifactRecord(
            request.ApprovedArtifactSetArtifactId.Trim(),
            GeneratorPlanDraftArtifactApprovalArtifactIds.ApprovedArtifactSetArtifactKind,
            GeneratorPlanDraftArtifactApprovalArtifactIds.ApprovedArtifactSetArtifactPath,
            json,
            request.GeneratedBy.Trim(),
            GeneratorPlanDraftArtifactApprovalPolicy.ToValidationState(approvalResult.Snapshot.Summary),
            BuildMetadataJson(approvalResult));
    }

    private static string BuildApprovedArtifactSetJson(GeneratorPlanDraftArtifactStagingSnapshot snapshot)
    {
        var approvedArtifacts = new JsonArray();
        foreach (var item in snapshot.Items.Where(item => item.State == GeneratorPlanDraftArtifactApprovalItemState.Approved).OrderBy(item => item.ArtifactId, StringComparer.OrdinalIgnoreCase))
        {
            approvedArtifacts.Add(new JsonObject
            {
                ["artifact_id"] = item.ArtifactId,
                ["artifact_kind"] = item.ArtifactKind,
                ["expected_artifact_contract"] = item.ExpectedArtifactContract,
                ["content_json"] = ParseContent(item.ContentJson)
            });
        }

        var root = new JsonObject
        {
            ["schema_version"] = "0.1",
            ["snapshot_id"] = snapshot.Id,
            ["source_production_batch_id"] = snapshot.SourceProductionBatchId,
            ["approved_artifacts"] = approvedArtifacts
        };

        return root.ToJsonString(JsonOptions);
    }

    private static JsonNode? ParseContent(string contentJson)
    {
        try
        {
            return JsonNode.Parse(contentJson);
        }
        catch (JsonException)
        {
            return contentJson;
        }
    }

    private static string BuildMetadataJson(GeneratorPlanDraftArtifactApprovalResult approvalResult)
    {
        return JsonSerializer.Serialize(new
        {
            generatedAtUtc = approvalResult.GeneratedAtUtc,
            snapshotId = approvalResult.Snapshot.Id,
            sourceProductionBatchId = approvalResult.Snapshot.SourceProductionBatchId,
            sourcePreviewExampleId = approvalResult.Snapshot.SourcePreviewExampleId,
            sourcePath = approvalResult.Snapshot.SourcePath,
            status = approvalResult.Snapshot.Status,
            itemCount = approvalResult.Snapshot.Summary.ItemCount,
            approvedCount = approvalResult.Snapshot.Summary.ApprovedCount,
            rejectedCount = approvalResult.Snapshot.Summary.RejectedCount,
            repairRequestedCount = approvalResult.Snapshot.Summary.RepairRequestedCount,
            blockedCount = approvalResult.Snapshot.Summary.BlockedCount,
            readyForPackageCount = approvalResult.Snapshot.Summary.ReadyForPackageCount,
            errorCount = approvalResult.Snapshot.Summary.ErrorCount,
            warningCount = approvalResult.Snapshot.Summary.WarningCount
        }, JsonOptions);
    }

    private sealed record GeneratorPlanDraftArtifactApprovalArtifactSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public bool Ok { get; init; }
        public string Status { get; init; } = string.Empty;
        public GeneratorPlanDraftArtifactStagingSnapshot Snapshot { get; init; } = new();
        public IReadOnlyList<GeneratorPlanDraftArtifactApprovalDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanDraftArtifactApprovalDiagnostic>();
    }

    private sealed record GeneratorPlanDraftArtifactApprovalMarkdownArtifactSnapshot
    {
        public DateTimeOffset GeneratedAtUtc { get; init; }
        public string Markdown { get; init; } = string.Empty;
    }
}
