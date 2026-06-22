using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Composition;

internal sealed class UnityArchiveRequestBuildContext
{
    public UnityArchiveRequestPipelineRequest Request { get; }
    public GamePackageDefinition? Package { get; }
    public GameDesignBrief DesignBrief { get; }
    public UnityTargetProfile TargetProfile { get; }
    public UnityGameArchiveManifest ArchiveManifest { get; }
    public HashSet<string> RuntimeModuleIds { get; }
    public IReadOnlyList<string> StyleTags { get; }

    public bool HasItems { get; }
    public bool HasNpcs { get; }
    public bool HasScenes { get; }
    public bool HasAbilities { get; }
    public bool HasMechanics { get; }
    public bool HasTilePrototypes { get; }
    public bool HasQuests { get; }
    public bool HasDialogues { get; }
    public bool HasCrafting { get; }
    public bool HasFactions { get; }
    public bool HasDynamicUi { get; }

    public UnityArchiveRequestBuildContext(UnityArchiveRequestPipelineRequest request)
    {
        Request = request;
        Package = request.Package;
        DesignBrief = request.DesignBrief;
        TargetProfile = request.TargetProfile;
        ArchiveManifest = request.ArchiveManifest;
        RuntimeModuleIds = new HashSet<string>(ArchiveManifest.RuntimeModuleIds, StringComparer.OrdinalIgnoreCase);

        HasItems = Package is not null && (Package.Game.Items.Count > 0 || Package.GeneratedContent.Items.Count > 0);
        HasNpcs = Package is not null && Package.GeneratedContent.Npcs.Count > 0;
        HasScenes = Package is not null && (Package.Game.Maps.Count > 0 || Package.GeneratedContent.Scenes.Count > 0);
        HasAbilities = Package is not null && Package.Game.Abilities.Count > 0;
        HasMechanics = Package is not null && Package.GeneratedContent.Mechanics.Count > 0;
        HasTilePrototypes = Package is not null && Package.Game.TilePrototypes.Count > 0;
        HasQuests = Package is not null && (Package.Game.Quests.Count > 0 || Package.GeneratedContent.Quests.Count > 0);
        HasDialogues = Package is not null && (Package.Game.Dialogues.Count > 0 || Package.GeneratedContent.Dialogues.Count > 0);
        HasCrafting = Package is not null && Package.Game.Recipes.Count > 0;
        HasFactions = Package is not null && Package.Game.Factions.Count > 0;
        HasDynamicUi = DesignBrief.ViewModeWishes.Any(w => w.Required) || ArchiveManifest.UiLayouts.Count > 0;

        StyleTags = DesignBrief.AssetStyleWishes
            .Select(w => w.WishId.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public bool HasLuaModule(string moduleId)
    {
        return RuntimeModuleIds.Contains(moduleId);
    }
}