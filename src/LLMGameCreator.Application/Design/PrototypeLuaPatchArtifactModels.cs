namespace LLMGameCreator.Application.Design;

public sealed class PrototypeLuaPatchArtifactRequest
{
    public string ScriptId { get; set; } = string.Empty;
    public string Title { get; set; } = "Prototype Lua";
    public string Source { get; set; } = string.Empty;
    public string? SourcePath { get; set; }
    public string? PlanId { get; set; }
    public string? SourceArtifactId { get; set; }
    public bool DryRun { get; set; }
    public int? TimeoutMs { get; set; }
    public int? MaxDeclarations { get; set; }
    public int? MaxInstructionCount { get; set; }
}

public sealed record PrototypeLuaPatchArtifactResult(
    GeneratedArtifactRecord? PatchArtifact,
    IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults,
    GamePackagePatchDryRunResult? DryRunResult,
    bool Saved,
    string Message);

