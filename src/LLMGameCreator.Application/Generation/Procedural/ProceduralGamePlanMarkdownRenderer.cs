namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class ProceduralGamePlanMarkdownRenderer
{
    public string Render(ProceduralGeneratedGamePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var lines = new List<string>
        {
            "# Seeded Procedural Game Plan v1",
            string.Empty,
            "This deterministic plan does not call an LLM, provider, Lua, Unity, media generator, or runtime execution.",
            string.Empty,
            "## Metadata",
            string.Empty,
            $"- Seed: `{plan.Metadata.Seed}`",
            $"- Mode: `{plan.Metadata.Mode}`",
            $"- Kernel version: `{plan.Metadata.KernelVersion}`",
            $"- Deterministic hash: `{plan.Metadata.DeterministicHash}`",
            $"- Stable summary: `{plan.Metadata.StableSummary}`",
            string.Empty,
            "## Counts",
            string.Empty,
            $"- Regions: `{plan.World.Regions.Count}`",
            $"- Region connections: `{plan.World.Connections.Count}`",
            $"- Factions: `{plan.Factions.Count}`",
            $"- Actor seeds: `{plan.ActorSeeds.Count}`",
            $"- Item/resource seeds: `{plan.ItemResourceSeeds.Count}`",
            $"- Encounter seeds: `{plan.EncounterSeeds.Count}`",
            $"- Quest/event seeds: `{plan.QuestEventSeeds.Count}`",
            $"- Formula/effect/action placeholders: `{plan.FormulaEffectActionPlaceholders.Count}`",
            string.Empty,
            "## Profile",
            string.Empty,
            "### Variant ids",
            string.Empty
        };

        lines.AddRange(plan.Profile.VariantIds.Select(id => $"- `{id}`"));
        lines.AddRange([string.Empty, "### Style hint ids", string.Empty]);
        lines.AddRange(plan.Profile.StyleHintIds.Count == 0
            ? ["- None"]
            : plan.Profile.StyleHintIds.Select(id => $"- `{id}`"));

        lines.AddRange([string.Empty, "## Regions", string.Empty]);
        lines.AddRange(plan.World.Regions.Select(region =>
            $"- `{region.RegionId}` {region.Label} mood=`{region.MoodHintId}` tags=`{string.Join(",", region.Tags)}`"));

        lines.AddRange([string.Empty, "## Factions", string.Empty]);
        lines.AddRange(plan.Factions.Select(faction =>
            $"- `{faction.FactionId}` {faction.Label} home=`{faction.HomeRegionId}` motive=`{faction.MotiveHintId}`"));

        lines.AddRange([string.Empty, "## Runtime-facing seeds", string.Empty]);
        lines.AddRange(plan.EncounterSeeds.Select(encounter =>
            $"- Encounter `{encounter.EncounterSeedId}` region=`{encounter.RegionId}` actors=`{string.Join(",", encounter.ActorSeedIds)}` action=`{encounter.ActionPlaceholderId}`"));
        lines.AddRange(plan.QuestEventSeeds.Select(quest =>
            $"- Quest/event `{quest.QuestEventSeedId}` region=`{quest.RegionId}` encounter=`{quest.TargetEncounterSeedId}` reward=`{quest.RewardPlaceholderId}`"));

        lines.AddRange([string.Empty, "## Slice 030 placeholders", string.Empty]);
        lines.AddRange(plan.FormulaEffectActionPlaceholders.Select(placeholder =>
            $"- `{placeholder.PlaceholderId}` kind=`{placeholder.Kind}` next=`{placeholder.RequiredNextSlice}`: {placeholder.Summary}"));

        lines.AddRange([string.Empty, "## Diagnostics", string.Empty]);
        lines.AddRange(plan.Diagnostics.Count == 0
            ? ["- None"]
            : plan.Diagnostics.Select(diagnostic =>
                $"- `{diagnostic.Severity}` `{diagnostic.Code}` target=`{diagnostic.Target}`: {diagnostic.Message}"));

        return string.Join("\n", lines) + "\n";
    }
}
