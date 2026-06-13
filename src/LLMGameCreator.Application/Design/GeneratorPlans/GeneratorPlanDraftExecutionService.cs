namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftExecutionService
{
    private readonly GeneratorPlanPreviewService _previewService;
    private readonly GeneratorPlanDraftExecutionPlanner _planner;
    private readonly GeneratorPlanDraftExecutionValidator _validator;
    private readonly GeneratorPlanDraftExecutionMarkdownRenderer _markdownRenderer;

    public GeneratorPlanDraftExecutionService()
        : this(
            new GeneratorPlanPreviewService(),
            new GeneratorPlanDraftExecutionPlanner(),
            new GeneratorPlanDraftExecutionValidator(),
            new GeneratorPlanDraftExecutionMarkdownRenderer())
    {
    }

    public GeneratorPlanDraftExecutionService(
        GeneratorPlanPreviewService previewService,
        GeneratorPlanDraftExecutionPlanner planner,
        GeneratorPlanDraftExecutionValidator validator,
        GeneratorPlanDraftExecutionMarkdownRenderer markdownRenderer)
    {
        _previewService = previewService;
        _planner = planner;
        _validator = validator;
        _markdownRenderer = markdownRenderer;
    }

    public Task<GeneratorPlanDraftExecutionResult> CreateDraftAsync(
        GeneratorPlanPreviewResult previewResult,
        GeneratorPlanDraftExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(previewResult);
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var generatedAtUtc = DateTimeOffset.UtcNow;
        var planned = _planner.CreateDraftPlan(previewResult.Preview, new GeneratorPlanDraftExecutionPlannerOptions
        {
            PlanId = request.PlanId,
            RequireHumanApprovalByDefault = request.RequireHumanApprovalByDefault
        });
        var plan = _validator.Validate(planned);
        var status = plan.Status == GeneratorPlanDraftExecutionStatus.Invalid || plan.Summary.ErrorCount > 0
            ? GeneratorPlanDraftExecutionStatus.Invalid
            : plan.Status;
        var markdown = request.RenderMarkdown ? _markdownRenderer.Render(plan) : string.Empty;

        var result = new GeneratorPlanDraftExecutionResult
        {
            Ok = status != GeneratorPlanDraftExecutionStatus.Invalid,
            Status = status,
            GeneratedAtUtc = generatedAtUtc,
            PreviewResult = previewResult,
            Plan = plan with { Status = status },
            MarkdownReport = markdown,
            Diagnostics = plan.Diagnostics
        };

        return Task.FromResult(result);
    }

    public async Task<GeneratorPlanDraftExecutionResult> CreateDraftFromExampleAsync(
        string examplePath,
        GeneratorPlanDraftExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(examplePath))
        {
            throw new ArgumentException("Example path is required.", nameof(examplePath));
        }

        ArgumentNullException.ThrowIfNull(request);

        var previewResult = await _previewService
            .PreviewAsync(
                new GeneratorPlanPreviewRequest
                {
                    SourcePath = examplePath,
                    RenderMarkdown = false
                },
                cancellationToken)
            .ConfigureAwait(false);

        return await CreateDraftAsync(previewResult, request, cancellationToken).ConfigureAwait(false);
    }
}
