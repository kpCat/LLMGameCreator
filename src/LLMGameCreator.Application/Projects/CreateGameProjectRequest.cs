namespace LLMGameCreator.Application.Projects;

public static class GameProjectCreationKinds
{
    public const string Template = "template";
    public const string SeededGenerated = "seeded_generated";
}

public static class GeneratedProjectMechanicsProfiles
{
    public const string AllSelectableDefaults = "all_selectable_defaults";
    public const string CoreOnly = "core_only";

    public static readonly IReadOnlyList<string> Supported =
    [
        AllSelectableDefaults,
        CoreOnly
    ];
}

public sealed class CreateGameProjectRequest
{
    public string GamesRootPath { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PackageId { get; set; } = string.Empty;
    public string Version { get; set; } = "0.1.0";
    public string CreationKind { get; set; } = GameProjectCreationKinds.Template;
    public string GenerationSeed { get; set; } = string.Empty;
    public string GenerationMode { get; set; } = string.Empty;
    public string GenerationPresetId { get; set; } = string.Empty;
    public string MechanicsProfileId { get; set; } = string.Empty;
    public IReadOnlyList<string> CompactStyleHintIds { get; set; } = [];
    public IReadOnlyList<string> SelectedVariantIds { get; set; } = [];
}
