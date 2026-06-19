using LLMGameCreator.Domain.Definitions;

namespace LLMGameCreator.GamePackage;

public sealed class GamePackageDefinition
{
    public GameManifest Manifest { get; set; } = new GameManifest();
    public GameDefinition Game { get; set; } = new GameDefinition();
    public AssetCatalog AssetCatalog { get; set; } = new AssetCatalog();
    public ScriptCatalog ScriptCatalog { get; set; } = new ScriptCatalog();
    public GeneratedContentDefinition GeneratedContent { get; set; } = new GeneratedContentDefinition();
}

public sealed class GamePackagePaths
{
    public string RootPath { get; set; } = string.Empty;
    public string PackageFilePath => System.IO.Path.Combine(RootPath, "package.json");
    public string AssetsPath => System.IO.Path.Combine(RootPath, "assets");
    public string ScriptsPath => System.IO.Path.Combine(RootPath, "scripts");
    public string SavesPath => System.IO.Path.Combine(RootPath, "saves");
}

public sealed class GeneratedContentDefinition
{
    public GeneratedGameProfileDefinition Profile { get; set; } = new GeneratedGameProfileDefinition();
    public List<GeneratedSceneDefinition> Scenes { get; set; } = new List<GeneratedSceneDefinition>();
    public List<GeneratedQuestSeedDefinition> Quests { get; set; } = new List<GeneratedQuestSeedDefinition>();
    public List<GeneratedMechanicDefinition> Mechanics { get; set; } = new List<GeneratedMechanicDefinition>();
    public List<GeneratedContentArtifactProvenance> AppliedArtifacts { get; set; } = new List<GeneratedContentArtifactProvenance>();
    public List<PreservedGeneratedArtifactDefinition> PreservedArtifacts { get; set; } = new List<PreservedGeneratedArtifactDefinition>();
}

public sealed class GeneratedGameProfileDefinition
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
    public string PresentationMode { get; set; } = string.Empty;
    public string WorldTopology { get; set; } = string.Empty;
    public string ActorModel { get; set; } = string.Empty;
    public string CombatModel { get; set; } = string.Empty;
    public List<string> CoreLoop { get; set; } = new List<string>();
    public List<string> Pillars { get; set; } = new List<string>();
    public string SourceContextJson { get; set; } = "{}";
}

public sealed class GeneratedSceneDefinition
{
    public string SourceId { get; set; } = string.Empty;
    public string PackageMapId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
}

public sealed class GeneratedQuestSeedDefinition
{
    public string SourceId { get; set; } = string.Empty;
    public string PackageQuestId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Steps { get; set; } = new List<string>();
    public List<string> Objectives { get; set; } = new List<string>();
}

public sealed class GeneratedMechanicDefinition
{
    public string SourceId { get; set; } = string.Empty;
    public string PackageAbilityId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new List<string>();
}

public sealed class GeneratedContentArtifactProvenance
{
    public string ArtifactId { get; set; } = string.Empty;
    public string ContractId { get; set; } = string.Empty;
    public string ArtifactKind { get; set; } = string.Empty;
    public string CapabilitySelectionId { get; set; } = string.Empty;
    public string GeneratedAt { get; set; } = string.Empty;
    public string AuditId { get; set; } = string.Empty;
    public string AppliedAt { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
    public string MappingResult { get; set; } = string.Empty;
}

public sealed class PreservedGeneratedArtifactDefinition
{
    public string ArtifactId { get; set; } = string.Empty;
    public string ContractId { get; set; } = string.Empty;
    public string ArtifactKind { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string RawJson { get; set; } = "{}";
}
