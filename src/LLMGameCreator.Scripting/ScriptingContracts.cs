using LLMGameCreator.Domain.Definitions;

namespace LLMGameCreator.Scripting;

public sealed class ScriptExecutionRequest
{
    public string ScriptId { get; set; } = string.Empty;
    public LuaScriptKind Kind { get; set; }
    public string EntryPoint { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
}

public sealed class ScriptExecutionResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public List<EffectDefinition> Effects { get; set; } = new List<EffectDefinition>();
}

public interface IScriptEngine
{
    Task<ScriptExecutionResult> ExecuteAsync(ScriptExecutionRequest request, CancellationToken cancellationToken);
}

public sealed class NullScriptEngine : IScriptEngine
{
    public Task<ScriptExecutionResult> ExecuteAsync(ScriptExecutionRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ScriptExecutionResult
        {
            Success = false,
            Error = "Lua engine не подключён. В v0.1 это ожидаемая заглушка."
        });
    }
}
