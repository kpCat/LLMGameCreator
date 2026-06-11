using LLMGameCreator.Domain.Definitions;

namespace LLMGameCreator.GamePackage;

public sealed class GamePackageDefinition
{
    public GameManifest Manifest { get; set; } = new GameManifest();
    public GameDefinition Game { get; set; } = new GameDefinition();
    public AssetCatalog AssetCatalog { get; set; } = new AssetCatalog();
    public ScriptCatalog ScriptCatalog { get; set; } = new ScriptCatalog();
}

public sealed class GamePackagePaths
{
    public string RootPath { get; set; } = string.Empty;
    public string PackageFilePath => System.IO.Path.Combine(RootPath, "package.json");
    public string AssetsPath => System.IO.Path.Combine(RootPath, "assets");
    public string ScriptsPath => System.IO.Path.Combine(RootPath, "scripts");
    public string SavesPath => System.IO.Path.Combine(RootPath, "saves");
}
