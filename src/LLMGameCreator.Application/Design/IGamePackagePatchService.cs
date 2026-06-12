namespace LLMGameCreator.Application.Design;

public interface IGamePackagePatchService
{
    Task<GamePackagePatchCreateResult> CreatePatchArtifactFromPreviewAsync(string previewArtifactId, CancellationToken cancellationToken);
    Task<GamePackagePatchDryRunResult> DryRunPatchArtifactAsync(string patchArtifactId, CancellationToken cancellationToken);
    Task<GamePackagePatchApplyResult> ApplyPatchArtifactAsync(string patchArtifactId, CancellationToken cancellationToken);
}

