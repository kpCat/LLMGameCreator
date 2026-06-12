namespace LLMGameCreator.Scripting;

public sealed class PrototypeLuaSandboxOptions
{
    public int DefaultTimeoutMs { get; set; } = 1000;
    public int DefaultMaxDeclarations { get; set; } = 100;
    public int DefaultMaxInstructionCount { get; set; } = 100000;
}

