namespace LLMGameCreator.Application.Design.Atlas;

public sealed class AtlasRegistryPipelineService
{
    private readonly AtlasRegistryPreviewService _previewService;
    private readonly AtlasRegistryPreviewArtifactService? _artifactService;

    public AtlasRegistryPipelineService(
        AtlasRegistryPreviewService previewService,
        AtlasRegistryPreviewArtifactService? artifactService = null)
    {
        _previewService = previewService;
        _artifactService = artifactService;
    }

    public async Task<AtlasRegistryPipelineRunResult> RunPreviewAsync(
        AtlasRegistryPipelineRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!request.PersistArtifacts)
        {
            var preview = await _previewService
                .PreviewAsync(request.PreviewRequest, cancellationToken)
                .ConfigureAwait(false);

            return new AtlasRegistryPipelineRunResult
            {
                PreviewResult = preview,
                ValidationResults = AtlasRegistryValidationPolicy.ToValidationResults(
                    request.ResultArtifactId,
                    preview.ImportResult.Diagnostics)
            };
        }

        if (_artifactService == null)
        {
            throw new InvalidOperationException("Atlas registry artifact service is required when PersistArtifacts is enabled.");
        }

        var capture = await _artifactService
            .CaptureAsync(new AtlasRegistryPreviewArtifactRequest
            {
                PreviewRequest = request.PreviewRequest,
                ResultArtifactId = request.ResultArtifactId,
                MarkdownArtifactId = request.MarkdownArtifactId,
                GeneratedBy = request.GeneratedBy
            }, cancellationToken)
            .ConfigureAwait(false);

        return new AtlasRegistryPipelineRunResult
        {
            PreviewResult = capture.PreviewResult,
            ResultArtifact = capture.ResultArtifact,
            MarkdownArtifact = capture.MarkdownArtifact,
            ValidationResults = capture.ValidationResults,
            PersistedArtifacts = true
        };
    }
}
