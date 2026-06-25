namespace LLMGameCreator.Application.RuntimePreview;

public sealed class VisibleGeneratedPlayablePreviewMarkdownRenderer
{
    public string RenderReport(
        VisibleGeneratedPlayablePreviewSnapshot snapshot,
        VisibleGeneratedPlayablePreviewReport report)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(report);

        var lines = new List<string>
        {
            "# Visible Generated Playable Preview v1",
            string.Empty,
            "This report is deterministic and does not call an LLM, provider, Lua, Unity, or media generator.",
            string.Empty,
            "## Package",
            string.Empty,
            $"- Package id: `{snapshot.PackageId}`",
            $"- Package title: `{snapshot.PackageTitle}`",
            $"- Start map id: `{snapshot.StartMapId}`",
            $"- Current map id: `{snapshot.CurrentMapId}`",
            $"- Snapshot hash: `{report.SnapshotHash}`",
            $"- Stable summary: `{report.StableSummary}`",
            string.Empty,
            "## Generation Options",
            string.Empty,
            $"- Seed: `{snapshot.GenerationOptions.Seed}`",
            $"- Mode: `{snapshot.GenerationOptions.Mode}`",
            $"- Preset: `{snapshot.GenerationOptions.PresetTitle}` / `{snapshot.GenerationOptions.PresetId}`",
            $"- Style hints: `{JoinIds(snapshot.GenerationOptions.CompactStyleHintIds)}`",
            $"- Variant ids: `{JoinIds(snapshot.GenerationOptions.SelectedVariantIds)}`",
            string.Empty,
            "## Source Hashes",
            string.Empty,
            $"- Plan hash: `{report.SourceHashes.PlanHash}`",
            $"- Rule pack hash: `{report.SourceHashes.RulePackHash}`",
            $"- Tiny loop state hash: `{report.SourceHashes.TinyLoopStateHash}`",
            $"- Generated package final hash: `{report.SourceHashes.GeneratedPackageFinalHash}`",
            string.Empty,
            "## Runtime Attempt",
            string.Empty,
            $"- Runtime start attempted: `{snapshot.RuntimeAttempt.RuntimeStartAttempted.ToString().ToLowerInvariant()}`",
            $"- Runtime start succeeded: `{snapshot.RuntimeAttempt.RuntimeStartSucceeded.ToString().ToLowerInvariant()}`",
            $"- Player start position: `{snapshot.RuntimeAttempt.PlayerStartPosition.X},{snapshot.RuntimeAttempt.PlayerStartPosition.Y}`",
            $"- Player current position: `{snapshot.RuntimeAttempt.PlayerCurrentPosition.X},{snapshot.RuntimeAttempt.PlayerCurrentPosition.Y}`",
            $"- Event types: `{(snapshot.RuntimeAttempt.EventTypes.Count == 0 ? "none" : string.Join(",", snapshot.RuntimeAttempt.EventTypes))}`",
            string.Empty,
            "## Command Attempts",
            string.Empty
        };

        lines.AddRange(snapshot.RuntimeAttempt.CommandAttempts.Count == 0
            ? ["- None"]
            : snapshot.RuntimeAttempt.CommandAttempts.Select(command =>
                $"- `{command.CommandId}` `{command.CommandType}` success=`{command.Succeeded.ToString().ToLowerInvariant()}` map=`{command.CurrentMapId}` pos=`{command.PlayerPosition.X},{command.PlayerPosition.Y}` events=`{(command.EventTypes.Count == 0 ? "none" : string.Join(",", command.EventTypes))}` targets=`{JoinIds(command.EventTargets)}`"));

        lines.AddRange(
        [
            string.Empty,
            "## Active Goal",
            string.Empty,
            $"- Active goal selected: `{snapshot.MicrogameGoal.ActiveGoalSelected.ToString().ToLowerInvariant()}`",
            $"- Quest: `{FirstNonEmpty(snapshot.MicrogameGoal.ActiveQuestTitle, snapshot.MicrogameGoal.ActiveQuestId, "none")}`",
            $"- Current objective: `{FirstNonEmpty(snapshot.MicrogameGoal.CurrentObjectiveText, "none")}`",
            $"- Progress: `{snapshot.MicrogameGoal.CompletedStepCount}/{snapshot.MicrogameGoal.StepCount}` `{snapshot.MicrogameGoal.ProgressStatus}`",
            $"- Advanced by interaction: `{snapshot.MicrogameGoal.ProgressAdvancedByInteraction.ToString().ToLowerInvariant()}`",
            $"- Related NPC: `{FirstNonEmpty(snapshot.MicrogameGoal.Related.NpcTitle, snapshot.MicrogameGoal.Related.NpcId, "none")}`",
            $"- Related item: `{FirstNonEmpty(snapshot.MicrogameGoal.Related.ItemTitle, snapshot.MicrogameGoal.Related.ItemId, "none")}`",
            $"- Related encounter: `{FirstNonEmpty(snapshot.MicrogameGoal.Related.EncounterTitle, snapshot.MicrogameGoal.Related.EncounterId, "none")}`",
            string.Empty,
            "## Challenge",
            string.Empty,
            $"- Challenge selected: `{snapshot.MicrogameChallenge.ChallengeSelected.ToString().ToLowerInvariant()}`",
            $"- Encounter: `{FirstNonEmpty(snapshot.MicrogameChallenge.EncounterTitle, snapshot.MicrogameChallenge.EncounterId, "none")}`",
            $"- Resolved: `{snapshot.MicrogameChallenge.Resolved.ToString().ToLowerInvariant()}`",
            $"- Reward visible: `{snapshot.MicrogameChallenge.RewardVisible.ToString().ToLowerInvariant()}`",
            $"- Reward: `{FirstNonEmpty(snapshot.MicrogameChallenge.RewardTitle, snapshot.MicrogameChallenge.RewardItemId, "none")}`",
            $"- Completion visible: `{snapshot.MicrogameChallenge.CompletionVisible.ToString().ToLowerInvariant()}`",
            $"- Completion status: `{FirstNonEmpty(snapshot.MicrogameChallenge.CompletionStatus, "none")}`",
            $"- Resolve action: `{FirstNonEmpty(snapshot.MicrogameChallenge.ResolveAction, "none")}`",
            string.Empty,
            "## Projection Counts",
            string.Empty,
            $"- Regions: `{snapshot.Counts.Regions}`",
            $"- NPCs: `{snapshot.Counts.Npcs}`",
            $"- Items: `{snapshot.Counts.Items}`",
            $"- Encounters: `{snapshot.Counts.Encounters}`",
            $"- Quests: `{snapshot.Counts.Quests}`",
            $"- Active goals: `{snapshot.Counts.ActiveGoals}`",
            $"- Active goal progress: `{snapshot.Counts.ActiveGoalCompletedSteps}/{snapshot.Counts.ActiveGoalTotalSteps}`",
            $"- Resolved challenges: `{snapshot.Counts.ResolvedChallenges}`",
            $"- Visible rewards: `{snapshot.Counts.VisibleRewards}`",
            $"- Visible completions: `{snapshot.Counts.VisibleCompletions}`",
            $"- Mechanics: `{snapshot.Counts.Mechanics}`",
            $"- Provenance records: `{snapshot.Counts.ProvenanceRecords}`",
            string.Empty,
            "## Representative Generated IDs",
            string.Empty,
            $"- Regions: `{JoinIds(snapshot.RepresentativeGeneratedIds.RegionIds)}`",
            $"- NPCs: `{JoinIds(snapshot.RepresentativeGeneratedIds.NpcIds)}`",
            $"- Items: `{JoinIds(snapshot.RepresentativeGeneratedIds.ItemIds)}`",
            $"- Encounters: `{JoinIds(snapshot.RepresentativeGeneratedIds.EncounterIds)}`",
            $"- Quests: `{JoinIds(snapshot.RepresentativeGeneratedIds.QuestIds)}`",
            $"- Mechanics: `{JoinIds(snapshot.RepresentativeGeneratedIds.MechanicIds)}`",
            $"- Provenance: `{JoinIds(snapshot.RepresentativeGeneratedIds.ProvenanceArtifactIds)}`",
            string.Empty,
            "## Diagnostics",
            string.Empty
        ]);
        lines.AddRange(report.Diagnostics.Count == 0
            ? ["- None"]
            : report.Diagnostics.Select(diagnostic =>
                $"- `{diagnostic.Severity}` `{diagnostic.Code}` target=`{diagnostic.Target}`: {diagnostic.Message}"));

        return string.Join("\n", lines) + "\n";
    }

    public string RenderManualVerification(VisibleGeneratedPlayablePreviewSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var lines = new List<string>
        {
            "# Manual Visible Generated Playable Preview Check",
            string.Empty,
            "Generated artifacts are written under:",
            string.Empty,
            "```text",
            ".llmgc/procedural/visible-generated-playable-preview/",
            "```",
            string.Empty,
            "Run the deterministic smoke command:",
            string.Empty,
            "```powershell",
            ".\\.devflow\\scripts\\run-product-smoke.ps1 -Scenario visible-generated-playable-preview",
            "```",
            string.Empty,
            "Manual app check:",
            string.Empty,
            "1. Open `LLMGameCreator.sln` in Visual Studio.",
            "2. Start `LLMGameCreator.WinForms`.",
            "3. Open the generated package MVP project/package path created by the smoke output.",
            "4. Open the existing Runtime Preview or Runtime Simulator page.",
            "5. Confirm the generated package title, map/scene, region/quest/mechanic/provenance data are visible.",
            "6. Try movement or interaction if the preview/simulator exposes the map commands for the loaded package.",
            string.Empty,
            "Expected current generated package:",
            string.Empty,
            $"- Package title: `{snapshot.PackageTitle}`",
            $"- Seed/preset: `{snapshot.GenerationOptions.Seed}` / `{snapshot.GenerationOptions.PresetId}`",
            $"- Start map id: `{snapshot.StartMapId}`",
            $"- Current map id: `{snapshot.CurrentMapId}`",
            $"- Active goal: `{FirstNonEmpty(snapshot.MicrogameGoal.ActiveQuestTitle, snapshot.MicrogameGoal.ActiveQuestId, "none")}`",
            $"- Current objective: `{FirstNonEmpty(snapshot.MicrogameGoal.CurrentObjectiveText, "none")}`",
            $"- Goal progress: `{snapshot.MicrogameGoal.CompletedStepCount}/{snapshot.MicrogameGoal.StepCount}`",
            $"- Challenge: `{FirstNonEmpty(snapshot.MicrogameChallenge.EncounterTitle, snapshot.MicrogameChallenge.EncounterId, "none")}`",
            $"- Reward/completion: `{snapshot.MicrogameChallenge.RewardVisible.ToString().ToLowerInvariant()}/{snapshot.MicrogameChallenge.CompletionVisible.ToString().ToLowerInvariant()}`",
            $"- Representative region ids: `{JoinIds(snapshot.RepresentativeGeneratedIds.RegionIds)}`",
            $"- Representative quest ids: `{JoinIds(snapshot.RepresentativeGeneratedIds.QuestIds)}`",
            string.Empty,
            "No LLM, provider, Lua, Unity, or media execution is required for this check."
        };

        return string.Join("\n", lines) + "\n";
    }

    private static string JoinIds(IReadOnlyList<string> ids) => ids.Count == 0 ? "none" : string.Join(",", ids);

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
}
