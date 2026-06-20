using System.Text;

namespace LLMGameCreator.Application.RuntimePreview;

public sealed class GeneratedContentInteractionPreviewService
{
    public GeneratedContentInteractionCatalog Build(GeneratedPackageRuntimePreviewModel preview)
    {
        ArgumentNullException.ThrowIfNull(preview);

        return new GeneratedContentInteractionCatalog
        {
            Categories = new[]
            {
                BuildCurrentScene(preview),
                BuildContentCategory("regions", "Regions", preview.Regions),
                BuildContentCategory("npcs", "NPCs", preview.Npcs),
                BuildContentCategory("items", "Items", preview.Items),
                BuildContentCategory("dialogues", "Dialogues", preview.Dialogues, "Lines"),
                BuildQuestCategory(preview.Quests),
                BuildMechanicCategory(preview.Mechanics),
                BuildContentCategory("encounters", "Encounters", preview.Encounters),
                BuildProvenanceCategory(preview.Provenance),
                BuildWarningCategory(preview.Warnings)
            }
        };
    }

    private static GeneratedContentInteractionCategory BuildCurrentScene(GeneratedPackageRuntimePreviewModel preview)
    {
        var entries = new List<GeneratedContentInteractionEntry>();
        if (preview.CurrentScene != null)
        {
            var scene = preview.CurrentScene;
            entries.Add(new GeneratedContentInteractionEntry
            {
                CategoryId = "current_scene",
                EntryId = FirstNonEmpty(scene.SourceId, scene.PackageMapId, "current_scene"),
                Title = FirstNonEmpty(scene.Title, scene.SourceId, scene.PackageMapId, "Current scene"),
                Subtitle = scene.Description,
                ReferenceIds = NonEmpty(scene.SourceId, scene.PackageMapId),
                DetailsText = BuildDetails(
                    ("Source id", scene.SourceId),
                    ("Package map id", scene.PackageMapId),
                    ("Title", scene.Title),
                    ("Description", scene.Description),
                    ("Purpose", scene.Purpose))
            });
        }

        return Category("current_scene", "Current scene", entries);
    }

    private static GeneratedContentInteractionCategory BuildContentCategory(
        string categoryId,
        string title,
        IReadOnlyList<GeneratedPackageRuntimePreviewContentItem> items,
        string detailLinesLabel = "Details")
    {
        var entries = items.Select(item => new GeneratedContentInteractionEntry
        {
            CategoryId = categoryId,
            EntryId = item.SourceId,
            Title = FirstNonEmpty(item.Title, item.SourceId, "(untitled)"),
            Subtitle = item.Description,
            ReferenceIds = item.References,
            DetailsText = BuildDetails(
                ("Id", item.SourceId),
                ("Title", item.Title),
                ("Description", item.Description),
                ("References", Join(item.References)),
                (detailLinesLabel, JoinLines(item.DetailLines)))
        }).ToList();

        return Category(categoryId, title, entries);
    }

    private static GeneratedContentInteractionCategory BuildQuestCategory(
        IReadOnlyList<GeneratedPackageRuntimePreviewQuest> quests)
    {
        var entries = quests.Select(quest => new GeneratedContentInteractionEntry
        {
            CategoryId = "quests",
            EntryId = FirstNonEmpty(quest.SourceId, quest.PackageQuestId),
            Title = FirstNonEmpty(quest.Title, quest.PackageQuestId, quest.SourceId, "(untitled)"),
            Subtitle = quest.Description,
            ReferenceIds = NonEmpty(quest.PackageQuestId),
            DetailsText = BuildDetails(
                ("Source id", quest.SourceId),
                ("Package quest id", quest.PackageQuestId),
                ("Title", quest.Title),
                ("Description", quest.Description),
                ("Steps", JoinLines(quest.Steps)),
                ("Objectives", JoinLines(quest.Objectives)))
        }).ToList();

        return Category("quests", "Quests", entries);
    }

    private static GeneratedContentInteractionCategory BuildMechanicCategory(
        IReadOnlyList<GeneratedPackageRuntimePreviewMechanic> mechanics)
    {
        var entries = mechanics.Select(mechanic => new GeneratedContentInteractionEntry
        {
            CategoryId = "mechanics",
            EntryId = FirstNonEmpty(mechanic.SourceId, mechanic.PackageAbilityId),
            Title = FirstNonEmpty(mechanic.Name, mechanic.PackageAbilityId, mechanic.SourceId, "(untitled)"),
            Subtitle = mechanic.Description,
            ReferenceIds = NonEmpty(mechanic.PackageAbilityId),
            DetailsText = BuildDetails(
                ("Source id", mechanic.SourceId),
                ("Package ability id", mechanic.PackageAbilityId),
                ("Name", mechanic.Name),
                ("Description", mechanic.Description),
                ("Tags", Join(mechanic.Tags)))
        }).ToList();

        return Category("mechanics", "Mechanics", entries);
    }

    private static GeneratedContentInteractionCategory BuildProvenanceCategory(
        IReadOnlyList<GeneratedPackageRuntimePreviewProvenance> provenanceItems)
    {
        var entries = provenanceItems.Select(provenance => new GeneratedContentInteractionEntry
        {
            CategoryId = "applied_artifacts",
            EntryId = provenance.ArtifactId,
            Title = FirstNonEmpty(provenance.ContractId, provenance.ArtifactKind, provenance.ArtifactId),
            Subtitle = provenance.MappingResult,
            ReferenceIds = NonEmpty(provenance.CapabilitySelectionId),
            DetailsText = BuildDetails(
                ("Contract", provenance.ContractId),
                ("Artifact id", provenance.ArtifactId),
                ("Artifact kind", provenance.ArtifactKind),
                ("Capability selection", provenance.CapabilitySelectionId),
                ("Mapping", provenance.MappingResult),
                ("Content hash", provenance.ContentHash))
        }).ToList();

        return Category("applied_artifacts", "Applied artifacts", entries);
    }

    private static GeneratedContentInteractionCategory BuildWarningCategory(IReadOnlyList<string> warnings)
    {
        var entries = warnings
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select((warning, index) => new GeneratedContentInteractionEntry
            {
                CategoryId = "warnings",
                EntryId = $"warning/{index + 1}",
                Title = $"Warning {index + 1}",
                Subtitle = warning.Trim(),
                DetailsText = warning.Trim()
            })
            .ToList();

        return Category("warnings", "Warnings", entries);
    }

    private static GeneratedContentInteractionCategory Category(
        string id,
        string title,
        IReadOnlyList<GeneratedContentInteractionEntry> entries)
    {
        return new GeneratedContentInteractionCategory
        {
            Id = id,
            Title = title,
            Entries = entries
        };
    }

    private static string BuildDetails(params (string Label, string Value)[] values)
    {
        var builder = new StringBuilder();
        foreach (var (label, value) in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                builder.Append(label).Append(": ").AppendLine(value.Trim());
            }
        }

        return builder.ToString().TrimEnd();
    }

    private static IReadOnlyList<string> NonEmpty(params string[] values) =>
        values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToList();

    private static string Join(IEnumerable<string> values) =>
        string.Join(", ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()));

    private static string JoinLines(IEnumerable<string> values) =>
        string.Join(Environment.NewLine, values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => "- " + value.Trim()));

    private static string FirstNonEmpty(params string[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
}

public sealed record GeneratedContentInteractionCatalog
{
    public IReadOnlyList<GeneratedContentInteractionCategory> Categories { get; init; } = Array.Empty<GeneratedContentInteractionCategory>();
}

public sealed record GeneratedContentInteractionCategory
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedContentInteractionEntry> Entries { get; init; } = Array.Empty<GeneratedContentInteractionEntry>();
}

public sealed record GeneratedContentInteractionEntry
{
    public string CategoryId { get; init; } = string.Empty;
    public string EntryId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public IReadOnlyList<string> ReferenceIds { get; init; } = Array.Empty<string>();
    public string DetailsText { get; init; } = string.Empty;
}
