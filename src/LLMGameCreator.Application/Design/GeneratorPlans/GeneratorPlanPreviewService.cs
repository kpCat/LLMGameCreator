namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanPreviewService
{
    private readonly GeneratorPlanPreviewLoader _loader;
    private readonly GeneratorPlanPreviewValidator _validator;
    private readonly GeneratorPlanPreviewMarkdownRenderer _markdownRenderer;

    public GeneratorPlanPreviewService()
        : this(new GeneratorPlanPreviewLoader(), new GeneratorPlanPreviewValidator(), new GeneratorPlanPreviewMarkdownRenderer())
    {
    }

    public GeneratorPlanPreviewService(
        GeneratorPlanPreviewLoader loader,
        GeneratorPlanPreviewValidator validator,
        GeneratorPlanPreviewMarkdownRenderer markdownRenderer)
    {
        _loader = loader;
        _validator = validator;
        _markdownRenderer = markdownRenderer;
    }

    public async Task<GeneratorPlanPreviewResult> PreviewAsync(
        GeneratorPlanPreviewRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.SourcePath))
        {
            throw new ArgumentException("Source path is required.", nameof(request));
        }

        var generatedAtUtc = DateTimeOffset.UtcNow;
        var loaded = await _loader.LoadAsync(request.SourcePath, cancellationToken).ConfigureAwait(false);
        var preview = loaded.Diagnostics.Any(diagnostic => diagnostic.Code == GeneratorPlanPreviewDiagnosticCodes.InvalidJson)
            ? loaded
            : _validator.Validate(loaded);
        var status = GeneratorPlanPreviewValidationState.FromSummary(preview.Summary);
        var markdown = request.RenderMarkdown ? _markdownRenderer.Render(preview) : string.Empty;

        return new GeneratorPlanPreviewResult
        {
            Ok = status != GeneratorPlanPreviewValidationState.Invalid,
            Status = status,
            GeneratedAtUtc = generatedAtUtc,
            Preview = preview,
            MarkdownReport = markdown,
            Diagnostics = preview.Diagnostics
        };
    }
}
