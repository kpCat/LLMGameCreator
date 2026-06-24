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
                $"- `{command.CommandId}` `{command.CommandType}` success=`{command.Succeeded.ToString().ToLowerInvariant()}` map=`{command.CurrentMapId}` pos=`{command.PlayerPosition.X},{command.PlayerPosition.Y}` events=`{(command.EventTypes.Count == 0 ? "none" : string.Join(",", command.EventTypes))}`"));

        lines.AddRange(
        [
            string.Empty,
            "## Projection Counts",
            string.Empty,
            $"- Regions: `{snapshot.Counts.Regions}`",
            $"- NPCs: `{snapshot.Counts.Npcs}`",
            $"- Items: `{snapshot.Counts.Items}`",
            $"- Encounters: `{snapshot.Counts.Encounters}`",
            $"- Quests: `{snapshot.Counts.Quests}`",
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
            $"- Start map id: `{snapshot.StartMapId}`",
            $"- Current map id: `{snapshot.CurrentMapId}`",
            $"- Representative region ids: `{JoinIds(snapshot.RepresentativeGeneratedIds.RegionIds)}`",
            $"- Representative quest ids: `{JoinIds(snapshot.RepresentativeGeneratedIds.QuestIds)}`",
            string.Empty,
            "No LLM, provider, Lua, Unity, or media execution is required for this check."
        };

        return string.Join("\n", lines) + "\n";
    }

    private static string JoinIds(IReadOnlyList<string> ids) => ids.Count == 0 ? "none" : string.Join(",", ids);
}
