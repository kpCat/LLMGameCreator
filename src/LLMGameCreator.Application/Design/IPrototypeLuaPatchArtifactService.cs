namespace LLMGameCreator.Application.Design;

public interface IPrototypeLuaPatchArtifactService
{
    Task<PrototypeLuaPatchArtifactResult> CreatePatchArtifactFromPrototypeLuaAsync(
        PrototypeLuaPatchArtifactRequest request,
        CancellationToken cancellationToken);
}

