using System.Text.Json.Nodes;

namespace LLMGameCreator.Scripting;

public sealed class PrototypeLuaExecutionRequest
{
    public string ScriptId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? SourcePath { get; set; }
    public int? TimeoutMs { get; set; }
    public int? MaxDeclarations { get; set; }
    public int? MaxInstructionCount { get; set; }
}

public sealed class PrototypeLuaExecutionResult
{
    public bool Success { get; set; }
    public List<PrototypeLuaDeclaration> Declarations { get; set; } = new List<PrototypeLuaDeclaration>();
    public List<PrototypeLuaDiagnostic> Diagnostics { get; set; } = new List<PrototypeLuaDiagnostic>();
    public long ElapsedMs { get; set; }
}

public sealed class PrototypeLuaDeclaration
{
    public string Type { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public JsonObject Json { get; set; } = new JsonObject();
    public int SourceIndex { get; set; }
}

public sealed class PrototypeLuaDiagnostic
{
    public string Severity { get; set; } = "info";
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
}

