using LLMGameCreator.Application.Generation.Procedural;

namespace LLMGameCreator.Application.Projects;

public sealed class GameProjectSummary
{
    public string FolderPath { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;
    public string? PackageId { get; set; }
    public string? Title { get; set; }
    public string? Version { get; set; }
    public bool HasPackageFile { get; set; }
    public bool IsValidPackage { get; set; }
    public bool HasValidationErrors { get; set; }
    public string? ErrorMessage { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public string CreationKind { get; set; } = GameProjectCreationKinds.Template;
    public string GenerationSeed { get; set; } = string.Empty;
    public string GenerationMode { get; set; } = string.Empty;
    public string GenerationPresetId { get; set; } = string.Empty;
    public string MechanicsProfileId { get; set; } = string.Empty;
    public bool GeneratedSourcePresent { get; set; }
    public string GeneratedSourceStatus { get; set; } = "ABSENT";
    public GeneratedProjectCounts GeneratedCounts { get; set; } = new();
}
