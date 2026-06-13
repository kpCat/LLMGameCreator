namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanDraftArtifactQueueService
{
    private readonly GeneratorPlanDraftExecutionService _draftExecutionService;
    private readonly GeneratorPlanDraftArtifactQueueBuilder _builder;
    private readonly GeneratorPlanDraftArtifactQueueValidator _validator;
    private readonly GeneratorPlanDraftArtifactQueueMarkdownRenderer _markdownRenderer;

    public GeneratorPlanDraftArtifactQueueService()
        : this(
            new GeneratorPlanDraftExecutionService(),
            new GeneratorPlanDraftArtifactQueueBuilder(),
            new GeneratorPlanDraftArtifactQueueValidator(),
            new GeneratorPlanDraftArtifactQueueMarkdownRenderer())
    {
    }

    public GeneratorPlanDraftArtifactQueueService(
        GeneratorPlanDraftExecutionService draftExecutionService,
        GeneratorPlanDraftArtifactQueueBuilder builder,
        GeneratorPlanDraftArtifactQueueValidator validator,
        GeneratorPlanDraftArtifactQueueMarkdownRenderer markdownRenderer)
    {
        _draftExecutionService = draftExecutionService;
        _builder = builder;
        _validator = validator;
        _markdownRenderer = markdownRenderer;
    }

    public Task<GeneratorPlanDraftArtifactQueueResult> CreateQueueAsync(
        GeneratorPlanDraftExecutionResult draftExecutionResult,
        GeneratorPlanDraftArtifactQueueRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(draftExecutionResult);
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var generatedAtUtc = DateTimeOffset.UtcNow;
        var built = _builder.BuildQueue(draftExecutionResult.Plan, new GeneratorPlanDraftArtifactQueueBuilderOptions
        {
            QueueId = request.QueueId,
            CreateRepairRequestsForBlockedItems = request.CreateRepairRequestsForBlockedItems
        });
        var queue = _validator.Validate(built);
        var status = !draftExecutionResult.Ok || draftExecutionResult.Status == GeneratorPlanDraftExecutionStatus.Invalid || queue.Summary.ErrorCount > 0
            ? GeneratorPlanDraftArtifactQueueStatus.Invalid
            : queue.Status;
        queue = queue with { Status = status };
        var markdown = request.RenderMarkdown ? _markdownRenderer.Render(queue) : string.Empty;

        return Task.FromResult(new GeneratorPlanDraftArtifactQueueResult
        {
            Ok = status != GeneratorPlanDraftArtifactQueueStatus.Invalid,
            Status = status,
            GeneratedAtUtc = generatedAtUtc,
            DraftExecutionResult = draftExecutionResult,
            Queue = queue,
            MarkdownReport = markdown,
            Diagnostics = queue.Diagnostics
        });
    }

    public async Task<GeneratorPlanDraftArtifactQueueResult> CreateQueueFromExampleAsync(
        string examplePath,
        GeneratorPlanDraftArtifactQueueRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(examplePath))
        {
            throw new ArgumentException("Example path is required.", nameof(examplePath));
        }

        ArgumentNullException.ThrowIfNull(request);

        var draftResult = await _draftExecutionService
            .CreateDraftFromExampleAsync(examplePath, request.DraftExecutionRequest, cancellationToken)
            .ConfigureAwait(false);

        return await CreateQueueAsync(draftResult, request, cancellationToken).ConfigureAwait(false);
    }
}
