using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.RuntimePreview;

public sealed class GeneratedPackageRuntimePreviewService
{
    public GeneratedPackageRuntimePreviewModel Build(GamePackageDefinition package, GameState? state)
    {
        ArgumentNullException.ThrowIfNull(package);

        var currentMapId = ResolveCurrentMapId(package, state);
        var currentMap = package.Game.Maps.FirstOrDefault(map =>
            string.Equals(map.Id, currentMapId, StringComparison.OrdinalIgnoreCase));
        var scene = ResolveScene(package, currentMapId);
        var generatedContent = package.GeneratedContent;
        var warnings = new List<string>();

        if (IsGeneratedContentEmpty(generatedContent))
        {
            warnings.Add("generatedContent is empty.");
        }

        if (!string.IsNullOrWhiteSpace(currentMapId) && scene == null)
        {
            warnings.Add($"No generated scene is mapped to current map: {currentMapId}.");
        }

        return new GeneratedPackageRuntimePreviewModel
        {
            PackageTitle = TrimOrEmpty(package.Manifest.Title),
            PackageDescription = TrimOrEmpty(package.Manifest.Description),
            CurrentMapId = currentMapId,
            CurrentMapName = currentMap?.Name.Trim() ?? string.Empty,
            CurrentScene = scene == null
                ? null
                : new GeneratedPackageRuntimePreviewScene
                {
                    SourceId = scene.SourceId.Trim(),
                    PackageMapId = scene.PackageMapId.Trim(),
                    Title = scene.Title.Trim(),
                    Description = scene.Description.Trim(),
                    Purpose = scene.Purpose.Trim()
                },
            Profile = BuildProfile(generatedContent.Profile),
            Regions = generatedContent.Regions.Select(region => new GeneratedPackageRuntimePreviewContentItem
            {
                SourceId = region.SourceId.Trim(),
                Title = region.Title.Trim(),
                Description = region.Description.Trim(),
                References = region.SceneIds.Where(NotBlank).Select(Trim).ToList()
            }).ToList(),
            Npcs = generatedContent.Npcs.Select(npc => new GeneratedPackageRuntimePreviewContentItem
            {
                SourceId = npc.SourceId.Trim(),
                Title = npc.Name.Trim(),
                Description = npc.Description.Trim(),
                References = new[] { npc.RegionId, npc.SceneId }.Where(NotBlank).Select(Trim).ToList()
            }).ToList(),
            Items = generatedContent.Items.Select(item => new GeneratedPackageRuntimePreviewContentItem
            {
                SourceId = item.SourceId.Trim(),
                Title = item.Name.Trim(),
                Description = item.Description.Trim()
            }).ToList(),
            Dialogues = generatedContent.Dialogues.Select(dialogue => new GeneratedPackageRuntimePreviewContentItem
            {
                SourceId = dialogue.SourceId.Trim(),
                Title = dialogue.Title.Trim(),
                Description = dialogue.Description.Trim(),
                References = new[] { dialogue.NpcId, dialogue.SceneId }.Where(NotBlank).Select(Trim).ToList()
            }).ToList(),
            Encounters = generatedContent.Encounters.Select(encounter => new GeneratedPackageRuntimePreviewContentItem
            {
                SourceId = encounter.SourceId.Trim(),
                Title = encounter.Title.Trim(),
                Description = encounter.Description.Trim(),
                References = new[] { encounter.RegionId, encounter.SceneId }.Concat(encounter.NpcIds).Where(NotBlank).Select(Trim).ToList()
            }).ToList(),
            Quests = generatedContent.Quests.Select(BuildQuest).ToList(),
            Mechanics = generatedContent.Mechanics.Select(BuildMechanic).ToList(),
            Provenance = generatedContent.AppliedArtifacts.Select(BuildProvenance).ToList(),
            Warnings = warnings
        };
    }

    private static string ResolveCurrentMapId(GamePackageDefinition package, GameState? state)
    {
        if (!string.IsNullOrWhiteSpace(state?.CurrentMapId))
        {
            return state.CurrentMapId.Trim();
        }

        if (!string.IsNullOrWhiteSpace(package.Manifest.StartMapId))
        {
            return package.Manifest.StartMapId.Trim();
        }

        return package.Game.Maps.FirstOrDefault()?.Id.Trim() ?? string.Empty;
    }

    private static GeneratedSceneDefinition? ResolveScene(GamePackageDefinition package, string currentMapId)
    {
        if (string.IsNullOrWhiteSpace(currentMapId))
        {
            return package.GeneratedContent.Scenes.FirstOrDefault();
        }

        return package.GeneratedContent.Scenes.FirstOrDefault(scene =>
                   string.Equals(scene.PackageMapId, currentMapId, StringComparison.OrdinalIgnoreCase))
               ?? package.GeneratedContent.Scenes.FirstOrDefault();
    }

    private static GeneratedPackageRuntimePreviewProfile BuildProfile(GeneratedGameProfileDefinition profile)
    {
        return new GeneratedPackageRuntimePreviewProfile
        {
            Title = profile.Title.Trim(),
            Description = profile.Description.Trim(),
            Genre = profile.Genre.Trim(),
            Tone = profile.Tone.Trim(),
            CoreLoop = profile.CoreLoop.Where(NotBlank).Select(Trim).ToList(),
            Pillars = profile.Pillars.Where(NotBlank).Select(Trim).ToList()
        };
    }

    private static GeneratedPackageRuntimePreviewQuest BuildQuest(GeneratedQuestSeedDefinition quest)
    {
        return new GeneratedPackageRuntimePreviewQuest
        {
            SourceId = quest.SourceId.Trim(),
            PackageQuestId = quest.PackageQuestId.Trim(),
            Title = quest.Title.Trim(),
            Description = quest.Description.Trim(),
            Steps = quest.Steps.Where(NotBlank).Select(Trim).ToList(),
            Objectives = quest.Objectives.Where(NotBlank).Select(Trim).ToList()
        };
    }

    private static GeneratedPackageRuntimePreviewMechanic BuildMechanic(GeneratedMechanicDefinition mechanic)
    {
        return new GeneratedPackageRuntimePreviewMechanic
        {
            SourceId = mechanic.SourceId.Trim(),
            PackageAbilityId = mechanic.PackageAbilityId.Trim(),
            Name = mechanic.Name.Trim(),
            Description = mechanic.Description.Trim(),
            Tags = mechanic.Tags.Where(NotBlank).Select(Trim).ToList()
        };
    }

    private static GeneratedPackageRuntimePreviewProvenance BuildProvenance(GeneratedContentArtifactProvenance provenance)
    {
        return new GeneratedPackageRuntimePreviewProvenance
        {
            ArtifactId = provenance.ArtifactId.Trim(),
            ContractId = provenance.ContractId.Trim(),
            ArtifactKind = provenance.ArtifactKind.Trim(),
            CapabilitySelectionId = provenance.CapabilitySelectionId.Trim(),
            MappingResult = provenance.MappingResult.Trim(),
            ContentHash = provenance.ContentHash.Trim()
        };
    }

    private static bool IsGeneratedContentEmpty(GeneratedContentDefinition generatedContent)
    {
        return string.IsNullOrWhiteSpace(generatedContent.Profile.Title)
               && string.IsNullOrWhiteSpace(generatedContent.Profile.Description)
               && generatedContent.Scenes.Count == 0
               && generatedContent.Regions.Count == 0
               && generatedContent.Npcs.Count == 0
               && generatedContent.Items.Count == 0
               && generatedContent.Dialogues.Count == 0
               && generatedContent.Encounters.Count == 0
               && generatedContent.Quests.Count == 0
               && generatedContent.Mechanics.Count == 0
               && generatedContent.AppliedArtifacts.Count == 0;
    }

    private static bool NotBlank(string value) => !string.IsNullOrWhiteSpace(value);

    private static string Trim(string value) => value.Trim();

    private static string TrimOrEmpty(string? value) => value?.Trim() ?? string.Empty;
}

public sealed record GeneratedPackageRuntimePreviewModel
{
    public string PackageTitle { get; init; } = string.Empty;
    public string PackageDescription { get; init; } = string.Empty;
    public string CurrentMapId { get; init; } = string.Empty;
    public string CurrentMapName { get; init; } = string.Empty;
    public GeneratedPackageRuntimePreviewScene? CurrentScene { get; init; }
    public GeneratedPackageRuntimePreviewProfile Profile { get; init; } = new();
    public IReadOnlyList<GeneratedPackageRuntimePreviewContentItem> Regions { get; init; } = Array.Empty<GeneratedPackageRuntimePreviewContentItem>();
    public IReadOnlyList<GeneratedPackageRuntimePreviewContentItem> Npcs { get; init; } = Array.Empty<GeneratedPackageRuntimePreviewContentItem>();
    public IReadOnlyList<GeneratedPackageRuntimePreviewContentItem> Items { get; init; } = Array.Empty<GeneratedPackageRuntimePreviewContentItem>();
    public IReadOnlyList<GeneratedPackageRuntimePreviewContentItem> Dialogues { get; init; } = Array.Empty<GeneratedPackageRuntimePreviewContentItem>();
    public IReadOnlyList<GeneratedPackageRuntimePreviewContentItem> Encounters { get; init; } = Array.Empty<GeneratedPackageRuntimePreviewContentItem>();
    public IReadOnlyList<GeneratedPackageRuntimePreviewQuest> Quests { get; init; } = Array.Empty<GeneratedPackageRuntimePreviewQuest>();
    public IReadOnlyList<GeneratedPackageRuntimePreviewMechanic> Mechanics { get; init; } = Array.Empty<GeneratedPackageRuntimePreviewMechanic>();
    public IReadOnlyList<GeneratedPackageRuntimePreviewProvenance> Provenance { get; init; } = Array.Empty<GeneratedPackageRuntimePreviewProvenance>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}

public sealed record GeneratedPackageRuntimePreviewContentItem
{
    public string SourceId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IReadOnlyList<string> References { get; init; } = Array.Empty<string>();
}

public sealed record GeneratedPackageRuntimePreviewProfile
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Genre { get; init; } = string.Empty;
    public string Tone { get; init; } = string.Empty;
    public IReadOnlyList<string> CoreLoop { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Pillars { get; init; } = Array.Empty<string>();
}

public sealed record GeneratedPackageRuntimePreviewScene
{
    public string SourceId { get; init; } = string.Empty;
    public string PackageMapId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
}

public sealed record GeneratedPackageRuntimePreviewQuest
{
    public string SourceId { get; init; } = string.Empty;
    public string PackageQuestId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IReadOnlyList<string> Steps { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Objectives { get; init; } = Array.Empty<string>();
}

public sealed record GeneratedPackageRuntimePreviewMechanic
{
    public string SourceId { get; init; } = string.Empty;
    public string PackageAbilityId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

public sealed record GeneratedPackageRuntimePreviewProvenance
{
    public string ArtifactId { get; init; } = string.Empty;
    public string ContractId { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public string CapabilitySelectionId { get; init; } = string.Empty;
    public string MappingResult { get; init; } = string.Empty;
    public string ContentHash { get; init; } = string.Empty;
}
