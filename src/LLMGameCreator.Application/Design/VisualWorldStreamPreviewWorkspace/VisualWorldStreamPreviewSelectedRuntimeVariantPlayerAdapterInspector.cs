using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewSelectedRuntimeVariantPlayerAdapterInspector
{
    private readonly SelectedRuntimeVariantPlayerAdapterArtifactService _artifacts;

    public VisualWorldStreamPreviewSelectedRuntimeVariantPlayerAdapterInspector(
        SelectedRuntimeVariantPlayerAdapterArtifactService? artifacts = null)
    {
        _artifacts = artifacts ?? new SelectedRuntimeVariantPlayerAdapterArtifactService();
    }

    public SelectedRuntimeVariantPlayerAdapterDashboard Load(string repositoryRootPath) =>
        _artifacts.ReadDashboard(repositoryRootPath);
}
