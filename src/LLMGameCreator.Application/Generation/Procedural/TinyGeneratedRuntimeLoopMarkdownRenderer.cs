namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class TinyGeneratedRuntimeLoopMarkdownRenderer
{
    public string Render(TinyGeneratedRuntimeLoopReport report, TinyGeneratedRuntimeState state)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(state);

        var lines = new List<string>
        {
            "# Tiny Generated Runtime Loop v1",
            string.Empty,
            "This deterministic loop does not call an LLM, provider, Lua, Unity, media generator, or broad runtime contract.",
            string.Empty,
            "## Metadata",
            string.Empty,
            $"- Plan id: `{report.Source.PlanId}`",
            $"- Plan hash: `{report.Source.PlanHash}`",
            $"- Rule pack id: `{report.Source.RulePackId}`",
            $"- Rule pack hash: `{report.Source.RulePackHash}`",
            $"- Seed: `{report.Source.Seed}`",
            $"- Mode: `{report.Source.Mode}`",
            $"- State hash: `{report.StateHash}`",
            $"- Stable summary: `{report.StableSummary}`",
            string.Empty,
            "## State",
            string.Empty,
            $"- Starting region: `{state.StartingRegionId}`",
            $"- Visited regions: `{FormatIds(state.VisitedRegionIds)}`",
            $"- Resolved encounter: `{FormatValue(state.ResolvedEncounterId)}`",
            $"- Advanced quest/event: `{FormatValue(state.AdvancedQuestEventId)}`",
            $"- Applied actions: `{FormatIds(state.AppliedActionIds)}`",
            $"- Applied effects: `{FormatIds(state.AppliedEffectIds)}`",
            string.Empty,
            "## Inventory",
            string.Empty
        };

        lines.AddRange(state.InventoryItemCounts.Count == 0
            ? ["- None"]
            : state.InventoryItemCounts.Select(item => $"- `{item.Key}` x`{item.Value}`"));

        lines.AddRange([string.Empty, "## Flags", string.Empty]);
        lines.AddRange(state.Flags.Count == 0
            ? ["- None"]
            : state.Flags.Select(item => $"- `{item.Key}` = `{item.Value.ToString().ToLowerInvariant()}`"));

        lines.AddRange([string.Empty, "## Faction reputation deltas", string.Empty]);
        lines.AddRange(state.FactionReputationDeltas.Count == 0
            ? ["- None"]
            : state.FactionReputationDeltas.Select(item => $"- `{item.Key}` delta=`{item.Value}`"));

        lines.AddRange([string.Empty, "## Quest/event states", string.Empty]);
        lines.AddRange(state.QuestEventStates.Count == 0
            ? ["- None"]
            : state.QuestEventStates.Select(item => $"- `{item.Key}` state=`{item.Value}`"));

        lines.AddRange([string.Empty, "## Steps", string.Empty]);
        lines.AddRange(report.Steps.Count == 0
            ? ["- None"]
            : report.Steps.Select(step => $"- `{step.StepId}` type=`{step.StepType}` target=`{step.TargetId}`: {step.Summary}"));

        lines.AddRange([string.Empty, "## Diagnostics", string.Empty]);
        lines.AddRange(report.Diagnostics.Count == 0
            ? ["- None"]
            : report.Diagnostics.Select(diagnostic =>
                $"- `{diagnostic.Severity}` `{diagnostic.Code}` target=`{diagnostic.Target}`: {diagnostic.Message}"));

        return string.Join("\n", lines) + "\n";
    }

    private static string FormatIds(IReadOnlyList<string> ids) =>
        ids.Count == 0 ? "none" : string.Join(",", ids);

    private static string FormatValue(string value) =>
        string.IsNullOrWhiteSpace(value) ? "none" : value;
}
