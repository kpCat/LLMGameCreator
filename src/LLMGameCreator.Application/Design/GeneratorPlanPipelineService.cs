namespace LLMGameCreator.Application.Design;

public sealed class GeneratorPlanPipelineService : IGeneratorPlanPipelineService
{
    private readonly IGeneratorPlanRepository _planRepository;
    private readonly IGeneratorPlanReviewService _reviewService;
    private readonly IGeneratorPlanPreviewService _previewService;
    private readonly IGamePackagePatchService _patchService;

    public GeneratorPlanPipelineService(
        IGeneratorPlanRepository planRepository,
        IGeneratorPlanReviewService reviewService,
        IGeneratorPlanPreviewService previewService,
        IGamePackagePatchService patchService)
    {
        _planRepository = planRepository;
        _reviewService = reviewService;
        _previewService = previewService;
        _patchService = patchService;
    }

    public async Task<GeneratorPlanPipelineResult> PreparePatchPipelineAsync(string planId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(planId))
        {
            return Failure(null, "Plan id is required.", "pipeline.plan.id.empty", "plan");
        }

        var plan = await _planRepository.GetGeneratorPlanByIdAsync(planId, cancellationToken).ConfigureAwait(false);
        if (plan == null)
        {
            return Failure(null, $"Plan was not found: {planId}", "pipeline.plan.not_found", planId);
        }

        if (!plan.Status.Equals("approved", StringComparison.OrdinalIgnoreCase))
        {
            return Failure(plan, "Prepare Patch Pipeline requires an approved plan.", "pipeline.plan.not_approved", plan.Id);
        }

        var review = await _reviewService.RevalidatePlanAsync(plan.Id, cancellationToken).ConfigureAwait(false);
        if (HasErrors(review.ValidationIssues))
        {
            return new GeneratorPlanPipelineResult(
                review.Plan ?? plan,
                null,
                null,
                null,
                review.ValidationIssues,
                false,
                "Approved plan has current validation errors; pipeline was not prepared.");
        }

        var preview = await _previewService.CreatePreviewArtifactAsync(new GeneratorPlanPreviewRequest(plan.Id), cancellationToken).ConfigureAwait(false);
        if (!preview.Saved || preview.Artifact == null)
        {
            return new GeneratorPlanPipelineResult(
                preview.Plan ?? plan,
                null,
                null,
                null,
                preview.ValidationIssues,
                false,
                preview.Message);
        }

        var patch = await _patchService.CreatePatchArtifactFromPreviewAsync(preview.Artifact.Id, cancellationToken).ConfigureAwait(false);
        if (!patch.Saved || patch.PatchArtifact == null)
        {
            return new GeneratorPlanPipelineResult(
                plan,
                preview.Artifact,
                null,
                null,
                review.ValidationIssues,
                false,
                IsNoPackageOperationsResult(patch)
                    ? "Plan is valid but has no data-only package operations, so no patch artifact can be created."
                    : patch.Message);
        }

        var dryRun = await _patchService.DryRunPatchArtifactAsync(patch.PatchArtifact.Id, cancellationToken).ConfigureAwait(false);
        return new GeneratorPlanPipelineResult(
            plan,
            preview.Artifact,
            patch.PatchArtifact,
            dryRun,
            review.ValidationIssues,
            dryRun.CanApply,
            dryRun.CanApply ? "Patch pipeline prepared. Explicit apply is available." : dryRun.Message);
    }

    public Task<GamePackagePatchApplyResult> ApplyPreparedPatchAsync(string patchArtifactId, CancellationToken cancellationToken)
    {
        return _patchService.ApplyPatchArtifactAsync(patchArtifactId, cancellationToken);
    }

    private static GeneratorPlanPipelineResult Failure(GeneratorPlanRecord? plan, string message, string code, string target)
    {
        return new GeneratorPlanPipelineResult(
            plan,
            null,
            null,
            null,
            new[] { new GeneratorPlanValidationIssue("error", code, message, target) },
            false,
            message);
    }

    private static bool IsNoPackageOperationsResult(GamePackagePatchCreateResult result)
    {
        return result.ValidationResults.Any(item => item.Code.Equals("patch.preview.package_operations.empty", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasErrors(IReadOnlyList<GeneratorPlanValidationIssue> issues)
    {
        return issues.Any(issue => issue.Severity.Equals("error", StringComparison.OrdinalIgnoreCase));
    }
}
