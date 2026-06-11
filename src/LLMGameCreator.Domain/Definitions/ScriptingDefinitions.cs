namespace LLMGameCreator.Domain.Definitions;

public enum LuaScriptKind
{
    Prototype = 0,
    Generator = 1,
    Behavior = 2,
    Interaction = 3,
    Formula = 4,
    Event = 5,
    Migration = 6
}

public sealed class ScriptCatalog
{
    public List<ScriptDefinition> Scripts { get; set; } = new List<ScriptDefinition>();
    public List<GeneratorDefinition> Generators { get; set; } = new List<GeneratorDefinition>();
}

public sealed class ScriptDefinition
{
    public string Id { get; set; } = string.Empty;
    public LuaScriptKind Kind { get; set; } = LuaScriptKind.Event;
    public string Path { get; set; } = string.Empty;
    public List<string> EntryPoints { get; set; } = new List<string>();
    public List<string> Capabilities { get; set; } = new List<string>();
    public List<string> UsedBy { get; set; } = new List<string>();
}

public sealed class GeneratorDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Kind { get; set; } = "chunk";
    public string ScriptId { get; set; } = string.Empty;
    public string EntryPoint { get; set; } = "generate_chunk";
}
