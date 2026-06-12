namespace LLMGameCreator.Scripting;

public interface IPrototypeLuaExecutor
{
    Task<PrototypeLuaExecutionResult> ExecuteAsync(PrototypeLuaExecutionRequest request, CancellationToken cancellationToken);
}

